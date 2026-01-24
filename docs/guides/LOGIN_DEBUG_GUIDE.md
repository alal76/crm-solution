# Login Debug Guide

## Quick Checklist

### 1. **API Status**
```bash
# Check if API is running and healthy
curl http://localhost:5000/health
# Expected: { "status": "healthy", "timestamp": "..." }
```

### 2. **Direct Login Test**
```bash
# Test login endpoint directly
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"abhi.lal@gmail.com","password":"Admin@123"}'
# Expected: 200 OK with accessToken
```

### 3. **Frontend Port Configuration**
- Internal API port: `5000`
- External API port: `5000`
- Database port: `3306`
- Frontend port: `3000`

### 4. **Browser Console Debugging**
When login fails, check browser console (F12) for:
- `[CRM DEBUG]` logs showing API URL
- `[CRM ERROR]` logs showing error details
- `[LoginPage]` logs showing form submission
- `[AuthContext]` logs showing login attempt/response

## Common Issues & Solutions

### Issue: "Login failed" with no details
**Solution**: 
- Check browser console for detailed error messages
- Verify API is running: `docker-compose ps`
- Check API logs: `docker logs crm-api`
- Verify port 5000 is accessible: `curl localhost:5000/health`

### Issue: API returns 401 "Invalid email or password"
**Solution**:
- Verify correct credentials (email: abhi.lal@gmail.com, password: Admin@123)
- Check database has user: Query database directly if needed
- Default user is seeded in database on first startup

### Issue: Frontend cannot reach API
**Solution**:
- Check Docker network: `docker network ls`
- Verify frontend container has correct API URL:
  - Run: `curl http://localhost:3000/static/js/main.*.js | grep -o 'http://[^"]*:500[01]/api' | head -3`
  - Should show both internal and external port URLs
- Check CORS headers: API should return `Access-Control-Allow-Origin: *`

### Issue: Network error in browser console
**Solution**:
- Check firewall/port accessibility
- Verify containers are on same network: `docker network inspect crm-solution_crm-network`
- Check Docker DNS resolution: `docker exec crm-frontend nslookup api`

## Debug Logging

### Enable Enhanced Logging
- Edit `/CRM.Frontend/src/utils/debug.ts`
- Ensure `DEBUG = true` (already set)
- Rebuild: `docker-compose up --build frontend`

### Console Log Patterns
- `[LoginPage]` - Form submission events
- `[AuthContext]` - Login API calls and responses
- `[CRM DEBUG]` - General debug information
- `[CRM ERROR]` - Error conditions

### API Logging
```bash
# Tail API container logs
docker logs -f crm-api | grep -E "Login|Auth|Error"
```

## Testing Workflow

### 1. Full System Test
```bash
# Check all services are healthy
docker-compose ps

# Health check
curl http://localhost:5000/health

# Test login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"abhi.lal@gmail.com","password":"Admin@123"}'
```

### 2. Frontend Test
1. Open http://localhost:3000 in browser
2. Open Developer Tools (F12)
3. Go to Console tab
4. Look for `[CRM DEBUG]` logs showing:
   - API URL initialization
   - Login attempt with email
5. Enter credentials and submit
6. Check for success or error logs
7. Check Network tab to see actual API requests

### 3. If Still Failing
```bash
# 1. Check API is receiving requests
docker logs crm-api -f

# 2. Check frontend can reach API
docker exec crm-frontend curl http://api:5000/health

# 3. Check from localhost
curl http://localhost:5000/health

# 4. Rebuild everything
docker-compose down
docker-compose up --build -d
```

## Port Configuration Details

The system uses `/CRM.Frontend/src/config/ports.ts` to:
- Cache port configuration from environment variables
- Auto-detect localhost vs Docker environments
- Provide correct API URLs to all components

Environment variables:
- `REACT_APP_API_PORT` (default: 5000)
- `REACT_APP_API_EXTERNAL_PORT` (default: 5000)
- `REACT_APP_DB_PORT` (default: 3306)
- `REACT_APP_FRONTEND_PORT` (default: 3000)

See `docker-compose.yml` for current configuration.
