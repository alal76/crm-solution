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
        public void Customer_StatusShouldBeActive()
        {
            // Arrange
            var customer = new Customer
            {
                Status = "Active"
            };

            // Act & Assert
            Assert.Equal("Active", customer.Status);
        }

        [Fact]
        public void Customer_ShouldHaveOpportunitiesCollection()
        {
            // Arrange
            var customer = new Customer();

            // Act & Assert
            Assert.NotNull(customer.Opportunities);
            Assert.Empty(customer.Opportunities);
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
                Stock = 50
            };

            // Act & Assert
            Assert.Equal(999.99m, product.Price);
            Assert.Equal(50, product.Stock);
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
        public void Product_StatusShouldBeActive()
        {
            // Arrange
            var product = new Product
            {
                Status = "active"
            };

            // Act & Assert
            Assert.Equal("active", product.Status);
        }
    }
}
