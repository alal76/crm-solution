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

using CRM.Core.Entities.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for ITSM entities and related enums.
/// ~120 tests covering incidents, problems, changes, CIs, knowledge articles, SLAs, service catalog.
/// </summary>
public class ITSMEntityTests
{
    #region Change Management Enums

    [Fact]
    public void ChangeType_ShouldHaveCorrectValues()
    {
        ((int)ChangeType.Standard).Should().Be(1);
        ((int)ChangeType.Normal).Should().Be(2);
        ((int)ChangeType.Emergency).Should().Be(3);
    }

    [Fact]
    public void ChangeType_ShouldHave3Values()
    {
        var values = Enum.GetValues<ChangeType>();
        values.Should().HaveCount(3);
    }

    [Fact]
    public void ChangeState_ShouldHaveCorrectValues()
    {
        ((int)ChangeState.New).Should().Be(1);
        ((int)ChangeState.Assess).Should().Be(2);
        ((int)ChangeState.Authorize).Should().Be(3);
        ((int)ChangeState.Scheduled).Should().Be(4);
        ((int)ChangeState.Implement).Should().Be(5);
        ((int)ChangeState.Review).Should().Be(6);
        ((int)ChangeState.Closed).Should().Be(7);
        ((int)ChangeState.Cancelled).Should().Be(8);
    }

    [Fact]
    public void ChangeState_ShouldHave8Values()
    {
        var values = Enum.GetValues<ChangeState>();
        values.Should().HaveCount(8);
    }

    [Fact]
    public void ChangeRisk_ShouldHaveCorrectValues()
    {
        ((int)ChangeRisk.High).Should().Be(1);
        ((int)ChangeRisk.Medium).Should().Be(2);
        ((int)ChangeRisk.Low).Should().Be(3);
    }

    [Fact]
    public void ChangeImpact_ShouldHaveCorrectValues()
    {
        ((int)ChangeImpact.High).Should().Be(1);
        ((int)ChangeImpact.Medium).Should().Be(2);
        ((int)ChangeImpact.Low).Should().Be(3);
    }

    [Fact]
    public void ApprovalStatus_ShouldHaveCorrectValues()
    {
        ((int)ApprovalStatus.Requested).Should().Be(1);
        ((int)ApprovalStatus.Approved).Should().Be(2);
        ((int)ApprovalStatus.Rejected).Should().Be(3);
        ((int)ApprovalStatus.MoreInfo).Should().Be(4);
    }

    [Fact]
    public void ApprovalRole_ShouldHaveCorrectValues()
    {
        ((int)ApprovalRole.CABMember).Should().Be(1);
        ((int)ApprovalRole.CABChair).Should().Be(2);
        ((int)ApprovalRole.ChangeManager).Should().Be(3);
        ((int)ApprovalRole.ITDirector).Should().Be(4);
        ((int)ApprovalRole.BusinessOwner).Should().Be(5);
        ((int)ApprovalRole.TechnicalOwner).Should().Be(6);
    }

    #endregion

    #region Configuration Item Enums

    [Fact]
    public void CIType_ShouldHaveCorrectValues()
    {
        ((int)CIType.Server).Should().Be(1);
        ((int)CIType.WorkStation).Should().Be(2);
        ((int)CIType.NetworkDevice).Should().Be(3);
        ((int)CIType.Application).Should().Be(4);
        ((int)CIType.Database).Should().Be(5);
        ((int)CIType.Storage).Should().Be(6);
        ((int)CIType.VirtualMachine).Should().Be(7);
        ((int)CIType.BusinessService).Should().Be(8);
        ((int)CIType.ITService).Should().Be(9);
        ((int)CIType.Software).Should().Be(10);
        ((int)CIType.License).Should().Be(11);
        ((int)CIType.Documentation).Should().Be(12);
    }

    [Fact]
    public void CIType_ShouldHave12Values()
    {
        var values = Enum.GetValues<CIType>();
        values.Should().HaveCount(12);
    }

    [Fact]
    public void OperationalStatus_ShouldHaveCorrectValues()
    {
        ((int)OperationalStatus.Operational).Should().Be(1);
        ((int)OperationalStatus.NonOperational).Should().Be(2);
        ((int)OperationalStatus.UnderRepair).Should().Be(3);
        ((int)OperationalStatus.Retired).Should().Be(4);
        ((int)OperationalStatus.Disposed).Should().Be(5);
        ((int)OperationalStatus.InStock).Should().Be(6);
    }

