// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using CRM.Core.Ports.Output.Events;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// AP-059-P2A: Zero-mock behavioral unit tests for Lead entity business methods.
/// All tests create Lead instances directly — no mocks, no service layer.
/// </summary>
public class LeadEntityTests
{
    // ─── helpers ────────────────────────────────────────────────────────────────

    private static Lead NewLead() => new Lead
    {
        FirstName = "Jane",
        LastName  = "Doe",
        Email     = "jane.doe@example.com"
    };

    private static Lead LeadInStatus(LeadLifecycleStatus status)
        => Lead.CreateForTesting(status);

    // ─── ConvertToOpportunity ────────────────────────────────────────────────────

    public class ConvertToOpportunityTests
    {
        [Fact]
        public void ConvertToOpportunity_ShouldSetStatusToConverted_WhenValid()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(1, "Acme - Opportunity");
            lead.Status.Should().Be(LeadLifecycleStatus.Converted);
        }

        [Fact]
        public void ConvertToOpportunity_ShouldSetConvertedDate()
        {
            var before = DateTime.UtcNow.AddSeconds(-1);
            var lead   = NewLead();
            lead.ConvertToOpportunity(1, "Acme - Opportunity");
            // UpdatedAt is set by the entity method
            lead.UpdatedAt.Should().BeOnOrAfter(before);
        }

        [Fact]
        public void ConvertToOpportunity_ShouldRaiseLeadConvertedEvent()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(42, "Acme - Deal");

            lead.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<LeadConvertedEvent>();

            var evt = (LeadConvertedEvent)lead.DomainEvents.Single();
            evt.AccountId.Should().Be(42);
            evt.OpportunityTitle.Should().Be("Acme - Deal");
        }

        [Fact]
        public void ConvertToOpportunity_ShouldThrowBusinessRuleException_WhenAlreadyConverted()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(1, "First");

            var act = () => lead.ConvertToOpportunity(1, "Second");
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*already converted*");
        }

        [Fact]
        public void ConvertToOpportunity_ShouldThrowBusinessRuleException_WhenDisqualified()
        {
            var lead = LeadInStatus(LeadLifecycleStatus.Disqualified);

            var act = () => lead.ConvertToOpportunity(1, "Acme");
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*disqualified*");
        }
    }

    // ─── Disqualify ──────────────────────────────────────────────────────────────

    public class DisqualifyTests
    {
        [Fact]
        public void Disqualify_ShouldSetStatusToDisqualified()
        {
            var lead = NewLead();
            lead.Disqualify("Not a fit");
            lead.Status.Should().Be(LeadLifecycleStatus.Disqualified);
        }

        [Fact]
        public void Disqualify_ShouldRaiseLeadDisqualifiedEvent()
        {
            var lead = NewLead();
            lead.Disqualify("Budget too small");

            lead.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<LeadDisqualifiedEvent>();

            var evt = (LeadDisqualifiedEvent)lead.DomainEvents.Single();
            evt.Reason.Should().Be("Budget too small");
        }

        [Fact]
        public void Disqualify_ShouldThrowBusinessRuleException_WhenAlreadyDisqualified()
        {
            var lead = LeadInStatus(LeadLifecycleStatus.Disqualified);

            var act = () => lead.Disqualify("Again");
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*already disqualified*");
        }

        [Fact]
        public void Disqualify_ShouldThrowBusinessRuleException_WhenAlreadyConverted()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(1, "Deal");

            var act = () => lead.Disqualify("Too late");
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*converted*");
        }
    }

    // ─── Qualify ─────────────────────────────────────────────────────────────────

    public class QualifyTests
    {
        [Fact]
        public void Qualify_ShouldSetStatusToQualified_WhenNew()
        {
            var lead = NewLead();
            lead.Qualify(75);
            lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
        }

        [Fact]
        public void Qualify_ShouldSetLeadScore_WhenQualified()
        {
            var lead = NewLead();
            lead.Qualify(80);
            lead.Score.Should().Be(80);
        }

        [Fact]
        public void Qualify_ShouldRaiseLeadQualifiedEvent()
        {
            var lead = NewLead();
            lead.Qualify(90);

            lead.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<LeadQualifiedEvent>();

            var evt = (LeadQualifiedEvent)lead.DomainEvents.Single();
            evt.Score.Should().Be(90);
        }

        [Fact]
        public void Qualify_ShouldThrowBusinessRuleException_WhenAlreadyConverted()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(1, "Deal");

            var act = () => lead.Qualify(50);
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*converted or disqualified*");
        }

        [Fact]
        public void Qualify_ShouldThrowBusinessRuleException_WhenScoreIsZeroOrNegative()
        {
            var lead = NewLead();

            var actZero     = () => lead.Qualify(0);
            var actNegative = () => lead.Qualify(-5);

            actZero.Should().Throw<BusinessRuleException>()
                   .WithMessage("*score must be positive*");
            actNegative.Should().Throw<BusinessRuleException>()
                       .WithMessage("*score must be positive*");
        }
    }

    // ─── Assign ──────────────────────────────────────────────────────────────────

    public class AssignTests
    {
        [Fact]
        public void Assign_ShouldSetAssignedUserId_WhenValid()
        {
            var lead = NewLead();
            lead.Assign(7);
            lead.OwnerId.Should().Be(7);
        }

        [Fact]
        public void Assign_ShouldRaiseLeadAssignedEvent()
        {
            var lead = NewLead();
            lead.Assign(7);

            lead.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<LeadAssignedEvent>();

            var evt = (LeadAssignedEvent)lead.DomainEvents.Single();
            evt.OwnerId.Should().Be(7);
        }

        [Fact]
        public void Assign_ShouldThrowBusinessRuleException_WhenOwnerIdIsZero()
        {
            var lead = NewLead();
            var act  = () => lead.Assign(0);
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*Owner ID must be positive*");
        }

        [Fact]
        public void Assign_ShouldThrowBusinessRuleException_WhenAlreadyConverted()
        {
            var lead = NewLead();
            lead.ConvertToOpportunity(1, "Deal");

            var act = () => lead.Assign(5);
            act.Should().Throw<BusinessRuleException>()
               .WithMessage("*converted*");
        }
    }
}
