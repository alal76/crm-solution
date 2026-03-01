// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Encrypts and decrypts sensitive data using ASP.NET Core Data Protection API.
/// Keys are managed automatically (stored in the app's key ring).
/// All encrypted values start with "ENC:" prefix for identification.
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const string EncryptedPrefix = "ENC:";
    private const string Purpose = "CRM.LLMProviderSettings.ApiKeys";
    private readonly IDataProtector _protector;
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(
        IDataProtectionProvider dataProtectionProvider,
        ILogger<EncryptionService> logger)
    {
        _protector = dataProtectionProvider.CreateProtector(Purpose);
        _logger = logger;
    }

    /// <inheritdoc />
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrWhiteSpace(plaintext))
        {
            return plaintext;
        }

        // Don't double-encrypt
        if (IsEncrypted(plaintext))
        {
            return plaintext;
        }

        var encrypted = _protector.Protect(plaintext);
        return $"{EncryptedPrefix}{encrypted}";
    }

    /// <inheritdoc />
    public string? Decrypt(string ciphertext)
    {
        if (string.IsNullOrWhiteSpace(ciphertext))
        {
            return ciphertext;
        }

        if (!IsEncrypted(ciphertext))
        {
            return ciphertext;
        }

        try
        {
            var encrypted = ciphertext[EncryptedPrefix.Length..];
            return _protector.Unprotect(encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt value. It may have been encrypted with a different key or is corrupted");
            return null;
        }
    }

    /// <inheritdoc />
    public bool IsEncrypted(string value)
    {
        return !string.IsNullOrWhiteSpace(value) && value.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
    }
}
