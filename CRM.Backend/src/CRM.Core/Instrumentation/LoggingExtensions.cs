// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;

namespace CRM.Core.Instrumentation;

/// <summary>
/// Structured logging extensions for consistent log formatting across the application.
/// Provides semantic logging with context and correlation.
/// </summary>
public static class LoggingExtensions
{
    #region Controller Logging

    public static void LogControllerEntry(this ILogger logger, string controller, string action, object? parameters = null)
    {
        logger.LogInformation(
            "📥 [{Controller}] → {Action} | Params: {Params}",
            controller, action, parameters ?? "none");
    }

    public static void LogControllerExit(this ILogger logger, string controller, string action, int statusCode, long durationMs)
    {
        var icon = statusCode < 400 ? "📤" : statusCode < 500 ? "⚠️" : "❌";
        logger.LogInformation(
            "{Icon} [{Controller}] ← {Action} | Status: {StatusCode} | Duration: {Duration}ms",
            icon, controller, action, statusCode, durationMs);
    }

    public static void LogControllerError(this ILogger logger, string controller, string action, Exception ex)
    {
        logger.LogError(ex,
            "❌ [{Controller}] ✖ {Action} | Error: {ErrorType} - {Message}",
            controller, action, ex.GetType().Name, ex.Message);
    }

    #endregion

    #region Service Logging

    public static void LogServiceOperation(this ILogger logger, string service, string operation, object? context = null)
    {
        logger.LogDebug(
            "🔧 [{Service}] {Operation} | Context: {Context}",
            service, operation, context ?? "none");
    }

    public static void LogServiceSuccess(this ILogger logger, string service, string operation, object? result = null)
    {
        logger.LogDebug(
            "✅ [{Service}] {Operation} completed | Result: {Result}",
            service, operation, result ?? "success");
    }

    public static void LogServiceError(this ILogger logger, string service, string operation, Exception ex)
    {
        logger.LogError(ex,
            "❌ [{Service}] {Operation} failed | Error: {ErrorType} - {Message}",
            service, operation, ex.GetType().Name, ex.Message);
    }

    public static void LogServiceWarning(this ILogger logger, string service, string operation, string warning)
    {
        logger.LogWarning(
            "⚠️ [{Service}] {Operation} | Warning: {Warning}",
            service, operation, warning);
    }

    #endregion

    #region Database Logging

    public static void LogDatabaseQuery(this ILogger logger, string table, string operation, object? parameters = null)
    {
        logger.LogDebug(
            "💾 [DB] {Operation} on {Table} | Params: {Params}",
            operation, table, parameters ?? "none");
    }

    public static void LogDatabaseQueryResult(this ILogger logger, string table, string operation, int recordCount, long durationMs)
    {
        logger.LogDebug(
            "💾 [DB] {Operation} on {Table} | Records: {Count} | Duration: {Duration}ms",
            operation, table, recordCount, durationMs);
    }

    public static void LogDatabaseError(this ILogger logger, string table, string operation, Exception ex)
    {
        logger.LogError(ex,
            "❌ [DB] {Operation} on {Table} failed | Error: {ErrorType} - {Message}",
            operation, table, ex.GetType().Name, ex.Message);
    }

    public static void LogDatabaseSlowQuery(this ILogger logger, string table, string operation, long durationMs)
    {
        logger.LogWarning(
            "🐢 [DB] SLOW QUERY: {Operation} on {Table} took {Duration}ms",
            operation, table, durationMs);
    }

    #endregion

    #region Authentication Logging

    public static void LogAuthAttempt(this ILogger logger, string username, string method)
    {
        logger.LogInformation(
            "🔐 [Auth] Login attempt | User: {Username} | Method: {Method}",
            username, method);
    }

    public static void LogAuthSuccess(this ILogger logger, string username, int userId)
    {
        logger.LogInformation(
            "✅ [Auth] Login successful | User: {Username} | UserId: {UserId}",
            username, userId);
    }

    public static void LogAuthFailure(this ILogger logger, string username, string reason)
    {
        logger.LogWarning(
            "❌ [Auth] Login failed | User: {Username} | Reason: {Reason}",
            username, reason);
    }

    public static void LogAuthLogout(this ILogger logger, string username, int userId)
    {
        logger.LogInformation(
            "🚪 [Auth] Logout | User: {Username} | UserId: {UserId}",
            username, userId);
    }

    #endregion

    #region Performance Logging

    public static void LogPerformanceMetric(this ILogger logger, string metric, double value, string unit = "ms")
    {
        logger.LogDebug(
            "📊 [Perf] {Metric}: {Value}{Unit}",
            metric, value, unit);
    }

    public static void LogPerformanceWarning(this ILogger logger, string operation, long thresholdMs, long actualMs)
    {
        logger.LogWarning(
            "⚠️ [Perf] {Operation} exceeded threshold | Threshold: {Threshold}ms | Actual: {Actual}ms",
            operation, thresholdMs, actualMs);
    }

    #endregion

    #region Business Logic Logging

    public static void LogBusinessEvent(this ILogger logger, string eventType, string entity, int? entityId = null, object? details = null)
    {
        logger.LogInformation(
            "📋 [Business] {EventType} | Entity: {Entity} | Id: {EntityId} | Details: {Details}",
            eventType, entity, entityId?.ToString() ?? "N/A", details ?? "none");
    }

    public static void LogWorkflowExecution(this ILogger logger, string workflowName, int workflowId, string status)
    {
        logger.LogInformation(
            "⚡ [Workflow] {WorkflowName} (ID: {WorkflowId}) | Status: {Status}",
            workflowName, workflowId, status);
    }

    public static void LogCommunication(this ILogger logger, string channelType, string direction, string recipient, string status)
    {
        logger.LogInformation(
            "📧 [Communication] {ChannelType} | Direction: {Direction} | To: {Recipient} | Status: {Status}",
            channelType, direction, recipient, status);
    }

    #endregion

    #region Integration Logging

    public static void LogExternalCall(this ILogger logger, string service, string endpoint, string method)
    {
        logger.LogDebug(
            "🌐 [External] Calling {Service} | {Method} {Endpoint}",
            service, method, endpoint);
    }

    public static void LogExternalResponse(this ILogger logger, string service, int statusCode, long durationMs)
    {
        var icon = statusCode < 400 ? "✅" : "❌";
        logger.LogDebug(
            "{Icon} [External] {Service} responded | Status: {StatusCode} | Duration: {Duration}ms",
            icon, service, statusCode, durationMs);
    }

    #endregion

    #region System Logging

    public static void LogSystemStartup(this ILogger logger, string component, string version, string environment)
    {
        logger.LogInformation(
            "🚀 [System] {Component} v{Version} starting in {Environment} mode",
            component, version, environment);
    }

    public static void LogSystemShutdown(this ILogger logger, string component)
    {
        logger.LogInformation(
            "🛑 [System] {Component} shutting down",
            component);
    }

    public static void LogHealthCheck(this ILogger logger, string component, bool isHealthy, string? details = null)
    {
        var icon = isHealthy ? "💚" : "❤️";
        logger.Log(isHealthy ? LogLevel.Information : LogLevel.Warning,
            "{Icon} [Health] {Component} | Status: {Status} | Details: {Details}",
            icon, component, isHealthy ? "Healthy" : "Unhealthy", details ?? "none");
    }

    #endregion
}
