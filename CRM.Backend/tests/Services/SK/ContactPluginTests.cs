// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// Spec: SK Plugin unit tests — ContactPlugin
// MANDATORY TEST RULE: All method signatures verified against actual source before writing.
// Source files read:
//   ContactPlugin.cs — KernelFunctions: GetContact, SearchContacts, GetContactAccounts,
//                      UpdateContact, AddContactNote
//   IContactsService.cs — GetByIdAsync(int)->Task<ContactDto>, GetAllAsync()->Task<List<ContactDto>>,
//                          UpdateAsync(int, UpdateContactRequest, string)->Task<ContactDto>
//   ContactDto.cs — ContactDto fields: FirstName, LastName, EmailPrimary, PhonePrimary, Company
//                   UpdateContactRequest fields: FirstName, LastName, Company, JobTitle, etc.
//   CrmPluginBase.cs — SuccessResult({error:false,data:...}), ErrorResult({error:true,...})
//   ICrmDbContext.cs — Contacts: DbSet<CRM.Core.Models.Contact>, Notes: DbSet<Note>

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

// ContactPlugin uses CRM.Core.Models.Contact via _context.Contacts
using ModelContact = CRM.Core.Models.Contact;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for <see cref="ContactPlugin"/>.
/// KernelFunctions tested: GetContact, SearchContacts, GetContactAccounts,
///   UpdateContact, AddContactNote
/// </summary>
public class ContactPluginTests
{
    private readonly Mock<IContactsService> _contactsService = new(MockBehavior.Loose);
    private readonly Mock<ICrmDbContext> _context = new(MockBehavior.Loose);
    private readonly Mock<ILogger<ContactPlugin>> _logger = new();
    private readonly ContactPlugin _sut;

