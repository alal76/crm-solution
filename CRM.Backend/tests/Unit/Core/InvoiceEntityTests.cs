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

public class InvoiceEntityTests
{
    public class SendTests
    {
        [Fact]
        public void Send_ShouldSetStatusToSent_WhenDraft()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Send();

            invoice.Status.Should().Be(InvoiceStatus.Sent);
        }

        [Fact]
        public void Send_ShouldSetSentDate()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Send();

            invoice.SentDate.Should().NotBeNull();
        }

        [Fact]
        public void Send_ShouldRaiseInvoiceSentEvent()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Send();

            invoice.DomainEvents.Should().ContainSingle(e => e is InvoiceSentEvent);
        }

        [Fact]
        public void Send_ShouldThrow_WhenVoided()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Voided);

            var act = () => invoice.Send();

            act.Should().Throw<BusinessRuleException>().WithMessage("*voided*");
        }

        [Fact]
        public void Send_ShouldThrow_WhenAlreadySent()
        {
            var invoice = Invoice.CreateForTesting();
            invoice.Send();

            var act = () => invoice.Send();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already been sent*");
        }

        [Fact]
        public void Send_ShouldThrow_WhenInvalidStatus()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Paid);

            var act = () => invoice.Send();

            act.Should().Throw<BusinessRuleException>().WithMessage("*must be*");
        }
    }

    public class MarkPaidTests
    {
        [Fact]
        public void MarkPaid_ShouldSetStatusToPaid()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.MarkPaid();

            invoice.Status.Should().Be(InvoiceStatus.Paid);
        }

        [Fact]
        public void MarkPaid_ShouldSetPaidDate()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.MarkPaid();

            invoice.PaidDate.Should().NotBeNull();
        }

        [Fact]
        public void MarkPaid_ShouldSetAmountPaidToTotalAmount()
        {
            var invoice = Invoice.CreateForTesting(totalAmount: 1000m);

            invoice.MarkPaid();

            invoice.AmountPaid.Should().Be(1000m);
        }

        [Fact]
        public void MarkPaid_ShouldRaiseInvoiceMarkedPaidEvent()
        {
            var invoice = Invoice.CreateForTesting(totalAmount: 1000m);

            invoice.MarkPaid();

            invoice.DomainEvents.Should().ContainSingle(e => e is InvoiceMarkedPaidEvent)
                .Which.Should().BeOfType<InvoiceMarkedPaidEvent>()
                .Which.AmountPaid.Should().Be(1000m);
        }

        [Fact]
        public void MarkPaid_ShouldThrow_WhenAlreadyPaid()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Paid);

            var act = () => invoice.MarkPaid();

            act.Should().Throw<BusinessRuleException>().WithMessage("*already*paid*");
        }

        [Fact]
        public void MarkPaid_ShouldThrow_WhenVoided()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Voided);

            var act = () => invoice.MarkPaid();

            act.Should().Throw<BusinessRuleException>().WithMessage("*voided*");
        }
    }

    public class VoidTests
    {
        [Fact]
        public void Void_ShouldSetStatusToVoided()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Void("error");

            invoice.Status.Should().Be(InvoiceStatus.Voided);
        }

        [Fact]
        public void Void_ShouldSetVoidedDate()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Void("error");

            invoice.VoidedDate.Should().NotBeNull();
        }

        [Fact]
        public void Void_ShouldSetVoidReason()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Void("duplicate entry");

            invoice.VoidReason.Should().Be("duplicate entry");
        }

        [Fact]
        public void Void_ShouldRaiseInvoiceVoidedEvent()
        {
            var invoice = Invoice.CreateForTesting();

            invoice.Void("error");

            invoice.DomainEvents.Should().ContainSingle(e => e is InvoiceVoidedEvent)
                .Which.Should().BeOfType<InvoiceVoidedEvent>()
                .Which.Reason.Should().Be("error");
        }

        [Fact]
        public void Void_ShouldThrow_WhenReasonEmpty()
        {
            var invoice = Invoice.CreateForTesting();

            var act = () => invoice.Void("");

            act.Should().Throw<BusinessRuleException>().WithMessage("*reason*");
        }

        [Fact]
        public void Void_ShouldThrow_WhenAlreadyVoided()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Voided);

            var act = () => invoice.Void("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*already voided*");
        }

        [Fact]
        public void Void_ShouldThrow_WhenPaid()
        {
            var invoice = Invoice.CreateForTesting(InvoiceStatus.Paid);

            var act = () => invoice.Void("reason");

            act.Should().Throw<BusinessRuleException>().WithMessage("*paid*");
        }
    }
}
