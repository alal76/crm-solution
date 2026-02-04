# GitHub Copilot Instructions

## Project Context

This is a full-stack CRM Solution with:
- **Backend:** ASP.NET Core 8.0 + Entity Framework Core
- **Frontend:** React 18 + TypeScript + Material-UI 5
- **Database:** MariaDB (primary)
- **Architecture:** Microservices with YARP Gateway

## Active Implementation Project

**🚧 ACTIVE PROJECT: Pluggable Architecture Implementation**

### Implementation Tracker (MUST READ)

**Always check this file at the start of each session:**

📋 **[docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md](../docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md)**

This tracker contains:
- Current phase and week progress
- 237 detailed task checkboxes
- Session progress log
- Blockers and decisions

### Architecture Decision Record

📐 **[docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md](../docs/architecture/ADR-001-Pluggable-Architecture-Strategy.md)**

Contains:
- Industry standard patterns (Microsoft.FeatureManagement, Strategy/Factory)
- Port interface definitions
- Provider implementation examples
- Configuration schemas

## Key Implementation Files

### Phase 0: Foundation (Current)

| Component | Location | Status |
|-----------|----------|--------|
| Feature Flags | `CRM.Application/Features/FeatureFlags.cs` | 🔴 To Create |
| Provider Types | `CRM.Application/Features/ProviderTypes.cs` | 🔴 To Create |
| Port Interfaces | `CRM.Core/Ports/Output/` | 🟡 Partially Exists |
| Provider Factories | `CRM.Application/Factories/` | 🔴 To Create |
| BuiltIn Providers | `CRM.Infrastructure/Providers/BuiltIn/` | 🔴 To Create |

### Existing Hexagonal Architecture

The project already has port/adapter foundations:
- `CRM.Core/Ports/Input/IInputPorts.cs` - Driving ports
- `CRM.Core/Ports/Output/IOutputPorts.cs` - Driven ports
- `CRM.Core/Ports/Output/Database/` - Database adapters
- `CRM.Core/Ports/Output/Storage/` - Storage adapters

## Implementation Patterns

### Feature Flags Pattern

```csharp
// Use Microsoft.FeatureManagement.AspNetCore
public static class FeatureFlags
{
    public const string UseExternalSearch = "Providers:Search:External";
    public const string UseExternalChat = "Providers:Chat:External";
    // etc.
}
```

### Provider Factory Pattern

```csharp
public interface IProviderFactory<TProvider> where TProvider : class
{
    TProvider GetProvider();
    TProvider GetProvider(string providerName);
    IEnumerable<string> GetAvailableProviders();
}
```

### Code Preservation Rule

**NEVER DELETE EXISTING CODE** - Always refactor to BuiltIn providers:
- Existing search → `BuiltInSearchProvider`
- Existing email → `BuiltInNotificationProvider`
- Existing reports → `BuiltInAnalyticsProvider`

## Session Workflow

1. **Start of Session:**
   - Read `PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md`
   - Check current phase/week status
   - Identify next uncompleted tasks

2. **During Implementation:**
   - Mark tasks ⬜ → ✅ as completed
   - Update progress counts
   - Document any blockers

3. **End of Session:**
   - Update Session Progress Log in tracker
   - Commit changes with descriptive message
   - Note what's next

## Quick Commands

```bash
# Build backend
cd CRM.Backend && dotnet build

# Run tests
cd CRM.Backend && dotnet test

# Build frontend
cd CRM.Frontend && npm run build

# Start Docker stack
docker-compose -f docker/docker-compose.yml up -d
```

## Documentation Hierarchy

1. `SOLUTION_CONTEXT.md` - Complete technical reference
2. `docs/AGENT_CONTEXT.md` - Quick orientation
3. `docs/architecture/ADR-001-*.md` - Architecture decisions
4. `docs/architecture/PLUGGABLE_ARCHITECTURE_IMPLEMENTATION_TRACKER.md` - **Current work tracking**

## Contact Points

- Architecture decisions: Check ADR documents in `docs/architecture/`
- Existing patterns: Check `CRM.Core/Ports/` for hexagonal architecture
- Feature flags: Check `CRM.Core/Entities/SystemSettings.cs` for existing flags
