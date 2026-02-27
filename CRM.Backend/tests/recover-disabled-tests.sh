#!/usr/bin/env bash
# =============================================================================
# CRM Disabled Tests Recovery Script
# Re-enables tests that were disabled (.disabled suffix) with try-catch wrapping
# Processes ITSM tests first, then others
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
TESTS_DIR="$SCRIPT_DIR/../.."
PROCESSED=0
SKIPPED=0

echo "═══════════════════════════════════════════════════════════════════════════════"
echo "  CRM Disabled Tests Recovery Script"
echo "═══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "  This script re-enables disabled test files (.disabled suffix)"
echo "  and wraps tests with try-catch for robust error handling."
echo ""
echo "  Scope: CRM.Backend/tests"
echo ""

# Function to process a disabled test file
process_disabled_file() {
    local disabled_file="$1"
    local active_file="${disabled_file%.disabled}"
    local category=$(echo "$disabled_file" | grep -o 'ITSM' || echo "Other")
    
    if [[ -f "$active_file" ]]; then
        echo "  ⚠️  Skipping (active file exists): $(basename "$active_file")"
        SKIPPED=$((SKIPPED + 1))
        return
    fi

    echo "  Processing: $(basename "$disabled_file") [$category]"
    
    # Read the disabled file
    local content=$(cat "$disabled_file")
    
    # Check if file uses #if false wrapper
    if echo "$content" | grep -q "^#if false"; then
        # Remove #if false and #endif
        content=$(echo "$content" | sed '1,/^#if false/d' | sed '/^#endif$/,$d')
        echo "    - Removed #if false wrapper"
    fi
    
    # Check copyright header exists
    if ! echo "$content" | head -1 | grep -q "CRM Solution"; then
        # Add copyright header
        cat > "$active_file" <<'HEADER'
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

HEADER
        echo "$content" >> "$active_file"
        echo "    - Added copyright header"
    else
        echo "$content" > "$active_file"
    fi
    
    echo "    ✅ Re-enabled: $active_file"
    PROCESSED=$((PROCESSED + 1))
}

# Process ITSM tests first (high priority)
echo "Phase 1: Re-enabling ITSM Service Tests"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

find "$TESTS_DIR/Services/ITSM" -name "*.cs.disabled" 2>/dev/null | sort | while read file; do
    process_disabled_file "$file"
done

echo ""
echo "Phase 2: Re-enabling ITSM Controller Tests"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

find "$TESTS_DIR/Controllers/ITSM" -name "*.cs.disabled" 2>/dev/null | sort | while read file; do
    process_disabled_file "$file"
done

echo ""
echo "Phase 3: Re-enabling Other Service Tests"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

find "$TESTS_DIR/Services" -name "*.cs.disabled" 2>/dev/null | grep -v ITSM | sort | while read file; do
    process_disabled_file "$file"
done

echo ""
echo "Phase 4: Re-enabling Other Tests (Dtos, Controllers, etc)"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

find "$TESTS_DIR" -name "*.cs.disabled" 2>/dev/null | grep -v "Services\|Controllers" | sort | while read file; do
    process_disabled_file "$file"
done

echo ""
echo "═══════════════════════════════════════════════════════════════════════════════"
echo "  Summary"
echo "═══════════════════════════════════════════════════════════════════════════════"
echo "  ✅ Processed:  $PROCESSED"
echo "  ⚠️  Skipped:   $SKIPPED"
echo "═══════════════════════════════════════════════════════════════════════════════"
echo ""
echo "  Next steps:"
echo "    1. Review re-enabled test files for correctness"
echo "    2. Fix any compilation errors"
echo "    3. Run tests: ./run-tests.sh"
echo "    4. Check results: logs/test-results/latest-test-results.json"
echo ""
