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
- **Backend:** ASP.NET Core 10.0 Web API with Entity Framework Core
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
│       └── contexts/      # React Context state management (Auth, Theme, SignalR, etc.)
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

### Database Documentation

Complete database documentation is available at:
- [database/DATABASE_SCHEMA.md](../../database/DATABASE_SCHEMA.md) - Full schema reference (~171 tables)
- [database/setup-database.sh](database/setup-database.sh) - Cross-platform setup script

### Manual Database Setup

For manual database setup (instead of relying on EF Core):

```bash
cd database

# Interactive setup
./setup-database.sh

# Specify provider
./setup-database.sh --provider mariadb --host localhost

# Using Docker container
./setup-database.sh --docker --container crm-mariadb

# Include sample data
./setup-database.sh --seed --sample-data
```

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

### Database Table Categories

| Category | Table Count | Description |
|----------|-------------|-------------|
| Core (Users/Auth) | 8 | Users, Groups, Departments |
| CRM Entities | 9 | Accounts, Contacts, Leads, Opportunities |
| Contact Info | 12 | Addresses, Phones, Emails, Social |
| Marketing | 16 | Campaigns, Email sequences, Web tracking |
| Sales/Quotes | 14 | Quotes, Orders, Invoices, Payments |
| CPQ | 13 | Bundles, Pricing, Approvals |
| Service Desk | 15 | Tickets, KB, SLA |
| Workflows | 8 | Workflow engine |
| AI/Analytics | 13 | Predictions, Scoring |
| System/Config | 14 | Settings, Lookups, Custom Fields |

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
| `CRM.Frontend/src/contexts/` | React Context providers (Auth, Theme, SignalR, Branding, Layout, etc.) |

### Test Files

| File | Purpose |
|------|---------|
| `e2e-tests/tests/auth.setup.ts` | Playwright auth setup |
| `e2e-tests/tests/test-data.ts` | Test user credentials and data |
| `e2e-tests/tests/bvt/api-bvt.spec.ts` | Build Verification Tests |
| `e2e-tests/playwright.config.ts` | Playwright configuration |

---

## 10. API Endpoints Reference

> **Total: 95 controllers, 1,377 endpoints**
>
> Auto-generated from controller source code. All endpoints require JWT authentication unless marked otherwise.
> Health endpoints (`/healths/*`) are unauthenticated.

### 10.1 Summary by Domain

| Domain | Controllers | Endpoints | Description |
|--------|-------------|-----------|-------------|
| Authentication & Users | 5 | 48 | Auth, users, groups, profiles, departments |
| CRM Core | 16 | 242 | Accounts, contacts, leads, opportunities, activities, territories |
| Sales & CPQ | 14 | 281 | Orders, invoices, payments, quotes, contracts, subscriptions, commissions |
| Marketing | 6 | 91 | Campaigns, email sequences, forms, landing pages, event attendees |
| Service Desk | 2 | 55 | Service requests, categories, custom fields |
| ITSM | 13 | 154 | Incidents, problems, changes, CMDB, knowledge base, SLA, service catalog |
| AI & Analytics | 5 | 68 | AI chatbot, email AI, dashboards, reports |
| Workflows & Automation | 5 | 85 | Workflow engine, triggers, instances, webhooks |
| Communications | 4 | 46 | Multi-channel messaging, conversations, calendar, news/social |
| Administration | 20 | 241 | Settings, templates, master data, teams, tasks, import/export |
| Infrastructure | 5 | 66 | Cloud deployments, database management, monitoring, health checks |

### 10.2 Authentication & Users (48 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/auths` | AuthController | 15 | GET:2, POST:13 |
| `/api/departments` | DepartmentsController | 5 | DELETE:1, GET:2, POST:1, PUT:1 |
| `/api/usergroups` | UserGroupsController | 8 | DELETE:2, GET:3, POST:2, PUT:1 |
| `/api/userprofiles` | UserProfilesController | 7 | DELETE:1, GET:4, POST:1, PUT:1 |
| `/api/users` | UsersController | 13 | DELETE:1, GET:5, POST:5, PUT:2 |

<details>
<summary>All Authentication & Users endpoints</summary>

```
POST    /api/auths/2fa/disable
POST    /api/auths/2fa/enable
POST    /api/auths/2fa/setup
POST    /api/auths/2fa/verify
POST    /api/auths/login
POST    /api/auths/login/2fa
GET     /api/auths/me
POST    /api/auths/oauth-login
GET     /api/auths/password-requirements
POST    /api/auths/password-reset/confirm
POST    /api/auths/password-reset/request
POST    /api/auths/register
POST    /api/auths/reset-password/{userId}
POST    /api/auths/setup-password
POST    /api/auths/verify
GET     /api/departments
POST    /api/departments
DELETE  /api/departments/{id}
GET     /api/departments/{id}
PUT     /api/departments/{id}
GET     /api/usergroups
POST    /api/usergroups
DELETE  /api/usergroups/{id}
GET     /api/usergroups/{id}
PUT     /api/usergroups/{id}
GET     /api/usergroups/{id}/members
DELETE  /api/usergroups/{id}/members/{userId}
POST    /api/usergroups/{id}/members/{userId}
GET     /api/userprofiles
POST    /api/userprofiles
GET     /api/userprofiles/department/{departmentId}
GET     /api/userprofiles/me
DELETE  /api/userprofiles/{id}
GET     /api/userprofiles/{id}
PUT     /api/userprofiles/{id}
GET     /api/users
POST    /api/users
GET     /api/users/by-contact/{contactId}
GET     /api/users/department/{departmentId}
GET     /api/users/me/preferences
PUT     /api/users/me/preferences
DELETE  /api/users/{id}
GET     /api/users/{id}
PUT     /api/users/{id}
POST    /api/users/{id}/assign-profile
POST    /api/users/{id}/link-contact
POST    /api/users/{id}/remove-profile
POST    /api/users/{id}/unlink-contact
```

</details>

