// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

public class MarketingCampaignEntityTests
{
    public class LaunchTests
    {
        [Fact]
        public void Launch_ShouldSetStatusToActive_WhenDraft()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Draft);

            campaign.Launch();

            campaign.Status.Should().Be(CampaignStatus.Active);
        }

        [Fact]
        public void Launch_ShouldSetStartedAt()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Draft);

            campaign.Launch();

            campaign.StartedAt.Should().NotBeNull();
        }

        [Fact]
        public void Launch_ShouldRaiseCampaignLaunchedEvent()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Draft);

            campaign.Launch();

            campaign.DomainEvents.Should().ContainSingle(e => e is CampaignLaunchedEvent);
        }

        [Fact]
        public void Launch_ShouldThrow_WhenAlreadyActive()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            var act = () => campaign.Launch();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already active*");
        }

        [Fact]
        public void Launch_ShouldThrow_WhenCompleted()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Completed);

            var act = () => campaign.Launch();

            act.Should().Throw<BusinessRuleException>().WithMessage("*completed*");
        }

        [Fact]
        public void Launch_ShouldThrow_WhenCancelled()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Cancelled);

            var act = () => campaign.Launch();

            act.Should().Throw<BusinessRuleException>().WithMessage("*cancelled*");
        }
    }

    public class PauseTests
    {
        [Fact]
        public void Pause_ShouldSetStatusToPaused_WhenActive()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            campaign.Pause();

            campaign.Status.Should().Be(CampaignStatus.Paused);
        }

        [Fact]
        public void Pause_ShouldRaiseCampaignPausedEvent()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            campaign.Pause();

            campaign.DomainEvents.Should().ContainSingle(e => e is CampaignPausedEvent);
        }

        [Fact]
        public void Pause_ShouldThrow_WhenNotActive()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Draft);

            var act = () => campaign.Pause();

            act.Should().Throw<BusinessRuleException>().WithMessage("*only active*");
        }

        [Fact]
        public void Pause_ShouldThrow_WhenAlreadyPaused()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Paused);

            var act = () => campaign.Pause();

            act.Should().Throw<BusinessRuleException>().WithMessage("*only active*");
        }
    }

    public class CompleteTests
    {
        [Fact]
        public void Complete_ShouldSetStatusToCompleted_WhenActive()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            campaign.Complete();

            campaign.Status.Should().Be(CampaignStatus.Completed);
        }

        [Fact]
        public void Complete_ShouldSetCompletedAt()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            campaign.Complete();

            campaign.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public void Complete_ShouldRaiseCampaignCompletedEvent()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Active);

            campaign.Complete();

            campaign.DomainEvents.Should().ContainSingle(e => e is CampaignCompletedEvent);
        }

        [Fact]
        public void Complete_ShouldThrow_WhenAlreadyCompleted()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Completed);

            var act = () => campaign.Complete();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already completed*");
        }

        [Fact]
        public void Complete_ShouldThrow_WhenCancelled()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Cancelled);

            var act = () => campaign.Complete();

            act.Should().Throw<BusinessRuleException>().WithMessage("*cancelled*");
        }

        [Fact]
        public void Complete_ShouldThrow_WhenDraft()
        {
            var campaign = MarketingCampaign.CreateForTesting(CampaignStatus.Draft);

            var act = () => campaign.Complete();

            act.Should().Throw<BusinessRuleException>().WithMessage("*draft*never launched*");
        }
    }
}
