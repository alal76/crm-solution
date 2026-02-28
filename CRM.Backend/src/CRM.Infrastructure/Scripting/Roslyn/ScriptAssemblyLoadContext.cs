// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Reflection;
using System.Runtime.Loader;

namespace CRM.Infrastructure.Scripting.Roslyn;

/// <summary>
/// Collectible <see cref="AssemblyLoadContext"/> that isolates compiled script assemblies.
/// Disposing (unloading) the context reclaims memory after script execution,
/// preventing long-running processes from accumulating JIT'd code.
/// </summary>
public class ScriptAssemblyLoadContext : AssemblyLoadContext
{
    public ScriptAssemblyLoadContext(string name) : base(name, isCollectible: true) { }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // Delegate to the default ALC — sandboxing is enforced at the Roslyn analyser level.
        return null;
    }
}