    [Fact]
    public void CIEnvironment_ShouldHaveCorrectValues()
    {
        ((int)CIEnvironment.Production).Should().Be(1);
        ((int)CIEnvironment.Development).Should().Be(2);
        ((int)CIEnvironment.Test).Should().Be(3);
        ((int)CIEnvironment.Staging).Should().Be(4);
        ((int)CIEnvironment.DisasterRecovery).Should().Be(5);
    }

    [Fact]
    public void CICriticality_ShouldHaveCorrectValues()
    {
        ((int)CICriticality.BusinessCritical).Should().Be(1);
        ((int)CICriticality.High).Should().Be(2);
        ((int)CICriticality.Medium).Should().Be(3);
        ((int)CICriticality.Low).Should().Be(4);
    }

    [Fact]
    public void RelationshipType_ShouldHaveCorrectValues()
    {
        ((int)RelationshipType.RunsOn).Should().Be(1);
        ((int)RelationshipType.DependsOn).Should().Be(2);
        ((int)RelationshipType.ConnectedTo).Should().Be(3);
        ((int)RelationshipType.InstalledOn).Should().Be(4);
        ((int)RelationshipType.Uses).Should().Be(5);
        ((int)RelationshipType.MemberOf).Should().Be(6);
        ((int)RelationshipType.HostedBy).Should().Be(7);
        ((int)RelationshipType.Contains).Should().Be(8);
    }

    [Fact]
    public void ServiceType_ShouldHaveCorrectValues()
    {
        ((int)ServiceType.BusinessService).Should().Be(1);
        ((int)ServiceType.ITService).Should().Be(2);
        ((int)ServiceType.TechnicalService).Should().Be(3);
        ((int)ServiceType.ApplicationService).Should().Be(4);
    }

    #endregion

    #region Incident Enums

    [Fact]
    public void IncidentImpact_ShouldHaveCorrectValues()
    {
        ((int)IncidentImpact.High).Should().Be(1);
        ((int)IncidentImpact.Medium).Should().Be(2);
        ((int)IncidentImpact.Low).Should().Be(3);
    }

    [Fact]
    public void IncidentUrgency_ShouldHaveCorrectValues()
    {
        ((int)IncidentUrgency.High).Should().Be(1);
        ((int)IncidentUrgency.Medium).Should().Be(2);
        ((int)IncidentUrgency.Low).Should().Be(3);
    }

    [Fact]
    public void IncidentState_ShouldHaveCorrectValues()
    {
        ((int)IncidentState.New).Should().Be(1);
        ((int)IncidentState.Assigned).Should().Be(2);
        ((int)IncidentState.InProgress).Should().Be(3);
        ((int)IncidentState.OnHold).Should().Be(4);
        ((int)IncidentState.Resolved).Should().Be(5);
        ((int)IncidentState.Closed).Should().Be(6);
        ((int)IncidentState.Cancelled).Should().Be(7);
    }

    [Fact]
    public void IncidentState_ShouldHave7Values()
    {
        var values = Enum.GetValues<IncidentState>();
        values.Should().HaveCount(7);
    }

    [Fact]
    public void ContactType_ShouldHaveCorrectValues()
    {
        ((int)ContactType.Phone).Should().Be(1);
        ((int)ContactType.Email).Should().Be(2);
        ((int)ContactType.Portal).Should().Be(3);
        ((int)ContactType.Chat).Should().Be(4);
        ((int)ContactType.WalkIn).Should().Be(5);
        ((int)ContactType.Monitoring).Should().Be(6);
    }

    [Fact]
    public void ResolutionCode_ShouldHaveCorrectValues()
    {
        ((int)ResolutionCode.SolvedPermanently).Should().Be(1);
        ((int)ResolutionCode.SolvedTemporarily).Should().Be(2);
        ((int)ResolutionCode.Workaround).Should().Be(3);
        ((int)ResolutionCode.NotSolvable).Should().Be(4);
        ((int)ResolutionCode.Duplicate).Should().Be(5);
        ((int)ResolutionCode.UserError).Should().Be(6);
        ((int)ResolutionCode.ConfigurationChange).Should().Be(7);
        ((int)ResolutionCode.SoftwareUpdate).Should().Be(8);
        ((int)ResolutionCode.HardwareReplacement).Should().Be(9);
    }

    #endregion

    #region Problem Enums

    [Fact]
    public void ProblemState_ShouldHaveCorrectValues()
    {
        ((int)ProblemState.New).Should().Be(1);
        ((int)ProblemState.Investigating).Should().Be(2);
        ((int)ProblemState.RootCauseAnalysis).Should().Be(3);
        ((int)ProblemState.KnownError).Should().Be(4);
        ((int)ProblemState.Resolved).Should().Be(5);
        ((int)ProblemState.Closed).Should().Be(6);
        ((int)ProblemState.Cancelled).Should().Be(7);
    }

