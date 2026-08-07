// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class CommissionRuleServiceTests : ServiceTestFixtureBase<CommissionRuleService>
{
    private readonly Mock<IRepository<CommissionRule>> _mockRuleRepository;
    private readonly Mock<IRepository<CommissionHistory>> _mockHistoryRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;    private readonly CommissionRuleService _service;

    public CommissionRuleServiceTests()
    {
        _mockRuleRepository = new Mock<IRepository<CommissionRule>>();
        _mockHistoryRepository = new Mock<IRepository<CommissionHistory>>();
        _mockDbContext = new Mock<ICrmDbContext>();        _service = new CommissionRuleService(_mockRuleRepository.Object, _mockHistoryRepository.Object, _mockDbContext.Object, MockLogger.Object);
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

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<CommissionRule>()));

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
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);

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
            EffectiveDate = DateTime.UtcNow.AddDays(-10), // Always in the past
            ExpiryDate = null
        };

        var rules = new List<CommissionRule> { rule };
        _mockRuleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rules);
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);

        // Act
        var result = await _service.CalculateCommissionAsync(1000m, "DirectSale");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000m, result.DealAmount);
        Assert.Equal(100m, result.Commission); // 10% of 1000
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_SetsIsDeleted()
    {
        // Arrange
        var rule = new CommissionRule { Id = 1, Name = "Test", IsDeleted = false };
        _mockRuleRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rule);
        _mockRuleRepository.Setup(r => r.UpdateAsync(It.IsAny<CommissionRule>()));

        // Act
        await _service.DeleteAsync(1);

        // Assert
        Assert.True(rule.IsDeleted);
        _mockRuleRepository.Verify(r => r.UpdateAsync(rule), Times.Once);
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

        _mockRuleRepository.Setup(r => r.AddAsync(It.IsAny<DiscountRule>()));

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
        _mockRuleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DiscountRule>());

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
            EffectiveDate = DateTime.UtcNow.AddDays(-10), // Always in the past
            IsCumulative = true
        };

        _mockRuleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<DiscountRule> { rule });

        // Act
        var result = await _service.CalculateDiscountAsync(1, null, 1000m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100m, result.DiscountAmount); // 10% of 1000
        Assert.Equal(900m, result.FinalAmount);
    }
}
