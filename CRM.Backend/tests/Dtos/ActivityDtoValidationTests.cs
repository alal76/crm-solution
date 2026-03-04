// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Core.DTOs;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Validation tests for ActivityDto and CreateActivityDto.
    ///
    /// Covers:
    ///   ActivityDto      – Title [Required, MaxLength 200], Description [MaxLength 2000]
    ///   CreateActivityDto – Title [Required, MaxLength 200], Description [MaxLength 2000]
    ///
    /// Note: [Required] on non-nullable value-type properties (int, DateTime) is a
    /// documentation marker only; Validator.TryValidateObject never fails them because
    /// the value can never be null. Those properties are exercised via "valid object"
    /// tests rather than "Required-absent" tests.
    /// </summary>
    public class ActivityDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static ActivityDto ValidActivityDto() => new()
        {
            ActivityType = 1,
            Title = "Call with client",
            ActivityDate = DateTime.UtcNow,
        };

        private static CreateActivityDto ValidCreateActivityDto() => new()
        {
            ActivityType = 1,
            Title = "Follow-up email",
            ActivityDate = DateTime.UtcNow,
        };

        // -----------------------------------------------------------------------
        // ActivityDto – Title (Required, MaxLength 200)
        // -----------------------------------------------------------------------

        [Fact]
        public void ActivityDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidActivityDto());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ActivityDto_Title_ShouldFail_WhenValueIsNullOrEmpty(string? title)
        {
            var dto = ValidActivityDto();
            dto.Title = title!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void ActivityDto_Title_ShouldFail_WhenValueExceedsMaxLength200()
        {
            var dto = ValidActivityDto();
            dto.Title = new string('A', 201); // one over MaxLength(200)
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void ActivityDto_Title_ShouldPass_WhenValueIsExactlyMaxLength200()
        {
            var dto = ValidActivityDto();
            dto.Title = new string('A', 200);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void ActivityDto_Title_ShouldPass_WhenValueIsShortString()
        {
            var dto = ValidActivityDto();
            dto.Title = "Hi";
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // ActivityDto – Description (MaxLength 2000, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void ActivityDto_Description_ShouldFail_WhenValueExceedsMaxLength2000()
        {
            var dto = ValidActivityDto();
            dto.Description = new string('D', 2001); // one over MaxLength(2000)
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Description"));
        }

        [Fact]
        public void ActivityDto_Description_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidActivityDto();
            dto.Description = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void ActivityDto_Description_ShouldPass_WhenValueIsExactlyMaxLength2000()
        {
            var dto = ValidActivityDto();
            dto.Description = new string('D', 2000);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateActivityDto – Title (Required, MaxLength 200)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateActivityDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidCreateActivityDto());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void CreateActivityDto_Title_ShouldFail_WhenValueIsNullOrEmpty(string? title)
        {
            var dto = ValidCreateActivityDto();
            dto.Title = title!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void CreateActivityDto_Title_ShouldFail_WhenValueExceedsMaxLength200()
        {
            var dto = ValidCreateActivityDto();
            dto.Title = new string('T', 201);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Title"));
        }

        [Fact]
        public void CreateActivityDto_Title_ShouldPass_WhenValueIsExactlyMaxLength200()
        {
            var dto = ValidCreateActivityDto();
            dto.Title = new string('T', 200);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateActivityDto – Description (MaxLength 2000, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateActivityDto_Description_ShouldFail_WhenValueExceedsMaxLength2000()
        {
            var dto = ValidCreateActivityDto();
            dto.Description = new string('D', 2001);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Description"));
        }

        [Fact]
        public void CreateActivityDto_Description_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidCreateActivityDto();
            dto.Description = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void CreateActivityDto_Description_ShouldPass_WhenValueIsAtMaxLength2000()
        {
            var dto = ValidCreateActivityDto();
            dto.Description = new string('D', 2000);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateActivityDto – ActivityType ([Required] on non-nullable int)
        // [Required] on int is never null, so TryValidateObject always passes.
        // This test documents that default ActivityType = 0 is accepted by the
        // DataAnnotations validator (business rules enforce non-zero elsewhere).
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateActivityDto_ActivityType_DefaultValue_PassesDataAnnotationsValidation()
        {
            // int default is 0. [Required] on int is a no-op for TryValidateObject.
            var dto = new CreateActivityDto
            {
                ActivityType = 0,
                Title = "Test",
            };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }
    }
}
