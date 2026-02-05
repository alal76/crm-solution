// CRM Solution - BuiltIn Signature Provider Tests
// Phase 4 Week 16: Tests for manual signature workflow provider

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for BuiltInSignatureProvider.
/// Verifies manual signature workflow functionality.
/// </summary>
public class BuiltInSignatureProviderTests
{
    private readonly Mock<ILogger<BuiltInSignatureProvider>> _loggerMock;
    private readonly BuiltInSignatureProvider _provider;

    public BuiltInSignatureProviderTests()
    {
        _loggerMock = new Mock<ILogger<BuiltInSignatureProvider>>();
        _provider = new BuiltInSignatureProvider(_loggerMock.Object);
    }

    #region Provider Properties Tests

    [Fact]
    public void ProviderName_ShouldReturn_BuiltIn()
    {
        // Assert
        Assert.Equal("BuiltIn", _provider.ProviderName);
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldAlwaysReturn_True()
    {
        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Template Management Tests

    [Fact]
    public async Task CreateTemplateAsync_WithValidRequest_ShouldCreateTemplate()
    {
        // Arrange
        var request = new CreateTemplateRequest
        {
            Name = "Sales Contract Template",
            Description = "Standard sales contract for enterprise customers",
            SignerRoles = new List<SignerRole> 
            { 
                new SignerRole { RoleId = "customer", RoleName = "Customer", Order = 1 },
                new SignerRole { RoleId = "sales_manager", RoleName = "Sales Manager", Order = 2 }
            }
        };

        // Act
        var template = await _provider.CreateTemplateAsync(request);

        // Assert
        Assert.NotNull(template);
        Assert.NotEmpty(template.Id);
        Assert.Equal("Sales Contract Template", template.Name);
        Assert.Equal("Standard sales contract for enterprise customers", template.Description);
        Assert.Equal(2, template.SignerCount);
        Assert.True(template.IsActive);
        Assert.Contains(template.SignerRoles!, r => r.RoleName == "Customer");
        Assert.Contains(template.SignerRoles!, r => r.RoleName == "Sales Manager");
    }

    [Fact]
    public async Task GetTemplatesAsync_ShouldReturn_OnlyActiveTemplates()
    {
        // Arrange
        _provider.ClearAll();
        await _provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Template 1" });
        await _provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Template 2" });

        // Act
        var templates = await _provider.GetTemplatesAsync();

        // Assert
        Assert.Equal(2, templates.Count());
        Assert.All(templates, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetTemplateAsync_WithValidId_ShouldReturn_Template()
    {
        // Arrange
        var created = await _provider.CreateTemplateAsync(new CreateTemplateRequest 
        { 
            Name = "Test Template" 
        });

        // Act
        var retrieved = await _provider.GetTemplateAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved!.Id);
        Assert.Equal("Test Template", retrieved.Name);
    }

    [Fact]
    public async Task GetTemplateAsync_WithInvalidId_ShouldReturn_Null()
    {
        // Act
        var template = await _provider.GetTemplateAsync("nonexistent-template");

        // Assert
        Assert.Null(template);
    }

    [Fact]
    public async Task CreateTemplateAsync_WithNullRequest_ShouldThrow_ArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _provider.CreateTemplateAsync(null!));
    }

    [Fact]
    public async Task CreateTemplateAsync_WithEmptyName_ShouldThrow_ArgumentException()
    {
        // Arrange
        var request = new CreateTemplateRequest { Name = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _provider.CreateTemplateAsync(request));
    }

    #endregion

    #region Signature Request Tests

    [Fact]
    public async Task CreateSignatureRequestAsync_WithValidRequest_ShouldCreateRequest()
    {
        // Arrange
        var request = new CreateSignatureRequest
        {
            Subject = "Sales Contract for Acme Corp",
            Signers = new List<Signer>
            {
                new Signer { Name = "John Doe", Email = "john@acme.com", Order = 1 },
                new Signer { Name = "Jane Smith", Email = "jane@crm.com", Order = 2 }
            },
            Documents = new List<SignatureDocument>
            {
                new SignatureDocument { Name = "Contract.pdf", Order = 1 }
            },
            EntityType = "Quote",
            EntityId = 123
        };

        // Act
        var signatureRequest = await _provider.CreateSignatureRequestAsync(request);

        // Assert
        Assert.NotNull(signatureRequest);
        Assert.NotEmpty(signatureRequest.Id);
        Assert.Equal("Sales Contract for Acme Corp", signatureRequest.Subject);
        Assert.Equal(SignatureStatus.Sent, signatureRequest.Status);
        Assert.Equal(2, signatureRequest.Signers.Count);
        Assert.Single(signatureRequest.Documents!);
        Assert.Equal("Quote", signatureRequest.EntityType);
        Assert.Equal(123, signatureRequest.EntityId);
        Assert.NotNull(signatureRequest.SentAt);
        Assert.NotNull(signatureRequest.ExpiresAt);
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_ShouldSet_SignersPending()
    {
        // Arrange
        var request = new CreateSignatureRequest
        {
            Subject = "Test Contract",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer 1", Email = "signer1@test.com" },
                new Signer { Name = "Signer 2", Email = "signer2@test.com" }
            }
        };

        // Act
        var signatureRequest = await _provider.CreateSignatureRequestAsync(request);

        // Assert
        Assert.All(signatureRequest.Signers, s => Assert.Equal("pending", s.Status));
    }

    [Fact]
    public async Task GetSignatureRequestAsync_WithValidId_ShouldReturn_Request()
    {
        // Arrange
        var created = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test Request",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        var retrieved = await _provider.GetSignatureRequestAsync(created.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved!.Id);
    }

    [Fact]
    public async Task GetSignatureRequestAsync_WithInvalidId_ShouldReturn_Null()
    {
        // Act
        var request = await _provider.GetSignatureRequestAsync("nonexistent");

        // Assert
        Assert.Null(request);
    }

    [Fact]
    public async Task GetStatusAsync_WithValidId_ShouldReturn_Status()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        var status = await _provider.GetStatusAsync(request.Id);

        // Assert
        Assert.Equal(SignatureStatus.Sent, status);
    }

