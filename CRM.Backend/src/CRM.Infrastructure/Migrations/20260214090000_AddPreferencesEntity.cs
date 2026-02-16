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

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto;

public partial class AddPreferencesEntity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Preferences",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                OptInEmail = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                OptInSms = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                OptInPhone = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                OptInPostal = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                PreferredContactMethod = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                PreferredLanguage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                Timezone = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                DoNotCallDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                DoNotEmailDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Preferences", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Preferences_Composite",
            table: "Preferences",
            columns: new[] { "OptInEmail", "OptInSms", "OptInPhone", "OptInPostal", "PreferredContactMethod", "PreferredLanguage", "Timezone" },
            unique: true);

        migrationBuilder.AddColumn<int>(
            name: "PreferencesId",
            table: "Customers",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "PreferencesId",
            table: "Contacts",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "UseCustomPreferences",
            table: "Contacts",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_Customers_PreferencesId",
            table: "Customers",
            column: "PreferencesId");

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_PreferencesId",
            table: "Contacts",
            column: "PreferencesId");

        migrationBuilder.AddForeignKey(
            name: "FK_Customers_Preferences_PreferencesId",
            table: "Customers",
            column: "PreferencesId",
            principalTable: "Preferences",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_Contacts_Preferences_PreferencesId",
            table: "Contacts",
            column: "PreferencesId",
            principalTable: "Preferences",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Customers_Preferences_PreferencesId",
            table: "Customers");

        migrationBuilder.DropForeignKey(
            name: "FK_Contacts_Preferences_PreferencesId",
            table: "Contacts");

        migrationBuilder.DropIndex(
            name: "IX_Customers_PreferencesId",
            table: "Customers");

        migrationBuilder.DropIndex(
            name: "IX_Contacts_PreferencesId",
            table: "Contacts");

        migrationBuilder.DropColumn(
            name: "PreferencesId",
            table: "Customers");

        migrationBuilder.DropColumn(
            name: "PreferencesId",
            table: "Contacts");

        migrationBuilder.DropColumn(
            name: "UseCustomPreferences",
            table: "Contacts");

        migrationBuilder.DropTable(
            name: "Preferences");
    }
}
