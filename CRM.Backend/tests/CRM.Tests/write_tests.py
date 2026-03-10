
# Helper script to write multiple test files
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"

files = {}

files["Services/LandingPageServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
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
    public async Task GetByIdAsync_ShouldReturnPage_WhenExists()
    {
        var page = new LandingPage
        {
            Id = 1,
            Name = "Summer Sale",
            Slug = "summer-sale",
            Status = LandingPageStatus.Draft,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.LandingPages.Add(page);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Summer Sale");
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
"""

files["Services/DuplicateDetectionServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
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

public class DuplicateDetectionServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<DuplicateDetectionService>> _mockLogger;
    private readonly DuplicateDetectionService _service;

    public DuplicateDetectionServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"DuplicateDetectionTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<DuplicateDetectionService>>();
        _service = new DuplicateDetectionService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenNoActiveRulesExist()
    {
        var fields = new Dictionary<string, string?>
        {
            ["email"] = "test@example.com",
            ["firstName"] = "John"
        };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Should().NotBeNull();
        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenNoRulesForEntityType()
    {
        // Add a rule for Lead, but check Contact
        _context.DuplicateRules.Add(new DuplicateRule
        {
            Id = 1,
            Name = "Lead Email Rule",
            IsActive = true,
            EntityType = DuplicateEntityType.Lead,
            MatchThreshold = 80,
            Action = DuplicateAction.Block,
            RunOnCreate = true,
            RunOnUpdate = false,
            RunOnImport = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var fields = new Dictionary<string, string?> { ["email"] = "test@example.com" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnEmpty_WhenRuleIsInactive()
    {
        _context.DuplicateRules.Add(new DuplicateRule
        {
            Id = 2,
            Name = "Inactive Rule",
            IsActive = false,
            EntityType = DuplicateEntityType.Contact,
            MatchThreshold = 80,
            Action = DuplicateAction.Warn,
            RunOnCreate = true,
            RunOnUpdate = false,
            RunOnImport = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var fields = new Dictionary<string, string?> { ["email"] = "test@example.com" };

        var result = await _service.CheckForDuplicatesAsync("Contact", fields, excludeRecordId: null);

        result.Duplicates.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldReturnNonNull_WithEmptyFields()
    {
        var result = await _service.CheckForDuplicatesAsync("Lead", new Dictionary<string, string?>(), null);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckForDuplicatesAsync_ShouldHandleNullFieldValues()
    {
        var fields = new Dictionary<string, string?>
        {
            ["email"] = null,
            ["phone"] = null
        };

        var act = async () => await _service.CheckForDuplicatesAsync("Account", fields, null);
        await act.Should().NotThrowAsync();
    }
}
"""

files["Services/SampleDataSeederServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class SampleDataSeederServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<SampleDataSeederService>> _mockLogger;
    private readonly IConfiguration _configuration;
    private readonly SampleDataSeederService _service;

    public SampleDataSeederServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SampleSeederTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<SampleDataSeederService>>();
        _configuration = new ConfigurationBuilder().Build();
        _service = new SampleDataSeederService(_context, _mockLogger.Object, _configuration);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var act = () => new SampleDataSeederService(_context, _mockLogger.Object, _configuration);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReturnResult()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAllSampleDataAsync_ShouldNotThrow()
    {
        var act = async () => await _service.SeedAllSampleDataAsync();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldReportSuccessOrPartialFailure()
    {
        var result = await _service.SeedAllSampleDataWithLogAsync();
        // Result should indicate some completion state
        result.Should().NotBeNull();
        // TotalSeeded should be non-negative
        result.TotalSeeded.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SeedAllSampleDataWithLogAsync_ShouldBeIdempotent()
    {
        // Running twice should not throw
        await _service.SeedAllSampleDataWithLogAsync();
        var act = async () => await _service.SeedAllSampleDataWithLogAsync();
        await act.Should().NotThrowAsync();
    }
}
"""

for rel_path, content in files.items():
    full_path = os.path.join(BASE, rel_path)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w") as f:
        f.write(content.lstrip("\n"))
    print(f"Written: {rel_path}")

print("Done.")
