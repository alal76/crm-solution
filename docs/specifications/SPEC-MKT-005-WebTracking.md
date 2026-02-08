# Feature Specification: Web Tracking

> **Spec ID:** SPEC-MKT-005  
> **Feature:** Web Tracking  
> **Module:** Marketing  
> **Version:** 1.0  
> **Last Updated:** February 12, 2026  
> **Status:** ✅ Implemented (Entity Layer)

---

## 1. Business Context

### 1.1 Feature Description
Website visitor tracking and analytics for identifying anonymous visitors, tracking page views, sessions, engagement metrics, and attribution. Integrates with lead management and campaign attribution for full customer journey visibility.

### 1.2 Sub-Features
| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Visitor Tracking | Track anonymous and known visitors | ✅ Entity Implemented |
| SF-002 | Session Management | Track visitor sessions | ✅ Entity Implemented |
| SF-003 | Page View Tracking | Track page views with metadata | ✅ Entity Implemented |
| SF-004 | Visitor Identification | Multiple identification sources | ✅ Entity Implemented |
| SF-005 | UTM Attribution | Track campaign UTM parameters | ✅ Entity Implemented |
| SF-006 | First/Last Touch | Attribution models | ✅ Entity Implemented |
| SF-007 | Engagement Scoring | Score based on activity | ✅ Entity Implemented |
| SF-008 | Company Lookup | Identify company from IP | ⚠️ Entity Only |
| SF-009 | Real-Time Alerts | Notify on high-value visits | ❌ Not Implemented |
| SF-010 | Visitor Timeline | Complete activity history | ⚠️ Entity Only |
| SF-011 | Tracking Script | JavaScript tracking code | ❌ Not Implemented |
| SF-012 | GDPR Compliance | Cookie consent integration | ⚠️ Entity Only |

### 1.3 Use Cases
| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | Track Page View | Visitor | Tracking enabled | View recorded | ⚠️ |
| UC-002 | Identify Visitor | System | Form submitted | Visitor identified | ⚠️ |
| UC-003 | View Visitor Profile | Sales Rep | Visitor exists | Profile displayed | ⚠️ |
| UC-004 | View Session Details | Sales Rep | Session exists | Details shown | ⚠️ |
| UC-005 | Track Attribution | System | UTM params exist | Attribution saved | ⚠️ |
| UC-006 | Score Engagement | System | Activity recorded | Score updated | ⚠️ |
| UC-007 | Get Tracking Script | Marketer | Account exists | Script generated | ❌ |
| UC-008 | View Campaign Attribution | Marketer | Touchpoints exist | Report shown | ⚠️ |
| UC-009 | Export Visitor Data | GDPR Request | Visitor identified | Data exported | ❌ |
| UC-010 | Delete Visitor Data | GDPR Request | Visitor identified | Data deleted | ⚠️ |

---

## 2. Frontend Implementation

### 2.1 Pages
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| WebTrackingPage | - | ❌ | Not Found |
| VisitorDetailPage | - | ❌ | Not Found |

### 2.2 Components
| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| VisitorList | - | ❌ | Not Found |
| VisitorProfile | - | ❌ | Not Found |
| SessionTimeline | - | ❌ | Not Found |
| AttributionReport | - | ❌ | Not Found |

### 2.3 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| webTrackingService | - | - | ❌ Not Found |

---

## 3. Backend Implementation

### 3.1 Entities
| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| WebVisitor | `CRM.Core/Entities/WebVisitor.cs` | ✅ | 439 lines |
| WebSession | `CRM.Core/Entities/WebVisitor.cs` | ✅ | Embedded |
| WebPageView | `CRM.Core/Entities/WebVisitor.cs` | ✅ | Embedded |
| AttributionSettings | `CRM.Core/Entities/WebVisitor.cs` | ✅ | Embedded |
| CampaignTouchpoint | `CRM.Core/Entities/WebVisitor.cs` | ✅ | Embedded |
| CampaignAttributionSummary | `CRM.Core/Entities/WebVisitor.cs` | ✅ | Embedded |

