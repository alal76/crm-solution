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

using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class CommissionRuleServiceTests
{
    private readonly Mock<IRepository<CommissionRule>> _mockRuleRepository;
    private readonly Mock<IRepository<CommissionHistory>> _mockHistoryRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<CommissionRuleService>> _mockLogger;
    private readonly CommissionRuleService _service;

    public CommissionRuleServiceTests()
    {
        _mockRuleRepository = new Mock<IRepository<CommissionRule>>();
        _mockHistoryRepository = new Mock<IRepository<CommissionHistory>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CommissionRuleService>>();
        _service = new CommissionRuleService(_mockRuleRepository.Object, _mockHistoryRepository.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedRule()
    {
        // Arrange
        var dto = new CreateCommissionRuleDto
        {
            Name = "Standard Sales",
            SaleType = "DirectSale",
            RuleType = CommissionRuleType.Percentage,
            Rate = 5m
        };

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<CommissionRule>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.SaleType, result.SaleType);
    }

    [Fact]
    public async Task CreateAsync_WithNegativeRate_ThrowsException()
    {
        // Arrange
        var dto = new CreateCommissionRuleDto { Name = "Test", SaleType = "Test", Rate = -5m };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsRule()
    {
        // Arrange
        var rule = new CommissionRule { Id = 1, Name = "Test", SaleType = "DirectSale", Rate = 5m };
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rule);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(rule.Name, result.Name);
    }

    [Fact]
    public async Task CalculateCommissionAsync_WithValidData_ReturnsCalculation()
    {
        // Arrange
        var rule = new CommissionRule
        {
            Id = 1,
            Name = "Percentage",
            SaleType = "DirectSale",
            RuleType = CommissionRuleType.Percentage,
            Rate = 10m,
            IsActive = true,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            ExpiryDate = null
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<CommissionRule> { rule });

        // Act
        var result = await _service.CalculateCommissionAsync(1000m, "DirectSale");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.SalesAmount);
        Assert.Equal(100m, result.CommissionAmount); // 10% of 1000
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_SetsIsDeleted()
    {
        // Arrange
        var rule = new CommissionRule { Id = 1, Name = "Test", IsDeleted = false };
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rule);
        _mockRuleRepository.Setup(r => r.UpdateAsync(It.IsAny<CommissionRule>(), It.IsAny<CancellationToken>()));

        // Act
        await _service.DeleteAsync(1);

        // Assert
        Assert.True(rule.IsDeleted);
        _mockRuleRepository.Verify(r => r.UpdateAsync(rule, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class DiscountRuleServiceTests
{
    private readonly Mock<IRepository<DiscountRule>> _mockRuleRepository;
    private readonly Mock<IRepository<DiscountHistory>> _mockHistoryRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<DiscountRuleService>> _mockLogger;
    private readonly DiscountRuleService _service;

    public DiscountRuleServiceTests()
    {
        _mockRuleRepository = new Mock<IRepository<DiscountRule>>();
        _mockHistoryRepository = new Mock<IRepository<DiscountHistory>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DiscountRuleService>>();
        _service = new DiscountRuleService(_mockRuleRepository.Object, _mockHistoryRepository.Object, _mockDbContext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedRule()
    {
        // Arrange
        var dto = new CreateDiscountRuleDto
        {
            Name = "Gold Tier Discount",
            Type = DiscountRuleType.Percentage,
            Value = 15m,
            CustomerTier = "Gold"
        };

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<DiscountRule>(), It.IsAny<CancellationToken>()));

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal("Gold", result.CustomerTier);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithNoApplicableRules_ReturnsZeroDiscount()
    {
        // Arrange
        _mockRuleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DiscountRule>());

        // Act
        var result = await _service.CalculateDiscountAsync(1, null, 1000m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(1000m, result.FinalAmount);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithPercentageDiscount_CalculatesCorrectly()
    {
        // Arrange
        var rule = new DiscountRule
        {
            Id = 1,
            Name = "Test Discount",
            Type = DiscountRuleType.Percentage,
            Value = 10m,
            IsActive = true,
            EffectiveDate = DateTime.UtcNow.AddDays(-1),
            IsCumulative = true
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<DiscountRule> { rule });

        // Act
        var result = await _service.CalculateDiscountAsync(1, null, 1000m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100m, result.DiscountAmount); // 10% of 1000
        Assert.Equal(900m, result.FinalAmount);
    }
}
