// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Entities.ITSM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.ServiceDesk;

/// <summary>
/// Entity configuration for ServiceRequest.
/// </summary>
public class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ServiceRequestCategory.
/// </summary>
public class ServiceRequestCategoryConfiguration : IEntityTypeConfiguration<ServiceRequestCategory>
{
    public void Configure(EntityTypeBuilder<ServiceRequestCategory> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ServiceRequestSubcategory.
/// </summary>
public class ServiceRequestSubcategoryConfiguration : IEntityTypeConfiguration<ServiceRequestSubcategory>
{
    public void Configure(EntityTypeBuilder<ServiceRequestSubcategory> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ServiceRequestType.
/// </summary>
public class ServiceRequestTypeConfiguration : IEntityTypeConfiguration<ServiceRequestType>
{
    public void Configure(EntityTypeBuilder<ServiceRequestType> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ServiceRequestCustomFieldDefinition.
/// </summary>
public class ServiceRequestCustomFieldDefinitionConfiguration : IEntityTypeConfiguration<ServiceRequestCustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<ServiceRequestCustomFieldDefinition> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ServiceRequestCustomFieldValue.
/// </summary>
public class ServiceRequestCustomFieldValueConfiguration : IEntityTypeConfiguration<ServiceRequestCustomFieldValue>
{
    public void Configure(EntityTypeBuilder<ServiceRequestCustomFieldValue> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for ChangeBlackout (Change Management - ITSM module).
/// </summary>
public class ChangeBlackoutConfiguration : IEntityTypeConfiguration<ChangeBlackout>
{
    public void Configure(EntityTypeBuilder<ChangeBlackout> builder)
    {
        builder.HasKey(e => e.BlackoutId);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Reason).HasMaxLength(500);

        // Relationship: ChangeBlackout -> Change
        builder.HasOne(e => e.Change)
            .WithMany(c => c.Blackouts)
            .HasForeignKey(e => e.ChangeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship: ChangeBlackout -> User (CreatedBy)
        builder.HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete index
        builder.HasIndex(e => e.IsDeleted);
        // Date range index for blackout period queries
        builder.HasIndex(e => new { e.StartDate, e.EndDate }).HasDatabaseName("IX_ChangeBlackout_DateRange");
        // Change lookup index
        builder.HasIndex(e => e.ChangeId).HasDatabaseName("IX_ChangeBlackout_ChangeId");
    }
}
