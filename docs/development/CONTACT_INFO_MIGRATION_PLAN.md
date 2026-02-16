# Contact Information Normalization Migration Plan

## Executive Summary

This document outlines the plan to migrate from flat contact information fields to normalized tables, eliminating data duplication across Customer, Lead, Contact, and Account entities.

**Current State:** ~100+ flat contact fields duplicated across entities  
**Target State:** Single source of truth using normalized tables with junction links  
**Estimated Effort:** 3 phases over 2-3 sprints  
**Risk Level:** Medium (requires careful data migration and API changes)

---

## Phase 1: Data Migration & Dual-Write (Week 1-2)

### 1.1 Migration Script: Populate Normalized Tables

Execute the following migration to copy existing flat data into normalized tables:

```sql
-- Migration: 015_migrate_contact_info_to_normalized.sql

-- ============================================
-- PHASE 1A: Migrate Email Addresses
-- ============================================

-- From Customers
INSERT INTO EmailAddresses (Email, EmailType, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.Email, 'Work', 1, 0, NOW(6), 0
FROM Customers c
WHERE c.Email IS NOT NULL AND c.Email != ''
  AND NOT EXISTS (SELECT 1 FROM EmailAddresses e WHERE e.Email = c.Email);

-- Secondary emails from Customers
INSERT INTO EmailAddresses (Email, EmailType, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.SecondaryEmail, 'Personal', 0, 0, NOW(6), 0
FROM Customers c
WHERE c.SecondaryEmail IS NOT NULL AND c.SecondaryEmail != ''
  AND NOT EXISTS (SELECT 1 FROM EmailAddresses e WHERE e.Email = c.SecondaryEmail);

-- From Leads
INSERT INTO EmailAddresses (Email, EmailType, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT l.Email, 'Work', 1, 0, NOW(6), 0
FROM Leads l
WHERE l.Email IS NOT NULL AND l.Email != ''
  AND NOT EXISTS (SELECT 1 FROM EmailAddresses e WHERE e.Email = l.Email);

-- From Contacts (Primary)
INSERT INTO EmailAddresses (Email, EmailType, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.EmailPrimary, 'Work', 1, 0, NOW(6), 0
FROM Contacts c
WHERE c.EmailPrimary IS NOT NULL AND c.EmailPrimary != ''
  AND NOT EXISTS (SELECT 1 FROM EmailAddresses e WHERE e.Email = c.EmailPrimary);

-- ============================================
-- PHASE 1B: Create Email Links
-- ============================================

-- Link Customers to their emails
INSERT INTO EntityEmailLinks (EntityType, EntityId, EmailAddressId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, e.Id, 1, 'Primary', NOW(6), 0
FROM Customers c
INNER JOIN EmailAddresses e ON e.Email = c.Email
WHERE c.Email IS NOT NULL AND c.Email != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityEmailLinks eel 
    WHERE eel.EntityType = 'Customer' AND eel.EntityId = c.Id AND eel.EmailAddressId = e.Id
  );

-- Link Customers to secondary emails
INSERT INTO EntityEmailLinks (EntityType, EntityId, EmailAddressId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, e.Id, 0, 'Secondary', NOW(6), 0
FROM Customers c
INNER JOIN EmailAddresses e ON e.Email = c.SecondaryEmail
WHERE c.SecondaryEmail IS NOT NULL AND c.SecondaryEmail != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityEmailLinks eel 
    WHERE eel.EntityType = 'Customer' AND eel.EntityId = c.Id AND eel.EmailAddressId = e.Id
  );

-- Link Leads to their emails
INSERT INTO EntityEmailLinks (EntityType, EntityId, EmailAddressId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Lead', l.Id, e.Id, 1, 'Primary', NOW(6), 0
FROM Leads l
INNER JOIN EmailAddresses e ON e.Email = l.Email
WHERE l.Email IS NOT NULL AND l.Email != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityEmailLinks eel 
    WHERE eel.EntityType = 'Lead' AND eel.EntityId = l.Id AND eel.EmailAddressId = e.Id
  );

-- Link Contacts to their emails
INSERT INTO EntityEmailLinks (EntityType, EntityId, EmailAddressId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Contact', c.Id, e.Id, 1, 'Primary', NOW(6), 0
FROM Contacts c
INNER JOIN EmailAddresses e ON e.Email = c.EmailPrimary
WHERE c.EmailPrimary IS NOT NULL AND c.EmailPrimary != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityEmailLinks eel 
    WHERE eel.EntityType = 'Contact' AND eel.EntityId = c.Id AND eel.EmailAddressId = e.Id
  );

-- ============================================
-- PHASE 1C: Migrate Phone Numbers
-- ============================================

-- Primary phones from Customers
INSERT INTO PhoneNumbers (PhoneNumber, PhoneType, CountryCode, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.Phone, 'Work', '+1', 1, 0, NOW(6), 0
FROM Customers c
WHERE c.Phone IS NOT NULL AND c.Phone != ''
  AND NOT EXISTS (SELECT 1 FROM PhoneNumbers p WHERE p.PhoneNumber = c.Phone);

-- Mobile phones from Customers
INSERT INTO PhoneNumbers (PhoneNumber, PhoneType, CountryCode, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.MobilePhone, 'Mobile', '+1', 0, 0, NOW(6), 0
FROM Customers c
WHERE c.MobilePhone IS NOT NULL AND c.MobilePhone != ''
  AND NOT EXISTS (SELECT 1 FROM PhoneNumbers p WHERE p.PhoneNumber = c.MobilePhone);

-- Fax from Customers
INSERT INTO PhoneNumbers (PhoneNumber, PhoneType, CountryCode, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT c.FaxNumber, 'Fax', '+1', 0, 0, NOW(6), 0
FROM Customers c
WHERE c.FaxNumber IS NOT NULL AND c.FaxNumber != ''
  AND NOT EXISTS (SELECT 1 FROM PhoneNumbers p WHERE p.PhoneNumber = c.FaxNumber);

-- From Leads
INSERT INTO PhoneNumbers (PhoneNumber, PhoneType, CountryCode, IsPrimary, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT l.Phone, 'Work', '+1', 1, 0, NOW(6), 0
FROM Leads l
WHERE l.Phone IS NOT NULL AND l.Phone != ''
  AND NOT EXISTS (SELECT 1 FROM PhoneNumbers p WHERE p.PhoneNumber = l.Phone);

-- ============================================
-- PHASE 1D: Create Phone Links
-- ============================================

-- Link Customers to primary phones
INSERT INTO EntityPhoneLinks (EntityType, EntityId, PhoneNumberId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, p.Id, 1, 'Primary', NOW(6), 0
FROM Customers c
INNER JOIN PhoneNumbers p ON p.PhoneNumber = c.Phone
WHERE c.Phone IS NOT NULL AND c.Phone != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityPhoneLinks epl 
    WHERE epl.EntityType = 'Customer' AND epl.EntityId = c.Id AND epl.PhoneNumberId = p.Id
  );

-- Link Customers to mobile phones
INSERT INTO EntityPhoneLinks (EntityType, EntityId, PhoneNumberId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, p.Id, 0, 'Mobile', NOW(6), 0
FROM Customers c
INNER JOIN PhoneNumbers p ON p.PhoneNumber = c.MobilePhone
WHERE c.MobilePhone IS NOT NULL AND c.MobilePhone != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityPhoneLinks epl 
    WHERE epl.EntityType = 'Customer' AND epl.EntityId = c.Id AND epl.PhoneNumberId = p.Id
  );

-- Link Leads to phones
INSERT INTO EntityPhoneLinks (EntityType, EntityId, PhoneNumberId, IsPrimary, Label, CreatedAt, IsDeleted)
SELECT 'Lead', l.Id, p.Id, 1, 'Primary', NOW(6), 0
FROM Leads l
INNER JOIN PhoneNumbers p ON p.PhoneNumber = l.Phone
WHERE l.Phone IS NOT NULL AND l.Phone != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityPhoneLinks epl 
    WHERE epl.EntityType = 'Lead' AND epl.EntityId = l.Id AND epl.PhoneNumberId = p.Id
  );

-- ============================================
-- PHASE 1E: Migrate Addresses
-- ============================================

-- Billing addresses from Customers
INSERT INTO Addresses (AddressLine1, AddressLine2, City, State, PostalCode, Country, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT DISTINCT 
    c.Address, 
    c.Address2, 
    c.City, 
    c.State, 
    c.ZipCode, 
    c.Country,
    'Billing',
    1,
    NOW(6),
    0
FROM Customers c
WHERE c.Address IS NOT NULL AND c.Address != '' AND c.City IS NOT NULL AND c.City != ''
  AND NOT EXISTS (
    SELECT 1 FROM Addresses a 
    WHERE a.AddressLine1 = c.Address 
      AND COALESCE(a.City, '') = COALESCE(c.City, '')
      AND COALESCE(a.State, '') = COALESCE(c.State, '')
      AND COALESCE(a.PostalCode, '') = COALESCE(c.ZipCode, '')
  );

-- Shipping addresses from Customers
INSERT INTO Addresses (AddressLine1, AddressLine2, City, State, PostalCode, Country, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT DISTINCT 
    c.ShippingAddress, 
    c.ShippingAddress2, 
    c.ShippingCity, 
    c.ShippingState, 
    c.ShippingZipCode, 
    c.ShippingCountry,
    'Shipping',
    0,
    NOW(6),
    0
FROM Customers c
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress != '' 
  AND c.ShippingCity IS NOT NULL AND c.ShippingCity != ''
  AND NOT EXISTS (
    SELECT 1 FROM Addresses a 
    WHERE a.AddressLine1 = c.ShippingAddress 
      AND COALESCE(a.City, '') = COALESCE(c.ShippingCity, '')
      AND COALESCE(a.State, '') = COALESCE(c.ShippingState, '')
      AND COALESCE(a.PostalCode, '') = COALESCE(c.ShippingZipCode, '')
  );

-- Addresses from Leads
INSERT INTO Addresses (AddressLine1, AddressLine2, City, State, PostalCode, Country, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT DISTINCT 
    l.Address, 
    l.Address2, 
    l.City, 
    l.State, 
    l.ZipCode, 
    l.Country,
    'Primary',
    1,
    NOW(6),
    0
FROM Leads l
WHERE l.Address IS NOT NULL AND l.Address != '' AND l.City IS NOT NULL AND l.City != ''
  AND NOT EXISTS (
    SELECT 1 FROM Addresses a 
    WHERE a.AddressLine1 = l.Address 
      AND COALESCE(a.City, '') = COALESCE(l.City, '')
      AND COALESCE(a.State, '') = COALESCE(l.State, '')
      AND COALESCE(a.PostalCode, '') = COALESCE(l.ZipCode, '')
  );

-- ============================================
-- PHASE 1F: Create Address Links
-- ============================================

-- Link Customers to billing addresses
INSERT INTO EntityAddressLinks (EntityType, EntityId, AddressId, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, a.Id, 'Billing', 1, NOW(6), 0
FROM Customers c
INNER JOIN Addresses a ON a.AddressLine1 = c.Address 
    AND COALESCE(a.City, '') = COALESCE(c.City, '')
    AND COALESCE(a.State, '') = COALESCE(c.State, '')
    AND COALESCE(a.PostalCode, '') = COALESCE(c.ZipCode, '')
WHERE c.Address IS NOT NULL AND c.Address != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityAddressLinks eal 
    WHERE eal.EntityType = 'Customer' AND eal.EntityId = c.Id AND eal.AddressId = a.Id
  );

-- Link Customers to shipping addresses
INSERT INTO EntityAddressLinks (EntityType, EntityId, AddressId, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, a.Id, 'Shipping', 0, NOW(6), 0
FROM Customers c
INNER JOIN Addresses a ON a.AddressLine1 = c.ShippingAddress 
    AND COALESCE(a.City, '') = COALESCE(c.ShippingCity, '')
    AND COALESCE(a.State, '') = COALESCE(c.ShippingState, '')
    AND COALESCE(a.PostalCode, '') = COALESCE(c.ShippingZipCode, '')
WHERE c.ShippingAddress IS NOT NULL AND c.ShippingAddress != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityAddressLinks eal 
    WHERE eal.EntityType = 'Customer' AND eal.EntityId = c.Id AND eal.AddressId = a.Id
  );

-- Link Leads to addresses
INSERT INTO EntityAddressLinks (EntityType, EntityId, AddressId, AddressType, IsPrimary, CreatedAt, IsDeleted)
SELECT 'Lead', l.Id, a.Id, 'Primary', 1, NOW(6), 0
FROM Leads l
INNER JOIN Addresses a ON a.AddressLine1 = l.Address 
    AND COALESCE(a.City, '') = COALESCE(l.City, '')
    AND COALESCE(a.State, '') = COALESCE(l.State, '')
    AND COALESCE(a.PostalCode, '') = COALESCE(l.ZipCode, '')
WHERE l.Address IS NOT NULL AND l.Address != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntityAddressLinks eal 
    WHERE eal.EntityType = 'Lead' AND eal.EntityId = l.Id AND eal.AddressId = a.Id
  );

-- ============================================
-- PHASE 1G: Migrate Social Media
-- ============================================

-- LinkedIn from Customers
INSERT INTO SocialMediaAccounts (Platform, ProfileUrl, Username, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT 'LinkedIn', c.LinkedInUrl, NULL, 0, NOW(6), 0
FROM Customers c
WHERE c.LinkedInUrl IS NOT NULL AND c.LinkedInUrl != ''
  AND NOT EXISTS (SELECT 1 FROM SocialMediaAccounts s WHERE s.ProfileUrl = c.LinkedInUrl);

-- Twitter from Customers
INSERT INTO SocialMediaAccounts (Platform, ProfileUrl, Username, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT 'Twitter', CONCAT('https://twitter.com/', REPLACE(c.TwitterHandle, '@', '')), REPLACE(c.TwitterHandle, '@', ''), 0, NOW(6), 0
FROM Customers c
WHERE c.TwitterHandle IS NOT NULL AND c.TwitterHandle != ''
  AND NOT EXISTS (SELECT 1 FROM SocialMediaAccounts s WHERE s.Username = REPLACE(c.TwitterHandle, '@', '') AND s.Platform = 'Twitter');

-- Facebook from Customers
INSERT INTO SocialMediaAccounts (Platform, ProfileUrl, Username, IsVerified, CreatedAt, IsDeleted)
SELECT DISTINCT 'Facebook', c.FacebookUrl, NULL, 0, NOW(6), 0
FROM Customers c
WHERE c.FacebookUrl IS NOT NULL AND c.FacebookUrl != ''
  AND NOT EXISTS (SELECT 1 FROM SocialMediaAccounts s WHERE s.ProfileUrl = c.FacebookUrl);

-- ============================================
-- PHASE 1H: Create Social Media Links
-- ============================================

-- Link Customers to LinkedIn
INSERT INTO EntitySocialMediaLinks (EntityType, EntityId, SocialMediaAccountId, IsPrimary, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, s.Id, 1, NOW(6), 0
FROM Customers c
INNER JOIN SocialMediaAccounts s ON s.ProfileUrl = c.LinkedInUrl AND s.Platform = 'LinkedIn'
WHERE c.LinkedInUrl IS NOT NULL AND c.LinkedInUrl != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntitySocialMediaLinks esml 
    WHERE esml.EntityType = 'Customer' AND esml.EntityId = c.Id AND esml.SocialMediaAccountId = s.Id
  );

-- Link Customers to Twitter
INSERT INTO EntitySocialMediaLinks (EntityType, EntityId, SocialMediaAccountId, IsPrimary, CreatedAt, IsDeleted)
SELECT 'Customer', c.Id, s.Id, 0, NOW(6), 0
FROM Customers c
INNER JOIN SocialMediaAccounts s ON s.Username = REPLACE(c.TwitterHandle, '@', '') AND s.Platform = 'Twitter'
WHERE c.TwitterHandle IS NOT NULL AND c.TwitterHandle != ''
  AND NOT EXISTS (
    SELECT 1 FROM EntitySocialMediaLinks esml 
    WHERE esml.EntityType = 'Customer' AND esml.EntityId = c.Id AND esml.SocialMediaAccountId = s.Id
  );
```

