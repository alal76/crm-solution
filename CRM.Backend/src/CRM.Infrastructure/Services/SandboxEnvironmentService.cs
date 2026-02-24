// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for managing sandbox environments for safe testing and previewing of configuration changes.
/// </summary>
public interface ISandboxEnvironmentService
{
    /// <summary>
    /// Creates a new sandbox environment cloned from production.
    /// </summary>
    Task<SandboxEnvironment> CreateSandboxAsync(CreateSandboxRequest request, CancellationToken ct = default);

    /// <summary>
    /// Lists all sandbox environments.
    /// </summary>
    Task<List<SandboxEnvironment>> ListSandboxesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a sandbox environment by ID.
    /// </summary>
    Task<SandboxEnvironment?> GetSandboxAsync(string sandboxId, CancellationToken ct = default);

    /// <summary>
    /// Activates a sandbox for use.
    /// </summary>
    Task<SandboxEnvironment> ActivateAsync(string sandboxId, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a sandbox.
    /// </summary>
    Task<SandboxEnvironment> DeactivateAsync(string sandboxId, CancellationToken ct = default);

    /// <summary>
    /// Refreshes sandbox data from production.
    /// </summary>
    Task<SandboxEnvironment> RefreshAsync(string sandboxId, CancellationToken ct = default);

    /// <summary>
    /// Promotes sandbox changes to production.
    /// </summary>
    Task<PromotionResult> PromoteToProductionAsync(string sandboxId, PromotionOptions options, CancellationToken ct = default);

    /// <summary>
    /// Deletes a sandbox environment.
    /// </summary>
    Task<bool> DeleteSandboxAsync(string sandboxId, CancellationToken ct = default);
}

/// <summary>
/// Represents a sandbox environment.
/// </summary>
public class SandboxEnvironment
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SandboxStatus Status { get; set; } = SandboxStatus.Inactive;
    public string SourceEnvironment { get; set; } = "production";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ActivatedAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string? ConnectionString { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Request to create a new sandbox.
/// </summary>
public class CreateSandboxRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreatedByUserId { get; set; }
    public bool CopyData { get; set; } = true;
    public bool CopyConfigurations { get; set; } = true;
    public bool CopyCustomFields { get; set; } = true;
    public int? ExpirationDays { get; set; } = 30;
}

/// <summary>
/// Options for promoting sandbox changes.
/// </summary>
public class PromotionOptions
{
    public bool PromoteSchemaChanges { get; set; } = true;
    public bool PromoteConfigurations { get; set; } = true;
    public bool PromoteCustomFields { get; set; } = true;
    public bool PromoteWorkflows { get; set; } = true;
    public bool DryRun { get; set; } = false;
}

/// <summary>
/// Result of a promotion operation.
/// </summary>
public class PromotionResult
{
    public bool Success { get; set; }
    public List<string> PromotedItems { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool WasDryRun { get; set; }
}

/// <summary>
/// Sandbox environment status.
/// </summary>
public enum SandboxStatus
{
    Inactive = 0,
    Active = 1,
    Refreshing = 2,
    Promoting = 3,
    Expired = 4,
    Error = 5
}

#endregion

/// <summary>
/// Stub implementation of sandbox environment management.
/// In production, this would manage separate database instances or schemas.
/// </summary>
public class SandboxEnvironmentService : ISandboxEnvironmentService
{
    private readonly ILogger<SandboxEnvironmentService> _logger;
    private readonly List<SandboxEnvironment> _sandboxes = new();

