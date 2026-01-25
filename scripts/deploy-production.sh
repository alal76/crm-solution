#!/bin/bash
#===============================================================================
# CRM Production Deployment Script
# Deploys to 192.168.0.9 with:
#   - Kubernetes: API + Frontend
#   - VM: MariaDB Database
#   - Docker Registry: 192.168.0.9:5000
#===============================================================================

set -e

# Configuration
REMOTE_HOST="192.168.0.9"
REMOTE_USER="${REMOTE_USER:-root}"
SSH_KEY="${SSH_KEY:-$HOME/.ssh/id_ed25519}"
REGISTRY="${REMOTE_HOST}:5000"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
NAMESPACE="crm-production"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

#-------------------------------------------------------------------------------
# SSH Helper
#-------------------------------------------------------------------------------
ssh_cmd() {
    ssh -i "$SSH_KEY" -o StrictHostKeyChecking=no -o ConnectTimeout=10 "${REMOTE_USER}@${REMOTE_HOST}" "$@"
}

scp_cmd() {
    scp -i "$SSH_KEY" -o StrictHostKeyChecking=no "$@"
}

#-------------------------------------------------------------------------------
# Check Prerequisites
#-------------------------------------------------------------------------------
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check SSH key
    if [ ! -f "$SSH_KEY" ]; then
        log_error "SSH key not found: $SSH_KEY"
        log_info "Generate with: ssh-keygen -t rsa -b 4096"
        log_info "Copy to server: ssh-copy-id -i $SSH_KEY ${REMOTE_USER}@${REMOTE_HOST}"
        exit 1
    fi
    
    # Test SSH connection
    log_info "Testing SSH connection to ${REMOTE_HOST}..."
    if ! ssh_cmd "echo 'SSH OK'" 2>/dev/null; then
        log_error "Cannot connect to ${REMOTE_HOST}"
        log_info "Ensure SSH key is authorized on the server"
        exit 1
    fi
    
    # Check Docker on local
    if ! command -v docker &> /dev/null; then
        log_error "Docker not found locally"
        exit 1
    fi
    
    log_success "Prerequisites check passed"
}

#-------------------------------------------------------------------------------
# Setup Remote Server
#-------------------------------------------------------------------------------
setup_remote_server() {
    log_info "Setting up remote server at ${REMOTE_HOST}..."
    
    ssh_cmd << 'REMOTE_SCRIPT'
#!/bin/bash
set -e

# Install Docker if not present
if ! command -v docker &> /dev/null; then
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com | sh
    sudo usermod -aG docker $USER
    sudo systemctl enable docker
    sudo systemctl start docker
fi

# Start Docker Registry if not running
if ! docker ps | grep -q "registry:2"; then
    echo "Starting Docker Registry..."
    docker run -d -p 5000:5000 --restart=always --name registry registry:2 2>/dev/null || \
    docker start registry 2>/dev/null || true
fi

# Install kubectl if not present
if ! command -v kubectl &> /dev/null; then
    echo "Installing kubectl..."
    curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
    sudo install -o root -g root -m 0755 kubectl /usr/local/bin/kubectl
    rm kubectl
fi

# Install k3s if not present (lightweight Kubernetes)
if ! command -v k3s &> /dev/null; then
    echo "Installing k3s (lightweight Kubernetes)..."
    curl -sfL https://get.k3s.io | sh -s - --write-kubeconfig-mode 644
    
    # Wait for k3s to be ready
    sleep 10
    sudo k3s kubectl get nodes
fi

# Ensure kubeconfig is accessible
mkdir -p ~/.kube
sudo cp /etc/rancher/k3s/k3s.yaml ~/.kube/config 2>/dev/null || true
sudo chown $(id -u):$(id -g) ~/.kube/config 2>/dev/null || true

echo "Remote server setup complete!"
REMOTE_SCRIPT
    
    log_success "Remote server setup complete"
}

