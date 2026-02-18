// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CRM.Core.Ports.Output.Providers;

// Resolve naming conflict: SignerStatus class from Ports vs SignerStatus enum from Entities
using PortSignerStatus = CRM.Core.Ports.Output.Providers.SignerStatus;
// Resolve naming conflict: SignerRole class from Ports vs SignerRole entity from Entities
using PortSignerRole = CRM.Core.Ports.Output.Providers.SignerRole;

namespace CRM.Infrastructure.Providers.DocuSeal;

/// <summary>
/// DocuSeal provider for electronic signature operations.
/// Integrates with DocuSeal's REST API for template-based document signing.
///
/// Features:
/// - Template management (list, get, create from documents)
/// - Submission creation (signature requests)
/// - Embedded and email-based signing
/// - Webhook processing for real-time status updates
/// - Signed document retrieval
///
/// DocuSeal is an open-source alternative to DocuSign, self-hostable.
/// </summary>
public class DocuSealProvider : ISignaturePort
{
    private readonly HttpClient _httpClient;
    private readonly DocuSealConfiguration _config;
    private readonly ILogger<DocuSealProvider> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <inheritdoc />
    public string ProviderName => "DocuSeal";

    public DocuSealProvider(
        HttpClient httpClient,
        IOptions<DocuSealConfiguration> config,
        ILogger<DocuSealProvider> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure JSON serialization for DocuSeal API (snake_case)
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Validate configuration
        var (isValid, error) = _config.Validate();
        if (!isValid)
        {
            _logger.LogError("Invalid DocuSeal configuration: {Error}", error);
            throw new InvalidOperationException($"Invalid DocuSeal configuration: {error}");
        }

        _logger.LogInformation("DocuSealProvider initialized for {Url}", _config.Url);
    }

