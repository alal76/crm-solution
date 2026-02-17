using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for export job management.
/// </summary>
public interface IExportJobService
{
    Task<ExportJobDto> CreateAsync(CreateExportJobDto dto, CancellationToken ct = default);
    Task<ExportJobDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ExportJobDto>> GetAllAsync(CancellationToken ct = default);
}
