// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for managing departments
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    /// Get all departments with optional filtering
    /// </summary>
    Task<IEnumerable<Department>> GetAllAsync(
        bool? isActive = null,
        int? parentDepartmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a department by ID
    /// </summary>
    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a department by code
    /// </summary>
    Task<Department?> GetByCodeAsync(string departmentCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new department
    /// </summary>
    Task<Department> CreateAsync(Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing department
    /// </summary>
    Task<bool> UpdateAsync(int id, Department department, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a department (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get child departments for a parent department
    /// </summary>
    Task<IEnumerable<Department>> GetSubDepartmentsAsync(int parentDepartmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the full department hierarchy as a flat list with depth info
    /// </summary>
    Task<IEnumerable<Department>> GetHierarchyAsync(CancellationToken cancellationToken = default);
}
