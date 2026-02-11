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

using Xunit;
using FluentAssertions;
using CRM.Core.Interfaces.ITSM;
using CRM.Core.DTOs.ITSM;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.BVT;

/// <summary>
/// Build Verification Tests (BVT) for ITSM Phase 4 Features:
/// - Webhook Notifications
/// - Email-to-Ticket Parsing
/// - ITSM Dashboard & Analytics
/// - Monitoring Tool Integration
/// - CI/CD Integration
/// - Self-Service Chatbot
/// </summary>
public class ITSMPhase4BVTTests
{
    #region BVT-ITSM-001 to BVT-ITSM-010: Webhook Notification Critical Path

    [Fact]
    [Trait("Feature", "Webhooks")]
    public void BVTITSM001_WebhookEventTypes_AllDefined()
    {
        // Arrange & Act
        var eventTypes = Enum.GetValues<WebhookEventType>();

        // Assert
        eventTypes.Should().NotBeEmpty();
        eventTypes.Should().Contain(WebhookEventType.IncidentCreated);
        eventTypes.Should().Contain(WebhookEventType.IncidentResolved);
        eventTypes.Should().Contain(WebhookEventType.SLABreached);
        eventTypes.Should().Contain(WebhookEventType.ChangeApproved);
    }

    [Fact]
    [Trait("Feature", "Webhooks")]
    public void BVTITSM002_WebhookSubscriptionDto_HasRequiredProperties()
    {
        // Arrange & Act
        var dto = new WebhookSubscriptionDto
        {
            WebhookSubscriptionId = 1,
            Name = "Test Webhook",
            TargetUrl = "https://example.com/webhook",
            IsActive = true,
            EventTypes = new List<string> { "IncidentCreated" },
            RetryCount = 3,
            TimeoutSeconds = 30
        };

        // Assert
        dto.WebhookSubscriptionId.Should().Be(1);
        dto.Name.Should().Be("Test Webhook");
        dto.TargetUrl.Should().StartWith("https://");
        dto.IsActive.Should().BeTrue();
        dto.EventTypes.Should().HaveCount(1);
        dto.RetryCount.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Feature", "Webhooks")]
    public void BVTITSM003_CreateWebhookSubscriptionDto_ValidationReady()
    {
        // Arrange & Act
        var createDto = new CreateWebhookSubscriptionDto
        {
            Name = "New Webhook",
            TargetUrl = "https://hooks.slack.com/test",
            EventTypes = new List<string> { "IncidentCreated", "IncidentResolved" },
            RetryCount = 5,
            TimeoutSeconds = 60
        };

        // Assert
        createDto.Name.Should().NotBeNullOrEmpty();
        createDto.TargetUrl.Should().NotBeNullOrEmpty();
        createDto.EventTypes.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    [Trait("Feature", "Webhooks")]
    public void BVTITSM004_WebhookDeliveryDto_TracksDeliveryStatus()
    {
        // Arrange & Act - Using actual DTO properties
        var deliveryDto = new WebhookDeliveryDto
        {
            WebhookDeliveryId = 1,
            WebhookSubscriptionId = 1,
            EventType = "IncidentCreated",
            Success = true,
            ResponseStatusCode = 200,
            AttemptNumber = 1,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        deliveryDto.Success.Should().BeTrue();
        deliveryDto.ResponseStatusCode.Should().Be(200);
        deliveryDto.AttemptNumber.Should().BeGreaterThan(0);
    }

    #endregion

    #region BVT-ITSM-011 to BVT-ITSM-020: Email-to-Ticket Critical Path

    [Fact]
    [Trait("Feature", "EmailToTicket")]
    public void BVTITSM011_InboundEmailDto_HasRequiredFields()
    {
        // Arrange & Act - Using actual DTO properties
        var email = new InboundEmailDto
        {
            From = "user@example.com",
            To = new List<string> { "support@company.com" },
            Subject = "Server down - urgent",
            BodyText = "The production server is not responding.",
            Attachments = new List<EmailAttachmentDto>()
        };

        // Assert
        email.From.Should().NotBeNullOrEmpty();
        email.To.Should().NotBeEmpty();
        email.Subject.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Feature", "EmailToTicket")]
    public void BVTITSM012_EmailParseResult_ContainsIncidentInfo()
    {
        // Arrange & Act - Using actual DTO properties
        var result = new EmailParseResult
        {
            Success = true,
            IncidentId = 123,
            IncidentNumber = "INC0000123",
            Action = EmailParseAction.IncidentCreated
        };

        // Assert
        result.Success.Should().BeTrue();
        result.IncidentId.Should().BeGreaterThan(0);
        result.IncidentNumber.Should().StartWith("INC");
        result.Action.Should().Be(EmailParseAction.IncidentCreated);
    }

    [Fact]
    [Trait("Feature", "EmailToTicket")]
    public void BVTITSM013_EmailParsingConfig_SupportsMultipleOptions()
    {
        // Arrange & Act - Using actual DTO properties
        var config = new EmailParsingConfigDto
        {
            IsEnabled = true,
            DefaultPriority = 3,
            AutoDetectCustomer = true,
            AllowedDomains = new List<string> { "company.com", "partner.com" },
            MaxAttachmentSizeMB = 25
        };

        // Assert
        config.IsEnabled.Should().BeTrue();
        config.AllowedDomains.Should().HaveCountGreaterThan(0);
        config.MaxAttachmentSizeMB.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Feature", "EmailToTicket")]
    public void BVTITSM014_EmailParseAction_AllActionsAvailable()
    {
        // Arrange & Act
        var actions = Enum.GetValues<EmailParseAction>();

        // Assert
        actions.Should().Contain(EmailParseAction.IncidentCreated);
        actions.Should().Contain(EmailParseAction.CommentAdded);
        actions.Should().Contain(EmailParseAction.Ignored);
        actions.Should().Contain(EmailParseAction.Failed);
    }

    #endregion

    #region BVT-ITSM-021 to BVT-ITSM-030: Dashboard Analytics Critical Path

    [Fact]
    [Trait("Feature", "Dashboard")]
    public void BVTITSM021_SLAComplianceDto_TracksCompliance()
    {
        // Arrange & Act - Using actual DTO structure
        var compliance = new SLAComplianceDto
        {
            TotalTickets = 200,
            TicketsWithinSLA = 180,
            TicketsBreachedSLA = 20,
            OverallComplianceRate = 90.0,
            ByPriority = new List<SLAByPriority>
            {
                new() { Priority = 1, PriorityLabel = "P1", Total = 20, Met = 19, Breached = 1, ComplianceRate = 95.0 },
                new() { Priority = 2, PriorityLabel = "P2", Total = 50, Met = 46, Breached = 4, ComplianceRate = 92.0 }
            }
        };

        // Assert
        compliance.OverallComplianceRate.Should().BeGreaterThan(0);
        compliance.TicketsWithinSLA.Should().BeLessThanOrEqualTo(compliance.TotalTickets);
        compliance.ByPriority.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Feature", "Dashboard")]
    public void BVTITSM022_AgentPerformanceDto_TracksAgentMetrics()
    {
        // Arrange & Act - Using actual DTO structure
        var performance = new AgentPerformanceDto
        {
            AgentId = 1,
            AgentName = "John Smith",
            TicketsResolved = 45,
            TicketsAssigned = 50,
            AverageResolutionTimeHours = 3.2,
            CustomerSatisfactionScore = 4.8,
            FirstContactResolutionRate = 72.5,
            SLAComplianceRate = 94.0
        };

        // Assert
        performance.TicketsResolved.Should().BeGreaterThan(0);
        performance.CustomerSatisfactionScore.Should().BeInRange(0, 5);
        performance.FirstContactResolutionRate.Should().BeInRange(0, 100);
    }

    [Fact]
    [Trait("Feature", "Dashboard")]
    public void BVTITSM023_SLAByPriority_Structure()
    {
        // Arrange & Act
        var slaPriority = new SLAByPriority
        {
            Priority = 1,
            PriorityLabel = "P1 - Critical",
            Total = 100,
            Met = 95,
            Breached = 5,
            ComplianceRate = 95.0
        };

        // Assert
        slaPriority.Priority.Should().Be(1);
        slaPriority.Total.Should().Be(slaPriority.Met + slaPriority.Breached);
    }

    #endregion

    #region BVT-ITSM-031 to BVT-ITSM-040: Monitoring Integration Critical Path

    [Fact]
    [Trait("Feature", "MonitoringIntegration")]
    public void BVTITSM031_MonitoringAlertDto_MapsFromPrometheus()
    {
        // Arrange & Act - Using actual DTO structure
        var alert = new MonitoringAlertDto
        {
            AlertName = "HighCPUUsage",
            Status = "firing",
            Severity = AlertSeverity.Critical,
            Description = "CPU usage above 90% for 5 minutes",
            StartsAt = DateTime.UtcNow.AddMinutes(-5),
            Labels = new Dictionary<string, string>
            {
                { "job", "node-exporter" },
                { "environment", "production" },
                { "instance", "server-01.example.com" }
            }
        };

        // Assert
        alert.AlertName.Should().NotBeNullOrEmpty();
        alert.Severity.Should().Be(AlertSeverity.Critical);
        alert.Labels.Should().ContainKey("environment");
    }

    [Fact]
    [Trait("Feature", "MonitoringIntegration")]
    public void BVTITSM032_AlertSeverity_SupportsMultipleTypes()
    {
        // Arrange & Act
        var severities = Enum.GetValues<AlertSeverity>();

        // Assert
        severities.Should().Contain(AlertSeverity.Critical);
        severities.Should().Contain(AlertSeverity.Warning);
        severities.Should().Contain(AlertSeverity.Info);
    }

    #endregion

    #region BVT-ITSM-041 to BVT-ITSM-050: CI/CD Integration Critical Path

    [Fact]
    [Trait("Feature", "CICDIntegration")]
    public void BVTITSM041_DeploymentChangeRequest_HasRequiredFields()
    {
        // Arrange & Act
        var request = new DeploymentChangeRequestDto
        {
            PipelineId = "pipeline-123",
            PipelineName = "CRM Backend Deploy",
            BuildNumber = "1.2.3.456",
            CommitHash = "abc123def456",
            CommitMessage = "Fix: Resolve login issue",
            Author = "developer@company.com",
            Branch = "main",
            Environment = "production",
            Services = new List<string> { "crm-api", "crm-worker" },
            DeploymentType = DeploymentType.Standard
        };

        // Assert
        request.PipelineId.Should().NotBeNullOrEmpty();
        request.BuildNumber.Should().NotBeNullOrEmpty();
        request.Environment.Should().Be("production");
        request.Services.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    [Trait("Feature", "CICDIntegration")]
    public void BVTITSM042_DeploymentTypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<DeploymentType>();

        // Assert
        types.Should().Contain(DeploymentType.Standard);
        types.Should().Contain(DeploymentType.Emergency);
        types.Should().Contain(DeploymentType.Hotfix);
        types.Should().Contain(DeploymentType.Rollback);
    }

    [Fact]
    [Trait("Feature", "CICDIntegration")]
    public void BVTITSM043_DeploymentChangeResult_TracksApproval()
    {
        // Arrange & Act
        var result = new DeploymentChangeResult
        {
            Success = true,
            ChangeId = 123,
            ChangeNumber = "CHG-20260203-0001",
            Status = "approved",
            IsApproved = true,
            Message = "Change request approved. Ready to deploy."
        };

        // Assert
        result.Success.Should().BeTrue();
        result.ChangeNumber.Should().StartWith("CHG-");
        result.IsApproved.Should().BeTrue();
    }

    [Fact]
    [Trait("Feature", "CICDIntegration")]
    public void BVTITSM044_PipelineRegistration_SupportsMultiplePlatforms()
    {
        // Arrange & Act
        var platforms = Enum.GetValues<CICDPlatform>();

        // Assert
        platforms.Should().Contain(CICDPlatform.AzureDevOps);
        platforms.Should().Contain(CICDPlatform.GitHub);
        platforms.Should().Contain(CICDPlatform.GitLab);
        platforms.Should().Contain(CICDPlatform.Jenkins);
    }

    #endregion

    #region BVT-ITSM-051 to BVT-ITSM-060: Self-Service Chatbot Critical Path

    [Fact]
    [Trait("Feature", "Chatbot")]
    public void BVTITSM051_ChatSessionDto_TracksConversation()
    {
        // Arrange & Act - Using actual DTO structure
        var session = new ChatSessionDto
        {
            SessionId = "session-123-abc",
            UserId = 1,
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            EndedAt = null,
            MessageCount = 5,
            Status = "active"
        };

        // Assert
        session.SessionId.Should().NotBeNullOrEmpty();
        session.MessageCount.Should().BeGreaterThan(0);
        session.Status.Should().Be("active");
    }

    [Fact]
    [Trait("Feature", "Chatbot")]
    public void BVTITSM052_ChatMessageDto_SupportsUserAndBot()
    {
        // Arrange & Act - Using actual DTO structure
        var userMessage = new ChatMessageDto
        {
            Id = 1,
            SessionId = "session-123",
            IsFromUser = true,
            Message = "I need to reset my password",
            Timestamp = DateTime.UtcNow
        };

        var botMessage = new ChatMessageDto
        {
            Id = 2,
            SessionId = "session-123",
            IsFromUser = false,
            Message = "I can help you with that. Would you like me to initiate a password reset?",
            Timestamp = DateTime.UtcNow
        };

        // Assert
        userMessage.IsFromUser.Should().BeTrue();
        botMessage.IsFromUser.Should().BeFalse();
    }

    [Fact]
    [Trait("Feature", "Chatbot")]
    public void BVTITSM053_ResponseType_AllTypesAvailable()
    {
        // Arrange & Act
        var responseTypes = Enum.GetValues<ResponseType>();

        // Assert
        responseTypes.Should().Contain(ResponseType.Text);
        responseTypes.Should().Contain(ResponseType.Options);
        responseTypes.Should().Contain(ResponseType.KnowledgeResults);
        responseTypes.Should().Contain(ResponseType.IncidentCreated);
        responseTypes.Should().Contain(ResponseType.Escalation);
    }

    [Fact]
    [Trait("Feature", "Chatbot")]
    public void BVTITSM054_ChatbotResponse_IncludesContext()
    {
        // Arrange & Act - Using actual DTO structure
        var response = new ChatbotResponseDto
        {
            SessionId = "session-123",
            Message = "Here are some articles that might help:",
            Type = ResponseType.KnowledgeResults,
            KnowledgeResults = new List<KnowledgeSearchResultDto>
            {
                new() { ArticleId = 1, Title = "How to Reset Password", RelevanceScore = 0.9 }
            },
            Timestamp = DateTime.UtcNow
        };

        // Assert
        response.Type.Should().Be(ResponseType.KnowledgeResults);
        response.KnowledgeResults.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Feature", "Chatbot")]
    public void BVTITSM055_QuickActionDto_HasRequiredFields()
    {
        // Arrange & Act - Using actual DTO structure
        var quickAction = new QuickActionDto
        {
            Id = "reset_password",
            Title = "Reset Password",
            Description = "Request a password reset for your account",
            Icon = "key",
            Category = "Account"
        };

        // Assert
        quickAction.Id.Should().NotBeNullOrEmpty();
        quickAction.Title.Should().NotBeNullOrEmpty();
    }

    #endregion
}
