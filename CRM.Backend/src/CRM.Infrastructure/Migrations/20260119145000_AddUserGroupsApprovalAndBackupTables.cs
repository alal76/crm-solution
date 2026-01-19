using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserGroupsApprovalAndBackupTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create UserGroups table
            migrationBuilder.CreateTable(
                name: "UserGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroups", x => x.Id);
                });

            // Create UserGroupMembers table
            migrationBuilder.CreateTable(
                name: "UserGroupMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserGroupId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGroupMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGroupMembers_UserGroups_UserGroupId",
                        column: x => x.UserGroupId,
                        principalTable: "UserGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGroupMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create UserApprovalRequests table
            migrationBuilder.CreateTable(
                name: "UserApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Company = table.Column<string>(type: "varchar(255)", nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    RejectionReason = table.Column<string>(type: "longtext", nullable: true),
                    AssignedUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserApprovalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserApprovalRequests_Users_AssignedUserId",
                        column: x => x.AssignedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserApprovalRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create DatabaseBackups table
            migrationBuilder.CreateTable(
                name: "DatabaseBackups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:AutoIncrement", true),
                    BackupName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "longtext", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SourceDatabase = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    BackupType = table.Column<string>(type: "varchar(50)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    IsCompressed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ChecksumHash = table.Column<string>(type: "varchar(255)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseBackups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DatabaseBackups_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_DatabaseBackups_CreatedByUserId",
                table: "DatabaseBackups",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApprovalRequests_AssignedUserId",
                table: "UserApprovalRequests",
                column: "AssignedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserApprovalRequests_ReviewedByUserId",
                table: "UserApprovalRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMembers_UserGroupId",
                table: "UserGroupMembers",
                column: "UserGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroupMembers_UserId",
                table: "UserGroupMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGroups_Name",
                table: "UserGroups",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DatabaseBackups");

            migrationBuilder.DropTable(
                name: "UserApprovalRequests");

            migrationBuilder.DropTable(
                name: "UserGroupMembers");

            migrationBuilder.DropTable(
                name: "UserGroups");
        }
    }
}
