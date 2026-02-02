#!/bin/bash
# =============================================================================
# CRM Solution - Parameterized Deployment Script
# =============================================================================
# Universal deployment script that adapts to different platforms and environments
# Supports: Docker Compose, Kubernetes, Cloud (AWS/Azure/GCP)
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# =============================================================================
# Color output
# =============================================================================
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# =============================================================================
# Load Configuration
# =============================================================================
load_config() {
    log_info "Loading deployment configuration..."
    
    # Load environment file safely
    if [ -f "$PROJECT_ROOT/.env" ]; then
        set -a  # Enable auto-export of variables
        source "$PROJECT_ROOT/.env"
        set +a  # Disable auto-export
    fi
    
    # Set defaults
    export ARCHITECTURE_MODE="${ARCHITECTURE_MODE:-monolithic}"
    export DEPLOY_PLATFORM="${DEPLOY_PLATFORM:-docker}"
    export TARGET_ENV="${TARGET_ENV:-development}"
    export CLOUD_PROVIDER="${CLOUD_PROVIDER:-none}"
    
    log_success "Configuration loaded successfully"
}

# =============================================================================
# Parse Arguments
# =============================================================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --platform)
                export DEPLOY_PLATFORM="$2"
                shift 2
                ;;
            --arch)
                export ARCHITECTURE_MODE="$2"
                shift 2
                ;;
            --env)
                export TARGET_ENV="$2"
                shift 2
                ;;
            --cloud)
                export CLOUD_PROVIDER="$2"
                shift 2
                ;;
            --location)
                export DEPLOY_LOCATION="$2"
                shift 2
                ;;
            --domain)
                export DEPLOY_DOMAIN="$2"
                shift 2
                ;;
            --namespace)
                export K8S_NAMESPACE="$2"
                shift 2
                ;;
            --dry-run)
                export DRY_RUN=true
                shift
                ;;
            --help|-h)
                show_help
                exit 0
                ;;
            *)
                log_error "Unknown option: $1"
                show_help
                exit 1
                ;;
        esac
    done
}

show_help() {
    cat << EOF
Usage: $0 [OPTIONS]

Parameterized deployment script for CRM Solution

OPTIONS:
    --platform PLATFORM        Deployment platform (docker|kubernetes|vm)
    --arch ARCHITECTURE        Architecture mode (monolithic|microservices)
    --env ENVIRONMENT          Target environment (development|staging|production)
    --cloud PROVIDER           Cloud provider (none|aws|azure|gcp)
    --location LOCATION        Deployment location (IP/hostname/region)
    --domain DOMAIN            Domain name for the deployment
    --namespace NAMESPACE      Kubernetes namespace (for k8s deployments)
    --dry-run                  Show what would be deployed without executing
    --help, -h                 Show this help message

EXAMPLES:
    # Deploy monolithic to local Docker
    $0 --platform docker --arch monolithic --env development
    
    # Deploy microservices to Kubernetes
    $0 --platform kubernetes --arch microservices --env production \\
       --namespace crm-prod --domain crm.company.com
    
    # Deploy to AWS EKS
    $0 --platform kubernetes --cloud aws --env production \\
       --location us-east-1 --namespace crm-prod
    
    # Dry run to see what would be deployed
    $0 --platform docker --arch microservices --dry-run

ENVIRONMENT VARIABLES:
    See .env.example for all available configuration options

EOF
}

# =============================================================================
# Deployment Functions
# =============================================================================

deploy_docker_compose() {
    log_info "Deploying with Docker Compose..."
    cd "$PROJECT_ROOT"
    
    # Select compose file based on architecture
    if [ "$ARCHITECTURE_MODE" == "microservices" ]; then
        COMPOSE_FILE="docker-compose.microservices.yml"
    else
        COMPOSE_FILE="docker/docker-compose.unified.yml"
    fi
    
    if [ ! -f "$COMPOSE_FILE" ]; then
        log_error "Compose file not found: $COMPOSE_FILE"
        exit 1
    fi
    
    log_info "Using compose file: $COMPOSE_FILE"
    
    if [ "$DRY_RUN" == "true" ]; then
        log_info "[DRY RUN] Would execute: docker compose -f $COMPOSE_FILE up -d"
        docker compose -f "$COMPOSE_FILE" config
        return 0
    fi
    
    # Pull latest images
    log_info "Pulling latest images..."
    docker compose -f "$COMPOSE_FILE" pull || log_warn "Some images could not be pulled"
    
    # Deploy
    log_info "Starting containers..."
    docker compose -f "$COMPOSE_FILE" up -d
    
    # Wait for health checks
    log_info "Waiting for services to be healthy..."
    sleep 10
    
    # Check container status
    docker compose -f "$COMPOSE_FILE" ps
    
    log_success "Docker deployment completed"
}

