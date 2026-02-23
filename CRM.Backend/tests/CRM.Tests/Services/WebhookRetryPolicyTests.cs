// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for WebhookRetryPolicy
/// Tests cover exponential backoff, retry decisions, and disable threshold
/// </summary>
public class WebhookRetryPolicyTests
{
    private readonly WebhookRetryPolicy _policy = new();

    [Fact]
    public void MaxRetries_ShouldReturnFive()
    {
        _policy.MaxRetries.Should().Be(5);
    }

    [Fact]
    public void GetRetryDelay_ShouldReturnZero_WhenAttemptIsZeroOrNegative()
    {
        _policy.GetRetryDelay(0).Should().Be(TimeSpan.Zero);
        _policy.GetRetryDelay(-1).Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void GetRetryDelay_ShouldReturnExponentialBackoff()
    {
        // Base = 30s, multiplier = 2x
        // Attempt 1: ~30s, Attempt 2: ~60s, Attempt 3: ~120s
        var delay1 = _policy.GetRetryDelay(1);
        var delay2 = _policy.GetRetryDelay(2);
        var delay3 = _policy.GetRetryDelay(3);

        // Allow for jitter (10%) – check approximate ranges
        delay1.TotalSeconds.Should().BeInRange(30, 33);
        delay2.TotalSeconds.Should().BeInRange(60, 66);
        delay3.TotalSeconds.Should().BeInRange(120, 132);
    }

    [Fact]
    public void GetRetryDelay_ShouldCapAtMaxDelay()
    {
        // Very high attempt number should cap at 3600 seconds (1 hour)
        var delay = _policy.GetRetryDelay(20);

        delay.TotalSeconds.Should().BeLessOrEqualTo(3600);
    }

    [Fact]
    public void GetRetryDelay_ShouldIncreaseWithAttemptNumber()
    {
        var delay1 = _policy.GetRetryDelay(1);
        var delay2 = _policy.GetRetryDelay(2);

        delay2.Should().BeGreaterThan(delay1);
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_WhenMaxRetriesExceeded()
    {
        _policy.ShouldRetry(5, 500).Should().BeFalse();
        _policy.ShouldRetry(6, 500).Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_For400BadRequest()
    {
        _policy.ShouldRetry(1, 400).Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnFalse_For404NotFound()
    {
        _policy.ShouldRetry(1, 404).Should().BeFalse();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_For408Timeout()
    {
        _policy.ShouldRetry(1, 408).Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_For429RateLimit()
    {
        _policy.ShouldRetry(1, 429).Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_For500ServerError()
    {
        _policy.ShouldRetry(1, 500).Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_For502BadGateway()
    {
        _policy.ShouldRetry(1, 502).Should().BeTrue();
    }

    [Fact]
    public void ShouldRetry_ShouldReturnTrue_For503ServiceUnavailable()
    {
        _policy.ShouldRetry(1, 503).Should().BeTrue();
    }

    [Fact]
    public void ShouldDisableWebhook_ShouldReturnFalse_BelowThreshold()
    {
        _policy.ShouldDisableWebhook(9).Should().BeFalse();
        _policy.ShouldDisableWebhook(0).Should().BeFalse();
    }

    [Fact]
    public void ShouldDisableWebhook_ShouldReturnTrue_AtThreshold()
    {
        _policy.ShouldDisableWebhook(10).Should().BeTrue();
    }

    [Fact]
    public void ShouldDisableWebhook_ShouldReturnTrue_AboveThreshold()
    {
        _policy.ShouldDisableWebhook(15).Should().BeTrue();
    }
}
