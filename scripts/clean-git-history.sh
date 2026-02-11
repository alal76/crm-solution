#!/bin/bash
# =============================================================================
# CRM Solution - Git History Cleanup Script
# Purpose: Remove plaintext passwords, SSL certificates, and other secrets
#          from Git history using git-filter-repo or BFG Repo Cleaner.
#
# IMPORTANT: This is a DESTRUCTIVE operation that rewrites Git history.
#            All team members must re-clone the repository after this runs.
#
# Prerequisites:
#   Option A (recommended): pip install git-filter-repo
#   Option B: Download BFG from https://rtyley.github.io/bfg-repo-cleaner/
#
# Usage:
#   ./scripts/clean-git-history.sh [--dry-run] [--method bfg|filter-repo]
#
# Created: February 2026
# =============================================================================

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

DRY_RUN=false
METHOD="auto"

# Parse arguments
while [[ $# -gt 0 ]]; do
    case "$1" in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --method)
            METHOD="$2"
            shift 2
            ;;
        -h|--help)
            echo "Usage: $0 [--dry-run] [--method bfg|filter-repo]"
            echo ""
            echo "Options:"
            echo "  --dry-run         Show what would be cleaned without modifying history"
            echo "  --method          Force a specific cleanup tool (bfg or filter-repo)"
            echo ""
            echo "IMPORTANT: This rewrites Git history. All team members must re-clone after."
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown argument: $1${NC}"
            exit 1
            ;;
    esac
done

# =============================================================================
# Secrets to remove from history
# =============================================================================

# File: List of file patterns to completely remove from history
SENSITIVE_FILES=(
    "ssl/server.pfx"
    "ssl/server.key"
    "ssl/server.crt"
    "ssl/server.pem"
    "*.pfx"
    "*.p12"
)

# Text: Plaintext passwords/secrets to replace in all files across history
# Format: "secret_value" -> each will be replaced with "***REDACTED***"
SENSITIVE_STRINGS=(
    "CrmSslCert2024"
    "CrmPass@Dev2024!"
    "CrmAdmin2024!"
    "RootPass@Dev2024"
    "CrmPass@Dev2024"
)

# =============================================================================
# Pre-flight checks
# =============================================================================

echo -e "${BLUE}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     CRM Solution - Git History Cleanup                  ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""

if [[ "$DRY_RUN" == "true" ]]; then
    echo -e "${YELLOW}🔍 DRY RUN MODE — no changes will be made${NC}"
    echo ""
fi

# Ensure we're in a git repo
if ! git -C "$PROJECT_ROOT" rev-parse --is-inside-work-tree &>/dev/null; then
    echo -e "${RED}Error: Not a Git repository: $PROJECT_ROOT${NC}"
    exit 1
fi

# Check for uncommitted changes
if ! git -C "$PROJECT_ROOT" diff --quiet HEAD 2>/dev/null; then
    echo -e "${RED}Error: You have uncommitted changes. Commit or stash them first.${NC}"
    exit 1
fi

# Detect available tool
detect_tool() {
    if [[ "$METHOD" == "filter-repo" ]] || [[ "$METHOD" == "auto" ]]; then
        if command -v git-filter-repo &>/dev/null; then
            echo "filter-repo"
            return
        fi
    fi
    if [[ "$METHOD" == "bfg" ]] || [[ "$METHOD" == "auto" ]]; then
        if command -v bfg &>/dev/null || [[ -f "$SCRIPT_DIR/bfg.jar" ]]; then
            echo "bfg"
            return
        fi
    fi
    echo "none"
}

TOOL=$(detect_tool)

if [[ "$TOOL" == "none" ]]; then
    echo -e "${RED}Error: No cleanup tool found.${NC}"
    echo ""
    echo "Install one of the following:"
    echo "  Option A (recommended): pip install git-filter-repo"
    echo "  Option B: brew install bfg   (or download from https://rtyley.github.io/bfg-repo-cleaner/)"
    exit 1
fi

echo -e "${GREEN}Using tool: ${TOOL}${NC}"
echo ""

# =============================================================================
# Phase 1: Scan for sensitive content (dry run or pre-check)
# =============================================================================

echo -e "${BLUE}Phase 1: Scanning for sensitive content in history...${NC}"
echo ""

