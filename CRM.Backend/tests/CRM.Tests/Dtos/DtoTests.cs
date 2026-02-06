// CRM Solution - DTO Tests
// Comprehensive tests for Data Transfer Objects (DTOs)
// Tests default values, property assignments, and computed properties

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Xunit;
using CRM.Core.Dtos;

namespace CRM.Tests.Dtos;

/// <summary>
/// Tests for AccountDto and related Account DTOs.
/// </summary>
public class AccountDtoTests
{
    #region Default Value Tests

    [Fact]
    public void AccountDto_DefaultValues_AreCorrect()
    {
        var dto = new AccountDto();

        dto.Id.Should().Be(0);
        dto.Category.Should().Be("Individual");
        dto.FirstName.Should().BeEmpty();
        dto.LastName.Should().BeEmpty();
        dto.Company.Should().BeEmpty();
        dto.Email.Should().BeEmpty();
        dto.Phone.Should().BeEmpty();
        dto.Address.Should().BeEmpty();
        dto.City.Should().BeEmpty();
        dto.State.Should().BeEmpty();
        dto.ZipCode.Should().BeEmpty();
        dto.Country.Should().BeEmpty();
        dto.AccountType.Should().Be("Individual");
        dto.Priority.Should().Be("Medium");
        dto.LifecycleStage.Should().Be("Lead");
        dto.ShippingSameAsBilling.Should().BeTrue();
        dto.AnnualRevenue.Should().Be(0);
        dto.TotalPurchases.Should().Be(0);
        dto.AccountBalance.Should().Be(0);
    }

