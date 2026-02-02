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

using CRM.Core.Dtos;

namespace CRM.Core.Interfaces;

/// <summary>
/// Interface for database backup and migration operations
/// </summary>
public interface IDatabaseBackupService
{
    // Backup operations
    Task<DatabaseBackupDto> CreateBackupAsync(int createdByUserId, CreateDatabaseBackupRequest request);
    Task<IEnumerable<DatabaseBackupDto>> GetAllBackupsAsync();
    Task<DatabaseBackupDto?> GetBackupByIdAsync(int id);
    Task RestoreBackupAsync(int backupId, string targetDatabase, int performedByUserId);
    Task DeleteBackupAsync(int id);
    Task<byte[]> DownloadBackupAsync(int backupId);
    Task<DatabaseBackupDto> UploadBackupAsync(Stream fileStream, string fileName, int createdByUserId, UploadBackupRequest request);
    Task RestoreFromFileAsync(Stream fileStream, string fileName, int performedByUserId);

    // Schedule operations
    Task<IEnumerable<BackupScheduleDto>> GetAllSchedulesAsync();
    Task<BackupScheduleDto?> GetScheduleByIdAsync(int id);
    Task<BackupScheduleDto> CreateScheduleAsync(CreateBackupScheduleRequest request);
    Task<BackupScheduleDto> UpdateScheduleAsync(int id, CreateBackupScheduleRequest request);
    Task DeleteScheduleAsync(int id);
    Task<BackupScheduleDto> ToggleScheduleAsync(int id, bool enabled);
    Task RunScheduledBackupAsync(int scheduleId);

    // Settings
    Task<BackupSettingsDto> GetBackupSettingsAsync();
    Task UpdateBackupPathAsync(string path);

    // Migration
    Task MigrateDatabaseAsync(DatabaseMigrationConfig config, int performedByUserId);
    Task<string> GenerateSeedScriptAsync(string targetDatabase = "");
}
