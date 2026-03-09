// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;

namespace CRM.Core.Entities;

/// <summary>
/// Quote template for standardized quote generation with predefined sections.
/// </summary>
public class QuoteTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? HeaderHtml { get; set; }
    public string? FooterHtml { get; set; }
    public string? BodyHtml { get; set; }
    public string? TermsAndConditions { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CssStyles { get; set; }
}
