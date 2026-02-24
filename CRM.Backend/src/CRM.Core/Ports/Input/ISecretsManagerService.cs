// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Ports.Input;

/// <summary>
/// Interface for a secrets manager abstraction layer.
/// Supports Vault, AWS Secrets Manager, Azure Key Vault, and GCP Secret Manager.
/// Implements TODO-ARCH-013-003: Secrets manager guidance.
/// </summary>
public interface ISecretsManagerService
{
    /// <summary>
    /// Gets a secret value by its key.
    /// </summary>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The secret value, or null if not found.</returns>
    Task<string?> GetSecretAsync(string secretKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a secret value and deserializes it as a typed object.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized secret, or default if not found.</returns>
    Task<T?> GetSecretAsync<T>(string secretKey, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Sets a secret value.
    /// </summary>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="secretValue">The secret value to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetSecretAsync(string secretKey, string secretValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a secret.
    /// </summary>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the secret was deleted.</returns>
    Task<bool> DeleteSecretAsync(string secretKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a secret exists.
    /// </summary>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the secret exists.</returns>
    Task<bool> SecretExistsAsync(string secretKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists secret keys matching a prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching secret keys.</returns>
    Task<IReadOnlyList<string>> ListSecretsAsync(string? prefix = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a secret (generates a new value, stores it, returns the new value).
    /// </summary>
    /// <param name="secretKey">The key identifying the secret.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new secret value.</returns>
    Task<string> RotateSecretAsync(string secretKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name of the secrets provider being used.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Tests the connection to the secrets provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if connected.</returns>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
