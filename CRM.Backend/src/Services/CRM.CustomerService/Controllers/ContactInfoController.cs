using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for managing consolidated contact information (addresses, phones, emails, social media)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactInfoController : ControllerBase
{
    private readonly IContactInfoService _contactInfoService;
    private readonly ILogger<ContactInfoController> _logger;

    public ContactInfoController(IContactInfoService contactInfoService, ILogger<ContactInfoController> logger)
    {
        _contactInfoService = contactInfoService;
        _logger = logger;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
        return int.TryParse(userIdClaim?.Value, out var userId) ? userId : null;
    }

    #region Entity Contact Info (Aggregate)

    /// <summary>
    /// Get all contact information for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}")]
    public async Task<ActionResult<EntityContactInfoDto>> GetEntityContactInfo(string entityType, int entityId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            var result = await _contactInfoService.GetEntityContactInfoAsync(type, entityId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact info for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving contact information");
        }
    }

    /// <summary>
    /// Share contact information with another entity
    /// </summary>
    [HttpPost("share")]
    public async Task<ActionResult> ShareContactInfo([FromBody] ShareContactInfoDto dto)
    {
        try
        {
            await _contactInfoService.ShareContactInfoAsync(dto, GetCurrentUserId());
            return Ok(new { message = "Contact information shared successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing contact info");
            return StatusCode(500, "An error occurred while sharing contact information");
        }
    }

    #endregion

    #region Address Endpoints

    /// <summary>
    /// Get all addresses for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}/addresses")]
    public async Task<ActionResult<List<LinkedAddressDto>>> GetAddresses(string entityType, int entityId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            var addresses = await _contactInfoService.GetAddressesAsync(type, entityId);
            return Ok(addresses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting addresses for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving addresses");
        }
    }

    /// <summary>
    /// Get a specific address by ID
    /// </summary>
    [HttpGet("addresses/{addressId}")]
    public async Task<ActionResult<AddressDto>> GetAddress(int addressId)
    {
        try
        {
            var address = await _contactInfoService.GetAddressByIdAsync(addressId);
            if (address == null)
            {
                return NotFound();
            }
            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting address {AddressId}", addressId);
            return StatusCode(500, "An error occurred while retrieving the address");
        }
    }

    /// <summary>
    /// Get all entities sharing an address
    /// </summary>
    [HttpGet("addresses/{addressId}/shared-by")]
    public async Task<ActionResult<List<object>>> GetEntitiesSharingAddress(int addressId)
    {
        try
        {
            var entities = await _contactInfoService.GetEntitiesSharingAddressAsync(addressId);
            return Ok(entities.Select(e => new { entityType = e.EntityType.ToString(), entityId = e.EntityId, entityName = e.EntityName }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities sharing address {AddressId}", addressId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new standalone address
    /// </summary>
    [HttpPost("addresses")]
    public async Task<ActionResult<AddressDto>> CreateAddress([FromBody] CreateAddressDto dto)
    {
        try
        {
            var address = await _contactInfoService.CreateAddressAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetAddress), new { addressId = address.Id }, address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating address");
            return StatusCode(500, "An error occurred while creating the address");
        }
    }

    /// <summary>
    /// Link an address to an entity (creates new address if NewAddress provided)
    /// </summary>
    [HttpPost("addresses/link")]
    public async Task<ActionResult<LinkedAddressDto>> LinkAddress([FromBody] LinkAddressDto dto)
    {
        try
        {
            var linked = await _contactInfoService.LinkAddressAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetAddresses), new { entityType = dto.EntityType, entityId = dto.EntityId }, linked);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking address");
            return StatusCode(500, "An error occurred while linking the address");
        }
    }

    /// <summary>
    /// Update an address
    /// </summary>
    [HttpPut("addresses/{addressId}")]
    public async Task<ActionResult<AddressDto>> UpdateAddress(int addressId, [FromBody] CreateAddressDto dto)
    {
        try
        {
            var address = await _contactInfoService.UpdateAddressAsync(addressId, dto, GetCurrentUserId());
            return Ok(address);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating address {AddressId}", addressId);
            return StatusCode(500, "An error occurred while updating the address");
        }
    }

    /// <summary>
    /// Unlink an address from an entity
    /// </summary>
    [HttpDelete("addresses/link/{linkId}")]
    public async Task<ActionResult> UnlinkAddress(int linkId)
    {
        try
        {
            await _contactInfoService.UnlinkAddressAsync(linkId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking address link {LinkId}", linkId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete an address permanently
    /// </summary>
    [HttpDelete("addresses/{addressId}")]
    public async Task<ActionResult> DeleteAddress(int addressId)
    {
        try
        {
            await _contactInfoService.DeleteAddressAsync(addressId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting address {AddressId}", addressId);
            return StatusCode(500, "An error occurred while deleting the address");
        }
    }

    /// <summary>
    /// Set an address as primary for an entity
    /// </summary>
    [HttpPost("{entityType}/{entityId}/addresses/{addressId}/set-primary")]
    public async Task<ActionResult> SetPrimaryAddress(string entityType, int entityId, int addressId, [FromQuery] string addressTypeStr = "Primary")
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }
            if (!Enum.TryParse<AddressType>(addressTypeStr, true, out var addressType))
            {
                addressType = AddressType.Primary;
            }

            await _contactInfoService.SetPrimaryAddressAsync(type, entityId, addressId, addressType);
            return Ok(new { message = "Primary address updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary address");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Phone Endpoints

    /// <summary>
    /// Get all phone numbers for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}/phones")]
    public async Task<ActionResult<List<LinkedPhoneDto>>> GetPhoneNumbers(string entityType, int entityId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            var phones = await _contactInfoService.GetPhoneNumbersAsync(type, entityId);
            return Ok(phones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting phones for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving phone numbers");
        }
    }

    /// <summary>
    /// Get a specific phone number by ID
    /// </summary>
    [HttpGet("phones/{phoneId}")]
    public async Task<ActionResult<PhoneNumberDto>> GetPhoneNumber(int phoneId)
    {
        try
        {
            var phone = await _contactInfoService.GetPhoneNumberByIdAsync(phoneId);
            if (phone == null)
            {
                return NotFound();
            }
            return Ok(phone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting phone {PhoneId}", phoneId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get all entities sharing a phone number
    /// </summary>
    [HttpGet("phones/{phoneId}/shared-by")]
    public async Task<ActionResult<List<object>>> GetEntitiesSharingPhone(int phoneId)
    {
        try
        {
            var entities = await _contactInfoService.GetEntitiesSharingPhoneAsync(phoneId);
            return Ok(entities.Select(e => new { entityType = e.EntityType.ToString(), entityId = e.EntityId, entityName = e.EntityName }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities sharing phone {PhoneId}", phoneId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new standalone phone number
    /// </summary>
    [HttpPost("phones")]
    public async Task<ActionResult<PhoneNumberDto>> CreatePhoneNumber([FromBody] CreatePhoneNumberDto dto)
    {
        try
        {
            var phone = await _contactInfoService.CreatePhoneNumberAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetPhoneNumber), new { phoneId = phone.Id }, phone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating phone number");
            return StatusCode(500, "An error occurred while creating the phone number");
        }
    }

    /// <summary>
    /// Link a phone number to an entity
    /// </summary>
    [HttpPost("phones/link")]
    public async Task<ActionResult<LinkedPhoneDto>> LinkPhoneNumber([FromBody] LinkPhoneDto dto)
    {
        try
        {
            var linked = await _contactInfoService.LinkPhoneNumberAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetPhoneNumbers), new { entityType = dto.EntityType, entityId = dto.EntityId }, linked);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking phone number");
            return StatusCode(500, "An error occurred while linking the phone number");
        }
    }

    /// <summary>
    /// Update a phone number
    /// </summary>
    [HttpPut("phones/{phoneId}")]
    public async Task<ActionResult<PhoneNumberDto>> UpdatePhoneNumber(int phoneId, [FromBody] CreatePhoneNumberDto dto)
    {
        try
        {
            var phone = await _contactInfoService.UpdatePhoneNumberAsync(phoneId, dto, GetCurrentUserId());
            return Ok(phone);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phone {PhoneId}", phoneId);
            return StatusCode(500, "An error occurred while updating the phone number");
        }
    }

    /// <summary>
    /// Unlink a phone number from an entity
    /// </summary>
    [HttpDelete("phones/link/{linkId}")]
    public async Task<ActionResult> UnlinkPhoneNumber(int linkId)
    {
        try
        {
            await _contactInfoService.UnlinkPhoneNumberAsync(linkId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking phone link {LinkId}", linkId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a phone number
    /// </summary>
    [HttpDelete("phones/{phoneId}")]
    public async Task<ActionResult> DeletePhoneNumber(int phoneId)
    {
        try
        {
            await _contactInfoService.DeletePhoneNumberAsync(phoneId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting phone {PhoneId}", phoneId);
            return StatusCode(500, "An error occurred while deleting the phone number");
        }
    }

    /// <summary>
    /// Set a phone number as primary
    /// </summary>
    [HttpPost("{entityType}/{entityId}/phones/{phoneId}/set-primary")]
    public async Task<ActionResult> SetPrimaryPhone(string entityType, int entityId, int phoneId, [FromQuery] string phoneTypeStr = "Office")
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }
            if (!Enum.TryParse<PhoneType>(phoneTypeStr, true, out var phoneType))
            {
                phoneType = PhoneType.Office;
            }

            await _contactInfoService.SetPrimaryPhoneAsync(type, entityId, phoneId, phoneType);
            return Ok(new { message = "Primary phone updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary phone");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Email Endpoints

    /// <summary>
    /// Get all email addresses for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}/emails")]
    public async Task<ActionResult<List<LinkedEmailDto>>> GetEmailAddresses(string entityType, int entityId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            var emails = await _contactInfoService.GetEmailAddressesAsync(type, entityId);
            return Ok(emails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emails for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving email addresses");
        }
    }

    /// <summary>
    /// Get a specific email address by ID
    /// </summary>
    [HttpGet("emails/{emailId}")]
    public async Task<ActionResult<EmailAddressDto>> GetEmailAddress(int emailId)
    {
        try
        {
            var email = await _contactInfoService.GetEmailAddressByIdAsync(emailId);
            if (email == null)
            {
                return NotFound();
            }
            return Ok(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting email {EmailId}", emailId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Find an email address by address string
    /// </summary>
    [HttpGet("emails/find")]
    public async Task<ActionResult<EmailAddressDto>> FindEmailByAddress([FromQuery] string email)
    {
        try
        {
            var result = await _contactInfoService.FindEmailByAddressAsync(email);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding email {Email}", email);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Get all entities sharing an email
    /// </summary>
    [HttpGet("emails/{emailId}/shared-by")]
    public async Task<ActionResult<List<object>>> GetEntitiesSharingEmail(int emailId)
    {
        try
        {
            var entities = await _contactInfoService.GetEntitiesSharingEmailAsync(emailId);
            return Ok(entities.Select(e => new { entityType = e.EntityType.ToString(), entityId = e.EntityId, entityName = e.EntityName }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entities sharing email {EmailId}", emailId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new standalone email address
    /// </summary>
    [HttpPost("emails")]
    public async Task<ActionResult<EmailAddressDto>> CreateEmailAddress([FromBody] CreateEmailAddressDto dto)
    {
        try
        {
            var email = await _contactInfoService.CreateEmailAddressAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetEmailAddress), new { emailId = email.Id }, email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email address");
            return StatusCode(500, "An error occurred while creating the email address");
        }
    }

    /// <summary>
    /// Link an email address to an entity
    /// </summary>
    [HttpPost("emails/link")]
    public async Task<ActionResult<LinkedEmailDto>> LinkEmailAddress([FromBody] LinkEmailDto dto)
    {
        try
        {
            var linked = await _contactInfoService.LinkEmailAddressAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetEmailAddresses), new { entityType = dto.EntityType, entityId = dto.EntityId }, linked);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking email address");
            return StatusCode(500, "An error occurred while linking the email address");
        }
    }

    /// <summary>
    /// Update an email address
    /// </summary>
    [HttpPut("emails/{emailId}")]
    public async Task<ActionResult<EmailAddressDto>> UpdateEmailAddress(int emailId, [FromBody] CreateEmailAddressDto dto)
    {
        try
        {
            var email = await _contactInfoService.UpdateEmailAddressAsync(emailId, dto, GetCurrentUserId());
            return Ok(email);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email {EmailId}", emailId);
            return StatusCode(500, "An error occurred while updating the email address");
        }
    }

    /// <summary>
    /// Update email preferences for a link
    /// </summary>
    [HttpPut("emails/link/{linkId}/preferences")]
    public async Task<ActionResult> UpdateEmailPreferences(int linkId, [FromBody] EmailPreferencesDto dto)
    {
        try
        {
            await _contactInfoService.UpdateEmailPreferencesAsync(linkId, dto.DoNotEmail, dto.MarketingOptIn, dto.TransactionalOnly);
            return Ok(new { message = "Email preferences updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email preferences for link {LinkId}", linkId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Unlink an email address from an entity
    /// </summary>
    [HttpDelete("emails/link/{linkId}")]
    public async Task<ActionResult> UnlinkEmailAddress(int linkId)
    {
        try
        {
            await _contactInfoService.UnlinkEmailAddressAsync(linkId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking email link {LinkId}", linkId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete an email address
    /// </summary>
    [HttpDelete("emails/{emailId}")]
    public async Task<ActionResult> DeleteEmailAddress(int emailId)
    {
        try
        {
            await _contactInfoService.DeleteEmailAddressAsync(emailId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email {EmailId}", emailId);
            return StatusCode(500, "An error occurred while deleting the email address");
        }
    }

    /// <summary>
    /// Set an email address as primary
    /// </summary>
    [HttpPost("{entityType}/{entityId}/emails/{emailId}/set-primary")]
    public async Task<ActionResult> SetPrimaryEmail(string entityType, int entityId, int emailId, [FromQuery] string emailTypeStr = "General")
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }
            if (!Enum.TryParse<EmailType>(emailTypeStr, true, out var emailType))
            {
                emailType = EmailType.General;
            }

            await _contactInfoService.SetPrimaryEmailAsync(type, entityId, emailId, emailType);
            return Ok(new { message = "Primary email updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary email");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion

    #region Social Media Endpoints

    /// <summary>
    /// Get all social media accounts for an entity
    /// </summary>
    [HttpGet("{entityType}/{entityId}/social-media")]
    public async Task<ActionResult<List<LinkedSocialMediaDto>>> GetSocialMediaAccounts(string entityType, int entityId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            var accounts = await _contactInfoService.GetSocialMediaAccountsAsync(type, entityId);
            return Ok(accounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting social media for {EntityType} {EntityId}", entityType, entityId);
            return StatusCode(500, "An error occurred while retrieving social media accounts");
        }
    }

    /// <summary>
    /// Get a specific social media account by ID
    /// </summary>
    [HttpGet("social-media/{socialMediaId}")]
    public async Task<ActionResult<SocialMediaAccountDto>> GetSocialMediaAccount(int socialMediaId)
    {
        try
        {
            var account = await _contactInfoService.GetSocialMediaAccountByIdAsync(socialMediaId);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting social media account {SocialMediaId}", socialMediaId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Create a new standalone social media account
    /// </summary>
    [HttpPost("social-media")]
    public async Task<ActionResult<SocialMediaAccountDto>> CreateSocialMediaAccount([FromBody] CreateSocialMediaAccountDto dto)
    {
        try
        {
            var account = await _contactInfoService.CreateSocialMediaAccountAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetSocialMediaAccount), new { socialMediaId = account.Id }, account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating social media account");
            return StatusCode(500, "An error occurred while creating the social media account");
        }
    }

    /// <summary>
    /// Link a social media account to an entity
    /// </summary>
    [HttpPost("social-media/link")]
    public async Task<ActionResult<LinkedSocialMediaDto>> LinkSocialMediaAccount([FromBody] LinkSocialMediaDto dto)
    {
        try
        {
            var linked = await _contactInfoService.LinkSocialMediaAccountAsync(dto, GetCurrentUserId());
            return CreatedAtAction(nameof(GetSocialMediaAccounts), new { entityType = dto.EntityType, entityId = dto.EntityId }, linked);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking social media account");
            return StatusCode(500, "An error occurred while linking the social media account");
        }
    }

    /// <summary>
    /// Update a social media account
    /// </summary>
    [HttpPut("social-media/{socialMediaId}")]
    public async Task<ActionResult<SocialMediaAccountDto>> UpdateSocialMediaAccount(int socialMediaId, [FromBody] CreateSocialMediaAccountDto dto)
    {
        try
        {
            var account = await _contactInfoService.UpdateSocialMediaAccountAsync(socialMediaId, dto, GetCurrentUserId());
            return Ok(account);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating social media account {SocialMediaId}", socialMediaId);
            return StatusCode(500, "An error occurred while updating the social media account");
        }
    }

    /// <summary>
    /// Unlink a social media account from an entity
    /// </summary>
    [HttpDelete("social-media/link/{linkId}")]
    public async Task<ActionResult> UnlinkSocialMediaAccount(int linkId)
    {
        try
        {
            await _contactInfoService.UnlinkSocialMediaAccountAsync(linkId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking social media link {LinkId}", linkId);
            return StatusCode(500, "An error occurred");
        }
    }

    /// <summary>
    /// Delete a social media account
    /// </summary>
    [HttpDelete("social-media/{socialMediaId}")]
    public async Task<ActionResult> DeleteSocialMediaAccount(int socialMediaId)
    {
        try
        {
            await _contactInfoService.DeleteSocialMediaAccountAsync(socialMediaId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting social media account {SocialMediaId}", socialMediaId);
            return StatusCode(500, "An error occurred while deleting the social media account");
        }
    }

    /// <summary>
    /// Set a social media account as primary
    /// </summary>
    [HttpPost("{entityType}/{entityId}/social-media/{socialMediaId}/set-primary")]
    public async Task<ActionResult> SetPrimarySocialMedia(string entityType, int entityId, int socialMediaId)
    {
        try
        {
            if (!Enum.TryParse<EntityType>(entityType, true, out var type))
            {
                return BadRequest($"Invalid entity type: {entityType}");
            }

            await _contactInfoService.SetPrimarySocialMediaAsync(type, entityId, socialMediaId);
            return Ok(new { message = "Primary social media updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting primary social media");
            return StatusCode(500, "An error occurred");
        }
    }

    #endregion
}

/// <summary>
/// DTO for updating email preferences
/// </summary>
public class EmailPreferencesDto
{
    public bool DoNotEmail { get; set; }
    public bool MarketingOptIn { get; set; }
    public bool TransactionalOnly { get; set; }
}
