# Account Problems - Documentation Index & Implementation Guide

**Last Updated**: February 15, 2026  
**Status**: 🚨 5 CRITICAL PROBLEMS IDENTIFIED - READY FOR FIX  
**Total Fix Effort**: 20-27 hours over 4-5 weeks

---

## 📋 Documentation Map

### 1. **START HERE** - Quick Problem Overview
**File**: [`FIX_ACCOUNT_PROBLEMS.md`](docs/development/FIX_ACCOUNT_PROBLEMS.md)
- ✅ 5-minute executive summary
- ✅ Each problem explained in plain English
- ✅ Why each problem matters
- ✅ What "correct 3NF" looks like
- ✅ Effort estimates and timelines
- **Read this first if you're new to the problems**

### 2. **TODAY'S TASK** - Immediate Action Plan
**File**: [`IMMEDIATE_ACTION_PLAN.md`](docs/development/IMMEDIATE_ACTION_PLAN.md)
- ✅ Step-by-step copy-paste instructions
- ✅ 90-minute quick wins (add fields)
- ✅ Build verification commands
- ✅ Git commit guidance
- ✅ Week 1 next-phase planning
- **Read this to START IMPLEMENTING TODAY**

### 3. **DETAILED ANALYSIS** - Complete Deep Dive
**File**: [`ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md`](docs/development/ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md)
- ✅ Part 1: What IS properly normalized (Contacts, Relationships, Tags)
- ✅ Part 2: What IS denormalized (Addresses, Phone/Email/Social)
- ✅ Part 3: Additional missing data (Financial, Compliance, Partnership, Contact Preferences)
- ✅ Part 4-5: Field-by-field breakdown & normalization recommendations
- ✅ Part 6: Priority recommendations (Priority 1-5)
- ✅ Part 7: Complete TODO list (45+ TODO items with exact tasks)
- ✅ Part 8: Summary & effort breakdown
- **Read this for comprehensive understanding and detailed TODO items**

---

## 🎯 The 5 Problems (Quick Reference)

