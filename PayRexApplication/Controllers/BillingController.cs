using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.DTOs;
using PayRexApplication.Helpers;
using PayRexApplication.Services;

namespace PayRexApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly ISubscriptionService _subscription;
        private readonly IPayMongoService _payMongo;
        private readonly IActivityLoggerService _audit;
        private readonly IConfiguration _config;
        private readonly ILogger<BillingController> _logger;
        private readonly AppDbContext _db;

        public BillingController(
            ISubscriptionService subscription,
            IPayMongoService payMongo,
            IActivityLoggerService audit,
            IConfiguration config,
            ILogger<BillingController> logger,
            AppDbContext db)
        {
            _subscription = subscription;
            _payMongo = payMongo;
            _audit = audit;
            _config = config;
            _logger = logger;
            _db = db;
        }

        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscription()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var dto = await _subscription.GetCompanySubscriptionAsync(companyId.Value);
            if (dto == null) return NotFound(new { message = "No subscription found" });

            return Ok(dto);
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var invoices = await _subscription.GetCompanyInvoicesAsync(companyId.Value);
            return Ok(invoices);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequestDto request)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            PayRexApplication.Models.BillingInvoice invoice;

            if (request.InvoiceId.HasValue)
            {
                var invoices = await _subscription.GetCompanyInvoicesAsync(companyId.Value);
                var inv = invoices.FirstOrDefault(i => i.InvoiceId == request.InvoiceId.Value);
                if (inv == null) return NotFound(new { message = "Invoice not found" });
                if (inv.Status == "Paid") return BadRequest(new { message = "Invoice already paid" });

                // Use existing invoice
                invoice = new PayRexApplication.Models.BillingInvoice
                {
                    InvoiceId = inv.InvoiceId,
                    Amount = inv.Amount,
                    Description = inv.Description ?? "Subscription payment"
                };
            }
            else if (request.PlanId.HasValue)
            {
                invoice = await _subscription.CreateUpgradeInvoiceAsync(companyId.Value, request.PlanId.Value);
            }
            else
            {
                invoice = await _subscription.CreateRenewalInvoiceAsync(companyId.Value);
            }

            // Try PayMongo first; fall back to test payment page if not configured
            var result = await _payMongo.CreateCheckoutSessionAsync(
                invoice.Amount,
                invoice.Description ?? "PayRex Subscription",
                invoice.InvoiceId,
                companyId.Value);

            if (result != null)
            {
                var payment = await _subscription.CreatePendingPaymentAsync(
                    invoice.InvoiceId, companyId.Value, result.Value.SessionId);

                var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
                await _audit.LogAsync(actorId, companyId.Value, "CheckoutCreated", "Payment",
                    payment.PaymentId.ToString(), null, $"₱{invoice.Amount:N2}");

                return Ok(new CheckoutResultDto
                {
                    CheckoutUrl = result.Value.CheckoutUrl,
                    CheckoutSessionId = result.Value.SessionId,
                    PaymentId = payment.PaymentId
                });
            }

            // Test mode — create pending payment and redirect to local test page
            return await CreateTestCheckoutResult(invoice, companyId.Value);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> RenewSubscription()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var invoice = await _subscription.CreateRenewalInvoiceAsync(companyId.Value);

            var result = await _payMongo.CreateCheckoutSessionAsync(
                invoice.Amount,
                invoice.Description ?? "Subscription Renewal",
                invoice.InvoiceId,
                companyId.Value);

            if (result != null)
            {
                var payment = await _subscription.CreatePendingPaymentAsync(
                    invoice.InvoiceId, companyId.Value, result.Value.SessionId);

                return Ok(new CheckoutResultDto
                {
                    CheckoutUrl = result.Value.CheckoutUrl,
                    CheckoutSessionId = result.Value.SessionId,
                    PaymentId = payment.PaymentId
                });
            }

            // Test mode
            return await CreateTestCheckoutResult(invoice, companyId.Value);
        }

        [HttpPost("test-confirm/{paymentId}")]
        public async Task<IActionResult> TestConfirmPayment(int paymentId, [FromBody] TestConfirmDto request)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var success = await _subscription.ProcessTestPaymentAsync(paymentId, companyId.Value, request.PaymentMethod ?? "gcash");
            if (!success) return BadRequest(new { message = "Payment not found or already processed" });

            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            await _audit.LogAsync(actorId, companyId.Value, "TestPaymentConfirmed", "Payment",
                paymentId.ToString(), null, $"Test payment via {request.PaymentMethod ?? "gcash"}");

            return Ok(new { message = "Payment confirmed", status = "Success" });
        }

        private async Task<IActionResult> CreateTestCheckoutResult(
            PayRexApplication.Models.BillingInvoice invoice, int companyId)
        {
            var testSessionId = $"test_{Guid.NewGuid():N}";
            var payment = await _subscription.CreatePendingPaymentAsync(
                invoice.InvoiceId, companyId, testSessionId);

            // Redirect to the Web project's test payment page
            var successUrl = _config["PayMongo:SuccessUrl"] ?? "https://localhost:7002/Billing";
            var uri = new Uri(successUrl);
            var webBaseUrl = $"{uri.Scheme}://{uri.Authority}";
            var testUrl = $"{webBaseUrl}/TestPayment?paymentId={payment.PaymentId}&amount={invoice.Amount}&description={Uri.EscapeDataString(invoice.Description ?? "Subscription")}";

            return Ok(new CheckoutResultDto
            {
                CheckoutUrl = testUrl,
                CheckoutSessionId = testSessionId,
                PaymentId = payment.PaymentId
            });
        }

        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var signature = Request.Headers["Paymongo-Signature"].FirstOrDefault() ?? "";

            var success = await _payMongo.HandleWebhookAsync(payload, signature);
            return success ? Ok() : BadRequest();
        }

        [HttpGet("payment-status/{paymentId}")]
        public async Task<IActionResult> GetPaymentStatus(int paymentId)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var payment = await _subscription.GetCompanyInvoicesAsync(companyId.Value);
            // For simplicity, return from the payment directly
            return Ok(new { status = "pending" });
        }

        [HttpGet("finance-summary")]
        public async Task<IActionResult> GetCompanyFinanceSummary()
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var entries = await _db.FinanceEntries
                .AsNoTracking()
                .Where(f => f.CompanyId == companyId)
                .ToListAsync();

            var totalCost = entries.Where(e => e.Type == "Deduction").Sum(e => e.Amount);
            var totalPaid = totalCost; // Cost = subscription spend, same value

            return Ok(new
            {
                TotalSubscriptionCost = totalCost,
                EntryCount = entries.Count
            });
        }

        private int? GetCompanyId()
        {
            var claim = User.FindFirst("companyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpPost("invoices/{invoiceId}/archive")]
        public async Task<IActionResult> ArchiveInvoice(int invoiceId)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _subscription.ArchiveInvoiceAsync(companyId.Value, invoiceId);
            if (!result) return NotFound(new { message = "Invoice not found or cannot be archived" });

            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            await _audit.LogAsync(actorId, companyId.Value, "InvoiceArchived", "Invoice",
                invoiceId.ToString(), null, $"Invoice #{invoiceId} archived");

            return Ok(new { message = "Invoice archived successfully" });
        }

        [HttpDelete("invoices/{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(int invoiceId)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, error) = await _subscription.DeleteInvoiceAsync(companyId.Value, invoiceId);
            if (!success) return BadRequest(new { message = error });

            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            await _audit.LogAsync(actorId, companyId.Value, "InvoiceDeleted", "Invoice",
                invoiceId.ToString(), null, $"Invoice #{invoiceId} permanently deleted");

            return Ok(new { message = "Invoice deleted successfully" });
        }

        [HttpPost("downgrade")]
        public async Task<IActionResult> ScheduleDowngrade([FromBody] ScheduleDowngradeRequest request)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, error) = await _subscription.ScheduleDowngradeAsync(companyId.Value, request.PlanId);
            if (!success) return BadRequest(new { message = error });

            var (actorId, _) = ClaimsHelper.GetUserIdFromClaims(User);
            await _audit.LogAsync(actorId, companyId.Value, "DowngradeScheduled", "Subscription",
                companyId.ToString(), null, $"Downgrade to plan {request.PlanId} scheduled");

            return Ok(new { message = "Downgrade scheduled. It will take effect when your current plan expires." });
        }
    }

    public record ScheduleDowngradeRequest(int PlanId);
}
