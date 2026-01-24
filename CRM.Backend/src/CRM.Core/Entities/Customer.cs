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

using CRM.Core.Models;

namespace CRM.Core.Entities;

/// <summary>
/// Customer category - Individual or Organization
/// </summary>
public enum CustomerCategory
{
    Individual = 0,
    Organization = 1
}

/// <summary>
/// Customer lifecycle stage enumeration
/// Lifecycle Flow: Other (default) → Lead → Opportunity → Customer → CustomerAtRisk → Churned → (Win-back) → Lead
/// </summary>
public enum CustomerLifecycleStage
{
    /// <summary>Initial default value for new customers</summary>
    Other = 0,
    /// <summary>A potential customer showing interest</summary>
    Lead = 1,
    /// <summary>A qualified lead with an active sales opportunity</summary>
    Opportunity = 2,
    /// <summary>An active paying customer</summary>
    Customer = 3,
    /// <summary>A customer at risk of churning</summary>
    CustomerAtRisk = 4,
    /// <summary>A former customer who has stopped doing business</summary>
    Churned = 5,
    /// <summary>A churned customer being re-engaged (transitions back to Lead)</summary>
    WinBack = 6
}

/// <summary>
/// Customer type enumeration (size/classification)
/// </summary>
public enum CustomerType
{
    Individual = 0,
    SmallBusiness = 1,
    MidMarket = 2,
    Enterprise = 3,
    Government = 4,
    NonProfit = 5
}

/// <summary>
/// Customer priority level
/// </summary>
public enum CustomerPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

/// <summary>
/// Customer entity for managing customer information
/// Supports both Individual and Organization customers
/// </summary>
public class Customer : BaseEntity
{
    // === Category & Type ===
    /// <summary>
    /// Whether this is an Individual or Organization customer
    /// </summary>
    public CustomerCategory Category { get; set; } = CustomerCategory.Individual;
    
    // === Individual Customer Fields ===
    // (Used when Category = Individual)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Salutation { get; set; } // Mr., Mrs., Dr., etc.
    public string? Suffix { get; set; } // Jr., Sr., III, etc.
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    
    /// <summary>
    /// For Individual customers, optionally link to a Contact record
    /// </summary>
    public int? LinkedContactId { get; set; }
    
    // === Organization Customer Fields ===
    // (Used when Category = Organization)
    /// <summary>
    /// Organization/Company name (primary name for Organization customers)
    /// </summary>
    public string Company { get; set; } = string.Empty;
    
    /// <summary>
    /// Legal/registered name of the organization
    /// </summary>
    public string? LegalName { get; set; }
    
    /// <summary>
    /// Doing Business As (DBA) name
    /// </summary>
    public string? DbaName { get; set; }
    
    /// <summary>
    /// Tax ID / EIN / VAT number
    /// </summary>
    public string? TaxId { get; set; }
    
    /// <summary>
    /// Organization registration number
    /// </summary>
    public string? RegistrationNumber { get; set; }
    
    /// <summary>
    /// Year the organization was founded
    /// </summary>
    public int? YearFounded { get; set; }
    
    /// <summary>
    /// Primary contact ID for the organization (from CustomerContacts)
    /// </summary>
    public int? PrimaryContactId { get; set; }
    
    // === Contact Information ===
    public string Email { get; set; } = string.Empty;
    public string? SecondaryEmail { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? JobTitle { get; set; } // For individual customers
    public string? Website { get; set; }
    
    // === Address Information - Primary/Billing ===
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // === Address Information - Shipping ===
    public string? ShippingAddress { get; set; }
    public string? ShippingAddress2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public bool ShippingSameAsBilling { get; set; } = true;
    
    // === Business Information ===
    public string? Industry { get; set; }
    public string? SubIndustry { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? EmployeeRange { get; set; } // 1-10, 11-50, 51-200, etc.
    public decimal AnnualRevenue { get; set; } = 0;
    public string? RevenueRange { get; set; } // <1M, 1-10M, 10-50M, etc.
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public CustomerPriority Priority { get; set; } = CustomerPriority.Medium;
    public string? StockSymbol { get; set; } // For public companies
    public string? Ownership { get; set; } // Public, Private, Subsidiary, etc.
    
    // === Lifecycle & Status ===
    public CustomerLifecycleStage LifecycleStage { get; set; } = CustomerLifecycleStage.Other;
    public string? LeadSource { get; set; }
    public DateTime? FirstContactDate { get; set; }
    public DateTime? ConversionDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }
    
    // === Financial ===
    public decimal TotalPurchases { get; set; } = 0;
    public decimal AccountBalance { get; set; } = 0;
    public decimal CreditLimit { get; set; } = 0;
    public string? PaymentTerms { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? Currency { get; set; } // Preferred currency
    public int? CurrencyLookupId { get; set; }
    public string? BillingCycle { get; set; } // Monthly, Quarterly, Annual
    public int? BillingCycleLookupId { get; set; }
    
    // === Scoring & Rating ===
    public int LeadScore { get; set; } = 0;
    public int CustomerHealthScore { get; set; } = 50;
    public int NpsScore { get; set; } = 0;
    public double SatisfactionRating { get; set; } = 0;
    
    // === Social & Communication ===
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    
    // === Assignment & Ownership ===
    public int? AssignedToUserId { get; set; }
    public int? AccountManagerId { get; set; }
    public string? Territory { get; set; }
    public string? Region { get; set; }
    
    // === Classification ===
    public string? Tags { get; set; }
    public string? Segment { get; set; }
    public string? ReferralSource { get; set; }
    public int? ReferredByCustomerId { get; set; }
    public int? ParentCustomerId { get; set; } // For subsidiary relationships
    
    // === Lead Conversion ===
    /// <summary>
    /// The lead that was converted to create this customer
    /// </summary>
    public int? ConvertedFromLeadId { get; set; }
    
    /// <summary>
    /// The campaign that generated the original lead
    /// </summary>
    public int? SourceCampaignId { get; set; }
    
    // === Documentation ===
    public string Notes { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }
    
    // === Branding ===
    /// <summary>
    /// URL to customer's company logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    // === Custom Fields ===
    public string? CustomFields { get; set; }

    // === Navigation Properties ===
    public ICollection<Opportunity>? Opportunities { get; set; }
    public ICollection<Interaction>? Interactions { get; set; }
    public ICollection<CustomerContact>? CustomerContacts { get; set; } // Linked contacts for organizations
    public ICollection<Account>? Accounts { get; set; }
    
    /// <summary>
    /// Contacts directly owned by this customer (one-to-many relationship)
    /// </summary>
    public ICollection<Contact>? Contacts { get; set; }
    
    // Contact information links (addresses, phones/emails, social accounts)
    public ICollection<ContactInfoLink>? ContactInfoLinks { get; set; }
    // Lookup navigation
    public LookupItem? CurrencyLookup { get; set; }
    public LookupItem? BillingCycleLookup { get; set; }
    public User? AssignedToUser { get; set; }
    public User? AccountManager { get; set; }
    public Customer? ReferredByCustomer { get; set; }
    public Customer? ParentCustomer { get; set; }
    public Lead? ConvertedFromLead { get; set; }
    public MarketingCampaign? SourceCampaign { get; set; }
    
    // === Computed Properties ===
    /// <summary>
    /// Display name - returns full name for individuals, company name for organizations
    /// </summary>
    public string DisplayName => Category == CustomerCategory.Organization 
        ? Company 
        : $"{FirstName} {LastName}".Trim();
}

