# PayRex — MonsterASP.NET Deployment Guide

This document explains the step-by-step process for deploying the **PayRex** Attendance Monitoring and Payroll SaaS System to **MonsterASP.NET (runasp.net)**.

PayRex has **two apps** that must both be deployed — the **API (backend)** and the **Web (frontend).**

---

## Step 1 — Create a MonsterASP.NET Account

1. Go to https://monsterasp.net and sign up
2. Choose **Free** or **Premium** plan

> Free plan = free subdomain only (e.g. `yoursite.siteasp.net`). Premium = custom domain support.

---

## Step 2 — Create Two Websites on MonsterASP

**PayRex needs two separate apps — one for the API, one for the Web.**

1. Log in to the Control Panel
2. Go to **Websites → Create New Website**
3. Create the **API app** — name it `payrex-api`
4. Create the **Web app** — name it `payrex-web`
5. For both apps set:
   - Application type: **ASP.NET Core**
   - Runtime: **.NET 9**
   - Hosting model: **InProcess** (better performance)
6. Note down both assigned URLs:
   - API: `https://payrex-api.siteasp.net`
   - Web: `https://payrex-web.siteasp.net`

---

## Step 3 — Set Up Production Database

**MonsterASP cannot host SQL Server — use DatabaseASP.net instead.**

1. Go to https://databaseasp.net and sign up
2. Go to **Databases → Create New Database**
3. Select **MSSQL**
4. Save your **server name, database name, username, and password**
5. Run migrations from your machine:
```powershell
dotnet ef database update --project PayRexApplication/PayRex.API.csproj
```
> This creates all the necessary tables in your production database.

---

## Step 4 — Update Config Files for Production

**Update both `appsettings.json` files with your live production values.**

**API** — `PayRexApplication/appsettings.json`:

| Setting | What to put |
|---|---|
| `ConnectionStrings:DefaultConnection` | Your **DatabaseASP.net** credentials |
| `Jwt:Key` | Secret key — minimum 32 characters |
| `Smtp:*` | Your live email server credentials |
| `Cloudinary:*` | Your Cloudinary credentials |
| `Recaptcha:*` | Your Google reCAPTCHA keys |

**Web** — `PayRex.Web/appsettings.json`:

| Setting | What to put |
|---|---|
| `ApiBaseUrl` | Your live API URL — `https://payrex-api.siteasp.net` |
| `Jwt:Key` | **Must match the API key exactly** |
| `Recaptcha:*` | Same reCAPTCHA keys as API |

> ⚠️ Never upload `appsettings.json` to GitHub — it contains sensitive credentials.

---

## Step 5 — Build Frontend CSS

**Compiles Tailwind CSS so the website looks correct on the live server:**
```powershell
cd PayRex.Web
npm ci
npm run build
```
> Skip this and the website will load completely unstyled.

---

## Step 6 — Publish Both Projects

**Publish manually inside Visual Studio — no PowerShell needed.**

### Publish the API
1. Open the project in **Visual Studio 2022**
2. In **Solution Explorer** find `PayRex.API`
3. **Right-click** `PayRex.API` → click **Publish**
4. Select **Folder** as the publish target → click **Next**
5. Set the output folder to `./publish/api`
6. Click **Finish** then click **Publish**

### Publish the Web
1. Repeat the same steps but right-click on `PayRex.Web`
2. Set the output folder to `./publish/web`
3. Click **Finish** then click **Publish**

After both are done you will have:
```
publish/
├── api/     ← API published files
└── web/     ← Web published files
```

---

## Step 7 — Deploy to MonsterASP

**Choose one of these deployment methods:**

### Option A — WebDeploy *(Recommended — easiest)*
1. In Control Panel go to your website → **Deploy (FTP/WebDeploy/Git)**
2. Click **Activate WebDeploy** and download the `.publishSettings` file
3. In Visual Studio right-click your project → **Publish**
4. Click **Import Profile** → select the `.publishSettings` file
5. Click **Publish**

> Visual Studio will automatically build and deploy in one click.

