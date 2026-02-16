using System;

namespace CRM.Core.Dtos;

/// <summary>
/// Generic entity status - used across many entities (Active, Inactive, Deleted).
/// </summary>
public enum EntityStatus
{
    /// <summary>Entity is active and visible.</summary>
    Active = 1,

    /// <summary>Entity is inactive but preserved.</summary>
    Inactive = 2,

    /// <summary>Entity is soft-deleted and hidden.</summary>
    Deleted = 3,

    /// <summary>Entity is in draft/pending state.</summary>
    Draft = 4,

    /// <summary>Entity is archived.</summary>
    Archived = 5
}

/// <summary>
/// Priority levels for tasks, tickets, issues, etc.
/// </summary>
public enum PriorityLevel
{
    /// <summary>Critical/emergent - requires immediate action.</summary>
    Critical = 1,

    /// <summary>High priority - address soon.</summary>
    High = 2,

    /// <summary>Normal/standard priority.</summary>
    Normal = 3,

    /// <summary>Low priority - address when time permits.</summary>
    Low = 4
}

/// <summary>
/// Generic workflow stage/status for multi-step processes.
/// </summary>
public enum WorkflowStage
{
    /// <summary>Initial stage - just created.</summary>
    New = 1,

    /// <summary>Currently being processed.</summary>
    InProgress = 2,

    /// <summary>Waiting for external action/input.</summary>
    OnHold = 3,

    /// <summary>Completed successfully.</summary>
    Completed = 4,

    /// <summary>Process cancelled or rejected.</summary>
    Cancelled = 5,

    /// <summary>Failed to complete.</summary>
    Failed = 6
}

/// <summary>
/// Enum for different types of actions (Create, Update, Delete, etc.).
/// </summary>
public enum ActionType
{
    /// <summary>Create/Insert action.</summary>
    Create = 1,

    /// <summary>Read/Retrieve action.</summary>
    Read = 2,

    /// <summary>Update/Modify action.</summary>
    Update = 3,

    /// <summary>Delete action.</summary>
    Delete = 4,

    /// <summary>List/Query action.</summary>
    List = 5,

    /// <summary>Export action.</summary>
    Export = 6,

    /// <summary>Import action.</summary>
    Import = 7,

    /// <summary>Bulk operation.</summary>
    Bulk = 8
}

/// <summary>
/// Enum for HTTP request methods.
/// </summary>
public enum HttpMethod
{
    /// <summary>GET request.</summary>
    Get = 1,

    /// <summary>POST request.</summary>
    Post = 2,

    /// <summary>PUT request.</summary>
    Put = 3,

    /// <summary>PATCH request.</summary>
    Patch = 4,

    /// <summary>DELETE request.</summary>
    Delete = 5
}

/// <summary>
/// Enum for user roles/permissions.
/// </summary>
public enum UserRole
{
    /// <summary>Super administrator - full system access.</summary>
    SuperAdmin = 1,

    /// <summary>Administrator - organizational/module level access.</summary>
    Admin = 2,

    /// <summary>Manager - team/department level access.</summary>
    Manager = 3,

    /// <summary>User - standard access.</summary>
    User = 4,

    /// <summary>Guest - limited, read-only access.</summary>
    Guest = 5,

    /// <summary>System - service/bot account (internal only).</summary>
    System = 6
}

/// <summary>
/// Enum for account/customer types.
/// </summary>
public enum AccountType
{
    /// <summary>Individual person.</summary>
    Individual = 1,

    /// <summary>Business/Company.</summary>
    Company = 2,

    /// <summary>Government/Public sector.</summary>
    Government = 3,

    /// <summary>Non-profit organization.</summary>
    NonProfit = 4,

    /// <summary>Educational institution.</summary>
    Educational = 5,

    /// <summary>Resource partner/reseller.</summary>
    Partner = 6
}

/// <summary>
/// Enum for contact roles/relationships.
/// </summary>
public enum ContactRole
{
    /// <summary>Decision maker.</summary>
    DecisionMaker = 1,

    /// <summary>Budget authority.</summary>
    BudgetAuthority = 2,

    /// <summary>Technical contact.</summary>
    TechnicalContact = 3,

    /// <summary>Maintenance contact.</summary>
    MaintenanceContact = 4,

    /// <summary>Invoice recipient.</summary>
    InvoiceRecipient = 5,

    /// <summary>General contact.</summary>
    General = 6
}

/// <summary>
/// Enum for opportunity/deal stages.
/// </summary>
public enum OpportunityStage
{
    /// <summary>Lead qualified - initial stage.</summary>
    Prospecting = 1,

    /// <summary>Proposal/demo delivered.</summary>
    Proposal = 2,

    /// <summary>Negotiation in progress.</summary>
    Negotiation = 3,

    /// <summary>Final stage - awaiting approval.</summary>
    Decision = 4,

    /// <summary>Successfully closed.</summary>
    Won = 5,

    /// <summary>Lost opportunity.</summary>
    Lost = 6,

    /// <summary>On hold - will revisit.</summary>
    OnHold = 7
}

