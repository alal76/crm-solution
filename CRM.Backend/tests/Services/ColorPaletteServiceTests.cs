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
using Microsoft.Extensions.Logging;
using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Infrastructure.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace CRM.Tests.Services;

/// <summary>
/// Unit tests for ColorPaletteService
/// Covers: Color palette CRUD, default palettes, validation
/// </summary>
public class ColorPaletteServiceTests
{
    private readonly Mock<IRepository<ColorPalette>> _mockRepository;
    private readonly Mock<ICrmDbContext> _mockDbContext;
    private readonly Mock<ILogger<ColorPaletteService>> _mockLogger;
    private readonly ColorPaletteService _service;

    public ColorPaletteServiceTests()
    {
        _mockRepository = new Mock<IRepository<ColorPalette>>();
        _mockDbContext = new Mock<ICrmDbContext>();
        _mockLogger = new Mock<ILogger<ColorPaletteService>>();

        _service = new ColorPaletteService(
            _mockRepository.Object,
            _mockDbContext.Object,
            _mockLogger.Object);
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllPalettes()
    {
        // Arrange
        var palettes = new List<ColorPalette>
        {
            new ColorPalette { Id = 1, Name = "Blue Theme", PrimaryColor = "#0066CC" },
            new ColorPalette { Id = 2, Name = "Green Theme", PrimaryColor = "#00CC66" }
        };

        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(palettes);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<ColorPalette>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActivePalettes()
    {
        // Arrange
        var palettes = new List<ColorPalette>
        {
            new ColorPalette { Id = 1, Name = "Active", IsActive = true },
            new ColorPalette { Id = 2, Name = "Inactive", IsActive = false }
        };

        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ColorPalette, bool>>>()))
            .ReturnsAsync(palettes.Where(p => p.IsActive).ToList());

        // Act
        var result = await _service.GetActiveAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().Be("Active");
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetByIdAsync_ExistingPalette_ReturnsPalette()
    {
        // Arrange
        var palette = new ColorPalette
        {
            Id = 1,
            Name = "Blue Theme",
            PrimaryColor = "#0066CC",
            SecondaryColor = "#003366"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Blue Theme");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingPalette_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ColorPalette?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateAsync_ValidPalette_ReturnsCreatedPalette()
    {
        // Arrange
        var createDto = new CreateColorPaletteDto
        {
            Name = "New Theme",
            PrimaryColor = "#FF0000",
            SecondaryColor = "#CC0000",
            AccentColor = "#FF3333"
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("New Theme");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsException()
    {
        // Arrange
        var createDto = new CreateColorPaletteDto { Name = "Existing Theme" };

        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ColorPalette, bool>>>()))
            .ReturnsAsync(new List<ColorPalette> { new ColorPalette { Name = "Existing Theme" } });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_InvalidHexColor_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateColorPaletteDto
        {
            Name = "Invalid Theme",
            PrimaryColor = "not-a-hex-color"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateAsync(createDto));
    }

    [Fact]
    public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateAsync(null!));
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateAsync_ValidPalette_ReturnsUpdatedPalette()
    {
        // Arrange
        var existingPalette = new ColorPalette
        {
            Id = 1,
            Name = "Old Name",
            PrimaryColor = "#0066CC"
        };

        var updateDto = new UpdateColorPaletteDto
        {
            Id = 1,
            Name = "New Name",
            PrimaryColor = "#FF0066"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingPalette);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => p);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.PrimaryColor.Should().Be("#FF0066");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingPalette_ReturnsNull()
    {
        // Arrange
        var updateDto = new UpdateColorPaletteDto { Id = 999 };

        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ColorPalette?)null);

        // Act
        var result = await _service.UpdateAsync(updateDto);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task DeleteAsync_ExistingPalette_ReturnsTrue()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, IsDefault = false };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        _mockRepository.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingPalette_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ColorPalette?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DefaultPalette_ThrowsException()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, IsDefault = true };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteAsync(1));
    }

    #endregion

    #region Default Palette Tests

    [Fact]
    public async Task GetDefaultAsync_ReturnsDefaultPalette()
    {
        // Arrange
        var defaultPalette = new ColorPalette
        {
            Id = 1,
            Name = "Default Theme",
            IsDefault = true
        };

        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ColorPalette, bool>>>()))
            .ReturnsAsync(new List<ColorPalette> { defaultPalette });

        // Act
        var result = await _service.GetDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task SetAsDefaultAsync_ValidPalette_SetsAsDefault()
    {
        // Arrange
        var currentDefault = new ColorPalette { Id = 1, IsDefault = true };
        var newDefault = new ColorPalette { Id = 2, IsDefault = false };

        _mockRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ColorPalette, bool>>>()))
            .ReturnsAsync(new List<ColorPalette> { currentDefault });

        _mockRepository.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(newDefault);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => p);

        // Act
        var result = await _service.SetAsDefaultAsync(2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SetAsDefaultAsync_NonExistingPalette_ReturnsFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ColorPalette?)null);

        // Act
        var result = await _service.SetAsDefaultAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Activation Tests

    [Fact]
    public async Task ActivateAsync_InactivePalette_ActivatesPalette()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, IsActive = false };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => { p.IsActive = true; return p; });

        // Act
        var result = await _service.ActivateAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_ActivePalette_DeactivatesPalette()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, IsActive = true, IsDefault = false };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => { p.IsActive = false; return p; });

        // Act
        var result = await _service.DeactivateAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_DefaultPalette_ThrowsException()
    {
        // Arrange
        var palette = new ColorPalette { Id = 1, IsActive = true, IsDefault = true };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(palette);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeactivateAsync(1));
    }

    #endregion

    #region Color Validation Tests

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#FF0066")]
    [InlineData("#ABC")]
    [InlineData("#abc")]
    public void ValidateHexColor_ValidColors_ReturnsTrue(string color)
    {
        // Act
        var result = _service.ValidateHexColor(color);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("FFFFFF")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345")]
    [InlineData("red")]
    [InlineData("")]
    [InlineData(null)]
    public void ValidateHexColor_InvalidColors_ReturnsFalse(string? color)
    {
        // Act
        var result = _service.ValidateHexColor(color);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Clone Tests

    [Fact]
    public async Task CloneAsync_ValidPalette_ReturnsClonedPalette()
    {
        // Arrange
        var original = new ColorPalette
        {
            Id = 1,
            Name = "Original Theme",
            PrimaryColor = "#0066CC",
            SecondaryColor = "#003366"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(original);

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ColorPalette>()))
            .ReturnsAsync((ColorPalette p) => { p.Id = 2; return p; });

        // Act
        var result = await _service.CloneAsync(1, "Cloned Theme");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("Cloned Theme");
        result.PrimaryColor.Should().Be("#0066CC");
    }

    [Fact]
    public async Task CloneAsync_NonExistingPalette_ReturnsNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((ColorPalette?)null);

        // Act
        var result = await _service.CloneAsync(999, "Clone");

        // Assert
        result.Should().BeNull();
    }

    #endregion
}
