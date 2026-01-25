# Remote Operations Utility Script
# Common remote server operations for managing the deployed CRM application

param(
    [Parameter(Position=0)]
    [ValidateSet("logs", "restart", "status", "shell", "health", "restart-all", "pull", "clean", "help")]
    [string]$Operation = "help"
)

$ConfigFile = "$PSScriptRoot/.docker-remote-config.json"

function Load-Config {
    if (Test-Path $ConfigFile) {
        return Get-Content $ConfigFile | ConvertFrom-Json
    }
    Write-Host "Error: Configuration not found. Run '.\deploy-remote.ps1 configure' first." -ForegroundColor Red
    exit 1
}

function Write-Status {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] " -NoNewline -ForegroundColor Green
    Write-Host $Message
}

function Write-Info {
    param([string]$Message)
    Write-Host "  " -NoNewline -ForegroundColor Cyan
    Write-Host $Message
}

function Invoke-Remote {
    param([string]$Command)
    
    $Config = Load-Config
    $Host = $Config.Host
    $User = $Config.Username
    $Port = $Config.Port -or 22
    $DeployPath = "/opt/crm"
    
    ssh -p $Port "$User@$Host" "cd $DeployPath && $Command"
}

# Show logs
function Show-Logs {
    param([string]$Service = "")
    
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "                     Container Logs" -ForegroundColor Cyan
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    $Config = Load-Config
    $Host = $Config.Host
    $User = $Config.Username
    $Port = $Config.Port -or 22
    $DeployPath = "/opt/crm"
    
    if ([string]::IsNullOrEmpty($Service)) {
        Write-Status "Streaming all logs (Press Ctrl+C to exit)..."
        ssh -p $Port "$User@$Host" "cd $DeployPath && docker-compose logs -f"
    } else {
        Write-Status "Streaming logs for: $Service (Press Ctrl+C to exit)..."
        ssh -p $Port "$User@$Host" "cd $DeployPath && docker-compose logs -f $Service"
    }
}

# Restart containers
function Restart-Containers {
    param([string]$Service = "")
    
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "                  Restarting Containers" -ForegroundColor Yellow
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    
    if ([string]::IsNullOrEmpty($Service)) {
        Write-Status "Restarting all containers..."
        Invoke-Remote "docker-compose restart"
    } else {
        Write-Status "Restarting: $Service"
        Invoke-Remote "docker-compose restart $Service"
    }
    
    Write-Host ""
    Write-Status "Container status:"
    Invoke-Remote "docker-compose ps"
}

# Show status
function Show-Status {
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Blue
    Write-Host "               Deployment Status" -ForegroundColor Blue
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Blue
    Write-Host ""
    
    Write-Status "Container Status:"
    Invoke-Remote "docker-compose ps"
    
    Write-Host ""
    Write-Status "Resource Usage:"
    Invoke-Remote "docker stats --no-stream"
    
    Write-Host ""
    Write-Status "System Disk Space:"
    Invoke-Remote "df -h"
}

