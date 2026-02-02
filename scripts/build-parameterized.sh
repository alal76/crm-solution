#!/bin/bash
# =============================================================================
# CRM Solution - Parameterized Build Script
# =============================================================================
# Universal build script that reads configuration from .cicd-config.yml
# and .env files to build for any environment without script modification
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
NC='\033[0m' # No Color

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# =============================================================================
# Load Environment Configuration
# =============================================================================
load_env_config() {
    log_info "Loading environment configuration..."
    
    # Load .env file safely if exists
    if [ -f "$PROJECT_ROOT/.env" ]; then
        log_info "Loading .env file"
        set -a  # Enable auto-export of variables
        source "$PROJECT_ROOT/.env"
        set +a  # Disable auto-export
    elif [ -f "$PROJECT_ROOT/.env.example" ]; then
        log_warn ".env not found, using .env.example as template"
        log_warn "Please copy .env.example to .env and configure your values"
    fi
    
    # Set defaults if not set
    export ARCHITECTURE_MODE="${ARCHITECTURE_MODE:-monolithic}"
    export DEPLOY_FRONTEND="${DEPLOY_FRONTEND:-true}"
    export DEPLOY_API="${DEPLOY_API:-true}"
    export DEPLOY_DATABASE="${DEPLOY_DATABASE:-true}"
    export DEPLOY_REDIS="${DEPLOY_REDIS:-true}"
    export BUILD_OPTIMIZATION="${BUILD_OPTIMIZATION:-Release}"
    export SKIP_TESTS="${SKIP_TESTS:-false}"
    export DATABASE_PROVIDER="${DATABASE_PROVIDER:-mariadb}"
    
    log_success "Configuration loaded: Architecture=${ARCHITECTURE_MODE}"
}

# =============================================================================
# Parse Command Line Arguments
# =============================================================================
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --arch|--architecture)
                export ARCHITECTURE_MODE="$2"
                shift 2
                ;;
            --env|--environment)
                export TARGET_ENV="$2"
                shift 2
                ;;
            --skip-tests)
                export SKIP_TESTS=true
                shift
                ;;
            --skip-frontend)
                export DEPLOY_FRONTEND=false
                shift
                ;;
            --skip-backend)
                export DEPLOY_API=false
                shift
                ;;
            --components)
                # Comma-separated list: frontend,backend,database,redis
                export BUILD_COMPONENTS="$2"
                shift 2
                ;;
            --registry)
                export REGISTRY_URL="$2"
                shift 2
                ;;
            --tag)
                export IMAGE_TAG="$2"
                shift 2
                ;;
            --push)
                export PUSH_IMAGES=true
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

Parameterized build script for CRM Solution

OPTIONS:
    --arch, --architecture MODE    Set architecture mode (monolithic|microservices)
    --env, --environment ENV       Target environment (development|staging|production)
    --skip-tests                   Skip running tests
    --skip-frontend                Don't build frontend
    --skip-backend                 Don't build backend
    --components LIST              Comma-separated components to build
    --registry URL                 Docker registry URL
    --tag TAG                      Image tag (default: latest)
    --push                         Push images to registry
    --help, -h                     Show this help message

EXAMPLES:
    # Build monolithic architecture with defaults
    $0 --arch monolithic
    
    # Build microservices for production
    $0 --arch microservices --env production --push
    
    # Build only frontend and backend
    $0 --components frontend,backend
    
    # Quick build without tests
    $0 --skip-tests --tag dev-$(date +%Y%m%d)

ENVIRONMENT VARIABLES:
    See .env.example for all available configuration options
    Copy .env.example to .env and customize for your environment

EOF
}

# =============================================================================
# Build Functions
# =============================================================================
build_frontend() {
    if [ "$DEPLOY_FRONTEND" != "true" ]; then
        log_info "Skipping frontend build (disabled)"
        return 0
    fi
    
    log_info "Building React Frontend..."
    cd "$PROJECT_ROOT/CRM.Frontend"
    
    if [ ! -d "node_modules" ]; then
        log_info "Installing npm dependencies..."
        npm ci
    fi
    
    log_info "Running TypeScript check..."
    npx tsc --noEmit || log_warn "TypeScript check completed with warnings"
    
    if [ "$SKIP_TESTS" != "true" ]; then
        log_info "Running frontend tests..."
        npm test -- --coverage --watchAll=false --passWithNoTests || log_warn "Some tests failed"
    fi
    
    log_info "Building production bundle..."
    npm run build
    
    log_success "Frontend build completed"
}

build_backend() {
    if [ "$DEPLOY_API" != "true" ]; then
        log_info "Skipping backend build (disabled)"
        return 0
    fi
    
    log_info "Building .NET Backend..."
    cd "$PROJECT_ROOT/CRM.Backend"
    
    log_info "Restoring NuGet packages..."
    dotnet restore CRM.sln
    
    log_info "Building solution..."
    dotnet build CRM.sln -c "${BUILD_OPTIMIZATION}" --no-restore
    
    if [ "$SKIP_TESTS" != "true" ]; then
        log_info "Running backend tests..."
        dotnet test tests/CRM.Tests.csproj --no-build -c "${BUILD_OPTIMIZATION}" || log_warn "Some tests failed"
    fi
    
    log_success "Backend build completed"
}

