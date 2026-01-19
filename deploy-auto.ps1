#!/usr/bin/env pwsh
# Automated CRM Deployment - No Interactive Prompts

$RemoteHost = "192.168.0.9"
$RemoteUser = "root"
$RemotePort = "22"
$DeployPath = "/opt/crm"

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║        CRM Automated Build & Deploy on Remote Server      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Step 1: Check remote server status
Write-Host "Step 1: Checking remote server..." -ForegroundColor Green
$sshTest = ssh -o StrictHostKeyChecking=no -p $RemotePort -i "$env:USERPROFILE\.ssh\crm-deploy-key" "${RemoteUser}@${RemoteHost}" "docker --version && docker compose version" 2>&1
Write-Host $sshTest
Write-Host ""

# Step 2: Deploy using docker compose (sources are already there from previous deploy)
Write-Host "Step 2: Building and deploying containers on remote server..." -ForegroundColor Green
$deployCmd = @"
cd /opt/crm
echo "=== Current directory ===" 
pwd && ls -la

echo ""
echo "=== Building Docker images ===" 
docker compose build --no-cache

echo ""
echo "=== Stopping any existing containers ==="
docker compose down || true

echo ""
echo "=== Starting containers ===" 
docker compose up -d

echo ""
echo "=== Container status ===" 
sleep 5
docker compose ps

echo ""
echo "=== Checking API health ===" 
docker compose logs api | tail -20
"@

Write-Host "Building on remote server (this may take 5-10 minutes)..." -ForegroundColor Yellow
$output = ssh -o StrictHostKeyChecking=no -p $RemotePort -i "$env:USERPROFILE\.ssh\crm-deploy-key" "${RemoteUser}@${RemoteHost}" $deployCmd 2>&1
Write-Host $output

# Step 3: Verify deployment
Write-Host ""
Write-Host "Step 3: Verifying deployment..." -ForegroundColor Green

$statusCmd = "cd /opt/crm && docker compose ps"
$status = ssh -o StrictHostKeyChecking=no -p $RemotePort -i "$env:USERPROFILE\.ssh\crm-deploy-key" "${RemoteUser}@${RemoteHost}" $statusCmd 2>&1
Write-Host $status

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║              ✅ Deployment Complete!                      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "Your application is ready:" -ForegroundColor Yellow
Write-Host "  API:      http://${RemoteHost}:5000" -ForegroundColor Cyan
Write-Host "  Swagger:  http://${RemoteHost}:5000/swagger" -ForegroundColor Cyan
Write-Host "  Frontend: http://${RemoteHost}:8070" -ForegroundColor Cyan
Write-Host ""

Write-Host "View detailed logs:" -ForegroundColor Yellow
Write-Host "  ssh -i `"$env:USERPROFILE\.ssh\crm-deploy-key`" root@${RemoteHost} `"cd /opt/crm && docker compose logs -f`"" -ForegroundColor Gray
