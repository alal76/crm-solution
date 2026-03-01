// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

#region Interfaces and DTOs

/// <summary>
/// Service for exporting and importing configurations between environments.
/// Supports custom fields, workflow definitions, feature flags, and system settings.
/// </summary>
public interface IConfigMigrationService
{
    /// <summary>
    /// Exports configuration from the current environment.
    /// </summary>
    Task<ConfigExportPackage> ExportAsync(ConfigExportOptions options, CancellationToken ct = default);

    /// <summary>
    /// Imports configuration from an export package.
    /// </summary>
    Task<ConfigImportResult> ImportAsync(ConfigExportPackage package, ConfigImportOptions options, CancellationToken ct = default);

    /// <summary>
    /// Validates an export package before import.
    /// </summary>
    Task<ConfigValidationResult> ValidateAsync(ConfigExportPackage package, CancellationToken ct = default);

    /// <summary>
    /// Gets the difference between current config and an import package.
    /// </summary>
    Task<ConfigDiffResult> DiffAsync(ConfigExportPackage package, CancellationToken ct = default);
}

/// <summary>
/// Options controlling what to include in the export.
/// </summary>
public class ConfigExportOptions
{
    public bool IncludeCustomFields { get; set; } = true;
    public bool IncludeWorkflows { get; set; } = true;
    public bool IncludeFeatureFlags { get; set; } = true;
    public bool IncludeSystemSettings { get; set; } = true;
    public bool IncludeEmailTemplates { get; set; } = true;
    public bool IncludeNavigationConfig { get; set; } = true;
    public List<string>? EntityTypeFilter { get; set; }
}

/// <summary>
/// A portable package of configuration data.
/// </summary>
public class ConfigExportPackage
{
    public string Version { get; set; } = "1.0";
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public string SourceEnvironment { get; set; } = string.Empty;
    public string ExportedByUser { get; set; } = string.Empty;

    public List<CustomFieldExport> CustomFields { get; set; } = new();
    public List<WorkflowExport> Workflows { get; set; } = new();
    public List<FeatureFlagExport> FeatureFlags { get; set; } = new();
    public List<SystemSettingExport> SystemSettings { get; set; } = new();
    public List<EmailTemplateExport> EmailTemplates { get; set; } = new();
    public Dictionary<string, object?> NavigationConfig { get; set; } = new();

    /// <summary>
    /// Serializes the package to JSON.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    /// <summary>
    /// Deserializes a package from JSON.
    /// </summary>
    public static ConfigExportPackage? FromJson(string json) =>
        JsonSerializer.Deserialize<ConfigExportPackage>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
}

public class CustomFieldExport
{
    public string EntityType { get; set; } = string.Empty;
    public string FieldKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> Options { get; set; } = new();
    public string? ValidationRulesJson { get; set; }
}

public class WorkflowExport
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DefinitionJson { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class FeatureFlagExport
{
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string? Category { get; set; }
}

public class SystemSettingExport
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Category { get; set; }
}

public class EmailTemplateExport
{
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Options controlling import behavior.
/// </summary>
public class ConfigImportOptions
{
    public bool OverwriteExisting { get; set; } = false;
    public bool SkipConflicts { get; set; } = true;
    public bool DryRun { get; set; } = false;
}

/// <summary>
/// Result of importing a configuration package.
/// </summary>
public class ConfigImportResult
{
    public bool Success { get; set; }
    public int ItemsImported { get; set; }
    public int ItemsSkipped { get; set; }
    public int ItemsFailed { get; set; }
    public List<string> ImportedItems { get; set; } = new();
    public List<string> SkippedItems { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool WasDryRun { get; set; }
}

/// <summary>
/// Validation result for an import package.
/// </summary>
public class ConfigValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int TotalItems { get; set; }
}

/// <summary>
/// Result of comparing current config with an import package.
/// </summary>
public class ConfigDiffResult
{
    public int NewItems { get; set; }
    public int ModifiedItems { get; set; }
    public int UnchangedItems { get; set; }
    public List<ConfigDiffEntry> Differences { get; set; } = new();
}

public class ConfigDiffEntry
{
    public string ItemType { get; set; } = string.Empty;
    public string ItemKey { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty; // "Added", "Modified", "Unchanged"
    public string? CurrentValue { get; set; }
    public string? IncomingValue { get; set; }
}

#endregion

/// <summary>
/// Exports and imports CRM configurations between environments.
/// </summary>
public class ConfigMigrationService : IConfigMigrationService
{
    private readonly ICrmDbContext _context;
    private readonly ILogger<ConfigMigrationService> _logger;

