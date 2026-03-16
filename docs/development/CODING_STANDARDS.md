# CRM Solution - Coding Standards

**Version:** 2.0.0  
**Last Updated:** March 2026  
**Applies To:** All CRM Solution code contributions

---

## Table of Contents

1. [Overview](#overview)
2. [General Principles](#general-principles)
3. [Backend Standards (.NET 10.0)](#backend-standards-net-100)
4. [Frontend Standards (React/TypeScript)](#frontend-standards-reacttypescript)
5. [API Design Standards](#api-design-standards)
6. [Database Standards](#database-standards)
7. [Testing Standards](#testing-standards)
8. [Documentation Standards](#documentation-standards)
9. [Git Workflow](#git-workflow)
10. [Tooling Configuration](#tooling-configuration)

---

## Overview

This document defines the coding standards for the CRM Solution project. All contributors must follow these guidelines to ensure code quality, maintainability, and consistency.

### Goals
- **Consistency**: Code looks like it was written by one person
- **Readability**: Code is easy to understand
- **Maintainability**: Code is easy to modify
- **Quality**: Code is robust and well-tested

---

## General Principles

### 1. Clean Code
- Write self-documenting code
- Use meaningful names for variables, functions, and classes
- Keep functions small and focused (single responsibility)
- Avoid magic numbers - use named constants

### 2. DRY (Don't Repeat Yourself)
- Extract common logic into reusable functions
- Use inheritance and composition appropriately
- Centralize configuration and constants

### 3. KISS (Keep It Simple, Stupid)
- Prefer simple solutions over complex ones
- Avoid premature optimization
- Write code for humans first, computers second

### 4. SOLID Principles
- **S**ingle Responsibility: One reason to change
- **O**pen/Closed: Open for extension, closed for modification
- **L**iskov Substitution: Subtypes must be substitutable
- **I**nterface Segregation: Many specific interfaces > one general
- **D**ependency Inversion: Depend on abstractions

---

## Backend Standards (.NET 10.0)

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Namespace | PascalCase | `CRM.Core.Entities` |
| Class | PascalCase | `AccountService` |
| Interface | IPascalCase | `IAccountService` |
| Method | PascalCase | `GetAccountById` |
| Property | PascalCase | `FirstName` |
| Private Field | _camelCase | `_accountRepository` |
| Parameter | camelCase | `accountId` |
| Constant | PascalCase | `MaxRetryCount` |
| Enum | PascalCase | `AccountStatus.Active` |

### File Organization

```csharp
// 1. License header (required)
// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal

// 2. Using statements (sorted, grouped)
using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using CRM.Core.Entities;

// 3. Namespace
namespace CRM.Infrastructure.Services;

// 4. Class definition
public class AccountService : IAccountService
{
    // 5. Fields (private, then protected, then public)
    private readonly IAccountRepository _repository;
    private readonly ILogger<AccountService> _logger;
    
    // 6. Constructor
    public AccountService(IAccountRepository repository, ILogger<AccountService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    // 7. Properties
    public int MaxPageSize => 100;
    
    // 8. Public methods
    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    // 9. Private methods
    private void ValidateAccount(Account account)
    {
        // Validation logic
    }
}
```

### Best Practices

```csharp
// ✅ DO: Use async/await properly
public async Task<Account> GetAccountAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// ❌ DON'T: Block on async code
public Account GetAccount(int id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}

// ✅ DO: Use null-conditional operators
var name = account?.Contact?.FirstName ?? "Unknown";

// ❌ DON'T: Nested null checks
if (account != null && account.Contact != null)
{
    name = account.Contact.FirstName;
}

// ✅ DO: Use pattern matching
if (result is Account { IsActive: true } activeAccount)
{
    ProcessAccount(activeAccount);
}

// ✅ DO: Use expression-bodied members for simple methods
public string FullName => $"{FirstName} {LastName}";

// ✅ DO: Use records for DTOs
public record AccountDto(int Id, string Name, string Email);
```

### Exception Handling

```csharp
// ✅ DO: Catch specific exceptions
try
{
    await _repository.SaveAsync(entity);
}
catch (DbUpdateConcurrencyException ex)
{
    _logger.LogWarning(ex, "Concurrency conflict for entity {Id}", entity.Id);
    throw new ConflictException("Resource was modified by another user");
}
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database error saving entity {Id}", entity.Id);
    throw new DataAccessException("Failed to save changes");
}

// ❌ DON'T: Catch generic Exception without reason
catch (Exception ex)
{
    // Log and rethrow or handle specifically
}
```

---

## Frontend Standards (React/TypeScript)

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Component | PascalCase | `AccountList.tsx` |
| Hook | camelCase with use prefix | `useAccounts.ts` |
| Context | PascalCase with Context suffix | `AuthContext.tsx` |
| Service | camelCase with Service suffix | `accountService.ts` |
| Type/Interface | PascalCase | `AccountData` |
| Constant | SCREAMING_SNAKE_CASE | `MAX_PAGE_SIZE` |
| Variable | camelCase | `accountList` |
| Function | camelCase | `fetchAccounts` |

### Component Structure

```tsx
/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 */

import React, { useState, useEffect, useCallback } from 'react';
import { Box, Typography, Button } from '@mui/material';

import { Account } from '../types';
import { accountService } from '../services/accountService';
import { useAuth } from '../contexts/AuthContext';
import { AccountCard } from './AccountCard';

// Types
interface AccountListProps {
  initialFilter?: string;
  onAccountSelect: (account: Account) => void;
}

// Component
export const AccountList: React.FC<AccountListProps> = ({
  initialFilter = '',
  onAccountSelect,
}) => {
  // Hooks
  const { user } = useAuth();
  
  // State
  const [accounts, setAccounts] = useState<Account[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Callbacks
  const fetchAccounts = useCallback(async () => {
    try {
      setLoading(true);
      const data = await accountService.getAll();
      setAccounts(data);
    } catch (err) {
      setError('Failed to load accounts');
    } finally {
      setLoading(false);
    }
  }, []);
  
  // Effects
  useEffect(() => {
    fetchAccounts();
  }, [fetchAccounts]);
  
  // Render
  if (loading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  
  return (
    <Box>
      <Typography variant="h5">Accounts</Typography>
      {accounts.map((account) => (
        <AccountCard
          key={account.id}
          account={account}
          onClick={() => onAccountSelect(account)}
        />
      ))}
    </Box>
  );
};
```

### TypeScript Best Practices

```typescript
// ✅ DO: Define proper types
interface Account {
  id: number;
  name: string;
  email: string;
  status: AccountStatus;
}

type AccountStatus = 'active' | 'inactive' | 'pending';

// ❌ DON'T: Use 'any' type
const data: any = response.data; // Avoid!

// ✅ DO: Use proper type assertions
const data = response.data as Account[];

// ✅ DO: Use generics for reusable types
interface ApiResponse<T> {
  data: T;
  message: string;
  success: boolean;
}

// ✅ DO: Use discriminated unions
type Result<T> = 
  | { success: true; data: T }
  | { success: false; error: string };
```

### React Best Practices

```tsx
// ✅ DO: Memoize expensive computations
const sortedAccounts = useMemo(
  () => accounts.sort((a, b) => a.name.localeCompare(b.name)),
  [accounts]
);

// ✅ DO: Use useCallback for event handlers
const handleClick = useCallback((id: number) => {
  onSelect(id);
}, [onSelect]);

// ✅ DO: Split large components
// Instead of one 500-line component, use:
// - AccountList (container)
// - AccountCard (presentational)
// - AccountFilters (controls)

// ✅ DO: Use proper key props
{accounts.map((account) => (
  <AccountCard key={account.id} account={account} />
))}

// ❌ DON'T: Use index as key for dynamic lists
{accounts.map((account, index) => (
  <AccountCard key={index} account={account} /> // Bad!
))}
```

---

## API Design Standards

### RESTful Endpoints

```
GET    /api/accounts              # List accounts
GET    /api/accounts/{id}         # Get single account
POST   /api/accounts              # Create account
PUT    /api/accounts/{id}         # Update account (full)
PATCH  /api/accounts/{id}         # Update account (partial)
DELETE /api/accounts/{id}         # Delete account

# Nested resources
GET    /api/accounts/{id}/contacts    # List contacts for account
POST   /api/accounts/{id}/contacts    # Add contact to account

# Actions (when CRUD doesn't fit)
POST   /api/accounts/{id}/activate    # Custom action
POST   /api/accounts/{id}/merge       # Custom action
```

### Response Format

```json
// Success response
{
  "data": { ... },
  "message": "Account created successfully",
  "success": true
}

// Error response
{
  "error": "Validation failed",
  "details": [
    { "field": "email", "message": "Invalid email format" }
  ],
  "success": false
}

// Paginated response
{
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

### HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | Success |
| 201 | Created |
| 204 | No Content (successful DELETE) |
| 400 | Bad Request (validation error) |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict (duplicate, concurrency) |
| 422 | Unprocessable Entity |
| 429 | Too Many Requests |
| 500 | Internal Server Error |

---

## Database Standards

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Table | PascalCase, Plural | `Accounts`, `Contacts` |
| Column | PascalCase | `FirstName`, `CreatedAt` |
| Primary Key | Id | `Id` |
| Foreign Key | EntityId | `AccountId`, `ContactId` |
| Index | IX_Table_Column | `IX_Accounts_Email` |
| Constraint | CK/FK/UQ_Table_Name | `UQ_Accounts_Email` |

### Entity Design

```csharp
public class Account : BaseEntity
{
    // Primary key (from BaseEntity)
    // public int Id { get; set; }
    
    // Required fields
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    // Optional fields (nullable)
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Foreign keys
    public int? PrimaryContactId { get; set; }
    
    // Navigation properties
    public Contact? PrimaryContact { get; set; }
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    
    // Audit fields (from BaseEntity)
    // public DateTime CreatedAt { get; set; }
    // public DateTime? UpdatedAt { get; set; }
    // public string? CreatedBy { get; set; }
    // public string? UpdatedBy { get; set; }
}
```

---

## Testing Standards

### Test Naming

```csharp
// Pattern: MethodName_StateUnderTest_ExpectedBehavior
[Fact]
public async Task GetById_ExistingAccount_ReturnsAccount()
{
    // Arrange
    var account = new Account { Id = 1, Name = "Test" };
    _mockRepository.Setup(r => r.GetByIdAsync(1))
        .ReturnsAsync(account);
    
    // Act
    var result = await _service.GetByIdAsync(1);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("Test", result.Name);
}

[Fact]
public async Task GetById_NonExistingAccount_ReturnsNull()
{
    // Arrange
    _mockRepository.Setup(r => r.GetByIdAsync(999))
        .ReturnsAsync((Account?)null);
    
    // Act
    var result = await _service.GetByIdAsync(999);
    
    // Assert
    Assert.Null(result);
}
```

### Test Organization

```
tests/
├── CRM.Tests/
│   ├── Unit/
│   │   ├── Services/
│   │   │   └── AccountServiceTests.cs
│   │   └── Validators/
│   │       └── AccountValidatorTests.cs
│   ├── Integration/
│   │   └── Api/
│   │       └── AccountsControllerTests.cs
│   └── E2E/
│       └── AccountWorkflowTests.cs
```

### Coverage Requirements

| Type | Minimum Coverage |
|------|------------------|
| Unit Tests | 80% |
| Integration Tests | 60% |
| E2E Tests | Key workflows |

---

## Documentation Standards

### Code Comments

```csharp
/// <summary>
/// Retrieves an account by its unique identifier.
/// </summary>
/// <param name="id">The account ID.</param>
/// <returns>The account if found; otherwise, null.</returns>
/// <exception cref="ArgumentException">Thrown when id is less than 1.</exception>
public async Task<Account?> GetByIdAsync(int id)
{
    if (id < 1)
        throw new ArgumentException("ID must be positive", nameof(id));
    
    return await _repository.GetByIdAsync(id);
}

// Use inline comments sparingly for complex logic
var result = accounts
    .Where(a => a.IsActive) // Filter active only
    .OrderBy(a => a.Name)   // Sort alphabetically
    .Take(10);              // Limit to 10 results
```

### README Requirements

Every service/module should have a README with:
1. Purpose and overview
2. Setup instructions
3. Configuration options
4. API documentation (if applicable)
5. Testing instructions

---

## Git Workflow

### Branch Naming

```
feature/CRM-123-add-account-import
bugfix/CRM-456-fix-login-error
hotfix/CRM-789-security-patch
release/v2.0.0
```

### Commit Messages

```
feat(accounts): add bulk import functionality

- Add CSV import endpoint
- Add validation for imported data
- Add progress tracking

Closes #123
```

Prefixes:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation
- `style:` Formatting
- `refactor:` Code refactoring
- `test:` Adding tests
- `chore:` Maintenance

---

## Tooling Configuration

### Required Extensions (VS Code)

- ESLint
- Prettier
- C# Dev Kit
- EditorConfig

### Pre-commit Checks

```bash
# Frontend
npm run lint
npm run format:check
npm run type-check

# Backend
dotnet build --no-restore
dotnet test --no-build
```

### CI/CD Pipeline

1. **Build**: Compile all projects
2. **Lint**: ESLint + StyleCop
3. **Test**: Unit + Integration tests
4. **Security**: Dependency scan
5. **Deploy**: Environment-specific

---

## Enforcement

These standards are enforced through:

1. **Automated Tools**: ESLint, Prettier, StyleCop
2. **CI/CD Pipeline**: Build fails on violations
3. **Code Review**: PR approval required
4. **IDE Configuration**: EditorConfig

---

**Document History**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024-01-15 | A. Lal | Initial version |
| 2.0 | 2026-02-02 | A. Lal | Complete rewrite, automation |
