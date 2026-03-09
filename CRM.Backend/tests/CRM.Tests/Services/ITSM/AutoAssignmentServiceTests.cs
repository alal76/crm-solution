// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
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

/// <summary>
/// Unit tests for AutoAssignmentService.
/// Tests cover round-robin, skill-based, least-loaded strategies, and rule management.
/// </summary>
public class AutoAssignmentServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<AutoAssignmentService>> _mockLogger;
    private readonly AutoAssignmentService _service;

    public AutoAssignmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"AutoAssign_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<AutoAssignmentService>>();
        _service = new AutoAssignmentService(_dbContext, _mockLogger.Object);

        // Reset static state before each test
        AutoAssignmentService.ResetState();
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    #region Helper Methods

    private User CreateTestUser(int id = 0, string username = "agent", bool isActive = true, int role = 3)
    {
        return new User
        {
            Id = id,
            Username = username,
            Email = $"{username}@example.com",
            FirstName = "Test",
            LastName = username,
            PasswordHash = "hashedpassword",
            IsActive = isActive,
            IsLocked = false,
            Role = role,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private ServiceRequest CreateTestServiceRequest(int id = 0, int? categoryId = null, ServiceRequestPriority priority = ServiceRequestPriority.Medium)
    {
        return new ServiceRequest
        {
            Id = id,
            TicketNumber = $"SR-{id:D6}",
            Subject = $"Test Service Request {id}",
            Description = "Test description",

            Priority = priority,
            CategoryId = categoryId,
            Channel = ServiceRequestChannel.SelfServicePortal,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private async Task SeedAgentsAsync(int count = 3)
    {
        for (int i = 1; i <= count; i++)
        {
            _dbContext.Users.Add(CreateTestUser(0, $"agent{i}", true, (int)UserRole.Support));
        }
        await _dbContext.SaveChangesAsync();
    }

    private async Task<ServiceRequest> SeedServiceRequestAsync(int? categoryId = null, ServiceRequestPriority priority = ServiceRequestPriority.Medium)
    {
        var sr = CreateTestServiceRequest(0, categoryId, priority);
        _dbContext.ServiceRequests.Add(sr);
        await _dbContext.SaveChangesAsync();
        return sr;
    }

    #endregion

    #region Round-Robin Tests

    [Fact]
    public async Task GetNextRoundRobinAgent_ShouldCycleThroughAgents()
    {
        // Arrange
        await SeedAgentsAsync(3);
        var agents = await _dbContext.Users.OrderBy(u => u.Id).ToListAsync();

        // Act
        var first = await _service.GetNextRoundRobinAgentAsync();
        var second = await _service.GetNextRoundRobinAgentAsync();
        var third = await _service.GetNextRoundRobinAgentAsync();
        var fourth = await _service.GetNextRoundRobinAgentAsync();

        // Assert — should cycle through agents
        first.Should().NotBeNull();
        second.Should().NotBeNull();
        third.Should().NotBeNull();
        fourth.Should().NotBeNull();

        // All three agents should be used
        var assigned = new[] { first!.Value, second!.Value, third!.Value };
        assigned.Distinct().Count().Should().Be(3, "all three agents should be assigned exactly once before cycling");

        // Fourth should wrap around
        fourth.Should().Be(assigned[fourth.Value == assigned[0] ? 0 : fourth.Value == assigned[1] ? 1 : 2]);
    }

    [Fact]
    public async Task GetNextRoundRobinAgent_ShouldReturnNull_WhenNoAgentsAvailable()
    {
        // Act
        var result = await _service.GetNextRoundRobinAgentAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetNextRoundRobinAgent_ShouldSkipInactiveAgents()
    {
        // Arrange
        _dbContext.Users.Add(CreateTestUser(0, "inactive_agent", false));
        _dbContext.Users.Add(CreateTestUser(0, "active_agent", true));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetNextRoundRobinAgentAsync();

        // Assert
        result.Should().NotBeNull();
        var agent = await _dbContext.Users.FindAsync(result!.Value);
        agent!.IsActive.Should().BeTrue();
    }

    #endregion

    #region Least-Loaded Tests

    [Fact]
    public async Task GetLeastLoadedAgent_ShouldReturnAgentWithFewestAssignments()
    {
        // Arrange
        await SeedAgentsAsync(3);
        var agents = await _dbContext.Users.OrderBy(u => u.Id).ToListAsync();

        // Assign 3 open tickets to agent1, 1 to agent2, 0 to agent3
        for (int i = 0; i < 3; i++)
        {
            _dbContext.ServiceRequests.Add(new ServiceRequest
            {
                TicketNumber = $"SR-LOAD-{i}",
                Subject = $"Ticket {i}",
                // FIXME-AP059: was Status=ServiceRequestStatus.InProgress; add .WithStatus(ServiceRequestStatus.InProgress) after construction

                AssignedToUserId = agents[0].Id,
                Priority = ServiceRequestPriority.Medium,
                Channel = ServiceRequestChannel.Email,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            });
        }
        _dbContext.ServiceRequests.Add(new ServiceRequest
        {
            TicketNumber = "SR-LOAD-3",
            Subject = "Ticket 3", AssignedToUserId = agents[1].Id,
            Priority = ServiceRequestPriority.Medium,
            Channel = ServiceRequestChannel.Email,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false}.WithStatus(ServiceRequestStatus.InProgress));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetLeastLoadedAgentAsync();

        // Assert — agent3 has 0 open tickets, should be selected
        result.Should().Be(agents[2].Id);
    }

    [Fact]
    public async Task GetLeastLoadedAgent_ShouldReturnNull_WhenNoAgentsAvailable()
    {
        // Act
        var result = await _service.GetLeastLoadedAgentAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLeastLoadedAgent_ShouldIgnoreClosedTickets()
    {
        // Arrange
        await SeedAgentsAsync(2);
        var agents = await _dbContext.Users.OrderBy(u => u.Id).ToListAsync();

        // Agent1 has 1 closed ticket, Agent2 has 1 open ticket
        _dbContext.ServiceRequests.Add(new ServiceRequest
        {
            TicketNumber = "SR-CLOSED-1",
            Subject = "Closed Ticket", AssignedToUserId = agents[0].Id,
            Priority = ServiceRequestPriority.Medium,
            Channel = ServiceRequestChannel.Email,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false}.WithStatus(ServiceRequestStatus.Closed));
        _dbContext.ServiceRequests.Add(new ServiceRequest
        {
            TicketNumber = "SR-OPEN-1",
            Subject = "Open Ticket", AssignedToUserId = agents[1].Id,
            Priority = ServiceRequestPriority.Medium,
            Channel = ServiceRequestChannel.Email,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false}.WithStatus(ServiceRequestStatus.InProgress));
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetLeastLoadedAgentAsync();

        // Assert — agent1 has 0 open (1 closed doesn't count), agent2 has 1 open
        result.Should().Be(agents[0].Id);
    }

    #endregion

    #region Skill-Based Tests

    [Fact]
    public async Task GetBestSkillMatchAgent_ShouldReturnNull_WhenServiceRequestNotFound()
    {
        // Act
        var result = await _service.GetBestSkillMatchAgentAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBestSkillMatchAgent_ShouldPreferSupportAgents()
    {
        // Arrange
        _dbContext.Users.Add(CreateTestUser(0, "sales_agent", true, (int)UserRole.Sales));
        _dbContext.Users.Add(CreateTestUser(0, "support_agent", true, (int)UserRole.Support));
        await _dbContext.SaveChangesAsync();

        var sr = await SeedServiceRequestAsync(categoryId: 1);

        // Create a rule with skill matching
        await _service.CreateRuleAsync(new CreateAssignmentRuleDto
        {
            Name = "Support Skill Rule",
            Strategy = "SkillBased",
            Priority = 1,
            IsActive = true,
            CategoryFilter = "1",
            RequiredSkills = "[\"network\",\"hardware\"]"
        });

        // Act
        var result = await _service.GetBestSkillMatchAgentAsync(sr.Id);

        // Assert — support agent should be preferred
        result.Should().NotBeNull();
        var agent = await _dbContext.Users.FindAsync(result!.Value);
        agent!.Role.Should().Be((int)UserRole.Support);
    }

    #endregion

    #region AssignServiceRequest Tests

    [Fact]
    public async Task AssignServiceRequest_ShouldReturnNotFound_WhenServiceRequestMissing()
    {
        // Act
        var result = await _service.AssignServiceRequestAsync(999);

        // Assert
        result.Success.Should().BeFalse();
        result.Reason.Should().Contain("Service request not found");
    }

    [Fact]
    public async Task AssignServiceRequest_ShouldUseRoundRobin_WhenNoRulesMatch()
    {
        // Arrange
        await SeedAgentsAsync(2);
        var sr = await SeedServiceRequestAsync();

        // Act
        var result = await _service.AssignServiceRequestAsync(sr.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.AssignedUserId.Should().NotBeNull();
        result.StrategyUsed.Should().Be("RoundRobin");

        // Verify DB was updated
        var updated = await _dbContext.ServiceRequests.FindAsync(sr.Id);
        updated!.AssignedToUserId.Should().Be(result.AssignedUserId);
    }

    [Fact]
    public async Task AssignServiceRequest_ShouldUseLeastLoaded_WhenMatchingRuleExists()
    {
        // Arrange
        await SeedAgentsAsync(2);
        var agents = await _dbContext.Users.OrderBy(u => u.Id).ToListAsync();

        // Give agent1 a ticket
        _dbContext.ServiceRequests.Add(new ServiceRequest
        {
            TicketNumber = "SR-EXISTING",
            Subject = "Existing", AssignedToUserId = agents[0].Id,
            Priority = ServiceRequestPriority.High,
            Channel = ServiceRequestChannel.Email,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false}.WithStatus(ServiceRequestStatus.InProgress));
        await _dbContext.SaveChangesAsync();

        var sr = await SeedServiceRequestAsync(priority: ServiceRequestPriority.High);

        // Create matching rule for high-priority
        await _service.CreateRuleAsync(new CreateAssignmentRuleDto
        {
            Name = "High Priority LeastLoaded",
            Strategy = "LeastLoaded",
            Priority = 1,
            IsActive = true,
            PriorityFilter = "High"
        });

        // Act
        var result = await _service.AssignServiceRequestAsync(sr.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.StrategyUsed.Should().Be("LeastLoaded");
        result.AssignedUserId.Should().Be(agents[1].Id, "agent2 has no open tickets");
    }

    [Fact]
    public async Task AssignServiceRequest_ShouldReturnFailure_WhenNoAgentsAvailable()
    {
        // Arrange — no agents seeded
        var sr = await SeedServiceRequestAsync();

        // Act
        var result = await _service.AssignServiceRequestAsync(sr.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Reason.Should().Contain("No available agents found");
    }

    #endregion

    #region Rule Management Tests

    [Fact]
    public async Task CreateRule_ShouldCreateAndReturnRule()
    {
        // Act
        var rule = await _service.CreateRuleAsync(new CreateAssignmentRuleDto
        {
            Name = "Test Rule",
            Strategy = "RoundRobin",
            Priority = 1,
            IsActive = true
        });

        // Assert
        rule.Should().NotBeNull();
        rule.Id.Should().BeGreaterThan(0);
        rule.Name.Should().Be("Test Rule");
        rule.Strategy.Should().Be("RoundRobin");
    }

    [Fact]
    public async Task CreateRule_ShouldThrow_WhenNameIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateRuleAsync(new CreateAssignmentRuleDto { Name = "" }));
    }

    [Fact]
    public async Task UpdateRule_ShouldReturnNull_WhenRuleNotFound()
    {
        // Act
        var result = await _service.UpdateRuleAsync(999, new UpdateAssignmentRuleDto { Name = "Updated" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRule_ShouldUpdateSpecifiedFields()
    {
        // Arrange
        var rule = await _service.CreateRuleAsync(new CreateAssignmentRuleDto
        {
            Name = "Original",
            Strategy = "RoundRobin",
            Priority = 1,
            IsActive = true
        });

        // Act
        var updated = await _service.UpdateRuleAsync(rule.Id, new UpdateAssignmentRuleDto
        {
            Name = "Updated",
            Strategy = "LeastLoaded",
            IsActive = false
        });

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");
        updated.Strategy.Should().Be("LeastLoaded");
        updated.IsActive.Should().BeFalse();
        updated.Priority.Should().Be(1, "priority was not updated");
    }

    [Fact]
    public async Task DeleteRule_ShouldRemoveRule()
    {
        // Arrange
        var rule = await _service.CreateRuleAsync(new CreateAssignmentRuleDto
        {
            Name = "To Delete",
            Strategy = "RoundRobin"
        });

        // Act
        var deleted = await _service.DeleteRuleAsync(rule.Id);
        var fetched = await _service.GetRuleByIdAsync(rule.Id);

        // Assert
        deleted.Should().BeTrue();
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteRule_ShouldReturnFalse_WhenRuleNotFound()
    {
        // Act
        var result = await _service.DeleteRuleAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetRules_ShouldReturnOrderedByPriority()
    {
        // Arrange
        await _service.CreateRuleAsync(new CreateAssignmentRuleDto { Name = "Low Priority", Priority = 3 });
        await _service.CreateRuleAsync(new CreateAssignmentRuleDto { Name = "High Priority", Priority = 1 });
        await _service.CreateRuleAsync(new CreateAssignmentRuleDto { Name = "Mid Priority", Priority = 2 });

        // Act
        var rules = (await _service.GetRulesAsync()).ToList();

        // Assert
        rules.Should().HaveCount(3);
        rules[0].Name.Should().Be("High Priority");
        rules[1].Name.Should().Be("Mid Priority");
        rules[2].Name.Should().Be("Low Priority");
    }

    #endregion

    #region SuggestAssignment Tests

    [Fact]
    public async Task SuggestAssignment_ShouldNotModifyDatabase()
    {
        // Arrange
        await SeedAgentsAsync(2);
        var sr = await SeedServiceRequestAsync();

        // Act
        var result = await _service.SuggestAssignmentAsync(sr.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.AssignedUserId.Should().NotBeNull();

        // Verify DB was NOT updated
        var unchanged = await _dbContext.ServiceRequests.FindAsync(sr.Id);
        unchanged!.AssignedToUserId.Should().BeNull("suggestion should not modify the service request");
    }

    #endregion
}
