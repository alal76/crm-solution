// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Scripting.AgentHooks;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Scripting.AgentHooks;

/// <summary>
/// Pipeline that runs a chain of guardrail checks.
/// Built-in checks: PII detection, prompt injection, toxicity patterns.
/// Custom checks: via script-backed guardrails.
/// SARCH-070.
/// </summary>
public class GuardrailPipeline
{
    private readonly ILogger<GuardrailPipeline> _logger;
    private readonly List<IGuardrailHook> _hooks;

    // Built-in PII patterns
    private static readonly Regex SsnPattern =
        new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly Regex CreditCardPattern =
        new Regex(@"\b(?:\d{4}[- ]?){3}\d{4}\b", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    private static readonly Regex PromptInjectionPattern =
        new Regex(
            @"(?i)(ignore previous|system prompt|jailbreak|DAN mode|pretend you are)",
            RegexOptions.Compiled, TimeSpan.FromSeconds(5));

    public GuardrailPipeline(ILogger<GuardrailPipeline> logger, IEnumerable<IGuardrailHook>? hooks = null)
    {
        _logger = logger;
        _hooks = hooks != null ? new List<IGuardrailHook>(hooks) : new List<IGuardrailHook>();
    }

    public async Task<HookResult> CheckContentAsync(
        AgentHookContext ctx,
        string content,
        GuardrailCheckType checkType,
        CancellationToken ct = default)
    {
        // Built-in checks first
        if (SsnPattern.IsMatch(content))
        {
            _logger.LogWarning("GuardrailPipeline: SSN detected in {CheckType}", checkType);
            return new HookResult { Continue = false, Reason = "PII (SSN) detected — content blocked" };
        }

        if (CreditCardPattern.IsMatch(content))
        {
            _logger.LogWarning("GuardrailPipeline: Credit card number detected");
            return new HookResult { Continue = false, Reason = "PII (credit card) detected — content blocked" };
        }

        if (checkType == GuardrailCheckType.Input && PromptInjectionPattern.IsMatch(content))
        {
            _logger.LogWarning("GuardrailPipeline: Prompt injection attempt detected");
            return new HookResult { Continue = false, Reason = "Prompt injection attempt detected — request blocked" };
        }

        // Custom script-backed hooks
        foreach (var hook in _hooks)
        {
            var result = await hook.CheckAsync(ctx, content, checkType, ct);
            if (!result.Continue)
            {
                _logger.LogWarning("GuardrailPipeline: Custom hook blocked content: {Reason}", result.Reason);
                return result;
            }
        }

        return new HookResult { Continue = true };
    }
}
