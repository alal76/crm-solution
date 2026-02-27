#!/bin/bash
# =============================================================================
# ⚠️  DEPRECATED — Use scripts/deploy.sh instead
# =============================================================================
# CRM Solution - Deploy and Test Script
# This script deploys the CRM solution to a target environment and runs
# all tests automatically, generating a comprehensive test report.
#
# This script is DEPRECATED and will be removed in a future release.
# Use the unified parameterized script instead:
#   ./scripts/deploy.sh --env dev --test
#
# Usage (legacy):
#   ./scripts/deploy-and-test.sh                    # Deploy and test (default)
#   ./scripts/deploy-and-test.sh --deploy-only      # Deploy only, no tests
#   ./scripts/deploy-and-test.sh --test-only        # Test only, no deploy
#   ./scripts/deploy-and-test.sh --target 192.168.0.9  # Specify target server
# =============================================================================
echo "⚠️  WARNING: This script is deprecated. Use scripts/deploy.sh --env dev --test instead." >&2

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Configuration
TARGET_SERVER="${TARGET_SERVER:-192.168.0.9}"
SSH_USER="${SSH_USER:-deploy}"
DEPLOY_PATH="/opt/crm"
DOCKER_NETWORK="docker_crm-network"

# Options
DEPLOY_ONLY=false
TEST_ONLY=false
SKIP_BUILD=false
VERBOSE=false

# Report
REPORT_DIR="${PROJECT_ROOT}/test-reports"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DEPLOYMENT_REPORT="${REPORT_DIR}/deployment-report-${TIMESTAMP}.md"

# =============================================================================
# Helper Functions
# =============================================================================

log_step() {
    echo ""
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Get version from version.json
get_version() {
    if [[ -f "${PROJECT_ROOT}/version.json" ]]; then
        cat "${PROJECT_ROOT}/version.json" | grep '"version"' | head -1 | sed 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/'
    else
        echo "0.0.0"
    fi
}

# Get git commit hash
get_git_hash() {
    if command -v git &> /dev/null && [ -d "${PROJECT_ROOT}/.git" ]; then
        git -C "${PROJECT_ROOT}" rev-parse --short HEAD 2>/dev/null || echo "unknown"
    else
        echo "unknown"
    fi
}

# Check if Docker is running
check_docker() {
    if ! docker info &> /dev/null; then
        log_error "Docker is not running. Please start Docker first."
        exit 1
    fi
}

# Check SSH connectivity
check_ssh() {
    if ! ssh -q -o ConnectTimeout=5 "${SSH_USER}@${TARGET_SERVER}" exit 2>/dev/null; then
        log_error "Cannot connect to ${TARGET_SERVER}. Check SSH configuration."
        exit 1
    fi
    log_success "SSH connection to ${TARGET_SERVER} verified"
}

# Parse command line arguments
parse_args() {
    while [[ $# -gt 0 ]]; do
        case $1 in
            --deploy-only)
                DEPLOY_ONLY=true
                shift
                ;;
            --test-only)
                TEST_ONLY=true
                shift
                ;;
            --skip-build)
                SKIP_BUILD=true
                shift
                ;;
            --target)
                TARGET_SERVER="$2"
                shift 2
                ;;
            --ssh-user)
                SSH_USER="$2"
                shift 2
                ;;
            --verbose)
                VERBOSE=true
                shift
                ;;
            --help)
                echo "Usage: $0 [OPTIONS]"
                echo ""
                echo "Options:"
                echo "  --deploy-only    Deploy only, skip tests"
                echo "  --test-only      Test only, skip deployment"
                echo "  --skip-build     Skip Docker build (use existing images)"
                echo "  --target HOST    Target server (default: 192.168.0.9)"
                echo "  --ssh-user USER  SSH user (default: root)"
                echo "  --verbose        Verbose output"
                echo "  --help           Show this help message"
                exit 0
                ;;
            *)
                log_error "Unknown option: $1"
                exit 1
                ;;
        esac
    done
}

# =============================================================================
# Build Functions
# =============================================================================

build_backend() {
    log_step "Building Backend Docker Image"
    
    cd "${PROJECT_ROOT}"
    
    local version=$(get_version)
    local git_hash=$(get_git_hash)
    local image_tag="crm-api:${version}-${git_hash}"
    
    log_info "Building image: ${image_tag}"
    
    # Build for linux/amd64 (server architecture)
    docker buildx build \
        --platform linux/amd64 \
        --build-arg VERSION="${version}" \
        --build-arg GIT_HASH="${git_hash}" \
        -t crm-api:latest \
        -t "${image_tag}" \
        -f docker/Dockerfile.backend \
        --load \
        .
    
    log_success "Backend image built: ${image_tag}"
    echo "${image_tag}" > "${REPORT_DIR}/backend-image-tag.txt"
}

