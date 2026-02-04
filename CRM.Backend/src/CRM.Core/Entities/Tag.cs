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

using System;
using System.Collections.Generic;

namespace CRM.Core.Entities
{
    /// <summary>
    /// Tag entity for categorizing and labeling entities
    /// </summary>
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional color for tag display (hex format)
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Optional description of the tag purpose
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property to entity links
        /// </summary>
        public virtual ICollection<EntityTag> EntityTags { get; set; } = new List<EntityTag>();
    }
}
