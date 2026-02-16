# Feature Specification: Web Form Builder

> **Spec ID:** SPEC-MKT-004  
> **Feature:** Web Form Builder  
> **Module:** Marketing  
> **Version:** 1.0  
> **Last Updated:** February 12, 2026  
> **Status:** ✅ Implemented (Entity Layer)

---

## 1. Business Context

### 1.1 Feature Description
Web form builder for creating lead capture forms, contact forms, event registrations, and surveys. Supports 23 field types, conditional logic, progressive profiling, multi-step forms, and integration with lead management.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Form CRUD | Create, read, update, delete forms | ⚠️ Entity Only |
| SF-002 | Field Types | Text, Email, Phone, Dropdown, Checkbox, etc. (23 types) | ✅ Entity Implemented |
| SF-003 | Field Validation | Required, format, min/max | ✅ Entity Implemented |
| SF-004 | Conditional Fields | Show/hide based on other fields | ⚠️ Entity Only |
| SF-005 | Multi-Step Forms | Break form into steps/pages | ⚠️ Entity Only |
| SF-006 | Submit Actions | Message, Redirect, ShowForm, StayOnPage | ✅ Entity Implemented |
| SF-007 | Form Styling | CSS customization, themes | ⚠️ Entity Only |
| SF-008 | Submission Handling | Lead/Contact creation, notifications | ✅ Entity Implemented |
| SF-009 | Spam Protection | Honeypot, reCAPTCHA, rate limiting | ✅ Entity Implemented |
| SF-010 | Form Analytics | Views, submissions, conversion rates | ⚠️ Entity Only |
| SF-011 | Form Embedding | JavaScript embed, iframe | ⚠️ Entity Only |
| SF-012 | Progressive Profiling | Pre-fill known data, ask new questions | ❌ Not Implemented |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Create Form | Marketer | Logged in | Form saved | ⚠️ |
| UC-002 | Add Fields | Marketer | Form exists | Fields configured | ⚠️ |
| UC-003 | Configure Validation | Marketer | Field exists | Validation set | ⚠️ |
| UC-004 | Publish Form | Marketer | Form valid | Form published | ⚠️ |
| UC-005 | View Submissions | Marketer | Form has submissions | Data displayed | ⚠️ |
| UC-006 | Convert to Lead | System | Submission received | Lead created | ⚠️ |
| UC-007 | Embed Form | Marketer | Form published | Embed code copied | ❌ |
| UC-008 | View Analytics | Marketer | Form has data | Analytics shown | ❌ |
| UC-009 | Export Submissions | Marketer | Submissions exist | Data exported | ❌ |
| UC-010 | Duplicate Form | Marketer | Form exists | Copy created | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| FormBuilderPage | - | ❌ | Not Found |
| FormSubmissionsPage | - | ❌ | Not Found |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| FormBuilder | - | ❌ | Not Found |
| FormFieldEditor | - | ❌ | Not Found |
| FormPreview | - | ❌ | Not Found |
| SubmissionsList | - | ❌ | Not Found |

### 2.3 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| formBuilderService | - | - | ❌ Not Found |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| FormDefinition | `CRM.Core/Entities/FormDefinition.cs` | ✅ | 573 lines |
| FormField | `CRM.Core/Entities/FormDefinition.cs` | ✅ | Embedded |
| FormSubmission | `CRM.Core/Entities/FormDefinition.cs` | ✅ | Embedded |
| FormFieldValue | `CRM.Core/Entities/FormDefinition.cs` | ✅ | Embedded |

### 3.2 Enums
| Enum | Values | File Path | Status |
|------|--------|-----------|--------|
| FormFieldType | Text, TextArea, Email, Phone, Number, Date, DateTime, Dropdown, MultiSelect, Radio, Checkbox, CheckboxGroup, FileUpload, Hidden, Country, State, Url, Rating, Range, Signature, Consent, Captcha, RichText | FormDefinition.cs | ✅ |
| FormStatus | Draft, Published, Paused, Archived, Expired, Testing | FormDefinition.cs | ✅ |
| FormSubmitAction | ShowMessage, Redirect, ShowForm, StayOnPage, CloseModal | FormDefinition.cs | ✅ |
| SubmissionStatus | New, Processing, LeadCreated, ContactCreated, SubmittedExternal, Failed, Spam, Duplicate, ManualReview | FormDefinition.cs | ✅ |
| FieldValidationType | Required, Email, Phone, Url, Pattern, MinLength, MaxLength, Min, Max, DateRange, FileSize, FileType | FormDefinition.cs | ✅ |

