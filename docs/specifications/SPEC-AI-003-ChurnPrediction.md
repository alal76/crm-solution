# SPEC-AI-003: Customer Churn Prediction & Retention Recommendations

> **Spec ID:** SPEC-AI-003  
> **Feature:** Customer Churn Prediction with AI-Powered Retention Recommendations  
> **Module:** AI & Analytics  
> **Version:** 1.0  
> **Last Updated:** February 14, 2026  
> **Status:** ❌ Not Implemented

---

## 1. Business Context

### 1.1 Feature Description

SPEC-AI-003 implements AI-powered churn risk prediction that identifies at-risk customers and recommends personalized retention actions. The system combines historical customer data, engagement metrics, financial performance, and contract terms to score churn risk in real-time, enabling proactive retention interventions.

**Key Capabilities:**
- Real-time churn risk scoring (0-100 scale)
- Root cause analysis identifying specific churn factors
- Segmented risk profiles (High, Medium, Low, Churned)
- Recommended retention interventions by risk tier
- Intervention ROI tracking and effectiveness metrics
- Predictive features cache for performance optimization

### 1.2 Sub-Features

| ID | Sub-Feature | Description | Status |
|----|-------------|-------------|--------|
| SF-001 | Churn Risk Scoring Engine | ML-based score calculation using 15+ engagement factors | ❌ |
| SF-002 | Risk Factor Analysis | Breakdown of top contributing factors to churn risk | ❌ |
| SF-003 | Customer Risk Dashboard | Visual representation of churn distribution and trends | ❌ |
| SF-004 | At-Risk Customers List | Filterable/sortable list of high-risk customers | ❌ |
| SF-005 | Retention Recommendations | AI-generated action recommendations by risk segment | ❌ |
| SF-006 | Intervention Tracking | Record and measure effectiveness of retention actions | ❌ |
| SF-007 | Engagement Metrics | LTV, ARR, engagement velocity, NPS trend calculation | ❌ |
| SF-008 | Risk Segmentation | Automatic grouping by risk level with cohort analysis | ❌ |
| SF-009 | Predictive Features Cache | Fast ML feature lookup for real-time scoring | ❌ |
| SF-010 | Historical Cohort Analysis | Churn rate trends by cohort, product, segment | ❌ |

### 1.3 Use Cases

| UC-ID | Use Case | Actor | Precondition | Postcondition | Status |
|-------|----------|-------|--------------|---------------|--------|
| UC-001 | View customer churn risk | CSM / Sales Manager | Customer exists with 6+ months data | Risk score displayed on customer profile | ❌ |
| UC-002 | Identify at-risk customers | Director of Retention | 1000+ active customers | Dashboard shows top 50 at-risk customers sorted by risk | ❌ |
| UC-003 | Drill into churn factors | CSM | Customer selected from at-risk list | Factor breakdown (engagement, NPS, ARR trend) displayed | ❌ |
| UC-004 | Execute retention action | CSM | At-risk customer identified | Action logged, effectiveness tracked over 90 days | ❌ |
| UC-005 | Measure intervention ROI | CFO / Revenue Ops | 100+ interventions completed | Dashboard shows retained customers vs intervention cost | ❌ |
| UC-006 | Analyze churn cohorts | Analytics / Finance | 6+ months historical data | Cohort analysis showing churn rates by product/segment | ❌ |
| UC-007 | Set churn alerts | Admin | Threshold configured | Alert sent when customer score exceeds threshold | ❌ |
| UC-008 | Forecast MRR impact | Finance | Churn predictions calculated | Dashboard estimates MRR impact of predicted churn | ❌ |

---

## 2. Frontend Implementation

### 2.1 Pages

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ChurnDashboardPage | `CRM.Frontend/src/pages/ai/ChurnDashboardPage.tsx` | ❌ | Main churn analytics dashboard |
| AtRiskCustomersPage | `CRM.Frontend/src/pages/ai/AtRiskCustomersPage.tsx` | ❌ | List of at-risk customers with filters |
| CustomerChurnDetailPage | `CRM.Frontend/src/pages/ai/CustomerChurnDetailPage.tsx` | ❌ | Deep dive into single customer risk |
| RetentionInterventionsPage | `CRM.Frontend/src/pages/ai/RetentionInterventionsPage.tsx` | ❌ | Log and track retention actions |
| ChurnMetricsPage | `CRM.Frontend/src/pages/ai/ChurnMetricsPage.tsx` | ❌ | Historical churn trends and cohort analysis |

### 2.2 Components

