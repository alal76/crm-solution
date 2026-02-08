// CRM Solution - Customer Relationship Management System
// Invoice Repository Unit Tests

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
/// Unit tests for Invoice Repository
/// Covers: Invoice-specific queries, payments, aging
/// </summary>
public class InvoiceRepositoryTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<DbSet<InvoiceEntity>> _mockDbSet;
    private readonly Mock<ILogger<InvoiceRepository>> _mockLogger;
    private readonly InvoiceRepository _repository;

    public InvoiceRepositoryTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockDbSet = new Mock<DbSet<InvoiceEntity>>();
        _mockLogger = new Mock<ILogger<InvoiceRepository>>();

        _mockContext.Setup(c => c.Set<InvoiceEntity>()).Returns(_mockDbSet.Object);
        _repository = new InvoiceRepository(_mockContext.Object, _mockLogger.Object);
    }

    #region GetByStatus Tests

    [Fact]
    public async Task GetByStatusAsync_HasMatches_ReturnsInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Draft" },
            new InvoiceEntity { Id = 2, Status = "Draft" },
            new InvoiceEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetByStatusAsync("Draft");

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDraftsAsync_ReturnsDraftInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Draft" },
            new InvoiceEntity { Id = 2, Status = "Draft" },
            new InvoiceEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetDraftsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSentAsync_ReturnsSentInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Sent" },
            new InvoiceEntity { Id = 2, Status = "Sent" },
            new InvoiceEntity { Id = 3, Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetSentAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaidAsync_ReturnsPaidInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Paid" },
            new InvoiceEntity { Id = 2, Status = "Paid" },
            new InvoiceEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetPaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetVoidedAsync_ReturnsVoidedInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Voided" },
            new InvoiceEntity { Id = 2, Status = "Voided" },
            new InvoiceEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetVoidedAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByAccount Tests

    [Fact]
    public async Task GetByAccountAsync_HasInvoices_ReturnsAccountInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AccountId = 1 },
            new InvoiceEntity { Id = 2, AccountId = 1 },
            new InvoiceEntity { Id = 3, AccountId = 2 }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetByInvoiceNumber Tests

    [Fact]
    public async Task GetByInvoiceNumberAsync_Exists_ReturnsInvoice()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, InvoiceNumber = "INV-001" },
            new InvoiceEntity { Id = 2, InvoiceNumber = "INV-002" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetByInvoiceNumberAsync("INV-001");

        // Assert
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().Be("INV-001");
    }

    #endregion

    #region Payment Status Tests

    [Fact]
    public async Task GetUnpaidAsync_ReturnsUnpaidInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AmountPaid = 0, TotalAmount = 1000, Status = "Sent" },
            new InvoiceEntity { Id = 2, AmountPaid = 0, TotalAmount = 2000, Status = "Sent" },
            new InvoiceEntity { Id = 3, AmountPaid = 3000, TotalAmount = 3000, Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetUnpaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPartiallyPaidAsync_ReturnsPartiallyPaidInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AmountPaid = 500, TotalAmount = 1000, Status = "Sent" },
            new InvoiceEntity { Id = 2, AmountPaid = 1000, TotalAmount = 2000, Status = "Sent" },
            new InvoiceEntity { Id = 3, AmountPaid = 0, TotalAmount = 3000, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetPartiallyPaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetFullyPaidAsync_ReturnsFullyPaidInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AmountPaid = 1000, TotalAmount = 1000, Status = "Paid" },
            new InvoiceEntity { Id = 2, AmountPaid = 2000, TotalAmount = 2000, Status = "Paid" },
            new InvoiceEntity { Id = 3, AmountPaid = 500, TotalAmount = 3000, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetFullyPaidAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Overdue Tests

    [Fact]
    public async Task GetOverdueAsync_ReturnsOverdueInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(-10), Status = "Sent" },
            new InvoiceEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(-5), Status = "Sent" },
            new InvoiceEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(10), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetOverdueAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDueSoonAsync_ReturnsDueSoonInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(3), Status = "Sent" },
            new InvoiceEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(5), Status = "Sent" },
            new InvoiceEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(30), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetDueSoonAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Aging Tests

    [Fact]
    public async Task GetAgingReportAsync_ReturnsAgingBuckets()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(-5), AmountDue = 1000, Status = "Sent" },
            new InvoiceEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(-35), AmountDue = 2000, Status = "Sent" },
            new InvoiceEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(-65), AmountDue = 3000, Status = "Sent" },
            new InvoiceEntity { Id = 4, DueDate = DateTime.UtcNow.AddDays(-95), AmountDue = 4000, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetAgingReportAsync();

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOverdue1To30Async_ReturnsCorrectInvoices()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, DueDate = DateTime.UtcNow.AddDays(-5), Status = "Sent" },
            new InvoiceEntity { Id = 2, DueDate = DateTime.UtcNow.AddDays(-25), Status = "Sent" },
            new InvoiceEntity { Id = 3, DueDate = DateTime.UtcNow.AddDays(-45), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetOverdue1To30Async();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Amount Tests

    [Fact]
    public async Task GetTotalOutstandingAsync_CalculatesTotalOutstanding()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AmountDue = 1000, Status = "Sent" },
            new InvoiceEntity { Id = 2, AmountDue = 2000, Status = "Sent" },
            new InvoiceEntity { Id = 3, AmountDue = 0, Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetTotalOutstandingAsync();

        // Assert
        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetTotalOverdueAsync_CalculatesTotalOverdue()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, AmountDue = 1000, DueDate = DateTime.UtcNow.AddDays(-10), Status = "Sent" },
            new InvoiceEntity { Id = 2, AmountDue = 2000, DueDate = DateTime.UtcNow.AddDays(-5), Status = "Sent" },
            new InvoiceEntity { Id = 3, AmountDue = 3000, DueDate = DateTime.UtcNow.AddDays(10), Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetTotalOverdueAsync();

        // Assert
        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetByAmountRangeAsync_ReturnsInRange()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, TotalAmount = 5000 },
            new InvoiceEntity { Id = 2, TotalAmount = 15000 },
            new InvoiceEntity { Id = 3, TotalAmount = 50000 }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetByAmountRangeAsync(5000, 20000);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByInvoiceNumber_ReturnsMatches()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, InvoiceNumber = "INV-2025-001" },
            new InvoiceEntity { Id = 2, InvoiceNumber = "INV-2025-002" },
            new InvoiceEntity { Id = 3, InvoiceNumber = "INV-2024-001" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.SearchAsync("2025");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetCountByStatusAsync_ReturnsStatusCounts()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Sent" },
            new InvoiceEntity { Id = 2, Status = "Sent" },
            new InvoiceEntity { Id = 3, Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetCountByStatusAsync();

        // Assert
        result["Sent"].Should().Be(2);
    }

    [Fact]
    public async Task GetMonthlyRevenueAsync_ReturnsMonthlyRevenue()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, InvoiceDate = DateTime.UtcNow, AmountPaid = 10000, Status = "Paid" },
            new InvoiceEntity { Id = 2, InvoiceDate = DateTime.UtcNow, AmountPaid = 20000, Status = "Paid" },
            new InvoiceEntity { Id = 3, InvoiceDate = DateTime.UtcNow.AddMonths(-1), AmountPaid = 15000, Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetMonthlyRevenueAsync(6);

        // Assert
        result.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetPaymentRateAsync_CalculatesRate()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, Status = "Paid" },
            new InvoiceEntity { Id = 2, Status = "Paid" },
            new InvoiceEntity { Id = 3, Status = "Sent" },
            new InvoiceEntity { Id = 4, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetPaymentRateAsync();

        // Assert
        result.Should().Be(50); // 2 paid out of 4 = 50%
    }

    [Fact]
    public async Task GetAverageDaysToPayAsync_CalculatesAverage()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, InvoiceDate = DateTime.UtcNow.AddDays(-30), PaidDate = DateTime.UtcNow.AddDays(-20), Status = "Paid" },
            new InvoiceEntity { Id = 2, InvoiceDate = DateTime.UtcNow.AddDays(-20), PaidDate = DateTime.UtcNow.AddDays(-5), Status = "Paid" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetAverageDaysToPayAsync();

        // Assert
        result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Date Range Tests

    [Fact]
    public async Task GetByDateRangeAsync_ReturnsInvoicesInRange()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, InvoiceDate = DateTime.UtcNow.AddDays(-5) },
            new InvoiceEntity { Id = 2, InvoiceDate = DateTime.UtcNow.AddDays(-15) },
            new InvoiceEntity { Id = 3, InvoiceDate = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetByDateRangeAsync(
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Recent Activity Tests

    [Fact]
    public async Task GetRecentlyCreatedAsync_ReturnsRecent()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
            new InvoiceEntity { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new InvoiceEntity { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-40) }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetRecentlyCreatedAsync(30);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentlyPaidAsync_ReturnsRecentlyPaid()
    {
        // Arrange
        var invoices = new List<InvoiceEntity>
        {
            new InvoiceEntity { Id = 1, PaidDate = DateTime.UtcNow.AddDays(-1), Status = "Paid" },
            new InvoiceEntity { Id = 2, PaidDate = DateTime.UtcNow.AddDays(-5), Status = "Paid" },
            new InvoiceEntity { Id = 3, Status = "Sent" }
        }.AsQueryable();

        SetupMockDbSet(invoices);

        // Act
        var result = await _repository.GetRecentlyPaidAsync(7);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbSet(IQueryable<InvoiceEntity> data)
    {
        _mockDbSet.As<IQueryable<InvoiceEntity>>().Setup(m => m.Provider).Returns(data.Provider);
        _mockDbSet.As<IQueryable<InvoiceEntity>>().Setup(m => m.Expression).Returns(data.Expression);
        _mockDbSet.As<IQueryable<InvoiceEntity>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _mockDbSet.As<IQueryable<InvoiceEntity>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
    }

    #endregion
}

// Supporting class
public class InvoiceEntity
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public int? AccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
