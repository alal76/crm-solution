// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Lead Entity Unit Tests

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.Services;

/// <summary>
/// Comprehensive unit tests for Lead entity functionality
/// </summary>
public class LeadServiceTests
{
    #region Create Lead Tests

    [Fact]
    public void CreateLead_Valid_CreatesCorrectly()
    {
        // Arrange & Act
        var lead = new Lead
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@example.com",
            CompanyName = "Johnson Inc",
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web
        };

        // Assert
        lead.Should().NotBeNull();
        lead.FirstName.Should().Be("Alice");
        lead.Source.Should().Be(LeadSource.Web);
    }

    [Fact]
    public void CreateLead_WithAllFields_SetsCorrectly()
    {
        // Arrange & Act
        var lead = new Lead
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234",
            CompanyName = "Doe Industries",
            Title = "CEO",
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Referral,
            Score = 85,
            FitScore = 45,
            EngagementScore = 40,
            QualificationNotes = "Important lead"
        };

        // Assert
        lead.FirstName.Should().Be("John");
        lead.Title.Should().Be("CEO");
        lead.Score.Should().Be(85);
        lead.Source.Should().Be(LeadSource.Referral);
    }

    #endregion

    #region Lead Status Progression Tests

    [Fact]
    public void Lead_NewToWorking_StatusProgression()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.New };

        // Act
        lead.Status = LeadLifecycleStatus.Working;

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.Working);
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_WorkingToQualified_StatusProgression()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.Working };

        // Act
        lead.Status = LeadLifecycleStatus.Qualified;

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
        lead.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void Lead_QualifiedToConverted_StatusProgression()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.Qualified };

        // Act
        lead.Status = LeadLifecycleStatus.Converted;

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.Converted);
        lead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void Lead_Disqualified_ClosesLead()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.New };

        // Act
        lead.Status = LeadLifecycleStatus.Disqualified;

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.Disqualified);
        lead.IsOpen.Should().BeFalse();
    }

    [Theory]
    [InlineData(LeadLifecycleStatus.New, true)]
    [InlineData(LeadLifecycleStatus.Working, true)]
    [InlineData(LeadLifecycleStatus.Qualified, true)]
    [InlineData(LeadLifecycleStatus.Nurturing, true)]
    [InlineData(LeadLifecycleStatus.Converted, false)]
    [InlineData(LeadLifecycleStatus.Disqualified, false)]
    public void Lead_IsOpen_CorrectForEachStatus(LeadLifecycleStatus status, bool expectedIsOpen)
    {
        // Arrange
        var lead = new Lead { Status = status };

        // Assert
        lead.IsOpen.Should().Be(expectedIsOpen);
    }

    #endregion

    #region Lead Source Tests

    [Theory]
    [InlineData(LeadSource.Web)]
    [InlineData(LeadSource.Campaign)]
    [InlineData(LeadSource.Referral)]
    [InlineData(LeadSource.Event)]
    [InlineData(LeadSource.Partner)]
    [InlineData(LeadSource.Manual)]
    public void LeadSource_AllSourcesValid(LeadSource source)
    {
        // Arrange
        var lead = new Lead();

        // Act
        lead.Source = source;

        // Assert
        lead.Source.Should().Be(source);
    }

    #endregion

    #region Lead Scoring Tests

    [Fact]
    public void Lead_Score_DefaultIsZero()
    {
        // Arrange & Act
        var lead = new Lead();

        // Assert
        lead.Score.Should().Be(0);
    }

    [Fact]
    public void Lead_Score_CanBeSet()
    {
        // Arrange
        var lead = new Lead();

        // Act
        lead.Score = 75;

        // Assert
        lead.Score.Should().Be(75);
    }

    [Fact]
    public void Lead_FitScore_CanBeSet()
    {
        // Arrange
        var lead = new Lead();

        // Act
        lead.FitScore = 40;

        // Assert
        lead.FitScore.Should().Be(40);
    }

    [Fact]
    public void Lead_EngagementScore_CanBeSet()
    {
        // Arrange
        var lead = new Lead();

        // Act
        lead.EngagementScore = 60;

        // Assert
        lead.EngagementScore.Should().Be(60);
    }

    [Fact]
    public void Lead_HighScore_IndicatesQuality()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Score = 90 },
            new() { Id = 2, Score = 60 },
            new() { Id = 3, Score = 30 }
        };

        // Act
        var highQuality = leads.Where(l => l.Score >= 80).ToList();
        var mediumQuality = leads.Where(l => l.Score >= 50 && l.Score < 80).ToList();
        var lowQuality = leads.Where(l => l.Score < 50).ToList();

        // Assert
        highQuality.Should().HaveCount(1);
        mediumQuality.Should().HaveCount(1);
        lowQuality.Should().HaveCount(1);
    }

    #endregion

    #region FullName Computed Property Tests

    [Fact]
    public void Lead_FullName_CombinesFirstAndLast()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        lead.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void Lead_FullName_WithOnlyFirstName()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "John",
            LastName = null
        };

        // Assert
        lead.FullName.Should().Contain("John");
    }

    #endregion

    #region Search and Filter Tests

    [Fact]
    public void SearchLeads_ByName_ReturnsMatching()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe" },
            new() { Id = 2, FirstName = "Jane", LastName = "Doe" },
            new() { Id = 3, FirstName = "Bob", LastName = "Smith" }
        };

        // Act
        var searchResult = leads.Where(l => 
            l.LastName?.Contains("Doe", StringComparison.OrdinalIgnoreCase) ?? false);

        // Assert
        searchResult.Should().HaveCount(2);
    }

    [Fact]
    public void FilterLeads_ByStatus_ReturnsMatching()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Status = LeadLifecycleStatus.New },
            new() { Id = 2, Status = LeadLifecycleStatus.Working },
            new() { Id = 3, Status = LeadLifecycleStatus.New }
        };

        // Act
        var newLeads = leads.Where(l => l.Status == LeadLifecycleStatus.New);

        // Assert
        newLeads.Should().HaveCount(2);
    }

    [Fact]
    public void FilterLeads_BySource_ReturnsMatching()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Source = LeadSource.Web },
            new() { Id = 2, Source = LeadSource.Referral },
            new() { Id = 3, Source = LeadSource.Web }
        };

        // Act
        var webLeads = leads.Where(l => l.Source == LeadSource.Web);

        // Assert
        webLeads.Should().HaveCount(2);
    }

    [Fact]
    public void FilterLeads_OpenOnly_ReturnsMatching()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Status = LeadLifecycleStatus.New },
            new() { Id = 2, Status = LeadLifecycleStatus.Converted },
            new() { Id = 3, Status = LeadLifecycleStatus.Working }
        };

        // Act
        var openLeads = leads.Where(l => l.IsOpen);

        // Assert
        openLeads.Should().HaveCount(2);
    }

    [Fact]
    public void FilterLeads_ByMinScore_ReturnsMatching()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Score = 90 },
            new() { Id = 2, Score = 50 },
            new() { Id = 3, Score = 20 }
        };

        // Act
        var hotLeads = leads.Where(l => l.Score >= 70);

        // Assert
        hotLeads.Should().HaveCount(1);
    }

    #endregion

    #region Update Lead Tests

    [Fact]
    public void UpdateLead_ChangeStatus_UpdatesCorrectly()
    {
        // Arrange
        var lead = new Lead
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Status = LeadLifecycleStatus.New
        };

        // Act
        lead.Status = LeadLifecycleStatus.Working;

        // Assert
        lead.Status.Should().Be(LeadLifecycleStatus.Working);
    }

    [Fact]
    public void UpdateLead_ChangeScore_UpdatesCorrectly()
    {
        // Arrange
        var lead = new Lead { Id = 1, Score = 50 };

        // Act
        lead.Score = 85;

        // Assert
        lead.Score.Should().Be(85);
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDeleteLead_SetsIsDeletedFlag()
    {
        // Arrange
        var lead = new Lead { Id = 1, IsDeleted = false };

        // Act
        lead.IsDeleted = true;

        // Assert
        lead.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Lead Conversion Tests

    [Fact]
    public void Lead_Conversion_ToCustomer_Workflow()
    {
        // Arrange - Qualified lead ready for conversion
        var lead = new Lead
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CompanyName = "Doe Industries",
            Status = LeadLifecycleStatus.Qualified
        };

        // Act - Convert to customer
        lead.Status = LeadLifecycleStatus.Converted;

        var customer = new Customer
        {
            FirstName = lead.FirstName,
            LastName = lead.LastName,
            Email = lead.Email,
            Company = lead.CompanyName,
            Category = CustomerCategory.Organization
        };

        // Assert
        lead.IsOpen.Should().BeFalse();
        customer.FirstName.Should().Be(lead.FirstName);
        customer.Company.Should().Be(lead.CompanyName);
    }

    [Fact]
    public void Lead_Conversion_ToOpportunity_Workflow()
    {
        // Arrange - Qualified lead
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            CompanyName = "Smith Corp",
            Status = LeadLifecycleStatus.Qualified
        };

        // Act - Convert lead and create opportunity
        lead.Status = LeadLifecycleStatus.Converted;

        var opportunity = new Opportunity
        {
            Name = $"{lead.CompanyName} - New Opportunity",
            LeadId = lead.Id,
            Stage = OpportunityStage.Discovery
        };

        // Assert
        lead.IsOpen.Should().BeFalse();
        opportunity.LeadId.Should().Be(lead.Id);
        opportunity.Stage.Should().Be(OpportunityStage.Discovery);
    }

    #endregion

    #region Edge Cases and Validation

    [Fact]
    public void Lead_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var lead = new Lead();

        // Assert
        lead.Id.Should().Be(0);
        lead.IsDeleted.Should().BeFalse();
        lead.Score.Should().Be(0);
    }

    [Fact]
    public void Lead_WithNullEmail_Allowed()
    {
        // Arrange & Act
        var lead = new Lead
        {
            FirstName = "Test",
            Email = null
        };

        // Assert
        lead.Email.Should().BeNull();
    }

    [Fact]
    public void Lead_Timestamps_Work()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var lead = new Lead
        {
            CreatedAt = now,
            UpdatedAt = now
        };

        // Assert
        lead.CreatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        lead.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region Pagination and Sorting Tests

    [Fact]
    public void GetLeads_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var leads = Enumerable.Range(1, 100)
            .Select(i => new Lead { Id = i, FirstName = $"Lead{i}" })
            .ToList();

        // Act
        var page1 = leads.Skip(0).Take(10).ToList();
        var page2 = leads.Skip(10).Take(10).ToList();

        // Assert
        page1.Should().HaveCount(10);
        page1.First().Id.Should().Be(1);
        page2.Should().HaveCount(10);
        page2.First().Id.Should().Be(11);
    }

    [Fact]
    public void GetLeads_SortByScore_ReturnsSorted()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Score = 50 },
            new() { Id = 2, Score = 90 },
            new() { Id = 3, Score = 30 }
        };

        // Act
        var sorted = leads.OrderByDescending(l => l.Score).ToList();

        // Assert
        sorted[0].Score.Should().Be(90);
        sorted[1].Score.Should().Be(50);
        sorted[2].Score.Should().Be(30);
    }

    #endregion

    #region Lead Pipeline Tests

    [Fact]
    public void LeadPipeline_GroupByStatus_Works()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Status = LeadLifecycleStatus.New },
            new() { Id = 2, Status = LeadLifecycleStatus.New },
            new() { Id = 3, Status = LeadLifecycleStatus.Working },
            new() { Id = 4, Status = LeadLifecycleStatus.Qualified },
            new() { Id = 5, Status = LeadLifecycleStatus.Converted }
        };

        // Act
        var grouped = leads.GroupBy(l => l.Status).ToList();

        // Assert
        grouped.Should().HaveCount(4);
        grouped.Single(g => g.Key == LeadLifecycleStatus.New).Should().HaveCount(2);
    }

    [Fact]
    public void LeadPipeline_ConversionRate_Calculated()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Id = 1, Status = LeadLifecycleStatus.New },
            new() { Id = 2, Status = LeadLifecycleStatus.Working },
            new() { Id = 3, Status = LeadLifecycleStatus.Converted },
            new() { Id = 4, Status = LeadLifecycleStatus.Converted }
        };

        // Act
        var total = leads.Count();
        var converted = leads.Count(l => l.Status == LeadLifecycleStatus.Converted);
        var conversionRate = (decimal)converted / total * 100;

        // Assert
        conversionRate.Should().Be(50m);
    }

    #endregion
}
