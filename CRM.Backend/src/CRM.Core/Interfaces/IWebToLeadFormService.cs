// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;

namespace CRM.Core.Interfaces;

/// <summary>
/// DTO for web-to-lead form submission
/// </summary>
public class WebToLeadSubmissionDto
{
    public string FormEmbedKey { get; set; } = string.Empty;
    public Dictionary<string, string> FieldValues { get; set; } = new();
    public string? CaptchaToken { get; set; }
    public string? SourceUrl { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Service interface for Web-to-Lead form management (TODO-CRM002-04)
/// </summary>
public interface IWebToLeadFormService
{
    /// <summary>
    /// Get all forms
    /// </summary>
    Task<IEnumerable<WebToLeadForm>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Get active forms only
    /// </summary>
    Task<IEnumerable<WebToLeadForm>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Get form by ID
    /// </summary>
    Task<WebToLeadForm?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Get form by embed key
    /// </summary>
    Task<WebToLeadForm?> GetByEmbedKeyAsync(string embedKey, CancellationToken ct = default);

    /// <summary>
    /// Create a new form
    /// </summary>
    Task<WebToLeadForm> CreateAsync(WebToLeadForm form, CancellationToken ct = default);

    /// <summary>
    /// Update an existing form
    /// </summary>
    Task<WebToLeadForm?> UpdateAsync(int id, WebToLeadForm form, CancellationToken ct = default);

    /// <summary>
    /// Soft delete a form
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Process a form submission and create a lead
    /// </summary>
    Task<(bool Success, int? LeadId, string? ErrorMessage)> ProcessSubmissionAsync(
        WebToLeadSubmissionDto submission,
        CancellationToken ct = default);

    /// <summary>
    /// Generate or regenerate embed key for a form
    /// </summary>
    Task<string> GenerateEmbedKeyAsync(int formId, CancellationToken ct = default);

    /// <summary>
    /// Get embed HTML for a form
    /// </summary>
    Task<string> GetEmbedHtmlAsync(int formId, CancellationToken ct = default);
}
