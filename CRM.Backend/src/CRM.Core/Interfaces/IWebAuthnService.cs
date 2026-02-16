using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service interface for WebAuthn (FIDO2) credential management.
/// Implements FIDO Alliance WebAuthn specification for passwordless authentication.
/// </summary>
public interface IWebAuthnService
{
    /// <summary>
    /// Initiates WebAuthn registration ceremony for a user.
    /// Returns challenge options for credential creation on client.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userEmail">User email</param>
    /// <param name="userName">Display name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>WebAuthn registration options with challenge</returns>
    Task<WebAuthnRegistrationOptionsDto> InitiateRegistrationAsync(
        int userId,
        string userEmail,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn registration and stores credential.
    /// Verifies attestation and saves credential public key.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="credentialName">Friendly name for this credential (e.g., "My Laptop")</param>
    /// <param name="attestationResponse">Client attestation response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registered credential DTO</returns>
    Task<WebAuthnCredentialDto> CompleteRegistrationAsync(
        int userId,
        string credentialName,
        WebAuthnAttestationResponseDto attestationResponse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates WebAuthn authentication ceremony for a user.
    /// Returns challenge options for assertion on client.
    /// </summary>
    /// <param name="userEmail">User email or username</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>WebAuthn authentication options with challenge</returns>
    Task<WebAuthnAuthenticationOptionsDto> InitiateAuthenticationAsync(
        string userEmail,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes WebAuthn authentication with client assertion.
    /// Verifies challenge, signature, and counter.
    /// </summary>
    /// <param name="userEmail">User email</param>
    /// <param name="credentialId">Credential ID</param>
    /// <param name="assertionResponse">Client assertion response</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result with user ID if successful</returns>
    Task<WebAuthnAuthenticationResultDto> CompleteAuthenticationAsync(
        string userEmail,
        string credentialId,
        WebAuthnAssertionResponseDto assertionResponse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all registered credentials for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's WebAuthn credentials</returns>
    Task<IEnumerable<WebAuthnCredentialDto>> GetCredentialsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates credential name/friendly label.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="credentialId">Credential ID</param>
    /// <param name="newName">New friendly name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated credential DTO</returns>
    Task<WebAuthnCredentialDto> UpdateCredentialAsync(
        int userId,
        string credentialId,
        string newName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a registered credential.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="credentialId">Credential ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful</returns>
    Task<bool> RemoveCredentialAsync(
        int userId,
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user has any registered WebAuthn credentials.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has at least one credential</returns>
    Task<bool> HasCredentialsAsync(
        int userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credential by ID for verification during authentication.
    /// </summary>
    /// <param name="credentialId">Base64-encoded credential ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Stored credential data for verification</returns>
    Task<StoredWebAuthnCredentialDto?> GetStoredCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default);
}
