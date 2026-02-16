using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CRM.Core.Dtos;

/// <summary>
/// Validates that a string value is a valid ISO 4217 currency code (3 uppercase letters).
/// Example: USD, EUR, GBP, CAD
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class CurrencyCodeAttribute : ValidationAttribute
{
    private static readonly Regex _currencyCodeRegex = new("^[A-Z]{3}$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyCodeAttribute"/> class.
    /// </summary>
    public CurrencyCodeAttribute()
    {
        ErrorMessage = "Currency code must be a valid 3-letter ISO 4217 code (e.g., USD, EUR, GBP).";
    }

    /// <summary>
    /// Validates the currency code.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string currencyCode)
            return false;

        return _currencyCodeRegex.IsMatch(currencyCode);
    }
}

/// <summary>
/// Validates that a string value is a valid phone number in E.164 format (+1234567890).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PhoneNumberAttribute : ValidationAttribute
{
    // E.164 format: +[1-9]{1}[0-9]{1,14}
    private static readonly Regex _e164Regex = new("^\\+[1-9]\\d{1,14}$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNumberAttribute"/> class.
    /// </summary>
    public PhoneNumberAttribute()
    {
        ErrorMessage = "Phone number must be in E.164 format (e.g., +14155552671, max 15 digits).";
    }

    /// <summary>
    /// Validates the phone number.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string phoneNumber)
            return false;

        // Allow flexible input - clean and validate
        var cleaned = System.Text.RegularExpressions.Regex.Replace(phoneNumber, @"[^\d+]", "");
        if (!cleaned.StartsWith("+"))
            cleaned = "+" + cleaned;

        return _e164Regex.IsMatch(cleaned);
    }
}

/// <summary>
/// Validates that a string value contains a valid email domain from a whitelist of allowed domains.
/// Useful for enterprise deployments that restrict email domains.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class EmailDomainAttribute : ValidationAttribute
{
    private readonly string[] _allowedDomains;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailDomainAttribute"/> class.
    /// </summary>
    /// <param name="allowedDomains">Comma-separated list of allowed email domains (e.g., "example.com,company.org").</param>
    public EmailDomainAttribute(string allowedDomains)
    {
        _allowedDomains = allowedDomains.Split(',')
            .Select(d => d.Trim().ToLowerInvariant())
            .Where(d => !string.IsNullOrEmpty(d))
            .ToArray();

        if (_allowedDomains.Length == 0)
            throw new ArgumentException("At least one allowed domain must be specified.", nameof(allowedDomains));

        ErrorMessage ??= $"Email domain must be one of: {string.Join(", ", _allowedDomains)}";
    }

    /// <summary>
    /// Validates the email domain.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string email)
            return false;

        var emailParts = email.Split('@');
        if (emailParts.Length != 2)
            return false;

        var domain = emailParts[1].ToLowerInvariant();
        return _allowedDomains.Contains(domain);
    }
}

/// <summary>
/// Validates that a decimal value has the correct precision and scale for database storage.
/// Example: DecimalPrecision(18, 4) means max 18 total digits with 4 decimal places ($999,999,999,999.9999).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DecimalPrecisionAttribute : ValidationAttribute
{
    private readonly byte _precision;
    private readonly byte _scale;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecimalPrecisionAttribute"/> class.
    /// </summary>
    /// <param name="precision">Total number of digits (default 18).</param>
    /// <param name="scale">Number of digits after decimal point (default 4).</param>
    public DecimalPrecisionAttribute(byte precision = 18, byte scale = 4)
    {
        if (precision < 1 || precision > 38)
            throw new ArgumentException("Precision must be between 1 and 38.", nameof(precision));

        if (scale < 0 || scale > precision)
            throw new ArgumentException($"Scale must be between 0 and {precision}.", nameof(scale));

        _precision = precision;
        _scale = scale;

        var maxValue = (decimal)Math.Pow(10, precision - scale) - (decimal)Math.Pow(10, -scale);
        string formatSpec = "F" + scale;
        ErrorMessage ??= $"Value must have at most {precision} total digits with {scale} decimal places.";
    }

    /// <summary>
    /// Validates the decimal precision and scale.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not decimal decimalValue)
            return false;

        var bits = decimal.GetBits(decimalValue);
        var sign = (bits[3] & 0x80000000) != 0;
        var scale = (bits[3] >> 16) & 0xFF;
        var integerBits = bits;

        // Count significant digits
        var absoluteValue = Math.Abs(decimalValue);
        var digitCount = absoluteValue == 0 ? 1 : (int)Math.Floor(Math.Log10(Math.Abs((double)absoluteValue))) + 1;

        // Validate scale
        if (scale > _scale)
            return false;

        // Validate total digits (precision)
        if (digitCount > _precision - _scale)
            return false;

        return true;
    }
}

/// <summary>
/// Validates that a string value represents a valid URL.
/// Supports HTTP, HTTPS, and optionally other schemes.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class UrlAttribute : ValidationAttribute
{
    private readonly string[] _allowedSchemes;

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlAttribute"/> class.
    /// </summary>
    /// <param name="allowedSchemes">Comma-separated list of allowed URL schemes (default: "http,https").</param>
    public UrlAttribute(string? allowedSchemes = null)
    {
        _allowedSchemes = (allowedSchemes ?? "http,https").Split(',')
            .Select(s => s.Trim().ToLowerInvariant())
            .ToArray();

        var schemes = string.Join(", ", _allowedSchemes);
        ErrorMessage ??= $"Value must be a valid URL with scheme: {schemes}";
    }

    /// <summary>
    /// Validates the URL.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string url)
            return false;

        return Uri.TryCreate(url, UriKind.Absolute, out var uri) 
            && _allowedSchemes.Contains(uri.Scheme.ToLowerInvariant());
    }
}

