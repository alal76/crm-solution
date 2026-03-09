// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities.KnowledgeBase;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

public class KnowledgeArticleEntityTests
{
    public class PublishTests
    {
        [Fact]
        public void Publish_ShouldSetStatusToPublished_WhenDraft()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Draft);

            article.Publish();

            article.Status.Should().Be(ArticleStatus.Published);
        }

        [Fact]
        public void Publish_ShouldSetPublishedAt()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Draft);

            article.Publish();

            article.PublishedAt.Should().NotBeNull();
        }

        [Fact]
        public void Publish_ShouldRaiseKnowledgeArticlePublishedEvent()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Draft);

            article.Publish();

            article.DomainEvents.Should().ContainSingle(e => e is KnowledgeArticlePublishedEvent);
        }

        [Fact]
        public void Publish_ShouldThrow_WhenAlreadyPublished()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Published);

            var act = () => article.Publish();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already published*");
        }

        [Fact]
        public void Publish_ShouldThrow_WhenArchived()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Archived);

            var act = () => article.Publish();

            act.Should().Throw<BusinessRuleException>().WithMessage("*archived*");
        }
    }

    public class ArchiveTests
    {
        [Fact]
        public void Archive_ShouldSetStatusToArchived_WhenPublished()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Published);

            article.Archive();

            article.Status.Should().Be(ArticleStatus.Archived);
        }

        [Fact]
        public void Archive_ShouldRaiseKnowledgeArticleArchivedEvent()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Published);

            article.Archive();

            article.DomainEvents.Should().ContainSingle(e => e is KnowledgeArticleArchivedEvent);
        }

        [Fact]
        public void Archive_ShouldThrow_WhenAlreadyArchived()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Archived);

            var act = () => article.Archive();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already archived*");
        }

        [Fact]
        public void Archive_ShouldWorkOnDraft()
        {
            var article = KnowledgeArticle.CreateForTesting(ArticleStatus.Draft);

            article.Archive();

            article.Status.Should().Be(ArticleStatus.Archived);
        }
    }
}
