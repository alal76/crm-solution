#!/bin/bash
# =============================================================================
# CRM Solution - Build & Deploy Script v2.0
# Target: 192.168.0.9 (Docker Compose deployment)
# Updated: January 2025
# =============================================================================

set -e

# Configuration
BUILD_HOST="${BUILD_HOST:-192.168.0.9}"
BUILD_USER="${BUILD_USER:-root}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
VERSION_FILE="${PROJECT_DIR}/version.json"
DEBUG_MODE="${DEBUG:-false}"

# Database configuration
DB_USER="crm_user"
DB_PASS="CrmPass@Dev2024!"
DB_NAME="crm_db"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
MAGENTA='\033[0;35m'
NC='\033[0m'

# Helper functions
log_info() { echo -e "${BLUE}ℹ${NC} $1"; }
log_success() { echo -e "${GREEN}✓${NC} $1"; }
log_warning() { echo -e "${YELLOW}⚠${NC} $1"; }
log_error() { echo -e "${RED}✗${NC} $1"; }
log_step() { echo -e "${CYAN}→${NC} $1"; }
log_debug() { [[ "$DEBUG_MODE" == "true" ]] && echo -e "${MAGENTA}[DEBUG]${NC} $1"; }

print_banner() {
    local version=$(get_version)
    echo ""
    echo -e "${CYAN}╔══════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║${NC}         ${GREEN}CRM Solution - Build & Deploy Pipeline${NC}                   ${CYAN}║${NC}"
    echo -e "${CYAN}║${NC}              Version: ${YELLOW}${version}${NC}                                   ${CYAN}║${NC}"
    echo -e "${CYAN}║${NC}              Target:  ${YELLOW}${BUILD_HOST}${NC}                             ${CYAN}║${NC}"
    echo -e "${CYAN}╚══════════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

# Get current version from version.json
get_version() {
    if [[ -f "$VERSION_FILE" ]]; then
        local major=$(grep -o '"major": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        local minor=$(grep -o '"minor": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        local patch=$(grep -o '"patch": *[0-9]*' "$VERSION_FILE" | grep -o '[0-9]*')
        echo "${major}.${minor}.${patch}"
    else
        echo "0.0.1"
    fi
}

# Increment version
increment_version() {
    local type="${1:-patch}"
    local current=$(get_version)
    
    local major=$(echo "$current" | cut -d. -f1)
    local minor=$(echo "$current" | cut -d. -f2)
    local patch=$(echo "$current" | cut -d. -f3)
    
    case "$type" in
        major) major=$((major + 1)); minor=0; patch=0 ;;
        minor) minor=$((minor + 1)); patch=0 ;;
        patch) patch=$((patch + 1)) ;;
    esac
    
    echo "${major}.${minor}.${patch}"
}

# Update version.json
update_version_json() {
    local new_version="$1"
    local build_date=$(date +%Y-%m-%d)
    local build_time=$(date +%H:%M:%S)
    local git_hash=$(git -C "$PROJECT_DIR" rev-parse --short HEAD 2>/dev/null || echo "unknown")
    local git_branch=$(git -C "$PROJECT_DIR" branch --show-current 2>/dev/null || echo "unknown")
    
    local major=$(echo "$new_version" | cut -d. -f1)
    local minor=$(echo "$new_version" | cut -d. -f2)
    local patch=$(echo "$new_version" | cut -d. -f3)
    
    cat > "$VERSION_FILE" << EOF
{
  "major": ${major},
  "minor": ${minor},
  "patch": ${patch},
  "lastUpdate": "${build_date}",
  "description": "CRM Solution v${new_version}",
  "git": {
    "branch": "${git_branch}",
    "commit": "${git_hash}"
  },
  "environment": "production",
  "architecture": "monolith",
  "buildServer": "${BUILD_HOST}",
  "buildTime": "${build_date}T${build_time}"
}
EOF
    
    # Copy to frontend for runtime access
    cp "$VERSION_FILE" "${PROJECT_DIR}/CRM.Frontend/public/version.json" 2>/dev/null || true
    
    log_success "Version updated to ${new_version}"
}

# Check SSH connectivity
check_ssh() {
    log_step "Checking SSH connectivity to ${BUILD_HOST}..."
    if ! ssh -o ConnectTimeout=5 -o BatchMode=yes ${BUILD_USER}@${BUILD_HOST} "echo 'SSH OK'" &>/dev/null; then
        log_error "Cannot connect to ${BUILD_HOST}"
        log_info "Run: ssh-copy-id ${BUILD_USER}@${BUILD_HOST}"
        exit 1
    fi
    log_success "SSH connection verified"
}

# Sync source code to build server
sync_source() {
    log_step "Syncing source code to ${BUILD_HOST}..."
    
    ssh ${BUILD_USER}@${BUILD_HOST} "mkdir -p /opt/crm/source"
    
    rsync -avz --delete \
        --exclude 'node_modules' \
        --exclude 'bin' \
        --exclude 'obj' \
        --exclude '.git' \
        --exclude 'coverage' \
        --exclude 'build' \
        --exclude 'test-results' \
        --exclude '*.log' \
        --exclude '.auth' \
        "${PROJECT_DIR}/" ${BUILD_USER}@${BUILD_HOST}:/opt/crm/source/ 2>&1 | tail -5
    
    log_success "Source synced to /opt/crm/source"
}

