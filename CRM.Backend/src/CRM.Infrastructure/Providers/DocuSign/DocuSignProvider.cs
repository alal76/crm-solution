// CRM Solution - DocuSign Provider
// Phase 4 Week 18: DocuSign e-signature provider implementation

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CRM.Core.Ports.Output.Providers;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

// Type aliases to resolve ambiguity with CRM.Core.Entities
using PortSignerStatus = CRM.Core.Ports.Output.Providers.SignerStatus;
using PortSignerRole = CRM.Core.Ports.Output.Providers.SignerRole;
using DocuSignSigner = DocuSign.eSign.Model.Signer;

namespace CRM.Infrastructure.Providers.DocuSign;

/// <summary>
/// DocuSign implementation of the ISignaturePort interface.
/// Provides e-signature functionality through the DocuSign eSignature API.
/// </summary>
public class DocuSignProvider : ISignaturePort
{
    private readonly DocuSignConfiguration _config;
    private readonly ILogger<DocuSignProvider> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    private DocuSignClient? _apiClient;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    /// <inheritdoc />
    public string ProviderName => "DocuSign";

    public DocuSignProvider(
        IOptions<DocuSignConfiguration> config,
        ILogger<DocuSignProvider> logger)
    {
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Only validate if configured (allow deferred configuration)
        if (!string.IsNullOrWhiteSpace(_config.IntegrationKey))
        {
            var (isValid, error) = _config.Validate();
            if (!isValid)
            {
                _logger.LogWarning("Invalid DocuSign configuration: {Error}", error);
            }
            else
            {
                _logger.LogInformation("DocuSignProvider initialized for account {AccountId} in {Environment} environment",
                    _config.AccountId, _config.Environment);
            }
        }
        else
        {
            _logger.LogInformation("DocuSignProvider initialized without configuration - will be unavailable until configured");
        }
    }

    #region Authentication

    private async Task<DocuSignClient> GetAuthenticatedClientAsync(CancellationToken cancellationToken = default)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Check if we have a valid token
            if (_apiClient != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
            {
                return _apiClient;
            }

            // Validate configuration
            var (isValid, error) = _config.Validate();
            if (!isValid)
            {
                throw new InvalidOperationException($"DocuSign not configured: {error}");
            }

            // Create new client and authenticate
            _apiClient = new DocuSignClient(_config.GetOAuthBaseUrl());

            var privateKeyBytes = _config.GetRsaPrivateKeyBytes();
            var scopes = _config.OAuthScopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            var tokenInfo = await Task.Run(() => _apiClient.RequestJWTUserToken(
                _config.IntegrationKey,
                _config.UserId,
                _config.GetOAuthBaseUrl(),
                privateKeyBytes,
                _config.JwtExpirationHours,
                scopes), cancellationToken);

            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenInfo.expires_in ?? 3600);

            // Update client configuration
            _apiClient.Configuration.DefaultHeader["Authorization"] = $"Bearer {tokenInfo.access_token}";
            _apiClient.SetBasePath(_config.GetApiBaseUrl());

            _logger.LogDebug("DocuSign JWT token refreshed, expires at {Expiry}", _tokenExpiry);

            return _apiClient;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    #endregion

