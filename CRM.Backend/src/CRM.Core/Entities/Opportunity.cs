namespace CRM.Core.Entities;

#region Opportunity Enumerations

/// <summary>
/// FUNCTIONAL: Defines the sales pipeline stages an opportunity progresses through.
/// TECHNICAL: Aligned with standard sales methodologies (BANT, MEDDIC, Sandler, Solution Selling).
/// Each stage has an associated default probability for forecasting accuracy.
/// </summary>
public enum OpportunityStage
{
    /// <summary>Initial contact - identifying potential opportunities (5% probability)</summary>
    Prospecting = 0,
    
    /// <summary>Qualifying the opportunity using BANT criteria (10% probability)</summary>
    Qualification = 1,
    
    /// <summary>Deep dive into customer requirements and pain points (20% probability)</summary>
    NeedsAnalysis = 2,
    
    /// <summary>Presenting value proposition aligned to needs (35% probability)</summary>
    ValueProposition = 3,
    
    /// <summary>Mapping the decision-making unit and influences (50% probability)</summary>
    IdentifyDecisionMakers = 4,
    
    /// <summary>Understanding stakeholder perceptions and objections (60% probability)</summary>
    PerceptionAnalysis = 5,
    
    /// <summary>Formal proposal or quote submitted (70% probability)</summary>
    ProposalQuote = 6,
    
    /// <summary>Active negotiation on terms, pricing, contracts (80% probability)</summary>
    NegotiationReview = 7,
    
    /// <summary>Verbal agreement obtained, pending contract (90% probability)</summary>
    VerbalCommitment = 8,
    
    /// <summary>Contract sent for signature (95% probability)</summary>
    ContractSent = 9,
    
    /// <summary>Deal successfully closed (100% probability)</summary>
    ClosedWon = 10,
    
    /// <summary>Deal lost to competition or no decision</summary>
    ClosedLost = 11,
    
    /// <summary>Opportunity put on hold by customer</summary>
    OnHold = 12,
    
    /// <summary>Opportunity disqualified - not a fit</summary>
    Disqualified = 13
}

/// <summary>
/// FUNCTIONAL: Categorizes the business context and origin of the opportunity.
/// TECHNICAL: Used for revenue attribution, commission calculations, and sales analytics.
/// </summary>
public enum OpportunityType
{
    /// <summary>Brand new customer acquisition</summary>
    NewBusiness = 0,
    
    /// <summary>Additional purchase from existing customer</summary>
    ExistingBusiness = 1,
    
    /// <summary>Contract or subscription renewal</summary>
    Renewal = 2,
    
    /// <summary>Upgrading existing product/service tier</summary>
    Upsell = 3,
    
    /// <summary>Selling additional products to existing customer</summary>
    CrossSell = 4,
    
    /// <summary>Opportunity from customer/partner referral</summary>
    Referral = 5,
    
    /// <summary>Reactivating churned customer</summary>
    Winback = 6,
    
    /// <summary>Expansion to new business unit/department</summary>
    Expansion = 7,
    
    /// <summary>Migration from competitor solution</summary>
    CompetitiveReplacement = 8,
    
    /// <summary>Partner-influenced or co-sell opportunity</summary>
    PartnerOpportunity = 9,
    
    /// <summary>Government or public sector opportunity</summary>
    PublicSector = 10,
    
    /// <summary>Strategic or enterprise account opportunity</summary>
    StrategicAccount = 11
}

/// <summary>
/// FUNCTIONAL: Indicates urgency and resource allocation priority.
/// TECHNICAL: Drives SLA timelines, assignment rules, and escalation workflows.
/// </summary>
public enum OpportunityPriority
{
    /// <summary>Standard priority, normal SLAs apply</summary>
    Low = 0,
    
    /// <summary>Moderate priority, enhanced attention</summary>
    Medium = 1,
    
    /// <summary>High priority, expedited processing</summary>
    High = 2,
    
    /// <summary>Critical priority, C-level visibility</summary>
    Critical = 3,
    
    /// <summary>Strategic priority, board-level visibility</summary>
    Strategic = 4
}

/// <summary>
/// FUNCTIONAL: Categorizes opportunities for accurate revenue forecasting.
/// TECHNICAL: Aligns with standard CRM forecasting categories for pipeline analysis.
/// </summary>
public enum ForecastCategory
{
    /// <summary>Early stage, not included in forecast</summary>
    Pipeline = 0,
    
    /// <summary>Possible win, included in upside</summary>
    BestCase = 1,
    
    /// <summary>High confidence, included in commit</summary>
    Commit = 2,
    
    /// <summary>Deal closed and won</summary>
    Closed = 3,
    
    /// <summary>Excluded from forecast calculations</summary>
    Omitted = 4,
    
    /// <summary>At risk, needs attention</summary>
    AtRisk = 5,
    
    /// <summary>Stalled, no recent activity</summary>
    Stalled = 6
}

/// <summary>
/// FUNCTIONAL: Tracks budget approval status using BANT methodology.
/// TECHNICAL: Critical qualification criterion for opportunity scoring.
/// </summary>
public enum BudgetStatus
{
    /// <summary>Budget status unknown</summary>
    Unknown = 0,
    
