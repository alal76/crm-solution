// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities.AI;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Connectors;
using CRM.Infrastructure.AI.SK.Services;
using CRM.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Services;

#nullable enable

/// <summary>
/// Unit tests for <see cref="AgentExecutionService"/>.
/// Validates conversation management, rating, and constructor guards.
/// </summary>
public class AgentExecutionServiceTests
{
    #region Fields & Setup

    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<AgentExecutionService>> _loggerMock = new();
    private readonly IOptions<SemanticKernelOptions> _options;
    private readonly CrmKernelFactory _kernelFactory;

    public AgentExecutionServiceTests()
    {
        var skOptions = new SemanticKernelOptions { Enabled = true };
        _options = Options.Create(skOptions);

        // Build a real CrmKernelFactory with minimal DI
        var serviceProviderMock = new Mock<IServiceProvider>();
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(serviceProviderMock.Object);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        var factoryLoggerMock = new Mock<ILogger<CrmKernelFactory>>();
        _kernelFactory = new CrmKernelFactory(
            serviceProviderMock.Object, _options, factoryLoggerMock.Object);
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullDbContext_ShouldThrow()
    {
        var act = () => new AgentExecutionService(null!, _kernelFactory, _options, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_NullKernelFactory_ShouldThrow()
    {
        var act = () => new AgentExecutionService(_dbContextMock.Object, null!, _options, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("kernelFactory");
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrow()
    {
        var act = () => new AgentExecutionService(_dbContextMock.Object, _kernelFactory, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new AgentExecutionService(_dbContextMock.Object, _kernelFactory, _options, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => new AgentExecutionService(_dbContextMock.Object, _kernelFactory, _options, _loggerMock.Object);
        act.Should().NotThrow();
    }

    #endregion

    #region ChatAsync Validation Tests

    [Fact]
    public async Task ChatAsync_EmptyMessage_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.ChatAsync(1, 1, string.Empty);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ChatAsync_NullMessage_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = async () => await service.ChatAsync(1, 1, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ChatAsync_AgentNotFound_ShouldThrowException()
    {
        // Arrange
        var service = CreateService();
        SetupEmptyAgentDbSet();

        // Act
        var act = async () => await service.ChatAsync(999, 1, "Hello");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region RateConversationAsync Tests

    [Fact]
    public async Task RateConversationAsync_ConversationNotFound_ShouldThrowException()
    {
        // Arrange
        var service = CreateService();
        SetupEmptyConversationDbSet();

        // Act
        var act = async () => await service.RateConversationAsync(999, 5, "Great!");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task RateConversationAsync_InvalidRating_ShouldThrowArgumentException()
    {
        // Arrange
        var service = CreateService();

        // Act — rating out of valid range
        var act = async () => await service.RateConversationAsync(1, 10, "Too high");

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region GetConversationHistoryAsync Tests

    [Fact]
    public async Task GetConversationHistoryAsync_ConversationNotFound_ShouldThrowException()
    {
        // Arrange
        var service = CreateService();
        SetupEmptyConversationDbSet();

        // Act
        var act = async () => await service.GetConversationHistoryAsync(999);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region CloseConversationAsync Tests

    [Fact]
    public async Task CloseConversationAsync_ConversationNotFound_ShouldThrowException()
    {
        // Arrange
        var service = CreateService();
        SetupEmptyConversationDbSet();

        // Act
        var act = async () => await service.CloseConversationAsync(999);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region ChatMessageRecord Tests

    [Fact]
    public void ChatMessageRecord_ShouldStoreRoleAndContent()
    {
        // Arrange & Act
        var record = new ChatMessageRecord("user", "Hello, agent!");

        // Assert
        record.Role.Should().Be("user");
        record.Content.Should().Be("Hello, agent!");
    }

    [Fact]
    public void ChatMessageRecord_Equality_ShouldWorkByValue()
    {
        // Record types support value equality
        var a = new ChatMessageRecord("assistant", "Hi there!");
        var b = new ChatMessageRecord("assistant", "Hi there!");

        a.Should().Be(b);
    }

    #endregion

    #region Helpers

    private AgentExecutionService CreateService()
    {
        return new AgentExecutionService(_dbContextMock.Object, _kernelFactory, _options, _loggerMock.Object);
    }

    private void SetupEmptyAgentDbSet()
    {
        var data = new List<AIAgent>().AsQueryable();
        var mockSet = new Mock<DbSet<AIAgent>>();
        mockSet.As<IQueryable<AIAgent>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<AIAgent>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<AIAgent>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<AIAgent>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        _dbContextMock.Setup(db => db.AIAgents).Returns(mockSet.Object);
    }

    private void SetupEmptyConversationDbSet()
    {
        var data = new List<AgentConversation>().AsQueryable();
        var mockSet = new Mock<DbSet<AgentConversation>>();
        mockSet.As<IQueryable<AgentConversation>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<AgentConversation>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<AgentConversation>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<AgentConversation>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        _dbContextMock.Setup(db => db.AgentConversations).Returns(mockSet.Object);
    }

    #endregion
}
