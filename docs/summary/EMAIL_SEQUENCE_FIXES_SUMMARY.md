# Email Sequence Module - Schema Alignment Fixes Summary

**Analysis Date:** February 16, 2026  
**Component:** EmailSequence, EmailSequenceStep, EmailSequenceEnrollment, EmailSequenceStepExecution

---

## ENTITY FIXES

### EmailSequence: Add Missing Properties
- `SequenceType` (string?, max 50) - Referenced in DTOs and service  
- `CampaignId` (int?) - Referenced in DTOs and service
- `DefaultFromName` (string?, max 100) - DTO uses "DefaultFromName" but entity has `FromName`
- `DefaultFromEmail` (string?, email) - DTO uses "DefaultFromEmail" but entity has `FromEmail`
- `DefaultReplyTo` (string?, email) - DTO uses "DefaultReplyTo" but entity has `ReplyToEmail`

**Note:** Consider keeping both names or standardizing on one. Currently entity has `FromName`/`FromEmail`/`ReplyToEmail` but DTOs expect `DefaultFromName`/`DefaultFromEmail`/`DefaultReplyTo`.

### EmailSequenceStep: Fix Properties
- Fix `Subject` (exists, OK)
- `Body` property (entity has this, DTOs call it `HtmlContent`) 
- Add property: `BodyHtml` doesn't exist - service code references it but should be `Body`
- Add property: Entity doesn't have `StepNumber` (has `StepOrder` instead)

### EmailSequenceEnrollment: Fix Enum Usage  
- Property `Status` is type `EnrollmentStatus` (enum)
- Service code assigns: `"Active"`, `"Paused"`, `"Completed"`, `"Exited"` (string values)
- Should be: `EnrollmentStatus.Active`, `EnrollmentStatus.Paused`, `EnrollmentStatus.Completed`, etc.

### EmailSequence: Fix Status Type
- Property `Status` is type `EmailSequenceStatus` (enum)
- Service code assigns: `"Draft"`, `"Active"` (string values)
- Should be: `EmailSequenceStatus.Draft`, `EmailSequenceStatus.Active`, etc.

---

## DTO FIXES

### EmailSequenceDtos.cs - EmailSequenceDto: Add Missing Properties
- `IsActive` (bool)
- `SendingStartHour` (int)
- `SendingEndHour` (int)
- `ExitOnReply` (bool)
- `ExitOnMeetingBooked` (bool)
- `ExitOnBounce` (bool)
- `ExitOnUnsubscribe` (bool)
- `TotalEmailsSent` (int)
- `TotalOpens` (int)
- `TotalClicks` (int)
- `TotalReplies` (int)
- `TotalBounces` (int)
- `TotalUnsubscribes` (int)
- `TotalMeetingsBooked` (int)

### EmailSequenceStepDto: Fix/Add Properties
- Rename: `HtmlContent` → `Body` (align with entity field names)
- Rename: `TextContent` → `BodyPlainText` (align with entity)
- Add: `EmailTemplateId` (int?)
- Add: `IsReply` (bool)
- Add: `ReplyToStepId` (int?)
- Add: `TaskTitle` (string?)
- Add: `TaskDescription` (string?)
- Add: `TaskPriority` (string?)
- Add: `TaskDueDays` (int)
- Add: `ConditionType` (string?)
- Add: `ConditionValue` (string?)
- Add: `TrueStepId` (int?)
- Add: `FalseStepId` (int?)
- Add: `ExecutionCount` (int)
- **Fix Rename:** `StepNumber` should align with entity `StepOrder` (or add both)

### EmailSequenceEnrollmentDto: Add Missing Properties
- `CurrentStepIndex` (int)
- `TotalOpens` (int)
- `TotalClicks` (int)
- `HasReplied` (bool)
- `RepliedAt` (DateTime?)
- `HasBounced` (bool)
- `BouncedAt` (DateTime?)
- `HasUnsubscribed` (bool)
- `UnsubscribedAt` (DateTime?)
- `MeetingBooked` (bool)
- `MeetingBookedAt` (DateTime?)
- `RecipientName` (string?)
- `RecipientTimezone` (string?)
- `EnrolledById` (int?)

### EmailSequenceAnalyticsDto: Add Missing Properties
- `PausedEnrollments` (int) - Calculated field
- `CompletedEnrollments` (int) - Calculated field
- Verify existing: `TotalOpens`, `TotalClicks`, `TotalReplies`, `TotalBounces`, `UnsubscribeCount`, `LastExecuted`, `StepAnalytics`

