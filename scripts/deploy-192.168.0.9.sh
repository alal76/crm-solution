#!/bin/bash
# =============================================================================
# CRM Solution Deployment Script for 192.168.0.9
# Deploys: Frontend + API on Kubernetes, Databases on Docker
# =============================================================================

set -e

# Configuration
REMOTE_HOST="192.168.0.9"
REMOTE_USER="${REMOTE_USER:-root}"
REMOTE_APP_DIR="/opt/crm"
KUBE_NAMESPACE="crm-app"
IMAGE_REGISTRY="${IMAGE_REGISTRY:-192.168.0.9:5000}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
DATABASE_PROVIDER="${DATABASE_PROVIDER:-sqlserver}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

print_banner() {
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║            CRM Solution Deployment to 192.168.0.9               ║"
    echo "║      Frontend + API: Kubernetes | Databases: Docker            ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check SSH connectivity
    if ! ssh -o ConnectTimeout=5 -o BatchMode=yes ${REMOTE_USER}@${REMOTE_HOST} "echo 'SSH OK'" &>/dev/null; then
        log_error "Cannot connect to ${REMOTE_HOST}. Please check SSH configuration."
        log_info "Hint: Run 'ssh-copy-id ${REMOTE_USER}@${REMOTE_HOST}' to setup SSH keys"
        exit 1
    fi
    log_success "SSH connectivity verified"
    
    # Check Docker on remote
    if ! ssh ${REMOTE_USER}@${REMOTE_HOST} "docker --version" &>/dev/null; then
        log_error "Docker is not installed on ${REMOTE_HOST}"
        exit 1
    fi
    log_success "Docker is available on remote host"
    
    # Check kubectl on remote
    if ! ssh ${REMOTE_USER}@${REMOTE_HOST} "kubectl version --client" &>/dev/null; then
        log_warning "kubectl not found on remote. Installing..."
        install_kubectl_remote
    fi
    log_success "kubectl is available on remote host"
}

install_kubectl_remote() {
    log_info "Installing kubectl on ${REMOTE_HOST}..."
    ssh ${REMOTE_USER}@${REMOTE_HOST} << 'KUBECTL_INSTALL'
        curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
        chmod +x kubectl
        sudo mv kubectl /usr/local/bin/
        kubectl version --client
KUBECTL_INSTALL
}