    /// <summary>No budget allocated</summary>
    NotAllocated = 1,
    
    /// <summary>Budget pending approval</summary>
    Pending = 2,
    
    /// <summary>Budget approved and available</summary>
    Approved = 3,
    
    /// <summary>Budget confirmed with purchase order</summary>
    Confirmed = 4,
    
    /// <summary>Budget cut or reallocated</summary>
    Cut = 5,
    
    /// <summary>Budget deferred to future period</summary>
    Deferred = 6
}

/// <summary>
/// FUNCTIONAL: Indicates the competitive landscape for the opportunity.
/// TECHNICAL: Enables win/loss analysis and competitive intelligence tracking.
/// </summary>
public enum CompetitiveSituation
{
    /// <summary>No known competition</summary>
    NoCompetition = 0,
    
    /// <summary>Single competitor identified</summary>
    SingleCompetitor = 1,
    
    /// <summary>Multiple competitors involved</summary>
    MultipleCompetitors = 2,
    
    /// <summary>Formal RFP/tender process</summary>
    FormalBid = 3,
    
    /// <summary>Customer evaluating build vs buy</summary>
    BuildVsBuy = 4,
    
    /// <summary>Incumbent vendor has advantage</summary>
    IncumbentAdvantage = 5,
    
    /// <summary>We are the incumbent vendor</summary>
    WeAreIncumbent = 6,
    
    /// <summary>Sole source opportunity</summary>
    SoleSource = 7
}

/// <summary>
/// FUNCTIONAL: Categorizes the reason for lost opportunities.
/// TECHNICAL: Enables loss analysis, product feedback, and competitive intelligence.
/// </summary>
public enum LossReasonCategory
{
    /// <summary>Not specified</summary>
    NotSpecified = 0,
    
    /// <summary>Lost to competitor on features</summary>
    CompetitorFeatures = 1,
    
    /// <summary>Lost to competitor on price</summary>
    CompetitorPrice = 2,
    
    /// <summary>Lost to competitor on relationship</summary>
    CompetitorRelationship = 3,
    
    /// <summary>Customer decided not to purchase</summary>
    NoDecision = 4,
    
    /// <summary>Budget was cut or reallocated</summary>
    BudgetCut = 5,
    
    /// <summary>Project cancelled or postponed</summary>
    ProjectCancelled = 6,
    
    /// <summary>Decision maker left the organization</summary>
    ChampionLeft = 7,
    
    /// <summary>Product/service didn't meet requirements</summary>
    ProductFit = 8,
    
    /// <summary>Implementation concerns</summary>
    ImplementationConcerns = 9,
    
    /// <summary>Timing not right for customer</summary>
    Timing = 10,
    
    /// <summary>Regulatory or compliance issues</summary>
    Regulatory = 11,
    
    /// <summary>Internal company issues (ours)</summary>
    InternalIssues = 12,
    
    /// <summary>Customer went with in-house solution</summary>
    InHouseSolution = 13,
    
    /// <summary>Contract terms unacceptable</summary>
    ContractTerms = 14,
    
    /// <summary>Security or privacy concerns</summary>
    SecurityConcerns = 15
}

/// <summary>
/// FUNCTIONAL: Categorizes the reason for winning opportunities.
/// TECHNICAL: Enables win analysis and success pattern identification.
/// </summary>
public enum WinReasonCategory
{
    /// <summary>Not specified</summary>
    NotSpecified = 0,
    
    /// <summary>Superior product features</summary>
    ProductFeatures = 1,
    
    /// <summary>Competitive pricing</summary>
    Price = 2,
    
    /// <summary>Strong customer relationship</summary>
    Relationship = 3,
    
    /// <summary>Better customer support/service</summary>
    Support = 4,
    
    /// <summary>Successful proof of concept</summary>
    ProofOfConcept = 5,
    
    /// <summary>Executive sponsorship</summary>
    ExecutiveSponsor = 6,
    
    /// <summary>Integration capabilities</summary>
    Integration = 7,
    
    /// <summary>Ease of implementation</summary>
    EaseOfImplementation = 8,
    
    /// <summary>Industry expertise</summary>
    IndustryExpertise = 9,
    
    /// <summary>Reference customers</summary>
    References = 10,
    
    /// <summary>Brand/company reputation</summary>
    BrandReputation = 11,
    
    /// <summary>Compliance/security certifications</summary>
    Compliance = 12,
    
    /// <summary>Long-term partnership value</summary>
    PartnershipValue = 13
}

/// <summary>
/// FUNCTIONAL: Tracks the engagement level of the customer.
/// TECHNICAL: Used for opportunity health scoring and prioritization.
/// </summary>
public enum EngagementLevel
{
    /// <summary>Customer is unresponsive</summary>
    Unresponsive = 0,
    
    /// <summary>Minimal engagement</summary>
    Low = 1,
    
    /// <summary>Moderate engagement</summary>
    Medium = 2,
    
    /// <summary>Highly engaged</summary>
    High = 3,
    
    /// <summary>Champion actively driving the deal</summary>
    Champion = 4
}

/// <summary>
/// FUNCTIONAL: Indicates the contract type for the opportunity.
/// TECHNICAL: Affects revenue recognition and billing configuration.
/// </summary>
public enum ContractType
{
    /// <summary>One-time purchase</summary>
    OneTime = 0,
    
