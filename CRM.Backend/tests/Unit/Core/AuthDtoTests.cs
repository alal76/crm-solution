// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using CRM.Core.Dtos;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for Authentication DTOs.
/// Tests LoginRequest, AuthResponse, UserPermissions, and GroupPermissionsDto.
/// </summary>
public class AuthDtoTests
{
    #region LoginRequest Tests

    public class LoginRequestTests
    {
        [Fact]
        public void LoginRequest_ShouldInitializeWithEmptyStrings()
        {
            // Act
            var request = new LoginRequest();

            // Assert
            request.Email.Should().Be(string.Empty);
            request.Password.Should().Be(string.Empty);
        }

        [Fact]
        public void LoginRequest_ShouldSetEmailProperty()
        {
            // Arrange
            var request = new LoginRequest();

            // Act
            request.Email = "user@example.com";

            // Assert
            request.Email.Should().Be("user@example.com");
        }

        [Fact]
        public void LoginRequest_ShouldSetPasswordProperty()
        {
            // Arrange
            var request = new LoginRequest();

            // Act
            request.Password = "SecurePassword123!";

            // Assert
            request.Password.Should().Be("SecurePassword123!");
        }

        [Fact]
        public void LoginRequest_ShouldAllowObjectInitializerSyntax()
        {
            // Act
            var request = new LoginRequest
            {
                Email = "admin@crm.local",
                Password = "Admin@123"
            };

            // Assert
            request.Email.Should().Be("admin@crm.local");
            request.Password.Should().Be("Admin@123");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("invalid-email")]
        [InlineData("@nodomain")]
        [InlineData("no@")]
        public void LoginRequest_ShouldAcceptAnyEmailFormat(string email)
        {
            // Note: Validation is not the responsibility of the DTO
            // Act
            var request = new LoginRequest { Email = email };

            // Assert
            request.Email.Should().Be(email);
        }
    }

    #endregion

    #region AuthResponse Tests

    public class AuthResponseTests
    {
        [Fact]
        public void AuthResponse_ShouldInitializeWithDefaults()
        {
            // Act
            var response = new AuthResponse();

            // Assert - Check all default values
            response.UserId.Should().Be(0);
            response.Username.Should().Be(string.Empty);
            response.Email.Should().Be(string.Empty);
            response.FirstName.Should().Be(string.Empty);
            response.LastName.Should().Be(string.Empty);
            response.Role.Should().Be(string.Empty);
            response.AccessToken.Should().Be(string.Empty);
            response.RefreshToken.Should().Be(string.Empty);
            response.ExpiresAt.Should().Be(default(DateTime));
            response.ThemePreference.Should().Be("system");
            response.RequiresTwoFactor.Should().BeFalse();
            response.TwoFactorEnabled.Should().BeFalse();
            response.RequiresPasswordSetup.Should().BeFalse();
            response.PasswordExpired.Should().BeFalse();
            response.PasswordExpirationWarning.Should().BeFalse();
            response.MustChangePassword.Should().BeFalse();
            response.RequiresApproval.Should().BeFalse();
        }

        [Fact]
        public void AuthResponse_ShouldInitializePermissionsWithNewInstance()
        {
            // Act
            var response = new AuthResponse();

            // Assert
            response.Permissions.Should().NotBeNull();
            response.AccessiblePages.Should().NotBeNull();
            response.AccessiblePages.Should().BeEmpty();
        }

        [Fact]
        public void AuthResponse_ShouldSetBasicUserInfo()
        {
            // Arrange & Act
            var response = new AuthResponse
            {
                UserId = 42,
                Username = "johndoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                Role = "Admin"
            };

            // Assert
            response.UserId.Should().Be(42);
            response.Username.Should().Be("johndoe");
            response.Email.Should().Be("john.doe@example.com");
            response.FirstName.Should().Be("John");
            response.LastName.Should().Be("Doe");
            response.Role.Should().Be("Admin");
        }

