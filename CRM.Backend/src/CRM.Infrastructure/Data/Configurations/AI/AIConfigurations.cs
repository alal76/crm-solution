// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.AI;

/// <summary>
/// Entity configuration for AIModel.
/// </summary>
public class AIModelConfiguration : IEntityTypeConfiguration<AIModel>
{
    public void Configure(EntityTypeBuilder<AIModel> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for Prediction.
/// </summary>
public class PredictionConfiguration : IEntityTypeConfiguration<Prediction>
{
    public void Configure(EntityTypeBuilder<Prediction> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for LeadScore.
/// </summary>
public class LeadScoreConfiguration : IEntityTypeConfiguration<LeadScore>
{
    public void Configure(EntityTypeBuilder<LeadScore> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for OpportunityInsight.
/// </summary>
public class OpportunityInsightConfiguration : IEntityTypeConfiguration<OpportunityInsight>
{
    public void Configure(EntityTypeBuilder<OpportunityInsight> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ChurnRisk.
/// </summary>
public class ChurnRiskConfiguration : IEntityTypeConfiguration<ChurnRisk>
{
    public void Configure(EntityTypeBuilder<ChurnRisk> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ActionRecommendation.
/// </summary>
public class ActionRecommendationConfiguration : IEntityTypeConfiguration<ActionRecommendation>
{
    public void Configure(EntityTypeBuilder<ActionRecommendation> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailIntelligence.
/// </summary>
public class EmailIntelligenceConfiguration : IEntityTypeConfiguration<EmailIntelligence>
{
    public void Configure(EntityTypeBuilder<EmailIntelligence> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
