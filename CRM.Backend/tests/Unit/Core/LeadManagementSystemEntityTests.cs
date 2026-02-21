// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for Lead Management, Duplicate Detection, System Configuration entities and enums.
/// </summary>
public class LeadManagementSystemEntityTests
{
    #region Lead Assignment Type Enum Tests

    [Fact]
    public void LeadAssignmentType_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<LeadAssignmentType>();

        // Assert
        values.Should().HaveCount(9);
        ((int)LeadAssignmentType.RoundRobin).Should().Be(0);
        ((int)LeadAssignmentType.Weighted).Should().Be(1);
        ((int)LeadAssignmentType.Territory).Should().Be(2);
        ((int)LeadAssignmentType.ScoreBased).Should().Be(3);
        ((int)LeadAssignmentType.FirstCome).Should().Be(4);
        ((int)LeadAssignmentType.Random).Should().Be(5);
        ((int)LeadAssignmentType.ManualQueue).Should().Be(6);
        ((int)LeadAssignmentType.SkillsBased).Should().Be(7);
        ((int)LeadAssignmentType.LoadBalanced).Should().Be(8);
    }

    [Theory]
    [InlineData(LeadAssignmentType.RoundRobin, "RoundRobin")]
    [InlineData(LeadAssignmentType.Weighted, "Weighted")]
    [InlineData(LeadAssignmentType.Territory, "Territory")]
    [InlineData(LeadAssignmentType.ScoreBased, "ScoreBased")]
    [InlineData(LeadAssignmentType.FirstCome, "FirstCome")]
    [InlineData(LeadAssignmentType.Random, "Random")]
    [InlineData(LeadAssignmentType.ManualQueue, "ManualQueue")]
    [InlineData(LeadAssignmentType.SkillsBased, "SkillsBased")]
    [InlineData(LeadAssignmentType.LoadBalanced, "LoadBalanced")]
    public void LeadAssignmentType_ShouldHaveCorrectNames(LeadAssignmentType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Routing Criteria Type Enum Tests

    [Fact]
    public void RoutingCriteriaType_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<RoutingCriteriaType>();

        // Assert
        values.Should().HaveCount(10);
        ((int)RoutingCriteriaType.LeadSource).Should().Be(0);
        ((int)RoutingCriteriaType.LeadScore).Should().Be(1);
        ((int)RoutingCriteriaType.Territory).Should().Be(2);
        ((int)RoutingCriteriaType.Industry).Should().Be(3);
        ((int)RoutingCriteriaType.CompanySize).Should().Be(4);
        ((int)RoutingCriteriaType.AnnualRevenue).Should().Be(5);
        ((int)RoutingCriteriaType.ProductInterest).Should().Be(6);
        ((int)RoutingCriteriaType.Campaign).Should().Be(7);
        ((int)RoutingCriteriaType.LeadStatus).Should().Be(8);
        ((int)RoutingCriteriaType.CustomField).Should().Be(9);
    }

    [Theory]
    [InlineData(RoutingCriteriaType.LeadSource, "LeadSource")]
    [InlineData(RoutingCriteriaType.LeadScore, "LeadScore")]
    [InlineData(RoutingCriteriaType.Territory, "Territory")]
    [InlineData(RoutingCriteriaType.Industry, "Industry")]
    [InlineData(RoutingCriteriaType.CompanySize, "CompanySize")]
    [InlineData(RoutingCriteriaType.CustomField, "CustomField")]
    public void RoutingCriteriaType_ShouldHaveCorrectNames(RoutingCriteriaType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Routing Rule Status Enum Tests

    [Fact]
    public void RoutingRuleStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<RoutingRuleStatus>();

        // Assert
        values.Should().HaveCount(3);
        ((int)RoutingRuleStatus.Active).Should().Be(0);
        ((int)RoutingRuleStatus.Inactive).Should().Be(1);
        ((int)RoutingRuleStatus.Draft).Should().Be(2);
    }

    [Theory]
    [InlineData(RoutingRuleStatus.Active, "Active")]
    [InlineData(RoutingRuleStatus.Inactive, "Inactive")]
    [InlineData(RoutingRuleStatus.Draft, "Draft")]
    public void RoutingRuleStatus_ShouldHaveCorrectNames(RoutingRuleStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Duplicate Action Enum Tests

    [Fact]
    public void DuplicateAction_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<DuplicateAction>();

        // Assert
        values.Should().HaveCount(5);
        ((int)DuplicateAction.Warn).Should().Be(0);
        ((int)DuplicateAction.Block).Should().Be(1);
        ((int)DuplicateAction.AutoMerge).Should().Be(2);
        ((int)DuplicateAction.QueueForReview).Should().Be(3);
        ((int)DuplicateAction.LogOnly).Should().Be(4);
    }

    [Theory]
    [InlineData(DuplicateAction.Warn, "Warn")]
    [InlineData(DuplicateAction.Block, "Block")]
    [InlineData(DuplicateAction.AutoMerge, "AutoMerge")]
    [InlineData(DuplicateAction.QueueForReview, "QueueForReview")]
    [InlineData(DuplicateAction.LogOnly, "LogOnly")]
    public void DuplicateAction_ShouldHaveCorrectNames(DuplicateAction value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Match Type Enum Tests

    [Fact]
    public void MatchType_ShouldHaveExpectedValues()
    {
        // Arrange & Act - Use fully qualified name to avoid ambiguity with System.IO.MatchType
        var values = Enum.GetValues<CRM.Core.Entities.MatchType>();

        // Assert
        values.Should().HaveCount(7);
        ((int)CRM.Core.Entities.MatchType.Exact).Should().Be(0);
        ((int)CRM.Core.Entities.MatchType.Fuzzy).Should().Be(1);
        ((int)CRM.Core.Entities.MatchType.Phonetic).Should().Be(2);
        ((int)CRM.Core.Entities.MatchType.Contains).Should().Be(3);
        ((int)CRM.Core.Entities.MatchType.StartsWith).Should().Be(4);
        ((int)CRM.Core.Entities.MatchType.Normalized).Should().Be(5);
        ((int)CRM.Core.Entities.MatchType.EmailDomain).Should().Be(6);
    }

    [Theory]
    [InlineData(CRM.Core.Entities.MatchType.Exact, "Exact")]
    [InlineData(CRM.Core.Entities.MatchType.Fuzzy, "Fuzzy")]
    [InlineData(CRM.Core.Entities.MatchType.Phonetic, "Phonetic")]
    [InlineData(CRM.Core.Entities.MatchType.Contains, "Contains")]
    [InlineData(CRM.Core.Entities.MatchType.StartsWith, "StartsWith")]
    [InlineData(CRM.Core.Entities.MatchType.Normalized, "Normalized")]
    [InlineData(CRM.Core.Entities.MatchType.EmailDomain, "EmailDomain")]
    public void MatchType_ShouldHaveCorrectNames(CRM.Core.Entities.MatchType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Duplicate Entity Type Enum Tests

    [Fact]
    public void DuplicateEntityType_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<DuplicateEntityType>();

        // Assert
        values.Should().HaveCount(5);
        ((int)DuplicateEntityType.Lead).Should().Be(0);
        ((int)DuplicateEntityType.Contact).Should().Be(1);
        ((int)DuplicateEntityType.Account).Should().Be(2);
        ((int)DuplicateEntityType.LeadContact).Should().Be(3);
        ((int)DuplicateEntityType.AllPersons).Should().Be(4);
    }

    [Theory]
    [InlineData(DuplicateEntityType.Lead, "Lead")]
    [InlineData(DuplicateEntityType.Contact, "Contact")]
    [InlineData(DuplicateEntityType.Account, "Account")]
    [InlineData(DuplicateEntityType.LeadContact, "LeadContact")]
    [InlineData(DuplicateEntityType.AllPersons, "AllPersons")]
    public void DuplicateEntityType_ShouldHaveCorrectNames(DuplicateEntityType value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Duplicate Candidate Status Enum Tests

    [Fact]
    public void DuplicateCandidateStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<DuplicateCandidateStatus>();

        // Assert
        values.Should().HaveCount(5);
        ((int)DuplicateCandidateStatus.Pending).Should().Be(0);
        ((int)DuplicateCandidateStatus.Confirmed).Should().Be(1);
        ((int)DuplicateCandidateStatus.Rejected).Should().Be(2);
        ((int)DuplicateCandidateStatus.Merged).Should().Be(3);
        ((int)DuplicateCandidateStatus.Ignored).Should().Be(4);
    }

    [Theory]
    [InlineData(DuplicateCandidateStatus.Pending, "Pending")]
    [InlineData(DuplicateCandidateStatus.Confirmed, "Confirmed")]
    [InlineData(DuplicateCandidateStatus.Rejected, "Rejected")]
    [InlineData(DuplicateCandidateStatus.Merged, "Merged")]
    [InlineData(DuplicateCandidateStatus.Ignored, "Ignored")]
    public void DuplicateCandidateStatus_ShouldHaveCorrectNames(DuplicateCandidateStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Merge Group Status Enum Tests

    [Fact]
    public void MergeGroupStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<MergeGroupStatus>();

        // Assert
        values.Should().HaveCount(3);
        ((int)MergeGroupStatus.Active).Should().Be(0);
        ((int)MergeGroupStatus.Unmerged).Should().Be(1);
        ((int)MergeGroupStatus.PartialUnmerge).Should().Be(2);
    }

    [Theory]
    [InlineData(MergeGroupStatus.Active, "Active")]
    [InlineData(MergeGroupStatus.Unmerged, "Unmerged")]
    [InlineData(MergeGroupStatus.PartialUnmerge, "PartialUnmerge")]
    public void MergeGroupStatus_ShouldHaveCorrectNames(MergeGroupStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Merge Group Member Status Enum Tests

    [Fact]
    public void MergeGroupMemberStatus_ShouldHaveExpectedValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<MergeGroupMemberStatus>();

        // Assert
        values.Should().HaveCount(2);
        ((int)MergeGroupMemberStatus.Merged).Should().Be(0);
        ((int)MergeGroupMemberStatus.Unmerged).Should().Be(1);
    }

    [Theory]
    [InlineData(MergeGroupMemberStatus.Merged, "Merged")]
    [InlineData(MergeGroupMemberStatus.Unmerged, "Unmerged")]
    public void MergeGroupMemberStatus_ShouldHaveCorrectNames(MergeGroupMemberStatus value, string expectedName)
    {
        value.ToString().Should().Be(expectedName);
    }

    #endregion

    #region Lead Routing Rule Entity Tests

    [Fact]
    public void LeadRoutingRule_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var rule = new LeadRoutingRule();

        // Assert
        rule.Name.Should().BeEmpty();
        rule.Description.Should().BeNull();
        rule.Status.Should().Be(RoutingRuleStatus.Active);
        rule.Priority.Should().Be(100);
        rule.AssignmentType.Should().Be(LeadAssignmentType.RoundRobin);
        rule.AssignToTeam.Should().BeFalse();
        rule.TeamId.Should().BeNull();
        rule.FallbackOwnerId.Should().BeNull();
        rule.BusinessHoursOnly.Should().BeFalse();
        rule.RoundRobinPosition.Should().Be(0);
        rule.TotalLeadsAssigned.Should().Be(0);
        rule.SendNotification.Should().BeTrue();
        rule.NotifyManager.Should().BeFalse();
        rule.Criteria.Should().NotBeNull().And.BeEmpty();
        rule.Targets.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void LeadRoutingRule_ShouldSetAndGetProperties()
    {
        // Arrange
        var rule = new LeadRoutingRule
        {
            Name = "High Score Leads",
            Description = "Route high-scoring leads to top performers",
            Status = RoutingRuleStatus.Active,
            Priority = 10,
            AssignmentType = LeadAssignmentType.ScoreBased,
            AssignToTeam = true,
            TeamId = 1,
            FallbackOwnerId = 5,
            BusinessHoursOnly = true,
            Timezone = "America/New_York",
            RoundRobinPosition = 3,
            TotalLeadsAssigned = 150,
            SendNotification = true,
            NotifyManager = true,
            NotificationTemplateId = 2
        };

        // Assert
        rule.Name.Should().Be("High Score Leads");
        rule.Description.Should().Be("Route high-scoring leads to top performers");
        rule.Status.Should().Be(RoutingRuleStatus.Active);
        rule.Priority.Should().Be(10);
        rule.AssignmentType.Should().Be(LeadAssignmentType.ScoreBased);
        rule.AssignToTeam.Should().BeTrue();
        rule.TeamId.Should().Be(1);
        rule.FallbackOwnerId.Should().Be(5);
        rule.BusinessHoursOnly.Should().BeTrue();
        rule.Timezone.Should().Be("America/New_York");
        rule.RoundRobinPosition.Should().Be(3);
        rule.TotalLeadsAssigned.Should().Be(150);
        rule.SendNotification.Should().BeTrue();
        rule.NotifyManager.Should().BeTrue();
        rule.NotificationTemplateId.Should().Be(2);
    }

    [Fact]
    public void LeadRoutingRule_ShouldTrackEffectiveDates()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddMonths(6);
        var lastAssignment = DateTime.UtcNow.AddDays(-1);

        // Act
        var rule = new LeadRoutingRule
        {
            EffectiveStartDate = startDate,
            EffectiveEndDate = endDate,
            LastAssignmentDate = lastAssignment
        };

        // Assert
        rule.EffectiveStartDate.Should().Be(startDate);
        rule.EffectiveEndDate.Should().Be(endDate);
        rule.LastAssignmentDate.Should().Be(lastAssignment);
    }

    #endregion

    #region Lead Routing Criteria Entity Tests

    [Fact]
    public void LeadRoutingCriteria_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var criteria = new LeadRoutingCriteria();

        // Assert
        criteria.CriteriaType.Should().Be(default(RoutingCriteriaType));
        criteria.FieldName.Should().BeNull();
        criteria.Operator.Should().Be("equals");
        criteria.Value.Should().BeNull();
        criteria.ValueTo.Should().BeNull();
        criteria.LogicalOperator.Should().Be("AND");
        criteria.Order.Should().Be(0);
    }

    [Fact]
    public void LeadRoutingCriteria_ShouldSetAllProperties()
    {
        // Arrange & Act
        var criteria = new LeadRoutingCriteria
        {
            CriteriaType = RoutingCriteriaType.LeadScore,
            FieldName = "Score",
            Operator = "greater_than",
            Value = "80",
            ValueTo = "100",
            LogicalOperator = "AND",
            Order = 1,
            LeadRoutingRuleId = 5
        };

        // Assert
        criteria.CriteriaType.Should().Be(RoutingCriteriaType.LeadScore);
        criteria.FieldName.Should().Be("Score");
        criteria.Operator.Should().Be("greater_than");
        criteria.Value.Should().Be("80");
        criteria.ValueTo.Should().Be("100");
        criteria.LogicalOperator.Should().Be("AND");
        criteria.Order.Should().Be(1);
        criteria.LeadRoutingRuleId.Should().Be(5);
    }

    #endregion

    #region Lead Routing Target Entity Tests

    [Fact]
    public void LeadRoutingTarget_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var target = new LeadRoutingTarget();

        // Assert
        target.UserId.Should().Be(0);
        target.Weight.Should().Be(100);
        target.MaxLeadsPerDay.Should().BeNull();
        target.MaxLeadsPerWeek.Should().BeNull();
        target.IsActive.Should().BeTrue();
        target.LeadsAssignedToday.Should().Be(0);
        target.LeadsAssignedThisWeek.Should().Be(0);
        target.LastAssignmentDate.Should().BeNull();
        target.TotalLeadsAssigned.Should().Be(0);
    }

    [Fact]
    public void LeadRoutingTarget_ShouldTrackAssignmentLimits()
    {
        // Arrange & Act
        var target = new LeadRoutingTarget
        {
            UserId = 10,
            Weight = 75,
            MaxLeadsPerDay = 20,
            MaxLeadsPerWeek = 100,
            LeadsAssignedToday = 15,
            LeadsAssignedThisWeek = 80,
            TotalLeadsAssigned = 500,
            LastAssignmentDate = DateTime.UtcNow.AddHours(-2)
        };

        // Assert
        target.UserId.Should().Be(10);
        target.Weight.Should().Be(75);
        target.MaxLeadsPerDay.Should().Be(20);
        target.MaxLeadsPerWeek.Should().Be(100);
        target.LeadsAssignedToday.Should().Be(15);
        target.LeadsAssignedThisWeek.Should().Be(80);
        target.TotalLeadsAssigned.Should().Be(500);
        target.LastAssignmentDate.Should().NotBeNull();
    }

    #endregion

    #region Lead Routing Log Entity Tests

    [Fact]
    public void LeadRoutingLog_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var log = new LeadRoutingLog();

        // Assert
        log.LeadId.Should().Be(0);
        log.LeadRoutingRuleId.Should().BeNull();
        log.AssignedToUserId.Should().BeNull();
        log.PreviousOwnerId.Should().BeNull();
        log.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        log.AssignmentType.Should().Be(default(LeadAssignmentType));
        log.Success.Should().BeTrue();
        log.FailureReason.Should().BeNull();
        log.ResponseTimeSeconds.Should().BeNull();
        log.ContactedWithinSLA.Should().BeNull();
    }

    [Fact]
    public void LeadRoutingLog_ShouldTrackAssignmentDetails()
    {
        // Arrange & Act
        var log = new LeadRoutingLog
        {
            LeadId = 123,
            LeadRoutingRuleId = 5,
            AssignedToUserId = 10,
            PreviousOwnerId = 8,
            AssignmentType = LeadAssignmentType.RoundRobin,
            Success = true,
            ResponseTimeSeconds = 3600,
            ContactedWithinSLA = true
        };

        // Assert
        log.LeadId.Should().Be(123);
        log.LeadRoutingRuleId.Should().Be(5);
        log.AssignedToUserId.Should().Be(10);
        log.PreviousOwnerId.Should().Be(8);
        log.AssignmentType.Should().Be(LeadAssignmentType.RoundRobin);
        log.Success.Should().BeTrue();
        log.ResponseTimeSeconds.Should().Be(3600);
        log.ContactedWithinSLA.Should().BeTrue();
    }

    [Fact]
    public void LeadRoutingLog_ShouldTrackFailureDetails()
    {
        // Arrange & Act
        var log = new LeadRoutingLog
        {
            LeadId = 456,
            Success = false,
            FailureReason = "No available targets matching criteria"
        };

        // Assert
        log.Success.Should().BeFalse();
        log.FailureReason.Should().Be("No available targets matching criteria");
    }

    #endregion

    #region Duplicate Rule Entity Tests

    [Fact]
    public void DuplicateRule_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var rule = new DuplicateRule();

        // Assert
        rule.Name.Should().BeEmpty();
        rule.Description.Should().BeNull();
        rule.IsActive.Should().BeTrue();
        rule.EntityType.Should().Be(DuplicateEntityType.Lead);
        rule.MatchThreshold.Should().Be(80);
        rule.Action.Should().Be(DuplicateAction.Warn);
        rule.RunOnCreate.Should().BeTrue();
        rule.RunOnUpdate.Should().BeTrue();
        rule.RunOnImport.Should().BeTrue();
        rule.Priority.Should().Be(100);
        rule.EnableBatchScan.Should().BeFalse();
        rule.TotalDuplicatesFound.Should().Be(0);
        rule.TotalDuplicatesMerged.Should().Be(0);
        rule.TotalFalsePositives.Should().Be(0);
        rule.MatchFields.Should().NotBeNull().And.BeEmpty();
        rule.Candidates.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DuplicateRule_ShouldSetAndGetProperties()
    {
        // Arrange & Act
        var rule = new DuplicateRule
        {
            Name = "Contact Email Match",
            Description = "Match contacts by email domain",
            IsActive = true,
            EntityType = DuplicateEntityType.Contact,
            MatchThreshold = 90,
            Action = DuplicateAction.Block,
            RunOnCreate = true,
            RunOnUpdate = false,
            RunOnImport = true,
            Priority = 10,
            EnableBatchScan = true,
            BatchScanFrequency = "daily",
            TotalDuplicatesFound = 150,
            TotalDuplicatesMerged = 100,
            TotalFalsePositives = 25
        };

        // Assert
        rule.Name.Should().Be("Contact Email Match");
        rule.Description.Should().Be("Match contacts by email domain");
        rule.IsActive.Should().BeTrue();
        rule.EntityType.Should().Be(DuplicateEntityType.Contact);
        rule.MatchThreshold.Should().Be(90);
        rule.Action.Should().Be(DuplicateAction.Block);
        rule.RunOnCreate.Should().BeTrue();
        rule.RunOnUpdate.Should().BeFalse();
        rule.RunOnImport.Should().BeTrue();
        rule.Priority.Should().Be(10);
        rule.EnableBatchScan.Should().BeTrue();
        rule.BatchScanFrequency.Should().Be("daily");
        rule.TotalDuplicatesFound.Should().Be(150);
        rule.TotalDuplicatesMerged.Should().Be(100);
        rule.TotalFalsePositives.Should().Be(25);
    }

    [Fact]
    public void DuplicateRule_ShouldTrackBatchScanDates()
    {
        // Arrange
        var lastScan = DateTime.UtcNow.AddDays(-1);
        var nextScan = DateTime.UtcNow.AddDays(1);

        // Act
        var rule = new DuplicateRule
        {
            EnableBatchScan = true,
            LastBatchScanDate = lastScan,
            NextBatchScanDate = nextScan
        };

        // Assert
        rule.LastBatchScanDate.Should().Be(lastScan);
        rule.NextBatchScanDate.Should().Be(nextScan);
    }

    #endregion

    #region Duplicate Match Field Entity Tests

    [Fact]
    public void DuplicateMatchField_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var field = new DuplicateMatchField();

        // Assert
        field.FieldName.Should().BeEmpty();
        field.DisplayName.Should().BeNull();
        field.MatchType.Should().Be(CRM.Core.Entities.MatchType.Exact);
        field.Weight.Should().Be(50);
        field.IsRequired.Should().BeFalse();
        field.FuzzyTolerance.Should().BeNull();
        field.IgnoreNullValues.Should().BeTrue();
        field.Transform.Should().BeNull();
        field.Order.Should().Be(0);
    }

    [Fact]
    public void DuplicateMatchField_ShouldSetAllProperties()
    {
        // Arrange & Act
        var field = new DuplicateMatchField
        {
            FieldName = "Email",
            DisplayName = "Email Address",
            MatchType = CRM.Core.Entities.MatchType.EmailDomain,
            Weight = 80,
            IsRequired = true,
            FuzzyTolerance = 85,
            IgnoreNullValues = false,
            Transform = "lowercase",
            Order = 1,
            DuplicateRuleId = 5
        };

        // Assert
        field.FieldName.Should().Be("Email");
        field.DisplayName.Should().Be("Email Address");
        field.MatchType.Should().Be(CRM.Core.Entities.MatchType.EmailDomain);
        field.Weight.Should().Be(80);
        field.IsRequired.Should().BeTrue();
        field.FuzzyTolerance.Should().Be(85);
        field.IgnoreNullValues.Should().BeFalse();
        field.Transform.Should().Be("lowercase");
        field.Order.Should().Be(1);
        field.DuplicateRuleId.Should().Be(5);
    }

    #endregion

    #region Duplicate Candidate Entity Tests

    [Fact]
    public void DuplicateCandidate_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var candidate = new DuplicateCandidate();

        // Assert
        candidate.EntityType.Should().Be(default(DuplicateEntityType));
        candidate.SourceRecordId.Should().Be(0);
        candidate.SourceRecordType.Should().BeEmpty();
        candidate.TargetRecordId.Should().Be(0);
        candidate.TargetRecordType.Should().BeEmpty();
        candidate.MatchScore.Should().Be(0);
        candidate.Status.Should().Be(DuplicateCandidateStatus.Pending);
        candidate.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        candidate.ReviewedAt.Should().BeNull();
        candidate.MergedAt.Should().BeNull();
    }

    [Fact]
    public void DuplicateCandidate_ShouldSetMatchDetails()
    {
        // Arrange & Act
        var candidate = new DuplicateCandidate
        {
            EntityType = DuplicateEntityType.Lead,
            SourceRecordId = 100,
            SourceRecordType = "Lead",
            TargetRecordId = 50,
            TargetRecordType = "Lead",
            MatchScore = 92,
            MatchingFields = "[\"Email\", \"Phone\"]",
            ComparisonData = "{\"Email\": {\"source\": \"test@example.com\", \"target\": \"test@example.com\", \"match\": 100}}",
            Status = DuplicateCandidateStatus.Confirmed,
            DuplicateRuleId = 5,
            ReviewedById = 10,
            Notes = "Confirmed duplicate - same person"
        };

        // Assert
        candidate.EntityType.Should().Be(DuplicateEntityType.Lead);
        candidate.SourceRecordId.Should().Be(100);
        candidate.SourceRecordType.Should().Be("Lead");
        candidate.TargetRecordId.Should().Be(50);
        candidate.TargetRecordType.Should().Be("Lead");
        candidate.MatchScore.Should().Be(92);
        candidate.MatchingFields.Should().Contain("Email");
        candidate.ComparisonData.Should().Contain("example.com");
        candidate.Status.Should().Be(DuplicateCandidateStatus.Confirmed);
        candidate.DuplicateRuleId.Should().Be(5);
        candidate.ReviewedById.Should().Be(10);
        candidate.Notes.Should().Be("Confirmed duplicate - same person");
    }

    #endregion

    #region Duplicate Merge History Entity Tests

    [Fact]
    public void DuplicateMergeHistory_ShouldTrackMergeDetails()
    {
        // Arrange & Act
        var history = new DuplicateMergeHistory
        {
            EntityType = DuplicateEntityType.Contact,
            SurvivingRecordId = 100,
            MergedRecordId = 200,
            MergedRecordData = "{\"FirstName\": \"John\", \"LastName\": \"Doe\"}",
            FieldsFromMergedRecord = "[\"Phone\", \"Address\"]",
            RelinkedRecords = "{\"Activities\": [1, 2, 3], \"Notes\": [5, 6]}",
            MergedById = 5,
            DuplicateCandidateId = 10,
            MergeGroupId = 3
        };

        // Assert
        history.EntityType.Should().Be(DuplicateEntityType.Contact);
        history.SurvivingRecordId.Should().Be(100);
        history.MergedRecordId.Should().Be(200);
        history.MergedRecordData.Should().Contain("John");
        history.FieldsFromMergedRecord.Should().Contain("Phone");
        history.RelinkedRecords.Should().Contain("Activities");
        history.MergedById.Should().Be(5);
        history.DuplicateCandidateId.Should().Be(10);
        history.MergeGroupId.Should().Be(3);
        history.MergedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Duplicate Merge Group Entity Tests

    [Fact]
    public void DuplicateMergeGroup_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var group = new DuplicateMergeGroup();

        // Assert
        group.EntityType.Should().Be(default(DuplicateEntityType));
        group.MasterRecordId.Should().Be(0);
        group.GroupIdentifier.Should().NotBeNullOrEmpty();
        group.Status.Should().Be(MergeGroupStatus.Active);
        group.MergedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        group.MergedById.Should().BeNull();
        group.UnmergedAt.Should().BeNull();
        group.UnmergedById.Should().BeNull();
        group.Notes.Should().BeNull();
        group.Members.Should().NotBeNull().And.BeEmpty();
        group.MergeHistories.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DuplicateMergeGroup_ShouldSetAllProperties()
    {
        // Arrange & Act
        var group = new DuplicateMergeGroup
        {
            EntityType = DuplicateEntityType.Contact,
            MasterRecordId = 100,
            GroupIdentifier = "merge-group-123",
            Status = MergeGroupStatus.Active,
            MergedById = 5,
            Notes = "Merged 3 duplicate contacts"
        };

        // Assert
        group.EntityType.Should().Be(DuplicateEntityType.Contact);
        group.MasterRecordId.Should().Be(100);
        group.GroupIdentifier.Should().Be("merge-group-123");
        group.Status.Should().Be(MergeGroupStatus.Active);
        group.MergedById.Should().Be(5);
        group.Notes.Should().Be("Merged 3 duplicate contacts");
    }

    [Fact]
    public void DuplicateMergeGroup_ShouldTrackUnmerge()
    {
        // Arrange & Act
        var unmergedAt = DateTime.UtcNow;
        var group = new DuplicateMergeGroup
        {
            Status = MergeGroupStatus.Unmerged,
            UnmergedAt = unmergedAt,
            UnmergedById = 10
        };

        // Assert
        group.Status.Should().Be(MergeGroupStatus.Unmerged);
        group.UnmergedAt.Should().Be(unmergedAt);
        group.UnmergedById.Should().Be(10);
    }

    #endregion

    #region Duplicate Merge Group Member Entity Tests

    [Fact]
    public void DuplicateMergeGroupMember_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var member = new DuplicateMergeGroupMember();

        // Assert
        member.MergeGroupId.Should().Be(0);
        member.RecordId.Should().Be(0);
        member.RecordType.Should().Be(default(DuplicateEntityType));
        member.IsMaster.Should().BeFalse();
        member.RecordSnapshot.Should().BeNull();
        member.FieldValuesUsed.Should().BeNull();
        member.RelinkedRecords.Should().BeNull();
        member.Status.Should().Be(MergeGroupMemberStatus.Merged);
        member.MergedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        member.UnmergedAt.Should().BeNull();
    }

    [Fact]
    public void DuplicateMergeGroupMember_ShouldSetAllProperties()
    {
        // Arrange & Act
        var member = new DuplicateMergeGroupMember
        {
            MergeGroupId = 5,
            RecordId = 200,
            RecordType = DuplicateEntityType.Contact,
            IsMaster = false,
            RecordSnapshot = "{\"FirstName\": \"Jane\", \"Email\": \"jane@example.com\"}",
            FieldValuesUsed = "[\"Phone\"]",
            RelinkedRecords = "{\"Activities\": [10, 11]}",
            Status = MergeGroupMemberStatus.Merged
        };

        // Assert
        member.MergeGroupId.Should().Be(5);
        member.RecordId.Should().Be(200);
        member.RecordType.Should().Be(DuplicateEntityType.Contact);
        member.IsMaster.Should().BeFalse();
        member.RecordSnapshot.Should().Contain("Jane");
        member.FieldValuesUsed.Should().Contain("Phone");
        member.RelinkedRecords.Should().Contain("Activities");
        member.Status.Should().Be(MergeGroupMemberStatus.Merged);
    }

    #endregion

    #region System Settings Entity Tests

    [Fact]
    public void SystemSettings_ShouldHaveDefaultModuleSettings()
    {
        // Arrange & Act
        var settings = new SystemSettings();

        // Assert - All modules enabled by default
        settings.AccountsEnabled.Should().BeTrue();
        settings.ContactsEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
        settings.ProductsEnabled.Should().BeTrue();
        settings.ServicesEnabled.Should().BeTrue();
        settings.CampaignsEnabled.Should().BeTrue();
        settings.QuotesEnabled.Should().BeTrue();
        settings.TasksEnabled.Should().BeTrue();
        settings.ActivitiesEnabled.Should().BeTrue();
        settings.NotesEnabled.Should().BeTrue();
        settings.WorkflowsEnabled.Should().BeTrue();
        settings.ReportsEnabled.Should().BeTrue();
        settings.DashboardEnabled.Should().BeTrue();
        settings.EmailEnabled.Should().BeTrue();
        settings.WhatsAppEnabled.Should().BeTrue();
        settings.SocialMediaEnabled.Should().BeTrue();
        settings.CommunicationsEnabled.Should().BeTrue();
        settings.InteractionsEnabled.Should().BeTrue();
    }

    [Fact]
    public void SystemSettings_ShouldAllowDisablingModules()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            AccountsEnabled = false,
            LeadsEnabled = false,
            CampaignsEnabled = false,
            WorkflowsEnabled = false,
            WhatsAppEnabled = false
        };

        // Assert
        settings.AccountsEnabled.Should().BeFalse();
        settings.LeadsEnabled.Should().BeFalse();
        settings.CampaignsEnabled.Should().BeFalse();
        settings.WorkflowsEnabled.Should().BeFalse();
        settings.WhatsAppEnabled.Should().BeFalse();
        // Others should still be true
        settings.ContactsEnabled.Should().BeTrue();
        settings.OpportunitiesEnabled.Should().BeTrue();
    }

    #endregion

    #region Lookup Category Entity Tests

    [Fact]
    public void LookupCategory_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var category = new LookupCategory();

        // Assert
        category.Name.Should().BeEmpty();
        category.Description.Should().BeNull();
        category.IsActive.Should().BeTrue();
        category.Items.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void LookupCategory_ShouldSetAndGetProperties()
    {
        // Arrange & Act
        var category = new LookupCategory
        {
            Name = "Industries",
            Description = "Industry vertical options",
            IsActive = true
        };

        // Assert
        category.Name.Should().Be("Industries");
        category.Description.Should().Be("Industry vertical options");
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LookupCategory_ShouldSupportItemsCollection()
    {
        // Arrange
        var category = new LookupCategory
        {
            Name = "Lead Sources",
            Items = new List<LookupItem>
            {
                new LookupItem { Key = "website", Value = "Website" },
                new LookupItem { Key = "referral", Value = "Referral" },
                new LookupItem { Key = "campaign", Value = "Marketing Campaign" }
            }
        };

        // Assert
        category.Items.Should().HaveCount(3);
    }

    #endregion

    #region Lookup Item Entity Tests

    [Fact]
    public void LookupItem_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var item = new LookupItem();

        // Assert
        item.LookupCategoryId.Should().Be(0);
        item.Category.Should().BeNull();
        item.Key.Should().BeEmpty();
        item.Value.Should().BeEmpty();
        item.Meta.Should().BeNull();
        item.SortOrder.Should().Be(0);
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LookupItem_ShouldSetAndGetProperties()
    {
        // Arrange & Act
        var item = new LookupItem
        {
            LookupCategoryId = 5,
            Key = "tech",
            Value = "Technology",
            Meta = "{\"icon\": \"laptop\", \"color\": \"#4A90D9\"}",
            SortOrder = 1,
            IsActive = true
        };

        // Assert
        item.LookupCategoryId.Should().Be(5);
        item.Key.Should().Be("tech");
        item.Value.Should().Be("Technology");
        item.Meta.Should().Contain("laptop");
        item.SortOrder.Should().Be(1);
        item.IsActive.Should().BeTrue();
    }

    #endregion
}
