#!/bin/bash
# =============================================================================
# CRM Solution - Monitoring Setup Script
# =============================================================================
# Sets up and configures Uptime Kuma and Portainer monitoring services
#
# Usage:
#   ./scripts/setup-monitoring.sh [options]
#
# Options:
#   --deploy        Deploy monitoring containers (default)
#   --configure     Configure monitoring services (requires npm)
#   --status        Show monitoring status only
#   --reset         Reset and reconfigure monitoring
#   --help          Show this help message
#
# Services:
#   - Uptime Kuma (port 3001): Service health monitoring
#   - Portainer (port 9000): Container management
# =============================================================================

set -e

# Script location
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

# Load configuration
CONFIG_FILE="${PROJECT_DIR}/config/infrastructure.env"
if [ -f "$CONFIG_FILE" ]; then
    set -a
    source "$CONFIG_FILE"
    set +a
fi

# Default configuration
BUILD_SERVER="${BUILD_SERVER:-192.168.0.9}"
BUILD_USER="${BUILD_USER:-root}"
PORT_UPTIME_KUMA="${PORT_UPTIME_KUMA:-3001}"
PORT_PORTAINER="${PORT_PORTAINER:-9000}"
UPTIME_KUMA_ADMIN_USER="${UPTIME_KUMA_ADMIN_USER:-admin}"
UPTIME_KUMA_ADMIN_PASS="${UPTIME_KUMA_ADMIN_PASS:-CrmAdmin2024!}"
PORTAINER_ADMIN_USER="${PORTAINER_ADMIN_USER:-admin}"
PORTAINER_ADMIN_PASS="${PORTAINER_ADMIN_PASS:-CrmAdmin2024!}"

# Colors
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

print_banner() {
    echo ""
    echo -e "${CYAN}╔══════════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${CYAN}║${NC}           ${MAGENTA}CRM Solution - Monitoring Setup${NC}                        ${CYAN}║${NC}"
    echo -e "${CYAN}║${NC}              Target: ${YELLOW}${BUILD_SERVER}${NC}                               ${CYAN}║${NC}"
    echo -e "${CYAN}╚══════════════════════════════════════════════════════════════════╝${NC}"
    echo ""
}

# Check if running locally or remote
is_local() {
    [[ "$BUILD_SERVER" == "localhost" || "$BUILD_SERVER" == "127.0.0.1" ]]
}

run_cmd() {
    if is_local; then
        eval "$1"
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "$1"
    fi
}

# Deploy monitoring containers
deploy_monitoring() {
    log_step "Deploying monitoring services..."
    
    local compose_file="docker/docker-compose.unified.yml"
    
    if is_local; then
        cd "$PROJECT_DIR"
        docker compose -f "$compose_file" up -d uptime-kuma portainer
    else
        # Sync source first
        log_step "Syncing configuration to ${BUILD_SERVER}..."
        rsync -avz --delete \
            --exclude 'node_modules' \
            --exclude '.git' \
            "${PROJECT_DIR}/" ${BUILD_USER}@${BUILD_SERVER}:/opt/crm/source/ 2>&1 | tail -3
        
        ssh ${BUILD_USER}@${BUILD_SERVER} "cd /opt/crm/source && docker compose -f $compose_file up -d uptime-kuma portainer"
    fi
    
    log_success "Monitoring containers deployed"
}

# Setup Portainer
setup_portainer() {
    log_step "Configuring Portainer..."
    
    local portainer_url="http://localhost:${PORT_PORTAINER}"
    if ! is_local; then
        portainer_url="http://${BUILD_SERVER}:${PORT_PORTAINER}"
    fi
    
    # Wait for Portainer to be ready
    local max_wait=30
    local waited=0
    while [ $waited -lt $max_wait ]; do
        local status=$(curl -s -o /dev/null -w '%{http_code}' "${portainer_url}/api/status" 2>/dev/null || echo "000")
        if [[ "$status" =~ ^(200|307)$ ]]; then
            break
        fi
        sleep 2
        waited=$((waited + 2))
    done
    
    # Check if admin already exists
    local admin_check=$(curl -s "${portainer_url}/api/users/admin/check" 2>/dev/null || echo '{}')
    
    if echo "$admin_check" | grep -q '"adminInitialized":false'; then
        log_step "Creating Portainer admin user..."
        local result=$(curl -s -X POST "${portainer_url}/api/users/admin/init" \
            -H "Content-Type: application/json" \
            -d "{\"Username\":\"${PORTAINER_ADMIN_USER}\",\"Password\":\"${PORTAINER_ADMIN_PASS}\"}" 2>/dev/null)
        
        if echo "$result" | grep -q '"Id"'; then
            log_success "Portainer admin user created"
            
            # Login and add local Docker environment
            local auth=$(curl -s -X POST "${portainer_url}/api/auth" \
                -H "Content-Type: application/json" \
                -d "{\"Username\":\"${PORTAINER_ADMIN_USER}\",\"Password\":\"${PORTAINER_ADMIN_PASS}\"}" 2>/dev/null)
            
            local token=$(echo "$auth" | grep -o '"jwt":"[^"]*"' | cut -d'"' -f4)
            
            if [ -n "$token" ]; then
                curl -s -X POST "${portainer_url}/api/endpoints" \
                    -H "Authorization: Bearer $token" \
                    -F "Name=local" \
                    -F "EndpointCreationType=1" 2>/dev/null
                log_success "Local Docker environment added"
            fi
        else
            log_warning "Portainer admin creation returned: $result"
        fi
    else
        log_info "Portainer admin already configured"
    fi
}

