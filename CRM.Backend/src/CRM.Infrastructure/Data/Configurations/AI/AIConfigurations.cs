// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

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
