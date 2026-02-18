// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Lead and Opportunity entities and their enums.
/// These are core CRM entities representing the sales pipeline.
/// </summary>
public class LeadOpportunityEntityTests
{
    #region OpportunityStage Enum Tests

    public class OpportunityStageTests
    {
        [Fact]
        public void OpportunityStage_Discovery_ShouldBeZero()
        {
            ((int)OpportunityStage.Discovery).Should().Be(0);
        }

        [Fact]
        public void OpportunityStage_Qualification_ShouldBeOne()
        {
            ((int)OpportunityStage.Qualification).Should().Be(1);
        }

        [Fact]
        public void OpportunityStage_Proposal_ShouldBeTwo()
        {
            ((int)OpportunityStage.Proposal).Should().Be(2);
        }

        [Fact]
        public void OpportunityStage_Negotiation_ShouldBeThree()
        {
            ((int)OpportunityStage.Negotiation).Should().Be(3);
        }

        [Fact]
        public void OpportunityStage_ClosedWon_ShouldBeFour()
        {
            ((int)OpportunityStage.ClosedWon).Should().Be(4);
        }

        [Fact]
        public void OpportunityStage_ClosedLost_ShouldBeFive()
        {
            ((int)OpportunityStage.ClosedLost).Should().Be(5);
        }

        [Fact]
        public void OpportunityStage_AllValues_ShouldHaveSixStages()
        {
            var values = Enum.GetValues<OpportunityStage>();
            values.Should().HaveCount(6);
            values.Distinct().Should().HaveCount(6);
        }
    }

    #endregion

    #region QualificationReason Enum Tests

    public class QualificationReasonTests
    {
        [Theory]
        [InlineData(QualificationReason.Budget, 0)]
        [InlineData(QualificationReason.Need, 1)]
        [InlineData(QualificationReason.Timing, 2)]
        [InlineData(QualificationReason.Authority, 3)]
        [InlineData(QualificationReason.Fit, 4)]
        public void QualificationReason_ShouldHaveCorrectValue(QualificationReason reason, int expected)
        {
            ((int)reason).Should().Be(expected);
        }

        [Fact]
        public void QualificationReason_AllValues_ShouldHaveFiveReasons()
        {
            // BANT: Budget, Authority, Need, Timing + Fit
            var values = Enum.GetValues<QualificationReason>();
            values.Should().HaveCount(5);
        }
    }

    #endregion

    #region OpportunityPricingModel Enum Tests

    public class OpportunityPricingModelTests
    {
        [Theory]
        [InlineData(OpportunityPricingModel.Subscription, 0)]
        [InlineData(OpportunityPricingModel.OneTime, 1)]
        [InlineData(OpportunityPricingModel.UsageBased, 2)]
        [InlineData(OpportunityPricingModel.Hybrid, 3)]
        public void OpportunityPricingModel_ShouldHaveCorrectValue(OpportunityPricingModel model, int expected)
        {
            ((int)model).Should().Be(expected);
        }

        [Fact]
        public void OpportunityPricingModel_AllValues_ShouldHaveFourModels()
        {
            var values = Enum.GetValues<OpportunityPricingModel>();
            values.Should().HaveCount(4);
        }
    }

    #endregion

    #region Opportunity Entity Tests

    public class OpportunityTests
    {
        [Fact]
        public void Opportunity_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var opp = new Opportunity();

            // Assert - Basic defaults
            opp.Name.Should().BeEmpty();
            opp.Stage.Should().Be(OpportunityStage.Discovery);
            opp.Probability.Should().Be(10); // Discovery = 10%
            opp.Amount.Should().Be(0);
            opp.Currency.Should().Be("USD");
            opp.PricingModel.Should().Be(OpportunityPricingModel.Subscription);
            opp.TermLengthMonths.Should().Be(12);

            // Assert - Nullable fields
            opp.ExpectedCloseDate.Should().BeNull();
            opp.SolutionNotes.Should().BeNull();
            opp.QualificationReason.Should().BeNull();
            opp.QualificationNotes.Should().BeNull();
            opp.Region.Should().BeNull();

            // Assert - Foreign keys
            opp.AccountId.Should().Be(0); // Required, but defaults to 0
            opp.PrimaryContactId.Should().BeNull();
            opp.SalesOwnerId.Should().BeNull();
            opp.LeadId.Should().BeNull();