| Component | File Path | Status | Notes |
|-----------|-----------|--------|-------|
| ChurnRiskCard | `CRM.Frontend/src/components/ai/ChurnRiskCard.tsx` | ❌ | Risk score display with color coding |
| RiskFactorBreakdown | `CRM.Frontend/src/components/ai/RiskFactorBreakdown.tsx` | ❌ | Top 5-10 factors contributing to risk |
| RetentionRecommendations | `CRM.Frontend/src/components/ai/RetentionRecommendations.tsx` | ❌ | AI-suggested actions by risk segment |
| InterventionHistory | `CRM.Frontend/src/components/ai/InterventionHistory.tsx` | ❌ | Timeline of past retention attempts |
| AtRiskCustomersTable | `CRM.Frontend/src/components/ai/AtRiskCustomersTable.tsx` | ❌ | Sortable/filterable list with inline actions |
| ChurnTrendChart | `CRM.Frontend/src/components/ai/ChurnTrendChart.tsx` | ❌ | Line chart of churn rate over time |
| RiskDistributionChart | `CRM.Frontend/src/components/ai/RiskDistributionChart.tsx` | ❌ | Distribution of customers by risk tier |
| CohortAnalysisTable | `CRM.Frontend/src/components/ai/CohortAnalysisTable.tsx` | ❌ | Cohort churn rates by product/segment |
| InterventionROIWidget | `CRM.Frontend/src/components/ai/InterventionROIWidget.tsx` | ❌ | Summary of intervention ROI metrics |
| RiskSegmentFilter | `CRM.Frontend/src/components/ai/RiskSegmentFilter.tsx` | ❌ | Filter UI for risk tiers |

### 2.3 Services (API Client)

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| churnService | `CRM.Frontend/src/services/churnService.ts` | getChurnScores, getAtRiskCustomers, getChurnFactors, recordIntervention, getInterventionHistory, getChurnMetrics, getRiskDistribution | ❌ |
| aiAgentService | `CRM.Frontend/src/services/aiAgentService.ts` | invokeSKAgent (existing) + churnAgent | ⚠️ |

### 2.4 Frontend Validations

| Field | Validation Rule | Type | Implementation Status |
|-------|-----------------|------|----------------------|
| Intervention.Action | Required, max 255 chars | Frontend/Backend | ❌ |
| Intervention.ExpectedOutcome | Max 500 chars | Frontend | ❌ |
| Intervention.Notes | Max 1000 chars | Frontend | ❌ |
| RiskThreshold | 0-100 number | Frontend | ❌ |

---

## 3. Backend Implementation

### 3.1 Entities

| Entity | File Path | Status | Notes |
|--------|-----------|--------|-------|
| ChurnRiskScore | `CRM.Core/Entities/ChurnRiskScore.cs` | ❌ | Historical scores with model version |
| ChurnFactor | `CRM.Core/Entities/ChurnFactor.cs` | ❌ | Factor contributions to risk score |
| RetentionIntervention | `CRM.Core/Entities/RetentionIntervention.cs` | ❌ | Logged retention actions |
| FeatureCache | `CRM.Core/Entities/FeatureCache.cs` | ❌ | Cached ML features for performance |
| ChurnRiskThreshold | `CRM.Core/Entities/ChurnRiskThreshold.cs` | ❌ | Configurable alert thresholds |

### 3.2 DTOs

| DTO | File Path | Status | Notes |
|-----|-----------|--------|-------|
| ChurnRiskScoreDto | `CRM.Core/DTOs/ChurnRiskScoreDto.cs` | ❌ | Score, risk tier, timestamp |
| ChurnFactorDto | `CRM.Core/DTOs/ChurnFactorDto.cs` | ❌ | Factor name, weight, contribution |
| RiskFactorBreakdownDto | `CRM.Core/DTOs/RiskFactorBreakdownDto.cs` | ❌ | Top factors for customer |
| RetentionInterventionDto | `CRM.Core/DTOs/RetentionInterventionDto.cs` | ❌ | Action, outcome, ROI |
| RetentionRecommendationDto | `CRM.Core/DTOs/RetentionRecommendationDto.cs` | ❌ | Action type, priority, estimated impact |
| ChurnMetricsDto | `CRM.Core/DTOs/ChurnMetricsDto.cs` | ❌ | Historical rates, trends, forecasts |
| ChurnCohortDto | `CRM.Core/DTOs/ChurnCohortDto.cs` | ❌ | Cohort analysis with churn rate |
| AtRiskCustomerDto | `CRM.Core/DTOs/AtRiskCustomerDto.cs` | ❌ | Customer + risk + top factors |

### 3.3 Interfaces

