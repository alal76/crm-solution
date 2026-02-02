#!/bin/bash
# ============================================================================
# CRM Frontend Build Validation Script
# ============================================================================
# This script validates the build output to ensure no hardcoded URLs or
# environment-specific values have been baked into the production bundle.
#
# Run this after every production build to catch configuration issues early.
# ============================================================================

# Don't use set -e because ((ERRORS++)) returns non-zero when incrementing from 0

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
FRONTEND_DIR="$PROJECT_DIR/CRM.Frontend"
BUILD_DIR="$FRONTEND_DIR/build"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  CRM Frontend Build Validation${NC}"
echo -e "${BLUE}============================================${NC}"
echo ""

ERRORS=0
WARNINGS=0

# Function to report errors
report_error() {
    echo -e "${RED}✗ ERROR:${NC} $1"
    ((ERRORS++))
}

report_warning() {
    echo -e "${YELLOW}⚠ WARNING:${NC} $1"
    ((WARNINGS++))
}

report_success() {
    echo -e "${GREEN}✓${NC} $1"
}

# Check if build directory exists
if [ ! -d "$BUILD_DIR" ]; then
    report_error "Build directory not found: $BUILD_DIR"
    echo "Run 'npm run build' first."
    exit 1
fi

# Check if JS bundles exist
JS_FILES=$(find "$BUILD_DIR/static/js" -name "main.*.js" 2>/dev/null | head -1)
if [ -z "$JS_FILES" ]; then
    report_error "No main.*.js bundle found in build output"
    exit 1
fi

echo -e "${BLUE}Checking JavaScript bundles for hardcoded values...${NC}"
echo ""

# ============================================
# Check 1: No hardcoded localhost URLs
# ============================================
echo "1. Checking for hardcoded localhost URLs..."
LOCALHOST_COUNT=$(grep -rE 'localhost:[0-9]{4}' "$BUILD_DIR/static/js/"*.js 2>/dev/null | grep -v '//.*localhost' | wc -l | tr -d ' ')
if [ "$LOCALHOST_COUNT" -gt 0 ]; then
    report_error "Found hardcoded localhost URLs in bundle ($LOCALHOST_COUNT occurrences)"
    grep -rE 'localhost:[0-9]{4}' "$BUILD_DIR/static/js/"*.js 2>/dev/null | head -3
else
    report_success "No hardcoded localhost URLs found"
fi

# ============================================
# Check 2: No private network IPs (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
# ============================================
echo ""
echo "2. Checking for hardcoded private network IPs as API URLs..."
# Look for IPs with ports that indicate API endpoints (not placeholder text)
# Exclude common placeholder patterns like "192.168.1.100" or "192.168.1.1" which are examples
PRIVATE_IP_COUNT=$(grep -rE 'http://(192\.168\.[0-9]+\.[0-9]+|10\.[0-9]+\.[0-9]+\.[0-9]+):[0-9]+/api' "$BUILD_DIR/static/js/"*.js 2>/dev/null | wc -l | tr -d ' ')
if [ "$PRIVATE_IP_COUNT" -gt 0 ]; then
    report_error "Found hardcoded private network API URLs in bundle ($PRIVATE_IP_COUNT occurrences)"
    grep -rE 'http://(192\.168|10\.)[0-9.]+:[0-9]+/api' "$BUILD_DIR/static/js/"*.js 2>/dev/null | head -3
else
    report_success "No hardcoded private network API URLs found"
fi

# Additional check: Look for specific patterns that are known safe (placeholders)
PLACEHOLDER_COUNT=$(grep -rE 'placeholder.*192\.168\.[0-9]+\.[0-9]+' "$BUILD_DIR/static/js/"*.js 2>/dev/null | wc -l | tr -d ' ')
if [ "$PLACEHOLDER_COUNT" -gt 0 ]; then
    echo -e "  ${BLUE}ℹ${NC} Note: $PLACEHOLDER_COUNT occurrences are UI placeholder text (safe)"
fi

