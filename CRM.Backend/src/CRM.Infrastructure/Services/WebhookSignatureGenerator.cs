// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Security.Cryptography;
using System.Text;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Generates and validates HMAC-SHA256 signatures for webhook payloads.
/// Extracted from WebhookService for reusability and testability.
/// </summary>
public interface IWebhookSignatureGenerator
{
    /// <summary>Generate HMAC-SHA256 signature for a payload</summary>
    string GenerateSignature(string payload, string secret);

    /// <summary>Validate a received signature against expected</summary>
    bool ValidateSignature(string payload, string secret, string receivedSignature);

    /// <summary>Generate a timestamp-based signature (prevents replay attacks)</summary>
    string GenerateTimestampedSignature(string payload, string secret, long timestamp);
}

public class WebhookSignatureGenerator : IWebhookSignatureGenerator
{
    public string GenerateSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public bool ValidateSignature(string payload, string secret, string receivedSignature)
    {
        var expected = GenerateSignature(payload, secret);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(receivedSignature));
    }

    public string GenerateTimestampedSignature(string payload, string secret, long timestamp)
    {
        var signedPayload = $"{timestamp}.{payload}";
        return GenerateSignature(signedPayload, secret);
    }
}
