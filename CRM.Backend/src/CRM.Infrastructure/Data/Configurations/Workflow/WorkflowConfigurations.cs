// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// AP-040: Workflow entity configurations extracted from CrmDbContext.OnModelCreating.
// Each class implements IEntityTypeConfiguration<T> — behavior is functionally identical
// to the previous inline modelBuilder.Entity<T>(entity => { ... }) lambdas.

using CRM.Core.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Data.Configurations.Workflow;

/// <summary>AP-040: Entity configuration for WorkflowDefinition.</summary>
public class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.WorkflowKey).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IconName).HasMaxLength(50);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.Tags).HasMaxLength(500);
        builder.Property(e => e.Metadata).HasColumnType("TEXT");
        builder.HasIndex(e => e.WorkflowKey).IsUnique();
        builder.HasIndex(e => e.EntityType);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.OwnerId);

        builder.HasOne(e => e.Owner)
            .WithMany()
            .HasForeignKey(e => e.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowVersion.</summary>
public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).HasMaxLength(50);
        builder.Property(e => e.ChangeLog).HasMaxLength(1000);
        builder.Property(e => e.CanvasLayout).HasColumnType("TEXT");
        builder.HasIndex(e => new { e.WorkflowDefinitionId, e.VersionNumber }).IsUnique();
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.WorkflowDefinition)
            .WithMany(d => d.Versions)
            .HasForeignKey(e => e.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PublishedBy)
            .WithMany()
            .HasForeignKey(e => e.PublishedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowNode.</summary>
public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NodeKey).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.NodeSubType).HasMaxLength(100);
        builder.Property(e => e.IconName).HasMaxLength(50);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.Configuration).HasColumnType("TEXT");
        builder.Property(e => e.PositionX).HasPrecision(10, 2);
        builder.Property(e => e.PositionY).HasPrecision(10, 2);
        builder.Property(e => e.Width).HasPrecision(10, 2);
        builder.Property(e => e.Height).HasPrecision(10, 2);
        builder.HasIndex(e => new { e.WorkflowVersionId, e.NodeKey }).IsUnique();
        builder.HasIndex(e => e.NodeType);
        builder.HasIndex(e => e.IsStartNode);
        builder.HasIndex(e => e.IsEndNode);

        builder.HasOne(e => e.WorkflowVersion)
            .WithMany(v => v.Nodes)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowTransition.</summary>
