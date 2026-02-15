# PayRex Authentication Implementation

## Overview

This implementation adds TWO SEPARATE security features to the PayRex ASP.NET Core application:

1. **TOTP (Time-based One-Time Password)** for login authentication
2. **SMTP Email** for password recovery

---

## A. TOTP FOR LOGIN AUTHENTICATION

### Features
- Compatible with **Google Authenticator** (RFC 6238)
- TOTP is tied to the user's **EMAIL**
- Only **Base32-encoded secret key** is stored in the database
- **No OTP codes** are stored
- **6-digit codes** with 30-second time windows
- Clock drift tolerance of ±1 time window

### Database Fields Added

#### SaasUser & User Models
```csharp
bool IsTwoFactorEnabled        // Default: false
string? TotpSecretKey          // Base32-encoded secret (max 64 chars)
```

### API Endpoints

#### 1. Setup TOTP
**Endpoint:** `POST /api/auth/totp/setup`  
**Authorization:** Required (JWT Bearer token)  
**Purpose:** Generate a new TOTP secret key and QR code

**Response:**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/PayRex:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=PayRex",
  "manualEntryKey": "JBSW Y3DP EHPK 3PXP"
}
```

**Usage:**
1. User calls this endpoint from their account settings
2. Display QR code using the `qrCodeUri` 
3. User scans QR code with Google Authenticator
4. Secret is saved but TOTP is NOT enabled yet

---

#### 2. Enable TOTP
**Endpoint:** `POST /api/auth/totp/enable`  
**Authorization:** Required (JWT Bearer token)  
**Purpose:** Enable TOTP after user verifies they can generate codes

**Request Body:**
```json
{
  "totpCode": "123456"
}
```

**Response:**
```json
{
  "message": "Two-factor authentication enabled successfully"
}
```

**Security:**
- User must verify they can generate valid codes before TOTP is enabled
- Prevents account lockout due to misconfiguration

---

#### 3. Disable TOTP
**Endpoint:** `POST /api/auth/totp/disable`  
**Authorization:** Required (JWT Bearer token)  
**Purpose:** Disable TOTP for the authenticated user

**Response:**
```json
{
  "message": "Two-factor authentication disabled successfully"
}
```

---

#### 4. Login with TOTP
**Endpoint:** `POST /api/auth/login`  
**Authorization:** None  
**Purpose:** Enhanced login flow with TOTP support

**Login Flow:**

**Step 1:** User submits email + password
```json
{
  "email": "user@example.com",
"password": "SecurePassword123!"
}
```

**Response (if TOTP is enabled):**
```json
{
  "requireTotp": true,
  "message": "Please provide your TOTP code",
  "email": "user@example.com"
}
```

**Step 2:** User submits TOTP code
```
POST /api/auth/totp/verify
```

```json
{
  "email": "user@example.com",
  "totpCode": "123456"
}
```

**Response (on success):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "id": 1,
  "email": "user@example.com",
  "role": "Admin",
  "expiresAt": "2025-02-05T22:00:00Z"
}
```

---

### NuGet Package
- **Otp.NET** (v1.4.1) - RFC 6238 compliant TOTP implementation

### Service: TotpService

**Location:** `PayRexApplication/Services/TotpService.cs`

**Key Methods:**
- `GenerateSecretKey()` - Generates 256-bit secure random key
- `GenerateQrCodeUri()` - Creates Google Authenticator compatible URI
- `VerifyTotpCode()` - Validates 6-digit TOTP code with time window tolerance
- `FormatSecretKeyForManualEntry()` - Formats key for manual entry

---

## B. SMTP EMAIL FOR PASSWORD RECOVERY

### Features
- SMTP is used **ONLY** for password recovery (not for TOTP)
- Cryptographically secure reset tokens (256-bit)
- Only **hashed tokens** are stored in the database
- **30-minute** expiration time
- HTML email templates
- Constant-time token comparison (prevents timing attacks)

