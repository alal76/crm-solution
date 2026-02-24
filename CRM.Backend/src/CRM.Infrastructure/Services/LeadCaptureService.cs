// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Cryptography;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Web-to-lead form capture service implementation.
/// TODO-CRM002-04: Implement web-to-lead form builder integration
/// </summary>
public class LeadCaptureService : ILeadCaptureService
{
    private readonly ICrmDbContext _context;
    private readonly ILeadService _leadService;
    private readonly ILogger<LeadCaptureService> _logger;

    // In-memory token store (in production, use Redis or database)
    private static readonly Dictionary<string, FormTokenData> _tokens = new();

    public LeadCaptureService(
        ICrmDbContext context,
        ILeadService leadService,
        ILogger<LeadCaptureService> logger)
    {
        _context = context;
        _leadService = leadService;
        _logger = logger;
    }

    public Task<FormTokenResult> GenerateFormTokenAsync(
        string formName,
        int? campaignId = null,
        int expiresInHours = 24,
        CancellationToken ct = default)
    {
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(expiresInHours);

        var tokenData = new FormTokenData
        {
            Token = token,
            FormName = formName,
            CampaignId = campaignId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };

        _tokens[token] = tokenData;

        var result = new FormTokenResult
        {
            Token = token,
            FormName = formName,
            CampaignId = campaignId,
            ExpiresAt = expiresAt,
            EmbedCode = GenerateEmbedCode(token)
        };

        _logger.LogInformation("Generated form token for form '{FormName}', expires at {ExpiresAt}", formName, expiresAt);
        return Task.FromResult(result);
    }

    public Task<bool> ValidateFormTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        if (_tokens.TryGetValue(token, out var tokenData))
        {
            return Task.FromResult(tokenData.IsActive && tokenData.ExpiresAt > DateTime.UtcNow);
        }

        return Task.FromResult(false);
    }

    public async Task<LeadCaptureResult> CaptureLeadFromFormAsync(
        LeadCaptureRequest request,
        CancellationToken ct = default)
    {
        // Validate token
        if (!await ValidateFormTokenAsync(request.Token, ct))
        {
            return new LeadCaptureResult
            {
                Success = false,
                ErrorMessage = "Invalid or expired form token"
            };
        }

        var tokenData = _tokens[request.Token];

        // Check for duplicates
        var (isDuplicate, existingLeadId, _) = await _leadService.CheckDuplicateAsync(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Company,
            ct);

        if (isDuplicate && existingLeadId.HasValue)
        {
            return new LeadCaptureResult
            {
                Success = false,
                IsDuplicate = true,
                ExistingLeadId = existingLeadId,
                ErrorMessage = "Lead already exists"
            };
        }

        try
        {
            // Create lead
            var lead = new Lead
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                CompanyName = request.Company,
                Title = request.Title,
                Website = request.Website,
                QualificationNotes = request.Message,
                Source = LeadSource.Web,
                CampaignId = tokenData.CampaignId,
                UtmSource = request.UtmSource,
                UtmMedium = request.UtmMedium,
                UtmCampaign = request.UtmCampaign,
                OriginalSource = $"Web Form: {tokenData.FormName}",
                FirstTouchDate = DateTime.UtcNow,
                Status = LeadLifecycleStatus.New
            };

            var leadId = await _leadService.CreateAsync(lead);

            // Increment submission count
            tokenData.SubmissionCount++;

            _logger.LogInformation("Lead captured from web form '{FormName}': Lead ID {LeadId}", 
                tokenData.FormName, leadId);

            return new LeadCaptureResult
            {
                Success = true,
                LeadId = leadId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing lead from form");
            return new LeadCaptureResult
            {
                Success = false,
                ErrorMessage = "An error occurred while processing the form submission"
            };
        }
    }

    public Task<IEnumerable<FormTokenInfo>> GetActiveTokensAsync(CancellationToken ct = default)
    {
        var activeTokens = _tokens.Values
            .Where(t => t.IsActive && t.ExpiresAt > DateTime.UtcNow)
            .Select(t => new FormTokenInfo
            {
                Token = t.Token,
                FormName = t.FormName,
                CampaignId = t.CampaignId,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                SubmissionCount = t.SubmissionCount,
                IsActive = t.IsActive
            })
            .ToList();

        return Task.FromResult<IEnumerable<FormTokenInfo>>(activeTokens);
    }

    public Task RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        if (_tokens.TryGetValue(token, out var tokenData))
        {
            tokenData.IsActive = false;
            _logger.LogInformation("Form token revoked: {Token}", token);
        }

        return Task.CompletedTask;
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string GenerateEmbedCode(string token)
    {
        return $@"<!-- CRM Lead Capture Form -->
<script src=""https://your-crm-domain.com/js/lead-capture.js""></script>
<div id=""crm-lead-form"" data-token=""{token}""></div>
<script>CRMLeadCapture.init({{ token: '{token}' }});</script>";
    }

    private class FormTokenData
    {
        public string Token { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public int? CampaignId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public int SubmissionCount { get; set; }
    }
}
