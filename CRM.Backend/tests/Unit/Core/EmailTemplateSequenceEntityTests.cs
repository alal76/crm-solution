// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Models;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for EmailTemplate and EmailSequence entities and related enums.
/// ~100 tests covering email template categories, sequences, steps, enrollments, and step executions.
/// </summary>
public class EmailTemplateSequenceEntityTests
{
    #region EmailTemplateCategory Enum Tests

    [Fact]
    public void EmailTemplateCategory_ShouldHaveCorrectValues()
    {
        // Assert - verify all enum values
        ((int)EmailTemplateCategory.General).Should().Be(0);
        ((int)EmailTemplateCategory.Sales).Should().Be(1);
        ((int)EmailTemplateCategory.Marketing).Should().Be(2);
        ((int)EmailTemplateCategory.Support).Should().Be(3);
        ((int)EmailTemplateCategory.Welcome).Should().Be(4);
        ((int)EmailTemplateCategory.FollowUp).Should().Be(5);
        ((int)EmailTemplateCategory.Newsletter).Should().Be(6);
        ((int)EmailTemplateCategory.Notification).Should().Be(7);
        ((int)EmailTemplateCategory.Transactional).Should().Be(8);
        ((int)EmailTemplateCategory.Custom).Should().Be(99);
    }

    [Fact]
    public void EmailTemplateCategory_ShouldHave13Values()
    {
        var values = Enum.GetValues<EmailTemplateCategory>();
        values.Should().HaveCount(13);
    }

    [Theory]
    [InlineData(EmailTemplateCategory.General, "General")]
    [InlineData(EmailTemplateCategory.Sales, "Sales")]
    [InlineData(EmailTemplateCategory.Marketing, "Marketing")]
    [InlineData(EmailTemplateCategory.Support, "Support")]
    [InlineData(EmailTemplateCategory.Welcome, "Welcome")]
    [InlineData(EmailTemplateCategory.FollowUp, "FollowUp")]
    [InlineData(EmailTemplateCategory.Newsletter, "Newsletter")]
    [InlineData(EmailTemplateCategory.Notification, "Notification")]
    [InlineData(EmailTemplateCategory.Transactional, "Transactional")]
    [InlineData(EmailTemplateCategory.Custom, "Custom")]
    public void EmailTemplateCategory_ShouldHaveCorrectStringRepresentation(EmailTemplateCategory category, string expected)
    {
        category.ToString().Should().Be(expected);
    }

    #endregion

    #region EmailSequenceStatus Enum Tests

    [Fact]
    public void EmailSequenceStatus_ShouldHaveCorrectValues()
    {
        ((int)EmailSequenceStatus.Draft).Should().Be(0);
        ((int)EmailSequenceStatus.Active).Should().Be(1);
        ((int)EmailSequenceStatus.Paused).Should().Be(2);
        ((int)EmailSequenceStatus.Archived).Should().Be(3);
    }

    [Fact]
    public void EmailSequenceStatus_ShouldHave4Values()
    {
        var values = Enum.GetValues<EmailSequenceStatus>();
        values.Should().HaveCount(4);
    }

    #endregion

    #region EmailStepType Enum Tests

    [Fact]
    public void EmailStepType_ShouldHaveCorrectValues()
    {
        ((int)EmailStepType.Email).Should().Be(0);
        ((int)EmailStepType.Wait).Should().Be(1);
        ((int)EmailStepType.Task).Should().Be(2);
        ((int)EmailStepType.Condition).Should().Be(3);
        ((int)EmailStepType.LinkedIn).Should().Be(4);
        ((int)EmailStepType.Call).Should().Be(5);
        ((int)EmailStepType.SMS).Should().Be(6);
        ((int)EmailStepType.Notification).Should().Be(7);
    }

    [Fact]
    public void EmailStepType_ShouldHave8Values()
    {
        var values = Enum.GetValues<EmailStepType>();
        values.Should().HaveCount(8);
    }

    #endregion

    #region StepTimingMode Enum Tests

    [Fact]
    public void StepTimingMode_ShouldHaveCorrectValues()
    {
        ((int)StepTimingMode.Delay).Should().Be(0);
        ((int)StepTimingMode.SpecificTime).Should().Be(1);
        ((int)StepTimingMode.BusinessHours).Should().Be(2);
        ((int)StepTimingMode.RecipientTimezone).Should().Be(3);
    }

