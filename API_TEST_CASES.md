# API Test Cases - Postman Collection

## Environment Variables
Create these in Postman Environment:

```
BASE_URL = https://localhost:7000
JWT_TOKEN = (will be set after login)
USER_EMAIL = user@example.com
TOTP_SECRET = (will be set after setup)
RESET_TOKEN = (will be from email)
```

---

## 1. TOTP Tests

### 1.1 Setup TOTP
```http
POST {{BASE_URL}}/api/auth/totp/setup
Authorization: Bearer {{JWT_TOKEN}}
```

**Expected Response (200 OK):**
```json
{
  "secretKey": "JBSWY3DPEHPK3PXP",
  "qrCodeUri": "otpauth://totp/PayRex:user@example.com?secret=JBSWY3DPEHPK3PXP&issuer=PayRex&algorithm=SHA1&digits=6&period=30",
  "manualEntryKey": "JBSW Y3DP EHPK 3PXP"
}
```

**Post-Request Script:**
```javascript
pm.environment.set("TOTP_SECRET", pm.response.json().secretKey);
```

**Manual Step:** Scan QR code with Google Authenticator

---

### 1.2 Enable TOTP (Invalid Code)
```http
POST {{BASE_URL}}/api/auth/totp/enable
Authorization: Bearer {{JWT_TOKEN}}
Content-Type: application/json

{
  "totpCode": "000000"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "message": "Invalid TOTP code"
}
```

---

### 1.3 Enable TOTP (Valid Code)
```http
POST {{BASE_URL}}/api/auth/totp/enable
Authorization: Bearer {{JWT_TOKEN}}
Content-Type: application/json

{
  "totpCode": "{{GOOGLE_AUTH_CODE}}"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Two-factor authentication enabled successfully"
}
```

**Note:** Replace `{{GOOGLE_AUTH_CODE}}` with actual 6-digit code from Google Authenticator

---

### 1.4 Login - Step 1 (Email + Password)
```http
POST {{BASE_URL}}/api/auth/login
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "password": "YourPassword123!"
}
```

**Expected Response (200 OK) - When TOTP Enabled:**
```json
{
  "requireTotp": true,
  "message": "Please provide your TOTP code",
  "email": "user@example.com"
}
```

---

### 1.5 Login - Step 2 (TOTP Verification)
```http
POST {{BASE_URL}}/api/auth/totp/verify
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "totpCode": "{{GOOGLE_AUTH_CODE}}"
}
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "id": 1,
  "email": "user@example.com",
  "role": "Admin",
  "expiresAt": "2025-02-05T22:00:00Z"
}
```

**Post-Request Script:**
```javascript
pm.environment.set("JWT_TOKEN", pm.response.json().token);
```

---

### 1.6 Disable TOTP
```http
POST {{BASE_URL}}/api/auth/totp/disable
Authorization: Bearer {{JWT_TOKEN}}
```

**Expected Response (200 OK):**
```json
{
  "message": "Two-factor authentication disabled successfully"
}
```

---

### 1.7 Login Without TOTP
```http
POST {{BASE_URL}}/api/auth/login
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "password": "YourPassword123!"
}
```

**Expected Response (200 OK) - When TOTP Disabled:**
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

## 2. Password Reset Tests

### 2.1 Forgot Password (Valid Email)
```http
POST {{BASE_URL}}/api/auth/forgot-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

**Manual Step:** Check email for reset link

---

### 2.2 Forgot Password (Invalid Email)
```http
POST {{BASE_URL}}/api/auth/forgot-password
Content-Type: application/json

{
  "email": "nonexistent@example.com"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "If the email exists, a password reset link has been sent."
}
```

**Note:** Same response to prevent email enumeration

---

### 2.3 Reset Password (Invalid Token)
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "invalid-token-123",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "message": "Invalid reset token"
}
```

---

### 2.4 Reset Password (Expired Token)
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
"token": "{{EXPIRED_TOKEN}}",
  "newPassword": "NewPassword123!",
  "confirmPassword": "NewPassword123!"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "message": "Reset token has expired. Please request a new one."
}
```

---

### 2.5 Reset Password (Password Mismatch)
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "{{RESET_TOKEN}}",
  "newPassword": "NewPassword123!",
  "confirmPassword": "DifferentPassword123!"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "confirmPassword": [
    "Passwords do not match"
  ]
}
```

---

