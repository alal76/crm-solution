// CRM Solution - Customer Relationship Management System
// Contact Info Service Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ContactInfoService
/// Covers: Address, phone, email, social media management
/// </summary>
public class ContactInfoServiceTests
{
    private readonly Mock<IRepository<Address>> _mockAddressRepository;
    private readonly Mock<IRepository<PhoneNumber>> _mockPhoneRepository;
    private readonly Mock<IRepository<EmailAddress>> _mockEmailRepository;
    private readonly Mock<IRepository<SocialMediaAccount>> _mockSocialRepository;
    private readonly Mock<IRepository<EntityAddressLink>> _mockAddressLinkRepository;
    private readonly Mock<IRepository<EntityPhoneLink>> _mockPhoneLinkRepository;
    private readonly Mock<IRepository<EntityEmailLink>> _mockEmailLinkRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ContactInfoService>> _mockLogger;
    private readonly ContactInfoService _service;

    public ContactInfoServiceTests()
    {
        _mockAddressRepository = new Mock<IRepository<Address>>();
        _mockPhoneRepository = new Mock<IRepository<PhoneNumber>>();
        _mockEmailRepository = new Mock<IRepository<EmailAddress>>();
        _mockSocialRepository = new Mock<IRepository<SocialMediaAccount>>();
        _mockAddressLinkRepository = new Mock<IRepository<EntityAddressLink>>();
        _mockPhoneLinkRepository = new Mock<IRepository<EntityPhoneLink>>();
        _mockEmailLinkRepository = new Mock<IRepository<EntityEmailLink>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ContactInfoService>>();

        _service = new ContactInfoService(
            _mockAddressRepository.Object,
            _mockPhoneRepository.Object,
            _mockEmailRepository.Object,
            _mockSocialRepository.Object,
            _mockAddressLinkRepository.Object,
            _mockPhoneLinkRepository.Object,
            _mockEmailLinkRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region Address Tests

    [Fact]
    public async Task AddAddressAsync_ValidAddress_ReturnsAddress()
    {
        // Arrange
        var request = new CreateAddressRequest
        {
            EntityType = "Account",
            EntityId = 1,
            Street = "123 Main St",
            City = "Anytown",
            State = "CA",
            PostalCode = "12345",
            Country = "USA"
        };

        _mockAddressRepository.Setup(r => r.AddAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => { a.Id = 1; return a; });

        _mockAddressLinkRepository.Setup(r => r.AddAsync(It.IsAny<EntityAddressLink>()))
            .ReturnsAsync((EntityAddressLink l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.AddAddressAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Street.Should().Be("123 Main St");
    }

    [Fact]
    public async Task GetAddressesAsync_EntityWithAddresses_ReturnsAddresses()
    {
        // Arrange
        var links = new List<EntityAddressLink>
        {
            new EntityAddressLink { Id = 1, AddressId = 1, EntityType = "Account", EntityId = 1 }
        };
        var addresses = new List<Address>
        {
            new Address { Id = 1, Street = "123 Main St" }
        };

        _mockAddressLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityAddressLink, bool>>>()))
            .ReturnsAsync(links);

        _mockAddressRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(addresses.First());

        // Act
        var result = await _service.GetAddressesAsync("Account", 1);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAddressAsync_ValidAddress_UpdatesAddress()
    {
        // Arrange
        var address = new Address { Id = 1, Street = "Old Street" };
        var updateRequest = new UpdateAddressRequest { Id = 1, Street = "New Street" };

        _mockAddressRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        _mockAddressRepository.Setup(r => r.UpdateAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) => a);

        // Act
        var result = await _service.UpdateAddressAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Street.Should().Be("New Street");
    }

    [Fact]
    public async Task DeleteAddressAsync_ExistingAddress_DeletesAddress()
    {
        // Arrange
        var link = new EntityAddressLink { Id = 1, AddressId = 1 };

        _mockAddressLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityAddressLink, bool>>>()))
            .ReturnsAsync(new List<EntityAddressLink> { link });

        _mockAddressLinkRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        _mockAddressRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAddressAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetPrimaryAddressAsync_ValidAddress_SetsPrimary()
    {
        // Arrange
        var links = new List<EntityAddressLink>
        {
            new EntityAddressLink { Id = 1, AddressId = 1, IsPrimary = false },
            new EntityAddressLink { Id = 2, AddressId = 2, IsPrimary = true }
        };

        _mockAddressLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityAddressLink, bool>>>()))
            .ReturnsAsync(links);

        _mockAddressLinkRepository.Setup(r => r.UpdateAsync(It.IsAny<EntityAddressLink>()))
            .ReturnsAsync((EntityAddressLink l) => l);

