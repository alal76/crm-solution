namespace CRM.Core.Entities;

/// <summary>
/// Database backup record for tracking backups
/// </summary>
public class DatabaseBackup : BaseEntity
{
    public string BackupName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SourceDatabase { get; set; } = string.Empty; // MariaDB, PostgreSQL, etc.
    public string? BackupType { get; set; } = "Full"; // Full, Incremental, Differential
    public int? CreatedByUserId { get; set; }
    public string? Description { get; set; }
    public bool IsCompressed { get; set; } = true;
    public string? ChecksumHash { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
}
