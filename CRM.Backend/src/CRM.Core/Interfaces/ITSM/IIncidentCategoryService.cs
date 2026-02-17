using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for managing ITSM incident categories.
/// </summary>
public interface IIncidentCategoryService
{
    Task<IncidentCategoryDto> CreateAsync(CreateIncidentCategoryDto dto, CancellationToken ct = default);
    Task<IncidentCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<IncidentCategoryDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
}
