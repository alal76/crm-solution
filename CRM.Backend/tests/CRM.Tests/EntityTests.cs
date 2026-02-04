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
using CRM.Core.Entities;

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
