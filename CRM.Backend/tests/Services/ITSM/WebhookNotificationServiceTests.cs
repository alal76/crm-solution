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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Services.ITSM;

public class WebhookNotificationServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<WebhookNotificationService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly WebhookNotificationService _service;

    public WebhookNotificationServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<WebhookNotificationService>>();
        _mockHttpHandler = new Mock<HttpMessageHandler>();

        // Setup mock HTTP client
        var httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };
        _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _service = new WebhookNotificationService(
            _mockHttpClientFactory.Object,
            _mockLogger.Object);
    }

    #region SendWebhookAsync Tests

    [Fact]
    public async Task SendWebhookAsync_WhenMatchingSubscription_SendsWebhook()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);

        var payload = new { IncidentId = 123, Title = "Test Incident" };

        // Act
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, payload);

        // Assert - verify HTTP request was made
        _mockHttpHandler.Protected()
            .Verify("SendAsync",
                Times.AtLeastOnce(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SendWebhookAsync_WhenNoMatchingSubscription_SkipsDelivery()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);

        // Act
        await _service.SendWebhookAsync(WebhookEventType.Unknown, new { });

        // Assert - no HTTP calls for unmatched event
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No webhook subscriptions found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SendWebhookAsync_IncludesSignatureHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { Test = "data" });

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Should().Contain(h => h.Key == "X-Webhook-Signature");
    }

    [Fact]
    public async Task SendWebhookAsync_IncludesEventTypeHeader()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        // Assert
        capturedRequest!.Headers.GetValues("X-Webhook-Event").Should().Contain("IncidentCreated");
    }

    [Fact]
    public async Task SendWebhookAsync_WhenHttpFails_RecordsFailure()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.InternalServerError);

        // Act
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        // Assert
        var history = await _service.GetDeliveryHistoryAsync();
        history.Should().Contain(d => !d.Success && d.ResponseStatusCode == 500);
    }

    [Fact]
    public async Task SendWebhookAsync_WhenHttpException_RecordsError()
    {
        // Arrange
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        // Assert
        var history = await _service.GetDeliveryHistoryAsync();
        history.Should().Contain(d => !d.Success && d.ErrorMessage!.Contains("Connection refused"));
    }

    #endregion

    #region CreateSubscriptionAsync Tests

    [Fact]
    public async Task CreateSubscriptionAsync_CreatesWithCorrectProperties()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Test Webhook",
            Description = "For testing purposes",
            TargetUrl = "https://api.example.com/webhook",
            EventTypes = new List<string> { "IncidentCreated", "IncidentClosed" },
            TimeoutSeconds = 60,
            RetryCount = 5
        };

        // Act
        var result = await _service.CreateSubscriptionAsync(dto, createdByUserId: 100);

        // Assert
        result.Name.Should().Be("Test Webhook");
        result.Description.Should().Be("For testing purposes");
        result.TargetUrl.Should().Be("https://api.example.com/webhook");
        result.EventTypes.Should().Contain("IncidentCreated", "IncidentClosed");
        result.TimeoutSeconds.Should().Be(60);
        result.RetryCount.Should().Be(5);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateSubscriptionAsync_GeneratesSecretWhenNotProvided()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Auto Secret",
            TargetUrl = "https://api.example.com",
            EventTypes = new List<string> { "IncidentCreated" }
        };

        // Act
        var result = await _service.CreateSubscriptionAsync(dto, createdByUserId: 1);

        // Assert
        result.Secret.Should().NotBeNullOrEmpty();
        result.Secret.Should().StartWith("whse"); // Masked secret starts with whse...
    }

    [Fact]
    public async Task CreateSubscriptionAsync_UsesProvidedSecret()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Custom Secret",
            TargetUrl = "https://api.example.com",
            EventTypes = new List<string> { "IncidentCreated" },
            Secret = "my_custom_secret_key_12345678"
        };

        // Act
        var result = await _service.CreateSubscriptionAsync(dto, createdByUserId: 1);

        // Assert
        result.Secret.Should().StartWith("my_c");
        result.Secret.Should().EndWith("5678");
    }

    [Fact]
    public async Task CreateSubscriptionAsync_LogsCreation()
    {
        // Arrange
        var dto = new CreateWebhookSubscriptionDto
        {
            Name = "Logging Test",
            TargetUrl = "https://api.example.com",
            EventTypes = new List<string>()
        };

        // Act
        await _service.CreateSubscriptionAsync(dto, createdByUserId: 50);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created webhook subscription")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetSubscriptionsAsync Tests

    [Fact]
    public async Task GetSubscriptionsAsync_ReturnsDefaultSubscriptions()
    {
        // Act
        var result = await _service.GetSubscriptionsAsync();

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(2);
        result.Should().Contain(s => s.Name == "Slack Notifications");
        result.Should().Contain(s => s.Name == "External Ticketing System");
    }

    [Fact]
    public async Task GetSubscriptionsAsync_ExcludesDeletedSubscriptions()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "To Delete",
            TargetUrl = "https://delete.me",
            EventTypes = new List<string>()
        }, 1);
        await _service.DeleteSubscriptionAsync(created.WebhookSubscriptionId);

        // Act
        var result = await _service.GetSubscriptionsAsync();

        // Assert
        result.Should().NotContain(s => s.Name == "To Delete");
    }

    #endregion

    #region GetSubscriptionByIdAsync Tests

    [Fact]
    public async Task GetSubscriptionByIdAsync_WhenExists_ReturnsSubscription()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "Find Me",
            TargetUrl = "https://find.me",
            EventTypes = new List<string> { "Test" }
        }, 1);

        // Act
        var result = await _service.GetSubscriptionByIdAsync(created.WebhookSubscriptionId);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Act
        var result = await _service.GetSubscriptionByIdAsync(9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSubscriptionByIdAsync_WhenDeleted_ReturnsNull()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "Deleted",
            TargetUrl = "https://deleted.example.com",
            EventTypes = new List<string>()
        }, 1);
        await _service.DeleteSubscriptionAsync(created.WebhookSubscriptionId);

        // Act
        var result = await _service.GetSubscriptionByIdAsync(created.WebhookSubscriptionId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region UpdateSubscriptionAsync Tests

    [Fact]
    public async Task UpdateSubscriptionAsync_UpdatesAllFields()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "Original",
            TargetUrl = "https://original.com",
            EventTypes = new List<string> { "Old" },
            TimeoutSeconds = 30,
            RetryCount = 3
        }, 1);

        var dto = new UpdateWebhookSubscriptionDto
        {
            Name = "Updated",
            Description = "New description",
            TargetUrl = "https://updated.com",
            EventTypes = new List<string> { "New1", "New2" },
            IsActive = false,
            TimeoutSeconds = 60,
            RetryCount = 5
        };

        // Act
        var result = await _service.UpdateSubscriptionAsync(created.WebhookSubscriptionId, dto, 2);

        // Assert
        result.Name.Should().Be("Updated");
        result.Description.Should().Be("New description");
        result.TargetUrl.Should().Be("https://updated.com");
        result.EventTypes.Should().Contain("New1", "New2");
        result.IsActive.Should().BeFalse();
        result.TimeoutSeconds.Should().Be(60);
        result.RetryCount.Should().Be(5);
    }

    [Fact]
    public async Task UpdateSubscriptionAsync_WhenNotFound_ThrowsException()
    {
        // Arrange
        var dto = new UpdateWebhookSubscriptionDto { Name = "Test" };

        // Act & Assert
        var act = () => _service.UpdateSubscriptionAsync(9999, dto, 1);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateSubscriptionAsync_PartialUpdate_PreservesOtherFields()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "Partial",
            TargetUrl = "https://partial.com",
            EventTypes = new List<string> { "Event1" },
            TimeoutSeconds = 30
        }, 1);

        var dto = new UpdateWebhookSubscriptionDto { Name = "Changed Name Only" };

        // Act
        var result = await _service.UpdateSubscriptionAsync(created.WebhookSubscriptionId, dto, 1);

        // Assert
        result.Name.Should().Be("Changed Name Only");
        result.TargetUrl.Should().Be("https://partial.com");
        result.TimeoutSeconds.Should().Be(30);
    }

    #endregion

    #region DeleteSubscriptionAsync Tests

    [Fact]
    public async Task DeleteSubscriptionAsync_WhenExists_ReturnsTrue()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "To Delete",
            TargetUrl = "https://delete.me",
            EventTypes = new List<string>()
        }, 1);

        // Act
        var result = await _service.DeleteSubscriptionAsync(created.WebhookSubscriptionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_WhenNotExists_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteSubscriptionAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSubscriptionAsync_SoftDeletes()
    {
        // Arrange
        var created = await _service.CreateSubscriptionAsync(new CreateWebhookSubscriptionDto
        {
            Name = "Soft Delete",
            TargetUrl = "https://soft.delete",
            EventTypes = new List<string>()
        }, 1);

        // Act
        await _service.DeleteSubscriptionAsync(created.WebhookSubscriptionId);
        var deleted = await _service.GetSubscriptionByIdAsync(created.WebhookSubscriptionId);

        // Assert
        deleted.Should().BeNull(); // Not visible via get
    }

    #endregion

    #region GetDeliveryHistoryAsync Tests

    [Fact]
    public async Task GetDeliveryHistoryAsync_ReturnsDeliveries()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        // Act
        var result = await _service.GetDeliveryHistoryAsync();

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDeliveryHistoryAsync_FiltersBySubscriptionId()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        var subs = await _service.GetSubscriptionsAsync();
        var targetId = subs.First().WebhookSubscriptionId;

        // Act
        var result = await _service.GetDeliveryHistoryAsync(targetId);

        // Assert
        result.Should().AllSatisfy(d => d.WebhookSubscriptionId.Should().Be(targetId));
    }

    [Fact]
    public async Task GetDeliveryHistoryAsync_SupportsPagination()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);

        // Send multiple webhooks
        for (int i = 0; i < 10; i++)
        {
            await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { Index = i });
        }

        // Act
        var page1 = await _service.GetDeliveryHistoryAsync(pageNumber: 1, pageSize: 5);
        var page2 = await _service.GetDeliveryHistoryAsync(pageNumber: 2, pageSize: 5);

        // Assert
        page1.Count().Should().BeLessOrEqualTo(5);
        page2.Count().Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task GetDeliveryHistoryAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.OK);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { First = true });
        await Task.Delay(10); // Small delay
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { Second = true });

        // Act
        var result = (await _service.GetDeliveryHistoryAsync()).ToList();

        // Assert
        if (result.Count >= 2)
        {
            result[0].CreatedAt.Should().BeOnOrAfter(result[1].CreatedAt);
        }
    }

    #endregion

    #region RetryDeliveryAsync Tests

    [Fact]
    public async Task RetryDeliveryAsync_WhenDeliveryNotFound_ReturnsFalse()
    {
        // Act
        var result = await _service.RetryDeliveryAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RetryDeliveryAsync_WhenSuccess_ReturnsTrue()
    {
        // Arrange - first create a failed delivery
        SetupMockHttpResponse(HttpStatusCode.InternalServerError);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        var deliveries = await _service.GetDeliveryHistoryAsync();
        var failedDelivery = deliveries.First(d => !d.Success);

        // Now setup success for retry
        SetupMockHttpResponse(HttpStatusCode.OK);

        // Act
        var result = await _service.RetryDeliveryAsync(failedDelivery.WebhookDeliveryId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RetryDeliveryAsync_IncrementsAttemptNumber()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.InternalServerError);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        var deliveries = (await _service.GetDeliveryHistoryAsync()).ToList();
        var originalDelivery = deliveries.First();
        var originalAttempt = originalDelivery.AttemptNumber;

        SetupMockHttpResponse(HttpStatusCode.OK);

        // Act
        await _service.RetryDeliveryAsync(originalDelivery.WebhookDeliveryId);

        // Assert
        var updatedDeliveries = (await _service.GetDeliveryHistoryAsync()).ToList();
        var retryDelivery = updatedDeliveries.First(d => d.AttemptNumber > originalAttempt);
        retryDelivery.AttemptNumber.Should().Be(originalAttempt + 1);
    }

    [Fact]
    public async Task RetryDeliveryAsync_AddsRetryHeader()
    {
        // Arrange
        SetupMockHttpResponse(HttpStatusCode.InternalServerError);
        await _service.SendWebhookAsync(WebhookEventType.IncidentCreated, new { });

        var deliveries = await _service.GetDeliveryHistoryAsync();
        var failedDelivery = deliveries.First();

        HttpRequestMessage? capturedRequest = null;
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        await _service.RetryDeliveryAsync(failedDelivery.WebhookDeliveryId);

        // Assert
        capturedRequest!.Headers.Should().Contain(h => h.Key == "X-Webhook-Retry");
    }

    #endregion

    #region Helper Methods

    private void SetupMockHttpResponse(HttpStatusCode statusCode, string content = "")
    {
        _mockHttpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content)
            });
    }

    #endregion
}