### 1.2 Implement Dual-Write in Services

During transition, update services to write to BOTH flat fields AND normalized tables:

```csharp
// In CustomerService.cs - Dual-write pattern
public async Task<Customer> UpdateCustomerAsync(int id, CustomerUpdateDto dto)
{
    var customer = await _context.Customers.FindAsync(id);
    
    // Update flat fields (for backward compatibility)
    customer.Email = dto.Email;
    customer.Phone = dto.Phone;
    // ... other flat fields
    
    // ALSO update normalized tables
    await _contactInfoService.UpdateEmailAsync("Customer", id, dto.Email, isPrimary: true);
    await _contactInfoService.UpdatePhoneAsync("Customer", id, dto.Phone, isPrimary: true);
    await _contactInfoService.UpdateAddressAsync("Customer", id, dto.BillingAddress);
    
    await _context.SaveChangesAsync();
    return customer;
}
```

---

## Phase 2: API & Frontend Updates (Week 2-3)

### 2.1 Add Normalized Contact Endpoints

Create new API endpoints that read from normalized tables:

| Endpoint | Description |
|----------|-------------|
| `GET /api/customers/{id}/emails` | Get all emails for customer |
| `POST /api/customers/{id}/emails` | Add email to customer |
| `DELETE /api/customers/{id}/emails/{emailId}` | Remove email link |
| `GET /api/customers/{id}/phones` | Get all phones for customer |
| `GET /api/customers/{id}/addresses` | Get all addresses for customer |
| `GET /api/customers/{id}/social-media` | Get all social accounts |

