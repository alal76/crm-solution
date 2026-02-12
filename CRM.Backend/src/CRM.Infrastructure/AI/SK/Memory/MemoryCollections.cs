// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
