// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.RegularExpressions;
using CRM.Core.Ports.Input;

// Alias to disambiguate from CRM.Core.Interfaces.ValidationResult
using DataValidationResult = CRM.Core.Ports.Input.ValidationResult;

// Alias to disambiguate FieldValidationError from CRM.Core.Interfaces.FieldValidationError
using FieldValidationError = CRM.Core.Ports.Input.FieldValidationError;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Validates import records against entity-specific rules for accounts, contacts,
/// leads, and opportunities.
/// </summary>
public sealed partial class DataValidatorService : IDataValidator
{
    // Email regex (RFC-5322 simplified, same pattern used in other CRM services)
    [GeneratedRegex(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    // Phone: allows digits, spaces, +, -, (, )  — at least 6 chars
    [GeneratedRegex(@"^[\d\s\+\-\(\)]{6,}$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    // Date: ISO 8601 or common locale formats (YYYY-MM-DD, MM/DD/YYYY, DD-MM-YYYY)
    [GeneratedRegex(
        @"^\d{4}-\d{2}-\d{2}$|^\d{1,2}[/\-]\d{1,2}[/\-]\d{2,4}$",
        RegexOptions.Compiled)]
    private static partial Regex DateRegex();

    // -------------------------------------------------------------------------
    // Validation rule definitions
    // -------------------------------------------------------------------------

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<FieldValidationRule>> _rulesByEntity =
        new Dictionary<string, IReadOnlyList<FieldValidationRule>>(StringComparer.OrdinalIgnoreCase)
        {
            ["accounts"] =
            [
                new FieldValidationRule { Field = "Name",        Required = true,  MaxLength = 200 },
                new FieldValidationRule { Field = "Industry",    Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Website",     Required = false, MaxLength = 500 },
                new FieldValidationRule { Field = "Phone",       Required = false, Format = "phone", MaxLength = 50 },
                new FieldValidationRule { Field = "Email",       Required = false, Format = "email", MaxLength = 200 },
                new FieldValidationRule { Field = "Address",     Required = false, MaxLength = 300 },
                new FieldValidationRule { Field = "City",        Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "State",       Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Country",     Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "PostalCode",  Required = false, MaxLength = 20 },
            ],
            ["contacts"] =
            [
                new FieldValidationRule { Field = "FirstName",   Required = true,  MaxLength = 100 },
                new FieldValidationRule { Field = "LastName",    Required = true,  MaxLength = 100 },
                new FieldValidationRule { Field = "Email",       Required = true,  Format = "email", MaxLength = 200 },
                new FieldValidationRule { Field = "Phone",       Required = false, Format = "phone", MaxLength = 50  },
                new FieldValidationRule { Field = "Title",       Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Department",  Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "AccountName", Required = false, MaxLength = 200 },
                new FieldValidationRule { Field = "Address",     Required = false, MaxLength = 300 },
                new FieldValidationRule { Field = "City",        Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "State",       Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Country",     Required = false, MaxLength = 100 },
            ],
            ["leads"] =
            [
                new FieldValidationRule { Field = "FirstName",   Required = true,  MaxLength = 100 },
                new FieldValidationRule { Field = "LastName",    Required = true,  MaxLength = 100 },
                new FieldValidationRule { Field = "Email",       Required = true,  Format = "email", MaxLength = 200 },
                new FieldValidationRule { Field = "Phone",       Required = false, Format = "phone", MaxLength = 50  },
                new FieldValidationRule { Field = "Company",     Required = false, MaxLength = 200 },
                new FieldValidationRule { Field = "Title",       Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Source",      Required = false, MaxLength = 100 },
                new FieldValidationRule { Field = "Status",      Required = false, MaxLength = 50  },
                new FieldValidationRule { Field = "Industry",    Required = false, MaxLength = 100 },
            ],
            ["opportunities"] =
            [
                new FieldValidationRule { Field = "Name",        Required = true,  MaxLength = 200 },
                new FieldValidationRule { Field = "AccountName", Required = false, MaxLength = 200 },
                new FieldValidationRule { Field = "Stage",       Required = true,  MaxLength = 100 },
                new FieldValidationRule { Field = "Amount",      Required = false, Format = null   },
                new FieldValidationRule { Field = "CloseDate",   Required = true,  Format = "date" },
                new FieldValidationRule { Field = "Probability", Required = false, Format = null   },
                new FieldValidationRule { Field = "Description", Required = false, MaxLength = 2000 },
                new FieldValidationRule { Field = "Type",        Required = false, MaxLength = 100  },
            ],
        };

    // -------------------------------------------------------------------------
    // IDataValidator implementation
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public IEnumerable<FieldValidationRule> GetValidationRules(string entityType) =>
        _rulesByEntity.TryGetValue(entityType, out var rules)
            ? rules
            : [];

    /// <inheritdoc />
    public Task<DataValidationResult> ValidateRecordAsync(
        string entityType,
        Dictionary<string, object?> record,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(ValidateRecord(entityType, record, rowNumber: null));
    }

    /// <inheritdoc />
    public Task<IEnumerable<DataValidationResult>> ValidateBatchAsync(
        string entityType,
        IEnumerable<Dictionary<string, object?>> records,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var results = new List<DataValidationResult>();
        var rowNumber = 1;
        foreach (var record in records)
        {
            ct.ThrowIfCancellationRequested();
            results.Add(ValidateRecord(entityType, record, rowNumber));
            rowNumber++;
        }
        return Task.FromResult<IEnumerable<DataValidationResult>>(results);
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private DataValidationResult ValidateRecord(
        string entityType,
        Dictionary<string, object?> record,
        int? rowNumber)
    {
        var errors = new List<FieldValidationError>();

        if (!_rulesByEntity.TryGetValue(entityType, out var rules))
        {
            // Unknown entity: return a single warning rather than crashing
            errors.Add(new FieldValidationError
            {
                Field = "*",
                Message = $"No validation rules defined for entity type '{entityType}'.",
                Code = "UNKNOWN_ENTITY",
            });
            return new DataValidationResult { IsValid = false, RowNumber = rowNumber, Errors = errors };
        }

        foreach (var rule in rules)
        {
            record.TryGetValue(rule.Field, out var rawValue);
            var stringValue = rawValue?.ToString()?.Trim() ?? string.Empty;

            // Required check
            if (rule.Required && string.IsNullOrWhiteSpace(stringValue))
            {
                errors.Add(new FieldValidationError
                {
                    Field = rule.Field,
                    Message = $"'{rule.Field}' is required.",
                    Code = "REQUIRED",
                });
                continue; // Skip further checks on this field
            }

            // Skip format / length checks when value is empty and field is optional
            if (string.IsNullOrWhiteSpace(stringValue))
                continue;

            // Format checks
            if (!string.IsNullOrEmpty(rule.Format))
            {
                switch (rule.Format.ToLowerInvariant())
                {
                    case "email":
                        if (!EmailRegex().IsMatch(stringValue))
                            errors.Add(new FieldValidationError
                            {
                                Field = rule.Field,
                                Message = $"'{rule.Field}' must be a valid email address.",
                                Code = "INVALID_EMAIL",
                            });
                        break;

                    case "phone":
                        if (!PhoneRegex().IsMatch(stringValue))
                            errors.Add(new FieldValidationError
                            {
                                Field = rule.Field,
                                Message = $"'{rule.Field}' must be a valid phone number.",
                                Code = "INVALID_PHONE",
                            });
                        break;

                    case "date":
                        if (!DateRegex().IsMatch(stringValue) &&
                            !DateTime.TryParse(stringValue, out _))
                        {
                            errors.Add(new FieldValidationError
                            {
                                Field = rule.Field,
                                Message = $"'{rule.Field}' must be a valid date (e.g. YYYY-MM-DD).",
                                Code = "INVALID_DATE",
                            });
                        }
                        break;
                }
            }

            // MaxLength check
            if (rule.MaxLength.HasValue && stringValue.Length > rule.MaxLength.Value)
            {
                errors.Add(new FieldValidationError
                {
                    Field = rule.Field,
                    Message = $"'{rule.Field}' exceeds maximum length of {rule.MaxLength.Value} characters.",
                    Code = "TOO_LONG",
                });
            }

            // MinLength check
            if (rule.MinLength.HasValue && stringValue.Length < rule.MinLength.Value)
            {
                errors.Add(new FieldValidationError
                {
                    Field = rule.Field,
                    Message = $"'{rule.Field}' must be at least {rule.MinLength.Value} characters.",
                    Code = "TOO_SHORT",
                });
            }
        }

        return new DataValidationResult
        {
            IsValid = errors.Count == 0,
            RowNumber = rowNumber,
            Errors = errors,
        };
    }
}
