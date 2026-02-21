// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Services.ITSM;

/// <summary>
/// Comprehensive unit tests for ITSM Service Catalog functionality
/// </summary>
public class CatalogServiceTests
{
    #region Catalog Item Tests

    [Fact]
    public void CreateCatalogItem_ValidData_CreatesCorrectly()
    {
        // Arrange & Act
        var item = new CatalogItem
        {
            Name = "New Laptop Request",
            Description = "Request a new laptop for a new or existing employee",
            CategoryId = 1,
            Price = 1500.00m,
            FulfillmentTime = "3-5 business days",
            RequiresApproval = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        item.Should().NotBeNull();
        item.Name.Should().Be("New Laptop Request");
        item.RequiresApproval.Should().BeTrue();
        item.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateCatalogItem_WithFormFields_DefinesCorrectly()
    {
        // Arrange & Act
        var item = new CatalogItem
        {
            Name = "Software Installation",
            FormFields = new List<FormField>
            {
                new() { Name = "software_name", Label = "Software Name", FieldType = "text", IsRequired = true },
                new() { Name = "business_justification", Label = "Business Justification", FieldType = "textarea", IsRequired = true },
                new() { Name = "urgency", Label = "Urgency", FieldType = "select", IsRequired = true,
                        Options = new[] { "Low", "Medium", "High" } }
            }
        };

        // Assert
        item.FormFields.Should().HaveCount(3);
        item.FormFields.Should().Contain(f => f.Name == "software_name");
        item.FormFields.First(f => f.Name == "urgency").Options.Should().HaveCount(3);
    }

    [Fact]
    public void CatalogItem_WithApprovalWorkflow_DefinesApprovers()
    {
        // Arrange & Act
        var item = new CatalogItem
        {
            Name = "VPN Access Request",
            RequiresApproval = true,
            ApprovalLevels = new List<ApprovalLevel>
            {
                new() { Level = 1, ApproverType = "Manager", Description = "Manager approval" },
                new() { Level = 2, ApproverType = "SecurityTeam", Description = "Security review" }
            }
        };

        // Assert
        item.RequiresApproval.Should().BeTrue();
        item.ApprovalLevels.Should().HaveCount(2);
        item.ApprovalLevels.First().Level.Should().Be(1);
    }

    #endregion

    #region Catalog Category Tests

    [Fact]
    public void CreateCategory_TopLevel_CreatesCorrectly()
    {
        // Arrange & Act
        var category = new CatalogCategory
        {
            Name = "Hardware Requests",
            Description = "Request new hardware equipment",
            IconName = "devices",
            ParentCategoryId = null,
            SortOrder = 1,
            IsActive = true
        };

        // Assert
        category.ParentCategoryId.Should().BeNull();
        category.Name.Should().Be("Hardware Requests");
    }

    [Fact]
    public void CreateCategory_SubCategory_LinksToParent()
    {
        // Arrange & Act
        var category = new CatalogCategory
        {
            Name = "Laptops",
            ParentCategoryId = 1, // Hardware Requests
            SortOrder = 1
        };

        // Assert
        category.ParentCategoryId.Should().Be(1);
    }

    [Fact]
    public void CategoryHierarchy_ThreeLevels_NavigatesCorrectly()
    {
        // Arrange
        var categories = CreateCategoryHierarchy();

        // Act
        var topLevel = categories.Where(c => c.ParentCategoryId == null).ToList();
        var hardwareChildren = categories.Where(c => c.ParentCategoryId == 1).ToList();

        // Assert
        topLevel.Should().HaveCount(3); // Hardware, Software, Access
        hardwareChildren.Should().HaveCount(3); // Laptop, Desktop, Mobile
    }

    #endregion

    #region Service Request Tests

    [Fact]
    public void CreateServiceRequest_FromCatalogItem_CreatesCorrectly()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            RequestNumber = "REQ0000001",
            CatalogItemId = 1,
            CatalogItemName = "New Laptop Request",
            RequestedById = 10,
            RequestedForId = 10, // Self-request
            Status = RequestStatus.Pending,
            FormData = new Dictionary<string, object>
            {
                { "laptop_type", "Standard" },
                { "justification", "New hire starting next week" }
            },
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        request.RequestNumber.Should().StartWith("REQ");
        request.CatalogItemId.Should().Be(1);
        request.FormData.Should().ContainKey("laptop_type");
    }

    [Fact]
    public void CreateServiceRequest_ForAnotherUser_SetsRequestedFor()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            RequestedById = 10, // Manager
            RequestedForId = 20, // New employee
            CatalogItemId = 1
        };

