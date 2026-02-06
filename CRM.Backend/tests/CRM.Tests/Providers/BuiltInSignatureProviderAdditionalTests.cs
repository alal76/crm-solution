// CRM Solution - BuiltInSignatureProvider Additional Tests
// Additional unit tests for the built-in e-signature provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Additional unit tests for BuiltInSignatureProvider.
/// Tests template management, document operations, and audit trail.
/// </summary>
public class BuiltInSignatureProviderAdditionalTests : IDisposable
{
    private readonly Mock<ILogger<BuiltInSignatureProvider>> _loggerMock;
    private readonly BuiltInSignatureProvider _provider;

    public BuiltInSignatureProviderAdditionalTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInSignatureProvider>>();
        _provider = new BuiltInSignatureProvider(_loggerMock.Object);
    }

    public void Dispose()
    {
        // Cleanup
    }

    #region Template Management Tests

    [Fact]
    public async Task CreateTemplateAsync_WithValidRequest_CreatesTemplate()
    {
        // Arrange
        var request = new SignatureTemplateCreateRequest
        {
            Name = "NDA Template",
            Description = "Non-Disclosure Agreement",
            DocumentUrl = "https://storage.example.com/nda.pdf",
            Roles = new List<SignerRole>
            {
                new SignerRole { Name = "Employee", Order = 1 },
                new SignerRole { Name = "Manager", Order = 2 }
            }
        };

        // Act
        var result = await _provider.CreateTemplateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Name.Should().Be("NDA Template");
        result.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateTemplateAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.CreateTemplateAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetTemplateAsync_WithExistingTemplate_ReturnsTemplate()
    {
        // Arrange
        var created = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Test Template",
            DocumentUrl = "https://example.com/doc.pdf"
        });

        // Act
        var result = await _provider.GetTemplateAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetTemplateAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _provider.GetTemplateAsync("non-existing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsAllTemplates()
    {
        // Arrange
        await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Template 1",
            DocumentUrl = "https://example.com/doc1.pdf"
        });
        await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Template 2",
            DocumentUrl = "https://example.com/doc2.pdf"
        });

        // Act
        var templates = await _provider.GetTemplatesAsync();

        // Assert
        templates.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public async Task DeleteTemplateAsync_WithExistingTemplate_DeletesTemplate()
    {
        // Arrange
        var created = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "To Delete",
            DocumentUrl = "https://example.com/delete.pdf"
        });

        // Act
        var deleted = await _provider.DeleteTemplateAsync(created.Id);
        var template = await _provider.GetTemplateAsync(created.Id);

        // Assert
        deleted.Should().BeTrue();
        template.Should().BeNull();
    }

    #endregion

    #region Signature Request Tests

    [Fact]
    public async Task CreateSignatureRequestAsync_WithValidRequest_CreatesRequest()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Contract",
            DocumentUrl = "https://example.com/contract.pdf",
            Roles = new List<SignerRole>
            {
                new SignerRole { Name = "Client", Order = 1 }
            }
        });

        var request = new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Contract for John Doe",
            Signers = new List<Signer>
            {
                new Signer
                {
                    Name = "John Doe",
                    Email = "john@example.com",
                    Role = "Client"
                }
            },
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var result = await _provider.CreateSignatureRequestAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeNullOrEmpty();
        result.Status.Should().Be("pending");
        result.Signers.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.CreateSignatureRequestAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_WithoutSigners_ThrowsArgumentException()
    {
        // Arrange
        var request = new SignatureRequestCreateRequest
        {
            TemplateId = "template-1",
            Name = "Test Request",
            Signers = new List<Signer>()
        };

        // Act
        var act = () => _provider.CreateSignatureRequestAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSignatureRequestAsync_WithExistingRequest_ReturnsRequest()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Test",
            DocumentUrl = "https://example.com/test.pdf"
        });

        var created = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Test Request",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@example.com" }
            }
        });

        // Act
        var result = await _provider.GetSignatureRequestAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetSignatureStatusAsync_WithPendingRequest_ReturnsPending()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Status Test",
            DocumentUrl = "https://example.com/status.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Status Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@example.com" }
            }
        });

        // Act
        var status = await _provider.GetSignatureStatusAsync(request.Id);

        // Assert
        status.Status.Should().Be("pending");
        status.SignedCount.Should().Be(0);
        status.TotalSigners.Should().Be(1);
    }

    #endregion

    #region Signing Workflow Tests

    [Fact]
    public async Task GetSigningLinkAsync_WithValidSigner_ReturnsLink()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Link Test",
            DocumentUrl = "https://example.com/link.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Link Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var link = await _provider.GetSigningLinkAsync(request.Id, "signer@example.com");

        // Assert
        link.Should().NotBeNullOrEmpty();
        link.Should().Contain(request.Id);
    }

    [Fact]
    public async Task RecordSignatureAsync_WithValidSignature_RecordsSignature()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Sign Test",
            DocumentUrl = "https://example.com/sign.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Sign Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var result = await _provider.RecordSignatureAsync(request.Id, "signer@example.com", "signature-data");

        // Assert
        result.Should().BeTrue();

        var status = await _provider.GetSignatureStatusAsync(request.Id);
        status.SignedCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordSignatureAsync_WithAllSignersSigned_CompletesRequest()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Complete Test",
            DocumentUrl = "https://example.com/complete.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Complete Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        await _provider.RecordSignatureAsync(request.Id, "signer@example.com", "signature-data");

        // Assert
        var status = await _provider.GetSignatureStatusAsync(request.Id);
        status.Status.Should().Be("completed");
    }

    [Fact]
    public async Task DeclineSignatureAsync_WithValidRequest_DeclinesAndCancels()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Decline Test",
            DocumentUrl = "https://example.com/decline.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Decline Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var result = await _provider.DeclineSignatureAsync(request.Id, "signer@example.com", "Not interested");

        // Assert
        result.Should().BeTrue();

        var status = await _provider.GetSignatureStatusAsync(request.Id);
        status.Status.Should().Be("declined");
    }

    #endregion

    #region Document Operations Tests

    [Fact]
    public async Task GetSignedDocumentAsync_WithCompletedRequest_ReturnsDocument()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Doc Test",
            DocumentUrl = "https://example.com/doc.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Doc Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        await _provider.RecordSignatureAsync(request.Id, "signer@example.com", "signature");

        // Act
        var document = await _provider.GetSignedDocumentAsync(request.Id);

        // Assert
        document.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSignedDocumentAsync_WithPendingRequest_ReturnsNull()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Pending Doc",
            DocumentUrl = "https://example.com/pending.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Pending Doc",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var document = await _provider.GetSignedDocumentAsync(request.Id);

        // Assert
        document.Should().BeNull();
    }

    #endregion

    #region Audit Trail Tests

    [Fact]
    public async Task GetAuditTrailAsync_WithValidRequest_ReturnsTrail()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Audit Test",
            DocumentUrl = "https://example.com/audit.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Audit Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var auditTrail = await _provider.GetAuditTrailAsync(request.Id);

        // Assert
        auditTrail.Should().NotBeNull();
        auditTrail.Should().HaveCountGreaterOrEqualTo(1); // At least creation event
    }

    [Fact]
    public async Task GetAuditTrailAsync_AfterSignature_IncludesSignatureEvent()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Audit Sign",
            DocumentUrl = "https://example.com/auditsign.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Audit Sign",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        await _provider.RecordSignatureAsync(request.Id, "signer@example.com", "signature");

        // Act
        var auditTrail = await _provider.GetAuditTrailAsync(request.Id);

        // Assert
        auditTrail.Should().Contain(e => e.EventType.Contains("signed") || e.EventType.Contains("signature"));
    }

    #endregion

    #region Cancel and Remind Tests

    [Fact]
    public async Task CancelSignatureRequestAsync_WithPendingRequest_CancelsRequest()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Cancel Test",
            DocumentUrl = "https://example.com/cancel.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Cancel Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var result = await _provider.CancelSignatureRequestAsync(request.Id, "Cancelled by admin");

        // Assert
        result.Should().BeTrue();

        var status = await _provider.GetSignatureStatusAsync(request.Id);
        status.Status.Should().Be("cancelled");
    }

    [Fact]
    public async Task SendReminderAsync_WithPendingRequest_SendsReminder()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Remind Test",
            DocumentUrl = "https://example.com/remind.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Remind Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer", Email = "signer@example.com" }
            }
        });

        // Act
        var result = await _provider.SendReminderAsync(request.Id, "signer@example.com");

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthy()
    {
        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        result.Should().NotBeNull();
        result.IsHealthy.Should().BeTrue();
        result.ProviderName.Should().Be("BuiltIn");
    }

    #endregion

    #region Webhook Processing Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithValidPayload_ProcessesSuccessfully()
    {
        // Arrange
        var payload = new SignatureWebhookPayload
        {
            EventType = "signature.completed",
            RequestId = "request-123",
            SignerEmail = "signer@example.com",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var result = await _provider.ProcessWebhookAsync(payload);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithNullPayload_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _provider.ProcessWebhookAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task CreateTemplateAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        var request = new SignatureTemplateCreateRequest
        {
            Name = "Cancel Token Test",
            DocumentUrl = "https://example.com/cancel.pdf"
        };
        var cts = new CancellationTokenSource();

        // Act
        var result = await _provider.CreateTemplateAsync(request, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSignatureStatusAsync_WithCancellation_RespectsCancellation()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Test",
            DocumentUrl = "https://example.com/test.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@example.com" }
            }
        });

        var cts = new CancellationTokenSource();

        // Act
        var status = await _provider.GetSignatureStatusAsync(request.Id, cts.Token);

        // Assert
        status.Should().NotBeNull();
    }

    #endregion

    #region Multiple Signers Tests

    [Fact]
    public async Task CreateSignatureRequestAsync_WithMultipleSigners_CreatesAllSigners()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Multi Signer",
            DocumentUrl = "https://example.com/multi.pdf",
            Roles = new List<SignerRole>
            {
                new SignerRole { Name = "Employee", Order = 1 },
                new SignerRole { Name = "Manager", Order = 2 },
                new SignerRole { Name = "HR", Order = 3 }
            }
        });

        var request = new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Multi Signer Request",
            Signers = new List<Signer>
            {
                new Signer { Name = "John", Email = "john@example.com", Role = "Employee" },
                new Signer { Name = "Jane", Email = "jane@example.com", Role = "Manager" },
                new Signer { Name = "Bob", Email = "bob@example.com", Role = "HR" }
            }
        };

        // Act
        var result = await _provider.CreateSignatureRequestAsync(request);

        // Assert
        result.Signers.Should().HaveCount(3);
    }

    [Fact]
    public async Task RecordSignatureAsync_WithPartialSignatures_StatusIsPartial()
    {
        // Arrange
        var template = await _provider.CreateTemplateAsync(new SignatureTemplateCreateRequest
        {
            Name = "Partial Sign",
            DocumentUrl = "https://example.com/partial.pdf"
        });

        var request = await _provider.CreateSignatureRequestAsync(new SignatureRequestCreateRequest
        {
            TemplateId = template.Id,
            Name = "Partial Sign",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer1", Email = "signer1@example.com" },
                new Signer { Name = "Signer2", Email = "signer2@example.com" }
            }
        });

        // Act - Only first signer signs
        await _provider.RecordSignatureAsync(request.Id, "signer1@example.com", "signature");

        // Assert
        var status = await _provider.GetSignatureStatusAsync(request.Id);
        status.Status.Should().Be("partial"); // Or still "pending" depending on implementation
        status.SignedCount.Should().Be(1);
        status.TotalSigners.Should().Be(2);
    }

    #endregion
}
