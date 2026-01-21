namespace CRM.Core.Entities;

public enum SocialNetwork
{
    Unknown = 0,
    LinkedIn = 1,
    Twitter = 2,
    Facebook = 3,
    Instagram = 4,
    YouTube = 5,
    Other = 99
}

public class SocialAccount : BaseEntity
{
    public SocialNetwork Network { get; set; } = SocialNetwork.Unknown;
    public string HandleOrUrl { get; set; } = string.Empty;
    public string? Label { get; set; }
    public bool IsPrimary { get; set; } = false;
    public string? Notes { get; set; }
}