### 2.2 Update DTOs

**Before (flat):**
```csharp
public class CustomerDto
{
    public string Email { get; set; }
    public string SecondaryEmail { get; set; }
    public string Phone { get; set; }
    public string MobilePhone { get; set; }
    // ... 20+ more flat fields
}
```

**After (normalized):**
```csharp
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    // Core business fields only
    
    // Linked contact info (from normalized tables)
    public List<EmailDto> Emails { get; set; }
    public List<PhoneDto> Phones { get; set; }
    public List<AddressDto> Addresses { get; set; }
    public List<SocialMediaDto> SocialMedia { get; set; }
    
    // Legacy - Deprecated (marked with [Obsolete])
    [Obsolete("Use Emails collection instead")]
    public string Email => Emails?.FirstOrDefault(e => e.IsPrimary)?.Email;
}
```

### 2.3 Frontend Component Updates

Update React components to use normalized collections:

```typescript
// Before: Flat fields
<TextField label="Email" value={customer.email} />
<TextField label="Secondary Email" value={customer.secondaryEmail} />
<TextField label="Phone" value={customer.phone} />

// After: Dynamic contact info sections
<ContactEmailsSection 
  emails={customer.emails} 
  onAdd={handleAddEmail}
  onRemove={handleRemoveEmail}
/>
<ContactPhonesSection 
  phones={customer.phones}
  onAdd={handleAddPhone}
  onRemove={handleRemovePhone}
/>
<ContactAddressesSection 
  addresses={customer.addresses}
  onAdd={handleAddAddress}
  onRemove={handleRemoveAddress}
/>
```

