# CRM Solution — Code Patterns

> **Last Updated:** March 9, 2026 (v0.621.0)  
> **ADR Reference:** [ADR-011 — Domain Model Enrichment Strategy](../01-architecture/ADR-011-domain-model-enrichment-strategy.md)

---

## Entity Enrichment Pattern (AP-059)

All entities with lifecycle state (`Status`, `Stage`, or similar fields) **must** implement domain methods that encapsulate state transitions, guard invariants, and raise typed domain events. Direct property assignment of state fields from services is prohibited.

### 1. Interface Implementation

Every enriched entity implements `IHasDomainEvents` from `CRM.Core.Ports.Output.Events`:

```csharp
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using CRM.Core.Ports.Output.Events;

public class ServiceRequest : BaseEntity, IHasDomainEvents
{
    // --- Domain Events (IHasDomainEvents) ---
    private readonly List<IDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### 2. Domain Methods (Canonical Example: `ServiceRequest.Resolve()`)

Each state transition is a **public method** on the entity with this structure:

1. **Guard clauses** — throw `BusinessRuleException(ruleName, message)` for invalid transitions
2. **State mutations** — update properties + set `UpdatedAt = DateTime.UtcNow`
3. **Domain event** — call `AddDomainEvent(new XxxEvent(...))`

```csharp
/// <summary>Resolves the service request with a resolution summary.</summary>
public void Resolve(string resolutionSummary, string? resolutionCode = null, string? rootCause = null)
{
    // 1. Guard clauses
    if (Status == ServiceRequestStatus.Closed)
        throw new BusinessRuleException("ServiceRequest.Resolve",
            "Cannot resolve a closed service request.");
    if (Status == ServiceRequestStatus.Resolved)
        throw new BusinessRuleException("ServiceRequest.Resolve",
            "Service request is already resolved.");

    // 2. State mutations
    Status = ServiceRequestStatus.Resolved;
    ResolutionSummary = resolutionSummary;
    ResolutionCode = resolutionCode;
    RootCause = rootCause;
    ResolvedDate = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;

    // SLA breach detection
    if (ResolutionDueDate.HasValue && ResolvedDate.Value > ResolutionDueDate.Value)
        ResolutionSlaBreached = true;

    // 3. Domain event
    AddDomainEvent(new ServiceRequestResolvedEvent(Id, resolutionSummary, ResolvedDate.Value));
}
```

### 3. Typed Domain Event Records

Domain events are **sealed records** extending `DomainEventBase`, stored in `CRM.Core/Entities/Events/{Entity}Events.cs`:

```csharp
using CRM.Core.Ports.Output.Events;

namespace CRM.Core.Entities.Events;

public sealed record ServiceRequestResolvedEvent(
    int ServiceRequestId,
    string ResolutionSummary,
    DateTime ResolvedAt) : DomainEventBase;

public sealed record ServiceRequestClosedEvent(
    int ServiceRequestId,
    string? CloseNotes,
    DateTime ClosedAt) : DomainEventBase;
```

**Conventions:**
- Record name: `{Entity}{Action}Event` (e.g., `AccountLifecycleChangedEvent`)
- Parameters: aggregate ID first, then relevant state, then timestamp if applicable
- All records are `sealed` and inherit from `DomainEventBase`
- One file per entity: `{Entity}Events.cs`

### 4. BusinessRuleException Convention

```csharp
throw new BusinessRuleException(
    "Entity.MethodName",           // ruleName — identifies the guard
    "Human-readable error message" // message — describes the violation
);
```

The two-parameter constructor is mandatory. The `ruleName` should be `{Entity}.{Method}`.

### 5. CreateForTesting Factory

Each enriched entity provides an `internal static` factory for zero-mock unit tests:

```csharp
/// <summary>Factory for unit-testing — creates an entity in a controlled initial state.</summary>
internal static Account CreateForTesting(
    AccountLifecycleStage stage = AccountLifecycleStage.Lead,
    bool isActive = true)
{
    return new Account
    {
        Id = 1,
        LifecycleStage = stage,
        IsActive = isActive,
        Company = "Test Account",
        CreatedAt = DateTime.UtcNow
    };
}
```

**Rules:**
- Visibility: `internal static` (tests access via `[InternalsVisibleTo]`)
- Defaults: sensible defaults for the happy-path test scenario
- Minimal: only set properties needed for domain method tests

### 6. Unit Test Pattern (Zero-Mock Entity Tests)

Tests go in `tests/Unit/Core/{Entity}EntityTests.cs` and instantiate entities directly — no mocks:

```csharp
public class AccountEntityBehaviorTests
{
    [Fact]
    public void ChangeLifecycleStage_ShouldUpdateStage_WhenActive()
    {
        var account = Account.CreateForTesting(AccountLifecycleStage.Lead, isActive: true);

        account.ChangeLifecycleStage(AccountLifecycleStage.Active);

        account.LifecycleStage.Should().Be(AccountLifecycleStage.Active);
    }

