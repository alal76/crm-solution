using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    public partial class AddIsDeletedToEntityTagsAndCustomFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsDeleted and UpdatedAt to EntityTags (table created earlier without BaseEntity columns)
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EntityTags",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "EntityTags",
                type: "datetime(6)",
                nullable: true);

            // Add IsDeleted and UpdatedAt to CustomFields as well
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomFields",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CustomFields",
                type: "datetime(6)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "IsDeleted", table: "EntityTags");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "EntityTags");

            migrationBuilder.DropColumn(name: "IsDeleted", table: "CustomFields");
            migrationBuilder.DropColumn(name: "UpdatedAt", table: "CustomFields");
        }
    }
}
