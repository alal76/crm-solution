// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data such as API keys.
/// Uses ASP.NET Core Data Protection API for key management and rotation.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext string. Returns a base64-encoded ciphertext.
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts a base64-encoded ciphertext back to plaintext.
    /// Returns null if decryption fails (e.g. corrupted or rotated key).
    /// </summary>
    string? Decrypt(string ciphertext);

    /// <summary>
    /// Checks whether a string looks like an encrypted value (starts with known prefix).
    /// </summary>
    bool IsEncrypted(string value);
}
