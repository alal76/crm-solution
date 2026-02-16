#!/bin/bash
# Quick reference card for build logging

cat << 'EOF'

╔════════════════════════════════════════════════════════════════╗
║           CRM Solution Build Logging Quick Reference           ║
╚════════════════════════════════════════════════════════════════╝

📁 LOG LOCATION: docs/buildlogs/
  ├── api-build.log
  └── frontend-build.log

🔨 BUILD COMMANDS:
  
  Build both (API + Frontend):
    ./build-with-logging.sh
    ./build-with-logging.sh all

  Build API only:
    ./build-with-logging.sh api

  Build Frontend only:
    ./build-with-logging.sh frontend

  Build and show log tail:
    ./build-with-logging.sh all tail

📖 VIEW LOGS:
  
  Current API build:
    cat docs/buildlogs/api-build.log
    
  Current Frontend build:
    cat docs/buildlogs/frontend-build.log

  Last 50 lines (useful during builds):
    tail -50 docs/buildlogs/api-build.log
    tail -50 docs/buildlogs/frontend-build.log

  Follow in real-time (from another terminal):
    tail -f docs/buildlogs/api-build.log

  Search for errors:
    grep -i "error\|failed" docs/buildlogs/api-build.log

🎯 FEATURES:
  
  ✓ Old logs automatically removed before each build
  ✓ Only latest build log retained
  ✓ Complete build output captured (no truncation)
  ✓ Build timestamps included
  ✓ Failed builds show immediate feedback

⚡ TROUBLESHOOTING:
  
  To rebuild API without cache:
    ./build-with-logging.sh api
  
  To check if build succeeded:
    grep "✓" docs/buildlogs/api-build.log || echo "Build failed"
  
  To see build errors immediately:
    ./build-with-logging.sh api tail

EOF
