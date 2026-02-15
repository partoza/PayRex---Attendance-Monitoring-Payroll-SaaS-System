using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using PayRex.Web.Services;
using PayRex.Web.DTOs;

namespace PayRex.Web.Pages
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly IAuthApiService _authApi;

        public ProfileModel(IAuthApiService authApi)
        {
            _authApi = authApi;
        }

        public string FirstName { get; set; } = "Juan";
        public string LastName { get; set; } = "Cruz";
        public string Email { get; set; } = "juan.cruz@company.com";
        public string Role { get; set; } = "Admin";
        public string? ProfileImageUrl { get; set; }
        public DateTime? LastPasswordChangeAt { get; set; } = DateTime.Now.AddMonths(-2);

        [BindProperty(SupportsGet = true)]
        public bool IsEditMode { get; set; }

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ToastType { get; set; }

        [BindProperty]
        public PasswordChangeInput PasswordInput { get; set; } = new();

        public bool IsTwoFactorEnabled { get; set; } = false;
        public string? TotpQrCodeUri { get; set; }
        public string? TotpManualEntryKey { get; set; }
        public bool ShowRecoveryCodes { get; set; }
        public List<string>? RecoveryCodes { get; set; }

        [BindProperty]
        public string? TotpCode { get; set; }

        // Allow binding of file on post
        [BindProperty]
        public IFormFile? File { get; set; }

        private string? GetAuthToken()
        {
            if (Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) && !string.IsNullOrEmpty(t))
                return t;
            return null;
        }

        public async Task OnGetAsync([FromQuery(Name = "edit")] bool edit = false)
        {
            IsEditMode = edit;

            // Load profile from API
            var token = GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                var profile = await _authApi.GetUserProfileAsync(token);
                if (profile != null)
                {
                    FirstName = profile.FirstName ?? FirstName;
                    LastName = profile.LastName ?? LastName;
                    Email = profile.Email ?? Email;
                    Role = profile.Role ?? Role;
                    ProfileImageUrl = profile.ProfileImageUrl ?? ProfileImageUrl;
                    LastPasswordChangeAt = profile.LastPasswordChangeAt ?? LastPasswordChangeAt;
                    // prefer server value for2FA
                    IsTwoFactorEnabled = profile.IsTwoFactorEnabled;
                }

                var two = await _authApi.GetTwoFactorStatusAsync(token);
                if (two != null)
                {
                    // OR with status endpoint
                    IsTwoFactorEnabled = IsTwoFactorEnabled || two.IsEnabled;
                }
                
                // If profile is null or incomplete, still try to populate from claims below
            }

            // Fallback to claims if some values still default or token was absent
            if (string.IsNullOrEmpty(GetAuthToken()))
            {
                // If no auth token in cookie, it's possible the user is authenticated by server-side JWT middleware.
                // Prefer JWT registered claim names (these are used by the API when issuing tokens).
                if (User.Identity?.IsAuthenticated == true)
                {
                    var given = User.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value;
                    var family = User.FindFirst(JwtRegisteredClaimNames.FamilyName)?.Value;
                    var emailClaim = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                    if (!string.IsNullOrEmpty(given)) FirstName = given;
                    if (!string.IsNullOrEmpty(family)) LastName = family;
                    if (!string.IsNullOrEmpty(emailClaim)) Email = emailClaim;

                    // Also try legacy claim types as a fallback
                    if (string.IsNullOrEmpty(given)) FirstName = User.FindFirstValue(ClaimTypes.GivenName) ?? FirstName;
                    if (string.IsNullOrEmpty(family)) LastName = User.FindFirstValue(ClaimTypes.Surname) ?? LastName;
                    if (string.IsNullOrEmpty(emailClaim)) Email = User.FindFirstValue(ClaimTypes.Email) ?? Email;

                    Role = User.FindFirstValue(ClaimTypes.Role) ?? Role;
                }
            }
        }

        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            IsEditMode = true; // stay in edit mode after upload
            var token = GetAuthToken();
            if (File == null || File.Length == 0)
            {
                ErrorMessage = "No file selected.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            // Validate file size (max 2MB)
            const long MaxBytes = 2 * 1024 * 1024;
            if (File.Length > MaxBytes)
            {
                ErrorMessage = "File too large. Max size is 2MB.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(File.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ErrorMessage = "Invalid file type. Allowed: JPG, PNG, WEBP.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Not authenticated.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            try
            {
                await using var ms = new MemoryStream();
                await File.CopyToAsync(ms);
                ms.Position = 0;
                var resp = await _authApi.UploadProfileImageAsync(token, ms, File.FileName, File.ContentType);
                if (resp != null && resp.Success)
                {
                    // Use ImageUrl property returned by API DTO
                    ProfileImageUrl = resp.ImageUrl ?? ProfileImageUrl;
                    SuccessMessage = resp.Message ?? "Profile image updated successfully.";
                    ToastType = "success";
                }
                else
                {
                    ErrorMessage = resp?.Message ?? "Error saving file.";
                    ToastType = "error";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error saving file.";
                ToastType = "error";
            }

            await OnGetAsync(true);
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveImageAsync()
        {
            IsEditMode = true; // keep editing after remove
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Not authenticated.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            var (success, message) = await _authApi.RemoveProfileImageAsync(token);
            if (success)
            {
                ProfileImageUrl = null;
                SuccessMessage = message ?? "Profile image removed.";
                ToastType = "success";
            }
            else
            {
                ErrorMessage = message ?? "Failed to remove profile image.";
                ToastType = "error";
            }

            await OnGetAsync(true);
            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            IsEditMode = true;
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Please correct the errors in the form.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToPage("/Auth/Login");
            }

            var dto = new ChangePasswordRequestDto
            {
                CurrentPassword = PasswordInput.CurrentPassword,
                NewPassword = PasswordInput.NewPassword,
                ConfirmPassword = PasswordInput.ConfirmPassword
            };

            var (success, message, requireRelogin) = await _authApi.ChangePasswordAsync(token, dto);
            if (success)
            {
                if (requireRelogin)
                {
                    // remove cookie and redirect to login
                    Response.Cookies.Delete("PayRex.AuthToken", new CookieOptions { Path = "/" });
                    TempData["SuccessMessage"] = message ?? "Password changed. Please login again.";
                    return RedirectToPage("/Auth/Login");
                }

                SuccessMessage = message ?? "Password changed successfully.";
                ToastType = "success";
                IsEditMode = false;
            }
            else
            {
                ErrorMessage = message ?? "Failed to change password.";
                ToastType = "error";
                IsEditMode = true;
            }

            await OnGetAsync(IsEditMode);
            return Page();
        }

        public async Task<IActionResult> OnPostSetupTotpAsync()
        {
            IsEditMode = true;
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Not authenticated.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            var resp = await _authApi.SetupTotpAsync(token);
            if (resp != null && resp.Success)
            {
                TotpQrCodeUri = resp.QrCodeUri;
                TotpManualEntryKey = resp.ManualEntryKey;
                SuccessMessage = "Scan the QR code to configure your authenticator app.";
                ToastType = "success";
            }
            else
            {
                ErrorMessage = resp?.Message ?? "Failed to start TOTP setup.";
                ToastType = "error";
            }

            await OnGetAsync(true);
            return Page();
        }

        public async Task<IActionResult> OnPostEnableTotpAsync()
        {
            IsEditMode = true;
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Not authenticated.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            // Build request using correct DTO property name
            var requestDto = new EnableTotpRequestDto { TotpCode = TotpCode ?? string.Empty };
            var resp = await _authApi.EnableTotpAsync(token, requestDto);
            if (resp != null && resp.Success)
            {
                IsTwoFactorEnabled = true;
                RecoveryCodes = resp.RecoveryCodes ?? new List<string>();
                ShowRecoveryCodes = RecoveryCodes.Any();
                SuccessMessage = resp.Message ?? "2FA enabled successfully.";
                ToastType = "success";
                IsEditMode = false; // finish setup
            }
            else
            {
                ErrorMessage = resp?.Message ?? "Failed to enable 2FA.";
                ToastType = "error";
                IsEditMode = true;
            }

            await OnGetAsync(IsEditMode);
            return Page();
        }

        public async Task<IActionResult> OnPostDisableTotpAsync()
        {
            IsEditMode = true;
            var token = GetAuthToken();
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Not authenticated.";
                ToastType = "error";
                await OnGetAsync(true);
                return Page();
            }

            var (success, message) = await _authApi.DisableTotpAsync(token);
            if (success)
            {
                IsTwoFactorEnabled = false;
                SuccessMessage = message ?? "2FA has been disabled.";
                ToastType = "warning";
                IsEditMode = false;
            }
            else
            {
                ErrorMessage = message ?? "Failed to disable 2FA.";
                ToastType = "error";
                IsEditMode = true;
            }

            await OnGetAsync(IsEditMode);
            return Page();
        }

        public class PasswordChangeInput
        {
            [Required, DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = "";

            [Required, MinLength(8), DataType(DataType.Password)]
            public string NewPassword { get; set; } = "";

            [Required, Compare("NewPassword"), DataType(DataType.Password)]
            public string ConfirmPassword { get; set; } = "";
        }
    }
}
