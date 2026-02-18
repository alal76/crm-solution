// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Entities.Workflow;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Workflow entities and related enums.
/// ~100 tests covering workflow definitions, versions, nodes, transitions, instances, tasks, logs.
/// </summary>
public class WorkflowEntityTests
{
    #region WorkflowStatus Enum Tests

    [Fact]
    public void WorkflowStatus_ShouldHaveCorrectValues()
    {
        ((int)WorkflowStatus.Draft).Should().Be(0);
        ((int)WorkflowStatus.Active).Should().Be(1);
        ((int)WorkflowStatus.Paused).Should().Be(2);
        ((int)WorkflowStatus.Archived).Should().Be(3);
        ((int)WorkflowStatus.Deprecated).Should().Be(4);
    }

    [Fact]
    public void WorkflowStatus_ShouldHave5Values()
    {
        var values = Enum.GetValues<WorkflowStatus>();
        values.Should().HaveCount(5);
    }

    #endregion

    #region WorkflowVersionStatus Enum Tests

    [Fact]
    public void WorkflowVersionStatus_ShouldHaveCorrectValues()
    {
        ((int)WorkflowVersionStatus.Draft).Should().Be(0);
        ((int)WorkflowVersionStatus.Active).Should().Be(1);
        ((int)WorkflowVersionStatus.Deprecated).Should().Be(2);
    }

    [Fact]
    public void WorkflowVersionStatus_ShouldHave3Values()
    {
        var values = Enum.GetValues<WorkflowVersionStatus>();
        values.Should().HaveCount(3);
    }

    #endregion

    #region WorkflowInstanceStatus Enum Tests

    [Fact]
    public void WorkflowInstanceStatus_ShouldHaveCorrectValues()
    {
        ((int)WorkflowInstanceStatus.Pending).Should().Be(0);
        ((int)WorkflowInstanceStatus.Running).Should().Be(1);
        ((int)WorkflowInstanceStatus.Waiting).Should().Be(2);
        ((int)WorkflowInstanceStatus.Paused).Should().Be(3);
        ((int)WorkflowInstanceStatus.Completed).Should().Be(4);
        ((int)WorkflowInstanceStatus.Failed).Should().Be(5);
        ((int)WorkflowInstanceStatus.Cancelled).Should().Be(6);
        ((int)WorkflowInstanceStatus.TimedOut).Should().Be(7);
        ((int)WorkflowInstanceStatus.Suspended).Should().Be(8);
    }

    [Fact]
    public void WorkflowInstanceStatus_ShouldHave9Values()
    {
        var values = Enum.GetValues<WorkflowInstanceStatus>();
        values.Should().HaveCount(9);
    }

    #endregion

    #region WorkflowNodeType Enum Tests

    [Fact]
    public void WorkflowNodeType_ShouldHaveCorrectStandardValues()
    {
        ((int)WorkflowNodeType.Trigger).Should().Be(0);
        ((int)WorkflowNodeType.Condition).Should().Be(1);
        ((int)WorkflowNodeType.Action).Should().Be(2);
        ((int)WorkflowNodeType.HumanTask).Should().Be(3);
        ((int)WorkflowNodeType.Wait).Should().Be(4);
        ((int)WorkflowNodeType.ParallelGateway).Should().Be(5);
        ((int)WorkflowNodeType.JoinGateway).Should().Be(6);
        ((int)WorkflowNodeType.Subprocess).Should().Be(7);
        ((int)WorkflowNodeType.LLMAction).Should().Be(8);
        ((int)WorkflowNodeType.End).Should().Be(9);
    }

    [Fact]
    public void WorkflowNodeType_ShouldHaveAIEnhancedValues()
    {
        ((int)WorkflowNodeType.AIDecision).Should().Be(10);
        ((int)WorkflowNodeType.AIAgent).Should().Be(11);
        ((int)WorkflowNodeType.AIContentGenerator).Should().Be(12);
        ((int)WorkflowNodeType.AIDataExtractor).Should().Be(13);
        ((int)WorkflowNodeType.AIClassifier).Should().Be(14);
        ((int)WorkflowNodeType.AISentimentAnalyzer).Should().Be(15);
        ((int)WorkflowNodeType.HumanReview).Should().Be(16);
    }

    [Fact]
    public void WorkflowNodeType_ShouldHave17Values()
    {
        var values = Enum.GetValues<WorkflowNodeType>();
        values.Should().HaveCount(17);
    }

    #endregion

    #region WorkflowNodeInstanceStatus Enum Tests