| Interface | File Path | Methods | Status |
|-----------|-----------|---------|--------|
| IChurnPredictionService | `CRM.Core/Interfaces/IChurnPredictionService.cs` | 15+ | ❌ |
| IFeatureEngineer | `CRM.Core/Interfaces/IFeatureEngineer.cs` | 8+ | ❌ |
| IRiskScorer | `CRM.Core/Interfaces/IRiskScorer.cs` | 5+ | ❌ |
| IRetentionRecommender | `CRM.Core/Interfaces/IRetentionRecommender.cs` | 4+ | ❌ |

#### 3.3.1 IChurnPredictionService

```csharp
public interface IChurnPredictionService
{
    // Scoring
    Task<ChurnRiskScoreDto> GetChurnScoreAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnRiskScoreDto>> GetAllChurnScoresAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnRiskScoreDto>> GetAtRiskCustomersAsync(decimal riskThreshold = 60, int topN = 50, CancellationToken cancellationToken = default);
    Task<ChurnRiskScoreDto> CalculateChurnScoreAsync(int customerId, CancellationToken cancellationToken = default);
    
    // Factor Analysis
    Task<RiskFactorBreakdownDto> GetRiskFactorsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnFactorDto>> GetTopFactorsAsync(int customerId, int topN = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnFactorDto>> GetFactorImportanceAsync(CancellationToken cancellationToken = default);
    
    // Risk Segmentation
    Task<ChurnRiskDistributionDto> GetRiskDistributionAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnSegmentDto>> GetRiskSegmentsAsync(CancellationToken cancellationToken = default);
    
    // Recommendations
    Task<IEnumerable<RetentionRecommendationDto>> GetRecommendationsAsync(int customerId, CancellationToken cancellationToken = default);
    
    // Interventions
    Task<RetentionInterventionDto> LogInterventionAsync(RetentionInterventionDto intervention, CancellationToken cancellationToken = default);
    Task<IEnumerable<RetentionInterventionDto>> GetInterventionHistoryAsync(int customerId, CancellationToken cancellationToken = default);
    Task<RetentionInterventionDto> UpdateInterventionOutcomeAsync(int interventionId, string outcome, bool succeeded, CancellationToken cancellationToken = default);
    
    // Metrics
    Task<ChurnMetricsDto> GetChurnMetricsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ChurnCohortDto>> GetCohortAnalysisAsync(string groupBy, CancellationToken cancellationToken = default);
}
```

#### 3.3.2 IFeatureEngineer

```csharp
public interface IFeatureEngineer
{
    Task<CustomerMLFeatures> ExtractFeaturesAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateLTVAsync(int customerId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateARRAsync(int customerId, CancellationToken cancellationToken = default);
    Task<EngagementMetrics> CalculateEngagementMetricsAsync(int customerId, CancellationToken cancellationToken = default);
    Task<ContractHealthMetrics> AnalyzeContractHealthAsync(int customerId, CancellationToken cancellationToken = default);
    Task<NLSMetrics> CalculateNLSAsync(int customerId, CancellationToken cancellationToken = default);
    Task<bool> RefreshFeatureCacheAsync(int customerId, CancellationToken cancellationToken = default);
    Task<int> RefreshAllFeatureCachesAsync(CancellationToken cancellationToken = default);
}

public class CustomerMLFeatures
{
    public decimal LTV { get; set; }
    public decimal ARR { get; set; }
    public decimal MRR { get; set; }
    public double AverageDealSize { get; set; }
    public int MonthsSinceSignup { get; set; }
    public int MonthsSinceLastInteraction { get; set; }
    public double EngagementScore { get; set; }
    public int ActiveProductCount { get; set; }
    public double NLSScore { get; set; }
    public int TicketsRaised { get; set; }
    public int TicketsResolved { get; set; }
    public double NpsScore { get; set; }
    public int ContractMonthsRemaining { get; set; }
    public double ArrTrend { get; set; }  // % change last 90 days
    public bool IsAtRiskBasedOnPriors { get; set; }
}
```

#### 3.3.3 IRiskScorer

```csharp
public interface IRiskScorer
{
    Task<(int score, string riskTier)> ScoreCustomerAsync(CustomerMLFeatures features, CancellationToken cancellationToken = default);
    Task<IEnumerable<(string factor, double weight, double contribution)>> ExplainScoreAsync(int customerId, CancellationToken cancellationToken = default);
    Task<double> GetModelAccuracyAsync(CancellationToken cancellationToken = default);
    Task<ModelPerformanceMetrics> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default);
}

public class ModelPerformanceMetrics
{
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Auc { get; set; }
    public int TruePositives { get; set; }
    public int FalsePositives { get; set; }
    public int TrueNegatives { get; set; }
    public int FalseNegatives { get; set; }
}
```

#### 3.3.4 IRetentionRecommender