    [Fact]
    public void StepTimingMode_ShouldHave4Values()
    {
        var values = Enum.GetValues<StepTimingMode>();
        values.Should().HaveCount(4);
    }

    #endregion

    #region EnrollmentStatus Enum Tests

    [Fact]
    public void EnrollmentStatus_ShouldHaveCorrectValues()
    {
        ((int)EnrollmentStatus.Active).Should().Be(0);
        ((int)EnrollmentStatus.Paused).Should().Be(1);
        ((int)EnrollmentStatus.Completed).Should().Be(2);
        ((int)EnrollmentStatus.Unsubscribed).Should().Be(3);
        ((int)EnrollmentStatus.Bounced).Should().Be(4);
        ((int)EnrollmentStatus.Replied).Should().Be(5);
        ((int)EnrollmentStatus.MeetingBooked).Should().Be(6);
        ((int)EnrollmentStatus.Converted).Should().Be(7);
        ((int)EnrollmentStatus.Removed).Should().Be(8);
        ((int)EnrollmentStatus.Error).Should().Be(9);
    }

    [Fact]
    public void EnrollmentStatus_ShouldHave10Values()
    {
        var values = Enum.GetValues<EnrollmentStatus>();
        values.Should().HaveCount(10);
    }

    #endregion

    #region SequenceExitCondition Enum Tests

    [Fact]
    public void SequenceExitCondition_ShouldHaveCorrectValues()
    {
        ((int)SequenceExitCondition.None).Should().Be(0);
        ((int)SequenceExitCondition.OnReply).Should().Be(1);
        ((int)SequenceExitCondition.OnMeetingBooked).Should().Be(2);
        ((int)SequenceExitCondition.OnOpportunityCreated).Should().Be(3);
        ((int)SequenceExitCondition.OnLinkClick).Should().Be(4);
        ((int)SequenceExitCondition.OnUnsubscribe).Should().Be(5);
        ((int)SequenceExitCondition.OnBounce).Should().Be(6);
        ((int)SequenceExitCondition.OnStatusChange).Should().Be(7);
    }

    [Fact]
    public void SequenceExitCondition_ShouldHave8Values()
    {
        var values = Enum.GetValues<SequenceExitCondition>();
        values.Should().HaveCount(8);
    }

    #endregion

    #region EmailTemplate Entity Tests

    [Fact]
    public void EmailTemplate_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var template = new EmailTemplate();