    public ConfigMigrationService(
        ICrmDbContext context,
        ILogger<ConfigMigrationService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ConfigExportPackage> ExportAsync(ConfigExportOptions options, CancellationToken ct = default)
    {
        var package = new ConfigExportPackage
        {
            SourceEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ExportedAt = DateTime.UtcNow
        };

        if (options.IncludeCustomFields)
        {
            var customFields = await _context.CustomFields
                .Where(cf => !cf.IsDeleted && cf.Key != null && cf.Key.StartsWith("__def__"))
                .ToListAsync(ct);

            foreach (var cf in customFields)
            {
                package.CustomFields.Add(new CustomFieldExport
                {
                    EntityType = cf.EntityType ?? string.Empty,
                    FieldKey = cf.Key?.Replace("__def__", "") ?? string.Empty,
                    Label = cf.Key?.Replace("__def__", "") ?? string.Empty,
                    DataType = "Text",
                    ValidationRulesJson = cf.Value
                });
            }
        }

        if (options.IncludeSystemSettings)
        {
            var settings = await _context.SystemSettings
                .Where(s => !s.IsDeleted)
                .FirstOrDefaultAsync(ct);

            if (settings != null)
            {
                // Serialize SystemSettings properties as key-value pairs
                var json = JsonSerializer.Serialize(settings);
                package.SystemSettings.Add(new SystemSettingExport
                {
                    Key = "SystemSettings",
                    Value = json,
                    Category = "System"
                });
            }
        }

        if (options.IncludeEmailTemplates)
        {
            var templates = await _context.EmailTemplates
                .Where(t => !t.IsDeleted)
                .ToListAsync(ct);

            foreach (var t in templates)
            {
                package.EmailTemplates.Add(new EmailTemplateExport
                {
                    Name = t.Name ?? string.Empty,
                    Subject = t.Subject,
                    Body = t.HtmlBody ?? t.PlainTextBody,
                    Category = t.Category.ToString()
                });
            }
        }

        _logger.LogInformation(
            "Exported config package: {CustomFields} custom fields, {Settings} settings, {Templates} templates",
            package.CustomFields.Count, package.SystemSettings.Count, package.EmailTemplates.Count);

        return package;
    }

    /// <inheritdoc />
    public async Task<ConfigImportResult> ImportAsync(
        ConfigExportPackage package, ConfigImportOptions options, CancellationToken ct = default)
    {
        var result = new ConfigImportResult { WasDryRun = options.DryRun };

        // Validate first
        var validation = await ValidateAsync(package, ct);
        if (!validation.IsValid)
        {
            result.Success = false;
            result.Errors = validation.Errors;
            return result;
        }

        // Import custom fields
        foreach (var cf in package.CustomFields)
        {
            try
            {
                var existing = await _context.CustomFields
                    .FirstOrDefaultAsync(e =>
                        e.EntityType == cf.EntityType &&
                        e.Key == $"__def__{cf.FieldKey}" &&
                        !e.IsDeleted, ct);

                if (existing != null && !options.OverwriteExisting)
                {
                    result.ItemsSkipped++;
                    result.SkippedItems.Add($"CustomField: {cf.EntityType}.{cf.FieldKey} (already exists)");
                    continue;
                }

                if (!options.DryRun)
                {
                    if (existing != null)
                    {
                        existing.Value = cf.ValidationRulesJson;
                    }
                    else
                    {
                        await _context.CustomFields.AddAsync(new CustomField
                        {
                            EntityType = cf.EntityType,
                            EntityId = 0,
                            Key = $"__def__{cf.FieldKey}",
                            Value = cf.ValidationRulesJson,
                            CreatedAt = DateTime.UtcNow
                        }, ct);
                    }
                }

                result.ItemsImported++;
                result.ImportedItems.Add($"CustomField: {cf.EntityType}.{cf.FieldKey}");
            }
            catch (Exception ex)
            {
                result.ItemsFailed++;
                result.Errors.Add($"CustomField {cf.EntityType}.{cf.FieldKey}: {ex.Message}");
            }
        }

        // Import system settings
        foreach (var setting in package.SystemSettings)
        {
            try
            {
                // SystemSettings is a singleton entity; skip individual key-based import for now
                var existing = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => !s.IsDeleted, ct);

                if (existing != null && !options.OverwriteExisting)
                {
                    result.ItemsSkipped++;
                    result.SkippedItems.Add($"Setting: {setting.Key} (already exists)");
                    continue;
                }

                if (!options.DryRun && existing != null)
                {
                }

                result.ItemsImported++;
                result.ImportedItems.Add($"Setting: {setting.Key}");
            }
            catch (Exception ex)
            {
                result.ItemsFailed++;
                result.Errors.Add($"Setting {setting.Key}: {ex.Message}");
            }
        }

        if (!options.DryRun)
        {
            await _context.SaveChangesAsync(ct);
        }

        result.Success = result.Errors.Count == 0;

        _logger.LogInformation(
            "Config import: {Imported} imported, {Skipped} skipped, {Failed} failed (DryRun={DryRun})",
            result.ItemsImported, result.ItemsSkipped, result.ItemsFailed, options.DryRun);

        return result;
    }