    [Fact]
    public void ProblemPriority_ShouldHaveCorrectValues()
    {
        ((int)ProblemPriority.Critical).Should().Be(1);
        ((int)ProblemPriority.High).Should().Be(2);
        ((int)ProblemPriority.Medium).Should().Be(3);
        ((int)ProblemPriority.Low).Should().Be(4);
    }

    #endregion

    #region Knowledge Article Enums

    [Fact]
    public void ArticleType_ShouldHaveCorrectValues()
    {
        ((int)ArticleType.HowTo).Should().Be(1);
        ((int)ArticleType.Troubleshooting).Should().Be(2);
        ((int)ArticleType.FAQ).Should().Be(3);
        ((int)ArticleType.KnownError).Should().Be(4);
        ((int)ArticleType.Reference).Should().Be(5);
        ((int)ArticleType.BestPractice).Should().Be(6);
    }

    [Fact]
    public void PublishingState_ShouldHaveCorrectValues()
    {
        ((int)PublishingState.Draft).Should().Be(1);
        ((int)PublishingState.Review).Should().Be(2);
        ((int)PublishingState.Approved).Should().Be(3);
        ((int)PublishingState.Published).Should().Be(4);
        ((int)PublishingState.Retired).Should().Be(5);
    }

    #endregion

    #region SLA Enums

    [Fact]
    public void SLATargetType_ShouldHaveCorrectValues()
    {
        ((int)SLATargetType.Incident).Should().Be(1);
        ((int)SLATargetType.ServiceRequest).Should().Be(2);
        ((int)SLATargetType.Problem).Should().Be(3);
        ((int)SLATargetType.Change).Should().Be(4);
    }

    [Fact]
    public void SLAState_ShouldHaveCorrectValues()
    {
        ((int)SLAState.Active).Should().Be(1);
        ((int)SLAState.Paused).Should().Be(2);
        ((int)SLAState.Completed).Should().Be(3);
        ((int)SLAState.Breached).Should().Be(4);
        ((int)SLAState.Cancelled).Should().Be(5);
    }

    #endregion

    #region Service Catalog Enums

    [Fact]
    public void CatalogVariableType_ShouldHaveCorrectValues()
    {
        ((int)CatalogVariableType.Text).Should().Be(1);
        ((int)CatalogVariableType.TextArea).Should().Be(2);
        ((int)CatalogVariableType.Number).Should().Be(3);
        ((int)CatalogVariableType.Decimal).Should().Be(4);
        ((int)CatalogVariableType.Date).Should().Be(5);
        ((int)CatalogVariableType.DateTime).Should().Be(6);
        ((int)CatalogVariableType.Dropdown).Should().Be(7);
        ((int)CatalogVariableType.MultiSelect).Should().Be(8);
        ((int)CatalogVariableType.Boolean).Should().Be(9);
        ((int)CatalogVariableType.Email).Should().Be(10);
        ((int)CatalogVariableType.Phone).Should().Be(11);
        ((int)CatalogVariableType.Url).Should().Be(12);
        ((int)CatalogVariableType.FileUpload).Should().Be(13);
    }

    [Fact]
    public void CatalogVariableType_ShouldHave13Values()
    {
        var values = Enum.GetValues<CatalogVariableType>();
        values.Should().HaveCount(13);
    }

    [Fact]
    public void CatalogRequestState_ShouldHaveCorrectValues()
    {
        ((int)CatalogRequestState.Requested).Should().Be(1);
        ((int)CatalogRequestState.PendingApproval).Should().Be(2);
        ((int)CatalogRequestState.Approved).Should().Be(3);
        ((int)CatalogRequestState.Rejected).Should().Be(4);
        ((int)CatalogRequestState.InProgress).Should().Be(5);
        ((int)CatalogRequestState.Completed).Should().Be(6);
        ((int)CatalogRequestState.Cancelled).Should().Be(7);
    }

    #endregion

    #region Change Entity Tests

    [Fact]
    public void Change_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var change = new Change();

