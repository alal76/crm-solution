// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Threading.Tasks;
using CRM.Infrastructure.Services.Integrations;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using AccountingSyncStatus = CRM.Core.Ports.Input.AccountingSyncStatus;

namespace CRM.Tests.Services.Integrations;

/// <summary>
/// Unit tests for <see cref="AccountingSyncService"/> (stub implementation).
/// The service is a placeholder that returns explicit failure / stub results for
/// every operation until a real QuickBooks/Xero SDK is integrated.
/// </summary>
public class AccountingSyncServiceTests
{
    private static AccountingSyncService BuildService(IConfiguration? config = null)
    {
        config ??= new ConfigurationBuilder().Build();
        return new AccountingSyncService(config, Mock.Of<ILogger<AccountingSyncService>>());
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncAccountAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncAccountAsync_ReturnsFailedResult()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.SyncAccountAsync(accountId: 1);

        // Assert — stub always returns failed
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SyncAccountAsync_IncludesProviderInResult()
    {
        // Arrange — configure QuickBooks provider
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string?>(
                    "Integrations:Accounting:Provider", "QuickBooks")
            })
            .Build();
        var service = BuildService(config);

        // Act
        var result = await service.SyncAccountAsync(1);

        // Assert
        result.Provider.Should().Be("QuickBooks");
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncInvoiceAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncInvoiceAsync_ReturnsFailedResult()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.SyncInvoiceAsync(invoiceId: 42);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    // ──────────────────────────────────────────────────────────────────
    // SyncPaymentAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SyncPaymentAsync_ReturnsFailedResult()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.SyncPaymentAsync("ext-pay-001");

        // Assert
        result.Success.Should().BeFalse();
    }

    // ──────────────────────────────────────────────────────────────────
    // GetSyncStatusAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSyncStatusAsync_ReturnsNotConfiguredStatus()
    {
        // Arrange
        var service = BuildService();

        // Act
        AccountingSyncStatus? status = await service.GetSyncStatusAsync("Account", 7);

        // Assert
        status.Should().NotBeNull();
        status!.Status.Should().Be("NotConfigured");
        status.EntityType.Should().Be("Account");
        status.EntityId.Should().Be(7);
    }

    // ──────────────────────────────────────────────────────────────────
    // RunBatchSyncAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task RunBatchSyncAsync_ReturnsEmptySummary()
    {
        // Arrange
        var service = BuildService();

        // Act
        var result = await service.RunBatchSyncAsync();

        // Assert
        result.TotalProcessed.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.Errors.Should().HaveCountGreaterOrEqualTo(1); // stub error message
    }

    // ──────────────────────────────────────────────────────────────────
    // TestConnectionAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TestConnectionAsync_WhenApiKeyNotConfigured_ReturnsFalse()
    {
        // Arrange — empty config → no API key
        var service = BuildService();

        // Act
        var connected = await service.TestConnectionAsync();

        // Assert
        connected.Should().BeFalse();
    }

    [Fact]
    public async Task TestConnectionAsync_WhenApiKeyConfigured_ReturnsFalseUntilSdkInstalled()
    {
        // Arrange — provide an API key
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string?>(
                    "Integrations:Accounting:QuickBooks:ApiKey", "test-api-key")
            })
            .Build();
        var service = BuildService(config);

        // Act
        var connected = await service.TestConnectionAsync();

        // Assert — stub always returns false even with key (SDK not installed)
        connected.Should().BeFalse();
    }
}