# Deploy databases using Docker
deploy_databases() {
    log_info "Deploying databases to ${REMOTE_HOST}..."
    
    # Create remote directory structure
    ssh ${REMOTE_USER}@${REMOTE_HOST} "mkdir -p ${REMOTE_APP_DIR}/docker/init-scripts/{mariadb,postgresql,mssql}"
    
    # Copy docker-compose and init scripts
    log_info "Copying Docker configuration files..."
    scp docker/docker-compose.databases.yml ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_APP_DIR}/docker/
    scp docker/init-scripts/mariadb/*.sql ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_APP_DIR}/docker/init-scripts/mariadb/ 2>/dev/null || true
    scp docker/init-scripts/postgresql/*.sql ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_APP_DIR}/docker/init-scripts/postgresql/ 2>/dev/null || true
    scp docker/init-scripts/mssql/*.sql ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_APP_DIR}/docker/init-scripts/mssql/ 2>/dev/null || true
    
    # Create .env file for databases
    log_info "Creating database environment configuration..."
    ssh ${REMOTE_USER}@${REMOTE_HOST} << ENVFILE
cat > ${REMOTE_APP_DIR}/docker/.env << 'EOF'
# MariaDB Configuration
MARIADB_ROOT_PASSWORD=RootPass@Dev2024!
MARIADB_DATABASE=crm_db
MARIADB_USER=crm_user
MARIADB_PASSWORD=CrmPass@Dev2024!

# PostgreSQL Configuration
POSTGRES_USER=crm_user
POSTGRES_PASSWORD=CrmPass@Dev2024!
POSTGRES_DB=crm_db

# SQL Server Configuration
MSSQL_SA_PASSWORD=CrmPass@Dev2024!
EOF
ENVFILE
    
    # Start database containers
    log_info "Starting database containers..."
    ssh ${REMOTE_USER}@${REMOTE_HOST} << DOCKER_UP
        cd ${REMOTE_APP_DIR}/docker
        docker compose -f docker-compose.databases.yml down 2>/dev/null || true
        docker compose -f docker-compose.databases.yml pull
        docker compose -f docker-compose.databases.yml up -d
DOCKER_UP
    
    log_success "Database containers deployed"
    
    # Wait for databases to be ready
    log_info "Waiting for databases to initialize (60 seconds)..."
    sleep 60
    
    # Verify database health
    verify_databases
}

verify_databases() {
    log_info "Verifying database connectivity..."
    
    ssh ${REMOTE_USER}@${REMOTE_HOST} << 'VERIFY_DB'
        echo "=== MariaDB Status ==="
        docker exec crm-mariadb mariadb-admin ping -h localhost -u root -pRootPass@Dev2024! 2>/dev/null && echo "MariaDB: OK" || echo "MariaDB: FAILED"
        
        echo ""
        echo "=== PostgreSQL Status ==="
        docker exec crm-postgresql pg_isready -U crm_user -d crm_db 2>/dev/null && echo "PostgreSQL: OK" || echo "PostgreSQL: FAILED"
        
        echo ""
        echo "=== SQL Server Status ==="
        docker exec crm-mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'CrmPass@Dev2024!' -Q "SELECT 1" 2>/dev/null && echo "SQL Server: OK" || echo "SQL Server: FAILED (may still be starting)"
        
        echo ""
        echo "=== Container Status ==="
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep crm-
VERIFY_DB
    
    log_success "Database verification complete"
}

# Deploy application to Kubernetes
deploy_kubernetes() {
    log_info "Deploying application to Kubernetes..."
    
    # Create remote kubernetes directory
    ssh ${REMOTE_USER}@${REMOTE_HOST} "mkdir -p ${REMOTE_APP_DIR}/kubernetes"
    
    # Copy kubernetes manifests
    log_info "Copying Kubernetes manifests..."
    scp kubernetes/*.yaml ${REMOTE_USER}@${REMOTE_HOST}:${REMOTE_APP_DIR}/kubernetes/
    
    # Update image references in manifests
    log_info "Updating image references..."
    ssh ${REMOTE_USER}@${REMOTE_HOST} << UPDATE_IMAGES
        cd ${REMOTE_APP_DIR}/kubernetes
        
        # Update API image
        sed -i "s|image: your-registry/crm-api:.*|image: ${IMAGE_REGISTRY}/crm-api:${IMAGE_TAG}|g" 02-application-tier.yaml
        
        # Update Frontend image  
        sed -i "s|image: your-registry/crm-frontend:.*|image: ${IMAGE_REGISTRY}/crm-frontend:${IMAGE_TAG}|g" 03-presentation-tier.yaml
        
        # Update database connection to use Docker host
        # The API should connect to the Docker database containers on the same host
        sed -i "s|DB_HOST=.*|DB_HOST=host.docker.internal|g" 02-application-tier.yaml || true
UPDATE_IMAGES
    
    # Apply Kubernetes manifests
    log_info "Applying Kubernetes configuration..."
    ssh ${REMOTE_USER}@${REMOTE_HOST} << KUBE_APPLY
        cd ${REMOTE_APP_DIR}/kubernetes
        
        # Create namespace if not exists
        kubectl create namespace ${KUBE_NAMESPACE} --dry-run=client -o yaml | kubectl apply -f -
        
        # Apply configurations in order
        kubectl apply -f 00-namespace-config.yaml -n ${KUBE_NAMESPACE}
        kubectl apply -f 02-application-tier.yaml -n ${KUBE_NAMESPACE}
        kubectl apply -f 03-presentation-tier.yaml -n ${KUBE_NAMESPACE}
        kubectl apply -f 04-ingress-network.yaml -n ${KUBE_NAMESPACE}
        
        # Wait for deployments
        echo "Waiting for deployments to be ready..."
        kubectl rollout status deployment/crm-api -n ${KUBE_NAMESPACE} --timeout=120s || true
        kubectl rollout status deployment/crm-frontend -n ${KUBE_NAMESPACE} --timeout=120s || true
KUBE_APPLY
    
    log_success "Kubernetes deployment complete"
}

# Build and push Docker images
build_and_push_images() {
    log_info "Building and pushing Docker images..."
    
    # Build backend image
    log_info "Building backend image..."
    docker build -f Dockerfile.backend -t ${IMAGE_REGISTRY}/crm-api:${IMAGE_TAG} .
    
    # Build frontend image
    log_info "Building frontend image..."
    docker build -f Dockerfile.frontend -t ${IMAGE_REGISTRY}/crm-frontend:${IMAGE_TAG} .
    
    # Push images
    log_info "Pushing images to registry..."
    docker push ${IMAGE_REGISTRY}/crm-api:${IMAGE_TAG}
    docker push ${IMAGE_REGISTRY}/crm-frontend:${IMAGE_TAG}
    
    log_success "Images built and pushed"
}

# Test deployment
test_deployment() {
    log_info "Testing deployment..."
    
    ssh ${REMOTE_USER}@${REMOTE_HOST} << TEST_DEPLOY
        echo "=== Kubernetes Pod Status ==="
        kubectl get pods -n ${KUBE_NAMESPACE} -o wide
        
        echo ""
        echo "=== Kubernetes Services ==="
        kubectl get services -n ${KUBE_NAMESPACE}
        
        echo ""
        echo "=== Testing API Health ==="
        API_POD=\$(kubectl get pods -n ${KUBE_NAMESPACE} -l app=crm,tier=application -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
        if [ -n "\$API_POD" ]; then
            kubectl exec -n ${KUBE_NAMESPACE} \$API_POD -- curl -s http://localhost:5000/health 2>/dev/null || echo "Health check pending..."
        fi
        
        echo ""
        echo "=== Database Containers ==="
        docker ps --filter "name=crm-" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
TEST_DEPLOY
    
    log_success "Deployment test complete"
}

# Initialize and seed databases
seed_databases() {
    log_info "Initializing and seeding databases..."
    
    ssh ${REMOTE_USER}@${REMOTE_HOST} << SEED_DB
        echo "=== Seeding MariaDB ==="
        docker exec crm-mariadb mysql -u root -pRootPass@Dev2024! -e "
            USE crm_db;
            -- Seed data will be handled by the API on first run
            SELECT 'MariaDB ready for seeding via API' AS Status;
        " 2>/dev/null || echo "MariaDB seeding skipped"
        
        echo ""
        echo "=== Seeding PostgreSQL ==="
        docker exec crm-postgresql psql -U crm_user -d crm_db -c "
            -- Seed data will be handled by the API on first run
            SELECT 'PostgreSQL ready for seeding via API' AS Status;
        " 2>/dev/null || echo "PostgreSQL seeding skipped"
        
        echo ""
        echo "Database seeding will be completed automatically when the API starts"
SEED_DB
    
    log_success "Database initialization complete"
}

# Print deployment summary
print_summary() {
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║                    DEPLOYMENT SUMMARY                           ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
    echo "  Target Host:     ${REMOTE_HOST}"
    echo ""
    echo "  📦 DATABASES (Docker)"
    echo "  ─────────────────────"
    echo "  • MariaDB:      ${REMOTE_HOST}:3306"
    echo "  • PostgreSQL:   ${REMOTE_HOST}:5432"
    echo "  • SQL Server:   ${REMOTE_HOST}:1433"
    echo "  • Adminer UI:   http://${REMOTE_HOST}:8080"
    echo ""
    echo "  🚀 APPLICATION (Kubernetes)"
    echo "  ───────────────────────────"
    echo "  • API:          http://${REMOTE_HOST}:5000"
    echo "  • Frontend:     http://${REMOTE_HOST}:80"
    echo ""
    echo "  📋 DATABASE CREDENTIALS"
    echo "  ────────────────────────"
    echo "  • MariaDB User:     crm_user / CrmPass@Dev2024!"
    echo "  • PostgreSQL User:  crm_user / CrmPass@Dev2024!"
    echo "  • SQL Server SA:    sa / CrmPass@Dev2024!"
    echo ""
    echo "  ✅ Deployment completed successfully!"
    echo ""
}

# Main function
main() {
    print_banner
    
    case "${1:-all}" in
        databases)
            check_prerequisites
            deploy_databases
            seed_databases
            ;;
        kubernetes)
            check_prerequisites
            deploy_kubernetes
            test_deployment
            ;;
        build)
            build_and_push_images
            ;;
        test)
            check_prerequisites
            test_deployment
            ;;
        verify)
            check_prerequisites
            verify_databases
            ;;
        all|"")
            check_prerequisites
            deploy_databases
            seed_databases
            deploy_kubernetes
            test_deployment
            print_summary
            ;;
        *)
            echo "Usage: $0 [databases|kubernetes|build|test|verify|all]"
            echo ""
            echo "  databases  - Deploy only database containers"
            echo "  kubernetes - Deploy only Kubernetes application"
            echo "  build      - Build and push Docker images"
            echo "  test       - Test the deployment"
            echo "  verify     - Verify database connectivity"
            echo "  all        - Full deployment (default)"
            exit 1
            ;;
    esac
}

# Run main function with arguments
main "$@"
