// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ─────────────────────────────────────────────────────────────────────────────
// MANDATORY pre-write verification performed:
//   Class:     BuiltInSignatureProvider
//   Namespace: CRM.Infrastructure.Providers.BuiltIn
//   File:      src/CRM.Infrastructure/Providers/BuiltIn/BuiltInSignatureProvider.cs
//   Constructor: (ILogger<BuiltInSignatureProvider> logger)
//     - logger ?? throw ArgumentNullException(nameof(logger))
//   Properties:
//     - ProviderName => "BuiltIn"
//   Storage: ConcurrentDictionary (pure in-memory — no DB dependency)
//   Key behaviours confirmed by reading source:
//     - IsAvailableAsync always returns true
//     - CreateTemplateAsync:
//         ArgumentNullException.ThrowIfNull(request)
//         ArgumentException.ThrowIfNullOrWhiteSpace(request.Name)
//         id = "builtin-template-{N}", template.IsActive = true
//     - GetTemplatesAsync: returns only templates where IsActive == true
//     - GetTemplateAsync: ArgumentException.ThrowIfNullOrWhiteSpace(templateId)
//     - CreateSignatureRequestAsync:
//         ArgumentNullException.ThrowIfNull(request)
//         ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject)
//         id = "builtin-sig-{N:D6}", initial status = SignatureStatus.Sent
//     - GetStatusAsync: returns SignatureStatus.Draft for unknown requestId
//     - GetByEntityAsync: ArgumentException.ThrowIfNullOrWhiteSpace(entityType)
//     - CancelSignatureRequestAsync: sets Status to SignatureStatus.Voided
//     - SendReminderAsync: no exception thrown, returns CompletedTask
//     - GetSigningLinkAsync: URL starts with "/signatures/manual-sign/{requestId}"
//     - GetEmbeddedSigningAsync: IsEmbedded = true
//     - RecordManualSignatureAsync: returns false for non-existent; when all signers signed -> Completed
//     - GetSignedDocumentAsync: always returns a SignedDocument (placeholder if not stored)
//     - GetAuditTrailAsync: empty byte[] for unknown request; UTF-8 text for known
//     - ProcessWebhookAsync: returns SignatureWebhookResult { Success = true }
//     - HealthCheckAsync: returns ProviderHealthResult { IsHealthy = true, ProviderName = "BuiltIn" }
//     - ClearAll(): public helper that resets all in-memory stores
// ─────────────────────────────────────────────────────────────────────────────

using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="BuiltInSignatureProvider"/>.
/// Covers constructor guards, template management, signature request lifecycle,
/// signing links, document retrieval, audit trail generation, and health.
/// All tests are pure in-memory (no DB or HTTP dependencies).
/// </summary>
public class BuiltInSignatureProviderTests
{
    // ── Factory ───────────────────────────────────────────────────────────────

    private static BuiltInSignatureProvider CreateProvider()
    {
        var logger = new Mock<ILogger<BuiltInSignatureProvider>>();
        return new BuiltInSignatureProvider(logger.Object);
    }

    private static CreateSignatureRequest BuildSignatureRequest(
        string subject = "Test Agreement",
        string? entityType = "Quote",
        int? entityId = 101,
        params Signer[] signers)
    {
        var effectiveSigners = signers.Length > 0
            ? signers.ToList()
            : new List<Signer>
            {
                new() { Name = "Alice Smith", Email = "alice@example.com", Order = 1 }
            };

        return new CreateSignatureRequest
        {
            Subject = subject,
            EntityType = entityType,
            EntityId = entityId,
            Signers = effectiveSigners
        };
    }

