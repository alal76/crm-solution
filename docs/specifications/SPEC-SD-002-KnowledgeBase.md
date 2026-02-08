# SPEC-SD-002: Knowledge Base

> **Module:** Service Desk  
> **Feature:** Knowledge Base  
> **Version:** 1.0  
> **Last Updated:** 2026-02-12  
> **Status:** ✅ Complete  
> **Dependencies:** SD-001 (Service Request Management)

---

## 1. Business Context

### 1.1 Overview

Knowledge Base provides a centralized repository for documentation, FAQs, troubleshooting guides, and best practices that support both self-service and agent-assisted support workflows. Features AI-powered suggestions and case deflection tracking.

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Priority |
|----|-------------|-------------|----------|
| SD002-SF01 | Article Management | Create, edit, version articles | P0 |
| SD002-SF02 | Category Management | Hierarchical category organization | P0 |
| SD002-SF03 | Article Publishing | Draft, review, publish workflow | P0 |
| SD002-SF04 | Search & Discovery | Full-text and AI-powered search | P1 |
| SD002-SF05 | Article Feedback | User ratings and comments | P1 |
| SD002-SF06 | Case Deflection | Track self-service success | P1 |
| SD002-SF07 | AI Integration | AI summaries and suggestions | P2 |
| SD002-SF08 | Multi-Language | Localization support | P2 |
| SD002-SF09 | SEO Optimization | Slug, meta tags, sitemap | P2 |
| SD002-SF10 | Analytics | View counts, engagement metrics | P1 |
| SD002-SF11 | Article Linking | Link articles to service requests | P1 |
| SD002-SF12 | Version Control | Article version history | P2 |

### 1.3 Functionalities

| ID | Functionality | Sub-Feature | Description |
|----|---------------|-------------|-------------|
| SD002-F01 | Create Article | SF01 | Author new knowledge article |
| SD002-F02 | Edit Article | SF01 | Modify existing article |
| SD002-F03 | Delete Article | SF01 | Remove article (soft delete) |
| SD002-F04 | Clone Article | SF01 | Duplicate article |
| SD002-F05 | Create Category | SF02 | Add new category |
| SD002-F06 | Create Subcategory | SF02 | Add child category |
| SD002-F07 | Reorder Categories | SF02 | Change category order |
| SD002-F08 | Submit for Review | SF03 | Author submits draft |
| SD002-F09 | Approve Article | SF03 | Reviewer approves |
| SD002-F10 | Publish Article | SF03 | Make article visible |
| SD002-F11 | Unpublish Article | SF03 | Remove from visibility |
| SD002-F12 | Archive Article | SF03 | Archive old content |
| SD002-F13 | Search Articles | SF04 | Full-text search |
| SD002-F14 | Filter by Category | SF04 | Category filtering |
| SD002-F15 | AI Search | SF04 | Semantic search |
| SD002-F16 | Rate Article | SF05 | Helpful/not helpful |
| SD002-F17 | Leave Comment | SF05 | User feedback |
| SD002-F18 | Track Deflection | SF06 | Record case deflected |
| SD002-F19 | Generate Summary | SF07 | AI article summary |
| SD002-F20 | Suggest Related | SF07 | AI related articles |
| SD002-F21 | Create Translation | SF08 | Add language version |
| SD002-F22 | Set SEO Metadata | SF09 | Configure SEO fields |
| SD002-F23 | View Analytics | SF10 | View engagement stats |
| SD002-F24 | Link to Request | SF11 | Associate with ticket |
| SD002-F25 | View History | SF12 | See version changes |
| SD002-F26 | Restore Version | SF12 | Revert to previous |

### 1.4 Use Cases

| ID | Use Case | Actor | Description |
|----|----------|-------|-------------|
| SD002-UC01 | Customer searches KB | Customer | Find answer to question |
| SD002-UC02 | Customer rates article | Customer | Provide feedback |
| SD002-UC03 | Agent uses KB | Support Agent | Find solution for ticket |
| SD002-UC04 | Agent links article | Support Agent | Attach article to ticket |
| SD002-UC05 | Author creates article | KB Author | Write new content |
| SD002-UC06 | Reviewer approves | KB Reviewer | Review and publish |
| SD002-UC07 | Admin manages categories | Admin | Organize KB structure |
| SD002-UC08 | Manager views analytics | Manager | Review KB effectiveness |