FOUND_ISSUES=0

# Check for sensitive files in history
echo "Checking for sensitive files..."
for pattern in "${SENSITIVE_FILES[@]}"; do
    # Search entire git history for the file
    matches=$(git -C "$PROJECT_ROOT" log --all --pretty=format: --name-only --diff-filter=A 2>/dev/null | grep -c "$pattern" || true)
    if [[ "$matches" -gt 0 ]]; then
        echo -e "  ${YELLOW}⚠  Found $pattern in $matches commit(s)${NC}"
        FOUND_ISSUES=$((FOUND_ISSUES + 1))
    else
        echo -e "  ${GREEN}✓  $pattern not found in history${NC}"
    fi
done

# Check for sensitive strings in tracked files
echo ""
echo "Checking for sensitive strings in current tracked files..."
for secret in "${SENSITIVE_STRINGS[@]}"; do
    matches=$(git -C "$PROJECT_ROOT" grep -rl "$secret" 2>/dev/null | wc -l | tr -d ' ')
    if [[ "$matches" -gt 0 ]]; then
        echo -e "  ${YELLOW}⚠  \"${secret:0:4}...\" found in $matches current file(s)${NC}"
        git -C "$PROJECT_ROOT" grep -rl "$secret" 2>/dev/null | head -5 | sed 's/^/     /'
        FOUND_ISSUES=$((FOUND_ISSUES + 1))
    else
        echo -e "  ${GREEN}✓  \"${secret:0:4}...\" not found in current files${NC}"
    fi
done

echo ""
echo -e "Found ${YELLOW}${FOUND_ISSUES}${NC} issue(s) to clean."
echo ""

if [[ "$DRY_RUN" == "true" ]]; then
    echo -e "${YELLOW}DRY RUN complete. Run without --dry-run to perform cleanup.${NC}"
    exit 0
fi

if [[ "$FOUND_ISSUES" -eq 0 ]]; then
    echo -e "${GREEN}No sensitive content found. History is clean.${NC}"
    exit 0
fi

# =============================================================================
# Phase 2: Confirmation
# =============================================================================

echo -e "${RED}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${RED}║  WARNING: This will rewrite Git history!                ║${NC}"
echo -e "${RED}║  All team members must re-clone after this operation.   ║${NC}"
echo -e "${RED}║  Ensure you have a backup of the repository.           ║${NC}"
echo -e "${RED}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""
read -p "Type 'YES' to proceed: " confirmation

if [[ "$confirmation" != "YES" ]]; then
    echo "Aborted."
    exit 1
fi

# =============================================================================
# Phase 3: Create backup
# =============================================================================

BACKUP_DIR="${PROJECT_ROOT}/../crm-solution-backup-$(date +%Y%m%d-%H%M%S)"
echo ""
echo -e "${BLUE}Phase 3: Creating backup at ${BACKUP_DIR}...${NC}"
git -C "$PROJECT_ROOT" clone --mirror "$PROJECT_ROOT" "$BACKUP_DIR"
echo -e "${GREEN}Backup created at: ${BACKUP_DIR}${NC}"

# =============================================================================
# Phase 4: Clean history
# =============================================================================

echo ""
echo -e "${BLUE}Phase 4: Cleaning Git history...${NC}"
echo ""

if [[ "$TOOL" == "filter-repo" ]]; then
    # =========================================================================
    # Method: git-filter-repo
    # =========================================================================

    # Step 1: Remove sensitive files from history
    echo "Removing sensitive files from history..."
    PATHS_ARGS=""
    for pattern in "${SENSITIVE_FILES[@]}"; do
        if [[ "$pattern" == *"*"* ]]; then
            # Glob pattern — use --path-glob
            PATHS_ARGS="$PATHS_ARGS --path-glob $pattern"
        else
            PATHS_ARGS="$PATHS_ARGS --path $pattern"
        fi
    done

    if [[ -n "$PATHS_ARGS" ]]; then
        # shellcheck disable=SC2086
        git -C "$PROJECT_ROOT" filter-repo --invert-paths $PATHS_ARGS --force
        echo -e "${GREEN}✓ Sensitive files removed from history${NC}"
    fi

    # Step 2: Replace sensitive strings in all blobs
    echo "Replacing sensitive strings in history..."
    REPLACEMENTS_FILE=$(mktemp)
    for secret in "${SENSITIVE_STRINGS[@]}"; do
        echo "${secret}==>***REDACTED***" >> "$REPLACEMENTS_FILE"
    done

    git -C "$PROJECT_ROOT" filter-repo --replace-text "$REPLACEMENTS_FILE" --force
    rm -f "$REPLACEMENTS_FILE"
    echo -e "${GREEN}✓ Sensitive strings replaced in history${NC}"

