namespace CRM.Core.Entities;

/// <summary>
/// Email template category for organization
/// </summary>
public enum EmailTemplateCategory
{
    General = 0,
    Sales = 1,
    Marketing = 2,
    Support = 3,
    Welcome = 4,
    FollowUp = 5,
    Newsletter = 6,
    Notification = 7,
    Transactional = 8,
    Custom = 99
}

/// <summary>
/// Email template for creating reusable email content
/// Supports merge fields/placeholders for personalization
/// </summary>
public class EmailTemplate : BaseEntity
{
    /// <summary>
    /// Template name for identification
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Template description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Category for organization
    /// </summary>
    public EmailTemplateCategory Category { get; set; } = EmailTemplateCategory.General;
    
    /// <summary>
    /// Email subject line (supports merge fields)
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Plain text body (supports merge fields)
    /// </summary>
    public string? PlainTextBody { get; set; }
    
    /// <summary>
    /// HTML body (supports merge fields)
    /// </summary>
    public string? HtmlBody { get; set; }
    
    /// <summary>
    /// Whether this template is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Whether this is a system template (cannot be deleted)
    /// </summary>
    public bool IsSystem { get; set; } = false;
    
    /// <summary>
    /// JSON array of available merge fields
    /// </summary>
    public string? MergeFieldsJson { get; set; }
    
    /// <summary>
    /// Default From email for this template
    /// </summary>
    public string? FromEmail { get; set; }
    
    /// <summary>
    /// Default From name for this template
    /// </summary>
    public string? FromName { get; set; }
    
    /// <summary>
    /// Default Reply-To address
    /// </summary>
    public string? ReplyToEmail { get; set; }
    
    /// <summary>
    /// JSON array of default attachments
    /// </summary>
    public string? DefaultAttachmentsJson { get; set; }
    
    /// <summary>
    /// User who created this template
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    /// <summary>
    /// Number of times this template was used
    /// </summary>
    public int UsageCount { get; set; } = 0;
    
    /// <summary>
    /// Last time this template was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Preview text for email clients
    /// </summary>
    public string? PreviewText { get; set; }
    
    /// <summary>
    /// Associated channel ID (optional)
    /// </summary>
    public int? ChannelId { get; set; }
    
    // Navigation
    public virtual ICollection<CommunicationMessage> Messages { get; set; } = new List<CommunicationMessage>();
}