# Build Docker images
build_images() {
    local version="$1"
    log_step "Building Docker images (v${version})..."
    
    ssh ${BUILD_USER}@${BUILD_HOST} << BUILDSCRIPT
        set -e
        cd /opt/crm/source
        
        echo "Building API image..."
        docker build -f docker/Dockerfile.backend -t crm-backend:v${version} -t crm-backend:latest . 2>&1 | tail -10
        
        echo "Building Frontend image..."
        docker build -f docker/Dockerfile.frontend -t crm-frontend:v${version} -t crm-frontend:latest \
            --build-arg REACT_APP_VERSION=${version} . 2>&1 | tail -10
        
        echo "Images built:"
        docker images | grep -E "crm-backend|crm-frontend" | head -6
BUILDSCRIPT
    
    log_success "Docker images built"
}

# Deploy with Docker Compose
deploy_containers() {
    local version="$1"
    log_step "Deploying containers (v${version})..."
    
    ssh ${BUILD_USER}@${BUILD_HOST} << 'DEPLOYSCRIPT'
        set -e
        cd /opt/crm/source
        
        # Stop ALL existing CRM containers (including microservices)
        echo "Stopping existing containers..."
        for container in crm-api crm-frontend crm-gateway crm-identity crm-customer crm-sales crm-marketing crm-servicedesk crm-core; do
            docker stop $container 2>/dev/null || true
            docker rm $container 2>/dev/null || true
        done
        
        # Ensure database is running
        if ! docker ps | grep -q crm-mariadb; then
            echo "Starting MariaDB..."
            docker rm -f crm-mariadb 2>/dev/null || true
            docker network create crm-database-network 2>/dev/null || true
            docker run -d --name crm-mariadb \
                --restart unless-stopped \
                --network crm-database-network \
                -p 3306:3306 \
                -v /opt/crm/data/mysql:/var/lib/mysql \
                -e MYSQL_ROOT_PASSWORD=RootPass@Dev2024 \
                -e MYSQL_DATABASE=crm_db \
                -e MYSQL_USER=crm_user \
                -e 'MYSQL_PASSWORD=CrmPass@Dev2024!' \
                mariadb:10.11
            sleep 10
        fi
        
        # Start API container
        echo "Starting API container..."
        docker run -d --name crm-api \
            --restart unless-stopped \
            --network crm-database-network \
            -p 5000:5000 \
            -v /opt/crm/data:/app/data \
            -e ASPNETCORE_ENVIRONMENT=Development \
            -e "ASPNETCORE_URLS=http://+:5000" \
            -e "ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024!;AllowUserVariables=true" \
            -e "DatabaseProvider=MariaDb" \
            -e "Jwt__Secret=CrmSuperSecretKey2024ForJwtTokenGenerationMinimum32Chars" \
            -e "Jwt__Issuer=CrmApi" \
            -e "Jwt__Audience=CrmClient" \
            -e "Jwt__ExpirationMinutes=1440" \
            -e "Cors__AllowedOrigins=http://192.168.0.9,http://localhost:3000,http://localhost" \
            -e "AllowedHosts=*" \
            -e "LLMProviders__LocalLLM__Enabled=true" \
            -e "LLMProviders__LocalLLM__BaseUrl=http://192.168.0.9:11434" \
            -e "LLMProviders__LocalLLM__DefaultModel=qwen2.5:0.5b" \
            -e "LLMProviders__LocalLLM__ApiFormat=ollama" \
            crm-backend:latest
        
        # Start Frontend container
        echo "Starting Frontend container..."
        docker run -d --name crm-frontend \
            --restart unless-stopped \
            --network crm-database-network \
            -p 80:80 \
            crm-frontend:latest
        
        # Wait for startup
        sleep 5
        
        # Show status
        echo ""
        echo "Container Status:"
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "crm-|NAMES"
DEPLOYSCRIPT
    
    log_success "Containers deployed"
}

