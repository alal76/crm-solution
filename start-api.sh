#!/bin/bash
# Start CRM API locally (connects to 192.168.0.9 database)

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
API_DIR="$SCRIPT_DIR/CRM.Backend/src/CRM.Api"

echo "================================================"
echo "Starting CRM Backend API (Development Mode)"
echo "================================================"
echo ""
echo "📂 CRM.Api directory: $API_DIR"
echo "🗄️  Database: 192.168.0.9:3306/crm_db"
echo "🚀 API will start on: http://localhost:5000"
echo "📚 Swagger: http://localhost:5000/swagger"
echo ""
echo "Ctrl+C to stop"
echo ""

cd "$API_DIR"
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5000"

dotnet run --no-launch-profile
