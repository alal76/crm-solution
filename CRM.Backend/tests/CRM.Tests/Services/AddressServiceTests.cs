// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for AddressService covering address management operations.
/// Tests cover: CRUD operations, validation, primary address logic, and soft deletes.
///
/// FUNCTIONAL VIEW:
/// - Tests address creation with valid and invalid data
/// - Tests address updates with field validation
/// - Tests soft delete behavior
/// - Tests retrieval of addresses and primary addresses
/// - Tests primary address flag management
///
/// TECHNICAL VIEW:
/// - Uses Moq to mock ICrmDbContext
/// - Tests async operations with proper task handling
/// - Verifies database context interactions
/// - Tests error handling and validation
/// </summary>
public class AddressServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<AddressService>> _mockLogger;
    private readonly AddressService _service;

    // Test data
    private readonly Account _testAccount;
    private readonly Address _testAddress;
    private readonly Address _testAddress2;

    public AddressServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<AddressService>>();
        _service = new AddressService(_mockContext.Object, _mockLogger.Object);

        // Setup test data
        _testAccount = new Account
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test Company",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _testAddress = new Address
        {
            Id = 1,
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            CountryCode = "US",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPrimary = true
        };

        _testAddress2 = new Address
        {
            Id = 2,
            Label = "Branch Office",
            Line1 = "456 Oak Avenue",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90001",
            Country = "United States",
            CountryCode = "US",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPrimary = false
        };
    }

    #region CreateAddressAsync Tests

    [Fact]
    public async Task CreateAddressAsync_ShouldCreateValidAddress_WhenInputIsValid()
    {
        // Arrange
        var newAddress = new Address
        {
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            CountryCode = "US"
        };

        SetupMockDbContext(new List<Account> { _testAccount }, new List<Address>());

        // Act
        var result = await _service.CreateAddressAsync(1, newAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Line1.Should().Be("123 Main Street");
        result.City.Should().Be("New York");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.IsDeleted.Should().BeFalse();

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAddressAsync_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        var newAddress = new Address
        {
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States"
        };

        SetupMockDbContext(new List<Account>(), new List<Address>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAddressAsync(999, newAddress, CancellationToken.None));

        exception.Message.Should().Contain("Account with ID 999 not found");
    }

    [Fact]
    public async Task CreateAddressAsync_ShouldThrowException_WhenLine1Missing()
    {
        // Arrange
        var newAddress = new Address
        {
            Line1 = "",  // Empty
            City = "New York",
            Country = "United States"
        };

        SetupMockDbContext(new List<Account> { _testAccount }, new List<Address>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAddressAsync(1, newAddress, CancellationToken.None));

        exception.Message.Should().Contain("Address Line1");
    }

    [Fact]
    public async Task CreateAddressAsync_ShouldThrowException_WhenCityMissing()
    {
        // Arrange
        var newAddress = new Address
        {
            Line1 = "123 Main Street",
            City = "",  // Empty
            Country = "United States"
        };

        SetupMockDbContext(new List<Account> { _testAccount }, new List<Address>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAddressAsync(1, newAddress, CancellationToken.None));

        exception.Message.Should().Contain("Address City");
    }

    [Fact]
    public async Task CreateAddressAsync_ShouldThrowException_WhenCountryMissing()
    {
        // Arrange
        var newAddress = new Address
        {
            Line1 = "123 Main Street",
            City = "New York",
            Country = ""  // Empty
        };

        SetupMockDbContext(new List<Account> { _testAccount }, new List<Address>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAddressAsync(1, newAddress, CancellationToken.None));

        exception.Message.Should().Contain("Address Country");
    }

    [Fact]
    public async Task CreateAddressAsync_ShouldSetDefaultLabel_WhenLabelNotProvided()
    {
        // Arrange
        var newAddress = new Address
        {
            Label = null,  // Not provided
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States"
        };

        SetupMockDbContext(new List<Account> { _testAccount }, new List<Address>());

        // Act
        var result = await _service.CreateAddressAsync(1, newAddress, CancellationToken.None);

        // Assert
        result.Label.Should().Be("Primary");
    }

    #endregion

    #region UpdateAddressAsync Tests

    [Fact]
    public async Task UpdateAddressAsync_ShouldUpdateValidAddress_WhenInputIsValid()
    {
        // Arrange
        var existingLink = new EntityAddressLink
        {
            Id = 1,
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            IsDeleted = false
        };

        var updatedAddress = new Address
        {
            Line1 = "456 New Street",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            Country = "United States"
        };

        SetupMockDbContextForUpdate(new List<Account> { _testAccount },
            new List<Address> { _testAddress },
            new List<EntityAddressLink> { existingLink });

        // Act
        var result = await _service.UpdateAddressAsync(1, 1, updatedAddress, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Line1.Should().Be("456 New Street");
        result.City.Should().Be("Boston");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAddressAsync_ShouldThrowException_WhenAddressNotFound()
    {
        // Arrange
        var updatedAddress = new Address
        {
            Line1 = "456 New Street",
            City = "Boston",
            Country = "United States"
        };

        SetupMockDbContextForUpdate(new List<Account> { _testAccount },
            new List<Address>(),
            new List<EntityAddressLink>());

        // Act & Assert - Service throws ArgumentException when address not found
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAddressAsync(1, 999, updatedAddress, CancellationToken.None));

        exception.Message.Should().Contain("999");
    }

    [Fact]
    public async Task UpdateAddressAsync_ShouldValidateRequiredFields_OnUpdate()
    {
        // Arrange
        var existingLink = new EntityAddressLink
        {
            Id = 1,
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            IsDeleted = false
        };

        var invalidAddress = new Address
        {
            Line1 = "",  // Invalid
            City = "Boston",
            Country = "United States"
        };

        SetupMockDbContextForUpdate(new List<Account> { _testAccount },
            new List<Address> { _testAddress },
            new List<EntityAddressLink> { existingLink });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateAddressAsync(1, 1, invalidAddress, CancellationToken.None));

        exception.Message.Should().Contain("Address Line1");
    }

    #endregion

    #region DeleteAddressAsync Tests

    [Fact]
    public async Task DeleteAddressAsync_ShouldSoftDeleteAddress_WhenValid()
    {
        // Arrange
        var existingLink = new EntityAddressLink
        {
            Id = 1,
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            IsDeleted = false
        };

        SetupMockDbContextForDelete(new List<Account> { _testAccount },
            new List<Address> { _testAddress },
            new List<EntityAddressLink> { existingLink });

        // Act
        var result = await _service.DeleteAddressAsync(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAddressAsync_ShouldReturnFalse_WhenAddressNotFound()
    {
        // Arrange
        SetupMockDbContextForDelete(new List<Account> { _testAccount },
            new List<Address>(),
            new List<EntityAddressLink>());

        // Act
        var result = await _service.DeleteAddressAsync(1, 999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAddressAsync_ShouldReturnFalse_WhenAddressNotLinkedToAccount()
    {
        // Arrange
        SetupMockDbContextForDelete(new List<Account> { _testAccount },
            new List<Address> { _testAddress },
            new List<EntityAddressLink>());  // No link

        // Act
        var result = await _service.DeleteAddressAsync(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAddressAsync_ShouldThrowException_WhenAccountNotFound()
    {
        // Arrange
        SetupMockDbContextForDelete(new List<Account>(),
            new List<Address> { _testAddress },
            new List<EntityAddressLink>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.DeleteAddressAsync(999, 1, CancellationToken.None));

        exception.Message.Should().Contain("Account with ID 999 not found");
    }

    #endregion

    #region GetAddressesByAccountAsync Tests

    [Fact]
    public async Task GetAddressesByAccountAsync_ShouldReturnAllAddresses_WhenAccountValid()
    {
        // Arrange
        var links = new List<EntityAddressLink>
        {
            new() { AddressId = 1, EntityId = 1, EntityType = EntityType.Account, IsDeleted = false },
            new() { AddressId = 2, EntityId = 1, EntityType = EntityType.Account, IsDeleted = false }
        };

        SetupMockDbContextForGetAddresses(
            new List<Address> { _testAddress, _testAddress2 },
            links);

        // Act
        var result = await _service.GetAddressesByAccountAsync(1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == 1);
        result.Should().Contain(a => a.Id == 2);
    }

    [Fact]
    public async Task GetAddressesByAccountAsync_ShouldNotReturnDeletedAddresses_WhenSoftDeleted()
    {
        // Arrange
        var deletedAddress = new Address
        {
            Id = 3,
            Line1 = "789 Deleted St",
            City = "Chicago",
            Country = "United States",
            IsDeleted = true
        };

        var links = new List<EntityAddressLink>
        {
            new() { AddressId = 1, EntityId = 1, EntityType = EntityType.Account, IsDeleted = false },
            new() { AddressId = 3, EntityId = 1, EntityType = EntityType.Account, IsDeleted = false }
        };

        SetupMockDbContextForGetAddresses(
            new List<Address> { _testAddress, deletedAddress },
            links);

        // Act
        var result = await _service.GetAddressesByAccountAsync(1, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Should().NotContain(a => a.Id == 3);
    }

    [Fact]
    public async Task GetAddressesByAccountAsync_ShouldReturnEmpty_WhenAccountHasNoAddresses()
    {
        // Arrange
        var links = new List<EntityAddressLink>();
        SetupMockDbContextForGetAddresses(new List<Address>(), links);

        // Act
        var result = await _service.GetAddressesByAccountAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetAddressByIdAsync Tests

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnAddress_WhenAddressExists()
    {
        // Arrange
        var addresses = new List<Address> { _testAddress };
        SetupMockQueryable(addresses);

        // Act
        var result = await _service.GetAddressByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(_testAddress);
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldReturnNull_WhenAddressDoesNotExist()
    {
        // Arrange
        var addresses = new List<Address>();
        SetupMockQueryable(addresses);

        // Act
        var result = await _service.GetAddressByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAddressByIdAsync_ShouldNotReturnDeletedAddress()
    {
        // Arrange
        var deletedAddress = new Address { Id = 1, IsDeleted = true, Line1 = "Deleted", City = "Deleted", Country = "Deleted" };
        var addresses = new List<Address> { deletedAddress };
        SetupMockQueryable(addresses);

        // Act
        var result = await _service.GetAddressByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPrimaryBillingAddressAsync Tests

    [Fact]
    public async Task GetPrimaryBillingAddressAsync_ShouldReturnPrimaryBillingAddress_WhenExists()
    {
        // Arrange
        var link = new EntityAddressLink
        {
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing,
            IsPrimary = true,
            IsDeleted = false
        };

        SetupMockDbContextForPrimaryAddress(
            new List<Address> { _testAddress },
            new List<EntityAddressLink> { link });

        // Act
        var result = await _service.GetPrimaryBillingAddressAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetPrimaryBillingAddressAsync_ShouldReturnNull_WhenNoPrimaryBilling()
    {
        // Arrange
        SetupMockDbContextForPrimaryAddress(
            new List<Address> { _testAddress },
            new List<EntityAddressLink>());

        // Act
        var result = await _service.GetPrimaryBillingAddressAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPrimaryShippingAddressAsync Tests

    [Fact]
    public async Task GetPrimaryShippingAddressAsync_ShouldReturnPrimaryShippingAddress_WhenExists()
    {
        // Arrange
        var link = new EntityAddressLink
        {
            AddressId = 2,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Shipping,
            IsPrimary = true,
            IsDeleted = false
        };

        SetupMockDbContextForPrimaryAddress(
            new List<Address> { _testAddress2 },
            new List<EntityAddressLink> { link });

        // Act
        var result = await _service.GetPrimaryShippingAddressAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
    }

    #endregion

    #region SetPrimaryBillingAddressAsync Tests

    [Fact]
    public async Task SetPrimaryBillingAddressAsync_ShouldSetPrimaryCorrectly_WhenValid()
    {
        // Arrange
        var link = new EntityAddressLink
        {
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing,
            IsPrimary = false,
            IsDeleted = false
        };

        SetupMockDbContextForSetPrimary(
            new List<Account> { _testAccount },
            new List<Address> { _testAddress },
            new List<EntityAddressLink> { link });

        // Act
        var result = await _service.SetPrimaryBillingAddressAsync(1, 1, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetPrimaryBillingAddressAsync_ShouldClearOtherPrimaryFlags_WhenSettingNew()
    {
        // Arrange
        var existingPrimary = new EntityAddressLink
        {
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing,
            IsPrimary = true,
            IsDeleted = false
        };

        var newPrimary = new EntityAddressLink
        {
            AddressId = 2,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing,
            IsPrimary = false,
            IsDeleted = false
        };

        SetupMockDbContextForSetPrimary(
            new List<Account> { _testAccount },
            new List<Address> { _testAddress, _testAddress2 },
            new List<EntityAddressLink> { existingPrimary, newPrimary });

        // Act
        var result = await _service.SetPrimaryBillingAddressAsync(1, 2, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SetPrimaryBillingAddressAsync_ShouldReturnFalse_WhenAddressNotFound()
    {
        // Arrange
        SetupMockDbContextForSetPrimary(
            new List<Account> { _testAccount },
            new List<Address>(),
            new List<EntityAddressLink>());

        // Act
        var result = await _service.SetPrimaryBillingAddressAsync(1, 999, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private void SetupMockDbContext(List<Account> accounts, List<Address> addresses)
    {
        var accountSet = CreateMockDbSet(accounts, a => a.Id);
        var addressSet = CreateMockDbSet(addresses, a => a.Id);

        _mockContext.Setup(c => c.Accounts).Returns(accountSet.Object);
        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
    }

    private void SetupMockDbContextForUpdate(List<Account> accounts, List<Address> addresses, List<EntityAddressLink> links)
    {
        var accountSet = CreateMockDbSet(accounts, a => a.Id);
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        var linkSet = CreateMockDbSet(links, l => l.Id);

        _mockContext.Setup(c => c.Accounts).Returns(accountSet.Object);
        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
        _mockContext.Setup(c => c.EntityAddressLinks).Returns(linkSet.Object);
    }

    private void SetupMockDbContextForDelete(List<Account> accounts, List<Address> addresses, List<EntityAddressLink> links)
    {
        var accountSet = CreateMockDbSet(accounts, a => a.Id);
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        var linkSet = CreateMockDbSet(links, l => l.Id);

        _mockContext.Setup(c => c.Accounts).Returns(accountSet.Object);
        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
        _mockContext.Setup(c => c.EntityAddressLinks).Returns(linkSet.Object);
    }

    private void SetupMockDbContextForGetAddresses(List<Address> addresses, List<EntityAddressLink> links)
    {
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        var linkSet = CreateMockDbSet(links, l => l.Id);

        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
        _mockContext.Setup(c => c.EntityAddressLinks).Returns(linkSet.Object);
    }

    private void SetupMockQueryable(List<Address> addresses)
    {
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
    }

    private void SetupMockDbContextForPrimaryAddress(List<Address> addresses, List<EntityAddressLink> links)
    {
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        var linkSet = CreateMockDbSet(links, l => l.Id);

        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
        _mockContext.Setup(c => c.EntityAddressLinks).Returns(linkSet.Object);
    }

    private void SetupMockDbContextForSetPrimary(List<Account> accounts, List<Address> addresses, List<EntityAddressLink> links)
    {
        var accountSet = CreateMockDbSet(accounts, a => a.Id);
        var addressSet = CreateMockDbSet(addresses, a => a.Id);
        var linkSet = CreateMockDbSet(links, l => l.Id);

        _mockContext.Setup(c => c.Accounts).Returns(accountSet.Object);
        _mockContext.Setup(c => c.Addresses).Returns(addressSet.Object);
        _mockContext.Setup(c => c.EntityAddressLinks).Returns(linkSet.Object);
    }

    private Mock<DbSet<T>> CreateMockDbSet<T, TKey>(List<T> items, Func<T, TKey> keySelector)
        where T : class
    {
        var queryable = items.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        // Async query provider for EF Core async operations (FirstOrDefaultAsync, ToListAsync, etc.)
        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(items.AsQueryable().GetEnumerator()));

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => items.AsQueryable().GetEnumerator());

        mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) =>
            {
                items.Add(entity);
            })
            .Returns(new ValueTask<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>>(
                (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<T>?)null!));

        mockSet.Setup(m => m.Add(It.IsAny<T>()))
            .Callback<T>(entity => items.Add(entity));

        mockSet.Setup(m => m.Update(It.IsAny<T>()))
            .Callback<T>(entity =>
            {
                var existing = items.FirstOrDefault(i => EqualityComparer<TKey>.Default.Equals(
                    keySelector(i), keySelector(entity)));
                if (existing != null)
                {
                    items.Remove(existing);
                    items.Add(entity);
                }
            });

        return mockSet;
    }

    #endregion

    #region Async Query Support Classes

    /// <summary>
    /// Async query provider implementation for EF Core mocking.
    /// </summary>
    private class TestAsyncQueryProvider<TEntity> : IQueryProvider, Microsoft.EntityFrameworkCore.Query.IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
            => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
            => new TestAsyncEnumerable<TElement>(expression);

        public object? Execute(System.Linq.Expressions.Expression expression) => _inner.Execute(expression);

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression) => _inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(name: nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(System.Linq.Expressions.Expression) })
                ?.MakeGenericMethod(expectedResultType)
                .Invoke(this, new object[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                ?.MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult })!;
        }
    }

    /// <summary>
    /// Async enumerable wrapper for EF Core mocking.
    /// </summary>
    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    /// <summary>
    /// Async enumerator wrapper for EF Core mocking.
    /// </summary>
    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync() => new(_inner.MoveNext());

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    #endregion
}
