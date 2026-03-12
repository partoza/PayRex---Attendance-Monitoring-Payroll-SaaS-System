namespace PayRexApplication.DTOs
{
    public class SubscriptionDto
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
        public int? PendingDowngradePlanId { get; set; }
        public string? PendingDowngradePlanName { get; set; }
    }

    public class BillingInvoiceDto
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

    public class CheckoutRequestDto
    {
        public int? InvoiceId { get; set; }
        public int? PlanId { get; set; }
    }

    public class CheckoutResultDto
    {
        public string CheckoutUrl { get; set; } = string.Empty;
        public string CheckoutSessionId { get; set; } = string.Empty;
        public int PaymentId { get; set; }
    }

    public class PaymentStatusDto
    {
        public int PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class TestConfirmDto
    {
        public string? PaymentMethod { get; set; }
    }

    public class AuditLogItemDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? IpAddress { get; set; }
        public string? Details { get; set; }
        public string? Role { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
    }

    public class AuditLogListDto
    {
        public List<AuditLogItemDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public AuditLogStatsDto Stats { get; set; } = new();
    }

    public class AuditLogStatsDto
    {
        public int FailedLogins { get; set; }
        public int Registrations { get; set; }
        public int PasswordResets { get; set; }
        public int SuccessfulLogins { get; set; }
    }
}