```csharp
public interface IRetentionRecommender
{
    Task<IEnumerable<RetentionRecommendationDto>> GetRecommendationsAsync(int customerId, int riskScore, CancellationToken cancellationToken = default);
    Task<RetentionRecommendationDto> GetTopRecommendationAsync(int customerId, CancellationToken cancellationToken = default);
    Task<RecommendationEffectivenessDto> TrackRecommendationEffectivenessAsync(CancellationToken cancellationToken = default);
}

public class RetentionRecommendationDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string ActionType { get; set; }  // "Discount", "Check-in", "ProductTraining", "EscalateToExec"
    public string Description { get; set; }
    public string Priority { get; set; }  // "High", "Medium", "Low"
    public decimal EstimatedSuccessProbability { get; set; }
    public decimal EstimatedArrRetained { get; set; }
    public int DaysToImplement { get; set; }
}
```

### 3.4 Services

| Service | File Path | Methods | Status |
|---------|-----------|---------|--------|
| ChurnPredictionService | `CRM.Infrastructure/Services/ChurnPredictionService.cs` | 15+ | ❌ |
| FeatureEngineer | `CRM.Infrastructure/Services/FeatureEngineer.cs` | 8+ | ❌ |
| RiskScorer | `CRM.Infrastructure/Services/RiskScorer.cs` | 5+ | ❌ |
| RetentionRecommender | `CRM.Infrastructure/Services/RetentionRecommender.cs` | 4+ | ❌ |

**Key Implementation Notes:**
- FeatureEngineer: Calculates features in-memory or caches to FeatureCache table for ML model
- RiskScorer: Uses Semantic Kernel agent for explainability or traditional ML (sklearn-exported model)
- ChurnPredictionService: Orchestrates the pipeline
- RetentionRecommender: Uses Semantic Kernel agents for personalized recommendations

### 3.5 Controllers

| Controller | File Path | Endpoints | Status |
|------------|-----------|-----------|--------|
| ChurnPredictionController | `CRM.Api/Controllers/ChurnPredictionController.cs` | 12 | ❌ |

### 3.6 API Endpoints

| Method | Endpoint | Controller Method | Auth | Status |
|--------|----------|-------------------|------|--------|
| GET | `/api/ai/churn/scores` | GetAllChurnScores | Yes | ❌ |
| GET | `/api/ai/churn/scores/{customerId}` | GetChurnScore | Yes | ❌ |
| GET | `/api/ai/churn/at-risk` | GetAtRiskCustomers | Yes | ❌ |
| GET | `/api/ai/churn/factors/{customerId}` | GetRiskFactors | Yes | ❌ |
| GET | `/api/ai/churn/recommendations/{customerId}` | GetRecommendations | Yes | ❌ |
| POST | `/api/ai/churn/interventions` | LogIntervention | Yes | ❌ |
| GET | `/api/ai/churn/interventions/{customerId}` | GetInterventionHistory | Yes | ❌ |
| PUT | `/api/ai/churn/interventions/{interventionId}` | UpdateIntervention | Yes | ❌ |
| GET | `/api/ai/churn/metrics` | GetChurnMetrics | Yes | ❌ |
| GET | `/api/ai/churn/cohorts` | GetCohortAnalysis | Yes | ❌ |
| GET | `/api/ai/churn/distribution` | GetRiskDistribution | Yes | ❌ |
| POST | `/api/ai/churn/calculate` | CalculateChurnScore (batch) | Yes | ❌ |

### 3.7 Backend Validations

| Field | Validation Rule | Location | Status |
|-------|-----------------|----------|--------|
| ChurnRiskScore (0-100) | Required, between 0 and 100 | Service/DTO | ❌ |
| RiskTier | Required, one of: "Critical", "High", "Medium", "Low", "Churned" | Service | ❌ |
| InterventionAction | Required, max 255 chars | DTO/Service | ❌ |
| InterventionOutcome | Max 500 chars | DTO | ❌ |
| CustomerId | Must exist in Customers table | Service | ❌ |
| RiskFactorWeight | 0-1 decimal range | Service | ❌ |

### 3.8 Semantic Kernel Integration

**ChurnPredictionAgent** (`CRM.Infrastructure/AI/SK/Agents/ChurnPredictionAgent.cs`)
- Analyzes customer data to identify churn risk
- Method: `[KernelFunction] AnalyzeChurnRiskAsync(int customerId)`
- Returns: Structured risk assessment with factors and recommendations
- Triggered by: Dashboard load, scheduled nightly batch job
- Model: Claude 3 Sonnet (via AIPort)

**Key Agent Plugins:**
- AccountPlugin: GetAccountAsync (customer context)
- SubscriptionPlugin: GetActiveSubscriptionsAsync (billing data)
- ContractPlugin: GetContractAsync (contract health)
- EngagementPlugin: GetEngagementMetricsAsync (NPS, tickets, interactions)

