namespace PayRex.Web.Models
{
    /// <summary>
    /// Represents a navigation menu item with role-based access control
    /// </summary>
    public class MenuItem
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();
        public string? Badge { get; set; }
    public string? BadgeColor { get; set; }
        /// <summary>
      /// Optional section header displayed above this menu item to group related items
   /// </summary>
public string? SectionHeader { get; set; }
    }
}
