namespace CRM.Core.Entities;

/// <summary>
/// Product entity for managing product information
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; } = 0;
    public decimal Cost { get; set; } = 0;
    public int Quantity { get; set; } = 0;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string ImageUrl { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<MarketingCampaign>? MarketingCampaigns { get; set; }
}
