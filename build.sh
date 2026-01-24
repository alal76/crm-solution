#!/bin/bash
# CRM Solution Build Script
# Copyright (C) 2024-2026 Abhishek Lal - GNU AGPL v3

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$SCRIPT_DIR/CRM.Backend"
FRONTEND_DIR="$SCRIPT_DIR/CRM.Frontend"
BUILD_DIR="$SCRIPT_DIR/build-output"

# Default configuration
BUILD_ENV="${1:-Development}"
BUILD_CONFIG="${2:-Debug}"
RUN_TESTS="${3:-true}"

print_header() {
    echo -e "${PURPLE}"
    echo "╔══════════════════════════════════════════════════════════════╗"
    echo "║           CRM Solution - Build System v1.3.1                 ║"
    echo "║           Environment: $BUILD_ENV                            "
    echo "║           Configuration: $BUILD_CONFIG                       "
    echo "╚══════════════════════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_step() {
    echo -e "\n${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${BLUE}▶ $1${NC}"
    echo -e "${CYAN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
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

# Usage information
show_usage() {
    echo -e "${YELLOW}Usage: $0 [environment] [configuration] [run_tests]${NC}"
    echo ""
    echo "Environments:"
    echo "  Development  - Dev mode with verbose logging (default)"
    echo "  Testing      - Functional testing with extensive debug info"
    echo "  Performance  - Performance testing with metrics collection"
    echo "  Production   - Clean, optimized production build"
    echo ""
    echo "Configurations:"
    echo "  Debug        - Debug symbols, no optimization (default for Dev/Test)"
    echo "  Release      - Optimized build (default for Perf/Prod)"
    echo ""
    echo "Examples:"
    echo "  $0                          # Dev + Debug + Tests"
    echo "  $0 Development Debug true   # Dev + Debug + Tests"
    echo "  $0 Testing Debug true       # Test deployment"
    echo "  $0 Performance Release true # Performance build"
    echo "  $0 Production Release false # Production build (no tests)"
}

# Check prerequisites
check_prerequisites() {
    print_step "Checking Prerequisites"
    
    local missing=0
    
    # Check .NET SDK
    if command -v dotnet &> /dev/null; then
        DOTNET_VERSION=$(dotnet --version)
        print_success "dotnet SDK: $DOTNET_VERSION"
    else
        print_error "dotnet SDK not found"
        missing=1
    fi
    
    # Check Node.js
    if command -v node &> /dev/null; then
        NODE_VERSION=$(node --version)
        print_success "Node.js: $NODE_VERSION"
    else
        print_error "Node.js not found"
        missing=1
    fi
    
    # Check npm
    if command -v npm &> /dev/null; then
        NPM_VERSION=$(npm --version)
        print_success "npm: $NPM_VERSION"
    else
        print_error "npm not found"
        missing=1
    fi
    
    if [ $missing -eq 1 ]; then
        print_error "Missing prerequisites. Please install them and try again."
        exit 1
    fi
}

# Clean previous builds
clean_build() {
    print_step "Cleaning Previous Build Artifacts"
    
    # Clean .NET
    if [ -d "$BACKEND_DIR" ]; then
        dotnet clean "$BACKEND_DIR/CRM.sln" -v q 2>/dev/null || true
        find "$BACKEND_DIR" -type d -name "bin" -exec rm -rf {} + 2>/dev/null || true
        find "$BACKEND_DIR" -type d -name "obj" -exec rm -rf {} + 2>/dev/null || true
        print_success "Backend cleaned"
    fi
    
    # Clean Frontend
    if [ -d "$FRONTEND_DIR/build" ]; then
        rm -rf "$FRONTEND_DIR/build"
        print_success "Frontend build cleaned"
    fi
    
    # Clean output directory
    if [ -d "$BUILD_DIR" ]; then
        rm -rf "$BUILD_DIR"
    fi
    mkdir -p "$BUILD_DIR"
    print_success "Build output directory prepared"
}

# Restore dependencies
restore_dependencies() {
    print_step "Restoring Dependencies"
    
    # .NET restore
    echo -e "${CYAN}Restoring .NET packages...${NC}"
    dotnet restore "$BACKEND_DIR/CRM.sln" --verbosity minimal
    print_success ".NET packages restored"
    
    # npm install
    echo -e "${CYAN}Installing npm packages...${NC}"
    cd "$FRONTEND_DIR"
    npm ci --silent 2>/dev/null || npm install --silent
    cd "$SCRIPT_DIR"
    print_success "npm packages installed"
}

# Build backend
build_backend() {
    print_step "Building Backend ($BUILD_CONFIG)"
    
    local build_args="-c $BUILD_CONFIG --no-restore"
    
    # Add environment-specific flags
    case $BUILD_ENV in
        Development)
            build_args="$build_args -p:DebugType=full -p:DebugSymbols=true"
            ;;
        Testing)
            build_args="$build_args -p:DebugType=portable"
            ;;
        Performance)
            build_args="$build_args -p:Optimize=true"
            ;;
        Production)
            build_args="$build_args -p:Optimize=true -p:DebugType=none -p:DebugSymbols=false"
            ;;
    esac
    
    echo -e "${CYAN}Build arguments: $build_args${NC}"
    
    dotnet build "$BACKEND_DIR/CRM.sln" $build_args
    print_success "Backend build completed"
}

