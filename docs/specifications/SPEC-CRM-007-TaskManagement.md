# SPEC-CRM-007: Task Management

> **Module:** Core CRM  
> **Status:** ✅ Complete  
> **Last Updated:** February 2026  
> **Dependencies:** SPEC-CRM-001 (Accounts), SPEC-CRM-003 (Opportunities), SPEC-CRM-004 (Contacts)

---

## 1. Business Context

### 1.1 Overview
Task Management enables users to create, assign, track, and complete CRM tasks. Tasks can be linked to accounts, contacts, opportunities, and campaigns. The feature includes filtering, workflow queue management, recurring tasks, and comprehensive status/priority tracking.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-007-01 | Task CRUD | Create, read, update, delete tasks | ✅ Implemented |
| SF-007-02 | Task Assignment | Assign tasks to users or groups | ✅ Implemented |
| SF-007-03 | Entity Linking | Link tasks to accounts, contacts, opportunities, campaigns | ✅ Implemented |
| SF-007-04 | Task Filtering | Filter by status, priority, due date, assignee | ✅ Implemented |
| SF-007-05 | Workflow Queue | My Queue for user/group-assigned tasks | ✅ Implemented |
| SF-007-06 | Task Completion | Mark tasks complete with completion date | ✅ Implemented |
| SF-007-07 | Task Statistics | Statistics by status, priority, overdue, due today | ✅ Implemented |
| SF-007-08 | Recurring Tasks | Support for recurring task patterns | ✅ Implemented |
| SF-007-09 | Task Reminders | Reminder dates and notifications | ✅ Implemented |

### 1.3 Functionalities

| ID | Functionality | Sub-Feature | Status |
|----|---------------|-------------|--------|
| F-007-01 | Create tasks with subject, description, type, priority | SF-007-01 | ✅ Implemented |
| F-007-02 | Update tasks with validation | SF-007-01 | ✅ Implemented |
| F-007-03 | Soft delete tasks | SF-007-01 | ✅ Implemented |
| F-007-04 | Assign to user by ID | SF-007-02 | ✅ Implemented |
| F-007-05 | Assign to group for workflow queue | SF-007-02 | ✅ Implemented |
| F-007-06 | Link to account by AccountId | SF-007-03 | ✅ Implemented |
| F-007-07 | Link to opportunity by OpportunityId | SF-007-03 | ✅ Implemented |
| F-007-08 | Link to contact by ContactId | SF-007-03 | ✅ Implemented |
| F-007-09 | Link to campaign by CampaignId | SF-007-03 | ✅ Implemented |
| F-007-10 | Filter by status (NotStarted, InProgress, Completed, etc.) | SF-007-04 | ✅ Implemented |
| F-007-11 | Filter by priority (Low, Normal, High, Urgent) | SF-007-04 | ✅ Implemented |
| F-007-12 | Filter by overdue status | SF-007-04 | ✅ Implemented |
| F-007-13 | Get My Queue with group-based filtering | SF-007-05 | ✅ Implemented |
| F-007-14 | Workflow admin sees all tasks | SF-007-05 | ✅ Implemented |
| F-007-15 | Mark task as complete with completion timestamp | SF-007-06 | ✅ Implemented |
| F-007-16 | Track percent complete (0-100) | SF-007-06 | ✅ Implemented |
| F-007-17 | Get task statistics per user | SF-007-07 | ✅ Implemented |
| F-007-18 | Get overdue tasks | SF-007-07 | ✅ Implemented |
| F-007-19 | Get tasks due today | SF-007-07 | ✅ Implemented |
| F-007-20 | Support recurring task patterns | SF-007-08 | ✅ Implemented |
| F-007-21 | Set reminder date/time | SF-007-09 | ✅ Implemented |

### 1.4 Use Cases

| ID | Use Case | Actor | Functionalities |
|----|----------|-------|-----------------|
| UC-007-01 | Create a follow-up task for an opportunity | Sales Rep | F-007-01, F-007-04, F-007-07 |
| UC-007-02 | View my task queue | Any User | F-007-13 |
| UC-007-03 | Complete a task | Any User | F-007-15, F-007-16 |
| UC-007-04 | Filter overdue tasks | Manager | F-007-10, F-007-12, F-007-18 |
| UC-007-05 | Assign task to team queue | Team Lead | F-007-05 |
| UC-007-06 | Review task statistics | Manager | F-007-17, F-007-18, F-007-19 |

---

