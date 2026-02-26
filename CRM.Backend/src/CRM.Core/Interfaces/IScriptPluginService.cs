// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

#region Data Transfer Objects

/// <summary>
/// Read-model DTO for a persisted <c>ScriptPlugin</c> record.
/// Returned by all query and mutation operations.
/// </summary>
/// <param name="Id">Primary key.</param>
/// <param name="Name">Display name of the plugin (max 200 chars).</param>
/// <param name="Description">Optional description shown in the UI and to the LLM planner.</param>
/// <param name="Language">Numeric value of the <c>ScriptLanguage</c> enum (0=JS, 1=Python, 2=C#).</param>
/// <param name="Code">Full script source code.</param>
/// <param name="ParameterSchema">JSON Schema document describing expected input parameters, or <c>null</c>.</param>
/// <param name="ReturnValueDescription">Plain-text description of the return value, or <c>null</c>.</param>
/// <param name="IsActive">Whether the plugin is registered in the Semantic Kernel tool registry.</param>
/// <param name="Version">Monotonically increasing schema version number.</param>
/// <param name="CreatedAt">UTC timestamp when the record was created.</param>
/// <param name="UpdatedAt">UTC timestamp of the most recent update, or <c>null</c>.</param>
public record ScriptPluginDto(
    int Id,
    string Name,
    string? Description,
    int Language,
    string Code,
    string? ParameterSchema,
    string? ReturnValueDescription,
    bool IsActive,
    int Version,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Command DTO used when creating a new <c>ScriptPlugin</c> record.
/// </summary>
/// <param name="Name">Display name — required, max 200 characters.</param>
/// <param name="Description">Optional description — max 2 000 characters.</param>
/// <param name="Language">
/// Numeric value of the <c>ScriptLanguage</c> enum
/// (0 = JavaScript, 1 = Python, 2 = C#).
/// </param>
/// <param name="Code">Script source code — required, max 50 000 characters.</param>
/// <param name="ParameterSchema">
/// JSON Schema document for input parameter validation — optional, max 5 000 characters.
/// </param>
/// <param name="ReturnValueDescription">
/// Description of what the script returns — optional, max 1 000 characters.
/// </param>
public record CreateScriptPluginDto(
    string Name,
    string? Description,
    int Language,
    string Code,
    string? ParameterSchema,
    string? ReturnValueDescription);

/// <summary>
/// Command DTO used when updating an existing <c>ScriptPlugin</c> record.
/// The <c>Language</c> field is intentionally omitted; to change the scripting language
/// delete the plugin and create a new one to preserve audit history.
/// </summary>
/// <param name="Name">New display name — required, max 200 characters.</param>
/// <param name="Description">New description — optional, max 2 000 characters.</param>
/// <param name="Code">Replacement script source code — required, max 50 000 characters.</param>
/// <param name="ParameterSchema">
/// Replacement JSON Schema for input parameters — optional, max 5 000 characters.
/// </param>
/// <param name="ReturnValueDescription">
/// Replacement return-value description — optional, max 1 000 characters.
/// </param>
/// <param name="IsActive">
/// Pass <c>false</c> to deactivate the plugin (removes it from the tool registry
/// without deleting the record).
/// </param>
public record UpdateScriptPluginDto(
    string Name,
    string? Description,
    string Code,
    string? ParameterSchema,
    string? ReturnValueDescription,
    bool IsActive);

/// <summary>
/// Input payload for a sandboxed test execution of a script plugin.
/// </summary>
/// <param name="Variables">
/// Named input variables injected into the script's execution context, keyed by
/// parameter name. Values must be JSON-serialisable. May be empty but not null.
/// </param>
/// <param name="Context">
/// Optional ambient context entries (e.g., <c>currentUserId</c>, <c>accountId</c>)
/// passed through to the sandbox but not validated against <c>ParameterSchema</c>.
/// </param>
/// <param name="Timeout">
/// Maximum wall-clock time allowed for execution. When <c>null</c> the service
/// applies a server-side default (typically 30 seconds).
/// </param>
public record ScriptPluginTestRequest(
    Dictionary<string, object?> Variables,
    Dictionary<string, object?> Context,
    TimeSpan? Timeout);

/// <summary>
/// Result of a sandboxed test execution of a script plugin.
/// </summary>
/// <param name="Success">
/// <c>true</c> when the script completed without throwing an unhandled exception.
/// </param>
/// <param name="ReturnValue">
/// The value returned by the script's entry-point function, or <c>null</c> if the
/// script returned nothing or execution failed.
/// </param>
/// <param name="Logs">
/// Ordered list of log lines emitted by the script via its sandbox logger
/// (e.g., <c>console.log</c> in JavaScript).
/// </param>
/// <param name="ErrorMessage">
/// Human-readable error description when <see cref="Success"/> is <c>false</c>;
/// otherwise <c>null</c>.
/// </param>
/// <param name="ExecutionTime">Actual wall-clock duration of the sandboxed execution.</param>
public record ScriptPluginTestResult(
    bool Success,
    object? ReturnValue,
    IReadOnlyList<string> Logs,
    string? ErrorMessage,
    TimeSpan ExecutionTime);

#endregion

/// <summary>
/// Service contract for managing <c>ScriptPlugin</c> entities — user-authored scripts that
/// are surfaced as Semantic Kernel tools available to AI agents and workflow nodes.
/// </summary>
/// <remarks>
/// Implementations must register in the DI container as scoped services and persist
/// records via <c>ICrmDbContext</c>. Execution is sandboxed by the underlying script engine
/// and must enforce resource limits independently of this interface contract.
/// </remarks>
public interface IScriptPluginService
{
    /// <summary>
    /// Returns all script plugins, optionally including inactive ones.
    /// Results are ordered by <c>Name</c> ascending.
    /// </summary>
    /// <param name="includeInactive">
    /// When <c>true</c> the result set includes plugins whose <c>IsActive</c> flag is
    /// <c>false</c>. Defaults to <c>false</c> (active plugins only).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An immutable, ordered list of <see cref="ScriptPluginDto"/> projections.</returns>
    Task<IReadOnlyList<ScriptPluginDto>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Returns a single script plugin by its primary key, or <c>null</c> if not found
    /// or soft-deleted.
    /// </summary>
    /// <param name="id">Primary key of the <c>ScriptPlugin</c> record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The matching <see cref="ScriptPluginDto"/>, or <c>null</c> when no active record
    /// with the given <paramref name="id"/> exists.
    /// </returns>
    Task<ScriptPluginDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Persists a new <c>ScriptPlugin</c> record and returns its projection.
    /// </summary>
    /// <param name="dto">Creation payload — all required fields must be populated.</param>
    /// <param name="createdByUserId">
    /// Primary key of the <c>User</c> authoring this plugin. Stored in
    /// <c>ScriptPlugin.CreatedBy</c> for audit purposes.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created <see cref="ScriptPluginDto"/> with its assigned <c>Id</c>.</returns>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown when <paramref name="dto"/> violates field constraints (e.g., name too long,
    /// code empty).
    /// </exception>
    Task<ScriptPluginDto> CreateAsync(CreateScriptPluginDto dto, int createdByUserId, CancellationToken ct = default);

    /// <summary>
    /// Applies a full replacement update to an existing <c>ScriptPlugin</c> record and
    /// returns the updated projection.
    /// </summary>
    /// <param name="id">Primary key of the record to update.</param>
    /// <param name="dto">Replacement values — all fields are overwritten.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated <see cref="ScriptPluginDto"/>.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when no active (non-deleted) record with <paramref name="id"/> exists.
    /// </exception>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">
    /// Thrown when <paramref name="dto"/> violates field constraints.
    /// </exception>
    Task<ScriptPluginDto> UpdateAsync(int id, UpdateScriptPluginDto dto, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a <c>ScriptPlugin</c> record by setting <c>IsDeleted = true</c>.
    /// The record is retained in the database for audit purposes and can be recovered
    /// by an administrator.
    /// </summary>
    /// <param name="id">Primary key of the record to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when no active record with <paramref name="id"/> exists.
    /// </exception>
    Task DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Executes the script plugin inside a sandboxed runtime using the supplied test inputs
    /// and returns a detailed result including logs, return value, and timing.
    /// </summary>
    /// <remarks>
    /// This method also updates <c>ScriptPlugin.LastTestedAt</c> and
    /// <c>ScriptPlugin.LastTestResult</c> on the persisted record after execution completes
    /// (regardless of success or failure).
    /// </remarks>
    /// <param name="id">Primary key of the <c>ScriptPlugin</c> to execute.</param>
    /// <param name="request">Input variables, context, and optional timeout override.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A <see cref="ScriptPluginTestResult"/> describing the outcome, including any
    /// captured log output and the actual wall-clock execution time.
    /// </returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">
    /// Thrown when no active record with <paramref name="id"/> exists.
    /// </exception>
    Task<ScriptPluginTestResult> TestExecuteAsync(int id, ScriptPluginTestRequest request, CancellationToken ct = default);
}