    [Theory]
    [InlineData("Individual", false)]
    [InlineData("Organization", true)]
    public void AccountDto_IsOrganization_ReturnsCorrectValue(string category, bool expectedIsOrg)
    {
        var dto = new AccountDto { Category = category };

        dto.IsOrganization.Should().Be(expectedIsOrg);
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void AccountDto_CanSetAllProperties()
    {
        var dto = new AccountDto
        {
            Id = 1,
            Category = "Organization",
            FirstName = "John",
            LastName = "Doe",
            Salutation = "Mr.",
            Suffix = "Jr.",
            DateOfBirth = new DateTime(1990, 1, 15),
            Gender = "Male",
            LinkedContactId = 10,
            LinkedContactName = "Contact Person",
            Company = "Acme Corp",
            LegalName = "Acme Corporation Inc.",
            DbaName = "Acme",
            TaxId = "12-3456789",
            RegistrationNumber = "REG-001",
            YearFounded = 2000,
            PrimaryContactId = 5,
            PrimaryContactName = "Primary Contact",
            Email = "john@acme.com",
            SecondaryEmail = "support@acme.com",
            Phone = "+1-234-567-8900",
            MobilePhone = "+1-234-567-8901",
            FaxNumber = "+1-234-567-8902",
            JobTitle = "CEO",
            Website = "https://acme.com",
            Address = "123 Main St",
            Address2 = "Suite 100",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            Country = "USA",
            ShippingAddress = "456 Ship St",
            ShippingAddress2 = "Dock 5",
            ShippingCity = "Los Angeles",
            ShippingState = "CA",
            ShippingZipCode = "90001",
            ShippingCountry = "USA",
            ShippingSameAsBilling = false,
            Industry = "Technology",
            SubIndustry = "Software",
            NumberOfEmployees = 500,
            EmployeeRange = "100-500",
            AnnualRevenue = 10000000m,
            RevenueRange = "$10M-$50M",
            AccountType = "Enterprise",
            Priority = "High",
            StockSymbol = "ACME",
            Ownership = "Public",
            LifecycleStage = "Customer"
        };

        dto.Id.Should().Be(1);
        dto.Category.Should().Be("Organization");
        dto.IsOrganization.Should().BeTrue();
        dto.Company.Should().Be("Acme Corp");
        dto.Industry.Should().Be("Technology");
        dto.AnnualRevenue.Should().Be(10000000m);
    }

    [Fact]
    public void AccountDto_ShippingAddress_CanBeDifferentFromBilling()
    {
        var dto = new AccountDto
        {
            Address = "123 Billing St",
            City = "Billing City",
            State = "BC",
            ZipCode = "11111",
            Country = "USA",
            ShippingAddress = "456 Shipping St",
            ShippingCity = "Shipping City",
            ShippingState = "SC",
            ShippingZipCode = "22222",
            ShippingCountry = "Canada",
            ShippingSameAsBilling = false
        };

        dto.ShippingSameAsBilling.Should().BeFalse();
        dto.ShippingAddress.Should().NotBe(dto.Address);
        dto.ShippingCity.Should().NotBe(dto.City);
        dto.ShippingCountry.Should().NotBe(dto.Country);
    }

    #endregion

    #region Financial Properties Tests

    [Fact]
    public void AccountDto_FinancialProperties_HandleLargeValues()
    {
        var dto = new AccountDto
        {
            AnnualRevenue = 999999999999.99m,
            TotalPurchases = 888888888888.88m,
            AccountBalance = 100000000.50m,
            LifetimeValue = 1500000000.00m
        };

        dto.AnnualRevenue.Should().Be(999999999999.99m);
        dto.TotalPurchases.Should().Be(888888888888.88m);
        dto.AccountBalance.Should().Be(100000000.50m);
    }

    [Fact]
    public void AccountDto_FinancialProperties_HandleNegativeValues()
    {
        var dto = new AccountDto
        {
            AccountBalance = -5000.50m
        };

        dto.AccountBalance.Should().Be(-5000.50m);
    }

    #endregion

    #region Date Properties Tests

    [Fact]
    public void AccountDto_DateProperties_CanBeNull()
    {
        var dto = new AccountDto();

        dto.DateOfBirth.Should().BeNull();
        dto.FirstContactDate.Should().BeNull();
        dto.ConversionDate.Should().BeNull();
        dto.LastActivityDate.Should().BeNull();
        dto.NextFollowUpDate.Should().BeNull();
    }

    [Fact]
    public void AccountDto_DateProperties_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var dto = new AccountDto
        {
            DateOfBirth = new DateTime(1985, 6, 15),
            FirstContactDate = now.AddDays(-30),
            ConversionDate = now.AddDays(-15),
            LastActivityDate = now.AddDays(-1),
            NextFollowUpDate = now.AddDays(7)
        };

        dto.DateOfBirth.Should().Be(new DateTime(1985, 6, 15));
        dto.FirstContactDate.Should().BeCloseTo(now.AddDays(-30), TimeSpan.FromSeconds(1));
        dto.ConversionDate.Should().BeCloseTo(now.AddDays(-15), TimeSpan.FromSeconds(1));
        dto.LastActivityDate.Should().BeCloseTo(now.AddDays(-1), TimeSpan.FromSeconds(1));
        dto.NextFollowUpDate.Should().BeCloseTo(now.AddDays(7), TimeSpan.FromSeconds(1));
    }

    #endregion
}

/// <summary>
/// Tests for UserDto and related User DTOs.
/// </summary>
public class UserDtoTests
{
    #region Default Value Tests

