# Testing Strategy & Best Practices

**Version:** 1.0  
**Last Updated:** February 2026  
**Status:** Active

## Overview

This document outlines the comprehensive testing strategy for the CRM Solution, covering unit tests, integration tests, E2E tests, and testing best practices.

---

## Table of Contents

1. [Testing Pyramid](#testing-pyramid)
2. [Frontend Testing](#frontend-testing)
3. [Backend Testing](#backend-testing)
4. [E2E Testing](#e2e-testing)
5. [Test Data Management](#test-data-management)
6. [CI/CD Integration](#cicd-integration)
7. [Coverage Targets](#coverage-targets)

---

## Testing Pyramid

```
         ╱╲
        ╱  ╲        E2E Tests (10%)
       ╱____╲       - Full user flows
      ╱      ╲      - Critical business paths
     ╱  E2E   ╲     
    ╱__________╲    
   ╱            ╲   Integration Tests (20%)
  ╱ Integration ╲  - API endpoints
 ╱________________╲ - Database operations
╱                  ╲ Unit Tests (70%)
╱   Unit Tests      ╲ - Business logic
╱____________________╲ - Utilities
```

### Test Distribution

| Type | Quantity | Execution Time | Coverage |
|------|----------|----------------|----------|
| **Unit Tests** | ~900+ | < 30 seconds | 70% of tests |
| **Integration Tests** | ~40+ | < 2 minutes | 20% of tests |
| **E2E Tests** | ~35+ | < 10 minutes | 10% of tests |

---

## Frontend Testing

### Technology Stack

- **Test Runner:** Jest
- **Testing Library:** React Testing Library
- **E2E:** Playwright (separate)
- **Mocking:** Jest mocks
- **Coverage:** Jest coverage reports

### Test File Structure

```
CRM.Frontend/src/
├── components/
│   ├── Button/
│   │   ├── Button.tsx
│   │   └── Button.test.tsx        ← Unit test
│   └── CustomerList/
│       ├── CustomerList.tsx
│       └── CustomerList.test.tsx
├── services/
│   ├── customerService.ts
│   └── customerService.test.ts    ← Service test
├── hooks/
│   ├── useCustomer.ts
│   └── useCustomer.test.ts        ← Hook test
└── utils/
    ├── validation.ts
    └── validation.test.ts         ← Utility test
```

### Unit Testing Components

#### Example: Testing a Component

```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { CustomerList } from './CustomerList';
import { customerService } from '../../services/customerService';

// Mock the service
jest.mock('../../services/customerService');

describe('CustomerList', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should render loading state initially', () => {
    // Arrange
    const mockGetAll = jest.fn().mockReturnValue(new Promise(() => {}));
    (customerService.getAll as jest.Mock) = mockGetAll;

    // Act
    render(<CustomerList />);

    // Assert
    expect(screen.getByText(/loading/i)).toBeInTheDocument();
  });

  it('should display customers after loading', async () => {
    // Arrange
    const mockCustomers = [
      { id: 1, company: 'Acme Corp', email: 'test@acme.com' },
      { id: 2, company: 'Tech Inc', email: 'test@tech.com' },
    ];
    (customerService.getAll as jest.Mock).mockResolvedValue({
      items: mockCustomers,
      totalCount: 2,
    });

    // Act
    render(<CustomerList />);

    // Assert
    await waitFor(() => {
      expect(screen.getByText('Acme Corp')).toBeInTheDocument();
      expect(screen.getByText('Tech Inc')).toBeInTheDocument();
    });
  });

  it('should handle delete action', async () => {
    // Arrange
    const mockCustomers = [{ id: 1, company: 'Acme Corp' }];
    const mockDelete = jest.fn().mockResolvedValue({});
    (customerService.getAll as jest.Mock).mockResolvedValue({
      items: mockCustomers,
      totalCount: 1,
    });
    (customerService.delete as jest.Mock) = mockDelete;

    render(<CustomerList />);
    await waitFor(() => screen.getByText('Acme Corp'));

    // Act
    const deleteButton = screen.getByRole('button', { name: /delete/i });
    fireEvent.click(deleteButton);

    // Confirm dialog
    const confirmButton = await screen.findByRole('button', { name: /confirm/i });
    fireEvent.click(confirmButton);

    // Assert
    await waitFor(() => {
      expect(mockDelete).toHaveBeenCalledWith(1);
    });
  });
});
```

### Testing Custom Hooks

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { useCustomer } from './useCustomer';
import { customerService } from '../services/customerService';

jest.mock('../services/customerService');

describe('useCustomer', () => {
  it('should fetch customer on mount', async () => {
    // Arrange
    const mockCustomer = { id: 1, company: 'Acme Corp' };
    (customerService.getById as jest.Mock).mockResolvedValue(mockCustomer);

    // Act
    const { result } = renderHook(() => useCustomer(1));

    // Assert
    expect(result.current.loading).toBe(true);

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.customer).toEqual(mockCustomer);
    });
  });

  it('should handle error state', async () => {
    // Arrange
    const error = new Error('Failed to fetch');
    (customerService.getById as jest.Mock).mockRejectedValue(error);

    // Act
    const { result } = renderHook(() => useCustomer(1));

    // Assert
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe(error);
    });
  });
});
```

### Testing Services

```typescript
import axios from 'axios';
import { customerService } from './customerService';
import { Customer } from '../types/entities';

jest.mock('axios');
const mockedAxios = axios as jest.Mocked<typeof axios>;

describe('customerService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getById', () => {
    it('should fetch customer by id', async () => {
      // Arrange
      const mockCustomer: Customer = {
        id: 1,
        company: 'Acme Corp',
        email: 'test@acme.com',
      };
      mockedAxios.get.mockResolvedValue({ data: mockCustomer });

      // Act
      const result = await customerService.getById(1);

      // Assert
      expect(mockedAxios.get).toHaveBeenCalledWith('/api/customers/1');
      expect(result).toEqual(mockCustomer);
    });

    it('should throw error on failure', async () => {
      // Arrange
      const error = new Error('Not found');
      mockedAxios.get.mockRejectedValue(error);

      // Act & Assert
      await expect(customerService.getById(999)).rejects.toThrow('Not found');
    });
  });
});
```

### Coverage Configuration

**jest.config.json**

```json
{
  "collectCoverageFrom": [
    "src/**/*.{ts,tsx}",
    "!src/**/*.d.ts",
    "!src/index.tsx",
    "!src/main.tsx",
    "!src/**/*.stories.tsx",
    "!src/test-utils.tsx"
  ],
  "coverageThreshold": {
    "global": {
      "branches": 70,
      "functions": 70,
      "lines": 70,
      "statements": 70
    },
    "./src/services/": {
      "branches": 80,
      "functions": 90,
      "lines": 90,
      "statements": 90
    },
    "./src/utils/": {
      "branches": 90,
      "functions": 95,
      "lines": 95,
      "statements": 95
    }
  }
}
```

---

## Backend Testing

### Technology Stack

- **Test Framework:** xUnit
- **Mocking:** Moq
- **Assertions:** FluentAssertions
- **Test Database:** In-Memory or TestContainers

### Test Project Structure

```
CRM.Backend/tests/
├── CRM.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   │   ├── CustomerServiceTests.cs
│   │   │   └── OpportunityServiceTests.cs
│   │   ├── BusinessLogic/
│   │   │   ├── ValidationTests.cs
│   │   │   └── CalculationTests.cs
│   │   └── Utilities/
│   │       └── StringUtilsTests.cs
│   ├── Integration/
│   │   ├── Controllers/
│   │   │   ├── CustomersControllerTests.cs
│   │   │   └── OpportunitiesControllerTests.cs
│   │   └── Repositories/
│   │       └── CustomerRepositoryTests.cs
│   └── Helpers/
│       ├── TestDataFactory.cs
│       └── TestDbContextFactory.cs
```

### Unit Testing Services

```csharp
using Xunit;
using Moq;
using FluentAssertions;
using CRM.Infrastructure.Services;
using CRM.Core.Interfaces;
using CRM.Core.Entities;
using Microsoft.Extensions.Logging;