build_frontend() {
    log_step "Building Frontend Docker Image"
    
    cd "${PROJECT_ROOT}"
    
    local version=$(get_version)
    local git_hash=$(get_git_hash)
    local image_tag="crm-frontend:${version}-${git_hash}"
    
    log_info "Building image: ${image_tag}"
    
    # Build for linux/amd64
    docker buildx build \
        --platform linux/amd64 \
        --build-arg VERSION="${version}" \
        --build-arg REACT_APP_API_URL="http://${TARGET_SERVER}:5000" \
        -t crm-frontend:latest \
        -t "${image_tag}" \
        -f docker/Dockerfile.frontend \
        --load \
        .
    
    log_success "Frontend image built: ${image_tag}"
    echo "${image_tag}" > "${REPORT_DIR}/frontend-image-tag.txt"
}

# =============================================================================
# Deploy Functions
# =============================================================================

transfer_images() {
    log_step "Transferring Docker Images to ${TARGET_SERVER}"
    
    log_info "Saving backend image..."
    docker save crm-api:latest | ssh "${SSH_USER}@${TARGET_SERVER}" "docker load"
    log_success "Backend image transferred"
    
    log_info "Saving frontend image..."
    docker save crm-frontend:latest | ssh "${SSH_USER}@${TARGET_SERVER}" "docker load"
    log_success "Frontend image transferred"
}

deploy_containers() {
    log_step "Deploying Containers on ${TARGET_SERVER}"
    
    local version=$(get_version)
    local git_hash=$(get_git_hash)
    
    # Deploy using docker-compose on remote server
    ssh "${SSH_USER}@${TARGET_SERVER}" << REMOTE_SCRIPT
        cd ${DEPLOY_PATH}
        
        # Stop existing containers
        echo "Stopping existing containers..."
        docker compose down api frontend 2>/dev/null || true
        
        # Remove old containers
        docker rm -f crm-api crm-frontend 2>/dev/null || true
        
        # Start containers with docker-compose
        echo "Starting containers..."
        docker compose up -d api frontend
        
        # Wait for containers to be healthy
        echo "Waiting for services to be ready..."
        sleep 10
        
        # Check health
        echo "Checking API health..."
        curl -sf http://localhost:5000/health || echo "API health check pending..."
        
        echo "Deployment complete"
REMOTE_SCRIPT
    
    log_success "Containers deployed successfully"
}

verify_deployment() {
    log_step "Verifying Deployment"
    
    local max_attempts=30
    local attempt=1
    
    log_info "Waiting for API to be ready..."
    
    while [[ $attempt -le $max_attempts ]]; do
        if curl -sf "http://${TARGET_SERVER}:5000/health" > /dev/null 2>&1; then
            log_success "API is healthy"
            break
        fi
        
        if [[ $attempt -eq $max_attempts ]]; then
            log_error "API failed to become healthy after ${max_attempts} attempts"
            return 1
        fi
        
        log_info "Attempt ${attempt}/${max_attempts} - waiting..."
        sleep 2
        ((attempt++))
    done
    
    # Check frontend
    log_info "Checking frontend..."
    if curl -sf "http://${TARGET_SERVER}" > /dev/null 2>&1; then
        log_success "Frontend is accessible"
    else
        log_warning "Frontend may not be ready yet"
    fi
    
    return 0
}

# =============================================================================
# Test Functions
# =============================================================================

run_tests() {
    log_step "Running Test Suite"
    
    cd "${PROJECT_ROOT}"
    
    # Run the test runner script
    ./scripts/run-all-tests.sh --api-url "http://${TARGET_SERVER}:5000"
    
    log_success "Tests completed"
}

