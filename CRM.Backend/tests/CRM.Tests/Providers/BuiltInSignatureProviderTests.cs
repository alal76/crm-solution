// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInSignatureProvider"/> (TCOV-056).</summary>
public class BuiltInSignatureProviderTests
{
    private readonly Mock<ILogger<BuiltInSignatureProvider>> _loggerMock = new();

    private BuiltInSignatureProvider Create() => new(_loggerMock.Object);

    private static CreateSignatureRequest ValidSignatureRequest() => new()
    {
        Subject = "Contract Signing",
        Signers = new List<Signer>
        {
            new() { Name = "Alice Smith", Email = "alice@example.com" }
        }
    };

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new BuiltInSignatureProvider(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => Create();
        act.Should().NotThrow();
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnBuiltIn()
    {
        Create().ProviderName.Should().Be("BuiltIn");
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        (await Create().IsAvailableAsync()).Should().BeTrue();
    }

    // ─── Template Management ─────────────────────────────────────────────────────
    [Fact]
    public async Task CreateTemplateAsync_ValidRequest_ShouldReturnTemplate()
    {
        var template = await Create().CreateTemplateAsync(new CreateTemplateRequest { Name = "NDA Template" });
        template.Should().NotBeNull();
        template.Id.Should().StartWith("builtin-template-");
        template.Name.Should().Be("NDA Template");
        template.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetTemplatesAsync_AfterCreate_ShouldContainCreated()
    {
        var provider = Create();
        await provider.CreateTemplateAsync(new CreateTemplateRequest { Name = "SOW" });
        var templates = (await provider.GetTemplatesAsync()).ToList();
        templates.Should().ContainSingle(t => t.Name == "SOW");
    }

    // ─── Signature Requests ──────────────────────────────────────────────────────
    [Fact]
    public async Task CreateSignatureRequestAsync_ValidRequest_ShouldReturnRequest()
    {
        var req = await Create().CreateSignatureRequestAsync(ValidSignatureRequest());
        req.Should().NotBeNull();
        req.Id.Should().StartWith("builtin-sig-");
        req.Subject.Should().Be("Contract Signing");
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_EmptySubject_ShouldThrow()
    {
        var bad = new CreateSignatureRequest { Subject = "", Signers = new List<Signer>() };
        var act = async () => await Create().CreateSignatureRequestAsync(bad);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSignatureRequestAsync_ExistingId_ShouldReturnRequest()
    {
        var provider = Create();
        var created = await provider.CreateSignatureRequestAsync(ValidSignatureRequest());
        var fetched = await provider.GetSignatureRequestAsync(created.Id);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetStatusAsync_ExistingRequest_ShouldReturnSent()
    {
        var provider = Create();
        var created = await provider.CreateSignatureRequestAsync(ValidSignatureRequest());
        var status = await provider.GetStatusAsync(created.Id);
        status.Should().Be(SignatureStatus.Sent);
    }

    [Fact]
    public async Task CancelSignatureRequestAsync_ExistingRequest_ShouldVoid()
    {
        var provider = Create();
        var created = await provider.CreateSignatureRequestAsync(ValidSignatureRequest());
        await provider.CancelSignatureRequestAsync(created.Id, "Cancelled by test");
        var status = await provider.GetStatusAsync(created.Id);
        status.Should().Be(SignatureStatus.Voided);
    }
}
