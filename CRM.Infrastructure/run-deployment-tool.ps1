# Launch CRM Solution Deployment Tool (PowerShell)
# Version: 0.0.24

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Starting CRM Solution Deployment Tool..."
Write-Host "========================================="

# Check Python
$pythonCmd = $null

if (Get-Command python3 -ErrorAction SilentlyContinue) {
    $pythonCmd = "python3"
} elseif (Get-Command python -ErrorAction SilentlyContinue) {
    $pythonCmd = "python"
} else {
    Write-Error "ERROR: Python 3 is required but not found."
    Write-Host "Please install Python 3.9+ from https://python.org"
    exit 1
}

# Check version
$pyVersion = & $pythonCmd -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')"
Write-Host "Found Python $pyVersion"

# Check tkinter
try {
    & $pythonCmd -c "import tkinter" 2>$null
} catch {
    Write-Error "ERROR: tkinter is not available."
    Write-Host "Please reinstall Python with tkinter support enabled."
    exit 1
}

# Run the tool
Set-Location "$ScriptDir\deployment-tool"
& $pythonCmd main.py
