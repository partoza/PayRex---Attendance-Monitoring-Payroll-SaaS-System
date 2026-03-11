using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Pages
{
    [Authorize(Roles = "Admin")]
    public class TestPaymentModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<TestPaymentModel> _logger;

        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? SelectedMethod { get; set; }
        public string? ErrorMessage { get; set; }

        public TestPaymentModel(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<TestPaymentModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public IActionResult OnGet(int paymentId, decimal amount, string? description)
        {
            PaymentId = paymentId;
            Amount = amount;
            Description = description ?? "Subscription Payment";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int paymentId, decimal amount, string? description, string paymentMethod)
        {
            PaymentId = paymentId;
            Amount = amount;
            Description = description ?? "Subscription Payment";

            if (!Request.Cookies.TryGetValue("PayRex.AuthToken", out var token) || string.IsNullOrEmpty(token))
                return RedirectToPage("/Auth/Login");

            try
            {
                var apiBase = _config["ApiBaseUrl"] ?? "https://localhost:5000";
                var client = _httpClientFactory.CreateClient();
                var t = token.Trim();
                if (t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    t = t[7..].Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", t);

                var body = new { PaymentMethod = paymentMethod };
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{apiBase}/api/billing/test-confirm/{paymentId}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Test payment of ₱{amount:N2} via {paymentMethod.ToUpper()} completed successfully!";
                    return RedirectToPage("/Billing");
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Test payment confirmation failed: {Status} {Body}", response.StatusCode, errorBody);
                ErrorMessage = "Payment confirmation failed. The payment may have already been processed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming test payment");
                ErrorMessage = "An error occurred while processing the payment. Please try again.";
            }

            return Page();
        }
    }
}
