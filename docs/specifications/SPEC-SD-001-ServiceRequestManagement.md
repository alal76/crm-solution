# SPEC-SD-001: Service Request Management

> **Module:** Service Desk  
> **Feature:** Service Request Management  
> **Version:** 1.0  
> **Last Updated:** 2026-02-12  
> **Status:** ✅ Complete  
> **Dependencies:** CRM-001 (Account Management), CRM-004 (Contact Management)

---

## 1. Business Context

### 1.1 Overview

Service Request Management is the core feature of the Service Desk module, enabling customer support ticket creation, tracking, assignment, resolution, and feedback collection across multiple communication channels.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Priority |
|----|-------------|-------------|----------|
| SD001-SF01 | Ticket Creation | Create tickets from multiple channels | P0 |
| SD001-SF02 | Ticket Tracking | Track status, SLA, and history | P0 |
| SD001-SF03 | Assignment Management | Assign to users/groups | P0 |
| SD001-SF04 | Categorization | Category/subcategory/type hierarchy | P1 |
| SD001-SF05 | Custom Fields | Configurable fields per category | P1 |
| SD001-SF06 | Status Workflow | Status transitions and lifecycle | P0 |
| SD001-SF07 | Channel Integration | Multi-channel support | P1 |
| SD001-SF08 | Resolution Tracking | Resolution summary, root cause | P0 |
| SD001-SF09 | Customer Feedback | Satisfaction rating collection | P1 |
| SD001-SF10 | Ticket Linking | Parent/child relationships | P2 |
| SD001-SF11 | Expedite Handling | Priority elevation workflow | P2 |
| SD001-SF12 | Statistics & Reporting | Request metrics and dashboards | P1 |

### 1.3 Functionalities

| ID | Functionality | Sub-Feature | Description |
|----|---------------|-------------|-------------|
| SD001-F01 | Create from Portal | SF01 | Self-service ticket creation |
| SD001-F02 | Create from Email | SF01 | Email-to-ticket conversion |
| SD001-F03 | Create from WhatsApp | SF01 | WhatsApp message to ticket |
| SD001-F04 | Create from Phone | SF01 | Agent creates during call |
| SD001-F05 | Create from LiveChat | SF01 | Chat escalation to ticket |
| SD001-F06 | Generate Ticket Number | SF01 | Unique ticket ID generation |
| SD001-F07 | View Ticket Details | SF02 | Full ticket information |
| SD001-F08 | Update Ticket | SF02 | Modify ticket properties |
| SD001-F09 | View SLA Status | SF02 | Response/resolution SLA times |
| SD001-F10 | Assign to User | SF03 | Direct user assignment |
| SD001-F11 | Assign to Group | SF03 | Group/queue assignment |
| SD001-F12 | Auto-Assignment | SF03 | Rule-based assignment |
| SD001-F13 | Select Category | SF04 | Category selection |
| SD001-F14 | Select Subcategory | SF04 | Subcategory filtering |
| SD001-F15 | Select Type | SF04 | Request type with templates |
| SD001-F16 | Define Custom Fields | SF05 | Configure 15 custom fields |
| SD001-F17 | Enter Custom Values | SF05 | Fill custom field values |
| SD001-F18 | Change Status | SF06 | Status transitions |
| SD001-F19 | Mark First Response | SF06 | Record first response time |
| SD001-F20 | Resolve Ticket | SF06 | Complete resolution workflow |
| SD001-F21 | Close Ticket | SF06 | Final closure |
| SD001-F22 | Reopen Ticket | SF06 | Reopen closed tickets |
| SD001-F23 | Enter Resolution | SF08 | Resolution details |
| SD001-F24 | Record Root Cause | SF08 | Root cause analysis |
| SD001-F25 | Submit Feedback | SF09 | Customer satisfaction survey |
| SD001-F26 | Link Tickets | SF10 | Parent/child linking |
| SD001-F27 | Request Expedite | SF11 | Expedite request workflow |
| SD001-F28 | View Statistics | SF12 | Dashboard metrics |

### 1.4 Use Cases

