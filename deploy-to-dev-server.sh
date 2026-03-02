#!/bin/bash
#
# CRM Solution - Deployment Script to 192.168.0.9 (Development Server)
# 
# This script handles the complete deployment process:
# 1. Builds Docker images locally (cross-platform for Linux amd64)
# 2. Transfers images and configuration to remote server
# 3. Configures remote environment
# 4. Starts services with Docker Compose
# 5. Verifies deployment health
# 6. Sets up monitoring and backups
#
# Prerequisites:
# - Docker & Docker Buildx installed locally
# - SSH access to root@192.168.0.9
# - ~/.ssh/id_rsa or SSH key configured for 192.168.0.9
#
# Usage:
#   ./deploy-to-dev-server.sh
#   or with custom target:
#   TARGET_SERVER=192.168.0.10 ./deploy-to-dev-server.sh

set -e  # Exit on error

# ============================================================================
# CONFIGURATION
# ============================================================================

TARGET_SERVER="${TARGET_SERVER:?Set TARGET_SERVER environment variable (e.g. 192.168.0.9)}"
SSH_USER="${SSH_USER:-root}"
REMOTE_DEPLOY_DIR="/opt/crm-deployment"
REMOTE_DATA_DIR="/opt/crm/data"
DOCKER_REGISTRY_URL="${DOCKER_REGISTRY_URL:-}"  # Leave empty for local images
BUILD_PLATFORM="linux/amd64"
DEPLOYMENT_TIMEOUT=600

