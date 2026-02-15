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

using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class SLAPolicyAdminServiceTests
{
    private readonly Mock<IRepository<SLAPolicy>> _mockPolicyRepository;
    private readonly Mock<IRepository<SLAInstance>> _mockInstanceRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<SLAPolicyAdminService>> _mockLogger;
    private readonly SLAPolicyAdminService _service;

    public SLAPolicyAdminServiceTests()
    {
        _mockPolicyRepository = new Mock<IRepository<SLAPolicy>>();
        _mockInstanceRepository = new Mock<IRepository<SLAInstance>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SLAPolicyAdminService>>();
        _service = new SLAPolicyAdminService(_mockPolicyRepository.Object, _mockInstanceRepository.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedPolicy()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto
        {
            Name = "Critical Priority SLA",
            Priority = "Critical",
            ResponseTimeHours = 2,
            ResolutionTimeHours = 24
        };

        _mockPolicyRepository.Setup(r => r.AddAsync(It.IsAny<SLAPolicy>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(2, result.ResponseTimeHours);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimes_ThrowsException()
    {
        // Arrange
        var dto = new CreateSLAPolicyDto { Name = "Test", ResponseTimeHours = -1, ResolutionTimeHours = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task AssignPolicyAsync_WithValidIds_CreatesInstance()
    {
        // Arrange
        var policy = new SLAPolicy { Id = 1, Name = "Test", ResponseTimeHours = 2, ResolutionTimeHours = 24 };
        _mockPolicyRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        _mockInstanceRepository.Setup(r => r.AddAsync(It.IsAny<SLAInstance>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.AssignPolicyAsync(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.ServiceRequestId);
        Assert.Equal(1, result.PolicyId);
    }

    [Fact]
    public async Task GetApplicablePoliciesAsync_FiltersByPriority_ReturnsMatchingPolicies()
    {
        // Arrange
        var policies = new List<SLAPolicy>
        {
            new SLAPolicy { Id = 1, Name = "Critical", Priority = "Critical", IsActive = true },
            new SLAPolicy { Id = 2, Name = "High", Priority = "High", IsActive = true }
        };

        _mockPolicyRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(policies);

        // Act
        var result = await _service.GetApplicablePoliciesAsync("Critical", null);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Critical", result.First().Name);
    }
}

public class EscalationRuleAdminServiceTests
{
    private readonly Mock<IRepository<EscalationRule>> _mockRuleRepository;
    private readonly Mock<IRepository<ServiceRequest>> _mockSrRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<EscalationRuleAdminService>> _mockLogger;
    private readonly EscalationRuleAdminService _service;

    public EscalationRuleAdminServiceTests()
    {
        _mockRuleRepository = new Mock<IRepository<EscalationRule>>();
        _mockSrRepository = new Mock<IRepository<ServiceRequest>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EscalationRuleAdminService>>();
        _service = new EscalationRuleAdminService(_mockRuleRepository.Object, _mockSrRepository.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedRule()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "Auto-Escalate Critical",
            Priority = "Critical",
            AgeInMinutes = 60,
            TargetType = "User"
        };

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<EscalationRule>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(60, result.AgeInMinutes);
    }

    [Fact]
    public async Task TestRuleAsync_WithMatchingConditions_ReturnsMatched()
    {
        // Arrange
        var rule = new EscalationRule { Id = 1, Priority = "Critical", IsActive = true };
        var sr = new ServiceRequest { Id = 100, Priority = "Critical" };

        _mockRuleRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rule);
        _mockSrRepository.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(sr);

        // Act
        var result = await _service.TestRuleAsync(1, 100);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.RuleMatched);
    }

    [Fact]
    public async Task GetApplicableRulesAsync_FiltersByPriority_ReturnsMatchingRules()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            new EscalationRule { Id = 1, Priority = "Critical", IsActive = true },
            new EscalationRule { Id = 2, Priority = "High", IsActive = true }
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rules);

        // Act
        var result = await _service.GetApplicableRulesAsync("Critical");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Critical", result.First().Priority);
    }
}

public class ServiceQueueServiceTests
{
    private readonly Mock<IRepository<ServiceQueue>> _mockQueueRepository;
    private readonly Mock<IRepository<ServiceRequest>> _mockSrRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ServiceQueueService>> _mockLogger;
    private readonly ServiceQueueService _service;

    public ServiceQueueServiceTests()
    {
        _mockQueueRepository = new Mock<IRepository<ServiceQueue>>();
        _mockSrRepository = new Mock<IRepository<ServiceRequest>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ServiceQueueService>>();
        _service = new ServiceQueueService(_mockQueueRepository.Object, _mockSrRepository.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedQueue()
    {
        // Arrange
        var dto = new CreateServiceQueueDto { Name = "Support Queue", Priority = 5 };

        _mockQueueRepository.Setup(r => r.AddAsync(It.IsAny<ServiceQueue>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(5, result.Priority);
    }

    [Fact]
    public async Task AssignToQueueAsync_WithValidData_AssignsServiceRequest()
    {
        // Arrange
        var sr = new ServiceRequest { Id = 100, Queue = "Old Queue" };
        var queue = new ServiceQueue { Id = 1, Name = "Support Queue" };

        _mockSrRepository.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(sr);
        _mockQueueRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(queue);
        _mockSrRepository.Setup(r => r.UpdateAsync(It.IsAny<ServiceRequest>(), It.IsAny<CancellationToken>()));

        // Act
        await _service.AssignToQueueAsync(100, 1);

        // Assert
        Assert.Equal("Support Queue", sr.Queue);
        _mockSrRepository.Verify(r => r.UpdateAsync(sr, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetQueueItemsAsync_WithValidQueueId_ReturnsItems()
    {
        // Arrange
        var queue = new ServiceQueue { Id = 1, Name = "Support Queue" };
        var items = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 100, Queue = "Support Queue", Title = "Issue 1", IsDeleted = false, Status = "Open" },
            new ServiceRequest { Id = 101, Queue = "Support Queue", Title = "Issue 2", IsDeleted = false, Status = "Open" }
        };

        _mockQueueRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(queue);
        _mockSrRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(items);

        // Act
        var result = await _service.GetQueueItemsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }
}
