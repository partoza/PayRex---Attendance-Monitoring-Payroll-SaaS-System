# Quick Migration Guide

## Step-by-Step Migration Process

### 1. Prepare the Repository (DO THIS FIRST!)

```powershell
# Run the migration script
.\migrate-to-new-repo.ps1

# OR on Windows without PowerShell:
migrate-to-new-repo.bat
```

This script will:
- ? Untrack sensitive files from git
- ? Add .gitignore updates
- ? Add template files
- ? Commit all changes

### 2. Verify Everything is Safe

```powershell
# Check no sensitive files are tracked
git ls-files | Select-String -Pattern "appsettings.json"
# Should ONLY show *.TEMPLATE.json files

# Check gitignore is working
git status --ignored | Select-String -Pattern "appsettings.json"
# Should show ignored files

# View what will be pushed
git log --oneline -10
```

### 3. Add New Remote

```powershell
# Add the new repository as a remote
git remote add new-origin https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git

# Verify remotes
git remote -v
```

You should see:
```
origin      https://github.com/partoza/PayRex-Application (current repo)
new-origin  https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git (new repo)
```

### 4. Push to New Repository

```powershell
# Push main branch
git push new-origin main

# Push all branches (if you have others)
git push new-origin --all

# Push tags (if any)
git push new-origin --tags
```

### 5. Update Remote Configuration

```powershell
# Remove old origin
git remote remove origin

# Rename new-origin to origin
git remote rename new-origin origin

# Verify
git remote -v
```

Should now show:
```
origin  https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git
```

### 6. Set Upstream Branch

```powershell
git branch --set-upstream-to=origin/main main
```

### 7. Verify Migration Success

```powershell
# Pull from new repository
git pull origin main

# Should say "Already up to date"
```

### 8. Test Clone (Important!)

In a different directory:

```powershell
# Go to a different location
cd C:\Temp

# Clone the new repo
git clone https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git TestPayRex
cd TestPayRex

# Verify no sensitive files
Get-ChildItem -Recurse -Include "appsettings.json" -Exclude "*.TEMPLATE.json"
# Should return nothing!

# Verify templates exist
Get-ChildItem -Recurse -Include "appsettings.TEMPLATE.json"
# Should show both template files

# Clean up
cd ..
Remove-Item -Recurse -Force TestPayRex
```

### 9. Configure New Clone (For Team Members)

```powershell
# Copy templates to actual config files
Copy-Item PayRexApplication\appsettings.TEMPLATE.json PayRexApplication\appsettings.json
Copy-Item PayRex.Web\appsettings.TEMPLATE.json PayRex.Web\appsettings.json

# Edit the appsettings.json files with real credentials
code PayRexApplication\appsettings.json
code PayRex.Web\appsettings.json

# Run migrations
dotnet ef database update --project PayRexApplication/PayRex.API.csproj

# Build Tailwind CSS
cd PayRex.Web
npm install
npm run build
cd ..

# Start the application
dotnet run --project PayRexApplication/PayRex.API.csproj
# In another terminal:
dotnet run --project PayRex.Web/PayRex.Web.csproj
```

## Troubleshooting

### "Remote already exists"
```powershell
git remote remove new-origin
# Then try adding again
```

### "Nothing to commit"
The migration script already ran. Proceed to step 3.

### "Failed to push"
```powershell
# Check authentication
git config --global user.name "Your Name"
git config --global user.email "your-email@example.com"

# Ensure you're logged into GitHub
gh auth login
# OR use Personal Access Token
```

### Accidentally Pushed Secrets

1. **Immediately rotate all credentials!**
2. Delete the repository on GitHub
3. Clean git history:
   ```powershell
   git filter-branch --force --index-filter "git rm --cached --ignore-unmatch appsettings.json" --prune-empty --tag-name-filter cat -- --all
   ```
4. Force push

## Security Reminders

- ?? Never commit `appsettings.json` files
- ?? Never commit `Program.cs` if it contains secrets
- ?? Always use template files for reference
- ?? Rotate credentials if they're exposed
- ?? Use environment variables in production

## Next Steps After Migration

1. Update repository settings on GitHub:
   - Add description
   - Add topics/tags
   - Set up branch protection rules
   - Configure secrets (for CI/CD)

2. Set up GitHub Actions (if needed)
3. Add collaborators
4. Create initial issues/milestones
5. Update project documentation

## Support

For issues with migration, contact the repository administrator or create an issue.
