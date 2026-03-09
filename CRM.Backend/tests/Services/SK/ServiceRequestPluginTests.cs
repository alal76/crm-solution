// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the ServiceRequestPlugin Semantic Kernel plugin.
/// </summary>
public class ServiceRequestPluginTests
{
    private readonly Mock<IServiceRequestService> _srServiceMock;
    private readonly Mock<ICrmDbContext> _dbContextMock;
    private readonly Mock<ILogger<ServiceRequestPlugin>> _loggerMock;
    private readonly ServiceRequestPlugin _sut;

    public ServiceRequestPluginTests()
    {
        _srServiceMock = new Mock<IServiceRequestService>();
        _dbContextMock = new Mock<ICrmDbContext>();
        _loggerMock = new Mock<ILogger<ServiceRequestPlugin>>();
        _sut = new ServiceRequestPlugin(_srServiceMock.Object, _dbContextMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenServiceRequestServiceIsNull()
    {
        var act = () => new ServiceRequestPlugin(null!, _dbContextMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceRequestService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDbContextIsNull()
    {
        var act = () => new ServiceRequestPlugin(_srServiceMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new ServiceRequestPlugin(_srServiceMock.Object, _dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_ServiceRequest()
    {
        _sut.PluginName.Should().Be("ServiceRequest");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetTicketAsync Tests

    [Fact]
    public async Task GetTicketAsync_ShouldReturnSuccessJson_WhenTicketExists()
    {
        var ticket = new ServiceRequestDto
        {
            Id = 1,
            Subject = "Cannot login",
            // FIXME-AP059: was Status=ServiceRequestStatus.Open; add .WithStatus(ServiceRequestStatus.Open) after construction

            Priority = ServiceRequestPriority.High
        };
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _sut.GetTicketAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetTicketAsync_ShouldReturnErrorJson_WhenTicketNotFound()
    {
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(99)).ReturnsAsync((ServiceRequestDto?)null);

        var result = await _sut.GetTicketAsync(99);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetTicket");
    }

    [Fact]
    public async Task GetTicketAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _srServiceMock
            .Setup(s => s.GetServiceRequestByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _sut.GetTicketAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchTicketsAsync Tests

    [Fact]
    public async Task SearchTicketsAsync_ShouldReturnSuccessJson_WithMatchingTickets()
    {
        var pagedResult = new PagedServiceRequestResult
        {
            Items = new List<ServiceRequestListDto>
            {
                new ServiceRequestListDto { Id = 1, Subject = "Login issue", Status = ServiceRequestStatus.Open }
            },
            TotalCount = 1
        };
        _srServiceMock
            .Setup(s => s.GetServiceRequestsAsync(It.IsAny<ServiceRequestFilterDto>()))
            .ReturnsAsync(pagedResult);

        var result = await _sut.SearchTicketsAsync("login", 10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("totalCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task SearchTicketsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _srServiceMock
            .Setup(s => s.GetServiceRequestsAsync(It.IsAny<ServiceRequestFilterDto>()))
            .ThrowsAsync(new Exception("Search failure"));

        var result = await _sut.SearchTicketsAsync("test");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetSLAStatusAsync Tests

    [Fact]
    public async Task GetSLAStatusAsync_ShouldReturnSuccessJson_WithSlaData_WhenTicketFound()
    {
        var ticket = new ServiceRequestDto
        {
            Id = 1,
            // FIXME-AP059: was Status=ServiceRequestStatus.InProgress; add .WithStatus(ServiceRequestStatus.InProgress) after construction

            Priority = ServiceRequestPriority.Critical,
            ResponseSlaBreached = false,
            ResolutionSlaBreached = false,
            CreatedAt = DateTime.UtcNow.AddHours(-4)
        };
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(1)).ReturnsAsync(ticket);

        var result = await _sut.GetSLAStatusAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("responseSlaBreached").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetSLAStatusAsync_ShouldReturnErrorJson_WhenTicketNotFound()
    {
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(99)).ReturnsAsync((ServiceRequestDto?)null);

        var result = await _sut.GetSLAStatusAsync(99);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("GetSLAStatus");
    }

    #endregion

    #region AssignTicketAsync Tests

    [Fact]
    public async Task AssignTicketAsync_ShouldReturnSuccessJson_WhenAssignSucceeds()
    {
        var assignedTicket = new ServiceRequestDto { Id = 1, Status = ServiceRequestStatus.InProgress };
        _srServiceMock.Setup(s => s.AssignToUserAsync(1, 5, null)).ReturnsAsync(assignedTicket);

        var result = await _sut.AssignTicketAsync(1, 5);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("assigned").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("data").GetProperty("assignedToUserId").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task AssignTicketAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _srServiceMock
            .Setup(s => s.AssignToUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("User not found"));

        var result = await _sut.AssignTicketAsync(1, 999);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region UpdatePriorityAsync Tests

    [Fact]
    public async Task UpdatePriorityAsync_ShouldReturnSuccessJson_WhenPriorityUpdated()
    {
        var srList = new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, Priority = ServiceRequestPriority.Medium, IsDeleted = false }
        };
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(srList);
        _dbContextMock.Setup(c => c.ServiceRequests).Returns(mockDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdatePriorityAsync(1, "High");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("updated").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePriorityAsync_ShouldReturnErrorJson_WhenTicketNotFound()
    {
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<ServiceRequest>());
        _dbContextMock.Setup(c => c.ServiceRequests).Returns(mockDbSet.Object);

        var result = await _sut.UpdatePriorityAsync(99, "Medium");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("UpdatePriority");
    }

    [Fact]
    public async Task UpdatePriorityAsync_ShouldReturnErrorJson_WhenPriorityIsInvalid()
    {
        // Invalid priority should return ErrorResult without hitting the DB
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(new List<ServiceRequest>
        {
            new ServiceRequest { Id = 1, IsDeleted = false }
        });
        _dbContextMock.Setup(c => c.ServiceRequests).Returns(mockDbSet.Object);

        var result = await _sut.UpdatePriorityAsync(1, "SUPER_URGENT_INVALID");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region AddCommentAsync Tests

    [Fact]
    public async Task AddCommentAsync_ShouldReturnErrorJson_WhenTicketNotFound()
    {
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(99)).ReturnsAsync((ServiceRequestDto?)null);

        var result = await _sut.AddCommentAsync(99, "Test comment");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
        doc.RootElement.GetProperty("operation").GetString().Should().Be("AddComment");
    }

    [Fact]
    public async Task AddCommentAsync_ShouldReturnSuccessJson_WhenTicketExistsAndNoteAdded()
    {
        var ticket = new ServiceRequestDto { Id = 1, Subject = "Test" };
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(1)).ReturnsAsync(ticket);

        var notesList = new List<Note>();
        var notesMock = MockDbSetFactory.CreateMockDbSet(notesList);
        _dbContextMock.Setup(c => c.Notes).Returns(notesMock.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.AddCommentAsync(1, "This ticket needs escalation.");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("ticketId").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task AddCommentAsync_ShouldReturnErrorJson_WhenDbThrows()
    {
        var ticket = new ServiceRequestDto { Id = 1 };
        _srServiceMock.Setup(s => s.GetServiceRequestByIdAsync(1)).ReturnsAsync(ticket);

        var notesMock = MockDbSetFactory.CreateMockDbSet(new List<Note>());
        _dbContextMock.Setup(c => c.Notes).Returns(notesMock.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB write failed"));

        var result = await _sut.AddCommentAsync(1, "Some comment");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region CloseTicketAsync Tests

    [Fact]
    public async Task CloseTicketAsync_ShouldReturnSuccessJson_WhenCloseSucceeds()
    {
        var closed = new ServiceRequestDto { Id = 1, Status = ServiceRequestStatus.Closed };
        _srServiceMock.Setup(s => s.CloseServiceRequestAsync(1, null)).ReturnsAsync(closed);

        var result = await _sut.CloseTicketAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("closed").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task CloseTicketAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _srServiceMock
            .Setup(s => s.CloseServiceRequestAsync(It.IsAny<int>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Ticket already closed"));

        var result = await _sut.CloseTicketAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region ResolveTicketAsync Tests

    [Fact]
    public async Task ResolveTicketAsync_ShouldReturnSuccessJson_WhenResolveSucceeds()
    {
        var resolved = new ServiceRequestDto { Id = 1, Status = ServiceRequestStatus.Resolved };
        _srServiceMock
            .Setup(s => s.ResolveServiceRequestAsync(1, "Rebooted server", null, null, null))
            .ReturnsAsync(resolved);

        var result = await _sut.ResolveTicketAsync(1, "Rebooted server");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("resolved").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task ResolveTicketAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _srServiceMock
            .Setup(s => s.ResolveServiceRequestAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(),
                It.IsAny<string?>(), It.IsAny<int?>()))
            .ThrowsAsync(new Exception("Cannot resolve"));

        var result = await _sut.ResolveTicketAsync(1, "Resolution text");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
