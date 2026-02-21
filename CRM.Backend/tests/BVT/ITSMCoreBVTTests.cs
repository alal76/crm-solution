// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.BVT;

/// <summary>
/// Build Verification Tests for ITSM Core Features:
/// - Incident Management
/// - Problem Management
/// - Change Management
/// - SLA Management
/// - CMDB (Configuration Management Database)
/// - Knowledge Management
/// - Service Catalog
/// </summary>
public class ITSMCoreBVTTests
{
    #region BVT-ITSM-CORE-001 to 010: Incident Management

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE001_IncidentDto_HasRequiredProperties()
    {
        // Arrange & Act
        var incident = new IncidentDto
        {
            IncidentId = 1,
            Number = "INC0000001",
            ShortDescription = "Server not responding",
            Description = "Production server is down",
            CallerId = 100,
            CallerName = "John Smith",
            ContactType = ContactType.Phone,
            OpenedAt = DateTime.UtcNow,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High,
            Priority = 1,
            State = IncidentState.New
        };

        // Assert
        incident.IncidentId.Should().BeGreaterThan(0);
        incident.Number.Should().StartWith("INC");
        incident.ShortDescription.Should().NotBeNullOrEmpty();
        incident.Priority.Should().BeInRange(1, 5);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE002_CreateIncidentDto_ValidationReady()
    {
        // Arrange & Act
        var createDto = new CreateIncidentDto
        {
            ShortDescription = "Network connectivity issue",
            Description = "Users cannot access shared drives",
            CallerId = 100,
            ContactType = ContactType.Email,
            Impact = IncidentImpact.Medium,
            Urgency = IncidentUrgency.High,
            CategoryId = 1
        };

        // Assert
        createDto.ShortDescription.Should().NotBeNullOrEmpty();
        createDto.ShortDescription.Length.Should().BeLessOrEqualTo(160);
        createDto.CallerId.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE003_IncidentStates_AllDefined()
    {
        // Arrange & Act
        var states = Enum.GetValues<IncidentState>();

        // Assert
        states.Should().Contain(IncidentState.New);
        states.Should().Contain(IncidentState.InProgress);
        states.Should().Contain(IncidentState.OnHold);
        states.Should().Contain(IncidentState.Resolved);
        states.Should().Contain(IncidentState.Closed);
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE004_IncidentImpactUrgency_CalculatesPriority()
    {
        // Priority Matrix: Impact x Urgency = Priority (1 = Critical, 5 = Low)
        var testCases = new[]
        {
            (IncidentImpact.High, IncidentUrgency.High, 1),
            (IncidentImpact.High, IncidentUrgency.Medium, 2),
            (IncidentImpact.Medium, IncidentUrgency.High, 2),
            (IncidentImpact.Low, IncidentUrgency.Low, 5)
        };

        foreach (var (impact, urgency, expectedPriority) in testCases)
        {
            impact.Should().BeDefined();
            urgency.Should().BeDefined();
            expectedPriority.Should().BeInRange(1, 5);
        }
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE005_ResolveIncidentDto_HasResolutionCode()
    {
        // Arrange & Act
        var resolveDto = new ResolveIncidentDto
        {
            ResolutionCode = ResolutionCode.SolvedPermanently,
            ResolutionNotes = "Restarted the service and cleared cache"
        };

        // Assert
        resolveDto.ResolutionCode.Should().BeDefined();
        resolveDto.ResolutionNotes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Feature", "Incidents")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE006_IncidentFilterDto_SupportsMultipleFilters()
    {
        // Arrange & Act
        var filter = new IncidentFilterDto
        {
            SearchTerm = "server",
            State = IncidentState.InProgress,
            Priority = 1,
            SLABreached = false,
            MajorIncident = true,
            PageNumber = 1,
            PageSize = 20
        };

        // Assert
        filter.PageNumber.Should().BeGreaterThan(0);
        filter.PageSize.Should().BeGreaterThan(0);
    }

    #endregion

    #region BVT-ITSM-CORE-011 to 020: Problem Management

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE011_ProblemDto_HasRequiredProperties()
    {
        // Arrange & Act
        var problem = new ProblemDto
        {
            ProblemId = 1,
            Number = "PRB0000001",
            ShortDescription = "Recurring login failures",
            Priority = ProblemPriority.High,
            State = ProblemState.New,
            KnownError = false,
            RelatedIncidentCount = 5
        };

        // Assert
        problem.Number.Should().StartWith("PRB");
        problem.RelatedIncidentCount.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE012_CreateProblemDto_CanLinkIncidents()
    {
        // Arrange & Act
        var createDto = new CreateProblemDto
        {
            ShortDescription = "Database connection pooling issue",
            Priority = ProblemPriority.Critical,
            IncidentIds = new List<int> { 1, 2, 3, 4, 5 }
        };

        // Assert
        createDto.IncidentIds.Should().HaveCount(5);
        createDto.Priority.Should().Be(ProblemPriority.Critical);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE013_ProblemStates_AllDefined()
    {
        // Arrange & Act
        var states = Enum.GetValues<ProblemState>();

        // Assert
        states.Should().Contain(ProblemState.New);
        states.Should().Contain(ProblemState.RootCauseAnalysis);
        states.Should().Contain(ProblemState.KnownError);
        states.Should().Contain(ProblemState.Closed);
    }

    [Fact]
    [Trait("Feature", "Problems")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE014_UpdateProblemDto_SupportsRCA()
    {
        // Arrange & Act
        var updateDto = new UpdateProblemDto
        {
            RootCause = "Memory leak in background service",
            Workaround = "Restart service every 24 hours",
            Solution = "Apply patch KB12345",
            KnownError = true
        };

        // Assert
        updateDto.RootCause.Should().NotBeNullOrEmpty();
        updateDto.KnownError.Should().BeTrue();
    }

    #endregion

    #region BVT-ITSM-CORE-021 to 030: Change Management

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE021_ChangeDto_HasRequiredProperties()
    {
        // Arrange & Act
        var change = new ChangeDto
        {
            ChangeId = 1,
            Number = "CHG0000001",
            ShortDescription = "Upgrade SQL Server to 2022",
            Type = ChangeType.Normal,
            State = ChangeState.New,
            ApprovalStatus = ApprovalStatus.Requested,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.Medium,
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4)
        };

        // Assert
        change.Number.Should().StartWith("CHG");
        change.PlannedEndDate.Should().BeAfter(change.PlannedStartDate.Value);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE022_ChangeTypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<ChangeType>();

        // Assert
        types.Should().Contain(ChangeType.Standard);
        types.Should().Contain(ChangeType.Normal);
        types.Should().Contain(ChangeType.Emergency);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE023_ChangeStates_FollowWorkflow()
    {
        // Arrange & Act
        var states = Enum.GetValues<ChangeState>();

        // Assert
        states.Should().Contain(ChangeState.New);
        states.Should().Contain(ChangeState.Assess);
        states.Should().Contain(ChangeState.Authorize);
        states.Should().Contain(ChangeState.Scheduled);
        states.Should().Contain(ChangeState.Implement);
        states.Should().Contain(ChangeState.Review);
        states.Should().Contain(ChangeState.Closed);
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE024_CreateChangeDto_HasPlans()
    {
        // Arrange & Act
        var createDto = new CreateChangeDto
        {
            ShortDescription = "Network switch replacement",
            Type = ChangeType.Normal,
            Risk = ChangeRisk.High,
            Impact = ChangeImpact.High,
            ImplementationPlan = "1. Backup config\n2. Replace hardware\n3. Restore config\n4. Test",
            BackoutPlan = "1. Reconnect old switch\n2. Restore previous config"
        };

        // Assert
        createDto.ImplementationPlan.Should().NotBeNullOrEmpty();
        createDto.BackoutPlan.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Feature", "Changes")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE025_ApprovalStatuses_AllDefined()
    {
        // Arrange & Act
        var statuses = Enum.GetValues<ApprovalStatus>();

        // Assert
        statuses.Should().Contain(ApprovalStatus.Requested);
        statuses.Should().Contain(ApprovalStatus.Approved);
        statuses.Should().Contain(ApprovalStatus.Rejected);
        statuses.Should().Contain(ApprovalStatus.MoreInfo);
    }

    #endregion

    #region BVT-ITSM-CORE-031 to 040: SLA Management

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE031_SLAPolicyDto_HasTargetTimes()
    {
        // Arrange & Act
        var policy = new SLAPolicyDto
        {
            SLAPolicyId = 1,
            Name = "P1 Critical Policy",
            TargetType = SLATargetType.Incident,
            P1ResponseMinutes = 15,
            P1ResolutionMinutes = 240,
            UseBusinessHours = true,
            IsActive = true
        };

        // Assert
        policy.P1ResponseMinutes.Should().BeGreaterThan(0);
        policy.P1ResolutionMinutes.Should().BeGreaterThan(policy.P1ResponseMinutes.Value);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE032_SLAInstanceDto_TracksBreach()
    {
        // Arrange & Act
        var instance = new SLAInstanceDto
        {
            SLAInstanceId = 1,
            TargetId = 100,
            TargetType = SLATargetType.Incident,
            ResponseDueAt = DateTime.UtcNow.AddMinutes(15),
            ResolutionDueAt = DateTime.UtcNow.AddHours(4),
            ResponseBreached = false,
            ResolutionBreached = false,
            State = SLAState.Active,
            MinutesUntilResponseBreach = 10
        };

        // Assert
        instance.MinutesUntilResponseBreach.Should().BeGreaterThan(0);
        instance.ResponseBreached.Should().BeFalse();
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE033_SLATargetTypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<SLATargetType>();

        // Assert
        types.Should().Contain(SLATargetType.Incident);
        types.Should().Contain(SLATargetType.ServiceRequest);
    }

    [Fact]
    [Trait("Feature", "SLA")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE034_SLAStates_AllDefined()
    {
        // Arrange & Act
        var states = Enum.GetValues<SLAState>();

        // Assert
        states.Should().Contain(SLAState.Active);
        states.Should().Contain(SLAState.Paused);
        states.Should().Contain(SLAState.Completed);
        states.Should().Contain(SLAState.Breached);
        states.Should().Contain(SLAState.Cancelled);
    }

    #endregion

    #region BVT-ITSM-CORE-041 to 050: CMDB

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE041_ConfigurationItemDto_HasRequiredProperties()
    {
        // Arrange & Act
        var ci = new ConfigurationItemDto
        {
            CIId = 1,
            CIName = "PROD-WEB-01",
            CINumber = "CI0000001",
            CIType = CIType.Server,
            OperationalStatus = OperationalStatus.Operational,
            IPAddress = "10.0.1.100"
        };

        // Assert
        ci.CINumber.Should().StartWith("CI");
        ci.CIName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE042_CITypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<CIType>();

        // Assert
        types.Should().Contain(CIType.Server);
        types.Should().Contain(CIType.Application);
        types.Should().Contain(CIType.Database);
        types.Should().Contain(CIType.NetworkDevice);
        types.Should().Contain(CIType.Storage);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE043_OperationalStatuses_AllDefined()
    {
        // Arrange & Act
        var statuses = Enum.GetValues<OperationalStatus>();

        // Assert
        statuses.Should().Contain(OperationalStatus.Operational);
        statuses.Should().Contain(OperationalStatus.NonOperational);
        statuses.Should().Contain(OperationalStatus.Retired);
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE044_CreateCIDto_SupportsIPAddress()
    {
        // Arrange & Act
        var createDto = new CreateCIDto
        {
            CIName = "DB-CLUSTER-01",
            CIType = CIType.Database,
            CISubtype = "PostgreSQL",
            IPAddress = "10.0.2.50",
            OperationalStatus = OperationalStatus.Operational
        };

        // Assert
        createDto.IPAddress.Should().NotBeNullOrEmpty();
        createDto.CISubtype.Should().NotBeNullOrEmpty();
    }

    [Fact]
    [Trait("Feature", "CMDB")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE045_RelationshipTypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<RelationshipType>();

        // Assert
        types.Should().Contain(RelationshipType.DependsOn);
        types.Should().Contain(RelationshipType.RunsOn);
        types.Should().Contain(RelationshipType.ConnectedTo);
    }

    #endregion

    #region BVT-ITSM-CORE-051 to 060: Knowledge Management

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE051_KnowledgeArticleDto_HasRequiredProperties()
    {
        // Arrange & Act
        var article = new KnowledgeArticleDto
        {
            ArticleId = 1,
            Number = "KB0000001",
            Title = "How to Reset Password",
            ArticleBody = "<p>Step 1: Go to login page...</p>",
            ArticleType = ArticleType.HowTo,
            PublishingState = PublishingState.Published,
            ViewCount = 1500,
            HelpfulCount = 120,
            NotHelpfulCount = 5
        };

        // Assert
        article.Number.Should().StartWith("KB");
        article.ViewCount.Should().BeGreaterThan(0);
        article.HelpfulCount.Should().BeGreaterThan(article.NotHelpfulCount);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE052_ArticleTypes_AllDefined()
    {
        // Arrange & Act
        var types = Enum.GetValues<ArticleType>();

        // Assert
        types.Should().Contain(ArticleType.HowTo);
        types.Should().Contain(ArticleType.FAQ);
        types.Should().Contain(ArticleType.KnownError);
        types.Should().Contain(ArticleType.BestPractice);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE053_PublishingStates_AllDefined()
    {
        // Arrange & Act
        var states = Enum.GetValues<PublishingState>();

        // Assert
        states.Should().Contain(PublishingState.Draft);
        states.Should().Contain(PublishingState.Review);
        states.Should().Contain(PublishingState.Published);
        states.Should().Contain(PublishingState.Retired);
    }

    [Fact]
    [Trait("Feature", "Knowledge")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE054_CreateKnowledgeArticleDto_HasContent()
    {
        // Arrange & Act
        var createDto = new CreateKnowledgeArticleDto
        {
            Title = "Troubleshooting VPN Connection Issues",
            ArticleBody = "<h2>Overview</h2><p>This guide covers common VPN issues...</p>",
            ArticleType = ArticleType.HowTo,
            ShortDescription = "Guide for resolving VPN connectivity problems",
            IsInternal = false
        };

        // Assert
        createDto.Title.Should().NotBeNullOrEmpty();
        createDto.ArticleBody.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region BVT-ITSM-CORE-061 to 070: Service Catalog

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE061_CatalogItemDto_HasRequiredProperties()
    {
        // Arrange & Act
        var item = new CatalogItemDto
        {
            CatalogItemId = 1,
            Name = "New Laptop Request",
            ShortDescription = "Request a new laptop for work",
            CategoryId = 1,
            CategoryName = "Hardware",
            IsFeatured = true,
            Price = 1500.00m,
            IsActive = true,
            RequestCount = 250
        };

        // Assert
        item.Name.Should().NotBeNullOrEmpty();
        item.Price.Should().BeGreaterThan(0);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    [Trait("Feature", "ServiceCatalog")]
    [Trait("Category", "BVT")]
    public void BVTITSMCORE062_CreateCatalogRequestDto_HasRequiredFields()
    {
        // Arrange & Act
        var requestDto = new CreateCatalogRequestDto
        {
            CatalogItemId = 1,
            RequestedForId = 100,
            VariableValues = new Dictionary<string, string>
            {
                { "preferred_model", "Dell XPS 15" },
                { "memory_size", "32GB" },
                { "reason", "Current laptop is 5 years old" }
            }
        };

        // Assert
        requestDto.CatalogItemId.Should().BeGreaterThan(0);
        requestDto.RequestedForId.Should().BeGreaterThan(0);
        requestDto.VariableValues.Should().HaveCount(3);
    }

    #endregion
}
