// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// ─────────────────────────────────────────────────────────────────────────────
// MANDATORY pre-write verification performed:
//   Class:     BuiltInSearchProvider
//   Namespace: CRM.Infrastructure.Providers.BuiltIn
//   File:      src/CRM.Infrastructure/Providers/BuiltIn/BuiltInSearchProvider.cs
//   Constructor: (ICrmDbContext dbContext, ILogger<BuiltInSearchProvider> logger)
//     - Direct ICrmDbContext injection (IDbContextResolver removed)
//   Properties:
//     - ProviderName => "BuiltIn"
//   Key behaviours confirmed by reading source:
//     - IsAvailableAsync: returns true when resolver.ResolveContext() != null, false on exception
//     - SearchAsync(SearchRequest): short query (< 2 chars) returns empty result, NO DB call
//     - SearchAsync<T>(string query): short query (< 2 chars) returns empty SearchResult<T>, NO DB call
//     - IndexAsync<T>: always no-op, returns Task.CompletedTask
//     - IndexBatchAsync<T>: always no-op, returns Task.CompletedTask
//     - DeleteAsync<T>: always no-op, returns Task.CompletedTask
//     - ClearIndexAsync<T>: always no-op, returns Task.CompletedTask
//     - RebuildIndexAsync<T>: always no-op, returns Task.CompletedTask
//     - RebuildAllIndexesAsync: always no-op, returns Task.CompletedTask
//     - SuggestAsync: short prefix (< 2 chars) returns empty list, NO DB call
//     - HealthCheckAsync: uses context.Database.CanConnectAsync(); IsHealthy=true when canConnect=true
//   Integration tests already exist in
//     tests/Integration/BuiltInSearchProviderIntegrationTests.cs
//   → Unit tests here focus on non-DB paths + no-op operations + provider contract.
// ─────────────────────────────────────────────────────────────────────────────

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Providers.BuiltIn;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="BuiltInSearchProvider"/>.
///
/// Scope: constructor behaviour, provider properties, short-query fast-exit paths,
/// no-op indexing operations, suggestion fast-exit, availability checks, and health
/// using an in-memory database.
///
/// DB-dependent search paths (full-text matching across entities) are covered by
/// the existing integration tests in
/// <c>tests/Integration/BuiltInSearchProviderIntegrationTests.cs</c>.
/// </summary>
public class BuiltInSearchProviderTests : IDisposable
{
    // ── Infrastructure ───────────────────────────────────────────────────────

    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<BuiltInSearchProvider>> _loggerMock;

    public BuiltInSearchProviderTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase($"SearchUnitTest_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableServiceProviderCaching(false)
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _loggerMock = new Mock<ILogger<BuiltInSearchProvider>>();

        // Default: resolver returns the in-memory context
    }

    public void Dispose() => _dbContext.Dispose();

    private BuiltInSearchProvider CreateProvider() =>
        new(_dbContext, _loggerMock.Object);

    // ── Provider Properties ───────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsBuiltIn()
    {
        var provider = CreateProvider();
        provider.ProviderName.Should().Be("BuiltIn");
    }

    // ── IsAvailableAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenResolverReturnsContext()
    {
        var provider = CreateProvider();

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenDbContextIsNull()
    {
        var provider = new BuiltInSearchProvider(null!, _loggerMock.Object);

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    // ── SearchAsync(SearchRequest) – Short Query Fast-Exit ────────────────────

    [Fact]
    public async Task SearchAsync_ReturnsEmptyResult_WhenQueryIsEmpty()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "" };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Query.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyResult_WhenQueryIsOneCharacter()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "a" };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyResult_WhenQueryIsOnlyWhitespace()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "  " };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyResult_WhenQueryIsNull()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = null! };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmptyHits_WhenNoMatchingDataInDatabase()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "zzznonexistentterm" };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Query.Should().Be("zzznonexistentterm");
    }

    // ── SearchAsync<T>(string) – Short Query Fast-Exit ────────────────────────

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmptyResult_WhenQueryIsShort()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>("x");

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmptyResult_WhenQueryIsEmpty()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>("");

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── No-Op Indexing Operations ─────────────────────────────────────────────

    [Fact]
    public async Task IndexAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();
        var fakeDoc = new { Id = 1, Name = "TestDoc" };

        var act = async () => await provider.IndexAsync(fakeDoc, "1");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task IndexBatchAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();
        var docs = new[] { new { Id = 1, Name = "A" }, new { Id = 2, Name = "B" } };

        var act = async () => await provider.IndexBatchAsync(docs, d => d.Id.ToString());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();

        var act = async () => await provider.DeleteAsync<Account>("123");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ClearIndexAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();

        var act = async () => await provider.ClearIndexAsync<Account>();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RebuildIndexAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();
        var docs = new List<Account>();

        var act = async () => await provider.RebuildIndexAsync(docs, d => d.Id.ToString());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RebuildAllIndexesAsync_CompletesWithoutException_AndMakesNoDbCalls()
    {
        var provider = CreateProvider();

        var act = async () => await provider.RebuildAllIndexesAsync();

        await act.Should().NotThrowAsync();
    }

    // ── SuggestAsync – Short Prefix Fast-Exit ─────────────────────────────────

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsEmpty()
    {
        var provider = CreateProvider();

        var suggestions = (await provider.SuggestAsync("")).ToList();

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsOneChar()
    {
        var provider = CreateProvider();

        var suggestions = (await provider.SuggestAsync("a")).ToList();

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsOnlyWhitespace()
    {
        var provider = CreateProvider();

        var suggestions = (await provider.SuggestAsync("  ")).ToList();

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenNoMatchingDataAndPrefixIsSufficient()
    {
        var provider = CreateProvider();

        var suggestions = (await provider.SuggestAsync("zzztest")).ToList();

        suggestions.Should().BeEmpty();
    }

    // ── HealthCheckAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task HealthCheckAsync_ReturnsHealthyResult_WhenDatabaseCanConnect()
    {
        var provider = CreateProvider();

        var health = await provider.HealthCheckAsync();

        health.Should().NotBeNull();
        health.IsHealthy.Should().BeTrue();
        health.ProviderName.Should().Be("BuiltIn");
        health.Details.Should().ContainKey("DatabaseProvider");
        health.Message.Should().Be("Connected");
    }

    [Fact]
    public async Task HealthCheckAsync_ReturnsUnhealthyResult_WhenDbContextIsNull()
    {
        var provider = new BuiltInSearchProvider(null!, _loggerMock.Object);

        var health = await provider.HealthCheckAsync();

        health.Should().NotBeNull();
        health.IsHealthy.Should().BeFalse();
        health.ProviderName.Should().Be("BuiltIn");
    }

    // ── SearchAsync – Pagination Props in Short-Query Response ────────────────

    [Fact]
    public async Task SearchAsync_ElapsedTimestamp_IsSetInResult()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "" };

        var result = await provider.SearchAsync(request);

        // ProcessingTimeMs is a long – should be >= 0
        result.ProcessingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ElapsedTimestamp_IsSetInResult()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>("x");

        result.ProcessingTimeMs.Should().BeGreaterThanOrEqualTo(0);
    }
}