    [Fact]
    public async Task GetStatusAsync_WithInvalidId_ShouldReturn_Draft()
    {
        // Act
        var status = await _provider.GetStatusAsync("nonexistent");

        // Assert
        Assert.Equal(SignatureStatus.Draft, status);
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturn_MatchingRequests()
    {
        // Arrange
        _provider.ClearAll();
        await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Quote 1",
            Signers = new List<Signer> { new Signer { Name = "A", Email = "a@test.com" } },
            EntityType = "Quote",
            EntityId = 100
        });
        await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Quote 2",
            Signers = new List<Signer> { new Signer { Name = "B", Email = "b@test.com" } },
            EntityType = "Quote",
            EntityId = 100
        });
        await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Contract 1",
            Signers = new List<Signer> { new Signer { Name = "C", Email = "c@test.com" } },
            EntityType = "Contract",
            EntityId = 200
        });

        // Act
        var quoteRequests = await _provider.GetByEntityAsync("Quote", 100);

        // Assert
        Assert.Equal(2, quoteRequests.Count());
        Assert.All(quoteRequests, r => Assert.Equal("Quote", r.EntityType));
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_ShouldVoid_Request()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "To Cancel",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        await _provider.CancelSignatureRequestAsync(request.Id, "Customer changed mind");
        var status = await _provider.GetStatusAsync(request.Id);

        // Assert
        Assert.Equal(SignatureStatus.Voided, status);
    }

    #endregion

    #region Manual Signature Recording Tests

    [Fact]
    public async Task RecordManualSignatureAsync_WithValidSigner_ShouldRecord_Signature()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Manual Signing Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "John Doe", Email = "john@test.com" }
            }
        });

        // Act
        var result = await _provider.RecordManualSignatureAsync(
            request.Id, "john@test.com", "John Doe");

        // Assert
        Assert.True(result);
        
        var updatedRequest = await _provider.GetSignatureRequestAsync(request.Id);
        Assert.Equal(SignatureStatus.Completed, updatedRequest!.Status);
        Assert.Equal("signed", updatedRequest.Signers.First().Status);
        Assert.NotNull(updatedRequest.Signers.First().SignedAt);
        Assert.NotNull(updatedRequest.CompletedAt);
    }

    [Fact]
    public async Task RecordManualSignatureAsync_WithMultipleSigners_ShouldSet_InProgress()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Multi-Signer Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer 1", Email = "signer1@test.com", Order = 1 },
                new Signer { Name = "Signer 2", Email = "signer2@test.com", Order = 2 }
            }
        });

        // Act - First signer signs
        await _provider.RecordManualSignatureAsync(request.Id, "signer1@test.com", "Signer 1");

        // Assert - Status should be InProgress
        var updatedRequest = await _provider.GetSignatureRequestAsync(request.Id);
        Assert.Equal(SignatureStatus.InProgress, updatedRequest!.Status);
    }

    [Fact]
    public async Task RecordManualSignatureAsync_WhenAllSign_ShouldComplete()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Complete Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Signer 1", Email = "signer1@test.com", Order = 1 },
                new Signer { Name = "Signer 2", Email = "signer2@test.com", Order = 2 }
            }
        });

        // Act - Both signers sign
        await _provider.RecordManualSignatureAsync(request.Id, "signer1@test.com", "Signer 1");
        await _provider.RecordManualSignatureAsync(request.Id, "signer2@test.com", "Signer 2");

        // Assert
        var updatedRequest = await _provider.GetSignatureRequestAsync(request.Id);
        Assert.Equal(SignatureStatus.Completed, updatedRequest!.Status);
        Assert.NotNull(updatedRequest.CompletedAt);
    }

    [Fact]
    public async Task RecordManualSignatureAsync_WithInvalidRequest_ShouldReturn_False()
    {
        // Act
        var result = await _provider.RecordManualSignatureAsync(
            "nonexistent", "test@test.com", "Test");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RecordManualSignatureAsync_WithInvalidSigner_ShouldReturn_False()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Real Signer", Email = "real@test.com" }
            }
        });

        // Act
        var result = await _provider.RecordManualSignatureAsync(
            request.Id, "fake@test.com", "Fake Signer");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Signing Link Tests

    [Fact]
    public async Task GetSigningLinkAsync_ShouldReturn_ManualSigningUrl()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        var link = await _provider.GetSigningLinkAsync(request.Id, "test@test.com");

        // Assert
        Assert.NotNull(link);
        Assert.Contains("/signatures/manual-sign/", link.Url);
        Assert.Contains(request.Id, link.Url);
        Assert.Contains("test%40test.com", link.Url);
        Assert.False(link.IsEmbedded);
    }

    [Fact]
    public async Task GetEmbeddedSigningAsync_ShouldReturn_EmbeddedUrl()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        var link = await _provider.GetEmbeddedSigningAsync(
            request.Id, "test@test.com", "/quotes/123");

        // Assert
        Assert.NotNull(link);
        Assert.Contains("/signatures/manual-sign/", link.Url);
        Assert.Contains("return=", link.Url);
        Assert.True(link.IsEmbedded);
    }

    #endregion

    #region Document Operations Tests

    [Fact]
    public async Task GetSignedDocumentAsync_WithNoStoredDocument_ShouldReturn_Placeholder()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        var doc = await _provider.GetSignedDocumentAsync(request.Id);

        // Assert
        Assert.NotNull(doc);
        Assert.Equal(request.Id, doc.RequestId);
        Assert.Empty(doc.Content); // Placeholder is empty
    }

    [Fact]
    public async Task StoreSignedDocumentAsync_ShouldStore_AndRetrieve()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });
        var content = Encoding.UTF8.GetBytes("Mock PDF content");

        // Act
        await _provider.StoreSignedDocumentAsync(request.Id, null, content, "signed-contract.pdf");
        var doc = await _provider.GetSignedDocumentAsync(request.Id);

        // Assert
        Assert.Equal("signed-contract.pdf", doc.FileName);
        Assert.Equal(content.Length, doc.Content.Length);
    }

    [Fact]
    public async Task GetAuditTrailAsync_ShouldReturn_AuditContent()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Audit Trail Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "John Doe", Email = "john@test.com" }
            }
        });
        await _provider.RecordManualSignatureAsync(request.Id, "john@test.com", "John Doe");

        // Act
        var auditBytes = await _provider.GetAuditTrailAsync(request.Id);
        var auditText = Encoding.UTF8.GetString(auditBytes);

        // Assert
        Assert.NotEmpty(auditBytes);
        Assert.Contains("SIGNATURE REQUEST AUDIT TRAIL", auditText);
        Assert.Contains(request.Id, auditText);
        Assert.Contains("Audit Trail Test", auditText);
        Assert.Contains("John Doe", auditText);
        Assert.Contains("signed", auditText);
        Assert.Contains("BuiltIn (Manual Signature Workflow)", auditText);
    }

    [Fact]
    public async Task GetAuditTrailAsync_WithInvalidId_ShouldReturn_Empty()
    {
        // Act
        var audit = await _provider.GetAuditTrailAsync("nonexistent");

        // Assert
        Assert.Empty(audit);
    }

    #endregion

    #region Helper Method Tests

    [Fact]
    public async Task MarkAsViewedAsync_ShouldUpdate_SignerStatus()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "View Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        await _provider.MarkAsViewedAsync(request.Id, "test@test.com");

        // Assert
        var updated = await _provider.GetSignatureRequestAsync(request.Id);
        var signer = updated!.Signers.First();
        Assert.Equal("viewed", signer.Status);
        Assert.NotNull(signer.ViewedAt);
        Assert.Equal(SignatureStatus.Delivered, updated.Status);
    }

    [Fact]
    public async Task DeclineSignatureAsync_ShouldDecline_Request()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Decline Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });

        // Act
        await _provider.DeclineSignatureAsync(request.Id, "test@test.com", "Terms not acceptable");

        // Assert
        var updated = await _provider.GetSignatureRequestAsync(request.Id);
        Assert.Equal(SignatureStatus.Declined, updated!.Status);
        var signer = updated.Signers.First();
        Assert.Equal("declined", signer.Status);
        Assert.NotNull(signer.DeclinedAt);
        Assert.Equal("Terms not acceptable", signer.DeclineReason);
    }

    [Fact]
    public void ClearAll_ShouldReset_AllData()
    {
        // Arrange - Add some data
        _provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Test" }).Wait();
        _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        }).Wait();

        // Act
        _provider.ClearAll();

        // Assert
        Assert.Empty(_provider.GetTemplatesAsync().Result);
    }

    #endregion

    #region Webhook Processing Tests

    [Fact]
    public async Task ProcessWebhookAsync_WithInternalEvent_ShouldReturn_Success()
    {
        // Arrange
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Webhook Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            },
            EntityType = "Quote",
            EntityId = 123
        });

        // Act
        var result = await _provider.ProcessWebhookAsync(
            $"builtin:{request.Id}:signed",
            "{}");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(request.Id, result.RequestId);
        Assert.Equal("signed", result.EventType);
        Assert.Equal("Quote", result.EntityType);
        Assert.Equal(123, result.EntityId);
    }

    [Fact]
    public async Task ProcessWebhookAsync_WithUnknownEvent_ShouldReturn_Success()
    {
        // Act
        var result = await _provider.ProcessWebhookAsync("unknown:event", "{}");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("unknown:event", result.EventType);
    }

    #endregion

    #region Health Check Tests

    [Fact]
    public async Task HealthCheckAsync_ShouldReturn_HealthyResult()
    {
        // Arrange - Add some data
        _provider.ClearAll();
        await _provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "Test" });
        var request = await _provider.CreateSignatureRequestAsync(new CreateSignatureRequest
        {
            Subject = "Test",
            Signers = new List<Signer>
            {
                new Signer { Name = "Test", Email = "test@test.com" }
            }
        });
        await _provider.RecordManualSignatureAsync(request.Id, "test@test.com", "Test");

        // Act
        var health = await _provider.HealthCheckAsync();

        // Assert
        Assert.True(health.IsHealthy);
        Assert.Equal("BuiltIn", health.ProviderName);
        Assert.Equal(0, health.ResponseTimeMs);
        Assert.Contains("always available", health.Message);
        Assert.NotNull(health.Details);
        Assert.Equal(1, health.Details["templates_count"]);
        Assert.Equal(1, health.Details["completed_requests"]);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task CreateSignatureRequestAsync_ConcurrentCalls_ShouldGenerate_UniqueIds()
    {
        // Arrange
        _provider.ClearAll();
        var tasks = new List<Task<SignatureRequest>>();

        // Act - Create 10 requests concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(_provider.CreateSignatureRequestAsync(new CreateSignatureRequest
            {
                Subject = $"Concurrent Test {i}",
                Signers = new List<Signer>
                {
                    new Signer { Name = $"Signer {i}", Email = $"signer{i}@test.com" }
                }
            }));
        }
        var requests = await Task.WhenAll(tasks);

        // Assert - All IDs should be unique
        var ids = requests.Select(r => r.Id).ToList();
        Assert.Equal(10, ids.Distinct().Count());
    }

    #endregion
}
