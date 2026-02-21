// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Service for managing discount rules and calculations
/// </summary>
public class DiscountRuleService : IDiscountRuleService
{
    private readonly IRepository<DiscountRule> _ruleRepository;
    private readonly IRepository<DiscountHistory> _historyRepository;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<DiscountRuleService> _logger;

    public DiscountRuleService(
        IRepository<DiscountRule> ruleRepository,
        IRepository<DiscountHistory> historyRepository,
        ICrmDbContext dbContext,
        ILogger<DiscountRuleService> logger)
    {
        _ruleRepository = ruleRepository;
        _historyRepository = historyRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DiscountRuleDto> CreateAsync(CreateDiscountRuleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Discount rule name is required");
        if (dto.Value < 0)
            throw new ArgumentException("Discount value cannot be negative");

        var rule = new DiscountRule
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Value = dto.Value,
            MinOrderAmount = dto.MinOrderAmount,
            MinQuantity = dto.MinQuantity,
            CustomerTier = dto.CustomerTier,
            ProductCategory = dto.ProductCategory,
            MaxDiscount = dto.MaxDiscount,
            EffectiveDate = dto.EffectiveDate,
            ExpiryDate = dto.ExpiryDate,
            IsActive = dto.IsActive,
            IsCumulative = dto.IsCumulative,
            CreatedAt = DateTime.UtcNow
        };

        await _ruleRepository.AddAsync(rule);
        _logger.LogInformation("Discount rule created: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<DiscountRuleDto> UpdateAsync(int id, UpdateDiscountRuleDto dto, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Discount rule with ID {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            rule.Name = dto.Name;
        if (dto.Description != null)
            rule.Description = dto.Description;
        if (dto.Type.HasValue)
            rule.Type = dto.Type.Value;
        if (dto.Value.HasValue)
            rule.Value = dto.Value.Value;
        if (dto.MinOrderAmount.HasValue)
            rule.MinOrderAmount = dto.MinOrderAmount;
        if (dto.MinQuantity.HasValue)
            rule.MinQuantity = dto.MinQuantity;
        if (dto.CustomerTier != null)
            rule.CustomerTier = dto.CustomerTier;
        if (dto.ProductCategory != null)
            rule.ProductCategory = dto.ProductCategory;
        if (dto.MaxDiscount.HasValue)
            rule.MaxDiscount = dto.MaxDiscount;
        if (dto.EffectiveDate.HasValue)
            rule.EffectiveDate = dto.EffectiveDate.Value;
        if (dto.ExpiryDate.HasValue)
            rule.ExpiryDate = dto.ExpiryDate;
        if (dto.IsActive.HasValue)
            rule.IsActive = dto.IsActive.Value;
        if (dto.IsCumulative.HasValue)
            rule.IsCumulative = dto.IsCumulative.Value;

        rule.UpdatedAt = DateTime.UtcNow;
        await _ruleRepository.UpdateAsync(rule);
        _logger.LogInformation("Discount rule updated: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<DiscountRuleDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<List<DiscountRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rules = await _ruleRepository.GetAllAsync();
        return rules.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Discount rule with ID {id} not found");

        rule.IsDeleted = true;
        await _ruleRepository.UpdateAsync(rule);
        _logger.LogInformation("Discount rule deleted: {RuleId}", id);
    }

    public async Task<List<DiscountRuleDto>> GetApplicableRulesAsync(
        int accountId,
        int? productId,
        decimal orderAmount,
        CancellationToken ct = default)
    {
        var today = DateTime.UtcNow;
        var rules = await _ruleRepository.GetAllAsync();

        var applicable = rules
            .Where(r => r.IsActive
                && r.EffectiveDate <= today
                && (r.ExpiryDate == null || r.ExpiryDate >= today)
                && (r.MinOrderAmount == null || r.MinOrderAmount <= orderAmount)
                && (r.ProductCategory == null || true)) // Simplified - full logic would check product category
            .ToList();

        return applicable.Select(MapToDto).ToList();
    }

    public async Task<DiscountCalculationDto> CalculateDiscountAsync(
        int accountId,
        int? productId,
        decimal orderAmount,
        CancellationToken ct = default)
    {
        var applicableRules = await GetApplicableRulesAsync(accountId, productId, orderAmount, ct);

        if (!applicableRules.Any())
        {
            return new DiscountCalculationDto
            {
                OrderAmount = orderAmount,
                DiscountAmount = 0,
                FinalAmount = orderAmount,
                AppliedRules = new(),
                CalculationDetails = "No applicable discount rules found"
            };
        }

        decimal totalDiscount = 0;
        var appliedRules = new List<DiscountRuleDto>();

        foreach (var ruleDto in applicableRules)
        {
            if (!ruleDto.IsCumulative && appliedRules.Any())
                continue;

            decimal ruleDiscount = 0;

            if (ruleDto.Type == "Percentage")
            {
                ruleDiscount = (orderAmount * ruleDto.Value) / 100;
            }
            else if (ruleDto.Type == "Fixed")
            {
                ruleDiscount = ruleDto.Value;
            }
            else if (ruleDto.Type == "VolumeBased" || ruleDto.Type == "TierBased")
            {
                ruleDiscount = (orderAmount * ruleDto.Value) / 100;
            }

            // Apply max discount cap
            if (ruleDto.MaxDiscount.HasValue && ruleDiscount > ruleDto.MaxDiscount)
                ruleDiscount = ruleDto.MaxDiscount.Value;

            totalDiscount += ruleDiscount;
            appliedRules.Add(ruleDto);
        }

        var finalAmount = Math.Max(0, orderAmount - totalDiscount);

        return new DiscountCalculationDto
        {
            OrderAmount = orderAmount,
            DiscountAmount = totalDiscount,
            FinalAmount = finalAmount,
            AppliedRules = appliedRules,
            CalculationDetails = $"Applied {appliedRules.Count} discount rule(s)"
        };
    }

    private static DiscountRuleDto MapToDto(DiscountRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        Type = rule.Type.ToString(),
        Value = rule.Value,
        MinOrderAmount = rule.MinOrderAmount,
        MinQuantity = rule.MinQuantity,
        CustomerTier = rule.CustomerTier,
        ProductCategory = rule.ProductCategory,
        MaxDiscount = rule.MaxDiscount,
        EffectiveDate = rule.EffectiveDate,
        ExpiryDate = rule.ExpiryDate,
        IsActive = rule.IsActive,
        IsCumulative = rule.IsCumulative,
        CreatedAt = rule.CreatedAt,
        UpdatedAt = rule.UpdatedAt
    };
}
