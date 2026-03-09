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

public class SLAPolicyEntityTests
{
    public class ActivateTests
    {
        [Fact]
        public void Activate_ShouldSetIsActiveTrue_WhenInactive()
        {
            var policy = SLAPolicy.CreateForTesting(false);

            policy.Activate();

            policy.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Activate_ShouldRaiseSLAPolicyActivatedEvent()
        {
            var policy = SLAPolicy.CreateForTesting(false);

            policy.Activate();

            policy.DomainEvents.Should().ContainSingle(e => e is SLAPolicyActivatedEvent);
        }

        [Fact]
        public void Activate_ShouldThrow_WhenAlreadyActive()
        {
            var policy = SLAPolicy.CreateForTesting(true);

            var act = () => policy.Activate();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already active*");
        }
    }

    public class DeactivateTests
    {
        [Fact]
        public void Deactivate_ShouldSetIsActiveFalse_WhenActive()
        {
            var policy = SLAPolicy.CreateForTesting(true);

            policy.Deactivate("maintenance");

            policy.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Deactivate_ShouldRaiseSLAPolicyDeactivatedEvent()
        {
            var policy = SLAPolicy.CreateForTesting(true);

            policy.Deactivate("maintenance");

            policy.DomainEvents.Should().ContainSingle(e => e is SLAPolicyDeactivatedEvent);
            var evt = policy.DomainEvents.OfType<SLAPolicyDeactivatedEvent>().Single();
            evt.Reason.Should().Be("maintenance");
        }

        [Fact]
        public void Deactivate_ShouldThrow_WhenReasonEmpty()
        {
            var policy = SLAPolicy.CreateForTesting(true);

            var act = () => policy.Deactivate("");

            act.Should().Throw<BusinessRuleException>().WithMessage("*reason*");
        }

        [Fact]
        public void Deactivate_ShouldThrow_WhenAlreadyInactive()
        {
            var policy = SLAPolicy.CreateForTesting(false);

            var act = () => policy.Deactivate("maintenance");

            act.Should().Throw<BusinessRuleException>().WithMessage("*already inactive*");
        }
    }
}