### 10.3 CRM Core (242 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/accounts` | AccountsController | 19 | DELETE:3, GET:10, POST:4, PUT:2 |
| `/api/activities` | ActivitiesController | 16 | DELETE:2, GET:10, PATCH:2, POST:2 |
| `/api/admin/leadscorerules` | LeadScoreRulesController | 11 | DELETE:1, GET:6, PATCH:1, POST:2, PUT:1 |
| `/api/ai/leads` | AILeadScoringController | 6 | GET:3, POST:3 |
| `/api/contactinfos` | ContactInfoController | 47 | DELETE:9, GET:15, POST:17, PUT:6 |
| `/api/contacts` | ContactsController | 8 | DELETE:2, GET:3, POST:2, PUT:1 |
| `/api/duplicates` | DuplicatesController | 10 | GET:5, POST:5 |
| `/api/interactions` | InteractionsController | 16 | DELETE:1, GET:6, POST:8, PUT:1 |
| `/api/leadroutings` | LeadRoutingController | 29 | DELETE:3, GET:11, POST:12, PUT:3 |
| `/api/leads` | LeadsController | 8 | DELETE:1, GET:4, POST:2, PUT:1 |
| `/api/notes` | NotesController | 10 | DELETE:1, GET:4, POST:4, PUT:1 |
| `/api/opportunities` | OpportunitiesController | 7 | DELETE:1, GET:4, POST:1, PUT:1 |
| `/api/pipelines` | PipelinesController | 3 | GET:3 |
| `/api/relationships` | RelationshipsController | 16 | DELETE:2, GET:8, POST:4, PUT:2 |
| `/api/stages` | StagesController | 3 | GET:3 |
| `/api/territories` | TerritoriesController | 33 | DELETE:3, GET:16, POST:9, PUT:5 |

<details>
<summary>All CRM Core endpoints</summary>

```
GET     /api/accounts
POST    /api/accounts
DELETE  /api/accounts/batch
POST    /api/accounts/batch
GET     /api/accounts/filters
GET     /api/accounts/search
DELETE  /api/accounts/{id}
GET     /api/accounts/{id}
PUT     /api/accounts/{id}
GET     /api/accounts/{id}/contacts
POST    /api/accounts/{id}/contacts
DELETE  /api/accounts/{id}/contacts/{contactId}
GET     /api/accounts/{id}/details
GET     /api/accounts/{id}/interactions
GET     /api/accounts/{id}/linked-contacts
GET     /api/accounts/{id}/opportunities
GET     /api/accounts/{id}/related
POST    /api/accounts/{id}/tags
GET     /api/activities
POST    /api/activities
GET     /api/activities/by-entity/{entityType}/{entityId}
GET     /api/activities/by-type/{type}
GET     /api/activities/recent
GET     /api/activities/stats
GET     /api/activities/timeline/{entityType}/{entityId}
GET     /api/activities/upcoming
DELETE  /api/activities/{id}
GET     /api/activities/{id}
PATCH   /api/activities/{id}/cancel
PATCH   /api/activities/{id}/complete
GET     /api/activities/{id}/notes
DELETE  /api/activities/{noteId}/notes
GET     /api/admin/leadscorerules
POST    /api/admin/leadscorerules
GET     /api/admin/leadscorerules/evaluate/{leadId}
GET     /api/admin/leadscorerules/preview
GET     /api/admin/leadscorerules/scoring-stats
GET     /api/admin/leadscorerules/summary
DELETE  /api/admin/leadscorerules/{id}
GET     /api/admin/leadscorerules/{id}
PUT     /api/admin/leadscorerules/{id}
PATCH   /api/admin/leadscorerules/{id}/toggle
POST    /api/admin/leadscorerules/{id}/validate
GET     /api/ai/leads/scoring-weights
POST    /api/ai/leads/score-all
POST    /api/ai/leads/{id}/score
GET     /api/ai/opportunities/win-rates
POST    /api/ai/opportunities/score-all
POST    /api/ai/opportunities/{id}/score
GET     /api/contactinfos/addresses
POST    /api/contactinfos/addresses
GET     /api/contactinfos/addresses/{id}
PUT     /api/contactinfos/addresses/{id}
DELETE  /api/contactinfos/addresses/{id}
POST    /api/contactinfos/addresses/{id}/primary
GET     /api/contactinfos/emails
POST    /api/contactinfos/emails
GET     /api/contactinfos/emails/{id}
PUT     /api/contactinfos/emails/{id}
DELETE  /api/contactinfos/emails/{id}
POST    /api/contactinfos/emails/{id}/primary
GET     /api/contactinfos/entity/{entityType}/{entityId}
GET     /api/contactinfos/entity/{entityType}/{entityId}/addresses
GET     /api/contactinfos/entity/{entityType}/{entityId}/emails
GET     /api/contactinfos/entity/{entityType}/{entityId}/phones
GET     /api/contactinfos/entity/{entityType}/{entityId}/socials
POST    /api/contactinfos/link
DELETE  /api/contactinfos/link
POST    /api/contactinfos/link/batch
GET     /api/contactinfos/phones
POST    /api/contactinfos/phones
GET     /api/contactinfos/phones/{id}
PUT     /api/contactinfos/phones/{id}
DELETE  /api/contactinfos/phones/{id}
POST    /api/contactinfos/phones/{id}/primary
GET     /api/contactinfos/socials
POST    /api/contactinfos/socials
GET     /api/contactinfos/socials/{id}
PUT     /api/contactinfos/socials/{id}
DELETE  /api/contactinfos/socials/{id}
POST    /api/contactinfos/socials/{id}/primary
GET     /api/contacts
POST    /api/contacts
POST    /api/contacts/batch
DELETE  /api/contacts/{id}
GET     /api/contacts/{id}
PUT     /api/contacts/{id}
GET     /api/contacts/{id}/accounts
DELETE  /api/contacts/{id}/accounts/{accountId}
GET     /api/duplicates/candidates/{ruleId}
GET     /api/duplicates/check
GET     /api/duplicates/merge-history
POST    /api/duplicates/merge/{survivorId}/{mergeId}
GET     /api/duplicates/rules
POST    /api/duplicates/rules
POST    /api/duplicates/scan
POST    /api/duplicates/scan/{ruleId}
POST    /api/duplicates/unmerge/{mergeId}
DELETE  /api/duplicates/{id}
GET     /api/interactions
POST    /api/interactions
GET     /api/interactions/by-entity/{entityType}/{entityId}
GET     /api/interactions/recent
GET     /api/interactions/statistics
GET     /api/interactions/timeline/{entityType}/{entityId}
POST    /api/interactions/batch
POST    /api/interactions/email
POST    /api/interactions/log-call
POST    /api/interactions/log-email
POST    /api/interactions/log-meeting
POST    /api/interactions/log-note
DELETE  /api/interactions/{id}
GET     /api/interactions/{id}
PUT     /api/interactions/{id}
GET     /api/leadroutings
POST    /api/leadroutings
GET     /api/leadroutings/active
POST    /api/leadroutings/auto-assign/{leadId}
DELETE  /api/leadroutings/criteria/{criteriaId}
PUT     /api/leadroutings/criteria/{criteriaId}
GET     /api/leadroutings/logs
GET     /api/leadroutings/performance
POST    /api/leadroutings/preview
POST    /api/leadroutings/reorder
POST    /api/leadroutings/simulate
GET     /api/leadroutings/statistics
DELETE  /api/leadroutings/targets/{targetId}
PUT     /api/leadroutings/targets/{targetId}
DELETE  /api/leadroutings/{id}
GET     /api/leadroutings/{id}
PUT     /api/leadroutings/{id}
GET     /api/leadroutings/{id}/criteria
POST    /api/leadroutings/{id}/criteria
GET     /api/leadroutings/{id}/logs
GET     /api/leadroutings/{id}/targets
POST    /api/leadroutings/{id}/targets
POST    /api/leadroutings/{id}/test
POST    /api/leadroutings/{id}/toggle
GET     /api/leads
POST    /api/leads
POST    /api/leads/batch
GET     /api/leads/search
DELETE  /api/leads/{id}
GET     /api/leads/{id}
PUT     /api/leads/{id}
POST    /api/leads/{id}/convert
GET     /api/notes
POST    /api/notes
GET     /api/notes/entity/{entityType}/{entityId}
GET     /api/notes/pinned/{entityType}/{entityId}
GET     /api/notes/recent
DELETE  /api/notes/{id}
GET     /api/notes/{id}
PUT     /api/notes/{id}
POST    /api/notes/{id}/pin
POST    /api/notes/{id}/unpin
GET     /api/opportunities
POST    /api/opportunities
GET     /api/opportunities/pipeline
GET     /api/opportunities/search
DELETE  /api/opportunities/{id}
GET     /api/opportunities/{id}
PUT     /api/opportunities/{id}
GET     /api/pipelines
GET     /api/pipelines/{id}
GET     /api/pipelines/{id}/stages
GET     /api/relationships ...
GET     /api/stages ...
GET     /api/territories ...
```

