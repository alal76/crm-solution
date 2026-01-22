// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using Xunit;
using FluentAssertions;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Entities.WorkflowEngine;
using System;
using System.Collections.Generic;

namespace CRM.Tests.BVT;

/// <summary>
/// Build Verification Tests (BVT) for CRM Solution
/// 
/// FUNCTIONAL VIEW:
/// These are critical path tests that verify the most essential functionality
/// of the CRM system works correctly. They should be run:
/// - Before every deployment to production
/// - After any major code changes
/// - As part of CI/CD pipeline
/// 
/// TECHNICAL VIEW:
/// - Tests entity creation and validation
/// - Tests DTO mappings
/// - Tests enum values
/// - No database required - all in-memory
/// </summary>
public class CriticalPathTests
{
    #region BVT-001: Customer Entity Creation

    /// <summary>
    /// BVT-001: Verify that Customer entities can be created with all required fields
    /// 
    /// FUNCTIONAL: The system must be able to create customer records
    /// TECHNICAL: Tests Customer entity instantiation and property assignment
    /// </summary>
    [Fact]
    public void BVT001_CustomerEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var customer = new Customer
        {
            Id = 1,
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1-555-0001",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        customer.Id.Should().Be(1);
        customer.Category.Should().Be(CustomerCategory.Individual);
        customer.FirstName.Should().Be("John");
        customer.LastName.Should().Be("Doe");
        customer.Email.Should().Be("john.doe@example.com");
        customer.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region BVT-002: Organization Customer Creation

    /// <summary>
    /// BVT-002: Verify organization customers have company name
    /// 
    /// FUNCTIONAL: Organizations must have a company name
    /// TECHNICAL: Tests Organization category with Company field
    /// </summary>
    [Fact]
    public void BVT002_OrganizationCustomer_ShouldHaveCompanyName()
    {
        // Arrange & Act
        var organization = new Customer
        {
            Id = 1,
            Category = CustomerCategory.Organization,
            Company = "Acme Corporation",
            Email = "contact@acme.com",
            Phone = "+1-555-0002",
            Industry = "Technology",
            AnnualRevenue = 5000000m,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        organization.Category.Should().Be(CustomerCategory.Organization);
        organization.Company.Should().Be("Acme Corporation");
        organization.Industry.Should().Be("Technology");
        organization.AnnualRevenue.Should().Be(5000000m);
    }

    #endregion

    #region BVT-003: User Entity Creation

    /// <summary>
    /// BVT-003: Verify User entities can be created
    /// 
    /// FUNCTIONAL: The system must support user management
    /// TECHNICAL: Tests User entity with all core fields
    /// </summary>
    [Fact]
    public void BVT003_UserEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var user = new User
        {
            Id = 1,
            Username = "johndoe",
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = "hashed_password",
            Role = (int)UserRole.Sales,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        user.Id.Should().Be(1);
        user.Username.Should().Be("johndoe");
        user.Email.Should().Be("john@example.com");
        user.IsActive.Should().BeTrue();
        user.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region BVT-004: CustomerDto Mapping

    /// <summary>
    /// BVT-004: Verify CustomerDto can hold mapped data
    /// 
    /// FUNCTIONAL: Customer data must be transferable via DTOs
    /// TECHNICAL: Tests CustomerDto properties
    /// </summary>
    [Fact]
    public void BVT004_CustomerDto_CanHoldCustomerData()
    {
        // Arrange & Act
        var dto = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-0001",
            Category = "Individual",
            DisplayName = "John Doe"
        };

        // Assert
        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("John");
        dto.Category.Should().Be("Individual");
        dto.DisplayName.Should().Be("John Doe");
    }

    #endregion

    #region BVT-005: Customer Category Enum

    /// <summary>
    /// BVT-005: Verify CustomerCategory enum values
    /// 
    /// FUNCTIONAL: System must distinguish between Individual and Organization
    /// TECHNICAL: Tests enum values
    /// </summary>
    [Fact]
    public void BVT005_CustomerCategory_HasCorrectValues()
    {
        // Assert
        CustomerCategory.Individual.Should().Be(CustomerCategory.Individual);
        CustomerCategory.Organization.Should().Be(CustomerCategory.Organization);
        
        // Verify enum has exactly 2 values
        Enum.GetValues(typeof(CustomerCategory)).Length.Should().Be(2);
    }

    #endregion

    #region BVT-006: User Role Enum

    /// <summary>
    /// BVT-006: Verify UserRole enum values
    /// 
    /// FUNCTIONAL: System must support Admin and User roles
    /// TECHNICAL: Tests UserRole enum values
    /// </summary>
    [Fact]
    public void BVT006_UserRole_HasCorrectValues()
    {
        // Assert
        ((int)UserRole.Admin).Should().Be(0);
        ((int)UserRole.Manager).Should().Be(1);
        ((int)UserRole.Sales).Should().Be(2);
        ((int)UserRole.Support).Should().Be(3);
    }

    #endregion

    #region BVT-007: Customer Lifecycle Stage

    /// <summary>
    /// BVT-007: Verify CustomerLifecycleStage enum values
    /// 
    /// FUNCTIONAL: Customers progress through lifecycle stages
    /// TECHNICAL: Tests lifecycle stage enum values
    /// </summary>
    [Fact]
    public void BVT007_CustomerLifecycleStage_HasCorrectValues()
    {
        // Arrange
        var customer = new Customer();

        // Assert - default should be Lead
        customer.LifecycleStage.Should().Be(CustomerLifecycleStage.Lead);
        
        // Verify all stages exist
        CustomerLifecycleStage.Lead.Should().BeDefined();
        CustomerLifecycleStage.Prospect.Should().BeDefined();
        CustomerLifecycleStage.Opportunity.Should().BeDefined();
        CustomerLifecycleStage.Customer.Should().BeDefined();
    }

    #endregion

    #region BVT-008: Product Entity

    /// <summary>
    /// BVT-008: Verify Product entity creation
    /// 
    /// FUNCTIONAL: Products must have name, price, and quantity
    /// TECHNICAL: Tests Product entity fields
    /// </summary>
    [Fact]
    public void BVT008_ProductEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var product = new Product
        {
            Id = 1,
            Name = "Premium CRM License",
            Description = "Enterprise CRM solution",
            Price = 999.99m,
            Quantity = 100,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        product.Id.Should().Be(1);
        product.Name.Should().Be("Premium CRM License");
        product.Price.Should().Be(999.99m);
        product.Quantity.Should().Be(100);
        product.IsActive.Should().BeTrue();
    }

    #endregion

    #region BVT-009: Opportunity Entity

    /// <summary>
    /// BVT-009: Verify Opportunity entity creation
    /// 
    /// FUNCTIONAL: Sales opportunities must link to customers
    /// TECHNICAL: Tests Opportunity entity with CustomerId
    /// </summary>
    [Fact]
    public void BVT009_OpportunityEntity_CanBeCreatedWithCustomerLink()
    {
        // Arrange & Act
        var opportunity = new Opportunity
        {
            Id = 1,
            Name = "Enterprise Deal",
            CustomerId = 10,
            Stage = OpportunityStage.Qualification,
            Amount = 50000m,
            Probability = 25,
            CloseDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        opportunity.Id.Should().Be(1);
        opportunity.CustomerId.Should().Be(10);
        opportunity.Stage.Should().Be(OpportunityStage.Qualification);
        opportunity.Amount.Should().Be(50000m);
        opportunity.Probability.Should().Be(25);
    }

    #endregion

    #region BVT-010: Department Entity

    /// <summary>
    /// BVT-010: Verify Department entity creation
    /// 
    /// FUNCTIONAL: Users belong to departments
    /// TECHNICAL: Tests Department entity fields
    /// </summary>
    [Fact]
    public void BVT010_DepartmentEntity_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var department = new Department
        {
            Id = 1,
            Name = "Sales",
            Description = "Sales and Business Development",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        department.Id.Should().Be(1);
        department.Name.Should().Be("Sales");
        department.Description.Should().Be("Sales and Business Development");
        department.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region BVT-011: AuthResponse DTO

    /// <summary>
    /// BVT-011: Verify AuthResponse DTO structure
    /// 
    /// FUNCTIONAL: Authentication must return user info and tokens
    /// TECHNICAL: Tests AuthResponse DTO fields
    /// </summary>
    [Fact]
    public void BVT011_AuthResponse_HasRequiredFields()
    {
        // Arrange & Act
        var response = new AuthResponse
        {
            UserId = 1,
            Username = "johndoe",
            Email = "john@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin",
            AccessToken = "jwt_token_here",
            RefreshToken = "refresh_token_here",
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Assert
        response.UserId.Should().Be(1);
        response.Username.Should().Be("johndoe");
        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
        response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    #endregion

    #region BVT-012: CustomerContact Junction Entity

    /// <summary>
    /// BVT-012: Verify CustomerContact junction for organization contacts
    /// 
    /// FUNCTIONAL: Organizations can have multiple contacts
    /// TECHNICAL: Tests CustomerContact junction entity
    /// </summary>
    [Fact]
    public void BVT012_CustomerContact_LinksContactToCustomer()
    {
        // Arrange & Act
        var customerContact = new CustomerContact
        {
            Id = 1,
            CustomerId = 10,
            ContactId = 20,
            Role = CustomerContactRole.Primary,
            IsPrimaryContact = true,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        customerContact.CustomerId.Should().Be(10);
        customerContact.ContactId.Should().Be(20);
        customerContact.Role.Should().Be(CustomerContactRole.Primary);
        customerContact.IsPrimaryContact.Should().BeTrue();
    }

    #endregion

    #region BVT-013: Workflow Definition Entity

    /// <summary>
    /// BVT-013: Verify WorkflowDefinition entity creation
    /// 
    /// FUNCTIONAL: Workflow automation must support definition storage
    /// TECHNICAL: Tests WorkflowDefinition entity fields
    /// </summary>
    [Fact]
    public void BVT013_WorkflowDefinition_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var workflow = new WorkflowDefinition
        {
            Id = 1,
            Name = "Lead Qualification Process",
            Description = "Automated lead qualification workflow",
            Version = "1.0.0",
            Status = WorkflowDefinitionStatus.Published,
            TriggerType = WorkflowTriggerTypes.Event,
            TriggerEntityType = "Customer",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = 1
        };

        // Assert
        workflow.Id.Should().Be(1);
        workflow.Name.Should().Be("Lead Qualification Process");
        workflow.Status.Should().Be(WorkflowDefinitionStatus.Published);
        workflow.TriggerType.Should().Be(WorkflowTriggerTypes.Event);
    }

    #endregion

    #region BVT-014: Workflow Instance Entity

    /// <summary>
    /// BVT-014: Verify WorkflowInstance entity creation
    /// 
    /// FUNCTIONAL: Running workflows must track state
    /// TECHNICAL: Tests WorkflowInstance entity status transitions
    /// </summary>
    [Fact]
    public void BVT014_WorkflowInstance_CanTrackExecutionState()
    {
        // Arrange & Act
        var instance = new WorkflowInstance
        {
            Id = 1,
            WorkflowDefinitionId = 10,
            Status = WorkflowInstanceStatus.Running,
            StartedAt = DateTime.UtcNow,
            StartedByUserId = 1,
            CurrentStepKey = "step_1",
            EntityType = "Customer",
            WorkflowVersion = "1.0.0"
        };

        // Assert
        instance.WorkflowDefinitionId.Should().Be(10);
        instance.Status.Should().Be(WorkflowInstanceStatus.Running);
        instance.CurrentStepKey.Should().Be("step_1");
        instance.EntityType.Should().Be("Customer");
    }

    #endregion

    #region BVT-015: Workflow Instance Status Enum

    /// <summary>
    /// BVT-015: Verify WorkflowInstanceStatus enum values
    /// 
    /// FUNCTIONAL: Workflows must have clear status indicators
    /// TECHNICAL: Tests WorkflowInstanceStatus enum
    /// </summary>
    [Fact]
    public void BVT015_WorkflowInstanceStatus_HasCorrectValues()
    {
        // Assert - WorkflowInstanceStatus is a static class with string constants
        WorkflowInstanceStatus.Pending.Should().Be("Pending");
        WorkflowInstanceStatus.Running.Should().Be("Running");
        WorkflowInstanceStatus.Completed.Should().Be("Completed");
        WorkflowInstanceStatus.Failed.Should().Be("Failed");
        WorkflowInstanceStatus.Cancelled.Should().Be("Cancelled");
        WorkflowInstanceStatus.Paused.Should().Be("Paused");
    }

    #endregion

    #region BVT-016: Contact Model

    /// <summary>
    /// BVT-016: Verify Contact model creation
    /// 
    /// FUNCTIONAL: Contacts must store individual information
    /// TECHNICAL: Tests Contact model fields (using CRM.Core.Models)
    /// </summary>
    [Fact]
    public void BVT016_ContactModel_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var contact = new CRM.Core.Models.Contact
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            EmailPrimary = "jane@example.com",
            PhonePrimary = "+1-555-0001",
            Company = "Acme Corp",
            JobTitle = "Sales Manager",
            DateAdded = DateTime.UtcNow
        };

        // Assert
        contact.Id.Should().Be(1);
        contact.FirstName.Should().Be("Jane");
        contact.EmailPrimary.Should().Be("jane@example.com");
        contact.Company.Should().Be("Acme Corp");
    }

    #endregion

    #region BVT-017: System Settings Entity

    /// <summary>
    /// BVT-017: Verify SystemSettings entity creation
    /// 
    /// FUNCTIONAL: System must support configurable settings
    /// TECHNICAL: Tests SystemSettings entity module toggles
    /// </summary>
    [Fact]
    public void BVT017_SystemSettings_CanStoreModuleConfiguration()
    {
        // Arrange & Act
        var settings = new SystemSettings
        {
            Id = 1,
            CustomersEnabled = true,
            ContactsEnabled = true,
            LeadsEnabled = false,
            OpportunitiesEnabled = true,
            WorkflowsEnabled = true,
            CompanyName = "Test Company",
            MinPasswordLength = 8,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        settings.CustomersEnabled.Should().BeTrue();
        settings.LeadsEnabled.Should().BeFalse();
        settings.CompanyName.Should().Be("Test Company");
    }

    #endregion

    #region BVT-018: Service Request Entity

    /// <summary>
    /// BVT-018: Verify ServiceRequest entity creation
    /// 
    /// FUNCTIONAL: Support tickets must track customer issues
    /// TECHNICAL: Tests ServiceRequest entity fields
    /// </summary>
    [Fact]
    public void BVT018_ServiceRequest_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var serviceRequest = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-001",
            Subject = "Login Issue",
            Description = "Cannot login to the system",
            CustomerId = 10,
            Priority = ServiceRequestPriority.High,
            Status = ServiceRequestStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        serviceRequest.Id.Should().Be(1);
        serviceRequest.Subject.Should().Be("Login Issue");
        serviceRequest.CustomerId.Should().Be(10);
        serviceRequest.Priority.Should().Be(ServiceRequestPriority.High);
        serviceRequest.Status.Should().Be(ServiceRequestStatus.New);
    }

    #endregion

    #region BVT-019: Activity Entity

    /// <summary>
    /// BVT-019: Verify Activity entity for tracking user activities
    /// 
    /// FUNCTIONAL: Activities must log user interactions
    /// TECHNICAL: Tests Activity entity fields
    /// </summary>
    [Fact]
    public void BVT019_Activity_CanTrackUserInteractions()
    {
        // Arrange & Act
        var activity = new Activity
        {
            Id = 1,
            Title = "Call with prospect",
            Description = "Discussed pricing and requirements",
            ActivityType = ActivityType.CallMade,
            CustomerId = 10,
            UserId = 5,
            ActivityDate = DateTime.UtcNow,
            DurationMinutes = 30,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        activity.Title.Should().Be("Call with prospect");
        activity.ActivityType.Should().Be(ActivityType.CallMade);
        activity.CustomerId.Should().Be(10);
        activity.DurationMinutes.Should().Be(30);
    }

    #endregion

    #region BVT-020: Quote Entity

    /// <summary>
    /// BVT-020: Verify Quote entity for proposals
    /// 
    /// FUNCTIONAL: Quotes must link to opportunities and track amounts
    /// TECHNICAL: Tests Quote entity with line items concept
    /// </summary>
    [Fact]
    public void BVT020_Quote_CanBeCreatedWithRequiredFields()
    {
        // Arrange & Act
        var quote = new Quote
        {
            Id = 1,
            QuoteNumber = "QT-001",
            Name = "Enterprise Package Quote",
            OpportunityId = 5,
            CustomerId = 10,
            Status = QuoteStatus.Draft,
            Total = 15000m,
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        quote.Id.Should().Be(1);
        quote.Name.Should().Be("Enterprise Package Quote");
        quote.OpportunityId.Should().Be(5);
        quote.Total.Should().Be(15000m);
        quote.Status.Should().Be(QuoteStatus.Draft);
    }

    #endregion
}