### 3.3 Entity Properties - FormDefinition
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| Name | string | Yes | - | Form name |
| InternalName | string | No | - | System identifier |
| Description | string | No | - | Form description |
| Status | FormStatus | Yes | Draft | Current status |
| FormType | string | No | - | Contact/Lead/Registration/Survey |
| Fields | List<FormField> | Yes | - | Navigation |
| Submissions | List<FormSubmission> | Yes | - | Navigation |
| SubmitButtonText | string | No | "Submit" | Button label |
| SubmitAction | FormSubmitAction | Yes | ShowMessage | Post-submit action |
| SuccessMessage | string | No | - | Thank you message |
| RedirectUrl | string | No | - | Redirect URL |
| FailureMessage | string | No | - | Error message |
| CreateLeadOnSubmit | bool | Yes | false | Auto-create lead |
| CreateContactOnSubmit | bool | Yes | false | Auto-create contact |
| NotifyOnSubmission | bool | Yes | false | Email notification |
| NotificationEmails | string | No | - | Comma-separated emails |
| CssClass | string | No | - | Custom CSS class |
| CustomCss | string | No | - | Inline CSS styles |
| IsMultiStep | bool | Yes | false | Multi-step form |
| TotalSteps | int | Yes | 1 | Step count |
| EnableProgressBar | bool | Yes | false | Show progress |
| EnableCaptcha | bool | Yes | false | CAPTCHA enabled |
| CaptchaType | string | No | - | reCAPTCHA/hCaptcha |
| CaptchaSiteKey | string | No | - | Public key |
| HoneypotEnabled | bool | Yes | true | Spam protection |
| RateLimitEnabled | bool | Yes | false | Rate limiting |
| RateLimitPerMinute | int | Yes | 5 | Rate limit value |
| TotalViews | int | Yes | 0 | Analytics |
| TotalSubmissions | int | Yes | 0 | Analytics |
| ConversionRate | decimal | Yes | 0 | Analytics |
| PublishedAt | DateTime? | No | - | Publish timestamp |
| ExpiresAt | DateTime? | No | - | Expiration |
| CampaignId | int? | No | - | FK→MarketingCampaigns |
| OwnerId | int? | No | - | FK→Users |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.4 Entity Properties - FormField
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| FormId | int | Yes | - | FK→FormDefinitions |
| FieldName | string | Yes | - | Internal name |
| Label | string | Yes | - | Display label |
| FieldType | FormFieldType | Yes | Text | Field type |
| Placeholder | string | No | - | Placeholder text |
| HelpText | string | No | - | Help/tooltip text |
| DefaultValue | string | No | - | Default value |
| IsRequired | bool | Yes | false | Required field |
| IsReadOnly | bool | Yes | false | Read-only field |
| IsHidden | bool | Yes | false | Hidden field |
| ValidationRules | string | No | - | JSON validation rules |
| Options | string | No | - | JSON options for dropdowns |
| DisplayOrder | int | Yes | 0 | Field order |
| Width | string | No | "100%" | Field width |
| CssClass | string | No | - | Custom CSS class |
| StepNumber | int | Yes | 1 | Multi-step position |
| ConditionalLogic | string | No | - | JSON conditions |
| MapToLeadField | string | No | - | Lead field mapping |
| MapToContactField | string | No | - | Contact field mapping |
| MinLength | int? | No | - | Min string length |
| MaxLength | int? | No | - | Max string length |
| MinValue | decimal? | No | - | Min numeric value |
| MaxValue | decimal? | No | - | Max numeric value |
| Pattern | string | No | - | Regex pattern |
| AllowedFileTypes | string | No | - | File type extensions |
| MaxFileSize | int? | No | - | Max file size (KB) |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.5 Entity Properties - FormSubmission
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| FormId | int | Yes | - | FK→FormDefinitions |
| Status | SubmissionStatus | Yes | New | Current status |
| FieldValues | List<FormFieldValue> | Yes | - | Navigation |
| SubmittedAt | DateTime | Yes | NOW | Submission time |
| IpAddress | string | No | - | Submitter IP |
| UserAgent | string | No | - | Browser info |
| Referrer | string | No | - | Referring URL |
| PageUrl | string | No | - | Submission page |
| LeadId | int? | No | - | Created lead FK |
| ContactId | int? | No | - | Created contact FK |
| VisitorId | int? | No | - | FK→WebVisitors |
| SessionId | string | No | - | Session identifier |
| UtmSource | string | No | - | UTM source |
| UtmMedium | string | No | - | UTM medium |
| UtmCampaign | string | No | - | UTM campaign |
| UtmContent | string | No | - | UTM content |
| UtmTerm | string | No | - | UTM term |
| ProcessedAt | DateTime? | No | - | Processing timestamp |
| ProcessingNotes | string | No | - | Processing notes |
| IsSpam | bool | Yes | false | Spam flag |
| SpamScore | decimal? | No | - | Spam probability |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.6 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IFormBuilderService | - | - | ❌ Not Found |

