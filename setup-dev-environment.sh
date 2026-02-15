#!/bin/bash

# ============================================================================
# CRM Solution Development Environment Setup Script
# ============================================================================
# This script fixes PATH issues and ensures all development tools are
# properly configured and accessible.
# ============================================================================

set -e  # Exit on error

echo "🔧 CRM Development Environment Setup"
echo "===================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# ============================================================================
# 1. Check and Fix PATH
# ============================================================================
echo -e "${BLUE}Step 1: Verifying and fixing PATH configuration...${NC}"

# Ensure /opt/homebrew/bin is in PATH
if [[ ":$PATH:" == *":/opt/homebrew/bin:"* ]]; then
    echo -e "${GREEN}✓ /opt/homebrew/bin is in PATH${NC}"
else
    echo -e "${YELLOW}⚠ Adding /opt/homebrew/bin to PATH${NC}"
    export PATH="/opt/homebrew/bin:$PATH"
fi

# ============================================================================
# 2. Verify Node.js Installation
# ============================================================================
echo -e "${BLUE}Step 2: Checking Node.js installation...${NC}"

if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo -e "${GREEN}✓ Node.js is installed: $NODE_VERSION${NC}"
else
    echo -e "${RED}✗ Node.js not found. Installing via Homebrew...${NC}"
    brew install node
    NODE_VERSION=$(node --version)
    echo -e "${GREEN}✓ Node.js installed: $NODE_VERSION${NC}"
fi

if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    echo -e "${GREEN}✓ npm is installed: v$NPM_VERSION${NC}"
else
    echo -e "${RED}✗ npm not found${NC}"
fi

if command -v npx &> /dev/null; then
    echo -e "${GREEN}✓ npx is available${NC}"
else
    echo -e "${RED}✗ npx not found${NC}"
fi

# ============================================================================
# 3. Install TypeScript and tsx globally
# ============================================================================
echo -e "${BLUE}Step 3: Installing TypeScript and tsx...${NC}"

if npm list -g typescript &> /dev/null; then
    TS_VERSION=$(npx typescript --version)
    echo -e "${GREEN}✓ TypeScript is installed: $TS_VERSION${NC}"
else
    echo -e "${YELLOW}Installing TypeScript globally...${NC}"
    npm install -g typescript
    TS_VERSION=$(npx typescript --version)
    echo -e "${GREEN}✓ TypeScript installed: $TS_VERSION${NC}"
fi

if npm list -g tsx &> /dev/null; then
    echo -e "${GREEN}✓ tsx is installed${NC}"
else
    echo -e "${YELLOW}Installing tsx globally...${NC}"
    npm install -g tsx
    echo -e "${GREEN}✓ tsx installed${NC}"
fi

# ============================================================================
# 4. Verify .NET Installation
# ============================================================================
echo -e "${BLUE}Step 4: Checking .NET installation...${NC}"

if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo -e "${GREEN}✓ .NET is installed: $DOTNET_VERSION${NC}"
else
    echo -e "${RED}✗ .NET not found${NC}"
    echo "Install .NET from: https://dotnet.microsoft.com/download"
fi

# ============================================================================
# 5. Verify Git
# ============================================================================
echo -e "${BLUE}Step 5: Checking Git...${NC}"

if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version)
    echo -e "${GREEN}✓ Git is installed: $GIT_VERSION${NC}"
else
    echo -e "${RED}✗ Git not found${NC}"
    echo "Install Git from: https://git-scm.com/download/mac"
fi

# ============================================================================
# 6. Verify npm packages for CRM.Frontend
# ============================================================================
echo -e "${BLUE}Step 6: Checking CRM Frontend dependencies...${NC}"

if [[ -f "CRM.Frontend/package.json" ]]; then
    echo -e "${YELLOW}Installing CRM Frontend dependencies...${NC}"
    cd CRM.Frontend
    npm install 2>&1 | tail -5
    cd ..
    echo -e "${GREEN}✓ CRM Frontend dependencies installed${NC}"
else
    echo -e "${YELLOW}⚠ CRM.Frontend/package.json not found${NC}"
fi

# ============================================================================
# 7. Test Development Tools
# ============================================================================
echo -e "${BLUE}Step 7: Running development tools verification...${NC}"

echo ""
echo "Tool Verification Results:"
echo "========================="

# Test Node.js
echo -n "Node.js: "
node --version && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test npm
echo -n "npm: "
npm --version && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test npx
echo -n "npx: "
npx --version && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test TypeScript
echo -n "TypeScript: "
npx typescript --version 2>/dev/null && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test tsx
echo -n "tsx: "
npx tsx --version 2>/dev/null && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test .NET
echo -n ".NET: "
dotnet --version 2>/dev/null && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# Test Git
echo -n "Git: "
git --version 2>/dev/null && echo -e "${GREEN}✓${NC}" || echo -e "${RED}✗${NC}"

# ============================================================================
# 8. Display Environment Summary
# ============================================================================
echo ""
echo -e "${BLUE}Environment Summary:${NC}"
echo "===================="
echo "Shell: $SHELL"
echo "HOME: $HOME"
echo "PATH: $PATH"
echo ""
echo "Homebrew Installations:"
brew list --formula | grep -E 'node|npm|typescript|dotnet' || echo "No matching packages"

# ============================================================================
# Summary
# ============================================================================
echo ""
echo -e "${GREEN}✅ Development environment setup complete!${NC}"
echo ""
echo "Next steps:"
echo "1. Add the following to your shell profile (~/.zshrc or ~/.bash_profile):"
echo "   export PATH=\"/opt/homebrew/bin:\$PATH\""
echo ""
echo "2. Ensure \$PATH includes:"
echo "   - /opt/homebrew/bin (Node.js, npm, etc.)"
echo "   - /opt/homebrew/sbin"
echo "   - /usr/local/bin"
echo "   - ~/.dotnet"
echo ""
echo "3. To use these tools in terminal:"
echo "   Run: source ~/.zshrc"
echo ""
echo "For the CRM projects:"
echo "   Backend: dotnet build CRM.Backend/CRM.sln"
echo "   Frontend: cd CRM.Frontend && npm install && npm start"
echo ""
