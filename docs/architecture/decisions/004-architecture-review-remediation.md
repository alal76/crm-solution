# ADR-004: Architecture Review and Remediation (February 2026)

## Status
Accepted

## Date
2026-02-02

## Context

A comprehensive architecture and code quality review was conducted on the CRM Solution. The review evaluated:
- Architecture adherence (Hexagonal/Ports & Adapters)
- Design patterns consistency
- Code quality and security
- Testing coverage
- Modularity and maintainability

## Findings Summary

### Critical Issues (Resolved)

| Issue | Status | Resolution |
|-------|--------|------------|
| Hardcoded secrets in config files | ✅ Fixed | Replaced with environment variable placeholders |
| Hardcoded IP addresses (192.168.0.9) | ✅ Fixed | Replaced with configurable values |
| Missing custom exception types | ✅ Fixed | Created `CRM.Core.Exceptions` namespace |

### High Priority Issues (Documented for Future)

| Issue | Status | Recommendation |
|-------|--------|----------------|
| Controllers directly using DbContext | 📋 Documented | Migrate to service layer in phases |
| TypeScript `as any` casts | ✅ Reduced | Reduced from 54 to ~9 instances |
| Generic exception throwing | 📋 Documented | Use custom exceptions progressively |

### Good Practices Found

1. **Layer Separation**: Clean project references (Core → Infrastructure → API)
2. **Hexagonal Architecture**: Ports defined but not fully utilized
3. **Frontend Code Splitting**: React.lazy() properly implemented for 30+ components
4. **Microservices Structure**: Well-organized service boundaries
5. **Error Handling Middleware**: Concurrency conflicts properly handled

## Decision

### 1. Security Configuration Pattern

All secrets must use environment variable placeholders:

```json
{
  "Jwt": {
    "Secret": "${JWT_SECRET:DEVELOPMENT_ONLY_CHANGE_IN_PRODUCTION_32CHARS!}"
  }
}
```

Production deployments MUST set:
- `JWT_SECRET` - 32+ character cryptographic secret
- `DB_PASSWORD` - Database credentials
- `SSL_CERT_PASSWORD` - Certificate password

### 2. Custom Exception Hierarchy

Created `CRM.Core.Exceptions` with typed exceptions:

| Exception | HTTP Code | Use Case |
|-----------|-----------|----------|
| `EntityNotFoundException` | 404 | Entity lookup failures |
| `ValidationException` | 400 | Input validation errors |
| `BusinessRuleException` | 422 | Business logic violations |
| `AuthorizationException` | 403 | Permission denied |
| `ConcurrencyException` | 409 | Optimistic locking conflicts |
| `ServiceException` | 500 | Internal service errors |
| `RateLimitException` | 429 | Rate limit exceeded |

### 3. Service Layer Refactoring (Future Work)

Controllers that bypass the service layer should be refactored in phases:

**Phase 1** (Next Sprint):
- `NotesController` → Create `NotesService`
- `ActivitiesController` → Create `ActivitiesService`

**Phase 2**:
- `CommunicationsController` → Create `CommunicationsService`
- `TasksController` → Create `TasksService`
- `InteractionsController` → Create `InteractionsService`

**Pattern to Follow**:
```csharp
// Controller (thin)
[HttpGet("{id}")]
public async Task<ActionResult<NoteDto>> GetById(int id)
{
    var note = await _noteService.GetByIdAsync(id);
    if (note == null) throw new EntityNotFoundException("Note", id);
    return Ok(note);
}

// Service (contains business logic)
public async Task<NoteDto?> GetByIdAsync(int id)
{
    var entity = await _repository.GetByIdAsync(id);
    return entity != null ? MapToDto(entity) : null;
}
```

### 4. Configuration Standards

Infrastructure configuration should:
1. Use `${ENV_VAR:default}` syntax for environment overrides
2. Default to `localhost` for development
3. Require explicit configuration for production
4. Never commit production secrets to version control

## Consequences

### Positive
- Improved security posture with no hardcoded secrets
- Better error handling with typed exceptions
- Clear path for architectural improvements
- Consistent configuration patterns

### Negative
- Some controllers still directly use DbContext (technical debt)
- ~9 TypeScript `as any` casts remain (complex cases)
- Requires environment setup for deployment

### Neutral
- Development requires setting environment variables
- Service layer refactoring is incremental work

## Related Documents

- [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- [SECURITY_BEST_PRACTICES.md](../../SECURITY_BEST_PRACTICES.md)
- [ADR-001](001-coding-standards-enforcement.md)
- [ADR-002](002-security-headers-middleware.md)
- [ADR-003](003-microservices-architecture.md)

## References

- [Hexagonal Architecture](../HEXAGONAL_ARCHITECTURE.md)
- [Distributed UI Architecture](../DISTRIBUTED_UI_ARCHITECTURE_RECOMMENDATIONS.md)
