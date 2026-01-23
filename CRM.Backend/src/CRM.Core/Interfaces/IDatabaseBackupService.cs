using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for database backup and migration operations
/// </summary>
public interface IDatabaseBackupService
{
    // Backup operations
    Task<DatabaseBackupDto> CreateBackupAsync(int createdByUserId, CreateDatabaseBackupRequest request);
    Task<IEnumerable<DatabaseBackupDto>> GetAllBackupsAsync();
    Task<DatabaseBackupDto?> GetBackupByIdAsync(int id);
    Task RestoreBackupAsync(int backupId, string targetDatabase, int performedByUserId);
    Task DeleteBackupAsync(int id);
    Task<byte[]> DownloadBackupAsync(int backupId);
    Task<DatabaseBackupDto> UploadBackupAsync(Stream fileStream, string fileName, int createdByUserId, UploadBackupRequest request);
    Task RestoreFromFileAsync(Stream fileStream, string fileName, int performedByUserId);
    
    // Schedule operations
    Task<IEnumerable<BackupScheduleDto>> GetAllSchedulesAsync();
    Task<BackupScheduleDto?> GetScheduleByIdAsync(int id);
    Task<BackupScheduleDto> CreateScheduleAsync(CreateBackupScheduleRequest request);
    Task<BackupScheduleDto> UpdateScheduleAsync(int id, CreateBackupScheduleRequest request);
    Task DeleteScheduleAsync(int id);
    Task<BackupScheduleDto> ToggleScheduleAsync(int id, bool enabled);
    Task RunScheduledBackupAsync(int scheduleId);
    
    // Settings
    Task<BackupSettingsDto> GetBackupSettingsAsync();
    Task UpdateBackupPathAsync(string path);
    
    // Migration
    Task MigrateDatabaseAsync(DatabaseMigrationConfig config, int performedByUserId);
    Task<string> GenerateSeedScriptAsync(string targetDatabase = "");
}
