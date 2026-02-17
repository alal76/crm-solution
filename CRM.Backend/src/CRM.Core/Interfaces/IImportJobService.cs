using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for import job management.
/// </summary>
public interface IImportJobService
{
    Task<ImportJobDto> CreateAsync(CreateImportJobDto dto, CancellationToken ct = default);
    Task<ImportJobDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<ImportJobDto>> GetAllAsync(CancellationToken ct = default);
}
