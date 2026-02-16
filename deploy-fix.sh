#!/bin/bash

# CRM API Deployment - Manual Steps for 192.168.0.9
# 
# NOTE: This script should be run ON the server (192.168.0.9)
# It assumes you have SSH access to admin@192.168.0.9 and the code is in /src/crm-solution
#
# To use:
# 1. SSH into server: ssh admin@192.168.0.9
# 2. Save this script: nano ~/deploy-fix.sh
# 3. Run: bash ~/deploy-fix.sh

set -e

echo "=========================================="
echo "CRM API Deployment - Field Config Fix"
echo "=========================================="
echo ""

# Step 1: Update source code
echo "[1/5] Pulling latest code from GitHub..."
cd /src/crm-solution
git fetch origin
git checkout feature/p0-p1-architecture-specs-2026-02-16
git pull origin feature/p0-p1-architecture-specs-2026-02-16

echo "✓ Code updated"
echo ""

# Step 2: Verify commit
echo "[2/5] Verifying commit..."
COMMIT=$(git rev-parse --short HEAD)
echo "Current commit: $COMMIT"
if [[ "$COMMIT" == "096d86f" ]]; then
    echo "✓ Correct commit found"
else
    echo "⚠ Expected commit 096d86f, but got $COMMIT"
    echo "This may still work if the fix was cherry-picked"
fi
echo ""

# Step 3: Build Docker image
echo "[3/5] Building Docker image (this may take 3-5 minutes)..."
docker build -t crm-api:latest -f docker/Dockerfile.backend .
echo "✓ Docker image built successfully"
echo ""

# Step 4: Redeploy container
echo "[4/5] Redeploying container..."
echo "  - Stopping old container..."
docker stop crm-api || true
docker rm crm-api || true

echo "  - Starting new container..."
docker run -d \
  --name crm-api \
  --network crm_crm-network \
  --network-alias crm-api \
  -p 5000:5000 \
  -e "ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;" \
  -e "ASPNETCORE_ENVIRONMENT=Production" \
  -e "SKIP_DB_MIGRATION=true" \
  -e "SKIP_WORKFLOW_WORKER=true" \
  -e "Jwt__Secret=CrmJwtSecretKey2024ForSecureAuthentication" \
  crm-api:latest

echo "  - Waiting for container to start..."
sleep 5

if docker ps | grep -q crm-api; then
    echo "✓ Container started successfully"
else
    echo "✗ Container failed to start"
    docker logs crm-api
    exit 1
fi
echo ""

# Step 5: Verify health
echo "[5/5] Verifying health..."
if curl -s http://localhost:5000/health > /dev/null; then
    echo "✓ API health check passed"
else
    echo "✗ API health check failed"
    docker logs crm-api
    exit 1
fi
echo ""

# Step 6: Database cleanup (optional but recommended)
echo ""
echo "=========================================="
echo "Database Cleanup (Optional but Recommended)"
echo "=========================================="
echo ""
echo "The database may still contain old field configurations with moduleName='Customers'."
echo "To clean up and reseed:"
echo ""
echo "  mysql -h crm-mariadb -u crm_user -p crm_db << 'SQL'"
echo "  DELETE FROM ModuleFieldConfigurations WHERE moduleName = 'Customers';"
echo "  SQL"
echo ""
echo "  docker restart crm-api"
echo ""
echo "Then verify:"
echo "  curl -s 'http://localhost:5000/api/modulefieldconfigurations/Accounts' | wc -l"
echo "  (should show 40+ field configurations)"
echo ""

echo "=========================================="
echo "Deployment Complete!"
echo "=========================================="
echo ""
echo "Test with:"
echo "  1. curl http://localhost:5000/health (should return healthy)"
echo "  2. curl 'http://192.168.0.9:5000/api/modulefieldconfigurations/Accounts' (should return fields)"
echo "  3. Open http://192.168.0.9/customers and click 'Add Account' (should show form fields)"
echo ""
