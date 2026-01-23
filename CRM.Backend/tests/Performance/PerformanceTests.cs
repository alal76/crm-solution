// CRM Solution - Performance Tests
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under GNU AGPL v3

using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;

namespace CRM.Tests.Performance;

/// <summary>
/// Performance test suite for CRM Solution.
/// These tests measure response times, throughput, and scalability.
/// </summary>
[Collection("Performance")]
public class PerformanceTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly PerformanceTestHarness _harness;
    private readonly PerformanceTestConfig _config;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _config = new PerformanceTestConfig
        {
            BaseUrl = Environment.GetEnvironmentVariable("CRM_API_URL") ?? "http://localhost:5000",
            TestUserEmail = Environment.GetEnvironmentVariable("CRM_TEST_EMAIL") ?? "abhi.lal@gmail.com",
            TestUserPassword = Environment.GetEnvironmentVariable("CRM_TEST_PASSWORD") ?? "Admin@123"
        };
        _harness = new PerformanceTestHarness(_config);
    }

    public async Task InitializeAsync()
    {
        var authenticated = await _harness.AuthenticateAsync();
        if (!authenticated)
        {
            _output.WriteLine("⚠️ Authentication failed - tests may fail");
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Load Tests

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Load")]
    public async Task LoadTest_GetCustomers_Should_HandleConcurrentUsers()
    {
        // Arrange
        var concurrentUsers = 20;
        var requestsPerUser = 50;

        // Act
        var result = await _harness.RunLoadTestAsync(
            "Get Customers Load Test",
            client => client.GetAsync("/api/customers"),
            concurrentUsers,
            requestsPerUser);

        // Assert
        Assert.True(result.SuccessRate >= 99.0, $"Success rate {result.SuccessRate}% is below 99%");
        Assert.True(result.P95ResponseTimeMs <= 1000, $"P95 response time {result.P95ResponseTimeMs}ms exceeds 1000ms");

        _output.WriteLine($"Requests/sec: {result.RequestsPerSecond:F2}");
        _output.WriteLine($"Avg Response: {result.AvgResponseTimeMs:F0}ms");
        _output.WriteLine($"P95 Response: {result.P95ResponseTimeMs:F0}ms");
    }

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Load")]
    public async Task LoadTest_GetContacts_Should_HandleConcurrentUsers()
    {
        var result = await _harness.RunLoadTestAsync(
            "Get Contacts Load Test",
            client => client.GetAsync("/api/contacts"),
            20, 50);

        Assert.True(result.SuccessRate >= 99.0);
        Assert.True(result.P95ResponseTimeMs <= 1000);
    }

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Load")]
    public async Task LoadTest_GetOpportunities_Should_HandleConcurrentUsers()
    {
        var result = await _harness.RunLoadTestAsync(
            "Get Opportunities Load Test",
            client => client.GetAsync("/api/opportunities"),
            20, 50);

        Assert.True(result.SuccessRate >= 99.0);
        Assert.True(result.P95ResponseTimeMs <= 1000);
    }

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Load")]
    public async Task LoadTest_Dashboard_Should_HandleConcurrentUsers()
    {
        var result = await _harness.RunLoadTestAsync(
            "Dashboard Load Test",
            client => client.GetAsync("/api/dashboard/stats"),
            30, 100);

        Assert.True(result.SuccessRate >= 99.0);
        Assert.True(result.P95ResponseTimeMs <= 2000); // Dashboard may be slower
    }

    #endregion

    #region Stress Tests

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Stress")]
    public async Task StressTest_MixedWorkload_Should_HandleGradualRampUp()
    {
        var endpoints = new[]
        {
            "/api/customers",
            "/api/contacts", 
            "/api/opportunities",
            "/api/leads",
            "/api/campaigns"
        };

        var result = await _harness.RunStressTestAsync(
            "Mixed Workload Stress Test",
            async client =>
            {
                var endpoint = endpoints[Random.Shared.Next(endpoints.Length)];
                return await client.GetAsync(endpoint);
            },
            maxConcurrentUsers: 50,
            rampUpSeconds: 30);

        Assert.True(result.SuccessRate >= 95.0, $"Success rate {result.SuccessRate}% is below 95% under stress");
        
        _output.WriteLine($"Max concurrent users: {result.ConcurrentUsers}");
        _output.WriteLine($"Total requests: {result.TotalRequests}");
        _output.WriteLine($"Success rate: {result.SuccessRate:F1}%");
    }

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Stress")]
    public async Task StressTest_Authentication_Should_HandleBurstLoad()
    {
        // Test authentication endpoint under burst load
        var result = await _harness.RunLoadTestAsync(
            "Authentication Burst Test",
            async client =>
            {
                var tempClient = new HttpClient { BaseAddress = client.BaseAddress };
                return await tempClient.PostAsJsonAsync("/api/auth/login", new
                {
                    email = _config.TestUserEmail,
                    password = _config.TestUserPassword
                });
            },
            concurrentUsers: 10,
            requestsPerUser: 5);

        // Auth should handle burst but may have rate limiting
        Assert.True(result.SuccessRate >= 50, "Auth endpoint completely overwhelmed");
    }

    #endregion

    #region Endurance Tests

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Endurance")]
    public async Task EnduranceTest_SteadyLoad_Should_MaintainPerformance()
    {
        var result = await _harness.RunEnduranceTestAsync(
            "Steady Load Endurance Test",
            client => client.GetAsync("/api/customers"),
            concurrentUsers: 10,
            durationMinutes: 5);

        Assert.True(result.SuccessRate >= 99.0, "Performance degraded during endurance test");
        Assert.True(result.P99ResponseTimeMs <= 5000, "Tail latency too high");
    }

    #endregion

    #region Benchmark Suite

    [Fact(Skip = "Performance test - run manually")]
    [Trait("Category", "Performance")]
    [Trait("Type", "Benchmark")]
    public async Task Benchmark_AllEndpoints_Should_MeetSLA()
    {
        var results = await _harness.RunAllBenchmarksAsync();

        foreach (var result in results)
        {
            _output.WriteLine($"{result.TestName}: RPS={result.RequestsPerSecond:F1}, P95={result.P95ResponseTimeMs:F0}ms, Success={result.SuccessRate:F1}%");
        }

        // Verify all tests meet minimum SLA
        Assert.All(results, r => Assert.True(r.SuccessRate >= 95.0, $"{r.TestName} failed SLA"));
    }

    #endregion

    #region Baseline Tests

    [Fact]
    [Trait("Category", "Performance")]
    [Trait("Type", "Baseline")]
    public async Task Baseline_SingleRequest_Should_BeUnder500ms()
    {
        // This test runs during normal test execution to establish baseline
        var result = await _harness.RunLoadTestAsync(
            "Single Request Baseline",
            client => client.GetAsync("/api/customers"),
            concurrentUsers: 1,
            requestsPerUser: 10);

        Assert.True(result.AvgResponseTimeMs <= 500, 
            $"Average response time {result.AvgResponseTimeMs}ms exceeds 500ms baseline");
    }

    #endregion
}
