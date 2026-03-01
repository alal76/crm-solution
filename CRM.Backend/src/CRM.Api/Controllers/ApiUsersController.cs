// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRM.Api.Infrastructure;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing API users and their API keys.
/// API users authenticate via X-Api-Key header instead of interactive login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class ApiUsersController : CrmControllerBase
{
    private const string ApiUserNotFoundMessage = "API user not found";

    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<ApiUsersController> _logger;

    public ApiUsersController(
        ICrmDbContext dbContext,
        ILogger<ApiUsersController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get all API users.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApiUserDto>>> GetAll()
    {
                var apiUsers = await _dbContext.Users
            .Where(u => !u.IsDeleted && u.IsApiUser)
            .Include(u => u.PrimaryGroup)
            .OrderBy(u => u.Username)
            .Select(u => new ApiUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = ((UserRole)u.Role).ToString(),
                IsActive = u.IsActive,
                ApiKeyPrefix = u.ApiKeyPrefix,
                ApiKeyCreatedAt = u.ApiKeyCreatedAt,
                ApiKeyLastUsedAt = u.ApiKeyLastUsedAt,
                ApiKeyExpiresAt = u.ApiKeyExpiresAt,
                ApiUserDescription = u.ApiUserDescription,
                PrimaryGroupId = u.PrimaryGroupId,
                PrimaryGroupName = u.PrimaryGroup != null ? u.PrimaryGroup.Name : null,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return Ok(apiUsers);
    }

    /// <summary>
    /// Get an API user by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiUserDto>> GetById(int id)
    {
                var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        return Ok(MapToApiUserDto(user));
    }

    /// <summary>
    /// Create a new API user and generate an API key.
    /// The raw API key is only returned in this response — store it securely.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiKeyResponse>> Create([FromBody] CreateApiUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required" });
        }
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { error = "Email is required" });
        }

        // Check for duplicate email
        var exists = await _dbContext.Users
            .AnyAsync(u => !u.IsDeleted && u.Email.ToLower() == request.Email.Trim().ToLower());
        if (exists)
        {
            return Conflict(new { error = "A user with this email already exists" });
        }

        // Validate group is an API group if specified
        if (request.PrimaryGroupId.HasValue)
        {
            var group = await _dbContext.UserGroups
                .FirstOrDefaultAsync(g => g.Id == request.PrimaryGroupId.Value && !g.IsDeleted);
            if (group == null)
            {
                return BadRequest(new { error = "Specified group not found" });
            }
            if (!group.IsApiGroup)
            {
                return BadRequest(new { error = "API users can only be assigned to API groups. The specified group is not an API group." });
            }
        }

        // Generate API key
        var (rawKey, hash, prefix) = ApiKeyAuthenticationHandler.GenerateApiKey();

        var nameParts = request.Name.Trim().Split(' ', 2);
        var user = new User
        {
            Email = request.Email.Trim(),
            Username = request.Name.Trim().Replace(" ", "_").ToLower(),
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : "API",
            PasswordHash = string.Empty,
            Role = request.RoleId,
            IsActive = true,
            IsApiUser = true,
            ApiKeyHash = hash,
            ApiKeyPrefix = prefix,
            ApiKeyCreatedAt = DateTime.UtcNow,
            ApiKeyExpiresAt = request.ExpiresAt,
            ApiUserDescription = request.Description,
            PrimaryGroupId = request.PrimaryGroupId,
            PasswordNeverSet = true,
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created API user {Username} (ID: {UserId}) with key prefix {Prefix}",
            user.Username, user.Id, prefix);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new ApiKeyResponse
        {
            UserId = user.Id,
            Username = user.Username,
            ApiKey = rawKey,
            ApiKeyPrefix = prefix,
            CreatedAt = user.ApiKeyCreatedAt!.Value,
            ExpiresAt = user.ApiKeyExpiresAt,
        });
    }

    /// <summary>
    /// Regenerate the API key for an existing API user.
    /// The old key is immediately invalidated. The new raw key is only shown once.
    /// </summary>
    [HttpPost("{id}/regenerate-key")]
    [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiKeyResponse>> RegenerateKey(int id)
    {
                var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        var (rawKey, hash, prefix) = ApiKeyAuthenticationHandler.GenerateApiKey();

        user.ApiKeyHash = hash;
        user.ApiKeyPrefix = prefix;
        user.ApiKeyCreatedAt = DateTime.UtcNow;
        user.ApiKeyLastUsedAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Regenerated API key for user {Username} (ID: {UserId})", user.Username, user.Id);

        return Ok(new ApiKeyResponse
        {
            UserId = user.Id,
            Username = user.Username,
            ApiKey = rawKey,
            ApiKeyPrefix = prefix,
            CreatedAt = user.ApiKeyCreatedAt!.Value,
            ExpiresAt = user.ApiKeyExpiresAt,
        });
    }

    /// <summary>
    /// Revoke the API key for an API user (deactivates the user).
    /// </summary>
    [HttpPost("{id}/revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Revoke(int id)
    {
                var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        user.IsActive = false;
        user.ApiKeyHash = null;
        user.ApiKeyPrefix = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Revoked API key for user {Username} (ID: {UserId})", user.Username, user.Id);

        return Ok(new { message = "API key revoked and user deactivated" });
    }

    /// <summary>
    /// Update an API user's details (description, role, group, expiration).
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiUserDto>> Update(int id, [FromBody] CreateApiUserRequest request) // NOSONAR
    {
                var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        // Validate group is an API group if specified
        if (request.PrimaryGroupId.HasValue)
        {
            var group = await _dbContext.UserGroups
                .FirstOrDefaultAsync(g => g.Id == request.PrimaryGroupId.Value && !g.IsDeleted);
            if (group == null)
            {
                return BadRequest(new { error = "Specified group not found" });
            }
            if (!group.IsApiGroup)
            {
                return BadRequest(new { error = "API users can only be assigned to API groups." });
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var nameParts = request.Name.Trim().Split(' ', 2);
            user.FirstName = nameParts[0];
            user.LastName = nameParts.Length > 1 ? nameParts[1] : "API";
            user.Username = request.Name.Trim().Replace(" ", "_").ToLower();
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.Trim().ToLower() != user.Email.ToLower())
        {
            var emailExists = await _dbContext.Users
                .AnyAsync(u => !u.IsDeleted && u.Id != id && u.Email.ToLower() == request.Email.Trim().ToLower());
            if (emailExists)
            {
                return Conflict(new { error = "A user with this email already exists" });
            }
            user.Email = request.Email.Trim();
        }

        user.Role = request.RoleId;
        user.PrimaryGroupId = request.PrimaryGroupId;
        user.ApiKeyExpiresAt = request.ExpiresAt;
        user.ApiUserDescription = request.Description;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Reload with group name
        var updated = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == id);

        return Ok(MapToApiUserDto(updated!));
    }

    /// <summary>
    /// Delete an API user (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
                var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.ApiKeyHash = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted API user {Username} (ID: {UserId})", user.Username, user.Id);

        return Ok(new { message = "API user deleted" });
    }

    /// <summary>
    /// Toggle active/inactive status for an API user.
    /// </summary>
    [HttpPost("{id}/toggle-status")]
    [ProducesResponseType(typeof(ApiUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiUserDto>> ToggleStatus(int id)
    {
                var user = await _dbContext.Users
            .Include(u => u.PrimaryGroup)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted && u.IsApiUser);

        if (user == null)
        {
            return NotFound(new { error = ApiUserNotFoundMessage });
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Toggled API user status: {Username} (ID: {UserId}) is now {Status}",
            user.Username, user.Id, user.IsActive ? "active" : "inactive");

        return Ok(MapToApiUserDto(user));
    }

    private static ApiUserDto MapToApiUserDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Role = ((UserRole)user.Role).ToString(),
        IsActive = user.IsActive,
        ApiKeyPrefix = user.ApiKeyPrefix,
        ApiKeyCreatedAt = user.ApiKeyCreatedAt,
        ApiKeyLastUsedAt = user.ApiKeyLastUsedAt,
        ApiKeyExpiresAt = user.ApiKeyExpiresAt,
        ApiUserDescription = user.ApiUserDescription,
        PrimaryGroupId = user.PrimaryGroupId,
        PrimaryGroupName = user.PrimaryGroup?.Name,
        CreatedAt = user.CreatedAt,
    };
}