public class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TransitionKey).HasMaxLength(100);
        builder.Property(e => e.Label).HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.ConditionExpression).HasColumnType("TEXT");
        builder.Property(e => e.SourceHandle).HasMaxLength(20);
        builder.Property(e => e.TargetHandle).HasMaxLength(20);
        builder.Property(e => e.LineStyle).HasMaxLength(20);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.Property(e => e.AnimationStyle).HasMaxLength(20);
        builder.HasIndex(e => e.WorkflowVersionId);
        builder.HasIndex(e => e.SourceNodeId);
        builder.HasIndex(e => e.TargetNodeId);

        builder.HasOne(e => e.WorkflowVersion)
            .WithMany(v => v.Transitions)
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.SourceNode)
            .WithMany(n => n.OutgoingTransitions)
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TargetNode)
            .WithMany(n => n.IncomingTransitions)
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowInstance.</summary>
public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CorrelationId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TriggerEvent).HasMaxLength(100);
        builder.Property(e => e.InputData).HasColumnType("TEXT");
        builder.Property(e => e.StateData).HasColumnType("TEXT");
        builder.Property(e => e.OutputData).HasColumnType("TEXT");
        builder.Property(e => e.ErrorMessage).HasColumnType("TEXT");
        builder.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
        builder.Property(e => e.CancellationReason).HasMaxLength(500);
        builder.HasIndex(e => e.CorrelationId).IsUnique();
        builder.HasIndex(e => e.WorkflowDefinitionId);
        builder.HasIndex(e => e.WorkflowVersionId);
        builder.HasIndex(e => new { e.EntityType, e.EntityId });
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ScheduledAt);
        builder.HasIndex(e => e.NextRetryAt);

        builder.HasOne(e => e.WorkflowDefinition)
            .WithMany(d => d.Instances)
            .HasForeignKey(e => e.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.WorkflowVersion)
            .WithMany()
            .HasForeignKey(e => e.WorkflowVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CurrentNode)
            .WithMany()
            .HasForeignKey(e => e.CurrentNodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TriggeredBy)
            .WithMany()
            .HasForeignKey(e => e.TriggeredById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.ParentInstance)
            .WithMany(i => i.ChildInstances)
            .HasForeignKey(e => e.ParentInstanceId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowNodeInstance.</summary>
public class WorkflowNodeInstanceConfiguration : IEntityTypeConfiguration<WorkflowNodeInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowNodeInstance> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.InputData).HasColumnType("TEXT");
        builder.Property(e => e.OutputData).HasColumnType("TEXT");
        builder.Property(e => e.ErrorMessage).HasColumnType("TEXT");
        builder.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
        builder.Property(e => e.SkipReason).HasMaxLength(500);
        builder.Property(e => e.WorkerId).HasMaxLength(100);
        builder.HasIndex(e => e.WorkflowInstanceId);
        builder.HasIndex(e => e.WorkflowNodeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.NextRetryAt);

        builder.HasOne(e => e.WorkflowInstance)
            .WithMany(i => i.NodeInstances)
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkflowNode)
            .WithMany(n => n.NodeInstances)
            .HasForeignKey(e => e.WorkflowNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TransitionTaken)
            .WithMany()
            .HasForeignKey(e => e.TransitionTakenId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowTask.</summary>
public class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.QueueName).HasMaxLength(100);
        builder.Property(e => e.LockedByWorkerId).HasMaxLength(100);
        builder.Property(e => e.AssignedToRole).HasMaxLength(100);
        builder.Property(e => e.InputData).HasColumnType("TEXT");
        builder.Property(e => e.OutputData).HasColumnType("TEXT");
        builder.Property(e => e.FormSchema).HasColumnType("TEXT");
        builder.Property(e => e.FormData).HasColumnType("TEXT");
        builder.Property(e => e.ErrorMessage).HasColumnType("TEXT");
        builder.Property(e => e.ErrorStackTrace).HasColumnType("TEXT");
        builder.Property(e => e.DeadLetterReason).HasMaxLength(500);
        builder.HasIndex(e => e.WorkflowInstanceId);
        builder.HasIndex(e => e.WorkflowNodeId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.QueueName);
        builder.HasIndex(e => e.ScheduledAt);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.IsDeadLetter);
        builder.HasIndex(e => e.AssignedToId);
        builder.HasIndex(e => e.LockExpiresAt);

        builder.HasOne(e => e.WorkflowInstance)
            .WithMany(i => i.Tasks)
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkflowNode)
            .WithMany()
            .HasForeignKey(e => e.WorkflowNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.NodeInstance)
            .WithMany()
            .HasForeignKey(e => e.NodeInstanceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AssignedTo)
            .WithMany()
            .HasForeignKey(e => e.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowLog.</summary>
public class WorkflowLogConfiguration : IEntityTypeConfiguration<WorkflowLog>
{
    public void Configure(EntityTypeBuilder<WorkflowLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Details).HasColumnType("TEXT");
        builder.Property(e => e.WorkerId).HasMaxLength(100);
        builder.Property(e => e.ExceptionType).HasMaxLength(200);
        builder.Property(e => e.StackTrace).HasColumnType("TEXT");
        builder.HasIndex(e => e.WorkflowInstanceId);
        builder.HasIndex(e => e.WorkflowNodeId);
        builder.HasIndex(e => e.Level);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Category);

        builder.HasOne(e => e.WorkflowInstance)
            .WithMany(i => i.Logs)
            .HasForeignKey(e => e.WorkflowInstanceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkflowNode)
            .WithMany()
            .HasForeignKey(e => e.WorkflowNodeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.NodeInstance)
            .WithMany()
            .HasForeignKey(e => e.NodeInstanceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowSchedule.</summary>
public class WorkflowScheduleConfiguration : IEntityTypeConfiguration<WorkflowSchedule>
{
    public void Configure(EntityTypeBuilder<WorkflowSchedule> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.CronExpression).IsRequired().HasMaxLength(100);
        builder.Property(e => e.TimeZone).HasMaxLength(100);
        builder.Property(e => e.ContextData).HasColumnType("TEXT");
        builder.HasOne(e => e.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(e => e.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.WorkflowDefinitionId);
        builder.HasIndex(e => e.IsEnabled);
        builder.HasIndex(e => e.NextTriggerAt);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowJob.</summary>
public class WorkflowJobConfiguration : IEntityTypeConfiguration<WorkflowJob>
{
    public void Configure(EntityTypeBuilder<WorkflowJob> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.JobType).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Status).IsRequired().HasMaxLength(50);
        builder.Property(e => e.StepKey).HasMaxLength(200);
        builder.Property(e => e.Payload).HasColumnType("TEXT");
        builder.Property(e => e.ProcessingWorkerId).HasMaxLength(200);
        builder.Property(e => e.ErrorMessage).HasColumnType("TEXT");
        builder.Property(e => e.ResultData).HasColumnType("TEXT");
        builder.Property(e => e.CorrelationId).HasMaxLength(200);
        builder.HasOne(e => e.WorkflowInstance)
            .WithMany()
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.WorkflowTask)
            .WithMany()
            .HasForeignKey(e => e.WorkflowTaskId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ScheduledAt);
        builder.HasIndex(e => e.CorrelationId);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowContextVariable.</summary>
public class WorkflowContextVariableConfiguration : IEntityTypeConfiguration<WorkflowContextVariable>
{
    public void Configure(EntityTypeBuilder<WorkflowContextVariable> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Value).HasColumnType("TEXT");
        builder.Property(e => e.ValueType).HasMaxLength(50);
        builder.Property(e => e.SetByStepKey).HasMaxLength(200);
        builder.HasOne(e => e.WorkflowInstance)
            .WithMany()
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.WorkflowInstanceId, e.Key }).IsUnique();
    }
}

