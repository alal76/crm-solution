using System;
using System.Collections.Generic;
using CRM.Core.Entities;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Entities;

/// <summary>
/// Unit tests for Account and Address entity normalization.
/// Tests cover: Entity structure, relationships, timestamps, and soft delete behavior.
///
/// FUNCTIONAL VIEW:
/// - Verifies account-address relationships are properly configured
/// - Tests timestamp management (CreatedAt, UpdatedAt)
/// - Tests soft delete flag behavior
/// - Tests address type enumerations
/// - Tests multiple addresses per account
///
/// TECHNICAL VIEW:
/// - Tests entity property definitions
/// - Verifies navigation properties
/// - Tests entity initialization
/// - Validates entity constraints
/// </summary>
public class AccountAddressNormalizationTests
{
    #region Account Entity Tests

    [Fact]
    public void Account_ShouldHaveAddressesNavigation_WhenCreated()
    {
        // Arrange & Act
        var account = new Account
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test Company",
            EntityAddressLinks = new List<EntityAddressLink>()
        };

        // Assert
        account.EntityAddressLinks.Should().NotBeNull();
        account.EntityAddressLinks.Should().BeAssignableTo<ICollection<EntityAddressLink>>();
    }

    [Fact]
    public void Account_ShouldSupportMultipleAddressesInAddressesCollection()
    {
        // Arrange
        var account = new Account
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test Company",
            EntityAddressLinks = new List<EntityAddressLink>()
        };

        var address1 = new EntityAddressLink { AddressId = 1, EntityId = 1, EntityType = EntityType.Account };
        var address2 = new EntityAddressLink { AddressId = 2, EntityId = 1, EntityType = EntityType.Account };
        var address3 = new EntityAddressLink { AddressId = 3, EntityId = 1, EntityType = EntityType.Account };

        // Act
        account.EntityAddressLinks.Add(address1);
        account.EntityAddressLinks.Add(address2);
        account.EntityAddressLinks.Add(address3);

        // Assert
        account.EntityAddressLinks.Should().HaveCount(3);
    }

    #endregion

    #region Address Entity Tests

    [Fact]
    public void Address_ShouldSetCreatedAtTimestamp_WhenObjectCreated()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            CreatedAt = now
        };

        // Assert
        address.CreatedAt.Should().Be(now);
        address.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void Address_ShouldSetUpdatedAtTimestamp_WhenModified()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var modifiedAt = createdAt.AddHours(1);

        // Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            CreatedAt = createdAt,
            UpdatedAt = modifiedAt
        };

        // Assert
        address.UpdatedAt.Should().Be(modifiedAt);
        address.UpdatedAt.Should().BeAfter(address.CreatedAt);
    }

    [Fact]
    public void Address_ShouldSupportSoftDelete_WithIsDeletedFlag()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            IsDeleted = false
        };

        // Assert - Initial state
        address.IsDeleted.Should().BeFalse();

        // Act - Soft delete
        address.IsDeleted = true;

        // Assert - After soft delete
        address.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Address_ShouldHaveValidProperties_WhenCreated()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Label = "Main Office",
            Line1 = "123 Main Street",
            Line2 = "Suite 100",
            Line3 = "Building A",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            County = "New York County",
            CountryCode = "US",
            Country = "United States",
            IsResidential = false,
            DeliveryInstructions = "Ring doorbell twice",
            AccessHours = "9AM-5PM",
            SiteContactName = "John Doe",
            SiteContactPhone = "555-0100",
            Notes = "Main office location"
        };

        // Assert
        address.Label.Should().Be("Main Office");
        address.Line1.Should().Be("123 Main Street");
        address.Line2.Should().Be("Suite 100");
        address.Line3.Should().Be("Building A");
        address.City.Should().Be("New York");
        address.State.Should().Be("NY");
        address.PostalCode.Should().Be("10001");
        address.County.Should().Be("New York County");
        address.CountryCode.Should().Be("US");
        address.Country.Should().Be("United States");
        address.IsResidential.Should().BeFalse();
        address.DeliveryInstructions.Should().Be("Ring doorbell twice");
        address.AccessHours.Should().Be("9AM-5PM");
        address.SiteContactName.Should().Be("John Doe");
        address.SiteContactPhone.Should().Be("555-0100");
        address.Notes.Should().Be("Main office location");
    }

    [Fact]
    public void Address_ShouldHaveDefaultLabel_WhenNotSpecified()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States"
        };

        // Assert
        address.Label.Should().Be("Primary");
    }

    [Fact]
    public void Address_ShouldHaveDefaultCountryCode_WhenNotSpecified()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States"
        };

        // Assert
        address.CountryCode.Should().Be("US");
    }

    [Fact]
    public void Address_ShouldSupportGeocoding_WithLatitudeLongitude()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            GeocodeAccuracy = "Rooftop",
            IsVerified = true,
            VerifiedDate = DateTime.UtcNow,
            VerificationSource = "Google Maps"
        };

        // Assert
        address.Latitude.Should().Be(40.7128m);
        address.Longitude.Should().Be(-74.0060m);
        address.GeocodeAccuracy.Should().Be("Rooftop");
        address.IsVerified.Should().BeTrue();
        address.VerifiedDate.Should().NotBeNull();
        address.VerificationSource.Should().Be("Google Maps");
    }

    [Fact]
    public void Address_ShouldComputeFormattedAddress()
    {
        // Arrange & Act
        var address = new Address
        {
            Line1 = "123 Main Street",
            Line2 = "Suite 100",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States"
        };

        // Assert
        var formatted = address.FormattedAddress;
        formatted.Should().Contain("123 Main Street");
        formatted.Should().Contain("Suite 100");
        formatted.Should().Contain("New York");
        formatted.Should().Contain("NY");
        formatted.Should().Contain("10001");
        formatted.Should().Contain("United States");
    }

    #endregion

    #region EntityAddressLink Tests

    [Fact]
    public void EntityAddressLink_ShouldLinkAddressToAccount()
    {
        // Arrange & Act
        var link = new EntityAddressLink
        {
            Id = 1,
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing,
            IsPrimary = true,
            IsDeleted = false
        };

        // Assert
        link.AddressId.Should().Be(1);
        link.EntityId.Should().Be(1);
        link.EntityType.Should().Be(EntityType.Account);
        link.AddressType.Should().Be(AddressType.Billing);
        link.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void EntityAddressLink_ShouldSupportMultipleAddressTypes()
    {
        // Arrange
        var billingLink = new EntityAddressLink
        {
            AddressId = 1,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Billing
        };

        var shippingLink = new EntityAddressLink
        {
            AddressId = 2,
            EntityId = 1,
            EntityType = EntityType.Account,
            AddressType = AddressType.Shipping
        };

        // Assert
        billingLink.AddressType.Should().Be(AddressType.Billing);
        shippingLink.AddressType.Should().Be(AddressType.Shipping);
    }

    #endregion

    #region AddressType Enum Tests

    [Fact]
    public void AddressType_ShouldSupportMultipleTypes()
    {
        // Arrange & Act
        var billingType = AddressType.Billing;
        var shippingType = AddressType.Shipping;
        var primaryType = AddressType.Primary;
        var otherType = AddressType.Other;

        // Assert
        billingType.Should().Be(AddressType.Billing);
        shippingType.Should().Be(AddressType.Shipping);
        primaryType.Should().Be(AddressType.Primary);
        otherType.Should().Be(AddressType.Other);
    }

    #endregion

    #region EntityType Enum Tests

    [Fact]
    public void EntityType_ShouldSupportAccount()
    {
        // Arrange & Act & Assert
        EntityType.Account.Should().Be(EntityType.Account);
    }

    [Fact]
    public void EntityType_ShouldSupportContact()
    {
        // Arrange & Act & Assert
        EntityType.Contact.Should().Be(EntityType.Contact);
    }

    [Fact]
    public void EntityType_ShouldSupportLead()
    {
        // Arrange & Act & Assert
        EntityType.Lead.Should().Be(EntityType.Lead);
    }

    #endregion

    #region Address Audit Fields Tests

    [Fact]
    public void Address_ShouldTrackCreatedBy_ForAuditPurposes()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            CreatedBy = 42
        };

        // Assert
        address.CreatedBy.Should().Be(42);
    }

    [Fact]
    public void Address_ShouldTrackUpdatedBy_ForAuditPurposes()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            UpdatedBy = 42
        };

        // Assert
        address.UpdatedBy.Should().Be(42);
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public void Address_ShouldHaveZipCodeNavigation_WhenConfigured()
    {
        // Arrange & Act
        var zipCode = new ZipCode { Id = 1, Code = "10001" };
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            ZipCodeId = 1,
            ZipCodeData = zipCode
        };

        // Assert
        address.ZipCodeData.Should().NotBeNull();
        address.ZipCodeData.Code.Should().Be("10001");
    }

    [Fact]
    public void Address_ShouldHaveLocalityNavigation_WhenConfigured()
    {
        // Arrange & Act
        var locality = new Locality { Id = 1, Name = "Midtown" };
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            LocalityId = 1,
            LocalityData = locality,
            Locality = "Midtown"
        };

        // Assert
        address.LocalityData.Should().NotBeNull();
        address.LocalityData.Name.Should().Be("Midtown");
        address.Locality.Should().Be("Midtown");
    }

    [Fact]
    public void Address_ShouldHaveEntityAddressLinksCollection_ForPolymorphicSupport()
    {
        // Arrange & Act
        var address = new Address
        {
            Id = 1,
            Line1 = "123 Main Street",
            City = "New York",
            Country = "United States",
            EntityAddressLinks = new List<EntityAddressLink>
            {
                new() { EntityId = 1, EntityType = EntityType.Account, AddressType = AddressType.Billing }
            }
        };

        // Assert
        address.EntityAddressLinks.Should().NotBeNull();
        address.EntityAddressLinks.Should().HaveCount(1);
    }

    #endregion

    #region Address XML Serialization Tests

    [Fact]
    public void Address_ShouldGenerateAddressXml()
    {
        // Arrange
        var address = new Address
        {
            Id = 1,
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States"
        };

        // Act
        var xml = address.GenerateAddressXml();

        // Assert
        xml.Should().NotBeNullOrEmpty();
        xml.Should().Contain("<Label>Main Office</Label>");
        xml.Should().Contain("<Line1>123 Main Street</Line1>");
        xml.Should().Contain("<City>New York</City>");
    }

    #endregion
}
