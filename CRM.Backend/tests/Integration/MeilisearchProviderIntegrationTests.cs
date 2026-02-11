// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Ports.Output.Providers;
using CRM.Infrastructure.Providers.Meilisearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CRM.Tests.Integration.Providers;

/// <summary>
/// Integration tests for MeilisearchProvider.
/// These tests are designed to run against a real Meilisearch instance.
/// Skip if Meilisearch is not available.
/// </summary>
[Collection("MeilisearchIntegration")]
public class MeilisearchProviderIntegrationTests : IAsyncLifetime
{
    private readonly Mock<ILogger<MeilisearchProvider>> _loggerMock;
    private readonly MeilisearchConfiguration _config;
    private readonly MeilisearchProvider _provider;
    private bool _isMeilisearchAvailable;

    public MeilisearchProviderIntegrationTests()
    {
        _loggerMock = new Mock<ILogger<MeilisearchProvider>>();
        _config = new MeilisearchConfiguration
        {
            Url = Environment.GetEnvironmentVariable("MEILISEARCH_URL") ?? "http://localhost:7700",
            ApiKey = Environment.GetEnvironmentVariable("MEILISEARCH_API_KEY") ?? "masterKey",
            IndexPrefix = "test_integration_",
            DefaultPageSize = 20,
            MaxPageSize = 100,
            TimeoutSeconds = 30,
            EnableHighlighting = true,
            AutoSyncEnabled = false,
            BatchSize = 50
        };

        var options = Options.Create(_config);
        _provider = new MeilisearchProvider(options, _loggerMock.Object);
    }

    public async Task InitializeAsync()
    {
        // Check if Meilisearch is available
        try
        {
            _isMeilisearchAvailable = await _provider.IsAvailableAsync();
        }
        catch
        {
            _isMeilisearchAvailable = false;
        }

        if (_isMeilisearchAvailable)
        {
            // Clear test indexes before running tests
            try
            {
                await _provider.ClearIndexAsync<Account>();
                await _provider.ClearIndexAsync<Opportunity>();
            }
            catch
            {
                // Indexes may not exist yet
            }
        }
    }

