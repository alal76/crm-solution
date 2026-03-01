// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Product service implementation.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IProductInputPort (primary/driving port)
/// - Implements IProductService (backward compatibility)
/// - Uses IRepository for data access (secondary/driven port)
/// </summary>
public class ProductService : IProductService, IProductInputPort
{
    private readonly IRepository<Product> _repository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;
    private readonly IDuplicateDetectionService _duplicateDetection;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IRepository<Product> repository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService,
        IDuplicateDetectionService duplicateDetection,
        ICrmDbContext dbContext,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
        _duplicateDetection = duplicateDetection;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return null;

        var tags = await _normalizationService.GetTagsAsync("Product", product.Id);
        if (!string.IsNullOrWhiteSpace(tags))
            product.Tags = tags;

        var cfs = await _normalizationService.GetCustomFieldsAsync("Product", product.Id);
        if (!string.IsNullOrWhiteSpace(cfs))
            product.CustomFields = cfs;

        return product;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        var products = await _repository.FindAsync(p => !p.IsDeleted && p.IsActive);
        foreach (var product in products)
        {
            var tags = await _normalizationService.GetTagsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(tags))
                product.Tags = tags;

            var cfs = await _normalizationService.GetCustomFieldsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(cfs))
                product.CustomFields = cfs;
        }
        return products;
    }

    public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
    {
        return await _repository.FindAsync(p => !p.IsDeleted && p.IsActive && p.Category == category);
    }

    public async Task<IEnumerable<Product>> GetProductsByTypeAsync(ProductType type)
    {
        var products = await _repository.FindAsync(p => !p.IsDeleted && p.IsActive && p.ProductType == type);
        foreach (var product in products)
        {
            var tags = await _normalizationService.GetTagsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(tags))
                product.Tags = tags;

            var cfs = await _normalizationService.GetCustomFieldsAsync("Product", product.Id);
            if (!string.IsNullOrWhiteSpace(cfs))
                product.CustomFields = cfs;
        }
        return products;
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        // Duplicate detection check before creation
        var fieldValues = new Dictionary<string, string?>
        {
            ["Name"] = product.Name,
            ["SKU"] = product.SKU,
            ["ProductCode"] = product.ProductCode
        };
        var candidatesQueued = await DuplicateCheckHelper.CheckAndHandleDuplicatesAsync(
            _duplicateDetection, _dbContext, "Product", fieldValues, _logger);

        await _repository.AddAsync(product);
        await _repository.SaveAsync();

        // Update any queued duplicate candidates with the new entity ID
        if (candidatesQueued > 0)
            await DuplicateCheckHelper.UpdateCandidateSourceIdsAsync(_dbContext, "Product", product.Id);

        return product.Id;
    }

    public async Task UpdateProductAsync(Product product)
    {
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