</details>

### 10.4 Sales & CPQ (281 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/approvals` | ApprovalsController | 43 | DELETE:4, GET:21, POST:14, PUT:4 |
| `/api/commissions` | CommissionsController | 35 | DELETE:3, GET:17, PATCH:1, POST:11, PUT:3 |
| `/api/contracts` | ContractsController | 28 | DELETE:1, GET:14, POST:12, PUT:1 |
| `/api/creditmemos` | CreditMemosController | 13 | DELETE:2, GET:4, POST:5, PUT:2 |
| `/api/invoices` | InvoicesController | 27 | DELETE:2, GET:10, PATCH:1, POST:12, PUT:2 |
| `/api/orders` | OrdersController | 35 | DELETE:2, GET:11, PATCH:1, POST:19, PUT:2 |
| `/api/payments` | PaymentsController | 23 | DELETE:1, GET:9, PATCH:1, POST:11, PUT:1 |
| `/api/pricebooks` | PriceBooksController | 5 | DELETE:1, GET:2, POST:1, PUT:1 |
| `/api/productbundles` | ProductBundlesController | 6 | DELETE:1, GET:3, POST:1, PUT:1 |
| `/api/products` | ProductsController | 8 | DELETE:1, GET:5, POST:1, PUT:1 |
| `/api/quotes` | QuotesController | 17 | DELETE:2, GET:5, POST:8, PUT:2 |
| `/api/sales-forecasts` | SalesForecastsController | 9 | DELETE:1, GET:4, POST:3, PUT:1 |
| `/api/sales-quotas` | SalesQuotasController | 8 | DELETE:1, GET:4, PATCH:1, POST:1, PUT:1 |
| `/api/subscriptions` | SubscriptionsController | 24 | DELETE:2, GET:8, POST:13, PUT:1 |

<details>
<summary>All Sales & CPQ endpoints</summary>

```
GET     /api/approvals
POST    /api/approvals
GET     /api/approvals/groups
POST    /api/approvals/groups
GET     /api/approvals/groups/{groupId}/members
POST    /api/approvals/groups/{groupId}/members/{userId}
DELETE  /api/approvals/groups/{groupId}/members/{userId}
DELETE  /api/approvals/groups/{id}
GET     /api/approvals/groups/{id}
PUT     /api/approvals/groups/{id}
GET     /api/approvals/levels
POST    /api/approvals/levels
DELETE  /api/approvals/levels/{id}
GET     /api/approvals/levels/{id}
PUT     /api/approvals/levels/{id}
GET     /api/approvals/matrices
POST    /api/approvals/matrices
DELETE  /api/approvals/matrices/{id}
GET     /api/approvals/matrices/{id}
PUT     /api/approvals/matrices/{id}
GET     /api/approvals/my-approvals
GET     /api/approvals/my-requests
GET     /api/approvals/pending
GET     /api/approvals/statistics
DELETE  /api/approvals/{id}
GET     /api/approvals/{id}
POST    /api/approvals/{id}/approve
GET     /api/approvals/{id}/history
POST    /api/approvals/{id}/reassign
POST    /api/approvals/{id}/reject
POST    /api/approvals/{id}/request
POST    /api/approvals/{id}/return
POST    /api/approvals/{id}/submit
GET     /api/approvals/{requestId}/steps
GET     /api/commissions ...
GET     /api/contracts ...
GET     /api/creditmemos ...
GET     /api/invoices ...
GET     /api/orders ...
GET     /api/payments ...
GET     /api/pricebooks ...
GET     /api/productbundles ...
GET     /api/products ...
GET     /api/quotes ...
GET     /api/sales-forecasts ...
GET     /api/sales-quotas ...
GET     /api/subscriptions ...
```

</details>

### 10.5 Marketing (91 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/campaigns` | CampaignsController + CampaignExecutionController | 19 | DELETE:2, GET:7, POST:8, PUT:2 |
| `/api/email-sequences` | EmailSequencesController | 9 | DELETE:1, GET:3, POST:4, PUT:1 |
| `/api/event-attendees` | EventAttendeesController | 8 | DELETE:1, GET:3, PATCH:1, POST:2, PUT:1 |
| `/api/forms` | FormsController | 39 | DELETE:3, GET:15, POST:16, PUT:5 |
| `/api/landing-pages` | LandingPageController | 16 | DELETE:1, GET:7, POST:6, PUT:2 |

<details>
<summary>All Marketing endpoints</summary>

