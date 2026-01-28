// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under the GNU Affero General Public License v3.0

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing account relationships
/// </summary>
public class RelationshipService
{
    private readonly CrmDbContext _context;
    private readonly ILogger<RelationshipService> _logger;

    public RelationshipService(CrmDbContext context, ILogger<RelationshipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Relationship Types

    /// <summary>
    /// Get all relationship types
    /// </summary>
    public async Task<List<RelationshipTypeDto>> GetRelationshipTypesAsync(bool includeInactive = false)
    {
        var query = _context.RelationshipTypes.Where(t => !t.IsDeleted);
        
        if (!includeInactive)
            query = query.Where(t => t.IsActive);

        var types = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.TypeName)
            .ToListAsync();

        return types.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Get a relationship type by ID
    /// </summary>
    public async Task<RelationshipTypeDto?> GetRelationshipTypeAsync(int id)
    {
        var type = await _context.RelationshipTypes
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        return type != null ? MapToDto(type) : null;
    }

    /// <summary>
    /// Create a new relationship type
    /// </summary>
    public async Task<RelationshipTypeDto> CreateRelationshipTypeAsync(RelationshipTypeCreateDto dto, int? userId = null)
    {
        var type = new RelationshipType
        {
            TypeName = dto.TypeName,
            TypeCategory = dto.TypeCategory,
            Description = dto.Description,
            IsBidirectional = dto.IsBidirectional,
            ReverseTypeName = dto.ReverseTypeName,
            Icon = dto.Icon,
            Color = dto.Color,
            IsActive = dto.IsActive,
            DisplayOrder = dto.DisplayOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.RelationshipTypes.Add(type);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created relationship type {TypeName} with ID {Id}", type.TypeName, type.Id);
        return MapToDto(type);
    }

    /// <summary>
    /// Update a relationship type
    /// </summary>
    public async Task<RelationshipTypeDto?> UpdateRelationshipTypeAsync(int id, RelationshipTypeCreateDto dto)
    {
        var type = await _context.RelationshipTypes.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (type == null) return null;

        if (type.IsSystem)
        {
            _logger.LogWarning("Cannot modify system relationship type {Id}", id);
            throw new InvalidOperationException("Cannot modify system relationship types");
        }

        type.TypeName = dto.TypeName;
        type.TypeCategory = dto.TypeCategory;
        type.Description = dto.Description;
        type.IsBidirectional = dto.IsBidirectional;
        type.ReverseTypeName = dto.ReverseTypeName;
        type.Icon = dto.Icon;
        type.Color = dto.Color;
        type.IsActive = dto.IsActive;
        type.DisplayOrder = dto.DisplayOrder;
        type.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(type);
    }

    /// <summary>
    /// Delete a relationship type
    /// </summary>
    public async Task<bool> DeleteRelationshipTypeAsync(int id)
    {
        var type = await _context.RelationshipTypes.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
        if (type == null) return false;

        if (type.IsSystem)
        {
            _logger.LogWarning("Cannot delete system relationship type {Id}", id);
            throw new InvalidOperationException("Cannot delete system relationship types");
        }

        // Check if type is in use
        var inUse = await _context.AccountRelationships.AnyAsync(r => r.RelationshipTypeId == id && !r.IsDeleted);
        if (inUse)
        {
            throw new InvalidOperationException("Cannot delete relationship type that is in use");
        }

        type.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Account Relationships

    /// <summary>
    /// Get relationships for a customer
    /// </summary>
    public async Task<List<AccountRelationshipDto>> GetCustomerRelationshipsAsync(
        int customerId,
        string? status = null,
        int? relationshipTypeId = null)
    {
        var query = _context.AccountRelationships
            .Include(r => r.SourceCustomer)
            .Include(r => r.TargetCustomer)
            .Include(r => r.RelationshipType)
            .Include(r => r.Interactions)
            .Where(r => !r.IsDeleted && (r.SourceCustomerId == customerId || r.TargetCustomerId == customerId));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (relationshipTypeId.HasValue)
            query = query.Where(r => r.RelationshipTypeId == relationshipTypeId.Value);

        var relationships = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return relationships.Select(r => MapToDto(r, customerId)).ToList();
    }

    /// <summary>
    /// Get all relationships with filtering
    /// </summary>
    public async Task<(List<AccountRelationshipDto> Items, int TotalCount)> GetRelationshipsAsync(
        string? search = null,
        string? status = null,
        int? relationshipTypeId = null,
        string? strategicImportance = null,
        int skip = 0,
        int take = 50)
    {
        var query = _context.AccountRelationships
            .Include(r => r.SourceCustomer)
            .Include(r => r.TargetCustomer)
            .Include(r => r.RelationshipType)
            .Include(r => r.Interactions)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);

        if (relationshipTypeId.HasValue)
            query = query.Where(r => r.RelationshipTypeId == relationshipTypeId.Value);

        if (!string.IsNullOrEmpty(strategicImportance))
            query = query.Where(r => r.StrategicImportance == strategicImportance);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(r =>
                (r.SourceCustomer != null && (r.SourceCustomer.Company ?? "").Contains(search)) ||
                (r.TargetCustomer != null && (r.TargetCustomer.Company ?? "").Contains(search)) ||
                (r.Description ?? "").Contains(search));
        }

        var totalCount = await query.CountAsync();
        var relationships = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (relationships.Select(r => MapToDto(r)).ToList(), totalCount);
    }

    /// <summary>
    /// Get a relationship by ID
    /// </summary>
    public async Task<AccountRelationshipDto?> GetRelationshipAsync(int id)
    {
        var relationship = await _context.AccountRelationships
            .Include(r => r.SourceCustomer)
            .Include(r => r.TargetCustomer)
            .Include(r => r.RelationshipType)
            .Include(r => r.Interactions)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return relationship != null ? MapToDto(relationship) : null;
    }

    /// <summary>
    /// Create a new relationship
    /// </summary>
    public async Task<AccountRelationshipDto> CreateRelationshipAsync(AccountRelationshipCreateDto dto, int? userId = null)
    {
        // Validate customers exist
        var sourceExists = await _context.Customers.AnyAsync(c => c.Id == dto.SourceCustomerId && !c.IsDeleted);
        var targetExists = await _context.Customers.AnyAsync(c => c.Id == dto.TargetCustomerId && !c.IsDeleted);
        
        if (!sourceExists || !targetExists)
            throw new InvalidOperationException("Source or target customer does not exist");

        // Check for duplicate
        var exists = await _context.AccountRelationships.AnyAsync(r =>
            !r.IsDeleted &&
            r.SourceCustomerId == dto.SourceCustomerId &&
            r.TargetCustomerId == dto.TargetCustomerId &&
            r.RelationshipTypeId == dto.RelationshipTypeId);

        if (exists)
            throw new InvalidOperationException("This relationship already exists");

        var relationship = new AccountRelationship
        {
            SourceCustomerId = dto.SourceCustomerId,
            TargetCustomerId = dto.TargetCustomerId,
            RelationshipTypeId = dto.RelationshipTypeId,
            Status = dto.Status,
            StrengthScore = dto.StrengthScore,
            StrategicImportance = dto.StrategicImportance,
            RelationshipStartDate = dto.RelationshipStartDate,
            RelationshipEndDate = dto.RelationshipEndDate,
            NextReviewDate = dto.NextReviewDate,
            AnnualRevenueImpact = dto.AnnualRevenueImpact,
            CostSavings = dto.CostSavings,
            Description = dto.Description,
            Notes = dto.Notes,
            TermsConditions = dto.TermsConditions,
            CreatedAt = DateTime.UtcNow
        };

        _context.AccountRelationships.Add(relationship);
        await _context.SaveChangesAsync();

        // Create reverse relationship for bidirectional types
        var relType = await _context.RelationshipTypes.FindAsync(dto.RelationshipTypeId);
        if (relType?.IsBidirectional == true && !string.IsNullOrEmpty(relType.ReverseTypeName))
        {
            var reverseType = await _context.RelationshipTypes
                .FirstOrDefaultAsync(t => t.TypeName == relType.ReverseTypeName && !t.IsDeleted);
                
            if (reverseType != null)
            {
                var reverseRelationship = new AccountRelationship
                {
                    SourceCustomerId = dto.TargetCustomerId,
                    TargetCustomerId = dto.SourceCustomerId,
                    RelationshipTypeId = reverseType.Id,
                    Status = dto.Status,
                    StrengthScore = dto.StrengthScore,
                    StrategicImportance = dto.StrategicImportance,
                    RelationshipStartDate = dto.RelationshipStartDate,
                    RelationshipEndDate = dto.RelationshipEndDate,
                    NextReviewDate = dto.NextReviewDate,
                    AnnualRevenueImpact = dto.AnnualRevenueImpact,
                    CostSavings = dto.CostSavings,
                    Description = dto.Description,
                    Notes = dto.Notes,
                    TermsConditions = dto.TermsConditions,
                    CreatedAt = DateTime.UtcNow
                };

                _context.AccountRelationships.Add(reverseRelationship);
                await _context.SaveChangesAsync();
            }
        }

        _logger.LogInformation("Created relationship between customer {Source} and {Target}", 
            dto.SourceCustomerId, dto.TargetCustomerId);

        return await GetRelationshipAsync(relationship.Id) ?? throw new Exception("Failed to retrieve created relationship");
    }

    /// <summary>
    /// Update a relationship
    /// </summary>
    public async Task<AccountRelationshipDto?> UpdateRelationshipAsync(int id, AccountRelationshipCreateDto dto, int? userId = null)
    {
        var relationship = await _context.AccountRelationships.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (relationship == null) return null;

        relationship.Status = dto.Status;
        relationship.StrengthScore = dto.StrengthScore;
        relationship.StrategicImportance = dto.StrategicImportance;
        relationship.RelationshipStartDate = dto.RelationshipStartDate;
        relationship.RelationshipEndDate = dto.RelationshipEndDate;
        relationship.NextReviewDate = dto.NextReviewDate;
        relationship.AnnualRevenueImpact = dto.AnnualRevenueImpact;
        relationship.CostSavings = dto.CostSavings;
        relationship.Description = dto.Description;
        relationship.Notes = dto.Notes;
        relationship.TermsConditions = dto.TermsConditions;
        relationship.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetRelationshipAsync(id);
    }

    /// <summary>
    /// Delete a relationship
    /// </summary>
    public async Task<bool> DeleteRelationshipAsync(int id)
    {
        var relationship = await _context.AccountRelationships.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (relationship == null) return false;

        relationship.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Relationship Interactions

    /// <summary>
    /// Get interactions for a relationship
    /// </summary>
    public async Task<List<RelationshipInteractionDto>> GetRelationshipInteractionsAsync(int relationshipId)
    {
        var interactions = await _context.RelationshipInteractions
            .Where(i => i.AccountRelationshipId == relationshipId && !i.IsDeleted)
            .OrderByDescending(i => i.InteractionDate)
            .ToListAsync();

        return interactions.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Create a new interaction
    /// </summary>
    public async Task<RelationshipInteractionDto> CreateInteractionAsync(RelationshipInteractionCreateDto dto, int? userId = null)
    {
        var interaction = new RelationshipInteraction
        {
            AccountRelationshipId = dto.AccountRelationshipId,
            InteractionType = dto.InteractionType,
            Subject = dto.Subject,
            Description = dto.Description,
            InteractionDate = dto.InteractionDate,
            DurationMinutes = dto.DurationMinutes,
            ParticipantContactIds = dto.ParticipantContactIds != null ? JsonSerializer.Serialize(dto.ParticipantContactIds) : null,
            ParticipantUserIds = dto.ParticipantUserIds != null ? JsonSerializer.Serialize(dto.ParticipantUserIds) : null,
            Outcome = dto.Outcome,
            ActionItems = dto.ActionItems,
            NextSteps = dto.NextSteps,
            FollowUpDate = dto.FollowUpDate,
            SentimentScore = dto.SentimentScore,
            HealthImpact = dto.HealthImpact,
            Location = dto.Location,
            MeetingLink = dto.MeetingLink,
            CreatedAt = DateTime.UtcNow
        };

        _context.RelationshipInteractions.Add(interaction);
        await _context.SaveChangesAsync();

        // Update last interaction on relationship
        var relationship = await _context.AccountRelationships.FindAsync(dto.AccountRelationshipId);
        if (relationship != null)
        {
            relationship.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return MapToDto(interaction);
    }

    #endregion

    #region Relationship Map

    /// <summary>
    /// Get relationship map visualization data for a customer
    /// </summary>
    public async Task<RelationshipMapVisualizationDto> GetRelationshipMapDataAsync(
        int centralCustomerId,
        int depth = 2,
        List<int>? includeTypeIds = null,
        int minStrength = 0)
    {
        var nodes = new Dictionary<int, RelationshipNodeDto>();
        var edges = new List<RelationshipEdgeDto>();
        var processedRelationships = new HashSet<int>();

        // Start with central customer
        var centralCustomer = await _context.Customers.FindAsync(centralCustomerId);
        if (centralCustomer == null)
            return new RelationshipMapVisualizationDto();

        await BuildMapRecursive(centralCustomerId, depth, nodes, edges, processedRelationships, 
            includeTypeIds, minStrength, true);

        return new RelationshipMapVisualizationDto
        {
            Nodes = nodes.Values.ToList(),
            Edges = edges
        };
    }

    private async Task BuildMapRecursive(
        int customerId,
        int remainingDepth,
        Dictionary<int, RelationshipNodeDto> nodes,
        List<RelationshipEdgeDto> edges,
        HashSet<int> processedRelationships,
        List<int>? includeTypeIds,
        int minStrength,
        bool isCentral)
    {
        if (remainingDepth < 0 || nodes.ContainsKey(customerId))
            return;

        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return;

        // Add node
        nodes[customerId] = new RelationshipNodeDto
        {
            Id = customerId,
            Label = customer.Company ?? $"{customer.FirstName} {customer.LastName}",
            Type = customer.Category.ToString(),
            Industry = customer.Industry,
            HealthScore = customer.CustomerHealthScore,
            IsCentral = isCentral
        };

        if (remainingDepth == 0)
            return;

        // Get relationships
        var relationships = await _context.AccountRelationships
            .Include(r => r.RelationshipType)
            .Where(r => !r.IsDeleted &&
                (r.SourceCustomerId == customerId || r.TargetCustomerId == customerId) &&
                r.StrengthScore >= minStrength)
            .ToListAsync();

        if (includeTypeIds?.Any() == true)
            relationships = relationships.Where(r => includeTypeIds.Contains(r.RelationshipTypeId)).ToList();

        foreach (var rel in relationships)
        {
            if (processedRelationships.Contains(rel.Id))
                continue;

            processedRelationships.Add(rel.Id);

            edges.Add(new RelationshipEdgeDto
            {
                Id = rel.Id,
                SourceId = rel.SourceCustomerId,
                TargetId = rel.TargetCustomerId,
                RelationshipType = rel.RelationshipType?.TypeName ?? "Unknown",
                Color = rel.RelationshipType?.Color,
                StrengthScore = rel.StrengthScore,
                Status = rel.Status
            });

            var nextCustomerId = rel.SourceCustomerId == customerId ? rel.TargetCustomerId : rel.SourceCustomerId;
            await BuildMapRecursive(nextCustomerId, remainingDepth - 1, nodes, edges, 
                processedRelationships, includeTypeIds, minStrength, false);
        }

        // Update relationship count
        if (nodes.TryGetValue(customerId, out var node))
        {
            node.RelationshipCount = relationships.Count;
        }
    }

    #endregion

    #region Account Health

    /// <summary>
    /// Get health snapshots for a customer
    /// </summary>
    public async Task<List<AccountHealthSnapshotDto>> GetHealthSnapshotsAsync(
        int customerId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _context.AccountHealthSnapshots
            .Include(s => s.Customer)
            .Where(s => s.CustomerId == customerId && !s.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(s => s.SnapshotDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SnapshotDate <= endDate.Value);

        var snapshots = await query.OrderByDescending(s => s.SnapshotDate).ToListAsync();
        return snapshots.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Create a health snapshot
    /// </summary>
    public async Task<AccountHealthSnapshotDto> CreateHealthSnapshotAsync(AccountHealthSnapshotCreateDto dto, int? userId = null)
    {
        var snapshotDate = dto.SnapshotDate ?? DateTime.UtcNow.Date;

        // Get previous snapshot for trend calculation
        var previousSnapshot = await _context.AccountHealthSnapshots
            .Where(s => s.CustomerId == dto.CustomerId && s.SnapshotDate < snapshotDate && !s.IsDeleted)
            .OrderByDescending(s => s.SnapshotDate)
            .FirstOrDefaultAsync();

        var snapshot = new AccountHealthSnapshot
        {
            CustomerId = dto.CustomerId,
            SnapshotDate = snapshotDate,
            OverallHealthScore = dto.OverallHealthScore,
            EngagementScore = dto.EngagementScore,
            ProductAdoptionScore = dto.ProductAdoptionScore,
            SupportSatisfactionScore = dto.SupportSatisfactionScore,
            FinancialHealthScore = dto.FinancialHealthScore,
            RelationshipScore = dto.RelationshipScore,
            ActiveUsersCount = dto.ActiveUsersCount,
            FeatureAdoptionRate = dto.FeatureAdoptionRate,
            SupportTicketsCount = dto.SupportTicketsCount,
            SupportTicketsResolved = dto.SupportTicketsResolved,
            AverageResponseTimeHours = dto.AverageResponseTimeHours,
            NPSScore = dto.NPSScore,
            RiskFactors = dto.RiskFactors != null ? JsonSerializer.Serialize(dto.RiskFactors) : null,
            WarningSignals = dto.WarningSignals != null ? JsonSerializer.Serialize(dto.WarningSignals) : null,
            GrowthIndicators = dto.GrowthIndicators != null ? JsonSerializer.Serialize(dto.GrowthIndicators) : null,
            AnalystNotes = dto.AnalystNotes,
            PreviousHealthScore = previousSnapshot?.OverallHealthScore,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate trend
        if (previousSnapshot != null)
        {
            var diff = dto.OverallHealthScore - previousSnapshot.OverallHealthScore;
            snapshot.HealthTrend = diff > 5 ? "Improving" : diff < -5 ? "Declining" : "Stable";
            if (dto.OverallHealthScore < 30) snapshot.HealthTrend = "Critical";
        }

        _context.AccountHealthSnapshots.Add(snapshot);

        // Update customer's current health score
        var customer = await _context.Customers.FindAsync(dto.CustomerId);
        if (customer != null)
        {
            customer.CustomerHealthScore = dto.OverallHealthScore;
            customer.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return MapToDto(snapshot);
    }

    #endregion

    #region Mapping Helpers

    private static RelationshipTypeDto MapToDto(RelationshipType type) => new()
    {
        Id = type.Id,
        TypeName = type.TypeName,
        TypeCategory = type.TypeCategory,
        Description = type.Description,
        IsBidirectional = type.IsBidirectional,
        ReverseTypeName = type.ReverseTypeName,
        Icon = type.Icon,
        Color = type.Color,
        IsActive = type.IsActive,
        IsSystem = type.IsSystem,
        DisplayOrder = type.DisplayOrder,
        CreatedAt = type.CreatedAt
    };

    private AccountRelationshipDto MapToDto(AccountRelationship rel, int? perspectiveCustomerId = null)
    {
        var isReverse = perspectiveCustomerId.HasValue && rel.TargetCustomerId == perspectiveCustomerId;
        
        return new AccountRelationshipDto
        {
            Id = rel.Id,
            SourceCustomerId = rel.SourceCustomerId,
            TargetCustomerId = rel.TargetCustomerId,
            RelationshipTypeId = rel.RelationshipTypeId,
            SourceCustomerName = rel.SourceCustomer?.Company ?? $"{rel.SourceCustomer?.FirstName} {rel.SourceCustomer?.LastName}",
            TargetCustomerName = rel.TargetCustomer?.Company ?? $"{rel.TargetCustomer?.FirstName} {rel.TargetCustomer?.LastName}",
            RelationshipTypeName = isReverse ? rel.RelationshipType?.ReverseTypeName ?? rel.RelationshipType?.TypeName : rel.RelationshipType?.TypeName,
            RelationshipTypeCategory = rel.RelationshipType?.TypeCategory,
            RelationshipTypeColor = rel.RelationshipType?.Color,
            RelationshipTypeIcon = rel.RelationshipType?.Icon,
            Status = rel.Status,
            StrengthScore = rel.StrengthScore,
            StrategicImportance = rel.StrategicImportance,
            RelationshipStartDate = rel.RelationshipStartDate,
            RelationshipEndDate = rel.RelationshipEndDate,
            LastReviewedDate = rel.LastReviewedDate,
            NextReviewDate = rel.NextReviewDate,
            AnnualRevenueImpact = rel.AnnualRevenueImpact,
            CostSavings = rel.CostSavings,
            Description = rel.Description,
            Notes = rel.Notes,
            TermsConditions = rel.TermsConditions,
            InteractionCount = rel.Interactions?.Count(i => !i.IsDeleted) ?? 0,
            LastInteractionDate = rel.Interactions?.Where(i => !i.IsDeleted).Max(i => (DateTime?)i.InteractionDate),
            CreatedAt = rel.CreatedAt,
            UpdatedAt = rel.UpdatedAt
        };
    }

    private RelationshipInteractionDto MapToDto(RelationshipInteraction interaction) => new()
    {
        Id = interaction.Id,
        AccountRelationshipId = interaction.AccountRelationshipId,
        InteractionType = interaction.InteractionType,
        Subject = interaction.Subject,
        Description = interaction.Description,
        InteractionDate = interaction.InteractionDate,
        DurationMinutes = interaction.DurationMinutes,
        ParticipantContactIds = !string.IsNullOrEmpty(interaction.ParticipantContactIds) 
            ? JsonSerializer.Deserialize<List<int>>(interaction.ParticipantContactIds) : null,
        ParticipantUserIds = !string.IsNullOrEmpty(interaction.ParticipantUserIds)
            ? JsonSerializer.Deserialize<List<int>>(interaction.ParticipantUserIds) : null,
        Outcome = interaction.Outcome,
        ActionItems = interaction.ActionItems,
        NextSteps = interaction.NextSteps,
        FollowUpDate = interaction.FollowUpDate,
        SentimentScore = interaction.SentimentScore,
        HealthImpact = interaction.HealthImpact,
        Location = interaction.Location,
        MeetingLink = interaction.MeetingLink,
        CreatedAt = interaction.CreatedAt
    };

    private AccountHealthSnapshotDto MapToDto(AccountHealthSnapshot snapshot) => new()
    {
        Id = snapshot.Id,
        CustomerId = snapshot.CustomerId,
        CustomerName = snapshot.Customer?.Company ?? $"{snapshot.Customer?.FirstName} {snapshot.Customer?.LastName}",
        SnapshotDate = snapshot.SnapshotDate,
        OverallHealthScore = snapshot.OverallHealthScore,
        EngagementScore = snapshot.EngagementScore,
        ProductAdoptionScore = snapshot.ProductAdoptionScore,
        SupportSatisfactionScore = snapshot.SupportSatisfactionScore,
        FinancialHealthScore = snapshot.FinancialHealthScore,
        RelationshipScore = snapshot.RelationshipScore,
        ActiveUsersCount = snapshot.ActiveUsersCount,
        FeatureAdoptionRate = snapshot.FeatureAdoptionRate,
        SupportTicketsCount = snapshot.SupportTicketsCount,
        SupportTicketsResolved = snapshot.SupportTicketsResolved,
        AverageResponseTimeHours = snapshot.AverageResponseTimeHours,
        NPSScore = snapshot.NPSScore,
        RiskFactors = !string.IsNullOrEmpty(snapshot.RiskFactors)
            ? JsonSerializer.Deserialize<List<string>>(snapshot.RiskFactors) : null,
        WarningSignals = !string.IsNullOrEmpty(snapshot.WarningSignals)
            ? JsonSerializer.Deserialize<List<string>>(snapshot.WarningSignals) : null,
        GrowthIndicators = !string.IsNullOrEmpty(snapshot.GrowthIndicators)
            ? JsonSerializer.Deserialize<List<string>>(snapshot.GrowthIndicators) : null,
        AnalystNotes = snapshot.AnalystNotes,
        PreviousHealthScore = snapshot.PreviousHealthScore,
        HealthTrend = snapshot.HealthTrend,
        CreatedAt = snapshot.CreatedAt
    };

    #endregion
}
