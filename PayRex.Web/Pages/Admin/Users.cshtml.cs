using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Admin
{
    [Authorize(Roles = "SuperAdmin")]
    public class UsersModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
   private readonly ILogger<UsersModel> _logger;

    public List<UserItem> Users { get; set; } = new();
        [TempData] public string? StatusMessage { get; set; }

        public UsersModel(IHttpClientFactory httpClientFactory, ILogger<UsersModel> logger)
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
    var response = await client.GetAsync("api/superadmin/users");
   if (response.IsSuccessStatusCode)
       {
    var json = await response.Content.ReadAsStringAsync();
        Users = JsonSerializer.Deserialize<List<UserItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
   }
  }
            catch (Exception ex) { _logger.LogError(ex, "Error loading admin users"); }
  return Page();
 }

        public async Task<IActionResult> OnPostToggleStatusAsync(int userId, string currentStatus)
        {
if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token)) return RedirectToPage("/Auth/Login");

   var client = _httpClientFactory.CreateClient("PayRexApi");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

  var newStatus = currentStatus == "Active" ? "Suspended" : "Active";
       var body = new StringContent(JsonSerializer.Serialize(new { status = newStatus }), Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"api/superadmin/users/{userId}/status", body);
            StatusMessage = response.IsSuccessStatusCode ? $"User status changed to {newStatus}" : "Failed to update user status";

       return RedirectToPage();
        }

            public class UserItem
          {
               public int UserId { get; set; }
               public string FirstName { get; set; } = "";
         public string LastName { get; set; } = "";
                public string Email { get; set; } = "";
            public string Role { get; set; } = "";
                public string Status { get; set; } = "";
            public int CompanyId { get; set; }
         public string? CompanyName { get; set; }
           public DateTime CreatedAt { get; set; }
            }
    }
}
