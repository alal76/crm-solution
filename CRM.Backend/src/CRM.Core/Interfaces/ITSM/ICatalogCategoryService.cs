using CRM.Core.Dtos.ITSM;

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service interface for managing ITSM catalog categories.
/// </summary>
public interface ICatalogCategoryService
{
    Task<CatalogCategoryDto> CreateAsync(CreateCatalogCategoryDto dto, CancellationToken ct = default);
    Task<CatalogCategoryDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<CatalogCategoryDto>> GetAllAsync(CancellationToken ct = default);
}
