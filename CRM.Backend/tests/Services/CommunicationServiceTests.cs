// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CommunicationService using InMemory database.
/// Tests channel CRUD, message operations, and conversation retrieval.
/// </summary>
public class CommunicationServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<CommunicationService>> _mockLogger;
    private readonly CommunicationService _service;

    public CommunicationServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"CommunicationServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _mockLogger = new Mock<ILogger<CommunicationService>>();
        _service = new CommunicationService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helpers

    private async Task<CommunicationChannel> SeedChannelAsync(
        string name = "Test Channel",
        ChannelType channelType = ChannelType.Email,
        ChannelStatus status = ChannelStatus.Configured,
        bool isDefault = false,
        bool isDeleted = false)
    {
        var channel = new CommunicationChannel
        {
            Name = name,
            ChannelType = channelType,
            Status = status,
            IsDefault = isDefault,
            IsDeleted = isDeleted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.CommunicationChannels.Add(channel);
        await _dbContext.SaveChangesAsync();
        return channel;
    }

    private async Task<CommunicationMessage> SeedMessageAsync(
        int channelId,
        string subject = "Test Message",
        MessageDirection direction = MessageDirection.Outbound,
        MessageStatus status = MessageStatus.Sent,
        int? accountId = null,
        int? contactId = null,
        string? conversationId = null)
    {
        var message = new CommunicationMessage
        {
            ChannelId = channelId,
            Subject = subject,
            Body = $"Body of {subject}",
            Direction = direction,
            Status = status,
            AccountId = accountId,
            ContactId = contactId,
            ConversationId = conversationId,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.CommunicationMessages.Add(message);
        await _dbContext.SaveChangesAsync();
        return message;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        var act = () => new CommunicationService(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new CommunicationService(_dbContext, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ShouldAcceptNullNotificationPort()
    {
        var service = new CommunicationService(_dbContext, _mockLogger.Object, null);
        service.Should().NotBeNull();
    }

    #endregion

    #region GetChannelsAsync Tests

    [Fact]
    public async Task GetChannelsAsync_ShouldReturnAllNonDeletedChannels()
    {
        await SeedChannelAsync("Email Channel", ChannelType.Email);
        await SeedChannelAsync("SMS Channel", ChannelType.SMS);
        await SeedChannelAsync("Deleted Channel", ChannelType.Email, isDeleted: true);

        var result = await _service.GetChannelsAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetChannelsAsync_ShouldReturnEmpty_WhenNoChannels()
    {
        var result = await _service.GetChannelsAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetChannelsAsync_ShouldMapToChannelInfo()
    {
        await SeedChannelAsync("Email Channel", ChannelType.Email, ChannelStatus.Connected, isDefault: true);

        var result = await _service.GetChannelsAsync();
        var channel = result.First();

        channel.Name.Should().Be("Email Channel");
        channel.IsDefault.Should().BeTrue();
    }

    #endregion

    #region GetChannelByIdAsync Tests

    [Fact]
    public async Task GetChannelByIdAsync_ShouldReturnChannel_WhenExists()
    {
        var seeded = await SeedChannelAsync("Find Me");
        var result = await _service.GetChannelByIdAsync(seeded.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetChannelByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetChannelByIdAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChannelByIdAsync_ShouldReturnNull_WhenDeleted()
    {
        var seeded = await SeedChannelAsync("Deleted", isDeleted: true);
        var result = await _service.GetChannelByIdAsync(seeded.Id);
        result.Should().BeNull();
    }

    #endregion

    #region CreateChannelAsync Tests

    [Fact]
    public async Task CreateChannelAsync_ShouldCreateChannel()
    {
        var request = new CommunicationChannelCreateRequest
        {
            Name = "New Email",
            ChannelType = "Email",
            IsDefault = false
        };

        var result = await _service.CreateChannelAsync(request);

        result.Should().NotBeNull();
        result.Name.Should().Be("New Email");
    }

    [Fact]
    public async Task CreateChannelAsync_ShouldPersistInDatabase()
    {
        var request = new CommunicationChannelCreateRequest
        {
            Name = "Persist Channel",
            ChannelType = "SMS"
        };

        var result = await _service.CreateChannelAsync(request);

        var dbChannel = await _dbContext.CommunicationChannels
            .FirstOrDefaultAsync(c => c.Name == "Persist Channel");
        dbChannel.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateChannelAsync_ShouldSetIsDefault_WhenSpecified()
    {
        var request = new CommunicationChannelCreateRequest
        {
            Name = "Default Channel",
            ChannelType = "Email",
            IsDefault = true
        };

        var result = await _service.CreateChannelAsync(request);
        result.IsDefault.Should().BeTrue();
    }

    #endregion

    #region DeleteChannelAsync Tests

    [Fact]
    public async Task DeleteChannelAsync_ShouldReturnTrue_WhenExists()
    {
        var seeded = await SeedChannelAsync("Delete Me");
        var result = await _service.DeleteChannelAsync(seeded.Id);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteChannelAsync_ShouldSoftDelete()
    {
        var seeded = await SeedChannelAsync("Soft Delete");
        await _service.DeleteChannelAsync(seeded.Id);

        var deleted = await _dbContext.CommunicationChannels.FindAsync(seeded.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteChannelAsync_ShouldReturnFalse_WhenNotFound()
    {
        var result = await _service.DeleteChannelAsync(999);
        result.Should().BeFalse();
    }

    #endregion

    #region GetMessagesAsync Tests

    [Fact]
    public async Task GetMessagesAsync_ShouldReturnAllNonDeletedMessages()
    {
        var channel = await SeedChannelAsync("Email");
        await SeedMessageAsync(channel.Id, "Msg 1");
        await SeedMessageAsync(channel.Id, "Msg 2");

        var result = await _service.GetMessagesAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMessagesAsync_ShouldFilterByAccountId()
    {
        var channel = await SeedChannelAsync("Email");
        await SeedMessageAsync(channel.Id, "Acct 1", accountId: 1);
        await SeedMessageAsync(channel.Id, "Acct 2", accountId: 2);

        var result = await _service.GetMessagesAsync(accountId: 1);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMessagesAsync_ShouldFilterByChannelId()
    {
        var ch1 = await SeedChannelAsync("Email");
        var ch2 = await SeedChannelAsync("SMS", ChannelType.SMS);
        await SeedMessageAsync(ch1.Id, "Email Msg");
        await SeedMessageAsync(ch2.Id, "SMS Msg");

        var result = await _service.GetMessagesAsync(channelId: ch1.Id);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetMessagesAsync_ShouldFilterByDirection()
    {
        var channel = await SeedChannelAsync("Email");
        await SeedMessageAsync(channel.Id, "Outbound", direction: MessageDirection.Outbound);
        await SeedMessageAsync(channel.Id, "Inbound", direction: MessageDirection.Inbound);

        var result = await _service.GetMessagesAsync(direction: MessageDirection.Outbound);

        result.Should().HaveCount(1);
    }

    #endregion

    #region GetMessageByIdAsync Tests

    [Fact]
    public async Task GetMessageByIdAsync_ShouldReturnMessage_WhenExists()
    {
        var channel = await SeedChannelAsync("Email");
        var msg = await SeedMessageAsync(channel.Id, "Find Me");

        var result = await _service.GetMessageByIdAsync(msg.Id);

        result.Should().NotBeNull();
        result!.Subject.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetMessageByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetMessageByIdAsync(999);
        result.Should().BeNull();
    }

    #endregion

    #region GetConversationAsync Tests

    [Fact]
    public async Task GetConversationAsync_ShouldReturnMessagesForAccount()
    {
        var channel = await SeedChannelAsync("Email");
        await SeedMessageAsync(channel.Id, "Msg 1", accountId: 1, conversationId: "conv-1");
        await SeedMessageAsync(channel.Id, "Msg 2", accountId: 1, conversationId: "conv-1");
        await SeedMessageAsync(channel.Id, "Other Account", accountId: 2);

        var result = await _service.GetConversationAsync(1);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetConversationAsync_ShouldFilterByContactId()
    {
        var channel = await SeedChannelAsync("Email");
        await SeedMessageAsync(channel.Id, "Contact 1", accountId: 1, contactId: 10);
        await SeedMessageAsync(channel.Id, "Contact 2", accountId: 1, contactId: 20);

        var result = await _service.GetConversationAsync(1, contactId: 10);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetConversationAsync_ShouldReturnEmpty_WhenNoMessages()
    {
        var result = await _service.GetConversationAsync(999);
        result.Should().BeEmpty();
    }

    #endregion

    #region SendMessageAsync Tests

    [Fact]
    public async Task SendMessageAsync_ShouldCreateMessage()
    {
        var channel = await SeedChannelAsync("Email", ChannelType.Email);
        var request = new SendMessageRequest
        {
            ChannelId = channel.Id,
            AccountId = 1,
            Subject = "Hello",
            Body = "Hello World",
            ToEmail = "test@example.com"
        };

        var result = await _service.SendMessageAsync(request);

        result.Should().NotBeNull();
        result.Subject.Should().Be("Hello");
    }

    [Fact]
    public async Task SendMessageAsync_ShouldPersistInDatabase()
    {
        var channel = await SeedChannelAsync("Email", ChannelType.Email);
        var request = new SendMessageRequest
        {
            ChannelId = channel.Id,
            Subject = "Persist Send",
            Body = "Body"
        };

        await _service.SendMessageAsync(request);

        var dbMsg = await _dbContext.CommunicationMessages
            .FirstOrDefaultAsync(m => m.Subject == "Persist Send");
        dbMsg.Should().NotBeNull();
    }

    #endregion

    #region TestChannelAsync Tests

    [Fact]
    public async Task TestChannelAsync_ShouldReturnFalse_WhenChannelNotFound()
    {
        var result = await _service.TestChannelAsync(999);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestChannelAsync_ShouldReturnFalse_WhenNoRealConnectionConfigured()
    {
        var channel = await SeedChannelAsync("Test Email", ChannelType.Email);
        // Without real SMTP/API config, test will fail gracefully and return false
        var result = await _service.TestChannelAsync(channel.Id);
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateChannelAsync Tests

    [Fact]
    public async Task UpdateChannelAsync_ShouldReturnUpdatedChannel_WhenExists()
    {
        var seeded = await SeedChannelAsync("Original");
        var update = new CommunicationChannelCreateRequest
        {
            Name = "Updated",
            ChannelType = "Email"
        };

        var result = await _service.UpdateChannelAsync(seeded.Id, update);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateChannelAsync_ShouldReturnNull_WhenNotFound()
    {
        var update = new CommunicationChannelCreateRequest { Name = "Update", ChannelType = "Email" };
        var result = await _service.UpdateChannelAsync(999, update);
        result.Should().BeNull();
    }

    #endregion
}
