// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
