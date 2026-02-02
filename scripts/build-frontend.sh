#!/bin/bash
# Frontend Build Script with Change Detection and Caching
# Only rebuilds when source files have actually changed

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
FRONTEND_DIR="$PROJECT_DIR/CRM.Frontend"
CACHE_DIR="$PROJECT_DIR/.build-cache"
HASH_FILE="$CACHE_DIR/frontend-source.hash"
LOG_FILE="/tmp/frontend-build.log"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== CRM Frontend Build (Optimized) ===${NC}"
echo "Project: $FRONTEND_DIR"
echo ""

# Parse arguments
FORCE_BUILD=false
DOCKER_BUILD=false
VERSION_TAG=""

while [[ $# -gt 0 ]]; do
    case $1 in
        --force|-f)
            FORCE_BUILD=true
            shift
            ;;
        --docker|-d)
            DOCKER_BUILD=true
            shift
            ;;
        --tag|-t)
            VERSION_TAG="$2"
            shift 2
            ;;
        *)
            shift
            ;;
    esac
done

# Create cache directory
mkdir -p "$CACHE_DIR"

# Calculate source hash (only src, public, package.json, tsconfig.json)
calculate_source_hash() {
    cd "$FRONTEND_DIR"
    find src public -type f \( -name "*.ts" -o -name "*.tsx" -o -name "*.js" -o -name "*.jsx" -o -name "*.css" -o -name "*.scss" -o -name "*.json" -o -name "*.html" -o -name "*.svg" -o -name "*.png" \) -exec md5 -q {} \; 2>/dev/null | sort | md5
    # Also include package.json and tsconfig.json
    cat package.json tsconfig.json 2>/dev/null | md5
}

# Get previous hash
get_previous_hash() {
    if [ -f "$HASH_FILE" ]; then
        cat "$HASH_FILE"
    else
        echo "none"
    fi
}

# Check if rebuild is needed
CURRENT_HASH=$(calculate_source_hash)
PREVIOUS_HASH=$(get_previous_hash)

echo -e "Current hash:  ${BLUE}${CURRENT_HASH:0:16}...${NC}"
echo -e "Previous hash: ${BLUE}${PREVIOUS_HASH:0:16}...${NC}"
echo ""

if [ "$FORCE_BUILD" = false ] && [ "$CURRENT_HASH" = "$PREVIOUS_HASH" ] && [ -d "$FRONTEND_DIR/build" ]; then
    echo -e "${GREEN}✓ No source changes detected. Skipping build.${NC}"
    echo "  Use --force to rebuild anyway."
    
    if [ "$DOCKER_BUILD" = true ]; then
        echo ""
        echo -e "${YELLOW}Docker build requested - will use cached build output.${NC}"
    fi
    
    # Show existing build info
    if [ -f "$FRONTEND_DIR/build/asset-manifest.json" ]; then
        BUILD_TIME=$(stat -f "%Sm" -t "%Y-%m-%d %H:%M" "$FRONTEND_DIR/build/asset-manifest.json" 2>/dev/null || echo "unknown")
        echo "  Last build: $BUILD_TIME"
    fi
    
    # Skip to Docker build if requested
    if [ "$DOCKER_BUILD" = false ]; then
        exit 0
    fi
