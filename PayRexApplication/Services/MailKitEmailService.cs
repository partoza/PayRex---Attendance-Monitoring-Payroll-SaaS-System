namespace PayRexApplication.Services
{
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="MailKitEmailService" />
    /// </summary>
    public class MailKitEmailService : IEmailService
    {
        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<MailKitEmailService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailKitEmailService"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IConfiguration"/></param>
        /// <param name="logger">The logger<see cref="ILogger{MailKitEmailService}"/></param>
        public MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// The SendPasswordResetEmailAsync
        /// </summary>
        /// <param name="toEmail">The toEmail<see cref="string"/></param>
        /// <param name="resetUrl">The resetUrl<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = _config.GetValue<int>("Smtp:Port");
            var smtpUsername = _config["Smtp:Username"];
            var smtpPassword = _config["Smtp:Password"]?.Trim();
            var smtpFromEmail = _config["Smtp:FromEmail"];
            var smtpFromName = _config["Smtp:FromName"] ?? "PayRex";
            var enableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUsername) ||
            string.IsNullOrWhiteSpace(smtpPassword) || string.IsNullOrWhiteSpace(smtpFromEmail))
            {
                _logger.LogError("SMTP configuration is incomplete");
                throw new InvalidOperationException("SMTP is not properly configured");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpFromName, smtpFromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Password Reset Request - PayRex";

            var builder = new BodyBuilder { HtmlBody = GeneratePasswordResetEmailBody(resetUrl) };
            message.Body = builder.ToMessageBody();

            try
            {
                using var client = new SmtpClient();

                // Accept all SSL certificates (useful for dev/test). In production consider stricter checks.
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Password reset email sent to {Email} via MailKit", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailKit failed to send email to {Email}", toEmail);
                throw;
            }
        }

        /// <summary>
        /// The GeneratePasswordResetEmailBody
        /// </summary>
        /// <param name="resetUrl">The resetUrl<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string GeneratePasswordResetEmailBody(string resetUrl)
        {
            // Use provided Cloudinary image URL for the email hero image
            var imageUrl = "https://res.cloudinary.com/dxyhmtaqw/image/upload/v1770396183/emailBg_ifrtxr.png";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Password Reset</title>
</head>

<body style=""margin:0;padding:0;background-color:#f4f6f8;font-family:Arial, Helvetica, sans-serif;"">

<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f8;padding:40px 0;"">
<tr>
<td align=""center"">

<table width=""100%"" cellpadding=""0"" cellspacing=""0""
       style=""max-width:520px;background:#ffffff;border-radius:10px;overflow:hidden;box-shadow:0 4px 12px rgba(0,0,0,0.08);"">

<!-- HERO IMAGE -->
<tr>
<td style=""line-height:0;"">
    <img src=""{imageUrl}""
         alt=""Reset Password""
         style=""width:100%;height:220px;object-fit:cover;display:block;"">
</td>
</tr>


<!-- TITLE -->
<tr>
<td style=""padding:25px 30px 10px;text-align:center;"">
    <h2 style=""margin:0;color:#111;"">Reset Your Password</h2>
</td>
</tr>

<!-- CONTENT -->
<tr>
<td style=""padding:10px 30px;color:#444;font-size:14px;line-height:1.6;text-align:center;"">

<p>
We received a request to reset your PayRex account password.
Click the button below to continue. This link expires in <b>30 minutes</b>.
</p>

</td>
</tr>

<!-- BUTTON -->
<tr>
<td align=""center"" style=""padding:20px 30px;"">
<a href=""{resetUrl}""
   style=""
   background:#2563eb;
   color:#ffffff;
   padding:14px 36px;
   text-decoration:none;
   border-radius:6px;
   font-weight:bold;
   display:inline-block;"">
Reset Password
</a>
</td>
</tr>

<!-- FALLBACK -->
<tr>
<td style=""padding:10px 30px;color:#666;font-size:12px;text-align:center;"">
<p>If the button doesn�t work, copy and paste this link:</p>

<p style=""word-break:break-all;"">
<a href=""{resetUrl}"" style=""color:#2563eb;"">{resetUrl}</a>
</p>
</td>
</tr>

<!-- FOOTER -->
<tr>
<td style=""padding:20px 30px 30px;color:#888;font-size:12px;text-align:center;"">
<p>If you didn�t request this password reset, you can safely ignore this email.</p>
<p>� PayRex</p>
</td>
</tr>

</table>

</td>
</tr>
</table>

</body>
</html>
";
        }

        public Task SendWelcomeEmailAsync(string toEmail, string employeeName, string companyName, string password, string? companyLogoUrl = null)
        {
            var smtpHost = _config["Smtp:Host"];
            var smtpPort = _config.GetValue<int>("Smtp:Port");
            var smtpUsername = _config["Smtp:Username"];
            var smtpPassword = _config["Smtp:Password"]?.Trim();
            var smtpFromEmail = _config["Smtp:FromEmail"];
            var smtpFromName = _config["Smtp:FromName"] ?? "PayRex";
            var enableSsl = _config.GetValue<bool>("Smtp:EnableSsl", true);

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUsername) ||
                string.IsNullOrWhiteSpace(smtpPassword) || string.IsNullOrWhiteSpace(smtpFromEmail))
            {
                _logger.LogError("SMTP configuration is incomplete");
                throw new InvalidOperationException("SMTP is not properly configured");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtpFromName, smtpFromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = $"Welcome to {companyName} - Your Account Details";

            var builder = new BodyBuilder();
            builder.HtmlBody = GenerateWelcomeEmailBody(employeeName, companyName, password, toEmail, companyLogoUrl);
            message.Body = builder.ToMessageBody();

            return Task.Run(async () =>
            {
                try
                {
                    using var client = new SmtpClient();
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Welcome email sent to {Email} via MailKit", toEmail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MailKit failed to send welcome email to {Email}", toEmail);
                    throw;
                }
            });
        }

        /// <summary>
        /// Generate HTML body for welcome email (same style as EmailService)
        /// </summary>
        private string GenerateWelcomeEmailBody(string employeeName, string companyName, string password, string email, string? logoUrl)
        {
            var logoHtml = !string.IsNullOrEmpty(logoUrl)
                ? $"<img src='{logoUrl}' alt='{companyName}' style='max-height:60px;margin-bottom:10px;' /><br/>"
                : "";

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
        <div class='header'>
            {logoHtml}
            <h1 style='margin:0;font-size:24px;'>Welcome to {companyName}!</h1>
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
