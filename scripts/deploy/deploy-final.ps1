#!/usr/bin/env pwsh
# Simplified CRM Deployment Script with SSH Password Caching

param(
    [string]$GHCRToken = ""
)

$RemoteHost = "192.168.0.9"
$RemoteUser = "root"
$RemotePort = "22"
$DeployPath = "/opt/crm"
$Registry = "ghcr.io/alal76"
$ImageTag = "development"
$SSHKey = "$env:USERPROFILE\.ssh\crm-deploy-key"

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║             CRM Deployment - Remote Docker Server         ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Helper function to run SSH commands 
function Invoke-RemoteCommand {
    param(
        [string]$Command,
        [switch]$Quiet = $false
    )
    
    $output = ssh -i "$SSHKey" -p $RemotePort -o ConnectTimeout=10 -o StrictHostKeyChecking=no "${RemoteUser}@${RemoteHost}" $Command 2>&1
    if (-not $Quiet) {
        $output
    }
    return $LASTEXITCODE
}

# Helper function for SCP 
function Copy-ToRemote {
    param(
        [string]$LocalPath,
        [string]$RemotePath
    )
    
    scp -i "$SSHKey" -P $RemotePort -o ConnectTimeout=10 -o StrictHostKeyChecking=no $LocalPath "${RemoteUser}@${RemoteHost}:${RemotePath}" 2>&1
    return $LASTEXITCODE
}

# Test SSH connection
Write-Host "Testing SSH connection..." -ForegroundColor Green
Invoke-RemoteCommand "echo 'SSH OK'" -Quiet | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ SSH connection failed" -ForegroundColor Red
    Write-Host "Your first SSH connection will prompt for password, which will be cached by Windows SSH for this session." -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ SSH connection successful`n" -ForegroundColor Green

# Get GHCR credentials
if (-not $GHCRToken) {
    Write-Host "GitHub Container Registry Credentials" -ForegroundColor Yellow
    Write-Host "Create token at: https://github.com/settings/tokens (read:packages)`n" -ForegroundColor Gray
    
    $GHCRUser = Read-Host "GHCR Username"
    $GHCRTokenSecure = Read-Host "GHCR Personal Access Token" -AsSecureString
    $GHCRToken = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
        [Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($GHCRTokenSecure)
    )
} else {
    $GHCRUser = "alal76"
}

Write-Host ""
Write-Host "Preparing deployment..." -ForegroundColor Green

# Create deploy directory and copy files
Write-Host "Setting up deployment directory..." -ForegroundColor Green
Invoke-RemoteCommand "mkdir -p $DeployPath" -Quiet | Out-Null
Copy-ToRemote docker-compose.yml "${RemoteUser}@${RemoteHost}:${DeployPath}/" | Out-Null
Copy-ToRemote Dockerfile.backend "${RemoteUser}@${RemoteHost}:${DeployPath}/" | Out-Null
Copy-ToRemote Dockerfile.frontend "${RemoteUser}@${RemoteHost}:${DeployPath}/" | Out-Null
Write-Host "✅ Files copied`n" -ForegroundColor Green

# Deploy using SSH
Write-Host "Starting deployment..." -ForegroundColor Green

$deployScript = @"
#!/bin/bash
set -e
cd $DeployPath

echo "=== Docker Login ==="
echo '$GHCRToken' | docker login $Registry -u '$GHCRUser' --password-stdin 2>&1 | grep -v 'WARNING'

echo ""
echo "=== Pulling Images ==="
docker pull ${Registry}/crm-api:${ImageTag}
docker pull ${Registry}/crm-frontend:${ImageTag}

echo ""
echo "=== Stopping Old Containers ==="
docker compose down || true

echo ""
echo "=== Starting New Containers ==="
docker compose up -d

echo ""
echo "=== Container Status ==="
sleep 3
docker compose ps

echo ""
echo "Deployment complete!"
"@

# Write script to temp file and execute using cached connection
$tempScript = "/tmp/crm-deploy-$(Get-Random).sh"
Invoke-RemoteCommand @"
cat > $tempScript << 'DEPLOY_EOF'
$deployScript
DEPLOY_EOF
chmod +x $tempScript
bash $tempScript
rm -f $tempScript
"@ | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n⚠️  Deployment script completed with status code: $LASTEXITCODE" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ✅ Deployment Complete!                      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "Access your application:" -ForegroundColor Yellow
Write-Host "  API:      http://${RemoteHost}:5000" -ForegroundColor Cyan
Write-Host "  Swagger:  http://${RemoteHost}:5000/swagger" -ForegroundColor Cyan
Write-Host "  Frontend: http://${RemoteHost}:8070" -ForegroundColor Cyan
Write-Host ""

Write-Host "Quick Commands:" -ForegroundColor Yellow
Write-Host "  View logs:    ssh -i `"$SSHKey`" -p $RemotePort ${RemoteUser}@${RemoteHost} `"cd $DeployPath && docker compose logs -f`"" -ForegroundColor Gray
Write-Host "  Restart:      ssh -i `"$SSHKey`" -p $RemotePort ${RemoteUser}@${RemoteHost} `"cd $DeployPath && docker compose restart`"" -ForegroundColor Gray
Write-Host "  Stop:         ssh -i `"$SSHKey`" -p $RemotePort ${RemoteUser}@${RemoteHost} `"cd $DeployPath && docker compose down`"" -ForegroundColor Gray
Write-Host ""
