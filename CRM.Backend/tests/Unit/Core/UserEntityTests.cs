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

using CRM.Core.Entities;
using FluentAssertions;
using Xunit;

namespace CRM.Tests.Unit.Core;

/// <summary>
/// Unit tests for User and UserGroup entities.
/// Tests property initialization, default values, enums, and entity relationships.
/// </summary>
public class UserEntityTests
{
    #region UserRole Enum Tests

    public class UserRoleTests
    {
        [Fact]
        public void UserRole_Admin_ShouldBeZero()
        {
            // Assert
            ((int)UserRole.Admin).Should().Be(0);
        }

        [Fact]
        public void UserRole_Manager_ShouldBeOne()
        {
            // Assert
            ((int)UserRole.Manager).Should().Be(1);
        }

        [Fact]
        public void UserRole_Sales_ShouldBeTwo()
        {
            // Assert
            ((int)UserRole.Sales).Should().Be(2);
        }

        [Fact]
        public void UserRole_Support_ShouldBeThree()
        {
            // Assert
            ((int)UserRole.Support).Should().Be(3);
        }

        [Fact]
        public void UserRole_Guest_ShouldBeFour()
        {
            // Assert
            ((int)UserRole.Guest).Should().Be(4);
        }

        [Fact]
        public void UserRole_AllValues_ShouldBeDefinedAndUnique()
        {
            // Arrange
            var values = Enum.GetValues<UserRole>();

            // Assert
            values.Should().HaveCount(5);
            values.Distinct().Should().HaveCount(5);
        }

        [Theory]
        [InlineData(0, UserRole.Admin)]
        [InlineData(1, UserRole.Manager)]
        [InlineData(2, UserRole.Sales)]
        [InlineData(3, UserRole.Support)]
        [InlineData(4, UserRole.Guest)]
        public void UserRole_IntToEnum_ShouldConvertCorrectly(int value, UserRole expected)
        {
            // Act
            var role = (UserRole)value;

            // Assert
            role.Should().Be(expected);
        }

        [Fact]
        public void UserRole_Names_ShouldBeReadable()
        {
            // Assert
            Enum.GetName(UserRole.Admin).Should().Be("Admin");
            Enum.GetName(UserRole.Manager).Should().Be("Manager");
            Enum.GetName(UserRole.Sales).Should().Be("Sales");
            Enum.GetName(UserRole.Support).Should().Be("Support");
            Enum.GetName(UserRole.Guest).Should().Be("Guest");
        }
    }

    #endregion

    #region User Entity Tests

    public class UserTests
    {
        [Fact]
        public void User_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var user = new User();

            // Assert - Default string values
            user.Username.Should().BeEmpty();
            user.Email.Should().BeEmpty();
            user.FirstName.Should().BeEmpty();
            user.LastName.Should().BeEmpty();
            user.PasswordHash.Should().BeEmpty();
            user.ThemePreference.Should().Be("system");

            // Assert - Default bool values
            user.IsActive.Should().BeTrue();
            user.TwoFactorEnabled.Should().BeFalse();
            user.MustResetPassword.Should().BeFalse();
            user.PasswordNeverSet.Should().BeFalse();
            user.EmailVerified.Should().BeFalse();

            // Assert - Default int value
            user.Role.Should().Be(0); // Admin by default

            // Assert - Nullable values should be null
            user.LastLoginDate.Should().BeNull();
            user.TwoFactorSecret.Should().BeNull();
            user.BackupCodes.Should().BeNull();
            user.PasswordLastChangedAt.Should().BeNull();
            user.PasswordResetToken.Should().BeNull();
            user.PasswordResetTokenExpiry.Should().BeNull();
            user.EmailVerificationToken.Should().BeNull();
            user.DepartmentId.Should().BeNull();
            user.UserProfileId.Should().BeNull();
            user.ContactId.Should().BeNull();
            user.PrimaryGroupId.Should().BeNull();
            user.HeaderColor.Should().BeNull();
            user.PhotoUrl.Should().BeNull();
            user.Language.Should().BeNull();
            user.Timezone.Should().BeNull();
            user.DateFormat.Should().BeNull();
            user.TimeFormat.Should().BeNull();
            user.RowsPerPage.Should().BeNull();
            user.EmailNotifications.Should().BeNull();
            user.DesktopNotifications.Should().BeNull();
            user.CompactMode.Should().BeNull();

