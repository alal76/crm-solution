// This file is part of the CRM Solution.
// Copyright (c) 2025 CRM Solution Contributors
// Licensed under the AGPL-3.0 license.
// See LICENSE file in the project root for full license information.
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

public enum CatalogVariableType
{
    Text = 1,
    TextArea = 2,
    Number = 3,
    Decimal = 4,
    Date = 5,
    DateTime = 6,
    Dropdown = 7,
    MultiSelect = 8,
    Boolean = 9,
    Email = 10,
    Phone = 11,
    Url = 12,
    FileUpload = 13
}

public enum CatalogRequestState
{
    Requested = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    InProgress = 5,
    Completed = 6,
    Cancelled = 7
}

public class CatalogCategory
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? IconName { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<CatalogItem>? CatalogItems { get; set; }
}

public class CatalogItem
{
    [Key]
    public int CatalogItemId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public CatalogCategory? Category { get; set; }

    // Display
    [StringLength(50)]
    public string? IconName { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsFeatured { get; set; } = false;

    // Availability
    public bool IsActive { get; set; } = true;

    public bool AvailableToAll { get; set; } = true;

    public string? RestrictedToGroups { get; set; }

    // Workflow
    public int? WorkflowDefinitionId { get; set; }

    // TODO: Implement workflow engine - WorkflowDefinition entity
    // [ForeignKey(nameof(WorkflowDefinitionId))]
    // public WorkflowDefinition? WorkflowDefinition { get; set; }

    public int? ApprovalWorkflowId { get; set; }

    public int? FulfillmentTaskTemplateId { get; set; }

    // SLA
    public int? ExpectedDeliveryDays { get; set; }

    public int Priority { get; set; } = 2;

    // Pricing
    [Column(TypeName = "decimal(18,2)")]
    public decimal? Price { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? RecurringCostMonthly { get; set; }

    public bool RequiresBudgetApproval { get; set; } = false;

    // Metrics
    public int RequestCount { get; set; } = 0;

    [Column(TypeName = "decimal(3,2)")]
    public decimal? AverageRating { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<CatalogVariable>? Variables { get; set; }

    public ICollection<CatalogRequest>? Requests { get; set; }
}

public class CatalogVariable
{
    [Key]
    public int VariableId { get; set; }

    [Required]
    public int CatalogItemId { get; set; }

    [ForeignKey(nameof(CatalogItemId))]
    public CatalogItem? CatalogItem { get; set; }

    [Required]
    [StringLength(100)]
    public string VariableName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string VariableLabel { get; set; } = string.Empty;

    [Required]
    public CatalogVariableType VariableType { get; set; }

    // Validation
    public bool IsRequired { get; set; } = false;

    [StringLength(500)]
    public string? ValidationRegex { get; set; }

    [StringLength(500)]
    public string? ValidationMessage { get; set; }

    public int? MinLength { get; set; }

    public int? MaxLength { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxValue { get; set; }

    // Options (for dropdown)
    public string? Options { get; set; }

    [StringLength(500)]
    public string? DefaultValue { get; set; }

    // Conditional display
    public string? ShowWhen { get; set; }

    public int DisplayOrder { get; set; } = 0;

    [StringLength(500)]
    public string? HelpText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class CatalogRequest
{
    [Key]
    public int RequestId { get; set; }

    [Required]
    public int CatalogItemId { get; set; }

    [ForeignKey(nameof(CatalogItemId))]
    public CatalogItem? CatalogItem { get; set; }

    [Required]
    public int RequestedForId { get; set; }

    [ForeignKey(nameof(RequestedForId))]
    public User? RequestedFor { get; set; }

    [Required]
    public int RequestedById { get; set; }

    [ForeignKey(nameof(RequestedById))]
    public User? RequestedBy { get; set; }

    public string? VariableValues { get; set; }

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Requested;

    [Required]
    public CatalogRequestState State { get; set; } = CatalogRequestState.Requested;

    public int? ServiceRequestId { get; set; }

    [ForeignKey(nameof(ServiceRequestId))]
    public ServiceRequest? ServiceRequest { get; set; }

    public int? WorkflowInstanceId { get; set; }

    // TODO: Implement workflow engine - WorkflowInstance entity
    // [ForeignKey(nameof(WorkflowInstanceId))]
    // public WorkflowInstance? WorkflowInstance { get; set; }

    // Fulfillment
    public int? AssignedToId { get; set; }

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? CompletionNotes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<CatalogRequestApproval>? Approvals { get; set; }

    public ICollection<CatalogRequestComment>? Comments { get; set; }
}

public class CatalogRequestApproval
{
    [Key]
    public int ApprovalId { get; set; }

    [Required]
    public int CatalogRequestId { get; set; }

    [ForeignKey(nameof(CatalogRequestId))]
    public CatalogRequest? CatalogRequest { get; set; }

    [Required]
    public int ApproverId { get; set; }

    [ForeignKey(nameof(ApproverId))]
    public User? Approver { get; set; }

    [Required]
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Requested;

    public DateTime? ApprovalDate { get; set; }

    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class CatalogRequestComment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int CatalogRequestId { get; set; }

    [ForeignKey(nameof(CatalogRequestId))]
    public CatalogRequest? CatalogRequest { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = false;

    [Required]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
