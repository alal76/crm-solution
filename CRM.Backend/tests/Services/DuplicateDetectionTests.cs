// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Duplicate Detection Service Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchTypeEntity = CRM.Core.Entities.MatchType;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Duplicate Detection functionality
/// </summary>
public class DuplicateDetectionTests
{
    #region Entity Tracking Fields Tests

    [Fact]
    public void Lead_HasMergeTrackingFields()
    {
        // Arrange & Act
        var lead = new Lead
        {
            FirstName = "Test",
            LastName = "Lead",
            Email = "test@example.com",
            IsMergedDuplicate = false,
            MergedIntoId = null,
            MergedAt = null
        };

        // Assert
        lead.Should().NotBeNull();
        lead.IsMergedDuplicate.Should().BeFalse();
        lead.MergedIntoId.Should().BeNull();
        lead.MergedAt.Should().BeNull();
    }

    [Fact]
    public void Lead_CanBeMerged()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Test",
            LastName = "Lead",
            Email = "test@example.com"
        };

        // Act - Mark as merged
        lead.IsMergedDuplicate = true;
        lead.MergedIntoId = 2;
        lead.MergedAt = DateTime.UtcNow;

        // Assert
        lead.IsMergedDuplicate.Should().BeTrue();
        lead.MergedIntoId.Should().Be(2);
        lead.MergedAt.Should().NotBeNull();
    }

    [Fact]
    public void Account_HasMergeTrackingFields()
    {
        // Arrange & Act
        var account = new Account
        {
            Company = "Test Account",
            Email = "account@example.com",
            IsMergedDuplicate = false,
            MergedIntoId = null,
            MergedAt = null
        };

        // Assert
        account.Should().NotBeNull();
        account.IsMergedDuplicate.Should().BeFalse();
        account.MergedIntoId.Should().BeNull();
        account.MergedAt.Should().BeNull();
    }

    [Fact]
    public void Contact_HasMergeTrackingFields()
    {
        // Arrange & Act
        var contact = new Contact
        {
            FirstName = "Test",
            LastName = "Contact",
            EmailPrimary = "contact@example.com",
            IsMergedDuplicate = false,
            MergedIntoId = null,
            MergedAt = null
        };

        // Assert
        contact.Should().NotBeNull();
        contact.IsMergedDuplicate.Should().BeFalse();
        contact.MergedIntoId.Should().BeNull();
        contact.MergedAt.Should().BeNull();
    }

    #endregion

    #region DuplicateRule Entity Tests

    [Fact]
    public void DuplicateRule_CreateForLead_IsValid()
    {
        // Arrange & Act
        var rule = new DuplicateRule
        {
            Name = "Lead Email Match",
            Description = "Match leads by email address",
            EntityType = DuplicateEntityType.Lead,
            IsActive = true,
            MatchThreshold = 80,
            Priority = 1
        };

        // Assert
        rule.Should().NotBeNull();
        rule.Name.Should().Be("Lead Email Match");
        rule.EntityType.Should().Be(DuplicateEntityType.Lead);
        rule.MatchThreshold.Should().Be(80);
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void DuplicateRule_WithMatchFields_IsValid()
    {
        // Arrange & Act
        var rule = new DuplicateRule
        {
            Name = "Lead Full Match",
            EntityType = DuplicateEntityType.Lead,
            IsActive = true,
            MatchThreshold = 70,
            MatchFields = new List<DuplicateMatchField>
            {
                new DuplicateMatchField 
                { 
                    FieldName = "Email", 
                    MatchType = MatchTypeEntity.Exact, 
                    Weight = 40 
                },
                new DuplicateMatchField 
                { 
                    FieldName = "CompanyName", 
                    MatchType = MatchTypeEntity.Fuzzy, 
                    Weight = 30 
                },
                new DuplicateMatchField 
                { 
                    FieldName = "Phone", 
                    MatchType = MatchTypeEntity.Normalized, 
                    Weight = 30 
                }
            }
        };

        // Assert
        rule.MatchFields.Should().HaveCount(3);
        rule.MatchFields.First().FieldName.Should().Be("Email");
        rule.MatchFields.Sum(f => f.Weight).Should().Be(100);
    }

    #endregion

    #region DuplicateMergeGroup Entity Tests

    [Fact]
    public void DuplicateMergeGroup_CreateForLeads_IsValid()
    {
        // Arrange & Act
        var group = new DuplicateMergeGroup
        {
            EntityType = DuplicateEntityType.Lead,
            MasterRecordId = 1,
            Status = MergeGroupStatus.Active,
            MergedAt = DateTime.UtcNow,
            MergedById = 1
        };

        // Assert
        group.Should().NotBeNull();
        group.EntityType.Should().Be(DuplicateEntityType.Lead);
        group.MasterRecordId.Should().Be(1);
        group.Status.Should().Be(MergeGroupStatus.Active);
    }

    [Fact]
    public void DuplicateMergeGroup_WithMembers_IsValid()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var group = new DuplicateMergeGroup
        {
            Id = 1,
            EntityType = DuplicateEntityType.Lead,
            MasterRecordId = 1,
            Status = MergeGroupStatus.Active,
            MergedAt = now,
            MergedById = 1
        };

        // Act - Add members
        group.Members = new List<DuplicateMergeGroupMember>
        {
            new DuplicateMergeGroupMember
            {
                MergeGroupId = 1,
                RecordId = 2,
                IsMaster = false,
                MergedAt = now,
                Status = MergeGroupMemberStatus.Merged
            },
            new DuplicateMergeGroupMember
            {
                MergeGroupId = 1,
                RecordId = 3,
                IsMaster = false,
                MergedAt = now,
                Status = MergeGroupMemberStatus.Merged
            }
        };

        // Assert
        group.Members.Should().HaveCount(2);
        group.Members.All(m => !m.IsMaster).Should().BeTrue();
        group.Members.All(m => m.Status == MergeGroupMemberStatus.Merged).Should().BeTrue();
    }

    #endregion

    #region DuplicateCandidate Entity Tests

    [Fact]
    public void DuplicateCandidate_Create_IsValid()
    {
        // Arrange & Act
        var candidate = new DuplicateCandidate
        {
            DuplicateRuleId = 1,
            EntityType = DuplicateEntityType.Lead,
            SourceRecordId = 1,
            TargetRecordId = 2,
            MatchScore = 85,
            DetectedAt = DateTime.UtcNow,
            Status = DuplicateCandidateStatus.Pending
        };

        // Assert
        candidate.Should().NotBeNull();
        candidate.MatchScore.Should().Be(85);
        candidate.Status.Should().Be(DuplicateCandidateStatus.Pending);
    }

    [Fact]
    public void DuplicateCandidate_StatusTransitions_AreValid()
    {
        // Arrange
        var candidate = new DuplicateCandidate
        {
            Status = DuplicateCandidateStatus.Pending
        };

        // Act & Assert - Pending to Confirmed
        candidate.Status = DuplicateCandidateStatus.Confirmed;
        candidate.Status.Should().Be(DuplicateCandidateStatus.Confirmed);

        // Create new for rejected
        var candidate2 = new DuplicateCandidate
        {
            Status = DuplicateCandidateStatus.Pending
        };
        candidate2.Status = DuplicateCandidateStatus.Rejected;
        candidate2.Status.Should().Be(DuplicateCandidateStatus.Rejected);

        // Create new for merged
        var candidate3 = new DuplicateCandidate
        {
            Status = DuplicateCandidateStatus.Confirmed
        };
        candidate3.Status = DuplicateCandidateStatus.Merged;
        candidate3.Status.Should().Be(DuplicateCandidateStatus.Merged);
    }

    #endregion

    #region DuplicateMergeHistory Entity Tests

    [Fact]
    public void DuplicateMergeHistory_Create_IsValid()
    {
        // Arrange & Act
        var history = new DuplicateMergeHistory
        {
            EntityType = DuplicateEntityType.Lead,
            SurvivingRecordId = 1,
            MergedRecordId = 2,
            MergedAt = DateTime.UtcNow,
            MergedById = 1,
            MergedRecordData = "{\"FirstName\":\"John\",\"LastName\":\"Doe\"}"
        };

        // Assert
        history.Should().NotBeNull();
        history.MergedRecordData.Should().NotBeEmpty();
        history.SurvivingRecordId.Should().Be(1);
        history.MergedRecordId.Should().Be(2);
    }

    #endregion

    #region FieldMatchResult Tests

    [Fact]
    public void FieldMatchResult_Create_IsValid()
    {
        // Arrange & Act
        var result = new FieldMatchResult
        {
            FieldName = "Email",
            Value1 = "john@example.com",
            Value2 = "john@example.com",
            MatchingType = MatchTypeEntity.Exact,
            Score = 100,
            IsMatch = true
        };

        // Assert
        result.Should().NotBeNull();
        result.FieldName.Should().Be("Email");
        result.Score.Should().Be(100);
        result.IsMatch.Should().BeTrue();
        result.MatchingType.Should().Be(MatchTypeEntity.Exact);
    }

    [Fact]
    public void FieldMatchResult_FuzzyMatch_HasCorrectScore()
    {
        // Arrange & Act
        var result = new FieldMatchResult
        {
            FieldName = "CompanyName",
            Value1 = "Acme Corporation",
            Value2 = "Acme Corp",
            MatchingType = MatchTypeEntity.Fuzzy,
            Score = 75,
            IsMatch = true
        };

        // Assert
        result.MatchingType.Should().Be(MatchTypeEntity.Fuzzy);
        result.Score.Should().BeLessThan(100);
        result.IsMatch.Should().BeTrue();
    }

    #endregion

    #region DuplicateMatch Tests

    [Fact]
    public void DuplicateMatch_Create_IsValid()
    {
        // Arrange & Act
        var match = new DuplicateMatch
        {
            RecordId = 1,
            EntityType = "Lead",
            MatchScore = 85,
            FieldComparisons = new Dictionary<string, FieldComparison>
            {
                { "Email", new FieldComparison
                    {
                        FieldName = "Email",
                        NewValue = "john@example.com",
                        ExistingValue = "john@example.com",
                        IsMatch = true,
                        MatchWeight = 100
                    }
                }
            },
            RecordSummary = new RecordSummary
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Assert
        match.Should().NotBeNull();
        match.MatchScore.Should().Be(85);
        match.FieldComparisons.Should().HaveCount(1);
        match.RecordSummary.Should().NotBeNull();
        match.RecordSummary!.FirstName.Should().Be("John");
    }

    #endregion

    #region DuplicateCheckResult Tests

    [Fact]
    public void DuplicateCheckResult_NoDuplicates_IsValid()
    {
        // Arrange & Act
        var result = new DuplicateCheckResult
        {
            Duplicates = new List<DuplicateMatch>(),
            RecordsScanned = 100
        };

        // Assert
        result.HasDuplicates.Should().BeFalse();
        result.Duplicates.Should().BeEmpty();
        result.RecordsScanned.Should().Be(100);
    }

    [Fact]
    public void DuplicateCheckResult_WithDuplicates_IsValid()
    {
        // Arrange & Act
        var result = new DuplicateCheckResult
        {
            Duplicates = new List<DuplicateMatch>
            {
                new DuplicateMatch
                {
                    RecordId = 1,
                    EntityType = "Lead",
                    MatchScore = 90
                },
                new DuplicateMatch
                {
                    RecordId = 2,
                    EntityType = "Lead",
                    MatchScore = 75
                }
            },
            RecordsScanned = 100
        };

        // Assert
        result.HasDuplicates.Should().BeTrue();
        result.Duplicates.Should().HaveCount(2);
        result.Duplicates.Max(m => m.MatchScore).Should().Be(90);
    }

    #endregion

    #region MergeRequest/MergeResult Tests

    [Fact]
    public void MergeRequest_Create_IsValid()
    {
        // Arrange & Act
        var request = new MergeRequest
        {
            EntityType = "Lead",
            MasterRecordId = 1,
            RecordsToMerge = new List<int> { 2, 3, 4 },
            FieldSourceOverrides = new Dictionary<string, int>
            {
                { "Email", 2 },
                { "Phone", 3 }
            },
            RelinkRelatedRecords = true,
            UserId = 1
        };

        // Assert
        request.Should().NotBeNull();
        request.MasterRecordId.Should().Be(1);
        request.RecordsToMerge.Should().HaveCount(3);
        request.FieldSourceOverrides.Should().HaveCount(2);
        request.RelinkRelatedRecords.Should().BeTrue();
    }

    [Fact]
    public void MergeResult_Success_IsValid()
    {
        // Arrange & Act
        var result = new MergeResult
        {
            Success = true,
            MergeGroupId = 1,
            MasterRecordId = 1,
            RecordsMerged = 2,
            RelatedRecordsRelinked = 15
        };

        // Assert
        result.Success.Should().BeTrue();
        result.MergeGroupId.Should().Be(1);
        result.RecordsMerged.Should().Be(2);
        result.RelatedRecordsRelinked.Should().Be(15);
    }

    [Fact]
    public void MergeResult_Failure_IsValid()
    {
        // Arrange & Act
        var result = new MergeResult
        {
            Success = false,
            ErrorMessage = "One or more records not found"
        };

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeEmpty();
    }

    #endregion

    #region UnmergeRequest/UnmergeResult Tests

    [Fact]
    public void UnmergeRequest_Create_IsValid()
    {
        // Arrange & Act
        var request = new UnmergeRequest
        {
            MergeGroupId = 1,
            SpecificRecordsToRestore = new List<int> { 2 },
            RestoreRelatedRecords = true,
            UserId = 1
        };

        // Assert
        request.Should().NotBeNull();
        request.MergeGroupId.Should().Be(1);
        request.SpecificRecordsToRestore.Should().HaveCount(1);
        request.RestoreRelatedRecords.Should().BeTrue();
    }

    [Fact]
    public void UnmergeResult_Success_IsValid()
    {
        // Arrange & Act
        var result = new UnmergeResult
        {
            Success = true,
            RestoredRecordIds = new List<int> { 2 },
            RelatedRecordsRestored = 5
        };

        // Assert
        result.Success.Should().BeTrue();
        result.RestoredRecordIds.Should().HaveCount(1);
        result.RelatedRecordsRestored.Should().Be(5);
    }

    #endregion

    #region Enum Values Tests

    [Fact]
    public void DuplicateEntityType_AllValuesExist()
    {
        // Assert
        Enum.GetValues<DuplicateEntityType>().Should().Contain(DuplicateEntityType.Lead);
        Enum.GetValues<DuplicateEntityType>().Should().Contain(DuplicateEntityType.Account);
        Enum.GetValues<DuplicateEntityType>().Should().Contain(DuplicateEntityType.Contact);
    }

    [Fact]
    public void MatchType_AllValuesExist()
    {
        // Assert - Using Entity MatchType
        Enum.GetValues<MatchTypeEntity>().Should().Contain(MatchTypeEntity.Exact);
        Enum.GetValues<MatchTypeEntity>().Should().Contain(MatchTypeEntity.Fuzzy);
        Enum.GetValues<MatchTypeEntity>().Should().Contain(MatchTypeEntity.Phonetic);
        Enum.GetValues<MatchTypeEntity>().Should().Contain(MatchTypeEntity.Normalized);
    }

    [Fact]
    public void MergeGroupStatus_AllValuesExist()
    {
        // Assert
        Enum.GetValues<MergeGroupStatus>().Should().Contain(MergeGroupStatus.Active);
        Enum.GetValues<MergeGroupStatus>().Should().Contain(MergeGroupStatus.Unmerged);
        Enum.GetValues<MergeGroupStatus>().Should().Contain(MergeGroupStatus.PartialUnmerge);
    }

    [Fact]
    public void DuplicateCandidateStatus_AllValuesExist()
    {
        // Assert
        Enum.GetValues<DuplicateCandidateStatus>().Should().Contain(DuplicateCandidateStatus.Pending);
        Enum.GetValues<DuplicateCandidateStatus>().Should().Contain(DuplicateCandidateStatus.Confirmed);
        Enum.GetValues<DuplicateCandidateStatus>().Should().Contain(DuplicateCandidateStatus.Rejected);
        Enum.GetValues<DuplicateCandidateStatus>().Should().Contain(DuplicateCandidateStatus.Merged);
    }

    #endregion

    #region DuplicateRuleInfo Tests

    [Fact]
    public void DuplicateRuleInfo_Create_IsValid()
    {
        // Arrange & Act
        var ruleInfo = new DuplicateRuleInfo
        {
            Id = 1,
            Name = "Lead Email Match",
            MatchThreshold = 80,
            Action = DuplicateAction.Warn
        };

        // Assert
        ruleInfo.Should().NotBeNull();
        ruleInfo.Id.Should().Be(1);
        ruleInfo.Name.Should().Be("Lead Email Match");
        ruleInfo.MatchThreshold.Should().Be(80);
        ruleInfo.Action.Should().Be(DuplicateAction.Warn);
    }

    #endregion

    #region FieldComparison Tests

    [Fact]
    public void FieldComparison_Create_IsValid()
    {
        // Arrange & Act
        var comparison = new FieldComparison
        {
            FieldName = "Email",
            DisplayName = "Email Address",
            NewValue = "new@example.com",
            ExistingValue = "existing@example.com",
            IsMatch = true,
            MatchWeight = 40,
            MatchType = "Exact",
            SimilarityPercent = 100
        };

        // Assert
        comparison.Should().NotBeNull();
        comparison.FieldName.Should().Be("Email");
        comparison.NewValue.Should().Be("new@example.com");
        comparison.ExistingValue.Should().Be("existing@example.com");
        comparison.IsMatch.Should().BeTrue();
        comparison.MatchWeight.Should().Be(40);
    }

    #endregion

    #region RecordSummary Tests

    [Fact]
    public void RecordSummary_Create_IsValid()
    {
        // Arrange & Act
        var summary = new RecordSummary
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            CompanyName = "Acme Corp",
            Title = "CEO",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        summary.Should().NotBeNull();
        summary.Id.Should().Be(1);
        summary.FirstName.Should().Be("John");
        summary.LastName.Should().Be("Doe");
        summary.Email.Should().Be("john@example.com");
        summary.CompanyName.Should().Be("Acme Corp");
    }

    #endregion

    #region MergePreview Tests

    [Fact]
    public void MergePreview_Create_IsValid()
    {
        // Arrange & Act
        var preview = new MergePreview
        {
            PreviewRecord = new Dictionary<string, object?>
            {
                { "FirstName", "John" },
                { "LastName", "Doe" },
                { "Email", "john@example.com" }
            },
            FieldPreviews = new Dictionary<string, FieldMergePreview>
            {
                { "Email", new FieldMergePreview 
                    { 
                        FieldName = "Email",
                        FinalValue = "john@example.com",
                        SourceRecordId = 1
                    }
                }
            }
        };

        // Assert
        preview.Should().NotBeNull();
        preview.PreviewRecord.Should().HaveCount(3);
        preview.FieldPreviews.Should().HaveCount(1);
    }

    #endregion
}