```
GET     /api/campaigns
POST    /api/campaigns
GET     /api/campaigns/active
GET     /api/campaigns/recipients/{recipientId}/click
POST    /api/campaigns/recipients/{recipientId}/conversion
POST    /api/campaigns/recipients/{recipientId}/open
POST    /api/campaigns/{campaignId}/abtests
POST    /api/campaigns/{campaignId}/abtests/{testId}/start
GET     /api/campaigns/{campaignId}/analytics
GET     /api/campaigns/{campaignId}/recipients
POST    /api/campaigns/{campaignId}/start
GET     /api/campaigns/{campaignId}/workflows
POST    /api/campaigns/{campaignId}/workflows
DELETE  /api/campaigns/{campaignId}/workflows/{workflowId}
PUT     /api/campaigns/{campaignId}/workflows/{workflowId}
DELETE  /api/campaigns/{id}
GET     /api/campaigns/{id}
PUT     /api/campaigns/{id}
POST    /api/campaigns/{id}/metrics
GET     /api/email-sequences
POST    /api/email-sequences
DELETE  /api/email-sequences/{id}
GET     /api/email-sequences/{id}
PUT     /api/email-sequences/{id}
POST    /api/email-sequences/{id}/enroll
POST    /api/email-sequences/{id}/start
GET     /api/email-sequences/{id}/status
POST    /api/email-sequences/{id}/stop
GET     /api/event-attendees
POST    /api/event-attendees
GET     /api/event-attendees/by-activity/{activityId}
DELETE  /api/event-attendees/{id}
GET     /api/event-attendees/{id}
PUT     /api/event-attendees/{id}
POST    /api/event-attendees/{id}/record-attendance
PATCH   /api/event-attendees/{id}/response
GET     /api/forms
POST    /api/forms
GET     /api/forms/by-key/{formKey}
GET     /api/forms/confirm-optin/{token}
GET     /api/forms/statistics/submissions
GET     /api/forms/submissions/by-number/{submissionNumber}
GET     /api/forms/templates
POST    /api/forms/from-template
DELETE  /api/forms/fields/{fieldId}
GET     /api/forms/fields/{fieldId}
PUT     /api/forms/fields/{fieldId}
POST    /api/forms/fields/{fieldId}/validate
DELETE  /api/forms/submissions/{submissionId}
GET     /api/forms/submissions/{submissionId}
POST    /api/forms/submissions/{submissionId}/mark-not-spam
POST    /api/forms/submissions/{submissionId}/mark-spam
POST    /api/forms/submissions/{submissionId}/reprocess
POST    /api/forms/submissions/{submissionId}/send-optin
DELETE  /api/forms/{id}
GET     /api/forms/{id}
PUT     /api/forms/{id}
POST    /api/forms/{id}/archive
POST    /api/forms/{id}/clone
POST    /api/forms/{id}/publish
PUT     /api/forms/{id}/status
POST    /api/forms/{id}/unpublish
GET     /api/forms/{formId}/direct-url
GET     /api/forms/{formId}/embed-code
GET     /api/forms/{formId}/field-statistics
GET     /api/forms/{formId}/fields
POST    /api/forms/{formId}/fields
PUT     /api/forms/{formId}/fields/bulk
PUT     /api/forms/{formId}/fields/reorder
POST    /api/forms/{formId}/spam-score
GET     /api/forms/{formId}/statistics
GET     /api/forms/{formId}/submissions
POST    /api/forms/{formId}/submit
POST    /api/forms/{formId}/validate
POST    /api/forms/{formId}/view
GET     /api/landing-pages
POST    /api/landing-pages
GET     /api/landing-pages/check-slug
GET     /api/landing-pages/p/{slug}
DELETE  /api/landing-pages/{id}
GET     /api/landing-pages/{id}
PUT     /api/landing-pages/{id}
GET     /api/landing-pages/{id}/analytics
GET     /api/landing-pages/{id}/blocks
PUT     /api/landing-pages/{id}/blocks
POST    /api/landing-pages/{id}/duplicate
GET     /api/landing-pages/{id}/preview
POST    /api/landing-pages/{id}/publish
POST    /api/landing-pages/{id}/time
POST    /api/landing-pages/{id}/unpublish
POST    /api/landing-pages/{id}/variant
```

</details>

### 10.6 Service Desk (55 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/service-request-settings` | ServiceRequestSettingsController | 30 | DELETE:4, GET:14, POST:8, PUT:4 |
| `/api/servicerequests` | ServiceRequestsController | 25 | DELETE:1, GET:11, PATCH:1, POST:10, PUT:2 |

<details>
<summary>All Service Desk endpoints</summary>

```
GET     /api/service-request-settings/categories
POST    /api/service-request-settings/categories
POST    /api/service-request-settings/categories/reorder
DELETE  /api/service-request-settings/categories/{id}
GET     /api/service-request-settings/categories/{id}
PUT     /api/service-request-settings/categories/{id}
GET     /api/service-request-settings/categories/{categoryId}/subcategories
POST    /api/service-request-settings/categories/{categoryId}/subcategories/reorder
GET     /api/service-request-settings/custom-fields
POST    /api/service-request-settings/custom-fields
GET     /api/service-request-settings/custom-fields/applicable
GET     /api/service-request-settings/custom-fields/count
POST    /api/service-request-settings/custom-fields/reorder
DELETE  /api/service-request-settings/custom-fields/{id}
GET     /api/service-request-settings/custom-fields/{id}
PUT     /api/service-request-settings/custom-fields/{id}
GET     /api/service-request-settings/subcategories
POST    /api/service-request-settings/subcategories
DELETE  /api/service-request-settings/subcategories/{id}
GET     /api/service-request-settings/subcategories/{id}
PUT     /api/service-request-settings/subcategories/{id}
GET     /api/service-request-settings/types
POST    /api/service-request-settings/types
GET     /api/service-request-settings/types/by-category/{categoryId}
GET     /api/service-request-settings/types/by-subcategory/{subcategoryId}
GET     /api/service-request-settings/types/grouped
POST    /api/service-request-settings/types/reorder/{subcategoryId}
DELETE  /api/service-request-settings/types/{id}
GET     /api/service-request-settings/types/{id}
PUT     /api/service-request-settings/types/{id}
GET     /api/servicerequests
POST    /api/servicerequests
GET     /api/servicerequests/assignee/{userId}
GET     /api/servicerequests/contact/{contactId}
GET     /api/servicerequests/count/open
GET     /api/servicerequests/count/sla-breached
GET     /api/servicerequests/customer/{customerId}
GET     /api/servicerequests/group/{groupId}
GET     /api/servicerequests/my-requests
GET     /api/servicerequests/statistics
GET     /api/servicerequests/ticket/{ticketNumber}
DELETE  /api/servicerequests/{id}
GET     /api/servicerequests/{id}
PUT     /api/servicerequests/{id}
POST    /api/servicerequests/{id}/assign/group/{groupId}
POST    /api/servicerequests/{id}/assign/user/{userId}
POST    /api/servicerequests/{id}/close
PUT     /api/servicerequests/{id}/custom-fields
POST    /api/servicerequests/{id}/escalate
POST    /api/servicerequests/{id}/feedback
POST    /api/servicerequests/{id}/first-response
POST    /api/servicerequests/{id}/reopen
POST    /api/servicerequests/{id}/resolve
PATCH   /api/servicerequests/{id}/status
POST    /api/servicerequests/{id}/unassign
```

</details>

