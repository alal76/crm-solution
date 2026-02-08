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

using System.ComponentModel.DataAnnotations.Schema;
/**
 * CRM Solution - Customer Relationship Management System
 * Copyright (C) 2024-2026 Abhishek Lal
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using CRM.Core.Models;

namespace CRM.Core.Entities;

#region Contract Enumerations

/// <summary>
/// Contract lifecycle status.
/// </summary>
public enum ContractStatus
{
    /// <summary>Contract is in draft stage</summary>
    Draft = 0,

    /// <summary>Contract pending approval</summary>
    PendingApproval = 1,

    /// <summary>Contract approved</summary>
    Approved = 2,

    /// <summary>Contract is active and in effect</summary>
    Active = 3,

    /// <summary>Contract has expired</summary>
    Expired = 4,

    /// <summary>Contract was terminated early</summary>
    Terminated = 5,

    /// <summary>Contract was renewed</summary>
    Renewed = 6,

    /// <summary>Contract is on hold</summary>
    OnHold = 7
}

/// <summary>
/// Type of contract.
/// </summary>
public enum ContractType
{
    /// <summary>Service contract</summary>
    Service = 0,

    /// <summary>License agreement</summary>
    License = 1,

    /// <summary>Subscription contract</summary>
    Subscription = 2,

    /// <summary>Support contract</summary>
    Support = 3,

    /// <summary>Maintenance contract</summary>
    Maintenance = 4,

    /// <summary>Non-disclosure agreement</summary>
    NDA = 5,

    /// <summary>Master service agreement</summary>
    Master = 6,

    /// <summary>Amendment to existing contract</summary>
    Amendment = 7,

    /// <summary>Other contract type</summary>
    Other = 8
}

#endregion

/// <summary>
/// Contract entity for managing customer agreements, service contracts, and legal documents.
/// </summary>
public class Contract : BaseEntity
{
    #region Identification

    /// <summary>Contract number (auto-generated)</summary>
    public string ContractNumber { get; set; } = $"CON-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}";

    /// <summary>Contract name/title</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Contract description</summary>
    public string? Description { get; set; }

    #endregion

    #region Status & Type

    /// <summary>Current contract status</summary>
    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    /// <summary>Type of contract</summary>
    public ContractType ContractType { get; set; } = ContractType.Service;

    #endregion

    #region Relationships

    /// <summary>Associated customer ID</summary>
    [Column("CustomerId")]
    public int AccountId { get; set; }

    /// <summary>Navigation to customer</summary>
    public Account? Account { get; set; }

    /// <summary>Primary contact ID</summary>
    public int? ContactId { get; set; }

    /// <summary>Navigation to contact</summary>
    public Contact? Contact { get; set; }

    /// <summary>Owner user ID</summary>
    public int? OwnerId { get; set; }

    /// <summary>Navigation to owner</summary>
    public User? Owner { get; set; }

    /// <summary>Parent contract ID (for amendments/renewals)</summary>
    public int? ParentContractId { get; set; }

    /// <summary>Navigation to parent contract</summary>
    public Contract? ParentContract { get; set; }

    /// <summary>Child contracts (amendments/renewals)</summary>
    public ICollection<Contract> ChildContracts { get; set; } = new List<Contract>();

    /// <summary>Related opportunity ID</summary>
    public int? OpportunityId { get; set; }

    /// <summary>Navigation to opportunity</summary>
    public Opportunity? Opportunity { get; set; }

    /// <summary>Related quote ID</summary>
    public int? QuoteId { get; set; }

    /// <summary>Navigation to quote</summary>
    public Quote? Quote { get; set; }

    #endregion

    #region Dates

    /// <summary>Contract start date</summary>
    public DateTime StartDate { get; set; }

    /// <summary>Contract end date</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Date contract was signed</summary>
    public DateTime? SignedDate { get; set; }

    /// <summary>Date activated</summary>
    public DateTime? ActivatedDate { get; set; }

    /// <summary>Date terminated</summary>
    public DateTime? TerminatedDate { get; set; }

    #endregion

    #region Financial

    /// <summary>Contract value/amount</summary>
    public decimal Value { get; set; }

    /// <summary>Currency code</summary>
    public string? CurrencyCode { get; set; } = "USD";

    /// <summary>Billing frequency</summary>
    public string? BillingFrequency { get; set; }

    #endregion

    #region Renewal Settings

    /// <summary>Auto-renew enabled</summary>
    public bool AutoRenew { get; set; } = false;

    /// <summary>Days before end to send renewal notice</summary>
    public int RenewalNoticeDays { get; set; } = 30;

    /// <summary>Renewal notice sent</summary>
    public bool RenewalNoticeSent { get; set; } = false;

    /// <summary>Date renewal notice was sent</summary>
    public DateTime? RenewalNoticeSentDate { get; set; }

    /// <summary>Date renewal was initiated</summary>
    public DateTime? RenewalInitiatedAt { get; set; }

    /// <summary>Date renewal was completed</summary>
    public DateTime? RenewalCompletedAt { get; set; }

    #endregion

    #region Terms & Conditions

    /// <summary>Contract terms and conditions</summary>
    public string? Terms { get; set; }

    /// <summary>Special conditions</summary>
    public string? SpecialConditions { get; set; }

    /// <summary>Termination clause</summary>
    public string? TerminationClause { get; set; }

    #endregion

    #region Documents

    /// <summary>Contract file URL</summary>
    public string? ContractFileUrl { get; set; }

    /// <summary>Contract file name</summary>
    public string? ContractFileName { get; set; }

    /// <summary>Contract file size in bytes</summary>
    public long? ContractFileSize { get; set; }

    /// <summary>Contract file MIME type</summary>
    public string? ContractFileMimeType { get; set; }

    /// <summary>Signed contract file URL</summary>
    public string? SignedContractFileUrl { get; set; }

    /// <summary>Signed contract file name</summary>
    public string? SignedContractFileName { get; set; }

    #endregion

    #region Approval

    /// <summary>Approved by user ID</summary>
    public int? ApprovedByUserId { get; set; }

    /// <summary>Navigation to approver</summary>
    public User? ApprovedByUser { get; set; }

    /// <summary>Date approved</summary>
    public DateTime? ApprovedDate { get; set; }

    /// <summary>Rejection reason</summary>
    public string? RejectionReason { get; set; }

    #endregion

    #region Suspension

    /// <summary>Reason for suspension</summary>
    public string? SuspensionReason { get; set; }

    /// <summary>Date suspended</summary>
    public DateTime? SuspendedDate { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>Days until expiration</summary>
    public int? DaysUntilExpiration => EndDate > DateTime.UtcNow
        ? (int)(EndDate - DateTime.UtcNow).TotalDays
        : null;

    /// <summary>Whether contract is expiring soon (within renewal notice days)</summary>
    public bool IsExpiringSoon => Status == ContractStatus.Active
        && DaysUntilExpiration.HasValue
        && DaysUntilExpiration.Value <= RenewalNoticeDays;

    #endregion

    #region Aliases

    /// <summary>Alias for Value for service compatibility</summary>
    public decimal TotalValue
    {
        get => Value;
        set => Value = value;
    }

    /// <summary>Alias for ActivatedDate for service compatibility</summary>
    public DateTime? ActivatedAt
    {
        get => ActivatedDate;
        set => ActivatedDate = value;
    }

    /// <summary>Alias for TerminatedDate for service compatibility</summary>
    public DateTime? TerminatedAt
    {
        get => TerminatedDate;
        set => TerminatedDate = value;
    }

    /// <summary>Alias for SignedDate for service compatibility</summary>
    public DateTime? SentForSignatureAt
    {
        get => SignedDate;
        set => SignedDate = value;
    }

    /// <summary>Termination reason for contract</summary>
    public string? TerminationReason { get; set; }

    /// <summary>Payment terms for contract</summary>
    public string? PaymentTerms { get; set; }

    /// <summary>Whether contract is signed</summary>
    public bool IsSigned => SignedDate.HasValue;

    /// <summary>Name of person who signed</summary>
    public string? SignedBy { get; set; }

    /// <summary>Alias for ContractFileUrl for service compatibility</summary>
    public string? DocumentUrl
    {
        get => ContractFileUrl;
        set => ContractFileUrl = value;
    }

    /// <summary>Alias for Terms for service compatibility</summary>
    public string? TermsAndConditions
    {
        get => Terms;
        set => Terms = value;
    }

    #endregion
}
