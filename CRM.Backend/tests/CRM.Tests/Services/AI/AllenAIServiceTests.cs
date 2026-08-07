// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;
using System.Text;
using CRM.Core.Entities;
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for AllenAIService (TCOV-001).
/// Uses InMemory CrmDbContext because AllenAIService takes the concrete CrmDbContext.
/// </summary>
public class AllenAIServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<AllenAIService>> _mockLogger;
    private readonly IOptions<AllenAIConfiguration> _options;
    private readonly IMemoryCache _memoryCache;
    private readonly AllenAIService _service;

    public AllenAIServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var mockConfig = new Mock<IConfiguration>();
        _dbContext = new CrmDbContext(dbOptions, mockConfig.Object);

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<AllenAIService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _options = Options.Create(new AllenAIConfiguration
        {
            OLMoEndpoint = string.Empty,
            TuluEndpoint = string.Empty,
            BatchSize = 10
        });
        _service = new AllenAIService(
            _dbContext,
            _mockHttpClientFactory.Object,
            _mockLogger.Object,
            _options,
            _memoryCache);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTopLeadsAsync_ShouldReturnEmpty_WhenNoLeadScoresExist()
    {
        var result = await _service.GetTopLeadsAsync(10);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnEmpty_WhenNoModelsExist()
    {
        var result = await _service.GetAvailableModelsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BatchScoreLeadsAsync_ShouldReturnEmpty_WhenLeadListIsEmpty()
    {
        var result = await _service.BatchScoreLeadsAsync(new List<int>());

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ScoreLeadAsync_ShouldThrow_WhenLeadDoesNotExist()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => _service.ScoreLeadAsync(999));
    }

    [Fact]
    public async Task CheckModelHealthAsync_ShouldReturnFalse_WhenOLMoEndpointIsEmpty()
    {
        var result = await _service.CheckModelHealthAsync(AIProvider.AllenAI_OLMo);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckModelHealthAsync_ShouldReturnFalse_WhenProviderNotSupported()
    {
        // AIProvider.OpenAI (4) maps to null endpoint → returns false immediately
        var result = await _service.CheckModelHealthAsync(AIProvider.OpenAI);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnActiveModels_WhenModelsExist()
    {
        _dbContext.AIModels.Add(new AIModel
        {
            Name = "Test Model",
            Provider = AIProvider.AllenAI_OLMo,
            Status = AIModelStatus.Active,
            IsDeleted = false,
            Description = "Test"
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAvailableModelsAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Test Model");
    }

    // ─── Helpers for HTTP-backed tests ─────────────────────────────────────────

    /// <summary>
    /// Builds an isolated AllenAIService instance backed by its own InMemory
    /// database and a fake HttpMessageHandler so tests can control the
    /// simulated AI provider response without any real network call.
    /// </summary>
    private static (AllenAIService Service, CrmDbContext DbContext) CreateServiceWithHandler(
        HttpMessageHandler handler,
        AllenAIConfiguration? configuration = null)
    {
        var dbOptions = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var mockConfig = new Mock<IConfiguration>();
        var dbContext = new CrmDbContext(dbOptions, mockConfig.Object);

        var httpClient = new HttpClient(handler);
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient("AllenAI")).Returns(httpClient);

        var config = configuration ?? new AllenAIConfiguration
        {
            OLMoEndpoint = "https://fake-olmo.test/model",
            TuluEndpoint = "https://fake-tulu.test/model",
            EnableLocalFallback = true,
            BatchSize = 10
        };

        var service = new AllenAIService(
            dbContext,
            factoryMock.Object,
            Mock.Of<ILogger<AllenAIService>>(),
            Options.Create(config),
            new MemoryCache(new MemoryCacheOptions()));

        return (service, dbContext);
    }

    private static string HuggingFaceResponse(string generatedText)
    {
        // Mirrors the real HuggingFace inference API response shape that
        // ParseHuggingFaceResponse expects: a JSON array with generated_text.
        var escaped = generatedText.Replace("\"", "\\\"").Replace("\n", "\\n");
        return $"[{{\"generated_text\": \"{escaped}\"}}]";
    }

    #region GenerateOpportunityInsightAsync / PredictWinProbabilityAsync / GetAtRiskOpportunitiesAsync

    [Fact]
    public async Task GenerateOpportunityInsightAsync_ExistingOpportunity_ReturnsAndPersistsInsight()
    {
        // Opportunity.AccountId is a required (non-nullable) FK, and the service
        // Includes the Account navigation, which EF Core translates to an inner
        // join. A real Account row is required or the Opportunity would vanish
        // from the query results entirely.
        var account = new Account { Category = AccountCategory.Organization, Company = "Big Deal Co" };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var opportunity = new Opportunity
        {
            Name = "Big Deal",
            Stage = OpportunityStage.Proposal,
            Amount = 50000m,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(20),
            AccountId = account.Id,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        var insight = await _service.GenerateOpportunityInsightAsync(opportunity.Id);

        insight.Should().NotBeNull();
        insight.OpportunityId.Should().Be(opportunity.Id);
        insight.WinProbability.Should().BeInRange(0m, 1m);
        insight.PredictedValue.Should().Be(50000m);
        insight.WeightedValue.Should().Be(50000m * insight.WinProbability);

        var persisted = await _dbContext.OpportunityInsights.FirstOrDefaultAsync(i => i.OpportunityId == opportunity.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateOpportunityInsightAsync_OpportunityNotFound_ThrowsArgumentException()
    {
        var act = () => _service.GenerateOpportunityInsightAsync(99999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task PredictWinProbabilityAsync_ExistingOpportunity_ReturnsStageBasedProbability()
    {
        var opportunity = new Opportunity
        {
            Name = "Negotiation Deal",
            Stage = OpportunityStage.Negotiation,
            AccountId = 0,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Opportunities.Add(opportunity);
        await _dbContext.SaveChangesAsync();

        var probability = await _service.PredictWinProbabilityAsync(opportunity.Id);

        // Negotiation stage factor is 0.75, time factor 1.0 (created "now")
        probability.Should().Be(0.75m);
    }

    [Fact]
    public async Task PredictWinProbabilityAsync_OpportunityNotFound_ReturnsZero()
    {
        var probability = await _service.PredictWinProbabilityAsync(99999);

        probability.Should().Be(0m);
    }

    [Fact]
    public async Task GetAtRiskOpportunitiesAsync_ReturnsOnlyNonExpiredAtRiskInsights()
    {
        // GetAtRiskOpportunitiesAsync Includes the Opportunity navigation, which
        // EF Core translates into an inner join since OpportunityId is a required
        // FK. Real Opportunity rows are needed or the insights would vanish.
        var opportunities = new[]
        {
            new Opportunity { Name = "Opp 1", AccountId = 0 },
            new Opportunity { Name = "Opp 2", AccountId = 0 },
            new Opportunity { Name = "Opp 3", AccountId = 0 }
        };
        _dbContext.Opportunities.AddRange(opportunities);
        await _dbContext.SaveChangesAsync();

        _dbContext.OpportunityInsights.AddRange(
            new OpportunityInsight
            {
                OpportunityId = opportunities[0].Id,
                HealthStatus = DealHealthStatus.AtRisk,
                HealthScore = 45m,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new OpportunityInsight
            {
                OpportunityId = opportunities[1].Id,
                HealthStatus = DealHealthStatus.Healthy,
                HealthScore = 90m,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new OpportunityInsight
            {
                OpportunityId = opportunities[2].Id,
                HealthStatus = DealHealthStatus.Critical,
                HealthScore = 5m,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
                IsDeleted = false
            });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAtRiskOpportunitiesAsync();

        result.Should().HaveCount(1);
        result[0].OpportunityId.Should().Be(opportunities[0].Id);
    }

    #endregion

    #region CalculateChurnRiskAsync / GetHighChurnRiskAccountsAsync

    [Fact]
    public async Task CalculateChurnRiskAsync_ExistingAccount_ReturnsAndPersistsChurnRisk()
    {
        var account = new Account
        {
            Category = AccountCategory.Organization,
            Company = "Acme Corp",
            LifecycleStage = AccountLifecycleStage.AtRisk,
            CreatedAt = DateTime.UtcNow.AddMonths(-2)
        };
        _dbContext.Accounts.Add(account);
        await _dbContext.SaveChangesAsync();

        var churnRisk = await _service.CalculateChurnRiskAsync(account.Id);

        churnRisk.Should().NotBeNull();
        churnRisk.AccountId.Should().Be(account.Id);
        churnRisk.ChurnProbability.Should().BeInRange(0m, 1m);

        var persisted = await _dbContext.ChurnRisks.FirstOrDefaultAsync(c => c.AccountId == account.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task CalculateChurnRiskAsync_AccountNotFound_ThrowsArgumentException()
    {
        var act = () => _service.CalculateChurnRiskAsync(99999);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetHighChurnRiskAccountsAsync_ReturnsOnlyHighRiskNonExpired()
    {
        // GetHighChurnRiskAccountsAsync Includes the Account navigation, which
        // EF Core translates into an inner join since AccountId is a required
        // FK. Real Account rows are needed or the churn risks would vanish.
        var accounts = new[]
        {
            new Account { Category = AccountCategory.Organization, Company = "Acct 1" },
            new Account { Category = AccountCategory.Organization, Company = "Acct 2" },
            new Account { Category = AccountCategory.Organization, Company = "Acct 3" }
        };
        _dbContext.Accounts.AddRange(accounts);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChurnRisks.AddRange(
            new ChurnRisk
            {
                AccountId = accounts[0].Id,
                ChurnProbability = 0.9m,
                RiskLevel = ChurnRiskLevel.Critical,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new ChurnRisk
            {
                AccountId = accounts[1].Id,
                ChurnProbability = 0.1m,
                RiskLevel = ChurnRiskLevel.VeryLow,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new ChurnRisk
            {
                AccountId = accounts[2].Id,
                ChurnProbability = 0.95m,
                RiskLevel = ChurnRiskLevel.Critical,
                ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
                IsDeleted = false
            });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetHighChurnRiskAccountsAsync(10);

        result.Should().HaveCount(1);
        result[0].AccountId.Should().Be(accounts[0].Id);
    }

    #endregion

    #region GetRecommendedActionsAsync / GetUserRecommendationsAsync

    [Fact]
    public async Task GetRecommendedActionsAsync_ExistingPendingRecommendations_ReturnsExistingWithoutGenerating()
    {
        _dbContext.ActionRecommendations.Add(new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = 1,
            TargetEntityName = "Existing Lead",
            Status = ActionRecommendationStatus.Pending,
            ImpactScore = 50,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetRecommendedActionsAsync(ActionTargetType.Lead, 1);

        result.Should().HaveCount(1);
        result[0].TargetEntityName.Should().Be("Existing Lead");
    }

    [Fact]
    public async Task GetRecommendedActionsAsync_NoExisting_GeneratesAndPersistsRecommendationForLead()
    {
        var lead = new Lead
        {
            FirstName = "Jane",
            LastName = "Doe",
            Status = LeadLifecycleStatus.New
        };
        _dbContext.Leads.Add(lead);
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetRecommendedActionsAsync(ActionTargetType.Lead, lead.Id);

        result.Should().HaveCount(1);
        result[0].TargetType.Should().Be(ActionTargetType.Lead);
        result[0].TargetEntityId.Should().Be(lead.Id);
        result[0].ActionType.Should().Be(NextBestActionType.Call);

        var persisted = await _dbContext.ActionRecommendations.CountAsync(a => a.TargetEntityId == lead.Id);
        persisted.Should().Be(1);
    }

    [Fact]
    public async Task GetRecommendedActionsAsync_EntityNotFound_ReturnsEmptyAndDoesNotPersist()
    {
        var result = await _service.GetRecommendedActionsAsync(ActionTargetType.Lead, 99999);

        result.Should().BeEmpty();
        (await _dbContext.ActionRecommendations.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetUserRecommendationsAsync_ReturnsAssignedAndUnassignedPendingActions()
    {
        _dbContext.ActionRecommendations.AddRange(
            new ActionRecommendation
            {
                TargetType = ActionTargetType.Lead,
                TargetEntityId = 1,
                AssignedUserId = 42,
                Status = ActionRecommendationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new ActionRecommendation
            {
                TargetType = ActionTargetType.Opportunity,
                TargetEntityId = 2,
                AssignedUserId = null, // unassigned, should still match
                Status = ActionRecommendationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            },
            new ActionRecommendation
            {
                TargetType = ActionTargetType.Lead,
                TargetEntityId = 3,
                AssignedUserId = 7, // different user, should be excluded
                Status = ActionRecommendationStatus.Pending,
                ExpiresAt = DateTime.UtcNow.AddDays(1),
                IsDeleted = false
            });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetUserRecommendationsAsync(42);

        result.Should().HaveCount(2);
        result.Select(r => r.TargetEntityId).Should().Contain(new[] { 1, 2 });
    }

    [Fact]
    public async Task GetUserRecommendationsAsync_ExcludesExpiredRecommendations()
    {
        _dbContext.ActionRecommendations.Add(new ActionRecommendation
        {
            TargetType = ActionTargetType.Lead,
            TargetEntityId = 1,
            AssignedUserId = 42,
            Status = ActionRecommendationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // expired
            IsDeleted = false
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetUserRecommendationsAsync(42);

        result.Should().BeEmpty();
    }

    #endregion

    #region AnalyzeEmailAsync

    [Fact]
    public async Task AnalyzeEmailAsync_PositivePurchaseIntent_ParsesFieldsFromModelResponse()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK,
            HuggingFaceResponse("Sentiment: very positive. Intent: purchase inquiry. Urgency: immediate."));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var result = await service.AnalyzeEmailAsync("I want to buy your product today!", "Purchase question");

            result.Should().NotBeNull();
            result.Sentiment.Should().Be(EmailSentiment.VeryPositive);
            result.PrimaryIntent.Should().Be(EmailIntent.PurchaseIntent);
            result.Urgency.Should().Be(ResponseUrgency.Immediate);
            result.SentimentScore.Should().Be(1.0m);
            result.UrgencyScore.Should().Be(100m);
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task AnalyzeEmailAsync_WhenAiCallFails_FallsBackToNeutralDefaults()
    {
        // Handler throws to simulate a network failure; EnableLocalFallback=true
        // means InvokeModelAsync falls back to GenerateLocalResponse, whose text
        // contains none of the sentiment/intent/urgency keywords, so extraction
        // falls back to Neutral/Other/Normal defaults.
        var handler = new FakeAiHttpMessageHandler(_ => throw new HttpRequestException("simulated network failure"));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var result = await service.AnalyzeEmailAsync("Just checking in.", null);

            result.Should().NotBeNull();
            result.Sentiment.Should().Be(EmailSentiment.Neutral);
            result.PrimaryIntent.Should().Be(EmailIntent.Other);
            result.Urgency.Should().Be(ResponseUrgency.Normal);
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    #endregion

    #region GenerateEmailResponseAsync

    [Fact]
    public async Task GenerateEmailResponseAsync_HappyPath_ReturnsModelGeneratedText()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK,
            HuggingFaceResponse("Thank you for reaching out, we will follow up shortly."));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var response = await service.GenerateEmailResponseAsync("Can you help me with pricing?", "VIP customer");

            response.Should().Be("Thank you for reaching out, we will follow up shortly.");
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task GenerateEmailResponseAsync_HttpErrorAndFallbackDisabled_ReturnsUnableToProcessMessage()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.InternalServerError, "{\"error\":\"server error\"}");
        var config = new AllenAIConfiguration
        {
            OLMoEndpoint = "https://fake-olmo.test/model",
            TuluEndpoint = "https://fake-tulu.test/model",
            EnableLocalFallback = false,
            BatchSize = 10
        };
        var (service, dbContext) = CreateServiceWithHandler(handler, config);
        try
        {
            var response = await service.GenerateEmailResponseAsync("Hello", null);

            response.Should().Be("Unable to process request.");
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    #endregion

    #region ExtractActionItemsAsync

    [Fact]
    public async Task ExtractActionItemsAsync_BulletedResponse_ParsesEachLineAsSeparateItem()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK,
            HuggingFaceResponse("- Send updated proposal\n- Schedule follow-up call\n- Loop in legal team"));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var items = await service.ExtractActionItemsAsync("Please send the proposal and set up a call, and loop legal in.");

            items.Should().HaveCount(3);
            items.Should().Contain("Send updated proposal");
            items.Should().Contain("Schedule follow-up call");
            items.Should().Contain("Loop in legal team");
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task ExtractActionItemsAsync_EmptyModelResponse_ReturnsEmptyList()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK, HuggingFaceResponse(""));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var items = await service.ExtractActionItemsAsync("No content here.");

            items.Should().BeEmpty();
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    #endregion

    #region GenerateTextAsync / SummarizeTextAsync

    [Fact]
    public async Task GenerateTextAsync_HappyPath_ReturnsModelText()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK, HuggingFaceResponse("Generated output text."));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var result = await service.GenerateTextAsync("Write something.");

            result.Should().Be("Generated output text.");
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextAsync_NetworkExceptionWithFallbackEnabled_ReturnsLocalFallbackText()
    {
        var handler = new FakeAiHttpMessageHandler(_ => throw new HttpRequestException("simulated timeout"));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var prompt = "Summarize this quarter's results.";
            var result = await service.GenerateTextAsync(prompt);

            result.Should().StartWith("Analysis completed locally.");
            result.Should().Contain(prompt.Length.ToString());
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task GenerateTextAsync_NetworkExceptionWithFallbackDisabled_PropagatesException()
    {
        var handler = new FakeAiHttpMessageHandler(_ => throw new HttpRequestException("simulated failure"));
        var config = new AllenAIConfiguration
        {
            OLMoEndpoint = "https://fake-olmo.test/model",
            TuluEndpoint = "https://fake-tulu.test/model",
            EnableLocalFallback = false,
            BatchSize = 10
        };
        var (service, dbContext) = CreateServiceWithHandler(handler, config);
        try
        {
            var act = () => service.GenerateTextAsync("Anything");

            await act.Should().ThrowAsync<HttpRequestException>();
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    [Fact]
    public async Task SummarizeTextAsync_HappyPath_ReturnsSummaryText()
    {
        var handler = new FakeAiHttpMessageHandler(HttpStatusCode.OK, HuggingFaceResponse("Short summary of the content."));
        var (service, dbContext) = CreateServiceWithHandler(handler);
        try
        {
            var result = await service.SummarizeTextAsync("A very long piece of content that needs summarizing.", 50);

            result.Should().Be("Short summary of the content.");
        }
        finally
        {
            dbContext.Dispose();
        }
    }

    #endregion
}

/// <summary>
/// Minimal fake HttpMessageHandler for AllenAIService tests. Supports either a
/// fixed status code/content response or a custom responder delegate (which may
/// throw to simulate network failures/timeouts) so tests never hit the network.
/// </summary>
internal sealed class FakeAiHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FakeAiHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public FakeAiHttpMessageHandler(HttpStatusCode statusCode, string content)
        : this(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        })
    {
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_responder(request));
    }
}
