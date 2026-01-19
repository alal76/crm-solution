using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Product service implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IRepository<Product> _repository;

    public ProductService(IRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return await _repository.FindAsync(p => !p.IsDeleted && p.IsActive);
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
