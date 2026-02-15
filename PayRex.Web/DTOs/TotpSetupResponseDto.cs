namespace PayRex.Web.DTOs
{
 public class ClientTotpSetupResponseDto
 {
 public bool Success { get; set; }
 public string? Message { get; set; }
 public string SecretKey { get; set; } = string.Empty;
 public string QrCodeUri { get; set; } = string.Empty;
 public string ManualEntryKey { get; set; } = string.Empty;
 }
}