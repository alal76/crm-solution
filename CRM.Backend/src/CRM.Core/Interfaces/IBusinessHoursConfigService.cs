// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities.KnowledgeBase;

namespace CRM.Core.Interfaces;

/// <summary>
/// DTO for creating or updating a business hours configuration.
/// </summary>
public class BusinessHoursConfigRequest
{
    public string Name { get; set; } = "Default Business Hours";
    public string Timezone { get; set; } = "UTC";
    public bool Is24x7 { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
    /// <summary>
    /// JSON schedule — array of { day, isWorkingDay, startTime, endTime }.
    /// </summary>
    public string ScheduleJson { get; set; } = "[]";
    public string? HolidaysJson { get; set; }
}

/// <summary>
/// DTO returned for a business hours configuration.
/// </summary>
public class BusinessHoursConfigDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public bool Is24x7 { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public string ScheduleJson { get; set; } = "[]";
    public string? HolidaysJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Service for managing business hours configurations.
/// TODO-SYS005-001
/// </summary>
public interface IBusinessHoursConfigService
{
    /// <summary>Returns all non-deleted business hours configurations.</summary>
    Task<IEnumerable<BusinessHoursConfigDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns a single configuration by ID.</summary>
    Task<BusinessHoursConfigDto?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Creates a new configuration.</summary>
    Task<BusinessHoursConfigDto> CreateAsync(BusinessHoursConfigRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing configuration.</summary>
    Task<BusinessHoursConfigDto?> UpdateAsync(int id, BusinessHoursConfigRequest request, CancellationToken ct = default);

    /// <summary>Soft-deletes a configuration.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>Sets the given configuration as the default (unsets any previous default).</summary>
    Task<BusinessHoursConfigDto?> SetDefaultAsync(int id, CancellationToken ct = default);
}
