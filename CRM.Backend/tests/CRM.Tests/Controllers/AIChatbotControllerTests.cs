// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AIChatbotController (TCOV-045).
/// </summary>
public class AIChatbotControllerTests : IDisposable
{
    private readonly Mock<ILLMService> _mockLLMService;
    private readonly Mock<ILLMSettingsService> _mockLLMSettingsService;
    private readonly Mock<ILogger<AIChatbotController>> _mockLogger;
    private readonly CrmDbContext _dbContext;
    private readonly AIChatbotController _controller;

    public AIChatbotControllerTests()
    {
        _mockLLMService = new Mock<ILLMService>();
        _mockLLMSettingsService = new Mock<ILLMSettingsService>();
        _mockLogger = new Mock<ILogger<AIChatbotController>>();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"AIChatbotTest_{Guid.NewGuid()}")
            .Options;
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        _dbContext = new CrmDbContext(options, config);

        _controller = new AIChatbotController(
            _dbContext,
            _mockLLMService.Object,
            _mockLLMSettingsService.Object,
            _mockLogger.Object);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public void Dispose() => _dbContext.Dispose();

    private void SetupLLMSettings(string fallbackProvider = "ollama")
    {
        _mockLLMSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto
            {
                EffectiveFallbackOrder = new List<string> { fallbackProvider },
                DefaultProvider = fallbackProvider
            });
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenProviderNotConfigured()
    {
        SetupLLMSettings("ollama");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenNoProvidersConfigured()
    {
        _mockLLMSettingsService.Setup(s => s.GetSettingsAsync())
            .ReturnsAsync(new LLMSettingsDto
            {
                EffectiveFallbackOrder = new List<string>(),
                DefaultProvider = "ollama"
            });
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenHealthCheckFails()
    {
        SetupLLMSettings("openai");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockLLMService.Setup(s => s.CompletionAsync(It.IsAny<LLMRequest>()))
            .ThrowsAsync(new InvalidOperationException("AI service error"));

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnOk_WhenProviderConfiguredAndHealthy()
    {
        SetupLLMSettings("openai");
        _mockLLMService.Setup(s => s.IsConfiguredAsync(It.IsAny<string>())).ReturnsAsync(true);
        _mockLLMService.Setup(s => s.CompletionAsync(It.IsAny<LLMRequest>()))
            .ReturnsAsync(new LLMResponse { Success = true, Model = "gpt-4o" });

        var result = await _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().NotBeNull();
    }

    [Fact]
    public void GetSuggestions_ShouldReturnOk()
    {
        var result = _controller.GetSuggestions();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SendMessage_ShouldReturnBadRequest_WhenMessageIsEmpty()
    {
        var request = new ChatMessageRequest { Message = "" };

        var result = await _controller.SendMessage(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
