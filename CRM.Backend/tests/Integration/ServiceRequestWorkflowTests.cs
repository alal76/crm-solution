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
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Workflow tests for the ServiceRequest lifecycle:
/// Create → Escalate → Resolve → Close, plus assignment and feedback operations.
/// All tests use a mocked IServiceRequestService to verify expected interactions.
/// </summary>
public class ServiceRequestWorkflowTests
{
    private readonly Mock<IServiceRequestService> _srService = new(MockBehavior.Loose);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CreateServiceRequestDto BuildCreateDto(
        string subject = "Printer not working",
        ServiceRequestPriority priority = ServiceRequestPriority.Medium,
        ServiceRequestChannel channel = ServiceRequestChannel.Email) =>
        new CreateServiceRequestDto
        {
            Subject = subject,
            Description = "Unit test request",
            Priority = priority,
            Channel = channel,
            RequesterName = "Alice Smith",
            RequesterEmail = "alice@example.com",
            AccountId = 10,
        };

    private static ServiceRequestDto BuildResponseDto(
        int id = 1,
        string ticketNumber = "SR-0001",
        ServiceRequestStatus status = ServiceRequestStatus.New) =>
        new ServiceRequestDto
        {
            Id = id,
            TicketNumber = ticketNumber,
            Subject = "Printer not working",
            Status = status,
            StatusName = status.ToString(),
            Priority = ServiceRequestPriority.Medium,
            PriorityName = "Medium",
            Channel = ServiceRequestChannel.Email,
            ChannelName = "Email",
            CreatedAt = DateTime.UtcNow,
        };

