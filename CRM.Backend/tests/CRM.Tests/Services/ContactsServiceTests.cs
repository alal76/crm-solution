// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>Unit tests for ContactsService (TCOV Wave-A).</summary>
public class ContactsServiceTests : IDisposable
{
    private readonly CrmDbContext _context;
    private readonly Mock<IContactInfoService> _mockContactInfo;
    private readonly Mock<IEntityEventDispatcher> _mockDispatcher;
    private readonly Mock<IDuplicateDetectionService> _mockDuplicates;
    private readonly Mock<ILogger<ContactsService>> _mockLogger;
    private readonly ContactsService _service;

    public ContactsServiceTests()
    {
        var opts = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new CrmDbContext(opts, null!);

        _mockContactInfo = new Mock<IContactInfoService>();
        _mockDispatcher = new Mock<IEntityEventDispatcher>();
        _mockDuplicates = new Mock<IDuplicateDetectionService>();
        _mockLogger = new Mock<ILogger<ContactsService>>();

        // Duplicate detection returns no duplicates — allows creation to proceed
        _mockDuplicates
            .Setup(d => d.CheckForDuplicatesAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string?>>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DuplicateCheckResult());

        // Event dispatcher completes without side effects
        _mockDispatcher
            .Setup(e => e.DispatchEntityEventAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<WorkflowTriggerType>(),
                It.IsAny<int?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new ContactsService(
            _context,
            _mockContactInfo.Object,
            _mockDispatcher.Object,
            _mockDuplicates.Object,
            _mockLogger.Object);
    }

    public void Dispose() => _context.Dispose();

    // ------------------------------------------------------------------
    // GetAllAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllContacts_WhenContactsExist()
    {
        _context.Contacts.AddRange(
            new Contact { Id = 1, FirstName = "Alice", LastName = "Smith" },
            new Contact { Id = 2, FirstName = "Bob", LastName = "Jones" });
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoContactsExist()
    {
        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ------------------------------------------------------------------
    // GetByIdAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ShouldReturnContact_WhenContactExists()
    {
        _context.Contacts.Add(new Contact { Id = 10, FirstName = "Carol", LastName = "White" });
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(10);

        result.Should().NotBeNull();
        result.FirstName.Should().Be("Carol");
        result.LastName.Should().Be("White");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenContactNotFound()
    {
        Func<Task> act = () => _service.GetByIdAsync(999);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    // ------------------------------------------------------------------
    // CreateAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ShouldCreateContact_WhenRequestIsValid()
    {
        var request = new CreateContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "john.doe@example.com"
        };

        var result = await _service.CreateAsync(request, "test-user");

        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        _context.Contacts.Should().ContainSingle(c => c.FirstName == "John" && c.LastName == "Doe");
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistPreferredContactMethod_WhenProvided()
    {
        // Regression: PreferredContactMethod was accepted on the request DTO but
        // never written to the entity (REM-BUG-009).
        var request = new CreateContactRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            PreferredContactMethod = "Phone"
        };

        await _service.CreateAsync(request, "test-user");

        var saved = _context.Contacts.Single(c => c.FirstName == "Jane" && c.LastName == "Smith");
        saved.PreferredContactMethod.Should().Be(CRM.Core.Models.PreferredContactMethod.Phone);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdatePreferredContactMethod_WhenProvided()
    {
        var contact = new Contact
        {
            FirstName = "Alex",
            LastName = "Green",
            PreferredContactMethod = CRM.Core.Models.PreferredContactMethod.Email
        };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        await _service.UpdateAsync(contact.Id, new UpdateContactRequest { PreferredContactMethod = "SMS" }, "test-user");

        var updated = await _context.Contacts.FindAsync(contact.Id);
        updated!.PreferredContactMethod.Should().Be(CRM.Core.Models.PreferredContactMethod.SMS);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenFirstNameIsEmpty()
    {
        var request = new CreateContactRequest { FirstName = "", LastName = "Doe" };

        Func<Task> act = () => _service.CreateAsync(request, "user");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenLastNameIsEmpty()
    {
        var request = new CreateContactRequest { FirstName = "John", LastName = "" };

        Func<Task> act = () => _service.CreateAsync(request, "user");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ------------------------------------------------------------------
    // DeleteAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ShouldRemoveContact_WhenContactExists()
    {
        _context.Contacts.Add(new Contact { Id = 20, FirstName = "Mark", LastName = "Brown" });
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(20);

        result.Should().BeTrue();
        _context.Contacts.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenContactNotFound()
    {
        Func<Task> act = () => _service.DeleteAsync(888);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*888*");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAssociatedSocialMediaLinks_WhenContactExists()
    {
        var contact = new Contact { Id = 21, FirstName = "Nina", LastName = "Blue" };
        contact.SocialMediaLinks.Add(new SocialMediaLink
        {
            ContactId = 21,
            Platform = SocialMediaPlatform.LinkedIn,
            Url = "https://linkedin.com/in/nina",
            DateAdded = DateTime.UtcNow
        });
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        await _service.DeleteAsync(21);

        _context.SocialMediaLinks.Should().BeEmpty();
    }

    // ------------------------------------------------------------------
    // UpdateAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges_WhenMultipleFieldsProvided()
    {
        var contact = new Contact
        {
            FirstName = "Original",
            LastName = "Name",
            Company = "OldCo",
            JobTitle = "Engineer",
            EmailPrimary = "old@example.com"
        };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var request = new UpdateContactRequest
        {
            FirstName = "Updated",
            LastName = "Person",
            Company = "NewCo",
            JobTitle = "Manager",
            EmailPrimary = "new@example.com",
            City = "Springfield"
        };

        var result = await _service.UpdateAsync(contact.Id, request, "editor");

        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Person");
        result.Company.Should().Be("NewCo");
        result.JobTitle.Should().Be("Manager");
        result.EmailPrimary.Should().Be("new@example.com");
        result.City.Should().Be("Springfield");
        result.ModifiedBy.Should().Be("editor");

        var saved = await _context.Contacts.FindAsync(contact.Id);
        saved!.FirstName.Should().Be("Updated");
        saved.LastName.Should().Be("Person");
        saved.Company.Should().Be("NewCo");
        saved.JobTitle.Should().Be("Manager");
        saved.EmailPrimary.Should().Be("new@example.com");
        saved.City.Should().Be("Springfield");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenContactNotFound()
    {
        Func<Task> act = () => _service.UpdateAsync(777, new UpdateContactRequest { FirstName = "X" }, "editor");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*777*");
    }

    // ------------------------------------------------------------------
    // AddSocialMediaLinkAsync / RemoveSocialMediaLinkAsync
    // ------------------------------------------------------------------

    [Fact]
    public async Task AddSocialMediaLinkAsync_ShouldAddLink_WhenContactExists()
    {
        var contact = new Contact { Id = 30, FirstName = "Sam", LastName = "Fox" };
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var request = new AddSocialMediaRequest
        {
            Platform = "LinkedIn",
            Url = "https://linkedin.com/in/samfox",
            Handle = "samfox"
        };

        var result = await _service.AddSocialMediaLinkAsync(30, request);

        result.Should().NotBeNull();
        result.Platform.Should().Be("LinkedIn");
        result.Url.Should().Be("https://linkedin.com/in/samfox");
        result.Handle.Should().Be("samfox");
        _context.SocialMediaLinks.Should().ContainSingle(l => l.ContactId == 30 && l.Url == "https://linkedin.com/in/samfox");
    }

    [Fact]
    public async Task AddSocialMediaLinkAsync_ShouldThrow_WhenContactNotFound()
    {
        var request = new AddSocialMediaRequest { Platform = "Twitter", Url = "https://twitter.com/nobody" };

        Func<Task> act = () => _service.AddSocialMediaLinkAsync(999, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_ShouldRemoveLink_WhenLinkExists()
    {
        var contact = new Contact { Id = 31, FirstName = "Gina", LastName = "Ray" };
        var link = new SocialMediaLink
        {
            Id = 100,
            ContactId = 31,
            Platform = SocialMediaPlatform.Twitter,
            Url = "https://twitter.com/gina",
            DateAdded = DateTime.UtcNow
        };
        contact.SocialMediaLinks.Add(link);
        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveSocialMediaLinkAsync(100);

        result.Should().BeTrue();
        _context.SocialMediaLinks.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveSocialMediaLinkAsync_ShouldThrow_WhenLinkNotFound()
    {
        Func<Task> act = () => _service.RemoveSocialMediaLinkAsync(555);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*555*");
    }
}
