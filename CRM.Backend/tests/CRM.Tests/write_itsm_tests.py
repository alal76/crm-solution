
import os

BASE = "/Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests"

files = {}

# TCOV-025 ITSM IncidentServiceTests
files["Services/ITSM/IncidentServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
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

namespace CRM.Tests.Services.ITSM;

public class IncidentServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ISLAService> _mockSlaService;
    private readonly Mock<ILogger<IncidentService>> _mockLogger;
    private readonly IncidentService _service;

    public IncidentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"IncidentTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockSlaService = new Mock<ISLAService>();
        _mockLogger = new Mock<ILogger<IncidentService>>();
        _service = new IncidentService(_context, _mockSlaService.Object, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetIncidentByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldAddIncidentToDatabase()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Test Incident",
            Description = "A test incident for unit testing",
            CallerId = 1,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low
        };

        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Test Incident");
        _context.Incidents.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldGenerateIncidentNumber()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "Numbered Incident",
            CallerId = 1,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.Medium
        };

        var result = await _service.CreateIncidentAsync(dto, createdById: 1);

        result.Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateIncidentAsync_ShouldCallSLAService()
    {
        var dto = new CreateIncidentDto
        {
            ShortDescription = "SLA Test",
            CallerId = 1,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High
        };

        await _service.CreateIncidentAsync(dto, createdById: 1);

        _mockSlaService.Verify(s => s.StartSLAAsync(It.IsAny<int>(), SLATargetType.Incident, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task GetIncidentByIdAsync_ShouldReturnIncident_WhenExists()
    {
        var incident = new Incident
        {
            IncidentId = 10,
            Number = "INC0000001",
            ShortDescription = "Existing Incident",
            State = IncidentState.New,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low,
            Priority = 4,
            CallerId = 1,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        var result = await _service.GetIncidentByIdAsync(10);

        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Existing Incident");
    }

    [Fact]
    public async Task GetIncidentsAsync_ShouldReturnAllIncidents_WhenNoFilter()
    {
        var incidents = new[]
        {
            new Incident { IncidentId = 1, Number = "INC001", ShortDescription = "Inc 1", State = IncidentState.New, Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Low, Priority = 4, CallerId = 1, CreatedById = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Incident { IncidentId = 2, Number = "INC002", ShortDescription = "Inc 2", State = IncidentState.InProgress, Impact = IncidentImpact.Medium, Urgency = IncidentUrgency.Medium, Priority = 3, CallerId = 1, CreatedById = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        _context.Incidents.AddRange(incidents);
        await _context.SaveChangesAsync();

        var filter = new IncidentFilterDto();
        var result = await _service.GetIncidentsAsync(filter);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldAddCommentToIncident()
    {
        var incident = new Incident
        {
            IncidentId = 20,
            Number = "INC0000020",
            ShortDescription = "Comment Test",
            State = IncidentState.New,
            Impact = IncidentImpact.Low,
            Urgency = IncidentUrgency.Low,
            Priority = 4,
            CallerId = 1,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        var dto = new AddIncidentCommentDto
        {
            Comment = "This is a test comment",
            IsPublic = true
        };

        var result = await _service.AddCommentAsync(20, dto, authorId: 1);

        result.Should().NotBeNull();
        result.Comment.Should().Be("This is a test comment");
        _context.IncidentComments.Count().Should().Be(1);
    }
}
"""

# TCOV-026 ITSM SLAServiceTests
files["Services/ITSM/SLAServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
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
    public async Task StartSLAAsync_ShouldReturnNull_WhenNoPolicyConfigured()
    {
        // No policies in DB
        var result = await _service.StartSLAAsync(1, SLATargetType.Incident, priority: 1);
        result.Should().BeNull();
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
        var result = await _service.GetSLAPoliciesAsync();
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.GetSLAPoliciesAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Default Policy");
    }

    [Fact]
    public async Task StartSLAAsync_ShouldCreateSLAInstance_WhenActivePolicyExists()
    {
        _mockBizHours.Setup(b => b.AddBusinessMinutesAsync(It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync((DateTime dt, int mins) => dt.AddMinutes(mins));

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var result = await _service.StartSLAAsync(1, SLATargetType.Incident, priority: 1);

        result.Should().NotBeNull();
        _context.ITSMSLAInstances.Count().Should().Be(1);
    }
}
"""

# TCOV-027 ITSM ProblemManagementServiceTests
files["Services/ITSM/ProblemManagementServiceTests.cs"] = """// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class ProblemManagementServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<ILogger<ProblemManagementService>> _mockLogger;
    private readonly ProblemManagementService _service;

    public ProblemManagementServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"ProblemManagementTestDb_{Guid.NewGuid()}")
            .Options;
        _context = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<ProblemManagementService>>();
        _service = new ProblemManagementService(_context, _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task CreateProblemAsync_ShouldThrow_WhenShortDescriptionIsEmpty()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "",
            Priority = ProblemPriority.P3
        };

        var act = async () => await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldAddProblemToDatabase_WhenValid()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "Application crashes on login",
            Description = "Users report crash when accessing the login screen",
            Priority = ProblemPriority.P2
        };

        var result = await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        result.Should().NotBeNull();
        result.ShortDescription.Should().Be("Application crashes on login");
        _context.Problems.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldGenerateProblemNumber()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "Numbered Problem",
            Priority = ProblemPriority.P4
        };

        var result = await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        result.Number.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetProblemByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetProblemByIdAsync(999, CancellationToken.None);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProblemByIdAsync_ShouldReturnProblem_WhenExists()
    {
        var problem = new Problem
        {
            ProblemId = 5,
            Number = "PRB0000001",
            ShortDescription = "Existing Problem",
            State = ProblemState.New,
            Priority = ProblemPriority.P3,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var result = await _service.GetProblemByIdAsync(5, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ShortDescription.Should().Be("Existing Problem");
    }

    [Fact]
    public async Task ListProblemsAsync_ShouldReturnAll_WhenNoFilter()
    {
        var problems = new[]
        {
            new Problem { ProblemId = 1, Number = "PRB001", ShortDescription = "Prob 1", State = ProblemState.New, Priority = ProblemPriority.P1, CreatedById = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Problem { ProblemId = 2, Number = "PRB002", ShortDescription = "Prob 2", State = ProblemState.InProgress, Priority = ProblemPriority.P2, CreatedById = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        _context.Problems.AddRange(problems);
        await _context.SaveChangesAsync();

        var result = await _service.ListProblemsAsync(new ProblemFilterDto(), CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResolveProblemAsync_ShouldSetStateToResolved()
    {
        var problem = new Problem
        {
            ProblemId = 10,
            Number = "PRB0000010",
            ShortDescription = "Resolve Me",
            State = ProblemState.InProgress,
            Priority = ProblemPriority.P3,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var dto = new ResolveProblemDto
        {
            RootCause = "Found root cause",
            Solution = "Applied the fix",
            Workaround = null
        };

        var result = await _service.ResolveProblemAsync(10, dto, resolvedById: 1, CancellationToken.None);

        result.Should().NotBeNull();
        result!.State.Should().Be(ProblemState.Resolved);
    }

    [Fact]
    public async Task CreateProblemAsync_ShouldThrow_WhenShortDescriptionIsWhitespace()
    {
        var dto = new CreateProblemDto
        {
            ShortDescription = "   ",
            Priority = ProblemPriority.P3
        };

        var act = async () => await _service.CreateProblemAsync(dto, createdById: 1, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
"""

for rel_path, content in files.items():
    full_path = os.path.join(BASE, rel_path)
    os.makedirs(os.path.dirname(full_path), exist_ok=True)
    with open(full_path, "w") as f:
        f.write(content.lstrip("\n"))
    print(f"Written: {rel_path}")

print("Done.")
