namespace CRM.Core.Entities;

public class LookupCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<LookupItem> Items { get; set; } = new List<LookupItem>();
}