        [Fact]
        public void AuthResponse_ShouldSetTokens()
        {
            // Arrange
            var expiresAt = DateTime.UtcNow.AddHours(1);

            // Act
            var response = new AuthResponse
            {
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                RefreshToken = "refresh-token-abc123",
                ExpiresAt = expiresAt
            };

            // Assert
            response.AccessToken.Should().StartWith("eyJ");
            response.RefreshToken.Should().Be("refresh-token-abc123");
            response.ExpiresAt.Should().Be(expiresAt);
        }

        [Fact]
        public void AuthResponse_ShouldSetDepartmentInfo()
        {
            // Act
            var response = new AuthResponse
            {
                DepartmentId = 5,
                DepartmentName = "Sales"
            };

            // Assert
            response.DepartmentId.Should().Be(5);
            response.DepartmentName.Should().Be("Sales");
        }

        [Fact]
        public void AuthResponse_ShouldSetUserProfileInfo()
        {
            // Act
            var response = new AuthResponse
            {
                UserProfileId = 10,
                UserProfileName = "Sales Representative"
            };

            // Assert
            response.UserProfileId.Should().Be(10);
            response.UserProfileName.Should().Be("Sales Representative");
        }

        [Fact]
        public void AuthResponse_ShouldSetPrimaryGroupInfo()
        {
            // Act
            var response = new AuthResponse
            {
                PrimaryGroupId = 1,
                PrimaryGroupName = "SysAdmin"
            };

            // Assert
            response.PrimaryGroupId.Should().Be(1);
            response.PrimaryGroupName.Should().Be("SysAdmin");
        }

        [Fact]
        public void AuthResponse_ShouldSetAccessiblePages()
        {
            // Act
            var response = new AuthResponse
            {
                AccessiblePages = new List<string> { "Dashboard", "Customers", "Reports" }
            };

            // Assert
            response.AccessiblePages.Should().HaveCount(3);
            response.AccessiblePages.Should().Contain("Dashboard");
            response.AccessiblePages.Should().Contain("Customers");
            response.AccessiblePages.Should().Contain("Reports");
        }

        [Fact]
        public void AuthResponse_ShouldSetCustomizationOptions()
        {
            // Act
            var response = new AuthResponse
            {
                HeaderColor = "#1976d2",
                PhotoUrl = "https://example.com/photo.jpg",
                ThemePreference = "dark"
            };

            // Assert
            response.HeaderColor.Should().Be("#1976d2");
            response.PhotoUrl.Should().Be("https://example.com/photo.jpg");
            response.ThemePreference.Should().Be("dark");
        }

        [Fact]
        public void AuthResponse_ShouldSetTwoFactorProperties()
        {
            // Act
            var response = new AuthResponse
            {
                RequiresTwoFactor = true,
                TwoFactorEnabled = true,
                TwoFactorToken = "temp-2fa-token"
            };

            // Assert
            response.RequiresTwoFactor.Should().BeTrue();
            response.TwoFactorEnabled.Should().BeTrue();
            response.TwoFactorToken.Should().Be("temp-2fa-token");
        }

        [Fact]
        public void AuthResponse_ShouldSetPasswordExpirationProperties()
        {
            // Act
            var response = new AuthResponse
            {
                PasswordExpired = true,
                PasswordExpirationWarning = true,
                DaysUntilPasswordExpiration = 7,
                MustChangePassword = true,
                PasswordSetupToken = "password-reset-token"
            };

            // Assert
            response.PasswordExpired.Should().BeTrue();
            response.PasswordExpirationWarning.Should().BeTrue();
            response.DaysUntilPasswordExpiration.Should().Be(7);
            response.MustChangePassword.Should().BeTrue();
            response.PasswordSetupToken.Should().Be("password-reset-token");
        }

