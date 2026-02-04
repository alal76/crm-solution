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

namespace CRM.Infrastructure.Migrations
{
    /// <summary>
    /// Adds one-to-many relationship between Customer and Contact entities
    /// - Adds CustomerId FK to Contacts table
    /// - Adds index for faster lookups
    /// </summary>
    public partial class AddCustomerContactRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add CustomerId column to Contacts table for one-to-many relationship
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Contacts",
                type: "int",
                nullable: true);

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Contacts_Customers_CustomerId",
                table: "Contacts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Add index for faster lookups
            migrationBuilder.CreateIndex(
                name: "IX_Contacts_CustomerId",
                table: "Contacts",
                column: "CustomerId");

            // Add foreign key for CustomerContacts -> Contacts relationship
            // (if not already exists - this links the junction table)
            migrationBuilder.Sql(@"
                -- Check if FK exists before adding
                SET @dbname = DATABASE();
                SET @tablename = 'CustomerContacts';
                SET @constraintname = 'FK_CustomerContacts_Contacts_ContactId';

                SET @preparedStatement = (SELECT IF(
                    (SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                     WHERE CONSTRAINT_SCHEMA = @dbname
                     AND TABLE_NAME = @tablename
                     AND CONSTRAINT_NAME = @constraintname) = 0,
                    'ALTER TABLE CustomerContacts ADD CONSTRAINT FK_CustomerContacts_Contacts_ContactId FOREIGN KEY (ContactId) REFERENCES Contacts(Id) ON DELETE CASCADE',
                    'SELECT 1'
                ));

                PREPARE stmt FROM @preparedStatement;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Create index on CustomerContacts.ContactId for performance
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS IX_CustomerContacts_ContactId ON CustomerContacts(ContactId);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the index
            migrationBuilder.DropIndex(
                name: "IX_Contacts_CustomerId",
                table: "Contacts");

            // Drop the foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_Contacts_Customers_CustomerId",
                table: "Contacts");

            // Drop the column
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Contacts");
        }
    }
}
