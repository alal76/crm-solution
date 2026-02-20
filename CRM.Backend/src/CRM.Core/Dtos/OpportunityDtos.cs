
// ------------------------------------------------------------------------------
// <summary>
//   Data Transfer Objects (DTOs) for Opportunities in CRM Solution.
//   Copyright (c) 2026 CRM Solution Contributors. All rights reserved.
//   Licensed under the MIT License. See LICENSE in the project root for license information.
// </summary>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.DTOs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Data Transfer Object for Opportunity.
    /// </summary>
    public class OpportunityDto
    {
        public int Id { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public int Stage { get; set; } // OpportunityStage enum (int)
        [Range(0, 100)]
        public int Probability { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required, MaxLength(3)]
        public string Currency { get; set; } = "USD";
        public string? ExpectedCloseDate { get; set; } // ISO 8601 string
        public int PricingModel { get; set; } // OpportunityPricingModel enum (int)
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
        public int? PrimaryContactId { get; set; }
        public int? SalesOwnerId { get; set; }
        public string? SalesOwnerName { get; set; }
        public int? LeadId { get; set; }
        public List<OpportunityProductDto> Products { get; set; } = new();
        public string CreatedAt { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public byte[]? RowVersion { get; set; }
        public decimal WeightedAmount { get; set; }
        public bool IsOpen { get; set; }
        public bool IsWon { get; set; }
    }


    public class CreateOpportunityDto
    {
        [Required, MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public int Stage { get; set; }
        [Range(0, 100)]
        public int Probability { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }
        [Required, MaxLength(3)]
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
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        [Range(0, 100)]
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
        [Range(0, 100)]
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
}
