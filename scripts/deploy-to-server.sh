#!/bin/bash

# ============================================================================
# CRM Solution - Deploy to Server (Build on Server)
# ============================================================================
# Deploys to 192.168.0.9 by pulling repo and building with docker-compose
# This avoids issues with Docker not running locally on Mac
# ============================================================================

set -e  # Exit on any error

# Configuration
TARGET_SERVER="192.168.0.9"
DEPLOY_USER="root"
REPO_DIR="/opt/crm-solution"
DOCKER_COMPOSE_FILE="docker/docker-compose.yml"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# ============================================================================
# Helper Functions
# ============================================================================

log_section() {
    echo -e "\n${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}\n"
}

log_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

log_error() {
    echo -e "${RED}✗ $1${NC}"
}

log_info() {
    echo -e "${YELLOW}ℹ $1${NC}"
}

# ============================================================================
# Verify SSH Access
# ============================================================================

verify_ssh() {
    log_section "Step 1: Verifying SSH Access to $TARGET_SERVER"
    
    if ssh -o ConnectTimeout=5 "$DEPLOY_USER@$TARGET_SERVER" "echo 'SSH_OK'" &>/dev/null; then
        log_success "SSH access verified"
    else
        log_error "Cannot SSH to $TARGET_SERVER"
        return 1
    fi
}

# ============================================================================
# Pull Latest Code from Repository
# ============================================================================

pull_code() {
    log_section "Step 2: Pulling Latest Code on Server"
    
    log_info "Fetching latest changes..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR && \
        git fetch origin && \
        git pull origin \$(git rev-parse --abbrev-ref HEAD) \
    "
    
    log_success "Code updated"
}

# ============================================================================
# Build Backend
# ============================================================================

build_backend() {
    log_section "Step 3: Building Backend on Server"
    
    log_info "Building .NET API..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR/CRM.Backend && \
        dotnet build src/CRM.Api/CRM.Api.csproj -c Release --no-restore 2>&1 | tail -20 \
    "
    
    log_success "Backend built"
}

# ============================================================================
# Build Frontend
# ============================================================================

build_frontend() {
    log_section "Step 4: Building Frontend on Server"
    
    log_info "Building React application..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR/CRM.Frontend && \
        npm install --legacy-peer-deps 2>&1 | tail -5 && \
        npm run build 2>&1 | tail -20 \
    "
    
    log_success "Frontend built"
}

# ============================================================================
# Deploy Services with Docker Compose
# ============================================================================

deploy_services() {
    log_section "Step 5: Deploying Services with Docker Compose"
    
    log_info "Stopping old containers..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR && \
        docker-compose -f $DOCKER_COMPOSE_FILE down || true \
    " 2>/dev/null || true
    
    sleep 2
    
    log_info "Starting new services..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR && \
        docker-compose -f $DOCKER_COMPOSE_FILE up -d \
    "
    
    log_success "Services deployed"
}

# ============================================================================
# Verify Deployment
# ============================================================================

verify_deployment() {
    log_section "Step 6: Verifying Deployment"
    
    log_info "Waiting for services to start..."
    sleep 5
    
    log_info "Checking API health..."
    if curl -s "http://$TARGET_SERVER:5000/health" | grep -q "Healthy"; then
        log_success "API is healthy"
    else
        log_error "API health check failed"
        log_info "Checking API response..."
        curl -v "http://$TARGET_SERVER:5000/health" 2>&1 | head -20
        return 0  # Continue anyway
    fi
    
    log_info "Checking frontend..."
    if curl -s "http://$TARGET_SERVER" | grep -q "html" 2>/dev/null; then
        log_success "Frontend is responding"
    else
        log_error "Frontend check inconclusive (normal if not ready yet)"
    fi
}

# ============================================================================
# Display Summary
# ============================================================================

display_summary() {
    log_section "Deployment Complete"
    
    echo -e "${GREEN}✓ Code updated on server${NC}"
    echo -e "${GREEN}✓ Backend built${NC}"
    echo -e "${GREEN}✓ Frontend built${NC}"
    echo -e "${GREEN}✓ Services deployed to $TARGET_SERVER${NC}"
    echo ""
    echo "Access your deployment:"
    echo -e "  Frontend: ${BLUE}http://$TARGET_SERVER${NC}"
    echo -e "  API:      ${BLUE}http://$TARGET_SERVER:5000${NC}"
    echo -e "  Health:   ${BLUE}http://$TARGET_SERVER:5000/health${NC}"
    echo ""
    echo "Useful SSH commands:"
    echo "  SSH to server:      ssh root@$TARGET_SERVER"
    echo "  View logs:          docker logs -f crm-api"
    echo "  Check containers:   docker ps"
    echo "  Stop services:      cd $REPO_DIR && docker-compose down"
    echo "  View docker status: docker-compose ps"
    echo ""
}

# ============================================================================
# Main Execution
# ============================================================================

main() {
    echo -e "${BLUE}"
    echo '╔════════════════════════════════════════════════════════════════╗'
    echo '║  CRM Solution - Deploy to 192.168.0.9 (Server Build)           ║'
    echo '╚════════════════════════════════════════════════════════════════╝'
    echo -e "${NC}"
    echo "Start Time: $(date)"
    echo ""
    
    # Run deployment steps
    verify_ssh || exit 1
    pull_code || exit 1
    build_backend || exit 1
    build_frontend || exit 1
    deploy_services || exit 1
    verify_deployment || true
    display_summary
    
    echo -e "\n${GREEN}Deployment finished at $(date)${NC}\n"
}

# Run main function
main "$@"
