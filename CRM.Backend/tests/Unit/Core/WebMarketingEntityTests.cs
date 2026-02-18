// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using CRM.Core.Entities;

namespace CRM.Tests.Unit.Core;

#region FormFieldType Enum Tests

public class FormFieldTypeEnumTests
{
    [Theory]
    [InlineData(FormFieldType.Text, 0)]
    [InlineData(FormFieldType.TextArea, 1)]
    [InlineData(FormFieldType.Email, 2)]
    [InlineData(FormFieldType.Phone, 3)]
    [InlineData(FormFieldType.Number, 4)]
    [InlineData(FormFieldType.Date, 5)]
    [InlineData(FormFieldType.DateTime, 6)]
    [InlineData(FormFieldType.Dropdown, 7)]
    [InlineData(FormFieldType.MultiSelect, 8)]
    [InlineData(FormFieldType.Radio, 9)]
    [InlineData(FormFieldType.Checkbox, 10)]
    [InlineData(FormFieldType.FileUpload, 11)]
    [InlineData(FormFieldType.Hidden, 12)]
    [InlineData(FormFieldType.Country, 13)]
    [InlineData(FormFieldType.State, 14)]
    [InlineData(FormFieldType.Url, 15)]
    [InlineData(FormFieldType.Rating, 16)]
    [InlineData(FormFieldType.Range, 17)]
    [InlineData(FormFieldType.Consent, 18)]
    [InlineData(FormFieldType.Captcha, 19)]
    [InlineData(FormFieldType.Heading, 20)]
    [InlineData(FormFieldType.Paragraph, 21)]
    [InlineData(FormFieldType.Divider, 22)]
    public void FormFieldType_ShouldHaveCorrectValues(FormFieldType fieldType, int expected)
    {
        ((int)fieldType).Should().Be(expected);
    }

    [Fact]
    public void FormFieldType_ShouldHave23Values()
    {
        var values = Enum.GetValues(typeof(FormFieldType));
        values.Length.Should().Be(23);
    }
}

#endregion

#region FormStatus Enum Tests

public class FormStatusEnumTests
{
    [Theory]
    [InlineData(FormStatus.Draft, 0)]
    [InlineData(FormStatus.Published, 1)]
    [InlineData(FormStatus.Paused, 2)]
    [InlineData(FormStatus.Archived, 3)]
    public void FormStatus_ShouldHaveCorrectValues(FormStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void FormStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(FormStatus));
        values.Length.Should().Be(4);
    }
}

#endregion

#region FormSubmitAction Enum Tests

public class FormSubmitActionEnumTests
{
    [Theory]
    [InlineData(FormSubmitAction.ShowMessage, 0)]
    [InlineData(FormSubmitAction.Redirect, 1)]
    [InlineData(FormSubmitAction.ShowForm, 2)]
    [InlineData(FormSubmitAction.StayOnPage, 3)]
    public void FormSubmitAction_ShouldHaveCorrectValues(FormSubmitAction action, int expected)
    {
        ((int)action).Should().Be(expected);
    }

    [Fact]
    public void FormSubmitAction_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(FormSubmitAction));
        values.Length.Should().Be(4);
    }
}

#endregion

#region SubmissionStatus Enum Tests

public class SubmissionStatusEnumTests
{
    [Theory]
    [InlineData(SubmissionStatus.New, 0)]
    [InlineData(SubmissionStatus.Processing, 1)]
    [InlineData(SubmissionStatus.LeadCreated, 2)]
    [InlineData(SubmissionStatus.ContactCreated, 3)]
    [InlineData(SubmissionStatus.SubmittedExternal, 4)]
    [InlineData(SubmissionStatus.Failed, 5)]
    [InlineData(SubmissionStatus.Spam, 6)]
    [InlineData(SubmissionStatus.Duplicate, 7)]
    public void SubmissionStatus_ShouldHaveCorrectValues(SubmissionStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void SubmissionStatus_ShouldHave8Values()
    {
        var values = Enum.GetValues(typeof(SubmissionStatus));
        values.Length.Should().Be(8);
    }
}

#endregion

#region LandingPageStatus Enum Tests

public class LandingPageStatusEnumTests
{
    [Theory]
    [InlineData(LandingPageStatus.Draft, 0)]
    [InlineData(LandingPageStatus.Published, 1)]
    [InlineData(LandingPageStatus.Archived, 2)]
    [InlineData(LandingPageStatus.Scheduled, 3)]
    public void LandingPageStatus_ShouldHaveCorrectValues(LandingPageStatus status, int expected)
    {
        ((int)status).Should().Be(expected);
    }