# Build frontend
build_frontend() {
    print_step "Building Frontend ($BUILD_ENV)"
    
    cd "$FRONTEND_DIR"
    
    # Set environment variables
    export REACT_APP_ENV=$BUILD_ENV
    export GENERATE_SOURCEMAP=$([ "$BUILD_ENV" == "Production" ] && echo "false" || echo "true")
    
    if [ "$BUILD_ENV" == "Production" ]; then
        echo -e "${CYAN}Building optimized production bundle...${NC}"
        npm run build 2>&1 | tail -20
    else
        echo -e "${CYAN}Building with source maps...${NC}"
        npm run build 2>&1 | tail -20
    fi
    
    cd "$SCRIPT_DIR"
    print_success "Frontend build completed"
}

# Run unit tests
run_unit_tests() {
    if [ "$RUN_TESTS" != "true" ]; then
        print_warning "Skipping tests (RUN_TESTS=$RUN_TESTS)"
        return 0
    fi
    
    print_step "Running Unit Tests"
    
    local test_args="--no-build -c $BUILD_CONFIG"
    
    # Add verbosity based on environment
    case $BUILD_ENV in
        Development|Testing)
            test_args="$test_args -v normal --logger 'console;verbosity=detailed'"
            ;;
        *)
            test_args="$test_args -v minimal --logger 'console;verbosity=minimal'"
            ;;
    esac
    
    echo -e "${CYAN}Running tests with: dotnet test $test_args${NC}"
    
    dotnet test "$BACKEND_DIR/tests/CRM.Tests.csproj" $test_args \
        --results-directory "$BUILD_DIR/test-results" \
        --collect:"XPlat Code Coverage" || {
        print_error "Unit tests failed"
        return 1
    }
    
    print_success "Unit tests passed"
}

# Run BVT tests
run_bvt_tests() {
    if [ "$RUN_TESTS" != "true" ]; then
        return 0
    fi
    
    print_step "Running Build Verification Tests (BVT)"
    
    dotnet test "$BACKEND_DIR/tests/CRM.Tests.csproj" \
        --no-build \
        -c $BUILD_CONFIG \
        --filter "FullyQualifiedName~BVT" \
        -v normal \
        --results-directory "$BUILD_DIR/bvt-results" || {
        print_error "BVT tests failed"
        return 1
    }
    
    print_success "BVT tests passed"
}

# Publish artifacts
publish_artifacts() {
    print_step "Publishing Build Artifacts"
    
    local publish_args="-c $BUILD_CONFIG --no-build -o $BUILD_DIR/backend"
    
    case $BUILD_ENV in
        Production)
            publish_args="$publish_args --self-contained false -p:PublishSingleFile=false"
            ;;
    esac
    
    dotnet publish "$BACKEND_DIR/src/CRM.Api/CRM.Api.csproj" $publish_args
    
    # Copy frontend build
    if [ -d "$FRONTEND_DIR/build" ]; then
        cp -r "$FRONTEND_DIR/build" "$BUILD_DIR/frontend"
        print_success "Frontend artifacts copied"
    fi
    
    # Copy configuration for environment
    cp "$BACKEND_DIR/src/CRM.Api/appsettings.$BUILD_ENV.json" "$BUILD_DIR/backend/" 2>/dev/null || true
    
    print_success "Artifacts published to: $BUILD_DIR"
}

