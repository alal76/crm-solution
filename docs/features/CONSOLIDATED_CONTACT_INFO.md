# Consolidated Contact Information Implementation

## Overview

This implementation consolidates contact information (addresses, phone numbers, emails, and social media accounts) into shared master tables with polymorphic junction tables. This allows a single address, phone, email, or social media account to be linked to multiple entities (Customers, Contacts, Leads, Accounts), eliminating data duplication.

## Architecture

### Database Schema

#### Master Tables
- **Addresses** - Enhanced with additional fields (Line3, CountryCode, GeocodeAccuracy, etc.)
- **PhoneNumbers** - New table for phone number storage
- **EmailAddresses** - New table for email address storage
- **SocialMediaAccounts** - New table for social media account storage

#### Junction Tables
- **EntityAddressLinks** - Links addresses to entities with type and primary designation
- **EntityPhoneLinks** - Links phones to entities with type, primary, and DoNotCall flags
- **EntityEmailLinks** - Links emails to entities with marketing preferences
- **EntitySocialMediaLinks** - Links social media to entities with preferred contact flags

### Entity Types Supported
- Customer
- Contact
- Lead
- Account

## Files Created/Modified

### Database Migrations
- `database/schema/007_consolidated_contact_info.sql` - SQL migration script

### Backend (C# .NET)

#### Entities (`CRM.Backend/src/CRM.Core/Entities/`)
- `Address.cs` - Enhanced with new fields
- `PhoneNumber.cs` - New entity
- `EmailAddress.cs` - New entity
- `SocialMediaAccount.cs` - New entity
- `EntityAddressLink.cs` - New junction entity
- `EntityPhoneLink.cs` - New junction entity
- `EntityEmailLink.cs` - New junction entity
- `EntitySocialMediaLink.cs` - New junction entity

#### Enums (in entities)
- `EntityType` - Customer, Contact, Lead, Account
- `AddressType` - Primary, Billing, Shipping, Mailing, etc.
- `PhoneType` - Mobile, Home, Office, Direct, Fax, etc.
- `EmailType` - Personal, Work, Invoicing, Support, etc.
- `SocialMediaPlatform` - LinkedIn, Twitter, Facebook, etc.
- `SocialMediaAccountType` - Personal, Business, Official, Support
- `EngagementLevel` - VeryLow, Low, Medium, High, VeryHigh

#### DTOs (`CRM.Backend/src/CRM.Core/Dtos/`)
- `ContactInfoDto.cs` - Comprehensive DTOs for all contact info types

#### Interfaces (`CRM.Backend/src/CRM.Core/Interfaces/`)
- `IContactInfoService.cs` - Service interface

#### Services (`CRM.Backend/src/CRM.Infrastructure/Services/`)
- `ContactInfoService.cs` - Full implementation with CRUD, linking, sharing

#### Controllers (`CRM.Backend/src/CRM.Api/Controllers/`)
- `ContactInfoController.cs` - REST API endpoints

#### Infrastructure (`CRM.Backend/src/CRM.Infrastructure/`)
- `Data/CrmDbContext.cs` - Updated with DbSets and entity configurations
- `Migrations/20260124T100000_AddConsolidatedContactInfo.cs` - EF Core migration

### Frontend (React TypeScript)

#### Services (`CRM.Frontend/src/services/`)
- `contactInfoService.ts` - TypeScript types and API client

#### Components (`CRM.Frontend/src/components/ContactInfo/`)
- `index.ts` - Exports all components
- `AddressManager.tsx` - Address CRUD component
- `PhoneManager.tsx` - Phone CRUD component
- `EmailManager.tsx` - Email CRUD component
- `SocialMediaManager.tsx` - Social media CRUD component
- `ContactInfoPanel.tsx` - Combined panel with tab/grid/stacked layouts
- `ShareContactInfoModal.tsx` - Modal for sharing contact info between entities

## API Endpoints

### Aggregate Operations
- `GET /api/contact-info/{entityType}/{entityId}` - Get all contact info for an entity
- `POST /api/contact-info/share` - Share contact info between entities

### Address Operations
- `GET /api/contact-info/{entityType}/{entityId}/addresses` - Get addresses
- `GET /api/contact-info/addresses/{addressId}` - Get specific address
- `GET /api/contact-info/addresses/{addressId}/shared-by` - Get entities sharing address
- `POST /api/contact-info/addresses` - Create standalone address
- `POST /api/contact-info/addresses/link` - Link address to entity
- `PUT /api/contact-info/addresses/{addressId}` - Update address
- `DELETE /api/contact-info/addresses/link/{linkId}` - Unlink address
- `DELETE /api/contact-info/addresses/{addressId}` - Delete address
- `POST /api/contact-info/{entityType}/{entityId}/addresses/{addressId}/set-primary` - Set primary

### Phone Operations
- `GET /api/contact-info/{entityType}/{entityId}/phones` - Get phones
- `GET /api/contact-info/phones/{phoneId}` - Get specific phone
- `GET /api/contact-info/phones/{phoneId}/shared-by` - Get entities sharing phone
- `POST /api/contact-info/phones` - Create standalone phone
- `POST /api/contact-info/phones/link` - Link phone to entity
- `PUT /api/contact-info/phones/{phoneId}` - Update phone
- `DELETE /api/contact-info/phones/link/{linkId}` - Unlink phone
- `DELETE /api/contact-info/phones/{phoneId}` - Delete phone
- `POST /api/contact-info/{entityType}/{entityId}/phones/{phoneId}/set-primary` - Set primary