---

## 4. Database Implementation

### 4.1 Tables

| Table Name | File Path | Status | Notes |
|------------|-----------|--------|-------|
| ChurnRiskScores | `database/schema/ai/001_churn_tables.sql` | ❌ | Historical scores with versioning |
| ChurnFactors | `database/schema/ai/001_churn_tables.sql` | ❌ | Factor contributions to score |
| RetentionInterventions | `database/schema/ai/001_churn_tables.sql` | ❌ | Logged retention actions |
| FeatureCache | `database/schema/ai/001_churn_tables.sql` | ❌ | Cached ML features |
| ChurnRiskThresholds | `database/schema/ai/001_churn_tables.sql` | ❌ | Alert thresholds |

### 4.2 Data Elements

#### ChurnRiskScores Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| CustomerId | INT | No | - | FK(Customers) | CustomerId | ❌ |
| Score | DECIMAL(5,2) | No | - | Range 0-100 | Score | ❌ |
| RiskTier | VARCHAR(20) | No | - | Check IN ('Critical','High','Medium','Low','Churned') | RiskTier | ❌ |
| ModelVersion | VARCHAR(20) | No | - | - | ModelVersion | ❌ |
| CalculatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CalculatedAt | ❌ |
| Confidence | DECIMAL(5,2) | Yes | NULL | 0-100 | Confidence | ❌ |
| ExpiresAt | DATETIME | No | - | - | ExpiresAt | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

**Indexes:**
- `IX_ChurnRiskScores_CustomerId_CalculatedAt` (Query by customer)
- `IX_ChurnRiskScores_Score` (Filter by risk tier)
- `IX_ChurnRiskScores_ExpiresAt` (Cache expiry)

#### ChurnFactors Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| ChurnRiskScoreId | INT | No | - | FK(ChurnRiskScores) | ChurnRiskScoreId | ❌ |
| FactorName | VARCHAR(100) | No | - | - | FactorName | ❌ |
| FactorType | VARCHAR(50) | No | - | Check IN ('Engagement','Financial','Contract','NPS','Product') | FactorType | ❌ |
| Weight | DECIMAL(5,3) | No | - | Range 0-1 | Weight | ❌ |
| Contribution | DECIMAL(8,2) | No | - | Points toward score | Contribution | ❌ |
| Value | VARCHAR(255) | Yes | NULL | - | Value | ❌ |
| Trend | VARCHAR(20) | Yes | NULL | 'Improving','Declining','Stable' | Trend | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

#### RetentionInterventions Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| CustomerId | INT | No | - | FK(Customers) | CustomerId | ❌ |
| ChurnRiskScoreId | INT | Yes | NULL | FK(ChurnRiskScores) | ChurnRiskScoreId | ❌ |
| ActionType | VARCHAR(100) | No | - | 'Discount','Check-in','Training','Escalate','ProductReview','Pricing' | ActionType | ❌ |
| Description | TEXT | No | - | - | Description | ❌ |
| Priority | VARCHAR(20) | No | 'Medium' | 'Critical','High','Medium','Low' | Priority | ❌ |
| ImplementedBy | INT | No | - | FK(Users) | ImplementedByUserId | ❌ |
| ImplementedAt | DATETIME | No | CURRENT_TIMESTAMP | - | ImplementedAt | ❌ |
| TargetOutcome | VARCHAR(255) | Yes | NULL | - | TargetOutcome | ❌ |
| ActualOutcome | TEXT | Yes | NULL | - | ActualOutcome | ❌ |
| Succeeded | BOOLEAN | Yes | NULL | - | Succeeded | ❌ |
| OutcomeRecordedAt | DATETIME | Yes | NULL | - | OutcomeRecordedAt | ❌ |
| ArrRetained | DECIMAL(15,2) | Yes | NULL | Estimated ARR retained | ArrRetained | ❌ |
| Cost | DECIMAL(10,2) | Yes | NULL | Cost of intervention | Cost | ❌ |
| Notes | TEXT | Yes | NULL | - | Notes | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ❌ |
| IsDeleted | BOOLEAN | No | FALSE | - | IsDeleted | ❌ |

**Indexes:**
- `IX_RetentionInterventions_CustomerId_ImplementedAt` (Customer history)
- `IX_RetentionInterventions_Succeeded` (Outcome tracking)
- `IX_RetentionInterventions_ImplementedBy` (Agent performance)

