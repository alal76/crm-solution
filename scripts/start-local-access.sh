#!/bin/bash
#===============================================================================
# CRM Local Access Script
# Provides reliable localhost access to Kubernetes services
# Uses kubectl port-forward with auto-restart on failure
#===============================================================================

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
PID_FILE="/tmp/crm-port-forwards.pid"
LOG_DIR="/tmp/crm-logs"

# Service configuration
FRONTEND_PORT=3000
API_PORT=5000

log_info() { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[SUCCESS]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

check_prerequisites() {
    log_info "Checking prerequisites..."
    
    # Check minikube
    if ! command -v minikube &> /dev/null; then
        log_error "minikube not found. Please install minikube."
        exit 1
    fi
    
    # Check minikube status
    if ! minikube status 2>/dev/null | grep -q "Running"; then
        log_error "minikube is not running. Start with: minikube start"
        exit 1
    fi
    
    # Check kubectl
    if ! command -v kubectl &> /dev/null; then
        log_error "kubectl not found."
        exit 1
    fi
    
    log_success "Prerequisites check passed"
}

check_services() {
    log_info "Checking CRM services in Kubernetes..."
    
    # Check pods
    if ! kubectl get pods -n crm-app 2>/dev/null | grep -q "Running"; then
        log_error "No running pods found in crm-app namespace"
        log_info "Apply manifests with: kubectl apply -f kubernetes/local/"
        exit 1
    fi
    
    kubectl get pods -n crm-app
    echo ""
    kubectl get svc -n crm-app
    log_success "Services are running"
}

stop_existing() {
    log_info "Stopping existing port forwards..."
    
    # Kill any existing port-forward processes
    pkill -f "kubectl port-forward.*crm-" 2>/dev/null || true
    
    # Kill processes on target ports
    lsof -ti:${FRONTEND_PORT} 2>/dev/null | xargs kill -9 2>/dev/null || true
    lsof -ti:${API_PORT} 2>/dev/null | xargs kill -9 2>/dev/null || true
    lsof -ti:80 2>/dev/null | xargs kill -9 2>/dev/null || true
    
    # Clean up PID file
    rm -f "$PID_FILE"
    
    sleep 2
    log_success "Cleaned up existing processes"
}

start_port_forward() {
    local service=$1
    local local_port=$2
    local remote_port=$3
    local log_file="${LOG_DIR}/${service}.log"
    
    mkdir -p "$LOG_DIR"
    
    log_info "Starting port-forward: localhost:${local_port} -> ${service}:${remote_port}"
    
    # Start port-forward with nohup to keep it running
    nohup kubectl port-forward "svc/${service}" "${local_port}:${remote_port}" -n crm-app \
        > "$log_file" 2>&1 &
    
    echo $! >> "$PID_FILE"
    log_success "Port-forward started for ${service} (PID: $!)"
}

start_all_forwards() {
    log_info "Starting reliable port forwards..."
    
    mkdir -p "$LOG_DIR"
    
    # API: localhost:5000 -> crm-api:5000
    start_port_forward "crm-api" ${API_PORT} 5000
    
    # Frontend: localhost:3000 -> crm-frontend:80
    start_port_forward "crm-frontend" ${FRONTEND_PORT} 80
    
    sleep 3
    log_success "All port forwards started"
}

start_daemon() {
    log_info "Starting port-forward daemon (auto-restart on failure)..."
    
    mkdir -p "$LOG_DIR"
    
    # Create a daemon script that monitors and restarts port-forwards
    cat > /tmp/crm-port-forward-daemon.sh << 'DAEMON_EOF'
#!/bin/bash
LOG_DIR="/tmp/crm-logs"
mkdir -p "$LOG_DIR"

restart_forward() {
    local service=$1
    local local_port=$2
    local remote_port=$3
    
    # Kill existing
    pkill -f "kubectl port-forward.*${service}.*${local_port}" 2>/dev/null || true
    lsof -ti:${local_port} 2>/dev/null | xargs kill -9 2>/dev/null || true
    sleep 1
    
    # Start new
    nohup kubectl port-forward "svc/${service}" "${local_port}:${remote_port}" -n crm-app \
        >> "${LOG_DIR}/${service}.log" 2>&1 &
    echo "$(date): Restarted ${service} on port ${local_port}" >> "${LOG_DIR}/daemon.log"
}

check_and_restart() {
    # Check API
    if ! curl -s --max-time 2 http://localhost:5000/health > /dev/null 2>&1; then
        restart_forward "crm-api" 5000 5000
    fi
    
    # Check Frontend
    if ! curl -s --max-time 2 http://localhost:3000 > /dev/null 2>&1; then
        restart_forward "crm-frontend" 3000 80
    fi
}

# Initial start
restart_forward "crm-api" 5000 5000
restart_forward "crm-frontend" 3000 80

# Monitor loop
while true; do
    sleep 30
    check_and_restart
done
DAEMON_EOF

    chmod +x /tmp/crm-port-forward-daemon.sh
    
    # Start daemon
    nohup /tmp/crm-port-forward-daemon.sh > "${LOG_DIR}/daemon.log" 2>&1 &
    echo $! > /tmp/crm-daemon.pid
    
    sleep 5
    log_success "Daemon started (PID: $(cat /tmp/crm-daemon.pid))"
}

stop_daemon() {
    log_info "Stopping daemon..."
    if [ -f /tmp/crm-daemon.pid ]; then
        kill $(cat /tmp/crm-daemon.pid) 2>/dev/null || true
        rm -f /tmp/crm-daemon.pid
    fi
    stop_existing
    log_success "Daemon stopped"
}

verify_access() {
    log_info "Verifying service access..."
    
    # Wait for services to be ready
    sleep 3
    
    local api_ok=false
    local frontend_ok=false
    
    # Check API health (retry a few times)
    for i in {1..5}; do
        if curl -s --max-time 3 http://localhost:${API_PORT}/health > /dev/null 2>&1; then
            api_ok=true
            break
        fi
        sleep 2
    done
    
    if [ "$api_ok" = true ]; then
        log_success "API accessible at http://localhost:${API_PORT}"
    else
        log_warn "API health check failed - may need more time to start"
    fi
    
    # Check Frontend
    for i in {1..3}; do
        if curl -s --max-time 3 http://localhost:${FRONTEND_PORT} > /dev/null 2>&1; then
            frontend_ok=true
            break
        fi
        sleep 2
    done
    
    if [ "$frontend_ok" = true ]; then
        log_success "Frontend accessible at http://localhost:${FRONTEND_PORT}"
    else
        log_warn "Frontend check failed - may need more time to start"
    fi
}

show_access_info() {
    echo ""
    echo "=============================================="
    echo -e "${GREEN}CRM Application Access Points${NC}"
    echo "=============================================="
    echo ""
    echo -e "${BLUE}Frontend (Browser):${NC}"
    echo "  • http://localhost:3000"
    echo ""
    echo -e "${BLUE}API:${NC}"
    echo "  • http://localhost:5000"
    echo "  • Health: http://localhost:5000/health"
    echo ""
    echo -e "${BLUE}Test credentials:${NC}"
    echo "  • Email: abhi.lal@gmail.com"
    echo "  • Password: Admin123!"
    echo ""
    echo -e "${BLUE}Logs:${NC}"
    echo "  • API: tail -f /tmp/crm-logs/crm-api.log"
    echo "  • Frontend: tail -f /tmp/crm-logs/crm-frontend.log"
    echo "  • Daemon: tail -f /tmp/crm-logs/daemon.log"
    echo ""
    echo "=============================================="
    echo -e "${YELLOW}To stop: ./scripts/start-local-access.sh stop${NC}"
    echo "=============================================="
}

# Main execution
case "${1:-start}" in
    start)
        check_prerequisites
        check_services
        stop_existing
        start_all_forwards
        verify_access
        show_access_info
        ;;
    daemon)
        check_prerequisites
        check_services
        stop_daemon
        start_daemon
        verify_access
        show_access_info
        ;;
    stop)
        stop_daemon
        stop_existing
        log_success "All port forwards stopped"
        ;;
    status)
        check_services
        echo ""
        log_info "Checking port forwards..."
        echo "Active processes:"
        lsof -i :3000 -i :5000 2>/dev/null | head -10 || log_warn "No active forwards found"
        echo ""
        log_info "Testing connectivity..."
        curl -s --max-time 2 http://localhost:5000/health && echo " - API OK" || echo "API unreachable"
        curl -s --max-time 2 http://localhost:3000 > /dev/null && echo "Frontend OK" || echo "Frontend unreachable"
        ;;
    restart)
        stop_existing
        check_prerequisites
        check_services
        start_all_forwards
        verify_access
        show_access_info
        ;;
    *)
        echo "Usage: $0 {start|stop|status|restart|daemon}"
        echo ""
        echo "  start   - Start port forwards (foreground processes)"
        echo "  daemon  - Start with auto-restart daemon"
        echo "  stop    - Stop all port forwards and daemon"
        echo "  status  - Show service and connection status"
        echo "  restart - Restart all port forwards"
        exit 1
        ;;
esac