# Setup Uptime Kuma
setup_uptime_kuma() {
    log_step "Configuring Uptime Kuma..."
    
    # Check if Node.js is available
    if ! command -v node &> /dev/null; then
        log_warning "Node.js not found. Please configure Uptime Kuma manually at http://${BUILD_SERVER}:${PORT_UPTIME_KUMA}"
        return 0
    fi
    
    local uptime_url="http://localhost:${PORT_UPTIME_KUMA}"
    if ! is_local; then
        uptime_url="http://${BUILD_SERVER}:${PORT_UPTIME_KUMA}"
    fi
    
    # Wait for Uptime Kuma to be ready
    local max_wait=30
    local waited=0
    while [ $waited -lt $max_wait ]; do
        local status=$(curl -s -o /dev/null -w '%{http_code}' "${uptime_url}" 2>/dev/null || echo "000")
        if [[ "$status" =~ ^(200|302)$ ]]; then
            break
        fi
        sleep 2
        waited=$((waited + 2))
    done
    
    # Create setup script
    local setup_script=$(mktemp)
    cat > "$setup_script" << 'SETUP_SCRIPT'
const { io } = require("socket.io-client");

const url = process.env.UPTIME_URL || "http://localhost:3001";
const adminUser = process.env.ADMIN_USER || "admin";
const adminPass = process.env.ADMIN_PASS || "CrmAdmin2024!";

const socket = io(url, { transports: ["websocket"], reconnection: false });

socket.on("connect", () => {
    console.log("Connected to Uptime Kuma");
});

setTimeout(() => {
    socket.emit("needSetup", (needSetup) => {
        if (needSetup) {
            console.log("Setting up admin user...");
            socket.emit("setup", adminUser, adminPass, (res) => {
                if (res && res.ok) {
                    console.log("Admin created, adding monitors...");
                    addMonitors();
                } else {
                    console.log("Setup failed:", res);
                    process.exit(1);
                }
            });
        } else {
            console.log("Already configured");
            socket.emit("login", { username: adminUser, password: adminPass, token: "" }, (res) => {
                if (res && res.ok) {
                    addMonitors();
                } else {
                    console.log("Login failed - monitors may already exist");
                    process.exit(0);
                }
            });
        }
    });
}, 2000);

function addMonitors() {
    const monitors = [
        { name: "CRM API Health", type: "http", url: "http://crm-api:5000/api/health", interval: 60 },
        { name: "CRM Frontend", type: "http", url: "http://crm-frontend:80", interval: 60 },
        { name: "MariaDB", type: "port", hostname: "crm-mariadb", port: 3306, interval: 60 },
        { name: "Redis Cache", type: "port", hostname: "crm-redis", port: 6379, interval: 60 }
    ];
    
    let done = 0;
    monitors.forEach((m) => {
        socket.emit("add", {
            type: m.type, name: m.name, url: m.url || "",
            hostname: m.hostname || "", port: m.port || 0,
            interval: m.interval, active: true, maxretries: 3,
            retryInterval: 60, accepted_statuscodes: ["200-299"], method: "GET"
        }, (res) => {
            console.log("  " + m.name + ":", res && res.ok ? "Added" : (res ? res.msg : "fail"));
            done++;
            if (done === monitors.length) {
                console.log("All monitors configured!");
                process.exit(0);
            }
        });
    });
}

socket.on("connect_error", (err) => {
    console.log("Connection error:", err.message);
    process.exit(1);
});

setTimeout(() => process.exit(0), 30000);
SETUP_SCRIPT
    
    # Install socket.io-client if needed
    local tmp_dir=$(mktemp -d)
    cd "$tmp_dir"
    npm init -y >/dev/null 2>&1
    npm install socket.io-client >/dev/null 2>&1
    
    # Run setup
    UPTIME_URL="${uptime_url}" \
    ADMIN_USER="${UPTIME_KUMA_ADMIN_USER}" \
    ADMIN_PASS="${UPTIME_KUMA_ADMIN_PASS}" \
    node "$setup_script" 2>&1
    
    # Cleanup
    rm -rf "$tmp_dir" "$setup_script"
    
    log_success "Uptime Kuma configured"
}

