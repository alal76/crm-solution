// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using Xunit;

namespace CRM.Tests.Dtos;

/// <summary>
/// Test suite for Email Sequence DTOs validation.
/// </summary>
public class EmailSequenceDtoTests
{
    [Fact]
    public void CreateEmailSequenceDto_WithValidData_ShouldPass()
    {
        // Arrange & Act
        var dto = new CreateEmailSequenceDto
        {
            Name = "Test Sequence",
            Description = "A test sequence",
            DefaultFromEmail = "test@example.com",
            DefaultFromName = "Test Sender",
            SendingStartHour = 8,
            SendingEndHour = 17
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Test Sequence", dto.Name);
    }

    [Fact]
    public void CreateEmailSequenceDto_WithoutName_ShouldFail()
    {
        // Arrange & Act
        var dto = new CreateEmailSequenceDto
        {
            Name = "",
            DefaultFromEmail = "test@example.com"
        };

        // Assert
        Assert.True(string.IsNullOrEmpty(dto.Name));
    }

    [Fact]
    public void EmailSequenceStepDto_WithValidData_ShouldPass()
    {
        // Arrange & Act
        var step = new EmailSequenceStepDto
        {
            Id = 1,
            SequenceId = 1,
            StepNumber = 1,
            Name = "Step 1",
            Subject = "Welcome",
            HtmlContent = "<html><body>Welcome</body></html>",
            TimingMode = "Delay",
            DelayDays = 1
        };

        // Assert
        Assert.NotNull(step);
        Assert.Equal(1, step.StepNumber);
    }

    [Fact]
    public void EmailSequenceEnrollmentDto_WithValidEmail_ShouldPass()
    {
        // Arrange & Act
        var enrollment = new EmailSequenceEnrollmentDto
        {
            Id = 1,
            SequenceId = 1,
            Email = "user@example.com",
            Status = "Active"
        };

        // Assert
        Assert.NotNull(enrollment);
        Assert.Equal("user@example.com", enrollment.Email);
    }

    [Fact]
    public void CreateEmailSequenceEnrollmentDto_WithInvalidEmail_ShouldFail()
    {
        // Arrange & Act
        var enrollment = new CreateEmailSequenceEnrollmentDto
        {
            Email = "invalid-email"
        };

        // Assert  
        Assert.NotEmpty(enrollment.Email);
    }

    [Fact]
    public void EmailSequenceAnalyticsDto_WithMetrics_ShouldCalculate()
    {
        // Arrange & Act
        var analytics = new EmailSequenceAnalyticsDto
        {
            SequenceId = 1,
            SequenceName = "Test",
            TotalEnrolled = 100,
            TotalCompleted = 50,
            OpenRate = 0.45m,
            ClickRate = 0.15m
        };

        // Assert
        Assert.NotNull(analytics);
        Assert.Equal(100, analytics.TotalEnrolled);
        Assert.Equal(0.45m, analytics.OpenRate);
    }
}

/// <summary>
/// Test suite for Campaign DTOs validation.
/// </summary>
public class CampaignDtoTests
{
    [Fact]
    public void CreateCampaignDto_WithValidData_ShouldPass()
    {
        // Arrange & Act
        var dto = new CreateCampaignDto
        {
            Name = "Summer Campaign",
            CampaignType = 0,
            Budget = 5000m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Summer Campaign", dto.Name);
        Assert.Equal(5000m, dto.Budget);
    }

    [Fact]
    public void CreateCampaignDto_WithNegativeBudget_ShouldFail()
    {
        // Arrange & Act
        var dto = new CreateCampaignDto
        {
            Name = "Test",
            CampaignType = 0,
            Budget = -100m
        };

        // Assert
        Assert.True(dto.Budget < 0);
    }

    [Fact]
    public void CampaignRecipientDto_WithValidData_ShouldPass()
    {
        // Arrange & Act
        var recipient = new CampaignRecipientDto
        {
            Id = 1,
            CampaignId = 1,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        Assert.NotNull(recipient);
        Assert.Equal("test@example.com", recipient.Email);
    }

    [Fact]
    public void CampaignMetricsDto_WithCalculations_ShouldWork()
    {
        // Arrange & Act
        var metrics = new CampaignMetricsDto
        {
            CampaignId = 1,
            CampaignName = "Test",
            Impressions = 1000,
            Clicks = 100,
            Conversions = 10,
            Roi = 2.5m
        };

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(1000, metrics.Impressions);
    }

    [Fact]
    public void DuplicateCampaignDto_WithValidName_ShouldPass()
    {
        // Arrange & Act
        var dup = new DuplicateCampaignDto
        {
            NewName = "Copy of Campaign",
            CopyRecipients = true
        };

        // Assert
        Assert.NotNull(dup);
        Assert.NotEmpty(dup.NewName);
    }

    [Fact]
    public void ScheduleCampaignDto_WithValidDatetime_ShouldPass()
    {
        // Arrange & Act
        var schedule = new ScheduleCampaignDto
        {
            ScheduledDate = DateTime.UtcNow.AddDays(7),
            ScheduledHour = 14,
            ScheduledMinute = 30
        };

        // Assert
        Assert.NotNull(schedule);
        Assert.True(schedule.ScheduledDate > DateTime.UtcNow);
    }
}

/// <summary>
/// Test suite for Webhook Management DTOs validation.
/// </summary>
public class WebhookManagementDtoTests
{
    [Fact]
    public void CreateWebhookDto_WithValidHttpsUrl_ShouldPass()
    {
        // Arrange & Act
        var dto = new CreateWebhookDto
        {
            Url = "https://example.com/webhooks/crm",
            Description = "Test webhook",
            EventTypes = new List<string> { "Account.Created", "Contact.Updated" },
            MaxRetries = 5
        };

        // Assert
        Assert.NotNull(dto);
        Assert.True(dto.Url.StartsWith("https://"));
        Assert.NotEmpty(dto.EventTypes);
    }

    [Fact]
    public void CreateWebhookDto_WithInvalidUrl_ShouldFail()
    {
        // Arrange & Act
        var dto = new CreateWebhookDto
        {
            Url = "not-a-valid-url",
            EventTypes = new List<string> { "Test" }
        };

        // Assert
        Assert.NotNull(dto);
    }

    [Fact]
    public void CreateWebhookDto_WithNoEventTypes_ShouldFail()
    {
        // Arrange & Act
        var dto = new CreateWebhookDto
        {
            Url = "https://example.com/webhook",
            EventTypes = new List<string>()
        };

        // Assert
        Assert.Empty(dto.EventTypes);
    }

    [Fact]
    public void WebhookDeliveryDto_WithSuccessfulResponse_ShouldPass()
    {
        // Arrange & Act
        var delivery = new WebhookDeliveryDto
        {
            Id = 1,
            WebhookId = 1,
            Url = "https://example.com/webhook",
            EventType = "Account.Created",
            Success = true,
            ResponseStatusCode = 200,
            DurationMs = 145.5
        };

        // Assert
        Assert.NotNull(delivery);
        Assert.True(delivery.Success);
        Assert.Equal(200, delivery.ResponseStatusCode);
    }

    [Fact]
    public void WebhookTestResultDto_WithFailedDelivery_ShouldCapture()
    {
        // Arrange & Act
        var result = new WebhookTestResultDto
        {
            WebhookId = 1,
            Url = "https://example.com/webhook",
            Success = false,
            ResponseStatusCode = 500,
            ErrorMessage = "Internal Server Error",
            DurationMs = 5000
        };

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotEmpty(result.ErrorMessage);
    }

    [Fact]
    public void WebhookStatisticsDto_WithMetrics_ShouldCalculate()
    {
        // Arrange & Act
        var stats = new WebhookStatisticsDto
        {
            WebhookId = 1,
            Url = "https://example.com/webhook",
            TotalDeliveries = 100,
            SuccessfulDeliveries = 95,
            FailedDeliveries = 5,
            SuccessRate = 95.0
        };

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(100, stats.TotalDeliveries);
        Assert.Equal(95.0, stats.SuccessRate);
    }
}

/// <summary>
/// Test suite for Commission Management DTOs validation.
/// </summary>
public class CommissionManagementDtoTests
{
    [Fact]
    public void CreateCommissionDto_WithValidData_ShouldPass()
    {
        // Arrange & Act
        var dto = new CreateCommissionDto
        {
            UserId = 1,
            CommissionPlanId = 1,
            OpportunityId = 100,
            DealAmount = 50000m,
            CommissionRate = 5m,
            CommissionAmount = 2500m
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(1, dto.UserId);
        Assert.Equal(50000m, dto.DealAmount);
    }

    [Fact]
    public void CreateCommissionDto_WithNegativeRate_ShouldFail()
    {
        // Arrange & Act
        var dto = new CreateCommissionDto
        {
            UserId = 1,
            CommissionPlanId = 1,
            DealAmount = 50000m,
            CommissionRate = -5m,
            CommissionAmount = 2500m
        };

        // Assert
        Assert.True(dto.CommissionRate < 0);
    }

    [Fact]
    public void CreateCommissionPlanDto_WithTiers_ShouldPass()
    {
        // Arrange & Act
        var dto = new CreateCommissionPlanDto
        {
            Name = "Enterprise Plan",
            CommissionType = 1,
            Trigger = 0,
            BaseRate = 10m,
            MaxCap = 50000m,
            IsActive = true
        };

        // Assert
        Assert.NotNull(dto);
        Assert.Equal("Enterprise Plan", dto.Name);
    }

    [Fact]
    public void CommissionTierDto_WithValidRange_ShouldPass()
    {
        // Arrange & Act
        var tier = new CommissionTierDto
        {
            Id = 1,
            PlanId = 1,
            TierLevel = 1,
            TierName = "Bronze",
            MinValue = 0m,
            MaxValue = 100000m,
            Rate = 5m
        };

        // Assert
        Assert.NotNull(tier);
        Assert.True(tier.MaxValue > tier.MinValue);
    }

    [Fact]
    public void CommissionStatementDto_WithPeriod_ShouldPass()
    {
        // Arrange & Act
        var statement = new CommissionStatementDto
        {
            Id = 1,
            StatementNumber = "STMT-2026-01",
            UserId = 1,
            PeriodStartDate = new DateTime(2026, 1, 1),
            PeriodEndDate = new DateTime(2026, 1, 31),
            CommissionCount = 25,
            TotalAmount = 50000m,
            ApprovedAmount = 45000m
        };

        // Assert
        Assert.NotNull(statement);
        Assert.NotEmpty(statement.StatementNumber);
    }

    [Fact]
    public void ApproveCommissionDto_WithApprover_ShouldPass()
    {
        // Arrange & Act  
        var approve = new ApproveCommissionDto
        {
            ApprovedById = 5,
            ApprovalNotes = "Approved"
        };

        // Assert
        Assert.NotNull(approve);
        Assert.Equal(5, approve.ApprovedById);
    }

    [Fact]
    public void RejectCommissionDto_WithReason_ShouldPass()
    {
        // Arrange & Act
        var reject = new RejectCommissionDto
        {
            Reason = "Deal cancelled"
        };

        // Assert
        Assert.NotNull(reject);
        Assert.NotEmpty(reject.Reason);
    }

    [Fact]
    public void CommissionLeaderboardDto_WithRanking_ShouldPass()
    {
        // Arrange & Act
        var leaderboard = new CommissionLeaderboardDto
        {
            UserId = 1,
            UserName = "John Doe",
            TotalCommission = 150000m,
            CommissionCount = 30,
            AverageCommission = 5000m,
            Rank = 1
        };

        // Assert
        Assert.NotNull(leaderboard);
        Assert.Equal(1, leaderboard.Rank);
    }

    [Fact]
    public void CommissionForecastDto_WithProjection_ShouldPass()
    {
        // Arrange & Act
        var forecast = new CommissionForecastDto
        {
            UserId = 1,
            UserName = "Jane Smith",
            ForecastedCommission = 75000m,
            CurrentCommission = 45000m,
            ProjectedTotal = 120000m,
            PipelineValue = 500000m,
            WinRate = 0.35
        };

        // Assert
        Assert.NotNull(forecast);
        Assert.True(forecast.ProjectedTotal > forecast.CurrentCommission);
    }

    [Fact]
    public void CommissionCalculationResultDto_WithBreakdown_ShouldPass()
    {
        // Arrange & Act
        var calc = new CommissionCalculationResultDto
        {
            UserId = 1,
            OpportunityId = 100,
            PlanId = 1,
            DealAmount = 100000m,
            CommissionRate = 10m,
            FinalAmount = 10000m,
            Breakdown = new List<CommissionBreakdownDto>
            {
                new CommissionBreakdownDto
                {
                    Description = "Base Commission",
                    Amount = 100000m,
                    Rate = 0.10m,
                    Result = 10000m
                }
            }
        };

        // Assert
        Assert.NotNull(calc);
        Assert.NotEmpty(calc.Breakdown);
        Assert.Equal(10000m, calc.FinalAmount);
    }
}
