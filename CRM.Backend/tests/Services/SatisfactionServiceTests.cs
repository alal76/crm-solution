// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class SatisfactionServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<SatisfactionService>> _mockLogger;
    private readonly SatisfactionService _service;

    public SatisfactionServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<SatisfactionService>>();
        _service = new SatisfactionService(_mockContext.Object, _mockLogger.Object);
    }

    // ── Test Helpers ──────────────────────────────────────────────────────────

    private void SetupSurveys(List<SatisfactionSurvey>? surveys = null)
    {
        surveys ??= new List<SatisfactionSurvey>();
        var mockSurveys = MockDbSetFactory.CreateMockDbSet(surveys);
        _mockContext.Setup(c => c.SatisfactionSurveys).Returns(mockSurveys.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private void SetupResponses(List<SatisfactionResponse>? responses = null)
    {
        responses ??= new List<SatisfactionResponse>();
        var mockResponses = MockDbSetFactory.CreateMockDbSet(responses);
        _mockContext.Setup(c => c.SatisfactionResponses).Returns(mockResponses.Object);
    }

    private static SatisfactionSurvey MakeSurvey(
        int id = 1,
        SurveyType type = SurveyType.NPS,
        SurveyStatus status = SurveyStatus.Sent,
        string token = "abc123",
        SatisfactionResponse? response = null)
    {
        return new SatisfactionSurvey
        {
            Id = id,
            EntityType = "ServiceRequest",
            EntityId = 10,
            Type = type,
            Status = status,
            ExternalToken = token,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            UpdatedAt = DateTime.UtcNow.AddHours(-1),
            Response = response,
        };
    }

    private static SatisfactionResponse MakeResponse(
        int id = 1,
        int surveyId = 1,
        int score = 8,
        SatisfactionSurvey? survey = null)
    {
        return new SatisfactionResponse
        {
            Id = id,
            SurveyId = surveyId,
            Score = score,
            Sentiment = SentimentType.Positive,
            RespondedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Survey = survey,
        };
    }

    // ── CreateSurvey Tests ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSurvey_ShouldSetPendingStatusAndGenerateToken()
    {
        // Arrange
        SetupSurveys();
        SetupResponses();
        var dto = new CreateSatisfactionSurveyDto
        {
            EntityType = "ServiceRequest",
            EntityId = 5,
            Type = SurveyType.NPS,
        };

        // Act
        var result = await _service.CreateSurveyAsync(dto);

        // Assert
        result.Status.Should().Be(SurveyStatus.Pending);
        result.EntityType.Should().Be("ServiceRequest");
        result.EntityId.Should().Be(5);
        result.Type.Should().Be(SurveyType.NPS);
    }

    [Fact]
    public async Task CreateSurvey_ShouldPersistSurveyWithToken()
    {
        // Arrange
        SatisfactionSurvey? captured = null;
        var surveys = new List<SatisfactionSurvey>();
        var mockSurveys = MockDbSetFactory.CreateMockDbSet(surveys);
        mockSurveys.Setup(m => m.Add(It.IsAny<SatisfactionSurvey>()))
            .Callback<SatisfactionSurvey>(s =>
            {
                s.Id = 99;
                captured = s;
            });
        _mockContext.Setup(c => c.SatisfactionSurveys).Returns(mockSurveys.Object);
        _mockContext.Setup(c => c.SatisfactionResponses)
            .Returns(MockDbSetFactory.CreateMockDbSet(new List<SatisfactionResponse>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateSatisfactionSurveyDto
        {
            EntityType = "Account",
            EntityId = 1,
            Type = SurveyType.CSAT,
            Subject = "Rate your experience",
        };

        // Act
        await _service.CreateSurveyAsync(dto);

        // Assert
        captured.Should().NotBeNull();
        captured!.ExternalToken.Should().NotBeNullOrEmpty();
        captured.ExternalToken!.Length.Should().Be(32); // Guid.NewGuid().ToString("N")
        captured.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(29));
    }

    // ── SubmitResponse Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task SubmitResponse_ShouldSetSurveyStatusToResponded()
    {
        // Arrange
        var survey = MakeSurvey(status: SurveyStatus.Sent, token: "tok123");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto
        {
            SurveyToken = "tok123",
            Score = 9,
        };

        // Act
        await _service.SubmitResponseAsync(dto);

        // Assert
        survey.Status.Should().Be(SurveyStatus.Responded);
        survey.ResponseReceivedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitResponse_ShouldCalculateSentiment_VeryPositive_ForNPSScore10()
    {
        // Arrange
        var survey = MakeSurvey(type: SurveyType.NPS, status: SurveyStatus.Sent, token: "nps10");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "nps10", Score = 10 };

        // Act
        var result = await _service.SubmitResponseAsync(dto);

        // Assert
        result.Sentiment.Should().Be(SentimentType.VeryPositive);
    }

    [Fact]
    public async Task SubmitResponse_ShouldCalculateSentiment_Neutral_ForNPSScore5()
    {
        // Arrange — score 5 maps to Neutral in the NPS sentiment scale (5-6 = Neutral)
        var survey = MakeSurvey(type: SurveyType.NPS, status: SurveyStatus.Sent, token: "nps5");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "nps5", Score = 5 };

        // Act
        var result = await _service.SubmitResponseAsync(dto);

        // Assert — NPS: >=9 VeryPositive, >=7 Positive, >=5 Neutral, >=3 Negative, <3 VeryNegative
        result.Sentiment.Should().Be(SentimentType.Neutral);
    }

    [Fact]
    public async Task SubmitResponse_ShouldCalculateSentiment_Negative_ForNPSScore4()
    {
        // Arrange — score 4 maps to Negative in the NPS sentiment scale (3-4 = Negative)
        var survey = MakeSurvey(type: SurveyType.NPS, status: SurveyStatus.Sent, token: "nps4");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "nps4", Score = 4 };

        // Act
        var result = await _service.SubmitResponseAsync(dto);

        // Assert
        result.Sentiment.Should().Be(SentimentType.Negative);
    }

    [Fact]
    public async Task SubmitResponse_ShouldThrow_WhenSurveyAlreadyAnswered()
    {
        // Arrange
        var survey = MakeSurvey(status: SurveyStatus.Responded, token: "answered");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "answered", Score = 7 };

        // Act & Assert
        await _service.Invoking(s => s.SubmitResponseAsync(dto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already*");
    }

    [Fact]
    public async Task SubmitResponse_ShouldThrow_WhenSurveyExpired()
    {
        // Arrange
        var survey = MakeSurvey(status: SurveyStatus.Sent, token: "expired");
        survey.ExpiresAt = DateTime.UtcNow.AddDays(-1); // expired yesterday
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "expired", Score = 3 };

        // Act & Assert
        await _service.Invoking(s => s.SubmitResponseAsync(dto))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task SubmitResponse_ShouldThrow_WhenTokenNotFound()
    {
        // Arrange
        SetupSurveys(new List<SatisfactionSurvey>()); // empty list
        SetupResponses();

        var dto = new SubmitSatisfactionResponseDto { SurveyToken = "notexist", Score = 5 };

        // Act & Assert
        await _service.Invoking(s => s.SubmitResponseAsync(dto))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ── NPS Score Tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetNPSScore_ShouldReturnCorrectFormula()
    {
        // Arrange: 3 promoters (>=9), 2 passives (7-8), 5 detractors (<=6)
        // NPS = (3 - 5) / 10 * 100 = -20
        var npsSurvey = MakeSurvey(type: SurveyType.NPS);
        var responses = new List<SatisfactionResponse>
        {
            MakeResponse(id: 1, score: 10, survey: npsSurvey),
            MakeResponse(id: 2, score: 9,  survey: npsSurvey),
            MakeResponse(id: 3, score: 9,  survey: npsSurvey),
            MakeResponse(id: 4, score: 8,  survey: npsSurvey), // passive
            MakeResponse(id: 5, score: 7,  survey: npsSurvey), // passive
            MakeResponse(id: 6, score: 6,  survey: npsSurvey), // detractor
            MakeResponse(id: 7, score: 5,  survey: npsSurvey), // detractor
            MakeResponse(id: 8, score: 4,  survey: npsSurvey), // detractor
            MakeResponse(id: 9, score: 3,  survey: npsSurvey), // detractor
            MakeResponse(id: 10, score: 1, survey: npsSurvey), // detractor
        };

        SetupSurveys();
        SetupResponses(responses);

        // Act
        var nps = await _service.GetNPSScoreAsync(null, null);

        // Assert: (3 - 5) / 10 * 100 = -20
        nps.Should().Be(-20.0);
    }

    [Fact]
    public async Task GetNPSScore_ShouldReturn100_WhenAllPromoters()
    {
        // Arrange: all 5 responses are promoters (score >= 9)
        var npsSurvey = MakeSurvey(type: SurveyType.NPS);
        var responses = Enumerable.Range(1, 5)
            .Select(i => MakeResponse(id: i, score: 9, survey: npsSurvey))
            .ToList();

        SetupSurveys();
        SetupResponses(responses);

        // Act
        var nps = await _service.GetNPSScoreAsync(null, null);

        // Assert
        nps.Should().Be(100.0);
    }

    // ── CSAT Score Tests ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetCSATScore_ShouldReturnCorrectPercentage()
    {
        // Arrange: 3 satisfied (score >= 4), 2 dissatisfied
        var csatSurvey = MakeSurvey(type: SurveyType.CSAT);
        var responses = new List<SatisfactionResponse>
        {
            MakeResponse(id: 1, score: 5, survey: csatSurvey),
            MakeResponse(id: 2, score: 4, survey: csatSurvey),
            MakeResponse(id: 3, score: 4, survey: csatSurvey),
            MakeResponse(id: 4, score: 3, survey: csatSurvey), // not satisfied
            MakeResponse(id: 5, score: 1, survey: csatSurvey), // not satisfied
        };

        SetupSurveys();
        SetupResponses(responses);

        // Act
        var csat = await _service.GetCSATScoreAsync(null, null);

        // Assert: 3/5 * 100 = 60%
        csat.Should().Be(60.0);
    }

    [Fact]
    public async Task GetCSATScore_ShouldReturn0_WhenNoResponses()
    {
        // Arrange
        SetupSurveys();
        SetupResponses(new List<SatisfactionResponse>());

        // Act
        var csat = await _service.GetCSATScoreAsync(null, null);

        // Assert
        csat.Should().Be(0.0);
    }

    // ── GetSurveyByToken Tests ────────────────────────────────────────────────

    [Fact]
    public async Task GetSurveyByToken_ShouldReturnDto_WhenFound()
    {
        // Arrange
        var survey = MakeSurvey(id: 7, token: "findme");
        SetupSurveys(new List<SatisfactionSurvey> { survey });
        SetupResponses();

        // Act
        var result = await _service.GetSurveyByTokenAsync("findme");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(7);
    }

    [Fact]
    public async Task GetSurveyByToken_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        SetupSurveys(new List<SatisfactionSurvey>());
        SetupResponses();

        // Act
        var result = await _service.GetSurveyByTokenAsync("doesnotexist");

        // Assert
        result.Should().BeNull();
    }
}
