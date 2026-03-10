// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ProviderHealthService (TCOV-015).
/// Lightweight stub service — tests verify safe-default returns.
/// </summary>
public class ProviderHealthServiceTests
{
    private readonly Mock<ILogger<ProviderHealthService>> _mockLogger;
    private readonly ProviderHealthService _service;

    public ProviderHealthServiceTests()
    {
        _mockLogger = new Mock<ILogger<ProviderHealthService>>();
        _service = new ProviderHealthService(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithLogger()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProviderHealthAsync_ShouldReturnNotConfiguredStatus_ForAnyProvider()
    {
        var result = await _service.GetProviderHealthAsync("Search", "Meilisearch");
        result.Should().NotBeNull();
        result.Status.Should().Be((int)ProviderHealthStatus.NotConfigured);
        result.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public async Task GetProviderHealthAsync_ShouldReturnCorrectProviderInfo()
    {
        var result = await _service.GetProviderHealthAsync("AI", "Ollama");
        result.Category.Should().Be("AI");
        result.ProviderName.Should().Be("Ollama");
    }

    [Fact]
    public async Task GetCategoryProvidersHealthAsync_ShouldReturnEmpty()
    {
        var result = await _service.GetCategoryProvidersHealthAsync("Search");
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllProvidersHealthAsync_ShouldReturnEmptyDictionary()
    {
        var result = await _service.GetAllProvidersHealthAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProviderHealthDashboardAsync_ShouldReturnDashboardDto()
    {
        var result = await _service.GetProviderHealthDashboardAsync();
        result.Should().NotBeNull();
        result.LastRefreshAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PerformProviderHealthCheckAsync_ShouldReturnNotConfiguredStatus()
    {
        var result = await _service.PerformProviderHealthCheckAsync("Notifications", "Novu");
        result.Should().NotBeNull();
        result.Status.Should().Be((int)ProviderHealthStatus.NotConfigured);
        result.ResponseTimeMs.Should().Be(0);
    }

    [Fact]
    public async Task PerformAllHealthChecksAsync_ShouldReturnEmptyDictionary()
    {
        var result = await _service.PerformAllHealthChecksAsync();
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
