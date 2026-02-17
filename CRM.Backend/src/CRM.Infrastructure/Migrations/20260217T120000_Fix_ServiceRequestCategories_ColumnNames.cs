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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations;

/// <summary>
/// Fix ServiceRequestCategories column names: rename old migration columns
/// (Icon, Color, SortOrder, SlaResponseHours, SlaResolutionHours, DefaultPriority)
/// to match new entity property names (IconName, ColorCode, DisplayOrder,
/// DefaultResponseTimeHours, DefaultResolutionTimeHours).
/// Uses MariaDB-safe conditional ALTER statements.
/// </summary>
public partial class Fix_ServiceRequestCategories_ColumnNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Rename old columns to new names if the old column exists.
        // MariaDB supports IF EXISTS in stored procedures; we use a safe approach.
        migrationBuilder.Sql(@"
            SET @dbname = DATABASE();

            -- Rename Icon -> IconName
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'Icon');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'IconName');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `Icon` `IconName` varchar(50) NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Rename Color -> ColorCode
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'Color');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'ColorCode');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `Color` `ColorCode` varchar(20) NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Rename SortOrder -> DisplayOrder
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'SortOrder');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DisplayOrder');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `SortOrder` `DisplayOrder` int NOT NULL DEFAULT 0',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Rename SlaResponseHours -> DefaultResponseTimeHours
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'SlaResponseHours');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DefaultResponseTimeHours');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `SlaResponseHours` `DefaultResponseTimeHours` int NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Rename SlaResolutionHours -> DefaultResolutionTimeHours
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'SlaResolutionHours');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DefaultResolutionTimeHours');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `SlaResolutionHours` `DefaultResolutionTimeHours` int NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Drop DefaultPriority column if it exists (removed from entity)
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DefaultPriority');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories DROP COLUMN `DefaultPriority`',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            -- Also rename CustomerId -> AccountId in ServiceRequests table if needed
            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequests' AND COLUMN_NAME = 'CustomerId');
            SET @new_col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequests' AND COLUMN_NAME = 'AccountId');
            SET @sql = IF(@col_exists > 0 AND @new_col_exists = 0,
                'ALTER TABLE ServiceRequests CHANGE COLUMN `CustomerId` `AccountId` int NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse the column renames
        migrationBuilder.Sql(@"
            SET @dbname = DATABASE();

            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'IconName');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `IconName` `Icon` varchar(50) NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'ColorCode');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `ColorCode` `Color` varchar(20) NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DisplayOrder');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `DisplayOrder` `SortOrder` int NOT NULL DEFAULT 0',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DefaultResponseTimeHours');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `DefaultResponseTimeHours` `SlaResponseHours` int NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;

            SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @dbname AND TABLE_NAME = 'ServiceRequestCategories' AND COLUMN_NAME = 'DefaultResolutionTimeHours');
            SET @sql = IF(@col_exists > 0,
                'ALTER TABLE ServiceRequestCategories CHANGE COLUMN `DefaultResolutionTimeHours` `SlaResolutionHours` int NULL',
                'SELECT 1');
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
        ");
    }
}