---

## Phase 3: Deprecation & Cleanup (Week 4-5)

### 3.1 Mark Flat Fields as Deprecated

Add [Obsolete] attributes to entity properties:

```csharp
public class Customer
{
    // Keep core business fields
    public int Id { get; set; }
    public string Name { get; set; }
    public string CompanyName { get; set; }
    
    // DEPRECATED - Contact info (to be removed in v3.0)
    [Obsolete("Use EntityEmailLinks instead. Will be removed in v3.0")]
    public string? Email { get; set; }
    
    [Obsolete("Use EntityEmailLinks instead. Will be removed in v3.0")]
    public string? SecondaryEmail { get; set; }
    
    [Obsolete("Use EntityPhoneLinks instead. Will be removed in v3.0")]
    public string? Phone { get; set; }
    
    // ... mark all flat contact fields
}
```

### 3.2 Remove Dual-Write (After Validation)

Once data integrity is confirmed:
1. Stop writing to flat fields
2. Read ONLY from normalized tables
3. Remove flat field mappings from DTOs

### 3.3 Final Migration: Drop Columns

```sql
-- Migration: 020_drop_deprecated_flat_fields.sql
-- WARNING: Only run after Phase 2 is fully validated!

-- Drop email flat fields
ALTER TABLE Customers DROP COLUMN Email;
ALTER TABLE Customers DROP COLUMN SecondaryEmail;

-- Drop phone flat fields
ALTER TABLE Customers DROP COLUMN Phone;
ALTER TABLE Customers DROP COLUMN MobilePhone;
ALTER TABLE Customers DROP COLUMN FaxNumber;

-- Drop address flat fields
ALTER TABLE Customers DROP COLUMN Address;
ALTER TABLE Customers DROP COLUMN Address2;
ALTER TABLE Customers DROP COLUMN City;
ALTER TABLE Customers DROP COLUMN State;
ALTER TABLE Customers DROP COLUMN ZipCode;
ALTER TABLE Customers DROP COLUMN Country;
ALTER TABLE Customers DROP COLUMN ShippingAddress;
ALTER TABLE Customers DROP COLUMN ShippingAddress2;
ALTER TABLE Customers DROP COLUMN ShippingCity;
ALTER TABLE Customers DROP COLUMN ShippingState;
ALTER TABLE Customers DROP COLUMN ShippingZipCode;
ALTER TABLE Customers DROP COLUMN ShippingCountry;

-- Drop social media flat fields
ALTER TABLE Customers DROP COLUMN LinkedInUrl;
ALTER TABLE Customers DROP COLUMN TwitterHandle;
ALTER TABLE Customers DROP COLUMN FacebookUrl;

-- Repeat for Leads table
ALTER TABLE Leads DROP COLUMN Email;
ALTER TABLE Leads DROP COLUMN SecondaryEmail;
ALTER TABLE Leads DROP COLUMN Phone;
ALTER TABLE Leads DROP COLUMN MobilePhone;
ALTER TABLE Leads DROP COLUMN FaxNumber;
ALTER TABLE Leads DROP COLUMN Address;
ALTER TABLE Leads DROP COLUMN Address2;
ALTER TABLE Leads DROP COLUMN City;
ALTER TABLE Leads DROP COLUMN State;
ALTER TABLE Leads DROP COLUMN ZipCode;
ALTER TABLE Leads DROP COLUMN Country;
ALTER TABLE Leads DROP COLUMN LinkedInUrl;
ALTER TABLE Leads DROP COLUMN TwitterHandle;

-- Repeat for Contacts table
ALTER TABLE Contacts DROP COLUMN EmailPrimary;
ALTER TABLE Contacts DROP COLUMN EmailSecondary;
ALTER TABLE Contacts DROP COLUMN EmailWork;
ALTER TABLE Contacts DROP COLUMN PhonePrimary;
ALTER TABLE Contacts DROP COLUMN PhoneSecondary;
ALTER TABLE Contacts DROP COLUMN PhoneMobile;
ALTER TABLE Contacts DROP COLUMN PhoneWork;
ALTER TABLE Contacts DROP COLUMN PhoneFax;
ALTER TABLE Contacts DROP COLUMN Address;
ALTER TABLE Contacts DROP COLUMN Address2;
ALTER TABLE Contacts DROP COLUMN City;
ALTER TABLE Contacts DROP COLUMN State;
ALTER TABLE Contacts DROP COLUMN ZipCode;
ALTER TABLE Contacts DROP COLUMN Country;
ALTER TABLE Contacts DROP COLUMN MailingAddress;
ALTER TABLE Contacts DROP COLUMN MailingCity;
ALTER TABLE Contacts DROP COLUMN MailingState;
ALTER TABLE Contacts DROP COLUMN MailingCountry;
ALTER TABLE Contacts DROP COLUMN MailingZipCode;
ALTER TABLE Contacts DROP COLUMN LinkedInUrl;
ALTER TABLE Contacts DROP COLUMN TwitterHandle;
ALTER TABLE Contacts DROP COLUMN FacebookUrl;
ALTER TABLE Contacts DROP COLUMN InstagramHandle;
ALTER TABLE Contacts DROP COLUMN Website;
ALTER TABLE Contacts DROP COLUMN BlogUrl;

-- Drop redundant entities/tables
DROP TABLE IF EXISTS ContactDetails;
DROP TABLE IF EXISTS SocialAccounts;
DROP TABLE IF EXISTS ContactInfoLinks;
```