    [Fact]
    public void WorkflowNodeInstanceStatus_ShouldHaveCorrectValues()
    {
        ((int)WorkflowNodeInstanceStatus.Pending).Should().Be(0);
        ((int)WorkflowNodeInstanceStatus.Running).Should().Be(1);
        ((int)WorkflowNodeInstanceStatus.Waiting).Should().Be(2);
        ((int)WorkflowNodeInstanceStatus.Completed).Should().Be(3);
        ((int)WorkflowNodeInstanceStatus.Failed).Should().Be(4);
        ((int)WorkflowNodeInstanceStatus.Skipped).Should().Be(5);
        ((int)WorkflowNodeInstanceStatus.Cancelled).Should().Be(6);
        ((int)WorkflowNodeInstanceStatus.Retrying).Should().Be(7);
    }

    [Fact]
    public void WorkflowNodeInstanceStatus_ShouldHave8Values()
    {
        var values = Enum.GetValues<WorkflowNodeInstanceStatus>();
        values.Should().HaveCount(8);
    }

    #endregion

    #region TransitionConditionType Enum Tests

    [Fact]
    public void TransitionConditionType_ShouldHaveCorrectValues()
    {
        ((int)TransitionConditionType.Always).Should().Be(0);
        ((int)TransitionConditionType.Expression).Should().Be(1);
        ((int)TransitionConditionType.FieldMatch).Should().Be(2);
        ((int)TransitionConditionType.Any).Should().Be(3);
        ((int)TransitionConditionType.All).Should().Be(4);
        ((int)TransitionConditionType.UserChoice).Should().Be(5);
    }

    [Fact]
    public void TransitionConditionType_ShouldHave6Values()
    {
        var values = Enum.GetValues<TransitionConditionType>();
        values.Should().HaveCount(6);
    }

    #endregion

    #region WorkflowTaskType Enum Tests

    [Fact]
    public void WorkflowTaskType_ShouldHaveCorrectValues()
    {
        ((int)WorkflowTaskType.Automated).Should().Be(0);
        ((int)WorkflowTaskType.Human).Should().Be(1);
        ((int)WorkflowTaskType.Timer).Should().Be(2);
        ((int)WorkflowTaskType.Event).Should().Be(3);
        ((int)WorkflowTaskType.LLM).Should().Be(4);
    }

    [Fact]
    public void WorkflowTaskType_ShouldHave5Values()
    {
        var values = Enum.GetValues<WorkflowTaskType>();
        values.Should().HaveCount(5);
    }

    #endregion

    #region WorkflowTaskStatus Enum Tests

    [Fact]
    public void WorkflowTaskStatus_ShouldHaveCorrectValues()
    {
        ((int)WorkflowTaskStatus.Pending).Should().Be(0);
        ((int)WorkflowTaskStatus.Locked).Should().Be(1);
        ((int)WorkflowTaskStatus.Running).Should().Be(2);
        ((int)WorkflowTaskStatus.Waiting).Should().Be(3);
        ((int)WorkflowTaskStatus.Completed).Should().Be(4);
        ((int)WorkflowTaskStatus.Failed).Should().Be(5);
        ((int)WorkflowTaskStatus.Retrying).Should().Be(6);
        ((int)WorkflowTaskStatus.Cancelled).Should().Be(7);
        ((int)WorkflowTaskStatus.Skipped).Should().Be(8);
        ((int)WorkflowTaskStatus.DeadLetter).Should().Be(9);
    }

    [Fact]
    public void WorkflowTaskStatus_ShouldHave10Values()
    {
        var values = Enum.GetValues<WorkflowTaskStatus>();
        values.Should().HaveCount(10);
    }

    #endregion

    #region WorkflowLogLevel Enum Tests

    [Fact]
    public void WorkflowLogLevel_ShouldHaveCorrectValues()
    {
        ((int)WorkflowLogLevel.Debug).Should().Be(0);
        ((int)WorkflowLogLevel.Info).Should().Be(1);
        ((int)WorkflowLogLevel.Warning).Should().Be(2);
        ((int)WorkflowLogLevel.Error).Should().Be(3);
    }

    [Fact]
    public void WorkflowLogLevel_ShouldHaveAtLeast4Values()
    {
        var values = Enum.GetValues<WorkflowLogLevel>();
        values.Length.Should().BeGreaterOrEqualTo(4);
    }

    #endregion

    #region WorkflowDefinition Entity Tests

    [Fact]
    public void WorkflowDefinition_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var definition = new WorkflowDefinition();

