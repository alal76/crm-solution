// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// Service for validating contact information (emails, phones, social media)
/// </summary>
public interface IContactInfoValidationService
{
    /// <summary>
    /// Validate an email address format and optionally check MX records
    /// </summary>
    Task<ValidationResult> ValidateEmailAsync(string email, bool checkMxRecords = false);

    /// <summary>
    /// Validate a phone number format for a specific country
    /// </summary>
    Task<ValidationResult> ValidatePhoneNumberAsync(string phoneNumber, string countryCode = "US");

    /// <summary>
    /// Validate a social media handle/URL for a specific platform
    /// </summary>
    Task<ValidationResult> ValidateSocialMediaAccountAsync(string handleOrUrl, SocialMediaPlatform platform);

    /// <summary>
    /// Format a phone number to international standard
    /// </summary>
    string FormatPhoneNumber(string phoneNumber, string countryCode = "US");

    /// <summary>
    /// Extract the username/handle from a social media profile URL
    /// </summary>
    string? ExtractSocialMediaHandle(string url, SocialMediaPlatform platform);

    /// <summary>
    /// Generate a profile URL from a handle for a specific platform
    /// </summary>
    string? GenerateProfileUrl(string handle, SocialMediaPlatform platform);
}

/// <summary>
/// Result of a validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuggestedCorrection { get; set; }
    public string? NormalizedValue { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();

    public static ValidationResult Success(string? normalizedValue = null, string? message = null) => new()
    {
        IsValid = true,
        NormalizedValue = normalizedValue,
        Message = message ?? "Validation successful"
    };

    public static ValidationResult Failure(string errorMessage, string? suggestedCorrection = null) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage,
        Message = errorMessage,
        SuggestedCorrection = suggestedCorrection
    };
}
