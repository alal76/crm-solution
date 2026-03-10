// CRM Solution — CRM Test Suite
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
        result.Hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ShouldReturnEmptyResult()
    {
        var request = new CRM.Core.Ports.Output.Providers.SearchRequest { Query = "" };
        var result = await Create().SearchAsync(request);
        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
    }
}
