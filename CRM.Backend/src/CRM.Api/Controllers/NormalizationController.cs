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

using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// API controller for normalization operations (tags, custom fields, contact info lookups).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class NormalizationController : ControllerBase
{
    private readonly INormalizationService _service;
    private readonly ILogger<NormalizationController> _logger;

    public NormalizationController(INormalizationService service, ILogger<NormalizationController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Tags & Custom Fields

    /// <summary>Gets tags for an entity.</summary>
    [HttpGet("tags/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetTags(string entityType, int entityId)
    {
        try
        {
            var tags = await _service.GetTagsAsync(entityType, entityId);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags for {EntityType} {EntityId}", entityType, entityId);
            return Problem("An error occurred while retrieving tags.");
        }
    }

    /// <summary>Gets custom fields for an entity.</summary>
    [HttpGet("custom-fields/{entityType}/{entityId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetCustomFields(string entityType, int entityId)
    {
        try
        {
            var fields = await _service.GetCustomFieldsAsync(entityType, entityId);
            return Ok(fields);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving custom fields for {EntityType} {EntityId}", entityType, entityId);
            return Problem("An error occurred while retrieving custom fields.");
        }
    }

    #endregion

    #region Contact Info Lookups

    /// <summary>Gets the primary contact detail value for an entity.</summary>
    [HttpGet("primary-contact-detail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetPrimaryContactDetail(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId,
        [FromQuery, Required] ContactDetailType detailType)
    {
        try
        {
            var value = await _service.GetPrimaryContactDetailValueAsync(ownerType, ownerId, detailType);
            return Ok(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary contact detail for {OwnerType} {OwnerId}", ownerType, ownerId);
            return Problem("An error occurred while retrieving the primary contact detail.");
        }
    }

    /// <summary>Gets the primary email for an entity.</summary>
    [HttpGet("primary-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetPrimaryEmail(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId)
    {
        try
        {
            var email = await _service.GetPrimaryEmailAsync(ownerType, ownerId);
            return Ok(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary email for {OwnerType} {OwnerId}", ownerType, ownerId);
            return Problem("An error occurred while retrieving the primary email.");
        }
    }

    /// <summary>Gets the primary phone for an entity.</summary>
    [HttpGet("primary-phone")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetPrimaryPhone(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId)
    {
        try
        {
            var phone = await _service.GetPrimaryPhoneAsync(ownerType, ownerId);
            return Ok(phone);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary phone for {OwnerType} {OwnerId}", ownerType, ownerId);
            return Problem("An error occurred while retrieving the primary phone.");
        }
    }

    /// <summary>Gets the primary fax for an entity.</summary>
    [HttpGet("primary-fax")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetPrimaryFax(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId)
    {
        try
        {
            var fax = await _service.GetPrimaryFaxAsync(ownerType, ownerId);
            return Ok(fax);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary fax for {OwnerType} {OwnerId}", ownerType, ownerId);
            return Problem("An error occurred while retrieving the primary fax.");
        }
    }

    /// <summary>Gets the primary address for an entity.</summary>
    [HttpGet("primary-address")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Address>> GetPrimaryAddress(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId)
    {
        try
        {
            var address = await _service.GetPrimaryAddressAsync(ownerType, ownerId);
            return Ok(address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary address for {OwnerType} {OwnerId}", ownerType, ownerId);
            return Problem("An error occurred while retrieving the primary address.");
        }
    }

    /// <summary>Gets the primary social account for an entity.</summary>
    [HttpGet("primary-social")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> GetPrimarySocialAccount(
        [FromQuery, Required] ContactInfoOwnerType ownerType,
        [FromQuery, Required] int ownerId,
        [FromQuery, Required] SocialNetwork network)
    {
        try
        {
            var account = await _service.GetPrimarySocialAccountAsync(ownerType, ownerId, network);
            return Ok(account);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving primary social account for {OwnerType} {OwnerId} {Network}", ownerType, ownerId, network);
            return Problem("An error occurred while retrieving the primary social account.");
        }
    }

    #endregion
}
