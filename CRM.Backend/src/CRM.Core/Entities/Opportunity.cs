namespace CRM.Core.Entities;

/// <summary>
/// Opportunity entity for managing sales opportunities
/// </summary>
public class Opportunity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; } = 0;
    public int Stage { get; set; } = 0; // Prospecting, Qualification, Proposal, Negotiation, Closed Won/Lost
    public DateTime CloseDate { get; set; }
    public double Probability { get; set; } = 0; // 0-100
    public int CustomerId { get; set; }
    public int? AssignedToUserId { get; set; }
    public int? ProductId { get; set; }

    // Navigation properties
    public Customer? Customer { get; set; }
    public Product? Product { get; set; }
}