### Option B — ZIP Upload
1. Zip `./publish/api` → save as `payrex-api.zip`
2. Zip `./publish/web` → save as `payrex-web.zip`
3. In Control Panel go to each app → **Deploy → ZIP Upload**
4. Upload each zip to its corresponding app

> ⚠️ Zip the **contents inside** the folder, not the folder itself.

### Option C — FTP Upload
1. In Control Panel go to **Deploy (FTP/WebDeploy/Git)** and get your FTP credentials
2. Download and open **FileZilla** at https://filezilla-project.org
3. Connect using your FTP credentials
4. Upload everything from `./publish/api` and `./publish/web` to each app's server root

### Option D — GitHub Auto-Deploy
1. In Control Panel go to **Deploy → GitHub**
2. Paste your GitHub Personal Access Token
3. Select your repository and branch
4. Enable **auto-deploy** — every push will automatically redeploy

> Best for ongoing development where you push updates frequently.

---

## Step 8 — Set Environment Variables

**In Control Panel go to each app → Settings → Environment Variables:**

**API app:**
```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = Server=...;Database=PayRexDB;...
Jwt__Key = YourSecretKey32CharsMinimum!
```

**Web app:**
```
ASPNETCORE_ENVIRONMENT = Production
ApiBaseUrl = https://payrex-api.siteasp.net
Jwt__Key = YourSecretKey32CharsMinimum!
```
> Use double underscores `__` for nested keys (e.g. `Jwt__Key` = `Jwt:Key` in JSON)

---

## Step 9 — Start the Applications

> ⚠️ Always start the **API first.**

1. In Control Panel → go to the **API app** → click **Start**
2. Wait until it's fully running
3. Then go to the **Web app** → click **Start**

---

## Step 10 — Verify Deployment

Open your browser and check:
- API: `https://payrex-api.siteasp.net`
- Web: `https://payrex-web.siteasp.net`

✅ If the Web loads and data appears — deployment is successful.

---

## Optional — Custom Domain *(Premium Plans Only)*

1. Buy a domain from Namecheap, GoDaddy, or Cloudflare
2. Add these DNS records at your registrar:

```
Type    Host    Value
A       @       [MonsterASP IP from Control Panel]
CNAME   www     your-site.siteasp.net
```
3. In Control Panel go to **Domains → Add Domain**
4. Enable HTTPS — go to **Websites → your site → HTTPS/SSL → Enable Let's Encrypt**

> DNS propagation can take up to **24–48 hours.**

---

## Troubleshooting

| Problem | Fix |
|---|---|
| 500 Error | Check error logs in Control Panel, verify all config values |
| Web loads but no data | `ApiBaseUrl` must match the live API URL exactly |
| Login fails | `Jwt:Key` must be identical in both API and Web configs |
| Database error | Check credentials and include `TrustServerCertificate=True` |
| Static files return 404 | Ensure `app.UseStaticFiles()` is in `Program.cs` |
| Images not uploading | Verify Cloudinary credentials |
| Emails not sending | Verify SMTP credentials |
| CORS error | API must allow requests from the Web app's URL |
| Files locked on upload | Stop the site in Control Panel first, then re-upload |

---

## Useful Links

| Resource | URL |
|---|---|
| MonsterASP.NET | https://monsterasp.net |
| MonsterASP Docs | https://help.monsterasp.net |
| DatabaseASP.net | https://databaseasp.net |
| .NET SDK Download | https://dotnet.microsoft.com/download |
| Cloudinary | https://cloudinary.com |
| Google reCAPTCHA | https://www.google.com/recaptcha/admin/create |
| FileZilla FTP | https://filezilla-project.org |

---

## Quick Reference Summary

| Step | What you do |
|---|---|
| 1 | Create MonsterASP account and two websites |
| 2 | Set up DatabaseASP.net and run migrations |
| 3 | Update both `appsettings.json` for production |
| 4 | Build Tailwind CSS |
| 5 | Publish both projects via Visual Studio |
| 6 | Deploy via WebDeploy, ZIP, FTP, or GitHub |
| 7 | Set environment variables |
| 8 | Start API first, then Web |
| 9 | Verify at your live URLs ✅ |