| ID | Use Case | Actor | Description |
|----|----------|-------|-------------|
| SD001-UC01 | Customer submits request | Customer | Create ticket via portal |
| SD001-UC02 | Agent creates ticket | Support Agent | Create on behalf of customer |
| SD001-UC03 | Supervisor assigns ticket | Supervisor | Manual assignment |
| SD001-UC04 | System auto-assigns | System | Automatic routing |
| SD001-UC05 | Agent resolves ticket | Support Agent | Complete resolution |
| SD001-UC06 | Customer rates service | Customer | Feedback submission |
| SD001-UC07 | Manager reviews metrics | Manager | Dashboard analysis |

---

## 2. Frontend

### 2.1 Pages

| Page | Route | Description | Status |
|------|-------|-------------|--------|
| ServiceRequestsPage | /service-requests | List all service requests | ⚠️ Partial |
| ServiceRequestDetailsPage | /service-requests/:id | View/edit single request | ⚠️ Partial |
| ServiceRequestCreatePage | /service-requests/new | Create new request | ⚠️ Partial |
| CategoriesAdminPage | /admin/service-categories | Manage categories | ⚠️ Partial |
| CustomFieldsAdminPage | /admin/service-custom-fields | Manage custom fields | ⚠️ Partial |

### 2.2 Components

| Component | Location | Description | Status |
|-----------|----------|-------------|--------|
| ServiceRequestList | components/service-desk/ | Request data grid | ⚠️ Partial |
| ServiceRequestForm | components/service-desk/ | Create/edit form | ⚠️ Partial |
| ServiceRequestCard | components/service-desk/ | Request summary card | ❌ Not Found |
| ServiceRequestTimeline | components/service-desk/ | Activity timeline | ❌ Not Found |
| CategorySelector | components/service-desk/ | Category dropdown | ⚠️ Partial |
| SubcategorySelector | components/service-desk/ | Subcategory dropdown | ⚠️ Partial |
| TypeSelector | components/service-desk/ | Type selection | ⚠️ Partial |
| CustomFieldRenderer | components/service-desk/ | Dynamic field rendering | ❌ Not Found |
| AssignmentPanel | components/service-desk/ | User/group assignment | ❌ Not Found |
| SLAStatusBadge | components/service-desk/ | SLA status indicator | ❌ Not Found |
| StatusTransitionButtons | components/service-desk/ | Status change actions | ❌ Not Found |
| ResolutionForm | components/service-desk/ | Resolution entry | ❌ Not Found |
| FeedbackForm | components/service-desk/ | Customer satisfaction | ❌ Not Found |
| ServiceRequestStats | components/service-desk/ | Statistics widgets | ❌ Not Found |

### 2.3 Services

| Service | File | Description | Status |
|---------|------|-------------|--------|
| serviceRequestService | src/services/serviceRequestService.ts | Service request API | ✅ Implemented |
| serviceCategoryService | src/services/serviceCategoryService.ts | Category API | ⚠️ Partial |

### 2.4 Frontend Validations

| Field | Validation | Error Message |
|-------|------------|---------------|
| Subject | Required, 3-500 chars | Subject must be between 3 and 500 characters |
| Description | Max 10,000 chars | Description cannot exceed 10,000 characters |
| Category | Required when creating | Please select a category |
| RequesterEmail | Valid email format | Please enter a valid email address |
| RequesterPhone | Valid phone format | Please enter a valid phone number |
| SatisfactionRating | 1-5 if provided | Rating must be between 1 and 5 |
| ResolutionSummary | Required for resolution | Resolution summary is required |

---

## 3. Backend

### 3.1 Entities

| Entity | File | Description |
|--------|------|-------------|
| ServiceRequest | CRM.Core/Entities/ServiceRequest.cs | Main service request entity |
| ServiceRequestCategory | CRM.Core/Entities/ServiceRequest.cs | Category definition |
| ServiceRequestSubcategory | CRM.Core/Entities/ServiceRequest.cs | Subcategory definition |
| ServiceRequestType | CRM.Core/Entities/ServiceRequest.cs | Type with templates |
| ServiceRequestCustomFieldDefinition | CRM.Core/Entities/ServiceRequest.cs | Custom field schema |
| ServiceRequestCustomFieldValue | CRM.Core/Entities/ServiceRequest.cs | Custom field values |

### 3.2 Enums

