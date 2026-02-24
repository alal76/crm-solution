// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Contract version entity for tracking contract changes over time.
/// Provides audit trail and version history for contracts.
/// </summary>
public class ContractVersion : BaseEntity
{
    #region Relationships

    /// <summary>Contract ID this version belongs to</summary>
    public int ContractId { get; set; }

    /// <summary>Navigation to contract</summary>
    [ForeignKey("ContractId")]
    public Contract? Contract { get; set; }

    /// <summary>User who created this version</summary>
    public int CreatedById { get; set; }

    /// <summary>Navigation to creator user</summary>
    [ForeignKey("CreatedById")]
    public User? CreatedByUser { get; set; }

    #endregion

    #region Version Information

    /// <summary>Version number (1, 2, 3...)</summary>
    public int VersionNumber { get; set; }

    /// <summary>Version label (e.g., "v1.0", "Draft", "Final")</summary>
    public string? VersionLabel { get; set; }

    /// <summary>Description of changes in this version</summary>
    public string? ChangeDescription { get; set; }

    /// <summary>JSON serialized changes (field name, old value, new value)</summary>
    public string? ChangesJson { get; set; }

    /// <summary>Full contract snapshot as JSON</summary>
    public string? SnapshotJson { get; set; }

    #endregion

    #region Metadata

    /// <summary>Reason for creating this version</summary>
    public string? Reason { get; set; }

    /// <summary>Whether this version is active/current</summary>
    public bool IsCurrent { get; set; }

    /// <summary>Whether this version is archived</summary>
    public bool IsArchived { get; set; }

    /// <summary>Hash of the contract content for integrity verification</summary>
    public string? ContentHash { get; set; }

    #endregion
}

/// <summary>
/// Represents a single field change in a contract version.
/// </summary>
public class ContractFieldChange
{
    /// <summary>Name of the changed field</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Previous value (as string)</summary>
    public string? OldValue { get; set; }

    /// <summary>New value (as string)</summary>
    public string? NewValue { get; set; }

    /// <summary>Type of change (Added, Modified, Removed)</summary>
    public string ChangeType { get; set; } = "Modified";
}
