using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages.Auth
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChangePasswordModel> _logger;

     public ChangePasswordModel(IHttpClientFactory httpClientFactory, ILogger<ChangePasswordModel> logger)
     {
      _httpClientFactory = httpClientFactory;
   _logger = logger;
     }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool IsForced { get; set; }

        public class InputModel
        {
 [Required(ErrorMessage = "Current password is required")]
      [DataType(DataType.Password)]
     public string CurrentPassword { get; set; } = string.Empty;

[Required(ErrorMessage = "New password is required")]
   [DataType(DataType.Password)]
[MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{8,}$",
                ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
  public string NewPassword { get; set; } = string.Empty;

  [Required(ErrorMessage = "Please confirm your new password")]
       [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet(bool forced = false)
  {
            if (!Request.Cookies.ContainsKey("PayRex.AuthToken"))
            {
                return RedirectToPage("/Auth/Login");
    }

            IsForced = forced;
    return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
       if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
            {
return RedirectToPage("/Auth/Login");
  }

    if (!ModelState.IsValid)
          {
    return Page();
      }

  var client = _httpClientFactory.CreateClient("PayRexApi");
  client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

   var payload = new
     {
                CurrentPassword = Input.CurrentPassword,
                NewPassword = Input.NewPassword,
                ConfirmPassword = Input.ConfirmPassword
     };

var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

      try
            {
                var response = await client.PostAsync("api/auth/change-password", content);

        if (response.IsSuccessStatusCode)
    {
             // Password changed successfully - clear cookie and redirect to login
Response.Cookies.Delete("PayRex.AuthToken", new CookieOptions { Path = "/" });
 TempData["SuccessMessage"] = "Password changed successfully. Please login with your new password.";
  return RedirectToPage("/Auth/Login");
}

       var errorJson = await response.Content.ReadAsStringAsync();
    try
          {
      var errorObj = JsonSerializer.Deserialize<JsonElement>(errorJson);
         ErrorMessage = errorObj.TryGetProperty("message", out var msg) ? msg.GetString() : "Failed to change password.";
 }
         catch
         {
                    ErrorMessage = "Failed to change password. Please try again.";
         }
            }
            catch (Exception ex)
            {
         _logger.LogError(ex, "Error changing password");
        ErrorMessage = "An error occurred. Please try again.";
        }

     return Page();
      }
    }
}
