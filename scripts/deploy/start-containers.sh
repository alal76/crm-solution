#!/usr/bin/env bash
# =============================================================================
# ⚠️  DEPRECATED — Use scripts/deploy.sh instead
# This script is DEPRECATED and will be removed in a future release.
# Use: ./scripts/deploy.sh --env dev
# =============================================================================
echo "⚠️  WARNING: This script is deprecated. Use scripts/deploy.sh --env dev instead." >&2

SSH_KEY="$HOME/.ssh/crm-deploy-key"
SSH_HOST="${BUILD_USER:-deploy}@192.168.0.9"

echo "🚀 Starting CRM containers..."
echo ""

# WARNING: accept-new accepts on first connect but rejects if the host key changes (MITM protection).
ssh -i "$SSH_KEY" -p 22 -o ConnectTimeout=10 -o StrictHostKeyChecking=accept-new "$SSH_HOST" << 'EOF'
cd /opt/crm
echo "Current directory: $(pwd)"
echo ""

echo "Checking docker compose file..."
if [[ -f docker-compose.yml ]]; then
    echo "✅ docker-compose.yml found"
else
    echo "❌ docker-compose.yml NOT found"
    exit 1
fi

echo ""
echo "Starting containers..."
docker compose up -d

echo ""
echo "Waiting 10 seconds for startup..."
sleep 10

echo ""
echo "Container status:"
docker compose ps

echo ""
echo "Frontend status:"
curl -s http://localhost:8070 | head -10 || echo "Frontend not responding yet"

echo ""
echo "API status:"
curl -s http://localhost:5000/health || echo "API not responding yet"
EOF
