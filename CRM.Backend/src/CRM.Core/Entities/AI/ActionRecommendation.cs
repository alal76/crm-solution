namespace CRM.Core.Entities.AI;

#region Action Recommendation Enumerations

/// <summary>
/// Type of next best action.
/// </summary>
public enum NextBestActionType
{
    /// <summary>Make a phone call</summary>
    Call = 0,
    
    /// <summary>Send an email</summary>
    Email = 1,
    
    /// <summary>Schedule a meeting</summary>
    Meeting = 2,
    
    /// <summary>Send proposal or quote</summary>
    SendProposal = 3,
    
    /// <summary>Follow up on previous interaction</summary>
    FollowUp = 4,
    
    /// <summary>Share content or resource</summary>
    ShareContent = 5,
    
    /// <summary>Create task or reminder</summary>
    CreateTask = 6,
    
    /// <summary>Escalate internally</summary>
    Escalate = 7,
    
    /// <summary>Introduce colleague</summary>
    IntroduceColleague = 8,
    
    /// <summary>Demo or presentation</summary>
    Demo = 9,
    
    /// <summary>Negotiation action</summary>
    Negotiate = 10,
    
    /// <summary>Close the deal</summary>
    CloseAsk = 11,
    
    /// <summary>Upsell or cross-sell</summary>
    Upsell = 12,
    
    /// <summary>Customer success check-in</summary>
    CheckIn = 13,
    
    /// <summary>Support ticket action</summary>
    SupportAction = 14,
    
    /// <summary>No action needed</summary>
    NoAction = 99
}

/// <summary>
/// Entity type the action relates to.
/// </summary>
public enum ActionTargetType
{
    /// <summary>Action for a lead</summary>
    Lead = 0,
    
    /// <summary>Action for an opportunity</summary>
    Opportunity = 1,
    
    /// <summary>Action for a customer</summary>
    Customer = 2,
    
    /// <summary>Action for a contact</summary>
    Contact = 3,
    
    /// <summary>Action for a support case</summary>
    SupportCase = 4,
    
    /// <summary>Action for a quote</summary>
    Quote = 5,
    
    /// <summary>General action</summary>
    General = 6
}

/// <summary>
/// Priority level for actions.
/// </summary>
public enum ActionPriorityLevel
{
    /// <summary>Critical - do immediately</summary>
    Critical = 0,
    
    /// <summary>High priority - do today</summary>
    High = 1,
    
    /// <summary>Medium priority - do this week</summary>
    Medium = 2,
    
    /// <summary>Low priority - do when time permits</summary>
    Low = 3,
    
    /// <summary>Optional action</summary>
    Optional = 4
}

/// <summary>
/// Status of a recommended action.
/// </summary>
public enum ActionRecommendationStatus
{
    /// <summary>Action is pending</summary>
    Pending = 0,
    
    /// <summary>Action was accepted</summary>
    Accepted = 1,
    
    /// <summary>Action was dismissed</summary>
    Dismissed = 2,
    
    /// <summary>Action is in progress</summary>
    InProgress = 3,
    
    /// <summary>Action was completed</summary>
    Completed = 4,
    
    /// <summary>Action has expired</summary>
    Expired = 5,
    
    /// <summary>Action was snoozed</summary>
    Snoozed = 6
}

/// <summary>
/// Channel for the action.
/// </summary>
public enum ActionChannel
{
    /// <summary>Phone call</summary>
    Phone = 0,
    
    /// <summary>Email</summary>
    Email = 1,
    
    /// <summary>Video meeting</summary>
    VideoMeeting = 2,
    
    /// <summary>In-person meeting</summary>
    InPerson = 3,
    
    /// <summary>LinkedIn</summary>
    LinkedIn = 4,
    
    /// <summary>Chat or messaging</summary>
    Chat = 5,
    
    /// <summary>Self-service portal</summary>
    SelfService = 6,
    
    /// <summary>Internal system</summary>
    Internal = 7
}

#endregion

/// <summary>
/// AI-generated next best action recommendations.
/// Uses Allen AI models for intelligent action suggestions.
/// </summary>
public class ActionRecommendation : BaseEntity
{
    #region Target Entity
    
    /// <summary>Entity type</summary>
    public ActionTargetType TargetType { get; set; }
    
    /// <summary>Entity ID (Lead, Opportunity, Customer, etc.)</summary>
    public int TargetEntityId { get; set; }
    
    /// <summary>Entity name for display</summary>
    public string TargetEntityName { get; set; } = string.Empty;
    
    #endregion
    
    #region Assigned User
    
