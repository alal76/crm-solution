// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Unit Tests: MeilisearchProvider
// SDK Decision: MeilisearchProvider uses Meilisearch .NET SDK (MeilisearchClient) created
// internally in the constructor — it cannot be mocked with HttpMessageHandler without
// refactoring the constructor. Tests focus on:
//   (a) ProviderName, (b) short-circuit code paths (empty/short queries),
//   (c) exception-handling paths via pre-cancelled CancellationTokens.
// Tests 8 and 9 (IsAvailableAsync / SearchAsync exception) accept either false or empty
// because the provider swallows exceptions and returns safe defaults.
using CRM.Core.Entities;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Meilisearch;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Providers;

/// <summary>
/// Unit tests for <see cref="MeilisearchProvider"/>.
/// </summary>
public class MeilisearchProviderTests
{
    // ── Factory Helpers ─────────────────────────────────────────────────────

    private static MeilisearchProvider CreateProvider(MeilisearchConfiguration? config = null)
    {
        var cfg = config ?? new MeilisearchConfiguration
        {
            Url = "http://localhost:7700",
            ApiKey = "test-master-key",
            IndexPrefix = "test_",
            DefaultPageSize = 20,
            EnableHighlighting = false
        };

        var logger = new Mock<ILogger<MeilisearchProvider>>();
        return new MeilisearchProvider(Options.Create(cfg), logger.Object);
    }

    private static CancellationToken CancelledToken()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        return cts.Token;
    }

    // ── 1. ProviderName ─────────────────────────────────────────────────────

    [Fact]
    public void ProviderName_ReturnsMeilisearch()
    {
        var provider = CreateProvider();

        provider.ProviderName.Should().Be("Meilisearch");
    }

    // ── 2. SearchAsync (non-generic) short-circuit paths ──────────────────

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsNull()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = null! };

        var result = await provider.SearchAsync(request);

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Query.Should().Be(string.Empty);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsEmpty()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "" };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsSingleCharacter()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "a" };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        // The original query is echoed back even on short-circuit
        result.Query.Should().Be("a");
    }

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenQueryIsWhitespace()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "  " };

        var result = await provider.SearchAsync(request);

        result.Hits.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── 3. SearchAsync<T> (generic) short-circuit paths ───────────────────

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenQueryIsNull()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>(null!);

        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenQueryIsEmpty()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>("");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchAsyncGeneric_ReturnsEmpty_WhenQueryIsSingleCharacter()
    {
        var provider = CreateProvider();

        var result = await provider.SearchAsync<Account>("x");

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── 4. SuggestAsync short-circuit paths ───────────────────────────────

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsEmpty()
    {
        var provider = CreateProvider();

        var suggestions = await provider.SuggestAsync(string.Empty);

        suggestions.Should().BeEmpty();
    }

    [Fact]
    public async Task SuggestAsync_ReturnsEmpty_WhenPrefixIsSingleCharacter()
    {
        var provider = CreateProvider();

        var suggestions = await provider.SuggestAsync("a");

        suggestions.Should().BeEmpty();
    }

    // ── 5. IsAvailableAsync exception handling ────────────────────────────

    [Fact]
    public async Task IsAvailableAsync_ReturnsFalse_WhenCancellationTokenAlreadyCancelled()
    {
        // A pre-cancelled token forces the SDK to throw OperationCanceledException,
        // which the provider catches and maps to false.
        var provider = CreateProvider();

        var result = await provider.IsAvailableAsync(CancelledToken());

        result.Should().BeFalse();
    }

    // ── 6. SearchAsync exception handling (pre-cancelled token) ───────────

    [Fact]
    public async Task SearchAsync_ReturnsEmpty_WhenCancellationTokenCancelledMidQuery()
    {
        var provider = CreateProvider();
        var request = new SearchRequest { Query = "valid long query" };

        // Provider catches OperationCanceledException from SDK and returns empty result
        var result = await provider.SearchAsync(request, CancelledToken());

        result.Should().NotBeNull();
        result.Hits.Should().BeEmpty();
    }
}