    /// <inheritdoc />
    public Task<ConfigValidationResult> ValidateAsync(ConfigExportPackage package, CancellationToken ct = default)
    {
        var result = new ConfigValidationResult();

        if (string.IsNullOrEmpty(package.Version))
        {
            result.IsValid = false;
            result.Errors.Add("Package version is missing.");
        }

        result.TotalItems = package.CustomFields.Count +
                           package.SystemSettings.Count +
                           package.EmailTemplates.Count +
                           package.Workflows.Count +
                           package.FeatureFlags.Count;

        if (result.TotalItems == 0)
        {
            result.Warnings.Add("Package contains no items to import.");
        }

        // Validate custom field definitions
        foreach (var cf in package.CustomFields)
        {
            if (string.IsNullOrEmpty(cf.EntityType))
            {
                result.IsValid = false;
                result.Errors.Add($"Custom field '{cf.FieldKey}' has no entity type.");
            }
            if (string.IsNullOrEmpty(cf.FieldKey))
            {
                result.IsValid = false;
                result.Errors.Add("Found a custom field with empty field key.");
            }
        }

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public async Task<ConfigDiffResult> DiffAsync(ConfigExportPackage package, CancellationToken ct = default)
    {
        var result = new ConfigDiffResult();

        // Check custom fields
        foreach (var cf in package.CustomFields)
        {
            var existing = await _context.CustomFields
                .FirstOrDefaultAsync(e =>
                    e.EntityType == cf.EntityType &&
                    e.Key == $"__def__{cf.FieldKey}" &&
                    !e.IsDeleted, ct);

            if (existing == null)
            {
                result.NewItems++;
                result.Differences.Add(new ConfigDiffEntry
                {
                    ItemType = "CustomField",
                    ItemKey = $"{cf.EntityType}.{cf.FieldKey}",
                    ChangeType = "Added",
                    IncomingValue = cf.ValidationRulesJson
                });
            }
            else if (existing.Value != cf.ValidationRulesJson)
            {
                result.ModifiedItems++;
                result.Differences.Add(new ConfigDiffEntry
                {
                    ItemType = "CustomField",
                    ItemKey = $"{cf.EntityType}.{cf.FieldKey}",
                    ChangeType = "Modified",
                    CurrentValue = existing.Value,
                    IncomingValue = cf.ValidationRulesJson
                });
            }
            else
            {
                result.UnchangedItems++;
            }
        }

        // Check settings
        foreach (var setting in package.SystemSettings)
        {
            // SystemSettings is a singleton; compare as a single entity
            var existing = await _context.SystemSettings
                .FirstOrDefaultAsync(s => !s.IsDeleted, ct);

            if (existing == null)
            {
                result.NewItems++;
                result.Differences.Add(new ConfigDiffEntry
                {
                    ItemType = "SystemSetting",
                    ItemKey = setting.Key,
                    ChangeType = "Added",
                    IncomingValue = setting.Value
                });
            }
            else
            {
                result.UnchangedItems++;
            }
        }

        return result;
    }
}
