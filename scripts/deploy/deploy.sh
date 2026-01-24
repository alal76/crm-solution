#!/bin/bash

# CRM Application Kubernetes Deployment Script
# This script deploys the entire CRM application to a Kubernetes cluster

set -e

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
NAMESPACE="crm-app"
REGISTRY="${DOCKER_REGISTRY:-}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
ENVIRONMENT="${ENVIRONMENT:-production}"

# Functions
print_status() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

print_error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" >&2
}

print_warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

prompt_registry() {
    if [ -z "$REGISTRY" ]; then
        echo ""
        echo -e "${YELLOW}Docker Registry Configuration${NC}"
        echo "Please enter your Docker registry address (examples: docker.io/username, ghcr.io/username, myregistry.azurecr.io)"
        read -p "Docker Registry Address: " REGISTRY
        
        if [ -z "$REGISTRY" ]; then
            print_error "Docker registry address is required"
            exit 1
        fi
        
        print_status "Using registry: $REGISTRY"
    fi
}

check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        print_error "kubectl is not installed"
        exit 1
    fi
    
    # Check cluster connectivity
    if ! kubectl cluster-info &> /dev/null; then
        print_error "Cannot connect to Kubernetes cluster"
        exit 1
    fi
    
    # Check metrics-server for HPA
    if ! kubectl get deployment metrics-server -n kube-system &> /dev/null; then
        print_warning "Metrics Server not found. HPA may not work. Install with: kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml"
    fi
    
    print_status "Prerequisites check passed"
}

create_namespace() {
    print_status "Creating namespace: $NAMESPACE"
    kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -
}

apply_manifests() {
    print_status "Applying Kubernetes manifests..."
    
    # Apply in order
    kubectl apply -f kubernetes/00-namespace-config.yaml
    print_status "Applied: namespace and config"
    
    kubectl apply -f kubernetes/01-database-tier.yaml
    print_status "Applied: database tier"
    
    kubectl apply -f kubernetes/02-application-tier.yaml
    print_status "Applied: application tier"
    
    kubectl apply -f kubernetes/03-presentation-tier.yaml
    print_status "Applied: presentation tier"
    
    kubectl apply -f kubernetes/04-ingress-network.yaml
    print_status "Applied: ingress and network policies"
}

wait_for_rollout() {
    print_status "Waiting for deployments to be ready..."
    
    kubectl rollout status deployment/crm-api -n $NAMESPACE --timeout=5m
    kubectl rollout status deployment/crm-frontend -n $NAMESPACE --timeout=5m
    
    print_status "Deployments are ready"
}

verify_deployment() {
    print_status "Verifying deployment..."
    
    echo ""
    print_status "Pods status:"
    kubectl get pods -n $NAMESPACE -o wide
    
    echo ""
    print_status "Services:"
    kubectl get services -n $NAMESPACE
    
    echo ""
    print_status "Deployments:"
    kubectl get deployments -n $NAMESPACE
    
    echo ""
    print_status "HPA status:"
    kubectl get hpa -n $NAMESPACE
    
    echo ""
    print_status "PVC status:"
    kubectl get pvc -n $NAMESPACE
}

port_forward_services() {
    print_status "Setting up port forwarding..."
    print_status "API available at: http://localhost:5000"
    print_status "Frontend available at: http://localhost:3000"
    print_status "Press Ctrl+C to stop port forwarding"
    
    # Port forward in background
    kubectl port-forward -n $NAMESPACE svc/crm-api 5000:5000 &
    API_PF_PID=$!
    
    kubectl port-forward -n $NAMESPACE svc/crm-frontend 3000:3000 &
    FRONTEND_PF_PID=$!
    
    trap "kill $API_PF_PID $FRONTEND_PF_PID" EXIT
    wait
}

scale_deployment() {
    local name=$1
    local replicas=$2
    
    print_status "Scaling $name to $replicas replicas..."
    kubectl scale deployment/$name -n $NAMESPACE --replicas=$replicas
}

show_logs() {
    local deployment=$1
    local lines=${2:-100}
    
    print_status "Last $lines logs from $deployment:"
    kubectl logs -n $NAMESPACE -l app=crm,tier=$deployment --tail=$lines -f
}

cleanup() {
    print_status "Cleaning up deployment..."
    kubectl delete namespace $NAMESPACE --ignore-not-found
    print_status "Cleanup complete"
}

update_images() {
    print_status "Updating container images..."
    
    kubectl set image deployment/crm-api crm-api=$REGISTRY/crm-api:$IMAGE_TAG -n $NAMESPACE
    kubectl set image deployment/crm-frontend crm-frontend=$REGISTRY/crm-frontend:$IMAGE_TAG -n $NAMESPACE
    
    print_status "Waiting for rollout to complete..."
    kubectl rollout status deployment/crm-api -n $NAMESPACE --timeout=5m
    kubectl rollout status deployment/crm-frontend -n $NAMESPACE --timeout=5m
    
    print_status "Update complete"
}

# Main
main() {
    print_status "CRM Application Kubernetes Deployment"
    echo "Namespace: $NAMESPACE"
    echo "Environment: $ENVIRONMENT"
    echo "Registry: $REGISTRY"
    echo "Image Tag: $IMAGE_TAG"
    echo ""
    
    case "${1:-deploy}" in
        deploy)
            prompt_registry
            check_prerequisites
            create_namespace
            apply_manifests
            wait_for_rollout
            verify_deployment
            ;;
        forward)
            port_forward_services
            ;;
        verify)
            verify_deployment
            ;;
        logs)
            if [ -z "$2" ]; then
                print_error "Usage: $0 logs [api|frontend]"
                exit 1
            fi
            show_logs "$2" "${3:-100}"
            ;;
        scale)
            if [ -z "$2" ] || [ -z "$3" ]; then
                print_error "Usage: $0 scale [api|frontend] <replicas>"
                exit 1
            fi
            scale_deployment "crm-$2" "$3"
            ;;
        update-images)
            prompt_registry
            update_images
            ;;
        cleanup)
            cleanup
            ;;
        *)
            print_error "Unknown command: $1"
            echo ""
            echo "Usage: $0 [deploy|forward|verify|logs|scale|update-images|cleanup]"
            echo ""
            echo "Commands:"
            echo "  deploy           Deploy the entire application (default)"
            echo "  forward          Setup port forwarding to services"
            echo "  verify           Verify deployment status"
            echo "  logs [api|frontend]   Show logs from a deployment"
            echo "  scale [api|frontend] <replicas>  Scale deployment to N replicas"
            echo "  update-images    Update container images"
            echo "  cleanup          Remove all resources"
            echo ""
            exit 1
            ;;
    esac
}

main "$@"
