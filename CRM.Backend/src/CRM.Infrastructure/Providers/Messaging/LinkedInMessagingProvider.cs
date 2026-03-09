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
/// Mock-only stub for LinkedIn Messaging.
/// <para>
/// COMM-004: LinkedIn outbound messaging via the Messages API requires
/// LinkedIn Sales Navigator ($1,600+/year). Real outbound implementation is
/// deferred to a production environment with the appropriate subscription. This
/// provider handles <b>inbound-only</b> webhook events via Mockoon simulation in
/// development.
/// </para>
/// <para>
/// <see cref="IsAvailable"/> always returns <c>false</c>.
/// <see cref="IsMockMode"/> always returns <c>true</c>.
/// <see cref="SendMessageAsync"/> always logs a warning and returns <c>false</c>.
/// </para>
/// </summary>
public class LinkedInMessagingProvider : ILinkedInMessagingProvider
{
    private readonly LinkedInMessagingOptions _options;
    private readonly ILogger<LinkedInMessagingProvider> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LinkedInMessagingProvider"/>.
    /// </summary>
    public LinkedInMessagingProvider(
        IOptions<LinkedInMessagingOptions> options,
        ILogger<LinkedInMessagingProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Always <c>false</c>: outbound messaging is not available without LinkedIn Sales Navigator.
    /// </remarks>
    public bool IsAvailable => false;

    /// <inheritdoc />
    /// <remarks>Always <c>true</c>: this implementation is mock-only.</remarks>
    public bool IsMockMode => true;

    /// <inheritdoc />
    /// <remarks>
    /// COMM-004: This method is a no-op stub. LinkedIn outbound messaging requires
    /// Sales Navigator ($1,600+/year). Always returns <c>false</c>.
    /// </remarks>
    public Task<bool> SendMessageAsync(
        string recipientUrn,
        string message,
        CancellationToken ct = default)
    {
        _logger.LogWarning(
            "LinkedIn outbound messaging not available (requires Sales Navigator). " +
            "Message to {RecipientUrn} skipped. " +
            "COMM-004: Enable real outbound by subscribing to LinkedIn Sales Navigator ($1,600+/year).",
            recipientUrn);

        return Task.FromResult(false);
    }
}
