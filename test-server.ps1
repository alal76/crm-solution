#!/usr/bin/env pwsh

Write-Host "Testing server connectivity..." -ForegroundColor Yellow

# Test network connectivity
Write-Host ""
Write-Host "Ping test:" -ForegroundColor Cyan
if (Test-Connection -ComputerName 192.168.0.9 -Count 1 -Quiet -TimeoutSeconds 5) {
    Write-Host "✅ Server is reachable" -ForegroundColor Green
    
    # Try SSH with timeout
    Write-Host ""
    Write-Host "Testing SSH connection..." -ForegroundColor Cyan
    $sshKey = "$env:USERPROFILE\.ssh\crm-deploy-key"
    $result = ssh -i "$sshKey" -p 22 -o ConnectTimeout=5 -o StrictHostKeyChecking=no root@192.168.0.9 "docker ps --format 'table {{.Names}}\t{{.Status}}'" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ SSH connection successful" -ForegroundColor Green
        Write-Host $result
    } else {
        Write-Host "❌ SSH connection failed or timed out" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Server is NOT reachable (ping failed)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting steps:" -ForegroundColor Yellow
    Write-Host "1. Check if server 192.168.0.9 is powered on" -ForegroundColor Yellow
    Write-Host "2. Verify network connectivity to the server" -ForegroundColor Yellow
    Write-Host "3. Check if firewall is blocking ICMP/SSH" -ForegroundColor Yellow
}