    [Fact]
    public void ChangeLifecycleStage_ShouldRaiseAccountLifecycleChangedEvent()
    {
        var account = Account.CreateForTesting(AccountLifecycleStage.Lead, isActive: true);

        account.ChangeLifecycleStage(AccountLifecycleStage.Active);

        var evt = account.DomainEvents.OfType<AccountLifecycleChangedEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.OldStage.Should().Be(AccountLifecycleStage.Lead);
        evt.NewStage.Should().Be(AccountLifecycleStage.Active);
    }

    [Fact]
    public void ChangeLifecycleStage_ShouldThrowBusinessRuleException_WhenDeactivated()
    {
        var account = Account.CreateForTesting(AccountLifecycleStage.Lead, isActive: false);

        var act = () => account.ChangeLifecycleStage(AccountLifecycleStage.Active);

        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Account is deactivated*");
    }
}
```

**Test naming:** `{Method}_Should{ExpectedBehavior}_When{Condition}`

### 7. Service Layer Delegation

Services **must not** directly assign state fields. Instead, call the entity's domain method:

```csharp
// ❌ WRONG — direct state mutation in service
serviceRequest.Status = ServiceRequestStatus.Resolved;
serviceRequest.ResolvedDate = DateTime.UtcNow;

// ✅ CORRECT — delegate to entity method
serviceRequest.Resolve(resolutionSummary, resolutionCode, rootCause);
await _dbContext.SaveChangesAsync(cancellationToken);
```

The `DomainEventDispatchInterceptor` (registered as an EF Core `SaveChangesInterceptor`) automatically dispatches domain events after `SaveChangesAsync`.

### 8. Enriched Entities (Phase 1+2 Complete)

| Entity | Methods | Events File | Tests |
|--------|---------|-------------|-------|
| ServiceRequest | Resolve, Close, Escalate, Assign, Reopen | ServiceRequestEvents.cs | 157 |
| Opportunity | TransitionToStage, Close, UpdateExpectedRevenue | OpportunityEvents.cs | 20 |
| Lead | ConvertToOpportunity, Disqualify, Qualify, Assign | LeadEvents.cs | 18 |
| Account | ChangeLifecycleStage, SetPrimaryContact, Deactivate | AccountEvents.cs | 12 |
| Contract | Approve, Renew, Terminate, Expire | ContractEvents.cs | 16 |
| Incident | Resolve, Close, Escalate | IncidentEvents.cs | 15 |

### 9. Phase 3 — Standing Rule (Ongoing)

Any entity with `Status`, `Stage`, or lifecycle fields that is **touched in a PR** must be enriched in that PR. Phase 3 candidates:

| Entity | Methods to Add |
|--------|---------------|
| Quote | Approve, Send, Revoke |
| Order | Confirm, Ship, Cancel |
| Invoice | Send, MarkPaid, Void |
| Subscription | Cancel, Reinstate |
| Campaign | Launch, Pause, Complete |
| SLAPolicy | Activate, Deactivate |
| KnowledgeBaseArticle | Publish, Archive |

**Code review rule:** Reviewer blocks PR if direct status assignment found in service code for any enriched entity.
