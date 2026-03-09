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

public class SubscriptionEntityTests
{
    public class CancelTests
    {
        [Fact]
        public void Cancel_ShouldSetStatusToCancelled()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            subscription.Cancel("too expensive");

            subscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Cancelled);
        }

        [Fact]
        public void Cancel_ShouldSetIsActiveFalse()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            subscription.Cancel("too expensive");

            subscription.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Cancel_ShouldSetCancelledAt()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            subscription.Cancel("too expensive");

            subscription.CancelledAt.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_ShouldSetCancellationReason()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            subscription.Cancel("too expensive");

            subscription.CancellationReason.Should().Be("too expensive");
        }

        [Fact]
        public void Cancel_ShouldRaiseSubscriptionCancelledEvent()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            subscription.Cancel("too expensive");

            subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionCancelledEvent);
            var evt = subscription.DomainEvents.OfType<SubscriptionCancelledEvent>().Single();
            evt.Reason.Should().Be("too expensive");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenReasonEmpty()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            var act = () => subscription.Cancel("");

            act.Should().Throw<BusinessRuleException>().WithMessage("*reason*");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenAlreadyCancelled()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            var act = () => subscription.Cancel("too expensive");

            act.Should().Throw<BusinessRuleException>().WithMessage("*already cancelled*");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenExpired()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Expired, false);

            var act = () => subscription.Cancel("too expensive");

            act.Should().Throw<BusinessRuleException>().WithMessage("*expired*");
        }
    }

    public class ReinstateTests
    {
        [Fact]
        public void Reinstate_ShouldSetStatusToActive_WhenCancelled()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            subscription.Reinstate();

            subscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        }

        [Fact]
        public void Reinstate_ShouldSetIsActiveTrue()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            subscription.Reinstate();

            subscription.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Reinstate_ShouldClearCancellationFields()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            subscription.Reinstate();

            subscription.CancelledAt.Should().BeNull();
            subscription.CancellationReason.Should().BeNull();
            subscription.CancelAtPeriodEnd.Should().BeFalse();
        }

        [Fact]
        public void Reinstate_ShouldClearPauseFields()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            subscription.Reinstate();

            subscription.PausedAt.Should().BeNull();
            subscription.ResumeAt.Should().BeNull();
        }

        [Fact]
        public void Reinstate_ShouldRaiseSubscriptionReinstatedEvent()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Cancelled, false);

            subscription.Reinstate();

            subscription.DomainEvents.Should().ContainSingle(e => e is SubscriptionReinstatedEvent);
        }

        [Fact]
        public void Reinstate_ShouldThrow_WhenAlreadyActive()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Active, true);

            var act = () => subscription.Reinstate();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already active*");
        }

        [Fact]
        public void Reinstate_ShouldThrow_WhenExpired()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Expired, false);

            var act = () => subscription.Reinstate();

            act.Should().Throw<BusinessRuleException>().WithMessage("*expired*");
        }

        [Fact]
        public void Reinstate_ShouldWork_WhenPaused()
        {
            var subscription = Subscription.CreateForTesting(SubscriptionStatus.Paused, true);

            subscription.Reinstate();

            subscription.SubscriptionStatus.Should().Be(SubscriptionStatus.Active);
        }
    }
}
