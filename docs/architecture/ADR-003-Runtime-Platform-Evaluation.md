# ADR-003: Runtime Platform Evaluation — .NET 10, Go, or Java

> **Status:** Proposed  
> **Date:** 2026-02-11  
> **Decision Makers:** Architecture Team  
> **Supersedes:** None  
> **References:** [ADR-001 Pluggable Architecture](ADR-001-Pluggable-Architecture-Strategy.md), [ADR-002 EF Core Schema Management](ADR-002-EF-Core-Schema-Management.md), [SOLUTION_CONTEXT.md](../../SOLUTION_CONTEXT.md)

---

## 1. Context

### 1.1 Trigger

The CRM backend runs on **.NET 8.0 (LTS)**, which reaches **end-of-support on November 10, 2026** — approximately 9 months from now. A platform decision is required before that date. Three options are evaluated:

| Option | Version | Type | Support Ends |
|--------|---------|------|--------------|
| **.NET 10** | 10.0 (GA Nov 11, 2025) | LTS | November 14, 2028 |
| **Go** | 1.23+ | Rolling | ~1 year per minor |
| **Java** | 21 LTS / 25 LTS (Sep 2025) | LTS | 8+ years (vendor) |

### 1.2 Current Codebase Inventory

This analysis is grounded in measured codebase metrics, not estimates.

#### Source Code Scale

| Dimension | Count | Lines of Code |
|-----------|-------|---------------|
| **C# backend files** | 674 | 402,791 |
| **Test files** | 244 | 152,063 |
| **Frontend (TS/TSX)** | 295 | 134,556 |
| **Grand total** | 1,213 | **689,410** |

#### Backend Structural Breakdown

| Layer | Files | Description |
|-------|-------|-------------|
| Entities | 94 | Domain model classes (`CRM.Core/Entities/`) |
| DTOs | 40+ | Data transfer objects (`CRM.Core/DTOs/`) |
| Interfaces | 103 | 78 service + 25 port interfaces |
| Services | 95 | Business logic (`CRM.Infrastructure/Services/`) |
| Controllers | 91 | REST API endpoints (`CRM.Api/Controllers/`) |
| Providers | 39 | 16 external integration providers |
| Factories | 8 | Provider resolution factories |
| Middleware | 4 | Error handling, rate limiting, security headers, instrumentation |
| Microservice projects | 8 | Gateway, Identity, Customer, Sales, Marketing, ServiceDesk, Core, ServiceDefaults |
| DbContext | 1 | 3,064 lines, 200+ DbSets |
| Program.cs (DI root) | 1 | 879 lines |

#### Type System Scale

| Type | Count |
|------|-------|
| Classes / Records | 2,054 |
| Interfaces | 188 |
| Enums | 340 |

#### API Surface

| Metric | Count |
|--------|-------|
| REST endpoints | ~1,377 |
| SignalR hubs | 1 |
| Webhook controllers | 7 (Stripe, DocuSeal, DocuSign, Chatwoot, Intercom, SendGrid, Twilio) |

#### External Dependencies (38 NuGet packages)

**Framework-coupled (would require replacements in Go/Java):**

| Package | Purpose | Go Equivalent | Java Equivalent |
|---------|---------|---------------|-----------------|
| Entity Framework Core 8.0 | ORM, migrations, 6 DB providers | No direct equiv (sqlx, GORM, ent) | Hibernate/JPA |
| ASP.NET Core + Controllers | REST framework | net/http, chi, gin, fiber | Spring Boot MVC |
| SignalR | WebSocket real-time | gorilla/websocket, melody | Spring WebSocket / STOMP |
| JWT Bearer Auth | Authentication | golang-jwt | Spring Security + jjwt |
| Serilog | Structured logging | zerolog, zap | Logback + SLF4J |
| Swashbuckle | OpenAPI/Swagger | swag, oapi-codegen | springdoc-openapi |
| FeatureManagement | Feature flags | custom / unleash-client | Togglz / FF4J |
| StackExchange.Redis | Caching | go-redis | Lettuce / Jedis |
| Polly | Resilience/retry | custom middleware | Resilience4j |
| ImageSharp | Image processing | imaging, bimg | imgscalr, Thumbnailator |
| AspNetCoreRateLimit | Rate limiting | Built-in (middleware) | Bucket4j |
| BCrypt.Net | Password hashing | golang.org/x/crypto/bcrypt | Spring Security BCrypt |
| Cronos | Cron parsing | robfig/cron | Quartz |

