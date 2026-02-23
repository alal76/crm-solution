#!/bin/bash

# ============================================================================
# CRM Solution - Full Build & Deploy Script
# ============================================================================
# Builds backend, frontend, and Docker images, then deploys to 192.168.0.9
# ============================================================================

set -e  # Exit on any error

# Configuration
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
TARGET_SERVER="192.168.0.9"
DEPLOY_USER="root"
REPO_DIR="/opt/crm-solution"
DOCKER_REGISTRY="localhost"
BACKEND_IMAGE="crm-api:latest"
FRONTEND_IMAGE="crm-frontend:latest"

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
# Build Backend
# ============================================================================

build_backend() {
    log_section "Step 1: Building Backend (.NET)"
    
    cd "$SCRIPT_DIR/CRM.Backend"
    
    log_info "Building CRM.Api in Release mode..."
    dotnet build src/CRM.Api/CRM.Api.csproj -c Release --no-restore
    
    log_success "Backend build completed"
}

# ============================================================================
# Build Frontend
# ============================================================================

build_frontend() {
    log_section "Step 2: Building Frontend (React)"
    
    cd "$SCRIPT_DIR/CRM.Frontend"
    
    log_info "Installing dependencies..."
    npm install --legacy-peer-deps
    
    log_info "Building for production..."
    npm run build
    
    log_success "Frontend build completed"
}

# ============================================================================
# Build Docker Images
# ============================================================================

build_docker_images() {
    log_section "Step 3: Building Docker Images"
    
    cd "$SCRIPT_DIR"
    
    # Check if buildx is available for cross-platform builds
    if command -v docker buildx &> /dev/null; then
        log_info "Using docker buildx for cross-platform build (linux/amd64)..."
        
        # Build Backend Image for Linux amd64
        log_info "Building backend Docker image..."
        docker buildx build \
            --platform linux/amd64 \
            -t "$BACKEND_IMAGE" \
            -f docker/Dockerfile.backend \
            --push=false \
            --output type=docker \
            .
        
        log_success "Backend Docker image built"
        
        # Build Frontend Image for Linux amd64
        log_info "Building frontend Docker image..."
        docker buildx build \
            --platform linux/amd64 \
            -t "$FRONTEND_IMAGE" \
            -f docker/Dockerfile.frontend.prebuilt \
            --push=false \
            --output type=docker \
            .
        
        log_success "Frontend Docker image built"
    else
        log_info "Using standard docker build..."
        
        log_info "Building backend Docker image..."
        docker build -t "$BACKEND_IMAGE" -f docker/Dockerfile.backend .
        log_success "Backend Docker image built"
        
        log_info "Building frontend Docker image..."
        docker build -t "$FRONTEND_IMAGE" -f docker/Dockerfile.frontend.prebuilt .
        log_success "Frontend Docker image built"
    fi
}

# ============================================================================
# Verify Deployment Prerequisites
# ============================================================================

verify_prerequisites() {
    log_section "Step 4: Verifying Deployment Prerequisites"
    
    # Check SSH access to target server
    log_info "Checking SSH access to $TARGET_SERVER..."
    if ssh -o ConnectTimeout=5 "$DEPLOY_USER@$TARGET_SERVER" "echo 'SSH OK'" &>/dev/null; then
        log_success "SSH access verified"
    else
        log_error "Cannot SSH to $TARGET_SERVER"
        echo "Please ensure:"
        echo "  1. Server at $TARGET_SERVER is running"
        echo "  2. SSH key is configured"
        echo "  3. User $DEPLOY_USER has access"
        return 1
    fi
    
    # Check if repository exists on server
    log_info "Checking repository on server..."
    if ssh "$DEPLOY_USER@$TARGET_SERVER" "test -d $REPO_DIR" 2>/dev/null; then
        log_success "Repository directory exists at $REPO_DIR"
    else
        log_error "Repository not found at $REPO_DIR on server"
        echo "Please clone the repository to $REPO_DIR on the server first"
        return 1
    fi
    
    # Check Docker on server
    log_info "Checking Docker on server..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "docker --version" >/dev/null 2>&1
    log_success "Docker is available on server"
}

# ============================================================================
# Push Images to Server
# ============================================================================

