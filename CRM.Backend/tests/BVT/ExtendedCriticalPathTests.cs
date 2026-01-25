// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.BVT;

/// <summary>
/// Extended Build Verification Tests (BVT) for CRM Solution
/// Tests additional entity creation and validation beyond core tests.
/// </summary>
public class ExtendedCriticalPathTests
{
    #region BVT-028: Opportunity Entity

    [Fact]
    public void BVT028_OpportunityEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise License - Acme Corp",
            Description = "Annual enterprise software license",
            Amount = 50000m,
            ExpectedRevenue = 35000m,
            Stage = OpportunityStage.Prospecting,
            OpportunityType = OpportunityType.NewBusiness,
            Priority = OpportunityPriority.High,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        opportunity.Id.Should().Be(1);
        opportunity.Name.Should().Be("Enterprise License - Acme Corp");
        opportunity.Amount.Should().Be(50000m);
        opportunity.Stage.Should().Be(OpportunityStage.Prospecting);
        opportunity.OpportunityType.Should().Be(OpportunityType.NewBusiness);
    }

    [Fact]
    public void BVT029_OpportunityStage_HasAllExpectedValues()
    {
        // Assert - verify key stages exist
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Prospecting).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.Qualification).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.NeedsAnalysis).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.ProposalQuote).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.NegotiationReview).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.ClosedWon).Should().BeTrue();
        Enum.IsDefined(typeof(OpportunityStage), OpportunityStage.ClosedLost).Should().BeTrue();
    }

    [Fact]
    public void BVT030_Opportunity_ExpectedRevenueCalculation()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 0.7 // 70% probability (double type)
        };

        // Act - simulate weighted value calculation
        var weightedValue = opportunity.Amount * (decimal)opportunity.Probability;

        // Assert
        weightedValue.Should().Be(70000m);
    }

    #endregion

    #region BVT-031: Product Entity

    [Fact]
    public void BVT031_ProductEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "Enterprise Software Suite",
            Description = "Complete CRM software package",
            SKU = "ENT-001",
            Price = 999.99m,
            Cost = 200m,
            ProductType = ProductType.Subscription,
            Status = ProductStatus.Active,
            Category = "Software",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Enterprise Software Suite");
        product.SKU.Should().Be("ENT-001");
        product.Price.Should().Be(999.99m);
        product.ProductType.Should().Be(ProductType.Subscription);
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void BVT032_Product_MarginCalculation()
    {
        // Arrange
        var product = new Product
        {
            Price = 100m,
            Cost = 40m
        };

        // Act - simulate margin calculation
        var margin = (product.Price - product.Cost) / product.Price * 100;

        // Assert
        margin.Should().Be(60m); // 60% margin
    }

    [Fact]
    public void BVT033_ProductStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Draft).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Active).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Discontinued).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.OutOfStock).Should().BeTrue();
        Enum.IsDefined(typeof(ProductStatus), ProductStatus.Archived).Should().BeTrue();
    }

    [Fact]
    public void BVT034_ProductType_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(ProductType), ProductType.Physical).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Digital).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Service).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Subscription).Should().BeTrue();
        Enum.IsDefined(typeof(ProductType), ProductType.Bundle).Should().BeTrue();
    }

    #endregion

    #region BVT-035: Marketing Campaign Entity

    [Fact]
    public void BVT035_MarketingCampaignEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var campaign = new MarketingCampaign
        {
            Id = 1,
            Name = "Q4 Product Launch",
            Description = "New product launch campaign",
            Status = CampaignStatus.Draft,
            CampaignType = CampaignType.Email,
            Budget = 10000m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        campaign.Id.Should().Be(1);
        campaign.Name.Should().Be("Q4 Product Launch");
        campaign.Status.Should().Be(CampaignStatus.Draft);
        campaign.CampaignType.Should().Be(CampaignType.Email);
        campaign.Budget.Should().Be(10000m);
    }

    [Fact]
    public void BVT036_CampaignStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Draft).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Scheduled).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Active).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Paused).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Completed).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignStatus), CampaignStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void BVT037_CampaignType_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(CampaignType), CampaignType.Email).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignType), CampaignType.SocialMedia).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignType), CampaignType.PaidSearch).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignType), CampaignType.Event).Should().BeTrue();
        Enum.IsDefined(typeof(CampaignType), CampaignType.Webinar).Should().BeTrue();
    }

    [Fact]
    public void BVT038_Campaign_BudgetUtilizationCalculation()
    {
        // Arrange
        var campaign = new MarketingCampaign
        {
            Budget = 10000m,
            ActualCost = 7500m
        };

        // Act - simulate utilization calculation
        var utilization = (campaign.ActualCost / campaign.Budget) * 100;

        // Assert
        utilization.Should().Be(75m); // 75% utilized
    }

    #endregion

    #region BVT-039: Service Request Entity

    [Fact]
    public void BVT039_ServiceRequestEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-2024-0001",
            Subject = "System access issue",
            Description = "Cannot log in to the application",
            Status = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.High,
            Channel = ServiceRequestChannel.Email,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        request.Id.Should().Be(1);
        request.TicketNumber.Should().Be("SR-2024-0001");
        request.Subject.Should().Be("System access issue");
        request.Status.Should().Be(ServiceRequestStatus.New);
        request.Priority.Should().Be(ServiceRequestPriority.High);
        request.Channel.Should().Be(ServiceRequestChannel.Email);
    }

    [Fact]
    public void BVT040_ServiceRequestStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.New).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.Open).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.InProgress).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.Resolved).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.Closed).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestStatus), ServiceRequestStatus.Escalated).Should().BeTrue();
    }

    [Fact]
    public void BVT041_ServiceRequestPriority_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(ServiceRequestPriority), ServiceRequestPriority.Low).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestPriority), ServiceRequestPriority.Medium).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestPriority), ServiceRequestPriority.High).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestPriority), ServiceRequestPriority.Critical).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestPriority), ServiceRequestPriority.Urgent).Should().BeTrue();
    }

    [Fact]
    public void BVT042_ServiceRequestChannel_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(ServiceRequestChannel), ServiceRequestChannel.Email).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestChannel), ServiceRequestChannel.Phone).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestChannel), ServiceRequestChannel.WhatsApp).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestChannel), ServiceRequestChannel.SelfServicePortal).Should().BeTrue();
        Enum.IsDefined(typeof(ServiceRequestChannel), ServiceRequestChannel.LiveChat).Should().BeTrue();
    }

    #endregion

    #region BVT-043: Lead Entity

    [Fact]
    public void BVT043_LeadEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Company = "Tech Corp",
            Source = LeadSource.WebsiteForm,
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        lead.Id.Should().Be(1);
        lead.FirstName.Should().Be("Jane");
        lead.LastName.Should().Be("Smith");
        lead.Email.Should().Be("jane.smith@example.com");
        lead.Source.Should().Be(LeadSource.WebsiteForm);
        lead.Status.Should().Be(LeadStatus.New);
    }

    [Fact]
    public void BVT044_LeadSource_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(LeadSource), LeadSource.WebsiteForm).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.Referral).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.TradeShow).Should().BeTrue();
        Enum.IsDefined(typeof(LeadSource), LeadSource.SocialMediaOrganic).Should().BeTrue();
    }

    [Fact]
    public void BVT045_LeadStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(LeadStatus), LeadStatus.New).Should().BeTrue();
        Enum.IsDefined(typeof(LeadStatus), LeadStatus.Contacted).Should().BeTrue();
        Enum.IsDefined(typeof(LeadStatus), LeadStatus.Qualified).Should().BeTrue();
        Enum.IsDefined(typeof(LeadStatus), LeadStatus.Converted).Should().BeTrue();
        Enum.IsDefined(typeof(LeadStatus), LeadStatus.Disqualified).Should().BeTrue();
    }

    #endregion

    #region BVT-046: Note Entity

    [Fact]
    public void BVT046_NoteEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var note = new Note
        {
            Id = 1,
            Content = "Customer requested follow-up next week",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        note.Id.Should().Be(1);
        note.Content.Should().Be("Customer requested follow-up next week");
    }

    #endregion

    #region BVT-047: CrmTask Entity

    [Fact]
    public void BVT047_CrmTaskEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var task = new CrmTask
        {
            Id = 1,
            Subject = "Follow up with customer",
            Description = "Discuss contract renewal",
            Status = CrmTaskStatus.NotStarted,
            Priority = CrmTaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        task.Id.Should().Be(1);
        task.Subject.Should().Be("Follow up with customer");
        task.Status.Should().Be(CrmTaskStatus.NotStarted);
        task.Priority.Should().Be(CrmTaskPriority.High);
    }

    [Fact]
    public void BVT048_CrmTaskStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(CrmTaskStatus), CrmTaskStatus.NotStarted).Should().BeTrue();
        Enum.IsDefined(typeof(CrmTaskStatus), CrmTaskStatus.InProgress).Should().BeTrue();
        Enum.IsDefined(typeof(CrmTaskStatus), CrmTaskStatus.Completed).Should().BeTrue();
        Enum.IsDefined(typeof(CrmTaskStatus), CrmTaskStatus.Cancelled).Should().BeTrue();
    }

    [Fact]
    public void BVT049_CrmTaskPriority_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(CrmTaskPriority), CrmTaskPriority.Low).Should().BeTrue();
        Enum.IsDefined(typeof(CrmTaskPriority), CrmTaskPriority.Normal).Should().BeTrue();
        Enum.IsDefined(typeof(CrmTaskPriority), CrmTaskPriority.High).Should().BeTrue();
    }

    #endregion

    #region BVT-050: Quote Entity

    [Fact]
    public void BVT050_QuoteEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var quote = new Quote
        {
            Id = 1,
            QuoteNumber = "QT-2024-0001",
            Name = "Enterprise Package Quote",
            Subtotal = 10000m,
            Total = 10750m,
            Tax = 750m,
            Status = QuoteStatus.Draft,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        quote.Id.Should().Be(1);
        quote.QuoteNumber.Should().Be("QT-2024-0001");
        quote.Subtotal.Should().Be(10000m);
        quote.Total.Should().Be(10750m);
        quote.Tax.Should().Be(750m);
        quote.Status.Should().Be(QuoteStatus.Draft);
    }

    [Fact]
    public void BVT051_QuoteStatus_HasAllExpectedValues()
    {
        // Assert
        Enum.IsDefined(typeof(QuoteStatus), QuoteStatus.Draft).Should().BeTrue();
        Enum.IsDefined(typeof(QuoteStatus), QuoteStatus.Sent).Should().BeTrue();
        Enum.IsDefined(typeof(QuoteStatus), QuoteStatus.Accepted).Should().BeTrue();
        Enum.IsDefined(typeof(QuoteStatus), QuoteStatus.Rejected).Should().BeTrue();
        Enum.IsDefined(typeof(QuoteStatus), QuoteStatus.Expired).Should().BeTrue();
    }

    #endregion

    #region BVT-052: Integration Tests

    [Fact]
    public void BVT052_Opportunity_CanLinkToCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Category = CustomerCategory.Organization,
            Company = "Acme Corp"
        };

        // Act
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Acme Enterprise Deal",
            CustomerId = customer.Id,
            Amount = 75000m
        };

        // Assert
        opportunity.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public void BVT053_ServiceRequest_CanLinkToCustomer()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var request = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-001",
            Subject = "Support Request",
            CustomerId = customer.Id
        };

        // Assert
        request.CustomerId.Should().Be(customer.Id);
    }

    [Fact]
    public void BVT054_Campaign_CanCalculateDuration()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddDays(30);

        var campaign = new MarketingCampaign
        {
            Id = 1,
            Name = "Monthly Campaign",
            StartDate = startDate,
            EndDate = endDate
        };

        // Act
        var duration = campaign.EndDate - campaign.StartDate;

        // Assert
        duration.Should().NotBeNull();
        duration!.Value.Days.Should().Be(30);
    }

    [Fact]
    public void BVT055_Opportunity_ForecastCategoryHasCorrectValues()
    {
        // Assert
        Enum.IsDefined(typeof(ForecastCategory), ForecastCategory.Pipeline).Should().BeTrue();
        Enum.IsDefined(typeof(ForecastCategory), ForecastCategory.BestCase).Should().BeTrue();
        Enum.IsDefined(typeof(ForecastCategory), ForecastCategory.Commit).Should().BeTrue();
        Enum.IsDefined(typeof(ForecastCategory), ForecastCategory.Closed).Should().BeTrue();
    }

    #endregion
}
