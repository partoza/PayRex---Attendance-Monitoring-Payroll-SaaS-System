using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using PayRexApplication.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveRequestController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<LeaveRequestController> _logger;

        public LeaveRequestController(AppDbContext db, ILogger<LeaveRequestController> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Get leave requests. Employees see their own; HR/Admin see all for their company.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] string? status, [FromQuery] bool selfOnly = false)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            IQueryable<LeaveRequest> query = _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.Reviewer)
                .Where(lr => lr.CompanyId == companyId && !lr.IsArchived);

            // Employees (or Employee View mode) only see their own requests
            if (role == "Employee" || selfOnly)
            {
                var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
                if (employee == null) return Ok(new List<object>());
                query = query.Where(lr => lr.EmployeeId == employee.EmployeeNumber);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeaveStatus>(status, true, out var s))
            {
                query = query.Where(lr => lr.Status == s);
            }

            var results = await query
                .OrderByDescending(lr => lr.CreatedAt)
                .Select(lr => new
                {
                    lr.LeaveRequestId,
                    lr.EmployeeId,
                    EmployeeName = lr.Employee.FirstName + " " + lr.Employee.LastName,
                    EmployeeCode = lr.Employee.EmployeeCode,
                    LeaveType = lr.LeaveType.ToString(),
                    lr.StartDate,
                    lr.EndDate,
                    lr.TotalDays,
                    lr.Reason,
                    Status = lr.Status.ToString(),
                    lr.ReviewRemarks,
                    ReviewerName = lr.Reviewer != null ? lr.Reviewer.FirstName + " " + lr.Reviewer.LastName : null,
                    lr.ReviewedAt,
                    lr.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Employee creates a leave request.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Employee,HR,Admin")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto dto)
        {
            var (userId, companyId, _) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
            if (employee == null)
                return BadRequest(new { message = "Employee record not found" });

            if (dto.StartDate > dto.EndDate)
                return BadRequest(new { message = "Start date must be before end date" });

            // Calculate business days
            int totalDays = 0;
            for (var d = dto.StartDate; d <= dto.EndDate; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            if (totalDays <= 0)
                return BadRequest(new { message = "Leave must include at least one business day" });

            var leave = new LeaveRequest
            {
                EmployeeId = employee.EmployeeNumber,
                CompanyId = companyId,
                LeaveType = dto.LeaveType,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                TotalDays = totalDays,
                Reason = dto.Reason,
                Status = LeaveStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _db.LeaveRequests.Add(leave);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Leave request {Id} created by employee {EmpId}", leave.LeaveRequestId, employee.EmployeeNumber);

            return Ok(new { message = "Leave request submitted successfully", leaveRequestId = leave.LeaveRequestId });
        }

        /// <summary>
        /// HR/Admin reviews (approves/rejects) a leave request.
        /// </summary>
        [HttpPut("{id}/review")]
        [Authorize(Roles = "HR,Admin,Accountant")]
        public async Task<IActionResult> ReviewLeaveRequest(int id, [FromBody] ReviewLeaveRequestDto dto)
        {
            var (userId, companyId, _) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var leave = await _db.LeaveRequests.FirstOrDefaultAsync(lr => lr.LeaveRequestId == id && lr.CompanyId == companyId);
            if (leave == null)
                return NotFound(new { message = "Leave request not found" });

            if (leave.Status != LeaveStatus.Pending)
                return BadRequest(new { message = "This leave request has already been reviewed" });

            leave.Status = dto.Approved ? LeaveStatus.Approved : LeaveStatus.Rejected;
            leave.ReviewedBy = userId;
            leave.ReviewRemarks = dto.Remarks;
            leave.ReviewedAt = DateTime.UtcNow;
            leave.UpdatedAt = DateTime.UtcNow;

            // If approved, create "On Leave" attendance records for the leave days
            if (dto.Approved)
            {
                for (var d = leave.StartDate; d <= leave.EndDate; d = d.AddDays(1))
                {
                    if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    // Check if an attendance record already exists for that day
                    var existingRecord = await _db.AttendanceRecords
                        .FirstOrDefaultAsync(ar => ar.EmployeeId == leave.EmployeeId && ar.Date == d);

                    if (existingRecord == null)
                    {
                        _db.AttendanceRecords.Add(new AttendanceRecord
                        {
                            EmployeeId = leave.EmployeeId,
                            CompanyId = leave.CompanyId,
                            Date = d,
                            Source = AttendanceSource.Manual,
                            Status = "On Leave",
                            Remarks = $"{leave.LeaveType} Leave (Request #{leave.LeaveRequestId})",
                            TotalHoursWorked = 0,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existingRecord.Status = "On Leave";
                        existingRecord.Remarks = $"{leave.LeaveType} Leave (Request #{leave.LeaveRequestId})";
                        existingRecord.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            await _db.SaveChangesAsync();

            _logger.LogInformation("Leave request {Id} {Status} by user {UserId}",
                id, leave.Status, userId);

            return Ok(new { message = $"Leave request {leave.Status.ToString().ToLower()} successfully" });
        }

        /// <summary>
        /// Get archived leave requests.
        /// </summary>
        [HttpGet("archived")]
        [Authorize(Roles = "HR,Admin,Accountant,SuperAdmin")]
        public async Task<IActionResult> GetArchivedLeaveRequests([FromQuery] string? status)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            IQueryable<LeaveRequest> query = _db.LeaveRequests
                .Include(lr => lr.Employee)
                .Include(lr => lr.Reviewer)
                .Where(lr => lr.CompanyId == companyId && lr.IsArchived);

            // Filter by status
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LeaveStatus>(status, true, out var s))
            {
                query = query.Where(lr => lr.Status == s);
            }

            var results = await query
                .OrderByDescending(lr => lr.UpdatedAt ?? lr.CreatedAt)
                .Select(lr => new
                {
                    lr.LeaveRequestId,
                    lr.EmployeeId,
                    EmployeeName = lr.Employee.FirstName + " " + lr.Employee.LastName,
                    EmployeeCode = lr.Employee.EmployeeCode,
                    LeaveType = lr.LeaveType.ToString(),
                    lr.StartDate,
                    lr.EndDate,
                    lr.TotalDays,
                    lr.Reason,
                    Status = lr.Status.ToString(),
                    lr.ReviewRemarks,
                    ReviewerName = lr.Reviewer != null ? lr.Reviewer.FirstName + " " + lr.Reviewer.LastName : null,
                    lr.ReviewedAt,
                    lr.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Archive a completed (approved/rejected) leave request.
        /// </summary>
        [HttpPut("{id}/archive")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> ArchiveLeaveRequest(int id)
        {
            var (userId, companyId, _) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var leave = await _db.LeaveRequests.FirstOrDefaultAsync(lr => lr.LeaveRequestId == id && lr.CompanyId == companyId);
            if (leave == null) return NotFound(new { message = "Leave request not found" });

            if (leave.Status == LeaveStatus.Pending)
                return BadRequest(new { message = "Cannot archive a pending leave request." });

            if (leave.IsArchived)
                return BadRequest(new { message = "Leave request is already archived." });

            leave.IsArchived = true;
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Leave request {Id} archived by user {UserId}", id, userId);

            return Ok(new { message = "Leave request archived successfully." });
        }

        /// <summary>
        /// Restore an archived leave request.
        /// </summary>
        [HttpPut("{id}/unarchive")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> UnarchiveLeaveRequest(int id)
        {
            var (userId, companyId, _) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var leave = await _db.LeaveRequests.FirstOrDefaultAsync(lr => lr.LeaveRequestId == id && lr.CompanyId == companyId);
            if (leave == null) return NotFound(new { message = "Leave request not found" });

            if (!leave.IsArchived)
                return BadRequest(new { message = "Leave request is not archived." });

            leave.IsArchived = false;
            leave.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogInformation("Leave request {Id} restored by user {UserId}", id, userId);

            return Ok(new { message = "Leave request restored successfully." });
        }

        /// <summary>
        /// Get leave balance summary for an employee.
        /// </summary>
        [HttpGet("balance")]
        public async Task<IActionResult> GetLeaveBalance()
        {
            var (userId, companyId, _) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            // Load company settings for leave allocation
            var settings = await _db.CompanySettings.FindAsync(companyId);
            var vacationTotal = settings?.VacationLeaveDays ?? 15;
            var sickTotal = 15; // Default sick leave days (PH standard)

            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
            if (employee == null) return Ok(new { sickUsed = 0, vacationUsed = 0, sickTotal, vacationTotal, sickRemaining = sickTotal, vacationRemaining = vacationTotal });

            var currentYear = DateTime.UtcNow.Year;
            var approved = await _db.LeaveRequests
                .Where(lr => lr.EmployeeId == employee.EmployeeNumber
                    && lr.CompanyId == companyId
                    && lr.Status == LeaveStatus.Approved
                    && lr.StartDate.Year == currentYear)
                .ToListAsync();

            var sickUsed = approved.Where(lr => lr.LeaveType == LeaveType.Sick).Sum(lr => lr.TotalDays);
            var vacationUsed = approved.Where(lr => lr.LeaveType == LeaveType.Vacation).Sum(lr => lr.TotalDays);

            return Ok(new
            {
                sickUsed,
                vacationUsed,
                sickTotal,
                vacationTotal,
                sickRemaining = sickTotal - sickUsed,
                vacationRemaining = vacationTotal - vacationUsed
            });
        }

        private (int userId, int companyId, string role) GetUserInfo()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token)) return (0, 0, "");

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                var uid = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "uid")?.Value, out var u) ? u : 0;
                var cid = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "companyId")?.Value, out var c) ? c : 0;
                var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "";

                return (uid, cid, role);
            }
            catch
            {
                return (0, 0, "");
            }
        }
    }

    public class CreateLeaveRequestDto
    {
        public LeaveType LeaveType { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? Reason { get; set; }
    }

    public class ReviewLeaveRequestDto
    {
        public bool Approved { get; set; }
        public string? Remarks { get; set; }
    }
}
