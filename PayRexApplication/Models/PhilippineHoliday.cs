namespace PayRexApplication.Models
{
    /// <summary>
    /// Represents a Philippine public holiday from the Nager.Date API
    /// </summary>
    public class PhilippineHoliday
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Fixed { get; set; }
        public string[]? Types { get; set; }
    }
}
