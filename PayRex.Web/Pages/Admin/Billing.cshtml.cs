using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class BillingModel : PageModel
 {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BillingModel> _logger;

  public List<BillingItem> Invoices { get; set; } = new();
 public List<PlanItem> Plans { get; set; } = new();

    public BillingModel(IHttpClientFactory httpClientFactory, ILogger<BillingModel> logger)
 {
     _httpClientFactory = httpClientFactory;
     _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
       if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

var client = _httpClientFactory.CreateClient("PayRexApi");
  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
          var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

   try
            {
                var billingTask = client.GetAsync("api/superadmin/billing");
  var plansTask = client.GetAsync("api/superadmin/plans");
await Task.WhenAll(billingTask, plansTask);

  if (billingTask.Result.IsSuccessStatusCode)
                Invoices = JsonSerializer.Deserialize<List<BillingItem>>(await billingTask.Result.Content.ReadAsStringAsync(), opts) ?? new();

     if (plansTask.Result.IsSuccessStatusCode)
         Plans = JsonSerializer.Deserialize<List<PlanItem>>(await plansTask.Result.Content.ReadAsStringAsync(), opts) ?? new();
  }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin billing"); }

   return Page();
  }

        public class BillingItem
     {
       public int InvoiceId { get; set; }
public string InvoiceNumber { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public decimal Amount { get; set; }
   public decimal VatAmount { get; set; }
   public string Status { get; set; } = "";
  public DateTime DueDate { get; set; }
     public DateTime CreatedAt { get; set; }
}

  public class PlanItem
    {
     public int PlanId { get; set; }
       public string Name { get; set; } = "";
  public int MaxEmployees { get; set; }
      public decimal Price { get; set; }
     public string BillingCycle { get; set; } = "";
  public string Status { get; set; } = "";
  public string? Description { get; set; }
            public int? PlanUserLimit { get; set; }
  }
    }
}
