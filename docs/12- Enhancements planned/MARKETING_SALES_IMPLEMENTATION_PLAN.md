# Marketing & Sales Enhancement - Implementation Plan

## Overview

This document provides step-by-step implementation details for closing the gaps identified in the Marketing & Sales gap analysis. Each section contains specific tasks, code structures, and testing requirements.

**Prerequisites:**
- Current solution on `dev` branch (fresh from main)
- All existing tests passing
- Docker environment running

---

## Phase 1: Foundation Gaps (Sprint 1)

### G1: Event Attendees Entity

**Objective:** Allow tracking of attendees for calendar events/meetings

#### Step 1.1: Create EventAttendee Entity

**File:** `CRM.Backend/src/CRM.Core/Entities/EventAttendee.cs`

```csharp
namespace CRM.Core.Entities;

/// <summary>
/// Tracks attendees for events (meetings, calls, demos)
/// </summary>
public enum AttendeeType
{
    User = 0,
    Contact = 1,
    Lead = 2
}

public enum AttendeeResponseStatus
{
    NotResponded = 0,
    Accepted = 1,
    Declined = 2,
    Tentative = 3
}

public class EventAttendee : BaseEntity
{
    /// <summary>Activity/Event ID</summary>
    public int ActivityId { get; set; }
    
    /// <summary>Type of attendee</summary>
    public AttendeeType AttendeeType { get; set; }
    
    /// <summary>Polymorphic ID (User/Contact/Lead)</summary>
    public int AttendeeId { get; set; }
    
    /// <summary>Response status</summary>
    public AttendeeResponseStatus ResponseStatus { get; set; } = AttendeeResponseStatus.NotResponded;
    
    /// <summary>Response timestamp</summary>
    public DateTime? RespondedAt { get; set; }
    
    /// <summary>Is this the organizer?</summary>
    public bool IsOrganizer { get; set; } = false;
    
    /// <summary>Is attendance required?</summary>
    public bool IsRequired { get; set; } = true;
    
    /// <summary>Attendee notes</summary>
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Activity? Activity { get; set; }
}
```

#### Step 1.2: Update CrmDbContext

Add to `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`:
```csharp
public DbSet<EventAttendee> EventAttendees { get; set; }
```

#### Step 1.3: Create EF Core Configuration

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/Configurations/EventAttendeeConfiguration.cs`

```csharp
public class EventAttendeeConfiguration : IEntityTypeConfiguration<EventAttendee>
{
    public void Configure(EntityTypeBuilder<EventAttendee> builder)
    {
        builder.ToTable("EventAttendees");
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.ActivityId);
        builder.HasIndex(e => new { e.AttendeeType, e.AttendeeId });
        builder.HasIndex(e => new { e.ActivityId, e.AttendeeType, e.AttendeeId }).IsUnique();
        
        builder.HasOne(e => e.Activity)
            .WithMany()
            .HasForeignKey(e => e.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Property(e => e.Notes).HasMaxLength(500);
    }
}
```

#### Step 1.4: Generate Migration

```bash
cd CRM.Backend/src/CRM.Infrastructure
dotnet ef migrations add AddEventAttendees -s ../CRM.Api
```

#### Step 1.5: Create API Endpoints

Update `ActivitiesController.cs`:
```csharp
[HttpGet("{id}/attendees")]
public async Task<IActionResult> GetAttendees(int id) { ... }

[HttpPost("{id}/attendees")]
public async Task<IActionResult> AddAttendee(int id, [FromBody] CreateAttendeeDto dto) { ... }

[HttpPut("{id}/attendees/{attendeeId}")]
public async Task<IActionResult> UpdateAttendeeResponse(int id, int attendeeId, [FromBody] UpdateAttendeeDto dto) { ... }

