# Email Sequence Module - Schema Alignment Analysis

**Analysis Date:** February 16, 2026  
**Scope:** Entity properties, DTO definitions, DbContext configuration, Service implementation  
**Status:** Critical misalignments found requiring fixes

---

## ENTITY FIXES

### EmailSequence: Add Missing Properties
- `SequenceType` (string?, max 50 chars) - Used by DTOs and service
- `CampaignId` (int?) - Used by DTOs and service for campaign linkage
- `DefaultFromName` (string?, max 100 chars) - Entity has `FromName` but DTOs use `DefaultFromName`
- `DefaultFromEmail` (string?, email format) - Entity has `FromEmail` but DTOs use `DefaultFromEmail`  
- `DefaultReplyTo` (string?, email format) - Entity has `ReplyToEmail` but DTOs use `DefaultReplyTo`

### EmailSequenceStep: Add/Fix Properties
- `StepNumber` (int) - DTOs use this; entity has `StepOrder` (alias exists via `[NotMapped]`)
- Fix property naming: `Body` → properly map to DTO `HtmlContent`; `BodyPlainText` → DTO `TextContent`
- Remove confusion with service code using non-existent `BodyHtml` property

### EmailSequenceEnrollment: Fix Enum Usage
- `Status` property is `EnrollmentStatus` enum, but service code assigns it string values:
  - `"Active"` should be `EnrollmentStatus.Active`
  - `"Paused"` should be `EnrollmentStatus.Paused`
  - `"Completed"` should be `EnrollmentStatus.Completed`
  - `"Exited"` should be `EnrollmentStatus.Removed` or new enum value

### EmailSequence: Fix Status Enum Usage  
- `Status` property is `EmailSequenceStatus` enum, but service code assigns it string values:
  - `"Draft"` should be `EmailSequenceStatus.Draft`
  - `"Active"` should be `EmailSequenceStatus.Active`
  - `"Paused"` should be `EmailSequenceStatus.Paused`

---

## DTO FIXES

### EmailSequenceDtos.cs - Add Missing Properties to EmailSequenceDto
Missing properties referenced in service mapping and analytics:
- `IsActive` (bool) - Entity has this, DTO missing
- `SendingStartHour` (int) - Entity has this, DTO missing
- `SendingEndHour` (int) - Entity has this, DTO missing
- `ExitOnReply` (bool) - Entity has this, DTO missing
- `ExitOnMeetingBooked` (bool) - Entity has this, DTO missing
- `ExitOnBounce` (bool) - Entity has this, DTO missing
- `ExitOnUnsubscribe` (bool) - Entity has this, DTO missing
- Add calculated properties from analytics:
  - `TotalEmailsSent` (int)
  - `TotalOpens` (int)
  - `TotalClicks` (int)
  - `TotalReplies` (int)
  - `TotalBounces` (int)
  - `TotalUnsubscribes` (int)
  - `TotalMeetingsBooked` (int)

### EmailSequenceStepDto - Fix Property Mappings
- Rename: `HtmlContent` → `Body` (align with entity)
- Rename: `TextContent` → `BodyPlainText` (align with entity)
- Add missing properties:
  - `Body` (string) - Main email body
  - `BodyPlainText` (string) - Plain text version
  - `EmailTemplateId` (int?)
  - `IsReply` (bool)
  - `ReplyToStepId` (int?)
  - `TaskTitle` (string?)
  - `TaskDescription` (string?)
  - `TaskPriority` (string?)
  - `TaskDueDays` (int)
  - `ConditionType` (string?)
  - `ConditionValue` (string?)
  - `TrueStepId` (int?)
  - `FalseStepId` (int?)
  - `ExecutionCount` (int)
  - Add `StepOrder` property (entity has this, DTO uses `StepNumber`)

