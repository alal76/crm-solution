#!/bin/bash
# ============================================================================
# CRM Microservices Build Script
# Build individual services or all services
# ============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$SCRIPT_DIR"
BACKEND_DIR="$PROJECT_ROOT/CRM.Backend"
DOCKER_DIR="$PROJECT_ROOT/docker"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Service configuration
declare -A SERVICES=(
    ["gateway"]="CRM.Gateway:5000"
    ["identity"]="CRM.Identity:5001"
    ["customer"]="CRM.CustomerService:5002"
    ["sales"]="CRM.SalesService:5003"
    ["marketing"]="CRM.MarketingService:5004"
    ["servicedesk"]="CRM.ServiceDeskService:5005"
    ["core"]="CRM.CoreService:5006"
)

# Version tracking
VERSION=${VERSION:-v1}

print_header() {
    echo ""
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}============================================${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

usage() {
    echo "Usage: $0 [command] [service] [options]"
    echo ""
    echo "Commands:"
    echo "  build       Build a specific service or all services"
    echo "  docker      Build Docker image for a service"
    echo "  deploy      Deploy a service to Kubernetes"
    echo "  all         Build, dockerize, and deploy all services"
    echo "  list        List all available services"
    echo ""
    echo "Services:"
    echo "  gateway     API Gateway (YARP-based routing)"
    echo "  identity    Identity Service (auth, users)"
    echo "  customer    Customer Service (customers, contacts)"
    echo "  sales       Sales Service (opportunities, quotes)"
    echo "  marketing   Marketing Service (campaigns)"
    echo "  servicedesk Service Desk (tickets, tasks, workflows)"
    echo "  core        Core Service (products, lookups, settings)"
    echo "  frontend    Frontend React application"
    echo "  all         All services"
    echo ""
    echo "Options:"
    echo "  --version   Version tag for Docker images (default: v1)"
    echo "  --no-cache  Build Docker images without cache"
    echo ""
    echo "Examples:"
    echo "  $0 build gateway          # Build gateway service"
    echo "  $0 docker identity        # Build Docker image for identity"
    echo "  $0 deploy customer        # Deploy customer service to K8s"
    echo "  $0 all --version v2       # Build and deploy all with version v2"
}

build_service() {
    local service=$1
    local service_info=${SERVICES[$service]}
    
    if [ -z "$service_info" ]; then
        print_error "Unknown service: $service"
        return 1
    fi
    
    local project_name=$(echo $service_info | cut -d':' -f1)
    local project_path="$BACKEND_DIR/src/Services/$project_name/$project_name.csproj"
    
    print_header "Building $service service ($project_name)"
    
    if [ ! -f "$project_path" ]; then
        print_error "Project file not found: $project_path"
        return 1
    fi
    
    cd "$BACKEND_DIR"
    dotnet build "$project_path" -c Release
    
    print_success "$service service built successfully"
}

build_docker() {
    local service=$1
    local no_cache=${2:-false}
    
    print_header "Building Docker image: crm-$service:$VERSION"
    
    local dockerfile="$DOCKER_DIR/Dockerfile.$service"
    
    if [ ! -f "$dockerfile" ]; then
        print_error "Dockerfile not found: $dockerfile"
        return 1
    fi
    
    cd "$PROJECT_ROOT"
    
    local cache_flag=""
    if [ "$no_cache" = true ]; then
        cache_flag="--no-cache"
    fi
    
    # Use minikube's Docker daemon
    eval $(minikube docker-env 2>/dev/null) || true
    
    docker build $cache_flag -t crm-$service:$VERSION -f "$dockerfile" .
    
    print_success "Docker image crm-$service:$VERSION built successfully"
}

deploy_service() {
    local service=$1
    
    print_header "Deploying $service to Kubernetes"
    
    local manifest=""
    case $service in
        gateway) manifest="02-gateway.yaml" ;;
        identity) manifest="03-identity.yaml" ;;
        customer) manifest="04-customer.yaml" ;;
        sales) manifest="05-sales.yaml" ;;
        marketing) manifest="06-marketing.yaml" ;;
        servicedesk) manifest="07-servicedesk.yaml" ;;
        core) manifest="08-core.yaml" ;;
        frontend) manifest="09-frontend.yaml" ;;
        *) print_error "Unknown service: $service"; return 1 ;;
    esac
    
    local manifest_path="$PROJECT_ROOT/kubernetes/microservices/$manifest"
    
    if [ ! -f "$manifest_path" ]; then
        print_error "Manifest not found: $manifest_path"
        return 1
    fi
    
    # Apply namespace and config first if not exists
    kubectl apply -f "$PROJECT_ROOT/kubernetes/microservices/00-namespace.yaml" 2>/dev/null || true
    kubectl apply -f "$PROJECT_ROOT/kubernetes/microservices/01-database.yaml" 2>/dev/null || true
    
    # Update image version in deployment
    kubectl apply -f "$manifest_path"
    kubectl set image deployment/crm-$service crm-$service=crm-$service:$VERSION -n crm-microservices 2>/dev/null || \
    kubectl set image deployment/crm-$service $service=crm-$service:$VERSION -n crm-microservices 2>/dev/null || true
    
    kubectl rollout status deployment/crm-$service -n crm-microservices --timeout=120s
    
    print_success "$service deployed successfully"
}

