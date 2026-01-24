#!/usr/bin/env pwsh
# Add SSH public key to remote server

param(
    [string]$RemoteHost = "192.168.0.9",
    [string]$RemoteUser = "root",
    [string]$RemotePort = "22",
    [string]$LocalKeyPath = "$env:USERPROFILE\.ssh\crm-deploy-key"
)

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║      Setup SSH Key Authentication for Remote Server      ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Remote Host:     $RemoteHost"
Write-Host "  Remote User:     $RemoteUser"
Write-Host "  Remote Port:     $RemotePort"
Write-Host "  Local Key Path:  $LocalKeyPath`n"

# Check if key exists
if (-not (Test-Path "$LocalKeyPath.pub")) {
    Write-Host "ERROR: Public key not found at: $LocalKeyPath.pub" -ForegroundColor Red
    exit 1
}

Write-Host "Step 1: Reading your public key..." -ForegroundColor Green
$PublicKey = Get-Content "$LocalKeyPath.pub"
Write-Host "✓ Public key read successfully`n"

Write-Host "Step 2: Adding public key to remote server..." -ForegroundColor Green
Write-Host "You will be prompted for your password (this is the last time!):`n"

# SSH command to add public key
ssh -p $RemotePort "${RemoteUser}@${RemoteHost}" @"
mkdir -p ~/.ssh
chmod 700 ~/.ssh
echo '$PublicKey' >> ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
echo 'SSH key added successfully!'
"@

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Failed to add SSH key to remote server" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 3: Testing SSH key authentication..." -ForegroundColor Green
ssh -i "$LocalKeyPath" -p $RemotePort "${RemoteUser}@${RemoteHost}" "echo 'SSH key authentication successful!'" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: SSH key authentication failed" -ForegroundColor Red
    exit 1
}

Write-Host "`n╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║         SSH Key Setup Complete! ✓                        ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Host "SSH Key Authentication Ready:" -ForegroundColor Yellow
Write-Host "  You can now connect without entering a password:"
Write-Host "  ssh -i '$LocalKeyPath' -p $RemotePort ${RemoteUser}@${RemoteHost}`n"

Write-Host "For deployment scripts, update the configuration:" -ForegroundColor Yellow
Write-Host "  AuthMethod: 'key'"
Write-Host "  KeyPath: '$LocalKeyPath'`n"

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Update .docker-remote-config.json to use SSH key"
Write-Host "  2. Run: .\deploy-now.ps1"
Write-Host "  3. Your application will be deployed!`n"
