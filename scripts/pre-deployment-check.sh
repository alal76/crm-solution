#!/bin/bash
#
# CRM Deployment - Pre-Deployment Validation Check
# This script verifies all prerequisites before initiating full deployment
#

set -e

TARGET_SERVER="${1:-192.168.0.9}"
SSH_USER="root"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_check() {
    echo -ne "${BLUE}[CHECK]${NC} $1 "
}

log_pass() {
    echo -e "${GREEN}[✓]${NC} $1"
}

log_fail() {
    echo -e "${RED}[✗]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[⚠]${NC} $1"
}

check_count=0
pass_count=0
fail_count=0
warn_count=0

# Helper function to track checks
run_check() {
    local check_name=$1
    local check_cmd=$2
    
    ((check_count++))
    log_check "$check_name"
    
    if eval "$check_cmd" >/dev/null 2>&1; then
        log_pass "$check_name"
        ((pass_count++))
        return 0
    else
        log_fail "$check_name"
        ((fail_count++))
        return 1
    fi
}

run_warn_check() {
    local check_name=$1
    local check_cmd=$2
    
    ((check_count++))
    log_check "$check_name"
    
    if eval "$check_cmd" >/dev/null 2>&1; then
        log_pass "$check_name"
        ((pass_count++))
        return 0
    else
        log_warn "$check_name"
        ((warn_count++))
        return 1
    fi
}

echo "╔════════════════════════════════════════════════════════════════╗"
echo "║  CRM Solution - Pre-Deployment Validation Check               ║"
echo "║  Target Server: $TARGET_SERVER                                    ║"
echo "║  Time: $(date '+%Y-%m-%d %H:%M:%S')                              ║"
echo "╚════════════════════════════════════════════════════════════════╝"
echo ""

# ============================================================================
# LOCAL ENVIRONMENT CHECKS
# ============================================================================
echo "📋 LOCAL ENVIRONMENT CHECKS"
echo "─────────────────────────────────────────"

run_check "Docker installed" "command -v docker"
run_check "Docker daemon running" "docker ps >/dev/null"
run_check "Docker Compose available" "docker compose --version"
run_check "SSH client available" "command -v ssh"
run_check "Solution directory exists" "[ -d './CRM.Backend' ]"
run_check "Dockerfile.backend exists" "[ -f 'docker/Dockerfile.backend' ]"
run_check "Dockerfile.frontend exists" "[ -f 'docker/Dockerfile.frontend' ]"
run_check "docker-compose.yml exists" "[ -f 'docker/docker-compose.yml' ]"

echo ""

# ============================================================================
# NETWORK & SSH CHECKS
# ============================================================================
echo "🌐 NETWORK & SSH CHECKS"
echo "─────────────────────────────────────────"

run_check "Network connectivity to $TARGET_SERVER" "ping -c 1 -t 2 $TARGET_SERVER >/dev/null 2>&1 || nc -zw 1 $TARGET_SERVER 22 >/dev/null"
run_check "SSH connectivity" "ssh -o ConnectTimeout=5 -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER 'echo ok' >/dev/null"

echo ""

# ============================================================================
# REMOTE SERVER CHECKS
# ============================================================================
echo "🖥️  REMOTE SERVER CHECKS"
echo "─────────────────────────────────────────"

run_check "Docker installed on server" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER 'command -v docker' >/dev/null"
run_check "Docker daemon running on server" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER 'docker ps >/dev/null' >/dev/null"
run_check "Docker Compose on server" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER 'docker compose --version' >/dev/null"

# Check disk space
log_check "Sufficient disk space on server (>10GB)"
DISK_SIZE=$(ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER "df / | awk 'NR==2 {print \$4}'" 2>/dev/null || echo "0")
DISK_GB=$((DISK_SIZE / 1024 / 1024))

if [[ "$DISK_GB" -gt 10 ]]; then
    log_pass "Sufficient disk space on server (>10GB) - Available: ${DISK_GB}GB"
    ((pass_count++))
