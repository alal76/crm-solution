// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Linq;
using CRM.Core.Entities;
using CRM.Core.Entities.Events;
using CRM.Core.Exceptions;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// AP-059-P2C: 16 zero-mock behavioral tests for Contract entity domain methods.
/// Covers Approve, Renew, Terminate, and Expire business methods.
/// </summary>
public class ContractEntityTests
{
    #region Approve Tests

    [Fact]
    public void Approve_ShouldSetStatusToApproved_WhenPendingApproval()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.PendingApproval);

        // Act
        contract.Approve(42);

        // Assert
        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldSetApprovedAt_WhenApproved()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.PendingApproval);
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        contract.Approve(42);

        // Assert
        contract.ApprovedDate.Should().NotBeNull();
        contract.ApprovedDate!.Value.Should().BeAfter(before);
        contract.ApprovedByUserId.Should().Be(42);
    }

    [Fact]
    public void Approve_ShouldRaiseContractApprovedEvent()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.PendingApproval);

        // Act
        contract.Approve(7);

        // Assert
        var evt = contract.DomainEvents.OfType<ContractApprovedEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.ContractId.Should().Be(contract.Id);
        evt.ApprovedByUserId.Should().Be(7);
    }

    [Fact]
    public void Approve_ShouldThrowBusinessRuleException_WhenNotPendingApproval()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Draft);

        // Act
        var act = () => contract.Approve(1);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Only contracts in PendingApproval status can be approved.*");
    }

    #endregion

    #region Renew Tests

    [Fact]
    public void Renew_ShouldSetStatusToRenewed_WhenActive()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);
        var newEnd = DateTime.UtcNow.AddYears(2);

        // Act
        contract.Renew(newEnd);

        // Assert
        contract.Status.Should().Be(ContractStatus.Renewed);
    }

    [Fact]
    public void Renew_ShouldUpdateEndDate_WhenRenewed()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);
        var newEnd = DateTime.UtcNow.AddYears(2);

        // Act
        contract.Renew(newEnd);

        // Assert
        contract.EndDate.Should().Be(newEnd);
        contract.RenewalCompletedAt.Should().NotBeNull();
        contract.RenewalCompletedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Renew_ShouldRaiseContractRenewedEvent()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);
        var newEnd = DateTime.UtcNow.AddYears(1);

        // Act
        contract.Renew(newEnd);

        // Assert
        var evt = contract.DomainEvents.OfType<ContractRenewedEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.ContractId.Should().Be(contract.Id);
        evt.NewEndDate.Should().Be(newEnd);
    }

    [Fact]
    public void Renew_ShouldThrowBusinessRuleException_WhenNotActiveOrExpired()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Draft);
        var newEnd = DateTime.UtcNow.AddYears(1);

        // Act
        var act = () => contract.Renew(newEnd);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Only active or expired contracts can be renewed.*");
    }

    #endregion

    #region Terminate Tests

    [Fact]
    public void Terminate_ShouldSetStatusToTerminated_WhenActive()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);

        // Act
        contract.Terminate("Breach of terms", 5);

        // Assert
        contract.Status.Should().Be(ContractStatus.Terminated);
    }

    [Fact]
    public void Terminate_ShouldSetTerminationReason()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);

        // Act
        contract.Terminate("Breach of terms", 5);

        // Assert
        contract.TerminationReason.Should().Be("Breach of terms");
        contract.TerminatedDate.Should().NotBeNull();
        contract.TerminatedDate!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Terminate_ShouldRaiseContractTerminatedEvent()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);

        // Act
        contract.Terminate("Budget cut", 9);

        // Assert
        var evt = contract.DomainEvents.OfType<ContractTerminatedEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.ContractId.Should().Be(contract.Id);
        evt.Reason.Should().Be("Budget cut");
        evt.TerminatedByUserId.Should().Be(9);
    }

    [Fact]
    public void Terminate_ShouldThrowBusinessRuleException_WhenAlreadyTerminated()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Terminated);

        // Act
        var act = () => contract.Terminate("Double terminate", 1);

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Already terminated contract cannot be terminated again.*");
    }

    #endregion

    #region Expire Tests

    [Fact]
    public void Expire_ShouldSetStatusToExpired_WhenActive()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);

        // Act
        contract.Expire();

        // Assert
        contract.Status.Should().Be(ContractStatus.Expired);
    }

    [Fact]
    public void Expire_ShouldSetUpdatedAt()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Active);
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        contract.Expire();

        // Assert
        contract.UpdatedAt.Should().NotBeNull();
        contract.UpdatedAt!.Value.Should().BeAfter(before);
    }

    [Fact]
    public void Expire_ShouldRaiseContractExpiredEvent()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Approved);

        // Act
        contract.Expire();

        // Assert
        var evt = contract.DomainEvents.OfType<ContractExpiredEvent>().SingleOrDefault();
        evt.Should().NotBeNull();
        evt!.ContractId.Should().Be(contract.Id);
    }

    [Fact]
    public void Expire_ShouldThrowBusinessRuleException_WhenNotActiveOrApproved()
    {
        // Arrange
        var contract = Contract.CreateForTesting(ContractStatus.Draft);

        // Act
        var act = () => contract.Expire();

        // Assert
        act.Should().Throw<BusinessRuleException>()
           .WithMessage("*Contract is not in a state that can expire.*");
    }

    #endregion
}
