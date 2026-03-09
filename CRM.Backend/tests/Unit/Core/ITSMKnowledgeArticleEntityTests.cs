// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities.Events;
using CRM.Core.Entities.ITSM;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;
using ITSM = CRM.Core.Entities.ITSM;

namespace CRM.Tests.Unit.Core;

public class ITSMKnowledgeArticleEntityTests
{
    public class PublishTests
    {
        [Fact]
        public void Publish_ShouldSetStatusToPublished_WhenApproved()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Approved);

            sut.Publish(42);

            sut.PublishingState.Should().Be(PublishingState.Published);
            sut.PublishedById.Should().Be(42);
            sut.PublishedDate.Should().NotBeNull();
            sut.ModifiedAt.Should().NotBeNull();
        }

        [Fact]
        public void Publish_ShouldRaisePublishedEvent()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Approved);

            sut.Publish(42);

            sut.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<ITSMKnowledgeArticlePublishedEvent>();
        }

        [Fact]
        public void Publish_ShouldThrow_WhenNotApproved()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Draft);

            var act = () => sut.Publish(42);

            act.Should().Throw<BusinessRuleException>()
                .WithMessage("*Approved*");
        }
    }

    public class SubmitForReviewTests
    {
        [Fact]
        public void SubmitForReview_ShouldSetStatusToReview_WhenDraft()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Draft);

            sut.SubmitForReview();

            sut.PublishingState.Should().Be(PublishingState.Review);
            sut.ModifiedAt.Should().NotBeNull();
        }

        [Fact]
        public void SubmitForReview_ShouldRaiseSubmittedEvent()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Draft);

            sut.SubmitForReview();

            sut.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<ITSMKnowledgeArticleSubmittedForReviewEvent>();
        }

        [Fact]
        public void SubmitForReview_ShouldThrow_WhenNotDraft()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Published);

            var act = () => sut.SubmitForReview();

            act.Should().Throw<BusinessRuleException>()
                .WithMessage("*Draft*");
        }
    }

    public class ApproveTests
    {
        [Fact]
        public void Approve_ShouldSetStatusToApproved_WhenInReview()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Review);

            sut.Approve();

            sut.PublishingState.Should().Be(PublishingState.Approved);
            sut.ModifiedAt.Should().NotBeNull();
        }

        [Fact]
        public void Approve_ShouldRaiseApprovedEvent()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Review);

            sut.Approve();

            sut.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<ITSMKnowledgeArticleApprovedEvent>();
        }

        [Fact]
        public void Approve_ShouldThrow_WhenNotInReview()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Draft);

            var act = () => sut.Approve();

            act.Should().Throw<BusinessRuleException>()
                .WithMessage("*Review*");
        }
    }

    public class RetireTests
    {
        [Fact]
        public void Retire_ShouldSetStatusToRetired_WhenPublished()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Published);

            sut.Retire("End of lifecycle");

            sut.PublishingState.Should().Be(PublishingState.Retired);
            sut.ModifiedAt.Should().NotBeNull();
        }

        [Fact]
        public void Retire_ShouldRaiseRetiredEvent()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Published);

            sut.Retire("Replaced by newer article");

            sut.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<ITSMKnowledgeArticleRetiredEvent>();
        }

        [Fact]
        public void Retire_ShouldThrow_WhenNotPublished()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Draft);

            var act = () => sut.Retire("Reason");

            act.Should().Throw<BusinessRuleException>()
                .WithMessage("*Published*");
        }

        [Fact]
        public void Retire_ShouldThrow_WhenReasonIsEmpty()
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(PublishingState.Published);

            var act = () => sut.Retire("");

            act.Should().Throw<BusinessRuleException>()
                .WithMessage("*reason*");
        }
    }

    public class CreateForTestingTests
    {
        [Theory]
        [InlineData(PublishingState.Draft)]
        [InlineData(PublishingState.Review)]
        [InlineData(PublishingState.Approved)]
        [InlineData(PublishingState.Published)]
        [InlineData(PublishingState.Retired)]
        public void CreateForTesting_ShouldSetState(PublishingState expected)
        {
            var sut = ITSM.KnowledgeArticle.CreateForTesting(expected);

            sut.PublishingState.Should().Be(expected);
            sut.Title.Should().NotBeNullOrEmpty();
            sut.ArticleBody.Should().NotBeNullOrEmpty();
        }
    }
}
