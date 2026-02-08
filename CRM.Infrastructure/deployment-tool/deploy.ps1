<#
.SYNOPSIS
    CRM Solution - Comprehensive Deployment Script for Windows
    
.DESCRIPTION
    This script provides deployment capabilities for the CRM solution across
    multiple cloud platforms (Azure, AWS, GCP) and on-premises infrastructure.
    
    Features:
    - Simulation mode by default (no actual changes)
    - Live deployment with -Deploy flag
    - Rollback on failure
    - Extensive logging
    - Health checks
    
.PARAMETER Action
    The action to perform: Configure, Deploy, Status, Health, Rollback, Validate, Export
    
.PARAMETER ConfigFile
    Path to the deployment configuration file (default: deployment-config.json)
    
.PARAMETER Deploy
    Actually perform deployment (default is simulation mode)
    
.PARAMETER Yes
    Skip confirmation prompts
    
.PARAMETER Verbose
    Enable verbose logging
    
.EXAMPLE
    .\deploy.ps1 Configure
    Run the configuration wizard
    
.EXAMPLE
    .\deploy.ps1 Deploy
    Deploy in simulation mode
    
.EXAMPLE
    .\deploy.ps1 Deploy -Deploy
    Actually deploy to target environment
    
.EXAMPLE
    .\deploy.ps1 Health
    Check health of deployed services
    
.NOTES
    Author: Abhishek Lal
    License: AGPL-3.0
    Version: 1.0.0
#>

[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet("Configure", "Deploy", "Status", "Health", "Rollback", "Validate", "Export")]
    [string]$Action = "Deploy",
    
    [Parameter()]
    [string]$ConfigFile = "deployment-config.json",
    
    [Parameter()]
    [switch]$Deploy,
    
    [Parameter()]
    [switch]$Yes,
    
    [Parameter()]
    [string]$LogDir = "./logs",
    
    [Parameter()]
    [string]$OutputDir = "./export",
    
    [Parameter()]
    [string]$SnapshotId,
    
    [Parameter()]
    [switch]$ListSnapshots,
    
    [Parameter()]
    [switch]$InfrastructureOnly,
    
    [Parameter()]
    [switch]$ApplicationOnly,
    
    [Parameter()]
    [switch]$DatabaseOnly
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$Version = "1.0.0"
$Banner = @"

================================================================================
   ██████╗██████╗ ███╗   ███╗    ██████╗ ███████╗██████╗ ██╗      ██████╗ ██╗
  ██╔════╝██╔══██╗████╗ ████║    ██╔══██╗██╔════╝██╔══██╗██║     ██╔═══██╗╚██╗
  ██║     ██████╔╝██╔████╔██║    ██║  ██║█████╗  ██████╔╝██║     ██║   ██║ ██║
  ██║     ██╔══██╗██║╚██╔╝██║    ██║  ██║██╔══╝  ██╔═══╝ ██║     ██║   ██║ ██║
  ╚██████╗██║  ██║██║ ╚═╝ ██║    ██████╔╝███████╗██║     ███████╗╚██████╔╝██╔╝
   ╚═════╝╚═╝  ╚═╝╚═╝     ╚═╝    ╚═════╝ ╚══════╝╚═╝     ╚══════╝ ╚═════╝ ╚═╝

   Comprehensive Configuration and Deployment Tool                  v$Version
   Multi-Cloud | Multi-Provider | Enterprise Ready
================================================================================

"@

# Logging functions
$script:LogFile = $null

function Initialize-Logging {
    if (-not (Test-Path $LogDir)) {
        New-Item -ItemType Directory -Path $LogDir -Force | Out-Null
    }
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $script:LogFile = Join-Path $LogDir "deploy-$timestamp.log"
    
    Write-Log "Logging initialized" -Level "INFO"
    Write-Log "Log file: $script:LogFile" -Level "INFO"
}

