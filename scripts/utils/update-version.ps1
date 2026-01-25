#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automatic version management script for CRM Solution
    
.DESCRIPTION
    Updates version numbers in package.json and .csproj files based on update type
    
.PARAMETER Type
    Type of update: "major" (new feature), "minor" (bug fix), or "patch" (small fix)
    
.PARAMETER Description
    Description of changes made in this update
    
.EXAMPLE
    .\update-version.ps1 -Type major -Description "Added company branding settings"
    .\update-version.ps1 -Type minor -Description "Fixed login button alignment"
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("major", "minor", "patch")]
    [string]$Type,
    
    [Parameter(Mandatory=$true)]
    [string]$Description
)

$versionFile = "version.json"
$packageJsonFile = "CRM.Frontend\package.json"
$csprojFile = "CRM.Backend\src\CRM.Api\CRM.Api.csproj"

if (-not (Test-Path $versionFile)) {
    Write-Error "version.json not found in root directory"
    exit 1
}

# Read current version
$versionContent = Get-Content $versionFile | ConvertFrom-Json
$major = $versionContent.major
$minor = $versionContent.minor
$patch = $versionContent.patch

# Update version based on type
switch ($Type) {
    "major" {
        $major++
        $minor = 0
        $patch = 0
        Write-Host "📈 Major version bump: $major.0.0 (New Feature)" -ForegroundColor Green
    }
    "minor" {
        $minor++
        $patch = 0
        Write-Host "📊 Minor version bump: $major.$minor.0 (Bug Fix)" -ForegroundColor Yellow
    }
    "patch" {
        $patch++
        Write-Host "🔧 Patch version bump: $major.$minor.$patch (Small Fix)" -ForegroundColor Blue
    }
}

$newVersion = "$major.$minor.$patch"
$now = (Get-Date).ToString("yyyy-MM-dd")

# Update version.json
$versionContent.major = $major
$versionContent.minor = $minor
$versionContent.patch = $patch
$versionContent.lastUpdate = $now
$versionContent.description = $Description

$versionContent | ConvertTo-Json | Set-Content $versionFile
Write-Host "✅ Updated version.json to $newVersion"

# Update package.json
$packageJson = Get-Content $packageJsonFile | ConvertFrom-Json
$packageJson.version = $newVersion
$packageJson | ConvertTo-Json -Depth 10 | Set-Content $packageJsonFile
Write-Host "✅ Updated package.json to $newVersion"

# Update .csproj (AssemblyVersion format: 1.1.0.0)
$assemblyVersion = "$major.$minor.$patch.0"
$csprojContent = Get-Content $csprojFile -Raw
$csprojContent = $csprojContent -replace '<Version>.*?</Version>', "<Version>$newVersion</Version>"
$csprojContent = $csprojContent -replace '<AssemblyVersion>.*?</AssemblyVersion>', "<AssemblyVersion>$assemblyVersion</AssemblyVersion>"
$csprojContent = $csprojContent -replace '<FileVersion>.*?</FileVersion>', "<FileVersion>$assemblyVersion</FileVersion>"
Set-Content $csprojFile $csprojContent
Write-Host "✅ Updated CRM.Api.csproj to $newVersion"

Write-Host ""
Write-Host "🎉 Version update complete!" -ForegroundColor Cyan
Write-Host "   Version: $newVersion"
Write-Host "   Type: $Type"
Write-Host "   Description: $Description"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Review changes: git diff"
Write-Host "  2. Commit changes: git commit -m 'Version $newVersion: $Description'"
Write-Host "  3. Build: npm run build (frontend) / dotnet build (backend)"
Write-Host "  4. Deploy: ./deploy.ps1"
