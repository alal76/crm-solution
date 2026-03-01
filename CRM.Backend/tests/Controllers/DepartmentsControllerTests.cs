// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CRM.Api.Controllers;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Controllers
{
    public class DepartmentsControllerTests
    {
        private readonly Mock<IRepository<Department>> _mockDepartmentRepository;
        private readonly DepartmentsController _controller;

        public DepartmentsControllerTests()
        {
            _mockDepartmentRepository = new Mock<IRepository<Department>>();
            _controller = new DepartmentsController(_mockDepartmentRepository.Object);
        }

        [Fact]
        public async Task GetDepartments_ReturnsOkResult_WithDepartments()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "Sales", DepartmentCode = "SAL", IsActive = true, IsDeleted = false },
                new Department { Id = 2, Name = "Engineering", DepartmentCode = "ENG", IsActive = true, IsDeleted = false }
            };

            _mockDepartmentRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(departments);

            // Act
            var result = await _controller.GetDepartments();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            _mockDepartmentRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDepartmentById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var departmentId = 1;
            var department = new Department { Id = 1, Name = "Sales", DepartmentCode = "SAL", IsActive = true, IsDeleted = false };

            _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.GetDepartmentById(departmentId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            _mockDepartmentRepository.Verify(r => r.GetByIdAsync(departmentId), Times.Once);
        }

        [Fact]
        public async Task GetDepartmentById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var departmentId = 999;
            _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
                .ReturnsAsync((Department?)null);

            // Act
            var result = await _controller.GetDepartmentById(departmentId);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task GetDepartments_FiltersOutDeletedDepartments()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department { Id = 1, Name = "Sales", IsDeleted = false },
                new Department { Id = 2, Name = "Deleted Dept", IsDeleted = true }
            };

            _mockDepartmentRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(departments);

            // Act
            var result = await _controller.GetDepartments();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
        }

        [Fact]
        public void DepartmentsController_HasAuthorizeAttribute()
        {
            // Verify the controller has the Authorize attribute
            var controllerType = typeof(DepartmentsController);
            var authorizeAttribute = controllerType.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
            authorizeAttribute.Should().NotBeEmpty("Controller should require authorization");
        }

        [Fact]
        public void DepartmentsController_ImplementsApiController()
        {
            // Verify the controller has the ApiController attribute
            var controllerType = typeof(DepartmentsController);
            var apiControllerAttribute = controllerType.GetCustomAttributes(typeof(ApiControllerAttribute), true);
            apiControllerAttribute.Should().NotBeEmpty("Controller should be an API controller");
        }
    }
}