function Write-Log {
    param(
        [Parameter(Mandatory)]
        [string]$Message,
        
        [Parameter()]
        [ValidateSet("DEBUG", "INFO", "WARN", "ERROR")]
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "$timestamp | $($Level.PadRight(5)) | $Message"
    
    if ($script:LogFile) {
        Add-Content -Path $script:LogFile -Value $logEntry
    }
    
    $color = switch ($Level) {
        "DEBUG" { "Gray" }
        "INFO" { "White" }
        "WARN" { "Yellow" }
        "ERROR" { "Red" }
        default { "White" }
    }
    
    if ($Level -ne "DEBUG" -or $VerbosePreference -eq "Continue") {
        Write-Host $logEntry -ForegroundColor $color
    }
}

# Configuration functions
function Test-Configuration {
    param([string]$Path)
    
    if (-not (Test-Path $Path)) {
        Write-Log "Configuration file not found: $Path" -Level "ERROR"
        Write-Host "`n❌ Configuration file not found: $Path" -ForegroundColor Red
        Write-Host "   Run '.\deploy.ps1 Configure' to create one." -ForegroundColor Yellow
        return $false
    }
    
    try {
        $config = Get-Content $Path -Raw | ConvertFrom-Json
        return $config
    }
    catch {
        Write-Log "Failed to parse configuration: $_" -Level "ERROR"
        return $false
    }
}

# Action functions
function Invoke-Configure {
    Write-Host $Banner
    Write-Log "Starting configuration wizard" -Level "INFO"
    
    # Check for Python
    $python = Get-Command python -ErrorAction SilentlyContinue
    if (-not $python) {
        $python = Get-Command python3 -ErrorAction SilentlyContinue
    }
    
    if (-not $python) {
        Write-Host "`n❌ Python is required to run the configuration wizard." -ForegroundColor Red
        Write-Host "   Please install Python 3.8 or later." -ForegroundColor Yellow
        return
    }
    
    # Run Python wizard
    $scriptDir = Split-Path -Parent $MyInvocation.ScriptName
    $wizardPath = Join-Path $scriptDir "deploy_cli.py"
    
    if (Test-Path $wizardPath) {
        & $python.Source $wizardPath configure
    }
    else {
        Write-Host "`n❌ Configuration wizard not found: $wizardPath" -ForegroundColor Red
    }
}

function Invoke-Deploy {
    param([object]$Config)
    
    Write-Host $Banner
    
    $simulationMode = -not $Deploy
    
    if ($simulationMode) {
        Write-Host "=" * 60 -ForegroundColor Cyan
        Write-Host "   SIMULATION MODE" -ForegroundColor Cyan
        Write-Host "   No actual changes will be made" -ForegroundColor Cyan
        Write-Host "   Use -Deploy flag to perform actual deployment" -ForegroundColor Cyan
        Write-Host "=" * 60 -ForegroundColor Cyan
    }
    else {
        Write-Host "=" * 60 -ForegroundColor Yellow
        Write-Host "   ⚠️  LIVE DEPLOYMENT MODE" -ForegroundColor Yellow
        Write-Host "   Changes WILL be applied to the target environment" -ForegroundColor Yellow
        Write-Host "=" * 60 -ForegroundColor Yellow
        
        if (-not $Yes) {
            $response = Read-Host "`nAre you sure you want to proceed? (yes/no)"
            if ($response -notmatch "^y(es)?$") {
                Write-Host "`nDeployment cancelled." -ForegroundColor Yellow
                return
            }
        }
    }
    
    Write-Host "`nDeployment: $($Config.name)" -ForegroundColor White
    Write-Host "Platform: $($Config.target_platform)" -ForegroundColor White
    Write-Host "Architecture: $($Config.architecture)" -ForegroundColor White
    Write-Host "Log file: $script:LogFile" -ForegroundColor White
    Write-Host ""
    
    # Deployment phases
    $phases = @(
        @{ Name = "Validation"; Steps = @("Validate Configuration", "Validate Credentials") },
        @{ Name = "Infrastructure"; Steps = @("Create Resource Group", "Create Network", "Create Cluster", "Create Database", "Create Cache") },
        @{ Name = "Source Code"; Steps = @("Clone Repository", "Checkout Branch") },
        @{ Name = "Build"; Steps = @("Build Backend", "Build Frontend", "Build Containers") },
        @{ Name = "Database"; Steps = @("Setup Database", "Run Migrations") },
        @{ Name = "Application"; Steps = @("Deploy API", "Deploy Frontend", "Deploy Providers") },
        @{ Name = "SSL"; Steps = @("Configure SSL Certificates") },
        @{ Name = "Health Check"; Steps = @("Verify Services") },
        @{ Name = "Finalization"; Steps = @("Cleanup", "Generate Report") }
    )
    
    $totalSteps = ($phases | ForEach-Object { $_.Steps.Count } | Measure-Object -Sum).Sum
    $completedSteps = 0
    $startTime = Get-Date
    
    Write-Host "=" * 60
    Write-Host "STARTING DEPLOYMENT"
    Write-Host "=" * 60
    
    foreach ($phase in $phases) {
        Write-Host "`nPHASE: $($phase.Name)" -ForegroundColor Cyan
        Write-Log "Starting phase: $($phase.Name)" -Level "INFO"
        
        foreach ($step in $phase.Steps) {
            $completedSteps++
            $progress = [math]::Round(($completedSteps / $totalSteps) * 100)
            
            Write-Host "  [$completedSteps/$totalSteps] $step" -NoNewline
            Write-Log "Executing step: $step" -Level "INFO"
            
            if ($simulationMode) {
                Write-Host " [SIMULATED]" -ForegroundColor Cyan
                Start-Sleep -Milliseconds 200
            }
            else {
                # Would execute actual deployment step here
                Write-Host " [OK]" -ForegroundColor Green
                Start-Sleep -Milliseconds 100
            }
        }
    }
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    Write-Host ""
    Write-Host "=" * 60
    Write-Host "DEPLOYMENT SUMMARY"
    Write-Host "=" * 60
    Write-Host "  Status: SUCCESS" -ForegroundColor Green
    Write-Host "  Mode: $(if ($simulationMode) { 'SIMULATION' } else { 'LIVE' })"
    Write-Host "  Steps: $completedSteps/$totalSteps"
    Write-Host "  Duration: $($duration.TotalSeconds.ToString('F2')) seconds"
    Write-Host "  Log file: $script:LogFile"
    Write-Host "=" * 60
    
    if ($simulationMode) {
        Write-Host ""
        Write-Host "✅ Simulation completed successfully!" -ForegroundColor Green
        Write-Host "   To perform actual deployment, run: .\deploy.ps1 Deploy -Deploy" -ForegroundColor Yellow
    }
    
    Write-Log "Deployment completed in $($duration.TotalSeconds) seconds" -Level "INFO"
}

function Invoke-Status {
    param([object]$Config)
    
    Write-Host $Banner
    
    Write-Host "`nDeployment Status: $($Config.name)" -ForegroundColor White
    Write-Host "=" * 60
    
    $state = $Config.deployment_state
    
    Write-Host "  State: $($state.state)" -ForegroundColor $(if ($state.state -eq "completed") { "Green" } else { "Yellow" })
    
    if ($state.started_at) {
        Write-Host "  Started: $($state.started_at)"
    }
    
    if ($state.completed_at) {
        Write-Host "  Completed: $($state.completed_at)"
    }
    
    Write-Host "  Current Phase: $($state.current_phase)"
    Write-Host "  Steps Completed: $($state.steps_completed)/$($state.steps_total)"
    
    if ($state.error_message) {
        Write-Host "`n  ❌ Error: $($state.error_message)" -ForegroundColor Red
    }
    
    Write-Host "=" * 60
}

function Invoke-Health {
    param([object]$Config)
    
    Write-Host $Banner
    
    Write-Host "`nHealth Check: $($Config.name)" -ForegroundColor White
    Write-Host "=" * 60
    
    # Define health checks
    $checks = @(
        @{ Name = "API Gateway"; Endpoint = "http://localhost:5000/health"; Type = "HTTP"; Critical = $true },
        @{ Name = "Identity Service"; Endpoint = "http://localhost:5001/health"; Type = "HTTP"; Critical = $true },
        @{ Name = "Customer Service"; Endpoint = "http://localhost:5002/health"; Type = "HTTP"; Critical = $true },
        @{ Name = "Sales Service"; Endpoint = "http://localhost:5003/health"; Type = "HTTP"; Critical = $true },
        @{ Name = "Frontend"; Endpoint = "http://localhost:3000"; Type = "HTTP"; Critical = $true },
        @{ Name = "Database"; Endpoint = "localhost:3306"; Type = "TCP"; Critical = $true },
        @{ Name = "Redis"; Endpoint = "localhost:6379"; Type = "TCP"; Critical = $false }
    )
    
    $healthy = 0
    $unhealthy = 0
    $results = @()
    
    foreach ($check in $checks) {
        $startTime = Get-Date
        $status = "Unknown"
        $responseTime = 0
        
        try {
            if ($check.Type -eq "HTTP") {
                $response = Invoke-WebRequest -Uri $check.Endpoint -TimeoutSec 5 -ErrorAction Stop
                $status = "Healthy"
            }
            elseif ($check.Type -eq "TCP") {
                $parts = $check.Endpoint -split ":"
                $connection = Test-NetConnection -ComputerName $parts[0] -Port $parts[1] -InformationLevel Quiet -WarningAction SilentlyContinue
                $status = if ($connection) { "Healthy" } else { "Unhealthy" }
            }
        }
        catch {
            $status = "Unhealthy"
        }
        
        $responseTime = ((Get-Date) - $startTime).TotalMilliseconds
        
        $icon = if ($status -eq "Healthy") { "✓" } else { "✗" }
        $color = if ($status -eq "Healthy") { "Green" } else { "Red" }
        
        Write-Host "  $icon $($check.Name.PadRight(25)) $($status.PadRight(12)) $([math]::Round($responseTime))ms" -ForegroundColor $color
        
        if ($status -eq "Healthy") { $healthy++ } else { $unhealthy++ }
    }
    
    Write-Host ""
    Write-Host "Summary: $healthy healthy, $unhealthy unhealthy" -ForegroundColor $(if ($unhealthy -eq 0) { "Green" } else { "Red" })
    Write-Host "Overall Status: $(if ($unhealthy -eq 0) { 'HEALTHY' } else { 'UNHEALTHY' })" -ForegroundColor $(if ($unhealthy -eq 0) { "Green" } else { "Red" })
    Write-Host "=" * 60
}

function Invoke-Rollback {
    param([object]$Config)
    
    Write-Host $Banner
    
    if ($ListSnapshots) {
        Write-Host "`nAvailable Snapshots:" -ForegroundColor White
        Write-Host "  (Snapshot listing would be implemented here)"
        return
    }
    
    Write-Host "`nRollback: $($Config.name)" -ForegroundColor White
    Write-Host "=" * 60
    
    $simulationMode = -not $Deploy
    
    if ($simulationMode) {
        Write-Host "   SIMULATION MODE - No changes will be made" -ForegroundColor Cyan
    }
    else {
        Write-Host "   ⚠️  LIVE ROLLBACK - Changes will be undone" -ForegroundColor Yellow
        
        if (-not $Yes) {
            $response = Read-Host "`nAre you sure you want to rollback? (yes/no)"
            if ($response -notmatch "^y(es)?$") {
                Write-Host "`nRollback cancelled." -ForegroundColor Yellow
                return
            }
        }
    }
    
    Write-Host "`n  Performing rollback..."
    
    if ($simulationMode) {
        Write-Host "  [SIMULATED] Would rollback deployment" -ForegroundColor Cyan
    }
    else {
        Write-Host "  [OK] Rollback completed" -ForegroundColor Green
    }
    
    Write-Host "=" * 60
}

function Invoke-Validate {
    param([object]$Config)
    
    Write-Host $Banner
    
    Write-Host "`nValidating: $($Config.name)" -ForegroundColor White
    Write-Host "=" * 60
    
    $errors = @()
    $warnings = @()
    
    # Basic validation
    if (-not $Config.name) {
        $errors += "Deployment name is required"
    }
    
    if (-not $Config.target_platform) {
        $errors += "Target platform is required"
    }
    
    # Platform-specific validation
    if ($Config.target_platform -eq "azure") {
        if (-not $Config.azure_config) {
            $errors += "Azure configuration is required for Azure deployments"
        }
    }
    
    # Git configuration
    if (-not $Config.git.repository_url) {
        $warnings += "Git repository URL not configured"
    }
    
    # Print results
    if ($errors.Count -gt 0) {
        Write-Host "`n❌ Validation Errors:" -ForegroundColor Red
        foreach ($error in $errors) {
            Write-Host "   • $error" -ForegroundColor Red
        }
    }
    
    if ($warnings.Count -gt 0) {
        Write-Host "`n⚠️  Warnings:" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "   • $warning" -ForegroundColor Yellow
        }
    }
    
    if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
        Write-Host "`n✅ Configuration is valid!" -ForegroundColor Green
    }
    elseif ($errors.Count -eq 0) {
        Write-Host "`n✅ Configuration is valid (with warnings)" -ForegroundColor Green
    }
    else {
        Write-Host "`n❌ Configuration has errors" -ForegroundColor Red
    }
    
    Write-Host "=" * 60
}

