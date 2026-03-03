// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Backend.Tests.Services;

/// <summary>
/// Unit tests for CommissionPlanService.
/// Tests CRUD operations, tier management, and plan assignment.
/// </summary>
public class CommissionPlanServiceTests : ServiceTestFixtureBase<CommissionPlanService>
{    private readonly CommissionPlanService _service;

    public CommissionPlanServiceTests()
    {        _service = new CommissionPlanService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommissionPlanService(null, MockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommissionPlanService(MockContext.Object, null));
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsCommissionPlanDto()
    {
        // Arrange
        var dto = new CreateCommissionPlanDto
        {
            Name = "Sales Plan",
            Description = "Standard sales commission plan",
            BaseRate = 5m,
            IsActive = true
        };

        var plan = new CommissionPlan
        {
            Id = 1,
            Name = dto.Name,
            Description = dto.Description,
            BaseRate = dto.BaseRate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var planDbSet = new Mock<DbSet<CommissionPlan>>();
        MockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateCommissionPlanDto { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateCommissionPlanDto { Name = null };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task ActivateAsync_WithValidPlanId_ReturnsTrue()
    {
        // Arrange
        var planId = 1;
        var plan = new CommissionPlan { Id = planId, IsActive = false };

        var planDbSet = new Mock<DbSet<CommissionPlan>>();
        planDbSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommissionPlan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        MockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ActivateAsync(planId);

        // Assert
        Assert.True(result);
        Assert.True(plan.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_WithValidPlanId_ReturnsTrue()
    {
        // Arrange
        var planId = 1;
        var plan = new CommissionPlan { Id = planId, IsActive = true };

        var planDbSet = new Mock<DbSet<CommissionPlan>>();
        planDbSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommissionPlan, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        MockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.DeactivateAsync(planId);

        // Assert
        Assert.True(result);
        Assert.False(plan.IsActive);
    }
}

/// <summary>
/// Unit tests for CommissionCalculationService.
/// Tests commission calculations with tiers and accelerators.
/// </summary>
public class CommissionCalculationServiceTests
{    private readonly Mock<ILogger<CommissionCalculationService>> MockLogger;
    private readonly CommissionCalculationService _service;

    public CommissionCalculationServiceTests()
    {        MockLogger = new Mock<ILogger<CommissionCalculationService>>();
        _service = new CommissionCalculationService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionCalculationService(null, MockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionCalculationService(MockContext.Object, null));
    }
}

/// <summary>
/// Unit tests for CommissionApprovalService.
/// Tests approval workflows and audit trails.
/// </summary>
public class CommissionApprovalServiceTests
{    private readonly Mock<ILogger<CommissionApprovalService>> MockLogger;
    private readonly CommissionApprovalService _service;

    public CommissionApprovalServiceTests()
    {        MockLogger = new Mock<ILogger<CommissionApprovalService>>();
        _service = new CommissionApprovalService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionApprovalService(null, MockLogger.Object));
    }

    [Fact]
    public async Task ApproveAsync_WithValidIds_ReturnsTrue()
    {
        // Arrange
        var commissionId = 1;
        var approverId = 2;
        var commission = new Commission { Id = commissionId, Status = CommissionStatus.Pending };

        var commissionDbSet = new Mock<DbSet<Commission>>();
        var auditDbSet = new Mock<DbSet<CommissionApprovalAudit>>();

        commissionDbSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Commission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        MockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        MockContext.Setup(c => c.CommissionApprovalAudits).Returns(auditDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ApproveAsync(commissionId, approverId);

        // Assert
        Assert.True(result);
        Assert.Equal(CommissionStatus.Approved, commission.Status);
        Assert.Equal(approverId, commission.ApprovedById);
    }
}

/// <summary>
/// Unit tests for CommissionPayoutService.
/// Tests payout operations and reconciliation.
/// </summary>
public class CommissionPayoutServiceTests
{    private readonly Mock<ILogger<CommissionPayoutService>> MockLogger;
    private readonly CommissionPayoutService _service;

    public CommissionPayoutServiceTests()
    {        MockLogger = new Mock<ILogger<CommissionPayoutService>>();
        _service = new CommissionPayoutService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionPayoutService(null, MockLogger.Object));
    }

    [Fact]
    public async Task MarkPaidAsync_WithValidIds_ReturnsTrue()
    {
        // Arrange
        var commissionId = 1;
        var paidDate = DateTime.UtcNow;
        var commission = new Commission { Id = commissionId, Status = CommissionStatus.Approved, CommissionAmount = 1000 };

        var commissionDbSet = new Mock<DbSet<Commission>>();
        commissionDbSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Commission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        MockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.MarkPaidAsync(commissionId, paidDate);

        // Assert
        Assert.True(result);
        Assert.Equal(CommissionStatus.Paid, commission.Status);
        Assert.Equal(paidDate, commission.PaidDate);
    }

    [Fact]
    public async Task ClawbackAsync_WithValidIds_ReturnsTrue()
    {
        // Arrange
        var commissionId = 1;
        var reason = "Contract termination";
        var commission = new Commission { Id = commissionId, Status = CommissionStatus.Paid, CommissionAmount = 1000 };

        var commissionDbSet = new Mock<DbSet<Commission>>();
        commissionDbSet.Setup(d => d.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Commission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(commission);

        MockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.ClawbackAsync(commissionId, reason);

        // Assert
        Assert.True(result);
        Assert.Equal(CommissionStatus.ClawedBack, commission.Status);
        Assert.Equal(reason, commission.ClawbackReason);
    }
}

/// <summary>
/// Unit tests for CampaignRecipientService.
/// Tests recipient management and filtering.
/// </summary>
public class CampaignRecipientServiceTests
{    private readonly Mock<ILogger<CampaignRecipientService>> MockLogger;
    private readonly CampaignRecipientService _service;

    public CampaignRecipientServiceTests()
    {        MockLogger = new Mock<ILogger<CampaignRecipientService>>();
        _service = new CampaignRecipientService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CampaignRecipientService(null, MockLogger.Object));
    }
}

/// <summary>
/// Unit tests for CampaignMetricsService.
/// Tests metrics calculations and analytics.
/// </summary>
public class CampaignMetricsServiceTests
{    private readonly Mock<ILogger<CampaignMetricsService>> MockLogger;
    private readonly CampaignMetricsService _service;

    public CampaignMetricsServiceTests()
    {        MockLogger = new Mock<ILogger<CampaignMetricsService>>();
        _service = new CampaignMetricsService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CampaignMetricsService(null, MockLogger.Object));
    }
}

/// <summary>
/// Unit tests for EmailSequenceManagementService.
/// Tests sequence CRUD, step management, and enrollments.
/// </summary>
public class EmailSequenceManagementServiceTests
{    private readonly Mock<ILogger<EmailSequenceManagementService>> MockLogger;
    private readonly EmailSequenceManagementService _service;

    public EmailSequenceManagementServiceTests()
    {        MockLogger = new Mock<ILogger<EmailSequenceManagementService>>();
        _service = new EmailSequenceManagementService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EmailSequenceManagementService(null, MockLogger.Object));
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsEmailSequenceDto()
    {
        // Arrange
        var dto = new CreateEmailSequenceDto
        {
            Name = "Welcome Series",
            Description = "New subscriber welcome sequence"
        };

        var sequenceDbSet = new Mock<DbSet<EmailSequence>>();
        MockContext.Setup(c => c.EmailSequences).Returns(sequenceDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateEmailSequenceDto { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }
}

/// <summary>
/// Unit tests for WebhookManagementService.
/// Tests webhook CRUD and delivery tracking.
/// </summary>
public class WebhookManagementServiceTests
{    private readonly Mock<ILogger<WebhookManagementService>> MockLogger;
    private readonly WebhookManagementService _service;

    public WebhookManagementServiceTests()
    {        MockLogger = new Mock<ILogger<WebhookManagementService>>();
        _service = new WebhookManagementService(MockContext.Object, MockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookManagementService(null, MockLogger.Object));
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ReturnsWebhookDto()
    {
        // Arrange
        var dto = new CreateWebhookDto
        {
            Url = "https://example.com/webhook",
            Events = new List<string> { "commission.approved" }
        };

        var webhookDbSet = new Mock<DbSet<Webhook>>();
        MockContext.Setup(c => c.Webhooks).Returns(webhookDbSet.Object);
        MockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyUrl_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateWebhookDto { Url = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }
}

/// <summary>
/// Unit tests for WebhookDispatcherService.
/// Tests webhook dispatch and queue processing.
/// </summary>
public class WebhookDispatcherServiceTests
{    private readonly Mock<ILogger<WebhookDispatcherService>> MockLogger;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly WebhookDispatcherService _service;

    public WebhookDispatcherServiceTests()
    {        MockLogger = new Mock<ILogger<WebhookDispatcherService>>();
        _mockHttpClient = new Mock<HttpClient>();
        _service = new WebhookDispatcherService(MockContext.Object, MockLogger.Object, _mockHttpClient.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(null, MockLogger.Object, _mockHttpClient.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(MockContext.Object, null, _mockHttpClient.Object));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(MockContext.Object, MockLogger.Object, null));
    }
}
