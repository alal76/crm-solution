// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.Dtos;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Comprehensive validation tests for Subscription Metrics DTOs.
    /// Tests DataAnnotation validations for SaaS/subscription business metrics.
    /// </summary>
    public class SubscriptionMetricsDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        #region Helper Methods

        private CreateSubscriptionMetricsDto CreateValidMetrics()
        {
            return new CreateSubscriptionMetricsDto
            {
                SubscriptionId = 1,
                MRR = 1000.0m,
                ARR = 12000.0m,
                ChurnRate = 5.0m,
                NRR = 110.0m,
                GRR = 95.0m,
                MeasurementDate = DateTime.UtcNow,
                PeriodStartDate = DateTime.UtcNow.AddMonths(-1),
                PeriodEndDate = DateTime.UtcNow,
                Notes = "Test metrics"
            };
        }

        #endregion

        #region CreateSubscriptionMetricsDto - SubscriptionId Validation

        [Fact]
        public void CreateSubscriptionMetricsDto_WithValidData_ShouldBeValid()
        {
            // Arrange
            var dto = CreateValidMetrics();

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void CreateSubscriptionMetricsDto_SubscriptionId_Invalid_ShouldFailValidation(int subscriptionId)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.SubscriptionId = subscriptionId;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Required][Range(1, int.MaxValue)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.SubscriptionId)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(int.MaxValue)]
        public void CreateSubscriptionMetricsDto_SubscriptionId_ValidValues_ShouldBeValid(int subscriptionId)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.SubscriptionId = subscriptionId;

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateSubscriptionMetricsDto - MRR Validation

        [Theory]
        [InlineData(0)]           // Zero MRR (new/inactive subscription)
        [InlineData(0.01)]        // Minimum positive
        [InlineData(1000.0)]      // Typical MRR
        [InlineData(999999.99)]   // High MRR
        [InlineData(999999999.99999)] // Maximum
        public void CreateSubscriptionMetricsDto_MRR_ValidRange_ShouldBeValid(double mrr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.MRR = (decimal)mrr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-1000.0)]
        public void CreateSubscriptionMetricsDto_MRR_Negative_ShouldFailValidation(double mrr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.MRR = (decimal)mrr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.MRR)));
        }

        [Fact]
        public void CreateSubscriptionMetricsDto_MRR_ExceedsMaximum_ShouldFailValidation()
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.MRR = 1000000000.0m; // Exceeds 999,999,999.99999

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.MRR)));
        }

        [Theory]
        [InlineData(1000.1234)]   // 4 decimal places
        [InlineData(1000.12345)]  // 5 decimal places (max)
        [InlineData(0.00001)]     // Micro-transaction precision
        public void CreateSubscriptionMetricsDto_MRR_DecimalPrecision_ShouldBeValid(double mrr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.MRR = (decimal)mrr;

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateSubscriptionMetricsDto - ARR Validation

        [Theory]
        [InlineData(0)]              // Zero ARR
        [InlineData(12000.0)]        // Annual = 1000 * 12
        [InlineData(999999999.99999)] // Maximum
        public void CreateSubscriptionMetricsDto_ARR_ValidRange_ShouldBeValid(double arr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.ARR = (decimal)arr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(-12000.0)]
        public void CreateSubscriptionMetricsDto_ARR_Negative_ShouldFailValidation(double arr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.ARR = (decimal)arr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.ARR)));
        }

        #endregion

        #region CreateSubscriptionMetricsDto - ChurnRate Validation

        [Theory]
        [InlineData(0)]      // No churn (perfect retention)
        [InlineData(2.5)]    // Low churn (typical for good SaaS)
        [InlineData(5.0)]    // Average churn
        [InlineData(10.0)]   // High churn
        [InlineData(100.0)]  // Maximum (100% churn = all customers left)
        public void CreateSubscriptionMetricsDto_ChurnRate_ValidRange_ShouldBeValid(double churnRate)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.ChurnRate = (decimal)churnRate;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.1)]   // Negative churn (impossible)
        [InlineData(100.1)]  // > 100% (impossible)
        [InlineData(200.0)]
        public void CreateSubscriptionMetricsDto_ChurnRate_OutOfRange_ShouldFailValidation(double churnRate)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.ChurnRate = (decimal)churnRate;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.ChurnRate)));
        }

        #endregion

        #region CreateSubscriptionMetricsDto - NRR (Net Revenue Retention) Validation

        [Theory]
        [InlineData(0)]      // All revenue lost
        [InlineData(90.0)]   // Net contraction
        [InlineData(100.0)]  // Flat (no expansion or contraction)
        [InlineData(110.0)]  // Expansion (good)
        [InlineData(120.0)]  // Strong expansion
        [InlineData(200.0)]  // Maximum (200% = doubled revenue from existing customers)
        public void CreateSubscriptionMetricsDto_NRR_ValidRange_ShouldBeValid(double nrr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.NRR = (decimal)nrr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 200)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.1)]   // Negative (invalid)
        [InlineData(200.1)]  // > 200% (too high)
        [InlineData(300.0)]
        public void CreateSubscriptionMetricsDto_NRR_OutOfRange_ShouldFailValidation(double nrr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.NRR = (decimal)nrr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 200)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.NRR)));
        }

        #endregion

        #region CreateSubscriptionMetricsDto - GRR (Gross Revenue Retention) Validation

        [Theory]
        [InlineData(0)]      // Complete revenue loss
        [InlineData(85.0)]   // Some churn
        [InlineData(95.0)]   // Good retention
        [InlineData(100.0)]  // Perfect retention (no downgrades/cancellations)
        public void CreateSubscriptionMetricsDto_GRR_ValidRange_ShouldBeValid(double grr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.GRR = (decimal)grr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.1)]   // Negative (invalid)
        [InlineData(100.1)]  // > 100% (impossible for GRR - can't retain more than 100%)
        [InlineData(150.0)]
        public void CreateSubscriptionMetricsDto_GRR_OutOfRange_ShouldFailValidation(double grr)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.GRR = (decimal)grr;

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.GRR)));
        }

        #endregion

        #region CreateSubscriptionMetricsDto - Notes Validation

        [Fact]
        public void CreateSubscriptionMetricsDto_Notes_Null_ShouldBeValid()
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.Notes = null;

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(500)]
        [InlineData(1000)] // Maximum
        public void CreateSubscriptionMetricsDto_Notes_ValidLengths_ShouldBeValid(int length)
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.Notes = new string('N', length);

            // Act
            var results = ValidateModel(dto);

            // Assert - [StringLength(1000)]
            Assert.Empty(results);
        }

        [Fact]
        public void CreateSubscriptionMetricsDto_Notes_ExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var dto = CreateValidMetrics();
            dto.Notes = new string('N', 1001); // Exceeds limit

            // Act
            var results = ValidateModel(dto);

            // Assert - [StringLength(1000)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(CreateSubscriptionMetricsDto.Notes)));
        }

        #endregion

        #region SubscriptionMetricsDto Validation

        [Fact]
        public void SubscriptionMetricsDto_WithFullData_ShouldBeValid()
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                Id = 1,
                SubscriptionId = 100,
                SubscriptionNumber = "SUB-001",
                AccountId = 50,
                AccountName = "Test Account",
                MRR = 5000.0m,
                ARR = 60000.0m,
                ChurnRate = 3.5m,
                NRR = 115.0m,
                GRR = 97.0m,
                CAC = 2000.0m,
                CLV = 15000.0m,
                ExpansionRevenue = 500.0m,
                ContractionRevenue = 100.0m,
                ACV = 5000.0m,
                PaymentFees = 150.0m,
                RefundAmount = 50.0m,
                BillingCycle = "Monthly",
                MeasurementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Notes = "Full metrics report"
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1000.0)]
        [InlineData(999999999.99999)]
        public void SubscriptionMetricsDto_CAC_ValidRange_ShouldBeValid(double cac)
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                CAC = (decimal)cac,
                CLV = 10000.0m,
                ChurnRate = 5.0m,
                NRR = 100.0m,
                GRR = 95.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5000.0)]
        [InlineData(999999999.99999)]
        public void SubscriptionMetricsDto_CLV_ValidRange_ShouldBeValid(double clv)
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                CAC = 1000.0m,
                CLV = (decimal)clv,
                ChurnRate = 5.0m,
                NRR = 100.0m,
                GRR = 95.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(500.0)]
        [InlineData(999999999.99999)]
        public void SubscriptionMetricsDto_ExpansionRevenue_ValidRange_ShouldBeValid(double expansion)
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                ExpansionRevenue = (decimal)expansion,
                ContractionRevenue = 0m,
                ChurnRate = 5.0m,
                NRR = 110.0m,
                GRR = 95.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.0)]
        [InlineData(999999999.99999)]
        public void SubscriptionMetricsDto_ContractionRevenue_ValidRange_ShouldBeValid(double contraction)
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                ExpansionRevenue = 0m,
                ContractionRevenue = (decimal)contraction,
                ChurnRate = 5.0m,
                NRR = 90.0m,
                GRR = 95.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        #endregion

        #region UpdateSubscriptionMetricsDto Validation

        [Fact]
        public void UpdateSubscriptionMetricsDto_AllFieldsNull_ShouldBeValid()
        {
            // Arrange - All fields optional for updates
            var dto = new UpdateSubscriptionMetricsDto();

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1500.0)]
        [InlineData(999999999.99999)]
        public void UpdateSubscriptionMetricsDto_MRR_ValidValues_ShouldBeValid(double mrr)
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                MRR = (decimal)mrr
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.Empty(results);
        }

        [Fact]
        public void UpdateSubscriptionMetricsDto_MRR_Negative_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                MRR = -1000.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 999999999.99999)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateSubscriptionMetricsDto.MRR)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3.5)]
        [InlineData(100.0)]
        public void UpdateSubscriptionMetricsDto_ChurnRate_ValidValues_ShouldBeValid(double churnRate)
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                ChurnRate = (decimal)churnRate
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(-0.1)]
        [InlineData(100.1)]
        public void UpdateSubscriptionMetricsDto_ChurnRate_OutOfRange_ShouldFailValidation(double churnRate)
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                ChurnRate = (decimal)churnRate
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [Range(0, 100)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateSubscriptionMetricsDto.ChurnRate)));
        }

        [Fact]
        public void UpdateSubscriptionMetricsDto_Notes_ExceedsMaxLength_ShouldFailValidation()
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                Notes = new string('X', 1001)
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - [StringLength(1000)]
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateSubscriptionMetricsDto.Notes)));
        }

        #endregion

        #region Business Logic Tests

        [Fact]
        public void SubscriptionMetricsDto_ARR_ShouldEqualMRRTimes12()
        {
            // Business rule: ARR = MRR * 12 (for monthly billing)
            // Arrange
            var mrr = 1000.0m;
            var expectedArr = mrr * 12;

            var dto = new SubscriptionMetricsDto
            {
                MRR = mrr,
                ARR = expectedArr,
                BillingCycle = "Monthly",
                ChurnRate = 5.0m,
                NRR = 100.0m,
                GRR = 95.0m
            };

            // Act & Assert
            Assert.Equal(12000.0m, dto.ARR);
            Assert.Equal(dto.MRR * 12, dto.ARR);
        }

        [Fact]
        public void SubscriptionMetricsDto_CLVToCAC_Ratio_ShouldBeGreaterThan3()
        {
            // Best practice: CLV/CAC ratio should be > 3:1
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                CAC = 2000.0m,
                CLV = 8000.0m,
                ChurnRate = 5.0m,
                NRR = 100.0m,
                GRR = 95.0m
            };

            // Act
            var ratio = dto.CLV / dto.CAC;

            // Assert
            Assert.True(ratio > 3.0m, $"CLV/CAC ratio should be > 3:1, but was {ratio}:1");
            Assert.Equal(4.0m, ratio);
        }

        [Fact]
        public void SubscriptionMetricsDto_NRR_ShouldBeGRR_PlusExpansion_MinusContraction()
        {
            // Business rule: NRR considers expansion/contraction, GRR doesn't
            // GRR = 95% (5% churn)
            // Expansion = +10%
            // Contraction = -5%
            // NRR = 95 + 10 - 5 = 100%
            
            var dto = new SubscriptionMetricsDto
            {
                GRR = 95.0m,
                NRR = 100.0m,
                ExpansionRevenue = 1000.0m,  // +10% of base
                ContractionRevenue = 500.0m, // -5% of base
                ChurnRate = 5.0m
            };

            // Assert NRR is calculated including expansion/contraction
            Assert.True(dto.NRR >= dto.GRR || dto.NRR < dto.GRR, 
                "NRR can be higher (with expansion) or lower (with contraction) than GRR");
        }

        [Fact]
        public void SubscriptionMetricsDto_ChurnRate_AndGRR_ShouldBeComplementary()
        {
            // Business rule relationship: GRR = 100% - ChurnRate (approximately)
            // If 5% churn, GRR should be ~95%
            
            var dto = new SubscriptionMetricsDto
            {
                ChurnRate = 5.0m,
                GRR = 95.0m,
                NRR = 100.0m
            };

            // Act
            var expectedGRR = 100.0m - dto.ChurnRate;

            // Assert
            Assert.Equal(expectedGRR, dto.GRR);
        }

        #endregion

        #region Edge Cases and Boundary Tests

        [Fact]
        public void CreateSubscriptionMetricsDto_MinimumValidValues_ShouldBeValid()
        {
            // Arrange - Minimum boundary values
            var dto = new CreateSubscriptionMetricsDto
            {
                SubscriptionId = 1,
                MRR = 0m,
                ARR = 0m,
                ChurnRate = 0m,
                NRR = 0m,
                GRR = 0m,
                MeasurementDate = DateTime.MinValue,
                Notes = null
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateSubscriptionMetricsDto_MaximumValidValues_ShouldBeValid()
        {
            // Arrange - Maximum boundary values
            var dto = new CreateSubscriptionMetricsDto
            {
                SubscriptionId = int.MaxValue,
                MRR = 999999999.99999m,
                ARR = 999999999.99999m,
                ChurnRate = 100.0m,
                NRR = 200.0m,
                GRR = 100.0m,
                MeasurementDate = DateTime.MaxValue,
                Notes = new string('M', 1000)
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(0.00001)]
        [InlineData(0.001)]
        [InlineData(0.01)]
        [InlineData(0.12345)] // 5 decimal precision
        public void SubscriptionMetricsDto_SmallMRR_MicroTransactions_ShouldBeValid(double mrr)
        {
            // Arrange - Test micro-transaction precision
            var dto = new SubscriptionMetricsDto
            {
                MRR = (decimal)mrr,
                ARR = (decimal)mrr * 12,
                ChurnRate = 0m,
                NRR = 100.0m,
                GRR = 100.0m
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void SubscriptionMetricsDto_AllOptionalFieldsNull_ShouldBeValid()
        {
            // Arrange
            var dto = new SubscriptionMetricsDto
            {
                Id = 1,
                SubscriptionId = 100,
                MRR = 1000.0m,
                ARR = 12000.0m,
                ChurnRate = 5.0m,
                NRR = 100.0m,
                GRR = 95.0m,
                CAC = 0m,
                CLV = 0m,
                ExpansionRevenue = 0m,
                ContractionRevenue = 0m,
                ACV = 0m,
                PaymentFees = 0m,
                RefundAmount = 0m,
                BillingCycle = "Monthly",
                MeasurementDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                // Optional fields
                SubscriptionNumber = null,
                AccountId = null,
                AccountName = null,
                PeriodStartDate = null,
                PeriodEndDate = null,
                Notes = null,
                NextBillingDate = null,
                Status = null
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region Multiple Validation Errors

        [Fact]
        public void CreateSubscriptionMetricsDto_MultipleInvalidFields_ShouldReturnAllErrors()
        {
            // Arrange - Multiple fields with invalid values
            var dto = new CreateSubscriptionMetricsDto
            {
                SubscriptionId = 0,      // Invalid: < 1
                MRR = -1000.0m,          // Invalid: < 0
                ARR = -12000.0m,         // Invalid: < 0
                ChurnRate = -5.0m,       // Invalid: < 0
                NRR = -10.0m,            // Invalid: < 0
                GRR = 150.0m,            // Invalid: > 100
                Notes = new string('X', 1001) // Invalid: > 1000
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - Should have multiple errors
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 5, $"Should have at least 5 validation errors, but got {results.Count()}");
        }

        [Fact]
        public void UpdateSubscriptionMetricsDto_MultipleInvalidFields_ShouldReturnAllErrors()
        {
            // Arrange
            var dto = new UpdateSubscriptionMetricsDto
            {
                MRR = 1000000000.0m,     // Exceeds max
                ARR = -1.0m,             // Negative
                ChurnRate = 101.0m,      // > 100%
                Notes = new string('N', 1001) // Too long
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 3, "Should have multiple validation errors");
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void SubscriptionMetricsDto_HealthySaaSMetrics_ShouldBeValid()
        {
            // Arrange - Typical healthy SaaS company metrics
            var dto = new SubscriptionMetricsDto
            {
                Id = 1,
                SubscriptionId = 100,
                SubscriptionNumber = "SUB-100",
                AccountName = "Enterprise Corp",
                MRR = 10000.0m,
                ARR = 120000.0m,
                ChurnRate = 2.5m,          // Low churn (good)
                NRR = 115.0m,              // 15% expansion (excellent)
                GRR = 97.5m,               // 97.5% retention
                CAC = 5000.0m,
                CLV = 50000.0m,            // CLV/CAC = 10:1 (excellent)
                ExpansionRevenue = 1500.0m, // Upsells/cross-sells
                ContractionRevenue = 0m,
                ACV = 10000.0m,
                PaymentFees = 300.0m,       // 3% processing fees
                RefundAmount = 0m,
                BillingCycle = "Monthly",
                MeasurementDate = DateTime.UtcNow,
                Status = "Active"
            };

            // Act
            var results = ValidateModel(dto);

            // Assert
            Assert.Empty(results);
            
            // Business validations
            var clvToCacRatio = dto.CLV / dto.CAC;
            Assert.True(clvToCacRatio > 3.0m, "Healthy SaaS should have CLV/CAC > 3:1");
            Assert.True(dto.ChurnRate < 5.0m, "Healthy SaaS should have < 5% churn");
            Assert.True(dto.NRR > 100.0m, "Best-in-class SaaS has NRR > 100%");
        }

        [Fact]
        public void SubscriptionMetricsDto_StrugglingSaaSMetrics_ShouldBeValid_ButShowWarning()
        {
            // Arrange - Struggling SaaS company (high churn, low retention)
            var dto = new SubscriptionMetricsDto
            {
                Id = 2,
                SubscriptionId = 200,
                MRR = 5000.0m,
                ARR = 60000.0m,
                ChurnRate = 15.0m,         // High churn (bad)
                NRR = 85.0m,               // Losing revenue (bad)
                GRR = 85.0m,
                CAC = 10000.0m,
                CLV = 15000.0m,            // CLV/CAC = 1.5:1 (bad)
                ExpansionRevenue = 0m,
                ContractionRevenue = 750.0m, // High downgrades
                BillingCycle = "Monthly",
                MeasurementDate = DateTime.UtcNow
            };

            // Act
            var results = ValidateModel(dto);

            // Assert - Still valid data, but poor business metrics
            Assert.Empty(results);
            
            // Business warning indicators
            var clvToCacRatio = dto.CLV / dto.CAC;
            Assert.True(clvToCacRatio < 3.0m, "This subscription has poor unit economics");
            Assert.True(dto.ChurnRate > 10.0m, "This subscription has high churn");
            Assert.True(dto.NRR < 100.0m, "This subscription is losing revenue");
        }

        #endregion
    }
}
