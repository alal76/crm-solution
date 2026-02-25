// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Enums;

/// <summary>
/// Schema version for report query payloads (TODO-AI005-FE-002).
/// Used to support backward-compatible evolution of report query structures.
/// </summary>
public enum ReportQuerySchemaVersion
{
    /// <summary>
    /// Original schema — flat filter fields on ReportParametersDto.
    /// </summary>
    V1 = 1,

    /// <summary>
    /// Current schema — structured filter array with typed operators.
    /// Adds FilterGroups, SortDescriptors, and ColumnVisibility.
    /// </summary>
    V2 = 2,
}
