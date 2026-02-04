// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