    /// <summary>User ID to perform action</summary>
    public int? AssignedUserId { get; set; }
    
    /// <summary>Navigation to assigned user</summary>
    public User? AssignedUser { get; set; }
    
    #endregion
    
    #region Action Details
    
    /// <summary>Action type</summary>
    public NextBestActionType ActionType { get; set; }
    
    /// <summary>Recommended channel</summary>
    public ActionChannel Channel { get; set; }
    
    /// <summary>Action title</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Action description</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Why this action is recommended</summary>
    public string Rationale { get; set; } = string.Empty;
    
    /// <summary>Suggested message or script (for email/call)</summary>
    public string? SuggestedContent { get; set; }
    
    /// <summary>Talking points (JSON array)</summary>
    public string? TalkingPointsJson { get; set; }
    
    #endregion
    
    #region Priority and Timing
    
    /// <summary>Priority level</summary>
    public ActionPriorityLevel Priority { get; set; } = ActionPriorityLevel.Medium;
    
    /// <summary>Impact score (0-100)</summary>
    public decimal ImpactScore { get; set; }
    
    /// <summary>Recommended timing</summary>
    public DateTime? RecommendedAt { get; set; }
    
    /// <summary>Best day of week</summary>
    public DayOfWeek? BestDayOfWeek { get; set; }
    
    /// <summary>Best hour of day (0-23)</summary>
    public int? BestHourOfDay { get; set; }
    
    /// <summary>Action deadline</summary>
    public DateTime? Deadline { get; set; }
    
    /// <summary>Time sensitivity (hours until impact decreases)</summary>
    public int? TimeSensitivityHours { get; set; }
    
    #endregion
    
    #region Expected Outcome
    
    /// <summary>Expected outcome if action taken</summary>
    public string? ExpectedOutcome { get; set; }
    
    /// <summary>Success probability (0-1)</summary>
    public decimal SuccessProbability { get; set; }
    
    /// <summary>Estimated value impact</summary>
    public decimal? EstimatedValueImpact { get; set; }
    
    /// <summary>Risk if action not taken</summary>
    public string? RiskIfNotTaken { get; set; }
    
    #endregion
    
    #region Confidence and Scoring
    
    /// <summary>AI confidence in recommendation (0-1)</summary>
    public decimal Confidence { get; set; }
    
    /// <summary>Relevance score (0-100)</summary>
    public decimal RelevanceScore { get; set; }
    
    /// <summary>Features that drove this recommendation (JSON)</summary>
    public string? DrivingFactorsJson { get; set; }
    
    #endregion
    
    #region Action Status
    
    /// <summary>Current status</summary>
    public ActionRecommendationStatus Status { get; set; } = ActionRecommendationStatus.Pending;
    
    /// <summary>When status was updated</summary>
    public DateTime? StatusUpdatedAt { get; set; }
    
    /// <summary>Reason for dismissal or snooze</summary>
    public string? StatusReason { get; set; }
    
    /// <summary>Snoozed until</summary>
    public DateTime? SnoozedUntil { get; set; }
    
    #endregion
    
    #region Feedback
    
    /// <summary>User feedback on recommendation quality (1-5)</summary>
    public int? UserRating { get; set; }
    
    /// <summary>User feedback comments</summary>
    public string? UserFeedback { get; set; }
    
    /// <summary>Was the action taken?</summary>
    public bool? ActionTaken { get; set; }
    
    /// <summary>Actual outcome if action was taken</summary>
    public string? ActualOutcome { get; set; }
    
    /// <summary>Did it achieve expected result?</summary>
    public bool? AchievedExpectedResult { get; set; }
    
    #endregion
    
    #region Related Actions
    
    /// <summary>Alternative actions (JSON array)</summary>
    public string? AlternativeActionsJson { get; set; }
    
    /// <summary>Follow-up action IDs (JSON array)</summary>
    public string? FollowUpActionIdsJson { get; set; }
    
    /// <summary>Sequence in a cadence (if part of sequence)</summary>
    public int? SequenceStep { get; set; }
    
    /// <summary>Cadence/sequence ID if part of automated sequence</summary>
    public int? CadenceId { get; set; }
    
    #endregion
    
    #region Generation Metadata
    
    /// <summary>When recommendation was generated</summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>When recommendation expires</summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(3);
    
    /// <summary>Model version</summary>
    public string ModelVersion { get; set; } = "1.0";
    
    /// <summary>AI Model used</summary>
    public int? AIModelId { get; set; }
    
    /// <summary>Navigation to AI Model</summary>
    public AIModel? AIModel { get; set; }
    
    #endregion
}
