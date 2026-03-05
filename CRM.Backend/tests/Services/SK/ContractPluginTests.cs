// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Text.Json;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.AI.SK.Plugins;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.SK;

/// <summary>
/// Unit tests for the ContractPlugin Semantic Kernel plugin.
/// </summary>
public class ContractPluginTests
{
    private readonly Mock<IContractService> _contractServiceMock;
    private readonly Mock<ILogger<ContractPlugin>> _loggerMock;
    private readonly ContractPlugin _sut;

    public ContractPluginTests()
    {
        _contractServiceMock = new Mock<IContractService>();
        _loggerMock = new Mock<ILogger<ContractPlugin>>();
        _sut = new ContractPlugin(_contractServiceMock.Object, _loggerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenContractServiceIsNull()
    {
        var act = () => new ContractPlugin(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("contractService");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        var act = () => new ContractPlugin(_contractServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Plugin Metadata Tests

    [Fact]
    public void PluginName_ShouldReturn_Contract()
    {
        _sut.PluginName.Should().Be("Contract");
    }

    [Fact]
    public void Description_ShouldNotBeNullOrEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetContractAsync Tests

    [Fact]
    public async Task GetContractAsync_ShouldReturnSuccessJson_WhenContractFound()
    {
        var contract = new Contract
        {
            Id = 1,
            ContractNumber = "CON-001",
            Name = "Support Agreement",
            Status = ContractStatus.Active,
            ContractType = ContractType.Service,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(335),
            Value = 5000m
        };
        _contractServiceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contract);

        var result = await _sut.GetContractAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("contractNumber").GetString().Should().Be("CON-001");
    }

    [Fact]
    public async Task GetContractAsync_ShouldReturnFoundFalseJson_WhenContractNotFound()
    {
        _contractServiceMock
            .Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contract?)null);

        var result = await _sut.GetContractAsync(99);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("data").GetProperty("found").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetContractAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contractServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB failure"));

        var result = await _sut.GetContractAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region SearchContractsAsync Tests

    [Fact]
    public async Task SearchContractsAsync_ShouldCallSearchAsync_WhenSearchTermProvided()
    {
        var contracts = new List<Contract>
        {
            new Contract { Id = 1, ContractNumber = "CON-001", Name = "Support", Status = ContractStatus.Active, ContractType = ContractType.Service, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) }
        };
        _contractServiceMock
            .Setup(s => s.SearchAsync("support", It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        var result = await _sut.SearchContractsAsync(searchTerm: "support");

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
        _contractServiceMock.Verify(s => s.SearchAsync("support", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchContractsAsync_ShouldCallGetAllAsync_WhenNoSearchTerm()
    {
        var contracts = new List<Contract>();
        _contractServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<ContractStatus?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        var result = await _sut.SearchContractsAsync(accountId: 5);

        _contractServiceMock.Verify(s => s.GetAllAsync(5, null, It.IsAny<CancellationToken>()), Times.Once);
        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task SearchContractsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contractServiceMock
            .Setup(s => s.GetAllAsync(It.IsAny<int?>(), It.IsAny<ContractStatus?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _sut.SearchContractsAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetExpiringContractsAsync Tests

    [Fact]
    public async Task GetExpiringContractsAsync_ShouldReturnSuccessJson()
    {
        var contracts = new List<Contract>
        {
            new Contract { Id = 1, ContractNumber = "CON-001", Name = "Expiring", Status = ContractStatus.Active, ContractType = ContractType.Service, EndDate = DateTime.UtcNow.AddDays(10) }
        };
        _contractServiceMock
            .Setup(s => s.GetContractsDueForRenewalAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        var result = await _sut.GetExpiringContractsAsync(30);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetExpiringContractsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contractServiceMock
            .Setup(s => s.GetContractsDueForRenewalAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Timeout"));

        var result = await _sut.GetExpiringContractsAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetActiveContractsAsync Tests

    [Fact]
    public async Task GetActiveContractsAsync_ShouldReturnSuccessJson()
    {
        var contracts = new List<Contract>
        {
            new Contract { Id = 1, ContractNumber = "CON-001", Name = "Active Contract", ContractType = ContractType.Service, StartDate = DateTime.UtcNow.AddDays(-60), EndDate = DateTime.UtcNow.AddDays(305) }
        };
        _contractServiceMock
            .Setup(s => s.GetActiveContractsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contracts);

        var result = await _sut.GetActiveContractsAsync(10);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveContractsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contractServiceMock
            .Setup(s => s.GetActiveContractsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _sut.GetActiveContractsAsync(1);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion

    #region GetContractStatisticsAsync Tests

    [Fact]
    public async Task GetContractStatisticsAsync_ShouldReturnSuccessJson()
    {
        var stats = new ContractStatistics
        {
            TotalContracts = 20,
            ActiveContracts = 15,
            TotalContractValue = 100000m
        };
        _contractServiceMock
            .Setup(s => s.GetStatisticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var result = await _sut.GetContractStatisticsAsync(90);

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task GetContractStatisticsAsync_ShouldReturnErrorJson_WhenServiceThrows()
    {
        _contractServiceMock
            .Setup(s => s.GetStatisticsAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Stats failure"));

        var result = await _sut.GetContractStatisticsAsync();

        var doc = JsonDocument.Parse(result);
        doc.RootElement.GetProperty("error").GetBoolean().Should().BeTrue();
    }

    #endregion
}
