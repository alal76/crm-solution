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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Account service implementation providing CRUD operations for Account entities.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IAccountInputPort (primary/driving port)
/// - Implements IAccountService (backward compatibility)
/// - Uses IRepository pattern for data access (secondary/driven ports)
///
/// FUNCTIONAL VIEW:
/// This service handles all account-related business operations including:
/// - Creating Individual and Organization accounts
/// - Managing account lifecycle (Lead → Prospect → Opportunity → Account)
/// - Linking contacts to organization accounts
/// - Searching and filtering accounts by various criteria
/// - Soft-deleting accounts (preserves data for audit/recovery)
///
/// TECHNICAL VIEW:
/// - Uses IRepository pattern for data access abstraction
/// - Maps between Account entities and AccountDto for API responses
/// - Supports async/await pattern for non-blocking database operations
/// - Integrates with IContactsService for contact management
///
/// PATTERN:
/// [Controller] → [IAccountInputPort] → [AccountService] → [IRepository] → [Database]
/// </summary>
public class AccountService : IAccountService, IAccountInputPort, ICustomerInputPort
{
    private readonly IRepository<Account> _accountRepository;
    private readonly IRepository<AccountContact> _accountContactRepository;
    private readonly IContactsService _contactsService;
    private readonly IContactInfoService _contactInfoService;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<ContactDetail> _contactDetailRepository;
    private readonly IRepository<SocialAccount> _socialAccountRepository;
    private readonly IRepository<ContactInfoLink> _contactInfoLinkRepository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;

    /// <summary>
    /// Initializes a new instance of AccountService with required dependencies.
    /// </summary>
    /// <param name="accountRepository">Repository for Account entity CRUD operations</param>
    /// <param name="accountContactRepository">Repository for AccountContact junction records</param>
    /// <param name="contactsService">Service for managing Contact records</param>
    public AccountService(
        IRepository<Account> accountRepository,
        IRepository<AccountContact> accountContactRepository,
        IContactsService contactsService,
        IContactInfoService contactInfoService,
        IRepository<Address> addressRepository,
        IRepository<ContactDetail> contactDetailRepository,
        IRepository<SocialAccount> socialAccountRepository,
        IRepository<ContactInfoLink> contactInfoLinkRepository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService)
    {
        _accountRepository = accountRepository;
        _accountContactRepository = accountContactRepository;
        _contactsService = contactsService;
        _contactInfoService = contactInfoService;
        _addressRepository = addressRepository;
        _contactDetailRepository = contactDetailRepository;
        _socialAccountRepository = socialAccountRepository;
        _contactInfoLinkRepository = contactInfoLinkRepository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
    }