| Enum | Values | Description |
|------|--------|-------------|
| ServiceRequestChannel | WhatsApp, Email, Phone, InPerson, SelfServicePortal, SocialMedia, LiveChat, API | Request source channel |
| ServiceRequestStatus | New, Open, InProgress, PendingCustomer, PendingInternal, Escalated, Resolved, Closed, Cancelled, OnHold, Reopened | Request status |
| ServiceRequestPriority | Low, Medium, High, Critical, Urgent | Priority level |
| CustomFieldType | Text, TextArea, Number, Decimal, Date, DateTime, Dropdown, MultiSelect, Boolean, Email, Phone, Url | Field types |

### 3.3 DTOs

| DTO | Purpose | Location |
|-----|---------|----------|
| ServiceRequestDto | Full request data | CRM.Core/Dtos/ |
| ServiceRequestListDto | List view data | CRM.Core/Dtos/ |
| CreateServiceRequestDto | Creation input | CRM.Core/Dtos/ |
| UpdateServiceRequestDto | Update input | CRM.Core/Dtos/ |
| ServiceRequestFilterDto | Search/filter parameters | CRM.Core/Dtos/ |
| PagedServiceRequestResult | Paginated results | CRM.Core/Dtos/ |
| ServiceRequestCategoryDto | Category data | CRM.Core/Dtos/ |
| ServiceRequestSubcategoryDto | Subcategory data | CRM.Core/Dtos/ |
| ServiceRequestTypeDto | Type data | CRM.Core/Dtos/ |
| ServiceRequestCustomFieldDefinitionDto | Field definition | CRM.Core/Dtos/ |
| ServiceRequestCustomFieldValueDto | Field value | CRM.Core/Dtos/ |
| ServiceRequestStatisticsDto | Statistics data | CRM.Core/Dtos/ |
| SetCustomFieldValueDto | Set field value | CRM.Core/Dtos/ |

### 3.4 Service Interfaces

| Interface | File | Status |
|-----------|------|--------|
| IServiceRequestService | CRM.Core/Interfaces/IServiceRequestService.cs | ✅ Implemented |
| IServiceRequestCategoryService | CRM.Core/Interfaces/IServiceRequestService.cs | ✅ Implemented |
| IServiceRequestSubcategoryService | CRM.Core/Interfaces/IServiceRequestService.cs | ✅ Implemented |
| IServiceRequestCustomFieldService | CRM.Core/Interfaces/IServiceRequestService.cs | ✅ Implemented |
| IServiceRequestTypeService | CRM.Core/Interfaces/IServiceRequestService.cs | ✅ Implemented |

### 3.5 Service Methods

#### IServiceRequestService

| Method | Signature | Description |
|--------|-----------|-------------|
| GetServiceRequestsAsync | `(ServiceRequestFilterDto filter) → PagedServiceRequestResult` | Paginated list with filters |
| GetServiceRequestByIdAsync | `(int id) → ServiceRequestDto?` | Get by ID |
| GetServiceRequestByTicketNumberAsync | `(string ticketNumber) → ServiceRequestDto?` | Get by ticket number |
| CreateServiceRequestAsync | `(CreateServiceRequestDto dto, int? createdByUserId) → ServiceRequestDto` | Create new request |
| UpdateServiceRequestAsync | `(int id, UpdateServiceRequestDto dto, int? modifiedByUserId) → ServiceRequestDto` | Update request |
| DeleteServiceRequestAsync | `(int id) → bool` | Soft delete |
| GetServiceRequestsByCustomerAsync | `(int customerId) → List<ServiceRequestListDto>` | By customer |
| GetServiceRequestsByContactAsync | `(int contactId) → List<ServiceRequestListDto>` | By contact |
| GetServiceRequestsByAssigneeAsync | `(int userId) → List<ServiceRequestListDto>` | By assignee |
| GetServiceRequestsByGroupAsync | `(int groupId) → List<ServiceRequestListDto>` | By group |
| UpdateStatusAsync | `(int id, ServiceRequestStatus newStatus, int? modifiedByUserId) → ServiceRequestDto` | Change status |
| MarkFirstResponseAsync | `(int id, int? userId) → ServiceRequestDto` | Record first response |
| ResolveServiceRequestAsync | `(int id, string resolutionSummary, string? resolutionCode, string? rootCause, int? resolvedByUserId) → ServiceRequestDto` | Resolve |
| CloseServiceRequestAsync | `(int id, int? closedByUserId) → ServiceRequestDto` | Close |
| ReopenServiceRequestAsync | `(int id, string reason, int? reopenedByUserId) → ServiceRequestDto` | Reopen |
| EscalateServiceRequestAsync | `(int id, string reason, int? escalatedByUserId) → ServiceRequestDto` | Escalate |
| AssignToUserAsync | `(int id, int userId, int? assignedByUserId) → ServiceRequestDto` | Assign to user |
| AssignToGroupAsync | `(int id, int groupId, int? assignedByUserId) → ServiceRequestDto` | Assign to group |
| UnassignAsync | `(int id, int? modifiedByUserId) → ServiceRequestDto` | Unassign |
| SetCustomFieldValuesAsync | `(int serviceRequestId, List<SetCustomFieldValueDto> values) → void` | Set custom fields |
| GetCustomFieldValuesAsync | `(int serviceRequestId) → List<ServiceRequestCustomFieldValueDto>` | Get custom fields |
| SubmitFeedbackAsync | `(int id, int rating, string? feedback) → ServiceRequestDto` | Submit feedback |
| GetStatisticsAsync | `() → ServiceRequestStatisticsDto` | Get statistics |
| GetOpenRequestsCountAsync | `() → int` | Count open |
| GetSlaBreachedCountAsync | `() → int` | Count breached |

