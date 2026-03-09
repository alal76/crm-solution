// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// AP-037: Extracted from AccountService.cs (god-class split)
// Contains IAccountContactService interface and AccountContactService implementation.
// AccountService contact-management methods now delegate to this class.

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// AP-037: Interface for account-contact relationship management.
/// Extracted from IAccountService to enable focused, testable contact operations.
/// </summary>
public interface IAccountContactService
{
    /// <summary>Link an existing contact to an account with relationship metadata.</summary>
    Task<AccountContactDto?> LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto);

    /// <summary>Remove a contact link from an account (soft-delete).</summary>
    Task<bool> UnlinkContactFromAccountAsync(int accountId, int contactId);

    /// <summary>Update relationship attributes for an existing account↔contact link.</summary>
    Task<AccountContactDto?> UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto);

    /// <summary>Get all contacts linked to an account.</summary>
    Task<IEnumerable<AccountContactDto>> GetAccountContactsAsync(int accountId);

    /// <summary>Designate one contact as the primary contact for an account.</summary>
    Task<bool> SetPrimaryContactAsync(int accountId, int contactId);

    /// <summary>Get contacts assigned directly via Contact.AccountId (one-to-many relationship).</summary>
    Task<IEnumerable<object>> GetDirectContactsAsync(int accountId);

    /// <summary>Assign a contact to an account by setting Contact.AccountId.</summary>
    Task<bool> AssignContactToAccountAsync(int accountId, int contactId);

    /// <summary>Remove a contact's assignment from an account by clearing Contact.AccountId.</summary>
    Task<bool> UnassignContactFromAccountAsync(int accountId, int contactId);

    /// <summary>Map an AccountContact entity to its DTO representation.</summary>
    AccountContactDto MapToDto(AccountContact contact, ContactDto? contactDto);
}

/// <summary>
/// AP-037: Manages the relationship between Accounts and Contacts.
/// Handles both the junction-table pattern (many-to-many via AccountContacts table)
/// and the direct assignment pattern (one-to-many via Contact.AccountId).
/// Extracted from AccountService.cs to reduce god-class complexity.
/// </summary>
public class AccountContactService : IAccountContactService
{
    private readonly IRepository<AccountContact> _accountContactRepository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IContactsService _contactsService;
    private readonly ILogger<AccountContactService> _logger;

    public AccountContactService(
        IRepository<AccountContact> accountContactRepository,
        IRepository<Account> accountRepository,
        IContactsService contactsService,
        ILogger<AccountContactService> logger)
    {
        _accountContactRepository = accountContactRepository;
        _accountRepository = accountRepository;
        _contactsService = contactsService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AccountContactDto?> LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
        {
            return null;
        }

        var contact = await _contactsService.GetByIdAsync(dto.ContactId);
        if (contact == null)
        {
            return null;
        }

        // Check if already linked
        var existingLinks = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == dto.ContactId && !cc.IsDeleted);
        if (existingLinks.Any())
        {
            return null;
        }

        // If this is primary contact, unset others
        if (dto.IsPrimaryContact)
        {
            var otherPrimary = await _accountContactRepository.FindAsync(cc =>
                cc.AccountId == accountId && cc.IsPrimaryContact && !cc.IsDeleted);
            foreach (var other in otherPrimary)
            {
                other.IsPrimaryContact = false;
                await _accountContactRepository.UpdateAsync(other);
            }
        }

        var accountContact = new AccountContact
        {
            AccountId = accountId,
            ContactId = dto.ContactId,
            Role = dto.Role,
            IsPrimaryContact = dto.IsPrimaryContact,
            IsDecisionMaker = dto.IsDecisionMaker,
            ReceivesBillingNotifications = dto.ReceivesBillingNotifications,
            ReceivesMarketingEmails = dto.ReceivesMarketingEmails,
            ReceivesTechnicalUpdates = dto.ReceivesTechnicalUpdates,
            PositionAtAccount = dto.PositionAtAccount,
            DepartmentAtAccount = dto.DepartmentAtAccount,
            Notes = dto.Notes,
            RelationshipStartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _accountContactRepository.AddAsync(accountContact);
        await _accountContactRepository.SaveAsync();

        if (dto.IsPrimaryContact)
        {
            account.PrimaryContactId = dto.ContactId;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveAsync();
        }

        return MapToDto(accountContact, contact);
    }

