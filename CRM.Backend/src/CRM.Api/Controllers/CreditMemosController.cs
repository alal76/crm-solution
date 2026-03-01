// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CreditMemosController : CrmControllerBase
{
    private readonly ICreditMemoService _service;
    private readonly ILogger<CreditMemosController> _logger;

    public CreditMemosController(ICreditMemoService service, ILogger<CreditMemosController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CreditMemo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CreditMemo>>> GetAll([FromQuery] int? accountId = null, [FromQuery] CreditMemoStatus? status = null, CancellationToken ct = default)
    {
        try
        {
            var items = await _service.GetAllAsync(accountId, status, ct);
            return Ok(items);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditMemo>> GetById(int id, CancellationToken ct = default)
    {
        try
        {
            var item = await _service.GetByIdAsync(id, ct);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpGet("by-number/{number}")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditMemo>> GetByNumber(string number, CancellationToken ct = default)
    {
        try
        {
            var item = await _service.GetByCreditMemoNumberAsync(number, ct);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditMemo>> Create([FromBody] CreditMemo model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var created = await _service.CreateAsync(model, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditMemo>> Update(int id, [FromBody] CreditMemo model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (id != model.Id)
        {
            return BadRequest("Id mismatch");
        }
        try
        {
            var updated = await _service.UpdateAsync(model, ct);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id, ct);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPost("{id:int}/apply")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditMemo>> Apply(int id, [FromBody] ApplyCreditMemoRequest req, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var cm = await _service.ApplyAsync(id, req.InvoiceId, ct);
            return Ok(cm);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPost("{id:int}/unapply")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreditMemo>> Unapply(int id, CancellationToken ct = default)
    {
        try
        {
            var cm = await _service.UnapplyAsync(id, ct);
            return Ok(cm);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPost("{id:int}/refund")]
    [ProducesResponseType(typeof(CreditMemo), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreditMemo>> Refund(int id, CancellationToken ct = default)
    {
        try
        {
            var cm = await _service.RefundAsync(id, ct);
            return Ok(cm);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpGet("{id:int}/line-items")]
    [ProducesResponseType(typeof(IEnumerable<CreditMemoLineItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CreditMemoLineItem>>> GetLineItems(int id, CancellationToken ct = default)
    {
        try
        {
            var items = await _service.GetLineItemsAsync(id, ct);
            return Ok(items);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPost("{id:int}/line-items")]
    [ProducesResponseType(typeof(CreditMemoLineItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreditMemoLineItem>> AddLineItem(int id, [FromBody] CreditMemoLineItem model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        try
        {
            var li = await _service.AddLineItemAsync(id, model, ct);
            return CreatedAtAction(nameof(GetLineItems), new { id }, li);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpPut("line-items/{lineItemId:int}")]
    [ProducesResponseType(typeof(CreditMemoLineItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CreditMemoLineItem>> UpdateLineItem(int lineItemId, [FromBody] CreditMemoLineItem model, CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (lineItemId != model.Id)
        {
            return BadRequest("Id mismatch");
        }
        try
        {
            var updated = await _service.UpdateLineItemAsync(model, ct);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    [HttpDelete("line-items/{lineItemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteLineItem(int lineItemId, CancellationToken ct = default)
    {
        try
        {
            var deleted = await _service.RemoveLineItemAsync(lineItemId, ct);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return HandleServiceException(ex);
        }
    }

    private ActionResult HandleServiceException(Exception ex)
    {
        _logger.LogError(ex, "Service error");
        if (ex is InvalidOperationException)
        {
            return NotFound(new { error = ex.Message });
        }
        if (ex is ArgumentException)
        {
            return BadRequest(new { error = ex.Message });
        }
        return Problem(detail: ex.Message, statusCode: 500);
    }

    public class ApplyCreditMemoRequest
    {
        public int InvoiceId { get; set; }
    }
}
