using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDuplicateCandidateAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "DuplicateCandidates",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                table: "DuplicateCandidates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateCandidates_AssignedToUserId",
                table: "DuplicateCandidates",
                column: "AssignedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DuplicateCandidates_Users_AssignedToUserId",
                table: "DuplicateCandidates",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DuplicateCandidates_Users_AssignedToUserId",
                table: "DuplicateCandidates");

            migrationBuilder.DropIndex(
                name: "IX_DuplicateCandidates_AssignedToUserId",
                table: "DuplicateCandidates");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "DuplicateCandidates");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                table: "DuplicateCandidates");
        }
    }
}