**Provider SDKs (cross-platform, have Go/Java equivalents):**

| Package | Go SDK | Java SDK |
|---------|--------|----------|
| Algolia.Search | algoliasearch-client-go | algoliasearch-client-java |
| Meilisearch | meilisearch-go | meilisearch-java |
| Twilio | twilio-go | twilio-java |
| SendGrid | sendgrid-go | sendgrid-java |
| DocuSign.eSign | docusign-esign-go-client | docusign-esign-java |
| Stripe | stripe-go | stripe-java |
| MongoDB.Driver | mongo-go-driver | mongodb-driver-sync |
| Novu | novu-go (community) | novu-java (community) |

#### Database Layer Complexity

| Metric | Value |
|--------|-------|
| Database tables | ~171 |
| DB providers supported | 6 (MariaDB, PostgreSQL, SQL Server, SQLite, Oracle, MongoDB) |
| DbContext OnModelCreating | 2,651 lines of Fluent API configuration |
| Junction tables (polymorphic) | 8 |
| Junction tables (traditional) | 6 |
| Soft delete filter | Global query filter on all entities |

#### Test Coverage

| Suite | Count |
|-------|-------|
| Active unit/integration tests | 5,160+ |
| Playwright E2E spec files | 39 |
| BVT tests | 118 (100% pass) |

#### Deployment Artifacts

| Artifact | Count |
|----------|-------|
| Dockerfiles | 10 |
| docker-compose files | 9 |
| Kubernetes manifests | Full set (namespace, configmap, secrets, deployments, HPA) |
| CI/CD pipeline (Azure DevOps) | 1 YAML pipeline |

---

## 2. Options Analysis

### 2.1 Option A: Upgrade to .NET 10 (RECOMMENDED)

**.NET 10** was released November 11, 2025 and is the current **LTS** release with support through **November 14, 2028**.

#### Migration Scope

| Task | Effort | Risk |
|------|--------|------|
| Update `TargetFramework` in 12 .csproj files | Minutes | Negligible |
| Update EF Core packages 8.x → 10.x | Hours | Low — breaking changes documented |
| Update ASP.NET Core packages 8.x → 10.x | Hours | Low — mostly additive |
| Update 3rd-party NuGet packages | Hours | Low — most already support .NET 10 |
| Fix any breaking API changes | 1-3 days | Low-Medium — typically <20 call sites |
| Update Dockerfiles (SDK/runtime base images) | Minutes | Negligible |
| Update CI pipeline (SDK version) | Minutes | Negligible |
| Re-run full test suite | Hours | Low |

**Estimated total effort: 3-5 developer-days**

#### What Changes

```diff
- <TargetFramework>net8.0</TargetFramework>
+ <TargetFramework>net10.0</TargetFramework>

- <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.11" />
+ <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.x" />

- FROM mcr.microsoft.com/dotnet/aspnet:8.0
+ FROM mcr.microsoft.com/dotnet/aspnet:10.0
```

#### What Does NOT Change

- All 402,791 lines of C# code — **virtually unchanged**
- All 94 entities, 95 services, 91 controllers — **unchanged**
- All 1,377 API endpoints — **unchanged**
- All 5,160 tests — **unchanged** (re-run to verify)
- Frontend — **zero changes** (talks to same REST API)
- Database schema — **zero changes**
- Docker/K8s topology — **unchanged** (just base image tag)
- 16 provider integrations — **unchanged**
- Hexagonal architecture — **unchanged**

#### Benefits Gained

| Benefit | Description |
|---------|-------------|
| **LTS until Nov 2028** | 3 years of security patches and support |
| **Performance** | 10-30% throughput improvement from runtime optimizations |
| **Native AOT improvements** | Faster cold-start for microservices |
| **EF Core 10 features** | Better query translation, improved migrations, JSON column enhancements |
| **ASP.NET Core 10 features** | Enhanced minimal APIs, better OpenAPI support, improved auth |
| **C# 14 language features** | Field keyword, extension members, params collections |
| **Ecosystem continuity** | All NuGet packages, all tooling, all team knowledge preserved |