        // Assert
        definition.WorkflowKey.Should().BeEmpty();
        definition.Name.Should().BeEmpty();
        definition.EntityType.Should().BeEmpty();
        definition.Status.Should().Be(WorkflowStatus.Draft);
        definition.CurrentVersion.Should().Be(1);
        definition.IconName.Should().Be("AccountTree");
        definition.Color.Should().Be("#6750A4");
        definition.IsSystem.Should().BeFalse();
        definition.Priority.Should().Be(100);
        definition.MaxConcurrentInstances.Should().Be(0);
        definition.DefaultTimeoutHours.Should().Be(0);
        definition.Versions.Should().BeEmpty();
        definition.Instances.Should().BeEmpty();
    }

    [Fact]
    public void WorkflowDefinition_ShouldAllowSettingProperties()
    {
        // Arrange
        var definition = new WorkflowDefinition
        {
            Id = 1,
            WorkflowKey = "lead-qualification",
            Name = "Lead Qualification Process",
            Description = "Automated lead qualification workflow",
            Category = "Sales",
            EntityType = "Lead",
            Status = WorkflowStatus.Active,
            CurrentVersion = 3,
            IconName = "FilterAlt",
            Color = "#4CAF50",
            IsSystem = false,
            Priority = 50,
            MaxConcurrentInstances = 100,
            DefaultTimeoutHours = 24,
            Tags = "sales,lead,qualification"
        };

        // Assert
        definition.Id.Should().Be(1);
        definition.WorkflowKey.Should().Be("lead-qualification");
        definition.Name.Should().Be("Lead Qualification Process");
        definition.Description.Should().Contain("qualification");
        definition.Category.Should().Be("Sales");
        definition.EntityType.Should().Be("Lead");
        definition.Status.Should().Be(WorkflowStatus.Active);
        definition.CurrentVersion.Should().Be(3);
        definition.IconName.Should().Be("FilterAlt");
        definition.Color.Should().Be("#4CAF50");
        definition.Priority.Should().Be(50);
        definition.MaxConcurrentInstances.Should().Be(100);
        definition.DefaultTimeoutHours.Should().Be(24);
        definition.Tags.Should().Contain("sales");
    }

    [Fact]
    public void WorkflowDefinition_ShouldSupportNavigationProperties()
    {
        // Arrange
        var owner = new User { Id = 1, FirstName = "Admin" };
        var definition = new WorkflowDefinition
        {
            OwnerId = 1,
            Owner = owner
        };

        // Assert
        definition.OwnerId.Should().Be(1);
        definition.Owner.Should().Be(owner);
    }

    [Theory]
    [InlineData(WorkflowStatus.Draft)]
    [InlineData(WorkflowStatus.Active)]
    [InlineData(WorkflowStatus.Paused)]
    [InlineData(WorkflowStatus.Archived)]
    [InlineData(WorkflowStatus.Deprecated)]
    public void WorkflowDefinition_ShouldAcceptAllStatuses(WorkflowStatus status)
    {
        // Arrange & Act
        var definition = new WorkflowDefinition { Status = status };

        // Assert
        definition.Status.Should().Be(status);
    }

    #endregion

    #region WorkflowVersion Entity Tests

    [Fact]
    public void WorkflowVersion_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var version = new WorkflowVersion();

        // Assert
        version.Status.Should().Be(WorkflowVersionStatus.Draft);
        version.Nodes.Should().BeEmpty();
        version.Transitions.Should().BeEmpty();
    }

    [Fact]
    public void WorkflowVersion_ShouldAllowSettingProperties()
    {
        // Arrange
        var version = new WorkflowVersion
        {
            Id = 1,
            WorkflowDefinitionId = 10,
            VersionNumber = 2,
            Label = "v2.0",
            ChangeLog = "Added AI decision node",
            Status = WorkflowVersionStatus.Active,
            PublishedAt = DateTime.UtcNow.AddDays(-1),
            PublishedById = 5,
            CanvasLayout = "{\"zoom\": 1.0, \"offset\": {\"x\": 0, \"y\": 0}}"
        };

        // Assert
        version.Id.Should().Be(1);
        version.WorkflowDefinitionId.Should().Be(10);
        version.VersionNumber.Should().Be(2);
        version.Label.Should().Be("v2.0");
        version.ChangeLog.Should().Contain("AI decision");
        version.Status.Should().Be(WorkflowVersionStatus.Active);
        version.PublishedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
        version.PublishedById.Should().Be(5);
    }

    [Fact]
    public void WorkflowVersion_ShouldSupportNodesCollection()
    {
        // Arrange
        var version = new WorkflowVersion();
        var node1 = new WorkflowNode { Id = 1, Name = "Start" };
        var node2 = new WorkflowNode { Id = 2, Name = "Action" };

        // Act
        version.Nodes.Add(node1);
        version.Nodes.Add(node2);

        // Assert
        version.Nodes.Should().HaveCount(2);
    }

    #endregion

    #region WorkflowNode Entity Tests

    [Fact]
    public void WorkflowNode_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var node = new WorkflowNode();

        // Assert
        node.NodeKey.Should().BeEmpty();
        node.Name.Should().BeEmpty();
        node.Width.Should().Be(200);
        node.Height.Should().Be(80);
        node.IconName.Should().Be("Circle");
        node.Color.Should().Be("#6750A4");
        node.IsStartNode.Should().BeFalse();
        node.IsEndNode.Should().BeFalse();
        node.TimeoutMinutes.Should().Be(0);
        node.RetryCount.Should().Be(0);
        node.RetryDelaySeconds.Should().Be(60);
        node.UseExponentialBackoff.Should().BeTrue();
        node.ExecutionOrder.Should().Be(0);
        node.OutgoingTransitions.Should().BeEmpty();
        node.IncomingTransitions.Should().BeEmpty();
    }

    [Fact]
    public void WorkflowNode_ShouldAllowSettingProperties()
    {
        // Arrange
        var node = new WorkflowNode
        {
            Id = 1,
            WorkflowVersionId = 5,
            NodeKey = "check-lead-score",
            Name = "Check Lead Score",
            Description = "Evaluates lead score against threshold",
            NodeType = WorkflowNodeType.Condition,
            NodeSubType = "FieldComparison",
            PositionX = 500,
            PositionY = 200,
            Width = 250,
            Height = 100,
            IconName = "Assessment",
            Color = "#FF9800",
            IsStartNode = false,
            IsEndNode = false,
            Configuration = "{\"field\": \"Score\", \"operator\": \">=\", \"value\": 80}",
            TimeoutMinutes = 5,
            RetryCount = 2,
            RetryDelaySeconds = 30,
            UseExponentialBackoff = false,
            ExecutionOrder = 3
        };

        // Assert
        node.NodeKey.Should().Be("check-lead-score");
        node.Name.Should().Be("Check Lead Score");
        node.NodeType.Should().Be(WorkflowNodeType.Condition);
        node.NodeSubType.Should().Be("FieldComparison");
        node.PositionX.Should().Be(500);
        node.PositionY.Should().Be(200);
        node.Width.Should().Be(250);
        node.TimeoutMinutes.Should().Be(5);
        node.RetryCount.Should().Be(2);
        node.Configuration.Should().Contain("Score");
    }

    [Theory]
    [InlineData(WorkflowNodeType.Trigger)]
    [InlineData(WorkflowNodeType.Condition)]
    [InlineData(WorkflowNodeType.Action)]
    [InlineData(WorkflowNodeType.HumanTask)]
    [InlineData(WorkflowNodeType.Wait)]
    [InlineData(WorkflowNodeType.ParallelGateway)]
    [InlineData(WorkflowNodeType.JoinGateway)]
    [InlineData(WorkflowNodeType.Subprocess)]
    [InlineData(WorkflowNodeType.LLMAction)]
    [InlineData(WorkflowNodeType.End)]
    [InlineData(WorkflowNodeType.AIDecision)]
    [InlineData(WorkflowNodeType.AIAgent)]
    [InlineData(WorkflowNodeType.AIContentGenerator)]
    [InlineData(WorkflowNodeType.AIDataExtractor)]
    [InlineData(WorkflowNodeType.AIClassifier)]
    [InlineData(WorkflowNodeType.AISentimentAnalyzer)]
    [InlineData(WorkflowNodeType.HumanReview)]
    public void WorkflowNode_ShouldAcceptAllNodeTypes(WorkflowNodeType nodeType)
    {
        // Arrange & Act
        var node = new WorkflowNode { NodeType = nodeType };

        // Assert
        node.NodeType.Should().Be(nodeType);
    }

    [Fact]
    public void WorkflowNode_ShouldSupportStartAndEndNodeFlags()
    {
        // Arrange
        var startNode = new WorkflowNode
        {
            NodeType = WorkflowNodeType.Trigger,
            IsStartNode = true,
            IsEndNode = false
        };

        var endNode = new WorkflowNode
        {
            NodeType = WorkflowNodeType.End,
            IsStartNode = false,
            IsEndNode = true
        };

        // Assert
        startNode.IsStartNode.Should().BeTrue();
        startNode.IsEndNode.Should().BeFalse();
        endNode.IsStartNode.Should().BeFalse();
        endNode.IsEndNode.Should().BeTrue();
    }

    #endregion

    #region WorkflowTransition Entity Tests

    [Fact]
    public void WorkflowTransition_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var transition = new WorkflowTransition();

        // Assert
        transition.ConditionType.Should().Be(TransitionConditionType.Always);
        transition.IsDefault.Should().BeFalse();
        transition.Priority.Should().Be(100);
        transition.SourceHandle.Should().Be("right");
        transition.TargetHandle.Should().Be("left");
        transition.LineStyle.Should().Be("solid");
        transition.Color.Should().Be("#888888");
        transition.AnimationStyle.Should().Be("none");
    }

    [Fact]
    public void WorkflowTransition_ShouldAllowSettingProperties()
    {
        // Arrange
        var transition = new WorkflowTransition
        {
            Id = 1,
            WorkflowVersionId = 5,
            SourceNodeId = 10,
            TargetNodeId = 20,
            TransitionKey = "qualified-path",
            Label = "Score >= 80",
            Description = "Path for qualified leads",
            ConditionType = TransitionConditionType.Expression,
            ConditionExpression = "entity.Score >= 80",
            IsDefault = false,
            Priority = 10,
            SourceHandle = "bottom",
            TargetHandle = "top",
            LineStyle = "dashed",
            Color = "#4CAF50",
            AnimationStyle = "flow"
        };

        // Assert
        transition.SourceNodeId.Should().Be(10);
        transition.TargetNodeId.Should().Be(20);
        transition.TransitionKey.Should().Be("qualified-path");
        transition.Label.Should().Be("Score >= 80");
        transition.ConditionType.Should().Be(TransitionConditionType.Expression);
        transition.ConditionExpression.Should().Contain("entity.Score");
        transition.Priority.Should().Be(10);
        transition.LineStyle.Should().Be("dashed");
        transition.AnimationStyle.Should().Be("flow");
    }

    [Theory]
    [InlineData(TransitionConditionType.Always)]
    [InlineData(TransitionConditionType.Expression)]
    [InlineData(TransitionConditionType.FieldMatch)]
    [InlineData(TransitionConditionType.Any)]
    [InlineData(TransitionConditionType.All)]
    [InlineData(TransitionConditionType.UserChoice)]
    public void WorkflowTransition_ShouldAcceptAllConditionTypes(TransitionConditionType conditionType)
    {
        // Arrange & Act
        var transition = new WorkflowTransition { ConditionType = conditionType };

        // Assert
        transition.ConditionType.Should().Be(conditionType);
    }

    #endregion

    #region WorkflowInstance Entity Tests

    [Fact]
    public void WorkflowInstance_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var instance = new WorkflowInstance();

        // Assert
        instance.CorrelationId.Should().NotBeNullOrEmpty();
        instance.EntityType.Should().BeEmpty();
        instance.Status.Should().Be(WorkflowInstanceStatus.Pending);
        instance.Priority.Should().Be(100);
        instance.RetryCount.Should().Be(0);
        instance.MaxRetries.Should().Be(3);
        instance.IsCancelled.Should().BeFalse();
        instance.ChildInstances.Should().BeEmpty();
        instance.NodeInstances.Should().BeEmpty();
        instance.Tasks.Should().BeEmpty();
        instance.Logs.Should().BeEmpty();
    }

    [Fact]
    public void WorkflowInstance_ShouldHaveUniqueCorrelationId()
    {
        // Arrange & Act
        var instance1 = new WorkflowInstance();
        var instance2 = new WorkflowInstance();

        // Assert
        instance1.CorrelationId.Should().NotBe(instance2.CorrelationId);
    }

    [Fact]
    public void WorkflowInstance_ShouldAllowSettingProperties()
    {
        // Arrange
        var startedAt = DateTime.UtcNow.AddMinutes(-10);
        var instance = new WorkflowInstance
        {
            Id = 1,
            WorkflowDefinitionId = 5,
            WorkflowVersionId = 10,
            CorrelationId = "corr-123",
            EntityType = "Lead",
            EntityId = 100,
            Status = WorkflowInstanceStatus.Running,
            CurrentNodeId = 15,
            StartedAt = startedAt,
            Priority = 50,
            TriggerEvent = "OnCreate",
            TriggeredById = 3,
            InputData = "{\"leadId\": 100}",
            StateData = "{\"currentStep\": 2}",
            MaxRetries = 5,
            TimeoutAt = DateTime.UtcNow.AddHours(24)
        };

        // Assert
        instance.WorkflowDefinitionId.Should().Be(5);
        instance.WorkflowVersionId.Should().Be(10);
        instance.CorrelationId.Should().Be("corr-123");
        instance.EntityType.Should().Be("Lead");
        instance.EntityId.Should().Be(100);
        instance.Status.Should().Be(WorkflowInstanceStatus.Running);
        instance.CurrentNodeId.Should().Be(15);
        instance.StartedAt.Should().Be(startedAt);
        instance.TriggerEvent.Should().Be("OnCreate");
        instance.InputData.Should().Contain("leadId");
    }

    [Fact]
    public void WorkflowInstance_ShouldSupportErrorTracking()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Status = WorkflowInstanceStatus.Failed,
            ErrorMessage = "Timeout waiting for external event",
            ErrorStackTrace = "at WorkflowEngine.Execute(...)"
        };

        // Assert
        instance.Status.Should().Be(WorkflowInstanceStatus.Failed);
        instance.ErrorMessage.Should().Contain("Timeout");
        instance.ErrorStackTrace.Should().Contain("WorkflowEngine");
    }

    [Fact]
    public void WorkflowInstance_ShouldSupportRetryLogic()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            RetryCount = 2,
            MaxRetries = 3,
            NextRetryAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Assert
        instance.RetryCount.Should().Be(2);
        instance.MaxRetries.Should().Be(3);
        instance.NextRetryAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void WorkflowInstance_ShouldSupportCancellation()
    {
        // Arrange
        var instance = new WorkflowInstance
        {
            Status = WorkflowInstanceStatus.Cancelled,
            IsCancelled = true,
            CancellationReason = "User requested cancellation"
        };

        // Assert
        instance.Status.Should().Be(WorkflowInstanceStatus.Cancelled);
        instance.IsCancelled.Should().BeTrue();
        instance.CancellationReason.Should().Contain("User requested");
    }

    [Fact]
    public void WorkflowInstance_ShouldSupportSubprocessHierarchy()
    {
        // Arrange
        var parentInstance = new WorkflowInstance { Id = 1 };
        var childInstance = new WorkflowInstance
        {
            Id = 2,
            ParentInstanceId = 1,
            ParentInstance = parentInstance
        };
        parentInstance.ChildInstances.Add(childInstance);

        // Assert
        childInstance.ParentInstanceId.Should().Be(1);
        childInstance.ParentInstance.Should().Be(parentInstance);
        parentInstance.ChildInstances.Should().Contain(childInstance);
    }

    [Theory]
    [InlineData(WorkflowInstanceStatus.Pending)]
    [InlineData(WorkflowInstanceStatus.Running)]
    [InlineData(WorkflowInstanceStatus.Waiting)]
    [InlineData(WorkflowInstanceStatus.Paused)]
    [InlineData(WorkflowInstanceStatus.Completed)]
    [InlineData(WorkflowInstanceStatus.Failed)]
    [InlineData(WorkflowInstanceStatus.Cancelled)]
    [InlineData(WorkflowInstanceStatus.TimedOut)]
    [InlineData(WorkflowInstanceStatus.Suspended)]
    public void WorkflowInstance_ShouldAcceptAllStatuses(WorkflowInstanceStatus status)
    {
        // Arrange & Act
        var instance = new WorkflowInstance { Status = status };

        // Assert
        instance.Status.Should().Be(status);
    }

    #endregion

    #region WorkflowTask Entity Tests

    [Fact]
    public void WorkflowTask_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var task = new WorkflowTask();

        // Assert
        task.Name.Should().BeEmpty();
        task.Status.Should().Be(WorkflowTaskStatus.Pending);
        task.Priority.Should().Be(100);
        task.QueueName.Should().Be("default");
        task.RetryCount.Should().Be(0);
        task.MaxRetries.Should().Be(3);
        task.IsDeadLetter.Should().BeFalse();
    }

    [Fact]
    public void WorkflowTask_ShouldAllowSettingAutomatedTaskProperties()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Id = 1,
            WorkflowInstanceId = 5,
            WorkflowNodeId = 10,
            TaskType = WorkflowTaskType.Automated,
            Name = "Send Email Notification",
            Description = "Send email to lead",
            Status = WorkflowTaskStatus.Running,
            Priority = 50,
            QueueName = "email-queue",
            ScheduledAt = DateTime.UtcNow,
            StartedAt = DateTime.UtcNow,
            InputData = "{\"to\": \"lead@example.com\", \"template\": \"welcome\"}",
            LockedByWorkerId = "worker-001",
            LockExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Assert
        task.TaskType.Should().Be(WorkflowTaskType.Automated);
        task.Name.Should().Be("Send Email Notification");
        task.QueueName.Should().Be("email-queue");
        task.LockedByWorkerId.Should().Be("worker-001");
        task.InputData.Should().Contain("lead@example.com");
    }

    [Fact]
    public void WorkflowTask_ShouldAllowSettingHumanTaskProperties()
    {
        // Arrange
        var task = new WorkflowTask
        {
            TaskType = WorkflowTaskType.Human,
            Name = "Review Lead Details",
            AssignedToId = 10,
            AssignedToRole = "SalesRep",
            DueAt = DateTime.UtcNow.AddHours(4),
            FormSchema = "{\"fields\": [{\"name\": \"approved\", \"type\": \"boolean\"}]}",
            FormData = "{\"approved\": true}"
        };

        // Assert
        task.TaskType.Should().Be(WorkflowTaskType.Human);
        task.AssignedToId.Should().Be(10);
        task.AssignedToRole.Should().Be("SalesRep");
        task.DueAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(4), TimeSpan.FromMinutes(1));
        task.FormSchema.Should().Contain("approved");
        task.FormData.Should().Contain("true");
    }

    [Fact]
    public void WorkflowTask_ShouldSupportDeadLetterQueue()
    {
        // Arrange
        var task = new WorkflowTask
        {
            Status = WorkflowTaskStatus.DeadLetter,
            IsDeadLetter = true,
            DeadLetterReason = "Exceeded maximum retries",
            DeadLetterAt = DateTime.UtcNow,
            RetryCount = 3,
            MaxRetries = 3
        };

        // Assert
        task.IsDeadLetter.Should().BeTrue();
        task.DeadLetterReason.Should().Contain("maximum retries");
        task.DeadLetterAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData(WorkflowTaskType.Automated)]
    [InlineData(WorkflowTaskType.Human)]
    [InlineData(WorkflowTaskType.Timer)]
    [InlineData(WorkflowTaskType.Event)]
    [InlineData(WorkflowTaskType.LLM)]
    public void WorkflowTask_ShouldAcceptAllTaskTypes(WorkflowTaskType taskType)
    {
        // Arrange & Act
        var task = new WorkflowTask { TaskType = taskType };

        // Assert
        task.TaskType.Should().Be(taskType);
    }

    #endregion

    #region WorkflowNodeInstance Entity Tests

    [Fact]
    public void WorkflowNodeInstance_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var nodeInstance = new WorkflowNodeInstance();

        // Assert
        nodeInstance.Status.Should().Be(WorkflowNodeInstanceStatus.Pending);
        nodeInstance.RetryCount.Should().Be(0);
        nodeInstance.IsSkipped.Should().BeFalse();
    }

    [Fact]
    public void WorkflowNodeInstance_ShouldAllowSettingProperties()
    {
        // Arrange
        var startedAt = DateTime.UtcNow.AddMinutes(-5);
        var completedAt = DateTime.UtcNow;

        var nodeInstance = new WorkflowNodeInstance
        {
            Id = 1,
            WorkflowInstanceId = 10,
            WorkflowNodeId = 20,
            Status = WorkflowNodeInstanceStatus.Completed,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            DurationMs = 300000,
            InputData = "{\"input\": \"value\"}",
            OutputData = "{\"output\": \"result\"}",
            ExecutionSequence = 5,
            WorkerId = "worker-002"
        };

        // Assert
        nodeInstance.WorkflowInstanceId.Should().Be(10);
        nodeInstance.WorkflowNodeId.Should().Be(20);
        nodeInstance.Status.Should().Be(WorkflowNodeInstanceStatus.Completed);
        nodeInstance.DurationMs.Should().Be(300000);
        nodeInstance.ExecutionSequence.Should().Be(5);
        nodeInstance.WorkerId.Should().Be("worker-002");
    }

    [Fact]
    public void WorkflowNodeInstance_ShouldSupportSkippedState()
    {
        // Arrange
        var nodeInstance = new WorkflowNodeInstance
        {
            Status = WorkflowNodeInstanceStatus.Skipped,
            IsSkipped = true,
            SkipReason = "Condition not met"
        };

        // Assert
        nodeInstance.IsSkipped.Should().BeTrue();
        nodeInstance.SkipReason.Should().Be("Condition not met");
        nodeInstance.Status.Should().Be(WorkflowNodeInstanceStatus.Skipped);
    }

    #endregion

    #region WorkflowLog Entity Tests

    [Fact]
    public void WorkflowLog_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var log = new WorkflowLog();

        // Assert
        log.Level.Should().Be(WorkflowLogLevel.Info);
        log.Category.Should().Be("General");
        log.Message.Should().BeEmpty();
        log.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void WorkflowLog_ShouldAllowSettingProperties()
    {
        // Arrange
        var log = new WorkflowLog
        {
            Id = 1,
            WorkflowInstanceId = 10,
            WorkflowNodeId = 20,
            NodeInstanceId = 30,
            Level = WorkflowLogLevel.Error,
            Category = "NodeExecution",
            Message = "Failed to send email: SMTP connection timeout",
            Details = "{\"smtp_server\": \"mail.example.com\", \"timeout_ms\": 30000}",
            WorkerId = "worker-001",
            UserId = 5,
            DurationMs = 30005,
            ExceptionType = "SmtpException",
            StackTrace = "at SmtpClient.Send(...)"
        };

        // Assert
        log.WorkflowInstanceId.Should().Be(10);
        log.WorkflowNodeId.Should().Be(20);
        log.NodeInstanceId.Should().Be(30);
        log.Level.Should().Be(WorkflowLogLevel.Error);
        log.Category.Should().Be("NodeExecution");
        log.Message.Should().Contain("SMTP connection timeout");
        log.Details.Should().Contain("smtp_server");
        log.DurationMs.Should().Be(30005);
        log.ExceptionType.Should().Be("SmtpException");
    }

    [Theory]
    [InlineData(WorkflowLogLevel.Debug)]
    [InlineData(WorkflowLogLevel.Info)]
    [InlineData(WorkflowLogLevel.Warning)]
    [InlineData(WorkflowLogLevel.Error)]
    public void WorkflowLog_ShouldAcceptAllLogLevels(WorkflowLogLevel level)
    {
        // Arrange & Act
        var log = new WorkflowLog { Level = level };

        // Assert
        log.Level.Should().Be(level);
    }

    #endregion

    #region Integration Tests - Complete Workflow

    [Fact]
    public void Workflow_ShouldSupportCompleteStructure()
    {
        // Arrange - Create a complete workflow structure
        var definition = new WorkflowDefinition
        {
            Id = 1,
            WorkflowKey = "lead-qualification",
            Name = "Lead Qualification",
            EntityType = "Lead",
            Status = WorkflowStatus.Active,
            CurrentVersion = 1
        };

        var version = new WorkflowVersion
        {
            Id = 1,
            WorkflowDefinitionId = 1,
            VersionNumber = 1,
            Status = WorkflowVersionStatus.Active,
            WorkflowDefinition = definition
        };

        var startNode = new WorkflowNode
        {
            Id = 1,
            WorkflowVersionId = 1,
            NodeKey = "start",
            Name = "Start",
            NodeType = WorkflowNodeType.Trigger,
            IsStartNode = true
        };

        var conditionNode = new WorkflowNode
        {
            Id = 2,
            WorkflowVersionId = 1,
            NodeKey = "check-score",
            Name = "Check Score",
            NodeType = WorkflowNodeType.Condition
        };

        var endNode = new WorkflowNode
        {
            Id = 3,
            WorkflowVersionId = 1,
            NodeKey = "end",
            Name = "End",
            NodeType = WorkflowNodeType.End,
            IsEndNode = true
        };

        var transition1 = new WorkflowTransition
        {
            Id = 1,
            SourceNodeId = 1,
            TargetNodeId = 2,
            ConditionType = TransitionConditionType.Always
        };

        var transition2 = new WorkflowTransition
        {
            Id = 2,
            SourceNodeId = 2,
            TargetNodeId = 3,
            ConditionType = TransitionConditionType.Expression,
            ConditionExpression = "entity.Score >= 80",
            Label = "Qualified"
        };

        version.Nodes.Add(startNode);
        version.Nodes.Add(conditionNode);
        version.Nodes.Add(endNode);
        version.Transitions.Add(transition1);
        version.Transitions.Add(transition2);
        definition.Versions.Add(version);

        // Assert
        definition.Versions.Should().HaveCount(1);
        version.Nodes.Should().HaveCount(3);
        version.Transitions.Should().HaveCount(2);
        version.Nodes.First(n => n.IsStartNode).NodeType.Should().Be(WorkflowNodeType.Trigger);
        version.Nodes.First(n => n.IsEndNode).NodeType.Should().Be(WorkflowNodeType.End);
    }

    [Fact]
    public void WorkflowInstance_ShouldTrackExecution()
    {
        // Arrange - Simulate workflow execution
        var instance = new WorkflowInstance
        {
            Id = 1,
            WorkflowDefinitionId = 1,
            WorkflowVersionId = 1,
            EntityType = "Lead",
            EntityId = 100,
            Status = WorkflowInstanceStatus.Running,
            StartedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Add node instances
        var nodeInstance1 = new WorkflowNodeInstance
        {
            Id = 1,
            WorkflowInstanceId = 1,
            WorkflowNodeId = 1,
            Status = WorkflowNodeInstanceStatus.Completed,
            ExecutionSequence = 1
        };

        var nodeInstance2 = new WorkflowNodeInstance
        {
            Id = 2,
            WorkflowInstanceId = 1,
            WorkflowNodeId = 2,
            Status = WorkflowNodeInstanceStatus.Running,
            ExecutionSequence = 2
        };

        instance.NodeInstances.Add(nodeInstance1);
        instance.NodeInstances.Add(nodeInstance2);

        // Add task
        var task = new WorkflowTask
        {
            Id = 1,
            WorkflowInstanceId = 1,
            WorkflowNodeId = 2,
            TaskType = WorkflowTaskType.Human,
            Name = "Review Lead",
            Status = WorkflowTaskStatus.Waiting
        };
        instance.Tasks.Add(task);

        // Add logs
        var log1 = new WorkflowLog
        {
            WorkflowInstanceId = 1,
            Level = WorkflowLogLevel.Info,
            Message = "Workflow started"
        };
        var log2 = new WorkflowLog
        {
            WorkflowInstanceId = 1,
            WorkflowNodeId = 1,
            Level = WorkflowLogLevel.Info,
            Message = "Node completed"
        };
        instance.Logs.Add(log1);
        instance.Logs.Add(log2);

        // Assert
        instance.NodeInstances.Should().HaveCount(2);
        instance.Tasks.Should().HaveCount(1);
        instance.Logs.Should().HaveCount(2);
        instance.NodeInstances.Count(ni => ni.Status == WorkflowNodeInstanceStatus.Completed).Should().Be(1);
        instance.Tasks.First().Status.Should().Be(WorkflowTaskStatus.Waiting);
    }

    #endregion
}