        [Fact]
        public void AuthResponse_ShouldSetRequiresPasswordSetup()
        {
            // Act
            var response = new AuthResponse
            {
                RequiresPasswordSetup = true,
                PasswordSetupToken = "first-login-token"
            };

            // Assert
            response.RequiresPasswordSetup.Should().BeTrue();
            response.PasswordSetupToken.Should().Be("first-login-token");
        }

        [Fact]
        public void AuthResponse_ShouldSetApprovalProperties()
        {
            // Act
            var response = new AuthResponse
            {
                RequiresApproval = true,
                Message = "Your registration is pending admin approval"
            };

            // Assert
            response.RequiresApproval.Should().BeTrue();
            response.Message.Should().Contain("pending");
        }

        [Fact]
        public void AuthResponse_ShouldSetGroupPermissions()
        {
            // Arrange
            var groupPermissions = new GroupPermissionsDto
            {
                IsSystemAdmin = true,
                CanAccessDashboard = true,
                CanAccessAccounts = true
            };

            // Act
            var response = new AuthResponse
            {
                GroupPermissions = groupPermissions
            };

            // Assert
            response.GroupPermissions.Should().NotBeNull();
            response.GroupPermissions!.IsSystemAdmin.Should().BeTrue();
        }

        [Theory]
        [InlineData("system")]
        [InlineData("light")]
        [InlineData("dark")]
        [InlineData("high-contrast")]
        public void AuthResponse_ShouldAcceptValidThemePreferences(string theme)
        {
            // Act
            var response = new AuthResponse { ThemePreference = theme };

            // Assert
            response.ThemePreference.Should().Be(theme);
        }
    }

    #endregion

    #region UserPermissions Tests

    public class UserPermissionsTests
    {
        [Fact]
        public void UserPermissions_ShouldInitializeWithFalseDefaults()
        {
            // Act
            var permissions = new UserPermissions();

            // Assert - All permissions should default to false
            permissions.CanCreateAccounts.Should().BeFalse();
            permissions.CanEditAccounts.Should().BeFalse();
            permissions.CanDeleteAccounts.Should().BeFalse();
            permissions.CanCreateOpportunities.Should().BeFalse();
            permissions.CanEditOpportunities.Should().BeFalse();
            permissions.CanDeleteOpportunities.Should().BeFalse();
            permissions.CanCreateProducts.Should().BeFalse();
            permissions.CanEditProducts.Should().BeFalse();
            permissions.CanDeleteProducts.Should().BeFalse();
            permissions.CanManageCampaigns.Should().BeFalse();
            permissions.CanViewReports.Should().BeFalse();
            permissions.CanManageUsers.Should().BeFalse();
        }

        [Fact]
        public void UserPermissions_ShouldSetCustomerPermissions()
        {
            // Act
            var permissions = new UserPermissions
            {
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = false
            };

            // Assert
            permissions.CanCreateAccounts.Should().BeTrue();
            permissions.CanEditAccounts.Should().BeTrue();
            permissions.CanDeleteAccounts.Should().BeFalse();
        }

        [Fact]
        public void UserPermissions_ShouldSetOpportunityPermissions()
        {
            // Act
            var permissions = new UserPermissions
            {
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true
            };

            // Assert
            permissions.CanCreateOpportunities.Should().BeTrue();
            permissions.CanEditOpportunities.Should().BeTrue();
            permissions.CanDeleteOpportunities.Should().BeTrue();
        }

        [Fact]
        public void UserPermissions_ShouldSetProductPermissions()
        {
            // Act
            var permissions = new UserPermissions
            {
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true
            };

            // Assert
            permissions.CanCreateProducts.Should().BeTrue();
            permissions.CanEditProducts.Should().BeTrue();
            permissions.CanDeleteProducts.Should().BeTrue();
        }

        [Fact]
        public void UserPermissions_ShouldSetAdminPermissions()
        {
            // Act
            var permissions = new UserPermissions
            {
                CanManageCampaigns = true,
                CanViewReports = true,
                CanManageUsers = true
            };

            // Assert
            permissions.CanManageCampaigns.Should().BeTrue();
            permissions.CanViewReports.Should().BeTrue();
            permissions.CanManageUsers.Should().BeTrue();
        }
    }

