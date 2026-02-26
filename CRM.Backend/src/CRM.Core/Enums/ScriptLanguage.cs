// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Core.Enums;

/// <summary>
/// Supported scripting languages for workflow Script nodes and agent script plugins.
/// </summary>
public enum ScriptLanguage
{
    /// <summary>JavaScript executed via the Jint engine (default)</summary>
    JavaScript = 0,

    /// <summary>Python executed via Python.NET with RestrictedPython sandbox</summary>
    Python = 1,

    /// <summary>C# scripting (reserved for future developer tooling)</summary>
    CSharp = 2
}