#### FeatureCache Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| CustomerId | INT | No | - | FK(Customers), UNIQUE | CustomerId | ❌ |
| LTV | DECIMAL(15,2) | No | - | - | LTV | ❌ |
| ARR | DECIMAL(15,2) | No | - | - | ARR | ❌ |
| MRR | DECIMAL(15,2) | No | - | - | MRR | ❌ |
| AverageDealSize | DECIMAL(15,2) | No | - | - | AverageDealSize | ❌ |
| MonthsSinceSignup | INT | No | - | - | MonthsSinceSignup | ❌ |
| MonthsSinceLastInteraction | INT | No | - | - | MonthsSinceLastInteraction | ❌ |
| EngagementScore | DECIMAL(5,2) | No | - | 0-100 | EngagementScore | ❌ |
| ActiveProductCount | INT | No | - | - | ActiveProductCount | ❌ |
| NLSScore | DECIMAL(5,2) | No | - | Net Loss Score | NLSScore | ❌ |
| TicketsRaisedLast90Days | INT | No | - | - | TicketsRaisedLast90Days | ❌ |
| TicketsResolvedLast90Days | INT | No | - | - | TicketsResolvedLast90Days | ❌ |
| NpsScore | DECIMAL(5,2) | Yes | NULL | - | NpsScore | ❌ |
| ContractMonthsRemaining | INT | No | - | - | ContractMonthsRemaining | ❌ |
| ArrTrendPercent | DECIMAL(8,2) | No | - | % change last 90 days | ArrTrendPercent | ❌ |
| IsAtRiskBasedOnPriors | BOOLEAN | No | FALSE | - | IsAtRiskBasedOnPriors | ❌ |
| CachedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CachedAt | ❌ |
| ExpiresAt | DATETIME | No | - | Usually CachedAt + 7 days | ExpiresAt | ❌ |
| IsValid | BOOLEAN | No | TRUE | - | IsValid | ❌ |

**Indexes:**
- `IX_FeatureCache_CustomerId` (Primary lookup)
- `IX_FeatureCache_ExpiresAt` (Refresh scheduling)

#### ChurnRiskThresholds Table

| Column | Data Type | Nullable | Default | Constraints | Entity Property | Status |
|--------|-----------|----------|---------|-------------|-----------------|--------|
| Id | INT | No | AUTO_INCREMENT | PK | Id | ❌ |
| Threshold | DECIMAL(5,2) | No | 60 | 0-100 | Threshold | ❌ |
| RiskTier | VARCHAR(20) | No | - | 'Critical','High','Medium','Low' | RiskTier | ❌ |
| AlertEnabled | BOOLEAN | No | TRUE | - | AlertEnabled | ❌ |
| AlertEmail | VARCHAR(255) | Yes | NULL | - | AlertEmail | ❌ |
| CreatedAt | DATETIME | No | CURRENT_TIMESTAMP | - | CreatedAt | ❌ |
| UpdatedAt | DATETIME | Yes | NULL | - | UpdatedAt | ❌ |

### 4.3 Relationships

| From Table | To Table | Relationship | FK Column | Status |
|------------|----------|--------------|-----------|--------|
| ChurnRiskScores | Customers | N:1 | CustomerId | ❌ |
| ChurnFactors | ChurnRiskScores | N:1 | ChurnRiskScoreId | ❌ |
| RetentionInterventions | Customers | N:1 | CustomerId | ❌ |
| RetentionInterventions | ChurnRiskScores | N:1 | ChurnRiskScoreId (nullable) | ❌ |
| RetentionInterventions | Users | N:1 | ImplementedBy | ❌ |
| FeatureCache | Customers | 1:1 | CustomerId | ❌ |

### 4.4 Indexes

| Index Name | Table | Columns | Type | Status |
|------------|-------|---------|------|--------|
| IX_ChurnRiskScores_CustomerId_CalculatedAt | ChurnRiskScores | CustomerId, CalculatedAt | NonClustered | ❌ |
| IX_ChurnRiskScores_Score | ChurnRiskScores | Score | NonClustered | ❌ |
| IX_ChurnRiskScores_ExpiresAt | ChurnRiskScores | ExpiresAt | NonClustered | ❌ |
| IX_ChurnFactors_ChurnRiskScoreId | ChurnFactors | ChurnRiskScoreId | NonClustered | ❌ |
| IX_RetentionInterventions_CustomerId_ImplementedAt | RetentionInterventions | CustomerId, ImplementedAt | NonClustered | ❌ |
| IX_RetentionInterventions_Succeeded | RetentionInterventions | Succeeded | NonClustered | ❌ |
| IX_FeatureCache_CustomerId | FeatureCache | CustomerId | NonClustered | ❌ |
| IX_FeatureCache_ExpiresAt | FeatureCache | ExpiresAt | NonClustered | ❌ |

---

## 5. Test Coverage

