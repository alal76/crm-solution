#!/bin/bash
# Start CRM Frontend locally (connects to local API)

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
FRONTEND_DIR="$SCRIPT_DIR/CRM.Frontend"

echo "================================================"
echo "Starting CRM Frontend (Development Mode)"
echo "================================================"
echo ""
echo "📂 Frontend directory: $FRONTEND_DIR"
echo "🔌 API endpoint: http://localhost:5000"
echo "🚀 Frontend will start on: http://localhost:3000"
echo ""
echo "Ctrl+C to stop"
echo ""

cd "$FRONTEND_DIR"

# Check if node_modules exists
if [ ! -d "node_modules" ]; then
    echo "📦 Installing dependencies..."
    npm install
    echo ""
fi

export REACT_APP_API_BASE_URL=http://localhost:5000

npm start
