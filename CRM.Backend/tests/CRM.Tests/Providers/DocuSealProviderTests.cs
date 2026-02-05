// CRM Solution - DocuSeal Provider Unit Tests
// Phase 4 Week 17: Tests for DocuSeal e-signature integration
// Tests the DocuSeal provider implementing ISignaturePort

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using CRM.Infrastructure.Providers.DocuSeal;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Tests.Providers;

public class DocuSealProviderTests
{
    private readonly Mock<IOptions<DocuSealConfiguration>> _mockOptions;
    private readonly Mock<ILogger<DocuSealProvider>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;
    private readonly HttpClient _httpClient;
    private readonly DocuSealConfiguration _config;

    public DocuSealProviderTests()
    {
        _config = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com",
            ApiKey = "test-api-key",
            WebhookSecret = "test-webhook-secret",
            DefaultExpirationDays = 14,
            EnableEmbedSigning = true,
            TimeoutSeconds = 30
        };

        _mockOptions = new Mock<IOptions<DocuSealConfiguration>>();
        _mockOptions.Setup(x => x.Value).Returns(_config);

        _mockLogger = new Mock<ILogger<DocuSealProvider>>();

        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri(_config.Url + "/api/")
        };
    }

    private DocuSealProvider CreateProvider()
    {
        return new DocuSealProvider(_httpClient, _mockOptions.Object, _mockLogger.Object);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object responseContent)
    {
        var json = JsonSerializer.Serialize(responseContent);
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }

    #region Provider Properties

    [Fact]
    public void ProviderName_ShouldReturn_DocuSeal()
    {
        var provider = CreateProvider();
        Assert.Equal("DocuSeal", provider.ProviderName);
    }

    #endregion

    #region Configuration Validation

    [Fact]
    public void Configuration_Validate_WithValidConfig_ReturnsTrue()
    {
        var (isValid, error) = _config.Validate();

        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Configuration_Validate_WithMissingUrl_ReturnsFalse()
    {
        var config = new DocuSealConfiguration
        {
            Url = "",
            ApiKey = "test-key"
        };

        var (isValid, error) = config.Validate();

        Assert.False(isValid);
        Assert.Contains("URL", error);
    }

    [Fact]
    public void Configuration_Validate_WithMissingApiKey_ReturnsFalse()
    {
        var config = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com",
            ApiKey = ""
        };

        var (isValid, error) = config.Validate();

        Assert.False(isValid);
        Assert.Contains("API key is required", error);
    }

    [Fact]
    public void Configuration_GetApiBaseUrl_ReturnsCorrectUrl()
    {
        var config = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com/"
        };

        Assert.Equal("https://docuseal.example.com/api", config.GetApiBaseUrl());
    }

    [Fact]
    public void Configuration_GetApiBaseUrl_HandlesNoTrailingSlash()
    {
        var config = new DocuSealConfiguration
        {
            Url = "https://docuseal.example.com"
        };

        Assert.Equal("https://docuseal.example.com/api", config.GetApiBaseUrl());
    }

    #endregion

    #region Health Check

    [Fact]
    public async Task HealthCheckAsync_WhenApiReachable_ReturnsHealthy()
    {
        // Health check calls templates endpoint twice (once with limit=1, once for full list)
        // Each call needs its own response instance since content can only be read once
        // Use snake_case format to match DocuSeal API format
        var json = "[{\"id\":1,\"name\":\"Template 1\",\"created_at\":\"2024-01-01T00:00:00Z\"}]";
        
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            }));

        var provider = CreateProvider();
        var result = await provider.HealthCheckAsync();

        Assert.True(result.IsHealthy);
        Assert.Equal("DocuSeal", result.ProviderName);
    }

    [Fact]
    public async Task HealthCheckAsync_WhenApiUnreachable_ReturnsUnhealthy()
    {
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var provider = CreateProvider();
        var result = await provider.HealthCheckAsync();

        Assert.False(result.IsHealthy);
        Assert.Contains("Connection refused", result.Message);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthy_ReturnsTrue()
    {
        SetupHttpResponse(HttpStatusCode.OK, new[]
        {
            new { id = 1, name = "Template 1" }
        });

        var provider = CreateProvider();
        var result = await provider.IsAvailableAsync();

        Assert.True(result);
    }

    #endregion

    #region Template Operations

    [Fact]
    public async Task GetTemplatesAsync_ReturnsTemplates()
    {
        var templates = new[]
        {
            new { id = 1, name = "Contract Template", created_at = "2024-01-15T10:00:00Z", updated_at = "2024-01-15T10:00:00Z" },
            new { id = 2, name = "NDA Template", created_at = "2024-01-16T10:00:00Z", updated_at = "2024-01-16T10:00:00Z" }
        };
        SetupHttpResponse(HttpStatusCode.OK, templates);

        var provider = CreateProvider();
        var result = await provider.GetTemplatesAsync();

        Assert.NotNull(result);
        var templateList = result.ToList();
        Assert.Equal(2, templateList.Count);
        Assert.Equal("Contract Template", templateList[0].Name);
        Assert.Equal("NDA Template", templateList[1].Name);
    }

    [Fact]
    public async Task GetTemplateAsync_ReturnsTemplate()
    {
        var template = new
        {
            id = 1,
            name = "Contract Template",
            created_at = "2024-01-15T10:00:00Z",
            updated_at = "2024-01-15T10:00:00Z",
            fields = new[]
            {
                new { name = "signature", type = "signature", required = true }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, template);

        var provider = CreateProvider();
        var result = await provider.GetTemplateAsync("1");

        Assert.NotNull(result);
        Assert.Equal("Contract Template", result.Name);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task GetTemplateAsync_WhenNotFound_ReturnsNull()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, new { error = "Not found" });

        var provider = CreateProvider();
        var result = await provider.GetTemplateAsync("999");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateTemplateAsync_WithDocument_CreatesTemplate()
    {
        var createdTemplate = new
        {
            id = 5,
            name = "New Template",
            created_at = "2024-01-20T10:00:00Z",
            updated_at = "2024-01-20T10:00:00Z"
        };
        SetupHttpResponse(HttpStatusCode.OK, createdTemplate);

        var provider = CreateProvider();
        var request = new CreateTemplateRequest
        {
            Name = "New Template",
            DocumentContent = Encoding.UTF8.GetBytes("PDF content"),
            DocumentName = "contract.pdf"
        };

        var result = await provider.CreateTemplateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("5", result.Id);
        Assert.Equal("New Template", result.Name);
    }

    #endregion

    #region Signature Request Operations

    [Fact]
    public async Task CreateSignatureRequestAsync_CreatesSubmission()
    {
        var submission = new
        {
            id = 100,
            status = "pending",
            created_at = "2024-01-20T10:00:00Z",
            submitters = new[]
            {
                new { id = 200, email = "signer@example.com", status = "pending", embed_src = "https://docuseal.example.com/sign/abc123" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var request = new CreateSignatureRequest
        {
            TemplateId = "1",
            Subject = "Contract Signing",
            EntityType = "Quote",
            EntityId = 42,
            Signers = new List<Signer>
            {
                new Signer
                {
                    Email = "signer@example.com",
                    Name = "John Doe",
                    Order = 1,
                    RoleId = "signer_1"
                }
            }
        };

        var result = await provider.CreateSignatureRequestAsync(request);

        Assert.NotNull(result);
        Assert.Equal("100", result.Id);
        Assert.Equal(SignatureStatus.Sent, result.Status);
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ReturnsRequest()
    {
        var submission = new
        {
            id = 100,
            status = "pending",
            created_at = "2024-01-20T10:00:00Z",
            submitters = new[]
            {
                new { id = 200, email = "signer@example.com", status = "pending" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var result = await provider.GetSignatureRequestAsync("100");

        Assert.NotNull(result);
        Assert.Equal("100", result.Id);
        Assert.Single(result.Signers);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsSent_ForPending()
    {
        var submission = new
        {
            id = 100,
            status = "pending"
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var result = await provider.GetStatusAsync("100");

        Assert.Equal(SignatureStatus.Sent, result);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsCompleted()
    {
        var submission = new
        {
            id = 100,
            status = "completed"
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var result = await provider.GetStatusAsync("100");

        Assert.Equal(SignatureStatus.Completed, result);
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_CancelsRequest()
    {
        SetupHttpResponse(HttpStatusCode.OK, new { id = 100, status = "cancelled" });

        var provider = CreateProvider();

        // Should not throw - method returns void
        await provider.CancelSignatureRequestAsync("100", "Test cancellation");
    }

    [Fact]
    public async Task SendReminderAsync_SendsReminder()
    {
        SetupHttpResponse(HttpStatusCode.OK, new { success = true });

        var provider = CreateProvider();

        // Should not throw - only takes requestId
        await provider.SendReminderAsync("100");
    }

    #endregion

    #region Signing Link Operations

    [Fact]
    public async Task GetSigningLinkAsync_ReturnsSigningLink()
    {
        var submission = new
        {
            id = 100,
            submitters = new[]
            {
                new { id = 200, email = "signer@example.com", slug = "abc123" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var result = await provider.GetSigningLinkAsync("100", "signer@example.com");

        Assert.NotNull(result);
        Assert.NotNull(result.Url);
        Assert.False(result.IsEmbedded);
    }

    [Fact]
    public async Task GetEmbeddedSigningAsync_ReturnsEmbeddedLink()
    {
        var submission = new
        {
            id = 100,
            submitters = new[]
            {
                new { id = 200, email = "signer@example.com", embed_src = "https://docuseal.example.com/embed/abc123" }
            }
        };
        SetupHttpResponse(HttpStatusCode.OK, submission);

        var provider = CreateProvider();
        var result = await provider.GetEmbeddedSigningAsync("100", "signer@example.com", "https://crm.example.com/return");

        Assert.NotNull(result);
        Assert.True(result.IsEmbedded);
        Assert.Contains("embed", result.Url);
    }

    #endregion

    #region Document Operations

    [Fact]
    public async Task GetSignedDocumentAsync_ReturnsDocument()
    {
        // Provider first fetches submission details, then downloads the document
        var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var submissionJson = JsonSerializer.Serialize(new
        {
            id = 100,
            status = "completed",
            combined_document_url = "https://docuseal.example.com/documents/100.pdf",
            documents = new[] { new { id = 1, url = "https://docuseal.example.com/documents/100.pdf" } },
            completed_at = "2024-01-20T12:00:00Z"
        });

        var callCount = 0;
        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // First call: get submission details (JSON)
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(submissionJson, Encoding.UTF8, "application/json")
                    };
                }
                else
                {
                    // Second call: download PDF
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(pdfContent)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                    return response;
                }
            });

        var provider = CreateProvider();
        var result = await provider.GetSignedDocumentAsync("100");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
        Assert.Equal("application/pdf", result.ContentType);
    }

    [Fact]
    public async Task GetAuditTrailAsync_ReturnsAuditData()
    {
        // Provider fetches submission details and generates audit trail from it
        var submissionJson = JsonSerializer.Serialize(new
        {
            id = 100,
            status = "completed",
            created_at = "2024-01-20T10:00:00Z",
            completed_at = "2024-01-20T12:00:00Z",
            submitters = new[]
            {
                new
                {
                    id = 1,
                    email = "signer@example.com",
                    name = "Test Signer",
                    status = "completed",
                    completed_at = "2024-01-20T12:00:00Z"
                }
            }
        });

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(submissionJson, Encoding.UTF8, "application/json")
            });

        var provider = CreateProvider();
        var result = await provider.GetAuditTrailAsync("100");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Webhook Processing

    [Fact]
    public async Task ProcessWebhookAsync_ValidSignature_ProcessesEvent()
    {
        var provider = CreateProvider();

        var webhookPayload = new
        {
            event_type = "form.completed",
            timestamp = "2024-01-20T12:00:00Z",
            data = new
            {
                id = 100,
                status = "completed"
            }
        };

        var payloadJson = JsonSerializer.Serialize(webhookPayload);
        var signature = ComputeHmacSignature(payloadJson, _config.WebhookSecret);

        var result = await provider.ProcessWebhookAsync(
            "form.completed",
            payloadJson,
            signature);

        Assert.True(result.Success);
        Assert.Equal("form.completed", result.EventType);
    }

    [Fact]
    public async Task ProcessWebhookAsync_InvalidSignature_ReturnsFailure()
    {
        var provider = CreateProvider();

        var payloadJson = "{\"event_type\":\"form.completed\"}";
        var invalidSignature = "invalid-signature";

        var result = await provider.ProcessWebhookAsync(
            "form.completed",
            payloadJson,
            invalidSignature);

        Assert.False(result.Success);
        Assert.Contains("signature", result.Error?.ToLower() ?? "");
    }

    [Fact]
    public async Task ProcessWebhookAsync_FormStartedEvent_ReturnsCorrectEventType()
    {
        var provider = CreateProvider();

        var webhookPayload = new
        {
            event_type = "form.started",
            timestamp = "2024-01-20T10:00:00Z",
            data = new { id = 100 }
        };

        var payloadJson = JsonSerializer.Serialize(webhookPayload);
        var signature = ComputeHmacSignature(payloadJson, _config.WebhookSecret);

        var result = await provider.ProcessWebhookAsync(
            "form.started",
            payloadJson,
            signature);

        Assert.True(result.Success);
        Assert.Equal("form.started", result.EventType);
    }

    private string ComputeHmacSignature(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash);
    }

    #endregion

    #region Entity Queries

    [Fact]
    public async Task GetByEntityAsync_ReturnsRequests()
    {
        // DocuSealSubmissionList expects { data: [...], pagination: {...} }
        var submissionsResponse = new
        {
            data = new[]
            {
                new
                {
                    id = 100,
                    status = "completed",
                    created_at = "2024-01-20T10:00:00Z",
                    metadata = new Dictionary<string, object>
                    {
                        { "crm_entity_type", "Quote" },
                        { "crm_entity_id", "42" }
                    }
                }
            },
            pagination = new { count = 1 }
        };
        SetupHttpResponse(HttpStatusCode.OK, submissionsResponse);

        var provider = CreateProvider();
        var result = await provider.GetByEntityAsync("Quote", 42);

        Assert.NotNull(result);
    }

    #endregion
}