    [Fact]
    public void LandingPageStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues(typeof(LandingPageStatus));
        values.Length.Should().Be(4);
    }
}

#endregion

#region LandingPageTemplate Enum Tests

public class LandingPageTemplateEnumTests
{
    [Theory]
    [InlineData(LandingPageTemplate.Blank, 0)]
    [InlineData(LandingPageTemplate.LeadCapture, 1)]
    [InlineData(LandingPageTemplate.ProductShowcase, 2)]
    [InlineData(LandingPageTemplate.EventRegistration, 3)]
    [InlineData(LandingPageTemplate.WebinarRegistration, 4)]
    [InlineData(LandingPageTemplate.EbookDownload, 5)]
    [InlineData(LandingPageTemplate.ThankYou, 6)]
    public void LandingPageTemplate_ShouldHaveCorrectValues(LandingPageTemplate template, int expected)
    {
        ((int)template).Should().Be(expected);
    }

    [Fact]
    public void LandingPageTemplate_ShouldHave7Values()
    {
        var values = Enum.GetValues(typeof(LandingPageTemplate));
        values.Length.Should().Be(7);
    }
}

#endregion

#region LandingPageBlockType Enum Tests

public class LandingPageBlockTypeEnumTests
{
    [Theory]
    [InlineData(LandingPageBlockType.Hero, 0)]
    [InlineData(LandingPageBlockType.Text, 1)]
    [InlineData(LandingPageBlockType.Image, 2)]
    [InlineData(LandingPageBlockType.Video, 3)]
    [InlineData(LandingPageBlockType.Form, 4)]
    [InlineData(LandingPageBlockType.Button, 5)]
    [InlineData(LandingPageBlockType.TwoColumn, 6)]
    [InlineData(LandingPageBlockType.ThreeColumn, 7)]
    [InlineData(LandingPageBlockType.Features, 8)]
    [InlineData(LandingPageBlockType.Testimonial, 9)]
    [InlineData(LandingPageBlockType.Pricing, 10)]
    [InlineData(LandingPageBlockType.FAQ, 11)]
    [InlineData(LandingPageBlockType.SocialProof, 12)]
    [InlineData(LandingPageBlockType.Countdown, 13)]
    [InlineData(LandingPageBlockType.CustomHtml, 14)]
    [InlineData(LandingPageBlockType.Divider, 15)]
    [InlineData(LandingPageBlockType.Header, 16)]
    [InlineData(LandingPageBlockType.Footer, 17)]
    public void LandingPageBlockType_ShouldHaveCorrectValues(LandingPageBlockType blockType, int expected)
    {
        ((int)blockType).Should().Be(expected);
    }

    [Fact]
    public void LandingPageBlockType_ShouldHave18Values()
    {
        var values = Enum.GetValues(typeof(LandingPageBlockType));
        values.Length.Should().Be(18);
    }
}

#endregion

#region FormDefinition Entity Tests

public class FormDefinitionEntityTests
{
    [Fact]
    public void FormDefinition_ShouldInitializeWithDefaults()
    {
        var form = new FormDefinition();

        form.Name.Should().Be(string.Empty);
        form.FormKey.Should().Be(string.Empty);
        form.Description.Should().BeNull();
        form.Status.Should().Be(FormStatus.Draft);
        form.Title.Should().BeNull();
        form.Subtitle.Should().BeNull();
        form.SubmitButtonText.Should().Be("Submit");
        form.SubmitAction.Should().Be(FormSubmitAction.ShowMessage);
        form.DoubleOptIn.Should().BeFalse();
        form.SpamProtection.Should().BeTrue();
        form.CreateLead.Should().BeTrue();
        form.UpdateExistingLead.Should().BeTrue();
        form.ExistingLeadMatchField.Should().Be("Email");
        form.NotifyOwner.Should().BeTrue();
        form.SendAutoresponder.Should().BeFalse();
        form.TotalViews.Should().Be(0);
        form.TotalSubmissions.Should().Be(0);
        form.Fields.Should().BeEmpty();
        form.Submissions.Should().BeEmpty();
    }

    [Fact]
    public void FormDefinition_ConversionRate_ShouldCalculateCorrectly()
    {
        var form = new FormDefinition
        {
            TotalViews = 1000,
            TotalSubmissions = 50
        };

        form.ConversionRate.Should().Be(5.0m);
    }

    [Fact]
    public void FormDefinition_ConversionRate_ShouldBeZeroWhenNoViews()
    {
        var form = new FormDefinition
        {
            TotalViews = 0,
            TotalSubmissions = 0
        };

        form.ConversionRate.Should().Be(0);
    }