### CreateEmailSequenceStepDto: Fix Property
- Change `BodyHtml` → `Body` (entity property name)
- Verify `StepOrder` is included (not just delay fields)
- Add `Name` property requirement

### **REMOVE Duplicate DTOs from EmailSequenceManagementService.cs**
The service file defines duplicate DTOs that conflict with EmailSequenceDtos.cs:
- Delete: Local `CreateEmailSequenceEnrollmentDto` 
- Delete: Local `EmailSequenceEnrollmentDto`
- Delete: Local `EmailSequenceAnalyticsDto`
- Delete: Local `EmailSequenceExecutionResultDto`
- Delete: Local `CreateEmailSequenceStepDto`
- Delete: Local `EmailSequenceStepDto`
- **Use imports from EmailSequenceDtos.cs instead**

---

## DBCONTEXT FIXES

### ICrmDbContext: Verify DbSets Present
✓ **CONFIRMED** - Already defined:
```csharp
DbSet<EmailSequence> EmailSequences { get; }
DbSet<EmailSequenceStep> EmailSequenceSteps { get; }
DbSet<EmailSequenceEnrollment> EmailSequenceEnrollments { get; }  
DbSet<EmailSequenceStepExecution> EmailSequenceStepExecutions { get; }
```

### CrmDbContext.OnModelCreating: Configure Entities
**CURRENT STATE:** MarketingConfigurations.cs has minimal configuration (only HasKey)

**ADD Configuration for:**

