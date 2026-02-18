// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.RegularExpressions;

namespace CRM.Infrastructure.Validation;

public static class ThemeColorValidator
{
    private static readonly Regex HexColorRegex = new("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$", RegexOptions.Compiled);

    public static void ValidateHexColor(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (!HexColorRegex.IsMatch(value.Trim()))
        {
            throw new ArgumentException($"Invalid {fieldName} color format. Use hex like #6750A4.", fieldName);
        }
    }
}
