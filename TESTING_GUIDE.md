# CRM Solution - Testing Guide

## Overview

This document provides comprehensive information about testing the CRM Solution, including unit tests, integration tests, and build verification procedures.

**Testing Framework:**
- **Frontend**: Jest + React Testing Library
- **Backend**: xUnit + Moq + FluentAssertions
- **CI/CD**: GitHub Actions

---

## 1. Frontend Testing

### 1.1 Unit Tests

#### Location
```
CRM.Frontend/src/__tests__/
├── LoginPage.test.tsx
├── CustomersPage.test.tsx
└── apiClient.test.ts
```

#### Running Tests

**Run all tests in watch mode:**
```bash
cd CRM.Frontend
npm test
```

**Run tests once (CI mode):**
```bash
npm test -- --coverage --watchAll=false
```

**Run specific test file:**
```bash
npm test -- LoginPage.test.tsx
```

**Run tests with coverage report:**
```bash
npm test -- --coverage
```

### 1.2 Test Coverage

Target coverage thresholds:
- **Statements**: 80%
- **Branches**: 75%
- **Functions**: 80%
- **Lines**: 80%

View coverage report:
```bash
npm test -- --coverage
# Open coverage/lcov-report/index.html in browser
```

### 1.3 Testing Best Practices

**Component Testing:**
```typescript
test('renders component correctly', () => {
  render(<Component />);
  expect(screen.getByText('Expected Text')).toBeInTheDocument();
});
```

**API Testing:**
```typescript
test('fetches data successfully', async () => {
  mockedAPI.get.mockResolvedValue({ data: mockData });
  
  render(<Component />);
  
  await waitFor(() => {
    expect(screen.getByText('Data')).toBeInTheDocument();
  });
});
```

**User Interaction Testing:**
```typescript
test('updates on user input', () => {
  render(<Component />);
  const input = screen.getByRole('textbox');
  
  fireEvent.change(input, { target: { value: 'new value' } });
  
  expect(input.value).toBe('new value');
});
```

---

## 2. Backend Testing

### 2.1 Unit Tests

#### Location
```
CRM.Backend/tests/
├── Controllers/
│   ├── DepartmentsControllerTests.cs
│   └── CustomersControllerTests.cs
└── Services/
```

#### Running Tests

**Run all tests:**
```bash
cd CRM.Backend
dotnet test
```

**Run specific test project:**
```bash
dotnet test tests/CRM.Tests.csproj
```

**Run specific test class:**
```bash
dotnet test --filter "ClassName=DepartmentsControllerTests"
```

**Run with verbose output:**
```bash
dotnet test --logger "console;verbosity=detailed"
```

**Run with code coverage:**
```bash
dotnet test /p:CollectCoverageMetrics=true
```

### 2.2 Test Structure

**Basic Test Pattern:**
```csharp
[Fact]
public async Task MethodName_WithCondition_ReturnsExpectedResult()
{
    // Arrange - Setup test data and mocks
    var mockService = new Mock<IService>();
    var controller = new Controller(mockService.Object);
    
    // Act - Execute the method being tested
    var result = await controller.GetData();
    
    // Assert - Verify the results
    Assert.IsType<OkObjectResult>(result);
    mockService.Verify(s => s.Method(), Times.Once);
}
```

### 2.3 Mocking Examples

**Mock Service:**
```csharp
var mockService = new Mock<IDepartmentService>();
mockService.Setup(s => s.GetAllAsync())
    .ReturnsAsync(new List<Department>());
```

**Verify Calls:**
```csharp
mockService.Verify(s => s.GetAllAsync(), Times.Once);
mockService.Verify(s => s.CreateAsync(It.IsAny<Department>()), Times.Never);
```

---

## 3. Build Verification

### 3.1 Automated Build Verification

Run comprehensive build verification:
```bash
./scripts/verify-build.sh
```

This script checks:
- ✅ System dependencies (Node.js, npm, .NET, Docker)
- ✅ Frontend build (TypeScript compilation, production bundle)
- ✅ Backend build (.NET solution build)
- ✅ Docker images (Frontend and API)
- ✅ Project structure integrity

### 3.2 Manual Build Steps

**Frontend Build:**
```bash
cd CRM.Frontend
npm install
npm run build
# Output: build/ directory with optimized production bundle
```

**Backend Build:**
```bash
cd CRM.Backend
dotnet restore
dotnet build -c Release
# Output: bin/Release/net8.0/
```

**Docker Build:**
```bash
docker compose build
# Builds both crm-frontend and crm-api images
```

### 3.3 Build Verification Checklist

- [ ] All dependencies installed
- [ ] TypeScript compilation successful (no errors)
- [ ] ESLint checks pass
- [ ] Frontend tests pass
- [ ] Backend tests pass
- [ ] Production bundle created
- [ ] Docker images built successfully
- [ ] No critical security vulnerabilities

