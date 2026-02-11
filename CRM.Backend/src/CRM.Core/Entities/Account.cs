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
using System.ComponentModel.DataAnnotations.Schema;
using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Account category - Individual or Organization
/// </summary>
public enum AccountCategory
{
    Individual = 0,
    Organization = 1
}

/// <summary>
/// Account lifecycle stage enumeration
/// Lifecycle Flow: Other (default) → Lead → Opportunity → Active → AtRisk → Churned → (Win-back) → Lead
/// </summary>
public enum AccountLifecycleStage
{
    /// <summary>Initial default value for new accounts</summary>
    Other = 0,

    /// <summary>A potential account showing interest</summary>
    Lead = 1,

    /// <summary>A qualified lead with an active sales opportunity</summary>
    Opportunity = 2,

    /// <summary>An active paying account (formerly Customer)</summary>
    Active = 3,

    /// <summary>An account at risk of churning</summary>
    AtRisk = 4,

    /// <summary>A former account who has stopped doing business</summary>
    Churned = 5,

    /// <summary>A churned account being re-engaged (transitions back to Lead)</summary>
    WinBack = 6
}

/// <summary>
/// Account type enumeration (size/classification)
/// </summary>
public enum AccountType
{
    Individual = 0,
    SmallBusiness = 1,
    MidMarket = 2,
    Enterprise = 3,
    Government = 4,
    NonProfit = 5
}

/// <summary>
/// Account priority level
/// </summary>
public enum AccountPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Account entity for managing account information
/// Supports both Individual and Organization accounts
/// </summary>
[Table("Customers")] // Keep table name for backward compatibility during migration
public class Account : BaseEntity
{
    #region Category & Type

    /// <summary>
    /// Whether this is an Individual or Organization account
    /// </summary>
    public AccountCategory Category { get; set; } = AccountCategory.Individual;

    #endregion

    #region Individual Account Fields
    // (Used when Category = Individual)

    /// <summary>First name (for Individual accounts)</summary>
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name (for Individual accounts)</summary>
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Salutation (Mr., Mrs., Dr., etc.)</summary>
    [MaxLength(20)]
    public string? Salutation { get; set; }

    /// <summary>Suffix (Jr., Sr., III, etc.)</summary>
    [MaxLength(20)]
    public string? Suffix { get; set; }

    /// <summary>Date of birth</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>Gender</summary>
    [MaxLength(20)]
    public string? Gender { get; set; }

    /// <summary>
    /// For Individual accounts, optionally link to a Contact record
    /// </summary>
    public int? LinkedContactId { get; set; }

    #endregion

    #region Organization Account Fields
    // (Used when Category = Organization)

    /// <summary>
    /// Organization/Company name (primary name for Organization accounts)
    /// </summary>
    [MaxLength(255)]
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Legal/registered name of the organization
    /// </summary>
    [MaxLength(255)]
    public string? LegalName { get; set; }

    /// <summary>
    /// Doing Business As (DBA) name
    /// </summary>
    [MaxLength(255)]
    public string? DbaName { get; set; }

    /// <summary>
    /// Tax ID / EIN / VAT number
    /// </summary>
    [MaxLength(50)]
    public string? TaxId { get; set; }

    /// <summary>
    /// Organization registration number
    /// </summary>
    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Year the organization was founded
    /// </summary>
    public int? YearFounded { get; set; }

    /// <summary>
    /// Primary contact ID for the organization (from AccountContacts)
    /// </summary>
    public int? PrimaryContactId { get; set; }

    #endregion

    #region Contact Information

    /// <summary>Primary email address</summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Secondary email</summary>
    [MaxLength(255)]
    [EmailAddress]
    public string? SecondaryEmail { get; set; }

    /// <summary>Primary phone number</summary>
    [Required]
    [MaxLength(30)]
    [Phone]
    public string Phone { get; set; } = string.Empty;

    /// <summary>Mobile phone</summary>
    [MaxLength(30)]
    [Phone]
    public string? MobilePhone { get; set; }

