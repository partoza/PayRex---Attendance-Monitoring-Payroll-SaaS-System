using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class SettingsModel : PageModel
    {
      private readonly IHttpClientFactory _httpClientFactory;
  private readonly ILogger<SettingsModel> _logger;

    [BindProperty] public decimal SssPercentage { get; set; }
 [BindProperty] public decimal PagIbigPercentage { get; set; }
     [BindProperty] public decimal PhilHealthPercentage { get; set; }
     [BindProperty, DataType(DataType.Date)] public DateTime EffectiveDate { get; set; }
  [BindProperty] public string? Note { get; set; }

 // New: require current user's password to confirm changes
 [BindProperty, DataType(DataType.Password)]
 [Required(ErrorMessage = "Password is required to confirm changes")]
 public string? ConfirmPassword { get; set; }

 [TempData] public string? StatusMessage { get; set; }
        public bool LoadSuccess { get; set; }

        public SettingsModel(IHttpClientFactory httpClientFactory, ILogger<SettingsModel> logger)
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
    var response = await client.GetAsync("api/superadmin/settings");
    if (response.IsSuccessStatusCode)
         {
  var json = await response.Content.ReadAsStringAsync();
 var dto = JsonSerializer.Deserialize<SettingDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
      if (dto != null)
  {
 SssPercentage = dto.SssPercentage;
    PagIbigPercentage = dto.PagIbigPercentage;
PhilHealthPercentage = dto.PhilHealthPercentage;
     EffectiveDate = dto.EffectiveDate;
   Note = dto.Note;
    LoadSuccess = true;
 }
 }
            }
     catch (Exception ex) { _logger.LogError(ex, "Error loading system settings"); }

  return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
   if (!ModelState.IsValid)
   {
 StatusMessage = "Please provide required information.";
 return Page();
 }

   if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

  var client = _httpClientFactory.CreateClient("PayRexApi");
 client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

     var payload = new
   {
   sssPercentage = SssPercentage,
 pagIbigPercentage = PagIbigPercentage,
    philHealthPercentage = PhilHealthPercentage,
      effectiveDate = EffectiveDate,
     note = Note,
 password = ConfirmPassword
    };

  var body = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
     var response = await client.PutAsync("api/superadmin/settings", body);
            StatusMessage = response.IsSuccessStatusCode ? "System settings updated successfully" : "Failed to update system settings. Ensure your password is correct.";

 return RedirectToPage();
  }

   public class SettingDto
        {
     public int SettingId { get; set; }
  public decimal SssPercentage { get; set; }
     public decimal PagIbigPercentage { get; set; }
  public decimal PhilHealthPercentage { get; set; }
  public DateTime EffectiveDate { get; set; }
            public string? Note { get; set; }
  }
    }
}
