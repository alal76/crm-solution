# Build Logs Directory

This directory contains the latest build logs for the CRM solution components.

## Files

- **api-build.log** - Backend API Docker build log (created/updated when building API)
- **frontend-build.log** - Frontend Docker build log (created/updated when building frontend)

## Usage

### Build with Centralized Logging

From the project root directory, use the build script:

```bash
# Build both API and frontend (removes old logs first)
./build-with-logging.sh

# Build only API
./build-with-logging.sh api

# Build only frontend
./build-with-logging.sh frontend

# Build both and show last 20 lines of each log
./build-with-logging.sh all tail
```

## How It Works

1. **Automatic Cleanup**: Old log files are removed before each build
2. **Complete Logs**: Full build output is captured including all warnings and errors
3. **Timestamps**: Each log includes start and completion timestamps
4. **Quick Reference**: Failed builds show tail of log immediately in terminal

## Viewing Logs

```bash
# View API build log
cat docs/buildlogs/api-build.log

# Show last 50 lines
tail -50 docs/buildlogs/api-build.log

# Search for errors
grep -i "error\|failed" docs/buildlogs/api-build.log

# Follow log during build (in another terminal)
tail -f docs/buildlogs/api-build.log
```

## Integration with TaskRunner

If using VS Code Task Runner, add to `.vscode/tasks.json`:

```json
{
    "label": "Build with Logging",
    "type": "shell",
    "command": "${workspaceFolder}/build-with-logging.sh",
    "group": {
        "kind": "build",
        "isDefault": true
    },
    "presentation": {
        "reveal": "always",
        "panel": "shared"
    }
}
```

## Log Contents

Each log file contains:
- Timestamp (started, completed)
- Full build command used
- Complete Docker build output
- All compiler warnings and errors
- Build status (success/failed)
