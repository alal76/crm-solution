// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.AI;

#region AIAgentConfiguration

/// <summary>
/// EF Core entity type configuration for the <see cref="AIAgent"/> entity.
/// Maps to the "AIAgents" table with indexes on Name, AgentType, and IsActive.
/// </summary>
public class AIAgentConfiguration : IEntityTypeConfiguration<AIAgent>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AIAgent> builder)
    {
        builder.ToTable("AIAgents");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.AgentType);
        builder.HasIndex(e => e.IsActive);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.SystemPrompt)
            .IsRequired();

        builder.Property(e => e.AllowedPlugins)
            .HasMaxLength(2000);

        builder.Property(e => e.ApprovalTier)
            .HasMaxLength(20);

        builder.Property(e => e.ModelOverride)
            .HasMaxLength(100);

        builder.Property(e => e.Temperature)
            .HasDefaultValue(0.3);

        builder.Property(e => e.MaxTokens)
            .HasDefaultValue(4096);

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.Conversations)
            .WithOne(c => c.Agent)
            .HasForeignKey(c => c.AgentId);

        builder.HasMany(e => e.Memories)
            .WithOne(m => m.Agent)
            .HasForeignKey(m => m.AgentId);
    }
}

#endregion

#region AgentConversationConfiguration

/// <summary>
/// EF Core entity type configuration for the <see cref="AgentConversation"/> entity.
/// Maps to the "AgentConversations" table with indexes on AgentId, UserId, Status, and EntityType+EntityId.
/// </summary>
public class AgentConversationConfiguration : IEntityTypeConfiguration<AgentConversation>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentConversation> builder)
    {
        builder.ToTable("AgentConversations");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.AgentId);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });

        builder.Property(e => e.Messages)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasMaxLength(100);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasMany(e => e.Actions)
            .WithOne(a => a.Conversation)
            .HasForeignKey(a => a.ConversationId);
    }
}

#endregion

#region AgentActionConfiguration

/// <summary>
/// EF Core entity type configuration for the <see cref="AgentAction"/> entity.
/// Maps to the "AgentActions" table with indexes on ConversationId, AgentId, and Status.
/// </summary>
public class AgentActionConfiguration : IEntityTypeConfiguration<AgentAction>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentAction> builder)
    {
        builder.ToTable("AgentActions");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.ConversationId);
        builder.HasIndex(e => e.AgentId);
        builder.HasIndex(e => e.Status);

        builder.Property(e => e.PluginName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FunctionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder.HasOne(e => e.ApprovalRequest)
            .WithOne(a => a.AgentAction)
            .HasForeignKey<AgentApprovalRequest>(a => a.AgentActionId);
    }
}

#endregion

#region AgentMemoryConfiguration

/// <summary>
/// EF Core entity type configuration for the <see cref="AgentMemory"/> entity.
/// Maps to the "AgentMemories" table with a unique composite index on AgentId+Key.
/// </summary>
public class AgentMemoryConfiguration : IEntityTypeConfiguration<AgentMemory>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentMemory> builder)
    {
        builder.ToTable("AgentMemories");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.AgentId);
        builder.HasIndex(e => new { e.AgentId, e.Key }).IsUnique();
        builder.HasIndex(e => new { e.EntityType, e.EntityId });

        builder.Property(e => e.Key)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Value)
            .IsRequired();

        builder.Property(e => e.EntityType)
            .HasMaxLength(100);

        builder.Property(e => e.Confidence)
            .HasDefaultValue(1.0);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

#endregion

#region AgentApprovalRequestConfiguration

/// <summary>
/// EF Core entity type configuration for the <see cref="AgentApprovalRequest"/> entity.
/// Maps to the "AgentApprovalRequests" table with indexes on Status, RequestedByUserId, and ConversationId.
/// </summary>
public class AgentApprovalRequestConfiguration : IEntityTypeConfiguration<AgentApprovalRequest>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AgentApprovalRequest> builder)
    {
        builder.ToTable("AgentApprovalRequests");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => e.ConversationId);

        builder.Property(e => e.ActionDescription)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(e => e.PluginName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.FunctionName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ApprovalTier)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("low");

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

#endregion
