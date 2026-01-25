using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// MariaDB/MySQL specific database provider interface.
/// Extends the base IDatabaseProvider with MariaDB-specific operations.
/// </summary>
public interface IMariaDbProvider : IDatabaseProvider
{
    #region MariaDB Specific Features

    /// <summary>
    /// Gets the MariaDB server version details.
    /// </summary>
    Task<MariaDbVersionInfo> GetVersionInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the connection is using SSL/TLS.
    /// </summary>
    Task<bool> IsSecureConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current character set and collation.
    /// </summary>
    Task<(string CharacterSet, string Collation)> GetCharacterSetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the character set for the connection.
    /// </summary>
    Task SetCharacterSetAsync(string characterSet, string collation, CancellationToken cancellationToken = default);

    #endregion

    #region Performance and Optimization

    /// <summary>
    /// Gets the InnoDB status.
    /// </summary>
    Task<string> GetInnoDbStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a table for optimization.
    /// </summary>
    Task<TableAnalysisResult> AnalyzeTableAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Optimizes a table.
    /// </summary>
    Task<bool> OptimizeTableAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the explain plan for a query.
    /// </summary>
    Task<IEnumerable<ExplainPlanRow>> GetExplainPlanAsync(string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets slow query log entries.
    /// </summary>
    Task<IEnumerable<SlowQueryEntry>> GetSlowQueriesAsync(int limit = 100, CancellationToken cancellationToken = default);

    #endregion

    #region Replication Support

    /// <summary>
    /// Gets the replication status.
    /// </summary>
    Task<ReplicationStatus?> GetReplicationStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if this server is a replica.
    /// </summary>
    Task<bool> IsReplicaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the binary log position.
    /// </summary>
    Task<BinLogPosition?> GetBinLogPositionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region User and Permission Management

    /// <summary>
    /// Gets a list of database users.
    /// </summary>
    Task<IEnumerable<DatabaseUser>> GetUsersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants permissions to a user.
    /// </summary>
    Task GrantPermissionsAsync(string username, string database, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes permissions from a user.
    /// </summary>
    Task RevokePermissionsAsync(string username, string database, IEnumerable<string> permissions, CancellationToken cancellationToken = default);

    #endregion

    #region Galera Cluster Support (MariaDB Specific)

    /// <summary>
    /// Checks if Galera cluster is enabled.
    /// </summary>
    Task<bool> IsGaleraEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Galera cluster status.
    /// </summary>
    Task<GaleraClusterStatus?> GetGaleraStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the number of nodes in the Galera cluster.
    /// </summary>
    Task<int> GetGaleraNodeCountAsync(CancellationToken cancellationToken = default);

    #endregion
}

#region MariaDB Specific Types

/// <summary>
/// MariaDB version information.
/// </summary>
public record MariaDbVersionInfo(
    string Version,
    int MajorVersion,
    int MinorVersion,
    int PatchVersion,
    string Edition,
    bool IsMariaDb
);

/// <summary>
/// Result of table analysis.
/// </summary>
public record TableAnalysisResult(
    string TableName,
    string Status,
    long RowCount,
    long DataLength,
    long IndexLength,
    DateTime? LastAnalyzed
);

/// <summary>
/// Row in an EXPLAIN plan.
/// </summary>
public record ExplainPlanRow(
    int Id,
    string SelectType,
    string? Table,
    string? Type,
    string? PossibleKeys,
    string? Key,
    int? KeyLen,
    string? Ref,
    long? Rows,
    string? Extra
);

/// <summary>
/// Slow query log entry.
/// </summary>
public record SlowQueryEntry(
    DateTime StartTime,
    TimeSpan QueryTime,
    TimeSpan LockTime,
    long RowsSent,
    long RowsExamined,
    string SqlText
);

/// <summary>
/// Replication status.
/// </summary>
public record ReplicationStatus(
    string MasterHost,
    int MasterPort,
    string ReplicaIoRunning,
    string ReplicaSqlRunning,
    long SecondsBehindMaster,
    string? LastError
);

/// <summary>
/// Binary log position.
/// </summary>
public record BinLogPosition(
    string FileName,
    long Position
);

/// <summary>
/// Database user information.
/// </summary>
public record DatabaseUser(
    string Username,
    string Host,
    bool IsLocked,
    DateTime? PasswordExpiry
);

/// <summary>
/// Galera cluster status.
/// </summary>
public record GaleraClusterStatus(
    string ClusterState,
    string ClusterStatus,
    int ClusterSize,
    string LocalState,
    bool IsReady,
    long ReplicationLatency
);

#endregion
