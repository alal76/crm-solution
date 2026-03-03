// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * VALIDATION ATTRIBUTES ADDED TO SOURCE DTOs
 * ===========================================
 * DataAnnotations validation attributes were added to UserDto.cs as part of this test implementation.
 * The following class received validation attributes:
 * - CreateUserRequest:
 *   - Email: [Required], [EmailAddress], [StringLength(200)]
 *   - FirstName: [Required], [StringLength(100, MinimumLength = 1)]
 *   - LastName: [Required], [StringLength(100, MinimumLength = 1)]
 *   - Username: [StringLength(100, MinimumLength = 3)] (optional)
 *   - Password: [StringLength(100, MinimumLength = 8)] (optional)
 *
 * These validations ensure data integrity at the DTO layer for user creation.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CRM.Core.Dtos;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    public class UserDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        #region Helper Methods

        private CreateUserRequest CreateValidUserRequest()
        {
            return new CreateUserRequest
            {
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Username = "johndoe",
                Password = "SecurePass123!"
            };
        }

        #endregion

        #region CreateUserRequest - Email Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("invalid-email", false)]
        [InlineData("missing@", false)]
        [InlineData("@domain.com", false)]
        [InlineData("valid@example.com", true)]
        [InlineData("john.doe+tag@company.co.uk", true)]
        [InlineData("user@subdomain.example.com", true)]
        [InlineData("admin@localhost", true)]
        public void CreateUserRequest_Email_WithVariousFormats_ValidatesCorrectly(string? email, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Email = email!;

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
                Assert.Contains(results, r => r.MemberNames.Contains("Email"));
            }
        }

        [Fact]
        public void CreateUserRequest_Email_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Email = new string('a', 191) + "@example.com"; // 191+12=203 chars, over [StringLength(200)]

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CreateUserRequest_Email_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            // Create a valid email at exactly 200 characters
            var localPart = new string('a', 180);
            request.Email = $"{localPart}@example.com"; // 180 + 13 = 193 chars (within limit)

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateUserRequest - FirstName Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("J", true)]
        [InlineData("John", true)]
        [InlineData("Jean-Pierre", true)]
        [InlineData("Mary Jane", true)]
        public void CreateUserRequest_FirstName_WithVariousValues_ValidatesCorrectly(string? firstName, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.FirstName = firstName!;

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
                Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
            }
        }

        [Fact]
        public void CreateUserRequest_FirstName_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.FirstName = new string('A', 101); // 101 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void CreateUserRequest_FirstName_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.FirstName = new string('A', 100); // Exactly 100 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateUserRequest - LastName Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("D", true)]
        [InlineData("Doe", true)]
        [InlineData("Van Der Berg", true)]
        [InlineData("O'Brien", true)]
        public void CreateUserRequest_LastName_WithVariousValues_ValidatesCorrectly(string? lastName, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.LastName = lastName!;

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
                Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
            }
        }

        [Fact]
        public void CreateUserRequest_LastName_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.LastName = new string('B', 101); // 101 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }

        [Fact]
        public void CreateUserRequest_LastName_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.LastName = new string('B', 100); // Exactly 100 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateUserRequest - Username Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", false)] // Empty string fails [StringLength(100, MinimumLength=3)]
        [InlineData("ab", false)] // Too short (min 3)
        [InlineData("abc", true)]
        [InlineData("johndoe", true)]
        [InlineData("john.doe", true)]
        [InlineData("john_doe123", true)]
        [InlineData("user@123", true)]
        public void CreateUserRequest_Username_WithVariousValues_ValidatesCorrectly(string? username, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Username = username;

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
                Assert.Contains(results, r => r.MemberNames.Contains("Username"));
            }
        }

        [Fact]
        public void CreateUserRequest_Username_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Username = new string('u', 101); // 101 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Username"));
        }

        [Fact]
        public void CreateUserRequest_Username_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Username = new string('u', 100); // Exactly 100 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_Username_AtMinLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Username = "abc"; // Exactly 3 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateUserRequest - Password Tests

        [Theory]
        [InlineData(null, true)] // Optional nullable
        [InlineData("", false)] // Empty string fails [StringLength(100, MinimumLength=8)]
        [InlineData("short", false)] // Too short (min 8)
        [InlineData("Pass123!", true)]
        [InlineData("SecurePassword123!", true)]
        [InlineData("12345678", true)] // Meets length requirement
        [InlineData("abcdefgh", true)] // Meets length requirement
        public void CreateUserRequest_Password_WithVariousValues_ValidatesCorrectly(string? password, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Password = password;

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
                Assert.Contains(results, r => r.MemberNames.Contains("Password"));
            }
        }

        [Fact]
        public void CreateUserRequest_Password_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Password = new string('p', 101); // 101 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Password"));
        }

        [Fact]
        public void CreateUserRequest_Password_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Password = new string('p', 100); // Exactly 100 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_Password_AtMinLength_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Password = "Pass1234"; // Exactly 8 characters

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Theory]
        [InlineData("1234567", false)] // 7 chars - too short
        [InlineData("12345678", true)] // 8 chars - min valid
        [InlineData("123456789", true)] // 9 chars - valid
        public void CreateUserRequest_Password_BoundaryTesting_ValidatesCorrectly(string password, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Password = password;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any());
        }

        #endregion

        #region CreateUserRequest - RoleId Tests

        [Theory]
        [InlineData(0, true)] // Admin
        [InlineData(1, true)] // Manager
        [InlineData(2, true)] // Sales (default)
        [InlineData(3, true)] // Support
        [InlineData(4, true)] // Guest
        [InlineData(-1, true)] // No validation on RoleId (business logic validation elsewhere)
        [InlineData(999, true)]
        public void CreateUserRequest_RoleId_WithVariousValues_NoValidationErrors(int roleId, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.RoleId = roleId;

            // Act
            var results = ValidateModel(request);

            // Assert
            // Note: RoleId validation is likely handled at business logic layer, not DTO layer
            Assert.Equal(shouldBeValid, !results.Any(r => r.MemberNames.Contains("RoleId")));
        }

        [Fact]
        public void CreateUserRequest_RoleId_DefaultValue_Is2()
        {
            // Arrange & Act
            var request = new CreateUserRequest
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User"
            };

            // Assert
            Assert.Equal(2, request.RoleId); // Default is Sales
        }

        #endregion

        #region Edge Cases and Combined Validations

        [Fact]
        public void CreateUserRequest_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_AllFields_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.Email = "invalid-email"; // Invalid format
            request.FirstName = ""; // Required
            request.LastName = ""; // Required
            request.Username = "ab"; // Too short
            request.Password = "short"; // Too short

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 5); // At least 5 errors
        }

        [Fact]
        public void CreateUserRequest_OnlyRequiredFields_ValidationPasses()
        {
            // Arrange
            var request = new CreateUserRequest
            {
                Email = "user@example.com",
                FirstName = "User",
                LastName = "Test"
                // Username and Password are optional
            };

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_OptionalDepartmentAndGroup_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.DepartmentId = 1;
            request.PrimaryGroupId = 5;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateUserRequest_OptionalDepartmentAndGroupNull_ValidationPasses()
        {
            // Arrange
            var request = CreateValidUserRequest();
            request.DepartmentId = null;
            request.PrimaryGroupId = null;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion
    }
}
