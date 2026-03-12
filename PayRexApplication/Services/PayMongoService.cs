using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.Enums;
using PayRexApplication.Models;

namespace PayRexApplication.Services
{
    public interface IPayMongoService
    {
        Task<(string CheckoutUrl, string SessionId)?> CreateCheckoutSessionAsync(
            decimal amount, string description, int invoiceId, int companyId);
        Task<string?> GetCheckoutSessionStatusAsync(string sessionId);
        Task<bool> HandleWebhookAsync(string payload, string signatureHeader);
    }

    public class PayMongoService : IPayMongoService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _db;
        private readonly IActivityLoggerService _audit;
        private readonly ILogger<PayMongoService> _logger;

        public PayMongoService(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            AppDbContext db,
            IActivityLoggerService audit,
            ILogger<PayMongoService> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _db = db;
            _audit = audit;
            _logger = logger;
        }

        public async Task<(string CheckoutUrl, string SessionId)?> CreateCheckoutSessionAsync(
            decimal amount, string description, int invoiceId, int companyId)
        {
            var secretKey = _config["PayMongo:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                _logger.LogWarning("PayMongo SecretKey is not configured");
                return null;
            }

            var baseUrl = _config["PayMongo:BaseUrl"] ?? "https://api.paymongo.com/v1";
            var successUrl = _config["PayMongo:SuccessUrl"] ?? "https://localhost:7002/Billing?handler=PaymentSuccess";
            var cancelUrl = _config["PayMongo:CancelUrl"] ?? "https://localhost:7002/Billing?handler=PaymentCancel";

            // Amount in centavos (PayMongo uses smallest currency unit)
            var amountInCentavos = (int)(amount * 100);

            var requestBody = new
            {
                data = new
                {
                    attributes = new
                    {
                        send_email_receipt = true,
                        show_description = true,
                        show_line_items = true,
                        description,
                        line_items = new[]
                        {
                            new
                            {
                                currency = "PHP",
                                amount = amountInCentavos,
                                name = description,
                                quantity = 1
                            }
                        },
                        payment_method_types = new[] { "card", "gcash", "grab_pay", "maya" },
                        success_url = successUrl.Replace("{CHECKOUT_SESSION_ID}", ""),
                        cancel_url = cancelUrl,
                        metadata = new
                        {
                            invoice_id = invoiceId.ToString(),
                            company_id = companyId.ToString()
                        }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/checkout_sessions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PayMongo checkout session creation failed: {Status} {Body}",
                    response.StatusCode, responseBody);
                return null;
            }

            using var doc = JsonDocument.Parse(responseBody);
            var data = doc.RootElement.GetProperty("data");
            var sessionId = data.GetProperty("id").GetString() ?? "";
            var checkoutUrl = data.GetProperty("attributes").GetProperty("checkout_url").GetString() ?? "";

            return (checkoutUrl, sessionId);
        }

        public async Task<string?> GetCheckoutSessionStatusAsync(string sessionId)
        {
            var secretKey = _config["PayMongo:SecretKey"];
            if (string.IsNullOrEmpty(secretKey)) return null;

            var baseUrl = _config["PayMongo:BaseUrl"] ?? "https://api.paymongo.com/v1";

            var client = _httpClientFactory.CreateClient();
            var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{secretKey}:"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            var response = await client.GetAsync($"{baseUrl}/checkout_sessions/{sessionId}");
            if (!response.IsSuccessStatusCode) return null;

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var status = doc.RootElement
                .GetProperty("data")
                .GetProperty("attributes")
                .GetProperty("status")
                .GetString();

            return status;
        }

        public async Task<bool> HandleWebhookAsync(string payload, string signatureHeader)
        {
            // In production, verify webhook signature using PayMongo:WebhookSecret
            // For test mode, we process the event directly

            try
            {
                using var doc = JsonDocument.Parse(payload);
                var eventType = doc.RootElement
                    .GetProperty("data")
                    .GetProperty("attributes")
                    .GetProperty("type")
                    .GetString();

                if (eventType == "checkout_session.payment.paid")
                {
                    var resourceData = doc.RootElement
                        .GetProperty("data")
                        .GetProperty("attributes")
                        .GetProperty("data");

                    var attributes = resourceData.GetProperty("attributes");
                    var metadata = attributes.GetProperty("metadata");

                    var invoiceIdStr = metadata.GetProperty("invoice_id").GetString();
                    var companyIdStr = metadata.GetProperty("company_id").GetString();
                    var sessionId = resourceData.GetProperty("id").GetString();

                    if (!int.TryParse(invoiceIdStr, out var invoiceId) ||
                        !int.TryParse(companyIdStr, out var companyId))
                    {
                        _logger.LogWarning("Invalid metadata in PayMongo webhook");
                        return false;
                    }

                    return await ProcessSuccessfulPaymentAsync(invoiceId, companyId, sessionId ?? "");
                }

                _logger.LogInformation("Unhandled PayMongo webhook event: {Type}", eventType);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PayMongo webhook");
                return false;
            }
        }

        private async Task<bool> ProcessSuccessfulPaymentAsync(int invoiceId, int companyId, string sessionId)
        {
            var invoice = await _db.BillingInvoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for webhook", invoiceId);
                return false;
            }

            // Update invoice
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;

            // Update or create payment record
            var payment = invoice.Payments.FirstOrDefault(p => p.PayMongoCheckoutSessionId == sessionId)
                ?? invoice.Payments.FirstOrDefault(p => p.Status == PaymentStatus.Pending);

            if (payment != null)
            {
                payment.Status = PaymentStatus.Success;
                payment.PaidAt = DateTime.UtcNow;
                payment.PayMongoCheckoutSessionId = sessionId;
                payment.Amount = invoice.Amount;
            }
            else
            {
                payment = new Payment
                {
                    InvoiceId = invoiceId,
                    CompanyId = companyId,
                    Provider = "PayMongo",
                    ReferenceNo = sessionId,
                    Status = PaymentStatus.Success,
                    PaidAt = DateTime.UtcNow,
                    Amount = invoice.Amount,
                    PayMongoCheckoutSessionId = sessionId,
                    Currency = "PHP",
                    CreatedAt = DateTime.UtcNow
                };
                _db.Payments.Add(payment);
            }

            // Extend subscription
            var subscription = await _db.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (subscription != null)
            {
                var extensionMonths = subscription.SubscriptionPlan.BillingCycle == BillingCycle.Yearly ? 12 : 1;
                var newStart = subscription.EndDate > DateTime.UtcNow ? subscription.EndDate : DateTime.UtcNow;
                subscription.StartDate = newStart;
                subscription.EndDate = newStart.AddMonths(extensionMonths);
                subscription.Status = SubscriptionStatus.Active;
                subscription.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            // Auto-log subscription payment as Finance income (SuperAdmin-level)
            var companyName = await _db.Companies
                .Where(c => c.CompanyId == companyId)
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync() ?? $"Company #{companyId}";
            var vatAmount = invoice.Amount * 0.12m;
            _db.FinanceEntries.Add(new FinanceEntry
            {
                Type = "Income",
                Description = $"Subscription payment from {companyName}",
                Amount = invoice.Amount,
                VatAmount = vatAmount,
                Category = "Subscription",
                Reference = invoice.InvoiceNumber ?? sessionId,
                IsAutoGenerated = true,
                CreatedAt = DateTime.UtcNow
            });

            // Auto-log subscription payment as Cost for the company (Admin side)
            _db.FinanceEntries.Add(new FinanceEntry
            {
                Type = "Deduction",
                Description = $"Subscription payment to PayRex",
                Amount = invoice.Amount,
                VatAmount = 0m,
                Category = "Subscription",
                Reference = invoice.InvoiceNumber ?? sessionId,
                IsAutoGenerated = true,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // Audit log
            await _audit.LogAsync(null, companyId, "PaymentReceived", "Payment",
                payment.PaymentId.ToString(), null, $"₱{invoice.Amount:N2} via PayMongo",
                null, null);

            _logger.LogInformation("Payment processed for invoice {InvoiceId}, company {CompanyId}",
                invoiceId, companyId);

            return true;
        }
    }
}
