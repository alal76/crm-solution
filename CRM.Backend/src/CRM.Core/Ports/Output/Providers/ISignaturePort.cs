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

namespace CRM.Core.Ports.Output.Providers;

#region Signature Port Interface

/// <summary>
/// Output port for electronic signature operations.
/// Enables document signing workflows for quotes, contracts, and agreements.
/// Implementations: BuiltIn (manual), DocuSeal, DocuSign, Adobe Sign, HelloSign.
/// </summary>
public interface ISignaturePort
{
    /// <summary>
    /// Gets the unique identifier for this signature provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Checks if the signature provider is properly configured and available.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    #region Template Management

    /// <summary>
    /// Gets available signature templates.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of templates.</returns>
    Task<IEnumerable<SignatureTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific template by ID.
    /// </summary>
    /// <param name="templateId">The template ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Template if found.</returns>
    Task<SignatureTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new template from a document.
    /// </summary>
    /// <param name="request">Template creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created template.</returns>
    Task<SignatureTemplate> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);

    #endregion

    #region Signature Request Operations

    /// <summary>
    /// Creates a new signature request (envelope).
    /// </summary>
    /// <param name="request">Signature request details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created signature request.</returns>
    Task<SignatureRequest> CreateSignatureRequestAsync(CreateSignatureRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a signature request by ID.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Signature request if found.</returns>
    Task<SignatureRequest?> GetSignatureRequestAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current status of a signature request.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current status.</returns>
    Task<SignatureStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets signature requests by CRM entity.
    /// </summary>
    /// <param name="entityType">Entity type (Quote, Contract).</param>
    /// <param name="entityId">Entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of related signature requests.</returns>
    Task<IEnumerable<SignatureRequest>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels/voids a signature request.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="reason">Cancellation reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CancelSignatureRequestAsync(string requestId, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reminder to pending signers.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendReminderAsync(string requestId, CancellationToken cancellationToken = default);

    #endregion

    #region Signing Operations

    /// <summary>
    /// Gets the signing URL for a signer.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="signerEmail">Signer's email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Signing URL.</returns>
    Task<SigningLink> GetSigningLinkAsync(string requestId, string signerEmail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an embedded signing session URL.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="signerEmail">Signer's email.</param>
    /// <param name="returnUrl">URL to redirect after signing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Embedded signing URL.</returns>
    Task<SigningLink> GetEmbeddedSigningAsync(string requestId, string signerEmail, string returnUrl, CancellationToken cancellationToken = default);

    #endregion

    #region Document Operations

    /// <summary>
    /// Gets the signed document(s).
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="documentId">Optional specific document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Signed document bytes.</returns>
    Task<SignedDocument> GetSignedDocumentAsync(string requestId, string? documentId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the audit trail/certificate.
    /// </summary>
    /// <param name="requestId">The signature request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Audit trail document.</returns>
    Task<byte[]> GetAuditTrailAsync(string requestId, CancellationToken cancellationToken = default);

    #endregion

    #region Webhook Processing

    /// <summary>
    /// Processes an incoming webhook event.
    /// </summary>
    /// <param name="eventType">The webhook event type.</param>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">Webhook signature for validation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Processed signature event.</returns>
    Task<SignatureWebhookResult> ProcessWebhookAsync(string eventType, string payload, string? signature = null, CancellationToken cancellationToken = default);

    #endregion

    /// <summary>
    /// Gets the health status of the signature provider.
    /// </summary>
    Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default);
}

#endregion

#region Signature DTOs

/// <summary>
/// Signature template information.
/// </summary>
public class SignatureTemplate
{
    /// <summary>
    /// Provider-assigned template ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Template name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// CRM document type this template is for.
    /// </summary>
    public string? DocumentType { get; set; } // Quote, Contract, NDA, etc.

    /// <summary>
    /// Number of signature fields.
    /// </summary>
    public int SignerCount { get; set; }

    /// <summary>
    /// Signer role definitions.
    /// </summary>
    public List<SignerRole>? SignerRoles { get; set; }

    /// <summary>
    /// Template creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether template is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Signer role in a template.
/// </summary>
public class SignerRole
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int Order { get; set; }
    public bool IsRequired { get; set; } = true;
}

/// <summary>
/// Request to create a template.
/// </summary>
public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public byte[] DocumentContent { get; set; } = Array.Empty<byte>();
    public string DocumentName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public List<SignerRole>? SignerRoles { get; set; }
    public List<SignatureField>? Fields { get; set; }
}

/// <summary>
/// Signature field placement.
/// </summary>
public class SignatureField
{
    public string Type { get; set; } = "signature"; // signature, initials, date, text, checkbox
    public string? SignerRoleId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; } = 200;
    public int Height { get; set; } = 50;
    public bool IsRequired { get; set; } = true;
    public string? Label { get; set; }
}

