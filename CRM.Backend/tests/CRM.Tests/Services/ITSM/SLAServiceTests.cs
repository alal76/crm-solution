// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using IBusinessHoursCalculator = CRM.Infrastructure.Services.ITSM.IBusinessHoursCalculator;

namespace CRM.Tests.Services.ITSM;

public class SLAServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<IBusinessHoursCalculator> _mockBizHours;
    private readonly Mock<ILogger<SLAService>> _mockLogger;
    private readonly SLAService _service;

    public SLAServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SLAServiceTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockBizHours = new Mock<IBusinessHoursCalculator>();
        _mockLogger = new Mock<ILogger<SLAService>>();
        _service = new SLAService(_context, _mockBizHours.Object, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateSLAPolicyAsync_ShouldAddPolicyToDatabase()
    {
        var dto = new SLAPolicyDto
        {
            Name = "P1 SLA Policy",
            TargetType = SLATargetType.Incident,
            P1ResolutionMinutes = 60,
            P1ResponseMinutes = 15,
            P2ResolutionMinutes = 240,
            P2ResponseMinutes = 60,
            P3ResolutionMinutes = 1440,
            P3ResponseMinutes = 240,
            P4ResolutionMinutes = 4320,
            P4ResponseMinutes = 480,
            UseBusinessHours = true,
            IsActive = true
        };

        var result = await _service.CreateSLAPolicyAsync(dto, createdById: 1);

        result.Should().NotBeNull();
        result.Name.Should().Be("P1 SLA Policy");
        _context.ITSMSLAPolicies.Count().Should().Be(1);
    }

    [Fact]
    public async Task StartSLAAsync_ShouldNotThrow_WhenNoPolicyConfigured()
    {
        // No policies in DB - StartSLAAsync returns Task (void), just verify it doesn't throw
        var act = async () => await ((ISLAService)_service).StartSLAAsync(1, SLATargetType.Incident, priority: 1);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CreateSLAPolicyAsync_ShouldSetIsActive()
    {
        var dto = new SLAPolicyDto
        {
            Name = "Active Policy",
            TargetType = SLATargetType.ServiceRequest,
            P1ResolutionMinutes = 60,
            P1ResponseMinutes = 15,
            P2ResolutionMinutes = 240,
            P2ResponseMinutes = 60,
            P3ResolutionMinutes = 1440,
            P3ResponseMinutes = 240,
            P4ResolutionMinutes = 4320,
            P4ResponseMinutes = 480,
            UseBusinessHours = false,
            IsActive = true
        };

        var result = await _service.CreateSLAPolicyAsync(dto, createdById: 1);

        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetSLAPoliciesAsync_ShouldReturnEmpty_WhenNoPolicies()
    {
        var result = await _service.GetSLAPoliciesAsync(null);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSLAPoliciesAsync_ShouldReturnPolicies_WhenExist()
    {
        _context.ITSMSLAPolicies.Add(new SLAPolicy
        {
            SLAPolicyId = 1,
            Name = "Default Policy",
            TargetType = SLATargetType.Incident,
            P1ResolutionMinutes = 60,
            P1ResponseMinutes = 15,
            P2ResolutionMinutes = 240,
            P2ResponseMinutes = 60,
            P3ResolutionMinutes = 1440,
            P3ResponseMinutes = 240,
            P4ResolutionMinutes = 4320,
            P4ResponseMinutes = 480,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetSLAPoliciesAsync(null);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Default Policy");
    }

    [Fact]
    public async Task StartSLAAsync_ShouldCreateSLAInstance_WhenActivePolicyExists()
    {
_mockBizHours.Setup(b => b.AddBusinessMinutesAsync(It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<int?>()))
                          .ReturnsAsync((DateTime dt, int mins, int? tz) => dt.AddMinutes(mins));

        _context.ITSMSLAPolicies.Add(new SLAPolicy
        {
            SLAPolicyId = 1,
            Name = "Test SLA",
            TargetType = SLATargetType.Incident,
            P1ResolutionMinutes = 60,
            P1ResponseMinutes = 15,
            P2ResolutionMinutes = 240,
            P2ResponseMinutes = 60,
            P3ResolutionMinutes = 1440,
            P3ResponseMinutes = 240,
            P4ResolutionMinutes = 4320,
            P4ResponseMinutes = 480,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // StartSLAAsync is an explicit interface implementation returning Task
        await ((ISLAService)_service).StartSLAAsync(1, SLATargetType.Incident, priority: 1);

        _context.ITSMSLAInstances.Count().Should().Be(1);
    }
}
