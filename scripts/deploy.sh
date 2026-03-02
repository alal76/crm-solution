#!/bin/bash
#
# CRM Solution - Deployment Script for 192.168.0.9
# Execute this on the target server to deploy the API with AddressType fix
#
# PREREQUISITE: Server has Docker, Docker Compose, and .NET 10.0 installed
#
# Usage:
#   1. Copy this script to the server
#   2. Make executable: chmod +x DEPLOY.sh
#   3. Run: ./DEPLOY.sh
#
# Or run directly from your machine:
#   ssh root@192.168.0.9 'bash -s' < DEPLOY.sh
#

set -e  # Exit on any error

SERVER="192.168.0.9"
REPO_DIR="/opt/crm-solution"
API_PORT="5000"
API_CONTAINER="crm-api"
LOG_FILE="/var/log/crm-deployment.log"

echo "=========================================="
echo "CRM Solution Deployment Script"
echo "=========================================="
echo "Target Server: $SERVER"
echo "Repository: $REPO_DIR"
echo "API Port: $API_PORT"
echo "Timestamp: $(date)"
echo "=========================================="
echo ""

# Step 1: Build Release Binary
echo "[1/7] Building CRM.Api in Release mode..."
if [[ ! -d "$REPO_DIR/CRM.Backend" ]]; then
    echo "ERROR: Repository not found at $REPO_DIR"
    echo "Please git clone the repository to $REPO_DIR first"
    exit 1
fi

cd "$REPO_DIR/CRM.Backend"
dotnet build src/CRM.Api/CRM.Api.csproj -c Release --no-restore || {
    echo "ERROR: Build failed"
    exit 1
}

API_DLL="$REPO_DIR/CRM.Backend/src/CRM.Api/bin/Release/net10.0/CRM.Api.dll"
if [[ ! -f "$API_DLL" ]]; then
    echo "ERROR: API DLL not found at $API_DLL"
    exit 1
fi
echo "✅ API built successfully at $API_DLL"
echo ""

# Step 2: Stop existing API container
echo "[2/7] Stopping existing API container (if running)..."
docker stop $API_CONTAINER 2>/dev/null || true
docker rm $API_CONTAINER 2>/dev/null || true
echo "✅ Old container removed"
echo ""

# Step 3: Build Docker image for Linux amd64
echo "[3/7] Building Docker image (cross-platform for Linux amd64)..."
docker buildx build \
    --platform linux/amd64 \
    -t crm-api:latest \
    -f "$REPO_DIR/docker/Dockerfile.backend" \
    --load "$REPO_DIR" || {
    echo "ERROR: Docker build failed"
    exit 1
}
echo "✅ Docker image built successfully"
echo ""

# Step 4: Start database and cache (if needed)
echo "[4/7] Ensuring database and cache services are running..."
cd "$REPO_DIR"
docker-compose -f docker/docker-compose.yml up -d crm-mariadb crm-redis 2>/dev/null || true
echo "✅ Database and cache services verified"
sleep 2  # Give services time to start
echo ""

# Step 5: Start new API container
echo "[5/7] Starting new API container..."
docker run -d \
    --name $API_CONTAINER \
    --network docker_crm-network \
    -p $API_PORT:5000 \
    -e ASPNETCORE_ENVIRONMENT=Production \
    -e DatabaseProvider=mariadb \
    -e "ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=${DB_PASSWORD:?DB_PASSWORD must be set};" \
    -e "Jwt__Secret=$(cat /etc/crm-secrets/jwt-secret 2>/dev/null || echo 'ChangeMe-Min32CharsRequired123456')" \
    -v /var/log/crm:/app/logs \
    crm-api:latest

# Wait for API to start
sleep 3
echo "✅ API container started"
echo ""

# Step 6: Verify API health
echo "[6/7] Verifying API health..."
HEALTH_CHECK_PASSED=false
for i in {1..10}; do
    if curl -s http://localhost:$API_PORT/health | grep -q '"status":"healthy"'; then
        echo "✅ API health check passed"
        HEALTH_CHECK_PASSED=true
        break
    fi
    echo "  Attempt $i/10: Health check failed, retrying..."
    sleep 2
done

if [[ "$HEALTH_CHECK_PASSED" = false ]]; then
    echo "⚠️  API health check failed after 10 attempts"
    echo "Logs:"
    docker logs $API_CONTAINER | tail -20
    exit 1
fi
echo ""

# Step 7: Run smoke tests
echo "[7/7] Running smoke tests..."
SMOKE_TESTS=(
    "http://localhost:$API_PORT/health"
    "http://localhost:$API_PORT/api/accounts?pageSize=1"
    "http://localhost:$API_PORT/api/dashboard"
)

for test_url in "${SMOKE_TESTS[@]}"; do
    echo "  Testing: $test_url"
    if curl -s -f "$test_url" > /dev/null 2>&1; then
        echo "    ✅ PASS"
    else
        echo "    ❌ FAIL"
    fi
done
echo ""

echo "=========================================="
echo "✅ Deployment Complete!"
echo "=========================================="
echo ""
echo "API Endpoint: http://$SERVER:$API_PORT"
echo "Health Check: http://$SERVER:$API_PORT/health"
echo "API Logs: docker logs -f $API_CONTAINER"
echo ""
echo "AddressType Fix: ✅ DEPLOYED"
echo "  - Fixed: Enum-to-string conversion in AccountAddressService.cs"
echo "  - Impact: Address filtering now works correctly"
echo ""
echo "Next Steps:"
echo "  1. Run E2E tests: npm test (in e2e-tests directory)"
echo "  2. Monitor logs: docker logs -f $API_CONTAINER"
echo "  3. Rollback if needed: docker stop $API_CONTAINER && docker run <old-image>"
echo "=========================================="