else
    if [ "$FORCE_BUILD" = true ]; then
        echo -e "${YELLOW}Force rebuild requested.${NC}"
    else
        echo -e "${YELLOW}Source changes detected. Rebuilding...${NC}"
    fi
    echo ""

    # Check available memory
    if command -v vm_stat &> /dev/null; then
        FREE_PAGES=$(vm_stat | grep "Pages free" | awk '{print $3}' | tr -d '.')
        PAGE_SIZE=16384
        FREE_MB=$((FREE_PAGES * PAGE_SIZE / 1024 / 1024))
        echo "Available memory: ~${FREE_MB}MB"
        
        if [ "$FREE_MB" -lt 500 ]; then
            echo -e "${YELLOW}⚠️  Low memory. Build may be slow.${NC}"
        fi
    fi

    cd "$FRONTEND_DIR"

    # Kill any existing node processes from previous failed builds
    pkill -f "react-scripts build" 2>/dev/null || true
    sleep 1

    # Optimize Node.js memory usage
    export NODE_OPTIONS="--max_old_space_size=4096"
    export GENERATE_SOURCEMAP=false
    export CI=true

    # Pre-build validation: Check .env.production
    echo -e "${YELLOW}Pre-build validation...${NC}"
    ENV_PROD_FILE="$FRONTEND_DIR/.env.production"
    if [ -f "$ENV_PROD_FILE" ]; then
        API_URL_LINE=$(grep -E "^REACT_APP_API_URL=" "$ENV_PROD_FILE" 2>/dev/null || echo "")
        if [ -n "$API_URL_LINE" ] && [ "$API_URL_LINE" != "REACT_APP_API_URL=" ]; then
            echo -e "${RED}ERROR: .env.production has hardcoded REACT_APP_API_URL${NC}"
            echo "Current value: $API_URL_LINE"
            echo "Fix: Set REACT_APP_API_URL= (empty) in .env.production"
            exit 1
        fi
        echo -e "${GREEN}✓${NC} .env.production is correctly configured"
    else
        echo -e "${YELLOW}⚠${NC} .env.production not found - using defaults"
    fi

    echo ""
    echo "Starting build..."
    echo "Build started at $(date)"
    echo ""

    # Run the build
    if npm run build 2>&1 | tee "$LOG_FILE"; then
        echo ""
        echo -e "${GREEN}✅ Build completed successfully!${NC}"
        
        # Save new hash
        echo "$CURRENT_HASH" > "$HASH_FILE"
        
        # Show bundle sizes
        if [ -d "$FRONTEND_DIR/build/static/js" ]; then
            echo ""
            echo "Bundle sizes:"
            ls -lh "$FRONTEND_DIR/build/static/js/"*.js 2>/dev/null | awk '{print "   " $9 ": " $5}'
        fi

        # Post-build validation
        echo ""
        echo -e "${YELLOW}Post-build validation...${NC}"
        if [ -x "$SCRIPT_DIR/validate-build.sh" ]; then
            if "$SCRIPT_DIR/validate-build.sh"; then
                echo -e "${GREEN}✓${NC} Build validation passed"
            else
                echo -e "${RED}✗${NC} Build validation failed - review errors above"
                exit 1
            fi
        else
            # Quick inline check if script doesn't exist
            HARDCODED_URLS=$(grep -rE 'localhost:[0-9]{4}|192\.168\.[0-9]+\.[0-9]+:[0-9]+' "$FRONTEND_DIR/build/static/js/"*.js 2>/dev/null | wc -l | tr -d ' ')
            if [ "$HARDCODED_URLS" -gt 0 ]; then
                echo -e "${RED}✗${NC} WARNING: Found $HARDCODED_URLS hardcoded URLs in bundle"
            else
                echo -e "${GREEN}✓${NC} No hardcoded URLs found in bundle"
            fi
        fi
    else
        echo ""
        echo -e "${RED}❌ Build failed!${NC}"
        echo "   Check log: $LOG_FILE"
        exit 1
    fi
fi

# Docker build if requested
if [ "$DOCKER_BUILD" = true ]; then
    echo ""
    echo -e "${BLUE}=== Building Docker Image ===${NC}"
    
    # Determine version tag
    if [ -z "$VERSION_TAG" ]; then
        # Auto-increment from existing images
        LATEST=$(docker images crm-frontend --format "{{.Tag}}" 2>/dev/null | grep -E "^v[0-9]+$" | sort -V | tail -1 || echo "v0")
        LATEST_NUM=${LATEST#v}
        VERSION_TAG="v$((LATEST_NUM + 1))"
    fi
    
    echo "Building crm-frontend:$VERSION_TAG"
    
    cd "$PROJECT_DIR"
    
    # Use BuildKit for better caching
    export DOCKER_BUILDKIT=1
    
    if docker build \
        --build-arg SOURCE_HASH="$CURRENT_HASH" \
        -f docker/Dockerfile.frontend \
        -t "crm-frontend:$VERSION_TAG" \
        . 2>&1 | tee -a "$LOG_FILE"; then
        
        echo ""
        echo -e "${GREEN}✅ Docker image built: crm-frontend:$VERSION_TAG${NC}"
        
        # Optionally load to minikube
        if command -v minikube &> /dev/null && minikube status &> /dev/null; then
            echo "Loading image to minikube..."
            minikube image load "crm-frontend:$VERSION_TAG"
            echo -e "${GREEN}✓ Image loaded to minikube${NC}"
        fi
    else
        echo -e "${RED}❌ Docker build failed!${NC}"
        exit 1
    fi
fi

echo ""
echo "Done!"
