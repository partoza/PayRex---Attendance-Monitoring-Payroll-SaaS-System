# ?? Quick Command Reference

## Database Migration

### Apply Migration (Recommended)
```bash
# Stop application first!
dotnet ef database update --project "PayRexApplication\PayRex.API.csproj" --startup-project "PayRexApplication\PayRex.API.csproj"
```

### Reset Database (If migration fails)
```bash
dotnet ef database drop --project "PayRexApplication\PayRex.API.csproj" --force
dotnet ef database update --project "PayRexApplication\PayRex.API.csproj"
```

---

## Run Applications

### Start API
```bash
cd PayRexApplication
dotnet run --project PayRex.API.csproj
```

### Start Web
```bash
cd PayRex.Web
dotnet run
```

---

## Test API Endpoints (PowerShell)

### 1. Get JWT Token (Login)
```powershell
$response = Invoke-RestMethod -Uri "https://localhost:7000/api/auth/login" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"email":"partozarex@payrex.com","password":"PartozaJohn12345!"}'
$token = $response.token
Write-Host "Token: $token"
```

### 2. Setup TOTP
```powershell
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/totp/setup" `
  -Method POST `
  -Headers @{Authorization="Bearer $token"}
```

### 3. Enable TOTP (Replace 123456 with actual code)
```powershell
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/totp/enable" `
  -Method POST `
  -Headers @{Authorization="Bearer $token"; "Content-Type"="application/json"} `
  -Body '{"totpCode":"123456"}'
```

### 4. Request Password Reset
```powershell
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/forgot-password" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"email":"your-email@example.com"}'
```

### 5. Test Email Service
```powershell
Invoke-RestMethod -Uri "https://localhost:7000/api/auth/test-email?email=your-email@example.com" `
  -Method GET `
  -Headers @{Authorization="Bearer $token"}
```

---

## Test API Endpoints (Bash/curl)

### 1. Get JWT Token (Login)
```bash
TOKEN=$(curl -s -X POST https://localhost:7000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"partozarex@payrex.com","password":"PartozaJohn12345!"}' \
  -k | jq -r '.token')
echo "Token: $TOKEN"
```

### 2. Setup TOTP
```bash
curl -X POST https://localhost:7000/api/auth/totp/setup \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

### 3. Enable TOTP (Replace 123456 with actual code)
```bash
curl -X POST https://localhost:7000/api/auth/totp/enable \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"totpCode":"123456"}' \
  -k
```

### 4. Request Password Reset
```bash
curl -X POST https://localhost:7000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"your-email@example.com"}' \
  -k
```

---

## Swagger UI

### Open Swagger
Navigate to: **https://localhost:7000/swagger**

All endpoints are documented and testable through Swagger UI.

---

## Razor Pages

### URLs
- Login: https://localhost:7001/Auth/Login
- Register: https://localhost:7001/Auth/Register
- Forgot Password: https://localhost:7001/Auth/ForgotPassword
- Reset Password: https://localhost:7001/Auth/ResetPassword?email=...&token=...
- Dashboard: https://localhost:7001/Dashboard

---

## Troubleshooting Commands

### Check if ports are in use
```bash
# Check port 7000 (API)
netstat -ano | findstr :7000

# Check port 7001 (Web)
netstat -ano | findstr :7001
```

### Kill process on port (if needed)
```bash
# Replace <PID> with process ID from netstat
taskkill /F /PID <PID>
```

### View database tables
```sql
-- Connect to (localdb)\MSSQLLocalDB
USE PayRexDatabase;

-- Check if new columns exist
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'saasUsers'
AND COLUMN_NAME IN ('isTwoFactorEnabled', 'totpSecretKey', 'passwordResetTokenHash', 'passwordResetTokenExpiry');
```

---

## Build Commands

### Clean and Rebuild
```bash
dotnet clean PayRexApplication\PayRex.API.csproj
dotnet build PayRexApplication\PayRex.API.csproj

dotnet clean PayRex.Web\PayRex.Web.csproj
dotnet build PayRex.Web\PayRex.Web.csproj
```

### Restore packages
```bash
dotnet restore PayRexApplication\PayRex.API.csproj
dotnet restore PayRex.Web\PayRex.Web.csproj
```

---

## Gmail SMTP Setup

### Quick Gmail Setup
1. Go to: https://myaccount.google.com/apppasswords
2. Select "Mail" as the app
3. Generate password
4. Copy 16-character password
5. Update `appsettings.json`:

```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "abcd efgh ijkl mnop",
  "FromEmail": "noreply@payrex.com",
  "FromName": "PayRex Support",
  "EnableSsl": true
}
```

---

## Test Default User

### Default SaaS Super Admin
- **Email:** partozarex@payrex.com
- **Password:** PartozaJohn12345!
- **Role:** SuperAdmin

Use this account to test TOTP and password reset features.

---

## Quick Status Check

### Check if Migration Applied
```bash
dotnet ef migrations list --project "PayRexApplication\PayRex.API.csproj"
```

Look for: `20260205200000_AddTotpAndPasswordResetFields`

### Check if SMTP Configured
Open `PayRexApplication/appsettings.json` and verify:
- Smtp:Username is set
- Smtp:Password is set
- Both are not default placeholders

---

## Emergency: Reset Everything

### Complete Reset
```bash
# 1. Stop all running applications
# 2. Drop database
dotnet ef database drop --project "PayRexApplication\PayRex.API.csproj" --force

# 3. Clean build
dotnet clean PayRexApplication\PayRex.API.csproj
dotnet clean PayRex.Web\PayRex.Web.csproj

# 4. Restore packages
dotnet restore PayRexApplication\PayRex.API.csproj
dotnet restore PayRex.Web\PayRex.Web.csproj

# 5. Rebuild
dotnet build PayRexApplication\PayRex.API.csproj
dotnet build PayRex.Web\PayRex.Web.csproj

# 6. Apply migrations
dotnet ef database update --project "PayRexApplication\PayRex.API.csproj"

# 7. Start applications
cd PayRexApplication
dotnet run --project PayRex.API.csproj
```

---

## Logs Location

### API Logs
Check Visual Studio Output window or console where API is running

### Web Logs
Check Visual Studio Output window or console where Web is running

---

## Success Indicators

### ? Migration Applied
```sql
-- Run this query
SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'saasUsers'
AND COLUMN_NAME IN ('isTwoFactorEnabled', 'totpSecretKey', 'passwordResetTokenHash', 'passwordResetTokenExpiry');

-- Should return: 4
```

### ? SMTP Working
```bash
# Call test endpoint
curl -X GET "https://localhost:7000/api/auth/test-email?email=your-email@example.com" \
  -H "Authorization: Bearer $TOKEN" \
  -k

# Should return: {"message":"Test email sent successfully"}
```

### ? TOTP Working
```bash
# Setup returns QR code URI
curl -X POST https://localhost:7000/api/auth/totp/setup \
  -H "Authorization: Bearer $TOKEN" \
  -k

# Should return: {"secretKey":"...","qrCodeUri":"otpauth://...","manualEntryKey":"..."}
```

---

**Need Help?** Check the full documentation files in the root directory!