    /// <inheritdoc />
    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/templates", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "DocuSeal availability check failed");
            return false;
        }
    }

    #region Template Management

    /// <inheritdoc />
    public async Task<IEnumerable<SignatureTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/templates", cancellationToken);
            response.EnsureSuccessStatusCode();

            var docuSealTemplates = await response.Content.ReadFromJsonAsync<List<DocuSealTemplate>>(_jsonOptions, cancellationToken);

            return docuSealTemplates?.Select(MapToSignatureTemplate) ?? Enumerable.Empty<SignatureTemplate>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get templates from DocuSeal");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureTemplate?> GetTemplateAsync(string templateId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateId);

        try
        {
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/templates/{templateId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var docuSealTemplate = await response.Content.ReadFromJsonAsync<DocuSealTemplate>(_jsonOptions, cancellationToken);
            return docuSealTemplate != null ? MapToSignatureTemplate(docuSealTemplate) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get template {TemplateId} from DocuSeal", templateId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureTemplate> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);

        try
        {
            // DocuSeal expects multipart form data for template creation with documents
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(request.Name), "name");

            if (!string.IsNullOrEmpty(request.Description))
            {
                content.Add(new StringContent(request.Description), "description");
            }

            // Add document as file
            if (request.DocumentContent?.Length > 0)
            {
                var fileContent = new ByteArrayContent(request.DocumentContent);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.ContentType);
                content.Add(fileContent, "documents[]", request.DocumentName);
            }

            var response = await _httpClient.PostAsync($"{_config.GetApiBaseUrl()}/templates", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var docuSealTemplate = await response.Content.ReadFromJsonAsync<DocuSealTemplate>(_jsonOptions, cancellationToken);

            if (docuSealTemplate == null)
            {
                throw new InvalidOperationException("DocuSeal returned null template");
            }

            _logger.LogInformation("Created DocuSeal template: {TemplateId} - {Name}", docuSealTemplate.Id, request.Name);
            return MapToSignatureTemplate(docuSealTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create template in DocuSeal: {Name}", request.Name);
            throw;
        }
    }

    #endregion

    #region Signature Request Operations

    /// <inheritdoc />
    public async Task<SignatureRequest> CreateSignatureRequestAsync(CreateSignatureRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.TemplateId) && (request.Documents == null || !request.Documents.Any()))
        {
            throw new ArgumentException("Either TemplateId or Documents must be provided", nameof(request));
        }

        if (!request.Signers.Any())
        {
            throw new ArgumentException("At least one signer is required", nameof(request));
        }

        try
        {
            // Build DocuSeal submission request
            var docuSealRequest = new DocuSealSubmissionRequest
            {
                TemplateId = int.TryParse(request.TemplateId, out var tid) ? tid : 0,
                SendEmail = !_config.EnableEmbedSigning, // If embedded, don't send email
                Message = request.Message,
                Submitters = request.Signers.Select(s => new DocuSealSubmitter
                {
                    Name = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    Role = s.RoleId ?? "Signer",
                    ExternalId = s.ContactId?.ToString()
                }).ToList(),
                Metadata = new Dictionary<string, object>
                {
                    ["crm_entity_type"] = request.EntityType ?? "",
                    ["crm_entity_id"] = request.EntityId ?? 0,
                    ["subject"] = request.Subject
                }
            };

            // Add field values if provided
            if (request.FieldValues?.Any() == true)
            {
                docuSealRequest.Values = request.FieldValues;
            }

            // Set expiration
            if (request.ExpiresAt.HasValue)
            {
                docuSealRequest.ExpireAt = request.ExpiresAt.Value.ToString("yyyy-MM-dd");
            }
            else if (_config.DefaultExpirationDays > 0)
            {
                docuSealRequest.ExpireAt = DateTime.UtcNow.AddDays(_config.DefaultExpirationDays).ToString("yyyy-MM-dd");
            }

            var response = await _httpClient.PostAsJsonAsync(
                $"{_config.GetApiBaseUrl()}/submissions",
                docuSealRequest,
                _jsonOptions,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var docuSealSubmission = await response.Content.ReadFromJsonAsync<DocuSealSubmission>(_jsonOptions, cancellationToken);

            if (docuSealSubmission == null)
            {
                throw new InvalidOperationException("DocuSeal returned null submission");
            }

            _logger.LogInformation("Created DocuSeal submission: {SubmissionId} for template {TemplateId}",
                docuSealSubmission.Id, request.TemplateId);

            return MapToSignatureRequest(docuSealSubmission, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create submission in DocuSeal");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureRequest?> GetSignatureRequestAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/submissions/{requestId}", cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var docuSealSubmission = await response.Content.ReadFromJsonAsync<DocuSealSubmission>(_jsonOptions, cancellationToken);
            return docuSealSubmission != null ? MapToSignatureRequest(docuSealSubmission, null) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submission {SubmissionId} from DocuSeal", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SignatureStatus> GetStatusAsync(string requestId, CancellationToken cancellationToken = default)
    {
        var signatureRequest = await GetSignatureRequestAsync(requestId, cancellationToken);
        return signatureRequest?.Status ?? SignatureStatus.Draft;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SignatureRequest>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        try
        {
            // DocuSeal doesn't have native entity filtering, so we get all and filter
            // In production, consider maintaining a local mapping table
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/submissions?limit=100", cancellationToken);
            response.EnsureSuccessStatusCode();

            var submissions = await response.Content.ReadFromJsonAsync<DocuSealSubmissionList>(_jsonOptions, cancellationToken);

            if (submissions?.Data == null)
            {
                return Enumerable.Empty<SignatureRequest>();
            }

            // Filter by metadata
            return submissions.Data
                .Where(s => s.Metadata != null &&
                    s.Metadata.TryGetValue("crm_entity_type", out var type) && type?.ToString() == entityType &&
                    s.Metadata.TryGetValue("crm_entity_id", out var id) && id?.ToString() == entityId.ToString())
                .Select(s => MapToSignatureRequest(s, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submissions by entity {EntityType}:{EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CancelSignatureRequestAsync(string requestId, string? reason = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            // DocuSeal uses archive to cancel/void submissions
            var response = await _httpClient.DeleteAsync($"{_config.GetApiBaseUrl()}/submissions/{requestId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Cancelled/archived DocuSeal submission: {SubmissionId}. Reason: {Reason}",
                requestId, reason ?? "No reason provided");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel submission {SubmissionId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SendReminderAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            // Get submission to find pending submitters
            var submission = await GetSignatureRequestAsync(requestId, cancellationToken);
            if (submission == null)
            {
                throw new InvalidOperationException($"Submission {requestId} not found");
            }

            // Send reminder to each pending signer
            foreach (var signer in submission.Signers.Where(s => s.Status == "pending" || s.Status == "sent"))
            {
                var response = await _httpClient.PostAsync(
                    $"{_config.GetApiBaseUrl()}/submitters/{signer.Email}/remind",
                    null,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send reminder to {Email} for submission {SubmissionId}",
                        signer.Email, requestId);
                }
            }

            _logger.LogInformation("Sent reminders for DocuSeal submission: {SubmissionId}", requestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder for submission {SubmissionId}", requestId);
            throw;
        }
    }

    #endregion

    #region Signing Operations

    /// <inheritdoc />
    public async Task<SigningLink> GetSigningLinkAsync(string requestId, string signerEmail, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        try
        {
            // Get the submission to find the submitter's slug/token
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/submissions/{requestId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var submission = await response.Content.ReadFromJsonAsync<DocuSealSubmission>(_jsonOptions, cancellationToken);

            var submitter = submission?.Submitters?.FirstOrDefault(s =>
                s.Email?.Equals(signerEmail, StringComparison.OrdinalIgnoreCase) == true);

            if (submitter == null)
            {
                throw new InvalidOperationException($"Signer {signerEmail} not found in submission {requestId}");
            }

            var signingUrl = $"{_config.Url}/s/{submitter.Slug}";

            return new SigningLink
            {
                Url = signingUrl,
                ExpiresAt = submission?.ExpireAt ?? DateTime.UtcNow.AddDays(_config.DefaultExpirationDays),
                IsEmbedded = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signing link for {Email} in submission {SubmissionId}",
                signerEmail, requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SigningLink> GetEmbeddedSigningAsync(string requestId, string signerEmail, string returnUrl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);
        ArgumentException.ThrowIfNullOrWhiteSpace(signerEmail);

        // Get the basic signing link
        var signingLink = await GetSigningLinkAsync(requestId, signerEmail, cancellationToken);

        // DocuSeal supports embedded signing via the embed parameter
        signingLink.Url = $"{signingLink.Url}?embed=true&return_url={Uri.EscapeDataString(returnUrl)}";
        signingLink.IsEmbedded = true;

        return signingLink;
    }

    #endregion

    #region Document Operations

    /// <inheritdoc />
    public async Task<SignedDocument> GetSignedDocumentAsync(string requestId, string? documentId = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            // Get submission to check status and get document info
            var submissionResponse = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/submissions/{requestId}", cancellationToken);
            submissionResponse.EnsureSuccessStatusCode();

            var submission = await submissionResponse.Content.ReadFromJsonAsync<DocuSealSubmission>(_jsonOptions, cancellationToken);

            if (submission?.Status != "completed")
            {
                throw new InvalidOperationException($"Submission {requestId} is not completed. Status: {submission?.Status}");
            }

            // Get the combined document URL
            var documentUrl = submission.CombinedDocumentUrl;
            if (string.IsNullOrEmpty(documentUrl))
            {
                // Try individual documents
                var doc = submission.Documents?.FirstOrDefault(d => documentId == null || d.Id.ToString() == documentId);
                documentUrl = doc?.Url;
            }

            if (string.IsNullOrEmpty(documentUrl))
            {
                throw new InvalidOperationException($"No signed document available for submission {requestId}");
            }

            // Download the document
            var documentResponse = await _httpClient.GetAsync(documentUrl, cancellationToken);
            documentResponse.EnsureSuccessStatusCode();

            var content = await documentResponse.Content.ReadAsByteArrayAsync(cancellationToken);

            return new SignedDocument
            {
                RequestId = requestId,
                DocumentId = documentId ?? submission.Documents?.FirstOrDefault()?.Id.ToString(),
                FileName = $"signed-document-{requestId}.pdf",
                ContentType = "application/pdf",
                Content = content,
                SignedAt = submission.CompletedAt ?? DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get signed document for submission {SubmissionId}", requestId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<byte[]> GetAuditTrailAsync(string requestId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(requestId);

        try
        {
            // Get submission details for audit information
            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/submissions/{requestId}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var submission = await response.Content.ReadFromJsonAsync<DocuSealSubmission>(_jsonOptions, cancellationToken);

            // Generate audit trail from submission data
            var auditTrail = GenerateAuditTrail(submission);
            return Encoding.UTF8.GetBytes(auditTrail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit trail for submission {SubmissionId}", requestId);
            throw;
        }
    }

    #endregion

    #region Webhook Processing

    /// <inheritdoc />
    public Task<SignatureWebhookResult> ProcessWebhookAsync(string eventType, string payload, string? signature = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventType);
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        try
        {
            // Validate webhook signature if configured
            if (!string.IsNullOrEmpty(_config.WebhookSecret) && !string.IsNullOrEmpty(signature))
            {
                if (!ValidateWebhookSignature(payload, signature))
                {
                    _logger.LogWarning("Invalid webhook signature for event {EventType}", eventType);
                    return Task.FromResult(new SignatureWebhookResult
                    {
                        Success = false,
                        EventType = eventType,
                        Error = "Invalid webhook signature"
                    });
                }
            }

            var webhookData = JsonSerializer.Deserialize<DocuSealWebhookPayload>(payload, _jsonOptions);
            if (webhookData == null)
            {
                return Task.FromResult(new SignatureWebhookResult
                {
                    Success = false,
                    EventType = eventType,
                    Error = "Failed to parse webhook payload"
                });
            }

            var result = new SignatureWebhookResult
            {
                Success = true,
                EventType = eventType,
                RequestId = webhookData.Data?.SubmissionId?.ToString() ?? "",
                SignerEmail = webhookData.Data?.Email
            };

            // Map DocuSeal event to signature status
            result.NewStatus = eventType.ToLower() switch
            {
                "submission.created" => SignatureStatus.Sent,
                "submission.started" => SignatureStatus.InProgress,
                "submission.completed" => SignatureStatus.Completed,
                "submission.expired" => SignatureStatus.Expired,
                "submitter.completed" => null, // Individual signer completed
                "submitter.opened" => null, // Individual signer opened
                _ => null
            };

            // Extract CRM entity reference from metadata
            if (webhookData.Data?.Metadata != null)
            {
                if (webhookData.Data.Metadata.TryGetValue("crm_entity_type", out var entityType))
                {
                    result.EntityType = entityType?.ToString();
                }
                if (webhookData.Data.Metadata.TryGetValue("crm_entity_id", out var entityId))
                {
                    if (int.TryParse(entityId?.ToString(), out var id))
                    {
                        result.EntityId = id;
                    }
                }
            }

            // Create activity mapping for CRM timeline
            result.ActivityMapping = CreateActivityMapping(eventType, webhookData);

            _logger.LogInformation("Processed DocuSeal webhook: {EventType} for submission {SubmissionId}",
                eventType, result.RequestId);

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process webhook {EventType}", eventType);
            return Task.FromResult(new SignatureWebhookResult
            {
                Success = false,
                EventType = eventType,
                Error = ex.Message
            });
        }
    }

    #endregion

    /// <inheritdoc />
    public async Task<ProviderHealthResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var result = new ProviderHealthResult
        {
            ProviderName = ProviderName,
            CheckedAt = DateTime.UtcNow
        };

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var response = await _httpClient.GetAsync($"{_config.GetApiBaseUrl()}/templates?limit=1", cancellationToken);

            stopwatch.Stop();

            result.IsHealthy = response.IsSuccessStatusCode;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.Message = result.IsHealthy
                ? "DocuSeal is available"
                : $"DocuSeal returned status {response.StatusCode}";

            result.Details["url"] = _config.Url;
            result.Details["embedded_signing_enabled"] = _config.EnableEmbedSigning;

            // Get template count for stats
            if (response.IsSuccessStatusCode)
            {
                var templates = await GetTemplatesAsync(cancellationToken);
                result.Details["template_count"] = templates.Count();
            }
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"DocuSeal health check failed: {ex.Message}";
            _logger.LogWarning(ex, "DocuSeal health check failed");
        }

        return result;
    }

    #region Private Helpers

    private SignatureTemplate MapToSignatureTemplate(DocuSealTemplate docuSeal)
    {
        return new SignatureTemplate
        {
            Id = docuSeal.Id.ToString(),
            Name = docuSeal.Name ?? "Unnamed Template",
            Description = docuSeal.Description,
            SignerCount = docuSeal.Submitters?.Count ?? 1,
            SignerRoles = docuSeal.Submitters?.Select((s, i) => new PortSignerRole
            {
                RoleId = s.Name ?? $"signer_{i + 1}",
                RoleName = s.Name ?? $"Signer {i + 1}",
                Order = i + 1,
                IsRequired = true
            }).ToList(),
            CreatedAt = docuSeal.CreatedAt ?? DateTime.UtcNow,
            IsActive = !docuSeal.Archived
        };
    }

    private SignatureRequest MapToSignatureRequest(DocuSealSubmission submission, CreateSignatureRequest? originalRequest)
    {
        var request = new SignatureRequest
        {
            Id = submission.Id.ToString(),
            Subject = originalRequest?.Subject ?? submission.Metadata?.GetValueOrDefault("subject")?.ToString() ?? "Signature Request",
            Status = MapStatus(submission.Status),
            Signers = submission.Submitters?.Select(MapToSignerStatus).ToList() ?? new List<PortSignerStatus>(),
            Documents = submission.Documents?.Select(d => new SignatureDocumentInfo
            {
                Id = d.Id.ToString(),
                Name = d.Name ?? "Document",
                Order = 1,
                PageCount = d.PageCount ?? 1
            }).ToList(),
            CreatedAt = submission.CreatedAt ?? DateTime.UtcNow,
            SentAt = submission.CreatedAt,
            CompletedAt = submission.CompletedAt,
            ExpiresAt = submission.ExpireAt
        };

        // Extract entity reference from metadata
        if (submission.Metadata != null)
        {
            if (submission.Metadata.TryGetValue("crm_entity_type", out var entityType))
            {
                request.EntityType = entityType?.ToString();
            }
            if (submission.Metadata.TryGetValue("crm_entity_id", out var entityId))
            {
                if (int.TryParse(entityId?.ToString(), out var id))
                {
                    request.EntityId = id;
                }
            }
        }

        return request;
    }

    private PortSignerStatus MapToSignerStatus(DocuSealSubmitter submitter)
    {
        return new PortSignerStatus
        {
            Name = submitter.Name ?? "Unknown",
            Email = submitter.Email ?? "",
            Status = submitter.Status ?? "pending",
            SentAt = submitter.SentAt,
            ViewedAt = submitter.OpenedAt,
            SignedAt = submitter.CompletedAt,
            Order = submitter.Order ?? 1
        };
    }

    private SignatureStatus MapStatus(string? docuSealStatus)
    {
        return docuSealStatus?.ToLower() switch
        {
            "pending" => SignatureStatus.Sent,
            "started" => SignatureStatus.InProgress,
            "completed" => SignatureStatus.Completed,
            "expired" => SignatureStatus.Expired,
            "archived" => SignatureStatus.Voided,
            _ => SignatureStatus.Draft
        };
    }

    private bool ValidateWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_config.WebhookSecret))
        {
            return true; // No validation if secret not configured
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.WebhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToBase64String(hash);

        return signature.Equals(computedSignature, StringComparison.Ordinal);
    }

    private string GenerateAuditTrail(DocuSealSubmission? submission)
    {
        if (submission == null)
        {
            return "No submission data available";
        }

        var sb = new StringBuilder();
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine($"SIGNATURE AUDIT TRAIL");
        sb.AppendLine($"Submission ID: {submission.Id}");
        sb.AppendLine($"Provider: DocuSeal");
        sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine($"Status: {submission.Status}");
        sb.AppendLine($"Created: {submission.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");

        if (submission.CompletedAt.HasValue)
        {
            sb.AppendLine($"Completed: {submission.CompletedAt:yyyy-MM-dd HH:mm:ss} UTC");
        }

        sb.AppendLine();
        sb.AppendLine("SIGNERS:");
        sb.AppendLine("─────────────────────────────────────────────────────────────");

        if (submission.Submitters != null)
        {
            foreach (var submitter in submission.Submitters)
            {
                sb.AppendLine($"  Name: {submitter.Name}");
                sb.AppendLine($"  Email: {submitter.Email}");
                sb.AppendLine($"  Status: {submitter.Status}");

                if (submitter.SentAt.HasValue)
                    sb.AppendLine($"  Sent: {submitter.SentAt:yyyy-MM-dd HH:mm:ss} UTC");
                if (submitter.OpenedAt.HasValue)
                    sb.AppendLine($"  Opened: {submitter.OpenedAt:yyyy-MM-dd HH:mm:ss} UTC");
                if (submitter.CompletedAt.HasValue)
                    sb.AppendLine($"  Signed: {submitter.CompletedAt:yyyy-MM-dd HH:mm:ss} UTC");
                if (!string.IsNullOrEmpty(submitter.Ip))
                    sb.AppendLine($"  IP Address: {submitter.Ip}");
                if (!string.IsNullOrEmpty(submitter.UserAgent))
                    sb.AppendLine($"  User Agent: {submitter.UserAgent}");

                sb.AppendLine();
            }
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════");
        sb.AppendLine("END OF AUDIT TRAIL");

        return sb.ToString();
    }

    private SignatureActivityMapping? CreateActivityMapping(string eventType, DocuSealWebhookPayload webhookData)
    {
        var activityType = eventType.ToLower() switch
        {
            "submission.created" => "DocumentSent",
            "submission.completed" => "DocumentSigned",
            "submitter.completed" => "SignerCompleted",
            "submitter.opened" => "DocumentViewed",
            _ => null
        };

        if (activityType == null) return null;

        return new SignatureActivityMapping
        {
            ActivityType = activityType,
            Title = $"E-Signature: {activityType}",
            Description = $"DocuSeal {eventType} event for submission {webhookData.Data?.SubmissionId}",
            ExternalId = $"docuseal:{webhookData.Data?.SubmissionId}:{eventType}",
            ExternalSource = "DocuSeal",
            ActivityDate = DateTime.UtcNow
        };
    }

    #endregion

    #region DocuSeal API DTOs

    private class DocuSealTemplate
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Archived { get; set; }
        public List<DocuSealTemplateSubmitter>? Submitters { get; set; }
        public List<DocuSealField>? Fields { get; set; }
    }

    private class DocuSealTemplateSubmitter
    {
        public string? Name { get; set; }
        public string? Uuid { get; set; }
    }

    private class DocuSealField
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? SubmitterUuid { get; set; }
        public bool Required { get; set; }
    }

    private class DocuSealSubmissionRequest
    {
        [JsonPropertyName("template_id")]
        public int TemplateId { get; set; }

        [JsonPropertyName("send_email")]
        public bool SendEmail { get; set; }

        public string? Message { get; set; }

        public List<DocuSealSubmitter>? Submitters { get; set; }

        public Dictionary<string, string>? Values { get; set; }

        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("expire_at")]
        public string? ExpireAt { get; set; }
    }

    private class DocuSealSubmitter
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public string? Slug { get; set; }
        public int? Order { get; set; }

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("sent_at")]
        public DateTime? SentAt { get; set; }

        [JsonPropertyName("opened_at")]
        public DateTime? OpenedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }

        public string? Ip { get; set; }

        [JsonPropertyName("user_agent")]
        public string? UserAgent { get; set; }
    }

    private class DocuSealSubmission
    {
        public int Id { get; set; }
        public string? Status { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTime? CompletedAt { get; set; }

        [JsonPropertyName("expire_at")]
        public DateTime? ExpireAt { get; set; }

        public List<DocuSealSubmitter>? Submitters { get; set; }
        public List<DocuSealDocument>? Documents { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("combined_document_url")]
        public string? CombinedDocumentUrl { get; set; }
    }

    private class DocuSealDocument
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }

        [JsonPropertyName("page_count")]
        public int? PageCount { get; set; }
    }

    private class DocuSealSubmissionList
    {
        public List<DocuSealSubmission>? Data { get; set; }
        public DocuSealPagination? Pagination { get; set; }
    }

    private class DocuSealPagination
    {
        public int Count { get; set; }
        public int? Next { get; set; }
        public int? Prev { get; set; }
    }

    private class DocuSealWebhookPayload
    {
        [JsonPropertyName("event_type")]
        public string? EventType { get; set; }

        public DateTime? Timestamp { get; set; }
        public DocuSealWebhookData? Data { get; set; }
    }

    private class DocuSealWebhookData
    {
        [JsonPropertyName("submission_id")]
        public int? SubmissionId { get; set; }

        public string? Email { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    #endregion
}
