using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PayRexApplication.Data;
using PayRexApplication.Models;
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
        public FinanceModel(AppDbContext db, IHttpClientFactory httpClientFactory) { _db = db; _httpClientFactory = httpClientFactory; }

        public List<IncomeItem> Income { get; set; } = new();
        public List<ExpenseItem> Expenses { get; set; } = new();
        public int TotalItems => Income.Count + Expenses.Count;

        public async Task OnGetAsync()
        {
            // Get companyId from JWT claims
            var companyIdClaim = User.FindFirst("companyId")?.Value;
            if (!int.TryParse(companyIdClaim, out var companyId)) return;

            try
            {
                Income = await _db.IncomeRecords.AsNoTracking()
                    .Where(i => i.CompanyId == companyId)
                    .OrderByDescending(i => i.Date).Select(i => new IncomeItem {
                    Date = i.Date,
                    Source = i.Source,
                    Category = i.Category,
                    Amount = i.Amount,
                    Note = i.Note
                }).ToListAsync();
            }
            catch { Income = new(); }

            try
            {
                Expenses = await _db.ExpenseRecords.AsNoTracking()
                    .Where(e => e.CompanyId == companyId)
                    .OrderByDescending(e => e.Date).Select(e => new ExpenseItem {
                    Date = e.Date,
                    Payee = e.Payee,
                    Category = e.Category,
                    Amount = e.Amount,
                    Note = e.Note
                }).ToListAsync();
            }
            catch { Expenses = new(); }
        }

        public class IncomeItem {
            public DateTime Date { get; set; }
            public string Source { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Amount { get; set; }
            public string Note { get; set; } = "";
        }

        public class ExpenseItem {
            public DateTime Date { get; set; }
            public string Payee { get; set; } = "";
            public string Category { get; set; } = "";
            public decimal Amount { get; set; }
            public string Note { get; set; } = "";
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