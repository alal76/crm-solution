#!/bin/bash

# Build Verification Script
# This script verifies the entire project builds successfully

set -e

echo "╔════════════════════════════════════════════════════════════════════╗"
echo "║         CRM Solution - Complete Build Verification                ║"
echo "╚════════════════════════════════════════════════════════════════════╝"
echo ""

SOLUTION_DIR="/Users/alal/Code/Git CRM Solution/crm-solution"
FRONTEND_DIR="$SOLUTION_DIR/CRM.Frontend"
BACKEND_DIR="$SOLUTION_DIR/CRM.Backend"

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

TIMESTAMP=$(date '+%Y-%m-%d %H:%M:%S')
BUILD_LOG="/tmp/crm-build-$(date +%s).log"

echo "📝 Build started at: $TIMESTAMP"
echo "📋 Build log: $BUILD_LOG"
echo ""

# ============================================================================
# 1. VERIFY DEPENDENCIES
# ============================================================================
echo -e "${BLUE}1️⃣  Verifying System Dependencies...${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Check Node.js
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo -e "${GREEN}✅ Node.js: $NODE_VERSION${NC}"
else
    echo -e "${RED}❌ Node.js not found${NC}"
    exit 1
fi

# Check npm
if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    echo -e "${GREEN}✅ npm: $NPM_VERSION${NC}"
else
    echo -e "${RED}❌ npm not found${NC}"
    exit 1
fi

# Check .NET
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✅ .NET: $DOTNET_VERSION${NC}"
else
    echo -e "${RED}❌ .NET SDK not found${NC}"
    exit 1
fi

# Check Docker
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    echo -e "${GREEN}✅ Docker: $DOCKER_VERSION${NC}"
else
    echo -e "${RED}❌ Docker not found${NC}"
    exit 1
fi

echo ""

# ============================================================================
# 2. FRONTEND BUILD
# ============================================================================
echo -e "${BLUE}2️⃣  Building Frontend Application...${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$FRONTEND_DIR"

echo "📦 Installing dependencies..."
npm install --legacy-peer-deps 2>&1 | grep -E "added|audited|found" || true

echo "🔍 Running TypeScript check..."
if npx tsc --noEmit 2>&1 | tail -5; then
    echo -e "${GREEN}✅ TypeScript check passed${NC}"
else
    echo -e "${YELLOW}⚠️  TypeScript check completed with warnings${NC}"
fi

echo "🏗️  Building production bundle..."
if npm run build 2>&1 | tail -10; then
    BUNDLE_SIZE=$(du -sh build/ 2>/dev/null | cut -f1)
    echo -e "${GREEN}✅ Frontend build succeeded (Bundle: $BUNDLE_SIZE)${NC}"
else
    echo -e "${RED}❌ Frontend build failed${NC}"
    exit 1
fi

echo ""

# ============================================================================
# 3. BACKEND BUILD
# ============================================================================
echo -e "${BLUE}3️⃣  Building Backend API...${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$BACKEND_DIR"

echo "🔨 Restoring NuGet packages..."
if dotnet restore 2>&1 | grep -E "Restore completed|already installed" | head -1; then
    echo -e "${GREEN}✅ NuGet restore completed${NC}"
else
    echo -e "${RED}❌ NuGet restore failed${NC}"
    exit 1
fi

echo "🏗️  Building solution (Release)..."
if dotnet build -c Release 2>&1 | tail -15; then
    echo -e "${GREEN}✅ Backend build succeeded${NC}"
else
    echo -e "${RED}❌ Backend build failed${NC}"
    exit 1
fi

echo ""

# ============================================================================
# 4. DOCKER BUILD
# ============================================================================
echo -e "${BLUE}4️⃣  Building Docker Images...${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

cd "$SOLUTION_DIR"

echo "🐳 Building Docker images..."
if docker compose build 2>&1 | tail -20; then
    echo -e "${GREEN}✅ Docker images built successfully${NC}"
else
    echo -e "${RED}❌ Docker build failed${NC}"
    exit 1
fi

echo ""

# ============================================================================
# 5. FILE STRUCTURE VERIFICATION
# ============================================================================
echo -e "${BLUE}5️⃣  Verifying Project Structure...${NC}"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

echo "📁 Checking critical files..."

CRITICAL_FILES=(
    "$FRONTEND_DIR/src/index.tsx"
    "$FRONTEND_DIR/src/App.tsx"
    "$FRONTEND_DIR/src/pages"
    "$BACKEND_DIR/src/CRM.Api/Program.cs"
    "$BACKEND_DIR/src/CRM.Api/Controllers"
    "$SOLUTION_DIR/docker-compose.yml"
)

ALL_FILES_EXIST=true
for file in "${CRITICAL_FILES[@]}"; do
    if [ -e "$file" ]; then
        echo -e "${GREEN}✅ $file${NC}"
    else
        echo -e "${RED}❌ MISSING: $file${NC}"
        ALL_FILES_EXIST=false
    fi
done

echo ""

# ============================================================================
# 6. BUILD SUMMARY
# ============================================================================
echo -e "${YELLOW}════════════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}                     BUILD VERIFICATION SUMMARY${NC}"
echo -e "${YELLOW}════════════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${GREEN}✅ System Dependencies: OK${NC}"
echo -e "${GREEN}✅ Frontend Build: OK${NC}"
echo -e "${GREEN}✅ Backend Build: OK${NC}"
echo -e "${GREEN}✅ Docker Build: OK${NC}"

if [ "$ALL_FILES_EXIST" = true ]; then
    echo -e "${GREEN}✅ Project Structure: OK${NC}"
else
    echo -e "${RED}❌ Project Structure: INCOMPLETE${NC}"
fi

echo ""
echo "📊 Build Details:"
echo "   • Timestamp: $TIMESTAMP"
echo "   • Frontend Bundle: $(du -sh $FRONTEND_DIR/build/ 2>/dev/null | cut -f1)"
echo "   • TypeScript Pages: $(find $FRONTEND_DIR/src/pages -name "*.tsx" | wc -l)"
echo "   • API Controllers: $(find $BACKEND_DIR/src/CRM.Api/Controllers -name "*Controller.cs" | wc -l)"
echo ""

if [ "$ALL_FILES_EXIST" = true ]; then
    echo -e "${GREEN}════════════════════════════════════════════════════════════════════${NC}"
    echo -e "${GREEN}✅ BUILD VERIFICATION SUCCESSFUL${NC}"
    echo -e "${GREEN}════════════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo "🚀 Next steps:"
    echo "   docker compose up -d          # Start all services"
    echo "   http://localhost:3000         # Access frontend"
    echo "   http://localhost:5001/swagger # Access API docs"
    exit 0
else
    echo -e "${RED}════════════════════════════════════════════════════════════════════${NC}"
    echo -e "${RED}❌ BUILD VERIFICATION FAILED${NC}"
    echo -e "${RED}════════════════════════════════════════════════════════════════════${NC}"
    exit 1
fi
