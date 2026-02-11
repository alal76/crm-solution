#!/bin/bash
# =============================================================================
# ⚠️  DEPRECATED — Use scripts/deploy.sh instead
# =============================================================================
# CRM Microservices Deployment to 192.168.0.9
# This script deploys the CRM solution as microservices to 192.168.0.9
# Uses Docker Compose for containerized deployment
#
# This script is DEPRECATED and will be removed in a future release.
# Use the unified parameterized script instead:
#   ./scripts/deploy.sh --env dev --mode microservices
# =============================================================================
echo "⚠️  WARNING: This script is deprecated. Use scripts/deploy.sh --env dev --mode microservices instead." >&2

set -e

# Configuration
REMOTE_HOST="192.168.0.9"
REMOTE_USER="${DEPLOY_USER:-alal}"
REMOTE_DIR="/opt/crm-solution"
LOCAL_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

print_header() {
    echo ""
    echo -e "${CYAN}============================================${NC}"
    echo -e "${CYAN}$1${NC}"
    echo -e "${CYAN}============================================${NC}"
}

# Check SSH connectivity
check_ssh() {
    print_header "Checking SSH Connectivity"
    if ssh -o ConnectTimeout=5 -o BatchMode=yes "${REMOTE_USER}@${REMOTE_HOST}" "echo 'SSH OK'" 2>/dev/null; then
        log_success "SSH connection established (key-based auth)"
        return 0
    else
        log_warning "Key-based SSH failed, will prompt for password"
        return 0
    fi
}

# Sync project files to remote server
sync_files() {
    print_header "Syncing Project Files to ${REMOTE_HOST}"
    
    log_info "Creating remote directory structure..."
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "sudo mkdir -p ${REMOTE_DIR} && sudo chown -R ${REMOTE_USER}:${REMOTE_USER} ${REMOTE_DIR}" || true
    
    log_info "Syncing files with rsync..."
    rsync -avz --delete \
        --exclude '.git' \
        --exclude 'node_modules' \
        --exclude 'bin' \
        --exclude 'obj' \
        --exclude '.vs' \
        --exclude '*.user' \
        --exclude 'coverage' \
        --exclude 'TestResults' \
        --exclude 'test-results' \
        --exclude 'playwright-report' \
        --exclude '.idea' \
        --exclude 'artifacts' \
        --exclude 'CRM.Frontend/build' \
        --exclude '*.log' \
        "${LOCAL_DIR}/" "${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_DIR}/"
    
    log_success "Files synced successfully"
}

# Stop existing containers
stop_containers() {
    print_header "Stopping Existing Containers"
    
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "
        cd ${REMOTE_DIR}
        docker compose -f docker/docker-compose.microservices.unified.yml down --remove-orphans 2>/dev/null || true
        docker compose -f docker-compose.microservices.yml down --remove-orphans 2>/dev/null || true
    "
    
    log_success "Existing containers stopped"
}

# Build Docker images on remote
build_images() {
    print_header "Building Docker Images on ${REMOTE_HOST}"
    
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "
        cd ${REMOTE_DIR}
        
        # Source environment variables
        export ASPNETCORE_ENVIRONMENT=Production
        export MS_VERSION=latest
        export DB_PASSWORD=CrmPass@Dev2024
        export DB_USER=crm_user
        export DB_NAME=crm_db
        export JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
        export PORT_FRONTEND_EXTERNAL=80
        
        echo '>>> Building all microservice images...'
        docker compose -f docker/docker-compose.microservices.unified.yml build --parallel
    "
    
    log_success "Docker images built successfully"
}

# Deploy containers
deploy_containers() {
    print_header "Deploying Microservices Containers"
    
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "
        cd ${REMOTE_DIR}
        
        # Export environment variables
        export ASPNETCORE_ENVIRONMENT=Production
        export MS_VERSION=latest
        export DB_PASSWORD=CrmPass@Dev2024
        export DB_USER=crm_user
        export DB_NAME=crm_db
        export DB_ROOT_PASSWORD=RootPass@Dev2024
        export JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharactersLong!
        export PORT_FRONTEND_EXTERNAL=80
        export PORT_GATEWAY=5000
        export PORT_IDENTITY=5001
        export PORT_CUSTOMER=5002
        export PORT_SALES=5003
        export PORT_MARKETING=5004
        export PORT_SERVICEDESK=5005
        export PORT_CORE=5006
        
        echo '>>> Starting microservices...'
        docker compose -f docker/docker-compose.microservices.unified.yml up -d
    "
    
    log_success "Containers deployed successfully"
}

# Wait for services to be healthy
wait_for_health() {
    print_header "Waiting for Services to be Healthy"
    
    local max_wait=180
    local wait_time=0
    local interval=10
    
    while [ $wait_time -lt $max_wait ]; do
        log_info "Checking service health (${wait_time}s / ${max_wait}s)..."
        
        local healthy_count=$(ssh "${REMOTE_USER}@${REMOTE_HOST}" "
            docker ps --filter 'name=crm-' --filter 'health=healthy' --format '{{.Names}}' 2>/dev/null | wc -l
        ")
        
        local total_count=$(ssh "${REMOTE_USER}@${REMOTE_HOST}" "
            docker ps --filter 'name=crm-' --format '{{.Names}}' 2>/dev/null | wc -l
        ")
        
        log_info "Healthy: ${healthy_count} / Running: ${total_count}"
        
        if [ "$healthy_count" -ge 8 ]; then
            log_success "All core services are healthy!"
            return 0
        fi
        
        sleep $interval
        wait_time=$((wait_time + interval))
    done
    
    log_warning "Some services may still be starting up"
}

# Show deployment status
show_status() {
    print_header "Deployment Status"
    
    ssh "${REMOTE_USER}@${REMOTE_HOST}" "
        echo '>>> Container Status:'
        docker ps --filter 'name=crm-' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'
        echo ''
        echo '>>> Recent Logs (last 5 lines per service):'
        for container in crm-gateway crm-identity crm-customer crm-frontend; do
            echo \"=== \$container ===\"
            docker logs --tail 3 \$container 2>/dev/null || echo 'Container not found'
        done
    "
    
    echo ""
    log_success "CRM Solution deployed to http://${REMOTE_HOST}"
    echo ""
    echo -e "${CYAN}Access Points:${NC}"
    echo -e "  Frontend:    http://${REMOTE_HOST}"
    echo -e "  API Gateway: http://${REMOTE_HOST}:5000"
    echo -e "  Identity:    http://${REMOTE_HOST}:5001"
    echo -e "  Customer:    http://${REMOTE_HOST}:5002"
    echo -e "  Sales:       http://${REMOTE_HOST}:5003"
    echo -e "  Marketing:   http://${REMOTE_HOST}:5004"
    echo -e "  ServiceDesk: http://${REMOTE_HOST}:5005"
    echo -e "  Core:        http://${REMOTE_HOST}:5006"
}

# Main deployment flow
main() {
    print_header "CRM Microservices Deployment to ${REMOTE_HOST}"
    
    log_info "Local directory: ${LOCAL_DIR}"
    log_info "Remote directory: ${REMOTE_DIR}"
    log_info "Remote user: ${REMOTE_USER}"
    
    check_ssh
    sync_files
    stop_containers
    build_images
    deploy_containers
    wait_for_health
    show_status
    
    print_header "Deployment Complete!"
}

# Run main if not sourced
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
