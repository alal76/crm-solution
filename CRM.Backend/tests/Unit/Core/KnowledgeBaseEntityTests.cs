// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// Licensed under the GNU Affero General Public License v3.0.
// See LICENSE file in the project root for full license information.

using CRM.Core.Entities;
using CRM.Core.Entities.KnowledgeBase;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for KnowledgeBase entities and enums.
/// Tests ArticleType, ArticleStatus, ArticleVisibility, SLAPriority, SLAMetricType,
/// SLATimeUnit, SLAStatus, EscalationType enums and KnowledgeArticle, KnowledgeCategory,
/// ServiceRequestArticle, ArticleFeedback, SLAPolicy, SLATarget, BusinessHours,
/// EscalationRule, SLAInstance entities.
/// </summary>
public class KnowledgeBaseEntityTests
{
    #region ArticleType Enum Tests

    [Fact]
    public void ArticleType_ShouldHaveCorrectValues()
    {
        ((int)ArticleType.HowTo).Should().Be(0);
        ((int)ArticleType.FAQ).Should().Be(1);
        ((int)ArticleType.Troubleshooting).Should().Be(2);
        ((int)ArticleType.BestPractice).Should().Be(3);
        ((int)ArticleType.Documentation).Should().Be(4);
        ((int)ArticleType.Process).Should().Be(5);
        ((int)ArticleType.Policy).Should().Be(6);
        ((int)ArticleType.ReleaseNotes).Should().Be(7);
        ((int)ArticleType.Video).Should().Be(8);
        ((int)ArticleType.Template).Should().Be(9);
    }

    [Fact]
    public void ArticleType_ShouldHave10Values()
    {
        Enum.GetValues<ArticleType>().Should().HaveCount(10);
    }

    #endregion

    #region ArticleStatus Enum Tests

    [Fact]
    public void ArticleStatus_ShouldHaveCorrectValues()
    {
        ((int)ArticleStatus.Draft).Should().Be(0);
        ((int)ArticleStatus.InReview).Should().Be(1);
        ((int)ArticleStatus.Published).Should().Be(2);
        ((int)ArticleStatus.NeedsUpdate).Should().Be(3);
        ((int)ArticleStatus.Archived).Should().Be(4);
        ((int)ArticleStatus.Deprecated).Should().Be(5);
    }

    [Fact]
    public void ArticleStatus_ShouldHave6Values()
    {
        Enum.GetValues<ArticleStatus>().Should().HaveCount(6);
    }

    #endregion

    #region ArticleVisibility Enum Tests

    [Fact]
    public void ArticleVisibility_ShouldHaveCorrectValues()
    {
        ((int)ArticleVisibility.Internal).Should().Be(0);
        ((int)ArticleVisibility.CustomerPortal).Should().Be(1);
        ((int)ArticleVisibility.Public).Should().Be(2);
    }

    [Fact]
    public void ArticleVisibility_ShouldHave3Values()
    {
        Enum.GetValues<ArticleVisibility>().Should().HaveCount(3);
    }

    #endregion

    #region SLAPriority Enum Tests

    [Fact]
    public void SLAPriority_ShouldHaveCorrectValues()
    {
        ((int)SLAPriority.Critical).Should().Be(0);
        ((int)SLAPriority.High).Should().Be(1);
        ((int)SLAPriority.Medium).Should().Be(2);
        ((int)SLAPriority.Low).Should().Be(3);
    }

    [Fact]
    public void SLAPriority_ShouldHave4Values()
    {
        Enum.GetValues<SLAPriority>().Should().HaveCount(4);
    }

    #endregion

    #region SLAMetricType Enum Tests

    [Fact]
    public void SLAMetricType_ShouldHaveCorrectValues()
    {
        ((int)SLAMetricType.FirstResponse).Should().Be(0);
        ((int)SLAMetricType.Resolution).Should().Be(1);
        ((int)SLAMetricType.NextResponse).Should().Be(2);
        ((int)SLAMetricType.Assignment).Should().Be(3);
        ((int)SLAMetricType.Custom).Should().Be(99);
    }

