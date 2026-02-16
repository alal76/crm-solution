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
