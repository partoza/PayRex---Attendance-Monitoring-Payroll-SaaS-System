using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PayRexApplication.Services
{
    /// <summary>
    /// Service for sending emails via SMTP
    /// </summary>
    public interface IEmailService
    {
     /// <summary>
   /// Send password reset email
   /// </summary>
        Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
    }

    /// <summary>
    /// Implementation of email service using SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
     private readonly IConfiguration _config;
   private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
  {
  _config = config;
  _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
   {
        var smtpHost = _config["Smtp:Host"];
            var smtpPort = _config.GetValue<int>("Smtp:Port");
     var smtpUsername = _config["Smtp:Username"];
            var smtpPassword = _config["Smtp:Password"];
    var smtpFromEmail = _config["Smtp:FromEmail"];
    var smtpFromName = _config["Smtp:FromName"];
 var smtpEnableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);

        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUsername) || 
string.IsNullOrWhiteSpace(smtpPassword) || string.IsNullOrWhiteSpace(smtpFromEmail))
   {
   _logger.LogError("SMTP configuration is incomplete");
     throw new InvalidOperationException("SMTP is not properly configured");
            }

            // Defensive: trim and remove accidental spaces/newlines from password loaded from configuration
            smtpPassword = smtpPassword?.Trim();
            if (smtpPassword != null && smtpPassword.Contains(' '))
            {
                _logger.LogWarning("SMTP password contains whitespace characters; trimming spaces before use.");
                smtpPassword = smtpPassword.Replace(" ", "");
            }

          try
            {
           using var client = new SmtpClient(smtpHost, smtpPort)
     {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(smtpUsername, smtpPassword),
            EnableSsl = smtpEnableSsl,
            Timeout =100000 // ms
   };

         var mailMessage = new MailMessage
        {
                  From = new MailAddress(smtpFromEmail, smtpFromName ?? "PayRex"),
  Subject = "Password Reset Request - PayRex",
Body = GeneratePasswordResetEmailBody(resetUrl),
         IsBodyHtml = true
 };

     mailMessage.To.Add(toEmail);

                _logger.LogInformation("Sending password reset email to {Email} via {Host}:{Port}", toEmail, smtpHost, smtpPort);

                await client.SendMailAsync(mailMessage);
       _logger.LogInformation("Password reset email sent to {Email}", toEmail);
 }
            catch (SmtpException smtpEx)
            {
        _logger.LogError(smtpEx, "SMTP error sending password reset email to {Email}", toEmail);

 // Provide a helpful message for common Gmail issues
 if (smtpHost?.Contains("gmail", StringComparison.OrdinalIgnoreCase) == true)
 {
 _logger.LogError("If using Gmail SMTP, ensure you are using an App Password (not your regular account password) and that the account allows SMTP access.");
 }

 throw;
 }
            catch (Exception ex)
            {
        _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
  }
        }

        private string GeneratePasswordResetEmailBody(string resetUrl)
  {
         return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
   body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
.header {{ background: #1E88E5; color: white; padding: 20px; text-align: center; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 5px; margin-top: 20px; }}
        .button {{ 
            display: inline-block; 
            padding: 12px 30px; 
            background: #1E88E5; 
     color: white; 
    text-decoration: none; 
         border-radius: 5px; 
            margin: 20px 0; 
        }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
      <h1>Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello,</p>
            <p>We received a request to reset your password for your PayRex account.</p>
        <p>Click the button below to reset your password. This link will expire in 30 minutes.</p>
            <div style='text-align: center;'>
         <a href='{resetUrl}' class='button'>Reset Password</a>
   </div>
            <p>If the button doesn't work, copy and paste this URL into your browser:</p>
   <p style='word-break: break-all; color: #1E88E5;'>{resetUrl}</p>
            <p><strong>If you didn't request this password reset, please ignore this email.</strong> Your password will remain unchanged.</p>
        </div>
        <div class='footer'>
   <p>This is an automated message from PayRex. Please do not reply to this email.</p>
            <p>&copy; {DateTime.UtcNow.Year} PayRex. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
