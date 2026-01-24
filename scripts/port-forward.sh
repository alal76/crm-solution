#!/bin/bash
# Persistent Port Forward Script for CRM Development
# Automatically reconnects when port-forward breaks

set -e

NAMESPACE="${1:-crm-app}"
LOG_DIR="/tmp/crm-port-forward"
mkdir -p "$LOG_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== CRM Port Forward Service ===${NC}"
echo "Namespace: $NAMESPACE"
echo "Press Ctrl+C to stop"
echo ""

# Kill existing port-forwards
cleanup() {
    echo -e "\n${YELLOW}Stopping port forwards...${NC}"
    pkill -f "kubectl port-forward.*$NAMESPACE" 2>/dev/null || true
    exit 0
}
trap cleanup SIGINT SIGTERM

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}Error: kubectl not found${NC}"
    exit 1
fi

# Check if namespace exists
if ! kubectl get namespace "$NAMESPACE" &> /dev/null; then
    echo -e "${RED}Error: Namespace '$NAMESPACE' not found${NC}"
    exit 1
fi

# Kill existing port-forwards for this namespace
pkill -f "kubectl port-forward.*$NAMESPACE" 2>/dev/null || true
sleep 2

start_port_forward() {
    local SERVICE=$1
    local PORTS=$2
    local LOG_FILE="$LOG_DIR/${SERVICE}.log"
    
    echo -e "${YELLOW}Starting port-forward for $SERVICE ($PORTS)...${NC}"
    
    # Start port-forward in background
    kubectl port-forward "svc/$SERVICE" $PORTS -n "$NAMESPACE" > "$LOG_FILE" 2>&1 &
    local PID=$!
    
    # Wait a moment and check if it started
    sleep 2
    if kill -0 $PID 2>/dev/null; then
        echo -e "${GREEN}✓ $SERVICE port-forward started (PID: $PID)${NC}"
        return 0
    else
        echo -e "${RED}✗ Failed to start $SERVICE port-forward${NC}"
        cat "$LOG_FILE"
        return 1
    fi
}

monitor_and_restart() {
    while true; do
        # Check API port-forward
        if ! pgrep -f "kubectl port-forward.*svc/crm-api.*$NAMESPACE" > /dev/null; then
            echo -e "${YELLOW}[$(date '+%H:%M:%S')] API port-forward died, restarting...${NC}"
            start_port_forward "crm-api" "5000:5000 5001:5001"
        fi
        
        # Check Frontend port-forward (if using NodePort, this may not be needed)
        # Uncomment if you want to forward frontend too:
        # if ! pgrep -f "kubectl port-forward.*svc/crm-frontend.*$NAMESPACE" > /dev/null; then
        #     echo -e "${YELLOW}[$(date '+%H:%M:%S')] Frontend port-forward died, restarting...${NC}"
        #     start_port_forward "crm-frontend" "3000:80"
        # fi
        
        # Wait before next check
        sleep 10
    done
}

# Initial startup
echo "Starting port forwards..."
echo ""

# Start API port-forward (HTTP on 5000, HTTPS on 5001)
start_port_forward "crm-api" "5000:5000 5001:5001"

echo ""
echo -e "${GREEN}Port forwards active:${NC}"
echo "  API HTTP:  http://localhost:5000"
echo "  API HTTPS: https://localhost:5001"
echo "  Frontend:  http://$(minikube ip):30080 (NodePort)"
echo ""
echo "Logs: $LOG_DIR/"
echo ""

# Monitor and restart if needed
monitor_and_restart
