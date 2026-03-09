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

public class QuoteEntityTests
{
    public class ApproveTests
    {
        [Fact]
        public void Approve_ShouldSetStatusToApproved_WhenValid()
        {
            var quote = Quote.CreateForTesting();

            quote.Approve(42);

            quote.Status.Should().Be(QuoteStatus.Approved);
        }

        [Fact]
        public void Approve_ShouldSetIsApprovedTrue()
        {
            var quote = Quote.CreateForTesting();

            quote.Approve(42);

            quote.IsApproved.Should().BeTrue();
        }

        [Fact]
        public void Approve_ShouldSetApprovalDate()
        {
            var quote = Quote.CreateForTesting();

            quote.Approve(42);

            quote.ApprovalDate.Should().NotBeNull();
        }

        [Fact]
        public void Approve_ShouldRaiseQuoteApprovedEvent()
        {
            var quote = Quote.CreateForTesting();

            quote.Approve(42);

            quote.DomainEvents.Should().ContainSingle(e => e is QuoteApprovedEvent)
                .Which.Should().BeOfType<QuoteApprovedEvent>()
                .Which.ApprovedByUserId.Should().Be(42);
        }

        [Fact]
        public void Approve_ShouldThrow_WhenAlreadyApproved()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Approved);

            var act = () => quote.Approve(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*already approved*");
        }

        [Fact]
        public void Approve_ShouldThrow_WhenCancelled()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Cancelled);

            var act = () => quote.Approve(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*cancelled*");
        }

        [Fact]
        public void Approve_ShouldThrow_WhenConverted()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Converted);

            var act = () => quote.Approve(42);

            act.Should().Throw<BusinessRuleException>().WithMessage("*converted*");
        }
    }

    public class SendTests
    {
        [Fact]
        public void Send_ShouldSetStatusToShared_WhenNew()
        {
            var quote = Quote.CreateForTesting();

            quote.Send();

            quote.Status.Should().Be(QuoteStatus.Shared);
        }

        [Fact]
        public void Send_ShouldSetSentDate()
        {
            var quote = Quote.CreateForTesting();

            quote.Send();

            quote.SentDate.Should().NotBeNull();
        }

        [Fact]
        public void Send_ShouldRaiseQuoteSentEvent()
        {
            var quote = Quote.CreateForTesting();

            quote.Send();

            quote.DomainEvents.Should().ContainSingle(e => e is QuoteSentEvent);
        }

        [Fact]
        public void Send_ShouldThrow_WhenAlreadySent()
        {
            var quote = Quote.CreateForTesting();
            quote.Send();

            var act = () => quote.Send();

            act.Should().Throw<BusinessRuleException>().WithMessage("*must be*");
        }

        [Fact]
        public void Send_ShouldThrow_WhenInvalidStatus()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Cancelled);

            var act = () => quote.Send();

            act.Should().Throw<BusinessRuleException>().WithMessage("*must be*");
        }
    }

    public class RevokeTests
    {
        [Fact]
        public void Revoke_ShouldSetStatusToCancelled()
        {
            var quote = Quote.CreateForTesting();

            quote.Revoke("reason");

            quote.Status.Should().Be(QuoteStatus.Cancelled);
        }

        [Fact]
        public void Revoke_ShouldRaiseQuoteRevokedEvent()
        {
            var quote = Quote.CreateForTesting();

            quote.Revoke("out of stock");

            quote.DomainEvents.Should().ContainSingle(e => e is QuoteRevokedEvent)
                .Which.Should().BeOfType<QuoteRevokedEvent>()
                .Which.Reason.Should().Be("out of stock");
        }

        [Fact]
        public void Revoke_ShouldThrow_WhenReasonEmpty()
        {
            var quote = Quote.CreateForTesting();

            var act = () => quote.Revoke("");

            act.Should().Throw<BusinessRuleException>().WithMessage("*reason*");
        }

        [Fact]
        public void Revoke_ShouldThrow_WhenAlreadyCancelled()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Cancelled);

            var act = () => quote.Revoke("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*already cancelled*");
        }

        [Fact]
        public void Revoke_ShouldThrow_WhenConverted()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Converted);

            var act = () => quote.Revoke("reason");

            act.Should().Throw<BusinessRuleException>();
        }

        [Fact]
        public void Revoke_ShouldThrow_WhenAccepted()
        {
            var quote = Quote.CreateForTesting(QuoteStatus.Accepted);

            var act = () => quote.Revoke("reason");

            act.Should().Throw<BusinessRuleException>();
        }
    }
}
