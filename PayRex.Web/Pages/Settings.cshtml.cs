using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.DTOs;
using PayRex.Web.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class SettingsModel : PageModel
    {
        private readonly IAuthApiService _authApi;
        public SettingsModel(IAuthApiService authApi) { _authApi = authApi; }

        [BindProperty(SupportsGet = true)] public bool IsEditMode { get; set; }
        [TempData(Key = "Settings_StatusMessage")] public string? StatusMessage { get; set; }
        [TempData(Key = "Settings_SuccessMessage")] public string? SuccessMessage { get; set; }
        [TempData(Key = "Settings_ErrorMessage")] public string? ErrorMessage { get; set; }

        [BindProperty]
        public CompanyProfileDto Profile { get; set; } = new();

        [BindProperty]
        public List<RoleConfig> Roles { get; set; } = new();

        public Task OnGetAsync([FromQuery(Name = "edit")] bool edit = false)
        {
            // Render page immediately without awaiting external API calls to avoid long blocking loads.
            IsEditMode = edit;
            Profile = new CompanyProfileDto();
            Roles = new List<RoleConfig>();
            return Task.CompletedTask;
        }

        // Return company profile as JSON for client-side fetch (keeps initial page render fast)
        public async Task<IActionResult> OnGetCompanyProfileJsonAsync()
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) return Unauthorized();
            var cp = await _authApi.GetCompanyProfileAsync(token);
            if (cp == null) return NotFound();
            return new JsonResult(cp);
        }

        // Return employee roles as JSON for client-side fetch
        public async Task<IActionResult> OnGetEmployeeRolesJsonAsync()
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) return Unauthorized();
            try
            {
                var apiRoles = await _authApi.GetEmployeeRolesAsync(token);
                if (apiRoles != null)
                {
                    var mapped = apiRoles.Select(r => new RoleConfig
                    {
                        RoleId = r.RoleId,
                        RoleName = r.RoleName ?? string.Empty,
                        BasicRate = r.BasicRate ?? 0m,
                        RateType = r.RateType ?? string.Empty,
                        Description = r.Description ?? string.Empty,
                        IsActive = true
                    }).ToList();
                    return new JsonResult(mapped);
                }
            }
            catch { }

            // Fallback to company profile RolesJson
            var cp = await _authApi.GetCompanyProfileAsync(token);
            if (cp != null && !string.IsNullOrEmpty(cp.RolesJson))
            {
                try { var list = JsonSerializer.Deserialize<List<RoleConfig>>(cp.RolesJson) ?? new List<RoleConfig>(); return new JsonResult(list); }
                catch { }
            }

            return new JsonResult(new List<RoleConfig>());
        }

        // Upload company logo (uses same API helper used by Profile page)
        [BindProperty]
        public IFormFile? CompanyLogoFile { get; set; }

        [BindProperty]
        public IFormFile? OwnerSignatureFile { get; set; }

        public async Task<IActionResult> OnPostUploadCompanyLogoAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (CompanyLogoFile == null || CompanyLogoFile.Length == 0)
            {
                StatusMessage = "No file selected.";
                return RedirectToPage(new { edit = true });
            }

            // Validate size and extension similar to Profile upload
            const long MaxBytes = 2 * 1024 * 1024;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(CompanyLogoFile.FileName).ToLowerInvariant();
            if (CompanyLogoFile.Length > MaxBytes || !allowed.Contains(ext))
            {
                StatusMessage = "Invalid file. Max 2MB, JPG/PNG/WEBP allowed.";
                return RedirectToPage(new { edit = true });
            }

            if (string.IsNullOrEmpty(token))
            {
                StatusMessage = "Not authenticated.";
                return RedirectToPage(new { edit = true });
            }

            try
            {
                await using var ms = new MemoryStream();
                await CompanyLogoFile.CopyToAsync(ms);
                ms.Position = 0;
                var resp = await _authApi.UploadCompanyLogoAsync(token, ms, CompanyLogoFile.FileName, CompanyLogoFile.ContentType);
                if (resp != null && resp.Success && !string.IsNullOrEmpty(resp.ImageUrl))
                {
                    // Preserve existing profile values when updating logo
                    var cp = await _authApi.GetCompanyProfileAsync(token);
                    if (cp == null)
                    {
                        StatusMessage = "Failed to load company profile before saving logo.";
                        return RedirectToPage(new { edit = true });
                    }
                    var dto = new UpdateCompanyRequestDto
                    {
                        CompanyName = cp.CompanyName ?? string.Empty,
                        Address = cp.Address,
                        ContactEmail = cp.ContactEmail,
                        ContactPhone = cp.ContactPhone,
                        Tin = cp.Tin,
                        LogoUrl = resp.ImageUrl,
                        OwnerSignatureUrl = cp.OwnerSignatureUrl,
                        PayrollCycle = cp.PayrollCycle,
                        WorkHoursPerDay = cp.WorkHoursPerDay,
                        OvertimeRate = cp.OvertimeRate,
                        LateGraceMinutes = cp.LateGraceMinutes,
                        HolidayRate = cp.HolidayRate,
                        ScheduledTimeIn = cp.ScheduledTimeIn,
                        ScheduledTimeOut = cp.ScheduledTimeOut,
                        TimeInCutoffHours = cp.TimeInCutoffHours,
                        VacationLeaveDays = cp.VacationLeaveDays,
                        VacationLeaveResetType = cp.VacationLeaveResetType,
                        RolesJson = cp.RolesJson
                    };

                    var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
                    if (success)
                    {
                        // Refresh JWT so nav logo updates immediately
                        var newToken = await _authApi.RefreshTokenAsync(token);
                        if (!string.IsNullOrEmpty(newToken))
                            Response.Cookies.Append("PayRex.AuthToken", newToken, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.None, Path = "/", Expires = DateTimeOffset.UtcNow.AddHours(8) });

                        SuccessMessage = message ?? "Company logo updated";
                    }
                    else ErrorMessage = message ?? "Failed to update logo";
                }
                else
                {
                    ErrorMessage = resp?.Message ?? "Failed to upload to cloud.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error uploading file: " + ex.Message;
            }

            return RedirectToPage(new { edit = true });
        }

        public async Task<IActionResult> OnPostRemoveCompanyLogoAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) { StatusMessage = "Not authenticated."; return RedirectToPage(new { edit = true }); }

            var cp = await _authApi.GetCompanyProfileAsync(token);
            if (cp == null)
            {
                ErrorMessage = "Failed to load company profile.";
                return RedirectToPage(new { edit = true });
            }
            var dto = new UpdateCompanyRequestDto
            {
                CompanyName = cp.CompanyName ?? string.Empty,
                Address = cp.Address,
                ContactEmail = cp.ContactEmail,
                ContactPhone = cp.ContactPhone,
                Tin = cp.Tin,
                LogoUrl = null,
                OwnerSignatureUrl = cp.OwnerSignatureUrl,
                PayrollCycle = cp.PayrollCycle,
                WorkHoursPerDay = cp.WorkHoursPerDay,
                OvertimeRate = cp.OvertimeRate,
                LateGraceMinutes = cp.LateGraceMinutes,
                HolidayRate = cp.HolidayRate,
                ScheduledTimeIn = cp.ScheduledTimeIn,
                ScheduledTimeOut = cp.ScheduledTimeOut,
                TimeInCutoffHours = cp.TimeInCutoffHours,
                VacationLeaveDays = cp.VacationLeaveDays,
                VacationLeaveResetType = cp.VacationLeaveResetType,
                RolesJson = cp.RolesJson
            };

            var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
            if (success)
            {
                // Refresh JWT so nav logo updates immediately
                var newToken = await _authApi.RefreshTokenAsync(token);
                if (!string.IsNullOrEmpty(newToken))
                    Response.Cookies.Append("PayRex.AuthToken", newToken, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.None, Path = "/", Expires = DateTimeOffset.UtcNow.AddHours(8) });

                SuccessMessage = message ?? "Company logo removed";
            }
            else ErrorMessage = message ?? "Failed to remove logo";
            return RedirectToPage(new { edit = true });
        }

        public async Task<IActionResult> OnPostUploadOwnerSignatureAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (OwnerSignatureFile == null || OwnerSignatureFile.Length == 0)
            {
                StatusMessage = "No file selected.";
                return RedirectToPage(new { edit = true });
            }

            const long MaxBytes = 2 * 1024 * 1024;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(OwnerSignatureFile.FileName).ToLowerInvariant();
            if (OwnerSignatureFile.Length > MaxBytes || !allowed.Contains(ext))
            {
                StatusMessage = "Invalid file. Max 2MB, JPG/PNG/WEBP allowed.";
                return RedirectToPage(new { edit = true });
            }

            if (string.IsNullOrEmpty(token))
            {
                StatusMessage = "Not authenticated.";
                return RedirectToPage(new { edit = true });
            }

            try
            {
                await using var ms = new MemoryStream();
                await OwnerSignatureFile.CopyToAsync(ms);
                ms.Position = 0;
                var resp = await _authApi.UploadOwnerSignatureAsync(token, ms, OwnerSignatureFile.FileName, OwnerSignatureFile.ContentType);
                if (resp != null && resp.Success && !string.IsNullOrEmpty(resp.ImageUrl))
                {
                    // The signature endpoint already saves to DB, but we call UpdateCompanyProfileAsync
                    // to keep all other fields intact and get a consistent API response.
                    var cp = await _authApi.GetCompanyProfileAsync(token);
                    if (cp == null)
                    {
                        StatusMessage = "Failed to load company profile before saving signature.";
                        return RedirectToPage(new { edit = true });
                    }
                    var dto = new UpdateCompanyRequestDto
                    {
                        CompanyName = cp.CompanyName ?? string.Empty,
                        Address = cp.Address,
                        ContactEmail = cp.ContactEmail,
                        ContactPhone = cp.ContactPhone,
                        Tin = cp.Tin,
                        LogoUrl = cp.LogoUrl,
                        OwnerSignatureUrl = resp.ImageUrl,
                        PayrollCycle = cp.PayrollCycle,
                        WorkHoursPerDay = cp.WorkHoursPerDay,
                        OvertimeRate = cp.OvertimeRate,
                        LateGraceMinutes = cp.LateGraceMinutes,
                        HolidayRate = cp.HolidayRate,
                        ScheduledTimeIn = cp.ScheduledTimeIn,
                        ScheduledTimeOut = cp.ScheduledTimeOut,
                        TimeInCutoffHours = cp.TimeInCutoffHours,
                        VacationLeaveDays = cp.VacationLeaveDays,
                        VacationLeaveResetType = cp.VacationLeaveResetType,
                        RolesJson = cp.RolesJson
                    };

                    var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
                    if (success) SuccessMessage = message ?? "Owner signature updated";
                    else ErrorMessage = message ?? "Failed to save signature";
                }
                else
                {
                    ErrorMessage = resp?.Message ?? "Failed to upload signature to cloud.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Error uploading signature: " + ex.Message;
            }

            return RedirectToPage(new { edit = true });
        }

        public async Task<IActionResult> OnPostRemoveOwnerSignatureAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) { StatusMessage = "Not authenticated."; return RedirectToPage(new { edit = true }); }

            var cp = await _authApi.GetCompanyProfileAsync(token);
            if (cp == null) { ErrorMessage = "Failed to load company profile."; return RedirectToPage(new { edit = true }); }

            var dto = new UpdateCompanyRequestDto
            {
                CompanyName = cp.CompanyName ?? string.Empty,
                Address = cp.Address,
                ContactEmail = cp.ContactEmail,
                ContactPhone = cp.ContactPhone,
                Tin = cp.Tin,
                LogoUrl = cp.LogoUrl,
                OwnerSignatureUrl = null,
                PayrollCycle = cp.PayrollCycle,
                WorkHoursPerDay = cp.WorkHoursPerDay,
                OvertimeRate = cp.OvertimeRate,
                LateGraceMinutes = cp.LateGraceMinutes,
                HolidayRate = cp.HolidayRate,
                ScheduledTimeIn = cp.ScheduledTimeIn,
                ScheduledTimeOut = cp.ScheduledTimeOut,
                TimeInCutoffHours = cp.TimeInCutoffHours,
                VacationLeaveDays = cp.VacationLeaveDays,
                VacationLeaveResetType = cp.VacationLeaveResetType,
                RolesJson = cp.RolesJson
            };

            var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
            if (success) SuccessMessage = message ?? "Owner signature removed";
            else ErrorMessage = message ?? "Failed to remove signature";
            return RedirectToPage(new { edit = true });
        }

        public async Task<IActionResult> OnPostSaveCompanyAsync()
        {
            // Accepts values from form fields bound by name
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            // Fetch current profile to preserve LogoUrl and OwnerSignatureUrl (uploaded separately)
            var currentProfile = await _authApi.GetCompanyProfileAsync(token);
            if (currentProfile == null)
            {
                ErrorMessage = "Failed to load current company profile";
                return RedirectToPage(new { edit = true });
            }

            var dto = new UpdateCompanyRequestDto
            {
                CompanyName = Request.Form["CompanyName"],
                Address = Request.Form["Address"],
                ContactEmail = Request.Form["ContactEmail"],
                ContactPhone = Request.Form["ContactPhone"],
                Tin = Request.Form["Tin"],
                // Preserve the current LogoUrl and OwnerSignatureUrl (may have been uploaded separately)
                LogoUrl = currentProfile.LogoUrl,
                OwnerSignatureUrl = currentProfile.OwnerSignatureUrl
            };

            // Also pick up payroll fields if present in the same form
            if (Request.Form.ContainsKey("PayrollCycle") && int.TryParse(Request.Form["PayrollCycle"], out var pc)) dto.PayrollCycle = pc;
            if (Request.Form.ContainsKey("WorkHoursPerDay") && decimal.TryParse(Request.Form["WorkHoursPerDay"], out var wh)) dto.WorkHoursPerDay = wh;
            if (Request.Form.ContainsKey("OvertimeRate") && decimal.TryParse(Request.Form["OvertimeRate"], out var orr)) dto.OvertimeRate = orr;
            if (Request.Form.ContainsKey("LateGraceMinutes") && int.TryParse(Request.Form["LateGraceMinutes"], out var lg)) dto.LateGraceMinutes = lg;
            if (Request.Form.ContainsKey("HolidayRate") && decimal.TryParse(Request.Form["HolidayRate"], out var hr)) dto.HolidayRate = hr;

            // Work Schedule & Leave
            if (Request.Form.ContainsKey("ScheduledTimeIn")) dto.ScheduledTimeIn = Request.Form["ScheduledTimeIn"];
            if (Request.Form.ContainsKey("ScheduledTimeOut")) dto.ScheduledTimeOut = Request.Form["ScheduledTimeOut"];
            if (Request.Form.ContainsKey("TimeInCutoffHours") && int.TryParse(Request.Form["TimeInCutoffHours"], out var tich)) dto.TimeInCutoffHours = tich;
            if (Request.Form.ContainsKey("VacationLeaveDays") && int.TryParse(Request.Form["VacationLeaveDays"], out var vld)) dto.VacationLeaveDays = vld;
            if (Request.Form.ContainsKey("VacationLeaveResetType") && int.TryParse(Request.Form["VacationLeaveResetType"], out var vlrt)) dto.VacationLeaveResetType = vlrt;

            // If roles were submitted as JSON (rare), include them on profile for backward compatibility
            if (Request.Form.ContainsKey("RolesJson")) dto.RolesJson = Request.Form["RolesJson"];

            var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
            if (success)
            {
                // Do not show a default "Company updated" message; show any message returned by API instead
                if (!string.IsNullOrEmpty(message)) SuccessMessage = message;
                IsEditMode = false;
            }
            else
            {
                ErrorMessage = message ?? "Failed to update company";
                IsEditMode = true;
            }

            return RedirectToPage(new { edit = IsEditMode });
        }

        public async Task<IActionResult> OnPostSaveCompanyRolesAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            var rolesJson = Request.Form["RolesJson"].FirstOrDefault();
            if (string.IsNullOrEmpty(rolesJson))
            {
                StatusMessage = "No roles provided";
                return RedirectToPage(new { edit = IsEditMode });
            }

            try
            {
                var apiRoles = JsonSerializer.Deserialize<List<EmployeeRoleDto>>(rolesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                 ?? new List<EmployeeRoleDto>();

                var (success, message) = await _authApi.SyncEmployeeRolesAsync(token, apiRoles);
                if (success)
                {
                    StatusMessage = message ?? "Roles updated";
                    IsEditMode = false;
                }
                else
                {
                    StatusMessage = message ?? "Failed to update roles";
                    IsEditMode = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Failed to parse roles JSON: " + ex.Message;
                IsEditMode = true;
            }

            return RedirectToPage(new { edit = IsEditMode });
        }

        public async Task<IActionResult> OnPostSavePayrollAsync()
        {
            IsEditMode = true;
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token)) return RedirectToPage("/Auth/Login");

            var dto = new UpdateCompanyRequestDto
            {
                CompanyName = Profile.CompanyName,
                PayrollCycle = Profile.PayrollCycle,
                WorkHoursPerDay = Profile.WorkHoursPerDay,
                OvertimeRate = Profile.OvertimeRate,
                LateGraceMinutes = Profile.LateGraceMinutes,
                HolidayRate = Profile.HolidayRate,
                ScheduledTimeIn = Profile.ScheduledTimeIn,
                ScheduledTimeOut = Profile.ScheduledTimeOut,
                VacationLeaveDays = Profile.VacationLeaveDays,
                VacationLeaveResetType = Profile.VacationLeaveResetType
            };

            var (success, message) = await _authApi.UpdateCompanyProfileAsync(token, dto);
            if (success)
            {
                StatusMessage = message ?? "Payroll settings updated";
                IsEditMode = false;
            }
            else
            {
                StatusMessage = message ?? "Failed to update payroll settings";
                IsEditMode = true;
            }

            return RedirectToPage(new { edit = IsEditMode });
        }

        public class RoleConfig
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; } = "";
            public decimal BasicRate { get; set; }
            public string RateType { get; set; } = "";
            public string Description { get; set; } = "";
            public bool IsActive { get; set; } = true;
        }
    }
}