deploy_kubernetes() {
    log_info "Deploying to Kubernetes..."
    
    # Check if kubectl is available
    if ! command -v kubectl &> /dev/null; then
        log_error "kubectl is not installed"
        exit 1
    fi
    
    # Set namespace
    K8S_NAMESPACE="${K8S_NAMESPACE:-crm-${TARGET_ENV}}"
    log_info "Target namespace: $K8S_NAMESPACE"
    
    # Create namespace if doesn't exist
    if [ "$DRY_RUN" != "true" ]; then
        kubectl create namespace "$K8S_NAMESPACE" --dry-run=client -o yaml | kubectl apply -f -
    fi
    
    # Select manifests based on architecture
    if [ "$ARCHITECTURE_MODE" == "microservices" ]; then
        MANIFESTS_DIR="$PROJECT_ROOT/kubernetes/microservices"
    else
        MANIFESTS_DIR="$PROJECT_ROOT/kubernetes"
    fi
    
    if [ ! -d "$MANIFESTS_DIR" ]; then
        log_error "Kubernetes manifests directory not found: $MANIFESTS_DIR"
        exit 1
    fi
    
    log_info "Using manifests from: $MANIFESTS_DIR"
    
    if [ "$DRY_RUN" == "true" ]; then
        log_info "[DRY RUN] Would apply manifests from $MANIFESTS_DIR to namespace $K8S_NAMESPACE"
        kubectl apply -f "$MANIFESTS_DIR" --dry-run=client --recursive
        return 0
    fi
    
    # Apply ConfigMaps and Secrets first
    if [ -f "$MANIFESTS_DIR/00-configmap.yaml" ]; then
        log_info "Applying ConfigMaps and Secrets..."
        kubectl apply -f "$MANIFESTS_DIR/00-configmap.yaml" -n "$K8S_NAMESPACE"
    fi
    
    # Apply remaining manifests
    log_info "Applying Kubernetes manifests..."
    kubectl apply -f "$MANIFESTS_DIR" -n "$K8S_NAMESPACE" --recursive
    
    # Wait for rollout
    log_info "Waiting for rollout to complete..."
    kubectl rollout status deployment -n "$K8S_NAMESPACE" --timeout=5m || log_warn "Some deployments did not complete in time"
    
    # Show pod status
    log_info "Pod status:"
    kubectl get pods -n "$K8S_NAMESPACE"
    
    log_success "Kubernetes deployment completed"
}

deploy_cloud_aws() {
    log_info "Deploying to AWS..."
    
    # Check AWS CLI
    if ! command -v aws &> /dev/null; then
        log_error "AWS CLI is not installed"
        exit 1
    fi
    
    AWS_REGION="${AWS_REGION:-us-east-1}"
    EKS_CLUSTER="${AWS_EKS_CLUSTER:-crm-cluster}"
    
    log_info "AWS Region: $AWS_REGION"
    log_info "EKS Cluster: $EKS_CLUSTER"
    
    if [ "$DRY_RUN" == "true" ]; then
        log_info "[DRY RUN] Would deploy to AWS EKS cluster: $EKS_CLUSTER in region: $AWS_REGION"
        return 0
    fi
    
    # Update kubeconfig
    log_info "Updating kubeconfig for EKS..."
    aws eks update-kubeconfig --name "$EKS_CLUSTER" --region "$AWS_REGION"
    
    # Deploy using Kubernetes
    deploy_kubernetes
}

deploy_cloud_azure() {
    log_info "Deploying to Azure..."
    
    # Check Azure CLI
    if ! command -v az &> /dev/null; then
        log_error "Azure CLI is not installed"
        exit 1
    fi
    
    AKS_CLUSTER="${AZURE_AKS_CLUSTER:-crm-cluster}"
    RESOURCE_GROUP="${AZURE_RESOURCE_GROUP:-crm-solution-rg}"
    
    log_info "AKS Cluster: $AKS_CLUSTER"
    log_info "Resource Group: $RESOURCE_GROUP"
    
    if [ "$DRY_RUN" == "true" ]; then
        log_info "[DRY RUN] Would deploy to Azure AKS cluster: $AKS_CLUSTER in resource group: $RESOURCE_GROUP"
        return 0
    fi
    
    # Get AKS credentials
    log_info "Getting AKS credentials..."
    az aks get-credentials --name "$AKS_CLUSTER" --resource-group "$RESOURCE_GROUP"
    
    # Deploy using Kubernetes
    deploy_kubernetes
}

