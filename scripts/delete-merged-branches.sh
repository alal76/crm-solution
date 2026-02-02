#!/bin/bash

# Script to delete branches that have been merged to main
# This script should be run with appropriate GitHub credentials

set -e

REPO_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_DIR"

echo "========================================"
echo "Branch Cleanup Script"
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

echo "Branches marked for deletion:"
for branch in "${BRANCHES_TO_DELETE[@]}"; do
    echo "  - $branch"
done
echo ""

echo "Protected branches (will NOT be deleted):"
for branch in "${PROTECTED_BRANCHES[@]}"; do
    echo "  - $branch"
done
echo ""

# Confirm with user
read -p "Do you want to proceed with deleting these branches? (yes/no): " confirm
if [ "$confirm" != "yes" ]; then
    echo "Aborted."
    exit 0
fi

echo ""
echo "Deleting merged branches..."
echo ""

# Delete each branch
for branch in "${BRANCHES_TO_DELETE[@]}"; do
    echo "Deleting branch: $branch"
    if git push origin --delete "$branch" 2>&1; then
        echo "  ✓ Successfully deleted $branch"
    else
        echo "  ✗ Failed to delete $branch"
    fi
    echo ""
done

echo "========================================"
echo "Branch cleanup complete!"
echo "========================================"
echo ""

# Show remaining branches
echo "Remaining remote branches:"
git ls-remote --heads origin | awk '{print "  " $2}' | sed 's|refs/heads/||'
