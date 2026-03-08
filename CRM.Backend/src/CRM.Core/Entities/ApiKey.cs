using System;

namespace CRM.Core.Entities;

/// <summary>
/// API key entity for third-party integrations and programmatic access.
/// </summary>
public class ApiKey : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Scopes { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public virtual User? User { get; set; }
}
