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
/// AP-059-P1B: Zero-mock behavioral unit tests for the Opportunity entity.
/// Tests cover TransitionToStage, Close, UpdateExpectedRevenue, and StageProbabilityDefaults.
/// </summary>
public class OpportunityEntityTests
{
    private static Opportunity CreateOpportunity(string name = "Test Opportunity") =>
        new Opportunity { AccountId = 1, Name = name };

    #region TransitionToStage Tests

    [Fact]
    public void TransitionToStage_ShouldUpdateStage_WhenTransitionIsValid()
    {
        var opp = CreateOpportunity();

        opp.TransitionToStage(OpportunityStage.Qualification);

        opp.Stage.Should().Be(OpportunityStage.Qualification);
    }

    [Fact]
    public void TransitionToStage_ShouldSetDefaultProbability_WhenNoCustomProvided()
    {
        var opp = CreateOpportunity();

        opp.TransitionToStage(OpportunityStage.Qualification);

        opp.Probability.Should().Be(Opportunity.StageProbabilityDefaults[OpportunityStage.Qualification]);
    }

    [Fact]
    public void TransitionToStage_ShouldSetCustomProbability_WhenProvided()
    {
        var opp = CreateOpportunity();

        opp.TransitionToStage(OpportunityStage.Proposal, 55);

        opp.Probability.Should().Be(55);
    }

    [Fact]
    public void TransitionToStage_ShouldRaiseOpportunityStageChangedEvent()
    {
        var opp = CreateOpportunity();

        opp.TransitionToStage(OpportunityStage.Qualification);

        var evt = opp.DomainEvents.OfType<OpportunityStageChangedEvent>().Should().ContainSingle().Subject;
        evt.OldStage.Should().Be(OpportunityStage.Discovery);
        evt.NewStage.Should().Be(OpportunityStage.Qualification);
        evt.Probability.Should().Be(Opportunity.StageProbabilityDefaults[OpportunityStage.Qualification]);
    }

    [Fact]
    public void TransitionToStage_ShouldThrowBusinessRuleException_WhenAlreadyClosed()
    {
        var opp = CreateOpportunity();
        opp.Close(OpportunityStage.ClosedWon);

        var act = () => opp.TransitionToStage(OpportunityStage.Qualification);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*Cannot change stage of a closed opportunity*");
    }

    [Fact]
    public void TransitionToStage_ShouldThrowBusinessRuleException_WhenTargetIsClosedWonOrLost()
    {
        var opp = CreateOpportunity();

        var act = () => opp.TransitionToStage(OpportunityStage.ClosedWon);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*not valid for transition*");
    }

    #endregion

    #region Close Tests

    [Fact]
    public void Close_ShouldSetStageToClosedWon_WhenWon()
    {
        var opp = CreateOpportunity();

        opp.Close(OpportunityStage.ClosedWon);

        opp.Stage.Should().Be(OpportunityStage.ClosedWon);
    }

    [Fact]
    public void Close_ShouldSetStageToClosedLost_WhenLost()
    {
        var opp = CreateOpportunity();

        opp.Close(OpportunityStage.ClosedLost);

        opp.Stage.Should().Be(OpportunityStage.ClosedLost);
    }

    [Fact]
    public void Close_ShouldSetProbabilityTo100_WhenWon()
    {
        var opp = CreateOpportunity();

        opp.Close(OpportunityStage.ClosedWon);

        opp.Probability.Should().Be(100);
    }

    [Fact]
    public void Close_ShouldSetProbabilityTo0_WhenLost()
    {
        var opp = CreateOpportunity();

        opp.Close(OpportunityStage.ClosedLost);

        opp.Probability.Should().Be(0);
    }

    [Fact]
    public void Close_ShouldRaiseOpportunityClosedEvent()
    {
        var opp = CreateOpportunity();

        opp.Close(OpportunityStage.ClosedWon, "Great deal");

        var evt = opp.DomainEvents.OfType<OpportunityClosedEvent>().Should().ContainSingle().Subject;
        evt.FinalStage.Should().Be(OpportunityStage.ClosedWon);
        evt.Reason.Should().Be("Great deal");
    }

    [Fact]
    public void Close_ShouldThrowBusinessRuleException_WhenAlreadyClosed()
    {
        var opp = CreateOpportunity();
        opp.Close(OpportunityStage.ClosedWon);

        var act = () => opp.Close(OpportunityStage.ClosedWon);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*already closed*");
    }

    [Fact]
    public void Close_ShouldThrowBusinessRuleException_WhenInvalidCloseStage()
    {
        var opp = CreateOpportunity();

        var act = () => opp.Close(OpportunityStage.Proposal);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*ClosedWon or ClosedLost*");
    }

    #endregion

    #region UpdateExpectedRevenue Tests

    [Fact]
    public void UpdateExpectedRevenue_ShouldUpdateRevenueAndDate()
    {
        var opp = CreateOpportunity();
        var date = new DateTime(2026, 12, 31);

        opp.UpdateExpectedRevenue(50000m, date);

        opp.Amount.Should().Be(50000m);
        opp.ExpectedCloseDate.Should().Be(date);
    }

    [Fact]
    public void UpdateExpectedRevenue_ShouldRaiseOpportunityRevenueUpdatedEvent()
    {
        var opp = CreateOpportunity();
        var date = new DateTime(2026, 12, 31);

        opp.UpdateExpectedRevenue(50000m, date);

        var evt = opp.DomainEvents.OfType<OpportunityRevenueUpdatedEvent>().Should().ContainSingle().Subject;
        evt.Amount.Should().Be(50000m);
        evt.ExpectedCloseDate.Should().Be(date);
    }

    [Fact]
    public void UpdateExpectedRevenue_ShouldThrowBusinessRuleException_WhenAmountIsNegative()
    {
        var opp = CreateOpportunity();

        var act = () => opp.UpdateExpectedRevenue(-1m, DateTime.UtcNow);

        act.Should().Throw<BusinessRuleException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public void UpdateExpectedRevenue_ShouldSetUpdatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var opp = CreateOpportunity();

        opp.UpdateExpectedRevenue(1000m, DateTime.UtcNow.AddDays(30));

        opp.UpdatedAt.Should().NotBeNull();
        opp.UpdatedAt!.Value.Should().BeAfter(before);
    }

    #endregion

    #region StageProbabilityDefaults Tests

    [Fact]
    public void StageProbabilityDefaults_ShouldReturnCorrectProbabilityForDiscovery()
    {
        Opportunity.StageProbabilityDefaults[OpportunityStage.Discovery].Should().Be(10);
    }

    [Fact]
    public void StageProbabilityDefaults_ShouldReturn100_ForClosedWon()
    {
        Opportunity.StageProbabilityDefaults[OpportunityStage.ClosedWon].Should().Be(100);
    }

    [Fact]
    public void StageProbabilityDefaults_ShouldReturn0_ForClosedLost()
    {
        Opportunity.StageProbabilityDefaults[OpportunityStage.ClosedLost].Should().Be(0);
    }

    #endregion
}
