// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
namespace CRM.Infrastructure.AI.SK.Configuration;

#region SemanticKernelOptions

/// <summary>
/// Root options for Semantic Kernel configuration, bound from the "SemanticKernel" section in appsettings.json.
/// </summary>
public class SemanticKernelOptions
{
    /// <summary>
    /// The configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "SemanticKernel";

    /// <summary>
    /// Gets or sets whether the Semantic Kernel integration is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the vector store configuration.
    /// </summary>
    public VectorStoreOptions VectorStore { get; set; } = new();

    /// <summary>
    /// Gets or sets the agent configuration.
    /// </summary>
    public AgentOptions Agents { get; set; } = new();

    /// <summary>
    /// Gets or sets the model configuration.
    /// </summary>
    public ModelOptions Models { get; set; } = new();
}

#endregion

#region VectorStoreOptions

/// <summary>
/// Configuration options for the vector store used by Semantic Kernel memory.
/// </summary>
public class VectorStoreOptions
{
    /// <summary>
    /// Gets or sets the vector store provider name (e.g., "Qdrant", "InMemory").
    /// </summary>
    public string Provider { get; set; } = "Qdrant";

    /// <summary>
    /// Gets or sets the Qdrant-specific configuration.
    /// </summary>
    public QdrantOptions Qdrant { get; set; } = new();

    /// <summary>
    /// Gets or sets the in-memory vector store configuration.
    /// </summary>
    public InMemoryOptions InMemory { get; set; } = new();

    /// <summary>
    /// Gets or sets the embedding dimension used for vector representations.
    /// </summary>
    public int EmbeddingDimension { get; set; } = 1536;

    /// <summary>
    /// Gets or sets the collection name mappings.
    /// </summary>
    public CollectionOptions Collections { get; set; } = new();
}

/// <summary>
/// Configuration options specific to the Qdrant vector database.
/// </summary>
public class QdrantOptions
{
    /// <summary>
    /// Gets or sets the Qdrant server hostname.
    /// </summary>
    public string Host { get; set; } = "crm-qdrant";

    /// <summary>
    /// Gets or sets the Qdrant gRPC port.
    /// </summary>
    public int Port { get; set; } = 6334;

    /// <summary>
    /// Gets or sets the Qdrant API key for authentication.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether TLS is used for the Qdrant connection.
    /// </summary>
    public bool UseTls { get; set; } = false;
}

/// <summary>
/// Configuration options for the in-memory vector store (used for development/testing).
/// </summary>
public class InMemoryOptions
{
    /// <summary>
    /// Gets or sets whether the in-memory vector store is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// Configuration for vector store collection names used by the CRM system.
/// </summary>
public class CollectionOptions
{
    /// <summary>
    /// Gets or sets the collection name for account embeddings.
    /// </summary>
    public string Accounts { get; set; } = "crm-accounts";

    /// <summary>
    /// Gets or sets the collection name for contact embeddings.
    /// </summary>
    public string Contacts { get; set; } = "crm-contacts";

    /// <summary>
    /// Gets or sets the collection name for knowledge base article embeddings.
    /// </summary>
    public string KBArticles { get; set; } = "crm-kb-articles";

    /// <summary>
    /// Gets or sets the collection name for email embeddings.
    /// </summary>
    public string Emails { get; set; } = "crm-emails";

    /// <summary>
    /// Gets or sets the collection name for conversation embeddings.
    /// </summary>
    public string Conversations { get; set; } = "crm-conversations";

    /// <summary>
    /// Gets or sets the collection name for agent memory embeddings.
    /// </summary>
    public string AgentMemory { get; set; } = "crm-agent-memory";
}

#endregion

#region AgentOptions

/// <summary>
/// Configuration options for AI agent behavior and resource limits.
/// </summary>
public class AgentOptions
{
    /// <summary>
    /// Gets or sets the maximum number of concurrent agent conversations.
    /// </summary>
    public int MaxConcurrentConversations { get; set; } = 100;

    /// <summary>
    /// Gets or sets the default temperature for LLM completions (0.0 = deterministic, 1.0 = creative).
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.3;

    /// <summary>
    /// Gets or sets the default maximum tokens for LLM completions.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 4096;

    /// <summary>
    /// Gets or sets the timeout in minutes for pending approval requests.
    /// </summary>
    public int ApprovalTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of messages to retain in conversation history.
    /// </summary>
    public int ConversationHistoryLimit { get; set; } = 50;

    /// <summary>
    /// Gets or sets the number of top results to retrieve from memory searches.
    /// </summary>
    public int MemorySearchTopK { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum similarity score (0.0–1.0) for memory search results.
    /// </summary>
    public double MemoryMinSimilarity { get; set; } = 0.75;

    /// <summary>
    /// Gets or sets the daily cost budget in dollars for AI operations.
    /// </summary>
    public decimal CostBudgetPerDay { get; set; } = 50.00m;

    /// <summary>
    /// Gets or sets the per-conversation cost budget in dollars.
    /// </summary>
    public decimal CostBudgetPerConversation { get; set; } = 0.50m;

    /// <summary>
    /// Gets or sets whether streaming responses are enabled for agent completions.
    /// </summary>
    public bool EnableStreaming { get; set; } = true;

    /// <summary>
    /// Gets or sets the interval in hours for background scoring jobs.
    /// </summary>
    public int BackgroundScoringIntervalHours { get; set; } = 6;

    /// <summary>
    /// Gets or sets the batch size for memory seeding operations.
    /// </summary>
    public int MemorySeedingBatchSize { get; set; } = 100;
}

#endregion

#region ModelOptions

/// <summary>
/// Configuration options for model selection across different AI operations.
/// </summary>
public class ModelOptions
{
    /// <summary>
    /// Gets or sets the default model used for general agent completions.
    /// </summary>
    public string Default { get; set; } = "gpt-4o";

    /// <summary>
    /// Gets or sets the model used for scoring and classification tasks.
    /// </summary>
    public string Scoring { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// Gets or sets the model used for generating text embeddings.
    /// </summary>
    public string Embedding { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// Gets or sets the model used for large context window operations.
    /// </summary>
    public string LargeContext { get; set; } = "gpt-4o";
}

#endregion