### 5.1 Unit Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| FeatureEngineerTests | `CRM.Backend/tests/CRM.Tests/Services/FeatureEngineerTests.cs` | 25 | ❌ |
| RiskScorerTests | `CRM.Backend/tests/CRM.Tests/Services/RiskScorerTests.cs` | 18 | ❌ |
| ChurnPredictionServiceTests | `CRM.Backend/tests/CRM.Tests/Services/ChurnPredictionServiceTests.cs` | 22 | ❌ |
| RetentionRecommenderTests | `CRM.Backend/tests/CRM.Tests/Services/RetentionRecommenderTests.cs` | 15 | ❌ |

**Test Coverage Targets:**
- Feature engineering: LTV/ARR/NLS/engagement calculations, edge cases (new customers, churn scenarios)
- Risk scoring: Score ranges, factor weights, explainability
- Churn prediction: At-risk thresholds, segmentation logic, performance metrics
- Retention recommendations: Personalization, effectiveness tracking

### 5.2 Integration Tests

| Test Class | File Path | Tests | Status |
|------------|-----------|-------|--------|
| ChurnPredictionIntegrationTests | `CRM.Backend/tests/CRM.Tests/Integration/ChurnPredictionIntegrationTests.cs` | 12 | ❌ |

**Integration Test Scope:**
- End-to-end churn pipeline (calculate → store → recommend)
- Feature cache refresh and expiry
- Database consistency

### 5.3 E2E Tests

| Test Suite | File Path | Tests | Status |
|------------|-----------|-------|--------|
| churn-dashboard.spec.ts | `e2e-tests/tests/ai/churn-dashboard.spec.ts` | 8 | ❌ |
| at-risk-customers.spec.ts | `e2e-tests/tests/ai/at-risk-customers.spec.ts` | 6 | ❌ |
| churn-interventions.spec.ts | `e2e-tests/tests/ai/churn-interventions.spec.ts` | 5 | ❌ |

**E2E Test Scenarios:**
- View churn dashboard, verify risk distribution
- Filter at-risk customers, drill into factors
- Log intervention, track outcome
- Verify ROI calculations

---

## 6. Inconsistencies & Issues

### 6.1 Data Type Mismatches

| Location A | Location B | Issue | Resolution |
|------------|------------|-------|------------|
| ChurnRiskScore.Score (0-100) | RiskFactorBreakdownDto.ContributionPercent (0-100) | Both represent scores but units unclear | TODO-AI003-06: Clarify scale (0-100 is percentage) |
| CustomerMLFeatures.ArrTrend (%) | Database.ArrTrendPercent (decimal) | Representation inconsistency | TODO-AI003-07: Normalize to percentage with consistent precision |

### 6.2 Missing Implementations

| Item | Expected Location | Reason | TODO ID |
|------|-------------------|--------|---------|
| Model training pipeline | `CRM.Infrastructure/AI/ML/ChurnModelTrainer.cs` | Models must be trained on historical data, currently not scoped | TODO-AI003-01 |
| Class imbalance handling | RiskScorer | 5% actual churn vs 95% retained creates skewed predictions | TODO-AI003-08 |
| Real-time scoring performance | ChurnPredictionService | FeatureCache helps but scoring latency not specified | TODO-AI003-09 |
| Model versioning | ChurnRiskScore.ModelVersion | Scoring changes need tracking for audit | TODO-AI003-10 |
| Automated model retraining | Background job | Models degrade over time, need scheduled updates | TODO-AI003-11 |

### 6.3 Validation Gaps

| Field | Issue | Status |
|-------|-------|--------|
| ChurnRiskScore | Must be calculated for customer with 6+ months data | TODO-AI003-12 |
| RetentionIntervention.Outcome | Recording outcome should trigger risk recalculation | TODO-AI003-13 |
| FeatureCache.ExpiresAt | Cache invalidation strategy not defined | TODO-AI003-14 |
| ARR calculation | Depends on subscription/invoice data accuracy | TODO-AI003-15 |

---

## 7. TODO Items (→ Master TODO List)