#### EmailSequenceConfiguration - Add
```csharp
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

#### EmailSequenceStepConfiguration - Add
```csharp
builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
builder.Property(e => e.StepType).HasConversion<string>();
builder.Property(e => e.TimingMode).HasConversion<string>();
builder.Property(e => e.IsActive).HasDefaultValue(true);
builder.Property(e => e.Subject).HasMaxLength(500);
builder.Property(e => e.Body).HasColumnType("LONGTEXT");
builder.Property(e => e.BodyPlainText).HasColumnType("LONGTEXT");
builder.Property(e => e.SpecificTime).HasMaxLength(50);
builder.Property(e => e.ABVariant).HasMaxLength(10);
builder.HasOne(e => e.EmailSequence).WithMany(s => s.Steps).HasForeignKey(e => e.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
```

#### EmailSequenceEnrollmentConfiguration - Add
```csharp
builder.Property(e => e.Status).HasConversion<string>();
builder.Property(e => e.ExitReason).HasConversion<string>();
builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
builder.Property(e => e.RecipientName).HasMaxLength(255);
builder.Property(e => e.RecipientTimezone).HasMaxLength(50);
builder.HasOne(e => e.EmailSequence).WithMany(s => s.Enrollments).HasForeignKey(e => e.EmailSequenceId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(e => e.Lead).WithMany().HasForeignKey(e => e.LeadId).OnDelete(DeleteBehavior.SetNull);
builder.HasOne(e => e.Contact).WithMany().HasForeignKey(e => e.ContactId).OnDelete(DeleteBehavior.SetNull);
builder.HasOne(e => e.EnrolledBy).WithMany().HasForeignKey(e => e.EnrolledById).OnDelete(DeleteBehavior.SetNull);
builder.HasMany(e => e.StepExecutions).WithOne(se => se.EmailSequenceEnrollment).HasForeignKey(se => se.EmailSequenceEnrollmentId).OnDelete(DeleteBehavior.Cascade);
```

#### EmailSequenceStepExecutionConfiguration - Add
```csharp
builder.Property(e => e.MessageId).HasMaxLength(255);
builder.Property(e => e.BounceType).HasMaxLength(100);
builder.Property(e => e.ErrorMessage).HasMaxLength(1000);
builder.HasOne(e => e.EmailSequenceStep).WithMany().HasForeignKey(e => e.EmailSequenceStepId).OnDelete(DeleteBehavior.Cascade);
builder.HasOne(e => e.EmailSequenceEnrollment).WithMany(en => en.StepExecutions).HasForeignKey(e => e.EmailSequenceEnrollmentId).OnDelete(DeleteBehavior.Cascade);
```

---

## SPECIFIC ISSUES FOUND

### Issue #1: String Status Values Should Be Enums
**Files:** EmailSequenceManagementService.cs (lines 72, 251, 285, 301, 318, 358)  
**Current Code:**
```csharp
Status = "Draft"              // Should be EmailSequenceStatus.Draft
enrollment.Status = "Active"  // Should be EnrollmentStatus.Active  
enrollment.Status = "Paused"  // Should be EnrollmentStatus.Paused
enrollment.Status = "Exited"  // Should be EnrollmentStatus.Removed
```
**Severity:** HIGH - Will cause type mismatch errors

### Issue #2: Non-Existent Property Reference
**File:** EmailSequenceManagementService.cs (lines 154, 166)  
**Current Code:**
```csharp
BodyHtml = dto.BodyHtml  // Property "BodyHtml" doesn't exist on entity
```
**Reality:** Entity has `Body` and `BodyPlainText`  
**Severity:** HIGH - Will cause compilation error

### Issue #3: Non-Existent Property References
**File:** EmailSequenceManagementService.cs (lines 72-77)  
**Current Code:**
```csharp
DefaultFromName = dto.DefaultFromName    // Entity has "FromName"
DefaultFromEmail = dto.DefaultFromEmail  // Entity has "FromEmail"
DefaultReplyTo = dto.DefaultReplyTo      // Entity has "ReplyToEmail"
SequenceType = dto.SequenceType          // Entity doesn't have this property
```
**Severity:** HIGH - Will cause compilation error (unless properties added to entity)

### Issue #4: Enum Comparison with String Fails
**File:** EmailSequenceManagementService.cs (lines 341-343)  
**Current Code:**
```csharp
ActiveEnrollments = enrollments.Count(e => e.Status == "Active")
PausedEnrollments = enrollments.Count(e => e.Status == "Paused")
CompletedEnrollments = enrollments.Count(e => e.Status == "Completed")
```
**Reality:** Status is `EnrollmentStatus` enum, not string  
**Impact:** Always returns 0 - breaks analytics  
**Severity:** HIGH - Silent data loss

### Issue #5: Property Name Mismatch in DTO
**Files:** EmailSequenceStepDto uses `StepNumber`, Entity has `StepOrder`  
**Impact:** Mapper confusion, data not properly mapped  
**Severity:** MEDIUM

### Issue #6: Duplicate DTO Definitions
**File:** EmailSequenceManagementService.cs (lines 475+)  
**Problem:** Local DTO definitions conflict with EmailSequenceDtos.cs:
- Has `BodyHtml` (wrong) while main file doesn't have this
- Incomplete compared to main file
- Creates confusion about which to use  
**Severity:** MEDIUM - Code duplication and maintenance burden

### Issue #7: Missing Enum Conversion in Configuration
**File:** MarketingConfigurations.cs  
**Missing:** `HasConversion<string>()` on enum properties
```csharp
builder.Property(e => e.Status).HasConversion<string>();
builder.Property(e => e.ExitReason).HasConversion<string>();
```
**Severity:** MEDIUM - May cause database serialization issues

### Issue #8: Incomplete DTO Mapping
**File:** EmailSequenceManagementService.cs (MapToDto, MapStepToDto, MapEnrollmentToDto)  
**Problem:** Mappers don't transfer all entity properties to DTOs
**Impact:** Data loss, incomplete API responses  
**Severity:** MEDIUM

### Issue #9: Missing Property on Entity
**Entity:** EmailSequence  
**Missing:** `CampaignId` (int?) - Referenced in DTOs and service  
**Decision Needed:** Either add to entity or remove from DTOs  
**Severity:** HIGH - Will cause compilation error

---

## REQUIRED CHANGES SUMMARY

| Item | Type | Priority | Status |
|------|------|----------|--------|
| Add `SequenceType` to EmailSequence | Entity | HIGH | ❌ Not Found |
| Add `CampaignId` to EmailSequence | Entity | HIGH | ❌ Not Found |
| Fix `FromName`/`DefaultFromName` mismatch | Entity/DTO | HIGH | ⚠️ Mismatch |
| Fix `FromEmail`/`DefaultFromEmail` mismatch | Entity/DTO | HIGH | ⚠️ Mismatch |
| Fix `ReplyToEmail`/`DefaultReplyTo` mismatch | Entity/DTO | HIGH | ⚠️ Mismatch |
| Convert status strings to enums in service | Service | HIGH | ❌ Wrong Type |
| Remove `BodyHtml` reference, use `Body` | Service | HIGH | ❌ Wrong Property |
| Remove duplicate DTOs from service file | Code | MEDIUM | ❌ Duplication |
| Add full configuration in OnModelCreating | Config | MEDIUM | ⚠️ Incomplete |
| Fix enum comparisons in analytics | Service | HIGH | ❌ Logic Error |
| Complete DTO field mappings | Service | MEDIUM | ⚠️ Incomplete |

---

**Full analysis document:** `EMAIL_SEQUENCE_SCHEMA_ALIGNMENT_ANALYSIS.md`