---

## Validation Queries

### Before Migration - Count Flat Data

```sql
-- Count records with flat contact data
SELECT 
    'Customers' as Entity,
    COUNT(*) as Total,
    SUM(CASE WHEN Email IS NOT NULL AND Email != '' THEN 1 ELSE 0 END) as WithEmail,
    SUM(CASE WHEN Phone IS NOT NULL AND Phone != '' THEN 1 ELSE 0 END) as WithPhone,
    SUM(CASE WHEN Address IS NOT NULL AND Address != '' THEN 1 ELSE 0 END) as WithAddress
FROM Customers
UNION ALL
SELECT 
    'Leads',
    COUNT(*),
    SUM(CASE WHEN Email IS NOT NULL AND Email != '' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Phone IS NOT NULL AND Phone != '' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Address IS NOT NULL AND Address != '' THEN 1 ELSE 0 END)
FROM Leads
UNION ALL
SELECT 
    'Contacts',
    COUNT(*),
    SUM(CASE WHEN EmailPrimary IS NOT NULL AND EmailPrimary != '' THEN 1 ELSE 0 END),
    SUM(CASE WHEN PhonePrimary IS NOT NULL AND PhonePrimary != '' THEN 1 ELSE 0 END),
    SUM(CASE WHEN Address IS NOT NULL AND Address != '' THEN 1 ELSE 0 END)
FROM Contacts;
```

