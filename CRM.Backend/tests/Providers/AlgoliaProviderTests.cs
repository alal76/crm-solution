// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: AlgoliaProvider
// SDK Decision: AlgoliaProvider uses the Algolia .NET SDK (SearchClient) created internally.
// IsAvailableAsync is SYNCHRONOUS (_client != null check) — no HTTP is made.
// Tests with empty credentials give _client = null for fast, safe unit tests without
// any real Algolia API calls. A non-empty credential set shows IsAvailableAsync = true.
using CRM.Core.Entities;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Algolia;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="AlgoliaProvider"/>.
/// </summary>
public class AlgoliaProviderTests
{
    // ── Factory Helpers ─────────────────────────────────────────────────────

    private static AlgoliaProvider CreateProvider(AlgoliaConfiguration? config = null)
    {
        var cfg = config ?? new AlgoliaConfiguration
        {
            ApplicationId = "TESTAPPID01",
            ApiKey = "testapikey1234567890",
            IndexPrefix = "test_",
            DefaultPageSize = 20
        };

        var logger = new Mock<ILogger<AlgoliaProvider>>();
        return new AlgoliaProvider(Options.Create(cfg), logger.Object);
    }

    /// <summary>
    /// Creates a provider with empty credentials so _client becomes null.
    /// All operations on this provider are safe (no real HTTP calls).
    /// </summary>
    private static AlgoliaProvider CreateNullClientProvider() =>
        CreateProvider(new AlgoliaConfiguration
        {
            ApplicationId = string.Empty,
            ApiKey = string.Empty
        });

    // ── 1. ProviderName ─────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsAlgolia()
    {
        var provider = CreateNullClientProvider();

        provider.ProviderName.Should().Be("Algolia");
    }

    // ── 2. IsAvailableAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenBothCredentialsAreEmpty()
    {
        // _client == null → returns false synchronously (no HTTP call)
        var provider = CreateNullClientProvider();

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApplicationIdIsEmpty()
    {
        var provider = CreateProvider(new AlgoliaConfiguration
        {
            ApplicationId = string.Empty,
            ApiKey = "some-api-key-12345"
        });

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenApiKeyIsEmpty()
    {
        var provider = CreateProvider(new AlgoliaConfiguration
        {
            ApplicationId = "APPID123",
            ApiKey = string.Empty
        });

        var result = await provider.IsAvailableAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_ReturnsTrue_WhenCredentialsAreNonEmpty()
    {
        // Non-empty credentials → _client is initialized → returns true
        // IsAvailableAsync is Task.FromResult(_client != null), so no network I/O occurs.
        var provider = CreateProvider();

        var result = await provider.IsAvailableAsync();

        result.Should().BeTrue();
    }

    // ── 3. SearchAsync (non-generic) short-circuit paths ──────────────────

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenClientIsNull()
    {
        var provider = CreateNullClientProvider();
        var request = new SearchRequest { Query = "valid long query" };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsEmpty()
    {
        var provider = CreateNullClientProvider();
        var request = new SearchRequest { Query = "" };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsSingleCharacter()
    {
        // Algolia SearchAsync checks query.Length < 2 (same guard as Meilisearch)
        var provider = CreateNullClientProvider();
        var request = new SearchRequest { Query = "z" };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Query.Should().Be("z");
    }

    // ── 4. SearchAsync<T> (generic) short-circuit paths ───────────────────

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenClientIsNull()
    {
        var provider = CreateNullClientProvider();

        // Any non-empty query still short-circuits because _client == null
        var result = await provider.SearchAsync<Account>("hello world");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenQueryIsNull()
    {
        var provider = CreateNullClientProvider();

        var result = await provider.SearchAsync<Account>(null!);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenQueryIsEmpty()
    {
        var provider = CreateNullClientProvider();

        var result = await provider.SearchAsync<Account>("");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── 5. SuggestAsync short-circuit paths ───────────────────────────────

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenClientIsNull()
    {
        var provider = CreateNullClientProvider();

        var suggestions = await provider.SuggestAsync("test prefix");

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsEmpty()
    {
        var provider = CreateNullClientProvider();

        var suggestions = await provider.SuggestAsync(string.Empty);

        suggestions.Should().BeEmpty();
    }
}
