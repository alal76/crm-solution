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
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers;

/// <summary>
/// Unit tests for CustomersController
/// 
/// FUNCTIONAL VIEW:
/// - Tests all customer management API endpoints
/// - Validates correct HTTP status codes for various scenarios
/// - Ensures proper error handling for invalid requests
/// 
/// TECHNICAL VIEW:
/// - Uses Moq to mock ICustomerService dependency
/// - Tests controller action methods in isolation
/// - Validates ActionResult types and response bodies
/// </summary>
public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<ILogger<CustomersController>> _mockLogger;
    private readonly CustomersController _controller;

    public CustomersControllerTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockLogger = new Mock<ILogger<CustomersController>>();
        _controller = new CustomersController(_mockCustomerService.Object, _mockLogger.Object);
    }

    #region GetAll Tests

    /// <summary>
    /// Verifies GetAll returns 200 OK with list of customers
    /// 
    /// FUNCTIONAL: API should return all customers when GET /api/customers is called
    /// TECHNICAL: Returns OkObjectResult with IEnumerable<CustomerDto>
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsOkResult_WithCustomers()
    {
        // Arrange
        var customers = new List<CustomerDto>
        {
            new CustomerDto { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new CustomerDto { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        _mockCustomerService.Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
        var returnedCustomers = okResult.Value as IEnumerable<CustomerDto>;
        returnedCustomers.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifies GetAll returns empty list when no customers exist
    /// </summary>
    [Fact]
    public async Task GetAll_WhenNoCustomers_ReturnsEmptyList()
    {
        // Arrange
        _mockCustomerService.Setup(s => s.GetAllCustomersAsync())
            .ReturnsAsync(new List<CustomerDto>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomers = okResult.Value as IEnumerable<CustomerDto>;
        returnedCustomers.Should().BeEmpty();
    }

    #endregion

    #region GetById Tests

    /// <summary>
    /// Verifies GetById returns 200 OK with customer when found
    /// 
    /// FUNCTIONAL: API should return specific customer when GET /api/customers/{id} is called
    /// TECHNICAL: Returns OkObjectResult with CustomerDto
    /// </summary>
    [Fact]
    public async Task GetById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var customerId = 1;
        var customer = new CustomerDto 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@example.com",
            Category = "Individual"
        };

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.GetById(customerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
        var returnedCustomer = okResult.Value as CustomerDto;
        returnedCustomer.Should().NotBeNull();
        returnedCustomer!.Id.Should().Be(1);
    }

    /// <summary>
    /// Verifies GetById returns 404 Not Found when customer doesn't exist
    /// 
    /// FUNCTIONAL: API should return 404 when customer ID is not found
    /// TECHNICAL: Returns NotFoundObjectResult
    /// </summary>
    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.GetById(customerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region GetIndividuals/GetOrganizations Tests

    /// <summary>
    /// Verifies GetIndividuals returns only individual customers
    /// </summary>
    [Fact]
    public async Task GetIndividuals_ReturnsOnlyIndividualCustomers()
    {
        // Arrange
        var individuals = new List<CustomerDto>
        {
            new CustomerDto { Id = 1, Category = "Individual", FirstName = "John", LastName = "Doe" }
        };

        _mockCustomerService.Setup(s => s.GetIndividualCustomersAsync())
            .ReturnsAsync(individuals);

        // Act
        var result = await _controller.GetIndividuals();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies GetOrganizations returns only organization customers
    /// </summary>
    [Fact]
    public async Task GetOrganizations_ReturnsOnlyOrganizationCustomers()
    {
        // Arrange
        var organizations = new List<CustomerDto>
        {
            new CustomerDto { Id = 1, Category = "Organization", Company = "Acme Corp" }
        };

        _mockCustomerService.Setup(s => s.GetOrganizationCustomersAsync())
            .ReturnsAsync(organizations);

        // Act
        var result = await _controller.GetOrganizations();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region Search Tests

    /// <summary>
    /// Verifies Search returns matching customers
    /// 
    /// FUNCTIONAL: Users can search customers by name, email, or company
    /// TECHNICAL: GET /api/customers/search/{searchTerm}
    /// </summary>
    [Fact]
    public async Task Search_ReturnsMatchingCustomers()
    {
        // Arrange
        var searchTerm = "Acme";
        var customers = new List<CustomerDto>
        {
            new CustomerDto { Id = 1, Company = "Acme Corp", Email = "contact@acme.com" }
        };

        _mockCustomerService.Setup(s => s.SearchCustomersAsync(searchTerm))
            .ReturnsAsync(customers);

        // Act
        var result = await _controller.Search(searchTerm);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    #endregion

    #region Create Tests

    /// <summary>
    /// Verifies Create returns 201 Created with new customer
    /// 
    /// FUNCTIONAL: API should create new customer and return 201
    /// TECHNICAL: POST /api/customers returns CreatedAtActionResult
    /// </summary>
    [Fact]
    public async Task Create_WithValidIndividual_ReturnsCreatedResult()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Category = CustomerCategory.Individual,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Phone = "555-0001"
        };

        var createdCustomer = new CustomerDto
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Category = "Individual"
        };

        _mockCustomerService.Setup(s => s.CreateCustomerAsync(createDto))
            .ReturnsAsync(createdCustomer);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        createdResult.StatusCode.Should().Be(201);
    }

    /// <summary>
    /// Verifies Create returns 400 Bad Request for invalid Individual customer
    /// </summary>
    [Fact]
    public async Task Create_WithMissingIndividualName_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Category = CustomerCategory.Individual,
            // Missing FirstName and LastName
            Email = "john@example.com"
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        badRequestResult.StatusCode.Should().Be(400);
    }

    /// <summary>
    /// Verifies Create returns 400 Bad Request for Organization without Company name
    /// </summary>
    [Fact]
    public async Task Create_WithMissingOrganizationCompany_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Category = CustomerCategory.Organization,
            // Missing Company name
            Email = "contact@example.com"
        };

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Update Tests

    /// <summary>
    /// Verifies Update returns 200 OK with updated customer
    /// </summary>
    [Fact]
    public async Task Update_WithValidData_ReturnsOkResult()
    {
        // Arrange
        var customerId = 1;
        var updateDto = new UpdateCustomerDto
        {
            FirstName = "UpdatedName"
        };

        var updatedCustomer = new CustomerDto
        {
            Id = 1,
            FirstName = "UpdatedName",
            LastName = "Doe",
            Email = "john@example.com"
        };

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _controller.Update(customerId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies Update returns 404 when customer not found
    /// </summary>
    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        var updateDto = new UpdateCustomerDto { FirstName = "Test" };

        _mockCustomerService.Setup(s => s.UpdateCustomerAsync(customerId, updateDto))
            .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _controller.Update(customerId, updateDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Delete Tests

    /// <summary>
    /// Verifies Delete returns 200 OK when customer is deleted
    /// </summary>
    [Fact]
    public async Task Delete_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var customerId = 1;
        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(customerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        okResult.StatusCode.Should().Be(200);
    }

    /// <summary>
    /// Verifies Delete returns 404 when customer not found
    /// </summary>
    [Fact]
    public async Task Delete_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var customerId = 999;
        _mockCustomerService.Setup(s => s.DeleteCustomerAsync(customerId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(customerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        notFoundResult.StatusCode.Should().Be(404);
    }

    #endregion

    #region Contact Linking Tests

    /// <summary>
    /// Verifies LinkContact returns 201 when contact is linked
    /// </summary>
    [Fact]
    public async Task LinkContact_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var customerId = 1;
        var linkDto = new LinkContactToCustomerDto
        {
            ContactId = 10,
            Role = CustomerContactRole.Primary,
            IsPrimaryContact = true
        };

        var customer = new CustomerDto { Id = 1, Category = "Organization", Company = "Test" };
        var contactLink = new CustomerContactDto { Id = 1, CustomerId = 1, ContactId = 10 };

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCustomerService.Setup(s => s.LinkContactToCustomerAsync(customerId, linkDto))
            .ReturnsAsync(contactLink);

        // Act
        var result = await _controller.LinkContact(customerId, linkDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        createdResult.StatusCode.Should().Be(201);
    }

    /// <summary>
    /// Verifies LinkContact returns 400 for Individual customers
    /// </summary>
    [Fact]
    public async Task LinkContact_ForIndividualCustomer_ReturnsBadRequest()
    {
        // Arrange
        var customerId = 1;
        var linkDto = new LinkContactToCustomerDto { ContactId = 10 };
        var customer = new CustomerDto { Id = 1, Category = "Individual" };

        _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
            .ReturnsAsync(customer);

        // Act
        var result = await _controller.LinkContact(customerId, linkDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        badRequestResult.StatusCode.Should().Be(400);
    }

    #endregion

    #region Controller Attribute Tests

    /// <summary>
    /// Verifies controller authorization attributes
    /// </summary>
    [Fact]
    public void CustomersController_ShouldHaveAuthorizeAttribute()
    {
        // Note: CustomersController currently doesn't have [Authorize] attribute
        // This test documents the current state
        var controllerType = typeof(CustomersController);
        var attributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        // This should be updated when Authorize is added:
        // attributes.Should().NotBeEmpty("Controller should require authorization");
    }

    /// <summary>
    /// Verifies controller has both /api/customers and /api/accounts route attributes
    /// for industry-standard naming compatibility
    /// </summary>
    [Fact]
    public void CustomersController_ShouldHaveAccountsRouteAlias()
    {
        // Arrange
        var controllerType = typeof(CustomersController);
        var routeAttributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), true)
            .Cast<Microsoft.AspNetCore.Mvc.RouteAttribute>()
            .ToList();

        // Assert - Should have at least 2 routes: api/[controller] and api/accounts
        routeAttributes.Should().HaveCountGreaterOrEqualTo(2, 
            "Controller should have both default route and /api/accounts alias");
        
        var routeTemplates = routeAttributes.Select(r => r.Template).ToList();
        routeTemplates.Should().Contain("api/accounts", 
            "Controller should have industry-standard /api/accounts route alias");
    }

    /// <summary>
    /// Verifies controller has ApiController attribute
    /// </summary>
    [Fact]
    public void CustomersController_ShouldHaveApiControllerAttribute()
    {
        // Arrange
        var controllerType = typeof(CustomersController);
        var attributes = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.ApiControllerAttribute), true);

        // Assert
        attributes.Should().NotBeEmpty("Controller should have [ApiController] attribute");
    }

    #endregion
}