    #endregion

    #region GroupPermissionsDto Tests

    public class GroupPermissionsDtoTests
    {
        [Fact]
        public void GroupPermissionsDto_ShouldInitializeWithDefaults()
        {
            // Act
            var permissions = new GroupPermissionsDto();

            // Assert
            permissions.IsSystemAdmin.Should().BeFalse();
            permissions.CanAccessDashboard.Should().BeTrue(); // Dashboard defaults to true
            permissions.DataAccessScope.Should().Be("own");
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetMenuAccessPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanAccessDashboard = true,
                CanAccessAccounts = true,
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessProducts = true,
                CanAccessServices = true,
                CanAccessCampaigns = true,
                CanAccessQuotes = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                CanAccessWorkflows = true,
                CanAccessServiceRequests = true,
                CanAccessITSM = true,
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true
            };

            // Assert
            permissions.CanAccessDashboard.Should().BeTrue();
            permissions.CanAccessAccounts.Should().BeTrue();
            permissions.CanAccessContacts.Should().BeTrue();
            permissions.CanAccessLeads.Should().BeTrue();
            permissions.CanAccessOpportunities.Should().BeTrue();
            permissions.CanAccessProducts.Should().BeTrue();
            permissions.CanAccessServices.Should().BeTrue();
            permissions.CanAccessCampaigns.Should().BeTrue();
            permissions.CanAccessQuotes.Should().BeTrue();
            permissions.CanAccessTasks.Should().BeTrue();
            permissions.CanAccessActivities.Should().BeTrue();
            permissions.CanAccessNotes.Should().BeTrue();
            permissions.CanAccessWorkflows.Should().BeTrue();
            permissions.CanAccessServiceRequests.Should().BeTrue();
            permissions.CanAccessITSM.Should().BeTrue();
            permissions.CanAccessReports.Should().BeTrue();
            permissions.CanAccessSettings.Should().BeTrue();
            permissions.CanAccessUserManagement.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetCustomerCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = false,
                CanViewAllAccounts = true
            };

