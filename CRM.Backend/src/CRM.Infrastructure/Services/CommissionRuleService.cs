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
/// Service for managing commission rules and calculations
/// </summary>
public class CommissionRuleService : ICommissionRuleService
{
    private readonly IRepository<CommissionRule> _ruleRepository;
    private readonly IRepository<CommissionHistory> _historyRepository;
    private readonly ICrmDbContext _dbContext;
    private readonly ILogger<CommissionRuleService> _logger;

    public CommissionRuleService(
        IRepository<CommissionRule> ruleRepository,
        IRepository<CommissionHistory> historyRepository,
        ICrmDbContext dbContext,
        ILogger<CommissionRuleService> logger)
    {
        _ruleRepository = ruleRepository;
        _historyRepository = historyRepository;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<CommissionRuleDto> CreateAsync(CreateCommissionRuleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Commission rule name is required");
        if (string.IsNullOrWhiteSpace(dto.SaleType))
            throw new ArgumentException("Sale type is required");
        if (dto.Rate < 0)
            throw new ArgumentException("Commission rate cannot be negative");

        var rule = new CommissionRule
        {
            Name = dto.Name,
            Description = dto.Description,
            SaleType = dto.SaleType,
            RuleType = dto.RuleType,
            Rate = dto.Rate,
            MinAmount = dto.MinAmount,
            MaxAmount = dto.MaxAmount,
            EffectiveDate = dto.EffectiveDate,
            ExpiryDate = dto.ExpiryDate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _ruleRepository.AddAsync(rule);
        _logger.LogInformation("Commission rule created: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<CommissionRuleDto> UpdateAsync(int id, UpdateCommissionRuleDto dto, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Commission rule with ID {id} not found");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            rule.Name = dto.Name;
        if (dto.Description != null)
            rule.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.SaleType))
            rule.SaleType = dto.SaleType;
        if (dto.RuleType.HasValue)
            rule.RuleType = dto.RuleType.Value;
        if (dto.Rate.HasValue)
            rule.Rate = dto.Rate.Value;
        if (dto.MinAmount.HasValue)
            rule.MinAmount = dto.MinAmount;
        if (dto.MaxAmount.HasValue)
            rule.MaxAmount = dto.MaxAmount;
        if (dto.EffectiveDate.HasValue)
            rule.EffectiveDate = dto.EffectiveDate.Value;
        if (dto.ExpiryDate.HasValue)
            rule.ExpiryDate = dto.ExpiryDate;
        if (dto.IsActive.HasValue)
            rule.IsActive = dto.IsActive.Value;

        await _ruleRepository.UpdateAsync(rule);
        _logger.LogInformation("Commission rule updated: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);

        return MapToDto(rule);
    }

    public async Task<CommissionRuleDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id);
        return rule == null ? null : MapToDto(rule);
    }

    public async Task<List<CommissionRuleDto>> GetAllAsync(CancellationToken ct = default)
    {
        var rules = await _ruleRepository.GetAllAsync();
        return rules.Select(MapToDto).ToList();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var rule = await _ruleRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Commission rule with ID {id} not found");

        rule.IsDeleted = true;
        await _ruleRepository.UpdateAsync(rule);
        _logger.LogInformation("Commission rule deleted: {RuleId}", id);
    }

    public async Task<List<CommissionRuleDto>> GetApplicableRulesAsync(string saleType, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow;
        var rules = await _ruleRepository.GetAllAsync();

        var applicable = rules
            .Where(r => r.SaleType == saleType
                && r.IsActive
                && r.EffectiveDate <= today
                && (r.ExpiryDate == null || r.ExpiryDate >= today))
            .OrderBy(r => r.RuleType)
            .ToList();

        return applicable.Select(MapToDto).ToList();
    }

    public async Task<CommissionCalculationDto> CalculateCommissionAsync(
        decimal saleAmount,
        string saleType,
        CancellationToken ct = default)
    {
        var applicableRules = await GetApplicableRulesAsync(saleType, ct);

        if (!applicableRules.Any())
            throw new InvalidOperationException($"No applicable commission rules found for sale type: {saleType}");

        // Find matching rule based on tiers or flat rates
        CommissionRuleDto? matchedRule = null;
        decimal commissionAmount = 0;

        foreach (var ruleDto in applicableRules)
        {
            var rule = await _ruleRepository.GetByIdAsync(ruleDto.Id);
            if (rule == null)
                continue;

            if (rule.RuleType == CommissionRuleType.Tiered)
            {
                if ((rule.MinAmount ?? 0) <= saleAmount && (rule.MaxAmount ?? decimal.MaxValue) >= saleAmount)
                {
                    matchedRule = ruleDto;
                    commissionAmount = (saleAmount * rule.Rate) / 100;
                    break;
                }
            }
            else if (rule.RuleType == CommissionRuleType.Percentage)
            {
                matchedRule = ruleDto;
                commissionAmount = (saleAmount * rule.Rate) / 100;
                break;
            }
            else if (rule.RuleType == CommissionRuleType.Flat)
            {
                matchedRule = ruleDto;
                commissionAmount = rule.Rate;
                break;
            }
        }

        if (matchedRule == null)
            throw new InvalidOperationException("Could not calculate commission: no matching rule found");

        return new CommissionCalculationDto
        {
            DealAmount = saleAmount,
            Commission = commissionAmount,
            CommissionRate = matchedRule.Rate,
            RuleId = matchedRule.Id,
            RuleName = matchedRule.Name
        };
    }

    private static CommissionRuleDto MapToDto(CommissionRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        Description = rule.Description,
        SaleType = rule.SaleType,
        RuleType = rule.RuleType.ToString(),
        Rate = rule.Rate,
        MinAmount = rule.MinAmount,
        MaxAmount = rule.MaxAmount,
        EffectiveDate = rule.EffectiveDate,
        ExpiryDate = rule.ExpiryDate,
        IsActive = rule.IsActive,
        CreatedAt = rule.CreatedAt,
        UpdatedAt = rule.UpdatedAt
    };
}
