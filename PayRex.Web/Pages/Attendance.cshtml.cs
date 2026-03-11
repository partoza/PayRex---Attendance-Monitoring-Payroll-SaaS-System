using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using PayRexApplication.Data;
using PayRex.Web.QuestPdf;
using PayRex.Web.Services;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin,HR,Employee")]
    [IgnoreAntiforgeryToken]  // Auth via JWT cookie; no form-based antiforgery needed
    public class AttendanceModel : PageModel
    {
        private readonly IAttendanceApiService _attendanceApi;
        private readonly ILogger<AttendanceModel> _logger;
        private readonly AppDbContext _db;

        public AttendanceModel(IAttendanceApiService attendanceApi, ILogger<AttendanceModel> logger, AppDbContext db)
        {
            _attendanceApi = attendanceApi;
            _logger = logger;
            _db = db;
        }

        public List<PayRex.Web.Services.AttendanceRecordDto> Records { get; set; } = new();
        public PayRex.Web.Services.AttendanceStatsDto Stats { get; set; } = new();
        public string CurrentDateRange { get; set; } = "";
        public bool IsAttendanceConfigured { get; set; } = true;

        public async Task OnGetAsync()
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token))
            {
                Response.Redirect("/Auth/Login");
                return;
            }

            // Fetch company config to determine payroll period and configuration status
            var config = await _attendanceApi.GetCompanyConfigAsync(token);
            IsAttendanceConfigured = config?.IsConfigured ?? false;

            // Compute period dates based on payroll cycle
            var now = DateTime.Now;
            DateTime from, to;

            if (config != null)
            {
                var cycle = config.PayrollCycle; // 0 = Monthly, 1 = SemiMonthly
                if (cycle == 1) // SemiMonthly
                {
                    if (now.Day <= 15)
                    {
                        from = new DateTime(now.Year, now.Month, 1);
                        to = new DateTime(now.Year, now.Month, 15);
                    }
                    else
                    {
                        var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                        from = new DateTime(now.Year, now.Month, 16);
                        to = new DateTime(now.Year, now.Month, lastDay);
                    }
                }
                else // Monthly (default)
                {
                    var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                    from = new DateTime(now.Year, now.Month, 1);
                    to = new DateTime(now.Year, now.Month, lastDay);
                }
            }
            else
            {
                // Fallback to monthly if config not available
                var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                from = new DateTime(now.Year, now.Month, 1);
                to = new DateTime(now.Year, now.Month, lastDay);
            }

            CurrentDateRange = from.ToString("MMMM dd, yyyy") + " - " + to.ToString("MMMM dd, yyyy");

            var statsTask = _attendanceApi.GetTodayStatsAsync(token);
            var recordsTask = _attendanceApi.GetAttendanceRecordsAsync(token, from, to);
            await Task.WhenAll(statsTask, recordsTask);
            Stats = statsTask.Result;
            Records = recordsTask.Result;
        }

        /// <summary>
        /// AJAX GET handler: preview employee by QR value (proxies to backend API).
        /// GET /Attendance?handler=Preview&amp;qrValue=XXX
        /// </summary>
        public async Task<IActionResult> OnGetPreviewAsync([FromQuery] string qrValue)
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token))
                return new JsonResult(new { message = "Unauthorized" }) { StatusCode = 401 };

            if (string.IsNullOrWhiteSpace(qrValue))
                return new JsonResult(new { message = "QR value is required" }) { StatusCode = 400 };

            var (found, error, data) = await _attendanceApi.PreviewEmployeeAsync(token, qrValue);

            if (!found)
                return new JsonResult(new { message = error }) { StatusCode = 404 };

            return new JsonResult(data);
        }

        /// <summary>
        /// AJAX POST handler: log attendance scan (proxies to backend API).
        /// POST /Attendance?handler=Scan  — body: JSON { "qrValue": "..." }
        /// </summary>
        public async Task<IActionResult> OnPostScanAsync()
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token))
                return new JsonResult(new { message = "Unauthorized" }) { StatusCode = 401 };

            // Razor Pages doesn't support [FromBody] — read JSON manually
            string qrValue = string.Empty;
            try
            {
                Request.EnableBuffering();
                using var reader = new System.IO.StreamReader(Request.Body, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                Request.Body.Position = 0;
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("qrValue", out var qrProp))
                    qrValue = qrProp.GetString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse scan request body");
                return new JsonResult(new { message = "Invalid request body" }) { StatusCode = 400 };
            }

            if (string.IsNullOrWhiteSpace(qrValue))
                return new JsonResult(new { message = "QR value is required" }) { StatusCode = 400 };

            var (success, message, _) = await _attendanceApi.ProcessQrScanAsync(token, qrValue, "web");

            if (!success)
                return new JsonResult(new { message }) { StatusCode = 400 };

            return new JsonResult(new { message });
        }

        private int GetCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var cid) ? cid : 0;
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            var token = Request.Cookies.TryGetValue("PayRex.AuthToken", out var t) ? t : null;
            if (string.IsNullOrEmpty(token))
            {
                return Redirect("/Auth/Login");
            }

            var exportCompanyId = GetCompanyId();

            var fromDateStr = Request.Form["fromDate"].ToString() ?? string.Empty;
            var toDateStr = Request.Form["toDate"].ToString() ?? string.Empty;
            var status = Request.Form["status"].ToString() ?? string.Empty;
            var search = Request.Form["search"].ToString() ?? string.Empty;

            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MaxValue;

            if (!DateTime.TryParse(fromDateStr, out from) || !DateTime.TryParse(toDateStr, out to))
            {
                // Fallback
                var config = await _attendanceApi.GetCompanyConfigAsync(token);
                var now = DateTime.Now;
                if (config != null && config.PayrollCycle == 1) // SemiMonthly
                {
                    if (now.Day <= 15)
                    {
                        from = new DateTime(now.Year, now.Month, 1);
                        to = new DateTime(now.Year, now.Month, 15);
                    }
                    else
                    {
                        var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                        from = new DateTime(now.Year, now.Month, 16);
                        to = new DateTime(now.Year, now.Month, lastDay);
                    }
                }
                else
                {
                    var lastDay = DateTime.DaysInMonth(now.Year, now.Month);
                    from = new DateTime(now.Year, now.Month, 1);
                    to = new DateTime(now.Year, now.Month, lastDay);
                }
            }

            string activeFilterDesc = BuildAttendanceFilterSentence(companyName: null, from, to, status, search);
            if (!string.IsNullOrWhiteSpace(status)) { /* included below after companyName is known */ }
            if (!string.IsNullOrWhiteSpace(search)) { /* included below after companyName is known */ }

            // Fetch records
            var records = await _attendanceApi.GetAttendanceRecordsAsync(token, from, to);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(status))
            {
                records = records.Where(r => r.Status == status).ToList();
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                records = records.Where(r => (r.EmployeeName ?? "").ToLower().Contains(search) || (r.EmployeeId ?? "").ToLower().Contains(search)).ToList();
            }

            // Map records exactly to our exported model
            var exportRecords = records.Select(r => new AttendanceRecordExport
            {
                Date = DateTime.TryParse(r.Date, out var pd) ? pd.ToString("MMMM dd, yyyy") : r.Date,
                EmployeeName = r.EmployeeName ?? "-",
                EmployeeId = r.EmployeeId ?? "-",
                TimeIn = r.TimeIn ?? "—",
                TimeOut = r.TimeOut ?? "—",
                Hours = r.TotalHoursWorked > 0 ? r.TotalHoursWorked.ToString("0.00") : "—",
                Status = r.Status ?? "Unknown",
                Remarks = string.IsNullOrWhiteSpace(r.Remarks) ? "None" : r.Remarks
            }).ToList();

            // Fetch full company details for the report
            var company = await _db.Companies
                .AsNoTracking()
                .Where(c => c.CompanyId == exportCompanyId)
                .FirstOrDefaultAsync();

            var companyName    = company?.CompanyName ?? "Company";
            var companyAddress = company?.Address;
            var companyEmail   = company?.ContactEmail;
            var companyPhone   = company?.ContactPhone;
            var companyLogoUrl = company?.LogoUrl;

            // Now that companyName is known, build the natural-language filter sentence
            activeFilterDesc = BuildAttendanceFilterSentence(companyName, from, to, status, search);

            // Get issuer name, position, and signature URL
            string issuerName      = "User";
            string issuerPosition  = User.FindFirst(ClaimTypes.Role)?.Value ?? "Staff";
            string? issuerSignatureUrl = null;
            int issuingUserId      = 0;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);
                var given  = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.GivenName)?.Value;
                var family = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.FamilyName)?.Value;
                if (!string.IsNullOrEmpty(given))
                    issuerName = !string.IsNullOrEmpty(family) ? $"{given} {family}" : given;
                int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "uid")?.Value, out issuingUserId);
            }
            catch { }

            if (issuingUserId > 0)
            {
                issuerSignatureUrl = await _db.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == issuingUserId)
                    .Select(u => u.SignatureUrl)
                    .FirstOrDefaultAsync();
            }

            var filename = $"{companyName}_AttendanceRecords_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            var generator = new AttendancePdfGenerator();
            var pdfBytes = generator.Generate(new AttendancePdfGeneratorOptions
            {
                CompanyName      = companyName,
                CompanyLogoUrl   = companyLogoUrl,
                CompanyAddress   = companyAddress,
                CompanyEmail     = companyEmail,
                CompanyPhone     = companyPhone,
                IssuerName       = issuerName,
                IssuerPosition   = issuerPosition,
                IssuerSignatureUrl = issuerSignatureUrl,
                ActiveFiltersDescription = activeFilterDesc,
                Records          = exportRecords
            });

            return new FileContentResult(pdfBytes, "application/pdf") { FileDownloadName = filename };
        }

        private static string BuildAttendanceFilterSentence(string? companyName, DateTime from, DateTime to, string status, string search)
        {
            var company = string.IsNullOrWhiteSpace(companyName) ? "the company" : companyName;
            string period = from.Month == to.Month && from.Year == to.Year
                ? $"{from:MMMM dd} – {to:dd, yyyy}"
                : $"{from:MMMM dd, yyyy} – {to:MMMM dd, yyyy}";

            var sentence = $"Attendance records for {company} covering the period {period}";

            var qualifiers = new List<string>();
            if (!string.IsNullOrWhiteSpace(status))
                qualifiers.Add($"with a status of \"{status}\"");
            if (!string.IsNullOrWhiteSpace(search))
                qualifiers.Add($"matching the keyword \"{search}\"");

            if (qualifiers.Count > 0)
                sentence += $", filtered to show entries {string.Join(" and ", qualifiers)}";

            return sentence + ".";
        }
    }
}
