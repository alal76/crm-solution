namespace CRM.Core.Entities.AI;

#region Opportunity Insight Enumerations

/// <summary>
/// Opportunity win probability category.
/// </summary>
public enum WinProbabilityCategory
{
    /// <summary>Very unlikely to win (0-20%)</summary>
    VeryUnlikely = 0,
    
    /// <summary>Unlikely to win (20-40%)</summary>
    Unlikely = 1,
    
    /// <summary>Possible win (40-60%)</summary>
    Possible = 2,
    
    /// <summary>Likely to win (60-80%)</summary>
    Likely = 3,
    
    /// <summary>Very likely to win (80-100%)</summary>
    VeryLikely = 4
}

/// <summary>
/// Deal health status.
/// </summary>
public enum DealHealthStatus
{
    /// <summary>Deal is progressing well</summary>
    Healthy = 0,
    
    /// <summary>Deal needs attention</summary>
    AtRisk = 1,
    
    /// <summary>Deal is stalling</summary>
    Stalled = 2,
    
    /// <summary>Deal is in danger</summary>
    Critical = 3,
    
    /// <summary>Deal velocity is excellent</summary>
    Accelerating = 4
}

/// <summary>
/// Type of deal risk.
/// </summary>
public enum DealRiskType
{
    /// <summary>Competitor threat</summary>
    CompetitorThreat = 0,
    
    /// <summary>Budget constraints</summary>
    BudgetRisk = 1,
    
    /// <summary>Timeline slippage</summary>
    TimelineRisk = 2,
    
    /// <summary>Champion leaving</summary>
    ChampionRisk = 3,
    
    /// <summary>Technical concerns</summary>
    TechnicalRisk = 4,
    
    /// <summary>Decision maker not engaged</summary>
    DecisionMakerRisk = 5,
    
    /// <summary>Deal velocity slowing</summary>
    VelocityRisk = 6,
    
    /// <summary>Stakeholder alignment issues</summary>
    StakeholderRisk = 7
}

/// <summary>
/// Recommended action type.
/// </summary>
public enum OpportunityActionType
{
    /// <summary>Schedule executive sponsor meeting</summary>
    ExecutiveSponsorMeeting = 0,
    
    /// <summary>Send case study</summary>
    SendCaseStudy = 1,
    
    /// <summary>Offer discount</summary>
    OfferDiscount = 2,
    
    /// <summary>Schedule technical demo</summary>
    TechnicalDemo = 3,
    
    /// <summary>Proposal review meeting</summary>
    ProposalReview = 4,
    
    /// <summary>Competitor displacement strategy</summary>
    CompetitorDisplacement = 5,
    
    /// <summary>Multi-threading outreach</summary>
    MultiThreading = 6,
    
    /// <summary>Risk mitigation discussion</summary>
    RiskMitigation = 7,
    
    /// <summary>Fast-track close</summary>
    FastTrackClose = 8
}

#endregion

/// <summary>
/// AI-generated insights for opportunities using Allen AI models.
/// </summary>
public class OpportunityInsight : BaseEntity
{
    #region Opportunity Reference
    
    /// <summary>Opportunity ID</summary>
    public int OpportunityId { get; set; }
    
    /// <summary>Navigation to Opportunity</summary>
    public Opportunity? Opportunity { get; set; }
    
    #endregion
    
    #region Win Prediction
    
    /// <summary>Win probability (0-1)</summary>
    public decimal WinProbability { get; set; }
    
    /// <summary>Win probability category</summary>
    public WinProbabilityCategory WinCategory { get; set; }
    
    /// <summary>Confidence in prediction (0-1)</summary>
    public decimal Confidence { get; set; }
    
    /// <summary>Probability trend (positive = improving)</summary>
    public decimal ProbabilityTrend { get; set; }
    
    /// <summary>Previous probability (for trend)</summary>
    public decimal? PreviousProbability { get; set; }
    
    #endregion
    
    #region Deal Health
    
    /// <summary>Deal health status</summary>
    public DealHealthStatus HealthStatus { get; set; }
    
    /// <summary>Deal health score (0-100)</summary>
    public decimal HealthScore { get; set; }
    
