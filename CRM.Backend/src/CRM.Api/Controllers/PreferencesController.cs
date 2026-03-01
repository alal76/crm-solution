// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Preferences management endpoints for Accounts and Contacts.
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
public class PreferencesController : CrmControllerBase
{
    private readonly IPreferencesService _preferencesService;

    public PreferencesController(IPreferencesService preferencesService)
    {
        _preferencesService = preferencesService;
    }

    [HttpGet("preferences/{id:int}")]
    [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _preferencesService.GetByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = "Preferences not found" });
        }

        return Ok(result);
    }

    [HttpGet("accounts/{accountId:int}/preferences")]
    [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountPreferences(int accountId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _preferencesService.GetAccountDefaultsAsync(accountId, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("accounts/{accountId:int}/preferences")]
    [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAccountPreferences(int accountId, [FromBody] PreferencesDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _preferencesService.UpdateAccountPreferencesAsync(accountId, dto, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("contacts/{contactId:int}/preferences")]
    [ProducesResponseType(typeof(ContactPreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactPreferences(int contactId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _preferencesService.GetContactPreferencesAsync(contactId, false, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("contacts/{contactId:int}/preferences/effective")]
    [ProducesResponseType(typeof(ContactPreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEffectiveContactPreferences(int contactId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _preferencesService.GetContactPreferencesAsync(contactId, true, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("contacts/{contactId:int}/preferences")]
    [ProducesResponseType(typeof(PreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContactPreferences(int contactId, [FromBody] PreferencesDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _preferencesService.UpdateContactPreferencesAsync(contactId, dto, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("contacts/{contactId:int}/preferences/use-custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UseCustomPreferences(int contactId, CancellationToken cancellationToken)
    {
        try
        {
            await _preferencesService.SetContactUseCustomPreferencesAsync(contactId, true, cancellationToken);
            return Ok(new { message = "Contact now uses custom preferences." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("contacts/{contactId:int}/preferences/reset-to-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetToAccountDefaults(int contactId, CancellationToken cancellationToken)
    {
        try
        {
            await _preferencesService.ResetContactToAccountAsync(contactId, cancellationToken);
            return Ok(new { message = "Contact preferences reset to account defaults." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
