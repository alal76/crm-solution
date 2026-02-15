# Phase 2 Implementation Guide

> This document provides templates and guidance for Phase 2 service implementations.  
> Phase 2 Effort: ~28 hours to complete all services, controllers, and tests.

---

## Template 1: Email Sequence Service Implementation

**File Location:** `CRM.Backend/src/CRM.Infrastructure/Services/EmailSequenceManagementService.cs`  
**Estimated Effort:** 4 hours

```csharp
// TEMPLATE - Use this as a starting point
public class EmailSequenceManagementService : IEmailSequenceManagementService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<EmailSequenceManagementService> _logger;
    private readonly IEmailTemplateService _templateService;

    public EmailSequenceManagementService(
        ICrmDbContext context,
        ILogger<EmailSequenceManagementService> logger,
        IEmailTemplateService templateService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _templateService = templateService;
    }

    #region Sequence CRUD

    public async Task<IEnumerable<EmailSequenceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sequences = await _context.EmailSequences
            .Include(s => s.Steps)
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return sequences.Select(s => MapToDto(s)).ToList();
    }

    public async Task<EmailSequenceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sequence = await _context.EmailSequences
            .Include(s => s.Steps)
            .Include(s => s.Enrollments)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        return sequence != null ? MapToDto(sequence) : null;
    }

    public async Task<EmailSequenceDto> CreateAsync(CreateEmailSequenceDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Name is required", nameof(dto));

        var sequence = new EmailSequence
        {
            Name = dto.Name,
            Description = dto.Description,
            DefaultFromName = dto.DefaultFromName,
            DefaultFromEmail = dto.DefaultFromEmail,
            DefaultReplyTo = dto.DefaultReplyTo,
            OwnerId = dto.OwnerId,
            CampaignId = dto.CampaignId,
            Status = EmailSequenceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EmailSequences.Add(sequence);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created email sequence {SequenceId} named '{SequenceName}'", sequence.Id, sequence.Name);

        return MapToDto(sequence);
    }

    // TODO: Implement Update, Delete methods
    // TODO: Implement Step management methods
    // TODO: Implement Enrollment methods
    // TODO: Implement Analytics aggregation

    #endregion

    #region Mapper

    private EmailSequenceDto MapToDto(EmailSequence sequence)
    {
        return new EmailSequenceDto
        {
            Id = sequence.Id,
            Name = sequence.Name,
            Description = sequence.Description,
            Status = sequence.Status.ToString(),
            TotalEnrolled = sequence.TotalEnrolled,
            TotalCompleted = sequence.TotalCompleted,
            TotalActive = sequence.TotalActive,
            OpenRate = sequence.OpenRate,
            ClickRate = sequence.ClickRate,
            ReplyRate = sequence.ReplyRate,
            ConversionRate = sequence.ConversionRate,
            DefaultFromName = sequence.DefaultFromName,
            DefaultFromEmail = sequence.DefaultFromEmail,
            DefaultReplyTo = sequence.DefaultReplyTo,
            CreatedAt = sequence.CreatedAt,
            UpdatedAt = sequence.UpdatedAt,
            Steps = sequence.Steps?.Select(s => MapStepToDto(s)).ToList() ?? new()
        };
    }

    private EmailSequenceStepDto MapStepToDto(EmailSequenceStep step)
    {
        return new EmailSequenceStepDto
        {
            Id = step.Id,
            SequenceId = step.SequenceId,
            StepNumber = step.StepNumber,
            StepType = step.Email?.StepType ?? step.LinkedTaskId.HasValue ? "Task" : "Email",
            Name = step.Email?.Subject ?? step.LinkedTaskId?.ToString() ?? "Unknown",
            Subject = step.Email?.Subject,
            HtmlContent = step.Email?.HtmlContent,
            TextContent = step.Email?.TextContent,
            TemplateId = step.Email?.TemplateId,
            DelayDays = step.DelayDays,
            DelayHours = step.DelayHours,
            DelayMinutes = step.DelayMinutes,
            TimingMode = step.TimingMode,
            SpecificTime = step.SpecificTime,
            SendOnWeekends = step.SendOnWeekends,
            IsABTest = step.IsABTest,
            ABVariant = step.ABVariant,
            ABTestPercentage = step.ABTestPercentage,
            TotalSent = step.TotalSent,
            TotalOpened = step.TotalOpened,
            TotalClicked = step.TotalClicked,
            TotalReplied = step.TotalReplied,
            IsActive = step.IsActive,
            CreatedAt = step.CreatedAt,
            UpdatedAt = step.UpdatedAt
        };
    }

    #endregion
}
```

---

## Template 2: Webhook Management Service Implementation

**File Location:** `CRM.Backend/src/CRM.Infrastructure/Services/WebhookManagementService.cs`  
**Estimated Effort:** 6 hours

### Key Implementation Points:
1. HMAC-SHA256 signature generation for webhook payloads
2. Exponential backoff retry logic
3. Delivery tracking with status history
4. Dead webhook detection (5+ consecutive failures)
5. URL validation (HTTPS requirement, localhost exception)

