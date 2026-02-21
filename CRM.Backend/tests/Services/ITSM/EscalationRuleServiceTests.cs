// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.KnowledgeBase;
using FluentAssertions;
using Xunit;
using EscalationType = CRM.Core.Entities.KnowledgeBase.EscalationType;
using SLAMetricType = CRM.Core.Entities.KnowledgeBase.SLAMetricType;
using SLAPriority = CRM.Core.Entities.KnowledgeBase.SLAPriority;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for EscalationRule DTOs and enums.
/// </summary>
public class EscalationRuleServiceTests
{
    [Fact]
    public void EscalationRuleDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new EscalationRuleDto();

        // Assert
        dto.Id.Should().Be(0);
        dto.SLAPolicyId.Should().Be(0);
        dto.SLAPolicyName.Should().BeNull();
        dto.Name.Should().BeEmpty();
        dto.TriggerAtPercent.Should().Be(0);
        dto.IsActive.Should().BeFalse();
        dto.ExecutionOrder.Should().Be(0);
        dto.EmailRecipients.Should().BeNull();
        dto.EmailTemplateId.Should().BeNull();
        dto.ReassignToUserId.Should().BeNull();
        dto.ReassignToUserName.Should().BeNull();
        dto.ReassignToTeamId.Should().BeNull();
        dto.ReassignToTeamName.Should().BeNull();
        dto.NewPriority.Should().BeNull();
        dto.WebhookUrl.Should().BeNull();
        dto.ActionConfig.Should().BeNull();
    }

    [Fact]
    public void EscalationRuleDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var now = DateTime.UtcNow;
        var dto = new EscalationRuleDto
        {
            Id = 1,
            SLAPolicyId = 10,
            SLAPolicyName = "Critical SLA",
            Name = "P1 Response Escalation",
            TriggerAtPercent = 80,
            TriggerMetric = SLAMetricType.FirstResponse,
            IsActive = true,
            ExecutionOrder = 1,
            EscalationType = EscalationType.Email,
            EmailRecipients = new List<string> { "admin@example.com", "manager@example.com" },
            EmailTemplateId = 5,
            ReassignToUserId = 10,
            ReassignToUserName = "Support Lead",
            ReassignToTeamId = 3,
            ReassignToTeamName = "IT Support",
            NewPriority = SLAPriority.Critical,
            WebhookUrl = "https://hooks.example.com/escalation",
            ActionConfig = new Dictionary<string, object> { { "notify_slack", true } },
            CreatedAt = now.AddDays(-10),
            UpdatedAt = now
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.SLAPolicyId.Should().Be(10);
        dto.SLAPolicyName.Should().Be("Critical SLA");
        dto.Name.Should().Be("P1 Response Escalation");
        dto.TriggerAtPercent.Should().Be(80);
        dto.TriggerMetric.Should().Be(SLAMetricType.FirstResponse);
        dto.IsActive.Should().BeTrue();
        dto.ExecutionOrder.Should().Be(1);
        dto.EscalationType.Should().Be(EscalationType.Email);
        dto.EmailRecipients.Should().HaveCount(2);
        dto.EmailTemplateId.Should().Be(5);
        dto.ReassignToUserId.Should().Be(10);
        dto.ReassignToTeamId.Should().Be(3);
        dto.NewPriority.Should().Be(SLAPriority.Critical);
        dto.WebhookUrl.Should().Be("https://hooks.example.com/escalation");
        dto.ActionConfig.Should().ContainKey("notify_slack");
    }

    [Fact]
    public void CreateEscalationRuleDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CreateEscalationRuleDto();

        // Assert
        dto.SLAPolicyId.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.TriggerAtPercent.Should().Be(100); // Default per class definition
        dto.IsActive.Should().BeTrue(); // Default per class definition
        dto.ExecutionOrder.Should().Be(0); // Default per class definition
        dto.EmailRecipients.Should().BeNull();
        dto.EmailTemplateId.Should().BeNull();
        dto.ReassignToUserId.Should().BeNull();
        dto.ReassignToTeamId.Should().BeNull();
        dto.NewPriority.Should().BeNull();
        dto.WebhookUrl.Should().BeNull();
        dto.ActionConfig.Should().BeNull();
    }

    [Fact]
    public void CreateEscalationRuleDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var dto = new CreateEscalationRuleDto
        {
            SLAPolicyId = 5,
            Name = "Resolution Warning",
            TriggerAtPercent = 75,
            TriggerMetric = SLAMetricType.Resolution,
            IsActive = true,
            ExecutionOrder = 2,
            EscalationType = EscalationType.ReassignUser,
            EmailRecipients = new List<string> { "team@example.com" },
            EmailTemplateId = 3,
            ReassignToUserId = 15,
            ReassignToTeamId = 4,
            NewPriority = SLAPriority.High,
            WebhookUrl = "https://api.example.com/notify",
            ActionConfig = new Dictionary<string, object> { { "createTask", true } }
        };

        // Assert
        dto.SLAPolicyId.Should().Be(5);
        dto.Name.Should().Be("Resolution Warning");
        dto.TriggerAtPercent.Should().Be(75);
        dto.TriggerMetric.Should().Be(SLAMetricType.Resolution);
        dto.IsActive.Should().BeTrue();
        dto.ExecutionOrder.Should().Be(2);
        dto.EscalationType.Should().Be(EscalationType.ReassignUser);
        dto.EmailRecipients.Should().HaveCount(1);
        dto.ReassignToUserId.Should().Be(15);
        dto.NewPriority.Should().Be(SLAPriority.High);
    }

    [Fact]
    public void UpdateEscalationRuleDto_ShouldBeAllNullable()
    {
        // Arrange & Act
        var dto = new UpdateEscalationRuleDto();

        // Assert - all properties should be null for partial updates
        dto.Name.Should().BeNull();
        dto.TriggerAtPercent.Should().BeNull();
        dto.TriggerMetric.Should().BeNull();
        dto.IsActive.Should().BeNull();
        dto.ExecutionOrder.Should().BeNull();
        dto.EscalationType.Should().BeNull();
        dto.EmailRecipients.Should().BeNull();
        dto.EmailTemplateId.Should().BeNull();
        dto.ReassignToUserId.Should().BeNull();
        dto.ReassignToTeamId.Should().BeNull();
        dto.NewPriority.Should().BeNull();
        dto.WebhookUrl.Should().BeNull();
        dto.ActionConfig.Should().BeNull();
    }

    [Fact]
    public void EscalationRuleFilterDto_ShouldHaveDefaultPagination()
    {
        // Arrange & Act
        var dto = new EscalationRuleFilterDto();

        // Assert
        dto.SLAPolicyId.Should().BeNull();
        dto.IsActive.Should().BeNull();
        dto.EscalationType.Should().BeNull();
        dto.SearchTerm.Should().BeNull();
        dto.PageNumber.Should().Be(1);
        dto.PageSize.Should().Be(20);
    }

    [Fact]
    public void SLAMetricType_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)SLAMetricType.FirstResponse).Should().Be(0);
        ((int)SLAMetricType.Resolution).Should().Be(1);
        ((int)SLAMetricType.NextResponse).Should().Be(2);
        ((int)SLAMetricType.Assignment).Should().Be(3);
    }

    [Fact]
    public void EscalationType_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)EscalationType.Email).Should().Be(0);
        ((int)EscalationType.ReassignUser).Should().Be(1);
        ((int)EscalationType.ReassignTeam).Should().Be(2);
        ((int)EscalationType.IncreasePriority).Should().Be(3);
        ((int)EscalationType.Webhook).Should().Be(4);
    }

    [Fact]
    public void SLAPriority_ShouldHaveExpectedValues()
    {
        // Arrange & Act & Assert
        ((int)SLAPriority.Critical).Should().Be(0);
        ((int)SLAPriority.High).Should().Be(1);
        ((int)SLAPriority.Medium).Should().Be(2);
        ((int)SLAPriority.Low).Should().Be(3);
    }
}
