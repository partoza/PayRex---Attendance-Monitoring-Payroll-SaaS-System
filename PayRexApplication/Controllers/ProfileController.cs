namespace PayRexApplication.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PayRexApplication.Data;
    using PayRexApplication.DTOs;
    using PayRexApplication.Helpers;
    using PayRexApplication.Services;
    using System;
    using System.IdentityModel.Tokens.Jwt;

    /// <summary>
    /// API Controller for user profile management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ProfileController> _logger;
        private readonly ITotpService _totpService;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IRecoveryCodeService _recoveryCodeService;
        private readonly IDataProtector _protector;

        public ProfileController(
            AppDbContext db,
            ILogger<ProfileController> logger,
            ITotpService totpService,
            ICloudinaryService cloudinaryService,
            IRecoveryCodeService recoveryCodeService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _db = db;
            _logger = logger;
            _totpService = totpService;
            _cloudinaryService = cloudinaryService;
            _recoveryCodeService = recoveryCodeService;
            _protector = dataProtectionProvider.CreateProtector("PayRex.TotpSecrets");
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            return Ok(new UserProfileResponseDto
            {
                Id = user.UserId.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfileImageUrl = user.ProfileImageUrl,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                LastPasswordChangeAt = user.LastPasswordChangeAt
            });
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        /// <param name="dto">The dto<see cref="UpdateProfileDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            // Check email uniqueness if changing
            if (!string.IsNullOrEmpty(dto.Email) && dto.Email.ToLower() != user.Email.ToLower())
            {
                var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.UserId != userId);
                if (emailExists)
                    return BadRequest(new { message = "Email is already in use" });

                user.Email = dto.Email.Trim();
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("Profile updated for user {UserId}", userId);
            return Ok(new { message = "Profile updated successfully" });
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="dto">The dto<see cref="ChangePasswordDto"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("ChangePassword validation failed: {Errors}", string.Join("; ", errors));
                return BadRequest(new { message = "Validation failed", errors });
            }

            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var currentPassword = dto.CurrentPassword?.Trim() ?? string.Empty;
            var newPassword = dto.NewPassword?.Trim() ?? string.Empty;
            var confirmPassword = dto.ConfirmPassword?.Trim() ?? string.Empty;

            if (newPassword != confirmPassword)
            {
                _logger.LogWarning("ChangePassword: new password and confirm password do not match for user {UserId}", userId);
                return BadRequest(new { message = "New password and confirmation do not match" });
            }

            try
            {
                var user = await _db.Users.FindAsync(userId.Value);
                if (user == null) return NotFound();

                // Verify current password
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogWarning("No password hash present for user {UserId}", userId);
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                try
                {
                    if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                    {
                        _logger.LogWarning("Incorrect current password for user {UserId}", userId);
                        return BadRequest(new { message = "Current password is incorrect" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BCrypt.Verify failed for user {UserId}", userId);
                    return StatusCode(500, new { message = "Unable to verify current password" });
                }

                // Update password
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.LastPasswordChangeAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                _logger.LogInformation("Password changed for user {UserId}", userId);
                return Ok(new { message = "Password changed successfully. Please log in again.", requireRelogin = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing password for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        /// <summary>
        /// Upload profile image
        /// </summary>
        /// <param name="file">The file<see cref="IFormFile"/></param>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpPost("image")]
        [RequestSizeLimit(2 * 1024 * 1024)] // 2MB limit
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ProfileImageResponseDto { Success = false, Message = "No file uploaded" });

            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new ProfileImageResponseDto
                {
                    Success = false,
                    Message = "Invalid file type. Only JPG, PNG, and WEBP are allowed."
                });
            }

            // Validate file size (2MB)
            if (file.Length > 2 * 1024 * 1024)
            {
                return BadRequest(new ProfileImageResponseDto
                {
                    Success = false,
                    Message = "File size exceeds 2MB limit."
                });
            }

            using var stream = file.OpenReadStream();
            var userIdString = $"user_{userId}";
            var imageUrl = await _cloudinaryService.UploadProfileImageAsync(stream, file.FileName, userIdString);

            if (string.IsNullOrEmpty(imageUrl))
            {
                return StatusCode(500, new ProfileImageResponseDto
                {
                    Success = false,
                    Message = "Failed to upload image. Please try again."
                });
            }

            var user = await _db.Users.FindAsync(userId.Value);
            if (user != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    var oldPublicId = _cloudinaryService.ExtractPublicIdFromUrl(user.ProfileImageUrl);
                    if (!string.IsNullOrEmpty(oldPublicId))
                        await _cloudinaryService.DeleteImageAsync(oldPublicId);
                }

                user.ProfileImageUrl = imageUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Profile image uploaded for user {UserId}", userId);
            return Ok(new ProfileImageResponseDto
            {
                Success = true,
                ImageUrl = imageUrl,
                Message = "Profile image updated successfully"
            });
        }

        /// <summary>
        /// Remove profile image
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpDelete("image")]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user != null && !string.IsNullOrEmpty(user.ProfileImageUrl))
            {
                var publicId = _cloudinaryService.ExtractPublicIdFromUrl(user.ProfileImageUrl);
                if (!string.IsNullOrEmpty(publicId))
                    await _cloudinaryService.DeleteImageAsync(publicId);

                user.ProfileImageUrl = null;
                user.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            _logger.LogInformation("Profile image removed for user {UserId}", userId);
            return Ok(new { message = "Profile image removed successfully" });
        }

        /// <summary>
        /// Setup TOTP (Step 1: Generate secret and QR code)
        /// Returns the secret key, QR URI, and manual entry key.
        /// Does NOT enable 2FA yet — the user must verify with a code first.
        /// </summary>
        [HttpPost("2fa/setup")]
        public async Task<IActionResult> SetupTwoFactor()
        {
            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
            {
                _logger.LogWarning("2FA setup: could not resolve userId from claims");
                return Unauthorized(new { message = "Could not identify user from token" });
            }

            // Try to get email from claims first
            var email = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)?.Value;

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null)
            {
                _logger.LogWarning("2FA setup: user {UserId} not found in database", userId);
                return NotFound(new { message = "User not found" });
            }

            // If email claim missing, fall back to email from database
            if (string.IsNullOrEmpty(email))
            {
                email = user.Email;
                _logger.LogDebug("2FA setup: email claim missing, using database email for user {UserId}", userId);
            }

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("2FA setup: no email available for userId {UserId}", userId);
                return Unauthorized(new { message = "Email claim missing from token and no email found in database" });
            }

            // Generate new secret key
            var secretKey = _totpService.GenerateSecretKey();
            var qrCodeUri = _totpService.GenerateQrCodeUri(email, secretKey);
            var manualEntryKey = _totpService.FormatSecretKeyForManualEntry(secretKey);

            // Store the protected secret — do NOT enable yet
            user.TotpSecretKey = _protector.Protect(secretKey);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP setup initiated for user {UserId} ({Email})", userId, email);

            return Ok(new TotpSetupWithRecoveryDto
            {
                SecretKey = secretKey,
                QrCodeUri = qrCodeUri,
                ManualEntryKey = manualEntryKey
            });
        }

        /// <summary>
        /// Enable TOTP (Step 2: Verify code, generate recovery codes, enable)
        /// The user sends a 6-digit code from their authenticator app.
        /// If valid: isTwoFactorEnabled = true, recovery codes are generated and returned.
        /// </summary>
        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTotpWithVerificationDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized(new { message = "Could not identify user from token" });

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found" });

            if (string.IsNullOrEmpty(user.TotpSecretKey))
                return BadRequest(new { message = "TOTP setup not initiated. Please call setup first." });

            // Unprotect the stored secret
            string unprotectedKey;
            try
            {
                unprotectedKey = _protector.Unprotect(user.TotpSecretKey);
            }
            catch
            {
                // Backwards compatibility: treat stored value as plaintext
                unprotectedKey = user.TotpSecretKey;
                _logger.LogDebug("TOTP secret appears to be plaintext for user {UserId}", userId);
            }

            // Verify the TOTP code
            if (!_totpService.VerifyTotpCode(unprotectedKey, dto.TotpCode))
            {
                _logger.LogWarning("Invalid TOTP code during enable for user {UserId}", userId);
                return BadRequest(new { message = "Invalid verification code. Please try again." });
            }

            // Generate recovery codes
            var recoveryCodes = _recoveryCodeService.GenerateRecoveryCodes(10);
            user.RecoveryCodesHash = _recoveryCodeService.HashRecoveryCodes(recoveryCodes);
            user.IsTwoFactorEnabled = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP enabled for user {UserId}", userId);

            return Ok(new TotpEnableResponseDto
            {
                Success = true,
                Message = "Two-factor authentication enabled successfully",
                RecoveryCodes = recoveryCodes
            });
        }

        /// <summary>
        /// Disable TOTP: sets isTwoFactorEnabled = false, clears secret and recovery codes
        /// </summary>
        [HttpPost("2fa/disable")]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized(new { message = "Could not identify user from token" });

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound(new { message = "User not found" });

            user.IsTwoFactorEnabled = false;
            user.TotpSecretKey = null;
            user.RecoveryCodesHash = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation("TOTP disabled for user {UserId}", userId);

            return Ok(new { message = "Two-factor authentication disabled successfully" });
        }

        /// <summary>
        /// Get 2FA status
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet("2fa/status")]
        public async Task<IActionResult> GetTwoFactorStatus()
        {
            var (userId, _) = GetUserIdFromClaims();
            if (userId == null)
                return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            return Ok(new
            {
                isEnabled = user.IsTwoFactorEnabled,
                hasSecretKey = !string.IsNullOrEmpty(user.TotpSecretKey),
                recoveryCodesRemaining = _recoveryCodeService.GetRemainingCodeCount(user.RecoveryCodesHash)
            });
        }

        /// <summary>
        /// Debug endpoint to inspect claims and DB values for current user
        /// </summary>
        /// <returns>The <see cref="Task{IActionResult}"/></returns>
        [HttpGet("debug")]
        public async Task<IActionResult> DebugProfile()
        {
            var (userId, isSuperAdmin) = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var user = await _db.Users.FindAsync(userId.Value);
            if (user == null) return NotFound();

            return Ok(new
            {
                ResolvedUserId = userId.Value,
                IsSuperAdmin = isSuperAdmin,
                user.Email,
                user.Role,
                user.CompanyId,
                user.IsTwoFactorEnabled,
                user.TotpSecretKey,
                user.RecoveryCodesHash,
                user.LastPasswordChangeAt
            });
        }

        private (int? UserId, bool IsSuperAdmin) GetUserIdFromClaims()
        {
            return ClaimsHelper.GetUserIdFromClaims(User);
        }
    }
}
