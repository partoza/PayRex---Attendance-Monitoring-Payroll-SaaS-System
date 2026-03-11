namespace PayRexApplication.Enums
{
    /// <summary>
    /// Status for subscriptions
    /// </summary>
    public enum SubscriptionStatus
    {
        Active = 0,
        Expired = 1,
        Cancelled = 2,
        Trial = 3,
        GracePeriod = 4,
        PastDue = 5
    }
}
