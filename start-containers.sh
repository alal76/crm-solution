#!/usr/bin/env bash

SSH_KEY="$HOME/.ssh/crm-deploy-key"
SSH_HOST="root@192.168.0.9"

echo "🚀 Starting CRM containers..."
echo ""

ssh -i "$SSH_KEY" -p 22 -o ConnectTimeout=10 -o StrictHostKeyChecking=no "$SSH_HOST" << 'EOF'
cd /opt/crm
echo "Current directory: $(pwd)"
echo ""

echo "Checking docker compose file..."
if [ -f docker-compose.yml ]; then
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
