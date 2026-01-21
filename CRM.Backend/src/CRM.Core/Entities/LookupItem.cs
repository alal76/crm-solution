namespace CRM.Core.Entities;

public class LookupItem : BaseEntity
{
    public int LookupCategoryId { get; set; }
    public LookupCategory? Category { get; set; }

    public string Key { get; set; } = string.Empty; // machine key
    public string Value { get; set; } = string.Empty; // display value
    public string? Meta { get; set; } // optional JSON/meta
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