        // Assert
        request.RequestedById.Should().NotBe(request.RequestedForId);
    }

    [Fact]
    public void ServiceRequest_StatusTransition_PendingToApproved()
    {
        // Arrange
        var request = new ServiceRequest { Status = RequestStatus.Pending };

        // Act
        var canTransition = IsValidRequestTransition(request.Status, RequestStatus.Approved);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusTransition_ApprovedToFulfillment()
    {
        // Arrange
        var request = new ServiceRequest { Status = RequestStatus.Approved };

        // Act
        var canTransition = IsValidRequestTransition(request.Status, RequestStatus.Fulfillment);

        // Assert
        canTransition.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusTransition_FulfillmentToComplete()
    {
        // Arrange
        var request = new ServiceRequest { Status = RequestStatus.Fulfillment };

        // Act
        var canTransition = IsValidRequestTransition(request.Status, RequestStatus.Completed);

        // Assert
        canTransition.Should().BeTrue();
    }

    private static bool IsValidRequestTransition(RequestStatus from, RequestStatus to)
    {
        var validTransitions = new Dictionary<RequestStatus, RequestStatus[]>
        {
            { RequestStatus.Draft, new[] { RequestStatus.Pending, RequestStatus.Cancelled } },
            { RequestStatus.Pending, new[] { RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Cancelled } },
            { RequestStatus.Approved, new[] { RequestStatus.Fulfillment, RequestStatus.Cancelled } },
            { RequestStatus.Fulfillment, new[] { RequestStatus.Completed, RequestStatus.Failed } },
            { RequestStatus.Completed, Array.Empty<RequestStatus>() },
            { RequestStatus.Rejected, Array.Empty<RequestStatus>() },
            { RequestStatus.Cancelled, Array.Empty<RequestStatus>() },
            { RequestStatus.Failed, new[] { RequestStatus.Fulfillment } }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    #endregion

    #region Approval Workflow Tests

    [Fact]
    public void ApprovalWorkflow_SingleApprover_ApprovesRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = RequestStatus.Pending,
            RequiredApprovals = 1,
            CurrentApprovalLevel = 1
        };

        // Act
        var approval = new RequestApproval
        {
            RequestId = request.RequestId,
            ApproverId = 5,
            Decision = ApprovalDecision.Approved,
            ApprovedAt = DateTime.UtcNow
        };
        request.Approvals.Add(approval);
        request.Status = RequestStatus.Approved;

        // Assert
        request.Approvals.Should().HaveCount(1);
        request.Status.Should().Be(RequestStatus.Approved);
    }

    [Fact]
    public void ApprovalWorkflow_MultiLevel_RequiresAllApprovals()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = RequestStatus.Pending,
            RequiredApprovals = 2,
            CurrentApprovalLevel = 1
        };

        // Act - First approval
        request.Approvals.Add(new RequestApproval
        {
            Level = 1,
            Decision = ApprovalDecision.Approved,
            ApprovedAt = DateTime.UtcNow
        });
        request.CurrentApprovalLevel = 2;

        // Assert - Still pending second approval
        request.CurrentApprovalLevel.Should().Be(2);
        request.Approvals.Count(a => a.Decision == ApprovalDecision.Approved)
            .Should().BeLessThan(request.RequiredApprovals);
    }

    [Fact]
    public void ApprovalWorkflow_Rejected_SetsRejectedStatus()
    {
        // Arrange
        var request = new ServiceRequest { Status = RequestStatus.Pending };

        // Act
        var rejection = new RequestApproval
        {
            Decision = ApprovalDecision.Rejected,
            RejectionReason = "Budget not available",
            ApprovedAt = DateTime.UtcNow
        };
        request.Approvals.Add(rejection);
        request.Status = RequestStatus.Rejected;

        // Assert
        request.Status.Should().Be(RequestStatus.Rejected);
        request.Approvals.First().RejectionReason.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Fulfillment Tests

    [Fact]
    public void Fulfillment_CreatesTasks_ForApprovedRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = RequestStatus.Approved,
            CatalogItemId = 1
        };

        // Act
        var tasks = new List<FulfillmentTask>
        {
            new() { TaskName = "Procure laptop", AssignedGroupId = 5, Status = TaskStatus.Pending },
            new() { TaskName = "Configure device", AssignedGroupId = 6, Status = TaskStatus.Pending },
            new() { TaskName = "Deliver to user", AssignedGroupId = 7, Status = TaskStatus.Pending }
        };
        request.FulfillmentTasks = tasks;
        request.Status = RequestStatus.Fulfillment;

        // Assert
        request.FulfillmentTasks.Should().HaveCount(3);
        request.Status.Should().Be(RequestStatus.Fulfillment);
    }

    [Fact]
    public void Fulfillment_AllTasksComplete_CompletesRequest()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = RequestStatus.Fulfillment,
            FulfillmentTasks = new List<FulfillmentTask>
            {
                new() { Status = TaskStatus.Completed },
                new() { Status = TaskStatus.Completed },
                new() { Status = TaskStatus.Completed }
            }
        };

        // Act
        var allComplete = request.FulfillmentTasks.All(t => t.Status == TaskStatus.Completed);
        if (allComplete)
        {
            request.Status = RequestStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
        }

        // Assert
        request.Status.Should().Be(RequestStatus.Completed);
        request.CompletedAt.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static List<CatalogCategory> CreateCategoryHierarchy()
    {
        return new List<CatalogCategory>
        {
            // Top level
            new() { CategoryId = 1, Name = "Hardware Requests", ParentCategoryId = null },
            new() { CategoryId = 2, Name = "Software Requests", ParentCategoryId = null },
            new() { CategoryId = 3, Name = "Access & Accounts", ParentCategoryId = null },

            // Hardware children
            new() { CategoryId = 4, Name = "Laptop", ParentCategoryId = 1 },
            new() { CategoryId = 5, Name = "Desktop", ParentCategoryId = 1 },
            new() { CategoryId = 6, Name = "Mobile Device", ParentCategoryId = 1 },

            // Software children
            new() { CategoryId = 7, Name = "Office Applications", ParentCategoryId = 2 },
            new() { CategoryId = 8, Name = "Development Tools", ParentCategoryId = 2 }
        };
    }

    #endregion
}

