// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * AP-024: Input Validation for CreateSubscriptionDto
 * ====================================================
 * Tests verify DataAnnotation validations on CreateSubscriptionDto.
 * Validations covered:
 * - AccountId: [Required], [Range(1, int.MaxValue)]
 * - Amount: [Range(0.01, 999999999.99)]
 * - BillingCycle: [Required], [StringLength(50)]
 * - BillingStartDate: [DataType(DataType.Date)] (metadata hint; no runtime range check)
 * - ProrationType: [StringLength(50)]
 * - Notes: [StringLength(1000)]
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.Dtos;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Unit tests for <see cref="CreateSubscriptionDto"/> DataAnnotation validations (AP-024).
    /// </summary>
    public class SubscriptionDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        private static CreateSubscriptionDto CreateValidDto() => new CreateSubscriptionDto
        {
            AccountId = 1,
            Amount = 99.99m,
            BillingCycle = "Monthly",
            BillingStartDate = DateTime.UtcNow.Date,
        };

        #region AccountId — AP-024

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(1, true)]
        [InlineData(1000, true)]
        [InlineData(int.MaxValue, true)]
        public void AccountId_WithVariousValues_ValidatesRange(int accountId, bool shouldBeValid)
        {
            var dto = CreateValidDto();
            dto.AccountId = accountId;
            var results = ValidateModel(dto);
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.Contains(results, r =>
                    r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.AccountId)));
            }
        }

        #endregion

        #region Amount — AP-024

        [Theory]
        [InlineData("0.00", false)]
        [InlineData("0.01", true)]
        [InlineData("100.00", true)]
        [InlineData("999999999.99", true)]
        [InlineData("1000000000.00", false)]
        public void Amount_WithVariousValues_ValidatesRange(string amountStr, bool shouldBeValid)
        {
            var dto = CreateValidDto();
            dto.Amount = decimal.Parse(amountStr, System.Globalization.CultureInfo.InvariantCulture);
            var results = ValidateModel(dto);
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.Contains(results, r =>
                    r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.Amount)));
            }
        }

        #endregion

        #region BillingCycle — AP-024

        [Fact]
        public void BillingCycle_Null_FailsRequired()
        {
            var dto = CreateValidDto();
            dto.BillingCycle = null!;
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.BillingCycle)));
        }

        [Fact]
        public void BillingCycle_Empty_FailsRequired()
        {
            var dto = CreateValidDto();
            dto.BillingCycle = string.Empty;
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.BillingCycle)));
        }

        [Fact]
        public void BillingCycle_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.BillingCycle = new string('X', 51);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.BillingCycle)));
        }

        [Theory]
        [InlineData("Monthly")]
        [InlineData("Annual")]
        [InlineData("Quarterly")]
        [InlineData("Weekly")]
        public void BillingCycle_ValidValues_Pass(string cycle)
        {
            var dto = CreateValidDto();
            dto.BillingCycle = cycle;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        #endregion

        #region Optional fields — length limits

        [Fact]
        public void Notes_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.Notes = new string('N', 1001);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.Notes)));
        }

        [Fact]
        public void ProrationType_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.ProrationType = new string('P', 51);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.ProrationType)));
        }

        #endregion

        #region Full valid DTO — AP-024

        [Fact]
        public void CreateSubscriptionDto_WithAllValidData_IsValid()
        {
            var dto = new CreateSubscriptionDto
            {
                AccountId = 42,
                ProductId = 5,
                Amount = 299.99m,
                BillingCycle = "Annual",
                BillingStartDate = DateTime.UtcNow.Date,
                TrialEndDate = DateTime.UtcNow.Date.AddDays(14),
                IsAutoRenewal = true,
                ProrationType = "ProRata",
                Notes = "Integration test subscription",
            };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        #endregion
    }
}
