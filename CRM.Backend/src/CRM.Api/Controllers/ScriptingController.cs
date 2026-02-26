// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Scripting;
using CRM.Infrastructure.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Scripting engine endpoints — manage script validation, execution, and plugins.
/// Implements TODO-SCRIPT-001, TODO-SCRIPT-002, TODO-SCRIPT-003, TODO-SCRIPT-007.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ScriptingController : ControllerBase
{
    private readonly ScriptEngineFactory _scriptEngineFactory;
    private readonly IEnumerable<IScriptEngine> _scriptEngines;
    private readonly IScriptPluginService _scriptPluginService;
    private readonly ILogger<ScriptingController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptingController"/> class.
    /// </summary>
    /// <param name="scriptEngineFactory">Factory for resolving script engines by language.</param>
    /// <param name="scriptEngines">All registered script engine implementations.</param>
    /// <param name="scriptPluginService">Service for managing persisted script plugins.</param>
    /// <param name="logger">Logger for error and audit logging.</param>
    public ScriptingController(
        ScriptEngineFactory scriptEngineFactory,
        IEnumerable<IScriptEngine> scriptEngines,
        IScriptPluginService scriptPluginService,
        ILogger<ScriptingController> logger)
    {
        _scriptEngineFactory = scriptEngineFactory;
        _scriptEngines = scriptEngines;
        _scriptPluginService = scriptPluginService;
        _logger = logger;
    }

    // ================================================================= //
    //  TODO-SCRIPT-001: List Available Script Engines
    // ================================================================= //

    /// <summary>
    /// Lists all registered script engines with their name, language enum value,
    /// availability status, and configured timeout.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of engine summary objects.</returns>
    /// <response code="200">Returns list of available script engines.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("engines")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetAvailableEngines(CancellationToken cancellationToken = default)
    {
        try
        {
            var engines = _scriptEngines.Select(e => new
            {
                name = e.Language.ToString(),
                language = (int)e.Language,
                isAvailable = e.IsAvailable
            });

            return Ok(engines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available script engines");
            return StatusCode(500, new { message = "Failed to retrieve script engines", error = ex.Message });
        }
    }

    // ================================================================= //
    //  TODO-SCRIPT-002: Validate Script Syntax
    // ================================================================= //

    /// <summary>
    /// Validates script syntax without execution. Returns a flag and any
    /// compiler/parser diagnostics produced by the target engine.
    /// </summary>
    /// <param name="request">Validation request containing the numeric language enum value and source code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with <c>isValid</c> flag and <c>diagnostics</c> array.</returns>
    /// <response code="200">Returns validation result.</response>
    /// <response code="400">Bad request (invalid language or missing code).</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ValidateScript(
        [FromBody] ScriptValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Code is required" });

            var language = (ScriptLanguage)request.Language;
            IScriptEngine engine;

            try
            {
                engine = _scriptEngineFactory.GetEngine(language);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var diagnostics = await engine.ValidateSyntaxAsync(request.Code, cancellationToken);

            return Ok(new
            {
                isValid = !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
                diagnostics = diagnostics.Select(d => new
                {
                    line = d.Line,
                    column = d.Column,
                    message = d.Message,
                    severity = d.Severity.ToString()
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating script in language {Language}", request?.Language);
            return StatusCode(500, new { message = "Failed to validate script", error = ex.Message });
        }
    }

    // ================================================================= //
    //  TODO-SCRIPT-003: Execute Script Synchronously
    // ================================================================= //

    /// <summary>
    /// Executes a script synchronously inside the sandboxed engine with optional
    /// variable context and timeout override. The response always returns HTTP 200;
    /// check <c>success</c> in the body for script-level failures.
    /// </summary>
    /// <param name="request">Execution request containing language, code, variables, and optional timeout (ms).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Execution result with success status, return value, logs, and duration.</returns>
    /// <response code="200">Returns execution result (check <c>success</c> for script-level errors).</response>
    /// <response code="400">Bad request (invalid language, missing code, or invalid timeout).</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("execute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExecuteScript(
        [FromBody] ScriptExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest(new { message = "Code is required" });

            if (request.Timeout.HasValue && request.Timeout.Value <= 0)
                return BadRequest(new { message = "Timeout must be a positive value in milliseconds" });

            var language = (ScriptLanguage)request.Language;
            IScriptEngine engine;

            try
            {
                engine = _scriptEngineFactory.GetEngine(language);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            var variables = request.Variables ?? new Dictionary<string, object?>();
            var context = request.Context ?? new Dictionary<string, object?>();
            var timeout = request.Timeout.HasValue
                ? TimeSpan.FromMilliseconds(request.Timeout.Value)
                : (TimeSpan?)null;

            var result = await engine.ExecuteAsync(
                request.Code,
                variables,
                context,
                timeout,
                cancellationToken);

            return Ok(new
            {
                success = result.Success,
                returnValue = result.ReturnValue,
                logs = result.Logs,
                errorMessage = result.ErrorMessage,
                executionTimeMs = result.ExecutionTime.TotalMilliseconds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing script in language {Language}", request?.Language);
            return StatusCode(500, new { message = "Failed to execute script", error = ex.Message });
        }
    }

    // ================================================================= //
    //  TODO-SCRIPT-007: Script Plugin Management
    // ================================================================= //

    /// <summary>
    /// Returns all persisted script plugins, optionally including inactive ones.
    /// Results are ordered by <c>Name</c> ascending.
    /// </summary>
    /// <param name="includeInactive">
    /// When <c>true</c>, inactive plugins are included. Defaults to <c>false</c>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Ordered list of <see cref="ScriptPluginDto"/> objects.</returns>
    /// <response code="200">Returns list of plugins.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("plugins")]
    [ProducesResponseType(typeof(IReadOnlyList<ScriptPluginDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ListPlugins(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plugins = await _scriptPluginService.GetAllAsync(includeInactive, cancellationToken);
            return Ok(plugins);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing script plugins");
            return StatusCode(500, new { message = "Failed to list plugins", error = ex.Message });
        }
    }

    /// <summary>
    /// Returns a single script plugin by its primary key.
    /// </summary>
    /// <param name="id">Primary key of the plugin record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching <see cref="ScriptPluginDto"/>, or 404 when not found.</returns>
    /// <response code="200">Returns plugin details.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Plugin not found or has been deleted.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("plugins/{id:int}")]
    [ProducesResponseType(typeof(ScriptPluginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> GetPluginById(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var plugin = await _scriptPluginService.GetByIdAsync(id, cancellationToken);

            if (plugin == null)
                return NotFound(new { message = $"Script plugin {id} not found" });

            return Ok(plugin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving script plugin {PluginId}", id);
            return StatusCode(500, new { message = "Failed to retrieve plugin", error = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new script plugin with the supplied metadata and source code.
    /// The authenticated user is recorded as the author.
    /// </summary>
    /// <param name="dto">Creation payload — name, language, and code are required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="ScriptPluginDto"/> with its assigned <c>Id</c>.</returns>
    /// <response code="201">Plugin created successfully. Location header points to the new resource.</response>
    /// <response code="400">Bad request (validation failure).</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("plugins")]
    [ProducesResponseType(typeof(ScriptPluginDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CreatePlugin(
        [FromBody] CreateScriptPluginDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var created = await _scriptPluginService.CreateAsync(dto, userId, cancellationToken);
            return CreatedAtAction(nameof(GetPluginById), new { id = created.Id }, created);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating script plugin");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating script plugin {PluginName}", dto?.Name);
            return StatusCode(500, new { message = "Failed to create plugin", error = ex.Message });
        }
    }

    /// <summary>
    /// Applies a full replacement update to an existing script plugin.
    /// All mutable fields (name, description, code, schema, active flag) are overwritten.
    /// </summary>
    /// <param name="id">Primary key of the plugin to update.</param>
    /// <param name="dto">Replacement values payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated <see cref="ScriptPluginDto"/>.</returns>
    /// <response code="200">Plugin updated successfully.</response>
    /// <response code="400">Bad request (validation failure).</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Plugin not found or has been deleted.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("plugins/{id:int}")]
    [ProducesResponseType(typeof(ScriptPluginDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdatePlugin(
        int id,
        [FromBody] UpdateScriptPluginDto dto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _scriptPluginService.UpdateAsync(id, dto, cancellationToken);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Script plugin {PluginId} not found for update", id);
            return NotFound(new { message = ex.Message });
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating script plugin {PluginId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating script plugin {PluginId}", id);
            return StatusCode(500, new { message = "Failed to update plugin", error = ex.Message });
        }
    }

    /// <summary>
    /// Soft-deletes a script plugin. The record is retained in the database for
    /// audit purposes but is excluded from all future queries.
    /// </summary>
    /// <param name="id">Primary key of the plugin to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 204 No Content on success.</returns>
    /// <response code="204">Plugin deleted successfully.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Plugin not found or already deleted.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("plugins/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeletePlugin(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _scriptPluginService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Script plugin {PluginId} not found for deletion", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting script plugin {PluginId}", id);
            return StatusCode(500, new { message = "Failed to delete plugin", error = ex.Message });
        }
    }

    /// <summary>
    /// Executes a script plugin inside a sandboxed runtime using the supplied
    /// test variables and context, and returns a detailed result including captured
    /// logs, return value, and wall-clock execution time.
    /// Also updates <c>LastTestedAt</c> and <c>LastTestResult</c> on the plugin record.
    /// </summary>
    /// <param name="id">Primary key of the plugin to execute.</param>
    /// <param name="request">
    /// Input variables, ambient context, and an optional timeout override.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="ScriptPluginTestResult"/> describing the execution outcome.</returns>
    /// <response code="200">Returns execution result (check <c>success</c> for script-level errors).</response>
    /// <response code="400">Bad request (invalid plugin or execution parameters).</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="404">Plugin not found or has been deleted.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("plugins/{id:int}/test")]
    [ProducesResponseType(typeof(ScriptPluginTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> TestExecutePlugin(
        int id,
        [FromBody] ScriptPluginTestRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request body is required" });

            var result = await _scriptPluginService.TestExecuteAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Script plugin {PluginId} not found for test execution", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during plugin test execution for plugin {PluginId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing plugin test {PluginId}", id);
            return StatusCode(500, new { message = "Failed to execute plugin test", error = ex.Message });
        }
    }

    // ================================================================= //
    //  Helpers
    // ================================================================= //

    /// <summary>
    /// Extracts the authenticated user's numeric ID from JWT claims.
    /// Returns 0 when the claim is absent or cannot be parsed.
    /// </summary>
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 0;
    }
}

// ================================================================= //
//  Request DTOs (controller-layer, separate from service-layer DTOs)
// ================================================================= //

/// <summary>
/// Request body for the <c>POST /api/scripting/validate</c> endpoint.
/// </summary>
public class ScriptValidationRequest
{
    /// <summary>
    /// Numeric value of the <see cref="ScriptLanguage"/> enum
    /// (0 = JavaScript, 1 = Python, 2 = C#).
    /// </summary>
    public int Language { get; set; }

    /// <summary>
    /// Source code to validate.
    /// </summary>
    public string? Code { get; set; }
}

/// <summary>
/// Request body for the <c>POST /api/scripting/execute</c> endpoint.
/// </summary>
public class ScriptExecutionRequest
{
    /// <summary>
    /// Numeric value of the <see cref="ScriptLanguage"/> enum
    /// (0 = JavaScript, 1 = Python, 2 = C#).
    /// </summary>
    public int Language { get; set; }

    /// <summary>
    /// Source code to execute.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Named variables injected into the script's execution context.
    /// </summary>
    public Dictionary<string, object?>? Variables { get; set; }

    /// <summary>
    /// Ambient context entries (e.g., <c>currentUserId</c>) passed through to the sandbox.
    /// </summary>
    public Dictionary<string, object?>? Context { get; set; }

    /// <summary>
    /// Execution timeout in milliseconds. When <c>null</c> the engine applies its default.
    /// </summary>
    public int? Timeout { get; set; }
}
