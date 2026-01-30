using CRM.Core.Models;

namespace CRM.Core.Entities;

#region E-Signature Enumerations

/// <summary>
/// FUNCTIONAL: E-signature request status.
/// TECHNICAL: Tracks document signing lifecycle.
/// </summary>
public enum ESignatureStatus
{
    /// <summary>Request created, not yet sent</summary>
    Draft = 0,
    
    /// <summary>Sent to signers</summary>
    Sent = 1,
    
    /// <summary>Viewed by at least one signer</summary>
    Viewed = 2,
    
    /// <summary>Partially signed</summary>
    PartiallySigned = 3,
    
    /// <summary>Fully signed/completed</summary>
    Completed = 4,
    
    /// <summary>Declined by a signer</summary>
    Declined = 5,
    
    /// <summary>Voided by sender</summary>
    Voided = 6,
    
    /// <summary>Expired</summary>
    Expired = 7,
    
    /// <summary>Authentication failed</summary>
    AuthenticationFailed = 8,
    
    /// <summary>Delivery failed</summary>
    DeliveryFailed = 9
}

/// <summary>
/// FUNCTIONAL: E-signature provider.
/// TECHNICAL: Determines integration to use.
/// </summary>
public enum ESignatureProvider
{
    /// <summary>DocuSign</summary>
    DocuSign = 0,
    
    /// <summary>Adobe Sign</summary>
    AdobeSign = 1,
    
    /// <summary>HelloSign</summary>
    HelloSign = 2,
    
    /// <summary>PandaDoc</summary>
    PandaDoc = 3,
    
    /// <summary>SignNow</summary>
    SignNow = 4,
    
    /// <summary>Built-in simple signature</summary>
    BuiltIn = 5
}

/// <summary>
/// FUNCTIONAL: Document type for signing.
/// TECHNICAL: Categorizes documents.
/// </summary>
public enum SignableDocumentType
{
    /// <summary>Quote/proposal</summary>
    Quote = 0,
    
    /// <summary>Contract</summary>
    Contract = 1,
    
    /// <summary>Order form</summary>
    OrderForm = 2,
    
    /// <summary>NDA</summary>
    NDA = 3,
    
    /// <summary>Statement of work</summary>
    SOW = 4,
    
    /// <summary>Master service agreement</summary>
    MSA = 5,
    
    /// <summary>Amendment</summary>
    Amendment = 6,
    
    /// <summary>Renewal</summary>
    Renewal = 7,
    
    /// <summary>Other</summary>
    Other = 8
}

/// <summary>
/// FUNCTIONAL: Signer status.
/// TECHNICAL: Tracks individual signer progress.
/// </summary>
public enum SignerStatus
{
    /// <summary>Waiting to sign (not their turn yet)</summary>
    Waiting = 0,
    
    /// <summary>Pending - their turn to sign</summary>
    Pending = 1,
    
    /// <summary>Sent email notification</summary>
    Sent = 2,
    
    /// <summary>Delivered successfully</summary>
    Delivered = 3,
    
    /// <summary>Viewed document</summary>
    Viewed = 4,
    
    /// <summary>Signed</summary>
    Signed = 5,
    
    /// <summary>Declined to sign</summary>
    Declined = 6,
    
    /// <summary>Delivery failed</summary>
    DeliveryFailed = 7,
    
    /// <summary>Authentication failed</summary>
    AuthFailed = 8
}

/// <summary>
/// FUNCTIONAL: Signer role/type.
/// TECHNICAL: Determines signing order and requirements.
/// </summary>
public enum SignerRole
{
    /// <summary>Primary signer (customer)</summary>
    Signer = 0,
    
    /// <summary>Co-signer</summary>
    CoSigner = 1,
    
    /// <summary>Internal counter-signer</summary>
    CounterSigner = 2,
    
    /// <summary>Carbon copy (no signature required)</summary>
    CarbonCopy = 3,
    
    /// <summary>Witness</summary>
    Witness = 4,
    
    /// <summary>Approver (internal)</summary>
    Approver = 5,
    
    /// <summary>In-person signer</summary>
    InPersonSigner = 6
}

#endregion

/// <summary>
/// E-signature request for document signing.
/// Integrates with DocuSign, Adobe Sign, etc.
/// </summary>
public class ESignatureRequest : BaseEntity
{
    #region Identification
    
    /// <summary>Request number</summary>
    public string RequestNumber { get; set; } = string.Empty;
    
