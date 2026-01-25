#!/bin/bash
#===============================================================================
# Stop CRM Local Access
#===============================================================================

echo "Stopping all CRM port forwards..."

# Kill port-forward processes
pkill -f "kubectl port-forward" 2>/dev/null || true
pkill -f "socat.*:80" 2>/dev/null || true
pkill -f "socat.*:3000" 2>/dev/null || true  
pkill -f "socat.*:5000" 2>/dev/null || true

# Kill processes on ports
lsof -ti:80 2>/dev/null | xargs kill -9 2>/dev/null || true
lsof -ti:3000 2>/dev/null | xargs kill -9 2>/dev/null || true
lsof -ti:5000 2>/dev/null | xargs kill -9 2>/dev/null || true

# Clean up PID file
rm -f /tmp/crm-port-forwards.pid

echo "All port forwards stopped."
