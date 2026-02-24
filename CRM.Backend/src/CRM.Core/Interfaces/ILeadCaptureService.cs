// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for web-to-lead form capture service.
/// TODO-CRM002-04: Web-to-lead form builder integration.
/// </summary>
public interface ILeadCaptureService
{
    Task<FormTokenResult> GenerateFormTokenAsync(
        string formName,
        int? campaignId = null,
        int expiresInHours = 24,
        CancellationToken ct = default);

    Task<bool> ValidateFormTokenAsync(string token, CancellationToken ct = default);

    Task<LeadCaptureResult> CaptureLeadFromFormAsync(
        LeadCaptureRequest request,
        CancellationToken ct = default);

    Task<IEnumerable<FormTokenInfo>> GetActiveTokensAsync(CancellationToken ct = default);

    Task RevokeTokenAsync(string token, CancellationToken ct = default);
}

/// <summary>
/// Result of generating a form token.
/// </summary>
public class FormTokenResult
{
    public string Token { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public int? CampaignId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string EmbedCode { get; set; } = string.Empty;
}

/// <summary>
/// Request for capturing a lead from a form submission.
/// </summary>
public class LeadCaptureRequest
{
    public string Token { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Company { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Source { get; set; }
    public string? Website { get; set; }
    public string? UtmSource { get; set; }
    public string? UtmMedium { get; set; }
    public string? UtmCampaign { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

/// <summary>
/// Result of a lead capture operation.
/// </summary>
public class LeadCaptureResult
{
    public bool Success { get; set; }
    public int? LeadId { get; set; }
    public bool IsDuplicate { get; set; }
    public int? ExistingLeadId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about an active form token.
/// </summary>
public class FormTokenInfo
{
    public string Token { get; set; } = string.Empty;
    public string FormName { get; set; } = string.Empty;
    public int? CampaignId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public int SubmissionCount { get; set; }
}
