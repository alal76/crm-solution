// CRM Solution - Performance Test Harness
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under GNU AGPL v3

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CRM.Tests.Performance;

/// <summary>
/// Performance Test Harness for CRM Solution
/// Provides load testing, stress testing, and performance benchmarking capabilities.
/// </summary>
public class PerformanceTestHarness
{
    private readonly HttpClient _httpClient;
    private readonly PerformanceTestConfig _config;
    private readonly ConcurrentBag<RequestMetric> _metrics;
    private string? _authToken;

    public PerformanceTestHarness(PerformanceTestConfig config)
    {
        _config = config;
        _metrics = new ConcurrentBag<RequestMetric>();
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(config.RequestTimeoutSeconds)
        };
    }

    #region Authentication

    public async Task<bool> AuthenticateAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                email = _config.TestUserEmail,
                password = _config.TestUserPassword
            });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                _authToken = result.GetProperty("accessToken").GetString();
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", _authToken);
                Console.WriteLine("✓ Authentication successful");
                return true;
            }
            
            Console.WriteLine($"✗ Authentication failed: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Authentication error: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region Test Execution

    public async Task<PerformanceTestResult> RunLoadTestAsync(
        string testName,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        int concurrentUsers,
        int requestsPerUser)
    {
        Console.WriteLine($"\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"Load Test: {testName}");
        Console.WriteLine($"Concurrent Users: {concurrentUsers}, Requests/User: {requestsPerUser}");
        Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        _metrics.Clear();
        var stopwatch = Stopwatch.StartNew();

        var tasks = Enumerable.Range(0, concurrentUsers)
            .Select(userId => SimulateUserAsync(userId, requestsPerUser, requestFunc))
            .ToArray();

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        return GenerateResult(testName, stopwatch.Elapsed, concurrentUsers, requestsPerUser);
    }

    public async Task<PerformanceTestResult> RunStressTestAsync(
        string testName,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        int maxConcurrentUsers,
        int rampUpSeconds)
    {
        Console.WriteLine($"\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"Stress Test: {testName}");
        Console.WriteLine($"Max Concurrent Users: {maxConcurrentUsers}, Ramp-up: {rampUpSeconds}s");
        Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        _metrics.Clear();
        var stopwatch = Stopwatch.StartNew();
        var cts = new CancellationTokenSource();
        var userTasks = new List<Task>();
        var userCount = 0;
        var delayPerUser = (rampUpSeconds * 1000) / maxConcurrentUsers;

        // Ramp up users gradually
        for (int i = 0; i < maxConcurrentUsers; i++)
        {
            userCount++;
            var userId = i;
            userTasks.Add(Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await ExecuteRequestAsync(userId, requestFunc);
                    await Task.Delay(100); // Small delay between requests
                }
            }));
            
            Console.WriteLine($"  → User {userCount} started");
            await Task.Delay(delayPerUser);
        }

        // Run at max capacity for additional time
        await Task.Delay(TimeSpan.FromSeconds(30));
        
        cts.Cancel();
        
        try { await Task.WhenAll(userTasks); }
        catch (OperationCanceledException) { }
        
        stopwatch.Stop();

        return GenerateResult(testName, stopwatch.Elapsed, maxConcurrentUsers, _metrics.Count);
    }

    public async Task<PerformanceTestResult> RunEnduranceTestAsync(
        string testName,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        int concurrentUsers,
        int durationMinutes)
    {
        Console.WriteLine($"\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine($"Endurance Test: {testName}");
        Console.WriteLine($"Concurrent Users: {concurrentUsers}, Duration: {durationMinutes} minutes");
        Console.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        _metrics.Clear();
        var stopwatch = Stopwatch.StartNew();
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(durationMinutes));
        
        var tasks = Enumerable.Range(0, concurrentUsers)
            .Select(userId => Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    await ExecuteRequestAsync(userId, requestFunc);
                    await Task.Delay(500); // Steady load
                }
            }))
            .ToArray();

        try { await Task.WhenAll(tasks); }
        catch (OperationCanceledException) { }
        
        stopwatch.Stop();

        return GenerateResult(testName, stopwatch.Elapsed, concurrentUsers, _metrics.Count);
    }

    private async Task SimulateUserAsync(
        int userId,
        int requestCount,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc)
    {
        for (int i = 0; i < requestCount; i++)
        {
            await ExecuteRequestAsync(userId, requestFunc);
            
            // Random think time between requests (50-200ms)
            await Task.Delay(Random.Shared.Next(50, 200));
        }
    }

    private async Task ExecuteRequestAsync(
        int userId,
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc)
    {
        var metric = new RequestMetric
        {
            UserId = userId,
            StartTime = DateTime.UtcNow
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await requestFunc(_httpClient);
            stopwatch.Stop();

            metric.DurationMs = stopwatch.ElapsedMilliseconds;
            metric.StatusCode = (int)response.StatusCode;
            metric.IsSuccess = response.IsSuccessStatusCode;
            metric.ResponseSize = response.Content.Headers.ContentLength ?? 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            metric.DurationMs = stopwatch.ElapsedMilliseconds;
            metric.IsSuccess = false;
            metric.Error = ex.Message;
        }

        _metrics.Add(metric);

        // Progress indicator
        if (_metrics.Count % 100 == 0)
        {
            Console.Write(".");
        }
    }

    #endregion

    #region Results

    private PerformanceTestResult GenerateResult(
        string testName,
        TimeSpan totalDuration,
        int concurrentUsers,
        int totalRequests)
    {
        var metricsList = _metrics.ToList();
        var successfulRequests = metricsList.Where(m => m.IsSuccess).ToList();
        var failedRequests = metricsList.Where(m => !m.IsSuccess).ToList();
        var durations = successfulRequests.Select(m => m.DurationMs).OrderBy(d => d).ToList();

        var result = new PerformanceTestResult
        {
            TestName = testName,
            StartTime = metricsList.Min(m => m.StartTime),
            EndTime = metricsList.Max(m => m.StartTime).AddMilliseconds(metricsList.Max(m => m.DurationMs)),
            TotalDuration = totalDuration,
            ConcurrentUsers = concurrentUsers,
            TotalRequests = metricsList.Count,
            SuccessfulRequests = successfulRequests.Count,
            FailedRequests = failedRequests.Count,
            RequestsPerSecond = metricsList.Count / totalDuration.TotalSeconds,
            
            // Latency metrics
            AvgResponseTimeMs = durations.Count > 0 ? durations.Average() : 0,
            MinResponseTimeMs = durations.Count > 0 ? durations.Min() : 0,
            MaxResponseTimeMs = durations.Count > 0 ? durations.Max() : 0,
            MedianResponseTimeMs = durations.Count > 0 ? durations[durations.Count / 2] : 0,
            P90ResponseTimeMs = durations.Count > 0 ? durations[(int)(durations.Count * 0.90)] : 0,
            P95ResponseTimeMs = durations.Count > 0 ? durations[(int)(durations.Count * 0.95)] : 0,
            P99ResponseTimeMs = durations.Count > 0 ? durations[(int)(durations.Count * 0.99)] : 0,
            
            // Throughput
            TotalBytesTransferred = successfulRequests.Sum(m => m.ResponseSize),
            SuccessRate = metricsList.Count > 0 ? (double)successfulRequests.Count / metricsList.Count * 100 : 0,
            
            // Error analysis
            ErrorsByType = failedRequests
                .GroupBy(m => m.Error ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };

        PrintResult(result);
        return result;
    }

    private void PrintResult(PerformanceTestResult result)
    {
        Console.WriteLine("\n");
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║  Performance Test Results: {result.TestName,-32} ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  Duration:           {result.TotalDuration.TotalSeconds:F2}s                              ║");
        Console.WriteLine($"║  Concurrent Users:   {result.ConcurrentUsers,-10}                        ║");
        Console.WriteLine($"║  Total Requests:     {result.TotalRequests,-10}                        ║");
        Console.WriteLine($"║  Successful:         {result.SuccessfulRequests,-10} ({result.SuccessRate:F1}%)              ║");
        Console.WriteLine($"║  Failed:             {result.FailedRequests,-10}                        ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  THROUGHPUT                                                  ║");
        Console.WriteLine($"║  Requests/sec:       {result.RequestsPerSecond:F2}                              ║");
        Console.WriteLine($"║  Data transferred:   {result.TotalBytesTransferred / 1024.0:F2} KB                       ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  LATENCY (ms)                                                ║");
        Console.WriteLine($"║  Min:                {result.MinResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  Avg:                {result.AvgResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  Median:             {result.MedianResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  Max:                {result.MaxResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  P90:                {result.P90ResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  P95:                {result.P95ResponseTimeMs,6:F0}                              ║");
        Console.WriteLine($"║  P99:                {result.P99ResponseTimeMs,6:F0}                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");

        // Print grade
        var grade = GetPerformanceGrade(result);
        var color = grade switch
        {
            "A" or "A+" => ConsoleColor.Green,
            "B" or "B+" => ConsoleColor.Yellow,
            "C" or "C+" => ConsoleColor.DarkYellow,
            _ => ConsoleColor.Red
        };
        
        Console.ForegroundColor = color;
        Console.WriteLine($"\n  Performance Grade: {grade}");
        Console.ResetColor();
    }

    private string GetPerformanceGrade(PerformanceTestResult result)
    {
        // Scoring based on P95 latency and success rate
        var latencyScore = result.P95ResponseTimeMs switch
        {
            <= 100 => 100,
            <= 200 => 90,
            <= 500 => 75,
            <= 1000 => 60,
            <= 2000 => 40,
            _ => 20
        };

        var successScore = result.SuccessRate switch
        {
            >= 99.9 => 100,
            >= 99.5 => 90,
            >= 99 => 80,
            >= 95 => 60,
            >= 90 => 40,
            _ => 20
        };

        var totalScore = (latencyScore * 0.6 + successScore * 0.4);

        return totalScore switch
        {
            >= 95 => "A+",
            >= 90 => "A",
            >= 85 => "B+",
            >= 80 => "B",
            >= 75 => "C+",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }

    #endregion

    #region Predefined Test Scenarios

    public async Task<List<PerformanceTestResult>> RunAllBenchmarksAsync()
    {
        var results = new List<PerformanceTestResult>();

        if (!await AuthenticateAsync()) return results;

        // API Health Check
        results.Add(await RunLoadTestAsync(
            "Health Check",
            client => client.GetAsync("/api/health"),
            10, 100));

        // Get Customers List
        results.Add(await RunLoadTestAsync(
            "Get Customers",
            client => client.GetAsync("/api/customers"),
            20, 50));

        // Get Dashboard
        results.Add(await RunLoadTestAsync(
            "Get Dashboard",
            client => client.GetAsync("/api/dashboard/stats"),
            20, 50));

        // Get Contacts
        results.Add(await RunLoadTestAsync(
            "Get Contacts",
            client => client.GetAsync("/api/contacts"),
            20, 50));

        // Get Opportunities
        results.Add(await RunLoadTestAsync(
            "Get Opportunities",
            client => client.GetAsync("/api/opportunities"),
            20, 50));

        // Mixed workload stress test
        results.Add(await RunStressTestAsync(
            "Mixed Workload Stress",
            async client =>
            {
                var endpoints = new[]
                {
                    "/api/customers",
                    "/api/contacts",
                    "/api/opportunities",
                    "/api/leads",
                    "/api/campaigns"
                };
                var endpoint = endpoints[Random.Shared.Next(endpoints.Length)];
                return await client.GetAsync(endpoint);
            },
            50, 30));

        PrintSummary(results);
        return results;
    }

    private void PrintSummary(List<PerformanceTestResult> results)
    {
        Console.WriteLine("\n\n");
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                     PERFORMANCE TEST SUMMARY                             ║");
        Console.WriteLine("╠═══════════════════════════╦═════════╦══════════╦══════════╦══════════════╣");
        Console.WriteLine("║ Test                      ║ RPS     ║ Avg(ms)  ║ P95(ms)  ║ Success Rate ║");
        Console.WriteLine("╠═══════════════════════════╬═════════╬══════════╬══════════╬══════════════╣");
        
        foreach (var result in results)
        {
            Console.WriteLine($"║ {result.TestName,-25} ║ {result.RequestsPerSecond,7:F1} ║ {result.AvgResponseTimeMs,8:F0} ║ {result.P95ResponseTimeMs,8:F0} ║ {result.SuccessRate,10:F1}% ║");
        }
        
        Console.WriteLine("╚═══════════════════════════╩═════════╩══════════╩══════════╩══════════════╝");
    }

    #endregion
}

#region Models

public class PerformanceTestConfig
{
    public string BaseUrl { get; set; } = "http://localhost:5000";
    public string TestUserEmail { get; set; } = "abhi.lal@gmail.com";
    public string TestUserPassword { get; set; } = "Admin@123";
    public int RequestTimeoutSeconds { get; set; } = 30;
    public int DefaultConcurrentUsers { get; set; } = 10;
    public int DefaultRequestsPerUser { get; set; } = 50;
}

public class RequestMetric
{
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public long DurationMs { get; set; }
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public long ResponseSize { get; set; }
    public string? Error { get; set; }
}

public class PerformanceTestResult
{
    public string TestName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int ConcurrentUsers { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double RequestsPerSecond { get; set; }
    
    // Latency
    public double AvgResponseTimeMs { get; set; }
    public double MinResponseTimeMs { get; set; }
    public double MaxResponseTimeMs { get; set; }
    public double MedianResponseTimeMs { get; set; }
    public double P90ResponseTimeMs { get; set; }
    public double P95ResponseTimeMs { get; set; }
    public double P99ResponseTimeMs { get; set; }
    
    // Throughput
    public long TotalBytesTransferred { get; set; }
    public double SuccessRate { get; set; }
    
    // Errors
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}

#endregion
