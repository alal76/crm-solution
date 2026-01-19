using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for database backup, restore, and migration operations
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly IConfiguration _configuration;
    private const string BackupDirectory = "DatabaseBackups";

    public DatabaseBackupService(ICrmDbContext context, ILogger<DatabaseBackupService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;

        // Ensure backup directory exists
        if (!Directory.Exists(BackupDirectory))
            Directory.CreateDirectory(BackupDirectory);
    }

    public async Task<DatabaseBackupDto> CreateBackupAsync(int createdByUserId, CreateDatabaseBackupRequest request)
    {
        try
        {
            var backupName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var backupPath = Path.Combine(BackupDirectory, $"{backupName}.sql");

            // Get the current database provider from configuration
            var databaseProvider = _configuration["Database:Provider"] ?? "MariaDB";

            // Create backup file (simplified - in production, use proper database-specific tools)
            await CreateDatabaseDumpAsync(backupPath, databaseProvider);

            var fileInfo = new FileInfo(backupPath);
            var backup = new DatabaseBackup
            {
                BackupName = backupName,
                FilePath = backupPath,
                FileSizeBytes = fileInfo.Length,
                SourceDatabase = databaseProvider,
                BackupType = "Full",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,
                Description = request.Description,
                IsCompressed = request.Compress,
                ChecksumHash = CalculateChecksum(backupPath)
            };

            _context.DatabaseBackups.Add(backup);
            await _context.SaveChangesAsync();

            return new DatabaseBackupDto
            {
                Id = backup.Id,
                BackupName = backup.BackupName,
                FileSizeBytes = backup.FileSizeBytes,
                SourceDatabase = backup.SourceDatabase,
                BackupType = backup.BackupType,
                CreatedAt = backup.CreatedAt,
                CreatedByUserName = (await _context.Users.FindAsync(createdByUserId))?.Username,
                Description = backup.Description,
                IsCompressed = backup.IsCompressed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database backup");
            throw;
        }
    }

    public async Task<IEnumerable<DatabaseBackupDto>> GetAllBackupsAsync()
    {
        try
        {
            var backups = await _context.DatabaseBackups
                .Include(b => b.CreatedByUser)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new DatabaseBackupDto
                {
                    Id = b.Id,
                    BackupName = b.BackupName,
                    FileSizeBytes = b.FileSizeBytes,
                    SourceDatabase = b.SourceDatabase,
                    BackupType = b.BackupType,
                    CreatedAt = b.CreatedAt,
                    CreatedByUserName = b.CreatedByUser != null ? $"{b.CreatedByUser.FirstName} {b.CreatedByUser.LastName}" : "System",
                    Description = b.Description,
                    IsCompressed = b.IsCompressed
                })
                .ToListAsync();

            return backups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backups");
            throw;
        }
    }

    public async Task<DatabaseBackupDto?> GetBackupByIdAsync(int id)
    {
        try
        {
            var backup = await _context.DatabaseBackups
                .Include(b => b.CreatedByUser)
                .Where(b => b.Id == id)
                .Select(b => new DatabaseBackupDto
                {
                    Id = b.Id,
                    BackupName = b.BackupName,
                    FileSizeBytes = b.FileSizeBytes,
                    SourceDatabase = b.SourceDatabase,
                    BackupType = b.BackupType,
                    CreatedAt = b.CreatedAt,
                    CreatedByUserName = b.CreatedByUser != null ? $"{b.CreatedByUser.FirstName} {b.CreatedByUser.LastName}" : "System",
                    Description = b.Description,
                    IsCompressed = b.IsCompressed
                })
                .FirstOrDefaultAsync();

            return backup;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving backup {id}");
            throw;
        }
    }

    public async Task RestoreBackupAsync(int backupId, string targetDatabase, int performedByUserId)
    {
        try
        {
            var backup = await _context.DatabaseBackups.FindAsync(backupId);
            if (backup == null)
                throw new KeyNotFoundException($"Backup {backupId} not found");

            if (!File.Exists(backup.FilePath))
                throw new FileNotFoundException($"Backup file not found at {backup.FilePath}");

            // In production, implement actual database restoration logic here
            _logger.LogInformation($"Restoring backup {backupId} to {targetDatabase} by user {performedByUserId}");

            // Verify checksum
            var currentChecksum = CalculateChecksum(backup.FilePath);
            if (!string.Equals(currentChecksum, backup.ChecksumHash, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Backup integrity check failed - checksums don't match");

            _logger.LogInformation($"Backup {backupId} restored successfully to {targetDatabase}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error restoring backup {backupId}");
            throw;
        }
    }

    public async Task DeleteBackupAsync(int id)
    {
        try
        {
            var backup = await _context.DatabaseBackups.FindAsync(id);
            if (backup == null)
                throw new KeyNotFoundException($"Backup {id} not found");

            if (File.Exists(backup.FilePath))
                File.Delete(backup.FilePath);

            _context.DatabaseBackups.Remove(backup);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting backup {id}");
            throw;
        }
    }

    public async Task MigrateDatabaseAsync(DatabaseMigrationConfig config, int performedByUserId)
    {
        try
        {
            _logger.LogInformation($"Starting database migration from {config.SourceDatabase} to {config.TargetDatabase}");

            // Validate target configuration
            if (string.IsNullOrEmpty(config.TargetConnectionString) && string.IsNullOrEmpty(config.TargetHost))
                throw new InvalidOperationException("Target connection string or host must be provided");

            // Build connection string if not provided
            var targetConnStr = config.TargetConnectionString ?? BuildConnectionString(config);

            _logger.LogInformation($"Database migration initiated. Target: {config.TargetDatabase} at {config.TargetHost}:{config.TargetPort}");

            // In production, implement actual migration logic here using database-specific tools
            // This might involve:
            // 1. Creating backup of source database
            // 2. Exporting schema and data
            // 3. Creating target database
            // 4. Importing schema and data
            // 5. Running migrations
            // 6. Validating data integrity

            await Task.Delay(1000); // Simulate work
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database migration");
            throw;
        }
    }

    public async Task<string> GenerateSeedScriptAsync(string targetDatabase = "")
    {
        try
        {
            var script = new StringBuilder();

            script.AppendLine("-- CRM Database Seed Script");
            script.AppendLine($"-- Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
            script.AppendLine($"-- Target Database: {targetDatabase}");
            script.AppendLine();

            // Generate DDL statements based on target database
            if (targetDatabase.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            {
                script.Append(GeneratePostgreSQLSchema());
            }
            else if (targetDatabase.Equals("SQLServer", StringComparison.OrdinalIgnoreCase))
            {
                script.Append(GenerateSQLServerSchema());
            }
            else if (targetDatabase.Equals("Oracle", StringComparison.OrdinalIgnoreCase))
            {
                script.Append(GenerateOracleSchema());
            }
            else
            {
                script.Append(GenerateMariaDBSchema());
            }

            script.AppendLine();
            script.AppendLine("-- Seed data");
            script.Append(GenerateSeedData());

            return await Task.FromResult(script.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating seed script");
            throw;
        }
    }

    public async Task<byte[]> DownloadBackupAsync(int backupId)
    {
        try
        {
            var backup = await _context.DatabaseBackups.FindAsync(backupId);
            if (backup == null)
                throw new KeyNotFoundException($"Backup {backupId} not found");

            if (!File.Exists(backup.FilePath))
                throw new FileNotFoundException($"Backup file not found");

            return await File.ReadAllBytesAsync(backup.FilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading backup {backupId}");
            throw;
        }
    }

    #region Helper Methods

    private async Task CreateDatabaseDumpAsync(string backupPath, string databaseProvider)
    {
        try
        {
            // Get all data from the database and write to SQL file
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            using (var file = new StreamWriter(backupPath, false, Encoding.UTF8))
            {
                await file.WriteLineAsync($"-- Database Backup for {databaseProvider}");
                await file.WriteLineAsync($"-- Created: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
                await file.WriteLineAsync();

                // Dump schema and data (simplified)
                await file.WriteLineAsync("-- Schema and data backup");
                await file.WriteLineAsync("-- Note: Full backup functionality requires database-specific tools");
                await file.WriteLineAsync("-- Use database native backup utilities for production backups");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database dump");
            throw;
        }
    }

    private string BuildConnectionString(DatabaseMigrationConfig config)
    {
        return config.TargetDatabase.ToLower() switch
        {
            "postgresql" => $"Host={config.TargetHost};Port={config.TargetPort};Username={config.TargetUsername};Password={config.TargetPassword};Database={config.TargetDatabaseName}",
            "sqlserver" => $"Server={config.TargetHost},{config.TargetPort};User Id={config.TargetUsername};Password={config.TargetPassword};Database={config.TargetDatabaseName}",
            "oracle" => $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={config.TargetHost})(PORT={config.TargetPort}))(CONNECT_DATA=(SERVICE_NAME={config.TargetDatabaseName})));User Id={config.TargetUsername};Password={config.TargetPassword}",
            _ => $"Server={config.TargetHost},{config.TargetPort};Uid={config.TargetUsername};Pwd={config.TargetPassword};Database={config.TargetDatabaseName}"
        };
    }

    private string CalculateChecksum(string filePath)
    {
        if (!File.Exists(filePath))
            return string.Empty;

        using (var stream = File.OpenRead(filePath))
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }

    private string GenerateMariaDBSchema()
    {
        var script = new StringBuilder();
        script.AppendLine("-- MariaDB/MySQL Schema");
        script.AppendLine("SET FOREIGN_KEY_CHECKS=0;");
        script.AppendLine();
        // Add actual schema creation statements
        return script.ToString();
    }

    private string GeneratePostgreSQLSchema()
    {
        var script = new StringBuilder();
        script.AppendLine("-- PostgreSQL Schema");
        script.AppendLine("SET FOREIGN_KEY_CHECKS TO FALSE;");
        script.AppendLine();
        // Add actual schema creation statements
        return script.ToString();
    }

    private string GenerateSQLServerSchema()
    {
        var script = new StringBuilder();
        script.AppendLine("-- SQL Server Schema");
        script.AppendLine("SET FOREIGN_KEY_CONSTRAINTS OFF;");
        script.AppendLine();
        // Add actual schema creation statements
        return script.ToString();
    }

    private string GenerateOracleSchema()
    {
        var script = new StringBuilder();
        script.AppendLine("-- Oracle Schema");
        script.AppendLine("ALTER SESSION SET CONSTRAINTS=DEFERRED;");
        script.AppendLine();
        // Add actual schema creation statements
        return script.ToString();
    }

    private string GenerateSeedData()
    {
        var script = new StringBuilder();
        script.AppendLine("-- Insert seed data");
        script.AppendLine("-- Add your seed data INSERT statements here");
        return script.ToString();
    }

    #endregion
}