### Database Fields Added

#### SaasUser & User Models
```csharp
string? PasswordResetTokenHash     // SHA256 hash (max 256 chars)
DateTime? PasswordResetTokenExpiry   // Token expiration timestamp
```

### API Endpoints

#### 1. Forgot Password
**Endpoint:** `POST /api/auth/forgot-password`  
**Authorization:** None  
**Purpose:** Initiate password reset flow

**Request Body:**
```json
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

**Security:**
- Always returns success to prevent email enumeration
- Generates cryptographically secure token
- Stores SHA256 hash of token (not plain text)
- Sets 30-minute expiration

**Email Sent:**
- Subject: "Password Reset Request - PayRex"
- Contains reset URL: `https://localhost:7001/Auth/ResetPassword?email=user@example.com&token=abc123...`
- Professional HTML template with PayRex branding

---

#### 2. Reset Password
**Endpoint:** `POST /api/auth/reset-password`  
**Authorization:** None  
**Purpose:** Reset password using token from email

**Request Body:**
```json
{
  "email": "user@example.com",
  "token": "abc123def456...",
  "newPassword": "NewSecure123!",
  "confirmPassword": "NewSecure123!"
}
```

**Response (on success):**
```json
{
  "message": "Password reset successfully. You can now log in with your new password."
}
```

**Response (on error):**
```json
{
  "message": "Invalid or expired reset token"
}
```

**Security:**
- Validates token expiration
- Uses constant-time comparison to prevent timing attacks
- Hashes new password with BCrypt
- Clears reset token fields after successful reset

---

### Services

#### EmailService
**Location:** `PayRexApplication/Services/EmailService.cs`

**Configuration Required:**
```json
{
  "Smtp": {
    "Host": "smtp.gmail.com",
 "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@payrex.com",
    "FromName": "PayRex Support",
 "EnableSsl": true
  }
}
```

**For Gmail:**
1. Enable 2-factor authentication
2. Generate an "App Password" at https://myaccount.google.com/apppasswords
3. Use the app password (not your regular password)

---

#### PasswordResetTokenService
**Location:** `PayRexApplication/Services/PasswordResetTokenService.cs`

**Key Methods:**
- `GenerateToken()` - Generates 256-bit URL-safe Base64 token
- `HashToken()` - SHA256 hash for secure storage
- `VerifyToken()` - Constant-time comparison

---

## C. RAZOR PAGES UI

### ForgotPassword.cshtml
**URL:** `/Auth/ForgotPassword`  
**Purpose:** User enters email to request password reset

**Features:**
- Email input field
- hCaptcha verification
- Success/error messages
- Matches Login.cshtml styling
- Tailwind CSS + Flowbite
- Animated background

**Files:**
- `PayRex.Web/Pages/Auth/ForgotPassword.cshtml`
- `PayRex.Web/Pages/Auth/ForgotPassword.cshtml.cs`

---

### ResetPassword.cshtml
**URL:** `/Auth/ResetPassword?email=user@example.com&token=abc123...`  
**Purpose:** User sets new password using reset link from email

**Features:**
- New password field
- Confirm password field
- Password visibility toggle
- Client-side validation
- Server-side validation
- Auto-redirect to login on success
- Matches Login.cshtml styling

**Files:**
- `PayRex.Web/Pages/Auth/ResetPassword.cshtml`
- `PayRex.Web/Pages/Auth/ResetPassword.cshtml.cs`

---

## D. DATABASE MIGRATION

### Migration File
**Location:** `PayRexApplication/Migrations/20260205200000_AddTotpAndPasswordResetFields.cs`

**Fields Added:**

#### saasUsers Table
- `isTwoFactorEnabled` (bit, NOT NULL, default: 0)
- `totpSecretKey` (nvarchar(64), NULL)
- `passwordResetTokenHash` (nvarchar(256), NULL)
- `passwordResetTokenExpiry` (datetime2, NULL)