## 2. Frontend Implementation

### 2.1 Pages

| Page | File | Status | Lines |
|------|------|--------|-------|
| TasksPage | `CRM.Frontend/src/pages/TasksPage.tsx` | ✅ Implemented | ~744 |

### 2.2 Components

| Component | Location | Status |
|-----------|----------|--------|
| TasksPage (full-featured) | `pages/TasksPage.tsx` | ✅ Implemented |
| TaskDialog | Embedded in TasksPage | ✅ Implemented |
| TaskTable | Embedded in TasksPage | ✅ Implemented |
| TaskQueueView | Embedded in TasksPage | ✅ Implemented |
| AdvancedSearch | `components/AdvancedSearch.tsx` | ✅ Implemented |
| ImportExportButtons | `components/ImportExportButtons.tsx` | ✅ Implemented |
| LookupSelect | `components/LookupSelect.tsx` | ✅ Implemented |
| EntitySelect | `components/EntitySelect.tsx` | ✅ Implemented |
| TaskForm | ❌ Not Found | ❌ Not Implemented |
| TaskCard | ❌ Not Found | ❌ Not Implemented |

### 2.3 Services

| Service | File | Status |
|---------|------|--------|
| apiClient | `services/apiClient.ts` | ✅ Implemented |
| Dedicated taskService | ❌ Not Found | ❌ Not Implemented |

**API Calls in TasksPage.tsx:**
```typescript
// Direct apiClient usage (no dedicated service)
apiClient.get('/tasks/my-queue')
apiClient.get('/tasks')
apiClient.post('/tasks', payload)
apiClient.put(`/tasks/${id}`, payload)
apiClient.put(`/tasks/${id}/complete`)
apiClient.delete(`/tasks/${id}`)
```

### 2.4 Frontend Validations

| Field | Validation | Status |
|-------|------------|--------|
| Subject/Title | Required, non-empty | ✅ Implemented |
| DueDate | Optional, date format | ✅ Implemented |
| Priority | Select from options | ✅ Implemented |
| Status | Select from options | ✅ Implemented |
| TaskType | Select from options | ✅ Implemented |
| PercentComplete | 0-100 range | ✅ Implemented |

---

## 3. Backend Implementation

### 3.1 Entity

| Entity | File | Status | Lines |
|--------|------|--------|-------|
| CrmTask | `CRM.Core/Entities/CrmTask.cs` | ✅ Implemented | ~200 |
| CrmTaskStatus | `CRM.Core/Entities/CrmTask.cs` | ✅ Implemented | Enum |
| CrmTaskPriority | `CRM.Core/Entities/CrmTask.cs` | ✅ Implemented | Enum |
| CrmTaskType | `CRM.Core/Entities/CrmTask.cs` | ✅ Implemented | Enum |

**CrmTask Properties:**
- Basic: Subject, Description, TaskType, Status, Priority
- Dates: DueDate, StartDate, CompletedDate, ReminderDate, HasReminder
- Progress: PercentComplete, EstimatedMinutes, ActualMinutes
- Recurrence: IsRecurring, RecurrencePattern, RecurrenceEndDate, ParentTaskId
- Relationships: AccountId, ContactId, OpportunityId, CampaignId, AssignedToUserId, AssignedToGroupId, CreatedByUserId
- Metadata: Tags, Category, Attachments, CustomFields

**Enums:**
- `CrmTaskStatus`: NotStarted(0), InProgress(1), Completed(2), Deferred(3), Waiting(4), Cancelled(5)
- `CrmTaskPriority`: Low(0), Normal(1), High(2), Urgent(3)
- `CrmTaskType`: Call(0), Email(1), Meeting(2), FollowUp(3), Demo(4), Proposal(5), Contract(6), Research(7), Other(8)

### 3.2 DTOs

| DTO | File | Status |
|-----|------|--------|
| TaskStatistics | `CRM.Core/Interfaces/ITaskService.cs` | ✅ Implemented |

**TaskStatistics Properties:**
- Total, NotStarted, InProgress, Completed
- Overdue, DueToday, DueThisWeek

### 3.3 Interfaces

| Interface | File | Status | Methods |
|-----------|------|--------|---------|
| ITaskService | `CRM.Core/Interfaces/ITaskService.cs` | ✅ Implemented | 9 |