/// <summary>
/// Request to create a signature request.
/// </summary>
public class CreateSignatureRequest
{
    /// <summary>
    /// Subject/title for the signing request.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Message to signers.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Template ID to use (if template-based).
    /// </summary>
    public string? TemplateId { get; set; }

    /// <summary>
    /// Documents to sign (if not using template).
    /// </summary>
    public List<SignatureDocument>? Documents { get; set; }

    /// <summary>
    /// Signers/recipients.
    /// </summary>
    public List<Signer> Signers { get; set; } = new();

    /// <summary>
    /// CC recipients (receive copy but don't sign).
    /// </summary>
    public List<SignatureCc>? CcRecipients { get; set; }

    /// <summary>
    /// Expiration date for the request.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// CRM entity reference.
    /// </summary>
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Template field values (for merge fields).
    /// </summary>
    public Dictionary<string, string>? FieldValues { get; set; }
}

/// <summary>
/// Document for signing.
/// </summary>
public class SignatureDocument
{
    public string Name { get; set; } = string.Empty;
    public byte[]? Content { get; set; }
    public string? ContentUrl { get; set; }
    public string ContentType { get; set; } = "application/pdf";
    public int Order { get; set; } = 1;
    public List<SignatureField>? Fields { get; set; }
}

/// <summary>
/// Signer information.
/// </summary>
public class Signer
{
    /// <summary>
    /// Signer's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Signer's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Signer's phone (for SMS delivery).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Role ID (if using template).
    /// </summary>
    public string? RoleId { get; set; }

    /// <summary>
    /// Signing order (for sequential signing).
    /// </summary>
    public int Order { get; set; } = 1;

    /// <summary>
    /// CRM Contact ID if linked.
    /// </summary>
    public int? ContactId { get; set; }

    /// <summary>
    /// Authentication method (email, sms, none).
    /// </summary>
    public string? AuthMethod { get; set; }
}

/// <summary>
/// CC recipient.
/// </summary>
public class SignatureCc
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// A signature request (envelope).
/// </summary>
public class SignatureRequest
{
    /// <summary>
    /// Provider-assigned request ID.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Request subject.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Current status.
    /// </summary>
    public SignatureStatus Status { get; set; }

    /// <summary>
    /// Signers with their status.
    /// </summary>
    public List<SignerStatus> Signers { get; set; } = new();

    /// <summary>
    /// Documents in the request.
    /// </summary>
    public List<SignatureDocumentInfo>? Documents { get; set; }

    /// <summary>
    /// CRM entity reference.
    /// </summary>
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Sent timestamp.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Completion timestamp.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Expiration timestamp.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Metadata.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Signature request status.
/// </summary>
public enum SignatureStatus
{
    Draft,
    Sent,
    Delivered,
    InProgress,
    Completed,
    Declined,
    Voided,
    Expired
}

/// <summary>
/// Individual signer status.
/// </summary>
public class SignerStatus
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // pending, viewed, signed, declined
    public DateTime? SentAt { get; set; }
    public DateTime? ViewedAt { get; set; }
    public DateTime? SignedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }
    public string? DeclineReason { get; set; }
    public int Order { get; set; }
}

/// <summary>
/// Document info in a signature request.
/// </summary>
public class SignatureDocumentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int PageCount { get; set; }
}

/// <summary>
/// Signing link for a signer.
/// </summary>
public class SigningLink
{
    /// <summary>
    /// The signing URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Link expiration.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this is an embedded signing URL.
    /// </summary>
    public bool IsEmbedded { get; set; }
}

/// <summary>
/// Signed document result.
/// </summary>
public class SignedDocument
{
    public string RequestId { get; set; } = string.Empty;
    public string? DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public DateTime SignedAt { get; set; }
}

/// <summary>
/// Signature webhook processing result.
/// </summary>
public class SignatureWebhookResult
{
    public bool Success { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public SignatureStatus? NewStatus { get; set; }
    public string? SignerEmail { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }

    /// <summary>
    /// CRM Activity to create.
    /// </summary>
    public SignatureActivityMapping? ActivityMapping { get; set; }

    public string? Error { get; set; }
}

/// <summary>
/// Mapping for creating CRM Activity from signature event.
/// </summary>
public class SignatureActivityMapping
{
    public string ActivityType { get; set; } = "DocumentSigned";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public string ExternalSource { get; set; } = string.Empty;
    public int? ContactId { get; set; }
    public int? AccountId { get; set; }
    public DateTime ActivityDate { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

#endregion
