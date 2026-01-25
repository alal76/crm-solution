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
