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
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Represents an analytics event for tracking user actions and system events.
/// Used for business intelligence, user behavior analysis, and audit purposes.
/// </summary>
public class AnalyticsEvent : BaseEntity
{
    /// <summary>
    /// Name of the event (e.g., "QuoteCreated", "OrderSubmitted", "PaymentReceived").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity the event relates to (e.g., "Quote", "Order", "Invoice").
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID of the related entity.
    /// </summary>
    public int EntityId { get; set; }

    /// <summary>
    /// ID of the user who triggered the event.
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public virtual User? User { get; set; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// JSON metadata containing additional event-specific information.
    /// </summary>
    [Column(TypeName = "json")]
    public string? Metadata { get; set; }
}
