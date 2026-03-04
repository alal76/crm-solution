// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Core.Dtos;
using CRM.Tests.Helpers;
using Xunit;

namespace CRM.Tests.Dtos
{
    /// <summary>
    /// Validation tests for Marketing Module DTOs:
    ///   • CreateTrackingLinkDto   – OriginalUrl [Required, Url, MaxLength 2048],
    ///                               UTM params [MaxLength 200], LinkAlias [MaxLength 100]
    ///   • UnsubscribeRequestDto   – Email [Required, EmailAddress, MaxLength 320],
    ///                               Token [MaxLength 500], ReasonNote [MaxLength 1000]
    ///   • EmailTrackingWebhookDto – MessageId [Required, MaxLength 200],
    ///                               Event [Required, MaxLength 50], RecipientEmail/ClickedUrl/UserAgent MaxLength
    ///   • EnrollLeadsDto          – LeadIds [Required] (reference type, fails when null)
    /// </summary>
    public class MarketingModuleDtoValidationTests : ValidatorTestFixtureBase<object>
    {
        protected override object CreateValidator() => new object();

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static CreateTrackingLinkDto ValidTrackingLinkDto() => new()
        {
            OriginalUrl = "https://example.com/landing",
        };

        private static UnsubscribeRequestDto ValidUnsubscribeDto() => new()
        {
            Email = "user@example.com",
        };

        private static EmailTrackingWebhookDto ValidWebhookDto() => new()
        {
            MessageId = "msg-abc-123",
            Event = "delivered",
        };

        // -----------------------------------------------------------------------
        // CreateTrackingLinkDto – OriginalUrl (Required, Url, MaxLength 2048)
        // -----------------------------------------------------------------------

