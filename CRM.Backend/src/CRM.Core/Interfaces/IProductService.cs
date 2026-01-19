using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Product service interface
/// </summary>
public interface IProductService
{
    Task<Product?> GetProductByIdAsync(int id);
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
    Task<int> CreateProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}
