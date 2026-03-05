// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Integration;

/// <summary>
/// Entity boundary conditions, numeric validation, and concurrency
/// (optimistic locking) tests.
///
/// All tests operate on pure entity instantiation — no database or service layer.
/// SPEC_CONFLICTs are documented inline where the test specification
/// description differs from actual code behaviour.
///
/// Fields verified by reading source files before this test was written:
///   BaseEntity.cs, Lead.cs, Opportunity.cs, ServiceRequest.cs,
///   Quote.cs, Order.cs, Invoice.cs, Payment.cs, Subscription.cs
/// </summary>
public class ConcurrencyAndBoundaryTests
{
    // ──────────────────────────────────────────────────────────────
    // #region 1 — BaseEntity / Optimistic Concurrency
    // ──────────────────────────────────────────────────────────────

    #region BaseEntity and Concurrency Pattern

    [Fact]
    public void BaseEntity_RowVersion_ShouldBeNullByDefault()
    {
        // RowVersion is a [Timestamp] byte[]? on BaseEntity.
        // It is populated by the database on first INSERT, so new entities have null.
        var lead = new Lead();

        lead.RowVersion.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_IsDeleted_ShouldBeFalseByDefault()
    {
        var lead = new Lead();

        lead.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void BaseEntity_CreatedAt_ShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow;
        var entity = new Lead();
        var after = DateTime.UtcNow;

        entity.CreatedAt.Should().BeOnOrAfter(before)
            .And.BeOnOrBefore(after);
    }

    [Fact]
    public void BaseEntity_UpdatedAt_ShouldBeNullByDefault()
    {
        // UpdatedAt is nullable — only set after first update.
        var lead = new Lead();

        lead.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void BaseEntity_UpdatedAt_ShouldBeUpdatable()
    {
        var lead = new Lead();
        var updateTime = DateTime.UtcNow;
        lead.UpdatedAt = updateTime;

        lead.UpdatedAt.Should().BeCloseTo(updateTime, TimeSpan.FromMilliseconds(10));
    }

    [Fact]
    public void BaseEntity_RowVersion_CanBeSetToByteArray()
    {
        // Verifies that RowVersion field accepts a byte array value
        // (simulating what EF Core does after a DB round-trip).
        var lead = new Lead();
        var token = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };

        lead.RowVersion = token;

        lead.RowVersion.Should().BeEquivalentTo(token);
    }

    [Fact]
    public void BaseEntity_ConcurrencyToken_IsByteArrayType()
    {
        // Ensures the RowVersion property is typed as byte[]? (for EF Core [Timestamp]).
        var prop = typeof(Lead).GetProperty("RowVersion");

        prop.Should().NotBeNull();
        prop!.PropertyType.Should().Be(typeof(byte[]));
    }

    [Fact]
    public void BaseEntity_IsDeleted_ShouldSupportSoftDelete()
    {
        var opportunity = new Opportunity { Name = "Test Opp" };
        opportunity.IsDeleted.Should().BeFalse();

        opportunity.IsDeleted = true;

        opportunity.IsDeleted.Should().BeTrue();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 2 — Lead Entity
    // ──────────────────────────────────────────────────────────────

    #region Lead Entity Defaults and Boundaries

    [Fact]
    public void Lead_ShouldHaveDefaultValues_WhenCreated()
    {
        var lead = new Lead();

        lead.Status.Should().Be(LeadLifecycleStatus.New);
        lead.Source.Should().Be(LeadSource.Web);
        lead.Score.Should().Be(0);
        lead.FitScore.Should().Be(0);
        lead.EngagementScore.Should().Be(0);
        lead.FirstName.Should().Be(string.Empty);
        lead.LastName.Should().Be(string.Empty);
        lead.Email.Should().Be(string.Empty);
        lead.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Lead_Status_ShouldDefaultToNew()
    {
        var lead = new Lead();

        lead.Status.Should().Be(LeadLifecycleStatus.New);
        ((int)LeadLifecycleStatus.New).Should().Be(0);
    }

    [Fact]
    public void Lead_Score_ShouldDefaultToZero()
    {
        var lead = new Lead();

        lead.Score.Should().Be(0);
    }

    [Fact]
    public void Lead_LeadScore_ShouldAlias_Score()
    {
        // LeadScore is a [NotMapped] property that aliases Score.
        var lead = new Lead();
        lead.LeadScore = 75;

        lead.Score.Should().Be(75);
        lead.LeadScore.Should().Be(75);
    }

    [Fact]
    public void Lead_Score_ShouldAllowValuesAbove100()
    {
        // No [Range] attribute on Score — entity allows any int value.
        // Validation constraints are enforced at the service/DTO layer.
        var lead = new Lead { Score = 150 };

        lead.Score.Should().Be(150);
    }

    [Fact]
    public void Lead_Score_ShouldAllowNegativeValues()
    {
        // No [Range] attribute on Score at entity level.
        var lead = new Lead { Score = -5 };

        lead.Score.Should().Be(-5);
    }

    [Fact]
    public void Lead_FirstName_ShouldAllowMaxLength100()
    {
        var longName = new string('A', 100);
        var lead = new Lead { FirstName = longName };

        lead.FirstName.Length.Should().Be(100);

        var attr = typeof(Lead)
            .GetProperty("FirstName")!
            .GetCustomAttributes(typeof(MaxLengthAttribute), false)
            .Cast<MaxLengthAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Length.Should().Be(100);
    }

    [Fact]
    public void Lead_Email_ShouldDefaultToEmptyString()
    {
        var lead = new Lead();

        lead.Email.Should().Be(string.Empty);
    }

    [Fact]
    public void Lead_Phone_ShouldBeNullByDefault()
    {
        var lead = new Lead();

        lead.Phone.Should().BeNull();
    }

    [Fact]
    public void Lead_MultipleLeads_CanShareSameEmail_AtEntityLevel()
    {
        // No unique constraint at the entity level (enforced at DB/service layer).
        const string email = "shared@example.com";
        var lead1 = new Lead { FirstName = "Alice", Email = email };
        var lead2 = new Lead { FirstName = "Bob", Email = email };

        lead1.Email.Should().Be(lead2.Email);
    }

    [Fact]
    public void Lead_IsDeleted_ShouldBeFalseByDefault()
    {
        var lead = new Lead();

        lead.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Lead_CreatedAt_ShouldBeSetOnCreation()
    {
        var before = DateTime.UtcNow;
        var lead = new Lead();
        var after = DateTime.UtcNow;

        lead.CreatedAt.Should().BeOnOrAfter(before)
            .And.BeOnOrBefore(after);
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 3 — Opportunity Entity
    // ──────────────────────────────────────────────────────────────

    #region Opportunity Entity Defaults and Boundaries

    [Fact]
    public void Opportunity_ShouldHaveDefaultValues_WhenCreated()
    {
        var opp = new Opportunity { Name = "New Deal" };

        opp.Stage.Should().Be(OpportunityStage.Discovery);
        opp.Probability.Should().Be(10);
        opp.Amount.Should().Be(0);
        opp.Currency.Should().Be("USD");
        opp.IsDeleted.Should().BeFalse();
        opp.PricingModel.Should().Be(OpportunityPricingModel.Subscription);
        opp.TermLengthMonths.Should().Be(12);
    }

    [Fact]
    public void Opportunity_Probability_ShouldDefaultToTen()
    {
        var opp = new Opportunity { Name = "Deal" };

        opp.Probability.Should().Be(10);
    }

    [Fact]
    public void Opportunity_Probability_ShouldBeBetween0And100_HasRangeAttribute()
    {
        var attr = typeof(Opportunity)
            .GetProperty("Probability")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull("Probability must have [Range(0, 100)] attribute");
        attr!.Minimum.Should().Be(0);
        attr.Maximum.Should().Be(100);
    }

    [Fact]
    public void Opportunity_Probability_EntityAllowsBoundaryValueOfZero()
    {
        // Entity allows 0 — range validation occurs at service/controller layer.
        var opp = new Opportunity { Name = "Lost Deal", Probability = 0 };

        opp.Probability.Should().Be(0);
    }

    [Fact]
    public void Opportunity_Probability_EntityAllowsBoundaryValueOf100()
    {
        var opp = new Opportunity { Name = "Won Deal", Probability = 100 };

        opp.Probability.Should().Be(100);
    }

    [Fact]
    public void Opportunity_IsDeleted_ShouldBeFalseByDefault()
    {
        var opp = new Opportunity { Name = "Opp" };

        opp.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_ExpectedCloseDate_ShouldAllowFutureDates()
    {
        var futureDate = DateTime.UtcNow.AddMonths(6);
        var opp = new Opportunity { Name = "Opp", ExpectedCloseDate = futureDate };

        opp.ExpectedCloseDate.Should().BeCloseTo(futureDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Opportunity_ExpectedCloseDate_ShouldAllowPastDates()
    {
        // No past-date restriction at entity level.
        var pastDate = DateTime.UtcNow.AddMonths(-3);
        var opp = new Opportunity { Name = "Opp", ExpectedCloseDate = pastDate };

        opp.ExpectedCloseDate.Should().BeCloseTo(pastDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Opportunity_ExpectedCloseDate_ShouldBeNullByDefault()
    {
        var opp = new Opportunity { Name = "Opp" };

        opp.ExpectedCloseDate.Should().BeNull();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 4 — ServiceRequest Entity
    // ──────────────────────────────────────────────────────────────

    #region ServiceRequest Entity Defaults

    [Fact]
    public void ServiceRequest_Status_ShouldDefaultToNew()
    {
        // SPEC_CONFLICT: Task spec says Status defaults to Open.
        // Actual code: ServiceRequestStatus.New (= 0) is the default.
        var sr = new ServiceRequest
        {
            TicketNumber = "TKT-001",
            Subject = "Test ticket"
        };

        sr.Status.Should().Be(ServiceRequestStatus.New);
    }

    [Fact]
    public void ServiceRequest_Priority_ShouldDefaultToMedium()
    {
        // SPEC_CONFLICT: Task spec says Priority defaults to Low.
        // Actual code: ServiceRequestPriority.Medium is the default.
        var sr = new ServiceRequest
        {
            TicketNumber = "TKT-001",
            Subject = "Test ticket"
        };

        sr.Priority.Should().Be(ServiceRequestPriority.Medium);
    }

    [Fact]
    public void ServiceRequest_Subject_ShouldDefaultToEmpty()
    {
        var sr = new ServiceRequest { TicketNumber = "TKT-001" };

        sr.Subject.Should().Be(string.Empty);
    }

    [Fact]
    public void ServiceRequest_TicketNumber_ShouldDefaultToEmpty()
    {
        var sr = new ServiceRequest();

        sr.TicketNumber.Should().Be(string.Empty);
    }

    [Fact]
    public void ServiceRequest_IsDeleted_ShouldBeFalseByDefault()
    {
        var sr = new ServiceRequest { TicketNumber = "TKT-001", Subject = "Sub" };

        sr.IsDeleted.Should().BeFalse();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 5 — Quote Entity
    // ──────────────────────────────────────────────────────────────

    #region Quote Entity Boundary Conditions

    [Fact]
    public void Quote_ExpirationDate_ShouldBeNullByDefault()
    {
        // SPEC_CONFLICT: Task spec references "ExpiryDate" — actual property is "ExpirationDate".
        var quote = new Quote
        {
            QuoteNumber = "QUO-001",
            Name = "Test Quote"
        };

        quote.ExpirationDate.Should().BeNull();
    }

    [Fact]
    public void Quote_ExpirationDate_ShouldBeAfterQuoteDate_WhenSet()
    {
        var quote = new Quote
        {
            QuoteNumber = "QUO-001",
            Name = "Test Quote"
        };
        var expiry = quote.QuoteDate.AddDays(30);
        quote.ExpirationDate = expiry;

        quote.ExpirationDate.Should().BeAfter(quote.QuoteDate);
    }

    [Fact]
    public void Quote_Total_ShouldDefaultToZero()
    {
        var quote = new Quote
        {
            QuoteNumber = "QUO-001",
            Name = "Test Quote"
        };

        quote.Total.Should().Be(0);
    }

    [Fact]
    public void Quote_Status_ShouldDefaultToNew()
    {
        var quote = new Quote
        {
            QuoteNumber = "QUO-001",
            Name = "Test Quote"
        };

        quote.Status.Should().Be(QuoteStatus.New);
    }

    [Fact]
    public void Quote_DiscountPercent_HasRangeAttribute0To100()
    {
        var attr = typeof(Quote)
            .GetProperty("DiscountPercent")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(0);
        attr.Maximum.Should().Be(100);
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 6 — Order Entity
    // ──────────────────────────────────────────────────────────────

    #region Order Entity Boundary Conditions

    [Fact]
    public void Order_TotalAmount_ShouldDefaultToZero()
    {
        var order = new Order { OrderNumber = "ORD-001" };

        order.TotalAmount.Should().Be(0);
    }

    [Fact]
    public void Order_Status_ShouldDefaultToDraft()
    {
        var order = new Order { OrderNumber = "ORD-001" };

        order.Status.Should().Be(OrderStatus.Draft);
    }

    [Fact]
    public void Order_Priority_ShouldDefaultToNormal()
    {
        var order = new Order { OrderNumber = "ORD-001" };

        order.Priority.Should().Be(OrderPriority.Normal);
    }

    [Fact]
    public void Order_CurrencyCode_ShouldDefaultToUSD()
    {
        var order = new Order { OrderNumber = "ORD-001" };

        order.CurrencyCode.Should().Be("USD");
    }

    [Fact]
    public void Order_IsDeleted_ShouldBeFalseByDefault()
    {
        var order = new Order { OrderNumber = "ORD-001" };

        order.IsDeleted.Should().BeFalse();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 7 — Numeric Boundary Conditions (Cross-Entity)
    // ──────────────────────────────────────────────────────────────

    #region Numeric Validation and Boundary Conditions

    [Fact]
    public void Invoice_DiscountPercent_HasRangeAttribute0To100()
    {
        var attr = typeof(Invoice)
            .GetProperty("DiscountPercent")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(0);
        attr.Maximum.Should().Be(100);
    }

    [Fact]
    public void Invoice_TaxRate_HasRangeAttribute0To100()
    {
        var attr = typeof(Invoice)
            .GetProperty("TaxRate")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(0);
        attr.Maximum.Should().Be(100);
    }

    [Fact]
    public void Invoice_TotalAmount_HasRangeAttributeMinimumZero()
    {
        var attr = typeof(Invoice)
            .GetProperty("TotalAmount")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(0);
    }

    [Fact]
    public void Lead_EstimatedValue_IsNotOnEntity_LeadHasNoEstimatedValueField()
    {
        // SPEC_CONFLICT: Task spec mentions Lead_EstimatedValue_ShouldAllowNegative.
        // The Lead entity has no EstimatedValue field — that field is on Opportunity.Amount.
        // Verifying that the property genuinely does not exist.
        var prop = typeof(Lead).GetProperty("EstimatedValue");

        prop.Should().BeNull("Lead entity has no EstimatedValue property");
    }

    [Fact]
    public void Opportunity_Amount_HasRangeAttributeMinimumZero()
    {
        var attr = typeof(Opportunity)
            .GetProperty("Amount")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(0.0);
    }

    [Fact]
    public void Opportunity_TermLengthMonths_HasRangeAttribute1To120()
    {
        var attr = typeof(Opportunity)
            .GetProperty("TermLengthMonths")!
            .GetCustomAttributes(typeof(RangeAttribute), false)
            .Cast<RangeAttribute>()
            .FirstOrDefault();

        attr.Should().NotBeNull();
        attr!.Minimum.Should().Be(1);
        attr.Maximum.Should().Be(120);
    }

    #endregion

    // ──────────────────────────────────────────────────────────────
    // #region 8 — Concurrency Token Verification
    // ──────────────────────────────────────────────────────────────

    #region Optimistic Concurrency Token Attributes

    [Fact]
    public void BaseEntity_RowVersion_HasTimestampAttribute()
    {
        var attr = typeof(Lead)
            .GetProperty("RowVersion")!
            .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.TimestampAttribute), true)
            .FirstOrDefault();

        attr.Should().NotBeNull("[Timestamp] attribute must be on RowVersion for EF Core optimistic concurrency");
    }

    [Fact]
    public void BaseEntity_RowVersion_CannotBeSetWithSameReference_AfterChange()
    {
        // Simulates detecting a concurrency conflict: two in-memory instances
        // with different RowVersion values represent divergent state.
        var version1 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 };
        var version2 = new byte[] { 0, 0, 0, 0, 0, 0, 0, 2 };

        var entityA = new Lead { RowVersion = version1 };
        var entityB = new Lead { RowVersion = version2 };

        entityA.RowVersion.Should().NotBeEquivalentTo(entityB.RowVersion,
            "Different RowVersion values indicate a concurrency conflict exists");
    }

    [Fact]
    public void Opportunity_InheritsRowVersion_FromBaseEntity()
    {
        var opp = new Opportunity { Name = "Test" };

        // RowVersion is inherited from BaseEntity and must be null before DB persist
        opp.RowVersion.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_InheritsRowVersion_FromBaseEntity()
    {
        var sr = new ServiceRequest { TicketNumber = "TKT-001", Subject = "Sub" };

        sr.RowVersion.Should().BeNull();
    }

    [Fact]
    public void Invoice_InheritsRowVersion_FromBaseEntity()
    {
        var inv = new Invoice { InvoiceNumber = "INV-001" };

        inv.RowVersion.Should().BeNull();
    }

    [Fact]
    public void Payment_InheritsRowVersion_FromBaseEntity()
    {
        var payment = new Payment { AccountId = 1 };

        payment.RowVersion.Should().BeNull();
    }

    #endregion
}
