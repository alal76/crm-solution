# CRM Solution Build Script for Windows PowerShell
# Copyright (C) 2024-2026 Abhishek Lal - GNU AGPL v3

param(
    [ValidateSet("Development", "Testing", "Performance", "Production")]
    [string]$Environment = "Development",
    
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [switch]$SkipTests,
    [switch]$Clean,
    [switch]$Publish,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# Script paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BackendDir = Join-Path $ScriptDir "CRM.Backend"
$FrontendDir = Join-Path $ScriptDir "CRM.Frontend"
$BuildDir = Join-Path $ScriptDir "build-output"

# Colors
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Header = "Magenta"
}

function Write-Header {
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor $Colors.Header
    Write-Host "║           CRM Solution - Build System v1.3.1                 ║" -ForegroundColor $Colors.Header
    Write-Host "║           Environment: $Environment                            " -ForegroundColor $Colors.Header
    Write-Host "║           Configuration: $Configuration                       " -ForegroundColor $Colors.Header
    Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor $Colors.Header
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $Colors.Info
    Write-Host "▶ $Message" -ForegroundColor $Colors.Info
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $Colors.Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor $Colors.Success
}

function Write-WarningMsg {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor $Colors.Warning
}

function Write-ErrorMsg {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor $Colors.Error
}

function Show-Usage {
    Write-Host ""
    Write-Host "CRM Solution Build Script" -ForegroundColor $Colors.Header
    Write-Host "=========================" -ForegroundColor $Colors.Header
    Write-Host ""
    Write-Host "Usage: .\build.ps1 [options]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Cyan
    Write-Host "  -Environment    Development | Testing | Performance | Production"
    Write-Host "  -Configuration  Debug | Release"
    Write-Host "  -SkipTests      Skip running unit tests and BVTs"
    Write-Host "  -Clean          Clean all build artifacts first"
    Write-Host "  -Publish        Publish artifacts to build-output folder"
    Write-Host "  -Help           Show this help message"
    Write-Host ""
    Write-Host "Examples:" -ForegroundColor Cyan
    Write-Host "  .\build.ps1                                    # Dev + Debug + Tests"
    Write-Host "  .\build.ps1 -Environment Development -Clean    # Clean Dev build"
    Write-Host "  .\build.ps1 -Environment Testing               # Test deployment build"
    Write-Host "  .\build.ps1 -Environment Performance -Publish  # Perf build + publish"
    Write-Host "  .\build.ps1 -Environment Production -Publish   # Production build"
    Write-Host ""
}

function Test-Prerequisites {
    Write-Step "Checking Prerequisites"
    
    $missing = $false
    
    # Check .NET SDK
    try {
        $dotnetVersion = dotnet --version
        Write-Success "dotnet SDK: $dotnetVersion"
    }
    catch {
        Write-ErrorMsg "dotnet SDK not found"
        $missing = $true
    }
    
    # Check Node.js
    try {
        $nodeVersion = node --version
        Write-Success "Node.js: $nodeVersion"
    }
    catch {
        Write-ErrorMsg "Node.js not found"
        $missing = $true
    }
    
    # Check npm
    try {
        $npmVersion = npm --version
        Write-Success "npm: $npmVersion"
    }
    catch {
        Write-ErrorMsg "npm not found"
        $missing = $true
    }
    
    if ($missing) {
        throw "Missing prerequisites. Please install them and try again."
    }
}

