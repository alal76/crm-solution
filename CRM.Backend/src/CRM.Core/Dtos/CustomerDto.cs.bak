using CRM.Core.Entities;

namespace CRM.Core.Dtos;

/// <summary>
/// DTO for customer responses
/// </summary>
public class CustomerDto
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
    public string CustomerType { get; set; } = "Individual";
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
    public int CustomerHealthScore { get; set; } = 50;
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
    public int? ReferredByCustomerId { get; set; }
    public string? ReferredByCustomerName { get; set; }
    public int? ParentCustomerId { get; set; }
    public string? ParentCustomerName { get; set; }
    
    // Documentation
    public string Notes { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }
    public string? Description { get; set; }
    public string? CustomFields { get; set; }
    
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
    public List<CustomerContactDto>? Contacts { get; set; }
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
public class CustomerContactDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
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
    public string? PositionAtCustomer { get; set; }
    public string? DepartmentAtCustomer { get; set; }
    public DateTime? RelationshipStartDate { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating a customer
/// </summary>
public class CreateCustomerDto
{
    // Category
    public CustomerCategory Category { get; set; } = CustomerCategory.Individual;
    
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
    public string Email { get; set; } = string.Empty;
    public string? SecondaryEmail { get; set; }
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
    public CustomerType CustomerType { get; set; } = CustomerType.Individual;
    public CustomerPriority Priority { get; set; } = CustomerPriority.Medium;
    public string? StockSymbol { get; set; }
    public string? Ownership { get; set; }
    
    // Lifecycle
    public CustomerLifecycleStage LifecycleStage { get; set; } = CustomerLifecycleStage.Lead;
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
    public int? ReferredByCustomerId { get; set; }
    public int? ParentCustomerId { get; set; }
    
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
}

/// <summary>
/// DTO for updating a customer
/// </summary>
public class UpdateCustomerDto
{
    // Category
    public CustomerCategory? Category { get; set; }
    
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
    public CustomerType? CustomerType { get; set; }
    public CustomerPriority? Priority { get; set; }
    public string? StockSymbol { get; set; }
    public string? Ownership { get; set; }
    
    // Lifecycle
    public CustomerLifecycleStage? LifecycleStage { get; set; }
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
    public int? CustomerHealthScore { get; set; }
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
    public int? ReferredByCustomerId { get; set; }
    public int? ParentCustomerId { get; set; }
    
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
}

/// <summary>
/// DTO for linking a contact to a customer
/// </summary>
public class LinkContactToCustomerDto
{
    public int ContactId { get; set; }
    public CustomerContactRole Role { get; set; } = CustomerContactRole.Primary;
    public bool IsPrimaryContact { get; set; } = false;
    public bool IsDecisionMaker { get; set; } = false;
    public bool ReceivesBillingNotifications { get; set; } = false;
    public bool ReceivesMarketingEmails { get; set; } = true;
    public bool ReceivesTechnicalUpdates { get; set; } = false;
    public string? PositionAtCustomer { get; set; }
    public string? DepartmentAtCustomer { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating a customer contact relationship
/// </summary>
public class UpdateCustomerContactDto
{
    public CustomerContactRole? Role { get; set; }
    public bool? IsPrimaryContact { get; set; }
    public bool? IsDecisionMaker { get; set; }
    public bool? ReceivesBillingNotifications { get; set; }
    public bool? ReceivesMarketingEmails { get; set; }
    public bool? ReceivesTechnicalUpdates { get; set; }
    public string? PositionAtCustomer { get; set; }
    public string? DepartmentAtCustomer { get; set; }
    public DateTime? RelationshipEndDate { get; set; }
    public string? Notes { get; set; }
}