        // Assert
        template.Name.Should().BeEmpty();
        template.Subject.Should().BeEmpty();
        template.Category.Should().Be(EmailTemplateCategory.General);
        template.IsActive.Should().BeTrue();
        template.IsSystem.Should().BeFalse();
        template.UsageCount.Should().Be(0);
    }

    [Fact]
    public void EmailTemplate_ShouldAllowSettingProperties()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Id = 1,
            Name = "Welcome Email",
            Description = "Sent to new customers",
            Category = EmailTemplateCategory.Welcome,
            Subject = "Welcome to {{CompanyName}}!",
            PlainTextBody = "Hello {{FirstName}}, welcome!",
            HtmlBody = "<h1>Hello {{FirstName}}</h1><p>Welcome!</p>",
            IsActive = true,
            IsSystem = true,
            MergeFieldsJson = "[\"FirstName\", \"LastName\", \"CompanyName\"]",
            FromEmail = "welcome@company.com",
            FromName = "Company Support",
            ReplyToEmail = "support@company.com"
        };

        // Assert
        template.Id.Should().Be(1);
        template.Name.Should().Be("Welcome Email");
        template.Description.Should().Be("Sent to new customers");
        template.Category.Should().Be(EmailTemplateCategory.Welcome);
        template.Subject.Should().Contain("{{CompanyName}}");
        template.PlainTextBody.Should().Contain("{{FirstName}}");
        template.HtmlBody.Should().Contain("<h1>");
        template.IsActive.Should().BeTrue();
        template.IsSystem.Should().BeTrue();
        template.MergeFieldsJson.Should().Contain("FirstName");
        template.FromEmail.Should().Be("welcome@company.com");
        template.FromName.Should().Be("Company Support");
        template.ReplyToEmail.Should().Be("support@company.com");
    }

    [Fact]
    public void EmailTemplate_ShouldTrackUsageStatistics()
    {
        // Arrange
        var template = new EmailTemplate
        {
            UsageCount = 150,
            LastUsedAt = DateTime.UtcNow.AddDays(-1)
        };

        // Assert
        template.UsageCount.Should().Be(150);
        template.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void EmailTemplate_ShouldSupportMessagesCollection()
    {
        // Arrange
        var template = new EmailTemplate
        {
            Messages = new List<CommunicationMessage>()
        };

        // Assert
        template.Messages.Should().NotBeNull();
        template.Messages.Should().BeEmpty();
    }

    [Theory]
    [InlineData(EmailTemplateCategory.General)]
    [InlineData(EmailTemplateCategory.Sales)]
    [InlineData(EmailTemplateCategory.Marketing)]
    [InlineData(EmailTemplateCategory.Support)]
    [InlineData(EmailTemplateCategory.Welcome)]
    [InlineData(EmailTemplateCategory.FollowUp)]
    [InlineData(EmailTemplateCategory.Newsletter)]
    [InlineData(EmailTemplateCategory.Notification)]
    [InlineData(EmailTemplateCategory.Transactional)]
    [InlineData(EmailTemplateCategory.Custom)]
    public void EmailTemplate_ShouldAcceptAllCategories(EmailTemplateCategory category)
    {
        // Arrange & Act
        var template = new EmailTemplate { Category = category };

        // Assert
        template.Category.Should().Be(category);
    }

    #endregion

    #region EmailSequence Entity Tests

    [Fact]
    public void EmailSequence_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var sequence = new EmailSequence();

        // Assert
        sequence.Name.Should().BeEmpty();
        sequence.Status.Should().Be(EmailSequenceStatus.Draft);
        sequence.SendFromOwner.Should().BeTrue();
        sequence.Timezone.Should().Be("America/New_York");
        sequence.SendingStartHour.Should().Be(9);
        sequence.SendingEndHour.Should().Be(17);
        sequence.ExitOnReply.Should().BeTrue();
        sequence.ExitOnMeetingBooked.Should().BeTrue();
        sequence.ExitOnBounce.Should().BeTrue();
        sequence.ExitOnUnsubscribe.Should().BeTrue();
        sequence.TotalEnrolled.Should().Be(0);
        sequence.ActiveEnrollments.Should().Be(0);
        sequence.TotalCompleted.Should().Be(0);
        sequence.TotalEmailsSent.Should().Be(0);
        sequence.Steps.Should().BeEmpty();
        sequence.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public void EmailSequence_ShouldAllowSettingProperties()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            Id = 1,
            Name = "New Customer Onboarding",
            Description = "Automated onboarding sequence for new customers",
            Status = EmailSequenceStatus.Active,
            FromEmail = "onboarding@company.com",
            FromName = "Onboarding Team",
            ReplyToEmail = "support@company.com",
            SenderId = 100,
            SendFromOwner = false,
            Timezone = "America/Los_Angeles",
            SendingDays = "[1,2,3,4,5]",
            SendingStartHour = 8,
            SendingEndHour = 18,
            MaxEmailsPerDay = 3,
            ThrottleMinutes = 60
        };

        // Assert
        sequence.Id.Should().Be(1);
        sequence.Name.Should().Be("New Customer Onboarding");
        sequence.Description.Should().Contain("onboarding");
        sequence.Status.Should().Be(EmailSequenceStatus.Active);
        sequence.FromEmail.Should().Be("onboarding@company.com");
        sequence.FromName.Should().Be("Onboarding Team");
        sequence.ReplyToEmail.Should().Be("support@company.com");
        sequence.SenderId.Should().Be(100);
        sequence.SendFromOwner.Should().BeFalse();
        sequence.Timezone.Should().Be("America/Los_Angeles");
        sequence.SendingDays.Should().Be("[1,2,3,4,5]");
        sequence.SendingStartHour.Should().Be(8);
        sequence.SendingEndHour.Should().Be(18);
        sequence.MaxEmailsPerDay.Should().Be(3);
        sequence.ThrottleMinutes.Should().Be(60);
    }

    [Fact]
    public void EmailSequence_ShouldTrackStatistics()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            TotalEnrolled = 500,
            ActiveEnrollments = 150,
            TotalCompleted = 300,
            TotalEmailsSent = 1200,
            TotalOpens = 800,
            TotalClicks = 200,
            TotalReplies = 50,
            TotalBounces = 25,
            TotalUnsubscribes = 10,
            TotalMeetingsBooked = 15
        };

        // Assert
        sequence.TotalEnrolled.Should().Be(500);
        sequence.ActiveEnrollments.Should().Be(150);
        sequence.TotalCompleted.Should().Be(300);
        sequence.TotalEmailsSent.Should().Be(1200);
        sequence.TotalOpens.Should().Be(800);
        sequence.TotalClicks.Should().Be(200);
        sequence.TotalReplies.Should().Be(50);
        sequence.TotalBounces.Should().Be(25);
        sequence.TotalUnsubscribes.Should().Be(10);
        sequence.TotalMeetingsBooked.Should().Be(15);
    }

    [Fact]
    public void EmailSequence_ShouldSupportNavigationProperties()
    {
        // Arrange
        var owner = new User { Id = 1, FirstName = "Test" };
        var sender = new User { Id = 2, FirstName = "Sender" };
        var sequence = new EmailSequence
        {
            OwnerId = 1,
            Owner = owner,
            SenderId = 2,
            Sender = sender
        };

        // Assert
        sequence.OwnerId.Should().Be(1);
        sequence.Owner.Should().Be(owner);
        sequence.SenderId.Should().Be(2);
        sequence.Sender.Should().Be(sender);
    }

    [Fact]
    public void EmailSequence_ShouldSupportStepsCollection()
    {
        // Arrange
        var sequence = new EmailSequence();
        var step1 = new EmailSequenceStep { Id = 1, StepOrder = 1, Name = "Introduction" };
        var step2 = new EmailSequenceStep { Id = 2, StepOrder = 2, Name = "Follow-up" };

        // Act
        sequence.Steps.Add(step1);
        sequence.Steps.Add(step2);

        // Assert
        sequence.Steps.Should().HaveCount(2);
        sequence.Steps.First().StepOrder.Should().Be(1);
    }

    [Theory]
    [InlineData(EmailSequenceStatus.Draft)]
    [InlineData(EmailSequenceStatus.Active)]
    [InlineData(EmailSequenceStatus.Paused)]
    [InlineData(EmailSequenceStatus.Archived)]
    public void EmailSequence_ShouldAcceptAllStatuses(EmailSequenceStatus status)
    {
        // Arrange & Act
        var sequence = new EmailSequence { Status = status };

        // Assert
        sequence.Status.Should().Be(status);
    }

    #endregion

    #region EmailSequenceStep Entity Tests

    [Fact]
    public void EmailSequenceStep_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var step = new EmailSequenceStep();

        // Assert
        step.Name.Should().BeEmpty();
        step.StepType.Should().Be(EmailStepType.Email);
        step.IsActive.Should().BeTrue();
        step.TimingMode.Should().Be(StepTimingMode.Delay);
        step.DelayDays.Should().Be(1);
        step.DelayHours.Should().Be(0);
        step.DelayMinutes.Should().Be(0);
        step.IsReply.Should().BeFalse();
        step.IsABTest.Should().BeFalse();
        step.ExecutionCount.Should().Be(0);
        step.EmailsSent.Should().Be(0);
        step.Opens.Should().Be(0);
        step.Clicks.Should().Be(0);
        step.Replies.Should().Be(0);
        step.Bounces.Should().Be(0);
    }

    [Fact]
    public void EmailSequenceStep_ShouldAllowSettingEmailContent()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            Id = 1,
            StepOrder = 1,
            Name = "Welcome Email",
            StepType = EmailStepType.Email,
            Subject = "Welcome, {{FirstName}}!",
            Body = "<h1>Welcome!</h1>",
            BodyPlainText = "Welcome!",
            EmailTemplateId = 10,
            IsReply = false
        };

        // Assert
        step.Id.Should().Be(1);
        step.StepOrder.Should().Be(1);
        step.Name.Should().Be("Welcome Email");
        step.StepType.Should().Be(EmailStepType.Email);
        step.Subject.Should().Contain("{{FirstName}}");
        step.Body.Should().Contain("<h1>");
        step.BodyPlainText.Should().Be("Welcome!");
        step.EmailTemplateId.Should().Be(10);
    }

    [Fact]
    public void EmailSequenceStep_ShouldAllowSettingTimingConfiguration()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            TimingMode = StepTimingMode.BusinessHours,
            DelayDays = 2,
            DelayHours = 4,
            DelayMinutes = 30,
            SpecificTime = "09:00"
        };

        // Assert
        step.TimingMode.Should().Be(StepTimingMode.BusinessHours);
        step.DelayDays.Should().Be(2);
        step.DelayHours.Should().Be(4);
        step.DelayMinutes.Should().Be(30);
        step.SpecificTime.Should().Be("09:00");
    }

    [Fact]
    public void EmailSequenceStep_ShouldSupportTaskStepType()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            StepType = EmailStepType.Task,
            TaskTitle = "Call customer",
            TaskDescription = "Follow up on the email sent",
            TaskPriority = "High",
            TaskDueDays = 2
        };

        // Assert
        step.StepType.Should().Be(EmailStepType.Task);
        step.TaskTitle.Should().Be("Call customer");
        step.TaskDescription.Should().Be("Follow up on the email sent");
        step.TaskPriority.Should().Be("High");
        step.TaskDueDays.Should().Be(2);
    }

    [Fact]
    public void EmailSequenceStep_ShouldSupportConditionStepType()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            StepType = EmailStepType.Condition,
            ConditionType = "opened",
            ConditionValue = "true",
            TrueStepId = 5,
            FalseStepId = 6
        };

        // Assert
        step.StepType.Should().Be(EmailStepType.Condition);
        step.ConditionType.Should().Be("opened");
        step.ConditionValue.Should().Be("true");
        step.TrueStepId.Should().Be(5);
        step.FalseStepId.Should().Be(6);
    }

    [Fact]
    public void EmailSequenceStep_ShouldSupportABTesting()
    {
        // Arrange
        var stepA = new EmailSequenceStep
        {
            IsABTest = true,
            ABVariant = "A",
            ABSplitPercent = 50,
            Subject = "Subject A"
        };

        var stepB = new EmailSequenceStep
        {
            IsABTest = true,
            ABVariant = "B",
            ABSplitPercent = 50,
            Subject = "Subject B"
        };

        // Assert
        stepA.IsABTest.Should().BeTrue();
        stepA.ABVariant.Should().Be("A");
        stepA.ABSplitPercent.Should().Be(50);
        stepB.ABVariant.Should().Be("B");
    }

    [Fact]
    public void EmailSequenceStep_ShouldTrackStatistics()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            ExecutionCount = 100,
            EmailsSent = 98,
            Opens = 75,
            Clicks = 25,
            Replies = 10,
            Bounces = 2
        };

        // Assert
        step.ExecutionCount.Should().Be(100);
        step.EmailsSent.Should().Be(98);
        step.Opens.Should().Be(75);
        step.Clicks.Should().Be(25);
        step.Replies.Should().Be(10);
        step.Bounces.Should().Be(2);
    }

    [Theory]
    [InlineData(EmailStepType.Email)]
    [InlineData(EmailStepType.Wait)]
    [InlineData(EmailStepType.Task)]
    [InlineData(EmailStepType.Condition)]
    [InlineData(EmailStepType.LinkedIn)]
    [InlineData(EmailStepType.Call)]
    [InlineData(EmailStepType.SMS)]
    [InlineData(EmailStepType.Notification)]
    public void EmailSequenceStep_ShouldAcceptAllStepTypes(EmailStepType stepType)
    {
        // Arrange & Act
        var step = new EmailSequenceStep { StepType = stepType };

        // Assert
        step.StepType.Should().Be(stepType);
    }

    [Fact]
    public void EmailSequenceStep_ShouldSupportReplyThreading()
    {
        // Arrange
        var step = new EmailSequenceStep
        {
            StepOrder = 3,
            IsReply = true,
            ReplyToStepId = 1
        };

        // Assert
        step.IsReply.Should().BeTrue();
        step.ReplyToStepId.Should().Be(1);
    }

    #endregion

    #region EmailSequenceEnrollment Entity Tests

    [Fact]
    public void EmailSequenceEnrollment_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var enrollment = new EmailSequenceEnrollment();

        // Assert
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        enrollment.RecipientEmail.Should().BeEmpty();
        enrollment.CurrentStepIndex.Should().Be(0);
        enrollment.StepsCompleted.Should().Be(0);
        enrollment.EmailsSent.Should().Be(0);
        enrollment.TotalOpens.Should().Be(0);
        enrollment.TotalClicks.Should().Be(0);
        enrollment.HasReplied.Should().BeFalse();
        enrollment.HasBounced.Should().BeFalse();
        enrollment.HasUnsubscribed.Should().BeFalse();
        enrollment.MeetingBooked.Should().BeFalse();
        enrollment.StepExecutions.Should().BeEmpty();
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldAllowSettingEnrollmentDetails()
    {
        // Arrange
        var enrolledAt = DateTime.UtcNow.AddDays(-7);
        var completedAt = DateTime.UtcNow;

        var enrollment = new EmailSequenceEnrollment
        {
            Id = 1,
            EmailSequenceId = 10,
            Status = EnrollmentStatus.Completed,
            EnrolledAt = enrolledAt,
            CompletedAt = completedAt,
            ExitReason = SequenceExitCondition.OnReply,
            ExitNotes = "Customer replied positively"
        };

        // Assert
        enrollment.Id.Should().Be(1);
        enrollment.EmailSequenceId.Should().Be(10);
        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        enrollment.EnrolledAt.Should().Be(enrolledAt);
        enrollment.CompletedAt.Should().Be(completedAt);
        enrollment.ExitReason.Should().Be(SequenceExitCondition.OnReply);
        enrollment.ExitNotes.Should().Be("Customer replied positively");
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldTrackProgress()
    {
        // Arrange
        var enrollment = new EmailSequenceEnrollment
        {
            CurrentStepIndex = 3,
            CurrentStepId = 15,
            NextStepScheduledAt = DateTime.UtcNow.AddDays(1),
            LastStepExecutedAt = DateTime.UtcNow.AddHours(-2),
            StepsCompleted = 3,
            EmailsSent = 2
        };

        // Assert
        enrollment.CurrentStepIndex.Should().Be(3);
        enrollment.CurrentStepId.Should().Be(15);
        enrollment.NextStepScheduledAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(1), TimeSpan.FromMinutes(1));
        enrollment.LastStepExecutedAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(-2), TimeSpan.FromMinutes(1));
        enrollment.StepsCompleted.Should().Be(3);
        enrollment.EmailsSent.Should().Be(2);
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldTrackRecipientInfo()
    {
        // Arrange
        var enrollment = new EmailSequenceEnrollment
        {
            RecipientEmail = "john.doe@example.com",
            RecipientName = "John Doe",
            RecipientTimezone = "America/New_York"
        };

        // Assert
        enrollment.RecipientEmail.Should().Be("john.doe@example.com");
        enrollment.RecipientName.Should().Be("John Doe");
        enrollment.RecipientTimezone.Should().Be("America/New_York");
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldTrackEngagementMetrics()
    {
        // Arrange
        var replyDate = DateTime.UtcNow.AddDays(-1);
        var enrollment = new EmailSequenceEnrollment
        {
            TotalOpens = 5,
            TotalClicks = 2,
            HasReplied = true,
            RepliedAt = replyDate,
            HasBounced = false,
            HasUnsubscribed = false,
            MeetingBooked = true,
            MeetingBookedAt = DateTime.UtcNow
        };

        // Assert
        enrollment.TotalOpens.Should().Be(5);
        enrollment.TotalClicks.Should().Be(2);
        enrollment.HasReplied.Should().BeTrue();
        enrollment.RepliedAt.Should().BeCloseTo(replyDate, TimeSpan.FromMinutes(1));
        enrollment.HasBounced.Should().BeFalse();
        enrollment.HasUnsubscribed.Should().BeFalse();
        enrollment.MeetingBooked.Should().BeTrue();
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldSupportLeadNavigation()
    {
        // Arrange
        var lead = new Lead { Id = 1, FirstName = "Jane", LastName = "Smith" };
        var enrollment = new EmailSequenceEnrollment
        {
            LeadId = 1,
            Lead = lead
        };

        // Assert
        enrollment.LeadId.Should().Be(1);
        enrollment.Lead.Should().Be(lead);
        enrollment.Lead!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldSupportContactNavigation()
    {
        // Arrange
        var contact = new Contact { Id = 1, FirstName = "Bob", LastName = "Johnson" };
        var enrollment = new EmailSequenceEnrollment
        {
            ContactId = 1,
            Contact = contact
        };

        // Assert
        enrollment.ContactId.Should().Be(1);
        enrollment.Contact.Should().Be(contact);
    }

    [Theory]
    [InlineData(EnrollmentStatus.Active)]
    [InlineData(EnrollmentStatus.Paused)]
    [InlineData(EnrollmentStatus.Completed)]
    [InlineData(EnrollmentStatus.Unsubscribed)]
    [InlineData(EnrollmentStatus.Bounced)]
    [InlineData(EnrollmentStatus.Replied)]
    [InlineData(EnrollmentStatus.MeetingBooked)]
    [InlineData(EnrollmentStatus.Converted)]
    [InlineData(EnrollmentStatus.Removed)]
    [InlineData(EnrollmentStatus.Error)]
    public void EmailSequenceEnrollment_ShouldAcceptAllStatuses(EnrollmentStatus status)
    {
        // Arrange & Act
        var enrollment = new EmailSequenceEnrollment { Status = status };

        // Assert
        enrollment.Status.Should().Be(status);
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldTrackBounceDetails()
    {
        // Arrange
        var bounceDate = DateTime.UtcNow;
        var enrollment = new EmailSequenceEnrollment
        {
            HasBounced = true,
            BouncedAt = bounceDate,
            Status = EnrollmentStatus.Bounced,
            ExitReason = SequenceExitCondition.OnBounce
        };

        // Assert
        enrollment.HasBounced.Should().BeTrue();
        enrollment.BouncedAt.Should().BeCloseTo(bounceDate, TimeSpan.FromMinutes(1));
        enrollment.Status.Should().Be(EnrollmentStatus.Bounced);
        enrollment.ExitReason.Should().Be(SequenceExitCondition.OnBounce);
    }

    [Fact]
    public void EmailSequenceEnrollment_ShouldTrackUnsubscribeDetails()
    {
        // Arrange
        var unsubDate = DateTime.UtcNow;
        var enrollment = new EmailSequenceEnrollment
        {
            HasUnsubscribed = true,
            UnsubscribedAt = unsubDate,
            Status = EnrollmentStatus.Unsubscribed,
            ExitReason = SequenceExitCondition.OnUnsubscribe
        };

        // Assert
        enrollment.HasUnsubscribed.Should().BeTrue();
        enrollment.UnsubscribedAt.Should().BeCloseTo(unsubDate, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region EmailSequenceStepExecution Entity Tests

    [Fact]
    public void EmailSequenceStepExecution_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var execution = new EmailSequenceStepExecution();

        // Assert
        execution.Success.Should().BeFalse();
        execution.Opens.Should().Be(0);
        execution.Clicks.Should().Be(0);
        execution.Replied.Should().BeFalse();
        execution.Bounced.Should().BeFalse();
    }

    [Fact]
    public void EmailSequenceStepExecution_ShouldTrackExecutionDetails()
    {
        // Arrange
        var scheduledAt = DateTime.UtcNow.AddMinutes(-30);
        var executedAt = DateTime.UtcNow.AddMinutes(-25);

        var execution = new EmailSequenceStepExecution
        {
            Id = 1,
            EmailSequenceStepId = 10,
            EmailSequenceEnrollmentId = 5,
            ScheduledAt = scheduledAt,
            ExecutedAt = executedAt,
            Success = true,
            MessageId = "msg-abc123"
        };

        // Assert
        execution.Id.Should().Be(1);
        execution.EmailSequenceStepId.Should().Be(10);
        execution.EmailSequenceEnrollmentId.Should().Be(5);
        execution.ScheduledAt.Should().Be(scheduledAt);
        execution.ExecutedAt.Should().Be(executedAt);
        execution.Success.Should().BeTrue();
        execution.MessageId.Should().Be("msg-abc123");
    }

    [Fact]
    public void EmailSequenceStepExecution_ShouldTrackEngagement()
    {
        // Arrange
        var replyDate = DateTime.UtcNow;
        var execution = new EmailSequenceStepExecution
        {
            Opens = 3,
            Clicks = 1,
            Replied = true,
            RepliedAt = replyDate
        };

        // Assert
        execution.Opens.Should().Be(3);
        execution.Clicks.Should().Be(1);
        execution.Replied.Should().BeTrue();
        execution.RepliedAt.Should().BeCloseTo(replyDate, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void EmailSequenceStepExecution_ShouldTrackFailure()
    {
        // Arrange
        var execution = new EmailSequenceStepExecution
        {
            Success = false,
            ErrorMessage = "SMTP connection timeout"
        };

        // Assert
        execution.Success.Should().BeFalse();
        execution.ErrorMessage.Should().Be("SMTP connection timeout");
    }

    [Fact]
    public void EmailSequenceStepExecution_ShouldTrackBounce()
    {
        // Arrange
        var execution = new EmailSequenceStepExecution
        {
            Bounced = true,
            BounceType = "hard"
        };

        // Assert
        execution.Bounced.Should().BeTrue();
        execution.BounceType.Should().Be("hard");
    }

    [Fact]
    public void EmailSequenceStepExecution_ShouldSupportNavigationProperties()
    {
        // Arrange
        var step = new EmailSequenceStep { Id = 10, Name = "Follow-up" };
        var enrollment = new EmailSequenceEnrollment { Id = 5, RecipientEmail = "test@example.com" };

        var execution = new EmailSequenceStepExecution
        {
            EmailSequenceStepId = 10,
            EmailSequenceStep = step,
            EmailSequenceEnrollmentId = 5,
            EmailSequenceEnrollment = enrollment
        };

        // Assert
        execution.EmailSequenceStep.Should().Be(step);
        execution.EmailSequenceStep!.Name.Should().Be("Follow-up");
        execution.EmailSequenceEnrollment.Should().Be(enrollment);
        execution.EmailSequenceEnrollment!.RecipientEmail.Should().Be("test@example.com");
    }

    #endregion

    #region Integration Tests - Complete Email Sequence

    [Fact]
    public void EmailSequence_ShouldSupportCompleteWorkflow()
    {
        // Arrange - Create a complete sequence with steps and enrollment
        var owner = new User { Id = 1, FirstName = "Sales", LastName = "Rep" };
        var lead = new Lead { Id = 10, FirstName = "Prospect", LastName = "Customer", Email = "prospect@example.com" };

        var sequence = new EmailSequence
        {
            Id = 1,
            Name = "Sales Outreach",
            Status = EmailSequenceStatus.Active,
            OwnerId = 1,
            Owner = owner,
            TotalEnrolled = 1
        };

        var step1 = new EmailSequenceStep
        {
            Id = 1,
            StepOrder = 1,
            Name = "Introduction",
            StepType = EmailStepType.Email,
            Subject = "Nice to meet you!",
            EmailSequenceId = 1,
            EmailSequence = sequence
        };

        var step2 = new EmailSequenceStep
        {
            Id = 2,
            StepOrder = 2,
            Name = "Wait 3 days",
            StepType = EmailStepType.Wait,
            DelayDays = 3,
            EmailSequenceId = 1,
            EmailSequence = sequence
        };

        var step3 = new EmailSequenceStep
        {
            Id = 3,
            StepOrder = 3,
            Name = "Follow-up",
            StepType = EmailStepType.Email,
            Subject = "Following up",
            EmailSequenceId = 1,
            EmailSequence = sequence
        };

        sequence.Steps.Add(step1);
        sequence.Steps.Add(step2);
        sequence.Steps.Add(step3);

        var enrollment = new EmailSequenceEnrollment
        {
            Id = 1,
            EmailSequenceId = 1,
            EmailSequence = sequence,
            LeadId = 10,
            Lead = lead,
            RecipientEmail = lead.Email!,
            RecipientName = $"{lead.FirstName} {lead.LastName}",
            Status = EnrollmentStatus.Active,
            CurrentStepIndex = 0
        };

        sequence.Enrollments.Add(enrollment);

        // Assert - Verify structure
        sequence.Steps.Should().HaveCount(3);
        sequence.Steps.Should().BeInAscendingOrder(s => s.StepOrder);
        sequence.Enrollments.Should().HaveCount(1);
        sequence.Enrollments.First().Lead.Should().Be(lead);
        sequence.Owner.Should().Be(owner);
    }

    [Fact]
    public void EmailSequence_CalculateOpenRate_ShouldBeCorrect()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            TotalEmailsSent = 100,
            TotalOpens = 40
        };

        // Act
        var openRate = sequence.TotalEmailsSent > 0
            ? (double)sequence.TotalOpens / sequence.TotalEmailsSent * 100
            : 0;

        // Assert
        openRate.Should().Be(40.0);
    }

    [Fact]
    public void EmailSequence_CalculateClickThroughRate_ShouldBeCorrect()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            TotalOpens = 40,
            TotalClicks = 10
        };

        // Act
        var clickThroughRate = sequence.TotalOpens > 0
            ? (double)sequence.TotalClicks / sequence.TotalOpens * 100
            : 0;

        // Assert
        clickThroughRate.Should().Be(25.0);
    }

    [Fact]
    public void EmailSequence_CalculateReplyRate_ShouldBeCorrect()
    {
        // Arrange
        var sequence = new EmailSequence
        {
            TotalEmailsSent = 100,
            TotalReplies = 5
        };

        // Act
        var replyRate = sequence.TotalEmailsSent > 0
            ? (double)sequence.TotalReplies / sequence.TotalEmailsSent * 100
            : 0;

        // Assert
        replyRate.Should().Be(5.0);
    }

    #endregion
}
