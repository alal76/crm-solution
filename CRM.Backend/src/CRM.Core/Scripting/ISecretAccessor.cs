// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Scripting;

/// <summary>
/// Provides scripts with read-only, policy-gated access to named secrets
/// from the platform secret store (e.g., Azure Key Vault, AWS Secrets Manager).
/// Only secrets declared in <see cref="ScriptDefinition.RequiredSecrets"/> may be read.
/// </summary>
public interface ISecretAccessor
{
    /// <summary>
    /// Returns the plaintext value of a named secret, or <c>null</c> if the secret
    /// does not exist or the script lacks access.
    /// </summary>
    Task<string?> GetAsync(string secretName, CancellationToken cancellationToken = default);
}
