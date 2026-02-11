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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities.ITSM;

public enum CIType
{
    Server = 1,
    WorkStation = 2,
    NetworkDevice = 3,
    Application = 4,
    Database = 5,
    Storage = 6,
    VirtualMachine = 7,
    BusinessService = 8,
    ITService = 9,
    Software = 10,
    License = 11,
    Documentation = 12
}

public enum OperationalStatus
{
    Operational = 1,
    NonOperational = 2,
    UnderRepair = 3,
    Retired = 4,
    Disposed = 5,
    InStock = 6
}

public enum CIEnvironment
{
    Production = 1,
    Development = 2,
    Test = 3,
    Staging = 4,
    DisasterRecovery = 5
}

public enum CICriticality
{
    BusinessCritical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

public class ConfigurationItem
{
    [Key]
    public int CIId { get; set; }

    [Required]
    [StringLength(200)]
    public string CIName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string CINumber { get; set; } = string.Empty;

    [Required]
    public CIType CIType { get; set; }

    [StringLength(50)]
    public string? CISubtype { get; set; }

    public string? Description { get; set; }

    // Identification
    [StringLength(100)]
    public string? SerialNumber { get; set; }

    [StringLength(100)]
    public string? AssetTag { get; set; }

    [StringLength(100)]
    public string? ModelNumber { get; set; }

    [StringLength(200)]
    public string? Manufacturer { get; set; }

    [StringLength(50)]
    public string? Version { get; set; }

    // Ownership
    public int? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    public int? SupportGroupId { get; set; }

    [ForeignKey(nameof(SupportGroupId))]
    public UserGroup? SupportGroup { get; set; }

    public int? ManagedById { get; set; }

    [ForeignKey(nameof(ManagedById))]
    public User? ManagedBy { get; set; }

    public int? DepartmentId { get; set; }

    // Department FK can be added if needed

    // Status
    [Required]
    public OperationalStatus OperationalStatus { get; set; }

    public CIEnvironment? Environment { get; set; }

    public CICriticality? Criticality { get; set; }

    // Location
    [StringLength(500)]
    public string? PhysicalLocation { get; set; }

    public int? DataCenterId { get; set; }

    [StringLength(100)]
    public string? RackLocation { get; set; }

    // Financial
    public DateTime? PurchaseDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PurchaseCost { get; set; }

    public int? VendorId { get; set; }

    // Vendor FK can be added if needed

    public DateTime? WarrantyExpiration { get; set; }

    public DateTime? LeaseExpiration { get; set; }

    // Technical Details
    [StringLength(50)]
    public string? IPAddress { get; set; }

    [StringLength(50)]
    public string? MACAddress { get; set; }

    [StringLength(200)]
    public string? OperatingSystem { get; set; }

    [StringLength(100)]
    public string? CPU { get; set; }

    [StringLength(100)]
    public string? RAM { get; set; }

    [StringLength(100)]
    public string? Disk { get; set; }

    public DateTime? LastDiscovered { get; set; }

    // Extended Attributes (JSON)
    public string? ExtendedAttributes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<CIRelationship>? ParentRelationships { get; set; }

    public ICollection<CIRelationship>? ChildRelationships { get; set; }

    public ICollection<ServiceCI>? Services { get; set; }
}

public enum RelationshipType
{
    RunsOn = 1,
    DependsOn = 2,
    ConnectedTo = 3,
    InstalledOn = 4,
    Uses = 5,
    MemberOf = 6,
    HostedBy = 7,
    Contains = 8
}

public class CIRelationship
{
    [Key]
    public int RelationshipId { get; set; }

    [Required]
    public int ParentCIId { get; set; }

    [ForeignKey(nameof(ParentCIId))]
    public ConfigurationItem? ParentCI { get; set; }

    [Required]
    public int ChildCIId { get; set; }

    [ForeignKey(nameof(ChildCIId))]
    public ConfigurationItem? ChildCI { get; set; }

    [Required]
    public RelationshipType RelationshipType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedById { get; set; }

    [ForeignKey(nameof(CreatedById))]
    public User? CreatedBy { get; set; }

    public bool IsDeleted { get; set; } = false;
}

public enum ServiceType
{
    BusinessService = 1,
    ITService = 2,
    TechnicalService = 3,
    ApplicationService = 4
}

public class Service
{
    [Key]
    public int ServiceId { get; set; }

    [Required]
    [StringLength(200)]
    public string ServiceName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? ServiceNumber { get; set; }

    public string? Description { get; set; }

    public ServiceType ServiceType { get; set; }

    // Ownership
    public int? OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public User? Owner { get; set; }

    public int? TechnicalOwnerId { get; set; }

    [ForeignKey(nameof(TechnicalOwnerId))]
    public User? TechnicalOwner { get; set; }

    public int? SupportGroupId { get; set; }

    [ForeignKey(nameof(SupportGroupId))]
    public UserGroup? SupportGroup { get; set; }

    public CICriticality? Criticality { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? AvailabilityTarget { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    // Navigation Properties
    public ICollection<ServiceCI>? ConfigurationItems { get; set; }
}

public enum DependencyType
{
    Direct = 1,
    Indirect = 2
}

public class ServiceCI
{
    [Key]
    public int ServiceCIId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public Service? Service { get; set; }

    [Required]
    public int CIId { get; set; }

    [ForeignKey(nameof(CIId))]
    public ConfigurationItem? ConfigurationItem { get; set; }

    public DependencyType DependencyType { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}
