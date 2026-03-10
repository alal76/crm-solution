// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// DTO for biometric credential registration options (WebAuthn).
/// </summary>
public class BiometricRegistrationOptions
{
    public string Challenge { get; set; } = string.Empty;
    public string RelyingPartyId { get; set; } = string.Empty;
    public string RelyingPartyName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string AttestationConveyance { get; set; } = "direct";
    public string AuthenticatorAttachment { get; set; } = "platform"; // platform = biometric
    public string UserVerification { get; set; } = "required";
    public int TimeoutMs { get; set; } = 60000;
    public string[]? ExcludeCredentials { get; set; }
}

/// <summary>
/// DTO for biometric authentication options (WebAuthn).
/// </summary>
public class BiometricAuthenticationOptions
{
    public string Challenge { get; set; } = string.Empty;
    public string RelyingPartyId { get; set; } = string.Empty;
    public string[]? AllowCredentials { get; set; }
    public string UserVerification { get; set; } = "required";
    public int TimeoutMs { get; set; } = 60000;
}

/// <summary>
/// DTO for biometric credential registration response.
/// </summary>
public class BiometricRegistrationResponse
{
    public string CredentialId { get; set; } = string.Empty;
    public string ClientDataJson { get; set; } = string.Empty;
    public string AttestationObject { get; set; } = string.Empty;
    public string? Transports { get; set; }
}

/// <summary>
/// DTO for biometric authentication response.
/// </summary>
public class BiometricAuthenticationResponse
{
    public string CredentialId { get; set; } = string.Empty;
    public string ClientDataJson { get; set; } = string.Empty;
    public string AuthenticatorData { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string? UserHandle { get; set; }
}

/// <summary>
/// DTO for biometric credential info.
/// </summary>
public class BiometricCredentialInfo
{
    public int Id { get; set; }
    public string CredentialId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public bool IsPlatformCredential { get; set; }
}

/// <summary>
/// DTO for biometric auth result.
/// </summary>
public class BiometricAuthResult
{
    public bool Success { get; set; }
    public int? UserId { get; set; }
    public string? CredentialId { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
}

/// <summary>
/// Service interface for platform biometric authentication (WebAuthn with platform authenticator).
/// TODO-AUTH-010: Platform Biometric Authentication
///
/// Uses WebAuthn with authenticatorAttachment: 'platform' for fingerprint, Face ID, Windows Hello, etc.
/// </summary>
public interface IBiometricAuthService
{
    /// <summary>
    /// Generates registration options for a new platform biometric credential.
    /// </summary>
    /// <param name="userId">The user ID to register the credential for</param>
    /// <param name="deviceName">Optional device name for identification</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Registration options to be sent to the client</returns>
    Task<BiometricRegistrationOptions> GetRegistrationOptionsAsync(int userId, string? deviceName = null, CancellationToken ct = default);

    /// <summary>
    /// Completes biometric credential registration.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="response">The registration response from the client</param>
    /// <param name="deviceName">Optional device name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if registration was successful</returns>
    Task<bool> CompleteRegistrationAsync(int userId, BiometricRegistrationResponse response, string? deviceName = null, CancellationToken ct = default);

    /// <summary>
    /// Generates authentication options for biometric login.
    /// </summary>
    /// <param name="userId">Optional user ID (if known, e.g., from username input)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication options to be sent to the client</returns>
    Task<BiometricAuthenticationOptions> GetAuthenticationOptionsAsync(int? userId = null, CancellationToken ct = default);

    /// <summary>
    /// Validates a biometric authentication response.
    /// </summary>
    /// <param name="response">The authentication response from the client</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Authentication result with user ID if successful</returns>
    Task<BiometricAuthResult> ValidateAuthenticationAsync(BiometricAuthenticationResponse response, CancellationToken ct = default);

    /// <summary>
    /// Gets all biometric credentials for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of credential information</returns>
    Task<IEnumerable<BiometricCredentialInfo>> GetUserCredentialsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Removes a biometric credential.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="credentialId">The credential ID to remove</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if removal was successful</returns>
    Task<bool> RemoveCredentialAsync(int userId, string credentialId, CancellationToken ct = default);

    /// <summary>
    /// Removes all biometric credentials for a user.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Number of credentials removed</returns>
    Task<int> RemoveAllCredentialsAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has any biometric credentials registered.
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if the user has biometric credentials</returns>
    Task<bool> HasBiometricCredentialAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if biometric authentication is supported and configured.
    /// </summary>
    /// <returns>True if biometric auth is available</returns>
    bool IsConfigured();
}
