// CRM Solution - Customer Relationship Management System
// Quote Repository Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Repositories;

/// <summary>
/// Unit tests for Quote Repository
/// Covers: Quote-specific queries, pricing, approval
/// </summary>
public class QuoteRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<QuoteEntity>> _mockDbSet;
    private readonly Mock<ILogger<QuoteRepository>> _mockLogger;
    private readonly QuoteRepository _repository;

    public QuoteRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<QuoteEntity>>();
        _mockLogger = new Mock<ILogger<QuoteRepository>>();

        _mockContext.Setup(c => c.Set<QuoteEntity>()).Returns(_mockDbSet.Object);
        _repository = new QuoteRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Draft" },
            new QuoteEntity { Id = 2, Status = "Draft" },
            new QuoteEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByStatusAsync("Draft");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDraftsAsync_ReturnsDraftQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Draft" },
            new QuoteEntity { Id = 2, Status = "Draft" },
            new QuoteEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetDraftsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSentAsync_ReturnsSentQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Sent" },
            new QuoteEntity { Id = 2, Status = "Sent" },
            new QuoteEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetSentAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAcceptedAsync_ReturnsAcceptedQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Accepted" },
            new QuoteEntity { Id = 2, Status = "Accepted" },
            new QuoteEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetAcceptedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRejectedAsync_ReturnsRejectedQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Rejected" },
            new QuoteEntity { Id = 2, Status = "Rejected" },
            new QuoteEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetRejectedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasQuotes_ReturnsAccountQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, AccountId = 1 },
            new QuoteEntity { Id = 2, AccountId = 1 },
            new QuoteEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByOpportunity Tests

    [Fact]
    public async Task GetByOpportunityAsync_HasQuotes_ReturnsOpportunityQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, OpportunityId = 1 },
            new QuoteEntity { Id = 2, OpportunityId = 1 },
            new QuoteEntity { Id = 3, OpportunityId = 2 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByOpportunityAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByQuoteNumber Tests

    [Fact]
    public async Task GetByQuoteNumberAsync_Exists_ReturnsQuote()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, QuoteNumber = "Q-001" },
            new QuoteEntity { Id = 2, QuoteNumber = "Q-002" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByQuoteNumberAsync("Q-001");

        // Assert
        result.Should().NotBeNull();
        result!.QuoteNumber.Should().Be("Q-001");
    }

    [Fact]
    public async Task GetByQuoteNumberAsync_NotExists_ReturnsNull()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, QuoteNumber = "Q-001" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByQuoteNumberAsync("Q-999");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Pricing Tests

    [Fact]
    public async Task GetByAmountRangeAsync_ReturnsInRange()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, TotalAmount = 5000 },
            new QuoteEntity { Id = 2, TotalAmount = 15000 },
            new QuoteEntity { Id = 3, TotalAmount = 50000 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByAmountRangeAsync(5000, 20000);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHighValueAsync_ReturnsHighValueQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, TotalAmount = 50000 },
            new QuoteEntity { Id = 2, TotalAmount = 75000 },
            new QuoteEntity { Id = 3, TotalAmount = 5000 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetHighValueAsync(25000);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Expiration Tests

    [Fact]
    public async Task GetExpiredAsync_ReturnsExpiredQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, ExpirationDate = DateTime.UtcNow.AddDays(-5), Status = "Sent" },
            new QuoteEntity { Id = 2, ExpirationDate = DateTime.UtcNow.AddDays(-1), Status = "Sent" },
            new QuoteEntity { Id = 3, ExpirationDate = DateTime.UtcNow.AddDays(10), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetExpiredAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExpiringSoonAsync_ReturnsExpiringSoon()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, ExpirationDate = DateTime.UtcNow.AddDays(3), Status = "Sent" },
            new QuoteEntity { Id = 2, ExpirationDate = DateTime.UtcNow.AddDays(5), Status = "Sent" },
            new QuoteEntity { Id = 3, ExpirationDate = DateTime.UtcNow.AddDays(30), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetExpiringSoonAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Approval Tests

    [Fact]
    public async Task GetPendingApprovalAsync_ReturnsPendingApproval()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, RequiresApproval = true, ApprovalStatus = "Pending" },
            new QuoteEntity { Id = 2, RequiresApproval = true, ApprovalStatus = "Pending" },
            new QuoteEntity { Id = 3, RequiresApproval = false }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetPendingApprovalAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetApprovedAsync_ReturnsApprovedQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, ApprovalStatus = "Approved" },
            new QuoteEntity { Id = 2, ApprovalStatus = "Approved" },
            new QuoteEntity { Id = 3, ApprovalStatus = "Pending" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetApprovedQuotesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatches()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Name = "Enterprise License Quote" },
            new QuoteEntity { Id = 2, Name = "Quote for Enterprise Package" },
            new QuoteEntity { Id = 3, Name = "Standard Package" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.SearchAsync("Enterprise");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Draft" },
            new QuoteEntity { Id = 2, Status = "Draft" },
            new QuoteEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Draft"].Should().Be(2);
    }

    [Fact]
    public async Task GetTotalValueAsync_CalculatesTotalValue()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, TotalAmount = 10000 },
            new QuoteEntity { Id = 2, TotalAmount = 20000 },
            new QuoteEntity { Id = 3, TotalAmount = 30000 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetTotalValueAsync();

        // Assert
        result.Should().Be(60000);
    }

    [Fact]
    public async Task GetAverageValueAsync_CalculatesAverageValue()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, TotalAmount = 10000 },
            new QuoteEntity { Id = 2, TotalAmount = 20000 },
            new QuoteEntity { Id = 3, TotalAmount = 30000 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetAverageValueAsync();

        // Assert
        result.Should().Be(20000);
    }

    [Fact]
    public async Task GetConversionRateAsync_CalculatesRate()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, Status = "Accepted" },
            new QuoteEntity { Id = 2, Status = "Accepted" },
            new QuoteEntity { Id = 3, Status = "Sent" },
            new QuoteEntity { Id = 4, Status = "Rejected" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetConversionRateAsync();

        // Assert
        result.Should().Be(50); // 2 accepted out of 4 = 50%
    }

    #endregion

    #region Owner Tests

    [Fact]
    public async Task GetByOwnerAsync_ReturnsOwnerQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, OwnerId = 1 },
            new QuoteEntity { Id = 2, OwnerId = 1 },
            new QuoteEntity { Id = 3, OwnerId = 2 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetByOwnerAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Discount Tests

    [Fact]
    public async Task GetWithDiscountAsync_ReturnsDiscountedQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, DiscountPercent = 10 },
            new QuoteEntity { Id = 2, DiscountPercent = 15 },
            new QuoteEntity { Id = 3, DiscountPercent = 0 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetWithDiscountAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetHighDiscountAsync_ReturnsHighDiscountQuotes()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, DiscountPercent = 25 },
            new QuoteEntity { Id = 2, DiscountPercent = 30 },
            new QuoteEntity { Id = 3, DiscountPercent = 10 }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetHighDiscountAsync(20);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new QuoteEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new QuoteEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlySentAsync_ReturnsRecentlySent()
    {
        // Arrange
        var quotes = new List<QuoteEntity>
        {
            new QuoteEntity { Id = 1, SentAt = DateTime.UtcNow.AddDays(-1), Status = "Sent" },
            new QuoteEntity { Id = 2, SentAt = DateTime.UtcNow.AddDays(-5), Status = "Sent" },
            new QuoteEntity { Id = 3, Status = "Draft" }
        }.AsQueryable();

        SetupMockDbSet(quotes);

        // Act
        var result = await _repository.GetRecentlySentAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<QuoteEntity> data)
    {
        _mockDbSet.As<IQueryable<QuoteEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<QuoteEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<QuoteEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<QuoteEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class QuoteEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string QuoteNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public int? AccountId { get; set; }
    public int? OpportunityId { get; set; }
    public int? OwnerId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public bool RequiresApproval { get; set; }
    public string? ApprovalStatus { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public bool IsDeleted { get; set; }
}
