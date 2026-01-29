#!/bin/bash
# =============================================================================
# CRM Solution - Unified Build & Deploy Script v2.0
# =============================================================================
# Uses master naming convention from config/infrastructure.env
#
# Usage:
#   ./scripts/deploy.sh [options] [target]
#
# Options:
#   --env=<env>         Environment: local|dev|staging|prod (default: dev)
#   --arch=<arch>       Architecture: monolith|microservices (default: monolith)
#   --version=<type>    Version bump: patch|minor|major (default: patch)
#   --build-only        Build images but don't deploy
#   --deploy-only       Deploy without rebuilding
#   --skip-sync         Skip source code sync
#   --skip-tests        Skip running tests
#   --debug             Enable debug output
#
# Target:
#   all                 Build and deploy everything (default)
#   api                 Build and deploy API only
#   frontend            Build and deploy frontend only
#   db                  Deploy database only
#
# Examples:
#   ./scripts/deploy.sh --env=dev --arch=monolith
#   ./scripts/deploy.sh --env=prod --version=minor --arch=microservices
#   ./scripts/deploy.sh --deploy-only api
# =============================================================================

set -e

# Script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Load master configuration
CONFIG_FILE="${PROJECT_DIR}/config/infrastructure.env"

# =============================================================================
# CONFIGURATION LOADING
# =============================================================================

load_config() {
    if [ -f "$CONFIG_FILE" ]; then
        # Export all non-comment, non-empty lines
        set -a
        source "$CONFIG_FILE"
        set +a
        log_debug "Loaded configuration from $CONFIG_FILE"
    else
        log_error "Configuration file not found: $CONFIG_FILE"
        exit 1
    fi
}

# =============================================================================
# PARSE ARGUMENTS
# =============================================================================

# Defaults
ENV_NAME="dev"
ARCH_MODE="monolith"
VERSION_TYPE="patch"
BUILD_ONLY=false
DEPLOY_ONLY=false
SKIP_SYNC=false
SKIP_TESTS=false
DEBUG_MODE=false
TARGET="all"

parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --env=*)
                ENV_NAME="${1#*=}"
                shift
                ;;
            --arch=*)
                ARCH_MODE="${1#*=}"
                shift
                ;;
            --version=*)
                VERSION_TYPE="${1#*=}"
                shift
                ;;
            --build-only)
                BUILD_ONLY=true
                shift
                ;;
            --deploy-only)
                DEPLOY_ONLY=true
                shift
                ;;
            --skip-sync)
                SKIP_SYNC=true
                shift
                ;;
            --skip-tests)
                SKIP_TESTS=true
                shift
                ;;
            --debug)
                DEBUG_MODE=true
                shift
                ;;
            all|api|frontend|db)
                TARGET="$1"
                shift
                ;;
            -h|--help)
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
    
    # Set environment-specific values
    case $ENV_NAME in
        local)
            BUILD_SERVER="localhost"
            BUILD_USER="$USER"
            ;;
        dev)
            BUILD_SERVER="${BUILD_SERVER_DEV:-192.168.0.9}"
            BUILD_USER="${BUILD_USER:-root}"
            ;;
        staging)
            BUILD_SERVER="${BUILD_SERVER_STAGING:-staging.crm.local}"
            BUILD_USER="${BUILD_USER:-deploy}"
            ;;
        prod|production)
            BUILD_SERVER="${BUILD_SERVER_PRODUCTION:-prod.crm.local}"
            BUILD_USER="${BUILD_USER:-deploy}"
            ;;
        *)
            log_error "Unknown environment: $ENV_NAME"
            exit 1
            ;;
    esac
    
    # Set architecture mode
    ARCHITECTURE_MODE="$ARCH_MODE"
    export ARCHITECTURE_MODE
}

show_help() {
    head -50 "$0" | grep -E "^#" | sed 's/^# //' | sed 's/^#//'
}

# =============================================================================
# LOGGING FUNCTIONS
# =============================================================================

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
log_step() { echo -e "${CYAN}[STEP]${NC} $1"; }
log_debug() { 
    if [ "$DEBUG_MODE" = "true" ]; then 
        echo -e "${YELLOW}[DEBUG]${NC} $1"
    fi
}

