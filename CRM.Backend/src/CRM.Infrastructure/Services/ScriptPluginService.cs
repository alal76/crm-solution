// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities.AI;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Manages <see cref="ScriptPlugin"/> entities and executes sandboxed scripts
/// on behalf of AI agents and workflow nodes.
/// </summary>
public class ScriptPluginService : IScriptPluginService
{
    private readonly ICrmDbContext _context;
    private readonly ScriptEngineFactory _scriptEngineFactory;
    private readonly ILogger<ScriptPluginService> _logger;

    public ScriptPluginService(
        ICrmDbContext context,
        ScriptEngineFactory scriptEngineFactory,
        ILogger<ScriptPluginService> logger)
    {
        _context = context;
        _scriptEngineFactory = scriptEngineFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ScriptPluginDto>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _context.ScriptPlugins.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        var entities = await query
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return entities.Select(MapToDto).ToList();
    }

    /// <inheritdoc/>
    public async Task<ScriptPluginDto?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        var entity = await _context.ScriptPlugins
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return entity is null ? null : MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<ScriptPluginDto> CreateAsync(
        CreateScriptPluginDto dto,
        int createdByUserId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Script plugin name is required.", nameof(dto));
        }

        var entity = new ScriptPlugin
        {
            Name = dto.Name,
            Description = dto.Description,
            Language = (ScriptLanguage)dto.Language,
            Code = dto.Code,
            ParameterSchema = dto.ParameterSchema,
            ReturnValueDescription = dto.ReturnValueDescription,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdByUserId,
        };

        _context.ScriptPlugins.Add(entity);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Created ScriptPlugin {Id} ({Name}) for user {UserId}", entity.Id, entity.Name, createdByUserId);

        return MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task<ScriptPluginDto> UpdateAsync(
        int id,
        UpdateScriptPluginDto dto,
        CancellationToken ct = default)
    {
        var entity = await _context.ScriptPlugins
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null)
        {
            throw new InvalidOperationException("ScriptPlugin not found");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Code = dto.Code;
        entity.ParameterSchema = dto.ParameterSchema;
        entity.ReturnValueDescription = dto.ReturnValueDescription;
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Updated ScriptPlugin {Id} ({Name})", entity.Id, entity.Name);

        return MapToDto(entity);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.ScriptPlugins
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null)
        {
            throw new InvalidOperationException("ScriptPlugin not found");
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted ScriptPlugin {Id}", id);
    }

    /// <inheritdoc/>
    public async Task<ScriptPluginTestResult> TestExecuteAsync(
        int id,
        ScriptPluginTestRequest request,
        CancellationToken ct = default)
    {
        var entity = await _context.ScriptPlugins
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (entity is null)
        {
            throw new InvalidOperationException("ScriptPlugin not found");
        }

        var engine = _scriptEngineFactory.GetEngine((ScriptLanguage)entity.Language);

        _logger.LogDebug("Test-executing ScriptPlugin {Id} ({Name}) via {Language} engine", entity.Id, entity.Name, entity.Language);

        var executionResult = await engine.ExecuteAsync(
            entity.Code,
            request.Variables,
            request.Context,
            request.Timeout,
            ct);

        entity.LastTestedAt = DateTime.UtcNow;
        entity.LastTestResult = JsonSerializer.Serialize(new
        {
            executionResult.Success,
            executionResult.ReturnValue,
            executionResult.Logs,
            executionResult.ErrorMessage,
            ExecutionTimeMs = executionResult.ExecutionTime.TotalMilliseconds,
            TestedAt = entity.LastTestedAt,
        });

        await _context.SaveChangesAsync(ct);

        return new ScriptPluginTestResult(
            executionResult.Success,
            executionResult.ReturnValue,
            executionResult.Logs,
            executionResult.ErrorMessage,
            executionResult.ExecutionTime);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ScriptPluginDto MapToDto(ScriptPlugin entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            (int)entity.Language,
            entity.Code,
            entity.ParameterSchema,
            entity.ReturnValueDescription,
            entity.IsActive,
            entity.Version,
            entity.CreatedAt,
            entity.UpdatedAt);
}