    /// <inheritdoc/>
    public async Task<bool> UnlinkContactFromAccountAsync(int accountId, int contactId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
        {
            return false;
        }

        link.IsDeleted = true;
        link.RelationshipEndDate = DateTime.UtcNow;

        await _accountContactRepository.UpdateAsync(link);
        await _accountContactRepository.SaveAsync();

        // If this was primary contact, clear it on the account
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account != null && account.PrimaryContactId == contactId)
        {
            account.PrimaryContactId = null;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveAsync();
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<AccountContactDto?> UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
        {
            return null;
        }

        if (dto.Role.HasValue) link.Role = dto.Role.Value;
        if (dto.IsDecisionMaker.HasValue) link.IsDecisionMaker = dto.IsDecisionMaker.Value;
        if (dto.ReceivesBillingNotifications.HasValue) link.ReceivesBillingNotifications = dto.ReceivesBillingNotifications.Value;
        if (dto.ReceivesMarketingEmails.HasValue) link.ReceivesMarketingEmails = dto.ReceivesMarketingEmails.Value;
        if (dto.ReceivesTechnicalUpdates.HasValue) link.ReceivesTechnicalUpdates = dto.ReceivesTechnicalUpdates.Value;
        if (dto.PositionAtAccount != null) link.PositionAtAccount = dto.PositionAtAccount;
        if (dto.DepartmentAtAccount != null) link.DepartmentAtAccount = dto.DepartmentAtAccount;
        if (dto.RelationshipEndDate.HasValue) link.RelationshipEndDate = dto.RelationshipEndDate;
        if (dto.Notes != null) link.Notes = dto.Notes;

        // Handle primary contact change
        if (dto.IsPrimaryContact.HasValue)
        {
            if (dto.IsPrimaryContact.Value && !link.IsPrimaryContact)
            {
                // Unset other primary contacts
                var otherPrimary = await _accountContactRepository.FindAsync(cc =>
                    cc.AccountId == accountId && cc.IsPrimaryContact && cc.Id != link.Id && !cc.IsDeleted);
                foreach (var other in otherPrimary)
                {
                    other.IsPrimaryContact = false;
                    await _accountContactRepository.UpdateAsync(other);
                }

                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account != null)
                {
                    account.PrimaryContactId = contactId;
                    await _accountRepository.UpdateAsync(account);
                }
            }
            link.IsPrimaryContact = dto.IsPrimaryContact.Value;
        }

        await _accountContactRepository.UpdateAsync(link);
        await _accountContactRepository.SaveAsync();

        var contactDto = await _contactsService.GetByIdAsync(contactId);
        return MapToDto(link, contactDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AccountContactDto>> GetAccountContactsAsync(int accountId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && !cc.IsDeleted);

        var dtos = new List<AccountContactDto>();
        foreach (var link in links)
        {
            var contact = await _contactsService.GetByIdAsync(link.ContactId);
            dtos.Add(MapToDto(link, contact));
        }
        return dtos;
    }

    /// <inheritdoc/>
    public async Task<bool> SetPrimaryContactAsync(int accountId, int contactId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
        {
            return false;
        }

        var allLinks = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && !cc.IsDeleted);
        foreach (var l in allLinks)
        {
            l.IsPrimaryContact = (l.Id == link.Id);
            await _accountContactRepository.UpdateAsync(l);
        }

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account != null)
        {
            account.PrimaryContactId = contactId;
            await _accountRepository.UpdateAsync(account);
        }

        await _accountContactRepository.SaveAsync();
        await _accountRepository.SaveAsync();
        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<object>> GetDirectContactsAsync(int accountId)
    {
        var contacts = await _contactsService.GetByAccountIdAsync(accountId);
        return contacts.Select(c => new
        {
            c.Id,
            c.FirstName,
            c.LastName,
            FullName = $"{c.FirstName} {c.LastName}".Trim(),
            c.EmailPrimary,
            c.PhonePrimary,
            c.JobTitle,
            c.Company,
            c.ContactType,
            c.Status,
            c.DateAdded
        }).ToList<object>();
    }

    /// <inheritdoc/>
    public async Task<bool> AssignContactToAccountAsync(int accountId, int contactId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
        {
            return false;
        }

        var contact = await _contactsService.GetByIdAsync(contactId);
        if (contact == null)
        {
            return false;
        }

        await _contactsService.AssignToAccountAsync(contactId, accountId);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> UnassignContactFromAccountAsync(int accountId, int contactId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
        {
            return false;
        }

        var contact = await _contactsService.GetByIdAsync(contactId);
        if (contact == null || contact.AccountId != accountId)
        {
            return false;
        }

        await _contactsService.UnassignFromAccountAsync(contactId);
        return true;
    }

    /// <inheritdoc/>
    public AccountContactDto MapToDto(AccountContact cc, ContactDto? contact)
    {
        return new AccountContactDto
        {
            Id = cc.Id,
            AccountId = cc.AccountId,
            ContactId = cc.ContactId,
            ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}" : "Unknown",
            ContactEmail = contact?.EmailPrimary,
            ContactPhone = contact?.PhonePrimary,
            Role = cc.Role.ToString(),
            IsPrimaryContact = cc.IsPrimaryContact,
            IsDecisionMaker = cc.IsDecisionMaker,
            ReceivesBillingNotifications = cc.ReceivesBillingNotifications,
            ReceivesMarketingEmails = cc.ReceivesMarketingEmails,
            ReceivesTechnicalUpdates = cc.ReceivesTechnicalUpdates,
            PositionAtAccount = cc.PositionAtAccount,
            DepartmentAtAccount = cc.DepartmentAtAccount,
            Notes = cc.Notes,
            RelationshipStartDate = cc.RelationshipStartDate,
            RelationshipEndDate = cc.RelationshipEndDate,
            CreatedAt = cc.CreatedAt
        };
    }
}
