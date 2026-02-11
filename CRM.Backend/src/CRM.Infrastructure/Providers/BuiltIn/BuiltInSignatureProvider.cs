// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using CRM.Core.Ports.Output.Providers;

// Resolve naming conflict: SignerStatus class from Ports vs SignerStatus enum from Entities
using PortSignerStatus = CRM.Core.Ports.Output.Providers.SignerStatus;

namespace CRM.Infrastructure.Providers.BuiltIn;

/// <summary>
/// Built-in signature provider for manual signature workflows.
/// Provides basic signature request tracking without external e-signature services.
/// Use cases:
/// - POC/development environments
/// - Manual signature collection (physical signatures scanned)
/// - Basic approval workflows without legal e-signatures
///
/// For production e-signatures, use DocuSeal, DocuSign, or other providers.
/// </summary>
public class BuiltInSignatureProvider : ISignaturePort
{
    private readonly ILogger<BuiltInSignatureProvider> _logger;

    // In-memory storage for development/testing
    private readonly ConcurrentDictionary<string, SignatureTemplate> _templates = new();
    private readonly ConcurrentDictionary<string, SignatureRequest> _requests = new();
    private readonly ConcurrentDictionary<string, SignedDocument> _signedDocuments = new();
    private readonly ConcurrentDictionary<string, List<byte[]>> _auditTrails = new();

    private int _templateCounter = 0;
    private int _requestCounter = 0;

    /// <inheritdoc />
    public string ProviderName => "BuiltIn";

