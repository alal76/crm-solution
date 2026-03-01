#!/bin/bash
# CRM Solution - Deployment GUI Launcher
# Starts the web-based configuration GUI

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
GUI_DIR="$SCRIPT_DIR/gui"
CDT_PORT=5050

echo ""
echo "=================================================================="
echo "   CRM Solution - Deployment Configuration GUI"
echo "=================================================================="
echo ""

# ── Kill any existing CDT instance ──────────────────────────
echo "[1/3] Checking for existing CDT instances..."
OLD_PIDS=$(lsof -ti :"$CDT_PORT" 2>/dev/null || true)
APP_PIDS=$(pgrep -f 'gui/app\.py' 2>/dev/null || true)
ALL_PIDS=$(echo "$OLD_PIDS $APP_PIDS" | tr ' ' '\n' | sort -u | grep -v '^$' || true)

if [ -n "$ALL_PIDS" ]; then
    echo "  Found existing CDT process(es): $ALL_PIDS"
    for pid in $ALL_PIDS; do
        echo "  Stopping PID $pid..."
        kill "$pid" 2>/dev/null || true
    done
    sleep 2
    # Force-kill survivors
    for pid in $ALL_PIDS; do
        if kill -0 "$pid" 2>/dev/null; then
            echo "  Force-killing PID $pid..."
            kill -9 "$pid" 2>/dev/null || true
        fi
    done
    sleep 1
    echo "  Old instance(s) terminated."
else
    echo "  No existing CDT instance found — clean start."
fi

# ── Check for Python ────────────────────────────────────────
echo "[2/3] Checking Python..."
if ! command -v python3 &> /dev/null; then
    echo "Error: Python 3 is required but not installed."
    exit 1
fi

# Note: Flask and other dependencies are checked at startup by
# the prerequisite checker (prerequisites.py).  If anything is
# missing the user will be prompted to install it automatically.

# ── Start the GUI server ────────────────────────────────────
echo "[3/3] Starting GUI server..."
echo ""
echo "   Open your browser to: http://localhost:$CDT_PORT"
echo ""
echo "   Press Ctrl+C to stop the server"
echo ""
echo "=================================================================="
echo ""

cd "$GUI_DIR"
python3 app.py
