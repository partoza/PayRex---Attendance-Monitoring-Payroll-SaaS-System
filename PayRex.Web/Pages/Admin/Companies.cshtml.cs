using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class CompaniesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
      private readonly ILogger<CompaniesModel> _logger;

      public List<CompanyItem> Companies { get; set; } = new();
 [TempData] public string? StatusMessage { get; set; }

     public CompaniesModel(IHttpClientFactory httpClientFactory, ILogger<CompaniesModel> logger)
        {
    _httpClientFactory = httpClientFactory;
  _logger = logger;
        }

    public async Task<IActionResult> OnGetAsync()
        {
   if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

   var client = _httpClientFactory.CreateClient("PayRexApi");
  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

       try
         {
                var response = await client.GetAsync("api/superadmin/companies");
       if (response.IsSuccessStatusCode)
     {
     var json = await response.Content.ReadAsStringAsync();
          Companies = JsonSerializer.Deserialize<List<CompanyItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
       }
    }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin companies"); }

  return Page();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync(string companyId, bool isActive)
        {
    if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

var client = _httpClientFactory.CreateClient("PayRexApi");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

  var newActive = !isActive;
  var body = new StringContent(JsonSerializer.Serialize(new { isActive = newActive }), Encoding.UTF8, "application/json");

   var response = await client.PostAsync($"api/superadmin/companies/{companyId}/status", body);
  StatusMessage = response.IsSuccessStatusCode
    ? $"Company {(newActive ? "activated" : "deactivated")} successfully. Associated users were also updated."
          : "Failed to update company status";

     return RedirectToPage();
        }

   public class CompanyItem
        {
       public string CompanyId { get; set; } = "";
     public string CompanyName { get; set; } = "";
   public string Status { get; set; } = "";
    public bool IsActive { get; set; }
        public string PlanName { get; set; } = "";
       public int UserCount { get; set; }
   public int EmployeeCount { get; set; }
 public DateTime CreatedAt { get; set; }
   }
    }
}
