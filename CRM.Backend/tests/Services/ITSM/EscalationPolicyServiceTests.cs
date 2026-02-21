// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Unit tests for EscalationPolicyService DTOs.
/// Tests the DTOs used for escalation policy management.
/// </summary>
public class EscalationPolicyServiceTests
{
    #region EscalationPolicyDto Tests

    [Fact]
    public void EscalationPolicyDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new EscalationPolicyDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.IsActive.Should().BeFalse();
        dto.IsDefault.Should().BeFalse();
        dto.CategoryId.Should().BeNull();
        dto.CategoryName.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.Levels.Should().NotBeNull();
        dto.Levels.Should().BeEmpty();
    }

    [Fact]
    public void EscalationPolicyDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var dto = new EscalationPolicyDto
        {
            Id = 1,
            Name = "Critical Issue Escalation",
            Description = "Auto-escalate critical issues",
            IsActive = true,
            IsDefault = true,
            CategoryId = 5,
            CategoryName = "Hardware",
            Priority = 1,
            Levels = new List<EscalationLevelDto>
            {
                new() { Id = 1, PolicyId = 1, LevelNumber = 1, Name = "Level 1" }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Critical Issue Escalation");
        dto.IsActive.Should().BeTrue();
        dto.IsDefault.Should().BeTrue();
        dto.Levels.Should().HaveCount(1);
    }

    #endregion

    #region EscalationLevelDto Tests

    [Fact]
    public void EscalationLevelDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new EscalationLevelDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.PolicyId.Should().Be(0);
        dto.LevelNumber.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.EscalateAfterMinutes.Should().Be(0);
        dto.NotifyUserId.Should().BeNull();
        dto.NotifyTeamId.Should().BeNull();
        dto.SendEmail.Should().BeFalse();
        dto.SendSms.Should().BeFalse();
    }

    [Fact]
    public void EscalationLevelDto_CanBeFullyPopulated()
    {
        // Arrange & Act
        var dto = new EscalationLevelDto
        {
            Id = 1,
            PolicyId = 1,
            LevelNumber = 1,
            Name = "Manager Escalation",
            EscalateAfterMinutes = 30,
            NotifyUserId = 10,
            NotifyUserName = "John Manager",
            NotifyTeamId = 5,
            NotifyTeamName = "Support Team",
            SendEmail = true,
            SendSms = true,
            EmailTemplateId = 3
        };

        // Assert
        dto.LevelNumber.Should().Be(1);
        dto.Name.Should().Be("Manager Escalation");
        dto.EscalateAfterMinutes.Should().Be(30);
        dto.SendEmail.Should().BeTrue();
        dto.SendSms.Should().BeTrue();
    }

    #endregion

    #region CreateEscalationPolicyDto Tests

    [Fact]
    public void CreateEscalationPolicyDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new CreateEscalationPolicyDto();

        // Assert
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.IsActive.Should().BeTrue(); // Default is true
        dto.IsDefault.Should().BeFalse(); // Default is false
        dto.CategoryId.Should().BeNull();
        dto.Priority.Should().BeNull();
        dto.Levels.Should().BeNull();
    }

    [Fact]
    public void CreateEscalationPolicyDto_CanBePopulated_WithLevels()
    {
        // Arrange & Act
        var dto = new CreateEscalationPolicyDto
        {
            Name = "Test Policy",
            Description = "Test Description",
            IsActive = true,
            IsDefault = false,
            Levels = new List<CreateEscalationLevelDto>
            {
                new()
                {
                    LevelNumber = 1,
                    Name = "Level 1",
                    EscalateAfterMinutes = 15,
                    SendEmail = true,
                    SendSms = false
                }
            }
        };

        // Assert
        dto.Name.Should().Be("Test Policy");
        dto.Levels.Should().HaveCount(1);
        dto.Levels![0].EscalateAfterMinutes.Should().Be(15);
    }

    #endregion

    #region CreateEscalationLevelDto Tests

    [Fact]
    public void CreateEscalationLevelDto_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new CreateEscalationLevelDto();

        // Assert
        dto.LevelNumber.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.EscalateAfterMinutes.Should().Be(0);
        dto.NotifyUserId.Should().BeNull();
        dto.NotifyTeamId.Should().BeNull();
        dto.SendEmail.Should().BeTrue(); // Default is true
        dto.SendSms.Should().BeFalse(); // Default is false
    }

    #endregion

    #region UpdateEscalationPolicyDto Tests

    [Fact]
    public void UpdateEscalationPolicyDto_AllPropertiesNullable()
    {
        // Arrange & Act
        var dto = new UpdateEscalationPolicyDto();

        // Assert - all properties should be null for partial updates
        dto.Name.Should().BeNull();
        dto.Description.Should().BeNull();
        dto.IsActive.Should().BeNull();
        dto.IsDefault.Should().BeNull();
        dto.CategoryId.Should().BeNull();
        dto.Priority.Should().BeNull();
    }

    [Fact]
    public void UpdateEscalationPolicyDto_SupportPartialUpdates()
    {
        // Arrange & Act - only update name
        var dto = new UpdateEscalationPolicyDto
        {
            Name = "Updated Name"
        };

        // Assert
        dto.Name.Should().Be("Updated Name");
        dto.IsActive.Should().BeNull(); // Not provided, null
    }

    #endregion
}
