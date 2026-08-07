// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Exceptions;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContactsController : CrmControllerBase
{
    private readonly IContactsService _contactsService;
    private readonly ILogger<ContactsController> _logger;
    private readonly ICrmNotificationService _notificationService;

    public ContactsController(
        IContactsService contactsService,
        ILogger<ContactsController> logger,
        ICrmNotificationService notificationService)
    {
        _contactsService = contactsService;
        _logger = logger;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Get all contacts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ContactDto>>> GetAllContacts()
    {
                var contacts = await _contactsService.GetAllAsync();
        return Ok(contacts);
    }

    /// <summary>
    /// Get contact by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactDto>> GetContactById(int id)
    {
        try
        {
            var contact = await _contactsService.GetByIdAsync(id);
            return Ok(contact);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get contacts by type (Employee, Partner, Lead, Customer, Vendor, Other)
    /// </summary>
    [HttpGet("type/{contactType}")]
    [ProducesResponseType(typeof(List<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<ContactDto>>> GetContactsByType(string contactType)
    {
                var contacts = await _contactsService.GetByTypeAsync(contactType);
        return Ok(contacts);
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.FindFirst("sub")?.Value ?? "System";
            var contact = await _contactsService.CreateAsync(request, currentUser);

            // Notify connected clients about the new contact
            await _notificationService.NotifyRecordCreatedAsync("Contact", contact.Id, contact, currentUser);

            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid data when creating contact");
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateExistsException dex)
        {
            return Conflict(new { message = dex.Message, entityType = dex.EntityType, existingRecordId = dex.ExistingRecordId, matchScore = dex.MatchScore });
        }
    }

    /// <summary>
    /// Update a contact
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ContactDto>> UpdateContact(int id, [FromBody] UpdateContactRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = User.FindFirst("sub")?.Value ?? "System";
            var contact = await _contactsService.UpdateAsync(id, request, currentUser);

            // Notify connected clients about the update
            await _notificationService.NotifyRecordUpdatedAsync("Contact", id, contact, currentUser);

            return Ok(contact);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteContact(int id)
    {
        try
        {
            await _contactsService.DeleteAsync(id);

            // Notify connected clients about the deletion
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("nameid")?.Value;
            await _notificationService.NotifyRecordDeletedAsync("Contact", id, userId);

            return Ok(new { message = $"Contact with ID {id} deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Add a social media link to a contact
    /// </summary>
    [HttpPost("{id}/social-media")]
    [ProducesResponseType(typeof(SocialMediaLinkDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SocialMediaLinkDto>> AddSocialMediaLink(int id, [FromBody] AddSocialMediaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var link = await _contactsService.AddSocialMediaLinkAsync(id, request);
            return CreatedAtAction(nameof(GetContactById), new { id }, link);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a social media link
    /// </summary>
    [HttpDelete("social-media/{linkId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveSocialMediaLink(int linkId)
    {
        try
        {
            await _contactsService.RemoveSocialMediaLinkAsync(linkId);
            return Ok(new { message = $"Social media link {linkId} removed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Social media link with ID {linkId} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Assign a contact to an account (sets the contact's AccountId)
    /// </summary>
    [HttpPost("{id}/account/{accountId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AssignToAccount(int id, int accountId)
    {
        try
        {
            await _contactsService.AssignToAccountAsync(id, accountId);
            return Ok(new { message = $"Contact {id} assigned to account {accountId} successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Remove a contact's account assignment (clears the contact's AccountId)
    /// </summary>
    [HttpDelete("{id}/account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnassignFromAccount(int id)
    {
        try
        {
            await _contactsService.UnassignFromAccountAsync(id);
            return Ok(new { message = $"Contact {id} unassigned from account successfully" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
    }
}
