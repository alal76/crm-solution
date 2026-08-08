// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for <see cref="CampaignExecutionService"/> service-layer methods.
/// Tests focus on the CRUD / workflow-linking operations that touch only
/// the database (no workflow-engine execution paths).
/// </summary>
public class CampaignExecutionServiceNewTests
{
    // Helper: build a shared InMemory CrmDbContext + the transitive services
    private static (CrmDbContext context, CampaignExecutionService service) BuildSut(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(dbName ?? $"CampaignExec_{Guid.NewGuid()}")
            .Options;
        var context = new CrmDbContext(options, null!);

        var workflowService = new WorkflowService(context, Mock.Of<ILogger<WorkflowService>>());
        var workflowInstanceService = new WorkflowInstanceService(
            context,
            Mock.Of<ILogger<WorkflowInstanceService>>(),
            workflowService,
            Mock.Of<CRM.Core.Interfaces.IHttpCalloutService>());

        var service = new CampaignExecutionService(
            context,
            workflowService,
            workflowInstanceService,
            Mock.Of<ILogger<CampaignExecutionService>>());

        return (context, service);
    }

    // Helper: build a SUT with a mockable INotificationPort + ICampaignExecutionJobScheduler
    // so ExecuteAsync/StartCampaignAsync send/enqueue behavior can be asserted (REV-STUB-011).
    private static (
        CrmDbContext Context,
        CampaignExecutionService Service,
        Mock<INotificationPort> NotificationPort,
        Mock<ICampaignExecutionJobScheduler> JobScheduler) BuildSutWithNotifications(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(dbName ?? $"CampaignExecNotify_{Guid.NewGuid()}")
            .Options;
        var context = new CrmDbContext(options, null!);

        var workflowService = new WorkflowService(context, Mock.Of<ILogger<WorkflowService>>());
        var workflowInstanceService = new WorkflowInstanceService(
            context,
            Mock.Of<ILogger<WorkflowInstanceService>>(),
            workflowService,
            Mock.Of<CRM.Core.Interfaces.IHttpCalloutService>());

        var notificationPort = new Mock<INotificationPort>();
        var jobScheduler = new Mock<ICampaignExecutionJobScheduler>();

        var service = new CampaignExecutionService(
            context,
            workflowService,
            workflowInstanceService,
            Mock.Of<ILogger<CampaignExecutionService>>(),
            notificationPort.Object,
            jobScheduler.Object);

        return (context, service, notificationPort, jobScheduler);
    }

    // ──────────────────────────────────────────────────────────────────
    // GetCampaignWorkflowsAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCampaignWorkflowsAsync_WhenNoneExist_ReturnsEmptyList()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 999);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCampaignWorkflowsAsync_ReturnsOnlyWorkflowsForGivenCampaign()
    {
        // Arrange
        var (context, service) = BuildSut();
        // Seed WorkflowDefinitions so Include(cw => cw.WorkflowDefinition) resolves (required FK = inner join in InMemory)
        context.WorkflowDefinitions.AddRange(
            new WorkflowDefinition { Id = 10 },
            new WorkflowDefinition { Id = 11 });
        context.CampaignWorkflows.AddRange(
            new CampaignWorkflow { CampaignId = 1, WorkflowDefinitionId = 10, TriggerEvent = "start", WorkflowType = "Sequential", IsActive = true },
            new CampaignWorkflow { CampaignId = 2, WorkflowDefinitionId = 11, TriggerEvent = "end", WorkflowType = "Sequential", IsActive = true });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 1);

