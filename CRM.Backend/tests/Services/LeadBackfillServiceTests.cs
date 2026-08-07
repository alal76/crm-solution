// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
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

/// <summary>
/// Unit tests for <see cref="LeadBackfillService"/> (REM-ORPHAN-003 groundwork) using an
/// EF Core InMemory database. This tool is never run against real data in these tests —
/// every DbContext instance is a fresh, isolated InMemory database per test.
/// </summary>
public class LeadBackfillServiceTests : IDisposable
{
    private readonly CrmDbContext _dbContext;
    private readonly Mock<ILogger<LeadBackfillService>> _mockLogger;
    private readonly ILeadBackfillService _service;

    public LeadBackfillServiceTests()
    {
        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"LeadBackfillServiceTests_{Guid.NewGuid()}")
            .Options;

        _dbContext = new CrmDbContext(options, null!);
        _mockLogger = new Mock<ILogger<LeadBackfillService>>();
        _service = new LeadBackfillService(_dbContext, _mockLogger.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region Helper Methods

    private Contact CreateLeadContact(
        string firstName = "Jane",
        string lastName = "Doe",
        string email = "jane.doe@example.com",
        string? notes = null,
        ContactType contactType = ContactType.Lead)
    {
        return new Contact
        {
            ContactType = contactType,
            FirstName = firstName,
            LastName = lastName,
            EmailPrimary = email,
            PhonePrimary = "555-1234",
            Company = "Acme Corp",
            JobTitle = "Buyer",
            Notes = notes,
            DateAdded = DateTime.UtcNow,
        };
    }

    private async Task<Contact> SeedContactAsync(Contact contact)
    {
        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync();
        return contact;
    }

    #endregion

    #region Valid JSON notes -> correctly mapped Lead

    [Fact]
    public async Task RunAsync_ShouldCreateCorrectlyMappedLead_WhenNotesContainValidJson()
    {
        // Arrange
        var notes = "{\"source\":\"referral\",\"status\":\"qualified\",\"notes\":\"Met at conference\"}";
        var contact = await SeedContactAsync(CreateLeadContact(notes: notes));

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.CreatedCount.Should().Be(1);
        result.ParseErrorCount.Should().Be(0);
        result.SkippedAlreadyMigratedCount.Should().Be(0);

        var lead = await _dbContext.Leads.SingleAsync();
        lead.ContactId.Should().Be(contact.Id);
        lead.FirstName.Should().Be("Jane");
        lead.LastName.Should().Be("Doe");
        lead.Email.Should().Be("jane.doe@example.com");
        lead.Phone.Should().Be("555-1234");
        lead.CompanyName.Should().Be("Acme Corp");
        lead.Title.Should().Be("Buyer");
        lead.Source.Should().Be(LeadSource.Referral);
        lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
        lead.QualificationNotes.Should().Be("Met at conference");
    }

    #endregion

    #region Malformed / non-JSON notes -> fallback, no crash

    [Fact]
    public async Task RunAsync_ShouldCreateLeadWithFallbackDefaults_WhenNotesAreMalformedJson()
    {
        // Arrange: looks like JSON but is not valid JSON.
        var contact = await SeedContactAsync(CreateLeadContact(notes: "{not valid json"));

        // Act
        var act = async () => await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();

        var result = await _service.RunAsync(dryRun: true, CancellationToken.None);
        // Second call after the first already created the lead - should now report already migrated.
        result.SkippedAlreadyMigratedCount.Should().Be(1);

        var lead = await _dbContext.Leads.SingleAsync();
        lead.ContactId.Should().Be(contact.Id);
        lead.Source.Should().Be(LeadSource.Web); // default fallback
        lead.Status.Should().Be(LeadLifecycleStatus.New); // default fallback
    }

    [Fact]
    public async Task RunAsync_ShouldCreateLeadAndReportParseError_WhenNotesArePlainText()
    {
        // Arrange
        var contact = await SeedContactAsync(CreateLeadContact(notes: "Just a plain text note, not JSON at all."));

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.CreatedCount.Should().Be(1);
        result.ParseErrorCount.Should().Be(1);
        result.ParseErrorContactIds.Should().Contain(contact.Id);

        var lead = await _dbContext.Leads.SingleAsync();
        lead.FirstName.Should().Be("Jane");
        lead.LastName.Should().Be("Doe");
        lead.Source.Should().Be(LeadSource.Web);
        lead.Status.Should().Be(LeadLifecycleStatus.New);
    }

    [Fact]
    public async Task RunAsync_ShouldCreateLead_WhenNotesAreNull()
    {
        // Arrange
        await SeedContactAsync(CreateLeadContact(notes: null));

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.CreatedCount.Should().Be(1);
        result.ParseErrorCount.Should().Be(0); // null/empty notes is not a "parse error", just absent data
        (await _dbContext.Leads.CountAsync()).Should().Be(1);
    }

    #endregion

    #region Idempotency

    [Fact]
    public async Task RunAsync_ShouldNotCreateDuplicateLeads_WhenRunTwice()
    {
        // Arrange
        var notes = "{\"source\":\"website\",\"status\":\"new\",\"notes\":\"\"}";
        await SeedContactAsync(CreateLeadContact(notes: notes));

        // Act
        var firstRun = await _service.RunAsync(dryRun: false, CancellationToken.None);
        var secondRun = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        firstRun.CreatedCount.Should().Be(1);
        secondRun.CreatedCount.Should().Be(0);
        secondRun.SkippedAlreadyMigratedCount.Should().Be(1);

        (await _dbContext.Leads.CountAsync()).Should().Be(1);
    }

    #endregion

    #region Dry run

    [Fact]
    public async Task RunAsync_ShouldMakeNoDatabaseWrites_WhenDryRunIsTrue()
    {
        // Arrange
        var notes = "{\"source\":\"event\",\"status\":\"contacted\",\"notes\":\"Trade show lead\"}";
        await SeedContactAsync(CreateLeadContact(notes: notes));
        await SeedContactAsync(CreateLeadContact(firstName: "John", lastName: "Smith", email: "john.smith@example.com"));

        // Act
        var result = await _service.RunAsync(dryRun: true, CancellationToken.None);

        // Assert
        result.DryRun.Should().BeTrue();
        result.TotalLeadContactsFound.Should().Be(2);
        result.CreatedCount.Should().Be(2);
        result.CreatedLeadIds.Should().BeEmpty();

        (await _dbContext.Leads.CountAsync()).Should().Be(0);
    }

    #endregion

    #region Non-Lead contacts ignored

    [Fact]
    public async Task RunAsync_ShouldIgnoreContact_WhenContactTypeIsNotLead()
    {
        // Arrange
        await SeedContactAsync(CreateLeadContact(contactType: ContactType.Customer));
        await SeedContactAsync(CreateLeadContact(contactType: ContactType.Employee));

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.TotalLeadContactsFound.Should().Be(0);
        result.CreatedCount.Should().Be(0);
        (await _dbContext.Leads.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_ShouldOnlyProcessLeadTypeContacts_WhenMixedContactTypesExist()
    {
        // Arrange
        await SeedContactAsync(CreateLeadContact(contactType: ContactType.Lead));
        await SeedContactAsync(CreateLeadContact(firstName: "Not", lastName: "ALead", contactType: ContactType.Customer));

        // Act
        var result = await _service.RunAsync(dryRun: false, CancellationToken.None);

        // Assert
        result.TotalLeadContactsFound.Should().Be(1);
        result.CreatedCount.Should().Be(1);
        (await _dbContext.Leads.CountAsync()).Should().Be(1);
    }

    #endregion

    #region Constructor guards

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContextIsNull()
    {
        var act = () => new LeadBackfillService(null!, _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new LeadBackfillService(_dbContext, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    #endregion
}
