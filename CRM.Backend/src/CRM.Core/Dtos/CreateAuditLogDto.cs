namespace CRM.Core.Dtos;

/// <summary>
/// DTO for creating an audit log entry via the API.
/// </summary>
public class CreateAuditLogDto
{
    /// <summary>Action performed (e.g., Login, Create, Update, Delete, Close).</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Type of entity affected.</summary>
    public string? EntityType { get; set; }

    /// <summary>ID of the affected entity.</summary>
    public int? EntityId { get; set; }

    /// <summary>ID of the user who performed the action.</summary>
    public int? UserId { get; set; }

    /// <summary>Additional details or context for the action.</summary>
    public string? Details { get; set; }

    /// <summary>Timestamp of the action (defaults to now).</summary>
    public DateTime? Timestamp { get; set; }
}
