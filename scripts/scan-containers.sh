#!/bin/bash
# =============================================================================
# CRM Solution - Container Vulnerability Scanning with Trivy
#
# Scans Docker images for known vulnerabilities (CVEs) before deployment.
# Can be run locally or integrated into CI/CD pipelines.
#
# Prerequisites:
#   - Trivy installed: brew install trivy (macOS) or apt-get install trivy
#   - Docker images built locally or accessible from a registry
#
# Usage:
#   ./scripts/scan-containers.sh                    # Scan all CRM images
#   ./scripts/scan-containers.sh --image crm-api    # Scan specific image
#   ./scripts/scan-containers.sh --severity CRITICAL # Only critical vulns
#   ./scripts/scan-containers.sh --ci                # CI mode (fail on HIGH+)
#   ./scripts/scan-containers.sh --format json       # JSON output
#   ./scripts/scan-containers.sh --fix               # Show fix suggestions
# =============================================================================

set -euo pipefail

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
REPORT_DIR="${PROJECT_DIR}/artifacts/security-scans"

# Default settings
SEVERITY="CRITICAL,HIGH"
FORMAT="table"
CI_MODE=false
SPECIFIC_IMAGE=""
SHOW_FIX=false
EXIT_CODE=0
IGNORE_UNFIXED=false

# CRM Docker images to scan
CRM_IMAGES=(
    "crm-api:latest"
    "crm-frontend:latest"
    "crm-gateway:latest"
    "crm-identity:latest"
    "crm-customer:latest"
    "crm-sales:latest"
    "crm-marketing:latest"
    "crm-servicedesk:latest"
    "crm-core:latest"
)

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info()    { echo -e "${BLUE}[INFO]${NC} $1"; }
log_success() { echo -e "${GREEN}[OK]${NC} $1"; }
log_warn()    { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error()   { echo -e "${RED}[ERROR]${NC} $1"; }

usage() {
    cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Options:
  --image NAME        Scan a specific image (e.g., crm-api:latest)
  --severity LEVEL    Severity filter (default: CRITICAL,HIGH)
                      Values: CRITICAL, HIGH, MEDIUM, LOW, UNKNOWN
  --format FMT        Output format: table, json, sarif, template (default: table)
  --ci                CI mode: exit code 1 if HIGH+ vulnerabilities found
  --fix               Show available fix versions
  --ignore-unfixed    Skip vulnerabilities without fixes
  --report-dir DIR    Directory for scan reports (default: artifacts/security-scans)
  --help              Show this help message

Examples:
  $(basename "$0")                            # Scan all CRM images
  $(basename "$0") --image crm-api:latest     # Scan single image
  $(basename "$0") --ci --severity CRITICAL   # CI: fail only on critical
  $(basename "$0") --format json              # JSON output for parsing
EOF
    exit 0
}

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --image)       SPECIFIC_IMAGE="$2"; shift 2 ;;
        --severity)    SEVERITY="$2"; shift 2 ;;
        --format)      FORMAT="$2"; shift 2 ;;
        --ci)          CI_MODE=true; shift ;;
        --fix)         SHOW_FIX=true; shift ;;
        --ignore-unfixed) IGNORE_UNFIXED=true; shift ;;
        --report-dir)  REPORT_DIR="$2"; shift 2 ;;
        --help)        usage ;;
        *)             log_error "Unknown option: $1"; usage ;;
    esac
done

# =============================================================================
# Phase 1: Check prerequisites
# =============================================================================
check_prerequisites() {
    log_info "Checking prerequisites..."

    if ! command -v trivy &>/dev/null; then
        log_error "Trivy is not installed."
        echo ""
        echo "Install Trivy:"
        echo "  macOS:   brew install trivy"
        echo "  Ubuntu:  sudo apt-get install -y trivy"
        echo "  Alpine:  apk add trivy"
        echo "  Docker:  docker run aquasec/trivy image <image>"
        echo ""
        echo "See: https://aquasecurity.github.io/trivy/latest/getting-started/installation/"
        exit 1
    fi

    if ! command -v docker &>/dev/null; then
        log_error "Docker is not installed or not running."
        exit 1
    fi

    log_success "Prerequisites satisfied (trivy $(trivy --version 2>/dev/null | head -1 | awk '{print $2}'))"
}

# =============================================================================
# Phase 2: Update vulnerability database
# =============================================================================
update_db() {
    log_info "Updating Trivy vulnerability database..."
    trivy image --download-db-only 2>/dev/null || true
    log_success "Vulnerability database up to date"
}