            // Assert - Navigation collections
            opp.Products.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenDiscovery_ShouldBeTrue()
        {
            var opp = new Opportunity { Stage = OpportunityStage.Discovery };
            opp.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenQualification_ShouldBeTrue()
        {
            var opp = new Opportunity { Stage = OpportunityStage.Qualification };
            opp.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenProposal_ShouldBeTrue()
        {
            var opp = new Opportunity { Stage = OpportunityStage.Proposal };
            opp.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenNegotiation_ShouldBeTrue()
        {
            var opp = new Opportunity { Stage = OpportunityStage.Negotiation };
            opp.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenClosedWon_ShouldBeFalse()
        {
            var opp = new Opportunity { Stage = OpportunityStage.ClosedWon };
            opp.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Opportunity_ComputedIsOpen_WhenClosedLost_ShouldBeFalse()
        {
            var opp = new Opportunity { Stage = OpportunityStage.ClosedLost };
            opp.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Opportunity_ComputedIsWon_WhenClosedWon_ShouldBeTrue()
        {
            var opp = new Opportunity { Stage = OpportunityStage.ClosedWon };
            opp.IsWon.Should().BeTrue();
        }

        [Fact]
        public void Opportunity_ComputedIsWon_WhenOtherStage_ShouldBeFalse()
        {
            var opp = new Opportunity { Stage = OpportunityStage.Negotiation };
            opp.IsWon.Should().BeFalse();
        }

        [Theory]
        [InlineData(100000, 10, 10000)]
        [InlineData(50000, 50, 25000)]
        [InlineData(200000, 75, 150000)]
        [InlineData(100000, 100, 100000)]
        [InlineData(100000, 0, 0)]
        public void Opportunity_ComputedWeightedAmount_ShouldCalculateCorrectly(
            decimal amount, int probability, decimal expectedWeighted)
        {
            // Arrange
            var opp = new Opportunity
            {
                Amount = amount,
                Probability = probability
            };

            // Assert
            opp.WeightedAmount.Should().Be(expectedWeighted);
        }

        [Fact]
        public void Opportunity_FullConfiguration_ShouldSetAllProperties()
        {
            // Arrange
            var closeDate = DateTime.UtcNow.AddDays(30);

            // Act
            var opp = new Opportunity
            {
                Id = 1,
                Name = "Acme Corp - Enterprise License",
                Stage = OpportunityStage.Negotiation,
                Probability = 75,
                Amount = 250000,
                Currency = "EUR",
                ExpectedCloseDate = closeDate,
                PricingModel = OpportunityPricingModel.Hybrid,
                TermLengthMonths = 24,
                SolutionNotes = "Multi-region deployment with premium support",
                QualificationReason = QualificationReason.Authority,
                QualificationNotes = "CTO is champion, CFO has budget approval",
                Region = "EMEA",
                AccountId = 100,
                PrimaryContactId = 200,
                SalesOwnerId = 300,
                LeadId = 400
            };

            // Assert
            opp.Name.Should().Be("Acme Corp - Enterprise License");
            opp.Stage.Should().Be(OpportunityStage.Negotiation);
            opp.Probability.Should().Be(75);
            opp.Amount.Should().Be(250000);
            opp.Currency.Should().Be("EUR");
            opp.ExpectedCloseDate.Should().Be(closeDate);
            opp.PricingModel.Should().Be(OpportunityPricingModel.Hybrid);
            opp.TermLengthMonths.Should().Be(24);
            opp.Region.Should().Be("EMEA");
            opp.QualificationReason.Should().Be(QualificationReason.Authority);
            opp.WeightedAmount.Should().Be(187500); // 250000 * 0.75
        }

        [Fact]
        public void Opportunity_InheritsFromBaseEntity()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            // Act
            var opp = new Opportunity
            {
                Id = 42,
                CreatedAt = now.AddDays(-7),
                UpdatedAt = now,
                IsDeleted = false,
                RowVersion = rowVersion
            };

            // Assert
            opp.Id.Should().Be(42);
            opp.CreatedAt.Should().Be(now.AddDays(-7));
            opp.UpdatedAt.Should().Be(now);
            opp.IsDeleted.Should().BeFalse();
            opp.RowVersion.Should().BeEquivalentTo(rowVersion);
        }
    }

    #endregion

    #region OpportunityProduct Junction Table Tests

    public class OpportunityProductTests
    {
        [Fact]
        public void OpportunityProduct_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var oppProduct = new OpportunityProduct();

            // Assert
            oppProduct.OpportunityId.Should().Be(0);
            oppProduct.ProductId.Should().Be(0);
            oppProduct.Quantity.Should().Be(1);
            oppProduct.UnitPrice.Should().BeNull();
            oppProduct.DiscountPercent.Should().BeNull();
            oppProduct.LineTotal.Should().BeNull();
            oppProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            oppProduct.Notes.Should().BeNull();
        }

        [Fact]
        public void OpportunityProduct_FullConfiguration_ShouldSetAllProperties()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;

            // Act
            var oppProduct = new OpportunityProduct
            {
                OpportunityId = 1,
                ProductId = 10,
                Quantity = 50,
                UnitPrice = 99.99m,
                DiscountPercent = 10,
                LineTotal = 4499.55m, // 50 * 99.99 * 0.90
                CreatedAt = createdAt,
                Notes = "Volume discount applied"
            };

            // Assert
            oppProduct.OpportunityId.Should().Be(1);
            oppProduct.ProductId.Should().Be(10);
            oppProduct.Quantity.Should().Be(50);
            oppProduct.UnitPrice.Should().Be(99.99m);
            oppProduct.DiscountPercent.Should().Be(10);
            oppProduct.LineTotal.Should().Be(4499.55m);
            oppProduct.Notes.Should().Be("Volume discount applied");
        }
    }

    #endregion

    #region LeadLifecycleStatus Enum Tests

    public class LeadLifecycleStatusTests
    {
        [Theory]
        [InlineData(LeadLifecycleStatus.New, 0)]
        [InlineData(LeadLifecycleStatus.Working, 1)]
        [InlineData(LeadLifecycleStatus.Nurturing, 2)]
        [InlineData(LeadLifecycleStatus.Qualified, 3)]
        [InlineData(LeadLifecycleStatus.Disqualified, 4)]
        [InlineData(LeadLifecycleStatus.Converted, 5)]
        public void LeadLifecycleStatus_ShouldHaveCorrectValue(LeadLifecycleStatus status, int expected)
        {
            ((int)status).Should().Be(expected);
        }

        [Fact]
        public void LeadLifecycleStatus_AllValues_ShouldHaveSixStatuses()
        {
            var values = Enum.GetValues<LeadLifecycleStatus>();
            values.Should().HaveCount(6);
        }

        [Fact]
        public void LeadLifecycleStatus_Names_ShouldBeReadable()
        {
            Enum.GetName(LeadLifecycleStatus.New).Should().Be("New");
            Enum.GetName(LeadLifecycleStatus.Working).Should().Be("Working");
            Enum.GetName(LeadLifecycleStatus.Nurturing).Should().Be("Nurturing");
            Enum.GetName(LeadLifecycleStatus.Qualified).Should().Be("Qualified");
            Enum.GetName(LeadLifecycleStatus.Disqualified).Should().Be("Disqualified");
            Enum.GetName(LeadLifecycleStatus.Converted).Should().Be("Converted");
        }
    }

    #endregion

    #region LeadSource Enum Tests

    public class LeadSourceTests
    {
        [Theory]
        [InlineData(LeadSource.Web, 0)]
        [InlineData(LeadSource.Campaign, 1)]
        [InlineData(LeadSource.Referral, 2)]
        [InlineData(LeadSource.Event, 3)]
        [InlineData(LeadSource.Partner, 4)]
        [InlineData(LeadSource.Manual, 5)]
        public void LeadSource_ShouldHaveCorrectValue(LeadSource source, int expected)
        {
            ((int)source).Should().Be(expected);
        }

        [Fact]
        public void LeadSource_AllValues_ShouldHaveSixSources()
        {
            var values = Enum.GetValues<LeadSource>();
            values.Should().HaveCount(6);
        }
    }

    #endregion

    #region Lead Entity Tests

    public class LeadTests
    {
        [Fact]
        public void Lead_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var lead = new Lead();

            // Assert - Status and Scoring defaults
            lead.Status.Should().Be(LeadLifecycleStatus.New);
            lead.Source.Should().Be(LeadSource.Web);
            lead.Score.Should().Be(0);
            lead.FitScore.Should().Be(0);
            lead.EngagementScore.Should().Be(0);

            // Assert - Contact info defaults
            lead.FirstName.Should().BeEmpty();
            lead.LastName.Should().BeEmpty();
            lead.Email.Should().BeEmpty();
            lead.Phone.Should().BeNull();
            lead.Title.Should().BeNull();
            lead.CompanyName.Should().BeNull();
            lead.Website.Should().BeNull();

            // Assert - Nullable fields
            lead.QualificationNotes.Should().BeNull();
            lead.MqlDate.Should().BeNull();
            lead.SqlDate.Should().BeNull();
            lead.Region.Should().BeNull();
            lead.Tags.Should().BeNull();
            lead.LastScoreDecayDate.Should().BeNull();
            lead.LastActivityDate.Should().BeNull();

            // Assert - Foreign keys
            lead.OwnerId.Should().BeNull();
            lead.CampaignId.Should().BeNull();
            lead.AccountId.Should().BeNull();
            lead.ContactId.Should().BeNull();

            // Assert - Merge tracking defaults
            lead.MergedIntoId.Should().BeNull();
            lead.MergeGroupId.Should().BeNull();
            lead.IsMergedDuplicate.Should().BeFalse();
            lead.MergedAt.Should().BeNull();

            // Assert - Navigation collections
            lead.ProductInterests.Should().NotBeNull().And.BeEmpty();
            lead.Opportunities.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void Lead_ComputedFullName_ShouldConcatenateCorrectly()
        {
            // Arrange & Act
            var lead = new Lead
            {
                FirstName = "John",
                LastName = "Smith"
            };

            // Assert
            lead.FullName.Should().Be("John Smith");
        }

        [Fact]
        public void Lead_ComputedFullName_WithOnlyFirstName_ShouldTrim()
        {
            var lead = new Lead { FirstName = "John", LastName = "" };
            lead.FullName.Should().Be("John");
        }

        [Fact]
        public void Lead_ComputedFullName_WithOnlyLastName_ShouldTrim()
        {
            var lead = new Lead { FirstName = "", LastName = "Smith" };
            lead.FullName.Should().Be("Smith");
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenNew_ShouldBeTrue()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.New };
            lead.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenWorking_ShouldBeTrue()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.Working };
            lead.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenNurturing_ShouldBeTrue()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.Nurturing };
            lead.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenQualified_ShouldBeTrue()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.Qualified };
            lead.IsOpen.Should().BeTrue();
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenDisqualified_ShouldBeFalse()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.Disqualified };
            lead.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Lead_ComputedIsOpen_WhenConverted_ShouldBeFalse()
        {
            var lead = new Lead { Status = LeadLifecycleStatus.Converted };
            lead.IsOpen.Should().BeFalse();
        }