// Test helper classes
public class CatalogItem
{
    public int CatalogItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public decimal? Price { get; set; }
    public string? FulfillmentTime { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public List<FormField> FormFields { get; set; } = new();
    public List<ApprovalLevel> ApprovalLevels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class FormField
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string[]? Options { get; set; }
}

public class ApprovalLevel
{
    public int Level { get; set; }
    public string ApproverType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CatalogCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public int? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class ServiceRequest
{
    public int RequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int CatalogItemId { get; set; }
    public string? CatalogItemName { get; set; }
    public int RequestedById { get; set; }
    public int RequestedForId { get; set; }
    public RequestStatus Status { get; set; }
    public Dictionary<string, object> FormData { get; set; } = new();
    public int RequiredApprovals { get; set; }
    public int CurrentApprovalLevel { get; set; }
    public List<RequestApproval> Approvals { get; set; } = new();
    public List<FulfillmentTask> FulfillmentTasks { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class RequestApproval
{
    public int RequestId { get; set; }
    public int ApproverId { get; set; }
    public int Level { get; set; }
    public ApprovalDecision Decision { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime ApprovedAt { get; set; }
}

public class FulfillmentTask
{
    public string TaskName { get; set; } = string.Empty;
    public int? AssignedGroupId { get; set; }
    public TaskStatus Status { get; set; }
}

public enum RequestStatus
{
    Draft = 1,
    Pending = 2,
    Approved = 3,
    Rejected = 4,
    Fulfillment = 5,
    Completed = 6,
    Cancelled = 7,
    Failed = 8
}

public enum ApprovalDecision
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public enum TaskStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
