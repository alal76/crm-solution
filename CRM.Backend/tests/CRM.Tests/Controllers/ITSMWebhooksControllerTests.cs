// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ITSMWebhooksController (TCOV-052).
/// </summary>
public class ITSMWebhooksControllerTests
{
    private readonly Mock<IWebhookNotificationService> _mockWebhookService;
    private readonly Mock<ILogger<ITSMWebhooksController>> _mockLogger;
    private readonly ITSMWebhooksController _controller;

    public ITSMWebhooksControllerTests()
    {
        _mockWebhookService = new Mock<IWebhookNotificationService>();
        _mockLogger = new Mock<ILogger<ITSMWebhooksController>>();
        _controller = new ITSMWebhooksController(_mockWebhookService.Object, _mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "test@crm.local")
                }, "test"))
            }
        };
    }

    [Fact]
    public async Task GetWebhooks_ShouldReturnOk_WithEmptyList()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionsAsync())
            .ReturnsAsync(Enumerable.Empty<WebhookSubscriptionDto>());

        var result = await _controller.GetWebhooks();

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetSubscriptions_ShouldReturnOk()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionsAsync())
            .ReturnsAsync(new List<WebhookSubscriptionDto>
            {
                new WebhookSubscriptionDto { WebhookSubscriptionId = 1, TargetUrl = "https://example.com/hook" }
            });

        var result = await _controller.GetSubscriptions();

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ((IEnumerable<WebhookSubscriptionDto>)ok.Value!).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSubscription_ShouldReturnNotFound_WhenNotExists()
    {
        _mockWebhookService.Setup(s => s.GetSubscriptionByIdAsync(999))
            .ReturnsAsync((WebhookSubscriptionDto?)null);

        var result = await _controller.GetSubscription(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSubscription_ShouldReturnOk_WhenExists()
    {
        var dto = new WebhookSubscriptionDto { WebhookSubscriptionId = 5, TargetUrl = "https://sub.example.com" };
        _mockWebhookService.Setup(s => s.GetSubscriptionByIdAsync(5)).ReturnsAsync(dto);

        var result = await _controller.GetSubscription(5);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegisterWebhook_ShouldReturnOk_WhenServiceSucceeds()
    {
        var request = new CreateWebhookSubscriptionDto
        {
            TargetUrl = "https://example.com/webhook",
            EventTypes = new List<string> { "incident.created" }
        };
        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(request, 1))
            .ReturnsAsync(new WebhookSubscriptionDto { WebhookSubscriptionId = 10, TargetUrl = request.TargetUrl });

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task RegisterWebhook_ShouldReturnOk_WhenServiceThrows()
    {
        var request = new CreateWebhookSubscriptionDto { TargetUrl = "https://bad.example.com" };
        _mockWebhookService.Setup(s => s.CreateSubscriptionAsync(It.IsAny<CreateWebhookSubscriptionDto>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<OkObjectResult>();
    }
}
