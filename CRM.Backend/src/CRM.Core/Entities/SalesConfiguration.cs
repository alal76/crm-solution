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
