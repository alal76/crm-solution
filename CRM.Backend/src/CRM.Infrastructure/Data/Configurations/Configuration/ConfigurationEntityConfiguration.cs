// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CRM.Core.Entities;

namespace CRM.Infrastructure.Data.Configurations;

public class ProviderConfigurationConfiguration : IEntityTypeConfiguration<ProviderConfiguration>
{
    public void Configure(EntityTypeBuilder<ProviderConfiguration> builder)
    {
        builder.ToTable("ProviderConfigurations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConfigurationKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => x.ConfigurationKey)
            .IsUnique()
            .HasDatabaseName("UQ_ProviderConfiguration_Key");

        builder.Property(x => x.ConfigurationType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.ConfigurationType)
            .HasDatabaseName("IX_ProviderConfiguration_Type");

        builder.Property(x => x.ProviderName)
            .HasMaxLength(100);

        builder.HasIndex(x => x.ProviderName)
            .HasDatabaseName("IX_ProviderConfiguration_Provider");

        builder.Property(x => x.ConfigurationData)
            .IsRequired()
            .HasColumnType("LONGTEXT");

        builder.Property(x => x.IsEncrypted)
            .HasDefaultValue(true);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.LastTestedStatus)
            .HasMaxLength(20);

        builder.Property(x => x.LastTestedError)
            .HasColumnType("LONGTEXT");

        // Relationships
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ProviderConfiguration_CreatedBy");

        builder.HasOne(x => x.UpdatedByUser)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ProviderConfiguration_UpdatedBy");

        builder.HasMany(x => x.ChangeLogs)
            .WithOne(x => x.ProviderConfiguration)
            .HasForeignKey(x => x.ProviderConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Base entity properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class ConfigurationChangeLogConfiguration : IEntityTypeConfiguration<ConfigurationChangeLog>
{
    public void Configure(EntityTypeBuilder<ConfigurationChangeLog> builder)
    {
        builder.ToTable("ConfigurationChangeLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ConfigurationKey)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(x => new { x.ConfigurationKey, x.ChangedAt })
            .HasDatabaseName("IX_ConfigLog_Key_Date");

        builder.Property(x => x.OldValue)
            .HasColumnType("LONGTEXT");

        builder.Property(x => x.NewValue)
            .HasColumnType("LONGTEXT");

        builder.Property(x => x.ChangeType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ChangedAt)
            .IsRequired();

        builder.HasIndex(x => x.ChangedAt)
            .HasDatabaseName("IX_ConfigLog_ChangedAt");

        builder.Property(x => x.IpAddress)
            .HasMaxLength(50);

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.ChangedByUser)
            .WithMany()
            .HasForeignKey(x => x.ChangedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_ConfigLog_ChangedBy");

        builder.HasOne(x => x.ProviderConfiguration)
            .WithMany(x => x.ChangeLogs)
            .HasForeignKey(x => x.ProviderConfigurationId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_ConfigLog_ProviderConfig");

        // Base entity properties
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
