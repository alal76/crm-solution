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

namespace CRM.Core.Entities
{
    /// <summary>
    /// Junction table for linking tags to entities (polymorphic tagging)
    /// Enables tagging of Accounts, Contacts, Leads, Opportunities, etc.
    /// </summary>
    public class EntityTag : BaseEntity
    {
        /// <summary>
        /// The type of entity being tagged (Account, Contact, Lead, Opportunity, etc.)
        /// </summary>
        public string EntityType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the entity being tagged
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Foreign key to the Tag
        /// </summary>
        public int TagId { get; set; }

        /// <summary>
        /// Denormalized tag name for quick display (optional, can be computed from navigation)
        /// </summary>
        public string? TagName { get; set; }

        /// <summary>
        /// Optional sort order for displaying tags on an entity
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Who added this tag
        /// </summary>
        public int? CreatedBy { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation to the Tag entity
        /// </summary>
        public virtual Tag? Tag { get; set; }
    }
}
