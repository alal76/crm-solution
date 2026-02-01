# Coding Standards & Conventions

> **Last Updated:** February 1, 2026 | **Version:** 1.7.28

---

## Table of Contents

1. [General Principles](#1-general-principles)
2. [C# / .NET Standards](#2-c--net-standards)
3. [TypeScript / React Standards](#3-typescript--react-standards)
4. [Database Standards](#4-database-standards)
5. [API Standards](#5-api-standards)
6. [Naming Conventions](#6-naming-conventions)
7. [File Organization](#7-file-organization)
8. [Libraries & Dependencies](#8-libraries--dependencies)

---

## 1. General Principles

### 1.1 Core Principles

| Principle | Description |
|-----------|-------------|
| **DRY** | Don't Repeat Yourself - Extract common code |
| **KISS** | Keep It Simple Stupid - Prefer simple solutions |
| **SOLID** | Follow SOLID principles for OOP |
| **Clean Code** | Readable, maintainable, testable code |
| **Fail Fast** | Validate early, throw meaningful exceptions |
| **Defensive Coding** | Null checks, input validation |

### 1.2 Code Review Checklist

- [ ] Code compiles without warnings
- [ ] Unit tests pass
- [ ] No hardcoded values (use constants/config)
- [ ] Proper error handling
- [ ] Logging for important operations
- [ ] Documentation for public APIs
- [ ] No sensitive data in logs
- [ ] SQL injection prevention (parameterized queries)

---

## 2. C# / .NET Standards

### 2.1 Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| **Namespace** | PascalCase | `CRM.Core.Entities` |
| **Class** | PascalCase | `CustomerService` |
| **Interface** | I + PascalCase | `ICustomerService` |
| **Method** | PascalCase | `GetCustomerById` |
| **Property** | PascalCase | `FirstName` |
| **Field (private)** | _camelCase | `_customerService` |
| **Field (const)** | PascalCase | `MaxRetries` |
| **Parameter** | camelCase | `customerId` |
| **Local variable** | camelCase | `customerList` |

### 2.2 File Naming

| Type | Pattern | Example |
|------|---------|---------|
| **Entity** | `{EntityName}.cs` | `Customer.cs` |
| **DTO** | `{EntityName}Dto.cs` | `CustomerDto.cs` |
| **Interface** | `I{ServiceName}.cs` | `ICustomerService.cs` |
| **Service** | `{ServiceName}.cs` | `CustomerService.cs` |
| **Controller** | `{Entity}Controller.cs` | `CustomersController.cs` |
| **Test** | `{ClassName}Tests.cs` | `CustomerServiceTests.cs` |

### 2.3 Code Style

```csharp
// Good: Clear naming, proper async pattern
public async Task<CustomerDto?> GetByIdAsync(int id)
{
    if (id <= 0)
    {
        throw new ArgumentException("ID must be positive", nameof(id));
    }

    var customer = await _context.Customers
        .Where(c => c.Id == id && !c.IsDeleted)
        .FirstOrDefaultAsync();

    return customer is null ? null : _mapper.Map<CustomerDto>(customer);
}

// Bad: Unclear naming, missing validation
public async Task<CustomerDto> Get(int id)
{
    var c = await _context.Customers.FindAsync(id);
    return _mapper.Map<CustomerDto>(c);
}
```

### 2.4 Entity Pattern

```csharp
/// <summary>
/// Customer entity representing a B2B or B2C account.
/// </summary>
public class Customer : BaseEntity
{
    /// <summary>
    /// Customer name (required, max 200 chars)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Customer type (B2B, B2C, Partner)
    /// </summary>
    public CustomerType Type { get; set; } = CustomerType.B2B;

    /// <summary>
    /// Is customer active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
}
```

### 2.5 Service Pattern

```csharp
public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(QueryParameters query);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<bool> DeleteAsync(int id);
}

public class CustomerService : ICustomerService
{
    private readonly ICrmDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICrmDbContext context,
        IMapper mapper,
        ILogger<CustomerService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        _logger.LogInformation("Creating customer: {Name}", dto.Name);

        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedAt = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created customer with ID: {Id}", customer.Id);
        return _mapper.Map<CustomerDto>(customer);
    }
}
```

### 2.6 Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>
    /// Get all customers with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] QueryParameters query)
    {
        var result = await _customerService.GetAllAsync(query);
        return Ok(result);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer is null)
        {
            return NotFound();
        }
        return Ok(customer);
    }
}
```

---

## 3. TypeScript / React Standards

### 3.1 Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| **Component** | PascalCase | `CustomerList` |
| **Hook** | use + PascalCase | `useCustomers` |
| **Context** | PascalCase + Context | `AuthContext` |
| **Service** | camelCase | `customerService` |
| **Function** | camelCase | `handleSubmit` |
| **Variable** | camelCase | `customerList` |
| **Constant** | SCREAMING_SNAKE | `MAX_PAGE_SIZE` |
| **Type/Interface** | PascalCase | `Customer` |
| **Enum** | PascalCase | `CustomerStatus` |

### 3.2 File Naming

| Type | Pattern | Example |
|------|---------|---------|
| **Component** | `{Name}.tsx` | `CustomerList.tsx` |
| **Page** | `{Name}Page.tsx` | `CustomersPage.tsx` |
| **Hook** | `use{Name}.ts` | `useCustomers.ts` |
| **Service** | `{name}Service.ts` | `customerService.ts` |
| **Context** | `{Name}Context.tsx` | `AuthContext.tsx` |
| **Types** | `{name}.types.ts` | `customer.types.ts` |
| **Test** | `{Name}.test.tsx` | `CustomerList.test.tsx` |

### 3.3 Component Pattern

```typescript
// Good: Functional component with proper typing
import React, { useState, useEffect } from 'react';
import { Box, Typography } from '@mui/material';
import { Customer } from '../../types/entities';
import { customerService } from '../../services/customerService';

interface CustomerListProps {
  onSelect?: (customer: Customer) => void;
  filter?: string;
}

const CustomerList: React.FC<CustomerListProps> = ({ onSelect, filter }) => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCustomers();
  }, [filter]);

  const loadCustomers = async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await customerService.getAll({ search: filter });
      setCustomers(result.items);
    } catch (err) {
      setError('Failed to load customers');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;

  return (
    <Box>
      {customers.map((customer) => (
        <CustomerCard
          key={customer.id}
          customer={customer}
          onClick={() => onSelect?.(customer)}
        />
      ))}
    </Box>
  );
};

export default CustomerList;
```

### 3.4 Hook Pattern

```typescript
// Custom hook for data fetching
import { useState, useEffect, useCallback } from 'react';
import { customerService } from '../services/customerService';
import { Customer, PagedResult } from '../types';

interface UseCustomersOptions {
  page?: number;
  pageSize?: number;
  search?: string;
}

interface UseCustomersResult {
  customers: Customer[];
  totalCount: number;
  loading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export const useCustomers = (options: UseCustomersOptions = {}): UseCustomersResult => {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const { page = 1, pageSize = 20, search } = options;

  const fetchCustomers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await customerService.getAll({ page, pageSize, search });
      setCustomers(result.items);
      setTotalCount(result.totalCount);
    } catch (err) {
      setError('Failed to load customers');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchCustomers();
  }, [fetchCustomers]);

  return { customers, totalCount, loading, error, refetch: fetchCustomers };
};
```

### 3.5 Service Pattern

```typescript
// API service with proper typing
import api from './api';
import { Customer, CreateCustomerDto, UpdateCustomerDto, PagedResult } from '../types';

interface QueryParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export const customerService = {
  getAll: async (params?: QueryParams): Promise<PagedResult<Customer>> => {
    const response = await api.get<PagedResult<Customer>>('/accounts', { params });
    return response.data;
  },

  getById: async (id: number): Promise<Customer> => {
    const response = await api.get<Customer>(`/accounts/${id}`);
    return response.data;
  },

  create: async (data: CreateCustomerDto): Promise<Customer> => {
    const response = await api.post<Customer>('/accounts', data);
    return response.data;
  },

  update: async (id: number, data: UpdateCustomerDto): Promise<Customer> => {
    const response = await api.put<Customer>(`/accounts/${id}`, data);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.delete(`/accounts/${id}`);
  },
};
```

---

## 4. Database Standards

### 4.1 Table Naming

| Type | Convention | Example |
|------|------------|---------|
| **Table** | PascalCase, Plural | `Customers` |
| **Column** | PascalCase | `FirstName` |
| **Primary Key** | Id | `Id` |
| **Foreign Key** | {Entity}Id | `CustomerId` |
| **Index** | IX_{Table}_{Column} | `IX_Customers_Email` |
| **Unique** | UQ_{Table}_{Column} | `UQ_Users_Email` |

### 4.2 Standard Columns

Every table should have:

```sql
CREATE TABLE `EntityName` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  -- Entity-specific columns --
  `CreatedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  `UpdatedAt` datetime(6) DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(6),
  `IsDeleted` tinyint(1) NOT NULL DEFAULT 0,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
```

### 4.3 Data Types

| C# Type | MariaDB Type | SQL Server Type |
|---------|--------------|-----------------|
| `int` | `int(11)` | `INT` |
| `long` | `bigint(20)` | `BIGINT` |
| `string` | `varchar(n)` | `NVARCHAR(n)` |
| `string` (long) | `text` | `NVARCHAR(MAX)` |
| `bool` | `tinyint(1)` | `BIT` |
| `decimal` | `decimal(18,2)` | `DECIMAL(18,2)` |
| `DateTime` | `datetime(6)` | `DATETIME2(7)` |
| `Guid` | `char(36)` | `UNIQUEIDENTIFIER` |

---

## 5. API Standards

### 5.1 URL Conventions

| Pattern | Example | Description |
|---------|---------|-------------|
| **List** | `GET /api/customers` | Get all with pagination |
| **Get** | `GET /api/customers/{id}` | Get single by ID |
| **Create** | `POST /api/customers` | Create new |
| **Update** | `PUT /api/customers/{id}` | Full update |
| **Patch** | `PATCH /api/customers/{id}` | Partial update |
| **Delete** | `DELETE /api/customers/{id}` | Soft delete |
| **Action** | `POST /api/customers/{id}/activate` | Custom action |
| **Sub-resource** | `GET /api/customers/{id}/contacts` | Related entities |

### 5.2 HTTP Status Codes

| Code | Usage |
|------|-------|
| **200** | Success |
| **201** | Created |
| **204** | No Content (delete success) |
| **400** | Bad Request (validation error) |
| **401** | Unauthorized |
| **403** | Forbidden |
| **404** | Not Found |
| **409** | Conflict |
| **500** | Internal Server Error |

### 5.3 Response Formats

**Single Entity:**
```json
{
  "id": 1,
  "name": "Acme Corp",
  "createdAt": "2026-01-01T00:00:00Z"
}
```

**Paginated List:**
```json
{
  "items": [],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

**Error:**
```json
{
  "error": "Validation failed",
  "details": {
    "name": ["Name is required"]
  }
}
```

---

## 6. Naming Conventions

### 6.1 Entity Terminology

| Term | Usage | Example |
|------|-------|---------|
| **Customer/Account** | B2B/B2C entity | `Customer`, `Account` |
| **Contact** | Individual person | `Contact` |
| **Lead** | Potential customer | `Lead` |
| **Opportunity** | Sales deal | `Opportunity` |
| **Quote** | Price proposal | `Quote` |
| **Campaign** | Marketing effort | `Campaign` |
| **ServiceRequest** | Support ticket | `ServiceRequest` |
| **Task** | User task | `CrmTask` (avoid conflict) |

### 6.2 Common Abbreviations

| Full | Abbreviation |
|------|--------------|
| Identifier | Id |
| Configuration | Config |
| Information | Info |
| Administration | Admin |
| Repository | Repo |
| Database | Db |
| Maximum | Max |
| Minimum | Min |

---

## 7. File Organization

### 7.1 Backend Structure

```
CRM.Core/
├── Entities/           # Domain entities
├── DTOs/               # Data transfer objects
├── Interfaces/         # Service interfaces
└── Enums/              # Enumerations

CRM.Infrastructure/
├── Data/               # DbContext, configurations
└── Services/           # Service implementations

CRM.Api/
├── Controllers/        # API controllers
├── Middleware/         # Custom middleware
└── Hubs/               # SignalR hubs
```

### 7.2 Frontend Structure

```
src/
├── components/         # Reusable components
│   ├── common/         # Generic UI
│   ├── forms/          # Form components
│   └── layout/         # Layout components
├── pages/              # Page components
├── services/           # API services
├── contexts/           # React contexts
├── hooks/              # Custom hooks
├── types/              # TypeScript types
└── utils/              # Utilities
```

---

## 8. Libraries & Dependencies

### 8.1 Backend Dependencies

| Package | Purpose | Notes |
|---------|---------|-------|
| **EF Core** | ORM | Use migrations, DbContext |
| **AutoMapper** | Object mapping | Define profiles |
| **FluentValidation** | Validation | Create validators |
| **Serilog** | Logging | Structured logging |
| **BCrypt.Net** | Password hashing | Never store plain passwords |
| **SignalR** | Real-time | WebSocket communication |

### 8.2 Frontend Dependencies

| Package | Purpose | Notes |
|---------|---------|-------|
| **React** | UI library | Functional components, hooks |
| **MUI** | UI components | Use theme provider |
| **Axios** | HTTP client | Create instances with interceptors |
| **React Router** | Routing | v6 with nested routes |
| **React Hook Form** | Forms | With Zod validation |
| **date-fns** | Date utilities | Prefer over moment.js |

### 8.3 Adding Dependencies

**Backend:**
```bash
cd CRM.Backend/src/CRM.Api
dotnet add package PackageName
```

**Frontend:**
```bash
cd CRM.Frontend
npm install package-name
npm install -D @types/package-name  # If needed
```

---

## Related Documentation

- [Architecture](../01-architecture/README.md)
- [Backend](../03-backend/README.md)
- [Frontend](../05-frontend/README.md)
