namespace CRM.Core.Entities;

/// <summary>
/// Customer entity for managing customer information
/// </summary>
public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int LifecycleStage { get; set; } = 0; // Lead, Customer, Inactive
    public decimal AnnualRevenue { get; set; } = 0;
    public string Notes { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<Interaction>? Interactions { get; set; }
}
