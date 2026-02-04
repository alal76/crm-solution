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

﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations.Auto
{
    /// <inheritdoc />
    public partial class NormalizeRelationships_LeadCampaigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId2",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropIndex(
                name: "IX_Leads_MarketingCampaignId2",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId1",
                table: "Leads");

            migrationBuilder.DropColumn(
                name: "MarketingCampaignId2",
                table: "Leads");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId1",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MarketingCampaignId2",
                table: "Leads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId",
                table: "Leads",
                column: "MarketingCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId1",
                table: "Leads",
                column: "MarketingCampaignId1");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_MarketingCampaignId2",
                table: "Leads",
                column: "MarketingCampaignId2");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId",
                table: "Leads",
                column: "MarketingCampaignId",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId1",
                table: "Leads",
                column: "MarketingCampaignId1",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Leads_MarketingCampaigns_MarketingCampaignId2",
                table: "Leads",
                column: "MarketingCampaignId2",
                principalTable: "MarketingCampaigns",
                principalColumn: "Id");
        }
    }
}
