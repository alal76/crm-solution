using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// WebAuthn registration options for client-side credential creation.
    /// </summary>
    public class WebAuthnRegistrationOptionsDto
    {
        /// <summary>
        /// Challenge bytes (encoded as base64url in JSON).
        /// Server-generated random data to prevent replay attacks.
        /// </summary>
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; } = string.Empty;

        /// <summary>
        /// Relying party (server) identity.
        /// </summary>
        [JsonPropertyName("rp")]
        public WebAuthnRelyingParty Rp { get; set; } = new();

        /// <summary>
        /// User identity.
        /// </summary>
        [JsonPropertyName("user")]
        public WebAuthnUserEntity User { get; set; } = new();

        /// <summary>
        /// Requested public key algorithm and parameters.
        /// </summary>
        [JsonPropertyName("pubKeyCredParams")]
        public List<WebAuthnPublicKeyAlgorithm> PubKeyCredParams { get; set; } = new();

        /// <summary>
        /// Timeout in milliseconds.
        /// </summary>
        [JsonPropertyName("timeout")]
        public int Timeout { get; set; } = 60000;

        /// <summary>
        /// Attestation conveyance preference.
        /// </summary>
        [JsonPropertyName("attestation")]
        public string Attestation { get; set; } = "direct";

        /// <summary>
        /// User verification requirement.
        /// </summary>
        [JsonPropertyName("userVerification")]
        public string UserVerification { get; set; } = "preferred";
    }

    /// <summary>
    /// WebAuthn relying party (server) identity.
    /// </summary>
    public class WebAuthnRelyingParty
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "CRM Solution";

        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }

    /// <summary>
    /// WebAuthn user entity for registration.
    /// </summary>
    public class WebAuthnUserEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;
    }

    /// <summary>
    /// WebAuthn public key algorithm specification.
    /// </summary>
    public class WebAuthnPublicKeyAlgorithm
    {
        [JsonPropertyName("alg")]
        public int Alg { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "public-key";
    }

    /// <summary>
    /// Client attestation response from credential creation.
    /// </summary>
    public class WebAuthnAttestationResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("rawId")]
        public string RawId { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public WebAuthnAttestationResponse Response { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "public-key";

        [JsonPropertyName("transports")]
        public List<string> Transports { get; set; } = new();
    }

    /// <summary>
    /// Attestation response data.
    /// </summary>
    public class WebAuthnAttestationResponse
    {
        [JsonPropertyName("clientDataJSON")]
        public string ClientDataJson { get; set; } = string.Empty;

        [JsonPropertyName("attestationObject")]
        public string AttestationObject { get; set; } = string.Empty;
    }

    /// <summary>
    /// Client assertion response from credential assertion.
    /// </summary>
    public class WebAuthnAssertionResponseDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("rawId")]
        public string RawId { get; set; } = string.Empty;

        [JsonPropertyName("response")]
        public WebAuthnAssertionResponse Response { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = "public-key";
    }

    /// <summary>
    /// Assertion response data.
    /// </summary>
    public class WebAuthnAssertionResponse
    {
        [JsonPropertyName("clientDataJSON")]
        public string ClientDataJson { get; set; } = string.Empty;

        [JsonPropertyName("authenticatorData")]
        public string AuthenticatorData { get; set; } = string.Empty;

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;

        [JsonPropertyName("userHandle")]
        public string? UserHandle { get; set; }
    }

    /// <summary>
    /// WebAuthn authentication options for client-side assertion.
    /// </summary>
    public class WebAuthnAuthenticationOptionsDto
    {
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; } = string.Empty;

        [JsonPropertyName("timeout")]
        public int Timeout { get; set; } = 60000;

        [JsonPropertyName("rpId")]
        public string RpId { get; set; } = string.Empty;

        [JsonPropertyName("allowCredentials")]
        public List<WebAuthnAllowedCredential> AllowCredentials { get; set; } = new();

        [JsonPropertyName("userVerification")]
        public string UserVerification { get; set; } = "preferred";
    }

    /// <summary>
    /// Allowed credential descriptor for authentication.
    /// </summary>
    public class WebAuthnAllowedCredential
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "public-key";

        [JsonPropertyName("transports")]
        public List<string> Transports { get; set; } = new();
    }

    /// <summary>
    /// WebAuthn authentication result.
    /// </summary>
    public class WebAuthnAuthenticationResultDto
    {
        public bool IsValid { get; set; }
        public int? UserId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime AuthenticatedAt { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialName { get; set; }
    }

    /// <summary>
    /// Registered WebAuthn credential DTO.
    /// </summary>
    public class WebAuthnCredentialDto
    {
        public string CredentialId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public long SignatureCounter { get; set; }
        public string AttestationFormat { get; set; } = string.Empty;
        public List<string> Transports { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public bool IsBackupEligible { get; set; }
        public string? AagGuid { get; set; }
    }

    /// <summary>
    /// Stored credential for verification during authentication.
    /// </summary>
    public class StoredWebAuthnCredentialDto
    {
        public string CredentialId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public byte[] PublicKey { get; set; } = Array.Empty<byte>();
        public long SignatureCounter { get; set; }
        public byte[] CredentialIdBytes { get; set; } = Array.Empty<byte>();
    }
}