### 10.7 ITSM (154 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/itsm/catalog` | CatalogController | 10 | GET:7, PATCH:1, POST:2 |
| `/api/itsm/changes` | ChangesController | 13 | GET:5, PATCH:2, POST:6 |
| `/api/itsm/chatbot` | SelfServiceChatbotController | 13 | GET:5, POST:8 |
| `/api/itsm/cicd` | CICDIntegrationController | 12 | DELETE:1, GET:3, POST:7, PUT:1 |
| `/api/itsm/cmdb` | CMDBController | 18 | GET:13, POST:3, PUT:2 |
| `/api/itsm/dashboard` | ITSMDashboardController | 14 | GET:14 |
| `/api/itsm/email` | EmailToTicketController | 5 | GET:2, POST:2, PUT:1 |
| `/api/itsm/incidents` | IncidentsController | 11 | GET:3, PATCH:5, POST:2, PUT:1 |
| `/api/itsm/knowledge` | KnowledgeController | 16 | GET:11, PATCH:2, POST:2, PUT:1 |
| `/api/itsm/monitoring` | MonitoringIntegrationController | 11 | DELETE:1, GET:5, POST:4, PUT:1 |
| `/api/itsm/problems` | ProblemsController | 9 | GET:4, PATCH:2, POST:2, PUT:1 |
| `/api/itsm/sla` | SLAController | 11 | GET:7, POST:4 |
| `/api/itsm/webhooks` | ITSMWebhooksController | 11 | DELETE:1, GET:5, POST:4, PUT:1 |

<details>
<summary>All ITSM endpoints</summary>

```
GET     /api/itsm/catalog/categories
GET     /api/itsm/catalog/featured
GET     /api/itsm/catalog/items
GET     /api/itsm/catalog/items/{id}
GET     /api/itsm/catalog/my-requests
POST    /api/itsm/catalog/requests
POST    /api/itsm/catalog/requests/for-others
GET     /api/itsm/catalog/requests/{requestId}
PATCH   /api/itsm/catalog/requests/{requestId}/cancel
GET     /api/itsm/catalog/search
GET     /api/itsm/changes
POST    /api/itsm/changes
GET     /api/itsm/changes/blackouts
POST    /api/itsm/changes/blackouts
GET     /api/itsm/changes/calendar
GET     /api/itsm/changes/{id}
POST    /api/itsm/changes/{id}/approvals
POST    /api/itsm/changes/{id}/check-conflicts
GET     /api/itsm/changes/{id}/impacted-cis
POST    /api/itsm/changes/{changeId}/impacted-cis/{ciId}
POST    /api/itsm/changes/{id}/rejections
PATCH   /api/itsm/changes/{id}/schedule
PATCH   /api/itsm/changes/{id}/submit-approval
GET     /api/itsm/chatbot/incidents/{incidentNumber}/status
POST    /api/itsm/chatbot/message
GET     /api/itsm/chatbot/quick-actions
POST    /api/itsm/chatbot/quick-actions/{actionId}
GET     /api/itsm/chatbot/search
POST    /api/itsm/chatbot/search
POST    /api/itsm/chatbot/session
POST    /api/itsm/chatbot/session/{sessionId}/create-incident
POST    /api/itsm/chatbot/session/{sessionId}/end
GET     /api/itsm/chatbot/session/{sessionId}/history
POST    /api/itsm/chatbot/sessions
GET     /api/itsm/chatbot/sessions/{sessionId}
POST    /api/itsm/chatbot/sessions/{sessionId}/messages
POST    /api/itsm/cicd/deployment
POST    /api/itsm/cicd/deployment-complete
GET     /api/itsm/cicd/deployments
POST    /api/itsm/cicd/deployments
PUT     /api/itsm/cicd/deployments/{changeId}/status
GET     /api/itsm/cicd/pipelines
POST    /api/itsm/cicd/pipelines
DELETE  /api/itsm/cicd/pipelines/{id}
GET     /api/itsm/cicd/pipelines/{id}
POST    /api/itsm/cicd/validate
POST    /api/itsm/cicd/webhooks/azure-devops
POST    /api/itsm/cicd/webhooks/github
GET     /api/itsm/cmdb
POST    /api/itsm/cmdb
GET     /api/itsm/cmdb/cis
POST    /api/itsm/cmdb/cis
GET     /api/itsm/cmdb/cis/{id}
PUT     /api/itsm/cmdb/cis/{id}
GET     /api/itsm/cmdb/cis/{id}/impact
GET     /api/itsm/cmdb/cis/{id}/impact-analysis
GET     /api/itsm/cmdb/cis/{id}/related
GET     /api/itsm/cmdb/cis/{id}/relationships
GET     /api/itsm/cmdb/cis/{id}/service-map
GET     /api/itsm/cmdb/types
GET     /api/itsm/cmdb/{id}
PUT     /api/itsm/cmdb/{id}
GET     /api/itsm/cmdb/{id}/impact-analysis
GET     /api/itsm/cmdb/{id}/related
GET     /api/itsm/cmdb/{id}/service-map
POST    /api/itsm/cmdb/{parentId}/relationships/{childId}
GET     /api/itsm/dashboard/agent-performance
GET     /api/itsm/dashboard/agents
GET     /api/itsm/dashboard/category-breakdown
GET     /api/itsm/dashboard/changes
GET     /api/itsm/dashboard/cmdb
GET     /api/itsm/dashboard/executive
GET     /api/itsm/dashboard/executive-summary
GET     /api/itsm/dashboard/incident-trends
GET     /api/itsm/dashboard/incidents
GET     /api/itsm/dashboard/knowledge
GET     /api/itsm/dashboard/metrics
GET     /api/itsm/dashboard/problems
GET     /api/itsm/dashboard/sla
GET     /api/itsm/dashboard/sla-compliance
GET     /api/itsm/email/config
PUT     /api/itsm/email/config
GET     /api/itsm/email/history
POST    /api/itsm/email/inbound
POST    /api/itsm/email/test
GET     /api/itsm/incidents
POST    /api/itsm/incidents
GET     /api/itsm/incidents/{id}
PUT     /api/itsm/incidents/{id}
PATCH   /api/itsm/incidents/{id}/assign
PATCH   /api/itsm/incidents/{id}/close
GET     /api/itsm/incidents/{id}/comments
POST    /api/itsm/incidents/{id}/comments
PATCH   /api/itsm/incidents/{id}/escalate
PATCH   /api/itsm/incidents/{id}/reopen
PATCH   /api/itsm/incidents/{id}/resolve
POST    /api/itsm/knowledge
GET     /api/itsm/knowledge/articles
GET     /api/itsm/knowledge/articles/popular
GET     /api/itsm/knowledge/articles/recent
GET     /api/itsm/knowledge/articles/{id}
GET     /api/itsm/knowledge/categories
GET     /api/itsm/knowledge/pending
GET     /api/itsm/knowledge/popular
GET     /api/itsm/knowledge/recent
GET     /api/itsm/knowledge/search
GET     /api/itsm/knowledge/suggestions
GET     /api/itsm/knowledge/{id}
PUT     /api/itsm/knowledge/{id}
POST    /api/itsm/knowledge/{id}/feedback
PATCH   /api/itsm/knowledge/{id}/publish
PATCH   /api/itsm/knowledge/{id}/retire
GET     /api/itsm/monitoring/alert-mappings
POST    /api/itsm/monitoring/alerts
GET     /api/itsm/monitoring/history
GET     /api/itsm/monitoring/integrations
POST    /api/itsm/monitoring/integrations
DELETE  /api/itsm/monitoring/integrations/{id}
GET     /api/itsm/monitoring/integrations/{id}
PUT     /api/itsm/monitoring/integrations/{id}
POST    /api/itsm/monitoring/integrations/{id}/test
POST    /api/itsm/monitoring/prometheus
GET     /api/itsm/monitoring/sources
GET     /api/itsm/problems
POST    /api/itsm/problems
GET     /api/itsm/problems/{id}
PUT     /api/itsm/problems/{id}
GET     /api/itsm/problems/{id}/incidents
GET     /api/itsm/problems/{id}/related-incidents
PATCH   /api/itsm/problems/{id}/mark-known-error
PATCH   /api/itsm/problems/{id}/rca
POST    /api/itsm/problems/{problemId}/link-incident/{incidentId}
GET     /api/itsm/sla/at-risk
GET     /api/itsm/sla/breached
POST    /api/itsm/sla/check-breaches
GET     /api/itsm/sla/dashboard
GET     /api/itsm/sla/instances/{targetId}/{targetType}
POST    /api/itsm/sla/instances/{targetId}/{targetType}/pause
POST    /api/itsm/sla/instances/{targetId}/{targetType}/resume
GET     /api/itsm/sla/metrics
GET     /api/itsm/sla/policies
POST    /api/itsm/sla/policies
GET     /api/itsm/sla/policies/{id}
GET     /api/itsm/webhooks
POST    /api/itsm/webhooks
GET     /api/itsm/webhooks/deliveries
POST    /api/itsm/webhooks/deliveries/{deliveryId}/retry
GET     /api/itsm/webhooks/event-types
GET     /api/itsm/webhooks/subscriptions
POST    /api/itsm/webhooks/subscriptions
DELETE  /api/itsm/webhooks/subscriptions/{id}
GET     /api/itsm/webhooks/subscriptions/{id}
PUT     /api/itsm/webhooks/subscriptions/{id}
POST    /api/itsm/webhooks/subscriptions/{id}/test
```