push_images_to_server() {
    log_section "Step 5: Pushing Docker Images to Server"
    
    # Get local Docker images
    BACKEND_ID=$(docker images --quiet "$BACKEND_IMAGE" | head -1)
    FRONTEND_ID=$(docker images --quiet "$FRONTEND_IMAGE" | head -1)
    
    if [ -z "$BACKEND_ID" ]; then
        log_error "Backend Docker image not found"
        return 1
    fi
    
    if [ -z "$FRONTEND_ID" ]; then
        log_error "Frontend Docker image not found"
        return 1
    fi
    
    log_info "Saving backend image..."
    docker save "$BACKEND_IMAGE" -o /tmp/crm-api.tar
    
    log_info "Transferring backend image to server..."
    scp /tmp/crm-api.tar "$DEPLOY_USER@$TARGET_SERVER:/tmp/"
    
    log_info "Loading backend image on server..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "docker load -i /tmp/crm-api.tar && rm /tmp/crm-api.tar"
    log_success "Backend image deployed"
    
    log_info "Saving frontend image..."
    docker save "$FRONTEND_IMAGE" -o /tmp/crm-frontend.tar
    
    log_info "Transferring frontend image to server..."
    scp /tmp/crm-frontend.tar "$DEPLOY_USER@$TARGET_SERVER:/tmp/"
    
    log_info "Loading frontend image on server..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "docker load -i /tmp/crm-frontend.tar && rm /tmp/crm-frontend.tar"
    log_success "Frontend image deployed"
    
    # Cleanup local tar files
    rm -f /tmp/crm-api.tar /tmp/crm-frontend.tar
}

# ============================================================================
# Deploy Services
# ============================================================================

deploy_services() {
    log_section "Step 6: Deploying Services to Server"
    
    log_info "Stopping old containers..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR && \
        docker-compose -f docker/docker-compose.yml down || true \
    " 2>/dev/null || true
    
    log_info "Starting new services..."
    ssh "$DEPLOY_USER@$TARGET_SERVER" "\
        cd $REPO_DIR && \
        docker-compose -f docker/docker-compose.yml up -d \
    "
    
    log_success "Services deployed"
}

# ============================================================================
# Verify Deployment
# ============================================================================

verify_deployment() {
    log_section "Step 7: Verifying Deployment"
    
    log_info "Waiting for services to start..."
    sleep 5
    
    log_info "Checking API health..."
    if curl -s "http://$TARGET_SERVER:5000/health" | grep -q "Healthy"; then
        log_success "API is healthy"
    else
        log_error "API health check failed"
        log_info "Running: curl http://$TARGET_SERVER:5000/health"
        curl -v "http://$TARGET_SERVER:5000/health"
        return 1
    fi
    
    log_info "Checking frontend..."
    if curl -s "http://$TARGET_SERVER" | grep -q "html"; then
        log_success "Frontend is responding"
    else
        log_error "Frontend health check failed"
        return 1
    fi
}

# ============================================================================
# Display Summary
# ============================================================================

display_summary() {
    log_section "Deployment Complete"
    
    echo -e "${GREEN}✓ Build completed successfully${NC}"
    echo -e "${GREEN}✓ Docker images created${NC}"
    echo -e "${GREEN}✓ Services deployed to $TARGET_SERVER${NC}"
    echo ""
    echo "Access your deployment:"
    echo -e "  Frontend: ${BLUE}http://$TARGET_SERVER${NC}"
    echo -e "  API:      ${BLUE}http://$TARGET_SERVER:5000${NC}"
    echo -e "  Health:   ${BLUE}http://$TARGET_SERVER:5000/health${NC}"
    echo ""
    echo "Useful commands:"
    echo "  View logs:      docker logs -f crm-api"
    echo "  SSH to server:  ssh root@$TARGET_SERVER"
    echo "  Stop services:  cd $REPO_DIR && docker-compose down"
    echo ""
}

# ============================================================================
# Main Execution
# ============================================================================

main() {
    echo -e "${BLUE}"
    echo '╔════════════════════════════════════════════════════════════════╗'
    echo '║  CRM Solution - Build & Deploy to 192.168.0.9                  ║'
    echo '╚════════════════════════════════════════════════════════════════╝'
    echo -e "${NC}"
    echo "Start Time: $(date)"
    echo ""
    
    # Run build and deployment steps
    build_backend || exit 1
    build_frontend || exit 1
    build_docker_images || exit 1
    verify_prerequisites || exit 1
    push_images_to_server || exit 1
    deploy_services || exit 1
    verify_deployment || exit 1
    display_summary
    
    echo -e "\n${GREEN}Deployment finished at $(date)${NC}\n"
}

# Run main function
main "$@"