    /// <summary>
    /// Retrieves a single account by their unique identifier.
    ///
    /// FUNCTIONAL: Returns account details including contact links for organizations.
    /// TECHNICAL: Filters out soft-deleted records, maps to DTO.
    /// </summary>
    /// <param name="id">The unique account ID</param>
    /// <returns>AccountDto if found and not deleted, null otherwise</returns>
    public async Task<AccountDto?> GetAccountByIdAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null || account.IsDeleted)
            return null;

        return await MapToDto(account);
    }

    /// <summary>
    /// Retrieves all active (non-deleted) accounts.
    ///
    /// FUNCTIONAL: Returns complete account list for dashboards and reports.
    /// TECHNICAL: Filters IsDeleted flag, maps each entity to DTO.
    /// </summary>
    /// <returns>Collection of AccountDto objects</returns>
    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        var accounts = await _accountRepository.GetAllAsync();
        var activeAccounts = accounts.Where(c => !c.IsDeleted).ToList();

        var dtos = new List<AccountDto>();
        foreach (var account in activeAccounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    /// <summary>
    /// Searches accounts by name, email, or company name.
    ///
    /// FUNCTIONAL: Allows users to find accounts quickly using partial matches.
    /// TECHNICAL: Uses case-insensitive Contains() for flexible matching.
    /// </summary>
    /// <param name="searchTerm">Text to search for in account records</param>
    /// <returns>Collection of matching AccountDto objects</returns>
    public async Task<IEnumerable<AccountDto>> SearchAccountsAsync(string searchTerm)
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && (
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Contains(searchTerm) ||
                c.Company.Contains(searchTerm)
            )
        );

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    /// <summary>
    /// Creates a new account record.
    ///
    /// FUNCTIONAL: Supports both Individual (name-based) and Organization (company-based) accounts.
    /// TECHNICAL: Maps DTO to entity, persists to database, returns created record.
    /// </summary>
    /// <param name="dto">Account creation data</param>
    /// <returns>Created AccountDto with assigned ID</returns>
    public async Task<AccountDto> CreateAccountAsync(CreateAccountDto dto)
    {
        var account = new Account
        {
            Category = dto.Category,
            FirstName = dto.FirstName ?? string.Empty,
            LastName = dto.LastName ?? string.Empty,
            Salutation = dto.Salutation,
            Suffix = dto.Suffix,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            LinkedContactId = dto.LinkedContactId,
            Company = dto.Company ?? string.Empty,
            LegalName = dto.LegalName,
            DbaName = dto.DbaName,
            TaxId = dto.TaxId,
            RegistrationNumber = dto.RegistrationNumber,
            YearFounded = dto.YearFounded,
            Email = dto.Email,
            SecondaryEmail = dto.SecondaryEmail,
            Phone = dto.Phone,
            MobilePhone = dto.MobilePhone,
            FaxNumber = dto.FaxNumber,
            JobTitle = dto.JobTitle,
            Website = dto.Website,
            Address = dto.Address ?? string.Empty,
            Address2 = dto.Address2,
            City = dto.City ?? string.Empty,
            State = dto.State ?? string.Empty,
            ZipCode = dto.ZipCode ?? string.Empty,
            Country = dto.Country ?? string.Empty,
            ShippingAddress = dto.ShippingAddress,
            ShippingAddress2 = dto.ShippingAddress2,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingZipCode = dto.ShippingZipCode,
            ShippingCountry = dto.ShippingCountry,
            ShippingSameAsBilling = dto.ShippingSameAsBilling,
            Industry = dto.Industry,
            SubIndustry = dto.SubIndustry,
            NumberOfEmployees = dto.NumberOfEmployees,
            EmployeeRange = dto.EmployeeRange,
            AnnualRevenue = dto.AnnualRevenue ?? 0,
            RevenueRange = dto.RevenueRange,
            AccountType = dto.AccountType,
            Priority = dto.Priority,
            StockSymbol = dto.StockSymbol,
            Ownership = dto.Ownership,
            LifecycleStage = dto.LifecycleStage,
            LeadSource = dto.LeadSource,
            FirstContactDate = DateTime.UtcNow,
            AssignedToUserId = dto.AssignedToUserId,
            AccountManagerId = dto.AccountManagerId,
            Territory = dto.Territory,
            Region = dto.Region,
            Tags = dto.Tags,
            Segment = dto.Segment,
            ReferralSource = dto.ReferralSource,
            ReferredByAccountId = dto.ReferredByAccountId,
            ParentAccountId = dto.ParentAccountId,
            Notes = dto.Notes ?? string.Empty,
            InternalNotes = dto.InternalNotes,
            Description = dto.Description,
            OptInEmail = dto.OptInEmail,
            OptInSms = dto.OptInSms,
            OptInPhone = dto.OptInPhone,
            PreferredContactMethod = dto.PreferredContactMethod,
            Timezone = dto.Timezone,
            PreferredLanguage = dto.PreferredLanguage,
            Currency = dto.Currency,
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account);
        await _accountRepository.SaveAsync();

        // Materialize normalized contact info for new account
        if (!string.IsNullOrWhiteSpace(dto.Address) || !string.IsNullOrWhiteSpace(dto.City) || !string.IsNullOrWhiteSpace(dto.Country))
        {
            var addr = new Address
            {
                Label = "Primary",
                Line1 = dto.Address ?? string.Empty,
                Line2 = dto.Address2,
                City = dto.City ?? string.Empty,
                State = dto.State,
                PostalCode = dto.ZipCode,
                Country = dto.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _addressRepository.AddAsync(addr);
            await _addressRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = dto.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepository.AddAsync(email);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = dto.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepository.AddAsync(phone);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }



        return await MapToDto(account);
    }

    public async Task<AccountDto?> UpdateAccountAsync(int id, UpdateAccountDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null || account.IsDeleted)
            return null;

        // Update fields if provided
        if (dto.Category.HasValue) account.Category = dto.Category.Value;
        if (dto.FirstName != null) account.FirstName = dto.FirstName;
        if (dto.LastName != null) account.LastName = dto.LastName;
        if (dto.Salutation != null) account.Salutation = dto.Salutation;
        if (dto.Suffix != null) account.Suffix = dto.Suffix;
        if (dto.DateOfBirth.HasValue) account.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) account.Gender = dto.Gender;
        if (dto.LinkedContactId.HasValue) account.LinkedContactId = dto.LinkedContactId;
        if (dto.Company != null) account.Company = dto.Company;
        if (dto.LegalName != null) account.LegalName = dto.LegalName;
        if (dto.DbaName != null) account.DbaName = dto.DbaName;
        if (dto.TaxId != null) account.TaxId = dto.TaxId;
        if (dto.RegistrationNumber != null) account.RegistrationNumber = dto.RegistrationNumber;
        if (dto.YearFounded.HasValue) account.YearFounded = dto.YearFounded;
        if (dto.PrimaryContactId.HasValue) account.PrimaryContactId = dto.PrimaryContactId;
        if (dto.Email != null) account.Email = dto.Email;
        if (dto.SecondaryEmail != null) account.SecondaryEmail = dto.SecondaryEmail;
        if (dto.Phone != null) account.Phone = dto.Phone;
        if (dto.MobilePhone != null) account.MobilePhone = dto.MobilePhone;
        if (dto.FaxNumber != null) account.FaxNumber = dto.FaxNumber;
        if (dto.JobTitle != null) account.JobTitle = dto.JobTitle;
        if (dto.Website != null) account.Website = dto.Website;
        if (dto.Address != null) account.Address = dto.Address;
        if (dto.Address2 != null) account.Address2 = dto.Address2;
        if (dto.City != null) account.City = dto.City;
        if (dto.State != null) account.State = dto.State;
        if (dto.ZipCode != null) account.ZipCode = dto.ZipCode;
        if (dto.Country != null) account.Country = dto.Country;
        if (dto.ShippingAddress != null) account.ShippingAddress = dto.ShippingAddress;
        if (dto.ShippingAddress2 != null) account.ShippingAddress2 = dto.ShippingAddress2;
        if (dto.ShippingCity != null) account.ShippingCity = dto.ShippingCity;
        if (dto.ShippingState != null) account.ShippingState = dto.ShippingState;
        if (dto.ShippingZipCode != null) account.ShippingZipCode = dto.ShippingZipCode;
        if (dto.ShippingCountry != null) account.ShippingCountry = dto.ShippingCountry;
        if (dto.ShippingSameAsBilling.HasValue) account.ShippingSameAsBilling = dto.ShippingSameAsBilling.Value;
        if (dto.Industry != null) account.Industry = dto.Industry;
        if (dto.SubIndustry != null) account.SubIndustry = dto.SubIndustry;
        if (dto.NumberOfEmployees.HasValue) account.NumberOfEmployees = dto.NumberOfEmployees;
        if (dto.EmployeeRange != null) account.EmployeeRange = dto.EmployeeRange;
        if (dto.AnnualRevenue.HasValue) account.AnnualRevenue = dto.AnnualRevenue.Value;
        if (dto.RevenueRange != null) account.RevenueRange = dto.RevenueRange;
        if (dto.AccountType.HasValue) account.AccountType = dto.AccountType.Value;
        if (dto.Priority.HasValue) account.Priority = dto.Priority.Value;
        if (dto.StockSymbol != null) account.StockSymbol = dto.StockSymbol;
        if (dto.Ownership != null) account.Ownership = dto.Ownership;
        if (dto.LifecycleStage.HasValue) account.LifecycleStage = dto.LifecycleStage.Value;
        if (dto.LeadSource != null) account.LeadSource = dto.LeadSource;
        if (dto.NextFollowUpDate.HasValue) account.NextFollowUpDate = dto.NextFollowUpDate;
        if (dto.CreditLimit.HasValue) account.CreditLimit = dto.CreditLimit.Value;
        if (dto.PaymentTerms != null) account.PaymentTerms = dto.PaymentTerms;
        if (dto.PreferredPaymentMethod != null) account.PreferredPaymentMethod = dto.PreferredPaymentMethod;
        if (dto.Currency != null) account.Currency = dto.Currency;
        if (dto.BillingCycle != null) account.BillingCycle = dto.BillingCycle;
        if (dto.LeadScore.HasValue) account.LeadScore = dto.LeadScore.Value;
        if (dto.AccountHealthScore.HasValue) account.AccountHealthScore = dto.AccountHealthScore.Value;
        if (dto.NpsScore.HasValue) account.NpsScore = dto.NpsScore.Value;
        if (dto.SatisfactionRating.HasValue) account.SatisfactionRating = dto.SatisfactionRating.Value;
        if (dto.AssignedToUserId.HasValue) account.AssignedToUserId = dto.AssignedToUserId;
        if (dto.AccountManagerId.HasValue) account.AccountManagerId = dto.AccountManagerId;
        if (dto.Territory != null) account.Territory = dto.Territory;
        if (dto.Region != null) account.Region = dto.Region;
        if (dto.Tags != null) account.Tags = dto.Tags;
        if (dto.Segment != null) account.Segment = dto.Segment;
        if (dto.ReferralSource != null) account.ReferralSource = dto.ReferralSource;
        if (dto.ReferredByAccountId.HasValue) account.ReferredByAccountId = dto.ReferredByAccountId;
        if (dto.ParentAccountId.HasValue) account.ParentAccountId = dto.ParentAccountId;
        if (dto.Notes != null) account.Notes = dto.Notes;
        if (dto.InternalNotes != null) account.InternalNotes = dto.InternalNotes;
        if (dto.Description != null) account.Description = dto.Description;
        if (dto.CustomFields != null) account.CustomFields = dto.CustomFields;
        if (dto.OptInEmail.HasValue) account.OptInEmail = dto.OptInEmail.Value;
        if (dto.OptInSms.HasValue) account.OptInSms = dto.OptInSms.Value;
        if (dto.OptInPhone.HasValue) account.OptInPhone = dto.OptInPhone.Value;
        if (dto.PreferredContactMethod != null) account.PreferredContactMethod = dto.PreferredContactMethod;
        if (dto.PreferredContactTime != null) account.PreferredContactTime = dto.PreferredContactTime;
        if (dto.Timezone != null) account.Timezone = dto.Timezone;
        if (dto.PreferredLanguage != null) account.PreferredLanguage = dto.PreferredLanguage;
        if (dto.LinkedInUrl != null) account.LinkedInUrl = dto.LinkedInUrl;
        if (dto.TwitterHandle != null) account.TwitterHandle = dto.TwitterHandle;
        if (dto.FacebookUrl != null) account.FacebookUrl = dto.FacebookUrl;

        account.UpdatedAt = DateTime.UtcNow;
        account.LastActivityDate = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        await _accountRepository.SaveAsync();

        // If inline contact fields were updated, materialize them into normalized tables
        if (dto.Address != null || dto.City != null || dto.Country != null)
        {
            // unset existing primary address links for this account
            var existingAddrLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == account.Id && l.InfoKind == ContactInfoKind.Address && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingAddrLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var addr = new Address
            {
                Label = "Primary",
                Line1 = dto.Address ?? string.Empty,
                Line2 = dto.Address2,
                City = dto.City ?? string.Empty,
                State = dto.State,
                PostalCode = dto.ZipCode,
                Country = dto.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _addressRepository.AddAsync(addr);
            await _addressRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.Email != null)
        {
            var existingEmailLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == account.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingEmailLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }
            // Save immediately to avoid entity tracking conflicts with phone updates
            await _contactInfoLinkRepository.SaveAsync();

            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = dto.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepository.AddAsync(email);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.Phone != null)
        {
            var existingPhoneLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == account.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingPhoneLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = dto.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepository.AddAsync(phone);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.LinkedInUrl != null)
        {
            var existingSocialLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == account.Id && l.InfoKind == ContactInfoKind.SocialAccount && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingSocialLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var sa = new SocialAccount
            {
                Network = SocialNetwork.LinkedIn,
                HandleOrUrl = dto.LinkedInUrl,
                Label = "LinkedIn",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _socialAccountRepository.AddAsync(sa);
            await _socialAccountRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = account.Id,
                InfoKind = ContactInfoKind.SocialAccount,
                InfoId = sa.Id,
                SocialAccountId = sa.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        return await MapToDto(account);
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
        var account = await _accountRepository.GetByIdAsync(id);
        if (account == null)
            return false;

        account.IsDeleted = true;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        await _accountRepository.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<AccountDto>> GetIndividualAccountsAsync()
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && c.Category == AccountCategory.Individual);

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    public async Task<IEnumerable<AccountDto>> GetOrganizationAccountsAsync()
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && c.Category == AccountCategory.Organization);

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    public async Task<AccountContactDto?> LinkContactToAccountAsync(int accountId, LinkContactToAccountDto dto)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
            return null;

        // Verify contact exists
        var contact = await _contactsService.GetByIdAsync(dto.ContactId);
        if (contact == null)
            return null;

        // Check if already linked
        var existingLinks = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == dto.ContactId && !cc.IsDeleted);
        if (existingLinks.Any())
            return null;

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

        // Update primary contact on account
        if (dto.IsPrimaryContact)
        {
            account.PrimaryContactId = dto.ContactId;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveAsync();
        }

        return MapAccountContactToDto(accountContact, contact);
    }

    public async Task<bool> UnlinkContactFromAccountAsync(int accountId, int contactId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
            return false;

        link.IsDeleted = true;
        link.RelationshipEndDate = DateTime.UtcNow;
        link.UpdatedAt = DateTime.UtcNow;

        await _accountContactRepository.UpdateAsync(link);
        await _accountContactRepository.SaveAsync();

        // If this was primary contact, clear it
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account != null && account.PrimaryContactId == contactId)
        {
            account.PrimaryContactId = null;
            await _accountRepository.UpdateAsync(account);
            await _accountRepository.SaveAsync();
        }

        return true;
    }

    public async Task<AccountContactDto?> UpdateAccountContactAsync(int accountId, int contactId, UpdateAccountContactDto dto)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
            return null;

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

                // Update account's primary contact
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account != null)
                {
                    account.PrimaryContactId = contactId;
                    await _accountRepository.UpdateAsync(account);
                }
            }
            link.IsPrimaryContact = dto.IsPrimaryContact.Value;
        }

        link.UpdatedAt = DateTime.UtcNow;
        await _accountContactRepository.UpdateAsync(link);
        await _accountContactRepository.SaveAsync();

        var contact = await _contactsService.GetByIdAsync(contactId);
        return MapAccountContactToDto(link, contact);
    }

    public async Task<IEnumerable<AccountContactDto>> GetAccountContactsAsync(int accountId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && !cc.IsDeleted);

        var dtos = new List<AccountContactDto>();
        foreach (var link in links)
        {
            var contact = await _contactsService.GetByIdAsync(link.ContactId);
            dtos.Add(MapAccountContactToDto(link, contact));
        }
        return dtos;
    }

    public async Task<bool> SetPrimaryContactAsync(int accountId, int contactId)
    {
        var links = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && cc.ContactId == contactId && !cc.IsDeleted);

        var link = links.FirstOrDefault();
        if (link == null)
            return false;

        // Unset all other primary contacts
        var allLinks = await _accountContactRepository.FindAsync(cc =>
            cc.AccountId == accountId && !cc.IsDeleted);
        foreach (var l in allLinks)
        {
            l.IsPrimaryContact = (l.Id == link.Id);
            await _accountContactRepository.UpdateAsync(l);
        }

        // Update account's primary contact
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

    // === Direct Contact Management (One-to-Many via Contact.AccountId) ===

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
        }).ToList();
    }

    public async Task<bool> AssignContactToAccountAsync(int accountId, int contactId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
            return false;

        var contact = await _contactsService.GetByIdAsync(contactId);
        if (contact == null)
            return false;

        // Update the contact's AccountId
        await _contactsService.AssignToAccountAsync(contactId, accountId);
        return true;
    }

    public async Task<bool> UnassignContactFromAccountAsync(int accountId, int contactId)
    {
        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null || account.IsDeleted)
            return false;

        var contact = await _contactsService.GetByIdAsync(contactId);
        if (contact == null || contact.AccountId != accountId)
            return false;

        // Clear the contact's AccountId
        await _contactsService.UnassignFromAccountAsync(contactId);
        return true;
    }

    public async Task<IEnumerable<AccountDto>> GetAccountsByAssignedUserAsync(int userId)
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && c.AssignedToUserId == userId);

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    public async Task<IEnumerable<AccountDto>> GetAccountsByLifecycleStageAsync(AccountLifecycleStage stage)
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && c.LifecycleStage == stage);

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    public async Task<IEnumerable<AccountDto>> GetAccountsByPriorityAsync(AccountPriority priority)
    {
        var accounts = await _accountRepository.FindAsync(c =>
            !c.IsDeleted && c.Priority == priority);

        var dtos = new List<AccountDto>();
        foreach (var account in accounts)
        {
            dtos.Add(await MapToDto(account));
        }
        return dtos;
    }

    private async Task<AccountDto> MapToDto(Account account)
    {
        // Get linked contacts for organizations
        List<AccountContactDto>? contactDtos = null;
        int contactCount = 0;
        string? primaryContactName = null;
        string? linkedContactName = null;

        if (account.Category == AccountCategory.Organization)
        {
            var contacts = await _accountContactRepository.FindAsync(cc =>
                cc.AccountId == account.Id && !cc.IsDeleted);
            contactCount = contacts.Count();

            contactDtos = new List<AccountContactDto>();
            foreach (var cc in contacts)
            {
                var contact = await _contactsService.GetByIdAsync(cc.ContactId);
                contactDtos.Add(MapAccountContactToDto(cc, contact));

                if (cc.IsPrimaryContact && contact != null)
                {
                    primaryContactName = $"{contact.FirstName} {contact.LastName}";
                }
            }
        }
        else if (account.LinkedContactId.HasValue)
        {
            var contact = await _contactsService.GetByIdAsync(account.LinkedContactId.Value);
            if (contact != null)
            {
                linkedContactName = $"{contact.FirstName} {contact.LastName}";
            }
        }

            // Prefer normalized contact-info, tags and custom fields when available (use NormalizationService)
            var primaryEmail = await _normalizationService.GetPrimaryEmailAsync(ContactInfoOwnerType.Account, account.Id) ?? account.Email;
            var primaryPhone = await _normalizationService.GetPrimaryPhoneAsync(ContactInfoOwnerType.Account, account.Id) ?? account.Phone;
            var primaryFax = await _normalizationService.GetPrimaryFaxAsync(ContactInfoOwnerType.Account, account.Id) ?? account.FaxNumber;
            var primaryAddressEntity = await _normalizationService.GetPrimaryAddressAsync(ContactInfoOwnerType.Account, account.Id);

            var addrLine1 = primaryAddressEntity?.Line1 ?? account.Address;
            var addrLine2 = primaryAddressEntity?.Line2 ?? account.Address2;
            var addrCity = primaryAddressEntity?.City ?? account.City;
            var addrState = primaryAddressEntity?.State ?? account.State;
            var addrPostal = primaryAddressEntity?.PostalCode ?? account.ZipCode;
            var addrCountry = primaryAddressEntity?.Country ?? account.Country;

            var tagsValue = await _normalizationService.GetTagsAsync("Account", account.Id) ?? account.Tags;
            var customFieldsValue = await _normalizationService.GetCustomFieldsAsync("Account", account.Id) ?? account.CustomFields;

            // Prefer normalized social accounts when available
            var linkedInUrl = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Account, account.Id, SocialNetwork.LinkedIn) ?? account.LinkedInUrl;
            var twitterHandle = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Account, account.Id, SocialNetwork.Twitter) ?? account.TwitterHandle;
            var facebookUrl = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Account, account.Id, SocialNetwork.Facebook) ?? account.FacebookUrl;

        return new AccountDto
        {
            Id = account.Id,
            Category = account.Category.ToString(),
            FirstName = account.FirstName,
            LastName = account.LastName,
            Salutation = account.Salutation,
            Suffix = account.Suffix,
            DateOfBirth = account.DateOfBirth,
            Gender = account.Gender,
            LinkedContactId = account.LinkedContactId,
            LinkedContactName = linkedContactName,
            Company = account.Company,
            LegalName = account.LegalName,
            DbaName = account.DbaName,
            TaxId = account.TaxId,
            RegistrationNumber = account.RegistrationNumber,
            YearFounded = account.YearFounded,
            PrimaryContactId = account.PrimaryContactId,
            PrimaryContactName = primaryContactName,
            Email = primaryEmail,
            SecondaryEmail = account.SecondaryEmail,
            Phone = primaryPhone,
            MobilePhone = account.MobilePhone,
            FaxNumber = primaryFax,
            JobTitle = account.JobTitle,
            Website = account.Website,
            Address = addrLine1,
            Address2 = addrLine2,
            City = addrCity,
            State = addrState,
            ZipCode = addrPostal,
            Country = addrCountry,
            ShippingAddress = account.ShippingAddress,
            ShippingAddress2 = account.ShippingAddress2,
            ShippingCity = account.ShippingCity,
            ShippingState = account.ShippingState,
            ShippingZipCode = account.ShippingZipCode,
            ShippingCountry = account.ShippingCountry,
            ShippingSameAsBilling = account.ShippingSameAsBilling,
            Industry = account.Industry,
            SubIndustry = account.SubIndustry,
            NumberOfEmployees = account.NumberOfEmployees,
            EmployeeRange = account.EmployeeRange,
            AnnualRevenue = account.AnnualRevenue,
            RevenueRange = account.RevenueRange,
            AccountType = account.AccountType.ToString(),
            Priority = account.Priority.ToString(),
            StockSymbol = account.StockSymbol,
            Ownership = account.Ownership,
            LifecycleStage = account.LifecycleStage.ToString(),
            LeadSource = account.LeadSource,
            FirstContactDate = account.FirstContactDate,
            ConversionDate = account.ConversionDate,
            LastActivityDate = account.LastActivityDate,
            NextFollowUpDate = account.NextFollowUpDate,
            TotalPurchases = account.TotalPurchases,
            AccountBalance = account.AccountBalance,
            CreditLimit = account.CreditLimit,
            PaymentTerms = account.PaymentTerms,
            PreferredPaymentMethod = account.PreferredPaymentMethod,
            Currency = account.Currency,
            BillingCycle = account.BillingCycle,
            LeadScore = account.LeadScore,
            AccountHealthScore = account.AccountHealthScore,
            NpsScore = account.NpsScore,
            SatisfactionRating = account.SatisfactionRating,
            LinkedInUrl = linkedInUrl,
            TwitterHandle = twitterHandle,
            FacebookUrl = facebookUrl,
            OptInEmail = account.OptInEmail,
            OptInSms = account.OptInSms,
            OptInPhone = account.OptInPhone,
            PreferredContactMethod = account.PreferredContactMethod,
            PreferredContactTime = account.PreferredContactTime,
            Timezone = account.Timezone,
            PreferredLanguage = account.PreferredLanguage,
            AssignedToUserId = account.AssignedToUserId,
            AssignedToUserName = account.AssignedToUser?.Username,
            AccountManagerId = account.AccountManagerId,
            AccountManagerName = account.AccountManager?.Username,
            Territory = account.Territory,
            Region = account.Region,
            Tags = tagsValue,
            Segment = account.Segment,
            ReferralSource = account.ReferralSource,
            ReferredByAccountId = account.ReferredByAccountId,
            ReferredByAccountName = account.ReferredByAccount?.DisplayName,
            ParentAccountId = account.ParentAccountId,
            ParentAccountName = account.ParentAccount?.DisplayName,
            Notes = account.Notes,
            InternalNotes = account.InternalNotes,
            Description = account.Description,
            CustomFields = customFieldsValue,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            RowVersion = account.RowVersion,
            DisplayName = account.DisplayName,
            Contacts = contactDtos,
            ContactCount = contactCount,

            // === Normalized Contact Info Collections ===
            EmailAddresses = await _contactInfoService.GetEmailAddressesAsync(EntityType.Account, account.Id),
            PhoneNumbers = await _contactInfoService.GetPhoneNumbersAsync(EntityType.Account, account.Id),
            Addresses = await _contactInfoService.GetAddressesAsync(EntityType.Account, account.Id),
            SocialMediaAccounts = await _contactInfoService.GetSocialMediaAccountsAsync(EntityType.Account, account.Id)
        };
    }

    private AccountContactDto MapAccountContactToDto(AccountContact cc, ContactDto? contact)
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
            RelationshipStartDate = cc.RelationshipStartDate,
            RelationshipEndDate = cc.RelationshipEndDate,
            Notes = cc.Notes,
            CreatedAt = cc.CreatedAt
        };
    }
}