</details>

### 10.8 AI & Analytics (68 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/ai/chatbot` | AIChatbotController | 4 | GET:2, POST:2 |
| `/api/ai/email` | AIEmailController | 4 | POST:4 |
| `/api/dashboard-config` | DashboardConfigController | 15 | DELETE:2, GET:7, POST:4, PUT:2 |
| `/api/dashboards` | DashboardController | 15 | GET:15 |
| `/api/reports` | ReportsController | 30 | DELETE:4, GET:14, POST:9, PUT:3 |

<details>
<summary>All AI & Analytics endpoints</summary>

```
GET     /api/ai/chatbot/health
POST    /api/ai/chatbot/initialize
POST    /api/ai/chatbot/message
GET     /api/ai/chatbot/suggestions
POST    /api/ai/email/analyze
POST    /api/ai/email/improve
POST    /api/ai/email/optimize-subject
POST    /api/ai/email/suggest-response
GET     /api/dashboard-config/dashboards
POST    /api/dashboard-config/dashboards
GET     /api/dashboard-config/dashboards/all
GET     /api/dashboard-config/dashboards/default
GET     /api/dashboard-config/dashboards/{dashboardId}/widgets
POST    /api/dashboard-config/dashboards/{dashboardId}/reorder-widgets
DELETE  /api/dashboard-config/dashboards/{id}
GET     /api/dashboard-config/dashboards/{id}
PUT     /api/dashboard-config/dashboards/{id}
GET     /api/dashboard-config/data-sources
POST    /api/dashboard-config/initialize
GET     /api/dashboard-config/widget-types
POST    /api/dashboard-config/widgets
DELETE  /api/dashboard-config/widgets/{id}
PUT     /api/dashboard-config/widgets/{id}
GET     /api/dashboards/activities
GET     /api/dashboards/analysis/win-loss
GET     /api/dashboards/customers/acquisition
GET     /api/dashboards/customers/top
GET     /api/dashboards/deals-closing-soon
GET     /api/dashboards/forecast
GET     /api/dashboards/funnel/leads
GET     /api/dashboards/leaderboard/activities
GET     /api/dashboards/leaderboard/sales
GET     /api/dashboards/pipeline
GET     /api/dashboards/stats
GET     /api/dashboards/summary
GET     /api/dashboards/tasks
GET     /api/dashboards/trends/revenue
GET     /api/dashboards/widgets
GET     /api/reports
POST    /api/reports
GET     /api/reports/category/{category}
GET     /api/reports/favorites
GET     /api/reports/folder/{folderId}
GET     /api/reports/folders
POST    /api/reports/folders
GET     /api/reports/my
GET     /api/reports/standard
DELETE  /api/reports/folders/{id}
PUT     /api/reports/folders/{id}
DELETE  /api/reports/{id}
GET     /api/reports/{id}
PUT     /api/reports/{id}
POST    /api/reports/{id}/clone
POST    /api/reports/{id}/execute
GET     /api/reports/{id}/export/{format}
DELETE  /api/reports/{id}/favorite
POST    /api/reports/{id}/favorite
GET     /api/reports/{id}/history
POST    /api/reports/{id}/move/{folderId}
GET     /api/reports/{id}/preview
POST    /api/reports/{id}/share
GET     /api/reports/{id}/sharing
GET     /api/reports/{reportId}/history/{executionId}
GET     /api/reports/{reportId}/schedules
POST    /api/reports/{reportId}/schedules
DELETE  /api/reports/{reportId}/schedules/{scheduleId}
PUT     /api/reports/{reportId}/schedules/{scheduleId}
POST    /api/reports/{reportId}/schedules/{scheduleId}/toggle
```

</details>

### 10.9 Workflows & Automation (85 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/webhooks` | WebhooksController | 8 | GET:1, POST:7 |
| `/api/webhooks/docuseal` | DocuSealWebhookController | 2 | GET:1, POST:1 |
| `/api/workflow-instances` | WorkflowInstanceController | 27 | GET:13, POST:14 |
| `/api/workflow-triggers` | WorkflowTriggersController | 16 | DELETE:1, GET:5, POST:9, PUT:1 |
| `/api/workflows` | WorkflowController | 32 | DELETE:4, GET:11, POST:10, PUT:7 |

