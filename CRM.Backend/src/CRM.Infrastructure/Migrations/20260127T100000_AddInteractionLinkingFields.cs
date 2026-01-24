/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 * Licensed under the GNU Affero General Public License v3.0
 */

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <summary>
    /// Adds fields for interaction linking and expedite handling to ServiceRequests
    /// </summary>
    public partial class AddInteractionLinkingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add SourceInteractionId column
            migrationBuilder.AddColumn<int>(
                name: "SourceInteractionId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            // Add expedite handling columns
            migrationBuilder.AddColumn<bool>(
                name: "IsExpedited",
                table: "ServiceRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExpediteReason",
                table: "ServiceRequests",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ExpeditedByUserId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpeditedAt",
                table: "ServiceRequests",
                type: "datetime(6)",
                nullable: true);

            // Add foreign key for SourceInteractionId
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_SourceInteractionId",
                table: "ServiceRequests",
                column: "SourceInteractionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Interactions_SourceInteractionId",
                table: "ServiceRequests",
                column: "SourceInteractionId",
                principalTable: "Interactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Add foreign key for ExpeditedByUserId
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ExpeditedByUserId",
                table: "ServiceRequests",
                column: "ExpeditedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_Users_ExpeditedByUserId",
                table: "ServiceRequests",
                column: "ExpeditedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // Add index for IsExpedited
            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_IsExpedited",
                table: "ServiceRequests",
                column: "IsExpedited");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign keys first
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Users_ExpeditedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_Interactions_SourceInteractionId",
                table: "ServiceRequests");

            // Remove indexes
            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_IsExpedited",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ExpeditedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_SourceInteractionId",
                table: "ServiceRequests");

            // Remove columns
            migrationBuilder.DropColumn(
                name: "ExpeditedAt",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ExpeditedByUserId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ExpediteReason",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "IsExpedited",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "SourceInteractionId",
                table: "ServiceRequests");
        }
    }
}
