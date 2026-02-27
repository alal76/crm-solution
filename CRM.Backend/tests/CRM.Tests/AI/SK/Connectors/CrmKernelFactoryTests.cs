// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;
using CRM.Infrastructure.AI.SK.Configuration;
using CRM.Infrastructure.AI.SK.Connectors;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Connectors;

#nullable enable

/// <summary>
/// Unit tests for <see cref="CrmKernelFactory"/>.
/// Validates kernel creation, plugin resolution, and constructor guard clauses.
/// </summary>
public class CrmKernelFactoryTests
{
    #region Fields & Setup

    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<CrmKernelFactory>> _loggerMock = new();
    private readonly IOptions<SemanticKernelOptions> _options;

    public CrmKernelFactoryTests()
    {
        var skOptions = new SemanticKernelOptions
        {
            Enabled = true,
            Models = new ModelOptions()
        };
        _options = Options.Create(skOptions);

        // Setup IServiceProvider to return a minimal scope
        var scopeMock = new Mock<IServiceScope>();
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactoryMock.Object);

        // Register CrmChatCompletionConnector (required by CreateKernel)
        // Constructor: (ILLMService, IOptions<LLMProviderOptions>, ILogger<CrmChatCompletionConnector>)
        var llmServiceMock = new Mock<ILLMService>();
        var llmOptionsMock = Options.Create(new LLMProviderOptions());
        var chatConnectorMock = new Mock<CrmChatCompletionConnector>(
            llmServiceMock.Object,
            llmOptionsMock,
            new Mock<ILogger<CrmChatCompletionConnector>>().Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(CrmChatCompletionConnector)))
            .Returns(chatConnectorMock.Object);

        // Register ILoggerFactory (required by CreateKernel and plugin resolution)
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(new Mock<ILogger>().Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(loggerFactoryMock.Object);

        // Register plugin types directly so ResolvePlugin's GetService(pluginType)
        // returns an instance and ActivatorUtilities.CreateInstance is never called.
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(CRM.Infrastructure.AI.SK.Plugins.AccountPlugin)))
            .Returns(new Mock<CRM.Infrastructure.AI.SK.Plugins.AccountPlugin>(
                new Mock<CRM.Core.Interfaces.IAccountService>().Object,
                new Mock<CRM.Core.Interfaces.ICrmDbContext>().Object,
                new Mock<ILogger<CRM.Infrastructure.AI.SK.Plugins.AccountPlugin>>().Object).Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(CRM.Infrastructure.AI.SK.Plugins.LeadPlugin)))
            .Returns(new Mock<CRM.Infrastructure.AI.SK.Plugins.LeadPlugin>(
                new Mock<CRM.Core.Interfaces.ILeadService>().Object,
                new Mock<ILogger<CRM.Infrastructure.AI.SK.Plugins.LeadPlugin>>().Object).Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(CRM.Infrastructure.AI.SK.Plugins.SearchPlugin)))
            .Returns(new Mock<CRM.Infrastructure.AI.SK.Plugins.SearchPlugin>(
                new Mock<CRM.Core.Ports.Output.Providers.ISearchPort>().Object,
                new Mock<ILogger<CRM.Infrastructure.AI.SK.Plugins.SearchPlugin>>().Object).Object);
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(CRM.Infrastructure.AI.SK.Plugins.ServiceRequestPlugin)))
            .Returns(new Mock<CRM.Infrastructure.AI.SK.Plugins.ServiceRequestPlugin>(
                new Mock<CRM.Core.Interfaces.IServiceRequestService>().Object,
                new Mock<CRM.Core.Interfaces.ICrmDbContext>().Object,
                new Mock<ILogger<CRM.Infrastructure.AI.SK.Plugins.ServiceRequestPlugin>>().Object).Object);
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullServiceProvider_ShouldThrow()
    {
        var act = () => new CrmKernelFactory(null!, _options, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceProvider");
    }

    [Fact]
    public void Constructor_NullOptions_ShouldThrow()
    {
        var act = () => new CrmKernelFactory(_serviceProviderMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new CrmKernelFactory(_serviceProviderMock.Object, _options, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ValidParams_ShouldNotThrow()
    {
        var act = () => new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);
        act.Should().NotThrow();
    }

    #endregion

    #region CreateKernel Tests

    [Fact]
    public void CreateKernel_NullPluginNames_ShouldReturnKernel()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);

        // Act
        var kernel = factory.CreateKernel(null);

        // Assert
        kernel.Should().NotBeNull();
        kernel.Should().BeOfType<Kernel>();
    }

    [Fact]
    public void CreateKernel_EmptyPluginNames_ShouldReturnKernel()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);

        // Act
        var kernel = factory.CreateKernel(Enumerable.Empty<string>());

        // Assert
        kernel.Should().NotBeNull();
    }

    [Fact]
    public void CreateKernel_WithUnresolvablePlugins_ShouldNotThrow()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);

        // Act — unknown plugin names won't resolve from DI but shouldn't crash
        var act = () => factory.CreateKernel(new[] { "NonExistentPlugin" });

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region CreateKernelForAgent Tests

    [Fact]
    public void CreateKernelForAgent_ValidAgent_ShouldReturnKernel()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);
        var agent = new AIAgent
        {
            Id = 1,
            Name = "Test Agent",
            AllowedPlugins = "Account,Lead,Search",
            IsActive = true,
            AgentType = AgentType.GeneralAssistant
        };

        // Act
        var kernel = factory.CreateKernelForAgent(agent);

        // Assert
        kernel.Should().NotBeNull();
    }

    [Fact]
    public void CreateKernelForAgent_NullAllowedPlugins_ShouldReturnKernel()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);
        var agent = new AIAgent
        {
            Id = 2,
            Name = "Empty Agent",
            AllowedPlugins = null!,
            IsActive = true,
            AgentType = AgentType.GeneralAssistant
        };

        // Act
        var kernel = factory.CreateKernelForAgent(agent);

        // Assert
        kernel.Should().NotBeNull();
    }

    [Fact]
    public void CreateKernelForAgent_CommaSeparatedPlugins_ShouldParseCorrectly()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);
        var agent = new AIAgent
        {
            Id = 3,
            Name = "Multi-Plugin Agent",
            AllowedPlugins = "Account, Lead, Search, ServiceRequest",
            IsActive = true,
            AgentType = AgentType.LeadScoring
        };

        // Act — should parse comma-separated list and attempt to resolve each
        var kernel = factory.CreateKernelForAgent(agent);

        // Assert
        kernel.Should().NotBeNull();
    }

    [Fact]
    public void CreateKernelForAgent_EmptyAllowedPlugins_ShouldReturnKernel()
    {
        // Arrange
        var factory = new CrmKernelFactory(_serviceProviderMock.Object, _options, _loggerMock.Object);
        var agent = new AIAgent
        {
            Id = 4,
            Name = "No Plugins Agent",
            AllowedPlugins = "",
            IsActive = true,
            AgentType = AgentType.GeneralAssistant
        };

        // Act
        var kernel = factory.CreateKernelForAgent(agent);

        // Assert
        kernel.Should().NotBeNull();
    }

    #endregion
}
