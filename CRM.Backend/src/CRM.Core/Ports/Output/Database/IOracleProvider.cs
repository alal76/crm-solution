using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// Oracle database specific provider interface.
/// Extends the base IDatabaseProvider with Oracle-specific operations.
/// </summary>
public interface IOracleProvider : IDatabaseProvider
{
    #region Oracle Specific Features

    /// <summary>
    /// Gets detailed Oracle database information.
    /// </summary>
    Task<OracleInstanceInfo> GetInstanceInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the Oracle database version.
    /// </summary>
    Task<string> GetBannerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current schema/user.
    /// </summary>
    Task<string> GetCurrentSchemaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the current schema.
    /// </summary>
    Task SetCurrentSchemaAsync(string schemaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all tablespaces.
    /// </summary>
    Task<IEnumerable<OracleTablespace>> GetTablespacesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Performance and Monitoring

    /// <summary>
    /// Gets active sessions from V$SESSION.
    /// </summary>
    Task<IEnumerable<OracleSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets SQL statements from V$SQL.
    /// </summary>
    Task<IEnumerable<OracleSqlStatement>> GetTopSqlAsync(int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wait events statistics.
    /// </summary>
    Task<IEnumerable<OracleWaitEvent>> GetWaitEventsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the explain plan for a SQL statement.
    /// </summary>
    Task<string> GetExplainPlanAsync(string sql, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gathers statistics for a table.
    /// </summary>
    Task GatherTableStatsAsync(string tableName, string? owner = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gathers statistics for a schema.
    /// </summary>
    Task GatherSchemaStatsAsync(string schemaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets AWR (Automatic Workload Repository) snapshots.
    /// </summary>
    Task<IEnumerable<AwrSnapshot>> GetAwrSnapshotsAsync(int limit = 24, CancellationToken cancellationToken = default);

    #endregion

    #region PL/SQL Support

    /// <summary>
    /// Executes a PL/SQL block.
    /// </summary>
    Task ExecutePlSqlAsync(string plsqlBlock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls a stored procedure.
    /// </summary>
    Task<T?> CallProcedureAsync<T>(string procedureName, object? parameters = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Calls a stored function.
    /// </summary>
    Task<T?> CallFunctionAsync<T>(string functionName, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets PL/SQL objects (procedures, functions, packages).
    /// </summary>
    Task<IEnumerable<OraclePlSqlObject>> GetPlSqlObjectsAsync(string? owner = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the source code of a PL/SQL object.
    /// </summary>
    Task<string> GetPlSqlSourceAsync(string objectName, string objectType, string? owner = null, CancellationToken cancellationToken = default);

    #endregion

    #region Data Guard and RAC

    /// <summary>
    /// Gets Data Guard configuration status.
    /// </summary>
    Task<OracleDataGuardInfo?> GetDataGuardInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if this is a standby database.
    /// </summary>
    Task<bool> IsStandbyDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets RAC (Real Application Clusters) instance information.
    /// </summary>
    Task<IEnumerable<OracleRacInstance>> GetRacInstancesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if RAC is enabled.
    /// </summary>
    Task<bool> IsRacEnabledAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Partitioning

    /// <summary>
    /// Gets partition information for a table.
    /// </summary>
    Task<IEnumerable<OraclePartition>> GetTablePartitionsAsync(string tableName, string? owner = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new partition to a table.
    /// </summary>
    Task AddPartitionAsync(string tableName, string partitionName, string highValue, string? tablespace = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops a partition from a table.
    /// </summary>
    Task DropPartitionAsync(string tableName, string partitionName, CancellationToken cancellationToken = default);

    #endregion

    #region Flashback and Recovery

    /// <summary>
    /// Performs a flashback query.
    /// </summary>
    Task<IEnumerable<T>> FlashbackQueryAsync<T>(string tableName, DateTime asOf, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets flashback database status.
    /// </summary>
    Task<FlashbackStatus> GetFlashbackStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets restore points.
    /// </summary>
    Task<IEnumerable<OracleRestorePoint>> GetRestorePointsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a restore point.
    /// </summary>
    Task CreateRestorePointAsync(string restorePointName, bool guarantee = false, CancellationToken cancellationToken = default);

    #endregion

    #region Advanced Security

    /// <summary>
    /// Gets TDE (Transparent Data Encryption) status.
    /// </summary>
    Task<OracleTdeStatus> GetTdeStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets database vault status.
    /// </summary>
    Task<bool> IsDatabaseVaultEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit policies.
    /// </summary>
    Task<IEnumerable<OracleAuditPolicy>> GetAuditPoliciesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Multitenant (CDB/PDB)

    /// <summary>
    /// Checks if this is a container database.
    /// </summary>
    Task<bool> IsContainerDatabaseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pluggable databases.
    /// </summary>
    Task<IEnumerable<OraclePluggableDb>> GetPluggableDatabasesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches to a pluggable database.
    /// </summary>
    Task SwitchToPluggableDbAsync(string pdbName, CancellationToken cancellationToken = default);

    #endregion
}

#region Oracle Specific Types

/// <summary>
/// Oracle instance information.
/// </summary>
public record OracleInstanceInfo(
    string InstanceName,
    string HostName,
    string Version,
    string Status,
    DateTime StartupTime,
    string InstanceRole,
    bool IsRac,
    bool IsStandby
);

/// <summary>
/// Oracle tablespace information.
/// </summary>
public record OracleTablespace(
    string TablespaceName,
    string Status,
    string Contents,
    long SizeBytes,
    long FreeBytes,
    int DatafileCount,
    bool IsBigfile
);

/// <summary>
/// Oracle session information.
/// </summary>
public record OracleSession(
    int Sid,
    int Serial,
    string Username,
    string Status,
    string Machine,
    string Program,
    string? SqlId,
    DateTime? LogonTime,
    string? WaitClass
);

/// <summary>
/// Oracle SQL statement from V$SQL.
/// </summary>
public record OracleSqlStatement(
    string SqlId,
    string SqlText,
    long Executions,
    double ElapsedTime,
    double CpuTime,
    long BufferGets,
    long DiskReads,
    DateTime FirstLoadTime
);

/// <summary>
/// Oracle wait event statistics.
/// </summary>
public record OracleWaitEvent(
    string WaitClass,
    string EventName,
    long TotalWaits,
    double TimeWaitedSecs,
    double AverageWaitMs
);

/// <summary>
/// AWR snapshot information.
/// </summary>
public record AwrSnapshot(
    long SnapId,
    DateTime BeginTime,
    DateTime EndTime,
    string SnapshotLevel
);

/// <summary>
/// Oracle PL/SQL object information.
/// </summary>
public record OraclePlSqlObject(
    string ObjectName,
    string ObjectType,
    string Owner,
    string Status,
    DateTime Created,
    DateTime LastDdlTime
);

/// <summary>
/// Oracle Data Guard information.
/// </summary>
public record OracleDataGuardInfo(
    string DatabaseRole,
    string ProtectionMode,
    string ProtectionLevel,
    string SwitchoverStatus,
    bool IsStandby
);

/// <summary>
/// Oracle RAC instance information.
/// </summary>
public record OracleRacInstance(
    int InstanceNumber,
    string InstanceName,
    string HostName,
    string Status,
    DateTime StartupTime
);

/// <summary>
/// Oracle table partition information.
/// </summary>
public record OraclePartition(
    string PartitionName,
    string HighValue,
    int PartitionPosition,
    string TablespaceName,
    long NumRows,
    long Blocks
);

/// <summary>
/// Oracle flashback status.
/// </summary>
public record FlashbackStatus(
    bool IsEnabled,
    int RetentionDays,
    long FlashbackSizeBytes,
    DateTime? OldestFlashbackTime
);

/// <summary>
/// Oracle restore point.
/// </summary>
public record OracleRestorePoint(
    string RestorePointName,
    DateTime Time,
    long Scn,
    bool IsGuaranteed,
    string StorageSize
);

/// <summary>
/// Oracle TDE status.
/// </summary>
public record OracleTdeStatus(
    bool IsEnabled,
    string WalletStatus,
    string EncryptionKeyId,
    string KeystoreMode
);

/// <summary>
/// Oracle audit policy.
/// </summary>
public record OracleAuditPolicy(
    string PolicyName,
    bool IsEnabled,
    string AuditCondition,
    string AuditOption
);

/// <summary>
/// Oracle pluggable database information.
/// </summary>
public record OraclePluggableDb(
    int ConId,
    string PdbName,
    string Status,
    string OpenMode,
    DateTime CreationTime,
    long TotalSizeBytes
);

#endregion
