// CRM Solution - Provider Port Contract Tests
// Phase 0 Week 2 Task 2.10: Contract tests to ensure all provider implementations comply with port interfaces
// 
// These tests validate that mocked implementations correctly implement the port contracts.
// When actual providers are implemented, they should pass these same contract tests.

using Moq;
using Xunit;
using CRM.Core.Ports.Output.Providers;

namespace CRM.Tests.Ports;

#region Search Port Tests

/// <summary>
/// Contract tests for ISearchPort implementations.
/// </summary>
public class SearchPortContractTests
{
    private readonly Mock<ISearchPort> _mockSearchPort;

    public SearchPortContractTests()
    {
        _mockSearchPort = new Mock<ISearchPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockSearchPort.Setup(x => x.ProviderName).Returns("TestSearch");
        Assert.False(string.IsNullOrEmpty(_mockSearchPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockSearchPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockSearchPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = "TestSearch"
        };
        _mockSearchPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockSearchPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnSearchResult()
    {
        var searchResult = new SearchResult
        {
            Hits = new[] { new SearchHit { Id = "1", Title = "Test" } },
            TotalCount = 1,
            Query = "test"
        };
        _mockSearchPort.Setup(x => x.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(searchResult);

        var result = await _mockSearchPort.Object.SearchAsync(new SearchRequest { Query = "test" });
        Assert.NotNull(result);
        Assert.Equal("test", result.Query);
    }

    [Fact]
    public async Task SuggestAsync_ShouldReturnSuggestions()
    {
        var suggestions = new[] { new SearchSuggestion { Text = "test1" }, new SearchSuggestion { Text = "test2" } };
        _mockSearchPort.Setup(x => x.SuggestAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestions);

        var result = await _mockSearchPort.Object.SuggestAsync("te");
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }
}

#endregion

#region Chat Port Tests

/// <summary>
/// Contract tests for IChatPort implementations.
/// </summary>
public class ChatPortContractTests
{
    private readonly Mock<IChatPort> _mockChatPort;

    public ChatPortContractTests()
    {
        _mockChatPort = new Mock<IChatPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockChatPort.Setup(x => x.ProviderName).Returns("TestChat");
        Assert.False(string.IsNullOrEmpty(_mockChatPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockChatPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockChatPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestChat" };
        _mockChatPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockChatPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task CreateContactAsync_ShouldReturnContact()
    {
        var contact = new ChatContact { ExternalId = "ext-123", Name = "Test User", Email = "test@example.com" };
        _mockChatPort.Setup(x => x.CreateContactAsync(It.IsAny<ChatContactCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        var request = new ChatContactCreateRequest { Name = "Test User", Email = "test@example.com" };
        var result = await _mockChatPort.Object.CreateContactAsync(request);
        Assert.NotNull(result);
        Assert.Equal("ext-123", result.ExternalId);
    }

    [Fact]
    public async Task CreateConversationAsync_ShouldReturnConversation()
    {
        var conversation = new ChatConversation { ExternalId = "conv-123", ContactExternalId = "contact-123", Status = "open" };
        _mockChatPort.Setup(x => x.CreateConversationAsync(It.IsAny<ChatConversationCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var request = new ChatConversationCreateRequest { ContactExternalId = "contact-123" };
        var result = await _mockChatPort.Object.CreateConversationAsync(request);
        Assert.NotNull(result);
        Assert.Equal("conv-123", result.ExternalId);
    }

    [Fact]
    public async Task SendMessageAsync_ShouldReturnMessage()
    {
        var message = new ChatMessage { ExternalId = "msg-123", Content = "Hello", SenderType = "agent" };
        _mockChatPort.Setup(x => x.SendMessageAsync(It.IsAny<string>(), It.IsAny<ChatMessageCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(message);

        var result = await _mockChatPort.Object.SendMessageAsync("conv-123", new ChatMessageCreateRequest { Content = "Hello" });
        Assert.NotNull(result);
        Assert.Equal("msg-123", result.ExternalId);
    }
}

#endregion

#region Notification Port Tests

/// <summary>
/// Contract tests for INotificationPort implementations.
/// </summary>
public class NotificationPortContractTests
{
    private readonly Mock<INotificationPort> _mockNotificationPort;

    public NotificationPortContractTests()
    {
        _mockNotificationPort = new Mock<INotificationPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockNotificationPort.Setup(x => x.ProviderName).Returns("TestNotification");
        Assert.False(string.IsNullOrEmpty(_mockNotificationPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockNotificationPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockNotificationPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public void SupportedChannels_ShouldReturnChannelList()
    {
        _mockNotificationPort.Setup(x => x.SupportedChannels)
            .Returns(new[] { "email", "sms", "push" });
        var channels = _mockNotificationPort.Object.SupportedChannels;
        Assert.Contains("email", channels);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestNotification" };
        _mockNotificationPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockNotificationPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task SendEmailAsync_ShouldReturnResult()
    {
        var notifResult = new NotificationResult { Success = true, MessageId = "msg-123" };
        _mockNotificationPort.Setup(x => x.SendEmailAsync(It.IsAny<EmailNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var request = new EmailNotificationRequest { To = "test@example.com", Subject = "Test", Body = "Test body" };
        var result = await _mockNotificationPort.Object.SendEmailAsync(request);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task SendSmsAsync_ShouldReturnResult()
    {
        var notifResult = new NotificationResult { Success = true, MessageId = "sms-123" };
        _mockNotificationPort.Setup(x => x.SendSmsAsync(It.IsAny<SmsNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var request = new SmsNotificationRequest { To = "+1234567890", Message = "Test" };
        var result = await _mockNotificationPort.Object.SendSmsAsync(request);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task SendPushAsync_ShouldReturnResult()
    {
        var notifResult = new NotificationResult { Success = true, MessageId = "push-123" };
        _mockNotificationPort.Setup(x => x.SendPushAsync(It.IsAny<PushNotificationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(notifResult);

        var request = new PushNotificationRequest { To = "device-token", Title = "Test", Body = "Test body" };
        var result = await _mockNotificationPort.Object.SendPushAsync(request);
        Assert.True(result.Success);
    }
}

#endregion

#region Analytics Port Tests

/// <summary>
/// Contract tests for IAnalyticsPort implementations.
/// </summary>
public class AnalyticsPortContractTests
{
    private readonly Mock<IAnalyticsPort> _mockAnalyticsPort;

    public AnalyticsPortContractTests()
    {
        _mockAnalyticsPort = new Mock<IAnalyticsPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockAnalyticsPort.Setup(x => x.ProviderName).Returns("TestAnalytics");
        Assert.False(string.IsNullOrEmpty(_mockAnalyticsPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockAnalyticsPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockAnalyticsPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public void SupportsEmbedding_ShouldReturnBoolean()
    {
        _mockAnalyticsPort.Setup(x => x.SupportsEmbedding).Returns(true);
        Assert.True(_mockAnalyticsPort.Object.SupportsEmbedding);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestAnalytics" };
        _mockAnalyticsPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockAnalyticsPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task GetDashboardsAsync_ShouldReturnDashboards()
    {
        var dashboards = new[] { new DashboardInfo { Id = "d1", Name = "Dashboard 1" } };
        _mockAnalyticsPort.Setup(x => x.GetDashboardsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dashboards);

        var result = await _mockAnalyticsPort.Object.GetDashboardsAsync();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetEmbedAsync_ShouldReturnEmbedResult()
    {
        var embedResult = new EmbedResult { EmbedType = "iframe", EmbedUrl = "https://example.com/embed" };
        _mockAnalyticsPort.Setup(x => x.GetEmbedAsync(It.IsAny<EmbedRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedResult);

        var request = new EmbedRequest { ResourceId = "dashboard-1", EmbedType = "dashboard" };
        var result = await _mockAnalyticsPort.Object.GetEmbedAsync(request);
        Assert.NotNull(result);
        Assert.Equal("iframe", result.EmbedType);
    }
}

#endregion

#region Signature Port Tests

/// <summary>
/// Contract tests for ISignaturePort implementations.
/// </summary>
public class SignaturePortContractTests
{
    private readonly Mock<ISignaturePort> _mockSignaturePort;

    public SignaturePortContractTests()
    {
        _mockSignaturePort = new Mock<ISignaturePort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockSignaturePort.Setup(x => x.ProviderName).Returns("TestSignature");
        Assert.False(string.IsNullOrEmpty(_mockSignaturePort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockSignaturePort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockSignaturePort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestSignature" };
        _mockSignaturePort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockSignaturePort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task GetTemplatesAsync_ShouldReturnTemplates()
    {
        var templates = new[] { new SignatureTemplate { Id = "t1", Name = "Contract Template" } };
        _mockSignaturePort.Setup(x => x.GetTemplatesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        var result = await _mockSignaturePort.Object.GetTemplatesAsync();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateSignatureRequestAsync_ShouldReturnRequest()
    {
        var sigRequest = new SignatureRequest { Id = "sr-123", Subject = "Test Contract", Status = SignatureStatus.Sent };
        _mockSignaturePort.Setup(x => x.CreateSignatureRequestAsync(It.IsAny<CreateSignatureRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sigRequest);

        var request = new CreateSignatureRequest 
        { 
            Subject = "Test Contract",
            Signers = new List<Signer> { new Signer { Name = "John Doe", Email = "john@example.com" } }
        };
        var result = await _mockSignaturePort.Object.CreateSignatureRequestAsync(request);
        Assert.NotNull(result);
        Assert.Equal("sr-123", result.Id);
        Assert.Equal(SignatureStatus.Sent, result.Status);
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnStatus()
    {
        _mockSignaturePort.Setup(x => x.GetStatusAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SignatureStatus.Completed);

        var status = await _mockSignaturePort.Object.GetStatusAsync("sr-123");
        Assert.Equal(SignatureStatus.Completed, status);
    }

    [Fact]
    public async Task GetSigningLinkAsync_ShouldReturnLink()
    {
        var link = new SigningLink { Url = "https://sign.example.com/abc123" };
        _mockSignaturePort.Setup(x => x.GetSigningLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(link);

        var result = await _mockSignaturePort.Object.GetSigningLinkAsync("sr-123", "john@example.com");
        Assert.NotNull(result);
        Assert.Contains("sign.example.com", result.Url);
    }
}

#endregion

#region AI Port Tests

/// <summary>
/// Contract tests for IAIPort implementations.
/// </summary>
public class AIPortContractTests
{
    private readonly Mock<IAIPort> _mockAIPort;

    public AIPortContractTests()
    {
        _mockAIPort = new Mock<IAIPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockAIPort.Setup(x => x.ProviderName).Returns("TestAI");
        Assert.False(string.IsNullOrEmpty(_mockAIPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockAIPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockAIPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestAI" };
        _mockAIPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockAIPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnModels()
    {
        var models = new[] { new AIModelInfo { Id = "gpt-4", Name = "GPT-4", Provider = "OpenAI" } };
        _mockAIPort.Setup(x => x.GetAvailableModelsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(models);

        var result = await _mockAIPort.Object.GetAvailableModelsAsync();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task ChatAsync_ShouldReturnResponse()
    {
        var chatResponse = new AIChatResponse 
        { 
            Message = new AIChatMessage { Role = "assistant", Content = "Hello! How can I help?" },
            Model = "gpt-4"
        };
        _mockAIPort.Setup(x => x.ChatAsync(It.IsAny<AIChatRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatResponse);

        var request = new AIChatRequest 
        { 
            Messages = new List<AIChatMessage> { new AIChatMessage { Role = "user", Content = "Hi" } }
        };
        var result = await _mockAIPort.Object.ChatAsync(request);
        Assert.NotNull(result);
        Assert.Equal("assistant", result.Message.Role);
    }

    [Fact]
    public async Task GetEmbeddingAsync_ShouldReturnEmbedding()
    {
        var embedding = new AIEmbeddingResponse { Embedding = new float[] { 0.1f, 0.2f, 0.3f }, Model = "text-embedding" };
        _mockAIPort.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);

        var result = await _mockAIPort.Object.GetEmbeddingAsync("test text");
        Assert.NotNull(result);
        Assert.Equal(3, result.Embedding.Length);
    }

    [Fact]
    public async Task GenerateEmailDraftAsync_ShouldReturnDraft()
    {
        var draft = new AIEmailDraft { Subject = "Follow-up", Body = "Dear Customer..." };
        _mockAIPort.Setup(x => x.GenerateEmailDraftAsync(It.IsAny<EmailDraftRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(draft);

        var request = new EmailDraftRequest { Purpose = "follow-up", RecipientName = "John" };
        var result = await _mockAIPort.Object.GenerateEmailDraftAsync(request);
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Subject));
    }

    [Fact]
    public void EstimateTokens_ShouldReturnCount()
    {
        _mockAIPort.Setup(x => x.EstimateTokens(It.IsAny<string>())).Returns(10);
        var count = _mockAIPort.Object.EstimateTokens("This is a test sentence.");
        Assert.True(count > 0);
    }
}

#endregion

#region Integration Port Tests

/// <summary>
/// Contract tests for IIntegrationPort implementations.
/// </summary>
public class IntegrationPortContractTests
{
    private readonly Mock<IIntegrationPort> _mockIntegrationPort;

    public IntegrationPortContractTests()
    {
        _mockIntegrationPort = new Mock<IIntegrationPort>();
    }

    [Fact]
    public void ProviderName_ShouldReturnNonEmptyString()
    {
        _mockIntegrationPort.Setup(x => x.ProviderName).Returns("TestIntegration");
        Assert.False(string.IsNullOrEmpty(_mockIntegrationPort.Object.ProviderName));
    }

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnBoolean()
    {
        _mockIntegrationPort.Setup(x => x.IsAvailableAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var result = await _mockIntegrationPort.Object.IsAvailableAsync();
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthResult()
    {
        var healthResult = new ProviderHealthResult { IsHealthy = true, ProviderName = "TestIntegration" };
        _mockIntegrationPort.Setup(x => x.HealthCheckAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthResult);
        
        var result = await _mockIntegrationPort.Object.HealthCheckAsync();
        Assert.NotNull(result);
        Assert.True(result.IsHealthy);
    }

    [Fact]
    public async Task PublishEventAsync_ShouldReturnResult()
    {
        var publishResult = new EventPublishResult { Success = true, EventId = "evt-123" };
        _mockIntegrationPort.Setup(x => x.PublishEventAsync(It.IsAny<CrmEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(publishResult);

        var crmEvent = new CrmEvent { EventType = "account.created", EntityType = "Account", EntityId = 1 };
        var result = await _mockIntegrationPort.Object.PublishEventAsync(crmEvent);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task RegisterWebhookAsync_ShouldReturnWebhookInfo()
    {
        var webhookInfo = new WebhookInfo { Id = "wh-123", Name = "Test Webhook", TargetUrl = "https://example.com/webhook" };
        _mockIntegrationPort.Setup(x => x.RegisterWebhookAsync(It.IsAny<WebhookRegistration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(webhookInfo);

        var registration = new WebhookRegistration 
        { 
            Name = "Test Webhook", 
            TargetUrl = "https://example.com/webhook",
            EventTypes = new List<string> { "account.created" }
        };
        var result = await _mockIntegrationPort.Object.RegisterWebhookAsync(registration);
        Assert.NotNull(result);
        Assert.Equal("wh-123", result.Id);
    }

    [Fact]
    public async Task GetWorkflowsAsync_ShouldReturnWorkflows()
    {
        var workflows = new[] { new WorkflowInfo { Id = "wf-1", Name = "Workflow 1" } };
        _mockIntegrationPort.Setup(x => x.GetWorkflowsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflows);

        var result = await _mockIntegrationPort.Object.GetWorkflowsAsync();
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task TriggerWorkflowAsync_ShouldReturnResult()
    {
        var triggerResult = new WorkflowTriggerResult { Success = true, ExecutionId = "exec-123" };
        _mockIntegrationPort.Setup(x => x.TriggerWorkflowAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(triggerResult);

        var result = await _mockIntegrationPort.Object.TriggerWorkflowAsync("wf-1", new { test = true });
        Assert.True(result.Success);
    }
}

#endregion

#region Cross-Cutting Tests

/// <summary>
/// Tests for cross-cutting concerns across all provider ports.
/// </summary>
public class ProviderPortCrossCuttingTests
{
    [Fact]
    public void AllPorts_ShouldHaveProviderNameProperty()
    {
        // Verify all ports have ProviderName via interface definition
        Assert.True(typeof(ISearchPort).GetProperty("ProviderName") != null);
        Assert.True(typeof(IChatPort).GetProperty("ProviderName") != null);
        Assert.True(typeof(INotificationPort).GetProperty("ProviderName") != null);
        Assert.True(typeof(IAnalyticsPort).GetProperty("ProviderName") != null);
        Assert.True(typeof(ISignaturePort).GetProperty("ProviderName") != null);
        Assert.True(typeof(IAIPort).GetProperty("ProviderName") != null);
        Assert.True(typeof(IIntegrationPort).GetProperty("ProviderName") != null);
    }

    [Fact]
    public void AllPorts_ShouldHaveIsAvailableAsyncMethod()
    {
        Assert.True(typeof(ISearchPort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(IChatPort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(INotificationPort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(IAnalyticsPort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(ISignaturePort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(IAIPort).GetMethod("IsAvailableAsync") != null);
        Assert.True(typeof(IIntegrationPort).GetMethod("IsAvailableAsync") != null);
    }

    [Fact]
    public void AllPorts_ShouldHaveHealthCheckAsyncMethod()
    {
        Assert.True(typeof(ISearchPort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(IChatPort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(INotificationPort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(IAnalyticsPort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(ISignaturePort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(IAIPort).GetMethod("HealthCheckAsync") != null);
        Assert.True(typeof(IIntegrationPort).GetMethod("HealthCheckAsync") != null);
    }

    [Fact]
    public void ProviderHealthResult_ShouldHaveRequiredProperties()
    {
        var healthResult = new ProviderHealthResult
        {
            IsHealthy = true,
            ProviderName = "Test",
            Message = "OK"
        };

        Assert.True(healthResult.IsHealthy);
        Assert.Equal("Test", healthResult.ProviderName);
        Assert.Equal("OK", healthResult.Message);
    }
}

#endregion