# Generate build report
generate_report() {
    print_step "Generating Build Report"
    
    local report_file="$BUILD_DIR/build-report.txt"
    
    {
        echo "═══════════════════════════════════════════════════════════════"
        echo "              CRM Solution Build Report"
        echo "═══════════════════════════════════════════════════════════════"
        echo ""
        echo "Build Date:      $(date '+%Y-%m-%d %H:%M:%S %Z')"
        echo "Environment:     $BUILD_ENV"
        echo "Configuration:   $BUILD_CONFIG"
        echo "Tests Run:       $RUN_TESTS"
        echo ""
        echo "───────────────────────────────────────────────────────────────"
        echo "                        Versions"
        echo "───────────────────────────────────────────────────────────────"
        echo ".NET SDK:        $(dotnet --version)"
        echo "Node.js:         $(node --version)"
        echo "npm:             $(npm --version)"
        echo ""
        echo "───────────────────────────────────────────────────────────────"
        echo "                      Artifact Sizes"
        echo "───────────────────────────────────────────────────────────────"
        if [ -d "$BUILD_DIR/backend" ]; then
            echo "Backend:         $(du -sh "$BUILD_DIR/backend" | cut -f1)"
        fi
        if [ -d "$BUILD_DIR/frontend" ]; then
            echo "Frontend:        $(du -sh "$BUILD_DIR/frontend" | cut -f1)"
        fi
        echo ""
        echo "═══════════════════════════════════════════════════════════════"
    } > "$report_file"
    
    cat "$report_file"
    print_success "Build report saved to: $report_file"
}

# Rebuild module field configurations
rebuild_module_configs() {
    print_step "Rebuilding Module Field Configurations"
    
    # Check if running in Kubernetes environment
    if command -v kubectl &> /dev/null; then
        local api_pod=$(kubectl get pods -l app=crm-api -o jsonpath='{.items[0].metadata.name}' 2>/dev/null)
        
        if [ -n "$api_pod" ] && [ "$api_pod" != "" ]; then
            echo -e "${CYAN}Calling reseed API via port-forward...${NC}"
            
            # Start port-forward in background
            kubectl port-forward "$api_pod" 5001:5000 &>/dev/null &
            local pf_pid=$!
            sleep 2
            
            # Call the reseed endpoint
            local response=$(curl -s -X POST "http://localhost:5001/api/database/reseed" \
                -H "Content-Type: application/json" \
                -H "Authorization: Bearer admin-build-token" \
                --max-time 30 2>/dev/null)
            
            # Kill port-forward
            kill $pf_pid 2>/dev/null
            
            if echo "$response" | grep -q "reseeded successfully"; then
                print_success "Module field configurations rebuilt successfully"
            else
                echo -e "${YELLOW}Note: Reseed may require authentication. Configs will be rebuilt on next authenticated reseed.${NC}"
                echo -e "${YELLOW}Response: $response${NC}"
            fi
        else
            echo -e "${YELLOW}No CRM API pod found. Skipping config rebuild - will apply on next deployment.${NC}"
        fi
    else
        echo -e "${YELLOW}kubectl not available. Skipping config rebuild - will apply on next deployment.${NC}"
    fi
    
    print_success "Module config rebuild step completed"
}

# Main execution
main() {
    print_header
    
    if [ "$1" == "--help" ] || [ "$1" == "-h" ]; then
        show_usage
        exit 0
    fi
    
    local start_time=$(date +%s)
    
    check_prerequisites
    clean_build
    restore_dependencies
    build_backend
    build_frontend
    run_unit_tests
    run_bvt_tests
    
    # Always rebuild module configs to ensure UI stays in sync
    rebuild_module_configs
    
    if [ "$BUILD_ENV" == "Production" ] || [ "$BUILD_ENV" == "Performance" ]; then
        publish_artifacts
    fi
    
    generate_report
    
    local end_time=$(date +%s)
    local duration=$((end_time - start_time))
    
    echo -e "\n${GREEN}╔══════════════════════════════════════════════════════════════╗${NC}"
    echo -e "${GREEN}║                    BUILD SUCCESSFUL                          ║${NC}"
    echo -e "${GREEN}║               Total Time: ${duration}s                                ║${NC}"
    echo -e "${GREEN}╚══════════════════════════════════════════════════════════════╝${NC}"
}

main "$@"
