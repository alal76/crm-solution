// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities
{
    public class CustomField : BaseEntity
    {
        public string? EntityType { get; set; }
        public int EntityId { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
    }
}
