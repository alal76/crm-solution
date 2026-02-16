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

using CRM.Core.Entities.ITSM;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.DTOs.ITSM;

// ============================================================================
// Problem Workflow DTOs
// ============================================================================

/// <summary>
/// DTO for resolving a problem with root cause analysis.
/// </summary>
public class ResolveProblemDto
{
    [Required]
    [StringLength(1000)]
    public string RootCause { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Workaround { get; set; }

    [Required]
    [StringLength(2000)]
    public string Solution { get; set; } = string.Empty;

    public string? ResolutionCode { get; set; }
}

/// <summary>
/// DTO for closing a problem.
/// </summary>
public class CloseProblemDto
{
    public string? ClosureNotes { get; set; }
    public bool CreateKnowledgeArticle { get; set; } = true;
}

/// <summary>
/// DTO for reopening a closed/resolved problem.
/// </summary>
public class ReopenProblemDto
{
    public string? ReopenReason { get; set; }
}

/// <summary>
/// DTO for relating a problem to incidents.
/// </summary>
public class RelateProblemToIncidentsDto
{
    [Required]
    public List<int> IncidentIds { get; set; } = new();
}

/// <summary>
/// DTO for problem root cause analysis details.
/// </summary>
public class ProblemRootCauseAnalysisDto
{
    public int ProblemId { get; set; }
    public string? Symptoms { get; set; }
    public string? RootCause { get; set; }
    public string? Workaround { get; set; }
    public string? FiveWhysAnalysis { get; set; }
    public string? FishboneAnalysis { get; set; }
    public string? Timeline { get; set; }
    public DateTime? AnalysisCompletedAt { get; set; }
}

/// <summary>
/// DTO for problem metrics and statistics.
/// </summary>
public class ProblemMetricsDto
{
    public int TotalProblems { get; set; }
    public int NewProblems { get; set; }
    public int InvestigatingProblems { get; set; }
    public int KnownErrors { get; set; }
    public int ResolvedProblems { get; set; }
    public double AverageResolutionDays { get; set; }
    public double AverageIncidentsPerProblem { get; set; }
    public int ProblemsWithArticles { get; set; }
    public DateTime? ReportGeneratedAt { get; set; }
}

/// <summary>
/// DTO for problem task tracking.
/// </summary>
public class ProblemTaskDto
{
    public int TaskId { get; set; }
    public int ProblemId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a problem task.
/// </summary>
public class CreateProblemTaskDto
{
    [Required]
    [StringLength(200)]
    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }
}

/// <summary>
/// DTO for problem comment/note.
/// </summary>
public class ProblemCommentDto
{
    public int CommentId { get; set; }
    public int ProblemId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public int CreatedById { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a problem comment.
/// </summary>
public class CreateProblemCommentDto
{
    [Required]
    [StringLength(5000)]
    public string Comment { get; set; } = string.Empty;

    public bool IsInternal { get; set; } = true;
}

/// <summary>
/// DTO for problem inventory analysis (related incidents).
/// </summary>
public class ProblemIncidentAnalysisDto
{
    public int ProblemId { get; set; }
    public string ProblemNumber { get; set; } = string.Empty;
    public string ProblemTitle { get; set; } = string.Empty;
    public List<int> RelatedIncidentIds { get; set; } = new();
    public int TotalRelatedIncidents { get; set; }
    public List<string> IncidentNumbers { get; set; } = new();
}

/// <summary>
/// DTO for assigning a problem to an investigator/manager.
/// </summary>
public class AssignProblemDto
{
    public int? ProblemInvestigatorId { get; set; }
    public int? ProblemManagerId { get; set; }
    public int? AssignmentGroupId { get; set; }
}
