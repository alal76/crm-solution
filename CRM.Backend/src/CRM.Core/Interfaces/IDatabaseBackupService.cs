using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for database backup and migration operations
/// </summary>
public interface IDatabaseBackupService
{
    Task<DatabaseBackupDto> CreateBackupAsync(int createdByUserId, CreateDatabaseBackupRequest request);
    Task<IEnumerable<DatabaseBackupDto>> GetAllBackupsAsync();
    Task<DatabaseBackupDto?> GetBackupByIdAsync(int id);
    Task RestoreBackupAsync(int backupId, string targetDatabase, int performedByUserId);
    Task DeleteBackupAsync(int id);
    Task MigrateDatabaseAsync(DatabaseMigrationConfig config, int performedByUserId);
    Task<string> GenerateSeedScriptAsync(string targetDatabase = "");
    Task<byte[]> DownloadBackupAsync(int backupId);
}
