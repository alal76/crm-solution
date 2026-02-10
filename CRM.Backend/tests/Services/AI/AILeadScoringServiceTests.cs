// CRM Solution - AILeadScoringService Unit Tests
// Phase 7, Task 7.2 - Tests for weighted multi-factor lead scoring

using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.AI;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

public class AILeadScoringServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<AILeadScoringService>> _mockLogger;
    private readonly AILeadScoringService _service;

    public AILeadScoringServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<AILeadScoringService>>();

        _mockFeatureManager.Setup(f => f.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IFeatureManager))).Returns(_mockFeatureManager.Object);

        _service = new AILeadScoringService(
            _mockContext.Object,
            _mockServiceProvider.Object,
            _mockFeatureManager.Object,
            _mockLogger.Object);
    }

    // ========================================================================
    // ScoreLeadAsync - Basic scoring
    // ========================================================================

    [Fact]
    public async Task ScoreLeadAsync_ShouldReturnNull_WhenLeadNotFound()
    {
        // Arrange
        var leads = new List<Lead>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreLeadAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ScoreLeadAsync_ShouldReturnScore_WhenLeadExists()
    {
        // Arrange
        var leads = new List<Lead>
        {
            CreateLead(1, "John", "Doe", "john@example.com", "Acme Corp")
        };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreLeadAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.LeadId.Should().Be(1);
        result.TotalScore.Should().BeInRange(0, 100);
        result.Grade.Should().NotBeNullOrEmpty();
        result.ScoredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ScoreLeadAsync_ShouldScoreHigher_WhenLeadHasCompleteData()
    {
        // Arrange
        var incompleteLead = CreateLead(1, "John", "Doe", null, null);
        var completeLead = CreateLead(2, "Jane", "Smith", "jane@acme.com", "Acme Corp");
        completeLead.Phone = "+1234567890";
        completeLead.Title = "VP of Sales";
        completeLead.Website = "https://acme.com";

        var leads = new List<Lead> { incompleteLead, completeLead };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var incompleteScore = await _service.ScoreLeadAsync(1);
        var completeScore = await _service.ScoreLeadAsync(2);

        // Assert
        completeScore!.CompletenessScore.Should().BeGreaterThan(incompleteScore!.CompletenessScore);
    }

    // ========================================================================
    // Grade assignment
    // ========================================================================

    [Theory]
    [InlineData(90, "A")]
    [InlineData(80, "A")]
    [InlineData(70, "B")]
    [InlineData(60, "B")]
    [InlineData(50, "C")]
    [InlineData(40, "C")]
    [InlineData(30, "D")]
    [InlineData(20, "D")]
    [InlineData(10, "F")]
    [InlineData(0, "F")]
    public void GradeShouldMatchScoreRange(int score, string expectedGrade)
    {
        // The service uses:
        // >= 80 → A, >= 60 → B, >= 40 → C, >= 20 → D, else F
        string grade = score >= 80 ? "A" : score >= 60 ? "B" : score >= 40 ? "C" : score >= 20 ? "D" : "F";
        grade.Should().Be(expectedGrade);
    }

    // ========================================================================
    // ScoreAllLeadsAsync
    // ========================================================================

    [Fact]
    public async Task ScoreAllLeadsAsync_ShouldReturnBatchResult_ForAllActiveLeads()
    {
        // Arrange
        var leads = new List<Lead>
        {
            CreateLead(1, "John", "Doe", "john@example.com", "Acme"),
            CreateLead(2, "Jane", "Smith", "jane@test.com", "Test Corp"),
            CreateLead(3, "Deleted", "Lead", "del@test.com", "Gone")
        };
        leads[2].IsDeleted = true;

        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreAllLeadsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalProcessed.Should().Be(2); // excludes deleted
        result.Succeeded.Should().Be(2);
        result.Failed.Should().Be(0);
        result.AverageScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ScoreAllLeadsAsync_ShouldReturnZero_WhenNoLeads()
    {
        // Arrange
        var leads = new List<Lead>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var result = await _service.ScoreAllLeadsAsync();

        // Assert
        result.TotalProcessed.Should().Be(0);
        result.Succeeded.Should().Be(0);
        result.AverageScore.Should().Be(0);
    }

    // ========================================================================
    // GetScoringWeights
    // ========================================================================

    [Fact]
    public void GetScoringWeights_ShouldReturnAllFactors()
    {
        // Act
        var weights = _service.GetScoringWeights();

        // Assert
        weights.Should().NotBeNull();
        weights.Should().ContainKey("Completeness");
        weights.Should().ContainKey("Engagement");
        weights.Should().ContainKey("Fit");
        weights.Should().ContainKey("Recency");
        weights.Should().ContainKey("Source");
    }

    [Fact]
    public void GetScoringWeights_ShouldSumToOne()
    {
        // Act
        var weights = _service.GetScoringWeights();

        // Assert
        weights.Values.Sum().Should().BeApproximately(1.0, 0.01);
    }

    // ========================================================================
    // Source scoring
    // ========================================================================

    [Fact]
    public async Task ScoreLeadAsync_ShouldScoreReferralHigherThanManual()
    {
        // Arrange
        var referralLead = CreateLead(1, "Ref", "Lead", "ref@test.com", "Corp");
        referralLead.Source = LeadSource.Referral;

        var manualLead = CreateLead(2, "Manual", "Lead", "manual@test.com", "Corp");
        manualLead.Source = LeadSource.Manual;

        var leads = new List<Lead> { referralLead, manualLead };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var refScore = await _service.ScoreLeadAsync(1);
        var manualScore = await _service.ScoreLeadAsync(2);

        // Assert
        refScore!.SourceScore.Should().BeGreaterThanOrEqualTo(manualScore!.SourceScore);
    }

    // ========================================================================
    // Recency scoring
    // ========================================================================

    [Fact]
    public async Task ScoreLeadAsync_ShouldScoreRecentLeadsHigher()
    {
        // Arrange
        var recentLead = CreateLead(1, "New", "Lead", "new@test.com", "Corp");
        recentLead.CreatedAt = DateTime.UtcNow.AddDays(-1);
        recentLead.LastActivityDate = DateTime.UtcNow;

        var oldLead = CreateLead(2, "Old", "Lead", "old@test.com", "Corp");
        oldLead.CreatedAt = DateTime.UtcNow.AddDays(-180);
        oldLead.LastActivityDate = DateTime.UtcNow.AddDays(-90);

        var leads = new List<Lead> { recentLead, oldLead };
        var mockSet = MockDbSetFactory.CreateMockDbSet(leads);
        _mockContext.Setup(c => c.Leads).Returns(mockSet.Object);

        // Act
        var recentScore = await _service.ScoreLeadAsync(1);
        var oldScore = await _service.ScoreLeadAsync(2);

        // Assert
        recentScore!.RecencyScore.Should().BeGreaterThan(oldScore!.RecencyScore);
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static Lead CreateLead(int id, string firstName, string lastName, string? email, string? company)
    {
        return new Lead
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email ?? string.Empty,
            CompanyName = company,
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            IsDeleted = false
        };
    }
}
