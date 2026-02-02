// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
