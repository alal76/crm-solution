using System;

namespace CRM.Core.Entities;

/// <summary>
/// Template for generating notifications across channels (email, push, in-app).
/// </summary>
public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Channel { get; set; } = "InApp";
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? EventTrigger { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Variables { get; set; }
}