    /// <summary>
    /// Initializes a new instance of the <see cref="BuiltInSignatureProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public BuiltInSignatureProvider(ILogger<BuiltInSignatureProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("BuiltInSignatureProvider initialized. Note: This is for manual workflows only.");
    }

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        // BuiltIn is always available
        return Task.FromResult(true);
    }

    #region Template Management

    /// <inheritdoc />
    public Task<IEnumerable<SignatureTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting all signature templates. Count: {Count}", _templates.Count);
        return Task.FromResult(_templates.Values.Where(t => t.IsActive).AsEnumerable());
    }

    /// <inheritdoc />
    public Task<SignatureTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        _templates.TryGetValue(templateId, out var template);
        return Task.FromResult(template);
    }

    /// <inheritdoc />
    public Task<SignatureTemplate> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        var templateId = $"builtin-template-{Interlocked.Increment(ref _templateCounter)}";

        var template = new SignatureTemplate
        {
            Id = templateId,
            Name = request.Name,
            Description = request.Description,
            SignerCount = request.SignerRoles?.Count ?? 1,
            SignerRoles = request.SignerRoles?.ToList(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _templates[templateId] = template;
        _logger.LogInformation("Created signature template: {TemplateId} - {Name}", templateId, template.Name);

        return Task.FromResult(template);
    }

    #endregion

    #region Signature Request Operations

    /// <inheritdoc />
    public Task<SignatureRequest> CreateSignatureRequestAsync(CreateSignatureRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject);

        var requestId = $"builtin-sig-{Interlocked.Increment(ref _requestCounter):D6}";
        var now = DateTime.UtcNow;

        var signerStatuses = request.Signers.Select((s, i) => new PortSignerStatus
        {
            Name = s.Name,
            Email = s.Email,
            Status = "pending",
            Order = s.Order > 0 ? s.Order : i + 1
        }).ToList();

        var documents = request.Documents?.Select((d, i) => new SignatureDocumentInfo
        {
            Id = $"{requestId}-doc-{i + 1}",
            Name = d.Name,
            Order = d.Order > 0 ? d.Order : i + 1,
            PageCount = 1 // Assume 1 page for built-in
        }).ToList();

        var signatureRequest = new SignatureRequest
        {
            Id = requestId,
            Subject = request.Subject,
            Status = SignatureStatus.Sent,
            Signers = signerStatuses,
            Documents = documents,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            CreatedAt = now,
            SentAt = now,
            ExpiresAt = request.ExpiresAt ?? now.AddDays(30),
            Metadata = request.Metadata
        };

        _requests[requestId] = signatureRequest;
        _logger.LogInformation(
            "Created signature request: {RequestId} for entity {EntityType}/{EntityId}",
            requestId, request.EntityType, request.EntityId);

        return Task.FromResult(signatureRequest);
    }

    /// <inheritdoc />
    public Task<SignatureRequest?> GetSignatureRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        _requests.TryGetValue(requestId, out var request);
        return Task.FromResult(request);
    }

    /// <inheritdoc />
    public Task<SignatureStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        if (_requests.TryGetValue(requestId, out var request))
        {
            return Task.FromResult(request.Status);
        }

        return Task.FromResult(SignatureStatus.Draft);
    }

    /// <inheritdoc />
    public Task<IEnumerable<SignatureRequest>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        var requests = _requests.Values
            .Where(r => r.EntityType == entityType && r.EntityId == entityId)
            .OrderByDescending(r => r.CreatedAt);

        return Task.FromResult(requests.AsEnumerable());
    }

    /// <inheritdoc />
    public Task CancelSignatureRequestAsync(string requestId, string? reason = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        if (_requests.TryGetValue(requestId, out var request))
        {
            request.Status = SignatureStatus.Voided;
            _logger.LogInformation("Cancelled signature request: {RequestId}. Reason: {Reason}", requestId, reason ?? "Not provided");
        }
        else
        {
            _logger.LogWarning("Attempted to cancel non-existent signature request: {RequestId}", requestId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SendReminderAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        if (_requests.TryGetValue(requestId, out var request))
        {
            _logger.LogInformation(
                "Reminder sent for signature request: {RequestId}. Pending signers: {PendingCount}",
                requestId,
                request.Signers.Count(s => s.Status == "pending"));
        }

        // BuiltIn provider doesn't actually send reminders - just logs
        return Task.CompletedTask;
    }

    #endregion

    #region Signing Operations

    /// <inheritdoc />
    public Task<SigningLink> GetSigningLinkAsync(string requestId, string signerEmail, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        // BuiltIn provider returns a placeholder URL
        // In production, this would link to an internal signing page or manual process
        var link = new SigningLink
        {
            Url = $"/signatures/manual-sign/{requestId}?signer={Uri.EscapeDataString(signerEmail)}",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsEmbedded = false
        };

        _logger.LogDebug("Generated signing link for {RequestId}, signer: {Email}", requestId, signerEmail);
        return Task.FromResult(link);
    }

    /// <inheritdoc />
    public Task<SigningLink> GetEmbeddedSigningAsync(string requestId, string signerEmail, string returnUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        // BuiltIn provider doesn't support true embedded signing
        // Returns a link to manual signing page
        var link = new SigningLink
        {
            Url = $"/signatures/manual-sign/{requestId}?signer={Uri.EscapeDataString(signerEmail)}&return={Uri.EscapeDataString(returnUrl ?? "/")}",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsEmbedded = true
        };

        _logger.LogDebug("Generated embedded signing link for {RequestId}", requestId);
        return Task.FromResult(link);
    }

    /// <summary>
    /// Records a manual signature (called when signature is physically collected).
    /// </summary>
    public Task<bool> RecordManualSignatureAsync(
        string requestId,
        string signerEmail,
        string signerName,
        byte[]? signatureImage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        if (!_requests.TryGetValue(requestId, out var request))
        {
            _logger.LogWarning("Cannot record signature for non-existent request: {RequestId}", requestId);
            return Task.FromResult(false);
        }

        var signer = request.Signers.FirstOrDefault(s =>
            s.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase));

        if (signer == null)
        {
            _logger.LogWarning("Signer {Email} not found in request {RequestId}", signerEmail, requestId);
            return Task.FromResult(false);
        }

        // Update signer status
        signer.Status = "signed";
        signer.SignedAt = DateTime.UtcNow;

        // Check if all signers have signed
        if (request.Signers.All(s => s.Status == "signed"))
        {
            request.Status = SignatureStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Signature request {RequestId} completed - all signers signed", requestId);
        }
        else
        {
            request.Status = SignatureStatus.InProgress;
        }

        _logger.LogInformation(
            "Recorded manual signature for {RequestId} by {SignerName} ({SignerEmail})",
            requestId, signerName, signerEmail);

        return Task.FromResult(true);
    }

    #endregion

    #region Document Operations

    /// <inheritdoc />
    public Task<SignedDocument> GetSignedDocumentAsync(string requestId, string? documentId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        // Check if we have a stored signed document
        var key = documentId != null ? $"{requestId}-{documentId}" : requestId;

        if (_signedDocuments.TryGetValue(key, out var doc))
        {
            return Task.FromResult(doc);
        }

        // BuiltIn provider returns empty document with placeholder
        // In production, this would return the actual signed PDF
        var signedDoc = new SignedDocument
        {
            RequestId = requestId,
            DocumentId = documentId,
            FileName = $"signed-document-{requestId}.pdf",
            ContentType = "application/pdf",
            Content = Array.Empty<byte>(), // Placeholder
            SignedAt = DateTime.UtcNow
        };

        _logger.LogWarning(
            "BuiltIn provider cannot generate signed documents. RequestId: {RequestId}",
            requestId);

        return Task.FromResult(signedDoc);
    }

    /// <summary>
    /// Stores a signed document (for manual upload of signed PDFs).
    /// </summary>
    public Task StoreSignedDocumentAsync(string requestId, string? documentId, byte[] content, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentNullException.ThrowIfNull(content);

        var key = documentId != null ? $"{requestId}-{documentId}" : requestId;

        var doc = new SignedDocument
        {
            RequestId = requestId,
            DocumentId = documentId,
            FileName = fileName,
            ContentType = "application/pdf",
            Content = content,
            SignedAt = DateTime.UtcNow
        };

        _signedDocuments[key] = doc;
        _logger.LogInformation("Stored signed document for request {RequestId}", requestId);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<byte[]> GetAuditTrailAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        // Generate a simple text-based audit trail
        if (!_requests.TryGetValue(requestId, out var request))
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        var auditLines = new List<string>
        {
            "===== SIGNATURE REQUEST AUDIT TRAIL =====",
            $"Request ID: {request.Id}",
            $"Subject: {request.Subject}",
            $"Created: {request.CreatedAt:O}",
            $"Status: {request.Status}",
            "",
            "--- Signers ---"
        };

        foreach (var signer in request.Signers)
        {
            auditLines.Add($"Name: {signer.Name}");
            auditLines.Add($"Email: {signer.Email}");
            auditLines.Add($"Status: {signer.Status}");
            if (signer.SignedAt.HasValue)
            {
                auditLines.Add($"Signed At: {signer.SignedAt.Value:O}");
            }
            auditLines.Add("");
        }

        auditLines.Add("===== END OF AUDIT TRAIL =====");
        auditLines.Add($"Generated: {DateTime.UtcNow:O}");
        auditLines.Add("Provider: BuiltIn (Manual Signature Workflow)");

        var content = string.Join(Environment.NewLine, auditLines);
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(content));
    }

    #endregion

    #region Webhook Processing

    /// <inheritdoc />
    public Task<SignatureWebhookResult> ProcessWebhookAsync(string eventType, string payload, string? signature = null, CancellationToken cancellationToken = default)
    {
        // BuiltIn provider doesn't receive external webhooks
        // This method can be used internally for state change notifications
        _logger.LogDebug("BuiltIn provider received internal event: {EventType}", eventType);

        var result = new SignatureWebhookResult
        {
            Success = true,
            EventType = eventType,
            RequestId = string.Empty
        };

        // Parse internal events if needed
        if (eventType.StartsWith("builtin:"))
        {
            var parts = eventType.Split(':');
            if (parts.Length >= 3)
            {
                result.RequestId = parts[1];
                result.EventType = parts[2];

                if (_requests.TryGetValue(result.RequestId, out var request))
                {
                    result.NewStatus = request.Status;
                    result.EntityType = request.EntityType;
                    result.EntityId = request.EntityId;
                }
            }
        }

        return Task.FromResult(result);
    }

    #endregion

    #region Health Check

    /// <inheritdoc />
    public Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = ProviderName,
            ResponseTimeMs = 0,
            Message = "BuiltIn signature provider is always available",
            Details = new Dictionary<string, object>
            {
                { "templates_count", _templates.Count },
                { "active_requests", _requests.Values.Count(r => r.Status is SignatureStatus.Sent or SignatureStatus.InProgress) },
                { "completed_requests", _requests.Values.Count(r => r.Status == SignatureStatus.Completed) },
                { "note", "BuiltIn provider is for manual workflows only" }
            }
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Marks a signer as having viewed the document.
    /// </summary>
    public Task MarkAsViewedAsync(string requestId, string signerEmail, CancellationToken cancellationToken = default)
    {
        if (_requests.TryGetValue(requestId, out var request))
        {
            var signer = request.Signers.FirstOrDefault(s =>
                s.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase));

            if (signer != null && signer.ViewedAt == null)
            {
                signer.ViewedAt = DateTime.UtcNow;
                signer.Status = "viewed";

                if (request.Status == SignatureStatus.Sent)
                {
                    request.Status = SignatureStatus.Delivered;
                }

                _logger.LogDebug("Marked request {RequestId} as viewed by {Email}", requestId, signerEmail);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Records a declined signature.
    /// </summary>
    public Task DeclineSignatureAsync(string requestId, string signerEmail, string? reason = null, CancellationToken cancellationToken = default)
    {
        if (_requests.TryGetValue(requestId, out var request))
        {
            var signer = request.Signers.FirstOrDefault(s =>
                s.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase));

            if (signer != null)
            {
                signer.Status = "declined";
                signer.DeclinedAt = DateTime.UtcNow;
                signer.DeclineReason = reason;
                request.Status = SignatureStatus.Declined;

                _logger.LogInformation(
                    "Signature declined for {RequestId} by {Email}. Reason: {Reason}",
                    requestId, signerEmail, reason ?? "Not provided");
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears all in-memory data (for testing).
    /// </summary>
    public void ClearAll()
    {
        _templates.Clear();
        _requests.Clear();
        _signedDocuments.Clear();
        _auditTrails.Clear();
        _templateCounter = 0;
        _requestCounter = 0;
        _logger.LogDebug("BuiltInSignatureProvider data cleared");
    }

    #endregion
}
