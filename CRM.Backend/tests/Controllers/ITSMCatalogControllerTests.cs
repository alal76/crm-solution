// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Security.Claims;
using CRM.Api.Controllers;
using CRM.Core.Dtos.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Services.ITSM;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

public class ITSMCatalogControllerTests
{
    private readonly Mock<IServiceCatalogService> _mockService;
    private readonly Mock<ICatalogApprovalService> _mockApprovalService;
    private readonly Mock<ICatalogFulfillmentService> _mockFulfillmentService;
    private readonly CatalogController _controller;

    public ITSMCatalogControllerTests()
    {
        _mockService = new Mock<IServiceCatalogService>();
        _mockApprovalService = new Mock<ICatalogApprovalService>();
        _mockFulfillmentService = new Mock<ICatalogFulfillmentService>();
        _controller = new CatalogController(_mockService.Object, _mockApprovalService.Object, _mockFulfillmentService.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "1") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ────────────────────────────────────────────────────────────────
    // GET /items
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCatalogItems_ShouldReturnOk()
    {
        var items = new List<CatalogItemDto>
        {
            new() { CatalogItemId = 1, Name = "Laptop Request" }
        };
        _mockService.Setup(s => s.GetCatalogItemsAsync(null, null)).ReturnsAsync(items);

        var result = await _controller.GetCatalogItems(null);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CatalogItemDto>>().Subject;
        returned.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCatalogItems_ShouldPassCategoryFilter()
    {
        var items = new List<CatalogItemDto>();
        _mockService.Setup(s => s.GetCatalogItemsAsync(5, null)).ReturnsAsync(items);

        var result = await _controller.GetCatalogItems(5);

        _mockService.Verify(s => s.GetCatalogItemsAsync(5, null), Times.Once);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /items/{id}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCatalogItem_ShouldReturnOk_WhenItemExists()
    {
        var item = new CatalogItemDto { CatalogItemId = 1, Name = "VPN Access" };
        _mockService.Setup(s => s.GetCatalogItemByIdAsync(1)).ReturnsAsync(item);

        var result = await _controller.GetCatalogItem(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(item);
    }

    [Fact]
    public async Task GetCatalogItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        _mockService.Setup(s => s.GetCatalogItemByIdAsync(999)).ReturnsAsync((CatalogItemDto?)null);

        var result = await _controller.GetCatalogItem(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateCatalogRequest_ShouldReturnOk_WithRequestId()
    {
        var dto = new CreateCatalogRequestDto { CatalogItemId = 1, RequestedForId = 1 };
        _mockService.Setup(s => s.CreateCatalogRequestAsync(dto, 1)).ReturnsAsync(42);

        var result = await _controller.CreateCatalogRequest(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        // Controller returns anonymous { requestId = 42 }
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/my
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyRequests_ShouldReturnOk_WithCurrentUserRequests()
    {
        var requests = new List<CatalogRequest>
        {
            new() { RequestId = 1, CatalogItemId = 1, RequestedById = 1 }
        };
        _mockService.Setup(s => s.GetMyRequestsAsync(1)).ReturnsAsync(requests);

        var result = await _controller.GetMyRequests();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<CatalogRequest>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /search
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchCatalog_ShouldReturnOk()
    {
        var items = new List<CatalogItemDto>
        {
            new() { CatalogItemId = 1, Name = "Monitor" }
        };
        _mockService.Setup(s => s.SearchCatalogAsync("monitor")).ReturnsAsync(items);

        var result = await _controller.SearchCatalog("monitor");

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /featured
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFeaturedItems_ShouldReturnOk_WithFeaturedOnly()
    {
        var items = new List<CatalogItemDto> { new() { CatalogItemId = 1, Name = "Featured" } };
        _mockService.Setup(s => s.GetCatalogItemsAsync(null, true)).ReturnsAsync(items);

        var result = await _controller.GetFeaturedItems();

        _mockService.Verify(s => s.GetCatalogItemsAsync(null, true), Times.Once);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /categories
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCategories_ShouldReturnOk()
    {
        var categories = new List<CatalogCategoryInfo>
        {
            new() { CategoryId = 1, Name = "Hardware", ItemCount = 5 }
        };
        _mockService.Setup(s => s.GetCategoriesAsync()).ReturnsAsync(categories);

        var result = await _controller.GetCategories();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/for-others
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRequestForOthers_ShouldReturnOk_WithRequestId()
    {
        _mockService
            .Setup(s => s.CreateCatalogRequestForOthersAsync(
                It.IsAny<CRM.Core.Interfaces.ITSM.CreateCatalogRequestForOthersDto>(), 1))
            .ReturnsAsync(99);

        var dto = new CreateCatalogRequestForOthersRequest
        {
            CatalogItemId = 1,
            RequestedForUserId = 42,
            Notes = "For new hire"
        };
        var result = await _controller.CreateRequestForOthers(dto);

        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/{requestId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetRequestById_ShouldReturnOk_WhenExists()
    {
        var request = new CatalogRequest { RequestId = 1, CatalogItemId = 5 };
        _mockService.Setup(s => s.GetRequestByIdAsync(1)).ReturnsAsync(request);

        var result = await _controller.GetRequestById(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(request);
    }

    [Fact]
    public async Task GetRequestById_ShouldReturnNotFound_WhenDoesNotExist()
    {
        _mockService.Setup(s => s.GetRequestByIdAsync(999)).ReturnsAsync((CatalogRequest?)null);

        var result = await _controller.GetRequestById(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // DELETE /requests/{requestId}
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelRequest_ShouldReturnOk_WhenSuccessful()
    {
        _mockService.Setup(s => s.CancelRequestAsync(1, 1)).ReturnsAsync(true);

        var result = await _controller.CancelRequest(1);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelRequest_ShouldReturnBadRequest_WhenFails()
    {
        _mockService.Setup(s => s.CancelRequestAsync(1, 1)).ReturnsAsync(false);

        var result = await _controller.CancelRequest(1);

        var badReq = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badReq.Value.Should().Be("Unable to cancel request");
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/{requestId}/approval/submit
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitForApproval_ShouldReturnOk_WithWorkflow()
    {
        var workflow = new ApprovalWorkflow { WorkflowId = 5, ServiceRequestId = 1, State = WorkflowState.InProgress };
        _mockApprovalService.Setup(s => s.SubmitForApprovalAsync(1, 1)).ReturnsAsync(workflow);

        var result = await _controller.SubmitForApproval(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(workflow);
    }

    [Fact]
    public async Task SubmitForApproval_ShouldReturnNotFound_WhenRequestMissing()
    {
        _mockApprovalService.Setup(s => s.SubmitForApprovalAsync(999, 1))
            .ThrowsAsync(new ArgumentException("Service request 999 not found"));

        var result = await _controller.SubmitForApproval(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/{requestId}/approval/status
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetApprovalStatus_ShouldReturnOk_WhenWorkflowExists()
    {
        var workflow = new ApprovalWorkflow { WorkflowId = 5, ServiceRequestId = 1 };
        _mockApprovalService.Setup(s => s.GetApprovalStatusAsync(1)).ReturnsAsync(workflow);

        var result = await _controller.GetApprovalStatus(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(workflow);
    }

    [Fact]
    public async Task GetApprovalStatus_ShouldReturnNotFound_WhenNoWorkflow()
    {
        _mockApprovalService.Setup(s => s.GetApprovalStatusAsync(1)).ReturnsAsync((ApprovalWorkflow?)null);

        var result = await _controller.GetApprovalStatus(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/{requestId}/approval/history
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetApprovalHistory_ShouldReturnOk_WithActions()
    {
        var actions = new List<ApprovalAction> { new() { ActionId = 1, WorkflowId = 5, Decision = ApprovalDecision.Approve } };
        _mockApprovalService.Setup(s => s.GetApprovalHistoryAsync(1)).ReturnsAsync(actions);

        var result = await _controller.GetApprovalHistory(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<ApprovalAction>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/{requestId}/approval/withdraw
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task WithdrawApproval_ShouldReturnOk_WhenSuccessful()
    {
        _mockApprovalService.Setup(s => s.WithdrawApprovalAsync(1, 1, "changed my mind")).ReturnsAsync(true);

        var result = await _controller.WithdrawApproval(1, new ReasonRequest { Reason = "changed my mind" });

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task WithdrawApproval_ShouldReturnBadRequest_WhenFails()
    {
        _mockApprovalService.Setup(s => s.WithdrawApprovalAsync(1, 1, "too late")).ReturnsAsync(false);

        var result = await _controller.WithdrawApproval(1, new ReasonRequest { Reason = "too late" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /approval/workflows/{workflowId}/process
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ProcessApproval_ShouldReturnOk_WithAction()
    {
        var action = new ApprovalAction { ActionId = 1, WorkflowId = 5, Decision = ApprovalDecision.Approve, ApproverId = 1 };
        _mockApprovalService
            .Setup(s => s.ProcessApprovalAsync(5, 1, ApprovalDecision.Approve, "looks good"))
            .ReturnsAsync(action);

        var result = await _controller.ProcessApproval(5, new ProcessApprovalRequest { Decision = ApprovalDecision.Approve, Comments = "looks good" });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(action);
    }

    [Fact]
    public async Task ProcessApproval_ShouldReturnNotFound_WhenWorkflowMissing()
    {
        _mockApprovalService
            .Setup(s => s.ProcessApprovalAsync(999, 1, ApprovalDecision.Approve, null))
            .ThrowsAsync(new ArgumentException("Workflow 999 not found"));

        var result = await _controller.ProcessApproval(999, new ProcessApprovalRequest { Decision = ApprovalDecision.Approve });

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ProcessApproval_ShouldReturnBadRequest_WhenNoActiveStage()
    {
        _mockApprovalService
            .Setup(s => s.ProcessApprovalAsync(5, 1, ApprovalDecision.Approve, null))
            .ThrowsAsync(new InvalidOperationException("No active stage found in workflow"));

        var result = await _controller.ProcessApproval(5, new ProcessApprovalRequest { Decision = ApprovalDecision.Approve });

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /approval/workflows/{workflowId}/escalate
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EscalateApproval_ShouldReturnOk_WhenSuccessful()
    {
        _mockApprovalService.Setup(s => s.EscalateApprovalAsync(5, "overdue")).ReturnsAsync(true);

        var result = await _controller.EscalateApproval(5, new ReasonRequest { Reason = "overdue" });

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task EscalateApproval_ShouldReturnNotFound_WhenWorkflowMissing()
    {
        _mockApprovalService.Setup(s => s.EscalateApprovalAsync(999, "overdue")).ReturnsAsync(false);

        var result = await _controller.EscalateApproval(999, new ReasonRequest { Reason = "overdue" });

        result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /approval/pending
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPendingApprovals_ShouldReturnOk_ForCurrentUser()
    {
        var pending = new List<PendingServiceRequestApproval> { new() { WorkflowId = 5, ServiceRequestId = 1 } };
        _mockApprovalService.Setup(s => s.GetPendingApprovalsAsync(1)).ReturnsAsync(pending);

        var result = await _controller.GetPendingApprovals();

        _mockApprovalService.Verify(s => s.GetPendingApprovalsAsync(1), Times.Once);
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returned = okResult.Value.Should().BeAssignableTo<IEnumerable<PendingServiceRequestApproval>>().Subject;
        returned.Should().HaveCount(1);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /items/{catalogItemId}/approval-rule
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetApprovalRule_ShouldReturnOk()
    {
        var rule = new CatalogApprovalRule { RuleId = 1, CatalogItemId = 1, RequiresApproval = true };
        _mockApprovalService.Setup(s => s.GetApprovalRuleAsync(1)).ReturnsAsync(rule);

        var result = await _controller.GetApprovalRule(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(rule);
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/{requestId}/fulfillment/start
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task StartFulfillment_ShouldReturnOk_WithWorkflow()
    {
        var workflow = new FulfillmentWorkflow { WorkflowId = 3, ServiceRequestId = 1, State = FulfillmentState.InProgress };
        _mockFulfillmentService.Setup(s => s.StartFulfillmentAsync(1)).ReturnsAsync(workflow);

        var result = await _controller.StartFulfillment(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(workflow);
    }

    [Fact]
    public async Task StartFulfillment_ShouldReturnNotFound_WhenRequestMissing()
    {
        _mockFulfillmentService.Setup(s => s.StartFulfillmentAsync(999))
            .ThrowsAsync(new ArgumentException("Service request 999 not found"));

        var result = await _controller.StartFulfillment(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET /requests/{requestId}/fulfillment
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFulfillmentStatus_ShouldReturnOk_WhenWorkflowExists()
    {
        var workflow = new FulfillmentWorkflow { WorkflowId = 3, ServiceRequestId = 1 };
        _mockFulfillmentService.Setup(s => s.GetFulfillmentStatusAsync(1)).ReturnsAsync(workflow);

        var result = await _controller.GetFulfillmentStatus(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(workflow);
    }

    [Fact]
    public async Task GetFulfillmentStatus_ShouldReturnNotFound_WhenNoWorkflow()
    {
        _mockFulfillmentService.Setup(s => s.GetFulfillmentStatusAsync(1)).ReturnsAsync((FulfillmentWorkflow?)null);

        var result = await _controller.GetFulfillmentStatus(1);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /requests/{requestId}/fulfillment/cancel
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelFulfillment_ShouldReturnOk_WhenSuccessful()
    {
        _mockFulfillmentService.Setup(s => s.CancelFulfillmentAsync(1, "no longer needed", 1)).ReturnsAsync(true);

        var result = await _controller.CancelFulfillment(1, new ReasonRequest { Reason = "no longer needed" });

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task CancelFulfillment_ShouldReturnBadRequest_WhenFails()
    {
        _mockFulfillmentService.Setup(s => s.CancelFulfillmentAsync(1, "too late", 1)).ReturnsAsync(false);

        var result = await _controller.CancelFulfillment(1, new ReasonRequest { Reason = "too late" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /fulfillment/tasks/{taskId}/complete
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteFulfillmentTask_ShouldReturnOk_WithTask()
    {
        var task = new FulfillmentTask { TaskId = 7, Name = "Reserve Equipment", State = TaskState.Completed };
        _mockFulfillmentService.Setup(s => s.CompleteTaskAsync(7, 1, "done")).ReturnsAsync(task);

        var result = await _controller.CompleteFulfillmentTask(7, new CompleteFulfillmentTaskRequest { Notes = "done" });

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(task);
    }

    [Fact]
    public async Task CompleteFulfillmentTask_ShouldReturnNotFound_WhenTaskMissing()
    {
        _mockFulfillmentService.Setup(s => s.CompleteTaskAsync(999, 1, null))
            .ThrowsAsync(new ArgumentException("Task 999 not found"));

        var result = await _controller.CompleteFulfillmentTask(999, new CompleteFulfillmentTaskRequest());

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task CompleteFulfillmentTask_ShouldReturnBadRequest_WhenNotInProgress()
    {
        _mockFulfillmentService.Setup(s => s.CompleteTaskAsync(7, 1, null))
            .ThrowsAsync(new InvalidOperationException("Task 7 is not in progress"));

        var result = await _controller.CompleteFulfillmentTask(7, new CompleteFulfillmentTaskRequest());

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // POST /fulfillment/tasks/{taskId}/automate
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteFulfillmentAutomation_ShouldReturnOk_WithResult()
    {
        var automationResult = new AutomationResult { TaskId = 7, Success = true, Message = "done" };
        _mockFulfillmentService.Setup(s => s.ExecuteAutomationAsync(7)).ReturnsAsync(automationResult);

        var result = await _controller.ExecuteFulfillmentAutomation(7);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(automationResult);
    }

    [Fact]
    public async Task ExecuteFulfillmentAutomation_ShouldReturnNotFound_WhenTaskMissing()
    {
        _mockFulfillmentService.Setup(s => s.ExecuteAutomationAsync(999))
            .ThrowsAsync(new ArgumentException("Task 999 not found"));

        var result = await _controller.ExecuteFulfillmentAutomation(999);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task ExecuteFulfillmentAutomation_ShouldReturnBadRequest_WhenNoAutomationConfig()
    {
        _mockFulfillmentService.Setup(s => s.ExecuteAutomationAsync(7))
            .ThrowsAsync(new InvalidOperationException("Task 7 has no automation configuration"));

        var result = await _controller.ExecuteFulfillmentAutomation(7);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ────────────────────────────────────────────────────────────────
    // GET/PUT /items/{catalogItemId}/fulfillment-template
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFulfillmentTemplate_ShouldReturnOk()
    {
        var template = new FulfillmentTemplate { TemplateId = 1, CatalogItemId = 1, CatalogItemName = "Laptop" };
        _mockFulfillmentService.Setup(s => s.GetFulfillmentTemplateAsync(1)).ReturnsAsync(template);

        var result = await _controller.GetFulfillmentTemplate(1);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(template);
    }

    [Fact]
    public async Task SaveFulfillmentTemplate_ShouldReturnOk_AndForceCatalogItemId()
    {
        var saved = new FulfillmentTemplate { TemplateId = 1, CatalogItemId = 1, CatalogItemName = "Laptop" };
        _mockFulfillmentService
            .Setup(s => s.SaveFulfillmentTemplateAsync(It.Is<FulfillmentTemplate>(t => t.CatalogItemId == 1)))
            .ReturnsAsync(saved);

        var template = new FulfillmentTemplate { CatalogItemName = "Laptop" };
        var result = await _controller.SaveFulfillmentTemplate(1, template);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(saved);
        _mockFulfillmentService.Verify(s => s.SaveFulfillmentTemplateAsync(It.Is<FulfillmentTemplate>(t => t.CatalogItemId == 1)), Times.Once);
    }

    // ────────────────────────────────────────────────────────────────
    // GET /fulfillment/metrics
    // ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetFulfillmentMetrics_ShouldReturnOk()
    {
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 2, 1);
        var metrics = new FulfillmentMetrics { FromDate = from, ToDate = to, TotalRequestsFulfilled = 3 };
        _mockFulfillmentService.Setup(s => s.GetMetricsAsync(from, to)).ReturnsAsync(metrics);

        var result = await _controller.GetFulfillmentMetrics(from, to);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(metrics);
    }
}