    /// <summary>External envelope/request ID from provider</summary>
    public string? ExternalEnvelopeId { get; set; }
    
    /// <summary>E-signature provider</summary>
    public ESignatureProvider Provider { get; set; } = ESignatureProvider.DocuSign;
    
    #endregion
    
    #region Document Details
    
    /// <summary>Document name/title</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Document type</summary>
    public SignableDocumentType DocumentType { get; set; } = SignableDocumentType.Quote;
    
    /// <summary>Email subject line</summary>
    public string? EmailSubject { get; set; }
    
    /// <summary>Email message body</summary>
    public string? EmailMessage { get; set; }
    
    /// <summary>Current status</summary>
    public ESignatureStatus Status { get; set; } = ESignatureStatus.Draft;
    
    #endregion
    
    #region Dates
    
    /// <summary>Date created</summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Date sent for signing</summary>
    public DateTime? SentDate { get; set; }
    
    /// <summary>Expiration date</summary>
    public DateTime? ExpirationDate { get; set; }
    
    /// <summary>Date completed (fully signed)</summary>
    public DateTime? CompletedDate { get; set; }
    
    /// <summary>Date voided</summary>
    public DateTime? VoidedDate { get; set; }
    
    /// <summary>Date declined</summary>
    public DateTime? DeclinedDate { get; set; }
    
    /// <summary>Last status update</summary>
    public DateTime? LastStatusUpdate { get; set; }
    
    #endregion
    
    #region Settings
    
    /// <summary>Days until expiration</summary>
    public int ExpirationDays { get; set; } = 30;
    
    /// <summary>Send reminder every N days</summary>
    public int? ReminderDays { get; set; }
    
    /// <summary>Reminders sent count</summary>
    public int RemindersSent { get; set; } = 0;
    
    /// <summary>Require signing order</summary>
    public bool RequireSigningOrder { get; set; } = true;
    
    /// <summary>Allow decline</summary>
    public bool AllowDecline { get; set; } = true;
    
    /// <summary>Allow comments</summary>
    public bool AllowComments { get; set; } = true;
    
    /// <summary>Authentication required (SMS, ID, etc.)</summary>
    public string? AuthenticationMethod { get; set; }
    
    #endregion
    
    #region Documents
    
    /// <summary>Source document URL (PDF to sign)</summary>
    public string? SourceDocumentUrl { get; set; }
    
    /// <summary>Signed document URL (completed PDF)</summary>
    public string? SignedDocumentUrl { get; set; }
    
    /// <summary>Certificate of completion URL</summary>
    public string? CertificateUrl { get; set; }
    
    /// <summary>Audit trail URL</summary>
    public string? AuditTrailUrl { get; set; }
    
    #endregion
    
    #region Results
    
    /// <summary>Decline reason</summary>
    public string? DeclineReason { get; set; }
    
    /// <summary>Void reason</summary>
    public string? VoidReason { get; set; }
    
    /// <summary>Error message if failed</summary>
    public string? ErrorMessage { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Quote ID</summary>
    public int? QuoteId { get; set; }
    
    /// <summary>Navigation to quote</summary>
    public Quote? Quote { get; set; }
    
    /// <summary>Order ID</summary>
    public int? OrderId { get; set; }
    
    /// <summary>Navigation to order</summary>
    public Order? Order { get; set; }
    
    /// <summary>Account ID</summary>
    public int? AccountId { get; set; }
    
    /// <summary>Navigation to account</summary>
    public Account? Account { get; set; }
    
    /// <summary>Opportunity ID</summary>
    public int? OpportunityId { get; set; }
    
    /// <summary>Navigation to opportunity</summary>
    public Opportunity? Opportunity { get; set; }
    
    /// <summary>Created by user ID</summary>
    public int? CreatedById { get; set; }
    
    /// <summary>Navigation to creator</summary>
    public User? CreatedBy { get; set; }
    
    /// <summary>Voided by user ID</summary>
    public int? VoidedById { get; set; }
    
    /// <summary>Navigation to voider</summary>
    public User? VoidedBy { get; set; }
    
    /// <summary>Signers</summary>
    public ICollection<ESignatureSigner> Signers { get; set; } = new List<ESignatureSigner>();
    
    /// <summary>Documents in envelope</summary>
    public ICollection<ESignatureDocument> Documents { get; set; } = new List<ESignatureDocument>();
    
    /// <summary>Audit events</summary>
    public ICollection<ESignatureAuditEvent> AuditEvents { get; set; } = new List<ESignatureAuditEvent>();
    
    #endregion
}

/// <summary>
/// Individual signer in an e-signature request.
/// </summary>
public class ESignatureSigner : BaseEntity
{
    #region Signer Details
    
