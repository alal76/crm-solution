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
