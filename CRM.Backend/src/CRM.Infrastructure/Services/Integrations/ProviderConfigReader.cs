// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using CRM.Core.Ports;

namespace CRM.Infrastructure.Services.Integrations;

/// <summary>
/// Shared helper for resolving provider credentials stored via <see cref="IProviderConfigurationService"/>
/// (the same DB-backed, encrypted-at-rest store used by the Admin &gt; Providers UI /
/// <c>ProviderRegistryService</c>). Reads the <c>crm.{category}.{providerType}</c> configuration key,
/// decrypts/deserializes it, and exposes its fields as a simple case-insensitive dictionary so
/// individual integration services don't each re-implement JSON parsing.
/// </summary>
public static class ProviderConfigReader
{
    /// <summary>
    /// Builds the standard configuration key used by <c>ProviderRegistryService.SetActiveProviderAsync</c>.
    /// </summary>
    public static string BuildConfigKey(string category, string providerType) =>
        $"crm.{category.ToLowerInvariant()}.{providerType.ToLowerInvariant()}";

    /// <summary>
    /// Reads and flattens the stored configuration fields for a given category/provider.
    /// Returns null if no configuration has been saved yet for that provider.
    /// </summary>
    public static async Task<Dictionary<string, string>?> ReadFieldsAsync(
        IProviderConfigurationService configService,
        string category,
        string providerType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configService);

        var configKey = BuildConfigKey(category, providerType);
        var dto = await configService.GetConfigurationAsync(configKey, cancellationToken).ConfigureAwait(false);

        if (dto == null || string.IsNullOrWhiteSpace(dto.ConfigurationData))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dto.ConfigurationData);
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var property in document.RootElement.EnumerateObject())
            {
                result[property.Name] = property.Value.ValueKind == JsonValueKind.String
                    ? property.Value.GetString() ?? string.Empty
                    : property.Value.ToString();
            }

            return result;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns true when every required key is present in <paramref name="fields"/> with a non-blank value.
    /// </summary>
    public static bool HasRequiredFields(Dictionary<string, string>? fields, params string[] requiredKeys)
    {
        if (fields == null || fields.Count == 0)
        {
            return false;
        }

        foreach (var key in requiredKeys)
        {
            if (!fields.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Gets a string field value, or null when absent/blank.</summary>
    public static string? GetValueOrDefault(Dictionary<string, string>? fields, string key)
    {
        if (fields != null && fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return null;
    }
}
