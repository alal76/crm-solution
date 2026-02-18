// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Core.Entities;

/// <summary>
/// Sales module configuration settings
/// </summary>
[Table("SalesConfigurations")]
public class SalesConfiguration : BaseEntity
{
    [Required]
    [StringLength(255)]
    public string Key { get; set; }

    [Column(TypeName = "longtext")]
    public string Value { get; set; }

    [StringLength(255)]
    public string Description { get; set; }

    [StringLength(50)]
    public string DataType { get; set; } // string, integer, decimal, boolean, json

    public bool IsSystem { get; set; } // System settings cannot be deleted

    public bool IsActive { get; set; } = true;
}
