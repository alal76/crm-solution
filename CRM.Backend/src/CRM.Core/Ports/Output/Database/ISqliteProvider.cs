using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// SQLite database specific provider interface.
/// Extends the base IDatabaseProvider with SQLite-specific operations.
/// </summary>
public interface ISqliteProvider : IDatabaseProvider
{
    #region SQLite Specific Features

    /// <summary>
    /// Gets the SQLite library version.
    /// </summary>
    Task<string> GetSqliteVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database file path.
    /// </summary>
    string DatabaseFilePath { get; }

    /// <summary>
    /// Gets the database file size in bytes.
    /// </summary>
    Task<long> GetDatabaseSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the database is encrypted.
    /// </summary>
    Task<bool> IsEncryptedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current journal mode.
    /// </summary>
    Task<string> GetJournalModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the journal mode (DELETE, TRUNCATE, PERSIST, MEMORY, WAL, OFF).
    /// </summary>
    Task<string> SetJournalModeAsync(SqliteJournalMode mode, CancellationToken cancellationToken = default);

    #endregion

    #region Pragmas and Configuration

    /// <summary>
    /// Gets a PRAGMA value.
    /// </summary>
    Task<T?> GetPragmaAsync<T>(string pragmaName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a PRAGMA value.
    /// </summary>
    Task SetPragmaAsync(string pragmaName, object value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the synchronous mode.
    /// </summary>
    Task<SqliteSynchronousMode> GetSynchronousModeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the synchronous mode.
    /// </summary>
    Task SetSynchronousModeAsync(SqliteSynchronousMode mode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the cache size (in pages or KB).
    /// </summary>
    Task<int> GetCacheSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the cache size.
    /// </summary>
    Task SetCacheSizeAsync(int sizeInPages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the page size.
    /// </summary>
    Task<int> GetPageSizeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets foreign keys enforcement status.
    /// </summary>
    Task<bool> GetForeignKeysEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables foreign key enforcement.
    /// </summary>
    Task SetForeignKeysEnabledAsync(bool enabled, CancellationToken cancellationToken = default);

    #endregion

    #region Database Maintenance

    /// <summary>
    /// Runs VACUUM to rebuild the database file.
    /// </summary>
    Task VacuumAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs VACUUM INTO to create a vacuumed copy.
    /// </summary>
    Task VacuumIntoAsync(string targetFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs integrity check.
    /// </summary>
    Task<IEnumerable<string>> IntegrityCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs quick check (faster but less thorough).
    /// </summary>
    Task<IEnumerable<string>> QuickCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs ANALYZE to update statistics.
    /// </summary>
    Task AnalyzeAsync(string? tableName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs REINDEX to rebuild indexes.
    /// </summary>
    Task ReindexAsync(string? indexOrTableName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checkpoints the WAL file (when using WAL mode).
    /// </summary>
    Task<WalCheckpointResult> WalCheckpointAsync(SqliteCheckpointMode mode = SqliteCheckpointMode.Passive, CancellationToken cancellationToken = default);

    #endregion

    #region Database Information

    /// <summary>
    /// Gets database statistics.
    /// </summary>
    Task<SqliteDatabaseStats> GetDatabaseStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets table information.
    /// </summary>
    Task<SqliteTableInfo> GetTableInfoAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets index information for a table.
    /// </summary>
    Task<IEnumerable<SqliteIndexInfo>> GetIndexInfoAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the explain query plan.
    /// </summary>
    Task<IEnumerable<SqliteQueryPlanRow>> ExplainQueryPlanAsync(string sql, CancellationToken cancellationToken = default);

    #endregion

    #region Attached Databases

    /// <summary>
    /// Attaches another database file.
    /// </summary>
    Task AttachDatabaseAsync(string filePath, string alias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Detaches an attached database.
    /// </summary>
    Task DetachDatabaseAsync(string alias, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of attached databases.
    /// </summary>
    Task<IEnumerable<SqliteAttachedDatabase>> GetAttachedDatabasesAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Encryption (SQLCipher)

    /// <summary>
    /// Sets the encryption key (for SQLCipher).
    /// </summary>
    Task SetEncryptionKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the encryption key (for SQLCipher).
    /// </summary>
    Task ChangeEncryptionKeyAsync(string newKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes encryption from the database.
    /// </summary>
    Task RemoveEncryptionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region User-Defined Functions

    /// <summary>
    /// Registers a custom scalar function.
    /// </summary>
    void RegisterScalarFunction(string name, int argCount, Func<object[], object> function);

    /// <summary>
    /// Registers a custom aggregate function.
    /// </summary>
    void RegisterAggregateFunction<TState>(string name, Func<TState> seed, Func<TState, object[], TState> step, Func<TState, object> final);

    /// <summary>
    /// Registers a custom collation.
    /// </summary>
    void RegisterCollation(string name, Comparison<string> comparison);

    #endregion
}

#region SQLite Specific Types

/// <summary>
/// SQLite journal mode options.
/// </summary>
public enum SqliteJournalMode
{
    Delete,
    Truncate,
    Persist,
    Memory,
    Wal,
    Off
}

/// <summary>
/// SQLite synchronous mode options.
/// </summary>
public enum SqliteSynchronousMode
{
    Off = 0,
    Normal = 1,
    Full = 2,
    Extra = 3
}

/// <summary>
/// SQLite WAL checkpoint mode options.
/// </summary>
public enum SqliteCheckpointMode
{
    Passive,
    Full,
    Restart,
    Truncate
}

/// <summary>
/// Result of a WAL checkpoint operation.
/// </summary>
public record WalCheckpointResult(
    bool Success,
    int TotalFrames,
    int CheckpointedFrames
);

/// <summary>
/// SQLite database statistics.
/// </summary>
public record SqliteDatabaseStats(
    int PageCount,
    int PageSize,
    long DatabaseSizeBytes,
    int FreePageCount,
    long FreeSizeBytes,
    string Encoding,
    string JournalMode,
    bool WalEnabled
);

/// <summary>
/// SQLite table information.
/// </summary>
public record SqliteTableInfo(
    string TableName,
    string Type,
    bool WithoutRowid,
    long RowCount,
    IEnumerable<SqliteColumnInfo> Columns
);

/// <summary>
/// SQLite column information.
/// </summary>
public record SqliteColumnInfo(
    int Cid,
    string Name,
    string Type,
    bool NotNull,
    string? DefaultValue,
    bool IsPrimaryKey
);

/// <summary>
/// SQLite index information.
/// </summary>
public record SqliteIndexInfo(
    string IndexName,
    bool IsUnique,
    bool IsPartial,
    string Origin,
    IEnumerable<string> Columns
);

/// <summary>
/// SQLite query plan row.
/// </summary>
public record SqliteQueryPlanRow(
    int Id,
    int Parent,
    int NotUsed,
    string Detail
);

/// <summary>
/// SQLite attached database information.
/// </summary>
public record SqliteAttachedDatabase(
    int Seq,
    string Name,
    string File
);

#endregion