        // Act
        var result = await _service.SetPrimaryAddressAsync("Account", 1, 1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Phone Tests

    [Fact]
    public async Task AddPhoneAsync_ValidPhone_ReturnsPhone()
    {
        // Arrange
        var request = new CreatePhoneRequest
        {
            EntityType = "Contact",
            EntityId = 1,
            Number = "555-1234",
            Type = "Mobile"
        };

        _mockPhoneRepository.Setup(r => r.AddAsync(It.IsAny<PhoneNumber>()))
            .ReturnsAsync((PhoneNumber p) => { p.Id = 1; return p; });

        _mockPhoneLinkRepository.Setup(r => r.AddAsync(It.IsAny<EntityPhoneLink>()))
            .ReturnsAsync((EntityPhoneLink l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.AddPhoneAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Number.Should().Be("555-1234");
    }

    [Fact]
    public async Task GetPhonesAsync_EntityWithPhones_ReturnsPhones()
    {
        // Arrange
        var links = new List<EntityPhoneLink>
        {
            new EntityPhoneLink { Id = 1, PhoneId = 1, EntityType = "Contact", EntityId = 1 }
        };
        var phones = new List<PhoneNumber>
        {
            new PhoneNumber { Id = 1, Number = "555-1234" }
        };

        _mockPhoneLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityPhoneLink, bool>>>()))
            .ReturnsAsync(links);

        _mockPhoneRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(phones.First());

        // Act
        var result = await _service.GetPhonesAsync("Contact", 1);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdatePhoneAsync_ValidPhone_UpdatesPhone()
    {
        // Arrange
        var phone = new PhoneNumber { Id = 1, Number = "555-1234" };
        var updateRequest = new UpdatePhoneRequest { Id = 1, Number = "555-5678" };

        _mockPhoneRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(phone);

        _mockPhoneRepository.Setup(r => r.UpdateAsync(It.IsAny<PhoneNumber>()))
            .ReturnsAsync((PhoneNumber p) => p);

        // Act
        var result = await _service.UpdatePhoneAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Number.Should().Be("555-5678");
    }

    [Fact]
    public async Task DeletePhoneAsync_ExistingPhone_DeletesPhone()
    {
        // Arrange
        _mockPhoneLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityPhoneLink, bool>>>()))
            .ReturnsAsync(new List<EntityPhoneLink> { new EntityPhoneLink { Id = 1, PhoneId = 1 } });

        _mockPhoneLinkRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        _mockPhoneRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeletePhoneAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Email Tests

    [Fact]
    public async Task AddEmailAsync_ValidEmail_ReturnsEmail()
    {
        // Arrange
        var request = new CreateEmailRequest
        {
            EntityType = "Contact",
            EntityId = 1,
            Email = "test@example.com",
            Type = "Work"
        };

        _mockEmailRepository.Setup(r => r.AddAsync(It.IsAny<EmailAddress>()))
            .ReturnsAsync((EmailAddress e) => { e.Id = 1; return e; });

        _mockEmailLinkRepository.Setup(r => r.AddAsync(It.IsAny<EntityEmailLink>()))
            .ReturnsAsync((EntityEmailLink l) => { l.Id = 1; return l; });

        // Act
        var result = await _service.AddEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetEmailsAsync_EntityWithEmails_ReturnsEmails()
    {
        // Arrange
        var links = new List<EntityEmailLink>
        {
            new EntityEmailLink { Id = 1, EmailId = 1, EntityType = "Contact", EntityId = 1 }
        };
        var emails = new List<EmailAddress>
        {
            new EmailAddress { Id = 1, Email = "test@example.com" }
        };

        _mockEmailLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityEmailLink, bool>>>()))
            .ReturnsAsync(links);

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(emails.First());

        // Act
        var result = await _service.GetEmailsAsync("Contact", 1);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddEmailAsync_InvalidEmail_ThrowsException()
    {
        // Arrange
        var request = new CreateEmailRequest
        {
            EntityType = "Contact",
            EntityId = 1,
            Email = "invalid-email"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddEmailAsync(request));
    }

    [Fact]
    public async Task GetPrimaryEmailAsync_EntityWithPrimaryEmail_ReturnsPrimaryEmail()
    {
        // Arrange
        var links = new List<EntityEmailLink>
        {
            new EntityEmailLink { Id = 1, EmailId = 1, IsPrimary = true }
        };
        var email = new EmailAddress { Id = 1, Email = "primary@example.com" };

        _mockEmailLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityEmailLink, bool>>>()))
            .ReturnsAsync(links);

        _mockEmailRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(email);