---

## 2. Frontend

### 2.1 Pages

| Page | Route | Description | Status |
|------|-------|-------------|--------|
| KnowledgeBasePage | /knowledge-base | Public KB home | ⚠️ Partial |
| ArticleViewPage | /knowledge-base/articles/:id | View article | ⚠️ Partial |
| ArticleSearchPage | /knowledge-base/search | Search results | ⚠️ Partial |
| ArticleEditorPage | /admin/knowledge/articles/:id/edit | Edit article | ⚠️ Partial |
| ArticleCreatePage | /admin/knowledge/articles/new | Create article | ⚠️ Partial |
| CategoryManagementPage | /admin/knowledge/categories | Manage categories | ⚠️ Partial |
| KBAnalyticsPage | /admin/knowledge/analytics | KB analytics | ❌ Not Found |

### 2.2 Components

| Component | Location | Description | Status |
|-----------|----------|-------------|--------|
| ArticleList | components/knowledge/ | Article listing | ⚠️ Partial |
| ArticleCard | components/knowledge/ | Article preview card | ⚠️ Partial |
| ArticleViewer | components/knowledge/ | Article content display | ⚠️ Partial |
| ArticleEditor | components/knowledge/ | WYSIWYG editor | ⚠️ Partial |
| CategoryTree | components/knowledge/ | Hierarchical categories | ❌ Not Found |
| CategorySelector | components/knowledge/ | Category picker | ⚠️ Partial |
| ArticleFeedbackWidget | components/knowledge/ | Rating/feedback form | ❌ Not Found |
| RelatedArticles | components/knowledge/ | AI suggestions panel | ❌ Not Found |
| ArticleSearchBar | components/knowledge/ | Search input | ⚠️ Partial |
| PopularArticles | components/knowledge/ | Popular articles list | ❌ Not Found |
| RecentArticles | components/knowledge/ | Recent articles list | ❌ Not Found |
| ArticleMetrics | components/knowledge/ | View/rating stats | ❌ Not Found |
| VersionHistory | components/knowledge/ | Version timeline | ❌ Not Found |
| PublishWorkflow | components/knowledge/ | Publish status stepper | ❌ Not Found |

### 2.3 Services

| Service | File | Description | Status |
|---------|------|-------------|--------|
| knowledgeService | src/services/knowledgeService.ts | Knowledge base API | ⚠️ Partial |
| knowledgeCategoryService | src/services/knowledgeCategoryService.ts | Category API | ⚠️ Partial |

### 2.4 Frontend Validations

| Field | Validation | Error Message |
|-------|------------|---------------|
| Title | Required, 5-500 chars | Title must be between 5 and 500 characters |
| Content | Required, min 50 chars | Content must be at least 50 characters |
| Slug | Required, URL-safe, unique | Slug must be unique and URL-friendly |
| CategoryId | Required | Please select a category |
| ArticleType | Required | Please select an article type |
| LanguageCode | Valid ISO code | Invalid language code |
| Rating | 1-5 if provided | Rating must be between 1 and 5 |

---

## 3. Backend

### 3.1 Entities

| Entity | File | Description |
|--------|------|-------------|
| KnowledgeArticle | CRM.Core/Entities/KnowledgeArticle.cs | Main article entity |
| KnowledgeCategory | CRM.Core/Entities/KnowledgeArticle.cs | Category entity |
| ServiceRequestArticle | CRM.Core/Entities/KnowledgeArticle.cs | Article-request link |
| ArticleFeedback | CRM.Core/Entities/KnowledgeArticle.cs | User feedback entity |

### 3.2 Enums

| Enum | Values | Description |
|------|--------|-------------|
| ArticleType | HowTo, FAQ, Troubleshooting, BestPractice, Documentation, Process, Policy, ReleaseNotes, Video, Template | Article content type |
| ArticleStatus | Draft, PendingReview, Approved, Published, Archived, Deprecated | Publishing status |
| ArticleVisibility | Internal, CustomerPortal, Public | Access level |

### 3.3 DTOs

