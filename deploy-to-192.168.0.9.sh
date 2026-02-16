#!/bin/bash
# ============================================================================
# CRM Solution - Deployment Script to 192.168.0.9
# ============================================================================
# This script deploys the CRM API Docker image along with MariaDB and Redis
# to the development server at 192.168.0.9
#
# Prerequisites:
# - SSH access to root@192.168.0.9
# - Docker installed on both local and remote machines
# - ~2GB disk space on remote server
#
# Usage:
#   bash deploy-to-192.168.0.9.sh
# ============================================================================

set -e

# Configuration
REMOTE_HOST="192.168.0.9"
REMOTE_USER="root"
REMOTE_APP_DIR="/opt/crm"
LOCAL_IMAGE="crm-api:latest"
DEPLOYMENT_TIMEOUT=600

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Helper functions
log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[✓]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Check prerequisites
log_info "Verifying prerequisites..."
command -v docker &> /dev/null || { log_error "Docker not found"; exit 1; }
command -v ssh &> /dev/null || { log_error "SSH not found"; exit 1; }
command -v scp &> /dev/null || { log_error "SCP not found"; exit 1; }
log_success "Prerequisites verified"

# Test SSH connectivity
log_info "Testing SSH connection to $REMOTE_USER@$REMOTE_HOST..."
if ! ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 "$REMOTE_USER@$REMOTE_HOST" "echo 'SSH OK'" &>/dev/null; then
    log_error "Cannot connect to $REMOTE_USER@$REMOTE_HOST. Please check:"
    log_info "  1. Server IP is accessible (ping $REMOTE_HOST)"
    log_info "  2. SSH is enabled on remote server"
    log_info "  3. Public key is in ~/.ssh/authorized_keys on remote"
    exit 1
fi
log_success "SSH connection established"

# Step 1: Save and transfer Docker image
log_info "Step 1: Transferring Docker image (this may take 2-3 minutes)..."
TEMP_IMAGE="/tmp/crm-api-$(date +%s).tar.gz"
docker image save $LOCAL_IMAGE | gzip > "$TEMP_IMAGE"
IMAGE_SIZE=$(du -h "$TEMP_IMAGE" | cut -f1)
log_info "  Image size: $IMAGE_SIZE"
log_info "  Uploading to remote server..."
scp -o ConnectTimeout=10 "$TEMP_IMAGE" "$REMOTE_USER@$REMOTE_HOST:/tmp/crm-api-latest.tar.gz"
rm "$TEMP_IMAGE"
log_success "Docker image transferred"

# Step 2: Create remote deployment directory
log_info "Step 2: Setting up remote deployment directory..."
ssh -o StrictHostKeyChecking=no "$REMOTE_USER@$REMOTE_HOST" "mkdir -p $REMOTE_APP_DIR"
log_success "Deployment directory created"

# Step 3: Transfer docker-compose file
log_info "Step 3: Configuring Docker Compose..."
COMPOSE_FILE="/tmp/docker-compose-deploy.yml"
cat > "$COMPOSE_FILE" << 'DOCKER_COMPOSE'
version: '3.8'

services:
  crm-api:
    image: crm-api:latest
    container_name: crm-api
    restart: unless-stopped
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - DATABASE_PROVIDER=mariadb
      - CONNECTIONSTRINGS__DEFAULTCONNECTION=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;
      - REDIS__CONNECTIONSTRING=crm-redis:6379
    depends_on:
      - crm-mariadb
      - crm-redis
    networks:
      - crm-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

  crm-mariadb:
    image: mariadb:11.4-ubi8
    container_name: crm-mariadb
    restart: unless-stopped
    ports:
      - "3306:3306"
    environment:
      MYSQL_ROOT_PASSWORD: RootPass@Dev2024
      MYSQL_DATABASE: crm_db
      MYSQL_USER: crm_user
      MYSQL_PASSWORD: CrmPass@Dev2024
    volumes:
      - crm-db-data:/var/lib/mysql
    networks:
      - crm-network
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost", "-u", "root", "-pRootPass@Dev2024"]
      interval: 30s
      timeout: 10s
      retries: 3

  crm-redis:
    image: redis:7-alpine
    container_name: crm-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    volumes:
      - crm-redis-data:/data
    networks:
      - crm-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 30s
      timeout: 10s
      retries: 3

volumes:
  crm-db-data:
  crm-redis-data:

networks:
  crm-network:
    driver: bridge
DOCKER_COMPOSE

scp -o ConnectTimeout=10 "$COMPOSE_FILE" "$REMOTE_USER@$REMOTE_HOST:$REMOTE_APP_DIR/docker-compose.yml"
rm "$COMPOSE_FILE"
log_success "Docker Compose configured"

# Step 4: Load image and start services
log_info "Step 4: Loading Docker image and starting services..."
ssh -o StrictHostKeyChecking=no "$REMOTE_USER@$REMOTE_HOST" << 'REMOTE_COMMANDS'
echo "[INFO] Loading Docker image..."
gunzip -c /tmp/crm-api-latest.tar.gz | docker load

echo "[INFO] Stopping existing services..."
cd /opt/crm
docker-compose down --remove-orphans 2>/dev/null || true

echo "[INFO] Starting services..."
docker-compose up -d

echo "[INFO] Waiting for services to be healthy..."
sleep 20

echo "[INFO] Checking container status..."
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
REMOTE_COMMANDS

log_success "Services started"

# Step 5: Verify deployment
log_info "Step 5: Verifying deployment..."
if ssh -o StrictHostKeyChecking=no "$REMOTE_USER@$REMOTE_HOST" "curl -f -s -m 5 http://localhost:5000/health > /dev/null && echo OK" &>/dev/null; then
    log_success "API is responding to health checks"
else
    log_error "API health check failed - services may still be starting"
    log_info "  Check status with: ssh root@192.168.0.9 'docker ps'"
    log_info "  Check logs with: ssh root@192.168.0.9 'docker logs crm-api'"
fi

# Display summary
echo ""
echo "╔════════════════════════════════════════════════════════════════╗"
echo "║           ✓ Deployment to 192.168.0.9 Complete!              ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""
echo "Services deployed:"
echo "  • CRM API............... http://192.168.0.9:5000"
echo "  • MariaDB............... 192.168.0.9:3306 (crm_user / CrmPass@Dev2024)"
echo "  • Redis Cache........... 192.168.0.9:6379"
echo ""
echo "Useful commands:"
echo "  Check status........... ssh root@192.168.0.9 'docker ps'"
echo "  View API logs.......... ssh root@192.168.0.9 'docker logs -f crm-api'"
echo "  Database access........ mysql -h 192.168.0.9 -u crm_user -p crm_db"
echo "  Access container....... ssh root@192.168.0.9 'docker exec -it crm-api /bin/bash'"
echo ""
echo "API Health Check:"
curl -s http://192.168.0.9:5000/health 2>/dev/null && echo "  ✓ API is healthy" || echo "  ⚠ API not responding yet"
echo ""