# Verify deployment
verify_deployment() {
    log_step "Verifying deployment..."
    
    # Wait for containers to be ready
    sleep 3
    
    # Check API health
    local api_status=$(ssh ${BUILD_USER}@${BUILD_HOST} "curl -s -o /dev/null -w '%{http_code}' http://localhost:5000/health 2>/dev/null" || echo "000")
    local frontend_status=$(ssh ${BUILD_USER}@${BUILD_HOST} "curl -s -o /dev/null -w '%{http_code}' http://localhost/ 2>/dev/null" || echo "000")
    
    echo ""
    if [[ "$api_status" == "200" ]]; then
        log_success "API:      http://${BUILD_HOST}:5000 (HTTP ${api_status})"
    else
        log_warning "API:      http://${BUILD_HOST}:5000 (HTTP ${api_status} - starting...)"
    fi
    
    if [[ "$frontend_status" == "200" ]]; then
        log_success "Frontend: http://${BUILD_HOST} (HTTP ${frontend_status})"
    else
        log_warning "Frontend: http://${BUILD_HOST} (HTTP ${frontend_status} - starting...)"
    fi
    
    # Get database stats
    echo ""
    log_info "Database Statistics:"
    ssh ${BUILD_USER}@${BUILD_HOST} "docker exec crm-mariadb mariadb -u${DB_USER} -p'${DB_PASS}' ${DB_NAME} -sN -e \"
        SELECT CONCAT('  Tables: ', COUNT(*)) FROM information_schema.tables WHERE table_schema='${DB_NAME}';
        SELECT CONCAT('  Customers: ', COUNT(*)) FROM Customers;
        SELECT CONCAT('  Contacts: ', COUNT(*)) FROM Contacts;
        SELECT CONCAT('  Accounts: ', COUNT(*)) FROM Accounts;
        SELECT CONCAT('  Opportunities: ', COUNT(*)) FROM Opportunities;
    \"" 2>/dev/null || log_warning "Could not get database stats"
}

# Clean old images
clean_images() {
    log_step "Cleaning old Docker images..."
    ssh ${BUILD_USER}@${BUILD_HOST} << 'CLEANSCRIPT'
        # Remove dangling images
        docker image prune -f 2>/dev/null || true
        
        # Keep only last 3 versions of each image
        for img in crm-backend crm-frontend; do
            docker images $img --format "{{.ID}} {{.Tag}}" | sort -t'v' -k2 -nr | tail -n +4 | awk '{print $1}' | xargs -r docker rmi 2>/dev/null || true
        done
        
        echo "Current images:"
        docker images | grep -E "crm-backend|crm-frontend" | head -6
CLEANSCRIPT
    log_success "Old images cleaned"
}

# Show status
show_status() {
    log_step "Current deployment status:"
    ssh ${BUILD_USER}@${BUILD_HOST} << 'STATUSSCRIPT'
        echo ""
        echo "=== Containers ==="
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | grep -E "crm-|NAMES" || echo "No CRM containers running"
        
        echo ""
        echo "=== Images ==="
        docker images | grep -E "crm-backend|crm-frontend|REPOSITORY" | head -7
        
        echo ""
        echo "=== Health ==="
        curl -s http://localhost:5000/health 2>/dev/null || echo "API not responding"
STATUSSCRIPT
}

# Print help
print_help() {
    echo "CRM Solution - Build & Deploy Script v2.0"
    echo ""
    echo "Usage: $0 [command] [options]"
    echo ""
    echo "Commands:"
    echo "  patch       Increment patch version and deploy (default)"
    echo "  minor       Increment minor version and deploy"
    echo "  major       Increment major version and deploy"
    echo "  status      Show current deployment status"
    echo "  clean       Clean old Docker images"
    echo "  version     Show current version"
    echo "  help        Show this help message"
    echo ""
    echo "Options:"
    echo "  --debug     Enable debug output"
    echo "  --host      Specify target host (default: 192.168.0.9)"
    echo ""
    echo "Examples:"
    echo "  $0              # Deploy with patch version bump"
    echo "  $0 minor        # Deploy with minor version bump"
    echo "  $0 status       # Show status only"
    echo "  $0 --debug      # Deploy with debug output"
    echo ""
}

# Main execution
main() {
    local command="${1:-patch}"
    
    case "$command" in
        help|--help|-h)
            print_help
            exit 0
            ;;
        version)
            echo "Current version: $(get_version)"
            exit 0
            ;;
        status)
            check_ssh
            show_status
            exit 0
            ;;
        clean)
            check_ssh
            clean_images
            exit 0
            ;;
        major|minor|patch)
            ;;
        *)
            log_error "Unknown command: $command"
            print_help
            exit 1
            ;;
    esac
    
    # Build and deploy
    print_banner
    
    local current_version=$(get_version)
    local new_version=$(increment_version "$command")
    
    log_info "Current Version: ${current_version}"
    log_info "New Version:     ${new_version}"
    log_info "Target Host:     ${BUILD_HOST}"
    echo ""
    
    check_ssh
    update_version_json "$new_version"
    sync_source
    build_images "$new_version"
    deploy_containers "$new_version"
    clean_images
    verify_deployment
    
    echo ""
    echo -e "${GREEN}╔══════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║${NC}                    ${GREEN}✓ Build & Deploy Complete${NC}                      ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}                                                                  ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Version:  ${YELLOW}${new_version}${NC}                                             ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   API:      ${CYAN}http://${BUILD_HOST}:5000${NC}                          ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Frontend: ${CYAN}http://${BUILD_HOST}${NC}                               ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Swagger:  ${CYAN}http://${BUILD_HOST}:5000/swagger${NC}                   ${GREEN}║${NC}"
    echo -e "${GREEN}╚══════════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --debug|-d)
            DEBUG_MODE="true"
            shift
            ;;
        --host)
            BUILD_HOST="$2"
            shift 2
            ;;
        *)
            COMMAND="$1"
            shift
            ;;
    esac
done

main "${COMMAND:-patch}"
