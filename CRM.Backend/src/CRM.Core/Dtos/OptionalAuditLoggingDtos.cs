// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// DTO for audit log entry.
    /// </summary>
    public class AuditLogEntryDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for audit log filter.
    /// </summary>
    public class AuditLogFilterDto
    {
        public int? UserId { get; set; }
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? Action { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>
    /// Represents a single audit event to be enqueued to the Redis Stream (crm:audit:stream)
    /// and persisted to AuditLogs by AuditLogConsumerHostedService (FLAG-005).
    /// </summary>
    public class AuditEvent
    {
        /// <summary>User ID who performed the action. Null for system-generated events.</summary>
        public int? UserId { get; set; }

        /// <summary>Action type (Create, Update, Delete, etc.).</summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>Entity type affected (Account, Contact, etc.).</summary>
        public string? EntityType { get; set; }

        /// <summary>ID of the affected entity.</summary>
        public int? EntityId { get; set; }

        /// <summary>JSON-serialized old values (for update/delete operations).</summary>
        public string? OldValues { get; set; }

        /// <summary>JSON-serialized new values (for create/update operations).</summary>
        public string? NewValues { get; set; }

        /// <summary>Optional reason or context for the action.</summary>
        public string? Reason { get; set; }

        /// <summary>Client IP address.</summary>
        public string? IpAddress { get; set; }

        /// <summary>Client user-agent string.</summary>
        public string? UserAgent { get; set; }

        /// <summary>When the action occurred (UTC).</summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO for audit retention policy.
    /// </summary>
    public class AuditRetentionPolicyDto
    {
        /// <summary>
        /// Number of days to retain audit logs (0 = indefinite)
        /// </summary>
        public int RetentionDays { get; set; } = 365;

        /// <summary>
        /// Automatically archive logs older than this (0 = no archiving)
        /// </summary>
        public int ArchiveAfterDays { get; set; } = 90;

        /// <summary>
        /// Whether to compress archived logs
        /// </summary>
        public bool CompressArchives { get; set; } = true;

        /// <summary>
        /// Storage location for archives (S3, local disk, etc.)
        /// </summary>
        public string? ArchiveStorageLocation { get; set; }
    }
}
