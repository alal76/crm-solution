// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Sales;

/// <summary>
/// Entity configuration for Opportunity.
/// </summary>
public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        // Link Opportunity -> Account (required)
        builder.HasOne(e => e.Account)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Link Opportunity -> Lead (optional, source lead)
        builder.HasOne(e => e.Lead)
            .WithMany(l => l.Opportunities)
            .HasForeignKey(e => e.LeadId)
            .OnDelete(DeleteBehavior.SetNull);

        // Link Opportunity -> User (sales owner)
        builder.HasOne(e => e.SalesOwner)
            .WithMany()
            .HasForeignKey(e => e.SalesOwnerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for OpportunityProduct junction table.
/// </summary>
public class OpportunityProductConfiguration : IEntityTypeConfiguration<OpportunityProduct>
{
    public void Configure(EntityTypeBuilder<OpportunityProduct> builder)
    {
        builder.HasKey(op => new { op.OpportunityId, op.ProductId });

        builder.HasOne(op => op.Opportunity)
            .WithMany(o => o.Products)
            .HasForeignKey(op => op.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.Product)
            .WithMany()
            .HasForeignKey(op => op.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.DiscountPercent).HasPrecision(5, 2);
        builder.Property(e => e.LineTotal).HasPrecision(18, 2);
    }
}

/// <summary>
/// Entity configuration for Lead.
/// </summary>
public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional Lead configurations can be added here
    }
}

