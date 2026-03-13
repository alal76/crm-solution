// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for TerritoryAssignmentService.
/// Uses EF InMemory database to seed Territory records.
/// </summary>
public class TerritoryAssignmentServiceTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"TerritoryAssignment_{Guid.NewGuid()}")
                .Options,
            null!);

    private static TerritoryAssignmentService CreateService(CrmDbContext db) =>
        new TerritoryAssignmentService(db, new Mock<ILogger<TerritoryAssignmentService>>().Object);

    // ── GetActiveTerritoriesAsync ────────────────────────────────────────────

    [Fact]
    public async Task GetActiveTerritoriesAsync_ShouldReturnOnlyActiveNonDeletedTerritories()
    {
        using var db = CreateDb();
        db.Territories.AddRange(
            new Territory { Id = 1, Name = "West", Code = "WEST", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Territory { Id = 2, Name = "East", Code = "EAST", IsActive = false, IsDeleted = false, CreatedAt = DateTime.UtcNow },
            new Territory { Id = 3, Name = "North", Code = "NORTH", IsActive = true, IsDeleted = true, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetActiveTerritoriesAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("West");
    }

    // ── CreateTerritoryAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateTerritoryAsync_ShouldPersistTerritoryAndReturnId()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var territory = new Territory
        {
            Name = "South", Code = "SOUTH",
            IsActive = true, IsDeleted = false
        };

        var id = await service.CreateTerritoryAsync(territory);

        id.Should().BeGreaterThan(0);
        db.Territories.Count().Should().Be(1);
        db.Territories.First().Name.Should().Be("South");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTerritory_WhenExists()
    {
        using var db = CreateDb();
        db.Territories.Add(new Territory { Id = 5, Name = "Central", Code = "CTR", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetByIdAsync(5);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Central");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.GetByIdAsync(9999);

        result.Should().BeNull();
    }

    // ── UpdateTerritoryAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTerritoryAsync_ShouldReturnTrue_AndApplyChanges()
    {
        using var db = CreateDb();
        db.Territories.Add(new Territory { Id = 10, Name = "OldName", Code = "OLD", IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var update = new Territory { Id = 10, Name = "NewName", Code = "NEW", IsActive = false };
        var result = await service.UpdateTerritoryAsync(update);

        result.Should().BeTrue();
        var territory = db.Territories.First();
        territory.Name.Should().Be("NewName");
        territory.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateTerritoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.UpdateTerritoryAsync(new Territory { Id = 9999, Name = "X", Code = "X" });

        result.Should().BeFalse();
    }

    // ── AssignLeadToTerritoryAsync ───────────────────────────────────────────

    [Fact]
    public async Task AssignLeadToTerritoryAsync_ShouldReturnNull_WhenNoActiveTerritoriesExist()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var lead = new Lead { Id = 1, FirstName = "Jane", LastName = "Doe", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var result = await service.AssignLeadToTerritoryAsync(lead);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AssignLeadToTerritoryAsync_ShouldAssignLead_WhenActiveTerritoryExists()
    {
        using var db = CreateDb();
        db.Territories.Add(new Territory
        {
            Id = 1, Name = "West", Code = "WEST",
            OwnerUserId = 42, IsActive = true, IsDeleted = false, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var lead = new Lead { Id = 1, FirstName = "John", LastName = "Smith", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Leads.Add(lead);
        await db.SaveChangesAsync();

        var ownerId = await service.AssignLeadToTerritoryAsync(lead);

        ownerId.Should().Be(42);
        lead.OwnerId.Should().Be(42);
        lead.Region.Should().Be("West");
    }
}
