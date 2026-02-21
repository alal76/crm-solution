// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.AI.SK.Memory;

#region MemoryCollections

/// <summary>
/// Defines constant collection names for the Semantic Kernel vector store.
/// These correspond to Qdrant (or in-memory) collections used for CRM entity embeddings.
/// </summary>
public static class MemoryCollections
{
    /// <summary>
    /// Collection name for account / customer embeddings.
    /// </summary>
    public const string Accounts = "crm-accounts";

    /// <summary>
    /// Collection name for contact embeddings.
    /// </summary>
    public const string Contacts = "crm-contacts";

    /// <summary>
    /// Collection name for knowledge base article embeddings.
    /// </summary>
    public const string KBArticles = "crm-kb-articles";

    /// <summary>
    /// Collection name for email embeddings.
    /// </summary>
    public const string Emails = "crm-emails";

    /// <summary>
    /// Collection name for conversation embeddings.
    /// </summary>
    public const string Conversations = "crm-conversations";

    /// <summary>
    /// Collection name for agent long-term memory embeddings.
    /// </summary>
    public const string AgentMemory = "crm-agent-memory";

    /// <summary>
    /// Array of all defined collection names, useful for initialization and health checks.
    /// </summary>
    public static readonly string[] All =
    {
        Accounts,
        Contacts,
        KBArticles,
        Emails,
        Conversations,
        AgentMemory,
    };

    /// <summary>
    /// Default embedding vector dimension (matches OpenAI text-embedding-3-small).
    /// </summary>
    public const int DefaultEmbeddingDimension = 1536;
}

#endregion
