// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Configuration;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CRM.Infrastructure.Providers.Messaging;

/// <summary>
/// Mock-only stub for Twitter/X Direct Messaging.
/// <para>
/// COMM-003: Twitter outbound DMs require the $100/month Basic API tier or higher.
/// Real outbound implementation is deferred to a production environment that has
/// the appropriate API access level. This provider handles <b>inbound-only</b>
/// webhook events via Mockoon simulation in development.
/// </para>
/// <para>
/// <see cref="IsAvailable"/> always returns <c>false</c>.
/// <see cref="IsMockMode"/> always returns <c>true</c>.
/// <see cref="SendDirectMessageAsync"/> always logs a warning and returns <c>false</c>.
/// </para>
/// </summary>
public class TwitterMessagingProvider : ITwitterMessagingProvider
{
    private readonly TwitterMessagingOptions _options;
    private readonly ILogger<TwitterMessagingProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="TwitterMessagingProvider"/>.
    /// </summary>
    public TwitterMessagingProvider(
        IOptions<TwitterMessagingOptions> options,
        ILogger<TwitterMessagingProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always <c>false</c>: outbound DMs are not available without a paid Twitter/X API tier.
    /// </remarks>
    public bool IsAvailable => false;

    /// <inheritdoc />
    /// <remarks>Always <c>true</c>: this implementation is mock-only.</remarks>
    public bool IsMockMode => true;

    /// <inheritdoc />
    /// <remarks>
    /// COMM-003: This method is a no-op stub. Twitter outbound DMs require the
    /// $100/month Basic API tier minimum. Always returns <c>false</c>.
    /// </remarks>
    public Task<bool> SendDirectMessageAsync(
        string recipientUserId,
        string message,
        CancellationToken ct = default)
    {
        _logger.LogWarning(
            "Twitter outbound DMs are not available (requires paid API tier). " +
            "Message to user {RecipientUserId} skipped. " +
            "COMM-003: Enable real outbound by upgrading to Twitter Basic API ($100/month).",
            recipientUserId);

        return Task.FromResult(false);
    }
}
