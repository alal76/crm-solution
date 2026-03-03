// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * VALIDATION ATTRIBUTES ADDED TO SOURCE DTOs
 * ===========================================
 * DataAnnotations validation attributes were added to ContactDto.cs as part of this test implementation.
 * The following classes received validation attributes:
 * - CreateContactRequest: FirstName, LastName (Required, StringLength), Email fields (EmailAddress), Phone fields (Phone), URL fields (Url, StringLength), Twitter/Instagram handles (RegularExpression)
 * - AddSocialMediaRequest: Platform (Required, StringLength), Url (Required, Url, StringLength)
 *
 * These validations ensure data integrity at the DTO layer and are tested comprehensively below.
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
    public class ContactDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        #region Helper Methods

        private CreateContactRequest CreateValidContact()
        {
            return new CreateContactRequest
            {
                FirstName = "John",
                LastName = "Doe",
                ContactType = "Lead",
                EmailPrimary = "john.doe@example.com"
            };
        }

        private AddSocialMediaRequest CreateValidSocialMedia()
        {
            return new AddSocialMediaRequest
            {
                Platform = "LinkedIn",
                Url = "https://www.linkedin.com/in/johndoe",
                Handle = "@johndoe"
            };
        }

        #endregion

        #region CreateContactRequest - FirstName Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("J", true)]
        [InlineData("John", true)]
        [InlineData("Jean-Pierre", true)]
        public void CreateContactRequest_FirstName_WithVariousValues_ValidatesCorrectly(string? firstName, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.FirstName = firstName!;

            // Act
            var results = ValidateModel(contact);

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
        public void CreateContactRequest_FirstName_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.FirstName = new string('A', 101); // 101 characters (max is 100)

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("FirstName"));
        }

        [Fact]
        public void CreateContactRequest_FirstName_AtMaxLength_ValidationPasses()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.FirstName = new string('A', 100); // Exactly 100 characters

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.Empty(results);
        }

        #endregion

        #region CreateContactRequest - LastName Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("D", true)]
        [InlineData("Doe", true)]
        [InlineData("Van Der Berg", true)]
        public void CreateContactRequest_LastName_WithVariousValues_ValidatesCorrectly(string? lastName, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.LastName = lastName!;

            // Act
            var results = ValidateModel(contact);

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
        public void CreateContactRequest_LastName_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.LastName = new string('B', 101); // 101 characters

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("LastName"));
        }

        #endregion

        #region CreateContactRequest - Email Tests

        [Theory]
        [InlineData(null, true)] // Email is optional
        [InlineData("", true)]
        [InlineData("invalid-email", false)]
        [InlineData("missing@domain", false)]
        [InlineData("valid@example.com", true)]
        [InlineData("test.user+tag@example.co.uk", true)]
        [InlineData("name@subdomain.example.com", true)]
        public void CreateContactRequest_EmailPrimary_WithVariousFormats_ValidatesCorrectly(string? email, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.EmailPrimary = email;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("EmailPrimary"));
            }
        }

        [Theory]
        [InlineData("invalid@", false)]
        [InlineData("@example.com", false)]
        [InlineData("test@example.com", true)]
        public void CreateContactRequest_EmailSecondary_WithVariousFormats_ValidatesCorrectly(string email, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.EmailSecondary = email;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("EmailSecondary"));
            }
        }

        [Fact]
        public void CreateContactRequest_EmailPrimary_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.EmailPrimary = new string('a', 180) + "@example.com"; // Over 200 chars

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("EmailPrimary"));
        }

        #endregion

        #region CreateContactRequest - Phone Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("+1 (555) 123-4567", true)]
        [InlineData("555-123-4567", true)]
        [InlineData("5551234567", true)]
        [InlineData("+44 20 7123 4567", true)]
        public void CreateContactRequest_PhonePrimary_WithVariousFormats_ValidatesCorrectly(string? phone, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.PhonePrimary = phone;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("PhonePrimary"));
            }
        }

        [Fact]
        public void CreateContactRequest_PhoneMobile_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.PhoneMobile = new string('1', 51); // Over 50 chars

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("PhoneMobile"));
        }

        #endregion

        #region CreateContactRequest - URL Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("not-a-url", false)]
        [InlineData("http://example.com", true)]
        [InlineData("https://www.example.com", true)]
        [InlineData("https://example.com/path/to/page", true)]
        [InlineData("ftp://files.example.com", true)]
        public void CreateContactRequest_Website_WithVariousFormats_ValidatesCorrectly(string? url, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.Website = url;

            // Act
            var results = ValidateModel(contact);

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

        [Theory]
        [InlineData("https://www.linkedin.com/in/johndoe", true)]
        [InlineData("invalid-url", false)]
        [InlineData("http://linkedin.com/company/example", true)]
        public void CreateContactRequest_LinkedInUrl_WithVariousFormats_ValidatesCorrectly(string url, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.LinkedInUrl = url;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("LinkedInUrl"));
            }
        }

        [Fact]
        public void CreateContactRequest_Website_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.Website = "https://example.com/" + new string('a', 500); // Over 500 chars

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Website"));
        }

        #endregion

        #region CreateContactRequest - Twitter Handle Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("johndoe", true)]
        [InlineData("@johndoe", true)]
        [InlineData("john_doe_123", true)]
        [InlineData("a", true)]
        [InlineData("averylonghandle1", false)] // 16 chars (max is 15)
        [InlineData("john.doe", false)] // Dots not allowed in Twitter handles
        [InlineData("john-doe", false)] // Hyphens not allowed
        [InlineData("john doe", false)] // Spaces not allowed
        public void CreateContactRequest_TwitterHandle_WithVariousFormats_ValidatesCorrectly(string? handle, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.TwitterHandle = handle;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("TwitterHandle"));
            }
        }

        #endregion

        #region CreateContactRequest - Instagram Handle Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("johndoe", true)]
        [InlineData("@johndoe", true)]
        [InlineData("john.doe", true)] // Dots allowed in Instagram
        [InlineData("john_doe", true)]
        [InlineData("a", true)]
        [InlineData("a_very_long_instagram_handle_30", true)] // 30 chars (max)
        [InlineData("a_very_long_instagram_handle_31x", false)] // 31 chars (over max)
        [InlineData("john-doe", false)] // Hyphens not allowed
        [InlineData("john doe", false)] // Spaces not allowed
        public void CreateContactRequest_InstagramHandle_WithVariousFormats_ValidatesCorrectly(string? handle, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.InstagramHandle = handle;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("InstagramHandle"));
            }
        }

        #endregion

        #region CreateContactRequest - Facebook URL Tests

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("https://www.facebook.com/johndoe", true)]
        [InlineData("http://facebook.com/john.doe", true)]
        [InlineData("not-a-url", false)]
        public void CreateContactRequest_FacebookUrl_WithVariousFormats_ValidatesCorrectly(string? url, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.FacebookUrl = url;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("FacebookUrl"));
            }
        }

        #endregion

        #region CreateContactRequest - Blog URL Tests

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("https://blog.example.com", true)]
        [InlineData("http://johndoe.wordpress.com", true)]
        [InlineData("invalid", false)]
        public void CreateContactRequest_BlogUrl_WithVariousFormats_ValidatesCorrectly(string? url, bool shouldBeValid)
        {
            // Arrange
            var contact = CreateValidContact();
            contact.BlogUrl = url;

            // Act
            var results = ValidateModel(contact);

            // Assert
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.NotEmpty(results);
                Assert.Contains(results, r => r.MemberNames.Contains("BlogUrl"));
            }
        }

        #endregion

        #region AddSocialMediaRequest - Platform Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("L", false)] // Too short (min 2)
        [InlineData("Li", true)]
        [InlineData("LinkedIn", true)]
        [InlineData("Twitter", true)]
        public void AddSocialMediaRequest_Platform_WithVariousValues_ValidatesCorrectly(string? platform, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Platform = platform!;

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
                Assert.Contains(results, r => r.MemberNames.Contains("Platform"));
            }
        }

        [Fact]
        public void AddSocialMediaRequest_Platform_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Platform = new string('A', 51); // Over 50 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Platform"));
        }

        #endregion

        #region AddSocialMediaRequest - URL Tests

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("not-a-url", false)]
        [InlineData("https://www.linkedin.com/in/johndoe", true)]
        [InlineData("http://twitter.com/johndoe", true)]
        [InlineData("https://facebook.com/john.doe", true)]
        public void AddSocialMediaRequest_Url_WithVariousFormats_ValidatesCorrectly(string? url, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Url = url!;

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
                Assert.Contains(results, r => r.MemberNames.Contains("Url"));
            }
        }

        [Fact]
        public void AddSocialMediaRequest_Url_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Url = "https://example.com/" + new string('a', 500); // Over 500 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Url"));
        }

        #endregion

        #region AddSocialMediaRequest - Handle Tests

        [Theory]
        [InlineData(null, true)] // Optional
        [InlineData("", true)]
        [InlineData("@johndoe", true)]
        [InlineData("johndoe", true)]
        public void AddSocialMediaRequest_Handle_WithVariousValues_ValidatesCorrectly(string? handle, bool shouldBeValid)
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Handle = handle;

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Equal(shouldBeValid, !results.Any());
        }

        [Fact]
        public void AddSocialMediaRequest_Handle_ExceedsMaxLength_ValidationFails()
        {
            // Arrange
            var request = CreateValidSocialMedia();
            request.Handle = new string('A', 101); // Over 100 chars

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Handle"));
        }

        #endregion

        #region Edge Cases and Combined Validations

        [Fact]
        public void CreateContactRequest_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var contact = CreateValidContact();

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void CreateContactRequest_MultipleInvalidFields_ReturnsMultipleErrors()
        {
            // Arrange
            var contact = CreateValidContact();
            contact.FirstName = ""; // Required
            contact.LastName = ""; // Required
            contact.EmailPrimary = "invalid-email"; // Invalid format

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.NotEmpty(results);
            Assert.True(results.Count() >= 3); // At least 3 errors
        }

        [Fact]
        public void CreateContactRequest_AllOptionalFieldsNull_ValidationPasses()
        {
            // Arrange
            var contact = new CreateContactRequest
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act
            var results = ValidateModel(contact);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public void AddSocialMediaRequest_AllRequiredFields_ValidationPasses()
        {
            // Arrange
            var request = CreateValidSocialMedia();

            // Act
            var results = ValidateModel(request);

            // Assert
            Assert.Empty(results);
        }

        #endregion
    }
}
