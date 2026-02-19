// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.

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
[Table("Accounts")]
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

    /// <summary>Preferred contact time</summary>
    [MaxLength(50)]
    public string? PreferredContactTime { get; set; }

    #endregion

    #region Preferences

    /// <summary>
    /// Default communication preferences for this account.
    /// </summary>
    public int? PreferencesId { get; set; }

    /// <summary>
    /// Default preferences entity (shared across contacts unless overridden).
    /// </summary>
    public Preferences? Preferences { get; set; }

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

    #region Financial Metrics

    /// <summary>Lifetime value of the account - total revenue expected from this account over the relationship</summary>
    [Range(0, double.MaxValue)]
    public decimal? LifetimeValue { get; set; }

    /// <summary>Monthly recurring revenue from this account</summary>
    [Range(0, double.MaxValue)]
    public decimal? MonthlyRecurringRevenue { get; set; }

    /// <summary>Annual recurring revenue from this account</summary>
    [Range(0, double.MaxValue)]
    public decimal? AnnualRecurringRevenue { get; set; }

    /// <summary>Average value per order/transaction</summary>
    [Range(0, double.MaxValue)]
    public decimal? AverageOrderValue { get; set; }

    /// <summary>Total contract value with this account</summary>
    [Range(0, double.MaxValue)]
    public decimal? ContractValue { get; set; }

    /// <summary>Date of the last payment received from this account</summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>Current payment status (e.g., Current, Overdue, Past Due)</summary>
    [MaxLength(50)]
    public string? PaymentStatus { get; set; }

    /// <summary>Number of active subscriptions for this account</summary>
    [Range(0, int.MaxValue)]
    public int? ActiveSubscriptionCount { get; set; }

    /// <summary>Total count of invoices issued to this account</summary>
    [Range(0, int.MaxValue)]
    public int? TotalInvoiceCount { get; set; }

    #endregion

    #region Compliance & Verification

    /// <summary>Verification status of the account (e.g., Unverified, Pending, Verified, Rejected)</summary>
    [MaxLength(50)]
    public string? VerificationStatus { get; set; } = "Unverified";

    /// <summary>Date when the account was verified</summary>
    public DateTime? VerificationDate { get; set; }

    /// <summary>Method used to verify the account (e.g., Email, Phone, Document)</summary>
    [MaxLength(50)]
    public string? VerificationMethod { get; set; }

    /// <summary>User ID of the person who performed the verification</summary>
    public int? VerifiedByUserId { get; set; }

    /// <summary>Flag indicating if this account requires a Non-Disclosure Agreement</summary>
    public bool RequiresNda { get; set; } = false;

    /// <summary>Flag indicating if the NDA has been signed</summary>
    public bool NdaSigned { get; set; } = false;

    /// <summary>Date when the NDA was signed</summary>
    public DateTime? NdaSignedDate { get; set; }

    /// <summary>Reference ID or document identifier for the signed NDA</summary>
    [MaxLength(255)]
    public string? NdaReferenceId { get; set; }

    /// <summary>Data classification level (e.g., Internal, Confidential, Public)</summary>
    [MaxLength(50)]
    public string? DataClassification { get; set; } = "Internal";

    /// <summary>Dun & Bradstreet number for business identification</summary>
    [MaxLength(20)]
    public string? DunsNumber { get; set; }

    /// <summary>Business license number or identifier</summary>
    [MaxLength(255)]
    public string? BusinessLicense { get; set; }

    /// <summary>Date of the most recent compliance check</summary>
    public DateTime? ComplianceCheckDate { get; set; }

    /// <summary>Notes regarding compliance status and requirements</summary>
    public string? ComplianceNotes { get; set; }

    #endregion

    #region Partnership & Reseller

    /// <summary>Flag indicating if this account is a reseller</summary>
    public bool? IsReseller { get; set; }

    /// <summary>Flag indicating if this account is a partner</summary>
    public bool? IsPartner { get; set; }

    /// <summary>Flag indicating if this account is an integration partner</summary>
    public bool? IsIntegrationPartner { get; set; }

    /// <summary>Partner tier classification (e.g., Gold, Silver, Bronze)</summary>
    [MaxLength(50)]
    public string? PartnerTier { get; set; }

    /// <summary>Date when this account enrolled as a partner</summary>
    public DateTime? PartnerEnrolledDate { get; set; }

    /// <summary>Current status of the partnership (e.g., Active, Inactive, Suspended)</summary>
    [MaxLength(50)]
    public string? PartnerStatus { get; set; }

    /// <summary>ID of the parent reseller account (for reseller hierarchy)</summary>
    public int? ParentResellerAccountId { get; set; }

    /// <summary>ID of related competitor account</summary>
    public int? CompetitorAccountId { get; set; }

    /// <summary>Technology stack or platforms used by this account</summary>
    public string? TechStack { get; set; }

    /// <summary>Type of integration partnership (e.g., API, Webhook, Native)</summary>
    [MaxLength(100)]
    public string? IntegrationPartnerType { get; set; }

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

    /// <summary>Collection of addresses associated with this account (linked via EntityAddressLink junction table)</summary>
    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    /// <summary>Polymorphic junction table for entity-address relationships</summary>
    public ICollection<EntityAddressLink>? EntityAddressLinks { get; set; }

    /// <summary>Currency lookup navigation</summary>
    public LookupItem? CurrencyLookup { get; set; }

    /// <summary>Billing cycle lookup navigation</summary>
    public LookupItem? BillingCycleLookup { get; set; }

    /// <summary>Assigned user navigation</summary>
    public User? AssignedToUser { get; set; }

    /// <summary>Account manager navigation</summary>
    public User? AccountManager { get; set; }

    /// <summary>User who verified the account</summary>
    public User? VerifiedByUser { get; set; }

    /// <summary>Referring account navigation</summary>
    public Account? ReferredByAccount { get; set; }

    /// <summary>Parent account navigation</summary>
    public Account? ParentAccount { get; set; }

    /// <summary>Converted lead navigation</summary>
    public Lead? ConvertedFromLead { get; set; }

    /// <summary>Source campaign navigation</summary>
    public MarketingCampaign? SourceCampaign { get; set; }

    /// <summary>Parent reseller account (for reseller hierarchy)</summary>
    public Account? ParentReseller { get; set; }

    /// <summary>Child reseller accounts (for reseller hierarchy)</summary>
    public ICollection<Account>? ResellerChildren { get; set; }

    /// <summary>Related competitor account</summary>
    public Account? CompetitorAccount { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>Display name - returns full name for individuals, company name for organizations</summary>
    public string DisplayName => Category == AccountCategory.Organization
        ? Company
        : $"{FirstName} {LastName}".Trim();

    /// <summary>Backward compatibility - returns first address street</summary>
    /// <summary>
    /// Backward compatibility - returns first address street. Not mapped. To set Address, add an Address to the Addresses collection.
    /// </summary>
    public string? Address
    {
        get => Addresses?.FirstOrDefault()?.Line1;
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (Addresses == null)
                    Addresses = new List<Address>();
                if (!Addresses.Any())
                    Addresses.Add(new Address { Line1 = value });
                else
                    Addresses.First().Line1 = value;
            }
        }
    }

    /// <summary>Backward compatibility - returns first address city</summary>
    public string? City
    {
        get => Addresses?.FirstOrDefault()?.City;
        set { /* for backward compat */ }
    }

    /// <summary>Backward compatibility - returns first address state</summary>
    public string? State
    {
        get => Addresses?.FirstOrDefault()?.State;
        set { /* for backward compat */ }
    }

    /// <summary>Backward compatibility - returns first address zip code</summary>
    public string? ZipCode
    {
        get => Addresses?.FirstOrDefault()?.PostalCode;
        set { /* for backward compat */ }
    }

    /// <summary>Backward compatibility - returns first address country</summary>
    public string? Country
    {
        get => Addresses?.FirstOrDefault()?.Country;
        set { /* for backward compat */ }
    }

    /// <summary>Flag indicating if shipping address is same as billing</summary>
    public bool ShippingSameAsBilling { get; set; } = true;

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

