// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under GNU AGPL v3

using CRM.Core.Instrumentation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CRM.Api.Middleware;

/// <summary>
/// Middleware for request/response logging, timing, and instrumentation.
/// Provides extensive debugging info in Dev/Debug mode.
/// </summary>
public class InstrumentationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InstrumentationMiddleware> _logger;
    private readonly bool _isVerbose;

    public InstrumentationMiddleware(
        RequestDelegate next,
        ILogger<InstrumentationMiddleware> logger,
        bool isVerbose = false)
    {
        _next = next;
        _logger = logger;
        _isVerbose = isVerbose;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();
        
        // Start activity for distributed tracing
        using var activity = InstrumentationService.StartActivity(
            $"HTTP {context.Request.Method} {context.Request.Path}",
            ActivityKind.Server);
        
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("http.request_id", requestId);
        
        // Add request ID to response headers for tracing
        context.Response.Headers["X-Request-Id"] = requestId;
        context.Response.Headers["X-Trace-Id"] = activity?.TraceId.ToString() ?? requestId;

        try
        {
            // Log request
            if (_isVerbose)
            {
                _logger.LogInformation(
                    "[{RequestId}] → {Method} {Path} | Query: {Query} | User: {User}",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString,
                    context.User.Identity?.Name ?? "anonymous");

                // Log headers in verbose mode
                foreach (var header in context.Request.Headers.Where(h => !h.Key.StartsWith("Authorization")))
                {
                    _logger.LogDebug("[{RequestId}] Header: {Key}: {Value}", requestId, header.Key, header.Value);
                }
            }
            else
            {
                _logger.LogInformation("[{RequestId}] → {Method} {Path}", 
                    requestId, context.Request.Method, context.Request.Path);
            }

            await _next(context);

            stopwatch.Stop();
            
            // Record metrics
            InstrumentationService.RecordMetric("http.request.duration_ms", stopwatch.ElapsedMilliseconds);
            InstrumentationService.RecordMetric($"http.request.{context.Response.StatusCode}", 1);

            activity?.SetTag("http.status_code", context.Response.StatusCode);

            // Log response
            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                          context.Response.StatusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel,
                "[{RequestId}] ← {StatusCode} | Duration: {Duration}ms",
                requestId,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            // Performance warning for slow requests
            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                _logger.LogWarning(
                    "[{RequestId}] ⚠️ SLOW REQUEST: {Method} {Path} took {Duration}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            activity?.SetTag("error", true);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            
            InstrumentationService.RecordMetric("http.request.errors", 1);

            _logger.LogError(ex,
                "[{RequestId}] ✖ ERROR: {Method} {Path} | Duration: {Duration}ms | Error: {Error}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            if (_isVerbose)
            {
                _logger.LogError("[{RequestId}] Stack Trace: {StackTrace}", requestId, ex.StackTrace);
            }

            throw;
        }
    }
}

/// <summary>
/// Extension methods for adding instrumentation middleware
/// </summary>
public static class InstrumentationMiddlewareExtensions
{
    public static IApplicationBuilder UseInstrumentation(
        this IApplicationBuilder builder,
        bool verbose = false)
    {
        return builder.UseMiddleware<InstrumentationMiddleware>(verbose);
    }
}
