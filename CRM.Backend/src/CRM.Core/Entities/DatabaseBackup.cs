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
/// Database backup record for tracking backups
/// </summary>
public class DatabaseBackup : BaseEntity
{
    public string BackupName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SourceDatabase { get; set; } = string.Empty; // MariaDB, PostgreSQL, etc.
    public string? BackupType { get; set; } = "Full"; // Full, Incremental, Differential
    public int? CreatedByUserId { get; set; }
    public string? Description { get; set; }
    public bool IsCompressed { get; set; } = true;
    public string? ChecksumHash { get; set; }

    // Navigation properties
    public virtual User? CreatedByUser { get; set; }
}
