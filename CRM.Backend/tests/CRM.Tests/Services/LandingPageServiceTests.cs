// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class LandingPageServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<LandingPageService>> _mockLogger;
    private readonly LandingPageService _service;

    public LandingPageServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"LandingPageTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<LandingPageService>>();
        _service = new LandingPageService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoLandingPages()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenPageIsDeleted()
    {
        // Seed a soft-deleted page directly
        var page = new LandingPage
        {
            Id = 42,
            Name = "Deleted Page",
            Slug = "deleted-page",
            Status = LandingPageStatus.Draft,
            IsActive = false,
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        // GetByIdAsync filters out deleted pages (!lp.IsDeleted)
        var result = await _service.GetByIdAsync(42);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenSlugNotFound()
    {
        var result = await _service.GetBySlugAsync("nonexistent-slug");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnPage_WhenSlugExists()
    {
        var page = new LandingPage
        {
            Id = 2,
            Name = "Black Friday",
            Slug = "black-friday",
            Status = LandingPageStatus.Published,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        var result = await _service.GetBySlugAsync("black-friday");

        result.Should().NotBeNull();
        result!.Slug.Should().Be("black-friday");
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnTrue_WhenSlugNotInUse()
    {
        var isAvailable = await _service.IsSlugAvailableAsync("brand-new-slug");
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnFalse_WhenSlugAlreadyUsed()
    {
        var page = new LandingPage
        {
            Id = 3,
            Name = "Existing",
            Slug = "existing-slug",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        var isAvailable = await _service.IsSlugAvailableAsync("existing-slug");
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateSlugAsync_ShouldReturnSlugifiedString()
    {
        var slug = await _service.GenerateSlugAsync("My Awesome Landing Page");
        slug.Should().NotBeNullOrEmpty();
        slug.Should().NotContain(" ");
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPageToDatabase()
    {
        var page = new LandingPage
        {
            Name = "New Product Launch",
            Slug = "",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _service.CreateAsync(page, userId: 1);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Product Launch");
        _context.LandingPages.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_ShouldAutoGenerateSlug_WhenSlugIsEmpty()
    {
        var page = new LandingPage
        {
            Name = "Auto Slug Page",
            Slug = "",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _service.CreateAsync(page, userId: 1);

        result.Slug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePage()
    {
        var page = new LandingPage
        {
            Id = 50,
            Name = "Delete Me",
            Slug = "delete-me",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(50);

        _context.LandingPages.Count().Should().Be(0);
    }

    [Fact]
    public async Task IsSlugAvailableAsync_ShouldReturnTrue_WhenExcludingCurrentPage()
    {
        var page = new LandingPage
        {
            Id = 60,
            Name = "Self Check",
            Slug = "self-slug",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        var isAvailable = await _service.IsSlugAvailableAsync("self-slug", excludeId: 60);
        isAvailable.Should().BeTrue();
    }
}
