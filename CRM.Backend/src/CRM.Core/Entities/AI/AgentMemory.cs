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

namespace CRM.Core.Entities.AI;

#region Memory Enumerations

/// <summary>
/// Categorizes the type of memory stored by an AI agent.
/// </summary>
public enum MemoryType
{
    /// <summary>A factual piece of information about an entity</summary>
    Fact = 0,

    /// <summary>A user or entity preference</summary>
    Preference = 1,

    /// <summary>Contextual information from a conversation or interaction</summary>
    Context = 2,

    /// <summary>A vector embedding for semantic search</summary>
    Embedding = 3,

    /// <summary>A summarized version of longer content</summary>
    Summary = 4
}

#endregion

/// <summary>
/// Represents a long-term memory entry for an AI agent, enabling
/// persistent knowledge across conversations and sessions.
/// </summary>
public class AgentMemory : BaseEntity
{
    #region References

    /// <summary>
    /// Foreign key to the agent that owns this memory.
    /// </summary>
    public int AgentId { get; set; }

    /// <summary>
    /// Navigation property to the owning agent.
    /// </summary>
    public AIAgent? Agent { get; set; }

    #endregion

    #region Content

    /// <summary>
    /// The type of memory stored.
    /// </summary>
    public MemoryType MemoryType { get; set; }

    /// <summary>
    /// The key or label identifying this memory entry.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The value or content of this memory entry.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    #endregion

    #region Context

    /// <summary>
    /// Optional entity type this memory relates to (e.g., "Account", "Contact").
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Optional entity ID this memory relates to.
    /// </summary>
    public int? EntityId { get; set; }

    /// <summary>
    /// Confidence score for this memory entry (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; } = 1.0;

    #endregion

    #region Usage

    /// <summary>
    /// Number of times this memory has been accessed or referenced.
    /// </summary>
    public int AccessCount { get; set; } = 0;

    /// <summary>
    /// Timestamp of the last time this memory was accessed.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    /// Optional expiration date after which this memory should be discarded.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    #endregion
}
