# Setup Checklist ?

Complete these steps to finalize your TOTP and Password Reset implementation:

## ?? CRITICAL - Do These First

### 1. Configure SMTP Settings
- [ ] Open `PayRexApplication/appsettings.json`
- [ ] Update SMTP configuration:
  ```json
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "YOUR_EMAIL@gmail.com",
    "Password": "YOUR_APP_PASSWORD",
    "FromEmail": "noreply@payrex.com",
    "FromName": "PayRex Support",
    "EnableSsl": true
  }
  ```

**For Gmail Users:**
1. Go to https://myaccount.google.com/security
2. Enable 2-Step Verification
3. Go to https://myaccount.google.com/apppasswords
4. Create app password for "Mail"
5. Use the 16-character password (no spaces)

### 2. Apply Database Migration
- [ ] Stop the running application
- [ ] Open terminal in solution directory
- [ ] Run:
  ```bash
  dotnet ef database update --project "PayRexApplication\PayRex.API.csproj" --startup-project "PayRexApplication\PayRex.API.csproj"
  ```
- [ ] Verify migration applied successfully
- [ ] Restart the application

**Alternative (if migration fails):**
```bash
# Drop and recreate database
dotnet ef database drop --project "PayRexApplication\PayRex.API.csproj" --force
dotnet ef database update --project "PayRexApplication\PayRex.API.csproj"
```

---

## ?? RECOMMENDED - Test Features

### 3. Test TOTP Flow
- [ ] Login to the application
- [ ] Call `/api/auth/totp/setup` (see QUICK_START.md)
- [ ] Scan QR code with Google Authenticator
- [ ] Call `/api/auth/totp/enable` with a valid code
- [ ] Logout
- [ ] Login again - verify TOTP is required
- [ ] Enter 6-digit code from Google Authenticator
- [ ] Verify successful login

### 4. Test Password Reset Flow
- [ ] Navigate to https://localhost:7001/Auth/ForgotPassword
- [ ] Enter your email
- [ ] Complete hCaptcha
- [ ] Submit form
- [ ] Check email inbox
- [ ] Click reset link in email
- [ ] Enter new password
- [ ] Verify redirect to login
- [ ] Login with new password

### 5. Test Email Delivery
Create a test endpoint in `AuthController.cs`:

```csharp
[HttpGet("test-email")]
public async Task<IActionResult> TestEmail([FromQuery] string email)
{
    try
    {
        await _emailService.SendPasswordResetEmailAsync(
            email,
    "https://localhost:7001/Auth/ResetPassword?token=test123&email=" + email
        );
        return Ok(new { message = "Test email sent successfully" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send test email");
        return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
    }
}
```

Then test:
```bash
curl "https://localhost:7000/api/auth/test-email?email=YOUR_EMAIL@example.com"
```

---

## ?? OPTIONAL - Additional Setup

### 6. Production Configuration
- [ ] Change JWT secret key in `appsettings.json`
- [ ] Update `AppSettings:FrontendUrl` to production URL
- [ ] Update CORS policy in `Program.cs` with production domains
- [ ] Configure production SMTP server
- [ ] Review token expiration times (currently 30 min for reset, 60 min for JWT)
- [ ] Set up logging/monitoring

### 7. Security Review
- [ ] Verify HTTPS is enforced
- [ ] Review CORS policies
- [ ] Check rate limiting (account lockout currently 5 attempts)
- [ ] Test with different time zones (TOTP clock drift)
- [ ] Verify email enumeration prevention works
- [ ] Test expired token handling

### 8. UI/UX Enhancements
- [ ] Add TOTP setup page in user settings
- [ ] Add "Disable 2FA" button in user settings
- [ ] Display backup codes (optional)
- [ ] Add email notification when password is reset
- [ ] Add email notification when TOTP is enabled/disabled

---

## ?? Verification Steps

### Database Verification
Run this SQL to verify new columns exist:

```sql
-- Check SaasUsers table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'saasUsers'
AND COLUMN_NAME IN ('isTwoFactorEnabled', 'totpSecretKey', 'passwordResetTokenHash', 'passwordResetTokenExpiry');

-- Check Users table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'users'
AND COLUMN_NAME IN ('isTwoFactorEnabled', 'totpSecretKey', 'passwordResetTokenHash', 'passwordResetTokenExpiry');
```

Expected: 4 columns in each table

### API Endpoints Verification
Test each endpoint with Swagger:

1. Go to https://localhost:7000/swagger
2. Verify these endpoints exist:
   - `POST /api/auth/totp/setup`
   - `POST /api/auth/totp/enable`
   - `POST /api/auth/totp/disable`
   - `POST /api/auth/totp/verify`
   - `POST /api/auth/forgot-password`
   - `POST /api/auth/reset-password`

### Razor Pages Verification
Navigate to each page:
- [ ] https://localhost:7001/Auth/Login
- [ ] https://localhost:7001/Auth/ForgotPassword
- [ ] https://localhost:7001/Auth/ResetPassword?email=test@test.com&token=test

---

## ?? Common Issues

### Issue: Migration Already Applied
**Symptom:** Running migration says "No migrations to apply"

**Fix:** The migration was already applied by auto-migration on startup. Verify columns exist in database.

---

### Issue: Build Failed
**Symptom:** Compilation errors

**Fix:**
1. Stop debugging
2. Clean solution: `dotnet clean`
3. Rebuild: `dotnet build`
4. Check errors in Output window

---

### Issue: SMTP Authentication Failed
**Symptom:** "535 Authentication failed" or similar

**Fix:**
1. Use Gmail App Password (not regular password)
2. Enable "Less secure app access" if needed
3. Check username is full email address
4. Verify port 587 with TLS

---

### Issue: TOTP Codes Always Invalid
**Symptom:** Codes from Google Authenticator are rejected

**Fix:**
1. Sync time: Google Authenticator ? Settings ? Time correction ? Sync now
2. Verify server time is correct
3. Check time zone settings
4. Try entering code twice (if near 30-second boundary)

---

### Issue: Password Reset Link Expired
**Symptom:** "Reset token has expired" even though just sent

**Fix:**
1. Check server time is correct
2. Verify `PasswordResetTokenExpiryMinutes` is set to 30
3. Request new link

---

## ?? Support

If you encounter issues:

1. Check logs in Output window
2. Review `AUTHENTICATION_IMPLEMENTATION.md` for detailed docs
3. Check `QUICK_START.md` for API examples
4. Test with Postman/Swagger to isolate frontend vs backend issues

---

## ? Final Checklist

Before marking this complete:
- [ ] Database migration applied successfully
- [ ] SMTP configured and tested
- [ ] Can send test email
- [ ] TOTP setup works
- [ ] TOTP login works
- [ ] Password reset email received
- [ ] Password reset works
- [ ] All Razor Pages load correctly
- [ ] No compilation errors
- [ ] Reviewed security settings

---

**Status Tracking:**
- ? Not Started
- ?? In Progress
- ? Complete
- ? Issues Found

---

**Estimated Time:** 30-45 minutes

**Priority Order:**
1. Configure SMTP (5 min)
2. Apply migration (2 min)
3. Test TOTP (10 min)
4. Test password reset (10 min)
5. Production setup (15 min)
