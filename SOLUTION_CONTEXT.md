# CRM Solution - Complete Context Reference

> **Last Updated:** February 1, 2026  
> **Purpose:** Provide comprehensive context for AI assistants and developers to understand the solution without re-studying the codebase.

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Build System & Deployment Process](#2-build-system--deployment-process)
3. [Secret Generation & Token Management](#3-secret-generation--token-management)
4. [Password Storage & Authentication](#4-password-storage--authentication)
5. [Health Check Endpoints](#5-health-check-endpoints)
6. [Key Architecture Decisions](#6-key-architecture-decisions)
7. [Deployment Configuration](#7-deployment-configuration)
8. [Database Startup & Seeding Process](#8-database-startup--seeding-process)
9. [Key Files Reference](#9-key-files-reference)
10. [API Endpoints Reference](#10-api-endpoints-reference)
11. [Testing](#11-testing)
12. [Common Issues & Solutions](#12-common-issues--solutions)
13. [Development Workflow](#13-development-workflow)
14. [Contact Info Normalization](#14-contact-info-normalization)
15. [Security & CORS](#15-security--cors)
16. [Quick Reference Commands](#16-quick-reference-commands)
17. [Debugging & Troubleshooting](#17-debugging--troubleshooting)
18. [Recent Changes Log](#18-recent-changes-log)
19. [Files Modified in Recent Sessions](#19-files-modified-in-recent-sessions)

---

## 1. Solution Overview

### What is this?
A full-stack CRM (Customer Relationship Management) application with:
- **Backend:** ASP.NET Core 8.0 Web API with Entity Framework Core
- **Frontend:** React 18 with TypeScript and Material-UI (MUI)
- **Database:** MariaDB (primary), SQL Server (supported), PostgreSQL (supported)
- **Caching:** Redis
- **Infrastructure:** Docker, Kubernetes

### Repository Structure
```
crm-solution/
├── CRM.Backend/           # .NET Backend (API, Services, Core, Infrastructure)
│   ├── src/
│   │   ├── CRM.Api/       # Web API controllers, middleware
│   │   ├── CRM.Core/      # Entities, DTOs, Interfaces
│   │   ├── CRM.Infrastructure/  # EF Core, Services, Repositories
│   │   └── CRM.DatabaseSeeder/  # Database seeding utility
│   └── tests/             # Unit and integration tests
├── CRM.Frontend/          # React Frontend
│   └── src/
│       ├── components/    # React components
│       ├── pages/         # Page components
│       ├── services/      # API service layer
│       └── store/         # Redux state management
├── e2e-tests/             # Playwright E2E tests
├── database/              # SQL schema and seed scripts
├── docker/                # Docker configurations
├── kubernetes/            # K8s manifests
└── docs/                  # Documentation
```

---

## 2. Build System & Deployment Process

### Build Scripts Overview

The build system is modular and located in `scripts/build/`:

| Script | Purpose |
|--------|---------|
| `build.sh` | Root convenience wrapper - calls quick-build.sh |
| `quick-build.sh` | Simplified wrapper for common operations |
| `build-modular.sh` | Full-featured modular build with caching |
| `setup-env.sh` | Environment setup and secret generation |

### Build Commands Quick Reference

```bash
# First-time setup (generates secrets, creates .env)
./build.sh setup              # Interactive mode
./build.sh setup --auto       # Auto-generate all secrets
./build.sh setup --dev        # Development defaults

# Build operations
./build.sh                    # Build all modules
./build.sh backend            # Build backend only
./build.sh frontend           # Build frontend only
./build.sh deploy             # Build and deploy to Kubernetes
./build.sh remote             # Build on remote server (192.168.0.9)
./build.sh microservices      # Build all microservices
./build.sh clean              # Clear build cache
./build.sh status             # Show build/deployment status
```

### Advanced Build Options (build-modular.sh)

```bash
# Specific module with version
./scripts/build/build-modular.sh backend --version v50 --no-cache

# Build and push to registry
./scripts/build/build-modular.sh frontend --push

# Dry run (show what would happen)
./scripts/build/build-modular.sh all --dry-run

# Force remote build on server
./scripts/build/build-modular.sh all --remote --deploy
```

### Cross-Platform Build (Mac → Linux Server)

**CRITICAL:** Development Mac is arm64, production server is amd64:

```bash
# Build with platform flag for server deployment
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend --load .

# Transfer to server
docker save crm-api:latest | ssh root@192.168.0.9 "docker load"

# Deploy container
ssh root@192.168.0.9 "docker stop crm-api; docker rm crm-api"
ssh root@192.168.0.9 "docker run -d --name crm-api --network docker_crm-network -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e DatabaseProvider=mariadb \
  -e 'ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;' \
  -e 'Jwt__Secret=ThisIsAVeryLongSecureJwtSecretKeyForDevelopmentPurposesOnly123456789' \
  crm-api:latest"
```

---

## 3. Secret Generation & Token Management

### Environment Setup Process

The `scripts/setup-env.sh` script creates a `.env` file from `.env.example`:

1. **Interactive Mode** (`./build.sh setup`): Prompts for each value
2. **Auto Mode** (`./build.sh setup --auto`): Generates all secrets automatically
3. **Dev Mode** (`./build.sh setup --dev`): Uses development-friendly defaults

### Secrets Generated at Setup

| Secret | Generated By | Purpose |
|--------|-------------|---------|
| `JWT_KEY` / `JWT_SECRET` | `openssl rand -base64 48` | JWT token signing (min 32 chars) |
| `DB_PASSWORD` | Random + special chars | Database app user password |
| `DB_ROOT_PASSWORD` | Random + special chars | MariaDB root password |
| `ADMIN_PASSWORD` | Random + special chars | Initial admin user password |

### Secret Generation Code (from setup-env.sh)

```bash
# Generate a random secure string
generate_secret() {
    local length=${1:-32}
    openssl rand -base64 $length | tr -d '/+=' | head -c $length
}

# Generate a random password with special chars
generate_password() {
    local length=${1:-16}
    local pass=$(openssl rand -base64 $length | tr -d '/+=' | head -c $((length-2)))
    echo "${pass}@1"  # Append @1 for complexity requirements
}
```

### JWT Token Details

**Token Service:** `CRM.Infrastructure/Services/JwtTokenService.cs`

**Access Token Generation:**
```csharp
public string GenerateAccessToken(User user)
{
    var key = Encoding.UTF8.GetBytes(_jwtSecret);
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.GivenName, user.FirstName),
        new Claim(ClaimTypes.Surname, user.LastName),
        new Claim(ClaimTypes.Role, Enum.GetName(typeof(UserRole), user.Role)),
    };
    // Token expires in 60 minutes by default (configurable via Jwt:ExpirationMinutes)
    // Uses HmacSha256 signature algorithm
}
```

**Refresh Token Generation:**
```csharp
public string GenerateRefreshToken()
{
    var randomNumber = new byte[64];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);  // 64 bytes → base64
    }
}
```

**Token Configuration:**
| Setting | Default | Environment Variable |
|---------|---------|---------------------|
| Secret | Required | `Jwt__Secret` or `JWT_KEY` |
| Issuer | CRMApp | `Jwt__Issuer` |
| Audience | CRMUsers | `Jwt__Audience` |
| Expiration | 60 minutes | `Jwt__ExpirationMinutes` |
| Refresh Token Expiry | 7 days | Hardcoded |

---

## 4. Password Storage & Authentication

### Password Hashing

**Library:** BCrypt.Net (`BCrypt.Net.BCrypt`)
**NOT using:** ASP.NET Identity's PasswordHasher

**Hashing:**
```csharp
// In AuthenticationService.cs and DbSeed.cs
private string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password);
}
```

**Verification (supports both BCrypt and legacy SHA-256):**
```csharp
private bool VerifyPassword(string password, string hash)
{
    // Support BCrypt hashes (preferred) - start with $2
    if (hash.StartsWith("$2"))
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
    
    // Legacy support for old SHA-256 hashes
    using (var sha256 = SHA256.Create())
    {
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashOfInput = Convert.ToBase64String(hashedBytes);
        return hashOfInput == hash;
    }
}
```

### Admin User Seeding

The admin user is seeded from environment variables on first startup:

```csharp
// From DbSeed.cs
var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@crm.local";
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";
```

**Admin User Properties:**
- Username: `admin` (configurable)
- Email: `admin@crm.local` (configurable)
- Password: `Admin@123` (default for dev)
- Role: `Admin` (int value)
- EmailVerified: `true`
- PrimaryGroup: `SysAdmin`

### Social/OAuth Login

For OAuth logins (Google, Microsoft, etc.), a random password is generated:
```csharp
PasswordHash = HashPassword(Guid.NewGuid().ToString()),  // Random password
```

---

## 5. Health Check Endpoints

### Health Controller (`CRM.Api/Controllers/HealthController.cs`)

**Endpoints (all unauthenticated, CORS enabled):**

| Endpoint | Purpose | Response |
|----------|---------|----------|
| `GET /health` | Liveness probe | `{"status": "healthy", "timestamp": "..."}` |
| `GET /health/ready` | Readiness probe | `{"status": "ready", "checks": {...}, "timestamp": "..."}` |
| `GET /health/live` | Kubernetes liveness | `{"status": "alive", "timestamp": "..."}` |

**HTTP Status Codes:**
- `200 OK` - Service is healthy
- `503 Service Unavailable` - Service is unhealthy/not ready

### Kubernetes Probe Configuration

```yaml
# Example for Kubernetes deployment
livenessProbe:
  httpGet:
    path: /health/live
    port: 5000
  initialDelaySeconds: 10
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 5000
  initialDelaySeconds: 5
  periodSeconds: 5
```

### Health Check Commands

```bash
# Quick health check
curl http://192.168.0.9:5000/health

# Detailed readiness check
curl http://192.168.0.9:5000/health/ready

# Liveness check
curl http://192.168.0.9:5000/health/live
```

### Health Checks Skip HTTPS Redirect

The API skips HTTPS redirect for health endpoints to allow Kubernetes probes on HTTP:
```csharp
app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"), appBuilder =>
{
    appBuilder.UseHttpsRedirection();
});
```

---

## 6. Key Architecture Decisions

### Customer → Account Migration (January 2026)
**CRITICAL:** The solution underwent a major refactoring where `Customer` was renamed to `Account`:

- **Entity:** `Customer` → `Account` (but table still named `Customers` for compatibility)
- **API Endpoints:** `/api/customers` → `/api/accounts`
- **Frontend Routes:** `/customers` → `/accounts` (but some UI still shows "Customers")
- **TPH Inheritance:** `Customer : Account` hierarchy with `Discriminator` column

**Files affected:**
- `CRM.Backend/src/CRM.Core/Entities/Account.cs` - Main entity
- `CRM.Backend/src/CRM.Api/Controllers/AccountsController.cs` - API controller
- `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs` - Business logic
- `CRM.Frontend/src/pages/AccountsPage.tsx` - Main UI page

### Database Provider Support
The solution supports multiple database providers via `DatabaseProvider` environment variable:
- `mariadb` - Primary, uses Pomelo.EntityFrameworkCore.MySql
- `sqlserver` - Microsoft SQL Server
- `postgresql` - PostgreSQL with Npgsql

**Configuration in `CrmDbContext.cs`:**
```csharp
var databaseProvider = Environment.GetEnvironmentVariable("DatabaseProvider")?.ToLower() ?? "mariadb";
```

### MariaDB Row Size Fix
MariaDB has a 65535 byte row limit. The solution includes a fix in `CrmDbContext.OnModelCreating()` that converts LONGTEXT columns to TEXT/VARCHAR to prevent row size overflow, especially for entities with many string properties like `MarketingCampaign`.

### Entity Tracking Fix
The `Repository.UpdateAsync()` method includes logic to handle already-tracked entities to prevent "another instance with the same key" errors when updating related entities.

---

## 7. Deployment Configuration

### Primary Test Server: 192.168.0.9

**Docker Containers:**
| Container | Port | Purpose |
|-----------|------|---------|
| crm-api | 5000 | .NET Web API |
| crm-frontend | 80 | React app (nginx) |
| crm-mariadb | 3306 | MariaDB database |
| crm-redis | 6379 | Redis cache |

**Docker Network:** `docker_crm-network`

**Database Credentials:**
- Root: `root` / `RootPass@Dev2024`
- App User: `crm_user` / `CrmPass@Dev2024`
- Database: `crm_db`

**API Environment Variables:**
```bash
ASPNETCORE_ENVIRONMENT=Development
DatabaseProvider=mariadb
ConnectionStrings__DefaultConnection=Server=crm-mariadb;Port=3306;Database=crm_db;User=crm_user;Password=CrmPass@Dev2024;
Jwt__Secret=ThisIsAVeryLongSecureJwtSecretKeyForDevelopmentPurposesOnly123456789
```

**Admin User (seeded):**
- Email: `admin@crm.local`
- Password: `Admin@123`
- Role: SystemAdmin

### Microservices Architecture

The solution supports both monolithic and microservices deployment:

**Microservices Containers (when using docker-compose.microservices.unified.yml):**
| Service | Internal Port | Purpose |
|---------|---------------|---------|
| crm-gateway | 5000 | YARP API Gateway, routes to services |
| crm-identity | 5001 | Authentication, users, JWT |
| crm-customer | 5002 | Accounts, contacts |
| crm-sales | 5003 | Opportunities, quotes |
| crm-marketing | 5004 | Campaigns, templates |
| crm-servicedesk | 5005 | Service requests, workflows |
| crm-core | 5006 | Products, settings, lookups |

### Environment Variables Reference (.env.example)

**Critical Variables:**
```bash
# Runtime
ASPNETCORE_ENVIRONMENT=Production|Development

# JWT (REQUIRED - minimum 32 characters)
JWT_KEY=your-secure-key-at-least-32-characters
JWT_SECRET=${JWT_KEY}
JWT_ISSUER=CRM.Api
JWT_AUDIENCE=CRM.Client

# Database
DB_HOST=crm-db
DB_PORT=3306
DB_NAME=crm_db
DB_USER=crm_user
DB_PASSWORD=your-db-password
DB_ROOT_PASSWORD=your-root-password

# Redis
REDIS_HOST=crm-redis
REDIS_PORT=6379

# Admin seeding (first run only)
ADMIN_USERNAME=admin
ADMIN_EMAIL=admin@crm.local
ADMIN_PASSWORD=your-admin-password
```

---

## 8. Database Startup & Seeding Process

### Automatic Startup Sequence

When the API starts (in `Program.cs`), the following happens automatically:

1. **Connection Check:** `db.Database.CanConnectAsync()`
2. **Schema Creation:** `db.Database.EnsureCreatedAsync()` (for MariaDB/MySQL)
3. **Raw SQL Migrations:** Applies files from `CRM.Backend/migrations/*.sql`
4. **Admin User Seeding:** `DbSeed.SeedAsync(db)`
5. **Master Data Seeding:** ZIP codes, color palettes
6. **Sample Data (optional):** If `SampleData:AutoSeed=true`

### DbSeed.SeedAsync() Process

The seed process (in `CRM.Infrastructure/Data/DbSeed.cs`) creates:

1. **SysAdmin User Group** with full permissions
2. **Admin User** with credentials from environment variables
3. **Department Structure** (Executive, Sales, Marketing, etc.)
4. **Sample Customers** (if none exist)

### Seed Order Dependency

```
1. UserGroups (SysAdmin first)
   └── 2. Users (admin needs SysAdmin group)
       └── 3. UserGroupMembers (link admin to SysAdmin)
           └── 4. Departments
               └── 5. Sample Data
```

### Re-seeding Admin User

If admin user is missing or needs reset:

```bash
# Direct database access
docker exec -it crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db

# Check admin user
SELECT Id, Username, Email, IsActive FROM Users WHERE Username = 'admin';

# Reset password (generate new BCrypt hash first)
# Or simply restart the container - it will re-seed if user doesn't exist
docker restart crm-api
```

---

## 9. Key Files Reference

### Backend Core Files

| File | Purpose |
|------|---------|
| `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` | EF Core DbContext with all entity configurations |
| `CRM.Backend/src/CRM.Infrastructure/Data/DbSeed.cs` | Database seeding (admin user, sample data) |
| `CRM.Backend/src/CRM.Core/Entities/Account.cs` | Main Account (formerly Customer) entity |
| `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs` | Account business logic |
| `CRM.Backend/src/CRM.Infrastructure/Services/AuthenticationService.cs` | JWT auth, login, registration |
| `CRM.Backend/src/CRM.Infrastructure/Repositories/Repository.cs` | Generic repository pattern |

### Frontend Core Files

| File | Purpose |
|------|---------|
| `CRM.Frontend/src/App.tsx` | Main app with routing |
| `CRM.Frontend/src/pages/AccountsPage.tsx` | Accounts list page |
| `CRM.Frontend/src/pages/AccountDetailsPage.tsx` | Account detail view |
| `CRM.Frontend/src/services/api.ts` | Axios API configuration |
| `CRM.Frontend/src/store/` | Redux store slices |

### Test Files

| File | Purpose |
|------|---------|
| `e2e-tests/tests/auth.setup.ts` | Playwright auth setup |
| `e2e-tests/tests/test-data.ts` | Test user credentials and data |
| `e2e-tests/tests/bvt/api-bvt.spec.ts` | Build Verification Tests |
| `e2e-tests/playwright.config.ts` | Playwright configuration |

---

## 10. API Endpoints Reference

### Authentication
```
POST /api/auth/login          # Login with email/password
POST /api/auth/register       # Register new user
POST /api/auth/refresh        # Refresh JWT token
GET  /api/auth/me             # Get current user
POST /api/auth/logout         # Logout
```

### Accounts (formerly Customers)
```
GET    /api/accounts          # List all accounts
GET    /api/accounts/{id}     # Get account by ID
POST   /api/accounts          # Create account
PUT    /api/accounts/{id}     # Update account
DELETE /api/accounts/{id}     # Delete account
```

### Other Core Endpoints
```
/api/contacts                 # Contact CRUD
/api/leads                    # Lead CRUD
/api/opportunities            # Opportunity CRUD
/api/products                 # Product CRUD
/api/quotes                   # Quote CRUD
/api/campaigns                # Campaign CRUD
/api/service-requests         # Service request CRUD
/api/users                    # User management
/api/user-groups              # User group management
/api/dashboard                # Dashboard data
/api/notes                    # Notes CRUD
/api/settings                 # System settings
/api/lookups                  # Lookup data (industries, etc.)
```

---

## 11. Testing

### Test Types

1. **Unit Tests** (`CRM.Backend/tests/CRM.UnitTests/`)
   - xUnit-based
   - Run with: `dotnet test`

2. **BVT Tests** (`e2e-tests/tests/bvt/api-bvt.spec.ts`)
   - Build Verification Tests
   - API-only, no browser
   - Fast, covers critical paths

3. **E2E Tests** (`e2e-tests/tests/`)
   - Playwright-based
   - Full browser automation
   - Multiple browsers: chromium, firefox, webkit

### Running Tests

```bash
# BVT tests only
cd e2e-tests
BASE_URL=http://192.168.0.9 npx playwright test tests/bvt/api-bvt.spec.ts

# All E2E tests (chromium only)
BASE_URL=http://192.168.0.9 npx playwright test --project=chromium

# Specific test file
BASE_URL=http://192.168.0.9 npx playwright test tests/auth/authentication.spec.ts
```

### Test Credentials
```typescript
// From e2e-tests/tests/test-data.ts
existingAdmin: {
  email: 'admin@crm.local',
  password: 'Admin@123',
}
```

---

## 12. Common Issues & Solutions

### Issue: MariaDB "Row size too large"
**Cause:** Entities with many string properties exceed 65535 byte limit  
**Solution:** Fix in `CrmDbContext.OnModelCreating()` converts strings to TEXT/VARCHAR at end of method

### Issue: Entity tracking conflicts
**Cause:** Same entity loaded multiple times in different tracking states  
**Solution:** `Repository.UpdateAsync()` checks if entity is already tracked before attaching

### Issue: Platform mismatch (arm64 vs amd64)
**Cause:** Building on Mac (arm64) for Linux server (amd64)  
**Solution:** Always use `--platform linux/amd64` flag when building Docker images

### Issue: Admin user not found after deployment
**Cause:** Database recreated without seeding  
**Solution:** Ensure `DbSeed.SeedAsync()` runs on startup or manually create admin user

### Issue: Tests fail with "networkidle" timeout
**Cause:** `waitForLoadState('networkidle')` times out on pages with polling  
**Solution:** Use `waitForLoadState('domcontentloaded')` instead

### Issue: Login tests fail - multiple buttons match
**Cause:** Selector matches both "Quick Admin Login" and "Sign In" buttons  
**Solution:** Use more specific selector: `button[type="submit"]:has-text("Sign In")`

---

## 13. Development Workflow

### Local Development

```bash
# Backend
cd CRM.Backend
dotnet restore
dotnet run --project src/CRM.Api

# Frontend
cd CRM.Frontend
npm install
npm start

# E2E Tests
cd e2e-tests
npm install
npx playwright install
```

### Database Migrations

The solution uses `EnsureCreated()` for schema management in development. For production migrations:

```bash
cd CRM.Backend
dotnet ef migrations add <MigrationName> --project src/CRM.Infrastructure --startup-project src/CRM.Api
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api
```

---

## 14. Contact Info Normalization

The solution has a normalized contact information architecture:

### Entities
- `Address` - Physical addresses
- `ContactDetail` - Emails, phones (with `DetailType` enum)
- `SocialAccount` - Social media accounts
- `ContactInfoLink` - Junction table linking info to owners (Account, Contact, Lead)

### Owner Types
```csharp
public enum ContactInfoOwnerType
{
    Account,
    Contact,
    Lead
}
```

---

## 15. Security & CORS

### JWT Authentication
- Tokens issued by `AuthenticationService` via `JwtTokenService`
- Secret configured via `Jwt__Secret` environment variable (minimum 32 characters)
- Token includes: userId, email, username, firstName, lastName, role
- Algorithm: HMAC-SHA256
- Default expiration: 60 minutes (configurable)
- Refresh token: 64 bytes, base64 encoded, 7-day expiry

### Password Hashing
- Uses BCrypt (`BCrypt.Net.BCrypt.HashPassword()`)
- NOT ASP.NET Identity's `PasswordHasher`
- Legacy SHA-256 hashes supported for migration

### Authorization
- Role-based via `UserGroup.IsSystemAdmin` and permission flags
- Controllers decorated with `[Authorize]` attribute
- UserProfile contains granular permissions (CanCreateCustomers, CanEditCustomers, etc.)

### CORS Policy

The API uses dynamic CORS origin validation:

```csharp
// From Program.cs
policy.SetIsOriginAllowed(origin => {
    // Allow configured origins from AllowedOrigins setting
    // Allow localhost and 127.0.0.1 (development)
    // Allow local network IPs (192.168.x.x, 10.x.x.x, 172.16-31.x.x)
});
```

**Allowed Origins:**
- Any configured origin in `AllowedOrigins` setting
- `localhost` and `127.0.0.1` (any port)
- Private network IPs: `192.168.x.x`, `10.x.x.x`, `172.16-31.x.x`

### Rate Limiting

Configured via `RateLimiting` section in appsettings.json:

```json
{
  "RateLimiting": {
    "EnableEndpointRateLimiting": true,
    "GeneralRules": [
      { "Endpoint": "*", "Period": "1m", "Limit": 1000 }
    ]
  }
}
```

- Default: 1000 requests per minute per IP
- Returns HTTP 429 when exceeded
- Disabled in Development mode by default

### HTTPS Configuration

- SSL certificate path: `ssl/server.pfx`
- Certificate password: `CrmSslCert2024` (configurable)
- HTTPS port: 5001 (configurable)
- HTTP port: 5000 (always available)
- Health endpoints skip HTTPS redirect for Kubernetes probes

---

## 16. Quick Reference Commands

```bash
# SSH to server
ssh root@192.168.0.9

# Check containers
docker ps

# View API logs
docker logs crm-api --tail 100

# Follow logs in real-time
docker logs crm-api -f

# Database shell
docker exec -it crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db

# Table count (should be ~171)
docker exec crm-mariadb mariadb -u root -pRootPass@Dev2024 crm_db -e "SHOW TABLES" | wc -l

# Test API health
curl http://192.168.0.9:5000/health

# Test readiness
curl http://192.168.0.9:5000/health/ready

# Test login
curl -X POST http://192.168.0.9:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@crm.local","password":"Admin@123"}'

# Restart services
docker restart crm-api
docker restart crm-frontend

# View container environment
docker exec crm-api env | grep -E "JWT|DB|ASPNET"

# Check database connection from API container
docker exec crm-api sh -c 'echo "SELECT 1" | mysql -h crm-mariadb -u crm_user -pCrmPass@Dev2024 crm_db'
```

### Build & Deploy Commands

```bash
# From Mac - build and deploy backend
cd "/Users/alal/Code/Git CRM Solution/crm-solution"
./build.sh backend

# Or manual cross-platform build
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend --load .
docker save crm-api:latest | ssh root@192.168.0.9 "docker load"
ssh root@192.168.0.9 "docker stop crm-api; docker rm crm-api; docker run -d --name crm-api ..."
```

### Testing Commands

```bash
# Run all E2E tests
cd e2e-tests
BASE_URL=http://192.168.0.9 npx playwright test --project=chromium

# Run specific test file
BASE_URL=http://192.168.0.9 npx playwright test tests/auth/authentication.spec.ts

# Run with headed browser (for debugging)
BASE_URL=http://192.168.0.9 npx playwright test --headed

# Generate test report
npx playwright show-report
```

---

## 17. Debugging & Troubleshooting

### API Startup Issues

Check logs for startup errors:
```bash
docker logs crm-api 2>&1 | head -100
```

Common startup log lines:
```
HTTPS enabled on port 5001 with certificate: ...
Configuring Redis cache at localhost:6379
Creating schema for mariadb database using EnsureCreated...
Database setup completed successfully
Master data status: 42522 ZIP codes, 3 color palettes
```

### JWT Token Issues

If JWT authentication fails:
```bash
# Check JWT secret is configured
docker exec crm-api env | grep JWT

# Verify secret matches between services
# Must be identical for all services sharing auth
```

### Database Connection Issues

```bash
# Test database connectivity from API container
docker exec crm-api sh -c 'mysql -h crm-mariadb -u crm_user -pCrmPass@Dev2024 crm_db -e "SELECT 1"'

# Check MariaDB logs
docker logs crm-mariadb --tail 50

# Verify network connectivity
docker exec crm-api ping -c 2 crm-mariadb
```

### Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "JWT Secret must be at least 32 characters" | JWT_SECRET too short | Set longer secret in .env |
| "User with this email already exists" | Duplicate registration | Use different email or login |
| "Row size too large" | MariaDB column limit | Already fixed in CrmDbContext |
| "Cannot track entity - already tracked" | EF Core tracking conflict | Already fixed in Repository.cs |

---

## 18. Recent Changes Log

### February 1, 2026
- Added comprehensive SOLUTION_CONTEXT.md for session continuity
- Fixed E2E test selectors for MUI components
- Fixed workflow test invalid CSS selectors
- Made campaign creation tests resilient with skip conditions

### January 31, 2026
- Customer → Account migration completed
- All tests updated for `/api/accounts` endpoint
- Fixed MariaDB row size issue in CrmDbContext
- Fixed entity tracking conflicts in Repository
- Deployed to 192.168.0.9 with 171 tables

### Test Status (Last Run)
- **BVT Tests:** 206 passed ✅
- **E2E Tests (subset):** 73 passed, 1 failed, 7 skipped (90% pass rate)
- **Known Issues:** Some admin dialog overlay tests need `{ force: true }`

---

## 19. Files Modified in Recent Sessions

1. `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs` - String column type fix
2. `CRM.Backend/src/CRM.Infrastructure/Repositories/Repository.cs` - Entity tracking fix
3. `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs` - Immediate save fix
4. `e2e-tests/tests/test-data.ts` - Updated admin credentials
5. `e2e-tests/tests/auth.setup.ts` - Login selector fixes
6. `e2e-tests/tests/auth/authentication.spec.ts` - Test resilience improvements
7. Multiple test files - Changed `networkidle` to `domcontentloaded`

---

*This document should be referenced at the start of any development session to quickly understand the solution context.*