**ITaskService Methods:**
```csharp
Task<IEnumerable<CrmTask>> GetTasksAsync(int? customerId, int? opportunityId, int? assignedToUserId, CrmTaskStatus? status, CrmTaskPriority? priority, bool? overdue);
Task<CrmTask?> GetByIdAsync(int id);
Task<CrmTask> CreateAsync(CrmTask task);
Task<bool> UpdateAsync(int id, CrmTask task);
Task<bool> DeleteAsync(int id);
Task<bool> CompleteAsync(int id);
Task<IEnumerable<CrmTask>> GetOverdueTasksAsync();
Task<IEnumerable<CrmTask>> GetTasksDueTodayAsync(int? userId);
Task<TaskStatistics> GetStatisticsAsync(int? userId);
```

### 3.4 Services

| Service | File | Status | Lines |
|---------|------|--------|-------|
| TaskService | `CRM.Infrastructure/Services/TaskService.cs` | ✅ Implemented | ~329 |

**TaskService Implementation:**
- Full CRUD operations with soft delete
- Multi-field filtering (customer, opportunity, assignee, status, priority, overdue)
- Ordering by priority, due date, created date
- Completion tracking with CompletedDate
- Statistics aggregation (total, by status, overdue, due today, due this week)
- Comprehensive logging

### 3.5 Controllers

| Controller | File | Status | Lines | Endpoints |
|------------|------|--------|-------|-----------|
| TasksController | `CRM.Api/Controllers/TasksController.cs` | ✅ Implemented | ~334 | 8 |

