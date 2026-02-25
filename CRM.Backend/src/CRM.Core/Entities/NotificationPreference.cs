// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Stores per-user notification preferences for specific event types/channels.
/// Implements TODO-PORTAL-07.
/// </summary>
public class NotificationPreference : BaseEntity
{
    /// <summary>ID of the user these preferences belong to.</summary>
    public int UserId { get; set; }

    /// <summary>Entity type this preference applies to (e.g. "Opportunity", "Account", "Task").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Event type within the entity (e.g. "StatusChanged", "Created", "Assigned").</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Delivery channel for the notification.</summary>
    public NotificationChannel Channel { get; set; } = NotificationChannel.InApp;

    /// <summary>Whether the notification is enabled for this channel.</summary>
    public bool IsEnabled { get; set; } = true;

    // Navigation
    public User? User { get; set; }
}

/// <summary>
/// Notification delivery channels.
/// </summary>
public enum NotificationChannel
{
    InApp = 0,
    Email = 1,
    Push = 2,
    Sms = 3
}
