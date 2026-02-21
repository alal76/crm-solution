// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
namespace CRM.Core.Dtos;

/// <summary>
/// Summary projection of a Lead — used in list and status-filtered responses.
/// </summary>
public class LeadSummaryDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? CompanyName { get; set; }
    public string? Title { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Score { get; set; }
    public int FitScore { get; set; }
    public int EngagementScore { get; set; }
    public int? OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Full detail projection of a Lead — used in single-record GET responses.
/// </summary>
public class LeadDto : LeadSummaryDto
{
    public string? QualificationNotes { get; set; }
    public string? Region { get; set; }
    public string? Website { get; set; }
    public string? Tags { get; set; }
    public int? AccountId { get; set; }
    public int? ContactId { get; set; }
    public int? CampaignId { get; set; }
    public DateTime? MqlDate { get; set; }
    public DateTime? SqlDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
}
