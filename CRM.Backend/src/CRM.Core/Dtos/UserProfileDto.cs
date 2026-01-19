namespace CRM.Core.Dtos;

/// <summary>
/// DTO for UserProfile creation and updates
/// </summary>
public class CreateUserProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public List<string> AccessiblePages { get; set; } = new();

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
}

/// <summary>
/// DTO for UserProfile responses
/// </summary>
public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> AccessiblePages { get; set; } = new();

    public bool CanCreateCustomers { get; set; }
    public bool CanEditCustomers { get; set; }
    public bool CanDeleteCustomers { get; set; }

    public bool CanCreateOpportunities { get; set; }
    public bool CanEditOpportunities { get; set; }
    public bool CanDeleteOpportunities { get; set; }

    public bool CanCreateProducts { get; set; }
    public bool CanEditProducts { get; set; }
    public bool CanDeleteProducts { get; set; }

    public bool CanManageCampaigns { get; set; }
    public bool CanViewReports { get; set; }
    public bool CanManageUsers { get; set; }

    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
}
