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
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookupsController : ControllerBase
{
    private readonly ICrmDbContext _context;

    public LookupsController(ICrmDbContext context)
    {
        _context = context;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var cats = await _context.LookupCategories.Where(c => c.IsActive).ToListAsync();
        return Ok(cats.Select(c => new { c.Id, c.Name, c.Description }));
    }

    [HttpGet("items/{categoryName}")]
    public async Task<IActionResult> GetItems(string categoryName)
    {
        var cat = await _context.LookupCategories.FirstOrDefaultAsync(c => c.Name.ToLower() == categoryName.ToLower());
        if (cat == null) return NotFound();

        var items = await _context.LookupItems.Where(i => i.LookupCategoryId == cat.Id && i.IsActive).OrderBy(i => i.SortOrder).ToListAsync();
        return Ok(items.Select(i => new { i.Id, i.Key, i.Value, i.Meta }));
    }
}
