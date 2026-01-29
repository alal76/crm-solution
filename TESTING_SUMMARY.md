# CRM Solution - Testing Summary

**Version:** 0.0.25  
**Last Updated:** January 2025

---

## Overview

The CRM Solution includes comprehensive testing at multiple levels:

| Test Type | Location | Framework |
|-----------|----------|-----------|
| **Backend Unit Tests** | `CRM.Backend/tests/` | xUnit, Moq, FluentAssertions |
| **Frontend Unit Tests** | `CRM.Frontend/src/__tests__/` | Jest, React Testing Library |
| **E2E Tests** | `e2e-tests/` | Playwright |

---

## Backend Tests

### Test Statistics
- **Total Tests:** 700+
- **Framework:** xUnit 2.6.2
- **Mocking:** Moq 4.20.70
- **Assertions:** FluentAssertions 6.12.0

### Test Categories

| Category | Tests | Description |
|----------|-------|-------------|
| **BVT** | ~95 | Build Verification Tests - critical path validation |
| **Entity** | ~80 | Core entity validation and business logic |
| **DTO** | ~40 | Data transfer object mapping tests |
| **Enum** | ~50 | Enum value and type tests |
| **Business Logic** | ~60 | Calculations and computed properties |
| **Contact Model** | ~30 | Contact entity model tests |
| **Utility** | ~45 | Helper functions and utilities |
| **Service** | ~100 | Service layer unit tests |
| **Controller** | ~50 | Controller layer tests |
| **Functional** | ~35 | API endpoint integration tests |

### Test Directory Structure

```
CRM.Backend/tests/
├── CRM.Tests.csproj           # Test project file
├── BVT/
│   └── CriticalPathBVTTests.cs
├── BusinessLogic/
│   └── BusinessLogicTests.cs
├── Controllers/
│   ├── CustomersControllerTests.cs
│   ├── DepartmentsControllerTests.cs
│   ├── OpportunitiesControllerTests.cs
│   └── ProductsControllerTests.cs
├── Dtos/
│   └── DtoMappingTests.cs
├── Entities/
│   ├── CoreEntityTests.cs
│   ├── EntityValidationTests.cs
│   └── EnumTypeTests.cs
├── Functional/
│   ├── ApiEndpointFunctionalTests.cs
│   └── FunctionalTestBase.cs
├── Models/
│   └── ContactModelTests.cs
├── Services/
│   ├── AccountServiceTests.cs
│   ├── AuthenticationServiceTests.cs
│   ├── CustomerServiceTests.cs
│   ├── LeadServiceTests.cs
│   ├── OpportunityServiceTests.cs
│   ├── ProductServiceTests.cs
│   ├── SystemSettingsServiceTests.cs
│   └── UserServiceTests.cs
├── Performance/
│   └── PerformanceTests.cs
└── Utilities/
    └── UtilityTests.cs
```

### Running Backend Tests

```bash
# Run all tests
cd CRM.Backend/tests
dotnet test

# Run with verbose output
dotnet test -v normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific category
dotnet test --filter "FullyQualifiedName~BVT"
dotnet test --filter "FullyQualifiedName~BusinessLogic"
dotnet test --filter "FullyQualifiedName~Controllers"

# Run specific test file
dotnet test --filter "FullyQualifiedName~CriticalPathBVTTests"

# Run with timeout
dotnet test --blame-hang-timeout 60s
```

---

## Frontend Tests

### Test Configuration
- **Framework:** Jest
- **Config:** `CRM.Frontend/jest.config.json`
- **Setup:** `CRM.Frontend/jest.setup.js`

### Running Frontend Tests

```bash
cd CRM.Frontend

# Run all tests
npm test

# Run with coverage
npm test -- --coverage

# Run specific test file
npm test -- src/__tests__/components/CustomerForm.test.tsx

# Run in watch mode
npm test -- --watch
```

---

## End-to-End Tests (Playwright)

### Test Structure

```
e2e-tests/
├── playwright.config.ts      # Playwright configuration
├── package.json
├── tests/
│   ├── bvt/                  # Build Verification Tests
│   │   └── critical-path.spec.ts
│   ├── functional/           # Functional Tests
│   │   ├── customers.spec.ts
│   │   ├── contacts.spec.ts
│   │   ├── opportunities.spec.ts
│   │   ├── accounts.spec.ts
│   │   └── settings.spec.ts
│   └── data/                 # Data Setup Tests
│       ├── create-microsoft-ui.spec.ts
│       └── seed-data.spec.ts
├── fixtures/                 # Test fixtures
├── page-objects/             # Page Object Models
└── test-results/             # Test artifacts
```

### Test Categories

| Category | Description |
|----------|-------------|
| **BVT** | Critical path tests - login, navigation, CRUD |
| **Functional** | Feature-specific tests for each module |
| **Data** | Data creation and verification tests |

### Running E2E Tests

```bash
cd e2e-tests

# Install dependencies
npm install

# Run all tests
npx playwright test

# Run specific test file
npx playwright test tests/bvt/critical-path.spec.ts

# Run with UI mode
npx playwright test --ui

# Run headed (visible browser)
npx playwright test --headed

# Run specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# Run with trace (for debugging)
npx playwright test --trace on
```

