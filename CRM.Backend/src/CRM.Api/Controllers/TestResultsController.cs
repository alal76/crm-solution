// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for serving test results from the latest test run.
/// Used by the Test Results Dashboard UI to display test outcomes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TestResultsController : CrmControllerBase
{
    private readonly ILogger<TestResultsController> _logger;
    private readonly IWebHostEnvironment _env;

    public TestResultsController(ILogger<TestResultsController> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    /// <summary>
    /// Get the latest test results
    /// </summary>
    /// <returns>Test run summary with all test results</returns>
    [HttpGet("latest")]
    [AllowAnonymous] // Allow testing dashboard access
    public async Task<IActionResult> GetLatestResults()
    {
                // Construct path: repo-root/logs/test-results/latest-test-results.json
        var repoRoot = _env.ContentRootPath.Split("CRM.Api").FirstOrDefault() ?? _env.ContentRootPath;
        var logFilePath = Path.Combine(repoRoot, "logs", "test-results", "latest-test-results.json");

        if (!System.IO.File.Exists(logFilePath))
        {
            _logger.LogWarning("Test results file not found: {Path}", logFilePath);
            return NotFound(new { message = "No test results available yet. Run tests to generate results." });
        }

        var json = await System.IO.File.ReadAllTextAsync(logFilePath);
        var results = JsonSerializer.Deserialize<object>(json);

        return Ok(results);
    }

    /// <summary>
    /// Get test results by session ID
    /// </summary>
    /// <param name="sessionId">The session ID</param>
    /// <returns>Test run summary for the specified session</returns>
    [HttpGet("session/{sessionId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResultsBySession(string sessionId)
    {
                var repoRoot = _env.ContentRootPath.Split("CRM.Api").FirstOrDefault() ?? _env.ContentRootPath;
        var logFilePath = Path.Combine(repoRoot, "logs", "test-results", $"test-results-{sessionId}.json");

        if (!System.IO.File.Exists(logFilePath))
        {
            return NotFound(new { message = $"Test results for session {sessionId} not found" });
        }

        var json = await System.IO.File.ReadAllTextAsync(logFilePath);
        var results = JsonSerializer.Deserialize<object>(json);

        return Ok(results);
    }

    /// <summary>
    /// Get list of all available test result sessions
    /// </summary>
    /// <returns>List of available session files</returns>
    [HttpGet("sessions")]
    [AllowAnonymous]
    public IActionResult GetAvailableSessions()
    {
                var repoRoot = _env.ContentRootPath.Split("CRM.Api").FirstOrDefault() ?? _env.ContentRootPath;
        var logsDir = Path.Combine(repoRoot, "logs", "test-results");

        if (!Directory.Exists(logsDir))
        {
            return Ok(new { sessions = new List<string>() });
        }

        var files = Directory.GetFiles(logsDir, "test-results-*.json")
            .OrderByDescending(f => f)
            .Select(f => Path.GetFileNameWithoutExtension(f).Replace("test-results-", ""))
            .ToList();

        return Ok(new { sessions = files, count = files.Count });
    }
}
