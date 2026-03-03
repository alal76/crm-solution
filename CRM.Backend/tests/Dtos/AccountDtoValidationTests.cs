// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * VALIDATION ATTRIBUTES ADDED TO SOURCE DTOs
 * ===========================================
 * DataAnnotations validation attributes were added to AccountDto.cs as part of this test implementation.
 * The following class received validation attributes:
 * - CreateAccountDto:
 *   - Email: [Required], [EmailAddress], [StringLength(200)]
 *   - SecondaryEmail: [EmailAddress], [StringLength(200)]
 *   - Phone: [Required], [Phone], [StringLength(50)]
 *   - MobilePhone: [Phone], [StringLength(50)]
 *   - FaxNumber: [Phone], [StringLength(50)]
 *   - Website: [Url], [StringLength(500)]
 *
 * Note: CreateAccountDto validation rules were partially applied. Full implementation
 * would include additional rules for FirstName, LastName, Company based on Category
 * (Individual vs Organization). These business rules are typically enforced at the
 * service layer rather than via DataAnnotations alone.
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
    public class AccountDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        #region Helper Methods

        private CreateAccountDto CreateValidAccount()
        {
            return new CreateAccountDto
            {
                Email = "john.doe@example.com",
                Phone = "+1-555-123-4567",
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Doe"
            };
        }

        private CreateAccountDto CreateValidOrganizationAccount()
        {
            return new CreateAccountDto
            {
                Email = "contact@company.com",
                Phone = "+1-555-987-6543",
                Category = AccountCategory.Organization,
                Company = "Acme Corporation"
            };
        }

        #endregion

        #region CreateAccountDto - Email Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("invalid-email", false)]
        [InlineData("missing@", false)]
        [InlineData("@domain.com", false)]
        [InlineData("valid@example.com", true)]
        [InlineData("john.doe+tag@company.co.uk", true)]
        [InlineData("contact@subdomain.example.com", true)]
        public void CreateAccountDto_Email_WithVariousFormats_ValidatesCorrectly(string? email, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.Email = email!;

            // Act
            var results = ValidateModel(account);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            }
        }

        [Fact]
        public void CreateAccountDto_Email_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Email = new string('a', 196) + "@b.co"; // 196+5 = 201 chars, over [StringLength(200)]

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateAccountDto_Email_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var account = CreateValidAccount();
            var localPart = new string('a', 180);
            account.Email = $"{localPart}@example.com"; // 193 chars total

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - SecondaryEmail Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", false)] // [EmailAddress] rejects empty strings
        [InlineData("invalid@", false)]
        [InlineData("@example.com", false)]
        [InlineData("secondary@example.com", true)]
        [InlineData("backup+email@company.com", true)]
        public void CreateAccountDto_SecondaryEmail_WithVariousFormats_ValidatesCorrectly(string? email, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.SecondaryEmail = email;

            // Act
            var results = ValidateModel(account);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("SecondaryEmail"));
            }
        }

        [Fact]
        public void CreateAccountDto_SecondaryEmail_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.SecondaryEmail = new string('b', 191) + "@example.com"; // 191+12=203 chars, over [StringLength(200)]

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("SecondaryEmail"));
        }

        #endregion

        #region CreateAccountDto - Phone Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("+1 (555) 123-4567", true)]
        [InlineData("555-123-4567", true)]
        [InlineData("5551234567", true)]
        [InlineData("+44 20 7123 4567", true)]
        [InlineData("1-800-FLOWERS", false)] // .NET [Phone] rejects alphabetic characters
        public void CreateAccountDto_Phone_WithVariousFormats_ValidatesCorrectly(string? phone, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.Phone = phone!;

            // Act
            var results = ValidateModel(account);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Phone"));
            }
        }

        [Fact]
        public void CreateAccountDto_Phone_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Phone = new string('1', 51); // Over 50 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Phone"));
        }

        [Fact]
        public void CreateAccountDto_Phone_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Phone = "+1 (555) " + new string('1', 39); // Exactly 50 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - MobilePhone Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", false)] // Empty string fails [Phone] attribute in .NET
        [InlineData("+1-555-987-6543", true)]
        [InlineData("555-987-6543", true)]
        public void CreateAccountDto_MobilePhone_WithVariousFormats_ValidatesCorrectly(string? phone, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.MobilePhone = phone;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("MobilePhone")));
        }

        [Fact]
        public void CreateAccountDto_MobilePhone_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.MobilePhone = new string('2', 51); // Over 50 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("MobilePhone"));
        }

        #endregion

        #region CreateAccountDto - FaxNumber Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", false)] // Empty string fails [Phone] attribute in .NET
        [InlineData("+1-555-FAX-1234", false)] // Letters in fax number fail [Phone]
        [InlineData("555-321-4567", true)]
        public void CreateAccountDto_FaxNumber_WithVariousFormats_ValidatesCorrectly(string? fax, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.FaxNumber = fax;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("FaxNumber")));
        }

        [Fact]
        public void CreateAccountDto_FaxNumber_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.FaxNumber = new string('3', 51); // Over 50 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("FaxNumber"));
        }

        #endregion

        #region CreateAccountDto - Website Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", false)] // Empty string fails [Url] attribute in .NET
        [InlineData("not-a-url", false)]
        [InlineData("http://example.com", true)]
        [InlineData("https://www.example.com", true)]
        [InlineData("https://www.company.com/about", true)]
        [InlineData("ftp://files.company.com", false)] // [Url] only allows http:// and https://
        public void CreateAccountDto_Website_WithVariousFormats_ValidatesCorrectly(string? url, bool shouldBeValid)
        {
            // Arrange
            var account = CreateValidAccount();
            account.Website = url;

            // Act
            var results = ValidateModel(account);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("Website"));
            }
        }

        [Fact]
        public void CreateAccountDto_Website_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Website = "https://example.com/" + new string('w', 500); // Over 500 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Website"));
        }

        [Fact]
        public void CreateAccountDto_Website_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var account = CreateValidAccount();
            var path = new string('w', 480);
            account.Website = $"https://example.com/{path}"; // Close to 500 chars

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - Category Tests

        [Theory]
        [InlineData(AccountCategory.Individual)]
        [InlineData(AccountCategory.Organization)]
        public void CreateAccountDto_Category_AllValidValues_ValidationPasses(AccountCategory category)
        {
            // Arrange
            var account = CreateValidAccount();
            account.Category = category;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - AccountType Tests

        [Theory]
        [InlineData(AccountType.Individual)]
        [InlineData(AccountType.SmallBusiness)]
        [InlineData(AccountType.MidMarket)]
        [InlineData(AccountType.Enterprise)]
        [InlineData(AccountType.Government)]
        [InlineData(AccountType.NonProfit)]
        public void CreateAccountDto_AccountType_AllValidValues_ValidationPasses(AccountType accountType)
        {
            // Arrange
            var account = CreateValidAccount();
            account.AccountType = accountType;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - Priority Tests

        [Theory]
        [InlineData(AccountPriority.Low)]
        [InlineData(AccountPriority.Medium)]
        [InlineData(AccountPriority.High)]
        [InlineData(AccountPriority.Critical)]
        public void CreateAccountDto_Priority_AllValidValues_ValidationPasses(AccountPriority priority)
        {
            // Arrange
            var account = CreateValidAccount();
            account.Priority = priority;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateAccountDto - LifecycleStage Tests

        [Theory]
        [InlineData(AccountLifecycleStage.Lead)]
        [InlineData(AccountLifecycleStage.Opportunity)]
        [InlineData(AccountLifecycleStage.Active)]
        [InlineData(AccountLifecycleStage.AtRisk)]
        [InlineData(AccountLifecycleStage.Churned)]
        [InlineData(AccountLifecycleStage.WinBack)]
        public void CreateAccountDto_LifecycleStage_AllValidValues_ValidationPasses(AccountLifecycleStage stage)
        {
            // Arrange
            var account = CreateValidAccount();
            account.LifecycleStage = stage;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region Edge Cases and Combined Validations

        [Fact]
        public void CreateAccountDto_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var account = CreateValidAccount();

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_MinimalValidIndividual_ValidationPasses()
        {
            // Arrange
            var account = new CreateAccountDto
            {
                Email = "john@example.com",
                Phone = "555-1234",
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_MinimalValidOrganization_ValidationPasses()
        {
            // Arrange
            var account = new CreateAccountDto
            {
                Email = "contact@company.com",
                Phone = "555-9876",
                Category = AccountCategory.Organization,
                Company = "Acme Corp"
            };

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            // Arrange
            var account = new CreateAccountDto
            {
                Email = "invalid-email", // Invalid format
                Phone = "", // Required
                SecondaryEmail = "@nodomain", // Invalid format
                Website = "not-a-url" // Invalid format
            };

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 3); // At least Email, Phone, Website errors
        }

        [Fact]
        public void CreateAccountDto_AllOptionalFieldsNull_ValidationPasses()
        {
            // Arrange
            var account = new CreateAccountDto
            {
                Email = "test@example.com",
                Phone = "555-1234"
            };

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_OrganizationAccount_WithAllFields_ValidationPasses()
        {
            // Arrange
            var account = CreateValidOrganizationAccount();
            account.Company = "Acme Corporation";
            account.LegalName = "Acme Corp. LLC";
            account.Industry = "Technology";
            account.AnnualRevenue = 5000000m;
            account.NumberOfEmployees = 250;

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_IndividualAccount_WithAllFields_ValidationPasses()
        {
            // Arrange
            var account = CreateValidAccount();
            account.FirstName = "John";
            account.LastName = "Doe";
            account.DateOfBirth = new DateTime(1985, 5, 15);
            account.Gender = "Male";
            account.JobTitle = "Software Engineer";

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateAccountDto_BothEmailFields_Invalid_ReturnsMultipleEmailErrors()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Email = "invalid-primary";
            account.SecondaryEmail = "invalid-secondary";

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            Assert.Contains(results, r => r.MemberNames.Contains("SecondaryEmail"));
        }

        [Fact]
        public void CreateAccountDto_AllPhoneFields_ExceedMaxLength_ValidationFails()
        {
            // Arrange
            var account = CreateValidAccount();
            account.Phone = new string('1', 51);
            account.MobilePhone = new string('2', 51);
            account.FaxNumber = new string('3', 51);

            // Act
            var results = ValidateModel(account);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Phone"));
            Assert.Contains(results, r => r.MemberNames.Contains("MobilePhone"));
            Assert.Contains(results, r => r.MemberNames.Contains("FaxNumber"));
        }

        #endregion

        #region Business Logic Tests (Category-specific validations)

        /*
         * NOTE: The following tests document expected business rules that should be
         * enforced at the service layer, not at the DTO validation layer.
         * DataAnnotations cannot easily express conditional validation based on Category field.
         *
         * Business Rules:
         * - If Category == Individual: FirstName and LastName should be required
         * - If Category == Organization: Company should be required
         */

        [Fact]
        public void CreateAccountDto_IndividualAccount_Documentation_FirstNameLastNameExpected()
        {
            // This test documents the expected behavior but cannot be enforced via DataAnnotations alone
            // Service layer should validate: Individual accounts require FirstName and LastName
            var account = new CreateAccountDto
            {
                Email = "john@example.com",
                Phone = "555-1234",
                Category = AccountCategory.Individual,
                FirstName = "John",
                LastName = "Doe"
            };

            var results = ValidateModel(account);
            Assert.Empty(results); // DTO validation passes, service layer would enforce business rules
        }

        [Fact]
        public void CreateAccountDto_OrganizationAccount_Documentation_CompanyNameExpected()
        {
            // This test documents the expected behavior but cannot be enforced via DataAnnotations alone
            // Service layer should validate: Organization accounts require Company name
            var account = new CreateAccountDto
            {
                Email = "contact@company.com",
                Phone = "555-9876",
                Category = AccountCategory.Organization,
                Company = "Acme Corporation"
            };

            var results = ValidateModel(account);
            Assert.Empty(results); // DTO validation passes, service layer would enforce business rules
        }

        #endregion
    }
}
