// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Licensed under AGPL-3.0

using CRM.Core.Entities;
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
