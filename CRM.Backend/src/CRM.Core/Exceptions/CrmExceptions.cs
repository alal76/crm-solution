// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Net;

namespace CRM.Core.Exceptions;

/// <summary>
/// Base exception for all CRM-specific exceptions
/// </summary>
public abstract class CrmException : Exception
{
    /// <summary>
    /// HTTP status code to return for this exception
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Error code for client-side handling
    /// </summary>
    public string ErrorCode { get; }

    protected CrmException(string message, HttpStatusCode statusCode, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception for entity not found errors (404)
/// </summary>
public class EntityNotFoundException : CrmException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base($"{entityType} not found" + (entityId != null ? $" with ID: {entityId}" : ""),
            HttpStatusCode.NotFound, "ENTITY_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception for validation errors (400)
/// </summary>
public class ValidationException : CrmException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]>? errors = null)
        : base(message, HttpStatusCode.BadRequest, "VALIDATION_ERROR")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    public ValidationException(string field, string error)
        : this($"Validation failed for {field}: {error}",
            new Dictionary<string, string[]> { { field, new[] { error } } })
    {
    }
}

/// <summary>
/// Exception for business rule violations (400/422)
/// </summary>
public class BusinessRuleException : CrmException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
        : base(message, HttpStatusCode.UnprocessableEntity, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception for authorization failures (403)
/// </summary>
public class AuthorizationException : CrmException
{
    public string? RequiredPermission { get; }

    public AuthorizationException(string message, string? requiredPermission = null)
        : base(message, HttpStatusCode.Forbidden, "ACCESS_DENIED")
    {
        RequiredPermission = requiredPermission;
    }
}

/// <summary>
/// Exception for authentication failures (401)
/// </summary>
public class AuthenticationException : CrmException
{
    public AuthenticationException(string message = "Authentication required")
        : base(message, HttpStatusCode.Unauthorized, "AUTHENTICATION_REQUIRED")
    {
    }
}

/// <summary>
/// Exception for concurrency conflicts (409)
/// </summary>
public class ConcurrencyException : CrmException
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public ConcurrencyException(string entityType, object? entityId = null)
        : base($"The {entityType} was modified by another user. Please refresh and try again.",
            HttpStatusCode.Conflict, "CONCURRENCY_CONFLICT")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception for service/infrastructure errors (500)
/// </summary>
public class ServiceException : CrmException
{
    public string ServiceName { get; }

    public ServiceException(string serviceName, string message, Exception? innerException = null)
        : base(message, HttpStatusCode.InternalServerError, "SERVICE_ERROR", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception for external API/integration errors (502)
/// </summary>
public class ExternalServiceException : CrmException
{
    public string ServiceName { get; }
    public int? ExternalStatusCode { get; }

    public ExternalServiceException(string serviceName, string message, int? externalStatusCode = null, Exception? innerException = null)
        : base($"External service '{serviceName}' error: {message}",
            HttpStatusCode.BadGateway, "EXTERNAL_SERVICE_ERROR", innerException)
    {
        ServiceName = serviceName;
        ExternalStatusCode = externalStatusCode;
    }
}

/// <summary>
/// Exception for rate limiting (429)
/// </summary>
public class RateLimitException : CrmException
{
    public int RetryAfterSeconds { get; }

    public RateLimitException(int retryAfterSeconds = 60)
        : base($"Rate limit exceeded. Please retry after {retryAfterSeconds} seconds.",
            (HttpStatusCode)429, "RATE_LIMIT_EXCEEDED")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

/// <summary>
/// Exception for configuration errors (500)
/// </summary>
public class ConfigurationException : CrmException
{
    public string ConfigurationKey { get; }

    public ConfigurationException(string configurationKey, string message)
        : base($"Configuration error for '{configurationKey}': {message}",
            HttpStatusCode.InternalServerError, "CONFIGURATION_ERROR")
    {
        ConfigurationKey = configurationKey;
    }
}

/// <summary>
/// Exception for duplicate entity detection (409)
/// </summary>
public class DuplicateEntityException : CrmException
{
    public string EntityType { get; }
    public IEnumerable<int> DuplicateIds { get; }

    public DuplicateEntityException(string entityType, IEnumerable<int> duplicateIds)
        : base($"Potential duplicate {entityType} detected",
            HttpStatusCode.Conflict, "DUPLICATE_DETECTED")
    {
        EntityType = entityType;
        DuplicateIds = duplicateIds;
    }
}
