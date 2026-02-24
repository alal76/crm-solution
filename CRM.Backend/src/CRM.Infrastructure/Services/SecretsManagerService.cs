// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using CRM.Core.Ports.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Configuration-driven secrets manager implementation.
/// Implements TODO-ARCH-013-003.
///
/// This is a local/environment-variable-backed implementation.
/// In production, swap the DI registration for an implementation
/// backed by HashiCorp Vault, AWS Secrets Manager, Azure Key Vault,
/// or GCP Secret Manager.
///
/// Secrets are read from IConfiguration (which already layers
/// appsettings.json → environment variables → user-secrets).
/// Write operations are held in memory for the lifetime of the process.
/// </summary>
public class SecretsManagerService : ISecretsManagerService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecretsManagerService> _logger;
    private readonly ConcurrentDictionary<string, string> _overrides = new(StringComparer.OrdinalIgnoreCase);

    private const string SecretsPrefix = "Secrets:";

    public SecretsManagerService(
        IConfiguration configuration,
        ILogger<SecretsManagerService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderName => _configuration["SecretsManager:Provider"] ?? "Local";

    /// <inheritdoc />
    public Task<string?> GetSecretAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

        // Check in-memory overrides first
        if (_overrides.TryGetValue(secretKey, out var overrideValue))
        {
            return Task.FromResult<string?>(overrideValue);
        }

        // Fall back to IConfiguration
        var value = _configuration[$"{SecretsPrefix}{secretKey}"];
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public async Task<T?> GetSecretAsync<T>(string secretKey, CancellationToken cancellationToken = default) where T : class
    {
        var json = await GetSecretAsync(secretKey, cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize secret {SecretKey} as {Type}", secretKey, typeof(T).Name);
            return default;
        }
    }

    /// <inheritdoc />
    public Task SetSecretAsync(string secretKey, string secretValue, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);
        ArgumentNullException.ThrowIfNull(secretValue);

        _overrides[secretKey] = secretValue;
        _logger.LogInformation("Secret {SecretKey} stored in memory (Local provider). " +
            "Note: This will not survive a process restart. Use a production secrets manager for persistence.",
            secretKey);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> DeleteSecretAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

        var removed = _overrides.TryRemove(secretKey, out _);
        if (removed)
        {
            _logger.LogInformation("Secret {SecretKey} removed from in-memory store", secretKey);
        }
        else
        {
            _logger.LogDebug("Secret {SecretKey} not found in in-memory overrides (may still exist in configuration)", secretKey);
        }

        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public async Task<bool> SecretExistsAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        var value = await GetSecretAsync(secretKey, cancellationToken);
        return value != null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListSecretsAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Gather keys from in-memory overrides
        foreach (var key in _overrides.Keys)
        {
            if (prefix == null || key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                keys.Add(key);
            }
        }

        // Gather keys from the Secrets: configuration section
        var section = _configuration.GetSection(SecretsPrefix.TrimEnd(':'));
        if (section.Exists())
        {
            foreach (var child in section.GetChildren())
            {
                var keyName = child.Key;
                if (prefix == null || keyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    keys.Add(keyName);
                }
            }
        }

        IReadOnlyList<string> result = keys.OrderBy(k => k).ToList().AsReadOnly();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<string> RotateSecretAsync(string secretKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

        // Generate a cryptographically random 32-byte value, encode as base64
        var bytes = RandomNumberGenerator.GetBytes(32);
        var newValue = Convert.ToBase64String(bytes);

        _overrides[secretKey] = newValue;
        _logger.LogInformation("Secret {SecretKey} rotated (Local provider, in-memory only)", secretKey);

        return Task.FromResult(newValue);
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        // Local provider is always available
        _logger.LogDebug("Secrets manager test connection: Local provider is always available");
        return Task.FromResult(true);
    }
}
