namespace PayRexApplication.Enums
{
    /// <summary>
    /// Status for billing invoices
    /// </summary>
    public enum InvoiceStatus
    {
        Unpaid = 0,
      Paid = 1,
       Failed = 2,
       Voided = 3,
       Archived = 4
    }
}