# Health check
function Health-Check {
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host "                 Health Check" -ForegroundColor Green
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Green
    Write-Host ""
    
    $Config = Load-Config
    $Host = $Config.Host
    
    Write-Status "API Health:"
    try {
        $response = Invoke-RestMethod -Uri "http://$Host:5000/health" -ErrorAction SilentlyContinue
        Write-Info "✓ API is responding"
        Write-Info "Status: $($response.status)"
    } catch {
        Write-Host "✗ API is not responding" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Status "Frontend Health:"
    try {
        $response = Invoke-RestMethod -Uri "http://$Host:3000" -ErrorAction SilentlyContinue -TimeoutSec 5
        Write-Info "✓ Frontend is responding"
    } catch {
        Write-Host "✗ Frontend is not responding" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Status "Container Status:"
    Invoke-Remote "docker-compose ps"
    
    Write-Host ""
    Write-Status "Container Health:"
    Invoke-Remote "docker-compose exec -T api curl http://localhost:5000/health 2>/dev/null || echo 'API container not responding'"
}

# SSH shell access
function SSH-Shell {
    $Config = Load-Config
    $Host = $Config.Host
    $User = $Config.Username
    $Port = $Config.Port -or 22
    
    Write-Host ""
    Write-Host "Connecting to $User@$Host..." -ForegroundColor Yellow
    Write-Host "Type 'exit' to disconnect" -ForegroundColor Yellow
    Write-Host ""
    
    ssh -p $Port "$User@$Host"
}

# Restart all services
function Restart-All {
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host "              Restarting All Services" -ForegroundColor Red
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Cancelled" -ForegroundColor Yellow
        return
    }
    
    Write-Status "Stopping all containers..."
    Invoke-Remote "docker-compose down"
    
    Write-Host ""
    Write-Status "Starting all containers..."
    Invoke-Remote "docker-compose up -d"
    
    Write-Host ""
    Write-Status "Waiting for containers to start..."
    Start-Sleep -Seconds 3
    
    Write-Status "Final Status:"
    Invoke-Remote "docker-compose ps"
}

# Pull latest images
function Pull-Images {
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Magenta
    Write-Host "              Pulling Latest Images" -ForegroundColor Magenta
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor Magenta
    Write-Host ""
    
    Write-Status "Pulling latest images from registry..."
    Invoke-Remote "docker-compose pull"
    
    Write-Host ""
    $confirm = Read-Host "Restart containers to apply updates? (yes/no)"
    if ($confirm -eq "yes") {
        Restart-All
    }
}

# Cleanup
function Cleanup {
    Write-Host ""
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor DarkRed
    Write-Host "                   Cleanup" -ForegroundColor DarkRed
    Write-Host "═════════════════════════════════════════════════════" -ForegroundColor DarkRed
    Write-Host ""
    
    Write-Host "This will remove:" -ForegroundColor Yellow
    Write-Host "  - Stopped containers"
    Write-Host "  - Unused networks"
    Write-Host "  - Unused images"
    Write-Host ""
    
    $confirm = Read-Host "Continue? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Cancelled" -ForegroundColor Yellow
        return
    }
    
    Write-Status "Running cleanup..."
    Invoke-Remote "docker system prune -f"
    
    Write-Status "Cleanup complete!"
}

# Show help
function Show-Help {
    Write-Host @"
Remote Operations Utility - Manage Deployed CRM Application

Usage: .\remote-ops.ps1 [Operation] [Service]

Operations:
  logs [service]        Show container logs (all or specific service)
                       Services: api, frontend, db
                       Example: .\remote-ops.ps1 logs api

  restart [service]    Restart containers (all or specific service)
                       Example: .\remote-ops.ps1 restart api

  status               Show deployment status and resource usage

  health               Run health checks on API and frontend

  shell                Open SSH shell to remote server

  restart-all          Restart all containers and services

  pull                 Pull latest images from registry

  clean                Clean up unused Docker resources

  help                 Show this help message

Examples:
  .\remote-ops.ps1 logs              # View all logs
  .\remote-ops.ps1 logs api          # View API logs
  .\remote-ops.ps1 status            # Check status
  .\remote-ops.ps1 health            # Health check
  .\remote-ops.ps1 restart api       # Restart API only
  .\remote-ops.ps1 shell             # SSH into remote server

Prerequisites:
  - Remote server must be configured (run: .\deploy-remote.ps1 configure)
  - SSH access must be available
  - Containers must be deployed

"@
}

# Main execution
try {
    switch ($Operation) {
        "logs" {
            Show-Logs
        }
        "restart" {
            Restart-Containers
        }
        "status" {
            Show-Status
        }
        "health" {
            Health-Check
        }
        "shell" {
            SSH-Shell
        }
        "restart-all" {
            Restart-All
        }
        "pull" {
            Pull-Images
        }
        "clean" {
            Cleanup
        }
        "help" {
            Show-Help
        }
        default {
            Write-Host "Unknown operation: $Operation" -ForegroundColor Red
            Show-Help
            exit 1
        }
    }
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}