| DTO | Purpose | Location |
|-----|---------|----------|
| KnowledgeArticleDto | Full article data | CRM.Core/Dtos/ |
| KnowledgeArticleListDto | List view data | CRM.Core/Dtos/ |
| CreateKnowledgeArticleDto | Creation input | CRM.Core/Dtos/ |
| UpdateKnowledgeArticleDto | Update input | CRM.Core/Dtos/ |
| KnowledgeArticleFilterDto | Search/filter parameters | CRM.Core/Dtos/ |
| KnowledgeCategoryDto | Category data | CRM.Core/Dtos/ |
| CreateKnowledgeCategoryDto | Category creation | CRM.Core/Dtos/ |
| ArticleFeedbackDto | Feedback data | CRM.Core/Dtos/ |
| SubmitFeedbackDto | Feedback submission | CRM.Core/Dtos/ |
| ArticleAnalyticsDto | Analytics data | CRM.Core/Dtos/ |

### 3.4 Service Interfaces

| Interface | File | Status |
|-----------|------|--------|
| IKnowledgeManagementService | CRM.Core/Interfaces/IITSMServices.cs | ✅ Implemented |

### 3.5 Service Methods

#### IKnowledgeManagementService

| Method | Signature | Description |
|--------|-----------|-------------|
| GetArticlesAsync | `(KnowledgeArticleFilterDto? filter) → IEnumerable<KnowledgeArticleListDto>` | List articles with filters |
| GetArticleByIdAsync | `(int id) → KnowledgeArticleDto?` | Get article by ID |
| GetArticleBySlugAsync | `(string slug) → KnowledgeArticleDto?` | Get by URL slug |
| CreateArticleAsync | `(CreateKnowledgeArticleDto dto) → KnowledgeArticleDto` | Create article |
| UpdateArticleAsync | `(int id, UpdateKnowledgeArticleDto dto) → KnowledgeArticleDto` | Update article |
| DeleteArticleAsync | `(int id) → bool` | Delete article |
| PublishArticleAsync | `(int id, int? publishedByUserId) → KnowledgeArticleDto` | Publish article |
| UnpublishArticleAsync | `(int id) → KnowledgeArticleDto` | Unpublish article |
| RetireArticleAsync | `(int id) → KnowledgeArticleDto` | Archive article |
| SubmitFeedbackAsync | `(int articleId, SubmitFeedbackDto dto) → ArticleFeedbackDto` | Submit feedback |
| GetFeedbackAsync | `(int articleId) → IEnumerable<ArticleFeedbackDto>` | Get article feedback |
| IncrementViewCountAsync | `(int articleId) → void` | Track page view |
| GetCategoriesAsync | `() → IEnumerable<KnowledgeCategoryDto>` | List categories |
| GetCategoryByIdAsync | `(int id) → KnowledgeCategoryDto?` | Get category |
| CreateCategoryAsync | `(CreateKnowledgeCategoryDto dto) → KnowledgeCategoryDto` | Create category |
| UpdateCategoryAsync | `(int id, UpdateKnowledgeCategoryDto dto) → KnowledgeCategoryDto` | Update category |
| DeleteCategoryAsync | `(int id) → bool` | Delete category |
| GetSuggestedArticlesAsync | `(int serviceRequestId) → IEnumerable<KnowledgeArticleListDto>` | AI suggestions for ticket |
| GetPopularArticlesAsync | `(int count) → IEnumerable<KnowledgeArticleListDto>` | Top viewed articles |
| GetRecentArticlesAsync | `(int count) → IEnumerable<KnowledgeArticleListDto>` | Recently published |
| SearchArticlesAsync | `(string query) → IEnumerable<KnowledgeArticleListDto>` | Full-text search |
| LinkArticleToRequestAsync | `(int articleId, int serviceRequestId, bool wasHelpful, bool deflectedCase) → ServiceRequestArticle` | Link to ticket |
| GetArticleAnalyticsAsync | `(int articleId, DateTime? fromDate, DateTime? toDate) → ArticleAnalyticsDto` | Get analytics |

### 3.6 Controllers

| Controller | Route | File | Status |
|------------|-------|------|--------|
| KnowledgeArticlesController | /api/knowledge/articles | CRM.Api/Controllers/ | ⚠️ Partial |
| KnowledgeCategoriesController | /api/knowledge/categories | CRM.Api/Controllers/ | ⚠️ Partial |

### 3.7 API Endpoints

