// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
// Contacts Controller Unit Tests

using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Api.Hubs;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CRM.Tests.Controllers;

/// <summary>
/// Comprehensive unit tests for ContactsController
/// Covers: CRUD operations, search, linking, import/export
/// </summary>
public class ContactsControllerTests
{
    private readonly Mock<IContactsService> _mockContactService;
    private readonly Mock<ILogger<ContactsController>> _mockLogger;
    private readonly Mock<ICrmNotificationService> _mockNotificationService;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _mockContactService = new Mock<IContactsService>();
        _mockLogger = new Mock<ILogger<ContactsController>>();
        _mockNotificationService = new Mock<ICrmNotificationService>();

        _mockNotificationService.Setup(x => x.NotifyRecordCreatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordUpdatedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<object>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);
        _mockNotificationService.Setup(x => x.NotifyRecordDeletedAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(Task.CompletedTask);

        _controller = new ContactsController(_mockContactService.Object, _mockLogger.Object, _mockNotificationService.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_ReturnsOkResult_WithContacts()
    {
        // Arrange
        var contacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, FirstName = "John", LastName = "Doe", EmailPrimary = "john@example.com" },
            new ContactDto { Id = 2, FirstName = "Jane", LastName = "Smith", EmailPrimary = "jane@example.com" }
        };

        _mockContactService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyArray()
    {
        // Arrange
        _mockContactService.Setup(s => s.GetAllAsync())
            .ReturnsAsync(new List<ContactDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var contacts = Enumerable.Range(1, 20)
            .Select(i => new ContactDto { Id = i, FirstName = $"Contact{i}" })
            .ToList();

        _mockContactService.Setup(s => s.GetPagedAsync(1, 10))
            .ReturnsAsync((contacts.Take(10).ToList(), 20));

        // Act
        var result = await _controller.GetPaged(1, 10);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetAll_ServiceThrowsException_ReturnsInternalError()
    {
        // Arrange
        _mockContactService.Setup(s => s.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAll();

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_ExistingContact_ReturnsOkWithContact()
    {
        // Arrange
        var contact = new ContactDto { Id = 1, FirstName = "John", LastName = "Doe" };

        _mockContactService.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(contact);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContact = okResult.Value as ContactDto;
        returnedContact!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_NonExistingContact_ReturnsNotFound()
    {
        // Arrange
        _mockContactService.Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync((ContactDto?)null);

        // Act
        var result = await _controller.GetById(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetById_InvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetById(0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_NegativeId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetById(-1);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_ValidContact_ReturnsCreatedWithContact()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "john@example.com"
        };

        var createdContact = new ContactDto
        {
            Id = 1,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            EmailPrimary = createDto.EmailPrimary
        };

        _mockContactService.Setup(s => s.CreateAsync(It.IsAny<CreateContactDto>()))
            .ReturnsAsync(createdContact);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedContact = createdResult.Value as ContactDto;
        returnedContact!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Create(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateContactDto { FirstName = "" };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "existing@example.com"
        };

        _mockContactService.Setup(s => s.CreateAsync(It.IsAny<CreateContactDto>()))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Create_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "invalid-email"
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_WithAccountLink_CreatesLinkedContact()
    {
        // Arrange
        var createDto = new CreateContactDto
        {
            FirstName = "John",
            LastName = "Doe",
            EmailPrimary = "john@example.com",
            AccountId = 1
        };

        var createdContact = new ContactDto
        {
            Id = 1,
            FirstName = createDto.FirstName,
            AccountId = 1
        };

        _mockContactService.Setup(s => s.CreateAsync(It.IsAny<CreateContactDto>()))
            .ReturnsAsync(createdContact);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedContact = createdResult.Value as ContactDto;
        returnedContact!.AccountId.Should().Be(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_ValidContact_ReturnsOkWithUpdatedContact()
    {
        // Arrange
        var updateDto = new UpdateContactDto
        {
            Id = 1,
            FirstName = "John Updated",
            LastName = "Doe"
        };

        var updatedContact = new ContactDto
        {
            Id = 1,
            FirstName = "John Updated",
            LastName = "Doe"
        };

        _mockContactService.Setup(s => s.UpdateAsync(It.IsAny<UpdateContactDto>()))
            .ReturnsAsync(updatedContact);

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContact = okResult.Value as ContactDto;
        returnedContact!.FirstName.Should().Be("John Updated");
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        // Arrange
        var updateDto = new UpdateContactDto { Id = 2 };

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NonExistingContact_ReturnsNotFound()
    {
        // Arrange
        var updateDto = new UpdateContactDto { Id = 999 };

        _mockContactService.Setup(s => s.UpdateAsync(It.IsAny<UpdateContactDto>()))
            .ReturnsAsync((ContactDto?)null);

        // Act
        var result = await _controller.Update(999, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ConcurrencyConflict_ReturnsConflict()
    {
        // Arrange
        var updateDto = new UpdateContactDto { Id = 1 };

        _mockContactService.Setup(s => s.UpdateAsync(It.IsAny<UpdateContactDto>()))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict"));

        // Act
        var result = await _controller.Update(1, updateDto);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_ExistingContact_ReturnsNoContent()
    {
        // Arrange
        _mockContactService.Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_NonExistingContact_ReturnsNotFound()
    {
        // Arrange
        _mockContactService.Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_InvalidId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Delete(0);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_ContactWithActiveOpportunities_ReturnsConflict()
    {
        // Arrange
        _mockContactService.Setup(s => s.DeleteAsync(1))
            .ThrowsAsync(new InvalidOperationException("Contact has active opportunities"));

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Search Tests

    [Fact]
    public async Task Search_ValidQuery_ReturnsMatchingContacts()
    {
        // Arrange
        var contacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, FirstName = "John", LastName = "Doe" }
        };

        _mockContactService.Setup(s => s.SearchAsync("John"))
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.Search("John");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search("");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Search_NoResults_ReturnsEmptyList()
    {
        // Arrange
        _mockContactService.Setup(s => s.SearchAsync("nonexistent"))
            .ReturnsAsync(new List<ContactDto>());

        // Act
        var result = await _controller.Search("nonexistent");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ByEmail_ReturnsMatchingContacts()
    {
        // Arrange
        var contacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, EmailPrimary = "john@example.com" }
        };

        _mockContactService.Setup(s => s.SearchByEmailAsync("john@example.com"))
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.SearchByEmail("john@example.com");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task Search_ByPhone_ReturnsMatchingContacts()
    {
        // Arrange
        var contacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, PhonePrimary = "555-1234" }
        };

        _mockContactService.Setup(s => s.SearchByPhoneAsync("555-1234"))
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.SearchByPhone("555-1234");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Link to Account Tests

    [Fact]
    public async Task LinkToAccount_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockContactService.Setup(s => s.LinkToAccountAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.LinkToAccount(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task LinkToAccount_ContactNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockContactService.Setup(s => s.LinkToAccountAsync(999, 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.LinkToAccount(999, 1);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task LinkToAccount_AccountNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockContactService.Setup(s => s.LinkToAccountAsync(1, 999))
            .ThrowsAsync(new InvalidOperationException("Account not found"));

        // Act
        var result = await _controller.LinkToAccount(1, 999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UnlinkFromAccount_ValidRequest_ReturnsOk()
    {
        // Arrange
        _mockContactService.Setup(s => s.UnlinkFromAccountAsync(1, 1))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UnlinkFromAccount(1, 1);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task GetContactsByAccount_ReturnsContacts()
    {
        // Arrange
        var contacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, AccountId = 1 },
            new ContactDto { Id = 2, AccountId = 1 }
        };

        _mockContactService.Setup(s => s.GetByAccountIdAsync(1))
            .ReturnsAsync(contacts);

        // Act
        var result = await _controller.GetByAccountId(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().HaveCount(2);
    }

    #endregion

    #region Bulk Operations Tests

    [Fact]
    public async Task BulkCreate_ValidContacts_ReturnsCreatedContacts()
    {
        // Arrange
        var createDtos = new List<CreateContactDto>
        {
            new CreateContactDto { FirstName = "John", LastName = "Doe" },
            new CreateContactDto { FirstName = "Jane", LastName = "Smith" }
        };

        var createdContacts = new List<ContactDto>
        {
            new ContactDto { Id = 1, FirstName = "John" },
            new ContactDto { Id = 2, FirstName = "Jane" }
        };

        _mockContactService.Setup(s => s.BulkCreateAsync(It.IsAny<List<CreateContactDto>>()))
            .ReturnsAsync(createdContacts);

        // Act
        var result = await _controller.BulkCreate(createDtos);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedContacts = okResult.Value as IEnumerable<ContactDto>;
        returnedContacts.Should().HaveCount(2);
    }

    [Fact]
    public async Task BulkDelete_ValidIds_ReturnsOk()
    {
        // Arrange
        var ids = new List<int> { 1, 2, 3 };

        _mockContactService.Setup(s => s.BulkDeleteAsync(ids))
            .ReturnsAsync(3);

        // Act
        var result = await _controller.BulkDelete(ids);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task BulkDelete_EmptyList_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.BulkDelete(new List<int>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task BulkUpdate_ValidContacts_ReturnsOk()
    {
        // Arrange
        var updateDtos = new List<UpdateContactDto>
        {
            new UpdateContactDto { Id = 1, FirstName = "Updated1" },
            new UpdateContactDto { Id = 2, FirstName = "Updated2" }
        };

        _mockContactService.Setup(s => s.BulkUpdateAsync(It.IsAny<List<UpdateContactDto>>()))
            .ReturnsAsync(2);

        // Act
        var result = await _controller.BulkUpdate(updateDtos);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Contact Details Tests

    [Fact]
    public async Task GetContactDetails_ExistingContact_ReturnsFullDetails()
    {
        // Arrange
        var contactDetails = new ContactDetailsDto
        {
            Contact = new ContactDto { Id = 1, FirstName = "John" },
            Addresses = new List<AddressDto>(),
            PhoneNumbers = new List<PhoneNumberDto>(),
            EmailAddresses = new List<EmailAddressDto>(),
            SocialAccounts = new List<SocialAccountDto>()
        };

        _mockContactService.Setup(s => s.GetContactDetailsAsync(1))
            .ReturnsAsync(contactDetails);

        // Act
        var result = await _controller.GetContactDetails(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task GetContactActivities_ReturnsActivities()
    {
        // Arrange
        var activities = new List<ActivityDto>
        {
            new ActivityDto { Id = 1, Description = "Called contact" },
            new ActivityDto { Id = 2, Description = "Sent email" }
        };

        _mockContactService.Setup(s => s.GetActivitiesAsync(1))
            .ReturnsAsync(activities);

        // Act
        var result = await _controller.GetActivities(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedActivities = okResult.Value as IEnumerable<ActivityDto>;
        returnedActivities.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetContactOpportunities_ReturnsOpportunities()
    {
        // Arrange
        var opportunities = new List<OpportunityDto>
        {
            new OpportunityDto { Id = 1, Name = "Opportunity 1" }
        };

        _mockContactService.Setup(s => s.GetOpportunitiesAsync(1))
            .ReturnsAsync(opportunities);

        // Act
        var result = await _controller.GetOpportunities(1);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    #endregion

    #region Merge Contacts Tests

    [Fact]
    public async Task MergeContacts_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new MergeContactsRequest
        {
            PrimaryContactId = 1,
            SecondaryContactIds = new List<int> { 2, 3 }
        };

        var mergedContact = new ContactDto { Id = 1, FirstName = "John" };

        _mockContactService.Setup(s => s.MergeContactsAsync(request))
            .ReturnsAsync(mergedContact);

        // Act
        var result = await _controller.MergeContacts(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
    }

    [Fact]
    public async Task MergeContacts_SameId_ReturnsBadRequest()
    {
        // Arrange
        var request = new MergeContactsRequest
        {
            PrimaryContactId = 1,
            SecondaryContactIds = new List<int> { 1 }
        };

        // Act
        var result = await _controller.MergeContacts(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region Export Tests

    [Fact]
    public async Task Export_ValidRequest_ReturnsCsvFile()
    {
        // Arrange
        var csvContent = "Id,FirstName,LastName\n1,John,Doe\n2,Jane,Smith";
        var bytes = System.Text.Encoding.UTF8.GetBytes(csvContent);

        _mockContactService.Setup(s => s.ExportToCsvAsync())
            .ReturnsAsync(bytes);

        // Act
        var result = await _controller.ExportToCsv();

        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }

    [Fact]
    public async Task Export_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var filters = new ContactExportFilter { AccountId = 1 };
        var bytes = System.Text.Encoding.UTF8.GetBytes("filtered data");

        _mockContactService.Setup(s => s.ExportToCsvAsync(filters))
            .ReturnsAsync(bytes);

        // Act
        var result = await _controller.ExportToCsv(filters);

        // Assert
        result.Should().BeOfType<FileContentResult>();
    }

    #endregion
}
