using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PayRexApplication.Data;
using PayRexApplication.Models;
using PayRexApplication.Enums;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,Accountant")]
    [IgnoreAntiforgeryToken]
    public class FinanceModel : PageModel
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpClientFactory;

        public FinanceModel(AppDbContext db, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)] public DateTime? FromDate { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? ToDate { get; set; }

        public List<IncomeItem> Income { get; set; } = new();
        public List<CombinedExpenseItem> Expenses { get; set; } = new();
        public List<ContributionItem> Contributions { get; set; } = new();

        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal TotalContributions { get; set; }
        public decimal NetBalance => TotalIncome - TotalExpenses;

        public async Task OnGetAsync()
        {
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId)) return;

            var from = FromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var to = (ToDate ?? DateTime.Now).Date.AddDays(1).AddSeconds(-1);
            FromDate = from;
            ToDate = to.Date;

            // ── Income records ──
            try
            {
                Income = await _db.IncomeRecords.AsNoTracking()
                    .Where(i => i.CompanyId == companyId && i.Date >= from && i.Date <= to)
                    .OrderByDescending(i => i.Date)
                    .Select(i => new IncomeItem
                    {
                        Id = i.Id,
                        Date = i.Date,
                        Source = i.Source,
                        Category = i.Category,
                        Amount = i.Amount,
                        Note = i.Note ?? ""
                    }).ToListAsync();
                TotalIncome = Income.Sum(i => i.Amount);
            }
            catch { Income = new(); }

            // ── Expenses = manual expense records + released/approved payroll periods ──
            try
            {
                var manualExpenses = await _db.ExpenseRecords.AsNoTracking()
                    .Where(e => e.CompanyId == companyId && e.Date >= from && e.Date <= to)
                    .OrderByDescending(e => e.Date)
                    .Select(e => new CombinedExpenseItem
                    {
                        Id = e.Id,
                        Date = e.Date,
                        Description = e.Payee,
                        Category = e.Category,
                        Amount = e.Amount,
                        Note = e.Note ?? "",
                        Type = "Manual"
                    }).ToListAsync();

                var fromDateOnly = DateOnly.FromDateTime(from);
                var toDateOnly = DateOnly.FromDateTime(to);

                var payrollRows = await _db.PayrollPeriods
                    .AsNoTracking()
                    .Where(p => p.CompanyId == companyId
                        && (p.Status == PayrollPeriodStatus.Released || p.Status == PayrollPeriodStatus.Approved)
                        && p.EndDate >= fromDateOnly
                        && p.StartDate <= toDateOnly)
                    .Select(p => new
                    {
                        p.PayrollPeriodId,
                        p.PeriodName,
                        p.UpdatedAt,
                        p.CreatedAt,
                        p.Status,
                        TotalNet = p.PayrollSummaries.Sum(s => s.NetPay),
                        EmpCount = p.PayrollSummaries.Count()
                    })
                    .ToListAsync();

                var payrollExpenses = payrollRows.Select(p => new CombinedExpenseItem
                {
                    Id = p.PayrollPeriodId,
                    Date = p.UpdatedAt ?? p.CreatedAt,
                    Description = p.PeriodName ?? "Payroll Period",
                    Category = "Payroll",
                    Amount = p.TotalNet,
                    Note = $"{p.EmpCount} employees · {p.Status}",
                    Type = "Payroll"
                });

                Expenses = manualExpenses.Concat(payrollExpenses).OrderByDescending(e => e.Date).ToList();
                TotalExpenses = Expenses.Sum(e => e.Amount);
            }
            catch { Expenses = new(); }

            // ── Government Contributions ──
            try
            {
                var fromDateOnly = DateOnly.FromDateTime(from);
                var toDateOnly = DateOnly.FromDateTime(to);

                var contribRows = await _db.GovernmentContributions
                    .AsNoTracking()
                    .Where(c => c.Employee.CompanyId == companyId
                        && c.PayrollPeriod.EndDate >= fromDateOnly
                        && c.PayrollPeriod.StartDate <= toDateOnly)
                    .Select(c => new
                    {
                        c.PayrollPeriodId,
                        PeriodName = c.PayrollPeriod.PeriodName ?? "Period",
                        PeriodEndYear = c.PayrollPeriod.EndDate.Year,
                        PeriodEndMonth = c.PayrollPeriod.EndDate.Month,
                        PeriodEndDay = c.PayrollPeriod.EndDate.Day,
                        TypeName = c.Type.ToString(),
                        c.EmployeeShare,
                        c.EmployerShare
                    })
                    .ToListAsync();

                Contributions = contribRows
                    .GroupBy(c => new { c.PayrollPeriodId, c.PeriodName, c.PeriodEndYear, c.PeriodEndMonth, c.PeriodEndDay, c.TypeName })
                    .Select(g => new ContributionItem
                    {
                        PeriodName = g.Key.PeriodName,
                        PeriodEnd = new DateTime(g.Key.PeriodEndYear, g.Key.PeriodEndMonth, g.Key.PeriodEndDay),
                        Type = g.Key.TypeName,
                        EmployeeShare = g.Sum(x => x.EmployeeShare),
                        EmployerShare = g.Sum(x => x.EmployerShare),
                        Count = g.Count()
                    })
                    .OrderByDescending(c => c.PeriodEnd).ThenBy(c => c.Type)
                    .ToList();

                TotalContributions = Contributions.Sum(c => c.EmployeeShare + c.EmployerShare);
            }
            catch { Contributions = new(); }
        }

        // ── DTOs ──
        public class IncomeItem
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string Source { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Amount { get; set; }
            public string Note { get; set; } = "";
        }

        public class CombinedExpenseItem
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string Description { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Amount { get; set; }
            public string Note { get; set; } = "";
            public string Type { get; set; } = "Manual";
        }

        public class ContributionItem
        {
            public string PeriodName { get; set; } = "";
            public DateTime PeriodEnd { get; set; }
            public string Type { get; set; } = "";
            public decimal EmployeeShare { get; set; }
            public decimal EmployerShare { get; set; }
            public int Count { get; set; }
        }

        public async Task<IActionResult> OnPostAddIncomeAsync([FromBody] IncomeRequest req)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return Unauthorized();
            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var body = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("api/admin/finance/income", body);
            return resp.IsSuccessStatusCode ? new OkResult() : StatusCode((int)resp.StatusCode);
        }

        public async Task<IActionResult> OnPostAddExpenseAsync([FromBody] ExpenseRequest req)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return Unauthorized();
            var client = _httpClientFactory.CreateClient("PayRexApi");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var body = new StringContent(JsonSerializer.Serialize(req), Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("api/admin/finance/expense", body);
            return resp.IsSuccessStatusCode ? new OkResult() : StatusCode((int)resp.StatusCode);
        }

        public class IncomeRequest { public DateTime Date { get; set; } public string Source { get; set; } = ""; public string Category { get; set; } = ""; public decimal Amount { get; set; } public string? Note { get; set; } }
        public class ExpenseRequest { public DateTime Date { get; set; } public string Payee { get; set; } = ""; public string Category { get; set; } = ""; public decimal Amount { get; set; } public string? Note { get; set; } }
    }
}