public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<IRepository<Customer>>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _sut = new CustomerService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = 1;
        var expectedCustomer = new Customer
        {
            Id = customerId,
            Company = "Acme Corp",
            Email = "test@acme.com"
        };
        _mockRepository
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _sut.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Company.Should().Be("Acme Corp");
        _mockRepository.Verify(r => r.GetByIdAsync(customerId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer)null);

        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Company = "New Corp",
            Email = "test@newcorp.com"
        };
        var createdCustomer = new Customer
        {
            Id = 1,
            Company = customer.Company,
            Email = customer.Email
        };
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _sut.CreateAsync(customer);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Company.Should().Be("New Corp");
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
    }
}
```

### Integration Testing Controllers

```csharp
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using FluentAssertions;
using CRM.Core.Dtos;

public class CustomersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        // Add authentication headers if needed
    }

    [Fact]
    public async Task GetCustomers_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ReturnsCreatedCustomer()
    {
        // Arrange
        var customerDto = new CustomerDto
        {
            Company = "Test Corp",
            Email = "test@testcorp.com"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", customerDto);
        var createdCustomer = await response.Content.ReadFromJsonAsync<CustomerDto>();

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        createdCustomer.Should().NotBeNull();
        createdCustomer.Company.Should().Be("Test Corp");
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var invalidDto = new CustomerDto { }; // Missing required fields

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", invalidDto);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}
```

### Test Data Factory

```csharp
public static class TestDataFactory
{
    public static Customer CreateCustomer(
        int? id = null,
        string company = "Test Company",
        string email = "test@example.com")
    {
        return new Customer
        {
            Id = id ?? 1,
            Company = company,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static List<Customer> CreateCustomers(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateCustomer(i, $"Company {i}", $"test{i}@example.com"))
            .ToList();
    }
}
```

---

## E2E Testing

### Technology Stack

- **Framework:** Playwright
- **Language:** TypeScript
- **Browser:** Chromium, Firefox, WebKit
- **Reports:** HTML, JSON

### Test Structure

```
e2e-tests/
├── tests/
│   ├── auth.setup.ts              # Authentication setup
│   ├── bvt/                       # Build Verification Tests
│   │   ├── login.spec.ts
│   │   └── navigation.spec.ts
│   ├── functional/                # Feature tests
│   │   ├── customers/
│   │   │   ├── create-customer.spec.ts
│   │   │   ├── edit-customer.spec.ts
│   │   │   └── delete-customer.spec.ts
│   │   └── opportunities/
│   └── data/                      # Data-driven tests
├── fixtures/
│   └── test-data.json
└── playwright.config.ts
```

### Example E2E Test

```typescript
import { test, expect } from '@playwright/test';

test.describe('Customer Management', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.fill('[name="email"]', 'test@example.com');
    await page.fill('[name="password"]', 'Test@123');
    await page.click('button[type="submit"]');
    await page.waitForURL('/dashboard');
  });

  test('should create new customer', async ({ page }) => {
    // Navigate to customers page
    await page.click('text=Customers');
    await page.waitForURL('/customers');

    // Click add button
    await page.click('button:has-text("Add Customer")');

    // Fill form
    await page.fill('[name="company"]', 'Test Company');
    await page.fill('[name="email"]', 'test@testcompany.com');
    await page.fill('[name="phone"]', '555-1234');

    // Submit
    await page.click('button:has-text("Save")');

    // Verify success
    await expect(page.locator('text=Customer created successfully')).toBeVisible();
    await expect(page.locator('text=Test Company')).toBeVisible();
  });

  test('should edit customer', async ({ page }) => {
    // Navigate and find customer
    await page.goto('/customers');
    await page.click('text=Acme Corp');

    // Edit
    await page.click('button:has-text("Edit")');
    await page.fill('[name="company"]', 'Acme Corporation');
    await page.click('button:has-text("Save")');

    // Verify
    await expect(page.locator('text=Customer updated successfully')).toBeVisible();
  });
});
```

---

## Test Data Management

### Strategies

1. **Test Data Factories**
   - Use factory functions to create test data
   - Ensures consistent, valid test data
   - Easy to modify for different scenarios

2. **Database Seeding**
   - Seed known data for E2E tests
   - Use database migrations for test data
   - Reset between test runs

3. **Test Isolation**
   - Each test should be independent
   - Clean up after each test
   - Use transactions for database tests

### Example Factory

```typescript
export class CustomerFactory {
  static create(overrides?: Partial<Customer>): Customer {
    return {
      id: 1,
      company: 'Test Company',
      email: 'test@example.com',
      phone: '555-1234',
      status: 'Active',
      ...overrides,
    };
  }

