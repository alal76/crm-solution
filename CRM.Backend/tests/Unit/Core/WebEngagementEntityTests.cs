// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Comprehensive unit tests for Web Engagement and Data Collection entities:
/// - Form Definitions (FormDefinition, FormField, FormSubmission, enums)
/// - Web Visitor Tracking (WebVisitor, WebSession, WebPageView, enums)
/// - Interactions (Interaction, enums)
/// - OAuth Tokens (OAuthToken)
/// </summary>
public class WebEngagementEntityTests
{
    #region FormFieldType Enum Tests

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
    public void FormFieldType_ShouldHaveCorrectValue(FormFieldType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void FormFieldType_ShouldHave23Values()
    {
        Enum.GetValues<FormFieldType>().Should().HaveCount(23);
    }

    #endregion

    #region FormStatus Enum Tests

    [Theory]
    [InlineData(FormStatus.Draft, 0)]
    [InlineData(FormStatus.Published, 1)]
    [InlineData(FormStatus.Paused, 2)]
    [InlineData(FormStatus.Archived, 3)]
    public void FormStatus_ShouldHaveCorrectValue(FormStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void FormStatus_ShouldHave4Values()
    {
        Enum.GetValues<FormStatus>().Should().HaveCount(4);
    }

    #endregion

    #region FormSubmitAction Enum Tests

    [Theory]
    [InlineData(FormSubmitAction.ShowMessage, 0)]
    [InlineData(FormSubmitAction.Redirect, 1)]
    [InlineData(FormSubmitAction.ShowForm, 2)]
    [InlineData(FormSubmitAction.StayOnPage, 3)]
    public void FormSubmitAction_ShouldHaveCorrectValue(FormSubmitAction action, int expectedValue)
    {
        ((int)action).Should().Be(expectedValue);
    }

    [Fact]
    public void FormSubmitAction_ShouldHave4Values()
    {
        Enum.GetValues<FormSubmitAction>().Should().HaveCount(4);
    }

    #endregion

    #region SubmissionStatus Enum Tests

    [Theory]
    [InlineData(SubmissionStatus.New, 0)]
    [InlineData(SubmissionStatus.Processing, 1)]
    [InlineData(SubmissionStatus.LeadCreated, 2)]
    [InlineData(SubmissionStatus.ContactCreated, 3)]
    [InlineData(SubmissionStatus.SubmittedExternal, 4)]
    [InlineData(SubmissionStatus.Failed, 5)]
    [InlineData(SubmissionStatus.Spam, 6)]
    [InlineData(SubmissionStatus.Duplicate, 7)]
    public void SubmissionStatus_ShouldHaveCorrectValue(SubmissionStatus status, int expectedValue)
    {
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void SubmissionStatus_ShouldHave8Values()
    {
        Enum.GetValues<SubmissionStatus>().Should().HaveCount(8);
    }

    #endregion

    #region FormDefinition Entity Tests

    [Fact]
    public void FormDefinition_ShouldInitializeWithDefaults()
    {
        // Act
        var form = new FormDefinition();

        // Assert
        form.Name.Should().BeEmpty();
        form.FormKey.Should().BeEmpty();
        form.Status.Should().Be(FormStatus.Draft);
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
        form.Fields.Should().NotBeNull().And.BeEmpty();
        form.Submissions.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void FormDefinition_ShouldSetPropertiesCorrectly()
    {
        // Act
        var form = new FormDefinition
        {
            Name = "Contact Us Form",
            FormKey = "contact-us",
            Description = "Main contact form for website",
            Status = FormStatus.Published,
            Title = "Get in Touch",
            Subtitle = "We'd love to hear from you",
            SubmitButtonText = "Send Message",
            Width = "600px",
            Theme = "modern",
            SubmitAction = FormSubmitAction.Redirect,
            ThankYouMessage = "Thank you for contacting us!",
            RedirectUrl = "https://example.com/thank-you",
            CreateLead = true,
            LeadSource = "Website Contact Form",
            NotifyOwner = true,
            TotalViews = 1500,
            TotalSubmissions = 120
        };

        // Assert
        form.Name.Should().Be("Contact Us Form");
        form.FormKey.Should().Be("contact-us");
        form.Status.Should().Be(FormStatus.Published);
        form.SubmitAction.Should().Be(FormSubmitAction.Redirect);
        form.LeadSource.Should().Be("Website Contact Form");
        form.TotalViews.Should().Be(1500);
        form.TotalSubmissions.Should().Be(120);
    }

    [Fact]
    public void FormDefinition_ConversionRate_ShouldCalculateCorrectly()
    {
        // Act
        var form = new FormDefinition
        {
            TotalViews = 1000,
            TotalSubmissions = 50
        };

        // Assert
        form.ConversionRate.Should().Be(5m); // 50/1000 * 100 = 5%
    }

    [Fact]
    public void FormDefinition_ConversionRate_ShouldReturnZeroWhenNoViews()
    {
        // Act
        var form = new FormDefinition
        {
            TotalViews = 0,
            TotalSubmissions = 0
        };

        // Assert
        form.ConversionRate.Should().Be(0);
    }

    #endregion

    #region FormField Entity Tests

    [Fact]
    public void FormField_ShouldInitializeWithDefaults()
    {
        // Act
        var field = new FormField();

        // Assert
        field.FieldName.Should().BeEmpty();
        field.Label.Should().BeEmpty();
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
    public void FormField_ShouldSetPropertiesCorrectly()
    {
        // Act
        var field = new FormField
        {
            FieldName = "email",
            Label = "Email Address",
            FieldType = FormFieldType.Email,
            Order = 2,
            IsRequired = true,
            RequiredMessage = "Email is required",
            Placeholder = "you@example.com",
            HelpText = "We'll never share your email",
            CrmFieldMapping = "Lead.Email",
            CrmEntityMapping = "Lead",
            FormDefinitionId = 1
        };

        // Assert
        field.FieldName.Should().Be("email");
        field.FieldType.Should().Be(FormFieldType.Email);
        field.IsRequired.Should().BeTrue();
        field.CrmFieldMapping.Should().Be("Lead.Email");
        field.CrmEntityMapping.Should().Be("Lead");
    }

    [Fact]
    public void FormField_ShouldSupportValidation()
    {
        // Act
        var field = new FormField
        {
            FieldName = "phone",
            FieldType = FormFieldType.Phone,
            MinLength = 10,
            MaxLength = 15,
            ValidationPattern = @"^\+?[\d\s-]+$",
            ValidationMessage = "Please enter a valid phone number"
        };

        // Assert
        field.MinLength.Should().Be(10);
        field.MaxLength.Should().Be(15);
        field.ValidationPattern.Should().NotBeEmpty();
        field.ValidationMessage.Should().Contain("valid phone number");
    }

    #endregion

    #region FormSubmission Entity Tests

    [Fact]
    public void FormSubmission_ShouldInitializeWithDefaults()
    {
        // Act
        var submission = new FormSubmission();

        // Assert
        submission.SubmissionNumber.Should().BeEmpty();
        submission.Status.Should().Be(SubmissionStatus.New);
        submission.FormData.Should().Be("{}");
        submission.OptInConfirmed.Should().BeFalse();
        submission.IsSpam.Should().BeFalse();
    }

    [Fact]
    public void FormSubmission_ShouldSetPropertiesCorrectly()
    {
        // Act
        var submission = new FormSubmission
        {
            SubmissionNumber = "SUB-2025-00001",
            SubmittedAt = DateTime.UtcNow,
            Status = SubmissionStatus.LeadCreated,
            FormData = "{\"name\":\"John\",\"email\":\"john@example.com\"}",
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            Referrer = "https://google.com",
            UtmSource = "google",
            UtmMedium = "cpc",
            UtmCampaign = "spring-sale",
            ProcessedAt = DateTime.UtcNow,
            LeadId = 123,
            FormDefinitionId = 1
        };

        // Assert
        submission.SubmissionNumber.Should().StartWith("SUB-");
        submission.Status.Should().Be(SubmissionStatus.LeadCreated);
        submission.FormData.Should().Contain("john@example.com");
        submission.UtmSource.Should().Be("google");
        submission.LeadId.Should().Be(123);
    }

    #endregion

    #region VisitorIdentificationSource Enum Tests

    [Theory]
    [InlineData(VisitorIdentificationSource.Anonymous, 0)]
    [InlineData(VisitorIdentificationSource.FormSubmission, 1)]
    [InlineData(VisitorIdentificationSource.EmailClick, 2)]
    [InlineData(VisitorIdentificationSource.Login, 3)]
    [InlineData(VisitorIdentificationSource.Chat, 4)]
    [InlineData(VisitorIdentificationSource.Cookie, 5)]
    [InlineData(VisitorIdentificationSource.CompanyLookup, 6)]
    [InlineData(VisitorIdentificationSource.Social, 7)]
    [InlineData(VisitorIdentificationSource.Manual, 8)]
    public void VisitorIdentificationSource_ShouldHaveCorrectValue(VisitorIdentificationSource source, int expectedValue)
    {
        ((int)source).Should().Be(expectedValue);
    }

    [Fact]
    public void VisitorIdentificationSource_ShouldHave9Values()
    {
        Enum.GetValues<VisitorIdentificationSource>().Should().HaveCount(9);
    }

    #endregion

    #region PageCategory Enum Tests

    [Theory]
    [InlineData(PageCategory.Home, 0)]
    [InlineData(PageCategory.Product, 1)]
    [InlineData(PageCategory.Pricing, 2)]
    [InlineData(PageCategory.Features, 3)]
    [InlineData(PageCategory.Blog, 4)]
    [InlineData(PageCategory.CaseStudy, 5)]
    [InlineData(PageCategory.Documentation, 6)]
    [InlineData(PageCategory.Demo, 7)]
    [InlineData(PageCategory.Contact, 8)]
    [InlineData(PageCategory.About, 9)]
    [InlineData(PageCategory.Careers, 10)]
    [InlineData(PageCategory.ThankYou, 11)]
    [InlineData(PageCategory.Other, 12)]
    public void PageCategory_ShouldHaveCorrectValue(PageCategory category, int expectedValue)
    {
        ((int)category).Should().Be(expectedValue);
    }

    [Fact]
    public void PageCategory_ShouldHave13Values()
    {
        Enum.GetValues<PageCategory>().Should().HaveCount(13);
    }

    #endregion

    #region WebVisitor Entity Tests

    [Fact]
    public void WebVisitor_ShouldInitializeWithDefaults()
    {
        // Act
        var visitor = new WebVisitor();

        // Assert
        visitor.VisitorId.Should().BeEmpty();
        visitor.IsIdentified.Should().BeFalse();
        visitor.IdentificationSource.Should().Be(VisitorIdentificationSource.Anonymous);
        visitor.TotalSessions.Should().Be(1);
        visitor.TotalPageViews.Should().Be(0);
        visitor.TotalTimeOnSite.Should().Be(0);
        visitor.FormsSubmitted.Should().Be(0);
        visitor.FilesDownloaded.Should().Be(0);
        visitor.VideosWatched.Should().Be(0);
        visitor.BehaviorScore.Should().Be(0);
        visitor.FitScore.Should().Be(0);
        visitor.TotalScore.Should().Be(0);
    }

    [Fact]
    public void WebVisitor_ShouldSetPropertiesCorrectly()
    {
        // Act
        var visitor = new WebVisitor
        {
            VisitorId = "vis_abc123",
            FingerprintId = "fp_xyz789",
            IsIdentified = true,
            IdentificationSource = VisitorIdentificationSource.FormSubmission,
            IdentifiedAt = DateTime.UtcNow,
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe",
            Company = "Acme Inc",
            Industry = "Technology",
            IpAddress = "192.168.1.100",
            Country = "United States",
            CountryCode = "US",
            City = "San Francisco",
            Browser = "Chrome",
            DeviceType = "desktop",
            TotalSessions = 5,
            TotalPageViews = 25,
            TotalTimeOnSite = 1800,
            BehaviorScore = 45,
            FitScore = 60,
            TotalScore = 105,
            LeadId = 123
        };

        // Assert
        visitor.VisitorId.Should().Be("vis_abc123");
        visitor.IsIdentified.Should().BeTrue();
        visitor.IdentificationSource.Should().Be(VisitorIdentificationSource.FormSubmission);
        visitor.Email.Should().Be("john@example.com");
        visitor.Company.Should().Be("Acme Inc");
        visitor.TotalSessions.Should().Be(5);
        visitor.TotalScore.Should().Be(105);
    }

    [Fact]
    public void WebVisitor_AveragePagePerSession_ShouldCalculateCorrectly()
    {
        // Act
        var visitor = new WebVisitor
        {
            TotalSessions = 4,
            TotalPageViews = 20
        };

        // Assert
        visitor.AveragePagePerSession.Should().Be(5m); // 20/4 = 5
    }

    [Fact]
    public void WebVisitor_AveragePagePerSession_ShouldReturnZeroWhenNoSessions()
    {
        // Act
        var visitor = new WebVisitor
        {
            TotalSessions = 0,
            TotalPageViews = 0
        };

        // Assert
        visitor.AveragePagePerSession.Should().Be(0);
    }

    #endregion

    #region WebSession Entity Tests

    [Fact]
    public void WebSession_ShouldInitializeWithDefaults()
    {
        // Act
        var session = new WebSession();

        // Assert
        session.SessionId.Should().BeEmpty();
        session.Duration.Should().Be(0);
        session.PageViewCount.Should().Be(0);
        session.PageViews.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void WebSession_ShouldSetPropertiesCorrectly()
    {
        // Act
        var session = new WebSession
        {
            SessionId = "sess_abc123",
            StartedAt = DateTime.UtcNow.AddMinutes(-30),
            EndedAt = DateTime.UtcNow,
            Duration = 1800,
            PageViewCount = 8,
            LandingPage = "/products",
            ExitPage = "/pricing",
            Referrer = "https://google.com",
            UtmParameters = "{\"source\":\"google\",\"medium\":\"organic\"}",
            IpAddress = "192.168.1.100",
            WebVisitorId = 1
        };

        // Assert
        session.SessionId.Should().Be("sess_abc123");
        session.Duration.Should().Be(1800);
        session.PageViewCount.Should().Be(8);
        session.LandingPage.Should().Be("/products");
        session.ExitPage.Should().Be("/pricing");
    }

    #endregion

    #region WebPageView Entity Tests

    [Fact]
    public void WebPageView_ShouldInitializeWithDefaults()
    {
        // Act
        var pageView = new WebPageView();

        // Assert
        pageView.PageUrl.Should().BeEmpty();
        pageView.Category.Should().Be(PageCategory.Other);
        pageView.TimeOnPage.Should().Be(0);
    }

    [Fact]
    public void WebPageView_ShouldSetPropertiesCorrectly()
    {
        // Act
        var pageView = new WebPageView
        {
            PageUrl = "https://example.com/products/crm",
            PagePath = "/products/crm",
            PageTitle = "CRM Product Features",
            Category = PageCategory.Product,
            ViewedAt = DateTime.UtcNow,
            TimeOnPage = 120,
            ScrollDepth = 75,
            Referrer = "/home",
            QueryParameters = "{\"ref\":\"banner\"}",
            WebVisitorId = 1,
            WebSessionId = 1
        };

        // Assert
        pageView.PageUrl.Should().Contain("products/crm");
        pageView.Category.Should().Be(PageCategory.Product);
        pageView.TimeOnPage.Should().Be(120);
        pageView.ScrollDepth.Should().Be(75);
    }

    #endregion

    #region InteractionType Enum Tests

    [Theory]
    [InlineData(InteractionType.Email, 0)]
    [InlineData(InteractionType.Phone, 1)]
    [InlineData(InteractionType.Meeting, 2)]
    [InlineData(InteractionType.VideoCall, 3)]
    [InlineData(InteractionType.Chat, 4)]
    [InlineData(InteractionType.SMS, 5)]
    [InlineData(InteractionType.SocialMedia, 6)]
    [InlineData(InteractionType.InPerson, 7)]
    [InlineData(InteractionType.WebForm, 8)]
    [InlineData(InteractionType.Note, 9)]
    [InlineData(InteractionType.Task, 10)]
    [InlineData(InteractionType.Demo, 11)]
    [InlineData(InteractionType.Presentation, 12)]
    [InlineData(InteractionType.Contract, 13)]
    [InlineData(InteractionType.Support, 14)]
    [InlineData(InteractionType.Other, 15)]
    public void InteractionType_ShouldHaveCorrectValue(InteractionType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Fact]
    public void InteractionType_ShouldHave16Values()
    {
        Enum.GetValues<InteractionType>().Should().HaveCount(16);
    }

    #endregion

    #region InteractionDirection Enum Tests

    [Theory]
    [InlineData(InteractionDirection.Inbound, 0)]
    [InlineData(InteractionDirection.Outbound, 1)]
    [InlineData(InteractionDirection.Internal, 2)]
    public void InteractionDirection_ShouldHaveCorrectValue(InteractionDirection direction, int expectedValue)
    {
        ((int)direction).Should().Be(expectedValue);
    }

    [Fact]
    public void InteractionDirection_ShouldHave3Values()
    {
        Enum.GetValues<InteractionDirection>().Should().HaveCount(3);
    }

    #endregion

    #region InteractionOutcome Enum Tests

    [Theory]
    [InlineData(InteractionOutcome.None, 0)]
    [InlineData(InteractionOutcome.Successful, 1)]
    [InlineData(InteractionOutcome.Unsuccessful, 2)]
    [InlineData(InteractionOutcome.FollowUpRequired, 3)]
    [InlineData(InteractionOutcome.NoResponse, 4)]
    [InlineData(InteractionOutcome.Voicemail, 5)]
    [InlineData(InteractionOutcome.Rescheduled, 6)]
    [InlineData(InteractionOutcome.Cancelled, 7)]
    public void InteractionOutcome_ShouldHaveCorrectValue(InteractionOutcome outcome, int expectedValue)
    {
        ((int)outcome).Should().Be(expectedValue);
    }

    [Fact]
    public void InteractionOutcome_ShouldHave8Values()
    {
        Enum.GetValues<InteractionOutcome>().Should().HaveCount(8);
    }

    #endregion

    #region InteractionSentiment Enum Tests

    [Theory]
    [InlineData(InteractionSentiment.VeryNegative, 0)]
    [InlineData(InteractionSentiment.Negative, 1)]
    [InlineData(InteractionSentiment.Neutral, 2)]
    [InlineData(InteractionSentiment.Positive, 3)]
    [InlineData(InteractionSentiment.VeryPositive, 4)]
    public void InteractionSentiment_ShouldHaveCorrectValue(InteractionSentiment sentiment, int expectedValue)
    {
        ((int)sentiment).Should().Be(expectedValue);
    }

    [Fact]
    public void InteractionSentiment_ShouldHave5Values()
    {
        Enum.GetValues<InteractionSentiment>().Should().HaveCount(5);
    }

    #endregion

    #region Interaction Entity Tests

    [Fact]
    public void Interaction_ShouldInitializeWithDefaults()
    {
        // Act
        var interaction = new Interaction();

        // Assert
        interaction.InteractionType.Should().Be(InteractionType.Note);
        interaction.Type.Should().BeEmpty();
        interaction.Direction.Should().Be(InteractionDirection.Outbound);
        interaction.Subject.Should().BeEmpty();
        interaction.Description.Should().BeEmpty();
        interaction.Outcome.Should().Be(InteractionOutcome.None);
        interaction.Sentiment.Should().Be(InteractionSentiment.Neutral);
        interaction.IsCompleted.Should().BeFalse();
        interaction.IsPrivate.Should().BeFalse();
        interaction.Priority.Should().Be(1);
    }

    [Fact]
    public void Interaction_ShouldSetPropertiesCorrectly()
    {
        // Act
        var interaction = new Interaction
        {
            InteractionType = InteractionType.Meeting,
            Direction = InteractionDirection.Outbound,
            Subject = "Quarterly Business Review",
            Description = "Reviewed Q4 results and discussed Q1 goals",
            InteractionDate = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(1),
            DurationMinutes = 60,
            Outcome = InteractionOutcome.Successful,
            Sentiment = InteractionSentiment.Positive,
            IsCompleted = true,
            Priority = 2,
            Location = "Conference Room A",
            MeetingLink = "https://zoom.us/j/123456789",
            Attendees = "[\"john@example.com\",\"jane@example.com\"]",
            MeetingNotes = "Great discussion about upcoming projects",
            ActionItems = "[\"Send proposal\",\"Schedule follow-up\"]",
            AccountId = 1,
            ContactId = 2,
            OpportunityId = 3,
            AssignedToUserId = 100
        };

        // Assert
        interaction.InteractionType.Should().Be(InteractionType.Meeting);
        interaction.Subject.Should().Be("Quarterly Business Review");
        interaction.DurationMinutes.Should().Be(60);
        interaction.Outcome.Should().Be(InteractionOutcome.Successful);
        interaction.Sentiment.Should().Be(InteractionSentiment.Positive);
        interaction.IsCompleted.Should().BeTrue();
        interaction.MeetingLink.Should().Contain("zoom.us");
        interaction.AccountId.Should().Be(1);
    }

    [Fact]
    public void Interaction_ShouldSupportEmailInteraction()
    {
        // Act
        var email = new Interaction
        {
            InteractionType = InteractionType.Email,
            Direction = InteractionDirection.Outbound,
            Subject = "Follow-up on our meeting",
            EmailAddress = "client@example.com",
            EmailCc = "manager@company.com",
            EmailOpened = true,
            EmailOpenedDate = DateTime.UtcNow,
            EmailClicked = true,
            EmailClickedDate = DateTime.UtcNow
        };

        // Assert
        email.InteractionType.Should().Be(InteractionType.Email);
        email.EmailAddress.Should().Be("client@example.com");
        email.EmailOpened.Should().BeTrue();
        email.EmailClicked.Should().BeTrue();
    }

    [Fact]
    public void Interaction_ShouldSupportPhoneInteraction()
    {
        // Act
        var call = new Interaction
        {
            InteractionType = InteractionType.Phone,
            Direction = InteractionDirection.Inbound,
            Subject = "Support inquiry",
            PhoneNumber = "+1-555-0100",
            DurationMinutes = 15,
            CallDisposition = "Resolved",
            CallRecordingUrl = "https://recordings.example.com/call123",
            Outcome = InteractionOutcome.Successful
        };

        // Assert
        call.InteractionType.Should().Be(InteractionType.Phone);
        call.PhoneNumber.Should().Be("+1-555-0100");
        call.CallDisposition.Should().Be("Resolved");
        call.CallRecordingUrl.Should().NotBeEmpty();
    }

    [Fact]
    public void Interaction_ShouldSupportFollowUp()
    {
        // Act
        var interaction = new Interaction
        {
            Subject = "Initial consultation",
            FollowUpDate = DateTime.UtcNow.AddDays(7),
            FollowUpNotes = "Send proposal and schedule demo",
            Outcome = InteractionOutcome.FollowUpRequired
        };

        // Assert
        interaction.FollowUpDate.Should().BeAfter(DateTime.UtcNow);
        interaction.FollowUpNotes.Should().Contain("Send proposal");
        interaction.Outcome.Should().Be(InteractionOutcome.FollowUpRequired);
    }

    #endregion

    #region OAuthToken Entity Tests

    [Fact]
    public void OAuthToken_ShouldInitializeWithDefaults()
    {
        // Act
        var token = new OAuthToken();

        // Assert
        token.UserId.Should().Be(0);
        token.Provider.Should().BeEmpty();
        token.ProviderUserId.Should().BeEmpty();
        token.AccessToken.Should().BeEmpty();
    }

    [Fact]
    public void OAuthToken_ShouldSetPropertiesCorrectly()
    {
        // Act
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var token = new OAuthToken
        {
            UserId = 123,
            Provider = "Google",
            ProviderUserId = "google-user-123456",
            AccessToken = "ya29.access_token_here",
            RefreshToken = "1//refresh_token_here",
            ExpiresAt = expiresAt
        };

        // Assert
        token.UserId.Should().Be(123);
        token.Provider.Should().Be("Google");
        token.ProviderUserId.Should().Be("google-user-123456");
        token.AccessToken.Should().StartWith("ya29.");
        token.RefreshToken.Should().NotBeEmpty();
        token.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void OAuthToken_ShouldSupportDifferentProviders()
    {
        // Act
        var googleToken = new OAuthToken { Provider = "Google" };
        var microsoftToken = new OAuthToken { Provider = "Microsoft" };
        var githubToken = new OAuthToken { Provider = "GitHub" };
        var linkedinToken = new OAuthToken { Provider = "LinkedIn" };

        // Assert
        googleToken.Provider.Should().Be("Google");
        microsoftToken.Provider.Should().Be("Microsoft");
        githubToken.Provider.Should().Be("GitHub");
        linkedinToken.Provider.Should().Be("LinkedIn");
    }

    #endregion
}