    /// <summary>Monthly subscription</summary>
    MonthlySubscription = 1,
    
    /// <summary>Annual subscription</summary>
    AnnualSubscription = 2,
    
    /// <summary>Multi-year contract</summary>
    MultiYear = 3,
    
    /// <summary>Usage-based pricing</summary>
    UsageBased = 4,
    
    /// <summary>Perpetual license</summary>
    PerpetualLicense = 5,
    
    /// <summary>Maintenance and support</summary>
    MaintenanceSupport = 6,
    
    /// <summary>Professional services</summary>
    ProfessionalServices = 7,
    
    /// <summary>Hybrid (subscription + services)</summary>
    Hybrid = 8
}

/// <summary>
/// FUNCTIONAL: Indicates the overall health/risk status of the opportunity.
/// TECHNICAL: Calculated based on multiple factors for pipeline risk assessment.
/// </summary>
public enum OpportunityHealth
{
    /// <summary>Deal is progressing well</summary>
    Healthy = 0,
    
    /// <summary>Some concerns, needs attention</summary>
    NeedsAttention = 1,
    
    /// <summary>At risk of loss</summary>
    AtRisk = 2,
    
    /// <summary>Critical issues, escalation needed</summary>
    Critical = 3
}

/// <summary>
/// FUNCTIONAL: Tracks MEDDIC qualification criteria completeness.
/// TECHNICAL: Each criterion is a flag indicating completion status.
/// </summary>
public enum MeddicCriteria
{
    /// <summary>No criteria met</summary>
    None = 0,
    
    /// <summary>Metrics identified and quantified</summary>
    Metrics = 1,
    
    /// <summary>Economic buyer identified</summary>
    EconomicBuyer = 2,
    
    /// <summary>Decision criteria understood</summary>
    DecisionCriteria = 4,
    
    /// <summary>Decision process mapped</summary>
    DecisionProcess = 8,
    
    /// <summary>Pain points identified</summary>
    IdentifyPain = 16,
    
    /// <summary>Champion engaged</summary>
    Champion = 32,
    
    /// <summary>All criteria met (63)</summary>
    Complete = 63
}

#endregion

/// <summary>
/// FUNCTIONAL VIEW:
/// ================
/// Represents a sales opportunity through its complete lifecycle from initial prospecting
/// to closed-won or closed-lost. Tracks all aspects of the sales process including:
/// - Financial metrics (deal size, recurring revenue, discounts)
/// - Sales stages and probability for forecasting
/// - Qualification criteria (BANT, MEDDIC)
/// - Competitive intelligence
/// - Decision-making unit mapping
/// - Activity and engagement tracking
/// - Risk assessment and health scoring
/// 
/// Key Business Processes:
/// - Pipeline management and forecasting
/// - Sales team performance tracking
/// - Win/loss analysis
/// - Revenue recognition planning
/// - Commission calculations
/// - Territory management
/// 
/// TECHNICAL VIEW:
/// ===============
/// Entity with comprehensive tracking for enterprise sales processes.
/// Supports multiple sales methodologies (BANT, MEDDIC, Challenger, Solution Selling).
/// 
/// Key Relationships:
/// - Belongs to a Customer (required)
/// - Optionally linked to Contact, Product, Quote, Campaign
/// - Assigned to User (sales rep)
/// - Tracks stage history in JSON format
/// 
/// Indexing Recommendations:
/// - CustomerId, AssignedToUserId (foreign keys)
/// - Stage, ForecastCategory (filtering)
/// - ExpectedCloseDate (date range queries)
/// - Amount, Probability (numeric filtering)
/// </summary>
public class Opportunity : BaseEntity
{
    #region Identification & Basic Information
    
