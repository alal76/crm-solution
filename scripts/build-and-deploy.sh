#!/bin/bash
# Automated build and deploy script for CRM Solution (local/cloud)
# - Reads env-config.json for environment/container info and templates
# - Prompts user for environment/target
# - Generates .env and docker-compose.yml
# - For dev: generates SSL certs and security tokens if needed
# - Builds and deploys frontend/backend

set -e

CONFIG_FILE="$(dirname "$0")/env-config.json"

# Helper: robust prompt for input with default, debug, and fallback
prompt() {
    local prompt_text="$1"
    local default_value="$2"
    local input
    echo "[DEBUG] Prompting: $prompt_text [$default_value] (tty: $(tty))"
    if [ -t 0 ]; then
        read -r -p "$prompt_text [$default_value]: " input
        echo "[DEBUG] User entered: $input"
        if [ -z "$input" ]; then
            input="$default_value"
        fi
    else
        echo "$prompt_text [$default_value]: "
        if ! read -r input; then
            echo "[DEBUG] Read failed or interrupted. Using default: $default_value"
            input="$default_value"
        fi
        echo "[DEBUG] User entered (non-tty): $input"
        if [ -z "$input" ]; then
            input="$default_value"
        fi
    fi
    echo "[DEBUG] Final value for prompt '$prompt_text': $input"
    echo "$input"
}

# Helper: generate random JWT secret
random_secret() {
    openssl rand -base64 48 | tr -d '/+=' | head -c 40
}

# Helper: generate self-signed SSL cert
generate_ssl() {
    local ssl_dir="$1"
    mkdir -p "$ssl_dir"
    openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
        -keyout "$ssl_dir/server.key" -out "$ssl_dir/server.crt" \
        -subj "/CN=localhost"
    echo "SSL certificate and key generated in $ssl_dir"
}


# 1. Ask for environment

echo "[DEBUG] Starting build-and-deploy.sh (PID $$, shell: $SHELL)"
ENV=$(prompt "Select environment (development/production/staging)" "development")
echo "[DEBUG] ENV selected: $ENV"

# 1b. Ask for deploy target (cloud/on-prem)
DEPLOY_TARGET=$(prompt "Select deploy target (cloud/on-prem)" "on-prem")
echo "[DEBUG] DEPLOY_TARGET selected: $DEPLOY_TARGET"

# 1c. Gather target specifics
if [ "$DEPLOY_TARGET" == "on-prem" ]; then
    TARGET_IP=$(prompt "Enter target server IP address" "192.168.0.9")
    echo "[DEBUG] TARGET_IP: $TARGET_IP"
    TARGET_USER=$(prompt "Enter SSH username for target" "root")
    echo "[DEBUG] TARGET_USER: $TARGET_USER"
    SSH_KEY_PATH=$(prompt "Enter SSH private key path (or leave blank for default)" "")
    echo "[DEBUG] SSH_KEY_PATH: $SSH_KEY_PATH"
    if [ -z "$SSH_KEY_PATH" ]; then
        SSH_KEY_ARG=""
    else
        SSH_KEY_ARG="-i $SSH_KEY_PATH"
    fi
    echo "On-prem deployment selected. Target: $TARGET_USER@$TARGET_IP $SSH_KEY_ARG"
elif [ "$DEPLOY_TARGET" == "cloud" ]; then
    CLOUD_PROVIDER=$(prompt "Select cloud provider (azure/aws/gcp)" "azure")
    echo "[DEBUG] CLOUD_PROVIDER: $CLOUD_PROVIDER"
    if [ "$CLOUD_PROVIDER" == "azure" ]; then
        echo "Opening Azure login page..."
        az login
        echo "Azure login complete."
        # Optionally get subscription/context
        az account show
    elif [ "$CLOUD_PROVIDER" == "aws" ]; then
        echo "Opening AWS login (browser or CLI)..."
        aws configure
        echo "AWS credentials configured."
    elif [ "$CLOUD_PROVIDER" == "gcp" ]; then
        echo "Opening GCP login page..."
        gcloud auth login
        echo "GCP login complete."
        gcloud config list
    else
        echo "Unknown cloud provider: $CLOUD_PROVIDER"
        exit 1
    fi
    echo "Cloud deployment selected: $CLOUD_PROVIDER"
