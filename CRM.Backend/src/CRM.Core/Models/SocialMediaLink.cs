namespace CRM.Core.Models;

/// <summary>
/// Represents a social media link for a contact
/// </summary>
public class SocialMediaLink
{
    public int Id { get; set; }
    
    public int ContactId { get; set; }
    public Contact Contact { get; set; } = null!;
    
    /// <summary>
    /// Type of social media platform
    /// </summary>
    public SocialMediaPlatform Platform { get; set; }
    
    /// <summary>
    /// URL or handle for the social media profile
    /// </summary>
    public string Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Username or handle (if applicable)
    /// </summary>
    public string? Handle { get; set; }
    
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Social media platform enumeration
/// </summary>
public enum SocialMediaPlatform
{
    LinkedIn = 0,
    Twitter = 1,
    Facebook = 2,
    Instagram = 3,
    GitHub = 4,
    Website = 5,
    Other = 6
}