### EmailSequenceEnrollmentDto - Add Missing Properties
- `CurrentStepIndex` (int) - Entity has this
- `TotalOpens` (int) - Entity has this
- `TotalClicks` (int) - Entity has this  
- `HasReplied` (bool) - Entity has this
- `RepliedAt` (DateTime?) - Entity has this
- `HasBounced` (bool) - Entity has this
- `BouncedAt` (DateTime?) - Entity has this
- `HasUnsubscribed` (bool) - Entity has this
- `UnsubscribedAt` (DateTime?) - Entity has this
- `MeetingBooked` (bool) - Entity has this
- `MeetingBookedAt` (DateTime?) - Entity has this
- `RecipientName` (string?) - Entity has this
- `RecipientTimezone` (string?) - Entity has this
- `EnrolledById` (int?) - Entity has this

### EmailSequenceAnalyticsDto - Add Missing Properties
Current service code expects these properties:
- `PausedEnrollments` (int) - Calculated from status
- `CompletedEnrollments` (int) - Calculated from status
- Add existing DTO properties from main file:
  - `TotalOpens` (int)
  - `TotalClicks` (int)
  - `TotalReplies` (int)
  - `TotalBounces` (int)
  - `UnsubscribeCount` (int)
  - `LastExecuted` (DateTime?)
  - `StepAnalytics` (List<StepAnalyticsDto>)

