// ============================================================================
// CRM Solution - Service Request Entity Tests
// Tests for ServiceRequest entity and related ITSM classes
// ============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Comprehensive tests for ServiceRequest entity, related classes, and ITSM enums.
/// Covers: 4 enums (ServiceRequestChannel, ServiceRequestStatus, ServiceRequestPriority, CustomFieldType),
/// 5 supporting entities (Category, Subcategory, Type, CustomFieldDefinition, CustomFieldValue),
/// and the main ServiceRequest entity with computed properties.
/// </summary>
public class ServiceRequestEntityTests
{
    #region ServiceRequestChannel Enum Tests

    [Fact]
    public void ServiceRequestChannel_ShouldHaveCorrectNumberOfValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ServiceRequestChannel>();

        // Assert - 8 channel types for multichannel support
        values.Should().HaveCount(8);
    }

    [Theory]
    [InlineData(ServiceRequestChannel.WhatsApp, 0)]
    [InlineData(ServiceRequestChannel.Email, 1)]
    [InlineData(ServiceRequestChannel.Phone, 2)]
    [InlineData(ServiceRequestChannel.InPerson, 3)]
    [InlineData(ServiceRequestChannel.SelfServicePortal, 4)]
    [InlineData(ServiceRequestChannel.SocialMedia, 5)]
    [InlineData(ServiceRequestChannel.LiveChat, 6)]
    [InlineData(ServiceRequestChannel.API, 7)]
    public void ServiceRequestChannel_ShouldHaveCorrectValue(ServiceRequestChannel channel, int expectedValue)
    {
        // Assert
        ((int)channel).Should().Be(expectedValue);
    }

    [Fact]
    public void ServiceRequestChannel_WhatsApp_ShouldBeDefaultZero()
    {
        // WhatsApp = 0, typically most common in modern support
        ServiceRequestChannel.WhatsApp.Should().Be(default(ServiceRequestChannel));
    }

    [Fact]
    public void ServiceRequestChannel_ShouldCoverAllDigitalChannels()
    {
        // Verify all digital communication channels are covered
        var channels = Enum.GetNames<ServiceRequestChannel>();
        
        channels.Should().Contain("WhatsApp");
        channels.Should().Contain("Email");
        channels.Should().Contain("LiveChat");
        channels.Should().Contain("SocialMedia");
        channels.Should().Contain("API");
    }

    [Fact]
    public void ServiceRequestChannel_ShouldCoverAllTraditionalChannels()
    {
        // Verify traditional support channels
        var channels = Enum.GetNames<ServiceRequestChannel>();
        
        channels.Should().Contain("Phone");
        channels.Should().Contain("InPerson");
        channels.Should().Contain("SelfServicePortal");
    }

    #endregion

    #region ServiceRequestStatus Enum Tests

    [Fact]
    public void ServiceRequestStatus_ShouldHaveCorrectNumberOfValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ServiceRequestStatus>();

        // Assert - 11 status values for complete lifecycle
        values.Should().HaveCount(11);
    }

    [Theory]
    [InlineData(ServiceRequestStatus.New, 0)]
    [InlineData(ServiceRequestStatus.Open, 1)]
    [InlineData(ServiceRequestStatus.InProgress, 2)]
    [InlineData(ServiceRequestStatus.PendingCustomer, 3)]
    [InlineData(ServiceRequestStatus.PendingInternal, 4)]
    [InlineData(ServiceRequestStatus.Escalated, 5)]
    [InlineData(ServiceRequestStatus.Resolved, 6)]
    [InlineData(ServiceRequestStatus.Closed, 7)]
    [InlineData(ServiceRequestStatus.Cancelled, 8)]
    [InlineData(ServiceRequestStatus.OnHold, 9)]
    [InlineData(ServiceRequestStatus.Reopened, 10)]
    public void ServiceRequestStatus_ShouldHaveCorrectValue(ServiceRequestStatus status, int expectedValue)
    {
        // Assert
        ((int)status).Should().Be(expectedValue);
    }

    [Fact]
    public void ServiceRequestStatus_New_ShouldBeDefaultZero()
    {
        // New tickets start at status 0
        ServiceRequestStatus.New.Should().Be(default(ServiceRequestStatus));
    }

    [Fact]
    public void ServiceRequestStatus_ShouldHaveActiveStatuses()
    {
        // Verify active/working statuses exist
        var statuses = Enum.GetNames<ServiceRequestStatus>();
        
        statuses.Should().Contain("New");
        statuses.Should().Contain("Open");
        statuses.Should().Contain("InProgress");
    }

    [Fact]
    public void ServiceRequestStatus_ShouldHaveWaitingStatuses()
    {
        // Verify waiting/pending statuses exist
        var statuses = Enum.GetNames<ServiceRequestStatus>();
        
        statuses.Should().Contain("PendingCustomer");
        statuses.Should().Contain("PendingInternal");
        statuses.Should().Contain("OnHold");
    }

    [Fact]
    public void ServiceRequestStatus_ShouldHaveClosedStatuses()
    {
        // Verify terminal statuses exist
        var statuses = Enum.GetNames<ServiceRequestStatus>();
        
        statuses.Should().Contain("Resolved");
        statuses.Should().Contain("Closed");
        statuses.Should().Contain("Cancelled");
    }

    [Fact]
    public void ServiceRequestStatus_ShouldHaveSpecialStatuses()
    {
        // Verify escalation and reopen statuses
        var statuses = Enum.GetNames<ServiceRequestStatus>();
        
        statuses.Should().Contain("Escalated");
        statuses.Should().Contain("Reopened");
    }

    #endregion

    #region ServiceRequestPriority Enum Tests

    [Fact]
    public void ServiceRequestPriority_ShouldHaveCorrectNumberOfValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<ServiceRequestPriority>();

        // Assert - 5 priority levels
        values.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(ServiceRequestPriority.Low, 0)]
    [InlineData(ServiceRequestPriority.Medium, 1)]
    [InlineData(ServiceRequestPriority.High, 2)]
    [InlineData(ServiceRequestPriority.Critical, 3)]
    [InlineData(ServiceRequestPriority.Urgent, 4)]
    public void ServiceRequestPriority_ShouldHaveCorrectValue(ServiceRequestPriority priority, int expectedValue)
    {
        // Assert
        ((int)priority).Should().Be(expectedValue);
    }

    [Fact]
    public void ServiceRequestPriority_Low_ShouldBeLowestValue()
    {
        // Low = 0, lowest priority
        ((int)ServiceRequestPriority.Low).Should().Be(0);
    }

    [Fact]
    public void ServiceRequestPriority_Urgent_ShouldBeHighestValue()
    {
        // Urgent = 4, highest priority
        ((int)ServiceRequestPriority.Urgent).Should().Be(4);
    }

    [Fact]
    public void ServiceRequestPriority_ShouldBeOrderedByUrgency()
    {
        // Verify numeric order matches urgency
        ((int)ServiceRequestPriority.Low).Should().BeLessThan((int)ServiceRequestPriority.Medium);
        ((int)ServiceRequestPriority.Medium).Should().BeLessThan((int)ServiceRequestPriority.High);
        ((int)ServiceRequestPriority.High).Should().BeLessThan((int)ServiceRequestPriority.Critical);
        ((int)ServiceRequestPriority.Critical).Should().BeLessThan((int)ServiceRequestPriority.Urgent);
    }

    [Fact]
    public void ServiceRequestPriority_ShouldDistinguishCriticalFromUrgent()
    {
        // Critical and Urgent are separate - Urgent is even more severe
        ServiceRequestPriority.Critical.Should().NotBe(ServiceRequestPriority.Urgent);
        ((int)ServiceRequestPriority.Urgent).Should().BeGreaterThan((int)ServiceRequestPriority.Critical);
    }

    #endregion

    #region CustomFieldType Enum Tests

    [Fact]
    public void CustomFieldType_ShouldHaveCorrectNumberOfValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<CustomFieldType>();

        // Assert - 12 field types for flexible custom fields
        values.Should().HaveCount(12);
    }

    [Theory]
    [InlineData(CustomFieldType.Text, 0)]
    [InlineData(CustomFieldType.TextArea, 1)]
    [InlineData(CustomFieldType.Number, 2)]
    [InlineData(CustomFieldType.Decimal, 3)]
    [InlineData(CustomFieldType.Date, 4)]
    [InlineData(CustomFieldType.DateTime, 5)]
    [InlineData(CustomFieldType.Dropdown, 6)]
    [InlineData(CustomFieldType.MultiSelect, 7)]
    [InlineData(CustomFieldType.Boolean, 8)]
    [InlineData(CustomFieldType.Email, 9)]
    [InlineData(CustomFieldType.Phone, 10)]
    [InlineData(CustomFieldType.Url, 11)]
    public void CustomFieldType_ShouldHaveCorrectValue(CustomFieldType fieldType, int expectedValue)
    {
        // Assert
        ((int)fieldType).Should().Be(expectedValue);
    }

    [Fact]
    public void CustomFieldType_Text_ShouldBeDefault()
    {
        // Text = 0, most common field type
        CustomFieldType.Text.Should().Be(default(CustomFieldType));
    }

    [Fact]
    public void CustomFieldType_ShouldHaveBasicTextTypes()
    {
        // Verify text-based field types
        var types = Enum.GetNames<CustomFieldType>();
        
        types.Should().Contain("Text");
        types.Should().Contain("TextArea");
    }

    [Fact]
    public void CustomFieldType_ShouldHaveNumericTypes()
    {
        // Verify numeric field types
        var types = Enum.GetNames<CustomFieldType>();
        
        types.Should().Contain("Number");
        types.Should().Contain("Decimal");
    }

    [Fact]
    public void CustomFieldType_ShouldHaveDateTimeTypes()
    {
        // Verify date/time field types
        var types = Enum.GetNames<CustomFieldType>();
        
        types.Should().Contain("Date");
        types.Should().Contain("DateTime");
    }

    [Fact]
    public void CustomFieldType_ShouldHaveSelectionTypes()
    {
        // Verify selection field types
        var types = Enum.GetNames<CustomFieldType>();
        
        types.Should().Contain("Boolean");
        types.Should().Contain("Dropdown");
        types.Should().Contain("MultiSelect");
    }

    [Fact]
    public void CustomFieldType_ShouldHaveFormattedTypes()
    {
        // Verify formatted input field types
        var types = Enum.GetNames<CustomFieldType>();
        
        types.Should().Contain("Email");
        types.Should().Contain("Phone");
        types.Should().Contain("Url");
    }

    #endregion

    #region ServiceRequestCategory Entity Tests

    [Fact]
    public void ServiceRequestCategory_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var category = new ServiceRequestCategory();

        // Assert
        category.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void ServiceRequestCategory_ShouldHaveRequiredProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert - verify key properties exist
        propertyNames.Should().Contain("Name");
        propertyNames.Should().Contain("Description");
        propertyNames.Should().Contain("DisplayOrder");
        propertyNames.Should().Contain("IsActive");
    }

    [Fact]
    public void ServiceRequestCategory_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var category = new ServiceRequestCategory();

        // Assert
        category.Name.Should().BeEmpty();
        category.IsActive.Should().BeTrue();
        category.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void ServiceRequestCategory_ShouldHaveSLAProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert - SLA-related properties
        propertyNames.Should().Contain("DefaultResponseTimeHours");
        propertyNames.Should().Contain("DefaultResolutionTimeHours");
    }

    [Fact]
    public void ServiceRequestCategory_ShouldHaveUIProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert - UI customization properties
        propertyNames.Should().Contain("IconName");
        propertyNames.Should().Contain("ColorCode");
    }

    [Fact]
    public void ServiceRequestCategory_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var category = new ServiceRequestCategory
        {
            Name = "Technical Support",
            Description = "Technical issues and troubleshooting",
            DisplayOrder = 1,
            IsActive = true,
            IconName = "fa-wrench",
            ColorCode = "#3498db",
            DefaultResponseTimeHours = 4,
            DefaultResolutionTimeHours = 24
        };

        // Assert
        category.Name.Should().Be("Technical Support");
        category.Description.Should().Be("Technical issues and troubleshooting");
        category.DisplayOrder.Should().Be(1);
        category.IsActive.Should().BeTrue();
        category.IconName.Should().Be("fa-wrench");
        category.ColorCode.Should().Be("#3498db");
        category.DefaultResponseTimeHours.Should().Be(4);
        category.DefaultResolutionTimeHours.Should().Be(24);
    }

    [Fact]
    public void ServiceRequestCategory_ShouldHaveSubcategoriesCollection()
    {
        // Arrange
        var category = new ServiceRequestCategory();

        // Assert - navigation property for child subcategories
        category.Subcategories.Should().NotBeNull();
        category.Subcategories.Should().BeEmpty();
    }

    #endregion

    #region ServiceRequestSubcategory Entity Tests

    [Fact]
    public void ServiceRequestSubcategory_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var subcategory = new ServiceRequestSubcategory();

        // Assert
        subcategory.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void ServiceRequestSubcategory_ShouldHaveRequiredProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestSubcategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("Name");
        propertyNames.Should().Contain("Description");
        propertyNames.Should().Contain("CategoryId");
        propertyNames.Should().Contain("DisplayOrder");
        propertyNames.Should().Contain("IsActive");
    }

    [Fact]
    public void ServiceRequestSubcategory_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var subcategory = new ServiceRequestSubcategory();

        // Assert
        subcategory.Name.Should().BeEmpty();
        subcategory.IsActive.Should().BeTrue();
        subcategory.DisplayOrder.Should().Be(0);
        subcategory.CategoryId.Should().Be(0);
    }

    [Fact]
    public void ServiceRequestSubcategory_ShouldHaveOwnSLAProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestSubcategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert - subcategory can override parent SLA
        propertyNames.Should().Contain("ResponseTimeHours");
        propertyNames.Should().Contain("ResolutionTimeHours");
    }

    [Fact]
    public void ServiceRequestSubcategory_ShouldHaveDefaultPriority()
    {
        // Arrange
        var properties = typeof(ServiceRequestSubcategory).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("DefaultPriority");
    }

    [Fact]
    public void ServiceRequestSubcategory_ShouldHaveCategoryNavigation()
    {
        // Arrange
        var subcategory = new ServiceRequestSubcategory();

        // Assert - parent category navigation
        subcategory.Category.Should().BeNull(); // Not loaded by default
    }

    [Fact]
    public void ServiceRequestSubcategory_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var subcategory = new ServiceRequestSubcategory
        {
            Name = "Software Installation",
            Description = "Requests for software installation",
            CategoryId = 1,
            DisplayOrder = 1,
            IsActive = true,
            ResponseTimeHours = 2,
            ResolutionTimeHours = 8,
            DefaultPriority = ServiceRequestPriority.Medium
        };

        // Assert
        subcategory.Name.Should().Be("Software Installation");
        subcategory.CategoryId.Should().Be(1);
        subcategory.ResponseTimeHours.Should().Be(2);
        subcategory.ResolutionTimeHours.Should().Be(8);
        subcategory.DefaultPriority.Should().Be(ServiceRequestPriority.Medium);
    }

    #endregion

    #region ServiceRequestType Entity Tests

    [Fact]
    public void ServiceRequestType_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var type = new ServiceRequestType();

        // Assert
        type.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void ServiceRequestType_ShouldHaveRequiredProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestType).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("Name");
        propertyNames.Should().Contain("RequestType");
        propertyNames.Should().Contain("DetailedDescription");
        propertyNames.Should().Contain("CategoryId");
        propertyNames.Should().Contain("SubcategoryId");
    }

    [Fact]
    public void ServiceRequestType_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var type = new ServiceRequestType();

        // Assert
        type.Name.Should().BeEmpty();
        type.IsActive.Should().BeTrue();
        type.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void ServiceRequestType_ShouldHaveWorkflowSupport()
    {
        // Arrange
        var properties = typeof(ServiceRequestType).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("WorkflowName");
    }

    [Fact]
    public void ServiceRequestType_ShouldHaveResolutionOptions()
    {
        // Arrange
        var properties = typeof(ServiceRequestType).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("PossibleResolutions");
        propertyNames.Should().Contain("FinalCustomerResolutions");
    }

    [Fact]
    public void ServiceRequestType_ShouldAllowSettingProperties()
    {
        // Arrange & Act
        var type = new ServiceRequestType
        {
            Name = "Password Reset",
            RequestType = "Access Issue",
            DetailedDescription = "User cannot log in and needs password reset",
            CategoryId = 1,
            SubcategoryId = 2,
            WorkflowName = "password_reset_workflow",
            PossibleResolutions = "Reset via email,Reset via phone,Manual reset",
            FinalCustomerResolutions = "Password reset completed,Account unlocked",
            DefaultPriority = ServiceRequestPriority.High,
            Tags = "password,access,login",
            DisplayOrder = 1,
            IsActive = true
        };

        // Assert
        type.Name.Should().Be("Password Reset");
        type.RequestType.Should().Be("Access Issue");
        type.WorkflowName.Should().Be("password_reset_workflow");
        type.PossibleResolutions.Should().Contain("Reset via email");
        type.DefaultPriority.Should().Be(ServiceRequestPriority.High);
        type.Tags.Should().Contain("password");
    }

    #endregion

    #region ServiceRequestCustomFieldDefinition Entity Tests

    [Fact]
    public void CustomFieldDefinition_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var field = new ServiceRequestCustomFieldDefinition();

        // Assert
        field.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void CustomFieldDefinition_ShouldHaveBasicProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldDefinition).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("Name");
        propertyNames.Should().Contain("FieldKey");
        propertyNames.Should().Contain("Description");
        propertyNames.Should().Contain("FieldType");
    }

    [Fact]
    public void CustomFieldDefinition_NewInstance_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var field = new ServiceRequestCustomFieldDefinition();

        // Assert
        field.Name.Should().BeEmpty();
        field.FieldKey.Should().BeEmpty();
        field.FieldType.Should().Be(CustomFieldType.Text);
        field.IsRequired.Should().BeFalse();
        field.IsActive.Should().BeTrue();
        field.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void CustomFieldDefinition_ShouldHaveUIHelperProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldDefinition).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("DefaultValue");
        propertyNames.Should().Contain("Placeholder");
        propertyNames.Should().Contain("HelpText");
    }

    [Fact]
    public void CustomFieldDefinition_ShouldHaveValidationProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldDefinition).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("MinValue");
        propertyNames.Should().Contain("MaxValue");
        propertyNames.Should().Contain("ValidationPattern");
        propertyNames.Should().Contain("ValidationMessage");
    }

    [Fact]
    public void CustomFieldDefinition_ShouldHaveDropdownOptions()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldDefinition).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("DropdownOptions");
    }

    [Fact]
    public void CustomFieldDefinition_ShouldHaveCategoryAssociation()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldDefinition).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("CategoryId");
        propertyNames.Should().Contain("SubcategoryId");
    }

    [Fact]
    public void CustomFieldDefinition_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var field = new ServiceRequestCustomFieldDefinition
        {
            Name = "Asset Tag",
            FieldKey = "asset_tag",
            Description = "Hardware asset identification number",
            FieldType = CustomFieldType.Text,
            IsRequired = true,
            IsActive = true,
            DisplayOrder = 1,
            DefaultValue = "AT-",
            Placeholder = "Enter asset tag (e.g., AT-12345)",
            HelpText = "Find this on the asset label",
            ValidationPattern = @"^AT-\d{5}$",
            ValidationMessage = "Asset tag must be in format AT-XXXXX",
            CategoryId = 1
        };

        // Assert
        field.Name.Should().Be("Asset Tag");
        field.FieldKey.Should().Be("asset_tag");
        field.FieldType.Should().Be(CustomFieldType.Text);
        field.IsRequired.Should().BeTrue();
        field.ValidationPattern.Should().Be(@"^AT-\d{5}$");
    }

    [Fact]
    public void CustomFieldDefinition_DropdownField_ShouldHaveOptions()
    {
        // Arrange & Act
        var field = new ServiceRequestCustomFieldDefinition
        {
            Name = "Impact Level",
            FieldKey = "impact_level",
            FieldType = CustomFieldType.Dropdown,
            DropdownOptions = "Low|Medium|High|Critical",
            IsRequired = true
        };

        // Assert
        field.FieldType.Should().Be(CustomFieldType.Dropdown);
        field.DropdownOptions.Should().Contain("Low");
        field.DropdownOptions.Should().Contain("Critical");
    }

    [Fact]
    public void CustomFieldDefinition_NumericField_ShouldHaveMinMax()
    {
        // Arrange & Act
        var field = new ServiceRequestCustomFieldDefinition
        {
            Name = "Number of Users Affected",
            FieldKey = "users_affected",
            FieldType = CustomFieldType.Number,
            MinValue = 1,
            MaxValue = 10000
        };

        // Assert
        field.FieldType.Should().Be(CustomFieldType.Number);
        field.MinValue.Should().Be(1);
        field.MaxValue.Should().Be(10000);
    }

    #endregion

    #region ServiceRequestCustomFieldValue Entity Tests

    [Fact]
    public void CustomFieldValue_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var value = new ServiceRequestCustomFieldValue();

        // Assert
        value.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void CustomFieldValue_ShouldHaveRequiredProperties()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldValue).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert
        propertyNames.Should().Contain("ServiceRequestId");
        propertyNames.Should().Contain("CustomFieldDefinitionId");
    }

    [Fact]
    public void CustomFieldValue_ShouldHaveMultipleValueTypes()
    {
        // Arrange
        var properties = typeof(ServiceRequestCustomFieldValue).GetProperties();
        var propertyNames = properties.Select(p => p.Name).ToList();

        // Assert - different value storage columns for different types
        propertyNames.Should().Contain("TextValue");
        propertyNames.Should().Contain("NumericValue");
        propertyNames.Should().Contain("DateValue");
        propertyNames.Should().Contain("BooleanValue");
    }

    [Fact]
    public void CustomFieldValue_NewInstance_ShouldHaveNullValues()
    {
        // Arrange & Act
        var value = new ServiceRequestCustomFieldValue();

        // Assert
        value.TextValue.Should().BeNull();
        value.NumericValue.Should().BeNull();
        value.DateValue.Should().BeNull();
        value.BooleanValue.Should().BeNull();
    }

    [Fact]
    public void CustomFieldValue_TextValue_ShouldStore()
    {
        // Arrange & Act
        var value = new ServiceRequestCustomFieldValue
        {
            ServiceRequestId = 1,
            CustomFieldDefinitionId = 1,
            TextValue = "AT-12345"
        };

        // Assert
        value.TextValue.Should().Be("AT-12345");
    }

    [Fact]
    public void CustomFieldValue_NumericValue_ShouldStore()
    {
        // Arrange & Act
        var value = new ServiceRequestCustomFieldValue
        {
            ServiceRequestId = 1,
            CustomFieldDefinitionId = 2,
            NumericValue = 42.5m
        };

        // Assert
        value.NumericValue.Should().Be(42.5m);
    }

    [Fact]
    public void CustomFieldValue_DateValue_ShouldStore()
    {
        // Arrange
        var expectedDate = new DateTime(2024, 6, 15);
        
        // Act
        var value = new ServiceRequestCustomFieldValue
        {
            ServiceRequestId = 1,
            CustomFieldDefinitionId = 3,
            DateValue = expectedDate
        };

        // Assert
        value.DateValue.Should().Be(expectedDate);
    }

    [Fact]
    public void CustomFieldValue_BooleanValue_ShouldStore()
    {
        // Arrange & Act
        var value = new ServiceRequestCustomFieldValue
        {
            ServiceRequestId = 1,
            CustomFieldDefinitionId = 4,
            BooleanValue = true
        };

        // Assert
        value.BooleanValue.Should().BeTrue();
    }

    [Fact]
    public void CustomFieldValue_ShouldHaveNavigationProperties()
    {
        // Arrange
        var value = new ServiceRequestCustomFieldValue();

        // Assert
        value.ServiceRequest.Should().BeNull();
        value.CustomFieldDefinition.Should().BeNull();
    }

    #endregion

    #region ServiceRequest Entity - Default Values Tests

    [Fact]
    public void ServiceRequest_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultTicketNumber()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.TicketNumber.Should().BeEmpty();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultSubject()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.Subject.Should().BeEmpty();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultChannel()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.SelfServicePortal);
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.New);
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultPriority()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.Priority.Should().Be(ServiceRequestPriority.Medium);
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultSlaFlags()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.ResponseSlaBreached.Should().BeFalse();
        request.ResolutionSlaBreached.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultExpediteFlags()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.IsExpedited.Should().BeFalse();
        request.ExpediteReason.Should().BeNull();
        request.ExpeditedByUserId.Should().BeNull();
        request.ExpeditedAt.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultCounters()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.EscalationLevel.Should().Be(0);
        request.ReopenCount.Should().Be(0);
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveDefaultVipFlag()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.IsVipCustomer.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveNullOptionalReferences()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.CategoryId.Should().BeNull();
        request.SubcategoryId.Should().BeNull();
        request.AccountId.Should().BeNull();
        request.ContactId.Should().BeNull();
        request.AssignedToUserId.Should().BeNull();
        request.AssignedToGroupId.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveNullDates()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.ResponseDueDate.Should().BeNull();
        request.ResolutionDueDate.Should().BeNull();
        request.FirstResponseDate.Should().BeNull();
        request.ResolvedDate.Should().BeNull();
        request.ClosedDate.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_NewInstance_ShouldHaveEmptyCollections()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.ChildServiceRequests.Should().NotBeNull();
        request.ChildServiceRequests.Should().BeEmpty();
        request.CustomFieldValues.Should().NotBeNull();
        request.CustomFieldValues.Should().BeEmpty();
        request.Notes.Should().NotBeNull();
        request.Notes.Should().BeEmpty();
        request.Activities.Should().NotBeNull();
        request.Activities.Should().BeEmpty();
    }

    #endregion

    #region ServiceRequest Entity - Property Assignment Tests

    [Fact]
    public void ServiceRequest_ShouldAllowSettingBasicInfo()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            TicketNumber = "SR-2024-001234",
            Subject = "Cannot access email",
            Description = "User reports unable to access company email since this morning"
        };

        // Assert
        request.TicketNumber.Should().Be("SR-2024-001234");
        request.Subject.Should().Be("Cannot access email");
        request.Description.Should().Contain("unable to access");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingChannel()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.Phone,
            SourcePhoneNumber = "+1-555-123-4567"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.Phone);
        request.SourcePhoneNumber.Should().Be("+1-555-123-4567");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingEmailChannel()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.Email,
            SourceEmailAddress = "user@company.com",
            ExternalReferenceId = "MSG-12345"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.Email);
        request.SourceEmailAddress.Should().Be("user@company.com");
        request.ExternalReferenceId.Should().Be("MSG-12345");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingRequesterInfo()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            RequesterName = "John Doe",
            RequesterEmail = "john.doe@example.com",
            RequesterPhone = "+1-555-987-6543"
        };

        // Assert
        request.RequesterName.Should().Be("John Doe");
        request.RequesterEmail.Should().Be("john.doe@example.com");
        request.RequesterPhone.Should().Be("+1-555-987-6543");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingAssignment()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            AssignedToUserId = 5,
            AssignedToGroupId = 2,
            CreatedByUserId = 1
        };

        // Assert
        request.AssignedToUserId.Should().Be(5);
        request.AssignedToGroupId.Should().Be(2);
        request.CreatedByUserId.Should().Be(1);
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingSLADates()
    {
        // Arrange
        var now = DateTime.UtcNow;
        
        // Act
        var request = new ServiceRequest
        {
            ResponseDueDate = now.AddHours(4),
            ResolutionDueDate = now.AddHours(24)
        };

        // Assert
        request.ResponseDueDate.Should().BeCloseTo(now.AddHours(4), TimeSpan.FromSeconds(1));
        request.ResolutionDueDate.Should().BeCloseTo(now.AddHours(24), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingResolution()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.Resolved,
            ResolvedDate = DateTime.UtcNow,
            ResolutionSummary = "Reset password and verified access",
            ResolutionCode = "PASSWORD_RESET",
            RootCause = "Password expired"
        };

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Resolved);
        request.ResolutionSummary.Should().Contain("Reset password");
        request.ResolutionCode.Should().Be("PASSWORD_RESET");
        request.RootCause.Should().Be("Password expired");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingFeedback()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            SatisfactionRating = 5,
            CustomerFeedback = "Great support, very quick resolution!"
        };

        // Assert
        request.SatisfactionRating.Should().Be(5);
        request.CustomerFeedback.Should().Contain("Great support");
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingExpedite()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            IsExpedited = true,
            ExpediteReason = "VIP customer - CEO",
            ExpeditedByUserId = 1,
            ExpeditedAt = DateTime.UtcNow
        };

        // Assert
        request.IsExpedited.Should().BeTrue();
        request.ExpediteReason.Should().Contain("VIP");
        request.ExpeditedByUserId.Should().Be(1);
        request.ExpeditedAt.Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingEscalation()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.Escalated,
            EscalationLevel = 2,
            Priority = ServiceRequestPriority.Critical
        };

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Escalated);
        request.EscalationLevel.Should().Be(2);
        request.Priority.Should().Be(ServiceRequestPriority.Critical);
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingEffortTracking()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            EstimatedEffortHours = 4.0m,
            ActualEffortHours = 3.5m
        };

        // Assert
        request.EstimatedEffortHours.Should().Be(4.0m);
        request.ActualEffortHours.Should().Be(3.5m);
    }

    [Fact]
    public void ServiceRequest_ShouldAllowSettingRelatedEntities()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            RelatedOpportunityId = 10,
            RelatedProductId = 5,
            ParentServiceRequestId = 100,
            SourceInteractionId = 50
        };

        // Assert
        request.RelatedOpportunityId.Should().Be(10);
        request.RelatedProductId.Should().Be(5);
        request.ParentServiceRequestId.Should().Be(100);
        request.SourceInteractionId.Should().Be(50);
    }

    #endregion

    #region ServiceRequest Entity - Computed Properties Tests

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeTrueForNewStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.New };

        // Assert
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeTrueForOpenStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Open };

        // Assert
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeTrueForInProgressStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.InProgress };

        // Assert
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeTrueForEscalatedStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Escalated };

        // Assert
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeTrueForReopenedStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Reopened };

        // Assert
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeFalseForClosedStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Closed };

        // Assert
        request.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeFalseForResolvedStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Resolved };

        // Assert
        request.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsOpen_ShouldBeFalseForCancelledStatus()
    {
        // Arrange & Act
        var request = new ServiceRequest { Status = ServiceRequestStatus.Cancelled };

        // Assert
        request.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_AgeInHours_ShouldBePositive()
    {
        // Arrange
        var request = new ServiceRequest();
        // BaseEntity sets CreatedAt in constructor
        
        // Act
        var age = request.AgeInHours;

        // Assert
        age.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void ServiceRequest_TimeToFirstResponseHours_ShouldBeNullWhenNotResponded()
    {
        // Arrange
        var request = new ServiceRequest
        {
            FirstResponseDate = null
        };

        // Assert
        request.TimeToFirstResponseHours.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_TimeToFirstResponseHours_ShouldCalculateWhenResponded()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-5);
        var respondedAt = DateTime.UtcNow.AddHours(-3);
        
        var request = new ServiceRequest
        {
            FirstResponseDate = respondedAt
        };
        // Manually set CreatedAt for test (normally done by EF)
        typeof(BaseEntity).GetProperty("CreatedAt")!.SetValue(request, createdAt);

        // Act
        var responseTime = request.TimeToFirstResponseHours;

        // Assert
        responseTime.Should().NotBeNull();
        responseTime.Should().BeApproximately(2.0, 0.1);
    }

    [Fact]
    public void ServiceRequest_TimeToResolutionHours_ShouldBeNullWhenNotResolved()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResolvedDate = null
        };

        // Assert
        request.TimeToResolutionHours.Should().BeNull();
    }

    [Fact]
    public void ServiceRequest_TimeToResolutionHours_ShouldCalculateWhenResolved()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-10);
        var resolvedAt = DateTime.UtcNow.AddHours(-2);
        
        var request = new ServiceRequest
        {
            ResolvedDate = resolvedAt
        };
        typeof(BaseEntity).GetProperty("CreatedAt")!.SetValue(request, createdAt);

        // Act
        var resolutionTime = request.TimeToResolutionHours;

        // Assert
        resolutionTime.Should().NotBeNull();
        resolutionTime.Should().BeApproximately(8.0, 0.1);
    }

    [Fact]
    public void ServiceRequest_IsResponseSlaAtRisk_ShouldBeFalseWhenAlreadyResponded()
    {
        // Arrange
        var request = new ServiceRequest
        {
            FirstResponseDate = DateTime.UtcNow.AddHours(-1),
            ResponseDueDate = DateTime.UtcNow.AddHours(1)
        };

        // Assert
        request.IsResponseSlaAtRisk.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsResponseSlaAtRisk_ShouldBeFalseWhenNoSla()
    {
        // Arrange
        var request = new ServiceRequest
        {
            FirstResponseDate = null,
            ResponseDueDate = null
        };

        // Assert
        request.IsResponseSlaAtRisk.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsResponseSlaAtRisk_ShouldBeTrueWhenDueSoon()
    {
        // Arrange - due in 1 hour (within 2-hour risk window)
        var request = new ServiceRequest
        {
            FirstResponseDate = null,
            ResponseDueDate = DateTime.UtcNow.AddHours(1)
        };

        // Assert
        request.IsResponseSlaAtRisk.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsResponseSlaAtRisk_ShouldBeFalseWhenPlentyOfTime()
    {
        // Arrange - due in 5 hours (outside 2-hour risk window)
        var request = new ServiceRequest
        {
            FirstResponseDate = null,
            ResponseDueDate = DateTime.UtcNow.AddHours(5)
        };

        // Assert
        request.IsResponseSlaAtRisk.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsResolutionSlaAtRisk_ShouldBeFalseWhenAlreadyResolved()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResolvedDate = DateTime.UtcNow.AddHours(-1),
            ResolutionDueDate = DateTime.UtcNow.AddHours(1)
        };

        // Assert
        request.IsResolutionSlaAtRisk.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsResolutionSlaAtRisk_ShouldBeFalseWhenNoSla()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResolvedDate = null,
            ResolutionDueDate = null
        };

        // Assert
        request.IsResolutionSlaAtRisk.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_IsResolutionSlaAtRisk_ShouldBeTrueWhenDueSoon()
    {
        // Arrange - due in 2 hours (within 4-hour risk window)
        var request = new ServiceRequest
        {
            ResolvedDate = null,
            ResolutionDueDate = DateTime.UtcNow.AddHours(2)
        };

        // Assert
        request.IsResolutionSlaAtRisk.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_IsResolutionSlaAtRisk_ShouldBeFalseWhenPlentyOfTime()
    {
        // Arrange - due in 10 hours (outside 4-hour risk window)
        var request = new ServiceRequest
        {
            ResolvedDate = null,
            ResolutionDueDate = DateTime.UtcNow.AddHours(10)
        };

        // Assert
        request.IsResolutionSlaAtRisk.Should().BeFalse();
    }

    #endregion

    #region ServiceRequest Entity - Status Workflow Tests

    [Fact]
    public void ServiceRequest_StatusWorkflow_NewToInProgress()
    {
        // Arrange
        var request = new ServiceRequest
        {
            TicketNumber = "SR-001",
            Subject = "Test Issue",
            Status = ServiceRequestStatus.New
        };

        // Act - Simulate assignment and work starting
        request.AssignedToUserId = 1;
        request.Status = ServiceRequestStatus.InProgress;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.InProgress);
        request.AssignedToUserId.Should().Be(1);
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_InProgressToResolved()
    {
        // Arrange
        var request = new ServiceRequest
        {
            TicketNumber = "SR-001",
            Status = ServiceRequestStatus.InProgress
        };

        // Act - Simulate resolution
        request.Status = ServiceRequestStatus.Resolved;
        request.ResolvedDate = DateTime.UtcNow;
        request.ResolutionSummary = "Issue fixed";

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Resolved);
        request.ResolvedDate.Should().NotBeNull();
        request.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_ResolvedToClosed()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.Resolved,
            ResolvedDate = DateTime.UtcNow.AddHours(-24)
        };

        // Act - Customer confirms resolution
        request.Status = ServiceRequestStatus.Closed;
        request.ClosedDate = DateTime.UtcNow;
        request.SatisfactionRating = 5;
        request.CustomerFeedback = "Great job!";

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Closed);
        request.ClosedDate.Should().NotBeNull();
        request.SatisfactionRating.Should().Be(5);
        request.IsOpen.Should().BeFalse();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_ResolvedToReopened()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.Resolved,
            ResolvedDate = DateTime.UtcNow.AddHours(-2),
            ReopenCount = 0
        };

        // Act - Customer reports issue not fixed
        request.Status = ServiceRequestStatus.Reopened;
        request.ReopenCount++;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Reopened);
        request.ReopenCount.Should().Be(1);
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_Escalation()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.InProgress,
            Priority = ServiceRequestPriority.Medium,
            EscalationLevel = 0
        };

        // Act - Escalate due to complexity
        request.Status = ServiceRequestStatus.Escalated;
        request.Priority = ServiceRequestPriority.High;
        request.EscalationLevel = 1;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Escalated);
        request.Priority.Should().Be(ServiceRequestPriority.High);
        request.EscalationLevel.Should().Be(1);
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_PendingCustomer()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.InProgress
        };

        // Act - Need more info from customer
        request.Status = ServiceRequestStatus.PendingCustomer;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.PendingCustomer);
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_PendingInternal()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.InProgress
        };

        // Act - Waiting for vendor support
        request.Status = ServiceRequestStatus.PendingInternal;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.PendingInternal);
        request.IsOpen.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_StatusWorkflow_Cancelled()
    {
        // Arrange
        var request = new ServiceRequest
        {
            Status = ServiceRequestStatus.New
        };

        // Act - Customer cancels request
        request.Status = ServiceRequestStatus.Cancelled;

        // Assert
        request.Status.Should().Be(ServiceRequestStatus.Cancelled);
        request.IsOpen.Should().BeFalse();
    }

    #endregion

    #region ServiceRequest Entity - SLA Breach Tests

    [Fact]
    public void ServiceRequest_SlaBreached_ShouldTrackResponseBreach()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResponseDueDate = DateTime.UtcNow.AddHours(-1), // Past due
            FirstResponseDate = null,
            ResponseSlaBreached = false
        };

        // Act - Simulate SLA check
        if (!request.FirstResponseDate.HasValue &&
            request.ResponseDueDate.HasValue &&
            DateTime.UtcNow > request.ResponseDueDate.Value)
        {
            request.ResponseSlaBreached = true;
        }

        // Assert
        request.ResponseSlaBreached.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_SlaBreached_ShouldTrackResolutionBreach()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResolutionDueDate = DateTime.UtcNow.AddHours(-1), // Past due
            ResolvedDate = null,
            ResolutionSlaBreached = false
        };

        // Act - Simulate SLA check
        if (!request.ResolvedDate.HasValue &&
            request.ResolutionDueDate.HasValue &&
            DateTime.UtcNow > request.ResolutionDueDate.Value)
        {
            request.ResolutionSlaBreached = true;
        }

        // Assert
        request.ResolutionSlaBreached.Should().BeTrue();
    }

    [Fact]
    public void ServiceRequest_SlaBreached_ShouldNotBreachWhenMetInTime()
    {
        // Arrange
        var request = new ServiceRequest
        {
            ResponseDueDate = DateTime.UtcNow.AddHours(4),
            FirstResponseDate = DateTime.UtcNow.AddHours(-1), // Responded within SLA
            ResponseSlaBreached = false
        };

        // Assert - SLA was met
        request.ResponseSlaBreached.Should().BeFalse();
    }

    #endregion

    #region ServiceRequest Entity - Channel-Specific Tests

    [Fact]
    public void ServiceRequest_WhatsAppChannel_ShouldHavePhoneSource()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.WhatsApp,
            SourcePhoneNumber = "+1-555-123-4567",
            ConversationId = "wa-conv-12345"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.WhatsApp);
        request.SourcePhoneNumber.Should().NotBeNullOrEmpty();
        request.ConversationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ServiceRequest_EmailChannel_ShouldHaveEmailSource()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.Email,
            SourceEmailAddress = "customer@example.com",
            ExternalReferenceId = "email-msg-12345"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.Email);
        request.SourceEmailAddress.Should().NotBeNullOrEmpty();
        request.ExternalReferenceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ServiceRequest_LiveChatChannel_ShouldHaveConversationId()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.LiveChat,
            ConversationId = "chat-session-98765"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.LiveChat);
        request.ConversationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ServiceRequest_InPersonChannel_ShouldNotRequireDigitalSource()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.InPerson,
            RequesterName = "John Walk-In",
            SourcePhoneNumber = null,
            SourceEmailAddress = null
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.InPerson);
        request.SourcePhoneNumber.Should().BeNull();
        request.SourceEmailAddress.Should().BeNull();
        request.RequesterName.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ServiceRequest_APIChannel_ShouldHaveExternalReference()
    {
        // Arrange & Act
        var request = new ServiceRequest
        {
            Channel = ServiceRequestChannel.API,
            ExternalReferenceId = "api-req-xyz-123",
            ConversationId = "api-session-456"
        };

        // Assert
        request.Channel.Should().Be(ServiceRequestChannel.API);
        request.ExternalReferenceId.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region ServiceRequest Entity - Parent-Child Relationship Tests

    [Fact]
    public void ServiceRequest_ShouldSupportParentChildRelationship()
    {
        // Arrange
        var parentRequest = new ServiceRequest
        {
            Id = 1,
            TicketNumber = "SR-001",
            Subject = "Main Issue"
        };

        // Act
        var childRequest = new ServiceRequest
        {
            TicketNumber = "SR-001-A",
            Subject = "Sub-task for SR-001",
            ParentServiceRequestId = parentRequest.Id
        };

        // Assert
        childRequest.ParentServiceRequestId.Should().Be(1);
    }

    [Fact]
    public void ServiceRequest_ChildServiceRequests_ShouldBeEmptyByDefault()
    {
        // Arrange & Act
        var request = new ServiceRequest();

        // Assert
        request.ChildServiceRequests.Should().NotBeNull();
        request.ChildServiceRequests.Should().BeEmpty();
    }

    #endregion

    #region ServiceRequest Entity - Data Annotation Tests

    [Fact]
    public void ServiceRequest_TicketNumber_ShouldHaveRequiredAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.TicketNumber));
        var attribute = property!.GetCustomAttribute<RequiredAttribute>();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_TicketNumber_ShouldHaveMaxLengthAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.TicketNumber));
        var attribute = property!.GetCustomAttribute<MaxLengthAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Length.Should().Be(50);
    }

    [Fact]
    public void ServiceRequest_Subject_ShouldHaveRequiredAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.Subject));
        var attribute = property!.GetCustomAttribute<RequiredAttribute>();

        // Assert
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_Subject_ShouldHaveMaxLengthAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.Subject));
        var attribute = property!.GetCustomAttribute<MaxLengthAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Length.Should().Be(500);
    }

    [Fact]
    public void ServiceRequest_Description_ShouldHaveMaxLengthAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.Description));
        var attribute = property!.GetCustomAttribute<MaxLengthAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Length.Should().Be(10000);
    }

    [Fact]
    public void ServiceRequest_ResolutionSummary_ShouldHaveMaxLengthAttribute()
    {
        // Arrange
        var property = typeof(ServiceRequest).GetProperty(nameof(ServiceRequest.ResolutionSummary));
        var attribute = property!.GetCustomAttribute<MaxLengthAttribute>();

        // Assert
        attribute.Should().NotBeNull();
        attribute!.Length.Should().Be(5000);
    }

    #endregion

    #region ServiceRequest Entity - Complete Ticket Scenario Tests

    [Fact]
    public void ServiceRequest_CompleteTicketLifecycle_ShouldWork()
    {
        // Arrange - Create new ticket
        var request = new ServiceRequest
        {
            TicketNumber = "SR-2024-00001",
            Subject = "Cannot access VPN",
            Description = "Getting error when trying to connect to company VPN",
            Channel = ServiceRequestChannel.Email,
            SourceEmailAddress = "employee@company.com",
            Priority = ServiceRequestPriority.High,
            AccountId = 100,
            ContactId = 200,
            CategoryId = 1,
            SubcategoryId = 5
        };

        // Assert initial state
        request.Status.Should().Be(ServiceRequestStatus.New);
        request.IsOpen.Should().BeTrue();

        // Act - Assign to agent
        request.AssignedToUserId = 10;
        request.Status = ServiceRequestStatus.InProgress;
        request.FirstResponseDate = DateTime.UtcNow;

        // Assert after assignment
        request.Status.Should().Be(ServiceRequestStatus.InProgress);
        request.TimeToFirstResponseHours.Should().NotBeNull();

        // Act - Resolve ticket
        request.Status = ServiceRequestStatus.Resolved;
        request.ResolvedDate = DateTime.UtcNow;
        request.ResolutionSummary = "Reset VPN credentials and verified connection";
        request.ResolutionCode = "CREDENTIALS_RESET";

        // Assert after resolution
        request.Status.Should().Be(ServiceRequestStatus.Resolved);
        request.IsOpen.Should().BeFalse();
        request.TimeToResolutionHours.Should().NotBeNull();

        // Act - Close ticket with feedback
        request.Status = ServiceRequestStatus.Closed;
        request.ClosedDate = DateTime.UtcNow;
        request.SatisfactionRating = 4;
        request.CustomerFeedback = "Issue resolved, thank you";

        // Final assertions
        request.Status.Should().Be(ServiceRequestStatus.Closed);
        request.IsOpen.Should().BeFalse();
        request.SatisfactionRating.Should().Be(4);
    }

    [Fact]
    public void ServiceRequest_VIPCustomerScenario_ShouldExpedite()
    {
        // Arrange
        var request = new ServiceRequest
        {
            TicketNumber = "SR-VIP-001",
            Subject = "CEO laptop not working",
            Channel = ServiceRequestChannel.Phone,
            Priority = ServiceRequestPriority.Medium,
            IsVipCustomer = false,
            IsExpedited = false
        };

        // Act - Mark as VIP and expedite
        request.IsVipCustomer = true;
        request.IsExpedited = true;
        request.ExpediteReason = "CEO - critical business meeting";
        request.Priority = ServiceRequestPriority.Urgent;
        request.ExpeditedByUserId = 1;
        request.ExpeditedAt = DateTime.UtcNow;

        // Assert
        request.IsVipCustomer.Should().BeTrue();
        request.IsExpedited.Should().BeTrue();
        request.Priority.Should().Be(ServiceRequestPriority.Urgent);
        request.ExpediteReason.Should().Contain("CEO");
    }

    [Fact]
    public void ServiceRequest_MultipleReopensScenario_ShouldTrackCount()
    {
        // Arrange
        var request = new ServiceRequest
        {
            TicketNumber = "SR-REOPEN-001",
            Status = ServiceRequestStatus.New,
            ReopenCount = 0
        };

        // Act - First resolution and reopen
        request.Status = ServiceRequestStatus.Resolved;
        request.ResolvedDate = DateTime.UtcNow;
        
        // Reopen 1
        request.Status = ServiceRequestStatus.Reopened;
        request.ReopenCount++;
        
        // Resolve again
        request.Status = ServiceRequestStatus.Resolved;
        
        // Reopen 2
        request.Status = ServiceRequestStatus.Reopened;
        request.ReopenCount++;

        // Assert
        request.ReopenCount.Should().Be(2);
        request.Status.Should().Be(ServiceRequestStatus.Reopened);
        request.IsOpen.Should().BeTrue();
    }

    #endregion
}
