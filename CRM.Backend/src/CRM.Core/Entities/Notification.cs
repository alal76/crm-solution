using System;

namespace CRM.Core.Entities;

/// <summary>
/// User notification entity for the in-app notification inbox.
/// </summary>
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? ActionUrl { get; set; }
    public virtual User? User { get; set; }
}
