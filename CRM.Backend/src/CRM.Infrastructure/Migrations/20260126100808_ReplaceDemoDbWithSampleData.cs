using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDemoDbWithSampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the old demo database columns
            migrationBuilder.DropColumn(
                name: "UseDemoDatabase",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "DemoDataSeeded",
                table: "SystemSettings");

            // Rename DemoDataLastSeeded to SampleDataLastSeeded
            migrationBuilder.RenameColumn(
                name: "DemoDataLastSeeded",
                table: "SystemSettings",
                newName: "SampleDataLastSeeded");

            // Add new SampleDataSeeded column
            migrationBuilder.AddColumn<bool>(
                name: "SampleDataSeeded",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop SampleDataSeeded column
            migrationBuilder.DropColumn(
                name: "SampleDataSeeded",
                table: "SystemSettings");

            // Rename SampleDataLastSeeded back to DemoDataLastSeeded
            migrationBuilder.RenameColumn(
                name: "SampleDataLastSeeded",
                table: "SystemSettings",
                newName: "DemoDataLastSeeded");

            // Add back the old demo database columns
            migrationBuilder.AddColumn<bool>(
                name: "UseDemoDatabase",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DemoDataSeeded",
                table: "SystemSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