#### users Table
- `isTwoFactorEnabled` (bit, NOT NULL, default: 0)
- `totpSecretKey` (nvarchar(64), NULL)
- `passwordResetTokenHash` (nvarchar(256), NULL)
- `passwordResetTokenExpiry` (datetime2, NULL)

### Apply Migration

**Note:** The application is already running with hot reload, so the migration will be applied automatically on the next restart. If you need to apply it manually:

```bash
# Stop the running application first
dotnet ef database update --project PayRexApplication\PayRex.API.csproj
```

---

## E. CONFIGURATION

### API Configuration (appsettings.json)

**Location:** `PayRexApplication/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "..."
  },
  "Jwt": {
    "Key": "ChangeThisLongRandomSecretKeyForProduction_ReplaceMe",
 "Issuer": "PayRex",
    "Audience": "PayRexUsers",
    "ExpiryMinutes": 60
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "noreply@payrex.com",
  "FromName": "PayRex Support",
    "EnableSsl": true
  },
  "AppSettings": {
    "FrontendUrl": "https://localhost:7001"
  }
}
```

### Web Configuration (appsettings.json)

**Location:** `PayRex.Web/appsettings.json`

```json
{
  "ApiBaseUrl": "https://localhost:7000",
  "ApiSettings": {
    "BaseUrl": "https://localhost:7000"
  },
  "HCaptcha": {
    "SiteKey": "your-site-key",
    "Secret": "your-secret"
  }
}
```

---

## F. TESTING INSTRUCTIONS

### Test TOTP Flow

1. **Setup TOTP:**
   ```bash
   curl -X POST https://localhost:7000/api/auth/totp/setup \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json"
   ```

2. **Scan QR Code:**
- Use the `qrCodeUri` from response
   - Open Google Authenticator app
   - Scan QR code or enter manual key

3. **Enable TOTP:**
   ```bash
   curl -X POST https://localhost:7000/api/auth/totp/enable \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
     -d '{"totpCode": "123456"}'
   ```

4. **Test Login:**
   - Log out
   - Log in with email/password
   - System will ask for TOTP code
   - Enter 6-digit code from Google Authenticator

---

### Test Password Reset Flow

1. **Request Reset:**
   - Navigate to `/Auth/ForgotPassword`
   - Enter your email
   - Complete hCaptcha
   - Click "Send Reset Link"

2. **Check Email:**
   - Check your inbox for reset email
   - Email subject: "Password Reset Request - PayRex"

3. **Reset Password:**
   - Click link in email
   - Enter new password (must meet requirements)
   - Confirm new password
   - Click "Reset Password"

4. **Verify:**
   - You'll be redirected to login
   - Log in with new password

---

## G. SECURITY BEST PRACTICES

### ? Implemented

1. **TOTP:**
   - Never stores OTP codes
   - Base32-encoded secrets only
   - Time window tolerance for clock drift
   - 256-bit random key generation
   - User must verify before enabling

2. **Password Reset:**
   - Never stores plain text tokens
   - SHA256 hashing
   - 30-minute expiration
   - Constant-time comparison
   - Email enumeration prevention
   - Secure token generation (256-bit)

3. **General:**
   - BCrypt password hashing
   - Input validation
   - HTTPS enforcement
   - JWT authentication
   - Rate limiting (via account lockout)

---

## H. COMMON ISSUES

### SMTP Email Not Sending

**Problem:** Email service throws exception

**Solutions:**
1. **Gmail:** Use App Password, not regular password
2. **Office365:** Enable "Allow less secure apps" or use OAuth
3. **Port 587** with TLS is standard
4. Check firewall settings
5. Verify SMTP credentials in `appsettings.json`

---

### TOTP Codes Not Working

**Problem:** Google Authenticator codes are rejected

