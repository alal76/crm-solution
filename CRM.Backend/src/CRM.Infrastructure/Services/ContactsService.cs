using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using CRM.Core.Models;
using CRM.Core.Ports.Input;
using CRM.Infrastructure.Data;
using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
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
    private readonly CrmDbContext _context;

    public ContactsService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<ContactDto> GetByIdAsync(int id)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
            throw new InvalidOperationException($"Contact with ID {id} not found");

        return MapToDto(contact);
    }

    public async Task<List<ContactDto>> GetAllAsync()
    {
        var contacts = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetByTypeAsync(string contactType)
    {
        if (!Enum.TryParse<ContactType>(contactType, true, out var type))
            return new List<ContactDto>();

        var contacts = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .Where(c => c.ContactType == type)
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<ContactDto> CreateAsync(CreateContactRequest request, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("First name and last name are required");

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
            PhonePrimary = request.PhonePrimary,
            PhoneSecondary = request.PhoneSecondary,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            ZipCode = request.ZipCode,
            JobTitle = request.JobTitle,
            Department = request.Department,
            Company = request.Company,
            ReportsTo = request.ReportsTo,
            Notes = request.Notes,
            DateOfBirth = request.DateOfBirth,
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

        return MapToDto(contact);
    }

    public async Task<ContactDto> UpdateAsync(int id, UpdateContactRequest request, string modifiedBy)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
            throw new InvalidOperationException($"Contact with ID {id} not found");

        if (!string.IsNullOrWhiteSpace(request.ContactType))
        {
            if (Enum.TryParse<ContactType>(request.ContactType, true, out var type))
                contact.ContactType = type;
        }

        if (!string.IsNullOrWhiteSpace(request.FirstName))
            contact.FirstName = request.FirstName;

        if (!string.IsNullOrWhiteSpace(request.LastName))
            contact.LastName = request.LastName;

        if (request.MiddleName != null)
            contact.MiddleName = request.MiddleName;

        if (request.EmailPrimary != null)
            contact.EmailPrimary = request.EmailPrimary;

        if (request.EmailSecondary != null)
            contact.EmailSecondary = request.EmailSecondary;

        if (request.PhonePrimary != null)
            contact.PhonePrimary = request.PhonePrimary;

        if (request.PhoneSecondary != null)
            contact.PhoneSecondary = request.PhoneSecondary;

        if (request.Address != null)
            contact.Address = request.Address;

        if (request.City != null)
            contact.City = request.City;

        if (request.State != null)
            contact.State = request.State;

        if (request.Country != null)
            contact.Country = request.Country;

        if (request.ZipCode != null)
            contact.ZipCode = request.ZipCode;

        if (request.JobTitle != null)
            contact.JobTitle = request.JobTitle;

        if (request.Department != null)
            contact.Department = request.Department;

        if (request.Company != null)
            contact.Company = request.Company;

        if (request.ReportsTo != null)
            contact.ReportsTo = request.ReportsTo;

        if (request.Notes != null)
            contact.Notes = request.Notes;

        if (request.DateOfBirth.HasValue)
            contact.DateOfBirth = request.DateOfBirth;

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

        return MapToDto(contact);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var contact = await _context.Contacts
            .Include(c => c.SocialMediaLinks)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contact == null)
            throw new InvalidOperationException($"Contact with ID {id} not found");

        // Delete associated social media links
        _context.SocialMediaLinks.RemoveRange(contact.SocialMediaLinks);
        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<SocialMediaLinkDto> AddSocialMediaLinkAsync(int contactId, AddSocialMediaRequest request)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        if (contact == null)
            throw new InvalidOperationException($"Contact with ID {contactId} not found");

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
            throw new InvalidOperationException($"Social media link with ID {linkId} not found");

        _context.SocialMediaLinks.Remove(link);
        await _context.SaveChangesAsync();

        return true;
    }

    private static ContactDto MapToDto(Contact contact)
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
            PhonePrimary = contact.PhonePrimary,
            PhoneSecondary = contact.PhoneSecondary,
            Address = contact.Address,
            City = contact.City,
            State = contact.State,
            Country = contact.Country,
            ZipCode = contact.ZipCode,
            JobTitle = contact.JobTitle,
            Department = contact.Department,
            Company = contact.Company,
            ReportsTo = contact.ReportsTo,
            Notes = contact.Notes,
            DateOfBirth = contact.DateOfBirth,
            DateAdded = contact.DateAdded,
            LastModified = contact.LastModified,
            ModifiedBy = contact.ModifiedBy,
            SocialMediaLinks = contact.SocialMediaLinks
                .Select(l => new SocialMediaLinkDto
                {
                    Id = l.Id,
                    Platform = l.Platform.ToString(),
                    Url = l.Url,
                    Handle = l.Handle
                })
                .ToList()
        };
    }
}