    [Fact]
    public void FormDefinition_ShouldSetFormProperties()
    {
        var form = new FormDefinition
        {
            Name = "Contact Us Form",
            FormKey = "contact-us",
            Title = "Get in Touch",
            Status = FormStatus.Published,
            SubmitAction = FormSubmitAction.Redirect,
            RedirectUrl = "https://example.com/thank-you",
            LeadSource = "Website",
            CampaignId = 1
        };

        form.Name.Should().Be("Contact Us Form");
        form.FormKey.Should().Be("contact-us");
        form.Title.Should().Be("Get in Touch");
        form.Status.Should().Be(FormStatus.Published);
        form.SubmitAction.Should().Be(FormSubmitAction.Redirect);
        form.RedirectUrl.Should().Be("https://example.com/thank-you");
        form.LeadSource.Should().Be("Website");
        form.CampaignId.Should().Be(1);
    }

    [Fact]
    public void FormDefinition_ShouldInheritFromBaseEntity()
    {
        var form = new FormDefinition();

        form.Should().BeAssignableTo<BaseEntity>();
        form.Id.Should().Be(0);
        form.IsDeleted.Should().BeFalse();
    }
}

#endregion

#region FormField Entity Tests

public class FormFieldEntityTests
{
    [Fact]
    public void FormField_ShouldInitializeWithDefaults()
    {
        var field = new FormField();

        field.FieldName.Should().Be(string.Empty);
        field.Label.Should().Be(string.Empty);
        field.FieldType.Should().Be(FormFieldType.Text);
        field.Order.Should().Be(0);
        field.IsRequired.Should().BeFalse();
        field.Width.Should().Be("full");
        field.IsHidden.Should().BeFalse();
        field.IsReadOnly.Should().BeFalse();
        field.AllowOther.Should().BeFalse();
        field.HasConditionalLogic.Should().BeFalse();
    }

    [Fact]
    public void FormField_ShouldSetFieldProperties()
    {
        var field = new FormField
        {
            FieldName = "email",
            Label = "Email Address",
            FieldType = FormFieldType.Email,
            IsRequired = true,
            RequiredMessage = "Email is required",
            Placeholder = "Enter your email",
            CrmFieldMapping = "Lead.Email",
            CrmEntityMapping = "Lead"
        };

        field.FieldName.Should().Be("email");
        field.Label.Should().Be("Email Address");
        field.FieldType.Should().Be(FormFieldType.Email);
        field.IsRequired.Should().BeTrue();
        field.RequiredMessage.Should().Be("Email is required");
        field.Placeholder.Should().Be("Enter your email");
        field.CrmFieldMapping.Should().Be("Lead.Email");
        field.CrmEntityMapping.Should().Be("Lead");
    }

    [Fact]
    public void FormField_ShouldSetValidationProperties()
    {
        var field = new FormField
        {
            MinLength = 5,
            MaxLength = 100,
            MinValue = 1,
            MaxValue = 1000,
            ValidationPattern = @"^\d{5}$",
            ValidationMessage = "Must be a 5-digit number"
        };

        field.MinLength.Should().Be(5);
        field.MaxLength.Should().Be(100);
        field.MinValue.Should().Be(1);
        field.MaxValue.Should().Be(1000);
        field.ValidationPattern.Should().Be(@"^\d{5}$");
        field.ValidationMessage.Should().Be("Must be a 5-digit number");
    }

    [Fact]
    public void FormField_ShouldInheritFromBaseEntity()
    {
        var field = new FormField();

        field.Should().BeAssignableTo<BaseEntity>();
        field.Id.Should().Be(0);
    }
}

#endregion

#region FormSubmission Entity Tests

public class FormSubmissionEntityTests
{
    [Fact]
    public void FormSubmission_ShouldInitializeWithDefaults()
    {
        var submission = new FormSubmission();

        submission.SubmissionNumber.Should().Be(string.Empty);
        submission.Status.Should().Be(SubmissionStatus.New);
        submission.FormData.Should().Be("{}");
        submission.OptInConfirmed.Should().BeFalse();
        submission.IsSpam.Should().BeFalse();
    }

    [Fact]
    public void FormSubmission_ShouldSetSubmissionProperties()
    {
        var submittedAt = DateTime.UtcNow;
        var submission = new FormSubmission
        {
            SubmissionNumber = "SUB-2026-001",
            SubmittedAt = submittedAt,
            Status = SubmissionStatus.LeadCreated,
            FormData = "{\"email\":\"test@example.com\"}",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            LeadId = 123
        };

        submission.SubmissionNumber.Should().Be("SUB-2026-001");
        submission.SubmittedAt.Should().Be(submittedAt);
        submission.Status.Should().Be(SubmissionStatus.LeadCreated);
        submission.FormData.Should().Be("{\"email\":\"test@example.com\"}");
        submission.IpAddress.Should().Be("192.168.1.1");
        submission.LeadId.Should().Be(123);
    }

