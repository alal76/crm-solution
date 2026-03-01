// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Web-to-Lead Forms Controller (TODO-CRM002-04)
/// Manages web forms that convert to leads.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WebToLeadFormsController : CrmControllerBase
{
    private readonly IWebToLeadFormService _service;

    public WebToLeadFormsController(IWebToLeadFormService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets all web-to-lead forms.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WebToLeadForm>>> GetAll(CancellationToken cancellationToken)
    {
        var forms = await _service.GetAllAsync(cancellationToken);
        return Ok(forms);
    }

    /// <summary>
    /// Gets a form by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WebToLeadForm>> GetById(int id, CancellationToken cancellationToken)
    {
        var form = await _service.GetByIdAsync(id, cancellationToken);
        if (form == null)
        {
            return NotFound();
        }
        return Ok(form);
    }

    /// <summary>
    /// Gets a form by embed key (for public embedding).
    /// </summary>
    [AllowAnonymous]
    [HttpGet("embed/{embedKey}")]
    public async Task<ActionResult<WebToLeadForm>> GetByEmbedKey(string embedKey, CancellationToken cancellationToken)
    {
        var form = await _service.GetByEmbedKeyAsync(embedKey, cancellationToken);
        if (form == null)
        {
            return NotFound();
        }
        return Ok(form);
    }

    /// <summary>
    /// Creates a new web-to-lead form.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WebToLeadForm>> Create([FromBody] WebToLeadForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(form, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing web-to-lead form.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WebToLeadForm>> Update(int id, [FromBody] WebToLeadForm form, CancellationToken cancellationToken)
    {
        if (id != form.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, form, cancellationToken);
        if (updated == null)
        {
            return NotFound();
        }
        return Ok(updated);
    }

    /// <summary>
    /// Soft deletes a web-to-lead form.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await _service.DeleteAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Public endpoint to submit a web-to-lead form.
    /// Converts submission to a new Lead.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("{embedKey}/submit")]
    public async Task<ActionResult<Lead>> SubmitForm(string embedKey, [FromBody] Dictionary<string, string> formData, CancellationToken cancellationToken)
    {
        try
        {
            var submission = new WebToLeadSubmissionDto
            {
                FormEmbedKey = embedKey,
                FieldValues = formData,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            };
            var result = await _service.ProcessSubmissionAsync(submission, cancellationToken);
            if (!result.Success)
            {
                return result.ErrorMessage?.Contains("not found") == true ? NotFound(result.ErrorMessage) : BadRequest(result.ErrorMessage);
            }
            return Ok(new { leadId = result.LeadId, message = "Lead created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Generates embed code for the form.
    /// </summary>
    [HttpGet("{id:int}/embed-code")]
    public async Task<ActionResult<string>> GetEmbedCode(int id, CancellationToken cancellationToken)
    {
        var form = await _service.GetByIdAsync(id, cancellationToken);
        if (form == null)
        {
            return NotFound();
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var embedCode = $@"<iframe src=""{baseUrl}/forms/{form.EmbedKey}"" width=""100%"" height=""600"" frameborder=""0""></iframe>";
        return Ok(new { embedCode, embedKey = form.EmbedKey });
    }
}