elif [[ "$TOOL" == "bfg" ]]; then
    # =========================================================================
    # Method: BFG Repo Cleaner
    # =========================================================================

    BFG_CMD="bfg"
    if [[ -f "$SCRIPT_DIR/bfg.jar" ]]; then
        BFG_CMD="java -jar $SCRIPT_DIR/bfg.jar"
    fi

    # Step 1: Remove sensitive files
    echo "Removing sensitive files from history..."
    for pattern in "${SENSITIVE_FILES[@]}"; do
        filename=$(basename "$pattern")
        $BFG_CMD --delete-files "$filename" "$PROJECT_ROOT" || true
    done
    echo -e "${GREEN}✓ Sensitive files removed from history${NC}"

    # Step 2: Replace sensitive strings
    echo "Replacing sensitive strings in history..."
    REPLACEMENTS_FILE=$(mktemp)
    for secret in "${SENSITIVE_STRINGS[@]}"; do
        echo "$secret" >> "$REPLACEMENTS_FILE"
    done

    $BFG_CMD --replace-text "$REPLACEMENTS_FILE" "$PROJECT_ROOT"
    rm -f "$REPLACEMENTS_FILE"
    echo -e "${GREEN}✓ Sensitive strings replaced in history${NC}"
fi

# =============================================================================
# Phase 5: Garbage collect
# =============================================================================

echo ""
echo -e "${BLUE}Phase 5: Running garbage collection...${NC}"
cd "$PROJECT_ROOT"
git reflog expire --expire=now --all
git gc --prune=now --aggressive
echo -e "${GREEN}✓ Garbage collection complete${NC}"

# =============================================================================
# Phase 6: Verify
# =============================================================================

echo ""
echo -e "${BLUE}Phase 6: Verifying cleanup...${NC}"
echo ""

REMAINING_ISSUES=0

for secret in "${SENSITIVE_STRINGS[@]}"; do
    matches=$(git -C "$PROJECT_ROOT" grep -rl "$secret" 2>/dev/null | wc -l | tr -d ' ')
    if [[ "$matches" -gt 0 ]]; then
        echo -e "  ${RED}✗  \"${secret:0:4}...\" still found in $matches file(s)${NC}"
        REMAINING_ISSUES=$((REMAINING_ISSUES + 1))
    else
        echo -e "  ${GREEN}✓  \"${secret:0:4}...\" cleaned${NC}"
    fi
done

echo ""
if [[ "$REMAINING_ISSUES" -gt 0 ]]; then
    echo -e "${YELLOW}Warning: $REMAINING_ISSUES issue(s) may remain in current files.${NC}"
    echo "These are in the current working tree and may need manual editing."
else
    echo -e "${GREEN}All sensitive content has been cleaned from history.${NC}"
fi

# =============================================================================
# Phase 7: Next steps
# =============================================================================

echo ""
echo -e "${BLUE}╔══════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Cleanup Complete — Next Steps                         ║${NC}"
echo -e "${BLUE}╚══════════════════════════════════════════════════════════╝${NC}"
echo ""
echo "  1. Review the changes: git log --oneline -20"
echo "  2. Force push to remote: git push origin --force --all"
echo "  3. Force push tags: git push origin --force --tags"
echo "  4. Notify all team members to re-clone the repository"
echo "  5. Rotate ALL credentials that were in the repository:"
echo "     - SSL certificate password"
echo "     - Database passwords (CrmPass@Dev2024, RootPass@Dev2024)"
echo "     - Monitoring passwords (CrmAdmin2024!)"
echo "  6. Delete the backup when satisfied: rm -rf $BACKUP_DIR"
echo ""
echo -e "${YELLOW}Backup location: ${BACKUP_DIR}${NC}"