    /// <summary>Fax number</summary>
    [MaxLength(30)]
    public string? FaxNumber { get; set; }

    /// <summary>Job title (for individual accounts)</summary>
    [MaxLength(100)]
    public string? JobTitle { get; set; }

    /// <summary>Website URL</summary>
    [MaxLength(500)]
    [Url]
    public string? Website { get; set; }

    #endregion

    #region Address - Primary/Billing

    /// <summary>Street address</summary>
    [Required]
    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    /// <summary>Address line 2</summary>
    [MaxLength(255)]
    public string? Address2 { get; set; }

    /// <summary>City</summary>
    [Required]
    [MaxLength(100)]
    public string City { get; set; } = string.Empty;

    /// <summary>State/Province</summary>
    [Required]
    [MaxLength(100)]
    public string State { get; set; } = string.Empty;

    /// <summary>Postal/ZIP code</summary>
    [Required]
    [MaxLength(20)]
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>Country</summary>
    [Required]
    [MaxLength(100)]
    public string Country { get; set; } = string.Empty;

    #endregion

    #region Address - Shipping

    /// <summary>Shipping street address</summary>
    [MaxLength(255)]
    public string? ShippingAddress { get; set; }

    /// <summary>Shipping address line 2</summary>
    [MaxLength(255)]
    public string? ShippingAddress2 { get; set; }

    /// <summary>Shipping city</summary>
    [MaxLength(100)]
    public string? ShippingCity { get; set; }

    /// <summary>Shipping state</summary>
    [MaxLength(100)]
    public string? ShippingState { get; set; }

    /// <summary>Shipping postal/ZIP code</summary>
    [MaxLength(20)]
    public string? ShippingZipCode { get; set; }

    /// <summary>Shipping country</summary>
    [MaxLength(100)]
    public string? ShippingCountry { get; set; }

    /// <summary>Whether shipping address is same as billing</summary>
    public bool ShippingSameAsBilling { get; set; } = true;

    #endregion

#region Business Information

    /// <summary>Industry classification</summary>
    [MaxLength(100)]
    public string? Industry { get; set; }

    /// <summary>Sub-industry classification</summary>
    [MaxLength(100)]
    public string? SubIndustry { get; set; }

    /// <summary>Number of employees</summary>
    public int? NumberOfEmployees { get; set; }

    /// <summary>Employee range (1-10, 11-50, 51-200, etc.)</summary>
    [MaxLength(50)]
    public string? EmployeeRange { get; set; }

    /// <summary>Annual revenue</summary>
    [Range(0, double.MaxValue)]
    public decimal AnnualRevenue { get; set; } = 0;

    /// <summary>Revenue range (&lt;1M, 1-10M, 10-50M, etc.)</summary>
    [MaxLength(50)]
    public string? RevenueRange { get; set; }

    /// <summary>Customer type (Individual, Business, etc.)</summary>
    public AccountType AccountType { get; set; } = AccountType.Individual;

    /// <summary>Priority level</summary>
    public AccountPriority Priority { get; set; } = AccountPriority.Medium;

    /// <summary>Stock ticker symbol (for public companies)</summary>
    [MaxLength(20)]
    public string? StockSymbol { get; set; }

    /// <summary>Ownership type (Public, Private, Subsidiary, etc.)</summary>
    [MaxLength(50)]
    public string? Ownership { get; set; }

    #endregion

    #region Lifecycle & Status

    /// <summary>Customer lifecycle stage</summary>
    public AccountLifecycleStage LifecycleStage { get; set; } = AccountLifecycleStage.Other;

    /// <summary>How the lead was sourced</summary>
    [MaxLength(100)]
    public string? LeadSource { get; set; }

    /// <summary>Date of first contact</summary>
    public DateTime? FirstContactDate { get; set; }

    /// <summary>Date converted from lead to account</summary>
    public DateTime? ConversionDate { get; set; }

    /// <summary>Last activity date</summary>
    public DateTime? LastActivityDate { get; set; }

    /// <summary>Next scheduled follow-up</summary>
    public DateTime? NextFollowUpDate { get; set; }

