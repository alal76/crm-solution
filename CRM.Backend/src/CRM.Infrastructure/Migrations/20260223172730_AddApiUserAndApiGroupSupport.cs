using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiUserAndApiGroupSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyCreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyExpiresAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyHash",
                table: "Users",
                type: "VARCHAR(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApiKeyLastUsedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApiKeyPrefix",
                table: "Users",
                type: "VARCHAR(12)",
                maxLength: 12,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApiUserDescription",
                table: "Users",
                type: "VARCHAR(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsApiUser",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApiGroup",
                table: "UserGroups",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiKeyHash",
                table: "Users",
                column: "ApiKeyHash");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsApiUser",
                table: "Users",
                column: "IsApiUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ApiKeyHash",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsApiUser",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiKeyCreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiKeyExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiKeyHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiKeyLastUsedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiKeyPrefix",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApiUserDescription",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsApiUser",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsApiGroup",
                table: "UserGroups");
        }
    }
}
