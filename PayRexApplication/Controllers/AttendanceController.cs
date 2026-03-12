using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.DTOs;
using PayRexApplication.Services;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly HolidayService _holidayService;
        private readonly ILogger<AttendanceController> _logger;

        public AttendanceController(AppDbContext db, HolidayService holidayService, ILogger<AttendanceController> logger)
        {
            _db = db;
            _holidayService = holidayService;
            _logger = logger;
        }

        /// <summary>
        /// Get attendance records for the current company, filtered by date range
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAttendanceRecords(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] string? status)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            // Use payroll-period-based defaults instead of a fixed 7-day window
            DateTime fromDate, toDate;
            if (from.HasValue && to.HasValue)
            {
                fromDate = from.Value;
                toDate = to.Value;
            }
            else
            {
                var settings = await _db.CompanySettings.FindAsync(companyId.Value);
                var cycle = settings?.PayrollCycle ?? Enums.PayrollCycle.Monthly;
                var (periodFrom, periodTo) = GetCurrentPayrollPeriod(cycle, DateTime.Today);
                fromDate = from ?? periodFrom;
                toDate = to ?? periodTo;
            }

            var query = _db.AttendanceRecords
                .Include(a => a.Employee)
                .Where(a => a.CompanyId == companyId.Value
                    && a.Date >= DateOnly.FromDateTime(fromDate)
                    && a.Date <= DateOnly.FromDateTime(toDate))
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a =>
                    (a.Employee.FirstName + " " + a.Employee.LastName).ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var records = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.LastName)
                .Select(a => new AttendanceRecordDto
                {
                    Id = a.AttendanceId,
                    EmployeeId = string.IsNullOrWhiteSpace(a.Employee.EmployeeCode) ? a.EmployeeId.ToString() : a.Employee.EmployeeCode,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    Date = a.Date.ToDateTime(TimeOnly.MinValue),
                    TimeIn = a.TimeIn.HasValue ? a.TimeIn.Value.ToString("hh:mm tt") : null,
                    TimeOut = a.TimeOut.HasValue ? a.TimeOut.Value.ToString("hh:mm tt") : null,
                    TotalHoursWorked = a.TotalHoursWorked.HasValue ? (double)a.TotalHoursWorked.Value : 0,
                    OvertimeHours = a.OvertimeHours.HasValue ? (double)a.OvertimeHours.Value : 0,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    IsHoliday = a.IsHoliday,
                    HolidayName = a.HolidayName
                })
                .ToListAsync();

            return Ok(records);
        }

        /// <summary>
        /// Get attendance records for the authenticated employee (self-service)
        /// </summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAttendance(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            var uidStr = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(uidStr) || !int.TryParse(uidStr, out var uid))
                return Unauthorized(new { message = "User not identified" });

            var employee = await _db.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == uid && e.CompanyId == companyId.Value);

            if (employee == null)
                return NotFound(new { message = "Employee record not found" });

            var fromDate = from ?? DateTime.Today.AddDays(-30);
            var toDate = to ?? DateTime.Today;

            var records = await _db.AttendanceRecords
                .Where(a => a.EmployeeId == employee.EmployeeNumber
                    && a.Date >= DateOnly.FromDateTime(fromDate)
                    && a.Date <= DateOnly.FromDateTime(toDate))
                .OrderByDescending(a => a.Date)
                .AsNoTracking()
                .Select(a => new AttendanceRecordDto
                {
                    Id = a.AttendanceId,
                    EmployeeId = string.IsNullOrWhiteSpace(employee.EmployeeCode) ? a.EmployeeId.ToString() : employee.EmployeeCode,
                    EmployeeName = employee.FirstName + " " + employee.LastName,
                    Date = a.Date.ToDateTime(TimeOnly.MinValue),
                    TimeIn = a.TimeIn.HasValue ? a.TimeIn.Value.ToString("hh:mm tt") : null,
                    TimeOut = a.TimeOut.HasValue ? a.TimeOut.Value.ToString("hh:mm tt") : null,
                    TotalHoursWorked = a.TotalHoursWorked.HasValue ? (double)a.TotalHoursWorked.Value : 0,
                    OvertimeHours = a.OvertimeHours.HasValue ? (double)a.OvertimeHours.Value : 0,
                    Status = a.Status,
                    Remarks = a.Remarks,
                    IsHoliday = a.IsHoliday,
                    HolidayName = a.HolidayName
                })
                .ToListAsync();

            // Compute summary stats
            var thisMonth = records.Where(r => r.Date.Month == DateTime.Today.Month && r.Date.Year == DateTime.Today.Year).ToList();
            var totalPresent = thisMonth.Count(r => r.Status == "Present" || r.Status == "Late");
            var totalLate = thisMonth.Count(r => r.Status == "Late");
            var totalHours = thisMonth.Sum(r => r.TotalHoursWorked);
            var totalOvertime = thisMonth.Sum(r => r.OvertimeHours);

            return Ok(new
            {
                records,
                summary = new
                {
                    totalPresent,
                    totalLate,
                    totalAbsent = thisMonth.Count(r => r.Status == "Absent"),
                    totalHours = Math.Round(totalHours, 2),
                    totalOvertime = Math.Round(totalOvertime, 2)
                }
            });
        }

        /// <summary>
        /// Get today's attendance statistics for the current company
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            var today = DateOnly.FromDateTime(DateTime.Today);
            var totalEmployees = await _db.Employees
                .CountAsync(e => e.CompanyId == companyId.Value && e.Status == Enums.EmployeeStatus.Active);

            var todayRecords = await _db.AttendanceRecords
                .Where(a => a.CompanyId == companyId.Value && a.Date == today)
                .ToListAsync();

            var present = todayRecords.Count(r => r.Status == "Present" || r.Status == "Late");
            var late = todayRecords.Count(r => r.Status == "Late");
            var onLeave = todayRecords.Count(r => r.Status == "On Leave");
            var absent = totalEmployees - present - onLeave;
            if (absent < 0) absent = 0;

            // Check if today is a holiday
            var holiday = await _holidayService.GetHolidayForDateAsync(DateTime.Today);

            return Ok(new
            {
                TotalEmployees = totalEmployees,
                Present = present,
                Late = late,
                Absent = absent,
                OnLeave = onLeave,
                IsHoliday = holiday != null,
                HolidayName = holiday?.LocalName
            });
        }

        /// <summary>
        /// Preview employee info by QR value (no logging, just lookup)
        /// </summary>
        [HttpGet("preview")]
        public async Task<IActionResult> PreviewEmployee([FromQuery] string qrValue)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            if (string.IsNullOrWhiteSpace(qrValue))
                return BadRequest(new { message = "QR value is required" });

            // Validate company has configured time in/out
            var settings = await _db.CompanySettings.FindAsync(companyId.Value);
            if (settings?.ScheduledTimeIn == null || settings?.ScheduledTimeOut == null)
                return BadRequest(new { message = "ATTENDANCE_NOT_CONFIGURED", detail = "Please configure Time In and Time Out in Company Settings before taking attendance." });

            // Validate the QR code belongs to this company by checking the code prefix
            var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == companyId.Value);
            if (company != null && !string.IsNullOrEmpty(company.CompanyCode))
            {
                if (!qrValue.StartsWith(company.CompanyCode + "-"))
                    return BadRequest(new { message = "This QR code does not belong to your company" });
            }

            // QR codes encode the EmployeeCode directly (e.g. "COMP-0001")
            var employee = await _db.Employees
                .Include(e => e.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeCode == qrValue && e.CompanyId == companyId.Value);

            if (employee == null)
                return NotFound(new { message = "Employee not found for this QR code" });
            var today = DateOnly.FromDateTime(DateTime.Today);

            var record = await _db.AttendanceRecords
                .Where(a => a.EmployeeId == employee.EmployeeNumber && a.Date == today)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            string pendingAction;
            string? existingTimeIn = null;
            string? existingTimeOut = null;

            if (record == null)
            {
                pendingAction = "Time In";
            }
            else if (!record.TimeOut.HasValue)
            {
                pendingAction = "Time Out";
                existingTimeIn = record.TimeIn?.ToString("hh:mm tt");
            }
            else
            {
                pendingAction = "Already Completed";
                existingTimeIn = record.TimeIn?.ToString("hh:mm tt");
                existingTimeOut = record.TimeOut?.ToString("hh:mm tt");
            }

            return Ok(new
            {
                employeeId = employee.EmployeeCode,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                profilePictureUrl = employee.User?.ProfileImageUrl,
                pendingAction,
                existingTimeIn,
                existingTimeOut,
                currentTime = DateTime.Now.ToString("hh:mm tt"),
                date = DateTime.Today.ToString("MMMM dd, yyyy")
            });
        }

        /// <summary>
        /// Process a QR scan for time-in or time-out
        /// </summary>
        [HttpPost("scan")]
        public async Task<IActionResult> ProcessScan([FromBody] ScanRequest request)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            if (string.IsNullOrWhiteSpace(request.QrValue))
                return BadRequest(new { message = "QR value is required" });

            // Validate the QR code belongs to this company by checking the code prefix
            var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.CompanyId == companyId.Value);
            if (company != null && !string.IsNullOrEmpty(company.CompanyCode))
            {
                if (!request.QrValue.StartsWith(company.CompanyCode + "-"))
                    return BadRequest(new { message = "This QR code does not belong to your company" });
            }

            // QR codes encode the EmployeeCode directly (e.g. "COMP-0001")
            var employee = await _db.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == request.QrValue && e.CompanyId == companyId.Value);

            if (employee == null)
                return NotFound(new { message = "Employee not found for this QR code" });
            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = TimeOnly.FromDateTime(DateTime.Now);

            // Get company settings for grace period and schedule
            var settings = await _db.CompanySettings.FindAsync(companyId.Value);

            // Validate company has configured time in/out
            if (settings?.ScheduledTimeIn == null || settings?.ScheduledTimeOut == null)
                return BadRequest(new { message = "ATTENDANCE_NOT_CONFIGURED", detail = "Please configure Time In and Time Out in Company Settings before taking attendance." });

            var graceMinutes = settings.LateGraceMinutes;
            var scheduledTimeIn = TimeOnly.FromTimeSpan(settings.ScheduledTimeIn.Value);
            var scheduledTimeOut = TimeOnly.FromTimeSpan(settings.ScheduledTimeOut.Value);

            // Check if today is a holiday
            var holiday = await _holidayService.GetHolidayForDateAsync(DateTime.Today);

            // Find or create attendance record for today
            var record = await _db.AttendanceRecords
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.EmployeeNumber && a.Date == today);

            string scanType;

            if (record == null)
            {
                // Time In — reject if before the scheduled time
                if (now < scheduledTimeIn)
                    return BadRequest(new { message = $"Time In is not yet allowed. Scheduled start is {scheduledTimeIn:hh\\:mm tt}." });

                // Time In — reject if after the cutoff window (e.g. 1 hour after scheduled time)
                var cutoffHours = settings.TimeInCutoffHours;
                if (cutoffHours > 0)
                {
                    var cutoffTime = scheduledTimeIn.AddMinutes(cutoffHours * 60);
                    if (now > cutoffTime)
                        return BadRequest(new { message = $"Time In is already locked. The cutoff was {cutoffTime:hh\\:mm tt} ({cutoffHours} hour{(cutoffHours > 1 ? "s" : "")} after scheduled start)." });
                }

                scanType = "Time In";
                var status = "Present";
                int lateMinutes = 0;
                var lateThreshold = scheduledTimeIn.AddMinutes(graceMinutes);

                if (now > lateThreshold)
                {
                    status = "Late";
                    lateMinutes = (int)(now.ToTimeSpan() - scheduledTimeIn.ToTimeSpan()).TotalMinutes;
                }

                record = new AttendanceRecord
                {
                    EmployeeId = employee.EmployeeNumber,
                    CompanyId = companyId.Value,
                    Date = today,
                    TimeIn = now,
                    Status = status,
                    LateMinutes = lateMinutes,
                    IsHoliday = holiday != null,
                    HolidayName = holiday?.LocalName,
                    Source = Enums.AttendanceSource.Qr,
                    Remarks = holiday != null ? $"Holiday: {holiday.LocalName}" : null,
                    CreatedAt = DateTime.UtcNow
                };

                _db.AttendanceRecords.Add(record);
            }
            else if (!record.TimeOut.HasValue)
            {
                // Time Out — reject if before the scheduled time out
                if (now < scheduledTimeOut)
                    return BadRequest(new { message = $"Time Out is not yet allowed. Scheduled end is {scheduledTimeOut:hh\\:mm tt}." });

                scanType = "Time Out";
                record.TimeOut = now;

                if (record.TimeIn.HasValue)
                {
                    var hoursWorked = (now.ToTimeSpan() - record.TimeIn.Value.ToTimeSpan()).TotalHours;
                    record.TotalHoursWorked = (decimal)Math.Round(hoursWorked, 2);

                    // Overtime: 1-hour allowance after scheduled time out before OT starts counting
                    // e.g., if scheduled out is 5:00 PM, OT only counts from 6:00 PM onward
                    var overtimeStart = scheduledTimeOut.AddMinutes(60);
                    if (now > overtimeStart)
                    {
                        var overtimeHours = (now.ToTimeSpan() - overtimeStart.ToTimeSpan()).TotalHours;
                        record.OvertimeHours = (decimal)Math.Round(overtimeHours, 2);
                    }

                    if (now < scheduledTimeOut)
                    {
                        record.UndertimeMinutes = (int)(scheduledTimeOut.ToTimeSpan() - now.ToTimeSpan()).TotalMinutes;
                    }
                }

                record.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                return BadRequest(new { message = "Already timed in and out for today" });
            }

            // Log the scan
            var scan = new AttendanceScan
            {
                EmployeeId = employee.EmployeeNumber,
                ScanTime = DateTime.Now,
                ScanType = record.TimeOut.HasValue ? Enums.ScanType.Out : Enums.ScanType.In,
                DeviceId = request.DeviceId ?? "web",
                Result = Enums.ScanResult.Success,
                Remarks = scanType,
                CreatedAt = DateTime.UtcNow
            };
            _db.AttendanceScans.Add(scan);

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = $"{scanType} recorded for {employee.FirstName} {employee.LastName}",
                scanType,
                employeeName = $"{employee.FirstName} {employee.LastName}",
                time = DateTime.Now.ToString("hh:mm tt"),
                status = record.Status,
                isHoliday = record.IsHoliday,
                holidayName = record.HolidayName
            });
        }

        /// <summary>
        /// Get archived attendance records (older than 7 days)
        /// </summary>
        [HttpGet("archives")]
        public async Task<IActionResult> GetArchives(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            // Archive cutoff = end of previous payroll period (records before current period)
            var settings = await _db.CompanySettings.FindAsync(companyId.Value);
            var cycle = settings?.PayrollCycle ?? Enums.PayrollCycle.Monthly;
            var cutoff = DateOnly.FromDateTime(GetArchiveCutoff(cycle, DateTime.Today));

            var fromDate = from.HasValue ? DateOnly.FromDateTime(from.Value) : DateOnly.FromDateTime(DateTime.Today.AddDays(-90));
            var toDate = to.HasValue ? DateOnly.FromDateTime(to.Value) : cutoff;

            if (toDate > cutoff) toDate = cutoff;

            var query = _db.AttendanceRecords
                .Include(a => a.Employee)
                .Where(a => a.CompanyId == companyId.Value
                    && a.Date >= fromDate
                    && a.Date <= toDate)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a =>
                    (a.Employee.FirstName + " " + a.Employee.LastName).ToLower().Contains(term));
            }

            var totalCount = await query.CountAsync();
            var records = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Employee.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.AttendanceId,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    a.EmployeeId,
                    Date = a.Date.ToString("yyyy-MM-dd"),
                    TimeIn = a.TimeIn.HasValue ? a.TimeIn.Value.ToString("hh:mm tt") : null,
                    TimeOut = a.TimeOut.HasValue ? a.TimeOut.Value.ToString("hh:mm tt") : null,
                    a.TotalHoursWorked,
                    a.LateMinutes,
                    a.OvertimeHours,
                    a.Status,
                    a.Remarks,
                    a.IsHoliday,
                    a.HolidayName
                })
                .ToListAsync();

            return Ok(new { records, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
        }

        /// <summary>
        /// Get company attendance configuration (payroll cycle, schedule, grace minutes)
        /// </summary>
        [HttpGet("company-config")]
        public async Task<IActionResult> GetCompanyConfig()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Forbid();

            var settings = await _db.CompanySettings.FindAsync(companyId.Value);
            return Ok(new
            {
                payrollCycle = (int)(settings?.PayrollCycle ?? Enums.PayrollCycle.Monthly),
                scheduledTimeIn = settings?.ScheduledTimeIn?.ToString(@"hh\:mm"),
                scheduledTimeOut = settings?.ScheduledTimeOut?.ToString(@"hh\:mm"),
                lateGraceMinutes = settings?.LateGraceMinutes ?? 15,
                timeInCutoffHours = settings?.TimeInCutoffHours ?? 0,
                isConfigured = settings?.ScheduledTimeIn != null && settings?.ScheduledTimeOut != null
            });
        }

        /// <summary>
        /// Get holidays for a given year
        /// </summary>
        [HttpGet("holidays/{year}")]
        public async Task<IActionResult> GetHolidays(int year)
        {
            var holidays = await _holidayService.GetHolidaysAsync(year);
            return Ok(holidays);
        }

        private int? GetCompanyId()
        {
            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (string.IsNullOrEmpty(companyIdStr) || !int.TryParse(companyIdStr, out var companyId))
                return null;
            return companyId;
        }

        /// <summary>
        /// Returns (periodStart, periodEnd) for the current payroll period based on cycle type.
        /// </summary>
        private static (DateTime From, DateTime To) GetCurrentPayrollPeriod(Enums.PayrollCycle cycle, DateTime today)
        {
            switch (cycle)
            {
                case Enums.PayrollCycle.SemiMonthly:
                    if (today.Day <= 15)
                        return (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 15));
                    else
                    {
                        var lastDay = DateTime.DaysInMonth(today.Year, today.Month);
                        return (new DateTime(today.Year, today.Month, 16), new DateTime(today.Year, today.Month, lastDay));
                    }

                case Enums.PayrollCycle.Monthly:
                default:
                    {
                        var lastDay = DateTime.DaysInMonth(today.Year, today.Month);
                        return (new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, lastDay));
                    }
            }
        }

        /// <summary>
        /// Returns the end date of the previous payroll period (archive cutoff).
        /// Records on or before this date are considered archived.
        /// </summary>
        private static DateTime GetArchiveCutoff(Enums.PayrollCycle cycle, DateTime today)
        {
            var (periodStart, _) = GetCurrentPayrollPeriod(cycle, today);
            return periodStart.AddDays(-1);
        }

        public class ScanRequest
        {
            public string QrValue { get; set; } = string.Empty;
            public string? DeviceId { get; set; }
        }
    }
}
