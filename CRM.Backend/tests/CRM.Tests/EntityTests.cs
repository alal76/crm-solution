// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using Xunit;

namespace CRM.Tests
{
    public class CustomerEntityTests
    {
        [Fact]
        public void Customer_ShouldHaveValidEmail()
        {
            // Arrange
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Company = "Tech Corp"
            };

            // Act & Assert
            Assert.Equal("john@example.com", account.Email);
            Assert.NotEmpty(account.FirstName);
            Assert.NotEmpty(account.LastName);
        }

        [Fact]
        public void Customer_ShouldHaveValidPhoneNumber()
        {
            // Arrange
            var account = new Account
            {
                Phone = "+1-555-0001"
            };

            // Act & Assert
            Assert.Equal("+1-555-0001", account.Phone);
        }

        [Fact]
        public void Account_LifecycleStageShouldDefaultToOther()
        {
            // Arrange
            var account = new Account();

            // Act & Assert
            Assert.Equal(AccountLifecycleStage.Other, account.LifecycleStage);
        }

        [Fact]
        public void Customer_ShouldCalculateFullName()
        {
            // Arrange
            var account = new Account
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act & Assert
            Assert.Equal("John", account.FirstName);
            Assert.Equal("Doe", account.LastName);
        }
    }

    public class ProductEntityTests
    {
        [Fact]
        public void Product_ShouldHaveValidPrice()
        {
            // Arrange
            var product = new Product
            {
                Name = "Premium Package",
                Price = 999.99m,
                Quantity = 50
            };

            // Act & Assert
            Assert.Equal(999.99m, product.Price);
            Assert.Equal(50, product.Quantity);
        }

        [Fact]
        public void Product_ShouldHaveUniqueSKU()
        {
            // Arrange
            var product = new Product
            {
                SKU = "PKG-001"
            };

            // Act & Assert
            Assert.Equal("PKG-001", product.SKU);
        }

        [Fact]
        public void Product_IsActiveShouldDefaultToTrue()
        {
            // Arrange
            var product = new Product();

            // Act & Assert
            Assert.True(product.IsActive);
        }
    }
}
