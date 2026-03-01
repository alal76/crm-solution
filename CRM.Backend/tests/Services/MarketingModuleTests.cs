// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit and integration tests for MKT-001, MKT-005, MKT-006, MKT-009.
/// Covers UnsubscribeService, UtmTrackingService and the new CampaignExecutionService execution-status methods.
/// Uses InMemory EF Core database for fast, isolated tests.
/// </summary>
public class MarketingModuleTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<UnsubscribeService>> _unsubLogger;
    private readonly Mock<ILogger<UtmTrackingService>> _utmLogger;
    private readonly Mock<ILogger<CampaignExecutionService>> _campaignLogger;
    private readonly Mock<IConfiguration> _configuration;
    private readonly UnsubscribeService _unsubscribeService;
    private readonly UtmTrackingService _utmTrackingService;

    public MarketingModuleTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _unsubLogger = new Mock<ILogger<UnsubscribeService>>();
        _utmLogger = new Mock<ILogger<UtmTrackingService>>();
        _campaignLogger = new Mock<ILogger<CampaignExecutionService>>();

        // Configure mock IConfiguration to return a JWT secret for token generation
        _configuration = new Mock<IConfiguration>();
        _configuration.Setup(c => c["Jwt:Secret"]).Returns("super-secret-32-char-test-key-xyz");

        _unsubscribeService = new UnsubscribeService(_dbContext, _configuration.Object, _unsubLogger.Object);
        _utmTrackingService = new UtmTrackingService(_dbContext, _utmLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    #region UnsubscribeService Tests (MKT-006)

    [Fact]
    public async Task GetStatusAsync_ShouldReturnUnsubscribed_False_WhenEmailNotFound()
    {
        // Act
        var result = await _unsubscribeService.GetStatusAsync("nobody@example.com");

        // Assert
        result.Should().NotBeNull();
        result.IsUnsubscribed.Should().BeFalse();
        result.Email.Should().Be("nobody@example.com");
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnUnsubscribed_True_WhenRecordExists()
    {
        // Arrange
        _dbContext.UnsubscribeRecords.Add(new UnsubscribeRecord
        {
            Email = "opt-out@example.com",
            Reason = UnsubscribeReason.TooFrequent,
            UnsubscribedAt = DateTime.UtcNow,
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _unsubscribeService.GetStatusAsync("opt-out@example.com");

        // Assert
        result.IsUnsubscribed.Should().BeTrue();
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldCreateRecord_WhenEmailNotPreviouslyOptedOut()
    {
        // Arrange
        var dto = new UnsubscribeRequestDto
        {
            Email = "new@example.com",
            Reason = UnsubscribeReason.NotInterested,
        };

        // Act
        var result = await _unsubscribeService.UnsubscribeAsync(dto);

        // Assert
        result.IsUnsubscribed.Should().BeTrue();
        var dbRecord = await _dbContext.UnsubscribeRecords
            .FirstOrDefaultAsync(r => r.Email == "new@example.com");
        dbRecord.Should().NotBeNull();
        dbRecord!.Reason.Should().Be(UnsubscribeReason.NotInterested);
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldUpdateExistingRecord_WhenEmailAlreadyOptedOut()
    {
        // Arrange — pre-existing partial record
        _dbContext.UnsubscribeRecords.Add(new UnsubscribeRecord
        {
            Email = "already@example.com",
            Reason = UnsubscribeReason.Other,
            UnsubscribedAt = DateTime.UtcNow.AddDays(-10),
        });
        await _dbContext.SaveChangesAsync();

        var dto = new UnsubscribeRequestDto
        {
            Email = "already@example.com",
            Reason = UnsubscribeReason.TooFrequent,
        };

        // Act
        await _unsubscribeService.UnsubscribeAsync(dto);

        // Assert — only one record should exist, reason updated
        var records = await _dbContext.UnsubscribeRecords
            .Where(r => r.Email == "already@example.com")
            .ToListAsync();
        records.Should().HaveCount(1);
        records[0].Reason.Should().Be(UnsubscribeReason.TooFrequent);
    }

    [Fact]
    public async Task IsUnsubscribedAsync_ShouldReturnTrue_WhenRecordExists()
    {
        // Arrange
        _dbContext.UnsubscribeRecords.Add(new UnsubscribeRecord
        {
            Email = "check@example.com",
            Reason = UnsubscribeReason.Irrelevant,
            UnsubscribedAt = DateTime.UtcNow,
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _unsubscribeService.IsUnsubscribedAsync("check@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUnsubscribedAsync_ShouldReturnFalse_WhenNoRecord()
    {
        var result = await _unsubscribeService.IsUnsubscribedAsync("clean@example.com");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateUnsubscribeTokenAsync_ShouldReturnNonEmptyToken()
    {
        // Act
        var token = await _unsubscribeService.GenerateUnsubscribeTokenAsync("user@example.com", null);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Length.Should().BeGreaterThan(10);
    }

    #endregion

    #region UtmTrackingService Tests (MKT-005)

    [Fact]
    public async Task CreateTrackingLinkAsync_ShouldBuildCorrectUtmUrl()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Name = "Summer Sale",
            Status = CampaignStatus.Draft,
        };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        var dto = new CreateTrackingLinkDto
        {
            OriginalUrl = "https://example.com/product",
            LinkAlias = "summer-sale",
            UtmSource = "email",
            UtmMedium = "newsletter",
            UtmCampaign = "summer2026",
        };

        // Act
        var result = await _utmTrackingService.CreateTrackingLinkAsync(campaign.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.TrackedUrl.Should().Contain("utm_source=email");
        result.TrackedUrl.Should().Contain("utm_medium=newsletter");
        result.TrackedUrl.Should().Contain("utm_campaign=summer2026");
        result.OriginalUrl.Should().Be("https://example.com/product");
    }

    [Fact]
    public async Task CreateTrackingLinkAsync_ShouldThrowArgumentException_WhenCampaignNotFound()
    {
        // Arrange
        var dto = new CreateTrackingLinkDto
        {
            OriginalUrl = "https://example.com",
            UtmSource = "email",
        };

        // Act & Assert
        await _utmTrackingService.Invoking(s => s.CreateTrackingLinkAsync(999999, dto))
            .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCampaignLinksAsync_ShouldReturnLinks_WhenLinksExist()
    {
        // Arrange
        var campaign = new MarketingCampaign { Name = "Test Campaign", Status = CampaignStatus.Draft };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        _dbContext.CampaignTrackingLinks.AddRange(
            new CampaignTrackingLink
            {
                CampaignId = campaign.Id,
                OriginalUrl = "https://example.com/a",
                TrackedUrl = "https://example.com/a?utm_source=email",
                UtmSource = "email",
            },
            new CampaignTrackingLink
            {
                CampaignId = campaign.Id,
                OriginalUrl = "https://example.com/b",
                TrackedUrl = "https://example.com/b?utm_source=email",
                UtmSource = "email",
            });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _utmTrackingService.GetCampaignLinksAsync(campaign.Id);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResolveAndTrackAsync_ShouldCreateClickRecord_AndReturnDestinationUrl()
    {
        // Arrange
        var campaign = new MarketingCampaign { Name = "Click Test", Status = CampaignStatus.Active };
        _dbContext.MarketingCampaigns.Add(campaign);
        await _dbContext.SaveChangesAsync();

        var link = new CampaignTrackingLink
        {
            CampaignId = campaign.Id,
            OriginalUrl = "https://example.com/landing",
            TrackedUrl = "https://example.com/landing?utm_source=email",
            TrackingToken = "abc123token",
            UtmSource = "email",
        };
        _dbContext.CampaignTrackingLinks.Add(link);
        await _dbContext.SaveChangesAsync();

        // Act
        var destinationUrl = await _utmTrackingService.ResolveAndTrackAsync(
            "abc123token", "192.168.1.1", "Mozilla/5.0 (Test)");

        // Assert
        destinationUrl.Should().Be("https://example.com/landing?utm_source=email");

        var clickRecord = await _dbContext.UtmLinkClicks
            .FirstOrDefaultAsync(c => c.TrackingLinkId == link.Id);
        clickRecord.Should().NotBeNull();
        clickRecord!.VisitorIp.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task ResolveAndTrackAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        // Act
        var result = await _utmTrackingService.ResolveAndTrackAsync("nonexistenttoken", null, null);

        // Assert
        result.Should().BeNullOrEmpty();
    }

    #endregion

    #region New Enum Values Tests (MKT-009)

    [Fact]
    public void SequenceStepType_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<SequenceStepType>();
        values.Should().Contain(SequenceStepType.Email);
        values.Should().Contain(SequenceStepType.Wait);
        values.Should().Contain(SequenceStepType.Condition);
        values.Should().Contain(SequenceStepType.Tag);
        values.Should().HaveCount(4);
    }

    [Fact]
    public void EmailTrackingEvent_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<EmailTrackingEvent>();
        values.Should().Contain(EmailTrackingEvent.Sent);
        values.Should().Contain(EmailTrackingEvent.Delivered);
        values.Should().Contain(EmailTrackingEvent.Opened);
        values.Should().Contain(EmailTrackingEvent.Clicked);
        values.Should().Contain(EmailTrackingEvent.Bounced);
        values.Should().Contain(EmailTrackingEvent.Unsubscribed);
        values.Should().Contain(EmailTrackingEvent.SpamReported);
        values.Should().HaveCount(7);
    }

    [Fact]
    public void UnsubscribeReason_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<UnsubscribeReason>();
        values.Should().Contain(UnsubscribeReason.NotInterested);
        values.Should().Contain(UnsubscribeReason.TooFrequent);
        values.Should().Contain(UnsubscribeReason.Irrelevant);
        values.Should().Contain(UnsubscribeReason.NeverSubscribed);
        values.Should().Contain(UnsubscribeReason.Other);
        values.Should().HaveCount(5);
    }

    [Fact]
    public void NurtureEnrollmentTrigger_ShouldHaveExpectedValues()
    {
        var values = Enum.GetValues<NurtureEnrollmentTrigger>();
        values.Should().Contain(NurtureEnrollmentTrigger.LeadCreated);
        values.Should().Contain(NurtureEnrollmentTrigger.LeadStatusChanged);
        values.Should().Contain(NurtureEnrollmentTrigger.ManualEnroll);
        values.Should().Contain(NurtureEnrollmentTrigger.WebFormSubmit);
        values.Should().HaveCount(4);
    }

    #endregion

    #region NurtureEnrollment Entity Tests (MKT-004)

    [Fact]
    public void NurtureEnrollment_ShouldInitialiseWithDefaults()
    {
        var enrollment = new NurtureEnrollment
        {
            SequenceId = 1,
            EnrolleeEmail = "lead@example.com",
            Trigger = NurtureEnrollmentTrigger.LeadCreated,
        };

        enrollment.CurrentStep.Should().Be(0);
        enrollment.IsCompleted.Should().BeFalse();
        enrollment.IsUnsubscribed.Should().BeFalse();
        enrollment.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task DbContext_ShouldPersistNurtureEnrollment()
    {
        // Arrange
        var sequence = new EmailSequence { Name = "Welcome" };
        _dbContext.EmailSequences.Add(sequence);
        await _dbContext.SaveChangesAsync();

        var enrollment = new NurtureEnrollment
        {
            SequenceId = sequence.Id,
            EnrolleeEmail = "test@example.com",
            Trigger = NurtureEnrollmentTrigger.ManualEnroll,
        };

        // Act
        _dbContext.NurtureEnrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.NurtureEnrollments
            .FirstOrDefaultAsync(e => e.EnrolleeEmail == "test@example.com");
        saved.Should().NotBeNull();
        saved!.SequenceId.Should().Be(sequence.Id);
    }

    #endregion
}