# Show status
show_status() {
    log_step "Monitoring Services Status:"
    echo ""
    
    local uptime_status=""
    local portainer_status=""
    
    if is_local; then
        uptime_status=$(curl -s -o /dev/null -w '%{http_code}' "http://localhost:${PORT_UPTIME_KUMA}" 2>/dev/null || echo "000")
        portainer_status=$(curl -s -o /dev/null -w '%{http_code}' "http://localhost:${PORT_PORTAINER}" 2>/dev/null || echo "000")
    else
        uptime_status=$(curl -s -o /dev/null -w '%{http_code}' "http://${BUILD_SERVER}:${PORT_UPTIME_KUMA}" 2>/dev/null || echo "000")
        portainer_status=$(curl -s -o /dev/null -w '%{http_code}' "http://${BUILD_SERVER}:${PORT_PORTAINER}" 2>/dev/null || echo "000")
    fi
    
    if [[ "$uptime_status" =~ ^(200|302)$ ]]; then
        log_success "Uptime Kuma:  http://${BUILD_SERVER}:${PORT_UPTIME_KUMA} (HTTP ${uptime_status})"
    else
        log_error "Uptime Kuma:  http://${BUILD_SERVER}:${PORT_UPTIME_KUMA} (HTTP ${uptime_status} - not running)"
    fi
    
    if [[ "$portainer_status" =~ ^(200|307)$ ]]; then
        log_success "Portainer:    http://${BUILD_SERVER}:${PORT_PORTAINER} (HTTP ${portainer_status})"
    else
        log_error "Portainer:    http://${BUILD_SERVER}:${PORT_PORTAINER} (HTTP ${portainer_status} - not running)"
    fi
    
    echo ""
    log_info "Container Status:"
    if is_local; then
        docker ps --filter 'name=uptime-kuma' --filter 'name=portainer' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}' 2>/dev/null || true
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "docker ps --filter 'name=uptime-kuma' --filter 'name=portainer' --format 'table {{.Names}}\t{{.Status}}\t{{.Ports}}'" 2>/dev/null || true
    fi
    
    echo ""
    log_info "Credentials:"
    echo "  Uptime Kuma: ${UPTIME_KUMA_ADMIN_USER} / ${UPTIME_KUMA_ADMIN_PASS}"
    echo "  Portainer:   ${PORTAINER_ADMIN_USER} / ${PORTAINER_ADMIN_PASS}"
}

# Reset monitoring
reset_monitoring() {
    log_warning "Resetting monitoring services..."
    
    if is_local; then
        docker stop uptime-kuma portainer 2>/dev/null || true
        docker rm uptime-kuma portainer 2>/dev/null || true
        docker volume rm uptime_kuma_data portainer_data 2>/dev/null || true
    else
        ssh ${BUILD_USER}@${BUILD_SERVER} "docker stop uptime-kuma portainer 2>/dev/null || true; docker rm uptime-kuma portainer 2>/dev/null || true; docker volume rm uptime_kuma_data portainer_data 2>/dev/null || true"
    fi
    
    log_success "Monitoring services reset"
}

# Print help
print_help() {
    echo "CRM Solution - Monitoring Setup Script"
    echo ""
    echo "Usage: $0 [options]"
    echo ""
    echo "Options:"
    echo "  --deploy        Deploy monitoring containers (default)"
    echo "  --configure     Configure monitoring services"
    echo "  --status        Show monitoring status"
    echo "  --reset         Reset monitoring (removes data)"
    echo "  --full          Full setup: deploy + configure"
    echo "  --help          Show this help"
    echo ""
    echo "Environment Variables:"
    echo "  BUILD_SERVER              Target server (default: 192.168.0.9)"
    echo "  PORT_UPTIME_KUMA          Uptime Kuma port (default: 3001)"
    echo "  PORT_PORTAINER            Portainer port (default: 9000)"
    echo "  UPTIME_KUMA_ADMIN_USER    Admin username (default: admin)"
    echo "  UPTIME_KUMA_ADMIN_PASS    Admin password (default: CrmAdmin2024!)"
    echo ""
}

# Main
main() {
    local action="${1:---deploy}"
    
    case "$action" in
        --help|-h)
            print_help
            exit 0
            ;;
        --status|-s)
            print_banner
            show_status
            exit 0
            ;;
        --deploy|-d)
            print_banner
            deploy_monitoring
            sleep 5
            show_status
            ;;
        --configure|-c)
            print_banner
            setup_portainer
            setup_uptime_kuma
            show_status
            ;;
        --reset|-r)
            print_banner
            reset_monitoring
            ;;
        --full|-f)
            print_banner
            deploy_monitoring
            sleep 10
            setup_portainer
            setup_uptime_kuma
            show_status
            ;;
        *)
            log_error "Unknown option: $action"
            print_help
            exit 1
            ;;
    esac
}

main "$@"
