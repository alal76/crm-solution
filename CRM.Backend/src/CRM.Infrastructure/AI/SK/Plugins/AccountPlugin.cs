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
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace CRM.Infrastructure.AI.SK.Plugins;

/// <summary>
/// Semantic Kernel plugin for CRM Account management operations.
/// Provides AI-accessible functions for querying, searching, and updating accounts.
/// </summary>
public class AccountPlugin : CrmPluginBase
{
    private readonly IAccountService _accountService;
    private readonly ICrmDbContext _context;

    /// <inheritdoc />
    public override string PluginName => "Account";

    /// <inheritdoc />
    public override string Description => "Manage CRM accounts — search, view details, check health scores, view related contacts, and update account information.";

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountPlugin"/> class.
    /// </summary>
    /// <param name="accountService">The account service for CRUD operations.</param>
    /// <param name="context">The database context for direct queries.</param>
    /// <param name="logger">The logger instance.</param>
    public AccountPlugin(
        IAccountService accountService,
        ICrmDbContext context,
        ILogger<AccountPlugin> logger) : base(logger)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Read Methods

    /// <summary>
    /// Retrieves a single account by its ID.
    /// </summary>
    [KernelFunction("GetAccount")]
    [Description("Get detailed information about a specific CRM account by its ID.")]
    public async Task<string> GetAccountAsync(
        [Description("The unique identifier of the account.")] int accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(accountId);
            return account != null
                ? SuccessResult(account)
                : ErrorResult("GetAccount", $"Account with ID {accountId} not found.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving account {AccountId}", accountId);
            return ErrorResult("GetAccount", ex.Message);
        }
    }

    /// <summary>
    /// Searches accounts by a query string.
    /// </summary>
    [KernelFunction("SearchAccounts")]
    [Description("Search for CRM accounts by name, email, company, or other fields.")]
    public async Task<string> SearchAccountsAsync(
        [Description("The search query string (e.g., company name, email, domain).")] string query,
        [Description("Maximum number of results to return.")] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var results = await _accountService.SearchAccountsAsync(query);
            var limited = results.Take(maxResults).ToList();
            return SuccessResult(new { count = limited.Count, accounts = limited });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching accounts with query '{Query}'", query);
            return ErrorResult("SearchAccounts", ex.Message);
        }
    }

    /// <summary>
    /// Gets the health score and summary for an account.
    /// </summary>
    [KernelFunction("GetAccountHealth")]
    [Description("Get the health score and health summary for a specific CRM account.")]
    public async Task<string> GetAccountHealthAsync(
        [Description("The unique identifier of the account.")] int accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return ErrorResult("GetAccountHealth", $"Account with ID {accountId} not found.");

            // Extract health-related properties via reflection since DTO shape may vary
            var healthScore = account.GetType().GetProperty("CustomerHealthScore")?.GetValue(account)
                           ?? account.GetType().GetProperty("HealthScore")?.GetValue(account);

            return SuccessResult(new
            {
                accountId,
                company = account.GetType().GetProperty("Company")?.GetValue(account)?.ToString(),
                healthScore = healthScore ?? 0,
                status = account.GetType().GetProperty("Status")?.GetValue(account)?.ToString() ?? "Unknown"
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving health for account {AccountId}", accountId);
            return ErrorResult("GetAccountHealth", ex.Message);
        }
    }

    /// <summary>
    /// Gets all contacts related to an account.
    /// </summary>
    [KernelFunction("GetRelatedContacts")]
    [Description("Get the list of contacts associated with a specific CRM account.")]
    public async Task<string> GetRelatedContactsAsync(
        [Description("The unique identifier of the account.")] int accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contacts = await _accountService.GetAccountContactsAsync(accountId);
            var list = contacts.ToList();
            return SuccessResult(new { accountId, count = list.Count, contacts = list });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving contacts for account {AccountId}", accountId);
            return ErrorResult("GetRelatedContacts", ex.Message);
        }
    }

    #endregion

    #region Write Methods

    /// <summary>
    /// Updates a specific field on an account.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Updates a single field on an account record.")]
    [KernelFunction("UpdateAccount")]
    [Description("Update a specific field on a CRM account (e.g., Company, Industry, Phone, Email).")]
    public async Task<string> UpdateAccountAsync(
        [Description("The unique identifier of the account to update.")] int accountId,
        [Description("The name of the field to update (e.g., Company, Industry, Phone, Email, Website).")] string fieldName,
        [Description("The new value for the field.")] string newValue,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = new UpdateAccountDto();
            var property = typeof(UpdateAccountDto).GetProperty(fieldName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property == null)
                return ErrorResult("UpdateAccount", $"Unknown field: '{fieldName}'. Check the field name and try again.");

            var convertedValue = Convert.ChangeType(newValue, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            property.SetValue(dto, convertedValue);

            var result = await _accountService.UpdateAccountAsync(accountId, dto);
            return result != null
                ? SuccessResult(new { updated = true, accountId, fieldName, newValue })
                : ErrorResult("UpdateAccount", $"Account with ID {accountId} not found or update failed.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating account {AccountId} field {FieldName}", accountId, fieldName);
            return ErrorResult("UpdateAccount", ex.Message);
        }
    }

    /// <summary>
    /// Adds a note to an account.
    /// </summary>
    [RequiresApproval(Tier = "low", Description = "Adds a text note to an account record.")]
    [KernelFunction("AddAccountNote")]
    [Description("Add a text note to a CRM account for record-keeping.")]
    public async Task<string> AddAccountNoteAsync(
        [Description("The unique identifier of the account.")] int accountId,
        [Description("The content of the note to add.")] string noteContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var account = await _accountService.GetAccountByIdAsync(accountId);
            if (account == null)
                return ErrorResult("AddAccountNote", $"Account with ID {accountId} not found.");

            var note = new Note
            {
                Title = "AI-Generated Note",
                Content = noteContent,
                EntityType = "Account",
                EntityId = accountId,
                NoteType = NoteType.General,
                Visibility = NoteVisibility.Team,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync(cancellationToken);

            return SuccessResult(new { noteId = note.Id, accountId, content = noteContent });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error adding note to account {AccountId}", accountId);
            return ErrorResult("AddAccountNote", ex.Message);
        }
    }

    #endregion
}