### E2E Test Configuration

```typescript
// playwright.config.ts
export default defineConfig({
  testDir: './tests',
  timeout: 60000,
  retries: 1,
  workers: 1,
  use: {
    baseURL: 'http://192.168.0.9',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  ],
});
```

---

## Test Categories Explained

### BVT (Build Verification Tests)
Critical path tests that must pass before any deployment:
- User authentication (login/logout)
- Customer CRUD operations
- Contact management
- Opportunity pipeline
- Product catalog
- System settings access

### Entity Tests
Validation of core domain entities:
- Customer entity with all field types
- Lead scoring and lifecycle
- Opportunity weighted amounts
- Account revenue calculations
- Product pricing and billing

### DTO Mapping Tests
Data transfer object tests:
- CustomerDto ↔ Customer mapping
- UserDto ↔ User mapping
- ContactDto ↔ Contact mapping
- Request/Response DTO validation

### Business Logic Tests
Business rule validation:
- Opportunity weighted amount calculations
- Lead scoring and qualification
- Pipeline value calculations
- Revenue projections
- Contract duration calculations

### Functional Tests
API endpoint integration:
- Authentication endpoints
- Customer API endpoints
- Product API endpoints
- Settings API endpoints

**Note:** Functional tests require a running server with authentication.

---

## Test Data

### Seed Data (Production Server: 192.168.0.9)

| Entity | Count | Notes |
|--------|-------|-------|
| Customers | 53 | Including Microsoft Corporation |
| Contacts | 105 | Including executive profiles |
| Products | 12 | Enterprise and SMB products |
| Accounts | 25 | Active accounts |
| Opportunities | 20 | Various pipeline stages |
| Marketing Campaigns | 5 | Active campaigns |
| Leads | 10 | Lead generation data |
| Service Requests | 10 | Support tickets |
| User Groups | 5 | Permission groups |
| Users | 1 | Admin user |

### Creating Test Data

```bash
# Run data seed tests
cd e2e-tests
npx playwright test tests/data/seed-data.spec.ts

# Create Microsoft account with executives
npx playwright test tests/data/create-microsoft-ui.spec.ts
```

---

## Code Coverage

### Backend Coverage
```bash
cd CRM.Backend/tests
dotnet test --collect:"XPlat Code Coverage"

# View coverage report
# Coverage data in TestResults/*/coverage.cobertura.xml
```

### Frontend Coverage
```bash
cd CRM.Frontend
npm test -- --coverage

# Coverage report in coverage/lcov-report/index.html
```

---

## CI/CD Integration

### GitHub Actions (Example)
```yaml
name: Tests

on: [push, pull_request]

jobs:
  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Run tests
        run: |
          cd CRM.Backend/tests
          dotnet test --logger "trx;LogFileName=test-results.trx"
      - uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: CRM.Backend/tests/TestResults/

  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - name: Run tests
        run: |
          cd CRM.Frontend
          npm ci
          npm test -- --coverage

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      - name: Install Playwright
        run: |
          cd e2e-tests
          npm ci
          npx playwright install --with-deps
      - name: Run tests
        run: |
          cd e2e-tests
          npx playwright test
```

---

## Writing New Tests

### Backend Test Template

```csharp
using Xunit;
using FluentAssertions;
using Moq;

namespace CRM.Tests.Services
{
    public class MyServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly MyService _service;

        public MyServiceTests()
        {
            _repositoryMock = new Mock<IRepository>();
            _service = new MyService(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsEntity()
        {
            // Arrange
            var expected = new Entity { Id = 1, Name = "Test" };
            _repositoryMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task GetById_InvalidId_ThrowsException(int id)
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.GetByIdAsync(id));
        }
    }
}
```

### E2E Test Template

```typescript
import { test, expect } from '@playwright/test';

test.describe('Customer Management', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.fill('[name="email"]', 'admin@crm.com');
    await page.fill('[name="password"]', 'password');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/dashboard');
  });

  test('should create new customer', async ({ page }) => {
    await page.goto('/customers');
    await page.click('button:has-text("Add Customer")');
    
    await page.fill('[name="name"]', 'Test Company');
    await page.fill('[name="email"]', 'test@company.com');
    await page.click('button[type="submit"]');
    
    await expect(page.locator('.success-message')).toBeVisible();
  });
});
```

---

## Troubleshooting

### Backend Tests Failing
```bash
# Check for missing dependencies
dotnet restore CRM.Backend/tests/CRM.Tests.csproj

# Check test output
dotnet test -v detailed
```

### E2E Tests Failing
```bash
# Update Playwright browsers
npx playwright install

# Run with debug mode
DEBUG=pw:api npx playwright test

# Check trace files
npx playwright show-trace test-results/*/trace.zip
```

### Timeout Issues
```bash
# Increase timeout for slow tests
dotnet test --blame-hang-timeout 120s

# For Playwright
npx playwright test --timeout 120000
```

---

## Related Documentation

- [README.md](README.md) - Project overview
- [ARCHITECTURE_OVERVIEW.md](ARCHITECTURE_OVERVIEW.md) - System architecture
- [docs/testing/](docs/testing/) - Detailed testing guides
- [e2e-tests/README.md](e2e-tests/README.md) - E2E test documentation