[HttpDelete("{id}/attendees/{attendeeId}")]
public async Task<IActionResult> RemoveAttendee(int id, int attendeeId) { ... }
```

#### Step 1.6: Frontend Updates

Add attendees section to Activity detail view in `ActivitiesPage.tsx`:
- Attendee list with avatar, name, response status
- Add attendee dialog (search users, contacts, leads)
- Response tracking indicators

#### Step 1.7: Tests

Create `CRM.Backend/tests/Entities/EventAttendeeTests.cs`:
- Entity creation/validation
- CRUD operations
- Unique constraint enforcement
- Cascade delete behavior

**Estimated Time:** 2 days

---

### G2: Lead Score Rules UI

**Objective:** Admin UI for configuring lead scoring rules

#### Step 2.1: Create/Verify LeadScoreRule Entity

**File:** `CRM.Backend/src/CRM.Core/Entities/LeadScoreRule.cs`

```csharp
namespace CRM.Core.Entities;

public enum LeadScoreRuleType
{
    Demographic = 0,    // Job title, company size, industry
    Behavioral = 1,     // Email opens, clicks, site visits
    Negative = 2,       // Personal emails, competitors
    Decay = 3           // Inactivity penalty
}

public class LeadScoreRule : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public LeadScoreRuleType RuleType { get; set; }
    
    /// <summary>JSON criteria for rule matching</summary>
    public string Criteria { get; set; } = "{}";
    
    /// <summary>Points to add (can be negative)</summary>
    public int Points { get; set; }
    
    /// <summary>Rule priority (lower = evaluated first)</summary>
    public int Priority { get; set; } = 100;
    
    /// <summary>Is rule active?</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Description for admins</summary>
    [MaxLength(500)]
    public string? Description { get; set; }
}
```

#### Step 2.2: Create LeadScoreRulesController

**File:** `CRM.Backend/src/CRM.Api/Controllers/LeadScoreRulesController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class LeadScoreRulesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() { ... }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id) { ... }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LeadScoreRuleDto dto) { ... }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeadScoreRuleDto dto) { ... }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) { ... }
    
    [HttpPost("{id}/toggle")]
    public async Task<IActionResult> ToggleActive(int id) { ... }
    
    [HttpPost("test")]
    public async Task<IActionResult> TestRules([FromBody] TestScoreDto dto) { ... }
}
```

#### Step 2.3: Create Admin Page

**File:** `CRM.Frontend/src/pages/admin/LeadScoreRulesPage.tsx`

Features:
- List of scoring rules with type icons
- Enable/disable toggle per rule
- Priority reordering (drag-and-drop)
- Rule builder dialog:
  - Rule type selection
  - Criteria builder (field, operator, value)
  - Points input
  - Preview panel

#### Step 2.4: Criteria Builder Component

**File:** `CRM.Frontend/src/components/leads/ScoreRuleCriteriaBuilder.tsx`

Supported criteria:
- **Demographic:** `title CONTAINS "CEO"`, `company_size > 500`, `industry IN [...]`
- **Behavioral:** `email_opens > 3`, `page_visits > 10`, `form_submissions > 0`
- **Negative:** `email CONTAINS "@gmail.com"`, `title CONTAINS "student"`
- **Decay:** `days_inactive > 30`, `decay_rate = 5`

#### Step 2.5: Integrate with AllenAIService

Update `AllenAIService.cs`:
```csharp
public async Task<decimal> CalculateRuleBasedScoreAsync(Lead lead)
{
    var rules = await _context.LeadScoreRules
        .Where(r => r.IsActive && !r.IsDeleted)
        .OrderBy(r => r.Priority)
        .ToListAsync();
        
    decimal score = 0;
    foreach (var rule in rules)
    {
        if (EvaluateCriteria(lead, rule.Criteria))
        {
            score += rule.Points;
        }
    }
    
    return Math.Clamp(score, 0, 100);
}
```

#### Step 2.6: Tests

- Rule CRUD operations
- Criteria evaluation logic
- Score calculation accuracy
- Admin authorization

**Estimated Time:** 3 days

---

### G7: Score Decay Background Job

**Objective:** Automatically reduce lead scores for inactive leads

#### Step 7.1: Create Background Job

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/BackgroundJobs/LeadScoreDecayJob.cs`

