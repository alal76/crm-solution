// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

// TCOV2-D15 — AuditLogsController unit tests
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AuditLogsController (TCOV2-D15).
/// Route: /api/audit-logs
/// Note: RequireRole attribute is custom middleware — not exercised in unit tests (direct instantiation bypasses middleware).
/// </summary>
public class AuditLogsControllerTests
{
    private readonly Mock<IAuditLogService> _mockService;
    private readonly AuditLogsController _controller;

    public AuditLogsControllerTests()
    {
        _mockService = new Mock<IAuditLogService>();
        // exportService is optional — pass null
        _controller = new AuditLogsController(_mockService.Object, null);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── GetAll ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ShouldReturnOk_WithAuditLogs()
    {
        // Arrange
        var page = new AuditLogPageDto
        {
            Items = new List<AuditLogDto>
            {
                new() { Id = 1, Action = "Login", EntityType = "User" }
            },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 50
        };
        _mockService.Setup(s => s.GetAuditLogsAsync(
                It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string?>(),
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().Be(page);
    }

    [Fact]
    public async Task GetAll_CallsServiceOnce()
    {
        _mockService.Setup(s => s.GetAuditLogsAsync(
                null, null, null, null, null, null, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuditLogPageDto { Items = new List<AuditLogDto>() });

        await _controller.GetAll();

        _mockService.Verify(s => s.GetAuditLogsAsync(
            null, null, null, null, null, null, 1, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Create ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await _controller.Create(null!, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenActionIsEmpty()
    {
        // Arrange
        var dto = new CreateAuditLogDto { Action = string.Empty, EntityType = "User" };

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ShouldReturnCreated_WhenDtoIsValid()
    {
        // Arrange
        var dto = new CreateAuditLogDto
        {
            Action = "Export",
            EntityType = "Report",
            EntityId = 42,
            UserId = 1
        };
        _mockService.Setup(s => s.LogActionAsync(
                dto.Action, dto.EntityType, dto.EntityId, dto.UserId, dto.Details,
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        // Act
        var result = await _controller.Create(dto, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        ((CreatedAtActionResult)result).StatusCode.Should().Be(201);
    }
}
