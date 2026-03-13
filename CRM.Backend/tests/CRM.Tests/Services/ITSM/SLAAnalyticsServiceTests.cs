// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class SLAAnalyticsServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<SLAAnalyticsService>> _mockLogger;
    private readonly SLAAnalyticsService _service;

    public SLAAnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SLAAnalyticsTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<SLAAnalyticsService>>();
        _service = new SLAAnalyticsService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    private static ServiceRequest CreateRequest(bool responseSlaBreached, bool resolutionSlaBreached,
        DateTime? firstResponseDate = null, DateTime? resolvedDate = null,
        ServiceRequestPriority priority = ServiceRequestPriority.Medium)
    {
        return new ServiceRequest
        {
            Subject = "Test SR",
            Priority = priority,
            ResponseSlaBreached = responseSlaBreached,
            ResolutionSlaBreached = resolutionSlaBreached,
            FirstResponseDate = firstResponseDate,
            ResolvedDate = resolvedDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturn100Compliance_WhenNoTickets()
    {
        var start = DateTime.UtcNow.AddDays(-7);
        var end = DateTime.UtcNow;

        var result = await _service.GetDashboardAsync(start, end);

        result.Should().NotBeNull();
        result.TotalTickets.Should().Be(0);
        result.ComplianceRate.Should().Be(100.0);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldCalculateCorrectTotals_WhenTicketsExist()
    {
        var now = DateTime.UtcNow;
        _context.ServiceRequests.AddRange(
            CreateRequest(false, false),
            CreateRequest(true, false),
            CreateRequest(false, true)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(now.AddMinutes(-1), now.AddMinutes(1));

        result.TotalTickets.Should().Be(3);
        result.BreachedSLA.Should().Be(2);
        result.WithinSLA.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldCalculateComplianceRate()
    {
        var now = DateTime.UtcNow;
        _context.ServiceRequests.AddRange(
            CreateRequest(false, false),
            CreateRequest(false, false),
            CreateRequest(false, false),
            CreateRequest(true, false)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(now.AddMinutes(-1), now.AddMinutes(1));

        result.ComplianceRate.Should().BeApproximately(75.0, 0.1);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldCalculateAvgResponseTime_WhenRespondedTicketsExist()
    {
        var now = DateTime.UtcNow;
        var createdAt = now.AddMinutes(-60);

        var sr = CreateRequest(false, false, firstResponseDate: now.AddMinutes(-30));
        sr.CreatedAt = createdAt;
        _context.ServiceRequests.Add(sr);
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(createdAt.AddMinutes(-1), now.AddMinutes(1));

        result.AvgResponseTimeMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldGroupBreachesByPriority()
    {
        var now = DateTime.UtcNow;
        _context.ServiceRequests.AddRange(
            CreateRequest(true, false, priority: ServiceRequestPriority.High),
            CreateRequest(false, true, priority: ServiceRequestPriority.High),
            CreateRequest(true, false, priority: ServiceRequestPriority.Low)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(now.AddMinutes(-1), now.AddMinutes(1));

        result.BreachesByPriority.Should().ContainKey("High");
        result.BreachesByPriority["High"].Should().Be(2);
        result.BreachesByPriority.Should().ContainKey("Low");
        result.BreachesByPriority["Low"].Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnDailyTrend()
    {
        var now = DateTime.UtcNow;
        _context.ServiceRequests.AddRange(
            CreateRequest(false, false),
            CreateRequest(true, false)
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(now.AddMinutes(-1), now.AddMinutes(1));

        result.DailyTrend.Should().NotBeEmpty();
        result.DailyTrend[0].TotalTickets.Should().Be(2);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldExcludeTicketsOutsideDateRange()
    {
        var pastDate = DateTime.UtcNow.AddDays(-10);

        var sr = CreateRequest(true, true);
        sr.CreatedAt = pastDate;
        _context.ServiceRequests.Add(sr);
        await _context.SaveChangesAsync();

        var result = await _service.GetDashboardAsync(
            DateTime.UtcNow.AddMinutes(-1),
            DateTime.UtcNow.AddMinutes(1));

        result.TotalTickets.Should().Be(0);
    }
}