    [Fact]
    public void SLAMetricType_ShouldHave5Values()
    {
        Enum.GetValues<SLAMetricType>().Should().HaveCount(5);
    }

    #endregion

    #region SLATimeUnit Enum Tests

    [Fact]
    public void SLATimeUnit_ShouldHaveCorrectValues()
    {
        ((int)SLATimeUnit.Minutes).Should().Be(0);
        ((int)SLATimeUnit.Hours).Should().Be(1);
        ((int)SLATimeUnit.BusinessHours).Should().Be(2);
        ((int)SLATimeUnit.Days).Should().Be(3);
        ((int)SLATimeUnit.BusinessDays).Should().Be(4);
    }

    [Fact]
    public void SLATimeUnit_ShouldHave5Values()
    {
        Enum.GetValues<SLATimeUnit>().Should().HaveCount(5);
    }

    #endregion

    #region SLAStatus Enum Tests

    [Fact]
    public void SLAStatus_ShouldHaveCorrectValues()
    {
        ((int)SLAStatus.OnTrack).Should().Be(0);
        ((int)SLAStatus.AtRisk).Should().Be(1);
        ((int)SLAStatus.Breached).Should().Be(2);
        ((int)SLAStatus.Paused).Should().Be(3);
        ((int)SLAStatus.Met).Should().Be(4);
    }

    [Fact]
    public void SLAStatus_ShouldHave5Values()
    {
        Enum.GetValues<SLAStatus>().Should().HaveCount(5);
    }

    #endregion

    #region EscalationType Enum Tests

    [Fact]
    public void EscalationType_ShouldHaveCorrectValues()
    {
        ((int)EscalationType.Email).Should().Be(0);
        ((int)EscalationType.ReassignUser).Should().Be(1);
        ((int)EscalationType.ReassignTeam).Should().Be(2);
        ((int)EscalationType.IncreasePriority).Should().Be(3);
        ((int)EscalationType.Webhook).Should().Be(4);
        ((int)EscalationType.SMS).Should().Be(5);
        ((int)EscalationType.Custom).Should().Be(99);
    }

    [Fact]
    public void EscalationType_ShouldHave7Values()
    {
        Enum.GetValues<EscalationType>().Should().HaveCount(7);
    }

    #endregion

    #region KnowledgeArticle Entity Tests

    [Fact]
    public void KnowledgeArticle_ShouldInitializeWithDefaults()
    {
        var article = new KnowledgeArticle();

        article.ArticleNumber.Should().StartWith("KB");
        article.Title.Should().Be(string.Empty);
        article.Summary.Should().BeNull();
        article.Slug.Should().Be(string.Empty);
        article.ArticleType.Should().Be(ArticleType.HowTo);
        article.Content.Should().Be(string.Empty);
        article.ContentFormat.Should().Be("html");
        article.Status.Should().Be(ArticleStatus.Draft);
        article.Visibility.Should().Be(ArticleVisibility.Internal);
        article.Version.Should().Be(1);
        article.ViewCount.Should().Be(0);
        article.UniqueVisitorCount.Should().Be(0);
        article.HelpfulCount.Should().Be(0);
        article.NotHelpfulCount.Should().Be(0);
        article.RatingCount.Should().Be(0);
        article.CaseDeflectionCount.Should().Be(0);
        article.SearchImpressionCount.Should().Be(0);
        article.SearchClickCount.Should().Be(0);
        article.LanguageCode.Should().Be("en");
    }

    [Fact]
    public void KnowledgeArticle_HelpfulnessScore_ShouldCalculateCorrectly_WhenNoVotes()
    {
        var article = new KnowledgeArticle { HelpfulCount = 0, NotHelpfulCount = 0 };

        article.HelpfulnessScore.Should().Be(0);
    }

    [Fact]
    public void KnowledgeArticle_HelpfulnessScore_ShouldCalculateCorrectly_WhenAllHelpful()
    {
        var article = new KnowledgeArticle { HelpfulCount = 10, NotHelpfulCount = 0 };

        article.HelpfulnessScore.Should().Be(100);
    }

