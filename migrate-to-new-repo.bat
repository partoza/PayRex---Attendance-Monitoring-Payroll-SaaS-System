@echo off
REM Migration Script for PayRex to New Repository (Windows Batch)

echo ================================================
echo PayRex Repository Migration Script
echo ================================================
echo.

REM Check if we're in a git repository
if not exist .git (
    echo [ERROR] Not a git repository!
    echo Please run this script from the repository root.
    exit /b 1
)

echo [OK] Git repository detected

REM Step 1: Remove tracked sensitive files
echo.
echo Step 1: Removing sensitive files from git tracking...

git rm --cached "PayRexApplication/appsettings.json" 2>nul
git rm --cached "PayRexApplication/appsettings.Development.json" 2>nul
git rm --cached "PayRexApplication/appsettings.Production.json" 2>nul
git rm --cached "PayRex.Web/appsettings.json" 2>nul
git rm --cached "PayRex.Web/appsettings.Development.json" 2>nul
git rm --cached "PayRex.Web/appsettings.Production.json" 2>nul
git rm --cached "PayRexApplication/Program.cs" 2>nul
git rm --cached "PayRex.Web/Program.cs" 2>nul

echo [OK] Sensitive files untracked

REM Step 2: Add migration files
echo.
echo Step 2: Adding migration files...

git add .gitignore
git add PayRexApplication\appsettings.TEMPLATE.json
git add PayRex.Web\appsettings.TEMPLATE.json
git add README.md
git add SETUP_GUIDE.md

echo [OK] Migration files staged

REM Step 3: Commit changes
echo.
echo Step 3: Committing migration changes...

git commit -m "chore: prepare for repository migration - Updated .gitignore to exclude sensitive files - Added appsettings.TEMPLATE.json files - Removed tracked sensitive files - Updated README with setup instructions - Added SETUP_GUIDE.md"

echo [OK] Changes committed

REM Step 4: Show next steps
echo.
echo ================================================
echo Next Steps:
echo ================================================
echo.
echo 1. Add the new remote repository:
echo    git remote add new-origin https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git
echo.
echo 2. Push to the new repository:
echo    git push new-origin main
echo.
echo 3. (Optional) Remove old remote and rename:
echo git remote remove origin
echo git remote rename new-origin origin
echo.
echo 4. (Optional) Push all branches:
echo    git push origin --all
echo    git push origin --tags
echo.
echo ================================================
echo IMPORTANT REMINDERS:
echo ================================================
echo * appsettings.json files are now ignored
echo * Use appsettings.TEMPLATE.json as reference
echo * Never commit sensitive credentials
echo.
echo [OK] Migration preparation complete!
echo.

pause
