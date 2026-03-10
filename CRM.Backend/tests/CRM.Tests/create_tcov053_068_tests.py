"""
Generate test files for TCOV-053 to TCOV-068.
Run from: /Users/alal/Code/Git CRM Solution/crm-solution/CRM.Backend/tests/CRM.Tests/
"""
import os

ROOT = os.path.dirname(os.path.abspath(__file__))

def write(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content)
    print(f"  Written: {os.path.relpath(path, ROOT)}")


# ─── TCOV-053: BuiltInAnalyticsProvider ────────────────────────────────────────
analytics = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Interfaces;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInAnalyticsProvider"/> (TCOV-053).</summary>
public class BuiltInAnalyticsProviderTests
{
    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<BuiltInAnalyticsProvider>> _loggerMock = new();

    private BuiltInAnalyticsProvider Create() =>
        new(_dbContextMock.Object, _loggerMock.Object);

    // ─── Constructor ────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullContext_ShouldThrow()
    {
        var act = () => new BuiltInAnalyticsProvider(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new BuiltInAnalyticsProvider(_dbContextMock.Object, null!);
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
    public void SupportsEmbedding_ShouldBeFalse()
    {
        Create().SupportsEmbedding.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrue()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeTrue();
    }

    // ─── Dashboard Operations ────────────────────────────────────────────────────
    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnPredefinedDashboards()
    {
        var dashboards = (await Create().GetDashboardsAsync()).ToList();
        dashboards.Should().NotBeEmpty();
        dashboards.All(d => !string.IsNullOrEmpty(d.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task GetDashboardAsync_ExistingId_ShouldReturnDashboard()
    {
        var all = (await Create().GetDashboardsAsync()).ToList();
        var first = all.First();
        var dashboard = await Create().GetDashboardAsync(first.Id);
        dashboard.Should().NotBeNull();
        dashboard!.Id.Should().Be(first.Id);
    }

    [Fact]
    public async Task GetDashboardAsync_UnknownId_ShouldReturnNull()
    {
        var result = await Create().GetDashboardAsync("does-not-exist");
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDashboardsForUserAsync_ShouldReturnAllDashboards()
    {
        var result = (await Create().GetDashboardsForUserAsync(42)).ToList();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetEmbedAsync_ShouldReturnUnsupportedResult()
    {
        var request = new CRM.Core.Ports.Output.Providers.EmbedRequest { DashboardId = "overview" };
        var result = await Create().GetEmbedAsync(request);
        result.Should().NotBeNull();
        result.EmbedType.Should().Be("unsupported");
    }
}
"""
write(os.path.join(ROOT, "Providers", "BuiltInAnalyticsProviderTests.cs"), analytics)


# ─── TCOV-054: BuiltInChatProvider ─────────────────────────────────────────────
chat = r"""// CRM Solution — CRM Test Suite
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
"""
write(os.path.join(ROOT, "Providers", "BuiltInChatProviderTests.cs"), chat)


# ─── TCOV-055: BuiltInNotificationProvider ─────────────────────────────────────
notification = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInNotificationProvider"/> (TCOV-055).</summary>
public class BuiltInNotificationProviderTests
{
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<BuiltInNotificationProvider>> _loggerMock = new();

    private BuiltInNotificationProvider Create()
    {
        // Bind an empty configuration section for Smtp
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.Key).Returns("Smtp");
        configSection.Setup(s => s.GetChildren()).Returns(Enumerable.Empty<IConfigurationSection>());
        _configMock.Setup(c => c.GetSection("Smtp")).Returns(configSection.Object);
        return new BuiltInNotificationProvider(_configMock.Object, _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullConfiguration_ShouldThrow()
    {
        var act = () => new BuiltInNotificationProvider(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configuration");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(s => s.GetChildren()).Returns(Enumerable.Empty<IConfigurationSection>());
        _configMock.Setup(c => c.GetSection("Smtp")).Returns(configSection.Object);
        var act = () => new BuiltInNotificationProvider(_configMock.Object, null!);
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
    public void SupportedChannels_ShouldContainEmail()
    {
        Create().SupportedChannels.Should().Contain("email");
    }

    // ─── Email ───────────────────────────────────────────────────────────────────
    [Fact]
    public async Task SendEmailAsync_NullRequest_ShouldThrow()
    {
        var act = async () => await Create().SendEmailAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendEmailAsync_EmptyTo_ShouldThrow()
    {
        var req = new EmailNotificationRequest { To = "", Subject = "Hi", Body = "Body" };
        var act = async () => await Create().SendEmailAsync(req);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_EmptySubject_ShouldThrow()
    {
        var req = new EmailNotificationRequest { To = "t@t.com", Subject = "", Body = "Body" };
        var act = async () => await Create().SendEmailAsync(req);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SendEmailAsync_NoSmtpConfigured_ShouldReturnDevSuccess()
    {
        // SMTP not configured => dev mode returns success
        var result = await Create().SendEmailAsync(new EmailNotificationRequest
        {
            To = "user@example.com",
            Subject = "Hello",
            Body = "Test body"
        });
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Provider.Should().Be("BuiltIn");
        result.Channel.Should().Be("email");
    }

    [Fact]
    public async Task SendTemplateEmailAsync_ShouldReturnUnsupported()
    {
        var result = await Create().SendTemplateEmailAsync("tpl-001", "user@example.com", new { });
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
    }
}
"""
write(os.path.join(ROOT, "Providers", "BuiltInNotificationProviderTests.cs"), notification)


# ─── TCOV-056: BuiltInSignatureProvider ────────────────────────────────────────
signature = r"""// CRM Solution — CRM Test Suite
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
        Signers = new List<SignerInfo>
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
        var bad = new CreateSignatureRequest { Subject = "", Signers = new List<SignerInfo>() };
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
"""
write(os.path.join(ROOT, "Providers", "BuiltInSignatureProviderTests.cs"), signature)


# ─── TCOV-057: BuiltInIntegrationProvider ──────────────────────────────────────
integration = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="BuiltInIntegrationProvider"/> (TCOV-057).</summary>
public class BuiltInIntegrationProviderTests
{
    private readonly Mock<ILogger<BuiltInIntegrationProvider>> _loggerMock = new();

    private BuiltInIntegrationProvider Create(HttpClient? httpClient = null)
    {
        var options = Options.Create(new BuiltInIntegrationConfiguration());
        return new BuiltInIntegrationProvider(
            httpClient ?? new HttpClient(),
            options,
            _loggerMock.Object);
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

    // ─── Event Publishing ─────────────────────────────────────────────────────────
    [Fact]
    public async Task PublishEventAsync_NoWebhooks_ShouldSucceedWithZeroDeliveries()
    {
        var crmEvent = new CrmEvent
        {
            EventType = "account.created",
            EntityType = "Account",
            EntityId = "1"
        };
        var result = await Create().PublishEventAsync(crmEvent);
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.WebhooksTriggered.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventsAsync_EmptyList_ShouldReturnEmptyBatch()
    {
        var result = await Create().PublishEventsAsync(Enumerable.Empty<CrmEvent>());
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishEventsAsync_SingleEvent_ShouldCountCorrectly()
    {
        var events = new[]
        {
            new CrmEvent { EventType = "contact.updated", EntityType = "Contact", EntityId = "2" }
        };
        var result = await Create().PublishEventsAsync(events);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetWebhooksAsync_NewInstance_ShouldReturnEmptyOrList()
    {
        var webhooks = (await Create().GetWebhooksAsync());
        webhooks.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterWebhookAsync_ValidRequest_ShouldReturnWebhook()
    {
        var req = new RegisterWebhookRequest
        {
            Name = "Test Webhook",
            Url = "https://webhook.example.com/events",
            EventTypes = new List<string> { "account.created" }
        };
        var webhook = await Create().RegisterWebhookAsync(req);
        webhook.Should().NotBeNull();
        webhook.Name.Should().Be("Test Webhook");
    }
}
"""
write(os.path.join(ROOT, "Providers", "BuiltInIntegrationProviderTests.cs"), integration)


# ─── TCOV-058: MeilisearchProvider ─────────────────────────────────────────────
meilisearch = r"""// CRM Solution — CRM Test Suite
using CRM.Infrastructure.Providers.Meilisearch;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="MeilisearchProvider"/> (TCOV-058).</summary>
public class MeilisearchProviderTests
{
    private readonly Mock<ILogger<MeilisearchProvider>> _loggerMock = new();

    private MeilisearchProvider Create(string url = "http://localhost:7700", string apiKey = "testKey")
    {
        var config = new MeilisearchConfiguration { Url = url, ApiKey = apiKey };
        return new MeilisearchProvider(Options.Create(config), _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => Create();
        act.Should().NotThrow();
    }

    // ─── Properties ─────────────────────────────────────────────────────────────
    [Fact]
    public void ProviderName_ShouldReturnMeilisearch()
    {
        Create().ProviderName.Should().Be("Meilisearch");
    }

    // ─── Availability ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IsAvailableAsync_Unreachable_ShouldReturnFalse()
    {
        // A non-existent host should cause the provider to return false, not throw
        var result = await Create("http://localhost:19999", "key").IsAvailableAsync();
        result.Should().BeFalse();
    }

    // ─── SearchAsync ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task SearchAsync_ShortQuery_ShouldReturnEmptyResult()
    {
        var request = new CRM.Core.Ports.Output.Providers.SearchRequest { Query = "x" };
        var result = await Create().SearchAsync(request);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ShouldReturnEmptyResult()
    {
        var request = new CRM.Core.Ports.Output.Providers.SearchRequest { Query = "" };
        var result = await Create().SearchAsync(request);
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }
}
"""
write(os.path.join(ROOT, "Providers", "MeilisearchProviderTests.cs"), meilisearch)


# ─── TCOV-059: OllamaProvider ──────────────────────────────────────────────────
# Fake HttpMessageHandler for testing
ollama = r"""// CRM Solution — CRM Test Suite
using System.Net;
using System.Net.Http.Json;
using System.Text;
using CRM.Infrastructure.Providers.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="OllamaProvider"/> (TCOV-059).</summary>
public class OllamaProviderTests
{
    private readonly Mock<ILogger<OllamaProvider>> _loggerMock = new();

    private static OllamaConfiguration DefaultConfig() => new()
    {
        BaseUrl = "http://localhost:11434",
        DefaultModel = "llama3",
        EmbeddingModel = "nomic-embed-text"
    };

    private OllamaProvider Create(HttpMessageHandler? handler = null)
    {
        var client = handler is null
            ? new HttpClient { BaseAddress = new Uri("http://localhost:11434") }
            : new HttpClient(handler) { BaseAddress = new Uri("http://localhost:11434") };
        return new OllamaProvider(client, Options.Create(DefaultConfig()), _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullHttpClient_ShouldThrow()
    {
        var act = () => new OllamaProvider(null!, Options.Create(DefaultConfig()), _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_NullConfig_ShouldThrow()
    {
        var act = () => new OllamaProvider(new HttpClient(), null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new OllamaProvider(new HttpClient(), Options.Create(DefaultConfig()), null!);
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
    public void ProviderName_ShouldReturnOllama()
    {
        Create().ProviderName.Should().Be("Ollama");
    }

    // ─── Availability ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IsAvailableAsync_WhenServerUnreachable_ShouldReturnFalse()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenServerResponds_ShouldReturnTrue()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var result = await Create(handler).IsAvailableAsync();
        result.Should().BeTrue();
    }

    // ─── GetAvailableModelsAsync ─────────────────────────────────────────────────
    [Fact]
    public async Task GetAvailableModelsAsync_WhenServerFails_ShouldReturnEmpty()
    {
        var result = await Create().GetAvailableModelsAsync();
        result.Should().BeEmpty();
    }
}

/// <summary>Minimal fake HttpMessageHandler for unit tests.</summary>
file sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content, Encoding.UTF8, "application/json")
        });
    }
}
"""
write(os.path.join(ROOT, "Providers", "OllamaProviderTests.cs"), ollama)


# ─── TCOV-060: AzureOpenAIProvider ─────────────────────────────────────────────
azure_openai = r"""// CRM Solution — CRM Test Suite
using System.Net;
using System.Text;
using CRM.Infrastructure.Providers.AI;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

#nullable enable

/// <summary>Tests for <see cref="AzureOpenAIProvider"/> (TCOV-060).</summary>
public class AzureOpenAIProviderTests
{
    private readonly Mock<ILogger<AzureOpenAIProvider>> _loggerMock = new();

    private static AzureOpenAIConfiguration DefaultConfig() => new()
    {
        Endpoint = "https://my-resource.openai.azure.com",
        ApiKey = "test-key",
        DeploymentName = "gpt-4o",
        ApiVersion = "2024-02-15-preview"
    };

    private AzureOpenAIProvider Create(HttpMessageHandler? handler = null)
    {
        var client = handler is null
            ? new HttpClient { BaseAddress = new Uri("https://my-resource.openai.azure.com") }
            : new HttpClient(handler) { BaseAddress = new Uri("https://my-resource.openai.azure.com") };
        return new AzureOpenAIProvider(client, Options.Create(DefaultConfig()), _loggerMock.Object);
    }

    // ─── Constructor ─────────────────────────────────────────────────────────────
    [Fact]
    public void Constructor_NullHttpClient_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(null!, Options.Create(DefaultConfig()), _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_NullConfig_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(new HttpClient(), null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new AzureOpenAIProvider(new HttpClient(), Options.Create(DefaultConfig()), null!);
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
    public void ProviderName_ShouldReturnAzureOpenAI()
    {
        Create().ProviderName.Should().Be("AzureOpenAI");
    }

    // ─── Availability ─────────────────────────────────────────────────────────────
    [Fact]
    public async Task IsAvailableAsync_WhenServerUnreachable_ShouldReturnFalse()
    {
        var result = await Create().IsAvailableAsync();
        result.Should().BeFalse();
    }

    // ─── Models ──────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetAvailableModelsAsync_ShouldIncludeDeploymentModel()
    {
        var models = (await Create().GetAvailableModelsAsync()).ToList();
        models.Should().NotBeEmpty();
        models.Should().Contain(m => m.Id == "gpt-4o");
    }

    // ─── Config Validation ────────────────────────────────────────────────────────
    [Fact]
    public void AzureOpenAIConfiguration_Validate_MissingEndpoint_ShouldReturnError()
    {
        var config = new AzureOpenAIConfiguration { Endpoint = "", DeploymentName = "gpt-4o", ApiKey = "key" };
        var (isValid, error) = config.Validate();
        isValid.Should().BeFalse();
        error.Should().Contain("Endpoint");
    }

    [Fact]
    public void AzureOpenAIConfiguration_Validate_ValidConfig_ShouldReturnValid()
    {
        var (isValid, error) = DefaultConfig().Validate();
        isValid.Should().BeTrue();
        error.Should().BeNull();
    }
}
"""
write(os.path.join(ROOT, "Providers", "AzureOpenAIProviderTests.cs"), azure_openai)


# ─── TCOV-061: MeetingIntelligenceAgent ────────────────────────────────────────
meeting = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="MeetingIntelligenceAgent"/> (TCOV-061).</summary>
public class MeetingIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<MeetingIntelligenceAgent>> _loggerMock = new();

    private MeetingIntelligenceAgent CreateAgent() =>
        new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new MeetingIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new MeetingIntelligenceAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeMeetingIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Meeting Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeMeetingIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.MeetingIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe04()
    {
        CreateAgent().Temperature.Should().Be(0.4);
    }

    [Fact]
    public void MaxTokens_ShouldBe4096()
    {
        CreateAgent().MaxTokens.Should().Be(4096);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainCalendarAndAccount()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Calendar");
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Contact");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrEmpty()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "MeetingIntelligenceAgentTests.cs"), meeting)


# ─── TCOV-062: SalesCoachAgent ─────────────────────────────────────────────────
sales_coach = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="SalesCoachAgent"/> (TCOV-062).</summary>
public class SalesCoachAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<SalesCoachAgent>> _loggerMock = new();

    private SalesCoachAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new SalesCoachAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new SalesCoachAgent(_kernel, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeSalesCoachAgent()
    {
        CreateAgent().AgentName.Should().Be("Sales Coach Agent");
    }

    [Fact]
    public void AgentType_ShouldBeSalesCoach()
    {
        CreateAgent().AgentType.Should().Be(AgentType.SalesCoach);
    }

    [Fact]
    public void Temperature_ShouldBe05()
    {
        CreateAgent().Temperature.Should().Be(0.5);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityAndAccount()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Contact");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionSales()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("sales", "coach", "deal", "SPIN", "MEDDIC");
    }

    [Fact]
    public void MaxTokens_ShouldBe4096()
    {
        CreateAgent().MaxTokens.Should().Be(4096);
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "SalesCoachAgentTests.cs"), sales_coach)


# ─── TCOV-063: SalesIntelligenceAgent ──────────────────────────────────────────
sales_intelligence = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="SalesIntelligenceAgent"/> (TCOV-063).</summary>
public class SalesIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<SalesIntelligenceAgent>> _loggerMock = new();

    private SalesIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new SalesIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeSalesIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Sales Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeSalesIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.SalesIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe03()
    {
        CreateAgent().Temperature.Should().Be(0.3);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityLeadAndSearch()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Lead");
        plugins.Should().Contain("Search");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrWhiteSpace()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "SalesIntelligenceAgentTests.cs"), sales_intelligence)


# ─── TCOV-064: NextBestActionAgent ─────────────────────────────────────────────
next_best = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="NextBestActionAgent"/> (TCOV-064).</summary>
public class NextBestActionAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<NextBestActionAgent>> _loggerMock = new();

    private NextBestActionAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new NextBestActionAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeNextBestActionAgent()
    {
        CreateAgent().AgentName.Should().Be("Next Best Action Agent");
    }

    [Fact]
    public void AgentType_ShouldBeNextBestAction()
    {
        CreateAgent().AgentType.Should().Be(AgentType.NextBestAction);
    }

    [Fact]
    public void Temperature_ShouldBe04()
    {
        CreateAgent().Temperature.Should().Be(0.4);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainAccountAndLead()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Account");
        plugins.Should().Contain("Lead");
    }

    [Fact]
    public void SystemPrompt_ShouldNotBeNullOrWhiteSpace()
    {
        CreateAgent().SystemPrompt.Should().NotBeNullOrWhiteSpace();
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "NextBestActionAgentTests.cs"), next_best)


# ─── TCOV-065: TicketResolutionAgent ───────────────────────────────────────────
ticket = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="TicketResolutionAgent"/> (TCOV-065).</summary>
public class TicketResolutionAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<TicketResolutionAgent>> _loggerMock = new();

    private TicketResolutionAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new TicketResolutionAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeTicketResolutionAgent()
    {
        CreateAgent().AgentName.Should().Be("Ticket Resolution Agent");
    }

    [Fact]
    public void AgentType_ShouldBeTicketResolution()
    {
        CreateAgent().AgentType.Should().Be(AgentType.TicketResolution);
    }

    [Fact]
    public void Temperature_ShouldBe03()
    {
        CreateAgent().Temperature.Should().Be(0.3);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainServiceRequestAndKnowledgeBase()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("ServiceRequest");
        plugins.Should().Contain("KnowledgeBase");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionTicket()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("ticket", "resolution", "knowledge");
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "TicketResolutionAgentTests.cs"), ticket)


# ─── TCOV-066: RevenueIntelligenceAgent ────────────────────────────────────────
revenue = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="RevenueIntelligenceAgent"/> (TCOV-066).</summary>
public class RevenueIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<RevenueIntelligenceAgent>> _loggerMock = new();

    private RevenueIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new RevenueIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeRevenueIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Revenue Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeRevenueIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.RevenueIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe02()
    {
        CreateAgent().Temperature.Should().Be(0.2);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainOpportunityAndContract()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Opportunity");
        plugins.Should().Contain("Contract");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionRevenue()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("revenue", "ARR", "MRR", "forecast");
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "RevenueIntelligenceAgentTests.cs"), revenue)


# ─── TCOV-067: DocumentIntelligenceAgent ───────────────────────────────────────
document = r"""// CRM Solution — CRM Test Suite
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Agents;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Agents;

#nullable enable

/// <summary>Tests for <see cref="DocumentIntelligenceAgent"/> (TCOV-067).</summary>
public class DocumentIntelligenceAgentTests
{
    private readonly Kernel _kernel = Kernel.CreateBuilder().Build();
    private readonly Mock<ILogger<DocumentIntelligenceAgent>> _loggerMock = new();

    private DocumentIntelligenceAgent CreateAgent() => new(_kernel, _loggerMock.Object);

    [Fact]
    public void Constructor_NullKernel_ShouldThrow()
    {
        var act = () => new DocumentIntelligenceAgent(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => CreateAgent();
        act.Should().NotThrow();
    }

    [Fact]
    public void AgentName_ShouldBeDocumentIntelligenceAgent()
    {
        CreateAgent().AgentName.Should().Be("Document Intelligence Agent");
    }

    [Fact]
    public void AgentType_ShouldBeDocumentIntelligence()
    {
        CreateAgent().AgentType.Should().Be(AgentType.DocumentIntelligence);
    }

    [Fact]
    public void Temperature_ShouldBe02()
    {
        CreateAgent().Temperature.Should().Be(0.2);
    }

    [Fact]
    public void AllowedPlugins_ShouldContainContractAndQuote()
    {
        var plugins = CreateAgent().AllowedPlugins;
        plugins.Should().Contain("Contract");
        plugins.Should().Contain("Quote");
    }

    [Fact]
    public void SystemPrompt_ShouldMentionDocument()
    {
        CreateAgent().SystemPrompt.Should().ContainAny("document", "contract", "clause");
    }
}
"""
write(os.path.join(ROOT, "AI", "SK", "Agents", "DocumentIntelligenceAgentTests.cs"), document)

print("\nAll files written successfully.")