        // Assert
        result.Should().ContainSingle(w => w.CampaignId == 1);
    }

    [Fact]
    public async Task GetCampaignWorkflowsAsync_SoftDeletedWorkflow_NotReturned()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.CampaignWorkflows.Add(new CampaignWorkflow
        {
            CampaignId = 5,
            WorkflowDefinitionId = 20,
            TriggerEvent = "t",
            WorkflowType = "Sequential",
            IsActive = false,
            IsDeleted = true
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowsAsync(campaignId: 5);

        // Assert
        result.Should().BeEmpty();
    }

    // ──────────────────────────────────────────────────────────────────
    // GetCampaignWorkflowByIdAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCampaignWorkflowByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var result = await service.GetCampaignWorkflowByIdAsync(campaignWorkflowId: 9999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCampaignWorkflowByIdAsync_ExistingId_ReturnsWorkflow()
    {
        // Arrange
        var (context, service) = BuildSut();
        // Seed WorkflowDefinition so Include resolves correctly in InMemory
        context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 30 });
        var workflow = new CampaignWorkflow
        {
            Id = 100,
            CampaignId = 3,
            WorkflowDefinitionId = 30,
            TriggerEvent = "click",
            WorkflowType = "TriggerBased",
            IsActive = true
        };
        context.CampaignWorkflows.Add(workflow);
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetCampaignWorkflowByIdAsync(100);

        // Assert
        result.Should().NotBeNull();
        result!.CampaignId.Should().Be(3);
    }

    // ──────────────────────────────────────────────────────────────────
    // LinkWorkflowToCampaignAsync — validation guards
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_CampaignNotFound_ThrowsArgumentException()
    {
        // Arrange
        var (_, service) = BuildSut();

        // Act
        var act = async () =>
            await service.LinkWorkflowToCampaignAsync(
                campaignId: 999,
                workflowDefinitionId: 1,
                workflowType: "Sequential",
                triggerEvent: "start");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Campaign not found*");
    }

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_WorkflowNotFound_ThrowsArgumentException()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 10, Name = "Test Campaign" });
        await context.SaveChangesAsync();

        // Act
        var act = async () =>
            await service.LinkWorkflowToCampaignAsync(
                campaignId: 10,
                workflowDefinitionId: 999,
                workflowType: "Sequential",
                triggerEvent: "start");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Workflow not found*");
    }

    [Fact]
    public async Task LinkWorkflowToCampaignAsync_ValidLink_CreatesWorkflowLink()
    {
        // Arrange
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 20, Name = "My Campaign" });
        context.WorkflowDefinitions.Add(new WorkflowDefinition { Id = 50, Name = "My Workflow" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.LinkWorkflowToCampaignAsync(
            campaignId: 20,
            workflowDefinitionId: 50,
            workflowType: "Sequential",
            triggerEvent: "CampaignStarted");

        // Assert
        result.Should().NotBeNull();
        result.CampaignId.Should().Be(20);
        result.WorkflowDefinitionId.Should().Be(50);
        result.TriggerEvent.Should().Be("CampaignStarted");
        result.IsActive.Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────────────────
    // ExecuteAsync (REV-STUB-011)
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_CampaignNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var (_, service, _, _) = BuildSutWithNotifications();

        // Act
        var act = async () => await service.ExecuteAsync(campaignId: 12345);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*12345*");
    }

    [Fact]
    public async Task ExecuteAsync_NoRecipients_CompletesGracefullyWithoutSending()
    {
        // Arrange
        var (context, service, notificationPort, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 200, Name = "Empty Campaign", CampaignType = CampaignType.Email });
        await context.SaveChangesAsync();

        // Act
        var result = await service.ExecuteAsync(200);

        // Assert
        result.RecipientsCount.Should().Be(0);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(0);
        result.Status.Should().Be("Completed");
        notificationPort.Verify(
            p => p.SendBulkEmailAsync(It.IsAny<IEnumerable<EmailNotificationRequest>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithRecipients_SendsBulkEmailAndRecordsTracking()
    {
        // Arrange
        var (context, service, notificationPort, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign
        {
            Id = 201,
            Name = "Spring Sale",
            CampaignType = CampaignType.Email,
            MessageSubject = "Spring Sale is here",
            MessageBody = "<p>Hello</p>",
            FromEmail = "marketing@example.com",
            FromName = "Marketing Team"
        });
        context.CampaignRecipients.AddRange(
            new CampaignRecipient { CampaignId = 201, Email = "a@example.com", FirstName = "Ann", Status = "Pending" },
            new CampaignRecipient { CampaignId = 201, Email = "b@example.com", FirstName = "Bob", Status = "Pending" });
        await context.SaveChangesAsync();

        notificationPort
            .Setup(p => p.SendBulkEmailAsync(It.IsAny<IEnumerable<EmailNotificationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<EmailNotificationRequest> reqs, CancellationToken _) =>
            {
                var list = reqs.ToList();
                return new BulkNotificationResult
                {
                    TotalCount = list.Count,
                    SuccessCount = list.Count,
                    FailureCount = 0,
                    Results = list.Select(r => new NotificationResult { Success = true, MessageId = $"msg-{r.To}" }).ToList()
                };
            });

        // Act
        var result = await service.ExecuteAsync(201);

        // Assert
        result.RecipientsCount.Should().Be(2);
        result.SuccessCount.Should().Be(2);
        result.FailureCount.Should().Be(0);
        result.Status.Should().Be("Completed");

        notificationPort.Verify(
            p => p.SendBulkEmailAsync(
                It.Is<IEnumerable<EmailNotificationRequest>>(reqs =>
                    reqs.Count() == 2 &&
                    reqs.Any(r => r.To == "a@example.com" && r.Subject == "Spring Sale is here") &&
                    reqs.Any(r => r.To == "b@example.com")),
                It.IsAny<CancellationToken>()),
            Times.Once);

        var recipients = context.CampaignRecipients.Where(r => r.CampaignId == 201).ToList();
        recipients.Should().OnlyContain(r => r.Status == CampaignRecipientStatus.Sent.ToString());
        recipients.Should().OnlyContain(r => r.SendActualTime != null);

        var tracking = context.CampaignEmailTrackings.Where(t => t.CampaignId == 201).ToList();
        tracking.Should().HaveCount(2);
        tracking.Should().OnlyContain(t => t.Event == EmailTrackingEvent.Sent);

        var campaign = await context.MarketingCampaigns.FindAsync(201);
        campaign!.EmailsSent.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_PartialSendFailure_CountsSuccessAndFailureCorrectly()
    {
        // Arrange
        var (context, service, notificationPort, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign
        {
            Id = 202,
            Name = "Partial Fail Campaign",
            CampaignType = CampaignType.Email
        });
        context.CampaignRecipients.AddRange(
            new CampaignRecipient { CampaignId = 202, Email = "good@example.com", Status = "Pending" },
            new CampaignRecipient { CampaignId = 202, Email = "bad@example.com", Status = "Pending" });
        await context.SaveChangesAsync();

        notificationPort
            .Setup(p => p.SendBulkEmailAsync(It.IsAny<IEnumerable<EmailNotificationRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<EmailNotificationRequest> reqs, CancellationToken _) =>
            {
                var list = reqs.ToList();
                var results = list.Select(r => r.To == "bad@example.com"
                    ? new NotificationResult { Success = false, Error = "Mailbox rejected" }
                    : new NotificationResult { Success = true, MessageId = "msg-ok" }).ToList();
                return new BulkNotificationResult
                {
                    TotalCount = list.Count,
                    SuccessCount = results.Count(r => r.Success),
                    FailureCount = results.Count(r => !r.Success),
                    Results = results
                };
            });

        // Act
        var result = await service.ExecuteAsync(202);

        // Assert
        result.RecipientsCount.Should().Be(2);
        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(1);
        result.Status.Should().Be("CompletedWithErrors");

        var badRecipient = context.CampaignRecipients.Single(r => r.Email == "bad@example.com");
        badRecipient.Status.Should().Be(CampaignRecipientStatus.Failed.ToString());
        badRecipient.ErrorMessage.Should().Be("Mailbox rejected");

        var goodRecipient = context.CampaignRecipients.Single(r => r.Email == "good@example.com");
        goodRecipient.Status.Should().Be(CampaignRecipientStatus.Sent.ToString());

        var tracking = context.CampaignEmailTrackings.Where(t => t.CampaignId == 202).ToList();
        tracking.Should().HaveCount(2);
        tracking.Should().Contain(t => t.RecipientEmail == "bad@example.com" && t.Event == EmailTrackingEvent.Bounced);
        tracking.Should().Contain(t => t.RecipientEmail == "good@example.com" && t.Event == EmailTrackingEvent.Sent);
    }

    [Fact]
    public async Task ExecuteAsync_NoNotificationPortConfigured_MarksAllRecipientsFailed()
    {
        // Arrange — service built WITHOUT a notification port (matches BuildSut() default null)
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 203, Name = "No Provider Campaign" });
        context.CampaignRecipients.Add(new CampaignRecipient { CampaignId = 203, Email = "x@example.com", Status = "Pending" });
        await context.SaveChangesAsync();

        // Act
        var result = await service.ExecuteAsync(203);

        // Assert
        result.RecipientsCount.Should().Be(1);
        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(1);
        result.Status.Should().Be("Failed");
        result.Errors.Should().ContainSingle();
    }

    // ──────────────────────────────────────────────────────────────────
    // PauseAsync / ResumeAsync — state-transition guards (REV-STUB-011)
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task PauseAsync_ActiveCampaign_PausesSuccessfully()
    {
        // Arrange
        var (context, service, _, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 300, Name = "Active Campaign", Status = CampaignStatus.Active });
        await context.SaveChangesAsync();

        // Act
        var paused = await service.PauseAsync(300);

        // Assert
        paused.Should().BeTrue();
        var campaign = await context.MarketingCampaigns.FindAsync(300);
        campaign!.Status.Should().Be(CampaignStatus.Paused);
    }

    [Theory]
    [InlineData(CampaignStatus.Draft)]
    [InlineData(CampaignStatus.Paused)]
    [InlineData(CampaignStatus.Completed)]
    public async Task PauseAsync_NonActiveCampaign_ReturnsFalseAndLeavesStatusUnchanged(CampaignStatus initialStatus)
    {
        // Arrange
        var (context, service, _, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 301, Name = "Non-active Campaign", Status = initialStatus });
        await context.SaveChangesAsync();

        // Act
        var paused = await service.PauseAsync(301);

        // Assert
        paused.Should().BeFalse();
        var campaign = await context.MarketingCampaigns.FindAsync(301);
        campaign!.Status.Should().Be(initialStatus);
    }

    [Fact]
    public async Task PauseAsync_CampaignNotFound_ReturnsFalse()
    {
        // Arrange
        var (_, service, _, _) = BuildSutWithNotifications();

        // Act
        var paused = await service.PauseAsync(999999);

        // Assert
        paused.Should().BeFalse();
    }

    [Fact]
    public async Task ResumeAsync_PausedCampaign_ResumesSuccessfully()
    {
        // Arrange
        var (context, service, _, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 302, Name = "Paused Campaign", Status = CampaignStatus.Paused });
        await context.SaveChangesAsync();

        // Act
        var resumed = await service.ResumeAsync(302);

        // Assert
        resumed.Should().BeTrue();
        var campaign = await context.MarketingCampaigns.FindAsync(302);
        campaign!.Status.Should().Be(CampaignStatus.Active);
    }

    [Theory]
    [InlineData(CampaignStatus.Draft)]
    [InlineData(CampaignStatus.Active)]
    [InlineData(CampaignStatus.Completed)]
    public async Task ResumeAsync_NonPausedCampaign_ReturnsFalseAndLeavesStatusUnchanged(CampaignStatus initialStatus)
    {
        // Arrange
        var (context, service, _, _) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 303, Name = "Non-paused Campaign", Status = initialStatus });
        await context.SaveChangesAsync();

        // Act
        var resumed = await service.ResumeAsync(303);

        // Assert
        resumed.Should().BeFalse();
        var campaign = await context.MarketingCampaigns.FindAsync(303);
        campaign!.Status.Should().Be(initialStatus);
    }

    // ──────────────────────────────────────────────────────────────────
    // StartCampaignAsync(int, CancellationToken) — enqueues via job scheduler (REV-STUB-011)
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartCampaignAsync_DraftCampaign_TransitionsToActiveAndEnqueuesExecutionJob()
    {
        // Arrange
        var (context, service, _, jobScheduler) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 400, Name = "Ready Campaign", Status = CampaignStatus.Draft });
        await context.SaveChangesAsync();
        jobScheduler.Setup(js => js.EnqueueExecution(400)).Returns("hangfire-job-1");

        // Act
        var status = await service.StartCampaignAsync(400, CancellationToken.None);

        // Assert
        status.Status.Should().Be(CampaignStatus.Active);
        var campaign = await context.MarketingCampaigns.FindAsync(400);
        campaign!.Status.Should().Be(CampaignStatus.Active);
        campaign.StartedAt.Should().NotBeNull();

        jobScheduler.Verify(js => js.EnqueueExecution(400), Times.Once);
    }

    [Fact]
    public async Task StartCampaignAsync_NoJobSchedulerRegistered_StillStartsWithoutThrowing()
    {
        // Arrange — service built WITHOUT a job scheduler (matches BuildSut() default null)
        var (context, service) = BuildSut();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 401, Name = "No Scheduler Campaign", Status = CampaignStatus.Scheduled });
        await context.SaveChangesAsync();

        // Act
        var status = await service.StartCampaignAsync(401, CancellationToken.None);

        // Assert
        status.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public async Task StartCampaignAsync_AlreadyActiveCampaign_DoesNotEnqueueAgain()
    {
        // Arrange
        var (context, service, _, jobScheduler) = BuildSutWithNotifications();
        context.MarketingCampaigns.Add(new MarketingCampaign { Id = 402, Name = "Already Active", Status = CampaignStatus.Active });
        await context.SaveChangesAsync();

        // Act
        await service.StartCampaignAsync(402, CancellationToken.None);

        // Assert
        jobScheduler.Verify(js => js.EnqueueExecution(It.IsAny<int>()), Times.Never);
    }
}
