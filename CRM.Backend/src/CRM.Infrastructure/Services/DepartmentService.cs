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
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing departments
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ICrmDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Department>> GetAllAsync(
        bool? isActive = null,
        int? parentDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Getting departments with filters: IsActive={IsActive}, ParentDepartmentId={ParentDepartmentId}",
            isActive, parentDepartmentId);

        var query = _context.Departments.AsNoTracking().Where(d => !d.IsDeleted);

        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        if (parentDepartmentId.HasValue)
        {
            query = query.Where(d => d.ParentDepartmentId == parentDepartmentId.Value);
        }

        var departments = await query
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} departments", departments.Count);
        return departments;
    }

    /// <inheritdoc />
    public async Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting department by ID: {DepartmentId}", id);

        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Department not found: {DepartmentId}", id);
        }

        return department;
    }

    /// <inheritdoc />
    public async Task<Department?> GetByCodeAsync(string departmentCode, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting department by code: {DepartmentCode}", departmentCode);

        var department = await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DepartmentCode == departmentCode && !d.IsDeleted, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Department not found for code: {DepartmentCode}", departmentCode);
        }

        return department;
    }

    /// <inheritdoc />
    public async Task<Department> CreateAsync(Department department, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(department);

        _logger.LogDebug("Creating department: {Name}", department.Name);

        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;
        department.IsDeleted = false;

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created department with ID: {DepartmentId}, Name: {Name}", department.Id, department.Name);
        return department;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, Department department, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(department);

        _logger.LogDebug("Updating department: {DepartmentId}", id);

        var existing = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Department not found for update: {DepartmentId}", id);
            return false;
        }

        existing.Name = department.Name;
        existing.Description = department.Description;
        existing.DepartmentCode = department.DepartmentCode;
        existing.IsActive = department.IsActive;
        existing.ParentDepartmentId = department.ParentDepartmentId;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated department: {DepartmentId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting department: {DepartmentId}", id);

        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted, cancellationToken);

        if (department == null)
        {
            _logger.LogWarning("Department not found for deletion: {DepartmentId}", id);
            return false;
        }

        // Soft delete
        department.IsDeleted = true;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted department: {DepartmentId}", id);
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Department>> GetSubDepartmentsAsync(int parentDepartmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting sub-departments for parent: {ParentDepartmentId}", parentDepartmentId);

        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.ParentDepartmentId == parentDepartmentId && !d.IsDeleted)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} sub-departments for parent {ParentDepartmentId}", departments.Count, parentDepartmentId);
        return departments;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Department>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting full department hierarchy");

        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.ParentDepartmentId)
            .ThenBy(d => d.Name)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} departments for hierarchy", departments.Count);
        return departments;
    }
}