#-------------------------------------------------------------------------------
# Setup MariaDB on VM
#-------------------------------------------------------------------------------
setup_database() {
    log_info "Setting up MariaDB on ${REMOTE_HOST}..."
    
    ssh_cmd << 'DB_SCRIPT'
#!/bin/bash
set -e

# Check if MariaDB container exists
if docker ps -a | grep -q crm-mariadb; then
    echo "MariaDB container already exists"
    docker start crm-mariadb 2>/dev/null || true
else
    echo "Creating MariaDB container..."
    docker run -d \
        --name crm-mariadb \
        --restart=always \
        -p 3306:3306 \
        -e MARIADB_ROOT_PASSWORD=RootPass@Prod2024 \
        -e MARIADB_DATABASE=crm_db \
        -e MARIADB_USER=crm_user \
        -e MARIADB_PASSWORD=CrmPass@Prod2024 \
        -v crm-mariadb-data:/var/lib/mysql \
        mariadb:10.11
    
    echo "Waiting for MariaDB to start..."
    sleep 15
fi

# Create demo database
docker exec crm-mariadb mariadb -u root -pRootPass@Prod2024 -e "
    CREATE DATABASE IF NOT EXISTS crm_demodb;
    GRANT ALL PRIVILEGES ON crm_demodb.* TO 'crm_user'@'%';
    FLUSH PRIVILEGES;
" 2>/dev/null || echo "Demo database may already exist"

# Test connection
docker exec crm-mariadb mariadb -u crm_user -pCrmPass@Prod2024 -e "SELECT 'Database OK'" 2>/dev/null && \
    echo "MariaDB is ready!" || echo "MariaDB connection test failed"
DB_SCRIPT
    
    log_success "MariaDB setup complete"
}

#-------------------------------------------------------------------------------
# Build and Push Images
#-------------------------------------------------------------------------------
build_and_push_images() {
    log_info "Building and pushing Docker images..."
    
    cd "$PROJECT_DIR"
    
    # Configure Docker to use insecure registry
    if ! grep -q "$REGISTRY" ~/.docker/daemon.json 2>/dev/null; then
        log_info "Configuring Docker for insecure registry..."
        mkdir -p ~/.docker
        echo "{\"insecure-registries\":[\"$REGISTRY\"]}" > ~/.docker/daemon.json
        # Note: Docker daemon restart may be needed
    fi
    
    # Get version from version.json (major.minor.patch format)
    if [[ -f version.json ]]; then
        MAJOR=$(grep -o '"major"[^,]*' version.json | grep -o '[0-9]*' || echo "1")
        MINOR=$(grep -o '"minor"[^,]*' version.json | grep -o '[0-9]*' || echo "0")
        PATCH=$(grep -o '"patch"[^,]*' version.json | grep -o '[0-9]*' || echo "0")
        VERSION="${MAJOR}.${MINOR}.${PATCH}"
    else
        VERSION="1.0.0"
    fi
    TIMESTAMP=$(date +%Y%m%d%H%M%S)
    TAG="${VERSION}-${TIMESTAMP}"
    
    log_info "Building backend image (tag: $TAG)..."
    docker build -f docker/Dockerfile.backend -t ${REGISTRY}/crm-backend:${TAG} -t ${REGISTRY}/crm-backend:latest .
    
    log_info "Building frontend image (tag: $TAG)..."
    docker build -f docker/Dockerfile.frontend -t ${REGISTRY}/crm-frontend:${TAG} -t ${REGISTRY}/crm-frontend:latest .
    
    log_info "Pushing images to registry..."
    docker push ${REGISTRY}/crm-backend:${TAG}
    docker push ${REGISTRY}/crm-backend:latest
    docker push ${REGISTRY}/crm-frontend:${TAG}
    docker push ${REGISTRY}/crm-frontend:latest
    
    log_success "Images built and pushed: $TAG"
}

#-------------------------------------------------------------------------------
# Deploy to Kubernetes
#-------------------------------------------------------------------------------
deploy_kubernetes() {
    log_info "Deploying to Kubernetes on ${REMOTE_HOST}..."
    
    # Copy manifests to remote server
    log_info "Copying Kubernetes manifests..."
    ssh_cmd "mkdir -p ~/crm-deploy"
    scp_cmd -r "${PROJECT_DIR}/kubernetes/production/"* "${REMOTE_USER}@${REMOTE_HOST}:~/crm-deploy/"
    
    # Apply manifests
    ssh_cmd << 'K8S_SCRIPT'
#!/bin/bash
set -e
cd ~/crm-deploy

# Configure kubectl for k3s
export KUBECONFIG=/etc/rancher/k3s/k3s.yaml

# Apply namespace and secrets
echo "Applying namespace and secrets..."
sudo kubectl apply -f 00-namespace-secrets.yaml

# Apply API deployment
echo "Deploying API..."
sudo kubectl apply -f 01-api.yaml

# Apply Frontend deployment
echo "Deploying Frontend..."
sudo kubectl apply -f 02-frontend.yaml

# Wait for deployments
echo "Waiting for deployments to be ready..."
sudo kubectl rollout status deployment/crm-api -n crm-production --timeout=180s || true
sudo kubectl rollout status deployment/crm-frontend -n crm-production --timeout=120s || true

# Show status
echo ""
echo "=== Deployment Status ==="
sudo kubectl get pods -n crm-production
echo ""
sudo kubectl get svc -n crm-production
K8S_SCRIPT
    
    log_success "Kubernetes deployment complete"
}