print_banner() {
    local env_upper=$(echo "$ENV_NAME" | tr '[:lower:]' '[:upper:]')
    local arch_upper=$(echo "$ARCH_MODE" | tr '[:lower:]' '[:upper:]')
    local target_upper=$(echo "$TARGET" | tr '[:lower:]' '[:upper:]')
    
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║           CRM Solution - Unified Deployment Pipeline             ║"
    echo "╠══════════════════════════════════════════════════════════════════╣"
    printf "║  Environment:   %-47s ║\n" "$env_upper"
    printf "║  Architecture:  %-47s ║\n" "$arch_upper"
    printf "║  Build Server:  %-47s ║\n" "$BUILD_SERVER"
    printf "║  Target:        %-47s ║\n" "$target_upper"
    printf "║  Debug:         %-47s ║\n" "$DEBUG_MODE"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
}

# =============================================================================
# VERSION MANAGEMENT
# =============================================================================

VERSION_FILE="${PROJECT_DIR}/version.json"

get_version() {
    if [ -f "$VERSION_FILE" ]; then
        MAJOR=$(grep -o '"major": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        MINOR=$(grep -o '"minor": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        PATCH=$(grep -o '"patch": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        echo "${MAJOR}.${MINOR}.${PATCH}"
    else
        echo "1.0.0"
    fi
}

increment_version() {
    local type="${1:-patch}"
    local current=$(get_version)
    
    MAJOR=$(echo "$current" | cut -d. -f1)
    MINOR=$(echo "$current" | cut -d. -f2)
    PATCH=$(echo "$current" | cut -d. -f3)
    
    case "$type" in
        major)
            MAJOR=$((MAJOR + 1))
            MINOR=0
            PATCH=0
            ;;
        minor)
            MINOR=$((MINOR + 1))
            PATCH=0
            ;;
        patch)
            PATCH=$((PATCH + 1))
            ;;
    esac
    
    echo "${MAJOR}.${MINOR}.${PATCH}"
}

update_version_file() {
    local new_version="${1:-$(increment_version $VERSION_TYPE)}"
    local build_date=$(date +%Y-%m-%d)
    local build_time=$(date +%H:%M:%S)
    local git_hash=$(git -C "$PROJECT_DIR" rev-parse --short HEAD 2>/dev/null || echo "unknown")
    local git_branch=$(git -C "$PROJECT_DIR" branch --show-current 2>/dev/null || echo "unknown")
    
    MAJOR=$(echo "$new_version" | cut -d. -f1)
    MINOR=$(echo "$new_version" | cut -d. -f2)
    PATCH=$(echo "$new_version" | cut -d. -f3)
    
    cat > "$VERSION_FILE" << EOF
{
  "major": ${MAJOR},
  "minor": ${MINOR},
  "patch": ${PATCH},
  "lastUpdate": "${build_date}",
  "description": "Auto-generated by deploy.sh",
  "git": {
    "branch": "${git_branch}",
    "commit": "${git_hash}"
  },
  "environment": "${ENV_NAME}",
  "architecture": "${ARCH_MODE}",
  "buildServer": "${BUILD_SERVER}",
  "buildTime": "${build_date}T${build_time}"
}
EOF
    
    # Copy to frontend public folder
    cp "$VERSION_FILE" "${PROJECT_DIR}/CRM.Frontend/public/version.json"
    
    log_success "Updated version to ${new_version}"
    export APP_VERSION="${new_version}"
}

# =============================================================================
# CONNECTIVITY CHECKS
# =============================================================================

check_ssh() {
    log_step "Checking SSH connectivity to ${BUILD_SERVER}..."
    
    if [ "$BUILD_SERVER" = "localhost" ]; then
        log_success "Local deployment - no SSH needed"
        return 0
    fi
    
    if ! ssh -o ConnectTimeout=5 -o BatchMode=yes ${BUILD_USER}@${BUILD_SERVER} "echo 'SSH OK'" &>/dev/null; then
        log_error "Cannot connect to ${BUILD_SERVER}"
        log_info "Hint: Run 'ssh-copy-id ${BUILD_USER}@${BUILD_SERVER}' to setup SSH keys"
        exit 1
    fi
    log_success "SSH connectivity verified"
}