            // Assert
            permissions.CanCreateAccounts.Should().BeTrue();
            permissions.CanEditAccounts.Should().BeTrue();
            permissions.CanDeleteAccounts.Should().BeTrue();
            permissions.CanViewAllAccounts.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetContactCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateContacts = true,
                CanEditContacts = true,
                CanDeleteContacts = true
            };

            // Assert
            permissions.CanCreateContacts.Should().BeTrue();
            permissions.CanEditContacts.Should().BeTrue();
            permissions.CanDeleteContacts.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetLeadCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateLeads = true,
                CanEditLeads = true,
                CanDeleteLeads = true,
                CanConvertLeads = true
            };

            // Assert
            permissions.CanCreateLeads.Should().BeTrue();
            permissions.CanEditLeads.Should().BeTrue();
            permissions.CanDeleteLeads.Should().BeTrue();
            permissions.CanConvertLeads.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetOpportunityCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCloseOpportunities = true
            };

            // Assert
            permissions.CanCreateOpportunities.Should().BeTrue();
            permissions.CanEditOpportunities.Should().BeTrue();
            permissions.CanDeleteOpportunities.Should().BeTrue();
            permissions.CanCloseOpportunities.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetProductCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManagePricing = true
            };

            // Assert
            permissions.CanCreateProducts.Should().BeTrue();
            permissions.CanEditProducts.Should().BeTrue();
            permissions.CanDeleteProducts.Should().BeTrue();
            permissions.CanManagePricing.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetCampaignCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateCampaigns = true,
                CanEditCampaigns = true,
                CanDeleteCampaigns = true,
                CanLaunchCampaigns = true
            };

            // Assert
            permissions.CanCreateCampaigns.Should().BeTrue();
            permissions.CanEditCampaigns.Should().BeTrue();
            permissions.CanDeleteCampaigns.Should().BeTrue();
            permissions.CanLaunchCampaigns.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetQuoteCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateQuotes = true,
                CanEditQuotes = true,
                CanDeleteQuotes = true,
                CanApproveQuotes = true
            };

            // Assert
            permissions.CanCreateQuotes.Should().BeTrue();
            permissions.CanEditQuotes.Should().BeTrue();
            permissions.CanDeleteQuotes.Should().BeTrue();
            permissions.CanApproveQuotes.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetTaskCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateTasks = true,
                CanEditTasks = true,
                CanDeleteTasks = true,
                CanAssignTasks = true
            };

            // Assert
            permissions.CanCreateTasks.Should().BeTrue();
            permissions.CanEditTasks.Should().BeTrue();
            permissions.CanDeleteTasks.Should().BeTrue();
            permissions.CanAssignTasks.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetWorkflowCrudPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                CanCreateWorkflows = true,
                CanEditWorkflows = true,
                CanDeleteWorkflows = true,
                CanActivateWorkflows = true
            };

            // Assert
            permissions.CanCreateWorkflows.Should().BeTrue();
            permissions.CanEditWorkflows.Should().BeTrue();
            permissions.CanDeleteWorkflows.Should().BeTrue();
            permissions.CanActivateWorkflows.Should().BeTrue();
        }

        [Fact]
        public void GroupPermissionsDto_ShouldSetDataAccessPermissions()
        {
            // Act
            var permissions = new GroupPermissionsDto
            {
                DataAccessScope = "team",
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true
            };

            // Assert
            permissions.DataAccessScope.Should().Be("team");
            permissions.CanExportData.Should().BeTrue();
            permissions.CanImportData.Should().BeTrue();
            permissions.CanBulkEdit.Should().BeTrue();
            permissions.CanBulkDelete.Should().BeTrue();
        }

        [Theory]
        [InlineData("own")]
        [InlineData("team")]
        [InlineData("department")]
        [InlineData("all")]
        public void GroupPermissionsDto_ShouldAcceptValidDataAccessScopes(string scope)
        {
            // Act
            var permissions = new GroupPermissionsDto { DataAccessScope = scope };

            // Assert
            permissions.DataAccessScope.Should().Be(scope);
        }

        [Fact]
        public void GroupPermissionsDto_SystemAdmin_ShouldBeDistinguished()
        {
            // Act
            var permissions = new GroupPermissionsDto { IsSystemAdmin = true };

            // Assert
            permissions.IsSystemAdmin.Should().BeTrue();
        }
    }

    #endregion

    #region Integration Tests - Complete Auth Flow Simulation

    public class AuthFlowTests
    {
        [Fact]
        public void AuthFlow_NormalLogin_ShouldReturnCompleteResponse()
        {
            // Simulate a normal login flow
            var request = new LoginRequest
            {
                Email = "user@example.com",
                Password = "Password123!"
            };

            var response = new AuthResponse
            {
                UserId = 100,
                Username = "jsmith",
                Email = request.Email,
                FirstName = "John",
                LastName = "Smith",
                Role = "User",
                AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test",
                RefreshToken = "refresh-token-xyz",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                PrimaryGroupId = 2,
                PrimaryGroupName = "Standard Users",
                AccessiblePages = new List<string> { "Dashboard", "Contacts", "Leads" },
                Permissions = new UserPermissions
                {
                    CanCreateAccounts = true,
                    CanEditAccounts = true,
                    CanDeleteAccounts = false
                }
            };

            // Assert
            request.Email.Should().Be(response.Email);
            response.AccessToken.Should().NotBeEmpty();
            response.RefreshToken.Should().NotBeEmpty();
            response.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            response.RequiresTwoFactor.Should().BeFalse();
            response.RequiresPasswordSetup.Should().BeFalse();
        }

        [Fact]
        public void AuthFlow_TwoFactorRequired_ShouldReturnPartialResponse()
        {
            // Simulate a login that requires 2FA
            var response = new AuthResponse
            {
                UserId = 100,
                Email = "secure@example.com",
                RequiresTwoFactor = true,
                TwoFactorToken = "temp-2fa-abc123",
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };

            // Assert
            response.RequiresTwoFactor.Should().BeTrue();
            response.TwoFactorToken.Should().NotBeNullOrEmpty();
            response.AccessToken.Should().BeEmpty();
            response.RefreshToken.Should().BeEmpty();
        }

        [Fact]
        public void AuthFlow_FirstTimeLogin_ShouldRequirePasswordSetup()
        {
            // Simulate first-time login with no password set
            var response = new AuthResponse
            {
                UserId = 100,
                Email = "newuser@example.com",
                RequiresPasswordSetup = true,
                PasswordSetupToken = "setup-token-123",
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };

            // Assert
            response.RequiresPasswordSetup.Should().BeTrue();
            response.PasswordSetupToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void AuthFlow_PasswordExpired_ShouldForcePasswordChange()
        {
            // Simulate login with expired password
            var response = new AuthResponse
            {
                UserId = 100,
                Email = "expiredpassword@example.com",
                PasswordExpired = true,
                MustChangePassword = true,
                PasswordSetupToken = "reset-token-456",
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };

            // Assert
            response.PasswordExpired.Should().BeTrue();
            response.MustChangePassword.Should().BeTrue();
            response.PasswordSetupToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void AuthFlow_PasswordExpirationWarning_ShouldIncludeDaysRemaining()
        {
            // Simulate login with password expiration warning
            var response = new AuthResponse
            {
                UserId = 100,
                Email = "warninguser@example.com",
                PasswordExpirationWarning = true,
                DaysUntilPasswordExpiration = 5,
                AccessToken = "valid-token",
                RefreshToken = "valid-refresh"
            };

            // Assert
            response.PasswordExpirationWarning.Should().BeTrue();
            response.DaysUntilPasswordExpiration.Should().Be(5);
            response.AccessToken.Should().NotBeEmpty(); // Still gets tokens
        }

        [Fact]
        public void AuthFlow_PendingApproval_ShouldReturnApprovalRequired()
        {
            // Simulate registration pending approval
            var response = new AuthResponse
            {
                UserId = 100,
                Email = "pending@example.com",
                RequiresApproval = true,
                Message = "Your account is pending administrator approval. You will receive an email when approved.",
                AccessToken = string.Empty,
                RefreshToken = string.Empty
            };

            // Assert
            response.RequiresApproval.Should().BeTrue();
            response.Message.Should().Contain("pending");
            response.AccessToken.Should().BeEmpty();
        }

        [Fact]
        public void AuthFlow_AdminUser_ShouldHaveFullPermissions()
        {
            // Simulate admin login
            var response = new AuthResponse
            {
                UserId = 1,
                Username = "admin",
                Email = "admin@crm.local",
                Role = "Admin",
                AccessToken = "admin-token",
                RefreshToken = "admin-refresh",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                GroupPermissions = new GroupPermissionsDto
                {
                    IsSystemAdmin = true,
                    CanAccessDashboard = true,
                    CanAccessAccounts = true,
                    CanAccessSettings = true,
                    CanAccessUserManagement = true,
                    CanDeleteAccounts = true,
                    DataAccessScope = "all"
                }
            };

            // Assert
            response.Role.Should().Be("Admin");
            response.GroupPermissions!.IsSystemAdmin.Should().BeTrue();
            response.GroupPermissions.CanAccessSettings.Should().BeTrue();
            response.GroupPermissions.CanAccessUserManagement.Should().BeTrue();
            response.GroupPermissions.DataAccessScope.Should().Be("all");
        }
    }

    #endregion
}