### 3.6 Controllers

| Controller | Route | File | Status |
|------------|-------|------|--------|
| ServiceRequestsController | /api/servicerequests | CRM.Api/Controllers/ServiceRequestsController.cs | ✅ Implemented |
| ServiceCategoriesController | /api/service-categories | CRM.Api/Controllers/ | ✅ Implemented |

### 3.7 API Endpoints

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| GET | /api/servicerequests | List with pagination/filters | ✅ |
| GET | /api/servicerequests/{id} | Get by ID | ✅ |
| GET | /api/servicerequests/ticket/{ticketNumber} | Get by ticket number | ✅ |
| POST | /api/servicerequests | Create new request | ✅ |
| PUT | /api/servicerequests/{id} | Update request | ✅ |
| DELETE | /api/servicerequests/{id} | Delete (soft) | ✅ |
| GET | /api/servicerequests/customer/{customerId} | By customer | ✅ |
| GET | /api/servicerequests/contact/{contactId} | By contact | ✅ |
| GET | /api/servicerequests/assignee/{userId} | By assignee | ✅ |
| GET | /api/servicerequests/group/{groupId} | By group | ✅ |
| PUT | /api/servicerequests/{id}/status | Change status | ✅ |
| POST | /api/servicerequests/{id}/first-response | Mark first response | ✅ |
| POST | /api/servicerequests/{id}/resolve | Resolve | ✅ |
| POST | /api/servicerequests/{id}/close | Close | ✅ |
| POST | /api/servicerequests/{id}/reopen | Reopen | ✅ |
| POST | /api/servicerequests/{id}/escalate | Escalate | ✅ |
| POST | /api/servicerequests/{id}/assign/user/{userId} | Assign to user | ✅ |
| POST | /api/servicerequests/{id}/assign/group/{groupId} | Assign to group | ✅ |
| POST | /api/servicerequests/{id}/unassign | Unassign | ✅ |
| PUT | /api/servicerequests/{id}/custom-fields | Set custom fields | ✅ |
| GET | /api/servicerequests/{id}/custom-fields | Get custom fields | ✅ |
| POST | /api/servicerequests/{id}/feedback | Submit feedback | ✅ |
| GET | /api/servicerequests/statistics | Get statistics | ✅ |
| GET | /api/service-categories | List categories | ✅ |
| GET | /api/service-categories/{id} | Get category | ✅ |
| POST | /api/service-categories | Create category | ✅ |
| PUT | /api/service-categories/{id} | Update category | ✅ |
| DELETE | /api/service-categories/{id} | Delete category | ✅ |
| GET | /api/service-categories/{categoryId}/subcategories | List subcategories | ✅ |

### 3.8 Backend Validations

