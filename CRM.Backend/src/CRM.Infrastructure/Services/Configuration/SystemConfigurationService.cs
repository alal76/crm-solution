// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Text.Json;
using System.Text.RegularExpressions;
using CRM.Core.Dtos;
using CRM.Core.Ports;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services.Configuration;

/// <summary>
/// Manages system-level configurations (email, 2FA, social login / SSO).
/// Delegates persistence to <see cref="IProviderConfigurationService"/>.
/// </summary>
public partial class SystemConfigurationService : ISystemConfigurationService
{
    // Well-known configuration keys
    private const string EmailSmtpKey = "system.email.smtp";
    private const string TwoFactorKey = "system.2fa.config";
    private const string SsoGoogleKey = "system.sso.google";
    private const string SsoMicrosoftKey = "system.sso.microsoft";
    private const string SsoAzureKey = "system.sso.azure";
    private const string SsoLinkedInKey = "system.sso.linkedin";
    private const string SsoFacebookKey = "system.sso.facebook";

    private readonly IProviderConfigurationService _providerConfig;
    private readonly ILogger<SystemConfigurationService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SystemConfigurationService(
        IProviderConfigurationService providerConfig,
        ILogger<SystemConfigurationService> logger)
    {
        _providerConfig = providerConfig;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SystemConfigResponseDto> GetSystemConfigAsync(
        CancellationToken cancellationToken = default)
    {
        var response = new SystemConfigResponseDto();
        DateTime latestUpdate = DateTime.MinValue;
        string? latestUpdater = null;

        // Email configuration
        var emailConfig = await _providerConfig.GetConfigurationAsync(EmailSmtpKey, cancellationToken);
        if (emailConfig != null)
        {
            response.EmailServer = DeserializeDto<EmailServerConfigDto>(emailConfig.ConfigurationData);
            TrackLatestUpdate(emailConfig.UpdatedAt, emailConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        // 2FA configuration
        var twoFaConfig = await _providerConfig.GetConfigurationAsync(TwoFactorKey, cancellationToken);
        if (twoFaConfig != null)
        {
            response.TwoFactor = DeserializeDto<TwoFactorConfigDto>(twoFaConfig.ConfigurationData);
            TrackLatestUpdate(twoFaConfig.UpdatedAt, twoFaConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        // Social login providers
        response.SocialLogin = new SocialLoginConfigDto();

        var googleConfig = await _providerConfig.GetConfigurationAsync(SsoGoogleKey, cancellationToken);
        if (googleConfig != null)
        {
            response.SocialLogin.Google = DeserializeDto<GoogleOAuthDto>(googleConfig.ConfigurationData);
            TrackLatestUpdate(googleConfig.UpdatedAt, googleConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        var msConfig = await _providerConfig.GetConfigurationAsync(SsoMicrosoftKey, cancellationToken);
        if (msConfig != null)
        {
            response.SocialLogin.Microsoft = DeserializeDto<MicrosoftOAuthDto>(msConfig.ConfigurationData);
            TrackLatestUpdate(msConfig.UpdatedAt, msConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        var azureConfig = await _providerConfig.GetConfigurationAsync(SsoAzureKey, cancellationToken);
        if (azureConfig != null)
        {
            response.SocialLogin.AzureAd = DeserializeDto<AzureAdDto>(azureConfig.ConfigurationData);
            TrackLatestUpdate(azureConfig.UpdatedAt, azureConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        var linkedInConfig = await _providerConfig.GetConfigurationAsync(SsoLinkedInKey, cancellationToken);
        if (linkedInConfig != null)
        {
            response.SocialLogin.LinkedIn = DeserializeDto<LinkedInOAuthDto>(linkedInConfig.ConfigurationData);
            TrackLatestUpdate(linkedInConfig.UpdatedAt, linkedInConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        var fbConfig = await _providerConfig.GetConfigurationAsync(SsoFacebookKey, cancellationToken);
        if (fbConfig != null)
        {
            response.SocialLogin.Facebook = DeserializeDto<FacebookOAuthDto>(fbConfig.ConfigurationData);
            TrackLatestUpdate(fbConfig.UpdatedAt, fbConfig.UpdatedByUserName, ref latestUpdate, ref latestUpdater);
        }

        response.LastUpdated = latestUpdate == DateTime.MinValue ? DateTime.UtcNow : latestUpdate;
        response.UpdatedBy = latestUpdater;

        return response;
    }

    /// <inheritdoc />
    public async Task UpdateEmailServerAsync(
        EmailServerConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        ValidateEmailConfig(config);

        var data = DtoToDictionary(config);
        await _providerConfig.UpdateConfigurationAsync(EmailSmtpKey, data, userId, cancellationToken);

        _logger.LogInformation("Email server configuration updated by user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task UpdateTwoFactorAsync(
        TwoFactorConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var data = DtoToDictionary(config);
        await _providerConfig.UpdateConfigurationAsync(TwoFactorKey, data, userId, cancellationToken);

        _logger.LogInformation("Two-factor configuration updated by user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task UpdateSocialLoginAsync(
        SocialLoginConfigDto config,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (config.Google != null)
        {
            var data = DtoToDictionary(config.Google);
            await _providerConfig.UpdateConfigurationAsync(SsoGoogleKey, data, userId, cancellationToken);
        }

        if (config.Microsoft != null)
        {
            var data = DtoToDictionary(config.Microsoft);
            await _providerConfig.UpdateConfigurationAsync(SsoMicrosoftKey, data, userId, cancellationToken);
        }

        if (config.AzureAd != null)
        {
            var data = DtoToDictionary(config.AzureAd);
            await _providerConfig.UpdateConfigurationAsync(SsoAzureKey, data, userId, cancellationToken);
        }

        if (config.LinkedIn != null)
        {
            var data = DtoToDictionary(config.LinkedIn);
            await _providerConfig.UpdateConfigurationAsync(SsoLinkedInKey, data, userId, cancellationToken);
        }

        if (config.Facebook != null)
        {
            var data = DtoToDictionary(config.Facebook);
            await _providerConfig.UpdateConfigurationAsync(SsoFacebookKey, data, userId, cancellationToken);
        }

        _logger.LogInformation("Social login configuration updated by user {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<ConfigurationTestResultDto> TestEmailServerAsync(
        EmailServerConfigDto config,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(config.SmtpServer))
        {
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = "SMTP server address is required",
                TestedAt = DateTime.UtcNow
            };
        }

        try
        {
            // Validate port range first
            if (config.SmtpPort < 1 || config.SmtpPort > 65535)
            {
                return new ConfigurationTestResultDto
                {
                    Success = false,
                    Message = $"Invalid SMTP port: {config.SmtpPort}. Must be between 1 and 65535.",
                    TestedAt = DateTime.UtcNow
                };
            }

            // Test TCP connectivity to SMTP server
            using var tcpClient = new System.Net.Sockets.TcpClient();
            var connectTask = tcpClient.ConnectAsync(config.SmtpServer, config.SmtpPort);
            var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(10), cancellationToken));

            if (completed != connectTask || !tcpClient.Connected)
            {
                return new ConfigurationTestResultDto
                {
                    Success = false,
                    Message = $"Connection to {config.SmtpServer}:{config.SmtpPort} timed out after 10 seconds",
                    TestedAt = DateTime.UtcNow
                };
            }

            return new ConfigurationTestResultDto
            {
                Success = true,
                Message = $"Successfully connected to SMTP server {config.SmtpServer}:{config.SmtpPort}",
                TestedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SMTP test failed for {SmtpServer}:{SmtpPort}", config.SmtpServer, config.SmtpPort);
            return new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"SMTP connection failed: {ex.Message}",
                ErrorDetails = ex.ToString(),
                TestedAt = DateTime.UtcNow
            };
        }
    }

    /// <inheritdoc />
    public Task<ConfigurationTestResultDto> TestSocialProviderAsync(
        string provider,
        Dictionary<string, string> credentials,
        CancellationToken cancellationToken = default)
    {
        // Validate OAuth configuration structure based on provider
        var missingFields = provider.ToLowerInvariant() switch
        {
            "google" => ValidateRequiredCredentials(credentials, "clientId", "clientSecret"),
            "microsoft" => ValidateRequiredCredentials(credentials, "clientId", "clientSecret", "tenantId"),
            "azure" => ValidateRequiredCredentials(credentials, "clientId", "clientSecret", "tenantId"),
            "linkedin" => ValidateRequiredCredentials(credentials, "clientId", "clientSecret"),
            "facebook" => ValidateRequiredCredentials(credentials, "appId", "appSecret"),
            _ => new List<string> { $"Unknown provider: {provider}" }
        };

        if (missingFields.Count > 0)
        {
            return Task.FromResult(new ConfigurationTestResultDto
            {
                Success = false,
                Message = $"Missing required fields: {string.Join(", ", missingFields)}",
                TestedAt = DateTime.UtcNow
            });
        }

        // Structure is valid — actual OAuth flow testing requires browser interaction
        return Task.FromResult(new ConfigurationTestResultDto
        {
            Success = true,
            Message = $"OAuth configuration for {provider} is structurally valid. " +
                      "Full authentication flow requires browser-based verification.",
            TestedAt = DateTime.UtcNow
        });
    }

    // ------------------------------------------------------------------
    // Private helpers
    // ------------------------------------------------------------------

    private static void ValidateEmailConfig(EmailServerConfigDto config)
    {
        if (string.IsNullOrWhiteSpace(config.SmtpServer))
        {
            throw new ArgumentException("SMTP server address is required.", nameof(config));
        }

        if (config.SmtpPort < 1 || config.SmtpPort > 65535)
        {
            throw new ArgumentException($"SMTP port must be between 1 and 65535. Got: {config.SmtpPort}", nameof(config));
        }

        if (!string.IsNullOrWhiteSpace(config.FromEmail) && !EmailRegex().IsMatch(config.FromEmail))
        {
            throw new ArgumentException($"Invalid email format: {config.FromEmail}", nameof(config));
        }
    }

    private static List<string> ValidateRequiredCredentials(
        Dictionary<string, string> credentials,
        params string[] requiredKeys)
    {
        var missing = new List<string>();

        foreach (var key in requiredKeys)
        {
            if (!credentials.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                missing.Add(key);
            }
        }

        return missing;
    }

    private static T? DeserializeDto<T>(string json) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, object> DtoToDictionary<T>(T dto)
    {
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json, JsonOptions)
            ?? new Dictionary<string, object>();
    }

    private static void TrackLatestUpdate(
        DateTime updatedAt,
        string? updatedBy,
        ref DateTime latestUpdate,
        ref string? latestUpdater)
    {
        if (updatedAt > latestUpdate)
        {
            latestUpdate = updatedAt;
            latestUpdater = updatedBy;
        }
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