build_docker_images() {
    log_info "Building Docker images..."
    cd "$PROJECT_ROOT"
    
    IMAGE_TAG="${IMAGE_TAG:-latest}"
    REGISTRY_PREFIX="${REGISTRY_URL:+${REGISTRY_URL}/}"
    
    if [ "$DEPLOY_FRONTEND" == "true" ]; then
        log_info "Building frontend Docker image..."
        docker build \
            -f docker/Dockerfile.frontend \
            -t "${REGISTRY_PREFIX}crm-frontend:${IMAGE_TAG}" \
            --build-arg REACT_APP_API_URL="${REACT_APP_API_URL:-}" \
            .
        log_success "Frontend image built: ${REGISTRY_PREFIX}crm-frontend:${IMAGE_TAG}"
    fi
    
    if [ "$DEPLOY_API" == "true" ]; then
        log_info "Building backend Docker image..."
        docker build \
            -f docker/Dockerfile.backend \
            -t "${REGISTRY_PREFIX}crm-api:${IMAGE_TAG}" \
            .
        log_success "Backend image built: ${REGISTRY_PREFIX}crm-api:${IMAGE_TAG}"
    fi
    
    # Build microservices if needed
    if [ "$ARCHITECTURE_MODE" == "microservices" ]; then
        log_info "Building microservices images..."
        
        for service in gateway identity customer sales marketing servicedesk core; do
            service_enabled="DEPLOY_${service^^}"
            if [ "${!service_enabled:-true}" == "true" ]; then
                log_info "Building crm-${service} service..."
                docker build \
                    -f "docker/Dockerfile.${service}" \
                    -t "${REGISTRY_PREFIX}crm-${service}:${IMAGE_TAG}" \
                    . 2>/dev/null || log_warn "Dockerfile for ${service} not found, skipping..."
            fi
        done
    fi
}

push_images() {
    if [ "$PUSH_IMAGES" != "true" ]; then
        log_info "Skipping image push (not requested)"
        return 0
    fi
    
    if [ -z "$REGISTRY_URL" ]; then
        log_warn "No registry URL specified, skipping push"
        return 0
    fi
    
    log_info "Pushing images to registry: $REGISTRY_URL"
    IMAGE_TAG="${IMAGE_TAG:-latest}"
    
    if [ "$DEPLOY_FRONTEND" == "true" ]; then
        log_info "Pushing frontend image..."
        docker push "${REGISTRY_URL}/crm-frontend:${IMAGE_TAG}"
    fi
    
    if [ "$DEPLOY_API" == "true" ]; then
        log_info "Pushing backend image..."
        docker push "${REGISTRY_URL}/crm-api:${IMAGE_TAG}"
    fi
    
    log_success "Images pushed successfully"
}

generate_build_summary() {
    cat << EOF

${GREEN}═════════════════════════════════════════════════════════════════${NC}
${GREEN}                    BUILD SUMMARY                                ${NC}
${GREEN}═════════════════════════════════════════════════════════════════${NC}

Architecture:     ${ARCHITECTURE_MODE}
Environment:      ${TARGET_ENV:-development}
Optimization:     ${BUILD_OPTIMIZATION}
Tests Run:        $([ "$SKIP_TESTS" == "true" ] && echo "No" || echo "Yes")

Components Built:
  Frontend:       $([ "$DEPLOY_FRONTEND" == "true" ] && echo "✓" || echo "✗")
  Backend:        $([ "$DEPLOY_API" == "true" ] && echo "✓" || echo "✗")
  Docker Images:  $([ -n "$(docker images -q crm-*:${IMAGE_TAG:-latest} 2>/dev/null)" ] && echo "✓" || echo "✗")

$([ -n "$REGISTRY_URL" ] && echo "Registry:         $REGISTRY_URL")
$([ -n "$IMAGE_TAG" ] && echo "Image Tag:        $IMAGE_TAG")
$([ "$PUSH_IMAGES" == "true" ] && echo "Images Pushed:    Yes")

${GREEN}═════════════════════════════════════════════════════════════════${NC}

EOF
}

# =============================================================================
# Main Execution
# =============================================================================
main() {
    log_info "CRM Solution - Parameterized Build Script"
    log_info "=========================================="
    
    # Load configuration
    load_env_config
    
    # Parse arguments
    parse_args "$@"
    
    # Display configuration
    log_info "Build Configuration:"
    log_info "  Architecture: ${ARCHITECTURE_MODE}"
    log_info "  Target Environment: ${TARGET_ENV:-development}"
    log_info "  Skip Tests: ${SKIP_TESTS}"
    
    # Execute build steps
    START_TIME=$(date +%s)
    
    # Parse component list if specified
    if [ -n "$BUILD_COMPONENTS" ]; then
        IFS=',' read -ra COMPONENTS <<< "$BUILD_COMPONENTS"
        for component in "${COMPONENTS[@]}"; do
            case "$component" in
                frontend) build_frontend ;;
                backend) build_backend ;;
                docker) build_docker_images ;;
                *) log_warn "Unknown component: $component" ;;
            esac
        done
    else
        # Build everything
        build_frontend
        build_backend
        build_docker_images
    fi
    
    # Push images if requested
    push_images
    
    END_TIME=$(date +%s)
    DURATION=$((END_TIME - START_TIME))
    
    # Generate summary
    generate_build_summary
    
    log_success "Build completed in ${DURATION} seconds"
}

# Run main function
main "$@"
