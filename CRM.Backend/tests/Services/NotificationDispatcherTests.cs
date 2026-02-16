// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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

        _notificationPort
            .Setup(port => port.TriggerWorkflowAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotificationResult { Success = true });

        var dispatcher = CreateDispatcher();
        await dispatcher.DispatchAsync("workflow", payload);

        _notificationPort.Verify(port => port.TriggerWorkflowAsync(
            "wf-1",
            "sub-1",
            It.Is<object>(obj => obj is JsonElement element
                && element.TryGetProperty("ticketId", out var value)
                && value.GetInt32() == 42),
            It.IsAny<CancellationToken>()),
            Times.Once);
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