    /// <summary>Signing order (1 = first)</summary>
    public int SigningOrder { get; set; } = 1;
    
    /// <summary>Signer role</summary>
    public SignerRole Role { get; set; } = SignerRole.Signer;
    
    /// <summary>Current status</summary>
    public SignerStatus Status { get; set; } = SignerStatus.Waiting;
    
    /// <summary>External recipient ID from provider</summary>
    public string? ExternalRecipientId { get; set; }
    
    #endregion
    
    #region Contact Information
    
    /// <summary>Signer name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Signer email</summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>Phone (for SMS auth)</summary>
    public string? Phone { get; set; }
    
    /// <summary>Title/role</summary>
    public string? Title { get; set; }
    
    /// <summary>Company</summary>
    public string? Company { get; set; }
    
    #endregion
    
    #region Dates
    
    /// <summary>Date sent</summary>
    public DateTime? SentDate { get; set; }
    
    /// <summary>Date delivered</summary>
    public DateTime? DeliveredDate { get; set; }
    
    /// <summary>Date viewed</summary>
    public DateTime? ViewedDate { get; set; }
    
    /// <summary>Date signed</summary>
    public DateTime? SignedDate { get; set; }
    
    /// <summary>Date declined</summary>
    public DateTime? DeclinedDate { get; set; }
    
    #endregion
    
    #region Signature Details
    
    /// <summary>Signature image URL</summary>
    public string? SignatureImageUrl { get; set; }
    
    /// <summary>IP address when signed</summary>
    public string? SignedFromIp { get; set; }
    
    /// <summary>Location when signed</summary>
    public string? SignedFromLocation { get; set; }
    
    /// <summary>Decline reason</summary>
    public string? DeclineReason { get; set; }
    
    #endregion
    
    #region Private Message
    
    /// <summary>Private message to this signer</summary>
    public string? PrivateMessage { get; set; }
    
    #endregion
    
    #region Relationships
    
    /// <summary>Parent request ID</summary>
    public int ESignatureRequestId { get; set; }
    
    /// <summary>Navigation to request</summary>
    public ESignatureRequest? ESignatureRequest { get; set; }
    
    /// <summary>Contact ID (if linked)</summary>
    public int? ContactId { get; set; }
    
    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }
    
    /// <summary>User ID (for internal signers)</summary>
    public int? UserId { get; set; }
    
    /// <summary>Navigation to user</summary>
    public User? User { get; set; }
    
    #endregion
}

/// <summary>
/// Document within an e-signature envelope.
/// </summary>
public class ESignatureDocument : BaseEntity
{
    /// <summary>Document name</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Document order</summary>
    public int DocumentOrder { get; set; } = 1;
    
    /// <summary>External document ID</summary>
    public string? ExternalDocumentId { get; set; }
    
    /// <summary>Document URL</summary>
    public string? DocumentUrl { get; set; }
    
    /// <summary>File type (pdf, docx)</summary>
    public string? FileType { get; set; }
    
    /// <summary>File size in bytes</summary>
    public long? FileSize { get; set; }
    
    /// <summary>Page count</summary>
    public int? PageCount { get; set; }
    
    /// <summary>Parent request ID</summary>
    public int ESignatureRequestId { get; set; }
    
    /// <summary>Navigation to request</summary>
    public ESignatureRequest? ESignatureRequest { get; set; }
}

/// <summary>
/// Audit event for e-signature tracking.
/// </summary>
public class ESignatureAuditEvent : BaseEntity
{
    /// <summary>Event type</summary>
    public string EventType { get; set; } = string.Empty;
    
    /// <summary>Event timestamp</summary>
    public DateTime EventDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Event description</summary>
    public string? Description { get; set; }
    
    /// <summary>IP address</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>User agent</summary>
    public string? UserAgent { get; set; }
    
    /// <summary>Location</summary>
    public string? Location { get; set; }
    
    /// <summary>Associated signer ID</summary>
    public int? ESignatureSignerId { get; set; }
    
    /// <summary>Navigation to signer</summary>
    public ESignatureSigner? ESignatureSigner { get; set; }
    
    /// <summary>Parent request ID</summary>
    public int ESignatureRequestId { get; set; }
    
    /// <summary>Navigation to request</summary>
    public ESignatureRequest? ESignatureRequest { get; set; }
}
