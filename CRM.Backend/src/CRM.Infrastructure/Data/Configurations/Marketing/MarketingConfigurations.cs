// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Marketing;

/// <summary>
/// Entity configuration for MarketingCampaign.
/// </summary>
public class MarketingCampaignConfiguration : IEntityTypeConfiguration<MarketingCampaign>
{
    public void Configure(EntityTypeBuilder<MarketingCampaign> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Budget).HasPrecision(18, 2);
    }
}

/// <summary>
/// Entity configuration for CampaignMetric.
/// </summary>
public class CampaignMetricConfiguration : IEntityTypeConfiguration<CampaignMetric>
{
    public void Configure(EntityTypeBuilder<CampaignMetric> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Campaign)
            .WithMany(c => c.Metrics)
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for CampaignRecipient.
/// </summary>
public class CampaignRecipientConfiguration : IEntityTypeConfiguration<CampaignRecipient>
{
    public void Configure(EntityTypeBuilder<CampaignRecipient> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for CampaignLinkClick.
/// </summary>
public class CampaignLinkClickConfiguration : IEntityTypeConfiguration<CampaignLinkClick>
{
    public void Configure(EntityTypeBuilder<CampaignLinkClick> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for CampaignABTest.
/// </summary>
public class CampaignABTestConfiguration : IEntityTypeConfiguration<CampaignABTest>
{
    public void Configure(EntityTypeBuilder<CampaignABTest> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for CampaignConversion.
/// </summary>
public class CampaignConversionConfiguration : IEntityTypeConfiguration<CampaignConversion>
{
    public void Configure(EntityTypeBuilder<CampaignConversion> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for CampaignWorkflow.
/// </summary>
public class CampaignWorkflowConfiguration : IEntityTypeConfiguration<CampaignWorkflow>
{
    public void Configure(EntityTypeBuilder<CampaignWorkflow> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailSequence.
/// </summary>
public class EmailSequenceConfiguration : IEntityTypeConfiguration<EmailSequence>
{
    public void Configure(EntityTypeBuilder<EmailSequence> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailSequenceStep.
/// </summary>
public class EmailSequenceStepConfiguration : IEntityTypeConfiguration<EmailSequenceStep>
{
    public void Configure(EntityTypeBuilder<EmailSequenceStep> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailSequenceEnrollment.
/// </summary>
public class EmailSequenceEnrollmentConfiguration : IEntityTypeConfiguration<EmailSequenceEnrollment>
{
    public void Configure(EntityTypeBuilder<EmailSequenceEnrollment> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailSequenceStepExecution.
/// </summary>
public class EmailSequenceStepExecutionConfiguration : IEntityTypeConfiguration<EmailSequenceStepExecution>
{
    public void Configure(EntityTypeBuilder<EmailSequenceStepExecution> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for EmailTemplate.
/// </summary>
public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for FormDefinition.
/// </summary>
public class FormDefinitionConfiguration : IEntityTypeConfiguration<FormDefinition>
{
    public void Configure(EntityTypeBuilder<FormDefinition> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for FormField.
/// </summary>
public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for FormSubmission.
/// </summary>
public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
