// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ResilienceService.
/// Tests the real Polly-based circuit breaker, retry, and timeout policies.
/// </summary>
public class ResilienceServiceTests
{
    private readonly Mock<ILogger<ResilienceService>> _mockLogger = new();

    /// <summary>
    /// Creates a ResilienceService with sensible test defaults (retry disabled, short timeout).
    /// </summary>
    private ResilienceService CreateService(ResilienceOptions? options = null)
    {
        var opts = options ?? new ResilienceOptions
        {
            DefaultTimeoutSeconds = 5,
            MaxRetryAttempts = 0,
            EnableRetry = false,
            EnableCircuitBreaker = false,
            CircuitBreakerThreshold = 5,
            CircuitBreakerDurationSeconds = 60,
            RetryBaseDelayMs = 10,
            RetryMaxDelayMs = 100
        };
        var mockOpts = new Mock<IOptions<ResilienceOptions>>();
        mockOpts.Setup(o => o.Value).Returns(opts);
        return new ResilienceService(_mockLogger.Object, mockOpts.Object);
    }

    /// <summary>
    /// Creates options with circuit breaker enabled and a low threshold for testing.
    /// </summary>
    private static ResilienceOptions CreateCircuitBreakerOptions(int threshold = 2) => new()
    {
        DefaultTimeoutSeconds = 5,
        MaxRetryAttempts = 0,
        EnableRetry = false,
        EnableCircuitBreaker = true,
        CircuitBreakerThreshold = threshold,
        CircuitBreakerDurationSeconds = 60,
        RetryBaseDelayMs = 10,
        RetryMaxDelayMs = 100
    };

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_SuccessfulAction_ReturnsResult()
    {
        var svc = CreateService();
        var result = await svc.ExecuteAsync("test-svc", ct => Task.FromResult("ok"));
        result.Should().Be("ok");
    }

    [Fact]
    public async Task ExecuteAsync_NonTransientException_PropagatesImmediately()
    {
        var svc = CreateService();
        Func<Task> act = () => svc.ExecuteAsync<string>("test-svc",
            ct => throw new InvalidOperationException("bad"));
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("bad");
    }

