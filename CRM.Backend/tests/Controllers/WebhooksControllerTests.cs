// CRM Solution - Customer Relationship Management System
// Webhooks Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for WebhooksController
/// Covers: Webhook registration, execution, validation
/// </summary>
public class WebhooksControllerTests
{
    private readonly Mock<IWebhookService> _mockWebhookService;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<WebhooksController>> _mockLogger;
    private readonly WebhooksController _controller;

    public WebhooksControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookService>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<WebhooksController>>();

        _controller = new WebhooksController(
            _mockWebhookService.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);

        SetupUserContext();
    }

    private void SetupUserContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkWithWebhooks()
    {
        // Arrange
        var webhooks = new List<WebhookDto>
        {
            new WebhookDto { Id = 1, Name = "Order Created", Url = "https://example.com/webhook1", IsActive = true },
            new WebhookDto { Id = 2, Name = "Contact Updated", Url = "https://example.com/webhook2", IsActive = true }
        };

        _mockWebhookService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(webhooks);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedWebhooks = okResult.Value.Should().BeAssignableTo<IEnumerable<WebhookDto>>().Subject;
        returnedWebhooks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActive_ReturnsActiveWebhooks()
    {
        // Arrange
        var webhooks = new List<WebhookDto>
        {
            new WebhookDto { Id = 1, Name = "Active Hook", IsActive = true }
        };

        _mockWebhookService.Setup(s => s.GetActiveAsync())
            .ReturnsAsync(webhooks);

        // Act
        var result = await _controller.GetActive();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<WebhookDto>>();
    }

    [Fact]
    public async Task GetByEvent_ReturnsWebhooksForEvent()
    {
        // Arrange
        var webhooks = new List<WebhookDto>
        {
            new WebhookDto { Id = 1, Name = "Account Webhook", Events = new List<string> { "account.created" } }
        };

        _mockWebhookService.Setup(s => s.GetByEventAsync("account.created"))
            .ReturnsAsync(webhooks);

        // Act
        var result = await _controller.GetByEvent("account.created");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<WebhookDto>>();
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingWebhook_ReturnsOk()
    {
        // Arrange
        var webhook = new WebhookDto
        {
            Id = 1,
            Name = "Order Webhook",
            Url = "https://example.com/orders",
            Events = new List<string> { "order.created", "order.updated" },
            IsActive = true
        };

        _mockWebhookService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(webhook);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedWebhook = okResult.Value.Should().BeOfType<WebhookDto>().Subject;
        returnedWebhook.Name.Should().Be("Order Webhook");
    }

    [Fact]
    public async Task GetById_NonExistingWebhook_ReturnsNotFound()
    {
        // Arrange
        _mockWebhookService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((WebhookDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidWebhook_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateWebhookDto
        {
            Name = "New Webhook",
            Url = "https://example.com/webhook",
            Events = new List<string> { "contact.created" },
            Secret = "webhook-secret-key"
        };

        var createdWebhook = new WebhookDto
        {
            Id = 3,
            Name = "New Webhook",
            Url = "https://example.com/webhook",
            IsActive = true
        };

        _mockWebhookService.Setup(s => s.CreateAsync(createDto))
            .ReturnsAsync(createdWebhook);
        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(_controller.GetById));
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_InvalidUrl_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateWebhookDto
        {
            Name = "Invalid Webhook",
            Url = "not-a-valid-url",
            Events = new List<string> { "contact.created" }
        };

        _mockWebhookService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new ArgumentException("Invalid URL format"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateUrl_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateWebhookDto
        {
            Name = "Duplicate Webhook",
            Url = "https://example.com/existing",
            Events = new List<string> { "contact.created" }
        };

        _mockWebhookService.Setup(s => s.CreateAsync(createDto))
            .ThrowsAsync(new InvalidOperationException("Webhook with this URL already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidWebhook_ReturnsOk()
    {
        // Arrange
        var updateDto = new UpdateWebhookDto
        {
            Id = 1,
            Name = "Updated Webhook",
            Url = "https://example.com/updated"
        };

        var updatedWebhook = new WebhookDto
        {
            Id = 1,
            Name = "Updated Webhook",
            Url = "https://example.com/updated"
        };

        _mockWebhookService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync(updatedWebhook);
        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedWebhook = okResult.Value.Should().BeOfType<WebhookDto>().Subject;
        returnedWebhook.Name.Should().Be("Updated Webhook");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateWebhookDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingWebhook_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateWebhookDto { Id = 999 };

        _mockWebhookService.Setup(s => s.UpdateAsync(updateDto))
            .ReturnsAsync((WebhookDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingWebhook_ReturnsNoContent()
    {
        // Arrange
        _mockWebhookService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);
        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingWebhook_ReturnsNotFound()
    {
        // Arrange
        _mockWebhookService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    #endregion

    #region Activate/Deactivate Tests

    [Fact]
    public async Task Activate_ValidWebhook_ReturnsOk()
    {
        // Arrange
        _mockWebhookService.Setup(s => s.ActivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Activate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Deactivate_ValidWebhook_ReturnsOk()
    {
        // Arrange
        _mockWebhookService.Setup(s => s.DeactivateAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Deactivate(1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    #endregion

    #region Test Webhook Tests

    [Fact]
    public async Task TestWebhook_ValidWebhook_ReturnsOkWithResult()
    {
        // Arrange
        var testResult = new WebhookTestResultDto
        {
            Success = true,
            StatusCode = 200,
            ResponseTime = 150,
            ResponseBody = "OK"
        };

        _mockWebhookService.Setup(s => s.TestWebhookAsync(1))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestWebhook(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<WebhookTestResultDto>().Subject;
        returnedResult.Success.Should().BeTrue();
    }

    [Fact]
    public async Task TestWebhook_FailedDelivery_ReturnsOkWithFailure()
    {
        // Arrange
        var testResult = new WebhookTestResultDto
        {
            Success = false,
            StatusCode = 500,
            Error = "Internal Server Error"
        };

        _mockWebhookService.Setup(s => s.TestWebhookAsync(1))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestWebhook(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResult = okResult.Value.Should().BeOfType<WebhookTestResultDto>().Subject;
        returnedResult.Success.Should().BeFalse();
    }

    [Fact]
    public async Task TestWebhookWithPayload_ReturnsOkWithResult()
    {
        // Arrange
        var payload = new { test = "data", timestamp = DateTime.UtcNow };
        var testResult = new WebhookTestResultDto { Success = true, StatusCode = 200 };

        _mockWebhookService.Setup(s => s.TestWebhookWithPayloadAsync(1, It.IsAny<object>()))
            .ReturnsAsync(testResult);

        // Act
        var result = await _controller.TestWebhookWithPayload(1, payload);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Delivery Logs Tests

    [Fact]
    public async Task GetDeliveryLogs_ReturnsLogs()
    {
        // Arrange
        var logs = new List<WebhookDeliveryLogDto>
        {
            new WebhookDeliveryLogDto
            {
                Id = 1,
                WebhookId = 1,
                Event = "order.created",
                Success = true,
                StatusCode = 200,
                DeliveredAt = DateTime.UtcNow
            }
        };

        _mockWebhookService.Setup(s => s.GetDeliveryLogsAsync(1, 1, 20))
            .ReturnsAsync(new PagedResult<WebhookDeliveryLogDto> { Items = logs, TotalCount = 1 });

        // Act
        var result = await _controller.GetDeliveryLogs(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PagedResult<WebhookDeliveryLogDto>>();
    }

    [Fact]
    public async Task GetDeliveryLogById_ExistingLog_ReturnsOk()
    {
        // Arrange
        var log = new WebhookDeliveryLogDetailDto
        {
            Id = 1,
            WebhookId = 1,
            Event = "order.created",
            RequestBody = "{\"orderId\": 123}",
            ResponseBody = "OK",
            Success = true
        };

        _mockWebhookService.Setup(s => s.GetDeliveryLogByIdAsync(1))
            .ReturnsAsync(log);

        // Act
        var result = await _controller.GetDeliveryLogById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<WebhookDeliveryLogDetailDto>();
    }

    [Fact]
    public async Task RetryDelivery_ValidLog_ReturnsOkWithResult()
    {
        // Arrange
        var retryResult = new WebhookRetryResultDto
        {
            Success = true,
            StatusCode = 200
        };

        _mockWebhookService.Setup(s => s.RetryDeliveryAsync(1))
            .ReturnsAsync(retryResult);

        // Act
        var result = await _controller.RetryDelivery(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<WebhookRetryResultDto>();
    }

    #endregion

    #region Events Tests

    [Fact]
    public async Task GetAvailableEvents_ReturnsEvents()
    {
        // Arrange
        var events = new List<WebhookEventDto>
        {
            new WebhookEventDto { Name = "account.created", Description = "When an account is created" },
            new WebhookEventDto { Name = "contact.created", Description = "When a contact is created" },
            new WebhookEventDto { Name = "order.created", Description = "When an order is created" }
        };

        _mockWebhookService.Setup(s => s.GetAvailableEventsAsync())
            .ReturnsAsync(events);

        // Act
        var result = await _controller.GetAvailableEvents();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedEvents = okResult.Value.Should().BeAssignableTo<IEnumerable<WebhookEventDto>>().Subject;
        returnedEvents.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetEventCategories_ReturnsCategories()
    {
        // Arrange
        var categories = new List<WebhookEventCategoryDto>
        {
            new WebhookEventCategoryDto { Name = "Account", Events = new List<string> { "account.created", "account.updated" } },
            new WebhookEventCategoryDto { Name = "Contact", Events = new List<string> { "contact.created", "contact.updated" } }
        };

        _mockWebhookService.Setup(s => s.GetEventCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetEventCategories();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeAssignableTo<IEnumerable<WebhookEventCategoryDto>>();
    }

    #endregion

    #region Secret Management Tests

    [Fact]
    public async Task RegenerateSecret_ReturnsNewSecret()
    {
        // Arrange
        var newSecret = "new-webhook-secret-12345";

        _mockWebhookService.Setup(s => s.RegenerateSecretAsync(1))
            .ReturnsAsync(newSecret);

        // Act
        var result = await _controller.RegenerateSecret(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { Secret = newSecret });
    }

    [Fact]
    public async Task GetSecret_ReturnsSecret()
    {
        // Arrange
        var secret = "webhook-secret-key";

        _mockWebhookService.Setup(s => s.GetSecretAsync(1))
            .ReturnsAsync(secret);

        // Act
        var result = await _controller.GetSecret(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { Secret = secret });
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetWebhookStatistics_ReturnsStats()
    {
        // Arrange
        var stats = new WebhookStatisticsDto
        {
            TotalWebhooks = 10,
            ActiveWebhooks = 8,
            TotalDeliveries = 5000,
            SuccessfulDeliveries = 4800,
            FailedDeliveries = 200,
            AverageResponseTime = 150
        };

        _mockWebhookService.Setup(s => s.GetStatisticsAsync(null))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatistics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStats = okResult.Value.Should().BeOfType<WebhookStatisticsDto>().Subject;
        returnedStats.SuccessfulDeliveries.Should().Be(4800);
    }

    [Fact]
    public async Task GetWebhookStatisticsById_ReturnsWebhookStats()
    {
        // Arrange
        var stats = new WebhookStatisticsDto
        {
            TotalDeliveries = 500,
            SuccessfulDeliveries = 480,
            FailedDeliveries = 20
        };

        _mockWebhookService.Setup(s => s.GetStatisticsAsync(1))
            .ReturnsAsync(stats);

        // Act
        var result = await _controller.GetStatisticsById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<WebhookStatisticsDto>();
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkActivate_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockWebhookService.Setup(s => s.BulkActivateAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkActivate(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { ActivatedCount = 3 });
    }

    [Fact]
    public async Task BulkDeactivate_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2 };

        _mockWebhookService.Setup(s => s.BulkDeactivateAsync(ids))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkDeactivate(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeactivatedCount = 2 });
    }

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOkWithCount()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockWebhookService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { DeletedCount = 3 });
    }

    #endregion
}
