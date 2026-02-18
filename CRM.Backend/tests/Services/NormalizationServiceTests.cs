// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for NormalizationService.
/// Covers: GetTagsAsync, GetCustomFieldsAsync, GetPrimaryEmailAsync,
/// GetPrimaryPhoneAsync, GetPrimaryFaxAsync, GetPrimaryAddressAsync,
/// GetPrimarySocialAccountAsync.
/// </summary>
public class NormalizationServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly NormalizationService _service;

    public NormalizationServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _service = new NormalizationService(_mockContext.Object);
    }

    private void SetupDbSets(
        List<EntityTag>? entityTags = null,
        List<CustomField>? customFields = null,
        List<ContactInfoLink>? contactInfoLinks = null)
    {
        entityTags ??= new List<EntityTag>();
        customFields ??= new List<CustomField>();
        contactInfoLinks ??= new List<ContactInfoLink>();

        var mockEntityTags = MockDbSetFactory.CreateMockDbSet(entityTags);
        _mockContext.Setup(c => c.EntityTags).Returns(mockEntityTags.Object);

        var mockCustomFields = MockDbSetFactory.CreateMockDbSet(customFields);
        _mockContext.Setup(c => c.CustomFields).Returns(mockCustomFields.Object);

        var mockContactInfoLinks = MockDbSetFactory.CreateMockDbSet(contactInfoLinks);
        _mockContext.Setup(c => c.ContactInfoLinks).Returns(mockContactInfoLinks.Object);
    }

    // ========================================================================
    // GetTagsAsync
    // ========================================================================

    [Fact]
    public async Task GetTagsAsync_ShouldReturnCommaSeparatedTags_WhenTagNamesExist()
    {
        // Arrange
        var tags = new List<EntityTag>
        {
            new EntityTag { Id = 1, EntityType = "Account", EntityId = 10, TagName = "VIP", IsDeleted = false },
            new EntityTag { Id = 2, EntityType = "Account", EntityId = 10, TagName = "Enterprise", IsDeleted = false }
        };
        SetupDbSets(entityTags: tags);

        // Act
        var result = await _service.GetTagsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("VIP");
        result.Should().Contain("Enterprise");
    }

    [Fact]
    public async Task GetTagsAsync_ShouldFallBackToTagNavName_WhenTagNameIsNull()
    {
        // Arrange
        var tags = new List<EntityTag>
        {
            new EntityTag
            {
                Id = 1, EntityType = "Account", EntityId = 10, TagName = null,
                Tag = new Tag { Id = 100, Name = "Premium" },
                IsDeleted = false
            }
        };
        SetupDbSets(entityTags: tags);

        // Act
        var result = await _service.GetTagsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Premium");
    }

    [Fact]
    public async Task GetTagsAsync_ShouldReturnNull_WhenNoTagsFound()
    {
        // Arrange
        SetupDbSets(entityTags: new List<EntityTag>());

        // Act
        var result = await _service.GetTagsAsync("Account", 99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetTagsAsync_ShouldExcludeDeletedTags()
    {
        // Arrange
        var tags = new List<EntityTag>
        {
            new EntityTag { Id = 1, EntityType = "Account", EntityId = 10, TagName = "Active", IsDeleted = false },
            new EntityTag { Id = 2, EntityType = "Account", EntityId = 10, TagName = "Deleted", IsDeleted = true }
        };
        SetupDbSets(entityTags: tags);

        // Act
        var result = await _service.GetTagsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Active");
        result.Should().NotContain("Deleted");
    }

    [Fact]
    public async Task GetTagsAsync_ShouldFilterByEntityTypeAndId()
    {
        // Arrange
        var tags = new List<EntityTag>
        {
            new EntityTag { Id = 1, EntityType = "Account", EntityId = 10, TagName = "Match", IsDeleted = false },
            new EntityTag { Id = 2, EntityType = "Contact", EntityId = 10, TagName = "WrongType", IsDeleted = false },
            new EntityTag { Id = 3, EntityType = "Account", EntityId = 20, TagName = "WrongId", IsDeleted = false }
        };
        SetupDbSets(entityTags: tags);

        // Act
        var result = await _service.GetTagsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Match");
        result.Should().NotContain("WrongType");
        result.Should().NotContain("WrongId");
    }

    // ========================================================================
    // GetCustomFieldsAsync
    // ========================================================================

    [Fact]
    public async Task GetCustomFieldsAsync_ShouldReturnSemicolonSeparatedKeyValues()
    {
        // Arrange
        var fields = new List<CustomField>
        {
            new CustomField { Id = 1, EntityType = "Account", EntityId = 10, Key = "Region", Value = "EMEA" },
            new CustomField { Id = 2, EntityType = "Account", EntityId = 10, Key = "Segment", Value = "Enterprise" }
        };
        SetupDbSets(customFields: fields);

        // Act
        var result = await _service.GetCustomFieldsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Region=EMEA");
        result.Should().Contain("Segment=Enterprise");
        result.Should().Contain(";");
    }

    [Fact]
    public async Task GetCustomFieldsAsync_ShouldReturnNull_WhenNoFieldsFound()
    {
        // Arrange
        SetupDbSets(customFields: new List<CustomField>());

        // Act
        var result = await _service.GetCustomFieldsAsync("Account", 99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomFieldsAsync_ShouldFilterByEntityTypeAndId()
    {
        // Arrange
        var fields = new List<CustomField>
        {
            new CustomField { Id = 1, EntityType = "Account", EntityId = 10, Key = "Match", Value = "Yes" },
            new CustomField { Id = 2, EntityType = "Lead", EntityId = 10, Key = "WrongType", Value = "No" }
        };
        SetupDbSets(customFields: fields);

        // Act
        var result = await _service.GetCustomFieldsAsync("Account", 10);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("Match=Yes");
        result.Should().NotContain("WrongType");
    }

    // ========================================================================
    // GetPrimaryEmailAsync
    // ========================================================================

    [Fact]
    public async Task GetPrimaryEmailAsync_ShouldReturnPrimaryEmailValue()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                ContactDetail = new ContactDetail
                {
                    Id = 100,
                    DetailType = ContactDetailType.Email,
                    Value = "primary@example.com"
                }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryEmailAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().Be("primary@example.com");
    }

    [Fact]
    public async Task GetPrimaryEmailAsync_ShouldReturnNull_WhenNoEmailLinks()
    {
        // Arrange
        SetupDbSets(contactInfoLinks: new List<ContactInfoLink>());

        // Act
        var result = await _service.GetPrimaryEmailAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimaryEmailAsync_ShouldReturnNull_WhenDetailTypeIsPhone()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                ContactDetail = new ContactDetail
                {
                    Id = 100,
                    DetailType = ContactDetailType.Phone,
                    Value = "+1234567890"
                }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryEmailAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimaryEmailAsync_ShouldPreferPrimaryLink()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = false,
                IsDeleted = false,
                ContactDetail = new ContactDetail { Id = 100, DetailType = ContactDetailType.Email, Value = "secondary@example.com" }
            },
            new ContactInfoLink
            {
                Id = 2,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                ContactDetail = new ContactDetail { Id = 101, DetailType = ContactDetailType.Email, Value = "primary@example.com" }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryEmailAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().Be("primary@example.com");
    }

    // ========================================================================
    // GetPrimaryPhoneAsync
    // ========================================================================

    [Fact]
    public async Task GetPrimaryPhoneAsync_ShouldReturnPrimaryPhoneValue()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = 5,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                ContactDetail = new ContactDetail { Id = 100, DetailType = ContactDetailType.Phone, Value = "+1-555-0100" }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryPhoneAsync(ContactInfoOwnerType.Contact, 5);

        // Assert
        result.Should().Be("+1-555-0100");
    }

    [Fact]
    public async Task GetPrimaryPhoneAsync_ShouldReturnNull_WhenNoPhoneLinks()
    {
        // Arrange
        SetupDbSets(contactInfoLinks: new List<ContactInfoLink>());

        // Act
        var result = await _service.GetPrimaryPhoneAsync(ContactInfoOwnerType.Contact, 5);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetPrimaryFaxAsync
    // ========================================================================

    [Fact]
    public async Task GetPrimaryFaxAsync_ShouldReturnPrimaryFaxValue()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Lead,
                OwnerId = 3,
                InfoKind = ContactInfoKind.ContactDetail,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                ContactDetail = new ContactDetail { Id = 100, DetailType = ContactDetailType.Fax, Value = "+1-555-FAX1" }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryFaxAsync(ContactInfoOwnerType.Lead, 3);

        // Assert
        result.Should().Be("+1-555-FAX1");
    }

    // ========================================================================
    // GetPrimaryAddressAsync
    // ========================================================================

    [Fact]
    public async Task GetPrimaryAddressAsync_ShouldReturnPrimaryAddress()
    {
        // Arrange
        var address = new Address
        {
            Id = 50,
            Label = "HQ",
            Line1 = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "United States"
        };
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.Address,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                Address = address
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryAddressAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().NotBeNull();
        result!.Line1.Should().Be("123 Main St");
        result.City.Should().Be("Springfield");
        result.State.Should().Be("IL");
    }

    [Fact]
    public async Task GetPrimaryAddressAsync_ShouldReturnNull_WhenNoAddressLinks()
    {
        // Arrange
        SetupDbSets(contactInfoLinks: new List<ContactInfoLink>());

        // Act
        var result = await _service.GetPrimaryAddressAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimaryAddressAsync_ShouldExcludeDeletedLinks()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Account,
                OwnerId = 10,
                InfoKind = ContactInfoKind.Address,
                IsPrimaryForOwner = true,
                IsDeleted = true,
                Address = new Address { Id = 50, Label = "Old", Line1 = "456 Elm", City = "Old Town" }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimaryAddressAsync(ContactInfoOwnerType.Account, 10);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // GetPrimarySocialAccountAsync
    // ========================================================================

    [Fact]
    public async Task GetPrimarySocialAccountAsync_ShouldReturnHandleOrUrl()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = 5,
                InfoKind = ContactInfoKind.SocialAccount,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                SocialAccount = new SocialAccount
                {
                    Id = 200,
                    Network = SocialNetwork.LinkedIn,
                    HandleOrUrl = "https://linkedin.com/in/johndoe"
                }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Contact, 5, SocialNetwork.LinkedIn);

        // Assert
        result.Should().Be("https://linkedin.com/in/johndoe");
    }

    [Fact]
    public async Task GetPrimarySocialAccountAsync_ShouldReturnNull_WhenNetworkDoesNotMatch()
    {
        // Arrange
        var links = new List<ContactInfoLink>
        {
            new ContactInfoLink
            {
                Id = 1,
                OwnerType = ContactInfoOwnerType.Contact,
                OwnerId = 5,
                InfoKind = ContactInfoKind.SocialAccount,
                IsPrimaryForOwner = true,
                IsDeleted = false,
                SocialAccount = new SocialAccount
                {
                    Id = 200,
                    Network = SocialNetwork.Twitter,
                    HandleOrUrl = "@johndoe"
                }
            }
        };
        SetupDbSets(contactInfoLinks: links);

        // Act
        var result = await _service.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Contact, 5, SocialNetwork.LinkedIn);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPrimarySocialAccountAsync_ShouldReturnNull_WhenNoSocialLinks()
    {
        // Arrange
        SetupDbSets(contactInfoLinks: new List<ContactInfoLink>());

        // Act
        var result = await _service.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Contact, 5, SocialNetwork.LinkedIn);

        // Assert
        result.Should().BeNull();
    }
}