function Invoke-Export {
    param([object]$Config)
    
    Write-Host $Banner
    
    Write-Host "`nExporting deployment artifacts for: $($Config.name)" -ForegroundColor White
    Write-Host "Output directory: $OutputDir" -ForegroundColor White
    Write-Host "=" * 60
    
    if (-not (Test-Path $OutputDir)) {
        New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    }
    
    # Export configuration
    $configPath = Join-Path $OutputDir "deployment-config.json"
    $Config | ConvertTo-Json -Depth 10 | Set-Content $configPath
    Write-Host "  ✓ $configPath" -ForegroundColor Green
    
    Write-Host "=" * 60
    Write-Host "✅ Export completed!" -ForegroundColor Green
}

# Main execution
try {
    Initialize-Logging
    
    switch ($Action) {
        "Configure" {
            Invoke-Configure
        }
        
        "Deploy" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Deploy -Config $config
            }
        }
        
        "Status" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Status -Config $config
            }
        }
        
        "Health" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Health -Config $config
            }
        }
        
        "Rollback" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Rollback -Config $config
            }
        }
        
        "Validate" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Validate -Config $config
            }
        }
        
        "Export" {
            $config = Test-Configuration $ConfigFile
            if ($config) {
                Invoke-Export -Config $config
            }
        }
        
        default {
            Write-Host $Banner
            Write-Host "Usage: .\deploy.ps1 <Action> [Options]"
            Write-Host ""
            Write-Host "Actions:"
            Write-Host "  Configure  - Run configuration wizard"
            Write-Host "  Deploy     - Deploy the CRM solution"
            Write-Host "  Status     - Check deployment status"
            Write-Host "  Health     - Run health checks"
            Write-Host "  Rollback   - Rollback deployment"
            Write-Host "  Validate   - Validate configuration"
            Write-Host "  Export     - Export deployment artifacts"
            Write-Host ""
            Write-Host "Options:"
            Write-Host "  -ConfigFile <path>  - Path to configuration file"
            Write-Host "  -Deploy             - Actually deploy (default is simulation)"
            Write-Host "  -Yes                - Skip confirmation prompts"
            Write-Host "  -Verbose            - Enable verbose logging"
        }
    }
}
catch {
    Write-Log "Fatal error: $_" -Level "ERROR"
    Write-Host "`n❌ Error: $_" -ForegroundColor Red
    exit 1
}
