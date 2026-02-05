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

# Check and install Flask if needed
if ! python3 -c "import flask" 2>/dev/null; then
    echo "Installing Flask..."
    pip3 install flask --quiet
fi

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
