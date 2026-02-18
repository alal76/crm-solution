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

using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for subscription metrics
/// Provides revenue metrics for subscription-based businesses (MRR, ARR, churn, etc.)
/// All monetary values are decimal[18,4] for precision
/// </summary>
public class SubscriptionMetricsDto
{
    /// <summary>Unique identifier</summary>
    public int Id { get; set; }

    /// <summary>Subscription ID being measured</summary>
    public int SubscriptionId { get; set; }

    /// <summary>Subscription number for display</summary>
    public string? SubscriptionNumber { get; set; }

    /// <summary>Related account ID</summary>
    public int? AccountId { get; set; }

    /// <summary>Related account name</summary>
    public string? AccountName { get; set; }

    /// <summary>Monthly Recurring Revenue (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal MRR { get; set; }

    /// <summary>Annual Recurring Revenue (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal ARR { get; set; }

    /// <summary>Churn rate (percentage, 0-100)</summary>
    [Range(0, 100)]
    public decimal ChurnRate { get; set; }

    /// <summary>Net revenue retention (percentage, 0-200)</summary>
    [Range(0, 200)]
    public decimal NRR { get; set; }

    /// <summary>Gross revenue retention (percentage, 0-100)</summary>
    [Range(0, 100)]
    public decimal GRR { get; set; }

    /// <summary>Customer acquisition cost (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal CAC { get; set; }

    /// <summary>Customer lifetime value (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal CLV { get; set; }

    /// <summary>Expansion revenue (decimal[18,4]) - new revenue from existing customers</summary>
    [Range(0, 999999999.99999)]
    public decimal ExpansionRevenue { get; set; }

    /// <summary>Contraction/downgrade revenue (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal ContractionRevenue { get; set; }

    /// <summary>Average Contract Value (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal ACV { get; set; }

    /// <summary>Payment processing fees (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal PaymentFees { get; set; }

    /// <summary>Refund amount during period (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal RefundAmount { get; set; }

    /// <summary>Billing cycle period (Monthly, Quarterly, Annual)</summary>
    public string BillingCycle { get; set; } = "Monthly";

    /// <summary>Measurement date</summary>
    public DateTime MeasurementDate { get; set; }

    /// <summary>Period start date</summary>
    public DateTime? PeriodStartDate { get; set; }

    /// <summary>Period end date</summary>
    public DateTime? PeriodEndDate { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Notes about metrics or anomalies</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Next billing date</summary>
    public DateTime? NextBillingDate { get; set; }

    /// <summary>Days until subscription expiry (-1 if no expiry)</summary>
    public int DaysUntilExpiry { get; set; }

    /// <summary>Subscription status</summary>
    public string? Status { get; set; }
}

/// <summary>
/// DTO for creating subscription metrics record
/// </summary>
public class CreateSubscriptionMetricsDto
{
    /// <summary>Subscription ID</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int SubscriptionId { get; set; }

    /// <summary>Monthly Recurring Revenue</summary>
    [Range(0, 999999999.99999)]
    public decimal MRR { get; set; }

    /// <summary>Annual Recurring Revenue</summary>
    [Range(0, 999999999.99999)]
    public decimal ARR { get; set; }

    /// <summary>Churn rate percentage</summary>
    [Range(0, 100)]
    public decimal ChurnRate { get; set; }

    /// <summary>Net revenue retention</summary>
    [Range(0, 200)]
    public decimal NRR { get; set; }

    /// <summary>Gross revenue retention</summary>
    [Range(0, 100)]
    public decimal GRR { get; set; }

    /// <summary>Measurement date</summary>
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;

    /// <summary>Period start</summary>
    public DateTime? PeriodStartDate { get; set; }

    /// <summary>Period end</summary>
    public DateTime? PeriodEndDate { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating subscription metrics
/// </summary>
public class UpdateSubscriptionMetricsDto
{
    /// <summary>Updated MRR</summary>
    [Range(0, 999999999.99999)]
    public decimal? MRR { get; set; }

    /// <summary>Updated ARR</summary>
    [Range(0, 999999999.99999)]
    public decimal? ARR { get; set; }

    /// <summary>Updated churn rate</summary>
    [Range(0, 100)]
    public decimal? ChurnRate { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// List DTO for subscription metrics (paginated)
/// </summary>
public class SubscriptionMetricsListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Subscription number</summary>
    public string? SubscriptionNumber { get; set; }

    /// <summary>Account name</summary>
    public string? AccountName { get; set; }

    /// <summary>MRR value</summary>
    public decimal MRR { get; set; }

    /// <summary>ARR value</summary>
    public decimal ARR { get; set; }

    /// <summary>Churn rate</summary>
    public decimal ChurnRate { get; set; }

    /// <summary>NRR percentage</summary>
    public decimal NRR { get; set; }

    /// <summary>Measurement date</summary>
    public DateTime MeasurementDate { get; set; }

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}