    [Fact]
    public void UserDto_DefaultValues_AreCorrect()
    {
        var dto = new UserDto();

        dto.Id.Should().Be(0);
        dto.Username.Should().BeEmpty();
        dto.Email.Should().BeEmpty();
        dto.FirstName.Should().BeEmpty();
        dto.LastName.Should().BeEmpty();
        dto.Role.Should().BeEmpty();
        dto.IsActive.Should().BeFalse();
        dto.DepartmentId.Should().BeNull();
        dto.DepartmentName.Should().BeNull();
        dto.UserProfileId.Should().BeNull();
        dto.UserProfileName.Should().BeNull();
        dto.PrimaryGroupId.Should().BeNull();
        dto.PrimaryGroupName.Should().BeNull();
        dto.ContactId.Should().BeNull();
        dto.ContactName.Should().BeNull();
        dto.ContactEmail.Should().BeNull();
        dto.HeaderColor.Should().BeNull();
        dto.PhotoUrl.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void UserDto_CanSetAllProperties()
    {
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var lastLogin = DateTime.UtcNow.AddHours(-1);

        var dto = new UserDto
        {
            Id = 1,
            Username = "johndoe",
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Admin",
            IsActive = true,
            DepartmentId = 5,
            DepartmentName = "Sales",
            UserProfileId = 10,
            UserProfileName = "Sales Manager Profile",
            PrimaryGroupId = 3,
            PrimaryGroupName = "Sales Team",
            ContactId = 100,
            ContactName = "John Doe Contact",
            ContactEmail = "john.contact@example.com",
            CreatedAt = createdAt,
            LastLoginDate = lastLogin,
            HeaderColor = "#FF5733",
            PhotoUrl = "https://example.com/photos/johndoe.jpg"
        };

        dto.Id.Should().Be(1);
        dto.Username.Should().Be("johndoe");
        dto.Email.Should().Be("john.doe@example.com");
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.Role.Should().Be("Admin");
        dto.IsActive.Should().BeTrue();
        dto.DepartmentId.Should().Be(5);
        dto.DepartmentName.Should().Be("Sales");
        dto.UserProfileId.Should().Be(10);
        dto.UserProfileName.Should().Be("Sales Manager Profile");
        dto.PrimaryGroupId.Should().Be(3);
        dto.PrimaryGroupName.Should().Be("Sales Team");
        dto.ContactId.Should().Be(100);
        dto.ContactName.Should().Be("John Doe Contact");
        dto.ContactEmail.Should().Be("john.contact@example.com");
        dto.CreatedAt.Should().Be(createdAt);
        dto.LastLoginDate.Should().Be(lastLogin);
        dto.HeaderColor.Should().Be("#FF5733");
        dto.PhotoUrl.Should().Be("https://example.com/photos/johndoe.jpg");
    }

    #endregion

    #region CreateUserRequest Tests

    [Fact]
    public void CreateUserRequest_DefaultValues_AreCorrect()
    {
        var dto = new CreateUserRequest();

        dto.Email.Should().BeEmpty();
        dto.FirstName.Should().BeEmpty();
        dto.LastName.Should().BeEmpty();
        dto.Password.Should().BeNull();
        dto.RoleId.Should().Be(2); // Default User role
        dto.DepartmentId.Should().BeNull();
        dto.PrimaryGroupId.Should().BeNull();
    }

    [Fact]
    public void CreateUserRequest_CanSetAllProperties()
    {
        var dto = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User",
            Password = "SecureP@ssw0rd!",
            RoleId = 1,
            DepartmentId = 10,
            PrimaryGroupId = 5
        };

        dto.Email.Should().Be("newuser@example.com");
        dto.FirstName.Should().Be("New");
        dto.LastName.Should().Be("User");
        dto.Password.Should().Be("SecureP@ssw0rd!");
        dto.RoleId.Should().Be(1);
        dto.DepartmentId.Should().Be(10);
        dto.PrimaryGroupId.Should().Be(5);
    }

