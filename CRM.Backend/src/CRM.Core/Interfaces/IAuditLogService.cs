using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces
{
    /// <summary>
    /// Interface for audit logging service.
    /// Tracks all changes to entities for compliance and troubleshooting.
    /// Implements comprehensive audit trail with user attribution, entity tracking,
    /// and change history capabilities.
    /// </summary>
    public interface IAuditLogService
    {
        #region Audit Log Creation

        /// <summary>
        /// Log an entity creation action.
        /// </summary>
        /// <param name="entityType">Type of entity created (e.g., "Account", "Contact")</param>
        /// <param name="entityId">ID of created entity</param>
        /// <param name="entityName">Display name of entity</param>
        /// <param name="userId">ID of user who performed action</param>
        /// <param name="newValues">New property values</param>
        /// <param name="ipAddress">IP address of user</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID of created audit log entry</returns>
        Task<int> LogCreateAsync(
            string entityType,
            int entityId,
            string entityName,
            int? userId,
            Dictionary<string, object> newValues,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Log an entity update action.
        /// </summary>
        /// <param name="entityType">Type of entity updated</param>
        /// <param name="entityId">ID of updated entity</param>
        /// <param name="entityName">Display name of entity</param>
        /// <param name="userId">ID of user who performed action</param>
        /// <param name="oldValues">Previous property values</param>
        /// <param name="newValues">New property values</param>
        /// <param name="changedProperties">List of property names that changed</param>
        /// <param name="ipAddress">IP address of user</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID of created audit log entry</returns>
        Task<int> LogUpdateAsync(
            string entityType,
            int entityId,
            string entityName,
            int? userId,
            Dictionary<string, object> oldValues,
            Dictionary<string, object> newValues,
            List<string> changedProperties,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Log an entity deletion action.
        /// </summary>
        /// <param name="entityType">Type of entity deleted</param>
        /// <param name="entityId">ID of deleted entity</param>
        /// <param name="entityName">Display name of entity</param>
        /// <param name="userId">ID of user who performed action</param>
        /// <param name="oldValues">Deleted property values</param>
        /// <param name="ipAddress">IP address of user</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID of created audit log entry</returns>
        Task<int> LogDeleteAsync(
            string entityType,
            int entityId,
            string entityName,
            int? userId,
            Dictionary<string, object> oldValues,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Log a custom action.
        /// </summary>
        /// <param name="action">Action description</param>
        /// <param name="entityType">Entity type (optional)</param>
        /// <param name="entityId">Entity ID (optional)</param>
        /// <param name="userId">ID of user who performed action</param>
        /// <param name="details">Additional details as JSON</param>
        /// <param name="ipAddress">IP address of user</param>
        /// <param name="userAgent">User agent string</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ID of created audit log entry</returns>
        Task<int> LogActionAsync(
            string action,
            string? entityType = null,
            int? entityId = null,
            int? userId = null,
            string? details = null,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Query Audit Logs

        /// <summary>
        /// Get all audit logs with optional filtering.
        /// </summary>
        /// <param name="entityType">Filter by entity type (optional)</param>
        /// <param name="entityId">Filter by entity ID (optional)</param>
        /// <param name="userId">Filter by user ID (optional)</param>
        /// <param name="action">Filter by action type (optional)</param>
        /// <param name="fromDate">Filter by start date (optional)</param>
        /// <param name="toDate">Filter by end date (optional)</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated collection of audit logs</returns>
        Task<AuditLogPageDto> GetAuditLogsAsync(
            string? entityType = null,
            int? entityId = null,
            int? userId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get audit history for a specific entity.
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <param name="entityId">ID of entity</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Collection of audit logs for entity</returns>
        Task<IEnumerable<AuditLogDto>> GetEntityHistoryAsync(
            string entityType,
            int entityId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get user activity logs.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="fromDate">Filter by start date (optional)</param>
        /// <param name="toDate">Filter by end date (optional)</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated user activity logs</returns>
        Task<AuditLogPageDto> GetUserActivityAsync(
            int userId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search audit logs by free-text query.
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="pageNumber">Page number for pagination</param>
        /// <param name="pageSize">Page size for pagination</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated search results</returns>
        Task<AuditLogPageDto> SearchAsync(
            string query,
            int pageNumber = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default);

        #endregion

        #region Statistics & Reports

        /// <summary>
        /// Get audit statistics for a date range.
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Audit statistics</returns>
        Task<AuditStatsDto> GetStatisticsAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get entity change history with comparison.
        /// </summary>
        /// <param name="entityType">Type of entity</param>
        /// <param name="entityId">ID of entity</param>
        /// <param name="fromDate">Start date (optional)</param>
        /// <param name="toDate">End date (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Detailed change history with before/after values</returns>
        Task<EntityChangeHistoryDto> GetEntityChangeHistoryAsync(
            string entityType,
            int entityId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Export audit logs to CSV format.
        /// </summary>
        /// <param name="entityType">Filter by entity type (optional)</param>
        /// <param name="fromDate">Filter by start date (optional)</param>
        /// <param name="toDate">Filter by end date (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>CSV content as byte array</returns>
        Task<byte[]> ExportToCsvAsync(
            string? entityType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default);

        #endregion

        #region Cleanup & Maintenance

        /// <summary>
        /// Delete old audit logs (archival).
        /// </summary>
        /// <param name="beforeDate">Delete logs before this date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of deleted entries</returns>
        Task<int> DeleteOldLogsAsync(
            DateTime beforeDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Archive audit logs to separate table.
        /// </summary>
        /// <param name="beforeDate">Archive logs before this date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of archived entries</returns>
        Task<int> ArchiveLogsAsync(
            DateTime beforeDate,
            CancellationToken cancellationToken = default);

        #endregion
    }

    /// <summary>
    /// Entity change history with before/after comparison.
    /// </summary>
    public class EntityChangeHistoryDto
    {
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string? EntityName { get; set; }
        public List<ChangeEntry> Changes { get; set; } = new();
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    /// <summary>
    /// Individual change entry in entity history.
    /// </summary>
    public class ChangeEntry
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string PropertyName { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }
}
