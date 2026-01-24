# Script to test login and capture API responses

Write-Host "=== Testing CRM Login Flow ===" -ForegroundColor Cyan

# Test 1: Check if API is responding
Write-Host "`n[1] Checking API health..." -ForegroundColor Yellow
$healthResponse = curl -s http://localhost:5000/api/health
Write-Host "API Health: $healthResponse" -ForegroundColor Green

# Test 2: Login attempt
Write-Host "`n[2] Attempting login..." -ForegroundColor Yellow
$loginResponse = curl -X POST http://localhost:5000/api/auth/login `
  -H "Content-Type: application/json" `
  -d '{
    "email": "abhi.lal@gmail.com",
    "password": "Microsoft@1"
  }' -s

Write-Host "Login Response:" -ForegroundColor Green
$loginResponse | ConvertFrom-Json | Format-List

# Test 3: Check frontend loads
Write-Host "`n[3] Checking frontend loads..." -ForegroundColor Yellow
$frontendResponse = curl -s http://localhost:8070
if ($frontendResponse -match '<title>CRM Solution</title>') {
  Write-Host "Frontend is serving correctly" -ForegroundColor Green
} else {
  Write-Host "Frontend may have issues" -ForegroundColor Red
}

Write-Host "`n=== Debug Instructions ===" -ForegroundColor Cyan
Write-Host "1. Open browser to http://localhost:8070"
Write-Host "2. Open Developer Console (F12)"
Write-Host "3. Go to Console tab"
Write-Host "4. Try to login with: abhi.lal@gmail.com / Microsoft@1"
Write-Host "5. Look for [CRM DEBUG], [CRM ERROR] messages in console"
Write-Host "6. Check Network tab for API requests and responses"
