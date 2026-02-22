// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

// NOTE: Core commission plan DTOs (CommissionPlanDto, CreateCommissionPlanDto, UpdateCommissionPlanDto)
// are defined in CommissionManagementDtos.cs. This file contains supplementary plan-specific DTOs.

/// <summary>
/// DTO for assigning a commission plan to a user.
/// </summary>
public class CommissionPlanAssignDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    public DateTime? EffectiveDate { get; set; }
}

/// <summary>
/// DTO for commission plan summary/statistics.
/// </summary>
public class CommissionPlanSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public bool IsActive { get; set; }
    public int AssignedUserCount { get; set; }
    public int TierCount { get; set; }
    public int CommissionCount { get; set; }
    public decimal TotalCommissionsPaid { get; set; }
    public decimal AverageCommissionAmount { get; set; }
    public DateTime? EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for paginated commission plan list.
/// </summary>
public class CommissionPlanListDto
{
    public List<CommissionPlanDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// DTO for commission plan user assignment details.
/// </summary>
public class CommissionPlanAssignmentDto
{
    public int Id { get; set; }
    public int CommissionPlanId { get; set; }
    public string? PlanName { get; set; }
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for duplicating a commission plan.
/// </summary>
public class DuplicateCommissionPlanDto
{
    [Required(ErrorMessage = "New plan name is required")]
    [StringLength(255)]
    public string NewName { get; set; } = string.Empty;
}
