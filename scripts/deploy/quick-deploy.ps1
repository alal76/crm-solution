# Quick Deploy Script - Builds, Pushes, and Deploys to Remote Docker Server
# This script automates the entire deployment pipeline

param(
    [Parameter(Position=0)]
    [ValidateSet("build", "push", "deploy", "full", "logs", "stop")]
    [string]$Action = "full"
)

# Load configuration
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

function Write-Error-Custom {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] ERROR: " -NoNewline -ForegroundColor Red
    Write-Host $Message
}

function Write-Info {
    param([string]$Message)
    Write-Host "  " -NoNewline -ForegroundColor Cyan
    Write-Host $Message
}

# Build Docker Images
function Build-Images {
    param($Config)
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                   Building Docker Images                   ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    
    $Registry = $Config.Registry.URL
    $ImageTag = $Config.ImageTag -or "latest"
    
    Write-Status "Building backend image: $Registry/crm-api:$ImageTag"
    docker build -f Dockerfile.backend -t "$Registry/crm-api:$ImageTag" .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Backend build failed"
        exit 1
    }
    
    Write-Host ""
    Write-Status "Building frontend image: $Registry/crm-frontend:$ImageTag"
    docker build -f Dockerfile.frontend -t "$Registry/crm-frontend:$ImageTag" .
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Frontend build failed"
        exit 1
    }
    
    Write-Host ""
    Write-Status "Build complete!"
    Write-Info "Backend: $Registry/crm-api:$ImageTag"
    Write-Info "Frontend: $Registry/crm-frontend:$ImageTag"
}

# Push Images to Registry
function Push-Images {
    param($Config)
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║              Pushing Images to Registry                    ║" -ForegroundColor Magenta
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
    Write-Host ""
    
    $Registry = $Config.Registry.URL
    $ImageTag = $Config.ImageTag -or "latest"
    
    # Check if logged in
    Write-Status "Checking Docker registry authentication..."
    $LoggedIn = docker info 2>&1 | Select-String "Username"
    
    if (-not $LoggedIn) {
        Write-Host ""
        Write-Host "Docker login required for: $Registry" -ForegroundColor Yellow
        Write-Host "You'll be prompted for credentials..." -ForegroundColor Yellow
        Write-Host ""
        
        docker login $Registry
        
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Docker login failed"
            exit 1
        }
    }
    
    Write-Status "Pushing backend image: $Registry/crm-api:$ImageTag"
    docker push "$Registry/crm-api:$ImageTag"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Failed to push backend image"
        exit 1
    }
    
    Write-Host ""
    Write-Status "Pushing frontend image: $Registry/crm-frontend:$ImageTag"
    docker push "$Registry/crm-frontend:$ImageTag"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Failed to push frontend image"
        exit 1
    }
    
    Write-Host ""
    Write-Status "Push complete!"
}

# Deploy to Remote Server
function Deploy-Remote {
    param($Config)
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
    Write-Host "║            Deploying to Remote Docker Server              ║" -ForegroundColor Yellow
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
    Write-Host ""
    
    $RemoteHost = $Config.Host
    $RemoteUser = $Config.Username
    $RemotePort = $Config.Port -or 22
    $DeployPath = "/opt/crm"
    
    Write-Info "Remote Host: $RemoteHost"
    Write-Info "Remote User: $RemoteUser"
    Write-Info "Deploy Path: $DeployPath"
    Write-Host ""
    
    # Test SSH connection
    Write-Status "Testing SSH connection..."
    ssh -p $RemotePort "${RemoteUser}@${RemoteHost}" "echo 'SSH connection successful'" 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "SSH connection failed"
        exit 1
    }
    
    # Copy docker-compose.yml to remote
    Write-Host ""
    Write-Status "Copying docker-compose.yml to remote server..."
    scp -P $RemotePort docker-compose.yml "${RemoteUser}@${RemoteHost}:${DeployPath}/"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Failed to copy docker-compose.yml"
        exit 1
    }
    
    # Deploy
    Write-Host ""
    Write-Status "Deploying containers on remote server..."
    Write-Host ""
    
    ssh -p $RemotePort "${RemoteUser}@${RemoteHost}" @"
cd $DeployPath
echo '=== Docker Login ==='
docker login $($Config.Registry.URL)

