// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Api.Infrastructure;

/// <summary>
/// Shared string constants used across CRM API controllers.
/// </summary>
public static class ControllerConstants
{
    /// <summary>Generic internal server error message.</summary>
    public const string InternalServerErrorMessage = "Internal server error";

    /// <summary>Generic error message for operations that fail without detail.</summary>
    public const string GenericErrorMessage = "An error occurred";

    /// <summary>Message returned when a request body is expected but absent.</summary>
    public const string RequestBodyRequiredMessage = "Request body is required";
}