check_docker() {
    log_step "Checking Docker availability..."
    
    local check_cmd="docker info &>/dev/null && echo 'Docker OK'"
    
    if [ "$BUILD_SERVER" = "localhost" ]; then
        if ! eval "$check_cmd"; then
            log_error "Docker is not running locally"
            exit 1
        fi
    else
        if ! ssh ${BUILD_USER}@${BUILD_SERVER} "$check_cmd" &>/dev/null; then
            log_error "Docker is not available on ${BUILD_SERVER}"
            exit 1
        fi
    fi
    log_success "Docker is available"
}

# =============================================================================
# SOURCE SYNC
# =============================================================================

sync_source() {
    if [ "$SKIP_SYNC" = "true" ]; then
        log_info "Skipping source sync (--skip-sync)"
        return 0
    fi
    
    if [ "$BUILD_SERVER" = "localhost" ]; then
        log_info "Local deployment - no sync needed"
        return 0
    fi
    
    log_step "Syncing source code to ${BUILD_SERVER}..."
    
    ssh ${BUILD_USER}@${BUILD_SERVER} "mkdir -p ${REMOTE_SOURCE_PATH:-/opt/crm/source}"
    
    rsync -avz --delete \
        --exclude 'node_modules' \
        --exclude 'bin' \
        --exclude 'obj' \
        --exclude '.git' \
        --exclude 'coverage' \
        --exclude 'build' \
        --exclude 'test-results' \
        "${PROJECT_DIR}/" ${BUILD_USER}@${BUILD_SERVER}:${REMOTE_SOURCE_PATH:-/opt/crm/source}/
    
    log_success "Source code synced"
}

# =============================================================================
# BUILD FUNCTIONS
# =============================================================================

get_compose_file() {
    if [ "$ARCH_MODE" = "microservices" ]; then
        echo "docker/docker-compose.microservices.unified.yml"
    else
        echo "docker/docker-compose.unified.yml"
    fi
}

build_images() {
    if [ "$DEPLOY_ONLY" = "true" ]; then
        log_info "Skipping build (--deploy-only)"
        return 0
    fi
    
    log_step "Building Docker images..."
    
    local compose_file=$(get_compose_file)
    local version="${APP_VERSION:-latest}"
    local build_cmd=""
    
    case $TARGET in
        all)
            if [ "$ARCH_MODE" = "microservices" ]; then
                build_cmd="docker compose -f ${compose_file} build --parallel"
            else
                build_cmd="docker compose -f ${compose_file} build crm-api crm-frontend"
            fi
            ;;
        api)
            if [ "$ARCH_MODE" = "microservices" ]; then
                build_cmd="docker compose -f ${compose_file} build crm-gateway crm-identity crm-customer crm-sales crm-marketing crm-servicedesk crm-core"
            else
                build_cmd="docker compose -f ${compose_file} build crm-api"
            fi
            ;;
        frontend)
            build_cmd="docker compose -f ${compose_file} build crm-frontend"
            ;;
        db)
            log_info "Database uses official image - no build needed"
            return 0
            ;;
    esac
    
    # Execute build
    if [ "$BUILD_SERVER" = "localhost" ]; then
        cd "$PROJECT_DIR"
        eval "$build_cmd"
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "cd ${REMOTE_SOURCE_PATH:-/opt/crm/source} && $build_cmd"
    fi
    
    log_success "Build completed"
}

# =============================================================================
# DEPLOYMENT FUNCTIONS
# =============================================================================

