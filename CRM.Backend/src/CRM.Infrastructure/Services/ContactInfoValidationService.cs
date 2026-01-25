using System.Net.Sockets;
using System.Text.RegularExpressions;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for validating contact information (emails, phones, social media)
/// </summary>
public class ContactInfoValidationService : IContactInfoValidationService
{
    private readonly ILogger<ContactInfoValidationService> _logger;
    
    // Email regex - RFC 5322 compliant simplified version
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    // Phone number patterns by country
    private static readonly Dictionary<string, (string Pattern, string Format)> PhonePatterns = new()
    {
        { "US", (@"^(\+?1)?[-.\s]?\(?[2-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", "+1 (XXX) XXX-XXXX") },
        { "CA", (@"^(\+?1)?[-.\s]?\(?[2-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", "+1 (XXX) XXX-XXXX") },
        { "GB", (@"^(\+?44)?[-.\s]?\(?0?\d{2,4}\)?[-.\s]?\d{3,4}[-.\s]?\d{3,4}$", "+44 XXXX XXXXXX") },
        { "UK", (@"^(\+?44)?[-.\s]?\(?0?\d{2,4}\)?[-.\s]?\d{3,4}[-.\s]?\d{3,4}$", "+44 XXXX XXXXXX") },
        { "DE", (@"^(\+?49)?[-.\s]?\(?0?\d{2,5}\)?[-.\s]?\d{3,10}$", "+49 XXX XXXXXXXX") },
        { "FR", (@"^(\+?33)?[-.\s]?\(?0?\d{1,2}\)?[-.\s]?\d{2}[-.\s]?\d{2}[-.\s]?\d{2}[-.\s]?\d{2}$", "+33 X XX XX XX XX") },
        { "IN", (@"^(\+?91)?[-.\s]?\(?[6-9]\d{4}\)?[-.\s]?\d{5}$", "+91 XXXXX XXXXX") },
        { "AU", (@"^(\+?61)?[-.\s]?\(?0?[2-9]\)?[-.\s]?\d{4}[-.\s]?\d{4}$", "+61 X XXXX XXXX") },
        { "JP", (@"^(\+?81)?[-.\s]?\(?0?\d{1,4}\)?[-.\s]?\d{1,4}[-.\s]?\d{4}$", "+81 XX XXXX XXXX") },
        { "CN", (@"^(\+?86)?[-.\s]?1[3-9]\d{9}$", "+86 XXX XXXX XXXX") },
        { "BR", (@"^(\+?55)?[-.\s]?\(?[1-9]{2}\)?[-.\s]?9?\d{4}[-.\s]?\d{4}$", "+55 (XX) XXXXX-XXXX") },
        { "MX", (@"^(\+?52)?[-.\s]?\(?[1-9]\d{2}\)?[-.\s]?\d{3}[-.\s]?\d{4}$", "+52 XXX XXX XXXX") },
    };
    
    // Social media URL patterns for extracting handles
    private static readonly Dictionary<SocialMediaPlatform, (string UrlPattern, string HandlePattern, string BaseUrl)> SocialMediaPatterns = new()
    {
        { SocialMediaPlatform.LinkedIn, (
            @"(?:https?://)?(?:www\.)?linkedin\.com/(?:in|company)/([a-zA-Z0-9_-]+)/?",
            @"^[a-zA-Z0-9_-]{3,100}$",
            "https://linkedin.com/in/") },
        { SocialMediaPlatform.Twitter, (
            @"(?:https?://)?(?:www\.)?(?:twitter\.com|x\.com)/([a-zA-Z0-9_]+)/?",
            @"^[a-zA-Z0-9_]{1,15}$",
            "https://x.com/") },
        { SocialMediaPlatform.Facebook, (
            @"(?:https?://)?(?:www\.)?facebook\.com/([a-zA-Z0-9.]+)/?",
            @"^[a-zA-Z0-9.]{5,50}$",
            "https://facebook.com/") },
        { SocialMediaPlatform.Instagram, (
            @"(?:https?://)?(?:www\.)?instagram\.com/([a-zA-Z0-9_.]+)/?",
            @"^[a-zA-Z0-9_.]{1,30}$",
            "https://instagram.com/") },
        { SocialMediaPlatform.YouTube, (
            @"(?:https?://)?(?:www\.)?youtube\.com/(?:@|c/|channel/|user/)?([a-zA-Z0-9_-]+)/?",
            @"^[a-zA-Z0-9_-]{1,100}$",
            "https://youtube.com/@") },
        { SocialMediaPlatform.TikTok, (
            @"(?:https?://)?(?:www\.)?tiktok\.com/@([a-zA-Z0-9_.]+)/?",
            @"^[a-zA-Z0-9_.]{2,24}$",
            "https://tiktok.com/@") },
        { SocialMediaPlatform.WhatsApp, (
            @"(?:https?://)?(?:wa\.me|api\.whatsapp\.com/send\?phone=)(\+?[0-9]+)",
            @"^\+?[0-9]{10,15}$",
            "https://wa.me/") },
        { SocialMediaPlatform.Telegram, (
            @"(?:https?://)?(?:t\.me|telegram\.me)/([a-zA-Z0-9_]+)/?",
            @"^[a-zA-Z0-9_]{5,32}$",
            "https://t.me/") },
    };
    
