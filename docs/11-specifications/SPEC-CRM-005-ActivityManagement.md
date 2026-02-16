# SPEC-CRM-005: Activity Management

> **Status:** ✅ Complete  
> **Priority:** P1  
> **Module:** Core CRM  
> **Last Updated:** February 12, 2026  
> **Dependencies:** SPEC-CRM-001 (Account), SPEC-CRM-004 (Contact)

---

## 1. Business Context

### 1.1 Overview

Activity Management provides a comprehensive timeline/activity feed tracking all interactions and events across the CRM. Activities are automatically created when key actions occur (opportunity won, email sent, call made) and can also be manually logged. The system supports 25+ activity types, polymorphic entity linking, and event attendee management.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| ACT-001 | Activity Timeline | Chronological view of all activities | ✅ Implemented |
| ACT-002 | Activity Filtering | Filter by type, date, entity, user | ✅ Implemented |
| ACT-003 | Customer Timeline | Activities linked to specific account | ✅ Implemented |
| ACT-004 | Opportunity Timeline | Activities linked to opportunity | ✅ Implemented |
| ACT-005 | Activity Statistics | Aggregate stats (emails, calls, etc.) | ✅ Implemented |
| ACT-006 | Event Attendees | Manage attendees for meeting activities | ✅ Implemented |
| ACT-007 | Activity CRUD | Create, read, delete activities | ✅ Implemented |
| ACT-008 | Chat Integration | Chat messages as timeline activities | ✅ Implemented |

### 1.3 Functionalities

| ID | Functionality | Use Case | Status |
|----|---------------|----------|--------|
| F-001 | View activity feed | User views timeline of all recent activities | ✅ Implemented |
| F-002 | Filter activities | Filter by type (EmailSent, CallMade, etc.) | ✅ Implemented |
| F-003 | Search activities | Search by title/description | ✅ Implemented |
| F-004 | Log manual activity | Sales rep logs a call or meeting | ✅ Implemented |
| F-005 | View customer timeline | See all activities for an account | ✅ Implemented |
| F-006 | View opportunity timeline | See all activities for a deal | ✅ Implemented |
| F-007 | Activity statistics dashboard | View email/call/meeting counts | ✅ Implemented |
| F-008 | Manage event attendees | Add/remove users, contacts, leads to meetings | ✅ Implemented |
| F-009 | Track attendee responses | Accept/decline/tentative for meetings | ✅ Implemented |
| F-010 | Record attendance | Mark who attended after event | ✅ Implemented |

### 1.4 Activity Types (25+)

| Enum Value | Description | Category |
|------------|-------------|----------|
| EmailSent | Outbound email sent | Communication |
| EmailReceived | Inbound email received | Communication |
| CallMade | Outbound call made | Communication |
| CallReceived | Inbound call received | Communication |
| MeetingScheduled | Meeting created | Events |
| MeetingCompleted | Meeting finished | Events |
| NoteAdded | Note added to entity | Documentation |
| TaskCreated | New task created | Tasks |
| TaskCompleted | Task marked done | Tasks |
| OpportunityCreated | New opportunity | Sales |
| OpportunityUpdated | Opportunity modified | Sales |
| OpportunityWon | Deal closed won | Sales |
| OpportunityLost | Deal closed lost | Sales |
| QuoteCreated | Quote generated | Sales |
| QuoteSent | Quote sent to customer | Sales |
| QuoteAccepted | Quote accepted | Sales |
| QuoteRejected | Quote rejected | Sales |
| CustomerCreated | New account created | CRM |
| CustomerUpdated | Account modified | CRM |
| CustomerDeleted | Account deleted | CRM |
| ContactCreated | New contact created | CRM |
| ContactUpdated | Contact modified | CRM |
| UserLogin | User logged in | System |
| UserLogout | User logged out | System |
| ChatMessage | Chat/messaging activity | Communication |

---

## 2. Frontend

### 2.1 Pages

| Page | File | Route | Status |
|------|------|-------|--------|
| Activities Page | `ActivitiesPage.tsx` | `/activities` | ✅ Implemented |

### 2.2 Components