---

### 2.2 Option B: Rewrite in Go

#### Migration Scope

| Task | Effort | Risk |
|------|--------|------|
| Design new project structure | 1-2 weeks | Medium |
| Rewrite 94 entity models (structs + tags) | 2-3 weeks | Medium |
| Rewrite DB layer (no EF Core equiv) | 4-8 weeks | **High** |
| Rewrite 95 services | 8-16 weeks | **High** |
| Rewrite 91 controllers + routing | 4-8 weeks | **High** |
| Rewrite 39 provider integrations | 4-6 weeks | **High** |
| Rewrite 8 factory pattern (no DI framework) | 2-3 weeks | Medium |
| Rewrite auth (JWT, BCrypt, 2FA, OAuth) | 2-3 weeks | **High** |
| Rewrite SignalR hub (WebSocket) | 1-2 weeks | Medium |
| Rewrite 4 middleware components | 1 week | Low |
| Rewrite/adapt 5,160 tests | 8-12 weeks | **High** |
| Rebuild 10 Dockerfiles | 1 week | Low |
| Rebuild CI/CD pipeline | 1 week | Low |
| Integration testing & stabilization | 4-8 weeks | **High** |

**Estimated total effort: 12-18 months (2-3 developers)**

#### Structural Challenges

| C#/.NET Feature | Go Challenge |
|----------------|-------------|
| **Generics** (used extensively) | Go generics are limited — no constraint hierarchies, no variance |
| **Entity Framework Core** | No equivalent ORM. GORM is closest but lacks: migrations, multi-provider, Fluent API, global filters, change tracking, lazy loading |
| **Dependency Injection** | No built-in DI container. Must use wire, dig, or manual wiring for 200+ registrations |
| **LINQ** | No equivalent. All queries become raw SQL or query builder calls |
| **Inheritance (TPH/TPT)** | Go has no inheritance. Composition-only requires entity redesign |
| **BaseEntity pattern** | Must be composition (`embed`) — changes every entity definition |
| **Nullable reference types** | Go uses pointers for nullability — different error model |
| **async/await** | Go uses goroutines — fundamentally different concurrency model |
| **Middleware pipeline** | Must build custom middleware chain |
| **SignalR** | No equivalent. Must implement WebSocket protocol from scratch |
| **Feature flags** | No Microsoft.FeatureManagement equivalent |
| **Fluent validation** | No equivalent — manual validation on every endpoint |
| **Swagger/OpenAPI** | swag works but requires manual annotation comments |
| **Multi-DB provider** | Must write 6 separate SQL dialects or use sqlx with dialect switching |
| **200+ DbSets / 3,064ube line OnModelCreating** | Must be manually translated to schema definitions |

#### What Go Does Well

| Strength | Relevance to This Project |
|----------|---------------------------|
| Fast compilation | Moderate — .NET hot reload already fast |
| Small binary size | Moderate — Docker images already 200MB |
| Goroutine concurrency | Low — ASP.NET Core async already handles 10K+ concurrent |
| Simple deployment (single binary) | Low — Docker already provides this |
| Systems programming | Not relevant — this is a business CRUD app |

#### What Go Does Poorly for This Project

| Weakness | Impact |
|----------|--------|
| No mature enterprise ORM | **Critical** — 171 tables, 6 DB providers, 3,064 lines Fluent API |
| No inheritance | **Critical** — TPH entities (Account/Customer), BaseEntity pattern |
| Limited generics | **High** — generic repository, generic factory pattern |
| No DI framework | **High** — 200+ service registrations |
| Verbose error handling | **High** — every function call needs `if err != nil` |
| No LINQ | **High** — complex queries throughout services |
| Small enterprise ecosystem | **Medium** — fewer libraries for CRM-style features |

---

### 2.3 Option C: Rewrite in Java (Spring Boot)

#### Migration Scope