    public ContactInfoValidationService(ILogger<ContactInfoValidationService> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task<ValidationResult> ValidateEmailAsync(string email, bool checkMxRecords = false)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return ValidationResult.Failure("Email address is required");
        }
        
        email = email.Trim().ToLowerInvariant();
        
        // Basic format validation
        if (!EmailRegex.IsMatch(email))
        {
            return ValidationResult.Failure("Invalid email format");
        }
        
        // Length check
        if (email.Length > 254)
        {
            return ValidationResult.Failure("Email address is too long (max 254 characters)");
        }
        
        var parts = email.Split('@');
        if (parts.Length != 2)
        {
            return ValidationResult.Failure("Invalid email format");
        }
        
        var localPart = parts[0];
        var domain = parts[1];
        
        // Local part length check
        if (localPart.Length > 64)
        {
            return ValidationResult.Failure("Local part of email is too long (max 64 characters)");
        }
        
        // Domain validation
        if (domain.Length > 253)
        {
            return ValidationResult.Failure("Domain part of email is too long");
        }
        
        // Check for common typos in popular domains
        var typoCheck = CheckDomainTypos(domain);
        if (typoCheck != null)
        {
            return ValidationResult.Failure($"Did you mean {localPart}@{typoCheck}?");
        }
        
        // Check for disposable email domains
        if (IsDisposableEmail(domain))
        {
            return ValidationResult.Failure("Disposable email addresses are not allowed");
        }
        
        // MX record check (optional, async)
        if (checkMxRecords)
        {
            try
            {
                var hasMx = await CheckMxRecordsAsync(domain);
                if (!hasMx)
                {
                    return ValidationResult.Failure($"Domain '{domain}' does not accept email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check MX records for domain: {Domain}", domain);
                // Don't fail if MX check fails, just log it
            }
        }
        
        return ValidationResult.Success(email);
    }
    
