// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Models;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for E-Signature sub-entities (Signer, Document, AuditEvent),
/// Order/Invoice line items, and Sales Forecast entities.
/// </summary>
public class SignerDocumentLineItemForecastEntityTests
{
    #region ESignatureSigner Entity Tests

    [Fact]
    public void ESignatureSigner_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var signer = new ESignatureSigner();

        // Assert
        signer.SigningOrder.Should().Be(1);
        signer.Role.Should().Be(SignerRole.Signer);
        signer.Status.Should().Be(SignerStatus.Waiting);
        signer.Name.Should().BeEmpty();
        signer.Email.Should().BeEmpty();
        signer.Phone.Should().BeNull();
        signer.Title.Should().BeNull();
        signer.Company.Should().BeNull();
        signer.ExternalRecipientId.Should().BeNull();
        signer.SentDate.Should().BeNull();
        signer.DeliveredDate.Should().BeNull();
        signer.ViewedDate.Should().BeNull();
        signer.SignedDate.Should().BeNull();
        signer.DeclinedDate.Should().BeNull();
        signer.SignatureImageUrl.Should().BeNull();
        signer.SignedFromIp.Should().BeNull();
        signer.SignedFromLocation.Should().BeNull();
        signer.DeclineReason.Should().BeNull();
        signer.PrivateMessage.Should().BeNull();
    }

    [Fact]
    public void ESignatureSigner_CanSetProperties()
    {
        // Arrange
        var signer = new ESignatureSigner();
        var sentDate = DateTime.UtcNow.AddDays(-5);
        var signedDate = DateTime.UtcNow.AddDays(-3);

        // Act
        signer.SigningOrder = 2;
        signer.Role = SignerRole.CounterSigner;
        signer.Status = SignerStatus.Signed;
        signer.ExternalRecipientId = "EXT-RECIP-123";
        signer.Name = "John Doe";
        signer.Email = "john.doe@example.com";
        signer.Phone = "+1-555-123-4567";
        signer.Title = "VP of Sales";
        signer.Company = "Acme Corp";
        signer.SentDate = sentDate;
        signer.SignedDate = signedDate;
        signer.SignatureImageUrl = "https://example.com/sig.png";
        signer.SignedFromIp = "192.168.1.100";
        signer.SignedFromLocation = "San Francisco, CA";
        signer.PrivateMessage = "Please review and sign ASAP";
        signer.ESignatureRequestId = 42;
        signer.ContactId = 100;
        signer.UserId = 5;

        // Assert
        signer.SigningOrder.Should().Be(2);
        signer.Role.Should().Be(SignerRole.CounterSigner);
        signer.Status.Should().Be(SignerStatus.Signed);
        signer.ExternalRecipientId.Should().Be("EXT-RECIP-123");
        signer.Name.Should().Be("John Doe");
        signer.Email.Should().Be("john.doe@example.com");
        signer.Phone.Should().Be("+1-555-123-4567");
        signer.Title.Should().Be("VP of Sales");
        signer.Company.Should().Be("Acme Corp");
        signer.SentDate.Should().Be(sentDate);
        signer.SignedDate.Should().Be(signedDate);
        signer.SignatureImageUrl.Should().Be("https://example.com/sig.png");
        signer.SignedFromIp.Should().Be("192.168.1.100");
        signer.SignedFromLocation.Should().Be("San Francisco, CA");
        signer.PrivateMessage.Should().Be("Please review and sign ASAP");
        signer.ESignatureRequestId.Should().Be(42);
        signer.ContactId.Should().Be(100);
        signer.UserId.Should().Be(5);
    }

    [Fact]
    public void ESignatureSigner_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var signer = new ESignatureSigner();
        var request = new ESignatureRequest { Id = 1, Name = "Test Request" };
        var contact = new Contact { Id = 2, FirstName = "Jane" };
        var user = new User { Id = 3, FirstName = "Admin" };

        // Act
        signer.ESignatureRequest = request;
        signer.Contact = contact;
        signer.User = user;

        // Assert
        signer.ESignatureRequest.Should().BeSameAs(request);
        signer.Contact.Should().BeSameAs(contact);
        signer.User.Should().BeSameAs(user);
    }

    [Theory]
    [InlineData(SignerStatus.Waiting, "Waiting to sign")]
    [InlineData(SignerStatus.Pending, "Their turn to sign")]
    [InlineData(SignerStatus.Sent, "Email sent")]
    [InlineData(SignerStatus.Delivered, "Delivered")]
    [InlineData(SignerStatus.Viewed, "Document viewed")]
    [InlineData(SignerStatus.Signed, "Completed")]
    [InlineData(SignerStatus.Declined, "Declined")]
    [InlineData(SignerStatus.DeliveryFailed, "Delivery failed")]
    [InlineData(SignerStatus.AuthFailed, "Authentication failed")]
    public void SignerStatus_ShouldHaveAllValues(SignerStatus status, string description)
    {
        // Arrange & Act
        var signer = new ESignatureSigner { Status = status };

        // Assert
        signer.Status.Should().Be(status);
        description.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(SignerRole.Signer, "Primary signer")]
    [InlineData(SignerRole.CoSigner, "Co-signer")]
    [InlineData(SignerRole.CounterSigner, "Internal counter-signer")]
    [InlineData(SignerRole.CarbonCopy, "CC only")]
    [InlineData(SignerRole.Witness, "Witness")]
    [InlineData(SignerRole.Approver, "Internal approver")]
    [InlineData(SignerRole.InPersonSigner, "In-person")]
    public void SignerRole_ShouldHaveAllValues(SignerRole role, string description)
    {
        // Arrange & Act
        var signer = new ESignatureSigner { Role = role };

        // Assert
        signer.Role.Should().Be(role);
        description.Should().NotBeEmpty();
    }

    [Fact]
    public void ESignatureSigner_DeclineScenario_ShouldTrackCorrectly()
    {
        // Arrange
        var signer = new ESignatureSigner
        {
            Name = "Reluctant Signer",
            Email = "reluctant@example.com",
            SentDate = DateTime.UtcNow.AddDays(-3),
            ViewedDate = DateTime.UtcNow.AddDays(-2)
        };

        // Act - Signer declines
        signer.Status = SignerStatus.Declined;
        signer.DeclinedDate = DateTime.UtcNow;
        signer.DeclineReason = "Terms not acceptable";

        // Assert
        signer.Status.Should().Be(SignerStatus.Declined);
        signer.DeclinedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        signer.DeclineReason.Should().Be("Terms not acceptable");
        signer.SignedDate.Should().BeNull();
    }

    #endregion

    #region ESignatureDocument Entity Tests

    [Fact]
    public void ESignatureDocument_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var doc = new ESignatureDocument();

        // Assert
        doc.Name.Should().BeEmpty();
        doc.DocumentOrder.Should().Be(1);
        doc.ExternalDocumentId.Should().BeNull();
        doc.DocumentUrl.Should().BeNull();
        doc.FileType.Should().BeNull();
        doc.FileSize.Should().BeNull();
        doc.PageCount.Should().BeNull();
    }

    [Fact]
    public void ESignatureDocument_CanSetProperties()
    {
        // Arrange
        var doc = new ESignatureDocument();

        // Act
        doc.Name = "Master Service Agreement";
        doc.DocumentOrder = 1;
        doc.ExternalDocumentId = "DOC-EXT-456";
        doc.DocumentUrl = "https://storage.example.com/docs/msa.pdf";
        doc.FileType = "pdf";
        doc.FileSize = 2048576; // 2 MB
        doc.PageCount = 15;
        doc.ESignatureRequestId = 99;

        // Assert
        doc.Name.Should().Be("Master Service Agreement");
        doc.DocumentOrder.Should().Be(1);
        doc.ExternalDocumentId.Should().Be("DOC-EXT-456");
        doc.DocumentUrl.Should().Be("https://storage.example.com/docs/msa.pdf");
        doc.FileType.Should().Be("pdf");
        doc.FileSize.Should().Be(2048576);
        doc.PageCount.Should().Be(15);
        doc.ESignatureRequestId.Should().Be(99);
    }

    [Fact]
    public void ESignatureDocument_NavigationProperty_ShouldBeSettable()
    {
        // Arrange
        var doc = new ESignatureDocument();
        var request = new ESignatureRequest { Id = 1, Name = "Contract Package" };

        // Act
        doc.ESignatureRequest = request;

        // Assert
        doc.ESignatureRequest.Should().BeSameAs(request);
    }

    [Theory]
    [InlineData("pdf", "application/pdf")]
    [InlineData("docx", "Microsoft Word")]
    [InlineData("doc", "Legacy Word")]
    public void ESignatureDocument_FileTypes_ShouldSupportCommonFormats(string fileType, string description)
    {
        // Arrange & Act
        var doc = new ESignatureDocument { FileType = fileType };

        // Assert
        doc.FileType.Should().Be(fileType);
        description.Should().NotBeEmpty();
    }

    #endregion

    #region ESignatureAuditEvent Entity Tests

    [Fact]
    public void ESignatureAuditEvent_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var evt = new ESignatureAuditEvent();

        // Assert
        evt.EventType.Should().BeEmpty();
        evt.EventDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        evt.Description.Should().BeNull();
        evt.IpAddress.Should().BeNull();
        evt.UserAgent.Should().BeNull();
        evt.Location.Should().BeNull();
        evt.ESignatureSignerId.Should().BeNull();
    }

    [Fact]
    public void ESignatureAuditEvent_CanSetProperties()
    {
        // Arrange
        var evt = new ESignatureAuditEvent();
        var eventDate = DateTime.UtcNow.AddHours(-2);

        // Act
        evt.EventType = "document_viewed";
        evt.EventDate = eventDate;
        evt.Description = "John Doe viewed the document";
        evt.IpAddress = "203.0.113.50";
        evt.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        evt.Location = "New York, NY, US";
        evt.ESignatureSignerId = 5;
        evt.ESignatureRequestId = 42;

        // Assert
        evt.EventType.Should().Be("document_viewed");
        evt.EventDate.Should().Be(eventDate);
        evt.Description.Should().Be("John Doe viewed the document");
        evt.IpAddress.Should().Be("203.0.113.50");
        evt.UserAgent.Should().Contain("Mozilla");
        evt.Location.Should().Be("New York, NY, US");
        evt.ESignatureSignerId.Should().Be(5);
        evt.ESignatureRequestId.Should().Be(42);
    }

    [Fact]
    public void ESignatureAuditEvent_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var evt = new ESignatureAuditEvent();
        var signer = new ESignatureSigner { Id = 1, Name = "Test Signer" };
        var request = new ESignatureRequest { Id = 2, Name = "Test Request" };

        // Act
        evt.ESignatureSigner = signer;
        evt.ESignatureRequest = request;

        // Assert
        evt.ESignatureSigner.Should().BeSameAs(signer);
        evt.ESignatureRequest.Should().BeSameAs(request);
    }

    [Theory]
    [InlineData("envelope_created")]
    [InlineData("envelope_sent")]
    [InlineData("envelope_delivered")]
    [InlineData("document_viewed")]
    [InlineData("document_signed")]
    [InlineData("envelope_completed")]
    [InlineData("envelope_declined")]
    [InlineData("envelope_voided")]
    public void ESignatureAuditEvent_CommonEventTypes_ShouldBeValid(string eventType)
    {
        // Arrange & Act
        var evt = new ESignatureAuditEvent { EventType = eventType };

        // Assert
        evt.EventType.Should().Be(eventType);
    }

    #endregion

    #region OrderLineItem Entity Tests

    [Fact]
    public void OrderLineItem_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var lineItem = new OrderLineItem();

        // Assert
        lineItem.LineNumber.Should().Be(0);
        lineItem.Name.Should().BeEmpty();
        lineItem.Description.Should().BeNull();
        lineItem.SKU.Should().BeNull();
        lineItem.ProductCode.Should().BeNull();
        lineItem.Quantity.Should().Be(1);
        lineItem.UnitOfMeasure.Should().BeNull();
        lineItem.UnitPrice.Should().Be(0);
        lineItem.UnitCost.Should().BeNull();
        lineItem.DiscountAmount.Should().Be(0);
        lineItem.DiscountPercent.Should().Be(0);
        lineItem.ExtendedAmount.Should().Be(0);
        lineItem.TaxAmount.Should().Be(0);
        lineItem.TotalAmount.Should().Be(0);
        lineItem.QuantityShipped.Should().Be(0);
        lineItem.QuantityInvoiced.Should().Be(0);
        lineItem.QuantityReturned.Should().Be(0);
        lineItem.FulfillmentStatus.Should().Be(OrderStatus.Draft);
        lineItem.AutoRenew.Should().BeFalse();
    }

    [Fact]
    public void OrderLineItem_CanSetProperties()
    {
        // Arrange
        var lineItem = new OrderLineItem();
        var serviceStart = DateTime.UtcNow;
        var serviceEnd = DateTime.UtcNow.AddYears(1);

        // Act
        lineItem.LineNumber = 1;
        lineItem.ExternalLineId = "EXT-LINE-001";
        lineItem.Name = "Enterprise License";
        lineItem.Description = "Annual enterprise software license";
        lineItem.SKU = "ENT-LIC-001";
        lineItem.ProductCode = "ENTERPRISE";
        lineItem.Quantity = 100;
        lineItem.UnitOfMeasure = "seats";
        lineItem.UnitPrice = 99.99m;
        lineItem.UnitCost = 25.00m;
        lineItem.DiscountAmount = 500.00m;
        lineItem.DiscountPercent = 5.0m;
        lineItem.ExtendedAmount = 9499.00m;
        lineItem.TaxAmount = 807.42m;
        lineItem.TotalAmount = 10306.42m;
        lineItem.BillingFrequency = BillingFrequency.Annually;
        lineItem.ServiceStartDate = serviceStart;
        lineItem.ServiceEndDate = serviceEnd;
        lineItem.TermLengthMonths = 12;
        lineItem.AutoRenew = true;
        lineItem.OrderId = 1;
        lineItem.ProductId = 5;

        // Assert
        lineItem.LineNumber.Should().Be(1);
        lineItem.ExternalLineId.Should().Be("EXT-LINE-001");
        lineItem.Name.Should().Be("Enterprise License");
        lineItem.Description.Should().Be("Annual enterprise software license");
        lineItem.SKU.Should().Be("ENT-LIC-001");
        lineItem.ProductCode.Should().Be("ENTERPRISE");
        lineItem.Quantity.Should().Be(100);
        lineItem.UnitOfMeasure.Should().Be("seats");
        lineItem.UnitPrice.Should().Be(99.99m);
        lineItem.UnitCost.Should().Be(25.00m);
        lineItem.DiscountAmount.Should().Be(500.00m);
        lineItem.DiscountPercent.Should().Be(5.0m);
        lineItem.ExtendedAmount.Should().Be(9499.00m);
        lineItem.TaxAmount.Should().Be(807.42m);
        lineItem.TotalAmount.Should().Be(10306.42m);
        lineItem.BillingFrequency.Should().Be(BillingFrequency.Annually);
        lineItem.ServiceStartDate.Should().Be(serviceStart);
        lineItem.ServiceEndDate.Should().Be(serviceEnd);
        lineItem.TermLengthMonths.Should().Be(12);
        lineItem.AutoRenew.Should().BeTrue();
        lineItem.OrderId.Should().Be(1);
        lineItem.ProductId.Should().Be(5);
    }

    [Fact]
    public void OrderLineItem_QuantityRemaining_ShouldCalculateCorrectly()
    {
        // Arrange
        var lineItem = new OrderLineItem
        {
            Quantity = 100,
            QuantityShipped = 60
        };

        // Act
        var remaining = lineItem.QuantityRemaining;

        // Assert
        remaining.Should().Be(40);
    }

    [Fact]
    public void OrderLineItem_QuantityRemaining_WhenFullyShipped_ShouldBeZero()
    {
        // Arrange
        var lineItem = new OrderLineItem
        {
            Quantity = 50,
            QuantityShipped = 50
        };

        // Act
        var remaining = lineItem.QuantityRemaining;

        // Assert
        remaining.Should().Be(0);
    }

    [Fact]
    public void OrderLineItem_FulfillmentTracking_ShouldWorkCorrectly()
    {
        // Arrange
        var lineItem = new OrderLineItem
        {
            Quantity = 100,
            QuantityShipped = 75,
            QuantityInvoiced = 50,
            QuantityReturned = 5,
            FulfillmentStatus = OrderStatus.Processing
        };

        // Assert
        lineItem.QuantityRemaining.Should().Be(25);
        lineItem.QuantityInvoiced.Should().Be(50);
        lineItem.QuantityReturned.Should().Be(5);
        lineItem.FulfillmentStatus.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void OrderLineItem_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var lineItem = new OrderLineItem();
        var order = new Order { Id = 1, OrderNumber = "ORD-001" };
        var product = new Product { Id = 2, Name = "Test Product" };
        var quoteLineItem = new QuoteLineItem { Id = 3 };
        var parentLineItem = new OrderLineItem { Id = 4 };

        // Act
        lineItem.Order = order;
        lineItem.Product = product;
        lineItem.QuoteLineItem = quoteLineItem;
        lineItem.ParentLineItem = parentLineItem;
        lineItem.BundleItems = new List<OrderLineItem> { new OrderLineItem { Id = 5 } };

        // Assert
        lineItem.Order.Should().BeSameAs(order);
        lineItem.Product.Should().BeSameAs(product);
        lineItem.QuoteLineItem.Should().BeSameAs(quoteLineItem);
        lineItem.ParentLineItem.Should().BeSameAs(parentLineItem);
        lineItem.BundleItems.Should().HaveCount(1);
    }

    #endregion

    #region InvoiceLineItem Entity Tests

    [Fact]
    public void InvoiceLineItem_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var lineItem = new InvoiceLineItem();

        // Assert
        lineItem.LineNumber.Should().Be(0);
        lineItem.Name.Should().BeEmpty();
        lineItem.Description.Should().BeNull();
        lineItem.SKU.Should().BeNull();
        lineItem.ProductCode.Should().BeNull();
        lineItem.Quantity.Should().Be(1);
        lineItem.UnitOfMeasure.Should().BeNull();
        lineItem.UnitPrice.Should().Be(0);
        lineItem.DiscountAmount.Should().Be(0);
        lineItem.DiscountPercent.Should().Be(0);
        lineItem.ExtendedAmount.Should().Be(0);
        lineItem.TaxAmount.Should().Be(0);
        lineItem.TaxRate.Should().BeNull();
        lineItem.TotalAmount.Should().Be(0);
        lineItem.DeferredRevenue.Should().BeNull();
        lineItem.RecognizedRevenue.Should().BeNull();
    }

    [Fact]
    public void InvoiceLineItem_CanSetProperties()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();
        var serviceStart = new DateTime(2024, 1, 1);
        var serviceEnd = new DateTime(2024, 12, 31);
        var revRecStart = new DateTime(2024, 1, 1);
        var revRecEnd = new DateTime(2024, 12, 31);

        // Act
        lineItem.LineNumber = 1;
        lineItem.ExternalLineId = "INV-LINE-001";
        lineItem.Name = "Professional Services";
        lineItem.Description = "Implementation consulting";
        lineItem.SKU = "PS-IMPL-100";
        lineItem.ProductCode = "PROFSVC";
        lineItem.Quantity = 40;
        lineItem.UnitOfMeasure = "hours";
        lineItem.UnitPrice = 250.00m;
        lineItem.DiscountAmount = 500.00m;
        lineItem.DiscountPercent = 5.0m;
        lineItem.ExtendedAmount = 9500.00m;
        lineItem.TaxAmount = 760.00m;
        lineItem.TaxRate = 8.0m;
        lineItem.TotalAmount = 10260.00m;
        lineItem.ServiceStartDate = serviceStart;
        lineItem.ServiceEndDate = serviceEnd;
        lineItem.RevenueRecognitionStartDate = revRecStart;
        lineItem.RevenueRecognitionEndDate = revRecEnd;
        lineItem.DeferredRevenue = 4750.00m;
        lineItem.RecognizedRevenue = 4750.00m;
        lineItem.InvoiceId = 10;
        lineItem.ProductId = 5;
        lineItem.OrderLineItemId = 3;
        lineItem.SubscriptionId = 2;
        lineItem.Notes = "Q1 delivery milestone";

        // Assert
        lineItem.LineNumber.Should().Be(1);
        lineItem.ExternalLineId.Should().Be("INV-LINE-001");
        lineItem.Name.Should().Be("Professional Services");
        lineItem.Description.Should().Be("Implementation consulting");
        lineItem.SKU.Should().Be("PS-IMPL-100");
        lineItem.ProductCode.Should().Be("PROFSVC");
        lineItem.Quantity.Should().Be(40);
        lineItem.UnitOfMeasure.Should().Be("hours");
        lineItem.UnitPrice.Should().Be(250.00m);
        lineItem.DiscountAmount.Should().Be(500.00m);
        lineItem.DiscountPercent.Should().Be(5.0m);
        lineItem.ExtendedAmount.Should().Be(9500.00m);
        lineItem.TaxAmount.Should().Be(760.00m);
        lineItem.TaxRate.Should().Be(8.0m);
        lineItem.TotalAmount.Should().Be(10260.00m);
        lineItem.ServiceStartDate.Should().Be(serviceStart);
        lineItem.ServiceEndDate.Should().Be(serviceEnd);
        lineItem.RevenueRecognitionStartDate.Should().Be(revRecStart);
        lineItem.RevenueRecognitionEndDate.Should().Be(revRecEnd);
        lineItem.DeferredRevenue.Should().Be(4750.00m);
        lineItem.RecognizedRevenue.Should().Be(4750.00m);
        lineItem.InvoiceId.Should().Be(10);
        lineItem.ProductId.Should().Be(5);
        lineItem.OrderLineItemId.Should().Be(3);
        lineItem.SubscriptionId.Should().Be(2);
        lineItem.Notes.Should().Be("Q1 delivery milestone");
    }

    [Fact]
    public void InvoiceLineItem_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var lineItem = new InvoiceLineItem();
        var invoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };
        var product = new Product { Id = 2, Name = "Test Product" };
        var orderLineItem = new OrderLineItem { Id = 3 };
        var subscription = new Subscription { Id = 4 };

        // Act
        lineItem.Invoice = invoice;
        lineItem.Product = product;
        lineItem.OrderLineItem = orderLineItem;
        lineItem.Subscription = subscription;

        // Assert
        lineItem.Invoice.Should().BeSameAs(invoice);
        lineItem.Product.Should().BeSameAs(product);
        lineItem.OrderLineItem.Should().BeSameAs(orderLineItem);
        lineItem.Subscription.Should().BeSameAs(subscription);
    }

    [Fact]
    public void InvoiceLineItem_RevenueRecognition_ShouldSupportDeferredRevenue()
    {
        // Arrange - 12 month subscription billed upfront
        var lineItem = new InvoiceLineItem
        {
            Name = "Annual Subscription",
            TotalAmount = 12000.00m,
            ServiceStartDate = new DateTime(2024, 1, 1),
            ServiceEndDate = new DateTime(2024, 12, 31),
            RevenueRecognitionStartDate = new DateTime(2024, 1, 1),
            RevenueRecognitionEndDate = new DateTime(2024, 12, 31)
        };

        // Act - After 6 months, recognize half
        lineItem.RecognizedRevenue = 6000.00m;
        lineItem.DeferredRevenue = 6000.00m;

        // Assert
        lineItem.RecognizedRevenue.Should().Be(6000.00m);
        lineItem.DeferredRevenue.Should().Be(6000.00m);
        (lineItem.RecognizedRevenue + lineItem.DeferredRevenue).Should().Be(lineItem.TotalAmount);
    }

    #endregion

    #region SalesForecast Entity Tests

    [Fact]
    public void SalesForecast_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var forecast = new SalesForecast();

        // Assert
        forecast.Name.Should().BeEmpty();
        forecast.Period.Should().BeEmpty();
        forecast.CurrencyCode.Should().Be("USD");
        forecast.QuotaAmount.Should().Be(0);
        forecast.ClosedWonAmount.Should().Be(0);
        forecast.CommitAmount.Should().Be(0);
        forecast.BestCaseAmount.Should().Be(0);
        forecast.PipelineAmount.Should().Be(0);
        forecast.OmittedAmount.Should().Be(0);
        forecast.ClosedWonCount.Should().Be(0);
        forecast.CommitCount.Should().Be(0);
        forecast.BestCaseCount.Should().Be(0);
        forecast.PipelineCount.Should().Be(0);
        forecast.IsSubmitted.Should().BeFalse();
        forecast.SnapshotDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SalesForecast_CanSetProperties()
    {
        // Arrange
        var forecast = new SalesForecast();
        var periodStart = new DateTime(2024, 1, 1);
        var periodEnd = new DateTime(2024, 3, 31);
        var snapshotDate = DateTime.UtcNow.AddDays(-1);
        var submittedAt = DateTime.UtcNow;

        // Act
        forecast.Name = "Q1 2024 Forecast";
        forecast.Period = "Q1 2024";
        forecast.PeriodStartDate = periodStart;
        forecast.PeriodEndDate = periodEnd;
        forecast.FiscalYear = 2024;
        forecast.FiscalQuarter = 1;
        forecast.FiscalMonth = 3;
        forecast.QuotaAmount = 500000.00m;
        forecast.CurrencyCode = "USD";
        forecast.ClosedWonAmount = 150000.00m;
        forecast.CommitAmount = 200000.00m;
        forecast.BestCaseAmount = 75000.00m;
        forecast.PipelineAmount = 300000.00m;
        forecast.OmittedAmount = 25000.00m;
        forecast.ClosedWonCount = 5;
        forecast.CommitCount = 8;
        forecast.BestCaseCount = 4;
        forecast.PipelineCount = 15;
        forecast.AdjustedCommitAmount = 180000.00m;
        forecast.AdjustedBestCaseAmount = 50000.00m;
        forecast.AdjustmentNotes = "Reduced commit based on pipeline review";
        forecast.AdjustedById = 3;
        forecast.AdjustedAt = snapshotDate;
        forecast.SnapshotDate = snapshotDate;
        forecast.IsSubmitted = true;
        forecast.SubmittedAt = submittedAt;
        forecast.UserId = 5;
        forecast.TeamId = 2;
        forecast.SalesQuotaId = 10;

        // Assert
        forecast.Name.Should().Be("Q1 2024 Forecast");
        forecast.Period.Should().Be("Q1 2024");
        forecast.PeriodStartDate.Should().Be(periodStart);
        forecast.PeriodEndDate.Should().Be(periodEnd);
        forecast.FiscalYear.Should().Be(2024);
        forecast.FiscalQuarter.Should().Be(1);
        forecast.FiscalMonth.Should().Be(3);
        forecast.QuotaAmount.Should().Be(500000.00m);
        forecast.CurrencyCode.Should().Be("USD");
        forecast.ClosedWonAmount.Should().Be(150000.00m);
        forecast.CommitAmount.Should().Be(200000.00m);
        forecast.BestCaseAmount.Should().Be(75000.00m);
        forecast.PipelineAmount.Should().Be(300000.00m);
        forecast.OmittedAmount.Should().Be(25000.00m);
        forecast.ClosedWonCount.Should().Be(5);
        forecast.CommitCount.Should().Be(8);
        forecast.BestCaseCount.Should().Be(4);
        forecast.PipelineCount.Should().Be(15);
        forecast.AdjustedCommitAmount.Should().Be(180000.00m);
        forecast.AdjustedBestCaseAmount.Should().Be(50000.00m);
        forecast.AdjustmentNotes.Should().Be("Reduced commit based on pipeline review");
        forecast.AdjustedById.Should().Be(3);
        forecast.IsSubmitted.Should().BeTrue();
        forecast.SubmittedAt.Should().Be(submittedAt);
        forecast.UserId.Should().Be(5);
        forecast.TeamId.Should().Be(2);
        forecast.SalesQuotaId.Should().Be(10);
    }

    [Fact]
    public void SalesForecast_ForecastAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            ClosedWonAmount = 100000.00m,
            CommitAmount = 150000.00m
        };

        // Act
        var forecastAmount = forecast.ForecastAmount;

        // Assert - Closed + Commit
        forecastAmount.Should().Be(250000.00m);
    }

    [Fact]
    public void SalesForecast_GapToQuota_ShouldCalculateCorrectly()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            QuotaAmount = 500000.00m,
            ClosedWonAmount = 150000.00m,
            CommitAmount = 200000.00m
        };

        // Act
        var gap = forecast.GapToQuota;

        // Assert - Gap = Quota - (Closed + Commit)
        gap.Should().Be(150000.00m);
    }

    [Fact]
    public void SalesForecast_GapToQuota_WhenOverQuota_ShouldBeZero()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            QuotaAmount = 400000.00m,
            ClosedWonAmount = 300000.00m,
            CommitAmount = 200000.00m
        };

        // Act
        var gap = forecast.GapToQuota;

        // Assert - Over quota, so gap is 0
        gap.Should().Be(0);
    }

    [Fact]
    public void SalesForecast_CoverageRatio_ShouldCalculateCorrectly()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            QuotaAmount = 500000.00m,
            ClosedWonAmount = 100000.00m,
            CommitAmount = 100000.00m,
            PipelineAmount = 600000.00m
        };

        // Act
        var coverage = forecast.CoverageRatio;

        // Assert - Gap = 300000, Pipeline = 600000, Coverage = 2.0
        coverage.Should().Be(2.0m);
    }

    [Fact]
    public void SalesForecast_CoverageRatio_WhenNoGap_ShouldBeZero()
    {
        // Arrange - Quota already achieved
        var forecast = new SalesForecast
        {
            QuotaAmount = 400000.00m,
            ClosedWonAmount = 300000.00m,
            CommitAmount = 200000.00m,
            PipelineAmount = 500000.00m
        };

        // Act
        var coverage = forecast.CoverageRatio;

        // Assert - No gap, so coverage is 0
        coverage.Should().Be(0);
    }

    [Fact]
    public void SalesForecast_ForecastAttainmentPercent_ShouldCalculateCorrectly()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            QuotaAmount = 500000.00m,
            ClosedWonAmount = 200000.00m,
            CommitAmount = 150000.00m
        };

        // Act
        var attainment = forecast.ForecastAttainmentPercent;

        // Assert - (350000 / 500000) * 100 = 70%
        attainment.Should().Be(70.0m);
    }

    [Fact]
    public void SalesForecast_ForecastAttainmentPercent_WhenNoQuota_ShouldBeZero()
    {
        // Arrange
        var forecast = new SalesForecast
        {
            QuotaAmount = 0,
            ClosedWonAmount = 200000.00m,
            CommitAmount = 150000.00m
        };

        // Act
        var attainment = forecast.ForecastAttainmentPercent;

        // Assert
        attainment.Should().Be(0);
    }

    [Fact]
    public void SalesForecast_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var forecast = new SalesForecast();
        var user = new User { Id = 1, FirstName = "John" };
        var team = new Team { Id = 2, Name = "West Region" };
        var quota = new SalesQuota { Id = 3, Name = "Q1 Quota" };
        var parent = new SalesForecast { Id = 4, Name = "Parent Forecast" };

        // Act
        forecast.User = user;
        forecast.Team = team;
        forecast.SalesQuota = quota;
        forecast.ParentForecast = parent;
        forecast.ChildForecasts = new List<SalesForecast> { new SalesForecast { Id = 5 } };
        forecast.LineItems = new List<ForecastLineItem> { new ForecastLineItem { Id = 1 } };

        // Assert
        forecast.User.Should().BeSameAs(user);
        forecast.Team.Should().BeSameAs(team);
        forecast.SalesQuota.Should().BeSameAs(quota);
        forecast.ParentForecast.Should().BeSameAs(parent);
        forecast.ChildForecasts.Should().HaveCount(1);
        forecast.LineItems.Should().HaveCount(1);
    }

    #endregion

    #region ForecastLineItem Entity Tests

    [Fact]
    public void ForecastLineItem_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var lineItem = new ForecastLineItem();

        // Assert
        lineItem.Category.Should().Be(ForecastCategory.Pipeline);
        lineItem.Amount.Should().Be(0);
        lineItem.Probability.Should().Be(0);
        lineItem.Stage.Should().BeNull();
        lineItem.OverrideCategory.Should().BeNull();
        lineItem.OverrideAmount.Should().BeNull();
        lineItem.OverrideNotes.Should().BeNull();
    }

    [Fact]
    public void ForecastLineItem_CanSetProperties()
    {
        // Arrange
        var lineItem = new ForecastLineItem();
        var closeDate = DateTime.UtcNow.AddDays(30);

        // Act
        lineItem.Category = ForecastCategory.Commit;
        lineItem.Amount = 75000.00m;
        lineItem.CloseDate = closeDate;
        lineItem.Stage = "Negotiation";
        lineItem.Probability = 80;
        lineItem.OverrideCategory = ForecastCategory.BestCase;
        lineItem.OverrideAmount = 60000.00m;
        lineItem.OverrideNotes = "Reduced based on customer feedback";
        lineItem.SalesForecastId = 10;
        lineItem.OpportunityId = 25;

        // Assert
        lineItem.Category.Should().Be(ForecastCategory.Commit);
        lineItem.Amount.Should().Be(75000.00m);
        lineItem.CloseDate.Should().Be(closeDate);
        lineItem.Stage.Should().Be("Negotiation");
        lineItem.Probability.Should().Be(80);
        lineItem.OverrideCategory.Should().Be(ForecastCategory.BestCase);
        lineItem.OverrideAmount.Should().Be(60000.00m);
        lineItem.OverrideNotes.Should().Be("Reduced based on customer feedback");
        lineItem.SalesForecastId.Should().Be(10);
        lineItem.OpportunityId.Should().Be(25);
    }

    [Theory]
    [InlineData(ForecastCategory.ClosedWon)]
    [InlineData(ForecastCategory.Commit)]
    [InlineData(ForecastCategory.BestCase)]
    [InlineData(ForecastCategory.Pipeline)]
    [InlineData(ForecastCategory.Omitted)]
    public void ForecastCategory_ShouldHaveAllValues(ForecastCategory category)
    {
        // Arrange & Act
        var lineItem = new ForecastLineItem { Category = category };

        // Assert
        lineItem.Category.Should().Be(category);
    }

    [Fact]
    public void ForecastLineItem_NavigationProperties_ShouldBeSettable()
    {
        // Arrange
        var lineItem = new ForecastLineItem();
        var forecast = new SalesForecast { Id = 1, Name = "Q1 Forecast" };
        var opportunity = new Opportunity { Id = 2, Name = "Big Deal" };

        // Act
        lineItem.SalesForecast = forecast;
        lineItem.Opportunity = opportunity;

        // Assert
        lineItem.SalesForecast.Should().BeSameAs(forecast);
        lineItem.Opportunity.Should().BeSameAs(opportunity);
    }

    [Fact]
    public void ForecastLineItem_ManagerOverride_ShouldWorkCorrectly()
    {
        // Arrange - Rep forecasts as Commit
        var lineItem = new ForecastLineItem
        {
            Category = ForecastCategory.Commit,
            Amount = 100000.00m,
            Stage = "Proposal",
            Probability = 75
        };

        // Act - Manager overrides to Best Case with lower amount
        lineItem.OverrideCategory = ForecastCategory.BestCase;
        lineItem.OverrideAmount = 80000.00m;
        lineItem.OverrideNotes = "Customer indicated budget constraints";

        // Assert
        lineItem.Category.Should().Be(ForecastCategory.Commit); // Original unchanged
        lineItem.OverrideCategory.Should().Be(ForecastCategory.BestCase);
        lineItem.OverrideAmount.Should().Be(80000.00m);
    }

    #endregion

    #region ForecastHistory Entity Tests

    [Fact]
    public void ForecastHistory_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var history = new ForecastHistory();

        // Assert
        history.SnapshotDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        history.Period.Should().BeEmpty();
        history.QuotaAmount.Should().Be(0);
        history.ClosedWonAmount.Should().Be(0);
        history.CommitAmount.Should().Be(0);
        history.BestCaseAmount.Should().Be(0);
        history.PipelineAmount.Should().Be(0);
        history.WeeksRemaining.Should().Be(0);
        history.UserId.Should().BeNull();
        history.TeamId.Should().BeNull();
    }

    [Fact]
    public void ForecastHistory_CanSetProperties()
    {
        // Arrange
        var history = new ForecastHistory();
        var snapshotDate = new DateTime(2024, 2, 15);

        // Act
        history.SnapshotDate = snapshotDate;
        history.Period = "Q1 2024";
        history.UserId = 5;
        history.TeamId = 2;
        history.QuotaAmount = 500000.00m;
        history.ClosedWonAmount = 100000.00m;
        history.CommitAmount = 175000.00m;
        history.BestCaseAmount = 75000.00m;
        history.PipelineAmount = 400000.00m;
        history.WeeksRemaining = 6;

        // Assert
        history.SnapshotDate.Should().Be(snapshotDate);
        history.Period.Should().Be("Q1 2024");
        history.UserId.Should().Be(5);
        history.TeamId.Should().Be(2);
        history.QuotaAmount.Should().Be(500000.00m);
        history.ClosedWonAmount.Should().Be(100000.00m);
        history.CommitAmount.Should().Be(175000.00m);
        history.BestCaseAmount.Should().Be(75000.00m);
        history.PipelineAmount.Should().Be(400000.00m);
        history.WeeksRemaining.Should().Be(6);
    }

    [Fact]
    public void ForecastHistory_TrendAnalysis_ShouldTrackMultipleSnapshots()
    {
        // Arrange - Simulate weekly snapshots
        var snapshots = new List<ForecastHistory>
        {
            new ForecastHistory
            {
                SnapshotDate = new DateTime(2024, 1, 5),
                Period = "Q1 2024",
                QuotaAmount = 500000.00m,
                ClosedWonAmount = 25000.00m,
                CommitAmount = 100000.00m,
                WeeksRemaining = 12
            },
            new ForecastHistory
            {
                SnapshotDate = new DateTime(2024, 1, 12),
                Period = "Q1 2024",
                QuotaAmount = 500000.00m,
                ClosedWonAmount = 50000.00m,
                CommitAmount = 125000.00m,
                WeeksRemaining = 11
            },
            new ForecastHistory
            {
                SnapshotDate = new DateTime(2024, 1, 19),
                Period = "Q1 2024",
                QuotaAmount = 500000.00m,
                ClosedWonAmount = 75000.00m,
                CommitAmount = 150000.00m,
                WeeksRemaining = 10
            }
        };

        // Assert - Trend should show progress
        snapshots.Should().HaveCount(3);
        snapshots[0].ClosedWonAmount.Should().BeLessThan(snapshots[2].ClosedWonAmount);
        snapshots[0].WeeksRemaining.Should().BeGreaterThan(snapshots[2].WeeksRemaining);
    }

    #endregion

    #region Invoice Computed Properties Tests

    [Fact]
    public void Invoice_BalanceDue_ShouldCalculateCorrectly()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 4000.00m,
            AmountCredited = 500.00m
        };

        // Act
        var balanceDue = invoice.BalanceDue;

        // Assert
        balanceDue.Should().Be(5500.00m);
    }

    [Fact]
    public void Invoice_IsPaid_WhenFullyPaid_ShouldBeTrue()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 10000.00m,
            AmountCredited = 0
        };

        // Act & Assert
        invoice.IsPaid.Should().BeTrue();
        invoice.BalanceDue.Should().Be(0);
    }

    [Fact]
    public void Invoice_IsPaid_WhenPartiallyPaid_ShouldBeFalse()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 5000.00m,
            AmountCredited = 0
        };

        // Act & Assert
        invoice.IsPaid.Should().BeFalse();
        invoice.BalanceDue.Should().Be(5000.00m);
    }

    [Fact]
    public void Invoice_DaysOverdue_WhenPastDue_ShouldBePositive()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 0,
            DueDate = DateTime.UtcNow.AddDays(-10)
        };

        // Act
        var daysOverdue = invoice.DaysOverdue;

        // Assert
        daysOverdue.Should().BeGreaterOrEqualTo(9); // Allow for day boundary
    }

    [Fact]
    public void Invoice_DaysOverdue_WhenNotDue_ShouldBeZero()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 0,
            DueDate = DateTime.UtcNow.AddDays(10)
        };

        // Act
        var daysOverdue = invoice.DaysOverdue;

        // Assert
        daysOverdue.Should().Be(0);
    }

    [Fact]
    public void Invoice_DaysOverdue_WhenPaid_ShouldBeZero()
    {
        // Arrange
        var invoice = new Invoice
        {
            TotalAmount = 10000.00m,
            AmountPaid = 10000.00m,
            DueDate = DateTime.UtcNow.AddDays(-30) // Past due but paid
        };

        // Act
        var daysOverdue = invoice.DaysOverdue;

        // Assert
        daysOverdue.Should().Be(0);
    }

    #endregion

    #region Order Computed Properties Tests

    [Fact]
    public void Order_BalanceDue_ShouldCalculateCorrectly()
    {
        // Arrange
        var order = new Order
        {
            TotalAmount = 50000.00m,
            AmountPaid = 20000.00m
        };

        // Act
        var balanceDue = order.BalanceDue;

        // Assert
        balanceDue.Should().Be(30000.00m);
    }

    [Fact]
    public void Order_IsPaid_WhenFullyPaid_ShouldBeTrue()
    {
        // Arrange
        var order = new Order
        {
            TotalAmount = 50000.00m,
            AmountPaid = 50000.00m
        };

        // Act & Assert
        order.IsPaid.Should().BeTrue();
    }

    [Fact]
    public void Order_IsPaid_WhenPartiallyPaid_ShouldBeFalse()
    {
        // Arrange
        var order = new Order
        {
            TotalAmount = 50000.00m,
            AmountPaid = 25000.00m
        };

        // Act & Assert
        order.IsPaid.Should().BeFalse();
    }

    #endregion
}