### 3.7 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| FormBuilderService | - | - | ❌ Not Found |

### 3.8 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| FormsController | - | - | ❌ Not Found |

### 3.9 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/forms` | GetAll | Yes | ❌ |
| GET | `/api/forms/{id}` | GetById | Yes | ❌ |
| POST | `/api/forms` | Create | Yes | ❌ |
| PUT | `/api/forms/{id}` | Update | Yes | ❌ |
| DELETE | `/api/forms/{id}` | Delete | Yes | ❌ |
| POST | `/api/forms/{id}/publish` | Publish | Yes | ❌ |
| POST | `/api/forms/{id}/unpublish` | Unpublish | Yes | ❌ |
| GET | `/api/forms/{id}/fields` | GetFields | Yes | ❌ |
| POST | `/api/forms/{id}/fields` | AddField | Yes | ❌ |
| PUT | `/api/forms/{id}/fields/{fieldId}` | UpdateField | Yes | ❌ |
| DELETE | `/api/forms/{id}/fields/{fieldId}` | RemoveField | Yes | ❌ |
| GET | `/api/forms/{id}/submissions` | GetSubmissions | Yes | ❌ |
| GET | `/api/forms/{id}/submissions/{submissionId}` | GetSubmission | Yes | ❌ |
| POST | `/api/forms/{id}/submit` | Submit | No | ❌ (Public) |
| GET | `/api/forms/{id}/embed` | GetEmbedCode | Yes | ❌ |
| GET | `/api/forms/{id}/analytics` | GetAnalytics | Yes | ❌ |
| POST | `/api/forms/{id}/clone` | Clone | Yes | ❌ |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | Schema File | Status | Notes |
|------------|-------------|--------|-------|
| FormDefinitions | `database/schema/002_marketing_tables.sql` | ✅ | Form definitions |
| FormFields | `database/schema/002_marketing_tables.sql` | ✅ | Field definitions |
| FormSubmissions | `database/schema/002_marketing_tables.sql` | ✅ | Submission data |
| FormFieldValues | `database/schema/002_marketing_tables.sql` | ✅ | Field values |

### 4.2 Indexes
| Index Name | Columns | Type | Status |
|------------|---------|------|--------|
| IX_FormDefinitions_Status | Status | Non-clustered | ✅ |
| IX_FormFields_FormId | FormId | Non-clustered | ✅ |
| IX_FormSubmissions_FormId | FormId | Non-clustered | ✅ |
| IX_FormSubmissions_SubmittedAt | SubmittedAt | Non-clustered | ✅ |
| IX_FormSubmissions_LeadId | LeadId | Non-clustered | ✅ |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | File Path | Test Count | Status |
|------------|-----------|------------|--------|
| FormBuilderServiceTests | - | - | ❌ Not Found |

---

## 6. Known Issues

### 6.1 Implementation Gaps
| Issue | Current State | Required State | Priority |
|-------|---------------|----------------|----------|
| No service layer | Entity only | Full service | High |
| No controller | Entity only | Full REST API | High |
| No frontend | Entity only | Full form builder UI | High |
| No public submit endpoint | Entity only | Anonymous submission | High |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-MKT004-001 | Create IFormBuilderService interface | P1 | Backend |
| TODO-MKT004-002 | Implement FormBuilderService | P1 | Backend |
| TODO-MKT004-003 | Create FormsController | P1 | Backend |
| TODO-MKT004-004 | Create public submit endpoint | P1 | Backend |
| TODO-MKT004-005 | Create FormBuilderPage.tsx | P1 | Frontend |
| TODO-MKT004-006 | Create FormSubmissionsPage.tsx | P1 | Frontend |
| TODO-MKT004-007 | Create drag-drop form builder | P1 | Frontend |
| TODO-MKT004-008 | Create form preview component | P2 | Frontend |
| TODO-MKT004-009 | Create embed code generator | P2 | Frontend |
| TODO-MKT004-010 | Create unit tests | P2 | Testing |
| TODO-MKT004-011 | Implement CAPTCHA integration | P2 | Backend |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | System | Initial specification created |
