using System;

namespace CRM.Core.Entities;

/// <summary>
/// Customer segment for targeted marketing and analytics grouping.
/// </summary>
public class CustomerSegment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CriteriaJson { get; set; }
    public string? SegmentType { get; set; }
    public bool IsDynamic { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int MemberCount { get; set; }
    public DateTime? LastCalculatedAt { get; set; }
    public int? CreatedById { get; set; }
}