**Solutions:**
1. **Check time sync:** Go to Google Authenticator ? Settings ? Time correction for codes ? Sync now
2. **Clock drift:** Server and phone must have accurate time
3. **Verify secret:** Make sure user scanned correct QR code
4. **Test window:** Code is valid for 30 seconds (±30 seconds tolerance)

---

### Migration Not Applied

**Problem:** Database doesn't have new columns

**Solution:**
```bash
# Stop the application
# Run migration
dotnet ef database update --project PayRexApplication\PayRex.API.csproj

# Or delete database and recreate
dotnet ef database drop --project PayRexApplication\PayRex.API.csproj
dotnet ef database update --project PayRexApplication\PayRex.API.csproj
```

---

## I. ARCHITECTURE

### Clean Architecture

```
Controllers
  ?? AuthController
?? TOTP endpoints
       ?? Password reset endpoints

Services
  ?? ITotpService / TotpService
  ?? IEmailService / EmailService
  ?? IPasswordResetTokenService / PasswordResetTokenService

Models
  ?? SaasUser (with TOTP + Reset fields)
  ?? User (with TOTP + Reset fields)

DTOs
  ?? TotpSetupDto
  ?? EnableTotpDto
  ?? VerifyTotpDto
  ?? ForgotPasswordDto
  ?? ResetPasswordDto

Data
  ?? AppDbContext (updated with new fields)

Migrations
  ?? 20260205200000_AddTotpAndPasswordResetFields
```

---

## J. PRODUCTION CHECKLIST

### Before Deploying

- [ ] Change JWT secret key in `appsettings.json`
- [ ] Configure production SMTP credentials
- [ ] Update `FrontendUrl` in API `appsettings.json`
- [ ] Update `ApiBaseUrl` in Web `appsettings.json`
- [ ] Enable HTTPS only
- [ ] Review token expiration times
- [ ] Test email delivery in production
- [ ] Test TOTP with multiple devices
- [ ] Configure proper CORS policies
- [ ] Set up monitoring/logging
- [ ] Review rate limiting settings

---

## K. SUPPORT

For issues or questions:
1. Check logs in `/PayRexApplication/logs/`
2. Review error messages in browser console
3. Test API endpoints with Postman/Swagger
4. Verify database columns exist
5. Check SMTP configuration

---

## L. FILES CREATED/MODIFIED

### New Files
- `PayRexApplication/Services/TotpService.cs`
- `PayRexApplication/Services/EmailService.cs`
- `PayRexApplication/Services/PasswordResetTokenService.cs`
- `PayRexApplication/DTOs/TotpSetupDto.cs`
- `PayRexApplication/DTOs/PasswordResetDto.cs`
- `PayRexApplication/Migrations/20260205200000_AddTotpAndPasswordResetFields.cs`
- `PayRex.Web/Pages/Auth/ForgotPassword.cshtml`
- `PayRex.Web/Pages/Auth/ForgotPassword.cshtml.cs`
- `PayRex.Web/Pages/Auth/ResetPassword.cshtml`
- `PayRex.Web/Pages/Auth/ResetPassword.cshtml.cs`
- `PayRex.Web/DTOs/PasswordResetRequestDto.cs`

### Modified Files
- `PayRexApplication/Models/SaasUser.cs` (added 4 fields)
- `PayRexApplication/Models/User.cs` (added 4 fields)
- `PayRexApplication/Controllers/AuthController.cs` (added endpoints, updated login)
- `PayRexApplication/Program.cs` (registered services)
- `PayRexApplication/appsettings.json` (added SMTP config)
- `PayRex.Web/Pages/Auth/Login.cshtml` (updated forgot password link)
- `PayRex.Web/Program.cs` (added HttpClient factory)
- `PayRex.Web/appsettings.json` (added ApiSettings)

---

**Implementation Date:** February 5, 2025  
**Framework:** .NET 9.0  
**Security Standards:** RFC 6238 (TOTP), SHA256 (Token Hashing), BCrypt (Password Hashing)
