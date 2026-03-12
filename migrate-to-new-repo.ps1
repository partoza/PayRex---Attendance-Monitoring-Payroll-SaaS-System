#!/usr/bin/env pwsh
# Migration Script for PayRex to New Repository

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "PayRex Repository Migration Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if we're in a git repository
if (-not (Test-Path .git)) {
    Write-Host "? Error: Not a git repository!" -ForegroundColor Red
    Write-Host "Please run this script from the repository root." -ForegroundColor Yellow
    exit 1
}

Write-Host "? Git repository detected" -ForegroundColor Green

# Step 2: Check for uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Host ""
    Write-Host "??  WARNING: You have uncommitted changes:" -ForegroundColor Yellow
    Write-Host $status
  Write-Host ""
    $continue = Read-Host "Do you want to commit these changes before migration? (y/n)"
    if ($continue -eq 'y') {
 git add .
        $message = Read-Host "Enter commit message"
        git commit -m $message
        Write-Host "? Changes committed" -ForegroundColor Green
    }
}

# Step 3: Remove tracked sensitive files from git history
Write-Host ""
Write-Host "Step 1: Removing sensitive files from git tracking..." -ForegroundColor Cyan

$sensitiveFiles = @(
    "PayRexApplication/appsettings.json",
    "PayRexApplication/appsettings.Development.json",
    "PayRexApplication/appsettings.Production.json",
    "PayRex.Web/appsettings.json",
    "PayRex.Web/appsettings.Development.json",
    "PayRex.Web/appsettings.Production.json",
  "PayRexApplication/Program.cs",
    "PayRex.Web/Program.cs"
)

foreach ($file in $sensitiveFiles) {
 if (Test-Path $file) {
        Write-Host "  Untracking: $file" -ForegroundColor Yellow
        git rm --cached $file 2>$null
    }
}

Write-Host "? Sensitive files untracked" -ForegroundColor Green

# Step 4: Verify .gitignore
Write-Host ""
Write-Host "Step 2: Verifying .gitignore..." -ForegroundColor Cyan

if (-not (Test-Path .gitignore)) {
    Write-Host "? Error: .gitignore not found!" -ForegroundColor Red
    exit 1
}

$gitignoreContent = Get-Content .gitignore -Raw
$requiredPatterns = @("appsettings.json", "appsettings.*.json", "**/Program.cs", "*.db", "*.env")
$missing = @()

foreach ($pattern in $requiredPatterns) {
    if ($gitignoreContent -notmatch [regex]::Escape($pattern)) {
        $missing += $pattern
    }
}

if ($missing.Count -gt 0) {
    Write-Host "??  WARNING: .gitignore is missing patterns:" -ForegroundColor Yellow
    foreach ($m in $missing) {
     Write-Host "    - $m" -ForegroundColor Yellow
    }
} else {
    Write-Host "? .gitignore is properly configured" -ForegroundColor Green
}

# Step 5: Verify template files exist
Write-Host ""
Write-Host "Step 3: Verifying template files..." -ForegroundColor Cyan

$templateFiles = @(
    "PayRexApplication/appsettings.TEMPLATE.json",
    "PayRex.Web/appsettings.TEMPLATE.json"
)

$templatesMissing = @()
foreach ($template in $templateFiles) {
    if (-not (Test-Path $template)) {
    $templatesMissing += $template
    } else {
        Write-Host "  ? Found: $template" -ForegroundColor Green
    }
}

if ($templatesMissing.Count -gt 0) {
    Write-Host "? Error: Missing template files:" -ForegroundColor Red
    foreach ($m in $templatesMissing) {
    Write-Host "    - $m" -ForegroundColor Red
    }
  exit 1
}

# Step 6: Commit the changes
Write-Host ""
Write-Host "Step 4: Committing migration changes..." -ForegroundColor Cyan

git add .gitignore
git add **/appsettings.TEMPLATE.json
git add README.md
git add SETUP_GUIDE.md 2>$null

$commitMsg = "chore: prepare for repository migration

- Updated .gitignore to exclude sensitive files
- Added appsettings.TEMPLATE.json files
- Removed tracked sensitive files
- Updated README with setup instructions
- Added SETUP_GUIDE.md for new developers"

git commit -m $commitMsg

if ($LASTEXITCODE -eq 0) {
    Write-Host "? Migration changes committed" -ForegroundColor Green
} else {
Write-Host "??  No changes to commit (already done)" -ForegroundColor Yellow
}

# Step 7: Instructions for pushing to new repository
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Add the new remote repository:" -ForegroundColor White
Write-Host "   git remote add new-origin https://github.com/partoza/PayRex---Attendance-Monitoring-Payroll-SaaS-System.git" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Push to the new repository:" -ForegroundColor White
Write-Host "   git push new-origin main" -ForegroundColor Gray
Write-Host ""
Write-Host "3. (Optional) Remove old remote and rename new one:" -ForegroundColor White
Write-Host "   git remote remove origin" -ForegroundColor Gray
Write-Host "   git remote rename new-origin origin" -ForegroundColor Gray
Write-Host ""
Write-Host "4. (Optional) Push all branches and tags:" -ForegroundColor White
Write-Host "   git push origin --all" -ForegroundColor Gray
Write-Host "   git push origin --tags" -ForegroundColor Gray
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "??  IMPORTANT REMINDERS:" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "• appsettings.json files are now ignored" -ForegroundColor Yellow
Write-Host "• Use appsettings.TEMPLATE.json as reference" -ForegroundColor Yellow
Write-Host "• Never commit sensitive credentials" -ForegroundColor Yellow
Write-Host "• Update SETUP_GUIDE.md with any team-specific instructions" -ForegroundColor Yellow
Write-Host ""
Write-Host "? Migration preparation complete!" -ForegroundColor Green
Write-Host ""
