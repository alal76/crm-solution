// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Microsoft.EntityFrameworkCore;
using CRM.Core.Exceptions;
using System.Text.Json;

namespace CRM.Api.Middleware;

/// <summary>
/// Error handling middleware - handles exceptions and converts to appropriate HTTP responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (CrmException ex)
        {
            // Handle custom CRM exceptions with appropriate status codes
            _logger.LogWarning(ex, "CRM exception {ErrorCode}: {Message} for request {Method} {Path}",
                ex.ErrorCode, ex.Message, context.Request.Method, context.Request.Path);

            context.Response.StatusCode = (int)ex.StatusCode;
            context.Response.ContentType = "application/json";

            var errorResponse = CreateErrorResponse(ex, context);
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Handle optimistic concurrency conflicts
            _logger.LogWarning(ex, "Concurrency conflict detected for request {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var conflictResponse = new ConcurrencyConflictResponse
            {
                Message = "The record was modified by another user. Please refresh and try again.",
                ConflictType = "ConcurrencyConflict",
                Timestamp = DateTime.UtcNow,
                RequestPath = context.Request.Path,
                EntityInfo = ex.Entries.Select(e => new EntityConflictInfo
                {
                    EntityType = e.Entity.GetType().Name,
                    State = e.State.ToString()
                }).ToList()
            };

            await context.Response.WriteAsJsonAsync(conflictResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Internal server error",
                errorCode = "INTERNAL_ERROR",
                timestamp = DateTime.UtcNow
            });
        }
    }

    private static object CreateErrorResponse(CrmException ex, HttpContext context)
    {
        var baseResponse = new
        {
            message = ex.Message,
            errorCode = ex.ErrorCode,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        return ex switch
        {
            ValidationException validationEx => new
            {
                baseResponse.message,
                baseResponse.errorCode,
                baseResponse.timestamp,
                baseResponse.path,
                errors = validationEx.Errors
            },
            EntityNotFoundException notFoundEx => new
            {
                baseResponse.message,
                baseResponse.errorCode,
                baseResponse.timestamp,
                baseResponse.path,
                entityType = notFoundEx.EntityType,
                entityId = notFoundEx.EntityId
            },
            RateLimitException rateLimitEx => new
            {
                baseResponse.message,
                baseResponse.errorCode,
                baseResponse.timestamp,
                baseResponse.path,
                retryAfterSeconds = rateLimitEx.RetryAfterSeconds
            },
            DuplicateEntityException duplicateEx => new
            {
                baseResponse.message,
                baseResponse.errorCode,
                baseResponse.timestamp,
                baseResponse.path,
                entityType = duplicateEx.EntityType,
                duplicateIds = duplicateEx.DuplicateIds
            },
            _ => baseResponse
        };
    }
}

/// <summary>
/// Response model for concurrency conflicts
/// </summary>
public class ConcurrencyConflictResponse
{
    public string Message { get; set; } = string.Empty;
    public string ConflictType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string RequestPath { get; set; } = string.Empty;
    public List<EntityConflictInfo> EntityInfo { get; set; } = new();
}

/// <summary>
/// Information about the conflicting entity
/// </summary>
public class EntityConflictInfo
{
    public string EntityType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