    [Fact]
    public async Task ExecuteAsync_TransientFailureThenSuccess_RetriesAndReturns()
    {
        var opts = new ResilienceOptions
        {
            DefaultTimeoutSeconds = 5,
            MaxRetryAttempts = 2,
            EnableRetry = true,
            EnableCircuitBreaker = false,
            RetryBaseDelayMs = 10,
            RetryMaxDelayMs = 100
        };
        var svc = CreateService(opts);

        var callCount = 0;
        var result = await svc.ExecuteAsync("retry-svc", ct =>
        {
            callCount++;
            if (callCount < 3)
                throw new HttpRequestException("transient");
            return Task.FromResult("recovered");
        });

        result.Should().Be("recovered");
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_CircuitOpen_ThrowsServiceUnavailableException()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        // Trip the circuit breaker
        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("cb-svc", ct => throw new HttpRequestException("fail")); }
            catch { }
        }

        // Next call should get ServiceUnavailableException (circuit open)
        Func<Task> act = () => svc.ExecuteAsync<string>("cb-svc", ct => Task.FromResult("ok"));
        await act.Should().ThrowAsync<ServiceUnavailableException>();
    }

    [Fact]
    public async Task ExecuteAsync_TracksSuccessState()
    {
        var opts = CreateCircuitBreakerOptions();
        var svc = CreateService(opts);

        await svc.ExecuteAsync("state-svc", ct => Task.FromResult("ok"));

        var states = svc.GetCircuitBreakerStates();
        var state = states.FirstOrDefault(s => s.ServiceName == "state-svc");
        state.Should().NotBeNull();
        state!.SuccessCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteAsync_TracksFailureState()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 10);
        var svc = CreateService(opts);

        try { await svc.ExecuteAsync<string>("fail-svc", ct => throw new HttpRequestException("err")); }
        catch { }

        var states = svc.GetCircuitBreakerStates();
        var state = states.FirstOrDefault(s => s.ServiceName == "fail-svc");
        state.Should().NotBeNull();
        state!.FailureCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExecuteAsync_DifferentServiceNames_IsolatedPolicies()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        // Trip circuit for svc-a
        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("svc-a", ct => throw new HttpRequestException("fail")); }
            catch { }
        }

        // svc-b should still work
        var result = await svc.ExecuteAsync("svc-b", ct => Task.FromResult("b-ok"));
        result.Should().Be("b-ok");
    }

    #endregion

    #region ExecuteWithFallbackAsync Tests

    [Fact]
    public async Task ExecuteWithFallbackAsync_PrimarySucceeds_ReturnsPrimaryResult()
    {
        var svc = CreateService();
        var result = await svc.ExecuteWithFallbackAsync(
            "fb-svc",
            ct => Task.FromResult("primary"),
            _ => "fallback");
        result.Should().Be("primary");
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_PrimaryFails_ReturnsFallback()
    {
        var svc = CreateService();
        var result = await svc.ExecuteWithFallbackAsync<string>(
            "fb-svc",
            ct => throw new InvalidOperationException("boom"),
            _ => "fallback");
        result.Should().Be("fallback");
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_ExceptionPassedToFallback()
    {
        var svc = CreateService();
        Exception? captured = null;

        await svc.ExecuteWithFallbackAsync<string>(
            "fb-svc",
            ct => throw new InvalidOperationException("specific-error"),
            ex =>
            {
                captured = ex;
                return "handled";
            });

        captured.Should().NotBeNull();
        captured!.Message.Should().Contain("specific-error");
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_CircuitOpen_InvokesFallback()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        // Trip the circuit
        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("cb-fb-svc", ct => throw new HttpRequestException("fail")); }
            catch { }
        }

        var result = await svc.ExecuteWithFallbackAsync(
            "cb-fb-svc",
            ct => Task.FromResult("primary"),
            _ => "circuit-fallback");
        result.Should().Be("circuit-fallback");
    }

    #endregion

    #region GetCircuitBreakerStates Tests

    [Fact]
    public void GetCircuitBreakerStates_NoActivity_ReturnsEmpty()
    {
        var svc = CreateService();
        var states = svc.GetCircuitBreakerStates();
        states.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCircuitBreakerStates_AfterActivity_ReturnsStates()
    {
        var opts = CreateCircuitBreakerOptions();
        var svc = CreateService(opts);

        await svc.ExecuteAsync("svc-x", ct => Task.FromResult(1));
        await svc.ExecuteAsync("svc-y", ct => Task.FromResult(2));

        var states = svc.GetCircuitBreakerStates();
        states.Should().HaveCountGreaterOrEqualTo(2);
        states.Select(s => s.ServiceName).Should().Contain("svc-x").And.Contain("svc-y");
    }

    #endregion

    #region IsCircuitOpen Tests

    [Fact]
    public void IsCircuitOpen_UnknownService_ReturnsFalse()
    {
        var svc = CreateService();
        svc.IsCircuitOpen("nonexistent").Should().BeFalse();
    }

    [Fact]
    public async Task IsCircuitOpen_ClosedCircuit_ReturnsFalse()
    {
        var opts = CreateCircuitBreakerOptions();
        var svc = CreateService(opts);
        await svc.ExecuteAsync("closed-svc", ct => Task.FromResult("ok"));
        svc.IsCircuitOpen("closed-svc").Should().BeFalse();
    }

    [Fact]
    public async Task IsCircuitOpen_TrippedCircuit_ReturnsTrue()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("open-svc", ct => throw new HttpRequestException("fail")); }
            catch { }
        }

        svc.IsCircuitOpen("open-svc").Should().BeTrue();
    }

    #endregion

    #region ResetCircuitBreaker Tests

    [Fact]
    public async Task ResetCircuitBreaker_OpensCircuitThenResets_AllowsCalls()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        // Trip the circuit
        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("reset-svc", ct => throw new HttpRequestException("fail")); }
            catch { }
        }
        svc.IsCircuitOpen("reset-svc").Should().BeTrue();

        // Reset
        svc.ResetCircuitBreaker("reset-svc");
        svc.IsCircuitOpen("reset-svc").Should().BeFalse();

        // Should be callable again
        var result = await svc.ExecuteAsync("reset-svc", ct => Task.FromResult("back"));
        result.Should().Be("back");
    }

    [Fact]
    public void ResetCircuitBreaker_UnknownService_DoesNotThrow()
    {
        var svc = CreateService();
        var act = () => svc.ResetCircuitBreaker("nonexistent");
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ResetCircuitBreaker_ResetsState()
    {
        var opts = CreateCircuitBreakerOptions(threshold: 2);
        var svc = CreateService(opts);

        for (var i = 0; i < 2; i++)
        {
            try { await svc.ExecuteAsync<string>("rst-svc", ct => throw new HttpRequestException("fail")); }
            catch { }
        }

        svc.ResetCircuitBreaker("rst-svc");
        var states = svc.GetCircuitBreakerStates();
        var state = states.FirstOrDefault(s => s.ServiceName == "rst-svc");
        // After reset the state should be Closed (or removed)
        if (state != null)
        {
            state.State.Should().NotBe(Polly.CircuitBreaker.CircuitState.Open);
        }
    }

    #endregion

    #region ServiceUnavailableException Tests

    [Fact]
    public void ServiceUnavailableException_MessagePreserved()
    {
        var ex = new ServiceUnavailableException("svc is down");
        ex.Message.Should().Be("svc is down");
    }

    [Fact]
    public void ServiceUnavailableException_InnerExceptionPreserved()
    {
        var inner = new HttpRequestException("timeout");
        var ex = new ServiceUnavailableException("svc is down", inner);
        ex.InnerException.Should().BeSameAs(inner);
    }

    #endregion
}