| Field | Validation | Error Message |
|-------|------------|---------------|
| TicketNumber | Required, unique, max 50 | Ticket number is required and must be unique |
| Subject | Required, 3-500 chars | Subject must be between 3 and 500 characters |
| Description | Max 10,000 chars | Description cannot exceed 10,000 characters |
| RequesterEmail | Valid email if provided | Invalid email format |
| RequesterPhone | Valid phone if provided | Invalid phone format |
| CategoryId | Must exist if provided | Invalid category |
| SubcategoryId | Must belong to category | Subcategory must belong to selected category |
| SatisfactionRating | 1-5 if provided | Rating must be between 1 and 5 |
| Status transitions | Valid transitions only | Invalid status transition |
| CustomFieldDefinition.Name | Required, max 100 | Field name is required |
| CustomFieldDefinition.FieldKey | Required, unique, max 100 | Field key is required and must be unique |
| CustomFieldDefinition.DisplayOrder | 1-15 | Display order must be between 1 and 15 |

---

## 4. Database

### 4.1 Tables

#### ServiceRequests

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| TicketNumber | VARCHAR(50) | NOT NULL, UNIQUE | Unique ticket number |
| Subject | VARCHAR(500) | NOT NULL | Request subject |
| Description | TEXT | | Detailed description |
| Channel | INT | NOT NULL | ServiceRequestChannel enum |
| Status | INT | NOT NULL, DEFAULT 0 | ServiceRequestStatus enum |
| Priority | INT | NOT NULL, DEFAULT 1 | ServiceRequestPriority enum |
| CategoryId | INT | FK | Category reference |
| SubcategoryId | INT | FK | Subcategory reference |
| AccountId | INT | FK | Customer/Account reference |
| ContactId | INT | FK | Contact reference |
| RequesterName | VARCHAR(200) | | Anonymous requester name |
| RequesterEmail | VARCHAR(200) | | Requester email |
| RequesterPhone | VARCHAR(50) | | Requester phone |
| AssignedToUserId | INT | FK | Assigned user |
| AssignedToGroupId | INT | FK | Assigned group |
| CreatedByUserId | INT | FK | Created by |
| LastModifiedByUserId | INT | FK | Modified by |
| ResponseDueDate | DATETIME | | Response SLA deadline |
| ResolutionDueDate | DATETIME | | Resolution SLA deadline |
| FirstResponseDate | DATETIME | | Actual first response |
| ResolvedDate | DATETIME | | Actual resolution |
| ClosedDate | DATETIME | | Closure date |
| ResponseSlaBreached | BIT | DEFAULT 0 | Response SLA breached |
| ResolutionSlaBreached | BIT | DEFAULT 0 | Resolution SLA breached |
| ExternalReferenceId | VARCHAR(500) | | External ID |
| SourcePhoneNumber | VARCHAR(50) | | Source phone |
| SourceEmailAddress | VARCHAR(200) | | Source email |
| ConversationId | VARCHAR(500) | | Thread ID |
| ResolutionSummary | TEXT | | Resolution details |
| ResolutionCode | VARCHAR(100) | | Resolution code |
| RootCause | VARCHAR(1000) | | Root cause |
| SatisfactionRating | INT | | 1-5 rating |
| CustomerFeedback | TEXT | | Feedback text |
| Tags | VARCHAR(500) | | Comma-separated tags |
| InternalNotes | TEXT | | Internal notes |
| EscalationLevel | INT | DEFAULT 0 | Escalation level |
| ReopenCount | INT | DEFAULT 0 | Reopen count |
| IsVipCustomer | BIT | DEFAULT 0 | VIP flag |
| EstimatedEffortHours | DECIMAL(10,2) | | Estimated effort |
| ActualEffortHours | DECIMAL(10,2) | | Actual effort |
| IsExpedited | BIT | DEFAULT 0 | Expedite flag |
| ExpediteReason | VARCHAR(500) | | Expedite reason |
| ExpeditedByUserId | INT | FK | Expedited by |
| ExpeditedAt | DATETIME | | Expedite timestamp |
| ParentServiceRequestId | INT | FK | Parent request |
| RelatedOpportunityId | INT | FK | Related opportunity |
| RelatedProductId | INT | FK | Related product |
| SourceInteractionId | INT | FK | Source interaction |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestCategories

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| Name | VARCHAR(200) | NOT NULL | Category name |
| Description | VARCHAR(500) | | Description |
| Code | VARCHAR(50) | | Category code |
| IconName | VARCHAR(100) | | Icon identifier |
| Color | VARCHAR(20) | | Display color |
| DisplayOrder | INT | DEFAULT 0 | Sort order |
| IsActive | BIT | DEFAULT 1 | Active flag |
| DefaultResponseTimeHours | INT | | Default response SLA |
| DefaultResolutionTimeHours | INT | | Default resolution SLA |
| DefaultPriority | INT | | Default priority |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestSubcategories

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| CategoryId | INT | FK, NOT NULL | Parent category |
| Name | VARCHAR(200) | NOT NULL | Subcategory name |
| Description | VARCHAR(500) | | Description |
| Code | VARCHAR(50) | | Subcategory code |
| DisplayOrder | INT | DEFAULT 0 | Sort order |
| IsActive | BIT | DEFAULT 1 | Active flag |
| OverrideResponseTimeHours | INT | | Override response SLA |
| OverrideResolutionTimeHours | INT | | Override resolution SLA |
| OverridePriority | INT | | Override priority |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestTypes

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| CategoryId | INT | FK | Category reference |
| SubcategoryId | INT | FK | Subcategory reference |
| Name | VARCHAR(200) | NOT NULL | Type name |
| Description | VARCHAR(500) | | Description |
| Code | VARCHAR(50) | | Type code |
| WorkflowTemplateId | INT | FK | Workflow template |
| ResolutionTemplateText | TEXT | | Resolution template |
| DisplayOrder | INT | DEFAULT 0 | Sort order |
| IsActive | BIT | DEFAULT 1 | Active flag |
| DefaultPriority | INT | | Default priority |
| ResponseTimeHours | INT | | Response SLA |
| ResolutionTimeHours | INT | | Resolution SLA |
| Tags | VARCHAR(500) | | Tags |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestCustomFieldDefinitions

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| Name | VARCHAR(100) | NOT NULL | Field name |
| FieldKey | VARCHAR(100) | NOT NULL, UNIQUE | Field key |
| Description | VARCHAR(500) | | Description |
| FieldType | INT | NOT NULL | CustomFieldType enum |
| IsRequired | BIT | DEFAULT 0 | Required flag |
| IsActive | BIT | DEFAULT 1 | Active flag |
| DisplayOrder | INT | DEFAULT 0 | Sort order (1-15) |
| DefaultValue | VARCHAR(500) | | Default value |
| Placeholder | VARCHAR(200) | | Placeholder text |
| HelpText | VARCHAR(500) | | Help text |
| DropdownOptions | VARCHAR(2000) | | JSON options array |
| MinValue | DECIMAL | | Minimum value |
| MaxValue | DECIMAL | | Maximum value |
| MaxLength | INT | | Maximum length |
| ValidationPattern | VARCHAR(500) | | Regex pattern |
| ValidationMessage | VARCHAR(200) | | Error message |
| CategoryId | INT | FK | Restrict to category |
| SubcategoryId | INT | FK | Restrict to subcategory |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestCustomFieldValues

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| ServiceRequestId | INT | FK, NOT NULL | Service request |
| CustomFieldDefinitionId | INT | FK, NOT NULL | Field definition |
| TextValue | VARCHAR(4000) | | Text storage |
| NumericValue | DECIMAL | | Number storage |
| DateValue | DATETIME | | Date storage |
| BooleanValue | BIT | | Boolean storage |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

