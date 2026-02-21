// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services;

public class DepartmentServiceTests
{
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<DepartmentService>> _mockLogger;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<DepartmentService>>();
        _service = new DepartmentService(_mockContext.Object, _mockLogger.Object);
    }

    private void SetupDepartments(List<Department>? departments = null)
    {
        departments ??= new List<Department>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(departments);
        _mockContext.Setup(c => c.Departments).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static Department CreateDepartment(int id, string name, bool isActive = true, int? parentId = null, bool isDeleted = false)
    {
        return new Department
        {
            Id = id,
            Name = name,
            IsActive = isActive,
            ParentDepartmentId = parentId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-id),
            IsDeleted = isDeleted
        };
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByIsActive()
    {
        var departments = new List<Department>
        {
            CreateDepartment(1, "Sales", isActive: true),
            CreateDepartment(2, "Ops", isActive: false)
        };
        SetupDepartments(departments);

        var result = await _service.GetAllAsync(isActive: true);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Sales");
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByParent()
    {
        var departments = new List<Department>
        {
            CreateDepartment(1, "Parent"),
            CreateDepartment(2, "Child", parentId: 1),
            CreateDepartment(3, "Other", parentId: 2)
        };
        SetupDepartments(departments);

        var result = await _service.GetAllAsync(parentDepartmentId: 1);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Child");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDepartment_WhenExists()
    {
        SetupDepartments(new List<Department> { CreateDepartment(1, "Sales") });

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Sales");
    }

    [Fact]
    public async Task GetByCodeAsync_ShouldReturnDepartment_WhenExists()
    {
        var dept = CreateDepartment(1, "Sales");
        dept.DepartmentCode = "SAL";
        SetupDepartments(new List<Department> { dept });

        var result = await _service.GetByCodeAsync("SAL");

        result.Should().NotBeNull();
        result!.DepartmentCode.Should().Be("SAL");
    }

    [Fact]
    public async Task CreateAsync_ShouldSetDefaultsAndPersist()
    {
        var departments = new List<Department>();
        SetupDepartments(departments);
        var dept = new Department { Name = "New Dept" };

        var result = await _service.CreateAsync(dept);

        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().NotBe(default);
        result.UpdatedAt.Should().NotBe(default);
        departments.Should().Contain(dept);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenMissing()
    {
        SetupDepartments(new List<Department>());

        var result = await _service.UpdateAsync(1, new Department { Name = "Updated" });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WhenExists()
    {
        var existing = CreateDepartment(1, "Sales", isActive: true, parentId: null);
        SetupDepartments(new List<Department> { existing });

        var updated = new Department
        {
            Name = "Sales & Marketing",
            Description = "Desc",
            DepartmentCode = "SM",
            IsActive = false,
            ParentDepartmentId = 2
        };

        var result = await _service.UpdateAsync(1, updated);

        result.Should().BeTrue();
        existing.Name.Should().Be("Sales & Marketing");
        existing.Description.Should().Be("Desc");
        existing.DepartmentCode.Should().Be("SM");
        existing.IsActive.Should().BeFalse();
        existing.ParentDepartmentId.Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteDepartment()
    {
        var existing = CreateDepartment(1, "Sales");
        SetupDepartments(new List<Department> { existing });

        var result = await _service.DeleteAsync(1);

        result.Should().BeTrue();
        existing.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetSubDepartmentsAsync_ShouldReturnChildren()
    {
        var departments = new List<Department>
        {
            CreateDepartment(1, "Parent"),
            CreateDepartment(2, "Child", parentId: 1),
            CreateDepartment(3, "Other", parentId: 2)
        };
        SetupDepartments(departments);

        var result = await _service.GetSubDepartmentsAsync(1);

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Child");
    }

    [Fact]
    public async Task GetHierarchyAsync_ShouldReturnAllNonDeleted()
    {
        var departments = new List<Department>
        {
            CreateDepartment(1, "Active"),
            CreateDepartment(2, "Deleted", isDeleted: true)
        };
        SetupDepartments(departments);

        var result = await _service.GetHierarchyAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }
}
