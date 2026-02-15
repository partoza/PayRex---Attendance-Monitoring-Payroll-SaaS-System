namespace PayRex.Web.DTOs
{
 public class ClientTotpEnableResponseDto
 {
 public bool Success { get; set; }
 public string? Message { get; set; }
 public List<string>? RecoveryCodes { get; set; }
 }
}