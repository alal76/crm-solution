// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using CRM.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for duplicate detection and merge operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DuplicatesController : ControllerBase
{
    private readonly IDuplicateDetectionService _duplicateDetectionService;
    private readonly IMergeService _mergeService;

    public DuplicatesController(
        IDuplicateDetectionService duplicateDetectionService,
        IMergeService mergeService)
    {
        _duplicateDetectionService = duplicateDetectionService;
        _mergeService = mergeService;
    }

    #region Duplicate Detection

    /// <summary>
    /// Check for potential duplicates when creating/updating a record
    /// </summary>
    /// <param name="request">The duplicate check request containing entity type and field values</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Duplicate check result with potential matches</returns>
    [HttpPost("check")]
    [ProducesResponseType(typeof(DuplicateCheckResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DuplicateCheckResult>> CheckForDuplicates(
        [FromBody] DuplicateCheckRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.EntityType))
        {
            return BadRequest("Entity type is required");
        }

        if (request.FieldValues == null || !request.FieldValues.Any())
        {
            return BadRequest("Field values are required");
        }

        var result = await _duplicateDetectionService.CheckForDuplicatesAsync(
            request.EntityType,
            request.FieldValues,
            request.ExcludeRecordId,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get active duplicate detection rules for an entity type
    /// </summary>
    [HttpGet("rules/{entityType}")]
    [ProducesResponseType(typeof(IEnumerable<Core.Entities.DuplicateRule>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Core.Entities.DuplicateRule>>> GetActiveRules(string entityType)
    {
        if (!Enum.TryParse<Core.Entities.DuplicateEntityType>(entityType, true, out var parsedType))
        {
            return BadRequest($"Invalid entity type: {entityType}. Valid values are: Lead, Contact, Account");
        }

        var rules = await _duplicateDetectionService.GetActiveRulesAsync(parsedType);
        return Ok(rules);
    }

    /// <summary>
    /// Trigger a full duplicate scan for an entity type
    /// </summary>
    [HttpPost("scan/{entityType}")]
    [ProducesResponseType(typeof(ScanResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ScanResult>> ScanForDuplicates(
        string entityType,
        [FromQuery] int? ruleId = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Core.Entities.DuplicateEntityType>(entityType, true, out var parsedType))
        {
            return BadRequest($"Invalid entity type: {entityType}. Valid values are: Lead, Contact, Account");
        }

        var candidates = await _duplicateDetectionService.ScanForDuplicatesAsync(parsedType, ruleId, cancellationToken);
        var result = new ScanResult
        {
            TotalRecordsScanned = 0, // Would need additional tracking in service
            DuplicateCandidatesFound = candidates.Count(),
            CompletedAt = DateTime.UtcNow
        };
        return Ok(result);
    }

    /// <summary>
    /// Get pending duplicate candidates for review
    /// </summary>
    [HttpGet("candidates/{entityType}")]
    [ProducesResponseType(typeof(IEnumerable<DuplicateMatch>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<Core.Entities.DuplicateCandidate>>> GetPendingCandidates(
        string entityType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!Enum.TryParse<Core.Entities.DuplicateEntityType>(entityType, true, out var parsedType))
        {
            return BadRequest($"Invalid entity type: {entityType}. Valid values are: Lead, Contact, Account");
        }

        var candidates = await _duplicateDetectionService.GetPendingCandidatesAsync(parsedType, page, pageSize);
        return Ok(candidates);
    }

    #endregion

    #region Merge Operations

    /// <summary>
    /// Preview a merge operation before executing
    /// </summary>
    [HttpPost("merge/preview")]
    [ProducesResponseType(typeof(MergePreview), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MergePreview>> PreviewMerge([FromBody] MergeRequest request)
    {
        var validation = ValidateMergeRequest(request);
        if (validation != null)
        {
            return validation;
        }

        var preview = await _mergeService.PreviewMergeAsync(request);
        return Ok(preview);
    }

    /// <summary>
    /// Merge multiple records into a master record
    /// </summary>
    [HttpPost("merge")]
    [ProducesResponseType(typeof(MergeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Manager,User")]
    public async Task<ActionResult<MergeResult>> MergeRecords([FromBody] MergeRequest request)
    {
        var validation = ValidateMergeRequest(request);
        if (validation != null)
        {
            return validation;
        }

        // Set user ID from claims - check multiple claim types for compatibility
        var userIdClaim = User.FindFirst("sub")
            ?? User.FindFirst("UserId")
            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("nameid");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            request.UserId = userId;
        }
        else
        {
            // Default to admin user ID 1 if no claim found (for API testing)
            request.UserId = 1;
        }

        var result = await _mergeService.MergeRecordsAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Unmerge records (restore previously merged duplicates)
    /// </summary>
    [HttpPost("unmerge")]
    [ProducesResponseType(typeof(UnmergeResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<UnmergeResult>> UnmergeRecords([FromBody] UnmergeRequest request)
    {
        if (request.MergeGroupId <= 0)
        {
            return BadRequest("Valid merge group ID is required");
        }

        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("UserId");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
        {
            request.UserId = userId;
        }

        var result = await _mergeService.UnmergeRecordsAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    #endregion

    #region Merge History

    /// <summary>
    /// Get merge history for a specific record
    /// </summary>
    [HttpGet("history/{entityType}/{recordId}")]
    [ProducesResponseType(typeof(IEnumerable<MergeGroupInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MergeGroupInfo>>> GetMergeHistory(
        string entityType,
        int recordId)
    {
        var history = await _mergeService.GetMergeHistoryAsync(recordId, entityType);
        return Ok(history);
    }

    /// <summary>
    /// Get records that were merged into a master record
    /// </summary>
    [HttpGet("merged-into/{entityType}/{masterRecordId}")]
    [ProducesResponseType(typeof(IEnumerable<MergedRecordInfo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MergedRecordInfo>>> GetMergedRecords(
        string entityType,
        int masterRecordId)
    {
        var records = await _mergeService.GetMergedRecordsAsync(masterRecordId, entityType);
        return Ok(records);
    }

    /// <summary>
    /// Get details of a specific merge group
    /// </summary>
    [HttpGet("groups/{mergeGroupId}")]
    [ProducesResponseType(typeof(MergeGroupInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MergeGroupInfo>> GetMergeGroup(int mergeGroupId)
    {
        var group = await _mergeService.GetMergeGroupAsync(mergeGroupId);
        if (group == null)
        {
            return NotFound();
        }

        return Ok(group);
    }

    #endregion

    #region Private Helpers

    private ActionResult? ValidateMergeRequest(MergeRequest request)
    {
        if (string.IsNullOrEmpty(request.EntityType))
        {
            return BadRequest("Entity type is required");
        }

        if (request.MasterRecordId <= 0)
        {
            return BadRequest("Valid master record ID is required");
        }

        if (request.RecordsToMerge == null || !request.RecordsToMerge.Any())
        {
            return BadRequest("At least one record to merge is required");
        }

        if (request.RecordsToMerge.Contains(request.MasterRecordId))
        {
            return BadRequest("Master record cannot be in the list of records to merge");
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Request model for checking duplicates
/// </summary>
public class DuplicateCheckRequest
{
    /// <summary>
    /// The entity type to check (Lead, Contact, Account)
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Field values to check against existing records
    /// </summary>
    public Dictionary<string, string?> FieldValues { get; set; } = new();

    /// <summary>
    /// Record ID to exclude from matching (for updates)
    /// </summary>
    public int? ExcludeRecordId { get; set; }

    /// <summary>
    /// Minimum match score threshold (0-100). Default is 70.
    /// </summary>
    public int MatchThreshold { get; set; } = 70;
}

/// <summary>
/// Result of a duplicate scan operation
/// </summary>
public class ScanResult
{
    public int TotalRecordsScanned { get; set; }
    public int DuplicateCandidatesFound { get; set; }
    public DateTime CompletedAt { get; set; }
}