    /// <summary>
    /// FUNCTIONAL: Display name for the opportunity (e.g., "Acme Corp - Enterprise License")
    /// TECHNICAL: Required field, used in search and display
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Unique opportunity identifier for external reference
    /// TECHNICAL: Auto-generated format: OPP-YYYYMM-XXXXX
    /// </summary>
    public string? OpportunityNumber { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External reference ID from integrated systems (e.g., ERP opportunity ID)
    /// TECHNICAL: Nullable string for system integration
    /// </summary>
    public string? ExternalOpportunityId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Detailed description of the opportunity and customer requirements
    /// TECHNICAL: Free-text field supporting rich text
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// FUNCTIONAL: Classification of the opportunity type for analytics
    /// TECHNICAL: Enum value mapped to commission rules
    /// </summary>
    public OpportunityType OpportunityType { get; set; } = OpportunityType.NewBusiness;
    
    /// <summary>
    /// FUNCTIONAL: Urgency level for resource allocation and SLAs
    /// TECHNICAL: Drives workflow escalation rules
    /// </summary>
    public OpportunityPriority Priority { get; set; } = OpportunityPriority.Medium;
    
    /// <summary>
    /// FUNCTIONAL: Overall health status of the opportunity
    /// TECHNICAL: Can be manually set or calculated from risk factors
    /// </summary>
    public OpportunityHealth Health { get; set; } = OpportunityHealth.Healthy;
    
    #endregion
    
    #region Financial Metrics
    
    /// <summary>
    /// FUNCTIONAL: Total deal value (one-time + recurring)
    /// TECHNICAL: Decimal precision 18,2 for currency accuracy
    /// </summary>
    public decimal Amount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Weighted revenue (Amount × Probability)
    /// TECHNICAL: Recalculated on stage/probability change
    /// </summary>
    public decimal ExpectedRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Monthly recurring revenue component
    /// TECHNICAL: Used for ARR/MRR calculations
    /// </summary>
    public decimal RecurringRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Annual recurring revenue (MRR × 12)
    /// TECHNICAL: Calculated field for subscription analytics
    /// </summary>
    public decimal AnnualRecurringRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: One-time fees (implementation, setup, etc.)
    /// TECHNICAL: Separated for revenue recognition purposes
    /// </summary>
    public decimal OneTimeRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Professional services component
    /// TECHNICAL: Separated for margin analysis
    /// </summary>
    public decimal ServicesRevenue { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Total discount amount applied
    /// TECHNICAL: Absolute value in currency
    /// </summary>
    public decimal Discount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Discount as percentage of list price
    /// TECHNICAL: 0-100 scale, triggers approval workflows if exceeded
    /// </summary>
    public decimal DiscountPercent { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Maximum discount allowed without approval
    /// TECHNICAL: Based on user's discount authority level
    /// </summary>
    public decimal MaxAllowedDiscount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Requires discount approval if threshold exceeded
    /// TECHNICAL: Triggers approval workflow
    /// </summary>
    public bool DiscountRequiresApproval { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Justification for applied discount
    /// TECHNICAL: Required if discount exceeds threshold
    /// </summary>
    public string? DiscountReason { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Currency for all monetary values
    /// TECHNICAL: ISO 4217 currency code (USD, EUR, GBP, etc.)
    /// </summary>
    public string? CurrencyCode { get; set; } = "USD";
    
    /// <summary>
    /// FUNCTIONAL: Exchange rate to base currency for consolidated reporting
    /// TECHNICAL: Applied at opportunity close for accurate conversion
    /// </summary>
    public decimal ExchangeRate { get; set; } = 1;
    
    /// <summary>
    /// FUNCTIONAL: Amount converted to base currency
    /// TECHNICAL: Calculated as Amount × ExchangeRate
    /// </summary>
    public decimal AmountInBaseCurrency { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Expected gross profit margin percentage
    /// TECHNICAL: Used for profitability analysis
    /// </summary>
    public decimal GrossMarginPercent { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Cost of goods/services for margin calculation
    /// TECHNICAL: Populated from product costs
    /// </summary>
    public decimal CostAmount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Type of contract/pricing model
    /// TECHNICAL: Affects revenue recognition schedule
    /// </summary>
    public ContractType ContractType { get; set; } = ContractType.OneTime;
    
    /// <summary>
    /// FUNCTIONAL: Contract duration in months
    /// TECHNICAL: Used for revenue spread calculations
    /// </summary>
    public int? ContractLengthMonths { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Total contract value over full term
    /// TECHNICAL: TCV = RecurringRevenue × ContractLength + OneTimeRevenue
    /// </summary>
    public decimal TotalContractValue { get; set; } = 0;
    
    #endregion
    
    #region Sales Stage & Pipeline
    
    /// <summary>
    /// FUNCTIONAL: Current stage in the sales pipeline
    /// TECHNICAL: Triggers stage-specific workflows and probability updates
    /// </summary>
    public OpportunityStage Stage { get; set; } = OpportunityStage.Prospecting;
    
    /// <summary>
    /// FUNCTIONAL: Previous stage before current transition
    /// TECHNICAL: Used for stage velocity analysis
    /// </summary>
    public OpportunityStage? PreviousStage { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Win probability percentage (0-100)
    /// TECHNICAL: Can be auto-set by stage or manually overridden
    /// </summary>
    public double Probability { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Indicates if probability was manually adjusted
    /// TECHNICAL: Overrides stage-based probability defaults
    /// </summary>
    public bool ProbabilityOverridden { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Category for revenue forecasting
    /// TECHNICAL: Determines inclusion in forecast reports
    /// </summary>
    public ForecastCategory ForecastCategory { get; set; } = ForecastCategory.Pipeline;
    
    /// <summary>
    /// FUNCTIONAL: Previous forecast category before change
    /// TECHNICAL: For forecast change tracking
    /// </summary>
    public ForecastCategory? PreviousForecastCategory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: AI-generated win probability score
    /// TECHNICAL: Machine learning prediction based on historical patterns
    /// </summary>
    public double? AiWinScore { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Factors contributing to AI score
    /// TECHNICAL: JSON array of scoring factors and weights
    /// </summary>
    public string? AiScoreFactors { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: History of all stage transitions
    /// TECHNICAL: JSON array of {stage, date, duration, userId}
    /// </summary>
    public string? StageHistory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Date when opportunity entered current stage
    /// TECHNICAL: Auto-set on stage change
    /// </summary>
    public DateTime? CurrentStageEnteredDate { get; set; }
    
    #endregion
    
    #region Dates & Timeline
    
    /// <summary>
    /// FUNCTIONAL: Target close date for the opportunity
    /// TECHNICAL: Primary date for forecasting and reporting
    /// </summary>
    public DateTime? CloseDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Original estimated close date (before any changes)
    /// TECHNICAL: For close date slippage analysis
    /// </summary>
    public DateTime? OriginalCloseDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Realistic expected close based on sales assessment
    /// TECHNICAL: May differ from CloseDate for internal planning
    /// </summary>
    public DateTime? ExpectedCloseDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Actual date when opportunity was closed
    /// TECHNICAL: Auto-set when stage changes to ClosedWon/ClosedLost
    /// </summary>
    public DateTime? ActualCloseDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Date of next scheduled action/task
    /// TECHNICAL: Drives reminder and follow-up workflows
    /// </summary>
    public DateTime? NextStepDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Most recent customer interaction date
    /// TECHNICAL: Auto-updated from activity tracking
    /// </summary>
    public DateTime? LastActivityDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Date of last customer contact
    /// TECHNICAL: Used for engagement health scoring
    /// </summary>
    public DateTime? LastContactDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of days in current stage
    /// TECHNICAL: Calculated field for stage velocity
    /// </summary>
    public int? DaysInCurrentStage { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Total days from creation to current/close
    /// TECHNICAL: Key metric for sales cycle analysis
    /// </summary>
    public int? TotalDaysOpen { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Expected average days based on similar deals
    /// TECHNICAL: Used for velocity comparison
    /// </summary>
    public int? ExpectedSalesCycleDays { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer's target start/go-live date
    /// TECHNICAL: Used for implementation planning
    /// </summary>
    public DateTime? CustomerTargetDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Contract start date if different from close
    /// TECHNICAL: Revenue recognition start date
    /// </summary>
    public DateTime? ContractStartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Contract end date for renewal tracking
    /// TECHNICAL: Triggers renewal opportunity creation
    /// </summary>
    public DateTime? ContractEndDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Decision deadline imposed by customer
    /// TECHNICAL: Influences close date accuracy
    /// </summary>
    public DateTime? DecisionDeadline { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of times close date was changed
    /// TECHNICAL: Indicator of forecast reliability
    /// </summary>
    public int CloseDatePushCount { get; set; } = 0;
    
    #endregion
    
    #region Sales Process & Methodology
    
    /// <summary>
    /// FUNCTIONAL: Description of next action to advance the deal
    /// TECHNICAL: Free-text field for sales playbook
    /// </summary>
    public string? NextStep { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Previous next step for tracking changes
    /// TECHNICAL: Supports sales activity analysis
    /// </summary>
    public string? PreviousNextStep { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Specific reason for loss (if ClosedLost)
    /// TECHNICAL: Detailed text supporting loss category
    /// </summary>
    public string? LossReason { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Categorized loss reason for analytics
    /// TECHNICAL: Enum for consistent loss analysis
    /// </summary>
    public LossReasonCategory LossReasonCategory { get; set; } = LossReasonCategory.NotSpecified;
    
    /// <summary>
    /// FUNCTIONAL: Specific reason for win (if ClosedWon)
    /// TECHNICAL: Detailed text supporting win category
    /// </summary>
    public string? WinReason { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Categorized win reason for analytics
    /// TECHNICAL: Enum for consistent win analysis
    /// </summary>
    public WinReasonCategory WinReasonCategory { get; set; } = WinReasonCategory.NotSpecified;
    
    /// <summary>
    /// FUNCTIONAL: Competitive situation description
    /// TECHNICAL: Enum for competitive analysis
    /// </summary>
    public CompetitiveSituation CompetitiveSituation { get; set; } = CompetitiveSituation.NoCompetition;
    
    /// <summary>
    /// FUNCTIONAL: Primary competitor for this opportunity
    /// TECHNICAL: Free-text, consider linking to Competitor entity
    /// </summary>
    public string? PrimaryCompetitor { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: All competitors involved in this opportunity
    /// TECHNICAL: JSON array of competitor names/details
    /// </summary>
    public string? Competitors { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Our strengths vs competition
    /// TECHNICAL: Captured for competitive intelligence
    /// </summary>
    public string? CompetitorStrengths { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Competitor weaknesses we can exploit
    /// TECHNICAL: Captured for competitive intelligence
    /// </summary>
    public string? CompetitorWeaknesses { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Competitor's quoted price if known
    /// TECHNICAL: Used for competitive pricing analysis
    /// </summary>
    public decimal? CompetitorPrice { get; set; }
    
    #endregion
    
    #region BANT Qualification
    
    /// <summary>
    /// FUNCTIONAL: Budget approval status
    /// TECHNICAL: Key BANT criterion
    /// </summary>
    public BudgetStatus BudgetStatus { get; set; } = BudgetStatus.Unknown;
    
    /// <summary>
    /// FUNCTIONAL: Approved/allocated budget amount
    /// TECHNICAL: Used to validate deal size fit
    /// </summary>
    public decimal? BudgetAmount { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal year of budget allocation
    /// TECHNICAL: For budget cycle alignment
    /// </summary>
    public string? BudgetFiscalYear { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Authority - decision maker identified
    /// TECHNICAL: BANT criterion flag
    /// </summary>
    public bool AuthorityConfirmed { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Need - business need validated
    /// TECHNICAL: BANT criterion flag
    /// </summary>
    public bool NeedConfirmed { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Timeline - purchase timeframe confirmed
    /// TECHNICAL: BANT criterion flag
    /// </summary>
    public bool TimelineConfirmed { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Overall BANT qualification score (0-4)
    /// TECHNICAL: Count of confirmed BANT criteria
    /// </summary>
    public int BantScore { get; set; } = 0;
    
    #endregion
    
    #region MEDDIC Qualification
    
    /// <summary>
    /// FUNCTIONAL: MEDDIC criteria completion flags
    /// TECHNICAL: Bitwise flags for each MEDDIC criterion
    /// </summary>
    public MeddicCriteria MeddicCriteria { get; set; } = MeddicCriteria.None;
    
    /// <summary>
    /// FUNCTIONAL: Quantified business metrics/ROI
    /// TECHNICAL: MEDDIC Metrics criterion
    /// </summary>
    public string? MetricsIdentified { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Economic buyer name and title
    /// TECHNICAL: MEDDIC Economic Buyer criterion
    /// </summary>
    public string? EconomicBuyer { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer's decision criteria documented
    /// TECHNICAL: MEDDIC Decision Criteria criterion
    /// </summary>
    public string? DecisionCriteria { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer's decision process documented
    /// TECHNICAL: MEDDIC Decision Process criterion
    /// </summary>
    public string? DecisionProcess { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Business pain points identified
    /// TECHNICAL: MEDDIC Identify Pain criterion
    /// </summary>
    public string? PainPoints { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Internal champion details
    /// TECHNICAL: MEDDIC Champion criterion
    /// </summary>
    public string? Champion { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Champion's title/role
    /// TECHNICAL: For stakeholder mapping
    /// </summary>
    public string? ChampionTitle { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Champion engagement level
    /// TECHNICAL: Health indicator
    /// </summary>
    public EngagementLevel ChampionEngagement { get; set; } = EngagementLevel.Low;
    
    /// <summary>
    /// FUNCTIONAL: MEDDIC score (0-100)
    /// TECHNICAL: Calculated from criteria completion
    /// </summary>
    public int MeddicScore { get; set; } = 0;
    
    #endregion
    
    #region Decision Making Unit
    
    /// <summary>
    /// FUNCTIONAL: All decision makers and influencers
    /// TECHNICAL: JSON array of {name, title, role, influence, stance}
    /// </summary>
    public string? DecisionMakers { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of stakeholders involved
    /// TECHNICAL: Complexity indicator
    /// </summary>
    public int? StakeholderCount { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Key blockers or detractors identified
    /// TECHNICAL: JSON array of blocker details
    /// </summary>
    public string? Blockers { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Proposed solution mapped to needs
    /// TECHNICAL: Solution selling artifact
    /// </summary>
    public string? ProposedSolution { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Quantified business case/ROI
    /// TECHNICAL: Value selling artifact
    /// </summary>
    public string? BusinessCase { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer's buying process stage
    /// TECHNICAL: Aligned with customer journey
    /// </summary>
    public string? CustomerBuyingStage { get; set; }
    
    #endregion
    
    #region Activity & Engagement Tracking
    
    /// <summary>
    /// FUNCTIONAL: Total number of activities logged
    /// TECHNICAL: Aggregate from activity records
    /// </summary>
    public int TotalActivities { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Number of meetings held
    /// TECHNICAL: Activity type aggregate
    /// </summary>
    public int MeetingCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Number of calls made
    /// TECHNICAL: Activity type aggregate
    /// </summary>
    public int CallCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Number of emails sent
    /// TECHNICAL: Activity type aggregate
    /// </summary>
    public int EmailCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Number of demos/presentations given
    /// TECHNICAL: Activity type aggregate
    /// </summary>
    public int DemoCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Last meeting date with customer
    /// TECHNICAL: Engagement tracking
    /// </summary>
    public DateTime? LastMeetingDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Next scheduled meeting
    /// TECHNICAL: Future engagement tracking
    /// </summary>
    public DateTime? NextMeetingDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Overall engagement level of the customer
    /// TECHNICAL: Manual or calculated assessment
    /// </summary>
    public EngagementLevel EngagementLevel { get; set; } = EngagementLevel.Medium;
    
    /// <summary>
    /// FUNCTIONAL: Days since last customer contact
    /// TECHNICAL: Calculated for stale opportunity detection
    /// </summary>
    public int? DaysSinceLastContact { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer response rate to communications
    /// TECHNICAL: Percentage of communications with response
    /// </summary>
    public decimal? ResponseRate { get; set; }
    
    #endregion
    
    #region Proof of Concept / Trial
    
    /// <summary>
    /// FUNCTIONAL: POC/trial is in progress or planned
    /// TECHNICAL: Boolean flag for POC deals
    /// </summary>
    public bool HasProofOfConcept { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: POC start date
    /// TECHNICAL: POC tracking
    /// </summary>
    public DateTime? PocStartDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: POC end date
    /// TECHNICAL: POC tracking
    /// </summary>
    public DateTime? PocEndDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: POC outcome (Success, Failed, Ongoing)
    /// TECHNICAL: POC result tracking
    /// </summary>
    public string? PocStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: POC success criteria defined
    /// TECHNICAL: POC evaluation criteria
    /// </summary>
    public string? PocSuccessCriteria { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: POC evaluation notes/results
    /// TECHNICAL: POC outcome documentation
    /// </summary>
    public string? PocNotes { get; set; }
    
    #endregion
    
    #region Source & Attribution
    
    /// <summary>
    /// FUNCTIONAL: Original lead source
    /// TECHNICAL: Marketing attribution
    /// </summary>
    public string? LeadSource { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Detailed lead source breakdown
    /// TECHNICAL: UTM parameters or granular source
    /// </summary>
    public string? LeadSourceDetail { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Marketing campaign that generated this
    /// TECHNICAL: Campaign attribution FK
    /// </summary>
    public int? CampaignId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Original lead before conversion
    /// TECHNICAL: Lead-to-opportunity tracking
    /// </summary>
    public int? OriginalLeadId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Partner who referred/influenced the deal
    /// TECHNICAL: Partner attribution
    /// </summary>
    public string? ReferralPartner { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Partner ID for referral commission
    /// TECHNICAL: Partner program tracking
    /// </summary>
    public int? ReferralPartnerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Marketing touchpoints in the journey
    /// TECHNICAL: JSON array of touchpoint data
    /// </summary>
    public string? MarketingTouchpoints { get; set; }
    
    #endregion
    
    #region Relationships (Foreign Keys)
    
    /// <summary>
    /// FUNCTIONAL: Customer/account this opportunity is for
    /// TECHNICAL: Required FK to Customer entity
    /// </summary>
    public int CustomerId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary contact person at customer
    /// TECHNICAL: Optional FK to CustomerContact entity
    /// </summary>
    public int? PrimaryContactId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sales rep owning this opportunity
    /// TECHNICAL: FK to User entity, drives assignment rules
    /// </summary>
    public int? AssignedToUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary product being sold
    /// TECHNICAL: Optional FK to Product entity
    /// </summary>
    public int? ProductId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Associated quote/proposal
    /// TECHNICAL: FK to Quote entity
    /// </summary>
    public int? QuoteId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Team member supporting this opportunity
    /// TECHNICAL: Optional FK for sales team collaboration
    /// </summary>
    public int? SalesEngineerUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Manager overseeing this opportunity
    /// TECHNICAL: FK for approval workflows
    /// </summary>
    public int? SalesManagerUserId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Parent opportunity for split deals
    /// TECHNICAL: Self-referencing FK
    /// </summary>
    public int? ParentOpportunityId { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Related opportunities (cross-sell, bundled)
    /// TECHNICAL: JSON array of related opportunity IDs
    /// </summary>
    public string? RelatedOpportunityIds { get; set; }
    
    #endregion
    
    #region Products & Line Items
    
    /// <summary>
    /// FUNCTIONAL: All products/services in this opportunity
    /// TECHNICAL: JSON array of {productId, name, quantity, price, discount}
    /// </summary>
    public string? Products { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Number of distinct products
    /// TECHNICAL: Aggregate count
    /// </summary>
    public int ProductCount { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Primary product family/category
    /// TECHNICAL: For product analytics
    /// </summary>
    public string? ProductFamily { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Product solution type
    /// TECHNICAL: For solution analytics
    /// </summary>
    public string? SolutionType { get; set; }
    
    #endregion
    
    #region Classification & Territory
    
    /// <summary>
    /// FUNCTIONAL: Tags for categorization and search
    /// TECHNICAL: Comma-separated or JSON array
    /// </summary>
    public string? Tags { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sales territory assignment
    /// TECHNICAL: Territory management integration
    /// </summary>
    public string? Territory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Geographic region
    /// TECHNICAL: Regional reporting
    /// </summary>
    public string? Region { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Country for the opportunity
    /// TECHNICAL: Geo reporting
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer industry vertical
    /// TECHNICAL: Vertical analytics
    /// </summary>
    public string? Industry { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Customer company size segment
    /// TECHNICAL: Segment analytics (SMB, Mid-Market, Enterprise)
    /// </summary>
    public string? Segment { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal quarter for close
    /// TECHNICAL: Fiscal reporting (Q1, Q2, Q3, Q4)
    /// </summary>
    public string? FiscalQuarter { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Fiscal year for close
    /// TECHNICAL: Fiscal reporting
    /// </summary>
    public int? FiscalYear { get; set; }
    
    #endregion
    
    #region Risk Assessment
    
    /// <summary>
    /// FUNCTIONAL: Overall risk score (0-100)
    /// TECHNICAL: Calculated from risk factors
    /// </summary>
    public int RiskScore { get; set; } = 0;
    
    /// <summary>
    /// FUNCTIONAL: Identified risk factors
    /// TECHNICAL: JSON array of {factor, severity, mitigation}
    /// </summary>
    public string? RiskFactors { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Deal is at risk of loss
    /// TECHNICAL: Flag for at-risk reporting
    /// </summary>
    public bool IsAtRisk { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Reason for at-risk status
    /// TECHNICAL: Documentation for risk
    /// </summary>
    public string? AtRiskReason { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Mitigation plan for risks
    /// TECHNICAL: Action plan documentation
    /// </summary>
    public string? RiskMitigationPlan { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Deal is stalled (no activity)
    /// TECHNICAL: Auto-flagged based on inactivity
    /// </summary>
    public bool IsStalled { get; set; } = false;
    
    /// <summary>
    /// FUNCTIONAL: Date when marked as stalled
    /// TECHNICAL: For stall duration tracking
    /// </summary>
    public DateTime? StalledDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Reason for stall
    /// TECHNICAL: Documentation
    /// </summary>
    public string? StalledReason { get; set; }
    
    #endregion
    
    #region Documentation & Notes
    
    /// <summary>
    /// FUNCTIONAL: General notes visible to all
    /// TECHNICAL: Rich text field
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Internal notes (not for customer)
    /// TECHNICAL: Restricted visibility
    /// </summary>
    public string? InternalNotes { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Executive summary of the opportunity
    /// TECHNICAL: High-level overview for leadership
    /// </summary>
    public string? ExecutiveSummary { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Technical requirements documented
    /// TECHNICAL: For solution architects
    /// </summary>
    public string? TechnicalRequirements { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Implementation requirements
    /// TECHNICAL: For services team planning
    /// </summary>
    public string? ImplementationRequirements { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Special terms or conditions
    /// TECHNICAL: Non-standard deal terms
    /// </summary>
    public string? SpecialTerms { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Attached documents references
    /// TECHNICAL: JSON array of document IDs/URLs
    /// </summary>
    public string? Attachments { get; set; }
    
    #endregion
    
    #region Custom Fields & Integration
    
    /// <summary>
    /// FUNCTIONAL: Flexible custom fields
    /// TECHNICAL: JSON object for extensibility
    /// </summary>
    public string? CustomFields { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sync status with external systems
    /// TECHNICAL: Integration tracking
    /// </summary>
    public string? SyncStatus { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Last sync timestamp
    /// TECHNICAL: Integration tracking
    /// </summary>
    public DateTime? LastSyncDate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: External system identifiers
    /// TECHNICAL: JSON object {system: id} for integrations
    /// </summary>
    public string? ExternalIds { get; set; }
    
    #endregion
    
    #region Audit & History
    
    /// <summary>
    /// FUNCTIONAL: Version number for optimistic concurrency
    /// TECHNICAL: Incremented on each update
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// FUNCTIONAL: Full audit trail of changes
    /// TECHNICAL: JSON array of {field, oldValue, newValue, userId, date}
    /// </summary>
    public string? ChangeHistory { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Date of last significant update
    /// TECHNICAL: Auto-updated on meaningful changes
    /// </summary>
    public DateTime? LastSignificantUpdate { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: User who last modified
    /// TECHNICAL: Audit tracking
    /// </summary>
    public int? LastModifiedByUserId { get; set; }
    
    #endregion

    #region Navigation Properties
    
    /// <summary>
    /// FUNCTIONAL: Associated customer account
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public Customer? Customer { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Primary product being sold
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public Product? Product { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Sales rep owning this opportunity
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public User? AssignedToUser { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Source marketing campaign
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public MarketingCampaign? Campaign { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Original lead that was converted
    /// TECHNICAL: Lead-to-opportunity attribution
    /// </summary>
    public Lead? OriginalLead { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Supporting sales engineer
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public User? SalesEngineer { get; set; }
    
    /// <summary>
    /// FUNCTIONAL: Supervising sales manager
    /// TECHNICAL: EF Core navigation property
    /// </summary>
    public User? SalesManager { get; set; }
    
    #endregion
    
    #region Calculated Properties
    
    /// <summary>
    /// FUNCTIONAL: Indicates if opportunity is in an open stage
    /// TECHNICAL: Calculated from Stage enum
    /// </summary>
    public bool IsOpen => Stage != OpportunityStage.ClosedWon && 
                          Stage != OpportunityStage.ClosedLost && 
                          Stage != OpportunityStage.Disqualified;
    
    /// <summary>
    /// FUNCTIONAL: Indicates if opportunity was won
    /// TECHNICAL: Calculated from Stage enum
    /// </summary>
    public bool IsWon => Stage == OpportunityStage.ClosedWon;
    
    /// <summary>
    /// FUNCTIONAL: Indicates if opportunity was lost
    /// TECHNICAL: Calculated from Stage enum
    /// </summary>
    public bool IsLost => Stage == OpportunityStage.ClosedLost || Stage == OpportunityStage.Disqualified;
    
    /// <summary>
    /// FUNCTIONAL: Indicates if opportunity is fully qualified
    /// TECHNICAL: Based on BANT score threshold
    /// </summary>
    public bool IsQualified => BantScore >= 3 || MeddicScore >= 50;
    
    /// <summary>
    /// FUNCTIONAL: Days until expected close
    /// TECHNICAL: Calculated from ExpectedCloseDate
    /// </summary>
    public int? DaysToClose => ExpectedCloseDate.HasValue 
        ? (int)(ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays 
        : null;
    
    /// <summary>
    /// FUNCTIONAL: Indicates if close date is overdue
    /// TECHNICAL: Calculated from CloseDate
    /// </summary>
    public bool IsOverdue => CloseDate.HasValue && CloseDate.Value < DateTime.UtcNow && IsOpen;
    
    #endregion
}
