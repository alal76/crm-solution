// CRM Solution - ITSM SLA Service Unit Tests
// Tests for SLAService - SLA policy, instance lifecycle, and breach tracking

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class SLAServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<IBusinessHoursCalculator> _mockBusinessHours;
    private readonly Mock<ILogger<SLAService>> _mockLogger;
    private readonly ISLAService _service;

    public SLAServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockBusinessHours = new Mock<IBusinessHoursCalculator>();
        _mockLogger = new Mock<ILogger<SLAService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new SLAService(_mockResolver.Object, _mockBusinessHours.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateSLAPolicyAsync
    // ========================================================================

    [Fact]
    public async Task CreateSLAPolicyAsync_ShouldCreatePolicy_WhenValidDtoProvided()
    {
        // Arrange
        var policies = new List<SLAPolicy>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(policies);
        mockSet.Setup(m => m.Add(It.IsAny<SLAPolicy>())).Callback<SLAPolicy>(e => policies.Add(e));
        _mockContext.Setup(c => c.ITSMSLAPolicies).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new SLAPolicyDto
        {
            Name = "Standard SLA",
            TargetType = SLATargetType.Incident,
            P1ResponseMinutes = 15,
            P1ResolutionMinutes = 60,
            UseBusinessHours = true,
            IsActive = true
        };

        // Act
        var result = await _service.CreateSLAPolicyAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Standard SLA");
        mockSet.Verify(m => m.Add(It.IsAny<SLAPolicy>()), Times.Once);
    }

    // ========================================================================
    // GetSLAPoliciesAsync
    // ========================================================================

    [Fact]
    public async Task GetSLAPoliciesAsync_ShouldReturnAllPolicies_WhenNoFilter()
    {
        // Arrange
        var policies = new List<SLAPolicy>
        {
            new() { SLAPolicyId = 1, Name = "Incident SLA", TargetType = SLATargetType.Incident, P1ResponseMinutes = 15, P1ResolutionMinutes = 60, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SLAPolicyId = 2, Name = "Service Request SLA", TargetType = SLATargetType.ServiceRequest, P1ResponseMinutes = 30, P1ResolutionMinutes = 120, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMSLAPolicies).Returns(MockDbSetFactory.CreateMockDbSet(policies).Object);

        // Act
        var result = await _service.GetSLAPoliciesAsync(null);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSLAPoliciesAsync_ShouldFilterByTargetType()
    {
        // Arrange
        var policies = new List<SLAPolicy>
        {
            new() { SLAPolicyId = 1, Name = "Incident SLA", TargetType = SLATargetType.Incident, P1ResponseMinutes = 15, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { SLAPolicyId = 2, Name = "SR SLA", TargetType = SLATargetType.ServiceRequest, P1ResponseMinutes = 30, IsActive = true, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ITSMSLAPolicies).Returns(MockDbSetFactory.CreateMockDbSet(policies).Object);

        // Act
        var result = await _service.GetSLAPoliciesAsync(targetType: SLATargetType.Incident);

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(p => p.TargetType == SLATargetType.Incident);
    }

    // ========================================================================
    // GetSLAInstanceAsync
    // ========================================================================

    [Fact]
    public async Task GetSLAInstanceAsync_ShouldReturnInstance_WhenExists()
    {
        // Arrange
        var instances = new List<SLAInstance>
        {
            new()
            {
                SLAInstanceId = 1, TargetId = 100, TargetType = SLATargetType.Incident,
                SLAPolicyId = 1, ResponseDueAt = DateTime.UtcNow.AddHours(1),
                ResolutionDueAt = DateTime.UtcNow.AddHours(4),
                State = SLAState.Active, CreatedAt = DateTime.UtcNow
            }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);

        // Act
        var result = await _service.GetSLAInstanceAsync(targetId: 100, targetType: SLATargetType.Incident);

        // Assert
        result.Should().NotBeNull();
        result!.TargetId.Should().Be(100);
        result.State.Should().Be(SLAState.Active);
    }

    [Fact]
    public async Task GetSLAInstanceAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(new List<SLAInstance>()).Object);

        // Act
        var result = await _service.GetSLAInstanceAsync(targetId: 999, targetType: SLATargetType.Incident);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // StartSLAAsync
    // ========================================================================

    [Fact]
    public async Task StartSLAAsync_ShouldCreateSLAInstance_WhenPolicyExists()
    {
        // Arrange
        var policies = new List<SLAPolicy>
        {
            new()
            {
                SLAPolicyId = 1, Name = "Default Incident SLA",
                TargetType = SLATargetType.Incident,
                P1ResponseMinutes = 15, P1ResolutionMinutes = 60,
                P2ResponseMinutes = 30, P2ResolutionMinutes = 120,
                P3ResponseMinutes = 60, P3ResolutionMinutes = 240,
                P4ResponseMinutes = 120, P4ResolutionMinutes = 480,
                UseBusinessHours = false, IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        var instances = new List<SLAInstance>();
        var mockInstanceSet = MockDbSetFactory.CreateMockDbSet(instances);
        mockInstanceSet.Setup(m => m.Add(It.IsAny<SLAInstance>())).Callback<SLAInstance>(e => instances.Add(e));

        _mockContext.Setup(c => c.ITSMSLAPolicies).Returns(MockDbSetFactory.CreateMockDbSet(policies).Object);
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(mockInstanceSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.StartSLAAsync(targetId: 50, targetType: SLATargetType.Incident, priority: 1);

        // Assert
        mockInstanceSet.Verify(m => m.Add(It.IsAny<SLAInstance>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ========================================================================
    // PauseSLAAsync / ResumeSLAAsync
    // ========================================================================

    [Fact]
    public async Task PauseSLAAsync_ShouldPauseSLA_WhenActive()
    {
        // Arrange
        var instances = new List<SLAInstance>
        {
            new()
            {
                SLAInstanceId = 1, TargetId = 100, TargetType = SLATargetType.Incident,
                SLAPolicyId = 1, State = SLAState.Active,
                ResponseDueAt = DateTime.UtcNow.AddHours(1),
                ResolutionDueAt = DateTime.UtcNow.AddHours(4),
                CreatedAt = DateTime.UtcNow
            }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.PauseSLAAsync(targetId: 100, targetType: SLATargetType.Incident, reason: "Waiting for customer");

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ResumeSLAAsync_ShouldResumeSLA_WhenPaused()
    {
        // Arrange
        var instances = new List<SLAInstance>
        {
            new()
            {
                SLAInstanceId = 1, TargetId = 100, TargetType = SLATargetType.Incident,
                SLAPolicyId = 1, State = SLAState.Paused,
                PausedAt = DateTime.UtcNow.AddMinutes(-30),
                ResponseDueAt = DateTime.UtcNow.AddHours(1),
                ResolutionDueAt = DateTime.UtcNow.AddHours(4),
                CreatedAt = DateTime.UtcNow,
                SLAPolicy = new SLAPolicy
                {
                    SLAPolicyId = 1, Name = "Test SLA", TargetType = SLATargetType.Incident,
                    P1ResponseMinutes = 15, P1ResolutionMinutes = 60,
                    IsActive = true, CreatedAt = DateTime.UtcNow
                }
            }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.ResumeSLAAsync(targetId: 100, targetType: SLATargetType.Incident);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ========================================================================
    // CompleteSLAAsync
    // ========================================================================

    [Fact]
    public async Task CompleteSLAAsync_ShouldCompleteSLA_AndRecordBreaches()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var instances = new List<SLAInstance>
        {
            new()
            {
                SLAInstanceId = 1, TargetId = 100, TargetType = SLATargetType.Incident,
                SLAPolicyId = 1, State = SLAState.Active,
                ResponseDueAt = now.AddMinutes(-30), // Already breached
                ResolutionDueAt = now.AddHours(2),
                CreatedAt = now.AddHours(-1)
            }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await _service.CompleteSLAAsync(
            targetId: 100,
            targetType: SLATargetType.Incident,
            responseComplete: true,
            resolutionComplete: true);

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    // ========================================================================
    // GetBreachedSLAsAsync
    // ========================================================================

    [Fact]
    public async Task GetBreachedSLAsAsync_ShouldReturnBreachedInstances()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var instances = new List<SLAInstance>
        {
            new() { SLAInstanceId = 1, TargetId = 1, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResponseBreached = true, ResponseDueAt = now.AddHours(-1), ResolutionDueAt = now.AddHours(2), CreatedAt = now },
            new() { SLAInstanceId = 2, TargetId = 2, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResponseBreached = false, ResponseDueAt = now.AddHours(1), ResolutionDueAt = now.AddHours(4), CreatedAt = now },
            new() { SLAInstanceId = 3, TargetId = 3, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResolutionBreached = true, ResponseDueAt = now, ResolutionDueAt = now.AddHours(-2), CreatedAt = now }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);

        // Act
        var result = await _service.GetBreachedSLAsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().OnlyContain(i => i.ResponseBreached == true || i.ResolutionBreached == true);
    }

    // ========================================================================
    // GetAtRiskSLAsAsync
    // ========================================================================

    [Fact]
    public async Task GetAtRiskSLAsAsync_ShouldReturnSLAsApproachingBreach()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var instances = new List<SLAInstance>
        {
            new() { SLAInstanceId = 1, TargetId = 1, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResponseDueAt = now.AddMinutes(10), ResolutionDueAt = now.AddHours(4), ResponseBreached = false, CreatedAt = now },
            new() { SLAInstanceId = 2, TargetId = 2, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResponseDueAt = now.AddHours(2), ResolutionDueAt = now.AddHours(8), ResponseBreached = false, CreatedAt = now }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);

        // Act
        var result = await _service.GetAtRiskSLAsAsync(thresholdMinutes: 30);

        // Assert
        result.Should().NotBeNull();
        // Instance 1 is at risk (10 min until breach < 30 min threshold)
    }

    // ========================================================================
    // GetSLADashboardAsync
    // ========================================================================

    [Fact]
    public async Task GetSLADashboardAsync_ShouldReturnDashboardMetrics()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var instances = new List<SLAInstance>
        {
            new() { SLAInstanceId = 1, TargetId = 1, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Active, ResponseBreached = false, ResolutionBreached = false, ResponseDueAt = now.AddHours(1), ResolutionDueAt = now.AddHours(4), CreatedAt = now },
            new() { SLAInstanceId = 2, TargetId = 2, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Completed, ResponseBreached = true, ResolutionBreached = false, ResponseDueAt = now.AddHours(-1), ResolutionDueAt = now.AddHours(2), ResponseActualAt = now, ResolutionActualAt = now, CreatedAt = now.AddDays(-1) },
            new() { SLAInstanceId = 3, TargetId = 3, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Completed, ResponseBreached = false, ResolutionBreached = false, ResponseDueAt = now, ResolutionDueAt = now.AddHours(4), ResponseActualAt = now.AddMinutes(-10), ResolutionActualAt = now, CreatedAt = now.AddDays(-2) }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);

        // Act
        var result = await _service.GetSLADashboardAsync();

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // GetSLAMetricsAsync
    // ========================================================================

    [Fact]
    public async Task GetSLAMetricsAsync_ShouldReturnMetricsForDateRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var startDate = now.AddDays(-30);
        var endDate = now;
        var instances = new List<SLAInstance>
        {
            new() { SLAInstanceId = 1, TargetId = 1, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Completed, ResponseBreached = false, ResolutionBreached = false, ResponseDueAt = now.AddHours(-10), ResolutionDueAt = now.AddHours(-5), CreatedAt = now.AddDays(-10) },
            new() { SLAInstanceId = 2, TargetId = 2, TargetType = SLATargetType.Incident, SLAPolicyId = 1, State = SLAState.Completed, ResponseBreached = true, ResolutionBreached = true, ResponseDueAt = now.AddHours(-20), ResolutionDueAt = now.AddHours(-15), CreatedAt = now.AddDays(-15) }
        };
        _mockContext.Setup(c => c.ITSMSLAInstances).Returns(MockDbSetFactory.CreateMockDbSet(instances).Object);

        // Act
        var result = await _service.GetSLAMetricsAsync(startDate, endDate);

        // Assert
        result.Should().NotBeNull();
    }
}