### 3.2 Enums
| Enum | Values | File Path | Status |
|------|--------|-----------|--------|
| VisitorIdentificationSource | Anonymous, FormSubmission, EmailClick, Login, Chat, Cookie, CompanyLookup, Social, Manual | WebVisitor.cs | ✅ |
| PageCategory | Home, Product, Pricing, Features, Blog, CaseStudy, Documentation, Demo, Contact, Careers, About, Support, Other | WebVisitor.cs | ✅ |
| AttributionModel | FirstTouch, LastTouch, Linear, TimeDecay, PositionBased, Custom | WebVisitor.cs | ✅ |
| TouchpointType | Organic, Paid, Social, Email, Referral, Direct, Other | WebVisitor.cs | ✅ |

### 3.3 Entity Properties - WebVisitor
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| VisitorId | string | Yes | - | Unique visitor ID (cookie) |
| Email | string | No | - | Identified email |
| FirstName | string | No | - | First name |
| LastName | string | No | - | Last name |
| Company | string | No | - | Company name |
| Phone | string | No | - | Phone number |
| IsIdentified | bool | Yes | false | Known visitor |
| IdentificationSource | VisitorIdentificationSource | Yes | Anonymous | How identified |
| IdentifiedAt | DateTime? | No | - | When identified |
| IpAddress | string | No | - | IP address |
| Country | string | No | - | GeoIP country |
| City | string | No | - | GeoIP city |
| Region | string | No | - | GeoIP region |
| Timezone | string | No | - | Timezone |
| Device | string | No | - | Device type |
| Browser | string | No | - | Browser name |
| BrowserVersion | string | No | - | Browser version |
| OperatingSystem | string | No | - | OS name |
| ScreenResolution | string | No | - | Screen size |
| Language | string | No | - | Browser language |
| TotalSessions | int | Yes | 0 | Session count |
| TotalPageViews | int | Yes | 0 | Page view count |
| TotalTimeOnSite | int | Yes | 0 | Total seconds |
| FirstVisitAt | DateTime | Yes | NOW | First visit |
| LastVisitAt | DateTime? | No | - | Last visit |
| FirstTouchSource | string | No | - | First touch attribution |
| FirstTouchMedium | string | No | - | First touch medium |
| FirstTouchCampaign | string | No | - | First touch campaign |
| LastTouchSource | string | No | - | Last touch attribution |
| LastTouchMedium | string | No | - | Last touch medium |
| LastTouchCampaign | string | No | - | Last touch campaign |
| EngagementScore | int | Yes | 0 | Engagement level |
| LeadScore | int | Yes | 0 | Lead potential |
| LeadId | int? | No | - | FK→Leads |
| ContactId | int? | No | - | FK→Contacts |
| CustomerId | int? | No | - | FK→Customers |
| HasConsentedToTracking | bool | Yes | false | GDPR consent |
| ConsentedAt | DateTime? | No | - | Consent timestamp |
| Sessions | List<WebSession> | Yes | - | Navigation |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.4 Entity Properties - WebSession
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| VisitorId | int | Yes | - | FK→WebVisitors |
| SessionId | string | Yes | - | Unique session ID |
| StartedAt | DateTime | Yes | NOW | Session start |
| EndedAt | DateTime? | No | - | Session end |
| DurationSeconds | int | Yes | 0 | Session duration |
| PageViews | int | Yes | 0 | Views in session |
| LandingPage | string | No | - | Entry page URL |
| LandingPageTitle | string | No | - | Entry page title |
| ExitPage | string | No | - | Exit page URL |
| Referrer | string | No | - | Referring URL |
| ReferrerDomain | string | No | - | Referring domain |
| UtmSource | string | No | - | UTM source |
| UtmMedium | string | No | - | UTM medium |
| UtmCampaign | string | No | - | UTM campaign |
| UtmContent | string | No | - | UTM content |
| UtmTerm | string | No | - | UTM term |
| IpAddress | string | No | - | Session IP |
| Device | string | No | - | Device type |
| Browser | string | No | - | Browser info |
| IsBounce | bool | Yes | false | Single page session |
| IsConverted | bool | Yes | false | Led to conversion |
| ConversionType | string | No | - | Conversion type |
| ConversionValue | decimal? | No | - | Conversion value |
| Pages | List<WebPageView> | Yes | - | Navigation |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| UpdatedAt | DateTime? | No | - | Modified timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.5 Entity Properties - WebPageView
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| SessionId | int | Yes | - | FK→WebSessions |
| Url | string | Yes | - | Page URL |
| Path | string | Yes | - | URL path |
| Title | string | No | - | Page title |
| Category | PageCategory | Yes | Other | Page category |
| ViewedAt | DateTime | Yes | NOW | View timestamp |
| TimeOnPage | int | Yes | 0 | Seconds on page |
| ScrollDepth | int | Yes | 0 | Max scroll % |
| ClickCount | int | Yes | 0 | Clicks on page |
| FormSubmitted | bool | Yes | false | Form submitted |
| FormId | int? | No | - | FK→FormDefinitions |
| QueryString | string | No | - | URL query params |
| Referrer | string | No | - | Previous page |
| CustomData | string | No | - | JSON custom events |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.6 Entity Properties - CampaignTouchpoint
| Property | Type | Required | Default | Notes |
|----------|------|----------|---------|-------|
| Id | int | Yes | AUTO | Primary key |
| VisitorId | int | Yes | - | FK→WebVisitors |
| CampaignId | int? | No | - | FK→MarketingCampaigns |
| SessionId | int | Yes | - | FK→WebSessions |
| TouchpointType | TouchpointType | Yes | Other | Channel type |
| Source | string | No | - | UTM source |
| Medium | string | No | - | UTM medium |
| Campaign | string | No | - | UTM campaign |
| Content | string | No | - | UTM content |
| Term | string | No | - | UTM term |
| TouchpointAt | DateTime | Yes | NOW | Touchpoint time |
| IsFirstTouch | bool | Yes | false | First touchpoint |
| IsLastTouch | bool | Yes | false | Last touchpoint |
| IsConversionTouch | bool | Yes | false | Conversion touch |
| AttributionWeight | decimal | Yes | 0 | Attribution % |
| LandingPage | string | No | - | Entry page |
| CreatedAt | DateTime | Yes | NOW | Created timestamp |
| IsDeleted | bool | Yes | false | Soft delete flag |

