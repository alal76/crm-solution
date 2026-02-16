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
/// Entity configuration for EmailSequence (Email drip campaigns).
/// Ensures proper column constraints, relationships, and performance indexes.
/// </summary>
public class EmailSequenceConfiguration : IEntityTypeConfiguration<EmailSequence>
{
    public void Configure(EntityTypeBuilder<EmailSequence> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        
        // Relationships
        builder.HasOne(e => e.Sender)
            .WithMany()
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(e => e.Steps)
            .WithOne(s => s.EmailSequence)
            .HasForeignKey(s => s.EmailSequenceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(e => e.Enrollments)
            .WithOne(e => e.EmailSequence)
            .HasForeignKey(e => e.EmailSequenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EmailSequenceStep (Individual steps within sequences).
/// Represents a single action in a sequence (email, wait, task, condition, etc.).
/// </summary>
public class EmailSequenceStepConfiguration : IEntityTypeConfiguration<EmailSequenceStep>
{
    public void Configure(EntityTypeBuilder<EmailSequenceStep> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.EmailSequenceId).IsRequired();
        
        builder.HasOne(e => e.EmailSequence)
            .WithMany(s => s.Steps)
            .HasForeignKey(e => e.EmailSequenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EmailSequenceEnrollment (Contact/Lead enrollment in a sequence).
/// Tracks which contacts/leads are enrolled, their current status, and progress.
/// </summary>
public class EmailSequenceEnrollmentConfiguration : IEntityTypeConfiguration<EmailSequenceEnrollment>
{
    public void Configure(EntityTypeBuilder<EmailSequenceEnrollment> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Required properties
        builder.Property(e => e.SequenceId).IsRequired();
        builder.Property(e => e.RecipientEmail).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Status).HasMaxLength(50).HasDefaultValue(EnrollmentStatus.Active);
        builder.Property(e => e.CurrentStepIndex).HasDefaultValue(0);
        builder.Property(e => e.EnrolledAt).HasDefaultValueSql("GETUTCDATE()");
        
        // Optional properties
        builder.Property(e => e.ContactId);
        builder.Property(e => e.LeadId);
        builder.Property(e => e.CurrentStepId);
        builder.Property(e => e.ExitReason).HasMaxLength(500);
        builder.Property(e => e.EnrolledById);
        
        // Relationships
        // EmailSequenceEnrollment -> EmailSequence (many-to-one)
        builder.HasOne(e => e.EmailSequence)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.SequenceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // EmailSequenceEnrollment -> Contact
        builder.HasOne(e => e.Contact)
            .WithMany()
            .HasForeignKey(e => e.ContactId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // EmailSequenceEnrollment -> Lead
        builder.HasOne(e => e.Lead)
            .WithMany()
            .HasForeignKey(e => e.LeadId)
            .OnDelete(DeleteBehavior.SetNull);
        
        // EmailSequenceEnrollment -> User (EnrolledBy)
        builder.HasOne(e => e.EnrolledBy)
            .WithMany()
            .HasForeignKey(e => e.EnrolledById)
            .OnDelete(DeleteBehavior.SetNull);
        
        // EmailSequenceEnrollment -> EmailSequenceStepExecution (one-to-many)
        builder.HasMany(e => e.StepExecutions)
            .WithOne(se => se.EmailSequenceEnrollment)
            .HasForeignKey(se => se.EmailSequenceEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes for performance
        builder.HasIndex(e => e.SequenceId).HasDatabaseName("IX_EmailSequenceEnrollments_SequenceId");
        builder.HasIndex(e => e.Status).HasDatabaseName("IX_EmailSequenceEnrollments_Status");
        builder.HasIndex(e => e.ContactId).HasDatabaseName("IX_EmailSequenceEnrollments_ContactId");
        builder.HasIndex(e => e.LeadId).HasDatabaseName("IX_EmailSequenceEnrollments_LeadId");
        builder.HasIndex(e => new { e.SequenceId, e.Status }).HasDatabaseName("IX_EmailSequenceEnrollments_SequenceId_Status");
    }
}

/// <summary>
/// Entity configuration for EmailSequenceStepExecution (Execution history of individual steps).
/// Captures when each step was executed, results, and any errors.
/// </summary>
public class EmailSequenceStepExecutionConfiguration : IEntityTypeConfiguration<EmailSequenceStepExecution>
{
    public void Configure(EntityTypeBuilder<EmailSequenceStepExecution> builder)
    {
        builder.HasKey(e => e.Id);
        
        // Required properties
        builder.Property(e => e.EmailSequenceStepId).IsRequired();
        builder.Property(e => e.EmailSequenceEnrollmentId).IsRequired();
        builder.Property(e => e.ScheduledAt).IsRequired();
        builder.Property(e => e.Success).HasDefaultValue(false);
        
        // Optional properties
        builder.Property(e => e.ExecutedAt);
        builder.Property(e => e.ErrorMessage).HasMaxLength(1000);
        builder.Property(e => e.MessageId).HasMaxLength(255);
        builder.Property(e => e.Opens).HasDefaultValue(0);
        builder.Property(e => e.Clicks).HasDefaultValue(0);
        builder.Property(e => e.Replied).HasDefaultValue(false);
        builder.Property(e => e.RepliedAt);
        builder.Property(e => e.Bounced).HasDefaultValue(false);
        builder.Property(e => e.BounceType).HasMaxLength(50);
        
        // Relationships
        // EmailSequenceStepExecution -> EmailSequenceEnrollment
        builder.HasOne(e => e.EmailSequenceEnrollment)
            .WithMany(en => en.StepExecutions)
            .HasForeignKey(e => e.EmailSequenceEnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // EmailSequenceStepExecution -> EmailSequenceStep
        builder.HasOne(e => e.EmailSequenceStep)
            .WithMany()
            .HasForeignKey(e => e.EmailSequenceStepId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Indexes for performance
        builder.HasIndex(e => e.EmailSequenceEnrollmentId).HasDatabaseName("IX_EmailSequenceStepExecutions_EnrollmentId");
        builder.HasIndex(e => e.ExecutedAt).HasDatabaseName("IX_EmailSequenceStepExecutions_ExecutedAt");
        builder.HasIndex(e => e.Success).HasDatabaseName("IX_EmailSequenceStepExecutions_Success");
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

/// <summary>
/// Entity configuration for LandingPage.
/// </summary>
public class LandingPageConfiguration : IEntityTypeConfiguration<LandingPage>
{
    public void Configure(EntityTypeBuilder<LandingPage> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(e => e.Slug)
            .IsUnique();

        builder.HasIndex(e => e.Status);

        builder.HasIndex(e => new { e.Status, e.IsActive });

        builder.HasOne(e => e.FormDefinition)
            .WithMany()
            .HasForeignKey(e => e.FormDefinitionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Campaign)
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CreatedByUser)
            .WithMany()
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Entity configuration for LandingPageBlock.
/// </summary>
public class LandingPageBlockConfiguration : IEntityTypeConfiguration<LandingPageBlock>
{
    public void Configure(EntityTypeBuilder<LandingPageBlock> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.LandingPageId, e.SortOrder });

        builder.HasOne(e => e.LandingPage)
            .WithMany(lp => lp.Blocks)
            .HasForeignKey(e => e.LandingPageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for LandingPageVisit.
/// </summary>
public class LandingPageVisitConfiguration : IEntityTypeConfiguration<LandingPageVisit>
{
    public void Configure(EntityTypeBuilder<LandingPageVisit> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.LandingPageId);

        builder.HasIndex(e => e.VisitedAt);

        builder.HasIndex(e => new { e.LandingPageId, e.VisitorId });

        builder.HasOne(e => e.LandingPage)
            .WithMany(lp => lp.Visits)
            .HasForeignKey(e => e.LandingPageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
