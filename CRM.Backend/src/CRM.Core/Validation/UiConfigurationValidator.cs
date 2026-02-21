// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using CRM.Core.Entities;

namespace CRM.Core.Validation;

/// <summary>
/// Centralized validation helpers for UI configuration payloads.
/// </summary>
public static class UiConfigurationValidator
{
    private static readonly HashSet<string> DefaultModuleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Dashboard",
        ModuleNames.Accounts,
        ModuleNames.Contacts,
        ModuleNames.Leads,
        ModuleNames.Opportunities,
        ModuleNames.Products,
        "Services",
        "Campaigns",
        "Quotes",
        "Tasks",
        "Activities",
        "Notes",
        "Workflows",
        "Reports",
    };

    /// <summary>
    /// Validates a module name for UI configuration updates.
    /// </summary>
    public static void ValidateModuleName(string? moduleName, bool allowCustom = true)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name is required.", nameof(moduleName));
        }

        if (moduleName.Length > 100)
        {
            throw new ArgumentException("Module name is too long.", nameof(moduleName));
        }

        if (!allowCustom && !DefaultModuleNames.Contains(moduleName))
        {
            throw new ArgumentException($"Module name '{moduleName}' is not recognized.", nameof(moduleName));
        }
    }

    /// <summary>
    /// Validates a navigation key/id.
    /// </summary>
    public static void ValidateNavigationKey(string? key, string fieldName = "Navigation key")
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException($"{fieldName} is required.");
        }
    }

    /// <summary>
    /// Ensures keys are unique (case-insensitive).
    /// </summary>
    public static void EnsureUniqueKeys(IEnumerable<string> keys, string fieldName)
    {
        var duplicates = keys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .GroupBy(k => k, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicates.Count > 0)
        {
            throw new ArgumentException($"{fieldName} values must be unique. Duplicates: {string.Join(", ", duplicates)}");
        }
    }

    /// <summary>
    /// Ensures all orders are non-negative.
    /// </summary>
    public static void EnsureNonNegativeOrders(IEnumerable<(string Id, int Order)> items, string fieldName)
    {
        var invalid = items.Where(i => i.Order < 0).Select(i => i.Id).ToList();
        if (invalid.Count > 0)
        {
            throw new ArgumentException($"{fieldName} order must be non-negative. Invalid: {string.Join(", ", invalid)}");
        }
    }
}