        [Fact]
        public void Lead_LeadScoreAlias_ShouldMatchScore()
        {
            // Arrange
            var lead = new Lead { Score = 85 };

            // Assert - LeadScore is alias for Score
            lead.LeadScore.Should().Be(85);

            // Act - Set via alias
            lead.LeadScore = 90;

            // Assert
            lead.Score.Should().Be(90);
        }

        [Fact]
        public void Lead_FullConfiguration_ShouldSetAllProperties()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var mqlDate = now.AddDays(-10);
            var sqlDate = now.AddDays(-5);

            // Act
            var lead = new Lead
            {
                Id = 1,
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@example.com",
                Phone = "+1-555-123-4567",
                Title = "VP of Engineering",
                CompanyName = "TechCorp Inc",
                Website = "https://techcorp.com",
                Status = LeadLifecycleStatus.Qualified,
                Source = LeadSource.Event,
                Score = 85,
                FitScore = 40,
                EngagementScore = 45,
                MqlDate = mqlDate,
                SqlDate = sqlDate,
                QualificationNotes = "Strong technical fit, active evaluator",
                Region = "North America",
                Tags = "[\"enterprise\",\"saas\",\"priority\"]",
                OwnerId = 10,
                CampaignId = 20
            };

            // Assert
            lead.FullName.Should().Be("Jane Doe");
            lead.Email.Should().Be("jane.doe@example.com");
            lead.Phone.Should().Be("+1-555-123-4567");
            lead.Title.Should().Be("VP of Engineering");
            lead.CompanyName.Should().Be("TechCorp Inc");
            lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
            lead.Source.Should().Be(LeadSource.Event);
            lead.Score.Should().Be(85);
            lead.FitScore.Should().Be(40);
            lead.EngagementScore.Should().Be(45);
            lead.MqlDate.Should().Be(mqlDate);
            lead.SqlDate.Should().Be(sqlDate);
            lead.IsOpen.Should().BeTrue();
            lead.Tags.Should().Contain("enterprise");
        }

        [Fact]
        public void Lead_MergeTracking_CanBeConfigured()
        {
            // Arrange
            var mergedAt = DateTime.UtcNow;

            // Act
            var lead = new Lead
            {
                MergedIntoId = 100,
                MergeGroupId = 50,
                IsMergedDuplicate = true,
                MergedAt = mergedAt
            };

            // Assert
            lead.MergedIntoId.Should().Be(100);
            lead.MergeGroupId.Should().Be(50);
            lead.IsMergedDuplicate.Should().BeTrue();
            lead.MergedAt.Should().Be(mergedAt);
        }

        [Fact]
        public void Lead_InheritsFromBaseEntity()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var lead = new Lead
            {
                Id = 42,
                CreatedAt = now.AddDays(-14),
                UpdatedAt = now,
                IsDeleted = false
            };

            // Assert
            lead.Id.Should().Be(42);
            lead.CreatedAt.Should().Be(now.AddDays(-14));
            lead.UpdatedAt.Should().Be(now);
            lead.IsDeleted.Should().BeFalse();
        }
    }

    #endregion

    #region LeadProductInterest Junction Table Tests

    public class LeadProductInterestTests
    {
        [Fact]
        public void LeadProductInterest_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var interest = new LeadProductInterest();

            // Assert
            interest.LeadId.Should().Be(0);
            interest.ProductId.Should().Be(0);
            interest.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            interest.InterestLevel.Should().BeNull();
            interest.Notes.Should().BeNull();
        }

        [Fact]
        public void LeadProductInterest_FullConfiguration_ShouldSetAllProperties()
        {
            // Arrange
            var createdAt = DateTime.UtcNow;

            // Act
            var interest = new LeadProductInterest
            {
                LeadId = 1,
                ProductId = 10,
                InterestLevel = 4,
                Notes = "Downloaded whitepaper and requested demo",
                CreatedAt = createdAt
            };

            // Assert
            interest.LeadId.Should().Be(1);
            interest.ProductId.Should().Be(10);
            interest.InterestLevel.Should().Be(4);
            interest.Notes.Should().Be("Downloaded whitepaper and requested demo");
        }
    }

    #endregion

    #region Lead-to-Opportunity Conversion Scenario Tests

    public class LeadConversionScenarioTests
    {
        [Fact]
        public void LeadConversion_Scenario_ShouldCreateRelatedOpportunity()
        {
            // Scenario: SDR qualifies a lead and converts it to opportunity

            // Arrange - Create qualified lead
            var now = DateTime.UtcNow;
            var lead = new Lead
            {
                Id = 1,
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@bigcorp.com",
                CompanyName = "BigCorp Ltd",
                Status = LeadLifecycleStatus.Qualified,
                Source = LeadSource.Campaign,
                Score = 95,
                MqlDate = now.AddDays(-14),
                SqlDate = now.AddDays(-7),
                QualificationNotes = "BANT verified: Budget $200K, Decision by Q3",
                OwnerId = 10
            };

            // Act - Convert to opportunity
            lead.Status = LeadLifecycleStatus.Converted;

            var opportunity = new Opportunity
            {
                Id = 100,
                Name = $"{lead.CompanyName} - Enterprise Deal",
                Stage = OpportunityStage.Qualification,
                Probability = 25,
                Amount = 200000,
                ExpectedCloseDate = now.AddMonths(3),
                LeadId = lead.Id,
                SalesOwnerId = lead.OwnerId,
                QualificationNotes = lead.QualificationNotes
            };

            // Assert
            lead.Status.Should().Be(LeadLifecycleStatus.Converted);
            lead.IsOpen.Should().BeFalse();
            opportunity.LeadId.Should().Be(lead.Id);
            opportunity.Name.Should().Contain(lead.CompanyName!);
            opportunity.QualificationNotes.Should().Be(lead.QualificationNotes);
        }

        [Fact]
        public void OpportunityProgression_Scenario_ShouldUpdateProbability()
        {
            // Scenario: Opportunity moves through pipeline stages

            // Arrange
            var opp = new Opportunity
            {
                Name = "Big Deal",
                Amount = 100000,
                Stage = OpportunityStage.Discovery,
                Probability = 10
            };

            // Assert initial state
            opp.WeightedAmount.Should().Be(10000);
            opp.IsOpen.Should().BeTrue();
            opp.IsWon.Should().BeFalse();

            // Act - Progress to Qualification
            opp.Stage = OpportunityStage.Qualification;
            opp.Probability = 25;
            opp.WeightedAmount.Should().Be(25000);

            // Act - Progress to Proposal
            opp.Stage = OpportunityStage.Proposal;
            opp.Probability = 50;
            opp.WeightedAmount.Should().Be(50000);

            // Act - Progress to Negotiation
            opp.Stage = OpportunityStage.Negotiation;
            opp.Probability = 75;
            opp.WeightedAmount.Should().Be(75000);

            // Act - Close Won!
            opp.Stage = OpportunityStage.ClosedWon;
            opp.Probability = 100;

            // Assert final state
            opp.IsOpen.Should().BeFalse();
            opp.IsWon.Should().BeTrue();
            opp.WeightedAmount.Should().Be(100000);
        }

        [Fact]
        public void LeadDisqualification_Scenario()
        {
            // Scenario: Lead doesn't meet criteria and is disqualified

            // Arrange
            var lead = new Lead
            {
                FirstName = "Test",
                LastName = "Lead",
                Email = "test@example.com",
                Status = LeadLifecycleStatus.Working,
                Score = 30,
                FitScore = 10,
                EngagementScore = 20
            };

            // Act - Disqualify
            lead.Status = LeadLifecycleStatus.Disqualified;
            lead.QualificationNotes = "Company too small, no budget this year";

            // Assert
            lead.IsOpen.Should().BeFalse();
            lead.Status.Should().Be(LeadLifecycleStatus.Disqualified);
            lead.QualificationNotes.Should().Contain("no budget");
        }

        [Fact]
        public void OpportunityLost_Scenario()
        {
            // Scenario: Opportunity is lost to competition

            // Arrange
            var opp = new Opportunity
            {
                Name = "Lost Deal",
                Amount = 75000,
                Stage = OpportunityStage.Negotiation,
                Probability = 75
            };

            // Act - Close Lost
            opp.Stage = OpportunityStage.ClosedLost;
            opp.Probability = 0;
            opp.SolutionNotes = "Lost to competitor - better pricing";

            // Assert
            opp.IsOpen.Should().BeFalse();
            opp.IsWon.Should().BeFalse();
            opp.WeightedAmount.Should().Be(0);
        }
    }

    #endregion

    #region Lead Scoring Scenario Tests

    public class LeadScoringScenarioTests
    {
        [Fact]
        public void LeadScoring_HighScore_ShouldQualifyForMQL()
        {
            // Arrange - Lead with high engagement
            var lead = new Lead
            {
                FirstName = "Hot",
                LastName = "Lead",
                Email = "hot@prospect.com",
                Status = LeadLifecycleStatus.New,
                FitScore = 40,      // Good company fit
                EngagementScore = 45 // High engagement
            };

            // Act - Calculate combined score
            lead.Score = lead.FitScore + lead.EngagementScore;

            // Act - Check MQL threshold (typically 80+)
            if (lead.Score >= 80)
            {
                lead.Status = LeadLifecycleStatus.Qualified;
                lead.MqlDate = DateTime.UtcNow;
            }

            // Assert
            lead.Score.Should().Be(85);
            lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
            lead.MqlDate.Should().NotBeNull();
        }

        [Fact]
        public void LeadScoring_LowScore_ShouldNurture()
        {
            // Arrange - Lead with low engagement
            var lead = new Lead
            {
                FirstName = "Cold",
                LastName = "Lead",
                Email = "cold@prospect.com",
                Status = LeadLifecycleStatus.New,
                FitScore = 30,      // Moderate fit
                EngagementScore = 15 // Low engagement
            };

            // Act - Calculate combined score
            lead.Score = lead.FitScore + lead.EngagementScore;

            // Act - Check if needs nurturing (below threshold)
            if (lead.Score < 50)
            {
                lead.Status = LeadLifecycleStatus.Nurturing;
            }

            // Assert
            lead.Score.Should().Be(45);
            lead.Status.Should().Be(LeadLifecycleStatus.Nurturing);
            lead.IsOpen.Should().BeTrue();
        }
    }

    #endregion
}
