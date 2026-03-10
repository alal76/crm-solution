// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for SubscriptionsController (TCOV-041).
/// </summary>
public class SubscriptionsControllerTests
{
    private readonly Mock<ISubscriptionService> _mockService;
    private readonly Mock<ILogger<SubscriptionsController>> _mockLogger;
    private readonly SubscriptionsController _controller;

    public SubscriptionsControllerTests()
    {
        _mockService = new Mock<ISubscriptionService>();
        _mockLogger = new Mock<ILogger<SubscriptionsController>>();
        _controller = new SubscriptionsController(_mockService.Object, _mockLogger.Object);
    }

    private static Subscription MakeSub(int id = 1) => new Subscription
    {
        Id = id, BillingCycle = "Monthly",
        SubscriptionStatus = SubscriptionStatus.Active,
        Amount = 99m, AccountId = 1,
        StartDate = DateTime.UtcNow
    };

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithEmptyList()
    {
        _mockService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Subscription>());

        var result = await _controller.GetAll(cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithSubscriptions()
    {
        _mockService.Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakeSub(1), MakeSub(2) });

        var result = await _controller.GetAll(cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result.Result!;
        okResult.Value.Should().BeAssignableTo<IEnumerable<Subscription>>();
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenSubscriptionExists()
    {
        _mockService.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSub(1));

        var result = await _controller.GetById(1, default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenNotExists()
    {
        _mockService.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Subscription?)null);

        var result = await _controller.GetById(999, default);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithStatusFilter()
    {
        _mockService.Setup(s => s.GetAllAsync(null, SubscriptionStatus.Active, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { MakeSub(1) });

        var result = await _controller.GetAll(status: SubscriptionStatus.Active, cancellationToken: default);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenInvalidBillingCycle()
    {
        var request = new SubscriptionsController.SubscriptionCreateRequest
        {
            AccountId = 1, BillingCycle = "Invalid", Amount = 99m,
            StartDate = DateTime.UtcNow
        };

        var result = await _controller.Create(request, default);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenValid()
    {
        var request = new SubscriptionsController.SubscriptionCreateRequest
        {
            AccountId = 1, BillingCycle = "Monthly", Amount = 99m,
            StartDate = DateTime.UtcNow
        };
        _mockService.Setup(s => s.CreateAsync(It.IsAny<Subscription>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeSub(42));

        var result = await _controller.Create(request, default);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }
}
