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

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ContactsService
/// Covers: Contact CRUD, search, pagination, validation, relationships
/// </summary>
public class ContactsServiceTests
{
    private readonly Mock<IRepository<Contact>> _mockContactRepository;
    private readonly Mock<IRepository<Account>> _mockAccountRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly Mock<ILogger<ContactsService>> _mockLogger;
    private readonly ContactsService _service;

    public ContactsServiceTests()
    {
        _mockContactRepository = new Mock<IRepository<Contact>>();
        _mockAccountRepository = new Mock<IRepository<Account>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockNotificationService = new Mock<ICrmNotificationService>();
        _mockLogger = new Mock<ILogger<ContactsService>>();

        _service = new ContactsService(
            _mockContactRepository.Object,
            _mockAccountRepository.Object,
            _mockDbContext.Object,
            _mockNotificationService.Object,
            _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new Contact { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Contact>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 100).Select(i => new Contact
        {
            Id = i,
            FirstName = $"Contact{i}",
            LastName = "Test",
            Email = $"contact{i}@example.com"
        }).ToList();

        _mockContactRepository.Setup(r => r.GetPagedAsync(1, 10, It.IsAny<Expression<Func<Contact, bool>>>(), null))
            .ReturnsAsync(new PagedResult<Contact>
            {
                Items = contacts.Take(10).ToList(),
                TotalCount = 100,
                Page = 1,
                PageSize = 10
            });

        // Act
        var result = await _service.GetPagedAsync(1, 10);

        // Assert
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(100);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_ReturnsFilteredContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@acme.com" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.SearchAsync("John");

        // Assert
        result.Should().HaveCount(1);
        result.First().FirstName.Should().Be("John");
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_ExistingContact_ReturnsContact()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(contact);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingContact_ReturnsNull()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithRelations_ReturnsContactWithRelations()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            AccountContacts = new List<AccountContact>
            {
                new AccountContact { AccountId = 1, Account = new Account { Id = 1, Company = "Acme" } }
            }
        };

        _mockContactRepository.Setup(r => r.GetByIdWithIncludesAsync(1, It.IsAny<string[]>()))
            .ReturnsAsync(contact);

        // Act
        var result = await _service.GetByIdWithRelationsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.AccountContacts.Should().HaveCount(1);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidContact_ReturnsCreatedContact()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234"
        };

        _mockContactRepository.Setup(r => r.AddAsync(It.IsAny<Contact>()))
            .ReturnsAsync((Contact c) => { c.Id = 1; return c; });

        _mockNotificationService.Setup(n => n.NotifyEntityCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsException()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com"
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact> { new Contact { Email = "existing@example.com" } });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_MissingRequiredFields_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "",
            LastName = ""
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_WithAccountLink_CreatesContactAndLink()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            AccountId = 1
        };

        _mockAccountRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Account { Id = 1 });

        _mockContactRepository.Setup(r => r.AddAsync(It.IsAny<Contact>()))
            .ReturnsAsync((Contact c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        _mockContactRepository.Verify(r => r.AddAsync(It.Is<Contact>(c => c.FirstName == "John")), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidContact_ReturnsUpdatedContact()
    {
        // Arrange
        var existingContact = new Contact
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        var updateDto = new UpdateContactDto
        {
            Id = 1,
            FirstName = "Johnny",
            LastName = "Doe",
            Email = "johnny@example.com"
        };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingContact);

        _mockContactRepository.Setup(r => r.UpdateAsync(It.IsAny<Contact>()))
            .ReturnsAsync((Contact c) => c);

        _mockNotificationService.Setup(n => n.NotifyEntityUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Johnny");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingContact_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateContactDto { Id = 999 };

        _mockContactRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_EmailToExistingEmail_ThrowsException()
    {
        // Arrange
        var existingContact = new Contact { Id = 1, Email = "john@example.com" };
        var updateDto = new UpdateContactDto { Id = 1, Email = "existing@example.com" };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingContact);

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact> { new Contact { Id = 2, Email = "existing@example.com" } });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(updateDto));
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingContact_ReturnsTrue()
    {
        // Arrange
        var contact = new Contact { Id = 1, FirstName = "John" };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(contact);

        _mockContactRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        _mockNotificationService.Setup(n => n.NotifyEntityDeletedAsync(It.IsAny<string>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingContact_ReturnsFalse()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Contact?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BulkDeleteAsync_ValidIds_DeletesAllContacts()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockContactRepository.Setup(r => r.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _service.BulkDeleteAsync(ids);

        // Assert
        result.Should().Be(3);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task SearchAsync_ByFirstName_ReturnsMatchingContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", LastName = "Doe" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.SearchAsync("John");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_ByEmail_ReturnsMatchingContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John", Email = "john@acme.com" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.SearchAsync("acme");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact>());

        // Act
        var result = await _service.SearchAsync("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsAllContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John" },
            new Contact { Id = 2, FirstName = "Jane" }
        };

        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.SearchAsync("");

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region Account Relationship Tests

    [Fact]
    public async Task GetByAccountAsync_ReturnsAccountContacts()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, FirstName = "John" },
            new Contact { Id = 2, FirstName = "Jane" }
        };

        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.GetByAccountAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task LinkToAccountAsync_ValidIds_CreatesLink()
    {
        // Arrange
        var contact = new Contact { Id = 1, FirstName = "John" };
        var account = new Account { Id = 1, Company = "Acme" };

        _mockContactRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(contact);
        _mockAccountRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(account);

        // Act
        var result = await _service.LinkToAccountAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UnlinkFromAccountAsync_ValidIds_RemovesLink()
    {
        // Arrange
        var contact = new Contact
        {
            Id = 1,
            AccountContacts = new List<AccountContact>
            {
                new AccountContact { AccountId = 1, ContactId = 1 }
            }
        };

        _mockContactRepository.Setup(r => r.GetByIdWithIncludesAsync(1, It.IsAny<string[]>()))
            .ReturnsAsync(contact);

        // Act
        var result = await _service.UnlinkFromAccountAsync(1, 1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateEmailAsync_ValidEmail_ReturnsTrue()
    {
        // Act
        var result = await _service.ValidateEmailAsync("test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEmailAsync_InvalidEmail_ReturnsFalse()
    {
        // Act
        var result = await _service.ValidateEmailAsync("invalid-email");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckEmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact> { new Contact { Email = "test@example.com" } });

        // Act
        var result = await _service.CheckEmailExistsAsync("test@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CheckEmailExistsAsync_NonExistingEmail_ReturnsFalse()
    {
        // Arrange
        _mockContactRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>()))
            .ReturnsAsync(new List<Contact>());

        // Act
        var result = await _service.CheckEmailExistsAsync("new@example.com");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetStatisticsAsync_ReturnsContactStats()
    {
        // Arrange
        var contacts = new List<Contact>
        {
            new Contact { Id = 1, CreatedAt = DateTime.UtcNow },
            new Contact { Id = 2, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new Contact { Id = 3, CreatedAt = DateTime.UtcNow.AddDays(-60) }
        };

        _mockContactRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
    }

    #endregion
}