### After Migration - Verify Normalized Data

```sql
-- Verify normalized data matches
SELECT 
    'Customers with Email Links' as Check,
    (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Email IS NOT NULL AND c.Email != '') as FlatCount,
    (SELECT COUNT(DISTINCT eel.EntityId) FROM EntityEmailLinks eel WHERE eel.EntityType = 'Customer') as NormalizedCount,
    CASE 
        WHEN (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Email IS NOT NULL AND c.Email != '') =
             (SELECT COUNT(DISTINCT eel.EntityId) FROM EntityEmailLinks eel WHERE eel.EntityType = 'Customer')
        THEN 'PASS' ELSE 'FAIL'
    END as Status
UNION ALL
SELECT 
    'Customers with Phone Links',
    (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Phone IS NOT NULL AND c.Phone != ''),
    (SELECT COUNT(DISTINCT epl.EntityId) FROM EntityPhoneLinks epl WHERE epl.EntityType = 'Customer'),
    CASE 
        WHEN (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Phone IS NOT NULL AND c.Phone != '') =
             (SELECT COUNT(DISTINCT epl.EntityId) FROM EntityPhoneLinks epl WHERE epl.EntityType = 'Customer')
        THEN 'PASS' ELSE 'FAIL'
    END
UNION ALL
SELECT 
    'Customers with Address Links',
    (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Address IS NOT NULL AND c.Address != ''),
    (SELECT COUNT(DISTINCT eal.EntityId) FROM EntityAddressLinks eal WHERE eal.EntityType = 'Customer'),
    CASE 
        WHEN (SELECT COUNT(DISTINCT c.Id) FROM Customers c WHERE c.Address IS NOT NULL AND c.Address != '') =
             (SELECT COUNT(DISTINCT eal.EntityId) FROM EntityAddressLinks eal WHERE eal.EntityType = 'Customer')
        THEN 'PASS' ELSE 'FAIL'
    END;
```

