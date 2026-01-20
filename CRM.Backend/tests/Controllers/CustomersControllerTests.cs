using Xunit;
using Moq;
using FluentAssertions;
using CRM.Api.Controllers;
using CRM.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM.Tests.Controllers
{
    public class CustomersControllerTests
    {
        private readonly Mock<ICustomerService> _mockCustomerService;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _mockCustomerService = new Mock<ICustomerService>();
            _controller = new CustomersController(_mockCustomerService.Object, null!);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithCustomers()
        {
            // Arrange
            var customers = new List<object>
            {
                new { Id = 1, Name = "Acme Corp", Email = "contact@acme.com" },
                new { Id = 2, Name = "TechStart Inc", Email = "info@techstart.com" }
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
        public async Task Create_WithValidCustomer_ReturnsCreatedResult()
        {
            // Arrange
            var customerRequest = new { Name = "New Corp", Email = "contact@newcorp.com" };
            var createdCustomer = new { Id = 1, Name = "New Corp", Email = "contact@newcorp.com" };

            _mockCustomerService.Setup(s => s.CreateCustomerAsync(It.IsAny<object>()))
                .ReturnsAsync(createdCustomer);

            // Act
            var result = await _controller.Create(customerRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            createdResult.StatusCode.Should().Be(201);
        }

        [Fact]
        public async Task GetById_WithInvalidId_ThrowsException()
        {
            // Arrange
            var customerId = 999;
            _mockCustomerService.Setup(s => s.GetCustomerByIdAsync(customerId))
                .ThrowsAsync(new Exception("Customer not found"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(customerId));
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsSuccess()
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
    }
}
