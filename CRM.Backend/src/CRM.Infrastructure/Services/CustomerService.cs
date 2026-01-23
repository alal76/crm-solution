using CRM.Core.Dtos;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Ports.Input;

namespace CRM.Infrastructure.Services;

/// <summary>
/// Customer service implementation providing CRUD operations for Customer entities.
/// 
/// HEXAGONAL ARCHITECTURE:
/// - Implements ICustomerInputPort (primary/driving port)
/// - Implements ICustomerService (backward compatibility)
/// - Uses IRepository pattern for data access (secondary/driven ports)
/// 
/// FUNCTIONAL VIEW:
/// This service handles all customer-related business operations including:
/// - Creating Individual and Organization customers
/// - Managing customer lifecycle (Lead → Prospect → Opportunity → Customer)
/// - Linking contacts to organization customers
/// - Searching and filtering customers by various criteria
/// - Soft-deleting customers (preserves data for audit/recovery)
/// 
/// TECHNICAL VIEW:
/// - Uses IRepository pattern for data access abstraction
/// - Maps between Customer entities and CustomerDto for API responses
/// - Supports async/await pattern for non-blocking database operations
/// - Integrates with IContactsService for contact management
/// 
/// PATTERN:
/// [Controller] → [ICustomerInputPort] → [CustomerService] → [IRepository] → [Database]
/// </summary>
public class CustomerService : ICustomerService, ICustomerInputPort
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<CustomerContact> _customerContactRepository;
    private readonly IContactsService _contactsService;
    private readonly IRepository<Address> _addressRepository;
    private readonly IRepository<ContactDetail> _contactDetailRepository;
    private readonly IRepository<SocialAccount> _socialAccountRepository;
    private readonly IRepository<ContactInfoLink> _contactInfoLinkRepository;
    private readonly IRepository<CRM.Core.Entities.EntityTag> _entityTagRepository;
    private readonly IRepository<CRM.Core.Entities.CustomField> _customFieldRepository;
    private readonly NormalizationService _normalizationService;

    /// <summary>
    /// Initializes a new instance of CustomerService with required dependencies.
    /// </summary>
    /// <param name="customerRepository">Repository for Customer entity CRUD operations</param>
    /// <param name="customerContactRepository">Repository for CustomerContact junction records</param>
    /// <param name="contactsService">Service for managing Contact records</param>
    public CustomerService(
        IRepository<Customer> customerRepository,
        IRepository<CustomerContact> customerContactRepository,
        IContactsService contactsService,
        IRepository<Address> addressRepository,
        IRepository<ContactDetail> contactDetailRepository,
        IRepository<SocialAccount> socialAccountRepository,
        IRepository<ContactInfoLink> contactInfoLinkRepository,
        IRepository<CRM.Core.Entities.EntityTag> entityTagRepository,
        IRepository<CRM.Core.Entities.CustomField> customFieldRepository,
        NormalizationService normalizationService)
    {
        _customerRepository = customerRepository;
        _customerContactRepository = customerContactRepository;
        _contactsService = contactsService;
        _addressRepository = addressRepository;
        _contactDetailRepository = contactDetailRepository;
        _socialAccountRepository = socialAccountRepository;
        _contactInfoLinkRepository = contactInfoLinkRepository;
        _entityTagRepository = entityTagRepository;
        _customFieldRepository = customFieldRepository;
        _normalizationService = normalizationService;
    }

    /// <summary>
    /// Retrieves a single customer by their unique identifier.
    /// 
    /// FUNCTIONAL: Returns customer details including contact links for organizations.
    /// TECHNICAL: Filters out soft-deleted records, maps to DTO.
    /// </summary>
    /// <param name="id">The unique customer ID</param>
    /// <returns>CustomerDto if found and not deleted, null otherwise</returns>
    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
            return null;
            
        return await MapToDto(customer);
    }

    /// <summary>
    /// Retrieves all active (non-deleted) customers.
    /// 
    /// FUNCTIONAL: Returns complete customer list for dashboards and reports.
    /// TECHNICAL: Filters IsDeleted flag, maps each entity to DTO.
    /// </summary>
    /// <returns>Collection of CustomerDto objects</returns>
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        var activeCustomers = customers.Where(c => !c.IsDeleted).ToList();
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in activeCustomers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    /// <summary>
    /// Searches customers by name, email, or company name.
    /// 
    /// FUNCTIONAL: Allows users to find customers quickly using partial matches.
    /// TECHNICAL: Uses case-insensitive Contains() for flexible matching.
    /// </summary>
    /// <param name="searchTerm">Text to search for in customer records</param>
    /// <returns>Collection of matching CustomerDto objects</returns>
    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm)
    {
        var customers = await _customerRepository.FindAsync(c =>
            !c.IsDeleted && (
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Contains(searchTerm) ||
                c.Company.Contains(searchTerm)
            )
        );
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    /// <summary>
    /// Creates a new customer record.
    /// 
    /// FUNCTIONAL: Supports both Individual (name-based) and Organization (company-based) customers.
    /// TECHNICAL: Maps DTO to entity, persists to database, returns created record.
    /// </summary>
    /// <param name="dto">Customer creation data</param>
    /// <returns>Created CustomerDto with assigned ID</returns>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Category = dto.Category,
            FirstName = dto.FirstName ?? string.Empty,
            LastName = dto.LastName ?? string.Empty,
            Salutation = dto.Salutation,
            Suffix = dto.Suffix,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            LinkedContactId = dto.LinkedContactId,
            Company = dto.Company ?? string.Empty,
            LegalName = dto.LegalName,
            DbaName = dto.DbaName,
            TaxId = dto.TaxId,
            RegistrationNumber = dto.RegistrationNumber,
            YearFounded = dto.YearFounded,
            Email = dto.Email,
            SecondaryEmail = dto.SecondaryEmail,
            Phone = dto.Phone,
            MobilePhone = dto.MobilePhone,
            FaxNumber = dto.FaxNumber,
            JobTitle = dto.JobTitle,
            Website = dto.Website,
            Address = dto.Address ?? string.Empty,
            Address2 = dto.Address2,
            City = dto.City ?? string.Empty,
            State = dto.State ?? string.Empty,
            ZipCode = dto.ZipCode ?? string.Empty,
            Country = dto.Country ?? string.Empty,
            ShippingAddress = dto.ShippingAddress,
            ShippingAddress2 = dto.ShippingAddress2,
            ShippingCity = dto.ShippingCity,
            ShippingState = dto.ShippingState,
            ShippingZipCode = dto.ShippingZipCode,
            ShippingCountry = dto.ShippingCountry,
            ShippingSameAsBilling = dto.ShippingSameAsBilling,
            Industry = dto.Industry,
            SubIndustry = dto.SubIndustry,
            NumberOfEmployees = dto.NumberOfEmployees,
            EmployeeRange = dto.EmployeeRange,
            AnnualRevenue = dto.AnnualRevenue ?? 0,
            RevenueRange = dto.RevenueRange,
            CustomerType = dto.CustomerType,
            Priority = dto.Priority,
            StockSymbol = dto.StockSymbol,
            Ownership = dto.Ownership,
            LifecycleStage = dto.LifecycleStage,
            LeadSource = dto.LeadSource,
            FirstContactDate = DateTime.UtcNow,
            AssignedToUserId = dto.AssignedToUserId,
            AccountManagerId = dto.AccountManagerId,
            Territory = dto.Territory,
            Region = dto.Region,
            Tags = dto.Tags,
            Segment = dto.Segment,
            ReferralSource = dto.ReferralSource,
            ReferredByCustomerId = dto.ReferredByCustomerId,
            ParentCustomerId = dto.ParentCustomerId,
            Notes = dto.Notes ?? string.Empty,
            InternalNotes = dto.InternalNotes,
            Description = dto.Description,
            OptInEmail = dto.OptInEmail,
            OptInSms = dto.OptInSms,
            OptInPhone = dto.OptInPhone,
            PreferredContactMethod = dto.PreferredContactMethod,
            Timezone = dto.Timezone,
            PreferredLanguage = dto.PreferredLanguage,
            Currency = dto.Currency,
            CreatedAt = DateTime.UtcNow
        };

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveAsync();

        // Materialize normalized contact info for new customer
        if (!string.IsNullOrWhiteSpace(dto.Address) || !string.IsNullOrWhiteSpace(dto.City) || !string.IsNullOrWhiteSpace(dto.Country))
        {
            var addr = new Address
            {
                Label = "Primary",
                Line1 = dto.Address ?? string.Empty,
                Line2 = dto.Address2,
                City = dto.City ?? string.Empty,
                State = dto.State,
                PostalCode = dto.ZipCode,
                Country = dto.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _addressRepository.AddAsync(addr);
            await _addressRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = dto.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepository.AddAsync(email);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (!string.IsNullOrWhiteSpace(dto.Phone))
        {
            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = dto.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "created_from_api"
            };
            await _contactDetailRepository.AddAsync(phone);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "created_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        

        return await MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
            return null;

        // Update fields if provided
        if (dto.Category.HasValue) customer.Category = dto.Category.Value;
        if (dto.FirstName != null) customer.FirstName = dto.FirstName;
        if (dto.LastName != null) customer.LastName = dto.LastName;
        if (dto.Salutation != null) customer.Salutation = dto.Salutation;
        if (dto.Suffix != null) customer.Suffix = dto.Suffix;
        if (dto.DateOfBirth.HasValue) customer.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) customer.Gender = dto.Gender;
        if (dto.LinkedContactId.HasValue) customer.LinkedContactId = dto.LinkedContactId;
        if (dto.Company != null) customer.Company = dto.Company;
        if (dto.LegalName != null) customer.LegalName = dto.LegalName;
        if (dto.DbaName != null) customer.DbaName = dto.DbaName;
        if (dto.TaxId != null) customer.TaxId = dto.TaxId;
        if (dto.RegistrationNumber != null) customer.RegistrationNumber = dto.RegistrationNumber;
        if (dto.YearFounded.HasValue) customer.YearFounded = dto.YearFounded;
        if (dto.PrimaryContactId.HasValue) customer.PrimaryContactId = dto.PrimaryContactId;
        if (dto.Email != null) customer.Email = dto.Email;
        if (dto.SecondaryEmail != null) customer.SecondaryEmail = dto.SecondaryEmail;
        if (dto.Phone != null) customer.Phone = dto.Phone;
        if (dto.MobilePhone != null) customer.MobilePhone = dto.MobilePhone;
        if (dto.FaxNumber != null) customer.FaxNumber = dto.FaxNumber;
        if (dto.JobTitle != null) customer.JobTitle = dto.JobTitle;
        if (dto.Website != null) customer.Website = dto.Website;
        if (dto.Address != null) customer.Address = dto.Address;
        if (dto.Address2 != null) customer.Address2 = dto.Address2;
        if (dto.City != null) customer.City = dto.City;
        if (dto.State != null) customer.State = dto.State;
        if (dto.ZipCode != null) customer.ZipCode = dto.ZipCode;
        if (dto.Country != null) customer.Country = dto.Country;
        if (dto.ShippingAddress != null) customer.ShippingAddress = dto.ShippingAddress;
        if (dto.ShippingAddress2 != null) customer.ShippingAddress2 = dto.ShippingAddress2;
        if (dto.ShippingCity != null) customer.ShippingCity = dto.ShippingCity;
        if (dto.ShippingState != null) customer.ShippingState = dto.ShippingState;
        if (dto.ShippingZipCode != null) customer.ShippingZipCode = dto.ShippingZipCode;
        if (dto.ShippingCountry != null) customer.ShippingCountry = dto.ShippingCountry;
        if (dto.ShippingSameAsBilling.HasValue) customer.ShippingSameAsBilling = dto.ShippingSameAsBilling.Value;
        if (dto.Industry != null) customer.Industry = dto.Industry;
        if (dto.SubIndustry != null) customer.SubIndustry = dto.SubIndustry;
        if (dto.NumberOfEmployees.HasValue) customer.NumberOfEmployees = dto.NumberOfEmployees;
        if (dto.EmployeeRange != null) customer.EmployeeRange = dto.EmployeeRange;
        if (dto.AnnualRevenue.HasValue) customer.AnnualRevenue = dto.AnnualRevenue.Value;
        if (dto.RevenueRange != null) customer.RevenueRange = dto.RevenueRange;
        if (dto.CustomerType.HasValue) customer.CustomerType = dto.CustomerType.Value;
        if (dto.Priority.HasValue) customer.Priority = dto.Priority.Value;
        if (dto.StockSymbol != null) customer.StockSymbol = dto.StockSymbol;
        if (dto.Ownership != null) customer.Ownership = dto.Ownership;
        if (dto.LifecycleStage.HasValue) customer.LifecycleStage = dto.LifecycleStage.Value;
        if (dto.LeadSource != null) customer.LeadSource = dto.LeadSource;
        if (dto.NextFollowUpDate.HasValue) customer.NextFollowUpDate = dto.NextFollowUpDate;
        if (dto.CreditLimit.HasValue) customer.CreditLimit = dto.CreditLimit.Value;
        if (dto.PaymentTerms != null) customer.PaymentTerms = dto.PaymentTerms;
        if (dto.PreferredPaymentMethod != null) customer.PreferredPaymentMethod = dto.PreferredPaymentMethod;
        if (dto.Currency != null) customer.Currency = dto.Currency;
        if (dto.BillingCycle != null) customer.BillingCycle = dto.BillingCycle;
        if (dto.LeadScore.HasValue) customer.LeadScore = dto.LeadScore.Value;
        if (dto.CustomerHealthScore.HasValue) customer.CustomerHealthScore = dto.CustomerHealthScore.Value;
        if (dto.NpsScore.HasValue) customer.NpsScore = dto.NpsScore.Value;
        if (dto.SatisfactionRating.HasValue) customer.SatisfactionRating = dto.SatisfactionRating.Value;
        if (dto.AssignedToUserId.HasValue) customer.AssignedToUserId = dto.AssignedToUserId;
        if (dto.AccountManagerId.HasValue) customer.AccountManagerId = dto.AccountManagerId;
        if (dto.Territory != null) customer.Territory = dto.Territory;
        if (dto.Region != null) customer.Region = dto.Region;
        if (dto.Tags != null) customer.Tags = dto.Tags;
        if (dto.Segment != null) customer.Segment = dto.Segment;
        if (dto.ReferralSource != null) customer.ReferralSource = dto.ReferralSource;
        if (dto.ReferredByCustomerId.HasValue) customer.ReferredByCustomerId = dto.ReferredByCustomerId;
        if (dto.ParentCustomerId.HasValue) customer.ParentCustomerId = dto.ParentCustomerId;
        if (dto.Notes != null) customer.Notes = dto.Notes;
        if (dto.InternalNotes != null) customer.InternalNotes = dto.InternalNotes;
        if (dto.Description != null) customer.Description = dto.Description;
        if (dto.CustomFields != null) customer.CustomFields = dto.CustomFields;
        if (dto.OptInEmail.HasValue) customer.OptInEmail = dto.OptInEmail.Value;
        if (dto.OptInSms.HasValue) customer.OptInSms = dto.OptInSms.Value;
        if (dto.OptInPhone.HasValue) customer.OptInPhone = dto.OptInPhone.Value;
        if (dto.PreferredContactMethod != null) customer.PreferredContactMethod = dto.PreferredContactMethod;
        if (dto.PreferredContactTime != null) customer.PreferredContactTime = dto.PreferredContactTime;
        if (dto.Timezone != null) customer.Timezone = dto.Timezone;
        if (dto.PreferredLanguage != null) customer.PreferredLanguage = dto.PreferredLanguage;
        if (dto.LinkedInUrl != null) customer.LinkedInUrl = dto.LinkedInUrl;
        if (dto.TwitterHandle != null) customer.TwitterHandle = dto.TwitterHandle;
        if (dto.FacebookUrl != null) customer.FacebookUrl = dto.FacebookUrl;

        customer.UpdatedAt = DateTime.UtcNow;
        customer.LastActivityDate = DateTime.UtcNow;

        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveAsync();
        
        // If inline contact fields were updated, materialize them into normalized tables
        if (dto.Address != null || dto.City != null || dto.Country != null)
        {
            // unset existing primary address links for this customer
            var existingAddrLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Customer && l.OwnerId == customer.Id && l.InfoKind == ContactInfoKind.Address && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingAddrLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var addr = new Address
            {
                Label = "Primary",
                Line1 = dto.Address ?? string.Empty,
                Line2 = dto.Address2,
                City = dto.City ?? string.Empty,
                State = dto.State,
                PostalCode = dto.ZipCode,
                Country = dto.Country ?? string.Empty,
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _addressRepository.AddAsync(addr);
            await _addressRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.Address,
                InfoId = addr.Id,
                AddressId = addr.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.Email != null)
        {
            var existingEmailLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Customer && l.OwnerId == customer.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingEmailLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var email = new ContactDetail
            {
                DetailType = ContactDetailType.Email,
                Value = dto.Email,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepository.AddAsync(email);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = email.Id,
                ContactDetailId = email.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.Phone != null)
        {
            var existingPhoneLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Customer && l.OwnerId == customer.Id && l.InfoKind == ContactInfoKind.ContactDetail && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingPhoneLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var phone = new ContactDetail
            {
                DetailType = ContactDetailType.Phone,
                Value = dto.Phone,
                Label = "Primary",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _contactDetailRepository.AddAsync(phone);
            await _contactDetailRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.ContactDetail,
                InfoId = phone.Id,
                ContactDetailId = phone.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        if (dto.LinkedInUrl != null)
        {
            var existingSocialLinks = await _contactInfoLinkRepository.FindAsync(l => l.OwnerType == ContactInfoOwnerType.Customer && l.OwnerId == customer.Id && l.InfoKind == ContactInfoKind.SocialAccount && l.IsPrimaryForOwner && !l.IsDeleted);
            foreach (var l in existingSocialLinks)
            {
                l.IsPrimaryForOwner = false;
                await _contactInfoLinkRepository.UpdateAsync(l);
            }

            var sa = new SocialAccount
            {
                Network = SocialNetwork.LinkedIn,
                HandleOrUrl = dto.LinkedInUrl,
                Label = "LinkedIn",
                IsPrimary = true,
                Notes = "updated_from_api"
            };
            await _socialAccountRepository.AddAsync(sa);
            await _socialAccountRepository.SaveAsync();

            var link = new ContactInfoLink
            {
                OwnerType = ContactInfoOwnerType.Customer,
                OwnerId = customer.Id,
                InfoKind = ContactInfoKind.SocialAccount,
                InfoId = sa.Id,
                SocialAccountId = sa.Id,
                IsPrimaryForOwner = true,
                Notes = "updated_from_api"
            };
            await _contactInfoLinkRepository.AddAsync(link);
            await _contactInfoLinkRepository.SaveAsync();
        }

        return await MapToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
            return false;

        customer.IsDeleted = true;
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _customerRepository.UpdateAsync(customer);
        await _customerRepository.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerDto>> GetIndividualCustomersAsync()
    {
        var customers = await _customerRepository.FindAsync(c => 
            !c.IsDeleted && c.Category == CustomerCategory.Individual);
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    public async Task<IEnumerable<CustomerDto>> GetOrganizationCustomersAsync()
    {
        var customers = await _customerRepository.FindAsync(c => 
            !c.IsDeleted && c.Category == CustomerCategory.Organization);
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    public async Task<CustomerContactDto?> LinkContactToCustomerAsync(int customerId, LinkContactToCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null || customer.IsDeleted)
            return null;

        // Verify contact exists
        var contact = await _contactsService.GetByIdAsync(dto.ContactId);
        if (contact == null)
            return null;

        // Check if already linked
        var existingLinks = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && cc.ContactId == dto.ContactId && !cc.IsDeleted);
        if (existingLinks.Any())
            return null;

        // If this is primary contact, unset others
        if (dto.IsPrimaryContact)
        {
            var otherPrimary = await _customerContactRepository.FindAsync(cc => 
                cc.CustomerId == customerId && cc.IsPrimaryContact && !cc.IsDeleted);
            foreach (var other in otherPrimary)
            {
                other.IsPrimaryContact = false;
                await _customerContactRepository.UpdateAsync(other);
            }
        }

        var customerContact = new CustomerContact
        {
            CustomerId = customerId,
            ContactId = dto.ContactId,
            Role = dto.Role,
            IsPrimaryContact = dto.IsPrimaryContact,
            IsDecisionMaker = dto.IsDecisionMaker,
            ReceivesBillingNotifications = dto.ReceivesBillingNotifications,
            ReceivesMarketingEmails = dto.ReceivesMarketingEmails,
            ReceivesTechnicalUpdates = dto.ReceivesTechnicalUpdates,
            PositionAtCustomer = dto.PositionAtCustomer,
            DepartmentAtCustomer = dto.DepartmentAtCustomer,
            Notes = dto.Notes,
            RelationshipStartDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _customerContactRepository.AddAsync(customerContact);
        await _customerContactRepository.SaveAsync();

        // Update primary contact on customer
        if (dto.IsPrimaryContact)
        {
            customer.PrimaryContactId = dto.ContactId;
            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveAsync();
        }

        return MapCustomerContactToDto(customerContact, contact);
    }

    public async Task<bool> UnlinkContactFromCustomerAsync(int customerId, int contactId)
    {
        var links = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && cc.ContactId == contactId && !cc.IsDeleted);
        
        var link = links.FirstOrDefault();
        if (link == null)
            return false;

        link.IsDeleted = true;
        link.RelationshipEndDate = DateTime.UtcNow;
        link.UpdatedAt = DateTime.UtcNow;
        
        await _customerContactRepository.UpdateAsync(link);
        await _customerContactRepository.SaveAsync();

        // If this was primary contact, clear it
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer != null && customer.PrimaryContactId == contactId)
        {
            customer.PrimaryContactId = null;
            await _customerRepository.UpdateAsync(customer);
            await _customerRepository.SaveAsync();
        }

        return true;
    }

    public async Task<CustomerContactDto?> UpdateCustomerContactAsync(int customerId, int contactId, UpdateCustomerContactDto dto)
    {
        var links = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && cc.ContactId == contactId && !cc.IsDeleted);
        
        var link = links.FirstOrDefault();
        if (link == null)
            return null;

        if (dto.Role.HasValue) link.Role = dto.Role.Value;
        if (dto.IsDecisionMaker.HasValue) link.IsDecisionMaker = dto.IsDecisionMaker.Value;
        if (dto.ReceivesBillingNotifications.HasValue) link.ReceivesBillingNotifications = dto.ReceivesBillingNotifications.Value;
        if (dto.ReceivesMarketingEmails.HasValue) link.ReceivesMarketingEmails = dto.ReceivesMarketingEmails.Value;
        if (dto.ReceivesTechnicalUpdates.HasValue) link.ReceivesTechnicalUpdates = dto.ReceivesTechnicalUpdates.Value;
        if (dto.PositionAtCustomer != null) link.PositionAtCustomer = dto.PositionAtCustomer;
        if (dto.DepartmentAtCustomer != null) link.DepartmentAtCustomer = dto.DepartmentAtCustomer;
        if (dto.RelationshipEndDate.HasValue) link.RelationshipEndDate = dto.RelationshipEndDate;
        if (dto.Notes != null) link.Notes = dto.Notes;

        // Handle primary contact change
        if (dto.IsPrimaryContact.HasValue)
        {
            if (dto.IsPrimaryContact.Value && !link.IsPrimaryContact)
            {
                // Unset other primary contacts
                var otherPrimary = await _customerContactRepository.FindAsync(cc => 
                    cc.CustomerId == customerId && cc.IsPrimaryContact && cc.Id != link.Id && !cc.IsDeleted);
                foreach (var other in otherPrimary)
                {
                    other.IsPrimaryContact = false;
                    await _customerContactRepository.UpdateAsync(other);
                }
                
                // Update customer's primary contact
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer != null)
                {
                    customer.PrimaryContactId = contactId;
                    await _customerRepository.UpdateAsync(customer);
                }
            }
            link.IsPrimaryContact = dto.IsPrimaryContact.Value;
        }

        link.UpdatedAt = DateTime.UtcNow;
        await _customerContactRepository.UpdateAsync(link);
        await _customerContactRepository.SaveAsync();

        var contact = await _contactsService.GetByIdAsync(contactId);
        return MapCustomerContactToDto(link, contact);
    }

    public async Task<IEnumerable<CustomerContactDto>> GetCustomerContactsAsync(int customerId)
    {
        var links = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && !cc.IsDeleted);
        
        var dtos = new List<CustomerContactDto>();
        foreach (var link in links)
        {
            var contact = await _contactsService.GetByIdAsync(link.ContactId);
            dtos.Add(MapCustomerContactToDto(link, contact));
        }
        return dtos;
    }

    public async Task<bool> SetPrimaryContactAsync(int customerId, int contactId)
    {
        var links = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && cc.ContactId == contactId && !cc.IsDeleted);
        
        var link = links.FirstOrDefault();
        if (link == null)
            return false;

        // Unset all other primary contacts
        var allLinks = await _customerContactRepository.FindAsync(cc => 
            cc.CustomerId == customerId && !cc.IsDeleted);
        foreach (var l in allLinks)
        {
            l.IsPrimaryContact = (l.Id == link.Id);
            await _customerContactRepository.UpdateAsync(l);
        }

        // Update customer's primary contact
        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer != null)
        {
            customer.PrimaryContactId = contactId;
            await _customerRepository.UpdateAsync(customer);
        }

        await _customerContactRepository.SaveAsync();
        await _customerRepository.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByAssignedUserAsync(int userId)
    {
        var customers = await _customerRepository.FindAsync(c => 
            !c.IsDeleted && c.AssignedToUserId == userId);
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByLifecycleStageAsync(CustomerLifecycleStage stage)
    {
        var customers = await _customerRepository.FindAsync(c => 
            !c.IsDeleted && c.LifecycleStage == stage);
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByPriorityAsync(CustomerPriority priority)
    {
        var customers = await _customerRepository.FindAsync(c => 
            !c.IsDeleted && c.Priority == priority);
        
        var dtos = new List<CustomerDto>();
        foreach (var customer in customers)
        {
            dtos.Add(await MapToDto(customer));
        }
        return dtos;
    }

    private async Task<CustomerDto> MapToDto(Customer customer)
    {
        // Get linked contacts for organizations
        List<CustomerContactDto>? contactDtos = null;
        int contactCount = 0;
        string? primaryContactName = null;
        string? linkedContactName = null;

        if (customer.Category == CustomerCategory.Organization)
        {
            var contacts = await _customerContactRepository.FindAsync(cc => 
                cc.CustomerId == customer.Id && !cc.IsDeleted);
            contactCount = contacts.Count();
            
            contactDtos = new List<CustomerContactDto>();
            foreach (var cc in contacts)
            {
                var contact = await _contactsService.GetByIdAsync(cc.ContactId);
                contactDtos.Add(MapCustomerContactToDto(cc, contact));
                
                if (cc.IsPrimaryContact && contact != null)
                {
                    primaryContactName = $"{contact.FirstName} {contact.LastName}";
                }
            }
        }
        else if (customer.LinkedContactId.HasValue)
        {
            var contact = await _contactsService.GetByIdAsync(customer.LinkedContactId.Value);
            if (contact != null)
            {
                linkedContactName = $"{contact.FirstName} {contact.LastName}";
            }
        }

            // Prefer normalized contact-info, tags and custom fields when available (use NormalizationService)
            var primaryEmail = await _normalizationService.GetPrimaryEmailAsync(ContactInfoOwnerType.Customer, customer.Id) ?? customer.Email;
            var primaryPhone = await _normalizationService.GetPrimaryPhoneAsync(ContactInfoOwnerType.Customer, customer.Id) ?? customer.Phone;
            var primaryFax = await _normalizationService.GetPrimaryFaxAsync(ContactInfoOwnerType.Customer, customer.Id) ?? customer.FaxNumber;
            var primaryAddressEntity = await _normalizationService.GetPrimaryAddressAsync(ContactInfoOwnerType.Customer, customer.Id);

            var addrLine1 = primaryAddressEntity?.Line1 ?? customer.Address;
            var addrLine2 = primaryAddressEntity?.Line2 ?? customer.Address2;
            var addrCity = primaryAddressEntity?.City ?? customer.City;
            var addrState = primaryAddressEntity?.State ?? customer.State;
            var addrPostal = primaryAddressEntity?.PostalCode ?? customer.ZipCode;
            var addrCountry = primaryAddressEntity?.Country ?? customer.Country;

            var tagsValue = await _normalizationService.GetTagsAsync("Customer", customer.Id) ?? customer.Tags;
            var customFieldsValue = await _normalizationService.GetCustomFieldsAsync("Customer", customer.Id) ?? customer.CustomFields;

            // Prefer normalized social accounts when available
            var linkedInUrl = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Customer, customer.Id, SocialNetwork.LinkedIn) ?? customer.LinkedInUrl;
            var twitterHandle = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Customer, customer.Id, SocialNetwork.Twitter) ?? customer.TwitterHandle;
            var facebookUrl = await _normalizationService.GetPrimarySocialAccountAsync(ContactInfoOwnerType.Customer, customer.Id, SocialNetwork.Facebook) ?? customer.FacebookUrl;

        return new CustomerDto
        {
            Id = customer.Id,
            Category = customer.Category.ToString(),
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Salutation = customer.Salutation,
            Suffix = customer.Suffix,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            LinkedContactId = customer.LinkedContactId,
            LinkedContactName = linkedContactName,
            Company = customer.Company,
            LegalName = customer.LegalName,
            DbaName = customer.DbaName,
            TaxId = customer.TaxId,
            RegistrationNumber = customer.RegistrationNumber,
            YearFounded = customer.YearFounded,
            PrimaryContactId = customer.PrimaryContactId,
            PrimaryContactName = primaryContactName,
            Email = primaryEmail,
            SecondaryEmail = customer.SecondaryEmail,
            Phone = primaryPhone,
            MobilePhone = customer.MobilePhone,
            FaxNumber = primaryFax,
            JobTitle = customer.JobTitle,
            Website = customer.Website,
            Address = addrLine1,
            Address2 = addrLine2,
            City = addrCity,
            State = addrState,
            ZipCode = addrPostal,
            Country = addrCountry,
            ShippingAddress = customer.ShippingAddress,
            ShippingAddress2 = customer.ShippingAddress2,
            ShippingCity = customer.ShippingCity,
            ShippingState = customer.ShippingState,
            ShippingZipCode = customer.ShippingZipCode,
            ShippingCountry = customer.ShippingCountry,
            ShippingSameAsBilling = customer.ShippingSameAsBilling,
            Industry = customer.Industry,
            SubIndustry = customer.SubIndustry,
            NumberOfEmployees = customer.NumberOfEmployees,
            EmployeeRange = customer.EmployeeRange,
            AnnualRevenue = customer.AnnualRevenue,
            RevenueRange = customer.RevenueRange,
            CustomerType = customer.CustomerType.ToString(),
            Priority = customer.Priority.ToString(),
            StockSymbol = customer.StockSymbol,
            Ownership = customer.Ownership,
            LifecycleStage = customer.LifecycleStage.ToString(),
            LeadSource = customer.LeadSource,
            FirstContactDate = customer.FirstContactDate,
            ConversionDate = customer.ConversionDate,
            LastActivityDate = customer.LastActivityDate,
            NextFollowUpDate = customer.NextFollowUpDate,
            TotalPurchases = customer.TotalPurchases,
            AccountBalance = customer.AccountBalance,
            CreditLimit = customer.CreditLimit,
            PaymentTerms = customer.PaymentTerms,
            PreferredPaymentMethod = customer.PreferredPaymentMethod,
            Currency = customer.Currency,
            BillingCycle = customer.BillingCycle,
            LeadScore = customer.LeadScore,
            CustomerHealthScore = customer.CustomerHealthScore,
            NpsScore = customer.NpsScore,
            SatisfactionRating = customer.SatisfactionRating,
            LinkedInUrl = linkedInUrl,
            TwitterHandle = twitterHandle,
            FacebookUrl = facebookUrl,
            OptInEmail = customer.OptInEmail,
            OptInSms = customer.OptInSms,
            OptInPhone = customer.OptInPhone,
            PreferredContactMethod = customer.PreferredContactMethod,
            PreferredContactTime = customer.PreferredContactTime,
            Timezone = customer.Timezone,
            PreferredLanguage = customer.PreferredLanguage,
            AssignedToUserId = customer.AssignedToUserId,
            AssignedToUserName = customer.AssignedToUser?.Username,
            AccountManagerId = customer.AccountManagerId,
            AccountManagerName = customer.AccountManager?.Username,
            Territory = customer.Territory,
            Region = customer.Region,
            Tags = tagsValue,
            Segment = customer.Segment,
            ReferralSource = customer.ReferralSource,
            ReferredByCustomerId = customer.ReferredByCustomerId,
            ReferredByCustomerName = customer.ReferredByCustomer?.DisplayName,
            ParentCustomerId = customer.ParentCustomerId,
            ParentCustomerName = customer.ParentCustomer?.DisplayName,
            Notes = customer.Notes,
            InternalNotes = customer.InternalNotes,
            Description = customer.Description,
            CustomFields = customFieldsValue,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            DisplayName = customer.DisplayName,
            Contacts = contactDtos,
            ContactCount = contactCount
        };
    }

    private CustomerContactDto MapCustomerContactToDto(CustomerContact cc, ContactDto? contact)
    {
        return new CustomerContactDto
        {
            Id = cc.Id,
            CustomerId = cc.CustomerId,
            ContactId = cc.ContactId,
            ContactName = contact != null ? $"{contact.FirstName} {contact.LastName}" : "Unknown",
            ContactEmail = contact?.EmailPrimary,
            ContactPhone = contact?.PhonePrimary,
            Role = cc.Role.ToString(),
            IsPrimaryContact = cc.IsPrimaryContact,
            IsDecisionMaker = cc.IsDecisionMaker,
            ReceivesBillingNotifications = cc.ReceivesBillingNotifications,
            ReceivesMarketingEmails = cc.ReceivesMarketingEmails,
            ReceivesTechnicalUpdates = cc.ReceivesTechnicalUpdates,
            PositionAtCustomer = cc.PositionAtCustomer,
            DepartmentAtCustomer = cc.DepartmentAtCustomer,
            RelationshipStartDate = cc.RelationshipStartDate,
            RelationshipEndDate = cc.RelationshipEndDate,
            Notes = cc.Notes,
            CreatedAt = cc.CreatedAt
        };
    }
}

