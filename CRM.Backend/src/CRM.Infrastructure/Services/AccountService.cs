using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly IRepository<Account> _accountRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly IRepository<ContactDetail> _contactDetailRepo;
    private readonly IRepository<SocialAccount> _socialAccountRepo;
    private readonly IRepository<ContactInfoLink> _linkRepo;

    public AccountService(
        IRepository<Account> accountRepo,
        IRepository<Address> addressRepo,
        IRepository<ContactDetail> contactDetailRepo,
        IRepository<SocialAccount> socialAccountRepo,
        IRepository<ContactInfoLink> linkRepo)
    {
        _accountRepo = accountRepo;
        _addressRepo = addressRepo;
        _contactDetailRepo = contactDetailRepo;
        _socialAccountRepo = socialAccountRepo;
        _linkRepo = linkRepo;
    }

    public async Task<object?> GetByIdAsync(int id)
    {
        var acc = await _accountRepo.GetByIdAsync(id);
        if (acc == null || acc.IsDeleted) return null;
        return new { acc.Id, acc.AccountNumber, acc.AccountOwner, acc.CreatedAt, acc.UpdatedAt };
    }

    public async Task<object> CreateAsync(CreateAccountRequest request, string modifiedBy)
    {
        var account = new Account
            {
                AccountNumber = request.Name ?? string.Empty,
                AccountOwner = request.Name,
                BillingContactEmail = request.Email,
                BillingContactPhone = request.Phone,
                CreatedAt = DateTime.UtcNow
            };

        await _accountRepo.AddAsync(account);
        await _accountRepo.SaveAsync();

        // Materialize inline info
        if (!string.IsNullOrWhiteSpace(request.Address) || !string.IsNullOrWhiteSpace(request.City) || !string.IsNullOrWhiteSpace(request.Country))
        {
            var addr = new Address
            {
                Label = "Primary",
                Line1 = request.Address ?? string.Empty,
                Line2 = request.Address2,
                City = request.City ?? string.Empty,
                State = request.State,
                PostalCode = request.ZipCode,
                Country = request.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _addressRepo.AddAsync(addr);
            await _addressRepo.SaveAsync();

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
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = request.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepo.AddAsync(email);
            await _contactDetailRepo.SaveAsync();

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
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = request.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepo.AddAsync(phone);
            await _contactDetailRepo.SaveAsync();

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
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        return new { account.Id };
    }

    public async Task<object?> UpdateAsync(int id, UpdateAccountRequest request, string modifiedBy)
    {
        var acc = await _accountRepo.GetByIdAsync(id);
        if (acc == null || acc.IsDeleted) return null;

            if (request.Name != null) acc.AccountNumber = request.Name;
            if (request.Name != null) acc.AccountOwner = request.Name;
            if (request.Email != null) acc.BillingContactEmail = request.Email;
            if (request.Phone != null) acc.BillingContactPhone = request.Phone;
        acc.UpdatedAt = DateTime.UtcNow;

        await _accountRepo.UpdateAsync(acc);
        await _accountRepo.SaveAsync();

        // Materialize updated inline info
        if (request.Address != null || request.City != null || request.Country != null)
        {
            var existingAddrLinks = (await _linkRepo.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == acc.Id && l.InfoKind == ContactInfoKind.Address && l.IsPrimaryForOwner && !l.IsDeleted)).ToList();
            foreach (var l in existingAddrLinks)
            {
                l.IsPrimaryForOwner = false;
                await _linkRepo.UpdateAsync(l);
            }

            var addr = new Address
            {
                Label = "Primary",
                Line1 = request.Address ?? string.Empty,
                Line2 = request.Address2,
                City = request.City ?? string.Empty,
                State = request.State,
                PostalCode = request.ZipCode,
                Country = request.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _addressRepo.AddAsync(addr);
            await _addressRepo.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = acc.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        if (request.Email != null)
        {
            var existingEmailLinks = (await _linkRepo.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == acc.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted)).ToList();
            foreach (var l in existingEmailLinks)
            {
                l.IsPrimaryForOwner = false;
                await _linkRepo.UpdateAsync(l);
            }

            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = request.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepo.AddAsync(email);
            await _contactDetailRepo.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = acc.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        if (request.Phone != null)
        {
            var existingPhoneLinks = (await _linkRepo.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Account && l.OwnerId == acc.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted)).ToList();
            foreach (var l in existingPhoneLinks)
            {
                l.IsPrimaryForOwner = false;
                await _linkRepo.UpdateAsync(l);
            }

            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = request.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepo.AddAsync(phone);
            await _contactDetailRepo.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = acc.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _linkRepo.AddAsync(link);
            await _linkRepo.SaveAsync();
        }

        return new { acc.Id };
    }
}
