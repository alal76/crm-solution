using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailDigestConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailDigestConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    TimeOfDay = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Timezone = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IncludeNewLeads = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeOpenOpportunities = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeRecentActivities = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeUpcomingTasks = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeOverdueTasks = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeTeamPerformance = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IncludeKpiSummary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastSentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BINARY(8)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailDigestConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailDigestConfigs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EmailDigestConfigs_UserId",
                table: "EmailDigestConfigs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailDigestConfigs");
        }
    }
}