    public SandboxEnvironmentService(ILogger<SandboxEnvironmentService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<SandboxEnvironment> CreateSandboxAsync(CreateSandboxRequest request, CancellationToken ct = default)
    {
        var sandbox = new SandboxEnvironment
        {
            Id = Guid.NewGuid().ToString("N")[..12],
            Name = request.Name,
            Description = request.Description,
            Status = SandboxStatus.Inactive,
            CreatedByUserId = request.CreatedByUserId,
            ExpiresAt = request.ExpirationDays.HasValue
                ? DateTime.UtcNow.AddDays(request.ExpirationDays.Value)
                : null,
            Metadata = new Dictionary<string, string>
            {
                ["CopyData"] = request.CopyData.ToString(),
                ["CopyConfigurations"] = request.CopyConfigurations.ToString(),
                ["CopyCustomFields"] = request.CopyCustomFields.ToString()
            }
        };

        _sandboxes.Add(sandbox);

        _logger.LogInformation("Created sandbox environment: {SandboxId} ({Name})", sandbox.Id, sandbox.Name);

        // TODO: In production, clone database schema and optionally data
        // TODO: Set up separate connection string for the sandbox database
        return Task.FromResult(sandbox);
    }

    /// <inheritdoc />
    public Task<List<SandboxEnvironment>> ListSandboxesAsync(CancellationToken ct = default)
    {
        return Task.FromResult(_sandboxes.ToList());
    }

    /// <inheritdoc />
    public Task<SandboxEnvironment?> GetSandboxAsync(string sandboxId, CancellationToken ct = default)
    {
        return Task.FromResult(_sandboxes.FirstOrDefault(s => s.Id == sandboxId));
    }

    /// <inheritdoc />
    public Task<SandboxEnvironment> ActivateAsync(string sandboxId, CancellationToken ct = default)
    {
        var sandbox = _sandboxes.FirstOrDefault(s => s.Id == sandboxId)
            ?? throw new InvalidOperationException($"Sandbox {sandboxId} not found.");

        sandbox.Status = SandboxStatus.Active;
        sandbox.ActivatedAt = DateTime.UtcNow;
        _logger.LogInformation("Activated sandbox: {SandboxId}", sandboxId);

        // TODO: Start sandbox database container or switch connection
        return Task.FromResult(sandbox);
    }

    /// <inheritdoc />
    public Task<SandboxEnvironment> DeactivateAsync(string sandboxId, CancellationToken ct = default)
    {
        var sandbox = _sandboxes.FirstOrDefault(s => s.Id == sandboxId)
            ?? throw new InvalidOperationException($"Sandbox {sandboxId} not found.");

        sandbox.Status = SandboxStatus.Inactive;
        _logger.LogInformation("Deactivated sandbox: {SandboxId}", sandboxId);

        // TODO: Stop sandbox database container
        return Task.FromResult(sandbox);
    }

    /// <inheritdoc />
    public Task<SandboxEnvironment> RefreshAsync(string sandboxId, CancellationToken ct = default)
    {
        var sandbox = _sandboxes.FirstOrDefault(s => s.Id == sandboxId)
            ?? throw new InvalidOperationException($"Sandbox {sandboxId} not found.");

        sandbox.Status = SandboxStatus.Refreshing;
        sandbox.LastRefreshedAt = DateTime.UtcNow;

        // TODO: Re-clone production data into sandbox
        sandbox.Status = SandboxStatus.Active;
        _logger.LogInformation("Refreshed sandbox: {SandboxId}", sandboxId);

        return Task.FromResult(sandbox);
    }

    /// <inheritdoc />
    public Task<PromotionResult> PromoteToProductionAsync(
        string sandboxId, PromotionOptions options, CancellationToken ct = default)
    {
        var sandbox = _sandboxes.FirstOrDefault(s => s.Id == sandboxId)
            ?? throw new InvalidOperationException($"Sandbox {sandboxId} not found.");

        var result = new PromotionResult
        {
            WasDryRun = options.DryRun,
            Success = true
        };

        if (options.PromoteConfigurations)
            result.PromotedItems.Add("System configurations");
        if (options.PromoteCustomFields)
            result.PromotedItems.Add("Custom field definitions");
        if (options.PromoteWorkflows)
            result.PromotedItems.Add("Workflow definitions");
        if (options.PromoteSchemaChanges)
            result.PromotedItems.Add("Schema changes");

        if (options.DryRun)
        {
            result.Warnings.Add("This was a dry run. No actual changes were applied.");
        }
        else
        {
            // TODO: Apply sandbox changes to production database
            _logger.LogInformation("Promoted sandbox {SandboxId} to production", sandboxId);
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<bool> DeleteSandboxAsync(string sandboxId, CancellationToken ct = default)
    {
        var removed = _sandboxes.RemoveAll(s => s.Id == sandboxId);
        if (removed > 0)
        {
            _logger.LogInformation("Deleted sandbox: {SandboxId}", sandboxId);
            // TODO: Drop sandbox database and clean up resources
        }
        return Task.FromResult(removed > 0);
    }
}
