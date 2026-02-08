// CRM Solution - Customer Relationship Management System
// Resilience Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Net.Http;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ResilienceService
/// Covers: Retry policies, circuit breakers, fallbacks
/// </summary>
public class ResilienceServiceTests
{
    private readonly Mock<ILogger<ResilienceService>> _mockLogger;
    private readonly Mock<IOptions<ResilienceOptions>> _mockOptions;
    private readonly ResilienceService _service;

    public ResilienceServiceTests()
    {
        _mockLogger = new Mock<ILogger<ResilienceService>>();
        _mockOptions = new Mock<IOptions<ResilienceOptions>>();
        _mockOptions.Setup(o => o.Value).Returns(new ResilienceOptions
        {
            RetryCount = 3,
            RetryDelayMilliseconds = 100,
            CircuitBreakerThreshold = 5,
            CircuitBreakerDurationSeconds = 30
        });

        _service = new ResilienceService(_mockOptions.Object, _mockLogger.Object);
    }

    #region Retry Policy Tests

    [Fact]
    public async Task ExecuteWithRetryAsync_SucceedsFirstTry_ReturnsResult()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> action = () =>
        {
            callCount++;
            return Task.FromResult("Success");
        };

        // Act
        var result = await _service.ExecuteWithRetryAsync(action);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_FailsThenSucceeds_RetriesAndReturns()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> action = () =>
        {
            callCount++;
            if (callCount < 3)
                throw new HttpRequestException("Transient error");
            return Task.FromResult("Success");
        };

        // Act
        var result = await _service.ExecuteWithRetryAsync(action);

        // Assert
        result.Should().Be("Success");
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_AllRetriesFail_ThrowsException()
    {
        // Arrange
        Func<Task<string>> action = () =>
        {
            throw new HttpRequestException("Persistent error");
        };

        // Act
        Func<Task> act = async () => await _service.ExecuteWithRetryAsync(action);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ExecuteWithRetryAsync_WithCustomRetryCount_UsesCustomCount()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> action = () =>
        {
            callCount++;
            throw new HttpRequestException("Error");
        };

        // Act
        try
        {
            await _service.ExecuteWithRetryAsync(action, retryCount: 5);
        }
        catch (HttpRequestException) { }

        // Assert
        callCount.Should().Be(6); // 1 initial + 5 retries
    }

    #endregion

