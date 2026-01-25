namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for import/export operations
/// </summary>
public interface IImportExportService
{
    /// <summary>
    /// Get available entity types for import/export
    /// </summary>
    IEnumerable<EntityTypeInfo> GetEntityTypes();

    /// <summary>
    /// Export entity data as JSON
    /// </summary>
    Task<byte[]> ExportToJsonAsync(string entityType);

    /// <summary>
    /// Export entity data as CSV
    /// </summary>
    Task<byte[]> ExportToCsvAsync(string entityType);

    /// <summary>
    /// Get import template as JSON
    /// </summary>
    byte[] GetTemplateJson(string entityType);

    /// <summary>
    /// Get import template as CSV
    /// </summary>
    byte[] GetTemplateCsv(string entityType);

    /// <summary>
    /// Import data from JSON
    /// </summary>
    Task<ImportResult> ImportFromJsonAsync(string entityType, byte[] data);

    /// <summary>
    /// Import data from CSV
    /// </summary>
    Task<ImportResult> ImportFromCsvAsync(string entityType, byte[] data);
}

/// <summary>
/// Entity type info for import/export
/// </summary>
public class EntityTypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool CanImport { get; set; }
    public bool CanExport { get; set; }
}

/// <summary>
/// Import result DTO
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int ImportedRecords { get; set; }
    public int FailedRecords { get; set; }
    public IEnumerable<ImportError> Errors { get; set; } = new List<ImportError>();
}

public class ImportError
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