# ============================================
# Check 3: REACT_APP_API_URL should be empty in production bundle
# ============================================
echo ""
echo "3. Checking REACT_APP_API_URL configuration..."
# Look for the pattern REACT_APP_API_URL:"something" where something is NOT empty
API_URL_VALUE=$(grep -oE 'REACT_APP_API_URL:"[^"]*"' "$BUILD_DIR/static/js/"*.js 2>/dev/null | head -1)
if [ -n "$API_URL_VALUE" ]; then
    if echo "$API_URL_VALUE" | grep -q 'REACT_APP_API_URL:""'; then
        report_success "REACT_APP_API_URL is correctly empty in bundle"
    else
        report_error "REACT_APP_API_URL has a hardcoded value: $API_URL_VALUE"
        echo "  Fix: Ensure .env.production has REACT_APP_API_URL= (empty)"
    fi
else
    report_warning "Could not detect REACT_APP_API_URL pattern in bundle"
fi

# ============================================
# Check 4: Verify .env.production is correct
# ============================================
echo ""
echo "4. Validating .env.production file..."
ENV_PROD="$FRONTEND_DIR/.env.production"
if [ -f "$ENV_PROD" ]; then
    # Check if REACT_APP_API_URL is set to empty
    API_LINE=$(grep -E "^REACT_APP_API_URL=" "$ENV_PROD" 2>/dev/null || echo "")
    if [ -z "$API_LINE" ]; then
        report_warning "REACT_APP_API_URL not found in .env.production"
    elif [ "$API_LINE" = "REACT_APP_API_URL=" ]; then
        report_success ".env.production has REACT_APP_API_URL correctly set to empty"
    else
        report_error ".env.production has hardcoded REACT_APP_API_URL: $API_LINE"
        echo "  Fix: Set REACT_APP_API_URL= (empty) in .env.production"
    fi
else
    report_error ".env.production file not found"
fi

# ============================================
# Check 5: No API keys or secrets in bundle
# ============================================
echo ""
echo "5. Checking for potential secrets in bundle..."
# Look for hardcoded secrets, but exclude common library patterns (SignalR Bearer header construction, etc.)
SECRET_PATTERNS="(api[_-]?key|secret[_-]?key)['\"]?\s*[:=]\s*['\"][a-zA-Z0-9_-]{20,}"
SECRET_COUNT=$(grep -riE "$SECRET_PATTERNS" "$BUILD_DIR/static/js/"*.js 2>/dev/null | grep -v 'accessTokenFactory' | grep -v 'Authorization.*Bearer' | wc -l | tr -d ' ')
if [ "$SECRET_COUNT" -gt 0 ]; then
    report_warning "Found potential secrets/keys in bundle ($SECRET_COUNT matches)"
    echo "  Review these carefully - may be false positives"
else
    report_success "No obvious secrets/keys found in bundle"
fi

# ============================================
# Check 6: Verify nginx config exists for Docker builds
# ============================================
echo ""
echo "6. Checking nginx configuration..."
NGINX_CONF="$PROJECT_DIR/docker/nginx-frontend.conf"
if [ -f "$NGINX_CONF" ]; then
    if grep -q 'location.*\/api\/' "$NGINX_CONF"; then
        report_success "nginx config has /api/ proxy route"
    else
        report_warning "nginx config may be missing /api/ proxy route"
    fi
else
    report_error "nginx-frontend.conf not found"
fi

# ============================================
# Summary
# ============================================
echo ""
echo -e "${BLUE}============================================${NC}"
echo -e "${BLUE}  Validation Summary${NC}"
echo -e "${BLUE}============================================${NC}"

if [ $ERRORS -gt 0 ]; then
    echo -e "${RED}✗ $ERRORS error(s) found${NC}"
    echo ""
    echo "The production build contains hardcoded values that will cause"
    echo "deployment issues. Please fix these before deploying."
    echo ""
    echo "Common fixes:"
    echo "  1. Ensure .env.production has REACT_APP_API_URL= (empty)"
    echo "  2. Remove any hardcoded URLs from source files"
    echo "  3. Use the centralized config in src/config/ports.ts"
    echo ""
    exit 1
elif [ $WARNINGS -gt 0 ]; then
    echo -e "${YELLOW}⚠ $WARNINGS warning(s) found${NC}"
    echo "Review warnings above, but build may still work."
    exit 0
else
    echo -e "${GREEN}✓ All checks passed!${NC}"
    echo "Build is ready for production deployment."
    exit 0
fi
