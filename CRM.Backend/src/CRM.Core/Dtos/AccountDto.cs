// CRM Solution - Customer Relationship Management System
// Copyright (C) 2024-2026 Abhishek Lal
//
// This software is source-available. Non-commercial use is permitted under
// the terms of the LICENSE file. Commercial use requires a separate license.
// See the LICENSE file in the root directory for full terms.
using CRM.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for customer responses
/// </summary>
public class AccountDto
{
    public int Id { get; set; }

    // Category
    public string Category { get; set; } = "Individual"; // Individual or Organization
    public bool IsOrganization => Category == "Organization";

    // Individual fields
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int? LinkedContactId { get; set; }
    public string? LinkedContactName { get; set; }

    // Organization fields
    public string Company { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? DbaName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? YearFounded { get; set; }
    public int? PrimaryContactId { get; set; }
    public string? PrimaryContactName { get; set; }

    // Contact Information
    public string Email { get; set; } = string.Empty;
    public string? SecondaryEmail { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? JobTitle { get; set; }
    public string? Website { get; set; }

    // Address - Billing
    public string Address { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Address - Shipping
    public string? ShippingAddress { get; set; }
    public string? ShippingAddress2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public bool ShippingSameAsBilling { get; set; } = true;

    // Business Information
    public string? Industry { get; set; }
    public string? SubIndustry { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? EmployeeRange { get; set; }
    public decimal AnnualRevenue { get; set; } = 0;
    public string? RevenueRange { get; set; }
    public string AccountType { get; set; } = "Individual";
    public string Priority { get; set; } = "Medium";
    public string? StockSymbol { get; set; }
    public string? Ownership { get; set; }

    // Lifecycle & Status
    public string LifecycleStage { get; set; } = "Lead";
    public string? LeadSource { get; set; }
    public DateTime? FirstContactDate { get; set; }
    public DateTime? ConversionDate { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    // Financial
    public decimal TotalPurchases { get; set; } = 0;
    public decimal AccountBalance { get; set; } = 0;
    public decimal CreditLimit { get; set; } = 0;
    public string? PaymentTerms { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }

    // Scoring
    public int LeadScore { get; set; } = 0;
    public int AccountHealthScore { get; set; } = 50;
    public int NpsScore { get; set; } = 0;
    public double SatisfactionRating { get; set; } = 0;

    // Social & Communication
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

    // Assignment
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public int? AccountManagerId { get; set; }
    public string? AccountManagerName { get; set; }
    public string? Territory { get; set; }
    public string? Region { get; set; }

    // Classification
    public string? Tags { get; set; }
    public string? Segment { get; set; }
    public string? ReferralSource { get; set; }
    public int? ReferredByAccountId { get; set; }
    public string? ReferredByAccountName { get; set; }
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }

    // Documentation
    public string Notes { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }
    public string? CustomFields { get; set; }

    // Financial Metrics (response DTO only - computed/aggregated)
    public decimal? LifetimeValue { get; set; }
    public decimal? MonthlyRecurringRevenue { get; set; }
    public decimal? AnnualRecurringRevenue { get; set; }
    public decimal? AverageOrderValue { get; set; }
    public decimal? ContractValue { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public string? PaymentStatus { get; set; }
    public int? ActiveSubscriptionCount { get; set; }
    public int? TotalInvoiceCount { get; set; }

    // Compliance & Verification
    public string? VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerificationMethod { get; set; }
    public int? VerifiedByUserId { get; set; }
    public bool RequiresNda { get; set; }
    public bool NdaSigned { get; set; }
    public DateTime? NdaSignedDate { get; set; }
    public string? NdaReferenceId { get; set; }
    public string? DataClassification { get; set; }
    public string? DunsNumber { get; set; }
    public string? BusinessLicense { get; set; }
    public DateTime? ComplianceCheckDate { get; set; }
    public string? ComplianceNotes { get; set; }

    // Partnership & Reseller
    public bool? IsReseller { get; set; }
    public bool? IsPartner { get; set; }
    public bool? IsIntegrationPartner { get; set; }
    public string? PartnerTier { get; set; }
    public DateTime? PartnerEnrolledDate { get; set; }
    public string? PartnerStatus { get; set; }
    public int? ParentResellerAccountId { get; set; }
    public int? CompetitorAccountId { get; set; }
    public string? TechStack { get; set; }
    public string? IntegrationPartnerType { get; set; }

    // Lead Conversion & Branding (response DTO only)
    public int? ConvertedFromLeadId { get; set; }
    public int? SourceCampaignId { get; set; }
    public string? LogoUrl { get; set; }
    public int? CurrencyLookupId { get; set; }
    public int? BillingCycleLookupId { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control.
    /// Used to detect concurrent updates - clients should send this in If-Match header.
    /// </summary>
    public byte[]? RowVersion { get; set; }

    // Display
    public string DisplayName { get; set; } = string.Empty;

    // Linked contacts (for organizations)
    public List<AccountContactDto>? Contacts { get; set; }
    public int ContactCount { get; set; } = 0;

    // === Normalized Contact Info Collections ===
    // These replace the flat contact fields above and are the source of truth
    public List<LinkedEmailDto>? EmailAddresses { get; set; }
    public List<LinkedPhoneDto>? PhoneNumbers { get; set; }
    public List<LinkedAddressDto>? Addresses { get; set; }
    public List<LinkedSocialMediaDto>? SocialMediaAccounts { get; set; }
}

/// <summary>
/// DTO for customer contact relationships
/// </summary>
public class AccountContactDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string Role { get; set; } = "Primary";
    public bool IsPrimaryContact { get; set; } = false;
    public bool IsDecisionMaker { get; set; } = false;
    public bool ReceivesBillingNotifications { get; set; } = false;
    public bool ReceivesMarketingEmails { get; set; } = true;
    public bool ReceivesTechnicalUpdates { get; set; } = false;
    public string? PositionAtAccount { get; set; }
    public string? DepartmentAtAccount { get; set; }
    public DateTime? RelationshipStartDate { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a customer
/// </summary>
public class CreateAccountDto
{
    // Category
    public AccountCategory Category { get; set; } = AccountCategory.Individual;

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int? LinkedContactId { get; set; }

    // Organization fields
    public string? Company { get; set; }
    public string? LegalName { get; set; }
    public string? DbaName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? YearFounded { get; set; }

    // Contact Information
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string Email { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(200)]
    public string? SecondaryEmail { get; set; }
    
    [Required(ErrorMessage = "Phone is required")]
    [Phone(ErrorMessage = "Invalid phone format")]
    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string Phone { get; set; } = string.Empty;
    public string? MobilePhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? JobTitle { get; set; }
    public string? Website { get; set; }

    // Address
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    // Shipping Address
    public string? ShippingAddress { get; set; }
    public string? ShippingAddress2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public bool ShippingSameAsBilling { get; set; } = true;

    // Business Information
    public string? Industry { get; set; }
    public string? SubIndustry { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? EmployeeRange { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? RevenueRange { get; set; }
    public AccountType AccountType { get; set; } = AccountType.Individual;
    public AccountPriority Priority { get; set; } = AccountPriority.Medium;
    public string? StockSymbol { get; set; }
    public string? Ownership { get; set; }

    // Lifecycle
    public AccountLifecycleStage LifecycleStage { get; set; } = AccountLifecycleStage.Lead;
    public string? LeadSource { get; set; }

    // Assignment
    public int? AssignedToUserId { get; set; }
    public int? AccountManagerId { get; set; }
    public string? Territory { get; set; }
    public string? Region { get; set; }

    // Classification
    public string? Tags { get; set; }
    public string? Segment { get; set; }
    public string? ReferralSource { get; set; }
    public int? ReferredByAccountId { get; set; }
    public int? ParentAccountId { get; set; }

    // Documentation
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }

    // Communication Preferences
    public bool OptInEmail { get; set; } = true;
    public bool OptInSms { get; set; } = false;
    public bool OptInPhone { get; set; } = true;
    public string? PreferredContactMethod { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Currency { get; set; }

    // Compliance & Verification
    public string? VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerificationMethod { get; set; }
    public int? VerifiedByUserId { get; set; }
    public bool RequiresNda { get; set; }
    public bool NdaSigned { get; set; }
    public DateTime? NdaSignedDate { get; set; }
    public string? NdaReferenceId { get; set; }
    public string? DataClassification { get; set; }
    public string? DunsNumber { get; set; }
    public string? BusinessLicense { get; set; }
    public DateTime? ComplianceCheckDate { get; set; }
    public string? ComplianceNotes { get; set; }

    // Partnership & Reseller
    public bool? IsReseller { get; set; }
    public bool? IsPartner { get; set; }
    public bool? IsIntegrationPartner { get; set; }
    public string? PartnerTier { get; set; }
    public DateTime? PartnerEnrolledDate { get; set; }
    public string? PartnerStatus { get; set; }
    public int? ParentResellerAccountId { get; set; }
    public int? CompetitorAccountId { get; set; }
    public string? TechStack { get; set; }
    public string? IntegrationPartnerType { get; set; }
}

/// <summary>
/// DTO for updating a customer
/// </summary>
public class UpdateAccountDto
{
    // Category
    public AccountCategory? Category { get; set; }

    // Individual fields
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation { get; set; }
    public string? Suffix { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int? LinkedContactId { get; set; }

    // Organization fields
    public string? Company { get; set; }
    public string? LegalName { get; set; }
    public string? DbaName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public int? YearFounded { get; set; }
    public int? PrimaryContactId { get; set; }

    // Contact Information
    public string? Email { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? FaxNumber { get; set; }
    public string? JobTitle { get; set; }
    public string? Website { get; set; }

    // Address
    public string? Address { get; set; }
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }

    // Shipping Address
    public string? ShippingAddress { get; set; }
    public string? ShippingAddress2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingZipCode { get; set; }
    public string? ShippingCountry { get; set; }
    public bool? ShippingSameAsBilling { get; set; }

    // Business Information
    public string? Industry { get; set; }
    public string? SubIndustry { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? EmployeeRange { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? RevenueRange { get; set; }
    public AccountType? AccountType { get; set; }
    public AccountPriority? Priority { get; set; }
    public string? StockSymbol { get; set; }
    public string? Ownership { get; set; }

    // Lifecycle
    public AccountLifecycleStage? LifecycleStage { get; set; }
    public string? LeadSource { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    // Financial
    public decimal? CreditLimit { get; set; }
    public string? PaymentTerms { get; set; }
    public string? PreferredPaymentMethod { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }

    // Scoring
    public int? LeadScore { get; set; }
    public int? AccountHealthScore { get; set; }
    public int? NpsScore { get; set; }
    public double? SatisfactionRating { get; set; }

    // Assignment
    public int? AssignedToUserId { get; set; }
    public int? AccountManagerId { get; set; }
    public string? Territory { get; set; }
    public string? Region { get; set; }

    // Classification
    public string? Tags { get; set; }
    public string? Segment { get; set; }
    public string? ReferralSource { get; set; }
    public int? ReferredByAccountId { get; set; }
    public int? ParentAccountId { get; set; }

    // Documentation
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }
    public string? CustomFields { get; set; }

    // Communication Preferences
    public bool? OptInEmail { get; set; }
    public bool? OptInSms { get; set; }
    public bool? OptInPhone { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? PreferredContactTime { get; set; }
    public string? Timezone { get; set; }
    public string? PreferredLanguage { get; set; }

    // Social
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }

    // Compliance & Verification
    public string? VerificationStatus { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerificationMethod { get; set; }
    public int? VerifiedByUserId { get; set; }
    public bool? RequiresNda { get; set; }
    public bool? NdaSigned { get; set; }
    public DateTime? NdaSignedDate { get; set; }
    public string? NdaReferenceId { get; set; }
    public string? DataClassification { get; set; }
    public string? DunsNumber { get; set; }
    public string? BusinessLicense { get; set; }
    public DateTime? ComplianceCheckDate { get; set; }
    public string? ComplianceNotes { get; set; }

    // Partnership & Reseller
    public bool? IsReseller { get; set; }
    public bool? IsPartner { get; set; }
    public bool? IsIntegrationPartner { get; set; }
    public string? PartnerTier { get; set; }
    public DateTime? PartnerEnrolledDate { get; set; }
    public string? PartnerStatus { get; set; }
    public int? ParentResellerAccountId { get; set; }
    public int? CompetitorAccountId { get; set; }
    public string? TechStack { get; set; }
    public string? IntegrationPartnerType { get; set; }
}

/// <summary>
/// DTO for linking a contact to a customer
/// </summary>
public class LinkContactToAccountDto
{
    public int ContactId { get; set; }
    public AccountContactRole Role { get; set; } = AccountContactRole.Primary;
    public bool IsPrimaryContact { get; set; } = false;
    public bool IsDecisionMaker { get; set; } = false;
    public bool ReceivesBillingNotifications { get; set; } = false;
    public bool ReceivesMarketingEmails { get; set; } = true;
    public bool ReceivesTechnicalUpdates { get; set; } = false;
    public string? PositionAtAccount { get; set; }
    public string? DepartmentAtAccount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a customer contact relationship
/// </summary>
public class UpdateAccountContactDto
{
    public AccountContactRole? Role { get; set; }
    public bool? IsPrimaryContact { get; set; }
    public bool? IsDecisionMaker { get; set; }
    public bool? ReceivesBillingNotifications { get; set; }
    public bool? ReceivesMarketingEmails { get; set; }
    public bool? ReceivesTechnicalUpdates { get; set; }
    public string? PositionAtAccount { get; set; }
    public string? DepartmentAtAccount { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public string? Notes { get; set; }
}