---

## 4. Test Runner Scripts

### 4.1 Comprehensive Test Runner

Run all tests and checks:
```bash
./scripts/run-tests.sh
```

**Includes:**
1. Frontend unit tests
2. Frontend build verification
3. TypeScript compilation check
4. Backend unit tests
5. Code quality checks (ESLint)

### 4.2 Test Reports

Test logs are saved to:
- Frontend: `/tmp/frontend-test.log`
- TypeScript: `/tmp/ts-check.log`
- Backend: `/tmp/backend-test.log`

---

## 5. Continuous Integration

### 5.1 GitHub Actions Pipeline

Automatic testing on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop`

**Pipeline Stages:**
1. **Frontend Tests & Build**
   - Node.js 18.x and 20.x
   - TypeScript check, linting, unit tests
   - Production bundle build
   - Coverage report upload

2. **Backend Tests & Build**
   - .NET 8.0
   - Solution restore and build
   - Unit tests with coverage
   - Test results upload

3. **Docker Build**
   - Build Docker images
   - Push to container registry

4. **Code Quality**
   - ESLint analysis
   - StyleCop analysis
   - Security vulnerability scan

5. **Integration Tests**
   - API health checks
   - Database connectivity

### 5.2 CI/CD Configuration

File: `.github/workflows/ci-cd.yml`

Trigger tests:
```yaml
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]
```

---

## 6. Test Database

### 6.1 In-Memory Database for Tests

Backend tests use in-memory MariaDB:
```csharp
services.AddDbContext<CrmContext>(options =>
    options.UseInMemoryDatabase("test-db"));
```

### 6.2 Test Data Seeding

Create sample data in tests:
```csharp
var departments = new List<Department>
{
    new Department { Id = 1, Name = "Sales" },
    new Department { Id = 2, Name = "Engineering" }
};
context.Departments.AddRange(departments);
context.SaveChanges();
```

---

## 7. Performance Testing

### 7.1 Frontend Performance

Monitor bundle size:
```bash
npm run build
du -sh CRM.Frontend/build/
```

Target: < 500 MB

### 7.2 Backend Performance

Test API response times:
```bash
time curl http://localhost:5000/api/customers
```

Target: < 200ms for list endpoints

---

## 8. Troubleshooting

### Issue: Tests Fail with "Cannot find module"

**Solution:**
```bash
npm install  # Reinstall dependencies
npm test     # Run tests again
```

### Issue: Backend Tests Fail with Database Error

**Solution:**
```bash
# Ensure migrations are applied
dotnet ef database update -p CRM.Backend/src/CRM.Api
dotnet test
```

### Issue: Docker Build Fails

**Solution:**
```bash
docker system prune -af  # Clean up old images
docker compose build     # Rebuild
```

### Issue: Coverage Not Generated

**Solution:**
```bash
npm test -- --coverage --watchAll=false
# Check coverage/ directory
```

---

## 9. Test Maintenance

### 9.1 Adding New Tests

**Frontend Test Template:**
```typescript
import { render, screen } from '@testing-library/react';
import ComponentToTest from '../ComponentToTest';

describe('ComponentToTest', () => {
  test('description of what is tested', () => {
    render(<ComponentToTest />);
    expect(screen.getByText('text')).toBeInTheDocument();
  });
});
```

**Backend Test Template:**
```csharp
[Fact]
public async Task TestDescription()
{
    // Arrange
    var mock = new Mock<IService>();
    
    // Act
    var result = await controller.Method();
    
    // Assert
    Assert.NotNull(result);
}
```

### 9.2 Test Naming Convention

**Frontend:** `ComponentName.test.tsx`
**Backend:** `ClassNameTests.cs`
**Test Methods:** `MethodUnderTest_Scenario_ExpectedResult`

Example: `GetAll_ReturnsOkResult_WithCustomers`

---

## 10. Quick Reference

### Run Frontend Tests
```bash
cd CRM.Frontend
npm test -- --watchAll=false --coverage
```

### Run Backend Tests
```bash
cd CRM.Backend
dotnet test
```

### Run All Tests
```bash
./scripts/run-tests.sh
```

### Verify Build
```bash
./scripts/verify-build.sh
```

### Docker Compose Up
```bash
docker compose up --build -d
```

### View Test Coverage
```bash
open CRM.Frontend/coverage/lcov-report/index.html
```

---

## 11. Coverage Goals

| Component | Target | Current |
|-----------|--------|---------|
| Controllers | 85% | TBD |
| Services | 80% | TBD |
| Utilities | 90% | TBD |
| Overall | 80% | TBD |

---

## 12. Resources

- [Jest Documentation](https://jestjs.io/)
- [React Testing Library](https://testing-library.com/react)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**Last Updated**: January 20, 2026  
**Version**: 1.2.0  
**Maintained By**: Development Team
