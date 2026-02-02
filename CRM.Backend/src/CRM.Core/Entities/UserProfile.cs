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

namespace CRM.Core.Entities;

/// <summary>
/// User profile for defining custom access permissions and page access
/// </summary>
public class UserProfile : BaseEntity
{
    public string Name { get; set; } = string.Empty; // e.g., "Sales Manager", "Support Agent"
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public bool IsActive { get; set; } = true;

    // Page access permissions (stored as JSON array of page identifiers)
    // Example: ["Dashboard", "Customers", "Opportunities", "Products"]
    public string AccessiblePages { get; set; } = "[]"; // JSON array

    // Feature permissions
    public bool CanCreateCustomers { get; set; } = false;
    public bool CanEditCustomers { get; set; } = false;
    public bool CanDeleteCustomers { get; set; } = false;

    public bool CanCreateOpportunities { get; set; } = false;
    public bool CanEditOpportunities { get; set; } = false;
    public bool CanDeleteOpportunities { get; set; } = false;

    public bool CanCreateProducts { get; set; } = false;
    public bool CanEditProducts { get; set; } = false;
    public bool CanDeleteProducts { get; set; } = false;

    public bool CanManageCampaigns { get; set; } = false;
    public bool CanViewReports { get; set; } = false;
    public bool CanManageUsers { get; set; } = false;

    // Navigation properties
    public virtual Department? Department { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