| Component | File | Purpose | Status |
|-----------|------|---------|--------|
| Timeline View | In ActivitiesPage | MUI Timeline component | ✅ Implemented |
| Activity Icons | In ActivitiesPage | Type-specific icons | ✅ Implemented |
| Activity Colors | In ActivitiesPage | Type-specific colors | ✅ Implemented |
| Stats Cards | In ActivitiesPage | Dashboard statistics | ✅ Implemented |
| Filter Controls | In ActivitiesPage | Entity filter dropdown | ✅ Implemented |
| Search Box | In ActivitiesPage | Title/description search | ✅ Implemented |
| Import/Export | ImportExportButtons | CSV import/export | ✅ Implemented |

### 2.3 Services

| Service | File | Purpose | Status |
|---------|------|---------|--------|
| API Client | `apiClient.ts` | Direct API calls in page | ✅ Implemented |
| Dashboard Service | `dashboardService.ts` | Activity widgets | ✅ Implemented |

**Note:** No dedicated activityService.ts - API calls are made directly in ActivitiesPage.tsx using apiClient.

### 2.4 Frontend Validations

| Field | Validation | Status |
|-------|------------|--------|
| entityFilter | Valid entity type | ✅ Implemented |
| limit | 1-100 range | ✅ Implemented |
| searchQuery | Min 2 chars before search | ✅ Implemented |

---

## 3. Backend

### 3.1 Entities

| Entity | File | Description | Status |
|--------|------|-------------|--------|
| Activity | `Activity.cs` | Main activity entity (~150 lines) | ✅ Implemented |
| EventAttendee | `EventAttendee.cs` | Meeting attendee tracking | ✅ Implemented |
| ActivityType | `Activity.cs` | Enum with 25+ types | ✅ Implemented |
| AttendeeType | `EventAttendee.cs` | User/Contact/Lead enum | ✅ Implemented |

### 3.2 Entity Properties (Activity)

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| Id | int | Yes | Primary key |
| ActivityType | ActivityType | Yes | Type enum (EmailSent, CallMade, etc.) |
| Title | string | Yes | Activity title |
| Description | string | No | Detailed description |
| ActivityDate | DateTime | Yes | When activity occurred |
| DurationMinutes | int? | No | Duration for calls/meetings |
| UserId | int? | No | User who performed activity |
| UserName | string? | No | Denormalized user name |
| EntityType | string? | No | Polymorphic entity type |
| EntityId | int? | No | Polymorphic entity ID |
| EntityName | string? | No | Denormalized entity name |
| SecondaryEntityType | string? | No | Secondary entity type |
| SecondaryEntityId | int? | No | Secondary entity ID |
| SecondaryEntityName | string? | No | Denormalized name |
| AccountId | int? | No | FK to Account |
| ContactId | int? | No | FK to Contact |
| OpportunityId | int? | No | FK to Opportunity |
| CampaignId | int? | No | FK to Campaign |
| TaskId | int? | No | FK to Task |
| QuoteId | int? | No | FK to Quote |
| NoteId | int? | No | FK to Note |
| Details | string? | No | JSON additional data |
| OldValue | string? | No | Previous value (for changes) |
| NewValue | string? | No | New value (for changes) |
| Category | string? | No | Activity category |
| Tags | string? | No | Comma-separated tags |
| CustomFields | string? | No | JSON custom fields |
| IpAddress | string? | No | User's IP address |
| UserAgent | string? | No | Browser user agent |
| Source | string? | No | Activity source |
| IsSystem | bool | No | System-generated flag |
| IsPrivate | bool | No | Private activity flag |
| IsImportant | bool | No | Important marker |
| CreatedAt | DateTime | Yes | Created timestamp |
| UpdatedAt | DateTime | No | Updated timestamp |
| IsDeleted | bool | No | Soft delete flag |

### 3.3 Interfaces

| Interface | File | Methods | Status |
|-----------|------|---------|--------|
| IActivityService | `IActivityService.cs` | 8 methods | ✅ Implemented |

### 3.4 Interface Methods

```csharp
public interface IActivityService
{
    Task<IEnumerable<Activity>> GetActivitiesAsync(
        int? customerId = null,
        int? opportunityId = null,
        int? userId = null,
        ActivityType? activityType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int limit = 50);
    
    Task<Activity?> GetByIdAsync(int id);
    Task<Activity> CreateAsync(Activity activity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Activity>> GetByEntityAsync(string entityType, int entityId, int limit = 50);
    Task<IEnumerable<Activity>> GetCustomerTimelineAsync(int customerId, int limit = 100);
    Task<IEnumerable<Activity>> GetOpportunityTimelineAsync(int opportunityId, int limit = 100);
    Task<IEnumerable<Activity>> GetRecentAsync(int limit = 20);
    Task<ActivityStats> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
```

