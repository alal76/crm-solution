// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Hubs;
using CRM.Core.Entities;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing products and services.
/// Provides endpoints for CRUD operations and filtering by category/type.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : CrmControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    private readonly ICrmNotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService">Service for product business logic.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    /// <param name="notificationService">Service for SignalR real-time notifications.</param>
    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger,
        ICrmNotificationService notificationService)
    {
        _productService = productService;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all products in the catalog.
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
                var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Gets a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <returns>The product if found</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
                var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });
        return Ok(product);
    }

    /// <summary>
    /// Gets products by category.
    /// </summary>
    /// <param name="category">The product category to filter by</param>
    /// <returns>List of products in the category</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByCategory(string category)
    {
                var products = await _productService.GetProductsByCategoryAsync(category);
        return Ok(products);
    }

    /// <summary>
    /// Gets products by type.
    /// </summary>
    /// <param name="type">The product type to filter by</param>
    /// <returns>List of products of the specified type</returns>
    /// <response code="200">Returns the list of products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("type/{type}")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByType(ProductType type)
    {
                var products = await _productService.GetProductsByTypeAsync(type);
        return Ok(products);
    }

    /// <summary>
    /// Gets all service-type products (for the Services page).
    /// Includes: Service, Consulting, ManagedService, ProfessionalServices, Training, SupportContract.
    /// </summary>
    /// <returns>List of service products</returns>
    /// <response code="200">Returns the list of service products</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpGet("services")]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetServices()
    {
                var serviceTypes = new[]
        {
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

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product to create</param>
    /// <returns>The created product</returns>
    /// <response code="201">Returns the newly created product</response>
    /// <response code="400">If the product data is invalid</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] Product product)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _productService.CreateProductAsync(product);
            product.Id = id;

            // Notify connected clients about the new product
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordCreatedAsync("Product", id, product, userId);

            return CreatedAtAction(nameof(GetById), new { id }, product);
        }
        catch (DuplicateExistsException dex)
        {
            return Conflict(new { message = dex.Message, entityType = dex.EntityType, existingRecordId = dex.ExistingRecordId, matchScore = dex.MatchScore });
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="product">The updated product data</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the product was updated successfully</response>
    /// <response code="400">If the product data is invalid</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
                if (!ModelState.IsValid)
            return BadRequest(ModelState);

        product.Id = id;
        await _productService.UpdateProductAsync(product);

        // Notify connected clients about the update
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        await _notificationService.NotifyRecordUpdatedAsync("Product", id, product, userId);

        return NoContent();
    }

    /// <summary>
    /// Deletes a product (soft delete).
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <returns>No content on success</returns>
    /// <response code="204">If the product was deleted successfully</response>
    /// <response code="404">If the product is not found</response>
    /// <response code="500">If there was an internal server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
                await _productService.DeleteProductAsync(id);

        // Notify connected clients about the deletion
        var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
        await _notificationService.NotifyRecordDeletedAsync("Product", id, userId);

        return NoContent();
    }
}