else
    echo "Unknown deploy target: $DEPLOY_TARGET"
    exit 1
fi

# 2. Parse env-config.json for selected environment
if ! command -v jq &>/dev/null; then
    echo "jq is required. Please install jq (brew install jq or apt-get install jq)."
    exit 1
fi

ENV_DATA=$(jq -r ".environments.$ENV" "$CONFIG_FILE")
if [ "$ENV_DATA" == "null" ]; then
    echo "Environment '$ENV' not found in $CONFIG_FILE."
    exit 1
fi

API_URL=$(echo "$ENV_DATA" | jq -r '.api_url')
FRONTEND_CONTAINER=$(echo "$ENV_DATA" | jq -r '.frontend_container')
BACKEND_CONTAINER=$(echo "$ENV_DATA" | jq -r '.backend_container')
DOCKER_COMPOSE_FILE=$(echo "$ENV_DATA" | jq -r '.docker_compose_file')
NODE_ENV=$(echo "$ENV_DATA" | jq -r '.node_env')
CLOUD_API_URL=$(echo "$ENV_DATA" | jq -r '.cloud_api_url // empty')
CLOUD_DOMAIN=$(echo "$ENV_DATA" | jq -r '.cloud_domain // empty')


# 3. Select template type (cloud/local) for env and docker-compose
if [ "$ENV" == "production" ] || [ "$ENV" == "staging" ]; then
    TEMPLATE_TYPE="cloud"
else
    TEMPLATE_TYPE="local"
fi

# 3a. Generate .env file for frontend
ENV_TEMPLATE=$(jq -r ".templates.env.$TEMPLATE_TYPE" "$CONFIG_FILE")
if [ "$TEMPLATE_TYPE" == "cloud" ]; then
    ENV_CONTENT=$(echo "$ENV_TEMPLATE" | sed "s|<cloud_api_url>|$CLOUD_API_URL|g" | sed "s|<node_env>|$NODE_ENV|g")
else
    ENV_CONTENT=$(echo "$ENV_TEMPLATE" | sed "s|<api_url>|$API_URL|g" | sed "s|<node_env>|$NODE_ENV|g")
fi

echo "$ENV_CONTENT" > ../CRM.Frontend/.env.generated
cp ../CRM.Frontend/.env.generated ../CRM.Frontend/.env

# 4. Generate docker-compose.yml
DOCKER_TEMPLATE=$(jq -r ".templates.docker_compose.$TEMPLATE_TYPE" "$CONFIG_FILE")
DOCKER_COMPOSE_CONTENT=$(echo "$DOCKER_TEMPLATE" \
    | sed "s|<frontend_container>|$FRONTEND_CONTAINER|g" \
    | sed "s|<backend_container>|$BACKEND_CONTAINER|g" \
    | sed "s|<api_url>|$API_URL|g" \
    | sed "s|<cloud_api_url>|$CLOUD_API_URL|g" \
    | sed "s|<node_env>|$NODE_ENV|g")

echo "$DOCKER_COMPOSE_CONTENT" > ../docker/docker-compose.generated.yml

# 5. For dev: generate SSL certs and JWT secret if needed
if [ "$ENV" == "development" ]; then
    SSL_DIR="../ssl"
    if [ ! -f "$SSL_DIR/server.crt" ] || [ ! -f "$SSL_DIR/server.key" ]; then
        echo "Generating self-signed SSL certificate..."
        generate_ssl "$SSL_DIR"
    fi
    JWT_SECRET=$(random_secret)
    echo "JWT_SECRET=$JWT_SECRET" > ../CRM.Backend/.env.generated
    cp ../CRM.Backend/.env.generated ../CRM.Backend/.env
    echo "Generated JWT secret for dev."