    /// <inheritdoc />
    public Task<ValidationResult> ValidatePhoneNumberAsync(string phoneNumber, string countryCode = "US")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Task.FromResult(ValidationResult.Failure("Phone number is required"));
        }
        
        // Normalize country code
        countryCode = countryCode.ToUpperInvariant();
        
        // Remove common formatting characters for validation
        var normalized = Regex.Replace(phoneNumber.Trim(), @"[\s\-\.\(\)]", "");
        
        // Check if we have a pattern for this country
        if (PhonePatterns.TryGetValue(countryCode, out var pattern))
        {
            if (!Regex.IsMatch(phoneNumber.Trim(), pattern.Pattern, RegexOptions.IgnoreCase))
            {
                return Task.FromResult(ValidationResult.Failure(
                    $"Invalid phone number format for {countryCode}. Expected format: {pattern.Format}"));
            }
        }
        else
        {
            // Generic validation for unknown countries - just check it looks like a phone number
            if (!Regex.IsMatch(normalized, @"^\+?\d{7,15}$"))
            {
                return Task.FromResult(ValidationResult.Failure(
                    "Invalid phone number format. Please include country code and 7-15 digits."));
            }
        }
        
        // Format the phone number
        var formatted = FormatPhoneNumber(phoneNumber, countryCode);
        
        return Task.FromResult(ValidationResult.Success(formatted));
    }
    
    /// <inheritdoc />
    public Task<ValidationResult> ValidateSocialMediaAccountAsync(string handleOrUrl, SocialMediaPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(handleOrUrl))
        {
            return Task.FromResult(ValidationResult.Failure("Social media handle or URL is required"));
        }
        
        handleOrUrl = handleOrUrl.Trim();
        
        // Handle "Other" platform - just validate it looks reasonable
        if (platform == SocialMediaPlatform.Other)
        {
            if (handleOrUrl.Length < 1 || handleOrUrl.Length > 200)
            {
                return Task.FromResult(ValidationResult.Failure("Handle/URL must be between 1 and 200 characters"));
            }
            return Task.FromResult(ValidationResult.Success(handleOrUrl));
        }
        
        if (!SocialMediaPatterns.TryGetValue(platform, out var patterns))
        {
            // Unknown platform, just accept it
            return Task.FromResult(ValidationResult.Success(handleOrUrl));
        }
        
        string? handle;
        
        // Check if it's a URL
        if (handleOrUrl.Contains("://") || handleOrUrl.Contains(".com") || handleOrUrl.Contains(".me"))
        {
            // Try to extract handle from URL
            var urlMatch = Regex.Match(handleOrUrl, patterns.UrlPattern, RegexOptions.IgnoreCase);
            if (!urlMatch.Success || urlMatch.Groups.Count < 2)
            {
                return Task.FromResult(ValidationResult.Failure(
                    $"Invalid {platform} URL format"));
            }
            handle = urlMatch.Groups[1].Value;
        }
        else
        {
            // It's a handle - remove @ if present
            handle = handleOrUrl.TrimStart('@');
        }
        
        // Validate handle format
        if (!Regex.IsMatch(handle, patterns.HandlePattern))
        {
            return Task.FromResult(ValidationResult.Failure(
                $"Invalid {platform} handle format. Handle: @{handle}"));
        }
        
        // Generate full profile URL
        var profileUrl = GenerateProfileUrl(handle, platform);
        
        var result = ValidationResult.Success(handle);
        result.Details["ProfileUrl"] = profileUrl ?? "";
        result.Details["Handle"] = handle;
        
        return Task.FromResult(result);
    }
    
    /// <inheritdoc />
    public string FormatPhoneNumber(string phoneNumber, string countryCode = "US")
    {
        // Remove all non-digits except leading +
        var hasPlus = phoneNumber.TrimStart().StartsWith("+");
        var digits = Regex.Replace(phoneNumber, @"[^\d]", "");
        
        // Format based on country
        switch (countryCode.ToUpperInvariant())
        {
            case "US":
            case "CA":
                // Remove leading 1 if present
                if (digits.StartsWith("1") && digits.Length == 11)
                    digits = digits[1..];
                
                if (digits.Length == 10)
                    return $"+1 ({digits[..3]}) {digits[3..6]}-{digits[6..]}";
                break;
                
            case "GB":
            case "UK":
                if (digits.StartsWith("44"))
                    digits = digits[2..];
                if (digits.StartsWith("0"))
                    digits = digits[1..];
                if (digits.Length >= 10)
                    return $"+44 {digits[..4]} {digits[4..]}";
                break;
                
            case "IN":
                if (digits.StartsWith("91"))
                    digits = digits[2..];
                if (digits.Length == 10)
                    return $"+91 {digits[..5]} {digits[5..]}";
                break;
        }
        
        // Default formatting: just add + if needed
        return hasPlus ? $"+{digits}" : digits;
    }
    
    /// <inheritdoc />
    public string? ExtractSocialMediaHandle(string url, SocialMediaPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
            
        if (!SocialMediaPatterns.TryGetValue(platform, out var patterns))
            return url.TrimStart('@');
            
        var match = Regex.Match(url, patterns.UrlPattern, RegexOptions.IgnoreCase);
        if (match.Success && match.Groups.Count >= 2)
            return match.Groups[1].Value;
            
        // Might be just a handle
        return url.TrimStart('@');
    }
    
    /// <inheritdoc />
    public string? GenerateProfileUrl(string handle, SocialMediaPlatform platform)
    {
        if (string.IsNullOrWhiteSpace(handle))
            return null;
            
        handle = handle.TrimStart('@');
        
        if (!SocialMediaPatterns.TryGetValue(platform, out var patterns))
            return null;
            
        return patterns.BaseUrl + handle;
    }
    
    private static string? CheckDomainTypos(string domain)
    {
        var typos = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "gmial.com", "gmail.com" },
            { "gmal.com", "gmail.com" },
            { "gamil.com", "gmail.com" },
            { "gnail.com", "gmail.com" },
            { "hotmal.com", "hotmail.com" },
            { "hotmial.com", "hotmail.com" },
            { "hotamil.com", "hotmail.com" },
            { "outlok.com", "outlook.com" },
            { "outllok.com", "outlook.com" },
            { "yaho.com", "yahoo.com" },
            { "yahooo.com", "yahoo.com" },
            { "yhoo.com", "yahoo.com" },
        };
        
        return typos.TryGetValue(domain, out var correct) ? correct : null;
    }
    
    private static bool IsDisposableEmail(string domain)
    {
        // Common disposable email domains
        var disposable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mailinator.com", "guerrillamail.com", "tempmail.com", "10minutemail.com",
            "throwaway.email", "fakeinbox.com", "trashmail.com", "yopmail.com",
            "sharklasers.com", "guerrillamailblock.com", "temp-mail.org", "dispostable.com"
        };
        
        return disposable.Contains(domain);
    }
    
    private static async Task<bool> CheckMxRecordsAsync(string domain)
    {
        try
        {
            // Simple DNS check - try to resolve the domain
            var entry = await System.Net.Dns.GetHostEntryAsync(domain);
            return entry.AddressList.Length > 0;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
