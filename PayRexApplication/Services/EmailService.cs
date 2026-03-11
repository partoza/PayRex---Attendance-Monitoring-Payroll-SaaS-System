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

        /// <summary>
        /// Send welcome email with generated password to new employee
        /// </summary>
        Task SendWelcomeEmailAsync(string toEmail, string employeeName, string companyName, string password, string? companyLogoUrl = null);
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

        public async Task SendWelcomeEmailAsync(string toEmail, string employeeName, string companyName, string password, string? companyLogoUrl = null)
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

            smtpPassword = smtpPassword?.Trim();
            if (smtpPassword != null && smtpPassword.Contains(' '))
            {
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
                    Timeout = 100000
                };

                var frontendUrls = _config.GetSection("AppSettings:FrontendUrls").Get<string[]>() ?? new string[0];
                var frontendBase = frontendUrls.Length > 0 ? frontendUrls[0] : "";
                var loginUrl = string.IsNullOrWhiteSpace(frontendBase) ? "/" : frontendBase.TrimEnd('/') + "/login";
                var headerBgUrl = "https://res.cloudinary.com/dxyhmtaqw/image/upload/v1771694059/loginbg_ffsmqq.png";

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpFromEmail, smtpFromName ?? "PayRex"),
                    Subject = $"Welcome to {companyName} - Your Account Details",
                    Body = GenerateWelcomeEmailBody(employeeName, companyName, password, toEmail, companyLogoUrl, loginUrl, headerBgUrl),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                _logger.LogInformation("Sending welcome email to {Email} via {Host}:{Port}", toEmail, smtpHost, smtpPort);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Welcome email sent to {Email}", toEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "SMTP error sending welcome email to {Email}", toEmail);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", toEmail);
                throw;
            }
        }

        private string GenerateWelcomeEmailBody(string employeeName, string companyName, string password, string email, string? logoUrl, string loginUrl, string headerBgUrl)
        {
            var logoHtml = !string.IsNullOrEmpty(logoUrl)
                ? $"<img src='{logoUrl}' alt='{companyName}' style='max-height:60px;margin-bottom:10px;' /><br/>"
                : "";

            var loginButton = !string.IsNullOrWhiteSpace(loginUrl) ? $"<div style='text-align:center;margin:18px 0;'><a href='{loginUrl}' style='display:inline-block;padding:12px 28px;background:#1E88E5;color:#fff;text-decoration:none;border-radius:6px;font-weight:600;'>Go to Login</a></div>" : "";

            var headerStyle = !string.IsNullOrWhiteSpace(headerBgUrl)
                ? $"background-image:url('{headerBgUrl}');background-size:cover;background-position:center;"
                : "background: linear-gradient(135deg, #1E88E5, #1565C0);";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background: #f5f7fa; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #1E88E5, #1565C0); color: white; padding: 30px; text-align: center; border-radius: 12px 12px 0 0; }}
        .content {{ background: white; padding: 30px; border-radius: 0 0 12px 12px; box-shadow: 0 2px 12px rgba(0,0,0,0.08); }}
        .credential-box {{ background: #f8f9fa; border: 1px solid #e9ecef; border-radius: 8px; padding: 20px; margin: 20px 0; }}
        .credential-box p {{ margin: 8px 0; }}
        .credential-label {{ font-size: 12px; color: #666; text-transform: uppercase; letter-spacing: 0.5px; }}
        .credential-value {{ font-size: 16px; font-weight: 600; color: #1E88E5; font-family: 'Courier New', monospace; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffc107; border-radius: 8px; padding: 15px; margin: 20px 0; font-size: 14px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header' style='{headerStyle}'>
            {logoHtml}
            <h1 style='margin:0;font-size:24px;color:#000;'>Welcome to {companyName}!</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{employeeName}</strong>,</p>
            <p>Your employee account has been created on <strong>PayRex</strong>. Below are your login credentials:</p>
            
            <div class='credential-box'>
                <p class='credential-label'>Email</p>
                <p class='credential-value'>{email}</p>
                <p class='credential-label' style='margin-top:15px;'>Temporary Password</p>
                <p class='credential-value'>{password}</p>
            </div>

            <div class='warning'>
                ⚠️ <strong>Important:</strong> You will be required to change your password upon first login. Please keep your new password secure and do not share it with anyone.
            </div>

            <p>If you have any questions, please contact your HR administrator.</p>
            {loginButton}
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
