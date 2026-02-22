// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CRM.Core.Dtos;

/// <summary>
/// Standard API response wrapper for all endpoints.
/// Provides consistent response format whether response succeeds or fails.
/// </summary>
/// <typeparam name="T">The type of data contained in the response.</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the API call succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the data payload (null if operation failed).
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets an optional message describing the response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets validation errors organized by field name.
    /// Null if no errors; Dictionary with field names as keys and error message arrays as values.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the response was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the request ID for tracking and correlation.
    /// Useful for debugging and tracing requests through logs.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Creates a successful API response.
    /// </summary>
    /// <param name="data">The response data payload.</param>
    /// <param name="message">Optional success message.</param>
    /// <param name="statusCode">HTTP status code (default 200 OK).</param>
    /// <returns>A successful ApiResponse.</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null, int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully.",
            Errors = null,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a successful API response with created status (201).
    /// </summary>
    /// <param name="data">The newly created resource data.</param>
    /// <param name="message">Optional message (default "Resource created successfully.").</param>
    /// <returns>A successful ApiResponse with 201 Created status.</returns>
    public static ApiResponse<T> CreatedResponse(T data, string? message = null)
    {
        return SuccessResponse(data, message ?? "Resource created successfully.", 201);
    }

    /// <summary>
    /// Creates a successful API response indicating no content (204).
    /// </summary>
    /// <param name="message">Optional message (default "Operation completed successfully.").</param>
    /// <returns>A successful ApiResponse with 204 No Content status.</returns>
    public static ApiResponse<T> NoContentResponse(string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = default,
            Message = message ?? "Operation completed successfully.",
            Errors = null,
            StatusCode = 204,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed API response.
    /// </summary>
    /// <param name="message">Error message describing what failed.</param>
    /// <param name="statusCode">HTTP status code (default 400 Bad Request).</param>
    /// <returns>A failed ApiResponse.</returns>
    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            Errors = null,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a validation error API response (400).
    /// </summary>
    /// <param name="errors">Dictionary mapping field names to error message arrays.</param>
    /// <param name="message">Optional message (default "Validation failed.").</param>
    /// <returns>A validation error ApiResponse with 400 Bad Request status.</returns>
    public static ApiResponse<T> ValidationErrorResponse(
        Dictionary<string, string[]> errors,
        string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message ?? "Validation failed.",
            Errors = errors,
            StatusCode = 400,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a single-field validation error response.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="errorMessage">The validation error message.</param>
    /// <param name="message">Optional message (default "Validation failed.").</param>
    /// <returns>A validation error ApiResponse with 400 Bad Request status.</returns>
    public static ApiResponse<T> SingleFieldValidationError(
        string fieldName,
        string errorMessage,
        string? message = null)
    {
        var errors = new Dictionary<string, string[]>
        {
            { fieldName, new[] { errorMessage } }
        };
        return ValidationErrorResponse(errors, message);
    }

    /// <summary>
    /// Creates an unauthorized error response (401).
    /// </summary>
    /// <param name="message">Error message (default "Unauthorized.").</param>
    /// <returns>An unauthorized ApiResponse with 401 Unauthorized status.</returns>
    public static ApiResponse<T> UnauthorizedResponse(string? message = null)
    {
        return ErrorResponse(message ?? "Unauthorized.", 401);
    }

    /// <summary>
    /// Creates a forbidden error response (403).
    /// </summary>
    /// <param name="message">Error message (default "Forbidden.").</param>
    /// <returns>A forbidden ApiResponse with 403 Forbidden status.</returns>
    public static ApiResponse<T> ForbiddenResponse(string? message = null)
    {
        return ErrorResponse(message ?? "Forbidden.", 403);
    }

    /// <summary>
    /// Creates a not found error response (404).
    /// </summary>
    /// <param name="message">Error message (default "Resource not found.").</param>
    /// <returns>A not found ApiResponse with 404 Not Found status.</returns>
    public static ApiResponse<T> NotFoundResponse(string? message = null)
    {
        return ErrorResponse(message ?? "Resource not found.", 404);
    }

    /// <summary>
    /// Creates a conflict error response (409).
    /// </summary>
    /// <param name="message">Error message (default "Resource conflict.").</param>
    /// <returns>A conflict ApiResponse with 409 Conflict status.</returns>
    public static ApiResponse<T> ConflictResponse(string? message = null)
    {
        return ErrorResponse(message ?? "Resource conflict.", 409);
    }

    /// <summary>
    /// Creates an internal server error response (500).
    /// </summary>
    /// <param name="message">Error message (default "An internal server error occurred.").</param>
    /// <returns>An internal server error ApiResponse with 500 Internal Server Error status.</returns>
    public static ApiResponse<T> InternalErrorResponse(string? message = null)
    {
        return ErrorResponse(message ?? "An internal server error occurred.", 500);
    }

    /// <summary>
    /// Creates a service unavailable error response (503).
    /// </summary>
    /// <param name="message">Error message (default "The service is currently unavailable.").</param>
    /// <returns>A service unavailable ApiResponse with 503 Service Unavailable status.</returns>
    public static ApiResponse<T> ServiceUnavailableResponse(string? message = null)
    {
        return ErrorResponse(message ?? "The service is currently unavailable.", 503);
    }
}

/// <summary>
/// Non-generic version of ApiResponse for situations where you don't need typed data.
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful non-typed API response.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <param name="statusCode">HTTP status code (default 200 OK).</param>
    /// <returns>A successful ApiResponse.</returns>
    public static ApiResponse SuccessResponse(string? message = null, int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Data = null,
            Message = message ?? "Operation completed successfully.",
            Errors = null,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a successful non-typed API response with created status (201).
    /// </summary>
    /// <param name="message">Optional message (default "Resource created successfully.").</param>
    /// <returns>A successful ApiResponse with 201 Created status.</returns>
    public static ApiResponse CreatedResponse(string? message = null)
    {
        return SuccessResponse(message ?? "Resource created successfully.", 201);
    }

    /// <summary>
    /// Creates a failed non-typed API response.
    /// </summary>
    /// <param name="message">Error message describing what failed.</param>
    /// <param name="statusCode">HTTP status code (default 400 Bad Request).</param>
    /// <returns>A failed ApiResponse.</returns>
    public static new ApiResponse ErrorResponse(string message, int statusCode = 400)
    {
        return new ApiResponse
        {
            Success = false,
            Data = null,
            Message = message,
            Errors = null,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a validation error non-typed API response (400).
    /// </summary>
    /// <param name="errors">Dictionary mapping field names to error message arrays.</param>
    /// <param name="message">Optional message (default "Validation failed.").</param>
    /// <returns>A validation error ApiResponse with 400 Bad Request status.</returns>
    public static new ApiResponse ValidationErrorResponse(
        Dictionary<string, string[]> errors,
        string? message = null)
    {
        return new ApiResponse
        {
            Success = false,
            Data = null,
            Message = message ?? "Validation failed.",
            Errors = errors,
            StatusCode = 400,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Standard wrapper for paginated list responses.
/// Combines pagination metadata with a list of items.
/// </summary>
/// <typeparam name="T">The type of items in the list.</typeparam>
public class PaginatedResponse<T> : ApiResponse<PaginatedList<T>>
{
    /// <summary>
    /// Creates a successful paginated response.
    /// </summary>
    /// <param name="items">The collection of items for the current page.</param>
    /// <param name="totalCount">The total count of items across all pages.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="message">Optional message (default "Items retrieved successfully.").</param>
    /// <returns>A successful paginated ApiResponse.</returns>
    public static PaginatedResponse<T> SuccessResponse(
        List<T> items,
        int totalCount,
        int page,
        int pageSize,
        string? message = null)
    {
        var paginatedList = new PaginatedList<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return new PaginatedResponse<T>
        {
            Success = true,
            Data = paginatedList,
            Message = message ?? "Items retrieved successfully.",
            Errors = null,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates an empty paginated response (no items found).
    /// </summary>
    /// <param name="message">Optional message (default "No items found.").</param>
    /// <returns>An empty paginated ApiResponse.</returns>
    public static PaginatedResponse<T> EmptyResponse(string? message = null)
    {
        return SuccessResponse(new List<T>(), 0, 1, 20, message ?? "No items found.");
    }
}

/// <summary>
/// Detailed error response with additional debugging information.
/// Use for comprehensive error details in development/debugging scenarios.
/// </summary>
public class DetailedErrorResponse
{
    /// <summary>
    /// Gets or sets the error title.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the exception type (class name).
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Gets or sets the stack trace (only in development).
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Gets or sets the inner exception info (if any).
    /// </summary>
    public DetailedErrorResponse? InnerException { get; set; }

    /// <summary>
    /// Gets or sets the request ID for tracing.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets or sets validation errors if applicable.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    /// <summary>
    /// Gets or sets the date and time the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a detailed error response from an exception.
    /// </summary>
    /// <param name="exception">The exception to convert.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <param name="includeStackTrace">Whether to include stack trace (typically true only in development).</param>
    /// <returns>A DetailedErrorResponse.</returns>
    public static DetailedErrorResponse FromException(
        Exception exception,
        int statusCode = 500,
        bool includeStackTrace = false)
    {
        var response = new DetailedErrorResponse
        {
            Title = exception.GetType().Name,
            Message = exception.Message,
            StatusCode = statusCode,
            ExceptionType = exception.GetType().FullName,
            StackTrace = includeStackTrace ? exception.StackTrace : null,
            Timestamp = DateTime.UtcNow
        };

        if (exception.InnerException != null)
        {
            response.InnerException = FromException(exception.InnerException, statusCode, includeStackTrace);
        }

        return response;
    }
}

/// <summary>
/// Bulk operation response containing success/failure results for each item.
/// </summary>
/// <typeparam name="T">The type of the result items.</typeparam>
public class BulkOperationResponse<T>
{
    /// <summary>
    /// Gets or sets the total number of items processed.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the number of items that succeeded.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of items that failed.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the collection of successfully processed items.
    /// </summary>
    public List<T> SuccessfulItems { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of failed items with error details.
    /// </summary>
    public List<BulkOperationError> FailedItems { get; set; } = new();

    /// <summary>
    /// Gets a value indicating whether all items succeeded.
    /// </summary>
    public bool AllSucceeded => FailureCount == 0;

    /// <summary>
    /// Gets the overall success rate as a percentage (0-100).
    /// </summary>
    public double SuccessRate => TotalCount > 0 ? (double)SuccessCount / TotalCount * 100 : 0;
}

/// <summary>
/// Represents a single failure in a bulk operation.
/// </summary>
public class BulkOperationError
{
    /// <summary>
    /// Gets or sets the index of the failed item in the original collection.
    /// </summary>
    public int ItemIndex { get; set; }

    /// <summary>
    /// Gets or sets the error message for this item.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Gets or sets the error code/type.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets validation errors for this item, if applicable.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