| Method | Endpoint | Description | Status |
|--------|----------|-------------|--------|
| GET | /api/knowledge/articles | List with filters | ✅ |
| GET | /api/knowledge/articles/{id} | Get by ID | ✅ |
| GET | /api/knowledge/articles/slug/{slug} | Get by slug | ✅ |
| POST | /api/knowledge/articles | Create article | ✅ |
| PUT | /api/knowledge/articles/{id} | Update article | ✅ |
| DELETE | /api/knowledge/articles/{id} | Delete article | ✅ |
| POST | /api/knowledge/articles/{id}/publish | Publish article | ✅ |
| POST | /api/knowledge/articles/{id}/unpublish | Unpublish | ✅ |
| POST | /api/knowledge/articles/{id}/retire | Archive | ✅ |
| POST | /api/knowledge/articles/{id}/view | Track view | ✅ |
| GET | /api/knowledge/articles/{id}/feedback | Get feedback | ✅ |
| POST | /api/knowledge/articles/{id}/feedback | Submit feedback | ✅ |
| GET | /api/knowledge/articles/search | Search | ✅ |
| GET | /api/knowledge/articles/popular | Popular | ✅ |
| GET | /api/knowledge/articles/recent | Recent | ✅ |
| GET | /api/knowledge/articles/suggestions/{serviceRequestId} | AI suggestions | ✅ |
| POST | /api/knowledge/articles/{id}/link/{serviceRequestId} | Link to ticket | ✅ |
| GET | /api/knowledge/articles/{id}/analytics | Get analytics | ⚠️ Partial |
| GET | /api/knowledge/categories | List categories | ✅ |
| GET | /api/knowledge/categories/{id} | Get category | ✅ |
| POST | /api/knowledge/categories | Create category | ✅ |
| PUT | /api/knowledge/categories/{id} | Update category | ✅ |
| DELETE | /api/knowledge/categories/{id} | Delete category | ✅ |
| GET | /api/knowledge/categories/{id}/articles | Articles in category | ✅ |

### 3.8 Backend Validations

| Field | Validation | Error Message |
|-------|------------|---------------|
| Title | Required, 5-500 chars | Title must be between 5 and 500 characters |
| Content | Required, min 50 chars | Content must be at least 50 characters |
| Slug | Required, unique, URL-safe | Slug must be unique and URL-friendly |
| CategoryId | Required, must exist | Invalid category |
| ArticleType | Valid enum value | Invalid article type |
| Status | Valid transitions only | Invalid status transition |
| LanguageCode | Valid ISO 639-1 | Invalid language code |
| Visibility | Valid enum value | Invalid visibility |
| Rating | 1-5 | Rating must be between 1 and 5 |
| MetaTitle | Max 70 chars | Meta title cannot exceed 70 characters |
| MetaDescription | Max 160 chars | Meta description cannot exceed 160 characters |

---

## 4. Database

### 4.1 Tables

