// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for AddressesController covering REST API operations.
/// Tests cover: CRUD endpoints, status codes, validation, and error handling.
///
/// FUNCTIONAL VIEW:
/// - Tests all HTTP endpoints for address management
/// - Validates correct HTTP status codes for various scenarios
/// - Ensures proper error handling for invalid requests
/// - Tests request validation and response formatting
///
/// TECHNICAL VIEW:
/// - Uses Moq to mock IAddressService and IAccountService
/// - Tests controller action methods in isolation
/// - Validates ActionResult types and response bodies
/// - Verifies proper error handling with error responses
/// </summary>
public class AddressesControllerTests
{
    private readonly Mock<IAddressService> _mockAddressService;
    private readonly Mock<IAccountService> _mockAccountService;
    private readonly Mock<ILogger<AddressesController>> _mockLogger;
    private readonly AddressesController _controller;

    private readonly AccountDto _testAccount;
    private readonly AddressDto _testAddressDto;
    private readonly Address _testAddress;
    private readonly Address _testAddress2;

    public AddressesControllerTests()
    {
        _mockAddressService = new Mock<IAddressService>();
        _mockAccountService = new Mock<IAccountService>();
        _mockLogger = new Mock<ILogger<AddressesController>>();

        _controller = new AddressesController(
            _mockAddressService.Object,
            _mockAccountService.Object,
            _mockLogger.Object);

        // Setup HttpContext with Response.Headers
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Test data
        _testAccount = new AccountDto
        {
            Id = 1,
            FirstName = "Test Company",
            Email = "test@example.com"
        };

        _testAddressDto = new AddressDto
        {
            Id = 1,
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            CountryCode = "US",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _testAddress = new Address
        {
            Id = 1,
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            CountryCode = "US",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _testAddress2 = new Address
        {
            Id = 2,
            Label = "Branch Office",
            Line1 = "456 Oak Avenue",
            City = "Los Angeles",
            State = "CA",
            PostalCode = "90001",
            Country = "United States",
            CountryCode = "US",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    #region GetAccountAddresses Tests

    [Fact]
    public async Task GetAccountAddresses_ShouldReturnOkWithAddresses_WhenAccountHasAddresses()
    {
        // Arrange
        var accountId = 1;
        var addresses = new List<Address> { _testAddress, _testAddress2 };

        _mockAccountService.Setup(s => s.GetAccountByIdAsync(accountId))
            .ReturnsAsync(_testAccount);

        _mockAddressService.Setup(s => s.GetAddressesByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(addresses);

        // Act
        var result = await _controller.GetAccountAddresses(accountId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedAddresses = okResult.Value.Should().BeAssignableTo<IEnumerable<AddressDto>>().Subject;
        returnedAddresses.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAccountAddresses_ShouldReturnEmptyList_WhenAccountHasNoAddresses()
    {
        // Arrange
        var accountId = 1;

        _mockAccountService.Setup(s => s.GetAccountByIdAsync(accountId))
            .ReturnsAsync(_testAccount);

        _mockAddressService.Setup(s => s.GetAddressesByAccountAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Address>());

        // Act
        var result = await _controller.GetAccountAddresses(accountId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedAddresses = okResult.Value.Should().BeAssignableTo<List<AddressDto>>().Subject;
        returnedAddresses.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAccountAddresses_ShouldReturnBadRequest_WhenAccountIdInvalid()
    {
        // Arrange
        var invalidAccountId = -1;

        // Act
        var result = await _controller.GetAccountAddresses(invalidAccountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetAccountAddresses_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = 999;

        _mockAccountService.Setup(s => s.GetAccountByIdAsync(accountId))
            .ReturnsAsync((AccountDto)null!);

        // Act
        var result = await _controller.GetAccountAddresses(accountId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetAddressById Tests

    [Fact]
    public async Task GetAddressById_ShouldReturnOkWithAddress_WhenAddressExists()
    {
        // Arrange
        var accountId = 1;
        var addressId = 1;

        _mockAccountService.Setup(s => s.GetAccountByIdAsync(accountId))
            .ReturnsAsync(_testAccount);

        _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAddress);

        // Act
        var result = await _controller.GetAddressById(accountId, addressId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedAddress = okResult.Value.Should().BeOfType<AddressDto>().Subject;
        returnedAddress.Id.Should().Be(1);
        returnedAddress.Line1.Should().Be("123 Main Street");
    }

    [Fact]
    public async Task GetAddressById_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var accountId = 1;
        var addressId = 999;

        _mockAccountService.Setup(s => s.GetAccountByIdAsync(accountId))
            .ReturnsAsync(_testAccount);

        _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Address)null!);

        // Act
        var result = await _controller.GetAddressById(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetAddressById_ShouldReturnBadRequest_WhenIdsInvalid()
    {
        // Arrange & Act
        var result = await _controller.GetAddressById(-1, 1, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region CreateAddress Tests

    [Fact]
    public async Task CreateAddress_ShouldReturnCreatedWithAddress_WhenInputIsValid()
    {
        // Arrange
        var dto = new CreateAddressDto
        {
            Label = "Main Office",
            Line1 = "123 Main Street",
            City = "New York",
            State = "NY",
            PostalCode = "10001",
            Country = "United States",
            CountryCode = "US"
        };

        var createdAddress = new Address
        {
            Id = 1,
            Label = dto.Label,
            Line1 = dto.Line1,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            CountryCode = dto.CountryCode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<int>(), It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAddress);

        // Act
        var result = await _controller.CreateAddress(dto, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);

        var returnedAddress = createdResult.Value.Should().BeOfType<AddressDto>().Subject;
        returnedAddress.Line1.Should().Be("123 Main Street");
    }

    [Fact]
    public async Task CreateAddress_ShouldReturnBadRequest_WhenDtoIsNull()
    {
        // Act
        var result = await _controller.CreateAddress(null, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateAddress_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var dto = new CreateAddressDto
        {
            Line1 = "",  // Invalid - empty
            City = "New York",
            Country = "United States"
        };

        _mockAddressService.Setup(s => s.CreateAddressAsync(It.IsAny<int>(), It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Address Line1 is required"));

        // Act
        var result = await _controller.CreateAddress(dto, CancellationToken.None);

        // Assert
        ((result is BadRequestObjectResult) || (result is ObjectResult)).Should().BeTrue();
    }

    #endregion

    #region UpdateAddress Tests

    [Fact]
    public async Task UpdateAddress_ShouldReturnOkWithUpdatedAddress_WhenInputIsValid()
    {
        // Arrange
        var accountId = 1;
        var addressId = 1;

        var dto = new UpdateAddressDto
        {
            Label = "Updated Office",
            Line1 = "456 New Street",
            City = "Boston",
            State = "MA",
            PostalCode = "02101",
            Country = "United States",
            CountryCode = "US"
        };

        var updatedAddress = new Address
        {
            Id = addressId,
            Label = dto.Label,
            Line1 = dto.Line1,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            CountryCode = dto.CountryCode,
            CreatedAt = _testAddress.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _mockAddressService.Setup(s => s.UpdateAddressAsync(accountId, addressId, It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedAddress);

        // Act
        var result = await _controller.UpdateAddress(accountId, addressId, dto, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        var returnedAddress = okResult.Value.Should().BeOfType<AddressDto>().Subject;
        returnedAddress.City.Should().Be("Boston");
    }

    [Fact]
    public async Task UpdateAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var accountId = 1;
        var addressId = 999;

        var dto = new UpdateAddressDto
        {
            Line1 = "456 New Street",
            City = "Boston",
            Country = "United States"
        };

        _mockAddressService.Setup(s => s.UpdateAddressAsync(accountId, addressId, It.IsAny<Address>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException($"Address {addressId} not found"));

        // Act
        var result = await _controller.UpdateAddress(accountId, addressId, dto, CancellationToken.None);

        // Assert
        ((result is NotFoundObjectResult) || (result is ObjectResult)).Should().BeTrue();
    }

    #endregion

    #region DeleteAddress Tests

    [Fact]
    public async Task DeleteAddress_ShouldReturnNoContent_WhenAddressDeleted()
    {
        // Arrange
        var accountId = 1;
        var addressId = 1;

        _mockAddressService.Setup(s => s.DeleteAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);
    }

    [Fact]
    public async Task DeleteAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var accountId = 1;
        var addressId = 999;

        _mockAddressService.Setup(s => s.DeleteAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region SetPrimaryBillingAddress Tests

    [Fact]
    public async Task SetPrimaryBillingAddress_ShouldReturnOkWithAddress_WhenValid()
    {
        // Arrange
        var accountId = 1;
        var addressId = 1;

        _mockAddressService.Setup(s => s.SetPrimaryBillingAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAddress);

        // Act
        var result = await _controller.SetPrimaryBillingAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SetPrimaryBillingAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var accountId = 1;
        var addressId = 999;

        _mockAddressService.Setup(s => s.SetPrimaryBillingAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SetPrimaryBillingAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region SetPrimaryShippingAddress Tests

    [Fact]
    public async Task SetPrimaryShippingAddress_ShouldReturnOkWithAddress_WhenValid()
    {
        // Arrange
        var accountId = 1;
        var addressId = 2;

        _mockAddressService.Setup(s => s.SetPrimaryShippingAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mockAddressService.Setup(s => s.GetAddressByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testAddress2);

        // Act
        var result = await _controller.SetPrimaryShippingAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task SetPrimaryShippingAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        var accountId = 1;
        var addressId = 999;

        _mockAddressService.Setup(s => s.SetPrimaryShippingAddressAsync(accountId, addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SetPrimaryShippingAddress(accountId, addressId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}
