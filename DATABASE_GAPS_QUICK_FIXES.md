# Database Schema Gaps - Quick Fix Guide

## 🚨 Critical Fixes (Fix First - Blocks Production)

### FIX #1: Email Sequence Entities (2-3 hours)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Lines:** Add after line 3200 (in OnModelCreating)

**Status:** ⚠️ EntityTypeConfiguration files exist but configurations are MINIMAL (only HasKey)

```csharp
// Add comprehensive Email Sequence configuration
// Reference: See DATABASE_EF_CORE_GAP_ANALYSIS.md Section 8, Example 1

// Fix these entities:
// - EmailSequenceStep (add relationships, indexes, precision)
// - EmailSequenceEnrollment (add status indexes)
// - EmailSequenceStepExecution (add execution tracking indexes)
```

**Test:** `dotnet test CRM.Backend/tests/Configurations/EntityConfigurationTests.cs -f EmailSequence*`

**Migration:**
```bash
cd CRM.Backend
dotnet ef migrations add CompleteEmailSequenceConfiguration
dotnet ef database update
```

---

### FIX #2: ITSM Module Relationships (4-5 hours)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Line Range:** Search for `ITSM.ConfigurationItem` pattern

**Status:** ⚠️ DbSets exist but OnModelCreating configs missing/incomplete

**Affected Entities:**
```
□ ITSM.ConfigurationItem - needs all relationship configs
□ ITSM.CIRelationship - needs self-referencing configs
□ ITSM.Service - needs relationship to ServiceCI
□ ITSM.ServiceCI - needs junction configuration
□ ITSM.Problem - needs relationship to Incidents
□ ITSM.Change - needs cascade delete on tasks
```

**Priority Order:**
1. ConfigurationItem (CMDB backbone)
2. CIRelationship (dependency tracking)
3. Change + ChangeTask (change management)
4. Problem + ProblemIncident (incident management)

**Location:** Add after line 3400 in OnModelCreating (ITSM section)

---

### FIX #3: Web Tracking Performance Indexes (2 hours)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Line Range:** Find WebPageView, WebSession, FormSubmission configurations (around line 2000-2500)

**Add These Indexes:**
```csharp
// WebPageView
entity.HasIndex(e => new { e.SessionId, e.ViewedAt });

// WebSession  
entity.HasIndex(e => new { e.VisitorId, e.StartedAt });

// FormSubmission
entity.HasIndex(e => new { e.FormDefinitionId, e.SubmittedAt });

// LandingPageVisit
entity.HasIndex(e => new { e.LandingPageId, e.VisitedAt });

// WebVisitor
entity.HasIndex(e => e.FingerPrintHash);
```

**Migration:**
```bash
dotnet ef migrations add AddWebTrackingPerformanceIndexes
dotnet ef database update
```

---

## 🟡 High Priority Fixes (Fix This Week - Performance Issues)

### FIX #4: Decimal Precision for AI Models (1 hour)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Search:** `LeadScore\|OpportunityInsight\|ChurnRisk\|Prediction`

**Current Problems:**
```
✗ Prediction.Confidence = DECIMAL(18,2)  → Should be DECIMAL(5,4)
✗ LeadScore.OverallScore = DECIMAL(18,2)  → Should be DECIMAL(5,2)
✗ OpportunityInsight.WinProbability = DECIMAL(18,2)  → Should be DECIMAL(5,4)
✗ ActionRecommendation.ImpactScore = DECIMAL(18,2)  → Should be DECIMAL(5,2)
```

**Fixes to Apply:**
```csharp
// In LeadScore config:
entity.Property(e => e.OverallScore).HasPrecision(5, 2);
entity.Property(e => e.Confidence).HasPrecision(10, 4);
entity.Property(e => e.DemographicScore).HasPrecision(5, 2);
entity.Property(e => e.FirmographicScore).HasPrecision(5, 2);
entity.Property(e => e.BehavioralScore).HasPrecision(5, 2);
entity.Property(e => e.EngagementScore).HasPrecision(5, 2);
entity.Property(e => e.IntentScore).HasPrecision(5, 2);

// Similar corrections for other AI entities
```

**Migration Script (Safe - with data conversion):**
```sql
-- Backup first!
ALTER TABLE LeadScores MODIFY COLUMN OverallScore DECIMAL(5, 2);
ALTER TABLE OpportunityInsights MODIFY COLUMN WinProbability DECIMAL(5, 4);
ALTER TABLE Predictions MODIFY COLUMN Confidence DECIMAL(5, 4);
ALTER TABLE ChurnRisks MODIFY COLUMN ChurnProbability DECIMAL(5, 4);
```

---

### FIX #5: Campaign Analytics Indexes (1.5 hours)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Line Range:** CampaignRecipient, CampaignLinkClick, CampaignConversion configs

