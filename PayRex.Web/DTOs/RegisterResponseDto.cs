namespace PayRex.Web.DTOs
{
    public class RegisterResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string CompanyId { get; set; } = string.Empty;
    }
}
