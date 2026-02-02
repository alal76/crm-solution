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
