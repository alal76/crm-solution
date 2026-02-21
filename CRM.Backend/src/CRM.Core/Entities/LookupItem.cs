// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json.Serialization;

namespace CRM.Core.Entities;

public class LookupItem : BaseEntity
{
    public int LookupCategoryId { get; set; }

    [JsonIgnore]
    public LookupCategory? Category { get; set; }

    public string Key { get; set; } = string.Empty; // machine key
    public string Value { get; set; } = string.Empty; // display value
    public string? Meta { get; set; } // optional JSON/meta
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
