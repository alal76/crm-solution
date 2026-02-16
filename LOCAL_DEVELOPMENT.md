# Local Development Setup

Fast iterative development: Run API & Frontend locally while connected to remote database.

## Quick Start (All-in-One)

```bash
cd /Users/alal/Code/Git\ CRM\ Solution/crm-solution
./start-dev.sh
```

This starts:
- ✅ Backend API on `http://localhost:5000` 
- ✅ Frontend on `http://localhost:3000`
- ✅ Connected to remote database: `192.168.0.9:3306/crm_db`

**Startup time: ~15-20 seconds** 🚀

## Individual Services

### Backend API Only

```bash
./start-api.sh
```

- Runs on: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- Database: `192.168.0.9:3306/crm_db`
- Config: `CRM.Backend/src/CRM.Api/appsettings.Development.json`

### Frontend Only

```bash
./start-frontend.sh
```

- Runs on: `http://localhost:3000`
- API endpoint: `http://localhost:5000` (auto-configured)
- Hot-reload enabled

## Configuration

### Database Connection
File: `CRM.Backend/src/CRM.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.0.9;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;"
  }
}
```

### Frontend API Endpoint
Environment variable: `REACT_APP_API_BASE_URL`

Default: `http://localhost:5000`

## Development Workflow

### 1. Fix EF Core Issue (testable immediately)

Edit: `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`

```bash
./start-api.sh
```

See errors in console **instantly**, no Docker builds needed.

### 2. Test Login

1. Frontend opens `http://localhost:3000`
2. Enter credentials: `admin@crm.local` / `Admin@123`
3. Watch API console for actual errors vs generic "An error occurred"

### 3. Change Code

```bash
# Terminal 1: API running
Ctrl+C
# Edit code
./start-api.sh
# Runs in ~3 seconds, test again
```

### 4. Frontend Changes

```bash
# Terminal 2: Frontend running
# Edit component, save
# Browser auto-refreshes (Hot Module Replacement)
```

## Debugging

### View Real Exceptions

API console shows full stack traces:
```
System.InvalidOperationException: Cannot use table 'EscalationRule'...
   at Microsoft.EntityFrameworkCore...
```

vs. Hidden behind "An error occurred during login" from Docker.

### Visual Studio Code Debugging (Optional)

`.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/CRM.Backend/src/CRM.Api/bin/Debug/net10.0/CRM.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/CRM.Backend/src/CRM.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "$1",
        "action": "openExternalTerminal"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ASPNETCORE_URLS": "http://localhost:5000"
      }
    }
  ]
}
```

Then press F5 in VS Code to debug with breakpoints.

### Database Connection Test

```bash
# From API console, check connection
# When API starts, look for SQL Server connection messages

# Or test manually:
mysql -h 192.168.0.9 -u crm_user -p crm_db
# Password: CrmPass@Dev2024
```

## Troubleshooting

### "Connection refused" to 192.168.0.9

```bash
# Test connectivity
ping 192.168.0.9
# You should get responses

# Test MySQL port
nc -zv 192.168.0.9 3306
```

### API won't start

```bash
# Check .NET SDK
dotnet --version
# Should be 10.0+

# Clean and rebuild
cd CRM.Backend
dotnet clean
dotnet build
```

### Frontend won't start

```bash
# Check Node version
node --version
npm --version

# Clear cache and reinstall
cd CRM.Frontend
rm -rf node_modules package-lock.json
npm install
```

### Port already in use

```bash
# API port 5000
lsof -i :5000
kill -9 <PID>

# Frontend port 3000
lsof -i :3000
kill -9 <PID>
```

## Performance Comparison

| Operation | Docker Build | Local Dev |
|-----------|------------|-----------|
| Full rebuild | 100s | 5s |
| Test cycle | 4 min | 15-20s |
| Error visibility | Generic message | Full stack trace |
| Code change → test | 4 min | 3-5s |
| Database latency | <10ms | <5ms (LAN) |

## Stopping Services

```bash
# Press Ctrl+C to stop all services gracefully
# Or kill individually:
kill 9 <PID>
```

## Network Architecture

```
Local Machine                   Remote Server (192.168.0.9)
┌──────────────────┐           ┌────────────────┐
│  dotnet run      │──────────→│  MariaDB       │
│  (API:5000)      │           │  (3306)        │
└──────────────────┘           └────────────────┘
        ↑
        │ http://localhost:5000
        ├──────────────────
┌──────────────────┐
│  npm start       │
│  (Frontend:3000) │
└──────────────────┘
```

## Next Steps

1. **Test the EF Core fix:**
   ```bash
   ./start-api.sh
   # Should see: "Admin Configuration Services registered"
   # NOT: "Cannot use table 'EscalationRule'"
   ```

2. **Try login:**
   ```
   http://localhost:3000
   Email: admin@crm.local
   Password: Admin@123
   ```

3. **If still failing:**
   - Watch API console for actual exception
   - Apply fixes directly in source
   - Restart API (Ctrl+C, ./start-api.sh)
   - Test immediately

## Files

- `./start-dev.sh` - Start both API & Frontend
- `./start-api.sh` - Backend only
- `./start-frontend.sh` - Frontend only
- `CRM.Backend/src/CRM.Api/appsettings.Development.json` - API config
- `CRM.Frontend/.env.local` - Frontend config (if needed)
