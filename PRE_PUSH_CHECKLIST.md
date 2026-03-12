# Pre-Push Security Checklist

Before pushing to the new repository, verify all these items:

## ? Sensitive Files Protection

- [ ] `.gitignore` includes `appsettings.json` and `appsettings.*.json`
- [ ] `.gitignore` includes `**/Program.cs`
- [ ] `.gitignore` includes `*.db`, `*.env`, `secrets.json`
- [ ] `.gitignore` includes `bin/`, `obj/`, `node_modules/`
- [ ] Template files created: `appsettings.TEMPLATE.json` (both projects)
- [ ] All real `appsettings.json` files are untracked

## ? Template Files Verification

### PayRexApplication/appsettings.TEMPLATE.json
- [ ] Contains placeholder for `ConnectionStrings:DefaultConnection`
- [ ] Contains placeholder for `Jwt:Key`
- [ ] Contains placeholder for `Smtp:*` settings
- [ ] Contains placeholder for `Cloudinary:*` settings
- [ ] Contains placeholder for `Recaptcha:*` settings
- [ ] NO real credentials present

### PayRex.Web/appsettings.TEMPLATE.json
- [ ] Contains placeholder for `ApiBaseUrl`
- [ ] Contains placeholder for `Jwt:Key`
- [ ] Contains placeholder for `Recaptcha:*` settings
- [ ] NO real credentials present

## ? Documentation

- [ ] README.md updated with setup instructions
- [ ] SETUP_GUIDE.md created with detailed steps
- [ ] Instructions for obtaining external API keys included
- [ ] Database setup instructions clear
- [ ] Both development and production scenarios covered

## ? Git Status Check

Run: `git status`

Should NOT see:
- ? `appsettings.json` (any)
- ? `appsettings.Development.json` (any)
- ? `appsettings.Production.json` (any)
- ? `Program.cs` files
- ? `.env` files
- ? Database files (`.db`, `.mdf`, `.ldf`)

Should see:
- ? `.gitignore`
- ? `appsettings.TEMPLATE.json` files
- ? `README.md`
- ? `SETUP_GUIDE.md`
- ? Migration scripts

## ? Verify Untracked Files

Run: `git ls-files | findstr appsettings`

Should ONLY show template files:
```
PayRex.Web/appsettings.TEMPLATE.json
PayRexApplication/appsettings.TEMPLATE.json
```

Should NOT show actual `appsettings.json` files!

## ? Check for Hardcoded Secrets

Search for these patterns in code:

- [ ] No hardcoded connection strings in `.cs` files
- [ ] No hardcoded API keys in code
- [ ] No hardcoded passwords in code
- [ ] No JWT secrets in code (should read from config)
- [ ] No email credentials in code

Run this search:
```powershell
# Search for potential secrets
git grep -i "password.*=.*\"" -- "*.cs"
git grep -i "connectionstring.*=.*\"" -- "*.cs"
git grep -i "apikey.*=.*\"" -- "*.cs"
```

Should return minimal or no results (only safe examples).

## ? Test Build Without Secrets

- [ ] Delete your local `appsettings.json` files
- [ ] Copy from templates: `Copy-Item **/appsettings.TEMPLATE.json **/appsettings.json`
- [ ] Verify build works: `dotnet build`
- [ ] Restore your actual configs

## ? Migration Files

- [ ] `migrate-to-new-repo.ps1` created
- [ ] `migrate-to-new-repo.bat` created
- [ ] Scripts are executable
- [ ] Scripts properly untrack sensitive files

## ? Binary and Generated Files

- [ ] `bin/` and `obj/` folders ignored
- [ ] `node_modules/` ignored
- [ ] `wwwroot/lib/` ignored (package manager files)
- [ ] Compiled CSS (`site.css`) handled appropriately

## ? Platform-Specific Files

- [ ] `.vs/` ignored (Visual Studio)
- [ ] `.vscode/` ignored (VS Code)
- [ ] `.idea/` ignored (Rider)
- [ ] `.DS_Store` ignored (macOS)
- [ ] `Thumbs.db` ignored (Windows)

## ? Database Files

- [ ] `*.db` ignored (SQLite)
- [ ] `*.mdf` ignored (SQL Server data)
- [ ] `*.ldf` ignored (SQL Server log)
- [ ] Migration files included (they're safe)

## ? Final Verification Commands

Run these before pushing:

```powershell
# 1. Check git status
git status

# 2. List all tracked files
git ls-tree -r HEAD --name-only | findstr -i "appsettings"
git ls-tree -r HEAD --name-only | findstr -i "Program.cs"

# 3. Check for large files
git ls-files | ForEach-Object { if (Test-Path $_) { (Get-Item $_).Length } } | Measure-Object -Maximum

# 4. Verify .gitignore is working
git check-ignore -v PayRexApplication/appsettings.json
# Should output: .gitignore:XX:appsettings.json...

# 5. Verify no secrets in commit history
git log --all --full-history --source -- "*appsettings.json"
```

## ? Post-Migration Verification

After pushing to new repo:

- [ ] Clone the new repo to a different directory
- [ ] Verify `appsettings.json` files are NOT present
- [ ] Verify template files ARE present
- [ ] Follow SETUP_GUIDE.md to ensure it works
- [ ] Verify build succeeds after configuration

## ?? Emergency: Secrets Already Pushed?

If you accidentally pushed secrets:

1. **Immediately rotate all credentials:**
   - Database passwords
   - JWT secret keys
   - API keys (Cloudinary, reCAPTCHA, SMTP)
   - Any other secrets

2. **Clean git history:**
   ```powershell
   # Use BFG Repo Cleaner or git filter-branch
   # This is complex - seek help if needed
   ```

3. **Force push cleaned history** (if repository is not shared yet)

4. **Update all team members** to fetch the cleaned history

## ?? Notes

- This checklist should be completed BEFORE running migration scripts
- Keep a backup of your current repository
- Test the migration in a separate directory first
- Document any project-specific secrets that developers will need

## ? Sign Off

- [ ] All checks above completed
- [ ] Ready to run migration script
- [ ] Backup created
- [ ] Team notified (if applicable)

Date: _______________
Performed by: _______________

---

## Quick Command Reference

```powershell
# Untrack file but keep local copy
git rm --cached path/to/file

# Check what's ignored
git status --ignored

# View gitignore rules for a file
git check-ignore -v filename

# List all tracked files
git ls-tree -r HEAD --name-only

# Search for patterns in tracked files
git grep -i "pattern"

# Check file size in repo
git ls-files | ForEach-Object { Get-Item $_ | Select-Object Name, Length }
```
