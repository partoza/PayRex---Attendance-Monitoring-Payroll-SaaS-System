namespace PayRex.Web.DTOs
{
    public class UserInfoDto
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? CompanyId { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
