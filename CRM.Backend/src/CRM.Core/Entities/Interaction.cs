namespace CRM.Core.Entities;

/// <summary>
/// Interaction entity for tracking customer communications
/// </summary>
public class Interaction : BaseEntity
{
    public int CustomerId { get; set; }
    public string Type { get; set; } = string.Empty; // Email, Phone, Meeting, Note
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime InteractionDate { get; set; }
    public int? AssignedToUserId { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
}
