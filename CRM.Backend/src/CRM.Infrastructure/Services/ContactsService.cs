// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CRM.Core.Exceptions;
using Microsoft.Extensions.Logging;
using SocialPlatform = CRM.Core.Models.SocialMediaPlatform;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Contact service implementation.
///
/// HEXAGONAL ARCHITECTURE:
/// - Implements IContactInputPort (primary/driving port)
/// - Implements IContactsService (backward compatibility)
/// - Uses CrmDbContext for data access (secondary/driven port)
/// </summary>
public class ContactsService : IContactsService, IContactInputPort
{
    private readonly ICrmDbContext _context;
    private readonly IContactInfoService _contactInfoService;
    private readonly IEntityEventDispatcher _eventDispatcher;
    private readonly IDuplicateDetectionService _duplicateDetection;
    private readonly ILogger<ContactsService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactsService"/> class.
    /// </summary>
    public ContactsService(ICrmDbContext context, IContactInfoService contactInfoService, IEntityEventDispatcher eventDispatcher, IDuplicateDetectionService duplicateDetection, ILogger<ContactsService> logger)
    {
        _context = context;
        _contactInfoService = contactInfoService;
        _eventDispatcher = eventDispatcher;
        _duplicateDetection = duplicateDetection;
        _logger = logger;
    }

