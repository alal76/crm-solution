// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRM.Api.Filters;

/// <summary>
/// Global exception filter that converts <see cref="DuplicateExistsException"/>
/// into HTTP 409 Conflict responses with structured error details.
///
/// Registered globally on all controllers via AddControllers options.
/// </summary>
public class DuplicateExistsExceptionFilter : IExceptionFilter
{
    private readonly ILogger<DuplicateExistsExceptionFilter> _logger;

    public DuplicateExistsExceptionFilter(ILogger<DuplicateExistsExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DuplicateExistsException dupEx)
        {
            _logger.LogWarning(
                "Duplicate record rejected: {EntityType} (existing ID: {ExistingId}, score: {Score}%)",
                dupEx.EntityType, dupEx.ExistingRecordId, dupEx.MatchScore);

            context.Result = new ConflictObjectResult(new
            {
                message = dupEx.Message,
                entityType = dupEx.EntityType,
                existingRecordId = dupEx.ExistingRecordId,
                matchScore = dupEx.MatchScore,
                error = "DUPLICATE_EXISTS"
            });

            context.ExceptionHandled = true;
        }
    }
}
