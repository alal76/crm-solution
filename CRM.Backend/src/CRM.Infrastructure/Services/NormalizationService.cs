using CRM.Core.Interfaces;
using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

public class NormalizationService : INormalizationService
{
    private readonly ICrmDbContext _context;

    public NormalizationService(ICrmDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetTagsAsync(string entityType, int entityId)
    {
        var tags = await _context.EntityTags
            .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDeleted)
            .Select(t => t.Tag)
            .ToListAsync();

        if (tags == null || !tags.Any()) return null;
        return string.Join(",", tags.Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    public async Task<string?> GetCustomFieldsAsync(string entityType, int entityId)
    {
        var fields = await _context.CustomFields
            .Where(cf => cf.EntityType == entityType && cf.EntityId == entityId && !cf.IsDeleted)
            .ToListAsync();

        if (fields == null || !fields.Any()) return null;
        return string.Join(";", fields.Select(cf => $"{cf.Key}={cf.Value}"));
    }

    public async Task<string?> GetPrimaryContactDetailValueAsync(ContactInfoOwnerType ownerType, int ownerId, ContactDetailType detailType)
    {
        var link = await _context.ContactInfoLinks
            .Include(l => l.ContactDetail)
            .Where(l => l.OwnerType == ownerType && l.OwnerId == ownerId && l.InfoKind == ContactInfoKind.ContactDetail && l.ContactDetail != null && !l.IsDeleted)
            .OrderByDescending(l => l.IsPrimaryForOwner)
            .FirstOrDefaultAsync();

        if (link == null || link.ContactDetail == null) return null;
        if (link.ContactDetail.DetailType != detailType) return null;
        return string.IsNullOrWhiteSpace(link.ContactDetail.Value) ? null : link.ContactDetail.Value;
    }

    public Task<string?> GetPrimaryEmailAsync(ContactInfoOwnerType ownerType, int ownerId)
        => GetPrimaryContactDetailValueAsync(ownerType, ownerId, ContactDetailType.Email);

    public Task<string?> GetPrimaryPhoneAsync(ContactInfoOwnerType ownerType, int ownerId)
        => GetPrimaryContactDetailValueAsync(ownerType, ownerId, ContactDetailType.Phone);

    public Task<string?> GetPrimaryFaxAsync(ContactInfoOwnerType ownerType, int ownerId)
        => GetPrimaryContactDetailValueAsync(ownerType, ownerId, ContactDetailType.Fax);

    public async Task<Address?> GetPrimaryAddressAsync(ContactInfoOwnerType ownerType, int ownerId)
    {
        var link = await _context.ContactInfoLinks
            .Include(l => l.Address)
            .Where(l => l.OwnerType == ownerType && l.OwnerId == ownerId && l.InfoKind == ContactInfoKind.Address && l.Address != null && !l.IsDeleted)
            .OrderByDescending(l => l.IsPrimaryForOwner)
            .FirstOrDefaultAsync();

        return link?.Address;
    }

    public async Task<string?> GetPrimarySocialAccountAsync(ContactInfoOwnerType ownerType, int ownerId, SocialNetwork network)
    {
        var link = await _context.ContactInfoLinks
            .Include(l => l.SocialAccount)
            .Where(l => l.OwnerType == ownerType && l.OwnerId == ownerId && l.InfoKind == ContactInfoKind.SocialAccount && l.SocialAccount != null && !l.IsDeleted)
            .OrderByDescending(l => l.IsPrimaryForOwner)
            .FirstOrDefaultAsync();

        if (link == null || link.SocialAccount == null) return null;
        if (link.SocialAccount.Network != network) return null;
        return string.IsNullOrWhiteSpace(link.SocialAccount.HandleOrUrl) ? null : link.SocialAccount.HandleOrUrl;
    }
}