### 3.5 Services

| Service | File | Lines | Status |
|---------|------|-------|--------|
| ActivityService | `ActivityService.cs` | ~365 | ✅ Implemented |

### 3.6 Controllers

| Controller | File | Lines | Status |
|------------|------|-------|--------|
| ActivitiesController | `ActivitiesController.cs` | ~472 | ✅ Implemented |

### 3.7 API Endpoints

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| GET | `/api/activities` | List with filters | ✅ Implemented |
| GET | `/api/activities/{id}` | Get by ID | ✅ Implemented |
| POST | `/api/activities` | Create activity | ✅ Implemented |
| DELETE | `/api/activities/{id}` | Delete activity | ✅ Implemented |
| GET | `/api/activities/entity/{type}/{id}` | By entity | ✅ Implemented |
| GET | `/api/activities/customer/{id}/timeline` | Customer timeline | ✅ Implemented |
| GET | `/api/activities/opportunity/{id}/timeline` | Opportunity timeline | ✅ Implemented |
| GET | `/api/activities/recent` | Recent activities | ✅ Implemented |
| GET | `/api/activities/stats` | Statistics | ✅ Implemented |
| GET | `/api/activities/{id}/attendees` | List attendees | ✅ Implemented |
| POST | `/api/activities/{id}/attendees` | Add attendee | ✅ Implemented |
| GET | `/api/activities/{id}/attendees/{aId}` | Get attendee | ✅ Implemented |
| PATCH | `/api/activities/{id}/attendees/{aId}/respond` | Update response | ✅ Implemented |
| PATCH | `/api/activities/{id}/attendees/{aId}/attendance` | Mark attended | ✅ Implemented |
| DELETE | `/api/activities/{id}/attendees/{aId}` | Remove attendee | ✅ Implemented |
| GET | `/api/activities/attendee/{type}/{id}/events` | Events for attendee | ✅ Implemented |

### 3.8 Backend Validations

| Validation | Location | Status |
|------------|----------|--------|
| ActivityDate default | Service | ✅ Implemented |
| CreatedAt/UpdatedAt auto | Service | ✅ Implemented |
| Limit bounds | Controller | ✅ Implemented |
| Entity existence | Controller | ✅ Implemented |

---

## 4. Database

### 4.1 Tables

| Table | Description | Status |
|-------|-------------|--------|
| Activities | Activity records | ✅ Exists |
| EventAttendees | Meeting attendees | ✅ Exists |

### 4.2 Activities Table Schema

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | INT | No | Primary key |
| ActivityType | INT | No | ActivityType enum |
| Title | VARCHAR(500) | No | Activity title |
| Description | TEXT | Yes | Full description |
| ActivityDate | DATETIME | No | When occurred |
| DurationMinutes | INT | Yes | Duration |
| UserId | INT | Yes | FK to Users |
| UserName | VARCHAR(200) | Yes | Denormalized |
| EntityType | VARCHAR(100) | Yes | Polymorphic type |
| EntityId | INT | Yes | Polymorphic ID |
| EntityName | VARCHAR(500) | Yes | Denormalized |
| AccountId | INT | Yes | FK to Customers |
| ContactId | INT | Yes | FK to Contacts |
| OpportunityId | INT | Yes | FK to Opportunities |
| Details | TEXT | Yes | JSON data |
| OldValue | TEXT | Yes | Before value |
| NewValue | TEXT | Yes | After value |
| Category | VARCHAR(100) | Yes | Category |
| Tags | VARCHAR(500) | Yes | Tags |
| CustomFields | TEXT | Yes | JSON |
| IpAddress | VARCHAR(50) | Yes | IP |
| UserAgent | VARCHAR(500) | Yes | Browser |
| Source | VARCHAR(100) | Yes | Source |
| IsSystem | BIT | No | System flag |
| IsPrivate | BIT | No | Private flag |
| IsImportant | BIT | No | Important flag |
| CreatedAt | DATETIME | No | Created |
| UpdatedAt | DATETIME | Yes | Updated |
| IsDeleted | BIT | No | Soft delete |

