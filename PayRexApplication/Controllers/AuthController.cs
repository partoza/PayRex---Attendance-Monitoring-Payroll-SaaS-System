namespace PayRexApplication.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using PayRexApplication.Data;
    using PayRexApplication.DTOs;
    using PayRexApplication.Enums;
    using PayRexApplication.Models;
    using PayRexApplication.Services;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;


    /// <summary>
    /// Defines the <see cref="AuthController" />
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]


    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Defines the MaxLoginAttempts
        /// </summary>
        private const int MaxLoginAttempts = 5;

        /// <summary>
        /// Defines the LockoutMinutes
        /// </summary>
        private const int LockoutMinutes = 1;

        /// <summary>
        /// Defines the PasswordResetTokenExpiryMinutes
        /// </summary>
        private const int PasswordResetTokenExpiryMinutes = 30;

        /// <summary>
        /// Defines the _db
        /// </summary>
        private readonly AppDbContext _db;

        /// <summary>
        /// Defines the _config
        /// </summary>
        private readonly IConfiguration _config;

        /// <summary>
        /// Defines the _logger
        /// </summary>
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Defines the _totpService
        /// </summary>
        private readonly ITotpService _totpService;

        /// <summary>
        /// Defines the _emailService
        /// </summary>
        private readonly IEmailService _emailService;

        /// <summary>
        /// Defines the _tokenService
        /// </summary>
        private readonly IPasswordResetTokenService _tokenService;

        /// <summary>
        /// Defines the _protector
        /// </summary>
        private readonly IDataProtector _protector;

        /// <summary>
        /// Defines the _activityLogger
        /// </summary>
        private readonly IActivityLoggerService _activityLogger;

        private readonly ICloudinaryService _cloudinary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="db">The db<see cref="AppDbContext"/></param>
        /// <param name="config">The config<see cref="IConfiguration"/></param>
        /// <param name="logger">The logger<see cref="ILogger{AuthController}"/></param>
        /// <param name="totpService">The totpService<see cref="ITotpService"/></param>
        /// <param name="emailService">The emailService<see cref="IEmailService"/></param>
        /// <param name="tokenService">The tokenService<see cref="IPasswordResetTokenService"/></param>
        /// <param name="dataProtectionProvider">The dataProtectionProvider<see cref="IDataProtectionProvider"/></param>
        /// <param name="activityLogger">The activityLogger<see cref="IActivityLoggerService"/></param>
        public AuthController(
        AppDbContext db,
            IConfiguration config,
       ILogger<AuthController> logger,
        ITotpService totpService,
      IEmailService emailService,
       IPasswordResetTokenService tokenService,
       IDataProtectionProvider dataProtectionProvider,
       IActivityLoggerService activityLogger,
            ICloudinaryService cloudinary)
        {
            _db = db;
            _config = config;
            _logger = logger;
            _totpService = totpService;
            _emailService = emailService;
            _tokenService = tokenService;
            _protector = dataProtectionProvider.CreateProtector("PayRex.TotpSecrets");
   _activityLogger = activityLogger;
            _cloudinary = cloudinary;
        }

        /// <summary>
        /// Generate a unique 4-digit company ID
        /// </summary>
        /// <returns>The <see cref="Task{string}"/></returns>
        private async Task<string> GenerateUniqueCompanyIdAsync()
        {
            var random = new Random();
            string companyCode;
            do
            {
                companyCode = random.Next(0, 10000).ToString("D4"); // 0000-9999
            } while (await _db.Companies.AnyAsync(c => c.CompanyCode == companyCode));

            return companyCode;
        }

        /// <summary>
        /// Register a new company user with their own company
        /// </summary>
        /// <param name="dto">The dto<see cref="RegisterDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalizedEmail = dto.Email.Trim().ToLower();

            var userExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (userExists) return Conflict(new { message = "Email already registered" });

            // Generate unique 4-digit company code
            var companyCode = await GenerateUniqueCompanyIdAsync();

            var company = new Company
            {
                CompanyCode = companyCode,
                CompanyName = $"{dto.FirstName} {dto.LastName}'s Company",
                PlanId = 1,
                Status = CompanyStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            var user = new User
            {
                CompanyId = company.CompanyId,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Admin,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}, CompanyId: {CompanyId}", user.Email, company.CompanyId);

            return CreatedAtAction(null, new
            {
                id = user.UserId,
                email = user.Email,
                companyId = company.CompanyId
            });
        }

        /// <summary>
        /// Login for all users (SuperAdmin, Admin, HR)
        /// </summary>
        /// <param name="dto">The dto<see cref="LoginDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalizedEmail = dto.Email.Trim().ToLower();
            var ipAddress = GetClientIpAddress();
            var userAgent = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();

            // Check for account lockout
            var lockoutInfo = await GetAccountLockoutInfoAsync(normalizedEmail);
            if (lockoutInfo.IsLocked)
            {
                _logger.LogWarning("Account locked for email {Email}. Unlock in {Seconds} seconds", normalizedEmail, lockoutInfo.RemainingSeconds);
                await PersistLoginAttemptAsync(normalizedEmail, null, ipAddress, userAgent, false, "AccountLocked");
                return Ok(new LoginResponseDto
                {
                    IsLockedOut = true,
                    LockoutRemainingSeconds = lockoutInfo.RemainingSeconds,
                    Message = $"Account locked. Try again in {lockoutInfo.RemainingSeconds} seconds."
                });
            }

            // Find user by email
            var user = await _db.Users.Include(u => u.Company).SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user != null)
            {
                _logger.LogInformation("User found for email {Email}. Status={Status}, Role={Role}", dto.Email, user.Status, user.Role);

                if (user.Status == UserStatus.Suspended)
                {
                    _logger.LogWarning("User {Email} is suspended", dto.Email);
                    await PersistLoginAttemptAsync(normalizedEmail, user.UserId, ipAddress, userAgent, false, "Suspended");
                    return Forbid();
                }

                var verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                _logger.LogInformation("Password verification result for user {Email}: {Result}", dto.Email, verified);

                if (!verified)
                {
                    await RecordFailedLoginAttemptAsync(normalizedEmail, user.UserId, ipAddress);
                    await PersistLoginAttemptAsync(normalizedEmail, user.UserId, ipAddress, userAgent, false, "InvalidCredentials");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Check if TOTP is enabled
                if (user.IsTwoFactorEnabled && !string.IsNullOrEmpty(user.TotpSecretKey))
                {
                    _logger.LogInformation("TOTP required for user {Email}", dto.Email);
                    return Ok(new LoginResponseDto
                    {
                        RequireTotp = true,
                        Message = "Please provide your TOTP code",
                        Email = user.Email
                    });
                }

                // Clear any login attempts on success
                await ClearLoginAttemptsAsync(normalizedEmail);
                await PersistLoginAttemptAsync(normalizedEmail, user.UserId, ipAddress, userAgent, true, null);

                var token = GenerateJwtForUser(user);

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    Id = user.UserId,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_config.GetSection("Jwt").GetValue<int>("ExpiryMinutes")),
                    MustChangePassword = user.MustChangePassword
                });
            }

            // No user found - still record attempt to prevent enumeration
            await RecordFailedLoginAttemptAsync(normalizedEmail, null, ipAddress);
            await PersistLoginAttemptAsync(normalizedEmail, null, ipAddress, userAgent, false, "UserNotFound");
            _logger.LogWarning("Login attempt failed. No user found for email {Email}", dto.Email);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        /// <summary>
        /// The GenerateJwtForUser
        /// </summary>
        /// <param name="user">The user<see cref="User"/></param>
        /// <returns>The <see cref="string"/></returns>
        private string GenerateJwtForUser(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiryMinutes = jwtSection.GetValue<int>("ExpiryMinutes");

            // Validate configuration
            if (string.IsNullOrEmpty(key))
            {
                _logger.LogError("JWT Key configuration is missing. Unable to generate token for user {Email}", user?.Email);
                return string.Empty; // caller must detect empty and handle
            }

            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
        {
              new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
      new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
         // Will add role claim normalized below
         new Claim("uid", user.UserId.ToString()),
new Claim("userId", user.UserId.ToString()),
    new Claim("companyId", user.CompanyId.ToString()),
                new Claim("isSuperAdmin", (user.Role == UserRole.SuperAdmin).ToString().ToLower()),
                new Claim("isTwoFactorEnabled", user.IsTwoFactorEnabled.ToString()),
                new Claim("mustChangePassword", user.MustChangePassword.ToString().ToLower())
        };

            // Normalize role string to expected values used in Authorize attributes (e.g., "HR")
            string normalizedRole = user.Role == UserRole.Hr ? "HR" : user.Role.ToString();
            // Ensure a single role claim with normalized value
            claims.Add(new Claim(ClaimTypes.Role, normalizedRole));

            // Add profile image URL if exists
            if (!string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                claims.Add(new Claim("profileImage", user.ProfileImageUrl));
            }

            // Add company logo URL if exists
            if (!string.IsNullOrEmpty(user.Company?.LogoUrl))
            {
                claims.Add(new Claim("companyLogo", user.Company.LogoUrl));
            }

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
    issuer: issuer,
    audience: audience,
                claims: claims,
         notBefore: now,
         expires: expiresAt,
      signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Get current user info from JWT token
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out var parsedUserId))
            {
                var user = await _db.Users.FindAsync(parsedUserId);
                if (user != null)
                {
                    return Ok(new
                    {
                        Id = user.UserId.ToString(),
                        Email = user.Email,
                        Role = user.Role.ToString(),
                        CompanyId = user.CompanyId,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                        MustChangePassword = user.MustChangePassword
                    });
                }
            }

            // Fallback if user not found in database
            return Ok(new
            {
                Id = userId,
                Email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value,
                CompanyId = User.FindFirst("companyId")?.Value,
                FirstName = "",
                LastName = "",
                IsTwoFactorEnabled = false
            });
        }

        /// <summary>
        /// Protected endpoint for SuperAdmin and Company Admin
        /// </summary>
        /// <returns>The <see cref="IActionResult"/></returns>
        [HttpGet("admin-only")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new { message = "You have access to admin resources" });
        }

        /// <summary>
        /// Get account lockout information
        /// </summary>
        /// <param name="email">The email<see cref="string"/></param>
        /// <returns>The <see cref="Task{(bool IsLocked, int RemainingSeconds)}"/></returns>
        private async Task<(bool IsLocked, int RemainingSeconds)> GetAccountLockoutInfoAsync(string email)
        {
            // Only look at the lockout-tracking row (the one with AttemptCount > 0 or IsLocked = true)
       // Exclude audit-only rows (AttemptCount == 0 && IsLocked == false && Success is set)
 var attempt = await _db.UserLoginAttempts
   .Where(a => a.Email.ToLower() == email.ToLower() && a.Reason == null)
   .FirstOrDefaultAsync();

         if (attempt == null) return (false, 0);

         if (attempt.IsLocked && attempt.LockUntil.HasValue)
            {
      if (attempt.LockUntil.Value > DateTime.UtcNow)
  {
     var remainingSeconds = (int)(attempt.LockUntil.Value - DateTime.UtcNow).TotalSeconds;
     return (true, remainingSeconds);
   }

        // Lock expired - reset
 attempt.IsLocked = false;
   attempt.LockUntil = null;
                attempt.AttemptCount = 0;
    await _db.SaveChangesAsync();
            }

       return (false, 0);
 }

     /// <summary>
        /// Record a failed login attempt
        /// </summary>
  private async Task RecordFailedLoginAttemptAsync(string email, int? userId, string? ipAddress)
      {
    // Only update the lockout-tracking row (Reason == null)
        var attempt = await _db.UserLoginAttempts
        .Where(a => a.Email.ToLower() == email.ToLower() && a.Reason == null)
     .FirstOrDefaultAsync();

        if (attempt == null)
    {
    attempt = new UserLoginAttempt
              {
 Email = email,
        UserId = userId,
             IpAddress = ipAddress,
    AttemptCount = 1,
         IsLocked = false,
   LastAttemptAt = DateTime.UtcNow,
     CreatedAt = DateTime.UtcNow
     };
     _db.UserLoginAttempts.Add(attempt);
     }
            else
            {
        attempt.AttemptCount++;
            attempt.IpAddress = ipAddress;
          attempt.LastAttemptAt = DateTime.UtcNow;

  if (attempt.AttemptCount >= MaxLoginAttempts)
  {
      attempt.IsLocked = true;
        attempt.LockUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
   attempt.AttemptCount = 0;
             _logger.LogWarning("Account locked for email {Email} due to {Max} failed attempts", email, MaxLoginAttempts);
                }
    }

          await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Clear login attempts on successful login
        /// </summary>
        private async Task ClearLoginAttemptsAsync(string email)
    {
            var attempt = await _db.UserLoginAttempts
     .Where(a => a.Email.ToLower() == email.ToLower() && a.Reason == null)
  .FirstOrDefaultAsync();

     if (attempt != null)
{
         attempt.AttemptCount = 0;
                attempt.IsLocked = false;
             attempt.LockUntil = null;
          await _db.SaveChangesAsync();
      }
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        /// <returns>The <see cref="string?"/></returns>
        private string? GetClientIpAddress()
        {
            var forwardedFor = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        /// <summary>
 /// Persist an individual login attempt record for audit purposes.
        /// </summary>
        private async Task PersistLoginAttemptAsync(string email, int? userId, string? ipAddress, string? userAgent, bool success, string? reason)
 {
     try
            {
   var attempt = new UserLoginAttempt
   {
   Email = email,
         UserId = userId,
 IpAddress = ipAddress,
          UserAgent = userAgent?.Length > 1024 ? userAgent[..1024] : userAgent,
         Success = success,
         Reason = reason ?? (success ? "Success" : "Unknown"),
         Timestamp = DateTime.UtcNow,
 AttemptCount = 0,
  IsLocked = false,
        CreatedAt = DateTime.UtcNow
           };
  _db.UserLoginAttempts.Add(attempt);
   await _db.SaveChangesAsync();
      }
catch (Exception ex)
         {
        _logger.LogError(ex, "Failed to persist login attempt for {Email}", email);
   }
 }

      // ==========================================
        // Change Password Endpoint
     // ==========================================

        /// <summary>
      /// Change password for authenticated user (used for forced password change and voluntary change)
    /// </summary>
     [HttpPost("change-password")]
     [Authorize]
     public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
    if (!ModelState.IsValid) return BadRequest(ModelState);

  var userId = User.FindFirst("uid")?.Value;
  if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId))
   return Unauthorized();

      var user = await _db.Users.FindAsync(parsedUserId);
   if (user == null) return NotFound(new { message = "User not found" });

  if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
     return BadRequest(new { message = "Current password is incorrect" });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
      user.MustChangePassword = false;
          user.LastPasswordChangeAt = DateTime.UtcNow;
user.UpdatedAt = DateTime.UtcNow;
   await _db.SaveChangesAsync();

            await _activityLogger.LogAsync(user.UserId, user.CompanyId, "PasswordChanged",
       "User", user.UserId.ToString(), null, null,
       GetClientIpAddress(), HttpContext.Request.Headers["User-Agent"].FirstOrDefault(),
        user.Role.ToString(), "User");

    _logger.LogInformation("Password changed for user {Email}", user.Email);
            return Ok(new { message = "Password changed successfully" });
    }

      // ==========================================
        // Password Reset Endpoints
     // ==========================================

        /// <summary>
        /// Request password reset - sends email with reset link
        /// </summary>
        /// <param name="dto">The dto<see cref="ForgotPasswordDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalizedEmail = dto.Email.Trim().ToLower();
            // Resolve frontend URL: support either a single FrontendUrl or an array FrontendUrls
            var frontendUrl = _config.GetValue<string>("AppSettings:FrontendUrl");
            if (string.IsNullOrEmpty(frontendUrl))
            {
                var urls = _config.GetSection("AppSettings:FrontendUrls").Get<string[]?>();
                if (urls != null && urls.Length >0)
                {
                    frontendUrl = urls[0];
                }
            }
            frontendUrl ??= "https://localhost:7002";
            var tokenExpiryMinutes = _config.GetSection("Security").GetValue<int>("PasswordResetTokenExpiryMinutes");
            if (tokenExpiryMinutes <=0) tokenExpiryMinutes = PasswordResetTokenExpiryMinutes;

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user != null)
            {
                var token = _tokenService.GenerateToken();
                var tokenHash = _tokenService.HashToken(token);

                user.PasswordResetTokenHash = tokenHash;
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(tokenExpiryMinutes);
                await _db.SaveChangesAsync();

                var resetLink = $"{frontendUrl}/Auth/ResetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
                await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink);

                _logger.LogInformation("Password reset requested for user {Email}", dto.Email);
            }

            return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
        }

        /// <summary>
        /// Reset password with token from email
        /// </summary>
        /// <param name="dto">The dto<see cref="ResetPasswordDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalizedEmail = dto.Email.Trim().ToLower();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user != null)
            {
                _logger.LogDebug("User found for reset: {Email}, TokenExpiry={Expiry}, Now={Now}, HasHash={HasHash}",
                         dto.Email, user.PasswordResetTokenExpiry, DateTime.UtcNow, !string.IsNullOrEmpty(user.PasswordResetTokenHash));

                if (string.IsNullOrEmpty(user.PasswordResetTokenHash) || !user.PasswordResetTokenExpiry.HasValue)
                {
                    _logger.LogWarning("Password reset attempted without valid token for user {Email}", dto.Email);
                    return BadRequest(new { message = "Invalid or expired reset token" });
                }

                if (user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired password reset token used for user {Email}. Expiry={Expiry}, Now={Now}", dto.Email, user.PasswordResetTokenExpiry, DateTime.UtcNow);
                    return BadRequest(new { message = "Reset token has expired. Please request a new one." });
                }

                if (!_tokenService.VerifyToken(dto.Token, user.PasswordResetTokenHash))
                {
                    _logger.LogWarning("Invalid password reset token for user {Email}", dto.Email);
                    return BadRequest(new { message = "Invalid reset token" });
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.PasswordResetTokenHash = null;
                user.PasswordResetTokenExpiry = null;
                user.LastPasswordChangeAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await ClearLoginAttemptsAsync(normalizedEmail);

                _logger.LogInformation("Password reset successful for user {Email}", dto.Email);
                return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
            }

            _logger.LogWarning("Password reset attempted for non-existent email {Email}", dto.Email);
            return BadRequest(new { message = "Invalid or expired reset token" });
        }

        /// <summary>
        /// Validate reset token
        /// </summary>
        /// <param name="email">The email<see cref="string"/></param>
        /// <param name="token">The token<see cref="string"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { valid = false, message = "Email and token are required" });

            var normalizedEmail = email.Trim().ToLower();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user != null)
            {
                if (string.IsNullOrEmpty(user.PasswordResetTokenHash) || !user.PasswordResetTokenExpiry.HasValue)
                    return Ok(new { valid = false, message = "No password reset was requested" });

                if (user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
                    return Ok(new { valid = false, message = "Reset token has expired" });

                if (!_tokenService.VerifyToken(token, user.PasswordResetTokenHash))
                    return Ok(new { valid = false, message = "Invalid reset token" });

                return Ok(new { valid = true, message = "Token is valid" });
            }

            return Ok(new { valid = false, message = "Email not found" });
        }

        // ==========================================
        // TOTP (Two-Factor Authentication) Endpoints
        // ==========================================

        /// <summary>
        /// Generate TOTP setup for authenticated user
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("totp/setup")]
        [Authorize]
        public async Task<IActionResult> SetupTotp()
        {
            var userId = User.FindFirst("uid")?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            if (!int.TryParse(userId, out var parsedUserId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(parsedUserId);
            if (user == null) return NotFound();

            var secretKey = _totpService.GenerateSecretKey();
            var qrCodeUri = _totpService.GenerateQrCodeUri(email, secretKey);
            var manualEntryKey = _totpService.FormatSecretKeyForManualEntry(secretKey);

            user.TotpSecretKey = _protector.Protect(secretKey);
            // Don't enable yet - user must verify first
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP setup initiated for user {Email}", email);

            return Ok(new TotpSetupDto
            {
                SecretKey = secretKey,
                QrCodeUri = qrCodeUri,
                ManualEntryKey = manualEntryKey
            });
        }

        /// <summary>
        /// Enable TOTP after user verifies they can generate codes
        /// </summary>
        /// <param name="dto">The dto<see cref="EnableTotpDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("totp/enable")]
        [Authorize]
        public async Task<IActionResult> EnableTotp([FromBody] EnableTotpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirst("uid")?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            if (!int.TryParse(userId, out var parsedUserId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(parsedUserId);
            if (user == null) return NotFound();

            var secretKey = user.TotpSecretKey;

            if (string.IsNullOrEmpty(secretKey))
                return BadRequest(new { message = "TOTP setup not initiated. Call /totp/setup first." });

            string stored = user.TotpSecretKey ?? string.Empty;
            string unprotected = stored;
            try { unprotected = _protector.Unprotect(stored); }
            catch { _logger.LogDebug("TOTP secret used plaintext for user {Email}", user.Email); }

            if (!_totpService.VerifyTotpCode(unprotected, dto.TotpCode))
                return BadRequest(new { message = "Invalid TOTP code" });

            user.IsTwoFactorEnabled = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP enabled for user {Email}", email);

            return Ok(new { message = "Two-factor authentication enabled successfully" });
        }

        /// <summary>
        /// Disable TOTP for authenticated user
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("totp/disable")]
        [Authorize]
        public async Task<IActionResult> DisableTotp()
        {
            var userId = User.FindFirst("uid")?.Value;
            var email = User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
                return Unauthorized();

            if (!int.TryParse(userId, out var parsedUserId))
                return Unauthorized();

            var user = await _db.Users.FindAsync(parsedUserId);
            if (user == null) return NotFound();

            user.IsTwoFactorEnabled = false;
            user.TotpSecretKey = null;
            user.RecoveryCodesHash = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP disabled for user {Email}", email);

            return Ok(new { message = "Two-factor authentication disabled successfully" });
        }

        /// <summary>
        /// Verify TOTP code during login
        /// </summary>
        /// <param name="dto">The dto<see cref="VerifyTotpDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("totp/verify")]
        public async Task<IActionResult> VerifyTotpLogin([FromBody] VerifyTotpDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var normalizedEmail = dto.Email.Trim().ToLower();

            var user = await _db.Users.Include(u => u.Company).SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            if (user != null)
            {
                if (!user.IsTwoFactorEnabled || string.IsNullOrEmpty(user.TotpSecretKey))
                    return BadRequest(new { message = "TOTP not enabled for this account" });

                string stored = user.TotpSecretKey ?? string.Empty;
                string unprotected = stored;
                try { unprotected = _protector.Unprotect(stored); }
                catch { _logger.LogDebug("TOTP secret used plaintext for user {Email}", user.Email); }

                if (!_totpService.VerifyTotpCode(unprotected, dto.TotpCode))
                {
                    _logger.LogWarning("Invalid TOTP code for user {Email}", dto.Email);
                    return Unauthorized(new { message = "Invalid TOTP code" });
                }

                await ClearLoginAttemptsAsync(normalizedEmail);

                var token = GenerateJwtForUser(user);

                if (string.IsNullOrEmpty(token))
                {
                    // Failed to generate token due to server configuration - don't expose sensitive details
                    _logger.LogError("Failed to generate JWT for user {Email} during TOTP login", user.Email);
                    return StatusCode(500, new { message = "An error occurred while generating authentication token. Please contact the administrator." });
                }

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    Id = user.UserId,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_config.GetSection("Jwt").GetValue<int>("ExpiryMinutes")),
                    MustChangePassword = user.MustChangePassword
                });
            }

            return Unauthorized(new { message = "Invalid credentials" });
   }

        /// <summary>
        /// Get current user's company profile
        /// </summary>
        [HttpGet("company")]
        [Authorize]
        public async Task<IActionResult> GetCompanyProfile()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId)) return Unauthorized();

            var user = await _db.Users.FindAsync(parsedUserId);
            if (user == null) return NotFound(new { message = "User not found" });

            var company = await _db.Companies.FindAsync(user.CompanyId);
            if (company == null) return NotFound(new { message = "Company not found" });

            // load company settings if available
            var settings = await _db.CompanySettings.FindAsync(company.CompanyId);

            return Ok(new CompanyProfileDto
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                Address = company.Address,
                ContactEmail = company.ContactEmail,
                ContactPhone = company.ContactPhone,
                Tin = company.Tin,
                LogoUrl = company.LogoUrl,
                PayrollCycle = settings != null ? (int?)settings.PayrollCycle : null,
                WorkHoursPerDay = settings?.WorkHoursPerDay,
                OvertimeRate = settings != null ? (decimal?)settings.OvertimeRate : null,
                LateGraceMinutes = settings?.LateGraceMinutes,
                HolidayRate = settings?.HolidayRate,
                AbsentRate = settings?.AbsentRate,
                // expose roles JSON so frontend can show/edit roles list
                RolesJson = settings?.RolesJson
            });
        }

        /// <summary>
 /// Update current user's company profile
 /// </summary>
 [HttpPut("company")]
 [Authorize(Roles = "SuperAdmin,Admin")]
 public async Task<IActionResult> UpdateCompanyProfile([FromBody] UpdateCompanyProfileDto dto)
 {
 if (!ModelState.IsValid) return BadRequest(ModelState);

 var userId = User.FindFirst("uid")?.Value;
 if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId)) return Unauthorized();

 var user = await _db.Users.FindAsync(parsedUserId);
 if (user == null) return NotFound(new { message = "User not found" });

 var company = await _db.Companies.FindAsync(user.CompanyId);
 if (company == null) return NotFound(new { message = "Company not found" });

 var oldValues = new { company.CompanyName, company.Address, company.ContactEmail, company.ContactPhone, company.Tin, company.LogoUrl };

 company.CompanyName = dto.CompanyName;
 company.Address = dto.Address;
 company.ContactEmail = dto.ContactEmail;
 company.ContactPhone = dto.ContactPhone;
 company.Tin = dto.Tin;
 company.LogoUrl = dto.LogoUrl;
 company.UpdatedAt = DateTime.UtcNow;

 // Update or create company settings
 var settings = await _db.CompanySettings.FindAsync(company.CompanyId);
 if (settings == null)
 {
 settings = new CompanySetting
 {
 CompanyId = company.CompanyId,
 CreatedAt = DateTime.UtcNow
 };
 _db.CompanySettings.Add(settings);
 }

 if (dto.PayrollCycle.HasValue) settings.PayrollCycle = (Enums.PayrollCycle)dto.PayrollCycle.Value;
 if (dto.WorkHoursPerDay.HasValue) settings.WorkHoursPerDay = dto.WorkHoursPerDay.Value;
 if (dto.OvertimeRate.HasValue) settings.OvertimeRate = dto.OvertimeRate.Value;
 if (dto.LateGraceMinutes.HasValue) settings.LateGraceMinutes = dto.LateGraceMinutes.Value;
 if (dto.HolidayRate.HasValue) settings.HolidayRate = dto.HolidayRate.Value;
 if (dto.AbsentRate.HasValue) settings.AbsentRate = dto.AbsentRate.Value;

 // Roles JSON persisted on settings
 if (!string.IsNullOrWhiteSpace(dto.RolesJson)) settings.RolesJson = dto.RolesJson;

 settings.UpdatedAt = DateTime.UtcNow;

 await _db.SaveChangesAsync();

 await _activityLogger.LogAsync(parsedUserId, company.CompanyId, "UpdateCompanyProfile", "Company", company.CompanyId.ToString(),
 System.Text.Json.JsonSerializer.Serialize(oldValues), System.Text.Json.JsonSerializer.Serialize(dto), GetClientIpAddress(), HttpContext.Request.Headers["User-Agent"].FirstOrDefault(), user.Role.ToString(), "Company");

 return Ok(new { message = "Company profile updated" });
        }

        [HttpPost("company/logo")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UploadCompanyLogo([FromForm] IFormFile file)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var parsedUserId)) return Unauthorized();

            var user = await _db.Users.FindAsync(parsedUserId);
            if (user == null) return NotFound(new { message = "User not found" });

            if (file == null || file.Length == 0) return BadRequest(new { message = "No file uploaded" });

            using var stream = file.OpenReadStream();
            var url = await _cloudinary.UploadCompanyLogoAsync(stream, file.FileName, user.CompanyId.ToString());

            if (url == null) return BadRequest(new { message = "Failed to upload logo" });

            return Ok(new ProfileImageResponseDto
            {
                Success = true,
                Message = "Logo uploaded successfully",
                ImageUrl = url
            });
        }
    }
}