deploy_services() {
    if [ "$BUILD_ONLY" = "true" ]; then
        log_info "Skipping deployment (--build-only)"
        return 0
    fi
    
    log_step "Deploying services..."
    
    local compose_file=$(get_compose_file)
    local deploy_cmd=""
    
    case $TARGET in
        all)
            deploy_cmd="docker compose -f ${compose_file} up -d"
            ;;
        api)
            if [ "$ARCH_MODE" = "microservices" ]; then
                deploy_cmd="docker compose -f ${compose_file} up -d crm-db crm-redis crm-gateway crm-identity crm-customer crm-sales crm-marketing crm-servicedesk crm-core"
            else
                deploy_cmd="docker compose -f ${compose_file} up -d crm-mariadb crm-redis crm-api"
            fi
            ;;
        frontend)
            deploy_cmd="docker compose -f ${compose_file} up -d crm-frontend"
            ;;
        db)
            if [ "$ARCH_MODE" = "microservices" ]; then
                deploy_cmd="docker compose -f ${compose_file} up -d crm-db crm-redis"
            else
                deploy_cmd="docker compose -f ${compose_file} up -d crm-mariadb crm-redis"
            fi
            ;;
    esac
    
    # Execute deployment
    if [ "$BUILD_SERVER" = "localhost" ]; then
        cd "$PROJECT_DIR"
        eval "$deploy_cmd"
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "cd ${REMOTE_SOURCE_PATH:-/opt/crm/source} && $deploy_cmd"
    fi
    
    log_success "Deployment completed"
}

# =============================================================================
# HEALTH CHECK
# =============================================================================

wait_for_health() {
    log_step "Waiting for services to become healthy..."
    
    local max_wait=120
    local wait_interval=5
    local waited=0
    
    local services_to_check=""
    
    case $TARGET in
        all|api)
            if [ "$ARCH_MODE" = "microservices" ]; then
                services_to_check="crm-gateway"
            else
                services_to_check="crm-api"
            fi
            ;;
        frontend)
            services_to_check="crm-frontend"
            ;;
        db)
            if [ "$ARCH_MODE" = "microservices" ]; then
                services_to_check="crm-db"
            else
                services_to_check="crm-mariadb"
            fi
            ;;
    esac
    
    local check_cmd="docker ps --filter 'name=${services_to_check}' --filter 'health=healthy' --format '{{.Names}}' 2>/dev/null"
    
    while [ $waited -lt $max_wait ]; do
        local healthy=""
        
        if [ "$BUILD_SERVER" = "localhost" ]; then
            healthy=$(eval "$check_cmd")
        else
            healthy=$(ssh ${BUILD_USER}@${BUILD_SERVER} "$check_cmd")
        fi
        
        if [ -n "$healthy" ]; then
            log_success "Services are healthy: $healthy"
            return 0
        fi
        
        sleep $wait_interval
        waited=$((waited + wait_interval))
        log_debug "Waited ${waited}s for health check..."
    done
    
    log_warning "Timeout waiting for services to become healthy"
    show_status
    return 1
}

# =============================================================================
# STATUS DISPLAY
# =============================================================================

show_status() {
    log_step "Current container status:"
    
    local status_cmd="docker ps --filter 'name=crm-' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'"
    
    if [ "$BUILD_SERVER" = "localhost" ]; then
        eval "$status_cmd"
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "$status_cmd"
    fi
}

# =============================================================================
# MAIN EXECUTION
# =============================================================================

main() {
    # Parse command line arguments
    parse_args "$@"
    
    # Load configuration
    load_config
    
    # Print banner
    print_banner
    
    # Pre-flight checks
    check_ssh
    check_docker
    
    # Version management
    if [ "$DEPLOY_ONLY" != "true" ]; then
        update_version_file
    fi
    
    # Sync source
    sync_source
    
    # Build
    build_images
    
    # Deploy
    deploy_services
    
    # Health check
    if [ "$BUILD_ONLY" != "true" ]; then
        wait_for_health
    fi
    
    # Show final status
    show_status
    
    log_success "Deployment pipeline completed successfully!"
    
    # Print access URLs
    if [ "$BUILD_ONLY" != "true" ]; then
        echo ""
        log_info "Access URLs:"
        log_info "  Frontend:  http://${BUILD_SERVER}/"
        log_info "  API:       http://${BUILD_SERVER}:5000/"
        log_info "  API Health: http://${BUILD_SERVER}:5000/health"
    fi
}

# Run main
main "$@"