### Implementation Strategy:
```csharp
// Generate HMAC signature
private string GenerateSignature(string payload, string secret)
{
    using (var hmac = new System.Security.Cryptography.HMACSHA256(
        System.Text.Encoding.UTF8.GetBytes(secret)))
    {
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        return "sha256=" + System.BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

// Calculate retry delay with exponential backoff
private int GetRetryDelaySeconds(int attemptNumber, int baseInterval)
{
    return baseInterval * (int)Math.Pow(2, attemptNumber - 1);
}
```

---

## Template 3: Campaign Execution Service Implementation

**File Location:** `CRM.Backend/src/CRM.Infrastructure/Services/CampaignExecutionService.cs`  
**Estimated Effort:** 3 hours

### Key Method Signatures:
```csharp
public async Task<CampaignExecutionResultDto> ExecuteAsync(int campaignId, CancellationToken cancellationToken)
{
    // 1. Validate campaign status (Draft → Active)
    // 2. Get all recipients
    // 3. Send/dispatch to each recipient
    // 4. Track success/failure metrics
    // 5. Update campaign status to Active
    // 6. Return execution result
}

public async Task<bool> PauseAsync(int campaignId, CancellationToken cancellationToken)
{
    // Set campaign status to Paused
    // Pause ongoing recipient sends
}
```

---

## Template 4: Commission Calculation Service Implementation

**File Location:** `CRM.Backend/src/CRM.Infrastructure/Services/CommissionCalculationService.cs`  
**Estimated Effort:** 4 hours

### Key Calculations:
```csharp
public async Task<decimal> ApplyTierAsync(int planId, decimal amount, CancellationToken cancellationToken)
{
    // 1. Get plan and tiers
    // 2. Find applicable tier based on amount
    // 3. Return: amount * tier.Rate
}

public async Task<decimal> ApplyAcceleratorAsync(
    int planId, 
    decimal baseAmount, 
    decimal achievementPercent, 
    CancellationToken cancellationToken)
{
    // 1. Get plan accelerator settings
    // 2. Calculate bonus based on achievement %
    // 3. Return: baseAmount + bonus
}
```

---

## Template 5: Controller Enhancement

**File Location:** `CRM.Backend/src/CRM.Api/Controllers/{Feature}Controller.cs`  
**Estimated Effort:** 2 hours per controller

### Adding Batch Operations:
```csharp
[HttpPost("batch/approve")]
public async Task<IActionResult> BulkApprove([FromBody] List<int> ids, CancellationToken ct)
{
    try
    {
        var count = await _approvalService.BulkApproveAsync(ids, UserId, ct);
        return Ok(new { message = $"Approved {count} items" });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Bulk approval failed");
        return StatusCode(500, "Internal server error");
    }
}
```

---

## Template 6: DI Registration

**Location:** `CRM.Backend/src/CRM.Api/Program.cs`

```csharp
// Add these service registrations
services.AddScoped<IEmailSequenceManagementService, EmailSequenceManagementService>();
services.AddScoped<IWebhookManagementService, WebhookManagementService>();
services.AddScoped<IWebhookDispatcherService, WebhookDispatcherService>();
services.AddScoped<ICampaignExecutionService, CampaignExecutionService>();
services.AddScoped<ICampaignRecipientService, CampaignRecipientService>();
services.AddScoped<ICampaignMetricsService, CampaignMetricsService>();
services.AddScoped<ICommissionCalculationService, CommissionCalculationService>();
services.AddScoped<ICommissionApprovalService, CommissionApprovalService>();
services.AddScoped<ICommissionPayoutService, CommissionPayoutService>();

// Add hosted services for background processing
services.AddHostedService<WebhookQueueProcessorService>();
services.AddHostedService<EmailSequenceExecutionService>();
services.AddHostedService<CommissionStatementGeneratorService>();
```

---

## Implementation Checklist

### For Each Service Implementation:

- [ ] Create service class inheriting from interface
- [ ] Add constructor with all dependencies
- [ ] Implement all interface methods
- [ ] Add XML documentation to all methods
- [ ] Add proper error logging
- [ ] Handle CancellationToken in all async operations
- [ ] Use _context.SaveChangesAsync(cancellationToken)
- [ ] Implement soft delete pattern (IsDeleted = true)
- [ ] Create mapper methods for DTOs
- [ ] Add validation for input DTOs
- [ ] Write unit tests (2-3 tests per major method)
- [ ] Write integration tests with mocked DB context

### For Each Controller Enhancement:

- [ ] Create end-to-end endpoint handler
- [ ] Add [HttpGet/Post/Put/Delete] attributes
- [ ] Add [ProducesResponseType] documentation
- [ ] Implement proper status codes (200, 201, 400, 404, 500)
- [ ] Add parameter validation
- [ ] Add service exception handling
- [ ] Add logging for important operations
- [ ] Add pagination support where applicable
- [ ] Write controller tests (happy path + error scenarios)

