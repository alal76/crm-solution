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

using CRM.Core.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Workflow;

/// <summary>
/// Entity configuration for WorkflowDefinition.
/// </summary>
public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowVersion.
/// </summary>
public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowNode.
/// </summary>
public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowTransition.
/// </summary>
public class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowInstance.
/// </summary>
public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowNodeInstance.
/// </summary>
public class WorkflowNodeInstanceConfiguration : IEntityTypeConfiguration<WorkflowNodeInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowNodeInstance> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowTask.
/// </summary>
public class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.HasKey(e => e.Id);
    }
}

/// <summary>
/// Entity configuration for WorkflowLog.
/// </summary>
public class WorkflowLogConfiguration : IEntityTypeConfiguration<WorkflowLog>
{
    public void Configure(EntityTypeBuilder<WorkflowLog> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
