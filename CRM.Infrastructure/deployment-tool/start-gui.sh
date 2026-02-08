#!/bin/bash
# CRM Solution - Deployment GUI Launcher
# Starts the web-based configuration GUI

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUI_DIR="$SCRIPT_DIR/gui"

echo ""
echo "=================================================================="
echo "   CRM Solution - Deployment Configuration GUI"
echo "=================================================================="
echo ""

# Check for Python
if ! command -v python3 &> /dev/null; then
    echo "Error: Python 3 is required but not installed."
    exit 1
fi

# Note: Flask and other dependencies are checked at startup by
# the prerequisite checker (prerequisites.py).  If anything is
# missing the user will be prompted to install it automatically.

# Start the GUI server
echo "Starting GUI server..."
echo ""
echo "   Open your browser to: http://localhost:5050"
echo ""
echo "   Press Ctrl+C to stop the server"
echo ""
echo "=================================================================="
echo ""

cd "$GUI_DIR"
python3 app.py
