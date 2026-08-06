// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for WebhookManagementService (TCOV-038).</summary>
public class WebhookManagementServiceTests
{
    private readonly Mock<ICrmDbContext> _mockCtx;
    private readonly WebhookManagementService _service;

    private readonly List<WebhookSubscription> _webhooks;

    public WebhookManagementServiceTests()
    {
        _mockCtx = new Mock<ICrmDbContext>();
        var logger = new Mock<ILogger<WebhookManagementService>>().Object;

        _webhooks = new List<WebhookSubscription>();
        _mockCtx.Setup(c => c.WebhookSubscriptions).Returns(MockDbSetFactory.CreateMockDbSet(_webhooks).Object);
        _mockCtx.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new WebhookManagementService(_mockCtx.Object, logger);
    }

    private Mock<DbSet<WebhookSubscription>> RefreshWebhookSet()
    {
        var mockSet = MockDbSetFactory.CreateMockDbSet(_webhooks);
        _mockCtx.Setup(c => c.WebhookSubscriptions).Returns(mockSet.Object);
        return mockSet;
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoWebhooksExist()
    {
        var result = await _service.GetAllAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeletedWebhooks()
    {
        _webhooks.AddRange(new[]
        {
            new WebhookSubscription { WebhookSubscriptionId = 1, TargetUrl = "https://a.com", IsDeleted = false },
            new WebhookSubscription { WebhookSubscriptionId = 2, TargetUrl = "https://b.com", IsDeleted = true }
        });
        RefreshWebhookSet();

        var result = await _service.GetAllAsync();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByIsActive()
    {
        _webhooks.AddRange(new[]
        {
            new WebhookSubscription { WebhookSubscriptionId = 1, TargetUrl = "https://active.com", IsActive = true, IsDeleted = false },
            new WebhookSubscription { WebhookSubscriptionId = 2, TargetUrl = "https://inactive.com", IsActive = false, IsDeleted = false }
        });
        RefreshWebhookSet();

        var result = await _service.GetAllAsync(isActive: true);
        result.Should().HaveCount(1);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetByIdAsync(999);
        result.Should().BeNull();
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenUrlIsEmpty()
    {
        var dto = new CreateWebhookDto { Url = "" };
        Func<Task> act = () => _service.CreateAsync(dto);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*URL*");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────
    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenWebhookNotFound()
    {
        var result = await _service.DeleteAsync(999);
        result.Should().BeFalse();
    }
}
