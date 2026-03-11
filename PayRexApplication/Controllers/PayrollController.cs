using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using PayRexApplication.Models;
using PayRexApplication.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayrollController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PayrollController> _logger;
        private readonly IActivityLoggerService _audit;

        public PayrollController(AppDbContext db, ILogger<PayrollController> logger, IActivityLoggerService audit)
        {
            _db = db;
            _logger = logger;
            _audit = audit;
        }

        // ──────────── PAYROLL PERIODS ────────────

        [HttpGet("periods")]
        [Authorize(Roles = "Admin,Accountant,HR,Employee")]
        public async Task<IActionResult> GetPeriods()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var periods = await _db.PayrollPeriods
                .Where(p => p.CompanyId == companyId)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new
                {
                    p.PayrollPeriodId,
                    p.StartDate,
                    p.EndDate,
                    p.PeriodName,
                    Status = p.Status.ToString(),
                    EmployeeCount = p.PayrollSummaries.Count,
                    TotalNetPay = p.PayrollSummaries.Sum(s => s.NetPay),
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(periods);
        }

        [HttpPost("periods")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> CreatePeriod([FromBody] CreatePeriodDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var period = new PayrollPeriod
            {
                CompanyId = companyId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                PeriodName = dto.PeriodName ?? $"{dto.StartDate:MMM dd} - {dto.EndDate:MMM dd, yyyy}",
                Status = PayrollPeriodStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _db.PayrollPeriods.Add(period);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Payroll period created", payrollPeriodId = period.PayrollPeriodId });
        }

        // ──────────── AUTO-GENERATE PERIODS ────────────

        [HttpPost("periods/auto-generate")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AutoGeneratePeriods()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var companySetting = await _db.CompanySettings.FirstOrDefaultAsync(cs => cs.CompanyId == companyId);
            var payrollCycle = companySetting?.PayrollCycle ?? PayrollCycle.SemiMonthly;

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var periodsToCreate = new List<(DateOnly start, DateOnly end, string name)>();

            if (payrollCycle == PayrollCycle.SemiMonthly)
            {
                // Two periods per month: 1-15 and 16-end
                var firstHalfStart = new DateOnly(today.Year, today.Month, 1);
                var firstHalfEnd = new DateOnly(today.Year, today.Month, 15);
                var secondHalfStart = new DateOnly(today.Year, today.Month, 16);
                var secondHalfEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                periodsToCreate.Add((firstHalfStart, firstHalfEnd, $"{firstHalfStart:MMM dd} - {firstHalfEnd:MMM dd, yyyy}"));
                periodsToCreate.Add((secondHalfStart, secondHalfEnd, $"{secondHalfStart:MMM dd} - {secondHalfEnd:MMM dd, yyyy}"));
            }
            else // Monthly
            {
                var monthStart = new DateOnly(today.Year, today.Month, 1);
                var monthEnd = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                periodsToCreate.Add((monthStart, monthEnd, $"{monthStart:MMM dd} - {monthEnd:MMM dd, yyyy}"));
            }

            // Get existing periods for this company to avoid duplicates
            var existingPeriods = await _db.PayrollPeriods
                .Where(p => p.CompanyId == companyId)
                .Select(p => new { p.StartDate, p.EndDate })
                .ToListAsync();

            int created = 0;
            foreach (var (start, end, name) in periodsToCreate)
            {
                bool alreadyExists = existingPeriods.Any(ep => ep.StartDate == start && ep.EndDate == end);
                if (alreadyExists) continue;

                _db.PayrollPeriods.Add(new PayrollPeriod
                {
                    CompanyId = companyId,
                    StartDate = start,
                    EndDate = end,
                    PeriodName = name,
                    Status = PayrollPeriodStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                });
                created++;
            }

            if (created > 0)
                await _db.SaveChangesAsync();

            return Ok(new { message = $"{created} payroll period(s) auto-generated", created });
        }

        // ──────────── SALARY COMPUTATION (Run Payroll: Draft → Computed, no calculation) ────────────

        [HttpPost("compute/{periodId}")]
        [Authorize(Roles = "Admin,Accountant,HR")]
        public async Task<IActionResult> ComputeSalaries(int periodId)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.CompanyId == companyId);
            if (period == null) return NotFound(new { message = "Payroll period not found" });
            if (period.Status != PayrollPeriodStatus.Draft)
                return BadRequest(new { message = "Only Draft payroll periods can be submitted for review." });

            // Count employees to report
            var employeeCount = await _db.Employees
                .CountAsync(e => e.CompanyId == companyId && e.Status == EmployeeStatus.Active);

            period.Status = PayrollPeriodStatus.Computed;
            period.UpdatedAt = DateTime.UtcNow;

            // Notify Admin and Accountant that payroll is pending review
            _db.SystemNotifications.Add(new SystemNotification
            {
                Title = "Payroll Pending Review",
                Message = $"Payroll period '{period.PeriodName}' has been submitted for review by {role}. Please verify and approve.",
                Type = "info",
                TargetRoles = "Admin,Accountant",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            await _audit.LogAsync(userId, companyId, "Payroll Run", "PayrollPeriod", periodId.ToString(),
                "Draft", "Computed", ip, ua, role, period.PeriodName);

            _logger.LogInformation("Payroll period {PeriodId} submitted for review by {Role}", periodId, role);
            return Ok(new { message = $"Payroll period '{period.PeriodName}' submitted for review. {employeeCount} employees will be processed upon approval.", employeeCount });
        }

        // ──────────── SALARY SUMMARIES ────────────

        [HttpGet("summaries/{periodId}")]
        [Authorize(Roles = "Admin,Accountant,Employee")]
        public async Task<IActionResult> GetSummaries(int periodId)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var query = _db.PayrollSummaries
                .Include(s => s.Employee)
                .Where(s => s.PayrollPeriodId == periodId && s.Employee.CompanyId == companyId);

            // Employees only see their own
            if (role == "Employee")
            {
                var emp = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
                if (emp == null) return Ok(new List<object>());
                query = query.Where(s => s.EmployeeId == emp.EmployeeNumber);
            }

            var summaries = await query
                .OrderBy(s => s.Employee.LastName)
                .Select(s => new
                {
                    s.PayrollSummaryId,
                    s.EmployeeId,
                    EmployeeName = s.Employee.FirstName + " " + s.Employee.LastName,
                    EmployeeCode = s.Employee.EmployeeCode,
                    s.BasicPay,
                    s.OvertimePay,
                    s.HolidayPay,
                    s.Allowances,
                    s.GrossPay,
                    s.TotalDeductions,
                    s.NetPay
                })
                .ToListAsync();

            return Ok(summaries);
        }

        // ──────────── GOVERNMENT CONTRIBUTIONS ────────────

        [HttpGet("contributions")]
        [Authorize(Roles = "Admin,Accountant,Employee,HR")]
        public async Task<IActionResult> GetContributions([FromQuery] int? periodId, [FromQuery] bool selfOnly = false)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var query = _db.GovernmentContributions
                .Include(c => c.Employee)
                .Include(c => c.PayrollPeriod)
                .Where(c => c.Employee.CompanyId == companyId);

            if (periodId.HasValue)
                query = query.Where(c => c.PayrollPeriodId == periodId.Value);

            if (role == "Employee" || selfOnly)
            {
                var emp = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
                if (emp == null) return Ok(new List<object>());
                query = query.Where(c => c.EmployeeId == emp.EmployeeNumber);
            }

            var contribs = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.ContributionId,
                    EmployeeName = c.Employee.FirstName + " " + c.Employee.LastName,
                    Type = c.Type.ToString(),
                    c.EmployeeShare,
                    c.EmployerShare,
                    PeriodName = c.PayrollPeriod.PeriodName,
                    c.PayrollPeriodId
                })
                .ToListAsync();

            return Ok(contribs);
        }

        // ──────────── COMPENSATION (Deductions & Benefits) ────────────

        public class AddCompensationDto
        {
            public int EmployeeId { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public bool IsRecurring { get; set; }
        }

        [HttpPost("deductions")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddDeduction([FromBody] AddCompensationDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == dto.EmployeeId && e.CompanyId == companyId);
            if (emp == null) return NotFound(new { message = "Employee not found." });

            var deduction = new EmployeeDeduction
            {
                EmployeeId = dto.EmployeeId,
                Name = dto.Name,
                Amount = dto.Amount,
                Recurring = dto.IsRecurring,
                CreatedAt = DateTime.UtcNow
            };
            _db.EmployeeDeductions.Add(deduction);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deduction added successfully." });
        }

        [HttpPost("benefits")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddBenefit([FromBody] AddCompensationDto dto)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var emp = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeNumber == dto.EmployeeId && e.CompanyId == companyId);
            if (emp == null) return NotFound(new { message = "Employee not found." });

            var benefit = new EmployeeBenefit
            {
                EmployeeId = dto.EmployeeId,
                Name = dto.Name,
                Amount = dto.Amount,
                CreatedAt = DateTime.UtcNow
            };
            _db.EmployeeBenefits.Add(benefit);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Benefit added successfully." });
        }

        [HttpGet("compensation")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> GetCompensation()
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var deductions = await _db.EmployeeDeductions
                .Include(d => d.Employee)
                .Where(d => d.Employee.CompanyId == companyId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new
                {
                    d.DeductionId,
                    EmployeeName = d.Employee.FirstName + " " + d.Employee.LastName,
                    Type = d.Name,
                    d.Amount,
                    IsActive = d.Recurring,
                    d.CreatedAt
                })
                .ToListAsync();

            var benefits = await _db.EmployeeBenefits
                .Include(b => b.Employee)
                .Where(b => b.Employee.CompanyId == companyId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BenefitId,
                    EmployeeName = b.Employee.FirstName + " " + b.Employee.LastName,
                    Type = b.Name,
                    b.Amount,
                    IsActive = true,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(new { deductions, benefits });
        }

        // ──────────── PAYSLIPS ────────────

        [HttpGet("payslips")]
        [Authorize(Roles = "Admin,Accountant,Employee,HR")]
        public async Task<IActionResult> GetPayslips([FromQuery] bool selfOnly = false)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var query = _db.Payslips
                .Include(p => p.PayrollSummary)
                    .ThenInclude(s => s.Employee)
                .Include(p => p.PayrollSummary)
                    .ThenInclude(s => s.PayrollPeriod)
                .Where(p => p.PayrollSummary.Employee.CompanyId == companyId);

            if (role == "Employee" || selfOnly)
            {
                var emp = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.CompanyId == companyId);
                if (emp == null) return Ok(new List<object>());
                query = query.Where(p => p.PayrollSummary.EmployeeId == emp.EmployeeNumber);
            }

            var payslipList = await query
                .OrderByDescending(p => p.GeneratedAt)
                .Select(p => new
                {
                    p.PayslipId,
                    EmployeeName = p.PayrollSummary.Employee.FirstName + " " + p.PayrollSummary.Employee.LastName,
                    PeriodName = p.PayrollSummary.PayrollPeriod.PeriodName,
                    p.PayrollSummary.BasicPay,
                    p.PayrollSummary.OvertimePay,
                    p.PayrollSummary.HolidayPay,
                    p.PayrollSummary.Allowances,
                    p.PayrollSummary.GrossPay,
                    p.PayrollSummary.TotalDeductions,
                    p.PayrollSummary.NetPay,
                    p.Released,
                    p.GeneratedAt,
                    p.PdfPath,
                    EmployeeId = p.PayrollSummary.EmployeeId,
                    PayrollPeriodId = p.PayrollSummary.PayrollPeriodId
                })
                .ToListAsync();

            // Fetch contributions for each payslip's employee+period
            var result = new List<object>();
            foreach (var ps in payslipList)
            {
                var contribs = await _db.GovernmentContributions
                    .Where(c => c.EmployeeId == ps.EmployeeId && c.PayrollPeriodId == ps.PayrollPeriodId)
                    .ToListAsync();

                var sss = contribs.Where(c => c.Type == ContributionType.SSS).Sum(c => c.EmployeeShare);
                var philHealth = contribs.Where(c => c.Type == ContributionType.PhilHealth).Sum(c => c.EmployeeShare);
                var pagIbig = contribs.Where(c => c.Type == ContributionType.PagIig).Sum(c => c.EmployeeShare);

                // Withholding tax estimate (simplified BIR table approximation)
                var taxableIncome = (ps.GrossPay) - sss - philHealth - pagIbig;
                var withholdingTax = taxableIncome > 20833m ? Math.Round((taxableIncome - 20833m) * 0.15m, 2) : 0m;

                result.Add(new
                {
                    ps.PayslipId,
                    ps.EmployeeName,
                    ps.PeriodName,
                    ps.BasicPay,
                    ps.OvertimePay,
                    ps.HolidayPay,
                    ps.Allowances,
                    ps.GrossPay,
                    ps.TotalDeductions,
                    ps.NetPay,
                    ps.Released,
                    ps.GeneratedAt,
                    ps.PdfPath,
                    SssContribution = sss,
                    PhilHealthContribution = philHealth,
                    PagIbigContribution = pagIbig,
                    WithholdingTax = withholdingTax
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Generate payslips for all employees in a payroll period.
        /// </summary>
        [HttpPost("payslips/generate/{periodId}")]
        [Authorize(Roles = "Admin,Accountant,HR")]
        public async Task<IActionResult> GeneratePayslips(int periodId)
        {
            var companyId = GetCompanyId();
            if (companyId == 0) return Unauthorized();

            var summaries = await _db.PayrollSummaries
                .Where(s => s.PayrollPeriodId == periodId && s.Employee.CompanyId == companyId)
                .Include(s => s.Payslip)
                .ToListAsync();

            if (!summaries.Any())
                return BadRequest(new { message = "No payroll summaries found. Please compute salaries first." });

            int generated = 0;
            foreach (var summary in summaries)
            {
                if (summary.Payslip == null)
                {
                    _db.Payslips.Add(new Payslip
                    {
                        PayrollSummaryId = summary.PayrollSummaryId,
                        GeneratedAt = DateTime.UtcNow,
                        Released = false
                    });
                    generated++;
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = $"{generated} payslips generated", count = generated });
        }

        // ──────────── PAYROLL PERIOD STATUS TRANSITIONS ────────────

        [HttpPost("periods/{periodId}/approve")]
        [Authorize(Roles = "Admin,Accountant,HR")]
        public async Task<IActionResult> ApprovePeriod(int periodId)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.CompanyId == companyId);
            if (period == null) return NotFound(new { message = "Payroll period not found." });
            if (period.Status != PayrollPeriodStatus.Computed)
                return BadRequest(new { message = "Only computed payroll periods can be approved." });

            // ── Run salary calculations on approval ──
            var companySetting = await _db.CompanySettings.FirstOrDefaultAsync(cs => cs.CompanyId == companyId);
            decimal workHoursPerDay = (decimal)(companySetting?.WorkHoursPerDay ?? 8.0m);
            decimal overtimeMultiplier = companySetting?.OvertimeRate ?? 1.25m;
            decimal holidayMultiplier = companySetting?.HolidayRate ?? 2.0m;
            int lateGraceMinutes = companySetting?.LateGraceMinutes ?? 15;
            var payrollCycle = companySetting?.PayrollCycle ?? PayrollCycle.SemiMonthly;

            var systemSettings = await _db.SystemSettings.OrderByDescending(s => s.EffectiveDate).FirstOrDefaultAsync();
            decimal sssRate = systemSettings?.SssPercentage ?? 4.5m;
            decimal philHealthRate = systemSettings?.PhilHealthPercentage ?? 2.25m;
            decimal pagIbigRate = systemSettings?.PagIbigPercentage ?? 2.0m;

            var employees = await _db.Employees
                .Where(e => e.CompanyId == companyId && e.Status == EmployeeStatus.Active)
                .Include(e => e.EmployeeDeductions)
                .Include(e => e.EmployeeBenefits)
                .ToListAsync();

            // Remove any existing summaries/contributions and recompute fresh
            var existingSummaries = await _db.PayrollSummaries.Where(s => s.PayrollPeriodId == periodId).ToListAsync();
            _db.PayrollSummaries.RemoveRange(existingSummaries);
            var existingContribs = await _db.GovernmentContributions.Where(c => c.PayrollPeriodId == periodId).ToListAsync();
            _db.GovernmentContributions.RemoveRange(existingContribs);

            int workingDaysInPeriod = 0;
            for (var d = period.StartDate; d <= period.EndDate; d = d.AddDays(1))
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    workingDaysInPeriod++;

            foreach (var emp in employees)
            {
                decimal periodRate = payrollCycle == PayrollCycle.SemiMonthly
                    ? emp.SalaryRate / 2
                    : emp.SalaryRate;

                var attendance = await _db.AttendanceRecords
                    .Where(a => a.EmployeeId == emp.EmployeeNumber && a.CompanyId == companyId
                        && a.Date >= period.StartDate && a.Date <= period.EndDate)
                    .ToListAsync();

                int daysPresent = attendance.Count(a => a.Status == "Present" || a.Status == "Late");
                int daysOnLeave = attendance.Count(a => a.Status == "On Leave");
                decimal dailyRate = workingDaysInPeriod > 0 ? periodRate / workingDaysInPeriod : 0m;
                decimal basicPay = dailyRate * daysPresent;

                decimal overtimeHrs = attendance.Where(a => a.OvertimeHours.HasValue).Sum(a => a.OvertimeHours!.Value);
                decimal hourlyRate = workHoursPerDay > 0 ? dailyRate / workHoursPerDay : 0m;
                decimal overtimePay = Math.Round(overtimeHrs * hourlyRate * overtimeMultiplier, 2);
                decimal holidayPay = attendance.Where(a => a.IsHoliday && a.Status == "Present").Sum(a => dailyRate * holidayMultiplier);
                decimal allowances = emp.EmployeeBenefits.Sum(b => b.Amount);
                decimal grossPay = basicPay + overtimePay + holidayPay + allowances;

                decimal sss = Math.Round(grossPay * sssRate / 100m, 2);
                decimal philHealth = Math.Round(grossPay * philHealthRate / 100m, 2);
                decimal pagIbig = Math.Min(Math.Round(grossPay * pagIbigRate / 100m, 2), 200m);
                decimal additionalDeductions = emp.EmployeeDeductions.Sum(d => d.Amount);
                int totalLateMinutes = attendance.Where(a => a.LateMinutes.HasValue).Sum(a => Math.Max(0, a.LateMinutes!.Value - lateGraceMinutes));
                decimal lateDeduction = Math.Round(hourlyRate / 60m * totalLateMinutes, 2);
                decimal totalDeductions = sss + philHealth + pagIbig + additionalDeductions + lateDeduction;
                decimal netPay = grossPay - totalDeductions;

                _db.PayrollSummaries.Add(new PayrollSummary
                {
                    PayrollPeriodId = periodId,
                    EmployeeId = emp.EmployeeNumber,
                    GrossPay = Math.Round(grossPay, 2),
                    TotalDeductions = Math.Round(totalDeductions, 2),
                    NetPay = Math.Round(netPay, 2),
                    BasicPay = Math.Round(basicPay, 2),
                    OvertimePay = Math.Round(overtimePay, 2),
                    HolidayPay = Math.Round(holidayPay, 2),
                    Allowances = Math.Round(allowances, 2),
                    CreatedAt = DateTime.UtcNow
                });

                _db.GovernmentContributions.AddRange(new[]
                {
                    new GovernmentContribution { EmployeeId = emp.EmployeeNumber, PayrollPeriodId = periodId, Type = ContributionType.SSS, EmployeeShare = sss, EmployerShare = Math.Round(sss * 2, 2), CreatedAt = DateTime.UtcNow },
                    new GovernmentContribution { EmployeeId = emp.EmployeeNumber, PayrollPeriodId = periodId, Type = ContributionType.PhilHealth, EmployeeShare = philHealth, EmployerShare = philHealth, CreatedAt = DateTime.UtcNow },
                    new GovernmentContribution { EmployeeId = emp.EmployeeNumber, PayrollPeriodId = periodId, Type = ContributionType.PagIig, EmployeeShare = pagIbig, EmployerShare = pagIbig, CreatedAt = DateTime.UtcNow }
                });
            }

            // Get approver's display name
            var approver = await _db.Users.FindAsync(userId);
            var approverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : role;

            period.Status = PayrollPeriodStatus.Approved;
            period.UpdatedAt = DateTime.UtcNow;

            // Notify Admin that payroll is approved and ready to release
            _db.SystemNotifications.Add(new SystemNotification
            {
                Title = "Payroll Approved",
                Message = $"Payroll period '{period.PeriodName}' has been approved by {approverName} ({role}). It is now ready for release.",
                Type = "success",
                TargetRoles = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            await _audit.LogAsync(userId, companyId, "Payroll Approved", "PayrollPeriod", periodId.ToString(),
                "Computed", "Approved", ip, ua, role, period.PeriodName);

            _logger.LogInformation("Payroll period {PeriodId} approved by {Role}, computed for {Count} employees", periodId, role, employees.Count);
            return Ok(new { message = $"Payroll approved. Salaries calculated for {employees.Count} employees.", employeeCount = employees.Count });
        }

        [HttpPost("periods/{periodId}/reject")]
        [Authorize(Roles = "Admin,Accountant,HR")]
        public async Task<IActionResult> RejectPeriod(int periodId, [FromBody] PeriodActionDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Remarks))
                return BadRequest(new { message = "Remarks are required when rejecting a payroll period." });

            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var period = await _db.PayrollPeriods.FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.CompanyId == companyId);
            if (period == null) return NotFound(new { message = "Payroll period not found." });
            if (period.Status != PayrollPeriodStatus.Computed)
                return BadRequest(new { message = "Only computed payroll periods can be rejected." });

            // Clear any partial data when rejecting
            var existingSummaries = await _db.PayrollSummaries.Where(s => s.PayrollPeriodId == periodId).ToListAsync();
            _db.PayrollSummaries.RemoveRange(existingSummaries);
            var existingContribs = await _db.GovernmentContributions.Where(c => c.PayrollPeriodId == periodId).ToListAsync();
            _db.GovernmentContributions.RemoveRange(existingContribs);

            var rejecter = await _db.Users.FindAsync(userId);
            var rejecterName = rejecter != null ? $"{rejecter.FirstName} {rejecter.LastName}" : role;

            period.Status = PayrollPeriodStatus.Draft;
            period.UpdatedAt = DateTime.UtcNow;

            // Notify HR and Admin about the rejection
            _db.SystemNotifications.Add(new SystemNotification
            {
                Title = "Payroll Rejected",
                Message = $"Payroll period '{period.PeriodName}' was rejected by {rejecterName} ({role}). Reason: {dto.Remarks}",
                Type = "warning",
                TargetRoles = "HR,Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            await _audit.LogAsync(userId, companyId, "Payroll Rejected", "PayrollPeriod", periodId.ToString(),
                "Computed", $"Draft (reason: {dto.Remarks})", ip, ua, role, period.PeriodName);

            return Ok(new { message = "Payroll period rejected and returned to Draft." });
        }

        [HttpPost("periods/{periodId}/release")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReleasePeriod(int periodId)
        {
            var (userId, companyId, role) = GetUserInfo();
            if (companyId == 0) return Unauthorized();

            var period = await _db.PayrollPeriods
                .Include(p => p.PayrollSummaries)
                    .ThenInclude(s => s.Payslip)
                .FirstOrDefaultAsync(p => p.PayrollPeriodId == periodId && p.CompanyId == companyId);
            if (period == null) return NotFound(new { message = "Payroll period not found." });
            if (period.Status != PayrollPeriodStatus.Approved)
                return BadRequest(new { message = "Only approved payroll periods can be released." });
            if (!period.PayrollSummaries.Any())
                return BadRequest(new { message = "No payroll summaries found. Please approve the payroll period first to trigger salary calculation." });

            // Generate payslips for summaries that don't have one yet
            int generated = 0;
            foreach (var summary in period.PayrollSummaries)
            {
                if (summary.Payslip == null)
                {
                    _db.Payslips.Add(new Payslip
                    {
                        PayrollSummaryId = summary.PayrollSummaryId,
                        GeneratedAt = DateTime.UtcNow,
                        Released = true
                    });
                    generated++;
                }
                else
                {
                    summary.Payslip.Released = true;
                }
            }

            period.Status = PayrollPeriodStatus.Released;
            period.UpdatedAt = DateTime.UtcNow;

            // Notify Employees that their payslips are available
            _db.SystemNotifications.Add(new SystemNotification
            {
                Title = "Payslip Available",
                Message = $"Your payslip for payroll period '{period.PeriodName}' is now available. Please check your payslip section.",
                Type = "success",
                TargetRoles = "Employee",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });

            await _db.SaveChangesAsync();

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            await _audit.LogAsync(userId, companyId, "Payroll Released", "PayrollPeriod", periodId.ToString(),
                "Approved", "Released", ip, ua, role, period.PeriodName);

            int totalCount = period.PayrollSummaries.Count;
            _logger.LogInformation("Payroll period {PeriodId} released by Admin. {Count} payslips generated/released.", periodId, totalCount);
            return Ok(new { message = $"Payroll released. {totalCount} payslips are now available to employees.", count = totalCount });
        }

        // ──────────── HELPERS ────────────

        private int GetCompanyId()
        {
            var (_, cid, _) = GetUserInfo();
            return cid;
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
            catch { return (0, 0, ""); }
        }
    }

    public class CreatePeriodDto
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string? PeriodName { get; set; }
    }

    public class PeriodActionDto
    {
        public string? Remarks { get; set; }
    }
}
