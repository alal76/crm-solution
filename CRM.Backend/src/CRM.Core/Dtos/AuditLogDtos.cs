using System;
using System.Collections.Generic;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// Data transfer object for individual audit log entry.
    /// </summary>
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? EntityType { get; set; }
        public int? EntityId { get; set; }
        public string? EntityName { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public Dictionary<string, object>? OldValues { get; set; }
        public Dictionary<string, object>? NewValues { get; set; }
        public List<string>? ChangedProperties { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Paginated collection of audit logs.
    /// </summary>
    public class AuditLogPageDto
    {
        public List<AuditLogDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }

    /// <summary>
    /// Audit statistics summary.
    /// </summary>
    public class AuditStatsDto
    {
        public int TotalActions { get; set; }
        public int CreatedCount { get; set; }
        public int UpdatedCount { get; set; }
        public int DeletedCount { get; set; }
        public int UniqueUsers { get; set; }
        public Dictionary<string, int> ActionsByType { get; set; } = new();
        public Dictionary<string, int> ActionsByEntity { get; set; } = new();
    }
}
