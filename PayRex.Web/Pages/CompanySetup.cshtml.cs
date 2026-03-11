using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.DTOs;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class CompanySetupModel : PageModel
    {
        private readonly IAuthApiService _auth;
        private readonly ILogger<CompanySetupModel> _logger;

        [BindProperty] public string CompanyName { get; set; } = "";
        [BindProperty] public string? Tin { get; set; }
        [BindProperty] public string? Address { get; set; }
        [BindProperty] public string? ContactPhone { get; set; }

        public string? ErrorMessage { get; set; }

        public CompanySetupModel(IAuthApiService auth, ILogger<CompanySetupModel> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // If the JWT already says setup is done, skip this page
            var isSetupComplete = User.FindFirst("isSetupComplete")?.Value;
            if (string.Equals(isSetupComplete, "true", StringComparison.OrdinalIgnoreCase))
                return RedirectToPage("/Dashboard");

            var token = Request.Cookies["PayRex.AuthToken"] ?? "";
            var profile = await _auth.GetCompanyProfileAsync(token);
            if (profile != null)
            {
                CompanyName = profile.CompanyName;
                Tin = profile.Tin;
                Address = profile.Address;
                ContactPhone = profile.ContactPhone;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                ErrorMessage = "Company name is required.";
                return Page();
            }

            var token = Request.Cookies["PayRex.AuthToken"] ?? "";

            // Upload logo if provided
            var logoFile = Request.Form.Files["logoFile"];
            if (logoFile != null && logoFile.Length > 0)
            {
                using var stream = logoFile.OpenReadStream();
                await _auth.UploadCompanyLogoAsync(token, stream, logoFile.FileName, logoFile.ContentType);
            }

            // Owner signature is required — upload it and capture URL
            var signatureFile = Request.Form.Files["signatureFile"];
            if (signatureFile == null || signatureFile.Length == 0)
            {
                ErrorMessage = "Owner signature is required. Please upload a signature image.";
                return Page();
            }

            string? signatureUrl = null;
            using (var sigStream = signatureFile.OpenReadStream())
            {
                var sigResult = await _auth.UploadOwnerSignatureAsync(token, sigStream, signatureFile.FileName, signatureFile.ContentType);
                if (sigResult == null || !sigResult.Success)
                {
                    ErrorMessage = "Failed to upload owner signature. Please try again.";
                    return Page();
                }
                signatureUrl = sigResult.ImageUrl;
            }

            var dto = new UpdateCompanyRequestDto
            {
                CompanyName = CompanyName.Trim(),
                Tin = Tin?.Trim(),
                Address = Address?.Trim(),
                ContactPhone = ContactPhone?.Trim(),
                OwnerSignatureUrl = signatureUrl
            };

            var (ok, msg) = await _auth.UpdateCompanyProfileAsync(token, dto);
            if (!ok)
            {
                ErrorMessage = msg ?? "Failed to save company information. Please try again.";
                return Page();
            }

            // Refresh JWT so isSetupComplete = true is reflected immediately
            var newToken = await _auth.RefreshTokenAsync(token);
            if (!string.IsNullOrEmpty(newToken))
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps || (Request.Host.Host?.Contains("localhost") ?? false),
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                };
                try
                {
                    var host = Request.Host.Host ?? string.Empty;
                    if (!host.Contains("localhost") && host.Contains("runasp.net"))
                    {
                        cookieOptions.Domain = ".runasp.net";
                        cookieOptions.Secure = true;
                    }
                }
                catch { }
                Response.Cookies.Append("PayRex.AuthToken", newToken, cookieOptions);
            }

            return RedirectToPage("/Dashboard");
        }
    }
}
