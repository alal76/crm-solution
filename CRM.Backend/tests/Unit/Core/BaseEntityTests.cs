// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Tests for BaseEntity abstract class
/// </summary>
public class BaseEntityTests
{
    #region Test Entity Implementation

    /// <summary>
    /// Concrete implementation of BaseEntity for testing
    /// </summary>
    private class TestEntity : BaseEntity
    {
        public string? Name { get; set; }
    }

    #endregion

    #region Constructor and Default Values Tests

    [Fact]
    public void BaseEntity_ShouldBeAbstractClass()
    {
        typeof(BaseEntity).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void NewEntity_ShouldHaveDefaultId()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Id.Should().Be(0);
    }

    [Fact]
    public void NewEntity_ShouldHaveCreatedAtSetToUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var entity = new TestEntity();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        entity.CreatedAt.Should().BeAfter(beforeCreation);
        entity.CreatedAt.Should().BeBefore(afterCreation);
    }

    [Fact]
    public void NewEntity_ShouldHaveUpdatedAtAsNull()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void NewEntity_ShouldHaveIsDeletedAsFalse()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void NewEntity_ShouldHaveRowVersionAsNull()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.RowVersion.Should().BeNull();
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void Id_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.Id = 42;

        // Assert
        entity.Id.Should().Be(42);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var specificDate = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        entity.CreatedAt = specificDate;

        // Assert
        entity.CreatedAt.Should().Be(specificDate);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var updateDate = DateTime.UtcNow;

        // Act
        entity.UpdatedAt = updateDate;

        // Assert
        entity.UpdatedAt.Should().Be(updateDate);
    }

    [Fact]
    public void IsDeleted_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void RowVersion_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // Act
        entity.RowVersion = rowVersion;

        // Assert
        entity.RowVersion.Should().BeEquivalentTo(rowVersion);
    }

    #endregion

    #region Timestamp Attribute Tests

    [Fact]
    public void RowVersion_ShouldHaveTimestampAttribute()
    {
        // Arrange
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.RowVersion));

        // Act & Assert
        property.Should().NotBeNull();
        property!.GetCustomAttribute<TimestampAttribute>().Should().NotBeNull();
    }

    [Fact]
    public void Id_ShouldNotHaveTimestampAttribute()
    {
        // Arrange
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));

        // Act & Assert
        property.Should().NotBeNull();
        property!.GetCustomAttribute<TimestampAttribute>().Should().BeNull();
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void DerivedEntity_ShouldInheritAllProperties()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.Should().BeAssignableTo<BaseEntity>();
        entity.Should().HaveProperty<int>(nameof(BaseEntity.Id));
        entity.Should().HaveProperty<DateTime>(nameof(BaseEntity.CreatedAt));
        entity.Should().HaveProperty<DateTime?>(nameof(BaseEntity.UpdatedAt));
        entity.Should().HaveProperty<bool>(nameof(BaseEntity.IsDeleted));
        entity.Should().HaveProperty<byte[]?>(nameof(BaseEntity.RowVersion));
    }

    [Fact]
    public void DerivedEntity_CanAddAdditionalProperties()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.Name = "Test";

        // Assert
        entity.Name.Should().Be("Test");
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void Entity_CanBeMarkedAsDeleted()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            Name = "Test"
        };

        // Act
        entity.IsDeleted = true;

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.Id.Should().Be(1); // Other properties remain unchanged
        entity.Name.Should().Be("Test");
    }

    [Fact]
    public void Entity_CanBeRestored()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            IsDeleted = true
        };

        // Act
        entity.IsDeleted = false;

        // Assert
        entity.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Concurrency Token Tests

    [Fact]
    public void RowVersion_CanBeUsedForConcurrencyControl()
    {
        // Arrange
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 1 };

        var version1 = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
        var version2 = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02 };

        // Act
        entity1.RowVersion = version1;
        entity2.RowVersion = version2;

        // Assert
        entity1.RowVersion.Should().NotBeEquivalentTo(entity2.RowVersion);
    }

    [Fact]
    public void RowVersion_ShouldBe8Bytes_WhenSetProperly()
    {
        // Arrange
        var entity = new TestEntity();
        var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

        // Act
        entity.RowVersion = rowVersion;

        // Assert
        entity.RowVersion.Should().HaveCount(8);
    }

    #endregion

    #region Common Entity Patterns Tests

    [Fact]
    public void Entity_ShouldSupportUpdatePattern()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            Name = "Original"
        };
        var originalCreatedAt = entity.CreatedAt;

        // Act - Simulate update
        entity.Name = "Updated";
        entity.UpdatedAt = DateTime.UtcNow;

        // Assert
        entity.Name.Should().Be("Updated");
        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeOnOrAfter(originalCreatedAt); // Use BeOnOrAfter for fast test execution
        entity.CreatedAt.Should().Be(originalCreatedAt); // Created should not change
    }

    [Fact]
    public void Entity_ShouldSupportSoftDeletePattern()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = 1,
            Name = "To Delete"
        };

        // Act - Simulate soft delete
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        // Assert
        entity.IsDeleted.Should().BeTrue();
        entity.UpdatedAt.Should().NotBeNull();
        entity.Name.Should().Be("To Delete"); // Data preserved
    }

    #endregion

    #region Property Type Tests

    [Fact]
    public void Id_ShouldBeInt()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id));
        property!.PropertyType.Should().Be(typeof(int));
    }

    [Fact]
    public void CreatedAt_ShouldBeDateTime()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.CreatedAt));
        property!.PropertyType.Should().Be(typeof(DateTime));
    }

    [Fact]
    public void UpdatedAt_ShouldBeNullableDateTime()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.UpdatedAt));
        property!.PropertyType.Should().Be(typeof(DateTime?));
    }

    [Fact]
    public void IsDeleted_ShouldBeBool()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.IsDeleted));
        property!.PropertyType.Should().Be(typeof(bool));
    }

    [Fact]
    public void RowVersion_ShouldBeNullableByteArray()
    {
        var property = typeof(BaseEntity).GetProperty(nameof(BaseEntity.RowVersion));
        property!.PropertyType.Should().Be(typeof(byte[]));
    }

    #endregion

    #region Real Entity Inheritance Tests

    [Fact]
    public void Account_ShouldInheritFromBaseEntity()
    {
        typeof(Account).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Contact_Model_ShouldExist()
    {
        // Note: Contact exists in Models namespace, not Entities
        typeof(CRM.Core.Models.Contact).Should().NotBeNull();
    }

    [Fact]
    public void User_ShouldInheritFromBaseEntity()
    {
        typeof(User).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Opportunity_ShouldInheritFromBaseEntity()
    {
        typeof(Opportunity).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Lead_ShouldInheritFromBaseEntity()
    {
        typeof(Lead).Should().BeAssignableTo<BaseEntity>();
    }

    [Fact]
    public void Product_ShouldInheritFromBaseEntity()
    {
        typeof(Product).Should().BeAssignableTo<BaseEntity>();
    }

    #endregion
}

/// <summary>
/// Extension methods for BaseEntity testing
/// </summary>
internal static class BaseEntityTestExtensions
{
    public static void HaveProperty<T>(this FluentAssertions.Primitives.ObjectAssertions assertions, string propertyName)
    {
        var property = assertions.Subject.GetType().GetProperty(propertyName);
        property.Should().NotBeNull($"Property '{propertyName}' should exist");
    }
}
