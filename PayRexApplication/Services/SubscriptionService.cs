using Microsoft.EntityFrameworkCore;
using PayRexApplication.Data;
using PayRexApplication.DTOs;
using PayRexApplication.Enums;
using PayRexApplication.Models;

namespace PayRexApplication.Services
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto?> GetCompanySubscriptionAsync(int companyId);
        Task<List<BillingInvoiceDto>> GetCompanyInvoicesAsync(int companyId);
        Task<BillingInvoice> CreateRenewalInvoiceAsync(int companyId);
        Task<BillingInvoice> CreateUpgradeInvoiceAsync(int companyId, int newPlanId);
        Task<Payment> CreatePendingPaymentAsync(int invoiceId, int companyId, string sessionId);
        Task CheckAndUpdateSubscriptionStatusAsync(int companyId);
        Task<Subscription> CreateTrialSubscriptionAsync(int companyId, int planId);
        Task<bool> ProcessTestPaymentAsync(int paymentId, int companyId, string paymentMethod);
        Task<bool> ArchiveInvoiceAsync(int companyId, int invoiceId);
        Task<(bool Success, string Error)> DeleteInvoiceAsync(int companyId, int invoiceId);
    }

    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _db;
        private readonly IActivityLoggerService _audit;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(AppDbContext db, IActivityLoggerService audit, ILogger<SubscriptionService> logger)
        {
            _db = db;
            _audit = audit;
            _logger = logger;
        }

        public async Task<SubscriptionDto?> GetCompanySubscriptionAsync(int companyId)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (sub == null) return null;

            var now = DateTime.UtcNow;
            var daysRemaining = (int)Math.Max(0, (sub.EndDate - now).TotalDays);
            var isExpired = sub.EndDate < now;
            var graceEnd = sub.EndDate.AddDays(sub.GracePeriodDays);
            var isInGracePeriod = isExpired && now <= graceEnd;

            return new SubscriptionDto
            {
                SubscriptionId = sub.SubscriptionId,
                CompanyId = sub.CompanyId,
                PlanId = sub.PlanId,
                PlanName = sub.SubscriptionPlan.Name,
                PlanPrice = sub.SubscriptionPlan.Price,
                BillingCycle = sub.SubscriptionPlan.BillingCycle.ToString(),
                MaxEmployees = sub.SubscriptionPlan.MaxEmployees,
                Status = sub.Status.ToString(),
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                DaysRemaining = daysRemaining,
                AutoRenew = sub.AutoRenew,
                GracePeriodDays = sub.GracePeriodDays,
                IsExpired = isExpired && !isInGracePeriod,
                IsInGracePeriod = isInGracePeriod,
                IsTrialing = sub.Status == SubscriptionStatus.Trial
            };
        }

        public async Task<List<BillingInvoiceDto>> GetCompanyInvoicesAsync(int companyId)
        {
            return await _db.BillingInvoices
                .Where(i => i.CompanyId == companyId && i.Status != InvoiceStatus.Archived)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new BillingInvoiceDto
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber ?? "",
                    Amount = i.Amount,
                    Status = i.Status.ToString(),
                    DueDate = i.DueDate,
                    PaidAt = i.PaidAt,
                    CreatedAt = i.CreatedAt,
                    Description = i.Description,
                    PeriodStart = i.PeriodStart,
                    PeriodEnd = i.PeriodEnd
                })
                .ToListAsync();
        }

        public async Task<BillingInvoice> CreateRenewalInvoiceAsync(int companyId)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (sub == null)
                throw new InvalidOperationException("No subscription found for company");

            var plan = sub.SubscriptionPlan;
            var periodStart = sub.EndDate > DateTime.UtcNow ? sub.EndDate : DateTime.UtcNow;
            var periodEnd = periodStart.AddMonths(plan.BillingCycle == BillingCycle.Yearly ? 12 : 1);

            var invoice = new BillingInvoice
            {
                CompanyId = companyId,
                SubscriptionId = sub.SubscriptionId,
                Amount = plan.Price,
                DueDate = periodStart,
                Status = InvoiceStatus.Unpaid,
                InvoiceNumber = GenerateInvoiceNumber(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Description = $"Subscription renewal - {plan.Name} ({plan.BillingCycle})",
                CreatedAt = DateTime.UtcNow
            };

            _db.BillingInvoices.Add(invoice);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(null, companyId, "InvoiceCreated", "BillingInvoice",
                invoice.InvoiceId.ToString(), null, $"₱{invoice.Amount:N2}");

            return invoice;
        }

        public async Task<BillingInvoice> CreateUpgradeInvoiceAsync(int companyId, int newPlanId)
        {
            var newPlan = await _db.SubscriptionPlans.FindAsync(newPlanId)
                ?? throw new InvalidOperationException("Plan not found");

            var sub = await _db.Subscriptions
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            var periodStart = DateTime.UtcNow;
            var periodEnd = periodStart.AddMonths(newPlan.BillingCycle == BillingCycle.Yearly ? 12 : 1);

            var invoice = new BillingInvoice
            {
                CompanyId = companyId,
                SubscriptionId = sub?.SubscriptionId,
                Amount = newPlan.Price,
                DueDate = periodStart,
                Status = InvoiceStatus.Unpaid,
                InvoiceNumber = GenerateInvoiceNumber(),
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Description = $"Plan upgrade to {newPlan.Name} ({newPlan.BillingCycle})",
                CreatedAt = DateTime.UtcNow
            };

            _db.BillingInvoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Update company plan and subscription
            var company = await _db.Companies.FindAsync(companyId);
            if (company != null)
            {
                company.PlanId = newPlanId;
                company.UpdatedAt = DateTime.UtcNow;
            }

            if (sub != null)
            {
                sub.PlanId = newPlanId;
                sub.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Subscriptions.Add(new Subscription
                {
                    CompanyId = companyId,
                    PlanId = newPlanId,
                    StartDate = periodStart,
                    EndDate = periodEnd,
                    Status = SubscriptionStatus.Active,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync();
            return invoice;
        }

        public async Task<Payment> CreatePendingPaymentAsync(int invoiceId, int companyId, string sessionId)
        {
            var payment = new Payment
            {
                InvoiceId = invoiceId,
                CompanyId = companyId,
                Provider = "PayMongo",
                ReferenceNo = $"PR-{DateTime.UtcNow:yyyyMMddHHmmss}-{invoiceId}",
                Status = PaymentStatus.Pending,
                PayMongoCheckoutSessionId = sessionId,
                Currency = "PHP",
                CreatedAt = DateTime.UtcNow,
                Amount = (await _db.BillingInvoices.FindAsync(invoiceId))?.Amount
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
            return payment;
        }

        public async Task CheckAndUpdateSubscriptionStatusAsync(int companyId)
        {
            var sub = await _db.Subscriptions
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            if (sub == null) return;

            var now = DateTime.UtcNow;

            if (sub.Status == SubscriptionStatus.Active || sub.Status == SubscriptionStatus.Trial)
            {
                if (sub.EndDate < now)
                {
                    var graceEnd = sub.EndDate.AddDays(sub.GracePeriodDays);
                    if (now <= graceEnd)
                    {
                        sub.Status = SubscriptionStatus.GracePeriod;
                    }
                    else
                    {
                        sub.Status = SubscriptionStatus.Expired;
                    }
                    sub.UpdatedAt = now;
                    await _db.SaveChangesAsync();
                }
            }
            else if (sub.Status == SubscriptionStatus.GracePeriod)
            {
                var graceEnd = sub.EndDate.AddDays(sub.GracePeriodDays);
                if (now > graceEnd)
                {
                    sub.Status = SubscriptionStatus.Expired;
                    sub.UpdatedAt = now;
                    await _db.SaveChangesAsync();
                }
            }
        }

        public async Task<Subscription> CreateTrialSubscriptionAsync(int companyId, int planId)
        {
            var now = DateTime.UtcNow;
            var subscription = new Subscription
            {
                CompanyId = companyId,
                PlanId = planId,
                StartDate = now,
                EndDate = now.AddDays(14),
                Status = SubscriptionStatus.Trial,
                AutoRenew = true,
                GracePeriodDays = 7,
                CreatedAt = now
            };

            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();

            await _audit.LogAsync(null, companyId, "TrialStarted", "Subscription",
                subscription.SubscriptionId.ToString(), null, "14-day trial");

            return subscription;
        }

        private static string GenerateInvoiceNumber()
        {
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}";
        }

        public async Task<bool> ProcessTestPaymentAsync(int paymentId, int companyId, string paymentMethod)
        {
            var payment = await _db.Payments
                .Include(p => p.BillingInvoice)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.CompanyId == companyId);

            if (payment == null || payment.Status == PaymentStatus.Success)
                return false;

            // Mark payment as success
            payment.Status = PaymentStatus.Success;
            payment.PaidAt = DateTime.UtcNow;
            payment.PaymentMethod = paymentMethod;
            payment.Provider = "TestMode";
            payment.Description = $"Test payment via {paymentMethod}";

            // Mark invoice as paid
            var invoice = payment.BillingInvoice;
            if (invoice != null)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTime.UtcNow;
                invoice.UpdatedAt = DateTime.UtcNow;
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

            await _audit.LogAsync(null, companyId, "TestPaymentReceived", "Payment",
                payment.PaymentId.ToString(), null, $"₱{payment.Amount:N2} via {paymentMethod} (test)",
                null, null);

            return true;
        }

        public async Task<bool> ArchiveInvoiceAsync(int companyId, int invoiceId)
        {
            var invoice = await _db.BillingInvoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.CompanyId == companyId);

            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.Archived;
            invoice.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Error)> DeleteInvoiceAsync(int companyId, int invoiceId)
        {
            var invoice = await _db.BillingInvoices
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.CompanyId == companyId);

            if (invoice == null) return (false, "Invoice not found.");
            if (invoice.Status != InvoiceStatus.Unpaid)
                return (false, "Only unpaid invoices can be deleted.");

            _db.BillingInvoices.Remove(invoice);
            await _db.SaveChangesAsync();
            return (true, "");
        }
    }
}
