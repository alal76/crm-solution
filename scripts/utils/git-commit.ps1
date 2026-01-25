#!/usr/bin/env pwsh

$repoPath = "c:\Users\AbhishekLal\OneDrive - HSO\Documents\Work\Vibe\CRM"
$gitHubRepo = "https://github.com/YOUR_USERNAME/CRM.git"  # Update this with actual GitHub repo

Write-Host "Initializing Git repository..." -ForegroundColor Yellow
Set-Location $repoPath

# Initialize git if not already initialized
if (-not (Test-Path ".git")) {
    Write-Host "Creating new Git repository..."
    & git init
    Write-Host "✅ Git repository initialized" -ForegroundColor Green
} else {
    Write-Host "✅ Git repository already exists" -ForegroundColor Green
}

# Configure git user (if not already configured)
$userName = & git config user.name
$userEmail = & git config user.email

if (-not $userName) {
    Write-Host "Configuring Git user..."
    & git config user.name "Abhishek Lal"
    & git config user.email "abhi.lal@gmail.com"
    Write-Host "✅ Git user configured" -ForegroundColor Green
}

# Add all files
Write-Host ""
Write-Host "Adding files to Git..."
& git add -A

# Check what's staged
$stagedFiles = & git status --short
Write-Host "Files to be committed:" -ForegroundColor Cyan
Write-Host $stagedFiles

# Commit
Write-Host ""
Write-Host "Creating commit..."
$commitMessage = @"
v1.2.0: Code cleanup, unit tests, admin user setup, and automatic versioning

Changes:
- Removed unused imports from frontend components
- Added Jest unit tests for frontend (utilities, components, services, theme)
- Added xUnit tests for backend (entities, roles)
- Created DbSeed.cs with admin user initialization (abhi.lal@gmail.com)
- Implemented automatic versioning system (major/minor/patch)
- Added version.json as single source of truth for version tracking
- Created update-version.ps1 (PowerShell) and update-version.js (Node.js) scripts
- Added npm version commands for automated version bumping
- Added Settings link to footer with Material Design 3 styling
- Comprehensive VERSIONING.md documentation
"@

& git commit -m $commitMessage

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Commit created successfully" -ForegroundColor Green
} else {
    Write-Host "⚠️ Commit may have failed or repository was already up to date" -ForegroundColor Yellow
}

# Show log
Write-Host ""
Write-Host "Recent commits:" -ForegroundColor Cyan
& git log --oneline -5

Write-Host ""
Write-Host "Git repository is ready for push to GitHub" -ForegroundColor Green
Write-Host "Next step: Configure remote and push" -ForegroundColor Cyan
Write-Host "Run: git remote add origin $gitHubRepo" -ForegroundColor Gray
Write-Host "Run: git branch -M main" -ForegroundColor Gray
Write-Host "Run: git push -u origin main" -ForegroundColor Gray
