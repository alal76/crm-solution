// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// AP-038: Extracted from MonitoringService.cs (god-class split)
// Contains IDatabaseHealthService interface and DatabaseHealthService implementation.
// MonitoringService.GetDatabaseMetricsAsync now delegates to this class.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;

namespace CRM.Infrastructure.Services;

/// <summary>
/// AP-038: Interface for database health monitoring.
/// Extracted from MonitoringService to separate database concerns.
/// </summary>
public interface IDatabaseHealthService
{
    /// <summary>Get full database metrics (size, connections, version, latency)</summary>
    Task<DatabaseMetrics> GetDatabaseMetricsAsync(CancellationToken ct = default);

    /// <summary>Get the database server hostname from the connection string</summary>
    string GetDatabaseHost();
}

/// <summary>
/// AP-038: Database-specific health checks and metrics for SQL Server, MySQL/MariaDB, PostgreSQL.
/// Extracted from MonitoringService.cs to reduce god-class complexity.
/// </summary>
public class DatabaseHealthService : IDatabaseHealthService
{
    private readonly IConfiguration _configuration;
    private readonly MonitoringOptions _options;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(
        IConfiguration configuration,
        IOptions<MonitoringOptions> options,
        ILogger<DatabaseHealthService> logger)
    {
        _configuration = configuration;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DatabaseMetrics> GetDatabaseMetricsAsync(CancellationToken ct = default)
    {
        var provider = ParseDatabaseProvider(_options.DatabaseProvider);
        var metrics = new DatabaseMetrics
        {
            Timestamp = DateTime.UtcNow,
            Provider = provider,
            ProviderName = provider.ToString()
        };

        var connectionString = GetDatabaseConnectionString();
        var sw = Stopwatch.StartNew();

        try
        {
            switch (provider)
            {
                case DatabaseProviderType.SqlServer:
                    metrics = await GetSqlServerMetricsAsync(connectionString, ct);
                    break;
                case DatabaseProviderType.MariaDB:
                case DatabaseProviderType.MySQL:
                    metrics = await GetMySqlMetricsAsync(connectionString, provider, ct);
                    break;
                case DatabaseProviderType.PostgreSQL:
                    metrics = await GetPostgreSqlMetricsAsync(connectionString, ct);
                    break;
                default:
                    metrics.IsHealthy = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get database metrics");
            metrics.IsHealthy = false;
        }

        sw.Stop();
        if (metrics.ResponseTimeMs == 0)
        {
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
        }

        return metrics;
    }

    /// <inheritdoc/>
    public string GetDatabaseHost()
    {
        var connectionString = GetDatabaseConnectionString();

        if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(connectionString, @"Server=([^;]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            return match.Success ? match.Groups[1].Value : "unknown";
        }

        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            var match = Regex.Match(connectionString, @"Host=([^;]+)", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            return match.Success ? match.Groups[1].Value : "unknown";
        }

        return "unknown";
    }

    private string GetDatabaseConnectionString()
        => _configuration.GetConnectionString("DefaultConnection") ?? "";

    private static DatabaseProviderType ParseDatabaseProvider(string provider) =>
        provider?.ToLowerInvariant() switch
        {
            "sqlserver" or "mssql" => DatabaseProviderType.SqlServer,
            "mariadb" => DatabaseProviderType.MariaDB,
            "mysql" => DatabaseProviderType.MySQL,
            "postgresql" or "postgres" => DatabaseProviderType.PostgreSQL,
            "mongodb" or "mongo" => DatabaseProviderType.MongoDB,
            "oracle" => DatabaseProviderType.Oracle,
            _ => DatabaseProviderType.Unknown
        };

    // ─── SQL Server ──────────────────────────────────────────────────────────

    private async Task<DatabaseInfo> GetSqlServerInfoAsync(string connectionString, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = DatabaseProviderType.SqlServer,
            ProviderName = "SQL Server"
        };

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);

            info.IsConnected = true;
            info.Host = conn.DataSource ?? "";

            var cmd = new SqlCommand(@"
                SELECT
                    SERVERPROPERTY('ProductVersion') AS Version,
                    SERVERPROPERTY('Edition') AS Edition,
                    SERVERPROPERTY('Collation') AS Collation", conn);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader["Version"]?.ToString() ?? "";
                info.Edition = reader["Edition"]?.ToString() ?? "";
                info.Collation = reader["Collation"]?.ToString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get SQL Server info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetSqlServerMetricsAsync(string connectionString, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = DatabaseProviderType.SqlServer,
            ProviderName = "SQL Server",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            var versionCmd = new SqlCommand("SELECT SERVERPROPERTY('ProductVersion') AS Version", conn);
            var version = await versionCmd.ExecuteScalarAsync(ct);
            metrics.Version = version?.ToString() ?? "";

            var connCmd = new SqlCommand(@"
                SELECT COUNT(*) FROM sys.dm_exec_sessions
                WHERE is_user_process = 1", conn);
            var connCount = await connCmd.ExecuteScalarAsync(ct);
            metrics.ActiveConnections = Convert.ToInt32(connCount);

            var sizeCmd = new SqlCommand(@"
                SELECT SUM(size * 8.0 / 1024) AS SizeMB
                FROM sys.master_files
                WHERE database_id = DB_ID()", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            var perfCmd = new SqlCommand(@"
                SELECT
                    (SELECT cntr_value FROM sys.dm_os_performance_counters
                     WHERE counter_name = 'Batch Requests/sec' AND instance_name = '') AS BatchRequests,
                    (SELECT cntr_value FROM sys.dm_os_performance_counters
                     WHERE counter_name = 'Buffer cache hit ratio' AND instance_name = '') AS CacheHitRatio,
                    (SELECT COUNT(*) FROM sys.dm_exec_requests WHERE status = 'running') AS ActiveQueries", conn);

            try
            {
                await using var reader = await perfCmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    metrics.ProviderSpecificMetrics["batchRequestsPerSec"] = reader["BatchRequests"] ?? 0;
                    metrics.ProviderSpecificMetrics["bufferCacheHitRatio"] = reader["CacheHitRatio"] ?? 0;
                    metrics.ProviderSpecificMetrics["activeQueries"] = reader["ActiveQueries"] ?? 0;
                }
            }
            catch
            {
                // Permissions may prevent reading performance counters
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get SQL Server metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }

    // ─── MySQL / MariaDB ─────────────────────────────────────────────────────

    private async Task<DatabaseInfo> GetMySqlInfoAsync(string connectionString, DatabaseProviderType provider, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = provider,
            ProviderName = provider == DatabaseProviderType.MariaDB ? "MariaDB" : "MySQL"
        };

        try
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);

            info.IsConnected = true;
            info.Host = conn.DataSource ?? "";

            var cmd = new MySqlCommand("SELECT VERSION() AS Version, @@collation_database AS Collation", conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader["Version"]?.ToString() ?? "";
                info.Collation = reader["Collation"]?.ToString() ?? "";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get MySQL/MariaDB info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetMySqlMetricsAsync(string connectionString, DatabaseProviderType provider, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = provider,
            ProviderName = provider == DatabaseProviderType.MariaDB ? "MariaDB" : "MySQL",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            var versionCmd = new MySqlCommand("SELECT VERSION()", conn);
            metrics.Version = (await versionCmd.ExecuteScalarAsync(ct))?.ToString() ?? "";

            var connCmd = new MySqlCommand("SHOW STATUS LIKE 'Threads_connected'", conn);
            await using (var reader = await connCmd.ExecuteReaderAsync(ct))
            {
                if (await reader.ReadAsync(ct))
                {
                    metrics.ActiveConnections = Convert.ToInt32(reader["Value"]);
                }
            }

            var sizeCmd = new MySqlCommand(@"
                SELECT SUM(data_length + index_length) / 1024 / 1024 AS SizeMB
                FROM information_schema.tables
                WHERE table_schema = DATABASE()", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            var statusCmd = new MySqlCommand("SHOW GLOBAL STATUS WHERE Variable_name IN ('Queries', 'Slow_queries', 'Uptime')", conn);
            await using (var reader = await statusCmd.ExecuteReaderAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    var name = reader["Variable_name"]?.ToString() ?? "";
                    var value = reader["Value"];
                    metrics.ProviderSpecificMetrics[name.ToLowerInvariant()] = value ?? 0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get MySQL/MariaDB metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }

    // ─── PostgreSQL ──────────────────────────────────────────────────────────

    private async Task<DatabaseInfo> GetPostgreSqlInfoAsync(string connectionString, CancellationToken ct)
    {
        var info = new DatabaseInfo
        {
            Provider = DatabaseProviderType.PostgreSQL,
            ProviderName = "PostgreSQL"
        };

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);

            info.IsConnected = true;
            info.Host = conn.Host ?? "";
            info.Port = conn.Port;

            var cmd = new NpgsqlCommand("SELECT version(), current_setting('server_encoding')", conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                info.Version = reader.GetString(0);
                info.Collation = reader.GetString(1);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get PostgreSQL info");
            info.IsConnected = false;
        }

        return info;
    }

    private async Task<DatabaseMetrics> GetPostgreSqlMetricsAsync(string connectionString, CancellationToken ct)
    {
        var metrics = new DatabaseMetrics
        {
            Provider = DatabaseProviderType.PostgreSQL,
            ProviderName = "PostgreSQL",
            ProviderSpecificMetrics = new Dictionary<string, object>()
        };

        var sw = Stopwatch.StartNew();

        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync(ct);
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = true;

            var versionCmd = new NpgsqlCommand("SELECT version()", conn);
            metrics.Version = (await versionCmd.ExecuteScalarAsync(ct))?.ToString() ?? "";

            var connCmd = new NpgsqlCommand("SELECT count(*) FROM pg_stat_activity WHERE state = 'active'", conn);
            metrics.ActiveConnections = Convert.ToInt32(await connCmd.ExecuteScalarAsync(ct));

            var sizeCmd = new NpgsqlCommand("SELECT pg_database_size(current_database()) / 1024.0 / 1024.0", conn);
            var size = await sizeCmd.ExecuteScalarAsync(ct);
            metrics.DatabaseSizeMB = Convert.ToDouble(size ?? 0);

            var statsCmd = new NpgsqlCommand(@"
                SELECT
                    xact_commit, xact_rollback, blks_read, blks_hit,
                    tup_returned, tup_fetched, tup_inserted, tup_updated, tup_deleted
                FROM pg_stat_database
                WHERE datname = current_database()", conn);

            await using var reader = await statsCmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                metrics.ProviderSpecificMetrics["xact_commit"] = reader["xact_commit"];
                metrics.ProviderSpecificMetrics["xact_rollback"] = reader["xact_rollback"];
                metrics.ProviderSpecificMetrics["blks_read"] = reader["blks_read"];
                metrics.ProviderSpecificMetrics["blks_hit"] = reader["blks_hit"];
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get PostgreSQL metrics");
            sw.Stop();
            metrics.ResponseTimeMs = (int)sw.ElapsedMilliseconds;
            metrics.IsHealthy = false;
        }

        return metrics;
    }
}
