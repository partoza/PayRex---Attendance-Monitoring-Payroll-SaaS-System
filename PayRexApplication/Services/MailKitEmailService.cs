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
<p>If the button doesn’t work, copy and paste this link:</p>

<p style=""word-break:break-all;"">
<a href=""{resetUrl}"" style=""color:#2563eb;"">{resetUrl}</a>
</p>
</td>
</tr>

<!-- FOOTER -->
<tr>
<td style=""padding:20px 30px 30px;color:#888;font-size:12px;text-align:center;"">
<p>If you didn’t request this password reset, you can safely ignore this email.</p>
<p>© PayRex</p>
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
    }
}
