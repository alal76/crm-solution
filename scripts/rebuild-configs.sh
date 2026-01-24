#!/bin/bash
# Script to rebuild module field configurations
# This script deletes existing configs from the database and restarts the API to reseed

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

NAMESPACE="${CRM_NAMESPACE:-crm-app}"
DB_PASSWORD="${MARIADB_ROOT_PASSWORD:-RootPass@Dev2024}"
DATABASE="${CRM_DATABASE:-crm_db}"

print_step() {
    echo -e "\n${CYAN}════════════════════════════════════════════════════════════════${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}════════════════════════════════════════════════════════════════${NC}\n"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    print_error "kubectl is not installed or not in PATH"
    exit 1
fi

# Check if the namespace exists
if ! kubectl get namespace "$NAMESPACE" &> /dev/null; then
    print_error "Namespace $NAMESPACE does not exist"
    exit 1
fi

print_step "Rebuilding Module Field Configurations"

# Get current count
current_count=$(kubectl exec deployment/crm-db -n "$NAMESPACE" -- \
    mysql -u root -p"$DB_PASSWORD" "$DATABASE" -N -e \
    "SELECT COUNT(*) FROM ModuleFieldConfigurations" 2>/dev/null || echo "0")

echo "Current configuration count: $current_count"

# Delete existing configurations
print_step "Deleting existing configurations..."
kubectl exec deployment/crm-db -n "$NAMESPACE" -- \
    mysql -u root -p"$DB_PASSWORD" "$DATABASE" -e \
    "DELETE FROM ModuleFieldConfigurations" 2>/dev/null

print_success "Existing configurations deleted"

# Restart the API to trigger reseed
print_step "Restarting API to trigger reseed..."
kubectl rollout restart deployment/crm-api -n "$NAMESPACE"
kubectl rollout status deployment/crm-api -n "$NAMESPACE" --timeout=120s

# Wait for the seed to complete
sleep 5

# Get new count
new_count=$(kubectl exec deployment/crm-db -n "$NAMESPACE" -- \
    mysql -u root -p"$DB_PASSWORD" "$DATABASE" -N -e \
    "SELECT COUNT(*) FROM ModuleFieldConfigurations" 2>/dev/null || echo "0")

echo ""
print_success "Module field configurations rebuilt!"
echo "  Previous count: $current_count"
echo "  New count: $new_count"

# Also update demo database if it exists
if kubectl exec deployment/crm-db -n "$NAMESPACE" -- \
    mysql -u root -p"$DB_PASSWORD" -N -e \
    "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'crm_demodb'" 2>/dev/null | grep -q crm_demodb; then
    
    print_step "Updating demo database configurations..."
    kubectl exec deployment/crm-db -n "$NAMESPACE" -- \
        mysql -u root -p"$DB_PASSWORD" crm_demodb -e \
        "DELETE FROM ModuleFieldConfigurations" 2>/dev/null || true
    print_success "Demo database configurations cleared (will be seeded on next access)"
fi

echo ""
print_success "Configuration rebuild complete!"
echo ""
echo "Note: You may need to restart port forwarding and refresh your browser."
