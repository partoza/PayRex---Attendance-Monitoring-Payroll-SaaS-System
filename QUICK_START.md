# Quick Start Guide: TOTP & Password Reset

## ?? TOTP (Two-Factor Authentication)

### 1. Enable TOTP for Your Account

**Step 1: Get Setup Info**
```http
POST https://localhost:7000/api/auth/totp/setup
Authorization: Bearer YOUR_JWT_TOKEN
```

**Response:**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/PayRex:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=PayRex",
  "manualEntryKey": "JBSW Y3DP EHPK 3PXP"
}
```

**Step 2: Scan QR Code**
- Open Google Authenticator
- Tap "+" ? "Scan QR code"
- Scan the QR code generated from `qrCodeUri`

**Step 3: Enable TOTP**
```http
POST https://localhost:7000/api/auth/totp/enable
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json

{
  "totpCode": "123456"
}
```

### 2. Login with TOTP

**Step 1: Login with Email/Password**
```http
POST https://localhost:7000/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "YourPassword123!"
}
```

**Response (if TOTP enabled):**
```json
{
  "requireTotp": true,
  "message": "Please provide your TOTP code",
  "email": "user@example.com"
}
```

**Step 2: Verify TOTP Code**
```http
POST https://localhost:7000/api/auth/totp/verify
Content-Type: application/json

{
  "email": "user@example.com",
  "totpCode": "123456"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "id": 1,
  "email": "user@example.com",
  "role": "Admin",
  "expiresAt": "2025-02-05T22:00:00Z"
}
```

### 3. Disable TOTP

```http
POST https://localhost:7000/api/auth/totp/disable
Authorization: Bearer YOUR_JWT_TOKEN
```

---

## ?? Password Reset

### 1. Forgot Password Flow

**Step 1: Request Reset Link**
```http
POST https://localhost:7000/api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

**Response:**
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

**Step 2: Check Email**
- Check inbox for "Password Reset Request - PayRex"
- Click the reset link (valid for 30 minutes)

**Step 3: Reset Password**
```http
POST https://localhost:7000/api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "abc123def456ghi789...",
  "newPassword": "NewSecure123!",
  "confirmPassword": "NewSecure123!"
}
```

**Response:**
```json
{
  "message": "Password reset successfully. You can now log in with your new password."
}
```

---

## ?? UI Access

### Razor Pages

1. **Login:** https://localhost:7001/Auth/Login
2. **Forgot Password:** https://localhost:7001/Auth/ForgotPassword
3. **Reset Password:** https://localhost:7001/Auth/ResetPassword?email=user@example.com&token=...

---

## ?? Configuration

### Gmail SMTP Setup

1. Go to https://myaccount.google.com/apppasswords
2. Create new app password for "Mail"
3. Update `appsettings.json`:

```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
 "Port": 587,
    "Username": "your-email@gmail.com",
"Password": "your-16-char-app-password",
    "FromEmail": "noreply@payrex.com",
    "FromName": "PayRex Support",
    "EnableSsl": true
  }
}
```

### Test Email Service

Create a test endpoint to verify SMTP:

```csharp
[HttpGet("test-email")]
public async Task<IActionResult> TestEmail([FromQuery] string email)
{
    try
{
        await _emailService.SendPasswordResetEmailAsync(
       email, 
            "https://localhost:7001/Auth/ResetPassword?token=test123"
        );
        return Ok(new { message = "Test email sent successfully" });
    }
 catch (Exception ex)
    {
    return StatusCode(500, new { message = ex.Message });
    }
}
```

---

## ?? Troubleshooting

### TOTP Not Working

**Problem:** Codes are always invalid

**Fix:**
1. Open Google Authenticator
2. Go to Settings ? Time correction for codes
3. Tap "Sync now"
4. Try again

### Email Not Sending

**Problem:** SMTP exception

**Fix:**
1. Verify Gmail app password (not regular password)
2. Check port 587 is not blocked
3. Enable "Less secure app access" if needed
4. Check logs for detailed error

### Token Expired

**Problem:** "Reset token has expired"

**Fix:**
- Request new reset link
- Tokens expire after 30 minutes
- Check server time is correct

---

## ?? Password Requirements

- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 digit
- At least 1 special character

**Valid Examples:**
- `SecurePass123!`
- `MyP@ssw0rd`
- `Admin#2025`

**Invalid Examples:**
- `password123` (no uppercase, no special)
- `PASSWORD` (no lowercase, no digit)
- `Pass1!` (too short)

---

## ?? Security Notes

### What's Stored in Database

**TOTP:**
- ? Base32-encoded secret key
- ? NO OTP codes
- ? NO timestamps

**Password Reset:**
- ? SHA256 hash of token
- ? Expiration timestamp
- ? NO plain text tokens

**Passwords:**
- ? BCrypt hash
- ? NO plain text passwords

---

## ?? Testing Checklist

### TOTP
- [ ] Setup TOTP returns QR code
- [ ] Can scan with Google Authenticator
- [ ] Enable requires valid code
- [ ] Login requires TOTP code when enabled
- [ ] Can disable TOTP
- [ ] Login works without TOTP when disabled

### Password Reset
- [ ] Forgot password sends email
- [ ] Email contains valid reset link
- [ ] Reset link opens reset page
- [ ] Can set new password
- [ ] Token expires after 30 minutes
- [ ] Can't reuse old token
- [ ] Can login with new password

---

## ?? Quick Commands

### Apply Migration
```bash
# Stop application first
dotnet ef database update --project PayRexApplication\PayRex.API.csproj
```

### Reset Database
```bash
dotnet ef database drop --project PayRexApplication\PayRex.API.csproj --force
dotnet ef database update --project PayRexApplication\PayRex.API.csproj
```

### Run Application
```bash
# Terminal 1: API
cd PayRexApplication
dotnet run --project PayRex.API.csproj

# Terminal 2: Web
cd PayRex.Web
dotnet run
```

---

**Quick Reference:** See `AUTHENTICATION_IMPLEMENTATION.md` for complete documentation.
