// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Services.Notifications;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class NotificationDispatcherTests
{
    private readonly Mock<INotificationPort> _notificationPort = new();
    private readonly Mock<ILogger<NotificationDispatcher>> _logger = new();

    private NotificationDispatcher CreateDispatcher()
    {
        return new NotificationDispatcher(_notificationPort.Object, _logger.Object);
    }

    [Fact]
    public async Task DispatchAsync_EmailPayload_CallsNotificationPort()
    {
        var request = new EmailNotificationRequest
        {
            To = "user@crm.local",
            Subject = "Hello",
            Body = "Test"
        };
        var payload = JsonSerializer.Serialize(request);

        _notificationPort
            .Setup(port => port.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var dispatcher = CreateDispatcher();
        await dispatcher.DispatchAsync("email", payload);

        _notificationPort.Verify(port => port.SendEmailAsync(
            It.Is<EmailNotificationRequest>(r => r.To == "user@crm.local" && r.Subject == "Hello"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WorkflowPayload_CallsWorkflowTrigger()
    {
        var payload = JsonSerializer.Serialize(new
        {
            workflowId = "wf-1",
            subscriberId = "sub-1",
            payload = new { ticketId = 42 }
        });

        object? capturedPayload = null;
        _notificationPort
            .Setup(port => port.TriggerWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, object, CancellationToken>((_, _, payload, _) => capturedPayload = payload)
            .ReturnsAsync(new NotificationResult { Success = true });

        var dispatcher = CreateDispatcher();
        await dispatcher.DispatchAsync("workflow", payload);

        _notificationPort.Verify(port => port.TriggerWorkflowAsync(
            "wf-1",
            "sub-1",
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify captured payload separately (outside expression tree)
        capturedPayload.Should().NotBeNull();
        var element = (JsonElement)capturedPayload!;
        element.TryGetProperty("ticketId", out var ticketIdValue).Should().BeTrue();
        ticketIdValue.GetInt32().Should().Be(42);
    }

    [Fact]
    public async Task DispatchBatchAsync_Emails_DispatchesEachPayload()
    {
        var payloads = new[]
        {
            JsonSerializer.Serialize(new EmailNotificationRequest
            {
                To = "first@crm.local",
                Subject = "One",
                Body = "Body"
            }),
            JsonSerializer.Serialize(new EmailNotificationRequest
            {
                To = "second@crm.local",
                Subject = "Two",
                Body = "Body"
            })
        };

        _notificationPort
            .Setup(port => port.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var dispatcher = CreateDispatcher();
        await dispatcher.DispatchBatchAsync("email", payloads);

        _notificationPort.Verify(port => port.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task DispatchAsync_UnknownChannel_Throws()
    {
        var dispatcher = CreateDispatcher();
        var act = async () => await dispatcher.DispatchAsync("unknown", "{}");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unsupported notification channel: unknown");
    }
}
