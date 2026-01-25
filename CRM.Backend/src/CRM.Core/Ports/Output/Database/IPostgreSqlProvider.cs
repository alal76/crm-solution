using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// PostgreSQL specific database provider interface.
/// Extends the base IDatabaseProvider with PostgreSQL-specific operations.
/// </summary>
public interface IPostgreSqlProvider : IDatabaseProvider
{
    #region PostgreSQL Specific Features

    /// <summary>
    /// Gets detailed PostgreSQL server information.
    /// </summary>
    Task<PostgreSqlServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current schema search path.
    /// </summary>
    Task<IEnumerable<string>> GetSearchPathAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the schema search path.
    /// </summary>
    Task SetSearchPathAsync(IEnumerable<string> schemas, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all schemas in the database.
    /// </summary>
    Task<IEnumerable<string>> GetSchemasAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new schema.
    /// </summary>
    Task CreateSchemaAsync(string schemaName, CancellationToken cancellationToken = default);

    #endregion

    #region Extensions Management

    /// <summary>
    /// Gets a list of installed extensions.
    /// </summary>
    Task<IEnumerable<PostgreSqlExtension>> GetInstalledExtensionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of available extensions.
    /// </summary>
    Task<IEnumerable<string>> GetAvailableExtensionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Installs an extension.
    /// </summary>
    Task InstallExtensionAsync(string extensionName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an extension is installed.
    /// </summary>
    Task<bool> IsExtensionInstalledAsync(string extensionName, CancellationToken cancellationToken = default);

    #endregion

    #region Performance and Monitoring

    /// <summary>
    /// Gets active connections and their status.
    /// </summary>
    Task<IEnumerable<PostgreSqlConnection>> GetActiveConnectionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets table statistics.
    /// </summary>
    Task<PostgreSqlTableStats> GetTableStatsAsync(string tableName, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets index statistics.
    /// </summary>
    Task<IEnumerable<PostgreSqlIndexStats>> GetIndexStatsAsync(string tableName, string? schema = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs VACUUM on a table.
    /// </summary>
    Task VacuumAsync(string? tableName = null, bool full = false, bool analyze = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs ANALYZE on a table.
    /// </summary>
    Task AnalyzeAsync(string? tableName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the query execution plan.
    /// </summary>
    Task<PostgreSqlExplainResult> ExplainAsync(string sql, bool analyze = false, bool buffers = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets slow queries from pg_stat_statements.
    /// </summary>
    Task<IEnumerable<PostgreSqlSlowQuery>> GetSlowQueriesAsync(int limit = 100, CancellationToken cancellationToken = default);

    #endregion

    #region Replication and High Availability

    /// <summary>
    /// Gets replication status.
    /// </summary>
    Task<PostgreSqlReplicationInfo?> GetReplicationInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets replication slots.
    /// </summary>
    Task<IEnumerable<ReplicationSlot>> GetReplicationSlotsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the server is in recovery mode (standby).
    /// </summary>
    Task<bool> IsInRecoveryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current WAL position.
    /// </summary>
    Task<string> GetCurrentWalPositionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region JSON and JSONB Support

    /// <summary>
    /// Executes a JSON path query.
    /// </summary>
    Task<IEnumerable<T>> JsonPathQueryAsync<T>(string tableName, string jsonColumn, string jsonPath, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Creates a GIN index for JSONB column.
    /// </summary>
    Task CreateJsonbIndexAsync(string tableName, string columnName, string indexName, CancellationToken cancellationToken = default);

    #endregion

    #region Full-Text Search

    /// <summary>
    /// Creates a full-text search configuration.
    /// </summary>
    Task CreateTextSearchConfigAsync(string configName, string language = "english", CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a full-text search.
    /// </summary>
    Task<IEnumerable<T>> FullTextSearchAsync<T>(string tableName, string[] columns, string searchQuery, string language = "english", CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Partitioning

    /// <summary>
    /// Gets partition information for a table.
    /// </summary>
    Task<IEnumerable<PostgreSqlPartition>> GetPartitionsAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new partition for a partitioned table.
    /// </summary>
    Task CreatePartitionAsync(string parentTable, string partitionName, string partitionBound, CancellationToken cancellationToken = default);

    #endregion
}

#region PostgreSQL Specific Types

/// <summary>
/// PostgreSQL server information.
/// </summary>
public record PostgreSqlServerInfo(
    string Version,
    int MajorVersion,
    int MinorVersion,
    string DataDirectory,
    string ConfigFile,
    DateTime ServerStartTime,
    int MaxConnections,
    long SharedBuffers
);

/// <summary>
/// PostgreSQL extension information.
/// </summary>
public record PostgreSqlExtension(
    string Name,
    string Version,
    string Schema,
    string Description
);

/// <summary>
/// PostgreSQL active connection information.
/// </summary>
public record PostgreSqlConnection(
    int Pid,
    string Database,
    string Username,
    string ClientAddr,
    DateTime? BackendStart,
    DateTime? QueryStart,
    string State,
    string? Query
);

/// <summary>
/// PostgreSQL table statistics.
/// </summary>
public record PostgreSqlTableStats(
    string TableName,
    string Schema,
    long LiveTuples,
    long DeadTuples,
    long TuplesInserted,
    long TuplesUpdated,
    long TuplesDeleted,
    DateTime? LastVacuum,
    DateTime? LastAnalyze,
    DateTime? LastAutoVacuum,
    DateTime? LastAutoAnalyze
);

/// <summary>
/// PostgreSQL index statistics.
/// </summary>
public record PostgreSqlIndexStats(
    string IndexName,
    string TableName,
    long IndexScans,
    long TuplesRead,
    long TuplesFetched,
    long IndexSizeBytes
);

/// <summary>
/// PostgreSQL EXPLAIN result.
/// </summary>
public record PostgreSqlExplainResult(
    string PlanType,
    double TotalCost,
    double StartupCost,
    long PlanRows,
    int PlanWidth,
    double? ActualTime,
    long? ActualRows,
    string? JoinType,
    string RawPlan
);

/// <summary>
/// PostgreSQL slow query information.
/// </summary>
public record PostgreSqlSlowQuery(
    string QueryId,
    string Query,
    long Calls,
    double TotalTime,
    double MeanTime,
    long Rows,
    long SharedBlocksHit,
    long SharedBlocksRead
);

/// <summary>
/// PostgreSQL replication information.
/// </summary>
public record PostgreSqlReplicationInfo(
    bool IsReplica,
    string? PrimaryConnInfo,
    DateTime? LastReceiveTime,
    DateTime? LastReplayTime,
    string? ReplayLag
);

/// <summary>
/// PostgreSQL replication slot.
/// </summary>
public record ReplicationSlot(
    string SlotName,
    string SlotType,
    string Database,
    bool Active,
    string RestartLsn
);

/// <summary>
/// PostgreSQL partition information.
/// </summary>
public record PostgreSqlPartition(
    string PartitionName,
    string ParentTable,
    string PartitionBound,
    long EstimatedRows,
    long SizeBytes
);

#endregion
