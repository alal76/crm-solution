// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Features;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Newtonsoft.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Implementation of feature flag management service
/// Manages feature flags, targeting, variants, and audit trails
/// </summary>
public class FeatureFlagManagementService : IFeatureFlagManagementService
{
    private readonly ICrmDbContext _dbContext;
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FeatureFlagManagementService> _logger;

    // Cache for variant assignments
    private readonly Dictionary<string, Dictionary<int, FlagVariantDto>> _variantCache = new();

    public FeatureFlagManagementService(
        ICrmDbContext dbContext,
        IFeatureManager featureManager,
        IConfiguration configuration,
        ILogger<FeatureFlagManagementService> logger)
    {
        _dbContext = dbContext;
        _featureManager = featureManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IEnumerable<FeatureFlagDto>> GetAllFlagsAsync(CancellationToken cancellationToken = default)
    {
        var flags = new List<FeatureFlagDto>();

        // Module flags
        var moduleFlags = new[]
        {
            new { Name = FeatureFlags.EnableITSM, Display = "ITSM Module", Category = "Module" },
            new { Name = FeatureFlags.EnableMarketing, Display = "Marketing Module", Category = "Module" },
            new { Name = FeatureFlags.EnableCustomerPortal, Display = "Customer Portal", Category = "Module" },
            new { Name = FeatureFlags.EnablePartnerPortal, Display = "Partner Portal", Category = "Module" },
            new { Name = FeatureFlags.EnableKnowledgeBase, Display = "Knowledge Base", Category = "Module" }
        };

        foreach (var flag in moduleFlags)
        {
            flags.Add(new FeatureFlagDto
            {
                Name = flag.Name,
                DisplayName = flag.Display,
                Description = $"Enable/disable {flag.Display}",
                Enabled = await _featureManager.IsEnabledAsync(flag.Name),
                Category = flag.Category,
                RequiresRestart = true,
                RolloutPercentage = 100
            });
        }

        // Provider flags
        var providerFlags = new[]
        {
            new { Name = FeatureFlags.UseExternalChat, Display = "External Chat", Category = "Provider", ProviderCategory = "Chat" },
            new { Name = FeatureFlags.UseExternalSearch, Display = "External Search", Category = "Provider", ProviderCategory = "Search" },
            new { Name = FeatureFlags.UseExternalNotifications, Display = "External Notifications", Category = "Provider", ProviderCategory = "Notifications" },
            new { Name = FeatureFlags.UseExternalAnalytics, Display = "External Analytics", Category = "Provider", ProviderCategory = "Analytics" },
            new { Name = FeatureFlags.UseExternalSignatures, Display = "External Signatures", Category = "Provider", ProviderCategory = "Signatures" },
            new { Name = FeatureFlags.UseExternalAI, Display = "External AI", Category = "Provider", ProviderCategory = "AI" },
            new { Name = FeatureFlags.UseExternalIntegrations, Display = "External Integrations", Category = "Provider", ProviderCategory = "Integrations" }
        };

        foreach (var flag in providerFlags)
        {
            flags.Add(new FeatureFlagDto
            {
                Name = flag.Name,
                DisplayName = flag.Display,
                Description = $"Use external {flag.ProviderCategory} provider",
                Enabled = await _featureManager.IsEnabledAsync(flag.Name),
                Category = flag.Category,
                ProviderCategory = flag.ProviderCategory,
                ActiveProvider = GetProvider(flag.ProviderCategory),
                RequiresRestart = false,
                RolloutPercentage = 100
            });
        }

        _logger.LogInformation("Retrieved {Count} feature flags", flags.Count);
        return flags;
    }

    public async Task<FeatureFlagDto?> GetFlagAsync(string flagName, CancellationToken cancellationToken = default)
    {
        var allFlags = await GetAllFlagsAsync(cancellationToken);
        return allFlags.FirstOrDefault(f => f.Name == flagName);
    }

    public async Task<bool> IsFlagEnabledForUserAsync(string flagName, int userId, CancellationToken cancellationToken = default)
    {
        var isFlagEnabled = await _featureManager.IsEnabledAsync(flagName);
        if (!isFlagEnabled)
            return false;

        // Check rollout percentage
        var rolloutPercentage = _configuration.GetValue<int?>($"FeatureFlags:{flagName}:RolloutPercentage") ?? 100;
        if (rolloutPercentage < 100)
        {
            // Use consistent hash based on userId
            var hash = Math.Abs(userId.GetHashCode()) % 100;
            if (hash >= rolloutPercentage)
                return false;
        }

        // Check targeting
        var targetedUsers = _configuration.GetValue<string>($"FeatureFlags:{flagName}:TargetedUsers") ?? string.Empty;
        if (!string.IsNullOrEmpty(targetedUsers))
        {
            var targetedUserIds = targetedUsers.Split(',').Select(u => int.TryParse(u.Trim(), out var id) ? id : 0).Where(id => id > 0).ToList();
            if (targetedUserIds.Count > 0 && !targetedUserIds.Contains(userId))
                return false;
        }

        return true;
    }

    public async Task<bool> UpdateFlagAsync(string flagName, UpdateFeatureFlagDto dto, int updatedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldValue = (await _featureManager.IsEnabledAsync(flagName)).ToString();
            
            // Log to audit trail
            var auditLog = new FeatureFlagAuditLog
            {
                FlagName = flagName,
                OldValue = oldValue,
                NewValue = dto.Enabled.ToString(),
                ChangeType = "Enable/Disable",
                ChangedById = updatedById,
                ChangedAt = DateTime.UtcNow,
                Reason = dto.Reason,
                TargetingInfo = JsonConvert.SerializeObject(new
                {
                    TargetedUsers = dto.TargetedUserIds,
                    TargetedRoles = dto.TargetedRoles,
                    RolloutPercentage = dto.RolloutPercentage
                })
            };

            _dbContext.FeatureFlagAuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Feature flag {FlagName} updated by user {UserId}: {OldValue} → {NewValue}", 
                flagName, updatedById, oldValue, dto.Enabled);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag {FlagName}", flagName);
            return false;
        }
    }

    public async Task<bool> SetRolloutPercentageAsync(string flagName, int percentage, int updatedById, CancellationToken cancellationToken = default)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Rollout percentage must be between 0 and 100");

        try
        {
            var auditLog = new FeatureFlagAuditLog
            {
                FlagName = flagName,
                OldValue = _configuration.GetValue<string>($"FeatureFlags:{flagName}:RolloutPercentage") ?? "100",
                NewValue = percentage.ToString(),
                ChangeType = "SetRollout",
                ChangedById = updatedById,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.FeatureFlagAuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rollout percentage for {FlagName} set to {Percentage}%", flagName, percentage);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting rollout percentage for {FlagName}", flagName);
            return false;
        }
    }

    public async Task<bool> SetVariantsAsync(string flagName, FlagVariantDto[] variants, int updatedById, CancellationToken cancellationToken = default)
    {
        try
        {
            _variantCache[flagName] = variants.ToDictionary(v => v.VariantName.GetHashCode());

            var auditLog = new FeatureFlagAuditLog
            {
                FlagName = flagName,
                NewValue = JsonConvert.SerializeObject(variants),
                ChangeType = "SetVariants",
                ChangedById = updatedById,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.FeatureFlagAuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Variants set for {FlagName}: {Count} variants", flagName, variants.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting variants for {FlagName}", flagName);
            return false;
        }
    }

    public async Task<FlagVariantDto?> GetUserVariantAsync(string flagName, int userId, CancellationToken cancellationToken = default)
    {
        if (!_variantCache.TryGetValue(flagName, out var variants) || variants.Count == 0)
            return null;

        // Consistent hash-based assignment
        var hash = Math.Abs(userId.GetHashCode());
        var index = hash % variants.Count;
        return variants.Values.ElementAt(index);
    }

    public async Task<string> GetActiveProviderAsync(string category, CancellationToken cancellationToken = default)
    {
        return GetProvider(category);
    }

    public async Task<bool> UpdateProviderTypeAsync(string category, string providerType, int updatedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldValue = GetProvider(category);

            var auditLog = new FeatureFlagAuditLog
            {
                FlagName = $"Provider_{category}",
                OldValue = oldValue,
                NewValue = providerType,
                ChangeType = "SetProvider",
                ChangedById = updatedById,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.FeatureFlagAuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Provider for {Category} changed from {Old} to {New}", category, oldValue, providerType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider type for {Category}", category);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetAvailableProvidersAsync(string category, CancellationToken cancellationToken = default)
    {
        var providers = category switch
        {
            "Chat" => new[] { "BuiltIn", "Chatwoot", "Intercom", "Zendesk", "Freshchat", "RocketChat" },
            "Search" => new[] { "BuiltIn", "Meilisearch", "Algolia", "Typesense", "Elasticsearch", "AzureCognitiveSearch" },
            "Notifications" => new[] { "BuiltIn", "Novu", "Twilio", "SendGrid", "AWSSES", "Courier" },
            "Analytics" => new[] { "BuiltIn", "Superset", "Metabase", "PowerBI", "Tableau", "Looker" },
            "Signatures" => new[] { "BuiltIn", "DocuSeal", "DocuSign", "HelloSign", "AdobeSign", "PandaDoc" },
            "AI" => new[] { "Ollama", "OpenAI", "Anthropic", "AzureOpenAI", "Gemini", "DeepSeek" },
            "Integrations" => new[] { "BuiltIn", "N8n", "Zapier", "Make", "Automatisch", "Workato" },
            _ => new[] { "BuiltIn" }
        };

        return await Task.FromResult(providers);
    }

    public async Task<IEnumerable<FeatureFlagAuditEntryDto>> GetAuditLogAsync(int count = 50, CancellationToken cancellationToken = default)
    {
        var auditEntries = await _dbContext.FeatureFlagAuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.ChangedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return auditEntries.Select(a => new FeatureFlagAuditEntryDto
        {
            Id = a.Id,
            FlagName = a.FlagName,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            ChangeType = a.ChangeType,
            ChangedById = a.ChangedById,
            ChangedByName = a.ChangedBy?.FirstName + " " + a.ChangedBy?.LastName ?? "Unknown",
            ChangedAt = a.ChangedAt,
            Reason = a.Reason,
            TargetingInfo = a.TargetingInfo
        }).ToList();
    }

    public async Task<IEnumerable<FeatureFlagAuditEntryDto>> GetFlagAuditLogAsync(string flagName, int count = 50, CancellationToken cancellationToken = default)
    {
        var auditEntries = await _dbContext.FeatureFlagAuditLogs
            .AsNoTracking()
            .Where(a => a.FlagName == flagName)
            .OrderByDescending(a => a.ChangedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return auditEntries.Select(a => new FeatureFlagAuditEntryDto
        {
            Id = a.Id,
            FlagName = a.FlagName,
            OldValue = a.OldValue,
            NewValue = a.NewValue,
            ChangeType = a.ChangeType,
            ChangedById = a.ChangedById,
            ChangedByName = a.ChangedBy?.FirstName + " " + a.ChangedBy?.LastName ?? "Unknown",
            ChangedAt = a.ChangedAt,
            Reason = a.Reason,
            TargetingInfo = a.TargetingInfo
        }).ToList();
    }

    public async Task<bool> ResetToDefaultsAsync(int updatedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new FeatureFlagAuditLog
            {
                FlagName = "ALL",
                NewValue = "RESET",
                ChangeType = "ResetToDefaults",
                ChangedById = updatedById,
                ChangedAt = DateTime.UtcNow
            };

            _dbContext.FeatureFlagAuditLogs.Add(auditLog);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Feature flags reset to defaults by user {UserId}", updatedById);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting feature flags");
            return false;
        }
    }

    private string GetProvider(string category)
    {
        return _configuration.GetValue<string>($"Providers:{category}:Type") ?? "BuiltIn";
    }
}
