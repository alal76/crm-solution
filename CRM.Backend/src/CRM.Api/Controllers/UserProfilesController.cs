// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing user profiles
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfilesController : ControllerBase
{
    private readonly IRepository<UserProfile> _profileRepository;
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<UserProfilesController> _logger;

    public UserProfilesController(
        IRepository<UserProfile> profileRepository,
        IRepository<Department> departmentRepository,
        IRepository<User> userRepository,
        ILogger<UserProfilesController> logger)
    {
        _profileRepository = profileRepository;
        _departmentRepository = departmentRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all user profiles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetProfiles()
    {
        try
        {
            var profiles = await _profileRepository.GetAllAsync();
            var profileDtos = profiles
                .Where(p => !p.IsDeleted)
                .Select(p => MapToDto(p))
                .ToList();

            return Ok(profileDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profiles");
            return StatusCode(500, new { message = "Error retrieving profiles", error = ex.Message });
        }
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUserProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "User not authenticated" });

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            if (!user.UserProfileId.HasValue)
                return NotFound(new { message = "User has no profile assigned" });

            var profile = await _profileRepository.GetByIdAsync(user.UserProfileId.Value);
            if (profile == null || profile.IsDeleted)
                return NotFound(new { message = "Profile not found" });

            return Ok(MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user profile");
            return StatusCode(500, new { message = "Error retrieving current user profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Get profiles by department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserProfileDto>>> GetProfilesByDepartment(int departmentId)
    {
        try
        {
            var profiles = await _profileRepository.GetAllAsync();
            var departmentProfiles = profiles
                .Where(p => !p.IsDeleted && p.DepartmentId == departmentId)
                .Select(p => MapToDto(p))
                .ToList();

            return Ok(departmentProfiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profiles for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "Error retrieving profiles", error = ex.Message });
        }
    }

    /// <summary>
    /// Get profile by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfileById(int id)
    {
        try
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null || profile.IsDeleted)
                return NotFound(new { message = "Profile not found" });

            return Ok(MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile {Id}", id);
            return StatusCode(500, new { message = "Error retrieving profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new user profile
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> CreateProfile([FromBody] CreateUserProfileDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                return BadRequest(new { message = "Profile name is required" });

            if (createDto.DepartmentId <= 0)
                return BadRequest(new { message = "Valid department ID is required" });

            // Verify department exists
            var department = await _departmentRepository.GetByIdAsync(createDto.DepartmentId);
            if (department == null || department.IsDeleted)
                return BadRequest(new { message = "Department not found" });

            var profile = new UserProfile
            {
                Name = createDto.Name,
                Description = createDto.Description,
                DepartmentId = createDto.DepartmentId,
                AccessiblePages = JsonSerializer.Serialize(createDto.AccessiblePages),
                CanCreateAccounts = createDto.CanCreateAccounts,
                CanEditAccounts = createDto.CanEditAccounts,
                CanDeleteAccounts = createDto.CanDeleteAccounts,
                CanCreateOpportunities = createDto.CanCreateOpportunities,
                CanEditOpportunities = createDto.CanEditOpportunities,
                CanDeleteOpportunities = createDto.CanDeleteOpportunities,
                CanCreateProducts = createDto.CanCreateProducts,
                CanEditProducts = createDto.CanEditProducts,
                CanDeleteProducts = createDto.CanDeleteProducts,
                CanManageCampaigns = createDto.CanManageCampaigns,
                CanViewReports = createDto.CanViewReports,
                CanManageUsers = createDto.CanManageUsers,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _profileRepository.AddAsync(profile);
            await _profileRepository.SaveAsync();

            return CreatedAtAction(nameof(GetProfileById), new { id = profile.Id }, MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating profile");
            return StatusCode(500, new { message = "Error creating profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a user profile
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(int id, [FromBody] CreateUserProfileDto updateDto)
    {
        try
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null || profile.IsDeleted)
                return NotFound(new { message = "Profile not found" });

            profile.Name = updateDto.Name;
            profile.Description = updateDto.Description;
            profile.AccessiblePages = JsonSerializer.Serialize(updateDto.AccessiblePages);
            profile.CanCreateAccounts = updateDto.CanCreateAccounts;
            profile.CanEditAccounts = updateDto.CanEditAccounts;
            profile.CanDeleteAccounts = updateDto.CanDeleteAccounts;
            profile.CanCreateOpportunities = updateDto.CanCreateOpportunities;
            profile.CanEditOpportunities = updateDto.CanEditOpportunities;
            profile.CanDeleteOpportunities = updateDto.CanDeleteOpportunities;
            profile.CanCreateProducts = updateDto.CanCreateProducts;
            profile.CanEditProducts = updateDto.CanEditProducts;
            profile.CanDeleteProducts = updateDto.CanDeleteProducts;
            profile.CanManageCampaigns = updateDto.CanManageCampaigns;
            profile.CanViewReports = updateDto.CanViewReports;
            profile.CanManageUsers = updateDto.CanManageUsers;
            profile.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(profile);
            await _profileRepository.SaveAsync();

            return Ok(MapToDto(profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile {Id}", id);
            return StatusCode(500, new { message = "Error updating profile", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a user profile
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(int id)
    {
        try
        {
            var profile = await _profileRepository.GetByIdAsync(id);
            if (profile == null || profile.IsDeleted)
                return NotFound(new { message = "Profile not found" });

            profile.IsDeleted = true;
            profile.UpdatedAt = DateTime.UtcNow;

            await _profileRepository.UpdateAsync(profile);
            await _profileRepository.SaveAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting profile {Id}", id);
            return StatusCode(500, new { message = "Error deleting profile", error = ex.Message });
        }
    }

    private UserProfileDto MapToDto(UserProfile profile)
    {
        var accessiblePages = new List<string>();
        try
        {
            accessiblePages = JsonSerializer.Deserialize<List<string>>(profile.AccessiblePages) ?? new();
        }
        catch
        {
        }

        return new UserProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Description = profile.Description,
            DepartmentId = profile.DepartmentId,
            DepartmentName = profile.Department?.Name ?? string.Empty,
            IsActive = profile.IsActive,
            AccessiblePages = accessiblePages,
            CanCreateAccounts = profile.CanCreateAccounts,
            CanEditAccounts = profile.CanEditAccounts,
            CanDeleteAccounts = profile.CanDeleteAccounts,
            CanCreateOpportunities = profile.CanCreateOpportunities,
            CanEditOpportunities = profile.CanEditOpportunities,
            CanDeleteOpportunities = profile.CanDeleteOpportunities,
            CanCreateProducts = profile.CanCreateProducts,
            CanEditProducts = profile.CanEditProducts,
            CanDeleteProducts = profile.CanDeleteProducts,
            CanManageCampaigns = profile.CanManageCampaigns,
            CanViewReports = profile.CanViewReports,
            CanManageUsers = profile.CanManageUsers,
            CreatedAt = profile.CreatedAt,
            UserCount = profile.Users?.Count ?? 0
        };
    }
}
