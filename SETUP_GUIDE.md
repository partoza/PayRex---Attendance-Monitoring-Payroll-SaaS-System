# Setup Guide for New Developers

## Quick Start Checklist

- [ ] Install .NET 9.0 SDK
- [ ] Install Node.js (v18+)
- [ ] Install SQL Server
- [ ] Clone repository
- [ ] Copy template files to appsettings.json
- [ ] Configure database connection
- [ ] Configure JWT secret
- [ ] Configure external services (optional)
- [ ] Run database migrations
- [ ] Build Tailwind CSS
- [ ] Start both projects

## Detailed Setup Instructions

### 1. Install Prerequisites

#### .NET 9.0 SDK
Download from: https://dotnet.microsoft.com/download/dotnet/9.0

Verify installation:
```powershell
dotnet --version
```

#### Node.js
Download from: https://nodejs.org/ (LTS version recommended)

Verify installation:
```powershell
node --version
npm --version
```

#### SQL Server
Options:
- SQL Server Express (Free): https://www.microsoft.com/sql-server/sql-server-downloads
- SQL Server LocalDB (comes with Visual Studio)
- SQL Server Developer Edition (Free)

### 2. Clone and Configure

```powershell
# Clone the repository
git clone https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git
cd PayRex---Attendance-Monitoring-Payroll-SaaS-System

# Copy template files
Copy-Item PayRexApplication\appsettings.TEMPLATE.json PayRexApplication\appsettings.json
Copy-Item PayRex.Web\appsettings.TEMPLATE.json PayRex.Web\appsettings.json
```

### 3. Configure Database

Edit `PayRexApplication/appsettings.json`:

**For SQL Server Express:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PayRexDB;Integrated Security=True;TrustServerCertificate=True;"
}
```

**For LocalDB:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PayRexDB;Integrated Security=True;TrustServerCertificate=True;"
}
```

**For SQL Server with authentication:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PayRexDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
}
```

### 4. Configure JWT Secret

Generate a secure random string (32+ characters). You can use this PowerShell command:

```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```

Update in **both** `appsettings.json` files:
```json
"Jwt": {
  "Key": "YOUR_GENERATED_SECRET_HERE",
  "Issuer": "PayRexAPI",
  "Audience": "PayRexWeb"
}
```

?? **Important:** The JWT key must be identical in both API and Web projects!

### 5. Configure External Services (Optional for Development)

#### Email (SMTP) - Required for password reset
```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "PayRex System"
}
```

For Gmail, you need to create an "App Password":
1. Enable 2FA on your Google account
2. Go to https://myaccount.google.com/apppasswords
3. Generate an app password for "Mail"

#### Cloudinary - Required for profile images
Sign up at: https://cloudinary.com (Free tier available)

```json
"Cloudinary": {
  "CloudName": "your-cloud-name",
  "ApiKey": "your-api-key",
  "ApiSecret": "your-api-secret"
}
```

#### Google reCAPTCHA - Required for login/register
Sign up at: https://www.google.com/recaptcha/admin

```json
"Recaptcha": {
  "SiteKey": "your-site-key",
  "SecretKey": "your-secret-key"
}
```

**For development**, you can use test keys:
- Site key: `6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI`
- Secret key: `6LeIxAcTAAAAAGG-vFI1TnRWxMZNFuojJ4WifJWe`

### 6. Run Database Migrations

```powershell
# From solution root
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```

This will:
- Create the PayRexDB database
- Apply all migrations
- Set up all tables

### 7. Build Frontend Assets

```powershell
cd PayRex.Web
npm install
npm run build
cd ..
```

### 8. Run the Application

#### Option A: Visual Studio 2022

1. Open `PayRexApplication.sln`
2. Right-click solution ? Properties
3. Select "Multiple startup projects"
4. Set both `PayRex.API` and `PayRex.Web` to "Start"
5. Press F5

#### Option B: Command Line

**Terminal 1 - API:**
```powershell
dotnet run --project PayRexApplication/PayRex.API.csproj
```

Wait for: `Now listening on: https://localhost:5000`

**Terminal 2 - Web:**
```powershell
dotnet run --project PayRex.Web/PayRex.Web.csproj
```

Wait for: `Now listening on: https://localhost:7002`

### 9. Access the Application

Open browser: `https://localhost:7002`

You may see a certificate warning - click "Advanced" and "Proceed" (development only).

## Default Login

After first run, you can register a new company and user through the registration page.

## Troubleshooting

### Database Connection Issues

**Error:** "Cannot open database"
- Check SQL Server is running
- Verify connection string
- Check user permissions

### Migration Issues

**Error:** "No migrations found"
```powershell
# Restore packages first
dotnet restore
# Then try migration
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```

### Port Already in Use

**Error:** "Address already in use"
- Change ports in `Properties/launchSettings.json`
- Or kill the process using the port

### Tailwind CSS Not Compiling

```powershell
cd PayRex.Web
# Delete node_modules and reinstall
Remove-Item -Recurse node_modules
npm install
npm run build
```

### JWT Token Issues

**Error:** "Unauthorized"
- Ensure JWT keys match in both appsettings.json files
- Clear browser cookies
- Restart both projects

## Development Tips

### Hot Reload

Both projects support hot reload:
```powershell
dotnet watch --project PayRexApplication/PayRex.API.csproj
dotnet watch --project PayRex.Web/PayRex.Web.csproj
```

### Tailwind Watch Mode

For CSS development:
```powershell
cd PayRex.Web
npm run watch
```

### Database Reset

To start fresh:
```powershell
dotnet ef database drop --project PayRexApplication/PayRex.API.csproj
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```

## Next Steps

- Read the main [README.md](README.md) for architecture details
- Review code style guidelines in `.github/copilot-instructions.md`
- Check out the [Project Guidelines](PROJECT_GUIDELINES.md)

## Getting Help

- Check existing issues: https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System/issues
- Create new issue if problem persists
- Contact the development team

## Security Reminder

?? **Never commit `appsettings.json` files to version control!**

These files contain sensitive credentials and are already in `.gitignore`.
