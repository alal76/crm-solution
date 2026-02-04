// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

namespace CRM.Core.Interfaces.ITSM;

/// <summary>
/// Service for CI/CD pipeline integration with ITSM change management.
/// </summary>
public interface ICICDIntegrationService
{
    /// <summary>
    /// Create a change request from a deployment pipeline.
    /// </summary>
    Task<DeploymentChangeResult> CreateDeploymentChangeAsync(DeploymentChangeRequestDto request);

    /// <summary>
    /// Update deployment status.
    /// </summary>
    Task<DeploymentChangeResult> UpdateDeploymentStatusAsync(int changeId, DeploymentStatusUpdateDto update);

    /// <summary>
    /// Get deployment history.
    /// </summary>
    Task<List<DeploymentHistoryDto>> GetDeploymentHistoryAsync(string? environment, DateTime? startDate, DateTime? endDate);

    /// <summary>
    /// Validate deployment prerequisites.
    /// </summary>
    Task<DeploymentValidationResult> ValidateDeploymentAsync(DeploymentValidationRequestDto request);

    /// <summary>
    /// Register a CI/CD pipeline.
    /// </summary>
    Task<PipelineRegistrationDto> RegisterPipelineAsync(RegisterPipelineDto request);

    /// <summary>
    /// Get registered pipelines.
    /// </summary>
    Task<List<PipelineRegistrationDto>> GetPipelinesAsync();

    /// <summary>
    /// Get pipeline by ID.
    /// </summary>
    Task<PipelineRegistrationDto?> GetPipelineAsync(int id);

    /// <summary>
    /// Delete a pipeline.
    /// </summary>
    Task<bool> DeletePipelineAsync(int id);
}

// ====== DTOs ======

public class DeploymentChangeRequestDto
{
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public string CommitHash { get; set; } = string.Empty;
    public string CommitMessage { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string? Version { get; set; }
    public List<string> Services { get; set; } = new();
    public List<string> AffectedComponents { get; set; } = new();
    public DeploymentType DeploymentType { get; set; } = DeploymentType.Standard;
    public bool AutoApprove { get; set; } = false;
    public string? RollbackPlan { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum DeploymentType
{
    Standard,
    Emergency,
    Hotfix,
    Rollback
}

public class DeploymentChangeResult
{
    public bool Success { get; set; }
    public int? ChangeId { get; set; }
    public string? ChangeNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? Message { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

public class DeploymentStatusUpdateDto
{
    public string Status { get; set; } = string.Empty; // started, in_progress, completed, failed, rolled_back
    public string? StatusMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool? RolledBack { get; set; }
    public string? RollbackReason { get; set; }
    public Dictionary<string, object>? Metrics { get; set; }
}

public class DeploymentHistoryDto
{
    public int Id { get; set; }
    public string ChangeNumber { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string BuildNumber { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    public bool WasRolledBack { get; set; }
}

public class DeploymentValidationRequestDto
{
    public string Environment { get; set; } = string.Empty;
    public List<string> Services { get; set; } = new();
    public DeploymentType DeploymentType { get; set; }
    public DateTime? ScheduledTime { get; set; }
}

public class DeploymentValidationResult
{
    public bool IsValid { get; set; }
    public bool HasOpenIncidents { get; set; }
    public int OpenIncidentCount { get; set; }
    public bool IsInMaintenanceWindow { get; set; }
    public bool HasPendingChanges { get; set; }
    public int PendingChangeCount { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Blockers { get; set; } = new();
    public string Recommendation { get; set; } = string.Empty;
}

public class RegisterPipelineDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CICDPlatform Platform { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = "main";
    public List<string> Environments { get; set; } = new();
    public bool RequiresApproval { get; set; } = true;
    public int? DefaultAssignmentGroupId { get; set; }
}

public enum CICDPlatform
{
    AzureDevOps,
    GitHub,
    GitLab,
    Jenkins,
    CircleCI,
    TravisCI,
    Bitbucket,
    Other
}

public class PipelineRegistrationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CICDPlatform Platform { get; set; }
    public string RepositoryUrl { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = string.Empty;
    public List<string> Environments { get; set; } = new();
    public bool RequiresApproval { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastDeploymentAt { get; set; }
    public int TotalDeployments { get; set; }
    public double SuccessRate { get; set; }
}
