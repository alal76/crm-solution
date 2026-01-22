using CRM.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

public class NormalizationService
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
}
