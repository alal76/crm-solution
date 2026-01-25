using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service implementation for managing consolidated contact information
/// </summary>
public class ContactInfoService : IContactInfoService
{
    private readonly CrmDbContext _context;

    public ContactInfoService(CrmDbContext context)
    {
        _context = context;
    }

    #region Address Operations

    public async Task<List<LinkedAddressDto>> GetAddressesAsync(EntityType entityType, int entityId)
    {
        var links = await _context.EntityAddressLinks
            .Include(l => l.Address)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && !l.IsDeleted)
            .Where(l => l.Address != null && !l.Address.IsDeleted)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.AddressType)
            .ToListAsync();

        return links.Select(l => MapToLinkedAddressDto(l)).ToList();
    }

    public async Task<AddressDto?> GetAddressByIdAsync(int addressId)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && !a.IsDeleted);
        
        return address == null ? null : MapToAddressDto(address);
    }

    public async Task<AddressDto> CreateAddressAsync(CreateAddressDto dto, int? createdByUserId = null)
    {
        var address = new Address
        {
            Label = dto.Label ?? "Primary",
            Line1 = dto.Line1,
            Line2 = dto.Line2,
            Line3 = dto.Line3,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            County = dto.County,
            CountryCode = dto.CountryCode ?? "US",
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            IsResidential = dto.IsResidential,
            DeliveryInstructions = dto.DeliveryInstructions,
            AccessHours = dto.AccessHours,
            SiteContactName = dto.SiteContactName,
            SiteContactPhone = dto.SiteContactPhone,
            Notes = dto.Notes,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();

        return MapToAddressDto(address);
    }

    public async Task<LinkedAddressDto> LinkAddressAsync(LinkAddressDto dto, int? createdByUserId = null)
    {
        int addressId;

        // Create new address if needed
        if (dto.AddressId.HasValue)
        {
            addressId = dto.AddressId.Value;
        }
        else if (dto.NewAddress != null)
        {
            var newAddress = await CreateAddressAsync(dto.NewAddress, createdByUserId);
            addressId = newAddress.Id;
        }
        else
        {
            throw new ArgumentException("Either AddressId or NewAddress must be provided");
        }

        // Parse entity type
        if (!Enum.TryParse<EntityType>(dto.EntityType, true, out var entityType))
        {
            throw new ArgumentException($"Invalid EntityType: {dto.EntityType}");
        }

        // Parse address type
        if (!Enum.TryParse<AddressType>(dto.AddressType, true, out var addressType))
        {
            addressType = AddressType.Primary;
        }

        // If setting as primary, unset other primaries of same type
        if (dto.IsPrimary)
        {
            await UnsetPrimaryAddressesAsync(entityType, dto.EntityId, addressType);
        }

        // Check for existing link
        var existingLink = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == dto.EntityId 
                && l.AddressId == addressId 
                && l.AddressType == addressType);

        if (existingLink != null)
        {
            existingLink.IsPrimary = dto.IsPrimary;
            existingLink.ValidFrom = dto.ValidFrom;
            existingLink.ValidTo = dto.ValidTo;
            existingLink.Notes = dto.Notes;
            existingLink.IsDeleted = false;
        }
        else
        {
            existingLink = new EntityAddressLink
            {
                AddressId = addressId,
                EntityType = entityType,
                EntityId = dto.EntityId,
                AddressType = addressType,
                IsPrimary = dto.IsPrimary,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.EntityAddressLinks.Add(existingLink);
        }

        await _context.SaveChangesAsync();

        // Reload with address
        var link = await _context.EntityAddressLinks
            .Include(l => l.Address)
            .FirstAsync(l => l.Id == existingLink.Id);

        return MapToLinkedAddressDto(link);
    }

    public async Task<AddressDto> UpdateAddressAsync(int addressId, CreateAddressDto dto, int? updatedByUserId = null)
    {
        var address = await _context.Addresses.FindAsync(addressId)
            ?? throw new KeyNotFoundException($"Address {addressId} not found");

        address.Label = dto.Label ?? address.Label;
        address.Line1 = dto.Line1;
        address.Line2 = dto.Line2;
        address.Line3 = dto.Line3;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.County = dto.County;
        address.CountryCode = dto.CountryCode ?? address.CountryCode;
        address.Country = dto.Country;
        address.Latitude = dto.Latitude;
        address.Longitude = dto.Longitude;
        address.IsResidential = dto.IsResidential;
        address.DeliveryInstructions = dto.DeliveryInstructions;
        address.AccessHours = dto.AccessHours;
        address.SiteContactName = dto.SiteContactName;
        address.SiteContactPhone = dto.SiteContactPhone;
        address.Notes = dto.Notes;
        address.UpdatedBy = updatedByUserId;
        address.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToAddressDto(address);
    }

    public async Task UnlinkAddressAsync(int linkId)
    {
        var link = await _context.EntityAddressLinks.FindAsync(linkId);
        if (link != null)
        {
            link.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAddressAsync(int addressId)
    {
        var address = await _context.Addresses.FindAsync(addressId);
        if (address != null)
        {
            address.IsDeleted = true;
            address.UpdatedAt = DateTime.UtcNow;

            // Also soft-delete all links
            var links = await _context.EntityAddressLinks
                .Where(l => l.AddressId == addressId)
                .ToListAsync();
            
            foreach (var link in links)
            {
                link.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task SetPrimaryAddressAsync(EntityType entityType, int entityId, int addressId, AddressType addressType)
    {
        await UnsetPrimaryAddressesAsync(entityType, entityId, addressType);

        var link = await _context.EntityAddressLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == entityId 
                && l.AddressId == addressId 
                && l.AddressType == addressType 
                && !l.IsDeleted);

        if (link != null)
        {
            link.IsPrimary = true;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UnsetPrimaryAddressesAsync(EntityType entityType, int entityId, AddressType addressType)
    {
        var primaryLinks = await _context.EntityAddressLinks
            .Where(l => l.EntityType == entityType 
                && l.EntityId == entityId 
                && l.AddressType == addressType 
                && l.IsPrimary 
                && !l.IsDeleted)
            .ToListAsync();

        foreach (var link in primaryLinks)
        {
            link.IsPrimary = false;
        }
    }

    #endregion

    #region Phone Number Operations

    public async Task<List<LinkedPhoneDto>> GetPhoneNumbersAsync(EntityType entityType, int entityId)
    {
        var links = await _context.EntityPhoneLinks
            .Include(l => l.PhoneNumber)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && !l.IsDeleted)
            .Where(l => l.PhoneNumber != null && !l.PhoneNumber.IsDeleted)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.PhoneType)
            .ToListAsync();

        return links.Select(l => MapToLinkedPhoneDto(l)).ToList();
    }

    public async Task<PhoneNumberDto?> GetPhoneNumberByIdAsync(int phoneId)
    {
        var phone = await _context.PhoneNumbers
            .FirstOrDefaultAsync(p => p.Id == phoneId && !p.IsDeleted);
        
        return phone == null ? null : MapToPhoneDto(phone);
    }

    public async Task<PhoneNumberDto> CreatePhoneNumberAsync(CreatePhoneNumberDto dto, int? createdByUserId = null)
    {
        var phone = new PhoneNumber
        {
            Label = dto.Label,
            CountryCode = dto.CountryCode,
            AreaCode = dto.AreaCode,
            Number = dto.Number,
            Extension = dto.Extension,
            CanSMS = dto.CanSMS,
            CanWhatsApp = dto.CanWhatsApp,
            CanFax = dto.CanFax,
            BestTimeToCall = dto.BestTimeToCall,
            Notes = dto.Notes,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        // Generate formatted number
        phone.FormattedNumber = FormatPhoneNumber(phone);

        _context.PhoneNumbers.Add(phone);
        await _context.SaveChangesAsync();

        return MapToPhoneDto(phone);
    }

    public async Task<LinkedPhoneDto> LinkPhoneNumberAsync(LinkPhoneDto dto, int? createdByUserId = null)
    {
        int phoneId;

        if (dto.PhoneId.HasValue)
        {
            phoneId = dto.PhoneId.Value;
        }
        else if (dto.NewPhone != null)
        {
            var newPhone = await CreatePhoneNumberAsync(dto.NewPhone, createdByUserId);
            phoneId = newPhone.Id;
        }
        else
        {
            throw new ArgumentException("Either PhoneId or NewPhone must be provided");
        }

        if (!Enum.TryParse<EntityType>(dto.EntityType, true, out var entityType))
        {
            throw new ArgumentException($"Invalid EntityType: {dto.EntityType}");
        }

        if (!Enum.TryParse<PhoneType>(dto.PhoneType, true, out var phoneType))
        {
            phoneType = PhoneType.Office;
        }

        if (dto.IsPrimary)
        {
            await UnsetPrimaryPhonesAsync(entityType, dto.EntityId);
        }

        var existingLink = await _context.EntityPhoneLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == dto.EntityId 
                && l.PhoneId == phoneId 
                && l.PhoneType == phoneType);

        if (existingLink != null)
        {
            existingLink.IsPrimary = dto.IsPrimary;
            existingLink.DoNotCall = dto.DoNotCall;
            existingLink.ValidFrom = dto.ValidFrom;
            existingLink.ValidTo = dto.ValidTo;
            existingLink.Notes = dto.Notes;
            existingLink.IsDeleted = false;
        }
        else
        {
            existingLink = new EntityPhoneLink
            {
                PhoneId = phoneId,
                EntityType = entityType,
                EntityId = dto.EntityId,
                PhoneType = phoneType,
                IsPrimary = dto.IsPrimary,
                DoNotCall = dto.DoNotCall,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.EntityPhoneLinks.Add(existingLink);
        }

        await _context.SaveChangesAsync();

        var link = await _context.EntityPhoneLinks
            .Include(l => l.PhoneNumber)
            .FirstAsync(l => l.Id == existingLink.Id);

        return MapToLinkedPhoneDto(link);
    }

    public async Task<PhoneNumberDto> UpdatePhoneNumberAsync(int phoneId, CreatePhoneNumberDto dto, int? updatedByUserId = null)
    {
        var phone = await _context.PhoneNumbers.FindAsync(phoneId)
            ?? throw new KeyNotFoundException($"Phone {phoneId} not found");

        phone.Label = dto.Label;
        phone.CountryCode = dto.CountryCode;
        phone.AreaCode = dto.AreaCode;
        phone.Number = dto.Number;
        phone.Extension = dto.Extension;
        phone.CanSMS = dto.CanSMS;
        phone.CanWhatsApp = dto.CanWhatsApp;
        phone.CanFax = dto.CanFax;
        phone.BestTimeToCall = dto.BestTimeToCall;
        phone.Notes = dto.Notes;
        phone.FormattedNumber = FormatPhoneNumber(phone);
        phone.UpdatedBy = updatedByUserId;
        phone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToPhoneDto(phone);
    }

    public async Task UnlinkPhoneNumberAsync(int linkId)
    {
        var link = await _context.EntityPhoneLinks.FindAsync(linkId);
        if (link != null)
        {
            link.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeletePhoneNumberAsync(int phoneId)
    {
        var phone = await _context.PhoneNumbers.FindAsync(phoneId);
        if (phone != null)
        {
            phone.IsDeleted = true;
            phone.UpdatedAt = DateTime.UtcNow;

            var links = await _context.EntityPhoneLinks
                .Where(l => l.PhoneId == phoneId)
                .ToListAsync();
            
            foreach (var link in links)
            {
                link.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task SetPrimaryPhoneAsync(EntityType entityType, int entityId, int phoneId, PhoneType phoneType)
    {
        await UnsetPrimaryPhonesAsync(entityType, entityId);

        var link = await _context.EntityPhoneLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == entityId 
                && l.PhoneId == phoneId 
                && !l.IsDeleted);

        if (link != null)
        {
            link.IsPrimary = true;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UnsetPrimaryPhonesAsync(EntityType entityType, int entityId)
    {
        var primaryLinks = await _context.EntityPhoneLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && l.IsPrimary && !l.IsDeleted)
            .ToListAsync();

        foreach (var link in primaryLinks)
        {
            link.IsPrimary = false;
        }
    }

    private static string FormatPhoneNumber(PhoneNumber phone)
    {
        var parts = new List<string> { phone.CountryCode };
        if (!string.IsNullOrEmpty(phone.AreaCode)) parts.Add(phone.AreaCode);
        parts.Add(phone.Number);
        var formatted = string.Join(" ", parts);
        if (!string.IsNullOrEmpty(phone.Extension))
        {
            formatted += $" x{phone.Extension}";
        }
        return formatted;
    }

    #endregion

    #region Email Address Operations

    public async Task<List<LinkedEmailDto>> GetEmailAddressesAsync(EntityType entityType, int entityId)
    {
        var links = await _context.EntityEmailLinks
            .Include(l => l.EmailAddress)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && !l.IsDeleted)
            .Where(l => l.EmailAddress != null && !l.EmailAddress.IsDeleted)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.EmailType)
            .ToListAsync();

        return links.Select(l => MapToLinkedEmailDto(l)).ToList();
    }

    public async Task<EmailAddressDto?> GetEmailAddressByIdAsync(int emailId)
    {
        var email = await _context.EmailAddresses
            .FirstOrDefaultAsync(e => e.Id == emailId && !e.IsDeleted);
        
        return email == null ? null : MapToEmailDto(email);
    }

    public async Task<EmailAddressDto?> FindEmailByAddressAsync(string email)
    {
        var emailEntity = await _context.EmailAddresses
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && !e.IsDeleted);
        
        return emailEntity == null ? null : MapToEmailDto(emailEntity);
    }

    public async Task<EmailAddressDto> CreateEmailAddressAsync(CreateEmailAddressDto dto, int? createdByUserId = null)
    {
        // Check if email already exists
        var existing = await _context.EmailAddresses
            .FirstOrDefaultAsync(e => e.Email.ToLower() == dto.Email.ToLower());

        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.Label = dto.Label;
                existing.DisplayName = dto.DisplayName;
                existing.Notes = dto.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = createdByUserId;
                await _context.SaveChangesAsync();
                return MapToEmailDto(existing);
            }
            return MapToEmailDto(existing);
        }

        var emailEntity = new EmailAddress
        {
            Label = dto.Label,
            Email = dto.Email.ToLower(),
            DisplayName = dto.DisplayName,
            Notes = dto.Notes,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.EmailAddresses.Add(emailEntity);
        await _context.SaveChangesAsync();

        return MapToEmailDto(emailEntity);
    }

    public async Task<LinkedEmailDto> LinkEmailAddressAsync(LinkEmailDto dto, int? createdByUserId = null)
    {
        int emailId;

        if (dto.EmailId.HasValue)
        {
            emailId = dto.EmailId.Value;
        }
        else if (dto.NewEmail != null)
        {
            var newEmail = await CreateEmailAddressAsync(dto.NewEmail, createdByUserId);
            emailId = newEmail.Id;
        }
        else
        {
            throw new ArgumentException("Either EmailId or NewEmail must be provided");
        }

        if (!Enum.TryParse<EntityType>(dto.EntityType, true, out var entityType))
        {
            throw new ArgumentException($"Invalid EntityType: {dto.EntityType}");
        }

        if (!Enum.TryParse<EmailType>(dto.EmailType, true, out var emailType))
        {
            emailType = EmailType.General;
        }

        if (dto.IsPrimary)
        {
            await UnsetPrimaryEmailsAsync(entityType, dto.EntityId);
        }

        var existingLink = await _context.EntityEmailLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == dto.EntityId 
                && l.EmailId == emailId 
                && l.EmailType == emailType);

        if (existingLink != null)
        {
            existingLink.IsPrimary = dto.IsPrimary;
            existingLink.DoNotEmail = dto.DoNotEmail;
            existingLink.MarketingOptIn = dto.MarketingOptIn;
            existingLink.TransactionalOnly = dto.TransactionalOnly;
            existingLink.ValidFrom = dto.ValidFrom;
            existingLink.ValidTo = dto.ValidTo;
            existingLink.Notes = dto.Notes;
            existingLink.IsDeleted = false;
        }
        else
        {
            existingLink = new EntityEmailLink
            {
                EmailId = emailId,
                EntityType = entityType,
                EntityId = dto.EntityId,
                EmailType = emailType,
                IsPrimary = dto.IsPrimary,
                DoNotEmail = dto.DoNotEmail,
                MarketingOptIn = dto.MarketingOptIn,
                TransactionalOnly = dto.TransactionalOnly,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.EntityEmailLinks.Add(existingLink);
        }

        await _context.SaveChangesAsync();

        var link = await _context.EntityEmailLinks
            .Include(l => l.EmailAddress)
            .FirstAsync(l => l.Id == existingLink.Id);

        return MapToLinkedEmailDto(link);
    }

    public async Task<EmailAddressDto> UpdateEmailAddressAsync(int emailId, CreateEmailAddressDto dto, int? updatedByUserId = null)
    {
        var email = await _context.EmailAddresses.FindAsync(emailId)
            ?? throw new KeyNotFoundException($"Email {emailId} not found");

        email.Label = dto.Label;
        email.Email = dto.Email.ToLower();
        email.DisplayName = dto.DisplayName;
        email.Notes = dto.Notes;
        email.UpdatedBy = updatedByUserId;
        email.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToEmailDto(email);
    }

    public async Task UnlinkEmailAddressAsync(int linkId)
    {
        var link = await _context.EntityEmailLinks.FindAsync(linkId);
        if (link != null)
        {
            link.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteEmailAddressAsync(int emailId)
    {
        var email = await _context.EmailAddresses.FindAsync(emailId);
        if (email != null)
        {
            email.IsDeleted = true;
            email.UpdatedAt = DateTime.UtcNow;

            var links = await _context.EntityEmailLinks
                .Where(l => l.EmailId == emailId)
                .ToListAsync();
            
            foreach (var link in links)
            {
                link.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task SetPrimaryEmailAsync(EntityType entityType, int entityId, int emailId, EmailType emailType)
    {
        await UnsetPrimaryEmailsAsync(entityType, entityId);

        var link = await _context.EntityEmailLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == entityId 
                && l.EmailId == emailId 
                && !l.IsDeleted);

        if (link != null)
        {
            link.IsPrimary = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateEmailPreferencesAsync(int linkId, bool doNotEmail, bool marketingOptIn, bool transactionalOnly)
    {
        var link = await _context.EntityEmailLinks.FindAsync(linkId);
        if (link != null)
        {
            link.DoNotEmail = doNotEmail;
            link.MarketingOptIn = marketingOptIn;
            link.TransactionalOnly = transactionalOnly;
            if (doNotEmail && !link.UnsubscribedDate.HasValue)
            {
                link.UnsubscribedDate = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }
    }

    private async Task UnsetPrimaryEmailsAsync(EntityType entityType, int entityId)
    {
        var primaryLinks = await _context.EntityEmailLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && l.IsPrimary && !l.IsDeleted)
            .ToListAsync();

        foreach (var link in primaryLinks)
        {
            link.IsPrimary = false;
        }
    }

    #endregion

    #region Social Media Account Operations

    public async Task<List<LinkedSocialMediaDto>> GetSocialMediaAccountsAsync(EntityType entityType, int entityId)
    {
        var links = await _context.EntitySocialMediaLinks
            .Include(l => l.SocialMediaAccount)
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && !l.IsDeleted)
            .Where(l => l.SocialMediaAccount != null && !l.SocialMediaAccount.IsDeleted)
            .OrderByDescending(l => l.IsPrimary)
            .ThenBy(l => l.SocialMediaAccount!.Platform)
            .ToListAsync();

        return links.Select(l => MapToLinkedSocialMediaDto(l)).ToList();
    }

    public async Task<SocialMediaAccountDto?> GetSocialMediaAccountByIdAsync(int socialMediaId)
    {
        var account = await _context.SocialMediaAccounts
            .FirstOrDefaultAsync(s => s.Id == socialMediaId && !s.IsDeleted);
        
        return account == null ? null : MapToSocialMediaDto(account);
    }

    public async Task<SocialMediaAccountDto> CreateSocialMediaAccountAsync(CreateSocialMediaAccountDto dto, int? createdByUserId = null)
    {
        if (!Enum.TryParse<SocialMediaPlatform>(dto.Platform, true, out var platform))
        {
            platform = SocialMediaPlatform.Other;
        }

        if (!Enum.TryParse<SocialMediaAccountType>(dto.AccountType, true, out var accountType))
        {
            accountType = SocialMediaAccountType.Personal;
        }

        var account = new SocialMediaAccount
        {
            Platform = platform,
            PlatformOther = dto.PlatformOther,
            AccountType = accountType,
            HandleOrUsername = dto.HandleOrUsername,
            ProfileUrl = dto.ProfileUrl,
            DisplayName = dto.DisplayName,
            FollowerCount = dto.FollowerCount,
            FollowingCount = dto.FollowingCount,
            IsVerifiedAccount = dto.IsVerifiedAccount,
            Notes = dto.Notes,
            CreatedBy = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SocialMediaAccounts.Add(account);
        await _context.SaveChangesAsync();

        return MapToSocialMediaDto(account);
    }

    public async Task<LinkedSocialMediaDto> LinkSocialMediaAccountAsync(LinkSocialMediaDto dto, int? createdByUserId = null)
    {
        int socialMediaId;

        if (dto.SocialMediaAccountId.HasValue)
        {
            socialMediaId = dto.SocialMediaAccountId.Value;
        }
        else if (dto.NewSocialMedia != null)
        {
            var newAccount = await CreateSocialMediaAccountAsync(dto.NewSocialMedia, createdByUserId);
            socialMediaId = newAccount.Id;
        }
        else
        {
            throw new ArgumentException("Either SocialMediaAccountId or NewSocialMedia must be provided");
        }

        if (!Enum.TryParse<EntityType>(dto.EntityType, true, out var entityType))
        {
            throw new ArgumentException($"Invalid EntityType: {dto.EntityType}");
        }

        if (dto.IsPrimary)
        {
            await UnsetPrimarySocialMediaAsync(entityType, dto.EntityId);
        }

        var existingLink = await _context.EntitySocialMediaLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == dto.EntityId 
                && l.SocialMediaAccountId == socialMediaId);

        if (existingLink != null)
        {
            existingLink.IsPrimary = dto.IsPrimary;
            existingLink.PreferredForContact = dto.PreferredForContact;
            existingLink.Notes = dto.Notes;
            existingLink.IsDeleted = false;
        }
        else
        {
            existingLink = new EntitySocialMediaLink
            {
                SocialMediaAccountId = socialMediaId,
                EntityType = entityType,
                EntityId = dto.EntityId,
                IsPrimary = dto.IsPrimary,
                PreferredForContact = dto.PreferredForContact,
                Notes = dto.Notes,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };
            _context.EntitySocialMediaLinks.Add(existingLink);
        }

        await _context.SaveChangesAsync();

        var link = await _context.EntitySocialMediaLinks
            .Include(l => l.SocialMediaAccount)
            .FirstAsync(l => l.Id == existingLink.Id);

        return MapToLinkedSocialMediaDto(link);
    }

    public async Task<SocialMediaAccountDto> UpdateSocialMediaAccountAsync(int socialMediaId, CreateSocialMediaAccountDto dto, int? updatedByUserId = null)
    {
        var account = await _context.SocialMediaAccounts.FindAsync(socialMediaId)
            ?? throw new KeyNotFoundException($"Social media account {socialMediaId} not found");

        if (Enum.TryParse<SocialMediaPlatform>(dto.Platform, true, out var platform))
        {
            account.Platform = platform;
        }

        if (Enum.TryParse<SocialMediaAccountType>(dto.AccountType, true, out var accountType))
        {
            account.AccountType = accountType;
        }

        account.PlatformOther = dto.PlatformOther;
        account.HandleOrUsername = dto.HandleOrUsername;
        account.ProfileUrl = dto.ProfileUrl;
        account.DisplayName = dto.DisplayName;
        account.FollowerCount = dto.FollowerCount;
        account.FollowingCount = dto.FollowingCount;
        account.IsVerifiedAccount = dto.IsVerifiedAccount;
        account.Notes = dto.Notes;
        account.UpdatedBy = updatedByUserId;
        account.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToSocialMediaDto(account);
    }

    public async Task UnlinkSocialMediaAccountAsync(int linkId)
    {
        var link = await _context.EntitySocialMediaLinks.FindAsync(linkId);
        if (link != null)
        {
            link.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteSocialMediaAccountAsync(int socialMediaId)
    {
        var account = await _context.SocialMediaAccounts.FindAsync(socialMediaId);
        if (account != null)
        {
            account.IsDeleted = true;
            account.UpdatedAt = DateTime.UtcNow;

            var links = await _context.EntitySocialMediaLinks
                .Where(l => l.SocialMediaAccountId == socialMediaId)
                .ToListAsync();
            
            foreach (var link in links)
            {
                link.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task SetPrimarySocialMediaAsync(EntityType entityType, int entityId, int socialMediaId)
    {
        await UnsetPrimarySocialMediaAsync(entityType, entityId);

        var link = await _context.EntitySocialMediaLinks
            .FirstOrDefaultAsync(l => l.EntityType == entityType 
                && l.EntityId == entityId 
                && l.SocialMediaAccountId == socialMediaId 
                && !l.IsDeleted);

        if (link != null)
        {
            link.IsPrimary = true;
            await _context.SaveChangesAsync();
        }
    }

    private async Task UnsetPrimarySocialMediaAsync(EntityType entityType, int entityId)
    {
        var primaryLinks = await _context.EntitySocialMediaLinks
            .Where(l => l.EntityType == entityType && l.EntityId == entityId && l.IsPrimary && !l.IsDeleted)
            .ToListAsync();

        foreach (var link in primaryLinks)
        {
            link.IsPrimary = false;
        }
    }

    #endregion

    #region Aggregate Operations

    public async Task<EntityContactInfoDto> GetEntityContactInfoAsync(EntityType entityType, int entityId)
    {
        var result = new EntityContactInfoDto
        {
            EntityType = entityType.ToString(),
            EntityId = entityId,
            Addresses = await GetAddressesAsync(entityType, entityId),
            PhoneNumbers = await GetPhoneNumbersAsync(entityType, entityId),
            EmailAddresses = await GetEmailAddressesAsync(entityType, entityId),
            SocialMediaAccounts = await GetSocialMediaAccountsAsync(entityType, entityId)
        };

        // Get entity name
        result.EntityName = entityType switch
        {
            EntityType.Customer => (await _context.Customers.FindAsync(entityId))?.DisplayName,
            EntityType.Contact => await _context.Contacts.Where(c => c.Id == entityId)
                .Select(c => c.FirstName + " " + c.LastName).FirstOrDefaultAsync(),
            EntityType.Lead => (await _context.Leads.FindAsync(entityId))?.FullName,
            EntityType.Account => (await _context.Accounts.FindAsync(entityId))?.AccountNumber,
            _ => null
        };

        return result;
    }

    public async Task ShareContactInfoAsync(ShareContactInfoDto dto, int? createdByUserId = null)
    {
        if (!Enum.TryParse<EntityType>(dto.TargetEntityType, true, out var targetEntityType))
        {
            throw new ArgumentException($"Invalid TargetEntityType: {dto.TargetEntityType}");
        }

        // Share addresses
        if (dto.AddressIds?.Any() == true)
        {
            foreach (var addressId in dto.AddressIds)
            {
                await LinkAddressAsync(new LinkAddressDto
                {
                    AddressId = addressId,
                    EntityType = dto.TargetEntityType,
                    EntityId = dto.TargetEntityId,
                    AddressType = dto.DefaultAddressType,
                    IsPrimary = dto.SetAsPrimary
                }, createdByUserId);
            }
        }

        // Share phones
        if (dto.PhoneIds?.Any() == true)
        {
            foreach (var phoneId in dto.PhoneIds)
            {
                await LinkPhoneNumberAsync(new LinkPhoneDto
                {
                    PhoneId = phoneId,
                    EntityType = dto.TargetEntityType,
                    EntityId = dto.TargetEntityId,
                    PhoneType = dto.DefaultPhoneType,
                    IsPrimary = dto.SetAsPrimary
                }, createdByUserId);
            }
        }

        // Share emails
        if (dto.EmailIds?.Any() == true)
        {
            foreach (var emailId in dto.EmailIds)
            {
                await LinkEmailAddressAsync(new LinkEmailDto
                {
                    EmailId = emailId,
                    EntityType = dto.TargetEntityType,
                    EntityId = dto.TargetEntityId,
                    EmailType = dto.DefaultEmailType,
                    IsPrimary = dto.SetAsPrimary
                }, createdByUserId);
            }
        }

        // Share social media
        if (dto.SocialMediaIds?.Any() == true)
        {
            foreach (var socialId in dto.SocialMediaIds)
            {
                await LinkSocialMediaAccountAsync(new LinkSocialMediaDto
                {
                    SocialMediaAccountId = socialId,
                    EntityType = dto.TargetEntityType,
                    EntityId = dto.TargetEntityId,
                    IsPrimary = dto.SetAsPrimary
                }, createdByUserId);
            }
        }
    }

    public async Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingAddressAsync(int addressId)
    {
        var links = await _context.EntityAddressLinks
            .Where(l => l.AddressId == addressId && !l.IsDeleted)
            .ToListAsync();

        var results = new List<(EntityType, int, string)>();
        foreach (var link in links)
        {
            var name = await GetEntityNameAsync(link.EntityType, link.EntityId);
            results.Add((link.EntityType, link.EntityId, name));
        }
        return results;
    }

    public async Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingPhoneAsync(int phoneId)
    {
        var links = await _context.EntityPhoneLinks
            .Where(l => l.PhoneId == phoneId && !l.IsDeleted)
            .ToListAsync();

        var results = new List<(EntityType, int, string)>();
        foreach (var link in links)
        {
            var name = await GetEntityNameAsync(link.EntityType, link.EntityId);
            results.Add((link.EntityType, link.EntityId, name));
        }
        return results;
    }

    public async Task<List<(EntityType EntityType, int EntityId, string EntityName)>> GetEntitiesSharingEmailAsync(int emailId)
    {
        var links = await _context.EntityEmailLinks
            .Where(l => l.EmailId == emailId && !l.IsDeleted)
            .ToListAsync();

        var results = new List<(EntityType, int, string)>();
        foreach (var link in links)
        {
            var name = await GetEntityNameAsync(link.EntityType, link.EntityId);
            results.Add((link.EntityType, link.EntityId, name));
        }
        return results;
    }

    private async Task<string> GetEntityNameAsync(EntityType entityType, int entityId)
    {
        return entityType switch
        {
            EntityType.Customer => (await _context.Customers.FindAsync(entityId))?.DisplayName ?? $"Customer #{entityId}",
            EntityType.Contact => await _context.Contacts.Where(c => c.Id == entityId)
                .Select(c => c.FirstName + " " + c.LastName).FirstOrDefaultAsync() ?? $"Contact #{entityId}",
            EntityType.Lead => (await _context.Leads.FindAsync(entityId))?.FullName ?? $"Lead #{entityId}",
            EntityType.Account => (await _context.Accounts.FindAsync(entityId))?.AccountNumber ?? $"Account #{entityId}",
            _ => $"Entity #{entityId}"
        };
    }

    #endregion

    #region Mapping Methods

    private static AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto
        {
            Id = address.Id,
            Label = address.Label,
            Line1 = address.Line1,
            Line2 = address.Line2,
            Line3 = address.Line3,
            City = address.City,
            State = address.State,
            PostalCode = address.PostalCode,
            County = address.County,
            CountryCode = address.CountryCode,
            Country = address.Country,
            Latitude = address.Latitude,
            Longitude = address.Longitude,
            GeocodeAccuracy = address.GeocodeAccuracy,
            IsVerified = address.IsVerified,
            VerifiedDate = address.VerifiedDate,
            VerificationSource = address.VerificationSource,
            IsResidential = address.IsResidential,
            DeliveryInstructions = address.DeliveryInstructions,
            AccessHours = address.AccessHours,
            SiteContactName = address.SiteContactName,
            SiteContactPhone = address.SiteContactPhone,
            Notes = address.Notes,
            FormattedAddress = address.FormattedAddress,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    private static LinkedAddressDto MapToLinkedAddressDto(EntityAddressLink link)
    {
        var dto = new LinkedAddressDto
        {
            LinkId = link.Id,
            AddressType = link.AddressType.ToString(),
            IsPrimary = link.IsPrimary,
            ValidFrom = link.ValidFrom,
            ValidTo = link.ValidTo,
            IsActive = link.IsActive,
            LinkNotes = link.Notes
        };

        if (link.Address != null)
        {
            dto.Id = link.Address.Id;
            dto.Label = link.Address.Label;
            dto.Line1 = link.Address.Line1;
            dto.Line2 = link.Address.Line2;
            dto.Line3 = link.Address.Line3;
            dto.City = link.Address.City;
            dto.State = link.Address.State;
            dto.PostalCode = link.Address.PostalCode;
            dto.County = link.Address.County;
            dto.CountryCode = link.Address.CountryCode;
            dto.Country = link.Address.Country;
            dto.Latitude = link.Address.Latitude;
            dto.Longitude = link.Address.Longitude;
            dto.GeocodeAccuracy = link.Address.GeocodeAccuracy;
            dto.IsVerified = link.Address.IsVerified;
            dto.VerifiedDate = link.Address.VerifiedDate;
            dto.VerificationSource = link.Address.VerificationSource;
            dto.IsResidential = link.Address.IsResidential;
            dto.DeliveryInstructions = link.Address.DeliveryInstructions;
            dto.AccessHours = link.Address.AccessHours;
            dto.SiteContactName = link.Address.SiteContactName;
            dto.SiteContactPhone = link.Address.SiteContactPhone;
            dto.Notes = link.Address.Notes;
            dto.FormattedAddress = link.Address.FormattedAddress;
            dto.CreatedAt = link.Address.CreatedAt;
            dto.UpdatedAt = link.Address.UpdatedAt;
        }

        return dto;
    }

    private static PhoneNumberDto MapToPhoneDto(PhoneNumber phone)
    {
        return new PhoneNumberDto
        {
            Id = phone.Id,
            Label = phone.Label,
            CountryCode = phone.CountryCode,
            AreaCode = phone.AreaCode,
            Number = phone.Number,
            Extension = phone.Extension,
            FormattedNumber = phone.FormattedNumber,
            CanSMS = phone.CanSMS,
            CanWhatsApp = phone.CanWhatsApp,
            CanFax = phone.CanFax,
            IsVerified = phone.IsVerified,
            VerifiedDate = phone.VerifiedDate,
            BestTimeToCall = phone.BestTimeToCall,
            Notes = phone.Notes,
            FullNumber = phone.FullNumber,
            CreatedAt = phone.CreatedAt,
            UpdatedAt = phone.UpdatedAt
        };
    }

    private static LinkedPhoneDto MapToLinkedPhoneDto(EntityPhoneLink link)
    {
        var dto = new LinkedPhoneDto
        {
            LinkId = link.Id,
            PhoneType = link.PhoneType.ToString(),
            IsPrimary = link.IsPrimary,
            DoNotCall = link.DoNotCall,
            ValidFrom = link.ValidFrom,
            ValidTo = link.ValidTo,
            IsActive = link.IsActive,
            LinkNotes = link.Notes
        };

        if (link.PhoneNumber != null)
        {
            dto.Id = link.PhoneNumber.Id;
            dto.Label = link.PhoneNumber.Label;
            dto.CountryCode = link.PhoneNumber.CountryCode;
            dto.AreaCode = link.PhoneNumber.AreaCode;
            dto.Number = link.PhoneNumber.Number;
            dto.Extension = link.PhoneNumber.Extension;
            dto.FormattedNumber = link.PhoneNumber.FormattedNumber;
            dto.CanSMS = link.PhoneNumber.CanSMS;
            dto.CanWhatsApp = link.PhoneNumber.CanWhatsApp;
            dto.CanFax = link.PhoneNumber.CanFax;
            dto.IsVerified = link.PhoneNumber.IsVerified;
            dto.VerifiedDate = link.PhoneNumber.VerifiedDate;
            dto.BestTimeToCall = link.PhoneNumber.BestTimeToCall;
            dto.Notes = link.PhoneNumber.Notes;
            dto.FullNumber = link.PhoneNumber.FullNumber;
            dto.CreatedAt = link.PhoneNumber.CreatedAt;
            dto.UpdatedAt = link.PhoneNumber.UpdatedAt;
        }

        return dto;
    }

    private static EmailAddressDto MapToEmailDto(EmailAddress email)
    {
        return new EmailAddressDto
        {
            Id = email.Id,
            Label = email.Label,
            Email = email.Email,
            DisplayName = email.DisplayName,
            IsVerified = email.IsVerified,
            VerifiedDate = email.VerifiedDate,
            BounceCount = email.BounceCount,
            LastBounceDate = email.LastBounceDate,
            HardBounce = email.HardBounce,
            LastEmailSent = email.LastEmailSent,
            LastEmailOpened = email.LastEmailOpened,
            EmailEngagementScore = email.EmailEngagementScore,
            IsDeliverable = email.IsDeliverable,
            Notes = email.Notes,
            CreatedAt = email.CreatedAt,
            UpdatedAt = email.UpdatedAt
        };
    }

    private static LinkedEmailDto MapToLinkedEmailDto(EntityEmailLink link)
    {
        var dto = new LinkedEmailDto
        {
            LinkId = link.Id,
            EmailType = link.EmailType.ToString(),
            IsPrimary = link.IsPrimary,
            DoNotEmail = link.DoNotEmail,
            UnsubscribedDate = link.UnsubscribedDate,
            MarketingOptIn = link.MarketingOptIn,
            TransactionalOnly = link.TransactionalOnly,
            CanSendMarketing = link.CanSendMarketing,
            ValidFrom = link.ValidFrom,
            ValidTo = link.ValidTo,
            IsActive = link.IsActive,
            LinkNotes = link.Notes
        };

        if (link.EmailAddress != null)
        {
            dto.Id = link.EmailAddress.Id;
            dto.Label = link.EmailAddress.Label;
            dto.Email = link.EmailAddress.Email;
            dto.DisplayName = link.EmailAddress.DisplayName;
            dto.IsVerified = link.EmailAddress.IsVerified;
            dto.VerifiedDate = link.EmailAddress.VerifiedDate;
            dto.BounceCount = link.EmailAddress.BounceCount;
            dto.LastBounceDate = link.EmailAddress.LastBounceDate;
            dto.HardBounce = link.EmailAddress.HardBounce;
            dto.LastEmailSent = link.EmailAddress.LastEmailSent;
            dto.LastEmailOpened = link.EmailAddress.LastEmailOpened;
            dto.EmailEngagementScore = link.EmailAddress.EmailEngagementScore;
            dto.IsDeliverable = link.EmailAddress.IsDeliverable;
            dto.Notes = link.EmailAddress.Notes;
            dto.CreatedAt = link.EmailAddress.CreatedAt;
            dto.UpdatedAt = link.EmailAddress.UpdatedAt;
        }

        return dto;
    }

    private static SocialMediaAccountDto MapToSocialMediaDto(SocialMediaAccount account)
    {
        return new SocialMediaAccountDto
        {
            Id = account.Id,
            Platform = account.Platform.ToString(),
            PlatformOther = account.PlatformOther,
            AccountType = account.AccountType.ToString(),
            HandleOrUsername = account.HandleOrUsername,
            ProfileUrl = account.ProfileUrl,
            DisplayName = account.DisplayName,
            FollowerCount = account.FollowerCount,
            FollowingCount = account.FollowingCount,
            IsVerifiedAccount = account.IsVerifiedAccount,
            IsActive = account.IsActive,
            LastActivityDate = account.LastActivityDate,
            EngagementLevel = account.EngagementLevel?.ToString(),
            PlatformName = account.PlatformName,
            Notes = account.Notes,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }

    private static LinkedSocialMediaDto MapToLinkedSocialMediaDto(EntitySocialMediaLink link)
    {
        var dto = new LinkedSocialMediaDto
        {
            LinkId = link.Id,
            IsPrimary = link.IsPrimary,
            PreferredForContact = link.PreferredForContact,
            LinkNotes = link.Notes
        };

        if (link.SocialMediaAccount != null)
        {
            dto.Id = link.SocialMediaAccount.Id;
            dto.Platform = link.SocialMediaAccount.Platform.ToString();
            dto.PlatformOther = link.SocialMediaAccount.PlatformOther;
            dto.AccountType = link.SocialMediaAccount.AccountType.ToString();
            dto.HandleOrUsername = link.SocialMediaAccount.HandleOrUsername;
            dto.ProfileUrl = link.SocialMediaAccount.ProfileUrl;
            dto.DisplayName = link.SocialMediaAccount.DisplayName;
            dto.FollowerCount = link.SocialMediaAccount.FollowerCount;
            dto.FollowingCount = link.SocialMediaAccount.FollowingCount;
            dto.IsVerifiedAccount = link.SocialMediaAccount.IsVerifiedAccount;
            dto.IsActive = link.SocialMediaAccount.IsActive;
            dto.LastActivityDate = link.SocialMediaAccount.LastActivityDate;
            dto.EngagementLevel = link.SocialMediaAccount.EngagementLevel?.ToString();
            dto.PlatformName = link.SocialMediaAccount.PlatformName;
            dto.Notes = link.SocialMediaAccount.Notes;
            dto.CreatedAt = link.SocialMediaAccount.CreatedAt;
            dto.UpdatedAt = link.SocialMediaAccount.UpdatedAt;
        }

        return dto;
    }

    #endregion
}