            // Assert - Navigation collections initialized
            user.GroupMemberships.Should().NotBeNull().And.BeEmpty();
            user.OAuthTokens.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void User_AuthenticationProperties_CanBeSet()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var user = new User
            {
                Username = "jdoe",
                Email = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = "$2a$12$hashed_password_here",
                Role = (int)UserRole.Sales
            };

            // Assert
            user.Username.Should().Be("jdoe");
            user.Email.Should().Be("john.doe@example.com");
            user.FirstName.Should().Be("John");
            user.LastName.Should().Be("Doe");
            user.PasswordHash.Should().StartWith("$2a$");
            user.Role.Should().Be(2);
        }

        [Fact]
        public void User_TwoFactorProperties_CanBeSet()
        {
            // Arrange & Act
            var user = new User
            {
                TwoFactorEnabled = true,
                TwoFactorSecret = "JBSWY3DPEHPK3PXP",
                BackupCodes = "[\"code1\",\"code2\",\"code3\"]"
            };

            // Assert
            user.TwoFactorEnabled.Should().BeTrue();
            user.TwoFactorSecret.Should().Be("JBSWY3DPEHPK3PXP");
            user.BackupCodes.Should().Contain("code1");
        }

        [Fact]
        public void User_PasswordManagement_CanBeSet()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expiry = now.AddHours(24);

            // Act
            var user = new User
            {
                PasswordLastChangedAt = now,
                MustResetPassword = true,
                PasswordNeverSet = false,
                PasswordResetToken = "reset_token_abc123",
                PasswordResetTokenExpiry = expiry
            };