build_all() {
    print_header "Building All Microservices"
    
    # Build shared libraries first
    cd "$BACKEND_DIR"
    dotnet build src/CRM.Core/CRM.Core.csproj -c Release
    dotnet build src/CRM.Infrastructure/CRM.Infrastructure.csproj -c Release
    dotnet build src/Services/CRM.ServiceDefaults/CRM.ServiceDefaults.csproj -c Release
    
    # Build all services
    for service in "${!SERVICES[@]}"; do
        build_service "$service"
    done
    
    print_success "All services built successfully"
}

docker_all() {
    local no_cache=${1:-false}
    
    print_header "Building All Docker Images (version: $VERSION)"
    
    for service in "${!SERVICES[@]}"; do
        build_docker "$service" "$no_cache"
    done
    
    # Build frontend
    build_docker "frontend" "$no_cache" 2>/dev/null || print_warning "Frontend Dockerfile not found"
    
    print_success "All Docker images built successfully"
}

deploy_all() {
    print_header "Deploying All Services to Kubernetes"
    
    # Apply namespace and config
    kubectl apply -f "$PROJECT_ROOT/kubernetes/microservices/00-namespace.yaml"
    kubectl apply -f "$PROJECT_ROOT/kubernetes/microservices/01-database.yaml"
    
    # Wait for database
    echo "Waiting for database to be ready..."
    kubectl wait --for=condition=ready pod -l app=crm-db -n crm-microservices --timeout=120s 2>/dev/null || true
    sleep 10
    
    # Deploy all services
    for service in gateway identity customer sales marketing servicedesk core frontend; do
        deploy_service "$service" || print_warning "Failed to deploy $service"
    done
    
    print_success "All services deployed"
    
    echo ""
    echo "Services running in crm-microservices namespace:"
    kubectl get pods -n crm-microservices
}

list_services() {
    echo ""
    echo "Available Services:"
    echo "==================="
    for service in "${!SERVICES[@]}"; do
        local info=${SERVICES[$service]}
        local name=$(echo $info | cut -d':' -f1)
        local port=$(echo $info | cut -d':' -f2)
        printf "  %-12s -> %s (port %s)\n" "$service" "$name" "$port"
    done
    echo ""
}

# Main entry point
main() {
    local command=${1:-help}
    local service=${2:-all}
    local no_cache=false
    
    # Parse options
    for arg in "$@"; do
        case $arg in
            --version=*)
                VERSION="${arg#*=}"
                ;;
            --version)
                VERSION="${3:-v1}"
                ;;
            --no-cache)
                no_cache=true
                ;;
        esac
    done
    
    case $command in
        build)
            if [ "$service" = "all" ]; then
                build_all
            else
                build_service "$service"
            fi
            ;;
        docker)
            if [ "$service" = "all" ]; then
                docker_all "$no_cache"
            else
                build_docker "$service" "$no_cache"
            fi
            ;;
        deploy)
            if [ "$service" = "all" ]; then
                deploy_all
            else
                deploy_service "$service"
            fi
            ;;
        all)
            build_all
            docker_all "$no_cache"
            deploy_all
            ;;
        list)
            list_services
            ;;
        help|--help|-h|*)
            usage
            ;;
    esac
}

main "$@"