### CreateEmailSequenceDto - Fix Issues
- Service assigns `SequenceType` from DTO to entity (which doesn't have property) ✓ Fixed by adding SequenceType to entity
- Missing property `CampaignId` in DTO - need to add
- Service references `SendingStartHour` and `SendingEndHour` from DTO - verify they're present and types match

### UpdateEmailSequenceDto - Fix Issues  
- Service references `ExitOnReply`, `ExitOnMeetingBooked`, `ExitOnBounce`, `ExitOnUnsubscribe`
- These already exist in DTO ✓

### CreateEmailSequenceStepDto - Fix Property Names
- Remove reference to `BodyHtml` (doesn't exist on entity)
- Change to `Body` for HTML content
- Add `BodyPlainText` property for plain text  
- Verify `StepOrder` property exists (not just `DelayDays`)

### Remove Duplicate DTOs from EmailSequenceManagementService
The following DTOs defined inline in service file should be REMOVED and use main DTOs file:
- `CreateEmailSequenceEnrollmentDto` - Use from EmailSequenceDtos.cs
- `EmailSequenceEnrollmentDto` - Use from EmailSequenceDtos.cs
- `EmailSequenceAnalyticsDto` - Use from EmailSequenceDtos.cs
- `EmailSequenceExecutionResultDto` - Use from EmailSequenceDtos.cs
- `CreateEmailSequenceStepDto` - Use from EmailSequenceDtos.cs
- `EmailSequenceStepDto` - Use from EmailSequenceDtos.cs

---

## DBCONTEXT FIXES

### ICrmDbContext Interface - Verify Properties
✓ Already has DbSets defined:
- `DbSet<EmailSequence> EmailSequences { get; }`
- `DbSet<EmailSequenceStep> EmailSequenceSteps { get; }`
- `DbSet<EmailSequenceEnrollment> EmailSequenceEnrollments { get; }`
- `DbSet<EmailSequenceStepExecution> EmailSequenceStepExecutions { get; }`

### CrmDbContext - Verify DbSets
✓ Already has DbSets defined:
- `public DbSet<EmailSequence> EmailSequences { get; set; }`
- `public DbSet<EmailSequenceStep> EmailSequenceSteps { get; set; }`
- `public DbSet<EmailSequenceEnrollment> EmailSequenceEnrollments { get; set; }`
- `public DbSet<EmailSequenceStepExecution> EmailSequenceStepExecutions { get; set; }`

### CrmDbContext.OnModelCreating - Enhance Entity Configurations
Current configurations in `MarketingConfigurations.cs` are minimal (only HasKey). Need to add:

#### EmailSequenceConfiguration
```csharp
builder.HasKey(e => e.Id);
builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
builder.Property(e => e.Status).HasConversion<string>();
builder.Property(e => e.IsActive).HasDefaultValue(true);
builder.Property(e => e.Timezone).HasMaxLength(50);
builder.Property(e => e.FromName).HasMaxLength(100);
builder.Property(e => e.FromEmail).HasMaxLength(255);
builder.Property(e => e.ReplyToEmail).HasMaxLength(255);
builder.Property(e => e.SendingDays).HasMaxLength(500);
builder.Property(e => e.ExitConditions).HasMaxLength(1000);
builder.HasOne(e => e.Owner).WithMany().HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.SetNull);
builder.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.SetNull);
builder.HasMany(e => e.Steps).WithOne(s => s.EmailSequence).HasForeignKey(s => s.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
builder.HasMany(e => e.Enrollments).WithOne(en => en.EmailSequence).HasForeignKey(en => en.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
```

#### EmailSequenceStepConfiguration
```csharp
builder.HasKey(e => e.Id);
builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
builder.Property(e => e.StepType).HasConversion<string>();
builder.Property(e => e.TimingMode).HasConversion<string>();
builder.Property(e => e.IsActive).HasDefaultValue(true);
builder.Property(e => e.Subject).HasMaxLength(500);
builder.Property(e => e.Body).HasColumnType("LONGTEXT");
builder.Property(e => e.BodyPlainText).HasColumnType("LONGTEXT");
builder.Property(e => e.SpecificTime).HasMaxLength(50);
builder.Property(e => e.TaskTitle).HasMaxLength(255);
builder.Property(e => e.TaskDescription).HasColumnType("LONGTEXT");
builder.Property(e => e.TaskPriority).HasMaxLength(50);
builder.Property(e => e.ConditionType).HasMaxLength(100);
builder.Property(e => e.ConditionValue).HasMaxLength(1000);
builder.Property(e => e.ABVariant).HasMaxLength(10);
builder.HasOne(e => e.EmailSequence).WithMany(s => s.Steps).HasForeignKey(e => e.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
```

#### EmailSequenceEnrollmentConfiguration
```csharp
builder.HasKey(e => e.Id);
builder.Property(e => e.Status).HasConversion<string>();
builder.Property(e => e.ExitReason).HasConversion<string>();
builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
builder.Property(e => e.RecipientName).HasMaxLength(255);
builder.Property(e => e.RecipientTimezone).HasMaxLength(50);
builder.Property(e => e.ExitNotes).HasMaxLength(1000);
builder.HasOne(e => e.EmailSequence).WithMany(s => s.Enrollments).HasForeignKey(e => e.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(e => e.Lead).WithMany().HasForeignKey(e => e.LeadId).OnDelete(DeleteBehavior.SetNull);
builder.HasOne(e => e.Contact).WithMany().HasForeignKey(e => e.ContactId).OnDelete(DeleteBehavior.SetNull);
builder.HasOne(e => e.EnrolledBy).WithMany().HasForeignKey(e => e.EnrolledById).OnDelete(DeleteBehavior.SetNull);
builder.HasMany(e => e.StepExecutions).WithOne(se => se.EmailSequenceEnrollment).HasForeignKey(se => se.EmailSequenceEnrollmentId).OnDelete(DeleteBehavior.Cascade);
```

#### EmailSequenceStepExecutionConfiguration
```csharp
builder.HasKey(e => e.Id);
builder.Property(e => e.MessageId).HasMaxLength(255);
builder.Property(e => e.BounceType).HasMaxLength(100);
builder.Property(e => e.ErrorMessage).HasMaxLength(1000);
builder.HasOne(e => e.EmailSequenceStep).WithMany().HasForeignKey(e => e.EmailSequenceStepId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(e => e.EmailSequenceEnrollment).WithMany(en => en.StepExecutions).HasForeignKey(e => e.EmailSequenceEnrollmentId).OnDelete(DeleteBehavior.Cascade);
```

---

## SPECIFIC ISSUES FOUND

### Issue 1: Property Name Mismatches Between Entity and DTO
**Location:** EmailSequenceManagementService.cs (lines 72-77)
**Problem:** Service assigns:
```csharp
DefaultFromName = dto.DefaultFromName,  // But entity property is FromName
DefaultFromEmail = dto.DefaultFromEmail,  // But entity property is FromEmail
DefaultReplyTo = dto.DefaultReplyTo,  // But entity property is ReplyToEmail
```
**Impact:** Code won't compile or will fail at runtime with null reference

### Issue 2: String Status Assignment to Enum Field
**Location:** EmailSequenceManagementService.cs (line 72, 251, 285, 301, 318, 358)
**Problem:** Service code:
```csharp
Status = "Draft"  // EmailSequence.Status is enum EmailSequenceStatus
Status = "Active"  // EmailSequenceEnrollment.Status is enum EnrollmentStatus
enrollment.Status = "Paused"  // Should be enum
```
**Impact:** Type mismatch - will cause compilation error

### Issue 3: BodyHtml Property Doesn't Exist
**Location:** EmailSequenceManagementService.cs (lines 154, 166)
**Problem:** Service code:
```csharp
Subject = dto.Subject,
BodyHtml = dto.BodyHtml,  // Property "BodyHtml" doesn't exist on EmailSequenceStep
```
Entity has `Body` and `BodyPlainText`, not `BodyHtml`
**Impact:** Compilation error - undefined member

### Issue 4: SequenceType Property Missing from Entity
**Location:** EmailSequenceManagementService.cs (line 73)
**Problem:** Service assigns `SequenceType = dto.SequenceType` but entity doesn't have this property
**Impact:** Compilation error - undefined member

### Issue 5: Incomplete DTO Mapping in Service
**Location:** EmailSequenceManagementService.cs (MapToDto method, lines ~470+)
**Problem:** DTO mapper doesn't map all entity properties to DTO:
```csharp
private EmailSequenceDto MapToDto(EmailSequence sequence)
{
    return new EmailSequenceDto
    {
        Id = sequence.Id,
        Name = sequence.Name,
        Description = sequence.Description,
        Status = sequence.Status,  // Enum to string conversion missing
        // Missing: IsActive, Timezone, FromName, FromEmail, ReplyToEmail, etc.
    };
}
```
**Impact:** Data loss on mapping; DTOs missing values; frontend won't display all properties

### Issue 6: Status Enum to String Comparison in Analytics
**Location:** EmailSequenceManagementService.cs (lines 341-343)
**Problem:** Code compares enum to string:
```csharp
ActiveEnrollments = enrollments.Count(e => e.Status == "Active"),  // Status is enum, "Active" is string
PausedEnrollments = enrollments.Count(e => e.Status == "Paused"),
CompletedEnrollments = enrollments.Count(e => e.Status == "Completed"),
```
**Impact:** LINQ comparison fails - returns 0 for all counts

### Issue 7: Duplicate/Conflicting DTO Definitions
**Location:** EmailSequenceManagementService.cs (lines ~475+)
**Problem:** Service file defines:
- `CreateEmailSequenceEnrollmentDto` (with only `ContactId`)
- `EmailSequenceEnrollmentDto` (incomplete)
- `EmailSequenceAnalyticsDto` (different properties than main file)
- `EmailSequenceExecutionResultDto` (missing properties)
- `CreateEmailSequenceStepDto` (using `BodyHtml` not `Body`)
- `EmailSequenceStepDto` (incomplete)

These conflict with definitions in `EmailSequenceDtos.cs`
**Impact:** Confusion about which DTO to use; conflicting mapping logic; type mismatches

### Issue 8: Missing Enum Conversion in OnModelCreating
**Location:** MarketingConfigurations.cs - EmailSequence configuration
**Problem:** Enum properties need explicit conversion for EF Core:
```csharp
builder.Property(e => e.Status).HasConversion<string>();
builder.Property(e => e.ExitReason).HasConversion<string>();
```
**Impact:** Database may serialize enums incorrectly; queries may fail

### Issue 9: StepNumber vs StepOrder Inconsistency
**Location:** EmailSequenceStepDto uses `StepNumber`, Entity uses `StepOrder`
**Problem:** DTOs in both files inconsistent:
- EmailSequenceDtos.cs: `public int StepNumber { get; set; }`
- EmailSequenceStep entity: `public int StepOrder { get; set; }`
- Service map: `StepOrder = step.StepOrder` (correct but DTO property is `StepNumber`)
**Impact:** DTO mapping mismatch; data not properly reflected in API responses

### Issue 10: Missing CampaignId on Entity
**Location:** EmailSequenceManagementService.cs (line 77)
**Problem:** Service assigns `CampaignId = dto.CampaignId` but entity doesn't have this property
**Note:** This might be intentional if campaign linkage isn't needed, but either:
- Add it to the entity, or
- Remove from DTOs and service
**Impact:** Compilation error or data loss

---

## SUMMARY TABLE

| Component | Issue | Severity | Entity Fix | DTO Fix | Service Fix|
|-----------|-------|----------|-----------|---------|-----------|
| EmailSequence | Missing `SequenceType` property | High | Add property | ✓ Already in DTO | Reference new property |
| EmailSequence | Missing `CampaignId` property | High | Add property | ✓ Already in DTO | Reference new property |
| EmailSequence | Mismatch: `FromName` vs `DefaultFromName` | High | Keep as-is | Fix DTO mapping | Fix mapper method |
| EmailSequence | Mismatch: `FromEmail` vs `DefaultFromEmail` | High | Keep as-is | Fix DTO mapping | Fix mapper method |
| EmailSequence | Mismatch: `ReplyToEmail` vs `DefaultReplyTo` | High | Keep as-is | Fix DTO mapping | Fix mapper method |
| EmailSequence | Status as enum, service uses string | High | ✓ Correct | ✓ Correct | Convert string to enum |
| EmailSequenceStep | Property mismatch: `BodyHtml` → `Body` | High | ✓ Correct | Rename property | Fix mapper |
| EmailSequenceStep | Missing `StepNumber` | Medium | Already `StepOrder` | Standardize name | Use correct property |
| EmailSequenceEnrollment | Status as enum, service uses string | High | ✓ Correct | ✓Correct | Convert string to enum |
| EmailSequenceEnrollment | Missing engagement properties | Medium | ✓ Has them | Add to DTO | Use in mapper |
| DTOs | Duplicate definitions in service | Medium | N/A | Remove inline DTOs | Import from main file |
| Config | Minimal OnModelCreating | Medium | N/A | N/A | Add proper configuration |

---

## IMPLEMENTATION PRIORITY

**Phase 1 (Critical - Will cause build failures):**
1. Fix string-to-enum status assignments in EmailSequenceManagementService
2. Fix `BodyHtml` → `Body` property reference
3. Add `SequenceType` property to EmailSequence entity
4. Add `CampaignId` property to EmailSequence entity or remove from DTOs
5. Fix property name mismatches (`FromName`, `FromEmail`, `ReplyToEmail`)

**Phase 2 (High - Will cause runtime errors):**
1. Complete DTO property mappings in service mappers
2. Fix enum comparisons in analytics queries
3. Remove duplicate DTOs from service file
4. Enhance OnModelCreating configurations with relationships

**Phase 3 (Medium - Data quality):**
1. Standardize `StepNumber` vs `StepOrder` naming
2. Add missing engagement properties to DTOs
3. Add proper property constraints and database column types
4. Add validation logic for required fields