elif [[ "$DISK_GB" -gt 5 ]]; then
    log_warn "Limited disk space on server - Available: ${DISK_GB}GB (recommended: >10GB)"
    ((warn_count++))
else
    log_fail "Insufficient disk space on server - Available: ${DISK_GB}GB (required: >10GB)"
    ((fail_count++))
fi
((check_count++))

# Check memory
log_check "Sufficient memory on server (>4GB)"
MEM_SIZE=$(ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER "free | awk 'NR==2 {print \$2}'" 2>/dev/null || echo "0")
MEM_GB=$((MEM_SIZE / 1024 / 1024))

if [[ "$MEM_GB" -gt 4 ]]; then
    log_pass "Sufficient memory on server (>4GB) - Available: ${MEM_GB}GB"
    ((pass_count++))
elif [[ "$MEM_GB" -gt 2 ]]; then
    log_warn "Limited memory on server - Available: ${MEM_GB}GB (recommended: >4GB)"
    ((warn_count++))
else
    log_fail "Insufficient memory on server - Available: ${MEM_GB}GB (required: >2GB)"
    ((fail_count++))
fi
((check_count++))

# Check required directories can be created
run_check "Can create deployment directory" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER 'mkdir -p /opt/crm-deployment 2>/dev/null && [ -d /opt/crm-deployment ]'"

echo ""

# ============================================================================
# PORT AVAILABILITY CHECKS
# ============================================================================
echo "🔌 PORT AVAILABILITY CHECKS"
echo "─────────────────────────────────────────"

run_warn_check "Port 5000 available (API)" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER '! netstat -tlnp 2>/dev/null | grep -q :5000'"
run_warn_check "Port 80 available (Frontend)" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER '! netstat -tlnp 2>/dev/null | grep -q :80'"
run_warn_check "Port 3306 available (Database)" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER '! netstat -tlnp 2>/dev/null | grep -q :3306'"
run_warn_check "Port 6379 available (Redis)" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER '! netstat -tlnp 2>/dev/null | grep -q :6379'"
run_warn_check "Port 7700 available (Meilisearch)" "ssh -o StrictHostKeyChecking=no $SSH_USER@$TARGET_SERVER '! netstat -tlnp 2>/dev/null | grep -q :7700'"

echo ""

# ============================================================================
# LOCAL DOCKER IMAGE BUILD TEST
# ============================================================================
echo "🐳 DOCKER IMAGE BUILD READINESS"
echo "─────────────────────────────────────────"

run_check ".NET SDK dependencies available" "[ -d 'CRM.Backend/src' ]"
run_check "Project files parseable" "ls CRM.Backend/src/CRM.*/CRM.*.csproj >/dev/null 2>&1 | wc -l | grep -q '^[1-9]'"

echo ""

# ============================================================================
# FINAL SUMMARY
# ============================================================================
echo "📊 VALIDATION SUMMARY"
echo "─────────────────────────────────────────"
echo "Total Checks: $check_count"
echo -e "${GREEN}Passed: $pass_count${NC}"
echo -e "${YELLOW}Warnings: $warn_count${NC}"
echo -e "${RED}Failed: $fail_count${NC}"
echo ""

if [[ $fail_count -eq 0 ]]; then
    echo -e "${GREEN}✓ PRE-DEPLOYMENT VALIDATION SUCCESSFUL${NC}"
    echo ""
    echo "You can now proceed with deployment:"
    echo ""
    echo "  Command: ./deploy-to-dev-server.sh"
    echo ""
    echo "Or to deploy to a different server:"
    echo ""
    echo "  Command: TARGET_SERVER=<ip> ./deploy-to-dev-server.sh"
    echo ""
    exit 0
else
    echo -e "${RED}✗ PRE-DEPLOYMENT VALIDATION FAILED${NC}"
    echo ""
    echo "Please resolve the above issues before proceeding."
    echo ""
    
    if [[ $warn_count -gt 0 ]]; then
        echo -e "${YELLOW}Note: $warn_count warnings detected. Review them carefully.${NC}"
        echo ""
    fi
    
    exit 1
fi
