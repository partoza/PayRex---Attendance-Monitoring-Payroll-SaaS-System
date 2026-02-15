using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;
using PayRex.Web.DTOs;

namespace PayRex.Web.Pages.Auth
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly HttpClient _httpClient;

        public ForgotPasswordModel(IConfiguration config, ILogger<ForgotPasswordModel> logger, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _logger = logger;
            // Use named client that has BaseAddress configured in Program.cs
            _httpClient = httpClientFactory.CreateClient("PayRexApi");
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet()
        {
            // If already authenticated, redirect to dashboard
            if (Request.Cookies.ContainsKey("PayRex.AuthToken"))
            {
                Response.Redirect("/Dashboard");
            }

            // Removed hCaptcha site key usage
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Call API to send reset email using named client
            var requestDto = new ForgotPasswordRequestDto { Email = Input.Email };

            try
            {
                var requestJson = JsonSerializer.Serialize(requestDto);
                var httpContent = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"api/auth/forgot-password", httpContent);

                if (response.IsSuccessStatusCode)
                {
                    SuccessMessage = "If the email exists in our system, a password reset link has been sent.";
                    _logger.LogInformation("Password reset requested for email: {Email}", Input.Email);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Forgot password request failed: {Error}", errorContent);
                    ErrorMessage = "An error occurred. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling forgot password API");
                ErrorMessage = "An error occurred. Please try again later.";
            }

            return Page();
        }
    }
}
