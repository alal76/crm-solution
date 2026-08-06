// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class ServiceCatalogServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<ServiceCatalogService>> _mockLogger;
    private readonly ServiceCatalogService _service;

    public ServiceCatalogServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"CatalogTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<ServiceCatalogService>>();
        _service = new ServiceCatalogService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetCatalogItemsAsync_ShouldReturnActiveItems()
    {
        var category = new CatalogCategory { Name = "Hardware", IsActive = true, CreatedAt = DateTime.UtcNow };
        _context.CatalogCategories.Add(category);
        await _context.SaveChangesAsync();

        _context.CatalogItems.AddRange(
            new CatalogItem { Name = "Laptop", IsActive = true, CategoryId = category.CategoryId, CreatedAt = DateTime.UtcNow },
            new CatalogItem { Name = "Inactive Item", IsActive = false, CategoryId = category.CategoryId, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var result = (await _service.GetCatalogItemsAsync(null, null)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Laptop");
    }

    [Fact]
    public async Task GetCatalogItemsAsync_ShouldFilterByCategory()
    {
        var cat1 = new CatalogCategory { Name = "Software", IsActive = true, CreatedAt = DateTime.UtcNow };
        var cat2 = new CatalogCategory { Name = "Hardware", IsActive = true, CreatedAt = DateTime.UtcNow };
        _context.CatalogCategories.AddRange(cat1, cat2);
        await _context.SaveChangesAsync();

        _context.CatalogItems.AddRange(
            new CatalogItem { Name = "Office Suite", IsActive = true, CategoryId = cat1.CategoryId, CreatedAt = DateTime.UtcNow },
            new CatalogItem { Name = "Monitor", IsActive = true, CategoryId = cat2.CategoryId, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var result = (await _service.GetCatalogItemsAsync(cat1.CategoryId, null)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Office Suite");
    }

    [Fact]
    public async Task GetCatalogItemByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetCatalogItemByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCatalogItemByIdAsync_ShouldReturnItem_WhenExists()
    {
        var category = new CatalogCategory { Name = "Services", IsActive = true, CreatedAt = DateTime.UtcNow };
        _context.CatalogCategories.Add(category);
        await _context.SaveChangesAsync();

        var item = new CatalogItem { Name = "VPN Access", IsActive = true, CategoryId = category.CategoryId, CreatedAt = DateTime.UtcNow };
        _context.CatalogItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _service.GetCatalogItemByIdAsync(item.CatalogItemId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("VPN Access");
    }

    [Fact]
    public async Task CreateCatalogRequestAsync_ShouldAddRequestToDatabase()
    {
        var dto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1,
            RequestedForId = 2
        };

        var requestId = await _service.CreateCatalogRequestAsync(dto, requestedById: 1);

        requestId.Should().BeGreaterThan(0);
        _context.CatalogRequests.Count().Should().Be(1);
        var request = await _context.CatalogRequests.FirstAsync();
        request.State.Should().Be(CatalogRequestState.Requested);
    }

    [Fact]
    public async Task CancelRequestAsync_ShouldSetStateToCancelled_WhenRequestedByOwner()
    {
        var request = new CatalogRequest
        {
            CatalogItemId = 1,
            RequestedById = 5,
            RequestedForId = 5,
            State = CatalogRequestState.Requested,
            CreatedAt = DateTime.UtcNow
        };
        _context.CatalogRequests.Add(request);
        await _context.SaveChangesAsync();

        var result = await _service.CancelRequestAsync(request.RequestId, userId: 5);

        result.Should().BeTrue();
        var updated = await _context.CatalogRequests.FindAsync(request.RequestId);
        updated!.State.Should().Be(CatalogRequestState.Cancelled);
    }

    [Fact]
    public async Task CancelRequestAsync_ShouldReturnFalse_WhenNotOwner()
    {
        var request = new CatalogRequest
        {
            CatalogItemId = 1,
            RequestedById = 5,
            RequestedForId = 5,
            State = CatalogRequestState.Requested,
            CreatedAt = DateTime.UtcNow
        };
        _context.CatalogRequests.Add(request);
        await _context.SaveChangesAsync();

        var result = await _service.CancelRequestAsync(request.RequestId, userId: 99);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCatalogAsync_ShouldReturnMatchingItems()
    {
        var category = new CatalogCategory { Name = "IT", IsActive = true, CreatedAt = DateTime.UtcNow };
        _context.CatalogCategories.Add(category);
        await _context.SaveChangesAsync();

        _context.CatalogItems.AddRange(
            new CatalogItem { Name = "Email Setup", ShortDescription = "Configure email", IsActive = true, CategoryId = category.CategoryId, CreatedAt = DateTime.UtcNow },
            new CatalogItem { Name = "VPN Access", ShortDescription = "Secure remote access", IsActive = true, CategoryId = category.CategoryId, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var result = (await _service.SearchCatalogAsync("email")).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Email Setup");
    }
}
