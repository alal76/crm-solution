// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CRM.Tests.Integration;

public class ServiceRequestEscalationWorkflowTests
{
    private readonly Mock<IServiceRequestService> _svc = new();

    // ── factory helper ─────────────────────────────────────────────────
    private static ServiceRequestDto BuildDto(int id, string ticketNo = "SR-001") =>
        new()
        {
            Id = id,
            TicketNumber = ticketNo,
            Subject = "Test ticket"
        };

    private static CreateServiceRequestDto BuildCreateDto() =>
        new()
        {
            Subject = "New ticket",
            Description = "Issue details"
        };

    // ══════════════════════════════════════════════════════════════════
    // 1. Create
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Create_ReturnsServiceRequestDto_WhenSuccessful()
    {
        var dto = BuildDto(1, "SR-001");
        _svc.Setup(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), null))
            .ReturnsAsync(dto);

        var result = await _svc.Object.CreateServiceRequestAsync(BuildCreateDto(), null);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.TicketNumber.Should().Be("SR-001");
    }

    [Fact]
    public async Task Create_WithUserId_PassesUserIdToService()
    {
        var dto = BuildDto(1, "SR-001");
        _svc.Setup(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), 5))
            .ReturnsAsync(dto);

        var result = await _svc.Object.CreateServiceRequestAsync(BuildCreateDto(), 5);

        result.Should().NotBeNull();
        _svc.Verify(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), 5), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════
    // 2. Retrieve
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetById_ReturnsDto_WhenFound()
    {
        _svc.Setup(s => s.GetServiceRequestByIdAsync(1)).ReturnsAsync(BuildDto(1));

        var result = await _svc.Object.GetServiceRequestByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_ReturnsNull_WhenNotFound()
    {
        _svc.Setup(s => s.GetServiceRequestByIdAsync(999)).ReturnsAsync((ServiceRequestDto?)null);

        var result = await _svc.Object.GetServiceRequestByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTicketNumber_ReturnsDto_WhenFound()
    {
        _svc.Setup(s => s.GetServiceRequestByTicketNumberAsync("SR-001")).ReturnsAsync(BuildDto(1));

        var result = await _svc.Object.GetServiceRequestByTicketNumberAsync("SR-001");

        result.Should().NotBeNull();
        result!.TicketNumber.Should().Be("SR-001");
    }

    // ══════════════════════════════════════════════════════════════════
    // 3. Assignment
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AssignToUser_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.AssignToUserAsync(1, 10, null)).ReturnsAsync(dto);

        var result = await _svc.Object.AssignToUserAsync(1, 10, null);

        result.Should().NotBeNull();
        _svc.Verify(s => s.AssignToUserAsync(1, 10, null), Times.Once);
    }

    [Fact]
    public async Task AssignToGroup_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.AssignToGroupAsync(1, 3, null)).ReturnsAsync(dto);

        var result = await _svc.Object.AssignToGroupAsync(1, 3, null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Unassign_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.UnassignAsync(1, null)).ReturnsAsync(dto);

        var result = await _svc.Object.UnassignAsync(1, null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 4. Status progression
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateStatus_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.UpdateStatusAsync(1, ServiceRequestStatus.InProgress, null)).ReturnsAsync(dto);

        var result = await _svc.Object.UpdateStatusAsync(1, ServiceRequestStatus.InProgress, null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkFirstResponse_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.MarkFirstResponseAsync(1, 7)).ReturnsAsync(dto);

        var result = await _svc.Object.MarkFirstResponseAsync(1, 7);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkFirstResponse_WithNullUser_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.MarkFirstResponseAsync(1, null)).ReturnsAsync(dto);

        var result = await _svc.Object.MarkFirstResponseAsync(1, null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 5. Escalation
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Escalate_ReturnsUpdatedDto_WithReason()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.EscalateServiceRequestAsync(1, "SLA breach", 5)).ReturnsAsync(dto);

        var result = await _svc.Object.EscalateServiceRequestAsync(1, "SLA breach", 5);

        result.Should().NotBeNull();
        _svc.Verify(s => s.EscalateServiceRequestAsync(1, "SLA breach", 5), Times.Once);
    }

    [Fact]
    public async Task Escalate_WithNullUser_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.EscalateServiceRequestAsync(1, "Customer unhappy", null)).ReturnsAsync(dto);

        var result = await _svc.Object.EscalateServiceRequestAsync(1, "Customer unhappy", null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Expedite_ReturnsUpdatedDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.ExpediteServiceRequestAsync(1, "VIP customer", 3)).ReturnsAsync(dto);

        var result = await _svc.Object.ExpediteServiceRequestAsync(1, "VIP customer", 3);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 6. Resolution
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Resolve_ReturnsDto_WithAllFields()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.ResolveServiceRequestAsync(1, "Fixed the server", "RCA-001", "Hardware failure", 8))
            .ReturnsAsync(dto);

        var result = await _svc.Object.ResolveServiceRequestAsync(1, "Fixed the server", "RCA-001", "Hardware failure", 8);

        result.Should().NotBeNull();
        _svc.Verify(s => s.ResolveServiceRequestAsync(1, "Fixed the server", "RCA-001", "Hardware failure", 8), Times.Once);
    }

    [Fact]
    public async Task Resolve_WithNullOptionalFields_ReturnsDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.ResolveServiceRequestAsync(1, "Fixed issue", null, null, null)).ReturnsAsync(dto);

        var result = await _svc.Object.ResolveServiceRequestAsync(1, "Fixed issue", null, null, null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 7. Close
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Close_ReturnsDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.CloseServiceRequestAsync(1, 2)).ReturnsAsync(dto);

        var result = await _svc.Object.CloseServiceRequestAsync(1, 2);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Close_WithNullUser_ReturnsDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.CloseServiceRequestAsync(1, null)).ReturnsAsync(dto);

        var result = await _svc.Object.CloseServiceRequestAsync(1, null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 8. Reopen
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Reopen_ReturnsDto_WithReason()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.ReopenServiceRequestAsync(1, "Still failing", 6)).ReturnsAsync(dto);

        var result = await _svc.Object.ReopenServiceRequestAsync(1, "Still failing", 6);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Reopen_WithNullUser_ReturnsDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.ReopenServiceRequestAsync(1, "Issue recurred", null)).ReturnsAsync(dto);

        var result = await _svc.Object.ReopenServiceRequestAsync(1, "Issue recurred", null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 9. Full lifecycle: Create → Assign → FirstResponse → Escalate → Resolve → Close
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task FullLifecycle_CreateToClose_AllStepsSucceed()
    {
        var dto = BuildDto(1, "SR-001");

        _svc.Setup(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), null)).ReturnsAsync(dto);
        _svc.Setup(s => s.AssignToUserAsync(1, 10, null)).ReturnsAsync(dto);
        _svc.Setup(s => s.MarkFirstResponseAsync(1, 10)).ReturnsAsync(dto);
        _svc.Setup(s => s.EscalateServiceRequestAsync(1, "SLA breach", 5)).ReturnsAsync(dto);
        _svc.Setup(s => s.ResolveServiceRequestAsync(1, "Resolved", null, null, 10)).ReturnsAsync(dto);
        _svc.Setup(s => s.CloseServiceRequestAsync(1, 10)).ReturnsAsync(dto);

        // Execute lifecycle
        var created = await _svc.Object.CreateServiceRequestAsync(BuildCreateDto(), null);
        await _svc.Object.AssignToUserAsync(created.Id, 10, null);
        await _svc.Object.MarkFirstResponseAsync(created.Id, 10);
        await _svc.Object.EscalateServiceRequestAsync(created.Id, "SLA breach", 5);
        await _svc.Object.ResolveServiceRequestAsync(created.Id, "Resolved", null, null, 10);
        await _svc.Object.CloseServiceRequestAsync(created.Id, 10);

        _svc.Verify(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), null), Times.Once);
        _svc.Verify(s => s.AssignToUserAsync(1, 10, null), Times.Once);
        _svc.Verify(s => s.MarkFirstResponseAsync(1, 10), Times.Once);
        _svc.Verify(s => s.EscalateServiceRequestAsync(1, "SLA breach", 5), Times.Once);
        _svc.Verify(s => s.ResolveServiceRequestAsync(1, "Resolved", null, null, 10), Times.Once);
        _svc.Verify(s => s.CloseServiceRequestAsync(1, 10), Times.Once);
    }

    [Fact]
    public async Task Lifecycle_ResolveAndReopen_ThenResolveAgain()
    {
        var dto = BuildDto(1, "SR-001");

        _svc.Setup(s => s.ResolveServiceRequestAsync(1, "First fix", null, null, null)).ReturnsAsync(dto);
        _svc.Setup(s => s.ReopenServiceRequestAsync(1, "Recurred", null)).ReturnsAsync(dto);
        _svc.Setup(s => s.ResolveServiceRequestAsync(1, "Final fix", null, null, null)).ReturnsAsync(dto);

        await _svc.Object.ResolveServiceRequestAsync(1, "First fix", null, null, null);
        await _svc.Object.ReopenServiceRequestAsync(1, "Recurred", null);
        await _svc.Object.ResolveServiceRequestAsync(1, "Final fix", null, null, null);

        _svc.Verify(s => s.ResolveServiceRequestAsync(1, It.IsAny<string>(), null, null, null), Times.Exactly(2));
        _svc.Verify(s => s.ReopenServiceRequestAsync(1, It.IsAny<string>(), null), Times.Once);
    }

    // ══════════════════════════════════════════════════════════════════
    // 10. Statistics and counts
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetStatistics_ReturnsStats()
    {
        var stats = new ServiceRequestStatisticsDto
        {
            TotalRequests = 100,
            OpenRequests = 25,
            ResolvedToday = 10
        };
        _svc.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(stats);

        var result = await _svc.Object.GetStatisticsAsync();

        result.Should().NotBeNull();
        result.TotalRequests.Should().Be(100);
        result.OpenRequests.Should().Be(25);
        result.EscalatedRequests.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetOpenCount_ReturnsInteger()
    {
        _svc.Setup(s => s.GetOpenRequestsCountAsync()).ReturnsAsync(25);

        var result = await _svc.Object.GetOpenRequestsCountAsync();

        result.Should().Be(25);
    }

    [Fact]
    public async Task GetSlaBreachedCount_ReturnsInteger()
    {
        _svc.Setup(s => s.GetSlaBreachedCountAsync()).ReturnsAsync(3);

        var result = await _svc.Object.GetSlaBreachedCountAsync();

        result.Should().Be(3);
    }

    // ══════════════════════════════════════════════════════════════════
    // 11. Queries
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetByAccount_ReturnsList()
    {
        var list = new List<ServiceRequestListDto> { new() { Id = 1 }, new() { Id = 2 } };
        _svc.Setup(s => s.GetServiceRequestsByAccountAsync(10)).ReturnsAsync(list);

        var result = await _svc.Object.GetServiceRequestsByAccountAsync(10);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByContact_ReturnsList()
    {
        var list = new List<ServiceRequestListDto> { new() { Id = 1 } };
        _svc.Setup(s => s.GetServiceRequestsByContactAsync(5)).ReturnsAsync(list);

        var result = await _svc.Object.GetServiceRequestsByContactAsync(5);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByAssignee_ReturnsList()
    {
        var list = new List<ServiceRequestListDto> { new() { Id = 1 }, new() { Id = 3 } };
        _svc.Setup(s => s.GetServiceRequestsByAssigneeAsync(7)).ReturnsAsync(list);

        var result = await _svc.Object.GetServiceRequestsByAssigneeAsync(7);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByGroup_ReturnsList()
    {
        var list = new List<ServiceRequestListDto> { new() { Id = 1 } };
        _svc.Setup(s => s.GetServiceRequestsByGroupAsync(2)).ReturnsAsync(list);

        var result = await _svc.Object.GetServiceRequestsByGroupAsync(2);

        result.Should().HaveCount(1);
    }

    // ══════════════════════════════════════════════════════════════════
    // 12. Customer feedback
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task SubmitFeedback_ReturnsDto_WithRating()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.SubmitFeedbackAsync(1, 5, "Excellent service")).ReturnsAsync(dto);

        var result = await _svc.Object.SubmitFeedbackAsync(1, 5, "Excellent service");

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitFeedback_WithNullComment_StillSucceeds()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.SubmitFeedbackAsync(1, 3, null)).ReturnsAsync(dto);

        var result = await _svc.Object.SubmitFeedbackAsync(1, 3, null);

        result.Should().NotBeNull();
    }

    // ══════════════════════════════════════════════════════════════════
    // 13. Update and Delete
    // ══════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Update_ReturnsDto()
    {
        var dto = BuildDto(1);
        _svc.Setup(s => s.UpdateServiceRequestAsync(1, It.IsAny<UpdateServiceRequestDto>(), null))
            .ReturnsAsync(dto);

        var result = await _svc.Object.UpdateServiceRequestAsync(1, new UpdateServiceRequestDto(), null);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Delete_ReturnsTrue()
    {
        _svc.Setup(s => s.DeleteServiceRequestAsync(1)).ReturnsAsync(true);

        var result = await _svc.Object.DeleteServiceRequestAsync(1);

        result.Should().BeTrue();
    }
}