    public async Task<ContactDto> GetByIdAsync(int id)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {id} not found");
        }

        return await MapToDtoAsync(contact);
    }

    public async Task<List<ContactDto>> GetAllAsync()
    {
        var contacts = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        var dtos = new List<ContactDto>();
        foreach (var contact in contacts)
        {
            dtos.Add(await MapToDtoAsync(contact));
        }
        return dtos;
    }

    public async Task<List<ContactDto>> GetByTypeAsync(string contactType)
    {
        if (!Enum.TryParse<ContactType>(contactType, true, out var type))
        {
            return new List<ContactDto>();
        }

        var contacts = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .Where(c => c.ContactType == type)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        var dtos = new List<ContactDto>();
        foreach (var contact in contacts)
        {
            dtos.Add(await MapToDtoAsync(contact));
        }
        return dtos;
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("First name and last name are required");
        }

        // Duplicate detection check before creation
        var fieldValues = new Dictionary<string, string?>
        {
            ["FirstName"] = request.FirstName,
            ["LastName"] = request.LastName,
            ["Email"] = request.EmailPrimary,
            ["Phone"] = request.PhonePrimary,
            ["CompanyName"] = request.Company
        };
        var candidatesQueued = await DuplicateCheckHelper.CheckAndHandleDuplicatesAsync(
            _duplicateDetection, _context, "Contact", fieldValues, _logger);

        var contactType = Enum.TryParse<ContactType>(request.ContactType, true, out var type)
            ? type
            : ContactType.Other;

        var contact = new Contact
        {
            ContactType = contactType,
            FirstName = request.FirstName,
            LastName = request.LastName,
            MiddleName = request.MiddleName,
            EmailPrimary = request.EmailPrimary,
            EmailSecondary = request.EmailSecondary,
            EmailWork = request.EmailWork,
            PhonePrimary = request.PhonePrimary,
            PhoneSecondary = request.PhoneSecondary,
            PhoneWork = request.PhoneWork,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            ZipCode = request.ZipCode,
            MailingAddress = request.MailingAddress,
            MailingCity = request.MailingCity,
            MailingState = request.MailingState,
            MailingCountry = request.MailingCountry,
            MailingZipCode = request.MailingZipCode,
            JobTitle = request.JobTitle,
            Department = request.Department,
            Company = request.Company,
            ReportsTo = request.ReportsTo,
            ReportsToContactId = request.ReportsToContactId,
            AssistantContactId = request.AssistantContactId,
            AssistantName = request.AssistantName,
            AssistantPhone = request.AssistantPhone,
            Notes = request.Notes,
            DateOfBirth = request.DateOfBirth,
            Salutation = request.Salutation,
            Suffix = request.Suffix,
            Nickname = request.Nickname,
            Gender = request.Gender,
            PhoneMobile = request.PhoneMobile,
            PhoneFax = request.PhoneFax,
            Website = request.Website,
            LinkedInUrl = request.LinkedInUrl,
            TwitterHandle = request.TwitterHandle,
            FacebookUrl = request.FacebookUrl,
            InstagramHandle = request.InstagramHandle,
            BlogUrl = request.BlogUrl,
            DoNotContact = request.DoNotContact,
            PreferredContactTime = request.PreferredContactTime,
            Timezone = request.Timezone,
            PreferredLanguage = request.PreferredLanguage,
            OptInEmail = request.OptInEmail,
            OptInSms = request.OptInSms,
            OptInPhone = request.OptInPhone,
            OptInMail = request.OptInMail,
            LastOptInDate = request.LastOptInDate,
            LastOptOutDate = request.LastOptOutDate,
            LeadSource = request.LeadSource,
            LeadScore = request.LeadScore,
            IsQualified = request.IsQualified,
            QualifiedDate = request.QualifiedDate,
            ConvertedDate = request.ConvertedDate,
            ConvertedToAccountId = request.ConvertedToAccountId,
            LeadRating = request.LeadRating,
            OwnerId = request.OwnerId,
            AssignedToUserId = request.AssignedToUserId,
            Territory = request.Territory,
            Tags = request.Tags,
            CustomFields = request.CustomFields,
            PhotoUrl = request.PhotoUrl,
            Description = request.Description,
            DateAdded = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            ModifiedBy = modifiedBy
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        // Materialize normalized contact info for new contact
        if (!string.IsNullOrWhiteSpace(request.Address) || !string.IsNullOrWhiteSpace(request.City) || !string.IsNullOrWhiteSpace(request.Country))
        {
            var addr = new Address
            {
                Label = "Primary",
                Line1 = request.Address ?? string.Empty,
                Line2 = null,
                City = request.City ?? string.Empty,
                State = request.State,
                PostalCode = request.ZipCode,
                Country = request.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "created_from_api"
            };
            _context.Addresses.Add(addr);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.EmailPrimary))
        {
            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = request.EmailPrimary,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            _context.ContactDetails.Add(email);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.PhonePrimary))
        {
            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = request.PhonePrimary,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            _context.ContactDetails.Add(phone);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        // Update any queued duplicate candidates with the new entity ID
        if (candidatesQueued > 0)
        {
            await DuplicateCheckHelper.UpdateCandidateSourceIdsAsync(_context, "Contact", contact.Id);
        }

        // Fire workflow triggers for entity creation
        await _eventDispatcher.DispatchEntityEventAsync("Contact", contact.Id, WorkflowTriggerType.OnCreate);

        return await MapToDtoAsync(contact);
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request, string modifiedBy)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {id} not found");
        }

        if (!string.IsNullOrWhiteSpace(request.ContactType))
        {
            if (Enum.TryParse<ContactType>(request.ContactType, true, out var type))
            {
                contact.ContactType = type;
            }
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            contact.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            contact.LastName = request.LastName;
        }

        if (request.MiddleName != null)
        {
            contact.MiddleName = request.MiddleName;
        }

        if (request.EmailPrimary != null)
        {
            contact.EmailPrimary = request.EmailPrimary;
        }

        if (request.EmailSecondary != null)
        {
            contact.EmailSecondary = request.EmailSecondary;
        }

        if (request.PhonePrimary != null)
        {
            contact.PhonePrimary = request.PhonePrimary;
        }

        if (request.PhoneSecondary != null)
        {
            contact.PhoneSecondary = request.PhoneSecondary;
        }

        if (request.Address != null)
        {
            contact.Address = request.Address;
        }

        if (request.City != null)
        {
            contact.City = request.City;
        }

        if (request.State != null)
        {
            contact.State = request.State;
        }

        if (request.Country != null)
        {
            contact.Country = request.Country;
        }

        if (request.ZipCode != null)
        {
            contact.ZipCode = request.ZipCode;
        }

        if (request.JobTitle != null)
        {
            contact.JobTitle = request.JobTitle;
        }

        if (request.Department != null)
        {
            contact.Department = request.Department;
        }

        if (request.Company != null)
        {
            contact.Company = request.Company;
        }

        if (request.ReportsTo != null)
        {
            contact.ReportsTo = request.ReportsTo;
        }

        if (request.Notes != null)
        {
            contact.Notes = request.Notes;
        }

        if (request.DateOfBirth.HasValue)
        {
            contact.DateOfBirth = request.DateOfBirth;
        }

        if (request.EmailWork != null)
        {
            contact.EmailWork = request.EmailWork;
        }

        if (request.PhoneWork != null)
        {
            contact.PhoneWork = request.PhoneWork;
        }

        if (request.MailingAddress != null)
        {
            contact.MailingAddress = request.MailingAddress;
        }

        if (request.MailingCity != null)
        {
            contact.MailingCity = request.MailingCity;
        }

        if (request.MailingState != null)
        {
            contact.MailingState = request.MailingState;
        }

        if (request.MailingCountry != null)
        {
            contact.MailingCountry = request.MailingCountry;
        }

        if (request.MailingZipCode != null)
        {
            contact.MailingZipCode = request.MailingZipCode;
        }

        if (request.ReportsToContactId.HasValue)
        {
            contact.ReportsToContactId = request.ReportsToContactId;
        }

        if (request.AssistantContactId.HasValue)
        {
            contact.AssistantContactId = request.AssistantContactId;
        }

        if (request.AssistantName != null)
        {
            contact.AssistantName = request.AssistantName;
        }

        if (request.AssistantPhone != null)
        {
            contact.AssistantPhone = request.AssistantPhone;
        }

        if (request.FacebookUrl != null)
        {
            contact.FacebookUrl = request.FacebookUrl;
        }

        if (request.InstagramHandle != null)
        {
            contact.InstagramHandle = request.InstagramHandle;
        }

        if (request.BlogUrl != null)
        {
            contact.BlogUrl = request.BlogUrl;
        }

        if (request.PreferredContactTime != null)
        {
            contact.PreferredContactTime = request.PreferredContactTime;
        }

        if (request.Timezone != null)
        {
            contact.Timezone = request.Timezone;
        }

        if (request.PreferredLanguage != null)
        {
            contact.PreferredLanguage = request.PreferredLanguage;
        }

        if (request.OptInEmail.HasValue)
        {
            contact.OptInEmail = request.OptInEmail.Value;
        }

        if (request.OptInSms.HasValue)
        {
            contact.OptInSms = request.OptInSms.Value;
        }

        if (request.OptInPhone.HasValue)
        {
            contact.OptInPhone = request.OptInPhone.Value;
        }

        if (request.OptInMail.HasValue)
        {
            contact.OptInMail = request.OptInMail.Value;
        }

        if (request.LastOptInDate.HasValue)
        {
            contact.LastOptInDate = request.LastOptInDate;
        }

        if (request.LastOptOutDate.HasValue)
        {
            contact.LastOptOutDate = request.LastOptOutDate;
        }

        if (request.LeadSource != null)
        {
            contact.LeadSource = request.LeadSource;
        }

        if (request.LeadScore.HasValue)
        {
            contact.LeadScore = request.LeadScore;
        }

        if (request.IsQualified.HasValue)
        {
            contact.IsQualified = request.IsQualified;
        }

        if (request.QualifiedDate.HasValue)
        {
            contact.QualifiedDate = request.QualifiedDate;
        }

        if (request.ConvertedDate.HasValue)
        {
            contact.ConvertedDate = request.ConvertedDate;
        }

        if (request.ConvertedToAccountId.HasValue)
        {
            contact.ConvertedToAccountId = request.ConvertedToAccountId;
        }

        if (request.LeadRating != null)
        {
            contact.LeadRating = request.LeadRating;
        }

        if (request.OwnerId.HasValue)
        {
            contact.OwnerId = request.OwnerId;
        }

        if (request.AssignedToUserId.HasValue)
        {
            contact.AssignedToUserId = request.AssignedToUserId;
        }

        if (request.Territory != null)
        {
            contact.Territory = request.Territory;
        }

        if (request.Tags != null)
        {
            contact.Tags = request.Tags;
        }

        if (request.CustomFields != null)
        {
            contact.CustomFields = request.CustomFields;
        }

        if (request.PhotoUrl != null)
        {
            contact.PhotoUrl = request.PhotoUrl;
        }

        if (request.Description != null)
        {
            contact.Description = request.Description;
        }

        contact.LastModified = DateTime.UtcNow;
        contact.ModifiedBy = modifiedBy;

        _context.Contacts.Update(contact);
        await _context.SaveChangesAsync();

        // If inline contact fields were updated, materialize them into normalized tables
        if (request.Address != null || request.City != null || request.Country != null)
        {
            // unset existing primary address links for this contact
            var existingAddrLinks = await _context.ContactInfoLinks
                .Where(l => l.OwnerType == ContactInfoOwnerType.Contact && l.OwnerId == contact.Id && l.InfoKind == ContactInfoKind.Address && l.IsPrimaryForOwner && !l.IsDeleted)
                .ToListAsync();
            foreach (var l in existingAddrLinks)
            {
                l.IsPrimaryForOwner = false;
                _context.ContactInfoLinks.Update(l);
            }

            var addr = new Address
            {
                Label = "Primary",
                Line1 = request.Address ?? string.Empty,
                Line2 = null,
                City = request.City ?? string.Empty,
                State = request.State,
                PostalCode = request.ZipCode,
                Country = request.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            _context.Addresses.Add(addr);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        if (request.EmailPrimary != null)
        {
            var existingEmailLinks = await _context.ContactInfoLinks
                .Where(l => l.OwnerType == ContactInfoOwnerType.Contact && l.OwnerId == contact.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted)
                .ToListAsync();
            foreach (var l in existingEmailLinks)
            {
                l.IsPrimaryForOwner = false;
                _context.ContactInfoLinks.Update(l);
            }

            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = request.EmailPrimary,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            _context.ContactDetails.Add(email);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        if (request.PhonePrimary != null)
        {
            var existingPhoneLinks = await _context.ContactInfoLinks
                .Where(l => l.OwnerType == ContactInfoOwnerType.Contact && l.OwnerId == contact.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted)
                .ToListAsync();
            foreach (var l in existingPhoneLinks)
            {
                l.IsPrimaryForOwner = false;
                _context.ContactInfoLinks.Update(l);
            }

            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = request.PhonePrimary,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            _context.ContactDetails.Add(phone);
            await _context.SaveChangesAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = contact.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            _context.ContactInfoLinks.Add(link);
            await _context.SaveChangesAsync();
        }

        // Fire workflow triggers for entity update
        _eventDispatcher.DispatchEntityEvent("Contact", contact.Id, WorkflowTriggerType.OnUpdate);

        return await MapToDtoAsync(contact);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {id} not found");
        }

        // Delete associated social media links
        _context.SocialMediaLinks.RemoveRange(contact.SocialMediaLinks);
        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        // Fire workflow triggers for entity deletion
        await _eventDispatcher.DispatchEntityEventAsync("Contact", id, WorkflowTriggerType.OnDelete);

        return true;
    }

    public async Task<SocialMediaLinkDto> AddSocialMediaLinkAsync(int contactId, AddSocialMediaRequest request)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {contactId} not found");
        }

        var platform = Enum.TryParse<SocialPlatform>(request.Platform, true, out var p)
            ? p
            : SocialPlatform.Other;

        var link = new SocialMediaLink
        {
            ContactId = contactId,
            Platform = platform,
            Url = request.Url,
            Handle = request.Handle,
            DateAdded = DateTime.UtcNow
        };

        _context.SocialMediaLinks.Add(link);
        await _context.SaveChangesAsync();

        return new SocialMediaLinkDto
        {
            Id = link.Id,
            Platform = link.Platform.ToString(),
            Url = link.Url,
            Handle = link.Handle
        };
    }

    public async Task<bool> RemoveSocialMediaLinkAsync(int linkId)
    {
        var link = await _context.SocialMediaLinks.FindAsync(linkId);
        if (link == null)
        {
            throw new InvalidOperationException($"Social media link with ID {linkId} not found");
        }

        _context.SocialMediaLinks.Remove(link);
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<ContactDto> MapToDtoAsync(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            ContactType = contact.ContactType.ToString(),
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            MiddleName = contact.MiddleName,
            EmailPrimary = contact.EmailPrimary,
            EmailSecondary = contact.EmailSecondary,
            EmailWork = contact.EmailWork,
            PhonePrimary = contact.PhonePrimary,
            PhoneSecondary = contact.PhoneSecondary,
            PhoneWork = contact.PhoneWork,
            Address = contact.Address,
            City = contact.City,
            State = contact.State,
            Country = contact.Country,
            ZipCode = contact.ZipCode,
            MailingAddress = contact.MailingAddress,
            MailingCity = contact.MailingCity,
            MailingState = contact.MailingState,
            MailingCountry = contact.MailingCountry,
            MailingZipCode = contact.MailingZipCode,
            JobTitle = contact.JobTitle,
            Department = contact.Department,
            Company = contact.Company,
            ReportsTo = contact.ReportsTo,
            ReportsToContactId = contact.ReportsToContactId,
            AssistantContactId = contact.AssistantContactId,
            AssistantName = contact.AssistantName,
            AssistantPhone = contact.AssistantPhone,
            Notes = contact.Notes,
            DateOfBirth = contact.DateOfBirth,
            DateAdded = contact.DateAdded,
            LastModified = contact.LastModified,
            ModifiedBy = contact.ModifiedBy,
            Salutation = contact.Salutation,
            Suffix = contact.Suffix,
            Nickname = contact.Nickname,
            Gender = contact.Gender,
            PhoneMobile = contact.PhoneMobile,
            PhoneFax = contact.PhoneFax,
            Website = contact.Website,
            LinkedInUrl = contact.LinkedInUrl,
            TwitterHandle = contact.TwitterHandle,
            FacebookUrl = contact.FacebookUrl,
            InstagramHandle = contact.InstagramHandle,
            BlogUrl = contact.BlogUrl,
            DoNotContact = contact.DoNotContact,
            PreferredContactMethod = contact.PreferredContactMethod.ToString(),
            PreferredContactTime = contact.PreferredContactTime,
            Timezone = contact.Timezone,
            PreferredLanguage = contact.PreferredLanguage,
            OptInEmail = contact.OptInEmail,
            OptInSms = contact.OptInSms,
            OptInPhone = contact.OptInPhone,
            OptInMail = contact.OptInMail,
            LastOptInDate = contact.LastOptInDate,
            LastOptOutDate = contact.LastOptOutDate,
            LeadStatus = contact.LeadStatus?.ToString(),
            LeadSource = contact.LeadSource,
            LeadScore = contact.LeadScore,
            IsQualified = contact.IsQualified,
            QualifiedDate = contact.QualifiedDate,
            ConvertedDate = contact.ConvertedDate,
            ConvertedToAccountId = contact.ConvertedToAccountId,
            LeadRating = contact.LeadRating,
            OwnerId = contact.OwnerId,
            AssignedToUserId = contact.AssignedToUserId,
            Territory = contact.Territory,
            Tags = contact.Tags,
            Status = contact.Status.ToString(),
            AccountId = contact.AccountId,
            LastActivityDate = contact.LastActivityDate,
            LastContactedDate = contact.LastContactedDate,
            NextFollowUpDate = contact.NextFollowUpDate,
            TotalInteractions = contact.TotalInteractions,
            EmailsReceived = contact.EmailsReceived,
            EmailsOpened = contact.EmailsOpened,
            LinksClicked = contact.LinksClicked,
            CustomFields = contact.CustomFields,
            PhotoUrl = contact.PhotoUrl,
            Description = contact.Description,
            SocialMediaLinks = contact.SocialMediaLinks
                .Select(l => new SocialMediaLinkDto
                {
                    Id = l.Id,
                    Platform = l.Platform.ToString(),
                    Url = l.Url,
                    Handle = l.Handle
                })
                .ToList(),

            // === Normalized Contact Info Collections ===
            EmailAddresses = await _contactInfoService.GetEmailAddressesAsync(EntityType.Contact, contact.Id),
            PhoneNumbers = await _contactInfoService.GetPhoneNumbersAsync(EntityType.Contact, contact.Id),
            Addresses = await _contactInfoService.GetAddressesAsync(EntityType.Contact, contact.Id),
            SocialMediaAccounts = await _contactInfoService.GetSocialMediaAccountsAsync(EntityType.Contact, contact.Id)
        };
    }

    // === Account Assignment Methods ===
    public async Task<List<ContactDto>> GetByAccountIdAsync(int accountId)
    {
        var contacts = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .Where(c => c.AccountId == accountId)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        var dtos = new List<ContactDto>();
        foreach (var contact in contacts)
        {
            dtos.Add(await MapToDtoAsync(contact));
        }
        return dtos;
    }

    public async Task AssignToAccountAsync(int contactId, int accountId)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {contactId} not found");
        }

        contact.AccountId = accountId;
        contact.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task UnassignFromAccountAsync(int contactId)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null)
        {
            throw new InvalidOperationException($"Contact with ID {contactId} not found");
        }

        contact.AccountId = null;
        contact.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
