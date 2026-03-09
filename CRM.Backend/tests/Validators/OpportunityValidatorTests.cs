// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CRM.Tests.Validators.OpportunityValidation;

/// <summary>
/// Unit tests for Opportunity Validator
/// Covers: Amount validation, stage transitions, close date validation
/// </summary>
public class OpportunityValidatorTests
{
    private readonly Mock<IOpportunityService> _mockOpportunityService;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly OpportunityValidator _validator;

    public OpportunityValidatorTests()
    {
        _mockOpportunityService = new Mock<IOpportunityService>();
        _mockAccountService = new Mock<IAccountService>();
        _mockUserService = new Mock<IUserService>();

        // Default mock: account ID 1 exists (used by CreateValidOpportunityDto)
        _mockAccountService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new AccountResultDto { Id = 1 });

        _validator = new OpportunityValidator(
            _mockOpportunityService.Object,
            _mockAccountService.Object,
            _mockUserService.Object);
    }

    #region Name Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidOpportunity_ReturnsNoErrors()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_NullName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Name = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task ValidateAsync_EmptyName_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Name = "";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Name"].Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public async Task ValidateAsync_NameTooLong_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Name = new string('A', 256);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Name"].Should().Contain(e => e.Contains("255"));
    }

    #endregion

    #region Amount Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidAmount_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Amount = 100000;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NegativeAmount_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Amount = -1000;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Amount");
    }

    [Fact]
    public async Task ValidateAsync_ZeroAmount_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Amount = 0;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ExcessiveAmount_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Amount = decimal.MaxValue;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Amount");
    }

    #endregion

    #region Probability Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task ValidateAsync_ValidProbability_ReturnsNoError(int probability)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Probability = probability;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task ValidateAsync_NegativeProbability_ReturnsError(int probability)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Probability = probability;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Probability");
    }

    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    public async Task ValidateAsync_ProbabilityOver100_ReturnsError(int probability)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Probability = probability;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Probability"].Should().Contain(e => e.Contains("100"));
    }

    #endregion

    #region Stage Validation Tests

    [Theory]
    [InlineData("Qualification")]
    [InlineData("Proposal")]
    [InlineData("Negotiation")]
    [InlineData("Closed Won")]
    [InlineData("Closed Lost")]
    public async Task ValidateAsync_ValidStage_ReturnsNoError(string stage)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = stage;

        // Provide required fields for closed stages
        if (stage.StartsWith("Closed"))
        {
            dto.CloseDate = DateTime.UtcNow;
        }
        if (stage == "Closed Lost")
        {
            dto.LossReason = "Went with competitor";
        }

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidStage_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = "InvalidStage";

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Stage");
    }

    [Fact]
    public async Task ValidateAsync_NullStage_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Stage");
    }

    #endregion

    #region Stage Transition Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidStageTransition_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Id = 1;
        dto.Stage = "Proposal";
        _mockOpportunityService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new OpportunityDto { Id = 1, Stage = "Qualification" });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_InvalidStageTransition_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Id = 1;
        dto.Stage = "Qualification"; // Going backwards from Closed Won
        _mockOpportunityService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new OpportunityDto { Id = 1, Stage = "Closed Won" });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["Stage"].Should().Contain(e => e.Contains("Cannot transition"));
    }

    [Fact]
    public async Task ValidateAsync_ClosedWonWithoutCloseDate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = "Closed Won";
        dto.CloseDate = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("CloseDate");
    }

    [Fact]
    public async Task ValidateAsync_ClosedLostWithoutCloseDate_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = "Closed Lost";
        dto.CloseDate = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("CloseDate");
    }

    [Fact]
    public async Task ValidateAsync_ClosedLostWithoutLossReason_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Stage = "Closed Lost";
        dto.CloseDate = DateTime.UtcNow;
        dto.LossReason = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("LossReason");
    }

    #endregion

    #region Close Date Validation Tests

    [Fact]
    public async Task ValidateAsync_FutureCloseDate_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.ExpectedCloseDate = DateTime.UtcNow.AddDays(30);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_PastCloseDate_ReturnsWarning()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.ExpectedCloseDate = DateTime.UtcNow.AddDays(-30);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert - Warning, not error
        result.IsValid.Should().BeTrue();
        result.Warnings.Should().ContainKey("ExpectedCloseDate");
    }

    [Fact]
    public async Task ValidateAsync_CloseDateTooFarInFuture_ReturnsWarning()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.ExpectedCloseDate = DateTime.UtcNow.AddYears(3);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.Warnings.Should().ContainKey("ExpectedCloseDate");
    }

    #endregion

    #region Account Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidAccountId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.AccountId = 1;
        _mockAccountService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new AccountResultDto { Id = 1 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentAccount_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.AccountId = 999;
        _mockAccountService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((AccountResultDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("AccountId");
    }

    [Fact]
    public async Task ValidateAsync_NullAccountId_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.AccountId = null;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors["AccountId"].Should().Contain(e => e.Contains("required"));
    }

    #endregion

    #region Owner Validation Tests

    [Fact]
    public async Task ValidateAsync_ValidOwnerId_ReturnsNoError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.OwnerId = 1;
        _mockUserService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new UserDto { Id = 1 });

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_NonExistentOwner_ReturnsError()
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.OwnerId = 999;
        _mockUserService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((UserDto?)null);

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("OwnerId");
    }

    #endregion

    #region Lead Source Validation Tests

    [Theory]
    [InlineData("Web")]
    [InlineData("Referral")]
    [InlineData("Trade Show")]
    [InlineData("Cold Call")]
    public async Task ValidateAsync_ValidLeadSource_ReturnsNoError(string source)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.LeadSource = source;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Type Validation Tests

    [Theory]
    [InlineData("New Business")]
    [InlineData("Existing Business")]
    [InlineData("Renewal")]
    [InlineData("Upsell")]
    public async Task ValidateAsync_ValidType_ReturnsNoError(string type)
    {
        // Arrange
        var dto = CreateValidOpportunityDto();
        dto.Type = type;

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public async Task ValidateAsync_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange
        var dto = new CreateOpportunityDto
        {
            Name = "",
            Stage = "InvalidStage",
            Amount = -1000,
            Probability = 150,
            AccountId = null
        };

        // Act
        var result = await _validator.ValidateAsync(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterOrEqualTo(4);
    }

    #endregion

    #region Helper Methods

    private CreateOpportunityDto CreateValidOpportunityDto()
    {
        return new CreateOpportunityDto
        {
            Name = "Enterprise Software Deal",
            Stage = "Qualification",
            Amount = 50000,
            Probability = 30,
            AccountId = 1,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(60)
        };
    }

    #endregion
}