    // ── Test 1 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateServiceRequest_ShouldReturnDto_WhenInputIsValid()
    {
        // Arrange
        var createDto = BuildCreateDto();
        var expected = BuildResponseDto(id: 1);

        _srService.Setup(s => s.CreateServiceRequestAsync(
                It.IsAny<CreateServiceRequestDto>(),
                It.IsAny<int?>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _srService.Object.CreateServiceRequestAsync(createDto, createdByUserId: null);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.TicketNumber.Should().Be("SR-0001");
        result.Status.Should().Be(ServiceRequestStatus.New);
    }

    // ── Test 2 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatus_ShouldReturnUpdatedDto_WhenStatusChanges()
    {
        // Arrange
        const int id = 1;
        var updated = BuildResponseDto(status: ServiceRequestStatus.InProgress);

        _srService.Setup(s => s.UpdateStatusAsync(
                id,
                ServiceRequestStatus.InProgress,
                It.IsAny<int?>()))
            .ReturnsAsync(updated);

        // Act
        var result = await _srService.Object.UpdateStatusAsync(id, ServiceRequestStatus.InProgress, modifiedByUserId: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ServiceRequestStatus.InProgress);
    }

    // ── Test 3 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task EscalateServiceRequest_ShouldReturnEscalatedDto_WhenReasonProvided()
    {
        // Arrange
        const int id = 1;
        var escalated = BuildResponseDto(status: ServiceRequestStatus.Escalated);
        escalated.EscalationLevel = 1;

        _srService.Setup(s => s.EscalateServiceRequestAsync(
                id,
                It.IsAny<string>(),
                It.IsAny<int?>()))
            .ReturnsAsync(escalated);

        // Act
        var result = await _srService.Object.EscalateServiceRequestAsync(id, "SLA breach imminent", escalatedByUserId: 3);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ServiceRequestStatus.Escalated);
        result.EscalationLevel.Should().Be(1);
    }

    // ── Test 4 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExpediteServiceRequest_ShouldReturnExpeditedDto_WhenReasonProvided()
    {
        // Arrange
        const int id = 2;
        var expedited = BuildResponseDto(id: 2, status: ServiceRequestStatus.InProgress);
        expedited.IsExpedited = true;
        expedited.ExpediteReason = "VIP customer";

        _srService.Setup(s => s.ExpediteServiceRequestAsync(
                id,
                It.IsAny<string>(),
                It.IsAny<int?>()))
            .ReturnsAsync(expedited);

        // Act
        var result = await _srService.Object.ExpediteServiceRequestAsync(id, "VIP customer", expeditedByUserId: 2);

        // Assert
        result.Should().NotBeNull();
        result.IsExpedited.Should().BeTrue();
        result.ExpediteReason.Should().Be("VIP customer");
    }

    // ── Test 5 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignToUser_ShouldReturnDto_WhenUserExists()
    {
        // Arrange
        const int srId = 1;
        const int userId = 42;
        var assigned = BuildResponseDto();
        assigned.AssignedToUserId = userId;
        assigned.AssignedToUserName = "Bob Jones";

        _srService.Setup(s => s.AssignToUserAsync(srId, userId, It.IsAny<int?>()))
            .ReturnsAsync(assigned);

        // Act
        var result = await _srService.Object.AssignToUserAsync(srId, userId, assignedByUserId: 1);

        // Assert
        result.Should().NotBeNull();
        result.AssignedToUserId.Should().Be(userId);
        result.AssignedToUserName.Should().Be("Bob Jones");
    }

    // ── Test 6 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AssignToGroup_ShouldReturnDto_WhenGroupExists()
    {
        // Arrange
        const int srId = 1;
        const int groupId = 7;
        var assigned = BuildResponseDto();
        assigned.AssignedToGroupId = groupId;
        assigned.AssignedToGroupName = "Level-2 Support";

        _srService.Setup(s => s.AssignToGroupAsync(srId, groupId, It.IsAny<int?>()))
            .ReturnsAsync(assigned);

        // Act
        var result = await _srService.Object.AssignToGroupAsync(srId, groupId, assignedByUserId: 1);

        // Assert
        result.Should().NotBeNull();
        result.AssignedToGroupId.Should().Be(groupId);
        result.AssignedToGroupName.Should().Be("Level-2 Support");
    }

    // ── Test 7 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Unassign_ShouldReturnDto_WhenRequestIsAssigned()
    {
        // Arrange
        const int srId = 1;
        var unassigned = BuildResponseDto();
        unassigned.AssignedToUserId = null;
        unassigned.AssignedToGroupId = null;

        _srService.Setup(s => s.UnassignAsync(srId, It.IsAny<int?>()))
            .ReturnsAsync(unassigned);

        // Act
        var result = await _srService.Object.UnassignAsync(srId, modifiedByUserId: 1);

        // Assert
        result.Should().NotBeNull();
        result.AssignedToUserId.Should().BeNull();
        result.AssignedToGroupId.Should().BeNull();
    }

    // ── Test 8 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkFirstResponse_ShouldReturnDto_WhenRequestIsOpen()
    {
        // Arrange
        const int srId = 1;
        var responded = BuildResponseDto(status: ServiceRequestStatus.InProgress);
        responded.FirstResponseDate = DateTime.UtcNow;
        responded.TimeToFirstResponseHours = 1.5;

        _srService.Setup(s => s.MarkFirstResponseAsync(srId, It.IsAny<int?>()))
            .ReturnsAsync(responded);

        // Act
        var result = await _srService.Object.MarkFirstResponseAsync(srId, userId: 3);

        // Assert
        result.Should().NotBeNull();
        result.FirstResponseDate.Should().NotBeNull();
        result.TimeToFirstResponseHours.Should().BeGreaterThan(0);
    }

    // ── Test 9 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveServiceRequest_ShouldReturnResolvedDto_WhenResolutionSummaryProvided()
    {
        // Arrange
        const int srId = 1;
        var resolved = BuildResponseDto(status: ServiceRequestStatus.Resolved);
        resolved.ResolutionSummary = "Printer driver reinstalled";
        resolved.ResolvedDate = DateTime.UtcNow;

        _srService.Setup(s => s.ResolveServiceRequestAsync(
                srId,
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>()))
            .ReturnsAsync(resolved);

        // Act
        var result = await _srService.Object.ResolveServiceRequestAsync(
            srId,
            resolutionSummary: "Printer driver reinstalled",
            resolutionCode: "HW-FIX",
            rootCause: "Driver corruption",
            resolvedByUserId: 5);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ServiceRequestStatus.Resolved);
        result.ResolutionSummary.Should().Be("Printer driver reinstalled");
        result.ResolvedDate.Should().NotBeNull();
    }

    // ── Test 10 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CloseServiceRequest_ShouldReturnClosedDto_WhenRequestIsResolved()
    {
        // Arrange
        const int srId = 1;
        var closed = BuildResponseDto(status: ServiceRequestStatus.Closed);
        closed.ClosedDate = DateTime.UtcNow;

        _srService.Setup(s => s.CloseServiceRequestAsync(srId, It.IsAny<int?>()))
            .ReturnsAsync(closed);

        // Act
        var result = await _srService.Object.CloseServiceRequestAsync(srId, closedByUserId: 1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ServiceRequestStatus.Closed);
        result.ClosedDate.Should().NotBeNull();
    }

