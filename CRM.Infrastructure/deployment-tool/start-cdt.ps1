#Requires -Version 5.1
<#
.SYNOPSIS
    CRM CDT Launcher for Windows
.DESCRIPTION
    Self-bootstraps Python, venv, and dependencies. Opens browser to CDT wizard.
.PARAMETER Port
    Port to run CDT on (default: 5050)
.PARAMETER NoBrowser
    Do not auto-open browser
.PARAMETER Headless
    Run without browser
.PARAMETER ResetVenv
    Delete and recreate the Python virtual environment
#>
param(
    [int]$Port = 5050,
    [switch]$NoBrowser,
    [switch]$Headless,
    [switch]$ResetVenv
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$CdtVersion = "0.609.1"
$VenvDir = Join-Path $ScriptDir ".venv"
$CdtBinDir = Join-Path $env:USERPROFILE ".crm-cdt\bin"
$CdtSnapDir = Join-Path $env:USERPROFILE ".crm-cdt\snapshots"

function Write-CDTInfo  { Write-Host "[INFO] $args" -ForegroundColor Green }
function Write-CDTWarn  { Write-Host "[WARN] $args" -ForegroundColor Yellow }
function Write-CDTError { Write-Host "[ERROR] $args" -ForegroundColor Red }
function Write-CDTStep  { param($n,$t,$msg); Write-Host "[STEP $n/$t] $msg" -ForegroundColor Cyan }

function Show-Banner {
    Write-Host ""
    Write-Host "  CRM Consolidated Deployment Tool" -ForegroundColor Cyan
    Write-Host "  CDT v$CdtVersion" -ForegroundColor Cyan
    Write-Host ""
}

function Test-PythonVersion {
    Write-CDTStep 1 5 "Detecting Python 3.10+"
    $candidates = @("python3.12", "python3.11", "python3.10", "python3", "python")
    foreach ($candidate in $candidates) {
        try {
            $ver = & $candidate -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')" 2>$null
            if ($ver) {
                $parts = $ver.Trim().Split(".")
                if ([int]$parts[0] -ge 3 -and [int]$parts[1] -ge 10) {
                    Write-CDTInfo "Found Python $ver"
                    return $candidate
                }
            }
        } catch { }
    }
    $commonPaths = @(
        "C:\Python312\python.exe","C:\Python311\python.exe","C:\Python310\python.exe",
        "C:\Program Files\Python312\python.exe","C:\Program Files\Python311\python.exe"
    )
    foreach ($p in $commonPaths) {
        if (Test-Path $p) {
            Write-CDTInfo "Found Python at $p"
            return $p
        }
    }
    Write-CDTError "Python 3.10+ is required. Download from https://www.python.org/downloads/"
    exit 1
}

function Initialize-Venv {
    param($PythonCmd)
    Write-CDTStep 2 5 "Setting up virtual environment"
    if ($ResetVenv -and (Test-Path $VenvDir)) {
        Write-CDTWarn "Removing existing venv (--ResetVenv)"
        Remove-Item -Recurse -Force $VenvDir
    }
    if (-not (Test-Path "$VenvDir\Scripts\python.exe")) {
        Write-CDTInfo "Creating virtual environment..."
        & $PythonCmd -m venv $VenvDir
    } else {
        Write-CDTInfo "Reusing existing virtual environment"
    }
    $pip = "$VenvDir\Scripts\pip.exe"
    Write-CDTInfo "Installing dependencies..."
    & $pip install -q --upgrade pip
    & $pip install -q -r "$ScriptDir\requirements.txt"
    Write-CDTInfo "Dependencies installed"
}

function Get-CLITools {
    Write-CDTStep 3 5 "Checking CLI tools"
    New-Item -ItemType Directory -Force -Path $CdtBinDir | Out-Null
    New-Item -ItemType Directory -Force -Path $CdtSnapDir | Out-Null
    $kubectlPath = Join-Path $CdtBinDir "kubectl.exe"
    if (-not (Test-Path $kubectlPath)) {
        $kubectlUrl = "https://dl.k8s.io/release/v1.31.4/bin/windows/amd64/kubectl.exe"
        Write-CDTInfo "Downloading kubectl..."
        try {
            Invoke-WebRequest -Uri $kubectlUrl -OutFile $kubectlPath -UseBasicParsing -ErrorAction Stop
            Write-CDTInfo "kubectl downloaded"
        } catch {
            Write-CDTWarn "kubectl download failed: $_"
        }
    } else {
        Write-CDTInfo "kubectl already present"
    }
    $env:PATH = "$CdtBinDir;$env:PATH"
}

function Start-CDT {
    param($PythonCmd)
    Write-CDTStep 5 5 "Starting CDT server on port $Port"
    $pythonExe = "$VenvDir\Scripts\python.exe"
    $appPath = Join-Path $ScriptDir "gui\app.py"
    Write-CDTInfo "CDT wizard: http://localhost:$Port"
    Write-CDTInfo "Day-2 ops:  http://localhost:$Port/day2"
    Write-CDTInfo "Press Ctrl+C to stop"
    if (-not $NoBrowser -and -not $Headless) {
        Write-CDTStep 4 5 "Opening browser"
        Start-Sleep 2
        Start-Process "http://localhost:$Port"
    }
    & $pythonExe $appPath --port $Port
}

try {
    Show-Banner
    $pythonCmd = Test-PythonVersion
    Initialize-Venv $pythonCmd
    Get-CLITools
    Start-CDT $pythonCmd
} catch {
    Write-CDTError "CDT startup failed: $_"
    exit 1
}