deploy_cloud_gcp() {
    log_info "Deploying to Google Cloud..."
    
    # Check gcloud CLI
    if ! command -v gcloud &> /dev/null; then
        log_error "gcloud CLI is not installed"
        exit 1
    fi
    
    GKE_CLUSTER="${GCP_GKE_CLUSTER:-crm-cluster}"
    GCP_REGION="${GCP_REGION:-us-central1}"
    GCP_PROJECT="${GCP_PROJECT_ID}"
    
    log_info "GKE Cluster: $GKE_CLUSTER"
    log_info "Region: $GCP_REGION"
    log_info "Project: $GCP_PROJECT"
    
    if [ "$DRY_RUN" == "true" ]; then
        log_info "[DRY RUN] Would deploy to GCP GKE cluster: $GKE_CLUSTER"
        return 0
    fi
    
    # Get GKE credentials
    log_info "Getting GKE credentials..."
    gcloud container clusters get-credentials "$GKE_CLUSTER" --region "$GCP_REGION" --project "$GCP_PROJECT"
    
    # Deploy using Kubernetes
    deploy_kubernetes
}

run_health_checks() {
    log_info "Running health checks..."
    
    # Determine API endpoint
    if [ "$DEPLOY_PLATFORM" == "docker" ]; then
        API_ENDPOINT="http://localhost:${API_PORT:-5000}"
    elif [ -n "$DEPLOY_DOMAIN" ]; then
        API_ENDPOINT="https://${DEPLOY_DOMAIN}"
    else
        API_ENDPOINT="http://localhost:${API_PORT:-5000}"
    fi
    
    log_info "Checking API health: $API_ENDPOINT/health"
    
    # Retry health check
    for i in {1..10}; do
        if curl -f -s "$API_ENDPOINT/health" > /dev/null 2>&1; then
            log_success "Health check passed"
            return 0
        fi
        log_info "Attempt $i/10: Waiting for API to be ready..."
        sleep 5
    done
    
    log_warn "Health check failed after 10 attempts"
    return 1
}

generate_deployment_summary() {
    cat << EOF

${GREEN}═════════════════════════════════════════════════════════════════${NC}
${GREEN}                 DEPLOYMENT SUMMARY                              ${NC}
${GREEN}═════════════════════════════════════════════════════════════════${NC}

Platform:         ${DEPLOY_PLATFORM}
Architecture:     ${ARCHITECTURE_MODE}
Environment:      ${TARGET_ENV}
Cloud Provider:   ${CLOUD_PROVIDER}
$([ -n "$DEPLOY_LOCATION" ] && echo "Location:         $DEPLOY_LOCATION")
$([ -n "$DEPLOY_DOMAIN" ] && echo "Domain:           https://$DEPLOY_DOMAIN")
$([ -n "$K8S_NAMESPACE" ] && echo "Namespace:        $K8S_NAMESPACE")
$([ "$DRY_RUN" == "true" ] && echo "Mode:             DRY RUN")

Access URLs:
$(if [ "$DEPLOY_PLATFORM" == "docker" ]; then
    echo "  Frontend:       http://localhost:${FRONTEND_PORT:-3000}"
    echo "  API:            http://localhost:${API_PORT:-5000}"
    echo "  API Health:     http://localhost:${API_PORT:-5000}/health"
elif [ -n "$DEPLOY_DOMAIN" ]; then
    echo "  Frontend:       https://$DEPLOY_DOMAIN"
    echo "  API:            https://$DEPLOY_DOMAIN/api"
    echo "  API Health:     https://$DEPLOY_DOMAIN/api/health"
fi)

${GREEN}═════════════════════════════════════════════════════════════════${NC}

EOF
}

# =============================================================================
# Main Execution
# =============================================================================
main() {
    log_info "CRM Solution - Parameterized Deployment Script"
    log_info "==============================================="
    
    # Load configuration
    load_config
    
    # Parse arguments
    parse_args "$@"
    
    # Validate configuration
    if [ "$DEPLOY_PLATFORM" != "docker" ] && [ "$DEPLOY_PLATFORM" != "kubernetes" ] && [ "$DEPLOY_PLATFORM" != "vm" ]; then
        log_error "Invalid platform: $DEPLOY_PLATFORM"
        show_help
        exit 1
    fi
    
    # Execute deployment
    START_TIME=$(date +%s)
    
    case "$DEPLOY_PLATFORM" in
        docker)
            deploy_docker_compose
            ;;
        kubernetes)
            if [ "$CLOUD_PROVIDER" == "aws" ]; then
                deploy_cloud_aws
            elif [ "$CLOUD_PROVIDER" == "azure" ]; then
                deploy_cloud_azure
            elif [ "$CLOUD_PROVIDER" == "gcp" ]; then
                deploy_cloud_gcp
            else
                deploy_kubernetes
            fi
            ;;
        vm)
            log_error "VM deployment not yet implemented"
            exit 1
            ;;
    esac
    
    # Run health checks (skip in dry run)
    if [ "$DRY_RUN" != "true" ]; then
        run_health_checks || log_warn "Health checks incomplete"
    fi
    
    END_TIME=$(date +%s)
    DURATION=$((END_TIME - START_TIME))
    
    # Generate summary
    generate_deployment_summary
    
    log_success "Deployment completed in ${DURATION} seconds"
}

# Run main function
main "$@"
