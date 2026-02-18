using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.DTOs;
using PayRex.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text.Json;

namespace PayRex.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IAuthApiService _authApiService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _config;

        public LoginModel(IAuthApiService authApiService, ILogger<LoginModel> logger, IConfiguration config)
        {
            _authApiService = authApiService;
            _logger = logger;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public TotpInputModel TotpInput { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? TotpErrorMessage { get; set; }
        public string? ReturnUrl { get; set; }
        public bool RequireTotp { get; set; } = false;
        public string? TotpEmail { get; set; }
        public bool IsLockedOut { get; set; } = false;
        public int LockoutRemainingSeconds { get; set; } =0;

        public class InputModel
        {
            [Required(ErrorMessage = "Email or username is required")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public class TotpInputModel
        {
            [Required(ErrorMessage = "TOTP code is required")]
            [StringLength(6, MinimumLength =6, ErrorMessage = "TOTP code must be6 digits")]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "TOTP code must be6 digits")]
            public string TotpCode { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");

            // If already authenticated, redirect to dashboard
            if (Request.Cookies.ContainsKey("PayRex.AuthToken"))
            {
                Response.Redirect(ReturnUrl);
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");

            // Validate required login input explicitly so Totp modelstate doesn't interfere
            if (string.IsNullOrWhiteSpace(Input.Email))
            {
                ErrorMessage = "Email or username is required";
                TempData["ErrorMessage"] = ErrorMessage;
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.Password))
            {
                ErrorMessage = "Password is required";
                TempData["ErrorMessage"] = ErrorMessage;
                return Page();
            }

            // If reCAPTCHA secret configured, validate captcha response
            try
            {
                var secret = _config["Recaptcha:SecretKey"];
                if (!string.IsNullOrWhiteSpace(secret))
                {
                    var token = Request.Form["g-recaptcha-response"].ToString();
                    if (string.IsNullOrWhiteSpace(token))
                    {
                        ErrorMessage = "Please complete the CAPTCHA.";
                        TempData["ErrorMessage"] = ErrorMessage;
                        return Page();
                    }

                    var ok = await VerifyRecaptchaAsync(token, secret);
                    if (!ok)
                    {
                        ErrorMessage = "CAPTCHA validation failed. Please try again.";
                        TempData["ErrorMessage"] = ErrorMessage;
                        _logger.LogWarning("reCAPTCHA validation failed for {Email}", Input.Email);
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying reCAPTCHA");
            }

            // Call API to login
            var loginRequest = new LoginRequestDto
            {
                Email = Input.Email,
                Password = Input.Password
            };

            var response = await _authApiService.LoginAsync(loginRequest);

            if (response == null)
            {
                ErrorMessage = "Invalid email or password. Please try again.";
                TempData["ErrorMessage"] = ErrorMessage;
                _logger.LogWarning("Login failed for email: {Email} (null response)", Input.Email);
                return Page();
            }

            // Check if account is locked out (API returns this info)
            if (response.IsLockedOut)
            {
                IsLockedOut = true;
                LockoutRemainingSeconds = response.LockoutRemainingSeconds;
                // Surface lockout message as well for clarity
                ErrorMessage = string.IsNullOrEmpty(response.Message) ? "Account locked. Please try again later." : response.Message;
                TempData["ErrorMessage"] = ErrorMessage;
                return Page();
            }

            // Credentials OK; if API signals TOTP required, show modal and stop here.
            if (response.RequireTotp)
            {
                RequireTotp = true;
                TotpEmail = response.Email ?? Input.Email;
                // Don't set ErrorMessage for TOTP prompt - it's not an error
                return Page();
            }

            // If API returned no token but provided a message, show it instead of attempting to set cookie
            if (string.IsNullOrEmpty(response.Token))
            {
                ErrorMessage = string.IsNullOrEmpty(response.Message) ? "Invalid email or password. Please try again." : response.Message;
                TempData["ErrorMessage"] = ErrorMessage;
                _logger.LogWarning("Login failed for email: {Email} - {Message}", Input.Email, response.Message);
                _logger.LogDebug("Setting ErrorMessage and returning Page() with ErrorMessage={Error}", ErrorMessage);
                return Page();
            }

            // Login successful - store JWT token
            return SetAuthTokenAndRedirect(response);
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

        public async Task<IActionResult> OnPostTotpAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/Dashboard");

            // When posting TOTP we may not have Input.Email/Password in the form.
            // Remove their validation entries so ModelState won't be invalid.
            if (ModelState.ContainsKey("Input.Email")) ModelState.Remove("Input.Email");
            if (ModelState.ContainsKey("Input.Password")) ModelState.Remove("Input.Password");

            var totpEmail = Request.Form["TotpEmail"].ToString();

            // Validate TOTP input
            if (string.IsNullOrEmpty(TotpInput.TotpCode) || !System.Text.RegularExpressions.Regex.IsMatch(TotpInput.TotpCode, @"^\d{6}$"))
            {
                // Prevent the ModelState errors from surfacing in the global login validation summary
                if (ModelState.ContainsKey("TotpInput.TotpCode"))
                {
                    ModelState.Remove("TotpInput.TotpCode");
                }

                RequireTotp = true;
                TotpEmail = totpEmail;
                TotpErrorMessage = "Please enter a valid6-digit code.";
                return Page();
            }

            var totpRequest = new TotpVerificationDto
            {
                Email = totpEmail,
                TotpCode = TotpInput.TotpCode
            };

            var response = await _authApiService.VerifyTotpAsync(totpRequest);

            // API may return an object containing a Message (error) but no Token. Treat that as a failure.
            if (response == null || string.IsNullOrEmpty(response.Token))
            {
                RequireTotp = true;
                TotpEmail = totpEmail;
                TotpErrorMessage = response == null || string.IsNullOrEmpty(response.Message)
                    ? "Invalid TOTP code. Please try again."
                    : response.Message;

                _logger.LogWarning("TOTP verification failed for email: {Email} - {Message}", totpEmail, response?.Message);
                return Page();
            }

            _logger.LogInformation("User {Email} logged in successfully with TOTP", totpEmail);

            // TOTP verification successful - store JWT token
            return SetAuthTokenAndRedirect(response);
        }

        // Handler for when the user cancels TOTP verification (modal close)
        public IActionResult OnPostCancelTotp()
        {
            // Redirect back to the login page to clear modal state
            return RedirectToPage("/Auth/Login");
        }

        private IActionResult SetAuthTokenAndRedirect(LoginResponseDto response)
        {
            // Store JWT in HTTP-only cookie first (needed for ChangePassword page auth)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                // Allow insecure cookies on localhost during development when TLS may not be used.
                // Use secure cookies when request is HTTPS or host is not localhost.
                Secure = Request.IsHttps || (Request.Host.Host?.Contains("localhost") ?? false),
                SameSite = SameSiteMode.Lax, // allow top-level navigations in development
                Path = "/" // Ensure cookie is sent for all paths
            };

            // Safely set cookie expiration
            if (response.ExpiresAt != default && response.ExpiresAt > DateTime.MinValue)
            {
                try
                {
                    var expiresUtc = response.ExpiresAt.Kind == DateTimeKind.Utc
                        ? response.ExpiresAt
                        : response.ExpiresAt.ToUniversalTime();

                    cookieOptions.Expires = new DateTimeOffset(expiresUtc);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to set cookie expiration from response, using default60 minutes");
                    cookieOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(60);
                }
            }
            else
            {
                cookieOptions.Expires = DateTimeOffset.UtcNow.AddMinutes(60);
            }

            Response.Cookies.Append("PayRex.AuthToken", response.Token, cookieOptions);

            // Decide destination based on role - each role has its own dashboard
            try
            {
                ReturnUrl = response.Role?.ToLowerInvariant() switch
                {
                    "superadmin" => Url.Content("~/Admin/Dashboard"),
                    "admin" => Url.Content("~/Dashboard"),
                    "hr" => Url.Content("~/Dashboard"),
                    "employee" => Url.Content("~/Dashboard"),
                    _ => Url.Content("~/Dashboard")
                };
            }
            catch
            {
                // Fallback
                ReturnUrl = Url.Content("~/Dashboard");
            }

            return LocalRedirect(ReturnUrl!);
        }
    }
}
