// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for ContractsController.
/// Tests contract lifecycle endpoints including CRUD, approval, and termination workflows.
/// </summary>
public class ContractsControllerTests
{
    private readonly Mock<IContractService> _mockContractService;
    private readonly Mock<IContractExportService> _mockExportService;
    private readonly Mock<ILogger<ContractsController>> _mockLogger;
    private readonly ContractsController _controller;

    public ContractsControllerTests()
    {
        _mockContractService = new Mock<IContractService>();
        _mockExportService = new Mock<IContractExportService>();
        _mockLogger = new Mock<ILogger<ContractsController>>();
        _controller = new ContractsController(_mockContractService.Object, _mockExportService.Object, _mockLogger.Object);
    }

    private static Contract CreateContract(int id = 1, ContractStatus status = ContractStatus.Draft) => new()
    {
        Id = id,
        ContractNumber = $"CON-001-{id}",
        Name = "Test Contract",
        AccountId = 10,
        Status = status,
        ContractType = ContractType.Service,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddYears(1),
        Value = 10000m,
        CurrencyCode = "USD"
    };

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithNoFilters_ReturnsOkWithPaginatedResult()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateContract(1),
            CreateContract(2)
        };
        _mockContractService
            .Setup(s => s.GetAllAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithAccountIdFilter_PassesFilterToService()
    {
        // Arrange
        _mockContractService
            .Setup(s => s.GetAllAsync(5, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contract>());

        // Act
        var result = await _controller.GetAll(accountId: 5);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mockContractService.Verify(s => s.GetAllAsync(5, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithContractDto()
    {
        // Arrange
        var contract = CreateContract(1);
        _mockContractService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<ContractDto>().Subject;
        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Test Contract");
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockContractService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new ContractsController.CreateContractRequest
        {
            Name = "Service Agreement",
            AccountId = 10,
            Value = 5000m
        };
        var createdContract = CreateContract(1);
        _mockContractService
            .Setup(s => s.CreateAsync(It.IsAny<Contract>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdContract);

        // Act
        var result = await _controller.Create(request);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ContractsController.GetById));
        createdResult.StatusCode.Should().Be(201);
        var dto = createdResult.Value.Should().BeOfType<ContractDto>().Subject;
        dto.Id.Should().Be(1);
    }

    #endregion

    #region Terminate Tests

    [Fact]
    public async Task Terminate_WithValidId_ReturnsOkWithTerminatedDto()
    {
        // Arrange
        var activeContract = CreateContract(1, ContractStatus.Active);
        var terminatedContract = CreateContract(1, ContractStatus.Terminated);

        _mockContractService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeContract);
        _mockContractService
            .Setup(s => s.TerminateAsync(1, It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(terminatedContract);

        var request = new ContractsController.TerminateContractRequest { Reason = "Customer request" };

        // Act
        var result = await _controller.Terminate(1, request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<ContractDto>().Subject;
        dto.Status.Should().Be(ContractStatus.Terminated);
    }

    [Fact]
    public async Task Terminate_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockContractService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        var request = new ContractsController.TerminateContractRequest { Reason = "Test" };

        // Act
        var result = await _controller.Terminate(999, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Approve Tests

    [Fact]
    public async Task Approve_WithContractInPendingApproval_ReturnsOkWithApprovedDto()
    {
        // Arrange
        var pendingContract = CreateContract(1, ContractStatus.PendingApproval);
        var approvedContract = CreateContract(1, ContractStatus.Approved);

        _mockContractService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingContract);
        _mockContractService
            .Setup(s => s.UpdateStatusAsync(1, ContractStatus.Approved, It.IsAny<CancellationToken>()))
            .ReturnsAsync(approvedContract);

        // Act
        var result = await _controller.Approve(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<ContractDto>().Subject;
        dto.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public async Task Approve_WithContractNotInPendingApproval_ReturnsBadRequest()
    {
        // Arrange
        var draftContract = CreateContract(1, ContractStatus.Draft);

        _mockContractService
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(draftContract);

        // Act
        var result = await _controller.Approve(1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Approve_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        _mockContractService
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _controller.Approve(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