// Supporting classes
public class OpportunityValidator
{
    private readonly IOpportunityService _opportunityService;
    private readonly IAccountService _accountService;
    private readonly IUserService _userService;

    private static readonly HashSet<string> ValidStages = new(StringComparer.OrdinalIgnoreCase)
    {
        "Qualification", "Discovery", "Proposal", "Negotiation", "Closed Won", "Closed Lost"
    };

    private static readonly HashSet<string> ClosedStages = new(StringComparer.OrdinalIgnoreCase)
    {
        "Closed Won", "Closed Lost"
    };

    public OpportunityValidator(
        IOpportunityService opportunityService,
        IAccountService accountService,
        IUserService userService)
    {
        _opportunityService = opportunityService;
        _accountService = accountService;
        _userService = userService;
    }

    public async Task<OpportunityValidationResult> ValidateAsync(CreateOpportunityDto dto)
    {
        var result = new OpportunityValidationResult();

        // Name validation
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            result.AddError("Name", "Opportunity name is required");
        }
        else if (dto.Name.Length > 255)
        {
            result.AddError("Name", "Opportunity name cannot exceed 255 characters");
        }

        // Stage validation
        if (string.IsNullOrWhiteSpace(dto.Stage))
        {
            result.AddError("Stage", "Stage is required");
        }
        else if (!ValidStages.Contains(dto.Stage))
        {
            result.AddError("Stage", "Invalid stage value");
        }
        else if (dto.Id.HasValue)
        {
            await ValidateStageTransition(dto, result);
        }

