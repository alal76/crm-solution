using CRM.Api.Infrastructure;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Controllers;

[ApiController]
[Route("api/apikeys")]
[Authorize]
public class ApiKeysController : CrmControllerBase
{
    private readonly ICrmDbContext _db;
    private readonly ILogger<ApiKeysController> _logger;

    public ApiKeysController(ICrmDbContext db, ILogger<ApiKeysController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var keys = await _db.ApiKeys.AsNoTracking()
            .Where(k => !k.IsDeleted)
            .Select(k => new
            {
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.UserId,
                k.ExpiresAt,
                k.IsActive,
                k.Scopes,
                k.LastUsedAt,
                k.CreatedAt,
                k.UpdatedAt
            })
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
        return Ok(keys);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var key = await _db.ApiKeys.AsNoTracking()
            .Where(k => k.Id == id && !k.IsDeleted)
            .Select(k => new
            {
                k.Id,
                k.Name,
                k.KeyPrefix,
                k.UserId,
                k.ExpiresAt,
                k.IsActive,
                k.Scopes,
                k.LastUsedAt,
                k.CreatedAt,
                k.UpdatedAt
            })
            .FirstOrDefaultAsync(ct);
        return key == null ? NotFound(new { message = "API key not found" }) : Ok(key);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ApiKey dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Name is required." });

        // Generate a random API key
        var rawKey = $"crm_{Guid.NewGuid():N}";
        dto.KeyPrefix = rawKey[..12];
        dto.KeyHash = BCrypt.Net.BCrypt.HashPassword(rawKey);
        dto.CreatedAt = DateTime.UtcNow;
        dto.UpdatedAt = DateTime.UtcNow;
        _db.ApiKeys.Add(dto);
        await _db.SaveChangesAsync(ct);

        // Return the raw key only on creation (cannot be retrieved later)
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, new
        {
            dto.Id,
            dto.Name,
            Key = rawKey,
            dto.KeyPrefix,
            dto.ExpiresAt,
            dto.IsActive,
            dto.Scopes,
            dto.CreatedAt
        });
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _db.ApiKeys
            .FirstOrDefaultAsync(k => k.Id == id && !k.IsDeleted, ct);
        if (existing == null)
            return NotFound(new { message = "API key not found" });

        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
