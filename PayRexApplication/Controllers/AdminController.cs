using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.Services;

namespace PayRexApplication.Controllers
{
    /// <summary>
    /// Admin-level endpoints scoped to the requesting admin's company.
    /// All data returned is automatically filtered to the company embedded in the JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,HR,Accountant")]
    public class AdminController : ControllerBase
    {
        private readonly ISuperAdminService _service;
        private readonly AppDbContext _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ISuperAdminService service, AppDbContext db, ILogger<AdminController> logger)
        {
            _service = service;
            _db = db;
            _logger = logger;
        }

        // ──────────────────────────────────────────────
        // GET api/admin/audit-logs
        // Returns audit logs scoped to the calling admin's company.
        // ──────────────────────────────────────────────
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? search = null,
            [FromQuery] string? action = null,
            [FromQuery] string? from = null,
            [FromQuery] string? to = null)
        {
            var companyIdStr = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdStr, out var companyId))
                return Forbid();

            DateTime? fromDate = null, toDate = null;
            if (DateTime.TryParse(from, out var fd)) fromDate = fd;
            if (DateTime.TryParse(to, out var td)) toDate = td.Date.AddDays(1);

            var result = await _service.GetAuditLogsAsync(page, pageSize, search, action, fromDate, toDate, companyId);
            return Ok(result);
        }

        private int? GetCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : (int?)null;
        }

        // ──────────────────────────────────────────────
        // GET api/admin/my-permissions
        // Returns the calling user's role permissions.
        // ──────────────────────────────────────────────
        [HttpGet("my-permissions")]
        public async Task<IActionResult> GetMyPermissions()
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return Forbid();

            var permissions = await _db.RolePermissions
                .AsNoTracking()
                .Where(p => p.RoleName == role)
                .Select(p => new
                {
                    p.PermissionId,
                    p.ModuleName,
                    p.CanAdd,
                    p.CanUpdate,
                    p.CanInactivate
                })
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpPost("finance/income")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddIncome([FromBody] IncomeRequest req)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var record = new IncomeRecord
            {
                CompanyId = companyId.Value,
                Date = req.Date,
                Source = req.Source,
                Category = req.Category,
                Amount = req.Amount,
                Note = req.Note ?? ""
            };
            _db.IncomeRecords.Add(record);
            await _db.SaveChangesAsync();
            return Ok(new { record.Id });
        }

        [HttpPost("finance/expense")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> AddExpense([FromBody] ExpenseRequest req)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var record = new ExpenseRecord
            {
                CompanyId = companyId.Value,
                Date = req.Date,
                Payee = req.Payee,
                Category = req.Category,
                Amount = req.Amount,
                Note = req.Note ?? ""
            };
            _db.ExpenseRecords.Add(record);
            await _db.SaveChangesAsync();
            return Ok(new { record.Id });
        }

        [HttpDelete("finance/income/{id}")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var record = await _db.IncomeRecords.FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId.Value);
            if (record == null) return NotFound();

            _db.IncomeRecords.Remove(record);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("finance/expense/{id}")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var record = await _db.ExpenseRecords.FirstOrDefaultAsync(r => r.Id == id && r.CompanyId == companyId.Value);
            if (record == null) return NotFound();

            _db.ExpenseRecords.Remove(record);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

    public class IncomeRequest
    {
        public DateTime Date { get; set; }
        public string Source { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class ExpenseRequest
    {
        public DateTime Date { get; set; }
        public string Payee { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}