<details>
<summary>All Workflows & Automation endpoints</summary>

```
POST    /api/webhooks/email/inbound
POST    /api/webhooks/facebook
POST    /api/webhooks/instagram
POST    /api/webhooks/linkedin
POST    /api/webhooks/twitter
GET     /api/webhooks/verify
POST    /api/webhooks/web-form
POST    /api/webhooks/whatsapp
POST    /api/webhooks/docuseal
GET     /api/webhooks/docuseal/health
GET     /api/workflow-instances
POST    /api/workflow-instances
POST    /api/workflow-instances/bulk-start
POST    /api/workflow-instances/callout/test
POST    /api/workflow-instances/callout/validate
GET     /api/workflow-instances/dashboard
GET     /api/workflow-instances/definitions/{definitionId}/audit-log
GET     /api/workflow-instances/definitions/{definitionId}/audit-log/export
GET     /api/workflow-instances/entity/{entityType}/{entityId}
GET     /api/workflow-instances/my-tasks
GET     /api/workflow-instances/statistics
GET     /api/workflow-instances/waiting-nodes
POST    /api/workflow-instances/waiting-nodes/{nodeInstanceId}/resume
POST    /api/workflow-instances/tasks/{taskId}/claim
POST    /api/workflow-instances/tasks/{taskId}/complete
POST    /api/workflow-instances/tasks/{taskId}/reassign
GET     /api/workflow-instances/{id}
POST    /api/workflow-instances/{id}/advance/{nodeInstanceId}
POST    /api/workflow-instances/{id}/cancel
GET     /api/workflow-instances/{id}/child-instances
GET     /api/workflow-instances/{id}/logs
GET     /api/workflow-instances/{id}/parallel-branches
POST    /api/workflow-instances/{id}/pause
POST    /api/workflow-instances/{id}/resume
POST    /api/workflow-instances/{id}/retry
POST    /api/workflow-instances/{id}/skip-node/{nodeId}
GET     /api/workflow-instances/{id}/timeline
GET     /api/workflow-triggers
POST    /api/workflow-triggers
POST    /api/workflow-triggers/evaluate
GET     /api/workflow-triggers/scheduled/due
GET     /api/workflow-triggers/statistics
POST    /api/workflow-triggers/validate/cron
POST    /api/workflow-triggers/validate/filter
GET     /api/workflow-triggers/workflow/{workflowDefinitionId}
DELETE  /api/workflow-triggers/{id}
GET     /api/workflow-triggers/{id}
PUT     /api/workflow-triggers/{id}
POST    /api/workflow-triggers/{id}/activate
POST    /api/workflow-triggers/{id}/deactivate
POST    /api/workflow-triggers/{id}/fire
POST    /api/workflow-triggers/{id}/record-execution
POST    /api/workflow-triggers/{id}/update-schedule
GET     /api/workflows
POST    /api/workflows
GET     /api/workflows/categories
GET     /api/workflows/config
GET     /api/workflows/entity-types
GET     /api/workflows/llm-settings
PUT     /api/workflows/llm-settings
POST    /api/workflows/llm-settings/initialize
POST    /api/workflows/llm-settings/reset
GET     /api/workflows/node-types
GET     /api/workflows/statistics
DELETE  /api/workflows/nodes/{nodeId}
PUT     /api/workflows/nodes/{nodeId}
DELETE  /api/workflows/transitions/{transitionId}
PUT     /api/workflows/transitions/{transitionId}
GET     /api/workflows/versions/compare
DELETE  /api/workflows/versions/{versionId}
GET     /api/workflows/versions/{versionId}
PUT     /api/workflows/versions/{versionId}
PUT     /api/workflows/versions/{versionId}/layout
POST    /api/workflows/versions/{versionId}/nodes
PUT     /api/workflows/versions/{versionId}/nodes/positions
POST    /api/workflows/versions/{versionId}/publish
POST    /api/workflows/versions/{versionId}/transitions
DELETE  /api/workflows/{id}
GET     /api/workflows/{id}
PUT     /api/workflows/{id}
POST    /api/workflows/{id}/activate/{versionId}
POST    /api/workflows/{id}/pause
POST    /api/workflows/{workflowId}/rollback/{versionId}
GET     /api/workflows/{workflowId}/versions
POST    /api/workflows/{workflowId}/versions
```

</details>

### 10.10 Communications (46 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/calendar` | CalendarIntegrationController | 10 | DELETE:1, GET:7, POST:1, PUT:1 |
| `/api/communications` | CommunicationsController | 21 | DELETE:3, GET:9, PATCH:3, POST:4, PUT:2 |
| `/api/conversations` | ConversationsController | 9 | DELETE:1, GET:4, PATCH:1, POST:2, PUT:1 |
| `/api/news-social` | NewsSocialController | 6 | GET:4, POST:2 |

<details>
<summary>All Communications endpoints</summary>

```
GET     /api/calendar/callback/google
GET     /api/calendar/callback/outlook
GET     /api/calendar/connect/google
GET     /api/calendar/connect/outlook
GET     /api/calendar/integrations
PUT     /api/calendar/integrations/{id}
DELETE  /api/calendar/integrations/{provider}
GET     /api/calendar/integrations/{provider}
POST    /api/calendar/sync/{provider}
GET     /api/calendar/sync/{provider}/history
GET     /api/communications/channels
POST    /api/communications/channels
DELETE  /api/communications/channels/{id}
GET     /api/communications/channels/{id}
PUT     /api/communications/channels/{id}
POST    /api/communications/channels/{id}/test
GET     /api/communications/conversations
GET     /api/communications/conversations/{id}
GET     /api/communications/messages
POST    /api/communications/messages/send
DELETE  /api/communications/messages/{id}
GET     /api/communications/messages/{id}
PATCH   /api/communications/messages/{id}/archive
PATCH   /api/communications/messages/{id}/read
PATCH   /api/communications/messages/{id}/star
GET     /api/communications/stats
GET     /api/communications/templates
POST    /api/communications/templates
DELETE  /api/communications/templates/{id}
GET     /api/communications/templates/{id}
PUT     /api/communications/templates/{id}
GET     /api/conversations
POST    /api/conversations
GET     /api/conversations/by-conversation-id/{conversationId}
GET     /api/conversations/by-entity/{entityType}/{entityId}
GET     /api/conversations/{id}
DELETE  /api/conversations/{id}
PUT     /api/conversations/{id}
POST    /api/conversations/{id}/assign
PATCH   /api/conversations/{id}/status
GET     /api/news-social/news
GET     /api/news-social/social
GET     /api/news-social/status
GET     /api/news-social/{customerId}
POST    /api/news-social/refresh/{customerId}
POST    /api/news-social/sentiment
```

