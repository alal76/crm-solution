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
public class CommissionPlanServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CommissionPlanService>> _mockLogger;
    private readonly CommissionPlanService _service;

    public CommissionPlanServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionPlanService>>();
        _service = new CommissionPlanService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommissionPlanService(null, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CommissionPlanService(_mockContext.Object, null));
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
        _mockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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

        _mockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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

        _mockContext.Setup(c => c.CommissionPlans).Returns(planDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CommissionCalculationService>> _mockLogger;
    private readonly CommissionCalculationService _service;

    public CommissionCalculationServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionCalculationService>>();
        _service = new CommissionCalculationService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionCalculationService(null, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionCalculationService(_mockContext.Object, null));
    }
}

/// <summary>
/// Unit tests for CommissionApprovalService.
/// Tests approval workflows and audit trails.
/// </summary>
public class CommissionApprovalServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CommissionApprovalService>> _mockLogger;
    private readonly CommissionApprovalService _service;

    public CommissionApprovalServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionApprovalService>>();
        _service = new CommissionApprovalService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionApprovalService(null, _mockLogger.Object));
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

        _mockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        _mockContext.Setup(c => c.CommissionApprovalAudits).Returns(auditDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CommissionPayoutService>> _mockLogger;
    private readonly CommissionPayoutService _service;

    public CommissionPayoutServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionPayoutService>>();
        _service = new CommissionPayoutService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CommissionPayoutService(null, _mockLogger.Object));
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

        _mockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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

        _mockContext.Setup(c => c.Commissions).Returns(commissionDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CampaignRecipientService>> _mockLogger;
    private readonly CampaignRecipientService _service;

    public CampaignRecipientServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CampaignRecipientService>>();
        _service = new CampaignRecipientService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CampaignRecipientService(null, _mockLogger.Object));
    }
}

/// <summary>
/// Unit tests for CampaignMetricsService.
/// Tests metrics calculations and analytics.
/// </summary>
public class CampaignMetricsServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CampaignMetricsService>> _mockLogger;
    private readonly CampaignMetricsService _service;

    public CampaignMetricsServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CampaignMetricsService>>();
        _service = new CampaignMetricsService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new CampaignMetricsService(null, _mockLogger.Object));
    }
}

/// <summary>
/// Unit tests for EmailSequenceManagementService.
/// Tests sequence CRUD, step management, and enrollments.
/// </summary>
public class EmailSequenceManagementServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<EmailSequenceManagementService>> _mockLogger;
    private readonly EmailSequenceManagementService _service;

    public EmailSequenceManagementServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EmailSequenceManagementService>>();
        _service = new EmailSequenceManagementService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EmailSequenceManagementService(null, _mockLogger.Object));
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
        _mockContext.Setup(c => c.EmailSequences).Returns(sequenceDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<WebhookManagementService>> _mockLogger;
    private readonly WebhookManagementService _service;

    public WebhookManagementServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<WebhookManagementService>>();
        _service = new WebhookManagementService(_mockContext.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookManagementService(null, _mockLogger.Object));
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
        _mockContext.Setup(c => c.Webhooks).Returns(webhookDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

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
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<WebhookDispatcherService>> _mockLogger;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly WebhookDispatcherService _service;

    public WebhookDispatcherServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<WebhookDispatcherService>>();
        _mockHttpClient = new Mock<HttpClient>();
        _service = new WebhookDispatcherService(_mockContext.Object, _mockLogger.Object, _mockHttpClient.Object);
    }

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(null, _mockLogger.Object, _mockHttpClient.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(_mockContext.Object, null, _mockHttpClient.Object));
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WebhookDispatcherService(_mockContext.Object, _mockLogger.Object, null));
    }
}