echo ''
echo '=== Pulling Images ==='
docker-compose pull

echo ''
echo '=== Starting Containers ==='
docker-compose up -d

echo ''
echo '=== Container Status ==='
docker-compose ps
"@
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Custom "Remote deployment failed"
        exit 1
    }
    
    Write-Host ""
    Write-Status "Deployment complete!"
    Write-Host ""
    Write-Info "API will be available at: http://$RemoteHost:5000"
    Write-Info "Frontend will be available at: http://$RemoteHost:3000"
    Write-Host ""
    Write-Status "To view logs:"
    Write-Host "  ssh -p $RemotePort ${RemoteUser}@${RemoteHost} 'cd $DeployPath && docker-compose logs -f'" -ForegroundColor Cyan
}

# View Logs
function View-Logs {
    param($Config)
    
    $RemoteHost = $Config.Host
    $RemoteUser = $Config.Username
    $RemotePort = $Config.Port -or 22
    $DeployPath = "/opt/crm"
    
    Write-Host ""
    Write-Status "Tailing logs from remote server (Press Ctrl+C to exit)..."
    Write-Host ""
    
    ssh -p $RemotePort "$RemoteUser@$RemoteHost" "cd $DeployPath && docker-compose logs -f"
}

# Stop Deployment
function Stop-Deployment {
    param($Config)
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║              Stopping Remote Deployment                   ║" -ForegroundColor Red
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    
    $RemoteHost = $Config.Host
    $RemoteUser = $Config.Username
    $RemotePort = $Config.Port -or 22
    $DeployPath = "/opt/crm"
    
    Write-Warning "This will stop all containers!"
    $confirm = Read-Host "Are you sure? (yes/no)"
    
    if ($confirm -ne "yes") {
        Write-Host "Cancelled" -ForegroundColor Yellow
        return
    }
    
    Write-Status "Stopping containers..."
    ssh -p $RemotePort "$RemoteUser@$RemoteHost" "cd $DeployPath && docker-compose down"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Containers stopped"
    } else {
        Write-Error-Custom "Failed to stop containers"
        exit 1
    }
}

# Show Help
function Show-Help {
    Write-Host @"
Quick Deploy - Build, Push, and Deploy to Remote Docker Server

Usage: .\quick-deploy.ps1 [Action]

Actions:
  build       Build Docker images locally
  push        Push images to registry
  deploy      Deploy to remote server (assumes images are pushed)
  full        Build → Push → Deploy (complete pipeline)
  logs        View logs from remote server
  stop        Stop remote deployment

Examples:
  .\quick-deploy.ps1 build
  .\quick-deploy.ps1 push
  .\quick-deploy.ps1 deploy
  .\quick-deploy.ps1 full
  .\quick-deploy.ps1 logs
  .\quick-deploy.ps1 stop

Prerequisites:
  1. Docker installed and running
  2. Remote server configuration (run: .\deploy-remote.ps1 configure)
  3. SSH access to remote server
  4. Docker registry credentials

Configuration file: .docker-remote-config.json

"@
}

# Main
try {
    $Config = Load-Config
    
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║          CRM Application - Quick Deploy Script             ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Info "Remote Host: $($Config.Host)"
    Write-Info "Registry: $($Config.Registry.URL)"
    Write-Info "Image Tag: $($Config.ImageTag -or 'latest')"
    Write-Info "Environment: $($Config.Environment -or 'production')"
    
    switch ($Action) {
        "build" {
            Build-Images $Config
        }
        "push" {
            Push-Images $Config
        }
        "deploy" {
            Deploy-Remote $Config
        }
        "full" {
            Write-Status "Starting full deployment pipeline..."
            Build-Images $Config
            Push-Images $Config
            Deploy-Remote $Config
            
            Write-Host ""
            Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
            Write-Host "║               Full Deployment Complete! 🎉                ║" -ForegroundColor Green
            Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
        }
        "logs" {
            View-Logs $Config
        }
        "stop" {
            Stop-Deployment $Config
        }
        default {
            Write-Error-Custom "Unknown action: $Action"
            Show-Help
            exit 1
        }
    }
}
catch {
    Write-Error-Custom "An error occurred: $_"
    exit 1
}
