// CRM Solution — Unit Tests
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for RelationshipService (TCOV-030).</summary>
public class RelationshipServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly RelationshipService _service;

    public RelationshipServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);
        var logger = new Mock<ILogger<RelationshipService>>().Object;
        _service = new RelationshipService(_context, logger);
    }

    public void Dispose() => _context.Dispose();

    // ── GetRelationshipTypesAsync ────────────────────────────────────────────
    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldReturnEmpty_WhenNoneExist()
    {
        var result = await _service.GetRelationshipTypesAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldExcludeDeletedTypes()
    {
        _context.RelationshipTypes.AddRange(
            new RelationshipType { Id = 1, TypeName = "Parent-Child", IsActive = true, IsDeleted = false },
            new RelationshipType { Id = 2, TypeName = "Deleted", IsActive = true, IsDeleted = true }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypesAsync();
        result.Should().HaveCount(1);
        result[0].TypeName.Should().Be("Parent-Child");
    }

    [Fact]
    public async Task GetRelationshipTypesAsync_ShouldIncludeInactive_WhenFlagSet()
    {
        _context.RelationshipTypes.AddRange(
            new RelationshipType { Id = 1, TypeName = "Active", IsActive = true, IsDeleted = false },
            new RelationshipType { Id = 2, TypeName = "Inactive", IsActive = false, IsDeleted = false }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypesAsync(includeInactive: true);
        result.Should().HaveCount(2);
    }

    // ── GetRelationshipTypeAsync ─────────────────────────────────────────────
    [Fact]
    public async Task GetRelationshipTypeAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetRelationshipTypeAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRelationshipTypeAsync_ShouldReturnDto_WhenFound()
    {
        _context.RelationshipTypes.Add(new RelationshipType
        {
            Id = 5,
            TypeName = "Subsidiary",
            IsActive = true,
            IsDeleted = false,
            IsBidirectional = true
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetRelationshipTypeAsync(5);
        result.Should().NotBeNull();
        result!.TypeName.Should().Be("Subsidiary");
    }

    // ── CreateRelationshipTypeAsync ──────────────────────────────────────────
    [Fact]
    public async Task CreateRelationshipTypeAsync_ShouldPersistNewType()
    {
        var dto = new RelationshipTypeCreateDto
        {
            TypeName = "Vendor",
            IsActive = true,
            IsBidirectional = false
        };

        var result = await _service.CreateRelationshipTypeAsync(dto);
        result.Should().NotBeNull();
        result.TypeName.Should().Be("Vendor");
        _context.RelationshipTypes.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateRelationshipTypeAsync_ShouldUpdateExisting_WhenSameNameExists()
    {
        _context.RelationshipTypes.Add(new RelationshipType
        {
            Id = 1,
            TypeName = "Partner",
            TypeCategory = "old cat",
            IsActive = true,
            IsDeleted = false
        });
        await _context.SaveChangesAsync();

        var dto = new RelationshipTypeCreateDto
        {
            TypeName = "Partner",
            TypeCategory = "new cat",
            IsActive = true
        };

        var result = await _service.CreateRelationshipTypeAsync(dto);
        result.TypeCategory.Should().Be("new cat");
        _context.RelationshipTypes.Count().Should().Be(1); // no new row
    }
}
