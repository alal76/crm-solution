// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Core.DTOs;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Validation tests for OpportunityDtos: CreateOpportunityDto, OpportunityDto,
    /// CreateOpportunityProductDto, CreateTeamMemberDto, and ForecastCategoryPatchDto.
    ///
    /// All validations are exercised via Validator.TryValidateObject to match
    /// ASP.NET Core model binding validation behaviour.
    /// </summary>
    public class OpportunityDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static CreateOpportunityDto ValidCreateOpportunity() => new()
        {
            Name = "Enterprise Deal Q1",
            Currency = "USD",
            Probability = 50,
            Amount = 10_000m,
            TermLengthMonths = 12,
        };

        private static CreateOpportunityProductDto ValidCreateProduct() => new()
        {
            ProductId = 1,
            Quantity = 5,
            DiscountPercent = 10m,
        };

        private static CreateTeamMemberDto ValidCreateTeamMember() => new()
        {
            UserId = 1,
            SplitPercentage = 50m,
        };

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – Name (Required, MaxLength 255)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateOpportunityDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidCreateOpportunity());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Name_ShouldFail_WhenValueIsNullOrEmpty(string? name)
        {
            var dto = ValidCreateOpportunity();
            dto.Name = name!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Name_ShouldFail_WhenValueExceedsMaxLength255()
        {
            var dto = ValidCreateOpportunity();
            dto.Name = new string('X', 256); // 256 chars – one over MaxLength(255)
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Name_ShouldPass_WhenValueIsExactlyMaxLength255()
        {
            var dto = ValidCreateOpportunity();
            dto.Name = new string('X', 255);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – Currency (Required, MaxLength 3)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Currency_ShouldFail_WhenValueIsNullOrEmpty(string? currency)
        {
            var dto = ValidCreateOpportunity();
            dto.Currency = currency!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Currency"));
        }

        [Fact]
        public void Currency_ShouldFail_WhenValueExceedsMaxLength3()
        {
            var dto = ValidCreateOpportunity();
            dto.Currency = "USDX"; // 4 chars – over MaxLength(3)
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Currency"));
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("GBP")]
        public void Currency_ShouldPass_WhenValueIsThreeCharIso(string currency)
        {
            var dto = ValidCreateOpportunity();
            dto.Currency = currency;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – Probability (Range 0–100)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Probability_ShouldFail_WhenValueIsOutsideRange0To100(int probability)
        {
            var dto = ValidCreateOpportunity();
            dto.Probability = probability;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Probability"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void Probability_ShouldPass_WhenValueIsInRange0To100(int probability)
        {
            var dto = ValidCreateOpportunity();
            dto.Probability = probability;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – Amount (Range 0–double.MaxValue)
        // -----------------------------------------------------------------------

        [Fact]
        public void Amount_ShouldFail_WhenValueIsNegative()
        {
            var dto = ValidCreateOpportunity();
            dto.Amount = -0.01m;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Amount"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(9999999)]
        public void Amount_ShouldPass_WhenValueIsZeroOrPositive(decimal amount)
        {
            var dto = ValidCreateOpportunity();
            dto.Amount = amount;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – TermLengthMonths (Range 1–120)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(121)]
        public void TermLengthMonths_ShouldFail_WhenValueIsOutsideRange1To120(int months)
        {
            var dto = ValidCreateOpportunity();
            dto.TermLengthMonths = months;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("TermLengthMonths"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(12)]
        [InlineData(120)]
        public void TermLengthMonths_ShouldPass_WhenValueIsInRange1To120(int months)
        {
            var dto = ValidCreateOpportunity();
            dto.TermLengthMonths = months;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – SolutionNotes / QualificationNotes (MaxLength 4000)
        // -----------------------------------------------------------------------

        [Fact]
        public void SolutionNotes_ShouldFail_WhenValueExceedsMaxLength4000()
        {
            var dto = ValidCreateOpportunity();
            dto.SolutionNotes = new string('N', 4001);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("SolutionNotes"));
        }

        [Fact]
        public void QualificationNotes_ShouldFail_WhenValueExceedsMaxLength4000()
        {
            var dto = ValidCreateOpportunity();
            dto.QualificationNotes = new string('Q', 4001);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("QualificationNotes"));
        }

        [Fact]
        public void SolutionNotes_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidCreateOpportunity();
            dto.SolutionNotes = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityDto – Region (MaxLength 100)
        // -----------------------------------------------------------------------

        [Fact]
        public void Region_ShouldFail_WhenValueExceedsMaxLength100()
        {
            var dto = ValidCreateOpportunity();
            dto.Region = new string('R', 101);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Region"));
        }

        [Fact]
        public void Region_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidCreateOpportunity();
            dto.Region = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityProductDto – Quantity (Range 1–int.MaxValue)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateOpportunityProductDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidCreateProduct());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Quantity_ShouldFail_WhenValueIsZeroOrNegative(int quantity)
        {
            var dto = ValidCreateProduct();
            dto.Quantity = quantity;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Quantity"));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Quantity_ShouldPass_WhenValueIsAtLeastOne(int quantity)
        {
            var dto = ValidCreateProduct();
            dto.Quantity = quantity;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateOpportunityProductDto – DiscountPercent (Range 0–100, nullable)
        // -----------------------------------------------------------------------

        [Fact]
        public void DiscountPercent_ShouldFail_WhenValueIsAbove100()
        {
            var dto = ValidCreateProduct();
            dto.DiscountPercent = 100.01m;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("DiscountPercent"));
        }

        [Fact]
        public void DiscountPercent_ShouldFail_WhenValueIsNegative()
        {
            var dto = ValidCreateProduct();
            dto.DiscountPercent = -1m;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("DiscountPercent"));
        }

        [Fact]
        public void DiscountPercent_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidCreateProduct();
            dto.DiscountPercent = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        public void DiscountPercent_ShouldPass_WhenValueIsInRange0To100(decimal discount)
        {
            var dto = ValidCreateProduct();
            dto.DiscountPercent = discount;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateTeamMemberDto – SplitPercentage (Range 0–100)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateTeamMemberDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidCreateTeamMember());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(100.01)]
        public void SplitPercentage_ShouldFail_WhenValueIsOutsideRange0To100(decimal split)
        {
            var dto = ValidCreateTeamMember();
            dto.SplitPercentage = split;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("SplitPercentage"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(33.33)]
        [InlineData(100)]
        public void SplitPercentage_ShouldPass_WhenValueIsInRange0To100(decimal split)
        {
            var dto = ValidCreateTeamMember();
            dto.SplitPercentage = split;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // ForecastCategoryPatchDto – ForecastCategory (Required on int)
        // Note: [Required] on a non-nullable int is a no-op for TryValidateObject
        // (int can never be null). This test documents that valid values pass.
        // -----------------------------------------------------------------------

        [Fact]
        public void ForecastCategoryPatchDto_ValidObject_PassesValidation()
        {
            var dto = new ForecastCategoryPatchDto { ForecastCategory = 1 };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void ForecastCategoryPatchDto_DefaultValue_PassesValidation()
        {
            // int defaults to 0; [Required] on non-nullable int does not trigger
            var dto = new ForecastCategoryPatchDto();
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }
    }
}