        [Fact]
        public void CreateTrackingLinkDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidTrackingLinkDto());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void OriginalUrl_ShouldFail_WhenValueIsNullOrEmpty(string? url)
        {
            var dto = ValidTrackingLinkDto();
            dto.OriginalUrl = url!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("OriginalUrl"));
        }

        [Theory]
        [InlineData("not-a-url")]
        [InlineData("ftp://invalid-scheme.com")]  // [Url] only allows http/https/ftp per .NET implementation
        public void OriginalUrl_ShouldFail_WhenValueIsInvalidUrl(string url)
        {
            var dto = ValidTrackingLinkDto();
            dto.OriginalUrl = url;
            var results = ValidateModel(dto);
            // Only assert the property fails – [Url] rejects non-http/ftp schemes and plain strings
            Assert.NotEmpty(results);
        }

        [Fact]
        public void OriginalUrl_ShouldFail_WhenValueExceedsMaxLength2048()
        {
            var dto = ValidTrackingLinkDto();
            dto.OriginalUrl = "https://example.com/" + new string('x', 2030); // well over 2048
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("OriginalUrl"));
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://subdomain.example.co.uk/path?q=1")]
        public void OriginalUrl_ShouldPass_WhenValueIsValidHttpUrl(string url)
        {
            var dto = ValidTrackingLinkDto();
            dto.OriginalUrl = url;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateTrackingLinkDto – LinkAlias (MaxLength 100, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void LinkAlias_ShouldFail_WhenValueExceedsMaxLength100()
        {
            var dto = ValidTrackingLinkDto();
            dto.LinkAlias = new string('L', 101);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("LinkAlias"));
        }

        [Fact]
        public void LinkAlias_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidTrackingLinkDto();
            dto.LinkAlias = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // CreateTrackingLinkDto – UTM params (MaxLength 200, all optional)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData("UtmSource")]
        [InlineData("UtmMedium")]
        [InlineData("UtmCampaign")]
        [InlineData("UtmContent")]
        public void UtmParam_ShouldFail_WhenValueExceedsMaxLength200(string propertyName)
        {
            var dto = ValidTrackingLinkDto();
            typeof(CreateTrackingLinkDto).GetProperty(propertyName)!
                .SetValue(dto, new string('U', 201));

            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains(propertyName));
        }

        [Theory]
        [InlineData("UtmSource")]
        [InlineData("UtmMedium")]
        [InlineData("UtmCampaign")]
        [InlineData("UtmContent")]
        public void UtmParam_ShouldPass_WhenValueIsNull(string propertyName)
        {
            var dto = ValidTrackingLinkDto();
            typeof(CreateTrackingLinkDto).GetProperty(propertyName)!
                .SetValue(dto, null);

            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // UnsubscribeRequestDto – Email (Required, EmailAddress, MaxLength 320)
        // -----------------------------------------------------------------------

        [Fact]
        public void UnsubscribeRequestDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidUnsubscribeDto());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void UnsubscribeEmail_ShouldFail_WhenValueIsNullOrEmpty(string? email)
        {
            var dto = ValidUnsubscribeDto();
            dto.Email = email!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Theory]
        [InlineData("plaintext")]
        [InlineData("missing@")]
        [InlineData("@example.com")]
        public void UnsubscribeEmail_ShouldFail_WhenValueIsInvalidEmailFormat(string email)
        {
            var dto = ValidUnsubscribeDto();
            dto.Email = email;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Fact]
        public void UnsubscribeEmail_ShouldFail_WhenValueExceedsMaxLength320()
        {
            var dto = ValidUnsubscribeDto();
            // 310-char local part + "@example.com" = 322 chars (over MaxLength 320)
            dto.Email = new string('a', 310) + "@example.com";
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Email"));
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("first.last+tag@company.co.uk")]
        public void UnsubscribeEmail_ShouldPass_WhenValueIsValidEmail(string email)
        {
            var dto = ValidUnsubscribeDto();
            dto.Email = email;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // UnsubscribeRequestDto – ReasonNote (MaxLength 1000, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void ReasonNote_ShouldFail_WhenValueExceedsMaxLength1000()
        {
            var dto = ValidUnsubscribeDto();
            dto.ReasonNote = new string('R', 1001);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("ReasonNote"));
        }

        [Fact]
        public void ReasonNote_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidUnsubscribeDto();
            dto.ReasonNote = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void ReasonNote_ShouldPass_WhenValueIsAtMaxLength1000()
        {
            var dto = ValidUnsubscribeDto();
            dto.ReasonNote = new string('R', 1000);
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // UnsubscribeRequestDto – Token (MaxLength 500, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void Token_ShouldFail_WhenValueExceedsMaxLength500()
        {
            var dto = ValidUnsubscribeDto();
            dto.Token = new string('T', 501);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Token"));
        }

        [Fact]
        public void Token_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidUnsubscribeDto();
            dto.Token = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // EmailTrackingWebhookDto – MessageId (Required, MaxLength 200)
        // -----------------------------------------------------------------------

        [Fact]
        public void EmailTrackingWebhookDto_ValidObject_PassesValidation()
        {
            var results = ValidateModel(ValidWebhookDto());
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MessageId_ShouldFail_WhenValueIsNullOrEmpty(string? messageId)
        {
            var dto = ValidWebhookDto();
            dto.MessageId = messageId!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("MessageId"));
        }

        [Fact]
        public void MessageId_ShouldFail_WhenValueExceedsMaxLength200()
        {
            var dto = ValidWebhookDto();
            dto.MessageId = new string('M', 201);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("MessageId"));
        }

        // -----------------------------------------------------------------------
        // EmailTrackingWebhookDto – Event (Required, MaxLength 50)
        // -----------------------------------------------------------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Event_ShouldFail_WhenValueIsNullOrEmpty(string? eventValue)
        {
            var dto = ValidWebhookDto();
            dto.Event = eventValue!;
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Event"));
        }

        [Fact]
        public void Event_ShouldFail_WhenValueExceedsMaxLength50()
        {
            var dto = ValidWebhookDto();
            dto.Event = new string('E', 51);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("Event"));
        }

        [Theory]
        [InlineData("delivered")]
        [InlineData("open")]
        [InlineData("click")]
        [InlineData("bounce")]
        [InlineData("unsubscribe")]
        public void Event_ShouldPass_WhenValueIsKnownEventName(string eventName)
        {
            var dto = ValidWebhookDto();
            dto.Event = eventName;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // EmailTrackingWebhookDto – RecipientEmail (MaxLength 320, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void RecipientEmail_ShouldFail_WhenValueExceedsMaxLength320()
        {
            var dto = ValidWebhookDto();
            dto.RecipientEmail = new string('r', 310) + "@example.com"; // 322 chars
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("RecipientEmail"));
        }

        [Fact]
        public void RecipientEmail_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidWebhookDto();
            dto.RecipientEmail = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // EmailTrackingWebhookDto – ClickedUrl (MaxLength 2048, optional)
        // -----------------------------------------------------------------------

        [Fact]
        public void ClickedUrl_ShouldFail_WhenValueExceedsMaxLength2048()
        {
            var dto = ValidWebhookDto();
            dto.ClickedUrl = "https://example.com/" + new string('c', 2030);
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("ClickedUrl"));
        }

        [Fact]
        public void ClickedUrl_ShouldPass_WhenValueIsNull()
        {
            var dto = ValidWebhookDto();
            dto.ClickedUrl = null;
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        // -----------------------------------------------------------------------
        // EnrollLeadsDto – LeadIds ([Required] on reference type List<int>)
        // When set to null, [Required] fires because the reference is null.
        // Default (= new()) is never null so it always passes DataAnnotations.
        // -----------------------------------------------------------------------

        [Fact]
        public void EnrollLeadsDto_ValidObject_PassesValidation()
        {
            var dto = new EnrollLeadsDto { LeadIds = new List<int> { 1, 2, 3 } };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void EnrollLeadsDto_EmptyLeadIdsList_PassesDataAnnotationsValidation()
        {
            // [Required] checks for null, not empty. An empty list is valid here;
            // business rules (service layer) enforce at-least-one-entry.
            var dto = new EnrollLeadsDto { LeadIds = new List<int>() };
            var results = ValidateModel(dto);
            Assert.Empty(results);
        }

        [Fact]
        public void EnrollLeadsDto_NullLeadIds_ShouldFail_RequiredValidation()
        {
            var dto = new EnrollLeadsDto { LeadIds = null! };
            var results = ValidateModel(dto);
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.MemberNames.Contains("LeadIds"));
        }
    }
}
