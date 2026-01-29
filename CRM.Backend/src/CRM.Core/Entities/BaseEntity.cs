using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Base entity class for all domain entities
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Used to detect concurrent updates to the same record.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