### 2.6 Reset Password (Weak Password)
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "{{RESET_TOKEN}}",
  "newPassword": "weak",
  "confirmPassword": "weak"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "newPassword": [
    "Password must be at least 8 characters",
    "Password must contain uppercase, lowercase, number, and special character"
  ]
}
```

---

### 2.7 Reset Password (Valid)
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "{{RESET_TOKEN}}",
  "newPassword": "NewSecure123!",
  "confirmPassword": "NewSecure123!"
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Password reset successfully. You can now log in with your new password."
}
```

---

### 2.8 Login with New Password
```http
POST {{BASE_URL}}/api/auth/login
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "password": "NewSecure123!"
}
```

**Expected Response (200 OK):**
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

### 2.9 Attempt Reuse of Reset Token
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "{{RESET_TOKEN}}",
  "newPassword": "AnotherPassword123!",
  "confirmPassword": "AnotherPassword123!"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "message": "Invalid or expired reset token"
}
```

**Note:** Token is cleared after successful use

---

## 3. Error Handling Tests

### 3.1 TOTP Setup Without Auth
```http
POST {{BASE_URL}}/api/auth/totp/setup
```

**Expected Response (401 Unauthorized):**

---

### 3.2 TOTP Enable Before Setup
```http
POST {{BASE_URL}}/api/auth/totp/enable
Authorization: Bearer {{JWT_TOKEN}}
Content-Type: application/json

{
  "totpCode": "123456"
}
```

**Expected Response (400 Bad Request) - If no setup:**
```json
{
  "message": "TOTP setup not initiated. Call /totp/setup first."
}
```

---

### 3.3 TOTP Verify for Non-Enabled Account
```http
POST {{BASE_URL}}/api/auth/totp/verify
Content-Type: application/json

{
  "email": "user-without-totp@example.com",
  "totpCode": "123456"
}
```

**Expected Response (400 Bad Request):**
```json
{
"message": "TOTP not enabled for this account"
}
```

---

### 3.4 Invalid Email Format
```http
POST {{BASE_URL}}/api/auth/forgot-password
Content-Type: application/json

{
  "email": "not-an-email"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "email": [
    "Invalid email format"
  ]
}
```

---

### 3.5 Missing Required Fields
```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "token": [
    "Reset token is required"
  ],
  "newPassword": [
    "New password is required"
  ],
  "confirmPassword": [
    "Please confirm your password"
  ]
}
```

---

## 4. Security Tests

### 4.1 SQL Injection Test
```http
POST {{BASE_URL}}/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com' OR '1'='1",
  "password": "anything"
}
```

**Expected Response (401 Unauthorized):**
```json
{
  "message": "Invalid credentials"
}
```

---

### 4.2 XSS Test
```http
POST {{BASE_URL}}/api/auth/forgot-password
Content-Type: application/json

{
  "email": "<script>alert('xss')</script>@example.com"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "email": [
    "Invalid email format"
  ]
}
```

---

### 4.3 Token Timing Attack Test
**Objective:** Verify constant-time comparison prevents timing attacks

```http
POST {{BASE_URL}}/api/auth/reset-password
Content-Type: application/json

{
  "email": "{{USER_EMAIL}}",
  "token": "aaaaaaaaaaaaaaaa",
  "newPassword": "Test123!",
  "confirmPassword": "Test123!"
}
```

**Expected:** Response time should be consistent regardless of how close token is to correct value

---

## Test Execution Order

1. **Initial Setup:**
   - Login to get JWT token
   - Test TOTP setup
 - Enable TOTP
   - Test TOTP login

2. **Password Reset:**
   - Request password reset
   - Check email
   - Copy token from email
   - Test reset with various scenarios
   - Verify login with new password

3. **Error Handling:**
   - Test all error scenarios
   - Verify appropriate error messages
   - Check status codes

4. **Security:**
   - Run security tests
   - Verify no sensitive data leakage
   - Check timing consistency

---

## Postman Collection Import

Save this as `PayRex-Auth-Tests.postman_collection.json`:

```json
{
  "info": {
    "name": "PayRex - TOTP & Password Reset",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "TOTP",
      "item": [
 {
       "name": "Setup TOTP",
      "request": {
        "method": "POST",
       "header": [
   {
     "key": "Authorization",
         "value": "Bearer {{JWT_TOKEN}}"
        }
            ],
            "url": {
         "raw": "{{BASE_URL}}/api/auth/totp/setup",
    "host": ["{{BASE_URL}}"],
   "path": ["api", "auth", "totp", "setup"]
     }
          }
        }
      ]
    }
  ]
}
```

---

**Note:** Replace `{{GOOGLE_AUTH_CODE}}` with actual codes from Google Authenticator app when testing.
