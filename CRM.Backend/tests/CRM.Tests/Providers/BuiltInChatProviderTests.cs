// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInChatProvider"/> (TCOV-054).</summary>
public class BuiltInChatProviderTests
{
    private readonly Mock<ILogger<BuiltInChatProvider>> _loggerMock = new();

    private BuiltInChatProvider Create() => new(_loggerMock.Object);

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new BuiltInChatProvider(null!);
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

    // ─── Contact Management ──────────────────────────────────────────────────────
    [Fact]
    public async Task CreateContactAsync_ValidRequest_ShouldReturnContact()
    {
        var request = new ChatContactCreateRequest
        {
            Name = "Jane Doe",
            Email = "jane@example.com"
        };
        var contact = await Create().CreateContactAsync(request);
        contact.Should().NotBeNull();
        contact.Name.Should().Be("Jane Doe");
        contact.Email.Should().Be("jane@example.com");
        contact.ExternalId.Should().StartWith("builtin_contact_");
    }

    [Fact]
    public async Task CreateContactAsync_NullRequest_ShouldThrow()
    {
        var act = async () => await Create().CreateContactAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetContactAsync_ExistingContact_ShouldReturnContact()
    {
        var provider = Create();
        var created = await provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Test", Email = "t@t.com" });
        var fetched = await provider.GetContactAsync(created.ExternalId);
        fetched.Should().NotBeNull();
        fetched!.ExternalId.Should().Be(created.ExternalId);
    }

    [Fact]
    public async Task GetContactAsync_UnknownId_ShouldReturnNull()
    {
        var result = await Create().GetContactAsync("nonexistent_id");
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindContactByEmailAsync_ExistingEmail_ShouldReturnContact()
    {
        var provider = Create();
        await provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Alice", Email = "alice@crm.io" });
        var found = await provider.FindContactByEmailAsync("alice@crm.io");
        found.Should().NotBeNull();
        found!.Email.Should().Be("alice@crm.io");
    }

    // ─── Conversation Management ─────────────────────────────────────────────────
    [Fact]
    public async Task CreateConversationAsync_ValidRequest_ShouldReturnConversation()
    {
        var provider = Create();
        var contact = await provider.CreateContactAsync(new ChatContactCreateRequest { Name = "Bob" });
        var conv = await provider.CreateConversationAsync(new ChatConversationCreateRequest
        {
            ContactExternalId = contact.ExternalId,
            Subject = "Support request"
        });
        conv.Should().NotBeNull();
        conv.ExternalId.Should().StartWith("builtin_conv_");
        conv.Status.Should().Be("open");
    }

    [Fact]
    public async Task CreateConversationAsync_NullRequest_ShouldThrow()
    {
        var act = async () => await Create().CreateConversationAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
