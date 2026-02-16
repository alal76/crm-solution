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
/// DTO for dunning history tracking
/// Records failed payment recovery attempts and dunning campaign lifecycle
/// Dunning = automated retry process for declined/failed payments
/// All monetary values are decimal[18,4] for precision
/// </summary>
public class DunningHistoryDto
{
    /// <summary>Unique identifier for dunning record</summary>
    public int Id { get; set; }

    /// <summary>Associated subscription ID</summary>
    public int? SubscriptionId { get; set; }

    /// <summary>Associated invoice ID</summary>
    public int? InvoiceId { get; set; }

    /// <summary>Associated account ID</summary>
    public int? AccountId { get; set; }

    /// <summary>Account name for display</summary>
    public string? AccountName { get; set; }

    /// <summary>Failed payment amount that triggered dunning (decimal[18,4])</summary>
    [Range(0, 999999999.99999)]
    public decimal FailedPaymentAmount { get; set; }

    /// <summary>Original failure date</summary>
    public DateTime FailureDate { get; set; }

    /// <summary>Original failure reason (Declined, Expired, InsufficientFunds, etc.)</summary>
    [StringLength(100)]
    public string? FailureReason { get; set; }

    /// <summary>Dunning campaign status (Active, Paused, Completed, Abandoned)</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Number of retry attempts made</summary>
    [Range(0, int.MaxValue)]
    public int RetryAttempts { get; set; }

    /// <summary>Maximum retry attempts allowed</summary>
    [Range(1, int.MaxValue)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Last retry attempt date</summary>
    public DateTime? LastRetryDate { get; set; }

    /// <summary>Next scheduled retry date</summary>
    public DateTime? NextRetryDate { get; set; }

    /// <summary>Date dunning campaign was abandoned</summary>
    public DateTime? AbandonedDate { get; set; }

    /// <summary>Reason for abandonment (MaxAttemptsReached, CustomerRequested, etc.)</summary>
    [StringLength(100)]
    public string? AbandonmentReason { get; set; }

    /// <summary>Date payment was finally recovered (if successful)</summary>
    public DateTime? RecoveredDate { get; set; }

    /// <summary>Amount recovered (if successful)</summary>
    [Range(0, 999999999.99999)]
    public decimal? RecoveredAmount { get; set; }

    /// <summary>Customer action taken (PaymentRetried, PaymentManuallyEntered, SubscriptionCancelled, etc.)</summary>
    [StringLength(100)]
    public string? CustomerAction { get; set; }

    /// <summary>Email notification count</summary>
    [Range(0, int.MaxValue)]
    public int EmailsNotificationsSent { get; set; }

    /// <summary>Notes about dunning process or customer communication</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Record creation timestamp</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Record last update timestamp</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Detailed retry history for this dunning campaign</summary>
    public List<DunningRetryAttemptDto> RetryAttemptHistory { get; set; } = new();
}

/// <summary>
/// DTO for individual dunning retry attempts
/// </summary>
public class DunningRetryAttemptDto
{
    /// <summary>Retry attempt number (1, 2, 3, etc.)</summary>
    public int AttemptNumber { get; set; }

    /// <summary>Attempt date/time</summary>
    public DateTime AttemptDate { get; set; }

    /// <summary>Payment method attempted (CreditCard, BankAccount, etc.)</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Result (Success, Declined, Failed, Pending)</summary>
    public string Result { get; set; } = "Pending";

    /// <summary>Response from payment processor</summary>
    [StringLength(500)]
    public string? ProcessorResponse { get; set; }

    /// <summary>Notes about this attempt</summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating dunning history record
/// </summary>
public class CreateDunningHistoryDto
{
    /// <summary>Subscription ID (if applicable)</summary>
    [Range(1, int.MaxValue)]
    public int? SubscriptionId { get; set; }

    /// <summary>Invoice ID</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int InvoiceId { get; set; }

    /// <summary>Failed payment amount</summary>
    [Required]
    [Range(0, 999999999.99999)]
    public decimal FailedPaymentAmount { get; set; }

    /// <summary>Failure reason</summary>
    [StringLength(100)]
    public string? FailureReason { get; set; }

    /// <summary>Max retry attempts</summary>
    [Range(1, int.MaxValue)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating dunning history
/// </summary>
public class UpdateDunningHistoryDto
{
    /// <summary>Updated status</summary>
    [StringLength(50)]
    public string? Status { get; set; }

    /// <summary>Next retry date</summary>
    public DateTime? NextRetryDate { get; set; }

    /// <summary>Abandonment reason</summary>
    [StringLength(100)]
    public string? AbandonmentReason { get; set; }

    /// <summary>Recovery amount (when payment succeeds)</summary>
    [Range(0, 999999999.99999)]
    public decimal? RecoveredAmount { get; set; }

    /// <summary>Customer action taken</summary>
    [StringLength(100)]
    public string? CustomerAction { get; set; }

    /// <summary>Notes</summary>
    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// List DTO for dunning history (paginated)
/// </summary>
public class DunningHistoryListDto
{
    /// <summary>Record ID</summary>
    public int Id { get; set; }

    /// <summary>Account name</summary>
    public string? AccountName { get; set; }

    /// <summary>Failed payment amount</summary>
    public decimal FailedPaymentAmount { get; set; }

    /// <summary>Failure date</summary>
    public DateTime FailureDate { get; set; }

    /// <summary>Status</summary>
    public string Status { get; set; } = "Active";

    /// <summary>Retry attempts</summary>
    public int RetryAttempts { get; set; }

    /// <summary>Max attempts</summary>
    public int MaxRetryAttempts { get; set; }

    /// <summary>Next retry date</summary>
    public DateTime? NextRetryDate { get; set; }

    /// <summary>Recovery date (if successful)</summary>
    public DateTime? RecoveredDate { get; set; }

    /// <summary>Creation date</summary>
    public DateTime CreatedAt { get; set; }
}