#-------------------------------------------------------------------------------
# Verify Deployment
#-------------------------------------------------------------------------------
verify_deployment() {
    log_info "Verifying deployment..."
    
    ssh_cmd << 'VERIFY_SCRIPT'
#!/bin/bash

echo "=== Checking Services ==="

# Test API
API_URL="http://localhost:30500/health"
echo -n "API Health: "
curl -s --max-time 10 "$API_URL" || echo "FAILED"

# Test Frontend
FRONTEND_URL="http://localhost:30080"
echo -n "Frontend: "
curl -s --max-time 10 "$FRONTEND_URL" | head -c 100 && echo "... OK" || echo "FAILED"

# Test Database
echo -n "Database: "
docker exec crm-mariadb mariadb -u crm_user -pCrmPass@Prod2024 -e "SELECT 'OK'" 2>/dev/null | tail -1 || echo "FAILED"

echo ""
echo "=== Access URLs ==="
echo "Frontend: http://192.168.0.9:30080"
echo "API:      http://192.168.0.9:30500"
echo "API Health: http://192.168.0.9:30500/health"
VERIFY_SCRIPT
    
    log_success "Verification complete"
}

#-------------------------------------------------------------------------------
# Main
#-------------------------------------------------------------------------------
show_usage() {
    echo "Usage: $0 [command]"
    echo ""
    echo "Commands:"
    echo "  all        - Full deployment (setup + build + deploy)"
    echo "  setup      - Setup remote server (Docker, k3s, registry)"
    echo "  database   - Setup MariaDB on VM"
    echo "  build      - Build and push Docker images"
    echo "  deploy     - Deploy to Kubernetes"
    echo "  verify     - Verify deployment"
    echo "  status     - Show current status"
    echo ""
    echo "Environment variables:"
    echo "  REMOTE_USER - SSH username (default: alal)"
    echo "  SSH_KEY     - Path to SSH key (default: ~/.ssh/id_rsa)"
}

show_status() {
    log_info "Checking deployment status on ${REMOTE_HOST}..."
    ssh_cmd << 'STATUS_SCRIPT'
echo "=== Docker Containers ==="
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "crm|registry" || echo "No CRM containers"

echo ""
echo "=== Kubernetes Pods ==="
sudo kubectl get pods -n crm-production 2>/dev/null || echo "No Kubernetes pods"

echo ""
echo "=== Kubernetes Services ==="
sudo kubectl get svc -n crm-production 2>/dev/null || echo "No Kubernetes services"
STATUS_SCRIPT
}

case "${1:-all}" in
    all)
        check_prerequisites
        setup_remote_server
        setup_database
        build_and_push_images
        deploy_kubernetes
        verify_deployment
        ;;
    setup)
        check_prerequisites
        setup_remote_server
        ;;
    database)
        check_prerequisites
        setup_database
        ;;
    build)
        check_prerequisites
        build_and_push_images
        ;;
    deploy)
        check_prerequisites
        deploy_kubernetes
        ;;
    verify)
        check_prerequisites
        verify_deployment
        ;;
    status)
        check_prerequisites
        show_status
        ;;
    *)
        show_usage
        exit 1
        ;;
esac

echo ""
log_success "Done!"
echo ""
echo "=============================================="
echo -e "${GREEN}CRM Production Deployment${NC}"
echo "=============================================="
echo ""
echo "Access Points:"
echo "  Frontend: http://192.168.0.9:30080"
echo "  API:      http://192.168.0.9:30500"
echo ""
echo "Credentials:"
echo "  Email:    abhi.lal@gmail.com"
echo "  Password: Admin123!"
echo ""
echo "=============================================="