# Color output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ============================================================================
# FUNCTIONS
# ============================================================================

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[✓]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Get CRM version from version.json
get_crm_version() {
    local script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    local version_file="$script_dir/version.json"
    if [[ -f "$version_file" ]] && command -v python3 &>/dev/null; then
        python3 -c "
import json, sys
with open('$version_file') as f:
    v = json.load(f)
print(f\"{v['major']}.{v['minor']}.{v['patch']}\")
" 2>/dev/null || echo "latest"
    else
        echo "latest"
    fi
}

# Get component-specific version from version.json (api, frontend, cdt)
get_component_version() {
    local component="$1"
    local script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    local version_file="$script_dir/version.json"
    if [[ -f "$version_file" ]] && command -v python3 &>/dev/null; then
        python3 -c "
import json, sys
with open('$version_file') as f:
    v = json.load(f)
comp = v.get('components', {}).get('$component', {})
ver = comp.get('version')
if ver:
    print(ver)
else:
    print(f\"{v['major']}.{v['minor']}.{v['patch']}\")
" 2>/dev/null || get_crm_version
    else
        get_crm_version
    fi
}

# Test SSH connectivity
test_ssh_connection() {
    log_info "Testing SSH connectivity to $SSH_USER@$TARGET_SERVER..."
    if ssh -o StrictHostKeyChecking=no -o ConnectTimeout=5 \
        "$SSH_USER@$TARGET_SERVER" "echo 'SSH connection successful'" 2>&1; then
        log_success "SSH connection established"
        return 0
    else
        log_error "Cannot connect to $SSH_USER@$TARGET_SERVER"
        return 1
    fi
}

# Check local Docker
check_docker() {
    log_info "Checking Docker installation..."
    if ! command -v docker &> /dev/null; then
        log_error "Docker is not installed. Please install Docker first."
        return 1
    fi
    
    if ! command -v docker buildx &> /dev/null; then
        log_warning "Docker Buildx not available. Installing..."
        docker pull docker/buildx-container 2>/dev/null || true
    fi
    
    log_success "Docker is available: $(docker --version)"
    return 0
}

# Build Docker images locally for Linux amd64
build_images() {
    log_info "Building Docker images for $BUILD_PLATFORM..."
    
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    
    # Read per-component versions from version.json
    CRM_VERSION=$(get_crm_version)
    API_VERSION=$(get_component_version "api")
    FE_VERSION=$(get_component_version "frontend")
    log_info "Solution version:  $CRM_VERSION"
    log_info "API version:       $API_VERSION"
    log_info "Frontend version:  $FE_VERSION"
    
    # Build Backend API image
    local api_tag="crm-api:${API_VERSION}"
    if [[ "$API_VERSION" != "latest" ]] && docker images -q "$api_tag" 2>/dev/null | grep -q .; then
        log_success "Image $api_tag already exists — skipping build"
    elif [[ -f "$SCRIPT_DIR/docker/Dockerfile.backend" ]]; then
        log_info "Building backend API image ($api_tag)..."
        if docker buildx build \
            --platform $BUILD_PLATFORM \
            -t "$api_tag" \
            -f "$SCRIPT_DIR/docker/Dockerfile.backend" \
            "$SCRIPT_DIR" \
            --load 2>&1 | tail -20; then
            docker tag "$api_tag" crm-api:latest
            log_success "Backend API image built: $api_tag"
        else
            log_error "Failed to build backend API image"
            return 1
        fi
    else
        log_error "Dockerfile.backend not found at $SCRIPT_DIR/docker/Dockerfile.backend"
        return 1
    fi
    
    # Build Frontend image
    local fe_tag="crm-frontend:${FE_VERSION}"
    if [[ "$FE_VERSION" != "latest" ]] && docker images -q "$fe_tag" 2>/dev/null | grep -q .; then
        log_success "Image $fe_tag already exists — skipping build"
    elif [[ -f "$SCRIPT_DIR/docker/Dockerfile.frontend" ]]; then
        log_info "Building frontend image ($fe_tag)..."
        if docker buildx build \
            --platform $BUILD_PLATFORM \
            -t "$fe_tag" \
            -f "$SCRIPT_DIR/docker/Dockerfile.frontend" \
            "$SCRIPT_DIR" \
            --load 2>&1 | tail -20; then
            docker tag "$fe_tag" crm-frontend:latest
            log_success "Frontend image built: $fe_tag"
        else
            log_error "Failed to build frontend image"
            return 1
        fi
    else
        log_error "Dockerfile.frontend not found at $SCRIPT_DIR/docker/Dockerfile.frontend"
        return 1
    fi
}

# Save images as tar archives
save_images() {
    log_info "Saving Docker images as tar archives..." >&2
    
    TEMP_DIR="/tmp/crm-deployment-$$"
    mkdir -p "$TEMP_DIR"
    
    local api_tag="crm-api:$(get_component_version api)"
    local fe_tag="crm-frontend:$(get_component_version frontend)"
    
    log_info "Saving $api_tag image..." >&2
    docker save "$api_tag" -o "$TEMP_DIR/crm-api.tar" || {
        log_error "Failed to save $api_tag image" >&2
        return 1
    }
    log_success "crm-api.tar saved ($(du -h "$TEMP_DIR/crm-api.tar" | cut -f1))" >&2
    
    log_info "Saving $fe_tag image..." >&2
    docker save "$fe_tag" -o "$TEMP_DIR/crm-frontend.tar" || {
        log_error "Failed to save $fe_tag image" >&2
        return 1
    }
    log_success "crm-frontend.tar saved ($(du -h "$TEMP_DIR/crm-frontend.tar" | cut -f1))" >&2
    
    echo "$TEMP_DIR"
}

# Transfer images to remote server
transfer_images() {
    local IMAGE_DIR=$1
    
    log_info "Creating deployment directory on remote server..."
    ssh -o StrictHostKeyChecking=no "$SSH_USER@$TARGET_SERVER" \
        "mkdir -p $REMOTE_DEPLOY_DIR $REMOTE_DATA_DIR && chmod 755 $REMOTE_DEPLOY_DIR $REMOTE_DATA_DIR"
    
    log_info "Transferring Docker images to $SSH_USER@$TARGET_SERVER..."
    log_info "  Transferring crm-api.tar (this may take several minutes)..."
    scp -o StrictHostKeyChecking=no -o ConnectTimeout=30 \
        "$IMAGE_DIR/crm-api.tar" \
        "$SSH_USER@$TARGET_SERVER:$REMOTE_DEPLOY_DIR/" || {
        log_error "Failed to transfer crm-api.tar"
        return 1
    }
    log_success "crm-api.tar transferred"
    
    log_info "  Transferring crm-frontend.tar..."
    scp -o StrictHostKeyChecking=no -o ConnectTimeout=30 \
        "$IMAGE_DIR/crm-frontend.tar" \
        "$SSH_USER@$TARGET_SERVER:$REMOTE_DEPLOY_DIR/" || {
        log_error "Failed to transfer crm-frontend.tar"
        return 1
    }
    log_success "crm-frontend.tar transferred"
}

# Transfer docker-compose file
transfer_docker_compose() {
    log_info "Transferring docker-compose configuration..."
    
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    
    # Use the standard docker-compose.yml (monolith setup)
    if [[ ! -f "$SCRIPT_DIR/docker/docker-compose.yml" ]]; then
        log_error "docker-compose.yml not found"
        return 1
    fi
    
    scp -o StrictHostKeyChecking=no \
        "$SCRIPT_DIR/docker/docker-compose.yml" \
        "$SSH_USER@$TARGET_SERVER:$REMOTE_DEPLOY_DIR/docker-compose.yml" || {
        log_error "Failed to transfer docker-compose.yml"
        return 1
    }
    log_success "docker-compose.yml transferred"
}

# Transfer environment file
transfer_env_file() {
    log_info "Transferring environment configuration..."
    
    SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
    
    # Create a production .env file with safe defaults
    cat > /tmp/crm-deploy.env << ENVFILE
# CRM Production Environment
ASPNETCORE_ENVIRONMENT=Production
DATABASE_PROVIDER=mariadb
DB_HOST=mariadb
DB_PORT=3306
DB_NAME=crm_db
DB_USER=crm_user
DB_PASSWORD=${DB_PASSWORD:?Set DB_PASSWORD before running deploy}
DB_ROOT_PASSWORD=${DB_ROOT_PASSWORD:?Set DB_ROOT_PASSWORD before running deploy}
DB_EXTERNAL_PORT=3306

# API Configuration
API_EXTERNAL_PORT=5000
API_INTERNAL_PORT=5000
API_EXTERNAL_HOSTNAME=${TARGET_SERVER}

# Frontend Configuration
FRONTEND_EXTERNAL_PORT=80
FRONTEND_URL=http://${TARGET_SERVER}

# JWT Security
JWT_SECRET=${JWT_SECRET:?Set JWT_SECRET (min 32 chars)}
JWT_EXPIRATION_MINUTES=60

# Redis Configuration
REDIS_HOST=redis
REDIS_PORT=6379
REDIS_EXTERNAL_PORT=6379
REDIS_ENABLED=true
REDIS_INSTANCE=crm_
Redis__ConnectionString=redis:6379
Redis__InstanceName=crm_
Redis__Enabled=true

# Meilisearch Configuration
MEILISEARCH_ENV=production
MEILISEARCH_API_KEY=${MEILISEARCH_API_KEY:-masterKey}
MEILISEARCH_EXTERNAL_PORT=7700
MEILISEARCH_URL=http://meilisearch:7700

# React App Configuration
REACT_APP_API_URL=http://${TARGET_SERVER}
REACT_APP_API_PORT=5000
REACT_APP_API_EXTERNAL_PORT=5000
REACT_APP_DB_PORT=3306
REACT_APP_FRONTEND_PORT=80

# Deployment Configuration
DEPLOYMENT_TYPE=docker
BUILD_SERVER=localhost
ENABLE_DOCKER_MONITORING=true
ENABLE_K8S_MONITORING=false

# Rate Limiting (disabled for dev server to allow bulk data loading/testing)
RATE_LIMITING_ENABLED=false

# Feature Flags - Provider Selection
USE_EXTERNAL_CHAT=false
USE_EXTERNAL_SEARCH=false
USE_EXTERNAL_NOTIFICATIONS=false
USE_EXTERNAL_ANALYTICS=false
USE_EXTERNAL_SIGNATURES=false
USE_EXTERNAL_AI=false
USE_EXTERNAL_INTEGRATIONS=false

# Feature Flags - Modules
ENABLE_ITSM=true
ENABLE_MARKETING=true
ENABLE_CUSTOMER_PORTAL=false
ENABLE_PARTNER_PORTAL=false
ENABLE_KNOWLEDGE_BASE=true

# Feature Flags - Features
NEW_SEARCH_EXPERIENCE=false
AI_ASSISTANT=true
REALTIME_NOTIFICATIONS=true
ADVANCED_WORKFLOWS=true
DEMO_AUTO_SEED=true
DEMO_DB_NAME=crm_demodb

# Module Field Config Re-seeding (set to true on first deploy or when field defs change)
FORCE_RESEED_FIELD_CONFIGS=true

# Contract Storage
CONTRACT_STORAGE_PATH=/app/data/contracts
MAX_CONTRACT_FILE_SIZE_BYTES=10485760
ALLOWED_MIME_TYPES=application/pdf,image/png,image/jpeg,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document

# Provider Configurations
PROVIDER_CHAT_TYPE=BuiltIn
PROVIDER_SEARCH_TYPE=BuiltIn
PROVIDER_NOTIFICATIONS_TYPE=BuiltIn
PROVIDER_ANALYTICS_TYPE=BuiltIn
PROVIDER_SIGNATURES_TYPE=BuiltIn
PROVIDER_INTEGRATIONS_TYPE=BuiltIn

# Compose Project Name
COMPOSE_PROJECT_NAME=crm
ENVFILE

    scp -o StrictHostKeyChecking=no \
        /tmp/crm-deploy.env \
        "$SSH_USER@$TARGET_SERVER:$REMOTE_DEPLOY_DIR/.env" || {
        log_error "Failed to transfer .env file"
        return 1
    }
    log_success ".env file transferred"
    
    rm /tmp/crm-deploy.env
}

# Load images and start services on remote server
deploy_to_remote() {
    log_info "Deploying to remote server..."
    
    ssh -o StrictHostKeyChecking=no "$SSH_USER@$TARGET_SERVER" << REMOTE_SCRIPT
set -e

echo "=========================================="
echo "CRM Deployment on Remote Server"
echo "=========================================="
echo "Target: $TARGET_SERVER"
echo "Time: \$(date)"
echo "=========================================="
echo ""

cd $REMOTE_DEPLOY_DIR

# Load Docker images
echo "[1/5] Loading Docker images..."
docker load -i crm-api.tar || { echo "Failed to load crm-api.tar"; exit 1; }
echo "✓ crm-api image loaded"

docker load -i crm-frontend.tar || { echo "Failed to load crm-frontend.tar"; exit 1; }
echo "✓ crm-frontend image loaded"

# Verify images
echo "[2/5] Verifying Docker images..."
docker images | grep -E "crm-api|crm-frontend"

# Stop and remove existing containers if any
echo "[3/5] Stopping existing containers (if any)..."
docker-compose down --remove-orphans 2>/dev/null || true
sleep 2

# Start services (force recreate to pick up config changes)
echo "[4/5] Starting services with Docker Compose..."
docker-compose -f docker-compose.yml up -d --force-recreate

# Wait for services to initialize
echo "[5/5] Waiting for services to be ready..."
sleep 15

# Show container status
echo ""
echo "=========================================="
echo "Service Status:"
echo "=========================================="
docker-compose ps

echo ""
echo "=========================================="
echo "Deployment Summary"
echo "=========================================="
echo "API Container: crm-api"
echo "Frontend Container: crm-frontend"
echo "Database Container: crm-mariadb"
echo "Redis Container: crm-redis"
echo "Search Container: crm-meilisearch"
echo ""
echo "API Health: http://${TARGET_SERVER}:5000/health"
echo "Frontend: http://${TARGET_SERVER}"
echo "=========================================="
REMOTE_SCRIPT

    if [[ $? -eq 0 ]]; then
        log_success "Services deployed successfully"
        return 0
    else
        log_error "Failed to deploy services"
        return 1
    fi
}

# Verify deployment
verify_deployment() {
    log_info "Verifying deployment..."
    
    sleep 5
    
    # Check API health
    log_info "Checking API health endpoint..."
    if curl -s -m 5 "http://$TARGET_SERVER:5000/health" > /dev/null 2>&1; then
        log_success "API is responding"
    else
        log_warning "API not responding yet, services may still be initializing"
    fi
    
    # Check frontend
    log_info "Checking Frontend availability..."
    if curl -s -m 5 "http://$TARGET_SERVER" > /dev/null 2>&1; then
        log_success "Frontend is responding"
    else
        log_warning "Frontend not responding yet, services may still be initializing"
    fi
    
    # Remote health check
    log_info "Performing remote health checks..."
    ssh -o StrictHostKeyChecking=no "$SSH_USER@$TARGET_SERVER" << HEALTHCHECK
echo "Checking container status..."
docker ps --format "table {{.Names}}\t{{.Status}}"

echo ""
echo "Checking network:"
docker network ls | grep crm

echo ""
echo "Checking volumes:"
docker volume ls | grep crm || echo "Volumes not yet created"

echo ""
echo "API Container logs (last 20 lines):"
docker logs --tail 20 crm-api 2>/dev/null || echo "Logs not available"

echo ""
echo "Database ready check:"
docker exec crm-mariadb mysql -u crm_user -p\${DB_PASSWORD} -e "SELECT 1" 2>/dev/null && echo "✓ Database is ready" || echo "⚠ Database still initializing"
HEALTHCHECK
    
    log_success "Deployment verification complete"
}

# Setup monitoring and auto-restart
setup_monitoring() {
    log_info "Setting up monitoring and auto-restart policies..."
    
    ssh -o StrictHostKeyChecking=no "$SSH_USER@$TARGET_SERVER" << MONITORING
# Enable auto-restart policy for containers
docker update --restart=unless-stopped crm-api
docker update --restart=unless-stopped crm-frontend
docker update --restart=unless-stopped crm-mariadb
docker update --restart=unless-stopped crm-redis
docker update --restart=unless-stopped crm-meilisearch

echo "✓ Auto-restart policies configured"

# Create systemd service for CRM stack (optional)
if command -v systemctl &>/dev/null; then
    cat > /etc/systemd/system/crm-docker.service << 'SERVICEEOF'
[Unit]
Description=CRM Application Stack (Docker Compose)
Requires=docker.service
After=docker.service
StartLimitIntervalSec=60
StartLimitBurst=3

[Service]
Type=simple
Restart=on-failure
RestartSec=10
WorkingDirectory=$REMOTE_DEPLOY_DIR
ExecStart=/usr/bin/docker-compose -f docker-compose.yml up
ExecStop=/usr/bin/docker-compose -f docker-compose.yml down

[Install]
WantedBy=multi-user.target
SERVICEEOF

    systemctl enable crm-docker.service 2>/dev/null || true
    echo "✓ Systemd service configured"
fi
MONITORING
    
    log_success "Monitoring and auto-restart configured"
}

# Setup backup strategy
setup_backups() {
    log_info "Setting up backup strategy..."
    
    ssh -o StrictHostKeyChecking=no "$SSH_USER@$TARGET_SERVER" << BACKUP
# Create backup directory
mkdir -p /opt/crm/backups
chmod 755 /opt/crm/backups

# Create backup script
cat > /opt/crm/backup.sh << 'BACKUPEOF'
#!/bin/bash
BACKUP_DIR="/opt/crm/backups"
TIMESTAMP=\$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="\$BACKUP_DIR/crm_db_\$TIMESTAMP.sql"

echo "Starting database backup at \$(date)..."

# Backup database
docker exec crm-mariadb mysqldump \\
  -u crm_user \\
  -p\${DB_PASSWORD} \\
  crm_db > "\$BACKUP_FILE" 2>/dev/null

if [[ -f "\$BACKUP_FILE" ]]; then
    # Compress backup
    gzip "\$BACKUP_FILE"
    COMPRESSED_FILE="\$BACKUP_FILE.gz"
    SIZE=\$(du -h "\$COMPRESSED_FILE" | cut -f1)
    
    echo "Backup completed: \$COMPRESSED_FILE (\$SIZE)"
    
    # Cleanup old backups (keep 7 days)
    echo "Cleaning up backups older than 7 days..."
    find "\$BACKUP_DIR" -name "crm_db_*.sql.gz" -mtime +7 -delete
    
    echo "Backup finished at \$(date)"
else
    echo "ERROR: Backup failed - no backup file created"
    exit 1
fi
BACKUPEOF

chmod +x /opt/crm/backup.sh

# Schedule daily backup at 2 AM
(crontab -l 2>/dev/null | grep -v backup.sh; echo "0 2 * * * /opt/crm/backup.sh") | crontab -

echo "✓ Backup script created and scheduled"
BACKUP
    
    log_success "Backup strategy configured"
}

# Cleanup temporary files
cleanup() {
    log_info "Cleaning up temporary files..."
    rm -rf /tmp/crm-deployment-$$
    log_success "Cleanup complete"
}

# Print deployment summary
print_summary() {
    cat << SUMMARY

========================================
✓ DEPLOYMENT COMPLETE
========================================

Target Server: $TARGET_SERVER
Deployment Directory: $REMOTE_DEPLOY_DIR

SERVICES:
  • API (Backend): http://$TARGET_SERVER:5000
  • Frontend: http://$TARGET_SERVER
  • Database: MariaDB on port 3306
  • Cache: Redis on port 6379
  • Search: Meilisearch on port 7700

MONITORING:
  • API Health: http://$TARGET_SERVER:5000/health
  • Frontend: http://$TARGET_SERVER/health (if available)
  • View Logs: ssh $SSH_USER@$TARGET_SERVER 'docker-compose -f $REMOTE_DEPLOY_DIR/docker-compose.yml logs -f'

MANAGEMENT:
  • View Services: ssh $SSH_USER@$TARGET_SERVER 'docker ps'
  • Stop Stack: ssh $SSH_USER@$TARGET_SERVER 'cd $REMOTE_DEPLOY_DIR && docker-compose down'
  • Restart Stack: ssh $SSH_USER@$TARGET_SERVER 'cd $REMOTE_DEPLOY_DIR && docker-compose restart'

ADMINISTRATION:
  • Database: Log in with user 'crm_user' — see .env for password
  • Default Admin: Email: admin@crm.local — see .env for password

BACKUPS:
  • Location: /opt/crm/backups
  • Schedule: Daily at 2 AM
  • Retention: 7 days

========================================
Next Steps:
1. Visit http://$TARGET_SERVER in your browser
2. Log in with admin credentials
3. Configure CRM settings as needed
4. Run any initial data seeding required
========================================

SUMMARY
}

# ============================================================================
# MAIN DEPLOYMENT FLOW
# ============================================================================

main() {
    log_info "CRM Solution Deployment to $TARGET_SERVER"
    echo ""
    
    # 1. Check prerequisites
    log_info "Step 1: Checking prerequisites..."
    check_docker || exit 1
    test_ssh_connection || exit 1
    log_success "Prerequisites check passed"
    echo ""
    
    # 2. Build images
    log_info "Step 2: Building Docker images..."
    build_images || exit 1
    log_success "Docker images built"
    echo ""
    
    # 3. Save and transfer images
    log_info "Step 3: Preparing and transferring deployment artifacts..."
    IMAGE_DIR=$(save_images) || exit 1
    transfer_images "$IMAGE_DIR" || exit 1
    transfer_docker_compose || exit 1
    transfer_env_file || exit 1
    log_success "Artifacts transferred to remote server"
    echo ""
    
    # 4. Deploy to remote
    log_info "Step 4: Deploying to remote server..."
    deploy_to_remote || exit 1
    echo ""
    
    # 5. Verify deployment
    log_info "Step 5: Verifying deployment..."
    verify_deployment || exit 1
    echo ""
    
    # 6. Setup monitoring and backups
    log_info "Step 6: Setting up monitoring and backups..."
    setup_monitoring
    setup_backups
    echo ""
    
    # 7. Cleanup
    cleanup
    
    # 8. Print summary
    print_summary
}

# Run main deployment
main "$@"
