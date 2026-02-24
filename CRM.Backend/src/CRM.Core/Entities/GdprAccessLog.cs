// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// GDPR Article 15 access log — tracks who accessed personal data, when, and for what purpose.
/// Supports the Right of Access and Right to Erasure under GDPR.
/// </summary>
public class GdprAccessLog : BaseEntity
{
    /// <summary>ID of the user who performed the access/action.</summary>
    public int RequestedByUserId { get; set; }

    /// <summary>Type of the data subject: "contact", "lead", "account".</summary>
    public string SubjectType { get; set; } = string.Empty;

    /// <summary>ID of the data subject record.</summary>
    public int SubjectId { get; set; }

    /// <summary>Action performed: "view", "export", "delete", "anonymize".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>IP address from which the action was performed.</summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>Optional notes or legal basis for the access.</summary>
    public string? Notes { get; set; }
}