### Email Operations
- `GET /api/contact-info/{entityType}/{entityId}/emails` - Get emails
- `GET /api/contact-info/emails/{emailId}` - Get specific email
- `GET /api/contact-info/emails/find?email=...` - Find by email address
- `GET /api/contact-info/emails/{emailId}/shared-by` - Get entities sharing email
- `POST /api/contact-info/emails` - Create standalone email
- `POST /api/contact-info/emails/link` - Link email to entity
- `PUT /api/contact-info/emails/{emailId}` - Update email
- `PUT /api/contact-info/emails/link/{linkId}/preferences` - Update email preferences
- `DELETE /api/contact-info/emails/link/{linkId}` - Unlink email
- `DELETE /api/contact-info/emails/{emailId}` - Delete email
- `POST /api/contact-info/{entityType}/{entityId}/emails/{emailId}/set-primary` - Set primary

### Social Media Operations
- `GET /api/contact-info/{entityType}/{entityId}/social-media` - Get social accounts
- `GET /api/contact-info/social-media/{socialMediaId}` - Get specific account
- `POST /api/contact-info/social-media` - Create standalone account
- `POST /api/contact-info/social-media/link` - Link account to entity
- `PUT /api/contact-info/social-media/{socialMediaId}` - Update account
- `DELETE /api/contact-info/social-media/link/{linkId}` - Unlink account
- `DELETE /api/contact-info/social-media/{socialMediaId}` - Delete account
- `POST /api/contact-info/{entityType}/{entityId}/social-media/{socialMediaId}/set-primary` - Set primary

## Usage Examples

### Frontend - Using ContactInfoPanel

```tsx
import { ContactInfoPanel } from './components/ContactInfo';

// In a Customer detail page:
<ContactInfoPanel
  entityType="Customer"
  entityId={customerId}
  layout="tabs"  // or "grid" or "stacked"
  readOnly={false}
  onContactInfoChange={() => refreshCustomerData()}
/>
```

### Frontend - Using Individual Managers

```tsx
import { AddressManager, PhoneManager } from './components/ContactInfo';

// Just addresses:
<AddressManager
  entityType="Contact"
  entityId={contactId}
  onAddressChange={handleUpdate}
/>

// Just phones:
<PhoneManager
  entityType="Lead"
  entityId={leadId}
  readOnly={true}
/>
```

### Backend - Using ContactInfoService

```csharp
// Get all contact info for a customer
var contactInfo = await _contactInfoService.GetEntityContactInfoAsync(EntityType.Customer, customerId);

// Share a customer's address with their contact
await _contactInfoService.ShareContactInfoAsync(new ShareContactInfoDto
{
    TargetEntityType = "Contact",
    TargetEntityId = contactId,
    AddressIds = new[] { addressId },
    SetAsPrimary = true
});

// Create and link a new phone
var linked = await _contactInfoService.LinkPhoneNumberAsync(new LinkPhoneDto
{
    NewPhone = new CreatePhoneNumberDto
    {
        CountryCode = "+1",
        AreaCode = "555",
        Number = "123-4567",
        CanSMS = true
    },
    EntityType = "Customer",
    EntityId = customerId,
    PhoneType = "Office",
    IsPrimary = true
});
```

## Migration Steps

1. **Run SQL Migration**
   ```bash
   # Execute database/schema/007_consolidated_contact_info.sql against your database
   ```

2. **Build Backend**
   ```bash
   cd CRM.Backend
   dotnet build
   ```

3. **Apply EF Core Migration** (optional, if using code-first)
   ```bash
   dotnet ef database update
   ```

4. **Build Frontend**
   ```bash
   cd CRM.Frontend
   npm install
   npm run build
   ```

## Key Features

### Sharing Contact Information
When a contact shares an address with their employer (customer), you only store the address once. Both the customer and contact link to the same address record, with their own metadata (primary flag, address type, etc.).

### Multi-Entity Support
The same infrastructure works for Customers, Contacts, Leads, and Accounts. Add support for new entity types by extending the `EntityType` enum.

### Rich Metadata
- **Addresses**: Geocoding, verification, delivery instructions, site contacts
- **Phones**: SMS/WhatsApp capability, best time to call, do-not-call flags
- **Emails**: Bounce tracking, engagement scores, marketing preferences
- **Social Media**: Platform, follower counts, engagement levels, verification status

### Soft Deletes
All records use soft delete (`IsDeleted` flag) to preserve data integrity and audit trails.

### Type-Specific Links
Junction tables store entity-specific metadata:
- Address links: address type (billing, shipping, etc.), validity periods
- Phone links: phone type, do-not-call flag
- Email links: email type, marketing opt-in, transactional-only flags
- Social links: preferred for contact flag

## Benefits

1. **Data Deduplication** - Single source of truth for shared contact info
2. **Consistency** - Updates propagate to all linked entities automatically
3. **Flexibility** - Each entity can have its own metadata for shared items
4. **Scalability** - Clean polymorphic design supports new entity types
5. **Rich Features** - Marketing preferences, verification, engagement tracking