| Task | Effort | Risk |
|------|--------|------|
| Design Maven/Gradle project structure | 1-2 weeks | Low |
| Rewrite 94 entities (JPA annotations) | 3-4 weeks | Medium |
| Rewrite DB layer (Hibernate/JPA) | 4-6 weeks | Medium |
| Rewrite 95 services (@Service) | 6-10 weeks | Medium-High |
| Rewrite 91 controllers (@RestController) | 4-6 weeks | Medium |
| Rewrite 39 provider integrations | 4-6 weeks | Medium-High |
| Rewrite factory pattern (Spring Profiles/Beans) | 2-3 weeks | Medium |
| Rewrite auth (Spring Security) | 3-4 weeks | **High** |
| Rewrite SignalR → Spring WebSocket/STOMP | 2-3 weeks | Medium |
| Rewrite middleware → Filters/Interceptors | 1-2 weeks | Low |
| Rewrite/adapt 5,160 tests (JUnit/Mockito) | 6-10 weeks | **High** |
| Rebuild Dockerfiles | 1 week | Low |
| Rebuild CI/CD pipeline | 1 week | Low |
| Integration testing & stabilization | 4-8 weeks | **High** |

**Estimated total effort: 10-15 months (2-3 developers)**

#### Structural Mapping

Java/Spring Boot is the most natural mapping from C#/.NET:

| C#/.NET Feature | Java/Spring Equivalent | Mapping Quality |
|----------------|------------------------|-----------------|
| Entity Framework Core | Hibernate/JPA | ⚠️ 80% — no multi-provider factory, different migration model |
| ASP.NET Core Controllers | Spring MVC @RestController | ✅ 95% — near-identical patterns |
| Dependency Injection | Spring IoC Container | ✅ 95% — constructor injection works same |
| LINQ | Java Streams + JPQL/Criteria API | ⚠️ 70% — more verbose, less powerful |
| async/await | CompletableFuture / Virtual Threads (Java 21+) | ⚠️ 75% — different model |
| Middleware pipeline | Spring Filters/Interceptors | ✅ 90% |
| SignalR | Spring WebSocket + STOMP | ⚠️ 70% — different protocol |
| Feature flags | Togglz / FF4J | ⚠️ 80% — different API |
| Generics | Java Generics | ✅ 90% — type erasure is only difference |
| Nullable types | Optional + @Nullable | ⚠️ 75% — less integrated |
| BaseEntity inheritance | @MappedSuperclass | ✅ 95% |
| BCrypt | Spring Security BCrypt | ✅ 95% |
| Redis caching | Spring Data Redis | ✅ 90% |
| Swagger/OpenAPI | springdoc-openapi | ✅ 95% |

#### What Java Does Well for This Project

| Strength | Relevance |
|----------|-----------|
| Mature enterprise ecosystem | **High** — Hibernate, Spring Security are battle-tested |
| JPA/Hibernate ORM | **High** — closest equivalent to EF Core |
| Spring DI container | **High** — equivalent to .NET DI |
| Large talent pool | **High** — easier to hire |
| Long-term LTS (8+ years) | **Medium** — longer than .NET's 3 years |

#### What Java Does Poorly for This Project

| Weakness | Impact |
|----------|--------|
| Verbosity | **Medium** — Java requires ~30-40% more code than C# for equivalent logic |
| Multi-DB provider support | **High** — Hibernate supports many DBs but no factory pattern for runtime switching |
| Cold start time | **Medium** — JVM startup slower than .NET (mitigated by GraalVM native image) |
| Memory footprint | **Medium** — JVM heap typically 2-3x .NET for same workload |
| No `record` types (until Java 16) | **Low** — Java 21+ has records |
| async model | **Medium** — Virtual Threads (Java 21) help but ecosystem not fully adapted |

---

## 3. Comparative Impact Analysis

### 3.1 Effort Comparison