### 3.7 Interfaces
| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IWebTrackingService | - | - | ❌ Not Found |

### 3.8 Services
| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| WebTrackingService | - | - | ❌ Not Found |

### 3.9 Controllers
| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| WebTrackingController | - | - | ❌ Not Found |

### 3.10 API Endpoints
| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/tracking/visitors` | GetVisitors | Yes | ❌ |
| GET | `/api/tracking/visitors/{id}` | GetVisitor | Yes | ❌ |
| GET | `/api/tracking/visitors/{id}/sessions` | GetSessions | Yes | ❌ |
| GET | `/api/tracking/visitors/{id}/pages` | GetPageViews | Yes | ❌ |
| GET | `/api/tracking/visitors/{id}/timeline` | GetTimeline | Yes | ❌ |
| GET | `/api/tracking/sessions/{id}` | GetSession | Yes | ❌ |
| POST | `/api/tracking/identify` | Identify | No* | ❌ (Public) |
| POST | `/api/tracking/pageview` | TrackPageView | No* | ❌ (Public) |
| POST | `/api/tracking/event` | TrackEvent | No* | ❌ (Public) |
| GET | `/api/tracking/script` | GetTrackingScript | Yes | ❌ |
| GET | `/api/tracking/attribution` | GetAttributionReport | Yes | ❌ |
| GET | `/api/tracking/attribution/touchpoints` | GetTouchpoints | Yes | ❌ |
| DELETE | `/api/tracking/visitors/{id}` | DeleteVisitor | Yes | ❌ (GDPR) |
| GET | `/api/tracking/visitors/{id}/export` | ExportVisitorData | Yes | ❌ (GDPR) |

---

## 4. Database Implementation

### 4.1 Tables
| Table Name | Schema File | Status | Notes |
|------------|-------------|--------|-------|
| WebVisitors | `database/schema/002_marketing_tables.sql` | ✅ | Visitor records |
| WebSessions | `database/schema/002_marketing_tables.sql` | ✅ | Session data |
| WebPageViews | `database/schema/002_marketing_tables.sql` | ✅ | Page view data |
| AttributionSettings | `database/schema/002_marketing_tables.sql` | ✅ | Attribution config |
| CampaignTouchpoints | `database/schema/002_marketing_tables.sql` | ✅ | Touchpoint data |
| CampaignAttributionSummaries | `database/schema/002_marketing_tables.sql` | ✅ | Attribution summaries |

### 4.2 Indexes
| Index Name | Columns | Type | Status |
|------------|---------|------|--------|
| IX_WebVisitors_VisitorId | VisitorId | Unique | ✅ |
| IX_WebVisitors_Email | Email | Non-clustered | ✅ |
| IX_WebVisitors_LeadId | LeadId | Non-clustered | ✅ |
| IX_WebSessions_VisitorId | VisitorId | Non-clustered | ✅ |
| IX_WebSessions_SessionId | SessionId | Unique | ✅ |
| IX_WebPageViews_SessionId | SessionId | Non-clustered | ✅ |
| IX_WebPageViews_ViewedAt | ViewedAt | Non-clustered | ✅ |
| IX_CampaignTouchpoints_VisitorId | VisitorId | Non-clustered | ✅ |

---

## 5. Tests

### 5.1 Unit Tests
| Test Class | File Path | Test Count | Status |
|------------|-----------|------------|--------|
| WebTrackingServiceTests | - | - | ❌ Not Found |

---

## 6. Known Issues

### 6.1 Implementation Gaps
| Issue | Current State | Required State | Priority |
|-------|---------------|----------------|----------|
| No tracking service | Entity only | Full service | High |
| No tracking controller | Entity only | Full REST API | High |
| No public tracking endpoints | Entity only | Anonymous endpoints | High |
| No JavaScript tracking script | Not implemented | Full tracking.js | High |
| No real-time visitor alerts | Not implemented | WebSocket alerts | Medium |

---

## 7. TODO Items

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-MKT005-001 | Create IWebTrackingService interface | P1 | Backend |
| TODO-MKT005-002 | Implement WebTrackingService | P1 | Backend |
| TODO-MKT005-003 | Create WebTrackingController | P1 | Backend |
| TODO-MKT005-004 | Create public tracking endpoints | P1 | Backend |
| TODO-MKT005-005 | Create JavaScript tracking script | P1 | Frontend |
| TODO-MKT005-006 | Create WebTrackingPage.tsx | P1 | Frontend |
| TODO-MKT005-007 | Create VisitorDetailPage.tsx | P1 | Frontend |
| TODO-MKT005-008 | Create visitor timeline component | P2 | Frontend |
| TODO-MKT005-009 | Implement GeoIP lookup | P2 | Backend |
| TODO-MKT005-010 | Implement company identification | P2 | Backend |
| TODO-MKT005-011 | Create GDPR export/delete endpoints | P2 | Backend |
| TODO-MKT005-012 | Create unit tests | P2 | Testing |

---

## 8. Change History

| Date | Author | Changes |
|------|--------|---------|
| 2026-02-12 | System | Initial specification created |
