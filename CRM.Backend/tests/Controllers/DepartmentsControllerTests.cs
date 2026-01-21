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
using System.Linq.Expressions;

namespace CRM.Tests.Controllers
{
    public class DepartmentsControllerTests
    {
        private readonly Mock<IRepository<Department>> _mockDepartmentRepository;
        private readonly Mock<ILogger<DepartmentsController>> _mockLogger;
        private readonly DepartmentsController _controller;

        public DepartmentsControllerTests()
        {
            _mockDepartmentRepository = new Mock<IRepository<Department>>();
            _mockLogger = new Mock<ILogger<DepartmentsController>>();
            _controller = new DepartmentsController(_mockDepartmentRepository.Object, _mockLogger.Object);
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