/// <summary>
/// Validates that a string value is a valid ISO 8601 date format (YYYY-MM-DD).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class IsoDateAttribute : ValidationAttribute
{
    private static readonly Regex _isoDateRegex = new(@"^\d{4}-\d{2}-\d{2}$", RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the <see cref="IsoDateAttribute"/> class.
    /// </summary>
    public IsoDateAttribute()
    {
        ErrorMessage = "Date must be in ISO 8601 format (YYYY-MM-DD, e.g., 2026-02-16).";
    }

    /// <summary>
    /// Validates the ISO date format.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not string dateString)
            return false;

        if (!_isoDateRegex.IsMatch(dateString))
            return false;

        return DateTime.TryParseExact(dateString, "yyyy-MM-dd", 
            System.Globalization.CultureInfo.InvariantCulture, 
            System.Globalization.DateTimeStyles.None, out _);
    }
}

/// <summary>
/// Validates that a string value is a valid enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class ValidEnumAttribute : ValidationAttribute
{
    private readonly Type _enumType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidEnumAttribute"/> class.
    /// </summary>
    /// <param name="enumType">The enum type to validate against.</param>
    public ValidEnumAttribute(Type enumType)
    {
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum.", nameof(enumType));

        _enumType = enumType;
        ErrorMessage ??= $"Value must be a valid {enumType.Name}.";
    }

    /// <summary>
    /// Validates the enum value.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is string stringValue)
        {
            try
            {
                Enum.Parse(_enumType, stringValue, ignoreCase: true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        return Enum.IsDefined(_enumType, value);
    }
}

/// <summary>
/// Validates that a numeric value representing a percentage is between 0 and 100.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PercentageAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PercentageAttribute"/> class.
    /// </summary>
    public PercentageAttribute()
    {
        ErrorMessage = "Value must be a percentage between 0 and 100.";
    }

    /// <summary>
    /// Validates the percentage value.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is decimal decimalValue)
            return decimalValue >= 0 && decimalValue <= 100;

        if (value is double doubleValue)
            return doubleValue >= 0 && doubleValue <= 100;

        if (value is float floatValue)
            return floatValue >= 0 && floatValue <= 100;

        if (value is int intValue)
            return intValue >= 0 && intValue <= 100;

        return false;
    }
}

/// <summary>
/// Validates that a string value is not empty or whitespace only (stronger than [Required]).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class NotBlankAttribute : ValidationAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotBlankAttribute"/> class.
    /// </summary>
    public NotBlankAttribute()
    {
        ErrorMessage = "This field is required and cannot be empty or whitespace.";
    }

    /// <summary>
    /// Validates that the string is not blank.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return false;

        if (value is not string stringValue)
            return true;

        return !string.IsNullOrWhiteSpace(stringValue);
    }
}

/// <summary>
/// Validates that a date value is within a specific range relative to today.
/// Useful for validating future dates, past dates, or date ranges.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DateRangeAttribute : ValidationAttribute
{
    private readonly int? _minDaysFromToday;
    private readonly int? _maxDaysFromToday;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateRangeAttribute"/> class.
    /// </summary>
    /// <param name="minDaysFromToday">Minimum days from today (negative for past, positive for future). Null for no limit.</param>
    /// <param name="maxDaysFromToday">Maximum days from today (negative for past, positive for future). Null for no limit.</param>
    public DateRangeAttribute(int? minDaysFromToday = null, int? maxDaysFromToday = null)
    {
        _minDaysFromToday = minDaysFromToday;
        _maxDaysFromToday = maxDaysFromToday;

        if (minDaysFromToday.HasValue && maxDaysFromToday.HasValue && minDaysFromToday > maxDaysFromToday)
            throw new ArgumentException("MinDaysFromToday must be less than or equal to MaxDaysFromToday.", nameof(minDaysFromToday));

        ErrorMessage ??= BuildErrorMessage();
    }

    /// <summary>
    /// Validates the date range.
    /// </summary>
    public override bool IsValid(object? value)
    {
        if (value == null)
            return true;

        if (value is not DateTime dateValue)
            return false;

        var today = DateTime.Today;
        var daysFromToday = (dateValue.Date - today).Days;

        if (_minDaysFromToday.HasValue && daysFromToday < _minDaysFromToday.Value)
            return false;

        if (_maxDaysFromToday.HasValue && daysFromToday > _maxDaysFromToday.Value)
            return false;

        return true;
    }

    private string BuildErrorMessage()
    {
        if (_minDaysFromToday.HasValue && _maxDaysFromToday.HasValue)
            return $"Date must be between {_minDaysFromToday} and {_maxDaysFromToday} days from today.";

        if (_minDaysFromToday.HasValue)
            return $"Date must be at least {_minDaysFromToday} days from today.";

        if (_maxDaysFromToday.HasValue)
            return $"Date must be at most {_maxDaysFromToday} days from today.";

        return "Invalid date range specified.";
    }
}
