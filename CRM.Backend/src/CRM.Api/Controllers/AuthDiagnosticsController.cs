// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Features;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace CRM.Api.Controllers;

/// <summary>
/// Diagnostics endpoint for auth pipeline isolation.
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/auth-diagnostics")]
public class AuthDiagnosticsController : ControllerBase
{
    private readonly ILogger<AuthDiagnosticsController> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly IFeatureManager _featureManager;

    public AuthDiagnosticsController(
        ILogger<AuthDiagnosticsController> logger,
        IHostEnvironment environment,
        IConfiguration configuration,
        IFeatureManager featureManager)
    {
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
        _featureManager = featureManager;
    }

    /// <summary>
    /// Simple POST endpoint to validate POST handling without auth service DI.
    /// </summary>
    [HttpPost("ping")]
    [AllowAnonymous]
    public IActionResult Ping()
    {
        if (_environment.IsProduction())
        {
            return NotFound();
        }

        _logger.LogWarning("AuthDiagnosticsController.Ping reached");
        return Ok(new { ok = true, environment = _environment.EnvironmentName });
    }

    /// <summary>
    /// Returns known or expected configuration issues for non-production diagnostics.
    /// </summary>
    [HttpGet("known-issues")]
    [AllowAnonymous]
    public async Task<IActionResult> KnownIssues()
    {
        if (_environment.IsProduction())
        {
            return NotFound();
        }

        var issues = new List<object>();

        void AddIssue(string id, string title, string status, string detail, string recommendation)
        {
            issues.Add(new
            {
                id,
                title,
                status,
                detail,
                recommendation
            });
        }

        AddIssue(
            "DIAG-NONPROD-ONLY",
            "Diagnostics endpoint scope",
            "info",
            "Auth diagnostics endpoints are disabled in production.",
            "Use these endpoints only in development or staging environments.");

        AddIssue(
            "FRONTEND-BASEURL-SAME-ORIGIN",
            "Frontend API base URL on private networks",
            "info",
            "In non-production, the frontend should use same-origin API requests to avoid private-network port timeouts.",
            "Ensure the frontend resolves API base URL to same-origin when not running on localhost:3000.");

        var useExternalNotifications = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalNotifications);
        if (useExternalNotifications)
        {
            var notificationsType = (_configuration["Providers:Notifications:Type"] ?? "BuiltIn").ToLowerInvariant();
            var novuApiKey = _configuration["Providers:Notifications:Novu:ApiKey"];
            var sendGridApiKey = _configuration["Providers:Notifications:SendGrid:ApiKey"];
            var twilioSid = _configuration["Providers:Notifications:Twilio:AccountSid"];

            if (string.IsNullOrWhiteSpace(novuApiKey) && string.IsNullOrWhiteSpace(sendGridApiKey) && string.IsNullOrWhiteSpace(twilioSid))
            {
                AddIssue(
                    "EXT-NOTIFICATIONS-NO-CONFIG",
                    "External notifications enabled without provider config",
                    "warning",
                    $"UseExternalNotifications is true, but provider config is missing (Type={notificationsType}).",
                    "Set Providers:Notifications:Type and corresponding provider credentials (Novu/SendGrid/Twilio), or disable UseExternalNotifications.");
            }
            else if (notificationsType == "novu" && string.IsNullOrWhiteSpace(novuApiKey))
            {
                AddIssue(
                    "EXT-NOTIFICATIONS-NOVU-MISSING",
                    "Novu selected but ApiKey missing",
                    "warning",
                    "Providers:Notifications:Type is Novu, but ApiKey is empty.",
                    "Set Providers:Notifications:Novu:ApiKey or switch provider type.");
            }
            else if (notificationsType == "sendgrid" && string.IsNullOrWhiteSpace(sendGridApiKey))
            {
                AddIssue(
                    "EXT-NOTIFICATIONS-SENDGRID-MISSING",
                    "SendGrid selected but ApiKey missing",
                    "warning",
                    "Providers:Notifications:Type is SendGrid, but ApiKey is empty.",
                    "Set Providers:Notifications:SendGrid:ApiKey or switch provider type.");
            }
            else if (notificationsType == "twilio" && string.IsNullOrWhiteSpace(twilioSid))
            {
                AddIssue(
                    "EXT-NOTIFICATIONS-TWILIO-MISSING",
                    "Twilio selected but AccountSid missing",
                    "warning",
                    "Providers:Notifications:Type is Twilio, but AccountSid is empty.",
                    "Set Providers:Notifications:Twilio:AccountSid or switch provider type.");
            }
        }