    [Fact]
    public void FormSubmission_ShouldSetUtmParameters()
    {
        var submission = new FormSubmission
        {
            UtmSource = "google",
            UtmMedium = "cpc",
            UtmCampaign = "summer2026",
            UtmContent = "ad1",
            UtmTerm = "crm software"
        };

        submission.UtmSource.Should().Be("google");
        submission.UtmMedium.Should().Be("cpc");
        submission.UtmCampaign.Should().Be("summer2026");
        submission.UtmContent.Should().Be("ad1");
        submission.UtmTerm.Should().Be("crm software");
    }

    [Fact]
    public void FormSubmission_ShouldInheritFromBaseEntity()
    {
        var submission = new FormSubmission();

        submission.Should().BeAssignableTo<BaseEntity>();
        submission.Id.Should().Be(0);
    }
}

#endregion

#region LandingPage Entity Tests

public class LandingPageEntityTests
{
    [Fact]
    public void LandingPage_ShouldInitializeWithDefaults()
    {
        var page = new LandingPage();

        page.Name.Should().Be(string.Empty);
        page.Slug.Should().Be(string.Empty);
        page.Template.Should().Be(LandingPageTemplate.Blank);
        page.Status.Should().Be(LandingPageStatus.Draft);
        page.IsActive.Should().BeTrue();
        page.PageViews.Should().Be(0);
        page.UniqueVisitors.Should().Be(0);
        page.Conversions.Should().Be(0);
        page.AverageTimeOnPage.Should().Be(0);
        page.BounceRate.Should().Be(0);
        page.Blocks.Should().BeEmpty();
        page.Visits.Should().BeEmpty();
    }

    [Fact]
    public void LandingPage_ConversionRate_ShouldCalculateCorrectly()
    {
        var page = new LandingPage
        {
            UniqueVisitors = 500,
            Conversions = 25
        };

        page.ConversionRate.Should().Be(5.0m);
    }

    [Fact]
    public void LandingPage_ConversionRate_ShouldBeZeroWhenNoVisitors()
    {
        var page = new LandingPage
        {
            UniqueVisitors = 0,
            Conversions = 0
        };

        page.ConversionRate.Should().Be(0);
    }

    [Fact]
    public void LandingPage_ShouldSetPageProperties()
    {
        var page = new LandingPage
        {
            Name = "Summer Sale 2026",
            Slug = "summer-sale-2026",
            Title = "Summer Sale - 50% Off",
            MetaDescription = "Get 50% off all products this summer",
            Template = LandingPageTemplate.ProductShowcase,
            Status = LandingPageStatus.Published,
            CampaignId = 5
        };

        page.Name.Should().Be("Summer Sale 2026");
        page.Slug.Should().Be("summer-sale-2026");
        page.Title.Should().Be("Summer Sale - 50% Off");
        page.MetaDescription.Should().Be("Get 50% off all products this summer");
        page.Template.Should().Be(LandingPageTemplate.ProductShowcase);
        page.Status.Should().Be(LandingPageStatus.Published);
        page.CampaignId.Should().Be(5);
    }

    [Fact]
    public void LandingPage_ShouldSetABTestingProperties()
    {
        var page = new LandingPage
        {
            ABTestVariant = "B",
            OriginalPageId = 100,
            ABTestTrafficPercentage = 50
        };

        page.ABTestVariant.Should().Be("B");
        page.OriginalPageId.Should().Be(100);
        page.ABTestTrafficPercentage.Should().Be(50);
    }

    [Fact]
    public void LandingPage_ShouldSetTrackingProperties()
    {
        var page = new LandingPage
        {
            FacebookPixelId = "123456789",
            GoogleAnalyticsId = "UA-12345678-1"
        };

        page.FacebookPixelId.Should().Be("123456789");
        page.GoogleAnalyticsId.Should().Be("UA-12345678-1");
    }

    [Fact]
    public void LandingPage_ShouldInheritFromBaseEntity()
    {
        var page = new LandingPage();

        page.Should().BeAssignableTo<BaseEntity>();
        page.Id.Should().Be(0);
        page.IsDeleted.Should().BeFalse();
    }
}

#endregion

#region LandingPageBlock Entity Tests