```csharp
public class LeadScoreDecayJob : IHostedService, IDisposable
{
    private Timer? _timer;
    private readonly IServiceProvider _serviceProvider;
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Run daily at 2 AM
        _timer = new Timer(ExecuteAsync, null, GetNextRunTime(), TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }
    
    private async void ExecuteAsync(object? state)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
        
        // Get decay rules
        var decayRules = await context.LeadScoreRules
            .Where(r => r.RuleType == LeadScoreRuleType.Decay && r.IsActive)
            .ToListAsync();
            
        foreach (var rule in decayRules)
        {
            var criteria = JsonSerializer.Deserialize<DecayCriteria>(rule.Criteria);
            var cutoffDate = DateTime.UtcNow.AddDays(-criteria.DaysInactive);
            
            // Update inactive leads
            await context.Leads
                .Where(l => l.LastActivityAt < cutoffDate && l.Score > 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(l => l.Score, l => Math.Max(0, l.Score + rule.Points)));
        }
    }
}
```

#### Step 7.2: Register Service

In `Program.cs`:
```csharp
builder.Services.AddHostedService<LeadScoreDecayJob>();
```

#### Step 7.3: Add Settings UI

Add to Settings page:
- Enable/disable decay
- Configure decay rate
- Set inactivity threshold

**Estimated Time:** 1 day

---

## Phase 2: Web Forms (Sprint 2)

### G3: Web Form Builder UI

**Objective:** Visual form builder for lead capture forms

#### Step 3.1: Extend FormDefinition Entity

Ensure `FormDefinition.cs` has:
```csharp
public string EmbedCode { get; set; }  // Generated JS snippet
public string RedirectUrl { get; set; }  // Thank you page
public int? CampaignId { get; set; }  // Link to campaign
public string SubmitEndpoint { get; set; }  // Webhook URL
```

#### Step 3.2: Create Web Forms Page

**File:** `CRM.Frontend/src/pages/admin/WebFormsPage.tsx`

Features:
- List of web forms with status
- Preview button
- Embed code copy button
- Submission stats

#### Step 3.3: Create Form Designer

**File:** `CRM.Frontend/src/components/forms/WebFormDesigner.tsx`

Components:
- Field palette (drag source)
- Form canvas (drop target)
- Field properties panel
- Form settings panel

Field types:
- Text, Email, Phone, Number
- Textarea, Dropdown, Radio, Checkbox
- Date, File upload
- Hidden (for tracking)

#### Step 3.4: Embed Code Generator

```typescript
function generateEmbedCode(form: WebForm): string {
    return `
<script>
(function() {
    var formId = '${form.id}';
    var container = document.getElementById('crm-form-${form.id}');
    var endpoint = '${API_URL}/api/webhooks/forms/${form.id}';
    // ... form rendering and submission logic
})();
</script>
<div id="crm-form-${form.id}"></div>
    `;
}
```

#### Step 3.5: Form Submission Handler

Update `WebhooksController.cs`:
```csharp
[HttpPost("forms/{formId}")]
[AllowAnonymous]
public async Task<IActionResult> SubmitForm(int formId, [FromBody] FormSubmissionDto dto)
{
    var form = await _context.FormDefinitions.FindAsync(formId);
    if (form == null) return NotFound();
    
    // Create lead from submission
    var lead = MapFormToLead(dto, form);
    _context.Leads.Add(lead);
    
    // Trigger workflow if configured
    if (form.WorkflowId.HasValue)
    {
        await _workflowService.TriggerAsync(form.WorkflowId.Value, lead);
    }
    
    await _context.SaveChangesAsync();
    
    return Ok(new { redirectUrl = form.RedirectUrl });
}
```

#### Step 3.6: Tests

