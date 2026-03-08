// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

/*
 * AP-027: Input Validation for CreateServiceRequestDto
 * ======================================================
 * Tests verify DataAnnotation validations on CreateServiceRequestDto.
 * Validations covered:
 * - Subject: [Required], [StringLength(500, MinimumLength = 1)]
 * - Description: [StringLength(4000)]
 * - RequesterEmail: [EmailAddress], [StringLength(254)]
 * - SourceEmailAddress: [EmailAddress], [StringLength(254)]
 * - RequesterName: [StringLength(200)]
 * - RequesterPhone: [StringLength(50)]
 * - ExternalReferenceId: [StringLength(200)]
 * - SourcePhoneNumber: [StringLength(50)]
 * - ConversationId: [StringLength(100)]
 * - Tags: [StringLength(500)]
 * - InternalNotes: [StringLength(4000)]
 * - ExpediteReason: [StringLength(500)]
 * - EstimatedEffortHours: [Range(0, 10000)]
 */

using System;
using System.Linq;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Unit tests for <see cref="CreateServiceRequestDto"/> DataAnnotation validations (AP-027).
    /// </summary>
    public class ServiceRequestDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        private static CreateServiceRequestDto CreateValidDto() => new CreateServiceRequestDto
        {
            Subject = "Test service request",
            Priority = ServiceRequestPriority.Medium,
            Channel = ServiceRequestChannel.SelfServicePortal,
        };

        #region Subject — AP-027

        [Fact]
        public void Subject_Null_FailsRequired()
        {
            var dto = CreateValidDto();
            dto.Subject = null!;
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.Subject)));
        }

        [Fact]
        public void Subject_Empty_FailsMinimumLength()
        {
            var dto = CreateValidDto();
            dto.Subject = string.Empty;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
        }

        [Fact]
        public void Subject_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.Subject = new string('A', 501);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.Subject)));
        }

        [Fact]
        public void Subject_Valid_Passes()
        {
            var dto = CreateValidDto();
            dto.Subject = "Network connectivity issue";
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        #endregion

        #region Email fields — AP-027

        [Theory]
        [InlineData("valid@example.com", true)]
        [InlineData("noatsign", false)]
        [InlineData(null, true)]
        public void RequesterEmail_ValidatesEmailFormat(string? email, bool shouldBeValid)
        {
            var dto = CreateValidDto();
            dto.RequesterEmail = email;
            var results = ValidateModel(dto);
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.Contains(results, r =>
                    r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.RequesterEmail)));
            }
        }

        [Fact]
        public void RequesterEmail_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.RequesterEmail = new string('a', 246) + "@test.com"; // 255 chars, exceeds 254 limit
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.RequesterEmail)));
        }

        [Theory]
        [InlineData("support@company.com", true)]
        [InlineData("bademailformat", false)]
        [InlineData(null, true)]
        public void SourceEmailAddress_ValidatesEmailFormat(string? email, bool shouldBeValid)
        {
            var dto = CreateValidDto();
            dto.SourceEmailAddress = email;
            var results = ValidateModel(dto);
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.Contains(results, r =>
                    r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.SourceEmailAddress)));
            }
        }

        #endregion

        #region StringLength fields — AP-027

        [Theory]
        [InlineData(nameof(CreateServiceRequestDto.RequesterName), 201)]
        [InlineData(nameof(CreateServiceRequestDto.RequesterPhone), 51)]
        [InlineData(nameof(CreateServiceRequestDto.ExternalReferenceId), 201)]
        [InlineData(nameof(CreateServiceRequestDto.SourcePhoneNumber), 51)]
        [InlineData(nameof(CreateServiceRequestDto.ConversationId), 101)]
        [InlineData(nameof(CreateServiceRequestDto.ExpediteReason), 501)]
        [InlineData(nameof(CreateServiceRequestDto.Tags), 501)]
        public void StringField_ExceedsMaxLength_FailsStringLength(string propertyName, int overLength)
        {
            var dto = CreateValidDto();
            var prop = typeof(CreateServiceRequestDto).GetProperty(propertyName)!;
            prop.SetValue(dto, new string('X', overLength));
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, propertyName));
        }

        [Fact]
        public void InternalNotes_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.InternalNotes = new string('N', 4001);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.InternalNotes)));
        }

        [Fact]
        public void Description_ExceedsMaxLength_FailsStringLength()
        {
            var dto = CreateValidDto();
            dto.Description = new string('D', 4001);
            var results = ValidateModel(dto);
            Assert.Contains(results, r =>
                r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.Description)));
        }

        #endregion

        #region EstimatedEffortHours — AP-027

        [Theory]
        [InlineData("0.00", true)]
        [InlineData("0.01", true)]
        [InlineData("10000.00", true)]
        [InlineData("10000.01", false)]
        public void EstimatedEffortHours_ValidatesRange(string hoursStr, bool shouldBeValid)
        {
            var dto = CreateValidDto();
            dto.EstimatedEffortHours = decimal.Parse(hoursStr, System.Globalization.CultureInfo.InvariantCulture);
            var results = ValidateModel(dto);
            if (shouldBeValid)
            {
                Assert.Empty(results);
            }
            else
            {
                Assert.Contains(results, r =>
                    r.MemberNames != null && Enumerable.Contains(r.MemberNames, nameof(dto.EstimatedEffortHours)));
            }
        }

        [Fact]
        public void EstimatedEffortHours_Null_IsValid()
        {
            var dto = CreateValidDto();
            dto.EstimatedEffortHours = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        #endregion

        #region Full valid DTO — AP-027

        [Fact]
        public void CreateServiceRequestDto_WithAllValidData_IsValid()
        {
            var dto = new CreateServiceRequestDto
            {
                Subject = "Network connectivity issue",
                Description = "Unable to connect to VPN after the recent update.",
                Channel = ServiceRequestChannel.Email,
                Priority = ServiceRequestPriority.High,
                AccountId = 10,
                ContactId = 5,
                RequesterName = "Jane Doe",
                RequesterEmail = "jane.doe@example.com",
                RequesterPhone = "+1-555-0100",
                SourceEmailAddress = "support@company.com",
                Tags = "network,vpn",
                InternalNotes = "Escalated from chat.",
                EstimatedEffortHours = 2.5m,
            };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        #endregion
    }
}