  static createMany(count: number): Customer[] {
    return Array.from({ length: count }, (_, i) =>
      this.create({
        id: i + 1,
        company: `Company ${i + 1}`,
        email: `test${i + 1}@example.com`,
      })
    );
  }
}
```

---

## CI/CD Integration

### Test Execution in Pipeline

```yaml
# .github/workflows/test.yml
name: Tests

on: [push, pull_request]

jobs:
  frontend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: cd CRM.Frontend && npm ci
      - run: cd CRM.Frontend && npm test -- --coverage
      - uses: codecov/codecov-action@v3

  backend-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      - run: cd CRM.Backend && dotnet test --collect:"XPlat Code Coverage"
      - uses: codecov/codecov-action@v3

  e2e-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - run: docker-compose up -d
      - uses: actions/setup-node@v3
      - run: cd e2e-tests && npm ci
      - run: npx playwright install
      - run: cd e2e-tests && BASE_URL=http://localhost npx playwright test
```

---

## Coverage Targets

### Current Status

| Component | Current | Target | Status |
|-----------|---------|--------|--------|
| **Backend Services** | 85% | 80% | ✅ Exceeds |
| **Backend Controllers** | 75% | 70% | ✅ Meets |
| **Frontend Services** | 45% | 80% | ❌ Below |
| **Frontend Components** | 40% | 60% | ❌ Below |
| **Frontend Hooks** | 20% | 70% | ❌ Below |

### Improvement Plan

1. **Q1 2026**: Increase frontend service coverage to 60%
2. **Q2 2026**: Increase frontend component coverage to 50%
3. **Q3 2026**: Increase frontend hook coverage to 50%
4. **Q4 2026**: Meet all coverage targets

---

## Testing Best Practices

### ✅ DO

- Write tests before fixing bugs
- Test behavior, not implementation
- Use descriptive test names
- Keep tests simple and focused
- Mock external dependencies
- Use factories for test data
- Run tests before committing

### ❌ DON'T

- Test implementation details
- Share state between tests
- Use hardcoded IDs in tests
- Skip cleanup
- Ignore flaky tests
- Write slow tests

---

## References

- [React Testing Library](https://testing-library.com/react)
- [Jest Documentation](https://jestjs.io/)
- [xUnit Documentation](https://xunit.net/)
- [Playwright Documentation](https://playwright.dev/)
- [Testing Best Practices](https://kentcdodds.com/blog/write-tests)

---

**Document Maintainer:** QA Team  
**Review Frequency:** Quarterly  
**Last Reviewed:** February 2026
