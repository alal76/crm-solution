// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for conversation thread management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ConversationsController : CrmControllerBase
{
    private const string ConversationNotFoundMessage = "Conversation {0} not found";
    private readonly IConversationService _service;
    private readonly ILogger<ConversationsController> _logger;

    public ConversationsController(IConversationService service, ILogger<ConversationsController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region CRUD Operations

    /// <summary>Gets all conversations with optional filters.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetAll(
        [FromQuery] int? accountId = null,
        [FromQuery] int? contactId = null,
        [FromQuery] ConversationStatus? status = null,
        [FromQuery] int? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var conversations = await _service.GetAllAsync(accountId, contactId, status, assignedToUserId, cancellationToken);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations");
            return Problem("An error occurred while retrieving conversations.");
        }
    }

    /// <summary>Gets a conversation by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Conversation>> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var conversation = await _service.GetByIdAsync(id, cancellationToken);
            if (conversation == null)
                return NotFound(string.Format(ConversationNotFoundMessage, id));
            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation {ConversationId}", id);
            return Problem("An error occurred while retrieving the conversation.");
        }
    }

    /// <summary>Gets a conversation by its unique conversation identifier string.</summary>
    [HttpGet("by-conversation-id/{conversationId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Conversation>> GetByConversationId(string conversationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var conversation = await _service.GetByConversationIdAsync(conversationId, cancellationToken);
            if (conversation == null)
                return NotFound($"Conversation '{conversationId}' not found");
            return Ok(conversation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversation by conversationId {ConversationId}", conversationId);
            return Problem("An error occurred while retrieving the conversation.");
        }
    }

    /// <summary>Creates a new conversation.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Conversation>> Create([FromBody] Conversation conversation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var created = await _service.CreateAsync(conversation, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating conversation");
            return Problem("An error occurred while creating the conversation.");
        }
    }

    /// <summary>Updates an existing conversation.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Update(int id, [FromBody] Conversation conversation, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateAsync(id, conversation, cancellationToken);
            if (!updated)
                return NotFound(string.Format(ConversationNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation {ConversationId}", id);
            return Problem("An error occurred while updating the conversation.");
        }
    }

    /// <summary>Deletes a conversation (soft delete).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return NotFound(string.Format(ConversationNotFoundMessage, id));
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting conversation {ConversationId}", id);
            return Problem("An error occurred while deleting the conversation.");
        }
    }

    #endregion

    #region Conversation-Specific Operations

    /// <summary>Updates the status of a conversation.</summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var updated = await _service.UpdateStatusAsync(id, request.Status, cancellationToken);
            if (!updated)
                return NotFound(string.Format(ConversationNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for conversation {ConversationId}", id);
            return Problem("An error occurred while updating the conversation status.");
        }
    }

    /// <summary>Assigns a conversation to a user.</summary>
    [HttpPost("{id}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Assign(int id, [FromBody] AssignRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);
            var assigned = await _service.AssignAsync(id, request.UserId, cancellationToken);
            if (!assigned)
                return NotFound(string.Format(ConversationNotFoundMessage, id));
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning conversation {ConversationId} to user {UserId}", id, request.UserId);
            return Problem("An error occurred while assigning the conversation.");
        }
    }

    /// <summary>Gets conversations for a specific entity (account, contact, or lead).</summary>
    [HttpGet("by-entity/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Conversation>>> GetByEntity(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var conversations = await _service.GetByEntityAsync(entityType, entityId, cancellationToken);
            return Ok(conversations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conversations for {EntityType} {EntityId}", entityType, entityId);
            return Problem("An error occurred while retrieving conversations by entity.");
        }
    }

    #endregion

    #region Request DTOs

    public class UpdateStatusRequest
    {
        [Required]
        public ConversationStatus Status { get; set; }
    }

    public class AssignRequest
    {
        [Required]
        public int UserId { get; set; }
    }

    #endregion
}