---

## Rollback Plan

If issues are encountered during migration:

### Phase 1 Rollback
```sql
-- Delete migrated links (normalized tables are append-only during dual-write)
DELETE FROM EntityEmailLinks WHERE CreatedAt >= '2026-01-24';
DELETE FROM EntityPhoneLinks WHERE CreatedAt >= '2026-01-24';
DELETE FROM EntityAddressLinks WHERE CreatedAt >= '2026-01-24';
DELETE FROM EntitySocialMediaLinks WHERE CreatedAt >= '2026-01-24';

-- Clean up orphaned normalized records
DELETE FROM EmailAddresses WHERE Id NOT IN (SELECT EmailAddressId FROM EntityEmailLinks);
DELETE FROM PhoneNumbers WHERE Id NOT IN (SELECT PhoneNumberId FROM EntityPhoneLinks);
DELETE FROM Addresses WHERE Id NOT IN (SELECT AddressId FROM EntityAddressLinks);
DELETE FROM SocialMediaAccounts WHERE Id NOT IN (SELECT SocialMediaAccountId FROM EntitySocialMediaLinks);
```

### Phase 3 Rollback
- Restore columns from backup before running DROP COLUMN
- Re-populate flat fields from normalized tables

---

## Files to Modify

### Backend (C#)

