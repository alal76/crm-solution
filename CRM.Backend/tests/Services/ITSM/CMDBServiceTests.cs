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

using CRM.Core.DTOs.ITSM;
using CRM.Core.Entities.ITSM;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.ITSM;
using CRM.Infrastructure.Data;
using CRM.Infrastructure.Services.ITSM;
using CRM.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CRM.Tests.Services.ITSM;

public class CMDBServiceTests
{
    private readonly Mock<IDbContextResolver> _mockResolver;
    private readonly Mock<ICrmDbContext> _mockContext;
    private readonly Mock<ILogger<CMDBService>> _mockLogger;
    private readonly ICMDBService _service;

    public CMDBServiceTests()
    {
        _mockResolver = new Mock<IDbContextResolver>();
        _mockContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<CMDBService>>();

        _mockResolver.Setup(r => r.ResolveContext()).Returns(_mockContext.Object);
        _service = new CMDBService(_mockResolver.Object, _mockLogger.Object);
    }

    // ========================================================================
    // CreateCIAsync
    // ========================================================================

    [Fact]
    public async Task CreateCIAsync_ShouldCreateCI_WhenValidDtoProvided()
    {
        // Arrange
        var items = new List<ConfigurationItem>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(items);
        mockSet.Setup(m => m.Add(It.IsAny<ConfigurationItem>())).Callback<ConfigurationItem>(e => items.Add(e));
        _mockContext.Setup(c => c.ConfigurationItems).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateCIDto
        {
            CIName = "Web Server 01",
            CIType = CIType.Server,
            Description = "Primary web server",
            OperationalStatus = OperationalStatus.Operational
        };

        // Act
        var result = await _service.CreateCIAsync(dto, createdById: 1);

        // Assert
        result.Should().NotBeNull();
        result.CIName.Should().Be("Web Server 01");
        result.CIType.Should().Be(CIType.Server);
        mockSet.Verify(m => m.Add(It.IsAny<ConfigurationItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateCIAsync_ShouldGenerateCINumber()
    {
        // Arrange
        var items = new List<ConfigurationItem>();
        var mockSet = MockDbSetFactory.CreateMockDbSet(items);
        mockSet.Setup(m => m.Add(It.IsAny<ConfigurationItem>())).Callback<ConfigurationItem>(e => items.Add(e));
        _mockContext.Setup(c => c.ConfigurationItems).Returns(mockSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateCIDto
        {
            CIName = "Database Server",
            CIType = CIType.Server,
            OperationalStatus = OperationalStatus.Operational
        };

        // Act
        var result = await _service.CreateCIAsync(dto, createdById: 1);

        // Assert
        result.CINumber.Should().NotBeNullOrEmpty();
    }

    // ========================================================================
    // GetCIByIdAsync
    // ========================================================================

    [Fact]
    public async Task GetCIByIdAsync_ShouldReturnCI_WhenExists()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new()
            {
                CIId = 1, CIName = "App Server", CINumber = "CI0001",
                CIType = CIType.Server, OperationalStatus = OperationalStatus.Operational,
                CreatedAt = DateTime.UtcNow, IsDeleted = false
            }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.GetCIByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CIName.Should().Be("App Server");
    }

    [Fact]
    public async Task GetCIByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(new List<ConfigurationItem>()).Object);

        // Act
        var result = await _service.GetCIByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCIByIdAsync_ShouldReturnNull_WhenSoftDeleted()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new()
            {
                CIId = 1, CIName = "Deleted CI", CINumber = "CI0001",
                CIType = CIType.Server, OperationalStatus = OperationalStatus.Retired,
                CreatedAt = DateTime.UtcNow, IsDeleted = true
            }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var result = await _service.GetCIByIdAsync(1);

        // Assert
        result.Should().BeNull();
    }

    // ========================================================================
    // SearchCIsAsync
    // ========================================================================

    [Fact]
    public async Task SearchCIsAsync_ShouldReturnMatchingCIs_WhenSearchTermProvided()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new() { CIId = 1, CIName = "Web Server 01", CINumber = "CI0001", CIType = CIType.Server, CreatedAt = DateTime.UtcNow },
            new() { CIId = 2, CIName = "Database Server 01", CINumber = "CI0002", CIType = CIType.Server, CreatedAt = DateTime.UtcNow },
            new() { CIId = 3, CIName = "Office Laptop 01", CINumber = "CI0003", CIType = CIType.WorkStation, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var results = await _service.SearchCIsAsync("Server", type: null, pageNumber: 1, pageSize: 20);

        // Assert
        results.Should().NotBeNull();
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchCIsAsync_ShouldFilterByType_WhenTypeProvided()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new() { CIId = 1, CIName = "Server 1", CINumber = "CI0001", CIType = CIType.Server, CreatedAt = DateTime.UtcNow },
            new() { CIId = 2, CIName = "Laptop 1", CINumber = "CI0002", CIType = CIType.WorkStation, CreatedAt = DateTime.UtcNow },
            new() { CIId = 3, CIName = "Server 2", CINumber = "CI0003", CIType = CIType.Server, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);

        // Act
        var results = await _service.SearchCIsAsync("", type: CIType.Server, pageNumber: 1, pageSize: 20);

        // Assert
        results.Should().HaveCount(2);
    }

    // ========================================================================
    // UpdateCIAsync
    // ========================================================================

    [Fact]
    public async Task UpdateCIAsync_ShouldUpdateFields_WhenCIExists()
    {
        // Arrange
        var ci = new ConfigurationItem
        {
            CIId = 1, CIName = "Old Name", CINumber = "CI0001",
            CIType = CIType.Server, OperationalStatus = OperationalStatus.Operational,
            CreatedAt = DateTime.UtcNow
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(new List<ConfigurationItem> { ci }).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var dto = new CreateCIDto
        {
            CIName = "New Name",
            CIType = CIType.Server,
            Description = "Updated description",
            OperationalStatus = OperationalStatus.UnderRepair
        };

        // Act
        var result = await _service.UpdateCIAsync(1, dto, modifiedById: 2);

        // Assert
        result.Should().NotBeNull();
        result.CIName.Should().Be("New Name");
    }

    // ========================================================================
    // CreateRelationshipAsync
    // ========================================================================

    [Fact]
    public async Task CreateRelationshipAsync_ShouldCreateLink_WhenBothCIsExist()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new() { CIId = 1, CIName = "Parent Server", CINumber = "CI0001", CIType = CIType.Server, CreatedAt = DateTime.UtcNow },
            new() { CIId = 2, CIName = "Child App", CINumber = "CI0002", CIType = CIType.Application, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);
        _mockContext.Setup(c => c.CIRelationships).Returns(MockDbSetFactory.CreateMockDbSet(new List<CIRelationship>()).Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _service.CreateRelationshipAsync(1, 2, RelationshipType.RunsOn, createdById: 1);

        // Assert
        result.Should().BeTrue();
    }

    // ========================================================================
    // GetRelatedCIsAsync
    // ========================================================================

    [Fact]
    public async Task GetRelatedCIsAsync_ShouldReturnRelatedItems()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new() { CIId = 1, CIName = "Server", CINumber = "CI0001", CIType = CIType.Server, CreatedAt = DateTime.UtcNow },
            new() { CIId = 2, CIName = "App", CINumber = "CI0002", CIType = CIType.Application, CreatedAt = DateTime.UtcNow }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);
        _mockContext.Setup(c => c.CIRelationships).Returns(MockDbSetFactory.CreateMockDbSet(new List<CIRelationship>()).Object);

        // Act
        var result = await _service.GetRelatedCIsAsync(1);

        // Assert
        result.Should().NotBeNull();
    }

    // ========================================================================
    // GetImpactAnalysisAsync
    // ========================================================================

    [Fact]
    public async Task GetImpactAnalysisAsync_ShouldReturnImpactedServices()
    {
        // Arrange
        var items = new List<ConfigurationItem>
        {
            new()
            {
                CIId = 1, CIName = "Core Database", CINumber = "CI0001",
                CIType = CIType.Server, OperationalStatus = OperationalStatus.Operational,
                CreatedAt = DateTime.UtcNow
            }
        };
        _mockContext.Setup(c => c.ConfigurationItems).Returns(MockDbSetFactory.CreateMockDbSet(items).Object);
        _mockContext.Setup(c => c.CIRelationships).Returns(MockDbSetFactory.CreateMockDbSet(new List<CIRelationship>()).Object);
        _mockContext.Setup(c => c.ServiceCIs).Returns(MockDbSetFactory.CreateMockDbSet(new List<ServiceCI>()).Object);
        _mockContext.Setup(c => c.Incidents).Returns(MockDbSetFactory.CreateMockDbSet(new List<Incident>()).Object);

        // Act
        var result = await _service.GetImpactAnalysisAsync(1);

        // Assert
        result.Should().NotBeNull();
    }
}
