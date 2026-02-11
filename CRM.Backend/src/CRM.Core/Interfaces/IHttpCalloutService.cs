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

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for executing HTTP callouts from workflow action nodes.
/// Nodes with Configuration JSON containing an "httpCallout" section
/// will be executed through this service.
/// </summary>
public interface IHttpCalloutService
{
    /// <summary>
    /// Execute an HTTP callout described by a <see cref="HttpCalloutConfig"/>.
    /// Returns a result with response status, body, and headers.
    /// </summary>
    Task<HttpCalloutResult> ExecuteAsync(HttpCalloutConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate that a callout configuration is well-formed.
    /// </summary>
    HttpCalloutValidation Validate(HttpCalloutConfig config);
}

/// <summary>
/// Configuration for an HTTP callout (parsed from node Configuration JSON "httpCallout" section).
/// </summary>
public class HttpCalloutConfig
{
    /// <summary>Absolute URL to call</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>HTTP method (GET, POST, PUT, PATCH, DELETE)</summary>
    public string Method { get; set; } = "GET";

    /// <summary>Request headers</summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>Optional request body (sent as JSON for POST/PUT/PATCH)</summary>
    public object? Body { get; set; }

    /// <summary>Timeout in seconds (default 30)</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Number of retry attempts on transient failure (default 0)</summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>Seconds between retries (default 5)</summary>
    public int RetryDelaySeconds { get; set; } = 5;

    /// <summary>If true, treat any 2xx as success; if false, only 200-204</summary>
    public bool AcceptAny2xx { get; set; } = true;

    /// <summary>Name/label for logging</summary>
    public string? Name { get; set; }
}

/// <summary>
/// Result of an HTTP callout execution.
/// </summary>
public class HttpCalloutResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
    public string? ResponseBody { get; set; }
    public Dictionary<string, string> ResponseHeaders { get; set; } = new();
    public long ElapsedMs { get; set; }
    public int Attempts { get; set; } = 1;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of validating a callout configuration.
/// </summary>
public class HttpCalloutValidation
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
