using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PayRex.Web.Services
{
    public class BillingApiService : IBillingApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BillingApiService> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public BillingApiService(HttpClient httpClient, ILogger<BillingApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private void SetAuth(string token)
        {
            var t = token.Trim();
            if (t.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                t = t[7..].Trim();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", t);
        }

        public async Task<SubscriptionInfoDto?> GetSubscriptionAsync(string token)
        {
            SetAuth(token);
            try
            {
                var response = await _httpClient.GetAsync("api/billing/subscription");
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SubscriptionInfoDto>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching subscription");
                return null;
            }
        }

        public async Task<List<InvoiceItemDto>> GetInvoicesAsync(string token)
        {
            SetAuth(token);
            try
            {
                var response = await _httpClient.GetAsync("api/billing/invoices");
                if (!response.IsSuccessStatusCode) return new();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<InvoiceItemDto>>(json, JsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching invoices");
                return new();
            }
        }

        public async Task<CheckoutResultItemDto?> CreateCheckoutAsync(string token, int? invoiceId = null, int? planId = null)
        {
            SetAuth(token);
            try
            {
                var body = new { InvoiceId = invoiceId, PlanId = planId };
                var content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/billing/checkout", content);
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CheckoutResultItemDto>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout");
                return null;
            }
        }

        public async Task<CheckoutResultItemDto?> RenewSubscriptionAsync(string token)
        {
            SetAuth(token);
            try
            {
                var response = await _httpClient.PostAsync("api/billing/renew", null);
                if (!response.IsSuccessStatusCode) return null;
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<CheckoutResultItemDto>(json, JsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing subscription");
                return null;
            }
        }
    }
}
