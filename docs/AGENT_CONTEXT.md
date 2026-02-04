# AI Agent Context - CRM Solution

> **Purpose:** Provide context for AI assistants to understand, maintain, and evolve the CRM Solution.  
> **Last Updated:** February 4, 2026  
> **This Document:** Should be updated whenever the solution structure, patterns, or key files change.

---

## 🚧 ACTIVE PROJECT: Pluggable Architecture Implementation

**Before doing anything else, check the implementation tracker:**

📋 **[docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md](architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md)**

| Document | Purpose |
|----------|---------|
| **Implementation Tracker** | 237 task checkboxes, current progress, session log |
| **ADR-001** | Architecture patterns, code examples, configuration |
| **Copilot Instructions** | `.github/copilot-instructions.md` |

---

## 0. How to Use This Document

### For AI Agents (Claude, Copilot, etc.)

1. **Check the Implementation Tracker first** for current work in progress
2. **Start here** for quick orientation to the codebase
3. **Read SOLUTION_CONTEXT.md** (root) for comprehensive technical details
4. **Reference docs/** for detailed documentation on specific topics
5. **Follow update rules** in Section 7 when making changes

### Document Hierarchy

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DOCUMENTATION HIERARCHY                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  SOLUTION_CONTEXT.md (root)                                       │   │
│  │  ► Complete technical reference                                   │   │
│  │  ► Build system, deployment, secrets                              │   │
│  │  ► Authentication, health checks                                  │   │
│  │  ► All API endpoints with examples                                │   │
│  │  ► Troubleshooting and debugging                                  │   │
│  │  ► Quick reference commands                                       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              │                                           │
│                              ▼                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  docs/AGENT_CONTEXT.md (this file)                                │   │
│  │  ► Quick orientation for AI agents                                │   │
│  │  ► Key file locations                                             │   │
│  │  ► Common operations guide                                        │   │
│  │  ► Documentation update rules                                     │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              │                                           │
│                              ▼                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER  │   │
│  │  ► Current implementation progress                                │   │
│  │  ► 237 task checkboxes                                            │   │
│  │  ► Session progress log                                           │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              │                                           │
│                              ▼                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  docs/INDEX.md                                                    │   │
│  │  ► Navigation hub for all documentation                          │   │
│  │  ► Links to 10 organized documentation sections                  │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              │                                           │
│                              ▼                                           │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  docs/01-10 Sections                                              │   │
│  │  01-architecture/  System architecture, diagrams                  │   │
│  │  02-design/        Data models, workflows, UI patterns            │   │
│  │  03-backend/       Entities, services, data access                │   │
│  │  04-api/           Complete API reference                         │   │
│  │  05-frontend/      Pages, components, state                       │   │
│  │  06-standards/     Coding conventions                             │   │
│  │  07-testing/       Test strategy and patterns                     │   │
│  │  08-deployment/    Docker, Kubernetes, builds                     │   │
│  │  09-operations/    Monitoring, runbooks                           │   │
│  │  10-traceability/  Feature-to-code mapping                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Quick Reference

### 1.1 Solution Summary

**CRM Solution** is a full-stack Customer Relationship Management system:
- **Backend:** ASP.NET Core 8.0 + Entity Framework Core
- **Frontend:** React 18 + TypeScript + Material-UI 5
- **Database:** MariaDB (primary), SQL Server, PostgreSQL supported
- **Real-time:** SignalR WebSocket communication
- **Deployment:** Docker + Kubernetes

### 1.2 Key Files to Read First

| Context Needed | Primary File | Supporting Docs |
|----------------|--------------|-----------------|
| **Complete solution overview** | `SOLUTION_CONTEXT.md` (root) | - |
| **Architecture** | `docs/01-architecture/README.md` | `ARCHITECTURE_OVERVIEW.md` |
| **All entities** | `CRM.Backend/src/CRM.Core/Entities/` | `docs/03-backend/README.md` |
| **All API endpoints** | `CRM.Backend/src/CRM.Api/Controllers/` | `docs/04-api/README.md` |
| **Database schema** | `database/schema/` | `docs/03-backend/README.md` |
| **Frontend pages** | `CRM.Frontend/src/pages/` | `docs/05-frontend/README.md` |
| **Build commands** | `scripts/build/` | `SOLUTION_CONTEXT.md` §2 |
| **Recent changes** | `CHANGELOG.md` | `SOLUTION_CONTEXT.md` §18-19 |
| **Feature traceability** | `docs/10-traceability/README.md` | - |
| **Coding standards** | `docs/06-standards/README.md` | - |

### 1.3 Current Version

```json
{
  "version": "1.7.28",
  "environment": "development",
  "database": "MariaDB 10.11",
  "backend": ".NET 8.0",
  "frontend": "React 18 + MUI 5"
}
```

> **Version File:** Update `version.json` in root when releasing

---

## 2. Repository Structure

```
crm-solution/
├── CRM.Backend/                    # .NET Backend
│   ├── src/
│   │   ├── CRM.Api/                # Controllers, Middleware, Hubs
│   │   ├── CRM.Core/               # Entities, DTOs, Interfaces
│   │   ├── CRM.Infrastructure/     # DbContext, Services
│   │   └── CRM.DatabaseSeeder/     # Seeding utility
│   ├── tests/                      # Unit/Integration tests
│   └── migrations/                 # SQL Server migrations
│
├── CRM.Frontend/                   # React Frontend
│   └── src/
│       ├── components/             # Reusable components
│       ├── pages/                  # Page components
│       ├── services/               # API service layer
│       ├── contexts/               # React contexts
│       └── hooks/                  # Custom hooks
│
├── database/                       # MariaDB scripts
│   ├── schema/                     # Table definitions (001-008)
│   ├── seed/                       # Seed data
│   └── master_data/                # Reference data
│
├── docker/                         # Docker files
├── kubernetes/                     # K8s manifests
├── e2e-tests/                      # Playwright tests
├── scripts/                        # Build scripts
│
└── docs/                           # Documentation (you are here)
```

---

## 3. Entity Reference

### 3.1 Core Entities

| Entity | File | Table | Purpose |
|--------|------|-------|---------|
| `User` | `Entities/User.cs` | `Users` | System users |
| `UserGroup` | `Entities/UserGroup.cs` | `UserGroups` | Permission groups |
| `Customer` | `Entities/Customer.cs` | `Customers` | B2B/B2C accounts |
| `Contact` | `Entities/Contact.cs` | `Contacts` | Contact persons |
| `Lead` | `Entities/Lead.cs` | `Leads` | Sales leads |
| `Opportunity` | `Entities/Opportunity.cs` | `Opportunities` | Sales opportunities |
| `Product` | `Entities/Product.cs` | `Products` | Products catalog |
| `Service` | `Entities/Service.cs` | `Services` | Services catalog |
| `Quote` | `Entities/Quote.cs` | `Quotes` | Price quotes |
| `Campaign` | `Entities/Campaign.cs` | `Campaigns` | Marketing campaigns |
| `Task` | `Entities/Task.cs` | `Tasks` | User tasks |
| `Activity` | `Entities/Activity.cs` | `Activities` | Activity tracking |
| `Note` | `Entities/Note.cs` | `Notes` | Entity notes |
| `ServiceRequest` | `Entities/ServiceRequest.cs` | `ServiceRequests` | Support tickets |
| `Workflow` | `Entities/Workflow.cs` | `Workflows` | Automation workflows |
| `SystemSettings` | `Entities/SystemSettings.cs` | `SystemSettings` | Global settings |

### 3.2 Entity Pattern

All entities inherit from `BaseEntity`:
```csharp
public class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }  // Soft delete
}
```

---

## 4. API Patterns

### 4.1 Controller Pattern

Controllers follow REST conventions at `/api/[controller]`:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {Entity}Controller : ControllerBase
{
    [HttpGet]           // GET /api/{entity} - List with pagination
    [HttpGet("{id}")]   // GET /api/{entity}/{id} - Get by ID
    [HttpPost]          // POST /api/{entity} - Create
    [HttpPut("{id}")]   // PUT /api/{entity}/{id} - Update
    [HttpDelete("{id}")]// DELETE /api/{entity}/{id} - Soft delete
}
```

### 4.2 Standard Response Patterns

**Paginated List:**
```json
{
  "items": [...],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

**Single Entity:**
```json
{
  "id": 1,
  "name": "...",
  "createdAt": "2026-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Error Response:**
```json
{
  "error": "Error message",
  "details": "Additional details"
}
```

---

## 5. Frontend Patterns

### 5.1 Page Structure

```typescript
// Standard page structure
const EntityPage: React.FC = () => {
  const [entities, setEntities] = useState<Entity[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  
  useEffect(() => {
    loadData();
  }, [page]);
  
  return (
    <Box>
      <PageHeader title="Entities" onAdd={handleAdd} />
      <DataTable data={entities} columns={columns} />
      <Pagination page={page} onChange={setPage} />
    </Box>
  );
};
```

### 5.2 Service Pattern

```typescript
// API service pattern
export const entityService = {
  getAll: (params?: QueryParams) => 
    api.get<PagedResult<Entity>>('/entity', { params }),
  
  getById: (id: number) => 
    api.get<Entity>(`/entity/${id}`),
  
  create: (data: CreateEntityDto) => 
    api.post<Entity>('/entity', data),
  
  update: (id: number, data: UpdateEntityDto) => 
    api.put<Entity>(`/entity/${id}`, data),
  
  delete: (id: number) => 
    api.delete(`/entity/${id}`)
};
```

---

## 6. Database Patterns

### 6.1 Schema Files

| File | Contents |
|------|----------|
| `001_core_tables.sql` | Users, UserGroups, Customers, Contacts, Addresses |
| `002_master_data_tables.sql` | ZipCodes, ColorPalettes, SystemSettings, LookupItems |
| `003_service_request_tables.sql` | ServiceRequests, Categories, SLAs |
| `004_products_opportunities.sql` | Products, Services, Opportunities, Quotes |
| `005_workflow_tables.sql` | Workflows, Steps, Triggers, Actions |
| `006_activities_communication.sql` | Activities, Notes, Communications |
| `007_consolidated_contact_info.sql` | ContactInfo (Email, Phone, Address) |
| `008_security_enhancements.sql` | Password, 2FA, Group policies |

### 6.2 Common Table Pattern

```sql
CREATE TABLE `EntityName` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  -- Entity-specific columns --
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`),
  -- Indexes and Foreign Keys --
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
```

---

## 7. Documentation Update Rules

> **CRITICAL:** Documentation must be maintained as the solution evolves. Follow these rules for every change.

### 7.1 Documentation Files Reference

| File | Purpose | When to Update |
|------|---------|----------------|
| `SOLUTION_CONTEXT.md` | Complete technical reference | Major changes, new systems |
| `CHANGELOG.md` | Version history | Every release, every PR |
| `version.json` | Version number | Every release |
| `docs/AGENT_CONTEXT.md` | This file - agent orientation | Structure/pattern changes |
| `docs/INDEX.md` | Navigation hub | New doc sections added |
| `docs/10-traceability/README.md` | Feature mapping | Every new feature |

### 7.2 When to Update Documentation

| Change Type | Files to Update |
|-------------|-----------------|
| **New entity/table** | `03-backend/README.md`, `04-api/README.md`, `10-traceability/README.md`, `CHANGELOG.md` |
| **New API endpoint** | `04-api/README.md`, `10-traceability/README.md`, `SOLUTION_CONTEXT.md` §10 |
| **New frontend page** | `05-frontend/README.md`, `10-traceability/README.md` |
| **New feature** | `10-traceability/README.md`, `CHANGELOG.md`, relevant section README |
| **Bug fix** | `CHANGELOG.md` only |
| **Database schema change** | `03-backend/README.md`, `SOLUTION_CONTEXT.md` §8-9 |
| **Security change** | `01-architecture/README.md`, `SOLUTION_CONTEXT.md` §4-5 |
| **Build/deploy change** | `08-deployment/README.md`, `SOLUTION_CONTEXT.md` §2 |
| **New dependency** | `06-standards/README.md` (dependencies section) |
| **Pattern change** | This file (AGENT_CONTEXT.md), `SOLUTION_CONTEXT.md` |

### 7.3 Traceability Matrix Updates

When adding new features, **ALWAYS** update `docs/10-traceability/README.md` with:

```markdown
## [Feature Name]

### Business Description
[What the feature does for end users]

### Implementation Trace

| Layer | Component | File Path | Description |
|-------|-----------|-----------|-------------|
| Entity | Model | `BE:CRM.Core/Entities/X.cs` | Data model |
| DTO | Transfer | `BE:CRM.Core/DTOs/XDto.cs` | API contract |
| Service | Logic | `BE:CRM.Infrastructure/Services/XService.cs` | Business logic |
| Controller | API | `BE:CRM.Api/Controllers/XController.cs` | REST endpoints |
| Page | UI | `FE:pages/X/XPage.tsx` | Frontend page |
| Database | Table | `DB:00X_table.sql` | Schema |
| Tests | E2E | `E2E:x/x.spec.ts` | Test coverage |

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/x` | List all |
| POST | `/api/x` | Create new |
```

### 7.4 CHANGELOG Format

```markdown
## [1.7.29] - 2026-02-XX

### Added
- New feature description (#issue-number)

### Changed
- Modified behavior description

### Fixed
- Bug fix description (#issue-number)

### Security
- Security-related changes
```

### 7.5 Version Bumping Rules

| Change Type | Version Bump | Example |
|-------------|--------------|---------|
| Bug fixes only | Patch | 1.7.28 → 1.7.29 |
| New features (backward compatible) | Minor | 1.7.28 → 1.8.0 |
| Breaking changes | Major | 1.7.28 → 2.0.0 |

### 7.6 Pre-Release Documentation Checklist

Before any release, verify:

- [ ] `CHANGELOG.md` updated with all changes
- [ ] `version.json` version number bumped
- [ ] `10-traceability/README.md` updated for new features
- [ ] API documentation matches implementation
- [ ] Entity list in `03-backend/README.md` is current
- [ ] `SOLUTION_CONTEXT.md` §18-19 updated with recent changes
- [ ] All new files mentioned in relevant docs

### 7.7 Keeping SOLUTION_CONTEXT.md Current

The root `SOLUTION_CONTEXT.md` is the **primary reference** for the solution. Key sections to maintain:

| Section | Content | Update Frequency |
|---------|---------|------------------|
| §2 Build System | Build commands, scripts | When build changes |
| §4 Password/Auth | Authentication flow | Security changes |
| §8-9 Database | Schema files, seeding | Database changes |
| §10 API Endpoints | Endpoint reference | API changes |
| §12 Common Issues | Troubleshooting | As issues arise |
| §18-19 Recent Changes | Modified files log | Every session |

### 7.8 Cross-Reference Maintenance

When updating documentation, ensure cross-references remain valid:

1. Check links between docs sections work
2. Verify file paths in examples exist
3. Update INDEX.md if adding new sections
4. Keep this AGENT_CONTEXT.md aligned with SOLUTION_CONTEXT.md

---

## 8. Common Operations

### 8.1 Adding a New Entity

1. **Create Entity:** `CRM.Core/Entities/NewEntity.cs`
2. **Add DbSet:** `CRM.Infrastructure/Data/CrmDbContext.cs`
3. **Create DTOs:** `CRM.Core/DTOs/NewEntityDto.cs`
4. **Create Service:** `CRM.Infrastructure/Services/NewEntityService.cs`
5. **Create Controller:** `CRM.Api/Controllers/NewEntityController.cs`
6. **Add Migration:** `database/schema/` or EF migration
7. **Create Frontend:** `CRM.Frontend/src/pages/NewEntity/`
8. **Update Docs:** Entity list, API reference, traceability

### 8.2 Adding an API Endpoint

1. **Add method to service interface:** `IXService.cs`
2. **Implement in service:** `XService.cs`
3. **Add controller action:** `XController.cs`
4. **Update API docs:** `docs/04-api/endpoints/`
5. **Add test:** `e2e-tests/` or unit tests

### 8.3 Modifying Database Schema

1. **Update Entity:** `CRM.Core/Entities/`
2. **Create Migration (SQL Server):** `CRM.Backend/migrations/`
3. **Create Schema (MariaDB):** `database/schema/`
4. **Update seed data if needed:** `database/seed/`
5. **Rebuild containers:** `./build.sh backend`

---

## 9. Build & Deploy

### 9.1 Quick Commands

```bash
# Development
./build.sh                    # Build all
./build.sh backend            # Backend only
./build.sh frontend           # Frontend only
./build.sh clean              # Clear cache

# Deployment
./build.sh deploy             # Deploy to K8s
./build.sh remote             # Build on remote server

# Testing
cd e2e-tests && npm test      # Run E2E tests
cd CRM.Backend && dotnet test # Run unit tests
```

### 9.2 Docker Images

| Image | Port | Purpose |
|-------|------|---------|
| `crm-frontend` | 80 | React app (nginx) |
| `crm-backend` / `crm-api` | 5000 | ASP.NET API |
| `crm-mariadb` | 3306 | Database |
| `crm-redis` | 6379 | Cache |

---

## 10. Debugging Tips

### 10.1 Backend Issues

```bash
# Check logs
docker logs crm-api --tail 100

# Test API health
curl http://localhost:5000/api/health

# Check database connection
curl http://localhost:5000/api/health/db
```

### 10.2 Frontend Issues

```bash
# Check browser console for errors
# Verify API URL in environment
# Check network tab for failed requests
```

### 10.3 Common Issues

| Issue | Solution |
|-------|----------|
| CORS errors | Check CORS config in `Program.cs` |
| JWT expired | Refresh token or re-login |
| 500 errors | Check API logs, database connection |
| Build fails | Run `./build.sh clean` then rebuild |

---

## 11. Release Checklist

Before releasing a new version:

- [ ] All tests passing (`dotnet test` and `npm test`)
- [ ] E2E tests passing (`cd e2e-tests && npm test`)
- [ ] CHANGELOG.md updated with all changes
- [ ] version.json version number bumped
- [ ] API documentation updated (`docs/04-api/README.md`)
- [ ] Traceability matrix updated (`docs/10-traceability/README.md`)
- [ ] SOLUTION_CONTEXT.md §18-19 updated with recent changes
- [ ] Database migrations tested
- [ ] Docker images built and tested
- [ ] Documentation cross-references verified

---

## 12. Evolving This Documentation

### 12.1 When to Update This File (AGENT_CONTEXT.md)

Update this file when:
- Repository structure changes
- New entity patterns are introduced
- API patterns change
- Frontend patterns change  
- Build/deploy process changes
- New documentation sections are added

### 12.2 Synchronization with SOLUTION_CONTEXT.md

This file (AGENT_CONTEXT.md) and SOLUTION_CONTEXT.md should stay aligned:

| This File Has | SOLUTION_CONTEXT.md Has |
|---------------|-------------------------|
| Quick overview | Detailed reference |
| Pattern summaries | Full examples |
| Key file locations | Complete file listings |
| Common operations | Step-by-step procedures |

**Rule:** If you update one, check if the other needs updating too.

### 12.3 Documentation Evolution Process

As the solution evolves to release:

1. **During Development:**
   - Update CHANGELOG.md with each significant change
   - Add traceability entries for new features
   - Keep SOLUTION_CONTEXT.md §18-19 current

2. **Before Each Release:**
   - Run the Release Checklist (Section 11)
   - Review all docs for accuracy
   - Update version numbers everywhere

3. **After Major Changes:**
   - Update architecture diagrams if structure changed
   - Revise patterns if conventions changed
   - Update this file if agent context needs change

### 12.4 Self-Updating Guidance for AI Agents

When an AI agent makes changes to the codebase:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    AI AGENT DOCUMENTATION WORKFLOW                       │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  1. BEFORE making changes:                                              │
│     → Read SOLUTION_CONTEXT.md for full context                         │
│     → Check docs/10-traceability/ for existing feature mapping          │
│     → Review relevant section README in docs/                           │
│                                                                          │
│  2. AFTER making code changes:                                          │
│     → Update CHANGELOG.md with change description                       │
│     → If new feature: Add to docs/10-traceability/README.md             │
│     → If API change: Update docs/04-api/README.md                       │
│     → If entity change: Update docs/03-backend/README.md                │
│     → If frontend change: Update docs/05-frontend/README.md             │
│                                                                          │
│  3. FOR significant changes:                                            │
│     → Update SOLUTION_CONTEXT.md relevant sections                      │
│     → Consider if this file (AGENT_CONTEXT.md) needs update            │
│     → Verify cross-references still work                                │
│                                                                          │
│  4. ALWAYS verify:                                                      │
│     → File paths in docs match actual files                             │
│     → Version numbers are consistent                                    │
│     → No broken internal links                                          │
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 13. Contact & Resources

### 13.1 Key Locations

| Resource | Path |
|----------|------|
| Repository Root | `/Users/alal/Code/Git CRM Solution/crm-solution` |
| Documentation Hub | `docs/INDEX.md` |
| Solution Context | `SOLUTION_CONTEXT.md` (root) |
| Agent Context | `docs/AGENT_CONTEXT.md` (this file) |
| Traceability | `docs/10-traceability/README.md` |

### 13.2 Infrastructure

| Component | Address | Port |
|-----------|---------|------|
| Primary Server | 192.168.0.9 | - |
| Frontend | localhost | 80 / 3000 |
| Backend API | localhost | 5000 |
| Database (MariaDB) | localhost | 3306 |
| Redis Cache | localhost | 6379 |

### 13.3 Documentation Quick Links

| Topic | File |
|-------|------|
| Full Technical Reference | `SOLUTION_CONTEXT.md` |
| Architecture | `docs/01-architecture/README.md` |
| Design & Data Models | `docs/02-design/README.md` |
| Backend Guide | `docs/03-backend/README.md` |
| API Reference | `docs/04-api/README.md` |
| Frontend Guide | `docs/05-frontend/README.md` |
| Coding Standards | `docs/06-standards/README.md` |
| Testing Guide | `docs/07-testing/README.md` |
| Deployment Guide | `docs/08-deployment/README.md` |
| Operations & Runbooks | `docs/09-operations/README.md` |
| Feature Traceability | `docs/10-traceability/README.md` |
