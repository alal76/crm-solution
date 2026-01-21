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
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for CustomerService
/// 
/// FUNCTIONAL VIEW:
/// Tests cover all customer management business operations including:
/// - Individual and Organization customer creation and retrieval
/// - Customer search and filtering by category
/// - Contact linking for organization customers
/// - Soft delete operations
/// 
/// TECHNICAL VIEW:
/// - Uses Moq to mock repository dependencies
/// - Tests service layer in isolation from database
/// - Validates DTO mapping and business logic
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<IRepository<Customer>> _mockCustomerRepo;
    private readonly Mock<IRepository<CustomerContact>> _mockCustomerContactRepo;
    private readonly Mock<IContactsService> _mockContactsService;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockCustomerRepo = new Mock<IRepository<Customer>>();
        _mockCustomerContactRepo = new Mock<IRepository<CustomerContact>>();
        _mockContactsService = new Mock<IContactsService>();
        
        _service = new CustomerService(
            _mockCustomerRepo.Object,
            _mockCustomerContactRepo.Object,
            _mockContactsService.Object);
    }

    #region GetCustomerByIdAsync Tests

    /// <summary>
    /// FUNCTIONAL: Verifies that a customer can be retrieved by their unique ID
    /// TECHNICAL: Tests GetByIdAsync repository call and DTO mapping
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerExists_ReturnsCustomerDto()
    {
        // Arrange
        var customerId = 1;
        var customer = CreateTestCustomer(customerId, CustomerCategory.Individual);
        
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(customer);
        _mockCustomerContactRepo.Setup(r => r.FindAsync(It.IsAny<Func<CustomerContact, bool>>()))
            .ReturnsAsync(new List<CustomerContact>());

        // Act
        var result = await _service.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(customerId);
        result.FirstName.Should().Be("John");
        result.Category.Should().Be("Individual");
    }

    /// <summary>
    /// FUNCTIONAL: Verifies that requesting a non-existent customer returns null
    /// TECHNICAL: Tests null handling in repository response
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerNotFound_ReturnsNull()
    {
        // Arrange
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.GetCustomerByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// FUNCTIONAL: Deleted customers should not be retrievable
    /// TECHNICAL: Tests IsDeleted flag handling
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_WhenCustomerIsDeleted_ReturnsNull()
    {
        // Arrange
        var customer = CreateTestCustomer(1, CustomerCategory.Individual);
        customer.IsDeleted = true;
        
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(customer);

        // Act
        var result = await _service.GetCustomerByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllCustomersAsync Tests

    /// <summary>
    /// FUNCTIONAL: Retrieve all active customers, excluding deleted ones
    /// TECHNICAL: Tests filtering of IsDeleted flag in GetAllAsync
    /// </summary>
    [Fact]
    public async Task GetAllCustomersAsync_ReturnsOnlyActiveCustomers()
    {
        // Arrange
        var deletedCustomer = CreateTestCustomer(3, CustomerCategory.Individual);
        deletedCustomer.IsDeleted = true;
        
        var customers = new List<Customer>
        {
            CreateTestCustomer(1, CustomerCategory.Individual),
            CreateTestCustomer(2, CustomerCategory.Organization),
            deletedCustomer
        };
        
        _mockCustomerRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(customers);
        _mockCustomerContactRepo.Setup(r => r.FindAsync(It.IsAny<Func<CustomerContact, bool>>()))
            .ReturnsAsync(new List<CustomerContact>());

        // Act
        var result = await _service.GetAllCustomersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Id != 3).Should().BeTrue();
    }

    #endregion

    #region CreateCustomerAsync Tests

    /// <summary>
    /// FUNCTIONAL: Create a new individual customer with required fields
    /// TECHNICAL: Tests AddAsync and SaveAsync repository calls
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_WithValidIndividual_ReturnsCreatedCustomer()
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
        
        _mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);
        _mockCustomerRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Category.Should().Be("Individual");
        
        _mockCustomerRepo.Verify(r => r.AddAsync(It.IsAny<Customer>()), Times.Once);
        _mockCustomerRepo.Verify(r => r.SaveAsync(), Times.Once);
    }

    /// <summary>
    /// FUNCTIONAL: Create a new organization customer with company name
    /// TECHNICAL: Validates organization-specific field requirements
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_WithValidOrganization_ReturnsCreatedCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Category = CustomerCategory.Organization,
            Company = "Acme Corporation",
            Email = "contact@acme.com",
            Phone = "555-0002"
        };
        
        _mockCustomerRepo.Setup(r => r.AddAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);
        _mockCustomerRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Company.Should().Be("Acme Corporation");
        result.Category.Should().Be("Organization");
    }

    #endregion

    #region UpdateCustomerAsync Tests

    /// <summary>
    /// FUNCTIONAL: Update an existing customer's information
    /// TECHNICAL: Tests GetByIdAsync, property update, and SaveAsync
    /// </summary>
    [Fact]
    public async Task UpdateCustomerAsync_WithValidData_ReturnsUpdatedCustomer()
    {
        // Arrange
        var existingCustomer = CreateTestCustomer(1, CustomerCategory.Individual);
        var updateDto = new UpdateCustomerDto { FirstName = "UpdatedName" };
        
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingCustomer);
        _mockCustomerRepo.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);
        _mockCustomerRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);
        _mockCustomerContactRepo.Setup(r => r.FindAsync(It.IsAny<Func<CustomerContact, bool>>()))
            .ReturnsAsync(new List<CustomerContact>());

        // Act
        var result = await _service.UpdateCustomerAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("UpdatedName");
    }

    /// <summary>
    /// FUNCTIONAL: Updating a non-existent customer should return null
    /// TECHNICAL: Tests null customer handling
    /// </summary>
    [Fact]
    public async Task UpdateCustomerAsync_WhenCustomerNotFound_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateCustomerDto { FirstName = "Test" };
        
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.UpdateCustomerAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteCustomerAsync Tests

    /// <summary>
    /// FUNCTIONAL: Soft delete a customer (sets IsDeleted flag)
    /// TECHNICAL: Tests UpdateAsync with IsDeleted = true
    /// </summary>
    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerExists_ReturnsTrue()
    {
        // Arrange
        var customer = CreateTestCustomer(1, CustomerCategory.Individual);
        
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(customer);
        _mockCustomerRepo.Setup(r => r.UpdateAsync(It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);
        _mockCustomerRepo.Setup(r => r.SaveAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteCustomerAsync(1);

        // Assert
        result.Should().BeTrue();
        _mockCustomerRepo.Verify(r => r.UpdateAsync(It.Is<Customer>(c => c.IsDeleted == true)), Times.Once);
    }

    /// <summary>
    /// FUNCTIONAL: Deleting a non-existent customer should return false
    /// TECHNICAL: Tests null customer handling in delete operation
    /// </summary>
    [Fact]
    public async Task DeleteCustomerAsync_WhenCustomerNotFound_ReturnsFalse()
    {
        // Arrange
        _mockCustomerRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var result = await _service.DeleteCustomerAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Category Filtering Tests

    /// <summary>
    /// FUNCTIONAL: Filter customers to get only individuals
    /// TECHNICAL: Tests CustomerCategory.Individual filtering
    /// </summary>
    [Fact]
    public async Task GetIndividualCustomersAsync_ReturnsOnlyIndividuals()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CreateTestCustomer(1, CustomerCategory.Individual),
            CreateTestCustomer(2, CustomerCategory.Organization),
            CreateTestCustomer(3, CustomerCategory.Individual)
        };
        
        _mockCustomerRepo.Setup(r => r.FindAsync(It.IsAny<Func<Customer, bool>>()))
            .ReturnsAsync(customers.Where(c => c.Category == CustomerCategory.Individual).ToList());
        _mockCustomerContactRepo.Setup(r => r.FindAsync(It.IsAny<Func<CustomerContact, bool>>()))
            .ReturnsAsync(new List<CustomerContact>());

        // Act
        var result = await _service.GetIndividualCustomersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Category == "Individual").Should().BeTrue();
    }

    /// <summary>
    /// FUNCTIONAL: Filter customers to get only organizations
    /// TECHNICAL: Tests CustomerCategory.Organization filtering
    /// </summary>
    [Fact]
    public async Task GetOrganizationCustomersAsync_ReturnsOnlyOrganizations()
    {
        // Arrange
        var customers = new List<Customer>
        {
            CreateTestCustomer(1, CustomerCategory.Individual),
            CreateTestCustomer(2, CustomerCategory.Organization),
            CreateTestCustomer(3, CustomerCategory.Organization)
        };
        
        _mockCustomerRepo.Setup(r => r.FindAsync(It.IsAny<Func<Customer, bool>>()))
            .ReturnsAsync(customers.Where(c => c.Category == CustomerCategory.Organization).ToList());
        _mockCustomerContactRepo.Setup(r => r.FindAsync(It.IsAny<Func<CustomerContact, bool>>()))
            .ReturnsAsync(new List<CustomerContact>());

        // Act
        var result = await _service.GetOrganizationCustomersAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.Category == "Organization").Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test customer entity with default values
    /// </summary>
    private Customer CreateTestCustomer(int id, CustomerCategory category)
    {
        return new Customer
        {
            Id = id,
            Category = category,
            FirstName = "John",
            LastName = "Doe",
            Company = category == CustomerCategory.Organization ? "Test Company" : "",
            Email = $"customer{id}@example.com",
            Phone = "555-0001",
            Address = "123 Main St",
            City = "Anytown",
            State = "CA",
            ZipCode = "12345",
            Country = "USA",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
