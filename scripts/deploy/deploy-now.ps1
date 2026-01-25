#!/usr/bin/env pwsh
# Direct deployment script for remote Docker server

$RemoteHost = "192.168.0.9"
$RemoteUser = "root"
$RemotePort = "22"
$DeployPath = "/opt/crm"
$Registry = "ghcr.io/alal76"
$ImageTag = "development"
$SSHKey = "$env:USERPROFILE\.ssh\crm-deploy-key"

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║             CRM Application - Remote Deployment           ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "Remote Server Configuration:" -ForegroundColor Yellow
Write-Host "  Host:      $RemoteHost"
Write-Host "  User:      $RemoteUser"
Write-Host "  Port:      $RemotePort"
Write-Host "  Deploy:    $DeployPath"
Write-Host "  Registry:  $Registry"
Write-Host "  Tag:       $ImageTag"
Write-Host "  SSH Key:   $SSHKey`n"

Write-Host "Step 1: Testing SSH Connection..." -ForegroundColor Green
ssh -i "$SSHKey" -p $RemotePort "${RemoteUser}@${RemoteHost}" "echo 'SSH connection successful'" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: SSH connection failed" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 2: Creating deploy directory on remote server..." -ForegroundColor Green
ssh -i "$SSHKey" -p $RemotePort "${RemoteUser}@${RemoteHost}" "mkdir -p $DeployPath" 2>&1

Write-Host "`nStep 3: Copying docker-compose.yml to remote server..." -ForegroundColor Green
scp -i "$SSHKey" -P $RemotePort docker-compose.yml "${RemoteUser}@${RemoteHost}:${DeployPath}/" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to copy docker-compose.yml" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 4: Deploying on remote server..." -ForegroundColor Green

# Prompt for GHCR credentials
Write-Host "`nGitHub Container Registry (GHCR) Credentials Required" -ForegroundColor Cyan
Write-Host "Visit https://github.com/settings/tokens to create a Personal Access Token" -ForegroundColor Gray
Write-Host "Token needs 'read:packages' scope minimum`n" -ForegroundColor Gray

$GHCRUser = Read-Host "GHCR Username (usually 'alal76')"
$GHCRTokenSecure = Read-Host "GHCR Personal Access Token" -AsSecureString
$GHCRToken = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToCoTaskMemUnicode($GHCRTokenSecure))

ssh -i "$SSHKey" -p $RemotePort "${RemoteUser}@${RemoteHost}" @"
set -e
echo "=== Creating deploy directory ==="
mkdir -p $DeployPath
cd $DeployPath

echo ""
echo "=== Docker Login to GHCR ==="
echo "$GHCRToken" | docker login $Registry -u "$GHCRUser" --password-stdin

echo ""
echo "=== Pulling Images ==="
docker pull ${Registry}/crm-api:${ImageTag}
docker pull ${Registry}/crm-frontend:${ImageTag}

echo ""
echo "=== Starting Containers ==="
docker-compose down 2>/dev/null || true
docker-compose up -d

echo ""
echo "=== Container Status ==="
docker-compose ps

echo ""
echo "=== Deployment Complete! ==="
echo "API:      http://${RemoteHost}:5000"
echo "Frontend: http://${RemoteHost}:3000"
"@

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Remote deployment failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║         Deployment Successful!  🎉                        ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "Access your application at:" -ForegroundColor Yellow
Write-Host "  API:      http://${RemoteHost}:5000"
Write-Host "  Swagger:  http://${RemoteHost}:5000/swagger"
Write-Host "  Frontend: http://${RemoteHost}:3000`n"

Write-Host "View logs with:" -ForegroundColor Yellow
Write-Host "  ssh -p $RemotePort ${RemoteUser}@${RemoteHost} 'cd $DeployPath && docker-compose logs -f'`n"