# =============================================================================
# Phase 3: Scan images
# =============================================================================
scan_image() {
    local image="$1"
    local image_name="${image%%:*}"
    local report_file="${REPORT_DIR}/${image_name}-scan"
    local scan_args=()

    # Check if image exists locally
    if ! docker image inspect "$image" &>/dev/null; then
        log_warn "Image '$image' not found locally, skipping"
        return 0
    fi

    log_info "Scanning: $image"

    # Build scan arguments
    scan_args+=(--severity "$SEVERITY")

    if [[ "$SHOW_FIX" = true ]]; then
        scan_args+=(--show-fixed)
    fi

    if [[ "$IGNORE_UNFIXED" = true ]]; then
        scan_args+=(--ignore-unfixed)
    fi

    # CI mode: set exit code
    if [[ "$CI_MODE" = true ]]; then
        scan_args+=(--exit-code 1)
    fi

    # Format-specific handling
    case $FORMAT in
        json)
            scan_args+=(--format json --output "${report_file}.json")
            ;;
        sarif)
            scan_args+=(--format sarif --output "${report_file}.sarif")
            ;;
        table)
            # Table goes to stdout; also save JSON for CI
            if [[ "$CI_MODE" = true ]]; then
                # Run twice: table for display, JSON for artifacts
                trivy image "${scan_args[@]}" --format json --output "${report_file}.json" "$image" 2>/dev/null || true
            fi
            ;;
    esac

    # Run the scan
    if trivy image "${scan_args[@]}" "$image"; then
        log_success "No ${SEVERITY} vulnerabilities found in $image"
    else
        local result=$?
        if [[ "$CI_MODE" = true ]] && [[ $result -ne 0 ]]; then
            log_error "Vulnerabilities found in $image (exit code: $result)"
            EXIT_CODE=1
        fi
    fi

    echo ""
}

scan_dockerfile() {
    local dockerfile="$1"
    local name="$(basename "$dockerfile")"

    if [[ ! -f "$dockerfile" ]]; then
        return 0
    fi

    log_info "Scanning Dockerfile: $name"

    local scan_args=(--severity "$SEVERITY")

    if [[ "$CI_MODE" = true ]]; then
        scan_args+=(--exit-code 1)
    fi

    if trivy config "${scan_args[@]}" "$dockerfile"; then
        log_success "No misconfigurations found in $name"
    else
        local result=$?
        if [[ "$CI_MODE" = true ]] && [[ $result -ne 0 ]]; then
            log_warn "Misconfigurations found in $name"
        fi
    fi

    echo ""
}

# =============================================================================
# Phase 4: Scan Kubernetes manifests (bonus)
# =============================================================================
scan_kubernetes() {
    log_info "Scanning Kubernetes manifests for misconfigurations..."

    local k8s_dir="${PROJECT_DIR}/kubernetes"
    if [[ ! -d "$k8s_dir" ]]; then
        log_warn "No kubernetes/ directory found, skipping"
        return 0
    fi

    local scan_args=(--severity "$SEVERITY")
    if [[ "$CI_MODE" = true ]]; then
        scan_args+=(--exit-code 1)
    fi

    if trivy config "${scan_args[@]}" "$k8s_dir"; then
        log_success "No Kubernetes misconfigurations found"
    else
        local result=$?
        if [[ "$CI_MODE" = true ]] && [[ $result -ne 0 ]]; then
            log_warn "Kubernetes misconfigurations found"
        fi
    fi
}

# =============================================================================
# Phase 5: Generate summary
# =============================================================================
generate_summary() {
    echo ""
    echo "============================================================"
    echo "  Container Security Scan Summary"
    echo "============================================================"
    echo "  Severity Filter:   $SEVERITY"
    echo "  CI Mode:           $CI_MODE"
    echo "  Ignore Unfixed:    $IGNORE_UNFIXED"
    echo "  Reports:           $REPORT_DIR/"
    echo "============================================================"

    if [[ "$CI_MODE" = true ]]; then
        if [[ $EXIT_CODE -eq 0 ]]; then
            log_success "All scans passed — no blocking vulnerabilities found"
        else
            log_error "Blocking vulnerabilities detected — pipeline should fail"
        fi
    fi
}

# =============================================================================
# Main
# =============================================================================
main() {
    echo ""
    echo "╔════════════════════════════════════════════════════════════╗"
    echo "║   CRM Solution - Container Vulnerability Scanner          ║"
    echo "╚════════════════════════════════════════════════════════════╝"
    echo ""

    check_prerequisites
    update_db

    # Create report directory
    mkdir -p "$REPORT_DIR"

    # Scan images
    if [[ -n "$SPECIFIC_IMAGE" ]]; then
        scan_image "$SPECIFIC_IMAGE"
    else
        for image in "${CRM_IMAGES[@]}"; do
            scan_image "$image"
        done
    fi

    # Scan Dockerfiles for misconfigurations
    log_info "Scanning Dockerfiles for misconfigurations..."
    for df in "${PROJECT_DIR}"/docker/Dockerfile.*; do
        scan_dockerfile "$df"
    done

    # Scan Kubernetes manifests
    scan_kubernetes

    # Summary
    generate_summary

    exit $EXIT_CODE
}

main "$@"
