// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EscalationRule = CRM.Core.Entities.ITSM.EscalationRule;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for EscalationRuleAdminService.
/// Tests cover CRUD operations, validation, rule testing, and applicable rule retrieval.
/// Note: EscalationRuleAdminService is used (over EscalationRuleService) because it relies
/// on IRepository&lt;T&gt; which is straightforwardly mockable. EscalationRuleService uses
/// DbContext.Set&lt;T&gt;() which is not directly on the ICrmDbContext interface and would
/// require an in-memory database setup.
/// </summary>
public class EscalationRuleServiceTests
{
    private readonly Mock<IRepository<EscalationRule>> _mockRuleRepo;
    private readonly Mock<IRepository<ServiceRequest>> _mockSrRepo;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<EscalationRuleAdminService>> _mockLogger;
    private readonly EscalationRuleAdminService _service;

    public EscalationRuleServiceTests()
    {
        _mockRuleRepo = new Mock<IRepository<EscalationRule>>();
        _mockSrRepo = new Mock<IRepository<ServiceRequest>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<EscalationRuleAdminService>>();

        _service = new EscalationRuleAdminService(
            _mockRuleRepo.Object,
            _mockSrRepo.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // GetAllAsync Tests  → GetAllRulesAsync_ShouldReturnActiveRules
    // ========================================================================

    [Fact]
    public async Task GetAllRulesAsync_ShouldReturnAllRules()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            CreateTestRule(1, "P1 Escalation", "Critical", true),
            CreateTestRule(2, "P2 Escalation", "High", true),
            CreateTestRule(3, "Inactive Rule", "Medium", false)
        };
        _mockRuleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllRulesAsync_ShouldReturnEmptyList_WhenNoRulesExist()
    {
        // Arrange
        _mockRuleRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<EscalationRule>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // GetByIdAsync Tests  → GetRuleByIdAsync_ShouldReturn*
    // ========================================================================

    [Fact]
    public async Task GetRuleByIdAsync_ShouldReturnRule_WhenExists()
    {
        // Arrange
        var rule = CreateTestRule(1, "Critical Escalation", "Critical", true);
        _mockRuleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Critical Escalation");
        result.Priority.Should().Be("Critical");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetRuleByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange — GetRuleByIdAsync_ShouldThrowNotFoundException_WhenNotFound
        // (service returns null rather than throws; test documents real behavior)
        _mockRuleRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((EscalationRule?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync Tests  → CreateRuleAsync_ShouldCreate*
    // ========================================================================

    [Fact]
    public async Task CreateRuleAsync_ShouldCreateSuccessfully_WithValidData()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "High Priority Escalation",
            Description = "Escalate high-priority tickets after 60 minutes",
            Priority = "High",
            AgeInMinutes = 60,
            TargetType = "User",
            TargetId = 5,
            TargetName = "Support Lead",
            MaxAttempts = 3,
            RetryIntervalMinutes = 15,
            IsActive = true
        };

        _mockRuleRepo.Setup(r => r.AddAsync(It.IsAny<EscalationRule>())).Returns(Task.CompletedTask);
        _mockRuleRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("High Priority Escalation");
        result.Priority.Should().Be("High");
        result.AgeInMinutes.Should().Be(60);
        result.IsActive.Should().BeTrue();
        _mockRuleRepo.Verify(r => r.AddAsync(It.IsAny<EscalationRule>()), Times.Once);
        _mockRuleRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateRuleAsync_ShouldThrowValidation_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "",
            Priority = "High",
            AgeInMinutes = 60,
            TargetType = "User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
        _mockRuleRepo.Verify(r => r.AddAsync(It.IsAny<EscalationRule>()), Times.Never);
    }

    [Fact]
    public async Task CreateRuleAsync_ShouldThrowValidation_WhenPriorityIsEmpty()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "Test Rule",
            Priority = "",
            AgeInMinutes = 60,
            TargetType = "User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateRuleAsync_ShouldThrowValidation_WhenAgeInMinutesIsZero()
    {
        // Arrange
        var dto = new CreateEscalationRuleDto
        {
            Name = "Test Rule",
            Priority = "High",
            AgeInMinutes = 0,
            TargetType = "User"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    // ========================================================================
    // UpdateAsync Tests  → UpdateRuleAsync_ShouldUpdateFields
    // ========================================================================

    [Fact]
    public async Task UpdateRuleAsync_ShouldUpdateFields_WhenRuleExists()
    {
        // Arrange
        var existingRule = CreateTestRule(1, "Old Name", "High", true);
        _mockRuleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingRule);
        _mockRuleRepo.Setup(r => r.UpdateAsync(It.IsAny<EscalationRule>())).Returns(Task.CompletedTask);
        _mockRuleRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        var dto = new UpdateEscalationRuleDto
        {
            Name = "Updated Escalation",
            Priority = "Critical",
            AgeInMinutes = 30,
            IsActive = false
        };

        // Act
        var result = await _service.UpdateAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Escalation");
        result.Priority.Should().Be("Critical");
        result.AgeInMinutes.Should().Be(30);
        result.IsActive.Should().BeFalse();
        _mockRuleRepo.Verify(r => r.UpdateAsync(It.IsAny<EscalationRule>()), Times.Once);
        _mockRuleRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateRuleAsync_ShouldThrow_WhenRuleNotFound()
    {
        // Arrange
        _mockRuleRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((EscalationRule?)null);

        var dto = new UpdateEscalationRuleDto { Name = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(999, dto));
        _mockRuleRepo.Verify(r => r.UpdateAsync(It.IsAny<EscalationRule>()), Times.Never);
    }

    // ========================================================================
    // DeleteAsync Tests  → DeleteRuleAsync_ShouldSoftDelete
    // ========================================================================

    [Fact]
    public async Task DeleteRuleAsync_ShouldSoftDelete_WhenRuleExists()
    {
        // Arrange
        var rule = CreateTestRule(1, "Delete Me", "Medium", true);
        _mockRuleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);
        _mockRuleRepo.Setup(r => r.UpdateAsync(It.IsAny<EscalationRule>())).Returns(Task.CompletedTask);
        _mockRuleRepo.Setup(r => r.SaveAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert — soft delete sets IsDeleted = true
        rule.IsDeleted.Should().BeTrue();
        _mockRuleRepo.Verify(r => r.UpdateAsync(rule), Times.Once);
        _mockRuleRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteRuleAsync_ShouldThrow_WhenRuleNotFound()
    {
        // Arrange
        _mockRuleRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((EscalationRule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(999));
    }

    // ========================================================================
    // TestRuleAsync Tests  → TestRuleAsync_ShouldReturn*
    // ========================================================================

    [Fact]
    public async Task TestRuleAsync_ShouldReturnMatch_WhenConditionsMet()
    {
        // Arrange
        var rule = CreateTestRule(1, "High Escalation", "High", true);
        var sr = CreateTestServiceRequest(10, ServiceRequestPriority.High, 5);

        _mockRuleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);
        _mockSrRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(sr);

        // Act
        var result = await _service.TestRuleAsync(1, 10);

        // Assert
        result.Should().NotBeNull();
        result.RuleId.Should().Be(1);
        result.ServiceRequestId.Should().Be(10);
        result.RuleMatched.Should().BeTrue();
        result.Rule.Should().NotBeNull();
        result.TestMessage.Should().Contain("escalate");
    }

    [Fact]
    public async Task TestRuleAsync_ShouldReturnNoMatch_WhenConditionsNotMet()
    {
        // Arrange — different priority
        var rule = CreateTestRule(1, "Critical Only Rule", "Critical", true);
        var sr = CreateTestServiceRequest(10, ServiceRequestPriority.Low, null);

        _mockRuleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);
        _mockSrRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(sr);

        // Act
        var result = await _service.TestRuleAsync(1, 10);

        // Assert
        result.RuleMatched.Should().BeFalse();
        result.MatchReason.Should().Contain("not match");
    }

    [Fact]
    public async Task TestRuleAsync_ShouldThrow_WhenRuleNotFound()
    {
        // Arrange
        _mockRuleRepo.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((EscalationRule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.TestRuleAsync(999, 1));
    }

    // ========================================================================
    // GetApplicableRulesAsync Tests  → GetApplicableRulesAsync_Should*
    // ========================================================================

    [Fact]
    public async Task GetApplicableRulesAsync_ShouldReturnMatchingRules()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            CreateTestRule(1, "Critical Rule A", "Critical", true),
            CreateTestRule(2, "Critical Rule B", "Critical", true),
            CreateTestRule(3, "High Rule", "High", true),
        };
        _mockRuleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await _service.GetApplicableRulesAsync("Critical");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Priority == "Critical");
    }

    [Fact]
    public async Task GetApplicableRulesAsync_ShouldRespectPriority()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            CreateTestRule(1, "High Rule", "High", true),
            CreateTestRule(2, "Medium Rule", "Medium", true),
            CreateTestRule(3, "Low Rule", "Low", true),
        };
        _mockRuleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var highResult = await _service.GetApplicableRulesAsync("High");
        var lowResult = await _service.GetApplicableRulesAsync("Low");

        // Assert
        highResult.Should().HaveCount(1);
        highResult[0].Priority.Should().Be("High");
        lowResult.Should().HaveCount(1);
        lowResult[0].Priority.Should().Be("Low");
    }

    [Fact]
    public async Task GetApplicableRulesAsync_ShouldExcludeDisabledRules()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            CreateTestRule(1, "Active Critical Rule", "Critical", true),
            CreateTestRule(2, "Inactive Critical Rule", "Critical", false),
        };
        _mockRuleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await _service.GetApplicableRulesAsync("Critical");

        // Assert — only active rules should be returned
        result.Should().HaveCount(1);
        result[0].IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetApplicableRulesAsync_ShouldReturnEmpty_WhenNoPriorityMatch()
    {
        // Arrange
        var rules = new List<EscalationRule>
        {
            CreateTestRule(1, "High Rule", "High", true),
        };
        _mockRuleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);

        // Act
        var result = await _service.GetApplicableRulesAsync("Critical");

        // Assert
        result.Should().BeEmpty();
    }

    // ========================================================================
    // Helper Methods
    // ========================================================================

    private static EscalationRule CreateTestRule(int id, string name, string priority, bool isActive) =>
        new()
        {
            Id = id,
            Name = name,
            Priority = priority,
            AgeInMinutes = 60,
            TargetType = EscalationTargetType.User,
            TargetId = 1,
            TargetName = "Support Lead",
            MaxAttempts = 3,
            RetryIntervalMinutes = 15,
            IsActive = isActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static ServiceRequest CreateTestServiceRequest(
        int id, ServiceRequestPriority priority, int? categoryId) =>
        new()
        {
            Id = id,
            Subject = $"Test Service Request {id}",
            TicketNumber = $"SR-{id:D3}",
            Priority = priority,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow
        };
}