/// <summary>
/// Enum for payment status tracking.
/// </summary>
public enum PaymentStatus
{
    /// <summary>Not yet sent to customer.</summary>
    Draft = 1,

    /// <summary>Sent and awaiting payment.</summary>
    Outstanding = 2,

    /// <summary>Partially paid.</summary>
    PartiallyPaid = 3,

    /// <summary>Fully paid.</summary>
    Paid = 4,

    /// <summary>Overdue - payment not received by due date.</summary>
    Overdue = 5,

    /// <summary>Payment cancelled/reversed.</summary>
    Cancelled = 6
}

/// <summary>
/// Enum for invoice status.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>Draft - not yet finalized.</summary>
    Draft = 1,

    /// <summary>Sent to customer.</summary>
    Sent = 2,

    /// <summary>Viewed by customer.</summary>
    Viewed = 3,

    /// <summary>Awaiting payment.</summary>
    Open = 4,

    /// <summary>Partially paid.</summary>
    PartiallyPaid = 5,

    /// <summary>Fully paid.</summary>
    Paid = 6,

    /// <summary>Overdue.</summary>
    Overdue = 7,

    /// <summary>Cancelled/voided.</summary>
    Cancelled = 8
}

/// <summary>
/// Enum for campaign status.
/// </summary>
public enum CampaignStatus
{
    /// <summary>Not yet started.</summary>
    Draft = 1,

    /// <summary>Currently running/active.</summary>
    Active = 2,

    /// <summary>Paused temporarily.</summary>
    Paused = 3,

    /// <summary>Completed/finished.</summary>
    Completed = 4,

    /// <summary>Cancelled.</summary>
    Cancelled = 5
}

/// <summary>
/// Enum for ticket/incident severity.
/// </summary>
public enum SeverityLevel
{
    /// <summary>System down - critical business impact.</summary>
    Critical = 1,

    /// <summary>Major feature unavailable.</summary>
    High = 2,

    /// <summary>Feature partially working.</summary>
    Medium = 3,

    /// <summary>Minor issue - workaround available.</summary>
    Low = 4
}

/// <summary>
/// Enum for ticket/incident status.
/// </summary>
public enum TicketStatus
{
    /// <summary>Just created.</summary>
    New = 1,

    /// <summary>Currently being worked on.</summary>
    InProgress = 2,

    /// <summary>Waiting for customer response.</summary>
    WaitingOnCustomer = 3,

    /// <summary>Waiting for vendor/third party.</summary>
    WaitingOnVendor = 4,

    /// <summary>In queue for assignment.</summary>
    Queued = 5,

    /// <summary>Escalated to higher level support.</summary>
    Escalated = 6,

    /// <summary>Resolved - awaiting customer confirmation.</summary>
    Resolved = 7,

    /// <summary>Closed.</summary>
    Closed = 8,

    /// <summary>Reopened by customer.</summary>
    Reopened = 9
}

/// <summary>
/// Enum for message/communication channels.
/// </summary>
public enum CommunicationChannel
{
    /// <summary>Email communication.</summary>
    Email = 1,

    /// <summary>Phone/call.</summary>
    Phone = 2,

    /// <summary>SMS text message.</summary>
    SMS = 3,

    /// <summary>In-app messaging.</summary>
    InApp = 4,

    /// <summary>Chat/instant messaging.</summary>
    Chat = 5,

    /// <summary>Social media.</summary>
    SocialMedia = 6,

    /// <summary>In-person meeting.</summary>
    InPerson = 7,

    /// <summary>Video conference.</summary>
    VideoConference = 8
}

/// <summary>
/// Enum for subscription status.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Trial period active.</summary>
    Trial = 1,

    /// <summary>Active subscription.</summary>
    Active = 2,

    /// <summary>Paused temporarily.</summary>
    Paused = 3,

    /// <summary>Cancelled by customer.</summary>
    Cancelled = 4,

    /// <summary>Expired.</summary>
    Expired = 5,

    /// <summary>Failed payment - at risk of cancellation.</summary>
    PaymentFailed = 6
}

/// <summary>
/// Enum for contract status.
/// </summary>
public enum ContractStatus
{
    /// <summary>In draft/negotiation.</summary>
    Draft = 1,

    /// <summary>Awaiting signature.</summary>
    AwaitingSignature = 2,

    /// <summary>Active/in effect.</summary>
    Active = 3,

    /// <summary>Expired but can be renewed.</summary>
    Expired = 4,

    /// <summary>Terminated early.</summary>
    Terminated = 5
}

/// <summary>
/// Enum for lead status.
/// </summary>
public enum LeadStatus
{
    /// <summary>New lead just created.</summary>
    New = 1,

    /// <summary>Currently being worked.</summary>
    Working = 2,

    /// <summary>Qualified and ready for sales.</summary>
    Qualified = 3,

    /// <summary>Lost lead - not interested.</summary>
    Lost = 4,

    /// <summary>Converted to opportunity/customer.</summary>
    Converted = 5,