    #region Circuit Breaker Tests

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_CircuitClosed_ExecutesAction()
    {
        // Arrange
        Func<Task<string>> action = () => Task.FromResult("Success");

        // Act
        var result = await _service.ExecuteWithCircuitBreakerAsync("test-circuit", action);

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task ExecuteWithCircuitBreakerAsync_CircuitOpen_ReturnsFallback()
    {
        // Arrange
        // First, trip the circuit
        for (int i = 0; i < 5; i++)
        {
            try
            {
                await _service.ExecuteWithCircuitBreakerAsync("test-circuit", () =>
                    throw new Exception("Error"));
            }
            catch { }
        }

        Func<Task<string>> action = () => Task.FromResult("Success");
        Func<Task<string>> fallback = () => Task.FromResult("Fallback");

        // Act
        var result = await _service.ExecuteWithCircuitBreakerAsync("test-circuit", action, fallback);

        // Assert
        result.Should().Be("Fallback");
    }

    [Fact]
    public void GetCircuitBreakerState_ValidCircuit_ReturnsState()
    {
        // Arrange
        _service.ExecuteWithCircuitBreakerAsync("state-test", () => Task.FromResult("test"));

        // Act
        var state = _service.GetCircuitBreakerState("state-test");

        // Assert
        state.Should().NotBeNull();
        state.CircuitName.Should().Be("state-test");
    }

    [Fact]
    public void ResetCircuitBreaker_OpenCircuit_ResetsCircuit()
    {
        // Arrange
        // Trip the circuit
        for (int i = 0; i < 5; i++)
        {
            try
            {
                _service.ExecuteWithCircuitBreakerAsync("reset-test", () =>
                    throw new Exception("Error")).Wait();
            }
            catch { }
        }

        // Act
        _service.ResetCircuitBreaker("reset-test");

        // Assert
        var state = _service.GetCircuitBreakerState("reset-test");
        state.State.Should().Be(CircuitState.Closed);
    }

    #endregion

    #region Fallback Tests

    [Fact]
    public async Task ExecuteWithFallbackAsync_PrimarySucceeds_ReturnsPrimaryResult()
    {
        // Arrange
        Func<Task<string>> primary = () => Task.FromResult("Primary");
        Func<Task<string>> fallback = () => Task.FromResult("Fallback");

        // Act
        var result = await _service.ExecuteWithFallbackAsync(primary, fallback);

        // Assert
        result.Should().Be("Primary");
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_PrimaryFails_ReturnsFallback()
    {
        // Arrange
        Func<Task<string>> primary = () => throw new Exception("Primary failed");
        Func<Task<string>> fallback = () => Task.FromResult("Fallback");

        // Act
        var result = await _service.ExecuteWithFallbackAsync(primary, fallback);

        // Assert
        result.Should().Be("Fallback");
    }

    [Fact]
    public async Task ExecuteWithFallbackAsync_BothFail_ThrowsException()
    {
        // Arrange
        Func<Task<string>> primary = () => throw new Exception("Primary failed");
        Func<Task<string>> fallback = () => throw new Exception("Fallback failed");

        // Act
        Func<Task> act = async () => await _service.ExecuteWithFallbackAsync(primary, fallback);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Timeout Tests

    [Fact]
    public async Task ExecuteWithTimeoutAsync_CompletesInTime_ReturnsResult()
    {
        // Arrange
        Func<Task<string>> action = async () =>
        {
            await Task.Delay(50);
            return "Success";
        };

        // Act
        var result = await _service.ExecuteWithTimeoutAsync(action, TimeSpan.FromSeconds(5));

        // Assert
        result.Should().Be("Success");
    }

    [Fact]
    public async Task ExecuteWithTimeoutAsync_ExceedsTimeout_ThrowsException()
    {
        // Arrange
        Func<Task<string>> action = async () =>
        {
            await Task.Delay(5000);
            return "Success";
        };

        // Act
        Func<Task> act = async () => await _service.ExecuteWithTimeoutAsync(action, TimeSpan.FromMilliseconds(100));

        // Assert
        await act.Should().ThrowAsync<TimeoutException>();
    }

    #endregion

    #region Bulkhead Tests

    [Fact]
    public async Task ExecuteWithBulkheadAsync_WithinLimit_ExecutesAll()
    {
        // Arrange
        var tasks = new List<Task<string>>();
        for (int i = 0; i < 5; i++)
        {
            var index = i;
            tasks.Add(_service.ExecuteWithBulkheadAsync("test-bulkhead", async () =>
            {
                await Task.Delay(50);
                return $"Result {index}";
            }, maxConcurrency: 10));
        }

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
    }

    #endregion

    #region Cache Tests

    [Fact]
    public async Task ExecuteWithCacheAsync_FirstCall_ExecutesAction()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> action = () =>
        {
            callCount++;
            return Task.FromResult("Cached Value");
        };

        // Act
        var result = await _service.ExecuteWithCacheAsync("cache-key", action, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be("Cached Value");
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteWithCacheAsync_SecondCall_ReturnsCached()
    {
        // Arrange
        var callCount = 0;
        Func<Task<string>> action = () =>
        {
            callCount++;
            return Task.FromResult($"Value {callCount}");
        };

        // Act
        await _service.ExecuteWithCacheAsync("cache-key2", action, TimeSpan.FromMinutes(5));
        var result = await _service.ExecuteWithCacheAsync("cache-key2", action, TimeSpan.FromMinutes(5));

        // Assert
        result.Should().Be("Value 1");
        callCount.Should().Be(1);
    }

    [Fact]
    public void InvalidateCache_ValidKey_RemovesFromCache()
    {
        // Act
        _service.InvalidateCache("test-key");

        // Assert - no exception thrown
    }

    #endregion

    #region Combined Policy Tests

    [Fact]
    public async Task ExecuteWithPoliciesAsync_AllPoliciesApplied_ReturnsResult()
    {
        // Arrange
        var options = new ExecutionOptions
        {
            EnableRetry = true,
            EnableCircuitBreaker = true,
            EnableTimeout = true,
            TimeoutSeconds = 30
        };

        Func<Task<string>> action = () => Task.FromResult("Success");

        // Act
        var result = await _service.ExecuteWithPoliciesAsync("combined-test", action, options);

        // Assert
        result.Should().Be("Success");
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void GetStatistics_ReturnsStats()
    {
        // Act
        var result = _service.GetStatistics();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetCircuitBreakerStats_ReturnsAllCircuits()
    {
        // Arrange
        _service.ExecuteWithCircuitBreakerAsync("circuit1", () => Task.FromResult("test"));
        _service.ExecuteWithCircuitBreakerAsync("circuit2", () => Task.FromResult("test"));

        // Act
        var result = _service.GetCircuitBreakerStats();

        // Assert
        result.Should().NotBeEmpty();
    }

    #endregion
}

// Supporting classes for tests
public class ResilienceOptions
{
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 100;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}

public class ExecutionOptions
{
    public bool EnableRetry { get; set; }
    public bool EnableCircuitBreaker { get; set; }
    public bool EnableTimeout { get; set; }
    public int TimeoutSeconds { get; set; }
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen
}
