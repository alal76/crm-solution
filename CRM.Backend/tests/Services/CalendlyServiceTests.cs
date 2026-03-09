// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
// Unit Tests: CalendlyService
//
// Verified from source before writing:
//   Class: CalendlyService, Namespace: CRM.Infrastructure.Services.Integrations
//   Interface: ICalendlyService (CRM.Core.Interfaces)
//   Constructor: (IOptions<CalendlyOptions>, HttpClient, ILogger<CalendlyService>)
//   RegisterWebhookAsync(webhookUrl, ct):
//     Returns false when Enabled=false
//     Returns false when PersonalAccessToken is empty
//     Makes POST to https://api.calendly.com/webhook_subscriptions when configured
//   GetUpcomingEventsAsync(ct):
//     Returns empty enumerable when Enabled=false
//     Returns empty enumerable when PersonalAccessToken is empty
//     Makes GET to /users/me then /scheduled_events when configured
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
/// Unit tests for <see cref="CalendlyService"/> — INT-004.
/// </summary>
public class CalendlyServiceTests
{
    private static CalendlyService BuildService(CalendlyOptions? opts = null, HttpClient? client = null)
    {
        var options = Options.Create(opts ?? new CalendlyOptions
        {
            Enabled = true,
            PersonalAccessToken = "test_token_abc",
            WebhookSigningKey = "test_signing_key"
        });
        var logger = new Mock<ILogger<CalendlyService>>().Object;
        return new CalendlyService(options, client ?? new HttpClient(), logger);
    }

    // ------------------------------------------------------------------ //
    //  RegisterWebhookAsync — disabled / misconfigured guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsFalse_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new CalendlyOptions { Enabled = false });

        // Act
        var result = await svc.RegisterWebhookAsync("https://example.com/api/webhooks/calendly");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsFalse_WhenPersonalAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new CalendlyOptions
        {
            Enabled = true,
            PersonalAccessToken = string.Empty
        });

        // Act
        var result = await svc.RegisterWebhookAsync("https://example.com/api/webhooks/calendly");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterWebhookAsync_ReturnsFalse_WhenHttpClientThrows()
    {
        // Arrange — HttpClient with no base address will throw on POST
        var svc = BuildService(new CalendlyOptions
        {
            Enabled = true,
            PersonalAccessToken = "valid_token"
        }, new HttpClient()); // No server → will fail gracefully

        // Act — should return false rather than propagating the exception
        var result = await svc.RegisterWebhookAsync("https://example.com/api/webhooks/calendly");

        // Assert — graceful degradation
        result.Should().BeFalse();
    }

    // ------------------------------------------------------------------ //
    //  GetUpcomingEventsAsync — disabled / misconfigured guards
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmpty_WhenNotEnabled()
    {
        // Arrange
        var svc = BuildService(new CalendlyOptions { Enabled = false });

        // Act
        var result = await svc.GetUpcomingEventsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmpty_WhenPersonalAccessTokenEmpty()
    {
        // Arrange
        var svc = BuildService(new CalendlyOptions
        {
            Enabled = true,
            PersonalAccessToken = string.Empty
        });

        // Act
        var result = await svc.GetUpcomingEventsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsEmpty_WhenHttpClientThrows()
    {
        // Arrange — HttpClient will throw when calling Calendly (no server available)
        var svc = BuildService(new CalendlyOptions
        {
            Enabled = true,
            PersonalAccessToken = "valid_token"
        }, new HttpClient()); // Will fail gracefully

        // Act
        var result = await svc.GetUpcomingEventsAsync();

        // Assert — graceful degradation: empty list, no exception thrown
        result.Should().BeEmpty();
    }

    // ------------------------------------------------------------------ //
    //  CalendlyEventDto — basic property coverage
    // ------------------------------------------------------------------ //

    [Fact]
    public void CalendlyEventDto_PropertiesSetAndReadCorrectly()
    {
        // Arrange & Act
        var dto = new CalendlyEventDto
        {
            EventName = "Discovery Call",
            InviteeName = "Alice Smith",
            InviteeEmail = "alice@example.com",
            StartTime = new DateTime(2026, 3, 20, 14, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2026, 3, 20, 14, 30, 0, DateTimeKind.Utc),
            JoinUrl = "https://zoom.us/j/987654321"
        };

        // Assert
        dto.EventName.Should().Be("Discovery Call");
        dto.InviteeName.Should().Be("Alice Smith");
        dto.InviteeEmail.Should().Be("alice@example.com");
        dto.StartTime.Should().Be(new DateTime(2026, 3, 20, 14, 0, 0, DateTimeKind.Utc));
        dto.EndTime.Should().Be(new DateTime(2026, 3, 20, 14, 30, 0, DateTimeKind.Utc));
        dto.JoinUrl.Should().Be("https://zoom.us/j/987654321");
    }
}
