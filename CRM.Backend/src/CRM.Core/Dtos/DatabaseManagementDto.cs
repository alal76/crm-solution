namespace CRM.Core.Dtos;

/// <summary>
/// DTO for database backup information
/// </summary>
public class DatabaseBackupDto
{
    public int Id { get; set; }
    public string BackupName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SourceDatabase { get; set; } = string.Empty;
    public string? BackupType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserName { get; set; }
    public string? Description { get; set; }
    public bool IsCompressed { get; set; }
}

/// <summary>
/// DTO for database migration configuration
/// </summary>
public class DatabaseMigrationConfig
{
    public string SourceDatabase { get; set; } = string.Empty; // Current database type
    public string TargetDatabase { get; set; } = string.Empty; // Target database type (PostgreSQL, SQLServer, Oracle)
    public string TargetConnectionString { get; set; } = string.Empty;
    public string TargetHost { get; set; } = string.Empty;
    public int TargetPort { get; set; }
    public string TargetUsername { get; set; } = string.Empty;
    public string TargetPassword { get; set; } = string.Empty;
    public string TargetDatabaseName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for database backup request
/// </summary>
public class CreateDatabaseBackupRequest
{
    public string? Description { get; set; }
    public bool Compress { get; set; } = true;
}

/// <summary>
/// DTO for database restore request
/// </summary>
public class RestoreDatabaseBackupRequest
{
    public int BackupId { get; set; }
    public string TargetDatabase { get; set; } = string.Empty;
    public string? TargetConnectionString { get; set; }
}

/// <summary>
/// DTO for database configuration request
/// </summary>
public class DatabaseConfigRequest
{
    public string DatabaseType { get; set; } = string.Empty; // PostgreSQL, SQLServer, Oracle, MySQL
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}