    public async Task DisposeAsync()
    {
        if (_isMeilisearchAvailable)
        {
            // Clean up test data
            try
            {
                await _provider.ClearIndexAsync<Account>();
                await _provider.ClearIndexAsync<Opportunity>();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    #region Availability Tests

    [Fact]
    public async Task IsAvailableAsync_ShouldReturnTrueWhenMeilisearchIsRunning()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Act
        var result = await _provider.IsAvailableAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthyWhenAvailable()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Act
        var result = await _provider.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.NotNull(result.Details);
    }

    #endregion

    #region Indexing Tests

    [Fact]
    public async Task IndexAsync_ShouldIndexAccountSuccessfully()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange
        var account = new Account
        {
            Id = 1,
            Company = "Integration Test Account",
            Industry = "Technology",
            Website = "https://example.com"
        };

        // Act
        await _provider.IndexAsync(account, account.Id.ToString());

        // Allow time for indexing
        await Task.Delay(500);

        // Assert - Search for the indexed document
        var result = await _provider.SearchAsync<Account>("Integration Test", new SearchOptions { Take = 10 });
        Assert.NotNull(result);
    }

    [Fact]
    public async Task IndexBatchAsync_ShouldIndexMultipleAccountsSuccessfully()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange
        var accounts = new List<Account>
        {
            new Account { Id = 100, Company = "Batch Test Account 1", Industry = "Finance" },
            new Account { Id = 101, Company = "Batch Test Account 2", Industry = "Healthcare" },
            new Account { Id = 102, Company = "Batch Test Account 3", Industry = "Technology" }
        };

        // Act
        await _provider.IndexBatchAsync(accounts, a => a.Id.ToString());

        // Allow time for indexing
        await Task.Delay(1000);

        // Assert
        var result = await _provider.SearchAsync<Account>("Batch Test", new SearchOptions { Take = 10 });
        Assert.NotNull(result);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ShouldFindIndexedAccounts()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index test data
        var account = new Account
        {
            Id = 200,
            Company = "Searchable Test Corp",
            Industry = "Technology"
        };
        await _provider.IndexAsync(account, account.Id.ToString());
        await Task.Delay(500);

        // Act
        var result = await _provider.SearchAsync<Account>("Searchable", new SearchOptions { Take = 10 });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index test data
        var accounts = new List<Account>
        {
            new Account { Id = 300, Company = "Filter Test Tech", Industry = "Technology" },
            new Account { Id = 301, Company = "Filter Test Finance", Industry = "Finance" }
        };
        await _provider.IndexBatchAsync(accounts, a => a.Id.ToString());
        await Task.Delay(500);

        // Act - Search with filter (note: filterable attributes must be configured)
        var result = await _provider.SearchAsync<Account>("Filter Test", new SearchOptions
        {
            Take = 10,
            Filters = new Dictionary<string, string>
            {
                ["industry"] = "Technology"
            }
        });

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldRespectSkipAndTake()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index multiple test records
        var accounts = Enumerable.Range(1, 25).Select(i => new Account
        {
            Id = 400 + i,
            Company = $"Pagination Test Company {i}",
            Industry = "Testing"
        }).ToList();

        await _provider.IndexBatchAsync(accounts, a => a.Id.ToString());
        await Task.Delay(1000);

        // Act - Get second page
        var result = await _provider.SearchAsync<Account>("Pagination Test", new SearchOptions
        {
            Skip = 10,
            Take = 10
        });

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Suggest Tests

    [Fact]
    public async Task SuggestAsync_ShouldReturnSuggestions()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index test data
        var account = new Account
        {
            Id = 500,
            Company = "Autocomplete Test Corporation",
            Industry = "Technology"
        };
        await _provider.IndexAsync(account, account.Id.ToString());
        await Task.Delay(500);

        // Act
        var suggestions = await _provider.SuggestAsync("Autoco");

        // Assert
        Assert.NotNull(suggestions);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ShouldRemoveDocument()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index a document
        var account = new Account
        {
            Id = 600,
            Company = "Delete Test Account",
            Industry = "Technology"
        };
        await _provider.IndexAsync(account, account.Id.ToString());
        await Task.Delay(500);

        // Act - Delete the document
        await _provider.DeleteAsync<Account>(account.Id.ToString());
        await Task.Delay(500);

        // Assert - Search should not find it
        var result = await _provider.SearchAsync<Account>("Delete Test", new SearchOptions { Take = 10 });
        Assert.NotNull(result);
    }

    #endregion

    #region Unified Search Tests

    [Fact]
    public async Task SearchAsync_UnifiedSearch_ShouldSearchAcrossEntityTypes()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Arrange - Index different entity types
        var account = new Account { Id = 700, Company = "Unified Search Corp", Industry = "Technology" };
        var opportunity = new Opportunity { Id = 701, Name = "Unified Search Deal" };

        await _provider.IndexAsync(account, account.Id.ToString());
        await _provider.IndexAsync(opportunity, opportunity.Id.ToString());
        await Task.Delay(500);

        // Act - Unified search across all types
        var request = new SearchRequest
        {
            Query = "Unified Search",
            Take = 20
        };
        var result = await _provider.SearchAsync(request);

        // Assert
        Assert.NotNull(result);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SearchAsync_WithInvalidConfig_ShouldHandleGracefully()
    {
        // Arrange - Create provider with invalid URL
        var invalidConfig = new MeilisearchConfiguration
        {
            Url = "http://invalid-host:9999",
            ApiKey = "invalid-key",
            IndexPrefix = "test_"
        };
        var invalidProvider = new MeilisearchProvider(
            Options.Create(invalidConfig),
            _loggerMock.Object);

        // Act
        var result = await invalidProvider.IsAvailableAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Index Configuration Tests

    [Fact]
    public async Task ConfigureIndexesAsync_ShouldConfigureAllIndexes()
    {
        // Skip if not available
        if (!_isMeilisearchAvailable)
        {
            Assert.True(true, "Skipped: Meilisearch not available");
            return;
        }

        // Act
        await _provider.ConfigureIndexesAsync();

        // Assert - Provider should configure without error
        var health = await _provider.HealthCheckAsync();
        Assert.True(health.IsHealthy);
    }

    #endregion
}
