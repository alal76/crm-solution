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
    public class DepartmentsControllerTests
    {
        private readonly Mock<IDepartmentService> _mockDepartmentService;
        private readonly DepartmentsController _controller;

        public DepartmentsControllerTests()
        {
            _mockDepartmentService = new Mock<IDepartmentService>();
            _controller = new DepartmentsController(_mockDepartmentService.Object, null!);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithDepartments()
        {
            // Arrange
            var departments = new List<object>
            {
                new { Id = 1, Name = "Sales", Code = "SAL" },
                new { Id = 2, Name = "Engineering", Code = "ENG" }
            };

            _mockDepartmentService.Setup(s => s.GetAllDepartmentsAsync())
                .ReturnsAsync(departments);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            _mockDepartmentService.Verify(s => s.GetAllDepartmentsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var departmentId = 1;
            var department = new { Id = 1, Name = "Sales", Code = "SAL" };

            _mockDepartmentService.Setup(s => s.GetDepartmentByIdAsync(departmentId))
                .ReturnsAsync(department);

            // Act
            var result = await _controller.GetById(departmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            _mockDepartmentService.Verify(s => s.GetDepartmentByIdAsync(departmentId), Times.Once);
        }

        [Fact]
        public async Task Create_WithValidDepartment_ReturnsCreatedResult()
        {
            // Arrange
            var departmentRequest = new { Name = "HR", Code = "HR" };
            var createdDepartment = new { Id = 1, Name = "HR", Code = "HR" };

            _mockDepartmentService.Setup(s => s.CreateDepartmentAsync(It.IsAny<object>()))
                .ReturnsAsync(createdDepartment);

            // Act
            var result = await _controller.Create(departmentRequest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            createdResult.StatusCode.Should().Be(201);
            _mockDepartmentService.Verify(s => s.CreateDepartmentAsync(It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task Delete_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var departmentId = 1;
            _mockDepartmentService.Setup(s => s.DeleteDepartmentAsync(departmentId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(departmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            _mockDepartmentService.Verify(s => s.DeleteDepartmentAsync(departmentId), Times.Once);
        }

        [Fact]
        public async Task GetAll_WhenServiceThrowsException_ReturnsBadRequest()
        {
            // Arrange
            _mockDepartmentService.Setup(s => s.GetAllDepartmentsAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAll());
        }
    }
}