- Form creation/editing
- Embed code generation
- Form submission handling
- Lead creation from forms
- CORS handling for embedded forms

**Estimated Time:** 5 days

---

## Phase 3: Calendar Integration (Sprints 3-4)

### G4: Google & Outlook Calendar Sync

**Objective:** Bi-directional calendar sync with external calendars

#### Step 4.1: Install NuGet Packages

```bash
dotnet add package Google.Apis.Calendar.v3
dotnet add package Microsoft.Graph
dotnet add package Azure.Identity
```

#### Step 4.2: Create Calendar Integration Entities

**File:** `CRM.Backend/src/CRM.Core/Entities/CalendarIntegration.cs`

```csharp
public enum CalendarProvider
{
    Google = 0,
    Outlook = 1
}

public class CalendarIntegration : BaseEntity
{
    public int UserId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string AccessToken { get; set; }  // Encrypted
    public string RefreshToken { get; set; }  // Encrypted
    public DateTime TokenExpiresAt { get; set; }
    public string CalendarId { get; set; }  // External calendar ID
    public DateTime LastSyncAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual User? User { get; set; }
}
```

#### Step 4.3: Create Calendar Sync Service

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/CalendarSyncService.cs`

```csharp
public class CalendarSyncService : ICalendarSyncService
{
    public async Task<string> GetGoogleAuthUrlAsync(int userId) { ... }
    public async Task HandleGoogleCallbackAsync(string code, int userId) { ... }
    public async Task SyncGoogleCalendarAsync(int userId) { ... }
    
    public async Task<string> GetOutlookAuthUrlAsync(int userId) { ... }
    public async Task HandleOutlookCallbackAsync(string code, int userId) { ... }
    public async Task SyncOutlookCalendarAsync(int userId) { ... }
    
    public async Task PushEventToExternalAsync(Activity activity, int userId) { ... }
    public async Task PullEventsFromExternalAsync(int userId) { ... }
}
```

#### Step 4.4: Create OAuth Controller

**File:** `CRM.Backend/src/CRM.Api/Controllers/CalendarIntegrationController.cs`

```csharp
[Route("api/calendar")]
public class CalendarIntegrationController : ControllerBase
{
    [HttpGet("connect/google")]
    public async Task<IActionResult> ConnectGoogle() { ... }
    
    [HttpGet("callback/google")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code) { ... }
    
    [HttpGet("connect/outlook")]
    public async Task<IActionResult> ConnectOutlook() { ... }
    
    [HttpGet("callback/outlook")]
    public async Task<IActionResult> OutlookCallback([FromQuery] string code) { ... }
    
    [HttpPost("sync")]
    public async Task<IActionResult> SyncNow() { ... }
    
    [HttpDelete("disconnect/{provider}")]
    public async Task<IActionResult> Disconnect(CalendarProvider provider) { ... }
}
```

#### Step 4.5: Create Settings Page

**File:** `CRM.Frontend/src/pages/settings/CalendarIntegrationPage.tsx`

Features:
- Connect Google Calendar button
- Connect Outlook Calendar button
- Sync status and last sync time
- Manual sync button
- Disconnect option

#### Step 4.6: Background Sync Job

**File:** `CRM.Backend/src/CRM.Infrastructure/Services/BackgroundJobs/CalendarSyncJob.cs`

- Run every 15 minutes
- Sync all active integrations
- Handle token refresh
- Log sync results

#### Step 4.7: Tests

- OAuth flow tests
- Sync logic tests
- Token refresh tests
- Conflict resolution tests

**Estimated Time:** 8 days

---

## Phase 4: Email Sync (Sprints 4-5)

### G5: Gmail & Outlook Email Sync

**Objective:** Sync emails from Gmail/Outlook to CRM communication history

#### Step 5.1: Create Email Integration Entities

Similar to calendar integration:
```csharp
public class EmailIntegration : BaseEntity
{
    public int UserId { get; set; }
    public CalendarProvider Provider { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime LastSyncAt { get; set; }
    public string LastSyncToken { get; set; }  // For incremental sync
    public bool IsActive { get; set; } = true;
}
```

#### Step 5.2: Create Email Sync Service

```csharp
public class EmailSyncService : IEmailSyncService
{
    public async Task SyncGmailAsync(int userId)
    {
        // Fetch emails since last sync
        // Match sender/recipient to contacts/leads
        // Create CommunicationMessage records
        // Handle threading
    }
    
