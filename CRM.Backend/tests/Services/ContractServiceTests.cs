// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class ContractServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<ContractService>> _mockLogger;
    private readonly ContractService _service;

    public ContractServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ContractService>>();
        _service = new ContractService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupDbSets(
        List<Contract>? contracts = null,
        List<Quote>? quotes = null,
        List<Order>? orders = null)
    {
        contracts ??= new List<Contract>();
        quotes ??= new List<Quote>();
        orders ??= new List<Order>();

        var mockContracts = MockDbSetFactory.CreateMockDbSet(contracts);
        mockContracts.Setup(m => m.Add(It.IsAny<Contract>())).Callback<Contract>(e => contracts.Add(e));
        mockContracts.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns<object[], CancellationToken>((keys, _) => mockContracts.Object.FindAsync(keys));
        _mockContext.Setup(c => c.Contracts).Returns(mockContracts.Object);

        var mockQuotes = MockDbSetFactory.CreateMockDbSet(quotes);
        _mockContext.Setup(c => c.Quotes).Returns(mockQuotes.Object);

        var mockOrders = MockDbSetFactory.CreateMockDbSet(orders);
        _mockContext.Setup(c => c.Orders).Returns(mockOrders.Object);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Contract CreateTestContract(
        int id = 1,
        ContractStatus status = ContractStatus.Draft,
        decimal value = 50000m,
        int accountId = 10,
        int? parentContractId = null)
    {
        return new Contract
        {
            Id = id,
            ContractNumber = $"CON-{id:D4}",
            Name = $"Contract {id}",
            Status = status,
            Value = value,
            AccountId = accountId,
            ParentContractId = parentContractId,
            ContractType = ContractType.Service,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            AutoRenew = false,
            RenewalNoticeDays = 30,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ========================================================================
    // GetAllAsync
    // ========================================================================
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllContracts_WhenNoFilter()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1),
            CreateTestContract(2),
            CreateTestContract(3)
        };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAccountId()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1, accountId: 10),
            CreateTestContract(2, accountId: 20),
            CreateTestContract(3, accountId: 10)
        };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetAllAsync(accountId: 10);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1, status: ContractStatus.Draft),
            CreateTestContract(2, status: ContractStatus.Active),
            CreateTestContract(3, status: ContractStatus.Active)
        };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetAllAsync(status: ContractStatus.Active);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldExcludeDeleted()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1),
            new Contract { Id = 2, ContractNumber = "DEL", Name = "Deleted", IsDeleted = true, CreatedAt = DateTime.UtcNow }
        };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    // ========================================================================
    // GetByIdAsync
    // ========================================================================
    [Fact]
    public async Task GetByIdAsync_ShouldReturnContract_WhenExists()
    {
        // Arrange
        SetupDbSets(contracts: new List<Contract> { CreateTestContract(1) });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // CreateAsync
    // ========================================================================
    [Fact]
    public async Task CreateAsync_ShouldAlwaysGenerateContractNumber()
    {
        // Arrange
        var contracts = new List<Contract>();
        SetupDbSets(contracts: contracts);

        var newContract = new Contract
        {
            Name = "New Service Agreement",
            AccountId = 10,
            Value = 25000m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };

        // Act
        var result = await _service.CreateAsync(newContract);

        // Assert
        result.Should().NotBeNull();
        result.ContractNumber.Should().NotBeNullOrEmpty();
        result.ContractNumber.Should().StartWith("CON-");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ========================================================================
    // DeleteAsync (Soft Delete)
    // ========================================================================
    [Fact]
    public async Task DeleteAsync_ShouldSoftDelete_WhenExists()
    {
        // Arrange
        var contract = CreateTestContract(1);
        SetupDbSets(contracts: new List<Contract> { contract });

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        contract.IsDeleted.Should().BeTrue();
    }

    // ========================================================================
    // Status Transitions
    // ========================================================================
    [Fact]
    public async Task ActivateAsync_ShouldSetActiveAndActivatedDate()
    {
        // Arrange
        var contract = CreateTestContract(1, status: ContractStatus.Draft);
        SetupDbSets(contracts: new List<Contract> { contract });

        // Act
        var result = await _service.ActivateAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.Active);
        result.ActivatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task SuspendAsync_ShouldSetOnHoldWithReason()
    {
        // Arrange
        var contract = CreateTestContract(1, status: ContractStatus.Active);
        SetupDbSets(contracts: new List<Contract> { contract });

        // Act
        var result = await _service.SuspendAsync(1, "Payment overdue");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.OnHold);
        result.SuspensionReason.Should().Be("Payment overdue");
    }

    [Fact]
    public async Task TerminateAsync_ShouldSetTerminatedWithReasonAndDate()
    {
        // Arrange
        var contract = CreateTestContract(1, status: ContractStatus.Active);
        SetupDbSets(contracts: new List<Contract> { contract });

        // Act
        var result = await _service.TerminateAsync(1, "Breach of terms");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.Terminated);
        result.TerminationReason.Should().Be("Breach of terms");
        result.TerminatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ExpireAsync_ShouldSetExpiredStatus()
    {
        // Arrange
        var contract = CreateTestContract(1, status: ContractStatus.Active);
        SetupDbSets(contracts: new List<Contract> { contract });

        // Act
        var result = await _service.ExpireAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.Expired);
    }

    // ========================================================================
    // Renewal
    // ========================================================================
    [Fact]
    public async Task CloneForRenewalAsync_ShouldCreateNewContractWithExtendedDates()
    {
        // Arrange
        var contract = CreateTestContract(1, status: ContractStatus.Active, value: 50000m);
        contract.StartDate = DateTime.UtcNow.AddYears(-1);
        contract.EndDate = DateTime.UtcNow;
        var contracts = new List<Contract> { contract };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.CloneForRenewalAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ContractStatus.Draft);
        result.Value.Should().Be(50000m);
        result.ParentContractId.Should().Be(1);
    }

    [Fact]
    public async Task CompleteRenewalAsync_ShouldSetOriginalToRenewed()
    {
        // Arrange
        var original = CreateTestContract(1, status: ContractStatus.Active);
        var renewed = CreateTestContract(2, status: ContractStatus.Draft);
        SetupDbSets(contracts: new List<Contract> { original, renewed });

        // Act
        var result = await _service.CompleteRenewalAsync(1, 2);

        // Assert
        result.Should().NotBeNull();
        original.Status.Should().Be(ContractStatus.Renewed);
        renewed.ParentContractId.Should().Be(1);
    }

    [Fact]
    public async Task GetContractsDueForRenewalAsync_ShouldReturnExpiringContracts()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1, status: ContractStatus.Active),
            CreateTestContract(2, status: ContractStatus.Active),
            CreateTestContract(3, status: ContractStatus.Expired)
        };
        contracts[0].EndDate = DateTime.UtcNow.AddDays(15);
        contracts[1].EndDate = DateTime.UtcNow.AddDays(60);
        contracts[2].EndDate = DateTime.UtcNow.AddDays(-5);
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetContractsDueForRenewalAsync(30);

        // Assert
        result.Should().HaveCountGreaterOrEqualTo(1);
    }

    // ========================================================================
    // Amendment
    // ========================================================================
    [Fact]
    public async Task CreateAmendmentAsync_ShouldCreateWithAmendmentNumber()
    {
        // Arrange
        var contract = CreateTestContract(1);
        var contracts = new List<Contract> { contract };
        SetupDbSets(contracts: contracts);

        var amendment = new Contract
        {
            Name = "Amendment to Service Agreement",
            Value = 10000m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1)
        };

        // Act
        var result = await _service.CreateAmendmentAsync(1, amendment);

        // Assert
        result.Should().NotBeNull();
        result.ParentContractId.Should().Be(1);
        result.ContractType.Should().Be(ContractType.Amendment);
    }

    // ========================================================================
    // CreateFromOrderAsync
    // ========================================================================
    [Fact]
    public async Task CreateFromOrderAsync_ShouldCreateContractFromOrder()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            OrderNumber = "ORD-0001",
            AccountId = 10,
            TotalAmount = 30000m,
            Status = OrderStatus.Approved,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        var contracts = new List<Contract>();
        SetupDbSets(contracts: contracts, orders: new List<Order> { order });

        // Act
        var result = await _service.CreateFromOrderAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.AccountId.Should().Be(10);
        result.Value.Should().Be(30000m);
    }

    // ========================================================================
    // GenerateContractNumberAsync
    // ========================================================================
    [Fact]
    public async Task GenerateContractNumberAsync_ShouldReturnFormattedNumber()
    {
        // Arrange
        SetupDbSets();

        // Act
        var result = await _service.GenerateContractNumberAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("CON-");
    }

    // ========================================================================
    // GetStatisticsAsync
    // ========================================================================
    [Fact]
    public async Task GetStatisticsAsync_ShouldReturnCorrectCounts()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1, status: ContractStatus.Active, value: 50000m),
            CreateTestContract(2, status: ContractStatus.Active, value: 30000m),
            CreateTestContract(3, status: ContractStatus.Expired, value: 20000m)
        };
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.GetStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalContracts.Should().Be(3);
        result.ActiveContracts.Should().Be(2);
        result.ActiveContractValue.Should().Be(80000m);
    }

    // ========================================================================
    // SearchAsync
    // ========================================================================
    [Fact]
    public async Task SearchAsync_ShouldMatchByNameOrNumber()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            CreateTestContract(1),
            CreateTestContract(2)
        };
        contracts[0].Name = "Enterprise Support Agreement";
        contracts[1].Name = "Basic Maintenance";
        SetupDbSets(contracts: contracts);

        // Act
        var result = await _service.SearchAsync("Enterprise");

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Contain("Enterprise");
    }
}
