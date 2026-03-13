// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for SatisfactionService.
/// Uses EF InMemory database to seed SatisfactionSurvey and SatisfactionResponse records.
/// </summary>
public class SatisfactionServiceTests
{
    private static CrmDbContext CreateDb() =>
        new CrmDbContext(
            new DbContextOptionsBuilder<CrmDbContext>()
                .UseInMemoryDatabase($"Satisfaction_{Guid.NewGuid()}")
                .Options,
            null!);

    private static SatisfactionService CreateService(CrmDbContext db) =>
        new SatisfactionService(db, new Mock<ILogger<SatisfactionService>>().Object);

    // ── CreateSurveyAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateSurveyAsync_ShouldCreateSurveyWithPendingStatus_AndPopulateToken()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var dto = new CreateSatisfactionSurveyDto
        {
            EntityType = "Opportunity",
            EntityId = 10,
            Type = SurveyType.CSAT,
            Subject = "How was the sales experience?",
        };

        var result = await service.CreateSurveyAsync(dto);

        result.Should().NotBeNull();
        result.Status.Should().Be(SurveyStatus.Pending);
        result.EntityType.Should().Be("Opportunity");
        result.EntityId.Should().Be(10);
        result.Type.Should().Be(SurveyType.CSAT);

        // ExternalToken is set on the entity; verify the survey was persisted
        db.SatisfactionSurveys.Count().Should().Be(1);
        var saved = db.SatisfactionSurveys.First();
        saved.ExternalToken.Should().NotBeNullOrEmpty();
        saved.ExpiresAt.Should().NotBeNull();
    }

    // ── GetSurveyByTokenAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetSurveyByTokenAsync_ShouldReturnSurvey_WhenTokenExists()
    {
        using var db = CreateDb();
        const string token = "abc123token";
        db.SatisfactionSurveys.Add(new SatisfactionSurvey
        {
            Id = 1, EntityType = "Contact", EntityId = 5,
            Type = SurveyType.NPS, Status = SurveyStatus.Sent,
            ExternalToken = token, IsDeleted = false,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetSurveyByTokenAsync(token);

        result.Should().NotBeNull();
        result!.EntityType.Should().Be("Contact");
        result.Type.Should().Be(SurveyType.NPS);
    }

    [Fact]
    public async Task GetSurveyByTokenAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        using var db = CreateDb();
        var service = CreateService(db);

        var result = await service.GetSurveyByTokenAsync("nonexistent-token");

        result.Should().BeNull();
    }

    // ── SubmitResponseAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SubmitResponseAsync_ShouldSetStatusResponded_WhenSurveyIsValid()
    {
        using var db = CreateDb();
        const string token = "valid-token-001";
        db.SatisfactionSurveys.Add(new SatisfactionSurvey
        {
            Id = 1, EntityType = "Account", EntityId = 3,
            Type = SurveyType.CSAT, Status = SurveyStatus.Sent,
            ExternalToken = token, IsDeleted = false,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var responseDto = await service.SubmitResponseAsync(new SubmitSatisfactionResponseDto
        {
            SurveyToken = token,
            Score = 5,
            Comment = "Excellent service",
        });

        responseDto.Should().NotBeNull();
        responseDto.Score.Should().Be(5);
        responseDto.Sentiment.Should().Be(SentimentType.VeryPositive);

        var survey = db.SatisfactionSurveys.IgnoreQueryFilters().First(s => s.ExternalToken == token);
        survey.Status.Should().Be(SurveyStatus.Responded);
        survey.ResponseReceivedAt.Should().NotBeNull();
        db.SatisfactionResponses.Count().Should().Be(1);
    }

    [Fact]
    public async Task SubmitResponseAsync_ShouldThrowInvalidOperation_WhenSurveyAlreadyAnswered()
    {
        using var db = CreateDb();
        const string token = "already-answered-token";
        db.SatisfactionSurveys.Add(new SatisfactionSurvey
        {
            Id = 2, EntityType = "Account", EntityId = 7,
            Type = SurveyType.NPS, Status = SurveyStatus.Responded,
            ExternalToken = token, IsDeleted = false,
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var act = () => service.SubmitResponseAsync(new SubmitSatisfactionResponseDto
        {
            SurveyToken = token, Score = 8,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been answered*");
    }

    [Fact]
    public async Task SubmitResponseAsync_ShouldThrowInvalidOperation_WhenSurveyIsExpired()
    {
        using var db = CreateDb();
        const string token = "expired-token-abc";
        db.SatisfactionSurveys.Add(new SatisfactionSurvey
        {
            Id = 3, EntityType = "Contact", EntityId = 9,
            Type = SurveyType.CES, Status = SurveyStatus.Sent,
            ExternalToken = token, IsDeleted = false,
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // already expired
            CreatedAt = DateTime.UtcNow.AddDays(-31), UpdatedAt = DateTime.UtcNow.AddDays(-31),
        });
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var act = () => service.SubmitResponseAsync(new SubmitSatisfactionResponseDto
        {
            SurveyToken = token, Score = 4,
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    // ── GetSurveysAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetSurveysAsync_ShouldFilterByEntityType()
    {
        using var db = CreateDb();
        db.SatisfactionSurveys.AddRange(
            new SatisfactionSurvey
            {
                Id = 1, EntityType = "Opportunity", EntityId = 1,
                Type = SurveyType.CSAT, Status = SurveyStatus.Pending,
                ExternalToken = "tok1", IsDeleted = false,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            },
            new SatisfactionSurvey
            {
                Id = 2, EntityType = "Contact", EntityId = 2,
                Type = SurveyType.NPS, Status = SurveyStatus.Pending,
                ExternalToken = "tok2", IsDeleted = false,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var result = await service.GetSurveysAsync(page: 1, pageSize: 10, entityType: "Opportunity");

        result.Items.Should().HaveCount(1);
        result.Items.First().EntityType.Should().Be("Opportunity");
        result.TotalCount.Should().Be(1);
    }

    // ── GetCSATScoreAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetCSATScoreAsync_ShouldReturnCorrectSatisfactionPercentage()
    {
        using var db = CreateDb();

        // Seed CSAT surveys
        var surveys = new[]
        {
            new SatisfactionSurvey { Id = 1, EntityType = "Account", EntityId = 1, Type = SurveyType.CSAT, Status = SurveyStatus.Responded, ExternalToken = "c1", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new SatisfactionSurvey { Id = 2, EntityType = "Account", EntityId = 2, Type = SurveyType.CSAT, Status = SurveyStatus.Responded, ExternalToken = "c2", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new SatisfactionSurvey { Id = 3, EntityType = "Account", EntityId = 3, Type = SurveyType.CSAT, Status = SurveyStatus.Responded, ExternalToken = "c3", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new SatisfactionSurvey { Id = 4, EntityType = "Account", EntityId = 4, Type = SurveyType.CSAT, Status = SurveyStatus.Responded, ExternalToken = "c4", IsDeleted = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        db.SatisfactionSurveys.AddRange(surveys);

        var now = DateTime.UtcNow;
        // Scores: 5, 4, 3, 2 → satisfied (>=4) = 2 out of 4 = 50%
        db.SatisfactionResponses.AddRange(
            new SatisfactionResponse { Id = 1, SurveyId = 1, Score = 5, Sentiment = SentimentType.VeryPositive, RespondedAt = now, CreatedAt = now, UpdatedAt = now },
            new SatisfactionResponse { Id = 2, SurveyId = 2, Score = 4, Sentiment = SentimentType.Positive, RespondedAt = now, CreatedAt = now, UpdatedAt = now },
            new SatisfactionResponse { Id = 3, SurveyId = 3, Score = 3, Sentiment = SentimentType.Neutral, RespondedAt = now, CreatedAt = now, UpdatedAt = now },
            new SatisfactionResponse { Id = 4, SurveyId = 4, Score = 2, Sentiment = SentimentType.Negative, RespondedAt = now, CreatedAt = now, UpdatedAt = now }
        );
        await db.SaveChangesAsync();

        var service = CreateService(db);
        var score = await service.GetCSATScoreAsync(null, null);

        // 2 satisfied out of 4 = 50.0
        score.Should().BeApproximately(50.0, 0.01);
    }
}
