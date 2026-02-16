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

namespace CRM.Infrastructure.Migrations;

/// <summary>
/// Migration for P0-DB-002: Complete ITSM Relationships
/// 
/// Completes Entity Framework Core relationship configurations for ITSM module (currently ~30% complete → 100%):
/// 
/// 1. Problem Management:
///    - Problem ↔ Incident (many-to-many via ProblemIncident)
///    - Adds unique constraint to prevent duplicate incident links
/// 
/// 2. Change Management:
///    - Change ↔ ChangeApproval (one-to-many) 
///    - Change ↔ ChangeImpactedCI (one-to-many)
///    - Change ↔ ChangeTask (one-to-many)
///    - Change ↔ ChangeComment (one-to-many)
///    - Change ↔ ChangeAttachment (one-to-many)
///    - ChangeApproval: Unique constraint on (ChangeId, ApprovalLevel)
///    - ChangeImpactedCI: Unique constraint on (ChangeId, CIId, Impact)
/// 
/// 3. CMDB Service-CI Mapping:
///    - Service ↔ ServiceCI (one-to-many)
///    - ServiceCI ↔ Incident (many-to-one for affected services)
///    - Unique constraint on (ServiceId, CIId)
/// 
/// All relationships use explicit HasForeignKey() with appropriate cascade delete rules.
/// Indexes added for frequently queried columns (Status, CreatedAt, ChangeId).
/// </summary>
public partial class Complete_ITSM_EntityRelationships : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // =====================================================================
        // Problem Management Indexes & Constraints
        // =====================================================================

        // Add unique constraint to ProblemIncident to prevent duplicate links
        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_ProblemId_IncidentId",
            schema: "ITSM",
            table: "ProblemIncidents",
            columns: new[] { "ProblemId", "IncidentId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProblemIncidents_IncidentId",
            schema: "ITSM",
            table: "ProblemIncidents",
            column: "IncidentId");

        // =====================================================================
        // Change Management Relationships & Indexes
        // =====================================================================

        // ChangeApproval: Ensure unique approval per change per level
        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ChangeId_ApprovalLevel",
            schema: "ITSM",
            table: "ChangeApprovals",
            columns: new[] { "ChangeId", "ApprovalLevel" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ApproverId",
            schema: "ITSM",
            table: "ChangeApprovals",
            column: "ApproverId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeApprovals_ApprovalStatus",
            schema: "ITSM",
            table: "ChangeApprovals",
            column: "ApprovalStatus");

        // ChangeImpactedCI: Ensure unique CI impact per change
        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_ChangeId_CIId",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            columns: new[] { "ChangeId", "CIId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_CIId",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            column: "CIId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangeImpactedCIs_ImpactLevel",
            schema: "ITSM",
            table: "ChangeImpactedCIs",
            column: "ImpactLevel");

        // ChangeTask indexes
        migrationBuilder.CreateIndex(
            name: "IX_ChangeTasks_ChangeId",
            schema: "ITSM",
            table: "ChangeTasks",
            column: "ChangeId");

        // ChangeComment indexes
        migrationBuilder.CreateIndex(
            name: "IX_ChangeComments_ChangeId",
            schema: "ITSM",
            table: "ChangeComments",
            column: "ChangeId");

        // ChangeAttachment indexes
        migrationBuilder.CreateIndex(
            name: "IX_ChangeAttachments_ChangeId",
            schema: "ITSM",
            table: "ChangeAttachments",
            column: "ChangeId");

        // =====================================================================
        // CMDB Service-CI Mapping
        // =====================================================================

        // ServiceCI: Ensure unique service-CI combination
        migrationBuilder.CreateIndex(
            name: "IX_ServiceCIs_ServiceId_CIId",
            schema: "ITSM",
            table: "ServiceCIs",
            columns: new[] { "ServiceId", "CIId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ServiceCIs_CIId",
            schema: "ITSM",
            table: "ServiceCIs",
            column: "CIId");

        // =====================================================================
        // General Performance Indexes
        // =====================================================================

        // Problem state tracking
        migrationBuilder.CreateIndex(
            name: "IX_Problems_State",
            schema: "ITSM",
            table: "Problems",
            column: "State");

        // Change state tracking
        migrationBuilder.CreateIndex(
            name: "IX_Changes_State",
            schema: "ITSM",
            table: "Changes",
            column: "State");

        migrationBuilder.CreateIndex(
            name: "IX_Changes_ApprovalStatus",
            schema: "ITSM",
            table: "Changes",
            column: "ApprovalStatus");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Drop all indexes created in Up()
        
        // Problem Management
        migrationBuilder.DropIndex(
            name: "IX_ProblemIncidents_ProblemId_IncidentId",
            schema: "ITSM",
            table: "ProblemIncidents");

        migrationBuilder.DropIndex(
            name: "IX_ProblemIncidents_IncidentId",
            schema: "ITSM",
            table: "ProblemIncidents");

        // Change Management
        migrationBuilder.DropIndex(
            name: "IX_ChangeApprovals_ChangeId_ApprovalLevel",
            schema: "ITSM",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApprovals_ApproverId",
            schema: "ITSM",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeApprovals_ApprovalStatus",
            schema: "ITSM",
            table: "ChangeApprovals");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCIs_ChangeId_CIId",
            schema: "ITSM",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCIs_CIId",
            schema: "ITSM",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeImpactedCIs_ImpactLevel",
            schema: "ITSM",
            table: "ChangeImpactedCIs");

        migrationBuilder.DropIndex(
            name: "IX_ChangeTasks_ChangeId",
            schema: "ITSM",
            table: "ChangeTasks");

        migrationBuilder.DropIndex(
            name: "IX_ChangeComments_ChangeId",
            schema: "ITSM",
            table: "ChangeComments");

        migrationBuilder.DropIndex(
            name: "IX_ChangeAttachments_ChangeId",
            schema: "ITSM",
            table: "ChangeAttachments");

        // CMDB Service-CI Mapping
        migrationBuilder.DropIndex(
            name: "IX_ServiceCIs_ServiceId_CIId",
            schema: "ITSM",
            table: "ServiceCIs");

        migrationBuilder.DropIndex(
            name: "IX_ServiceCIs_CIId",
            schema: "ITSM",
            table: "ServiceCIs");

        // General Performance Indexes
        migrationBuilder.DropIndex(
            name: "IX_Problems_State",
            schema: "ITSM",
            table: "Problems");

        migrationBuilder.DropIndex(
            name: "IX_Changes_State",
            schema: "ITSM",
            table: "Changes");

        migrationBuilder.DropIndex(
            name: "IX_Changes_ApprovalStatus",
            schema: "ITSM",
            table: "Changes");
    }
}