            // Assert
            user.PasswordLastChangedAt.Should().Be(now);
            user.MustResetPassword.Should().BeTrue();
            user.PasswordNeverSet.Should().BeFalse();
            user.PasswordResetToken.Should().Be("reset_token_abc123");
            user.PasswordResetTokenExpiry.Should().Be(expiry);
        }

        [Fact]
        public void User_RefreshTokenProperties_CanBeSet()
        {
            // Arrange
            var expiry = DateTime.UtcNow.AddDays(7);

            // Act
            var user = new User();
            var token = new RefreshToken
            {
                Token = "refresh_token_xyz789",
                UserId = 1,
                ExpiresAt = expiry
            };
            user.RefreshTokens.Add(token);

            // Assert
            user.RefreshTokens.Should().HaveCount(1);
            user.RefreshTokens.First().Token.Should().Be("refresh_token_xyz789");
            user.RefreshTokens.First().ExpiresAt.Should().Be(expiry);
        }

        [Fact]
        public void User_CustomizationProperties_CanBeSet()
        {
            // Arrange & Act
            var user = new User
            {
                HeaderColor = "#FF5733",
                PhotoUrl = "https://example.com/photos/user.jpg",
                ThemePreference = "dark",
                Language = "es",
                Timezone = "America/Los_Angeles",
                DateFormat = "DD/MM/YYYY",
                TimeFormat = "24h",
                RowsPerPage = 50,
                EmailNotifications = true,
                DesktopNotifications = false,
                CompactMode = true
            };

            // Assert
            user.HeaderColor.Should().Be("#FF5733");
            user.PhotoUrl.Should().Be("https://example.com/photos/user.jpg");
            user.ThemePreference.Should().Be("dark");
            user.Language.Should().Be("es");
            user.Timezone.Should().Be("America/Los_Angeles");
            user.DateFormat.Should().Be("DD/MM/YYYY");
            user.TimeFormat.Should().Be("24h");
            user.RowsPerPage.Should().Be(50);
            user.EmailNotifications.Should().BeTrue();
            user.DesktopNotifications.Should().BeFalse();
            user.CompactMode.Should().BeTrue();
        }

        [Fact]
        public void User_DepartmentAndProfile_CanBeSet()
        {
            // Arrange & Act
            var user = new User
            {
                DepartmentId = 10,
                UserProfileId = 20,
                ContactId = 30,
                PrimaryGroupId = 40
            };

            // Assert
            user.DepartmentId.Should().Be(10);
            user.UserProfileId.Should().Be(20);
            user.ContactId.Should().Be(30);
            user.PrimaryGroupId.Should().Be(40);
        }

        [Fact]
        public void User_InheritsFromBaseEntity()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var rowVersion = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            // Act
            var user = new User
            {
                Id = 42,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now,
                IsDeleted = false,
                RowVersion = rowVersion
            };

            // Assert
            user.Id.Should().Be(42);
            user.CreatedAt.Should().Be(now.AddDays(-30));
            user.UpdatedAt.Should().Be(now);
            user.IsDeleted.Should().BeFalse();
            user.RowVersion.Should().BeEquivalentTo(rowVersion);
        }

        [Fact]
        public void User_RoleRangeValidation_ShouldAcceptValidRoles()
        {
            // Arrange - The Range attribute is [0, 4]
            var user = new User();

            // Act & Assert - Valid roles
            user.Role = 0; user.Role.Should().Be(0);
            user.Role = 1; user.Role.Should().Be(1);
            user.Role = 2; user.Role.Should().Be(2);
            user.Role = 3; user.Role.Should().Be(3);
            user.Role = 4; user.Role.Should().Be(4);
        }
    }

    #endregion

    #region PasswordExpirationPolicy Enum Tests

    public class PasswordExpirationPolicyTests
    {
        [Fact]
        public void PasswordExpirationPolicy_None_ShouldBeZero()
        {
            // Assert
            ((int)PasswordExpirationPolicy.None).Should().Be(0);
        }

        [Fact]
        public void PasswordExpirationPolicy_MustChange_ShouldBeOne()
        {
            // Assert
            ((int)PasswordExpirationPolicy.MustChange).Should().Be(1);
        }

        [Fact]
        public void PasswordExpirationPolicy_Alert_ShouldBeTwo()
        {
            // Assert
            ((int)PasswordExpirationPolicy.Alert).Should().Be(2);
        }

        [Fact]
        public void PasswordExpirationPolicy_Warn_ShouldBeThree()
        {
            // Assert
            ((int)PasswordExpirationPolicy.Warn).Should().Be(3);
        }

        [Fact]
        public void PasswordExpirationPolicy_AllValues_ShouldBeDefinedAndUnique()
        {
            // Arrange
            var values = Enum.GetValues<PasswordExpirationPolicy>();

            // Assert
            values.Should().HaveCount(4);
            values.Distinct().Should().HaveCount(4);
        }
    }

    #endregion

    #region UserGroup Entity Tests

    public class UserGroupTests
    {
        [Fact]
        public void UserGroup_DefaultValues_ShouldBeCorrect()
        {
            // Arrange & Act
            var group = new UserGroup();

            // Assert - Basic info defaults
            group.Name.Should().BeEmpty();
            group.Description.Should().BeEmpty();
            group.IsActive.Should().BeTrue();
            group.IsDefault.Should().BeFalse();
            group.DisplayOrder.Should().Be(0);
            group.HeaderColor.Should().Be("#6750A4");

            // Assert - Admin flag default
            group.IsSystemAdmin.Should().BeFalse();

            // Assert - Menu permissions all false by default (except Dashboard)
            group.CanAccessDashboard.Should().BeTrue();
            group.CanAccessAccounts.Should().BeFalse();
            group.CanAccessContacts.Should().BeFalse();
            group.CanAccessLeads.Should().BeFalse();
            group.CanAccessOpportunities.Should().BeFalse();
            group.CanAccessProducts.Should().BeFalse();
            group.CanAccessServices.Should().BeFalse();
            group.CanAccessCampaigns.Should().BeFalse();
            group.CanAccessQuotes.Should().BeFalse();
            group.CanAccessTasks.Should().BeFalse();
            group.CanAccessActivities.Should().BeFalse();
            group.CanAccessNotes.Should().BeFalse();
            group.CanAccessWorkflows.Should().BeFalse();
            group.CanAccessServiceRequests.Should().BeFalse();
            group.CanAccessITSM.Should().BeFalse();
            group.CanAccessReports.Should().BeFalse();
            group.CanAccessSettings.Should().BeFalse();
            group.CanAccessUserManagement.Should().BeFalse();

            // Assert - JSON defaults
            group.AccessibleMenuItems.Should().Be("[]");

            // Assert - Data access defaults
            group.DataAccessScope.Should().Be("own");
            group.CanExportData.Should().BeFalse();
            group.CanImportData.Should().BeFalse();
            group.CanBulkEdit.Should().BeFalse();
            group.CanBulkDelete.Should().BeFalse();

            // Assert - Security policy defaults
            group.PasswordExpirationDays.Should().BeNull();
            group.PasswordExpirationPolicy.Should().Be(PasswordExpirationPolicy.None);
            group.PasswordExpirationWarningDays.Should().Be(7);
            group.RequireTwoFactor.Should().BeFalse();
            group.EnforceTwoFactor.Should().BeFalse();

            // Assert - Navigation collections initialized
            group.Members.Should().NotBeNull().And.BeEmpty();
            group.PrimaryUsers.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void UserGroup_CRUDPermissions_DefaultToFalse()
        {
            // Arrange & Act
            var group = new UserGroup();

            // Assert - Account CRUD
            group.CanCreateAccounts.Should().BeFalse();
            group.CanEditAccounts.Should().BeFalse();
            group.CanDeleteAccounts.Should().BeFalse();
            group.CanViewAllAccounts.Should().BeFalse();

            // Assert - Contact CRUD
            group.CanCreateContacts.Should().BeFalse();
            group.CanEditContacts.Should().BeFalse();
            group.CanDeleteContacts.Should().BeFalse();

            // Assert - Lead CRUD
            group.CanCreateLeads.Should().BeFalse();
            group.CanEditLeads.Should().BeFalse();
            group.CanDeleteLeads.Should().BeFalse();
            group.CanConvertLeads.Should().BeFalse();

            // Assert - Opportunity CRUD
            group.CanCreateOpportunities.Should().BeFalse();
            group.CanEditOpportunities.Should().BeFalse();
            group.CanDeleteOpportunities.Should().BeFalse();
            group.CanCloseOpportunities.Should().BeFalse();

            // Assert - Product CRUD
            group.CanCreateProducts.Should().BeFalse();
            group.CanEditProducts.Should().BeFalse();
            group.CanDeleteProducts.Should().BeFalse();
            group.CanManagePricing.Should().BeFalse();

            // Assert - Campaign CRUD
            group.CanCreateCampaigns.Should().BeFalse();
            group.CanEditCampaigns.Should().BeFalse();
            group.CanDeleteCampaigns.Should().BeFalse();
            group.CanLaunchCampaigns.Should().BeFalse();

            // Assert - Quote CRUD
            group.CanCreateQuotes.Should().BeFalse();
            group.CanEditQuotes.Should().BeFalse();
            group.CanDeleteQuotes.Should().BeFalse();
            group.CanApproveQuotes.Should().BeFalse();

            // Assert - Task CRUD
            group.CanCreateTasks.Should().BeFalse();
            group.CanEditTasks.Should().BeFalse();
            group.CanDeleteTasks.Should().BeFalse();
            group.CanAssignTasks.Should().BeFalse();

            // Assert - Workflow CRUD
            group.CanCreateWorkflows.Should().BeFalse();
            group.CanEditWorkflows.Should().BeFalse();
            group.CanDeleteWorkflows.Should().BeFalse();
            group.CanActivateWorkflows.Should().BeFalse();
        }

        [Fact]
        public void UserGroup_AdminGroup_CanBeConfigured()
        {
            // Arrange & Act
            var adminGroup = new UserGroup
            {
                Name = "System Administrators",
                Description = "Full system access",
                IsActive = true,
                IsSystemAdmin = true,
                HeaderColor = "#FF0000",
                DataAccessScope = "all",
                // Enable all menu access
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
                CanAccessUserManagement = true,
                // Enable all CRUD
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanDeleteAccounts = true,
                CanViewAllAccounts = true,
                // Enable data operations
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true
            };

            // Assert
            adminGroup.IsSystemAdmin.Should().BeTrue();
            adminGroup.DataAccessScope.Should().Be("all");
            adminGroup.CanAccessSettings.Should().BeTrue();
            adminGroup.CanAccessUserManagement.Should().BeTrue();
            adminGroup.CanBulkDelete.Should().BeTrue();
        }

        [Fact]
        public void UserGroup_SalesGroup_CanBeConfigured()
        {
            // Arrange & Act
            var salesGroup = new UserGroup
            {
                Name = "Sales Team",
                Description = "Sales users with customer and opportunity access",
                IsActive = true,
                IsSystemAdmin = false,
                DataAccessScope = "team",
                // Sales-relevant menu access
                CanAccessDashboard = true,
                CanAccessAccounts = true,
                CanAccessContacts = true,
                CanAccessLeads = true,
                CanAccessOpportunities = true,
                CanAccessQuotes = true,
                CanAccessProducts = true,
                CanAccessTasks = true,
                CanAccessActivities = true,
                CanAccessNotes = true,
                // Sales-relevant CRUD
                CanCreateAccounts = true,
                CanEditAccounts = true,
                CanViewAllAccounts = false, // Only see own accounts
                CanCreateContacts = true,
                CanEditContacts = true,
                CanCreateLeads = true,
                CanEditLeads = true,
                CanConvertLeads = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanCloseOpportunities = true,
                CanCreateQuotes = true,
                CanEditQuotes = true,
                // Data operations
                CanExportData = true
            };

            // Assert
            salesGroup.IsSystemAdmin.Should().BeFalse();
            salesGroup.DataAccessScope.Should().Be("team");
            salesGroup.CanAccessSettings.Should().BeFalse();
            salesGroup.CanDeleteAccounts.Should().BeFalse();
            salesGroup.CanConvertLeads.Should().BeTrue();
            salesGroup.CanCloseOpportunities.Should().BeTrue();
            salesGroup.CanExportData.Should().BeTrue();
            salesGroup.CanBulkDelete.Should().BeFalse();
        }

        [Fact]
        public void UserGroup_SecurityPolicy_CanBeConfigured()
        {
            // Arrange & Act
            var secureGroup = new UserGroup
            {
                Name = "Finance Team",
                PasswordExpirationDays = 90,
                PasswordExpirationPolicy = PasswordExpirationPolicy.MustChange,
                PasswordExpirationWarningDays = 14,
                RequireTwoFactor = true,
                EnforceTwoFactor = true
            };

            // Assert
            secureGroup.PasswordExpirationDays.Should().Be(90);
            secureGroup.PasswordExpirationPolicy.Should().Be(PasswordExpirationPolicy.MustChange);
            secureGroup.PasswordExpirationWarningDays.Should().Be(14);
            secureGroup.RequireTwoFactor.Should().BeTrue();
            secureGroup.EnforceTwoFactor.Should().BeTrue();
        }

        [Fact]
        public void UserGroup_InheritsFromBaseEntity()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            var group = new UserGroup
            {
                Id = 99,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now,
                IsDeleted = false
            };

            // Assert
            group.Id.Should().Be(99);
            group.CreatedAt.Should().Be(now.AddDays(-60));
            group.UpdatedAt.Should().Be(now);
            group.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void UserGroup_AccessibleMenuItems_CanStoreJson()
        {
            // Arrange & Act
            var group = new UserGroup
            {
                AccessibleMenuItems = "[\"Dashboard\",\"Customers\",\"Contacts\",\"Opportunities\"]"
            };

            // Assert
            group.AccessibleMenuItems.Should().Contain("Dashboard");
            group.AccessibleMenuItems.Should().Contain("Customers");
        }

        [Theory]
        [InlineData("own")]
        [InlineData("team")]
        [InlineData("all")]
        public void UserGroup_DataAccessScope_AcceptsValidValues(string scope)
        {
            // Arrange & Act
            var group = new UserGroup { DataAccessScope = scope };

            // Assert
            group.DataAccessScope.Should().Be(scope);
        }
    }

    #endregion

    #region User-UserGroup Integration Tests

    public class UserGroupIntegrationTests
    {
        [Fact]
        public void User_WithPrimaryGroup_SimulatesRelationship()
        {
            // Arrange
            var group = new UserGroup
            {
                Id = 1,
                Name = "Sales Team",
                IsSystemAdmin = false,
                CanAccessAccounts = true,
                CanAccessOpportunities = true
            };

            var user = new User
            {
                Id = 100,
                Username = "salesperson",
                Email = "sales@example.com",
                FirstName = "Sales",
                LastName = "Person",
                PrimaryGroupId = group.Id,
                PrimaryGroup = group
            };

            // Assert
            user.PrimaryGroupId.Should().Be(1);
            user.PrimaryGroup.Should().NotBeNull();
            user.PrimaryGroup!.Name.Should().Be("Sales Team");
            user.PrimaryGroup.CanAccessAccounts.Should().BeTrue();
        }

        [Fact]
        public void User_AdminRole_ShouldHaveSystemAdminGroup()
        {
            // Arrange - Typical admin user setup
            var adminGroup = new UserGroup
            {
                Id = 1,
                Name = "System Administrators",
                IsSystemAdmin = true,
                DataAccessScope = "all",
                CanAccessSettings = true,
                CanAccessUserManagement = true
            };

            var adminUser = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@crm.local",
                FirstName = "System",
                LastName = "Administrator",
                Role = (int)UserRole.Admin,
                PrimaryGroupId = adminGroup.Id,
                PrimaryGroup = adminGroup,
                IsActive = true,
                EmailVerified = true
            };

            // Assert
            adminUser.Role.Should().Be(0);
            adminUser.PrimaryGroup!.IsSystemAdmin.Should().BeTrue();
            adminUser.PrimaryGroup.DataAccessScope.Should().Be("all");
        }

        [Fact]
        public void UserGroup_WithMembers_SimulatesManyToManyRelationship()
        {
            // Arrange
            var group = new UserGroup
            {
                Id = 2,
                Name = "Marketing Team"
            };

            var user1 = new User { Id = 10, Username = "marketer1" };
            var user2 = new User { Id = 11, Username = "marketer2" };

            var membership1 = new UserGroupMember { UserId = 10, UserGroupId = 2 };
            var membership2 = new UserGroupMember { UserId = 11, UserGroupId = 2 };

            // Add to collections
            group.Members.Add(membership1);
            group.Members.Add(membership2);
            user1.GroupMemberships.Add(membership1);
            user2.GroupMemberships.Add(membership2);

            // Assert
            group.Members.Should().HaveCount(2);
            user1.GroupMemberships.Should().HaveCount(1);
            user2.GroupMemberships.Should().HaveCount(1);
        }

        [Fact]
        public void User_PasswordExpiration_WithGroupPolicy_SimulatesFlow()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var group = new UserGroup
            {
                PasswordExpirationDays = 90,
                PasswordExpirationPolicy = PasswordExpirationPolicy.Warn,
                PasswordExpirationWarningDays = 14
            };

            var user = new User
            {
                Username = "user1",
                PasswordLastChangedAt = now.AddDays(-80), // 80 days ago
                PrimaryGroup = group
            };

            // Act - Calculate days until expiration
            var daysSinceChange = (now - user.PasswordLastChangedAt!.Value).TotalDays;
            var daysUntilExpiry = group.PasswordExpirationDays!.Value - daysSinceChange;
            var shouldWarn = daysUntilExpiry <= group.PasswordExpirationWarningDays;

            // Assert
            daysSinceChange.Should().BeApproximately(80, 1);
            daysUntilExpiry.Should().BeApproximately(10, 1);
            shouldWarn.Should().BeTrue(); // 10 days left, warning threshold is 14
        }

        [Fact]
        public void User_TwoFactorRequired_WithGroupPolicy()
        {
            // Arrange
            var group = new UserGroup
            {
                RequireTwoFactor = true,
                EnforceTwoFactor = true
            };

            var userWithout2FA = new User
            {
                Username = "user_no_2fa",
                TwoFactorEnabled = false,
                PrimaryGroup = group
            };

            var userWith2FA = new User
            {
                Username = "user_with_2fa",
                TwoFactorEnabled = true,
                TwoFactorSecret = "SECRET123",
                PrimaryGroup = group
            };

            // Assert
            userWithout2FA.TwoFactorEnabled.Should().BeFalse();
            userWithout2FA.PrimaryGroup!.EnforceTwoFactor.Should().BeTrue();
            // In real app, this user would be prompted to set up 2FA

            userWith2FA.TwoFactorEnabled.Should().BeTrue();
            userWith2FA.TwoFactorSecret.Should().NotBeNullOrEmpty();
        }
    }

    #endregion

    #region Scenario Tests

    public class UserScenarioTests
    {
        [Fact]
        public void NewUserCreation_Scenario()
        {
            // Scenario: Admin creates a new sales user

            // Arrange
            var now = DateTime.UtcNow;
            var salesGroup = new UserGroup
            {
                Id = 3,
                Name = "Sales",
                CanAccessAccounts = true,
                CanAccessOpportunities = true,
                CanCreateOpportunities = true,
                CanEditOpportunities = true
            };

            // Act - Create new user
            var newUser = new User
            {
                Username = "new.salesperson",
                Email = "new.sales@example.com",
                FirstName = "New",
                LastName = "Salesperson",
                PasswordHash = "$2a$12$...", // BCrypt hash
                Role = (int)UserRole.Sales,
                PrimaryGroupId = salesGroup.Id,
                PrimaryGroup = salesGroup,
                IsActive = true,
                EmailVerified = false,
                PasswordNeverSet = true, // First-time login required
                MustResetPassword = false,
                CreatedAt = now
            };

            // Assert
            newUser.PasswordNeverSet.Should().BeTrue();
            newUser.EmailVerified.Should().BeFalse();
            newUser.IsActive.Should().BeTrue();
            newUser.Role.Should().Be((int)UserRole.Sales);
        }

        [Fact]
        public void UserLogin_Scenario()
        {
            // Scenario: User successfully logs in

            // Arrange
            var loginTime = DateTime.UtcNow;
            var refreshExpiry = loginTime.AddDays(7);

            var user = new User
            {
                Id = 50,
                Username = "testuser",
                Email = "test@example.com",
                IsActive = true,
                EmailVerified = true,
                LastLoginDate = loginTime.AddDays(-1) // Last login was yesterday
            };

            // Act - Simulate login
            user.LastLoginDate = loginTime;
            var refreshToken = new RefreshToken
            {
                Token = "new_refresh_token_abc123",
                UserId = user.Id,
                ExpiresAt = refreshExpiry
            };
            user.RefreshTokens.Add(refreshToken);

            // Assert
            user.LastLoginDate.Should().Be(loginTime);
            user.RefreshTokens.Should().HaveCount(1);
            user.RefreshTokens.First().Token.Should().NotBeNullOrEmpty();
            user.RefreshTokens.First().ExpiresAt.Should().BeAfter(loginTime);
        }

        [Fact]
        public void UserPasswordReset_Scenario()
        {
            // Scenario: User requests password reset

            // Arrange
            var now = DateTime.UtcNow;
            var tokenExpiry = now.AddHours(24);

            var user = new User
            {
                Id = 60,
                Username = "forgotpw",
                Email = "forgot@example.com",
                PasswordResetToken = null,
                PasswordResetTokenExpiry = null
            };

            // Act - Generate reset token
            user.PasswordResetToken = Guid.NewGuid().ToString("N");
            user.PasswordResetTokenExpiry = tokenExpiry;

            // Assert
            user.PasswordResetToken.Should().NotBeNullOrEmpty();
            user.PasswordResetToken.Should().HaveLength(32);
            user.PasswordResetTokenExpiry.Should().Be(tokenExpiry);
        }

        [Fact]
        public void UserPasswordChange_Scenario()
        {
            // Scenario: User changes password

            // Arrange
            var now = DateTime.UtcNow;
            var user = new User
            {
                Id = 70,
                Username = "changepw",
                PasswordHash = "$2a$12$old_hash",
                PasswordLastChangedAt = now.AddDays(-100),
                MustResetPassword = true,
                PasswordResetToken = "some_token"
            };

            // Act - Change password
            user.PasswordHash = "$2a$12$new_hash";
            user.PasswordLastChangedAt = now;
            user.MustResetPassword = false;
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Assert
            user.PasswordHash.Should().Contain("new_hash");
            user.PasswordLastChangedAt.Should().Be(now);
            user.MustResetPassword.Should().BeFalse();
            user.PasswordResetToken.Should().BeNull();
        }
    }

    #endregion
}