    /// <summary>Not qualified for business.</summary>
    Unqualified = 6
}

/// <summary>
/// Enum for notification/event types.
/// </summary>
public enum EventType
{
    /// <summary>Entity created event.</summary>
    Created = 1,

    /// <summary>Entity updated event.</summary>
    Updated = 2,

    /// <summary>Entity deleted event.</summary>
    Deleted = 3,

    /// <summary>Status changed event.</summary>
    StatusChanged = 4,

    /// <summary>Deadline/reminder event.</summary>
    Reminder = 5,

    /// <summary>Assignment event.</summary>
    Assigned = 6,

    /// <summary>Escalation event.</summary>
    Escalated = 7,

    /// <summary>Custom business event.</summary>
    Custom = 8
}

/// <summary>
/// Enum for data import/export formats.
/// </summary>
public enum DataFormat
{
    /// <summary>Comma-separated values.</summary>
    CSV = 1,

    /// <summary>Excel spreadsheet.</summary>
    Excel = 2,

    /// <summary>JSON format.</summary>
    JSON = 3,

    /// <summary>XML format.</summary>
    XML = 4,

    /// <summary>PDF document.</summary>
    PDF = 5
}

/// <summary>
/// Enum for record visibility/sharing levels.
/// </summary>
public enum VisibilityLevel
{
    /// <summary>Visible only to creator/owner.</summary>
    Private = 1,

    /// <summary>Visible to team/department.</summary>
    Team = 2,

    /// <summary>Visible to organization.</summary>
    Organization = 3,

    /// <summary>Public - visible to everyone including unknowns.</summary>
    Public = 4,

    /// <summary>Visible to specific shared users/groups.</summary>
    Shared = 5
}

/// <summary>
/// Enum for agreement/approval status.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Pending - awaiting approval.</summary>
    Pending = 1,

    /// <summary>Approved.</summary>
    Approved = 2,

    /// <summary>Rejected.</summary>
    Rejected = 3,

    /// <summary>Changes requested - requires revision.</summary>
    ChangesRequested = 4,

    /// <summary>Revoked/withdrawn.</summary>
    Revoked = 5
}

/// <summary>
/// Enum for common frequency intervals.
/// </summary>
public enum FrequencyInterval
{
    /// <summary>One-time/no recurrence.</summary>
    Once = 0,

    /// <summary>Every day.</summary>
    Daily = 1,

    /// <summary>Every week on the same day.</summary>
    Weekly = 2,

    /// <summary>Every two weeks.</summary>
    BiWeekly = 3,

    /// <summary>Every month on the same date.</summary>
    Monthly = 4,

    /// <summary>Every quarter.</summary>
    Quarterly = 5,

    /// <summary>Every six months.</summary>
    SemiAnnually = 6,

    /// <summary>Every year on the same date.</summary>
    Annually = 7
}

/// <summary>
/// Enum for time zone handling.
/// </summary>
public enum TimeZoneHandling
{
    /// <summary>Convert to user's time zone.</summary>
    UserTimeZone = 1,

    /// <summary>Use UTC/GMT.</summary>
    UTC = 2,

    /// <summary>Use specific fixed time zone.</summary>
    FixedTimeZone = 3
}

/// <summary>
/// Enum for language locale preferences.
/// </summary>
public enum LanguageLocale
{
    /// <summary>English (United States).</summary>
    EnglishUS = 1,

    /// <summary>English (United Kingdom).</summary>
    EnglishGB = 2,

    /// <summary>Spanish (Spain).</summary>
    SpanishES = 3,

    /// <summary>Spanish (Latin America).</summary>
    SpanishLA = 4,

    /// <summary>French.</summary>
    French = 5,

    /// <summary>German.</summary>
    German = 6,

    /// <summary>Italian.</summary>
    Italian = 7,

    /// <summary>Portuguese (Brazil).</summary>
    PortugueseBR = 8,

    /// <summary>Japanese.</summary>
    Japanese = 9,

    /// <summary>Chinese (Simplified).</summary>
    ChineseSimplified = 10,

    /// <summary>Chinese (Traditional).</summary>
    ChineseTraditional = 11
}

/// <summary>
/// Enum for relationship types between entities.
/// </summary>
public enum RelationshipType
{
    /// <summary>One-to-one relationship.</summary>
    OneToOne = 1,

    /// <summary>One-to-many relationship.</summary>
    OneToMany = 2,

    /// <summary>Many-to-many relationship.</summary>
    ManyToMany = 3,

    /// <summary>Hierarchical parent-child.</summary>
    Hierarchical = 4
}

/// <summary>
/// Enum for data consistency/replication status.
/// </summary>
public enum SyncStatus
{
    /// <summary>Not yet synchronized.</summary>
    Pending = 1,

    /// <summary>Currently syncing.</summary>
    InProgress = 2,

    /// <summary>Successfully synchronized.</summary>
    Synced = 3,

    /// <summary>Sync failed.</summary>
    Failed = 4,

    /// <summary>Out of sync with source.</summary>
    OutOfSync = 5
}
