// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CRM.Core.Scripting;

/// <summary>Attribute that marks a class as a registered script tool.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ScriptToolAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }
    public string[] RequiredPermissions { get; }

    public ScriptToolAttribute(string name, string description, params string[] requiredPermissions)
    {
        Name = name;
        Description = description;
        RequiredPermissions = requiredPermissions;
    }
}

/// <summary>Descriptor for a registered script tool.</summary>
public record ToolDescriptor(
    string Name,
    string Description,
    string[] RequiredPermissions,
    Type ImplementationType,
    MethodInfo InvokeMethod);

/// <summary>
/// Registry of all platform tools available to scripts via the Tool Bridge.
/// Tools are registered by DI and discovered via <see cref="ScriptToolAttribute"/>.
/// </summary>
public class ToolRegistry
{
    private readonly ConcurrentDictionary<string, ToolDescriptor> _tools =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers a tool descriptor, overwriting any existing registration with the same name.</summary>
    public void Register(ToolDescriptor descriptor)
        => _tools[descriptor.Name] = descriptor;

    /// <summary>Attempts to retrieve a tool descriptor by name (case-insensitive).</summary>
    public bool TryGet(string name, out ToolDescriptor? descriptor)
        => _tools.TryGetValue(name, out descriptor);

    /// <summary>Returns all registered tool descriptors.</summary>
    public IReadOnlyCollection<ToolDescriptor> GetAll()
        => _tools.Values.ToList().AsReadOnly();

    /// <summary>Scans <paramref name="assembly"/> for classes annotated with <see cref="ScriptToolAttribute"/> and registers them.</summary>
    public void DiscoverFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<ScriptToolAttribute>();
            if (attr == null) continue;

            var invokeMethod = type.GetMethod("InvokeAsync")
                ?? type.GetMethod("Execute")
                ?? type.GetMethod("RunAsync");

            if (invokeMethod == null) continue;

            Register(new ToolDescriptor(
                attr.Name,
                attr.Description,
                attr.RequiredPermissions,
                type,
                invokeMethod));
        }
    }
}
