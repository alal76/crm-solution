// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.ContactInfo;

/// <summary>
/// Entity configuration for Address.
/// </summary>
public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Line1).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Line2).HasMaxLength(500);
        builder.Property(e => e.Line3).HasMaxLength(500);
        builder.Property(e => e.City).HasMaxLength(200);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.PostalCode).HasMaxLength(20);
        builder.Property(e => e.County).HasMaxLength(100);
        builder.Property(e => e.CountryCode).HasMaxLength(10);
        builder.Property(e => e.Country).HasMaxLength(200);
        builder.Property(e => e.Locality).HasMaxLength(200);
        builder.Property(e => e.AddressXml).HasColumnType("TEXT");
        builder.Property(e => e.Latitude).HasPrecision(10, 6);
        builder.Property(e => e.Longitude).HasPrecision(10, 6);

        // FK to ZipCode
        builder.HasOne(e => e.ZipCodeData)
            .WithMany(z => z.Addresses)
            .HasForeignKey(e => e.ZipCodeId)
            .OnDelete(DeleteBehavior.SetNull);

        // FK to Locality
        builder.HasOne(e => e.LocalityData)
            .WithMany()
            .HasForeignKey(e => e.LocalityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ZipCodeId);
        builder.HasIndex(e => e.LocalityId);
        builder.HasIndex(e => e.PostalCode);
        builder.HasIndex(e => e.City);
    }
}

/// <summary>
/// Entity configuration for ContactDetail.
/// </summary>
public class ContactDetailConfiguration : IEntityTypeConfiguration<ContactDetail>
{
    public void Configure(EntityTypeBuilder<ContactDetail> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Value).IsRequired().HasMaxLength(1000);
    }
}

/// <summary>
/// Entity configuration for SocialAccount.
/// </summary>
public class SocialAccountConfiguration : IEntityTypeConfiguration<SocialAccount>
{
    public void Configure(EntityTypeBuilder<SocialAccount> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HandleOrUrl).IsRequired().HasMaxLength(2000);
    }
}

/// <summary>
/// Entity configuration for ContactInfoLink.
/// </summary>
public class ContactInfoLinkConfiguration : IEntityTypeConfiguration<ContactInfoLink>
{
    public void Configure(EntityTypeBuilder<ContactInfoLink> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.OwnerType, e.OwnerId });
        builder.HasIndex(e => new { e.InfoKind, e.InfoId });

        builder.HasOne(e => e.Address)
            .WithMany()
            .HasForeignKey(e => e.AddressId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ContactDetail)
            .WithMany()
            .HasForeignKey(e => e.ContactDetailId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.SocialAccount)
            .WithMany()
            .HasForeignKey(e => e.SocialAccountId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// Entity configuration for PhoneNumber.
/// </summary>
public class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Number).IsRequired().HasMaxLength(30);
        builder.Property(e => e.CountryCode).HasMaxLength(5).HasDefaultValue("+1");
        builder.Property(e => e.AreaCode).HasMaxLength(10);
        builder.Property(e => e.Extension).HasMaxLength(10);
        builder.Property(e => e.FormattedNumber).HasMaxLength(50);
        builder.Property(e => e.Label).HasMaxLength(100);
        builder.Property(e => e.BestTimeToCall).HasMaxLength(100);
        builder.HasIndex(e => e.Number);
        builder.HasIndex(e => e.IsDeleted);
    }
}

/// <summary>
/// Entity configuration for EmailAddress.
/// </summary>
public class EmailAddressConfiguration : IEntityTypeConfiguration<EmailAddress>
{
    public void Configure(EntityTypeBuilder<EmailAddress> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.Label).HasMaxLength(100);
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.EmailEngagementScore).HasPrecision(3, 2);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => e.IsDeleted);
    }
}

/// <summary>
/// Entity configuration for SocialMediaAccount.
/// </summary>
public class SocialMediaAccountConfiguration : IEntityTypeConfiguration<SocialMediaAccount>
{
    public void Configure(EntityTypeBuilder<SocialMediaAccount> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.HandleOrUsername).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Platform).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.PlatformOther).HasMaxLength(100);
        builder.Property(e => e.AccountType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ProfileUrl).HasMaxLength(500);
        builder.Property(e => e.DisplayName).HasMaxLength(200);
        builder.Property(e => e.EngagementLevel).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => e.Platform);
        builder.HasIndex(e => e.HandleOrUsername);
        builder.HasIndex(e => e.IsDeleted);
    }
}

/// <summary>
/// Entity configuration for EntityAddressLink.
/// </summary>
public class EntityAddressLinkConfiguration : IEntityTypeConfiguration<EntityAddressLink>
{
    public void Configure(EntityTypeBuilder<EntityAddressLink> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.AddressType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(AddressType.Primary);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.AddressId, e.AddressType }).IsUnique();
        builder.HasOne(e => e.Address)
            .WithMany(a => a.EntityAddressLinks)
            .HasForeignKey(e => e.AddressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EntityPhoneLink.
/// </summary>
public class EntityPhoneLinkConfiguration : IEntityTypeConfiguration<EntityPhoneLink>
{
    public void Configure(EntityTypeBuilder<EntityPhoneLink> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.PhoneType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(PhoneType.Office);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.PhoneId, e.PhoneType }).IsUnique();
        builder.HasOne(e => e.PhoneNumber)
            .WithMany(p => p.EntityPhoneLinks)
            .HasForeignKey(e => e.PhoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EntityEmailLink.
/// </summary>
public class EntityEmailLinkConfiguration : IEntityTypeConfiguration<EntityEmailLink>
{
    public void Configure(EntityTypeBuilder<EntityEmailLink> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.EmailType).HasConversion<string>().HasMaxLength(50).HasDefaultValue(EmailType.General);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.EmailId, e.EmailType }).IsUnique();
        builder.HasOne(e => e.EmailAddress)
            .WithMany(e => e.EntityEmailLinks)
            .HasForeignKey(e => e.EmailId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Entity configuration for EntitySocialMediaLink.
/// </summary>
public class EntitySocialMediaLinkConfiguration : IEntityTypeConfiguration<EntitySocialMediaLink>
{
    public void Configure(EntityTypeBuilder<EntitySocialMediaLink> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EntityType).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.SocialMediaAccountId }).IsUnique();
        builder.HasOne(e => e.SocialMediaAccount)
            .WithMany(s => s.EntitySocialMediaLinks)
            .HasForeignKey(e => e.SocialMediaAccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
