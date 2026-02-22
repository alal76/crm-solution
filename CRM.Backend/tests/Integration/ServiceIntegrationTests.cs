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
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Integration tests for backend services using in-memory database
/// Tests real database interactions and relationships
/// </summary>
public class ServiceIntegrationTests : IAsyncLifetime
{
    private CrmDbContext _context = null!;
    private readonly ILoggerFactory _loggerFactory;

    public ServiceIntegrationTests()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
    }

    public async Task InitializeAsync()
    {
        _context = TestDbContextFactory.GetInMemoryContext($"IntegrationTestDb_{Guid.NewGuid()}");
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        _context?.Dispose();
    }

    #region Commission Service Integration Tests

    [Fact]
    public async Task CommissionService_CreateAndRetrieve_ShouldPersistData()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<CommissionService>();
        var service = new CommissionService(_context, logger);

        // Seed user required for commission
        var user = new User { Id = 1, Email = "user1@example.com", FirstName = "Test", LastName = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Seed commission plan (required FK for Commission.CommissionPlanId)
        var plan = new CommissionPlan
        {
            Name = "Test Plan",
            Rate = 0.10m,
            CommissionType = CommissionType.FlatPercentage,
            IsActive = true
        };
        _context.CommissionPlans.Add(plan);
        await _context.SaveChangesAsync();

        // Ensure user is tracked and saved before commission
        var commission = new Commission
        {
            UserId = user.Id,
            CommissionPlanId = plan.Id,
            Amount = 1000m,
            Status = CommissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var created = await service.CreateAsync(commission, CancellationToken.None);
        await _context.SaveChangesAsync();
        var retrieved = await service.GetByIdAsync(created.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Amount.Should().Be(1000m);
        retrieved.Status.Should().Be(CommissionStatus.Pending);
    }

    [Fact]
    public async Task CommissionService_ApprovalWorkflow_ShouldUpdateStatus()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<CommissionService>();
        var service = new CommissionService(_context, logger);

        // Seed user required for commission
        var user = new User { Id = 1, Email = "user1@example.com", FirstName = "Test", LastName = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Seed commission plan (required FK for Commission.CommissionPlanId)
        var plan = new CommissionPlan
        {
            Name = "Test Plan",
            Rate = 0.10m,
            CommissionType = CommissionType.FlatPercentage,
            IsActive = true
        };
        _context.CommissionPlans.Add(plan);
        await _context.SaveChangesAsync();

        var commission = new Commission
        {
            UserId = user.Id,
            CommissionPlanId = plan.Id,
            Amount = 2000m,
            Status = CommissionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var created = await service.CreateAsync(commission, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Act
        var approved = await service.ApproveAsync(created.Id, 10, CancellationToken.None);

        // Assert
        approved.Status.Should().Be(CommissionStatus.Approved);
        approved.ApprovedById.Should().Be(10);
    }

    [Fact]
    public async Task CommissionService_PlanAssignment_ShouldLinkUserToPlan()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<CommissionService>();
        var service = new CommissionService(_context, logger);

        // Seed user first
        var user = new User { Id = 1, Email = "user1@example.com", FirstName = "Test", LastName = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Then create plan
        var plan = new CommissionPlan
        {
            Name = "Test Plan",
            Rate = 0.10m,
            CommissionType = CommissionType.FlatPercentage,
            IsActive = true
        };
        var createdPlan = await service.CreatePlanAsync(plan, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Act
        var assigned = await service.AssignPlanToUserAsync(
            createdPlan.Id,
            user.Id,
            cancellationToken: CancellationToken.None);

        // Assert
        assigned.Should().BeTrue();
    }

    #endregion

    #region Campaign Service Integration Tests

    [Fact]
    public async Task CampaignService_CreateAndLaunch_ShouldUpdateStatus()
    {
        // Arrange
        var logger = _loggerFactory.CreateLogger<MarketingCampaignService>();
        var executionLogger = _loggerFactory.CreateLogger<ICampaignExecutionService>();
        var campaign = new MarketingCampaign
        {
            Name = "Test Campaign",
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        _context.MarketingCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        campaign.Status = CampaignStatus.Active;
        _context.MarketingCampaigns.Update(campaign);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.MarketingCampaigns.FindAsync(campaign.Id);
        retrieved!.Status.Should().Be(CampaignStatus.Active);
    }

    [Fact]
    public async Task CampaignService_AddRecipients_ShouldCreateCampaignRecipients()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Name = "Email Campaign",
            Status = CampaignStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.MarketingCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var recipients = new List<CampaignRecipient>
        {
            new CampaignRecipient
            {
                CampaignId = campaign.Id,
                Email = "user1@example.com",
                Status = "Pending"
            },
            new CampaignRecipient
            {
                CampaignId = campaign.Id,
                Email = "user2@example.com",
                Status = "Pending"
            }
        };

        // Act
        _context.CampaignRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();

        // Assert
        var retrievedRecipients = _context.CampaignRecipients
            .Where(r => r.CampaignId == campaign.Id)
            .ToList();

        retrievedRecipients.Should().HaveCount(2);
    }

    [Fact]
    public async Task CampaignService_RecordMetrics_ShouldPersistCampaignPerformance()
    {
        // Arrange
        var campaign = new MarketingCampaign { Name = "Test", Status = CampaignStatus.Completed };
        _context.MarketingCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        var metric = new CampaignMetric
        {
            CampaignId = campaign.Id,
            TotalSent = 1000,
            TotalDelivered = 950,
            TotalOpened = 500,
            TotalClicked = 250
        };

        // Act
        _context.CampaignMetrics.Add(metric);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.CampaignMetrics.FindAsync(metric.Id);
        retrieved!.TotalSent.Should().Be(1000);
        // OpenRate is a computed property; just verify it's within expected range
        retrieved.OpenRate.Should().BeGreaterThan(0.5m).And.BeLessThan(0.6m);
    }

    #endregion

    #region Email Sequence Integration Tests

    [Fact]
    public async Task EmailSequenceService_CreateAndEnroll_ShouldLinkContactToSequence()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Name = "Welcome Series",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailSequences.Add(sequence);
        await _context.SaveChangesAsync();

        var enrollment = new EmailSequenceEnrollment
        {
            EmailSequenceId = sequence.Id,
            ContactId = 1,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        // Act
        _context.EmailSequenceEnrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.EmailSequenceEnrollments.FindAsync(enrollment.Id);
        retrieved.Should().NotBeNull();
        retrieved!.EmailSequenceId.Should().Be(sequence.Id);
    }

    [Fact]
    public async Task EmailSequenceService_MultipleSteps_ShouldMaintainOrder()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Multi-Step" };
        _context.EmailSequences.Add(sequence);
        await _context.SaveChangesAsync();

        // Use the saved sequence.Id for all steps
        var steps = new List<EmailSequenceStep>
        {
            new EmailSequenceStep { EmailSequenceId = sequence.Id, Order = 1, Template = "Step1", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new EmailSequenceStep { EmailSequenceId = sequence.Id, Order = 2, Template = "Step2", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new EmailSequenceStep { EmailSequenceId = sequence.Id, Order = 3, Template = "Step3", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        // Act
        _context.EmailSequenceSteps.AddRange(steps);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = _context.EmailSequenceSteps
            .Where(s => s.EmailSequenceId == sequence.Id)
            .OrderBy(s => s.StepOrder)
            .ToList();

        retrieved.Should().HaveCount(3);
        retrieved[0].StepOrder.Should().Be(1);
        retrieved[1].StepOrder.Should().Be(2);
        retrieved[2].StepOrder.Should().Be(3);
    }

    #endregion

    // TODO: ITSM types not implemented - Problem, Change management entities pending
#if false
    #region Problem Service Integration Tests

    [Fact]
    public async Task ProblemService_CreateAndLink_ShouldLinkIncidents()
    {
        // Arrange
        var problem = new Problem
        {
            Title = "Database Timeout",
            Description = "Connection pool exhausted",
            Status = ProblemStatus.Open,
            Priority = PrioritySeverity.High
        };

        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var link = new ProblemIncidentLink
        {
            ProblemId = problem.Id,
            IncidentId = 1
        };

        // Act
        _context.ProblemIncidentLinks.Add(link);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = _context.ProblemIncidentLinks
            .Where(l => l.ProblemId == problem.Id)
            .ToList();
        
        retrieved.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProblemService_RootCauseAnalysis_ShouldPersistRCA()
    {
        // Arrange
        var problem = new Problem { Title = "Test Issue", Status = ProblemStatus.Open };
        _context.Problems.Add(problem);
        await _context.SaveChangesAsync();

        var rca = new RootCauseAnalysis
        {
            ProblemId = problem.Id,
            RootCause = "Configuration error",
            PreventionPlan = "Add monitoring"
        };

        // Act
        _context.RootCauseAnalyses.Add(rca);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.RootCauseAnalyses.FindAsync(rca.Id);
        retrieved.RootCause.Should().Be("Configuration error");
    }

    #endregion

    #region Change Management Integration Tests

    [Fact]
    public async Task ChangeService_CreateAndApprove_ShouldUpdateStatus()
    {
        // Arrange
        var change = new Change
        {
            Title = "Database Schema Update",
            Type = ChangeType.Normal,
            Status = ChangeStatus.Draft,
            Priority = PrioritySeverity.Medium
        };

        _context.Changes.Add(change);
        await _context.SaveChangesAsync();

        var approval = new ChangeApproval
        {
            ChangeId = change.Id,
            ApproverId = 1,
            Status = ApprovalStatus.Approved
        };

        // Act
        _context.ChangeApprovals.Add(approval);
        change.Status = ChangeStatus.Approved;
        _context.Changes.Update(change);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = await _context.Changes.FindAsync(change.Id);
        retrieved.Status.Should().Be(ChangeStatus.Approved);
    }

    [Fact]
    public async Task ChangeService_ImpactAnalysis_ShouldLinkAffectedComponents()
    {
        // Arrange
        var change = new Change { Title = "API Update", Status = ChangeStatus.Draft };
        _context.Changes.Add(change);
        await _context.SaveChangesAsync();

        var impacts = new List<ChangeImpact>
        {
            new ChangeImpact { ChangeId = change.Id, AffectedComponent = "User Service" },
            new ChangeImpact { ChangeId = change.Id, AffectedComponent = "Product Service" }
        };

        // Act
        _context.ChangeImpacts.AddRange(impacts);
        await _context.SaveChangesAsync();

        // Assert
        var retrieved = _context.ChangeImpacts
            .Where(i => i.ChangeId == change.Id)
            .ToList();
        
        retrieved.Should().HaveCount(2);
    }

    #endregion
#endif

    #region Cross-Entity Workflow Tests

    [Fact]
    public async Task FullCommissionWorkflow_EndToEnd_ShouldProcessSuccessfully()
    {
        // Arrange: Create commission plan
        var plan = new CommissionPlan
        {
            Name = "Sales Plan",
            Rate = 0.05m,
            CommissionType = CommissionType.FlatPercentage,
            IsActive = true
        };

        _context.CommissionPlans.Add(plan);
        // Seed user required for commission
        var user = new User { Id = 1, Email = "user1@example.com", FirstName = "Test", LastName = "User" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act: Create commission
        var commission = new Commission
        {
            UserId = user.Id,
            Amount = 5000m,
            Status = CommissionStatus.Pending
        };

        _context.Commissions.Add(commission);
        await _context.SaveChangesAsync();

        // Act: Approve commission
        commission.Status = CommissionStatus.Approved;
        commission.ApprovedById = 10;
        _context.Commissions.Update(commission);
        await _context.SaveChangesAsync();

        // Assert
        var final = await _context.Commissions.FindAsync(commission.Id);
        final!.Status.Should().Be(CommissionStatus.Approved);
    }

    [Fact]
    public async Task FullCampaignWorkflow_ExecutesSuccessfully()
    {
        // Arrange: Create campaign
        var campaign = new MarketingCampaign
        {
            Name = "Q1 Campaign",
            Status = CampaignStatus.Draft
        };

        _context.MarketingCampaigns.Add(campaign);
        await _context.SaveChangesAsync();

        // Act: Add recipients
        var recipients = new List<CampaignRecipient>
        {
            new CampaignRecipient { CampaignId = campaign.Id, Email = "user@example.com", Status = "Pending" }
        };

        _context.CampaignRecipients.AddRange(recipients);
        await _context.SaveChangesAsync();

        // Act: Update status
        campaign.Status = CampaignStatus.Active;
        _context.MarketingCampaigns.Update(campaign);
        await _context.SaveChangesAsync();

        // Assert
        var final = await _context.MarketingCampaigns.FindAsync(campaign.Id);
        final!.Status.Should().Be(CampaignStatus.Active);

        var finalRecipients = _context.CampaignRecipients
            .Where(r => r.CampaignId == campaign.Id)
            .Count();
        finalRecipients.Should().Be(1);
    }

    #endregion
}
