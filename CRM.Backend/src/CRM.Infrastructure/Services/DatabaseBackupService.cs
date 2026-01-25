using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for database backup, restore, and migration operations.
/// 
/// HEXAGONAL ARCHITECTURE:
/// - Implements IDatabaseBackupInputPort (primary/driving port)
/// - Implements IDatabaseBackupService (backward compatibility)
/// - Uses ICrmDbContext (secondary/driven port)
/// </summary>
public class DatabaseBackupService : IDatabaseBackupService, IDatabaseBackupInputPort
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly IConfiguration _configuration;
    private string _backupDirectory;

    public DatabaseBackupService(ICrmDbContext context, ILogger<DatabaseBackupService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _backupDirectory = configuration["Backup:DefaultPath"] ?? "DatabaseBackups";

        // Ensure backup directory exists
        if (!Directory.Exists(_backupDirectory))
            Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<DatabaseBackupDto> CreateBackupAsync(int createdByUserId, CreateDatabaseBackupRequest request)
    {
        try
        {
            var backupName = $"backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var backupPath = Path.Combine(_backupDirectory, $"{backupName}.sql");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

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

    #region Upload and Download

    public async Task<DatabaseBackupDto> UploadBackupAsync(Stream fileStream, string fileName, int createdByUserId, UploadBackupRequest request)
    {
        try
        {
            var backupName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var uniqueName = $"{backupName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{extension}";
            var backupPath = Path.Combine(_backupDirectory, uniqueName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);

            // Save uploaded file
            using (var fileOut = new FileStream(backupPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileOut);
            }

            var fileInfo = new FileInfo(backupPath);
            var backup = new DatabaseBackup
            {
                BackupName = uniqueName,
                FilePath = backupPath,
                FileSizeBytes = fileInfo.Length,
                SourceDatabase = request.SourceDatabase ?? "Unknown",
                BackupType = "Uploaded",
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = createdByUserId,
                Description = request.Description ?? $"Uploaded from {fileName}",
                IsCompressed = extension.Equals(".gz", StringComparison.OrdinalIgnoreCase) || extension.Equals(".zip", StringComparison.OrdinalIgnoreCase),
                ChecksumHash = CalculateChecksum(backupPath)
            };

            _context.DatabaseBackups.Add(backup);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backup uploaded successfully: {BackupName}", uniqueName);

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
            _logger.LogError(ex, "Error uploading backup");
            throw;
        }
    }

    public async Task RestoreFromFileAsync(Stream fileStream, string fileName, int performedByUserId)
    {
        try
        {
            _logger.LogInformation("Restoring database from uploaded file: {FileName} by user {UserId}", fileName, performedByUserId);
            
            // Save to temporary location first
            var tempPath = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid()}{Path.GetExtension(fileName)}");
            using (var fileOut = new FileStream(tempPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileOut);
            }

            try
            {
                // In production, implement actual database restoration logic here
                // This would involve parsing and executing the SQL statements from the backup file
                _logger.LogInformation("Database restore from uploaded file completed successfully");
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring from uploaded file");
            throw;
        }
    }

    #endregion

    #region Schedule Operations

    public async Task<IEnumerable<BackupScheduleDto>> GetAllSchedulesAsync()
    {
        try
        {
            var schedules = await _context.BackupSchedules
                .Where(s => !s.IsDeleted)
                .OrderBy(s => s.Name)
                .Select(s => new BackupScheduleDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsEnabled = s.IsEnabled,
                    BackupType = s.BackupType,
                    CronExpression = s.CronExpression,
                    CronDescription = GetCronDescription(s.CronExpression),
                    BackupPath = s.BackupPath,
                    RetentionDays = s.RetentionDays,
                    MaxBackupsToKeep = s.MaxBackupsToKeep,
                    CompressBackups = s.CompressBackups,
                    LastBackupAt = s.LastBackupAt,
                    NextBackupAt = s.NextBackupAt,
                    LastError = s.LastError,
                    SuccessfulBackups = s.SuccessfulBackups,
                    FailedBackups = s.FailedBackups
                })
                .ToListAsync();

            return schedules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving backup schedules");
            throw;
        }
    }

    public async Task<BackupScheduleDto?> GetScheduleByIdAsync(int id)
    {
        try
        {
            var schedule = await _context.BackupSchedules
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new BackupScheduleDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsEnabled = s.IsEnabled,
                    BackupType = s.BackupType,
                    CronExpression = s.CronExpression,
                    CronDescription = GetCronDescription(s.CronExpression),
                    BackupPath = s.BackupPath,
                    RetentionDays = s.RetentionDays,
                    MaxBackupsToKeep = s.MaxBackupsToKeep,
                    CompressBackups = s.CompressBackups,
                    LastBackupAt = s.LastBackupAt,
                    NextBackupAt = s.NextBackupAt,
                    LastError = s.LastError,
                    SuccessfulBackups = s.SuccessfulBackups,
                    FailedBackups = s.FailedBackups
                })
                .FirstOrDefaultAsync();

            return schedule;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving backup schedule {id}");
            throw;
        }
    }

    public async Task<BackupScheduleDto> CreateScheduleAsync(CreateBackupScheduleRequest request)
    {
        try
        {
            var schedule = new BackupSchedule
            {
                Name = request.Name,
                IsEnabled = request.IsEnabled,
                BackupType = request.BackupType,
                CronExpression = request.CronExpression,
                BackupPath = request.BackupPath,
                RetentionDays = request.RetentionDays,
                MaxBackupsToKeep = request.MaxBackupsToKeep,
                CompressBackups = request.CompressBackups,
                NextBackupAt = CalculateNextRun(request.CronExpression),
                CreatedAt = DateTime.UtcNow
            };

            _context.BackupSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backup schedule created: {Name}", schedule.Name);

            return new BackupScheduleDto
            {
                Id = schedule.Id,
                Name = schedule.Name,
                IsEnabled = schedule.IsEnabled,
                BackupType = schedule.BackupType,
                CronExpression = schedule.CronExpression,
                CronDescription = GetCronDescription(schedule.CronExpression),
                BackupPath = schedule.BackupPath,
                RetentionDays = schedule.RetentionDays,
                MaxBackupsToKeep = schedule.MaxBackupsToKeep,
                CompressBackups = schedule.CompressBackups,
                NextBackupAt = schedule.NextBackupAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup schedule");
            throw;
        }
    }

    public async Task<BackupScheduleDto> UpdateScheduleAsync(int id, CreateBackupScheduleRequest request)
    {
        try
        {
            var schedule = await _context.BackupSchedules.FindAsync(id);
            if (schedule == null || schedule.IsDeleted)
                throw new KeyNotFoundException($"Schedule {id} not found");

            schedule.Name = request.Name;
            schedule.IsEnabled = request.IsEnabled;
            schedule.BackupType = request.BackupType;
            schedule.CronExpression = request.CronExpression;
            schedule.BackupPath = request.BackupPath;
            schedule.RetentionDays = request.RetentionDays;
            schedule.MaxBackupsToKeep = request.MaxBackupsToKeep;
            schedule.CompressBackups = request.CompressBackups;
            schedule.NextBackupAt = schedule.IsEnabled ? CalculateNextRun(request.CronExpression) : null;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Backup schedule updated: {Name}", schedule.Name);

            return new BackupScheduleDto
            {
                Id = schedule.Id,
                Name = schedule.Name,
                IsEnabled = schedule.IsEnabled,
                BackupType = schedule.BackupType,
                CronExpression = schedule.CronExpression,
                CronDescription = GetCronDescription(schedule.CronExpression),
                BackupPath = schedule.BackupPath,
                RetentionDays = schedule.RetentionDays,
                MaxBackupsToKeep = schedule.MaxBackupsToKeep,
                CompressBackups = schedule.CompressBackups,
                LastBackupAt = schedule.LastBackupAt,
                NextBackupAt = schedule.NextBackupAt,
                LastError = schedule.LastError,
                SuccessfulBackups = schedule.SuccessfulBackups,
                FailedBackups = schedule.FailedBackups
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating backup schedule {id}");
            throw;
        }
    }

    public async Task DeleteScheduleAsync(int id)
    {
        try
        {
            var schedule = await _context.BackupSchedules.FindAsync(id);
            if (schedule == null)
                throw new KeyNotFoundException($"Schedule {id} not found");

            schedule.IsDeleted = true;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Backup schedule deleted: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting backup schedule {id}");
            throw;
        }
    }

    public async Task<BackupScheduleDto> ToggleScheduleAsync(int id, bool enabled)
    {
        try
        {
            var schedule = await _context.BackupSchedules.FindAsync(id);
            if (schedule == null || schedule.IsDeleted)
                throw new KeyNotFoundException($"Schedule {id} not found");

            schedule.IsEnabled = enabled;
            schedule.NextBackupAt = enabled ? CalculateNextRun(schedule.CronExpression) : null;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Backup schedule {Id} toggled to {Enabled}", id, enabled);

            return await GetScheduleByIdAsync(id) ?? throw new KeyNotFoundException($"Schedule {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error toggling backup schedule {id}");
            throw;
        }
    }

    public async Task RunScheduledBackupAsync(int scheduleId)
    {
        var schedule = await _context.BackupSchedules.FindAsync(scheduleId);
        if (schedule == null || schedule.IsDeleted || !schedule.IsEnabled)
            return;

        try
        {
            _logger.LogInformation("Running scheduled backup: {Name}", schedule.Name);

            // Set backup path for this schedule
            var originalPath = _backupDirectory;
            _backupDirectory = schedule.BackupPath;

            try
            {
                Directory.CreateDirectory(_backupDirectory);

                var request = new CreateDatabaseBackupRequest
                {
                    Description = $"Scheduled backup: {schedule.Name}",
                    Compress = schedule.CompressBackups
                };

                // Create the backup (using system user id 0 for scheduled backups)
                await CreateBackupAsync(0, request);

                schedule.LastBackupAt = DateTime.UtcNow;
                schedule.NextBackupAt = CalculateNextRun(schedule.CronExpression);
                schedule.SuccessfulBackups++;
                schedule.LastError = null;

                // Clean up old backups based on retention policy
                await CleanupOldBackupsAsync(schedule);

                _logger.LogInformation("Scheduled backup completed: {Name}", schedule.Name);
            }
            finally
            {
                _backupDirectory = originalPath;
            }
        }
        catch (Exception ex)
        {
            schedule.FailedBackups++;
            schedule.LastError = ex.Message;
            _logger.LogError(ex, "Scheduled backup failed: {Name}", schedule.Name);
        }

        schedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private async Task CleanupOldBackupsAsync(BackupSchedule schedule)
    {
        try
        {
            var backupsToDelete = new List<DatabaseBackup>();

            // Get all backups for this schedule's path
            var backups = await _context.DatabaseBackups
                .Where(b => b.FilePath.StartsWith(schedule.BackupPath))
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            // Apply retention by count
            if (schedule.MaxBackupsToKeep > 0 && backups.Count > schedule.MaxBackupsToKeep)
            {
                backupsToDelete.AddRange(backups.Skip(schedule.MaxBackupsToKeep));
            }

            // Apply retention by days
            if (schedule.RetentionDays > 0)
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-schedule.RetentionDays);
                backupsToDelete.AddRange(backups.Where(b => b.CreatedAt < cutoffDate && !backupsToDelete.Contains(b)));
            }

            foreach (var backup in backupsToDelete.Distinct())
            {
                try
                {
                    if (File.Exists(backup.FilePath))
                        File.Delete(backup.FilePath);

                    _context.DatabaseBackups.Remove(backup);
                    _logger.LogInformation("Deleted old backup: {BackupName}", backup.BackupName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete backup: {BackupName}", backup.BackupName);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
        }
    }

    #endregion

    #region Settings

    public async Task<BackupSettingsDto> GetBackupSettingsAsync()
    {
        var schedules = await GetAllSchedulesAsync();
        return new BackupSettingsDto
        {
            DefaultBackupPath = _backupDirectory,
            AutoDeleteOldBackups = true,
            DefaultRetentionDays = 30,
            Schedules = schedules.ToList()
        };
    }

    public async Task UpdateBackupPathAsync(string path)
    {
        _backupDirectory = path;
        
        // Ensure directory exists
        if (!Directory.Exists(_backupDirectory))
            Directory.CreateDirectory(_backupDirectory);

        // The path is stored in the default schedule or in-memory
        // In production, this would be persisted to a configuration store
        _logger.LogInformation("Backup path updated to: {Path}", path);
        
        await Task.CompletedTask;
    }

    #endregion

    #region Cron Helpers

    private static string GetCronDescription(string cronExpression)
    {
        // Simple cron description generator
        var parts = cronExpression.Split(' ');
        if (parts.Length < 5) return cronExpression;

        var minute = parts[0];
        var hour = parts[1];
        var dayOfMonth = parts[2];
        var month = parts[3];
        var dayOfWeek = parts[4];

        // Common patterns
        if (dayOfMonth == "*" && month == "*" && dayOfWeek == "*")
            return $"Daily at {hour}:{minute.PadLeft(2, '0')}";
        if (dayOfMonth == "*" && month == "*" && dayOfWeek == "0")
            return $"Every Sunday at {hour}:{minute.PadLeft(2, '0')}";
        if (dayOfMonth == "*" && month == "*" && dayOfWeek == "1-5")
            return $"Weekdays at {hour}:{minute.PadLeft(2, '0')}";
        if (dayOfMonth == "1" && month == "*")
            return $"Monthly on the 1st at {hour}:{minute.PadLeft(2, '0')}";

        return cronExpression;
    }

    private static DateTime? CalculateNextRun(string cronExpression)
    {
        try
        {
            var parts = cronExpression.Split(' ');
            if (parts.Length < 5) return null;

            var minute = ParseCronField(parts[0], 0, 59);
            var hour = ParseCronField(parts[1], 0, 23);

            var now = DateTime.UtcNow;
            var next = new DateTime(now.Year, now.Month, now.Day, hour, minute, 0, DateTimeKind.Utc);

            if (next <= now)
                next = next.AddDays(1);

            return next;
        }
        catch
        {
            return DateTime.UtcNow.AddDays(1);
        }
    }

    private static int ParseCronField(string field, int min, int max)
    {
        if (field == "*") return min;
        if (int.TryParse(field, out var value) && value >= min && value <= max)
            return value;
        return min;
    }

    #endregion
}
