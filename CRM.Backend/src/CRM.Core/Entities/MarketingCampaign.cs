namespace CRM.Core.Entities;

/// <summary>
/// Marketing Campaign entity for managing marketing campaigns
/// </summary>
public class MarketingCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Email, Social, Event, etc.
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Status { get; set; } = 0; // Planning, Active, Completed
    public decimal Budget { get; set; } = 0;
    public int TargetAudience { get; set; } = 0;
    public double ConversionRate { get; set; } = 0;

    // Navigation properties
    public ICollection<Product>? Products { get; set; }
    public ICollection<CampaignMetric>? Metrics { get; set; }
}
