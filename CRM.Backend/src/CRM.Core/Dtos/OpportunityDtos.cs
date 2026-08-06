// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
#pragma warning disable SA1649 // file name should match first type name
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos
{
    /// <summary>
    /// Data Transfer Object for Opportunity.
    /// </summary>
    public class OpportunityDto
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public int Stage { get; set; } // OpportunityStage enum (int)

        /// <summary>Human-readable label for the current stage (e.g. "Closed Won").</summary>
        public string? StageName { get; set; }

        /// <summary>Configurable stage FK (ENUM-MIG-005)</summary>
        public int? StageId { get; set; }

        [Range(0, 100)]
        public int Probability { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";
        public string? ExpectedCloseDate { get; set; } // ISO 8601 string
        public int PricingModel { get; set; } // OpportunityPricingModel enum (int)

        /// <summary>Human-readable label for the pricing model (e.g. "Subscription").</summary>
        public string? PricingModelName { get; set; }

        [Range(1, 120)]
        public int TermLengthMonths { get; set; }
        [MaxLength(4000)]
        public string? SolutionNotes { get; set; }
        public int? QualificationReason { get; set; } // QualificationReason enum (int)
        [MaxLength(4000)]
        public string? QualificationNotes { get; set; }
        [MaxLength(100)]
        public string? Region { get; set; }
        public int AccountId { get; set; }

        /// <summary>Display name of the linked Account (populated when navigation property is loaded).</summary>
        public string? AccountName { get; set; }

        public int? PrimaryContactId { get; set; }

        /// <summary>Full name of the primary contact (populated when navigation property is loaded).</summary>
        public string? PrimaryContactName { get; set; }

        public int? SalesOwnerId { get; set; }
        public string? SalesOwnerName { get; set; }
        public int? LeadId { get; set; }
        public List<OpportunityProductDto> Products { get; set; } = new();
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public byte[]? RowVersion { get; set; }
        public decimal WeightedAmount { get; set; }

        /// <summary>
        /// Probability-adjusted value (Amount * Probability / 100).
        /// Convenience alias matching the entity computed property name.
        /// </summary>
        public decimal WeightedValue { get; set; }

        public bool IsOpen { get; set; }
        public bool IsWon { get; set; }

        /// <summary>Forecast category for pipeline management (ForecastCategory enum, int)</summary>
        public int ForecastCategory { get; set; }

        /// <summary>Reason category for lost deals (LossReasonCategory enum, int, nullable)</summary>
        public int? LossReasonCategory { get; set; }

        [MaxLength(2000)]
        public string? LossReason { get; set; }

        /// <summary>ID of competitor who won (if lost to competition)</summary>
        public int? CompetitorWinnerId { get; set; }

        [MaxLength(4000)]
        public string? WinLossNotes { get; set; }

        /// <summary>Date when deal was won or lost (ISO 8601 string)</summary>
        public string? ClosedDate { get; set; }
    }
#pragma warning restore SA1649


    public class CreateOpportunityDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public int Stage { get; set; }
        /// <summary>Configurable stage FK (ENUM-MIG-005)</summary>
        public int? StageId { get; set; }
        [Range(0, 100)]
        public int Probability { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "USD";
        public string? ExpectedCloseDate { get; set; }
        public int PricingModel { get; set; }
        [Range(1, 120)]
        public int TermLengthMonths { get; set; }
        [MaxLength(4000)]
        public string? SolutionNotes { get; set; }
        public int? QualificationReason { get; set; }
        [MaxLength(4000)]
        public string? QualificationNotes { get; set; }
        [MaxLength(100)]
        public string? Region { get; set; }
        public int AccountId { get; set; }
        public int? PrimaryContactId { get; set; }
        public int? SalesOwnerId { get; set; }
        public int? LeadId { get; set; }
        public List<CreateOpportunityProductDto> Products { get; set; } = new();
    }

    /// <summary>
    /// For PATCH semantics, all fields nullable.
    /// </summary>
    public class UpdateOpportunityDto
    {
        public string? Name { get; set; }
        public int? Stage { get; set; }
        /// <summary>Configurable stage FK (ENUM-MIG-005)</summary>
        public int? StageId { get; set; }
        public int? Probability { get; set; }
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? ExpectedCloseDate { get; set; }
        public int? PricingModel { get; set; }
        public int? TermLengthMonths { get; set; }
        public string? SolutionNotes { get; set; }
        public int? QualificationReason { get; set; }
        public string? QualificationNotes { get; set; }
        public string? Region { get; set; }
        public int? AccountId { get; set; }
        public int? PrimaryContactId { get; set; }
        public int? SalesOwnerId { get; set; }
        public int? LeadId { get; set; }
        public List<UpdateOpportunityProductDto>? Products { get; set; }
    }



    public class OpportunityProductDto
    {
        public int OpportunityId { get; set; }
        public int ProductId { get; set; }

        /// <summary>Product name (populated when Product navigation property is loaded).</summary>
        public string? ProductName { get; set; }

        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        [Range(0.0, 100.0)]
        public decimal? DiscountPercent { get; set; }
        public decimal? LineTotal { get; set; }
        public decimal? TotalPrice { get; set; } // Alias for LineTotal
        public string? Notes { get; set; }
        public string? CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateOpportunityProductDto
    {
        [Required]
        public int ProductId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        [Range(0.0, 100.0)]
        public decimal? DiscountPercent { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateOpportunityProductDto
    {
        public int? OpportunityId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? DiscountPercent { get; set; }
        public string? Notes { get; set; }
        public bool? IsDeleted { get; set; }
    }

    // --- Team Member DTOs (TODO-CRM003-08) ---

    /// <summary>
    /// DTO for opportunity team member.
    /// </summary>
    public class OpportunityTeamMemberDto
    {
        public int Id { get; set; }
        public int OpportunityId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int Role { get; set; }
        public string? RoleName { get; set; }
        public decimal SplitPercentage { get; set; }
        public bool IsPrimary { get; set; }
        public int? CommissionPlanId { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a team member on an opportunity.
    /// </summary>
    public class CreateTeamMemberDto
    {
        [Required]
        public int UserId { get; set; }

        public int Role { get; set; } // OpportunityTeamRole enum

        [Range(0.0, 100.0)]
        public decimal SplitPercentage { get; set; }

        public bool IsPrimary { get; set; }
        public int? CommissionPlanId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating a team member on an opportunity.
    /// </summary>
    public class UpdateTeamMemberDto
    {
        public int? Role { get; set; }
        public decimal? SplitPercentage { get; set; }
        public bool? IsPrimary { get; set; }
        public int? CommissionPlanId { get; set; }
        public string? Notes { get; set; }
    }

    // --- Opportunity Competitor DTOs (TODO-CRM003-03) ---

    /// <summary>
    /// DTO for a competitor linked to an opportunity.
    /// </summary>
    public class OpportunityCompetitorDto
    {
        public int OpportunityId { get; set; }
        public int CompetitorId { get; set; }
        public string? CompetitorName { get; set; }
        public string? ThreatLevel { get; set; }
        public string? Status { get; set; }
        public decimal? CompetitorPrice { get; set; }
        public bool WonAgainst { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for adding a competitor to an opportunity.
    /// </summary>
    public class CreateOpportunityCompetitorDto
    {
        [Required]
        public int CompetitorId { get; set; }

        public string? ThreatLevel { get; set; }
        public string? Status { get; set; }
        public decimal? CompetitorPrice { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating a competitor linked to an opportunity (TODO-CRM003-03).
    /// All fields are optional for PATCH-style semantics.
    /// </summary>
    public class UpdateOpportunityCompetitorDto
    {
        /// <summary>Threat level string (Unknown | Low | Medium | High | Critical).</summary>
        public string? ThreatLevel { get; set; }

        /// <summary>Status string (Identified | Active | Leading | Eliminated | Won).</summary>
        public string? Status { get; set; }

        /// <summary>Competitor's quoted price for this deal.</summary>
        public decimal? CompetitorPrice { get; set; }

        /// <summary>Deal-specific notes about this competitor.</summary>
        public string? Notes { get; set; }

        /// <summary>Whether we won against this competitor.</summary>
        public bool? WonAgainst { get; set; }
    }

    // --- Forecast Category DTOs (TODO-CRM003-07) ---

    /// <summary>
    /// Request body for PATCH /api/opportunities/{id}/forecast-category.
    /// </summary>
    public class ForecastCategoryPatchDto
    {
        /// <summary>Numeric value of the ForecastCategory enum.</summary>
        [Required]
        public int ForecastCategory { get; set; }
    }

    /// <summary>
    /// One row in the forecast summary — a single ForecastCategory bucket.
    /// </summary>
    public class ForecastCategoryLineDto
    {
        public int CategoryValue { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal WeightedAmount { get; set; }

        /// <summary>Monthly Recurring Revenue estimate (subscription deals only).</summary>
        public decimal Mrr { get; set; }

        /// <summary>Annual Recurring Revenue (MRR * 12).</summary>
        public decimal Arr { get; set; }
    }

    /// <summary>
    /// Forecast summary report (TODO-CRM003-07).
    /// </summary>
    public class ForecastSummaryDto
    {
        public List<ForecastCategoryLineDto> Categories { get; set; } = new();
        public decimal TotalPipelineAmount { get; set; }
        public decimal TotalWeightedAmount { get; set; }
        public decimal TotalMrr { get; set; }
        public decimal TotalArr { get; set; }
        public DateTime AsOf { get; set; }
    }
}
