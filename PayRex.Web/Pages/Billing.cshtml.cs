using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayRex.Web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class BillingModel : PageModel
    {
        private readonly IBillingApiService _billing;
        private readonly IAuthApiService _auth;
        private readonly ILogger<BillingModel> _logger;

        public SubscriptionInfoDto? Subscription { get; set; }
        public List<InvoiceItemDto> Invoices { get; set; } = new();
        public List<PlanItemDto> Plans { get; set; } = new();
        public string? UserRole { get; set; }
        public string? ErrorMessage { get; set; }
        [TempData(Key = "Billing_SuccessMessage")] public string? SuccessMessage { get; set; }
        [TempData(Key = "Billing_ErrorMessage")] public string? BillingTempError { get; set; }

        public BillingModel(IBillingApiService billing, IAuthApiService auth, ILogger<BillingModel> logger)
        {
            _billing = billing;
            _auth = auth;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int? planId = null)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            if (planId.HasValue)
            {
                var checkout = await _billing.CreateCheckoutAsync(token, planId: planId.Value);
                if (checkout != null && !string.IsNullOrEmpty(checkout.CheckoutUrl))
                    return Redirect(checkout.CheckoutUrl);
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                UserRole = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            }
            catch { }

            var subTask = _billing.GetSubscriptionAsync(token);
            var invTask = _billing.GetInvoicesAsync(token);
            await Task.WhenAll(subTask, invTask);
            Subscription = subTask.Result;
            Invoices = invTask.Result;

            // Fetch available plans for upgrade/downgrade section
            try
            {
                var factory = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                var apiClient = factory.CreateClient("PayRexApi");
                apiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var planResp = await apiClient.GetAsync("api/superadmin/plans");
                if (planResp.IsSuccessStatusCode)
                {
                    var json = await planResp.Content.ReadAsStringAsync();
                    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    Plans = System.Text.Json.JsonSerializer.Deserialize<List<PlanItemDto>>(json, opts) ?? new();
                }
            }
            catch { }

            // After a successful payment redirect, refresh the JWT so subscriptionStatus and
            // isSetupComplete claims are updated without requiring the user to log out / in.
            var sessionId = Request.Query["session_id"].ToString();
            if (!string.IsNullOrEmpty(sessionId) && Subscription != null &&
                Subscription.Status is "Active" or "Trialing" or "Trial")
            {
                try
                {
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
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to refresh token after payment success");
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRenewAsync()
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var result = await _billing.RenewSubscriptionAsync(token);
            if (result != null && !string.IsNullOrEmpty(result.CheckoutUrl))
                return Redirect(result.CheckoutUrl);

            ErrorMessage = "Unable to create checkout session. Please try again or contact support.";
            var subTask2 = _billing.GetSubscriptionAsync(token);
            var invTask2 = _billing.GetInvoicesAsync(token);
            await Task.WhenAll(subTask2, invTask2);
            Subscription = subTask2.Result;
            Invoices = invTask2.Result;
            return Page();
        }

        public async Task<IActionResult> OnPostPayInvoiceAsync(int invoiceId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var result = await _billing.CreateCheckoutAsync(token, invoiceId: invoiceId);
            if (result != null && !string.IsNullOrEmpty(result.CheckoutUrl))
                return Redirect(result.CheckoutUrl);

            ErrorMessage = "Unable to create checkout session. Please try again.";
            var subTask3 = _billing.GetSubscriptionAsync(token);
            var invTask3 = _billing.GetInvoicesAsync(token);
            await Task.WhenAll(subTask3, invTask3);
            Subscription = subTask3.Result;
            Invoices = invTask3.Result;
            return Page();
        }

        public async Task<IActionResult> OnPostArchiveInvoiceAsync(int invoiceId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var factory = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var apiClient = factory.CreateClient("PayRexApi");
            apiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await apiClient.PostAsync($"api/billing/invoices/{invoiceId}/archive", null);
            if (response.IsSuccessStatusCode)
                SuccessMessage = "Invoice archived successfully.";
            else
                BillingTempError = "Unable to archive invoice.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteInvoiceAsync(int invoiceId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var factory = HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var apiClient = factory.CreateClient("PayRexApi");
            apiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Delete,
                $"api/billing/invoices/{invoiceId}");
            var response = await apiClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
                SuccessMessage = "Invoice deleted successfully.";
            else
                BillingTempError = "Unable to delete invoice. Only unpaid invoices can be deleted.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostScheduleDowngradeAsync(int planId)
        {
            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            var (success, message) = await _billing.ScheduleDowngradeAsync(token, planId);
            if (success)
                SuccessMessage = message;
            else
                BillingTempError = message;

            return RedirectToPage();
        }

        public class PlanItemDto
        {
            public int PlanId { get; set; }
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string BillingCycle { get; set; } = string.Empty;
            public int MaxEmployees { get; set; }
            public string? Description { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