    /// <summary>Deal velocity score (0-100)</summary>
    public decimal VelocityScore { get; set; }
    
    /// <summary>Engagement score (0-100)</summary>
    public decimal EngagementScore { get; set; }
    
    /// <summary>Stakeholder coverage score (0-100)</summary>
    public decimal StakeholderScore { get; set; }
    
    #endregion
    
    #region Timeline Analysis
    
    /// <summary>Predicted close date</summary>
    public DateTime? PredictedCloseDate { get; set; }
    
    /// <summary>Days until predicted close</summary>
    public int? DaysToClose { get; set; }
    
    /// <summary>Is deal on track for expected close</summary>
    public bool OnTrackForClose { get; set; }
    
    /// <summary>Days slippage from original close date</summary>
    public int? CloseSlippage { get; set; }
    
    /// <summary>Average sales cycle for similar deals (days)</summary>
    public int? AvgSalesCycleDays { get; set; }
    
    /// <summary>Deal is ahead/behind average cycle (days)</summary>
    public int? CycleVariance { get; set; }
    
    #endregion
    
    #region Value Analysis
    
    /// <summary>Predicted deal value</summary>
    public decimal? PredictedValue { get; set; }
    
    /// <summary>Weighted pipeline value (probability * value)</summary>
    public decimal? WeightedValue { get; set; }
    
    /// <summary>Upside potential</summary>
    public decimal? UpsidePotential { get; set; }
    
    /// <summary>Risk-adjusted value</summary>
    public decimal? RiskAdjustedValue { get; set; }
    
    #endregion
    
    #region Risk Analysis
    
    /// <summary>Primary risk type</summary>
    public DealRiskType? PrimaryRiskType { get; set; }
    
    /// <summary>Risk level (0-100)</summary>
    public decimal RiskLevel { get; set; }
    
    /// <summary>All identified risks (JSON array)</summary>
    public string? RisksJson { get; set; }
    
    /// <summary>Risk mitigation suggestions</summary>
    public string? RiskMitigationSuggestions { get; set; }
    
    #endregion
    
    #region Competitive Intelligence
    
    /// <summary>Competitors identified</summary>
    public string? CompetitorsJson { get; set; }
    
    /// <summary>Primary competitor</summary>
    public string? PrimaryCompetitor { get; set; }
    
    /// <summary>Competitive position score (0-100)</summary>
    public decimal? CompetitivePositionScore { get; set; }
    
    /// <summary>Differentiation suggestions</summary>
    public string? DifferentiationSuggestions { get; set; }
    
    #endregion
    
    #region AI Recommendations
    
    /// <summary>Recommended next action</summary>
    public OpportunityActionType? RecommendedAction { get; set; }
    
    /// <summary>Action priority (1=highest, 5=lowest)</summary>
    public int ActionPriority { get; set; } = 3;
    
    /// <summary>Detailed action recommendations (JSON array)</summary>
    public string? ActionRecommendationsJson { get; set; }
    
    /// <summary>Talking points for next meeting</summary>
    public string? TalkingPoints { get; set; }
    
    /// <summary>Objection handling suggestions</summary>
    public string? ObjectionHandling { get; set; }
    
    /// <summary>AI narrative insights</summary>
    public string? AIInsights { get; set; }
    
    #endregion
    
    #region Similar Deal Analysis
    
    /// <summary>Similar won deals count</summary>
    public int? SimilarWonDealsCount { get; set; }
    
    /// <summary>Similar lost deals count</summary>
    public int? SimilarLostDealsCount { get; set; }
    
    /// <summary>Win rate for similar deals</summary>
    public decimal? SimilarDealsWinRate { get; set; }
    
    /// <summary>Key success patterns from similar deals</summary>
    public string? SuccessPatternsJson { get; set; }
    
    #endregion
    
    #region Scoring Metadata
    
    /// <summary>When insight was generated</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>When insight expires</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(1);
    
    /// <summary>Model version</summary>
    public string ModelVersion { get; set; } = "1.0";
    
    /// <summary>AI Model used</summary>
    public int? AIModelId { get; set; }
    
    /// <summary>Navigation to AI Model</summary>
    public AIModel? AIModel { get; set; }
    
    #endregion
}
