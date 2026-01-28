#!/bin/bash
# Launch CRM Solution Deployment Tool
# Version: 0.0.24

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Starting CRM Solution Deployment Tool..."
echo "========================================="

# Check Python version
PYTHON_CMD=""
if command -v python3 &> /dev/null; then
    PYTHON_CMD="python3"
elif command -v python &> /dev/null; then
    PYTHON_CMD="python"
else
    echo "ERROR: Python 3 is required but not found."
    echo "Please install Python 3.9+ from https://python.org"
    exit 1
fi

# Check Python version is 3.9+
PY_VERSION=$($PYTHON_CMD -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')")
echo "Found Python $PY_VERSION"

# Check tkinter is available
if ! $PYTHON_CMD -c "import tkinter" &> /dev/null; then
    echo "ERROR: tkinter is not available."
    echo ""
    echo "On macOS: brew install python-tk"
    echo "On Ubuntu/Debian: sudo apt-get install python3-tk"
    echo "On Fedora: sudo dnf install python3-tkinter"
    exit 1
fi

# Run the tool
cd "$SCRIPT_DIR/deployment-tool"
$PYTHON_CMD main.py
