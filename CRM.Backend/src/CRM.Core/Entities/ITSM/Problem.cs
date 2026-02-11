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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

public enum ProblemState
{
    New = 1,
    Investigating = 2,
    RootCauseAnalysis = 3,
    KnownError = 4,
    Resolved = 5,
    Closed = 6,
    Cancelled = 7
}

public enum ProblemPriority
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

public class Problem
{
    [Key]
    public int ProblemId { get; set; }

    [Required]
    [StringLength(20)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string ShortDescription { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Classification
    public int? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public ServiceRequestCategory? Category { get; set; }

    public int? SubcategoryId { get; set; }

    [ForeignKey(nameof(SubcategoryId))]
    public ServiceRequestSubcategory? Subcategory { get; set; }

    public int? ConfigurationItemId { get; set; }

    // ConfigurationItem FK will be added in Phase 2
    [Required]
    public ProblemPriority Priority { get; set; }

    // Analysis
    public string? Symptoms { get; set; }

    public string? RootCause { get; set; }

    public string? Workaround { get; set; }

    public bool KnownError { get; set; } = false;

    public DateTime? KnownErrorDate { get; set; }

    // Assignment
    [Required]
    public ProblemState State { get; set; } = ProblemState.New;

    public int? ProblemInvestigatorId { get; set; }

    [ForeignKey(nameof(ProblemInvestigatorId))]
    public User? ProblemInvestigator { get; set; }

    public int? ProblemManagerId { get; set; }

    [ForeignKey(nameof(ProblemManagerId))]
    public User? ProblemManager { get; set; }

    public int? AssignmentGroupId { get; set; }

    [ForeignKey(nameof(AssignmentGroupId))]
    public UserGroup? AssignmentGroup { get; set; }

    // Resolution
    public string? Solution { get; set; }

    public string? ResolutionCode { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public bool FixVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    public int? KnowledgeArticleId { get; set; }

    // KnowledgeArticle FK will be added in Phase 3

    // RCA Details
    public string? FiveWhysAnalysis { get; set; }

    public string? FishboneAnalysis { get; set; }

    public string? Timeline { get; set; }

    // Closure
    public DateTime? ClosedAt { get; set; }

    public int? ClosedById { get; set; }

    [ForeignKey(nameof(ClosedById))]
    public User? ClosedBy { get; set; }

    public string? ClosureNotes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<ProblemIncident>? ProblemIncidents { get; set; }

    public ICollection<ProblemTask>? Tasks { get; set; }

    public ICollection<ProblemComment>? Comments { get; set; }

    public ICollection<ProblemAttachment>? Attachments { get; set; }
}

public class ProblemIncident
{
    [Key]
    public int ProblemIncidentId { get; set; }

    [Required]
    public int ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public Problem? Problem { get; set; }

    [Required]
    public int IncidentId { get; set; }

    [ForeignKey(nameof(IncidentId))]
    public Incident? Incident { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }
}

public class ProblemTask
{
    [Key]
    public int TaskId { get; set; }

    [Required]
    public int ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public Problem? Problem { get; set; }

    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedToId { get; set; }

    [ForeignKey(nameof(AssignedToId))]
    public User? AssignedTo { get; set; }

    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ProblemComment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public Problem? Problem { get; set; }

    [Required]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true;

    [Required]
    public int CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}

public class ProblemAttachment
{
    [Key]
    public int AttachmentId { get; set; }

    [Required]
    public int ProblemId { get; set; }

    [ForeignKey(nameof(ProblemId))]
    public Problem? Problem { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [StringLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [Required]
    public int UploadedById { get; set; }

    [ForeignKey(nameof(UploadedById))]
    public User? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