---

## Quick Implementation Order

**Recommended sequence for Phase 2 (28 hours):**

1. **Day 1** (8 hours)
   - EmailSequenceManagementService (4 hours)
   - Enhance EmailSequencesController (2 hours)
   - Write EmailSequence tests (2 hours)

2. **Day 2** (8 hours)
   - WebhookManagementService (5 hours)
   - Create WebhooksController (2 hours)
   - Write webhook tests (1 hour)

3. **Day 3** (8 hours)
   - CampaignExecutionService + CampaignRecipientService (4 hours)
   - Enhance CampaignsController (2 hours)
   - Write campaign tests (2 hours)

4. **Day 4** (6 hours)
   - CommissionCalculationService + CommissionApprovalService (3 hours)
   - Enhance CommissionsController (2 hours)
   - DI registration + final testing (1 hour)

---

## Testing Templates

### Service Unit Test Template:
```csharp
[Fact]
public async Task CreateAsync_WithValidDto_ReturnsDto()
{
    // Arrange
    var dto = new CreateXxxDto { /* valid data */ };
    var service = new XxxService(_mockContext.Object, _mockLogger.Object);

    // Act
    var result = await service.CreateAsync(dto, CancellationToken.None);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(dto.Name, result.Name);
    _mockContext.Verify(x => x.SaveChangesAsync(), Times.Once);
}
```

### Controller Test Template:
```csharp
[Fact]
public async Task PostXxx_WithValidDto_Returns201()
{
    // Arrange
    var dto = new CreateXxxDto { /* valid data */ };
    var controller = new XxxController(_mockService.Object, _mockLogger.Object);

    // Act
    var result = await controller.Create(dto, CancellationToken.None);

    // Assert
    Assert.IsType<CreatedAtActionResult>(result);
    var createdResult = result as CreatedAtActionResult;
    Assert.Equal(nameof(controller.GetById), createdResult.ActionName);
}
```

---

## Key Files Reference

| Phase 2 Task | Primary File | Secondary Files | Estimated Time |
|--------------|--------------|-----------------|-----------------|
| Email Sequences | EmailSequenceManagementService.cs | EmailSequencesController.cs, Tests | 4 hours |
| Webhooks | WebhookManagementService.cs | WebhooksController.cs, WebhookDispatcher.cs, Tests | 6 hours |
| Campaigns | CampaignExecutionService.cs | CampaignRecipientService.cs, CampaignMetricsService.cs, CampaignsController.cs, Tests | 8 hours |
| Commissions | CommissionCalculationService.cs | CommissionApprovalService.cs, CommissionPayoutService.cs, CommissionsController.cs, Tests | 10 hours |
| Configuration | Program.cs | appsettings.json | 2 hours |
| Database | Migrations | Seed data updates | 2 hours |

---

## Use-Case Examples for Testing

### Email Sequence: Send daily digest
```
1. Create sequence: Daily digest email
2. Add 5 steps (Day 1-5 after enrollment)
3. Enroll 3 contacts
4. Execute sequence daily for 5 days
5. Verify 15 emails sent (3 contacts × 5 days)
6. Assert correct contact has correct step each day
```

### Campaign: Q1 product launch
```
1. Create campaign: Q1 product launch
2. Add 1,000 recipients from segment
3. Execute campaign (send to all)
4. Track opens/clicks over 30 days
5. Calculate ROI: (conversions × deal_value) / spend
6. Verify metrics aggregation
```

### Commission: Quarterly payout
```
1. Create commission plan: 10% base, tier up to 15%
2. Create 5 commissions from Q1 deals
3. Calculate commission using tiers
4. Bulk approve all 5
5. Generate quarterly statement
6. Mark as paid
7. Assert total_paid = sum(final_amounts)
```

---

## Common Pitfalls to Avoid

❌ **Don't:** Hard code default values
✅ **Do:** Use configuration or method parameters

❌ **Don't:** Forget CancellationToken in SaveChangesAsync
✅ **Do:** `await _context.SaveChangesAsync(cancellationToken)`

❌ **Don't:** Query entire dataset then filter in code
✅ **Do:** Use LINQ Where() before ToListAsync()

❌ **Don't:** Catch all exceptions silently
✅ **Do:** Log and rethrow or return meaningful error

❌ **Don't:** Mix DTOs and entities in response
✅ **Do:** Always return DTOs from controllers

❌ **Don't:** Skip null checks on dependencies
✅ **Do:** Throw ArgumentNullException in constructor

---

## Success Indicators

✅ When Phase 2 is complete:
- All 9 services fully implemented
- All controllers have all required endpoints
- All 50+ new tests passing
- Zero compile errors
- DI registration complete
- Code review approved
- Ready for E2E testing

---

**Document Version:** 1.0  
**Created:** February 15, 2026  
**For Phase 2 Implementation**