### 4.2 Indexes

| Index | Table | Columns | Type |
|-------|-------|---------|------|
| IX_ServiceRequests_TicketNumber | ServiceRequests | TicketNumber | UNIQUE |
| IX_ServiceRequests_Status | ServiceRequests | Status | INDEX |
| IX_ServiceRequests_Priority | ServiceRequests | Priority | INDEX |
| IX_ServiceRequests_AccountId | ServiceRequests | AccountId | INDEX |
| IX_ServiceRequests_ContactId | ServiceRequests | ContactId | INDEX |
| IX_ServiceRequests_AssignedToUserId | ServiceRequests | AssignedToUserId | INDEX |
| IX_ServiceRequests_AssignedToGroupId | ServiceRequests | AssignedToGroupId | INDEX |
| IX_ServiceRequests_CategoryId | ServiceRequests | CategoryId | INDEX |
| IX_ServiceRequests_CreatedAt | ServiceRequests | CreatedAt | INDEX |
| IX_ServiceRequests_ResponseDue | ServiceRequests | ResponseDueDate | INDEX |
| IX_ServiceRequests_ResolutionDue | ServiceRequests | ResolutionDueDate | INDEX |
| IX_Categories_DisplayOrder | ServiceRequestCategories | DisplayOrder | INDEX |
| IX_Subcategories_CategoryId | ServiceRequestSubcategories | CategoryId | INDEX |
| IX_Types_SubcategoryId | ServiceRequestTypes | SubcategoryId | INDEX |
| IX_CustomFields_FieldKey | ServiceRequestCustomFieldDefinitions | FieldKey | UNIQUE |
| IX_FieldValues_ServiceRequestId | ServiceRequestCustomFieldValues | ServiceRequestId | INDEX |