| Dimension | .NET 10 | Go | Java |
|-----------|---------|-----|------|
| **Calendar time** | 3-5 days | 12-18 months | 10-15 months |
| **Developer-months** | 0.2 | 24-54 | 20-45 |
| **Files modified** | ~15 (.csproj + Dockerfiles) | 674+ (everything) | 674+ (everything) |
| **Lines rewritten** | <100 | ~400,000 | ~520,000 (more verbose) |
| **Tests rewritten** | 0 (re-run only) | ~152,000 lines | ~152,000 lines |
| **API contract changes** | 0 | Risk of subtle differences | Risk of subtle differences |
| **Frontend changes** | 0 | 0-minimal (same REST) | 0-minimal (same REST) |
| **DB schema changes** | 0 | Likely (ORM differences) | Possible (Hibernate quirks) |
| **Deployment changes** | Base image tag only | Complete rebuild | Complete rebuild |

### 3.2 Risk Comparison

| Risk Category | .NET 10 | Go | Java |
|---------------|---------|-----|------|
| **Feature parity loss** | None | **HIGH** — ORM, DI, SignalR gaps | Medium — most features map |
| **Regression bugs** | Very Low | **VERY HIGH** — full rewrite | **HIGH** — full rewrite |
| **Team ramp-up** | None (same language) | **HIGH** — new language + idioms | Medium — similar concepts |
| **Vendor lock-in** | Microsoft (.NET Foundation OSS) | None | Oracle (OpenJDK mitigates) |
| **Ecosystem maturity** | Excellent for this workload | Weak for enterprise CRUD | Excellent |
| **Production stabilization** | 1 day | 2-4 months | 1-3 months |
| **Lost development velocity** | None | 12-18 months of zero features | 10-15 months of zero features |

### 3.3 Runtime Performance Comparison

| Metric | .NET 10 | Go 1.23 | Java 21 (Spring Boot 3) |
|--------|---------|---------|-------------------------|
| Cold start | ~800ms | ~100ms | ~2,500ms (JVM), ~200ms (GraalVM) |
| Throughput (req/s) | ~320K (TechEmpower) | ~290K | ~200K |
| Memory (idle) | ~40MB | ~15MB | ~120MB |
| Memory (loaded) | ~200MB | ~80MB | ~400MB |
| Docker image size | ~220MB | ~20MB | ~350MB (JRE), ~80MB (GraalVM) |
| P99 latency | ~2ms | ~1ms | ~5ms |

*Benchmarks are illustrative for a typical CRUD API workload. Actual results vary.*

> **Note:** For a database-bound CRM application handling typical enterprise loads (100-1,000 concurrent users), the performance differences between these runtimes are **negligible**. The bottleneck is database I/O and network latency, not CPU throughput.

### 3.4 Total Cost of Ownership (3-Year Projection)

| Cost Factor | .NET 10 | Go Rewrite | Java Rewrite |
|-------------|---------|------------|--------------|
| **Migration development** | ~$5K (1 week) | ~$300K-500K (12-18 mo) | ~$250K-400K (10-15 mo) |
| **Feature development freeze** | 0 days | 12-18 months | 10-15 months |
| **Opportunity cost of freeze** | $0 | $500K-1M+ | $400K-800K+ |
| **Re-training** | $0 | $20K-50K | $10K-30K |
| **Regression fixing** | ~$1K | $50K-100K | $40K-80K |
| **Infrastructure** | Same | 20-40% lower compute | 30-50% higher memory |
| **Hiring (talent pool)** | Good | Smaller pool | Largest pool |
| **3-year total** | **~$6K** | **$870K-1.65M** | **$700K-1.31M** |

### 3.5 Codebase-Specific Impact

#### 3.5.1 The ORM Problem (Critical for Go)

The `CrmDbContext` is 3,064 lines with:
- 200+ `DbSet<T>` declarations
- 2,651 lines of Fluent API in `OnModelCreating()`
- Global soft-delete query filters
- TPH inheritance mapping (Account/Customer)
- Multi-provider strategy factory (6 databases)
- Polymorphic junction tables with `EntityType` discriminators
- Optimistic concurrency via `RowVersion`
- Row-size limit workarounds for MariaDB

In Go, **there is no equivalent**. GORM covers ~40% of this. The remaining 60% would require:
- Hand-written SQL for each of 6 database dialects
- Manual change tracking
- Custom migration tooling
- Manual soft-delete filtering on every query
- No lazy loading — explicit joins everywhere

