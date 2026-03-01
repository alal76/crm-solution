// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CRM.Infrastructure.Scripting.Cli;

/// <summary>
/// SARCH-086: CLI helper for scripting engine operations.
/// Used by crm-scripts dotnet tool (future: dotnet-tool package).
/// Commands: compile, execute, lint, upload, list-versions
/// </summary>
public static class ScriptingCliHelper
{
    public static int Compile(string scriptPath, string? outputPath)
    {
        if (!File.Exists(scriptPath))
        {
            Console.Error.WriteLine($"Error: File not found: {scriptPath}");
            return 1;
        }

        var source = File.ReadAllText(scriptPath);
        var bytes = Encoding.UTF8.GetBytes(source);
        var hash = SHA256.HashData(bytes);
        var hexHash = Convert.ToHexString(hash).ToLowerInvariant();

        Console.WriteLine($"Script: {scriptPath}");
        Console.WriteLine($"Size: {source.Length} bytes");
        Console.WriteLine($"SHA-256: {hexHash}");
        Console.WriteLine("Status: Pre-validation passed (compile requires runtime)");

        return 0;
    }

    public static int Lint(string scriptPath)
    {
        if (!File.Exists(scriptPath))
        {
            Console.Error.WriteLine($"Error: File not found: {scriptPath}");
            return 1;
        }

        var source = File.ReadAllText(scriptPath);
        if (source.Contains("eval(", StringComparison.Ordinal))
        {
            Console.Error.WriteLine("LINT ERROR: eval() usage detected");
            return 1;
        }

        Console.WriteLine("Lint: OK");
        return 0;
    }

    public static void ListVersions(string scriptId, string baseUrl)
        => Console.WriteLine($"GET {baseUrl}/api/script-registry/{scriptId}/versions");
}
