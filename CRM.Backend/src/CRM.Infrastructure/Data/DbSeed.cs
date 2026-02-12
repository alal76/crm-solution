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
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace CRM.Infrastructure.Data;

/// <summary>
/// Database seeding for initial admin user and test data
/// </summary>
public class DbSeed
{
    /// <summary>
    /// Hash a password using BCrypt
    /// </summary>
    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Seed initial admin user and sample data
    /// </summary>
    public static async Task SeedAsync(CrmDbContext context)
    {
        // Seed SysAdmin Group - always required for system administration
        var sysAdminGroup = await context.UserGroups.FirstOrDefaultAsync(g => g.Name == "SysAdmin");
        if (sysAdminGroup == null)
        {
            sysAdminGroup = new UserGroup
            {
                Name = "SysAdmin",
                Description = "System Administrators with full access to all features and settings",
                IsActive = true,
                IsDefault = false,
                IsSystemAdmin = true,
                DisplayOrder = 0,
                HeaderColor = "#DC2626", // Red for admin visibility
                // Menu/Page Access
                CanAccessDashboard = true,
                CanAccessCustomers = true,
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
                CanAccessReports = true,
                CanAccessSettings = true,
                CanAccessUserManagement = true,
                // Customer CRUD
                CanCreateCustomers = true,
                CanEditCustomers = true,
                CanDeleteCustomers = true,
                CanViewAllCustomers = true,
                // Contact CRUD
                CanCreateContacts = true,
                CanEditContacts = true,
                CanDeleteContacts = true,
                // Lead CRUD
                CanCreateLeads = true,
                CanEditLeads = true,
                CanDeleteLeads = true,
                CanConvertLeads = true,
                // Opportunity CRUD
                CanCreateOpportunities = true,
                CanEditOpportunities = true,
                CanDeleteOpportunities = true,
                CanCloseOpportunities = true,
                // Product CRUD
                CanCreateProducts = true,
                CanEditProducts = true,
                CanDeleteProducts = true,
                CanManagePricing = true,
                // Campaign CRUD
                CanCreateCampaigns = true,
                CanEditCampaigns = true,
                CanDeleteCampaigns = true,
                CanLaunchCampaigns = true,
                // Quote CRUD
                CanCreateQuotes = true,
                CanEditQuotes = true,
                CanDeleteQuotes = true,
                CanApproveQuotes = true,
                // Task CRUD
                CanCreateTasks = true,
                CanEditTasks = true,
                CanDeleteTasks = true,
                CanAssignTasks = true,
                // Workflow CRUD
                CanCreateWorkflows = true,
                CanEditWorkflows = true,
                CanDeleteWorkflows = true,
                CanActivateWorkflows = true,
                // Data Access
                DataAccessScope = "all",
                CanExportData = true,
                CanImportData = true,
                CanBulkEdit = true,
                CanBulkDelete = true,
                AccessibleMenuItems = "[\"Dashboard\",\"Customers\",\"Contacts\",\"Leads\",\"Opportunities\",\"Products\",\"Services\",\"Campaigns\",\"Quotes\",\"Tasks\",\"Activities\",\"Notes\",\"Workflows\",\"ServiceRequests\",\"Reports\",\"Settings\",\"UserManagement\",\"Admin\"]"
            };
            context.UserGroups.Add(sysAdminGroup);
            await context.SaveChangesAsync();
        }

        // Get admin credentials from environment variables (with defaults for development)
        var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            adminEmail = "admin@crm.local";
        }
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@123";
        var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME") ?? "System";
        var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LASTNAME") ?? "Administrator";

        // Seed Admin User - check by username OR email to avoid duplicates
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Username == adminUsername || u.Email == adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Username = adminUsername,
                Email = adminEmail,
                FirstName = adminFirstName,
                LastName = adminLastName,
                PasswordHash = HashPassword(adminPassword),
                Role = (int)UserRole.Admin,
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false,
                PrimaryGroupId = sysAdminGroup.Id
            };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
        else if (adminUser.PrimaryGroupId != sysAdminGroup.Id)
        {
            // Ensure existing admin user has SysAdmin as primary group
            adminUser.PrimaryGroupId = sysAdminGroup.Id;
            await context.SaveChangesAsync();
        }

        // Ensure admin user is a member of SysAdmin group
        var isMember = await context.UserGroupMembers
            .AnyAsync(m => m.UserId == adminUser.Id && m.UserGroupId == sysAdminGroup.Id);

        if (!isMember)
        {
            var membership = new UserGroupMember
            {
                UserId = adminUser.Id,
                UserGroupId = sysAdminGroup.Id,
                AddedAt = DateTime.UtcNow
            };
            context.UserGroupMembers.Add(membership);
            await context.SaveChangesAsync();
        }
    }
}
