#!/bin/bash
# Quick Start - Local Development

cat << 'EOF'

╔════════════════════════════════════════════════════════════════╗
║         CRM Solution - Local Development Quick Start           ║
╚════════════════════════════════════════════════════════════════╝

📦 PREREQUISITES
  ✓ .NET SDK 10.0+ (dotnet --version)
  ✓ Node.js 18+ (node --version)
  ✓ npm 9+ (npm --version)
  ✓ Access to 192.168.0.9 (database server)

═══════════════════════════════════════════════════════════════════

🚀 FASTEST WAY (Start Both Services)

  Terminal 1 (from project root):
  cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
  ./start-dev.sh

  Then open browser:
  http://localhost:3000

═══════════════════════════════════════════════════════════════════

🎯 INDIVIDUAL SERVICES

  Backend API Only:
  cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
  ./start-api.sh
  → http://localhost:5000/swagger

  Frontend Only:
  cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
  ./start-frontend.sh
  → http://localhost:3000

═══════════════════════════════════════════════════════════════════

🔍 DEBUG THE ESCALATIONRULE ISSUE

  Goal: See if the modelBuilder.Ignore() fix works

  Run API:
  ./start-api.sh

  Expected: "Admin Configuration Services registered..."
  NOT: "Cannot use table 'EscalationRule'..."

  If still failing:
  - Watch console for ACTUAL exception (full stack trace)
  - Not hidden generic "An error occurred during login"
  - Edit CrmDbContext.cs directly
  - Ctrl+C to stop API
  - Run again: ./start-api.sh (~3 seconds)

═══════════════════════════════════════════════════════════════════

🧪 TEST LOGIN

  1. Open: http://localhost:3000
  2. Email: admin@crm.local
  3. Password: Admin@123
  4. Watch API console for actual errors

═══════════════════════════════════════════════════════════════════

⚡ PERFORMANCE (vs Docker)

  Cycle Speed:
    Docker build → Deploy → Test: 4 minutes
    Local dev (edit → save → reload): 5-10 seconds

  Error Visibility:
    Docker: "An error occurred during login"
    Local: Full stack trace + line numbers

═══════════════════════════════════════════════════════════════════

🔧 CONFIG FILES

  API Config:
  CRM.Backend/src/CRM.Api/appsettings.Development.json
  → Points to 192.168.0.9:3306

  Frontend Config:
  Environment: REACT_APP_API_BASE_URL=http://localhost:5000

═══════════════════════════════════════════════════════════════════

🆘 TROUBLESHOOTING

  Port 5000 already in use:
  lsof -i :5000 | grep LISTEN
  kill -9 <PID>

  Port 3000 already in use:
  lsof -i :3000 | grep LISTEN
  kill -9 <PID>

  Can't connect to 192.168.0.9:
  ping 192.168.0.9
  nc -zv 192.168.0.9 3306

  Dependencies missing:
  dotnet restore
  npm install

═══════════════════════════════════════════════════════════════════

📚 FULL GUIDE
  See: LOCAL_DEVELOPMENT.md

EOF