public class LandingPageBlockEntityTests
{
    [Fact]
    public void LandingPageBlock_ShouldInitializeWithDefaults()
    {
        var block = new LandingPageBlock();

        block.BlockType.Should().Be(LandingPageBlockType.Hero);
        block.SortOrder.Should().Be(0);
        block.IsVisible.Should().BeTrue();
        block.ContentJson.Should().BeNull();
        block.StyleJson.Should().BeNull();
    }

    [Fact]
    public void LandingPageBlock_ShouldSetBlockProperties()
    {
        var block = new LandingPageBlock
        {
            LandingPageId = 10,
            BlockType = LandingPageBlockType.Form,
            SortOrder = 3,
            ContentJson = "{\"formId\": 5}",
            StyleJson = "{\"backgroundColor\": \"#ffffff\"}",
            VisibilityCondition = "desktop-only",
            IsVisible = true
        };

        block.LandingPageId.Should().Be(10);
        block.BlockType.Should().Be(LandingPageBlockType.Form);
        block.SortOrder.Should().Be(3);
        block.ContentJson.Should().Be("{\"formId\": 5}");
        block.StyleJson.Should().Be("{\"backgroundColor\": \"#ffffff\"}");
        block.VisibilityCondition.Should().Be("desktop-only");
    }

    [Fact]
    public void LandingPageBlock_ShouldInheritFromBaseEntity()
    {
        var block = new LandingPageBlock();

        block.Should().BeAssignableTo<BaseEntity>();
        block.Id.Should().Be(0);
    }
}

#endregion

#region LandingPageVisit Entity Tests

public class LandingPageVisitEntityTests
{
    [Fact]
    public void LandingPageVisit_ShouldInitializeWithDefaults()
    {
        var visit = new LandingPageVisit();

        visit.Converted.Should().BeFalse();
        visit.VisitedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void LandingPageVisit_ShouldSetVisitProperties()
    {
        var visitedAt = DateTime.UtcNow.AddMinutes(-30);
        var visit = new LandingPageVisit
        {
            LandingPageId = 5,
            VisitorId = "visitor-123",
            IpAddressHash = "abc123hash",
            UserAgent = "Mozilla/5.0",
            Referrer = "https://google.com",
            VisitedAt = visitedAt,
            TimeOnPageSeconds = 120,
            DeviceType = "mobile",
            Browser = "Chrome",
            OperatingSystem = "iOS"
        };

        visit.LandingPageId.Should().Be(5);
        visit.VisitorId.Should().Be("visitor-123");
        visit.IpAddressHash.Should().Be("abc123hash");
        visit.UserAgent.Should().Be("Mozilla/5.0");
        visit.Referrer.Should().Be("https://google.com");
        visit.VisitedAt.Should().Be(visitedAt);
        visit.TimeOnPageSeconds.Should().Be(120);
        visit.DeviceType.Should().Be("mobile");
        visit.Browser.Should().Be("Chrome");
        visit.OperatingSystem.Should().Be("iOS");
    }

    [Fact]
    public void LandingPageVisit_ShouldSetUtmParameters()
    {
        var visit = new LandingPageVisit
        {
            UtmSource = "facebook",
            UtmMedium = "social",
            UtmCampaign = "brand",
            UtmTerm = "crm",
            UtmContent = "image-ad"
        };

        visit.UtmSource.Should().Be("facebook");
        visit.UtmMedium.Should().Be("social");
        visit.UtmCampaign.Should().Be("brand");
        visit.UtmTerm.Should().Be("crm");
        visit.UtmContent.Should().Be("image-ad");
    }

    [Fact]
    public void LandingPageVisit_ShouldSetConversionProperties()
    {
        var convertedAt = DateTime.UtcNow;
        var visit = new LandingPageVisit
        {
            Converted = true,
            ConvertedAt = convertedAt,
            LeadId = 999
        };

        visit.Converted.Should().BeTrue();
        visit.ConvertedAt.Should().Be(convertedAt);
        visit.LeadId.Should().Be(999);
    }

    [Fact]
    public void LandingPageVisit_ShouldSetGeolocationProperties()
    {
        var visit = new LandingPageVisit
        {
            Country = "United States",
            City = "San Francisco"
        };

        visit.Country.Should().Be("United States");
        visit.City.Should().Be("San Francisco");
    }

    [Fact]
    public void LandingPageVisit_ShouldInheritFromBaseEntity()
    {
        var visit = new LandingPageVisit();

        visit.Should().BeAssignableTo<BaseEntity>();
        visit.Id.Should().Be(0);
    }
}

#endregion
