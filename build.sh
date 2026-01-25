#!/bin/bash
# =============================================================================
# CRM Build - Root Convenience Script
# =============================================================================
# Wrapper to run the modular build system from project root
# Usage: ./build.sh [args]
# =============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "$SCRIPT_DIR/scripts/build/quick-build.sh" "$@"
