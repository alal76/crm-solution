// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInAnalyticsProvider"/> (TCOV-053).</summary>
public class BuiltInAnalyticsProviderTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<BuiltInAnalyticsProvider>> _loggerMock = new();

    private BuiltInAnalyticsProvider Create() =>
        new(_dbContextMock.Object, _loggerMock.Object);

    // ─── Constructor ────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullContext_ShouldThrow()
    {
        var act = () => new BuiltInAnalyticsProvider(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new BuiltInAnalyticsProvider(_dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => Create();
        act.Should().NotThrow();
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        Create().ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public void SupportsEmbedding_ShouldBeFalse()
    {
        Create().SupportsEmbedding.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeTrue();
    }

    // ─── Dashboard Operations ────────────────────────────────────────────────────
    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnPredefinedDashboards()
    {
        var dashboards = (await Create().GetDashboardsAsync()).ToList();
        dashboards.Should().NotBeEmpty();
        dashboards.All(d => !string.IsNullOrEmpty(d.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardAsync_ExistingId_ShouldReturnDashboard()
    {
        var all = (await Create().GetDashboardsAsync()).ToList();
        var first = all.First();
        var dashboard = await Create().GetDashboardAsync(first.Id);
        dashboard.Should().NotBeNull();
        dashboard!.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task GetDashboardAsync_UnknownId_ShouldReturnNull()
    {
        var result = await Create().GetDashboardAsync("does-not-exist");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_ShouldReturnAllDashboards()
    {
        var result = (await Create().GetDashboardsForUserAsync(42)).ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEmbedAsync_ShouldReturnUnsupportedResult()
    {
        var request = new CRM.Core.Ports.Output.Providers.EmbedRequest { ResourceId = "overview" };
        var result = await Create().GetEmbedAsync(request);
        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
    }
}