        var useExternalSearch = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSearch);
        if (useExternalSearch)
        {
            var searchType = (_configuration["Providers:Search:Type"] ?? "BuiltIn").ToLowerInvariant();
            var meiliUrl = _configuration["Providers:Search:Meilisearch:Url"];
            var algoliaAppId = _configuration["Providers:Search:Algolia:ApplicationId"];

            if (searchType == "meilisearch" && string.IsNullOrWhiteSpace(meiliUrl))
            {
                AddIssue(
                    "EXT-SEARCH-MEILI-MISSING",
                    "Meilisearch selected but Url missing",
                    "warning",
                    "Providers:Search:Type is Meilisearch, but Url is empty.",
                    "Set Providers:Search:Meilisearch:Url or switch provider type.");
            }
            else if (searchType == "algolia" && string.IsNullOrWhiteSpace(algoliaAppId))
            {
                AddIssue(
                    "EXT-SEARCH-ALGOLIA-MISSING",
                    "Algolia selected but ApplicationId missing",
                    "warning",
                    "Providers:Search:Type is Algolia, but ApplicationId is empty.",
                    "Set Providers:Search:Algolia:ApplicationId or switch provider type.");
            }
        }

        var useExternalChat = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalChat);
        if (useExternalChat)
        {
            var chatType = (_configuration["Providers:Chat:Type"] ?? "BuiltIn").ToLowerInvariant();
            var chatwootBaseUrl = _configuration["Providers:Chat:Chatwoot:BaseUrl"];
            var intercomAppId = _configuration["Providers:Chat:Intercom:AppId"];

            if (chatType == "chatwoot" && string.IsNullOrWhiteSpace(chatwootBaseUrl))
            {
                AddIssue(
                    "EXT-CHAT-CHATWOOT-MISSING",
                    "Chatwoot selected but BaseUrl missing",
                    "warning",
                    "Providers:Chat:Type is Chatwoot, but BaseUrl is empty.",
                    "Set Providers:Chat:Chatwoot:BaseUrl or switch provider type.");
            }
            else if (chatType == "intercom" && string.IsNullOrWhiteSpace(intercomAppId))
            {
                AddIssue(
                    "EXT-CHAT-INTERCOM-MISSING",
                    "Intercom selected but AppId missing",
                    "warning",
                    "Providers:Chat:Type is Intercom, but AppId is empty.",
                    "Set Providers:Chat:Intercom:AppId or switch provider type.");
            }
        }

        var useExternalAnalytics = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAnalytics);
        if (useExternalAnalytics)
        {
            var analyticsType = (_configuration["Providers:Analytics:Type"] ?? "BuiltIn").ToLowerInvariant();
            var supersetBaseUrl = _configuration["Providers:Analytics:Superset:BaseUrl"];
            var powerBiTenant = _configuration["Providers:Analytics:PowerBI:TenantId"];

            if (analyticsType == "superset" && string.IsNullOrWhiteSpace(supersetBaseUrl))
            {
                AddIssue(
                    "EXT-ANALYTICS-SUPERSET-MISSING",
                    "Superset selected but BaseUrl missing",
                    "warning",
                    "Providers:Analytics:Type is Superset, but BaseUrl is empty.",
                    "Set Providers:Analytics:Superset:BaseUrl or switch provider type.");
            }
            else if (analyticsType == "powerbi" && string.IsNullOrWhiteSpace(powerBiTenant))
            {
                AddIssue(
                    "EXT-ANALYTICS-POWERBI-MISSING",
                    "Power BI selected but TenantId missing",
                    "warning",
                    "Providers:Analytics:Type is PowerBI, but TenantId is empty.",
                    "Set Providers:Analytics:PowerBI:TenantId or switch provider type.");
            }
        }

        var useExternalSignatures = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalSignatures);
        if (useExternalSignatures)
        {
            var signatureType = (_configuration["Providers:Signatures:Type"] ?? "BuiltIn").ToLowerInvariant();
            var docusealUrl = _configuration["Providers:Signatures:DocuSeal:Url"];
            var docusignAccount = _configuration["Providers:Signatures:DocuSign:AccountId"];

            if (signatureType == "docuseal" && string.IsNullOrWhiteSpace(docusealUrl))
            {
                AddIssue(
                    "EXT-SIGNATURES-DOCUSEAL-MISSING",
                    "DocuSeal selected but Url missing",
                    "warning",
                    "Providers:Signatures:Type is DocuSeal, but Url is empty.",
                    "Set Providers:Signatures:DocuSeal:Url or switch provider type.");
            }
            else if (signatureType == "docusign" && string.IsNullOrWhiteSpace(docusignAccount))
            {
                AddIssue(
                    "EXT-SIGNATURES-DOCUSIGN-MISSING",
                    "DocuSign selected but AccountId missing",
                    "warning",
                    "Providers:Signatures:Type is DocuSign, but AccountId is empty.",
                    "Set Providers:Signatures:DocuSign:AccountId or switch provider type.");
            }
        }

        var useExternalAI = await _featureManager.IsEnabledAsync(FeatureFlags.UseExternalAI);
        if (useExternalAI)
        {
            CheckAIProviderConfig(AddIssue);
        }

        return Ok(new
        {
            environment = _environment.EnvironmentName,
            issueCount = issues.Count,
            issues
        });
    }

    /// <summary>
    /// Validates AI provider configuration. Extracted to reduce cognitive complexity.
    /// </summary>
    private void CheckAIProviderConfig(Action<string, string, string, string, string> addIssue)
    {
        var aiType = (_configuration["Providers:AI:Type"] ?? "Ollama").ToLowerInvariant();

        var validations = new Dictionary<string, (Func<bool> IsMissing, string Id, string Title, string Detail, string Recommendation)>
        {
            ["openai"] = (
                () => string.IsNullOrWhiteSpace(_configuration["Providers:AI:OpenAI:ApiKey"]),
                "EXT-AI-OPENAI-MISSING", "OpenAI selected but ApiKey missing",
                "Providers:AI:Type is OpenAI, but ApiKey is empty.",
                "Set Providers:AI:OpenAI:ApiKey or switch provider type."),
            ["azureopenai"] = (
                () => string.IsNullOrWhiteSpace(_configuration["Providers:AI:AzureOpenAI:ApiKey"]) ||
                      string.IsNullOrWhiteSpace(_configuration["Providers:AI:AzureOpenAI:Endpoint"]),
                "EXT-AI-AZUREOPENAI-MISSING", "Azure OpenAI selected but ApiKey/Endpoint missing",
                "Providers:AI:Type is AzureOpenAI, but ApiKey or Endpoint is empty.",
                "Set Providers:AI:AzureOpenAI:ApiKey and Providers:AI:AzureOpenAI:Endpoint or switch provider type."),
            ["ollama"] = (
                () => string.IsNullOrWhiteSpace(_configuration["Providers:AI:Ollama:Url"]),
                "EXT-AI-OLLAMA-MISSING", "Ollama selected but Url missing",
                "Providers:AI:Type is Ollama, but Url is empty.",
                "Set Providers:AI:Ollama:Url or switch provider type."),
            ["anthropic"] = (
                () => string.IsNullOrWhiteSpace(_configuration["Providers:AI:Anthropic:ApiKey"]),
                "EXT-AI-ANTHROPIC-MISSING", "Anthropic selected but ApiKey missing",
                "Providers:AI:Type is Anthropic, but ApiKey is empty.",
                "Set Providers:AI:Anthropic:ApiKey or switch provider type."),
            ["openrouter"] = (
                () => string.IsNullOrWhiteSpace(_configuration["Providers:AI:OpenRouter:ApiKey"]),
                "EXT-AI-OPENROUTER-MISSING", "OpenRouter selected but ApiKey missing",
                "Providers:AI:Type is OpenRouter, but ApiKey is empty.",
                "Set Providers:AI:OpenRouter:ApiKey or switch provider type.")
        };

        if (validations.TryGetValue(aiType, out var validation) && validation.IsMissing())
        {
            addIssue(validation.Id, validation.Title, "warning", validation.Detail, validation.Recommendation);
        }
    }
}