#### KnowledgeArticles

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| Title | VARCHAR(500) | NOT NULL | Article title |
| Content | TEXT | NOT NULL | Plain text content |
| ContentHtml | TEXT | | HTML formatted content |
| Excerpt | VARCHAR(1000) | | Short summary |
| ArticleType | INT | NOT NULL | ArticleType enum |
| Status | INT | NOT NULL, DEFAULT 0 | ArticleStatus enum |
| Visibility | INT | NOT NULL, DEFAULT 0 | ArticleVisibility enum |
| CategoryId | INT | FK, NOT NULL | Category reference |
| Slug | VARCHAR(500) | UNIQUE | URL-friendly slug |
| Tags | VARCHAR(500) | | Comma-separated tags |
| Keywords | VARCHAR(500) | | Search keywords |
| MetaTitle | VARCHAR(70) | | SEO title |
| MetaDescription | VARCHAR(160) | | SEO description |
| AuthorUserId | INT | FK | Article author |
| LastUpdatedByUserId | INT | FK | Last editor |
| ReviewedByUserId | INT | FK | Reviewer |
| ApprovedByUserId | INT | FK | Approver |
| PublishedByUserId | INT | FK | Publisher |
| SubmittedForReviewAt | DATETIME | | Submit timestamp |
| ReviewedAt | DATETIME | | Review timestamp |
| ApprovedAt | DATETIME | | Approval timestamp |
| PublishedAt | DATETIME | | Publish timestamp |
| LastPublishedAt | DATETIME | | Last publish |
| ExpiresAt | DATETIME | | Expiration date |
| RetiredAt | DATETIME | | Archive date |
| ViewCount | INT | DEFAULT 0 | Total views |
| UniqueVisitorCount | INT | DEFAULT 0 | Unique visitors |
| HelpfulCount | INT | DEFAULT 0 | Helpful votes |
| NotHelpfulCount | INT | DEFAULT 0 | Not helpful votes |
| HelpfulnessScore | DECIMAL(5,2) | | Calculated score |
| AverageRating | DECIMAL(3,2) | | Average rating |
| RatingCount | INT | DEFAULT 0 | Number of ratings |
| CaseDeflectionCount | INT | DEFAULT 0 | Cases deflected |
| IsFeatured | BIT | DEFAULT 0 | Featured flag |
| IsPinned | BIT | DEFAULT 0 | Pinned to top |
| LanguageCode | VARCHAR(10) | DEFAULT 'en' | ISO language |
| ParentArticleId | INT | FK | Translation parent |
| Version | INT | DEFAULT 1 | Content version |
| VersionNotes | VARCHAR(500) | | Version description |
| EmbeddingVectorJson | TEXT | | AI embeddings |
| AISummary | TEXT | | AI-generated summary |
| RelatedArticleIdsJson | VARCHAR(500) | | Related article IDs |
| AISuggestionsJson | TEXT | | AI suggestions |
| EstimatedReadTimeMinutes | INT | | Reading time |
| AttachmentsJson | TEXT | | File attachments |
| ExternalLinks | VARCHAR(2000) | | External resources |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### KnowledgeCategories

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| Name | VARCHAR(200) | NOT NULL | Category name |
| Description | VARCHAR(500) | | Description |
| Slug | VARCHAR(200) | UNIQUE | URL slug |
| ParentCategoryId | INT | FK | Parent category |
| IconName | VARCHAR(100) | | Icon identifier |
| Color | VARCHAR(20) | | Display color |
| DisplayOrder | INT | DEFAULT 0 | Sort order |
| IsActive | BIT | DEFAULT 1 | Active flag |
| ArticleCount | INT | DEFAULT 0 | Denormalized count |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ServiceRequestArticles

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| ServiceRequestId | INT | FK, NOT NULL | Service request |
| KnowledgeArticleId | INT | FK, NOT NULL | Article reference |
| LinkedByUserId | INT | FK | Linked by user |
| LinkedAt | DATETIME | NOT NULL | Link timestamp |
| WasHelpful | BIT | | Article was helpful |
| DeflectedCase | BIT | DEFAULT 0 | Deflected ticket |
| Notes | VARCHAR(500) | | Link notes |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

#### ArticleFeedbacks

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | INT | PK, AUTO_INCREMENT | Primary key |
| KnowledgeArticleId | INT | FK, NOT NULL | Article reference |
| UserId | INT | FK | Feedback user |
| IsHelpful | BIT | | Was helpful |
| Rating | INT | | 1-5 rating |
| Comment | VARCHAR(2000) | | Feedback text |
| IpAddress | VARCHAR(50) | | User IP |
| UserAgent | VARCHAR(500) | | Browser info |
| SessionId | VARCHAR(100) | | Session ID |
| IsAnonymous | BIT | DEFAULT 0 | Anonymous feedback |
| CreatedAt | DATETIME | NOT NULL | Created timestamp |
| UpdatedAt | DATETIME | | Updated timestamp |
| IsDeleted | BIT | DEFAULT 0 | Soft delete flag |

### 4.2 Indexes