function Invoke-Clean {
    Write-Step "Cleaning Previous Build Artifacts"
    
    # Clean .NET
    if (Test-Path $BackendDir) {
        dotnet clean "$BackendDir\CRM.sln" -v q 2>$null
        Get-ChildItem -Path $BackendDir -Include bin,obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "Backend cleaned"
    }
    
    # Clean Frontend
    $frontendBuild = Join-Path $FrontendDir "build"
    if (Test-Path $frontendBuild) {
        Remove-Item -Path $frontendBuild -Recurse -Force
        Write-Success "Frontend build cleaned"
    }
    
    # Clean output directory
    if (Test-Path $BuildDir) {
        Remove-Item -Path $BuildDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $BuildDir -Force | Out-Null
    Write-Success "Build output directory prepared"
}

function Restore-Dependencies {
    Write-Step "Restoring Dependencies"
    
    # .NET restore
    Write-Host "Restoring .NET packages..." -ForegroundColor $Colors.Info
    dotnet restore "$BackendDir\CRM.sln" --verbosity minimal
    Write-Success ".NET packages restored"
    
    # npm install
    Write-Host "Installing npm packages..." -ForegroundColor $Colors.Info
    Push-Location $FrontendDir
    try {
        npm ci --silent 2>$null
    }
    catch {
        npm install --silent
    }
    Pop-Location
    Write-Success "npm packages installed"
}

function Build-Backend {
    Write-Step "Building Backend ($Configuration)"
    
    $buildArgs = @("-c", $Configuration, "--no-restore")
    
    # Add environment-specific flags
    switch ($Environment) {
        "Development" {
            $buildArgs += @("-p:DebugType=full", "-p:DebugSymbols=true")
        }
        "Testing" {
            $buildArgs += @("-p:DebugType=portable")
        }
        "Performance" {
            $buildArgs += @("-p:Optimize=true")
        }
        "Production" {
            $buildArgs += @("-p:Optimize=true", "-p:DebugType=none", "-p:DebugSymbols=false")
        }
    }
    
    Write-Host "Build arguments: $($buildArgs -join ' ')" -ForegroundColor $Colors.Info
    
    dotnet build "$BackendDir\CRM.sln" @buildArgs
    if ($LASTEXITCODE -ne 0) { throw "Backend build failed" }
    
    Write-Success "Backend build completed"
}

function Build-Frontend {
    Write-Step "Building Frontend ($Environment)"
    
    Push-Location $FrontendDir
    
    # Set environment variables
    $env:REACT_APP_ENV = $Environment
    $env:GENERATE_SOURCEMAP = if ($Environment -eq "Production") { "false" } else { "true" }
    
    if ($Environment -eq "Production") {
        Write-Host "Building optimized production bundle..." -ForegroundColor $Colors.Info
    }
    else {
        Write-Host "Building with source maps..." -ForegroundColor $Colors.Info
    }
    
    npm run build
    if ($LASTEXITCODE -ne 0) { throw "Frontend build failed" }
    
    Pop-Location
    Write-Success "Frontend build completed"
}

function Invoke-UnitTests {
    if ($SkipTests) {
        Write-WarningMsg "Skipping tests (-SkipTests flag)"
        return
    }
    
    Write-Step "Running Unit Tests"
    
    $testArgs = @("--no-build", "-c", $Configuration)
    
    switch ($Environment) {
        { $_ -in "Development", "Testing" } {
            $testArgs += @("-v", "normal", "--logger", "console;verbosity=detailed")
        }
        default {
            $testArgs += @("-v", "minimal", "--logger", "console;verbosity=minimal")
        }
    }
    
    $resultsDir = Join-Path $BuildDir "test-results"
    $testArgs += @("--results-directory", $resultsDir)
    
    Write-Host "Running tests with: dotnet test $($testArgs -join ' ')" -ForegroundColor $Colors.Info
    
    dotnet test "$BackendDir\tests\CRM.Tests.csproj" @testArgs
    if ($LASTEXITCODE -ne 0) { throw "Unit tests failed" }
    
    Write-Success "Unit tests passed"
}

function Invoke-BVTTests {
    if ($SkipTests) { return }
    
    Write-Step "Running Build Verification Tests (BVT)"
    
    $resultsDir = Join-Path $BuildDir "bvt-results"
    
    dotnet test "$BackendDir\tests\CRM.Tests.csproj" `
        --no-build `
        -c $Configuration `
        --filter "FullyQualifiedName~BVT" `
        -v normal `
        --results-directory $resultsDir
    
    if ($LASTEXITCODE -ne 0) { throw "BVT tests failed" }
    
    Write-Success "BVT tests passed"
}

function Publish-Artifacts {
    if (-not $Publish -and $Environment -notin @("Production", "Performance")) { return }
    
    Write-Step "Publishing Build Artifacts"
    
    $backendOutput = Join-Path $BuildDir "backend"
    $publishArgs = @("-c", $Configuration, "--no-build", "-o", $backendOutput)
    
    if ($Environment -eq "Production") {
        $publishArgs += @("--self-contained", "false")
    }
    
    dotnet publish "$BackendDir\src\CRM.Api\CRM.Api.csproj" @publishArgs
    
    # Copy frontend build
    $frontendBuild = Join-Path $FrontendDir "build"
    if (Test-Path $frontendBuild) {
        $frontendOutput = Join-Path $BuildDir "frontend"
        Copy-Item -Path $frontendBuild -Destination $frontendOutput -Recurse -Force
        Write-Success "Frontend artifacts copied"
    }
    
    # Copy configuration
    $configFile = Join-Path $BackendDir "src\CRM.Api\appsettings.$Environment.json"
    if (Test-Path $configFile) {
        Copy-Item -Path $configFile -Destination $backendOutput -Force
    }
    
    Write-Success "Artifacts published to: $BuildDir"
}

function New-BuildReport {
    Write-Step "Generating Build Report"
    
    $reportFile = Join-Path $BuildDir "build-report.txt"
    
    $report = @"
═══════════════════════════════════════════════════════════════
              CRM Solution Build Report
═══════════════════════════════════════════════════════════════

Build Date:      $(Get-Date -Format "yyyy-MM-dd HH:mm:ss K")
Environment:     $Environment
Configuration:   $Configuration
Tests Run:       $(-not $SkipTests)

───────────────────────────────────────────────────────────────
                        Versions
───────────────────────────────────────────────────────────────
.NET SDK:        $(dotnet --version)
Node.js:         $(node --version)
npm:             $(npm --version)

───────────────────────────────────────────────────────────────
                      Artifact Sizes
───────────────────────────────────────────────────────────────
"@

    $backendPath = Join-Path $BuildDir "backend"
    if (Test-Path $backendPath) {
        $size = (Get-ChildItem -Path $backendPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        $report += "Backend:         $([math]::Round($size, 2)) MB`n"
    }
    
    $frontendPath = Join-Path $BuildDir "frontend"
    if (Test-Path $frontendPath) {
        $size = (Get-ChildItem -Path $frontendPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
        $report += "Frontend:        $([math]::Round($size, 2)) MB`n"
    }
    
    $report += "`n═══════════════════════════════════════════════════════════════"
    
    $report | Out-File -FilePath $reportFile -Encoding UTF8
    
    Write-Host $report
    Write-Success "Build report saved to: $reportFile"
}

# Main execution
function Main {
    if ($Help) {
        Show-Usage
        return
    }
    
    Write-Header
    
    $startTime = Get-Date
    
    try {
        Test-Prerequisites
        
        if ($Clean) {
            Invoke-Clean
        }
        
        Restore-Dependencies
        Build-Backend
        Build-Frontend
        Invoke-UnitTests
        Invoke-BVTTests
        
        if ($Publish -or $Environment -in @("Production", "Performance")) {
            Invoke-Clean
            Restore-Dependencies
            Build-Backend
            Build-Frontend
            Publish-Artifacts
        }
        
        New-BuildReport
        
        $duration = (Get-Date) - $startTime
        
        Write-Host ""
        Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor $Colors.Success
        Write-Host "║                    BUILD SUCCESSFUL                          ║" -ForegroundColor $Colors.Success
        Write-Host "║               Total Time: $([math]::Round($duration.TotalSeconds))s                                ║" -ForegroundColor $Colors.Success
        Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor $Colors.Success
    }
    catch {
        Write-Host ""
        Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor $Colors.Error
        Write-Host "║                     BUILD FAILED                             ║" -ForegroundColor $Colors.Error
        Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor $Colors.Error
        Write-Host ""
        Write-Host "Error: $_" -ForegroundColor $Colors.Error
        exit 1
    }
}

Main
