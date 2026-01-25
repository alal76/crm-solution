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
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Company = "Tech Corp"
            };

            // Act & Assert
            Assert.Equal("john@example.com", customer.Email);
            Assert.NotEmpty(customer.FirstName);
            Assert.NotEmpty(customer.LastName);
        }

        [Fact]
        public void Customer_ShouldHaveValidPhoneNumber()
        {
            // Arrange
            var customer = new Customer
            {
                Phone = "+1-555-0001"
            };

            // Act & Assert
            Assert.Equal("+1-555-0001", customer.Phone);
        }

        [Fact]
        public void Customer_LifecycleStageShouldDefaultToOther()
        {
            // Arrange
            var customer = new Customer();

            // Act & Assert
            Assert.Equal(CustomerLifecycleStage.Other, customer.LifecycleStage);
        }

        [Fact]
        public void Customer_ShouldCalculateFullName()
        {
            // Arrange
            var customer = new Customer
            {
                FirstName = "John",
                LastName = "Doe"
            };

            // Act & Assert
            Assert.Equal("John", customer.FirstName);
            Assert.Equal("Doe", customer.LastName);
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