In Java, Hibernate covers ~80% but still requires:
- Custom `@Filter` annotations for soft delete (less elegant than EF global filters)
- Different migration tool (Flyway/Liquibase instead of EF migrations)
- Manual multi-provider configuration (no runtime factory switching)

#### 3.5.2 The Pluggable Architecture (ADR-001)

The hexagonal architecture with 16 providers, 8 factories, and 7 port interfaces is:
- **Trivial to preserve in .NET 10** — zero changes needed
- **Possible in Java** — Spring Profiles + `@Qualifier` can approximate the factory pattern
- **Painful in Go** — no DI container, no interface-based injection, factories must be hand-wired

#### 3.5.3 The SignalR Problem

SignalR provides:
- WebSocket with automatic fallback (Long Polling, Server-Sent Events)
- Hub method invocation with strongly-typed clients
- Group management (per-entity subscriptions)
- Connection lifecycle management
- Backplane support (Redis) for multi-server

In Go: Must implement raw WebSocket server. No equivalent to hub pattern.
In Java: Spring WebSocket + STOMP provides ~70% of functionality but different protocol.

#### 3.5.4 The Test Suite Problem

5,160+ tests using xUnit, Moq, and EF Core InMemory provider:
- **Go**: No Moq equivalent. Must rewrite all mocks as manual implementations. No InMemory DB provider — must use test containers or SQLite.
- **Java**: JUnit + Mockito provide equivalent patterns. H2 in-memory DB replaces EF InMemory. Migration is mechanical but time-consuming.

---

## 4. Decision

### Recommended: Option A — Upgrade to .NET 10

The decision is **overwhelmingly clear** based on the evidence:

| Decision Criterion | Winner | Margin |
|-------------------|--------|--------|
| Migration effort | .NET 10 | 100:1 vs Go/Java |
| Risk | .NET 10 | Negligible vs High/Very High |
| Feature velocity impact | .NET 10 | 0 days lost vs 10-18 months |
| Cost | .NET 10 | $6K vs $700K-1.6M |
| Performance | Tie | Negligible for this workload |
| Team continuity | .NET 10 | No ramp-up needed |
| Ecosystem fit | .NET 10 | Already using it, proven |
| LTS support | .NET 10 | 3 years (adequate) |

### Decision Rationale

1. **The codebase is 400K+ lines of well-structured C#.** Rewriting it gains nothing — it's not legacy code, it's 3-month-old production code using current patterns.

2. **.NET 10 is already released and LTS.** The upgrade is a version bump, not a migration.

3. **.NET 8 end-of-support (Nov 2026) creates urgency** — but only for a version bump, not a platform change.

4. **Go is architecturally unsuitable** for this workload. Enterprise CRUD applications with complex ORM requirements, 171 database tables, and pluggable provider architectures are not Go's strength. Go excels at infrastructure tooling, network services, and CLI tools — not business applications with rich domain models.

5. **Java/Spring Boot is technically viable** but offers no advantage over .NET for this codebase. The 1:1 concept mapping means the rewrite would produce nearly identical architecture in a more verbose language, at a cost of 10-15 months and $700K+.

6. **A rewrite to any platform carries the "second-system effect" risk** — the temptation to redesign everything leads to scope creep and the new system never reaching parity with the old one.

---

## 5. Implementation Plan (.NET 10 Upgrade)

### Phase 1: Package Update (Day 1)

```bash
# Update global.json (if exists)
# Update Directory.Build.props
# Update all .csproj files: net8.0 → net10.0
# Update all NuGet packages to latest compatible versions
```

**Files to modify:**

| File | Change |
|------|--------|
| `CRM.Backend/Directory.Build.props` | TargetFramework → net10.0 |
| `CRM.Backend/src/CRM.Api/CRM.Api.csproj` | Package versions |
| `CRM.Backend/src/CRM.Core/CRM.Core.csproj` | Package versions |
| `CRM.Backend/src/CRM.Infrastructure/CRM.Infrastructure.csproj` | Package versions |
| `CRM.Backend/src/Services/*/` (8 projects) | TargetFramework → net10.0 |
| `CRM.Backend/tests/CRM.Tests.csproj` | TargetFramework + test package versions |

### Phase 2: Build & Fix (Day 1-2)

