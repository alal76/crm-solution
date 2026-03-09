// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