    public ContactPluginTests()
    {
        _sut = new ContactPlugin(_contactsService.Object, _context.Object, _logger.Object);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Property / Constructor tests
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PluginName_ShouldBe_Contact()
    {
        _sut.PluginName.Should().Be("Contact");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContactsServiceIsNull()
    {
        var act = () => new ContactPlugin(null!, _context.Object, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("contactsService");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenContextIsNull()
    {
        var act = () => new ContactPlugin(_contactsService.Object, null!, _logger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerIsNull()
    {
        var act = () => new ContactPlugin(_contactsService.Object, _context.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetContactAsync
    // IContactsService.GetByIdAsync(int) -> Task<ContactDto> (non-nullable)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactAsync_ShouldReturnSuccessJson_WhenContactExists()
    {
        var contactDto = new ContactDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            EmailPrimary = "jane@example.com",
            Company = "Acme Corp"
        };
        _contactsService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(contactDto);

        var result = await _sut.GetContactAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.TryGetProperty("data", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetContactAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contactsService.Setup(s => s.GetByIdAsync(99))
            .ThrowsAsync(new KeyNotFoundException("Contact 99 not found"));

        var result = await _sut.GetContactAsync(99);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SearchContactsAsync
    // Uses _contactsService.GetAllAsync() then in-memory filter
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchContactsAsync_ShouldReturnFilteredResults_ByFirstName()
    {
        var allContacts = new List<ContactDto>
        {
            new() { Id = 1, FirstName = "Alice", LastName = "Smith",  EmailPrimary = "alice@x.com" },
            new() { Id = 2, FirstName = "Bob",   LastName = "Jones",  EmailPrimary = "bob@x.com"  },
            new() { Id = 3, FirstName = "Alice", LastName = "Cooper", EmailPrimary = "cooper@x.com" }
        };
        _contactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(allContacts);

        var result = await _sut.SearchContactsAsync("alice");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(2);
    }

    [Fact]
    public async Task SearchContactsAsync_ShouldReturnEmptyList_WhenNoMatch()
    {
        var allContacts = new List<ContactDto>
        {
            new() { Id = 1, FirstName = "Zack", LastName = "Frost", EmailPrimary = "z@x.com" }
        };
        _contactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(allContacts);

        var result = await _sut.SearchContactsAsync("xxxxxxnotfound");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task SearchContactsAsync_ShouldRespectMaxResults()
    {
        var allContacts = Enumerable.Range(1, 20)
            .Select(i => new ContactDto { Id = i, FirstName = "Test", LastName = $"User{i}", EmailPrimary = $"t{i}@x.com" })
            .ToList();
        _contactsService.Setup(s => s.GetAllAsync()).ReturnsAsync(allContacts);

        var result = await _sut.SearchContactsAsync("test", maxResults: 4);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(4);
    }

    [Fact]
    public async Task SearchContactsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contactsService.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new Exception("Failed to load contacts"));

        var result = await _sut.SearchContactsAsync("any");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GetContactAccountsAsync — uses _context.Contacts (MockDbSet<ModelContact>)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetContactAccountsAsync_ShouldReturnSuccessJson_WithAccountLinks()
    {
        // CRM.Core.Models.Contact has AccountId: int? and Id: int
        var contacts = new List<ModelContact>
        {
            new ModelContact { Id = 3, AccountId = 100, FirstName = "Jane", LastName = "Doe" }
        };
        var mockContacts = MockDbSetFactory.CreateMockDbSet(contacts);
        _context.Setup(c => c.Contacts).Returns(mockContacts.Object);

        var result = await _sut.GetContactAccountsAsync(3);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        var data = doc.RootElement.GetProperty("data");
        data.GetProperty("contactId").GetInt32().Should().Be(3);
        data.GetProperty("count").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task GetContactAccountsAsync_ShouldReturnEmptyList_WhenContactHasNoAccount()
    {
        var contacts = new List<ModelContact>
        {
            new ModelContact { Id = 5, AccountId = null, FirstName = "Unlinked", LastName = "User" }
        };
        var mockContacts = MockDbSetFactory.CreateMockDbSet(contacts);
        _context.Setup(c => c.Contacts).Returns(mockContacts.Object);

        var result = await _sut.GetContactAccountsAsync(5);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("count").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task GetContactAccountsAsync_ShouldReturnErrorJson_WhenContextThrows()
    {
        _context.Setup(c => c.Contacts).Throws(new Exception("Contacts DB error"));

        var result = await _sut.GetContactAccountsAsync(1);

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UpdateContactAsync
    // Uses reflection on UpdateContactRequest to find property by name
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateContactAsync_ShouldReturnSuccessJson_WhenFieldIsValid()
    {
        // UpdateContactRequest has 'FirstName' (string?) — valid reflectable field
        var updatedDto = new ContactDto { Id = 2, FirstName = "NewFirstName", LastName = "Smith" };
        _contactsService
            .Setup(s => s.UpdateAsync(2, It.IsAny<CRM.Core.Dtos.UpdateContactRequest>(), "AI Agent"))
            .ReturnsAsync(updatedDto);

        var result = await _sut.UpdateContactAsync(2, "FirstName", "NewFirstName");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("updated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdateContactAsync_ShouldReturnErrorJson_WhenFieldIsUnknown()
    {
        var result = await _sut.UpdateContactAsync(1, "GhostField", "value");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("Unknown field");
    }

    [Fact]
    public async Task UpdateContactAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contactsService
            .Setup(s => s.UpdateAsync(It.IsAny<int>(), It.IsAny<CRM.Core.Dtos.UpdateContactRequest>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Update failed"));

        var result = await _sut.UpdateContactAsync(1, "FirstName", "Fail");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // AddContactNoteAsync — uses _context.Notes + SaveChangesAsync
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddContactNoteAsync_ShouldReturnSuccessJson_WhenContactExists()
    {
        var contactDto = new ContactDto { Id = 8, FirstName = "Note", LastName = "Person", EmailPrimary = "np@x.com" };
        _contactsService.Setup(s => s.GetByIdAsync(8)).ReturnsAsync(contactDto);

        var mockNotes = MockDbSetFactory.CreateMockDbSet(new List<Note>());
        _context.Setup(c => c.Notes).Returns(mockNotes.Object);
        _context.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AddContactNoteAsync(8, "Follow up next week.");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        result.Should().Contain("Follow up next week");
    }

    [Fact]
    public async Task AddContactNoteAsync_ShouldReturnErrorJson_WhenContactNotFound()
    {
        // IContactsService.GetByIdAsync returns Task<ContactDto> (non-nullable).
        // Plugin checks `contact != null`. When mocked to return null, error path fires.
        _contactsService.Setup(s => s.GetByIdAsync(55)).ReturnsAsync((ContactDto)null!);

        var result = await _sut.AddContactNoteAsync(55, "Note for missing");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        result.Should().Contain("not found");
    }

    [Fact]
    public async Task AddContactNoteAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contactsService.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Note add failed"));

        var result = await _sut.AddContactNoteAsync(1, "Failing note");

        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }
}