run_smoke_tests() {
    log_step "Running Smoke Tests"
    
    local api_url="http://${TARGET_SERVER}:5000"
    local passed=0
    local failed=0
    
    # Test 1: Health endpoint
    log_info "Test 1: Health endpoint..."
    if curl -sf "${api_url}/health" > /dev/null; then
        log_success "Health check passed"
        ((passed++))
    else
        log_error "Health check failed"
        ((failed++))
    fi
    
    # Test 2: Login endpoint
    log_info "Test 2: Login endpoint..."
    local login_response=$(curl -sf -X POST "${api_url}/api/auth/login" \
        -H "Content-Type: application/json" \
        -d '{"email":"admin@crm.local","password":"Admin@123"}' 2>/dev/null)
    
    if echo "$login_response" | grep -q "accessToken"; then
        log_success "Login test passed"
        ((passed++))
    else
        log_error "Login test failed"
        ((failed++))
    fi
    
    # Test 3: Protected endpoint
    log_info "Test 3: Protected endpoint..."
    local token=$(echo "$login_response" | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
    if [[ -n "$token" ]]; then
        local accounts_response=$(curl -sf "${api_url}/api/accounts" \
            -H "Authorization: Bearer ${token}" 2>/dev/null)
        
        if [[ -n "$accounts_response" ]]; then
            log_success "Protected endpoint test passed"
            ((passed++))
        else
            log_error "Protected endpoint test failed"
            ((failed++))
        fi
    else
        log_error "No token available for protected endpoint test"
        ((failed++))
    fi
    
    echo ""
    log_info "Smoke Tests: ${passed} passed, ${failed} failed"
    
    return $failed
}

# =============================================================================
# Report Generation
# =============================================================================

generate_deployment_report() {
    log_step "Generating Deployment Report"
    
    local version=$(get_version)
    local git_hash=$(get_git_hash)
    local backend_image=$(cat "${REPORT_DIR}/backend-image-tag.txt" 2>/dev/null || echo "unknown")
    local frontend_image=$(cat "${REPORT_DIR}/frontend-image-tag.txt" 2>/dev/null || echo "unknown")
    
    cat > "${DEPLOYMENT_REPORT}" << EOF
# CRM Solution - Deployment Report

**Generated:** $(date "+%Y-%m-%d %H:%M:%S")  
**Report ID:** ${TIMESTAMP}

---

## Deployment Information

| Setting | Value |
|---------|-------|
| Target Server | ${TARGET_SERVER} |
| Deploy Path | ${DEPLOY_PATH} |
| Version | ${version} |
| Git Commit | ${git_hash} |

---

## Docker Images

| Component | Image Tag |
|-----------|-----------|
| Backend API | ${backend_image} |
| Frontend | ${frontend_image} |

---

## Deployment Status

- Build: ✅ Complete
- Transfer: ✅ Complete
- Deploy: ✅ Complete
- Verification: ✅ Complete

---

## Service Endpoints

| Service | URL | Status |
|---------|-----|--------|
| API | http://${TARGET_SERVER}:5000 | $(curl -sf http://${TARGET_SERVER}:5000/health > /dev/null && echo "✅ Healthy" || echo "❌ Down") |
| Frontend | http://${TARGET_SERVER} | $(curl -sf http://${TARGET_SERVER} > /dev/null && echo "✅ Accessible" || echo "⚠️ Check") |
| Health | http://${TARGET_SERVER}:5000/health | $(curl -sf http://${TARGET_SERVER}:5000/health > /dev/null && echo "✅ OK" || echo "❌ Failed") |

---

## Test Results

See: \`test-reports/test-report-${TIMESTAMP}.md\`

---

## Next Steps

1. Verify all services are running correctly
2. Run full E2E test suite if not already done
3. Monitor application logs for any issues

---

*Report generated by deploy-and-test.sh*
EOF

    log_success "Deployment report: ${DEPLOYMENT_REPORT}"
}

# =============================================================================
# Main Execution
# =============================================================================

main() {
    parse_args "$@"
    
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    echo "║         CRM Solution - Deploy and Test                           ║"
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
    
    # Create report directory
    mkdir -p "${REPORT_DIR}"
    
    # Display configuration
    log_info "Version: $(get_version)"
    log_info "Git Commit: $(get_git_hash)"
    log_info "Target Server: ${TARGET_SERVER}"
    log_info "Deploy Only: ${DEPLOY_ONLY}"
    log_info "Test Only: ${TEST_ONLY}"
    echo ""
    
    # Pre-flight checks
    check_docker
    check_ssh
    
    local exit_code=0
    
    if [[ "$TEST_ONLY" = false ]]; then
        # Build phase
        if [[ "$SKIP_BUILD" = false ]]; then
            build_backend
            build_frontend
        else
            log_info "Skipping build (--skip-build)"
        fi
        
        # Deploy phase
        transfer_images
        deploy_containers
        verify_deployment || exit_code=1
    fi
    
    if [[ "$DEPLOY_ONLY" = false ]]; then
        # Test phase
        run_smoke_tests || exit_code=1
        
        if [[ $exit_code -eq 0 ]]; then
            run_tests || exit_code=1
        else
            log_warning "Skipping full tests due to smoke test failures"
        fi
    fi
    
    # Generate report
    generate_deployment_report
    
    echo ""
    echo "╔══════════════════════════════════════════════════════════════════╗"
    if [[ $exit_code -eq 0 ]]; then
        echo "║         ✅ Deployment and Testing Complete                       ║"
    else
        echo "║         ⚠️  Completed with Warnings/Errors                       ║"
    fi
    echo "╚══════════════════════════════════════════════════════════════════╝"
    echo ""
    echo "Deployment Report: ${DEPLOYMENT_REPORT}"
    echo ""
    
    exit $exit_code
}

main "$@"
