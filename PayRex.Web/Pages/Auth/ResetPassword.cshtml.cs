using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using PayRex.Web.DTOs;

namespace PayRex.Web.Pages.Auth
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ResetPasswordModel> _logger;
        private readonly HttpClient _httpClient;

        public ResetPasswordModel(IConfiguration config, ILogger<ResetPasswordModel> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("PayRexApi");
        }

        [BindProperty(SupportsGet = true)]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.EmailAddress)]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Token { get; set; } = string.Empty;

            [Required(ErrorMessage = "New password is required")]
            [MinLength(8, ErrorMessage = "Password must be at least8 characters")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
     ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
            [DataType(DataType.Password)]
          [Display(Name = "New Password")]
 public string NewPassword { get; set; } = string.Empty;

   [Required(ErrorMessage = "Please confirm your password")]
          [DataType(DataType.Password)]
         [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
  [Display(Name = "Confirm New Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
    }

        public IActionResult OnGet()
        {
            // If already authenticated, redirect to dashboard
          if (Request.Cookies.ContainsKey("PayRex.AuthToken"))
            {
                return Redirect("/Dashboard");
            }

            // Validate required query parameters
  if (string.IsNullOrEmpty(Input.Email) || string.IsNullOrEmpty(Input.Token))
            {
ErrorMessage = "Invalid password reset link. Please request a new one.";
   }

            return Page();
      }

     public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
 {
          return Page();
            }

            // Validate required parameters
            if (string.IsNullOrEmpty(Input.Email) || string.IsNullOrEmpty(Input.Token))
            {
       ErrorMessage = "Invalid password reset link. Please request a new one.";
          return Page();
        }

            // Call API to reset password using named client
            var requestDto = new ResetPasswordRequestDto
 {
         Email = Input.Email,
   Token = Input.Token,
        NewPassword = Input.NewPassword,
     ConfirmPassword = Input.ConfirmPassword
            };

      try
         {
  var requestJson = JsonSerializer.Serialize(requestDto);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"api/auth/reset-password", httpContent);

      if (response.IsSuccessStatusCode)
     {
         SuccessMessage = "Password reset successfully! Redirecting to login...";
            _logger.LogInformation("Password reset successful for email: {Email}", Input.Email);

        // Redirect to login after 3 seconds
     Response.Headers.Append("Refresh", "3; url=/Auth/Login");
         }
          else
  {
    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Password reset failed: {Error}", errorContent);

    try
     {
       using var doc = JsonDocument.Parse(errorContent);
   var message = doc.RootElement.GetProperty("message").GetString();
        ErrorMessage = message ?? "Password reset failed. Please try again.";
   }
           catch
          {
         ErrorMessage = "Password reset failed. The link may have expired. Please request a new one.";
         }
    }
  }
       catch (Exception ex)
          {
    _logger.LogError(ex, "Error calling reset password API");
         ErrorMessage = "An error occurred. Please try again later.";
       }

  return Page();
     }
    }
}
