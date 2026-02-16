# CRM Solution - Integration Testing Guide

> **Last Updated:** February 11, 2026  
> **Test Status:** 5,000+ active tests, 118/118 BVT passing (100%)

---

## Testing Pyramid

The CRM solution follows a three-tier testing strategy:

```
           ┌───────────┐
           │   E2E     │  ~722 Playwright tests (browser + API)
           │  Tests    │  Slowest, highest confidence
          ┌┴───────────┴┐
          │ Integration  │  BVT (118 tests) + Controller tests
          │   Tests      │  API-level, no browser
         ┌┴──────────────┴┐
         │   Unit Tests    │  ~5,000 xUnit tests (services, providers, factories)
         │                 │  Fastest, most granular
         └─────────────────┘
```

| Layer | Framework | Count | Speed | Location |
|-------|-----------|-------|-------|----------|
| Unit | xUnit + Moq | ~5,000 | Fast (~30s) | `CRM.Backend/tests/` |
| BVT | Playwright (API) | 118 | Medium (~20s) | `e2e-tests/tests/bvt/` |
| E2E | Playwright (browser) | ~722 | Slow (~5min) | `e2e-tests/tests/` |

---

## 1. Running Unit Tests

Unit tests cover backend services, providers, and factories.

```bash
# Run all unit tests
cd CRM.Backend && dotnet test

# Run a specific test project
cd CRM.Backend && dotnet test tests/CRM.Tests

# Run tests matching a filter
cd CRM.Backend && dotnet test --filter "FullyQualifiedName~AccountServiceTests"

# Run with verbose output
cd CRM.Backend && dotnet test --verbosity normal

# Run with code coverage
cd CRM.Backend && dotnet test --collect:"XPlat Code Coverage"
```

### Test Projects

| Project | Tests | Description |
|---------|-------|-------------|
| `CRM.Tests` | ~1,686 | Core service + controller tests |
| `CRM.ProviderTests` | ~460 | Pluggable provider tests (Search, Chat, AI, etc.) |
| `CRM.IntegrationTests` | ~2,854 | Integration + comprehensive tests |

### Writing New Unit Tests

Follow the naming convention: `{Method}_Should{ExpectedBehavior}_When{Condition}`

```csharp
[Fact]
public async Task GetById_ShouldReturnAccount_WhenAccountExists()
{
    // Arrange - use MockDbSetFactory from AsyncQueryTestHelpers
    var accounts = new List<Account> { new Account { Id = 1, Company = "Test" } };
    var mockDbSet = MockDbSetFactory.Create(accounts);
    _mockContext.Setup(c => c.Accounts).Returns(mockDbSet.Object);

    // Act
    var result = await _service.GetByIdAsync(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Company);
}
```

### Shared Test Infrastructure

The `AsyncQueryTestHelpers.cs` file provides:
- `MockDbSetFactory.Create<T>()` — Creates mock DbSets with EF Core async support
- Automatic `FindAsync` support with EF convention PK detection
- `Add`/`AddAsync` tracking
- `IAsyncEnumerable` support for LINQ queries

Location: `CRM.Backend/tests/CRM.Tests/Helpers/AsyncQueryTestHelpers.cs`

---

## 2. Running BVT (Build Verification Tests)

BVT tests validate that all API endpoints are reachable and return expected status codes. They run against a live server — no browser needed.

```bash
cd e2e-tests

# Run BVT against local server
npx playwright test tests/bvt/api-bvt.spec.ts

# Run BVT against development server
BASE_URL=http://192.168.0.9 npx playwright test tests/bvt/api-bvt.spec.ts

# Run with custom config
npx playwright test --config=playwright.bvt.config.ts
```

### BVT Status

- **Current:** 118/118 passing (100%)
- **Coverage:** All ITSM, Sales, CRM, Admin, and AI endpoints

---

## 3. Running E2E Tests

E2E tests use Playwright to drive a real browser against the full application stack.

```bash
cd e2e-tests

# Install browsers (first time only)
npx playwright install

# Run all E2E tests (chromium)
BASE_URL=http://192.168.0.9 npx playwright test --project=chromium

# Run a specific test file
BASE_URL=http://192.168.0.9 npx playwright test tests/auth/authentication.spec.ts

# Run with headed browser (for debugging)
BASE_URL=http://192.168.0.9 npx playwright test --headed

# Run with Playwright UI mode
BASE_URL=http://192.168.0.9 npx playwright test --ui

# Generate HTML report
npx playwright show-report
```

---

## 4. Test Credentials

| Role | Email | Password | Notes |
|------|-------|----------|-------|
| Admin | `admin@crm.local` | `Admin@123` | Seeded on first startup |

These credentials are defined in `e2e-tests/tests/test-data.ts`.

---

## 5. Known Test Status

| Metric | Value |
|--------|-------|
| Active unit tests | ~5,000 across 3 projects |
| BVT tests | 118/118 passing (100%) |
| Pre-existing unit test failures | ~95 (entity property drift in older tests) |
| Excluded test files | ~97 (in `CRM.Tests.csproj` via `<Compile Remove>`) |
| E2E skipped tests | ~47 (ITSM BVT + pending features) |
| Backend skipped tests | 8 (performance tests, `Skip = "run manually"`) |

### Excluded Tests

~97 test files are excluded from compilation in `CRM.Tests.csproj` due to entity property drift (entity classes were updated but test mocks were not). These need `MockDbSetFactory` updates to match current entity shapes.

---

## 6. Code Coverage

```bash
# Collect coverage
cd CRM.Backend && dotnet test --collect:"XPlat Code Coverage"

# Coverage reports are generated in:
# tests/*/TestResults/*/coverage.cobertura.xml

# To generate an HTML report, install ReportGenerator:
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator \
  -reports:"tests/*/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html
```

### Current Coverage

| Category | Tested | Total | Coverage |
|----------|--------|-------|----------|
| Services | 40 | 125 | ~32% |
| Controllers | 14 | 94 | ~15% |
| Providers | 20+ | 30+ | ~67% |
| Frontend (Jest) | 0 | — | 0% |

---

## 7. CI/CD Test Integration

Tests run automatically in GitHub Actions (`.github/workflows/ci-cd.yml`):

1. **Backend Tests** — `dotnet test` on every push
2. **Frontend Build** — `npm run build` on every push
3. **BVT** — Runs against deployed environment after Docker build

---

## Related Documentation

- [TESTING_SUMMARY.md](docs/test/TESTING_SUMMARY.md) — High-level testing overview
- [SOLUTION_CONTEXT.md](docs/development/SOLUTION_CONTEXT.md) — Solution architecture
- [SOLUTION_GAPS_REMEDIATION_PLAN.md](docs/development/SOLUTION_GAPS_REMEDIATION_PLAN.md) — Test coverage gaps (Phase 10.10, 11.11)

---

**END OF INTEGRATION TESTING GUIDE**