        // Amount validation
        if (dto.Amount.HasValue)
        {
            if (dto.Amount < 0)
            {
                result.AddError("Amount", "Amount cannot be negative");
            }
            else if (dto.Amount > 999999999999)
            {
                result.AddError("Amount", "Amount is too large");
            }
        }

        // Probability validation
        if (dto.Probability.HasValue)
        {
            if (dto.Probability < 0)
            {
                result.AddError("Probability", "Probability cannot be negative");
            }
            else if (dto.Probability > 100)
            {
                result.AddError("Probability", "Probability cannot exceed 100%");
            }
        }

        // Account validation
        if (!dto.AccountId.HasValue)
        {
            result.AddError("AccountId", "Account is required");
        }
        else
        {
            var account = await _accountService.GetByIdAsync(dto.AccountId.Value);
            if (account == null)
            {
                result.AddError("AccountId", "Account does not exist");
            }
        }

        // Owner validation
        if (dto.OwnerId.HasValue)
        {
            var owner = await _userService.GetByIdAsync(dto.OwnerId.Value);
            if (owner == null)
            {
                result.AddError("OwnerId", "Owner does not exist");
            }
        }

        // Close date validation for closed stages
        if (ClosedStages.Contains(dto.Stage ?? ""))
        {
            if (!dto.CloseDate.HasValue)
            {
                result.AddError("CloseDate", "Close date is required for closed opportunities");
            }

            if (dto.Stage?.Equals("Closed Lost", StringComparison.OrdinalIgnoreCase) == true &&
                string.IsNullOrWhiteSpace(dto.LossReason))
            {
                result.AddError("LossReason", "Loss reason is required for lost opportunities");
            }
        }

        // Expected close date warnings
        if (dto.ExpectedCloseDate.HasValue)
        {
            if (dto.ExpectedCloseDate < DateTime.UtcNow)
            {
                result.AddWarning("ExpectedCloseDate", "Expected close date is in the past");
            }
            else if (dto.ExpectedCloseDate > DateTime.UtcNow.AddYears(2))
            {
                result.AddWarning("ExpectedCloseDate", "Expected close date is more than 2 years away");
            }
        }

        return result;
    }

    private async Task ValidateStageTransition(CreateOpportunityDto dto, OpportunityValidationResult result)
    {
        var existing = await _opportunityService.GetByIdAsync(dto.Id!.Value);
        if (existing != null && ClosedStages.Contains(existing.Stage ?? ""))
        {
            if (!ClosedStages.Contains(dto.Stage ?? ""))
            {
                result.AddError("Stage", "Cannot transition from a closed stage back to an open stage");
            }
        }
    }
}

public class OpportunityValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public Dictionary<string, List<string>> Errors { get; } = new();
    public Dictionary<string, List<string>> Warnings { get; } = new();

    public void AddError(string field, string message)
    {
        if (!Errors.ContainsKey(field))
            Errors[field] = new List<string>();
        Errors[field].Add(message);
    }

    public void AddWarning(string field, string message)
    {
        if (!Warnings.ContainsKey(field))
            Warnings[field] = new List<string>();
        Warnings[field].Add(message);
    }
}

public class CreateOpportunityDto
{
    public int? Id { get; set; }
    public string? Name { get; set; }
    public string? Stage { get; set; }
    public decimal? Amount { get; set; }
    public int? Probability { get; set; }
    public int? AccountId { get; set; }
    public int? OwnerId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public string? LossReason { get; set; }
    public string? LeadSource { get; set; }
    public string? Type { get; set; }
}

public class OpportunityDto
{
    public int Id { get; set; }
    public string? Stage { get; set; }
}

public class AccountResultDto
{
    public int Id { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
}

public interface IOpportunityService
{
    Task<OpportunityDto?> GetByIdAsync(int id);
}

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(int id);
}

public interface IAccountService
{
    Task<AccountResultDto?> GetByIdAsync(int id);
}
