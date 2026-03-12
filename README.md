# PayRex - Attendance Monitoring & Payroll SaaS System

A comprehensive SaaS solution for attendance monitoring and payroll management built with ASP.NET Core 9.0.

## ??? Architecture

Two-project solution:
- **PayRex.API** (`PayRexApplication/`): REST API backend on `https://localhost:5000`
- **PayRex.Web** (`PayRex.Web/`): Razor Pages frontend on `https://localhost:7002`

## ?? Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server (LocalDB, Express, or Full)
- Node.js (for Tailwind CSS compilation)
- Visual Studio 2022 or VS Code

### Configuration

1. **Clone the repository**
   ```bash
   git clone https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git
   cd PayRex---Attendance-Monitoring-Payroll-SaaS-System
   ```

2. **Configure API Settings**
   - Copy `PayRexApplication/appsettings.TEMPLATE.json` to `PayRexApplication/appsettings.json`
   - Update the following settings:
     - `ConnectionStrings:DefaultConnection` - Your SQL Server connection string
     - `Jwt:Key` - A secure random string (minimum 32 characters)
     - `Smtp:*` - Your email server settings (for password resets)
     - `Cloudinary:*` - Your Cloudinary credentials (for image uploads)
     - `Recaptcha:*` - Your Google reCAPTCHA keys

3. **Configure Web Settings**
   - Copy `PayRex.Web/appsettings.TEMPLATE.json` to `PayRex.Web/appsettings.json`
   - Update:
     - `ApiBaseUrl` - Should match your API URL (default: `https://localhost:5000`)
     - `Jwt:Key` - Must match the API's JWT key
     - `Recaptcha:*` - Same reCAPTCHA keys as API

### Database Setup

```powershell
# Navigate to the solution root
cd PayRex---Attendance-Monitoring-Payroll-SaaS-System

# Create and apply migrations
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```

### Build Frontend Assets

```powershell
# Navigate to web project
cd PayRex.Web

# Install dependencies
npm ci

# Build Tailwind CSS
npm run build
```

### Run the Application

**Option 1: Visual Studio**
- Set multiple startup projects: `PayRex.API` and `PayRex.Web`
- Press F5

**Option 2: Command Line**

Terminal 1 (API):
```powershell
dotnet run --project PayRexApplication/PayRex.API.csproj
```

Terminal 2 (Web):
```powershell
dotnet run --project PayRex.Web/PayRex.Web.csproj
```

Access the application at `https://localhost:7002`

## ?? Security Features

- JWT-based authentication with HttpOnly cookies
- BCrypt password hashing
- TOTP 2FA support
- Email-based password reset
- Role-based authorization (5 roles: SuperAdmin, Admin, HR, Employee, Accountant)
- Account lockout after 5 failed attempts
- Google reCAPTCHA integration

## ??? Technology Stack

### Backend
- ASP.NET Core 9.0
- Entity Framework Core 8.0
- SQL Server
- JWT Authentication
- BCrypt.Net
- MailKit (SMTP)

### Frontend
- Razor Pages
- Tailwind CSS 3.4
- Flowbite Components
- Vanilla JavaScript

### External Services
- Cloudinary (Image uploads)
- Google reCAPTCHA
- QuestPDF (PDF generation)
- PuppeteerSharp (Headless browser)

## ?? Project Structure

```
PayRex---Attendance-Monitoring-Payroll-SaaS-System/
??? PayRexApplication/      # API Project
?   ??? Controllers/           # API Controllers
?   ??? Models/     # Domain Models
?   ??? DTOs/         # Data Transfer Objects
?   ??? Services/     # Business Logic
?   ??? Data/  # DbContext & Migrations
?   ??? Enums/               # Application Enums
??? PayRex.Web/     # Web Project
?   ??? Pages/                 # Razor Pages
?   ??? Services/    # HTTP Client Services
?   ??? Configuration/         # Menu & Auth Config
?   ??? wwwroot/    # Static files
??? PayRex.Tests/              # Test Project (Placeholder)
```

## ?? Required Environment Variables

Create `appsettings.json` files from templates and configure:

**Critical Settings:**
- Database connection string
- JWT secret key (32+ characters)
- SMTP credentials
- Cloudinary API keys
- reCAPTCHA keys

**Never commit these files to version control!**

## ?? Deployment

The application is production-ready for:
- MonsterASP (runasp.net) - Web hosting
- DatabaseASP.net - SQL Server hosting

Ensure all `appsettings.json` files are configured with production values.

## ?? Development Workflow

### Adding Migrations

```powershell
dotnet ef migrations add MigrationName --project PayRexApplication/PayRex.API.csproj
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```

### Building Tailwind CSS

```powershell
cd PayRex.Web
npm run build       # Production build
npm run watch       # Development watch mode
```

### Code Style

- C# nullable reference types enabled
- Private fields prefixed with `_`
- DTOs suffixed with `Dto`
- EF entities use `[Table("lowercase")]` attributes
- Controllers return `Task<IActionResult>`

## ?? Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## ?? License

[Your License Here]

## ????? Author

[Your Name/Organization]

## ?? Issues

Report issues at: https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System/issues

## ?? Important Notes

- Both API and Web projects must run simultaneously
- API must start before Web for proper initialization
- Database auto-migrates on API startup
- All data queries are company-scoped (multi-tenant architecture)

