namespace PayRex.Web.Services
{
    public interface IBillingApiService
    {
        Task<SubscriptionInfoDto?> GetSubscriptionAsync(string token);
        Task<List<InvoiceItemDto>> GetInvoicesAsync(string token);
        Task<CheckoutResultItemDto?> CreateCheckoutAsync(string token, int? invoiceId = null, int? planId = null);
        Task<CheckoutResultItemDto?> RenewSubscriptionAsync(string token);
    }

    public class SubscriptionInfoDto
    {
        public int SubscriptionId { get; set; }
        public int CompanyId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public string BillingCycle { get; set; } = string.Empty;
        public int MaxEmployees { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public bool AutoRenew { get; set; }
        public int GracePeriodDays { get; set; }
        public bool IsExpired { get; set; }
        public bool IsInGracePeriod { get; set; }
        public bool IsTrialing { get; set; }
    }

    public class InvoiceItemDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
        public DateTime? PeriodStart { get; set; }
        public DateTime? PeriodEnd { get; set; }
    }

    public class CheckoutResultItemDto
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public string CheckoutSessionId { get; set; } = string.Empty;
        public int PaymentId { get; set; }
    }
}