fi


# 6. Build frontend (clean cache and build first)
cd ../CRM.Frontend
export $(grep -v '^#' .env | xargs)
echo "Cleaning frontend build and cache..."
rm -rf build node_modules/.cache
echo "Building frontend..."
npm install
npm run build

# 7. Build and start containers
cd ../
echo "Starting containers using $DOCKER_COMPOSE_FILE ..."
docker-compose -f docker/docker-compose.generated.yml up -d --build

# 8. Deploy frontend build to container (if local)
if [ "$ENV" == "development" ] || [ "$ENV" == "production" ]; then
    echo "Deploying frontend build to $FRONTEND_CONTAINER ..."
    ./scripts/deploy-frontend.sh
fi

echo "Build and deployment complete for $ENV."
#!/bin/bash
# =============================================================================
# ⚠️  DEPRECATED — Use scripts/deploy.sh instead
# =============================================================================
# CRM Solution - Build & Deploy Script v2.0
# Target: 192.168.0.9 (Docker Compose deployment)
# Updated: January 2025
#
# This script is DEPRECATED and will be removed in a future release.
# Use the unified parameterized script instead:
#   ./scripts/deploy.sh --env dev --mode compose
# =============================================================================
echo "⚠️  WARNING: This script is deprecated. Use scripts/deploy.sh --env dev --mode compose instead." >&2

set -e

# Configuration
BUILD_HOST="${BUILD_HOST:-192.168.0.9}"
BUILD_USER="${BUILD_USER:-deploy}"
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
        
        # Pre-build validation: Check .env.production
        echo "Validating .env.production..."
        if [ -f "CRM.Frontend/.env.production" ]; then
            API_URL=\$(grep -E "^REACT_APP_API_URL=" CRM.Frontend/.env.production | cut -d= -f2)
            if [ -n "\$API_URL" ] && [ "\$API_URL" != "" ]; then
                echo "ERROR: .env.production has hardcoded REACT_APP_API_URL: \$API_URL"
                echo "Fix: Set REACT_APP_API_URL= (empty) in CRM.Frontend/.env.production"
                exit 1
            fi
            echo "✓ .env.production is correctly configured (REACT_APP_API_URL is empty)"
        fi
        
        echo "Building API image..."
        docker build -f docker/Dockerfile.backend -t crm-backend:v${version} -t crm-backend:latest . 2>&1 | tail -10
        
        echo "Building Frontend image..."
        # Note: REACT_APP_API_URL is intentionally NOT passed - it should be empty
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
        
        # Ensure database is running (use crm-db as the standard name)
        if ! docker ps | grep -q crm-db; then
            echo "Starting MariaDB..."
            docker rm -f crm-db 2>/dev/null || true
            docker rm -f crm-mariadb 2>/dev/null || true
            docker network create crm-database-network 2>/dev/null || true
            docker run -d --name crm-db \
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
            -e "ConnectionStrings__DefaultConnection=Server=crm-db;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024!;AllowUserVariables=true" \
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
    local uptime_status=$(ssh ${BUILD_USER}@${BUILD_HOST} "curl -s -o /dev/null -w '%{http_code}' http://localhost:3001 2>/dev/null" || echo "000")
    local portainer_status=$(ssh ${BUILD_USER}@${BUILD_HOST} "curl -s -o /dev/null -w '%{http_code}' http://localhost:9000 2>/dev/null" || echo "000")
    
    echo ""
    log_info "Application Services:"
    if [[ "$api_status" == "200" ]]; then
        log_success "API:        http://${BUILD_HOST}:5000 (HTTP ${api_status})"
    else
        log_warning "API:        http://${BUILD_HOST}:5000 (HTTP ${api_status} - starting...)"
    fi
    
    if [[ "$frontend_status" == "200" ]]; then
        log_success "Frontend:   http://${BUILD_HOST} (HTTP ${frontend_status})"
    else
        log_warning "Frontend:   http://${BUILD_HOST} (HTTP ${frontend_status} - starting...)"
    fi
    
    echo ""
    log_info "Monitoring Services:"
    if [[ "$uptime_status" =~ ^(200|302)$ ]]; then
        log_success "Uptime Kuma: http://${BUILD_HOST}:3001 (HTTP ${uptime_status})"
    else
        log_warning "Uptime Kuma: http://${BUILD_HOST}:3001 (HTTP ${uptime_status} - not running)"
    fi
    
    if [[ "$portainer_status" =~ ^(200|307)$ ]]; then
        log_success "Portainer:   http://${BUILD_HOST}:9000 (HTTP ${portainer_status})"
    else
        log_warning "Portainer:   http://${BUILD_HOST}:9000 (HTTP ${portainer_status} - not running)"
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