/// <summary>AP-040: Entity configuration for WorkflowAuditLog.</summary>
public class WorkflowAuditLogConfiguration : IEntityTypeConfiguration<WorkflowAuditLog>
{
    public void Configure(EntityTypeBuilder<WorkflowAuditLog> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ActorId).HasMaxLength(200);
        builder.Property(e => e.ActorName).HasMaxLength(200);
        builder.Property(e => e.Details).HasColumnType("TEXT");
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent).HasMaxLength(500);
        builder.HasOne(e => e.WorkflowInstance)
            .WithMany()
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.WorkflowInstanceId);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => e.Timestamp);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowMetric.</summary>
public class WorkflowMetricConfiguration : IEntityTypeConfiguration<WorkflowMetric>
{
    public void Configure(EntityTypeBuilder<WorkflowMetric> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.MetricType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.MetricName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.MetricValue).HasColumnType("decimal(18,4)");
        builder.Property(e => e.Dimensions).HasColumnType("TEXT");
        builder.HasOne(e => e.WorkflowDefinition)
            .WithMany()
            .HasForeignKey(e => e.WorkflowDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.WorkflowDefinitionId);
        builder.HasIndex(e => e.MetricType);
        builder.HasIndex(e => e.RecordedAt);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowLlmUsage.</summary>
public class WorkflowLlmUsageConfiguration : IEntityTypeConfiguration<WorkflowLlmUsage>
{
    public void Configure(EntityTypeBuilder<WorkflowLlmUsage> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Provider).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Model).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CostEstimate).HasColumnType("decimal(10,6)");
        builder.Property(e => e.ErrorMessage).HasColumnType("TEXT");
        builder.HasOne(e => e.WorkflowInstance)
            .WithMany()
            .HasForeignKey(e => e.WorkflowInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NodeInstance)
            .WithMany()
            .HasForeignKey(e => e.NodeInstanceId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => e.WorkflowInstanceId);
        builder.HasIndex(e => e.Provider);
    }
}

/// <summary>AP-040: Entity configuration for WorkflowCircuitBreakerState.</summary>
public class WorkflowCircuitBreakerStateConfiguration : IEntityTypeConfiguration<WorkflowCircuitBreakerState>
{
    public void Configure(EntityTypeBuilder<WorkflowCircuitBreakerState> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ServiceName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.State).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.ServiceName).IsUnique();
    }
}
