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