| # | Problem | Severity | Status | Fix Time | Doc Link |
|---|---------|----------|--------|----------|----------|
| 1 | Address Denormalization | 🔴 P0 | 🚨 TODO | 4-6h | [FIX_ACCOUNT_PROBLEMS.md #1](./FIX_ACCOUNT_PROBLEMS.md#problem-1-address-denormalization-%EF%B8%8F-critical) |
| 2 | Phone/Email/Social Duplication | 🔴 P1 | 🚨 TODO | 10-12h | [FIX_ACCOUNT_PROBLEMS.md #2](./FIX_ACCOUNT_PROBLEMS.md#problem-2-phoneemai lsocial-duplication-%EF%B8%8F-critical) |
| 3 | Missing Financial Metrics | 🟡 P2 | 🔄 QUICK WIN | 2-3h | [FIX_ACCOUNT_PROBLEMS.md #3](./FIX_ACCOUNT_PROBLEMS.md#problem-3-missing-financial-metrics-%EF%B8%8F-medium-priority) |
| 4 | Missing Compliance Fields | 🟡 P2 | 🔄 QUICK WIN | 2-3h | [FIX_ACCOUNT_PROBLEMS.md #4](./FIX_ACCOUNT_PROBLEMS.md#problem-4-missing-compliance-fields-%EF%B8%8F-medium-priority) |
| 5 | Missing Partnership Tracking | 🟠 P3 | 🔄 QUICK WIN | 2-3h | [FIX_ACCOUNT_PROBLEMS.md #5](./FIX_ACCOUNT_PROBLEMS.md#problem-5-missing-partnership-tracking-%EF%B8%8F-low-priority) |

---

## 🚀 Quick Start Guide

### TODAY (90 minutes)
1. Read: [`FIX_ACCOUNT_PROBLEMS.md`](docs/development/FIX_ACCOUNT_PROBLEMS.md) (15 min)
2. Follow: [`IMMEDIATE_ACTION_PLAN.md`](docs/development/IMMEDIATE_ACTION_PLAN.md) Steps 1-3 (30 min)
3. Build & Commit (30 min)
4. Result: ✅ Financial + Compliance + Partnership fields added

### WEEK 1 (Week of Feb 15-21)
1. Read: [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Part 7 - Phase 2](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-2-address-normalization)
2. Implement: Address normalization (4-6 hours)
3. Result: ✅ Denormalized addresses moved to Address entity

### WEEK 2-3 (Week of Feb 22 & Mar 1)
1. Implement: Phone/Email/Social consolidation (10-12 hours)
2. Result: ✅ Contact info moved to PhoneNumbers/EmailAddresses/SocialMediaAccounts

### WEEK 4+ (Post-launch/Optional)
- Financial metrics reporting (batch jobs, dashboards)
- Compliance tracking (NDA workflow, audit trail)
- Partnership management (hierarchy, tiers)

---

## 📊 Implementation Roadmap

```
WEEK 1 (Feb 15-21): Quick Wins + Address Normalization
├─ TODAY: Add financial/compliance/partnership fields (1.5h)
├─ Mon-Tue: Address normalization start (1-2h)
├─ Wed-Thu: Address API endpoints (1.5-2h)
├─ Fri: Address UI components (2-3h)
└─ Verify: All tests passing, 0 build errors

WEEK 2-3 (Feb 22 - Mar 7): Phone/Email/Social Consolidation
├─ Mon-Tue: Entity updates + migration (1-2h)
├─ Wed: Service layer changes (1.5-2h)
├─ Thu: API endpoints (1-1.5h)
├─ Fri: UI components (2-3h)
└─ Verify: All tests passing

WEEK 4+ (Optional): Compliance & Reporting
├─ Financial metrics batch jobs
├─ NDA workflow integration
├─ Partnership management
└─ Post-launch enhancements
```

---

## 📚 Related Documentation

### Architecture & Design
- **[SOLUTION_CONTEXT.md](docs/development/SOLUTION_CONTEXT.md)** - Database schema overview, 3NF explanation
- **[ARCHITECTURE_OVERVIEW.md](docs/development/ARCHITECTURE_OVERVIEW.md)** - System architecture patterns
- **[database/DATABASE_SCHEMA.md](./database/DATABASE_SCHEMA.md)** - Complete database reference

### Code References
- **Entity**: `CRM.Backend/src/CRM.Core/Entities/Account.cs` (575 lines)
- **Service**: `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs`
- **API**: `CRM.Backend/src/CRM.Api/Controllers/AccountsController.cs`
- **Context**: `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`

### Testing
- **Test Guide**: `e2e-tests/README.md`
- **Unit Tests**: `CRM.Backend/tests/CRM.Tests/Services/AccountServiceTests.cs`
- **E2E Tests**: `e2e-tests/tests/customers/account-details.spec.ts`

---

## 🔗 Problem Details Matrix

### Problem 1: Address Denormalization
| Aspect | Details |
|--------|---------|
| **What's Wrong** | Addresses stored directly in Account.cs instead of Address entity |
| **Why It Matters** | Violates 3NF, causes update/delete/insert anomalies |
| **Current Code** | `public string? Address { get; set; }`, `public string? City { get; set; }`, etc. |
| **Correct Code** | `public ICollection<Address> Addresses { get; set; }` + EntityAddressLinks |
| **Data Impact** | 50+ accounts with duplicate address data |
| **Test Impact** | Need address query tests, UI tests |
| **Fix Document** | [FIX_ACCOUNT_PROBLEMS.md #1](./FIX_ACCOUNT_PROBLEMS.md#problem-1-address-denormalization-%EF%B8%8F-critical) |
| **Implementation** | [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Phase 2](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-2-address-normalization) |

### Problem 2: Phone/Email/Social Duplication
| Aspect | Details |
|--------|---------|
| **What's Wrong** | Phone/Email/Social stored in Account.cs AND separate entities |
| **Why It Matters** | Partial normalization, can't track multiple phones/emails per account |
| **Current Code** | `public string Phone { get; set; }`, `public string? LinkedInUrl { get; set; }` |
| **Correct Code** | `public ICollection<PhoneNumber> PhoneNumbers { get; set; }` |
| **Data Impact** | 50+ accounts with single phone, but database supports multiple |
| **Test Impact** | Need multi-phone query tests, type filtering tests |
| **Fix Document** | [FIX_ACCOUNT_PROBLEMS.md #2](./FIX_ACCOUNT_PROBLEMS.md#problem-2-phoneemail-social-duplication-%EF%B8%8F-critical) |
| **Implementation** | [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Phase 4](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-4-phoneemail-social-consolidation) |

### Problem 3: Missing Financial Metrics
| Aspect | Details |
|--------|---------|
| **What's Wrong** | No fields to track revenue, MRR, ARR, payment status |
| **Why It Matters** | Can't generate financial reports, identify high-value customers |
| **Missing Fields** | LifetimeValue, MRR, ARR, AverageOrderValue, PaymentStatus, LastPaymentDate |
| **Fix Type** | Quick-win (add nullable fields, no migration complexity) |
| **Data Impact** | All new accounts will have NULL; batch job populates |
| **Business Impact** | Enables financial dashboards, customer segmentation |
| **Fix Document** | [FIX_ACCOUNT_PROBLEMS.md #3](./FIX_ACCOUNT_PROBLEMS.md#problem-3-missing-financial-metrics-%EF%B8%8F-medium-priority) |
| **Implementation** | [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Phase 3a](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-3a-financial-metrics) |

### Problem 4: Missing Compliance Fields
| Aspect | Details |
|--------|---------|
| **What's Wrong** | No audit trail for account verification, NDA tracking, data classification |
| **Why It Matters** | GDPR compliance, legal requirements, security |
| **Missing Fields** | VerificationStatus, NdaSigned, NdaSignedDate, DataClassification, DunsNumber |
| **Fix Type** | Quick-win (add nullable fields) |
| **Data Impact** | All new accounts start with VerificationStatus='Unverified' |
| **Business Impact** | Enables compliance workflows, audit trails |
| **Fix Document** | [FIX_ACCOUNT_PROBLEMS.md #4](./FIX_ACCOUNT_PROBLEMS.md#problem-4-missing-compliance-fields-%EF%B8%8F-medium-priority) |
| **Implementation** | [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Phase 3b](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-3b-compliance--verification-fields) |

### Problem 5: Missing Partnership Tracking
| Aspect | Details |
|--------|---------|
| **What's Wrong** | No fields to track partners, resellers, competitor relationships |
| **Why It Matters** | Channel management, partner segmentation |
| **Missing Fields** | IsReseller, IsPartner, PartnerTier, ParentResellerAccountId, CompetitorAccountId |
| **Fix Type** | Quick-win (add nullable fields) |
| **Data Impact** | All new accounts start with IsPartner=null |
| **Business Impact** | Enables partner program management |
| **Fix Document** | [FIX_ACCOUNT_PROBLEMS.md #5](./FIX_ACCOUNT_PROBLEMS.md#problem-5-missing-partnership-tracking-%EF%B8%8F-low-priority) |
| **Implementation** | [ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md Phase 3c](./ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md#phase-3c-partnership--context-fields) |

---

## ✅ Success Checklist

### Phase 0: Quick Wins (TODAY - 90 min)
- [ ] Read FIX_ACCOUNT_PROBLEMS.md
- [ ] Follow IMMEDIATE_ACTION_PLAN.md Step 1-3
- [ ] Account.cs builds with 0 errors
- [ ] Migration created successfully
- [ ] Code committed to git
- [ ] All fields visible in Account entity

### Phase 1: Address Normalization (Week 1 - 4-6h)
- [ ] Address fields removed from Account.cs
- [ ] EntityAddressLinks junction properly configured
- [ ] EF Core migration creates Address table (if needed)
- [ ] Data migrated from Account.Address → Address + junction
- [ ] AccountService methods updated
- [ ] API endpoints working (/api/accounts/{id}/addresses)
- [ ] AddressListComponent renders
- [ ] AddressModalComponent functional
- [ ] Unit tests passing
- [ ] E2E tests passing

### Phase 2: Phone/Email/Social (Week 2-3 - 10-12h)
- [ ] Phone/Email/Social fields removed from Account.cs
- [ ] PhoneNumbers/EmailAddresses/SocialMediaAccounts navigation added
- [ ] EF Core migration moves data
- [ ] AccountService methods updated
- [ ] API endpoints working
- [ ] UI components render
- [ ] Unit tests passing
- [ ] E2E tests passing

### Phase 3+: Optional (Week 4+)
- [ ] Financial metrics batch job running nightly
- [ ] FinancialsDashboard component displays metrics
- [ ] CompliancePanel component functional
- [ ] NDA workflow integrated
- [ ] PartnershipPanel component functional

---

## 🔄 Git Workflow

```bash
# Start new branch for each phase
git checkout -b fix/account-denormalization-phase1

# Commit quick wins
git add CRM.Backend/src/CRM.Core/Entities/Account.cs
git commit -m "Add financial, compliance, partnership fields to Account

- Add 9 financial metrics (LifetimeValue, MRR, ARR, etc.)
- Add 12 compliance fields (VerificationStatus, NDA, etc.)
- Add 11 partnership fields (IsReseller, PartnerTier, etc.)
- EF Core migration created"

# Push and create PR
git push origin fix/account-denormalization-phase1
```

---

## 📞 Getting Help

### If You're Stuck On...
- **Understanding the problem**: See [`FIX_ACCOUNT_PROBLEMS.md`](docs/development/FIX_ACCOUNT_PROBLEMS.md)
- **Exact implementation steps**: See [`IMMEDIATE_ACTION_PLAN.md`](docs/development/IMMEDIATE_ACTION_PLAN.md)
- **Detailed TODO items**: See [`ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md`](docs/development/ACCOUNT_DATA_NORMALIZATION_ANALYSIS.md)
- **Database design**: See [`database/DATABASE_SCHEMA.md`](./database/DATABASE_SCHEMA.md)
- **3NF concepts**: See [`SOLUTION_CONTEXT.md`](docs/development/SOLUTION_CONTEXT.md) - Database section

### Key Contacts
- **Backend Questions**: Check `CRM.Backend/src/CRM.Infrastructure/Services/AccountService.cs`
- **Database Questions**: Check `CRM.Backend/src/CRM.Infrastructure/Data/CrmDbContext.cs`
- **API Questions**: Check `CRM.Backend/src/CRM.Api/Controllers/AccountsController.cs`
- **Frontend Questions**: Check `CRM.Frontend/src/pages/AccountDetailsPage.tsx`

---

## 📈 Progress Tracking

Use this template to track progress:

```markdown
## Weekly Status (Feb 15-21)

### Quick Wins ✅
- [x] Add financial fields (30 min)
- [x] Add compliance fields (30 min)
- [x] Add partnership fields (30 min)
- [x] Build verification (5 min)
- [x] Migration created (10 min)
- [x] Code committed (5 min)
**Subtotal: 1.5 hours**

### Phase 1: Address Normalization 🚧
- [ ] Entity updates (30 min) - IN PROGRESS
- [ ] API endpoints (1.5-2h)
- [ ] UI components (2-3h)
- [ ] Tests (1.5-2h)
**Target: Feb 21**

### Blockers
- None yet

### Notes
- Starting with quick wins today
- Address normalization next week
```

---

## 🎯 Final Notes

✅ **You have everything needed to fix these problems:**
1. Clear problem descriptions
2. Step-by-step implementation guides
3. Code examples (old vs. new)
4. Effort estimates
5. Success criteria
6. Testing strategies

✅ **Recommended Order:**
1. **TODAY**: Quick wins (1.5h) - Financial + Compliance + Partnership fields
2. **WEEK 1**: Address normalization (4-6h) - Core 3NF fix
3. **WEEK 2-3**: Phone/Email/Social (10-12h) - Complete normalization
4. **WEEK 4+**: Optional enhancements (reporting, workflows)

✅ **START HERE:**
→ Read [`FIX_ACCOUNT_PROBLEMS.md`](docs/development/FIX_ACCOUNT_PROBLEMS.md) (15 min)
→ Follow [`IMMEDIATE_ACTION_PLAN.md`](docs/development/IMMEDIATE_ACTION_PLAN.md) (90 min)
→ Commit to git & celebrate! 🎉

---

**READY TO START? Open [`IMMEDIATE_ACTION_PLAN.md`](docs/development/IMMEDIATE_ACTION_PLAN.md) NOW!**
