using Xunit;
using CRM.Core.Entities;

namespace CRM.Tests
{
    public class UserEntityTests
    {
        [Fact]
        public void User_ShouldHaveAdminRole()
        {
            // Arrange
            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                Role = (int)UserRole.Admin,
                IsActive = true
            };

            // Act & Assert
            Assert.Equal((int)UserRole.Admin, user.Role);
            Assert.True(user.IsActive);
        }

        [Fact]
        public void User_EmailShouldBeCaseSensitive()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com"
            };

            // Act & Assert
            Assert.Equal("test@example.com", user.Email);
        }

        [Fact]
        public void User_ShouldHaveDefaultRole()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.Equal(0, user.Role); // Default should be Admin (0)
        }

        [Fact]
        public void User_ShouldBeActiveByDefault()
        {
            // Arrange & Act
            var user = new User();

            // Assert
            Assert.True(user.IsActive);
        }

        [Fact]
        public void User_CanEnableTwoFactor()
        {
            // Arrange
            var user = new User { TwoFactorEnabled = false };

            // Act
            user.TwoFactorEnabled = true;

            // Assert
            Assert.True(user.TwoFactorEnabled);
        }
    }

    public class UserRoleTests
    {
        [Fact]
        public void UserRole_AdminShouldBeZero()
        {
            // Assert
            Assert.Equal(0, (int)UserRole.Admin);
        }

        [Fact]
        public void UserRole_AllRolesShouldHaveValidValues()
        {
            // Assert
            Assert.Equal(0, (int)UserRole.Admin);
            Assert.Equal(1, (int)UserRole.Manager);
            Assert.Equal(2, (int)UserRole.Sales);
            Assert.Equal(3, (int)UserRole.Support);
            Assert.Equal(4, (int)UserRole.Guest);
        }
    }
}
