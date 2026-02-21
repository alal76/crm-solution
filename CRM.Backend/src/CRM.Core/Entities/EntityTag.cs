// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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
