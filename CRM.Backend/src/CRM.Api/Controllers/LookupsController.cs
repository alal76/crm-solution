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
