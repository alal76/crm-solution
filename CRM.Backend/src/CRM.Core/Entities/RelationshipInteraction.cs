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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Entities;

/// <summary>
/// Health impact of an interaction
/// </summary>
public enum HealthImpact
{
    Positive = 0,
    Neutral = 1,
    Negative = 2,
    Critical = 3
}

/// <summary>
/// Tracks interactions and activities within a relationship
/// </summary>
public class RelationshipInteraction : BaseEntity
{
    /// <summary>
    /// The account relationship this interaction belongs to
    /// </summary>
    [Required]
    public int AccountRelationshipId { get; set; }

    /// <summary>
    /// Type of interaction (meeting, call, email, event, etc.)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string InteractionType { get; set; } = string.Empty;

    /// <summary>
    /// Subject/title of the interaction
    /// </summary>
    [MaxLength(255)]
    public string? Subject { get; set; }

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the interaction occurred
    /// </summary>
    [Required]
    public DateTime InteractionDate { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// JSON array of contact IDs involved
    /// </summary>
    public string? ParticipantContactIds { get; set; }

    /// <summary>
    /// JSON array of internal user IDs involved
    /// </summary>
    public string? ParticipantUserIds { get; set; }

    /// <summary>
    /// Outcome of the interaction
    /// </summary>
    [MaxLength(100)]
    public string? Outcome { get; set; }

    /// <summary>
    /// Action items from the interaction
    /// </summary>
    public string? ActionItems { get; set; }

    /// <summary>
    /// Next steps
    /// </summary>
    public string? NextSteps { get; set; }

    /// <summary>
    /// Follow up date if needed
    /// </summary>
    public DateTime? FollowUpDate { get; set; }

    /// <summary>
    /// Sentiment score (-100 to +100)
    /// </summary>
    public int SentimentScore { get; set; } = 0;

    /// <summary>
    /// Impact on relationship health
    /// </summary>
    [MaxLength(50)]
    public string HealthImpact { get; set; } = "Neutral";

    /// <summary>
    /// Location of the interaction
    /// </summary>
    [MaxLength(255)]
    public string? Location { get; set; }

    /// <summary>
    /// Meeting link if applicable
    /// </summary>
    [MaxLength(500)]
    public string? MeetingLink { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation property
    public virtual AccountRelationship? AccountRelationship { get; set; }
}
