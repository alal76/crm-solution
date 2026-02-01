# Testing Documentation

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

Comprehensive testing strategy, test patterns, and quality assurance processes for the CRM solution.

---

## Table of Contents

1. [Testing Strategy](#1-testing-strategy)
2. [Unit Testing](#2-unit-testing)
3. [Integration Testing](#3-integration-testing)
4. [End-to-End Testing](#4-end-to-end-testing)
5. [Test Data Management](#5-test-data-management)
6. [Running Tests](#6-running-tests)

---

## 1. Testing Strategy

### 1.1 Test Pyramid

```
                          ┌─────────────┐
                         ╱             ╲
                        ╱     E2E       ╲          ~ 10%
                       ╱    (Playwright) ╲         Slow, Expensive
                      ╱─────────────────────╲
                     ╱                       ╲
                    ╱      Integration        ╲     ~ 20%
                   ╱     (API + Database)      ╲    Medium Speed
                  ╱─────────────────────────────╲
                 ╱                               ╲
                ╱           Unit Tests            ╲  ~ 70%
               ╱      (Services, Utilities)        ╲ Fast, Cheap
              └─────────────────────────────────────┘
```

### 1.2 Coverage Goals

| Test Type | Target Coverage | Current Status |
|-----------|-----------------|----------------|
| Unit Tests | 80% | ✅ Achieved |
| Integration Tests | 60% | ✅ Achieved |
| E2E Critical Paths | 100% | ✅ Achieved |

### 1.3 Testing Technologies

| Layer | Technology | Location |
|-------|------------|----------|
| Backend Unit | xUnit + Moq | `CRM.Backend/tests/CRM.UnitTests/` |
| Backend Integration | xUnit + TestContainers | `CRM.Backend/tests/CRM.IntegrationTests/` |
| Frontend Unit | Jest + React Testing Library | `CRM.Frontend/src/**/*.test.tsx` |
| E2E | Playwright | `e2e-tests/tests/` |

---

## 2. Unit Testing

### 2.1 Backend Unit Tests

#### Test Structure

```csharp
namespace CRM.UnitTests.Services
{
    public class CustomerServiceTests
    {
        private readonly Mock<ICustomerRepository> _repositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _repositoryMock = new Mock<ICustomerRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _service = new CustomerService(
                _repositoryMock.Object, 
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task GetById_ExistingCustomer_ReturnsCustomer()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { Id = customerId, Name = "Test" };
            _repositoryMock.Setup(r => r.GetByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _service.GetByIdAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.Id);
        }

        [Fact]
        public async Task GetById_NonExistingCustomer_ReturnsNull()
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((Customer)null);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }
    }
}
```

#### Naming Convention

```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `CreateCustomer_ValidData_ReturnsCreatedCustomer`
- `CreateCustomer_DuplicateEmail_ThrowsValidationException`
- `DeleteCustomer_HasRelatedOrders_ThrowsBusinessException`

### 2.2 Frontend Unit Tests

#### Component Test Structure

```typescript
// CustomerForm.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CustomerForm } from './CustomerForm';

describe('CustomerForm', () => {
  const mockOnSubmit = jest.fn();
  
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('renders all required fields', () => {
    render(<CustomerForm onSubmit={mockOnSubmit} />);
    
    expect(screen.getByLabelText(/company name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
  });

  it('shows validation error for empty required field', async () => {
    render(<CustomerForm onSubmit={mockOnSubmit} />);
    
    await userEvent.click(screen.getByRole('button', { name: /save/i }));
    
    expect(await screen.findByText(/company name is required/i)).toBeInTheDocument();
  });

  it('calls onSubmit with form data when valid', async () => {
    render(<CustomerForm onSubmit={mockOnSubmit} />);
    
    await userEvent.type(screen.getByLabelText(/company name/i), 'Acme Corp');
    await userEvent.type(screen.getByLabelText(/email/i), 'contact@acme.com');
    await userEvent.click(screen.getByRole('button', { name: /save/i }));
    
    await waitFor(() => {
      expect(mockOnSubmit).toHaveBeenCalledWith({
        companyName: 'Acme Corp',
        email: 'contact@acme.com',
      });
    });
  });
});
```

#### Service Test Structure

```typescript
// customerService.test.ts
import { customerService } from './customerService';
import { api } from './api';

jest.mock('./api');
const mockApi = api as jest.Mocked<typeof api>;

describe('customerService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getAll', () => {
    it('returns customers list', async () => {
      const mockCustomers = [
        { id: 1, name: 'Customer 1' },
        { id: 2, name: 'Customer 2' },
      ];
      mockApi.get.mockResolvedValue({ data: mockCustomers });

      const result = await customerService.getAll();

      expect(mockApi.get).toHaveBeenCalledWith('/api/accounts');
      expect(result).toEqual(mockCustomers);
    });
  });
});
```

---

## 3. Integration Testing

### 3.1 Backend Integration Tests

#### API Integration Test

```csharp
public class AccountsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AccountsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace database with in-memory for testing
                services.RemoveAll(typeof(DbContextOptions<CrmDbContext>));
                services.AddDbContext<CrmDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAccounts_ReturnsSuccessStatusCode()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/accounts");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateAccount_ValidData_ReturnsCreatedAccount()
    {
        // Arrange
        await AuthenticateAsync();
        var account = new CreateAccountRequest
        {
            CompanyName = "Test Company",
            Email = "test@company.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/accounts", account);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<AccountDto>();
        Assert.Equal("Test Company", created.CompanyName);
    }

    private async Task AuthenticateAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "admin123"
        });
        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", auth.Token);
    }
}
```

### 3.2 Database Integration Tests

```csharp
public class CustomerRepositoryIntegrationTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly CustomerRepository _repository;

    public CustomerRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new CrmDbContext(options);
        _repository = new CustomerRepository(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        _context.Accounts.AddRange(
            new Account { Id = 1, CompanyName = "Acme Corp" },
            new Account { Id = 2, CompanyName = "Test Inc" }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomer()
    {
        var result = await _repository.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Acme Corp", result.CompanyName);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

---

## 4. End-to-End Testing

### 4.1 Playwright Setup

**Location:** `e2e-tests/`

```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
    { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  ],
  webServer: {
    command: 'npm run start',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
  },
});
```

### 4.2 E2E Test Structure

```typescript
// tests/customers/customer-crud.spec.ts
import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { CustomersPage } from '../pages/CustomersPage';

test.describe('Customer CRUD Operations', () => {
  let loginPage: LoginPage;
  let customersPage: CustomersPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    customersPage = new CustomersPage(page);
    
    await loginPage.goto();
    await loginPage.login('admin', 'admin123');
  });

  test('should create a new customer', async ({ page }) => {
    await customersPage.goto();
    await customersPage.clickCreateNew();
    
    await customersPage.fillForm({
      companyName: 'E2E Test Company',
      email: 'e2e@test.com',
      industry: 'Technology',
    });
    
    await customersPage.submitForm();
    
    await expect(page.getByText('Customer created successfully')).toBeVisible();
    await expect(page.getByText('E2E Test Company')).toBeVisible();
  });

  test('should edit an existing customer', async ({ page }) => {
    await customersPage.goto();
    await customersPage.openCustomer('E2E Test Company');
    await customersPage.clickEdit();
    
    await customersPage.fillForm({
      companyName: 'E2E Test Company Updated',
    });
    
    await customersPage.submitForm();
    
    await expect(page.getByText('Customer updated successfully')).toBeVisible();
  });

  test('should delete a customer', async ({ page }) => {
    await customersPage.goto();
    await customersPage.openCustomer('E2E Test Company Updated');
    await customersPage.clickDelete();
    await customersPage.confirmDelete();
    
    await expect(page.getByText('Customer deleted successfully')).toBeVisible();
  });
});
```

### 4.3 Page Object Model

```typescript
// tests/pages/CustomersPage.ts
import { Page, Locator } from '@playwright/test';

export class CustomersPage {
  readonly page: Page;
  readonly createButton: Locator;
  readonly dataGrid: Locator;
  readonly searchInput: Locator;

  constructor(page: Page) {
    this.page = page;
    this.createButton = page.getByRole('button', { name: /create/i });
    this.dataGrid = page.getByRole('grid');
    this.searchInput = page.getByPlaceholder(/search/i);
  }

  async goto() {
    await this.page.goto('/customers');
    await this.page.waitForLoadState('networkidle');
  }

  async clickCreateNew() {
    await this.createButton.click();
  }

  async fillForm(data: Partial<CustomerFormData>) {
    if (data.companyName) {
      await this.page.getByLabel(/company name/i).fill(data.companyName);
    }
    if (data.email) {
      await this.page.getByLabel(/email/i).fill(data.email);
    }
    if (data.industry) {
      await this.page.getByLabel(/industry/i).click();
      await this.page.getByRole('option', { name: data.industry }).click();
    }
  }

  async submitForm() {
    await this.page.getByRole('button', { name: /save/i }).click();
  }

  async openCustomer(name: string) {
    await this.page.getByRole('row', { name }).click();
  }

  async clickEdit() {
    await this.page.getByRole('button', { name: /edit/i }).click();
  }

  async clickDelete() {
    await this.page.getByRole('button', { name: /delete/i }).click();
  }

  async confirmDelete() {
    await this.page.getByRole('button', { name: /confirm/i }).click();
  }
}
```

### 4.4 E2E Test Categories

| Category | Location | Description |
|----------|----------|-------------|
| Authentication | `tests/auth/` | Login, logout, password reset |
| Customers | `tests/customers/` | Customer CRUD operations |
| Contacts | `tests/contacts/` | Contact management |
| Leads | `tests/leads/` | Lead management, conversion |
| Opportunities | `tests/opportunities/` | Sales pipeline |
| Campaigns | `tests/campaigns/` | Marketing campaigns |
| Settings | `tests/settings/` | System configuration |

---

## 5. Test Data Management

### 5.1 Test Fixtures

```typescript
// fixtures/customers.ts
export const testCustomers = {
  basic: {
    companyName: 'Test Company',
    email: 'test@company.com',
    accountType: 'Customer',
  },
  withContacts: {
    companyName: 'Company With Contacts',
    email: 'contact@company.com',
    contacts: [
      { firstName: 'John', lastName: 'Doe', email: 'john@company.com' },
      { firstName: 'Jane', lastName: 'Smith', email: 'jane@company.com' },
    ],
  },
};
```

### 5.2 Database Seeding (Tests)

```csharp
// TestDataSeeder.cs
public static class TestDataSeeder
{
    public static void SeedTestData(CrmDbContext context)
    {
        // Users
        context.Users.AddRange(
            new User { Id = 1, Username = "admin", Role = "Admin" },
            new User { Id = 2, Username = "sales", Role = "SalesRep" }
        );

        // Customers
        context.Accounts.AddRange(
            new Account { Id = 1, CompanyName = "Acme Corp", OwnerId = 2 },
            new Account { Id = 2, CompanyName = "Test Inc", OwnerId = 2 }
        );

        context.SaveChanges();
    }
}
```

---

## 6. Running Tests

### 6.1 Backend Tests

```bash
# Run all backend tests
cd CRM.Backend
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/CRM.UnitTests/

# Run specific test class
dotnet test --filter "FullyQualifiedName~CustomerServiceTests"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### 6.2 Frontend Tests

```bash
# Run all frontend tests
cd CRM.Frontend
npm test

# Run with coverage
npm test -- --coverage

# Run specific test file
npm test -- CustomerForm.test.tsx

# Run in watch mode
npm test -- --watch

# Update snapshots
npm test -- -u
```

### 6.3 E2E Tests

```bash
# Run all E2E tests
cd e2e-tests
npm test

# Run specific test file
npx playwright test tests/customers/

# Run with UI mode
npx playwright test --ui

# Run headed (visible browser)
npx playwright test --headed

# Generate report
npx playwright show-report

# Run in Docker
./run-tests.sh
```

### 6.4 CI/CD Test Commands

```yaml
# .github/workflows/test.yml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Backend Tests
        run: |
          cd CRM.Backend
          dotnet test --collect:"XPlat Code Coverage"
      
      - name: Frontend Tests
        run: |
          cd CRM.Frontend
          npm ci
          npm test -- --coverage --watchAll=false
      
      - name: E2E Tests
        run: |
          cd e2e-tests
          npm ci
          npx playwright install --with-deps
          npx playwright test
```

---

## Test Results

See [TEST_RESULTS_SUMMARY.md](../../e2e-tests/TEST_RESULTS_SUMMARY.md) for latest E2E test results.
See [TESTING_SUMMARY.md](../../TESTING_SUMMARY.md) for overall testing status.
