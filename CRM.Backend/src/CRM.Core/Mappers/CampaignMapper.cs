// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using CRM.Core.Entities;

namespace CRM.Core.Mappers;

public static class CampaignMapper
{
    public static CampaignDto ToDto(MarketingCampaign entity)
    {
        return new CampaignDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CampaignCode = entity.CampaignCode,
            Description = entity.Description,
            Objective = (int)entity.ObjectiveType,
            CampaignType = (int)entity.CampaignType,
            Status = (int)entity.Status,
            Priority = (int)entity.Priority,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Budget = entity.Budget,
            ActualCost = entity.ActualCost,
            ActualRevenue = entity.ActualRevenue ?? 0,
            LeadsGenerated = entity.LeadsGenerated,
            MqlsGenerated = entity.MqlsGenerated,
            SqlsGenerated = entity.SqlsGenerated,
            OwnerId = entity.OwnerId,
            OwnerName = entity.Owner?.FullName,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Roi = (decimal)entity.ROI,
            Cpl = entity.CostPerLead ?? 0,
            Cpa = entity.CostPerAcquisition ?? 0
        };
    }

    public static MarketingCampaign ToEntity(CreateCampaignDto dto)
    {
        return new MarketingCampaign
        {
            Name = dto.Name,
            CampaignCode = dto.CampaignCode,
            Description = dto.Description ?? string.Empty,
            ObjectiveType = (CampaignObjective)(dto.Objective ?? 0),
            CampaignType = (CampaignType)dto.CampaignType,
            Priority = (CampaignPriority)dto.Priority,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Budget = dto.Budget,
            ActualCost = dto.ActualCost,
            OwnerId = dto.OwnerId,
            Tags = dto.Tags
        };
    }

    public static void UpdateEntity(MarketingCampaign entity, UpdateCampaignDto dto)
    {
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.CampaignCode != null) entity.CampaignCode = dto.CampaignCode;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.Objective.HasValue) entity.ObjectiveType = (CampaignObjective)dto.Objective.Value;
        if (dto.CampaignType.HasValue) entity.CampaignType = (CampaignType)dto.CampaignType.Value;
        if (dto.Priority.HasValue) entity.Priority = (CampaignPriority)dto.Priority.Value;
        if (dto.StartDate.HasValue) entity.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) entity.EndDate = dto.EndDate;
        if (dto.Budget.HasValue) entity.Budget = dto.Budget.Value;
        if (dto.ActualCost.HasValue) entity.ActualCost = dto.ActualCost.Value;
        if (dto.Status.HasValue) entity.Status = (CampaignStatus)dto.Status.Value;
        if (dto.OwnerId.HasValue) entity.OwnerId = dto.OwnerId;
    }
}
