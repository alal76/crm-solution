using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CRM.Api.Controllers;

/// <summary>
/// Controller for managing departments
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IRepository<Department> _departmentRepository;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IRepository<Department> departmentRepository,
        ILogger<DepartmentsController> logger)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        try
        {
            var departments = await _departmentRepository.GetAllAsync();
            var departmentDtos = departments
                .Where(d => !d.IsDeleted)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    DepartmentCode = d.DepartmentCode,
                    IsActive = d.IsActive,
                    ParentDepartmentId = d.ParentDepartmentId,
                    CreatedAt = d.CreatedAt,
                    UserCount = d.Users?.Count ?? 0
                })
                .ToList();

            return Ok(departmentDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving departments");
            return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
        }
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDetailDto>> GetDepartmentById(int id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null || department.IsDeleted)
                return NotFound(new { message = "Department not found" });

            var dto = new DepartmentDetailDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                DepartmentCode = department.DepartmentCode,
                IsActive = department.IsActive,
                ParentDepartmentId = department.ParentDepartmentId,
                CreatedAt = department.CreatedAt,
                UserCount = department.Users?.Count ?? 0,
                Users = department.Users?
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        IsActive = u.IsActive
                    })
                    .ToList() ?? new(),
                Profiles = department.Profiles?
                    .Select(p => new UserProfileDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        IsActive = p.IsActive
                    })
                    .ToList() ?? new()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving department {Id}", id);
            return StatusCode(500, new { message = "Error retrieving department", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
                return BadRequest(new { message = "Department name is required" });

            var department = new Department
            {
                Name = createDto.Name,
                Description = createDto.Description,
                DepartmentCode = createDto.DepartmentCode,
                ParentDepartmentId = createDto.ParentDepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveAsync();

            var dto = new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                DepartmentCode = department.DepartmentCode,
                IsActive = department.IsActive,
                ParentDepartmentId = department.ParentDepartmentId,
                CreatedAt = department.CreatedAt,
                UserCount = 0
            };

            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(500, new { message = "Error creating department", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a department
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, [FromBody] CreateDepartmentDto updateDto)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null || department.IsDeleted)
                return NotFound(new { message = "Department not found" });

            department.Name = updateDto.Name;
            department.Description = updateDto.Description;
            department.DepartmentCode = updateDto.DepartmentCode;
            department.ParentDepartmentId = updateDto.ParentDepartmentId;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            await _departmentRepository.SaveAsync();

            var dto = new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                DepartmentCode = department.DepartmentCode,
                IsActive = department.IsActive,
                ParentDepartmentId = department.ParentDepartmentId,
                CreatedAt = department.CreatedAt,
                UserCount = department.Users?.Count ?? 0
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department {Id}", id);
            return StatusCode(500, new { message = "Error updating department", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null || department.IsDeleted)
                return NotFound(new { message = "Department not found" });

            department.IsDeleted = true;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            await _departmentRepository.SaveAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting department {Id}", id);
            return StatusCode(500, new { message = "Error deleting department", error = ex.Message });
        }
    }
}