### 4.3 EventAttendees Table Schema

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | INT | No | Primary key |
| ActivityId | INT | No | FK to Activities |
| AttendeeType | INT | No | User/Contact/Lead enum |
| AttendeeId | INT | No | FK to attendee |
| ResponseStatus | INT | No | Accepted/Declined/etc. |
| IsOrganizer | BIT | No | Is organizer |
| IsRequired | BIT | No | Required attendee |
| Role | VARCHAR(100) | Yes | Attendee role |
| DidAttend | BIT | Yes | Actually attended |
| RespondedAt | DATETIME | Yes | Response time |
| CreatedAt | DATETIME | No | Created |
| UpdatedAt | DATETIME | Yes | Updated |

### 4.4 Indexes

| Index | Table | Columns | Status |
|-------|-------|---------|--------|
| IX_Activities_ActivityDate | Activities | ActivityDate DESC | ✅ Exists |
| IX_Activities_AccountId | Activities | AccountId | ✅ Exists |
| IX_Activities_OpportunityId | Activities | OpportunityId | ✅ Exists |
| IX_Activities_EntityType_EntityId | Activities | EntityType, EntityId | ✅ Exists |
| IX_EventAttendees_ActivityId | EventAttendees | ActivityId | ✅ Exists |

### 4.5 Foreign Keys

| FK Name | From | To | Status |
|---------|------|------|--------|
| FK_Activities_Users | Activities.UserId | Users.Id | ✅ Exists |
| FK_Activities_Customers | Activities.AccountId | Customers.Id | ✅ Exists |
| FK_Activities_Contacts | Activities.ContactId | Contacts.Id | ✅ Exists |
| FK_Activities_Opportunities | Activities.OpportunityId | Opportunities.Id | ✅ Exists |
| FK_EventAttendees_Activities | EventAttendees.ActivityId | Activities.Id | ✅ Exists |

---

## 5. Tests

### 5.1 Backend Tests

| Test File | Location | Tests | Status |
|-----------|----------|-------|--------|
| ActivityServiceTests.cs | `tests/CRM.Tests/Integration/Services/` | 16 | ✅ Exists |
| ActivityRepositoryTests.cs | `tests/Repositories/` | 5 | ✅ Exists |
| ActivityNoteTagAddressEntityTests.cs | `tests/Unit/Core/` | 8 | ✅ Exists |
| ActivitiesControllerTests.cs | `tests/Controllers/` | 6 | ✅ Exists |

### 5.2 Test Coverage

| Method | Test Count | Coverage |
|--------|------------|----------|
| GetActivitiesAsync | 4 | ✅ High |
| GetByIdAsync | 1 | ✅ Covered |
| CreateAsync | 2 | ✅ Covered |
| DeleteAsync | 1 | ✅ Covered |
| GetByEntityAsync | 1 | ✅ Covered |
| GetCustomerTimelineAsync | 2 | ✅ Covered |
| GetOpportunityTimelineAsync | 1 | ✅ Covered |
| GetStatsAsync | 2 | ✅ Covered |
| Chat conversations | 2 | ✅ Covered |

### 5.3 Frontend Tests

| Test File | Status |
|-----------|--------|
| ActivitiesPage.test.tsx | ❌ Not Found |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Recommendation |
|----|-------|----------|----------------|
| ISS-001 | No dedicated activityService.ts | Low | Consider extracting API calls to service file |
| ISS-002 | Activity entity has ~50 properties | Low | Consider splitting into detail entity |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM005-001 | Create ActivitiesPage.test.tsx unit tests | P2 | Testing |
| TODO-CRM005-002 | Create dedicated activityService.ts | P3 | Frontend |
| TODO-CRM005-003 | Add ActivityFeed reusable component | P3 | Frontend |
| TODO-CRM005-004 | Add ActivityCalendar view | P3 | Frontend |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | Copilot | Initial specification from existing implementation |

---

## 9. Related Specifications

- [SPEC-CRM-001](SPEC-CRM-001-AccountManagement.md) - Account Management (Activities link to accounts)
- [SPEC-CRM-004](SPEC-CRM-004-ContactManagement.md) - Contact Management (Activities link to contacts)
- [SPEC-CRM-003](SPEC-CRM-003-OpportunityManagement.md) - Opportunity Management (Activities link to opportunities)
- [SPEC-CRM-007](SPEC-CRM-007-TaskManagement.md) - Task Management (Tasks create activities)
