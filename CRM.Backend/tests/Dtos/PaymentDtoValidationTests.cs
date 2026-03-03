// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * VALIDATION ATTRIBUTES ADDED TO SOURCE DTOs
 * ===========================================
 * DataAnnotations validation attributes were added to PaymentDto.cs as part of this test implementation.
 * The following classes received validation attributes:
 * - CreatePaymentDto:
 *   - AccountId: [Required], [Range(1, int.MaxValue)]
 *   - Amount: [Required], [Range(0.01, 999999999.99)]
 *   - Description: [StringLength(1000)]
 *   - TokenizedCardId: [StringLength(200)]
 * - ProcessPaymentDto:
 *   - Amount: [Required], [Range(0.01, 999999999.99)]
 *   - TokenizedCardId: [StringLength(200)]
 *   - AuthorizationCode: [StringLength(100)]
 *   - Description: [StringLength(1000)]
 * - RefundPaymentRequestDto:
 *   - RefundAmount: [Range(0.01, 999999999.99)]
 *   - Reason: [Required], [StringLength(500, MinimumLength = 10)]
 *
 * These validations ensure data integrity for payment operations.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    public class PaymentDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        #region Helper Methods

        private CreatePaymentDto CreateValidPaymentDto()
        {
            return new CreatePaymentDto
            {
                AccountId = 1,
                Amount = 100.00m,
                PaymentMethod = PaymentMethod.CreditCard,
                Description = "Test payment"
            };
        }

        private ProcessPaymentDto CreateValidProcessPaymentDto()
        {
            return new ProcessPaymentDto
            {
                Amount = 250.50m,
                PaymentMethod = PaymentMethod.CreditCard,
                TokenizedCardId = "tok_visa_1234",
                Description = "Processing payment"
            };
        }

        private RefundPaymentRequestDto CreateValidRefundRequest()
        {
            return new RefundPaymentRequestDto
            {
                RefundAmount = 50.00m,
                Reason = "Customer requested refund due to duplicate charge"
            };
        }

        #endregion

        #region CreatePaymentDto - AccountId Tests

        [Theory]
        [InlineData(0, false)]
        [InlineData(-1, false)]
        [InlineData(1, true)]
        [InlineData(100, true)]
        [InlineData(int.MaxValue, true)]
        public void CreatePaymentDto_AccountId_WithVariousValues_ValidatesCorrectly(int accountId, bool shouldBeValid)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.AccountId = accountId;

            // Act
            var results = ValidateModel(payment);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("AccountId"));
            }
        }

        #endregion

        #region CreatePaymentDto - Amount Tests

        [Theory]
        [InlineData(0, false)]
        [InlineData(0.001, false)] // Less than min 0.01
        [InlineData(0.01, true)]
        [InlineData(1.00, true)]
        [InlineData(100.50, true)]
        [InlineData(999999999.99, true)]
        [InlineData(1000000000.00, false)] // Exceeds max
        public void CreatePaymentDto_Amount_WithVariousValues_ValidatesCorrectly(decimal amount, bool shouldBeValid)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Amount = amount;

            // Act
            var results = ValidateModel(payment);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Amount"));
            }
        }

        [Theory]
        [InlineData(-100.00, false)]
        [InlineData(-0.01, false)]
        [InlineData(0.00, false)]
        public void CreatePaymentDto_Amount_NegativeOrZero_ValidationFails(decimal amount, bool shouldBeValid)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Amount = amount;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("Amount")));
        }

        [Fact]
        public void CreatePaymentDto_Amount_AtMinBoundary_ValidationPasses()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Amount = 0.01m;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreatePaymentDto_Amount_AtMaxBoundary_ValidationPasses()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Amount = 999999999.99m;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreatePaymentDto - Description Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("Short description", true)]
        [InlineData("A very long description that contains detailed information about the payment transaction", true)]
        public void CreatePaymentDto_Description_WithVariousValues_ValidatesCorrectly(string? description, bool shouldBeValid)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Description = description;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("Description")));
        }

        [Fact]
        public void CreatePaymentDto_Description_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Description = new string('A', 1001); // Over 1000 chars

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Description"));
        }

        [Fact]
        public void CreatePaymentDto_Description_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.Description = new string('A', 1000); // Exactly 1000 chars

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreatePaymentDto - TokenizedCardId Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("tok_visa_1234", true)]
        [InlineData("card_token_abc123xyz", true)]
        public void CreatePaymentDto_TokenizedCardId_WithVariousValues_ValidatesCorrectly(string? tokenizedCardId, bool shouldBeValid)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.TokenizedCardId = tokenizedCardId;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("TokenizedCardId")));
        }

        [Fact]
        public void CreatePaymentDto_TokenizedCardId_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.TokenizedCardId = new string('T', 201); // Over 200 chars

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("TokenizedCardId"));
        }

        [Fact]
        public void CreatePaymentDto_TokenizedCardId_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.TokenizedCardId = new string('T', 200); // Exactly 200 chars

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region ProcessPaymentDto - Amount Tests

        [Theory]
        [InlineData(0, false)]
        [InlineData(0.01, true)]
        [InlineData(100.00, true)]
        [InlineData(999999999.99, true)]
        [InlineData(1000000000.00, false)]
        public void ProcessPaymentDto_Amount_WithVariousValues_ValidatesCorrectly(decimal amount, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.Amount = amount;

            // Act
            var results = ValidateModel(request);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Amount"));
            }
        }

        [Fact]
        public void ProcessPaymentDto_Amount_NegativeValue_ValidationFails()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.Amount = -50.00m;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Amount"));
        }

        #endregion

        #region ProcessPaymentDto - TokenizedCardId Tests

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("tok_card_123", true)]
        public void ProcessPaymentDto_TokenizedCardId_WithVariousValues_ValidatesCorrectly(string? tokenizedCardId, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.TokenizedCardId = tokenizedCardId;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("TokenizedCardId")));
        }

        [Fact]
        public void ProcessPaymentDto_TokenizedCardId_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.TokenizedCardId = new string('T', 201);

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("TokenizedCardId"));
        }

        #endregion

        #region ProcessPaymentDto - AuthorizationCode Tests

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("AUTH123", true)]
        [InlineData("ABC-123-XYZ", true)]
        public void ProcessPaymentDto_AuthorizationCode_WithVariousValues_ValidatesCorrectly(string? authCode, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.AuthorizationCode = authCode;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("AuthorizationCode")));
        }

        [Fact]
        public void ProcessPaymentDto_AuthorizationCode_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.AuthorizationCode = new string('A', 101);

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("AuthorizationCode"));
        }

        [Fact]
        public void ProcessPaymentDto_AuthorizationCode_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.AuthorizationCode = new string('A', 100);

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region ProcessPaymentDto - Description Tests

        [Fact]
        public void ProcessPaymentDto_Description_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();
            request.Description = new string('D', 1001);

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Description"));
        }

        #endregion

        #region RefundPaymentRequestDto - RefundAmount Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData(0, false)] // Zero not allowed
        [InlineData(0.001, false)] // Less than min
        [InlineData(0.01, true)]
        [InlineData(50.00, true)]
        [InlineData(999999999.99, true)]
        [InlineData(1000000000.00, false)]
        public void RefundPaymentRequestDto_RefundAmount_WithVariousValues_ValidatesCorrectly(object? amount, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidRefundRequest();
            decimal? decimalAmount = amount == null ? (decimal?)null : Convert.ToDecimal(amount);
            if (decimalAmount.HasValue && decimalAmount.Value == 0)
            {
                request.RefundAmount = 0m;
            }
            else
            {
                request.RefundAmount = decimalAmount;
            }

            // Act
            var results = ValidateModel(request);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("RefundAmount"));
            }
        }

        [Theory]
        [InlineData(-100.00, false)]
        [InlineData(-0.01, false)]
        public void RefundPaymentRequestDto_RefundAmount_NegativeValue_ValidationFails(double amount, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.RefundAmount = Convert.ToDecimal(amount);

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("RefundAmount")));
        }

        #endregion

        #region RefundPaymentRequestDto - Reason Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Too short", false)] // Less than 10 chars
        [InlineData("Valid reason for refund request", true)]
        [InlineData("Customer requested refund", true)]
        public void RefundPaymentRequestDto_Reason_WithVariousValues_ValidatesCorrectly(string? reason, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.Reason = reason!;

            // Act
            var results = ValidateModel(request);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Reason"));
            }
        }

        [Fact]
        public void RefundPaymentRequestDto_Reason_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.Reason = new string('R', 501); // Over 500 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Reason"));
        }

        [Fact]
        public void RefundPaymentRequestDto_Reason_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.Reason = new string('R', 500); // Exactly 500 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RefundPaymentRequestDto_Reason_AtMinLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.Reason = "1234567890"; // Exactly 10 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RefundPaymentRequestDto_Reason_BelowMinLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidRefundRequest();
            request.Reason = "123456789"; // 9 chars - below min

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Reason"));
        }

        #endregion

        #region Edge Cases and Combined Validations

        [Fact]
        public void CreatePaymentDto_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var payment = CreateValidPaymentDto();

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreatePaymentDto_MinimalValidData_ValidationPasses()
        {
            // Arrange
            var payment = new CreatePaymentDto
            {
                AccountId = 1,
                Amount = 0.01m
            };

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreatePaymentDto_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            // Arrange
            var payment = new CreatePaymentDto
            {
                AccountId = 0, // Invalid
                Amount = 0, // Invalid
                Description = new string('X', 1001) // Too long
            };

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 2); // At least AccountId and Amount errors
        }

        [Fact]
        public void ProcessPaymentDto_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var request = CreateValidProcessPaymentDto();

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void ProcessPaymentDto_MinimalValidData_ValidationPasses()
        {
            // Arrange
            var request = new ProcessPaymentDto
            {
                Amount = 25.00m
            };

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RefundPaymentRequestDto_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var request = CreateValidRefundRequest();

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RefundPaymentRequestDto_WithoutRefundAmount_ValidationPasses()
        {
            // Arrange
            var request = new RefundPaymentRequestDto
            {
                RefundAmount = null,
                Reason = "Full refund requested by customer"
            };

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void RefundPaymentRequestDto_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            // Arrange
            var request = new RefundPaymentRequestDto
            {
                RefundAmount = 0m, // Invalid
                Reason = "short" // Too short
            };

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 2);
        }

        #endregion

        #region Payment Method and Type Tests

        [Theory]
        [InlineData(PaymentMethod.CreditCard)]
        [InlineData(PaymentMethod.BankTransfer)]
        [InlineData(PaymentMethod.Cash)]
        [InlineData(PaymentMethod.Check)]
        public void CreatePaymentDto_PaymentMethod_AllValidValues_ValidationPasses(PaymentMethod method)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.PaymentMethod = method;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(PaymentType.Payment)]
        [InlineData(PaymentType.Refund)]
        public void CreatePaymentDto_PaymentType_AllValidValues_ValidationPasses(PaymentType type)
        {
            // Arrange
            var payment = CreateValidPaymentDto();
            payment.PaymentType = type;

            // Act
            var results = ValidateModel(payment);

            // Assert
            Assert.Empty(results);
        }

        #endregion
    }
}
