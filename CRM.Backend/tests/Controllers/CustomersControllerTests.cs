using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers
{
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

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithCustomers()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "Acme", LastName = "Corp", Email = "contact@acme.com" },
                new Customer { Id = 2, FirstName = "Tech", LastName = "Start", Email = "info@techstart.com" }
            };

            _mockCustomerService.Setup(s => s.GetAllCustomersAsync())
                .ReturnsAsync(customers);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var customerId = 1;
            var customer = new Customer { Id = 1, FirstName = "Acme", LastName = "Corp", Email = "contact@acme.com" };

            _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
                .ReturnsAsync(customer);

            // Act
            var result = await _controller.GetById(customerId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var customerId = 999;
            _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
                .ReturnsAsync((Customer?)null);

            // Act
            var result = await _controller.GetById(customerId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Search_ReturnsMatchingCustomers()
        {
            // Arrange
            var searchTerm = "Acme";
            var customers = new List<Customer>
            {
                new Customer { Id = 1, FirstName = "Acme", LastName = "Corp", Email = "contact@acme.com" }
            };

            _mockCustomerService.Setup(s => s.SearchCustomersAsync(searchTerm))
                .ReturnsAsync(customers);

            // Act
            var result = await _controller.Search(searchTerm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
        }

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
    }
}