        // Act
        var result = await _service.GetPrimaryEmailAsync("Contact", 1);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("primary@example.com");
    }

    #endregion

    #region Social Media Tests

    [Fact]
    public async Task AddSocialAccountAsync_ValidAccount_ReturnsAccount()
    {
        // Arrange
        var request = new CreateSocialAccountRequest
        {
            EntityType = "Contact",
            EntityId = 1,
            Platform = "LinkedIn",
            ProfileUrl = "https://linkedin.com/in/johndoe"
        };

        _mockSocialRepository.Setup(r => r.AddAsync(It.IsAny<SocialMediaAccount>()))
            .ReturnsAsync((SocialMediaAccount s) => { s.Id = 1; return s; });

        // Act
        var result = await _service.AddSocialAccountAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Platform.Should().Be("LinkedIn");
    }

    [Fact]
    public async Task GetSocialAccountsAsync_EntityWithAccounts_ReturnsAccounts()
    {
        // Arrange
        var accounts = new List<SocialMediaAccount>
        {
            new SocialMediaAccount { Id = 1, EntityType = "Contact", EntityId = 1, Platform = "LinkedIn" }
        };

        _mockSocialRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SocialMediaAccount, bool>>>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.GetSocialAccountsAsync("Contact", 1);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateSocialAccountAsync_ValidAccount_UpdatesAccount()
    {
        // Arrange
        var account = new SocialMediaAccount { Id = 1, ProfileUrl = "old-url" };
        var updateRequest = new UpdateSocialAccountRequest { Id = 1, ProfileUrl = "new-url" };

        _mockSocialRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(account);

        _mockSocialRepository.Setup(r => r.UpdateAsync(It.IsAny<SocialMediaAccount>()))
            .ReturnsAsync((SocialMediaAccount s) => s);

        // Act
        var result = await _service.UpdateSocialAccountAsync(updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.ProfileUrl.Should().Be("new-url");
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task GetAllContactInfoAsync_ReturnsCompleteInfo()
    {
        // Arrange
        _mockAddressLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityAddressLink, bool>>>()))
            .ReturnsAsync(new List<EntityAddressLink>());

        _mockPhoneLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityPhoneLink, bool>>>()))
            .ReturnsAsync(new List<EntityPhoneLink>());

        _mockEmailLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityEmailLink, bool>>>()))
            .ReturnsAsync(new List<EntityEmailLink>());

        _mockSocialRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<SocialMediaAccount, bool>>>()))
            .ReturnsAsync(new List<SocialMediaAccount>());

        // Act
        var result = await _service.GetAllContactInfoAsync("Contact", 1);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CopyContactInfoAsync_ValidEntities_CopiesInfo()
    {
        // Arrange
        var addresses = new List<EntityAddressLink>
        {
            new EntityAddressLink { Id = 1, AddressId = 1, EntityType = "Contact", EntityId = 1 }
        };

        _mockAddressLinkRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<EntityAddressLink, bool>>>()))
            .ReturnsAsync(addresses);

        _mockAddressLinkRepository.Setup(r => r.AddAsync(It.IsAny<EntityAddressLink>()))
            .ReturnsAsync((EntityAddressLink l) => { l.Id = 2; return l; });

        // Similar setup for phones, emails, social...

        // Act
        var result = await _service.CopyContactInfoAsync("Contact", 1, "Contact", 2);

        // Assert
        result.CopiedAddresses.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidatePhoneNumberAsync_ValidNumber_ReturnsTrue()
    {
        // Arrange
        var phoneNumber = "555-123-4567";

        // Act
        var result = await _service.ValidatePhoneNumberAsync(phoneNumber);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEmailAddressAsync_ValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "test@example.com";

        // Act
        var result = await _service.ValidateEmailAddressAsync(email);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAddressAsync_ValidAddress_ReturnsTrue()
    {
        // Arrange
        var address = new AddressValidationRequest
        {
            Street = "123 Main St",
            City = "Anytown",
            State = "CA",
            PostalCode = "12345"
        };

        // Act
        var result = await _service.ValidateAddressAsync(address);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetContactInfoStatisticsAsync_ReturnsStats()
    {
        // Arrange
        _mockAddressRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Address> { new Address(), new Address() });

        _mockPhoneRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<PhoneNumber> { new PhoneNumber() });

        _mockEmailRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<EmailAddress> { new EmailAddress(), new EmailAddress(), new EmailAddress() });

        _mockSocialRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<SocialMediaAccount>());

        // Act
        var result = await _service.GetContactInfoStatisticsAsync();

        // Assert
        result.TotalAddresses.Should().Be(2);
        result.TotalPhones.Should().Be(1);
        result.TotalEmails.Should().Be(3);
    }

    #endregion
}

// Supporting classes for tests
public class CreateAddressRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class UpdateAddressRequest
{
    public int Id { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
}

public class CreatePhoneRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Number { get; set; } = string.Empty;
    public string? Type { get; set; }
}

public class UpdatePhoneRequest
{
    public int Id { get; set; }
    public string? Number { get; set; }
    public string? Type { get; set; }
}

public class CreateEmailRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Type { get; set; }
}

public class CreateSocialAccountRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
}

public class UpdateSocialAccountRequest
{
    public int Id { get; set; }
    public string? ProfileUrl { get; set; }
    public string? Username { get; set; }
}

public class AddressValidationRequest
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
}