    // ── Constructor Guards ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new BuiltInSignatureProvider(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ── Provider Properties ───────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        var provider = CreateProvider();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    // ── IsAvailableAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_Always()
    {
        var provider = CreateProvider();

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    // ── Template Management ───────────────────────────────────────────────────

    [Fact]
    public async Task GetTemplatesAsync_ReturnsEmpty_WhenNoTemplatesCreated()
    {
        var provider = CreateProvider();

        var templates = (await provider.GetTemplatesAsync()).ToList();

        templates.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTemplateAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var provider = CreateProvider();

        var act = async () => await provider.CreateTemplateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateTemplateAsync_ThrowsArgumentException_WhenNameIsEmpty()
    {
        var provider = CreateProvider();
        var request = new CreateTemplateRequest { Name = "" };

        var act = async () => await provider.CreateTemplateAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateTemplateAsync_ReturnsTemplate_WithGeneratedId()
    {
        var provider = CreateProvider();
        var request = new CreateTemplateRequest
        {
            Name = "NDA Template",
            Description = "Standard NDA",
            DocumentName = "nda.pdf"
        };

        var template = await provider.CreateTemplateAsync(request);

        template.Should().NotBeNull();
        template.Id.Should().StartWith("builtin-template-");
        template.Name.Should().Be("NDA Template");
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsCreatedTemplates_OnlyActiveOnes()
    {
        var provider = CreateProvider();
        await provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Quote Template", DocumentName = "quote.pdf" });
        await provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Contract Template", DocumentName = "contract.pdf" });

        var templates = (await provider.GetTemplatesAsync()).ToList();

        templates.Should().HaveCount(2);
        templates.Should().AllSatisfy(t => t.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetTemplateAsync_ThrowsArgumentException_WhenTemplateIdIsWhitespace()
    {
        var provider = CreateProvider();

        var act = async () => await provider.GetTemplateAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetTemplateAsync_ReturnsNull_WhenTemplateDoesNotExist()
    {
        var provider = CreateProvider();

        var template = await provider.GetTemplateAsync("nonexistent-id");

        template.Should().BeNull();
    }

    [Fact]
    public async Task GetTemplateAsync_ReturnsTemplate_WhenTemplateExists()
    {
        var provider = CreateProvider();
        var created = await provider.CreateTemplateAsync(new CreateTemplateRequest
        {
            Name = "Master Services Agreement",
            DocumentName = "msa.pdf"
        });

        var fetched = await provider.GetTemplateAsync(created.Id);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Name.Should().Be("Master Services Agreement");
    }

    // ── Signature Request Lifecycle ───────────────────────────────────────────

    [Fact]
    public async Task CreateSignatureRequestAsync_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        var provider = CreateProvider();

        var act = async () => await provider.CreateSignatureRequestAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_ThrowsArgumentException_WhenSubjectIsEmpty()
    {
        var provider = CreateProvider();
        var request = BuildSignatureRequest(subject: "");

        var act = async () => await provider.CreateSignatureRequestAsync(request);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_ReturnsRequest_WithSentStatus()
    {
        var provider = CreateProvider();
        var request = BuildSignatureRequest("Quote Approval");

        var result = await provider.CreateSignatureRequestAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().StartWith("builtin-sig-");
        result.Subject.Should().Be("Quote Approval");
        result.Status.Should().Be(SignatureStatus.Sent);
        result.SentAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_SetsSignerStatusToPending()
    {
        var provider = CreateProvider();
        var request = BuildSignatureRequest("Contract",
            signers: new Signer { Name = "Bob Jones", Email = "bob@example.com", Order = 1 });

        var result = await provider.CreateSignatureRequestAsync(request);

        result.Signers.Should().HaveCount(1);
        result.Signers[0].Status.Should().Be("pending");
        result.Signers[0].Email.Should().Be("bob@example.com");
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ThrowsArgumentException_WhenRequestIdIsWhitespace()
    {
        var provider = CreateProvider();

        var act = async () => await provider.GetSignatureRequestAsync("  ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ReturnsNull_WhenRequestDoesNotExist()
    {
        var provider = CreateProvider();

        var result = await provider.GetSignatureRequestAsync("nonexistent-request-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ReturnsRequest_WhenRequestExists()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest("PO Approval"));

        var fetched = await provider.GetSignatureRequestAsync(created.Id);

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Subject.Should().Be("PO Approval");
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsDraft_WhenRequestDoesNotExist()
    {
        var provider = CreateProvider();

        var status = await provider.GetStatusAsync("nonexistent-id");

        status.Should().Be(SignatureStatus.Draft);
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsSent_AfterCreation()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var status = await provider.GetStatusAsync(created.Id);

        status.Should().Be(SignatureStatus.Sent);
    }

    [Fact]
    public async Task GetByEntityAsync_ThrowsArgumentException_WhenEntityTypeIsWhitespace()
    {
        var provider = CreateProvider();

        var act = async () => await provider.GetByEntityAsync(" ", 1);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetByEntityAsync_ReturnsRequestsForMatchingEntity()
    {
        var provider = CreateProvider();
        await provider.CreateSignatureRequestAsync(BuildSignatureRequest("NDA", "Quote", 5));
        await provider.CreateSignatureRequestAsync(BuildSignatureRequest("Other", "Contract", 9));

        var results = (await provider.GetByEntityAsync("Quote", 5)).ToList();

        results.Should().HaveCount(1);
        results[0].EntityType.Should().Be("Quote");
        results[0].EntityId.Should().Be(5);
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_SetsStatusToVoided()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        await provider.CancelSignatureRequestAsync(created.Id, "Superseded by new version");

        var status = await provider.GetStatusAsync(created.Id);
        status.Should().Be(SignatureStatus.Voided);
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_DoesNotThrow_WhenRequestIdDoesNotExist()
    {
        var provider = CreateProvider();

        var act = async () => await provider.CancelSignatureRequestAsync("nonexistent-id");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendReminderAsync_DoesNotThrow_ForExistingOrMissingRequest()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var act1 = async () => await provider.SendReminderAsync(created.Id);
        var act2 = async () => await provider.SendReminderAsync("nonexistent-id");

        await act1.Should().NotThrowAsync();
        await act2.Should().NotThrowAsync();
    }

    // ── Signing Operations ────────────────────────────────────────────────────

    [Fact]
    public async Task GetSigningLinkAsync_ReturnsLinkWithCorrectUrl()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var link = await provider.GetSigningLinkAsync(created.Id, "alice@example.com");

        link.Should().NotBeNull();
        link.Url.Should().StartWith($"/signatures/manual-sign/{created.Id}");
        link.IsEmbedded.Should().BeFalse();
        link.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task GetEmbeddedSigningAsync_ReturnsEmbeddedLink()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var link = await provider.GetEmbeddedSigningAsync(created.Id, "alice@example.com", "/done");

        link.Should().NotBeNull();
        link.IsEmbedded.Should().BeTrue();
        link.Url.Should().Contain(created.Id);
    }

    // ── RecordManualSignatureAsync ────────────────────────────────────────────

    [Fact]
    public async Task RecordManualSignatureAsync_ReturnsFalse_WhenRequestDoesNotExist()
    {
        var provider = CreateProvider();

        var result = await provider.RecordManualSignatureAsync(
            "nonexistent-id", "alice@example.com", "Alice Smith");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RecordManualSignatureAsync_ReturnsFalse_WhenSignerEmailNotFound()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var result = await provider.RecordManualSignatureAsync(
            created.Id, "unknown@example.com", "Unknown Person");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RecordManualSignatureAsync_UpdatesSignerStatusToSigned()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest(
            signers: new Signer { Name = "Alice Smith", Email = "alice@example.com", Order = 1 }));

        var result = await provider.RecordManualSignatureAsync(created.Id, "alice@example.com", "Alice Smith");

        result.Should().BeTrue();
        var refreshed = await provider.GetSignatureRequestAsync(created.Id);
        refreshed!.Signers[0].Status.Should().Be("signed");
    }

    [Fact]
    public async Task RecordManualSignatureAsync_SetsStatusToCompleted_WhenAllSignersSigned()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest(
            signers: new Signer { Name = "Alice", Email = "alice@example.com", Order = 1 }));

        await provider.RecordManualSignatureAsync(created.Id, "alice@example.com", "Alice");

        var status = await provider.GetStatusAsync(created.Id);
        status.Should().Be(SignatureStatus.Completed);
    }

    [Fact]
    public async Task RecordManualSignatureAsync_SetsStatusToInProgress_WhenNotAllSignersSigned()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest(
            signers: new Signer[]
            {
                new() { Name = "Alice", Email = "alice@example.com", Order = 1 },
                new() { Name = "Bob", Email = "bob@example.com", Order = 2 }
            }));

        // Only Alice signs
        await provider.RecordManualSignatureAsync(created.Id, "alice@example.com", "Alice");

        var status = await provider.GetStatusAsync(created.Id);
        status.Should().Be(SignatureStatus.InProgress);
    }

    // ── Document Operations ───────────────────────────────────────────────────

    [Fact]
    public async Task GetSignedDocumentAsync_ReturnsPlaceholder_WhenNotStored()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var doc = await provider.GetSignedDocumentAsync(created.Id);

        doc.Should().NotBeNull();
        doc.RequestId.Should().Be(created.Id);
        doc.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task GetAuditTrailAsync_ReturnsEmptyByteArray_WhenRequestDoesNotExist()
    {
        var provider = CreateProvider();

        var trail = await provider.GetAuditTrailAsync("nonexistent-id");

        trail.Should().NotBeNull();
        trail.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAuditTrailAsync_ReturnsNonEmptyBytes_WhenRequestExists()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest("Contract Review"));

        var trail = await provider.GetAuditTrailAsync(created.Id);

        trail.Should().NotBeEmpty();
        var text = System.Text.Encoding.UTF8.GetString(trail);
        text.Should().Contain(created.Id);
        text.Should().Contain("Contract Review");
        text.Should().Contain("AUDIT TRAIL");
    }

    // ── Webhook Processing ────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessWebhookAsync_ReturnsSuccess_ForAnyEventType()
    {
        var provider = CreateProvider();

        var result = await provider.ProcessWebhookAsync("document.signed", "{}");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_ParsesBuiltInEvent_AndReturnsRequestId()
    {
        var provider = CreateProvider();
        var created = await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        var result = await provider.ProcessWebhookAsync($"builtin:{created.Id}:completed", "{}");

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.RequestId.Should().Be(created.Id);
    }

    // ── Health Check ──────────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthyResult()
    {
        var provider = CreateProvider();

        var health = await provider.HealthCheckAsync();

        health.Should().NotBeNull();
        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
        health.Details.Should().ContainKey("templates_count");
        health.Details.Should().ContainKey("active_requests");
    }

    // ── ClearAll Helper ───────────────────────────────────────────────────────

    [Fact]
    public async Task ClearAll_RemovesAllTemplatesAndRequests()
    {
        var provider = CreateProvider();
        await provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Template A", DocumentName = "a.pdf" });
        await provider.CreateSignatureRequestAsync(BuildSignatureRequest());

        provider.ClearAll();

        var templates = (await provider.GetTemplatesAsync()).ToList();
        templates.Should().BeEmpty();
    }
}
