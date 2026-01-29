using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    private readonly ICrmNotificationService _notificationService;

    public ProductsController(
        IProductService productService, 
        ILogger<ProductsController> logger,
        ICrmNotificationService notificationService)
    {
        _productService = productService;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving product {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(category);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving products for category {category}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("type/{type}")]
    public async Task<IActionResult> GetByType(ProductType type)
    {
        try
        {
            var products = await _productService.GetProductsByTypeAsync(type);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving products for type {type}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all service-type products (for the Services page)
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        try
        {
            var serviceTypes = new[] { 
                ProductType.Service, 
                ProductType.Consulting, 
                ProductType.ManagedService, 
                ProductType.ProfessionalServices,
                ProductType.Training,
                ProductType.SupportContract
            };
            var allProducts = await _productService.GetAllProductsAsync();
            var services = allProducts.Where(p => serviceTypes.Contains(p.ProductType)).ToList();
            return Ok(services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving service products");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        try
        {
            var id = await _productService.CreateProductAsync(product);
            product.Id = id;
            
            // Notify connected clients about the new product
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Product", id, product, userId);
            
            return CreatedAtAction(nameof(GetById), new { id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product product)
    {
        try
        {
            product.Id = id;
            await _productService.UpdateProductAsync(product);
            
            // Notify connected clients about the update
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordUpdatedAsync("Product", id, product, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating product {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            
            // Notify connected clients about the deletion
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordDeletedAsync("Product", id, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting product {id}");
            return StatusCode(500, "Internal server error");
        }
    }
}
