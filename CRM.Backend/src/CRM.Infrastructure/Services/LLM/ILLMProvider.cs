// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.Services.LLM;

/// <summary>
/// AP-036: Interface for individual LLM provider implementations.
/// Each provider handles API communication for a specific LLM backend.
/// </summary>
public interface ILLMProvider
{
    /// <summary>Primary provider identifier (e.g., "openai", "anthropic", "local")</summary>
    string ProviderName { get; }

    /// <summary>All aliases recognized as this provider (e.g., "ollama", "lmstudio" → "local")</summary>
    string[] SupportedAliases { get; }

    /// <summary>Execute an LLM chat request against this provider's API.</summary>
    Task<LLMResponse> CallAsync(LLMRequest request, CancellationToken cancellationToken = default);
}
