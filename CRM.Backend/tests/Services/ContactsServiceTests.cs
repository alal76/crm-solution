// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Dtos;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ContactsService
/// Tests cover contact CRUD operations using InMemory database
/// </summary>
public class ContactsServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly ContactsService _service;

    public ContactsServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Contacts_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null);
        _service = new ContactsService(_dbContext);
    }

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenContactExists_ReturnsContact()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "john.doe@example.com",
            ContactType = ContactType.Customer,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(contact.Id);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.EmailPrimary.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenContactDoesNotExist_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.GetByIdAsync(999))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllContacts()
    {
        // Arrange
        _dbContext.Contacts.AddRange(
            new Contact { FirstName = "John", LastName = "Doe", ContactType = ContactType.Customer, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Jane", LastName = "Smith", ContactType = ContactType.Lead, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Bob", LastName = "Johnson", ContactType = ContactType.Partner, DateAdded = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyListWhenNoContacts()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_OrdersByLastNameThenFirstName()
    {
        // Arrange
        _dbContext.Contacts.AddRange(
            new Contact { FirstName = "Zoe", LastName = "Adams", ContactType = ContactType.Customer, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Alice", LastName = "Adams", ContactType = ContactType.Lead, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Bob", LastName = "Brown", ContactType = ContactType.Partner, DateAdded = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result[0].FirstName.Should().Be("Alice");
        result[1].FirstName.Should().Be("Zoe");
        result[2].FirstName.Should().Be("Bob");
    }

    #endregion

    #region GetByTypeAsync Tests

    [Fact]
    public async Task GetByTypeAsync_ReturnsContactsOfSpecificType()
    {
        // Arrange
        _dbContext.Contacts.AddRange(
            new Contact { FirstName = "John", LastName = "Doe", ContactType = ContactType.Customer, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Jane", LastName = "Smith", ContactType = ContactType.Lead, DateAdded = DateTime.UtcNow },
            new Contact { FirstName = "Bob", LastName = "Johnson", ContactType = ContactType.Customer, DateAdded = DateTime.UtcNow }
        );
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetByTypeAsync("Customer");

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.ContactType == "Customer").Should().BeTrue();
    }

    [Fact]
    public async Task GetByTypeAsync_WithInvalidType_ReturnsEmptyList()
    {
        // Arrange
        _dbContext.Contacts.Add(new Contact 
        { 
            FirstName = "John", 
            LastName = "Doe", 
            ContactType = ContactType.Customer, 
            DateAdded = DateTime.UtcNow 
        });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetByTypeAsync("InvalidType");

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_CreatesContact()
    {
        // Arrange
        var request = new CreateContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "john.doe@example.com",
            PhonePrimary = "555-0001",
            ContactType = "Customer"
        };

        // Act
        var result = await _service.CreateAsync(request, "TestUser");

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.EmailPrimary.Should().Be("john.doe@example.com");

        var savedContact = await _dbContext.Contacts.FirstOrDefaultAsync(c => c.Id == result.Id);
        savedContact.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WithMissingFirstName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateContactRequest
        {
            FirstName = "",
            LastName = "Doe"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAsync(request, "TestUser"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*First name and last name are required*");
    }

    [Fact]
    public async Task CreateAsync_WithMissingLastName_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateContactRequest
        {
            FirstName = "John",
            LastName = ""
        };

        // Act & Assert
        await FluentActions.Invoking(() => _service.CreateAsync(request, "TestUser"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*First name and last name are required*");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequest_UpdatesContact()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "old@example.com",
            ContactType = ContactType.Customer,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane",
            EmailPrimary = "new@example.com"
        };

        // Act
        var result = await _service.UpdateAsync(contact.Id, updateRequest, "TestUser");

        // Assert
        result.FirstName.Should().Be("Jane");
        result.EmailPrimary.Should().Be("new@example.com");
    }

    [Fact]
    public async Task UpdateAsync_WhenContactNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var updateRequest = new UpdateContactRequest
        {
            FirstName = "Jane"
        };

        // Act & Assert
        await FluentActions.Invoking(() => _service.UpdateAsync(999, updateRequest, "TestUser"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WhenContactExists_ReturnsTrue()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            ContactType = ContactType.Customer,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(contact.Id);

        // Assert
        result.Should().BeTrue();
        var deletedContact = await _dbContext.Contacts.FindAsync(contact.Id);
        deletedContact.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenContactNotFound_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.DeleteAsync(999))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Social Media Link Tests

    [Fact]
    public async Task AddSocialMediaLinkAsync_AddsLinkToContact()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            ContactType = ContactType.Customer,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var request = new AddSocialMediaRequest
        {
            Platform = "LinkedIn",
            Url = "https://linkedin.com/in/johndoe",
            Handle = "johndoe"
        };

        // Act
        var result = await _service.AddSocialMediaLinkAsync(contact.Id, request);

        // Assert
        result.Should().NotBeNull();
        result.Platform.Should().Be("LinkedIn");
        result.Url.Should().Be("https://linkedin.com/in/johndoe");
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_WhenLinkExists_ReturnsTrue()
    {
        // Arrange
        var contact = new Contact
        {
            FirstName = "John",
            LastName = "Doe",
            ContactType = ContactType.Customer,
            DateAdded = DateTime.UtcNow
        };
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();

        var link = new SocialMediaLink
        {
            ContactId = contact.Id,
            Platform = SocialMediaPlatform.LinkedIn,
            Url = "https://linkedin.com/in/johndoe"
        };
        _dbContext.SocialMediaLinks.Add(link);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.RemoveSocialMediaLinkAsync(link.Id);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
