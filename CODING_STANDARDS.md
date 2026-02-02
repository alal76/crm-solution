# CRM Solution - Coding Standards & Best Practices

**Version:** 2.0  
**Last Updated:** February 2, 2026  
**Status:** Active

---

## Table of Contents

1. [Overview](#overview)
2. [General Principles](#general-principles)
3. [Backend Standards (.NET 8.0)](#backend-standards-net-80)
4. [Frontend Standards (React/TypeScript)](#frontend-standards-reacttypescript)
5. [Database Standards](#database-standards)
6. [API Design Standards](#api-design-standards)
7. [Testing Standards](#testing-standards)
8. [Security Standards](#security-standards)
9. [Documentation Standards](#documentation-standards)
10. [Code Review Checklist](#code-review-checklist)

---

## Overview

This document establishes coding standards and best practices for the CRM Solution project. All contributors must follow these guidelines to ensure code quality, maintainability, and consistency across the codebase.

### Goals

- **Consistency**: Uniform code style across all components
- **Maintainability**: Easy to understand and modify
- **Quality**: Reduce bugs and technical debt
- **Performance**: Efficient and scalable code
- **Security**: Secure by default

---

## General Principles

### Code Style

1. **Use EditorConfig**: Follow the `.editorconfig` file settings
2. **Formatting**: Use automated formatters (Prettier for frontend, built-in for C#)
3. **Naming**: Use clear, descriptive names for variables, functions, and classes
4. **Comments**: Write self-documenting code; use comments for complex logic only
5. **DRY**: Don't Repeat Yourself - extract common functionality
6. **KISS**: Keep It Simple, Stupid - prefer simple solutions
7. **YAGNI**: You Aren't Gonna Need It - don't over-engineer

### Version Control

1. **Commit Messages**: Use conventional commits format
   ```
   type(scope): subject
   
   Examples:
   feat(customers): add customer deduplication feature
   fix(auth): resolve JWT token expiration issue
   docs(readme): update installation instructions
   refactor(api): improve error handling patterns
   test(leads): add lead scoring unit tests
   ```

2. **Branch Naming**:
   - `feature/` - New features
   - `fix/` - Bug fixes
   - `refactor/` - Code refactoring
   - `docs/` - Documentation updates
   - `test/` - Test additions/updates

3. **Pull Requests**:
   - Keep PRs focused and small
   - Link to related issues
   - Include tests for new features
   - Update documentation as needed

---

## Backend Standards (.NET 8.0)

### Naming Conventions

```csharp
// Namespaces: PascalCase
namespace CRM.Core.Services;

// Classes: PascalCase
public class CustomerService : ICustomerService

// Interfaces: PascalCase with 'I' prefix
public interface ICustomerService

// Methods: PascalCase
public async Task<Customer> GetByIdAsync(int id)

// Properties: PascalCase
public string FirstName { get; set; }

// Private fields: camelCase with '_' prefix
private readonly IRepository<Customer> _customerRepository;

// Constants: PascalCase
public const int MaxCustomersPerPage = 100;

// Local variables: camelCase
var customer = await _customerRepository.GetByIdAsync(id);

// Parameters: camelCase
public void UpdateCustomer(int customerId, CustomerDto dto)
```

### File Organization

```csharp
// 1. Using statements (sorted)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

// 2. Namespace
namespace CRM.Infrastructure.Services
{
    // 3. Class documentation
    /// <summary>
    /// Service for managing customer operations.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        // 4. Private fields
        private readonly IRepository<Customer> _repository;
        private readonly ILogger<CustomerService> _logger;
        
        // 5. Constructor
        public CustomerService(
            IRepository<Customer> repository,
            ILogger<CustomerService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        // 6. Public methods
        public async Task<Customer> GetByIdAsync(int id)
        {
            // Implementation
        }
        
        // 7. Private methods
        private void ValidateCustomer(Customer customer)
        {
            // Implementation
        }
    }
}
```

### Async/Await Best Practices

```csharp
// ✅ GOOD - Use async/await properly
public async Task<Customer> GetCustomerAsync(int id)
{
    var customer = await _repository.GetByIdAsync(id);
    return customer;
}

// ❌ BAD - Don't use async void (except event handlers)
public async void UpdateCustomer(int id) // Wrong!

// ✅ GOOD - Use ConfigureAwait(false) in library code
var result = await SomeMethodAsync().ConfigureAwait(false);

// ✅ GOOD - Use Task.WhenAll for parallel operations
var tasks = new[]
{
    GetCustomersAsync(),
    GetOrdersAsync(),
    GetProductsAsync()
};
await Task.WhenAll(tasks);
```

### Dependency Injection

```csharp
// ✅ GOOD - Constructor injection
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerController> _logger;
    
    public CustomerController(
        ICustomerService customerService,
        ILogger<CustomerController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }
}

// ❌ BAD - Service locator anti-pattern
var service = ServiceLocator.Get<ICustomerService>(); // Avoid!
```

### Error Handling

```csharp
// ✅ GOOD - Specific exceptions with context
public async Task<Customer> GetByIdAsync(int id)
{
    if (id <= 0)
    {
        throw new ArgumentException("Customer ID must be positive", nameof(id));
    }
    
    var customer = await _repository.GetByIdAsync(id);
    
    if (customer == null)
    {
        throw new NotFoundException($"Customer with ID {id} not found");
    }
    
    return customer;
}

// ✅ GOOD - Logging with structured data
_logger.LogError(ex, "Failed to create customer. Email: {Email}", dto.Email);

// ❌ BAD - Swallowing exceptions
try
{
    // code
}
catch
{
    // Empty catch block - never do this!
}
```

### LINQ Best Practices

```csharp
// ✅ GOOD - Use appropriate LINQ methods
var activeCustomers = customers
    .Where(c => c.Status == CustomerStatus.Active)
    .OrderBy(c => c.Company)
    .Take(10)
    .ToList();

// ✅ GOOD - Use Any() instead of Count() for existence checks
if (customers.Any(c => c.Email == email))

// ❌ BAD - Inefficient
if (customers.Count(c => c.Email == email) > 0)

// ✅ GOOD - Use FirstOrDefault for single item
var customer = customers.FirstOrDefault(c => c.Id == id);

// ❌ BAD - Don't use First() without checking existence
var customer = customers.First(c => c.Id == id); // Throws if not found
```

### Entity Framework Best Practices

```csharp
// ✅ GOOD - Use AsNoTracking for read-only queries
var customers = await _context.Customers
    .AsNoTracking()
    .Where(c => c.Status == CustomerStatus.Active)
    .ToListAsync();

// ✅ GOOD - Use Include for eager loading
var customer = await _context.Customers
    .Include(c => c.Contacts)
    .Include(c => c.Addresses)
    .FirstOrDefaultAsync(c => c.Id == id);

// ✅ GOOD - Use Select for projection
var customerNames = await _context.Customers
    .Select(c => new { c.Id, c.Company })
    .ToListAsync();

// ❌ BAD - Don't load entire entities when you need a subset
var customers = await _context.Customers.ToListAsync();
var names = customers.Select(c => c.Company).ToList(); // Inefficient!
```

---

## Frontend Standards (React/TypeScript)

### TypeScript Standards

#### Type Safety

```typescript
// ✅ GOOD - Always define types
interface Customer {
  id: number;
  company: string;
  email: string;
  status: CustomerStatus;
}

type CustomerStatus = 'Active' | 'Inactive' | 'Prospect';

// ❌ BAD - Never use 'any' (enforced by ESLint)
const data: any = response.data; // Not allowed!

// ✅ GOOD - Use unknown and type guards when needed
const data: unknown = response.data;

function isCustomer(data: unknown): data is Customer {
  return (
    typeof data === 'object' &&
    data !== null &&
    'id' in data &&
    'company' in data
  );
}

if (isCustomer(data)) {
  console.log(data.company); // Type-safe!
}

// ✅ GOOD - Use generics for reusable code
interface ApiResponse<T> {
  data: T;
  status: number;
  message?: string;
}

async function fetchData<T>(url: string): Promise<ApiResponse<T>> {
  // Implementation
}
```

#### Interfaces vs Types

```typescript
// ✅ Use interfaces for object shapes
interface Customer {
  id: number;
  company: string;
}

// ✅ Use types for unions, tuples, and complex types
type CustomerStatus = 'Active' | 'Inactive' | 'Prospect';
type CustomerOrLead = Customer | Lead;
type Coordinates = [number, number];

// ✅ Use types for utility types
type ReadonlyCustomer = Readonly<Customer>;
type PartialCustomer = Partial<Customer>;
```

### React Component Standards

#### Component Structure

```typescript
// ✅ GOOD - Functional components with TypeScript
import React, { useState, useEffect } from 'react';

interface CustomerFormProps {
  customerId?: number;
  onSave: (customer: Customer) => void;
  onCancel: () => void;
}

export const CustomerForm: React.FC<CustomerFormProps> = ({
  customerId,
  onSave,
  onCancel,
}) => {
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (customerId) {
      loadCustomer(customerId);
    }
  }, [customerId]);

  const loadCustomer = async (id: number) => {
    setLoading(true);
    try {
      const data = await customerService.getById(id);
      setCustomer(data);
    } catch (error) {
      console.error('Failed to load customer', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (customer) {
      onSave(customer);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Form fields */}
    </form>
  );
};
```

#### Hooks Best Practices

```typescript
// ✅ GOOD - Custom hooks for reusable logic
export function useCustomer(customerId: number) {
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const loadCustomer = async () => {
      setLoading(true);
      try {
        const data = await customerService.getById(customerId);
        setCustomer(data);
      } catch (err) {
        setError(err as Error);
      } finally {
        setLoading(false);
      }
    };

    loadCustomer();
  }, [customerId]);

  return { customer, loading, error };
}

// Usage
const { customer, loading, error } = useCustomer(customerId);

// ✅ GOOD - Use useCallback for memoized callbacks
const handleSave = useCallback(
  (customer: Customer) => {
    onSave(customer);
  },
  [onSave]
);

// ✅ GOOD - Use useMemo for expensive computations
const sortedCustomers = useMemo(
  () => customers.sort((a, b) => a.company.localeCompare(b.company)),
  [customers]
);
```

### State Management

```typescript
// ✅ GOOD - Use Context for shared state
interface AppContextValue {
  user: User | null;
  theme: Theme;
  updateTheme: (theme: Theme) => void;
}

const AppContext = React.createContext<AppContextValue | undefined>(undefined);

export const useAppContext = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useAppContext must be used within AppProvider');
  }
  return context;
};

// ✅ GOOD - Reducer for complex state logic
interface State {
  customers: Customer[];
  loading: boolean;
  error: string | null;
}

type Action =
  | { type: 'FETCH_START' }
  | { type: 'FETCH_SUCCESS'; payload: Customer[] }
  | { type: 'FETCH_ERROR'; payload: string };

function customerReducer(state: State, action: Action): State {
  switch (action.type) {
    case 'FETCH_START':
      return { ...state, loading: true, error: null };
    case 'FETCH_SUCCESS':
      return { ...state, loading: false, customers: action.payload };
    case 'FETCH_ERROR':
      return { ...state, loading: false, error: action.payload };
    default:
      return state;
  }
}
```

### API Client Standards

```typescript
// ✅ GOOD - Typed API client
class CustomerService {
  private baseUrl = '/api/customers';

  async getAll(): Promise<Customer[]> {
    const response = await apiClient.get<Customer[]>(this.baseUrl);
    return response.data;
  }

  async getById(id: number): Promise<Customer> {
    const response = await apiClient.get<Customer>(`${this.baseUrl}/${id}`);
    return response.data;
  }

  async create(customer: Omit<Customer, 'id'>): Promise<Customer> {
    const response = await apiClient.post<Customer>(this.baseUrl, customer);
    return response.data;
  }

  async update(id: number, customer: Partial<Customer>): Promise<Customer> {
    const response = await apiClient.put<Customer>(
      `${this.baseUrl}/${id}`,
      customer
    );
    return response.data;
  }

  async delete(id: number): Promise<void> {
    await apiClient.delete(`${this.baseUrl}/${id}`);
  }
}

export const customerService = new CustomerService();
```

---

## Database Standards

### Table Naming

- Use PascalCase for table names: `Customers`, `CustomerContacts`
- Use singular names for lookup tables: `CustomerStatus`, `Priority`
- Use plural names for entity tables: `Customers`, `Orders`

### Column Naming

- Use PascalCase: `FirstName`, `EmailAddress`, `CreatedDate`
- Always include audit fields: `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy`
- Use consistent ID naming: `Id` for primary key, `CustomerId` for foreign key

### Indexing

```sql
-- ✅ GOOD - Index foreign keys
CREATE INDEX IX_Opportunities_CustomerId ON Opportunities(CustomerId);

-- ✅ GOOD - Composite indexes for common queries
CREATE INDEX IX_Customers_Status_CreatedDate 
ON Customers(Status, CreatedDate DESC);

-- ✅ GOOD - Unique constraints
CREATE UNIQUE INDEX UX_Customers_Email ON Customers(Email);
```

---

## API Design Standards

### RESTful API Conventions

```
GET    /api/customers           - Get all customers
GET    /api/customers/{id}      - Get customer by ID
POST   /api/customers           - Create customer
PUT    /api/customers/{id}      - Update customer
DELETE /api/customers/{id}      - Delete customer
GET    /api/customers/{id}/contacts - Get customer contacts
```

### Response Format

```json
// ✅ GOOD - Consistent response format
{
  "data": {
    "id": 1,
    "company": "Acme Corp",
    "email": "contact@acme.com"
  },
  "success": true,
  "message": "Customer retrieved successfully"
}

// ✅ GOOD - Paginated response
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 25,
  "totalPages": 6
}

// ✅ GOOD - Error response
{
  "success": false,
  "message": "Customer not found",
  "errors": {
    "customerId": ["Customer with ID 123 does not exist"]
  }
}
```

### HTTP Status Codes

- `200 OK` - Successful GET, PUT
- `201 Created` - Successful POST
- `204 No Content` - Successful DELETE
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Testing Standards

### Unit Tests

```csharp
// ✅ GOOD - Test naming convention: MethodName_Scenario_ExpectedResult
[Fact]
public async Task GetByIdAsync_ValidId_ReturnsCustomer()
{
    // Arrange
    var mockRepo = new Mock<IRepository<Customer>>();
    var expected = new Customer { Id = 1, Company = "Acme" };
    mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(expected);
    var service = new CustomerService(mockRepo.Object);

    // Act
    var result = await service.GetByIdAsync(1);

    // Assert
    result.Should().BeEquivalentTo(expected);
}

[Fact]
public async Task GetByIdAsync_InvalidId_ThrowsArgumentException()
{
    // Arrange
    var service = new CustomerService(Mock.Of<IRepository<Customer>>());

    // Act & Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.GetByIdAsync(0)
    );
}
```

### Frontend Tests

```typescript
// ✅ GOOD - React Testing Library
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CustomerForm } from './CustomerForm';

describe('CustomerForm', () => {
  it('should render form fields', () => {
    render(<CustomerForm onSave={jest.fn()} onCancel={jest.fn()} />);
    
    expect(screen.getByLabelText('Company Name')).toBeInTheDocument();
    expect(screen.getByLabelText('Email')).toBeInTheDocument();
  });

  it('should call onSave when form is submitted', async () => {
    const handleSave = jest.fn();
    render(<CustomerForm onSave={handleSave} onCancel={jest.fn()} />);
    
    await userEvent.type(screen.getByLabelText('Company Name'), 'Acme Corp');
    await userEvent.type(screen.getByLabelText('Email'), 'test@acme.com');
    await userEvent.click(screen.getByText('Save'));
    
    await waitFor(() => {
      expect(handleSave).toHaveBeenCalledWith(
        expect.objectContaining({
          company: 'Acme Corp',
          email: 'test@acme.com',
        })
      );
    });
  });
});
```

### Test Coverage Goals

- Unit tests: 80% code coverage
- Integration tests: Critical paths covered
- E2E tests: Happy paths and key workflows
- Performance tests: Key operations benchmarked

---

## Security Standards

### Authentication & Authorization

```csharp
// ✅ GOOD - Use authorization attributes
[Authorize(Roles = "Admin,Manager")]
[HttpPost("customers")]
public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto dto)
{
    // Implementation
}

// ✅ GOOD - Validate user permissions
if (!User.HasPermission(Permission.CustomerWrite))
{
    return Forbid();
}
```

### Input Validation

```csharp
// ✅ GOOD - Use FluentValidation
public class CustomerValidator : AbstractValidator<CustomerDto>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(200);
            
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
```

### Secure Coding

- Never log sensitive data (passwords, tokens, PII)
- Use parameterized queries (prevent SQL injection)
- Validate and sanitize all user input
- Use HTTPS only
- Implement rate limiting
- Keep dependencies updated

---

## Documentation Standards

### Code Documentation

```csharp
/// <summary>
/// Retrieves a customer by their unique identifier.
/// </summary>
/// <param name="id">The customer's unique identifier.</param>
/// <returns>The customer entity if found.</returns>
/// <exception cref="ArgumentException">Thrown when id is less than or equal to 0.</exception>
/// <exception cref="NotFoundException">Thrown when customer is not found.</exception>
public async Task<Customer> GetByIdAsync(int id)
{
    // Implementation
}
```

### README Files

- Every module/service should have a README
- Include purpose, setup instructions, and usage examples
- Keep documentation up-to-date with code changes

---

## Code Review Checklist

### Functionality
- [ ] Code solves the problem correctly
- [ ] Edge cases are handled
- [ ] No obvious bugs

### Code Quality
- [ ] Follows naming conventions
- [ ] No code duplication
- [ ] Functions are small and focused
- [ ] No TypeScript 'any' types
- [ ] Proper error handling

### Testing
- [ ] Unit tests included
- [ ] Tests pass locally
- [ ] Test coverage meets requirements

### Security
- [ ] No security vulnerabilities
- [ ] Input validation present
- [ ] No sensitive data in logs

### Performance
- [ ] No obvious performance issues
- [ ] Database queries optimized
- [ ] No N+1 query problems

### Documentation
- [ ] Code is self-documenting
- [ ] Complex logic explained
- [ ] README updated if needed

---

## Enforcement

These standards are enforced through:

1. **EditorConfig** - Automatic formatting
2. **ESLint** - Frontend linting (TypeScript)
3. **StyleCop** - Backend linting (C#)
4. **Prettier** - Frontend code formatting
5. **Code Reviews** - Manual review process
6. **CI/CD Pipeline** - Automated checks

---

## Resources

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [TypeScript Style Guide](https://google.github.io/styleguide/tsguide.html)
- [React Best Practices](https://react.dev/learn/thinking-in-react)
- [REST API Design](https://restfulapi.net/)

---

## Changelog

| Version | Date | Changes |
|---------|------|---------|
| 2.0 | 2026-02-02 | Complete rewrite with comprehensive standards |
| 1.0 | 2025-01-15 | Initial version |

---

**Questions or suggestions?** Open an issue or submit a PR to improve these standards.