| TODO ID | Description | Priority | Category |
|---------|-------------|----------|----------|
| TODO-AI003-01 | Implement ChurnModelTrainer service with Python/scikit-learn integration or ONNX model loading | P1 | Backend |
| TODO-AI003-02 | Create historical churn dataset from 6+ months of customer data for model training | P1 | Data |
| TODO-AI003-03 | Implement feature engineering: LTV, ARR, NLS, engagement score calculations | P1 | Backend |
| TODO-AI003-04 | Create RiskScorer service integrating trained ML model or Semantic Kernel agent | P1 | Backend |
| TODO-AI003-05 | Design and implement feature caching strategy for sub-second scoring latency | P1 | Backend |
| TODO-AI003-06 | Clarify score scale and units documentation (0-100 percentage vs other scales) | P2 | Documentation |
| TODO-AI003-07 | Normalize ARR trend representation across DTOs, entities, and database | P2 | Backend |
| TODO-AI003-08 | Implement class imbalance handling (SMOTE, weighted loss, or threshold tuning) | P2 | Backend |
| TODO-AI003-09 | Define and measure end-to-end scoring latency targets (target: <100ms per customer) | P2 | Performance |
| TODO-AI003-10 | Implement model versioning in ChurnRiskScore table with audit trail | P2 | Backend |
| TODO-AI003-11 | Create scheduled background job for automated model retraining (monthly or quarterly) | P2 | Infrastructure |
| TODO-AI003-12 | Add validation: ChurnRiskScore can only be calculated for customers with 6+ months history | P1 | Backend |
| TODO-AI003-13 | Implement trigger: Recording intervention outcome recalculates customer risk score | P2 | Backend |
| TODO-AI003-14 | Define FeatureCache expiry and refresh strategy (e.g., 7-day TTL, refresh on interval) | P2 | Backend |
| TODO-AI003-15 | Validate ARR calculation accuracy by reconciling with subscription/invoice data | P2 | Backend |
| TODO-AI003-16 | Implement ChurnPredictionService with all 15+ methods per IChurnPredictionService | P1 | Backend |
| TODO-AI003-17 | Implement FeatureEngineer with LTV, ARR, engagement metric calculations | P1 | Backend |
| TODO-AI003-18 | Implement RiskScorer with explainability (factor breakdown) | P1 | Backend |
| TODO-AI003-19 | Implement RetentionRecommender with Semantic Kernel agent integration | P1 | Backend |
| TODO-AI003-20 | Create ChurnPredictionController with 12 REST endpoints | P1 | Backend |
| TODO-AI003-21 | Implement database schema: ChurnRiskScores, ChurnFactors, RetentionInterventions, FeatureCache tables | P1 | Database |
| TODO-AI003-22 | Create frontend pages: ChurnDashboard, AtRiskCustomers, CustomerChurnDetail, RetentionInterventions, ChurnMetrics | P1 | Frontend |
| TODO-AI003-23 | Create frontend components: ChurnRiskCard, RiskFactorBreakdown, Recommendations, InterventionHistory, Charts | P1 | Frontend |
| TODO-AI003-24 | Implement churnService.ts with API client methods for all controller endpoints | P1 | Frontend |
| TODO-AI003-25 | Create Semantic Kernel ChurnPredictionAgent with explainability plugins | P2 | AI |
| TODO-AI003-26 | Integrate ChurnPredictionAgent with dashboard real-time updates | P2 | Integration |
| TODO-AI003-27 | Implement intervention ROI tracking: cost, ARR retained, success metrics | P2 | Backend |
| TODO-AI003-28 | Add churn risk widget to customer detail page | P2 | Frontend |
| TODO-AI003-29 | Create nightly batch job to calculate churn scores for all active customers | P2 | Infrastructure |
| TODO-AI003-30 | Set up alerting for customers exceeding churn risk threshold | P2 | Infrastructure |
| TODO-AI003-31 | Create documentation: ML model training, feature definitions, scoring logic | P3 | Documentation |
| TODO-AI003-32 | Create unit tests: FeatureEngineer (25), RiskScorer (18), ChurnPredictionService (22), RetentionRecommender (15) | P1 | Testing |
| TODO-AI003-33 | Create integration tests: End-to-end pipeline, cache refresh, database consistency (12 tests) | P1 | Testing |
| TODO-AI003-34 | Create E2E tests: Dashboard, at-risk list, interventions, ROI tracking (19 tests total) | P1 | Testing |
| TODO-AI003-35 | Implement cohort analysis for churn rates by product, segment, geography | P3 | Analytics |
| TODO-AI003-36 | Add MRR impact forecasting: predict MRR loss if predicted churns occur | P3 | Analytics |
| TODO-AI003-37 | Create model performance dashboard showing accuracy, precision, recall, AUC metrics | P3 | Analytics |
| TODO-AI003-38 | Implement intervention recommendation bias detection and fairness checks | P3 | ML |

---

## 8. Change History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2026-02-14 | AI Assistant | Initial specification for AI-powered churn prediction |

---

## Appendix: Model Performance Targets

**Target Model Metrics:**
- **Precision:** 75%+ (minimize false positives)
- **Recall:** 70%+ (catch most true churners)
- **F1-Score:** 72%+
- **AUC-ROC:** 80%+

**Scoring Latency:**
- Single customer: <100ms (with cache)
- Batch (1000 customers): <10s

**Availability:**
- Feature cache: 99%+ hit rate
- Score accuracy: Re-baseline quarterly

---

**END OF SPECIFICATION**
