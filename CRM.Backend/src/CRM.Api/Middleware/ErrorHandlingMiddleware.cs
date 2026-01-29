using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Middleware;

/// <summary>
/// Error handling middleware - handles exceptions and converts to appropriate HTTP responses
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

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
            await context.Response.WriteAsJsonAsync(new { message = "Internal server error" });
        }
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