---

## 5. Tests

### 5.1 Unit Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| ServiceRequestServiceTests | CreateServiceRequest_ValidData_Success | Create with valid data | ✅ |
| ServiceRequestServiceTests | CreateServiceRequest_GeneratesTicketNumber | Auto-generate ticket | ✅ |
| ServiceRequestServiceTests | UpdateStatus_ValidTransition_Success | Valid status change | ✅ |
| ServiceRequestServiceTests | UpdateStatus_InvalidTransition_Throws | Invalid status change | ✅ |
| ServiceRequestServiceTests | AssignToUser_ValidUser_Success | User assignment | ✅ |
| ServiceRequestServiceTests | AssignToGroup_ValidGroup_Success | Group assignment | ✅ |
| ServiceRequestServiceTests | ResolveServiceRequest_ValidData_Success | Resolution | ✅ |
| ServiceRequestServiceTests | SubmitFeedback_ValidRating_Success | Feedback submission | ✅ |

### 5.2 Integration Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| ServiceRequestsControllerTests | GetServiceRequests_ReturnsPaginated | Pagination | ✅ |
| ServiceRequestsControllerTests | CreateServiceRequest_Returns201 | Create endpoint | ✅ |
| ServiceRequestsControllerTests | UpdateStatus_Returns200 | Status change | ✅ |
| ServiceRequestsControllerTests | GetByTicketNumber_ReturnsRequest | Ticket lookup | ✅ |

### 5.3 E2E Tests

| Test File | Test | Description | Status |
|-----------|------|-------------|--------|
| service-requests.spec.ts | Create service request | End-to-end creation | ❌ Not Found |
| service-requests.spec.ts | Assign to agent | Assignment workflow | ❌ Not Found |
| service-requests.spec.ts | Resolve and close | Resolution workflow | ❌ Not Found |
| service-requests.spec.ts | Submit feedback | Feedback workflow | ❌ Not Found |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SD001-ISS01 | Frontend components incomplete | Medium | Many UI components not yet implemented |
| SD001-ISS02 | E2E tests missing | Medium | No Playwright tests for service requests |
| SD001-ISS03 | CustomFieldRenderer missing | Low | Dynamic field rendering not implemented |
| SD001-ISS04 | SLA integration needed | Medium | SLA calculation should be automatic |
| SD001-ISS05 | Channel integrations stub | Medium | WhatsApp/Email channels need full implementation |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SD001-001 | Create ServiceRequestCard component | P2 | Frontend |
| TODO-SD001-002 | Create ServiceRequestTimeline component | P2 | Frontend |
| TODO-SD001-003 | Create CustomFieldRenderer component | P2 | Frontend |
| TODO-SD001-004 | Create AssignmentPanel component | P2 | Frontend |
| TODO-SD001-005 | Create SLAStatusBadge component | P2 | Frontend |
| TODO-SD001-006 | Create StatusTransitionButtons component | P2 | Frontend |
| TODO-SD001-007 | Create ResolutionForm component | P2 | Frontend |
| TODO-SD001-008 | Create FeedbackForm component | P2 | Frontend |
| TODO-SD001-009 | Create ServiceRequestStats component | P2 | Frontend |
| TODO-SD001-010 | Create E2E tests for service requests | P2 | Testing |
| TODO-SD001-011 | Implement email-to-ticket integration | P1 | Backend |
| TODO-SD001-012 | Implement auto-assignment rules | P1 | Backend |
| TODO-SD001-013 | Add SLA auto-calculation on create | P1 | Backend |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-12 | 1.0 | System | Initial specification |

---

**END OF SPECIFICATION**