**TasksController Endpoints:**

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/tasks` | Get all tasks with optional filters |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create new task |
| PUT | `/api/tasks/{id}` | Update existing task |
| DELETE | `/api/tasks/{id}` | Delete task |
| POST | `/api/tasks/{id}/complete` | Mark task as complete |
| GET | `/api/tasks/due-today` | Get tasks due today |
| GET | `/api/tasks/overdue` | Get overdue tasks |
| GET | `/api/tasks/my-queue` | Get user's workflow queue |

### 3.6 Backend Validations

| Field | Validation | Location | Status |
|-------|------------|----------|--------|
| Subject | Required, MaxLength(500) | Entity | ✅ Implemented |
| Description | MaxLength(5000) | Entity | ✅ Implemented |
| PercentComplete | Range(0, 100) | Entity | ✅ Implemented |
| EstimatedMinutes | Range(0, 525600) | Entity | ✅ Implemented |
| ActualMinutes | Range(0, 525600) | Entity | ✅ Implemented |
| Tags | MaxLength(500) | Entity | ✅ Implemented |
| Category | MaxLength(100) | Entity | ✅ Implemented |
| RecurrencePattern | MaxLength(500) | Entity | ✅ Implemented |

---

## 4. Database Implementation

### 4.1 Tables

| Table | Status | Primary Key |
|-------|--------|-------------|
| CrmTasks | ✅ Implemented | Id (int, auto-increment) |

### 4.2 Columns

| Column | Type | Nullable | Default | Status |
|--------|------|----------|---------|--------|
| Id | int | No | Auto | ✅ |
| Subject | varchar(500) | No | - | ✅ |
| Description | varchar(5000) | Yes | - | ✅ |
| TaskType | int | No | 8 (Other) | ✅ |
| Status | int | No | 0 (NotStarted) | ✅ |
| Priority | int | No | 1 (Normal) | ✅ |
| DueDate | datetime | Yes | - | ✅ |
| StartDate | datetime | Yes | - | ✅ |
| CompletedDate | datetime | Yes | - | ✅ |
| ReminderDate | datetime | Yes | - | ✅ |
| HasReminder | bit | No | false | ✅ |
| PercentComplete | int | No | 0 | ✅ |
| EstimatedMinutes | int | Yes | - | ✅ |
| ActualMinutes | int | Yes | - | ✅ |
| IsRecurring | bit | No | false | ✅ |
| RecurrencePattern | varchar(500) | Yes | - | ✅ |
| RecurrenceEndDate | datetime | Yes | - | ✅ |
| ParentTaskId | int | Yes | FK | ✅ |
| CustomerId/AccountId | int | Yes | FK | ✅ |
| ContactId | int | Yes | FK | ✅ |
| OpportunityId | int | Yes | FK | ✅ |
| CampaignId | int | Yes | FK | ✅ |
| AssignedToUserId | int | Yes | FK | ✅ |
| AssignedToGroupId | int | Yes | FK | ✅ |
| CreatedByUserId | int | Yes | FK | ✅ |
| Tags | varchar(500) | Yes | - | ✅ |
| Category | varchar(100) | Yes | - | ✅ |
| Attachments | varchar(5000) | Yes | - | ✅ |
| CustomFields | varchar(10000) | Yes | - | ✅ |
| CreatedAt | datetime | No | CURRENT_TIMESTAMP | ✅ |
| UpdatedAt | datetime | Yes | - | ✅ |
| IsDeleted | bit | No | false | ✅ |

### 4.3 Relationships

| From | To | Type | FK Column |
|------|-----|------|-----------|
| CrmTasks | Customers | Many-to-One | CustomerId (AccountId in code) |
| CrmTasks | Contacts | Many-to-One | ContactId |
| CrmTasks | Opportunities | Many-to-One | OpportunityId |
| CrmTasks | MarketingCampaigns | Many-to-One | CampaignId |
| CrmTasks | Users | Many-to-One | AssignedToUserId |
| CrmTasks | UserGroups | Many-to-One | AssignedToGroupId |
| CrmTasks | Users | Many-to-One | CreatedByUserId |
| CrmTasks | CrmTasks | Self-Reference | ParentTaskId |

### 4.4 Indexes

| Index | Columns | Status |
|-------|---------|--------|
| IX_CrmTasks_AccountId | AccountId | ✅ Implemented |
| IX_CrmTasks_AssignedToUserId | AssignedToUserId | ✅ Implemented |
| IX_CrmTasks_Status | Status | ✅ Implemented |
| IX_CrmTasks_DueDate | DueDate | ✅ Implemented |
| IX_CrmTasks_IsDeleted | IsDeleted | ✅ Implemented |

---

## 5. Testing

### 5.1 Unit Tests

| Test File | Status | Test Count |
|-----------|--------|------------|
| `TasksControllerTests.cs` | ✅ Implemented | ~990 lines |

**Test Coverage:**
- CRUD operations (Create, Read, Update, Delete)
- Assignment (User, Group)
- Priority handling
- Status transitions
- Completion flow
- Reminders
- Recurring tasks
- Overdue filtering
- Due today filtering
- My Queue functionality

### 5.2 Integration Tests

| Test Suite | Status |
|------------|--------|
| TasksController API Tests | ✅ Implemented (in TasksControllerTests.cs) |

### 5.3 E2E Tests

| Test File | Status |
|-----------|--------|
| Tasks E2E Tests | ⚠️ Partial (basic CRUD via BVT) |

---

## 6. Issues & Inconsistencies

### 6.1 Naming Issues

| Issue | Location | Recommendation |
|-------|----------|----------------|
| CustomerId vs AccountId | Entity has Column("CustomerId") but code uses AccountId | Align naming in migration |
| title vs Subject | Frontend uses 'title', backend uses 'Subject' | Standardize on 'Subject' |

### 6.2 Validation Gaps

| Gap | Impact | Priority |
|-----|--------|----------|
| No DueDate > StartDate validation | Invalid date ranges possible | P2 |
| No EstimatedMinutes > 0 for certain types | Could track 0-minute tasks | P3 |

### 6.3 Feature Gaps

| Gap | Description | Priority |
|-----|-------------|----------|
| No dedicated taskService.ts | Frontend uses apiClient directly | P3 |
| No TaskForm component | Form embedded in page | P3 |
| No TaskCard component | No card view available | P3 |
| Task notifications | No email/push notifications for reminders | P2 |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-CRM007-001 | Create dedicated taskService.ts for frontend | P3 | Frontend |
| TODO-CRM007-002 | Extract TaskForm.tsx component | P3 | Frontend |
| TODO-CRM007-003 | Add TaskCard.tsx for card/grid view | P3 | Frontend |
| TODO-CRM007-004 | Add DueDate > StartDate validation | P2 | Validation |
| TODO-CRM007-005 | Implement task reminder notifications | P2 | Backend |
| TODO-CRM007-006 | Add subtask management UI | P3 | Frontend |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02 | System | Initial specification from existing implementation |

---

## 9. References

- [DATABASE_SCHEMA.md](../../database/DATABASE_SCHEMA.md) - CrmTasks table definition
- [SPEC-CRM-001](SPEC-CRM-001-AccountManagement.md) - Account Management (task linking)
- [SPEC-CRM-003](SPEC-CRM-003-OpportunityManagement.md) - Opportunity Management (task linking)
- [SPEC-CRM-004](SPEC-CRM-004-ContactManagement.md) - Contact Management (task linking)
