namespace CRM.Core.Entities;

/// <summary>
/// Tracks following of social media accounts for contacts/accounts
/// Allows CRM users to follow a contact's social media activity
/// </summary>
public class SocialMediaFollow : BaseEntity
{
    /// <summary>
    /// The social media account being followed
    /// </summary>
    public int SocialMediaAccountId { get; set; }
    
    /// <summary>
    /// The CRM user who is following this social media account
    /// </summary>
    public int FollowedByUserId { get; set; }
    
    /// <summary>
    /// Type of entity the social media account belongs to (Customer, Contact, Lead, Account)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the entity the social media account belongs to
    /// </summary>
    public int EntityId { get; set; }
    
    /// <summary>
    /// When the follow was initiated
    /// </summary>
    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Is the follow active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Receive notifications for activity on this account
    /// </summary>
    public bool NotifyOnActivity { get; set; } = true;
    
    /// <summary>
    /// Notification frequency (Immediate, Daily, Weekly)
    /// </summary>
    public NotificationFrequency NotificationFrequency { get; set; } = NotificationFrequency.Daily;
    
    /// <summary>
    /// Last time notifications were sent
    /// </summary>
    public DateTime? LastNotifiedAt { get; set; }
    
    /// <summary>
    /// Notes about why following this account
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation Properties
    public SocialMediaAccount? SocialMediaAccount { get; set; }
    public User? FollowedByUser { get; set; }
}

/// <summary>
/// Notification frequency for social media follows
/// </summary>
public enum NotificationFrequency
{
    Immediate = 0,
    Daily = 1,
    Weekly = 2,
    Never = 3
}
