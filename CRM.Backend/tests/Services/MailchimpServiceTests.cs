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
/// Unit tests for MailchimpService — INT-002.
/// </summary>
public class MailchimpServiceTests
{
    private static MailchimpService BuildService(MailchimpOptions? opts = null)
    {
        var options = Options.Create(opts ?? new MailchimpOptions
        {
            Enabled = true,
            ApiKey = "testkey-us1",
            ListId = "abc123",
            ServerPrefix = "us1"
        });

        var db = new Mock<ICrmDbContext>().Object;
        var logger = new Mock<ILogger<MailchimpService>>().Object;
        var httpClient = new HttpClient();

        return new MailchimpService(options, db, logger, httpClient);
    }

    // ------------------------------------------------------------------ //
    //  SyncContactAsync — disabled guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new MailchimpOptions { Enabled = false });

        // Act
        var result = await svc.SyncContactAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SyncContactAsync_ReturnsFalse_WhenApiKeyEmpty()
    {
        // Arrange
        var svc = BuildService(new MailchimpOptions { Enabled = true, ApiKey = string.Empty });

        // Act
        var result = await svc.SyncContactAsync(1);

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
        var svc = BuildService(new MailchimpOptions { Enabled = false });

        // Act
        var count = await svc.SyncAllContactsAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task SyncAllContactsAsync_ReturnsZero_WhenApiKeyEmpty()
    {
        // Arrange
        var svc = BuildService(new MailchimpOptions { Enabled = true, ApiKey = string.Empty });

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
        var svc = BuildService(new MailchimpOptions { Enabled = false });

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeFalse();
        status.ListName.Should().BeNull();
        status.MemberCount.Should().BeNull();
    }

    [Fact]
    public async Task GetConnectionStatus_ReturnsNotConnected_WhenApiKeyEmpty()
    {
        // Arrange
        var svc = BuildService(new MailchimpOptions { Enabled = true, ApiKey = string.Empty });

        // Act
        var status = await svc.GetConnectionStatusAsync();

        // Assert
        status.IsConnected.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  UnsubscribeContactAsync — disabled guard
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task UnsubscribeContactAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new MailchimpOptions { Enabled = false });

        // Act
        var result = await svc.UnsubscribeContactAsync("test@example.com");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnsubscribeContactAsync_ReturnsFalse_WhenEmailEmpty()
    {
        // Arrange — enabled but empty email
        var svc = BuildService(new MailchimpOptions { Enabled = true, ApiKey = "key-us1", ListId = "list1" });

        // Act
        var result = await svc.UnsubscribeContactAsync(string.Empty);

        // Assert
        result.Should().BeFalse();
    }
}