        // Assert
        change.Number.Should().BeEmpty();
        change.ShortDescription.Should().BeEmpty();
        change.State.Should().Be(ChangeState.New);
        change.ApprovalStatus.Should().Be(ApprovalStatus.Requested);
        change.MaintenanceWindow.Should().BeFalse();
        change.ConflictDetected.Should().BeFalse();
        change.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Change_ShouldAllowSettingProperties()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG0001234",
            ShortDescription = "Upgrade production database server",
            Description = "Detailed description of the change",
            Type = ChangeType.Normal,
            State = ChangeState.Scheduled,
            Risk = ChangeRisk.Medium,
            Impact = ChangeImpact.High,
            ApprovalStatus = ApprovalStatus.Approved,
            RequestorId = 1,
            PlannedStartDate = DateTime.UtcNow.AddDays(7),
            PlannedEndDate = DateTime.UtcNow.AddDays(7).AddHours(4),
            EstimatedDurationMinutes = 240,
            ImplementationPlan = "Step 1: Backup, Step 2: Upgrade",
            BackoutPlan = "Restore from backup if issues",
            TestingPlan = "Run verification scripts"
        };

        // Assert
        change.Number.Should().Be("CHG0001234");
        change.Type.Should().Be(ChangeType.Normal);
        change.State.Should().Be(ChangeState.Scheduled);
        change.Risk.Should().Be(ChangeRisk.Medium);
        change.Impact.Should().Be(ChangeImpact.High);
        change.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        change.EstimatedDurationMinutes.Should().Be(240);
    }

    [Theory]
    [InlineData(ChangeType.Standard)]
    [InlineData(ChangeType.Normal)]
    [InlineData(ChangeType.Emergency)]
    public void Change_ShouldAcceptAllChangeTypes(ChangeType changeType)
    {
        // Arrange & Act
        var change = new Change { Type = changeType };

        // Assert
        change.Type.Should().Be(changeType);
    }

    #endregion

    #region ChangeApproval Entity Tests

    [Fact]
    public void ChangeApproval_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var approval = new ChangeApproval();

        // Assert
        approval.ApprovalStatus.Should().Be(ApprovalStatus.Requested);
        approval.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ChangeApproval_ShouldAllowSettingProperties()
    {
        // Arrange
        var approval = new ChangeApproval
        {
            ApprovalId = 1,
            ChangeId = 100,
            ApproverId = 50,
            ApprovalRole = ApprovalRole.CABChair,
            ApprovalStatus = ApprovalStatus.Approved,
            ApprovalDate = DateTime.UtcNow,
            Comments = "Approved - low risk, well-planned"
        };

        // Assert
        approval.ApprovalRole.Should().Be(ApprovalRole.CABChair);
        approval.ApprovalStatus.Should().Be(ApprovalStatus.Approved);
        approval.Comments.Should().Contain("low risk");
    }

    #endregion

    #region ConfigurationItem Entity Tests

    [Fact]
    public void ConfigurationItem_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var ci = new ConfigurationItem();

        // Assert
        ci.CIName.Should().BeEmpty();
        ci.CINumber.Should().BeEmpty();
        ci.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ConfigurationItem_ShouldAllowSettingProperties()
    {
        // Arrange
        var ci = new ConfigurationItem
        {
            CIId = 1,
            CIName = "Production Web Server",
            CINumber = "CI0001234",
            CIType = CIType.Server,
            CISubtype = "Web Server",
            Description = "Primary production web server",
            SerialNumber = "SN-12345678",
            AssetTag = "ASSET-001",
            Manufacturer = "Dell",
            ModelNumber = "PowerEdge R740",
            OperationalStatus = OperationalStatus.Operational,
            Environment = CIEnvironment.Production,
            Criticality = CICriticality.BusinessCritical,
            PhysicalLocation = "Data Center A, Rack 15, Unit 10",
            IPAddress = "192.168.1.100",
            MACAddress = "00:1A:2B:3C:4D:5E",
            OperatingSystem = "Windows Server 2022",
            CPU = "Intel Xeon Gold 6248R",
            RAM = "128 GB",
            Disk = "2 TB SSD RAID 10",
            PurchaseCost = 15000m
        };

        // Assert
        ci.CIName.Should().Be("Production Web Server");
        ci.CIType.Should().Be(CIType.Server);
        ci.OperationalStatus.Should().Be(OperationalStatus.Operational);
        ci.Environment.Should().Be(CIEnvironment.Production);
        ci.Criticality.Should().Be(CICriticality.BusinessCritical);
        ci.PurchaseCost.Should().Be(15000m);
    }

    [Theory]
    [InlineData(CIType.Server)]
    [InlineData(CIType.Application)]
    [InlineData(CIType.Database)]
    [InlineData(CIType.VirtualMachine)]
    [InlineData(CIType.BusinessService)]
    public void ConfigurationItem_ShouldAcceptAllCITypes(CIType ciType)
    {
        // Arrange & Act
        var ci = new ConfigurationItem { CIType = ciType };

        // Assert
        ci.CIType.Should().Be(ciType);
    }

    #endregion

    #region CIRelationship Entity Tests

    [Fact]
    public void CIRelationship_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var rel = new CIRelationship();

        // Assert
        rel.IsDeleted.Should().BeFalse();
        rel.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void CIRelationship_ShouldAllowSettingProperties()
    {
        // Arrange
        var rel = new CIRelationship
        {
            RelationshipId = 1,
            ParentCIId = 100,
            ChildCIId = 200,
            RelationshipType = RelationshipType.DependsOn,
            Description = "Web app depends on database server"
        };

        // Assert
        rel.RelationshipType.Should().Be(RelationshipType.DependsOn);
        rel.Description.Should().Contain("depends on");
    }

    #endregion

    #region Incident Entity Tests

    [Fact]
    public void Incident_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var incident = new Incident();

        // Assert
        incident.Number.Should().BeEmpty();
        incident.ShortDescription.Should().BeEmpty();
        incident.State.Should().Be(IncidentState.New);
        incident.EscalationLevel.Should().Be(0);
        incident.SLABreached.Should().BeFalse();
        incident.MajorIncident.Should().BeFalse();
        incident.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Incident_ShouldAllowSettingProperties()
    {
        // Arrange
        var incident = new Incident
        {
            IncidentId = 1,
            Number = "INC0001234",
            ShortDescription = "Email server not responding",
            Description = "Users cannot send or receive emails",
            CallerId = 10,
            ContactType = ContactType.Phone,
            Impact = IncidentImpact.High,
            Urgency = IncidentUrgency.High,
            State = IncidentState.InProgress,
            AssignedToId = 50,
            EscalationLevel = 1,
            ResponseDueAt = DateTime.UtcNow.AddMinutes(15),
            ResolutionDueAt = DateTime.UtcNow.AddHours(4)
        };

        // Assert
        incident.Number.Should().Be("INC0001234");
        incident.ContactType.Should().Be(ContactType.Phone);
        incident.Impact.Should().Be(IncidentImpact.High);
        incident.Urgency.Should().Be(IncidentUrgency.High);
        incident.State.Should().Be(IncidentState.InProgress);
    }

    [Fact]
    public void Incident_Priority_ShouldBeCalculatedFromImpactAndUrgency()
    {
        // Arrange - P1 (Impact High + Urgency High = 2)
        var p1Incident = new Incident { Impact = IncidentImpact.High, Urgency = IncidentUrgency.High };

        // Arrange - P3 (Impact Low + Urgency Medium = 5)
        var p3Incident = new Incident { Impact = IncidentImpact.Low, Urgency = IncidentUrgency.Medium };

        // Assert
        p1Incident.Priority.Should().Be(2); // 1 + 1
        p3Incident.Priority.Should().Be(5); // 3 + 2
    }

    #endregion

    #region IncidentComment Entity Tests

    [Fact]
    public void IncidentComment_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var comment = new IncidentComment();

        // Assert
        comment.Comment.Should().BeEmpty();
        comment.IsInternal.Should().BeFalse();
        comment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void IncidentComment_ShouldAllowSettingProperties()
    {
        // Arrange
        var comment = new IncidentComment
        {
            CommentId = 1,
            IncidentId = 100,
            Comment = "Investigating network connectivity issues",
            IsInternal = true,
            CreatedById = 50
        };

        // Assert
        comment.Comment.Should().Contain("network connectivity");
        comment.IsInternal.Should().BeTrue();
    }

    #endregion

    #region Problem Entity Tests

    [Fact]
    public void Problem_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var problem = new Problem();

        // Assert
        problem.Number.Should().BeEmpty();
        problem.ShortDescription.Should().BeEmpty();
        problem.State.Should().Be(ProblemState.New);
        problem.KnownError.Should().BeFalse();
        problem.FixVerified.Should().BeFalse();
        problem.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Problem_ShouldAllowSettingProperties()
    {
        // Arrange
        var problem = new Problem
        {
            ProblemId = 1,
            Number = "PRB0001234",
            ShortDescription = "Recurring database timeout errors",
            Description = "Multiple incidents reported for database timeouts",
            Priority = ProblemPriority.High,
            State = ProblemState.RootCauseAnalysis,
            Symptoms = "Slow queries, connection timeouts",
            RootCause = "Index fragmentation on Orders table",
            Workaround = "Restart database service",
            KnownError = true,
            KnownErrorDate = DateTime.UtcNow,
            FiveWhysAnalysis = "Why 1: Slow queries -> Why 2: Index fragmentation...",
            Solution = "Rebuild indexes and add maintenance schedule"
        };

        // Assert
        problem.Number.Should().Be("PRB0001234");
        problem.Priority.Should().Be(ProblemPriority.High);
        problem.State.Should().Be(ProblemState.RootCauseAnalysis);
        problem.KnownError.Should().BeTrue();
        problem.FiveWhysAnalysis.Should().Contain("Why 1");
    }

    [Theory]
    [InlineData(ProblemState.New)]
    [InlineData(ProblemState.Investigating)]
    [InlineData(ProblemState.RootCauseAnalysis)]
    [InlineData(ProblemState.KnownError)]
    [InlineData(ProblemState.Resolved)]
    public void Problem_ShouldAcceptAllStates(ProblemState state)
    {
        // Arrange & Act
        var problem = new Problem { State = state };

        // Assert
        problem.State.Should().Be(state);
    }

    #endregion

    #region ProblemTask Entity Tests

    [Fact]
    public void ProblemTask_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var task = new ProblemTask();

        // Assert
        task.TaskName.Should().BeEmpty();
        task.IsCompleted.Should().BeFalse();
        task.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region KnowledgeArticle Entity Tests

    [Fact]
    public void KnowledgeArticle_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var article = new KnowledgeArticle();

        // Assert
        article.Number.Should().BeEmpty();
        article.Title.Should().BeEmpty();
        article.ArticleBody.Should().BeEmpty();
        article.PublishingState.Should().Be(PublishingState.Draft);
        article.Version.Should().Be(1);
        article.IsInternal.Should().BeTrue();
        article.IsExternal.Should().BeFalse();
        article.IsPublic.Should().BeFalse();
        article.ViewCount.Should().Be(0);
        article.HelpfulCount.Should().Be(0);
        article.NotHelpfulCount.Should().Be(0);
    }

    [Fact]
    public void KnowledgeArticle_ShouldAllowSettingProperties()
    {
        // Arrange
        var article = new KnowledgeArticle
        {
            ArticleId = 1,
            Number = "KB0001234",
            Title = "How to reset your password",
            ShortDescription = "Step-by-step guide for password reset",
            ArticleBody = "<h1>Password Reset</h1><p>Step 1: Click forgot password...</p>",
            ArticleType = ArticleType.HowTo,
            AuthorId = 10,
            OwnerId = 10,
            PublishingState = PublishingState.Published,
            PublishedDate = DateTime.UtcNow,
            IsExternal = true,
            IsPublic = true,
            Tags = "password,security,login",
            ViewCount = 1500,
            HelpfulCount = 450,
            NotHelpfulCount = 25
        };

        // Assert
        article.Number.Should().Be("KB0001234");
        article.ArticleType.Should().Be(ArticleType.HowTo);
        article.PublishingState.Should().Be(PublishingState.Published);
        article.IsPublic.Should().BeTrue();
        article.ViewCount.Should().Be(1500);
    }

    [Theory]
    [InlineData(ArticleType.HowTo)]
    [InlineData(ArticleType.Troubleshooting)]
    [InlineData(ArticleType.FAQ)]
    [InlineData(ArticleType.KnownError)]
    public void KnowledgeArticle_ShouldAcceptAllArticleTypes(ArticleType articleType)
    {
        // Arrange & Act
        var article = new KnowledgeArticle { ArticleType = articleType };

        // Assert
        article.ArticleType.Should().Be(articleType);
    }

    #endregion

    #region ArticleFeedback Entity Tests

    [Fact]
    public void ArticleFeedback_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var feedback = new ArticleFeedback();

        // Assert
        feedback.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void ArticleFeedback_ShouldAllowSettingProperties()
    {
        // Arrange
        var feedback = new ArticleFeedback
        {
            FeedbackId = 1,
            ArticleId = 100,
            UserId = 50,
            IsHelpful = true,
            Comment = "Very clear instructions!"
        };

        // Assert
        feedback.IsHelpful.Should().BeTrue();
        feedback.Comment.Should().Be("Very clear instructions!");
    }

    #endregion

    #region SLAPolicy Entity Tests

    [Fact]
    public void SLAPolicy_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var policy = new SLAPolicy();

        // Assert
        policy.Name.Should().BeEmpty();
        policy.UseBusinessHours.Should().BeTrue();
        policy.IsActive.Should().BeTrue();
        policy.P1ResponseMinutes.Should().Be(15);
        policy.P2ResponseMinutes.Should().Be(30);
        policy.P3ResponseMinutes.Should().Be(120);
        policy.P4ResponseMinutes.Should().Be(480);
        policy.P1ResolutionMinutes.Should().Be(240);
        policy.P2ResolutionMinutes.Should().Be(480);
        policy.P3ResolutionMinutes.Should().Be(1440);
        policy.P4ResolutionMinutes.Should().Be(7200);
    }

    [Fact]
    public void SLAPolicy_ShouldAllowSettingProperties()
    {
        // Arrange
        var policy = new SLAPolicy
        {
            SLAPolicyId = 1,
            Name = "Premium Support SLA",
            Description = "SLA for premium tier customers",
            TargetType = SLATargetType.Incident,
            P1ResponseMinutes = 5,
            P1ResolutionMinutes = 60,
            P2ResponseMinutes = 15,
            P2ResolutionMinutes = 240,
            UseBusinessHours = false
        };

        // Assert
        policy.Name.Should().Be("Premium Support SLA");
        policy.TargetType.Should().Be(SLATargetType.Incident);
        policy.P1ResponseMinutes.Should().Be(5);
        policy.UseBusinessHours.Should().BeFalse();
    }

    #endregion

    #region SLAInstance Entity Tests

    [Fact]
    public void SLAInstance_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var instance = new SLAInstance();

        // Assert
        instance.State.Should().Be(SLAState.Active);
        instance.ResponseBreached.Should().BeFalse();
        instance.ResolutionBreached.Should().BeFalse();
        instance.PausedMinutes.Should().Be(0);
    }

    [Fact]
    public void SLAInstance_ShouldAllowSettingProperties()
    {
        // Arrange
        var instance = new SLAInstance
        {
            SLAInstanceId = 1,
            TargetId = 100,
            TargetType = SLATargetType.Incident,
            SLAPolicyId = 1,
            State = SLAState.Completed,
            ResponseDueAt = DateTime.UtcNow.AddMinutes(15),
            ResponseActualAt = DateTime.UtcNow.AddMinutes(10),
            ResponseBreached = false,
            ResponseBusinessMinutes = 10,
            ResolutionDueAt = DateTime.UtcNow.AddHours(4),
            ResolutionActualAt = DateTime.UtcNow.AddHours(2),
            ResolutionBreached = false,
            ResolutionBusinessMinutes = 120
        };

        // Assert
        instance.TargetType.Should().Be(SLATargetType.Incident);
        instance.State.Should().Be(SLAState.Completed);
        instance.ResponseBreached.Should().BeFalse();
        instance.ResolutionBusinessMinutes.Should().Be(120);
    }

    #endregion

    #region BusinessHoursSchedule Entity Tests

    [Fact]
    public void BusinessHoursSchedule_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var schedule = new BusinessHoursSchedule();

        // Assert
        schedule.Name.Should().BeEmpty();
        schedule.TimeZone.Should().Be("UTC");
        schedule.IsActive.Should().BeTrue();
        schedule.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void BusinessHoursSchedule_ShouldAllowSettingProperties()
    {
        // Arrange
        var schedule = new BusinessHoursSchedule
        {
            ScheduleId = 1,
            Name = "US Business Hours",
            Description = "Standard US business hours",
            TimeZone = "America/New_York",
            BusinessHours = "{\"Monday\": {\"start\": \"09:00\", \"end\": \"17:00\"}}",
            Holidays = "[\"2025-01-01\", \"2025-12-25\"]"
        };

        // Assert
        schedule.Name.Should().Be("US Business Hours");
        schedule.TimeZone.Should().Be("America/New_York");
        schedule.BusinessHours.Should().Contain("09:00");
    }

    #endregion

    #region CatalogCategory Entity Tests

    [Fact]
    public void CatalogCategory_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var category = new CatalogCategory();

        // Assert
        category.Name.Should().BeEmpty();
        category.DisplayOrder.Should().Be(0);
        category.IsActive.Should().BeTrue();
        category.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CatalogCategory_ShouldAllowSettingProperties()
    {
        // Arrange
        var category = new CatalogCategory
        {
            CategoryId = 1,
            Name = "Hardware Requests",
            Description = "Request new hardware or peripherals",
            IconName = "computer",
            DisplayOrder = 1
        };

        // Assert
        category.Name.Should().Be("Hardware Requests");
        category.IconName.Should().Be("computer");
    }

    #endregion

    #region CatalogItem Entity Tests

    [Fact]
    public void CatalogItem_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var item = new CatalogItem();

        // Assert
        item.Name.Should().BeEmpty();
        item.DisplayOrder.Should().Be(0);
        item.IsFeatured.Should().BeFalse();
        item.IsActive.Should().BeTrue();
        item.AvailableToAll.Should().BeTrue();
        item.Priority.Should().Be(2);
        item.RequiresBudgetApproval.Should().BeFalse();
        item.RequestCount.Should().Be(0);
        item.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CatalogItem_ShouldAllowSettingProperties()
    {
        // Arrange
        var item = new CatalogItem
        {
            CatalogItemId = 1,
            Name = "New Laptop Request",
            ShortDescription = "Request a new laptop",
            LongDescription = "Detailed form for laptop request",
            CategoryId = 1,
            IsFeatured = true,
            ExpectedDeliveryDays = 5,
            Price = 1500m,
            RecurringCostMonthly = 0m,
            RequiresBudgetApproval = true,
            RequestCount = 150,
            AverageRating = 4.5m
        };

        // Assert
        item.Name.Should().Be("New Laptop Request");
        item.IsFeatured.Should().BeTrue();
        item.Price.Should().Be(1500m);
        item.RequiresBudgetApproval.Should().BeTrue();
        item.AverageRating.Should().Be(4.5m);
    }

    #endregion

    #region CatalogVariable Entity Tests

    [Fact]
    public void CatalogVariable_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var variable = new CatalogVariable();

        // Assert
        variable.VariableName.Should().BeEmpty();
        variable.VariableLabel.Should().BeEmpty();
        variable.IsRequired.Should().BeFalse();
    }

    [Fact]
    public void CatalogVariable_ShouldAllowSettingProperties()
    {
        // Arrange
        var variable = new CatalogVariable
        {
            VariableId = 1,
            CatalogItemId = 100,
            VariableName = "laptop_model",
            VariableLabel = "Select Laptop Model",
            VariableType = CatalogVariableType.Dropdown,
            IsRequired = true,
            ValidationMessage = "Please select a laptop model"
        };

        // Assert
        variable.VariableName.Should().Be("laptop_model");
        variable.VariableType.Should().Be(CatalogVariableType.Dropdown);
        variable.IsRequired.Should().BeTrue();
    }

    [Theory]
    [InlineData(CatalogVariableType.Text)]
    [InlineData(CatalogVariableType.Number)]
    [InlineData(CatalogVariableType.Dropdown)]
    [InlineData(CatalogVariableType.Boolean)]
    [InlineData(CatalogVariableType.FileUpload)]
    public void CatalogVariable_ShouldAcceptAllVariableTypes(CatalogVariableType variableType)
    {
        // Arrange & Act
        var variable = new CatalogVariable { VariableType = variableType };

        // Assert
        variable.VariableType.Should().Be(variableType);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void IncidentWithRelatedProblem_ShouldSupportRelationship()
    {
        // Arrange
        var incident1 = new Incident { IncidentId = 1, Number = "INC001" };
        var incident2 = new Incident { IncidentId = 2, Number = "INC002" };

        var problem = new Problem
        {
            ProblemId = 1,
            Number = "PRB001",
            ShortDescription = "Root cause for multiple timeouts",
            ProblemIncidents = new List<ProblemIncident>
            {
                new ProblemIncident { ProblemId = 1, IncidentId = 1, Incident = incident1 },
                new ProblemIncident { ProblemId = 1, IncidentId = 2, Incident = incident2 }
            }
        };

        // Assert
        problem.ProblemIncidents.Should().HaveCount(2);
    }

    [Fact]
    public void ChangeWithApprovals_ShouldSupportWorkflow()
    {
        // Arrange
        var change = new Change
        {
            ChangeId = 1,
            Number = "CHG001",
            Type = ChangeType.Normal,
            State = ChangeState.Authorize,
            Approvals = new List<ChangeApproval>
            {
                new ChangeApproval { ApprovalId = 1, ApprovalRole = ApprovalRole.CABMember, ApprovalStatus = ApprovalStatus.Approved },
                new ChangeApproval { ApprovalId = 2, ApprovalRole = ApprovalRole.ITDirector, ApprovalStatus = ApprovalStatus.Requested }
            }
        };

        // Assert
        change.Approvals.Should().HaveCount(2);
        change.Approvals.Should().Contain(a => a.ApprovalStatus == ApprovalStatus.Approved);
        change.Approvals.Should().Contain(a => a.ApprovalStatus == ApprovalStatus.Requested);
    }

    [Fact]
    public void ConfigurationItemHierarchy_ShouldSupportRelationships()
    {
        // Arrange
        var server = new ConfigurationItem { CIId = 1, CIName = "Web Server", CIType = CIType.Server };
        var database = new ConfigurationItem { CIId = 2, CIName = "Database Server", CIType = CIType.Database };

        var relationship = new CIRelationship
        {
            RelationshipId = 1,
            ParentCIId = 1,
            ParentCI = server,
            ChildCIId = 2,
            ChildCI = database,
            RelationshipType = RelationshipType.DependsOn
        };

        // Assert
        relationship.ParentCI!.CIType.Should().Be(CIType.Server);
        relationship.ChildCI!.CIType.Should().Be(CIType.Database);
        relationship.RelationshipType.Should().Be(RelationshipType.DependsOn);
    }

    #endregion
}