| Index | Table | Columns | Type |
|-------|-------|---------|------|
| IX_KnowledgeArticles_Slug | KnowledgeArticles | Slug | UNIQUE |
| IX_KnowledgeArticles_Status | KnowledgeArticles | Status | INDEX |
| IX_KnowledgeArticles_CategoryId | KnowledgeArticles | CategoryId | INDEX |
| IX_KnowledgeArticles_PublishedAt | KnowledgeArticles | PublishedAt | INDEX |
| IX_KnowledgeArticles_ViewCount | KnowledgeArticles | ViewCount DESC | INDEX |
| IX_KnowledgeArticles_LanguageCode | KnowledgeArticles | LanguageCode | INDEX |
| IX_KnowledgeArticles_ParentId | KnowledgeArticles | ParentArticleId | INDEX |
| IX_KnowledgeCategories_Slug | KnowledgeCategories | Slug | UNIQUE |
| IX_KnowledgeCategories_ParentId | KnowledgeCategories | ParentCategoryId | INDEX |
| IX_ServiceRequestArticles_RequestId | ServiceRequestArticles | ServiceRequestId | INDEX |
| IX_ServiceRequestArticles_ArticleId | ServiceRequestArticles | KnowledgeArticleId | INDEX |
| IX_ArticleFeedbacks_ArticleId | ArticleFeedbacks | KnowledgeArticleId | INDEX |
| FULLTEXT_KnowledgeArticles_Search | KnowledgeArticles | Title, Content, Keywords | FULLTEXT |

---

## 5. Tests

### 5.1 Unit Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| KnowledgeManagementServiceTests | CreateArticle_ValidData_Success | Create article | ⚠️ Partial |
| KnowledgeManagementServiceTests | PublishArticle_DraftArticle_Success | Publish workflow | ⚠️ Partial |
| KnowledgeManagementServiceTests | SubmitFeedback_ValidRating_Success | Feedback submission | ⚠️ Partial |
| KnowledgeManagementServiceTests | SearchArticles_ReturnsMatches | Search functionality | ⚠️ Partial |
| KnowledgeManagementServiceTests | GetSuggestedArticles_ReturnsRelevant | AI suggestions | ❌ Not Found |

### 5.2 Integration Tests

| Test Class | Method | Description | Status |
|------------|--------|-------------|--------|
| KnowledgeArticlesControllerTests | GetArticles_ReturnsList | List articles | ❌ Not Found |
| KnowledgeArticlesControllerTests | CreateArticle_Returns201 | Create endpoint | ❌ Not Found |
| KnowledgeArticlesControllerTests | PublishArticle_ChangesStatus | Publish endpoint | ❌ Not Found |

### 5.3 E2E Tests

| Test File | Test | Description | Status |
|-----------|------|-------------|--------|
| knowledge-base.spec.ts | Search articles | Search functionality | ❌ Not Found |
| knowledge-base.spec.ts | Rate article | Feedback submission | ❌ Not Found |
| knowledge-base.spec.ts | Create and publish | Publishing workflow | ❌ Not Found |

---

## 6. Issues & Inconsistencies

| ID | Issue | Severity | Description |
|----|-------|----------|-------------|
| SD002-ISS01 | Frontend components incomplete | Medium | Many UI components not implemented |
| SD002-ISS02 | Full-text search implementation | Medium | MySQL FULLTEXT needs configuration |
| SD002-ISS03 | AI embedding generation | Low | Vector storage needs completion |
| SD002-ISS04 | Version history not exposed | Low | API endpoint needed |
| SD002-ISS05 | Analytics endpoint partial | Low | Need full metrics |

---

## 7. TODO Items

| ID | Description | Priority | Category |
|----|-------------|----------|----------|
| TODO-SD002-001 | Create CategoryTree component | P2 | Frontend |
| TODO-SD002-002 | Create ArticleFeedbackWidget component | P2 | Frontend |
| TODO-SD002-003 | Create RelatedArticles component | P2 | Frontend |
| TODO-SD002-004 | Create PopularArticles component | P2 | Frontend |
| TODO-SD002-005 | Create ArticleMetrics component | P2 | Frontend |
| TODO-SD002-006 | Create VersionHistory component | P3 | Frontend |
| TODO-SD002-007 | Create PublishWorkflow component | P2 | Frontend |
| TODO-SD002-008 | Implement AI embedding generation | P2 | Backend |
| TODO-SD002-009 | Implement semantic search | P2 | Backend |
| TODO-SD002-010 | Add version history API endpoint | P3 | Backend |
| TODO-SD002-011 | Create E2E tests for knowledge base | P2 | Testing |
| TODO-SD002-012 | Add full-text search index configuration | P1 | Database |

---

## 8. Change History

| Date | Version | Author | Changes |
|------|---------|--------|---------|
| 2026-02-12 | 1.0 | System | Initial specification |

---

**END OF SPECIFICATION**
