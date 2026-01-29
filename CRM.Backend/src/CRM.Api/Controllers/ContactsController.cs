using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Api.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContactsController : ControllerBase
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
        try
        {
            var contacts = await _contactsService.GetAllAsync();
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all contacts");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving contacts");
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving contact with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving contact");
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
        try
        {
            var contacts = await _contactsService.GetByTypeAsync(contactType);
            return Ok(contacts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving contacts by type {contactType}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving contacts");
        }
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
                return BadRequest(ModelState);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating contact");
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
                return BadRequest(ModelState);

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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating contact with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating contact");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting contact with ID {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting contact");
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
                return BadRequest(ModelState);

            var link = await _contactsService.AddSocialMediaLinkAsync(id, request);
            return CreatedAtAction(nameof(GetContactById), new { id }, link);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Contact with ID {id} not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error adding social media link to contact {id}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error adding social media link");
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error removing social media link {linkId}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error removing social media link");
        }
    }
}
