#!/usr/bin/env pwsh

# VS Code Git Configuration Setup Script (PowerShell)
# Configures VS Code to use the system git installation on Windows

$ErrorActionPreference = "Stop"

# Colors for output
$colors = @{
    'Red'    = 'Red'
    'Green'  = 'Green'
    'Yellow' = 'Yellow'
    'Blue'   = 'Blue'
    'Cyan'   = 'Cyan'
}

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = 'White'
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Header {
    param([string]$Text)
    Write-ColorOutput "======================================" $colors.Blue
    Write-ColorOutput $Text $colors.Blue
    Write-ColorOutput "======================================" $colors.Blue
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✓ $Message" $colors.Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "⚠ $Message" $colors.Yellow
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "❌ $Message" $colors.Red
    exit 1
}

# Main script
Write-Header "VS Code Git Configuration Setup (PowerShell)"
Write-Host ""

# Detect OS (Windows)
if (-not [System.Environment]::OSVersion.Platform -eq 'Win32NT') {
    Write-Error "This script is designed for Windows. Use setup-vscode-git.sh for macOS/Linux"
}

Write-ColorOutput "Detected OS: Windows" $colors.Yellow

# Determine VS Code settings path
$appDataPath = [Environment]::GetFolderPath("ApplicationData")
$vsCodeSettingsPath = Join-Path $appDataPath "Code\User\settings.json"

Write-ColorOutput "VS Code Settings File: $vsCodeSettingsPath" $colors.Yellow
Write-Host ""

# Find git executable
try {
    $gitPath = (Get-Command git -ErrorAction Stop).Source
} catch {
    Write-Error "Git not found in PATH. Please install Git for Windows first."
}

if (-not (Test-Path $gitPath)) {
    Write-Error "Git executable not found at: $gitPath"
}

Write-Success "Git found at: $gitPath"
Write-Success "Git version: $(git --version)"
Write-Host ""

# Create settings directory if it doesn't exist
$settingsDir = Split-Path -Parent $vsCodeSettingsPath
if (-not (Test-Path $settingsDir)) {
    Write-ColorOutput "Creating VS Code settings directory..." $colors.Yellow
    New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null
    Write-Success "Settings directory created"
}

# Backup existing settings.json if it exists
if (Test-Path $vsCodeSettingsPath) {
    $timestamp = Get-Date -Format "yyyyMMddHHmmss"
    $backupFile = "$vsCodeSettingsPath.backup.$timestamp"
    Write-ColorOutput "Backing up existing settings.json..." $colors.Yellow
    Copy-Item $vsCodeSettingsPath $backupFile -Force
    Write-Success "Backup saved to: $backupFile"
    Write-Host ""
}

# Create or update settings.json
Write-ColorOutput "Updating VS Code settings..." $colors.Yellow

try {
    $settings = if (Test-Path $vsCodeSettingsPath) {
        Get-Content $vsCodeSettingsPath -Raw | ConvertFrom-Json
    } else {
        @{}
    }
    
    # Normalize git path for JSON (use forward slashes)
    $gitPathNormalized = $gitPath.Replace('\', '/')
    
    # Update git.path setting
    $settings | Add-Member -MemberType NoteProperty -Name "git.path" -Value $gitPathNormalized -Force
    
    # Write updated settings
    $settings | ConvertTo-Json -Depth 10 | Set-Content $vsCodeSettingsPath -Encoding UTF8
    
    Write-Success "git.path set to: $gitPathNormalized"
} catch {
    Write-Error "Failed to update settings.json: $_"
}

Write-Host ""
Write-Header "✓ Setup Complete!"
Write-Host ""

Write-ColorOutput "Next steps:" $colors.Yellow
Write-Host "1. Reload VS Code: Ctrl+Shift+P → 'Developer: Reload Window'"
Write-Host "2. Check Source Control panel (Ctrl+Shift+G)"
Write-Host "3. Your git changes should now appear"
Write-Host ""

# Verification
Write-ColorOutput "Verification:" $colors.Yellow
$gitPathSetting = (Get-Content $vsCodeSettingsPath -Raw | ConvertFrom-Json)."git.path"
if ($gitPathSetting) {
    Write-Success "git.path = $gitPathSetting"
} else {
    Write-Warning "git.path setting not found"
}