    #region ISignaturePort Implementation

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if configured
            var (isValid, _) = _config.Validate();
            if (!isValid) return false;

            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var accountsApi = new AccountsApi(client);
            await Task.Run(() => accountsApi.GetAccountInformation(_config.AccountId), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DocuSign availability check failed");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SignatureTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var templatesApi = new TemplatesApi(client);

            var options = new TemplatesApi.ListTemplatesOptions
            {
                count = "100"
            };

            var templates = await Task.Run(() => 
                templatesApi.ListTemplates(_config.AccountId, options), cancellationToken);

            return templates.EnvelopeTemplates?.Select(MapToSignatureTemplate) 
                ?? Enumerable.Empty<SignatureTemplate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get templates from DocuSign");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var templatesApi = new TemplatesApi(client);

            var template = await Task.Run(() =>
                templatesApi.Get(_config.AccountId, templateId), cancellationToken);

            return template != null ? MapToSignatureTemplate(template) : null;
        }
        catch (ApiException ex) when (ex.ErrorCode == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {TemplateId} from DocuSign", templateId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureTemplate> CreateTemplateAsync(
        CreateTemplateRequest request, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var templatesApi = new TemplatesApi(client);

            var envelopeTemplate = new EnvelopeTemplate
            {
                Name = request.Name,
                Description = request.Description,
                EmailSubject = request.Name,
                Status = "created"
            };

            // Add document if provided
            if (request.DocumentContent?.Length > 0)
            {
                var document = new Document
                {
                    DocumentId = "1",
                    Name = request.DocumentName ?? request.Name,
                    FileExtension = request.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ? "pdf" : "docx",
                    DocumentBase64 = Convert.ToBase64String(request.DocumentContent)
                };
                envelopeTemplate.Documents = new List<Document> { document };
            }

            // Add template roles based on signer roles
            if (request.SignerRoles?.Any() == true)
            {
                envelopeTemplate.Recipients = new Recipients
                {
                    Signers = request.SignerRoles.Select((s, i) => new DocuSignSigner
                    {
                        RoleName = s.RoleName,
                        RecipientId = s.RoleId ?? (i + 1).ToString(),
                        RoutingOrder = s.Order.ToString()
                    }).ToList()
                };
            }

            var result = await Task.Run(() => 
                templatesApi.CreateTemplate(_config.AccountId, envelopeTemplate), cancellationToken);

            return new SignatureTemplate
            {
                Id = result.TemplateId,
                Name = request.Name,
                Description = request.Description,
                SignerCount = request.SignerRoles?.Count ?? 0,
                SignerRoles = request.SignerRoles,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template in DocuSign");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureRequest> CreateSignatureRequestAsync(
        CreateSignatureRequest request, 
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            var envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = request.Subject ?? "Please sign this document",
                EmailBlurb = request.Message,
                Status = "sent"
            };

            // Add template if specified
            if (!string.IsNullOrWhiteSpace(request.TemplateId))
            {
                envelopeDefinition.TemplateId = request.TemplateId;
                
                // Map template roles to signers
                if (request.Signers?.Any() == true)
                {
                    envelopeDefinition.TemplateRoles = request.Signers.Select(s => new TemplateRole
                    {
                        Email = s.Email,
                        Name = s.Name,
                        RoleName = s.RoleId ?? "Signer",
                        ClientUserId = _config.EnableEmbeddedSigning ? s.Email : null
                    }).ToList();
                }
            }
            else
            {
                // Add documents
                if (request.Documents?.Any() == true)
                {
                    envelopeDefinition.Documents = request.Documents.Select((doc, i) => new Document
                    {
                        DocumentId = (i + 1).ToString(),
                        Name = doc.Name,
                        FileExtension = Path.GetExtension(doc.Name)?.TrimStart('.') ?? "pdf",
                        DocumentBase64 = doc.Content != null 
                            ? Convert.ToBase64String(doc.Content) 
                            : null
                    }).ToList();
                }

                // Add signers
                if (request.Signers?.Any() == true)
                {
                    var signers = new List<DocuSignSigner>();
                    
                    for (int i = 0; i < request.Signers.Count; i++)
                    {
                        var s = request.Signers[i];
                        var signer = new DocuSignSigner
                        {
                            Email = s.Email,
                            Name = s.Name,
                            RecipientId = (i + 1).ToString(),
                            RoutingOrder = s.Order.ToString(),
                            ClientUserId = _config.EnableEmbeddedSigning ? s.Email : null
                        };

                        // Add signature tabs
                        signer.Tabs = new Tabs
                        {
                            SignHereTabs = new List<SignHere>
                            {
                                new SignHere
                                {
                                    DocumentId = "1",
                                    PageNumber = "1",
                                    AnchorString = "/sn1/",
                                    AnchorUnits = "pixels",
                                    AnchorXOffset = "0",
                                    AnchorYOffset = "0"
                                }
                            }
                        };

                        signers.Add(signer);
                    }

                    envelopeDefinition.Recipients = new Recipients { Signers = signers };
                }
            }

            // Add CC recipients
            if (request.CcRecipients?.Any() == true)
            {
                var ccList = request.CcRecipients.Select((cc, i) => new CarbonCopy
                {
                    Email = cc.Email,
                    Name = cc.Name,
                    RecipientId = (100 + i).ToString(),
                    RoutingOrder = "999"
                }).ToList();

                if (envelopeDefinition.Recipients == null)
                    envelopeDefinition.Recipients = new Recipients();
                
                envelopeDefinition.Recipients.CarbonCopies = ccList;
            }

            // Set expiration
            var expiryDays = request.ExpiresAt.HasValue 
                ? (int)(request.ExpiresAt.Value - DateTime.UtcNow).TotalDays 
                : _config.DefaultExpirationDays;
                
            if (expiryDays > 0)
            {
                envelopeDefinition.Notification = new Notification
                {
                    Expirations = new Expirations
                    {
                        ExpireEnabled = "true",
                        ExpireAfter = expiryDays.ToString(),
                        ExpireWarn = Math.Min(expiryDays - 1, _config.DefaultReminderDays).ToString()
                    }
                };
            }

            // Add custom metadata (entity reference)
            if (!string.IsNullOrWhiteSpace(request.EntityType) && request.EntityId.HasValue)
            {
                envelopeDefinition.CustomFields = new CustomFields
                {
                    TextCustomFields = new List<TextCustomField>
                    {
                        new TextCustomField { Name = "entityType", Value = request.EntityType },
                        new TextCustomField { Name = "entityId", Value = request.EntityId.Value.ToString() }
                    }
                };
            }

            var result = await Task.Run(() => 
                envelopesApi.CreateEnvelope(_config.AccountId, envelopeDefinition), cancellationToken);

            return new SignatureRequest
            {
                Id = result.EnvelopeId,
                Subject = request.Subject ?? "Please sign this document",
                Status = MapEnvelopeStatus(result.Status),
                Signers = request.Signers?.Select(s => new PortSignerStatus
                {
                    Name = s.Name,
                    Email = s.Email,
                    Status = "pending",
                    Order = s.Order
                }).ToList() ?? new List<PortSignerStatus>(),
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                CreatedAt = DateTime.UtcNow,
                SentAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt ?? (expiryDays > 0 ? DateTime.UtcNow.AddDays(expiryDays) : null),
                Metadata = request.Metadata
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create signature request in DocuSign");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureRequest?> GetSignatureRequestAsync(
        string requestId, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            var envelope = await Task.Run(() => 
                envelopesApi.GetEnvelope(_config.AccountId, requestId), cancellationToken);

            if (envelope == null) return null;

            // Get recipients for signer status
            var recipients = await Task.Run(() => 
                envelopesApi.ListRecipients(_config.AccountId, requestId), cancellationToken);

            return MapToSignatureRequest(envelope, recipients);
        }
        catch (ApiException ex) when (ex.ErrorCode == 404)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signature request {RequestId} from DocuSign", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureStatus> GetStatusAsync(
        string requestId, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            var envelope = await Task.Run(() => 
                envelopesApi.GetEnvelope(_config.AccountId, requestId), cancellationToken);

            return MapEnvelopeStatus(envelope?.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for {RequestId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SignatureRequest>> GetByEntityAsync(
        string entityType, 
        int entityId, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Search by custom field
            var options = new EnvelopesApi.ListStatusChangesOptions
            {
                customField = $"entityType={entityType},entityId={entityId}",
                fromDate = DateTime.UtcNow.AddYears(-1).ToString("o")
            };

            var envelopes = await Task.Run(() => 
                envelopesApi.ListStatusChanges(_config.AccountId, options), cancellationToken);

            var requests = new List<SignatureRequest>();
            foreach (var envelope in envelopes.Envelopes ?? Enumerable.Empty<Envelope>())
            {
                var recipients = await Task.Run(() => 
                    envelopesApi.ListRecipients(_config.AccountId, envelope.EnvelopeId), cancellationToken);
                
                var request = MapToSignatureRequest(envelope, recipients);
                request.EntityType = entityType;
                request.EntityId = entityId;
                requests.Add(request);
            }

            return requests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signature requests for {EntityType}/{EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CancelSignatureRequestAsync(
        string requestId, 
        string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            var envelope = new Envelope
            {
                Status = "voided",
                VoidedReason = reason ?? "Cancelled by CRM"
            };

            await Task.Run(() => 
                envelopesApi.Update(_config.AccountId, requestId, envelope), cancellationToken);

            _logger.LogInformation("Cancelled DocuSign envelope {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel signature request {RequestId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendReminderAsync(
        string requestId, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Get envelope to check status
            var envelope = await Task.Run(() => 
                envelopesApi.GetEnvelope(_config.AccountId, requestId), cancellationToken);

            if (envelope.Status != "sent" && envelope.Status != "delivered")
            {
                throw new InvalidOperationException($"Cannot send reminder for envelope in '{envelope.Status}' status");
            }

            // Use notification API to send reminder
            await Task.Run(() => 
                envelopesApi.UpdateNotificationSettings(_config.AccountId, requestId), cancellationToken);

            _logger.LogInformation("Sent reminder for DocuSign envelope {RequestId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder for {RequestId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SigningLink> GetSigningLinkAsync(
        string requestId, 
        string signerEmail, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Get recipients to find the signer's recipient ID
            var recipients = await Task.Run(() => 
                envelopesApi.ListRecipients(_config.AccountId, requestId), cancellationToken);

            var signer = recipients.Signers?.FirstOrDefault(s => 
                s.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase));

            if (signer == null)
            {
                throw new InvalidOperationException($"Signer with email {signerEmail} not found in envelope {requestId}");
            }

            // If the signer has a ClientUserId, we can use embedded signing
            if (!string.IsNullOrEmpty(signer.ClientUserId))
            {
                var viewRequest = new RecipientViewRequest
                {
                    ReturnUrl = "https://www.docusign.com/",
                    AuthenticationMethod = "none",
                    Email = signer.Email,
                    UserName = signer.Name,
                    ClientUserId = signer.ClientUserId,
                    RecipientId = signer.RecipientId
                };

                var viewUrl = await Task.Run(() => 
                    envelopesApi.CreateRecipientView(_config.AccountId, requestId, viewRequest), cancellationToken);

                return new SigningLink
                {
                    Url = viewUrl.Url,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    IsEmbedded = true
                };
            }

            // For non-embedded signing, the signer will receive an email from DocuSign
            // Return a URL to the DocuSign portal where they can access pending documents
            return new SigningLink
            {
                Url = $"https://app.docusign.com/sign/{requestId}",
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsEmbedded = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signing link for {RequestId}, signer {SignerEmail}", requestId, signerEmail);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SigningLink> GetEmbeddedSigningAsync(
        string requestId, 
        string signerEmail,
        string returnUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Get recipient info
            var recipients = await Task.Run(() => 
                envelopesApi.ListRecipients(_config.AccountId, requestId), cancellationToken);

            var signer = recipients.Signers?.FirstOrDefault(s => 
                s.Email.Equals(signerEmail, StringComparison.OrdinalIgnoreCase));

            if (signer == null)
            {
                throw new InvalidOperationException($"Signer with email {signerEmail} not found in envelope {requestId}");
            }

            var viewRequest = new RecipientViewRequest
            {
                ReturnUrl = returnUrl,
                AuthenticationMethod = "none",
                Email = signer.Email,
                UserName = signer.Name,
                ClientUserId = signer.ClientUserId ?? signer.Email,
                RecipientId = signer.RecipientId
            };

            var viewUrl = await Task.Run(() => 
                envelopesApi.CreateRecipientView(_config.AccountId, requestId, viewRequest), cancellationToken);

            return new SigningLink
            {
                Url = viewUrl.Url,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsEmbedded = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get embedded signing URL for {RequestId}, signer {SignerEmail}", requestId, signerEmail);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignedDocument> GetSignedDocumentAsync(
        string requestId, 
        string? documentId = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Get envelope info for metadata
            var envelope = await Task.Run(() => 
                envelopesApi.GetEnvelope(_config.AccountId, requestId), cancellationToken);

            // Get combined document or specific document
            Stream docStream;
            string fileName;
            
            if (string.IsNullOrWhiteSpace(documentId))
            {
                docStream = await Task.Run(() => 
                    envelopesApi.GetDocument(_config.AccountId, requestId, "combined"), cancellationToken);
                fileName = $"{envelope.EmailSubject ?? "signed_document"}_combined.pdf";
            }
            else
            {
                docStream = await Task.Run(() => 
                    envelopesApi.GetDocument(_config.AccountId, requestId, documentId), cancellationToken);
                fileName = $"document_{documentId}.pdf";
            }

            using var ms = new MemoryStream();
            await docStream.CopyToAsync(ms, cancellationToken);

            return new SignedDocument
            {
                RequestId = requestId,
                DocumentId = documentId,
                FileName = fileName,
                ContentType = "application/pdf",
                Content = ms.ToArray(),
                SignedAt = DateTime.TryParse(envelope.CompletedDateTime, out var completed) 
                    ? completed 
                    : DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signed document for {RequestId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> GetAuditTrailAsync(
        string requestId, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var envelopesApi = new EnvelopesApi(client);

            // Get the certificate of completion (audit trail document)
            var docStream = await Task.Run(() => 
                envelopesApi.GetDocument(_config.AccountId, requestId, "certificate"), cancellationToken);

            using var ms = new MemoryStream();
            await docStream.CopyToAsync(ms, cancellationToken);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit trail for {RequestId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureWebhookResult> ProcessWebhookAsync(
        string eventType,
        string payload, 
        string? signature = null, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        try
        {
            // Validate signature if webhook secret is configured
            if (!string.IsNullOrWhiteSpace(_config.WebhookSecret) && !string.IsNullOrWhiteSpace(signature))
            {
                using var hmac = new System.Security.Cryptography.HMACSHA256(
                    Encoding.UTF8.GetBytes(_config.WebhookSecret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = Convert.ToBase64String(hash);

                if (!computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("DocuSign webhook signature validation failed");
                    return new SignatureWebhookResult
                    {
                        Success = false,
                        EventType = eventType,
                        Error = "Invalid webhook signature"
                    };
                }
            }

            // Parse the webhook payload
            var webhookData = JsonSerializer.Deserialize<DocuSignWebhookPayload>(payload, _jsonOptions);

            if (webhookData == null)
            {
                return new SignatureWebhookResult
                {
                    Success = false,
                    EventType = eventType,
                    Error = "Failed to parse webhook payload"
                };
            }

            var result = new SignatureWebhookResult
            {
                Success = true,
                EventType = webhookData.Status ?? eventType,
                RequestId = webhookData.EnvelopeId ?? string.Empty,
                NewStatus = MapEnvelopeStatus(webhookData.Status)
            };

            // Extract signer info if available
            if (webhookData.RecipientStatuses?.Any() == true)
            {
                var signer = webhookData.RecipientStatuses.FirstOrDefault();
                if (signer != null)
                {
                    result.SignerEmail = signer.Email;
                }
            }

            // Create activity mapping for timeline
            result.ActivityMapping = new SignatureActivityMapping
            {
                ActivityType = GetActivityTypeForStatus(webhookData.Status),
                Title = $"Document {webhookData.Status}: {webhookData.EnvelopeId}",
                Description = $"DocuSign envelope status changed to {webhookData.Status}",
                ExternalId = $"docusign:{webhookData.EnvelopeId}:{webhookData.Status}",
                ExternalSource = "DocuSign",
                ActivityDate = webhookData.StatusChangedDateTime ?? DateTime.UtcNow
            };

            _logger.LogInformation("Processed DocuSign webhook for envelope {EnvelopeId}, status: {Status}",
                result.RequestId, result.EventType);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process DocuSign webhook");
            return new SignatureWebhookResult
            {
                Success = false,
                EventType = eventType,
                Error = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var result = new ProviderHealthResult
        {
            ProviderName = ProviderName,
            CheckedAt = DateTime.UtcNow,
            Details = new Dictionary<string, object>()
        };

        try
        {
            // Check configuration first
            var (isValid, error) = _config.Validate();
            if (!isValid)
            {
                result.IsHealthy = false;
                result.Message = $"Not configured: {error}";
                return result;
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var client = await GetAuthenticatedClientAsync(cancellationToken);
            var accountsApi = new AccountsApi(client);
            
            var accountInfo = await Task.Run(() => 
                accountsApi.GetAccountInformation(_config.AccountId), cancellationToken);

            stopwatch.Stop();

            result.IsHealthy = true;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.Message = "DocuSign is available";
            result.Details["account_name"] = accountInfo.AccountName ?? "Unknown";
            result.Details["plan_name"] = accountInfo.PlanName ?? "Unknown";
            result.Details["environment"] = _config.Environment;
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"DocuSign health check failed: {ex.Message}";
            _logger.LogWarning(ex, "DocuSign health check failed");
        }

        return result;
    }

    #endregion

    #region Private Helpers

    private static string GetActivityTypeForStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "completed" => "DocumentSigned",
            "declined" => "DocumentDeclined",
            "voided" => "DocumentVoided",
            "sent" => "DocumentSent",
            "delivered" => "DocumentDelivered",
            _ => "DocumentStatusChanged"
        };
    }

    private static SignatureTemplate MapToSignatureTemplate(EnvelopeTemplate template)
    {
        return new SignatureTemplate
        {
            Id = template.TemplateId,
            Name = template.Name,
            Description = template.Description ?? string.Empty,
            SignerCount = template.Recipients?.Signers?.Count ?? 0,
            SignerRoles = template.Recipients?.Signers?.Select(s => new PortSignerRole
            {
                RoleId = s.RecipientId,
                RoleName = s.RoleName ?? $"Signer{s.RecipientId}",
                Order = int.TryParse(s.RoutingOrder, out var order) ? order : 1,
                IsRequired = true
            }).ToList(),
            CreatedAt = DateTime.TryParse(template.Created, out var created) ? created : DateTime.UtcNow,
            IsActive = true
        };
    }

    private SignatureRequest MapToSignatureRequest(Envelope envelope, Recipients? recipients = null)
    {
        var request = new SignatureRequest
        {
            Id = envelope.EnvelopeId,
            Subject = envelope.EmailSubject ?? string.Empty,
            Status = MapEnvelopeStatus(envelope.Status),
            CreatedAt = DateTime.TryParse(envelope.CreatedDateTime, out var created) ? created : DateTime.UtcNow,
            SentAt = DateTime.TryParse(envelope.SentDateTime, out var sent) ? sent : null,
            CompletedAt = DateTime.TryParse(envelope.CompletedDateTime, out var completed) ? completed : null,
            ExpiresAt = DateTime.TryParse(envelope.ExpireDateTime, out var expires) ? expires : null
        };

        // Map signers
        if (recipients?.Signers?.Any() == true)
        {
            request.Signers = recipients.Signers.Select(s => new PortSignerStatus
            {
                Email = s.Email,
                Name = s.Name,
                Status = s.Status?.ToLowerInvariant() ?? "pending",
                SentAt = DateTime.TryParse(s.SentDateTime, out var signerSent) ? signerSent : null,
                ViewedAt = DateTime.TryParse(s.DeliveredDateTime, out var viewed) ? viewed : null,
                SignedAt = DateTime.TryParse(s.SignedDateTime, out var signed) ? signed : null,
                DeclinedAt = s.Status?.ToLowerInvariant() == "declined" 
                    ? DateTime.TryParse(s.DeclinedDateTime, out var declined) ? declined : DateTime.UtcNow 
                    : null,
                DeclineReason = s.DeclinedReason,
                Order = int.TryParse(s.RoutingOrder, out var order) ? order : 1
            }).ToList();
        }

        // Map documents
        var docs = envelope.EnvelopeDocuments;
        if (docs?.Any() == true)
        {
            request.Documents = docs.Select(d => new SignatureDocumentInfo
            {
                Id = d.DocumentId,
                Name = d.Name ?? $"Document {d.DocumentId}",
                Order = int.TryParse(d.Order, out var docOrder) ? docOrder : 1,
                PageCount = d.Pages?.Count ?? 0
            }).ToList();
        }

        return request;
    }

    private static SignatureStatus MapEnvelopeStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "completed" => SignatureStatus.Completed,
            "declined" => SignatureStatus.Declined,
            "voided" => SignatureStatus.Voided,
            "sent" => SignatureStatus.Sent,
            "delivered" => SignatureStatus.Delivered,
            "created" => SignatureStatus.Draft,
            "expired" => SignatureStatus.Expired,
            "signed" => SignatureStatus.InProgress,
            _ => SignatureStatus.Sent
        };
    }

    #endregion

    #region Internal Types

    private class DocuSignWebhookPayload
    {
        public string? EnvelopeId { get; set; }
        public string? Status { get; set; }
        public DateTime? StatusChangedDateTime { get; set; }
        public List<DocuSignRecipientStatus>? RecipientStatuses { get; set; }
    }

    private class DocuSignRecipientStatus
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? Status { get; set; }
        public DateTime? SignedDateTime { get; set; }
    }

    #endregion
}
