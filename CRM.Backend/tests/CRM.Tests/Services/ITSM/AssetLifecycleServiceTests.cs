// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class AssetLifecycleServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<AssetLifecycleService>> _mockLogger;
    private readonly AssetLifecycleService _service;

    public AssetLifecycleServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AssetLifecycleTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<AssetLifecycleService>>();
        _service = new AssetLifecycleService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private async Task<ConfigurationItem> SeedCIAsync(OperationalStatus status = OperationalStatus.InStock)
    {
        var ci = new ConfigurationItem
        {
            CIName = $"Server-{Guid.NewGuid():N}".Substring(0, 16),
            CINumber = $"CI{Guid.NewGuid():N}".Substring(0, 10),
            CIType = CIType.Server,
            OperationalStatus = status,
            CreatedAt = DateTime.UtcNow
        };
        _context.ConfigurationItems.Add(ci);
        await _context.SaveChangesAsync();
        return ci;
    }

    [Fact]
    public async Task GetLifecycleStateAsync_ShouldThrow_WhenCINotFound()
    {
        var act = async () => await _service.GetLifecycleStateAsync(999999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetLifecycleStateAsync_ShouldReturnCorrectStage_WhenCIExists()
    {
        var ci = await SeedCIAsync(OperationalStatus.Operational);

        var result = await _service.GetLifecycleStateAsync(ci.CIId);

        result.Should().NotBeNull();
        result.ConfigurationItemId.Should().Be(ci.CIId);
        result.CurrentStage.Should().Be(LifecycleStage.Deployed);
    }

    [Fact]
    public async Task GetLifecycleStateAsync_ShouldMapInStockStatusToInStockStage()
    {
        var ci = await SeedCIAsync(OperationalStatus.InStock);

        var result = await _service.GetLifecycleStateAsync(ci.CIId);

        result.CurrentStage.Should().Be(LifecycleStage.InStock);
    }

    [Fact]
    public async Task GetLifecycleStateAsync_ShouldIncludeAllowedTransitions()
    {
        var ci = await SeedCIAsync(OperationalStatus.InStock);

        var result = await _service.GetLifecycleStateAsync(ci.CIId);

        result.AllowedTransitions.Should().NotBeEmpty();
        result.AllowedTransitions.Should().Contain(LifecycleStage.Deployed);
    }

    [Fact]
    public async Task TransitionAsync_ShouldThrow_WhenCINotFound()
    {
        var act = async () => await _service.TransitionAsync(999999, LifecycleStage.Deployed, performedById: 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task TransitionAsync_ShouldThrow_WhenTransitionNotAllowed()
    {
        // Operational → Ordered is not allowed (Deployed doesn't permit Ordered)
        var ci = await SeedCIAsync(OperationalStatus.Operational);

        var act = async () => await _service.TransitionAsync(ci.CIId, LifecycleStage.Ordered, performedById: 1);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task TransitionAsync_ShouldUpdateCIOperationalStatus_WhenTransitionAllowed()
    {
        // InStock → Deployed is allowed
        var ci = await SeedCIAsync(OperationalStatus.InStock);

        var result = await _service.TransitionAsync(ci.CIId, LifecycleStage.Deployed, performedById: 1, notes: "Going live");

        result.Should().NotBeNull();
        result.FromStage.Should().Be(LifecycleStage.InStock);
        result.ToStage.Should().Be(LifecycleStage.Deployed);

        var updated = await _context.ConfigurationItems.FindAsync(ci.CIId);
        updated!.OperationalStatus.Should().Be(OperationalStatus.Operational);
    }

    [Fact]
    public async Task GetLifecycleHistoryAsync_ShouldReturnTransitions_ForAsset()
    {
        var ci = await SeedCIAsync(OperationalStatus.InStock);

        // Do a valid transition: InStock → Deployed
        await _service.TransitionAsync(ci.CIId, LifecycleStage.Deployed, performedById: 1);

        var history = await _service.GetLifecycleHistoryAsync(ci.CIId);

        history.Should().NotBeEmpty();
        history[0].ConfigurationItemId.Should().Be(ci.CIId);
        history[0].ToStage.Should().Be(LifecycleStage.Deployed);
    }

    [Fact]
    public async Task GetEndOfLifeAlertsAsync_ShouldReturnAlert_WhenWarrantyExpiringSoon()
    {
        var ci = new ConfigurationItem
        {
            CIName = $"WarrantyCI-{Guid.NewGuid():N}".Substring(0, 18),
            CINumber = $"CI{Guid.NewGuid():N}".Substring(0, 10),
            CIType = CIType.Server,
            OperationalStatus = OperationalStatus.Operational,
            WarrantyExpiration = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        _context.ConfigurationItems.Add(ci);
        await _context.SaveChangesAsync();

        var alerts = await _service.GetEndOfLifeAlertsAsync(daysAhead: 90);

        alerts.Should().Contain(a => a.ConfigurationItemId == ci.CIId && a.Type == AlertType.WarrantyExpiring);
    }
}
