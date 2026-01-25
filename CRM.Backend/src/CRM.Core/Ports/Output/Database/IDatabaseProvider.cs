using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;

namespace CRM.Core.Ports.Output.Database;

/// <summary>
/// Represents the type of database provider.
/// </summary>
public enum DatabaseProviderType
{
    MariaDb,
    MySql,
    PostgreSql,
    SqlServer,
    Oracle,
    Sqlite,
    InMemory
}

/// <summary>
/// Base interface for all database providers following hexagonal architecture.
/// This port defines the contract that database adapters must implement.
/// </summary>
public interface IDatabaseProvider : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the type of the database provider.
    /// </summary>
    DatabaseProviderType ProviderType { get; }

    /// <summary>
    /// Gets the provider name as a string.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets the current connection string (with sensitive data masked).
    /// </summary>
    string MaskedConnectionString { get; }

    /// <summary>
    /// Gets whether the provider is currently connected.
    /// </summary>
    bool IsConnected { get; }

    #region Connection Management

    /// <summary>
    /// Opens a connection to the database.
    /// </summary>
    Task OpenConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the current database connection.
    /// </summary>
    Task CloseConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the database connection.
    /// </summary>
    /// <returns>True if the connection is successful.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the connection state.
    /// </summary>
    ConnectionState GetConnectionState();

    #endregion

    #region Transaction Management

    /// <summary>
    /// Begins a new transaction.
    /// </summary>
    Task<IDbTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Raw SQL Execution

    /// <summary>
    /// Executes a raw SQL command and returns the number of affected rows.
    /// </summary>
    Task<int> ExecuteNonQueryAsync(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL query and returns a scalar value.
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw SQL query and returns a list of results.
    /// </summary>
    Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Executes a raw SQL query and returns a single result or default.
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, CancellationToken cancellationToken = default) where T : class;

    #endregion

    #region Database Metadata

    /// <summary>
    /// Gets the database server version.
    /// </summary>
    Task<string> GetServerVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current database name.
    /// </summary>
    Task<string> GetDatabaseNameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of all tables in the database.
    /// </summary>
    Task<IEnumerable<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a table exists in the database.
    /// </summary>
    Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the column information for a table.
    /// </summary>
    Task<IEnumerable<DatabaseColumnInfo>> GetTableColumnsAsync(string tableName, CancellationToken cancellationToken = default);

    #endregion

    #region Database Health

    /// <summary>
    /// Performs a health check on the database.
    /// </summary>
    Task<DatabaseHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets database statistics.
    /// </summary>
    Task<DatabaseStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Migration Support

    /// <summary>
    /// Ensures the database exists, creating it if necessary.
    /// </summary>
    Task<bool> EnsureDatabaseExistsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies pending migrations.
    /// </summary>
    Task MigrateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of pending migrations.
    /// </summary>
    Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of applied migrations.
    /// </summary>
    Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Backup and Restore

    /// <summary>
    /// Creates a backup of the database.
    /// </summary>
    Task<string> BackupAsync(string backupPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Restores the database from a backup.
    /// </summary>
    Task RestoreAsync(string backupPath, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Represents database column metadata.
/// </summary>
public record DatabaseColumnInfo(
    string Name,
    string DataType,
    bool IsNullable,
    bool IsPrimaryKey,
    bool IsForeignKey,
    int? MaxLength,
    string? DefaultValue
);

/// <summary>
/// Represents the result of a database health check.
/// </summary>
public record DatabaseHealthResult(
    bool IsHealthy,
    string Status,
    TimeSpan ResponseTime,
    string? ErrorMessage = null
);

/// <summary>
/// Represents database statistics.
/// </summary>
public record DatabaseStatistics(
    long DatabaseSizeBytes,
    int ConnectionCount,
    int ActiveTransactions,
    DateTime ServerStartTime,
    Dictionary<string, long> TableRowCounts
);

/// <summary>
/// Factory interface for creating database providers.
/// </summary>
public interface IDatabaseProviderFactory
{
    /// <summary>
    /// Creates a database provider based on the specified type.
    /// </summary>
    IDatabaseProvider CreateProvider(DatabaseProviderType providerType, string connectionString);

    /// <summary>
    /// Creates a database provider from configuration.
    /// </summary>
    IDatabaseProvider CreateFromConfiguration();

    /// <summary>
    /// Gets all supported provider types.
    /// </summary>
    IEnumerable<DatabaseProviderType> GetSupportedProviders();
}