    public async Task SyncOutlookAsync(int userId) { ... }
}
```

#### Step 5.3: Email Matching Logic

```csharp
private async Task<(Contact?, Lead?)> MatchEmailToRecordAsync(string email)
{
    var contact = await _context.ContactDetails
        .FirstOrDefaultAsync(c => c.Email == email);
    if (contact != null) return (contact, null);
    
    var lead = await _context.Leads
        .FirstOrDefaultAsync(l => l.Email == email);
    return (null, lead);
}
```

#### Step 5.4: Tests

- Email sync flow
- Contact/Lead matching
- Threading logic
- Duplicate prevention

**Estimated Time:** 8 days

---

## Phase 5: Landing Pages (Sprints 5-6)

### G6: Landing Page Builder

**Objective:** Create and host marketing landing pages

#### Step 6.1: Create LandingPage Entity

```csharp
public class LandingPage : BaseEntity
{
    public string Name { get; set; }
    public string Slug { get; set; }  // URL path
    public string HtmlContent { get; set; }
    public string CssContent { get; set; }
    public int? FormId { get; set; }
    public int? CampaignId { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int Views { get; set; }
    public int Conversions { get; set; }
}
```

#### Step 6.2: Create Landing Page Controller

```csharp
[Route("lp")]
[AllowAnonymous]
public class LandingPageController : ControllerBase
{
    [HttpGet("{slug}")]
    public async Task<IActionResult> ServeLandingPage(string slug)
    {
        var page = await _context.LandingPages
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
            
        if (page == null) return NotFound();
        
        // Track view
        page.Views++;
        await _context.SaveChangesAsync();
        
        return Content(page.HtmlContent, "text/html");
    }
}
```

#### Step 6.3: Create Visual Builder

**File:** `CRM.Frontend/src/components/landing/LandingPageDesigner.tsx`

Features:
- Template selection
- Block-based editor (header, hero, features, CTA, form, footer)
- Style customization
- Preview mode
- Responsive preview (desktop/tablet/mobile)

#### Step 6.4: Tests

- Page creation
- Slug uniqueness
- View tracking
- Form integration

**Estimated Time:** 10 days

---

## Testing Guidelines

### Test File Naming

- Unit tests: `*Tests.cs`
- Integration tests: `*IntegrationTests.cs`
- E2E tests: `*.spec.ts`

### Test Coverage Targets

| Module | Minimum Coverage |
|--------|------------------|
| Entities | 95% |
| Services | 85% |
| Controllers | 80% |
| Frontend Components | 75% |

### Running Tests

```bash
# Backend tests
cd CRM.Backend/tests
dotnet test --collect:"XPlat Code Coverage"

# Frontend tests
cd CRM.Frontend
npm test -- --coverage

# E2E tests
cd e2e-tests
npx playwright test
```

---

## Deployment Checklist

Before each phase merge:

- [ ] All new tests passing
- [ ] Existing BVT suite passing
- [ ] Code coverage ≥80%
- [ ] No security warnings
- [ ] Database migrations tested
- [ ] API documentation updated
- [ ] Feature flag for new features (if applicable)
- [ ] Rollback plan documented

---

## Rollback Procedures

Each phase should be independently rollback-able:

1. Revert migration (if applicable)
2. Revert code changes
3. Clear any cached data
4. Verify system stability

---

*Document Version: 1.0*  
*Last Updated: 2025-02-04*