#region Supporting DTOs and Enums

public enum WebhookEventType
{
    Unknown,
    IncidentCreated,
    IncidentUpdated,
    IncidentClosed,
    IncidentResolved,
    SLABreached,
    ChangeCreated,
    ChangeApproved,
    ProblemCreated
}

public class CreateWebhookSubscriptionDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string TargetUrl { get; set; } = "";
    public string? Secret { get; set; }
    public List<string> EventTypes { get; set; } = new();
    public Dictionary<string, string>? Headers { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}

public class UpdateWebhookSubscriptionDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? TargetUrl { get; set; }
    public string? Secret { get; set; }
    public List<string>? EventTypes { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    public bool? IsActive { get; set; }
    public int? TimeoutSeconds { get; set; }
    public int? RetryCount { get; set; }
}

public class WebhookSubscriptionDto
{
    public int WebhookSubscriptionId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string TargetUrl { get; set; } = "";
    public string Secret { get; set; } = "";
    public List<string> EventTypes { get; set; } = new();
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool IsActive { get; set; }
    public int TimeoutSeconds { get; set; }
    public int RetryCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebhookDeliveryDto
{
    public int WebhookDeliveryId { get; set; }
    public int WebhookSubscriptionId { get; set; }
    public string SubscriptionName { get; set; } = "";
    public string EventType { get; set; } = "";
    public string TargetUrl { get; set; } = "";
    public string? RequestBody { get; set; }
    public int? ResponseStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? DurationMs { get; set; }
}

public class WebhookPayload<T>
{
    public string EventType { get; set; } = "";
    public string EventId { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = "";
    public T? Data { get; set; }
}

#endregion