    #endregion

    #region Financial

    /// <summary>Total purchases amount</summary>
    [Range(0, double.MaxValue)]
    public decimal TotalPurchases { get; set; } = 0;

    /// <summary>Current account balance</summary>
    public decimal AccountBalance { get; set; } = 0;

    /// <summary>Credit limit</summary>
    [Range(0, double.MaxValue)]
    public decimal CreditLimit { get; set; } = 0;

    /// <summary>Payment terms (Net 30, Net 60, etc.)</summary>
    [MaxLength(50)]
    public string? PaymentTerms { get; set; }

    /// <summary>Preferred payment method</summary>
    [MaxLength(50)]
    public string? PreferredPaymentMethod { get; set; }

    /// <summary>Preferred currency (3-letter ISO code)</summary>
    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>Currency lookup ID</summary>
    public int? CurrencyLookupId { get; set; }

    /// <summary>Billing cycle (Monthly, Quarterly, Annual)</summary>
    [MaxLength(50)]
    public string? BillingCycle { get; set; }

    /// <summary>Billing cycle lookup ID</summary>
    public int? BillingCycleLookupId { get; set; }

    #endregion

    #region Scoring & Rating

    /// <summary>Lead score (0-100)</summary>
    [Range(0, 100)]
    public int LeadScore { get; set; } = 0;

    /// <summary>Customer health score (0-100)</summary>
    [Range(0, 100)]
    public int AccountHealthScore { get; set; } = 50;

    /// <summary>Net Promoter Score (-100 to 100)</summary>
    [Range(-100, 100)]
    public int NpsScore { get; set; } = 0;

    /// <summary>Satisfaction rating (0-5)</summary>
    [Range(0, 5)]
    public double SatisfactionRating { get; set; } = 0;

    #endregion

    #region Social & Communication Preferences

    /// <summary>LinkedIn profile URL</summary>
    [MaxLength(500)]
    [Url]
    public string? LinkedInUrl { get; set; }

    /// <summary>Twitter/X handle</summary>
    [MaxLength(100)]
    public string? TwitterHandle { get; set; }

    /// <summary>Facebook profile URL</summary>
    [MaxLength(500)]
    [Url]
    public string? FacebookUrl { get; set; }

    /// <summary>Email marketing opt-in</summary>
    public bool OptInEmail { get; set; } = true;

    /// <summary>SMS marketing opt-in</summary>
    public bool OptInSms { get; set; } = false;

    /// <summary>Phone marketing opt-in</summary>
    public bool OptInPhone { get; set; } = true;

    /// <summary>Preferred contact method</summary>
    [MaxLength(50)]
    public string? PreferredContactMethod { get; set; }

    /// <summary>Preferred contact time</summary>
    [MaxLength(50)]
    public string? PreferredContactTime { get; set; }

    /// <summary>Timezone</summary>
    [MaxLength(100)]
    public string? Timezone { get; set; }

    /// <summary>Preferred language</summary>
    [MaxLength(50)]
    public string? PreferredLanguage { get; set; }

    #endregion

    #region Assignment & Ownership

    /// <summary>Assigned sales rep user ID</summary>
    [ForeignKey("AssignedToUser")]
    public int? AssignedToUserId { get; set; }

    /// <summary>Account manager user ID</summary>
    [ForeignKey("AccountManager")]
    public int? AccountManagerId { get; set; }

    /// <summary>Sales territory</summary>
    [MaxLength(100)]
    public string? Territory { get; set; }

    /// <summary>Geographic region</summary>
    [MaxLength(100)]
    public string? Region { get; set; }

    #endregion

    #region Classification

    /// <summary>Tags (comma-separated)</summary>
    [MaxLength(500)]
    public string? Tags { get; set; }

    /// <summary>Customer segment</summary>
    [MaxLength(100)]
    public string? Segment { get; set; }

    /// <summary>Referral source</summary>
    [MaxLength(255)]
    public string? ReferralSource { get; set; }

    /// <summary>Referring account ID</summary>
    public int? ReferredByAccountId { get; set; }

