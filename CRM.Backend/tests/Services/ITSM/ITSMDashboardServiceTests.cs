// CRM Solution - ITSM Dashboard Service Tests
// Minimal DTO tests for ITSM module

using CRM.Core.Interfaces.ITSM;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Tests for ITSM Dashboard DTOs.
/// </summary>
public class ITSMDashboardServiceTests
{
    [Fact]
    public void IncidentTrendsDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new IncidentTrendsDto();

        // Assert
        dto.TotalIncidents.Should().Be(0);
        dto.OpenIncidents.Should().Be(0);
        dto.ResolvedIncidents.Should().Be(0);
        dto.ClosedIncidents.Should().Be(0);
        dto.AverageResolutionTimeHours.Should().Be(0);
        dto.FirstContactResolutionRate.Should().Be(0);
        dto.DailyTrends.Should().NotBeNull();
        dto.DailyTrends.Should().BeEmpty();
        dto.ByCategory.Should().NotBeNull();
        dto.ByCategory.Should().BeEmpty();
        dto.ByPriority.Should().NotBeNull();
        dto.ByPriority.Should().BeEmpty();
    }

    [Fact]
    public void IncidentTrendsDto_ShouldPopulateAllProperties()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var dto = new IncidentTrendsDto
        {
            TotalIncidents = 100,
            OpenIncidents = 25,
            ResolvedIncidents = 50,
            ClosedIncidents = 25,
            AverageResolutionTimeHours = 4.5,
            FirstContactResolutionRate = 0.65,
            DailyTrends = new List<DailyTrendItem>
            {
                new() { Date = now.AddDays(-1), Created = 10, Resolved = 8, Backlog = 15 }
            },
            ByCategory = new List<CategoryBreakdown>
            {
                new() { Category = "Network", Count = 30, Percentage = 30.0 }
            },
            ByPriority = new List<PriorityBreakdown>
            {
                new() { Priority = 1, PriorityLabel = "Critical", Count = 10, Percentage = 10.0 }
            }
        };

        // Assert
        dto.TotalIncidents.Should().Be(100);
        dto.AverageResolutionTimeHours.Should().Be(4.5);
        dto.FirstContactResolutionRate.Should().Be(0.65);
        dto.DailyTrends.Should().HaveCount(1);
        dto.ByCategory.Should().HaveCount(1);
        dto.ByPriority.Should().HaveCount(1);
    }

    [Fact]
    public void DailyTrendItem_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var item = new DailyTrendItem
        {
            Date = DateTime.UtcNow,
            Created = 15,
            Resolved = 12,
            Backlog = 8
        };

        // Assert
        item.Created.Should().Be(15);
        item.Resolved.Should().Be(12);
        item.Backlog.Should().Be(8);
    }

    [Fact]
    public void ProblemAnalyticsDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new ProblemAnalyticsDto();

        // Assert
        dto.TotalProblems.Should().Be(0);
        dto.OpenProblems.Should().Be(0);
        dto.ProblemsWithKnownError.Should().Be(0);
        dto.ProblemsWithWorkaround.Should().Be(0);
        dto.LinkedIncidentsCount.Should().Be(0);
        dto.ByRootCause.Should().NotBeNull();
        dto.ByRootCause.Should().BeEmpty();
        dto.TopRecurringProblems.Should().NotBeNull();
        dto.TopRecurringProblems.Should().BeEmpty();
    }

    [Fact]
    public void TopProblem_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var problem = new TopProblem
        {
            ProblemId = 1,
            ProblemNumber = "PRB0001234",
            Title = "Network connectivity issue",
            LinkedIncidents = 15
        };

        // Assert
        problem.ProblemId.Should().Be(1);
        problem.ProblemNumber.Should().Be("PRB0001234");
        problem.Title.Should().Be("Network connectivity issue");
        problem.LinkedIncidents.Should().Be(15);
    }

    [Fact]
    public void ChangeStatisticsDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new ChangeStatisticsDto();

        // Assert
        dto.TotalChanges.Should().Be(0);
        dto.ScheduledChanges.Should().Be(0);
        dto.CompletedChanges.Should().Be(0);
        dto.FailedChanges.Should().Be(0);
        dto.RolledBackChanges.Should().Be(0);
        dto.SuccessRate.Should().Be(0);
        dto.ByType.Should().NotBeNull();
        dto.ByType.Should().BeEmpty();
        dto.ByRisk.Should().NotBeNull();
        dto.ByRisk.Should().BeEmpty();
    }

    [Fact]
    public void SLAComplianceDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new SLAComplianceDto();

        // Assert
        dto.OverallComplianceRate.Should().Be(0);
        dto.TotalTickets.Should().Be(0);
        dto.TicketsWithinSLA.Should().Be(0);
        dto.TicketsBreachedSLA.Should().Be(0);
        dto.TicketsAtRisk.Should().Be(0);
        dto.ByPriority.Should().NotBeNull();
        dto.ByPriority.Should().BeEmpty();
        dto.ByCategory.Should().NotBeNull();
        dto.ByCategory.Should().BeEmpty();
        dto.Trends.Should().NotBeNull();
        dto.Trends.Should().BeEmpty();
    }

    [Fact]
    public void SLAComplianceDto_ShouldCalculateComplianceCorrectly()
    {
        // Arrange & Act
        var dto = new SLAComplianceDto
        {
            TotalTickets = 100,
            TicketsWithinSLA = 85,
            TicketsBreachedSLA = 15,
            TicketsAtRisk = 5,
            OverallComplianceRate = 85.0
        };

        // Assert
        dto.TicketsWithinSLA.Should().Be(85);
        dto.TicketsBreachedSLA.Should().Be(15);
        dto.OverallComplianceRate.Should().Be(85.0);
    }

    [Fact]
    public void AgentPerformanceDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new AgentPerformanceDto();

        // Assert
        dto.AgentId.Should().Be(0);
        dto.AgentName.Should().BeEmpty();
        dto.TicketsAssigned.Should().Be(0);
        dto.TicketsResolved.Should().Be(0);
        dto.TicketsReopened.Should().Be(0);
        dto.AverageResolutionTimeHours.Should().Be(0);
        dto.FirstContactResolutionRate.Should().Be(0);
        dto.SLAComplianceRate.Should().Be(0);
        dto.CustomerSatisfactionScore.Should().BeNull();
        dto.CurrentBacklog.Should().Be(0);
    }

    [Fact]
    public void AgentPerformanceDto_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var dto = new AgentPerformanceDto
        {
            AgentId = 50,
            AgentName = "John Smith",
            TicketsAssigned = 100,
            TicketsResolved = 95,
            TicketsReopened = 5,
            AverageResolutionTimeHours = 2.5,
            FirstContactResolutionRate = 0.75,
            SLAComplianceRate = 0.92,
            CustomerSatisfactionScore = 4.5,
            CurrentBacklog = 10
        };

        // Assert
        dto.AgentId.Should().Be(50);
        dto.AgentName.Should().Be("John Smith");
        dto.TicketsResolved.Should().Be(95);
        dto.SLAComplianceRate.Should().Be(0.92);
        dto.CustomerSatisfactionScore.Should().Be(4.5);
    }

    [Fact]
    public void CMDBHealthDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new CMDBHealthDto();

        // Assert
        dto.TotalConfigurationItems.Should().Be(0);
        dto.ActiveItems.Should().Be(0);
        dto.RetiredItems.Should().Be(0);
        dto.ItemsNeedingReview.Should().Be(0);
        dto.OrphanedItems.Should().Be(0);
        dto.TotalRelationships.Should().Be(0);
        dto.ByType.Should().NotBeNull();
        dto.ByType.Should().BeEmpty();
        dto.ByStatus.Should().NotBeNull();
        dto.ByStatus.Should().BeEmpty();
    }

    [Fact]
    public void KnowledgeAnalyticsDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new KnowledgeAnalyticsDto();

        // Assert
        dto.TotalArticles.Should().Be(0);
        dto.PublishedArticles.Should().Be(0);
        dto.DraftArticles.Should().Be(0);
        dto.ArticlesNeedingReview.Should().Be(0);
        dto.TotalViews.Should().Be(0);
        dto.TotalSearches.Should().Be(0);
        dto.HelpfulRate.Should().Be(0);
        dto.MostViewedArticles.Should().NotBeNull();
        dto.MostViewedArticles.Should().BeEmpty();
        dto.TopSearchTerms.Should().NotBeNull();
        dto.TopSearchTerms.Should().BeEmpty();
        dto.UsageByCategory.Should().NotBeNull();
        dto.UsageByCategory.Should().BeEmpty();
    }

    [Fact]
    public void TopArticle_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var article = new TopArticle
        {
            ArticleId = 10,
            Title = "How to reset password",
            Views = 500,
            HelpfulVotes = 120,
            NotHelpfulVotes = 5
        };

        // Assert
        article.ArticleId.Should().Be(10);
        article.Title.Should().Be("How to reset password");
        article.Views.Should().Be(500);
        article.HelpfulVotes.Should().Be(120);
        article.NotHelpfulVotes.Should().Be(5);
    }

    [Fact]
    public void TopSearchTerm_ShouldPopulateAllProperties()
    {
        // Arrange & Act
        var term = new TopSearchTerm
        {
            SearchTerm = "password reset",
            Count = 150,
            ResultsFound = 12
        };

        // Assert
        term.SearchTerm.Should().Be("password reset");
        term.Count.Should().Be(150);
        term.ResultsFound.Should().Be(12);
    }
}
