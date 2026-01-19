namespace CRM.Core.Entities;

/// <summary>
/// Campaign metric entity for tracking campaign performance
/// </summary>
public class CampaignMetric : BaseEntity
{
    public int CampaignId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double MetricValue { get; set; } = 0;
    public DateTime RecordedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public MarketingCampaign? Campaign { get; set; }
}