    /// <summary>Parent account ID (for subsidiary relationships)</summary>
    public int? ParentAccountId { get; set; }

    #endregion

    #region Lead Conversion

    /// <summary>The lead that was converted to create this account</summary>
    public int? ConvertedFromLeadId { get; set; }

    /// <summary>The campaign that generated the original lead</summary>
    public int? SourceCampaignId { get; set; }

    #endregion

    #region Documentation

    /// <summary>General notes</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Internal notes (not visible to account)</summary>
    public string? InternalNotes { get; set; }

    /// <summary>Description</summary>
    public string? Description { get; set; }

    #endregion

    #region Branding

    /// <summary>URL to account's company logo</summary>
    [MaxLength(500)]
    [Url]
    public string? LogoUrl { get; set; }

    #endregion

    #region Custom Fields

    /// <summary>JSON-serialized custom fields</summary>
    public string? CustomFields { get; set; }

    #endregion

    #region Merge Tracking

    /// <summary>If this record was merged, ID of the master record it was merged into</summary>
    public int? MergedIntoId { get; set; }

    /// <summary>Reference to the merge group this record belongs to</summary>
    public int? MergeGroupId { get; set; }

    /// <summary>Quick flag indicating this is a merged duplicate (soft-deleted)</summary>
    public bool IsMergedDuplicate { get; set; } = false;

    /// <summary>When this record was merged</summary>
    public DateTime? MergedAt { get; set; }

    #endregion

    #region Navigation Properties

    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<Interaction>? Interactions { get; set; }
    public ICollection<AccountContact>? AccountContacts { get; set; }
    public ICollection<Subscription>? Subscriptions { get; set; }

    /// <summary>Contacts directly owned by this account</summary>
    public ICollection<Contact>? Contacts { get; set; }

    /// <summary>Contact information links (addresses, phones/emails, social accounts)</summary>
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }

    /// <summary>Currency lookup navigation</summary>
    public LookupItem? CurrencyLookup { get; set; }

    /// <summary>Billing cycle lookup navigation</summary>
    public LookupItem? BillingCycleLookup { get; set; }

    /// <summary>Assigned user navigation</summary>
    public User? AssignedToUser { get; set; }

    /// <summary>Account manager navigation</summary>
    public User? AccountManager { get; set; }

    /// <summary>Referring account navigation</summary>
    public Account? ReferredByAccount { get; set; }

    /// <summary>Parent account navigation</summary>
    public Account? ParentAccount { get; set; }

    /// <summary>Converted lead navigation</summary>
    public Lead? ConvertedFromLead { get; set; }

    /// <summary>Source campaign navigation</summary>
    public MarketingCampaign? SourceCampaign { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>Display name - returns full name for individuals, company name for organizations</summary>
    public string DisplayName => Category == AccountCategory.Organization
        ? Company
        : $"{FirstName} {LastName}".Trim();

    #endregion
}

#region Backward Compatibility Aliases

/// <summary>
/// Backward compatibility alias for Customer - use Account instead
/// </summary>
[Obsolete("Use Account instead. Customer is deprecated.")]
public class Customer : Account
{
}

/// <summary>Backward compatibility alias</summary>
[Obsolete("Use AccountCategory instead")]
public enum CustomerCategory
{
    Individual = 0,
    Organization = 1
}

/// <summary>Backward compatibility alias</summary>
[Obsolete("Use AccountLifecycleStage instead")]
public enum CustomerLifecycleStage
{
    Other = 0,
    Lead = 1,
    Opportunity = 2,
    Customer = 3,
    CustomerAtRisk = 4,
    Churned = 5,
    WinBack = 6
}

/// <summary>Backward compatibility alias</summary>
[Obsolete("Use AccountType instead")]
public enum CustomerType
{
    Individual = 0,
    SmallBusiness = 1,
    MidMarket = 2,
    Enterprise = 3,
    Government = 4,
    NonProfit = 5
}

/// <summary>Backward compatibility alias</summary>
[Obsolete("Use AccountPriority instead")]
public enum CustomerPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

#endregion

