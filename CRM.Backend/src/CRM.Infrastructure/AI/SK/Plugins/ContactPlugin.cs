// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#nullable enable

using System.ComponentModel;
using System.Reflection;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM Contact management operations.
/// Provides AI-accessible functions for querying, searching, and updating contacts.
/// </summary>
public class ContactPlugin : CrmPluginBase
{
    private readonly IContactsService _contactsService;
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "Contact";

    /// <inheritdoc />
    public override string Description => "Manage CRM contacts — search, view details, view associated accounts, and update contact information.";

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactPlugin"/> class.
    /// </summary>
    /// <param name="contactsService">The contacts service for CRUD operations.</param>
    /// <param name="context">The database context for direct queries.</param>
    /// <param name="logger">The logger instance.</param>
    public ContactPlugin(
        IContactsService contactsService,
        ICrmDbContext context,
        ILogger<ContactPlugin> logger) : base(logger)
    {
        _contactsService = contactsService ?? throw new ArgumentNullException(nameof(contactsService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Methods

    /// <summary>
    /// Retrieves a single contact by its ID.
    /// </summary>
    [KernelFunction("GetContact")]
    [Description("Get detailed information about a specific CRM contact by their ID.")]
    public async Task<string> GetContactAsync(
        [Description("The unique identifier of the contact.")] int contactId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _contactsService.GetByIdAsync(contactId);
            return contact != null
                ? SuccessResult(contact)
                : ErrorResult("GetContact", $"Contact with ID {contactId} not found.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving contact {ContactId}", contactId);
            return ErrorResult("GetContact", ex.Message);
        }
    }

    /// <summary>
    /// Searches contacts by a query string.
    /// </summary>
    [KernelFunction("SearchContacts")]
    [Description("Search for CRM contacts by name, email, phone, or company. Filters in-memory from all contacts.")]
    public async Task<string> SearchContactsAsync(
        [Description("The search query string (e.g., name, email, phone).")] string query,
        [Description("Maximum number of results to return.")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var all = await _contactsService.GetAllAsync();
            var lowerQuery = query.ToLowerInvariant();

            var filtered = all
                .Where(c =>
                    (c.FirstName?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                    (c.LastName?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                    (c.EmailPrimary?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                    (c.PhonePrimary?.ToLowerInvariant().Contains(lowerQuery) == true) ||
                    (c.Company?.ToLowerInvariant().Contains(lowerQuery) == true))
                .Take(maxResults)
                .ToList();

            return SuccessResult(new { count = filtered.Count, contacts = filtered });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching contacts with query '{Query}'", query);
            return ErrorResult("SearchContacts", ex.Message);
        }
    }

    /// <summary>
    /// Gets the accounts associated with a contact.
    /// </summary>
    [KernelFunction("GetContactAccounts")]
    [Description("Get all accounts (organizations) associated with a specific contact.")]
    public async Task<string> GetContactAccountsAsync(
        [Description("The unique identifier of the contact.")] int contactId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _context.Contacts
                .Where(c => c.Id == contactId && c.AccountId != null)
                .Select(c => new { c.AccountId, IsPrimary = (bool)true })
                .ToListAsync(cancellationToken);

            return SuccessResult(new { contactId, count = accounts.Count, accounts });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving accounts for contact {ContactId}", contactId);
            return ErrorResult("GetContactAccounts", ex.Message);
        }
    }

    #endregion

    #region Write Methods

    /// <summary>
    /// Updates a specific field on a contact.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Updates a single field on a contact record.")]
    [KernelFunction("UpdateContact")]
    [Description("Update a specific field on a CRM contact (e.g., FirstName, LastName, Email, Phone, JobTitle).")]
    public async Task<string> UpdateContactAsync(
        [Description("The unique identifier of the contact to update.")] int contactId,
        [Description("The name of the field to update (e.g., FirstName, LastName, Email, Phone).")] string fieldName,
        [Description("The new value for the field.")] string newValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var req = new UpdateContactRequest();
            var property = typeof(UpdateContactRequest).GetProperty(fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null)
                return ErrorResult("UpdateContact", $"Unknown field: '{fieldName}'. Check the field name and try again.");

            var convertedValue = Convert.ChangeType(newValue, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            property.SetValue(req, convertedValue);

            var result = await _contactsService.UpdateAsync(contactId, req, "AI Agent");
            return result != null
                ? SuccessResult(new { updated = true, contactId, fieldName, newValue })
                : ErrorResult("UpdateContact", $"Contact with ID {contactId} not found or update failed.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating contact {ContactId} field {FieldName}", contactId, fieldName);
            return ErrorResult("UpdateContact", ex.Message);
        }
    }

    /// <summary>
    /// Adds a note to a contact.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Adds a text note to a contact record.")]
    [KernelFunction("AddContactNote")]
    [Description("Add a text note to a CRM contact for record-keeping.")]
    public async Task<string> AddContactNoteAsync(
        [Description("The unique identifier of the contact.")] int contactId,
        [Description("The content of the note to add.")] string noteContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contact = await _contactsService.GetByIdAsync(contactId);
            if (contact == null)
                return ErrorResult("AddContactNote", $"Contact with ID {contactId} not found.");

            var note = new Note
            {
                Title = "AI-Generated Note",
                Content = noteContent,
                EntityType = "Contact",
                EntityId = contactId,
                NoteType = NoteType.General,
                Visibility = NoteVisibility.Team,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return SuccessResult(new { noteId = note.Id, contactId, content = noteContent });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding note to contact {ContactId}", contactId);
            return ErrorResult("AddContactNote", ex.Message);
        }
    }

    #endregion
}
