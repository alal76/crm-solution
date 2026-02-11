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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Xunit;
using FluentAssertions;

namespace CRM.Tests.Configurations;

/// <summary>
/// Unit tests for EF Core entity configurations.
/// Tests that entities are properly configured with correct column types, indexes, and relationships.
/// </summary>
public class EntityConfigurationTests
{
    private readonly DbContextOptions<CrmDbContext> _options;
    private readonly CrmDbContext _context;
    private readonly IModel _model;

    public EntityConfigurationTests()
    {
        _options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseInMemoryDatabase(databaseName: $"ConfigTest_{Guid.NewGuid()}")
            .Options;

        _context = new CrmDbContext(_options);
        _model = _context.Model;
    }

    #region Account Configuration Tests

    [Fact]
    public void Account_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
        entity.FindPrimaryKey()!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void Account_Email_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var property = entity?.FindProperty("Email");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Account_Email_ShouldHaveMaxLength255()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var property = entity?.FindProperty("Email");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void Account_FirstName_ShouldHaveMaxLength100()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var property = entity?.FindProperty("FirstName");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void Account_Company_ShouldHaveMaxLength255()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var property = entity?.FindProperty("Company");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void Account_ShouldHaveEmailIndex()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var indexes = entity?.GetIndexes();

        // Assert
        indexes.Should().NotBeNull();
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Email"));
    }

    [Fact]
    public void Account_ShouldHaveSelfReferencingParentAccountRelationship()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Account));
        var navigation = entity?.FindNavigation("ParentAccount");

        // Assert
        navigation.Should().NotBeNull();
    }

    #endregion

    #region Contact Configuration Tests

    [Fact]
    public void Contact_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Contact));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Contact_FirstName_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Contact));
        var property = entity?.FindProperty("FirstName");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Contact_LastName_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Contact));
        var property = entity?.FindProperty("LastName");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    #endregion

    #region User Configuration Tests

    [Fact]
    public void User_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(User));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void User_Username_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(User));
        var property = entity?.FindProperty("Username");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void User_Email_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(User));
        var property = entity?.FindProperty("Email");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void User_ShouldHaveEmailIndex()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(User));
        var indexes = entity?.GetIndexes();

        // Assert
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Email"));
    }

    [Fact]
    public void User_ShouldHaveUsernameIndex()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(User));
        var indexes = entity?.GetIndexes();

        // Assert
        indexes.Should().Contain(i => i.Properties.Any(p => p.Name == "Username"));
    }

    #endregion

    #region Opportunity Configuration Tests

    [Fact]
    public void Opportunity_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Opportunity));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Opportunity_Name_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Opportunity));
        var property = entity?.FindProperty("Name");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_ShouldHaveAccountRelationship()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Opportunity));
        var navigation = entity?.FindNavigation("Account");

        // Assert
        navigation.Should().NotBeNull();
    }

    #endregion

    #region Product Configuration Tests

    [Fact]
    public void Product_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Product));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Product_Name_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Product));
        var property = entity?.FindProperty("Name");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    #endregion

    #region Lead Configuration Tests

    [Fact]
    public void Lead_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Lead));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Lead_FirstName_ShouldHaveMaxLength100()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Lead));
        var property = entity?.FindProperty("FirstName");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(100);
    }

    #endregion

    #region Junction Table Configuration Tests

    [Fact]
    public void AccountContact_ShouldHaveCompositePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(AccountContact));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void AccountContact_ShouldHaveUniqueIndexOnAccountIdContactId()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(AccountContact));
        var indexes = entity?.GetIndexes();

        // Assert
        indexes.Should().NotBeNull();
        indexes.Should().Contain(i =>
            i.Properties.Any(p => p.Name == "AccountId") &&
            i.Properties.Any(p => p.Name == "ContactId") &&
            i.IsUnique);
    }

    [Fact]
    public void OpportunityProduct_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(OpportunityProduct));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    #endregion

    #region Address Configuration Tests

    [Fact]
    public void Address_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Address));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Address_City_ShouldHaveMaxLength100()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Address));
        var property = entity?.FindProperty("City");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(100);
    }

    [Fact]
    public void Address_PostalCode_ShouldHaveMaxLength20()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Address));
        var property = entity?.FindProperty("PostalCode");

        // Assert
        property.Should().NotBeNull();
        property!.GetMaxLength().Should().Be(20);
    }

    #endregion

    #region Workflow Configuration Tests

    [Fact]
    public void WorkflowDefinition_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(WorkflowDefinition));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void WorkflowInstance_ShouldHaveWorkflowDefinitionRelationship()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(WorkflowInstance));
        var navigation = entity?.FindNavigation("Definition");

        // Assert
        navigation.Should().NotBeNull();
    }

    #endregion

    #region Marketing Campaign Configuration Tests

    [Fact]
    public void MarketingCampaign_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(MarketingCampaign));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void MarketingCampaign_Name_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(MarketingCampaign));
        var property = entity?.FindProperty("Name");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    #endregion

    #region Service Request Configuration Tests

    [Fact]
    public void ServiceRequest_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(ServiceRequest));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void ServiceRequest_Subject_ShouldBeRequired()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(ServiceRequest));
        var property = entity?.FindProperty("Subject");

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
    }

    #endregion

    #region Quote Configuration Tests

    [Fact]
    public void Quote_ShouldHavePrimaryKey()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Quote));

        // Assert
        entity.Should().NotBeNull();
        entity!.FindPrimaryKey().Should().NotBeNull();
    }

    [Fact]
    public void Quote_ShouldHaveOpportunityRelationship()
    {
        // Arrange
        var entity = _model.FindEntityType(typeof(Quote));
        var navigation = entity?.FindNavigation("Opportunity");

        // Assert
        navigation.Should().NotBeNull();
    }

    #endregion

    #region BaseEntity Configuration Tests

    [Theory]
    [InlineData(typeof(Account))]
    [InlineData(typeof(Contact))]
    [InlineData(typeof(Lead))]
    [InlineData(typeof(Opportunity))]
    [InlineData(typeof(Product))]
    public void AllBaseEntities_ShouldHaveCreatedAtProperty(Type entityType)
    {
        // Arrange
        var entity = _model.FindEntityType(entityType);
        var property = entity?.FindProperty("CreatedAt");

        // Assert
        property.Should().NotBeNull();
    }

    [Theory]
    [InlineData(typeof(Account))]
    [InlineData(typeof(Contact))]
    [InlineData(typeof(Lead))]
    [InlineData(typeof(Opportunity))]
    [InlineData(typeof(Product))]
    public void AllBaseEntities_ShouldHaveUpdatedAtProperty(Type entityType)
    {
        // Arrange
        var entity = _model.FindEntityType(entityType);
        var property = entity?.FindProperty("UpdatedAt");

        // Assert
        property.Should().NotBeNull();
    }

    [Theory]
    [InlineData(typeof(Account))]
    [InlineData(typeof(Contact))]
    [InlineData(typeof(Lead))]
    [InlineData(typeof(Opportunity))]
    [InlineData(typeof(Product))]
    public void AllBaseEntities_ShouldHaveIsDeletedProperty(Type entityType)
    {
        // Arrange
        var entity = _model.FindEntityType(entityType);
        var property = entity?.FindProperty("IsDeleted");

        // Assert
        property.Should().NotBeNull();
    }

    #endregion

    #region Cleanup

    public void Dispose()
    {
        _context?.Dispose();
    }

    #endregion
}
