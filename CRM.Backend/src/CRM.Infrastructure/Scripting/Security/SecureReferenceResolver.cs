// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;

namespace CRM.Infrastructure.Scripting.Security;

/// <summary>
/// Restricts which assemblies a Roslyn script can reference.
/// Only assemblies in the allow-list can be resolved;
/// all others are silently denied so compilation fails rather than
/// loading untrusted code.
/// </summary>
public class SecureReferenceResolver : MetadataReferenceResolver
{
    private static readonly ImmutableHashSet<string> AllowedAssemblyNames =
        ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
            "System.Private.CoreLib",
            "System.Runtime",
            "System.Linq",
            "System.Text.Json",
            "System.Collections",
            "CRM.Core",
            "Newtonsoft.Json");

    public override bool Equals(object? other) => other is SecureReferenceResolver;
    public override int GetHashCode() => typeof(SecureReferenceResolver).GetHashCode();

    public override ImmutableArray<PortableExecutableReference> ResolveReference(
        string reference,
        string? baseFilePath,
        MetadataReferenceProperties properties)
    {
        var assemblyName = Path.GetFileNameWithoutExtension(reference);
        if (!AllowedAssemblyNames.Contains(assemblyName))
        {
            return ImmutableArray<PortableExecutableReference>.Empty;
        }

        try
        {
            var assembly = Assembly.Load(assemblyName);
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location))
            {
                return ImmutableArray.Create(MetadataReference.CreateFromFile(location));
            }
        }
        catch
        {
            // Silently deny — malformed or missing assembly
        }

        return ImmutableArray<PortableExecutableReference>.Empty;
    }
}