    [Fact]
    public void CreateUserRequest_Password_IsOptional()
    {
        var dto = new CreateUserRequest
        {
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User"
            // Password not set - should remain null
        };

        dto.Password.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Tests for ContactDto and related Contact DTOs.
/// </summary>
public class ContactDtoTests
{
    [Fact]
    public void ContactDto_DefaultValues_AreCorrect()
    {
        var dto = new ContactDto();

        dto.Id.Should().Be(0);
        dto.FirstName.Should().BeEmpty();
        dto.LastName.Should().BeEmpty();
        dto.EmailPrimary.Should().BeEmpty();
        dto.PhonePrimary.Should().BeEmpty();
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ContactDto_CanSetAllProperties()
    {
        var dto = new ContactDto
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Smith",
            Salutation = "Ms.",
            Suffix = "PhD",
            EmailPrimary = "jane@example.com",
            EmailSecondary = "jane.smith@example.com",
            PhonePrimary = "+1-234-567-8900",
            PhoneMobile = "+1-234-567-8901",
            PhoneWork = "+1-234-567-8902",
            JobTitle = "Director of Engineering",
            Department = "Engineering",
            AccountId = 100,
            AccountName = "Tech Corp",
            IsActive = true,
            IsPrimary = true,
            OwnerId = 5,
            OwnerName = "John Doe"
        };

        dto.Id.Should().Be(1);
        dto.FirstName.Should().Be("Jane");
        dto.LastName.Should().Be("Smith");
        dto.Salutation.Should().Be("Ms.");
        dto.Suffix.Should().Be("PhD");
        dto.EmailPrimary.Should().Be("jane@example.com");
        dto.PhonePrimary.Should().Be("+1-234-567-8900");
        dto.JobTitle.Should().Be("Director of Engineering");
        dto.AccountId.Should().Be(100);
        dto.AccountName.Should().Be("Tech Corp");
        dto.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void ContactDto_SocialMediaLinks_CanBeSet()
    {
        var dto = new ContactDto
        {
            FirstName = "Test",
            LastName = "User",
            LinkedInUrl = "https://linkedin.com/in/testuser",
            TwitterHandle = "@testuser",
            FacebookUrl = "https://facebook.com/testuser"
        };

        dto.LinkedInUrl.Should().Be("https://linkedin.com/in/testuser");
        dto.TwitterHandle.Should().Be("@testuser");
        dto.FacebookUrl.Should().Be("https://facebook.com/testuser");
    }
}

/// <summary>
/// Tests for DepartmentDto.
/// </summary>
public class DepartmentDtoTests
{
    [Fact]
    public void DepartmentDto_DefaultValues_AreCorrect()
    {
        var dto = new DepartmentDto();

        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.Description.Should().BeNull();
        dto.ParentDepartmentId.Should().BeNull();
        dto.ParentDepartmentName.Should().BeNull();
        dto.IsActive.Should().BeTrue();
        dto.MemberCount.Should().Be(0);
    }

    [Fact]
    public void DepartmentDto_CanSetAllProperties()
    {
        var dto = new DepartmentDto
        {
            Id = 1,
            Name = "Engineering",
            Description = "Software development team",
            Code = "ENG",
            ParentDepartmentId = 10,
            ParentDepartmentName = "Technology",
            ManagerId = 5,
            ManagerName = "John Manager",
            IsActive = true,
            SortOrder = 1,
            MemberCount = 25
        };

        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Engineering");
        dto.Code.Should().Be("ENG");
        dto.ParentDepartmentId.Should().Be(10);
        dto.ManagerId.Should().Be(5);
        dto.MemberCount.Should().Be(25);
    }
}

/// <summary>
/// Tests for UserGroupDto.
/// </summary>
public class UserGroupDtoTests
{
    [Fact]
    public void UserGroupDto_DefaultValues_AreCorrect()
    {
        var dto = new UserGroupDto();

        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.IsActive.Should().BeTrue();
        dto.IsDefault.Should().BeFalse();
        dto.IsSystemAdmin.Should().BeFalse();
        dto.MemberCount.Should().Be(0);
    }

    [Fact]
    public void UserGroupDto_CanSetAllProperties()
    {
        var dto = new UserGroupDto
        {
            Id = 1,
            Name = "Administrators",
            Description = "System administrators with full access",
            IsActive = true,
            IsDefault = false,
            IsSystemAdmin = true,
            DisplayOrder = 1,
            HeaderColor = "#FF0000",
            MemberCount = 5
        };

        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Administrators");
        dto.IsSystemAdmin.Should().BeTrue();
        dto.MemberCount.Should().Be(5);
        dto.HeaderColor.Should().Be("#FF0000");
    }

    [Fact]
    public void UserGroupDto_Permissions_CanBeSet()
    {
        var dto = new UserGroupDto
        {
            Name = "Sales Team",
            CanAccessDashboard = true,
            CanAccessCustomers = true,
            CanAccessContacts = true,
            CanAccessLeads = true,
            CanAccessOpportunities = true,
            CanCreateCustomers = true,
            CanEditCustomers = true,
            CanDeleteCustomers = false,
            CanViewAllCustomers = true,
            CanExportData = true,
            CanImportData = false
        };

        dto.CanAccessDashboard.Should().BeTrue();
        dto.CanAccessCustomers.Should().BeTrue();
        dto.CanCreateCustomers.Should().BeTrue();
        dto.CanDeleteCustomers.Should().BeFalse();
        dto.CanExportData.Should().BeTrue();
        dto.CanImportData.Should().BeFalse();
    }
}

/// <summary>
/// Tests for SystemSettingsDto.
/// </summary>
public class SystemSettingsDtoTests
{
    [Fact]
    public void SystemSettingsDto_DefaultValues_AreCorrect()
    {
        var dto = new SystemSettingsDto();

        dto.Id.Should().Be(0);
        dto.Category.Should().BeEmpty();
        dto.Key.Should().BeEmpty();
        dto.Value.Should().BeEmpty();
        dto.IsEncrypted.Should().BeFalse();
        dto.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void SystemSettingsDto_CanSetAllProperties()
    {
        var dto = new SystemSettingsDto
        {
            Id = 1,
            Category = "Email",
            Key = "SmtpServer",
            Value = "smtp.example.com",
            Description = "SMTP server address for outgoing email",
            DataType = "string",
            IsEncrypted = false,
            IsReadOnly = true,
            DisplayOrder = 1
        };

        dto.Id.Should().Be(1);
        dto.Category.Should().Be("Email");
        dto.Key.Should().Be("SmtpServer");
        dto.Value.Should().Be("smtp.example.com");
        dto.Description.Should().Be("SMTP server address for outgoing email");
        dto.IsReadOnly.Should().BeTrue();
    }

    [Fact]
    public void SystemSettingsDto_EncryptedSetting_CanBeMarked()
    {
        var dto = new SystemSettingsDto
        {
            Category = "Security",
            Key = "ApiKey",
            Value = "encrypted-value-here",
            IsEncrypted = true
        };

        dto.IsEncrypted.Should().BeTrue();
    }
}

/// <summary>
/// Tests for ServiceRequestDto.
/// </summary>
public class ServiceRequestDtoTests
{
    [Fact]
    public void ServiceRequestDto_DefaultValues_AreCorrect()
    {
        var dto = new ServiceRequestDto();

        dto.Id.Should().Be(0);
        dto.Title.Should().BeEmpty();
        dto.Status.Should().Be("New");
        dto.Priority.Should().Be("Medium");
    }

    [Fact]
    public void ServiceRequestDto_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var dto = new ServiceRequestDto
        {
            Id = 1,
            TicketNumber = "SR-2024-001",
            Title = "Cannot login to system",
            Description = "User unable to login after password reset",
            Status = "Open",
            Priority = "High",
            CategoryId = 5,
            CategoryName = "Access Issues",
            SubcategoryId = 10,
            SubcategoryName = "Login Problems",
            AccountId = 100,
            AccountName = "Acme Corp",
            ContactId = 50,
            ContactName = "John User",
            ContactEmail = "john@acme.com",
            AssignedToId = 25,
            AssignedToName = "Support Agent",
            CreatedAt = now.AddDays(-1),
            UpdatedAt = now,
            DueDate = now.AddDays(1),
            ResolvedAt = null,
            ClosedAt = null
        };

        dto.Id.Should().Be(1);
        dto.TicketNumber.Should().Be("SR-2024-001");
        dto.Title.Should().Be("Cannot login to system");
        dto.Status.Should().Be("Open");
        dto.Priority.Should().Be("High");
        dto.CategoryName.Should().Be("Access Issues");
        dto.AssignedToName.Should().Be("Support Agent");
    }

    [Fact]
    public void ServiceRequestDto_SLAFields_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var dto = new ServiceRequestDto
        {
            Title = "Test Request",
            SLAPolicyId = 1,
            SLAPolicyName = "Standard SLA",
            ResponseDueDate = now.AddHours(4),
            ResolutionDueDate = now.AddDays(2),
            FirstResponseAt = now.AddMinutes(30),
            SLABreached = false
        };

        dto.SLAPolicyName.Should().Be("Standard SLA");
        dto.ResponseDueDate.Should().BeCloseTo(now.AddHours(4), TimeSpan.FromSeconds(1));
        dto.SLABreached.Should().BeFalse();
    }
}

/// <summary>
/// Tests for ContactInfoDto.
/// </summary>
public class ContactInfoDtoTests
{
    [Fact]
    public void ContactInfoDto_DefaultValues_AreCorrect()
    {
        var dto = new ContactInfoDto();

        dto.Id.Should().Be(0);
        dto.IsPrimary.Should().BeFalse();
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ContactInfoDto_EmailAddress_CanBeSet()
    {
        var dto = new ContactInfoDto
        {
            Id = 1,
            Type = "Email",
            Value = "user@example.com",
            Label = "Work",
            IsPrimary = true,
            IsVerified = true,
            IsActive = true
        };

        dto.Type.Should().Be("Email");
        dto.Value.Should().Be("user@example.com");
        dto.Label.Should().Be("Work");
        dto.IsPrimary.Should().BeTrue();
        dto.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void ContactInfoDto_PhoneNumber_CanBeSet()
    {
        var dto = new ContactInfoDto
        {
            Id = 2,
            Type = "Phone",
            Value = "+1-234-567-8900",
            Label = "Mobile",
            IsPrimary = true,
            Extension = "123"
        };

        dto.Type.Should().Be("Phone");
        dto.Value.Should().Be("+1-234-567-8900");
        dto.Label.Should().Be("Mobile");
        dto.Extension.Should().Be("123");
    }
}

/// <summary>
/// Tests for UserProfileDto.
/// </summary>
public class UserProfileDtoTests
{
    [Fact]
    public void UserProfileDto_DefaultValues_AreCorrect()
    {
        var dto = new UserProfileDto();

        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UserProfileDto_CanSetAllProperties()
    {
        var dto = new UserProfileDto
        {
            Id = 1,
            Name = "Sales Representative",
            Description = "Profile for sales team members",
            IsActive = true,
            IsDefault = false,
            AssignedUserCount = 15
        };

        dto.Id.Should().Be(1);
        dto.Name.Should().Be("Sales Representative");
        dto.Description.Should().Be("Profile for sales team members");
        dto.AssignedUserCount.Should().Be(15);
    }
}

/// <summary>
/// Tests for ModuleUIConfigDto.
/// </summary>
public class ModuleUIConfigDtoTests
{
    [Fact]
    public void ModuleUIConfigDto_DefaultValues_AreCorrect()
    {
        var dto = new ModuleUIConfigDto();

        dto.Id.Should().Be(0);
        dto.ModuleName.Should().BeEmpty();
        dto.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void ModuleUIConfigDto_CanSetAllProperties()
    {
        var dto = new ModuleUIConfigDto
        {
            Id = 1,
            ModuleName = "Customers",
            DisplayName = "Customer Management",
            Description = "Manage customer accounts",
            IconName = "people",
            Route = "/customers",
            SortOrder = 1,
            IsEnabled = true,
            IsVisible = true,
            ParentModuleId = null,
            RequiredPermission = "CanAccessCustomers"
        };

        dto.ModuleName.Should().Be("Customers");
        dto.DisplayName.Should().Be("Customer Management");
        dto.IconName.Should().Be("people");
        dto.Route.Should().Be("/customers");
        dto.IsEnabled.Should().BeTrue();
    }
}

/// <summary>
/// Tests for CommunicationDto.
/// </summary>
public class CommunicationDtoTests
{
    [Fact]
    public void CommunicationDto_DefaultValues_AreCorrect()
    {
        var dto = new CommunicationDto();

        dto.Id.Should().Be(0);
        dto.Subject.Should().BeEmpty();
        dto.Direction.Should().Be("Outbound");
        dto.Status.Should().Be("Draft");
    }

    [Fact]
    public void CommunicationDto_EmailCommunication_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var dto = new CommunicationDto
        {
            Id = 1,
            Type = "Email",
            Subject = "Follow-up on our meeting",
            Body = "Thank you for meeting with us...",
            Direction = "Outbound",
            Status = "Sent",
            FromAddress = "sales@company.com",
            ToAddresses = "customer@example.com",
            CcAddresses = "manager@company.com",
            SentAt = now,
            AccountId = 100,
            AccountName = "Customer Corp",
            ContactId = 50,
            ContactName = "John Customer",
            UserId = 10,
            UserName = "Sales Rep"
        };

        dto.Type.Should().Be("Email");
        dto.Subject.Should().Be("Follow-up on our meeting");
        dto.Status.Should().Be("Sent");
        dto.SentAt.Should().Be(now);
    }
}

/// <summary>
/// Tests for RelationshipDto.
/// </summary>
public class RelationshipDtoTests
{
    [Fact]
    public void AccountRelationshipDto_CanBeSet()
    {
        var dto = new AccountRelationshipDto
        {
            Id = 1,
            FromAccountId = 100,
            FromAccountName = "Parent Corp",
            ToAccountId = 200,
            ToAccountName = "Subsidiary Inc",
            RelationshipTypeId = 1,
            RelationshipTypeName = "Parent-Subsidiary",
            IsPrimary = true,
            Notes = "100% owned subsidiary"
        };

        dto.FromAccountName.Should().Be("Parent Corp");
        dto.ToAccountName.Should().Be("Subsidiary Inc");
        dto.RelationshipTypeName.Should().Be("Parent-Subsidiary");
        dto.IsPrimary.Should().BeTrue();
    }
}

/// <summary>
/// Tests for CloudDeploymentDto.
/// </summary>
public class CloudDeploymentDtoTests
{
    [Fact]
    public void CloudDeploymentDto_DefaultValues_AreCorrect()
    {
        var dto = new CloudDeploymentDto();

        dto.Id.Should().Be(0);
        dto.Name.Should().BeEmpty();
        dto.Status.Should().Be("Pending");
    }

    [Fact]
    public void CloudDeploymentDto_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;
        var dto = new CloudDeploymentDto
        {
            Id = 1,
            Name = "Production Deployment",
            Provider = "Azure",
            Environment = "Production",
            Region = "East US",
            Status = "Running",
            Version = "v2.1.0",
            CreatedAt = now.AddDays(-30),
            LastDeployedAt = now.AddDays(-1),
            HealthStatus = "Healthy",
            ResourceGroup = "crm-prod-rg",
            SubscriptionId = "sub-123",
            TenantId = "tenant-456"
        };

        dto.Name.Should().Be("Production Deployment");
        dto.Provider.Should().Be("Azure");
        dto.Environment.Should().Be("Production");
        dto.Status.Should().Be("Running");
        dto.HealthStatus.Should().Be("Healthy");
    }
}

/// <summary>
/// Tests for UpdateUserDto.
/// </summary>
public class UpdateUserDtoTests
{
    [Fact]
    public void UpdateUserDto_DefaultValues_AreCorrect()
    {
        var dto = new UpdateUserDto();

        dto.FirstName.Should().BeNull();
        dto.LastName.Should().BeNull();
        dto.Email.Should().BeNull();
        dto.DepartmentId.Should().BeNull();
        dto.IsActive.Should().BeNull();
    }

    [Fact]
    public void UpdateUserDto_PartialUpdate_OnlyChangedFields()
    {
        var dto = new UpdateUserDto
        {
            FirstName = "NewFirstName",
            IsActive = false
            // Only updating FirstName and IsActive
        };

        dto.FirstName.Should().Be("NewFirstName");
        dto.LastName.Should().BeNull(); // Not changed
        dto.Email.Should().BeNull(); // Not changed
        dto.IsActive.Should().BeFalse();
    }
}