**Add Indexes:**
```csharp
// CampaignRecipient
entity.HasIndex(e => new { e.CampaignId, e.Status });

// CampaignLinkClick
entity.HasIndex(e => new { e.CampaignRecipientId, e.ClickedAt });
entity.HasIndex(e => new { e.CampaignId, e.ClickedAt });

// CampaignConversion
entity.HasIndex(e => new { e.CampaignId, e.ConvertedAt });
entity.HasIndex(e => new { e.CampaignRecipientId, e.ConvertedAt });
```

---

### FIX #6: Quote → Order Revenue Chain Indexes (1 hour)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`  
**Entities:** Quote, QuoteLineItem, Order, OrderLineItem

**Add Indexes:**
```csharp
// Quote
entity.HasIndex(e => new { e.Status, e.CreatedAt, e.Amount });

// Order  
entity.HasIndex(e => new { e.AccountId, e.CreatedAt });

// Subscription (for ARR tracking)
entity.HasIndex(e => new { e.AccountId, e.Status, e.StartDate });
```

---

## 🟢 Medium Priority Fixes (Fix Next Sprint - Robustness)

### FIX #7: ITSM Validations (1.5 hours)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`

**Add Validations:**
```csharp
// ServiceRequest -> ServiceRequestCustomFieldDefinition
entity.HasIndex(e => e.FieldKey).IsUnique();

// CatalogRequest -> Unique RequestNumber
entity.HasIndex(e => e.RequestNumber).IsUnique();

// CatalogVariable -> JSON field constraint
entity.Property(e => e.VariableSchemaJson).HasColumnType("TEXT");
```

---

### FIX #8: Polymorphic Contact Links Navigation (2 hours)  

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`

**Problem:** Reverse navigation missing (can't easily query addresses for an account)

**Current:**
```csharp
entity.HasOne(e => e.Address)
    .WithMany(a => a.EntityAddressLinks)  // ✓ Forward
    .HasForeignKey(e => e.AddressId);
    
// Missing reverse for EntityAddressLink -> Entity
```

**Solution:** May require refactoring to use proper FK or shadow property

---

### FIX #9: Contract/Subscription Temporal Queries (1 hour)

**File:** `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`

**Add Indexes:**
```csharp
// Contract
entity.HasIndex(e => new { e.AccountId, e.Status, e.EffectiveFrom, e.EffectiveTo });

// Subscription
entity.HasIndex(e => new { e.AccountId, e.Status, e.StartDate, e.EndDate });

// SubscriptionUsage
entity.HasIndex(e => new { e.SubscriptionId, e.UsageDate });
```

---

## Quick Action Checklist

### Sprint This Week
- [ ] Fix Email Sequence Configurations
- [ ] Add Web Tracking Performance Indexes  
- [ ] Correct AI Model Decimal Precision
- [ ] Add Campaign Analytics Indexes
- [ ] Generate migrations and test
- [ ] Deploy to dev environment

### Next Sprint
- [ ] Complete ITSM Module Relationships
- [ ] Add Quote → Order Indexes
- [ ] Implement ITSM Field Validations
- [ ] Add Contract Temporal Indexes
- [ ] Fix Polymorphic Navigation
- [ ] Performance testing and tuning

---

## Testing Commands

```bash
# Test entity configurations
cd CRM.Backend
dotnet test tests/Configurations/EntityConfigurationTests.cs

# Generate migrations
dotnet ef migrations add NAME_HERE

# List pending migrations
dotnet ef migrations list

# Apply migrations
dotnet ef database update

# Validate schema
dotnet ef dbcontext info

# Check for EF Core warnings
dotnet build --diagnostic
```

---

## Database Commands

### MariaDB - Verify Indexes Created
```sql
-- Check if index exists
SHOW INDEX FROM table_name WHERE Column_name = 'column_name';

-- Verify index is being used
EXPLAIN SELECT * FROM table_name WHERE status = 'Active' AND created_at > '2026-01-01';

-- Compare query plans before/after
-- Should see "Using index" in Extra column after adding index
```

### SQL Server - Verify Indexes
```sql
SELECT name, type_desc 
FROM sys.indexes 
WHERE object_id = OBJECT_ID('dbo.TableName');

-- Query plan analysis
SET STATISTICS IO ON;
SELECT * FROM TableName WHERE ColumnName = 'Value';
SET STATISTICS IO OFF;
```

---

## Rollback Plan

If migration fails:
```bash
# Remove last migration
dotnet ef migrations remove

# Revert database to previous state
dotnet ef database update <previous_migration_name>

# List migration history
dotnet ef migrations list
```

---

**Last Updated:** February 16, 2026  
**Effort Estimate (All Fixes):** ~20-25 hours  
**Estimated Timeline:** 2-3 sprints  
**Blocker Status:** 3 critical items (email, ITSM, indexes) block production
