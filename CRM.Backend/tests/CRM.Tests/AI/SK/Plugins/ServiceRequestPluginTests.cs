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
using CRM.Infrastructure.Data;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.AI.SK.Plugins;

#nullable enable

/// <summary>
/// Unit tests for <see cref="ServiceRequestPlugin"/>.
/// Validates ticket operations, SLA status, priority updates, and comments.
/// </summary>
public class ServiceRequestPluginTests
{
    #region Fields & Setup

    private readonly Mock<IServiceRequestService> _serviceRequestServiceMock = new();
    private readonly Mock<ICrmDbContext> _dbContextMock = new();
    private readonly Mock<ILogger<ServiceRequestPlugin>> _loggerMock = new();
    private readonly ServiceRequestPlugin _plugin;

    public ServiceRequestPluginTests()
    {
        _plugin = new ServiceRequestPlugin(
            _serviceRequestServiceMock.Object,
            _dbContextMock.Object,
            _loggerMock.Object);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PluginName_ShouldReturnServiceRequest()
    {
        _plugin.PluginName.Should().Be("ServiceRequest");
    }

    [Fact]
    public void Description_ShouldNotBeEmpty()
    {
        _plugin.Description.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Constructor Validation

    [Fact]
    public void Constructor_NullService_ShouldThrow()
    {
        var act = () => new ServiceRequestPlugin(null!, _dbContextMock.Object, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullDbContext_ShouldThrow()
    {
        var act = () => new ServiceRequestPlugin(_serviceRequestServiceMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullLogger_ShouldThrow()
    {
        var act = () => new ServiceRequestPlugin(_serviceRequestServiceMock.Object, _dbContextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetTicketAsync Tests

    [Fact]
    public async Task GetTicketAsync_ExistingTicket_ShouldReturnSuccess()
    {
        // Arrange
        var ticket = new ServiceRequestDto { Id = 1, Subject = "Printer not working" };
        _serviceRequestServiceMock.Setup(s => s.GetServiceRequestByIdAsync(1))
            .ReturnsAsync(ticket);

        // Act
        var result = await _plugin.GetTicketAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetTicketAsync_NonExistent_ShouldReturnError()
    {
        // Arrange
        _serviceRequestServiceMock.Setup(s => s.GetServiceRequestByIdAsync(999))
            .ReturnsAsync((ServiceRequestDto?)null);

        // Act
        var result = await _plugin.GetTicketAsync(999);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchTicketsAsync Tests

    [Fact]
    public async Task SearchTicketsAsync_ValidQuery_ShouldReturnResults()
    {
        // Arrange
        var pagedResult = new PagedServiceRequestResult
        {
            Items = new List<ServiceRequestListDto>
            {
                new() { Id = 1, Subject = "Printer issue" },
                new() { Id = 2, Subject = "Network down" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        _serviceRequestServiceMock
            .Setup(s => s.GetServiceRequestsAsync(It.IsAny<ServiceRequestFilterDto>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _plugin.SearchTicketsAsync("printer");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region AssignTicketAsync Tests

    [Fact]
    public async Task AssignTicketAsync_ValidParams_ShouldReturnResult()
    {
        // Arrange
        var ticket = new ServiceRequestDto { Id = 1, Subject = "Test" };
        _serviceRequestServiceMock.Setup(s => s.AssignToUserAsync(1, 5, It.IsAny<int?>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _plugin.AssignTicketAsync(1, 5);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region UpdatePriorityAsync Tests

    [Fact]
    public async Task UpdatePriorityAsync_ValidPriority_ShouldReturnResult()
    {
        // Arrange
        var ticketList = new List<ServiceRequest>
        {
            new() { Id = 1, Subject = "Test", IsDeleted = false }
        };
        var mockDbSet = MockDbSetFactory.CreateMockDbSet(ticketList);
        _dbContextMock.Setup(c => c.ServiceRequests).Returns(mockDbSet.Object);
        _dbContextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _plugin.UpdatePriorityAsync(1, "High");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region CloseTicketAsync Tests

    [Fact]
    public async Task CloseTicketAsync_ExistingTicket_ShouldReturnResult()
    {
        // Arrange
        var ticket = new ServiceRequestDto { Id = 1, Subject = "Test" };
        _serviceRequestServiceMock.Setup(s => s.CloseServiceRequestAsync(1, It.IsAny<int?>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _plugin.CloseTicketAsync(1);

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region ResolveTicketAsync Tests

    [Fact]
    public async Task ResolveTicketAsync_ValidResolution_ShouldReturnResult()
    {
        // Arrange
        var ticket = new ServiceRequestDto { Id = 1, Subject = "Test" };
        _serviceRequestServiceMock.Setup(s => s.ResolveServiceRequestAsync(1, "Replaced toner cartridge", It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .ReturnsAsync(ticket);

        // Act
        var result = await _plugin.ResolveTicketAsync(1, "Replaced toner cartridge");

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetTicketAsync_ServiceThrows_ShouldReturnError()
    {
        // Arrange
        _serviceRequestServiceMock.Setup(s => s.GetServiceRequestByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));

        // Act
        var result = await _plugin.GetTicketAsync(1);

        // Assert
        using var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
