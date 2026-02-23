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
/// Unit tests for WebhookCircuitBreaker
/// Tests cover circuit state transitions: Closed → Open → HalfOpen → Closed
/// </summary>
public class WebhookCircuitBreakerTests
{
    private readonly WebhookCircuitBreaker _breaker = new();

    private const string TestUrl = "https://example.com/webhook";

    [Fact]
    public void GetState_ShouldReturnClosed_ForUnknownUrl()
    {
        _breaker.GetState("https://unknown.com/hook").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void IsCircuitOpen_ShouldReturnFalse_WhenNoFailures()
    {
        _breaker.IsCircuitOpen(TestUrl).Should().BeFalse();
    }

    [Fact]
    public void IsCircuitOpen_ShouldReturnFalse_WhenBelowThreshold()
    {
        for (int i = 0; i < 4; i++)
            _breaker.RecordFailure(TestUrl);

        _breaker.IsCircuitOpen(TestUrl).Should().BeFalse();
        _breaker.GetState(TestUrl).Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void IsCircuitOpen_ShouldReturnTrue_WhenThresholdReached()
    {
        for (int i = 0; i < 5; i++)
            _breaker.RecordFailure(TestUrl);

        _breaker.IsCircuitOpen(TestUrl).Should().BeTrue();
        _breaker.GetState(TestUrl).Should().Be(CircuitState.Open);
    }

    [Fact]
    public void RecordSuccess_ShouldResetCircuit_AfterFailures()
    {
        // Push to open
        for (int i = 0; i < 5; i++)
            _breaker.RecordFailure(TestUrl);

        _breaker.GetState(TestUrl).Should().Be(CircuitState.Open);

        // Success resets
        _breaker.RecordSuccess(TestUrl);

        _breaker.GetState(TestUrl).Should().Be(CircuitState.Closed);
        _breaker.IsCircuitOpen(TestUrl).Should().BeFalse();
    }

    [Fact]
    public void RecordSuccess_ShouldKeepCircuitClosed_ForNewUrl()
    {
        _breaker.RecordSuccess("https://new.com/hook");

        _breaker.GetState("https://new.com/hook").Should().Be(CircuitState.Closed);
    }

    [Fact]
    public void CircuitBreaker_ShouldTrackUrlsIndependently()
    {
        var url1 = "https://service-a.com/hook";
        var url2 = "https://service-b.com/hook";

        // Open circuit for url1 only
        for (int i = 0; i < 5; i++)
            _breaker.RecordFailure(url1);

        _breaker.IsCircuitOpen(url1).Should().BeTrue();
        _breaker.IsCircuitOpen(url2).Should().BeFalse();
    }

    [Fact]
    public void RecordFailure_ShouldNotOpenCircuit_WhenInterspersedWithSuccess()
    {
        // Fail 3 times, succeed, fail 3 more → only 3 consecutive, not 6
        for (int i = 0; i < 3; i++)
            _breaker.RecordFailure(TestUrl);

        _breaker.RecordSuccess(TestUrl);

        for (int i = 0; i < 3; i++)
            _breaker.RecordFailure(TestUrl);

        // Consecutive failures should be 3 (reset by success), so circuit stays closed
        _breaker.GetState(TestUrl).Should().Be(CircuitState.Closed);
    }
}
