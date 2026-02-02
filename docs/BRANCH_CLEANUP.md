# Branch Cleanup Documentation

## Overview

This document describes the process for cleaning up merged branches in the crm-solution repository.

## Current Branch Status

### Branches in Repository

| Branch Name | Status | Action |
|------------|---------|--------|
| `main` | Main branch | **KEEP** - Protected |
| `dev` | Active development branch | **KEEP** - Protected |
| `copilot/delete-merged-branches` | Current working branch | **KEEP** - Active PR |
| `copilot/review-architecture-and-standards` | Merged to main (PR #3) | **DELETE** |
| `copilot/review-solution-architecture` | Merged to main (PR #2) | **DELETE** |
| `copilot/remove-hardcoded-values` | Merged to main (PR #1) | Already deleted ✓ |

## Merged Pull Requests

The following pull requests have been merged to `main` and their branches should be deleted:

1. **PR #1**: `copilot/remove-hardcoded-values` - Add build type system
   - **Status**: Already deleted ✓
   - **Merged**: 2026-02-02

2. **PR #2**: `copilot/review-solution-architecture` - Architecture review
   - **Status**: Needs deletion
   - **Merged**: 2026-02-02
   - **Verified**: Merged to main

3. **PR #3**: `copilot/review-architecture-and-standards` - Code quality standards
   - **Status**: Needs deletion
   - **Merged**: 2026-02-02
   - **Verified**: Merged to main

## Branch Deletion Methods

### Method 1: GitHub Actions Workflow (Recommended)

A GitHub Actions workflow has been created to automate the branch deletion process.

**To run the workflow:**

1. Navigate to the Actions tab in GitHub
2. Select "Delete Merged Branches" workflow
3. Click "Run workflow"
4. (Optional) Customize the branches to delete or use defaults
5. Click "Run workflow" to execute

The workflow will:
- Verify that branches are actually merged to main
- Protect `main` and `dev` branches from deletion
- Delete only the specified merged branches
- Provide a summary of deleted/skipped branches

**Workflow file**: `.github/workflows/delete-merged-branches.yml`

### Method 2: Manual Script Execution

A bash script is provided for manual execution with proper GitHub credentials.

**To run the script:**

```bash
cd /path/to/crm-solution
./scripts/delete-merged-branches.sh
```

The script will:
- Display branches marked for deletion
- Request confirmation before proceeding
- Delete each branch individually
- Show success/failure status for each operation
- Display remaining branches after cleanup

**Script file**: `scripts/delete-merged-branches.sh`

### Method 3: Manual Git Commands

If you have appropriate permissions, you can manually delete branches:

```bash
# Delete individual branches
git push origin --delete copilot/review-architecture-and-standards
git push origin --delete copilot/review-solution-architecture

# Verify deletion
git ls-remote --heads origin
```

## Protected Branches

The following branches are protected and will **NEVER** be deleted:

- `main` - Main production branch
- `dev` - Active development branch

## Verification

After deletion, verify the remaining branches:

```bash
# List all remote branches
git ls-remote --heads origin

# Expected remaining branches:
# - main
# - dev
# - copilot/delete-merged-branches (current PR)
```

## Merge Verification

Before deleting a branch, verify it's merged to main:

```bash
# Check if a branch is merged
git fetch origin main
git fetch origin <branch-name>
git merge-base --is-ancestor origin/<branch-name> origin/main && echo "MERGED" || echo "NOT MERGED"
```

## Best Practices

1. **Always verify merge status** before deleting a branch
2. **Keep active development branches** like `dev`
3. **Protect production branches** like `main`
4. **Document deleted branches** for audit purposes
5. **Use automated workflows** for consistent deletion process

## Troubleshooting

### Authentication Errors

If you encounter authentication errors:
- Ensure you have write permissions to the repository
- Verify your GitHub token has `repo` scope
- Use the GitHub Actions workflow which uses built-in credentials

### Branch Not Found

If a branch doesn't exist:
- It may have already been deleted
- Check remote branches: `git ls-remote --heads origin`

### Branch Not Merged

If a branch shows as not merged:
- **Do not delete** until verified
- Check PR status in GitHub
- Use `git log` to verify merge commit

## References

- GitHub Actions Workflow: `.github/workflows/delete-merged-branches.yml`
- Deletion Script: `scripts/delete-merged-branches.sh`
- Repository: https://github.com/alal76/crm-solution
