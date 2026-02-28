// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using CRM.Core.Scripting;

namespace CRM.Infrastructure.Scripting.Security;

/// <summary>
/// Security policy tiers for script execution.
/// SARCH-078: T3 — Data exfiltration prevention (all egress via Tool Bridge only)
/// SARCH-079: T6 — Resource abuse prevention (memory + CPU limits + concurrency ceiling)
/// SARCH-080: T8 — Script supply chain integrity (content hash verification)
/// </summary>
public static class ScriptSecurityPolicy
{
    // T3: Data exfiltration prevention
    // Enforced by ToolBridgeInvoker — scripts call tools, not external HTTP
    public static bool IsDataExfiltrationRisk(ScriptDefinition definition)
    {
        // Check for suspicious source patterns that bypass Tool Bridge
        var suspiciousPatterns = new[]
        {
            "System.Net.Http",
            "HttpClient",
            "WebClient",
            "Socket",
            "TcpClient",
        };

        foreach (var p in suspiciousPatterns)
        {
            if (definition.Source.Contains(p, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    // T6: Resource abuse prevention — returns enforcement config
    public static ResourceLimits GetResourceLimits(int memoryLimitMb, TimeSpan timeout)
        => new ResourceLimits(
            MemoryLimitBytes: (long)memoryLimitMb * 1024 * 1024,
            TimeoutMs: (long)timeout.TotalMilliseconds,
            MaxConcurrentExecutions: 10); // matches RoslynScriptEngine SemaphoreSlim

    // T8: Script supply chain integrity — verify content hash matches stored hash
    public static bool VerifyContentHash(ScriptDefinition definition, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;
        var bytes = System.Text.Encoding.UTF8.GetBytes(definition.Source);
        var computed = System.Security.Cryptography.SHA256.HashData(bytes);
        var computedHex = Convert.ToHexString(computed).ToLowerInvariant();
        return storedHash.Equals(computedHex, StringComparison.OrdinalIgnoreCase);
    }

    public static string ComputeContentHash(string source)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(source);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
    }
}

public record ResourceLimits(long MemoryLimitBytes, long TimeoutMs, int MaxConcurrentExecutions);