/// <summary>
/// Entity configuration for LeadProductInterest junction table.
/// </summary>
public class LeadProductInterestConfiguration : IEntityTypeConfiguration<LeadProductInterest>
{
    public void Configure(EntityTypeBuilder<LeadProductInterest> builder)
    {
        builder.HasKey(lpi => new { lpi.LeadId, lpi.ProductId });

        builder.HasOne(lpi => lpi.Lead)
            .WithMany(l => l.ProductInterests)
            .HasForeignKey(lpi => lpi.LeadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(lpi => lpi.Product)
            .WithMany()
            .HasForeignKey(lpi => lpi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Entity configuration for Quote.
/// </summary>
public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional Quote configurations can be added here
    }
}

/// <summary>
/// Entity configuration for QuoteLineItem.
/// </summary>
public class QuoteLineItemConfiguration : IEntityTypeConfiguration<QuoteLineItem>
{
    public void Configure(EntityTypeBuilder<QuoteLineItem> builder)
    {
        builder.HasKey(e => e.Id);
        // Additional QuoteLineItem configurations can be added here
    }
}

/// <summary>
/// Entity configuration for EventAttendee.
/// Tracks attendees for calendar/meeting activities.
/// </summary>
public class EventAttendeeConfiguration : IEntityTypeConfiguration<EventAttendee>
{
    public void Configure(EntityTypeBuilder<EventAttendee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.AttendeeId).IsRequired();
        builder.Property(e => e.Role).HasMaxLength(100);
        builder.Property(e => e.ExternalCalendarEventId).HasMaxLength(500);

        // Index for quick lookup by activity
        builder.HasIndex(e => e.ActivityId);

        // Index for finding all events a user/contact/lead is attending
        builder.HasIndex(e => new { e.AttendeeType, e.AttendeeId });

        // Foreign key to Activity
        builder.HasOne(e => e.Activity)
            .WithMany(a => a.Attendees)
            .HasForeignKey(e => e.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for CalendarIntegration.
/// Part of G4: Calendar Sync implementation.
/// </summary>
public class CalendarIntegrationConfiguration : IEntityTypeConfiguration<CalendarIntegration>
{
    public void Configure(EntityTypeBuilder<CalendarIntegration> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.AccessToken).IsRequired();
        builder.Property(e => e.RefreshToken).IsRequired();
        builder.Property(e => e.CalendarId).HasMaxLength(500);
        builder.Property(e => e.CalendarName).HasMaxLength(200);
        builder.Property(e => e.ExternalEmail).HasMaxLength(254);
        builder.Property(e => e.LastSyncError).HasMaxLength(2000);
        builder.Property(e => e.SyncToken).HasMaxLength(1000);

        // Index for finding integrations by user
        builder.HasIndex(e => e.UserId);

        // Index for finding active integrations
        builder.HasIndex(e => new { e.IsActive, e.NextSyncAt });

        // Unique constraint: one integration per user per provider
        builder.HasIndex(e => new { e.UserId, e.Provider }).IsUnique();

        // Foreign key to User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for CalendarSyncLog.
/// </summary>
public class CalendarSyncLogConfiguration : IEntityTypeConfiguration<CalendarSyncLog>
{
    public void Configure(EntityTypeBuilder<CalendarSyncLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ErrorMessage).HasMaxLength(4000);

        // Index for finding logs by integration
        builder.HasIndex(e => e.CalendarIntegrationId);

        // Foreign key to CalendarIntegration
        builder.HasOne(e => e.CalendarIntegration)
            .WithMany(c => c.SyncLogs)
            .HasForeignKey(e => e.CalendarIntegrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for CalendarEventMapping.
/// </summary>
public class CalendarEventMappingConfiguration : IEntityTypeConfiguration<CalendarEventMapping>
{
    public void Configure(EntityTypeBuilder<CalendarEventMapping> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalEventId).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ExternalEventUid).HasMaxLength(500);
        builder.Property(e => e.ExternalETag).HasMaxLength(200);

        // Index for finding mappings by activity
        builder.HasIndex(e => e.ActivityId);

        // Index for finding mappings by external event
        builder.HasIndex(e => new { e.CalendarIntegrationId, e.ExternalEventId }).IsUnique();

        // Foreign key to Activity
        builder.HasOne(e => e.Activity)
            .WithMany()
            .HasForeignKey(e => e.ActivityId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to CalendarIntegration
        builder.HasOne(e => e.CalendarIntegration)
            .WithMany()
            .HasForeignKey(e => e.CalendarIntegrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EmailIntegration.
/// Part of G5: Email Sync implementation.
/// </summary>
public class EmailIntegrationConfiguration : IEntityTypeConfiguration<EmailIntegration>
{
    public void Configure(EntityTypeBuilder<EmailIntegration> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmailAddress).HasMaxLength(254).IsRequired();
        builder.Property(e => e.ImapServer).HasMaxLength(200);
        builder.Property(e => e.ImapUsername).HasMaxLength(254);
        builder.Property(e => e.LastSyncError).HasMaxLength(2000);
        builder.Property(e => e.LastSyncToken).HasMaxLength(2000);

        // Index for finding integrations by user
        builder.HasIndex(e => e.UserId);

        // Unique constraint: one integration per user per provider per email
        builder.HasIndex(e => new { e.UserId, e.Provider, e.EmailAddress }).IsUnique();

        // Foreign key to User
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EmailSyncLog.
/// </summary>
public class EmailSyncLogConfiguration : IEntityTypeConfiguration<EmailSyncLog>
{
    public void Configure(EntityTypeBuilder<EmailSyncLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ErrorMessage).HasMaxLength(4000);

        // Index for finding logs by integration
        builder.HasIndex(e => e.EmailIntegrationId);

        // Foreign key to EmailIntegration
        builder.HasOne(e => e.EmailIntegration)
            .WithMany(i => i.SyncLogs)
            .HasForeignKey(e => e.EmailIntegrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EmailMessageMapping.
/// </summary>
public class EmailMessageMappingConfiguration : IEntityTypeConfiguration<EmailMessageMapping>
{
    public void Configure(EntityTypeBuilder<EmailMessageMapping> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ExternalMessageId).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ExternalThreadId).HasMaxLength(500);
        builder.Property(e => e.ExternalChangeKey).HasMaxLength(200);

        // Index for mapping lookups
        builder.HasIndex(e => new { e.EmailIntegrationId, e.ExternalMessageId }).IsUnique();

        // Foreign key to CommunicationMessage
        builder.HasOne(e => e.CommunicationMessage)
            .WithMany()
            .HasForeignKey(e => e.CommunicationMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to EmailIntegration
        builder.HasOne(e => e.EmailIntegration)
            .WithMany(i => i.MessageMappings)
            .HasForeignKey(e => e.EmailIntegrationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
