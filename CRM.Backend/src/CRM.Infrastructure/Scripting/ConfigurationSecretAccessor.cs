// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting;

/// <summary>
/// <see cref="ISecretAccessor"/> backed by <see cref="IConfiguration"/>.
/// Resolves secrets from the configuration hierarchy (environment variables,
/// Azure Key Vault via the Key Vault configuration provider, or <c>secrets.json</c>
/// in development).
/// <para>
/// Scripts request secrets by logical name; the accessor resolves from the
/// <c>ScriptSecrets:{secretName}</c> configuration key.  Only secrets declared
/// in <see cref="ScriptDefinition.RequiredSecrets"/> should be requested.
/// </para>
/// </summary>
public class ConfigurationSecretAccessor : ISecretAccessor
{
    private readonly IConfiguration _config;
    private readonly ILogger<ConfigurationSecretAccessor> _logger;

    /// <summary>Configuration section that holds script-accessible secrets.</summary>
    private const string SecretsSection = "ScriptSecrets";

    /// <summary>Initialises a new <see cref="ConfigurationSecretAccessor"/>.</summary>
    public ConfigurationSecretAccessor(IConfiguration config, ILogger<ConfigurationSecretAccessor> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public Task<string?> GetAsync(string secretName, CancellationToken cancellationToken = default)
    {
        // Lookup order: ScriptSecrets:{secretName} in configuration hierarchy
        // (appsettings → environment variables → Key Vault provider)
        var value = _config[$"{SecretsSection}:{secretName}"];
        if (value == null)
        {
            _logger.LogWarning("Secret '{SecretName}' not found in configuration", secretName);
        }

        return Task.FromResult(value);
    }
}
