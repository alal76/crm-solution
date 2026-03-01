// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Infrastructure;

/// <summary>
/// Base controller providing shared helpers for all CRM API controllers.
/// Global exception handling is performed by <see cref="CRM.Api.Middleware.ErrorHandlingMiddleware"/>;
/// individual actions do not need to catch <see cref="Exception"/>.
/// </summary>
[ApiController]
public abstract class CrmControllerBase : ControllerBase
{
    /// <summary>
    /// Returns a standardised "not found" message for a given entity and ID.
    /// </summary>
    protected static string NotFoundMessage(string entityType, object id) =>
        $"{entityType} {id} not found";

    /// <summary>
    /// Returns a standardised "not found" message for a given entity (no ID).
    /// </summary>
    protected static string NotFoundMessage(string entityType) =>
        $"{entityType} not found";

    /// <summary>
    /// Returns a 404 NotFound result with a standardised message.
    /// </summary>
    protected IActionResult EntityNotFound(string entityType, object id) =>
        NotFound(NotFoundMessage(entityType, id));

    /// <summary>
    /// Returns a 404 NotFound result with a standardised message (no ID).
    /// </summary>
    protected IActionResult EntityNotFound(string entityType) =>
        NotFound(NotFoundMessage(entityType));
}
