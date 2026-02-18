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

            // reCAPTCHA server-side verification (only if secret configured)
            try
            {
                var secret = _config["Recaptcha:SecretKey"];
                if (!string.IsNullOrWhiteSpace(secret))
                {
                    var token = Request.Form["g-recaptcha-response"].ToString();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        ErrorMessage = "Please complete the CAPTCHA.";
                        return Page();
                    }

                    var ok = await VerifyRecaptchaAsync(token, secret);
                    if (!ok)
                    {
                        ErrorMessage = "CAPTCHA validation failed. Please try again.";
                        _logger.LogWarning("reCAPTCHA validation failed for forgot-password request: {Email}", Input.Email);
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying reCAPTCHA for forgot password");
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

        private async Task<bool> VerifyRecaptchaAsync(string token, string secret)
        {
            try
            {
                using var client = new HttpClient();
                var values = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "secret", secret },
                    { "response", token }
                };

                var content = new FormUrlEncodedContent(values);
                var resp = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                if (!resp.IsSuccessStatusCode) return false;

                using var stream = await resp.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                if (doc.RootElement.TryGetProperty("success", out var success))
                {
                    return success.GetBoolean();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "reCAPTCHA verification error");
            }

            return false;
        }
    }
}
