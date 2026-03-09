// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for HubSpotService — INT-002.
/// </summary>
public class HubSpotServiceTests
{
    private static HubSpotService BuildService(HubSpotOptions? opts = null)
    {
        var options = Options.Create(opts ?? new HubSpotOptions
        {
            Enabled = true,
            AccessToken = "pat-test-token"
        });

        var db = new Mock<ICrmDbContext>().Object;
        var logger = new Mock<ILogger<HubSpotService>>().Object;
        var httpClient = new HttpClient();

        return new HubSpotService(options, db, logger, httpClient);
    }

    // ------------------------------------------------------------------ //
    //  SyncContactAsync — disabled guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = false });

        // Act
        var result = await svc.SyncContactAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = true, AccessToken = string.Empty });

        // Act
        var result = await svc.SyncContactAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  SyncDealAsync — disabled guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncDealAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = false });

        // Act
        var result = await svc.SyncDealAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncDealAsync_ReturnsFalse_WhenAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = true, AccessToken = string.Empty });

        // Act
        var result = await svc.SyncDealAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  SyncAllContactsAsync — disabled guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncAllContactsAsync_ReturnsZero_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = false });

        // Act
        var count = await svc.SyncAllContactsAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task SyncAllContactsAsync_ReturnsZero_WhenAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = true, AccessToken = string.Empty });

        // Act
        var count = await svc.SyncAllContactsAsync();

        // Assert
        count.Should().Be(0);
    }

    // ------------------------------------------------------------------ //
    //  GetConnectionStatusAsync — disabled guard
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetConnectionStatus_ReturnsNotConnected_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = false });

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeFalse();
        status.PortalId.Should().BeNull();
        status.AccountName.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStatus_ReturnsNotConnected_WhenAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new HubSpotOptions { Enabled = true, AccessToken = string.Empty });

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeFalse();
    }
}
