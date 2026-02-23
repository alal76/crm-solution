// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for Contract validation logic in ContractService.
/// Tests EndDate > StartDate and TotalValue >= 0 validations.
/// </summary>
public class ContractValidationTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ContractService>> _mockLogger;
    private readonly ContractService _service;

    private readonly List<Contract> _contracts;
    private readonly List<Quote> _quotes;
    private readonly List<Order> _orders;

    public ContractValidationTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ContractService>>();

        _contracts = new List<Contract>();
        _quotes = new List<Quote>();
        _orders = new List<Order>();

        var mockContracts = MockDbSetFactory.CreateMockDbSet(_contracts);
        var mockQuotes = MockDbSetFactory.CreateMockDbSet(_quotes);
        var mockOrders = MockDbSetFactory.CreateMockDbSet(_orders);

        // Add FindAsync(object[], CancellationToken) overload
        mockContracts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) =>
            {
                var id = keys.FirstOrDefault();
                if (id == null)
                    return ValueTask.FromResult<Contract?>(default);
                return ValueTask.FromResult(_contracts.FirstOrDefault(e => e.Id == Convert.ToInt32(id)));
            });

        _mockContext.Setup(c => c.Contracts).Returns(mockContracts.Object);
        _mockContext.Setup(c => c.Quotes).Returns(mockQuotes.Object);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _service = new ContractService(_mockContext.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateAsync - Date Validation
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Test Contract",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 1, 1),
            Value = 1000m
        };

        // Act
        var act = async () => await _service.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenEndDateEqualsStartDate()
    {
        // Arrange
        var sameDate = new DateTime(2026, 6, 1);
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Test Contract",
            StartDate = sameDate,
            EndDate = sameDate,
            Value = 500m
        };

        // Act
        var act = async () => await _service.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    // ========================================================================
    // CreateAsync - Value Validation
    // ========================================================================

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenValueIsNegative()
    {
        // Arrange
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Test Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = -500m
        };

        // Act
        var act = async () => await _service.CreateAsync(contract);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Contract value must be zero or positive*");
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenValueIsZero()
    {
        // Arrange
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Zero Value Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = 0m
        };

        // Act
        var result = await _service.CreateAsync(contract);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().Be(0m);
        result.ContractNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenDatesAndValueAreValid()
    {
        // Arrange
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Valid Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2027, 1, 1),
            Value = 10000m
        };

        // Act
        var result = await _service.CreateAsync(contract);

        // Assert
        result.Should().NotBeNull();
        result.ContractNumber.Should().NotBeNullOrEmpty();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_ShouldSucceed_WhenDatesAreDefault()
    {
        // Arrange - default dates (DateTime.MinValue) should not trigger validation
        var contract = new Contract
        {
            AccountId = 1,
            Name = "Default Dates Contract",
            Value = 100m
        };

        // Act
        var result = await _service.CreateAsync(contract);

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // UpdateAsync - Date Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenEndDateBeforeStartDate()
    {
        // Arrange
        _contracts.Add(new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Existing Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = 1000m,
            IsDeleted = false
        });

        var updated = new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Updated Contract",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 1, 1),
            Value = 1000m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenEndDateEqualsStartDate()
    {
        // Arrange
        _contracts.Add(new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Existing Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = 1000m,
            IsDeleted = false
        });

        var sameDate = new DateTime(2026, 6, 1);
        var updated = new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Updated Contract",
            StartDate = sameDate,
            EndDate = sameDate,
            Value = 1000m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    // ========================================================================
    // UpdateAsync - Value Validation
    // ========================================================================

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenValueIsNegative()
    {
        // Arrange
        _contracts.Add(new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Existing Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = 1000m,
            IsDeleted = false
        });

        var updated = new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Updated Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = -100m
        };

        // Act
        var act = async () => await _service.UpdateAsync(updated);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Contract value must be zero or positive*");
    }

    [Fact]
    public async Task UpdateAsync_ShouldSucceed_WhenDatesAndValueAreValid()
    {
        // Arrange
        _contracts.Add(new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Existing Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Value = 1000m,
            IsDeleted = false
        });

        var updated = new Contract
        {
            Id = 1,
            AccountId = 1,
            Name = "Updated Contract",
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2027, 6, 30),
            Value = 2000m
        };

        // Act
        var result = await _service.UpdateAsync(updated);

        // Assert
        result.Should().NotBeNull();
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