```bash
cd CRM.Backend && dotnet build 2>&1 | grep "error CS"
# Fix any breaking API changes (typically <20 call sites)
```

Common .NET 8 → 10 breaking changes to check:
- Obsolete API removals
- Default serialization behavior changes
- Auth middleware ordering changes
- EF Core query translation changes

### Phase 3: Test (Day 2-3)

```bash
cd CRM.Backend && dotnet test
# Expect: 5,160+ tests pass
# Fix any failures caused by runtime behavior changes
```

### Phase 4: Docker & CI (Day 3)

```bash
# Update all 10 Dockerfiles:
# FROM mcr.microsoft.com/dotnet/aspnet:8.0 → 10.0
# FROM mcr.microsoft.com/dotnet/sdk:8.0 → 10.0
```

### Phase 5: Deploy & Verify (Day 4-5)

```bash
# Build cross-platform image
docker buildx build --platform linux/amd64 -t crm-api:latest -f docker/Dockerfile.backend .

# Deploy to test server
# Run BVT: 118/118 passing
# Run E2E suite
```

---

## 6. Consequences

### Positive

- **.NET 8 EOL risk eliminated** — 3 years of LTS support secured
- **Zero feature development freeze** — upgrade takes days, not months
- **Performance improvements for free** — runtime optimizations, GC improvements
- **New language features available** — C# 14 (field keyword, extension members)
- **EF Core 10 improvements** — better query translation, JSON column support
- **Team morale preserved** — no demoralizing rewrite-without-new-features phase

### Negative

- **Microsoft ecosystem dependency continues** — mitigated by .NET being fully open-source (.NET Foundation, MIT license)
- **Next upgrade (to .NET 12 or 14) required in ~3 years** — but will also be a minor version bump

### Neutral

- **Hiring pool unchanged** — C#/.NET developers remain available
- **Architecture unchanged** — hexagonal/ports-adapters pattern carries forward
- **All documentation remains valid** — no rewrites needed

---

## 7. When Would a Platform Change Be Justified?

For completeness, scenarios where Go or Java would merit serious consideration:

| Scenario | Recommended Platform |
|----------|---------------------|
| Greenfield microservice (new, small, network-heavy) | Go |
| Organization standardized on JVM | Java |
| Need for 20+ year LTS (regulated industry) | Java (Oracle/Red Hat support) |
| Extreme cold-start requirements (serverless) | Go or .NET Native AOT |
| Team has zero .NET experience | Java (closest conceptual match) |
| Existing project with 400K lines of working C# | **.NET (upgrade in place)** |

---

## 8. Appendix: Package Upgrade Reference

### Critical Packages to Update

| Package | Current | Target (.NET 10) | Breaking Changes |
|---------|---------|-------------------|-----------------|
| Microsoft.EntityFrameworkCore | 8.0.11 | 10.0.x | Check: query translation, value converters |
| Pomelo.EntityFrameworkCore.MySql | 8.0.0 | 10.0.x | Check: MariaDB compatibility |
| Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.11 | 10.0.x | Usually non-breaking |
| Oracle.EntityFrameworkCore | 8.21.121 | 10.x | Check Oracle release notes |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | 10.0.x | Check auth changes |
| Serilog.AspNetCore | 8.0.0 | 9.x+ | Usually non-breaking |
| Swashbuckle.AspNetCore | 6.5.0 | 7.x+ or switch to Microsoft.AspNetCore.OpenApi | Check: ASP.NET Core 10 native OpenAPI |
| StackExchange.Redis | 2.7.33 | 2.8.x+ | Usually non-breaking |
| Polly | 8.2.0 | 8.5.x+ | Non-breaking within 8.x |

### Packages That May Need Attention

| Package | Notes |
|---------|-------|
| `AspNetCoreRateLimit 5.0.0` | Consider migrating to built-in `System.Threading.RateLimiting` (available since .NET 7) |
| `Microsoft.AspNetCore.Cors 2.2.0` | **Outdated** — CORS is built into ASP.NET Core 8+. Remove this package. |
| `Swashbuckle.AspNetCore 6.5.0` | ASP.NET Core 10 has built-in OpenAPI — consider migration |

---

**END OF ADR-003**
