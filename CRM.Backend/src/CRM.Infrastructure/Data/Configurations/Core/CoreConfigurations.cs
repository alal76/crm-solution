// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Core;

/// <summary>
/// Entity configuration for Account (formerly Customer).
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).HasMaxLength(100);
        builder.Property(e => e.LastName).HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Company).HasMaxLength(255);
        builder.Property(e => e.LegalName).HasMaxLength(500);
        builder.Property(e => e.DbaName).HasMaxLength(255);
        builder.Property(e => e.TaxId).HasMaxLength(50);
        builder.Property(e => e.RegistrationNumber).HasMaxLength(100);
        builder.Property(e => e.Salutation).HasMaxLength(20);
        builder.Property(e => e.Suffix).HasMaxLength(20);
        builder.Property(e => e.Gender).HasMaxLength(20);

        // Map renamed properties to original database columns for backward compatibility
        builder.Property(e => e.AccountHealthScore).HasColumnName("CustomerHealthScore");

        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Company);

        // Self-referencing relationships
        builder.HasOne(e => e.ReferredByAccount)
            .WithMany()
            .HasForeignKey(e => e.ReferredByAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentAccount)
            .WithMany()
            .HasForeignKey(e => e.ParentAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // Lookup relationships
        builder.HasOne(c => c.CurrencyLookup)
            .WithMany()
            .HasForeignKey(c => c.CurrencyLookupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.BillingCycleLookup)
            .WithMany()
            .HasForeignKey(c => c.BillingCycleLookupId)
            .OnDelete(DeleteBehavior.SetNull);

        // User relationships
        builder.HasOne(e => e.AssignedToUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AccountManager)
            .WithMany()
            .HasForeignKey(e => e.AccountManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.VerifiedByUser)
            .WithMany()
            .HasForeignKey(e => e.VerifiedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Partnership relationships (self-referencing)
        builder.HasOne(e => e.ParentReseller)
            .WithMany(e => e.ResellerChildren)
            .HasForeignKey(e => e.ParentResellerAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.CompetitorAccount)
            .WithMany()
            .HasForeignKey(e => e.CompetitorAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // Lead conversion relationships
        builder.HasOne(e => e.ConvertedFromLead)
            .WithMany()
            .HasForeignKey(e => e.ConvertedFromLeadId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SourceCampaign)
            .WithMany()
            .HasForeignKey(e => e.SourceCampaignId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for AccountContact junction table.
/// </summary>
public class AccountContactConfiguration : IEntityTypeConfiguration<AccountContact>
{
    public void Configure(EntityTypeBuilder<AccountContact> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.AccountId, e.ContactId }).IsUnique();
        builder.Property(e => e.PositionAtAccount).HasMaxLength(100);
        builder.Property(e => e.DepartmentAtAccount).HasMaxLength(100);

        builder.HasOne(e => e.Account)
            .WithMany(c => c.AccountContacts)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Contact)
            .WithMany(c => c.AccountContacts)
            .HasForeignKey(e => e.ContactId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for Contact.
/// </summary>
public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        // Preferred contact method uses LookupItem
        builder.HasOne(c => c.PreferredContactMethodLookup)
            .WithMany()
            .HasForeignKey(c => c.PreferredContactMethodLookupId)
            .OnDelete(DeleteBehavior.SetNull);

        // Contact belongs to Account (one-to-many)
        builder.HasOne(c => c.Account)
            .WithMany(a => a.Contacts)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for Product.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(255);
        builder.Property(e => e.SKU).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.Cost).HasPrecision(18, 2);
        builder.HasIndex(e => e.SKU).IsUnique();
    }
}

/// <summary>
/// Entity configuration for Interaction.
/// </summary>
public class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type).IsRequired().HasMaxLength(50);

        builder.HasOne(e => e.Account)
            .WithMany(c => c.Interactions)
            .HasForeignKey(e => e.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Link Interaction -> MarketingCampaign
        builder.HasOne(e => e.Campaign)
            .WithMany()
            .HasForeignKey(e => e.CampaignId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for User.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Username).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        // Column mappings for backward compatibility
        builder.Property(e => e.LastLoginAt).HasColumnName("LastLoginAt");
        builder.Property(e => e.EmailVerified).HasColumnName("IsEmailVerified");

        // Configure relationships
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Users)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.UserProfile)
            .WithMany(p => p.Users)
            .HasForeignKey(e => e.UserProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.PrimaryGroup)
            .WithMany(g => g.PrimaryUsers)
            .HasForeignKey(e => e.PrimaryGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for UserGroup.
/// </summary>
public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
    }
}

/// <summary>
/// Entity configuration for LookupCategory.
/// </summary>
public class LookupCategoryConfiguration : IEntityTypeConfiguration<LookupCategory>
{
    public void Configure(EntityTypeBuilder<LookupCategory> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
    }
}

/// <summary>
/// Entity configuration for LookupItem.
/// </summary>
public class LookupItemConfiguration : IEntityTypeConfiguration<LookupItem>
{
    public void Configure(EntityTypeBuilder<LookupItem> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Value).IsRequired().HasMaxLength(500);
        builder.HasOne(e => e.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(e => e.LookupCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.LookupCategoryId, e.SortOrder });
    }
}