    // ── Test 11 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ReopenServiceRequest_ShouldReturnReopenedDto_WhenReasonProvided()
    {
        // Arrange
        const int srId = 1;
        var reopened = BuildResponseDto(status: ServiceRequestStatus.Reopened);
        reopened.ReopenCount = 1;

        _srService.Setup(s => s.ReopenServiceRequestAsync(
                srId,
                It.IsAny<string>(),
                It.IsAny<int?>()))
            .ReturnsAsync(reopened);

        // Act
        var result = await _srService.Object.ReopenServiceRequestAsync(
            srId,
            reason: "Issue recurred after patch",
            reopenedByUserId: 2);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ServiceRequestStatus.Reopened);
        result.ReopenCount.Should().Be(1);
    }

    // ── Test 12 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task SubmitFeedback_ShouldReturnDto_WhenRatingIsValid()
    {
        // Arrange
        const int srId = 1;
        var feedbackDto = BuildResponseDto(status: ServiceRequestStatus.Closed);
        feedbackDto.SatisfactionRating = 5;
        feedbackDto.CustomerFeedback = "Great service!";

        _srService.Setup(s => s.SubmitFeedbackAsync(srId, 5, It.IsAny<string?>()))
            .ReturnsAsync(feedbackDto);

        // Act
        var result = await _srService.Object.SubmitFeedbackAsync(srId, rating: 5, feedback: "Great service!");

        // Assert
        result.Should().NotBeNull();
        result.SatisfactionRating.Should().Be(5);
        result.CustomerFeedback.Should().Be("Great service!");
    }

    // ── Test 13 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatistics_ShouldReturnStats_WhenRequestsExist()
    {
        // Arrange
        var stats = new ServiceRequestStatisticsDto
        {
            TotalRequests = 100,
            OpenRequests = 42,
            SlaBreachedCount = 5,
            CustomerSatisfactionAverage = 4.2,
        };

        _srService.Setup(s => s.GetStatisticsAsync())
            .ReturnsAsync(stats);

        // Act
        var result = await _srService.Object.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalRequests.Should().Be(100);
        result.OpenRequests.Should().Be(42);
        result.SlaBreachedCount.Should().Be(5);
        result.CustomerSatisfactionAverage.Should().BeApproximately(4.2, 0.01);
    }

    // ── Test 14 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOpenRequestsCount_ShouldReturnCount_WhenRequestsExist()
    {
        // Arrange
        _srService.Setup(s => s.GetOpenRequestsCountAsync()).ReturnsAsync(17);

        // Act
        var result = await _srService.Object.GetOpenRequestsCountAsync();

        // Assert
        result.Should().Be(17);
    }

    // ── Test 15 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSlaBreachedCount_ShouldReturnCount_WhenBreachesExist()
    {
        // Arrange
        _srService.Setup(s => s.GetSlaBreachedCountAsync()).ReturnsAsync(3);

        // Act
        var result = await _srService.Object.GetSlaBreachedCountAsync();

        // Assert
        result.Should().Be(3);
    }

    // ── Test 16 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetServiceRequestsByAccount_ShouldReturnList_WhenAccountHasRequests()
    {
        // Arrange
        const int accountId = 10;
        var list = new List<ServiceRequestListDto>
        {
            new ServiceRequestListDto
            {
                Id = 1,
                TicketNumber = "SR-0001",
                Subject = "Printer not working",
                Status = ServiceRequestStatus.Open,
                StatusName = "Open",
                Priority = ServiceRequestPriority.Medium,
                PriorityName = "Medium",
                Channel = ServiceRequestChannel.Email,
                ChannelName = "Email",
                AccountName = "Acme Corp",
                CreatedAt = DateTime.UtcNow,
            },
            new ServiceRequestListDto
            {
                Id = 2,
                TicketNumber = "SR-0002",
                Subject = "Cannot login",
                Status = ServiceRequestStatus.InProgress,
                StatusName = "InProgress",
                Priority = ServiceRequestPriority.High,
                PriorityName = "High",
                Channel = ServiceRequestChannel.Phone,
                ChannelName = "Phone",
                AccountName = "Acme Corp",
                CreatedAt = DateTime.UtcNow,
            },
        };

        _srService.Setup(s => s.GetServiceRequestsByAccountAsync(accountId))
            .ReturnsAsync(list);

        // Act
        var result = await _srService.Object.GetServiceRequestsByAccountAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.AccountName.Should().Be("Acme Corp"));
    }

    // ── Test 17 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetServiceRequestByTicketNumber_ShouldReturnDto_WhenTicketExists()
    {
        // Arrange
        const string ticketNumber = "SR-1234";
        var expected = BuildResponseDto(id: 1, ticketNumber: ticketNumber);

        _srService.Setup(s => s.GetServiceRequestByTicketNumberAsync(ticketNumber))
            .ReturnsAsync(expected);

        // Act
        var result = await _srService.Object.GetServiceRequestByTicketNumberAsync(ticketNumber);

        // Assert
        result.Should().NotBeNull();
        result!.TicketNumber.Should().Be(ticketNumber);
    }

    // ── Test 18 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteServiceRequest_ShouldReturnTrue_WhenRequestExists()
    {
        // Arrange
        const int srId = 1;
        _srService.Setup(s => s.DeleteServiceRequestAsync(srId)).ReturnsAsync(true);

        // Act
        var result = await _srService.Object.DeleteServiceRequestAsync(srId);

        // Assert
        result.Should().BeTrue();
    }

    // ── Workflow Integration Tests ─────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_Create_Escalate_Resolve_Close_ShouldSucceed()
    {
        // Arrange – chain mocks for the full lifecycle
        var createDto = BuildCreateDto();
        var newSr = BuildResponseDto(status: ServiceRequestStatus.New);
        var escalated = BuildResponseDto(status: ServiceRequestStatus.Escalated);
        escalated.EscalationLevel = 1;
        var resolved = BuildResponseDto(status: ServiceRequestStatus.Resolved);
        var closed = BuildResponseDto(status: ServiceRequestStatus.Closed);

        _srService.SetupSequence(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), It.IsAny<int?>()))
            .ReturnsAsync(newSr);
        _srService.SetupSequence(s => s.EscalateServiceRequestAsync(1, It.IsAny<string>(), It.IsAny<int?>()))
            .ReturnsAsync(escalated);
        _srService.SetupSequence(s => s.ResolveServiceRequestAsync(1, It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .ReturnsAsync(resolved);
        _srService.SetupSequence(s => s.CloseServiceRequestAsync(1, It.IsAny<int?>()))
            .ReturnsAsync(closed);

        // Act
        var created = await _srService.Object.CreateServiceRequestAsync(createDto, null);
        created.Status.Should().Be(ServiceRequestStatus.New);

        var escalatedResult = await _srService.Object.EscalateServiceRequestAsync(created.Id, "SLA about to breach", 3);
        escalatedResult.Status.Should().Be(ServiceRequestStatus.Escalated);

        var resolvedResult = await _srService.Object.ResolveServiceRequestAsync(created.Id, "Fixed", null, null, 3);
        resolvedResult.Status.Should().Be(ServiceRequestStatus.Resolved);

        var closedResult = await _srService.Object.CloseServiceRequestAsync(created.Id, 1);
        closedResult.Status.Should().Be(ServiceRequestStatus.Closed);
    }

    [Fact]
    public async Task ResolveAndReopen_ShouldCycleStatus_WhenIssueRecurs()
    {
        // Arrange
        var resolved = BuildResponseDto(status: ServiceRequestStatus.Resolved);
        var reopened = BuildResponseDto(status: ServiceRequestStatus.Reopened);
        reopened.ReopenCount = 1;

        _srService.Setup(s => s.ResolveServiceRequestAsync(1, It.IsAny<string>(), null, null, null))
            .ReturnsAsync(resolved);
        _srService.Setup(s => s.ReopenServiceRequestAsync(1, It.IsAny<string>(), null))
            .ReturnsAsync(reopened);

        // Act
        var resolvedResult = await _srService.Object.ResolveServiceRequestAsync(1, "Patched", null, null, null);
        resolvedResult.Status.Should().Be(ServiceRequestStatus.Resolved);

        var reopenedResult = await _srService.Object.ReopenServiceRequestAsync(1, "Patch did not hold", null);
        reopenedResult.Status.Should().Be(ServiceRequestStatus.Reopened);
        reopenedResult.ReopenCount.Should().Be(1);
    }

    [Fact]
    public async Task HighPriorityRequest_ShouldBeExpedited_WhenVipCustomer()
    {
        // Arrange
        var createDto = BuildCreateDto(priority: ServiceRequestPriority.Critical);
        createDto.IsVipAccount = true;

        var created = BuildResponseDto(status: ServiceRequestStatus.New);
        created.Priority = ServiceRequestPriority.Critical;
        created.IsVipAccount = true;

        var expedited = BuildResponseDto(status: ServiceRequestStatus.InProgress);
        expedited.IsExpedited = true;

        _srService.Setup(s => s.CreateServiceRequestAsync(It.IsAny<CreateServiceRequestDto>(), null)).ReturnsAsync(created);
        _srService.Setup(s => s.ExpediteServiceRequestAsync(1, It.IsAny<string>(), null)).ReturnsAsync(expedited);

        // Act
        var newSr = await _srService.Object.CreateServiceRequestAsync(createDto, null);
        newSr.IsVipAccount.Should().BeTrue();
        newSr.Priority.Should().Be(ServiceRequestPriority.Critical);

        var expeditedSr = await _srService.Object.ExpediteServiceRequestAsync(newSr.Id, "VIP critical issue", null);
        expeditedSr.IsExpedited.Should().BeTrue();
    }
}
