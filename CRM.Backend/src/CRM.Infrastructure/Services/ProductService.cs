using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Product service implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;

    public ProductService(IRepository<Product> repository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService)
    {
        _repository = repository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null) return null;

        var tags = await _normalizationService.GetTagsAsync("Product", product.Id);
        if (!string.IsNullOrWhiteSpace(tags)) product.Tags = tags;

        var cfs = await _normalizationService.GetCustomFieldsAsync("Product", product.Id);
        if (!string.IsNullOrWhiteSpace(cfs)) product.CustomFields = cfs;

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        var products = await _repository.FindAsync(p => !p.IsDeleted && p.IsActive);
        foreach (var product in products)
        {
            var tags = await _normalizationService.GetTagsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(tags)) product.Tags = tags;

            var cfs = await _normalizationService.GetCustomFieldsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(cfs)) product.CustomFields = cfs;
        }
        return products;
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _repository.FindAsync(p => !p.IsDeleted && p.IsActive && p.Category == category);
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        await _repository.AddAsync(product);
        await _repository.SaveAsync();
        return product.Id;
    }

    public async Task UpdateProductAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(product);
        await _repository.SaveAsync();
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product != null)
        {
            await _repository.DeleteAsync(product);
            await _repository.SaveAsync();
        }
    }
}
