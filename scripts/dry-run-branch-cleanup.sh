#!/bin/bash

# Dry-run script to show what branches would be deleted
# This script does NOT actually delete anything

set -e

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_DIR"

echo "========================================"
echo "Branch Cleanup Dry-Run"
echo "========================================"
echo ""

# Define the branches to delete (merged to main)
BRANCHES_TO_DELETE=(
    "copilot/review-architecture-and-standards"
    "copilot/review-solution-architecture"
)

# Protected branches that should NOT be deleted
PROTECTED_BRANCHES=(
    "main"
    "dev"
)

echo "Fetching latest branch information..."
git fetch --all --unshallow 2>/dev/null || git fetch --all
git fetch origin main:refs/remotes/origin/main 2>/dev/null || true
echo ""

echo "Current remote branches:"
git ls-remote --heads origin | awk '{print "  " $2}' | sed 's|refs/heads/||'
echo ""

echo "Branches marked for deletion:"
for branch in "${BRANCHES_TO_DELETE[@]}"; do
    if git ls-remote --heads origin | grep -q "refs/heads/$branch"; then
        echo "  ✓ $branch (exists remotely)"
    else
        echo "  ⚠ $branch (already deleted or not found)"
    fi
done
echo ""

echo "Protected branches (will NOT be deleted):"
for branch in "${PROTECTED_BRANCHES[@]}"; do
    if git ls-remote --heads origin | grep -q "refs/heads/$branch"; then
        echo "  ✓ $branch (exists)"
    else
        echo "  ⚠ $branch (not found)"
    fi
done
echo ""

echo "Verifying merge status:"
for branch in "${BRANCHES_TO_DELETE[@]}"; do
    if ! git ls-remote --heads origin | grep -q "refs/heads/$branch"; then
        echo "  ⚠ $branch - not found remotely"
        continue
    fi
    
    echo "  Checking $branch..."
    git fetch origin "$branch" 2>/dev/null || true
    git fetch origin main 2>/dev/null || true
    
    if git merge-base --is-ancestor "origin/$branch" origin/main 2>/dev/null; then
        echo "    ✓ Merged to main - safe to delete"
    else
        echo "    ✗ NOT merged to main - should NOT delete"
    fi
done
echo ""

echo "========================================"
echo "Summary"
echo "========================================"
echo ""

DELETE_COUNT=0
SKIP_COUNT=0

for branch in "${BRANCHES_TO_DELETE[@]}"; do
    if git ls-remote --heads origin | grep -q "refs/heads/$branch"; then
        if git merge-base --is-ancestor "origin/$branch" origin/main 2>/dev/null; then
            DELETE_COUNT=$((DELETE_COUNT + 1))
        else
            SKIP_COUNT=$((SKIP_COUNT + 1))
        fi
    fi
done

echo "Would delete: $DELETE_COUNT branch(es)"
echo "Would skip: $SKIP_COUNT branch(es)"
echo ""

echo "After cleanup, remaining branches would be:"
echo "  - main"
echo "  - dev"
echo "  - copilot/delete-merged-branches"
echo ""

echo "To execute the actual deletion:"
echo "  1. Use GitHub Actions workflow: .github/workflows/delete-merged-branches.yml"
echo "  2. Use deletion script: ./scripts/delete-merged-branches.sh"
echo "  3. Use manual git commands (see docs/BRANCH_CLEANUP.md)"
