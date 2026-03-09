using System;

namespace CRM.Core.Entities;

/// <summary>
/// Saved list/grid view per entity type for user customization.
/// </summary>
public class SavedView : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public bool IsDefault { get; set; }
    public bool IsShared { get; set; }
    public string? ColumnsJson { get; set; }
    public string? FiltersJson { get; set; }
    public string? SortJson { get; set; }
    public int? PageSize { get; set; }
    public virtual User? User { get; set; }
}