    [Fact]
    public void KnowledgeArticle_HelpfulnessScore_ShouldCalculateCorrectly_WhenMixed()
    {
        var article = new KnowledgeArticle { HelpfulCount = 8, NotHelpfulCount = 2 };

        article.HelpfulnessScore.Should().Be(80);
    }

    [Fact]
    public void KnowledgeArticle_ShouldSetProperties()
    {
        var publishDate = DateTime.UtcNow;
        var article = new KnowledgeArticle
        {
            Title = "How to reset password",
            Slug = "how-to-reset-password",
            ArticleType = ArticleType.HowTo,
            Content = "<p>Instructions here</p>",
            Status = ArticleStatus.Published,
            Visibility = ArticleVisibility.Public,
            PublishedAt = publishDate,
            AuthorUserId = 1
        };

        article.Title.Should().Be("How to reset password");
        article.Slug.Should().Be("how-to-reset-password");
        article.ArticleType.Should().Be(ArticleType.HowTo);
        article.Status.Should().Be(ArticleStatus.Published);
        article.Visibility.Should().Be(ArticleVisibility.Public);
        article.PublishedAt.Should().Be(publishDate);
    }

    [Fact]
    public void KnowledgeArticle_Collections_ShouldInitializeEmpty()
    {
        var article = new KnowledgeArticle();

        article.Translations.Should().NotBeNull().And.BeEmpty();
        article.ServiceRequests.Should().NotBeNull().And.BeEmpty();
        article.Feedback.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region KnowledgeCategory Entity Tests

    [Fact]
    public void KnowledgeCategory_ShouldInitializeWithDefaults()
    {
        var category = new KnowledgeCategory();

        category.Name.Should().Be(string.Empty);
        category.Description.Should().BeNull();
        category.Slug.Should().Be(string.Empty);
        category.Icon.Should().BeNull();
        category.DisplayOrder.Should().Be(0);
        category.IsActive.Should().BeTrue();
        category.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public void KnowledgeCategory_ShouldSetProperties()
    {
        var category = new KnowledgeCategory
        {
            Name = "Getting Started",
            Slug = "getting-started",
            Description = "Beginner guides",
            Icon = "start-icon",
            DisplayOrder = 1,
            IsActive = true
        };

        category.Name.Should().Be("Getting Started");
        category.Slug.Should().Be("getting-started");
        category.Description.Should().Be("Beginner guides");
        category.Icon.Should().Be("start-icon");
        category.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public void KnowledgeCategory_Collections_ShouldInitializeEmpty()
    {
        var category = new KnowledgeCategory();

        category.ChildCategories.Should().NotBeNull().And.BeEmpty();
        category.Articles.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region ServiceRequestArticle Entity Tests

    [Fact]
    public void ServiceRequestArticle_ShouldInitializeWithDefaults()
    {
        var link = new ServiceRequestArticle();

        link.ServiceRequestId.Should().Be(0);
        link.KnowledgeArticleId.Should().Be(0);
        link.WasHelpful.Should().BeNull();
        link.DeflectedCase.Should().BeFalse();
        link.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        link.LinkedByUserId.Should().BeNull();
    }

    [Fact]
    public void ServiceRequestArticle_ShouldSetProperties()
    {
        var linkTime = DateTime.UtcNow;
        var link = new ServiceRequestArticle
        {
            ServiceRequestId = 100,
            KnowledgeArticleId = 50,
            WasHelpful = true,
            DeflectedCase = true,
            LinkedAt = linkTime,
            LinkedByUserId = 1
        };

        link.ServiceRequestId.Should().Be(100);
        link.KnowledgeArticleId.Should().Be(50);
        link.WasHelpful.Should().BeTrue();
        link.DeflectedCase.Should().BeTrue();
        link.LinkedByUserId.Should().Be(1);
    }

    #endregion

    #region ArticleFeedback Entity Tests

    [Fact]
    public void ArticleFeedback_ShouldInitializeWithDefaults()
    {
        var feedback = new ArticleFeedback();

        feedback.KnowledgeArticleId.Should().Be(0);
        feedback.IsHelpful.Should().BeFalse();
        feedback.Rating.Should().BeNull();
        feedback.Comment.Should().BeNull();
        feedback.UserId.Should().BeNull();
        feedback.AccountId.Should().BeNull();
        feedback.SessionId.Should().BeNull();
        feedback.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ArticleFeedback_ShouldSetProperties()
    {
        var feedback = new ArticleFeedback
        {
            KnowledgeArticleId = 10,
            IsHelpful = true,
            Rating = 5,
            Comment = "Very helpful article!",
            UserId = 1
        };

        feedback.KnowledgeArticleId.Should().Be(10);
        feedback.IsHelpful.Should().BeTrue();
        feedback.Rating.Should().Be(5);
        feedback.Comment.Should().Be("Very helpful article!");
        feedback.UserId.Should().Be(1);
    }

    #endregion

    #region SLAPolicy Entity Tests

    [Fact]
    public void SLAPolicy_ShouldInitializeWithDefaults()
    {
        var policy = new SLAPolicy();

        policy.Name.Should().Be(string.Empty);
        policy.Description.Should().BeNull();
        policy.IsActive.Should().BeTrue();
        policy.IsDefault.Should().BeFalse();
        policy.Priority.Should().Be(0);
        policy.CasePriority.Should().BeNull();
        policy.CustomerSegmentsJson.Should().BeNull();
        policy.ProductsJson.Should().BeNull();
        policy.CaseTypesJson.Should().BeNull();
        policy.CustomerTiersJson.Should().BeNull();
        policy.MatchConditionsJson.Should().BeNull();
        policy.BusinessHoursId.Should().BeNull();
        policy.ExcludeHolidays.Should().BeTrue();
    }

    [Fact]
    public void SLAPolicy_ShouldSetProperties()
    {
        var policy = new SLAPolicy
        {
            Name = "Enterprise SLA",
            Description = "For enterprise customers",
            IsActive = true,
            IsDefault = false,
            Priority = 100,
            CasePriority = SLAPriority.High,
            ExcludeHolidays = true,
            BusinessHoursId = 1
        };

        policy.Name.Should().Be("Enterprise SLA");
        policy.Description.Should().Be("For enterprise customers");
        policy.IsActive.Should().BeTrue();
        policy.Priority.Should().Be(100);
        policy.CasePriority.Should().Be(SLAPriority.High);
        policy.BusinessHoursId.Should().Be(1);
    }

    [Fact]
    public void SLAPolicy_Collections_ShouldInitializeEmpty()
    {
        var policy = new SLAPolicy();

        policy.Targets.Should().NotBeNull().And.BeEmpty();
        policy.EscalationRules.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region SLATarget Entity Tests

    [Fact]
    public void SLATarget_ShouldInitializeWithDefaults()
    {
        var target = new SLATarget();

        target.SLAPolicyId.Should().Be(0);
        target.MetricType.Should().Be(SLAMetricType.FirstResponse);
        target.TargetValue.Should().Be(0);
        target.TimeUnit.Should().Be(SLATimeUnit.Hours);
        target.WarningThresholdPercent.Should().Be(75);
        target.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SLATarget_ShouldSetProperties()
    {
        var target = new SLATarget
        {
            SLAPolicyId = 1,
            MetricType = SLAMetricType.Resolution,
            TargetValue = 4,
            TimeUnit = SLATimeUnit.BusinessHours,
            WarningThresholdPercent = 80,
            IsActive = true
        };

        target.SLAPolicyId.Should().Be(1);
        target.MetricType.Should().Be(SLAMetricType.Resolution);
        target.TargetValue.Should().Be(4);
        target.TimeUnit.Should().Be(SLATimeUnit.BusinessHours);
        target.WarningThresholdPercent.Should().Be(80);
    }

    #endregion

    #region BusinessHours Entity Tests

    [Fact]
    public void BusinessHours_ShouldInitializeWithDefaults()
    {
        var hours = new BusinessHours();

        hours.Name.Should().Be(string.Empty);
        hours.Timezone.Should().Be("UTC");
        hours.Is24x7.Should().BeFalse();
        hours.IsActive.Should().BeTrue();
        hours.ScheduleJson.Should().Be("{}");
        hours.HolidaysJson.Should().BeNull();
    }

    [Fact]
    public void BusinessHours_ShouldSetProperties()
    {
        var hours = new BusinessHours
        {
            Name = "US Business Hours",
            Timezone = "America/New_York",
            Is24x7 = false,
            IsActive = true,
            ScheduleJson = "{\"mon\":\"09:00-17:00\",\"tue\":\"09:00-17:00\"}"
        };

        hours.Name.Should().Be("US Business Hours");
        hours.Timezone.Should().Be("America/New_York");
        hours.Is24x7.Should().BeFalse();
        hours.ScheduleJson.Should().Contain("mon");
    }

    [Fact]
    public void BusinessHours_Collections_ShouldInitializeEmpty()
    {
        var hours = new BusinessHours();

        hours.Policies.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region EscalationRule Entity Tests

    [Fact]
    public void EscalationRule_ShouldInitializeWithDefaults()
    {
        var rule = new EscalationRule();

        rule.SLAPolicyId.Should().Be(0);
        rule.Name.Should().Be(string.Empty);
        rule.TriggerAtPercent.Should().Be(100);
        rule.TriggerMetric.Should().Be(SLAMetricType.FirstResponse);
        rule.IsActive.Should().BeTrue();
        rule.ExecutionOrder.Should().Be(0);
        rule.EscalationType.Should().Be(EscalationType.Email);
        rule.EmailRecipientsJson.Should().BeNull();
        rule.EmailTemplateId.Should().BeNull();
        rule.ReassignToUserId.Should().BeNull();
        rule.ReassignToTeamId.Should().BeNull();
        rule.NewPriority.Should().BeNull();
        rule.WebhookUrl.Should().BeNull();
        rule.ActionConfigJson.Should().BeNull();
    }

    [Fact]
    public void EscalationRule_ShouldSetProperties()
    {
        var rule = new EscalationRule
        {
            SLAPolicyId = 1,
            Name = "First Warning",
            TriggerAtPercent = 75,
            TriggerMetric = SLAMetricType.FirstResponse,
            EscalationType = EscalationType.Email,
            EmailRecipientsJson = "[\"manager@example.com\"]",
            ExecutionOrder = 1
        };

        rule.Name.Should().Be("First Warning");
        rule.TriggerAtPercent.Should().Be(75);
        rule.TriggerMetric.Should().Be(SLAMetricType.FirstResponse);
        rule.EscalationType.Should().Be(EscalationType.Email);
        rule.ExecutionOrder.Should().Be(1);
    }

    [Fact]
    public void EscalationRule_ReassignUser_ShouldSetProperties()
    {
        var rule = new EscalationRule
        {
            Name = "Escalate to Manager",
            EscalationType = EscalationType.ReassignUser,
            ReassignToUserId = 5,
            TriggerAtPercent = 100
        };

        rule.EscalationType.Should().Be(EscalationType.ReassignUser);
        rule.ReassignToUserId.Should().Be(5);
    }

    [Fact]
    public void EscalationRule_IncreasePriority_ShouldSetNewPriority()
    {
        var rule = new EscalationRule
        {
            Name = "Increase Priority",
            EscalationType = EscalationType.IncreasePriority,
            NewPriority = SLAPriority.Critical
        };

        rule.EscalationType.Should().Be(EscalationType.IncreasePriority);
        rule.NewPriority.Should().Be(SLAPriority.Critical);
    }

    #endregion

    #region SLAInstance Entity Tests

    [Fact]
    public void SLAInstance_ShouldInitializeWithDefaults()
    {
        var instance = new SLAInstance();

        instance.ServiceRequestId.Should().Be(0);
        instance.SLAPolicyId.Should().Be(0);
        instance.SLATargetId.Should().Be(0);
        instance.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        instance.ElapsedMinutes.Should().Be(0);
        instance.BusinessTimeElapsedMinutes.Should().Be(0);
        instance.Status.Should().Be(SLAStatus.OnTrack);
        instance.WasBreached.Should().BeFalse();
        instance.BreachedAt.Should().BeNull();
        instance.MinutesOverSla.Should().BeNull();
        instance.IsPaused.Should().BeFalse();
        instance.PausedAt.Should().BeNull();
        instance.TotalPauseMinutes.Should().Be(0);
        instance.PauseReason.Should().BeNull();
        instance.EscalationLevel.Should().Be(0);
        instance.EscalationsTriggeredJson.Should().BeNull();
        instance.LastEscalationAt.Should().BeNull();
    }

    [Fact]
    public void SLAInstance_ShouldSetProperties()
    {
        var now = DateTime.UtcNow;
        var dueDate = now.AddHours(4);
        var warningDate = now.AddHours(3);

        var instance = new SLAInstance
        {
            ServiceRequestId = 100,
            SLAPolicyId = 1,
            SLATargetId = 1,
            StartedAt = now,
            DueAt = dueDate,
            WarningAt = warningDate,
            RemainingMinutes = 240,
            Status = SLAStatus.OnTrack
        };

        instance.ServiceRequestId.Should().Be(100);
        instance.SLAPolicyId.Should().Be(1);
        instance.DueAt.Should().Be(dueDate);
        instance.WarningAt.Should().Be(warningDate);
        instance.RemainingMinutes.Should().Be(240);
        instance.Status.Should().Be(SLAStatus.OnTrack);
    }

    [Fact]
    public void SLAInstance_Breached_ShouldTrackBreachDetails()
    {
        var breachTime = DateTime.UtcNow;

        var instance = new SLAInstance
        {
            Status = SLAStatus.Breached,
            WasBreached = true,
            BreachedAt = breachTime,
            MinutesOverSla = 30
        };

        instance.Status.Should().Be(SLAStatus.Breached);
        instance.WasBreached.Should().BeTrue();
        instance.BreachedAt.Should().Be(breachTime);
        instance.MinutesOverSla.Should().Be(30);
    }

    [Fact]
    public void SLAInstance_Paused_ShouldTrackPauseDetails()
    {
        var pauseTime = DateTime.UtcNow;

        var instance = new SLAInstance
        {
            IsPaused = true,
            PausedAt = pauseTime,
            TotalPauseMinutes = 60,
            PauseReason = "Waiting for customer response",
            Status = SLAStatus.Paused
        };

        instance.IsPaused.Should().BeTrue();
        instance.PausedAt.Should().Be(pauseTime);
        instance.TotalPauseMinutes.Should().Be(60);
        instance.PauseReason.Should().Be("Waiting for customer response");
        instance.Status.Should().Be(SLAStatus.Paused);
    }

    [Fact]
    public void SLAInstance_Escalation_ShouldTrackEscalationDetails()
    {
        var escalationTime = DateTime.UtcNow;

        var instance = new SLAInstance
        {
            EscalationLevel = 2,
            EscalationsTriggeredJson = "[\"level1\",\"level2\"]",
            LastEscalationAt = escalationTime
        };

        instance.EscalationLevel.Should().Be(2);
        instance.EscalationsTriggeredJson.Should().Contain("level1");
        instance.LastEscalationAt.Should().Be(escalationTime);
    }

    [Fact]
    public void SLAInstance_Met_ShouldMarkCompleted()
    {
        var completionTime = DateTime.UtcNow;

        var instance = new SLAInstance
        {
            Status = SLAStatus.Met,
            CompletedAt = completionTime,
            WasBreached = false
        };

        instance.Status.Should().Be(SLAStatus.Met);
        instance.CompletedAt.Should().Be(completionTime);
        instance.WasBreached.Should().BeFalse();
    }

    #endregion
}
