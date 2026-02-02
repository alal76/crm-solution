# CRM Solution - Coding Standards & Best Practices

**Version:** 1.0  
**Last Updated:** February 2026  
**Status:** Active

## Table of Contents

- [TypeScript Standards](#typescript-standards)
- [React Best Practices](#react-best-practices)
- [Backend (.NET) Standards](#backend-net-standards)
- [Testing Standards](#testing-standards)
- [Code Review Guidelines](#code-review-guidelines)
- [Git Commit Standards](#git-commit-standards)

---

## TypeScript Standards

### Type Safety

#### ✅ DO: Use Explicit Types

```typescript
// Good
interface Customer {
  id: number;
  name: string;
  email: string;
}

function getCustomer(id: number): Promise<Customer> {
  return apiClient.get<Customer>(`/customers/${id}`);
}

// Bad
function getCustomer(id: any): any {
  return apiClient.get(`/customers/${id}`);
}
```

#### ❌ DON'T: Use `any` Type

```typescript
// Bad - Loses all type safety
const data: any = response.data;
const customer = data as any;

// Good - Use proper types
const data = response.data as Customer;
// Or better - define the response type
const response = await apiClient.get<ApiResponse<Customer>>('/customers/1');
```

#### ✅ DO: Use Type Guards

```typescript
// Good
function isCustomer(obj: unknown): obj is Customer {
  return (
    typeof obj === 'object' &&
    obj !== null &&
    'id' in obj &&
    'name' in obj
  );
}

if (isCustomer(data)) {
  console.log(data.name); // Type-safe
}
```

#### ✅ DO: Use Generics for Reusable Code

```typescript
// Good
interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

async function fetchEntity<T>(url: string): Promise<ApiResponse<T>> {
  const response = await apiClient.get<ApiResponse<T>>(url);
  return response.data;
}
```

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| **Interface** | PascalCase | `Customer`, `UserProfile` |
| **Type** | PascalCase | `PermissionKey`, `EntityTypeName` |
| **Function** | camelCase | `getCustomer`, `hasPermission` |
| **Variable** | camelCase | `customerId`, `isActive` |
| **Constant** | UPPER_SNAKE_CASE | `MAX_RETRY_COUNT`, `API_BASE_URL` |
| **Component** | PascalCase | `CustomerList`, `FormField` |
| **Hook** | camelCase with `use` prefix | `useCustomer`, `usePermissions` |

### File Organization

```
src/
├── components/          # Reusable UI components
│   ├── common/         # Shared components (Button, Input, etc.)
│   ├── layout/         # Layout components (Header, Sidebar, etc.)
│   └── features/       # Feature-specific components
├── pages/              # Page components (routes)
├── services/           # API service layer
├── hooks/              # Custom React hooks
├── contexts/           # React Context providers
├── types/              # TypeScript type definitions
├── utils/              # Utility functions
├── config/             # Configuration files
└── theme/              # Theme and styling
```

---

## React Best Practices

### Component Structure

#### ✅ DO: Use Functional Components with Hooks

```typescript
// Good
import React, { useState, useEffect } from 'react';

interface Props {
  customerId: number;
}

export const CustomerDetails: React.FC<Props> = ({ customerId }) => {
  const [customer, setCustomer] = useState<Customer | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadCustomer();
  }, [customerId]);

  async function loadCustomer() {
    setLoading(true);
    try {
      const data = await customerService.getById(customerId);
      setCustomer(data);
    } catch (error) {
      console.error('Failed to load customer:', error);
    } finally {
      setLoading(false);
    }
  }

  if (loading) return <Loading />;
  if (!customer) return <NotFound />;

  return <div>{customer.name}</div>;
};
```

### State Management

#### ✅ DO: Use `useReducer` for Complex State

```typescript
// Good - Complex state with multiple related values
interface State {
  items: Customer[];
  loading: boolean;
  error: string | null;
  page: number;
  totalPages: number;
}

type Action =
  | { type: 'FETCH_START' }
  | { type: 'FETCH_SUCCESS'; payload: { items: Customer[]; totalPages: number } }
  | { type: 'FETCH_ERROR'; payload: string }
  | { type: 'SET_PAGE'; payload: number };

function reducer(state: State, action: Action): State {
  switch (action.type) {
    case 'FETCH_START':
      return { ...state, loading: true, error: null };
    case 'FETCH_SUCCESS':
      return {
        ...state,
        loading: false,
        items: action.payload.items,
        totalPages: action.payload.totalPages,
      };
    case 'FETCH_ERROR':
      return { ...state, loading: false, error: action.payload };
    case 'SET_PAGE':
      return { ...state, page: action.payload };
    default:
      return state;
  }
}

// Usage
const [state, dispatch] = useReducer(reducer, initialState);
```

### useEffect Dependencies

#### ✅ DO: Include All Dependencies

```typescript
// Good
useEffect(() => {
  fetchData(id, filter);
}, [id, filter]); // All dependencies included

// Bad - Disabling the rule hides the problem
useEffect(() => {
  fetchData(id, filter);
}, [id]); // eslint-disable-line react-hooks/exhaustive-deps
```

#### ✅ DO: Use Callbacks for Stable References

```typescript
// Good
const fetchData = useCallback(async () => {
  const result = await apiClient.get('/data');
  setData(result);
}, []); // Stable reference

useEffect(() => {
  fetchData();
}, [fetchData]); // Safe to include
```

### Error Handling

#### ✅ DO: Implement Error Boundaries

```typescript
// Good
import React, { Component, ErrorInfo } from 'react';

interface Props {
  children: React.ReactNode;
  fallback?: React.ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = {
    hasError: false,
    error: null,
  };

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('Error boundary caught:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || <ErrorFallback error={this.state.error} />;
    }

    return this.props.children;
  }
}
```

#### ✅ DO: Handle Async Errors Properly

```typescript
// Good
async function handleSubmit(data: FormData) {
  try {
    setLoading(true);
    await apiClient.post('/customers', data);
    showSuccess('Customer created successfully');
    navigate('/customers');
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 400) {
        showError(error.response.data.message || 'Invalid data');
      } else if (error.response?.status === 401) {
        showError('Unauthorized. Please log in.');
        navigate('/login');
      } else {
        showError('An unexpected error occurred');
      }
    } else {
      showError('Network error. Please try again.');
    }
  } finally {
    setLoading(false);
  }
}
```

---

## Backend (.NET) Standards

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| **Class** | PascalCase | `Customer`, `CustomerService` |
| **Interface** | `I` + PascalCase | `ICustomerService`, `IRepository<T>` |
| **Method** | PascalCase | `GetCustomer`, `CreateCustomerAsync` |
| **Private Field** | `_` + camelCase | `_dbContext`, `_logger` |
| **Property** | PascalCase | `CustomerId`, `IsActive` |
| **Constant** | PascalCase | `MaxRetryCount`, `DefaultPageSize` |

### Async Best Practices

#### ✅ DO: Use Async/Await Consistently

```csharp
// Good
public async Task<Customer> GetCustomerAsync(int id)
{
    return await _dbContext.Customers
        .Include(c => c.Contacts)
        .FirstOrDefaultAsync(c => c.Id == id);
}

// Bad - Blocking call
public Customer GetCustomer(int id)
{
    return _dbContext.Customers
        .Include(c => c.Contacts)
        .FirstOrDefault(c => c.Id == id);
}
```

### Dependency Injection

#### ✅ DO: Use Constructor Injection

```csharp
// Good
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        _logger.LogInformation("Getting customer {CustomerId}", id);
        var customer = await _customerService.GetByIdAsync(id);
        return customer == null ? NotFound() : Ok(customer);
    }
}
```

### Error Handling

#### ✅ DO: Use Proper Exception Handling

```csharp
// Good
public async Task<Customer> CreateCustomerAsync(CustomerDto dto)
{
    try
    {
        var customer = _mapper.Map<Customer>(dto);
        _dbContext.Customers.Add(customer);
        await _dbContext.SaveChangesAsync();
        return customer;
    }
    catch (DbUpdateException ex)
    {
        _logger.LogError(ex, "Failed to create customer {CustomerName}", dto.Name);
        throw new BusinessException("Failed to create customer", ex);
    }
}
```

---

## Testing Standards

### Unit Testing

#### ✅ DO: Follow AAA Pattern (Arrange, Act, Assert)

```typescript
// Good
describe('CustomerService', () => {
  describe('getById', () => {
    it('should return customer when id exists', async () => {
      // Arrange
      const customerId = 1;
      const expectedCustomer = { id: 1, name: 'Test Customer' };
      mockApiClient.get.mockResolvedValue({ data: expectedCustomer });

      // Act
      const result = await customerService.getById(customerId);

      // Assert
      expect(result).toEqual(expectedCustomer);
      expect(mockApiClient.get).toHaveBeenCalledWith('/customers/1');
    });

    it('should throw error when customer not found', async () => {
      // Arrange
      mockApiClient.get.mockRejectedValue(new Error('Not found'));

      // Act & Assert
      await expect(customerService.getById(999)).rejects.toThrow('Not found');
    });
  });
});
```

### Test Coverage Targets

| Component | Minimum Coverage | Target |
|-----------|------------------|--------|
| **Services** | 80% | 90% |
| **Utilities** | 90% | 95% |
| **Components** | 60% | 70% |
| **Hooks** | 70% | 80% |
| **Overall** | 70% | 80% |

### Backend Testing

```csharp
// Good - xUnit with FluentAssertions
public class CustomerServiceTests
{
    private readonly Mock<CrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _sut;

    public CustomerServiceTests()
    {
        _mockDbContext = new Mock<CrmDbContext>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _sut = new CustomerService(_mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsCustomer()
    {
        // Arrange
        var customerId = 1;
        var customer = new Customer { Id = customerId, Name = "Test" };
        _mockDbContext.Setup(db => db.Customers.FindAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _sut.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Name.Should().Be("Test");
    }
}
```

---

## Code Review Guidelines

### Review Checklist

- [ ] **Type Safety**: No use of `any` type
- [ ] **Error Handling**: All async operations have try-catch
- [ ] **Testing**: New code has accompanying tests
- [ ] **Dependencies**: useEffect has complete dependency array
- [ ] **Performance**: No unnecessary re-renders or calculations
- [ ] **Security**: No hardcoded secrets or credentials
- [ ] **Accessibility**: Interactive elements have proper ARIA attributes
- [ ] **Documentation**: Public APIs have JSDoc comments
- [ ] **Naming**: Variables and functions have descriptive names
- [ ] **Complexity**: Functions are small and focused (< 50 lines)

### Review Comments Format

```
[CATEGORY] Issue description

Example:
[TYPE_SAFETY] Using 'as any' loses type safety
Suggestion: Define a proper interface for this data structure

[PERFORMANCE] Expensive calculation in render
Suggestion: Move to useMemo or useCallback

[SECURITY] API key exposed in code
Action Required: Move to environment variable
```

---

## Git Commit Standards

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation changes |
| `style` | Code style changes (formatting) |
| `refactor` | Code refactoring |
| `test` | Adding or updating tests |
| `chore` | Maintenance tasks |
| `perf` | Performance improvements |

### Examples

```
feat(customers): add duplicate detection feature

- Implement fuzzy matching algorithm
- Add merge wizard UI
- Create unmerge capability

Closes #123
```

```
fix(auth): prevent infinite redirect loop

Check for redirect flag before redirecting to login
to prevent infinite loop when token is expired

Fixes #456
```

---

## Security Best Practices

### ✅ DO: Sanitize User Input

```typescript
// Good
import DOMPurify from 'dompurify';

function sanitizeHtml(html: string): string {
  return DOMPurify.sanitize(html);
}

// Usage
const safeHtml = sanitizeHtml(userProvidedHtml);
```

### ✅ DO: Store Secrets Securely

```typescript
// Bad
const API_KEY = 'sk-1234567890abcdef';

// Good
const API_KEY = process.env.REACT_APP_API_KEY;
```

### ✅ DO: Validate All API Responses

```typescript
// Good
const response = await apiClient.get('/customer/1');
if (!isValidCustomer(response.data)) {
  throw new Error('Invalid response from server');
}
```

---

## Performance Best Practices

### ✅ DO: Memoize Expensive Calculations

```typescript
// Good
const expensiveValue = useMemo(() => {
  return calculateExpensiveValue(data);
}, [data]);
```

### ✅ DO: Use React.memo for Pure Components

```typescript
// Good
export const CustomerListItem = React.memo<Props>(({ customer }) => {
  return <div>{customer.name}</div>;
});
```

### ✅ DO: Lazy Load Routes

```typescript
// Good
const CustomersPage = lazy(() => import('./pages/CustomersPage'));

<Suspense fallback={<Loading />}>
  <Routes>
    <Route path="/customers" element={<CustomersPage />} />
  </Routes>
</Suspense>
```

---

## Accessibility Standards

### ✅ DO: Use Semantic HTML

```typescript
// Good
<button onClick={handleClick}>Submit</button>

// Bad
<div onClick={handleClick}>Submit</div>
```

### ✅ DO: Add ARIA Labels

```typescript
// Good
<button
  onClick={handleDelete}
  aria-label={`Delete customer ${customer.name}`}
>
  <DeleteIcon />
</button>
```

---

## References

- [TypeScript Handbook](https://www.typescriptlang.org/docs/)
- [React Documentation](https://react.dev)
- [.NET Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Testing Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)

---

**Document Maintainer:** Development Team  
**Review Frequency:** Quarterly  
**Last Reviewed:** February 2026
