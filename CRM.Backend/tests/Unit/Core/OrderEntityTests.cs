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

public class OrderEntityTests
{
    public class ConfirmTests
    {
        [Fact]
        public void Confirm_ShouldSetStatusToApproved_WhenDraft()
        {
            var order = Order.CreateForTesting();

            order.Confirm(42);

            order.Status.Should().Be(OrderStatus.Approved);
        }

        [Fact]
        public void Confirm_ShouldSetApprovedDate()
        {
            var order = Order.CreateForTesting();

            order.Confirm(42);

            order.ApprovedDate.Should().NotBeNull();
        }

        [Fact]
        public void Confirm_ShouldSetApprovedById()
        {
            var order = Order.CreateForTesting();

            order.Confirm(42);

            order.ApprovedById.Should().Be(42);
        }

        [Fact]
        public void Confirm_ShouldRaiseOrderConfirmedEvent()
        {
            var order = Order.CreateForTesting();

            order.Confirm(42);

            order.DomainEvents.Should().ContainSingle(e => e is OrderConfirmedEvent);
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenAlreadyApproved()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            var act = () => order.Confirm(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*already confirmed*");
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenCancelled()
        {
            var order = Order.CreateForTesting(OrderStatus.Cancelled);

            var act = () => order.Confirm(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*cancelled*");
        }

        [Fact]
        public void Confirm_ShouldThrow_WhenCompleted()
        {
            var order = Order.CreateForTesting(OrderStatus.Completed);

            var act = () => order.Confirm(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*completed*");
        }
    }

    public class ShipTests
    {
        [Fact]
        public void Ship_ShouldSetStatusToFulfilled_WhenApproved()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            order.Ship();

            order.Status.Should().Be(OrderStatus.Fulfilled);
        }

        [Fact]
        public void Ship_ShouldSetShippedDate()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            order.Ship();

            order.ShippedDate.Should().NotBeNull();
        }

        [Fact]
        public void Ship_ShouldSetTrackingNumber()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            order.Ship("TRACK123");

            order.TrackingNumber.Should().Be("TRACK123");
        }

        [Fact]
        public void Ship_ShouldRaiseOrderShippedEvent()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            order.Ship("TRACK123");

            order.DomainEvents.Should().ContainSingle(e => e is OrderShippedEvent)
                .Which.Should().BeOfType<OrderShippedEvent>()
                .Which.TrackingNumber.Should().Be("TRACK123");
        }

        [Fact]
        public void Ship_ShouldAllowNullTrackingNumber()
        {
            var order = Order.CreateForTesting(OrderStatus.Approved);

            order.Ship(null);

            order.Status.Should().Be(OrderStatus.Fulfilled);
            order.TrackingNumber.Should().BeNull();
        }

        [Fact]
        public void Ship_ShouldThrow_WhenDraft()
        {
            var order = Order.CreateForTesting(OrderStatus.Draft);

            var act = () => order.Ship();

            act.Should().Throw<BusinessRuleException>().WithMessage("*must be*");
        }

        [Fact]
        public void Ship_ShouldThrow_WhenCancelled()
        {
            var order = Order.CreateForTesting(OrderStatus.Cancelled);

            var act = () => order.Ship();

            act.Should().Throw<BusinessRuleException>();
        }
    }

    public class CancelTests
    {
        [Fact]
        public void Cancel_ShouldSetStatusToCancelled()
        {
            var order = Order.CreateForTesting();

            order.Cancel("reason");

            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        [Fact]
        public void Cancel_ShouldSetCancelledDate()
        {
            var order = Order.CreateForTesting();

            order.Cancel("reason");

            order.CancelledDate.Should().NotBeNull();
        }

        [Fact]
        public void Cancel_ShouldSetCancellationReason()
        {
            var order = Order.CreateForTesting();

            order.Cancel("customer request");

            order.CancellationReason.Should().Be("customer request");
        }

        [Fact]
        public void Cancel_ShouldRaiseOrderCancelledEvent()
        {
            var order = Order.CreateForTesting();

            order.Cancel("reason");

            order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenReasonEmpty()
        {
            var order = Order.CreateForTesting();

            var act = () => order.Cancel("");

            act.Should().Throw<BusinessRuleException>().WithMessage("*reason*");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenAlreadyCancelled()
        {
            var order = Order.CreateForTesting(OrderStatus.Cancelled);

            var act = () => order.Cancel("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*already cancelled*");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenCompleted()
        {
            var order = Order.CreateForTesting(OrderStatus.Completed);

            var act = () => order.Cancel("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*completed*");
        }

        [Fact]
        public void Cancel_ShouldThrow_WhenRefunded()
        {
            var order = Order.CreateForTesting(OrderStatus.Refunded);

            var act = () => order.Cancel("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*refunded*");
        }
    }
}
