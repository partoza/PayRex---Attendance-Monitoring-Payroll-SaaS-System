namespace PayRex.Web.Pages.Auth
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Configuration;
    using PayRex.Web.DTOs;
    using PayRex.Web.Services;
    using System.ComponentModel.DataAnnotations;
    using System.Net.Http;
    using System.Text.Json;

    /// <summary>
    /// Defines the <see cref="RegisterModel" />
    /// </summary>
    public class RegisterModel : PageModel
    {
        /// <summary>
        /// Defines the _authApiService
        /// </summary>
        private readonly IAuthApiService _authApiService;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<RegisterModel> _logger;

        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterModel"/> class.
        /// </summary>
        /// <param name="authApiService">The authApiService<see cref="IAuthApiService"/></param>
        /// <param name="logger">The logger<see cref="ILogger{RegisterModel}"/></param>
        /// <param name="config">The config<see cref="IConfiguration"/></param>
        public RegisterModel(IAuthApiService authApiService, ILogger<RegisterModel> logger, IConfiguration config)
        {
            _authApiService = authApiService;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Gets or sets the Input
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

        /// <summary>
        /// Gets or sets the ErrorMessage
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the SuccessMessage
        /// </summary>
        public string? SuccessMessage { get; set; }

        /// <summary>
        /// Defines the <see cref="InputModel" />
        /// </summary>
        public class InputModel
        {
            /// <summary>
            /// Gets or sets the FirstName
            /// </summary>
            [Required(ErrorMessage = "First name is required")]
            [MaxLength(100, ErrorMessage = "First name cannot exceed100 characters")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the LastName
            /// </summary>
            [Required(ErrorMessage = "Last name is required")]
            [MaxLength(100, ErrorMessage = "Last name cannot exceed100 characters")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Email
            /// </summary>
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Password
            /// </summary>
            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            [MinLength(12, ErrorMessage = "Password must be at least 12 characters")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{12,}$",
 ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the ConfirmPassword
            /// </summary>
            [Required(ErrorMessage = "Please confirm your password")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Passwords do not match")]
            [Display(Name = "Confirm Password")]
            public string ConfirmPassword { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets a value indicating whether AcceptTerms
            /// </summary>
            [Required(ErrorMessage = "You must accept the terms and conditions")]
            [Display(Name = "I agree to the Terms and Conditions")]
            public bool AcceptTerms { get; set; }
        }

        /// <summary>
        /// The OnGet
        /// </summary>
        public void OnGet()
        {
            // If already authenticated, redirect to dashboard
            if (Request.Cookies.ContainsKey("PayRex.AuthToken"))
            {
                Response.Redirect("/Dashboard");
            }

            // Removed hCaptcha site key usage
        }

        /// <summary>
        /// The OnPostAsync
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!Input.AcceptTerms)
            {
                ErrorMessage = "You must accept the terms and conditions to register.";
                // Attach the error to the specific field so the validation message appears next to the checkbox
                ModelState.AddModelError("Input.AcceptTerms", ErrorMessage);
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
                        ModelState.AddModelError(string.Empty, ErrorMessage);
                        return Page();
                    }

                    var ok = await VerifyRecaptchaAsync(token, secret);
                    if (!ok)
                    {
                        ErrorMessage = "CAPTCHA validation failed. Please try again.";
                        ModelState.AddModelError(string.Empty, ErrorMessage);
                        _logger.LogWarning("reCAPTCHA validation failed during registration for {Email}", Input.Email);
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error verifying reCAPTCHA during registration");
            }

            var registerRequest = new RegisterRequestDto
            {
                FirstName = Input.FirstName,
                LastName = Input.LastName,
                Email = Input.Email,
                Password = Input.Password,
                ConfirmPassword = Input.ConfirmPassword
            };

            var response = await _authApiService.RegisterAsync(registerRequest);

            if (response == null)
            {
                ErrorMessage = "Registration failed. Email may already be in use.";
                _logger.LogWarning("Registration failed for email: {Email}", Input.Email);
                return Page();
            }

            _logger.LogInformation("User {Email} registered successfully with ID: {Id}", response.Email, response.Id);

            TempData["Auth_SuccessMessage"] = "Registration successful! Please log in with your credentials.";
            return RedirectToPage("/Auth/Login");
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