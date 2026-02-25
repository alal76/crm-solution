// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TODO-SD005-010: Unit tests for Slack/Teams ITSM notification integration.

using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for Slack/Teams ITSM notification channel implementations and dispatcher (TODO-SD005-010).
/// </summary>
public class ItsmNotificationTests
{
    // ──────────────────────────────────────────────────────────────────────
    // SlackItsmNotificationService
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void SlackService_IsConfigured_ReturnsFalse_WhenWebhookUrlMissing()
    {
        var config = new ConfigurationBuilder().Build(); // no keys
        var service = new SlackItsmNotificationService(
            new System.Net.Http.HttpClient(),
            config,
            NullLogger<SlackItsmNotificationService>.Instance);

        service.IsConfigured.Should().BeFalse("Slack channel should not be configured when WebhookUrl is absent");
    }

    [Fact]
    public void SlackService_IsConfigured_ReturnsTrue_WhenWebhookUrlPresent()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ItsmNotifications:Slack:WebhookUrl"] = "https://hooks.slack.com/services/T00/B00/XXX"
            })
            .Build();

        var service = new SlackItsmNotificationService(
            new System.Net.Http.HttpClient(),
            config,
            NullLogger<SlackItsmNotificationService>.Instance);

        service.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void SlackService_ChannelName_IsSlack()
    {
        var service = new SlackItsmNotificationService(
            new System.Net.Http.HttpClient(),
            new ConfigurationBuilder().Build(),
            NullLogger<SlackItsmNotificationService>.Instance);

        service.ChannelName.Should().Be("Slack");
    }

    // ──────────────────────────────────────────────────────────────────────
    // TeamsItsmNotificationService
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public void TeamsService_IsConfigured_ReturnsFalse_WhenWebhookUrlMissing()
    {
        var config = new ConfigurationBuilder().Build();
        var service = new TeamsItsmNotificationService(
            new System.Net.Http.HttpClient(),
            config,
            NullLogger<TeamsItsmNotificationService>.Instance);

        service.IsConfigured.Should().BeFalse("Teams channel should not be configured when WebhookUrl is absent");
    }

    [Fact]
    public void TeamsService_ChannelName_IsTeams()
    {
        var service = new TeamsItsmNotificationService(
            new System.Net.Http.HttpClient(),
            new ConfigurationBuilder().Build(),
            NullLogger<TeamsItsmNotificationService>.Instance);

        service.ChannelName.Should().Be("Teams");
    }

    // ──────────────────────────────────────────────────────────────────────
    // ItsmNotificationDispatcher
    // ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Dispatcher_DispatchAsync_CallsAllConfiguredChannels()
    {
        // Arrange – two configured mock channels
        var channel1 = new Mock<IItsmNotificationChannel>();
        channel1.Setup(c => c.IsConfigured).Returns(true);
        channel1.Setup(c => c.ChannelName).Returns("MockCh1");

        var channel2 = new Mock<IItsmNotificationChannel>();
        channel2.Setup(c => c.IsConfigured).Returns(true);
        channel2.Setup(c => c.ChannelName).Returns("MockCh2");

        var dispatcher = new ItsmNotificationDispatcher(
            new[] { channel1.Object, channel2.Object },
            NullLogger<ItsmNotificationDispatcher>.Instance);

        // Act
        await dispatcher.DispatchAsync("Test Title", "Test Body", "High", serviceRequestId: 42);

        // Assert — both channels should have received the notification
        channel1.Verify(c => c.SendNotificationAsync("Test Title", "Test Body", "High", 42, It.IsAny<CancellationToken>()), Times.Once);
        channel2.Verify(c => c.SendNotificationAsync("Test Title", "Test Body", "High", 42, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Dispatcher_DispatchAsync_SkipsUnconfiguredChannels()
    {
        // Arrange – one configured, one not
        var configured = new Mock<IItsmNotificationChannel>();
        configured.Setup(c => c.IsConfigured).Returns(true);
        configured.Setup(c => c.ChannelName).Returns("Active");

        var unconfigured = new Mock<IItsmNotificationChannel>();
        unconfigured.Setup(c => c.IsConfigured).Returns(false);
        unconfigured.Setup(c => c.ChannelName).Returns("Inactive");

        var dispatcher = new ItsmNotificationDispatcher(
            new[] { configured.Object, unconfigured.Object },
            NullLogger<ItsmNotificationDispatcher>.Instance);

        await dispatcher.DispatchAsync("T", "B", "Low");

        configured.Verify(c => c.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
        unconfigured.Verify(c => c.SendNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Dispatcher_DispatchAsync_DoesNotThrow_WhenNoChannelsPresent()
    {
        var dispatcher = new ItsmNotificationDispatcher(
            Enumerable.Empty<IItsmNotificationChannel>(),
            NullLogger<ItsmNotificationDispatcher>.Instance);

        // Should complete without throwing
        var act = async () => await dispatcher.DispatchAsync("T", "B", "Medium");
        await act.Should().NotThrowAsync();
    }
}
