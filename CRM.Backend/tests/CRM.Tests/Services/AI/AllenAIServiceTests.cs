// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;
using CRM.Core.Interfaces.AI;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.AI;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Services.AI;

/// <summary>
/// Unit tests for AllenAIService (TCOV-001).
/// Uses InMemory CrmDbContext because AllenAIService takes the concrete CrmDbContext.
/// </summary>
public class AllenAIServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<AllenAIService>> _mockLogger;
    private readonly IOptions<AllenAIConfiguration> _options;
    private readonly IMemoryCache _memoryCache;
    private readonly AllenAIService _service;

    public AllenAIServiceTests()
    {
        var dbOptions = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var mockConfig = new Mock<IConfiguration>();
        _dbContext = new CrmDbContext(dbOptions, mockConfig.Object);

        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockLogger = new Mock<ILogger<AllenAIService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _options = Options.Create(new AllenAIConfiguration
        {
            OLMoEndpoint = string.Empty,
            TuluEndpoint = string.Empty,
            BatchSize = 10
        });
        _service = new AllenAIService(
            _dbContext,
            _mockHttpClientFactory.Object,
            _mockLogger.Object,
            _options,
            _memoryCache);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _memoryCache.Dispose();
    }

    [Fact]
    public void Constructor_ShouldCreateService_WithValidDependencies()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTopLeadsAsync_ShouldReturnEmpty_WhenNoLeadScoresExist()
    {
        var result = await _service.GetTopLeadsAsync(10);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnEmpty_WhenNoModelsExist()
    {
        var result = await _service.GetAvailableModelsAsync();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BatchScoreLeadsAsync_ShouldReturnEmpty_WhenLeadListIsEmpty()
    {
        var result = await _service.BatchScoreLeadsAsync(new List<int>());

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ScoreLeadAsync_ShouldThrow_WhenLeadDoesNotExist()
    {
        await Assert.ThrowsAnyAsync<Exception>(() => _service.ScoreLeadAsync(999));
    }

    [Fact]
    public async Task CheckModelHealthAsync_ShouldReturnFalse_WhenOLMoEndpointIsEmpty()
    {
        var result = await _service.CheckModelHealthAsync(AIProvider.AllenAI_OLMo);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckModelHealthAsync_ShouldReturnFalse_WhenProviderNotSupported()
    {
        // AIProvider.OpenAI (4) maps to null endpoint → returns false immediately
        var result = await _service.CheckModelHealthAsync(AIProvider.OpenAI);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnActiveModels_WhenModelsExist()
    {
        _dbContext.AIModels.Add(new AIModel
        {
            Name = "Test Model",
            Provider = AIProvider.AllenAI_OLMo,
            Status = AIModelStatus.Active,
            IsDeleted = false,
            Description = "Test"
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.GetAvailableModelsAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Test Model");
    }
}