# Configure monitoring services
configure_monitoring() {
    log_step "Configuring monitoring services..."
    
    # Check if monitoring scripts exist and have dependencies
    if [[ -d "${PROJECT_DIR}/scripts/monitoring" && -f "${PROJECT_DIR}/scripts/monitoring/package.json" ]]; then
        # Install dependencies if needed
        if [[ ! -d "${PROJECT_DIR}/scripts/monitoring/node_modules" ]]; then
            log_info "Installing monitoring script dependencies..."
            cd "${PROJECT_DIR}/scripts/monitoring" && npm install --silent
        fi
        
        # Wait for Uptime Kuma to be fully ready
        log_info "Waiting for Uptime Kuma to be ready..."
        local max_attempts=30
        local attempt=0
        while [[ $attempt -lt $max_attempts ]]; do
            local uptime_status=$(curl -s -o /dev/null -w '%{http_code}' "http://${BUILD_HOST}:3001" 2>/dev/null || echo "000")
            if [[ "$uptime_status" =~ ^(200|302)$ ]]; then
                break
            fi
            sleep 2
            attempt=$((attempt + 1))
        done
        
        if [[ $attempt -lt $max_attempts ]]; then
            # Configure Uptime Kuma monitors
            log_info "Configuring Uptime Kuma monitors..."
            cd "${PROJECT_DIR}/scripts/monitoring"
            UPTIME_KUMA_HOST="${BUILD_HOST}" \
            UPTIME_KUMA_USER="admin" \
            UPTIME_KUMA_PASS="CrmAdmin2024!" \
            node configure-uptime-kuma.js 2>&1 | grep -E "^[✓⚠✗📊📄═]|Created:|Skipping|Monitor Summary|complete" || true
            log_success "Uptime Kuma monitors configured"
        else
            log_warning "Uptime Kuma not ready, skipping monitor configuration"
        fi
    else
        log_warning "Monitoring scripts not found, skipping configuration"
    fi
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
    configure_monitoring
    
    echo ""
    echo -e "${GREEN}╔══════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║${NC}                    ${GREEN}✓ Build & Deploy Complete${NC}                      ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}                                                                  ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Version:    ${YELLOW}${new_version}${NC}                                             ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   API:        ${CYAN}http://${BUILD_HOST}:5000${NC}                          ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Frontend:   ${CYAN}http://${BUILD_HOST}${NC}                               ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Swagger:    ${CYAN}http://${BUILD_HOST}:5000/swagger${NC}                   ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}                                                                  ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   ${MAGENTA}Monitoring:${NC}                                                    ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Uptime Kuma: ${CYAN}http://${BUILD_HOST}:3001${NC}                          ${GREEN}║${NC}"
    echo -e "${GREEN}║${NC}   Portainer:   ${CYAN}http://${BUILD_HOST}:9000${NC}                          ${GREEN}║${NC}"
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