| File | Changes |
|------|---------|
| `Customer.cs` | Add [Obsolete] attributes, remove flat properties (Phase 3) |
| `Lead.cs` | Add [Obsolete] attributes, remove flat properties (Phase 3) |
| `Contact.cs` | Refactor to Entity, add [Obsolete], remove flat (Phase 3) |
| `Account.cs` | Add [Obsolete] to billing contact fields |
| `CustomerService.cs` | Add dual-write, then remove flat field writes |
| `LeadService.cs` | Add dual-write, then remove flat field writes |
| `ContactService.cs` | Add dual-write, then remove flat field writes |
| `CustomerDto.cs` | Add Emails/Phones/Addresses collections |
| `ContactInfoController.cs` | New controller for normalized CRUD |

### Frontend (React/TypeScript)

| File | Changes |
|------|---------|
| `CustomerForm.tsx` | Replace flat fields with ContactInfo components |
| `LeadForm.tsx` | Replace flat fields with ContactInfo components |
| `ContactForm.tsx` | Replace flat fields with ContactInfo components |
| `ContactEmailsSection.tsx` | New component for managing emails |
| `ContactPhonesSection.tsx` | New component for managing phones |
| `ContactAddressesSection.tsx` | New component for managing addresses |
| `customerService.ts` | Add email/phone/address API calls |

### Database Migrations

| File | Phase |
|------|-------|
| `015_migrate_contact_info_to_normalized.sql` | Phase 1 |
| `016_add_dual_write_triggers.sql` | Phase 1 (optional) |
| `020_drop_deprecated_flat_fields.sql` | Phase 3 |
| `021_drop_redundant_tables.sql` | Phase 3 |

---

## Timeline Summary

| Week | Phase | Activities |
|------|-------|------------|
| 1 | 1A | Run migration scripts, verify data integrity |
| 1-2 | 1B | Implement dual-write in services |
| 2 | 2A | Add normalized API endpoints |
| 2-3 | 2B | Update frontend components |
| 3 | 2C | Testing and validation |
| 4 | 3A | Mark fields deprecated, stop flat writes |
| 5 | 3B | Drop columns (after validation period) |

---

## Success Criteria

- [ ] All flat contact data migrated to normalized tables (100% coverage)
- [ ] API returns contact info from normalized tables only
- [ ] Frontend uses new ContactInfo components
- [ ] No data loss during migration
- [ ] Performance metrics maintained or improved
- [ ] All automated tests passing
- [ ] Zero production incidents during transition

---

## Appendix: Entity Field Removal Summary

### Customer Entity - Fields to Remove (25 fields)
- `Email`, `SecondaryEmail`
- `Phone`, `MobilePhone`, `FaxNumber`
- `Address`, `Address2`, `City`, `State`, `ZipCode`, `Country`
- `ShippingAddress`, `ShippingAddress2`, `ShippingCity`, `ShippingState`, `ShippingZipCode`, `ShippingCountry`
- `LinkedInUrl`, `TwitterHandle`, `FacebookUrl`
- `BillingAddress`, `BillingCity`, `BillingState`, `BillingZip`, `BillingCountry` (if exists)

### Lead Entity - Fields to Remove (15 fields)
- `Email`, `SecondaryEmail`
- `Phone`, `MobilePhone`, `FaxNumber`
- `Address`, `Address2`, `City`, `State`, `ZipCode`, `Country`, `Region`
- `LinkedInUrl`, `TwitterHandle`

### Contact Model - Fields to Remove (27 fields)
- `EmailPrimary`, `EmailSecondary`, `EmailWork`
- `PhonePrimary`, `PhoneSecondary`, `PhoneMobile`, `PhoneWork`, `PhoneFax`
- `Address`, `Address2`, `City`, `State`, `Country`, `ZipCode`
- `MailingAddress`, `MailingCity`, `MailingState`, `MailingCountry`, `MailingZipCode`
- `LinkedInUrl`, `TwitterHandle`, `FacebookUrl`, `InstagramHandle`, `Website`, `BlogUrl`

### Account Entity - Fields to Remove (8 fields)
- `BillingAddress`, `BillingCity`, `BillingState`, `BillingZip`, `BillingCountry`
- `BillingContactName`, `BillingContactEmail`, `BillingContactPhone`

**Total: ~75 flat fields to be removed**
