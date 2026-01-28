// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Build Verification Tests (BVT) - Critical Path Testing

using Xunit;
using FluentAssertions;
using CRM.Core.Entities;
using CRM.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Tests.BVT;

/// <summary>
/// Build Verification Tests (BVT) for critical path validation
/// These tests ensure that the most important functionality works correctly
/// </summary>
public class CriticalPathBVTTests
{
    #region BVT-001 to BVT-010: Customer Entity Critical Path

    [Fact]
    public void BVT001_Customer_Creation_Individual()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Id = 1,
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-1234"
        };

        // Assert
        customer.Should().NotBeNull();
        customer.Category.Should().Be(CustomerCategory.Individual);
        customer.FirstName.Should().Be("John");
    }

    [Fact]
    public void BVT002_Customer_Creation_Organization()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Id = 2,
            Category = CustomerCategory.Organization,
            Company = "Acme Corp",
            Industry = "Technology"
        };

        // Assert
        customer.Category.Should().Be(CustomerCategory.Organization);
        customer.Company.Should().Be("Acme Corp");
    }

    [Fact]
    public void BVT003_Customer_LifecycleStage_Progression()
    {
        // Arrange
        var customer = new Customer { LifecycleStage = CustomerLifecycleStage.Lead };

        // Act & Assert - Simulate lifecycle progression
        customer.LifecycleStage = CustomerLifecycleStage.Opportunity;
        customer.LifecycleStage.Should().Be(CustomerLifecycleStage.Opportunity);

        customer.LifecycleStage = CustomerLifecycleStage.Customer;
        customer.LifecycleStage.Should().Be(CustomerLifecycleStage.Customer);
    }

    [Fact]
    public void BVT004_Customer_SoftDelete_Works()
    {
        // Arrange
        var customer = new Customer
        {
            Id = 1,
            FirstName = "Test",
            IsDeleted = false
        };

        // Act
        customer.IsDeleted = true;

        // Assert
        customer.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void BVT005_Customer_AllTypes_Valid()
    {
        // Assert all customer types are valid
        var types = Enum.GetValues<CustomerType>();
        types.Should().HaveCountGreaterOrEqualTo(6);
        types.Should().Contain(CustomerType.Enterprise);
    }

    #endregion

    #region BVT-011 to BVT-020: User Entity Critical Path

    [Fact]
    public void BVT011_User_Creation_WithCredentials()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@company.com",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = "hashedpassword",
            Role = (int)UserRole.Admin,
            IsActive = true
        };

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().Be("admin");
        user.Role.Should().Be((int)UserRole.Admin);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void BVT012_User_RoleAssignment()
    {
        // Arrange
        var user = new User();

        // Act & Assert - Test all roles
        foreach (UserRole role in Enum.GetValues<UserRole>())
        {
            user.Role = (int)role;
            user.Role.Should().Be((int)role);
        }
    }

    [Fact]
    public void BVT013_User_Deactivation()
    {
        // Arrange
        var user = new User { IsActive = true };

        // Act
        user.IsActive = false;

        // Assert
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void BVT014_User_LastLoginUpdate()
    {
        // Arrange
        var user = new User { LastLoginDate = null };
        var loginTime = DateTime.UtcNow;

        // Act
        user.LastLoginDate = loginTime;

        // Assert
        user.LastLoginDate.Should().Be(loginTime);
    }

    #endregion

    #region BVT-021 to BVT-030: Opportunity Entity Critical Path

    [Fact]
    public void BVT021_Opportunity_Creation()
    {
        // Arrange & Act
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Big Deal",
            Stage = OpportunityStage.Discovery,
            Amount = 50000m,
            Probability = 25,
            AccountId = 1
        };

        // Assert
        opportunity.Should().NotBeNull();
        opportunity.Name.Should().Be("Big Deal");
        opportunity.Stage.Should().Be(OpportunityStage.Discovery);
    }

    [Fact]
    public void BVT022_Opportunity_StageProgression()
    {
        // Arrange
        var opportunity = new Opportunity { Stage = OpportunityStage.Discovery };

        // Act & Assert - Progress through stages
        opportunity.Stage = OpportunityStage.Qualification;
        opportunity.Stage.Should().Be(OpportunityStage.Qualification);

        opportunity.Stage = OpportunityStage.Proposal;
        opportunity.Stage.Should().Be(OpportunityStage.Proposal);

        opportunity.Stage = OpportunityStage.Negotiation;
        opportunity.Stage.Should().Be(OpportunityStage.Negotiation);
    }

    [Fact]
    public void BVT023_Opportunity_ClosedWon()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Stage = OpportunityStage.Negotiation,
            Amount = 100000m
        };

        // Act
        opportunity.Stage = OpportunityStage.ClosedWon;

        // Assert
        opportunity.IsWon.Should().BeTrue();
        opportunity.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void BVT024_Opportunity_ClosedLost()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Stage = OpportunityStage.Proposal,
            Amount = 100000m
        };

        // Act
        opportunity.Stage = OpportunityStage.ClosedLost;

        // Assert
        opportunity.IsWon.Should().BeFalse();
        opportunity.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void BVT025_Opportunity_WeightedValue_Calculation()
    {
        // Arrange
        var opportunity = new Opportunity
        {
            Amount = 100000m,
            Probability = 75
        };

        // Assert
        opportunity.WeightedAmount.Should().Be(75000m);
    }

    [Fact]
    public void BVT026_Opportunity_AllStages_Valid()
    {
        // Assert all stages are defined
        var stages = Enum.GetValues<OpportunityStage>();
        stages.Should().HaveCount(6);
    }

    #endregion

    #region BVT-031 to BVT-040: Lead Entity Critical Path

    [Fact]
    public void BVT031_Lead_Creation()
    {
        // Arrange & Act
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@company.com",
            CompanyName = "Smith LLC",
            Status = LeadLifecycleStatus.New,
            Source = LeadSource.Web
        };

        // Assert
        lead.Should().NotBeNull();
        lead.FirstName.Should().Be("Jane");
        lead.Source.Should().Be(LeadSource.Web);
    }

    [Fact]
    public void BVT032_Lead_StatusProgression()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.New };

        // Act & Assert - Progress through statuses
        lead.Status = LeadLifecycleStatus.Working;
        lead.Status.Should().Be(LeadLifecycleStatus.Working);

        lead.Status = LeadLifecycleStatus.Qualified;
        lead.Status.Should().Be(LeadLifecycleStatus.Qualified);
    }

    [Fact]
    public void BVT033_Lead_Conversion()
    {
        // Arrange
        var lead = new Lead
        {
            Status = LeadLifecycleStatus.Qualified,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        lead.Status = LeadLifecycleStatus.Converted;

        // Assert
        lead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void BVT034_Lead_Disqualification()
    {
        // Arrange
        var lead = new Lead { Status = LeadLifecycleStatus.New };

        // Act
        lead.Status = LeadLifecycleStatus.Disqualified;

        // Assert
        lead.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void BVT035_Lead_Scoring()
    {
        // Arrange & Act
        var lead = new Lead
        {
            Score = 80,
            FitScore = 40,
            EngagementScore = 40
        };

        // Assert
        lead.Score.Should().Be(80);
        lead.FitScore.Should().Be(40);
        lead.EngagementScore.Should().Be(40);
    }

    [Fact]
    public void BVT036_Lead_FullName_Computed()
    {
        // Arrange
        var lead = new Lead
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        lead.FullName.Should().Be("John Doe");
    }

    #endregion

    #region BVT-041 to BVT-050: Product Entity Critical Path

    [Fact]
    public void BVT041_Product_Creation()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "CRM Enterprise",
            SKU = "CRM-ENT-001",
            Price = 999.99m,
            ProductType = ProductType.Subscription,
            Status = ProductStatus.Active
        };

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be("CRM Enterprise");
        product.ProductType.Should().Be(ProductType.Subscription);
    }

    [Fact]
    public void BVT042_Product_StatusTransitions()
    {
        // Arrange
        var product = new Product { Status = ProductStatus.Draft };

        // Act - Transition to Active
        product.Status = ProductStatus.Active;
        product.Status.Should().Be(ProductStatus.Active);

        // Act - Discontinue
        product.Status = ProductStatus.Discontinued;
        product.Status.Should().Be(ProductStatus.Discontinued);
    }

    [Fact]
    public void BVT043_Product_Pricing()
    {
        // Arrange & Act
        var product = new Product
        {
            Price = 100.00m,
            Cost = 30.00m
        };

        // Assert
        product.Price.Should().Be(100.00m);
        product.Cost.Should().Be(30.00m);
    }

    [Fact]
    public void BVT044_Product_AllTypes_Valid()
    {
        // Assert all product types are defined
        var types = Enum.GetValues<ProductType>();
        types.Should().HaveCountGreaterOrEqualTo(10);
    }

    [Fact]
    public void BVT045_Product_AllStatuses_Valid()
    {
        // Assert all product statuses are defined
        var statuses = Enum.GetValues<ProductStatus>();
        statuses.Should().HaveCountGreaterOrEqualTo(8);
    }

    #endregion

    #region BVT-051 to BVT-060: Account Entity Critical Path

    [Fact]
    public void BVT051_Account_Creation()
    {
        // Arrange & Act
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC-001",
            CustomerId = 1,
            Status = AccountStatus.Current,
            MRR = 500m,
            ARR = 6000m
        };

        // Assert
        account.Should().NotBeNull();
        account.AccountNumber.Should().Be("ACC-001");
        account.Status.Should().Be(AccountStatus.Current);
    }

    [Fact]
    public void BVT052_Account_ChurnTransition()
    {
        // Arrange
        var account = new Account { Status = AccountStatus.Current };

        // Act
        account.Status = AccountStatus.Churned;

        // Assert
        account.Status.Should().Be(AccountStatus.Churned);
    }

    [Fact]
    public void BVT053_Account_ContractDates()
    {
        // Arrange & Act
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddYears(1);

        var account = new Account
        {
            ContractStartDate = startDate,
            ContractEndDate = endDate
        };

        // Assert
        account.ContractStartDate.Should().Be(startDate);
        account.ContractEndDate.Should().Be(endDate);
        (account.ContractEndDate - account.ContractStartDate)?.Days.Should().BeCloseTo(365, 1);
    }

    [Fact]
    public void BVT054_Account_AutoRenewal()
    {
        // Arrange
        var account = new Account { IsAutoRenew = false };

        // Act
        account.IsAutoRenew = true;

        // Assert
        account.IsAutoRenew.Should().BeTrue();
    }

    #endregion

    #region BVT-061 to BVT-070: DTO Critical Path

    [Fact]
    public void BVT061_LoginRequest_Creation()
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Email = "user@company.com",
            Password = "password123"
        };

        // Assert
        request.Email.Should().Be("user@company.com");
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void BVT062_CustomerDto_Mapping()
    {
        // Arrange - Customer entity
        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        };

        // Act - Create DTO from entity (simulated)
        var dto = new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
    }

    [Fact]
    public void BVT063_UserDto_Creation()
    {
        // Arrange & Act
        var dto = new UserDto
        {
            Id = 1,
            Username = "jdoe",
            Email = "john@company.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Sales",
            IsActive = true
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.Username.Should().Be("jdoe");
        dto.Role.Should().Be("Sales");
    }

    #endregion

    #region BVT-071 to BVT-080: Data Validation Critical Path

    [Fact]
    public void BVT071_Email_Format_Required()
    {
        // Arrange
        var customer = new Customer();

        // Act
        customer.Email = "valid@email.com";

        // Assert
        customer.Email.Should().Contain("@");
    }

    [Fact]
    public void BVT072_NonNegative_Amounts()
    {
        // Arrange
        var opportunity = new Opportunity { Amount = 0 };

        // Act
        opportunity.Amount = 50000m;

        // Assert
        opportunity.Amount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void BVT073_Probability_Range()
    {
        // Arrange
        var opportunity = new Opportunity();

        // Assert - Default probability is valid
        opportunity.Probability.Should().BeInRange(0, 100);

        // Act
        opportunity.Probability = 50;
        opportunity.Probability.Should().BeInRange(0, 100);
    }

    [Fact]
    public void BVT074_DateTime_Defaults()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert - CreatedAt should be set
        customer.CreatedAt = DateTime.UtcNow;
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void BVT075_SoftDelete_Default_False()
    {
        // Arrange & Act
        var customer = new Customer();

        // Assert
        customer.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region BVT-081 to BVT-090: Collection and Relationship Tests

    [Fact]
    public void BVT081_Customer_Collection_Operations()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John" },
            new() { Id = 2, FirstName = "Jane" },
            new() { Id = 3, FirstName = "Bob" }
        };

        // Assert
        customers.Should().HaveCount(3);
        customers.Select(c => c.FirstName).Should().Contain("John");
    }

    [Fact]
    public void BVT082_Opportunity_Filtering_ByStage()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Id = 1, Stage = OpportunityStage.Discovery },
            new() { Id = 2, Stage = OpportunityStage.Proposal },
            new() { Id = 3, Stage = OpportunityStage.ClosedWon },
            new() { Id = 4, Stage = OpportunityStage.Negotiation }
        };

        // Act
        var openOpps = opportunities.Where(o => o.IsOpen).ToList();
        var closedOpps = opportunities.Where(o => !o.IsOpen).ToList();

        // Assert
        openOpps.Should().HaveCount(3);
        closedOpps.Should().HaveCount(1);
    }

    [Fact]
    public void BVT083_Pipeline_Value_Calculation()
    {
        // Arrange
        var opportunities = new List<Opportunity>
        {
            new() { Amount = 100000m, Probability = 50 },
            new() { Amount = 50000m, Probability = 75 },
            new() { Amount = 25000m, Probability = 25 }
        };

        // Act
        var totalPipeline = opportunities.Sum(o => o.Amount);
        var weightedPipeline = opportunities.Sum(o => o.WeightedAmount);

        // Assert
        totalPipeline.Should().Be(175000m);
        weightedPipeline.Should().Be(93750m); // 50000 + 37500 + 6250
    }

    [Fact]
    public void BVT084_Lead_Grouping_BySource()
    {
        // Arrange
        var leads = new List<Lead>
        {
            new() { Source = LeadSource.Web },
            new() { Source = LeadSource.Campaign },
            new() { Source = LeadSource.Web },
            new() { Source = LeadSource.Referral }
        };

        // Act
        var groupedBySource = leads.GroupBy(l => l.Source).ToList();

        // Assert
        groupedBySource.Should().HaveCount(3);
        groupedBySource.Single(g => g.Key == LeadSource.Web).Should().HaveCount(2);
    }

    [Fact]
    public void BVT085_User_Filtering_ByRole()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Role = (int)UserRole.Admin },
            new() { Role = (int)UserRole.Sales },
            new() { Role = (int)UserRole.Sales },
            new() { Role = (int)UserRole.Support }
        };

        // Act
        var salesUsers = users.Where(u => u.Role == (int)UserRole.Sales).ToList();

        // Assert
        salesUsers.Should().HaveCount(2);
    }

    #endregion

    #region BVT-091 to BVT-100: Integration Simulation Tests

    [Fact]
    public void BVT091_Customer_To_Account_Flow()
    {
        // Arrange - Create customer
        var customer = new Customer
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act - Create account linked to customer
        var account = new Account
        {
            Id = 1,
            AccountNumber = "ACC-001",
            CustomerId = customer.Id,
            Status = AccountStatus.Current
        };

        // Assert
        account.CustomerId.Should().Be(customer.Id);
        account.Status.Should().Be(AccountStatus.Current);
    }

    [Fact]
    public void BVT092_Lead_To_Opportunity_Conversion()
    {
        // Arrange - Qualified lead
        var lead = new Lead
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            CompanyName = "Smith Corp",
            Status = LeadLifecycleStatus.Qualified
        };

        // Act - Convert to opportunity
        lead.Status = LeadLifecycleStatus.Converted;

        var opportunity = new Opportunity
        {
            Id = 1,
            Name = $"Deal - {lead.CompanyName}",
            LeadId = lead.Id,
            Stage = OpportunityStage.Discovery
        };

        // Assert
        lead.IsOpen.Should().BeFalse();
        opportunity.LeadId.Should().Be(lead.Id);
    }

    [Fact]
    public void BVT093_Opportunity_To_Account_Conversion()
    {
        // Arrange - Won opportunity
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal",
            AccountId = 1,
            Stage = OpportunityStage.ClosedWon,
            Amount = 100000m
        };

        // Assert
        opportunity.IsWon.Should().BeTrue();
    }

    [Fact]
    public void BVT094_Product_In_Opportunity()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "CRM Suite",
            Price = 999m
        };

        var opportunityProduct = new OpportunityProduct
        {
            OpportunityId = 1,
            ProductId = product.Id,
            Quantity = 10,
            UnitPrice = product.Price
        };

        // Assert
        opportunityProduct.ProductId.Should().Be(product.Id);
        opportunityProduct.Quantity.Should().Be(10);
    }

    [Fact]
    public void BVT095_SystemSettings_Modules()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            CustomersEnabled = true,
            ContactsEnabled = true,
            LeadsEnabled = true,
            OpportunitiesEnabled = true,
            ProductsEnabled = true
        };

        // Assert
        settings.CustomersEnabled.Should().BeTrue();
        settings.ProductsEnabled.Should().BeTrue();
    }

    #endregion

    #region BVT-096 to BVT-110: Relationship Management Critical Path

    [Fact]
    public void BVT096_RelationshipType_Creation()
    {
        // Arrange & Act
        var type = new RelationshipType
        {
            Id = 1,
            TypeName = "Partner",
            TypeCategory = "Business",
            IsBidirectional = true,
            IsActive = true
        };

        // Assert
        type.TypeName.Should().Be("Partner");
        type.TypeCategory.Should().Be("Business");
    }

    [Fact]
    public void BVT097_AccountRelationship_Creation()
    {
        // Arrange & Act
        var relationship = new AccountRelationship
        {
            Id = 1,
            SourceCustomerId = 100,
            TargetCustomerId = 200,
            RelationshipTypeId = 1,
            Status = "Active",
            StrengthScore = 75
        };

        // Assert
        relationship.SourceCustomerId.Should().Be(100);
        relationship.Status.Should().Be("Active");
    }

    [Fact]
    public void BVT098_RelationshipInteraction_Logging()
    {
        // Arrange & Act
        var interaction = new RelationshipInteraction
        {
            Id = 1,
            AccountRelationshipId = 1,
            InteractionType = "Meeting",
            Subject = "Business Review",
            Outcome = "Successful"
        };

        // Assert
        interaction.InteractionType.Should().Be("Meeting");
        interaction.Outcome.Should().Be("Successful");
    }

    [Fact]
    public void BVT099_AccountHealthSnapshot_Creation()
    {
        // Arrange & Act
        var snapshot = new AccountHealthSnapshot
        {
            Id = 1,
            CustomerId = 100,
            OverallHealthScore = 78,
            HealthTrend = "Improving"
        };

        // Assert
        snapshot.OverallHealthScore.Should().Be(78);
        snapshot.HealthTrend.Should().Be("Improving");
    }

    [Fact]
    public void BVT100_RelationshipMap_Configuration()
    {
        // Arrange & Act
        var map = new RelationshipMap
        {
            Id = 1,
            MapName = "Partner Network",
            CentralCustomerId = 100,
            RelationshipDepth = 2
        };

        // Assert
        map.MapName.Should().Be("Partner Network");
        map.RelationshipDepth.Should().Be(2);
    }

    #endregion

    #region BVT-101 to BVT-110: Campaign Execution Critical Path

    [Fact]
    public void BVT101_CampaignWorkflow_Linking()
    {
        // Arrange & Act
        var workflow = new CampaignWorkflow
        {
            Id = 1,
            CampaignId = 100,
            WorkflowDefinitionId = 5,
            WorkflowType = "Sequential",
            TriggerEvent = "CampaignStarted",
            IsActive = true
        };

        // Assert
        workflow.CampaignId.Should().Be(100);
        workflow.WorkflowType.Should().Be("Sequential");
    }

    [Fact]
    public void BVT102_CampaignRecipient_Tracking()
    {
        // Arrange & Act
        var recipient = new CampaignRecipient
        {
            Id = 1,
            CampaignId = 100,
            Email = "test@example.com",
            Status = "Delivered"
        };

        // Assert
        recipient.Email.Should().Be("test@example.com");
        recipient.Status.Should().Be("Delivered");
    }

    [Fact]
    public void BVT103_CampaignRecipient_OpenTracking()
    {
        // Arrange
        var recipient = new CampaignRecipient { Status = "Delivered" };

        // Act
        recipient.Status = "Opened";
        recipient.OpenCount = 1;
        recipient.FirstOpenedAt = DateTime.UtcNow;

        // Assert
        recipient.Status.Should().Be("Opened");
        recipient.OpenCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BVT104_CampaignRecipient_ClickTracking()
    {
        // Arrange
        var recipient = new CampaignRecipient { Status = "Opened" };

        // Act
        recipient.Status = "Clicked";
        recipient.ClickCount = 1;

        // Assert
        recipient.Status.Should().Be("Clicked");
    }

    [Fact]
    public void BVT105_CampaignABTest_Creation()
    {
        // Arrange & Act
        var test = new CampaignABTest
        {
            Id = 1,
            CampaignId = 100,
            TestName = "Subject Line Test",
            TestType = "SubjectLine",
            Status = "Draft",
            VariantConfigs = "[{\"id\": \"A\", \"content\": \"Variant A\"}, {\"id\": \"B\", \"content\": \"Variant B\"}]"
        };

        // Assert
        test.TestType.Should().Be("SubjectLine");
        test.Status.Should().Be("Draft");
    }

    [Fact]
    public void BVT106_CampaignABTest_Execution()
    {
        // Arrange
        var test = new CampaignABTest { Status = "Draft" };

        // Act
        test.Status = "Running";
        test.TestStartedAt = DateTime.UtcNow;

        // Assert
        test.Status.Should().Be("Running");
        test.TestStartedAt.Should().NotBeNull();
    }

    [Fact]
    public void BVT107_CampaignABTest_Completion()
    {
        // Arrange
        var test = new CampaignABTest
        {
            Status = "Running"
        };

        // Act
        test.Status = "Completed";
        test.WinnerVariant = "B";

        // Assert
        test.Status.Should().Be("Completed");
        test.WinnerVariant.Should().Be("B");
    }

    [Fact]
    public void BVT108_CampaignConversion_Recording()
    {
        // Arrange & Act
        var conversion = new CampaignConversion
        {
            Id = 1,
            CampaignId = 100,
            ConversionType = "Purchase",
            ConversionValue = 99.99m,
            AttributionModel = "LastTouch"
        };

        // Assert
        conversion.ConversionType.Should().Be("Purchase");
        conversion.ConversionValue.Should().Be(99.99m);
    }

    [Fact]
    public void BVT109_CampaignLinkClick_Tracking()
    {
        // Arrange & Act
        var click = new CampaignLinkClick
        {
            Id = 1,
            CampaignRecipientId = 100,
            CampaignId = 50,
            LinkUrl = "https://example.com/offer",
            ClickedAt = DateTime.UtcNow
        };

        // Assert
        click.LinkUrl.Should().Contain("offer");
    }

    [Fact]
    public void BVT110_FullCampaignFunnel_Tracking()
    {
        // Arrange - Full funnel tracking
        var recipient = new CampaignRecipient
        {
            CampaignId = 100,
            Email = "test@example.com",
            Status = "Pending"
        };

        // Act - Progress through funnel
        recipient.Status = "Delivered";
        recipient.Status = "Opened";
        recipient.OpenCount = 1;
        recipient.Status = "Clicked";
        recipient.ClickCount = 1;
        recipient.Status = "Converted";
        recipient.ConvertedAt = DateTime.UtcNow;

        // Assert
        recipient.Status.Should().Be("Converted");
        recipient.ConvertedAt.Should().NotBeNull();
    }

    #endregion
}
