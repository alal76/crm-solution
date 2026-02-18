// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Entities;

/// <summary>
/// Sales team for quota and forecast rollups.
/// </summary>
public class Team : BaseEntity
{
    /// <summary>Team name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Team code</summary>
    public string? Code { get; set; }

    /// <summary>Team description</summary>
    public string? Description { get; set; }

    /// <summary>Whether team is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Team manager user ID</summary>
    public int? ManagerId { get; set; }

    /// <summary>Navigation to manager</summary>
    public User? Manager { get; set; }

    /// <summary>Parent team ID</summary>
    public int? ParentTeamId { get; set; }

    /// <summary>Navigation to parent team</summary>
    public Team? ParentTeam { get; set; }

    /// <summary>Child teams</summary>
    public ICollection<Team> ChildTeams { get; set; } = new List<Team>();

    /// <summary>Team members</summary>
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    /// <summary>Team quotas</summary>
    public ICollection<SalesQuota> Quotas { get; set; } = new List<SalesQuota>();

    /// <summary>Team forecasts</summary>
    public ICollection<SalesForecast> Forecasts { get; set; } = new List<SalesForecast>();
}

/// <summary>
/// Team member association.
/// </summary>
public class TeamMember : BaseEntity
{
    /// <summary>Team ID</summary>
    public int TeamId { get; set; }

    /// <summary>Navigation to team</summary>
    public Team? Team { get; set; }

    /// <summary>User ID</summary>
    public int UserId { get; set; }

    /// <summary>Navigation to user</summary>
    public User? User { get; set; }

    /// <summary>Role in team</summary>
    public string? Role { get; set; }

    /// <summary>Whether user is team lead</summary>
    public bool IsTeamLead { get; set; } = false;

    /// <summary>Start date</summary>
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    /// <summary>End date</summary>
    public DateTime? EndDate { get; set; }
}