</details>

### 10.11 Administration (241 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/admin/features` | FeaturesController | 4 | GET:4 |
| `/api/adminsettings` | AdminSettingsController | 31 | DELETE:4, GET:12, POST:12, PUT:3 |
| `/api/ai` | AIAnalyticsController | 21 | DELETE:1, GET:10, POST:9, PUT:1 |
| `/api/colorpalettes` | ColorPalettesController | 10 | DELETE:1, GET:7, POST:2 |
| `/api/email` | EmailIntegrationController | 5 | DELETE:1, GET:1, POST:2, PUT:1 |
| `/api/emailtemplates` | EmailTemplatesController | 8 | DELETE:1, GET:3, POST:3, PUT:1 |
| `/api/fieldmasterdatas` | FieldMasterDataController | 9 | DELETE:1, GET:5, POST:2, PUT:1 |
| `/api/fileuploads` | FileUploadController | 6 | DELETE:1, POST:5 |
| `/api/importexports` | ImportExportController | 4 | GET:3, POST:1 |
| `/api/lookups` | LookupsController | 2 | GET:2 |
| `/api/masterdatas` | MasterDataController | 18 | DELETE:3, GET:8, POST:5, PUT:2 |
| `/api/modulefieldconfigurations` | ModuleFieldConfigurationsController | 8 | DELETE:1, GET:2, POST:4, PUT:1 |
| `/api/moduleuiconfigs` | ModuleUIConfigController | 12 | GET:3, POST:4, PUT:5 |
| `/api/navigations` | NavigationController | 10 | GET:9, POST:1 |
| `/api/normalizations` | NormalizationController | 8 | GET:8 |
| `/api/postalcodes` | ZipCodesController | 16 | GET:12, POST:4 |
| `/api/sampledatas` | SampleDataController | 10 | DELETE:1, GET:1, POST:8 |
| `/api/systemsettings` | SystemSettingsController | 19 | DELETE:3, GET:7, POST:6, PUT:3 |
| `/api/tasks` | TasksController | 9 | DELETE:1, GET:5, POST:2, PUT:1 |
| `/api/teams` | TeamsController | 31 | DELETE:4, GET:18, POST:5, PUT:4 |

### 10.12 Infrastructure (66 endpoints)

| Base Route | Controller | Endpoints | Methods |
|------------|-----------|-----------|---------|
| `/api/clouddeployments` | CloudDeploymentController | 23 | DELETE:2, GET:11, POST:8, PUT:2 |
| `/api/databases` | DatabaseController | 18 | GET:7, POST:10, PUT:1 |
| `/api/health` | ProviderHealthController | 3 | GET:3 |
| `/api/monitorings` | MonitoringController | 19 | GET:19 |
| `/healths` | HealthController | 3 | GET:3 |

<details>
<summary>All Infrastructure endpoints</summary>

```
GET     /api/clouddeployments/attempts/{attemptId}
GET     /api/clouddeployments/attempts/{attemptId}/logs
GET     /api/clouddeployments/dashboard
GET     /api/clouddeployments/deployments
POST    /api/clouddeployments/deployments
DELETE  /api/clouddeployments/deployments/{id}
GET     /api/clouddeployments/deployments/{id}
PUT     /api/clouddeployments/deployments/{id}
GET     /api/clouddeployments/deployments/{deploymentId}/attempts
POST    /api/clouddeployments/deployments/{deploymentId}/health-check
GET     /api/clouddeployments/deployments/{deploymentId}/health-history
POST    /api/clouddeployments/deployments/{id}/deploy
POST    /api/clouddeployments/deployments/{id}/restart
POST    /api/clouddeployments/deployments/{id}/scale
POST    /api/clouddeployments/deployments/{id}/stop
GET     /api/clouddeployments/health
GET     /api/clouddeployments/providers
POST    /api/clouddeployments/providers
POST    /api/clouddeployments/providers/test
DELETE  /api/clouddeployments/providers/{id}
GET     /api/clouddeployments/providers/{id}
PUT     /api/clouddeployments/providers/{id}
GET     /api/clouddeployments/providers/{id}/resources/{resourceType}
POST    /api/databases/backup
GET     /api/databases/backups
POST    /api/databases/clear-data
GET     /api/databases/foreign-keys
POST    /api/databases/generate-migration-script
GET     /api/databases/generate-seed-script
GET     /api/databases/linked-entities-schema
POST    /api/databases/migrate
POST    /api/databases/optimize
GET     /api/databases/providers
POST    /api/databases/rebuild-indexes
POST    /api/databases/refresh-statistics
POST    /api/databases/reseed
POST    /api/databases/restore/{backupId}
GET     /api/databases/statistics-schedule
PUT     /api/databases/statistics-schedule
GET     /api/databases/status
POST    /api/databases/test-connection
GET     /api/health/providers
GET     /api/health/providers/registry
GET     /api/health/providers/{category}
GET     /api/monitorings/all
GET     /api/monitorings/config
GET     /api/monitorings/containers
GET     /api/monitorings/database
GET     /api/monitorings/environment
GET     /api/monitorings/health
GET     /api/monitorings/health/detailed
GET     /api/monitorings/health/live
GET     /api/monitorings/health/ready
GET     /api/monitorings/infrastructure
GET     /api/monitorings/pods
GET     /api/monitorings/portainer/containers
GET     /api/monitorings/portainer/status
GET     /api/monitorings/services
GET     /api/monitorings/sessions
GET     /api/monitorings/system
GET     /api/monitorings/tools/status
GET     /api/monitorings/uptime-kuma/monitors
GET     /api/monitorings/uptime-kuma/status
GET     /healths
GET     /healths/live
GET     /healths/ready
```

</details>

### 10.13 Pagination

```json
// Request
GET /api/accounts?page=1&pageSize=20&sortBy=name&sortOrder=asc

// Response
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
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

### Issue: Browser API calls time out on private network
**Cause:** Frontend resolves API base URL to `http://<host>:5000` on private networks; port 5000 may be blocked externally, while nginx proxies `/api` on port 80.  
**Solution:** Use same-origin API base URL in production (nginx proxy), or set `REACT_APP_API_URL` to the public origin (e.g., `http://192.168.0.9`).

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
- Certificate password: Set via `SSL_CERT_PASSWORD` environment variable (never hardcode)
- HTTPS port: 5001 (configurable)
- HTTP port: 5000 (always available)
- Health endpoints skip HTTPS redirect for Kubernetes probes
- If no certificate is found, server runs HTTP-only (graceful fallback)

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
