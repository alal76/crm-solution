// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.Notifications;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using EscalationRule = CRM.Core.Entities.ITSM.EscalationRule;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Verifies that EscalationRuleService (the SLA-focused escalation evaluator, REM-ORPHAN-005)
/// pings Slack/Teams via ISlackNotificationService/ITeamsNotificationService whenever an
/// escalation rule actually executes, in addition to whatever channel-specific action
/// (reassignment, priority bump, etc.) the rule already performs.
/// </summary>
public class EscalationRuleServiceNotificationTests
{
    private static CrmDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        return new CrmDbContext(options, configuration);
    }

    private static EscalationRuleService CreateService(
        CrmDbContext context,
        out Mock<ISlackNotificationService> slackMock,
        out Mock<ITeamsNotificationService> teamsMock)
    {
        slackMock = new Mock<ISlackNotificationService>();
        slackMock
            .Setup(s => s.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<SlackEscalationInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        teamsMock = new Mock<ITeamsNotificationService>();
        teamsMock
            .Setup(t => t.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<TeamsEscalationInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return new EscalationRuleService(
            context,
            Mock.Of<ILogger<EscalationRuleService>>(),
            slackMock.Object,
            teamsMock.Object);
    }

    [Fact]
    public async Task EvaluateRulesAsync_ShouldNotifySlackAndTeams_WhenRuleExecutes()
    {
        // Arrange
        using var context = CreateContext();

        var serviceRequest = new ServiceRequest
        {
            TicketNumber = "SR-100",
            Subject = "Payment gateway down",
            Priority = ServiceRequestPriority.Critical,
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };
        context.ServiceRequests.Add(serviceRequest);

        var rule = new EscalationRule
        {
            Name = "Critical Auto-Escalate",
            Priority = string.Empty, // matches any service request priority
            Category = null,
            Queue = null,
            AgeInMinutes = 0, // trigger immediately regardless of actual age
            TargetType = EscalationTargetType.User,
            TargetId = 42,
            TargetName = "On-call Engineer",
            MaxAttempts = 3,
            RetryIntervalMinutes = 15,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Set<EscalationRule>().Add(rule);

        await context.SaveChangesAsync();

        var service = CreateService(context, out var slackMock, out var teamsMock);

        // Act
        var result = await service.EvaluateRulesAsync(serviceRequest.Id);

        // Assert
        result.Should().BeTrue();

        slackMock.Verify(
            s => s.SendEscalationAlertAsync(
                It.IsAny<string>(),
                It.Is<SlackEscalationInfo>(i =>
                    i.ServiceRequestNumber == "SR-100" &&
                    i.Title == "Payment gateway down" &&
                    i.AssignedTo == "On-call Engineer"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        teamsMock.Verify(
            t => t.SendEscalationAlertAsync(
                It.IsAny<string>(),
                It.Is<TeamsEscalationInfo>(i =>
                    i.ServiceRequestNumber == "SR-100" &&
                    i.Title == "Payment gateway down" &&
                    i.AssignedTo == "On-call Engineer"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EvaluateRulesAsync_ShouldNotNotify_WhenNoRuleMatches()
    {
        // Arrange
        using var context = CreateContext();

        var serviceRequest = new ServiceRequest
        {
            TicketNumber = "SR-200",
            Subject = "Minor UI glitch",
            Priority = ServiceRequestPriority.Low,
            CreatedAt = DateTime.UtcNow
        };
        context.ServiceRequests.Add(serviceRequest);
        await context.SaveChangesAsync();

        var service = CreateService(context, out var slackMock, out var teamsMock);

        // Act
        var result = await service.EvaluateRulesAsync(serviceRequest.Id);

        // Assert — no applicable rules exist, so evaluation returns false and no chat calls occur
        result.Should().BeFalse();

        slackMock.Verify(
            s => s.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<SlackEscalationInfo>(), It.IsAny<CancellationToken>()),
            Times.Never);

        teamsMock.Verify(
            t => t.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<TeamsEscalationInfo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EvaluateRulesAsync_ShouldNotThrow_WhenChatNotificationFails()
    {
        // Arrange — Slack/Teams throwing must not break the escalation flow that already saved.
        using var context = CreateContext();

        var serviceRequest = new ServiceRequest
        {
            TicketNumber = "SR-300",
            Subject = "Database replication lag",
            Priority = ServiceRequestPriority.High,
            CreatedAt = DateTime.UtcNow.AddHours(-3)
        };
        context.ServiceRequests.Add(serviceRequest);

        var rule = new EscalationRule
        {
            Name = "High Auto-Escalate",
            Priority = string.Empty,
            AgeInMinutes = 0,
            TargetType = EscalationTargetType.User,
            TargetId = 7,
            TargetName = "Support Lead",
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Set<EscalationRule>().Add(rule);
        await context.SaveChangesAsync();

        var slackMock = new Mock<ISlackNotificationService>();
        slackMock
            .Setup(s => s.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<SlackEscalationInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Slack webhook unreachable"));

        var teamsMock = new Mock<ITeamsNotificationService>();
        teamsMock
            .Setup(t => t.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<TeamsEscalationInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new EscalationRuleService(
            context,
            Mock.Of<ILogger<EscalationRuleService>>(),
            slackMock.Object,
            teamsMock.Object);

        // Act
        var act = async () => await service.EvaluateRulesAsync(serviceRequest.Id);

        // Assert — the whole evaluation still completes successfully despite Slack throwing
        await act.Should().NotThrowAsync();

        teamsMock.Verify(
            t => t.SendEscalationAlertAsync(
                It.IsAny<string>(), It.IsAny<TeamsEscalationInfo>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
