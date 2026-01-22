using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.WorkflowEngine;

/// <summary>
/// API credentials storage for workflow API call steps
/// </summary>
public class WorkflowApiCredential : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Authentication type: None, ApiKey, Basic, Bearer, OAuth2
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string AuthenticationType { get; set; } = "None";
    
    /// <summary>
    /// Encrypted credential data JSON
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? CredentialData { get; set; }
    
    /// <summary>
    /// Base URL for API (optional)
    /// </summary>
    [MaxLength(500)]
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Default headers JSON
    /// </summary>
    [Column(TypeName = "longtext")]
    public string? DefaultHeaders { get; set; }
    
    /// <summary>
    /// Whether credential is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// User who created this credential
    /// </summary>
    public int? CreatedByUserId { get; set; }
    
    /// <summary>
    /// Last time credential was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Expiration date for credential
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
}

/// <summary>
/// Authentication type constants
/// </summary>
public static class ApiAuthenticationTypes
{
    public const string None = "None";
    public const string ApiKey = "ApiKey";
    public const string Basic = "Basic";
    public const string Bearer = "Bearer";
    public const string OAuth2 = "OAuth2";
    public const string Custom = "Custom";
}
