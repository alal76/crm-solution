using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// SQL Server specific database provider interface.
/// Extends the base IDatabaseProvider with SQL Server-specific operations.
/// </summary>
public interface ISqlServerProvider : IDatabaseProvider
{
    #region SQL Server Specific Features

    /// <summary>
    /// Gets detailed SQL Server information.
    /// </summary>
    Task<SqlServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the SQL Server edition.
    /// </summary>
    Task<string> GetEditionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the compatibility level of the database.
    /// </summary>
    Task<int> GetCompatibilityLevelAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the compatibility level of the database.
    /// </summary>
    Task SetCompatibilityLevelAsync(int level, CancellationToken cancellationToken = default);

    #endregion

    #region Performance and Monitoring

    /// <summary>
    /// Gets active sessions from sys.dm_exec_sessions.
    /// </summary>
    Task<IEnumerable<SqlServerSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets currently executing queries.
    /// </summary>
    Task<IEnumerable<SqlServerExecutingQuery>> GetExecutingQueriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wait statistics.
    /// </summary>
    Task<IEnumerable<SqlServerWaitStats>> GetWaitStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets index usage statistics.
    /// </summary>
    Task<IEnumerable<SqlServerIndexUsage>> GetIndexUsageStatsAsync(string? tableName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets missing index recommendations.
    /// </summary>
    Task<IEnumerable<SqlServerMissingIndex>> GetMissingIndexesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the estimated query execution plan.
    /// </summary>
    Task<string> GetExecutionPlanAsync(string sql, bool actual = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates statistics for a table.
    /// </summary>
    Task UpdateStatisticsAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rebuilds indexes for a table.
    /// </summary>
    Task RebuildIndexesAsync(string tableName, bool online = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reorganizes indexes for a table.
    /// </summary>
    Task ReorganizeIndexesAsync(string tableName, CancellationToken cancellationToken = default);

    #endregion

    #region Always On Availability Groups

    /// <summary>
    /// Checks if Always On is enabled.
    /// </summary>
    Task<bool> IsAlwaysOnEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets availability group status.
    /// </summary>
    Task<IEnumerable<SqlServerAvailabilityGroup>> GetAvailabilityGroupsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets availability replica status.
    /// </summary>
    Task<IEnumerable<SqlServerAvailabilityReplica>> GetAvailabilityReplicasAsync(string groupName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if this is the primary replica.
    /// </summary>
    Task<bool> IsPrimaryReplicaAsync(string groupName, CancellationToken cancellationToken = default);

    #endregion

    #region Security Features

    /// <summary>
    /// Gets database principals (users and roles).
    /// </summary>
    Task<IEnumerable<SqlServerPrincipal>> GetPrincipalsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets database permissions.
    /// </summary>
    Task<IEnumerable<SqlServerPermission>> GetPermissionsAsync(string? principalName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables row-level security on a table.
    /// </summary>
    Task EnableRowLevelSecurityAsync(string tableName, string policyName, string predicateFunction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables dynamic data masking on a column.
    /// </summary>
    Task EnableDataMaskingAsync(string tableName, string columnName, string maskingFunction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Transparent Data Encryption (TDE) status.
    /// </summary>
    Task<TdeStatus> GetTdeStatusAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Temporal Tables

    /// <summary>
    /// Checks if a table is a temporal table.
    /// </summary>
    Task<bool> IsTemporalTableAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries temporal table history.
    /// </summary>
    Task<IEnumerable<T>> QueryTemporalAsync<T>(string tableName, DateTime asOf, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Queries temporal table for a period.
    /// </summary>
    Task<IEnumerable<T>> QueryTemporalPeriodAsync<T>(string tableName, DateTime from, DateTime to, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region In-Memory OLTP

    /// <summary>
    /// Checks if In-Memory OLTP is enabled.
    /// </summary>
    Task<bool> IsInMemoryOltpEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets memory-optimized table information.
    /// </summary>
    Task<IEnumerable<SqlServerMemoryOptimizedTable>> GetMemoryOptimizedTablesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Backup and Restore (Extended)

    /// <summary>
    /// Gets backup history.
    /// </summary>
    Task<IEnumerable<SqlServerBackupInfo>> GetBackupHistoryAsync(int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a differential backup.
    /// </summary>
    Task<string> DifferentialBackupAsync(string backupPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a transaction log backup.
    /// </summary>
    Task<string> LogBackupAsync(string backupPath, CancellationToken cancellationToken = default);

    #endregion
}

#region SQL Server Specific Types

/// <summary>
/// SQL Server information.
/// </summary>
public record SqlServerInfo(
    string Version,
    string ProductLevel,
    string Edition,
    string MachineName,
    string InstanceName,
    int ProcessorCount,
    long PhysicalMemoryKb,
    DateTime ServerStartTime
);

/// <summary>
/// SQL Server session information.
/// </summary>
public record SqlServerSession(
    int SessionId,
    string LoginName,
    string HostName,
    string ProgramName,
    string Status,
    int? CpuTime,
    long? MemoryUsage,
    DateTime LoginTime
);

/// <summary>
/// SQL Server executing query information.
/// </summary>
public record SqlServerExecutingQuery(
    int SessionId,
    string Status,
    string Command,
    string QueryText,
    int WaitTime,
    string? WaitType,
    int PercentComplete,
    DateTime StartTime
);

/// <summary>
/// SQL Server wait statistics.
/// </summary>
public record SqlServerWaitStats(
    string WaitType,
    long WaitingTasksCount,
    long WaitTimeMs,
    long MaxWaitTimeMs,
    long SignalWaitTimeMs
);

/// <summary>
/// SQL Server index usage statistics.
/// </summary>
public record SqlServerIndexUsage(
    string TableName,
    string IndexName,
    string IndexType,
    long UserSeeks,
    long UserScans,
    long UserLookups,
    long UserUpdates,
    DateTime? LastUserSeek,
    DateTime? LastUserScan
);

/// <summary>
/// SQL Server missing index recommendation.
/// </summary>
public record SqlServerMissingIndex(
    string TableName,
    string EqualityColumns,
    string? InequalityColumns,
    string? IncludedColumns,
    double ImprovementMeasure,
    long UserSeeks
);

/// <summary>
/// SQL Server Availability Group information.
/// </summary>
public record SqlServerAvailabilityGroup(
    string GroupName,
    string PrimaryReplica,
    string SynchronizationHealth,
    string FailureConditionLevel
);

/// <summary>
/// SQL Server Availability Replica information.
/// </summary>
public record SqlServerAvailabilityReplica(
    string ReplicaName,
    string Role,
    string AvailabilityMode,
    string FailoverMode,
    string ConnectionState,
    string SynchronizationState
);

/// <summary>
/// SQL Server principal (user/role) information.
/// </summary>
public record SqlServerPrincipal(
    int PrincipalId,
    string Name,
    string Type,
    string? DefaultSchema,
    DateTime CreateDate
);

/// <summary>
/// SQL Server permission information.
/// </summary>
public record SqlServerPermission(
    string PrincipalName,
    string PermissionName,
    string StateDescription,
    string ObjectName,
    string ObjectType
);

/// <summary>
/// Transparent Data Encryption status.
/// </summary>
public record TdeStatus(
    bool IsEnabled,
    string EncryptionState,
    string? KeyAlgorithm,
    int? KeyLength
);

/// <summary>
/// SQL Server memory-optimized table information.
/// </summary>
public record SqlServerMemoryOptimizedTable(
    string TableName,
    string Durability,
    long RowCount,
    long MemoryAllocatedKb
);

/// <summary>
/// SQL Server backup information.
/// </summary>
public record SqlServerBackupInfo(
    string DatabaseName,
    string BackupType,
    DateTime BackupStartDate,
    DateTime BackupFinishDate,
    long BackupSizeBytes,
    string BackupPath,
    bool IsCompressed
);

#endregion
